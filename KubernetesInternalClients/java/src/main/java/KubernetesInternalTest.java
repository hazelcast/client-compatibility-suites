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
    private static final Logger LOGGER;

    static {
        // The following makes jdk logging appear in single lines instead of 2 lines:
        // https://stackoverflow.com/a/10706033
        System.setProperty("java.util.logging.SimpleFormatter.format",
                "%1$tY-%1$tm-%1$td %1$tH:%1$tM:%1$tS %4$s %2$s %5$s%6$s%n");
        LOGGER = Logger.getLogger(KubernetesInternalTest.class.getName());
    }


    public static void main( String[] args ) {
        String internalIp = "hz-hazelcast";

        ClientConfig clientConfig = new ClientConfig();
        clientConfig.getNetworkConfig().addAddress(internalIp);
        clientConfig.setProperty( "hazelcast.logging.type", "log4j2" );

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
