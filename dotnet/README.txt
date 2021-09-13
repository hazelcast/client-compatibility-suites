
HAZELCAST CSHARP CLIENT COMPATIBILITY TESTS

Jenkins project csharp-client-compatibility
  tests one version of the client against servers
  uses test-client.ps1 (which in turns uses test-client-server.ps1)
  versions of the server are defined in test-client.ps1
  
Jenkins project client-compatibility-tests-csharp
  tests one version of the server against clients
  uses test-server.ps1 (which in turns uses test-client-server.ps1)
  versions of the client are defined in test-server.ps1
  
Client libraries are in dotnet/3.x/
Server libraries are in hazelcast-archive/
RC libraries are in hazelcast/

test-client-server.ps1
  uses NuGet to fetch NUnit (via test.proj)
  stores NuGet in temp/
  produces nunit-result-*ml files in temp/
  and the RC logs in temp/ too
  
  uses NUnit 3
  
notes:
  we can probably remove all dotnet/*.bat and dotnet/*.sh files