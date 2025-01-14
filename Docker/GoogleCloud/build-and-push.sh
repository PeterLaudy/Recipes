cd ../..
./build.sh
if [ "$?" == "0" ]; then
  cd ./Docker/GoogleCloud
  ./push.sh
fi
