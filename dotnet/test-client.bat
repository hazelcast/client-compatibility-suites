@echo off
del /q nunit-result-*.xml
powershell ./test-client.ps1 %*