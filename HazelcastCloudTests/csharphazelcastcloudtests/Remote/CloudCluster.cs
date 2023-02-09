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


namespace Hazelcast.Testing.Remote
{

  public partial class CloudCluster : TBase
  {
    private string _id;
    private string _name;
    private string _releaseName;
    private string _hazelcastVersion;
    private bool _isTlsEnabled;
    private string _state;
    private string _token;
    private string _certificatePath;
    private string _tlsPassword;

    public string Id
    {
      get
      {
        return _id;
      }
      set
      {
        __isset.id = true;
        this._id = value;
      }
    }

    public string Name
    {
      get
      {
        return _name;
      }
      set
      {
        __isset.name = true;
        this._name = value;
      }
    }

    public string ReleaseName
    {
      get
      {
        return _releaseName;
      }
      set
      {
        __isset.releaseName = true;
        this._releaseName = value;
      }
    }

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

    public string State
    {
      get
      {
        return _state;
      }
      set
      {
        __isset.state = true;
        this._state = value;
      }
    }

    public string Token
    {
      get
      {
        return _token;
      }
      set
      {
        __isset.token = true;
        this._token = value;
      }
    }

    public string CertificatePath
    {
      get
      {
        return _certificatePath;
      }
      set
      {
        __isset.certificatePath = true;
        this._certificatePath = value;
      }
    }

    public string TlsPassword
    {
      get
      {
        return _tlsPassword;
      }
      set
      {
        __isset.tlsPassword = true;
        this._tlsPassword = value;
      }
    }


    public Isset __isset;
    public struct Isset
    {
      public bool id;
      public bool name;
      public bool releaseName;
      public bool hazelcastVersion;
      public bool isTlsEnabled;
      public bool state;
      public bool token;
      public bool certificatePath;
      public bool tlsPassword;
    }

