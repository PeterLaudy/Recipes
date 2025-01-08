# Build this image using
# docker build -t  europe-west4-docker.pkg.dev/zestien3-web-site/recipes/recipes .
# Then push the image
# docker push europe-west4-docker.pkg.dev/zestien3-web-site/recipes/recipes

# Build our app. Does not work, because we do not have npm installed in the build-env.
#FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
#WORKDIR /app
#COPY . ./
#RUN ./build.sh

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /recipe
COPY ./bin/Release/net8.0/linux-x64/publish ./
EXPOSE 8080

ENTRYPOINT ["/recipe/Recepten"]
