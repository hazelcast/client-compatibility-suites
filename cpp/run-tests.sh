#!/usr/bin/env bash

function cleanup {
    echo "cleanup is being performed."
    if [ "x${serverPid}" != "x" ]
    then
        echo "Killing server with pid ${serverPid}"
        kill -9 ${serverPid}
    fi
    if [ "x${tailPid}" != "x" ]
    then
        echo "Killing tail process with pid ${tailPid}"
        kill -9 ${tailPid}
    fi
    echo "Cleanup is finished."
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
		--build-name=*)
		BUILD_NAME=`echo $i | sed 's/[-a-zA-Z0-9]*=//'`
		;;
		*)
				# unknown option
		;;
	esac
done

USAGE="run-tests.sh --client-version=<clientversion> --server-version=<serverversion> --build-name=<unique build name for_logs>"

if [ "x${CLIENT_VERSION}" == "x" ]
then
    echo "You should provide client version. Usage: ${USAGE}"
    exit 1
fi

if [ "x${SERVER_VERSION}" == "x" ]
then
    echo "You should provide server version. Usage: ${USAGE}"
    exit 1
fi

if [ "x${BUILD_NAME}" == "x" ]
then
    echo "You should provide unique build name for log file names. Usage: ${USAGE}"
    exit 1
fi

CLASSPATH=${CLASSPATH}:${PWD}/${CLIENT_VERSION}/cpp-hazelcast-1.0-SNAPSHOT.jar

echo CLASSPATH:${CLASSPATH}
echo CLIENT_VERSION:${CLIENT_VERSION}

SERVER_LOG_FILE=javaserver-${BUILD_NAME}-out.log
SERVER_ERR_FILE=javaserver-${BUILD_NAME}-err.log
CLIENT_LOG_FILE=client-${BUILD_NAME}-out.log
CLIENT_ERR_FILE=client-${BUILD_NAME}-err.log
TEST_REPORT_FILE=CPP_Client_Test_Report_${BUILD_NAME}.xml

echo "Server log files are: ${SERVER_LOG_FILE} and ${SERVER_ERR_FILE}"
echo "Client log files are: ${CLIENT_LOG_FILE} and ${CLIENT_ERR_FILE}"

echo "Starting server for version:${CLIENT_VERSION}"

java -cp "${CLASSPATH}" -Dhazelcast.enterprise.license.key=${HAZELCAST_ENTERPRISE_KEY} CppClientListener ${HAZELCAST_ENTERPRISE_KEY} > ${SERVER_LOG_FILE} 2>${SERVER_ERR_FILE} &
serverPid=$!

tail -f  ${SERVER_LOG_FILE} ${SERVER_ERR_FILE} &

DEFAULT_TIMEOUT=300 #seconds
SERVER_PORT=6543

timeout=${DEFAULT_TIMEOUT}

echo "Waiting for the test server to start"

while [ ${timeout} -gt 0 ]
do
    netstat -an  | grep "${SERVER_PORT} "
    if [ $? -eq 0 ]; then
        break
    fi

    echo "Sleeping 1 second. Remaining ${timeout} seconds"
    sleep 1

    timeout=$((timeout-1))
done

if [ ${timeout} -eq 0 ]; then
    echo "Server could not start on port ${SERVER_PORT} in $DEFAULT_TIMEOUT seconds. Test FAILED."
    exit 1
else
    echo "Server started in $((DEFAULT_TIMEOUT - timeout)) seconds"
fi

echo "Started server. Starting tests for client version:${CLIENT_VERSION} against server version:${SERVER_VERSION}"
CLIENT_LOG_FILE=client-${BUILD_NAME}-out.log
CLIENT_ERR_FILE=client-${BUILD_NAME}-err.log
if [ ${CLIENT_VERSION} == "3.6" ]
then
    LD_LIBRARY_PATH=${PWD}/${CLIENT_VERSION}:${LD_LIBRARY_PATH} ${PWD}/${CLIENT_VERSION}/clientTest_SHARED_64 > ${CLIENT_LOG_FILE} 2>${CLIENT_ERR_FILE} &
    clientPid=$!
