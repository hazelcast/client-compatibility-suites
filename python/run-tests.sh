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
		--user)
		LOCAL_ONLY=true
		;;
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

VENV_NAME="client-${CLIENT_VERSION}-server-${SERVER_VERSION}"

python -m virtualenv -p python ${VENV_NAME}
source ${VENV_NAME}/bin/activate

pip install --no-index --find-links=./deps/ -r ./${CLIENT_VERSION}/test-requirements.txt

echo "Starting remote controller for client:${CLIENT_VERSION} - server:${SERVER_VERSION}"
java -cp "${CLASSPATH}" -Dhazelcast.enterprise.license.key=${HAZELCAST_ENTERPRISE_KEY} com.hazelcast.remotecontroller.Main > rc-${CLIENT_VERSION}-${SERVER_VERSION}-out.log 2>rc-${CLIENT_VERSION}-${SERVER_VERSION}-err.log &
serverPid=$!

sleep 15

echo "Starting nose tests for client:${CLIENT_VERSION} - server:${SERVER_VERSION}"
find . -name \*.pyc -delete

ARR_SERVER_VERSION=(${SERVER_VERSION//./ })
SERVER_VERSION_PADDED=$(printf %02d ${ARR_SERVER_VERSION[1]})

ENTERPRISE_ARG=""

if [ "${SERVER_TYPE}" = "oss" ];
then
    ENTERPRISE_ARG="and (not enterprise)"
fi

nosetests -v --with-xunit --xunit-file=nosetests-${CLIENT_VERSION}-${SERVER_VERSION}.xml ${CLIENT_VERSION}/tests/* --nologcapture -A "(not category or category <= 3.${SERVER_VERSION_PADDED}) ${ENTERPRISE_ARG}"
result=$?

deactivate
rm -rf ${VENV_NAME}

exit ${result}
