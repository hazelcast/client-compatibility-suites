// Copyright (c) 2008-2022, Hazelcast, Inc. All Rights Reserved.
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
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Thrift;
using Thrift.Collections;

using Thrift.Protocol;
using Thrift.Protocol.Entities;
using Thrift.Protocol.Utilities;
using Thrift.Transport;
using Thrift.Transport.Client;
using Thrift.Transport.Server;
using Thrift.Processor;

#pragma warning disable IDE0079  // remove unnecessary pragmas
#pragma warning disable IDE1006  // parts of the code use IDL spelling
#pragma warning disable IDE0083  // pattern matching "that is not SomeType" requires net5.0 but we still support earlier versions

namespace Hazelcast.Testing.Remote
{
  public partial class RemoteController
  {
    public interface IAsync
    {
      Task<bool> pingAsync(CancellationToken cancellationToken = default(CancellationToken));

      Task<bool> cleanAsync(CancellationToken cancellationToken = default(CancellationToken));

      Task<bool> exitAsync(CancellationToken cancellationToken = default(CancellationToken));

      Task<Cluster> createClusterAsync(string hzVersion, string xmlconfig, CancellationToken cancellationToken = default(CancellationToken));

      Task<Cluster> createClusterKeepClusterNameAsync(string hzVersion, string xmlconfig, CancellationToken cancellationToken = default(CancellationToken));

      Task<Member> startMemberAsync(string clusterId, CancellationToken cancellationToken = default(CancellationToken));

      Task<bool> shutdownMemberAsync(string clusterId, string memberId, CancellationToken cancellationToken = default(CancellationToken));

      Task<bool> terminateMemberAsync(string clusterId, string memberId, CancellationToken cancellationToken = default(CancellationToken));

      Task<bool> suspendMemberAsync(string clusterId, string memberId, CancellationToken cancellationToken = default(CancellationToken));

      Task<bool> resumeMemberAsync(string clusterId, string memberId, CancellationToken cancellationToken = default(CancellationToken));

      Task<bool> shutdownClusterAsync(string clusterId, CancellationToken cancellationToken = default(CancellationToken));

      Task<bool> terminateClusterAsync(string clusterId, CancellationToken cancellationToken = default(CancellationToken));

      Task<Cluster> splitMemberFromClusterAsync(string memberId, CancellationToken cancellationToken = default(CancellationToken));

      Task<Cluster> mergeMemberToClusterAsync(string clusterId, string memberId, CancellationToken cancellationToken = default(CancellationToken));

      /// <summary>
      /// Reads the environment variables and calls loginToCloud() method with these variables.
      /// @throws CloudException
      /// </summary>
      Task loginToCloudUsingEnvironmentAsync(CancellationToken cancellationToken = default(CancellationToken));

      /// <summary>
      /// Logins to the cloud, sets the bearerToken, baseUrl variables in CloudManager to make it ready to use cloud API
      /// @throws CloudException
      /// 
      /// @param baseUrl -> Base url of the cloud environment. i.e. https://uat.hazelcast.cloud
      /// @param apiKey -> Api key of the hazelcast cloud
      /// @param apiSecret -> Api secret of the hazelcast cloud
      /// </summary>
      /// <param name="baseUrl"></param>
      /// <param name="apiKey"></param>
      /// <param name="apiSecret"></param>
      Task loginToCloudAsync(string baseUrl, string apiKey, string apiSecret, CancellationToken cancellationToken = default(CancellationToken));

      /// <summary>
      /// Creates a cluster
      /// @return CloudCluster
      /// @throws CloudException
      /// 
      /// @param hazelcastVersion -> Hazelcast version
      /// @param isTlsEnabled -> True if ssl enabled cluster is requested, otherwise false.
      /// </summary>
      /// <param name="hazelcastVersion"></param>
      /// <param name="isTlsEnabled"></param>
      Task<CloudCluster> createCloudClusterAsync(string hazelcastVersion, bool isTlsEnabled, CancellationToken cancellationToken = default(CancellationToken));

      /// <summary>
      /// Get information of the given cluster
      /// @return CloudCluster
      /// @throws CloudException
      /// 
      /// @param cloudClusterId -> Id of the cluster
      /// </summary>
      /// <param name="cloudClusterId"></param>
      Task<CloudCluster> getCloudClusterAsync(string cloudClusterId, CancellationToken cancellationToken = default(CancellationToken));

      /// <summary>
      /// Stops the given cluster
      /// @return CloudCluster
      /// @throws CloudException
      /// 
      /// @param cloudClusterId -> Id of the cluster
      /// </summary>
      /// <param name="cloudClusterId"></param>
      Task<CloudCluster> stopCloudClusterAsync(string cloudClusterId, CancellationToken cancellationToken = default(CancellationToken));

      /// <summary>
      /// Resumes the given cluster
      /// @return CloudCluster
      /// @throws CloudException
      /// 
      /// @param cloudClusterId
      /// </summary>
      /// <param name="cloudClusterId"></param>
      Task<CloudCluster> resumeCloudClusterAsync(string cloudClusterId, CancellationToken cancellationToken = default(CancellationToken));

      /// <summary>
      /// Deletes the given cluster
      /// @return boolean
      /// @throws CloudException
      /// 
      /// @param cloudClusterId
      /// </summary>
      /// <param name="cloudClusterId"></param>
      Task deleteCloudClusterAsync(string cloudClusterId, CancellationToken cancellationToken = default(CancellationToken));

      Task<Response> executeOnControllerAsync(string clusterId, string script, Lang lang, CancellationToken cancellationToken = default(CancellationToken));

    }


    public class Client : TBaseClient, IDisposable, IAsync
    {
      public Client(TProtocol protocol) : this(protocol, protocol)
      {
      }

      public Client(TProtocol inputProtocol, TProtocol outputProtocol) : base(inputProtocol, outputProtocol)      {
      }
      public async Task<bool> pingAsync(CancellationToken cancellationToken = default(CancellationToken))
      {
        await OutputProtocol.WriteMessageBeginAsync(new TMessage("ping", TMessageType.Call, SeqId), cancellationToken);
        
        var args = new pingArgs();
        
        await args.WriteAsync(OutputProtocol, cancellationToken);
        await OutputProtocol.WriteMessageEndAsync(cancellationToken);
        await OutputProtocol.Transport.FlushAsync(cancellationToken);
        
        var msg = await InputProtocol.ReadMessageBeginAsync(cancellationToken);
        if (msg.Type == TMessageType.Exception)
        {
          var x = await TApplicationException.ReadAsync(InputProtocol, cancellationToken);
          await InputProtocol.ReadMessageEndAsync(cancellationToken);
          throw x;
        }

        var result = new pingResult();
        await result.ReadAsync(InputProtocol, cancellationToken);
        await InputProtocol.ReadMessageEndAsync(cancellationToken);
        if (result.__isset.success)
        {
          return result.Success;
        }
        throw new TApplicationException(TApplicationException.ExceptionType.MissingResult, "ping failed: unknown result");
      }

      public async Task<bool> cleanAsync(CancellationToken cancellationToken = default(CancellationToken))
      {
        await OutputProtocol.WriteMessageBeginAsync(new TMessage("clean", TMessageType.Call, SeqId), cancellationToken);
        
        var args = new cleanArgs();
        
        await args.WriteAsync(OutputProtocol, cancellationToken);
        await OutputProtocol.WriteMessageEndAsync(cancellationToken);
        await OutputProtocol.Transport.FlushAsync(cancellationToken);
        
        var msg = await InputProtocol.ReadMessageBeginAsync(cancellationToken);
        if (msg.Type == TMessageType.Exception)
        {
          var x = await TApplicationException.ReadAsync(InputProtocol, cancellationToken);
          await InputProtocol.ReadMessageEndAsync(cancellationToken);
          throw x;
        }

        var result = new cleanResult();
        await result.ReadAsync(InputProtocol, cancellationToken);
        await InputProtocol.ReadMessageEndAsync(cancellationToken);
        if (result.__isset.success)
        {
          return result.Success;
        }
        throw new TApplicationException(TApplicationException.ExceptionType.MissingResult, "clean failed: unknown result");
      }

      public async Task<bool> exitAsync(CancellationToken cancellationToken = default(CancellationToken))
      {
        await OutputProtocol.WriteMessageBeginAsync(new TMessage("exit", TMessageType.Call, SeqId), cancellationToken);
        
        var args = new exitArgs();
        
        await args.WriteAsync(OutputProtocol, cancellationToken);
        await OutputProtocol.WriteMessageEndAsync(cancellationToken);
        await OutputProtocol.Transport.FlushAsync(cancellationToken);
        
        var msg = await InputProtocol.ReadMessageBeginAsync(cancellationToken);
        if (msg.Type == TMessageType.Exception)
        {
          var x = await TApplicationException.ReadAsync(InputProtocol, cancellationToken);
          await InputProtocol.ReadMessageEndAsync(cancellationToken);
          throw x;
        }

        var result = new exitResult();
        await result.ReadAsync(InputProtocol, cancellationToken);
        await InputProtocol.ReadMessageEndAsync(cancellationToken);
        if (result.__isset.success)
        {
          return result.Success;
        }
        throw new TApplicationException(TApplicationException.ExceptionType.MissingResult, "exit failed: unknown result");
      }

      public async Task<Cluster> createClusterAsync(string hzVersion, string xmlconfig, CancellationToken cancellationToken = default(CancellationToken))
      {
        await OutputProtocol.WriteMessageBeginAsync(new TMessage("createCluster", TMessageType.Call, SeqId), cancellationToken);
        
        var args = new createClusterArgs();
        args.HzVersion = hzVersion;
        args.Xmlconfig = xmlconfig;
        
        await args.WriteAsync(OutputProtocol, cancellationToken);
        await OutputProtocol.WriteMessageEndAsync(cancellationToken);
        await OutputProtocol.Transport.FlushAsync(cancellationToken);
        
        var msg = await InputProtocol.ReadMessageBeginAsync(cancellationToken);
        if (msg.Type == TMessageType.Exception)
        {
          var x = await TApplicationException.ReadAsync(InputProtocol, cancellationToken);
          await InputProtocol.ReadMessageEndAsync(cancellationToken);
          throw x;
        }

        var result = new createClusterResult();
        await result.ReadAsync(InputProtocol, cancellationToken);
        await InputProtocol.ReadMessageEndAsync(cancellationToken);
        if (result.__isset.success)
        {
          return result.Success;
        }
        if (result.__isset.serverException)
        {
          throw result.ServerException;
        }
        throw new TApplicationException(TApplicationException.ExceptionType.MissingResult, "createCluster failed: unknown result");
      }

      public async Task<Cluster> createClusterKeepClusterNameAsync(string hzVersion, string xmlconfig, CancellationToken cancellationToken = default(CancellationToken))
      {
        await OutputProtocol.WriteMessageBeginAsync(new TMessage("createClusterKeepClusterName", TMessageType.Call, SeqId), cancellationToken);
        
        var args = new createClusterKeepClusterNameArgs();
        args.HzVersion = hzVersion;
        args.Xmlconfig = xmlconfig;
        
        await args.WriteAsync(OutputProtocol, cancellationToken);
        await OutputProtocol.WriteMessageEndAsync(cancellationToken);
        await OutputProtocol.Transport.FlushAsync(cancellationToken);
        
        var msg = await InputProtocol.ReadMessageBeginAsync(cancellationToken);
        if (msg.Type == TMessageType.Exception)
        {
          var x = await TApplicationException.ReadAsync(InputProtocol, cancellationToken);
          await InputProtocol.ReadMessageEndAsync(cancellationToken);
          throw x;
        }

        var result = new createClusterKeepClusterNameResult();
        await result.ReadAsync(InputProtocol, cancellationToken);
        await InputProtocol.ReadMessageEndAsync(cancellationToken);
        if (result.__isset.success)
        {
          return result.Success;
        }
        if (result.__isset.serverException)
        {
          throw result.ServerException;
        }
        throw new TApplicationException(TApplicationException.ExceptionType.MissingResult, "createClusterKeepClusterName failed: unknown result");
      }

      public async Task<Member> startMemberAsync(string clusterId, CancellationToken cancellationToken = default(CancellationToken))
      {
        await OutputProtocol.WriteMessageBeginAsync(new TMessage("startMember", TMessageType.Call, SeqId), cancellationToken);
        
        var args = new startMemberArgs();
        args.ClusterId = clusterId;
        
        await args.WriteAsync(OutputProtocol, cancellationToken);
        await OutputProtocol.WriteMessageEndAsync(cancellationToken);
        await OutputProtocol.Transport.FlushAsync(cancellationToken);
        
        var msg = await InputProtocol.ReadMessageBeginAsync(cancellationToken);
        if (msg.Type == TMessageType.Exception)
        {
          var x = await TApplicationException.ReadAsync(InputProtocol, cancellationToken);
          await InputProtocol.ReadMessageEndAsync(cancellationToken);
          throw x;
        }

        var result = new startMemberResult();
        await result.ReadAsync(InputProtocol, cancellationToken);
        await InputProtocol.ReadMessageEndAsync(cancellationToken);
        if (result.__isset.success)
        {
          return result.Success;
        }
        if (result.__isset.serverException)
        {
          throw result.ServerException;
        }
        throw new TApplicationException(TApplicationException.ExceptionType.MissingResult, "startMember failed: unknown result");
      }

      public async Task<bool> shutdownMemberAsync(string clusterId, string memberId, CancellationToken cancellationToken = default(CancellationToken))
      {
        await OutputProtocol.WriteMessageBeginAsync(new TMessage("shutdownMember", TMessageType.Call, SeqId), cancellationToken);
        
        var args = new shutdownMemberArgs();
        args.ClusterId = clusterId;
        args.MemberId = memberId;
        
        await args.WriteAsync(OutputProtocol, cancellationToken);
        await OutputProtocol.WriteMessageEndAsync(cancellationToken);
        await OutputProtocol.Transport.FlushAsync(cancellationToken);
        
        var msg = await InputProtocol.ReadMessageBeginAsync(cancellationToken);
        if (msg.Type == TMessageType.Exception)
        {
          var x = await TApplicationException.ReadAsync(InputProtocol, cancellationToken);
          await InputProtocol.ReadMessageEndAsync(cancellationToken);
          throw x;
        }

        var result = new shutdownMemberResult();
        await result.ReadAsync(InputProtocol, cancellationToken);
        await InputProtocol.ReadMessageEndAsync(cancellationToken);
        if (result.__isset.success)
        {
          return result.Success;
        }
        throw new TApplicationException(TApplicationException.ExceptionType.MissingResult, "shutdownMember failed: unknown result");
      }

      public async Task<bool> terminateMemberAsync(string clusterId, string memberId, CancellationToken cancellationToken = default(CancellationToken))
      {
        await OutputProtocol.WriteMessageBeginAsync(new TMessage("terminateMember", TMessageType.Call, SeqId), cancellationToken);
        
        var args = new terminateMemberArgs();
        args.ClusterId = clusterId;
        args.MemberId = memberId;
        
        await args.WriteAsync(OutputProtocol, cancellationToken);
        await OutputProtocol.WriteMessageEndAsync(cancellationToken);
        await OutputProtocol.Transport.FlushAsync(cancellationToken);
        
        var msg = await InputProtocol.ReadMessageBeginAsync(cancellationToken);
        if (msg.Type == TMessageType.Exception)
        {
          var x = await TApplicationException.ReadAsync(InputProtocol, cancellationToken);
          await InputProtocol.ReadMessageEndAsync(cancellationToken);
          throw x;
        }

        var result = new terminateMemberResult();
        await result.ReadAsync(InputProtocol, cancellationToken);
        await InputProtocol.ReadMessageEndAsync(cancellationToken);
        if (result.__isset.success)
        {
          return result.Success;
        }
        throw new TApplicationException(TApplicationException.ExceptionType.MissingResult, "terminateMember failed: unknown result");
      }

      public async Task<bool> suspendMemberAsync(string clusterId, string memberId, CancellationToken cancellationToken = default(CancellationToken))
      {
        await OutputProtocol.WriteMessageBeginAsync(new TMessage("suspendMember", TMessageType.Call, SeqId), cancellationToken);
        
        var args = new suspendMemberArgs();
        args.ClusterId = clusterId;
        args.MemberId = memberId;
        
        await args.WriteAsync(OutputProtocol, cancellationToken);
        await OutputProtocol.WriteMessageEndAsync(cancellationToken);
        await OutputProtocol.Transport.FlushAsync(cancellationToken);
        
        var msg = await InputProtocol.ReadMessageBeginAsync(cancellationToken);
        if (msg.Type == TMessageType.Exception)
        {
          var x = await TApplicationException.ReadAsync(InputProtocol, cancellationToken);
          await InputProtocol.ReadMessageEndAsync(cancellationToken);
          throw x;
        }

        var result = new suspendMemberResult();
        await result.ReadAsync(InputProtocol, cancellationToken);
        await InputProtocol.ReadMessageEndAsync(cancellationToken);
        if (result.__isset.success)
        {
          return result.Success;
        }
        throw new TApplicationException(TApplicationException.ExceptionType.MissingResult, "suspendMember failed: unknown result");
      }

      public async Task<bool> resumeMemberAsync(string clusterId, string memberId, CancellationToken cancellationToken = default(CancellationToken))
      {
        await OutputProtocol.WriteMessageBeginAsync(new TMessage("resumeMember", TMessageType.Call, SeqId), cancellationToken);
        
        var args = new resumeMemberArgs();
        args.ClusterId = clusterId;
        args.MemberId = memberId;
        
        await args.WriteAsync(OutputProtocol, cancellationToken);
        await OutputProtocol.WriteMessageEndAsync(cancellationToken);
        await OutputProtocol.Transport.FlushAsync(cancellationToken);
        
        var msg = await InputProtocol.ReadMessageBeginAsync(cancellationToken);
        if (msg.Type == TMessageType.Exception)
        {
          var x = await TApplicationException.ReadAsync(InputProtocol, cancellationToken);
          await InputProtocol.ReadMessageEndAsync(cancellationToken);
          throw x;
        }

        var result = new resumeMemberResult();
        await result.ReadAsync(InputProtocol, cancellationToken);
        await InputProtocol.ReadMessageEndAsync(cancellationToken);
        if (result.__isset.success)
        {
          return result.Success;
        }
        throw new TApplicationException(TApplicationException.ExceptionType.MissingResult, "resumeMember failed: unknown result");
      }

      public async Task<bool> shutdownClusterAsync(string clusterId, CancellationToken cancellationToken = default(CancellationToken))
      {
        await OutputProtocol.WriteMessageBeginAsync(new TMessage("shutdownCluster", TMessageType.Call, SeqId), cancellationToken);
        
        var args = new shutdownClusterArgs();
        args.ClusterId = clusterId;
        
        await args.WriteAsync(OutputProtocol, cancellationToken);
        await OutputProtocol.WriteMessageEndAsync(cancellationToken);
        await OutputProtocol.Transport.FlushAsync(cancellationToken);
        
        var msg = await InputProtocol.ReadMessageBeginAsync(cancellationToken);
        if (msg.Type == TMessageType.Exception)
        {
          var x = await TApplicationException.ReadAsync(InputProtocol, cancellationToken);
          await InputProtocol.ReadMessageEndAsync(cancellationToken);
          throw x;
        }

        var result = new shutdownClusterResult();
        await result.ReadAsync(InputProtocol, cancellationToken);
        await InputProtocol.ReadMessageEndAsync(cancellationToken);
        if (result.__isset.success)
        {
          return result.Success;
        }
        throw new TApplicationException(TApplicationException.ExceptionType.MissingResult, "shutdownCluster failed: unknown result");
      }

      public async Task<bool> terminateClusterAsync(string clusterId, CancellationToken cancellationToken = default(CancellationToken))
      {
        await OutputProtocol.WriteMessageBeginAsync(new TMessage("terminateCluster", TMessageType.Call, SeqId), cancellationToken);
        
        var args = new terminateClusterArgs();
        args.ClusterId = clusterId;
        
        await args.WriteAsync(OutputProtocol, cancellationToken);
        await OutputProtocol.WriteMessageEndAsync(cancellationToken);
        await OutputProtocol.Transport.FlushAsync(cancellationToken);
        
        var msg = await InputProtocol.ReadMessageBeginAsync(cancellationToken);
        if (msg.Type == TMessageType.Exception)
        {
          var x = await TApplicationException.ReadAsync(InputProtocol, cancellationToken);
          await InputProtocol.ReadMessageEndAsync(cancellationToken);
          throw x;
        }

        var result = new terminateClusterResult();
        await result.ReadAsync(InputProtocol, cancellationToken);
        await InputProtocol.ReadMessageEndAsync(cancellationToken);
        if (result.__isset.success)
        {
          return result.Success;
        }
        throw new TApplicationException(TApplicationException.ExceptionType.MissingResult, "terminateCluster failed: unknown result");
      }

      public async Task<Cluster> splitMemberFromClusterAsync(string memberId, CancellationToken cancellationToken = default(CancellationToken))
      {
        await OutputProtocol.WriteMessageBeginAsync(new TMessage("splitMemberFromCluster", TMessageType.Call, SeqId), cancellationToken);
        
        var args = new splitMemberFromClusterArgs();
        args.MemberId = memberId;
        
        await args.WriteAsync(OutputProtocol, cancellationToken);
        await OutputProtocol.WriteMessageEndAsync(cancellationToken);
        await OutputProtocol.Transport.FlushAsync(cancellationToken);
        
        var msg = await InputProtocol.ReadMessageBeginAsync(cancellationToken);
        if (msg.Type == TMessageType.Exception)
        {
          var x = await TApplicationException.ReadAsync(InputProtocol, cancellationToken);
          await InputProtocol.ReadMessageEndAsync(cancellationToken);
          throw x;
        }

        var result = new splitMemberFromClusterResult();
        await result.ReadAsync(InputProtocol, cancellationToken);
        await InputProtocol.ReadMessageEndAsync(cancellationToken);
        if (result.__isset.success)
        {
          return result.Success;
        }
        throw new TApplicationException(TApplicationException.ExceptionType.MissingResult, "splitMemberFromCluster failed: unknown result");
      }

      public async Task<Cluster> mergeMemberToClusterAsync(string clusterId, string memberId, CancellationToken cancellationToken = default(CancellationToken))
      {
        await OutputProtocol.WriteMessageBeginAsync(new TMessage("mergeMemberToCluster", TMessageType.Call, SeqId), cancellationToken);
        
        var args = new mergeMemberToClusterArgs();
        args.ClusterId = clusterId;
        args.MemberId = memberId;
        
        await args.WriteAsync(OutputProtocol, cancellationToken);
        await OutputProtocol.WriteMessageEndAsync(cancellationToken);
        await OutputProtocol.Transport.FlushAsync(cancellationToken);
        
        var msg = await InputProtocol.ReadMessageBeginAsync(cancellationToken);
        if (msg.Type == TMessageType.Exception)
        {
          var x = await TApplicationException.ReadAsync(InputProtocol, cancellationToken);
          await InputProtocol.ReadMessageEndAsync(cancellationToken);
          throw x;
        }

        var result = new mergeMemberToClusterResult();
        await result.ReadAsync(InputProtocol, cancellationToken);
        await InputProtocol.ReadMessageEndAsync(cancellationToken);
        if (result.__isset.success)
        {
          return result.Success;
        }
        throw new TApplicationException(TApplicationException.ExceptionType.MissingResult, "mergeMemberToCluster failed: unknown result");
      }

      public async Task loginToCloudUsingEnvironmentAsync(CancellationToken cancellationToken = default(CancellationToken))
      {
        await OutputProtocol.WriteMessageBeginAsync(new TMessage("loginToCloudUsingEnvironment", TMessageType.Call, SeqId), cancellationToken);
        
        var args = new loginToCloudUsingEnvironmentArgs();
        
        await args.WriteAsync(OutputProtocol, cancellationToken);
        await OutputProtocol.WriteMessageEndAsync(cancellationToken);
        await OutputProtocol.Transport.FlushAsync(cancellationToken);
        
        var msg = await InputProtocol.ReadMessageBeginAsync(cancellationToken);
        if (msg.Type == TMessageType.Exception)
        {
          var x = await TApplicationException.ReadAsync(InputProtocol, cancellationToken);
          await InputProtocol.ReadMessageEndAsync(cancellationToken);
          throw x;
        }

        var result = new loginToCloudUsingEnvironmentResult();
        await result.ReadAsync(InputProtocol, cancellationToken);
        await InputProtocol.ReadMessageEndAsync(cancellationToken);
        if (result.__isset.cloudException)
        {
          throw result.CloudException;
        }
        return;
      }

      public async Task loginToCloudAsync(string baseUrl, string apiKey, string apiSecret, CancellationToken cancellationToken = default(CancellationToken))
      {
        await OutputProtocol.WriteMessageBeginAsync(new TMessage("loginToCloud", TMessageType.Call, SeqId), cancellationToken);
        
        var args = new loginToCloudArgs();
        args.BaseUrl = baseUrl;
        args.ApiKey = apiKey;
        args.ApiSecret = apiSecret;
        
        await args.WriteAsync(OutputProtocol, cancellationToken);
        await OutputProtocol.WriteMessageEndAsync(cancellationToken);
        await OutputProtocol.Transport.FlushAsync(cancellationToken);
        
        var msg = await InputProtocol.ReadMessageBeginAsync(cancellationToken);
        if (msg.Type == TMessageType.Exception)
        {
          var x = await TApplicationException.ReadAsync(InputProtocol, cancellationToken);
          await InputProtocol.ReadMessageEndAsync(cancellationToken);
          throw x;
        }

        var result = new loginToCloudResult();
        await result.ReadAsync(InputProtocol, cancellationToken);
        await InputProtocol.ReadMessageEndAsync(cancellationToken);
        if (result.__isset.cloudException)
        {
          throw result.CloudException;
        }
        return;
      }

      public async Task<CloudCluster> createCloudClusterAsync(string hazelcastVersion, bool isTlsEnabled, CancellationToken cancellationToken = default(CancellationToken))
      {
        await OutputProtocol.WriteMessageBeginAsync(new TMessage("createCloudCluster", TMessageType.Call, SeqId), cancellationToken);
        
        var args = new createCloudClusterArgs();
        args.HazelcastVersion = hazelcastVersion;
        args.IsTlsEnabled = isTlsEnabled;
        
        await args.WriteAsync(OutputProtocol, cancellationToken);
        await OutputProtocol.WriteMessageEndAsync(cancellationToken);
        await OutputProtocol.Transport.FlushAsync(cancellationToken);
        
        var msg = await InputProtocol.ReadMessageBeginAsync(cancellationToken);
        if (msg.Type == TMessageType.Exception)
        {
          var x = await TApplicationException.ReadAsync(InputProtocol, cancellationToken);
          await InputProtocol.ReadMessageEndAsync(cancellationToken);
          throw x;
        }

        var result = new createCloudClusterResult();
        await result.ReadAsync(InputProtocol, cancellationToken);
        await InputProtocol.ReadMessageEndAsync(cancellationToken);
        if (result.__isset.success)
        {
          return result.Success;
        }
        if (result.__isset.cloudException)
        {
          throw result.CloudException;
        }
        throw new TApplicationException(TApplicationException.ExceptionType.MissingResult, "createCloudCluster failed: unknown result");
      }

