import com.hazelcast.client.HazelcastClient;
import com.hazelcast.client.config.ClientConfig;
import com.hazelcast.client.properties.ClientProperty;
import com.hazelcast.core.HazelcastInstance;
import com.hazelcast.map.IMap;
import com.hazelcast.spi.properties.HazelcastProperty;

import java.util.Random;
import java.util.concurrent.ThreadLocalRandom;
import java.util.logging.Logger;

/**
 * Kubernetes external test for thin java client
 */
public class KubernetesExternalTest {

    private static final Logger LOGGER;
    static {
        // The following makes jdk logging appear in single lines instead of 2 lines:
        // https://stackoverflow.com/a/10706033
        System.setProperty("java.util.logging.SimpleFormatter.format",
                "%1$tY-%1$tm-%1$td %1$tH:%1$tM:%1$tS %4$s %2$s %5$s%6$s%n");
        LOGGER = Logger.getLogger(KubernetesExternalTest.class.getName());
    }

    public static void main( String[] args ) {
        String externalIp = "<EXTERNAL-IP>";

        ClientConfig clientConfig = new ClientConfig();
        clientConfig.getNetworkConfig().addAddress(externalIp);
        // disable client logging for test assertion to work properly
        clientConfig.setProperty( "hazelcast.logging.type", "none" );

        HazelcastInstance client = HazelcastClient.newHazelcastClient(clientConfig);
        LOGGER.info("Successful connection!");

        try {
            IMap<String, String> map =  client.getMap("mapForJava");
            LOGGER.info("Starting to fill the map with random entries.");

            Random random = ThreadLocalRandom.current();
            for (int i = 0; i < 120; i++) {
                int randomKey = random.nextInt(100000);
                try {
                    map.put("key" + randomKey, "value" + randomKey);
                } catch (Throwable exc) {
                    LOGGER.warning("Put operation failed: " + exc);
                }
                int size = map.size();
                LOGGER.info("Current map size: " + size);
                Thread.sleep(1000);
            }
            client.shutdown();
        } catch (Throwable e) {
            LOGGER.severe("Shutting down the client, an error occurred: " + e);
            client.shutdown();
        }
    }
}
