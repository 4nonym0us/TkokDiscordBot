@echo off
dotnet build -c Release --self-contained false -r linux-x64 -p:PublishReadyToRun=true
pause