      public async Task<CloudCluster> getCloudClusterAsync(string cloudClusterId, CancellationToken cancellationToken = default(CancellationToken))
      {
        await OutputProtocol.WriteMessageBeginAsync(new TMessage("getCloudCluster", TMessageType.Call, SeqId), cancellationToken);
        
        var args = new getCloudClusterArgs();
        args.CloudClusterId = cloudClusterId;
        
        await args.WriteAsync(OutputProtocol, cancellationToken);
        await OutputProtocol.WriteMessageEndAsync(cancellationToken);
        await OutputProtocol.Transport.FlushAsync(cancellationToken);
        
        var msg = await InputProtocol.ReadMessageBeginAsync(cancellationToken);
        if (msg.Type == TMessageType.Exception)
        {
          var x = await TApplicationException.ReadAsync(InputProtocol, cancellationToken);
          await InputProtocol.ReadMessageEndAsync(cancellationToken);
          throw x;
        }

        var result = new getCloudClusterResult();
        await result.ReadAsync(InputProtocol, cancellationToken);
        await InputProtocol.ReadMessageEndAsync(cancellationToken);
        if (result.__isset.success)
        {
          return result.Success;
        }
        if (result.__isset.cloudException)
        {
          throw result.CloudException;
        }
        throw new TApplicationException(TApplicationException.ExceptionType.MissingResult, "getCloudCluster failed: unknown result");
      }

      public async Task<CloudCluster> stopCloudClusterAsync(string cloudClusterId, CancellationToken cancellationToken = default(CancellationToken))
      {
        await OutputProtocol.WriteMessageBeginAsync(new TMessage("stopCloudCluster", TMessageType.Call, SeqId), cancellationToken);
        
        var args = new stopCloudClusterArgs();
        args.CloudClusterId = cloudClusterId;
        
        await args.WriteAsync(OutputProtocol, cancellationToken);
        await OutputProtocol.WriteMessageEndAsync(cancellationToken);
        await OutputProtocol.Transport.FlushAsync(cancellationToken);
        
        var msg = await InputProtocol.ReadMessageBeginAsync(cancellationToken);
        if (msg.Type == TMessageType.Exception)
        {
          var x = await TApplicationException.ReadAsync(InputProtocol, cancellationToken);
          await InputProtocol.ReadMessageEndAsync(cancellationToken);
          throw x;
        }

        var result = new stopCloudClusterResult();
        await result.ReadAsync(InputProtocol, cancellationToken);
        await InputProtocol.ReadMessageEndAsync(cancellationToken);
        if (result.__isset.success)
        {
          return result.Success;
        }
        if (result.__isset.cloudException)
        {
          throw result.CloudException;
        }
        throw new TApplicationException(TApplicationException.ExceptionType.MissingResult, "stopCloudCluster failed: unknown result");
      }

      public async Task<CloudCluster> resumeCloudClusterAsync(string cloudClusterId, CancellationToken cancellationToken = default(CancellationToken))
      {
        await OutputProtocol.WriteMessageBeginAsync(new TMessage("resumeCloudCluster", TMessageType.Call, SeqId), cancellationToken);
        
        var args = new resumeCloudClusterArgs();
        args.CloudClusterId = cloudClusterId;
        
        await args.WriteAsync(OutputProtocol, cancellationToken);
        await OutputProtocol.WriteMessageEndAsync(cancellationToken);
        await OutputProtocol.Transport.FlushAsync(cancellationToken);
        
        var msg = await InputProtocol.ReadMessageBeginAsync(cancellationToken);
        if (msg.Type == TMessageType.Exception)
        {
          var x = await TApplicationException.ReadAsync(InputProtocol, cancellationToken);
          await InputProtocol.ReadMessageEndAsync(cancellationToken);
          throw x;
        }

        var result = new resumeCloudClusterResult();
        await result.ReadAsync(InputProtocol, cancellationToken);
        await InputProtocol.ReadMessageEndAsync(cancellationToken);
        if (result.__isset.success)
        {
          return result.Success;
        }
        if (result.__isset.cloudException)
        {
          throw result.CloudException;
        }
        throw new TApplicationException(TApplicationException.ExceptionType.MissingResult, "resumeCloudCluster failed: unknown result");
      }

      public async Task deleteCloudClusterAsync(string cloudClusterId, CancellationToken cancellationToken = default(CancellationToken))
      {
        await OutputProtocol.WriteMessageBeginAsync(new TMessage("deleteCloudCluster", TMessageType.Call, SeqId), cancellationToken);
        
        var args = new deleteCloudClusterArgs();
        args.CloudClusterId = cloudClusterId;
        
        await args.WriteAsync(OutputProtocol, cancellationToken);
        await OutputProtocol.WriteMessageEndAsync(cancellationToken);
        await OutputProtocol.Transport.FlushAsync(cancellationToken);
        
        var msg = await InputProtocol.ReadMessageBeginAsync(cancellationToken);
        if (msg.Type == TMessageType.Exception)
        {
          var x = await TApplicationException.ReadAsync(InputProtocol, cancellationToken);
          await InputProtocol.ReadMessageEndAsync(cancellationToken);
          throw x;
        }

        var result = new deleteCloudClusterResult();
        await result.ReadAsync(InputProtocol, cancellationToken);
        await InputProtocol.ReadMessageEndAsync(cancellationToken);
        if (result.__isset.cloudException)
        {
          throw result.CloudException;
        }
        return;
      }

      public async Task<Response> executeOnControllerAsync(string clusterId, string script, Lang lang, CancellationToken cancellationToken = default(CancellationToken))
      {
        await OutputProtocol.WriteMessageBeginAsync(new TMessage("executeOnController", TMessageType.Call, SeqId), cancellationToken);
        
        var args = new executeOnControllerArgs();
        args.ClusterId = clusterId;
        args.Script = script;
        args.Lang = lang;
        
        await args.WriteAsync(OutputProtocol, cancellationToken);
        await OutputProtocol.WriteMessageEndAsync(cancellationToken);
        await OutputProtocol.Transport.FlushAsync(cancellationToken);
        
        var msg = await InputProtocol.ReadMessageBeginAsync(cancellationToken);
        if (msg.Type == TMessageType.Exception)
        {
          var x = await TApplicationException.ReadAsync(InputProtocol, cancellationToken);
          await InputProtocol.ReadMessageEndAsync(cancellationToken);
          throw x;
        }

        var result = new executeOnControllerResult();
        await result.ReadAsync(InputProtocol, cancellationToken);
        await InputProtocol.ReadMessageEndAsync(cancellationToken);
        if (result.__isset.success)
        {
          return result.Success;
        }
        throw new TApplicationException(TApplicationException.ExceptionType.MissingResult, "executeOnController failed: unknown result");
      }

    }

    public class AsyncProcessor : ITAsyncProcessor
    {
      private IAsync _iAsync;

      public AsyncProcessor(IAsync iAsync)
      {
        if (iAsync == null) throw new ArgumentNullException(nameof(iAsync));

        _iAsync = iAsync;
        processMap_["ping"] = ping_ProcessAsync;
        processMap_["clean"] = clean_ProcessAsync;
        processMap_["exit"] = exit_ProcessAsync;
        processMap_["createCluster"] = createCluster_ProcessAsync;
        processMap_["createClusterKeepClusterName"] = createClusterKeepClusterName_ProcessAsync;
        processMap_["startMember"] = startMember_ProcessAsync;
        processMap_["shutdownMember"] = shutdownMember_ProcessAsync;
        processMap_["terminateMember"] = terminateMember_ProcessAsync;
        processMap_["suspendMember"] = suspendMember_ProcessAsync;
        processMap_["resumeMember"] = resumeMember_ProcessAsync;
        processMap_["shutdownCluster"] = shutdownCluster_ProcessAsync;
        processMap_["terminateCluster"] = terminateCluster_ProcessAsync;
        processMap_["splitMemberFromCluster"] = splitMemberFromCluster_ProcessAsync;
        processMap_["mergeMemberToCluster"] = mergeMemberToCluster_ProcessAsync;
        processMap_["loginToCloudUsingEnvironment"] = loginToCloudUsingEnvironment_ProcessAsync;
        processMap_["loginToCloud"] = loginToCloud_ProcessAsync;
        processMap_["createCloudCluster"] = createCloudCluster_ProcessAsync;
        processMap_["getCloudCluster"] = getCloudCluster_ProcessAsync;
        processMap_["stopCloudCluster"] = stopCloudCluster_ProcessAsync;
        processMap_["resumeCloudCluster"] = resumeCloudCluster_ProcessAsync;
        processMap_["deleteCloudCluster"] = deleteCloudCluster_ProcessAsync;
        processMap_["executeOnController"] = executeOnController_ProcessAsync;
      }

      protected delegate Task ProcessFunction(int seqid, TProtocol iprot, TProtocol oprot, CancellationToken cancellationToken);
      protected Dictionary<string, ProcessFunction> processMap_ = new Dictionary<string, ProcessFunction>();

      public async Task<bool> ProcessAsync(TProtocol iprot, TProtocol oprot)
      {
        return await ProcessAsync(iprot, oprot, CancellationToken.None);
      }

      public async Task<bool> ProcessAsync(TProtocol iprot, TProtocol oprot, CancellationToken cancellationToken)
      {
        try
        {
          var msg = await iprot.ReadMessageBeginAsync(cancellationToken);

          ProcessFunction fn;
          processMap_.TryGetValue(msg.Name, out fn);

          if (fn == null)
          {
            await TProtocolUtil.SkipAsync(iprot, TType.Struct, cancellationToken);
            await iprot.ReadMessageEndAsync(cancellationToken);
            var x = new TApplicationException (TApplicationException.ExceptionType.UnknownMethod, "Invalid method name: '" + msg.Name + "'");
            await oprot.WriteMessageBeginAsync(new TMessage(msg.Name, TMessageType.Exception, msg.SeqID), cancellationToken);
            await x.WriteAsync(oprot, cancellationToken);
            await oprot.WriteMessageEndAsync(cancellationToken);
            await oprot.Transport.FlushAsync(cancellationToken);
            return true;
          }

          await fn(msg.SeqID, iprot, oprot, cancellationToken);

        }
        catch (IOException)
        {
          return false;
        }

        return true;
      }

      public async Task ping_ProcessAsync(int seqid, TProtocol iprot, TProtocol oprot, CancellationToken cancellationToken)
      {
        var args = new pingArgs();
        await args.ReadAsync(iprot, cancellationToken);
        await iprot.ReadMessageEndAsync(cancellationToken);
        var result = new pingResult();
        try
        {
          result.Success = await _iAsync.pingAsync(cancellationToken);
          await oprot.WriteMessageBeginAsync(new TMessage("ping", TMessageType.Reply, seqid), cancellationToken); 
          await result.WriteAsync(oprot, cancellationToken);
        }
        catch (TTransportException)
        {
          throw;
        }
        catch (Exception ex)
        {
          Console.Error.WriteLine("Error occurred in processor:");
          Console.Error.WriteLine(ex.ToString());
          var x = new TApplicationException(TApplicationException.ExceptionType.InternalError," Internal error.");
          await oprot.WriteMessageBeginAsync(new TMessage("ping", TMessageType.Exception, seqid), cancellationToken);
          await x.WriteAsync(oprot, cancellationToken);
        }
        await oprot.WriteMessageEndAsync(cancellationToken);
        await oprot.Transport.FlushAsync(cancellationToken);
      }

      public async Task clean_ProcessAsync(int seqid, TProtocol iprot, TProtocol oprot, CancellationToken cancellationToken)
      {
        var args = new cleanArgs();
        await args.ReadAsync(iprot, cancellationToken);
        await iprot.ReadMessageEndAsync(cancellationToken);
        var result = new cleanResult();
        try
        {
          result.Success = await _iAsync.cleanAsync(cancellationToken);
          await oprot.WriteMessageBeginAsync(new TMessage("clean", TMessageType.Reply, seqid), cancellationToken); 
          await result.WriteAsync(oprot, cancellationToken);
        }
        catch (TTransportException)
        {
          throw;
        }
        catch (Exception ex)
        {
          Console.Error.WriteLine("Error occurred in processor:");
          Console.Error.WriteLine(ex.ToString());
          var x = new TApplicationException(TApplicationException.ExceptionType.InternalError," Internal error.");
          await oprot.WriteMessageBeginAsync(new TMessage("clean", TMessageType.Exception, seqid), cancellationToken);
          await x.WriteAsync(oprot, cancellationToken);
        }
        await oprot.WriteMessageEndAsync(cancellationToken);
        await oprot.Transport.FlushAsync(cancellationToken);
      }

      public async Task exit_ProcessAsync(int seqid, TProtocol iprot, TProtocol oprot, CancellationToken cancellationToken)
      {
        var args = new exitArgs();
        await args.ReadAsync(iprot, cancellationToken);
        await iprot.ReadMessageEndAsync(cancellationToken);
        var result = new exitResult();
        try
        {
          result.Success = await _iAsync.exitAsync(cancellationToken);
          await oprot.WriteMessageBeginAsync(new TMessage("exit", TMessageType.Reply, seqid), cancellationToken); 
          await result.WriteAsync(oprot, cancellationToken);
        }
        catch (TTransportException)
        {
          throw;
        }
        catch (Exception ex)
        {
          Console.Error.WriteLine("Error occurred in processor:");
          Console.Error.WriteLine(ex.ToString());
          var x = new TApplicationException(TApplicationException.ExceptionType.InternalError," Internal error.");
          await oprot.WriteMessageBeginAsync(new TMessage("exit", TMessageType.Exception, seqid), cancellationToken);
          await x.WriteAsync(oprot, cancellationToken);
        }
        await oprot.WriteMessageEndAsync(cancellationToken);
        await oprot.Transport.FlushAsync(cancellationToken);
      }

      public async Task createCluster_ProcessAsync(int seqid, TProtocol iprot, TProtocol oprot, CancellationToken cancellationToken)
      {
        var args = new createClusterArgs();
        await args.ReadAsync(iprot, cancellationToken);
        await iprot.ReadMessageEndAsync(cancellationToken);
        var result = new createClusterResult();
        try
        {
          try
          {
            result.Success = await _iAsync.createClusterAsync(args.HzVersion, args.Xmlconfig, cancellationToken);
          }
          catch (ServerException serverException)
          {
            result.ServerException = serverException;
          }
          await oprot.WriteMessageBeginAsync(new TMessage("createCluster", TMessageType.Reply, seqid), cancellationToken); 
          await result.WriteAsync(oprot, cancellationToken);
        }
        catch (TTransportException)
        {
          throw;
        }
        catch (Exception ex)
        {
          Console.Error.WriteLine("Error occurred in processor:");
          Console.Error.WriteLine(ex.ToString());
          var x = new TApplicationException(TApplicationException.ExceptionType.InternalError," Internal error.");
          await oprot.WriteMessageBeginAsync(new TMessage("createCluster", TMessageType.Exception, seqid), cancellationToken);
          await x.WriteAsync(oprot, cancellationToken);
        }
        await oprot.WriteMessageEndAsync(cancellationToken);
        await oprot.Transport.FlushAsync(cancellationToken);
      }

      public async Task createClusterKeepClusterName_ProcessAsync(int seqid, TProtocol iprot, TProtocol oprot, CancellationToken cancellationToken)
      {
        var args = new createClusterKeepClusterNameArgs();
        await args.ReadAsync(iprot, cancellationToken);
        await iprot.ReadMessageEndAsync(cancellationToken);
        var result = new createClusterKeepClusterNameResult();
        try
        {
          try
          {
            result.Success = await _iAsync.createClusterKeepClusterNameAsync(args.HzVersion, args.Xmlconfig, cancellationToken);
          }
          catch (ServerException serverException)
          {
            result.ServerException = serverException;
          }
          await oprot.WriteMessageBeginAsync(new TMessage("createClusterKeepClusterName", TMessageType.Reply, seqid), cancellationToken); 
          await result.WriteAsync(oprot, cancellationToken);
        }
        catch (TTransportException)
        {
          throw;
        }
        catch (Exception ex)
        {
          Console.Error.WriteLine("Error occurred in processor:");
          Console.Error.WriteLine(ex.ToString());
          var x = new TApplicationException(TApplicationException.ExceptionType.InternalError," Internal error.");
          await oprot.WriteMessageBeginAsync(new TMessage("createClusterKeepClusterName", TMessageType.Exception, seqid), cancellationToken);
          await x.WriteAsync(oprot, cancellationToken);
        }
        await oprot.WriteMessageEndAsync(cancellationToken);
        await oprot.Transport.FlushAsync(cancellationToken);
      }

      public async Task startMember_ProcessAsync(int seqid, TProtocol iprot, TProtocol oprot, CancellationToken cancellationToken)
      {
        var args = new startMemberArgs();
        await args.ReadAsync(iprot, cancellationToken);
        await iprot.ReadMessageEndAsync(cancellationToken);
        var result = new startMemberResult();
        try
        {
          try
          {
            result.Success = await _iAsync.startMemberAsync(args.ClusterId, cancellationToken);
          }
          catch (ServerException serverException)
          {
            result.ServerException = serverException;
          }
          await oprot.WriteMessageBeginAsync(new TMessage("startMember", TMessageType.Reply, seqid), cancellationToken); 
          await result.WriteAsync(oprot, cancellationToken);
        }
        catch (TTransportException)
        {
          throw;
        }
        catch (Exception ex)
        {
          Console.Error.WriteLine("Error occurred in processor:");
          Console.Error.WriteLine(ex.ToString());
          var x = new TApplicationException(TApplicationException.ExceptionType.InternalError," Internal error.");
          await oprot.WriteMessageBeginAsync(new TMessage("startMember", TMessageType.Exception, seqid), cancellationToken);
          await x.WriteAsync(oprot, cancellationToken);
        }
        await oprot.WriteMessageEndAsync(cancellationToken);
        await oprot.Transport.FlushAsync(cancellationToken);
      }

      public async Task shutdownMember_ProcessAsync(int seqid, TProtocol iprot, TProtocol oprot, CancellationToken cancellationToken)
      {
        var args = new shutdownMemberArgs();
        await args.ReadAsync(iprot, cancellationToken);
        await iprot.ReadMessageEndAsync(cancellationToken);
        var result = new shutdownMemberResult();
        try
        {
          result.Success = await _iAsync.shutdownMemberAsync(args.ClusterId, args.MemberId, cancellationToken);
          await oprot.WriteMessageBeginAsync(new TMessage("shutdownMember", TMessageType.Reply, seqid), cancellationToken); 
          await result.WriteAsync(oprot, cancellationToken);
        }
        catch (TTransportException)
        {
          throw;
        }
        catch (Exception ex)
        {
          Console.Error.WriteLine("Error occurred in processor:");
          Console.Error.WriteLine(ex.ToString());
          var x = new TApplicationException(TApplicationException.ExceptionType.InternalError," Internal error.");
          await oprot.WriteMessageBeginAsync(new TMessage("shutdownMember", TMessageType.Exception, seqid), cancellationToken);
          await x.WriteAsync(oprot, cancellationToken);
        }
        await oprot.WriteMessageEndAsync(cancellationToken);
        await oprot.Transport.FlushAsync(cancellationToken);
      }

      public async Task terminateMember_ProcessAsync(int seqid, TProtocol iprot, TProtocol oprot, CancellationToken cancellationToken)
      {
        var args = new terminateMemberArgs();
        await args.ReadAsync(iprot, cancellationToken);
        await iprot.ReadMessageEndAsync(cancellationToken);
        var result = new terminateMemberResult();
        try
        {
          result.Success = await _iAsync.terminateMemberAsync(args.ClusterId, args.MemberId, cancellationToken);
          await oprot.WriteMessageBeginAsync(new TMessage("terminateMember", TMessageType.Reply, seqid), cancellationToken); 
          await result.WriteAsync(oprot, cancellationToken);
        }
        catch (TTransportException)
        {
          throw;
        }
        catch (Exception ex)
        {
          Console.Error.WriteLine("Error occurred in processor:");
          Console.Error.WriteLine(ex.ToString());
          var x = new TApplicationException(TApplicationException.ExceptionType.InternalError," Internal error.");
          await oprot.WriteMessageBeginAsync(new TMessage("terminateMember", TMessageType.Exception, seqid), cancellationToken);
          await x.WriteAsync(oprot, cancellationToken);
        }
        await oprot.WriteMessageEndAsync(cancellationToken);
        await oprot.Transport.FlushAsync(cancellationToken);
      }

      public async Task suspendMember_ProcessAsync(int seqid, TProtocol iprot, TProtocol oprot, CancellationToken cancellationToken)
      {
        var args = new suspendMemberArgs();
        await args.ReadAsync(iprot, cancellationToken);
        await iprot.ReadMessageEndAsync(cancellationToken);
        var result = new suspendMemberResult();
        try
        {
          result.Success = await _iAsync.suspendMemberAsync(args.ClusterId, args.MemberId, cancellationToken);
          await oprot.WriteMessageBeginAsync(new TMessage("suspendMember", TMessageType.Reply, seqid), cancellationToken); 
          await result.WriteAsync(oprot, cancellationToken);
        }
        catch (TTransportException)
        {
          throw;
        }
        catch (Exception ex)
        {
          Console.Error.WriteLine("Error occurred in processor:");
          Console.Error.WriteLine(ex.ToString());
          var x = new TApplicationException(TApplicationException.ExceptionType.InternalError," Internal error.");
          await oprot.WriteMessageBeginAsync(new TMessage("suspendMember", TMessageType.Exception, seqid), cancellationToken);
          await x.WriteAsync(oprot, cancellationToken);
        }
        await oprot.WriteMessageEndAsync(cancellationToken);
        await oprot.Transport.FlushAsync(cancellationToken);
      }

      public async Task resumeMember_ProcessAsync(int seqid, TProtocol iprot, TProtocol oprot, CancellationToken cancellationToken)
      {
        var args = new resumeMemberArgs();
        await args.ReadAsync(iprot, cancellationToken);
        await iprot.ReadMessageEndAsync(cancellationToken);
        var result = new resumeMemberResult();
        try
        {
          result.Success = await _iAsync.resumeMemberAsync(args.ClusterId, args.MemberId, cancellationToken);
          await oprot.WriteMessageBeginAsync(new TMessage("resumeMember", TMessageType.Reply, seqid), cancellationToken); 
          await result.WriteAsync(oprot, cancellationToken);
        }
        catch (TTransportException)
        {
          throw;
        }
        catch (Exception ex)
        {
          Console.Error.WriteLine("Error occurred in processor:");
          Console.Error.WriteLine(ex.ToString());
          var x = new TApplicationException(TApplicationException.ExceptionType.InternalError," Internal error.");
          await oprot.WriteMessageBeginAsync(new TMessage("resumeMember", TMessageType.Exception, seqid), cancellationToken);
          await x.WriteAsync(oprot, cancellationToken);
        }
        await oprot.WriteMessageEndAsync(cancellationToken);
        await oprot.Transport.FlushAsync(cancellationToken);
      }

      public async Task shutdownCluster_ProcessAsync(int seqid, TProtocol iprot, TProtocol oprot, CancellationToken cancellationToken)
      {
        var args = new shutdownClusterArgs();
        await args.ReadAsync(iprot, cancellationToken);
        await iprot.ReadMessageEndAsync(cancellationToken);
        var result = new shutdownClusterResult();
        try
        {
          result.Success = await _iAsync.shutdownClusterAsync(args.ClusterId, cancellationToken);
          await oprot.WriteMessageBeginAsync(new TMessage("shutdownCluster", TMessageType.Reply, seqid), cancellationToken); 
          await result.WriteAsync(oprot, cancellationToken);
        }
        catch (TTransportException)
        {
          throw;
        }
        catch (Exception ex)
        {
          Console.Error.WriteLine("Error occurred in processor:");
          Console.Error.WriteLine(ex.ToString());
          var x = new TApplicationException(TApplicationException.ExceptionType.InternalError," Internal error.");
          await oprot.WriteMessageBeginAsync(new TMessage("shutdownCluster", TMessageType.Exception, seqid), cancellationToken);
          await x.WriteAsync(oprot, cancellationToken);
        }
        await oprot.WriteMessageEndAsync(cancellationToken);
        await oprot.Transport.FlushAsync(cancellationToken);
      }

      public async Task terminateCluster_ProcessAsync(int seqid, TProtocol iprot, TProtocol oprot, CancellationToken cancellationToken)
      {
        var args = new terminateClusterArgs();
        await args.ReadAsync(iprot, cancellationToken);
        await iprot.ReadMessageEndAsync(cancellationToken);
        var result = new terminateClusterResult();
        try
        {
          result.Success = await _iAsync.terminateClusterAsync(args.ClusterId, cancellationToken);
          await oprot.WriteMessageBeginAsync(new TMessage("terminateCluster", TMessageType.Reply, seqid), cancellationToken); 
          await result.WriteAsync(oprot, cancellationToken);
        }
        catch (TTransportException)
        {
          throw;
        }
        catch (Exception ex)
        {
          Console.Error.WriteLine("Error occurred in processor:");
          Console.Error.WriteLine(ex.ToString());
          var x = new TApplicationException(TApplicationException.ExceptionType.InternalError," Internal error.");
          await oprot.WriteMessageBeginAsync(new TMessage("terminateCluster", TMessageType.Exception, seqid), cancellationToken);
          await x.WriteAsync(oprot, cancellationToken);
        }
        await oprot.WriteMessageEndAsync(cancellationToken);
        await oprot.Transport.FlushAsync(cancellationToken);
      }

      public async Task splitMemberFromCluster_ProcessAsync(int seqid, TProtocol iprot, TProtocol oprot, CancellationToken cancellationToken)
      {
        var args = new splitMemberFromClusterArgs();
        await args.ReadAsync(iprot, cancellationToken);
        await iprot.ReadMessageEndAsync(cancellationToken);
        var result = new splitMemberFromClusterResult();
        try
        {
          result.Success = await _iAsync.splitMemberFromClusterAsync(args.MemberId, cancellationToken);
          await oprot.WriteMessageBeginAsync(new TMessage("splitMemberFromCluster", TMessageType.Reply, seqid), cancellationToken); 
          await result.WriteAsync(oprot, cancellationToken);
        }
        catch (TTransportException)
        {
          throw;
        }
        catch (Exception ex)
        {
          Console.Error.WriteLine("Error occurred in processor:");
          Console.Error.WriteLine(ex.ToString());
          var x = new TApplicationException(TApplicationException.ExceptionType.InternalError," Internal error.");
          await oprot.WriteMessageBeginAsync(new TMessage("splitMemberFromCluster", TMessageType.Exception, seqid), cancellationToken);
          await x.WriteAsync(oprot, cancellationToken);
        }
        await oprot.WriteMessageEndAsync(cancellationToken);
        await oprot.Transport.FlushAsync(cancellationToken);
      }

      public async Task mergeMemberToCluster_ProcessAsync(int seqid, TProtocol iprot, TProtocol oprot, CancellationToken cancellationToken)
      {
        var args = new mergeMemberToClusterArgs();
        await args.ReadAsync(iprot, cancellationToken);
        await iprot.ReadMessageEndAsync(cancellationToken);
        var result = new mergeMemberToClusterResult();
        try
        {
          result.Success = await _iAsync.mergeMemberToClusterAsync(args.ClusterId, args.MemberId, cancellationToken);
          await oprot.WriteMessageBeginAsync(new TMessage("mergeMemberToCluster", TMessageType.Reply, seqid), cancellationToken); 
          await result.WriteAsync(oprot, cancellationToken);
        }
        catch (TTransportException)
        {
          throw;
        }
        catch (Exception ex)
        {
          Console.Error.WriteLine("Error occurred in processor:");
          Console.Error.WriteLine(ex.ToString());
          var x = new TApplicationException(TApplicationException.ExceptionType.InternalError," Internal error.");
          await oprot.WriteMessageBeginAsync(new TMessage("mergeMemberToCluster", TMessageType.Exception, seqid), cancellationToken);
          await x.WriteAsync(oprot, cancellationToken);
        }
        await oprot.WriteMessageEndAsync(cancellationToken);
        await oprot.Transport.FlushAsync(cancellationToken);
      }

