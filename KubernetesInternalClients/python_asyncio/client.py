import logging
import random
import time
import asyncio

from hazelcast.asyncio import HazelcastClient


async def amain():
    logging.basicConfig(level=logging.INFO)

    client = await HazelcastClient.create_and_start(
        cluster_members=["hz-hazelcast"],
    )

    my_map = await client.get_map("map").blocking()
    await my_map.put("key", "value")

    if await my_map.get("key") == "value":
        print("Successful connection!")
        print("Starting to fill the map with random entries.")

        while True:
            random_key = random.randint(1, 100000)
            try:
                await my_map.put("key" + str(random_key), "value" + str(random_key))
            except:
                logging.exception("Put operation failed!")

            if random_key % 100 == 0:
                map_size = await my_map.size()
                print(f"Current map size: {map_size}")
                await asyncio.sleep(1)
    else:
        await client.shutdown()
        raise Exception("Connection failed, check your configuration.")


if __name__ == '__main__':
    asyncio.run(amain())
