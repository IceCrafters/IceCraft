#!/usr/bin/env pwsh

Remove-Item -Path "TestResults/*" -Recurse
dotnet-coverage collect "dotnet test src/IceCraft.sln --no-build" -f xml -o "TestResults/coverage.xml"
