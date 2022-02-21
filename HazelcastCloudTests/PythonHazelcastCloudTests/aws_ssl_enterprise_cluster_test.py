import os
import time
import unittest
from os.path import abspath

import hazelcast
import logging
import random

from hazelcast import HazelcastClient
from hazelcast.discovery import HazelcastCloudDiscovery
from hzrc.client import HzRemoteController
from hzrc.ttypes import CloudCluster


class TestAwsEnterpriseClusterTestsWithSsl(unittest.TestCase):
    cluster: CloudCluster = None
    client: HazelcastClient = None
    rc: HzRemoteController = None
    HazelcastCloudDiscovery._CLOUD_URL_BASE = os.getenv('baseUrl').replace("https://", "")

    @classmethod
    def setUpClass(cls) -> None:
        cls.rc = HzRemoteController("127.0.0.1", 9701)
        cls.cluster = cls.rc.createEnterpriseCluster("aws", os.getenv('hzVersion'), True)
        cls.client = hazelcast.HazelcastClient(
            cluster_name=cls.cluster.nameForConnect,
            cloud_discovery_token=cls.cluster.token,
            statistics_enabled=True,
            ssl_enabled=True,
            ssl_cafile=abspath(os.path.join(cls.cluster.certificatePath + "ca.pem")),
            ssl_certfile=abspath(os.path.join(cls.cluster.certificatePath + "cert.pem")),
            ssl_keyfile=abspath(os.path.join(cls.cluster.certificatePath + "key.pem")),
            ssl_password=cls.cluster.tlsPassword)

    def test_try_connect_enterprise_ssl_cluster_without_certificates(self):
        value = False
        try:
            hazelcast.HazelcastClient(
                cluster_name=self.cluster.nameForConnect,
                cloud_discovery_token=self.cluster.token,
                cluster_connect_timeout=10)
        except:
            value = True
        self.assertTrue(value, "Client shouldn't able to connect to ssl cluster without certificates")

    def test_connect_enterprise_ssl_cluster_with_certificates(self):

        map1 = self.client.get_map("map_for_test_connect_enterprise_ssl_cluster_with_certificates").blocking()
        map1.clear()
        while map1.size() < 20:
            random_key = random.randint(1, 100000)
            try:
                map1.put("key" + str(random_key), "value" + str(random_key))
            except:
                logging.exception("Put operation failed!")

        self.assertEqual(map1.size(), 20, "Map size should be 20")