      public async Task loginToCloudUsingEnvironment_ProcessAsync(int seqid, TProtocol iprot, TProtocol oprot, CancellationToken cancellationToken)
      {
        var args = new loginToCloudUsingEnvironmentArgs();
        await args.ReadAsync(iprot, cancellationToken);
        await iprot.ReadMessageEndAsync(cancellationToken);
        var result = new loginToCloudUsingEnvironmentResult();
        try
        {
          try
          {
            await _iAsync.loginToCloudUsingEnvironmentAsync(cancellationToken);
          }
          catch (CloudException cloudException)
          {
            result.CloudException = cloudException;
          }
          await oprot.WriteMessageBeginAsync(new TMessage("loginToCloudUsingEnvironment", TMessageType.Reply, seqid), cancellationToken); 
          await result.WriteAsync(oprot, cancellationToken);
        }
        catch (TTransportException)
        {
          throw;
        }
        catch (Exception ex)
        {
          Console.Error.WriteLine("Error occurred in processor:");
          Console.Error.WriteLine(ex.ToString());
          var x = new TApplicationException(TApplicationException.ExceptionType.InternalError," Internal error.");
          await oprot.WriteMessageBeginAsync(new TMessage("loginToCloudUsingEnvironment", TMessageType.Exception, seqid), cancellationToken);
          await x.WriteAsync(oprot, cancellationToken);
        }
        await oprot.WriteMessageEndAsync(cancellationToken);
        await oprot.Transport.FlushAsync(cancellationToken);
      }

      public async Task loginToCloud_ProcessAsync(int seqid, TProtocol iprot, TProtocol oprot, CancellationToken cancellationToken)
      {
        var args = new loginToCloudArgs();
        await args.ReadAsync(iprot, cancellationToken);
        await iprot.ReadMessageEndAsync(cancellationToken);
        var result = new loginToCloudResult();
        try
        {
          try
          {
            await _iAsync.loginToCloudAsync(args.BaseUrl, args.ApiKey, args.ApiSecret, cancellationToken);
          }
          catch (CloudException cloudException)
          {
            result.CloudException = cloudException;
          }
          await oprot.WriteMessageBeginAsync(new TMessage("loginToCloud", TMessageType.Reply, seqid), cancellationToken); 
          await result.WriteAsync(oprot, cancellationToken);
        }
        catch (TTransportException)
        {
          throw;
        }
        catch (Exception ex)
        {
          Console.Error.WriteLine("Error occurred in processor:");
          Console.Error.WriteLine(ex.ToString());
          var x = new TApplicationException(TApplicationException.ExceptionType.InternalError," Internal error.");
          await oprot.WriteMessageBeginAsync(new TMessage("loginToCloud", TMessageType.Exception, seqid), cancellationToken);
          await x.WriteAsync(oprot, cancellationToken);
        }
        await oprot.WriteMessageEndAsync(cancellationToken);
        await oprot.Transport.FlushAsync(cancellationToken);
      }

      public async Task createCloudCluster_ProcessAsync(int seqid, TProtocol iprot, TProtocol oprot, CancellationToken cancellationToken)
      {
        var args = new createCloudClusterArgs();
        await args.ReadAsync(iprot, cancellationToken);
        await iprot.ReadMessageEndAsync(cancellationToken);
        var result = new createCloudClusterResult();
        try
        {
          try
          {
            result.Success = await _iAsync.createCloudClusterAsync(args.HazelcastVersion, args.IsTlsEnabled, cancellationToken);
          }
          catch (CloudException cloudException)
          {
            result.CloudException = cloudException;
          }
          await oprot.WriteMessageBeginAsync(new TMessage("createCloudCluster", TMessageType.Reply, seqid), cancellationToken); 
          await result.WriteAsync(oprot, cancellationToken);
        }
        catch (TTransportException)
        {
          throw;
        }
        catch (Exception ex)
        {
          Console.Error.WriteLine("Error occurred in processor:");
          Console.Error.WriteLine(ex.ToString());
          var x = new TApplicationException(TApplicationException.ExceptionType.InternalError," Internal error.");
          await oprot.WriteMessageBeginAsync(new TMessage("createCloudCluster", TMessageType.Exception, seqid), cancellationToken);
          await x.WriteAsync(oprot, cancellationToken);
        }
        await oprot.WriteMessageEndAsync(cancellationToken);
        await oprot.Transport.FlushAsync(cancellationToken);
      }

      public async Task getCloudCluster_ProcessAsync(int seqid, TProtocol iprot, TProtocol oprot, CancellationToken cancellationToken)
      {
        var args = new getCloudClusterArgs();
        await args.ReadAsync(iprot, cancellationToken);
        await iprot.ReadMessageEndAsync(cancellationToken);
        var result = new getCloudClusterResult();
        try
        {
          try
          {
            result.Success = await _iAsync.getCloudClusterAsync(args.CloudClusterId, cancellationToken);
          }
          catch (CloudException cloudException)
          {
            result.CloudException = cloudException;
          }
          await oprot.WriteMessageBeginAsync(new TMessage("getCloudCluster", TMessageType.Reply, seqid), cancellationToken); 
          await result.WriteAsync(oprot, cancellationToken);
        }
        catch (TTransportException)
        {
          throw;
        }
        catch (Exception ex)
        {
          Console.Error.WriteLine("Error occurred in processor:");
          Console.Error.WriteLine(ex.ToString());
          var x = new TApplicationException(TApplicationException.ExceptionType.InternalError," Internal error.");
          await oprot.WriteMessageBeginAsync(new TMessage("getCloudCluster", TMessageType.Exception, seqid), cancellationToken);
          await x.WriteAsync(oprot, cancellationToken);
        }
        await oprot.WriteMessageEndAsync(cancellationToken);
        await oprot.Transport.FlushAsync(cancellationToken);
      }

      public async Task stopCloudCluster_ProcessAsync(int seqid, TProtocol iprot, TProtocol oprot, CancellationToken cancellationToken)
      {
        var args = new stopCloudClusterArgs();
        await args.ReadAsync(iprot, cancellationToken);
        await iprot.ReadMessageEndAsync(cancellationToken);
        var result = new stopCloudClusterResult();
        try
        {
          try
          {
            result.Success = await _iAsync.stopCloudClusterAsync(args.CloudClusterId, cancellationToken);
          }
          catch (CloudException cloudException)
          {
            result.CloudException = cloudException;
          }
          await oprot.WriteMessageBeginAsync(new TMessage("stopCloudCluster", TMessageType.Reply, seqid), cancellationToken); 
          await result.WriteAsync(oprot, cancellationToken);
        }
        catch (TTransportException)
        {
          throw;
        }
        catch (Exception ex)
        {
          Console.Error.WriteLine("Error occurred in processor:");
          Console.Error.WriteLine(ex.ToString());
          var x = new TApplicationException(TApplicationException.ExceptionType.InternalError," Internal error.");
          await oprot.WriteMessageBeginAsync(new TMessage("stopCloudCluster", TMessageType.Exception, seqid), cancellationToken);
          await x.WriteAsync(oprot, cancellationToken);
        }
        await oprot.WriteMessageEndAsync(cancellationToken);
        await oprot.Transport.FlushAsync(cancellationToken);
      }

      public async Task resumeCloudCluster_ProcessAsync(int seqid, TProtocol iprot, TProtocol oprot, CancellationToken cancellationToken)
      {
        var args = new resumeCloudClusterArgs();
        await args.ReadAsync(iprot, cancellationToken);
        await iprot.ReadMessageEndAsync(cancellationToken);
        var result = new resumeCloudClusterResult();
        try
        {
          try
          {
            result.Success = await _iAsync.resumeCloudClusterAsync(args.CloudClusterId, cancellationToken);
          }
          catch (CloudException cloudException)
          {
            result.CloudException = cloudException;
          }
          await oprot.WriteMessageBeginAsync(new TMessage("resumeCloudCluster", TMessageType.Reply, seqid), cancellationToken); 
          await result.WriteAsync(oprot, cancellationToken);
        }
        catch (TTransportException)
        {
          throw;
        }
        catch (Exception ex)
        {
          Console.Error.WriteLine("Error occurred in processor:");
          Console.Error.WriteLine(ex.ToString());
          var x = new TApplicationException(TApplicationException.ExceptionType.InternalError," Internal error.");
          await oprot.WriteMessageBeginAsync(new TMessage("resumeCloudCluster", TMessageType.Exception, seqid), cancellationToken);
          await x.WriteAsync(oprot, cancellationToken);
        }
        await oprot.WriteMessageEndAsync(cancellationToken);
        await oprot.Transport.FlushAsync(cancellationToken);
      }

      public async Task deleteCloudCluster_ProcessAsync(int seqid, TProtocol iprot, TProtocol oprot, CancellationToken cancellationToken)
      {
        var args = new deleteCloudClusterArgs();
        await args.ReadAsync(iprot, cancellationToken);
        await iprot.ReadMessageEndAsync(cancellationToken);
        var result = new deleteCloudClusterResult();
        try
        {
          try
          {
            await _iAsync.deleteCloudClusterAsync(args.CloudClusterId, cancellationToken);
          }
          catch (CloudException cloudException)
          {
            result.CloudException = cloudException;
          }
          await oprot.WriteMessageBeginAsync(new TMessage("deleteCloudCluster", TMessageType.Reply, seqid), cancellationToken); 
          await result.WriteAsync(oprot, cancellationToken);
        }
        catch (TTransportException)
        {
          throw;
        }
        catch (Exception ex)
        {
          Console.Error.WriteLine("Error occurred in processor:");
          Console.Error.WriteLine(ex.ToString());
          var x = new TApplicationException(TApplicationException.ExceptionType.InternalError," Internal error.");
          await oprot.WriteMessageBeginAsync(new TMessage("deleteCloudCluster", TMessageType.Exception, seqid), cancellationToken);
          await x.WriteAsync(oprot, cancellationToken);
        }
        await oprot.WriteMessageEndAsync(cancellationToken);
        await oprot.Transport.FlushAsync(cancellationToken);
      }

      public async Task executeOnController_ProcessAsync(int seqid, TProtocol iprot, TProtocol oprot, CancellationToken cancellationToken)
      {
        var args = new executeOnControllerArgs();
        await args.ReadAsync(iprot, cancellationToken);
        await iprot.ReadMessageEndAsync(cancellationToken);
        var result = new executeOnControllerResult();
        try
        {
          result.Success = await _iAsync.executeOnControllerAsync(args.ClusterId, args.Script, args.Lang, cancellationToken);
          await oprot.WriteMessageBeginAsync(new TMessage("executeOnController", TMessageType.Reply, seqid), cancellationToken); 
          await result.WriteAsync(oprot, cancellationToken);
        }
        catch (TTransportException)
        {
          throw;
        }
        catch (Exception ex)
        {
          Console.Error.WriteLine("Error occurred in processor:");
          Console.Error.WriteLine(ex.ToString());
          var x = new TApplicationException(TApplicationException.ExceptionType.InternalError," Internal error.");
          await oprot.WriteMessageBeginAsync(new TMessage("executeOnController", TMessageType.Exception, seqid), cancellationToken);
          await x.WriteAsync(oprot, cancellationToken);
        }
        await oprot.WriteMessageEndAsync(cancellationToken);
        await oprot.Transport.FlushAsync(cancellationToken);
      }

    }


    public partial class pingArgs : TBase
    {

      public pingArgs()
      {
      }

      public async Task ReadAsync(TProtocol iprot, CancellationToken cancellationToken)
      {
        iprot.IncrementRecursionDepth();
        try
        {
          TField field;
          await iprot.ReadStructBeginAsync(cancellationToken);
          while (true)
          {
            field = await iprot.ReadFieldBeginAsync(cancellationToken);
            if (field.Type == TType.Stop)
            {
              break;
            }

            switch (field.ID)
            {
              default: 
                await TProtocolUtil.SkipAsync(iprot, field.Type, cancellationToken);
                break;
            }

            await iprot.ReadFieldEndAsync(cancellationToken);
          }

          await iprot.ReadStructEndAsync(cancellationToken);
        }
        finally
        {
          iprot.DecrementRecursionDepth();
        }
      }

      public async Task WriteAsync(TProtocol oprot, CancellationToken cancellationToken)
      {
        oprot.IncrementRecursionDepth();
        try
        {
          var struc = new TStruct("ping_args");
          await oprot.WriteStructBeginAsync(struc, cancellationToken);
          await oprot.WriteFieldStopAsync(cancellationToken);
          await oprot.WriteStructEndAsync(cancellationToken);
        }
        finally
        {
          oprot.DecrementRecursionDepth();
        }
      }

      public override bool Equals(object that)
      {
        var other = that as pingArgs;
        if (other == null) return false;
        if (ReferenceEquals(this, other)) return true;
        return true;
      }

      public override int GetHashCode() {
        int hashcode = 157;
        unchecked {
        }
        return hashcode;
      }

      public override string ToString()
      {
        var sb = new StringBuilder("ping_args(");
        sb.Append(")");
        return sb.ToString();
      }
    }


    public partial class pingResult : TBase
    {
      private bool _success;

      public bool Success
      {
        get
        {
          return _success;
        }
        set
        {
          __isset.success = true;
          this._success = value;
        }
      }


      public Isset __isset;
      public struct Isset
      {
        public bool success;
      }

      public pingResult()
      {
      }

      public async Task ReadAsync(TProtocol iprot, CancellationToken cancellationToken)
      {
        iprot.IncrementRecursionDepth();
        try
        {
          TField field;
          await iprot.ReadStructBeginAsync(cancellationToken);
          while (true)
          {
            field = await iprot.ReadFieldBeginAsync(cancellationToken);
            if (field.Type == TType.Stop)
            {
              break;
            }

            switch (field.ID)
            {
              case 0:
                if (field.Type == TType.Bool)
                {
                  Success = await iprot.ReadBoolAsync(cancellationToken);
                }
                else
                {
                  await TProtocolUtil.SkipAsync(iprot, field.Type, cancellationToken);
                }
                break;
              default: 
                await TProtocolUtil.SkipAsync(iprot, field.Type, cancellationToken);
                break;
            }

            await iprot.ReadFieldEndAsync(cancellationToken);
          }

          await iprot.ReadStructEndAsync(cancellationToken);
        }
        finally
        {
          iprot.DecrementRecursionDepth();
        }
      }

      public async Task WriteAsync(TProtocol oprot, CancellationToken cancellationToken)
      {
        oprot.IncrementRecursionDepth();
        try
        {
          var struc = new TStruct("ping_result");
          await oprot.WriteStructBeginAsync(struc, cancellationToken);
          var field = new TField();

          if(this.__isset.success)
          {
            field.Name = "Success";
            field.Type = TType.Bool;
            field.ID = 0;
            await oprot.WriteFieldBeginAsync(field, cancellationToken);
            await oprot.WriteBoolAsync(Success, cancellationToken);
            await oprot.WriteFieldEndAsync(cancellationToken);
          }
          await oprot.WriteFieldStopAsync(cancellationToken);
          await oprot.WriteStructEndAsync(cancellationToken);
        }
        finally
        {
          oprot.DecrementRecursionDepth();
        }
      }

      public override bool Equals(object that)
      {
        var other = that as pingResult;
        if (other == null) return false;
        if (ReferenceEquals(this, other)) return true;
        return ((__isset.success == other.__isset.success) && ((!__isset.success) || (System.Object.Equals(Success, other.Success))));
      }

      public override int GetHashCode() {
        int hashcode = 157;
        unchecked {
          if(__isset.success)
            hashcode = (hashcode * 397) + Success.GetHashCode();
        }
        return hashcode;
      }

      public override string ToString()
      {
        var sb = new StringBuilder("ping_result(");
        bool __first = true;
        if (__isset.success)
        {
          if(!__first) { sb.Append(", "); }
          __first = false;
          sb.Append("Success: ");
          sb.Append(Success);
        }
        sb.Append(")");
        return sb.ToString();
      }
    }


    public partial class cleanArgs : TBase
    {

      public cleanArgs()
      {
      }

      public async Task ReadAsync(TProtocol iprot, CancellationToken cancellationToken)
      {
        iprot.IncrementRecursionDepth();
        try
        {
          TField field;
          await iprot.ReadStructBeginAsync(cancellationToken);
          while (true)
          {
            field = await iprot.ReadFieldBeginAsync(cancellationToken);
            if (field.Type == TType.Stop)
            {
              break;
            }

            switch (field.ID)
            {
              default: 
                await TProtocolUtil.SkipAsync(iprot, field.Type, cancellationToken);
                break;
            }

            await iprot.ReadFieldEndAsync(cancellationToken);
          }

          await iprot.ReadStructEndAsync(cancellationToken);
        }
        finally
        {
          iprot.DecrementRecursionDepth();
        }
      }

      public async Task WriteAsync(TProtocol oprot, CancellationToken cancellationToken)
      {
        oprot.IncrementRecursionDepth();
        try
        {
          var struc = new TStruct("clean_args");
          await oprot.WriteStructBeginAsync(struc, cancellationToken);
          await oprot.WriteFieldStopAsync(cancellationToken);
          await oprot.WriteStructEndAsync(cancellationToken);
        }
        finally
        {
          oprot.DecrementRecursionDepth();
        }
      }

      public override bool Equals(object that)
      {
        var other = that as cleanArgs;
        if (other == null) return false;
        if (ReferenceEquals(this, other)) return true;
        return true;
      }

      public override int GetHashCode() {
        int hashcode = 157;
        unchecked {
        }
        return hashcode;
      }

      public override string ToString()
      {
        var sb = new StringBuilder("clean_args(");
        sb.Append(")");
        return sb.ToString();
      }
    }


    public partial class cleanResult : TBase
    {
      private bool _success;

      public bool Success
      {
        get
        {
          return _success;
        }
        set
        {
          __isset.success = true;
          this._success = value;
        }
      }


      public Isset __isset;
      public struct Isset
      {
        public bool success;
      }

      public cleanResult()
      {
      }

      public async Task ReadAsync(TProtocol iprot, CancellationToken cancellationToken)
      {
        iprot.IncrementRecursionDepth();
        try
        {
          TField field;
          await iprot.ReadStructBeginAsync(cancellationToken);
          while (true)
          {
            field = await iprot.ReadFieldBeginAsync(cancellationToken);
            if (field.Type == TType.Stop)
            {
              break;
            }

            switch (field.ID)
            {
              case 0:
                if (field.Type == TType.Bool)
                {
                  Success = await iprot.ReadBoolAsync(cancellationToken);
                }
                else
                {
                  await TProtocolUtil.SkipAsync(iprot, field.Type, cancellationToken);
                }
                break;
              default: 
                await TProtocolUtil.SkipAsync(iprot, field.Type, cancellationToken);
                break;
            }

            await iprot.ReadFieldEndAsync(cancellationToken);
          }

          await iprot.ReadStructEndAsync(cancellationToken);
        }
        finally
        {
          iprot.DecrementRecursionDepth();
        }
      }

      public async Task WriteAsync(TProtocol oprot, CancellationToken cancellationToken)
      {
        oprot.IncrementRecursionDepth();
        try
        {
          var struc = new TStruct("clean_result");
          await oprot.WriteStructBeginAsync(struc, cancellationToken);
          var field = new TField();

          if(this.__isset.success)
          {
            field.Name = "Success";
            field.Type = TType.Bool;
            field.ID = 0;
            await oprot.WriteFieldBeginAsync(field, cancellationToken);
            await oprot.WriteBoolAsync(Success, cancellationToken);
            await oprot.WriteFieldEndAsync(cancellationToken);
          }
          await oprot.WriteFieldStopAsync(cancellationToken);
          await oprot.WriteStructEndAsync(cancellationToken);
        }
        finally
        {
          oprot.DecrementRecursionDepth();
        }
      }

      public override bool Equals(object that)
      {
        var other = that as cleanResult;
        if (other == null) return false;
        if (ReferenceEquals(this, other)) return true;
        return ((__isset.success == other.__isset.success) && ((!__isset.success) || (System.Object.Equals(Success, other.Success))));
      }

      public override int GetHashCode() {
        int hashcode = 157;
        unchecked {
          if(__isset.success)
            hashcode = (hashcode * 397) + Success.GetHashCode();
        }
        return hashcode;
      }

      public override string ToString()
      {
        var sb = new StringBuilder("clean_result(");
        bool __first = true;
        if (__isset.success)
        {
          if(!__first) { sb.Append(", "); }
          __first = false;
          sb.Append("Success: ");
          sb.Append(Success);
        }
        sb.Append(")");
        return sb.ToString();
      }
    }


    public partial class exitArgs : TBase
    {

      public exitArgs()
      {
      }

      public async Task ReadAsync(TProtocol iprot, CancellationToken cancellationToken)
      {
        iprot.IncrementRecursionDepth();
        try
        {
          TField field;
          await iprot.ReadStructBeginAsync(cancellationToken);
          while (true)
          {
            field = await iprot.ReadFieldBeginAsync(cancellationToken);
            if (field.Type == TType.Stop)
            {
              break;
            }

            switch (field.ID)
            {
              default: 
                await TProtocolUtil.SkipAsync(iprot, field.Type, cancellationToken);
                break;
            }

            await iprot.ReadFieldEndAsync(cancellationToken);
          }

          await iprot.ReadStructEndAsync(cancellationToken);
        }
        finally
        {
          iprot.DecrementRecursionDepth();
        }
      }

      public async Task WriteAsync(TProtocol oprot, CancellationToken cancellationToken)
      {
        oprot.IncrementRecursionDepth();
        try
        {
          var struc = new TStruct("exit_args");
          await oprot.WriteStructBeginAsync(struc, cancellationToken);
          await oprot.WriteFieldStopAsync(cancellationToken);
          await oprot.WriteStructEndAsync(cancellationToken);
        }
        finally
        {
          oprot.DecrementRecursionDepth();
        }
      }

      public override bool Equals(object that)
      {
        var other = that as exitArgs;
        if (other == null) return false;
        if (ReferenceEquals(this, other)) return true;
        return true;
      }

      public override int GetHashCode() {
        int hashcode = 157;
        unchecked {
        }
        return hashcode;
      }

      public override string ToString()
      {
        var sb = new StringBuilder("exit_args(");
        sb.Append(")");
        return sb.ToString();
      }
    }


    public partial class exitResult : TBase
    {
      private bool _success;

      public bool Success
      {
        get
        {
          return _success;
        }
        set
        {
          __isset.success = true;
          this._success = value;
        }
      }


      public Isset __isset;
      public struct Isset
      {
        public bool success;
      }

      public exitResult()
      {
      }

      public async Task ReadAsync(TProtocol iprot, CancellationToken cancellationToken)
      {
        iprot.IncrementRecursionDepth();
        try
        {
          TField field;
          await iprot.ReadStructBeginAsync(cancellationToken);
          while (true)
          {
            field = await iprot.ReadFieldBeginAsync(cancellationToken);
            if (field.Type == TType.Stop)
            {
              break;
            }

            switch (field.ID)
            {
              case 0:
                if (field.Type == TType.Bool)
                {
                  Success = await iprot.ReadBoolAsync(cancellationToken);
                }
                else
                {
                  await TProtocolUtil.SkipAsync(iprot, field.Type, cancellationToken);
                }
                break;
              default: 
                await TProtocolUtil.SkipAsync(iprot, field.Type, cancellationToken);
                break;
            }

            await iprot.ReadFieldEndAsync(cancellationToken);
          }

          await iprot.ReadStructEndAsync(cancellationToken);
        }
        finally
        {
          iprot.DecrementRecursionDepth();
        }
      }

      public async Task WriteAsync(TProtocol oprot, CancellationToken cancellationToken)
      {
        oprot.IncrementRecursionDepth();
        try
        {
          var struc = new TStruct("exit_result");
          await oprot.WriteStructBeginAsync(struc, cancellationToken);
          var field = new TField();

          if(this.__isset.success)
          {
            field.Name = "Success";
            field.Type = TType.Bool;
            field.ID = 0;
            await oprot.WriteFieldBeginAsync(field, cancellationToken);
            await oprot.WriteBoolAsync(Success, cancellationToken);
            await oprot.WriteFieldEndAsync(cancellationToken);
          }
          await oprot.WriteFieldStopAsync(cancellationToken);
          await oprot.WriteStructEndAsync(cancellationToken);
        }
        finally
        {
          oprot.DecrementRecursionDepth();
        }
      }

      public override bool Equals(object that)
      {
        var other = that as exitResult;
        if (other == null) return false;
        if (ReferenceEquals(this, other)) return true;
        return ((__isset.success == other.__isset.success) && ((!__isset.success) || (System.Object.Equals(Success, other.Success))));
      }

      public override int GetHashCode() {
        int hashcode = 157;
        unchecked {
          if(__isset.success)
            hashcode = (hashcode * 397) + Success.GetHashCode();
        }
        return hashcode;
      }

      public override string ToString()
      {
        var sb = new StringBuilder("exit_result(");
        bool __first = true;
        if (__isset.success)
        {
          if(!__first) { sb.Append(", "); }
          __first = false;
          sb.Append("Success: ");
          sb.Append(Success);
        }
        sb.Append(")");
        return sb.ToString();
      }
    }


    public partial class createClusterArgs : TBase
    {
      private string _hzVersion;
      private string _xmlconfig;

      public string HzVersion
      {
        get
        {
          return _hzVersion;
        }
        set
        {
          __isset.hzVersion = true;
          this._hzVersion = value;
        }
      }

      public string Xmlconfig
      {
        get
        {
          return _xmlconfig;
        }
        set
        {
          __isset.xmlconfig = true;
          this._xmlconfig = value;
        }
      }


      public Isset __isset;
      public struct Isset
      {
        public bool hzVersion;
        public bool xmlconfig;
      }

      public createClusterArgs()
      {
      }

      public async Task ReadAsync(TProtocol iprot, CancellationToken cancellationToken)
      {
        iprot.IncrementRecursionDepth();
        try
        {
          TField field;
          await iprot.ReadStructBeginAsync(cancellationToken);
          while (true)
          {
            field = await iprot.ReadFieldBeginAsync(cancellationToken);
            if (field.Type == TType.Stop)
            {
              break;
            }

            switch (field.ID)
            {
              case 1:
                if (field.Type == TType.String)
                {
                  HzVersion = await iprot.ReadStringAsync(cancellationToken);
                }
                else
                {
                  await TProtocolUtil.SkipAsync(iprot, field.Type, cancellationToken);
                }
                break;
              case 2:
                if (field.Type == TType.String)
                {
                  Xmlconfig = await iprot.ReadStringAsync(cancellationToken);
                }
                else
                {
                  await TProtocolUtil.SkipAsync(iprot, field.Type, cancellationToken);
                }
                break;
              default: 
                await TProtocolUtil.SkipAsync(iprot, field.Type, cancellationToken);
                break;
            }

            await iprot.ReadFieldEndAsync(cancellationToken);
          }

          await iprot.ReadStructEndAsync(cancellationToken);
        }
        finally
        {
          iprot.DecrementRecursionDepth();
        }
      }

      public async Task WriteAsync(TProtocol oprot, CancellationToken cancellationToken)
      {
        oprot.IncrementRecursionDepth();
        try
        {
          var struc = new TStruct("createCluster_args");
          await oprot.WriteStructBeginAsync(struc, cancellationToken);
          var field = new TField();
          if (HzVersion != null && __isset.hzVersion)
          {
            field.Name = "hzVersion";
            field.Type = TType.String;
            field.ID = 1;
            await oprot.WriteFieldBeginAsync(field, cancellationToken);
            await oprot.WriteStringAsync(HzVersion, cancellationToken);
            await oprot.WriteFieldEndAsync(cancellationToken);
          }
          if (Xmlconfig != null && __isset.xmlconfig)
          {
            field.Name = "xmlconfig";
            field.Type = TType.String;
            field.ID = 2;
            await oprot.WriteFieldBeginAsync(field, cancellationToken);
            await oprot.WriteStringAsync(Xmlconfig, cancellationToken);
            await oprot.WriteFieldEndAsync(cancellationToken);
          }
          await oprot.WriteFieldStopAsync(cancellationToken);
          await oprot.WriteStructEndAsync(cancellationToken);
        }
        finally
        {
          oprot.DecrementRecursionDepth();
        }
      }

