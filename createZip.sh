#!/bin/bash -e
rm -rf FrogHelper.zip
dotnet build
zip FrogHelper.zip -r everest.yaml FrogHelper/bin/Debug/FrogHelper.dll Ahorn Graphics Loenn