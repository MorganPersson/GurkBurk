param(
	$version = "0.1.0.0", 
	$task = "default", 
	$buildFile = ".\build.ps1"
)

Write-Host "buildFile $buildFile"
Write-Host "task $task"
Write-Host "version $version"

function Build($taskToRun) {
	invoke-psake $buildFile -framework '4.0x86' -t $taskToRun -parameters @{"version"="$version"}
	if ($LastExitCode -ne $null) {
		if ($LastExitCode -ne 0) { 
			$msg = "build exited with errorcode $LastExitCode"
			throw $msg 
		}
	}
}

$scriptPath = (Get-Location).Path
remove-module psake -ea 'SilentlyContinue'
Import-Module (join-path $scriptPath ".\tools\psake\psake.psm1")

if (-not(test-path $buildFile)) {
    $buildFile = (join-path $scriptPath $buildFile)
} 

Build $task