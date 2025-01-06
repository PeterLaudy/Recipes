if (Test-Path .\bin) { Remove-Item .\bin -Recurse }
if (Test-Path .\obj) { Remove-Item .\obj -Recurse }
if (Test-Path .\appsettings.json) { Remove-Item .\appsettings.json }

<# If this line starts with #, the application will be build for Linux arm64. If it starts with <# it's for Linux x64
dotnet publish -c Release -r linux-arm64 --self-contained=false "-p:PublishSingleFile=true" .\Recepten.sln -v n
Copy-Item .\bin\Release\net8.0\linux-arm64\publish\appsettings.Release.json .\bin\Release\net8.0\linux-arm64\publish\appsettings.json
Remove-Item .\bin\Release\net8.0\linux-arm64\publish\appsettings.*.json
Remove-Item .\bin\Release\net8.0\linux-arm64\publish\Dockerfile
Write-Output "Linux arm64"
<#>
dotnet publish -c Release -r linux-x64 --self-contained=false "-p:PublishSingleFile=true" .\Recepten.sln -v n
Copy-Item .\bin\Release\net8.0\linux-x64\publish\appsettings.Release.json .\bin\Release\net8.0\linux-x64\publish\appsettings.json
Remove-Item .\bin\Release\net8.0\linux-x64\publish\appsettings.*.json
Remove-Item .\bin\Release\net8.0\linux-x64\publish\Dockerfile
Write-Output "Linux x64"
<##>