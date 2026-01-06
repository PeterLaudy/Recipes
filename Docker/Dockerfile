# Build this image using
# docker build -t  europe-west4-docker.pkg.dev/zestien3-web-site/recipes/recipes .
# Then push the image
# docker push europe-west4-docker.pkg.dev/zestien3-web-site/recipes/recipes
# There must have been build before creating the image.

# Build docker image

# Get the docker image containing the required dotnet core version.
FROM mcr.microsoft.com/dotnet/aspnet:8.0

# Install our project into the image.
WORKDIR /recipes
COPY ./BackEnd/bin/Release/net8.0/linux-x64/publish ./

# Install the required shell scripts for startup and shutdown.
WORKDIR /
COPY ./Docker/GoogleCloud/start.sh ./
COPY ./Docker/GoogleCloud/countdown.sh ./

# Create the folders and mountpoints to persist the SQLite database.
VOLUME /data

RUN mkdir /localdata/
RUN chmod go+w /localdata

# Expose our HTTP port to the world.
EXPOSE 8080

# Set the entrypoint for the image.
ENTRYPOINT ["/start.sh"]
