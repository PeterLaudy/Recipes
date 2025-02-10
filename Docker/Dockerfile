# Build this image using
# docker build -t  europe-west4-docker.pkg.dev/zestien3-web-site/recipes/recipes .
# Then push the image
# docker push europe-west4-docker.pkg.dev/zestien3-web-site/recipes/recipes
# There must have been build before creating the image.

# Build docker image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /recipes
COPY ./BackEnd/bin/Release/net8.0/linux-x64/publish ./

WORKDIR /
COPY ./Docker/GoogleCloud/start.sh ./
COPY ./Docker/GoogleCloud/countdown.sh ./

VOLUME /data

RUN mkdir /localdata/
RUN chmod go+w /localdata

EXPOSE 8080

ENTRYPOINT ["/start.sh"]
