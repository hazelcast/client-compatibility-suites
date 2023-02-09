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
    cluster: CloudCluster = None
    rc: HzRemoteController = None
    HazelcastCloudDiscovery._CLOUD_URL_BASE = os.getenv('BASE_URL').replace("https://", "")
    logger = logging.getLogger("test")

    @classmethod
    def setUpClass(cls) -> None:
        logger = logging.getLogger("hazelcast")
        logger.setLevel(logging.ERROR)
        cls.rc = HzRemoteController("127.0.0.1", 9701)
        cls.rc.loginToCloudUsingEnvironment()


    def tearDown(self) -> None:
        self.rc.deleteCloudCluster(self.cluster.id)


    def logCloudCluster(self, cluster: CloudCluster):
        self.logger.error("Cluster id: " + str(cluster.id))
        self.logger.error("Cluster name: " + str(cluster.name))
        self.logger.error("Cluster release name: " + str(cluster.releaseName))
        self.logger.error("Cluster cloud token: " + str(cluster.token))
        self.logger.error("Cluster cloud certificate path: " + str(cluster.certificatePath))
        self.logger.error("Cluster cloud tls password: " + str(cluster.tlsPassword))
        self.logger.error("Cluster cloud status: " + str(cluster.state))
        self.logger.error("Cluster cloud version: " + str(cluster.hazelcastVersion))
        self.logger.error("Cluster cloud tls enabled: " + str(cluster.isTlsEnabled))

    
    @parameterized.expand([(True, True), (False, True), (True, False), (False, False)])
    def test_cloud(self, is_smart, is_ssl_enabled):
        if is_ssl_enabled:
            self.logger.error("Create ssl enabled client config for smart routing " + str(is_smart))
            self.cluster = self.rc.createCloudCluster(os.getenv('HZ_VERSION'), True)
            client = hazelcast.HazelcastClient(
                **HelperMethods.create_client_config_with_ssl(self.cluster.releaseName,
                                                              self.cluster.token, is_smart,
                                                              self.cluster.certificatePath,
                                                              self.cluster.tlsPassword))
            
        else:
            self.logger.error("Create ssl disabled client config for smart routing " + str(is_smart))
            self.cluster = self.rc.createCloudCluster(os.getenv('HZ_VERSION'), False)
            client = hazelcast.HazelcastClient(
                **HelperMethods.create_client_config(self.cluster.releaseName,
                                                     self.cluster.token, is_smart))

        HelperMethods.map_put_get_and_verify(self, client.get_map("map_for_test_cloud").blocking())

        self.logger.error("Stopping cluster")
        cluster = self.rc.stopCloudCluster(self.cluster.id)
        self.logCloudCluster(cluster)

        self.logger.error("Resuming cluster")
        cluster = self.rc.resumeCloudCluster(self.cluster.id)
        self.logCloudCluster(cluster)

        self.logger.error("Wait 5 seconds to be sure client is connected")
        time.sleep(5)
        HelperMethods.map_put_get_and_verify(self, client.get_map("map_for_test_cloud").blocking())

        client.shutdown()

    @parameterized.expand([(True,), (False,)])
    def test_try_connect_ssl_cluster_without_certificates(self, is_smart):
        self.cluster = self.rc.createCloudCluster(os.getenv('HZ_VERSION'), True)
        with self.assertRaises(IllegalStateError):
            config = HelperMethods.create_client_config(self.cluster.releaseName,
                                                        self.cluster.token, is_smart)
            config["ssl_enabled"] = True
            config["cluster_connect_timeout"] = 10
            hazelcast.HazelcastClient(**config)

    @classmethod
    def tearDownClass(cls) -> None:
        cls.rc.exit()