      public override bool Equals(object that)
      {
        var other = that as createClusterArgs;
        if (other == null) return false;
        if (ReferenceEquals(this, other)) return true;
        return ((__isset.hzVersion == other.__isset.hzVersion) && ((!__isset.hzVersion) || (System.Object.Equals(HzVersion, other.HzVersion))))
          && ((__isset.xmlconfig == other.__isset.xmlconfig) && ((!__isset.xmlconfig) || (System.Object.Equals(Xmlconfig, other.Xmlconfig))));
      }

      public override int GetHashCode() {
        int hashcode = 157;
        unchecked {
          if(__isset.hzVersion)
            hashcode = (hashcode * 397) + HzVersion.GetHashCode();
          if(__isset.xmlconfig)
            hashcode = (hashcode * 397) + Xmlconfig.GetHashCode();
        }
        return hashcode;
      }

      public override string ToString()
      {
        var sb = new StringBuilder("createCluster_args(");
        bool __first = true;
        if (HzVersion != null && __isset.hzVersion)
        {
          if(!__first) { sb.Append(", "); }
          __first = false;
          sb.Append("HzVersion: ");
          sb.Append(HzVersion);
        }
        if (Xmlconfig != null && __isset.xmlconfig)
        {
          if(!__first) { sb.Append(", "); }
          __first = false;
          sb.Append("Xmlconfig: ");
          sb.Append(Xmlconfig);
        }
        sb.Append(")");
        return sb.ToString();
      }
    }


    public partial class createClusterResult : TBase
    {
      private Cluster _success;
      private ServerException _serverException;

      public Cluster Success
      {
        get
        {
          return _success;
        }
        set
        {
          __isset.success = true;
          this._success = value;
        }
      }

      public ServerException ServerException
      {
        get
        {
          return _serverException;
        }
        set
        {
          __isset.serverException = true;
          this._serverException = value;
        }
      }


      public Isset __isset;
      public struct Isset
      {
        public bool success;
        public bool serverException;
      }

      public createClusterResult()
      {
      }

      public async Task ReadAsync(TProtocol iprot, CancellationToken cancellationToken)
      {
        iprot.IncrementRecursionDepth();
        try
        {
          TField field;
          await iprot.ReadStructBeginAsync(cancellationToken);
          while (true)
          {
            field = await iprot.ReadFieldBeginAsync(cancellationToken);
            if (field.Type == TType.Stop)
            {
              break;
            }

            switch (field.ID)
            {
              case 0:
                if (field.Type == TType.Struct)
                {
                  Success = new Cluster();
                  await Success.ReadAsync(iprot, cancellationToken);
                }
                else
                {
                  await TProtocolUtil.SkipAsync(iprot, field.Type, cancellationToken);
                }
                break;
              case 1:
                if (field.Type == TType.Struct)
                {
                  ServerException = new ServerException();
                  await ServerException.ReadAsync(iprot, cancellationToken);
                }
                else
                {
                  await TProtocolUtil.SkipAsync(iprot, field.Type, cancellationToken);
                }
                break;
              default: 
                await TProtocolUtil.SkipAsync(iprot, field.Type, cancellationToken);
                break;
            }

            await iprot.ReadFieldEndAsync(cancellationToken);
          }

          await iprot.ReadStructEndAsync(cancellationToken);
        }
        finally
        {
          iprot.DecrementRecursionDepth();
        }
      }

      public async Task WriteAsync(TProtocol oprot, CancellationToken cancellationToken)
      {
        oprot.IncrementRecursionDepth();
        try
        {
          var struc = new TStruct("createCluster_result");
          await oprot.WriteStructBeginAsync(struc, cancellationToken);
          var field = new TField();

          if(this.__isset.success)
          {
            if (Success != null)
            {
              field.Name = "Success";
              field.Type = TType.Struct;
              field.ID = 0;
              await oprot.WriteFieldBeginAsync(field, cancellationToken);
              await Success.WriteAsync(oprot, cancellationToken);
              await oprot.WriteFieldEndAsync(cancellationToken);
            }
          }
          else if(this.__isset.serverException)
          {
            if (ServerException != null)
            {
              field.Name = "ServerException";
              field.Type = TType.Struct;
              field.ID = 1;
              await oprot.WriteFieldBeginAsync(field, cancellationToken);
              await ServerException.WriteAsync(oprot, cancellationToken);
              await oprot.WriteFieldEndAsync(cancellationToken);
            }
          }
          await oprot.WriteFieldStopAsync(cancellationToken);
          await oprot.WriteStructEndAsync(cancellationToken);
        }
        finally
        {
          oprot.DecrementRecursionDepth();
        }
      }

      public override bool Equals(object that)
      {
        var other = that as createClusterResult;
        if (other == null) return false;
        if (ReferenceEquals(this, other)) return true;
        return ((__isset.success == other.__isset.success) && ((!__isset.success) || (System.Object.Equals(Success, other.Success))))
          && ((__isset.serverException == other.__isset.serverException) && ((!__isset.serverException) || (System.Object.Equals(ServerException, other.ServerException))));
      }

      public override int GetHashCode() {
        int hashcode = 157;
        unchecked {
          if(__isset.success)
            hashcode = (hashcode * 397) + Success.GetHashCode();
          if(__isset.serverException)
            hashcode = (hashcode * 397) + ServerException.GetHashCode();
        }
        return hashcode;
      }

      public override string ToString()
      {
        var sb = new StringBuilder("createCluster_result(");
        bool __first = true;
        if (Success != null && __isset.success)
        {
          if(!__first) { sb.Append(", "); }
          __first = false;
          sb.Append("Success: ");
          sb.Append(Success== null ? "<null>" : Success.ToString());
        }
        if (ServerException != null && __isset.serverException)
        {
          if(!__first) { sb.Append(", "); }
          __first = false;
          sb.Append("ServerException: ");
          sb.Append(ServerException== null ? "<null>" : ServerException.ToString());
        }
        sb.Append(")");
        return sb.ToString();
      }
    }


    public partial class createClusterKeepClusterNameArgs : TBase
    {
      private string _hzVersion;
      private string _xmlconfig;

      public string HzVersion
      {
        get
        {
          return _hzVersion;
        }
        set
        {
          __isset.hzVersion = true;
          this._hzVersion = value;
        }
      }

      public string Xmlconfig
      {
        get
        {
          return _xmlconfig;
        }
        set
        {
          __isset.xmlconfig = true;
          this._xmlconfig = value;
        }
      }


      public Isset __isset;
      public struct Isset
      {
        public bool hzVersion;
        public bool xmlconfig;
      }

      public createClusterKeepClusterNameArgs()
      {
      }

      public async Task ReadAsync(TProtocol iprot, CancellationToken cancellationToken)
      {
        iprot.IncrementRecursionDepth();
        try
        {
          TField field;
          await iprot.ReadStructBeginAsync(cancellationToken);
          while (true)
          {
            field = await iprot.ReadFieldBeginAsync(cancellationToken);
            if (field.Type == TType.Stop)
            {
              break;
            }

            switch (field.ID)
            {
              case 1:
                if (field.Type == TType.String)
                {
                  HzVersion = await iprot.ReadStringAsync(cancellationToken);
                }
                else
                {
                  await TProtocolUtil.SkipAsync(iprot, field.Type, cancellationToken);
                }
                break;
              case 2:
                if (field.Type == TType.String)
                {
                  Xmlconfig = await iprot.ReadStringAsync(cancellationToken);
                }
                else
                {
                  await TProtocolUtil.SkipAsync(iprot, field.Type, cancellationToken);
                }
                break;
              default: 
                await TProtocolUtil.SkipAsync(iprot, field.Type, cancellationToken);
                break;
            }

            await iprot.ReadFieldEndAsync(cancellationToken);
          }

          await iprot.ReadStructEndAsync(cancellationToken);
        }
        finally
        {
          iprot.DecrementRecursionDepth();
        }
      }

      public async Task WriteAsync(TProtocol oprot, CancellationToken cancellationToken)
      {
        oprot.IncrementRecursionDepth();
        try
        {
          var struc = new TStruct("createClusterKeepClusterName_args");
          await oprot.WriteStructBeginAsync(struc, cancellationToken);
          var field = new TField();
          if (HzVersion != null && __isset.hzVersion)
          {
            field.Name = "hzVersion";
            field.Type = TType.String;
            field.ID = 1;
            await oprot.WriteFieldBeginAsync(field, cancellationToken);
            await oprot.WriteStringAsync(HzVersion, cancellationToken);
            await oprot.WriteFieldEndAsync(cancellationToken);
          }
          if (Xmlconfig != null && __isset.xmlconfig)
          {
            field.Name = "xmlconfig";
            field.Type = TType.String;
            field.ID = 2;
            await oprot.WriteFieldBeginAsync(field, cancellationToken);
            await oprot.WriteStringAsync(Xmlconfig, cancellationToken);
            await oprot.WriteFieldEndAsync(cancellationToken);
          }
          await oprot.WriteFieldStopAsync(cancellationToken);
          await oprot.WriteStructEndAsync(cancellationToken);
        }
        finally
        {
          oprot.DecrementRecursionDepth();
        }
      }

      public override bool Equals(object that)
      {
        var other = that as createClusterKeepClusterNameArgs;
        if (other == null) return false;
        if (ReferenceEquals(this, other)) return true;
        return ((__isset.hzVersion == other.__isset.hzVersion) && ((!__isset.hzVersion) || (System.Object.Equals(HzVersion, other.HzVersion))))
          && ((__isset.xmlconfig == other.__isset.xmlconfig) && ((!__isset.xmlconfig) || (System.Object.Equals(Xmlconfig, other.Xmlconfig))));
      }

      public override int GetHashCode() {
        int hashcode = 157;
        unchecked {
          if(__isset.hzVersion)
            hashcode = (hashcode * 397) + HzVersion.GetHashCode();
          if(__isset.xmlconfig)
            hashcode = (hashcode * 397) + Xmlconfig.GetHashCode();
        }
        return hashcode;
      }

      public override string ToString()
      {
        var sb = new StringBuilder("createClusterKeepClusterName_args(");
        bool __first = true;
        if (HzVersion != null && __isset.hzVersion)
        {
          if(!__first) { sb.Append(", "); }
          __first = false;
          sb.Append("HzVersion: ");
          sb.Append(HzVersion);
        }
        if (Xmlconfig != null && __isset.xmlconfig)
        {
          if(!__first) { sb.Append(", "); }
          __first = false;
          sb.Append("Xmlconfig: ");
          sb.Append(Xmlconfig);
        }
        sb.Append(")");
        return sb.ToString();
      }
    }


    public partial class createClusterKeepClusterNameResult : TBase
    {
      private Cluster _success;
      private ServerException _serverException;

      public Cluster Success
      {
        get
        {
          return _success;
        }
        set
        {
          __isset.success = true;
          this._success = value;
        }
      }

      public ServerException ServerException
      {
        get
        {
          return _serverException;
        }
        set
        {
          __isset.serverException = true;
          this._serverException = value;
        }
      }


      public Isset __isset;
      public struct Isset
      {
        public bool success;
        public bool serverException;
      }

      public createClusterKeepClusterNameResult()
      {
      }

      public async Task ReadAsync(TProtocol iprot, CancellationToken cancellationToken)
      {
        iprot.IncrementRecursionDepth();
        try
        {
          TField field;
          await iprot.ReadStructBeginAsync(cancellationToken);
          while (true)
          {
            field = await iprot.ReadFieldBeginAsync(cancellationToken);
            if (field.Type == TType.Stop)
            {
              break;
            }

            switch (field.ID)
            {
              case 0:
                if (field.Type == TType.Struct)
                {
                  Success = new Cluster();
                  await Success.ReadAsync(iprot, cancellationToken);
                }
                else
                {
                  await TProtocolUtil.SkipAsync(iprot, field.Type, cancellationToken);
                }
                break;
              case 1:
                if (field.Type == TType.Struct)
                {
                  ServerException = new ServerException();
                  await ServerException.ReadAsync(iprot, cancellationToken);
                }
                else
                {
                  await TProtocolUtil.SkipAsync(iprot, field.Type, cancellationToken);
                }
                break;
              default: 
                await TProtocolUtil.SkipAsync(iprot, field.Type, cancellationToken);
                break;
            }

            await iprot.ReadFieldEndAsync(cancellationToken);
          }

          await iprot.ReadStructEndAsync(cancellationToken);
        }
        finally
        {
          iprot.DecrementRecursionDepth();
        }
      }

      public async Task WriteAsync(TProtocol oprot, CancellationToken cancellationToken)
      {
        oprot.IncrementRecursionDepth();
        try
        {
          var struc = new TStruct("createClusterKeepClusterName_result");
          await oprot.WriteStructBeginAsync(struc, cancellationToken);
          var field = new TField();

          if(this.__isset.success)
          {
            if (Success != null)
            {
              field.Name = "Success";
              field.Type = TType.Struct;
              field.ID = 0;
              await oprot.WriteFieldBeginAsync(field, cancellationToken);
              await Success.WriteAsync(oprot, cancellationToken);
              await oprot.WriteFieldEndAsync(cancellationToken);
            }
          }
          else if(this.__isset.serverException)
          {
            if (ServerException != null)
            {
              field.Name = "ServerException";
              field.Type = TType.Struct;
              field.ID = 1;
              await oprot.WriteFieldBeginAsync(field, cancellationToken);
              await ServerException.WriteAsync(oprot, cancellationToken);
              await oprot.WriteFieldEndAsync(cancellationToken);
            }
          }
          await oprot.WriteFieldStopAsync(cancellationToken);
          await oprot.WriteStructEndAsync(cancellationToken);
        }
        finally
        {
          oprot.DecrementRecursionDepth();
        }
      }

      public override bool Equals(object that)
      {
        var other = that as createClusterKeepClusterNameResult;
        if (other == null) return false;
        if (ReferenceEquals(this, other)) return true;
        return ((__isset.success == other.__isset.success) && ((!__isset.success) || (System.Object.Equals(Success, other.Success))))
          && ((__isset.serverException == other.__isset.serverException) && ((!__isset.serverException) || (System.Object.Equals(ServerException, other.ServerException))));
      }

      public override int GetHashCode() {
        int hashcode = 157;
        unchecked {
          if(__isset.success)
            hashcode = (hashcode * 397) + Success.GetHashCode();
          if(__isset.serverException)
            hashcode = (hashcode * 397) + ServerException.GetHashCode();
        }
        return hashcode;
      }

      public override string ToString()
      {
        var sb = new StringBuilder("createClusterKeepClusterName_result(");
        bool __first = true;
        if (Success != null && __isset.success)
        {
          if(!__first) { sb.Append(", "); }
          __first = false;
          sb.Append("Success: ");
          sb.Append(Success== null ? "<null>" : Success.ToString());
        }
        if (ServerException != null && __isset.serverException)
        {
          if(!__first) { sb.Append(", "); }
          __first = false;
          sb.Append("ServerException: ");
          sb.Append(ServerException== null ? "<null>" : ServerException.ToString());
        }
        sb.Append(")");
        return sb.ToString();
      }
    }


    public partial class startMemberArgs : TBase
    {
      private string _clusterId;

      public string ClusterId
      {
        get
        {
          return _clusterId;
        }
        set
        {
          __isset.clusterId = true;
          this._clusterId = value;
        }
      }


      public Isset __isset;
      public struct Isset
      {
        public bool clusterId;
      }

      public startMemberArgs()
      {
      }

      public async Task ReadAsync(TProtocol iprot, CancellationToken cancellationToken)
      {
        iprot.IncrementRecursionDepth();
        try
        {
          TField field;
          await iprot.ReadStructBeginAsync(cancellationToken);
          while (true)
          {
            field = await iprot.ReadFieldBeginAsync(cancellationToken);
            if (field.Type == TType.Stop)
            {
              break;
            }

            switch (field.ID)
            {
              case 1:
                if (field.Type == TType.String)
                {
                  ClusterId = await iprot.ReadStringAsync(cancellationToken);
                }
                else
                {
                  await TProtocolUtil.SkipAsync(iprot, field.Type, cancellationToken);
                }
                break;
              default: 
                await TProtocolUtil.SkipAsync(iprot, field.Type, cancellationToken);
                break;
            }

            await iprot.ReadFieldEndAsync(cancellationToken);
          }

          await iprot.ReadStructEndAsync(cancellationToken);
        }
        finally
        {
          iprot.DecrementRecursionDepth();
        }
      }

      public async Task WriteAsync(TProtocol oprot, CancellationToken cancellationToken)
      {
        oprot.IncrementRecursionDepth();
        try
        {
          var struc = new TStruct("startMember_args");
          await oprot.WriteStructBeginAsync(struc, cancellationToken);
          var field = new TField();
          if (ClusterId != null && __isset.clusterId)
          {
            field.Name = "clusterId";
            field.Type = TType.String;
            field.ID = 1;
            await oprot.WriteFieldBeginAsync(field, cancellationToken);
            await oprot.WriteStringAsync(ClusterId, cancellationToken);
            await oprot.WriteFieldEndAsync(cancellationToken);
          }
          await oprot.WriteFieldStopAsync(cancellationToken);
          await oprot.WriteStructEndAsync(cancellationToken);
        }
        finally
        {
          oprot.DecrementRecursionDepth();
        }
      }

      public override bool Equals(object that)
      {
        var other = that as startMemberArgs;
        if (other == null) return false;
        if (ReferenceEquals(this, other)) return true;
        return ((__isset.clusterId == other.__isset.clusterId) && ((!__isset.clusterId) || (System.Object.Equals(ClusterId, other.ClusterId))));
      }

      public override int GetHashCode() {
        int hashcode = 157;
        unchecked {
          if(__isset.clusterId)
            hashcode = (hashcode * 397) + ClusterId.GetHashCode();
        }
        return hashcode;
      }

      public override string ToString()
      {
        var sb = new StringBuilder("startMember_args(");
        bool __first = true;
        if (ClusterId != null && __isset.clusterId)
        {
          if(!__first) { sb.Append(", "); }
          __first = false;
          sb.Append("ClusterId: ");
          sb.Append(ClusterId);
        }
        sb.Append(")");
        return sb.ToString();
      }
    }


    public partial class startMemberResult : TBase
    {
      private Member _success;
      private ServerException _serverException;

      public Member Success
      {
        get
        {
          return _success;
        }
        set
        {
          __isset.success = true;
          this._success = value;
        }
      }

      public ServerException ServerException
      {
        get
        {
          return _serverException;
        }
        set
        {
          __isset.serverException = true;
          this._serverException = value;
        }
      }


      public Isset __isset;
      public struct Isset
      {
        public bool success;
        public bool serverException;
      }

      public startMemberResult()
      {
      }

      public async Task ReadAsync(TProtocol iprot, CancellationToken cancellationToken)
      {
        iprot.IncrementRecursionDepth();
        try
        {
          TField field;
          await iprot.ReadStructBeginAsync(cancellationToken);
          while (true)
          {
            field = await iprot.ReadFieldBeginAsync(cancellationToken);
            if (field.Type == TType.Stop)
            {
              break;
            }

            switch (field.ID)
            {
              case 0:
                if (field.Type == TType.Struct)
                {
                  Success = new Member();
                  await Success.ReadAsync(iprot, cancellationToken);
                }
                else
                {
                  await TProtocolUtil.SkipAsync(iprot, field.Type, cancellationToken);
                }
                break;
              case 1:
                if (field.Type == TType.Struct)
                {
                  ServerException = new ServerException();
                  await ServerException.ReadAsync(iprot, cancellationToken);
                }
                else
                {
                  await TProtocolUtil.SkipAsync(iprot, field.Type, cancellationToken);
                }
                break;
              default: 
                await TProtocolUtil.SkipAsync(iprot, field.Type, cancellationToken);
                break;
            }

            await iprot.ReadFieldEndAsync(cancellationToken);
          }

          await iprot.ReadStructEndAsync(cancellationToken);
        }
        finally
        {
          iprot.DecrementRecursionDepth();
        }
      }

      public async Task WriteAsync(TProtocol oprot, CancellationToken cancellationToken)
      {
        oprot.IncrementRecursionDepth();
        try
        {
          var struc = new TStruct("startMember_result");
          await oprot.WriteStructBeginAsync(struc, cancellationToken);
          var field = new TField();

          if(this.__isset.success)
          {
            if (Success != null)
            {
              field.Name = "Success";
              field.Type = TType.Struct;
              field.ID = 0;
              await oprot.WriteFieldBeginAsync(field, cancellationToken);
              await Success.WriteAsync(oprot, cancellationToken);
              await oprot.WriteFieldEndAsync(cancellationToken);
            }
          }
          else if(this.__isset.serverException)
          {
            if (ServerException != null)
            {
              field.Name = "ServerException";
              field.Type = TType.Struct;
              field.ID = 1;
              await oprot.WriteFieldBeginAsync(field, cancellationToken);
              await ServerException.WriteAsync(oprot, cancellationToken);
              await oprot.WriteFieldEndAsync(cancellationToken);
            }
          }
          await oprot.WriteFieldStopAsync(cancellationToken);
          await oprot.WriteStructEndAsync(cancellationToken);
        }
        finally
        {
          oprot.DecrementRecursionDepth();
        }
      }

      public override bool Equals(object that)
      {
        var other = that as startMemberResult;
        if (other == null) return false;
        if (ReferenceEquals(this, other)) return true;
        return ((__isset.success == other.__isset.success) && ((!__isset.success) || (System.Object.Equals(Success, other.Success))))
          && ((__isset.serverException == other.__isset.serverException) && ((!__isset.serverException) || (System.Object.Equals(ServerException, other.ServerException))));
      }

      public override int GetHashCode() {
        int hashcode = 157;
        unchecked {
          if(__isset.success)
            hashcode = (hashcode * 397) + Success.GetHashCode();
          if(__isset.serverException)
            hashcode = (hashcode * 397) + ServerException.GetHashCode();
        }
        return hashcode;
      }

      public override string ToString()
      {
        var sb = new StringBuilder("startMember_result(");
        bool __first = true;
        if (Success != null && __isset.success)
        {
          if(!__first) { sb.Append(", "); }
          __first = false;
          sb.Append("Success: ");
          sb.Append(Success== null ? "<null>" : Success.ToString());
        }
        if (ServerException != null && __isset.serverException)
        {
          if(!__first) { sb.Append(", "); }
          __first = false;
          sb.Append("ServerException: ");
          sb.Append(ServerException== null ? "<null>" : ServerException.ToString());
        }
        sb.Append(")");
        return sb.ToString();
      }
    }


    public partial class shutdownMemberArgs : TBase
    {
      private string _clusterId;
      private string _memberId;

      public string ClusterId
      {
        get
        {
          return _clusterId;
        }
        set
        {
          __isset.clusterId = true;
          this._clusterId = value;
        }
      }

      public string MemberId
      {
        get
        {
          return _memberId;
        }
        set
        {
          __isset.memberId = true;
          this._memberId = value;
        }
      }


      public Isset __isset;
      public struct Isset
      {
        public bool clusterId;
        public bool memberId;
      }

      public shutdownMemberArgs()
      {
      }

      public async Task ReadAsync(TProtocol iprot, CancellationToken cancellationToken)
      {
        iprot.IncrementRecursionDepth();
        try
        {
          TField field;
          await iprot.ReadStructBeginAsync(cancellationToken);
          while (true)
          {
            field = await iprot.ReadFieldBeginAsync(cancellationToken);
            if (field.Type == TType.Stop)
            {
              break;
            }

            switch (field.ID)
            {
              case 1:
                if (field.Type == TType.String)
                {
                  ClusterId = await iprot.ReadStringAsync(cancellationToken);
                }
                else
                {
                  await TProtocolUtil.SkipAsync(iprot, field.Type, cancellationToken);
                }
                break;
              case 2:
                if (field.Type == TType.String)
                {
                  MemberId = await iprot.ReadStringAsync(cancellationToken);
                }
                else
                {
                  await TProtocolUtil.SkipAsync(iprot, field.Type, cancellationToken);
                }
                break;
              default: 
                await TProtocolUtil.SkipAsync(iprot, field.Type, cancellationToken);
                break;
            }

            await iprot.ReadFieldEndAsync(cancellationToken);
          }

          await iprot.ReadStructEndAsync(cancellationToken);
        }
        finally
        {
          iprot.DecrementRecursionDepth();
        }
      }

      public async Task WriteAsync(TProtocol oprot, CancellationToken cancellationToken)
      {
        oprot.IncrementRecursionDepth();
        try
        {
          var struc = new TStruct("shutdownMember_args");
          await oprot.WriteStructBeginAsync(struc, cancellationToken);
          var field = new TField();
          if (ClusterId != null && __isset.clusterId)
          {
            field.Name = "clusterId";
            field.Type = TType.String;
            field.ID = 1;
            await oprot.WriteFieldBeginAsync(field, cancellationToken);
            await oprot.WriteStringAsync(ClusterId, cancellationToken);
            await oprot.WriteFieldEndAsync(cancellationToken);
          }
          if (MemberId != null && __isset.memberId)
          {
            field.Name = "memberId";
            field.Type = TType.String;
            field.ID = 2;
            await oprot.WriteFieldBeginAsync(field, cancellationToken);
            await oprot.WriteStringAsync(MemberId, cancellationToken);
            await oprot.WriteFieldEndAsync(cancellationToken);
          }
          await oprot.WriteFieldStopAsync(cancellationToken);
          await oprot.WriteStructEndAsync(cancellationToken);
        }
        finally
        {
          oprot.DecrementRecursionDepth();
        }
      }

      public override bool Equals(object that)
      {
        var other = that as shutdownMemberArgs;
        if (other == null) return false;
        if (ReferenceEquals(this, other)) return true;
        return ((__isset.clusterId == other.__isset.clusterId) && ((!__isset.clusterId) || (System.Object.Equals(ClusterId, other.ClusterId))))
          && ((__isset.memberId == other.__isset.memberId) && ((!__isset.memberId) || (System.Object.Equals(MemberId, other.MemberId))));
      }

      public override int GetHashCode() {
        int hashcode = 157;
        unchecked {
          if(__isset.clusterId)
            hashcode = (hashcode * 397) + ClusterId.GetHashCode();
          if(__isset.memberId)
            hashcode = (hashcode * 397) + MemberId.GetHashCode();
        }
        return hashcode;
      }

      public override string ToString()
      {
        var sb = new StringBuilder("shutdownMember_args(");
        bool __first = true;
        if (ClusterId != null && __isset.clusterId)
        {
          if(!__first) { sb.Append(", "); }
          __first = false;
          sb.Append("ClusterId: ");
          sb.Append(ClusterId);
        }
        if (MemberId != null && __isset.memberId)
        {
          if(!__first) { sb.Append(", "); }
          __first = false;
          sb.Append("MemberId: ");
          sb.Append(MemberId);
        }
        sb.Append(")");
        return sb.ToString();
      }
    }


    public partial class shutdownMemberResult : TBase
    {
      private bool _success;

      public bool Success
      {
        get
        {
          return _success;
        }
        set
        {
          __isset.success = true;
          this._success = value;
        }
      }


      public Isset __isset;
      public struct Isset
      {
        public bool success;
      }

      public shutdownMemberResult()
      {
      }

      public async Task ReadAsync(TProtocol iprot, CancellationToken cancellationToken)
      {
        iprot.IncrementRecursionDepth();
        try
        {
          TField field;
          await iprot.ReadStructBeginAsync(cancellationToken);
          while (true)
          {
            field = await iprot.ReadFieldBeginAsync(cancellationToken);
            if (field.Type == TType.Stop)
            {
              break;
            }

            switch (field.ID)
            {
              case 0:
                if (field.Type == TType.Bool)
                {
                  Success = await iprot.ReadBoolAsync(cancellationToken);
                }
                else
                {
                  await TProtocolUtil.SkipAsync(iprot, field.Type, cancellationToken);
                }
                break;
              default: 
                await TProtocolUtil.SkipAsync(iprot, field.Type, cancellationToken);
                break;
            }

            await iprot.ReadFieldEndAsync(cancellationToken);
          }

          await iprot.ReadStructEndAsync(cancellationToken);
        }
        finally
        {
          iprot.DecrementRecursionDepth();
        }
      }

