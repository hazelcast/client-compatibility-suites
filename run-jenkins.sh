#!/usr/bin/env bash

LOCAL_ONLY_ARG=""
CLASS_PATH_ARG="../hazelcast/*"

for i in "$@"
do
	case $i in
		--local)
		LOCAL_ONLY_ARG="--local"
		;;
		--hz-version=*)
		HZ_VERSION=`echo $i | sed 's/[-a-zA-Z0-9]*=//'`
		;;
		*)
			# unknown option
			echo "Unrecognised option $i" 
		;;
	esac
done

mkdir tmp_install
cd tmp_install
wget --no-check-certificate --output-document=${HZ_VERSION}.zip https://github.com/hazelcast/hazelcast/archive/${HZ_VERSION}.zip
unzip ${HZ_VERSION}.zip
cd hazelcast-${HZ_VERSION}
mvn install -DskipTests

cp ./hazelcast/target/hazelcast-${HZ_VERSION}-tests.jar ../../hazelcast/hazelcast-${HZ_VERSION}-tests.jar
cp ./hazelcast/target/hazelcast-${HZ_VERSION}.jar ../../hazelcast/hazelcast-${HZ_VERSION}.jar

cd ../..

./run-all.sh --local
