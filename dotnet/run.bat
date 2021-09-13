@echo off
setlocal EnableDelayedExpansion
pushd %~dp0

IF -%1-==-- (
	set CLASSFOLDER=..\hazelcast\*
) ELSE (
	set CLASSFOLDER=%1
)
echo Hazelcast Folder:%CLASSFOLDER%
echo Starting .Net Compatibility Tests...

CALL .\run-tests.bat %CLASSFOLDER% 3.9.3
CALL .\run-tests.bat %CLASSFOLDER% 3.9.2
CALL .\run-tests.bat %CLASSFOLDER% 3.9.1
CALL .\run-tests.bat %CLASSFOLDER% 3.9
CALL .\run-tests.bat %CLASSFOLDER% 3.8.2
CALL .\run-tests.bat %CLASSFOLDER% 3.8.1
CALL .\run-tests.bat %CLASSFOLDER% 3.8
CALL .\run-tests.bat %CLASSFOLDER% 3.7.1
CALL .\run-tests.bat %CLASSFOLDER% 3.6.4
CALL .\run-tests.bat %CLASSFOLDER% 3.6.3
CALL .\run-tests.bat %CLASSFOLDER% 3.6.2
CALL .\run-tests.bat %CLASSFOLDER% 3.6.1

popd
