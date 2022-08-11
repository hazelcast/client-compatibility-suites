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
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Core;
using Thrift.Protocol;

namespace Hazelcast.Testing.Remote
{
    /// <summary>
    /// Represents a remote controller client.
    /// </summary>
    public class RemoteControllerClient : RemoteController.Client, IRemoteControllerClient
    {
        private readonly SemaphoreSlim _lock = new SemaphoreSlim(1);

        /// <summary>
        /// Initializes a new instance of the <see cref="RemoteControllerClient"/> class.
        /// </summary>
        /// <param name="protocol">The protocol.</param>
        private RemoteControllerClient(TProtocol protocol)
            : base(protocol)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="RemoteControllerClient"/> class.
        /// </summary>
        /// <param name="inputProtocol">The input protocol.</param>
        /// <param name="outputProtocol">The output protocol.</param>
        private RemoteControllerClient(TProtocol inputProtocol, TProtocol outputProtocol)
            : base(inputProtocol, outputProtocol)
        { }

        /// <summary>
        /// Creates and connects a new remote controller client.
        /// </summary>
        /// <param name="rcHostAddress">The remote controller address.</param>
        /// <param name="port">The remote controller port.</param>
        /// <returns>A new remote controller client.</returns>
        public static Task<IRemoteControllerClient> CreateAsync(string rcHostAddress = "127.0.0.1", int port = 9701)
            => CreateAsync(IPAddress.Parse(rcHostAddress), port);

        /// <summary>
        /// Creates and connects a new remote controller client.
        /// </summary>
        /// <param name="rcHostAddress">The remote controller address.</param>
        /// <param name="port">The remote controller port.</param>
        /// <returns>A new remote controller client.</returns>
        public static async Task<IRemoteControllerClient> CreateAsync(IPAddress rcHostAddress, int port = 9701)
        {
            var configuration = new Thrift.TConfiguration();
            var tSocketTransport = new Thrift.Transport.Client.TSocketTransport(rcHostAddress, port, configuration);
            var transport = new Thrift.Transport.TFramedTransport(tSocketTransport);
            if (!transport.IsOpen)
            {
                await transport.OpenAsync(CancellationToken.None).CfAwait();
            }
            var protocol = new TBinaryProtocol(transport);
            return Create(protocol);
        }

        /// <summary>
        /// Creates a new remote controller client.
        /// </summary>
        /// <param name="protocol">The protocol.</param>
        /// <returns>A new remote controller client.</returns>
        public static IRemoteControllerClient Create(TProtocol protocol)
            => new RemoteControllerClient(protocol);

        /// <summary>
        /// Creates a new remote controller client.
        /// </summary>
        /// <param name="inputProtocol">The input protocol.</param>
        /// <param name="outputProtocol">The output protocol.</param>
        /// <returns>A new remote controller client.</returns>
        public static IRemoteControllerClient Create(TProtocol inputProtocol, TProtocol outputProtocol)
            => new RemoteControllerClient(inputProtocol, outputProtocol);

        private async Task<T> WithLock<T>(Func<CancellationToken, Task<T>> action, CancellationToken cancellationToken)
        {
            await _lock.WaitAsync(cancellationToken).CfAwait();
            try
            {
                if (!cancellationToken.IsCancellationRequested)
                    return await action(cancellationToken).CfAwait();
                else
                    return default;
            }
            finally
            {
                _lock.Release();
            }
        }

        private async Task WithLock(Func<CancellationToken, Task> action, CancellationToken cancellationToken)
        {
            await WithLock<object>(async token => { await action(token); return default; }, cancellationToken);
        }


        /// <inheritdoc />
        public Task<bool> PingAsync(CancellationToken cancellationToken = default)
            => WithLock(ping, cancellationToken);

        /// <inheritdoc />
        public Task<bool> CleanAsync(CancellationToken cancellationToken = default)
            => WithLock(clean, cancellationToken);

        /// <inheritdoc />
        public async Task<bool> ExitAsync(CancellationToken cancellationToken = default)
        {
            var result = await WithLock(exit, cancellationToken).CfAwait();
            InputProtocol?.Transport?.Close();
            return result;
        }

        /// <inheritdoc />
        public Task<Cluster> CreateClusterAsync(string hzVersion, string xmlconfig, CancellationToken cancellationToken = default)
            => WithLock(token => createCluster(hzVersion, xmlconfig, token), cancellationToken);

