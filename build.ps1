param($version = "0.1.0")

task Default -depends Test, NuGet

properties {
	$rootDir            = Split-Path $psake.build_script_file
	$sourceDir          = "$rootDir\src"
	$toolsDir           = "$rootDir\tools"
	$buildDir           = "$rootDir\build"
	$testReportsDir     = "$buildDir\test-reports"
	$testDir            = "$buildDir\Tests"
	$artifactsDir       = "$buildDir\Artifacts"
	$exclusions         = @("*.pdb", "*.xml")
	$buildVersion			  = (getFullVersion)
	$assemblyInfoFile		= "$sourceDir\GurkBurk\Properties\AssemblyInfo.cs"
}

task Clean {
	if ($true -eq (Test-Path "$buildDir")) {
		Get-ChildItem $buildDir\**\*.* -Recurse | ForEach-Object { Remove-Item $_.FullName }
		Remove-Item $buildDir -Recurse
	}
	New-Item $buildDir -type directory
	New-Item $testReportsDir -type directory
	New-Item $artifactsDir -type directory
}

function getFullVersion() {
	$nt = 0
	$fullVersion = "$version.0"
	$file = "$rootDir\version.txt"
	if (Test-Path "$file") {
		$v = "0" + (Get-Content -path "$file")
		$nt = 1 + [System.Int32]::Parse($v)
		$fullVersion = "$version.$nt"
	}
	Set-Content -path "$file" $nt.ToString()
	return $fullVersion
}

task Version {
	Copy-Item $assemblyInfoFile "$assemblyInfoFile.bak"
	$src = Get-Content $assemblyInfoFile
	$newSrc = foreach($row in $src) {
		if ($row -match 'Assembly((Version)|(FileVersion))\s*\(\s*"\d+\.\d+\.\d+\.\d+"\s*\)') {
			$row -replace "`".*`"", ("`"$buildVersion`"")
		}
		elseif ($row -match 'AssemblyInformationalVersion') {
			$row -replace "`".*`"", ("`"$version`"")
		}
		else { $row }
	}
	Set-Content -path $assemblyInfoFile -value $newSrc
}

task Init -depends Clean, Version

task Compile -depends Init {
	Exec { msbuild "$sourceDir\GurkBurk.sln" /p:Configuration=Automated-3.5 /v:m /m /p:TargetFrameworkVersion=v3.5 /toolsversion:4.0 /t:Rebuild }
	Exec { msbuild "$sourceDir\GurkBurk.sln" /p:Configuration=Automated-4.0 /v:m /m /p:TargetFrameworkVersion=v4.0 /toolsversion:4.0 /t:Rebuild }
}

task Test -depends Compile {
	new-item $testReportsDir -type directory -ErrorAction SilentlyContinue

	$arguments = Get-Item "$testDir\3.5\*Spec*.dll"
	Exec { .\src\packages\nunit.2.5.10.11092\tools\nunit-console.exe $arguments /xml:$testReportsDir\UnitTests.xml}
	Copy-Item "$assemblyInfoFile.bak" $assemblyInfoFile
	Remove-Item "$assemblyInfoFile.bak"
}

task NuGet -depends Compile {
	Exec { .\src\.nuget\nuget.exe pack "$rootDir\GurkBurk.nuspec"  -Version $version -OutputDirectory $artifactsDir}
}

task TagGithub {
	#$x = git ls-remote "https://github.com/MorganPersson/GurkBurk.git"
	#$x = git ls-remote "."
	#$sha = $x[0].SubString(0,40)
	#write-host "git sha1: $sha"

	#authenticate first
	#$ curl -u "username" https://api.github.com

	#$urlAuth = "https://api.github.com/"
	#$dataAuth = new-object Collections.Specialized.NameValueCollection
	#$url = "https://api.github.com/repos/MorganPersson/GurkBurk/statuses/$sha"
  #$wb = new-object Net.WebClient
	#$data = new-object Collections.Specialized.NameValueCollection
  #$data["status"] = (Get-Content ".\currentBuildStatus.txt")
  #$data["target_rul"] = "url_to_ci_server_build"
  #$data["description"] = "description"
	#$response = wb.UploadValues($url, "POST", data)
}