    public CloudCluster()
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
                Id = await iprot.ReadStringAsync(cancellationToken);
              }
              else
              {
                await TProtocolUtil.SkipAsync(iprot, field.Type, cancellationToken);
              }
              break;
            case 2:
              if (field.Type == TType.String)
              {
                Name = await iprot.ReadStringAsync(cancellationToken);
              }
              else
              {
                await TProtocolUtil.SkipAsync(iprot, field.Type, cancellationToken);
              }
              break;
            case 3:
              if (field.Type == TType.String)
              {
                ReleaseName = await iprot.ReadStringAsync(cancellationToken);
              }
              else
              {
                await TProtocolUtil.SkipAsync(iprot, field.Type, cancellationToken);
              }
              break;
            case 4:
              if (field.Type == TType.String)
              {
                HazelcastVersion = await iprot.ReadStringAsync(cancellationToken);
              }
              else
              {
                await TProtocolUtil.SkipAsync(iprot, field.Type, cancellationToken);
              }
              break;
            case 5:
              if (field.Type == TType.Bool)
              {
                IsTlsEnabled = await iprot.ReadBoolAsync(cancellationToken);
              }
              else
              {
                await TProtocolUtil.SkipAsync(iprot, field.Type, cancellationToken);
              }
              break;
            case 6:
              if (field.Type == TType.String)
              {
                State = await iprot.ReadStringAsync(cancellationToken);
              }
              else
              {
                await TProtocolUtil.SkipAsync(iprot, field.Type, cancellationToken);
              }
              break;
            case 7:
              if (field.Type == TType.String)
              {
                Token = await iprot.ReadStringAsync(cancellationToken);
              }
              else
              {
                await TProtocolUtil.SkipAsync(iprot, field.Type, cancellationToken);
              }
              break;
            case 8:
              if (field.Type == TType.String)
              {
                CertificatePath = await iprot.ReadStringAsync(cancellationToken);
              }
              else
              {
                await TProtocolUtil.SkipAsync(iprot, field.Type, cancellationToken);
              }
              break;
            case 9:
              if (field.Type == TType.String)
              {
                TlsPassword = await iprot.ReadStringAsync(cancellationToken);
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
        var struc = new TStruct("CloudCluster");
        await oprot.WriteStructBeginAsync(struc, cancellationToken);
        var field = new TField();
        if (Id != null && __isset.id)
        {
          field.Name = "id";
          field.Type = TType.String;
          field.ID = 1;
          await oprot.WriteFieldBeginAsync(field, cancellationToken);
          await oprot.WriteStringAsync(Id, cancellationToken);
          await oprot.WriteFieldEndAsync(cancellationToken);
        }
        if (Name != null && __isset.name)
        {
          field.Name = "name";
          field.Type = TType.String;
          field.ID = 2;
          await oprot.WriteFieldBeginAsync(field, cancellationToken);
          await oprot.WriteStringAsync(Name, cancellationToken);
          await oprot.WriteFieldEndAsync(cancellationToken);
        }
        if (ReleaseName != null && __isset.releaseName)
        {
          field.Name = "releaseName";
          field.Type = TType.String;
          field.ID = 3;
          await oprot.WriteFieldBeginAsync(field, cancellationToken);
          await oprot.WriteStringAsync(ReleaseName, cancellationToken);
          await oprot.WriteFieldEndAsync(cancellationToken);
        }
        if (HazelcastVersion != null && __isset.hazelcastVersion)
        {
          field.Name = "hazelcastVersion";
          field.Type = TType.String;
          field.ID = 4;
          await oprot.WriteFieldBeginAsync(field, cancellationToken);
          await oprot.WriteStringAsync(HazelcastVersion, cancellationToken);
          await oprot.WriteFieldEndAsync(cancellationToken);
        }
        if (__isset.isTlsEnabled)
        {
          field.Name = "isTlsEnabled";
          field.Type = TType.Bool;
          field.ID = 5;
          await oprot.WriteFieldBeginAsync(field, cancellationToken);
          await oprot.WriteBoolAsync(IsTlsEnabled, cancellationToken);
          await oprot.WriteFieldEndAsync(cancellationToken);
        }
        if (State != null && __isset.state)
        {
          field.Name = "state";
          field.Type = TType.String;
          field.ID = 6;
          await oprot.WriteFieldBeginAsync(field, cancellationToken);
          await oprot.WriteStringAsync(State, cancellationToken);
          await oprot.WriteFieldEndAsync(cancellationToken);
        }
        if (Token != null && __isset.token)
        {
          field.Name = "token";
          field.Type = TType.String;
          field.ID = 7;
          await oprot.WriteFieldBeginAsync(field, cancellationToken);
          await oprot.WriteStringAsync(Token, cancellationToken);
          await oprot.WriteFieldEndAsync(cancellationToken);
        }
        if (CertificatePath != null && __isset.certificatePath)
        {
          field.Name = "certificatePath";
          field.Type = TType.String;
          field.ID = 8;
          await oprot.WriteFieldBeginAsync(field, cancellationToken);
          await oprot.WriteStringAsync(CertificatePath, cancellationToken);
          await oprot.WriteFieldEndAsync(cancellationToken);
        }
        if (TlsPassword != null && __isset.tlsPassword)
        {
          field.Name = "tlsPassword";
          field.Type = TType.String;
          field.ID = 9;
          await oprot.WriteFieldBeginAsync(field, cancellationToken);
          await oprot.WriteStringAsync(TlsPassword, cancellationToken);
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
      var other = that as CloudCluster;
      if (other == null) return false;
      if (ReferenceEquals(this, other)) return true;
      return ((__isset.id == other.__isset.id) && ((!__isset.id) || (System.Object.Equals(Id, other.Id))))
        && ((__isset.name == other.__isset.name) && ((!__isset.name) || (System.Object.Equals(Name, other.Name))))
        && ((__isset.releaseName == other.__isset.releaseName) && ((!__isset.releaseName) || (System.Object.Equals(ReleaseName, other.ReleaseName))))
        && ((__isset.hazelcastVersion == other.__isset.hazelcastVersion) && ((!__isset.hazelcastVersion) || (System.Object.Equals(HazelcastVersion, other.HazelcastVersion))))
        && ((__isset.isTlsEnabled == other.__isset.isTlsEnabled) && ((!__isset.isTlsEnabled) || (System.Object.Equals(IsTlsEnabled, other.IsTlsEnabled))))
        && ((__isset.state == other.__isset.state) && ((!__isset.state) || (System.Object.Equals(State, other.State))))
        && ((__isset.token == other.__isset.token) && ((!__isset.token) || (System.Object.Equals(Token, other.Token))))
        && ((__isset.certificatePath == other.__isset.certificatePath) && ((!__isset.certificatePath) || (System.Object.Equals(CertificatePath, other.CertificatePath))))
        && ((__isset.tlsPassword == other.__isset.tlsPassword) && ((!__isset.tlsPassword) || (System.Object.Equals(TlsPassword, other.TlsPassword))));
    }

    public override int GetHashCode() {
      int hashcode = 157;
      unchecked {
        if(__isset.id)
          hashcode = (hashcode * 397) + Id.GetHashCode();
        if(__isset.name)
          hashcode = (hashcode * 397) + Name.GetHashCode();
        if(__isset.releaseName)
          hashcode = (hashcode * 397) + ReleaseName.GetHashCode();
        if(__isset.hazelcastVersion)
          hashcode = (hashcode * 397) + HazelcastVersion.GetHashCode();
        if(__isset.isTlsEnabled)
          hashcode = (hashcode * 397) + IsTlsEnabled.GetHashCode();
        if(__isset.state)
          hashcode = (hashcode * 397) + State.GetHashCode();
        if(__isset.token)
          hashcode = (hashcode * 397) + Token.GetHashCode();
        if(__isset.certificatePath)
          hashcode = (hashcode * 397) + CertificatePath.GetHashCode();
        if(__isset.tlsPassword)
          hashcode = (hashcode * 397) + TlsPassword.GetHashCode();
      }
      return hashcode;
    }

    public override string ToString()
    {
      var sb = new StringBuilder("CloudCluster(");
      bool __first = true;
      if (Id != null && __isset.id)
      {
        if(!__first) { sb.Append(", "); }
        __first = false;
        sb.Append("Id: ");
        sb.Append(Id);
      }
      if (Name != null && __isset.name)
      {
        if(!__first) { sb.Append(", "); }
        __first = false;
        sb.Append("Name: ");
        sb.Append(Name);
      }
      if (ReleaseName != null && __isset.releaseName)
      {
        if(!__first) { sb.Append(", "); }
        __first = false;
        sb.Append("ReleaseName: ");
        sb.Append(ReleaseName);
      }
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
      if (State != null && __isset.state)
      {
        if(!__first) { sb.Append(", "); }
        __first = false;
        sb.Append("State: ");
        sb.Append(State);
      }
      if (Token != null && __isset.token)
      {
        if(!__first) { sb.Append(", "); }
        __first = false;
        sb.Append("Token: ");
        sb.Append(Token);
      }
      if (CertificatePath != null && __isset.certificatePath)
      {
        if(!__first) { sb.Append(", "); }
        __first = false;
        sb.Append("CertificatePath: ");
        sb.Append(CertificatePath);
      }
      if (TlsPassword != null && __isset.tlsPassword)
      {
        if(!__first) { sb.Append(", "); }
        __first = false;
        sb.Append("TlsPassword: ");
        sb.Append(TlsPassword);
      }
      sb.Append(")");
      return sb.ToString();
    }
  }

}
