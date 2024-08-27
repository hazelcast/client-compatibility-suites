import com.hazelcast.client.HazelcastClient;
import com.hazelcast.client.config.ClientConfig;
import com.hazelcast.core.HazelcastInstance;
import com.hazelcast.map.IMap;

import java.util.Random;
import java.util.concurrent.ThreadLocalRandom;
import java.util.logging.Logger;

/**
 * Kubernetes internal test for thin java client
 */
public class KubernetesInternalTest {

    private static final Logger LOGGER = Logger.getLogger(KubernetesInternalTest.class.getName());

    public static void main( String[] args ) {
        String internalIp = "hz-hazelcast";

        ClientConfig clientConfig = new ClientConfig();
        clientConfig.getNetworkConfig().addAddress(internalIp);

        HazelcastInstance client = HazelcastClient.newHazelcastClient(clientConfig);

        try {
            IMap<String, String> map =  client.getMap("map");
            map.put("key", "value");
            String res = map.get("key");
            if (!res.equals("value")) {
                LOGGER.warning("Connection failed, check your configuration.");
            }
            LOGGER.info("Successful connection!");
            LOGGER.info("Starting to fill the map with random entries.");

            Random random = ThreadLocalRandom.current();
            while (true) {
                int randomKey = random.nextInt(100000);
                try {
                    map.put("key" + randomKey, "value" + randomKey);
                } catch (Throwable exc) {
                    LOGGER.warning("Put operation failed: " + exc);
                }
                if (randomKey % 100 == 0) {
                    int size = map.size();
                    LOGGER.info("Current map size: " + size);
                    Thread.sleep(1000);
                }
            }
        } catch (Throwable e) {
            LOGGER.severe("An error occurred, shutting down the client: " + e);
            client.shutdown();
        }

    }
}
