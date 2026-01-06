#!/bin/sh

SERVER="recipes"
EXECUTABLE="Recepten"

# First we check if we have PID 1
echo "Our PID is $$ (should be 1)"

# We wait until the old container signals us that it has persisted
# the data. It does so by deleting the up-and-running.flag file in
# the volume where the data is persisted after it is done.
echo 'Waiting for running container to stop'

# Now we check  if there is a service up and running.
# If so we will signal it to stop. The web-server has
# a mechanism to stop using the web-interface.

if [ -f /data/up-and-running.flag ]; then
  echo 'Old server is running, sending a stop signal.'
  # Get a challenge
  challenge=$(/${SERVER}/httpget https://www.zestien3.nl/${SERVER}/admin/gettoken?purpose=shutdown)
  # Get the key
  key=$(grep TokenKey /${SERVER}/appsettings.json | sed "s/^.*:\s*//" | sed 's/,//' | sed 's/\r//')
  # Calculate the response from the challenge and the key
  response=$(($challenge^$key))
  # Send the response to the server to stop it.
  /${SERVER}/httpget "https://www.zestien3.nl/${SERVER}/admin/executetoken?purpose=shutdown&response=$response"
fi

# We check every second if the file is already removed. We do this
# for max 30 seconds. Since the internal SECONDS counter is not
# always installed in a container, we build something ourselves.
# Currently this does not work. It seems that once a file is present,
# we will not see it disappear if it is removed from the bucket.
# That might have to do with the GCSFuse interface that is used.
# If this changes in the future, we will be ready for it.
count=0
until [ $count -gt 30 ] || [ ! -f /data/up-and-running.flag ]; do
  sleep 1
  count=$((count + 1))
done

# The file has been removed or we have a time-out.
echo 'Done waiting'

# The file signaling us that there is still another container
# running is still there, so we can not startup the server
if [ -f /data/up-and-running.flag ]; then
  echo 'Old server still running'
  exit 1
fi

# We declare this variable, because I believe that it will create
# problems in the next function if it is not yet known there.
SRV_PID=0

# This variable allows us to detect if the server crashed.
SRV_EXIT=1

# This is the SIGTERM handler
# Everything that should be done before the container goes down
# should be done in this function. If we return from the handler,
# the kernel will very soon kill the whole process. So anything
# done in this script from then one is not guaranteed to execute.
# We have 10 seconds to execute all of this, which should be OK.
persistDataAfterSigTerm() {

  SRV_EXIT=0

  # Simple countdown counter which puts it's counter value in
  # the log every 250 msec. Just so you know if you're OK
  # timing wise during the shutdown.
  /countdown.sh &

  # We only wait for the server to stop if it has started.
  if [ $SRV_PID -ne 0 ]; then
    # We send the SIGTERM signal to the server.
    echo 'SIGTERM caught! Passing it on to the server'
    kill -15 $SRV_PID

    # We need to be sure that the server stopped and the files we
    # need to persist are no longer in use.
    echo 'Waiting for the server to stop'
    wait $SRV_PID
  fi

  # Here we do the actual persisting.
  echo 'Server stopped. Copying the local data to persistent storage'
  \cp -f /localdata/${SERVER}.db /data/

  # The next container to start will look for this file to be removed.
  # So we cannot remove it before we have persisted the data.
  echo 'Data copied, removing up-and-running.flag'
  rm /data/up-and-running.flag

  # We are done. This will make sure we exit the process before
  # the Kernel kills it because we return from the SIGTERM handler.
  # If the server stopped by itself, this is not called as a
  # SIGTERM handler. So we don't want to exit the script yet.
  if [ $SRV_PID -ne 0 ]; then
    exit 0
  fi
}

# Now we create the up-and-running.flag file to signal that another
# container has started.
echo 'Creating up-and-running.flag'
touch /data/up-and-running.flag

# We copy the database and any other stuff from the persistent storage.
echo 'Copying the data from persistent storage to local drive'
\cp /data/${SERVER}.db /localdata/

SRV_PID=0
# Here we already can install the SIGTERM handler.
echo 'Installing SIGTERM handler.'
trap persistDataAfterSigTerm TERM

# We start the server...
echo 'Starting the server'
cd /${SERVER}
/${SERVER}/${EXECUTABLE} "${@}" &

# ...and store its Process ID.
SRV_PID=$!

echo "Received $SRV_PID as the PID of the server."

echo 'Going to sleep'
wait $SRV_PID
wait $SRV_PID
wait $SRV_PID

# If the server has exited, SRV_EXIT will still be 1 as
# it is set to 0 before we pass the SIGTERM on to our server.
# So if it is 1, we need to call persistDataAfterSigTerm here.
if [ $SRV_EXIT -eq 1 ]; then
  echo "The server has exited"

  # Disable the previously installed trap
  trap - TERM

  # Since the server already stopped, we only need to persist
  # the database. We don't need to wait for the server to stop.
  SRV_PID=0
  persistDataAfterSigTerm

  # Now we have no server active. The new server needs some time to startup.
  # If we exit now, Google Cloud thinks it needs to start a new instance.
  # So we need to wait until the new server is up and running.
  # 10 seconds should be enough for the new server to spin up.
  # If we wait longer, the container will be killed.
  sleep 10
fi