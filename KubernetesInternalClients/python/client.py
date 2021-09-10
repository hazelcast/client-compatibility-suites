import hazelcast
import logging
import random
import time

if __name__ == "__main__":
    logging.basicConfig(level=logging.INFO)

    client = hazelcast.HazelcastClient(
        cluster_members=["hz-hazelcast"],
    )

    my_map = client.get_map("map").blocking()
    my_map.put("key", "value")

    if my_map.get("key") == "value":
        print("Successful connection!")
        print("Starting to fill the map with random entries.")
        
        while True:
            random_key = random.randint(1, 100000)
            try:
                my_map.put("key" + str(random_key), "value" + str(random_key))
            except:
                logging.exception("Put operation failed!")

            if random_key % 100 == 0:
                map_size = my_map.size()
                print(f"Current map size: {map_size}")
                time.sleep(1)
    else:
        client.shutdown()
        raise Exception("Connection failed, check your configuration.")

