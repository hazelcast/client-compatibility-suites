#!/usr/bin/env bash

for i in "$@"
do
	case $i in
		--client-versions=*)
		CLIENT_VERSIONS_TO_BE_TESTED=`echo $i | sed 's/[-a-zA-Z0-9]*=//'`
		;;
		--server-versions=*)
		SERVER_VERSIONS_TO_BE_TESTED=`echo $i | sed 's/[-a-zA-Z0-9]*=//'`
		;;
		*)
			# unknown option
			echo "Unrecognised option $i"
		;;
	esac
done

if [ "x${CLIENT_VERSIONS_TO_BE_TESTED}" == "x" ]
then
    echo "You should provide client versions to be tested with --client-versions=<versions> and the server versions to be tested with --server-versions=<versions>."
    exit 1
fi

if [ "x${SERVER_VERSIONS_TO_BE_TESTED}" == "x" ]
then
    echo "You should provide server versions to be tested with --server-versions=<versions> . Client version(s):${CLIENT_VERSIONS_TO_BE_TESTED}"
    exit 1
fi

SERVER_JAR_DIRECTORY="../hazelcast-archive"
TEST_JAR_VERSION="3.12"

cd cpp
pids=""
for CLIENT_VERSION in ${CLIENT_VERSIONS_TO_BE_TESTED}
do
    for SERVER_VERSION in ${SERVER_VERSIONS_TO_BE_TESTED}
    do
        if [ "${SERVER_VERSION}" == "3.6.*" ] || [ "${SERVER_VERSION}" == "3.7.*" ] || [ "${SERVER_VERSION}" == "3.8.*" ] || [ "${SERVER_VERSION}" == "3.9.*" ] ||[ "${SERVER_VERSION}" == "3.10.*" ] || [ "${SERVER_VERSION}" == "3.11.*" ]; then
           CLASSPATH=${SERVER_JAR_DIRECTORY}/hazelcast-enterprise-${SERVER_VERSION}.jar:${SERVER_JAR_DIRECTORY}/hazelcast-${TEST_JAR_VERSION}-tests.jar:../hazelcast/hazelcast-remote-controller-0.3-SNAPSHOT.jar
        else
           CLASSPATH=${SERVER_JAR_DIRECTORY}/hazelcast-enterprise-${SERVER_VERSION}.jar:${SERVER_JAR_DIRECTORY}/hazelcast-enterprise-${SERVER_VERSION}-tests.jar:${SERVER_JAR_DIRECTORY}/hazelcast-${TEST_JAR_VERSION}-tests.jar:../hazelcast/hazelcast-remote-controller-0.3-SNAPSHOT.jar
        fi
        ./run.sh --classpath=${CLASSPATH} --client-versions=${CLIENT_VERSION} --server-version=${SERVER_VERSION} --server-type=enterprise &
        pid=$!
        pids+=" ${pid}"
        echo "Started test for client version ${CLIENT_VERSION} against server version ${SERVER_VERSION}. Pid: ${pid}"
    done
done

echo "The following test processes are spawned ${pids}"

failed=0
for CLIENT_VERSION in ${CLIENT_VERSIONS_TO_BE_TESTED}
do
    for pid in ${pids}
    do
        wait ${pid}
        result=$?
        if [ ${result} -eq 0 ]
        then
             echo "Test PASSED for pid ${pid} :)"
        else
             echo "Test FAILED for pid ${pid} ! Result:${result}"
             failed=1
        fi
    done
done


cd ..

exit ${failed}

