Param(
  [string]$clientVersion = $(throw "-clientVersion is required."), # the client version to test
  [string]$serverVersion = $(throw "-serverVersion is required."), # the server version to test
  [switch]$testServer = $false, # whether to test a (candidate) server
  [string]$classpath = "",
  [string]$type = ""
)

# this is the sub script controlling the compatility tests of
# ONE VERSION of the client against ONE VERSION of the server

# configure
$options = @{
	Local = $false;
	Cache = 12 #days
}

# https://stackoverflow.com/questions/18862716
[Environment]::CurrentDirectory = get-location

# detect & validate enterprise
if ($type -eq "enterprise") {
  $enterprise = $true
}
elseif ($type -eq "oss") {
  if ($env:HZ_TYPE -eq "enterprise") {
    throw "Conflicting type specification."
  }
}
elseif ($type -eq "") {
  $enterprise = ($env:HZ_TYPE -eq "enterprise")
}
else {
  throw "Not a supported type."
}

if ($enterprise) {
  $enterprise_key = $env:HAZELCAST_ENTERPRISE_KEY
  if (-not $enterprise_key) {
    throw "Cannot run enterprise tests without a key."
  }
}

# get server category = major + minor
$catArr = $serverVersion.split(".")
$serverCategory = $catArr[0] + "." + $catArr[1]

# get client minor version number (eg 312 for 3.12)
$clientVersionArr = $clientVersion.split(".")
$clientVersionMinor = [int]$clientVersionArr[0] *100 + [int]$clientVersionArr[1]

# get rc version
$clientVersionArr = $clientVersion.split(".")
if($clientVersionMinor -ge 310)
{
    $rc = "0.5";
}
else
{
    $rc = "0.3";
}

if ($testServer) {
  $jarPathOS = "hazelcast"
  $jarPathEE = "hazelcast-enterprise"
}
else {
  $jarPathOS = "hazelcast-archive"
  $jarPathEE = "hazelcast-archive"
}

# setup classpath
[string]$hzjars = `
  "..\${jarPathOS}\hazelcast-${serverVersion}-tests.jar;" + `
  "..\hazelcast\hazelcast-remote-controller-${rc}-SNAPSHOT.jar"
  
if ($enterprise) {
	$classpath = `
    "..\${jarPathEE}\hazelcast-enterprise-${serverVersion}.jar;" + `
    "..\${jarPathEE}\hazelcast-enterprise-${serverVersion}-tests.jar;" + `
    "${hzjars}"
} else {
	$classpath = `
    "..\${jarPathOS}\hazelcast-${serverVersion}.jar;" + `
    "${hzjars}"
}

# create a filter to exclude tests for more recent servers
[string]$testCategory = ""
[int]$currentVersion = ([int]$catArr[1]) + 1
[int]$endVersion = 20
while($currentVersion -lt $endVersion) {
	if($testCategory.Length -gt 0) {
		$testCategory += " and "
	}
	$testCategory += "cat != 3.${currentVersion}"
	$currentVersion += 1
}

if(!($enterprise)) {
	$testCategory += " and cat != enterprise"
}

# these tests are generally unstable and should be fixed
$testCategory += " and test != Hazelcast.Client.Test.ClientSemaphoreTest"
$testCategory += " and test != Hazelcast.Client.Test.ClientReconnectionTest"

# the hazelcast test jars contain SSL certs that can expire
# ignore these tests, they fail due to SSL certs being expired
if ($clientVersionMinor -eq 310 -or $clientVersionMinor -eq 311)
{
  $testCategory += " and test != Hazelcast.Client.Test.ClientSSLTest.TestSSLEnabled_validateChain_validateName_validName"
  $testCategory += " and test != Hazelcast.Client.Test.ClientSSLTest.TestSSLEnabled_validateName_validName"
}

$whereParam="`"${testCategory}`""

Write-Host "-----------------------------"
Write-Host "Client version: $clientVersion"
Write-Host "Server version: $serverVersion cat: $serverCategory"
Write-Host "Is Enterprise:  $Enterprise"
Write-Host "Classpath:      $classpath"
Write-Host "Where:          $whereParam"
Write-Host "-----------------------------"

# prepare directories
$testTemp = "$PSScriptRoot\temp"
if (-not (test-path $testTemp)) { mkdir $testTemp >$null }

# ensure we have NuGet
$nuget = "$testTemp\nuget.exe"
if (-not $options.Local)
{
	$source = "https://dist.nuget.org/win-x86-commandline/latest/nuget.exe"
	if ((test-path $nuget) -and ((ls $nuget).CreationTime -lt [DateTime]::Now.AddDays(-$options.Cache)))
	{
		Remove-Item $nuget -force -errorAction SilentlyContinue > $null
	}
	if (-not (test-path $nuget))
	{
		Write-Host "Download NuGet..."
		Invoke-WebRequest $source -OutFile $nuget
		if (-not $?) { throw "Failed to download NuGet." }
		Write-Host "  -> $nuget"
	}
	else {
		Write-Host "Detected NuGet."
	}
}
elseif (-not (test-path $nuget))
{
	throw "Failed to locate NuGet.exe."
}

# ensure we have the required NuGet packages
&$nuget restore test.proj

# find nunit
$v = ls "$($env:USERPROFILE)\.nuget\packages\nunit.consolerunner\*" | `
     foreach-object { [Version]::Parse($_.Name) } | `
     sort -descending | `
     select -first 1
$nunit = "$($env:USERPROFILE)\.nuget\packages\nunit.consolerunner\$v\tools\nunit3-console.exe"

# validate classpath
$classpaths = $classpath.Split(';')
foreach ($path in $classpaths) {
  $fullpath = [System.IO.Path]::GetFullPath($path)
  if (-not (test-path $fullpath)) {
    throw "Classpath error: $fullpath"
  }
}

Write-Host "Starting hazelcast-remote-controller..."

$remoteControllerApp = Start-Process -FilePath java -ArgumentList ( "-Dhazelcast.enterprise.license.key=$enterprise_key","-cp", "$classpath", "com.hazelcast.remotecontroller.Main" ) -RedirectStandardOutput "temp/rc_stdout${serverVersion}.log" -RedirectStandardError "temp/rc_stderr${serverVersion}.log" -PassThru

# tell tests which server they are talking to
$env:HAZELCAST_SERVER_VERSION=$serverVersion

# tell tests to wait - test env is too slow (120 seconds)
$env:HAZELCAST_TEST_EVENTUALLY_TIMEOUT = 120

Write-Host "Wait for Hazelcast to start ..."
Start-Sleep -s 5

$testDLL=".\${clientVersion}\Hazelcast.Test.dll"

Write-Host "Running Unit Tests for client $clientVersion - server $serverVersion - rc $rc"
Write-Host "============================================================================="

$nunitArgs = @("$testDLL", "--where", $whereParam, "--labels=All", "--result=temp/nunit-result-c${clientVersion}-s${serverVersion}.xml", "--framework=v4.0")

Write-Host $nunit $nunitArgs
& $nunit $nunitArgs

Stop-Process -Force -Id $remoteControllerApp.Id

Write-Host "Unit test run completed for client $clientVersion - server $serverVersion - rc $rc"
Write-Host "=============================================================================="

exit $LASTEXITCODE
