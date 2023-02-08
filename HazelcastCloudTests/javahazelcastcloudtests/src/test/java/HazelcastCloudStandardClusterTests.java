import com.hazelcast.client.HazelcastClient;
import com.hazelcast.client.config.ClientConfig;
import com.hazelcast.core.HazelcastInstance;
import com.hazelcast.map.IMap;

import com.hazelcast.remotecontroller.CloudCluster;
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
        ClientConfig config;
        if (isTlsEnabled) {
            config = HelperMethods.getConfigForSslEnabledCluster(sslEnabledCluster.getReleaseName(), sslEnabledCluster.getToken(), isSmartClient, sslEnabledCluster.getCertificatePath(), sslEnabledCluster.getTlsPassword());
            cluster = sslEnabledCluster;
        } else {
            config = HelperMethods.getConfigForSslDisabledCluster(sslDisabledCluster.getReleaseName(), sslDisabledCluster.getToken(), isSmartClient);
            cluster = sslDisabledCluster;
        }

        TestCasesLogger.info("Create client");
        HazelcastInstance client = HazelcastClient.newHazelcastClient(config);
        IMap<String, String> map = client.getMap("mapForTest");

        HelperMethods.mapPutgetAndVerify(map);
        TestCasesLogger.info("Stop cluster");
        cloudManager.stopCloudCluster(cluster.getId());
        TestCasesLogger.info("Resume cluster");
        cloudManager.resumeCloudCluster(cluster.getId());
        HelperMethods.mapPutgetAndVerify(map);
        client.shutdown();
    }


    @ParameterizedTest
    @ValueSource(booleans = {true, false})
    public void TryConnectSslClusterWithoutCertificates(Boolean isSmartClient) {
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