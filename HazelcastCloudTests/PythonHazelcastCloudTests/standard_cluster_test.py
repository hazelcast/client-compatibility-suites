import os
import time
import unittest
from os.path import abspath

import pytest as pytest
from parameterized import parameterized

import hazelcast
import logging
import random

from Helper import HelperMethods
from hazelcast import HazelcastClient
from hazelcast.discovery import HazelcastCloudDiscovery
from hazelcast.errors import IllegalStateError
from hzrc.client import HzRemoteController
from hzrc.ttypes import CloudCluster


class StandardClusterTests(unittest.TestCase):
    ssl_enabled_cluster: CloudCluster = None
    ssl_disabled_cluster: CloudCluster = None
    rc: HzRemoteController = None
    HazelcastCloudDiscovery._CLOUD_URL_BASE = os.getenv('baseUrl').replace("https://", "")

    @classmethod
    def setUpClass(cls) -> None:
        cls.rc = HzRemoteController("127.0.0.1", 9701)
        cls.ssl_enabled_cluster = cls.rc.createStandardCluster(os.getenv('hzVersion'), True)
        cls.ssl_disabled_cluster = cls.rc.createStandardCluster(os.getenv('hzVersion'), False)

    @parameterized.expand([(True, True), (False, True), (True, False), (False, False)])
    def test_ssl_cluster(self, is_smart, is_ssl_enabled):
        if is_ssl_enabled:
            hazelcast.client._logger.info("Create ssl enabled client config for smart routing " + str(is_smart))
            client = hazelcast.HazelcastClient(
                **HelperMethods.create_client_config_with_ssl(self.ssl_enabled_cluster.nameForConnect, self.ssl_enabled_cluster.token, is_smart,
                                                              self.ssl_enabled_cluster.certificatePath, self.ssl_enabled_cluster.tlsPassword))
            cluster = self.ssl_enabled_cluster
        else:
            hazelcast.client._logger.info("Create ssl disabled client config for smart routing " + str(is_smart))
            client = hazelcast.HazelcastClient(**HelperMethods.create_client_config(self.ssl_disabled_cluster.nameForConnect, self.ssl_disabled_cluster.token, is_smart))
            cluster = self.ssl_disabled_cluster

        HelperMethods.map_put_get_and_verify(self, client.get_map("map_for_ssl_cluster").blocking())

        hazelcast.client._logger.info("Scaling up cluster from 2 node to 4")
        self.rc.scaleUpDownStandardCluster(cluster.id, 2)
        HelperMethods.map_put_get_and_verify(self, client.get_map("map_for_ssl_cluster").blocking())

        hazelcast.client._logger("Scaling down cluster from 4 node to 2")
        self.rc.scaleUpDownStandardCluster(cluster.id, -2)
        HelperMethods.map_put_get_and_verify(self, client.get_map("map_for_ssl_cluster").blocking())

        hazelcast.client._logger("Stopping cluster")
        self.rc.stopCluster(cluster.id)

        hazelcast.client._logger("Resuming cluster")
        self.rc.resumeCluster(cluster.id)

        hazelcast.client._logger("Wait 5 seconds to be sure client is connected")
        time.sleep(5)
        HelperMethods.map_put_get_and_verify(self, client.get_map("map_for_ssl_cluster").blocking())

        client.shutdown()

    @parameterized.expand([(True,), (False,)])
    def test_try_connect_ssl_cluster_without_certificates(self, is_smart):
        with self.assertRaises(IllegalStateError):
            config = HelperMethods.create_client_config(self.ssl_enabled_cluster.nameForConnect, self.ssl_enabled_cluster.token, is_smart)
            config["ssl_enabled"] = True
            config["cluster_connect_timeout"] = 10
            hazelcast.HazelcastClient(**config)

    @classmethod
    def tearDownClass(cls) -> None:
        cls.rc.deleteCluster(cls.ssl_enabled_cluster.id)
        cls.rc.deleteCluster(cls.ssl_disabled_cluster.id)
        cls.rc.exit()
