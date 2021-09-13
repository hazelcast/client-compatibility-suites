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
		*)
			# unknown option
			echo "Unrecognised option $i" 
		;;
	esac
done

echo "Server version is:${SERVER_VERSION}"

cd python
./run.sh --classpath=${CLASS_PATH_ARG} ${LOCAL_ONLY_ARG} 
pythonResult=$?
if [ ${pythonResult} -eq 0 ]
then
     echo "Test PASSED for ALL PYTHON client versions :)"
else
     echo "Test FAILED for at least one PYTHON client version !!! Result:${pythonResult}"
fi
cd ..

cd cpp
./run.sh --classpath=${CLASS_PATH_ARG} --server-version=${SERVER_VERSION}
cppResult=$?
if [ ${cppResult} -eq 0 ]
then
     echo "Test PASSED for ALL CPP client versions :)"
else
     echo "Test FAILED for at least one CPP client version !!! Result:${cppResult}"
fi
cd ..

if [ $pythonResult -ne 0 ] || [ $cppResult -ne 0 ]
then
    exit -1
fi

exit 0