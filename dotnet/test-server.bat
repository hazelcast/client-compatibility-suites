@echo off
del /q nunit-result-*.xml
powershell .\test-server.ps1 %*