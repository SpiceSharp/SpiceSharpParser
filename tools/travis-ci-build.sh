#!/bin/sh
echo "Changing to /src directory...."
cd src
echo "Running build..."
dotnet build
echo "Running tests..."
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=\"opencover,lcov\" /p:CoverletOutput=../lcov

