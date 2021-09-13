@echo off
setlocal EnableDelayedExpansion
pushd %~dp0

set HZ_VERSION=%1

mkdir tmp_install
cd tmp_install
"C:\Program Files (x86)\GnuWin32\bin\wget.exe" --no-check-certificate --output-document=%HZ_VERSION%.zip https://github.com/hazelcast/hazelcast/archive/%HZ_VERSION%.zip
"C:\Program Files\7-Zip\7z.exe" x %HZ_VERSION%.zip
cd hazelcast-%HZ_VERSION%
mvn install -DskipTests
COPY /Y .\hazelcast\target\hazelcast-%HZ_VERSION%-tests.jar ..\..\hazelcast\hazelcast-%HZ_VERSION%-tests.jar
COPY /Y .\hazelcast\target\hazelcast-%HZ_VERSION%.jar ..\..\hazelcast\hazelcast-%HZ_VERSION%.jar
cd ..
cd ..

CALL .\run-all.bat

popd
