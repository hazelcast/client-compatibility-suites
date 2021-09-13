#!/usr/bin/env bash

LOCAL_ONLY_ARG=""
CLASS_PATH_ARG="../hazelcast/*"

for i in "$@"
do
	case $i in
		--local)
		LOCAL_ONLY_ARG="--user"
		;;
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

pip install --no-index --find-links=./deps/ virtualenv ${LOCAL_ONLY_ARG}

failedAtLeastOnce=0
for CLIENT_VERSION in 3.7.1 3.7.2 3.8 3.8.1 3.9 3.10 3.11 3.12 3.12.1 3.12.2 3.12.3
do
    echo "Running PYTHON client test for client version ${CLIENT_VERSION}. "
    ./run-tests.sh ${LOCAL_ONLY_ARG} --classpath=${CLASS_PATH_ARG} --client-version=${CLIENT_VERSION} --server-version=${SERVER_VERSION} --type=${SERVER_TYPE}
    result=$?
    if [ ${result} -eq 0 ]
    then
        echo "Test PASSED for PYTHON client version:${CLIENT_VERSION} :)"
    else
        failedAtLeastOnce=1
        echo "Test FAILED for PYTHON client version:${CLIENT_VERSION} !!! Result:${result}"
    fi
done

exit ${failedAtLeastOnce}
