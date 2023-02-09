import os
import time
import unittest
import hazelcast
import logging

from parameterized import parameterized
from Helper import HelperMethods
from hazelcast.discovery import HazelcastCloudDiscovery
from hazelcast.errors import IllegalStateError
from hzrc.client import HzRemoteController
from hzrc.ttypes import CloudCluster


class StandardClusterTests(unittest.TestCase):
    ssl_enabled_cluster: CloudCluster = None
    ssl_disabled_cluster: CloudCluster = None
    rc: HzRemoteController = None
    HazelcastCloudDiscovery._CLOUD_URL_BASE = os.getenv('BASE_URL').replace("https://", "")
    logger = logging.getLogger("test")

    @classmethod
    def setUpClass(cls) -> None:
        logger = logging.getLogger("hazelcast")
        logger.setLevel(logging.ERROR)
        cls.rc = HzRemoteController("127.0.0.1", 9701)
        cls.rc.loginToCloudUsingEnvironment()


    def setUp(self) -> None:
        self.ssl_enabled_cluster = self.rc.createCloudCluster(os.getenv('HZ_VERSION'), True)
        self.ssl_disabled_cluster = self.rc.createCloudCluster(os.getenv('HZ_VERSION'), False)


    def tearDown(self) -> None:
        self.rc.deleteCloudCluster(self.ssl_enabled_cluster.id)
        self.rc.deleteCloudCluster(self.ssl_disabled_cluster.id)


    def logCloudCluster(self, cluster: CloudCluster):
        self.logger.error("Cluster id: " + cluster.id)
        self.logger.error("Cluster name: " + cluster.name)
        self.logger.error("Cluster release name: " + cluster.releaseName)
        self.logger.error("Cluster cloud token: " + cluster.token)
        self.logger.error("Cluster cloud certificate path: " + cluster.certificatePath)
        self.logger.error("Cluster cloud tls password: " + cluster.tlsPassword)
        self.logger.error("Cluster cloud status: " + cluster.state)
        self.logger.error("Cluster cloud version: " + cluster.hazelcastVersion)
        self.logger.error("Cluster cloud tls enabled: " + str(cluster.isTlsEnabled))

    
    @parameterized.expand([(True, True), (False, True), (True, False), (False, False)])
    def test_cloud(self, is_smart, is_ssl_enabled):
        if is_ssl_enabled:
            self.logger.error("Create ssl enabled client config for smart routing " + str(is_smart))
            client = hazelcast.HazelcastClient(
                **HelperMethods.create_client_config_with_ssl(self.ssl_enabled_cluster.releaseName,
                                                              self.ssl_enabled_cluster.token, is_smart,
                                                              self.ssl_enabled_cluster.certificatePath,
                                                              self.ssl_enabled_cluster.tlsPassword))
            cluster = self.ssl_enabled_cluster
        else:
            self.logger.error("Create ssl disabled client config for smart routing " + str(is_smart))
            client = hazelcast.HazelcastClient(
                **HelperMethods.create_client_config(self.ssl_disabled_cluster.releaseName,
                                                     self.ssl_disabled_cluster.token, is_smart))
            cluster = self.ssl_disabled_cluster

        HelperMethods.map_put_get_and_verify(self, client.get_map("map_for_test_cloud").blocking())

        self.logger.error("Stopping cluster")
        tmp = self.rc.stopCloudCluster(cluster.id)
        self.logCloudCluster(tmp)

        self.logger.error("Resuming cluster")
        tmp = self.rc.resumeCloudCluster(cluster.id)
        self.logCloudCluster(tmp)

        self.logger.error("Wait 5 seconds to be sure client is connected")
        time.sleep(5)
        HelperMethods.map_put_get_and_verify(self, client.get_map("map_for_test_cloud").blocking())

        client.shutdown()

    @parameterized.expand([(True,), (False,)])
    def test_try_connect_ssl_cluster_without_certificates(self, is_smart):
        with self.assertRaises(IllegalStateError):
            config = HelperMethods.create_client_config(self.ssl_enabled_cluster.releaseName,
                                                        self.ssl_enabled_cluster.token, is_smart)
            config["ssl_enabled"] = True
            config["cluster_connect_timeout"] = 10
            hazelcast.HazelcastClient(**config)

    @classmethod
    def tearDownClass(cls) -> None:
        cls.rc.exit()
