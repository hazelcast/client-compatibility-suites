import com.hazelcast.client.HazelcastClient;
import com.hazelcast.client.config.ClientConfig;
import com.hazelcast.core.HazelcastInstance;
import com.hazelcast.map.IMap;

import com.hazelcast.remotecontroller.CloudCluster;
import com.hazelcast.remotecontroller.CloudManager;
import com.hazelcast.remotecontroller.CloudException;
import org.apache.logging.log4j.LogManager;
import org.apache.logging.log4j.Logger;
import org.junit.jupiter.api.AfterAll;
import org.junit.jupiter.api.Assertions;
import org.junit.jupiter.api.BeforeAll;
import org.junit.jupiter.params.ParameterizedTest;
import org.junit.jupiter.params.provider.CsvSource;
import org.junit.jupiter.params.provider.ValueSource;

public class HazelcastCloudStandardClusterTests {
    static CloudCluster sslDisabledCluster;
    static CloudCluster sslEnabledCluster;
    static CloudCluster cluster;
    static CloudManager cloudManager;
    private static final Logger TestCasesLogger = LogManager.getLogger(HazelcastCloudStandardClusterTests.class);

    @BeforeAll
    public static void setUpClass() throws CloudException {
        cloudManager = new CloudManager();
        cloudManager.loginToCloudUsingEnvironment();
        sslDisabledCluster = cloudManager.createCloudCluster(System.getenv("HZ_VERSION"), false);
        sslEnabledCluster = cloudManager.createCloudCluster(System.getenv("HZ_VERSION"), true);
    }

    @AfterAll
    public static void tearDownClass() throws CloudException {
        cloudManager.deleteCloudCluster(sslDisabledCluster.id);
        cloudManager.deleteCloudCluster(sslEnabledCluster.id);
    }

    @ParameterizedTest
    @CsvSource({
            "true,true",
            "true,false",
            "false,true",
            "false,false"
    })
    public void StandardClusterTests(Boolean isSmartClient, Boolean isTlsEnabled) throws CloudException {
        TestCasesLogger.info(String.format("StandardClusterTests started: isSmartClient: %s, isTlsEnabled: %s", isSmartClient, isTlsEnabled));
        ClientConfig config;
        if (isTlsEnabled) {
            config = HelperMethods.getConfigForSslEnabledCluster(sslEnabledCluster.getReleaseName(), sslEnabledCluster.getToken(), isSmartClient, sslEnabledCluster.getCertificatePath(), sslEnabledCluster.getTlsPassword());
            cluster = sslEnabledCluster;
        } else {
            config = HelperMethods.getConfigForSslDisabledCluster(sslDisabledCluster.getReleaseName(), sslDisabledCluster.getToken(), isSmartClient);
            cluster = sslDisabledCluster;
        }

        TestCasesLogger.info("Creating client");
        HazelcastInstance client = HazelcastClient.newHazelcastClient(config);
        IMap<String, String> map = client.getMap("mapForTest");

        HelperMethods.mapPutgetAndVerify(map);
        TestCasesLogger.info("Stopping cluster");
        cloudManager.stopCloudCluster(cluster.getId());
        TestCasesLogger.info("Stopped cluster");
        TestCasesLogger.info("Resuming cluster");
        cloudManager.resumeCloudCluster(cluster.getId());
        TestCasesLogger.info("Resumed cluster");
        TestCasesLogger.info("Map put get and verify starting");
        HelperMethods.mapPutgetAndVerify(map);
        TestCasesLogger.info("Map put get and verify done");
        client.shutdown();
    }


    @ParameterizedTest
    @ValueSource(booleans = {true, false})
    public void TryConnectSslClusterWithoutCertificates(Boolean isSmartClient) {
        TestCasesLogger.info(String.format("TryConnectSslClusterWithoutCertificates started: isSmartClient: %s", isSmartClient));
        ClientConfig config = HelperMethods.getConfigForSslDisabledCluster(sslEnabledCluster.getReleaseName(), sslEnabledCluster.getToken(), isSmartClient);
        config.getConnectionStrategyConfig().getConnectionRetryConfig().setClusterConnectTimeoutMillis(10000);
        boolean value = false;
        HazelcastInstance client = null;
        try {
            client = HazelcastClient.newHazelcastClient(config);
        } catch(Exception e) {
            value = true;
        } finally {
            if (client != null) {
                client.shutdown();
            }
        }
        Assertions.assertTrue(value, "Client shouldn't be able to connect ssl cluster without certificates");
    }
}