#!/bin/sh
echo "Changing to /src directory..."
cd src
echo "Executing MSBuild DLL begin command..."
dotnet ../tools/sonar/SonarScanner.MSBuild.dll begin /o:"marcin-golebiowski" /k:"SpiceSharpParser" /d:sonar.cs.vstest.reportsPaths="/home/travis/build/marcin-golebiowski/SpiceSharpParser/src/SpiceSharpParser.IntegrationTests/TestResults/IntegrationTests.trx" /d:sonar.host.url="https://sonarcloud.io" /d:sonar.verbose=true /d:sonar.login=${SONAR_TOKEN}
echo "Running build..."
dotnet build SpiceSharpParser.Tests/SpiceSharpParser.Tests.csproj
dotnet build SpiceSharpParser.IntegrationTests/SpiceSharpParser.IntegrationTests.csproj
echo "Running tests..."
dotnet test SpiceSharpParser.Tests/SpiceSharpParser.Tests.csproj --logger "trx;LogFileName=Tests.trx"
dotnet test SpiceSharpParser.IntegrationTests/SpiceSharpParser.IntegrationTests.csproj --logger "trx;LogFileName=IntegrationTests.trx"
echo "Executing MSBuild DLL end command..."
dotnet ../tools/sonar/SonarScanner.MSBuild.dll end /d:sonar.login=${SONAR_TOKEN}

