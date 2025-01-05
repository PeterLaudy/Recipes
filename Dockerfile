# Build this image using
# docker build -t  europe-west4-docker.pkg.dev/zestien3-web-site/recipes/recipes .
# Then push the image
# docker push europe-west4-docker.pkg.dev/zestien3-web-site/recipes/recipes

# Build our app.
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /app
COPY . ./
RUN ./build.sh

# Build runtime image (and add SQLite to it)
FROM mcr.microsoft.com/dotnet/aspnet:8.0
#RUN apt-get update && apt-get install -y sqlite3
WORKDIR /recipe
COPY --from=build-env /app/bin/Release/net8.0/linux-x64/publish/ ./
EXPOSE 80

#ENTRYPOINT ["/recipe/Recepten"]
ENTRYPOINT ["ls", "-als", "/recipe/"]