        /// <inheritdoc />
        public Task<Member> StartMemberAsync(string clusterId, CancellationToken cancellationToken = default)
            => WithLock(token => startMember(clusterId, token), cancellationToken);

        /// <inheritdoc />
        public Task<bool> ShutdownMemberAsync(string clusterId, string memberId, CancellationToken cancellationToken = default)
            => WithLock(token => shutdownMember(clusterId, memberId, token), cancellationToken);

        /// <inheritdoc />
        public Task<bool> TerminateMemberAsync(string clusterId, string memberId, CancellationToken cancellationToken = default)
            => WithLock(token => terminateMember(clusterId, memberId, token), cancellationToken);

        /// <inheritdoc />
        public Task<bool> SuspendMemberAsync(string clusterId, string memberId, CancellationToken cancellationToken = default)
            => WithLock(token => suspendMember(clusterId, memberId, token), cancellationToken);

        /// <inheritdoc />
        public Task<bool> ResumeMemberAsync(string clusterId, string memberId, CancellationToken cancellationToken = default)
            => WithLock(token => resumeMember(clusterId, memberId, token), cancellationToken);

        /// <inheritdoc />
        public Task<bool> ShutdownClusterAsync(string clusterId, CancellationToken cancellationToken = default)
            => WithLock(token => shutdownCluster(clusterId, token), cancellationToken);

        /// <inheritdoc />
        public Task<bool> TerminateClusterAsync(string clusterId, CancellationToken cancellationToken = default)
            => WithLock(token => terminateCluster(clusterId, token), cancellationToken);

        /// <inheritdoc />
        public Task<Cluster> SplitMemberFromClusterAsync(string memberId, CancellationToken cancellationToken = default)
            => WithLock(token => splitMemberFromCluster(memberId, token), cancellationToken);

        /// <inheritdoc />
        public Task<Cluster> MergeMemberToClusterAsync(string clusterId, string memberId, CancellationToken cancellationToken = default)
            => WithLock(token => mergeMemberToCluster(clusterId, memberId, token), cancellationToken);

        /// <inheritdoc />
        public Task<Response> ExecuteOnControllerAsync(string clusterId, string script, Lang lang, CancellationToken cancellationToken = default)
            => WithLock(token => executeOnController(clusterId, script, lang, token), cancellationToken);

        public Task LoginToHazelcastCloud(string uri, string apiKey, string apiSecret, CancellationToken cancellationToken)
            => WithLock(token => loginToHazelcastCloud(uri, apiKey, apiSecret, token), cancellationToken);

        public Task<CloudCluster> CreateHazelcastCloudStandardCluster(string hzVersion, bool isTlsEnabled, CancellationToken cancellationToken = default)
            => WithLock(token => createHazelcastCloudStandardCluster(hzVersion, isTlsEnabled, token), cancellationToken);

        /// <inheritdoc />
        public Task LoginToHazelcastCloudUsingEnvironment(CancellationToken token = default) => loginToHazelcastCloudUsingEnvironment(token);

        /// <inheritdoc />
        public Task SetHazelcastCloudClusterMemberCount(string cloudClusterId, int totalMemberCount, CancellationToken cancellationToken = default)
            => WithLock(token => setHazelcastCloudClusterMemberCount(cloudClusterId, totalMemberCount, token), cancellationToken);

        /// <inheritdoc />
        public Task<CloudCluster> GetHazelcastCloudCluster(string cloudClusterId, CancellationToken cancellationToken = default)
        => WithLock<CloudCluster>(token => getHazelcastCloudCluster(cloudClusterId, token), cancellationToken);

        /// <inheritdoc />
        public Task<CloudCluster> StopHazelcastCloudCluster(string cloudClusterId, CancellationToken cancellationToken = default)
        => WithLock<CloudCluster>(token => stopHazelcastCloudCluster(cloudClusterId, token), cancellationToken);

        /// <inheritdoc />
        public Task<CloudCluster> ResumeHazelcastCloudCluster(string cloudClusterId, CancellationToken cancellationToken = default)
        => WithLock<CloudCluster>(token => resumeHazelcastCloudCluster(cloudClusterId, token), cancellationToken);

        /// <inheritdoc />
        public Task DeleteHazelcastCloudCluster(string cloudClusterId, CancellationToken cancellationToken = default)
        => WithLock(token => deleteHazelcastCloudCluster(cloudClusterId, token), cancellationToken);
    }
}
