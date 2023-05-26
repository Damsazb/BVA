set lib=BvCore
set version=1.1.2
set source=c:/dev/packages

dotnet build -c Release
dotnet pack -c Release -o nupkgs
nuget.exe delete %lib% %version% -Source %source% -NonInteractive
nuget.exe add ./nupkgs/%lib%.%version%.nupkg -source %source%
