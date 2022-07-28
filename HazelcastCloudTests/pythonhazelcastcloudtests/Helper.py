import os
import random
import logging
import unittest
from os.path import abspath


class HelperMethods:

    @staticmethod
    def create_client_config(name_for_connect, token, is_smart_client):
        config = {
            "cluster_name": name_for_connect,
            "cloud_discovery_token": token,
            "statistics_enabled": True,
            "smart_routing": is_smart_client
        }
        return config

    @staticmethod
    def create_client_config_with_ssl(name_for_connect, token, is_smart_client, certificates_path, tls_password):
        config = HelperMethods.create_client_config(name_for_connect, token, is_smart_client)
        config["ssl_cafile"] = abspath(os.path.join(certificates_path + "ca.pem"))
        config["ssl_certfile"] = abspath(os.path.join(certificates_path + "cert.pem"))
        config["ssl_keyfile"] = abspath(os.path.join(certificates_path + "key.pem"))
        config["ssl_password"] = tls_password
        config["ssl_enabled"] = True
        return config

    @staticmethod
    def map_put_get_and_verify(test_instance, test_map):
        print("Put get to map and verify")
        test_map.clear()
        while test_map.size() < 20:
            random_key = random.randint(1, 100000)
            try:
                test_map.put("key" + str(random_key), "value" + str(random_key))
            except:
                logging.exception("Put operation failed!")
        test_instance.assertEqual(test_map.size(), 20, "Map size should be 20")
