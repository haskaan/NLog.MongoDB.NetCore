# restore and builds all projects as release.
# creates NuGet package at \artifacts
dotnet --version

dotnet restore
dotnet pack .\src\NLog.MongoDB.NetCore\ --configuration release   -o artifacts

# remove this when we get clean builds
Get-ChildItem artifacts\*.nupkg | % { Push-AppveyorArtifact $_.FullName -FileName $_.Name }
