using System;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast;
using Hazelcast.Testing.Remote;
using NUnit.Framework;
using Microsoft.Extensions.Logging;

namespace CloudTests
{
    [TestFixture]
    public class StandardClusterTests : CloudRemoteTestBase
    {
        private CloudCluster _sslDisabledCluster;
        private CloudCluster _sslEnabledCluster;
        private CloudCluster _tmpClusterObject;

        [OneTimeSetUp]
        public async Task CreateStandardCluster()
        {
            string hzVersion = "";
            if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("hzVersion")))
                hzVersion = Environment.GetEnvironmentVariable("hzVersion");

            await RcClient.LoginToHazelcastCloudUsingEnvironment();

            _sslEnabledCluster = await RcClient.CreateHazelcastCloudStandardCluster(hzVersion, true);
            _sslDisabledCluster = await RcClient.CreateHazelcastCloudStandardCluster(hzVersion, false);
        }

        [OneTimeTearDown]
        public async Task DeleteClusters()
        {
            if (!string.IsNullOrEmpty(_sslEnabledCluster?.Id))
                await RcClient.DeleteHazelcastCloudCluster(_sslEnabledCluster.Id);

            if (!string.IsNullOrEmpty(_sslDisabledCluster?.Id))
                await RcClient.DeleteHazelcastCloudCluster(_sslDisabledCluster.Id);
        }

        [TestCase(true, true)]
        [TestCase(true, false)]
        [TestCase(false, true)]
        [TestCase(false, false)]
        [Timeout(300_000)]
        public async Task TestCloudConnection(bool isSmart, bool isSslEnabled)
        {
            HazelcastOptions options;
            if (isSslEnabled)
            {
                options = Helper.CreateClientConfigWithSsl(_sslEnabledCluster.NameForConnect, _sslEnabledCluster.Token, isSmart,
                    _sslEnabledCluster.CertificatePath, _sslEnabledCluster.TlsPassword);
                _tmpClusterObject = _sslEnabledCluster;
            }
            else
            {
                options = Helper.CreateClientConfigWithoutSsl(_sslDisabledCluster.NameForConnect, _sslDisabledCluster.Token,
                    isSmart);
                _tmpClusterObject = _sslDisabledCluster;
            }

            var logger = options.LoggerFactory.Service.CreateLogger("test");

            logger.LogInformation("Create client");
            await using var client = await HazelcastClientFactory.StartNewClientAsync(options);
            await using var map = await client.GetMapAsync<string, string>("MapForTest");
            await Helper.MapPutGetAndVerify(map, logger);

            logger.LogInformation("Scale up cluster from 2 node to 4");
            await RcClient.SetHazelcastCloudClusterMemberCount(_tmpClusterObject.Id, 4);
            await Helper.MapPutGetAndVerify(map, logger);

            logger.LogInformation("Scale down cluster from 4 node to 2");
            await RcClient.SetHazelcastCloudClusterMemberCount(_tmpClusterObject.Id, 2);
            await Helper.MapPutGetAndVerify(map, logger);

            logger.LogInformation("Stop cluster");
            await RcClient.StopHazelcastCloudCluster(_tmpClusterObject.Id);

            logger.LogInformation("Resume cluster");
            await RcClient.ResumeHazelcastCloudCluster(_tmpClusterObject.Id);

            logger.LogInformation("Wait 5 seconds to be sure client is connected again");
            await Task.Delay(5_000);

            await Helper.MapPutGetAndVerify(map, logger);
            await client.DisposeAsync();
        }

        [TestCase(true)]
        [TestCase(false)]
        [Timeout(30_000)]
        public void TryConnectSslClusterWithoutCertificates(bool isSmart)
        {
            var options = Helper.CreateClientConfigWithoutSsl(_sslEnabledCluster.NameForConnect,
                _sslEnabledCluster.Token, isSmart);
            options.Networking.Ssl.Enabled = true;
            options.Networking.ConnectionRetry.ClusterConnectionTimeoutMilliseconds = 10000;
            var logger = options.LoggerFactory.Service.CreateLogger("test");

            logger.LogInformation("Try connecting ssl cluster without certificates");
            Assert.ThrowsAsync<Exception>(async () => await HazelcastClientFactory.StartNewClientAsync(options), "Client shouldn't be able to connect ssl cluster without certificates");

        }
    }
}
