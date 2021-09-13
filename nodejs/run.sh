#!/usr/bin/env bash

CLASS_PATH_ARG="../hazelcast/*"

for i in "$@"
do
	case $i in
		--classpath=*)
		CLASS_PATH_ARG=`echo $i | sed 's/[-a-zA-Z0-9]*=//'`
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

failedAtLeastOnce=0
for CLIENT_VERSION in 3.12 3.12.1 3.12.2 3.12.3 3.12.4
do
    echo "Running Node.js client test for client version ${CLIENT_VERSION}. "
    ./run-tests.sh --classpath=${CLASS_PATH_ARG} --client-version=${CLIENT_VERSION} --server-version=${SERVER_VERSION} --type=${SERVER_TYPE}
    result=$?
    if [ ${result} -eq 0 ]
    then
        echo "Test PASSED for Node.js client version:${CLIENT_VERSION} :)"
    else
        failedAtLeastOnce=1
        echo "Test FAILED for Node.js client version:${CLIENT_VERSION} !!! Result:${result}"
    fi
done

exit ${failedAtLeastOnce}
