#!/usr/bin/env bash

for i in "$@"
do
	case $i in
		--client-version=*)
		CLIENT_VERSION=`echo $i | sed 's/[-a-zA-Z0-9]*=//'`
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

if [ "${SERVER_TYPE}" == "oss" ]
then
    JAR_PREFIX="../hazelcast-archive/hazelcast-"
else
	JAR_PREFIX="../hazelcast-archive/hazelcast-enterprise-"
fi


failedAtLeastOnce=0
for SERVER_VERSION in 3.6.8 3.7.8 3.8.9 3.9.4 3.10.7 3.11.7 3.12.10
do
    echo "Running Node.js client version:${CLIENT_VERSION} - server:${SERVER_VERSION}"
    CLASS_PATH_ARG="${JAR_PREFIX}${SERVER_VERSION}.jar:../hazelcast/hazelcast-remote-controller-0.4-SNAPSHOT.jar:../hazelcast-archive/hazelcast-3.12.10-tests.jar:../hazelcast-archive/hazelcast-enterprise-3.12.10-tests.jar"
    ./run-tests.sh --classpath=${CLASS_PATH_ARG} --client-version=${CLIENT_VERSION} --server-version=${SERVER_VERSION} --type=${SERVER_TYPE}
    result=$?
    if [ ${result} -eq 0 ]
    then
        echo "Test PASSED for Node.js client version:${CLIENT_VERSION} - server:${SERVER_VERSION}"
    else
        failedAtLeastOnce=1
        echo "Test FAILED for Node.js client version:${CLIENT_VERSION} - server:${SERVER_VERSION} !!! Result:${result}"
    fi
done

exit ${failedAtLeastOnce}
