Param(
  [string]$serverVersion = $(throw "-serverVersion is required."),
  [string]$type = ""
)

# this is the main script controlling the compatility tests of
# SEVERAL VERSIONS of the client against ONE VERSION of the server
#
# this should be the latest minor of each major or the client:
#
$clientVersions = @("3.6.4","3.7.1","3.8.2","3.9.4","3.10","3.11","3.12.2")

# prepare directories
$testTemp = "$PSScriptRoot\temp"
if (-not (test-path $testTemp)) {
  mkdir $testTemp >$null 
} else {
  rm $testTemp/nunit-*
}

# run
foreach ($clientVersion in $clientVersions) {
	Write-Host "Start test for client version:" $clientVersion
	& .\test-client-server.ps1 -clientVersion $clientVersion -serverVersion $serverVersion -testServer -type $type
}
