
import asyncio
import logging
import random

from hazelcast.asyncio import HazelcastClient

async def amain():
    logging.basicConfig(level=logging.INFO)

    client = await HazelcastClient.create_and_start(
        cluster_members=["<EXTERNAL-IP>"],
        use_public_ip=True,
    )

    my_map = await client.get_map("map_for_python")
    await my_map.put("key", "value")

    if await my_map.get("key") == "value":
        print("Successful connection!")
        print("Starting to fill the map with random entries.")

        for _ in range(120):
            random_key = random.randint(1, 100000)
            try:
                await my_map.put(f"key-{random_key}", f"value-{random_key}")
            except Exception:
                logging.exception("Put operation failed!")

            map_size = await my_map.size()
            print(f"Current map size: {map_size}")
    else:
        raise Exception("Connection failed, check your configuration.")

    await client.shutdown()


if __name__ == "__main__":
    asyncio.run(amain())