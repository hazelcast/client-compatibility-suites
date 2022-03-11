using System;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast;
using Hazelcast.Testing.Remote;
using NUnit.Framework;

namespace CloudTests
{
    [TestFixture]
    public class StandardClusterTests:CloudRemoteTestBase
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
            
            _sslEnabledCluster = await RcClient.CreateHazelcastCloudStandardCluster(hzVersion, true);
            _sslDisabledCluster = await RcClient.CreateHazelcastCloudStandardCluster(hzVersion, false);
            //_sslDisabledCluster = await RcClient.GetHazelcastCloudCluster("1584");
            //_sslEnabledCluster = await RcClient.GetHazelcastCloudCluster("1565");
        }

        [OneTimeTearDown]
        public async Task DeleteClusters()
        {
            await RcClient.DeleteHazelcastCloudCluster(_sslEnabledCluster.Id);
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
            Console.WriteLine("Create client");
            await using var client = await HazelcastClientFactory.StartNewClientAsync(options);
            await using var map = await client.GetMapAsync<string, string>("MapForTest");
            await Helper.MapPutGetAndVerify(map);
            
            // Console.WriteLine("Scale up cluster from 2 node to 4");
            // await RcClient.ScaleUpDownHazelcastCloudStandardCluster(_tmpClusterObject.Id, 2);
            // await Helper.MapPutGetAndVerify(map);
            //
            // Console.WriteLine("Scale down cluster from 4 node to 2");
            // await RcClient.ScaleUpDownHazelcastCloudStandardCluster(_tmpClusterObject.Id, -2);
            // await Helper.MapPutGetAndVerify(map);
            //
            // Console.WriteLine("Stop cluster");
            // await RcClient.StopHazelcastCloudCluster(_tmpClusterObject.Id);
            //
            // Console.WriteLine("Resume cluster");
            // await RcClient.ResumeHazelcastCloudCluster(_tmpClusterObject.Id);
            //
            // Console.WriteLine("Wait 5 seconds to be sure client is connected again");
            // Thread.Sleep(5000);
            //
            // await Helper.MapPutGetAndVerify(map);
            await client.DisposeAsync();
        }
        
        [TestCase(true)]
        [TestCase(false)]
        [Timeout(30_000)]
        public async Task TryConnectSslClusterWithoutCertificates(bool isSmart)
        {
            var options = Helper.CreateClientConfigWithoutSsl(_sslEnabledCluster.NameForConnect,
                _sslEnabledCluster.Token, isSmart);
            options.Networking.Ssl.Enabled = true;
            options.Networking.ConnectionRetry.ClusterConnectionTimeoutMilliseconds = 10000;
            bool value = false;
            try
            {
                Console.WriteLine("Try connecting ssl cluster without certificates");
                await using var client = await HazelcastClientFactory.StartNewClientAsync(options);
            }
            catch (Exception e)
            {
                value = true;
            }
            Assert.IsTrue(value, "Client shouldn't be able to connect ssl cluster without certificates");
        }
    }
}
