import com.hazelcast.client.config.ClientConfig;
import com.hazelcast.config.SSLConfig;
import com.hazelcast.map.IMap;
import org.apache.logging.log4j.LogManager;
import org.apache.logging.log4j.Logger;
import org.junit.jupiter.api.Assertions;

import java.nio.file.Paths;
import java.util.Properties;
import java.util.Random;

public class HelperMethods {
    private static final Logger HelperLogger = LogManager.getLogger(HelperMethods.class);

    public static ClientConfig getConfigForSslEnabledCluster(String nameForConnect, String token, boolean isSmart, String certificatePath, String tlsPassword) {
        Properties props = new Properties();
        props.setProperty("javax.net.ssl.keyStore", String.valueOf(Paths.get(certificatePath, "client.keystore")));
        props.setProperty("javax.net.ssl.keyStorePassword", tlsPassword);
        props.setProperty("javax.net.ssl.trustStore", String.valueOf(Paths.get(certificatePath, "client.truststore")));
        props.setProperty("javax.net.ssl.trustStorePassword", tlsPassword);
        ClientConfig config = getConfigForSslDisabledCluster(nameForConnect, token, isSmart);
        config.getNetworkConfig().setSSLConfig(new SSLConfig().setEnabled(true).setProperties(props));
        return config;
    }

    public static ClientConfig getConfigForSslDisabledCluster(String nameForConnect, String token, boolean isSmart) {
        ClientConfig config = new ClientConfig();
        config.getNetworkConfig().getCloudConfig().setDiscoveryToken(token).setEnabled(true);
        config.setProperty("hazelcast.client.cloud.url", System.getenv("BASE_URL"));
        config.setClusterName(nameForConnect);
        config.getNetworkConfig().setSmartRouting(isSmart);
        return config;
    }

    public static void mapPutgetAndVerify(IMap<String, String> map){
        Random random = new Random();
        map.clear();
        for(int i = 0; i < 20; i++)
        {
            int randomKey = random.nextInt(100_000);
            map.put("key-" + randomKey, "value-" + randomKey);
            Assertions.assertEquals(map.get("key-" + randomKey), "value-" + randomKey );
        }
        Assertions.assertEquals(map.size(), 20);
    }

}