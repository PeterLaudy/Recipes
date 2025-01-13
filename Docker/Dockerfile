# Build this image using
# docker build -t  europe-west4-docker.pkg.dev/zestien3-web-site/recipes/recipes .
# Then push the image
# docker push europe-west4-docker.pkg.dev/zestien3-web-site/recipes/recipes
# The must have been build before creating the image.

# Build docker image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /recipe
COPY ../../BackEnd/bin/Release/net8.0/linux-x64/publish ./
EXPOSE 8080

ENTRYPOINT ["/recipe/Recepten"]
