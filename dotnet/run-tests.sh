#!/usr/bin/env bash

function cleanup {
    echo "cleanup is being performed."
    if [ "x${serverPid}" != "x" ]
    then
        echo "Killing remote-controller with pid ${serverPid}"
        kill -9 ${serverPid}
    fi
    exit
}

#trap cleanup EXIT

for i in "$@"
do
	case $i in
		--classpath=*)
		CLASSPATH=`echo $i | sed 's/[-a-zA-Z0-9]*=//'`
		;;
		--client-version=*)
		CLIENT_VERSION=`echo $i | sed 's/[-a-zA-Z0-9]*=//'`
		;;
		*)
				# unknown option
		;;
	esac
done

echo CLASSPATH:${CLASSPATH}
echo CLIENT_VERSION:${CLIENT_VERSION}

echo "Starting remote controller for version:${CLIENT_VERSION}"
java -cp "${CLASSPATH}" -Dhazelcast.enterprise.license.key=${HAZELCAST_ENTERPRISE_KEY} com.hazelcast.remotecontroller.Main>rc_stdout-${CLIENT_VERSION}.log 2>rc_stderr-${CLIENT_VERSION}.log &
serverPid=$!

sleep 5

echo "Starting nunit tests for version:${CLIENT_VERSION}"
MONO_IOMAP=all
mono tools/nunit-console.exe "${CLIENT_VERSION}/Hazelcast.Test.dll" #/labels /xml:test-${CLIENT_VERSION}-results.xml /noshadow"

cleanup
