if test -d ./bin; then
  rm -Rf ./bin
fi
if test -d ./obj; then
  rm -Rf ./obj
fi
if test -f ./appsettings.json; then
  rm -f ./appsettings.json
fi

# if [ ] will build for Linux x64, if [ 1 ] builds for Linux arm64
if [ ]; then
  dotnet publish -c Release -r linux-arm64 --self-contained=false "-p:PublishSingleFile=true" ./Recepten.sln -v n
  cp ./bin/Release/net8.0/linux-arm64/publish/appsettings.Release.json ./bin/Release/net8.0/linux-arm64/publish/appsettings.json
  rm ./bin/Release/net8.0/linux-arm64/publish/appsettings.*.json
  echo "Linux arm64"
else
  dotnet publish -c Release -r linux-x64 --self-contained=false "-p:PublishSingleFile=true" ./Recepten.sln -v n
  cp ./bin/Release/net8.0/linux-x64/publish/appsettings.Release.json ./bin/Release/net8.0/linux-x64/publish/appsettings.json
  rm ./bin/Release/net8.0/linux-x64/publish/appsettings.*.json
  echo "Linux x64"
fi