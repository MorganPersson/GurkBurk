// include Fake libs
#I "./packages/FAKE/tools"
#r "FakeLib.dll"

open Fake
open System
open System.IO


// params from teamcity
let buildNumber = getBuildParamOrDefault "buildNumber" "0"
let buildTag = getBuildParamOrDefault "buildTag" "devlocal" // For release, set this to "release"
let frameworkVersions = ["4.6"]

let version = "2.0.0"
let assemblyVersion = version + "." + buildNumber
let assemblyInfoVersion =
  match buildTag.ToLower() with
  | "release"  -> version + "." + buildNumber
  | _          -> version + "-" + buildTag + buildNumber.PadLeft(4, '0')
let nugetVersionNumber = assemblyInfoVersion

let rootDir = "./" |> FullName
let sourceDir = (rootDir + "/src") |> FullName
let buildDir = (rootDir + "/build") |> FullName
let testReportsDir = (buildDir + "/test-reports") |> FullName
let artifactsDir = (buildDir + "/artifacts") |> FullName
let packagesDir = (rootDir + "/packages") |> FullName
let nugetExe = (packagesDir + "/NuGet.CommandLine/tools/nuget.exe") |> FullName
let nugetAccessKey = getBuildParamOrDefault "nugetAccessKey" "NotSet"

let appReferences = !! (sourceDir + "/**/GurkBurk.csproj")
let testReferences = !! (sourceDir + "/**/*Spec.csproj")


// --------------------------------------------------------------------------------------
// Helpers
// --------------------------------------------------------------------------------------
let compileAnyCpu frameworkVer proj outputPathPrefix =
  build (fun f ->
          { f with
              MaxCpuCount = Some (Some Environment.ProcessorCount)
              ToolsVersion = Some "14.0"
              Verbosity = Some MSBuildVerbosity.Minimal
              Properties =  [ ("Configuration", "Debug");
                              ("TargetFrameworkVersion", "v" + frameworkVer)
                              ("OutputPath", Path.Combine(buildDir, outputPathPrefix + frameworkVer))
                            ]
              Targets = ["Rebuild"]
          }) proj

// --------------------------------------------------------------------------------------
// Targets
// --------------------------------------------------------------------------------------

Target "Clean" (fun _ ->
  killMSBuild()
  CleanDirs [buildDir]
  CleanDirs [testReportsDir; artifactsDir]
)

Target "Set teamcity buildnumber" (fun _ ->
  SetBuildNumber nugetVersionNumber
)

Target "AssemblyInfo" (fun _ ->
    appReferences
    |> Seq.iter (fun p ->
        let dir = System.IO.Path.GetDirectoryName p
        printfn "--DIR-- '%s'" dir
        let fileName = (Path.Combine(dir, "Properties", "AssemblyInfo.cs")) |> FullName
        ReplaceAssemblyInfoVersions (fun p ->
          { p with
              AssemblyVersion = assemblyVersion
              AssemblyFileVersion = assemblyVersion
              AssemblyInformationalVersion = assemblyInfoVersion
              OutputFileName = fileName
          })
    )
)

Target "Compile" (fun _ ->
  frameworkVersions
  |> Seq.iter (fun v ->
    appReferences
    |> Seq.iter (fun p -> compileAnyCpu v p "")
  )
)

Target "Test" (fun _ ->
  frameworkVersions
  |> Seq.iter (fun v ->
    testReferences
    |> Seq.iter (fun p -> compileAnyCpu v p "test-")
  )

  frameworkVersions
  |> Seq.iter(fun frameworkVer ->
    let testDir = (Path.Combine(buildDir, "test-" + frameworkVer)) |> FullName
    let testDlls = !! (testDir + "/*Spec.dll")
    let xmlFile = (Path.Combine(testReportsDir, "UnitTests-" + frameworkVer + ".xml")) |> FullName
    NUnit (fun p ->
            {p with
              ToolPath = (Path.Combine(packagesDir, "NUnit.Runners", "tools")) |> FullName
              OutputFile = xmlFile
              Framework = frameworkVer
              ShowLabels = false
            }) testDlls
    sendTeamCityNUnitImport xmlFile
  )
)

Target "Create NuGet packages" (fun _ ->
  let nugetParams p =
    { p with
        ToolPath = nugetExe
        Version = nugetVersionNumber
        OutputPath = artifactsDir
        WorkingDir = artifactsDir
        AccessKey = nugetAccessKey
        NoDefaultExcludes = true
    }
  NuGetPack nugetParams (Path.Combine(rootDir, "gurkburk.nuspec"))
)

(*
Target "Publish to NuGet" (fun _ ->
  let nugetParams p project =
    { p with
          WorkingDir = rootDir
          ToolPath = nugetExe
          AccessKey = nugetAccessKey
          OutputPath = artifactsDir
          Project = project
          Version = nugetVersionNumber
    }
  let publish pkg =
    let project = Path.GetFileName(pkg).Replace(nugetVersionNumber, "").Replace(Path.GetExtension(pkg), "").TrimEnd([|'.'|])
    //NuGetPublish (fun p -> nuGetParams p project) pkg
    ()
  let files = Directory.GetFiles(artifactsDir, "*.nupkg")
  files |> Array.iter publish
)
*)
// Dependencies
"Clean"
  ==> "Set teamcity buildnumber"
  ==> "AssemblyInfo"
  ==> "Compile"
  ==> "Test"
//   ==> "Create NuGet packages"

// Start build
RunTargetOrDefault "Test"