else
    if [[ $SERVER_VERSION == 3.9* ]] || [[ $SERVER_VERSION == 3.8* ]] || [[ $SERVER_VERSION == 3.7* ]] || [[ $SERVER_VERSION == 3.6* ]] ; then
        if [[ ${CLIENT_VERSION} != 3.6* ]] || [[ ${CLIENT_VERSION} != 3.7* ]] || [[ ${CLIENT_VERSION} != 3.8* ]] || [[ ${CLIENT_VERSION} != 3.9* ]]; then
            TEST_FILTER="--gtest_filter=-ClientSemaphoreTest.*"
        fi
        if [ "x${TEST_FILTER}" == "x" ]; then
           TEST_FILTER="--gtest_filter=-FlakeIdGeneratorApiTest.*:PnCounterFunctionalityTest.*:ClientPNCounterNoDataMemberTest.*:ClientPNCounterConsistencyLostTest.*:BasicPnCounterAPITest.*"
        else
           TEST_FILTER+=":FlakeIdGeneratorApiTest.*:PnCounterFunctionalityTest.*:ClientPNCounterNoDataMemberTest.*:ClientPNCounterConsistencyLostTest.*:BasicPnCounterAPITest.*"
        fi
    else
        if [[ ${CLIENT_VERSION} != 3.6* ]] || [[ ${CLIENT_VERSION} != 3.7* ]] || [[ ${CLIENT_VERSION} != 3.8* ]] || [[ ${CLIENT_VERSION} != 3.9* ]]; then
            TEST_FILTER="--gtest_filter=-ClientSemaphoreTest.*"
        fi
    fi
    if [[ $SERVER_VERSION == 3.7* ]]; then
         if [ "x${TEST_FILTER}" == "x" ]; then
            TEST_FILTER="--gtest_filter=-*.testRemoveAll/*:*.testRemoveAll:*.testRemoveAllNearCache"
         else
            TEST_FILTER+=":*.testRemoveAll/*:*.testRemoveAll:*.testRemoveAllNearCache"
         fi
    fi

    if [[ ${CLIENT_VERSION} != 3.8* ]] || [[ ${CLIENT_VERSION} != 3.9* ]] || [[ ${CLIENT_VERSION} != 3.10* ]]; then
         if [ "x${TEST_FILTER}" == "x" ]; then
            TEST_FILTER="--gtest_filter=-RawPointerMapTest.testGetEntryViewForNonExistentData:MixedMapAPITestInstance/MixedMapAPITest.testGetEntryViewForNonExistentData/*"
         else
            TEST_FILTER+=":RawPointerMapTest.testGetEntryViewForNonExistentData:MixedMapAPITestInstance/MixedMapAPITest.testGetEntryViewForNonExistentData/*"
         fi
    fi

    # The ClientConnectionTest test is excluded since there was a test error prior to 3.10 tests (used non-ssl factory but useSSL flag is to true in the test)
    if [[ "x${TEST_FILTER}" == "x" ]]; then
         TEST_FILTER="--gtest_filter=-ClientConnectionTest.*"
    else
         TEST_FILTER+=":ClientConnectionTest.*"
    fi

    echo "Using the google test filter as: ${TEST_FILTER}"
    LD_LIBRARY_PATH=${PWD}/${CLIENT_VERSION}:${LD_LIBRARY_PATH} ${PWD}/${CLIENT_VERSION}/clientTest_SHARED_64 ${TEST_FILTER} --gtest_output="xml:${TEST_REPORT_FILE}" > ${CLIENT_LOG_FILE} 2>${CLIENT_ERR_FILE} &
    clientPid=$!
fi

tail -f ${CLIENT_LOG_FILE} ${CLIENT_ERR_FILE} &
tailPid=$!

wait ${clientPid}
result=$?

cleanup

echo "run-tests.sh is exiting with result ${result}"
exit ${result}

