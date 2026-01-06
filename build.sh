PROJECT="Recepten"
PLATFORM="linux-x64"

if test -d ./BackEnd/bin; then
  rm -Rf ./BackEnd/bin
fi
if test -d ./BackEnd/obj; then
  rm -Rf ./BackEnd/obj
fi
if test -d ./httpget/bin; then
  rm -Rf ./httpget/bin
fi
if test -d ./httpget/obj; then
  rm -Rf ./httpget/obj
fi


dotnet publish -c Release -r ${PLATFORM} --self-contained=false "-p:PublishSingleFile=true" "-p:UseAppHost=true" ./BackEnd/${PROJECT}.csproj -v n
if [ "$?" != "0" ]; then
  echo "Build for ${PLATFORM} failed"
  exit 10
fi
dotnet publish -c Release -r ${PLATFORM} --self-contained=false "-p:PublishSingleFile=true" "-p:UseAppHost=true" ./httpget/httpget.csproj -v n
if [ "$?" != "0" ]; then
  echo "Build for ${PLATFORM} failed"
  exit 1
fi
rm -f ./BackEnd/bin/Release/net10.0/${PLATFORM}/publish/wwwroot/*.map
rm -f ./BackEnd/bin/Release/net10.0/${PLATFORM}/publish/wwwroot/*.map.br
rm -f ./BackEnd/bin/Release/net10.0/${PLATFORM}/publish/wwwroot/*.map.gz
rm -Rf ./BackEnd/bin/Release/net10.0/${PLATFORM}/publish/BuildHost-*
chmod u+x ./BackEnd/bin/Release/net10.0/${PLATFORM}/publish/${PROJECT}
chmod u+x ./BackEnd/bin/Release/net10.0/${PLATFORM}/publish/libe_sqlite3.so
mv ./httpget/bin/Release/net10.0/${PLATFORM}/publish/httpget ./BackEnd/bin/Release/net10.0/${PLATFORM}/publish/
chmod u+x ./BackEnd/bin/Release/net10.0/${PLATFORM}/publish/httpget
echo "Linux x64"
exit 0
