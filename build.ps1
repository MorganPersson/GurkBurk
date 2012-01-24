param($version = "0.1.0.0")

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

task Version {
	$asmInfo = "$sourceDir\GurkBurk\Properties\AssemblyInfo.cs"
	$src = Get-Content $asmInfo
	$newSrc = foreach($row in $src) { 
		if ($row -match 'Assembly((Version)|(FileVersion))\s*\(\s*"\d+\.\d+\.\d+\.\d+"\s*\)') { 
			$row -replace "\d+\.\d+\.\d+\.\d+", $version 
		}
		else { $row }
	}
	Set-Content -path $asmInfo -value $newSrc			
}

task Init -depends Clean, Version

task Compile -depends Init {
	Exec { msbuild "$sourceDir\GurkBurk.sln" /p:Configuration=Automated-3.5 /v:m /p:TargetFrameworkVersion=v3.5 /toolsversion:4.0 /t:Rebuild }
	Exec { msbuild "$sourceDir\GurkBurk.sln" /p:Configuration=Automated-4.0 /v:m /p:TargetFrameworkVersion=v4.0 /toolsversion:4.0 /t:Rebuild }
}

task Test -depends Compile {
	new-item $testReportsDir -type directory -ErrorAction SilentlyContinue
	
	$arguments = Get-Item "$testDir\3.5\*Spec*.dll"
	Exec { .\src\packages\nunit.2.5.10.11092\tools\nunit-console.exe $arguments /xml:$testReportsDir\UnitTests.xml}
}

task NuGet -depends Compile {
	Exec { .\src\.nuget\nuget.exe pack "$rootDir\GurkBurk.nuspec"  -Version $version -OutputDirectory $artifactsDir}
}