      public async Task WriteAsync(TProtocol oprot, CancellationToken cancellationToken)
      {
        oprot.IncrementRecursionDepth();
        try
        {
          var struc = new TStruct("shutdownMember_result");
          await oprot.WriteStructBeginAsync(struc, cancellationToken);
          var field = new TField();

          if(this.__isset.success)
          {
            field.Name = "Success";
            field.Type = TType.Bool;
            field.ID = 0;
            await oprot.WriteFieldBeginAsync(field, cancellationToken);
            await oprot.WriteBoolAsync(Success, cancellationToken);
            await oprot.WriteFieldEndAsync(cancellationToken);
          }
          await oprot.WriteFieldStopAsync(cancellationToken);
          await oprot.WriteStructEndAsync(cancellationToken);
        }
        finally
        {
          oprot.DecrementRecursionDepth();
        }
      }

      public override bool Equals(object that)
      {
        var other = that as shutdownMemberResult;
        if (other == null) return false;
        if (ReferenceEquals(this, other)) return true;
        return ((__isset.success == other.__isset.success) && ((!__isset.success) || (System.Object.Equals(Success, other.Success))));
      }

      public override int GetHashCode() {
        int hashcode = 157;
        unchecked {
          if(__isset.success)
            hashcode = (hashcode * 397) + Success.GetHashCode();
        }
        return hashcode;
      }

      public override string ToString()
      {
        var sb = new StringBuilder("shutdownMember_result(");
        bool __first = true;
        if (__isset.success)
        {
          if(!__first) { sb.Append(", "); }
          __first = false;
          sb.Append("Success: ");
          sb.Append(Success);
        }
        sb.Append(")");
        return sb.ToString();
      }
    }


    public partial class terminateMemberArgs : TBase
    {
      private string _clusterId;
      private string _memberId;

      public string ClusterId
      {
        get
        {
          return _clusterId;
        }
        set
        {
          __isset.clusterId = true;
          this._clusterId = value;
        }
      }

      public string MemberId
      {
        get
        {
          return _memberId;
        }
        set
        {
          __isset.memberId = true;
          this._memberId = value;
        }
      }


      public Isset __isset;
      public struct Isset
      {
        public bool clusterId;
        public bool memberId;
      }

      public terminateMemberArgs()
      {
      }

      public async Task ReadAsync(TProtocol iprot, CancellationToken cancellationToken)
      {
        iprot.IncrementRecursionDepth();
        try
        {
          TField field;
          await iprot.ReadStructBeginAsync(cancellationToken);
          while (true)
          {
            field = await iprot.ReadFieldBeginAsync(cancellationToken);
            if (field.Type == TType.Stop)
            {
              break;
            }

            switch (field.ID)
            {
              case 1:
                if (field.Type == TType.String)
                {
                  ClusterId = await iprot.ReadStringAsync(cancellationToken);
                }
                else
                {
                  await TProtocolUtil.SkipAsync(iprot, field.Type, cancellationToken);
                }
                break;
              case 2:
                if (field.Type == TType.String)
                {
                  MemberId = await iprot.ReadStringAsync(cancellationToken);
                }
                else
                {
                  await TProtocolUtil.SkipAsync(iprot, field.Type, cancellationToken);
                }
                break;
              default: 
                await TProtocolUtil.SkipAsync(iprot, field.Type, cancellationToken);
                break;
            }

            await iprot.ReadFieldEndAsync(cancellationToken);
          }

          await iprot.ReadStructEndAsync(cancellationToken);
        }
        finally
        {
          iprot.DecrementRecursionDepth();
        }
      }

      public async Task WriteAsync(TProtocol oprot, CancellationToken cancellationToken)
      {
        oprot.IncrementRecursionDepth();
        try
        {
          var struc = new TStruct("terminateMember_args");
          await oprot.WriteStructBeginAsync(struc, cancellationToken);
          var field = new TField();
          if (ClusterId != null && __isset.clusterId)
          {
            field.Name = "clusterId";
            field.Type = TType.String;
            field.ID = 1;
            await oprot.WriteFieldBeginAsync(field, cancellationToken);
            await oprot.WriteStringAsync(ClusterId, cancellationToken);
            await oprot.WriteFieldEndAsync(cancellationToken);
          }
          if (MemberId != null && __isset.memberId)
          {
            field.Name = "memberId";
            field.Type = TType.String;
            field.ID = 2;
            await oprot.WriteFieldBeginAsync(field, cancellationToken);
            await oprot.WriteStringAsync(MemberId, cancellationToken);
            await oprot.WriteFieldEndAsync(cancellationToken);
          }
          await oprot.WriteFieldStopAsync(cancellationToken);
          await oprot.WriteStructEndAsync(cancellationToken);
        }
        finally
        {
          oprot.DecrementRecursionDepth();
        }
      }

      public override bool Equals(object that)
      {
        var other = that as terminateMemberArgs;
        if (other == null) return false;
        if (ReferenceEquals(this, other)) return true;
        return ((__isset.clusterId == other.__isset.clusterId) && ((!__isset.clusterId) || (System.Object.Equals(ClusterId, other.ClusterId))))
          && ((__isset.memberId == other.__isset.memberId) && ((!__isset.memberId) || (System.Object.Equals(MemberId, other.MemberId))));
      }

      public override int GetHashCode() {
        int hashcode = 157;
        unchecked {
          if(__isset.clusterId)
            hashcode = (hashcode * 397) + ClusterId.GetHashCode();
          if(__isset.memberId)
            hashcode = (hashcode * 397) + MemberId.GetHashCode();
        }
        return hashcode;
      }

      public override string ToString()
      {
        var sb = new StringBuilder("terminateMember_args(");
        bool __first = true;
        if (ClusterId != null && __isset.clusterId)
        {
          if(!__first) { sb.Append(", "); }
          __first = false;
          sb.Append("ClusterId: ");
          sb.Append(ClusterId);
        }
        if (MemberId != null && __isset.memberId)
        {
          if(!__first) { sb.Append(", "); }
          __first = false;
          sb.Append("MemberId: ");
          sb.Append(MemberId);
        }
        sb.Append(")");
        return sb.ToString();
      }
    }


    public partial class terminateMemberResult : TBase
    {
      private bool _success;

      public bool Success
      {
        get
        {
          return _success;
        }
        set
        {
          __isset.success = true;
          this._success = value;
        }
      }


      public Isset __isset;
      public struct Isset
      {
        public bool success;
      }

      public terminateMemberResult()
      {
      }

      public async Task ReadAsync(TProtocol iprot, CancellationToken cancellationToken)
      {
        iprot.IncrementRecursionDepth();
        try
        {
          TField field;
          await iprot.ReadStructBeginAsync(cancellationToken);
          while (true)
          {
            field = await iprot.ReadFieldBeginAsync(cancellationToken);
            if (field.Type == TType.Stop)
            {
              break;
            }

            switch (field.ID)
            {
              case 0:
                if (field.Type == TType.Bool)
                {
                  Success = await iprot.ReadBoolAsync(cancellationToken);
                }
                else
                {
                  await TProtocolUtil.SkipAsync(iprot, field.Type, cancellationToken);
                }
                break;
              default: 
                await TProtocolUtil.SkipAsync(iprot, field.Type, cancellationToken);
                break;
            }

            await iprot.ReadFieldEndAsync(cancellationToken);
          }

          await iprot.ReadStructEndAsync(cancellationToken);
        }
        finally
        {
          iprot.DecrementRecursionDepth();
        }
      }

      public async Task WriteAsync(TProtocol oprot, CancellationToken cancellationToken)
      {
        oprot.IncrementRecursionDepth();
        try
        {
          var struc = new TStruct("terminateMember_result");
          await oprot.WriteStructBeginAsync(struc, cancellationToken);
          var field = new TField();

          if(this.__isset.success)
          {
            field.Name = "Success";
            field.Type = TType.Bool;
            field.ID = 0;
            await oprot.WriteFieldBeginAsync(field, cancellationToken);
            await oprot.WriteBoolAsync(Success, cancellationToken);
            await oprot.WriteFieldEndAsync(cancellationToken);
          }
          await oprot.WriteFieldStopAsync(cancellationToken);
          await oprot.WriteStructEndAsync(cancellationToken);
        }
        finally
        {
          oprot.DecrementRecursionDepth();
        }
      }

      public override bool Equals(object that)
      {
        var other = that as terminateMemberResult;
        if (other == null) return false;
        if (ReferenceEquals(this, other)) return true;
        return ((__isset.success == other.__isset.success) && ((!__isset.success) || (System.Object.Equals(Success, other.Success))));
      }

      public override int GetHashCode() {
        int hashcode = 157;
        unchecked {
          if(__isset.success)
            hashcode = (hashcode * 397) + Success.GetHashCode();
        }
        return hashcode;
      }

      public override string ToString()
      {
        var sb = new StringBuilder("terminateMember_result(");
        bool __first = true;
        if (__isset.success)
        {
          if(!__first) { sb.Append(", "); }
          __first = false;
          sb.Append("Success: ");
          sb.Append(Success);
        }
        sb.Append(")");
        return sb.ToString();
      }
    }


    public partial class suspendMemberArgs : TBase
    {
      private string _clusterId;
      private string _memberId;

      public string ClusterId
      {
        get
        {
          return _clusterId;
        }
        set
        {
          __isset.clusterId = true;
          this._clusterId = value;
        }
      }

      public string MemberId
      {
        get
        {
          return _memberId;
        }
        set
        {
          __isset.memberId = true;
          this._memberId = value;
        }
      }


      public Isset __isset;
      public struct Isset
      {
        public bool clusterId;
        public bool memberId;
      }

      public suspendMemberArgs()
      {
      }

      public async Task ReadAsync(TProtocol iprot, CancellationToken cancellationToken)
      {
        iprot.IncrementRecursionDepth();
        try
        {
          TField field;
          await iprot.ReadStructBeginAsync(cancellationToken);
          while (true)
          {
            field = await iprot.ReadFieldBeginAsync(cancellationToken);
            if (field.Type == TType.Stop)
            {
              break;
            }

            switch (field.ID)
            {
              case 1:
                if (field.Type == TType.String)
                {
                  ClusterId = await iprot.ReadStringAsync(cancellationToken);
                }
                else
                {
                  await TProtocolUtil.SkipAsync(iprot, field.Type, cancellationToken);
                }
                break;
              case 2:
                if (field.Type == TType.String)
                {
                  MemberId = await iprot.ReadStringAsync(cancellationToken);
                }
                else
                {
                  await TProtocolUtil.SkipAsync(iprot, field.Type, cancellationToken);
                }
                break;
              default: 
                await TProtocolUtil.SkipAsync(iprot, field.Type, cancellationToken);
                break;
            }

            await iprot.ReadFieldEndAsync(cancellationToken);
          }

          await iprot.ReadStructEndAsync(cancellationToken);
        }
        finally
        {
          iprot.DecrementRecursionDepth();
        }
      }

      public async Task WriteAsync(TProtocol oprot, CancellationToken cancellationToken)
      {
        oprot.IncrementRecursionDepth();
        try
        {
          var struc = new TStruct("suspendMember_args");
          await oprot.WriteStructBeginAsync(struc, cancellationToken);
          var field = new TField();
          if (ClusterId != null && __isset.clusterId)
          {
            field.Name = "clusterId";
            field.Type = TType.String;
            field.ID = 1;
            await oprot.WriteFieldBeginAsync(field, cancellationToken);
            await oprot.WriteStringAsync(ClusterId, cancellationToken);
            await oprot.WriteFieldEndAsync(cancellationToken);
          }
          if (MemberId != null && __isset.memberId)
          {
            field.Name = "memberId";
            field.Type = TType.String;
            field.ID = 2;
            await oprot.WriteFieldBeginAsync(field, cancellationToken);
            await oprot.WriteStringAsync(MemberId, cancellationToken);
            await oprot.WriteFieldEndAsync(cancellationToken);
          }
          await oprot.WriteFieldStopAsync(cancellationToken);
          await oprot.WriteStructEndAsync(cancellationToken);
        }
        finally
        {
          oprot.DecrementRecursionDepth();
        }
      }

      public override bool Equals(object that)
      {
        var other = that as suspendMemberArgs;
        if (other == null) return false;
        if (ReferenceEquals(this, other)) return true;
        return ((__isset.clusterId == other.__isset.clusterId) && ((!__isset.clusterId) || (System.Object.Equals(ClusterId, other.ClusterId))))
          && ((__isset.memberId == other.__isset.memberId) && ((!__isset.memberId) || (System.Object.Equals(MemberId, other.MemberId))));
      }

      public override int GetHashCode() {
        int hashcode = 157;
        unchecked {
          if(__isset.clusterId)
            hashcode = (hashcode * 397) + ClusterId.GetHashCode();
          if(__isset.memberId)
            hashcode = (hashcode * 397) + MemberId.GetHashCode();
        }
        return hashcode;
      }

      public override string ToString()
      {
        var sb = new StringBuilder("suspendMember_args(");
        bool __first = true;
        if (ClusterId != null && __isset.clusterId)
        {
          if(!__first) { sb.Append(", "); }
          __first = false;
          sb.Append("ClusterId: ");
          sb.Append(ClusterId);
        }
        if (MemberId != null && __isset.memberId)
        {
          if(!__first) { sb.Append(", "); }
          __first = false;
          sb.Append("MemberId: ");
          sb.Append(MemberId);
        }
        sb.Append(")");
        return sb.ToString();
      }
    }


    public partial class suspendMemberResult : TBase
    {
      private bool _success;

      public bool Success
      {
        get
        {
          return _success;
        }
        set
        {
          __isset.success = true;
          this._success = value;
        }
      }


      public Isset __isset;
      public struct Isset
      {
        public bool success;
      }

      public suspendMemberResult()
      {
      }

      public async Task ReadAsync(TProtocol iprot, CancellationToken cancellationToken)
      {
        iprot.IncrementRecursionDepth();
        try
        {
          TField field;
          await iprot.ReadStructBeginAsync(cancellationToken);
          while (true)
          {
            field = await iprot.ReadFieldBeginAsync(cancellationToken);
            if (field.Type == TType.Stop)
            {
              break;
            }

            switch (field.ID)
            {
              case 0:
                if (field.Type == TType.Bool)
                {
                  Success = await iprot.ReadBoolAsync(cancellationToken);
                }
                else
                {
                  await TProtocolUtil.SkipAsync(iprot, field.Type, cancellationToken);
                }
                break;
              default: 
                await TProtocolUtil.SkipAsync(iprot, field.Type, cancellationToken);
                break;
            }

            await iprot.ReadFieldEndAsync(cancellationToken);
          }

          await iprot.ReadStructEndAsync(cancellationToken);
        }
        finally
        {
          iprot.DecrementRecursionDepth();
        }
      }

      public async Task WriteAsync(TProtocol oprot, CancellationToken cancellationToken)
      {
        oprot.IncrementRecursionDepth();
        try
        {
          var struc = new TStruct("suspendMember_result");
          await oprot.WriteStructBeginAsync(struc, cancellationToken);
          var field = new TField();

          if(this.__isset.success)
          {
            field.Name = "Success";
            field.Type = TType.Bool;
            field.ID = 0;
            await oprot.WriteFieldBeginAsync(field, cancellationToken);
            await oprot.WriteBoolAsync(Success, cancellationToken);
            await oprot.WriteFieldEndAsync(cancellationToken);
          }
          await oprot.WriteFieldStopAsync(cancellationToken);
          await oprot.WriteStructEndAsync(cancellationToken);
        }
        finally
        {
          oprot.DecrementRecursionDepth();
        }
      }

      public override bool Equals(object that)
      {
        var other = that as suspendMemberResult;
        if (other == null) return false;
        if (ReferenceEquals(this, other)) return true;
        return ((__isset.success == other.__isset.success) && ((!__isset.success) || (System.Object.Equals(Success, other.Success))));
      }

      public override int GetHashCode() {
        int hashcode = 157;
        unchecked {
          if(__isset.success)
            hashcode = (hashcode * 397) + Success.GetHashCode();
        }
        return hashcode;
      }

      public override string ToString()
      {
        var sb = new StringBuilder("suspendMember_result(");
        bool __first = true;
        if (__isset.success)
        {
          if(!__first) { sb.Append(", "); }
          __first = false;
          sb.Append("Success: ");
          sb.Append(Success);
        }
        sb.Append(")");
        return sb.ToString();
      }
    }


    public partial class resumeMemberArgs : TBase
    {
      private string _clusterId;
      private string _memberId;

      public string ClusterId
      {
        get
        {
          return _clusterId;
        }
        set
        {
          __isset.clusterId = true;
          this._clusterId = value;
        }
      }

      public string MemberId
      {
        get
        {
          return _memberId;
        }
        set
        {
          __isset.memberId = true;
          this._memberId = value;
        }
      }


      public Isset __isset;
      public struct Isset
      {
        public bool clusterId;
        public bool memberId;
      }

      public resumeMemberArgs()
      {
      }

      public async Task ReadAsync(TProtocol iprot, CancellationToken cancellationToken)
      {
        iprot.IncrementRecursionDepth();
        try
        {
          TField field;
          await iprot.ReadStructBeginAsync(cancellationToken);
          while (true)
          {
            field = await iprot.ReadFieldBeginAsync(cancellationToken);
            if (field.Type == TType.Stop)
            {
              break;
            }

            switch (field.ID)
            {
              case 1:
                if (field.Type == TType.String)
                {
                  ClusterId = await iprot.ReadStringAsync(cancellationToken);
                }
                else
                {
                  await TProtocolUtil.SkipAsync(iprot, field.Type, cancellationToken);
                }
                break;
              case 2:
                if (field.Type == TType.String)
                {
                  MemberId = await iprot.ReadStringAsync(cancellationToken);
                }
                else
                {
                  await TProtocolUtil.SkipAsync(iprot, field.Type, cancellationToken);
                }
                break;
              default: 
                await TProtocolUtil.SkipAsync(iprot, field.Type, cancellationToken);
                break;
            }

            await iprot.ReadFieldEndAsync(cancellationToken);
          }

          await iprot.ReadStructEndAsync(cancellationToken);
        }
        finally
        {
          iprot.DecrementRecursionDepth();
        }
      }

      public async Task WriteAsync(TProtocol oprot, CancellationToken cancellationToken)
      {
        oprot.IncrementRecursionDepth();
        try
        {
          var struc = new TStruct("resumeMember_args");
          await oprot.WriteStructBeginAsync(struc, cancellationToken);
          var field = new TField();
          if (ClusterId != null && __isset.clusterId)
          {
            field.Name = "clusterId";
            field.Type = TType.String;
            field.ID = 1;
            await oprot.WriteFieldBeginAsync(field, cancellationToken);
            await oprot.WriteStringAsync(ClusterId, cancellationToken);
            await oprot.WriteFieldEndAsync(cancellationToken);
          }
          if (MemberId != null && __isset.memberId)
          {
            field.Name = "memberId";
            field.Type = TType.String;
            field.ID = 2;
            await oprot.WriteFieldBeginAsync(field, cancellationToken);
            await oprot.WriteStringAsync(MemberId, cancellationToken);
            await oprot.WriteFieldEndAsync(cancellationToken);
          }
          await oprot.WriteFieldStopAsync(cancellationToken);
          await oprot.WriteStructEndAsync(cancellationToken);
        }
        finally
        {
          oprot.DecrementRecursionDepth();
        }
      }

      public override bool Equals(object that)
      {
        var other = that as resumeMemberArgs;
        if (other == null) return false;
        if (ReferenceEquals(this, other)) return true;
        return ((__isset.clusterId == other.__isset.clusterId) && ((!__isset.clusterId) || (System.Object.Equals(ClusterId, other.ClusterId))))
          && ((__isset.memberId == other.__isset.memberId) && ((!__isset.memberId) || (System.Object.Equals(MemberId, other.MemberId))));
      }

      public override int GetHashCode() {
        int hashcode = 157;
        unchecked {
          if(__isset.clusterId)
            hashcode = (hashcode * 397) + ClusterId.GetHashCode();
          if(__isset.memberId)
            hashcode = (hashcode * 397) + MemberId.GetHashCode();
        }
        return hashcode;
      }

      public override string ToString()
      {
        var sb = new StringBuilder("resumeMember_args(");
        bool __first = true;
        if (ClusterId != null && __isset.clusterId)
        {
          if(!__first) { sb.Append(", "); }
          __first = false;
          sb.Append("ClusterId: ");
          sb.Append(ClusterId);
        }
        if (MemberId != null && __isset.memberId)
        {
          if(!__first) { sb.Append(", "); }
          __first = false;
          sb.Append("MemberId: ");
          sb.Append(MemberId);
        }
        sb.Append(")");
        return sb.ToString();
      }
    }


    public partial class resumeMemberResult : TBase
    {
      private bool _success;

      public bool Success
      {
        get
        {
          return _success;
        }
        set
        {
          __isset.success = true;
          this._success = value;
        }
      }


      public Isset __isset;
      public struct Isset
      {
        public bool success;
      }

      public resumeMemberResult()
      {
      }

      public async Task ReadAsync(TProtocol iprot, CancellationToken cancellationToken)
      {
        iprot.IncrementRecursionDepth();
        try
        {
          TField field;
          await iprot.ReadStructBeginAsync(cancellationToken);
          while (true)
          {
            field = await iprot.ReadFieldBeginAsync(cancellationToken);
            if (field.Type == TType.Stop)
            {
              break;
            }

            switch (field.ID)
            {
              case 0:
                if (field.Type == TType.Bool)
                {
                  Success = await iprot.ReadBoolAsync(cancellationToken);
                }
                else
                {
                  await TProtocolUtil.SkipAsync(iprot, field.Type, cancellationToken);
                }
                break;
              default: 
                await TProtocolUtil.SkipAsync(iprot, field.Type, cancellationToken);
                break;
            }

            await iprot.ReadFieldEndAsync(cancellationToken);
          }

          await iprot.ReadStructEndAsync(cancellationToken);
        }
        finally
        {
          iprot.DecrementRecursionDepth();
        }
      }

      public async Task WriteAsync(TProtocol oprot, CancellationToken cancellationToken)
      {
        oprot.IncrementRecursionDepth();
        try
        {
          var struc = new TStruct("resumeMember_result");
          await oprot.WriteStructBeginAsync(struc, cancellationToken);
          var field = new TField();

          if(this.__isset.success)
          {
            field.Name = "Success";
            field.Type = TType.Bool;
            field.ID = 0;
            await oprot.WriteFieldBeginAsync(field, cancellationToken);
            await oprot.WriteBoolAsync(Success, cancellationToken);
            await oprot.WriteFieldEndAsync(cancellationToken);
          }
          await oprot.WriteFieldStopAsync(cancellationToken);
          await oprot.WriteStructEndAsync(cancellationToken);
        }
        finally
        {
          oprot.DecrementRecursionDepth();
        }
      }

      public override bool Equals(object that)
      {
        var other = that as resumeMemberResult;
        if (other == null) return false;
        if (ReferenceEquals(this, other)) return true;
        return ((__isset.success == other.__isset.success) && ((!__isset.success) || (System.Object.Equals(Success, other.Success))));
      }

      public override int GetHashCode() {
        int hashcode = 157;
        unchecked {
          if(__isset.success)
            hashcode = (hashcode * 397) + Success.GetHashCode();
        }
        return hashcode;
      }

      public override string ToString()
      {
        var sb = new StringBuilder("resumeMember_result(");
        bool __first = true;
        if (__isset.success)
        {
          if(!__first) { sb.Append(", "); }
          __first = false;
          sb.Append("Success: ");
          sb.Append(Success);
        }
        sb.Append(")");
        return sb.ToString();
      }
    }


    public partial class shutdownClusterArgs : TBase
    {
      private string _clusterId;

      public string ClusterId
      {
        get
        {
          return _clusterId;
        }
        set
        {
          __isset.clusterId = true;
          this._clusterId = value;
        }
      }


      public Isset __isset;
      public struct Isset
      {
        public bool clusterId;
      }

      public shutdownClusterArgs()
      {
      }

      public async Task ReadAsync(TProtocol iprot, CancellationToken cancellationToken)
      {
        iprot.IncrementRecursionDepth();
        try
        {
          TField field;
          await iprot.ReadStructBeginAsync(cancellationToken);
          while (true)
          {
            field = await iprot.ReadFieldBeginAsync(cancellationToken);
            if (field.Type == TType.Stop)
            {
              break;
            }

            switch (field.ID)
            {
              case 1:
                if (field.Type == TType.String)
                {
                  ClusterId = await iprot.ReadStringAsync(cancellationToken);
                }
                else
                {
                  await TProtocolUtil.SkipAsync(iprot, field.Type, cancellationToken);
                }
                break;
              default: 
                await TProtocolUtil.SkipAsync(iprot, field.Type, cancellationToken);
                break;
            }

            await iprot.ReadFieldEndAsync(cancellationToken);
          }

          await iprot.ReadStructEndAsync(cancellationToken);
        }
        finally
        {
          iprot.DecrementRecursionDepth();
        }
      }

      public async Task WriteAsync(TProtocol oprot, CancellationToken cancellationToken)
      {
        oprot.IncrementRecursionDepth();
        try
        {
          var struc = new TStruct("shutdownCluster_args");
          await oprot.WriteStructBeginAsync(struc, cancellationToken);
          var field = new TField();
          if (ClusterId != null && __isset.clusterId)
          {
            field.Name = "clusterId";
            field.Type = TType.String;
            field.ID = 1;
            await oprot.WriteFieldBeginAsync(field, cancellationToken);
            await oprot.WriteStringAsync(ClusterId, cancellationToken);
            await oprot.WriteFieldEndAsync(cancellationToken);
          }
          await oprot.WriteFieldStopAsync(cancellationToken);
          await oprot.WriteStructEndAsync(cancellationToken);
        }
        finally
        {
          oprot.DecrementRecursionDepth();
        }
      }

      public override bool Equals(object that)
      {
        var other = that as shutdownClusterArgs;
        if (other == null) return false;
        if (ReferenceEquals(this, other)) return true;
        return ((__isset.clusterId == other.__isset.clusterId) && ((!__isset.clusterId) || (System.Object.Equals(ClusterId, other.ClusterId))));
      }

      public override int GetHashCode() {
        int hashcode = 157;
        unchecked {
          if(__isset.clusterId)
            hashcode = (hashcode * 397) + ClusterId.GetHashCode();
        }
        return hashcode;
      }

      public override string ToString()
      {
        var sb = new StringBuilder("shutdownCluster_args(");
        bool __first = true;
        if (ClusterId != null && __isset.clusterId)
        {
          if(!__first) { sb.Append(", "); }
          __first = false;
          sb.Append("ClusterId: ");
          sb.Append(ClusterId);
        }
        sb.Append(")");
        return sb.ToString();
      }
    }


    public partial class shutdownClusterResult : TBase
    {
      private bool _success;

      public bool Success
      {
        get
        {
          return _success;
        }
        set
        {
          __isset.success = true;
          this._success = value;
        }
      }


      public Isset __isset;
      public struct Isset
      {
        public bool success;
      }

      public shutdownClusterResult()
      {
      }

      public async Task ReadAsync(TProtocol iprot, CancellationToken cancellationToken)
      {
        iprot.IncrementRecursionDepth();
        try
        {
          TField field;
          await iprot.ReadStructBeginAsync(cancellationToken);
          while (true)
          {
            field = await iprot.ReadFieldBeginAsync(cancellationToken);
            if (field.Type == TType.Stop)
            {
              break;
            }

            switch (field.ID)
            {
              case 0:
                if (field.Type == TType.Bool)
                {
                  Success = await iprot.ReadBoolAsync(cancellationToken);
                }
                else
                {
                  await TProtocolUtil.SkipAsync(iprot, field.Type, cancellationToken);
                }
                break;
              default: 
                await TProtocolUtil.SkipAsync(iprot, field.Type, cancellationToken);
                break;
            }

            await iprot.ReadFieldEndAsync(cancellationToken);
          }

          await iprot.ReadStructEndAsync(cancellationToken);
        }
        finally
        {
          iprot.DecrementRecursionDepth();
        }
      }

