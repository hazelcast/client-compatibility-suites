@echo off
setlocal EnableDelayedExpansion
pushd %~dp0

set CLASSFOLDER=%1
set CLIENT_VERSION=%2
set TEST_DLL=%CLIENT_VERSION%/Hazelcast.Test.dll

IF -%HZ_TYPE%-==-oss- (
	set WHERE_PARAM=--where "cat != enterprise"
) ELSE (
	set WHERE_PARAM=
)

echo "Starting hazelcast-remote-controller"
start /min "hazelcast-remote-controller-%CLIENT_VERSION%" cmd /c "java -Dhazelcast.enterprise.license.key=%HAZELCAST_ENTERPRISE_KEY% -cp %CLASSFOLDER% com.hazelcast.remotecontroller.Main|| call echo %^errorlevel% > errorlevel"
REM> rc_stdout-%CLIENT_VERSION%.txt 2>rc_stderr-%CLIENT_VERSION%.txt

REM Wait for Hazelcast RC to start
ping -n 4 127.0.0.1 > nul
if exist errorlevel (
    set /p exitcode=<errorlevel
    echo ERROR: Unable to start hazelcast-remote-controller
    exit /b %exitcode%
)

IF %ERRORLEVEL% NEQ 0 exit /b %ERRORLEVEL%

set TEST_PARAMETERS="%TEST_DLL%" %WHERE_PARAM% --labels=All --result=nunit-result-%CLIENT_VERSION%.xml;format=nunit2 --framework=v4.0
echo Running Unit Tests for version:%CLIENT_VERSION% ...
packages\NUnit.ConsoleRunner.3.7.0\tools\nunit3-console.exe %TEST_PARAMETERS%

taskkill /T /F /FI "WINDOWTITLE eq hazelcast-remote-controller-%CLIENT_VERSION%"

echo Unit test run completed for version:%CLIENT_VERSION% ...
echo ========================================================
popd
