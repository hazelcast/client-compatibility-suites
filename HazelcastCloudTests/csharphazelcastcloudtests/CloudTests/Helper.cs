// Copyright (c) 2008-2021, Hazelcast, Inc. All Rights Reserved.
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.IO;
using System.Security.Authentication;
using System.Threading.Tasks;
using Hazelcast;
using Hazelcast.DistributedObjects;
using Hazelcast.Networking;
using Hazelcast.Testing.Logging;
using Microsoft.Extensions.Logging;
using NUnit.Framework;

namespace CloudTests
{
    public class Helper
    {
        public static HazelcastOptions CreateClientConfigWithSsl(string nameForConnect, string token, bool isSmartClient, string certificatesPath, string tlsPassword)
        {
            var options = CreateClientConfigWithoutSsl(nameForConnect, token, isSmartClient);
            options.Networking.Ssl.Enabled = true;
            options.Networking.Ssl.ValidateCertificateChain = false;
            options.Networking.Ssl.Protocol = SslProtocols.Tls12;
            options.Networking.Ssl.CertificatePath = Path.GetFullPath(certificatesPath + "client.pfx");
            options.Networking.Ssl.CertificatePassword = tlsPassword;
            return options;
        }

        public static HazelcastOptions CreateClientConfigWithoutSsl(string nameForConnect, string token, bool isSmartClient)
        {
            var options = new HazelcastOptionsBuilder().WithHConsoleLogger()
                .With("Logging:LogLevel:Hazelcast", "WARNING")
                .Build();
            options.ClusterName = nameForConnect;
            options.Networking.Cloud.DiscoveryToken = token;
            options.Networking.ReconnectMode = ReconnectMode.ReconnectAsync;
            options.LoggerFactory.Creator = () => LoggerFactory.Create(logBuilder => logBuilder.AddConsole());

            options.Networking.Cloud.Url = new Uri(Environment.GetEnvironmentVariable("baseUrl") ?? throw new InvalidOperationException("baseUrl is not set as an env variable"));
            options.Metrics.Enabled = true;
            options.Networking.SmartRouting = isSmartClient;

            return options;

        }

        public static async Task MapPutGetAndVerify(IHMap<string, string> map)
        {
            await map.ClearAsync();
            var random = new Random();
            for (var i = 0; i < 20; i++)
            {
                var randomValue = random.Next(100_000);
                await map.PutAsync("key_" + randomValue, "value_" + randomValue);
                Assert.AreEqual("value_" + randomValue, await map.GetAsync("key_" + randomValue)) ;
            }
            Assert.AreEqual(await map.GetSizeAsync(), 20, "Size should be 20");
        }
    }
}
