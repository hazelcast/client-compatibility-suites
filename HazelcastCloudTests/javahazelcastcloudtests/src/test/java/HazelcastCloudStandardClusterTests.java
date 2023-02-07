import com.hazelcast.client.HazelcastClient;
import com.hazelcast.client.config.ClientConfig;
import com.hazelcast.core.HazelcastInstance;
import com.hazelcast.map.IMap;

import com.hazelcast.remotecontroller.CloudManager;
import com.hazelcast.remotecontroller.CloudCluster;
import com.hazelcast.remotecontroller.CloudException;
import org.apache.logging.log4j.LogManager;
import org.apache.logging.log4j.Logger;
import org.junit.jupiter.api.AfterAll;
import org.junit.jupiter.api.AfterEach;
import org.junit.jupiter.api.Assertions;
import org.junit.jupiter.api.BeforeAll;
import org.junit.jupiter.params.ParameterizedTest;
import org.junit.jupiter.params.provider.CsvSource;
import org.junit.jupiter.params.provider.ValueSource;

public class HazelcastCloudStandardClusterTests {
    static CloudCluster sslDisabledCluster;
    static CloudCluster sslEnabledCluster;
    static CloudCluster tempCluster;
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
        if(isTlsEnabled) {
            config = HelperMethods.getConfigForSslEnabledCluster(sslEnabledCluster.getReleaseName(), sslEnabledCluster.getToken(), isSmartClient, sslEnabledCluster.getCertificatePath(), sslEnabledCluster.getTlsPassword());
            tempCluster = sslEnabledCluster;
            config.getConnectionStrategyConfig().getConnectionRetryConfig().setClusterConnectTimeoutMillis(10000);
        } else {
            config = HelperMethods.getConfigForSslDisabledCluster(sslDisabledCluster.getReleaseName(), sslDisabledCluster.getToken(), isSmartClient);
            tempCluster = sslDisabledCluster;
            config.getConnectionStrategyConfig().getConnectionRetryConfig().setClusterConnectTimeoutMillis(10000);
        }

        TestCasesLogger.info("Create client");
        HazelcastInstance client = HazelcastClient.newHazelcastClient(config);
        IMap<String, String> map = client.getMap("mapForTest");
        HelperMethods.mapPutgetAndVerify(map);

        HelperMethods.mapPutgetAndVerify(map);
        TestCasesLogger.info("Stop cluster");
        cloudManager.stopCloudCluster(tempCluster.getId());
        TestCasesLogger.info("Resume cluster");
        cloudManager.resumeCloudCluster(tempCluster.getId());
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