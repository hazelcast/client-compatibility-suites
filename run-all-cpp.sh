#!/usr/bin/env bash

LOCAL_ONLY_ARG=""
CLASS_PATH_ARG="../hazelcast/*"

for i in "$@"
do
	case $i in
		--local)
		LOCAL_ONLY_ARG="--local"
		;;
		--classpath=*)
		CLASS_PATH_ARG=`echo $i | sed 's/[-a-zA-Z0-9]*=//'`
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

echo "Server version is:${SERVER_VERSION}"

cd cpp
./run.sh --classpath=${CLASS_PATH_ARG} --server-version=${SERVER_VERSION} --server-type=${SERVER_TYPE}
cppResult=$?
if [ ${cppResult} -eq 0 ]
then
     echo "Test PASSED for ALL CPP client versions :)"
else
     echo "Test FAILED for at least one CPP client version !!! Result:${cppResult}"
fi

exit ${cppResult}
