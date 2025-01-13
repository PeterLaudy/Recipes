if (Test-Path .\BackEnd\bin) { Remove-Item .\BackEnd\bin -Recurse }
if (Test-Path .\BackEnd\obj) { Remove-Item .\BackEnd\obj -Recurse }

<# If this line starts with #, the application will be build for Linux arm64. If it starts with <# it's for Linux x64
dotnet publish -c Release -r linux-arm64 --self-contained=false "-p:PublishSingleFile=true" .\Recepten.sln -v n
Write-Output "Linux arm64"
<#>
dotnet publish -c Release -r linux-x64 --self-contained=false "-p:PublishSingleFile=true" .\Recepten.sln -v n
Write-Output "Linux x64"
<##>