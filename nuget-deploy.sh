dotnet pack ServForOracle.NetCore --include-source -o $TRAVIS_BUILD_DIR
dotnet nuget push ServForOracle.NetCore.*.nupkg --source https://api.nuget.org/v3/index.json --api-key $NUGET_TOKEN