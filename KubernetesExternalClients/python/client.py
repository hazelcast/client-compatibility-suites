import hazelcast
import logging
import random

if __name__ == "__main__":
    logging.basicConfig(level=logging.INFO)

    client = hazelcast.HazelcastClient(
        cluster_members=["<EXTERNAL-IP>"],
        use_public_ip=True,
    )

    my_map = client.get_map("map_for_python").blocking()
    my_map.put("key", "value")

    if my_map.get("key") == "value":
        print("Successful connection!")
        print("Starting to fill the map with random entries.")

        for _ in range(120):
            random_key = random.randint(1, 100000)
            try:
                my_map.put(f"key-{random_key}", f"value-{random_key}")
            except:
                logging.exception("Put operation failed!")

            map_size = my_map.size()
            print(f"Current map size: {map_size}")
    else:
        raise Exception("Connection failed, check your configuration.")

    client.shutdown()
