import argparse
import json

from util import (
    ServerKind,
)

# Filters for 'os' type
os_test_filters = [
    "com.hazelcast.aggregation.**",
    "com.hazelcast.aws.**",
    "com.hazelcast.azure.**",
    "com.hazelcast.cache.*Test",
    "com.hazelcast.cache.eviction.**",
    "com.hazelcast.cache.impl.**",
    "com.hazelcast.cache.instance.**",
    "com.hazelcast.cache.jsr.**",
    "com.hazelcast.cache.stats.**",
    "com.hazelcast.client.*Test",
    "com.hazelcast.client.aggregation.**",
    "com.hazelcast.client.bluegreen.**",
    "com.hazelcast.client.cache.**",
    "com.hazelcast.client.cardinality.**",
    "com.hazelcast.client.cluster.**",
    "com.hazelcast.client.collections.**",
    "com.hazelcast.client.config.**",
    "com.hazelcast.client.connectionstrategy.**",
    "com.hazelcast.client.console.**",
    "com.hazelcast.client.executor.**",
    "com.hazelcast.client.flakeidgen.**",
    "com.hazelcast.client.genericrecord.**",
    "com.hazelcast.client.heartbeat.**",
    "com.hazelcast.client.impl.**",
    "com.hazelcast.client.internal.**",
    "com.hazelcast.client.io.**",
    "com.hazelcast.client.listeners.**",
    "com.hazelcast.client.loadBalancer.**",
    "com.hazelcast.client.logging.**",
    "com.hazelcast.client.map.*Test",
    "com.hazelcast.client.map.helpers.**",
    "com.hazelcast.client.map.impl.listener.**",
    "com.hazelcast.client.map.impl.nearcache.**",
    "com.hazelcast.client.map.impl.query.**",
    "com.hazelcast.client.map.impl.querycache.**",
    "com.hazelcast.client.multimap.**",
    "com.hazelcast.client.partitionservice.**",
    "com.hazelcast.client.pncounter.**",
    "com.hazelcast.client.projection.**",
    "com.hazelcast.client.properties.**",
    "com.hazelcast.client.protocol.**",
    "com.hazelcast.client.queue.**",
    "com.hazelcast.client.replicatedmap.**",
    "com.hazelcast.client.ringbuffer.**",
    "com.hazelcast.client.scheduledexecutor.**",
    "com.hazelcast.client.serialization.**",
    "com.hazelcast.client.standalone.**",
    "com.hazelcast.client.statistics.**",
    "com.hazelcast.client.stress.**",
    "com.hazelcast.client.test.**",
    "com.hazelcast.client.topic.**",
    "com.hazelcast.client.tpc.**",
    "com.hazelcast.client.txn.**",
    "com.hazelcast.client.usercodedeployment.**",
    "com.hazelcast.client.util.**",
    "com.hazelcast.cluster.**",
    "com.hazelcast.collection.**",
    "com.hazelcast.config.**",
    "com.hazelcast.core.**",
    "com.hazelcast.cp.**",
    "com.hazelcast.dataconnection.**",
    "com.hazelcast.executor.**",
    "com.hazelcast.flakeidgen.impl.**",
    "com.hazelcast.function.**",
    "com.hazelcast.gcp.**",
    "com.hazelcast.instance.**",
    "com.hazelcast.internal.**",
    "com.hazelcast.it.hibernate.**",
    "com.hazelcast.jet.**",
    "com.hazelcast.journal.**",
    "com.hazelcast.json.**",
    "com.hazelcast.kubernetes.**",
    "com.hazelcast.listeners.**",
    "com.hazelcast.logging.**",
    "com.hazelcast.map.**",
    "com.hazelcast.memory.**",
    "com.hazelcast.mock.**",
    "com.hazelcast.multimap.**",
    "com.hazelcast.nio.**",
    "com.hazelcast.partition.**",
    "com.hazelcast.projection.**",
    "com.hazelcast.query.**",
    "com.hazelcast.replicatedmap.**",
    "com.hazelcast.ringbuffer.**",
    "com.hazelcast.scheduledexecutor.impl.**",
    "com.hazelcast.security.**",
    "com.hazelcast.spi.**",
    "com.hazelcast.sql.**",
    "com.hazelcast.test.**",
    "com.hazelcast.topic.impl.reliable.**",
    "com.hazelcast.version.**"
]

# Filters for 'enterprise' type
enterprise_test_filters = [
    "com.hazelcast.auditlog.**",
    "com.hazelcast.cache.**",
    "com.hazelcast.client.*Test",
    "com.hazelcast.config.tpc.**",
    "com.hazelcast.cp.**",
    "com.hazelcast.instance.impl.**",
    "com.hazelcast.internal.**",
    "com.hazelcast.jet.**",
    "com.hazelcast.map.**",
    "com.hazelcast.nio.**",
    "com.hazelcast.query.**",
    "com.hazelcast.security.**",
    "com.hazelcast.serialization.**"
]

def parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser(
        description="Returns the Java Standalone Client test filters matrix as a JSON array"
    )

    parser.add_argument(
        "--server-kind",
        dest="server_kind",
        action="store",
        type=str,
        required=True,
        choices=[kind.name.lower() for kind in ServerKind],
        help="The Hazelcast server type that the tests should be running against",
    )

    return parser.parse_args()


if __name__ == "__main__":
    args = parse_args()
    server_kind = ServerKind[args.server_kind.upper()]
    if args.server_kind == ServerKind.ENTERPRISE:
        test_filters = os_test_filters
    else:
        test_filters = enterprise_test_filters

    print(json.dumps(test_filters))