import com.hazelcast.client.HazelcastClient;
import com.hazelcast.client.config.ClientConfig;
import com.hazelcast.core.HazelcastInstance;
import com.hazelcast.map.IMap;

import java.util.Random;
import java.util.concurrent.ThreadLocalRandom;
import java.util.logging.Logger;

/**
 * Kubernetes external test for thin java client
 */
public class KubernetesExternalTest {

    private static final Logger LOGGER = Logger.getLogger(KubernetesExternalTest.class.getName());

    public static void main( String[] args ) throws InterruptedException {
        String externalIp = "<EXTERNAL-IP>";

        ClientConfig clientConfig = new ClientConfig();
        clientConfig.getNetworkConfig().addAddress(externalIp);

        HazelcastInstance client = HazelcastClient.newHazelcastClient(clientConfig);
        LOGGER.info("Successful connection!");

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
    }
}
