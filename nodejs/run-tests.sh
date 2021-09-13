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

trap cleanup EXIT

for i in "$@"
do
	case $i in
		--classpath=*)
		CLASSPATH=`echo $i | sed 's/[-a-zA-Z0-9]*=//'`
		;;
		--client-version=*)
		CLIENT_VERSION=`echo $i | sed 's/[-a-zA-Z0-9]*=//'`
		;;
		--server-version=*)
		SERVER_VERSION=`echo $i | sed 's/[-a-zA-Z0-9]*=//'`
		;;
		--type=*)
		SERVER_TYPE=`echo $i | sed 's/[-a-zA-Z0-9]*=//'`
		;;
		*)
			# unknown option
			echo "Unrecognised option $i"
		;;
	esac
done

echo CLASSPATH:${CLASSPATH}
echo CLIENT_VERSION:${CLIENT_VERSION}
echo SERVER_VERSION:${SERVER_VERSION}
echo SERVER_TYPE:${SERVER_TYPE}

export SERVER_VERSION=${SERVER_VERSION}
export SERVER_TYPE=${SERVER_TYPE}


echo "Starting remote controller for client:${CLIENT_VERSION} - server:${SERVER_VERSION}"

java -cp "${CLASSPATH}" -Dhazelcast.enterprise.license.key=${HAZELCAST_ENTERPRISE_KEY} com.hazelcast.remotecontroller.Main > rc-${CLIENT_VERSION}-${SERVER_VERSION}-out.log 2>rc-${CLIENT_VERSION}-${SERVER_VERSION}-err.log &
serverPid=$!

sleep 15

cd ${CLIENT_VERSION}

npm install

echo "Starting tests for client:${CLIENT_VERSION} - server:${SERVER_VERSION}"

node_modules/mocha/bin/_mocha --recursive
result=$?

unset SERVER_TYPE
unset SERVER_VERSION

exit ${result}
