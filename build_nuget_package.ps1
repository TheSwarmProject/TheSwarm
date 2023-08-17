rm -r bin
rm -r obj

dotnet build
nuget pack TheSwarm.nuspec