      public async Task WriteAsync(TProtocol oprot, CancellationToken cancellationToken)
      {
        oprot.IncrementRecursionDepth();
        try
        {
          var struc = new TStruct("shutdownCluster_result");
          await oprot.WriteStructBeginAsync(struc, cancellationToken);
          var field = new TField();

          if(this.__isset.success)
          {
            field.Name = "Success";
            field.Type = TType.Bool;
            field.ID = 0;
            await oprot.WriteFieldBeginAsync(field, cancellationToken);
            await oprot.WriteBoolAsync(Success, cancellationToken);
            await oprot.WriteFieldEndAsync(cancellationToken);
          }
          await oprot.WriteFieldStopAsync(cancellationToken);
          await oprot.WriteStructEndAsync(cancellationToken);
        }
        finally
        {
          oprot.DecrementRecursionDepth();
        }
      }

      public override bool Equals(object that)
      {
        var other = that as shutdownClusterResult;
        if (other == null) return false;
        if (ReferenceEquals(this, other)) return true;
        return ((__isset.success == other.__isset.success) && ((!__isset.success) || (System.Object.Equals(Success, other.Success))));
      }

      public override int GetHashCode() {
        int hashcode = 157;
        unchecked {
          if(__isset.success)
            hashcode = (hashcode * 397) + Success.GetHashCode();
        }
        return hashcode;
      }

      public override string ToString()
      {
        var sb = new StringBuilder("shutdownCluster_result(");
        bool __first = true;
        if (__isset.success)
        {
          if(!__first) { sb.Append(", "); }
          __first = false;
          sb.Append("Success: ");
          sb.Append(Success);
        }
        sb.Append(")");
        return sb.ToString();
      }
    }


    public partial class terminateClusterArgs : TBase
    {
      private string _clusterId;

      public string ClusterId
      {
        get
        {
          return _clusterId;
        }
        set
        {
          __isset.clusterId = true;
          this._clusterId = value;
        }
      }


      public Isset __isset;
      public struct Isset
      {
        public bool clusterId;
      }

      public terminateClusterArgs()
      {
      }

      public async Task ReadAsync(TProtocol iprot, CancellationToken cancellationToken)
      {
        iprot.IncrementRecursionDepth();
        try
        {
          TField field;
          await iprot.ReadStructBeginAsync(cancellationToken);
          while (true)
          {
            field = await iprot.ReadFieldBeginAsync(cancellationToken);
            if (field.Type == TType.Stop)
            {
              break;
            }

            switch (field.ID)
            {
              case 1:
                if (field.Type == TType.String)
                {
                  ClusterId = await iprot.ReadStringAsync(cancellationToken);
                }
                else
                {
                  await TProtocolUtil.SkipAsync(iprot, field.Type, cancellationToken);
                }
                break;
              default: 
                await TProtocolUtil.SkipAsync(iprot, field.Type, cancellationToken);
                break;
            }

            await iprot.ReadFieldEndAsync(cancellationToken);
          }

          await iprot.ReadStructEndAsync(cancellationToken);
        }
        finally
        {
          iprot.DecrementRecursionDepth();
        }
      }

      public async Task WriteAsync(TProtocol oprot, CancellationToken cancellationToken)
      {
        oprot.IncrementRecursionDepth();
        try
        {
          var struc = new TStruct("terminateCluster_args");
          await oprot.WriteStructBeginAsync(struc, cancellationToken);
          var field = new TField();
          if (ClusterId != null && __isset.clusterId)
          {
            field.Name = "clusterId";
            field.Type = TType.String;
            field.ID = 1;
            await oprot.WriteFieldBeginAsync(field, cancellationToken);
            await oprot.WriteStringAsync(ClusterId, cancellationToken);
            await oprot.WriteFieldEndAsync(cancellationToken);
          }
          await oprot.WriteFieldStopAsync(cancellationToken);
          await oprot.WriteStructEndAsync(cancellationToken);
        }
        finally
        {
          oprot.DecrementRecursionDepth();
        }
      }

      public override bool Equals(object that)
      {
        var other = that as terminateClusterArgs;
        if (other == null) return false;
        if (ReferenceEquals(this, other)) return true;
        return ((__isset.clusterId == other.__isset.clusterId) && ((!__isset.clusterId) || (System.Object.Equals(ClusterId, other.ClusterId))));
      }

      public override int GetHashCode() {
        int hashcode = 157;
        unchecked {
          if(__isset.clusterId)
            hashcode = (hashcode * 397) + ClusterId.GetHashCode();
        }
        return hashcode;
      }

      public override string ToString()
      {
        var sb = new StringBuilder("terminateCluster_args(");
        bool __first = true;
        if (ClusterId != null && __isset.clusterId)
        {
          if(!__first) { sb.Append(", "); }
          __first = false;
          sb.Append("ClusterId: ");
          sb.Append(ClusterId);
        }
        sb.Append(")");
        return sb.ToString();
      }
    }


    public partial class terminateClusterResult : TBase
    {
      private bool _success;

      public bool Success
      {
        get
        {
          return _success;
        }
        set
        {
          __isset.success = true;
          this._success = value;
        }
      }


      public Isset __isset;
      public struct Isset
      {
        public bool success;
      }

      public terminateClusterResult()
      {
      }

      public async Task ReadAsync(TProtocol iprot, CancellationToken cancellationToken)
      {
        iprot.IncrementRecursionDepth();
        try
        {
          TField field;
          await iprot.ReadStructBeginAsync(cancellationToken);
          while (true)
          {
            field = await iprot.ReadFieldBeginAsync(cancellationToken);
            if (field.Type == TType.Stop)
            {
              break;
            }

            switch (field.ID)
            {
              case 0:
                if (field.Type == TType.Bool)
                {
                  Success = await iprot.ReadBoolAsync(cancellationToken);
                }
                else
                {
                  await TProtocolUtil.SkipAsync(iprot, field.Type, cancellationToken);
                }
                break;
              default: 
                await TProtocolUtil.SkipAsync(iprot, field.Type, cancellationToken);
                break;
            }

            await iprot.ReadFieldEndAsync(cancellationToken);
          }

          await iprot.ReadStructEndAsync(cancellationToken);
        }
        finally
        {
          iprot.DecrementRecursionDepth();
        }
      }

      public async Task WriteAsync(TProtocol oprot, CancellationToken cancellationToken)
      {
        oprot.IncrementRecursionDepth();
        try
        {
          var struc = new TStruct("terminateCluster_result");
          await oprot.WriteStructBeginAsync(struc, cancellationToken);
          var field = new TField();

          if(this.__isset.success)
          {
            field.Name = "Success";
            field.Type = TType.Bool;
            field.ID = 0;
            await oprot.WriteFieldBeginAsync(field, cancellationToken);
            await oprot.WriteBoolAsync(Success, cancellationToken);
            await oprot.WriteFieldEndAsync(cancellationToken);
          }
          await oprot.WriteFieldStopAsync(cancellationToken);
          await oprot.WriteStructEndAsync(cancellationToken);
        }
        finally
        {
          oprot.DecrementRecursionDepth();
        }
      }

      public override bool Equals(object that)
      {
        var other = that as terminateClusterResult;
        if (other == null) return false;
        if (ReferenceEquals(this, other)) return true;
        return ((__isset.success == other.__isset.success) && ((!__isset.success) || (System.Object.Equals(Success, other.Success))));
      }

      public override int GetHashCode() {
        int hashcode = 157;
        unchecked {
          if(__isset.success)
            hashcode = (hashcode * 397) + Success.GetHashCode();
        }
        return hashcode;
      }

      public override string ToString()
      {
        var sb = new StringBuilder("terminateCluster_result(");
        bool __first = true;
        if (__isset.success)
        {
          if(!__first) { sb.Append(", "); }
          __first = false;
          sb.Append("Success: ");
          sb.Append(Success);
        }
        sb.Append(")");
        return sb.ToString();
      }
    }


    public partial class splitMemberFromClusterArgs : TBase
    {
      private string _memberId;

      public string MemberId
      {
        get
        {
          return _memberId;
        }
        set
        {
          __isset.memberId = true;
          this._memberId = value;
        }
      }


      public Isset __isset;
      public struct Isset
      {
        public bool memberId;
      }

      public splitMemberFromClusterArgs()
      {
      }

      public async Task ReadAsync(TProtocol iprot, CancellationToken cancellationToken)
      {
        iprot.IncrementRecursionDepth();
        try
        {
          TField field;
          await iprot.ReadStructBeginAsync(cancellationToken);
          while (true)
          {
            field = await iprot.ReadFieldBeginAsync(cancellationToken);
            if (field.Type == TType.Stop)
            {
              break;
            }

            switch (field.ID)
            {
              case 1:
                if (field.Type == TType.String)
                {
                  MemberId = await iprot.ReadStringAsync(cancellationToken);
                }
                else
                {
                  await TProtocolUtil.SkipAsync(iprot, field.Type, cancellationToken);
                }
                break;
              default: 
                await TProtocolUtil.SkipAsync(iprot, field.Type, cancellationToken);
                break;
            }

            await iprot.ReadFieldEndAsync(cancellationToken);
          }

          await iprot.ReadStructEndAsync(cancellationToken);
        }
        finally
        {
          iprot.DecrementRecursionDepth();
        }
      }

      public async Task WriteAsync(TProtocol oprot, CancellationToken cancellationToken)
      {
        oprot.IncrementRecursionDepth();
        try
        {
          var struc = new TStruct("splitMemberFromCluster_args");
          await oprot.WriteStructBeginAsync(struc, cancellationToken);
          var field = new TField();
          if (MemberId != null && __isset.memberId)
          {
            field.Name = "memberId";
            field.Type = TType.String;
            field.ID = 1;
            await oprot.WriteFieldBeginAsync(field, cancellationToken);
            await oprot.WriteStringAsync(MemberId, cancellationToken);
            await oprot.WriteFieldEndAsync(cancellationToken);
          }
          await oprot.WriteFieldStopAsync(cancellationToken);
          await oprot.WriteStructEndAsync(cancellationToken);
        }
        finally
        {
          oprot.DecrementRecursionDepth();
        }
      }

      public override bool Equals(object that)
      {
        var other = that as splitMemberFromClusterArgs;
        if (other == null) return false;
        if (ReferenceEquals(this, other)) return true;
        return ((__isset.memberId == other.__isset.memberId) && ((!__isset.memberId) || (System.Object.Equals(MemberId, other.MemberId))));
      }

      public override int GetHashCode() {
        int hashcode = 157;
        unchecked {
          if(__isset.memberId)
            hashcode = (hashcode * 397) + MemberId.GetHashCode();
        }
        return hashcode;
      }

      public override string ToString()
      {
        var sb = new StringBuilder("splitMemberFromCluster_args(");
        bool __first = true;
        if (MemberId != null && __isset.memberId)
        {
          if(!__first) { sb.Append(", "); }
          __first = false;
          sb.Append("MemberId: ");
          sb.Append(MemberId);
        }
        sb.Append(")");
        return sb.ToString();
      }
    }


    public partial class splitMemberFromClusterResult : TBase
    {
      private Cluster _success;

      public Cluster Success
      {
        get
        {
          return _success;
        }
        set
        {
          __isset.success = true;
          this._success = value;
        }
      }


      public Isset __isset;
      public struct Isset
      {
        public bool success;
      }

      public splitMemberFromClusterResult()
      {
      }

      public async Task ReadAsync(TProtocol iprot, CancellationToken cancellationToken)
      {
        iprot.IncrementRecursionDepth();
        try
        {
          TField field;
          await iprot.ReadStructBeginAsync(cancellationToken);
          while (true)
          {
            field = await iprot.ReadFieldBeginAsync(cancellationToken);
            if (field.Type == TType.Stop)
            {
              break;
            }

            switch (field.ID)
            {
              case 0:
                if (field.Type == TType.Struct)
                {
                  Success = new Cluster();
                  await Success.ReadAsync(iprot, cancellationToken);
                }
                else
                {
                  await TProtocolUtil.SkipAsync(iprot, field.Type, cancellationToken);
                }
                break;
              default: 
                await TProtocolUtil.SkipAsync(iprot, field.Type, cancellationToken);
                break;
            }

            await iprot.ReadFieldEndAsync(cancellationToken);
          }

          await iprot.ReadStructEndAsync(cancellationToken);
        }
        finally
        {
          iprot.DecrementRecursionDepth();
        }
      }

      public async Task WriteAsync(TProtocol oprot, CancellationToken cancellationToken)
      {
        oprot.IncrementRecursionDepth();
        try
        {
          var struc = new TStruct("splitMemberFromCluster_result");
          await oprot.WriteStructBeginAsync(struc, cancellationToken);
          var field = new TField();

          if(this.__isset.success)
          {
            if (Success != null)
            {
              field.Name = "Success";
              field.Type = TType.Struct;
              field.ID = 0;
              await oprot.WriteFieldBeginAsync(field, cancellationToken);
              await Success.WriteAsync(oprot, cancellationToken);
              await oprot.WriteFieldEndAsync(cancellationToken);
            }
          }
          await oprot.WriteFieldStopAsync(cancellationToken);
          await oprot.WriteStructEndAsync(cancellationToken);
        }
        finally
        {
          oprot.DecrementRecursionDepth();
        }
      }

      public override bool Equals(object that)
      {
        var other = that as splitMemberFromClusterResult;
        if (other == null) return false;
        if (ReferenceEquals(this, other)) return true;
        return ((__isset.success == other.__isset.success) && ((!__isset.success) || (System.Object.Equals(Success, other.Success))));
      }

      public override int GetHashCode() {
        int hashcode = 157;
        unchecked {
          if(__isset.success)
            hashcode = (hashcode * 397) + Success.GetHashCode();
        }
        return hashcode;
      }

      public override string ToString()
      {
        var sb = new StringBuilder("splitMemberFromCluster_result(");
        bool __first = true;
        if (Success != null && __isset.success)
        {
          if(!__first) { sb.Append(", "); }
          __first = false;
          sb.Append("Success: ");
          sb.Append(Success== null ? "<null>" : Success.ToString());
        }
        sb.Append(")");
        return sb.ToString();
      }
    }


    public partial class mergeMemberToClusterArgs : TBase
    {
      private string _clusterId;
      private string _memberId;

      public string ClusterId
      {
        get
        {
          return _clusterId;
        }
        set
        {
          __isset.clusterId = true;
          this._clusterId = value;
        }
      }

      public string MemberId
      {
        get
        {
          return _memberId;
        }
        set
        {
          __isset.memberId = true;
          this._memberId = value;
        }
      }


      public Isset __isset;
      public struct Isset
      {
        public bool clusterId;
        public bool memberId;
      }

      public mergeMemberToClusterArgs()
      {
      }

      public async Task ReadAsync(TProtocol iprot, CancellationToken cancellationToken)
      {
        iprot.IncrementRecursionDepth();
        try
        {
          TField field;
          await iprot.ReadStructBeginAsync(cancellationToken);
          while (true)
          {
            field = await iprot.ReadFieldBeginAsync(cancellationToken);
            if (field.Type == TType.Stop)
            {
              break;
            }

            switch (field.ID)
            {
              case 1:
                if (field.Type == TType.String)
                {
                  ClusterId = await iprot.ReadStringAsync(cancellationToken);
                }
                else
                {
                  await TProtocolUtil.SkipAsync(iprot, field.Type, cancellationToken);
                }
                break;
              case 2:
                if (field.Type == TType.String)
                {
                  MemberId = await iprot.ReadStringAsync(cancellationToken);
                }
                else
                {
                  await TProtocolUtil.SkipAsync(iprot, field.Type, cancellationToken);
                }
                break;
              default: 
                await TProtocolUtil.SkipAsync(iprot, field.Type, cancellationToken);
                break;
            }

            await iprot.ReadFieldEndAsync(cancellationToken);
          }

          await iprot.ReadStructEndAsync(cancellationToken);
        }
        finally
        {
          iprot.DecrementRecursionDepth();
        }
      }

      public async Task WriteAsync(TProtocol oprot, CancellationToken cancellationToken)
      {
        oprot.IncrementRecursionDepth();
        try
        {
          var struc = new TStruct("mergeMemberToCluster_args");
          await oprot.WriteStructBeginAsync(struc, cancellationToken);
          var field = new TField();
          if (ClusterId != null && __isset.clusterId)
          {
            field.Name = "clusterId";
            field.Type = TType.String;
            field.ID = 1;
            await oprot.WriteFieldBeginAsync(field, cancellationToken);
            await oprot.WriteStringAsync(ClusterId, cancellationToken);
            await oprot.WriteFieldEndAsync(cancellationToken);
          }
          if (MemberId != null && __isset.memberId)
          {
            field.Name = "memberId";
            field.Type = TType.String;
            field.ID = 2;
            await oprot.WriteFieldBeginAsync(field, cancellationToken);
            await oprot.WriteStringAsync(MemberId, cancellationToken);
            await oprot.WriteFieldEndAsync(cancellationToken);
          }
          await oprot.WriteFieldStopAsync(cancellationToken);
          await oprot.WriteStructEndAsync(cancellationToken);
        }
        finally
        {
          oprot.DecrementRecursionDepth();
        }
      }

      public override bool Equals(object that)
      {
        var other = that as mergeMemberToClusterArgs;
        if (other == null) return false;
        if (ReferenceEquals(this, other)) return true;
        return ((__isset.clusterId == other.__isset.clusterId) && ((!__isset.clusterId) || (System.Object.Equals(ClusterId, other.ClusterId))))
          && ((__isset.memberId == other.__isset.memberId) && ((!__isset.memberId) || (System.Object.Equals(MemberId, other.MemberId))));
      }

      public override int GetHashCode() {
        int hashcode = 157;
        unchecked {
          if(__isset.clusterId)
            hashcode = (hashcode * 397) + ClusterId.GetHashCode();
          if(__isset.memberId)
            hashcode = (hashcode * 397) + MemberId.GetHashCode();
        }
        return hashcode;
      }

      public override string ToString()
      {
        var sb = new StringBuilder("mergeMemberToCluster_args(");
        bool __first = true;
        if (ClusterId != null && __isset.clusterId)
        {
          if(!__first) { sb.Append(", "); }
          __first = false;
          sb.Append("ClusterId: ");
          sb.Append(ClusterId);
        }
        if (MemberId != null && __isset.memberId)
        {
          if(!__first) { sb.Append(", "); }
          __first = false;
          sb.Append("MemberId: ");
          sb.Append(MemberId);
        }
        sb.Append(")");
        return sb.ToString();
      }
    }


    public partial class mergeMemberToClusterResult : TBase
    {
      private Cluster _success;

      public Cluster Success
      {
        get
        {
          return _success;
        }
        set
        {
          __isset.success = true;
          this._success = value;
        }
      }


      public Isset __isset;
      public struct Isset
      {
        public bool success;
      }

      public mergeMemberToClusterResult()
      {
      }

      public async Task ReadAsync(TProtocol iprot, CancellationToken cancellationToken)
      {
        iprot.IncrementRecursionDepth();
        try
        {
          TField field;
          await iprot.ReadStructBeginAsync(cancellationToken);
          while (true)
          {
            field = await iprot.ReadFieldBeginAsync(cancellationToken);
            if (field.Type == TType.Stop)
            {
              break;
            }

            switch (field.ID)
            {
              case 0:
                if (field.Type == TType.Struct)
                {
                  Success = new Cluster();
                  await Success.ReadAsync(iprot, cancellationToken);
                }
                else
                {
                  await TProtocolUtil.SkipAsync(iprot, field.Type, cancellationToken);
                }
                break;
              default: 
                await TProtocolUtil.SkipAsync(iprot, field.Type, cancellationToken);
                break;
            }

            await iprot.ReadFieldEndAsync(cancellationToken);
          }

          await iprot.ReadStructEndAsync(cancellationToken);
        }
        finally
        {
          iprot.DecrementRecursionDepth();
        }
      }

      public async Task WriteAsync(TProtocol oprot, CancellationToken cancellationToken)
      {
        oprot.IncrementRecursionDepth();
        try
        {
          var struc = new TStruct("mergeMemberToCluster_result");
          await oprot.WriteStructBeginAsync(struc, cancellationToken);
          var field = new TField();

          if(this.__isset.success)
          {
            if (Success != null)
            {
              field.Name = "Success";
              field.Type = TType.Struct;
              field.ID = 0;
              await oprot.WriteFieldBeginAsync(field, cancellationToken);
              await Success.WriteAsync(oprot, cancellationToken);
              await oprot.WriteFieldEndAsync(cancellationToken);
            }
          }
          await oprot.WriteFieldStopAsync(cancellationToken);
          await oprot.WriteStructEndAsync(cancellationToken);
        }
        finally
        {
          oprot.DecrementRecursionDepth();
        }
      }

      public override bool Equals(object that)
      {
        var other = that as mergeMemberToClusterResult;
        if (other == null) return false;
        if (ReferenceEquals(this, other)) return true;
        return ((__isset.success == other.__isset.success) && ((!__isset.success) || (System.Object.Equals(Success, other.Success))));
      }

      public override int GetHashCode() {
        int hashcode = 157;
        unchecked {
          if(__isset.success)
            hashcode = (hashcode * 397) + Success.GetHashCode();
        }
        return hashcode;
      }

      public override string ToString()
      {
        var sb = new StringBuilder("mergeMemberToCluster_result(");
        bool __first = true;
        if (Success != null && __isset.success)
        {
          if(!__first) { sb.Append(", "); }
          __first = false;
          sb.Append("Success: ");
          sb.Append(Success== null ? "<null>" : Success.ToString());
        }
        sb.Append(")");
        return sb.ToString();
      }
    }


    public partial class loginToCloudUsingEnvironmentArgs : TBase
    {

      public loginToCloudUsingEnvironmentArgs()
      {
      }

      public async Task ReadAsync(TProtocol iprot, CancellationToken cancellationToken)
      {
        iprot.IncrementRecursionDepth();
        try
        {
          TField field;
          await iprot.ReadStructBeginAsync(cancellationToken);
          while (true)
          {
            field = await iprot.ReadFieldBeginAsync(cancellationToken);
            if (field.Type == TType.Stop)
            {
              break;
            }

            switch (field.ID)
            {
              default: 
                await TProtocolUtil.SkipAsync(iprot, field.Type, cancellationToken);
                break;
            }

            await iprot.ReadFieldEndAsync(cancellationToken);
          }

          await iprot.ReadStructEndAsync(cancellationToken);
        }
        finally
        {
          iprot.DecrementRecursionDepth();
        }
      }

      public async Task WriteAsync(TProtocol oprot, CancellationToken cancellationToken)
      {
        oprot.IncrementRecursionDepth();
        try
        {
          var struc = new TStruct("loginToCloudUsingEnvironment_args");
          await oprot.WriteStructBeginAsync(struc, cancellationToken);
          await oprot.WriteFieldStopAsync(cancellationToken);
          await oprot.WriteStructEndAsync(cancellationToken);
        }
        finally
        {
          oprot.DecrementRecursionDepth();
        }
      }

      public override bool Equals(object that)
      {
        var other = that as loginToCloudUsingEnvironmentArgs;
        if (other == null) return false;
        if (ReferenceEquals(this, other)) return true;
        return true;
      }

      public override int GetHashCode() {
        int hashcode = 157;
        unchecked {
        }
        return hashcode;
      }

      public override string ToString()
      {
        var sb = new StringBuilder("loginToCloudUsingEnvironment_args(");
        sb.Append(")");
        return sb.ToString();
      }
    }


    public partial class loginToCloudUsingEnvironmentResult : TBase
    {
      private CloudException _cloudException;

      public CloudException CloudException
      {
        get
        {
          return _cloudException;
        }
        set
        {
          __isset.cloudException = true;
          this._cloudException = value;
        }
      }


      public Isset __isset;
      public struct Isset
      {
        public bool cloudException;
      }

      public loginToCloudUsingEnvironmentResult()
      {
      }

      public async Task ReadAsync(TProtocol iprot, CancellationToken cancellationToken)
      {
        iprot.IncrementRecursionDepth();
        try
        {
          TField field;
          await iprot.ReadStructBeginAsync(cancellationToken);
          while (true)
          {
            field = await iprot.ReadFieldBeginAsync(cancellationToken);
            if (field.Type == TType.Stop)
            {
              break;
            }

            switch (field.ID)
            {
              case 1:
                if (field.Type == TType.Struct)
                {
                  CloudException = new CloudException();
                  await CloudException.ReadAsync(iprot, cancellationToken);
                }
                else
                {
                  await TProtocolUtil.SkipAsync(iprot, field.Type, cancellationToken);
                }
                break;
              default: 
                await TProtocolUtil.SkipAsync(iprot, field.Type, cancellationToken);
                break;
            }

            await iprot.ReadFieldEndAsync(cancellationToken);
          }

          await iprot.ReadStructEndAsync(cancellationToken);
        }
        finally
        {
          iprot.DecrementRecursionDepth();
        }
      }

      public async Task WriteAsync(TProtocol oprot, CancellationToken cancellationToken)
      {
        oprot.IncrementRecursionDepth();
        try
        {
          var struc = new TStruct("loginToCloudUsingEnvironment_result");
          await oprot.WriteStructBeginAsync(struc, cancellationToken);
          var field = new TField();

          if(this.__isset.cloudException)
          {
            if (CloudException != null)
            {
              field.Name = "CloudException";
              field.Type = TType.Struct;
              field.ID = 1;
              await oprot.WriteFieldBeginAsync(field, cancellationToken);
              await CloudException.WriteAsync(oprot, cancellationToken);
              await oprot.WriteFieldEndAsync(cancellationToken);
            }
          }
          await oprot.WriteFieldStopAsync(cancellationToken);
          await oprot.WriteStructEndAsync(cancellationToken);
        }
        finally
        {
          oprot.DecrementRecursionDepth();
        }
      }

      public override bool Equals(object that)
      {
        var other = that as loginToCloudUsingEnvironmentResult;
        if (other == null) return false;
        if (ReferenceEquals(this, other)) return true;
        return ((__isset.cloudException == other.__isset.cloudException) && ((!__isset.cloudException) || (System.Object.Equals(CloudException, other.CloudException))));
      }

      public override int GetHashCode() {
        int hashcode = 157;
        unchecked {
          if(__isset.cloudException)
            hashcode = (hashcode * 397) + CloudException.GetHashCode();
        }
        return hashcode;
      }

      public override string ToString()
      {
        var sb = new StringBuilder("loginToCloudUsingEnvironment_result(");
        bool __first = true;
        if (CloudException != null && __isset.cloudException)
        {
          if(!__first) { sb.Append(", "); }
          __first = false;
          sb.Append("CloudException: ");
          sb.Append(CloudException== null ? "<null>" : CloudException.ToString());
        }
        sb.Append(")");
        return sb.ToString();
      }
    }


    public partial class loginToCloudArgs : TBase
    {
      private string _baseUrl;
      private string _apiKey;
      private string _apiSecret;

      public string BaseUrl
      {
        get
        {
          return _baseUrl;
        }
        set
        {
          __isset.baseUrl = true;
          this._baseUrl = value;
        }
      }

      public string ApiKey
      {
        get
        {
          return _apiKey;
        }
        set
        {
          __isset.apiKey = true;
          this._apiKey = value;
        }
      }

      public string ApiSecret
      {
        get
        {
          return _apiSecret;
        }
        set
        {
          __isset.apiSecret = true;
          this._apiSecret = value;
        }
      }


      public Isset __isset;
      public struct Isset
      {
        public bool baseUrl;
        public bool apiKey;
        public bool apiSecret;
      }

      public loginToCloudArgs()
      {
      }

