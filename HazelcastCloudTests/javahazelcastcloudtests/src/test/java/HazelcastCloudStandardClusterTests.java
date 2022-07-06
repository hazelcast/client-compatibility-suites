import com.hazelcast.client.HazelcastClient;
import com.hazelcast.client.config.ClientConfig;
import com.hazelcast.core.HazelcastInstance;
import com.hazelcast.map.IMap;
import org.apache.logging.log4j.LogManager;
import org.apache.logging.log4j.Logger;


import org.junit.jupiter.api.Assertions;
import org.junit.jupiter.api.BeforeAll;
import org.junit.jupiter.params.ParameterizedTest;
import org.junit.jupiter.params.provider.CsvSource;
import org.junit.jupiter.params.provider.ValueSource;

public class HazelcastCloudStandardClusterTests {
    static CloudCluster sslDisabledCluster;
    static CloudCluster sslEnabledCluster;
    static CloudCluster tempCluster;
    static HazelcastCloudManager cloudManager;
    private static final Logger TestCasesLogger = LogManager.getLogger(HazelcastCloudStandardClusterTests.class);

    @BeforeAll
    public static void setUpClass() throws CloudException {
        cloudManager = new HazelcastCloudManager();
        cloudManager.loginToHazelcastCloudUsingEnvironment();
        sslDisabledCluster = cloudManager.createHazelcastCloudStandardCluster(System.getenv("HZ_VERSION"), false);
        sslEnabledCluster = cloudManager.createHazelcastCloudStandardCluster(System.getenv("HZ_VERSION"), true);
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
        if(isTlsEnabled)
        {
            config = HelperMethods.getConfigForSslEnabledCluster(sslEnabledCluster.getNameForConnect(), sslEnabledCluster.getToken(), isSmartClient, sslEnabledCluster.getCertificatePath(), sslEnabledCluster.getTlsPassword());
            tempCluster = sslEnabledCluster;
            config.getConnectionStrategyConfig().getConnectionRetryConfig().setClusterConnectTimeoutMillis(10000);
        }
        else
        {
            config = HelperMethods.getConfigForSslDisabledCluster(sslDisabledCluster.getNameForConnect(), sslDisabledCluster.getToken(), isSmartClient);
            tempCluster = sslDisabledCluster;
            config.getConnectionStrategyConfig().getConnectionRetryConfig().setClusterConnectTimeoutMillis(10000);
        }

        TestCasesLogger.info("Create client");
        HazelcastInstance client = HazelcastClient.newHazelcastClient(config);
        IMap<String, String> map = client.getMap("mapForTest");
        HelperMethods.mapPutgetAndVerify(map);

        TestCasesLogger.info("Scale up cluster from 2 node to 4");
        cloudManager.setHazelcastCloudClusterMemberCount(tempCluster.getId(), 4);
        HelperMethods.mapPutgetAndVerify(map);

        TestCasesLogger.info("Scale down cluster from 4 node to 2");
        cloudManager.setHazelcastCloudClusterMemberCount(tempCluster.getId(), 2);
        HelperMethods.mapPutgetAndVerify(map);
        TestCasesLogger.info("Stop cluster");
        CloudCluster test = cloudManager.stopHazelcastCloudCluster(tempCluster.getId());
        TestCasesLogger.info("Resume cluster");
        cloudManager.resumeHazelcastCloudCluster(tempCluster.getId());
        HelperMethods.mapPutgetAndVerify(map);
    }


    @ParameterizedTest
    @ValueSource(booleans = {true, false})
    public void TryConnectSslClusterWithoutCertificates(Boolean isSmartClient)
    {
        ClientConfig config = HelperMethods.getConfigForSslDisabledCluster(sslEnabledCluster.getNameForConnect(), sslEnabledCluster.getToken(), isSmartClient);
        config.getConnectionStrategyConfig().getConnectionRetryConfig().setClusterConnectTimeoutMillis(10000);
        boolean value = false;
        try
        {
            HazelcastClient.newHazelcastClient(config);
        }
        catch(Exception e)
        {
            value = true;
        }
        Assertions.assertTrue(value, "Client shouldn't be able to connect ssl cluster without certificates");
    }
}