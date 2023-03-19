@echo off
dotnet build -c Release --self-contained true -r linux-x64 -p:PublishReadyToRun=true
pause