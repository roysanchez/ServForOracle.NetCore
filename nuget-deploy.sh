dotnet pack ServForOracle.NetCore --include-source --include-symbols --no-build -o $TRAVIS_BUILD_DIR
nuget push ServForOracle.NetCore.nupkg -source https://api.nuget.org/v3/index.json