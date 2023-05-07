set /P version=Enter version: 
cd %~dp0\helpers\bin\Debug && nuget.exe push -Source "https://pkgs.dev.azure.com/acsint/_packaging/generic_api/nuget/v3/index.json" -ApiKey key "Acs.GenericApi.Helpers.%version%.nupkg"

cmd \k