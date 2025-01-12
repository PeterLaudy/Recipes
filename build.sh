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
  if [ "$?" == "0" ]; then
    rm ./BackEnd/bin/Release/net8.0/linux-arm64/publish/appsettings.*.json
  else
    echo "Build for Linux x64 failed"
    exit 1
  fi
  echo "Linux arm64"
else
  dotnet publish -c Release -r linux-x64 --self-contained=false "-p:PublishSingleFile=true" ./Recepten.sln -v n
  if [ "$?" == "0" ]; then
    cp ./BackEnd/bin/Release/net8.0/linux-x64/publish/appsettings.Release.json ./BackEnd/bin/Release/net8.0/linux-x64/publish/appsettings.json
    rm ./BackEnd/bin/Release/net8.0/linux-x64/publish/appsettings.*.json
  else
    echo "Build for Linux x64 failed"
    exit 1
  fi
  echo "Linux x64"
  exit 0
fi
