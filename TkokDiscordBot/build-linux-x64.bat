@echo off
dotnet publish --self-contained true -r linux-x64 -p:PublishReadyToRun=true
pause