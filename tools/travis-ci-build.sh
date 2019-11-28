#!/bin/sh
echo "Changing to /src directory...."
cd src
echo "Executing MSBuild DLL begin command..."
dotnet ../tools/sonar/SonarScanner.MSBuild.dll begin /o:"marcin-golebiowski" /k:"SpiceSharpParser" /d:sonar.host.url="https://sonarcloud.io" /d:sonar.verbose=true /d:sonar.login=${SONAR_TOKEN} /d:sonar.language="cs" /d:sonar.exclusions="**/bin/**/*,**/obj/**/*" /d:sonar.cs.opencover.reportsPaths="lcov.opencover.xml"
echo "Running build..."
dotnet build
echo "Running tests..."
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=\"opencover,lcov\" /p:CoverletOutput=../lcov
echo "Executing MSBuild DLL end command..."
dotnet ../tools/sonar/SonarScanner.MSBuild.dll end /d:sonar.login=${SONAR_TOKEN}
