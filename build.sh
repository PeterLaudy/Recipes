if test -d ./BackEnd/bin; then
  rm -Rf ./BackEnd/bin
fi
if test -d ./BackEnd/obj; then
  rm -Rf ./BackEnd/obj
fi

# if [ ] will build for Linux x64, if [ 1 ] builds for Linux arm64
if [ ]; then
  dotnet publish -c Release -r linux-arm64 --self-contained=false "-p:PublishSingleFile=true" ./Recepten.sln -v n
  if [ "$?" != "0" ]; then
    echo "Build for Linux x64 failed"
    exit 1
  fi
  echo "Linux arm64"
else
  dotnet publish -c Release -r linux-x64 --self-contained=false "-p:PublishSingleFile=true" ./Recepten.sln -v n
  if [ "$?" != "0" ]; then
    echo "Build for Linux x64 failed"
    exit 1
  fi
  echo "Linux x64"
  exit 0
fi
