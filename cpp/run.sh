#!/usr/bin/env bash

CLASS_PATH_ARG="../hazelcast/*"

for i in "$@"
do
	case $i in
		--classpath=*)
		CLASS_PATH_ARG=`echo $i | sed 's/[-a-zA-Z0-9]*=//'`
		;;
		--client-versions=*)
		CLIENT_VERSIONS=`echo $i | sed 's/[-a-zA-Z0-9]*=//'`
		;;
		--server-version=*)
		SERVER_VERSION=`echo $i | sed 's/[-a-zA-Z0-9]*=//'`
		;;
		--server-type=*)
		SERVER_TYPE=`echo $i | sed 's/[-a-zA-Z0-9]*=//'`
		;;
		*)
			# unknown option
			echo "Unrecognised option $i" 
		;;
	esac
done

if [ "x${CLIENT_VERSIONS}" != "x" ]; then
    ALL_CLIENT_VERSIONS=${CLIENT_VERSIONS}
else
    if [ "${HZ_TYPE}" = "oss" ];
    then
        # do not run for client version 3.8 which is using TLS that requires an enterprise server
        ALL_CLIENT_VERSIONS="3.12 3.11 3.10.1 3.10 3.8.1 3.7 3.6.3 3.6.2 3.6.1 3.6"
    else
        ALL_CLIENT_VERSIONS="3.12 3.11 3.10.1 3.10 3.9.1 3.9 3.8.3 3.8.2 3.8.1 3.8 3.7 3.6.3 3.6.2 3.6.1 3.6"
    fi
fi

echo "Will test for client version(s): ${ALL_CLIENT_VERSIONS}"

cd ..
parentDirectory=`pwd`
cd cpp

env

failedAtLeastOnce=0
for CLIENT_VERSION in ${ALL_CLIENT_VERSIONS}
do
    runTestsScriptName=run-tests-with-remote-controller.sh
    if [[ $CLIENT_VERSION == 3.9* ]] || [[ $CLIENT_VERSION == 3.8* ]] || [[ $CLIENT_VERSION == 3.7* ]] || [[ $CLIENT_VERSION == 3.6* ]] ; then
        runTestsScriptName=run-tests.sh
    fi

    BUILD_NAME=build_client_${CLIENT_VERSION}_server_${SERVER_VERSION}_${SERVER_TYPE}
    docker ps -a --filter "name=/${BUILD_NAME}"
    DOCKER_CONTAINER_ID=`docker ps -a -q -f "name=${BUILD_NAME}"`
    if [ "x${DOCKER_CONTAINER_ID}" != "x" ]
    then
    	echo "Found existing docker container named ${BUILD_NAME} and id ${DOCKER_CONTAINER_ID}. Deleting the existing container."
        docker stop -t 1 ${DOCKER_CONTAINER_ID}
        docker rm ${DOCKER_CONTAINER_ID}
    fi

    echo "Running cpp client test for client version ${CLIENT_VERSION} and server version ${SERVER_VERSION} (${SERVER_TYPE}). Using run tests script ${runTestsScriptName}. Container name: ${BUILD_NAME}"
    docker run -d --name ${BUILD_NAME} -v ${parentDirectory}:/client-compatibility-test_src \
    -e AWS_ACCESS_KEY_ID=${AWS_ACCESS_KEY_ID} -e HAZELCAST_ENTERPRISE_KEY=${HAZELCAST_ENTERPRISE_KEY} -e AWS_SECRET_ACCESS_KEY=${AWS_SECRET_ACCESS_KEY} -e HZ_TEST_AWS_INSTANCE_PRIVATE_IP=${HZ_TEST_AWS_INSTANCE_PRIVATE_IP} \
    ihsan/gcc_346_ssl /bin/bash -l -c "cp -r /client-compatibility-test_src client-compatibility-test; cd client-compatibility-test/cpp; env; ./${runTestsScriptName} --classpath=${CLASS_PATH_ARG} --client-version=${CLIENT_VERSION} --server-version=${SERVER_VERSION} --build-name=${BUILD_NAME} &> ${BUILD_NAME}.log "
done

for CLIENT_VERSION in ${ALL_CLIENT_VERSIONS}
do
    BUILD_NAME=build_client_${CLIENT_VERSION}_server_${SERVER_VERSION}_${SERVER_TYPE}
    echo "Waiting docker build ${BUILD_NAME} to finish"
    result=`docker wait ${BUILD_NAME}`
    echo "Docker build ${BUILD_NAME} exited with ${result}"
    if [ "${result}" == "0" ]
    then
        echo "Test PASSED for client version:${CLIENT_VERSION} for server version ${SERVER_VERSION} (Server Type: ${SERVER_TYPE}) :)"
    else
        failedAtLeastOnce=1
        echo "Test FAILED for client version:${CLIENT_VERSION}  for server version ${SERVER_VERSION} (Server Type: ${SERVER_TYPE}) ! Result:${result}"
    fi
    SERVER_LOG_FILE=javaserver-${BUILD_NAME}-out.log
    SERVER_ERR_FILE=javaserver-${BUILD_NAME}-err.log
    CLIENT_LOG_FILE=client-${BUILD_NAME}-out.log
    CLIENT_ERR_FILE=client-${BUILD_NAME}-err.log
    TEST_REPORT_FILE=CPP_Client_Test_Report_${BUILD_NAME}.xml

    docker cp ${BUILD_NAME}:client-compatibility-test/cpp/${BUILD_NAME}.log .
    docker cp ${BUILD_NAME}:client-compatibility-test/cpp/${SERVER_LOG_FILE} .
    docker cp ${BUILD_NAME}:client-compatibility-test/cpp/${SERVER_ERR_FILE} .
    docker cp ${BUILD_NAME}:client-compatibility-test/cpp/${CLIENT_LOG_FILE} .
    docker cp ${BUILD_NAME}:client-compatibility-test/cpp/${CLIENT_ERR_FILE} .
    docker cp ${BUILD_NAME}:client-compatibility-test/cpp/${TEST_REPORT_FILE} .

    docker rm ${BUILD_NAME}
done

exit ${failedAtLeastOnce}

