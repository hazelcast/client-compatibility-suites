#!/usr/bin/env bash

if [ -n "${HAZELCAST_ENTERPRISE_KEY}" ]; then
	TYPE="enterprise"
else
	TYPE="oss"
fi

for i in "$@"
do
	case $i in
		--hz-version=*)
		HZ_VERSION=`echo $i | sed 's/[-a-zA-Z0-9]*=//'`
		;;
		--type=*)
		TYPE=`echo $i | sed 's/[-a-zA-Z0-9]*=//'`
		if [[ "${TYPE}" != "oss" && "${TYPE}" != "enterprise" ]]
		then
        	echo "type parameter usage: --type=[oss|enterprise]"
        	exit
    	fi
		;;
		*)
				# unknown option
		;;
	esac
done

if [ -z $HZ_VERSION ]; then
	echo "hazelcast version parameter is missing. Usage: --hz-version=VERSION"
	exit
fi

HAZELCAST_TEST_VERSION=${HZ_VERSION}
HAZELCAST_VERSION=${HZ_VERSION}
HAZELCAST_ENTERPRISE_VERSION=${HZ_VERSION}
HAZELCAST_RC_VERSION="0.3-SNAPSHOT"
SNAPSHOT_REPO="https://oss.sonatype.org/content/repositories/snapshots"
RELEASE_REPO="http://repo1.maven.apache.org/maven2"
ENTERPRISE_RELEASE_REPO="https://repository.hazelcast.com/release/"
ENTERPRISE_SNAPSHOT_REPO="https://repository.hazelcast.com/snapshot/"


if [[ ${HZ_VERSION} == *-SNAPSHOT ]]
then
	REPO=${SNAPSHOT_REPO}
	ENTERPRISE_REPO=${ENTERPRISE_SNAPSHOT_REPO}
else
	REPO=${RELEASE_REPO}
	ENTERPRISE_REPO=${ENTERPRISE_RELEASE_REPO}
fi


mvn dependency:get -DrepoUrl=${SNAPSHOT_REPO} -Dartifact=com.hazelcast:hazelcast-remote-controller:${HAZELCAST_RC_VERSION} -Ddest=hazelcast-remote-controller-${HAZELCAST_RC_VERSION}.jar
if [ $? -ne 0 ]
then
    echo "Failed download remote-controller jar com.hazelcast:hazelcast-remote-controller:${HAZELCAST_RC_VERSION}"
    exit 1
fi

mvn dependency:get -DrepoUrl=${REPO} -Dartifact=com.hazelcast:hazelcast:${HAZELCAST_VERSION} -Ddest=hazelcast-${HAZELCAST_VERSION}.jar
if [ $? -ne 0 ]
then
    echo "Failed download hazelcast jar com.hazelcast:hazelcast:${HAZELCAST_VERSION}"
    exit 1
fi

mvn dependency:get -DrepoUrl=${REPO} -Dartifact=com.hazelcast:hazelcast:${HAZELCAST_TEST_VERSION}:jar:tests -Ddest=hazelcast-${HAZELCAST_TEST_VERSION}-tests.jar
if [ $? -ne 0 ]
then
    echo "Failed download hazelcast test jar com.hazelcast:hazelcast:${HAZELCAST_TEST_VERSION}:jar:tests"
    exit 1
fi


if [ "${TYPE}" == "enterprise" ]
then
    mvn dependency:get -DrepoUrl=${ENTERPRISE_REPO} -Dartifact=com.hazelcast:hazelcast-enterprise:${HAZELCAST_ENTERPRISE_VERSION} -Ddest=hazelcast-enterprise-${HAZELCAST_ENTERPRISE_VERSION}.jar
    if [ $? -ne 0 ]
    then
        echo "Failed download hazelcast enterprise jar com.hazelcast:hazelcast-enterprise:${HAZELCAST_ENTERPRISE_VERSION}"
        exit 1
    fi
fi

exit 0

