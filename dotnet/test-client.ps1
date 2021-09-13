Param(
  [string]$clientVersion = $(throw "-clientVersion is required."),
  [string]$type = ""
)

# this is the main script controlling the compatility tests of
# ONE VERSION of the client against SEVERAL VERSIONS of the server
#
# this should be the latest minor of each major or the server:
#
$serverVersions = @("3.6.8","3.7.8","3.8.9","3.9.4","3.10.7","3.11.7", "3.12.10")

# download enterprise jars from https://repository.hazelcast.com/release/com/hazelcast/hazelcast-enterprise/
# download oss jars from https://search.maven.org/artifact/com.hazelcast/hazelcast

# prepare directories
$testTemp = "$PSScriptRoot\temp"
if (-not (test-path $testTemp)) {
  mkdir $testTemp >$null 
} else {
  rm $testTemp/nunit-*
}

# run
foreach ($serverVersion in $serverVersions) {
	Write-Host "Start test for serverVersion: " $serverVersion
	& .\test-client-server.ps1 -clientVersion $clientVersion -serverVersion $serverVersion -testServer -type $type
}
