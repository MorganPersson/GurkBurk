param(
	$version = "0.1.0",
	$task = "default",
	$buildFile = ".\build.ps1"
)

Write-Host "buildFile $buildFile"
Write-Host "task $task"
Write-Host "version $version"

function Build($taskToRun) {
	Set-Content -path "currentBuildStatus.txt" "yellow"
	invoke-psake $buildFile -framework '4.0x86' -t $taskToRun -parameters @{"version"="$version"}
	if ($psake.build_success) {
		Set-Content -path "currentBuildStatus.txt" "passed"
	}
	else {
		Set-Content -path "currentBuildStatus.txt" "failure"
		$errMsg = $psake.build_error_message
		$replace = @{ "\|" = "||"; "`n" = "|n"; "`r" = "|r"; "'" = "|'"; "\[" = "|["; "\]"  = "|]"; '0x0085' = "|x"; '0x2028' = "|l"; '0x2029' = "|p" }
		$replace.GetEnumerator() | ForEach-Object {  $errMsg = $errMsg -replace $_.Key, $_.Value }
		write-host "##teamcity[message text='build failed: $errMsg' errorDetails='$errMsg' status='ERROR']"
		throw $errMsg
	}
}

$scriptPath = (Get-Location).Path
remove-module psake -ea 'SilentlyContinue'
Import-Module (join-path $scriptPath ".\tools\psake\psake.psm1")

if (-not(test-path $buildFile)) {
    $buildFile = (join-path $scriptPath $buildFile)
}

Build $task