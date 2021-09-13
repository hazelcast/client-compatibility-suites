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

cd python
./run.sh ${LOCAL_ONLY_ARG} --classpath=${CLASS_PATH_ARG}  --server-version=${SERVER_VERSION} --type=${SERVER_TYPE}
pythonResult=$?
if [ ${pythonResult} -eq 0 ]
then
     echo "Test PASSED for ALL PYTHON client versions :)"
else
     echo "Test FAILED for at least one PYTHON client version !!! Result:${pythonResult}"

fi

exit ${pythonResult}