      public async Task ReadAsync(TProtocol iprot, CancellationToken cancellationToken)
      {
        iprot.IncrementRecursionDepth();
        try
        {
          TField field;
          await iprot.ReadStructBeginAsync(cancellationToken);
          while (true)
          {
            field = await iprot.ReadFieldBeginAsync(cancellationToken);
            if (field.Type == TType.Stop)
            {
              break;
            }

            switch (field.ID)
            {
              case 1:
                if (field.Type == TType.String)
                {
                  BaseUrl = await iprot.ReadStringAsync(cancellationToken);
                }
                else
                {
                  await TProtocolUtil.SkipAsync(iprot, field.Type, cancellationToken);
                }
                break;
              case 2:
                if (field.Type == TType.String)
                {
                  ApiKey = await iprot.ReadStringAsync(cancellationToken);
                }
                else
                {
                  await TProtocolUtil.SkipAsync(iprot, field.Type, cancellationToken);
                }
                break;
              case 3:
                if (field.Type == TType.String)
                {
                  ApiSecret = await iprot.ReadStringAsync(cancellationToken);
                }
                else
                {
                  await TProtocolUtil.SkipAsync(iprot, field.Type, cancellationToken);
                }
                break;
              default: 
                await TProtocolUtil.SkipAsync(iprot, field.Type, cancellationToken);
                break;
            }

            await iprot.ReadFieldEndAsync(cancellationToken);
          }

          await iprot.ReadStructEndAsync(cancellationToken);
        }
        finally
        {
          iprot.DecrementRecursionDepth();
        }
      }

      public async Task WriteAsync(TProtocol oprot, CancellationToken cancellationToken)
      {
        oprot.IncrementRecursionDepth();
        try
        {
          var struc = new TStruct("loginToCloud_args");
          await oprot.WriteStructBeginAsync(struc, cancellationToken);
          var field = new TField();
          if (BaseUrl != null && __isset.baseUrl)
          {
            field.Name = "baseUrl";
            field.Type = TType.String;
            field.ID = 1;
            await oprot.WriteFieldBeginAsync(field, cancellationToken);
            await oprot.WriteStringAsync(BaseUrl, cancellationToken);
            await oprot.WriteFieldEndAsync(cancellationToken);
          }
          if (ApiKey != null && __isset.apiKey)
          {
            field.Name = "apiKey";
            field.Type = TType.String;
            field.ID = 2;
            await oprot.WriteFieldBeginAsync(field, cancellationToken);
            await oprot.WriteStringAsync(ApiKey, cancellationToken);
            await oprot.WriteFieldEndAsync(cancellationToken);
          }
          if (ApiSecret != null && __isset.apiSecret)
          {
            field.Name = "apiSecret";
            field.Type = TType.String;
            field.ID = 3;
            await oprot.WriteFieldBeginAsync(field, cancellationToken);
            await oprot.WriteStringAsync(ApiSecret, cancellationToken);
            await oprot.WriteFieldEndAsync(cancellationToken);
          }
          await oprot.WriteFieldStopAsync(cancellationToken);
          await oprot.WriteStructEndAsync(cancellationToken);
        }
        finally
        {
          oprot.DecrementRecursionDepth();
        }
      }

      public override bool Equals(object that)
      {
        var other = that as loginToCloudArgs;
        if (other == null) return false;
        if (ReferenceEquals(this, other)) return true;
        return ((__isset.baseUrl == other.__isset.baseUrl) && ((!__isset.baseUrl) || (System.Object.Equals(BaseUrl, other.BaseUrl))))
          && ((__isset.apiKey == other.__isset.apiKey) && ((!__isset.apiKey) || (System.Object.Equals(ApiKey, other.ApiKey))))
          && ((__isset.apiSecret == other.__isset.apiSecret) && ((!__isset.apiSecret) || (System.Object.Equals(ApiSecret, other.ApiSecret))));
      }

      public override int GetHashCode() {
        int hashcode = 157;
        unchecked {
          if(__isset.baseUrl)
            hashcode = (hashcode * 397) + BaseUrl.GetHashCode();
          if(__isset.apiKey)
            hashcode = (hashcode * 397) + ApiKey.GetHashCode();
          if(__isset.apiSecret)
            hashcode = (hashcode * 397) + ApiSecret.GetHashCode();
        }
        return hashcode;
      }

      public override string ToString()
      {
        var sb = new StringBuilder("loginToCloud_args(");
        bool __first = true;
        if (BaseUrl != null && __isset.baseUrl)
        {
          if(!__first) { sb.Append(", "); }
          __first = false;
          sb.Append("BaseUrl: ");
          sb.Append(BaseUrl);
        }
        if (ApiKey != null && __isset.apiKey)
        {
          if(!__first) { sb.Append(", "); }
          __first = false;
          sb.Append("ApiKey: ");
          sb.Append(ApiKey);
        }
        if (ApiSecret != null && __isset.apiSecret)
        {
          if(!__first) { sb.Append(", "); }
          __first = false;
          sb.Append("ApiSecret: ");
          sb.Append(ApiSecret);
        }
        sb.Append(")");
        return sb.ToString();
      }
    }


    public partial class loginToCloudResult : TBase
    {
      private CloudException _cloudException;

      public CloudException CloudException
      {
        get
        {
          return _cloudException;
        }
        set
        {
          __isset.cloudException = true;
          this._cloudException = value;
        }
      }


      public Isset __isset;
      public struct Isset
      {
        public bool cloudException;
      }

      public loginToCloudResult()
      {
      }

      public async Task ReadAsync(TProtocol iprot, CancellationToken cancellationToken)
      {
        iprot.IncrementRecursionDepth();
        try
        {
          TField field;
          await iprot.ReadStructBeginAsync(cancellationToken);
          while (true)
          {
            field = await iprot.ReadFieldBeginAsync(cancellationToken);
            if (field.Type == TType.Stop)
            {
              break;
            }

            switch (field.ID)
            {
              case 1:
                if (field.Type == TType.Struct)
                {
                  CloudException = new CloudException();
                  await CloudException.ReadAsync(iprot, cancellationToken);
                }
                else
                {
                  await TProtocolUtil.SkipAsync(iprot, field.Type, cancellationToken);
                }
                break;
              default: 
                await TProtocolUtil.SkipAsync(iprot, field.Type, cancellationToken);
                break;
            }

            await iprot.ReadFieldEndAsync(cancellationToken);
          }

          await iprot.ReadStructEndAsync(cancellationToken);
        }
        finally
        {
          iprot.DecrementRecursionDepth();
        }
      }

      public async Task WriteAsync(TProtocol oprot, CancellationToken cancellationToken)
      {
        oprot.IncrementRecursionDepth();
        try
        {
          var struc = new TStruct("loginToCloud_result");
          await oprot.WriteStructBeginAsync(struc, cancellationToken);
          var field = new TField();

          if(this.__isset.cloudException)
          {
            if (CloudException != null)
            {
              field.Name = "CloudException";
              field.Type = TType.Struct;
              field.ID = 1;
              await oprot.WriteFieldBeginAsync(field, cancellationToken);
              await CloudException.WriteAsync(oprot, cancellationToken);
              await oprot.WriteFieldEndAsync(cancellationToken);
            }
          }
          await oprot.WriteFieldStopAsync(cancellationToken);
          await oprot.WriteStructEndAsync(cancellationToken);
        }
        finally
        {
          oprot.DecrementRecursionDepth();
        }
      }

      public override bool Equals(object that)
      {
        var other = that as loginToCloudResult;
        if (other == null) return false;
        if (ReferenceEquals(this, other)) return true;
        return ((__isset.cloudException == other.__isset.cloudException) && ((!__isset.cloudException) || (System.Object.Equals(CloudException, other.CloudException))));
      }

      public override int GetHashCode() {
        int hashcode = 157;
        unchecked {
          if(__isset.cloudException)
            hashcode = (hashcode * 397) + CloudException.GetHashCode();
        }
        return hashcode;
      }

      public override string ToString()
      {
        var sb = new StringBuilder("loginToCloud_result(");
        bool __first = true;
        if (CloudException != null && __isset.cloudException)
        {
          if(!__first) { sb.Append(", "); }
          __first = false;
          sb.Append("CloudException: ");
          sb.Append(CloudException== null ? "<null>" : CloudException.ToString());
        }
        sb.Append(")");
        return sb.ToString();
      }
    }


    public partial class createCloudClusterArgs : TBase
    {
      private string _hazelcastVersion;
      private bool _isTlsEnabled;

      public string HazelcastVersion
      {
        get
        {
          return _hazelcastVersion;
        }
        set
        {
          __isset.hazelcastVersion = true;
          this._hazelcastVersion = value;
        }
      }

      public bool IsTlsEnabled
      {
        get
        {
          return _isTlsEnabled;
        }
        set
        {
          __isset.isTlsEnabled = true;
          this._isTlsEnabled = value;
        }
      }


      public Isset __isset;
      public struct Isset
      {
        public bool hazelcastVersion;
        public bool isTlsEnabled;
      }

      public createCloudClusterArgs()
      {
      }

      public async Task ReadAsync(TProtocol iprot, CancellationToken cancellationToken)
      {
        iprot.IncrementRecursionDepth();
        try
        {
          TField field;
          await iprot.ReadStructBeginAsync(cancellationToken);
          while (true)
          {
            field = await iprot.ReadFieldBeginAsync(cancellationToken);
            if (field.Type == TType.Stop)
            {
              break;
            }

            switch (field.ID)
            {
              case 1:
                if (field.Type == TType.String)
                {
                  HazelcastVersion = await iprot.ReadStringAsync(cancellationToken);
                }
                else
                {
                  await TProtocolUtil.SkipAsync(iprot, field.Type, cancellationToken);
                }
                break;
              case 2:
                if (field.Type == TType.Bool)
                {
                  IsTlsEnabled = await iprot.ReadBoolAsync(cancellationToken);
                }
                else
                {
                  await TProtocolUtil.SkipAsync(iprot, field.Type, cancellationToken);
                }
                break;
              default: 
                await TProtocolUtil.SkipAsync(iprot, field.Type, cancellationToken);
                break;
            }

            await iprot.ReadFieldEndAsync(cancellationToken);
          }

          await iprot.ReadStructEndAsync(cancellationToken);
        }
        finally
        {
          iprot.DecrementRecursionDepth();
        }
      }

      public async Task WriteAsync(TProtocol oprot, CancellationToken cancellationToken)
      {
        oprot.IncrementRecursionDepth();
        try
        {
          var struc = new TStruct("createCloudCluster_args");
          await oprot.WriteStructBeginAsync(struc, cancellationToken);
          var field = new TField();
          if (HazelcastVersion != null && __isset.hazelcastVersion)
          {
            field.Name = "hazelcastVersion";
            field.Type = TType.String;
            field.ID = 1;
            await oprot.WriteFieldBeginAsync(field, cancellationToken);
            await oprot.WriteStringAsync(HazelcastVersion, cancellationToken);
            await oprot.WriteFieldEndAsync(cancellationToken);
          }
          if (__isset.isTlsEnabled)
          {
            field.Name = "isTlsEnabled";
            field.Type = TType.Bool;
            field.ID = 2;
            await oprot.WriteFieldBeginAsync(field, cancellationToken);
            await oprot.WriteBoolAsync(IsTlsEnabled, cancellationToken);
            await oprot.WriteFieldEndAsync(cancellationToken);
          }
          await oprot.WriteFieldStopAsync(cancellationToken);
          await oprot.WriteStructEndAsync(cancellationToken);
        }
        finally
        {
          oprot.DecrementRecursionDepth();
        }
      }

      public override bool Equals(object that)
      {
        var other = that as createCloudClusterArgs;
        if (other == null) return false;
        if (ReferenceEquals(this, other)) return true;
        return ((__isset.hazelcastVersion == other.__isset.hazelcastVersion) && ((!__isset.hazelcastVersion) || (System.Object.Equals(HazelcastVersion, other.HazelcastVersion))))
          && ((__isset.isTlsEnabled == other.__isset.isTlsEnabled) && ((!__isset.isTlsEnabled) || (System.Object.Equals(IsTlsEnabled, other.IsTlsEnabled))));
      }

      public override int GetHashCode() {
        int hashcode = 157;
        unchecked {
          if(__isset.hazelcastVersion)
            hashcode = (hashcode * 397) + HazelcastVersion.GetHashCode();
          if(__isset.isTlsEnabled)
            hashcode = (hashcode * 397) + IsTlsEnabled.GetHashCode();
        }
        return hashcode;
      }

      public override string ToString()
      {
        var sb = new StringBuilder("createCloudCluster_args(");
        bool __first = true;
        if (HazelcastVersion != null && __isset.hazelcastVersion)
        {
          if(!__first) { sb.Append(", "); }
          __first = false;
          sb.Append("HazelcastVersion: ");
          sb.Append(HazelcastVersion);
        }
        if (__isset.isTlsEnabled)
        {
          if(!__first) { sb.Append(", "); }
          __first = false;
          sb.Append("IsTlsEnabled: ");
          sb.Append(IsTlsEnabled);
        }
        sb.Append(")");
        return sb.ToString();
      }
    }


    public partial class createCloudClusterResult : TBase
    {
      private CloudCluster _success;
      private CloudException _cloudException;

      public CloudCluster Success
      {
        get
        {
          return _success;
        }
        set
        {
          __isset.success = true;
          this._success = value;
        }
      }

      public CloudException CloudException
      {
        get
        {
          return _cloudException;
        }
        set
        {
          __isset.cloudException = true;
          this._cloudException = value;
        }
      }


      public Isset __isset;
      public struct Isset
      {
        public bool success;
        public bool cloudException;
      }

      public createCloudClusterResult()
      {
      }

      public async Task ReadAsync(TProtocol iprot, CancellationToken cancellationToken)
      {
        iprot.IncrementRecursionDepth();
        try
        {
          TField field;
          await iprot.ReadStructBeginAsync(cancellationToken);
          while (true)
          {
            field = await iprot.ReadFieldBeginAsync(cancellationToken);
            if (field.Type == TType.Stop)
            {
              break;
            }

            switch (field.ID)
            {
              case 0:
                if (field.Type == TType.Struct)
                {
                  Success = new CloudCluster();
                  await Success.ReadAsync(iprot, cancellationToken);
                }
                else
                {
                  await TProtocolUtil.SkipAsync(iprot, field.Type, cancellationToken);
                }
                break;
              case 1:
                if (field.Type == TType.Struct)
                {
                  CloudException = new CloudException();
                  await CloudException.ReadAsync(iprot, cancellationToken);
                }
                else
                {
                  await TProtocolUtil.SkipAsync(iprot, field.Type, cancellationToken);
                }
                break;
              default: 
                await TProtocolUtil.SkipAsync(iprot, field.Type, cancellationToken);
                break;
            }

            await iprot.ReadFieldEndAsync(cancellationToken);
          }

          await iprot.ReadStructEndAsync(cancellationToken);
        }
        finally
        {
          iprot.DecrementRecursionDepth();
        }
      }

      public async Task WriteAsync(TProtocol oprot, CancellationToken cancellationToken)
      {
        oprot.IncrementRecursionDepth();
        try
        {
          var struc = new TStruct("createCloudCluster_result");
          await oprot.WriteStructBeginAsync(struc, cancellationToken);
          var field = new TField();

          if(this.__isset.success)
          {
            if (Success != null)
            {
              field.Name = "Success";
              field.Type = TType.Struct;
              field.ID = 0;
              await oprot.WriteFieldBeginAsync(field, cancellationToken);
              await Success.WriteAsync(oprot, cancellationToken);
              await oprot.WriteFieldEndAsync(cancellationToken);
            }
          }
          else if(this.__isset.cloudException)
          {
            if (CloudException != null)
            {
              field.Name = "CloudException";
              field.Type = TType.Struct;
              field.ID = 1;
              await oprot.WriteFieldBeginAsync(field, cancellationToken);
              await CloudException.WriteAsync(oprot, cancellationToken);
              await oprot.WriteFieldEndAsync(cancellationToken);
            }
          }
          await oprot.WriteFieldStopAsync(cancellationToken);
          await oprot.WriteStructEndAsync(cancellationToken);
        }
        finally
        {
          oprot.DecrementRecursionDepth();
        }
      }

      public override bool Equals(object that)
      {
        var other = that as createCloudClusterResult;
        if (other == null) return false;
        if (ReferenceEquals(this, other)) return true;
        return ((__isset.success == other.__isset.success) && ((!__isset.success) || (System.Object.Equals(Success, other.Success))))
          && ((__isset.cloudException == other.__isset.cloudException) && ((!__isset.cloudException) || (System.Object.Equals(CloudException, other.CloudException))));
      }

      public override int GetHashCode() {
        int hashcode = 157;
        unchecked {
          if(__isset.success)
            hashcode = (hashcode * 397) + Success.GetHashCode();
          if(__isset.cloudException)
            hashcode = (hashcode * 397) + CloudException.GetHashCode();
        }
        return hashcode;
      }

      public override string ToString()
      {
        var sb = new StringBuilder("createCloudCluster_result(");
        bool __first = true;
        if (Success != null && __isset.success)
        {
          if(!__first) { sb.Append(", "); }
          __first = false;
          sb.Append("Success: ");
          sb.Append(Success== null ? "<null>" : Success.ToString());
        }
        if (CloudException != null && __isset.cloudException)
        {
          if(!__first) { sb.Append(", "); }
          __first = false;
          sb.Append("CloudException: ");
          sb.Append(CloudException== null ? "<null>" : CloudException.ToString());
        }
        sb.Append(")");
        return sb.ToString();
      }
    }


    public partial class getCloudClusterArgs : TBase
    {
      private string _cloudClusterId;

      public string CloudClusterId
      {
        get
        {
          return _cloudClusterId;
        }
        set
        {
          __isset.cloudClusterId = true;
          this._cloudClusterId = value;
        }
      }


      public Isset __isset;
      public struct Isset
      {
        public bool cloudClusterId;
      }

      public getCloudClusterArgs()
      {
      }

      public async Task ReadAsync(TProtocol iprot, CancellationToken cancellationToken)
      {
        iprot.IncrementRecursionDepth();
        try
        {
          TField field;
          await iprot.ReadStructBeginAsync(cancellationToken);
          while (true)
          {
            field = await iprot.ReadFieldBeginAsync(cancellationToken);
            if (field.Type == TType.Stop)
            {
              break;
            }

            switch (field.ID)
            {
              case 1:
                if (field.Type == TType.String)
                {
                  CloudClusterId = await iprot.ReadStringAsync(cancellationToken);
                }
                else
                {
                  await TProtocolUtil.SkipAsync(iprot, field.Type, cancellationToken);
                }
                break;
              default: 
                await TProtocolUtil.SkipAsync(iprot, field.Type, cancellationToken);
                break;
            }

            await iprot.ReadFieldEndAsync(cancellationToken);
          }

          await iprot.ReadStructEndAsync(cancellationToken);
        }
        finally
        {
          iprot.DecrementRecursionDepth();
        }
      }

      public async Task WriteAsync(TProtocol oprot, CancellationToken cancellationToken)
      {
        oprot.IncrementRecursionDepth();
        try
        {
          var struc = new TStruct("getCloudCluster_args");
          await oprot.WriteStructBeginAsync(struc, cancellationToken);
          var field = new TField();
          if (CloudClusterId != null && __isset.cloudClusterId)
          {
            field.Name = "cloudClusterId";
            field.Type = TType.String;
            field.ID = 1;
            await oprot.WriteFieldBeginAsync(field, cancellationToken);
            await oprot.WriteStringAsync(CloudClusterId, cancellationToken);
            await oprot.WriteFieldEndAsync(cancellationToken);
          }
          await oprot.WriteFieldStopAsync(cancellationToken);
          await oprot.WriteStructEndAsync(cancellationToken);
        }
        finally
        {
          oprot.DecrementRecursionDepth();
        }
      }

      public override bool Equals(object that)
      {
        var other = that as getCloudClusterArgs;
        if (other == null) return false;
        if (ReferenceEquals(this, other)) return true;
        return ((__isset.cloudClusterId == other.__isset.cloudClusterId) && ((!__isset.cloudClusterId) || (System.Object.Equals(CloudClusterId, other.CloudClusterId))));
      }

      public override int GetHashCode() {
        int hashcode = 157;
        unchecked {
          if(__isset.cloudClusterId)
            hashcode = (hashcode * 397) + CloudClusterId.GetHashCode();
        }
        return hashcode;
      }

      public override string ToString()
      {
        var sb = new StringBuilder("getCloudCluster_args(");
        bool __first = true;
        if (CloudClusterId != null && __isset.cloudClusterId)
        {
          if(!__first) { sb.Append(", "); }
          __first = false;
          sb.Append("CloudClusterId: ");
          sb.Append(CloudClusterId);
        }
        sb.Append(")");
        return sb.ToString();
      }
    }


    public partial class getCloudClusterResult : TBase
    {
      private CloudCluster _success;
      private CloudException _cloudException;

      public CloudCluster Success
      {
        get
        {
          return _success;
        }
        set
        {
          __isset.success = true;
          this._success = value;
        }
      }

      public CloudException CloudException
      {
        get
        {
          return _cloudException;
        }
        set
        {
          __isset.cloudException = true;
          this._cloudException = value;
        }
      }


      public Isset __isset;
      public struct Isset
      {
        public bool success;
        public bool cloudException;
      }

      public getCloudClusterResult()
      {
      }

      public async Task ReadAsync(TProtocol iprot, CancellationToken cancellationToken)
      {
        iprot.IncrementRecursionDepth();
        try
        {
          TField field;
          await iprot.ReadStructBeginAsync(cancellationToken);
          while (true)
          {
            field = await iprot.ReadFieldBeginAsync(cancellationToken);
            if (field.Type == TType.Stop)
            {
              break;
            }

            switch (field.ID)
            {
              case 0:
                if (field.Type == TType.Struct)
                {
                  Success = new CloudCluster();
                  await Success.ReadAsync(iprot, cancellationToken);
                }
                else
                {
                  await TProtocolUtil.SkipAsync(iprot, field.Type, cancellationToken);
                }
                break;
              case 1:
                if (field.Type == TType.Struct)
                {
                  CloudException = new CloudException();
                  await CloudException.ReadAsync(iprot, cancellationToken);
                }
                else
                {
                  await TProtocolUtil.SkipAsync(iprot, field.Type, cancellationToken);
                }
                break;
              default: 
                await TProtocolUtil.SkipAsync(iprot, field.Type, cancellationToken);
                break;
            }

            await iprot.ReadFieldEndAsync(cancellationToken);
          }

          await iprot.ReadStructEndAsync(cancellationToken);
        }
        finally
        {
          iprot.DecrementRecursionDepth();
        }
      }

      public async Task WriteAsync(TProtocol oprot, CancellationToken cancellationToken)
      {
        oprot.IncrementRecursionDepth();
        try
        {
          var struc = new TStruct("getCloudCluster_result");
          await oprot.WriteStructBeginAsync(struc, cancellationToken);
          var field = new TField();

          if(this.__isset.success)
          {
            if (Success != null)
            {
              field.Name = "Success";
              field.Type = TType.Struct;
              field.ID = 0;
              await oprot.WriteFieldBeginAsync(field, cancellationToken);
              await Success.WriteAsync(oprot, cancellationToken);
              await oprot.WriteFieldEndAsync(cancellationToken);
            }
          }
          else if(this.__isset.cloudException)
          {
            if (CloudException != null)
            {
              field.Name = "CloudException";
              field.Type = TType.Struct;
              field.ID = 1;
              await oprot.WriteFieldBeginAsync(field, cancellationToken);
              await CloudException.WriteAsync(oprot, cancellationToken);
              await oprot.WriteFieldEndAsync(cancellationToken);
            }
          }
          await oprot.WriteFieldStopAsync(cancellationToken);
          await oprot.WriteStructEndAsync(cancellationToken);
        }
        finally
        {
          oprot.DecrementRecursionDepth();
        }
      }

      public override bool Equals(object that)
      {
        var other = that as getCloudClusterResult;
        if (other == null) return false;
        if (ReferenceEquals(this, other)) return true;
        return ((__isset.success == other.__isset.success) && ((!__isset.success) || (System.Object.Equals(Success, other.Success))))
          && ((__isset.cloudException == other.__isset.cloudException) && ((!__isset.cloudException) || (System.Object.Equals(CloudException, other.CloudException))));
      }

      public override int GetHashCode() {
        int hashcode = 157;
        unchecked {
          if(__isset.success)
            hashcode = (hashcode * 397) + Success.GetHashCode();
          if(__isset.cloudException)
            hashcode = (hashcode * 397) + CloudException.GetHashCode();
        }
        return hashcode;
      }

      public override string ToString()
      {
        var sb = new StringBuilder("getCloudCluster_result(");
        bool __first = true;
        if (Success != null && __isset.success)
        {
          if(!__first) { sb.Append(", "); }
          __first = false;
          sb.Append("Success: ");
          sb.Append(Success== null ? "<null>" : Success.ToString());
        }
        if (CloudException != null && __isset.cloudException)
        {
          if(!__first) { sb.Append(", "); }
          __first = false;
          sb.Append("CloudException: ");
          sb.Append(CloudException== null ? "<null>" : CloudException.ToString());
        }
        sb.Append(")");
        return sb.ToString();
      }
    }


    public partial class stopCloudClusterArgs : TBase
    {
      private string _cloudClusterId;

      public string CloudClusterId
      {
        get
        {
          return _cloudClusterId;
        }
        set
        {
          __isset.cloudClusterId = true;
          this._cloudClusterId = value;
        }
      }


      public Isset __isset;
      public struct Isset
      {
        public bool cloudClusterId;
      }

      public stopCloudClusterArgs()
      {
      }

      public async Task ReadAsync(TProtocol iprot, CancellationToken cancellationToken)
      {
        iprot.IncrementRecursionDepth();
        try
        {
          TField field;
          await iprot.ReadStructBeginAsync(cancellationToken);
          while (true)
          {
            field = await iprot.ReadFieldBeginAsync(cancellationToken);
            if (field.Type == TType.Stop)
            {
              break;
            }

            switch (field.ID)
            {
              case 1:
                if (field.Type == TType.String)
                {
                  CloudClusterId = await iprot.ReadStringAsync(cancellationToken);
                }
                else
                {
                  await TProtocolUtil.SkipAsync(iprot, field.Type, cancellationToken);
                }
                break;
              default: 
                await TProtocolUtil.SkipAsync(iprot, field.Type, cancellationToken);
                break;
            }

            await iprot.ReadFieldEndAsync(cancellationToken);
          }

          await iprot.ReadStructEndAsync(cancellationToken);
        }
        finally
        {
          iprot.DecrementRecursionDepth();
        }
      }

      public async Task WriteAsync(TProtocol oprot, CancellationToken cancellationToken)
      {
        oprot.IncrementRecursionDepth();
        try
        {
          var struc = new TStruct("stopCloudCluster_args");
          await oprot.WriteStructBeginAsync(struc, cancellationToken);
          var field = new TField();
          if (CloudClusterId != null && __isset.cloudClusterId)
          {
            field.Name = "cloudClusterId";
            field.Type = TType.String;
            field.ID = 1;
            await oprot.WriteFieldBeginAsync(field, cancellationToken);
            await oprot.WriteStringAsync(CloudClusterId, cancellationToken);
            await oprot.WriteFieldEndAsync(cancellationToken);
          }
          await oprot.WriteFieldStopAsync(cancellationToken);
          await oprot.WriteStructEndAsync(cancellationToken);
        }
        finally
        {
          oprot.DecrementRecursionDepth();
        }
      }

      public override bool Equals(object that)
      {
        var other = that as stopCloudClusterArgs;
        if (other == null) return false;
        if (ReferenceEquals(this, other)) return true;
        return ((__isset.cloudClusterId == other.__isset.cloudClusterId) && ((!__isset.cloudClusterId) || (System.Object.Equals(CloudClusterId, other.CloudClusterId))));
      }

      public override int GetHashCode() {
        int hashcode = 157;
        unchecked {
          if(__isset.cloudClusterId)
            hashcode = (hashcode * 397) + CloudClusterId.GetHashCode();
        }
        return hashcode;
      }

