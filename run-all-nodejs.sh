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

echo "Server version is:${SERVER_VERSION}"

cd nodejs
./run.sh --classpath=${CLASS_PATH_ARG} --server-version=${SERVER_VERSION} --type=${SERVER_TYPE}
nodejsResult=$?
if [ ${nodejsResult} -eq 0 ]
then
     echo "Test PASSED for ALL Node.js client versions :)"
else
     echo "Test FAILED for at least one Node.js client version !!! Result:${nodejsResult}"

fi

exit ${nodejsResult}
