@echo off

dotnet test .\ServForOracle.NetCore.UnitTests
coverlet .\ServForOracle.NetCore.UnitTests\bin\Debug\netcoreapp2.1\ServForOracle.NetCore.UnitTests.dll --target "dotnet" --targetargs "test .\ServForOracle.NetCore.UnitTests --no-build" --include "[ServForOracle.NetCore]ServForOracle.NetCore*"