      public override string ToString()
      {
        var sb = new StringBuilder("stopCloudCluster_args(");
        bool __first = true;
        if (CloudClusterId != null && __isset.cloudClusterId)
        {
          if(!__first) { sb.Append(", "); }
          __first = false;
          sb.Append("CloudClusterId: ");
          sb.Append(CloudClusterId);
        }
        sb.Append(")");
        return sb.ToString();
      }
    }


    public partial class stopCloudClusterResult : TBase
    {
      private CloudCluster _success;
      private CloudException _cloudException;

      public CloudCluster Success
      {
        get
        {
          return _success;
        }
        set
        {
          __isset.success = true;
          this._success = value;
        }
      }

      public CloudException CloudException
      {
        get
        {
          return _cloudException;
        }
        set
        {
          __isset.cloudException = true;
          this._cloudException = value;
        }
      }


      public Isset __isset;
      public struct Isset
      {
        public bool success;
        public bool cloudException;
      }

      public stopCloudClusterResult()
      {
      }

      public async Task ReadAsync(TProtocol iprot, CancellationToken cancellationToken)
      {
        iprot.IncrementRecursionDepth();
        try
        {
          TField field;
          await iprot.ReadStructBeginAsync(cancellationToken);
          while (true)
          {
            field = await iprot.ReadFieldBeginAsync(cancellationToken);
            if (field.Type == TType.Stop)
            {
              break;
            }

            switch (field.ID)
            {
              case 0:
                if (field.Type == TType.Struct)
                {
                  Success = new CloudCluster();
                  await Success.ReadAsync(iprot, cancellationToken);
                }
                else
                {
                  await TProtocolUtil.SkipAsync(iprot, field.Type, cancellationToken);
                }
                break;
              case 1:
                if (field.Type == TType.Struct)
                {
                  CloudException = new CloudException();
                  await CloudException.ReadAsync(iprot, cancellationToken);
                }
                else
                {
                  await TProtocolUtil.SkipAsync(iprot, field.Type, cancellationToken);
                }
                break;
              default: 
                await TProtocolUtil.SkipAsync(iprot, field.Type, cancellationToken);
                break;
            }

            await iprot.ReadFieldEndAsync(cancellationToken);
          }

          await iprot.ReadStructEndAsync(cancellationToken);
        }
        finally
        {
          iprot.DecrementRecursionDepth();
        }
      }

      public async Task WriteAsync(TProtocol oprot, CancellationToken cancellationToken)
      {
        oprot.IncrementRecursionDepth();
        try
        {
          var struc = new TStruct("stopCloudCluster_result");
          await oprot.WriteStructBeginAsync(struc, cancellationToken);
          var field = new TField();

          if(this.__isset.success)
          {
            if (Success != null)
            {
              field.Name = "Success";
              field.Type = TType.Struct;
              field.ID = 0;
              await oprot.WriteFieldBeginAsync(field, cancellationToken);
              await Success.WriteAsync(oprot, cancellationToken);
              await oprot.WriteFieldEndAsync(cancellationToken);
            }
          }
          else if(this.__isset.cloudException)
          {
            if (CloudException != null)
            {
              field.Name = "CloudException";
              field.Type = TType.Struct;
              field.ID = 1;
              await oprot.WriteFieldBeginAsync(field, cancellationToken);
              await CloudException.WriteAsync(oprot, cancellationToken);
              await oprot.WriteFieldEndAsync(cancellationToken);
            }
          }
          await oprot.WriteFieldStopAsync(cancellationToken);
          await oprot.WriteStructEndAsync(cancellationToken);
        }
        finally
        {
          oprot.DecrementRecursionDepth();
        }
      }

      public override bool Equals(object that)
      {
        var other = that as stopCloudClusterResult;
        if (other == null) return false;
        if (ReferenceEquals(this, other)) return true;
        return ((__isset.success == other.__isset.success) && ((!__isset.success) || (System.Object.Equals(Success, other.Success))))
          && ((__isset.cloudException == other.__isset.cloudException) && ((!__isset.cloudException) || (System.Object.Equals(CloudException, other.CloudException))));
      }

      public override int GetHashCode() {
        int hashcode = 157;
        unchecked {
          if(__isset.success)
            hashcode = (hashcode * 397) + Success.GetHashCode();
          if(__isset.cloudException)
            hashcode = (hashcode * 397) + CloudException.GetHashCode();
        }
        return hashcode;
      }

      public override string ToString()
      {
        var sb = new StringBuilder("stopCloudCluster_result(");
        bool __first = true;
        if (Success != null && __isset.success)
        {
          if(!__first) { sb.Append(", "); }
          __first = false;
          sb.Append("Success: ");
          sb.Append(Success== null ? "<null>" : Success.ToString());
        }
        if (CloudException != null && __isset.cloudException)
        {
          if(!__first) { sb.Append(", "); }
          __first = false;
          sb.Append("CloudException: ");
          sb.Append(CloudException== null ? "<null>" : CloudException.ToString());
        }
        sb.Append(")");
        return sb.ToString();
      }
    }


    public partial class resumeCloudClusterArgs : TBase
    {
      private string _cloudClusterId;

      public string CloudClusterId
      {
        get
        {
          return _cloudClusterId;
        }
        set
        {
          __isset.cloudClusterId = true;
          this._cloudClusterId = value;
        }
      }


      public Isset __isset;
      public struct Isset
      {
        public bool cloudClusterId;
      }

      public resumeCloudClusterArgs()
      {
      }

      public async Task ReadAsync(TProtocol iprot, CancellationToken cancellationToken)
      {
        iprot.IncrementRecursionDepth();
        try
        {
          TField field;
          await iprot.ReadStructBeginAsync(cancellationToken);
          while (true)
          {
            field = await iprot.ReadFieldBeginAsync(cancellationToken);
            if (field.Type == TType.Stop)
            {
              break;
            }

            switch (field.ID)
            {
              case 1:
                if (field.Type == TType.String)
                {
                  CloudClusterId = await iprot.ReadStringAsync(cancellationToken);
                }
                else
                {
                  await TProtocolUtil.SkipAsync(iprot, field.Type, cancellationToken);
                }
                break;
              default: 
                await TProtocolUtil.SkipAsync(iprot, field.Type, cancellationToken);
                break;
            }

            await iprot.ReadFieldEndAsync(cancellationToken);
          }

          await iprot.ReadStructEndAsync(cancellationToken);
        }
        finally
        {
          iprot.DecrementRecursionDepth();
        }
      }

      public async Task WriteAsync(TProtocol oprot, CancellationToken cancellationToken)
      {
        oprot.IncrementRecursionDepth();
        try
        {
          var struc = new TStruct("resumeCloudCluster_args");
          await oprot.WriteStructBeginAsync(struc, cancellationToken);
          var field = new TField();
          if (CloudClusterId != null && __isset.cloudClusterId)
          {
            field.Name = "cloudClusterId";
            field.Type = TType.String;
            field.ID = 1;
            await oprot.WriteFieldBeginAsync(field, cancellationToken);
            await oprot.WriteStringAsync(CloudClusterId, cancellationToken);
            await oprot.WriteFieldEndAsync(cancellationToken);
          }
          await oprot.WriteFieldStopAsync(cancellationToken);
          await oprot.WriteStructEndAsync(cancellationToken);
        }
        finally
        {
          oprot.DecrementRecursionDepth();
        }
      }

      public override bool Equals(object that)
      {
        var other = that as resumeCloudClusterArgs;
        if (other == null) return false;
        if (ReferenceEquals(this, other)) return true;
        return ((__isset.cloudClusterId == other.__isset.cloudClusterId) && ((!__isset.cloudClusterId) || (System.Object.Equals(CloudClusterId, other.CloudClusterId))));
      }

      public override int GetHashCode() {
        int hashcode = 157;
        unchecked {
          if(__isset.cloudClusterId)
            hashcode = (hashcode * 397) + CloudClusterId.GetHashCode();
        }
        return hashcode;
      }

      public override string ToString()
      {
        var sb = new StringBuilder("resumeCloudCluster_args(");
        bool __first = true;
        if (CloudClusterId != null && __isset.cloudClusterId)
        {
          if(!__first) { sb.Append(", "); }
          __first = false;
          sb.Append("CloudClusterId: ");
          sb.Append(CloudClusterId);
        }
        sb.Append(")");
        return sb.ToString();
      }
    }


    public partial class resumeCloudClusterResult : TBase
    {
      private CloudCluster _success;
      private CloudException _cloudException;

      public CloudCluster Success
      {
        get
        {
          return _success;
        }
        set
        {
          __isset.success = true;
          this._success = value;
        }
      }

      public CloudException CloudException
      {
        get
        {
          return _cloudException;
        }
        set
        {
          __isset.cloudException = true;
          this._cloudException = value;
        }
      }


      public Isset __isset;
      public struct Isset
      {
        public bool success;
        public bool cloudException;
      }

      public resumeCloudClusterResult()
      {
      }

      public async Task ReadAsync(TProtocol iprot, CancellationToken cancellationToken)
      {
        iprot.IncrementRecursionDepth();
        try
        {
          TField field;
          await iprot.ReadStructBeginAsync(cancellationToken);
          while (true)
          {
            field = await iprot.ReadFieldBeginAsync(cancellationToken);
            if (field.Type == TType.Stop)
            {
              break;
            }

            switch (field.ID)
            {
              case 0:
                if (field.Type == TType.Struct)
                {
                  Success = new CloudCluster();
                  await Success.ReadAsync(iprot, cancellationToken);
                }
                else
                {
                  await TProtocolUtil.SkipAsync(iprot, field.Type, cancellationToken);
                }
                break;
              case 1:
                if (field.Type == TType.Struct)
                {
                  CloudException = new CloudException();
                  await CloudException.ReadAsync(iprot, cancellationToken);
                }
                else
                {
                  await TProtocolUtil.SkipAsync(iprot, field.Type, cancellationToken);
                }
                break;
              default: 
                await TProtocolUtil.SkipAsync(iprot, field.Type, cancellationToken);
                break;
            }

            await iprot.ReadFieldEndAsync(cancellationToken);
          }

          await iprot.ReadStructEndAsync(cancellationToken);
        }
        finally
        {
          iprot.DecrementRecursionDepth();
        }
      }

      public async Task WriteAsync(TProtocol oprot, CancellationToken cancellationToken)
      {
        oprot.IncrementRecursionDepth();
        try
        {
          var struc = new TStruct("resumeCloudCluster_result");
          await oprot.WriteStructBeginAsync(struc, cancellationToken);
          var field = new TField();

          if(this.__isset.success)
          {
            if (Success != null)
            {
              field.Name = "Success";
              field.Type = TType.Struct;
              field.ID = 0;
              await oprot.WriteFieldBeginAsync(field, cancellationToken);
              await Success.WriteAsync(oprot, cancellationToken);
              await oprot.WriteFieldEndAsync(cancellationToken);
            }
          }
          else if(this.__isset.cloudException)
          {
            if (CloudException != null)
            {
              field.Name = "CloudException";
              field.Type = TType.Struct;
              field.ID = 1;
              await oprot.WriteFieldBeginAsync(field, cancellationToken);
              await CloudException.WriteAsync(oprot, cancellationToken);
              await oprot.WriteFieldEndAsync(cancellationToken);
            }
          }
          await oprot.WriteFieldStopAsync(cancellationToken);
          await oprot.WriteStructEndAsync(cancellationToken);
        }
        finally
        {
          oprot.DecrementRecursionDepth();
        }
      }

      public override bool Equals(object that)
      {
        var other = that as resumeCloudClusterResult;
        if (other == null) return false;
        if (ReferenceEquals(this, other)) return true;
        return ((__isset.success == other.__isset.success) && ((!__isset.success) || (System.Object.Equals(Success, other.Success))))
          && ((__isset.cloudException == other.__isset.cloudException) && ((!__isset.cloudException) || (System.Object.Equals(CloudException, other.CloudException))));
      }

      public override int GetHashCode() {
        int hashcode = 157;
        unchecked {
          if(__isset.success)
            hashcode = (hashcode * 397) + Success.GetHashCode();
          if(__isset.cloudException)
            hashcode = (hashcode * 397) + CloudException.GetHashCode();
        }
        return hashcode;
      }

      public override string ToString()
      {
        var sb = new StringBuilder("resumeCloudCluster_result(");
        bool __first = true;
        if (Success != null && __isset.success)
        {
          if(!__first) { sb.Append(", "); }
          __first = false;
          sb.Append("Success: ");
          sb.Append(Success== null ? "<null>" : Success.ToString());
        }
        if (CloudException != null && __isset.cloudException)
        {
          if(!__first) { sb.Append(", "); }
          __first = false;
          sb.Append("CloudException: ");
          sb.Append(CloudException== null ? "<null>" : CloudException.ToString());
        }
        sb.Append(")");
        return sb.ToString();
      }
    }


    public partial class deleteCloudClusterArgs : TBase
    {
      private string _cloudClusterId;

      public string CloudClusterId
      {
        get
        {
          return _cloudClusterId;
        }
        set
        {
          __isset.cloudClusterId = true;
          this._cloudClusterId = value;
        }
      }


      public Isset __isset;
      public struct Isset
      {
        public bool cloudClusterId;
      }

      public deleteCloudClusterArgs()
      {
      }

      public async Task ReadAsync(TProtocol iprot, CancellationToken cancellationToken)
      {
        iprot.IncrementRecursionDepth();
        try
        {
          TField field;
          await iprot.ReadStructBeginAsync(cancellationToken);
          while (true)
          {
            field = await iprot.ReadFieldBeginAsync(cancellationToken);
            if (field.Type == TType.Stop)
            {
              break;
            }

            switch (field.ID)
            {
              case 1:
                if (field.Type == TType.String)
                {
                  CloudClusterId = await iprot.ReadStringAsync(cancellationToken);
                }
                else
                {
                  await TProtocolUtil.SkipAsync(iprot, field.Type, cancellationToken);
                }
                break;
              default: 
                await TProtocolUtil.SkipAsync(iprot, field.Type, cancellationToken);
                break;
            }

            await iprot.ReadFieldEndAsync(cancellationToken);
          }

          await iprot.ReadStructEndAsync(cancellationToken);
        }
        finally
        {
          iprot.DecrementRecursionDepth();
        }
      }

      public async Task WriteAsync(TProtocol oprot, CancellationToken cancellationToken)
      {
        oprot.IncrementRecursionDepth();
        try
        {
          var struc = new TStruct("deleteCloudCluster_args");
          await oprot.WriteStructBeginAsync(struc, cancellationToken);
          var field = new TField();
          if (CloudClusterId != null && __isset.cloudClusterId)
          {
            field.Name = "cloudClusterId";
            field.Type = TType.String;
            field.ID = 1;
            await oprot.WriteFieldBeginAsync(field, cancellationToken);
            await oprot.WriteStringAsync(CloudClusterId, cancellationToken);
            await oprot.WriteFieldEndAsync(cancellationToken);
          }
          await oprot.WriteFieldStopAsync(cancellationToken);
          await oprot.WriteStructEndAsync(cancellationToken);
        }
        finally
        {
          oprot.DecrementRecursionDepth();
        }
      }

      public override bool Equals(object that)
      {
        var other = that as deleteCloudClusterArgs;
        if (other == null) return false;
        if (ReferenceEquals(this, other)) return true;
        return ((__isset.cloudClusterId == other.__isset.cloudClusterId) && ((!__isset.cloudClusterId) || (System.Object.Equals(CloudClusterId, other.CloudClusterId))));
      }

      public override int GetHashCode() {
        int hashcode = 157;
        unchecked {
          if(__isset.cloudClusterId)
            hashcode = (hashcode * 397) + CloudClusterId.GetHashCode();
        }
        return hashcode;
      }

      public override string ToString()
      {
        var sb = new StringBuilder("deleteCloudCluster_args(");
        bool __first = true;
        if (CloudClusterId != null && __isset.cloudClusterId)
        {
          if(!__first) { sb.Append(", "); }
          __first = false;
          sb.Append("CloudClusterId: ");
          sb.Append(CloudClusterId);
        }
        sb.Append(")");
        return sb.ToString();
      }
    }


    public partial class deleteCloudClusterResult : TBase
    {
      private CloudException _cloudException;

      public CloudException CloudException
      {
        get
        {
          return _cloudException;
        }
        set
        {
          __isset.cloudException = true;
          this._cloudException = value;
        }
      }


      public Isset __isset;
      public struct Isset
      {
        public bool cloudException;
      }

      public deleteCloudClusterResult()
      {
      }

      public async Task ReadAsync(TProtocol iprot, CancellationToken cancellationToken)
      {
        iprot.IncrementRecursionDepth();
        try
        {
          TField field;
          await iprot.ReadStructBeginAsync(cancellationToken);
          while (true)
          {
            field = await iprot.ReadFieldBeginAsync(cancellationToken);
            if (field.Type == TType.Stop)
            {
              break;
            }

            switch (field.ID)
            {
              case 1:
                if (field.Type == TType.Struct)
                {
                  CloudException = new CloudException();
                  await CloudException.ReadAsync(iprot, cancellationToken);
                }
                else
                {
                  await TProtocolUtil.SkipAsync(iprot, field.Type, cancellationToken);
                }
                break;
              default: 
                await TProtocolUtil.SkipAsync(iprot, field.Type, cancellationToken);
                break;
            }

            await iprot.ReadFieldEndAsync(cancellationToken);
          }

          await iprot.ReadStructEndAsync(cancellationToken);
        }
        finally
        {
          iprot.DecrementRecursionDepth();
        }
      }

      public async Task WriteAsync(TProtocol oprot, CancellationToken cancellationToken)
      {
        oprot.IncrementRecursionDepth();
        try
        {
          var struc = new TStruct("deleteCloudCluster_result");
          await oprot.WriteStructBeginAsync(struc, cancellationToken);
          var field = new TField();

          if(this.__isset.cloudException)
          {
            if (CloudException != null)
            {
              field.Name = "CloudException";
              field.Type = TType.Struct;
              field.ID = 1;
              await oprot.WriteFieldBeginAsync(field, cancellationToken);
              await CloudException.WriteAsync(oprot, cancellationToken);
              await oprot.WriteFieldEndAsync(cancellationToken);
            }
          }
          await oprot.WriteFieldStopAsync(cancellationToken);
          await oprot.WriteStructEndAsync(cancellationToken);
        }
        finally
        {
          oprot.DecrementRecursionDepth();
        }
      }

      public override bool Equals(object that)
      {
        var other = that as deleteCloudClusterResult;
        if (other == null) return false;
        if (ReferenceEquals(this, other)) return true;
        return ((__isset.cloudException == other.__isset.cloudException) && ((!__isset.cloudException) || (System.Object.Equals(CloudException, other.CloudException))));
      }

      public override int GetHashCode() {
        int hashcode = 157;
        unchecked {
          if(__isset.cloudException)
            hashcode = (hashcode * 397) + CloudException.GetHashCode();
        }
        return hashcode;
      }

      public override string ToString()
      {
        var sb = new StringBuilder("deleteCloudCluster_result(");
        bool __first = true;
        if (CloudException != null && __isset.cloudException)
        {
          if(!__first) { sb.Append(", "); }
          __first = false;
          sb.Append("CloudException: ");
          sb.Append(CloudException== null ? "<null>" : CloudException.ToString());
        }
        sb.Append(")");
        return sb.ToString();
      }
    }


    public partial class executeOnControllerArgs : TBase
    {
      private string _clusterId;
      private string _script;
      private Lang _lang;

      public string ClusterId
      {
        get
        {
          return _clusterId;
        }
        set
        {
          __isset.clusterId = true;
          this._clusterId = value;
        }
      }

      public string Script
      {
        get
        {
          return _script;
        }
        set
        {
          __isset.script = true;
          this._script = value;
        }
      }

      /// <summary>
      /// 
      /// <seealso cref="Lang"/>
      /// </summary>
      public Lang Lang
      {
        get
        {
          return _lang;
        }
        set
        {
          __isset.lang = true;
          this._lang = value;
        }
      }


      public Isset __isset;
      public struct Isset
      {
        public bool clusterId;
        public bool script;
        public bool lang;
      }

      public executeOnControllerArgs()
      {
      }

      public async Task ReadAsync(TProtocol iprot, CancellationToken cancellationToken)
      {
        iprot.IncrementRecursionDepth();
        try
        {
          TField field;
          await iprot.ReadStructBeginAsync(cancellationToken);
          while (true)
          {
            field = await iprot.ReadFieldBeginAsync(cancellationToken);
            if (field.Type == TType.Stop)
            {
              break;
            }

            switch (field.ID)
            {
              case 1:
                if (field.Type == TType.String)
                {
                  ClusterId = await iprot.ReadStringAsync(cancellationToken);
                }
                else
                {
                  await TProtocolUtil.SkipAsync(iprot, field.Type, cancellationToken);
                }
                break;
              case 2:
                if (field.Type == TType.String)
                {
                  Script = await iprot.ReadStringAsync(cancellationToken);
                }
                else
                {
                  await TProtocolUtil.SkipAsync(iprot, field.Type, cancellationToken);
                }
                break;
              case 3:
                if (field.Type == TType.I32)
                {
                  Lang = (Lang)await iprot.ReadI32Async(cancellationToken);
                }
                else
                {
                  await TProtocolUtil.SkipAsync(iprot, field.Type, cancellationToken);
                }
                break;
              default: 
                await TProtocolUtil.SkipAsync(iprot, field.Type, cancellationToken);
                break;
            }

            await iprot.ReadFieldEndAsync(cancellationToken);
          }

          await iprot.ReadStructEndAsync(cancellationToken);
        }
        finally
        {
          iprot.DecrementRecursionDepth();
        }
      }

      public async Task WriteAsync(TProtocol oprot, CancellationToken cancellationToken)
      {
        oprot.IncrementRecursionDepth();
        try
        {
          var struc = new TStruct("executeOnController_args");
          await oprot.WriteStructBeginAsync(struc, cancellationToken);
          var field = new TField();
          if (ClusterId != null && __isset.clusterId)
          {
            field.Name = "clusterId";
            field.Type = TType.String;
            field.ID = 1;
            await oprot.WriteFieldBeginAsync(field, cancellationToken);
            await oprot.WriteStringAsync(ClusterId, cancellationToken);
            await oprot.WriteFieldEndAsync(cancellationToken);
          }
          if (Script != null && __isset.script)
          {
            field.Name = "script";
            field.Type = TType.String;
            field.ID = 2;
            await oprot.WriteFieldBeginAsync(field, cancellationToken);
            await oprot.WriteStringAsync(Script, cancellationToken);
            await oprot.WriteFieldEndAsync(cancellationToken);
          }
          if (__isset.lang)
          {
            field.Name = "lang";
            field.Type = TType.I32;
            field.ID = 3;
            await oprot.WriteFieldBeginAsync(field, cancellationToken);
            await oprot.WriteI32Async((int)Lang, cancellationToken);
            await oprot.WriteFieldEndAsync(cancellationToken);
          }
          await oprot.WriteFieldStopAsync(cancellationToken);
          await oprot.WriteStructEndAsync(cancellationToken);
        }
        finally
        {
          oprot.DecrementRecursionDepth();
        }
      }

      public override bool Equals(object that)
      {
        var other = that as executeOnControllerArgs;
        if (other == null) return false;
        if (ReferenceEquals(this, other)) return true;
        return ((__isset.clusterId == other.__isset.clusterId) && ((!__isset.clusterId) || (System.Object.Equals(ClusterId, other.ClusterId))))
          && ((__isset.script == other.__isset.script) && ((!__isset.script) || (System.Object.Equals(Script, other.Script))))
          && ((__isset.lang == other.__isset.lang) && ((!__isset.lang) || (System.Object.Equals(Lang, other.Lang))));
      }

      public override int GetHashCode() {
        int hashcode = 157;
        unchecked {
          if(__isset.clusterId)
            hashcode = (hashcode * 397) + ClusterId.GetHashCode();
          if(__isset.script)
            hashcode = (hashcode * 397) + Script.GetHashCode();
            if(__isset.lang)
            if(__isset.lang)
            {
          if(__isset.lang)
            {
            hashcode = (hashcode * 397) + Lang.GetHashCode();
        }
        return hashcode;
      }

      public override string ToString()
      {
        var sb = new StringBuilder("executeOnController_args(");
        bool __first = true;
        if (ClusterId != null && __isset.clusterId)
        {
          if(!__first) { sb.Append(", "); }
          __first = false;
          sb.Append("ClusterId: ");
          sb.Append(ClusterId);
        }
        if (Script != null && __isset.script)
        {
          if(!__first) { sb.Append(", "); }
          __first = false;
          sb.Append("Script: ");
          sb.Append(Script);
        }
        if (__isset.lang)
        {
          if(!__first) { sb.Append(", "); }
          __first = false;
          sb.Append("Lang: ");
          sb.Append(Lang);
        }
        sb.Append(")");
        return sb.ToString();
      }
    }


    public partial class executeOnControllerResult : TBase
    {
      private Response _success;

      public Response Success
      {
        get
        {
          return _success;
        }
        set
        {
          __isset.success = true;
          this._success = value;
        }
      }


      public Isset __isset;
      public struct Isset
      {
        public bool success;
      }

      public executeOnControllerResult()
      {
      }

      public async Task ReadAsync(TProtocol iprot, CancellationToken cancellationToken)
      {
        iprot.IncrementRecursionDepth();
        try
        {
          TField field;
          await iprot.ReadStructBeginAsync(cancellationToken);
          while (true)
          {
            field = await iprot.ReadFieldBeginAsync(cancellationToken);
            if (field.Type == TType.Stop)
            {
              break;
            }

            switch (field.ID)
            {
              case 0:
                if (field.Type == TType.Struct)
                {
                  Success = new Response();
                  await Success.ReadAsync(iprot, cancellationToken);
                }
                else
                {
                  await TProtocolUtil.SkipAsync(iprot, field.Type, cancellationToken);
                }
                break;
              default: 
                await TProtocolUtil.SkipAsync(iprot, field.Type, cancellationToken);
                break;
            }

            await iprot.ReadFieldEndAsync(cancellationToken);
          }

          await iprot.ReadStructEndAsync(cancellationToken);
        }
        finally
        {
          iprot.DecrementRecursionDepth();
        }
      }

      public async Task WriteAsync(TProtocol oprot, CancellationToken cancellationToken)
      {
        oprot.IncrementRecursionDepth();
        try
        {
          var struc = new TStruct("executeOnController_result");
          await oprot.WriteStructBeginAsync(struc, cancellationToken);
          var field = new TField();

          if(this.__isset.success)
          {
            if (Success != null)
            {
              field.Name = "Success";
              field.Type = TType.Struct;
              field.ID = 0;
              await oprot.WriteFieldBeginAsync(field, cancellationToken);
              await Success.WriteAsync(oprot, cancellationToken);
              await oprot.WriteFieldEndAsync(cancellationToken);
            }
          }
          await oprot.WriteFieldStopAsync(cancellationToken);
          await oprot.WriteStructEndAsync(cancellationToken);
        }
        finally
        {
          oprot.DecrementRecursionDepth();
        }
      }

      public override bool Equals(object that)
      {
        var other = that as executeOnControllerResult;
        if (other == null) return false;
        if (ReferenceEquals(this, other)) return true;
        return ((__isset.success == other.__isset.success) && ((!__isset.success) || (System.Object.Equals(Success, other.Success))));
      }

      public override int GetHashCode() {
        int hashcode = 157;
        unchecked {
          if(__isset.success)
            hashcode = (hashcode * 397) + Success.GetHashCode();
        }
        return hashcode;
      }

      public override string ToString()
      {
        var sb = new StringBuilder("executeOnController_result(");
        bool __first = true;
        if (Success != null && __isset.success)
        {
          if(!__first) { sb.Append(", "); }
          __first = false;
          sb.Append("Success: ");
          sb.Append(Success== null ? "<null>" : Success.ToString());
        }
        sb.Append(")");
        return sb.ToString();
      }
    }

  }
}
