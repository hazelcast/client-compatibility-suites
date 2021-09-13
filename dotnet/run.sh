#!/usr/bin/env bash

CLASSPATH="../hazelcast/*"

for i in "$@"
do
	case $i in
		--classpath=*)
		CLASS_PATH_ARG=`echo $i | sed 's/[-a-zA-Z0-9]*=//'`
		;;
		*)
				# unknown option
		;;
	esac
done

bash ./run-tests.sh --classpath=${CLASS_PATH_ARG} --client-version=3.8
