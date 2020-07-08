// include Fake libs
#I "./packages/FAKE/tools"
#r "FakeLib.dll"

open Fake
open System
open System.IO


// params from teamcity/commandline
let buildNumber = getBuildParamOrDefault "buildNumber" "0"
let buildTag = getBuildParamOrDefault "buildTag" "devlocal" // For release, set this to "release"
let frameworkVersions = ["4.5"]
let netstandardVersions = ["netstandard1.3"]

let version = "2.1"
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
let nugetExe = (packagesDir + "/NuGet.CommandLine/tools/NuGet.exe") |> FullName
let nugetAccessKey = getBuildParamOrDefault "nugetAccessKey" "NotSet"

let appReferences = !! (sourceDir + "/**/GurkBurk.csproj")
let testReferences = !! (sourceDir + "/**/*Spec.csproj")

// let dotnetcliVersion = "2.2.103"
let dotnetcliVersion = "2.1.503"
// let dotnetcliVersion = "2.1.302"
// let dotnetcliVersion = "1.1.9"
let mutable dotnetExePath = "/Users/morganpersson/.local/share/dotnetcore/dotnet"

// --------------------------------------------------------------------------------------
// Helpers
// --------------------------------------------------------------------------------------
let deleteObjectDirs () =
  DeleteDirs (!! "src/**/obj")
  DeleteDirs (!! "src/**/bin")

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
              Targets = ["Build"]
          }) proj

let run' timeout cmd args dir =
    if execProcess (fun info ->
        info.FileName <- cmd
        if not (String.IsNullOrWhiteSpace dir) then
            info.WorkingDirectory <- dir
        info.Arguments <- args
    ) timeout |> not then
        failwithf "Error while running '%s' with args: %s" cmd args

let run = run' System.TimeSpan.MaxValue

let runDotnet workingDir args =
    let result =
        ExecProcess (fun info ->
            info.FileName <- dotnetExePath
            info.WorkingDirectory <- workingDir
            info.Arguments <- args) TimeSpan.MaxValue
    if result <> 0 then failwithf "dotnet %s failed" args


// --------------------------------------------------------------------------------------
// Targets .netstandard
// --------------------------------------------------------------------------------------


Target "InstallDotNetCLI" (fun _ ->
    dotnetExePath <- DotNetCli.InstallDotNetSDK dotnetcliVersion
)

// Target "Restore" (fun _ ->
//     appReferences
//     |> Seq.iter (fun p ->
//         let dir = System.IO.Path.GetDirectoryName p
//     //     runDotnet dir "restore"
//         DotNetCli.Restore (fun p -> { p with WorkingDir = dir } )
//     )
// )

Target "Build" (fun _ ->
    // deleteObjectDirs ()

    netstandardVersions
    |> Seq.iter(fun v ->
        deleteObjectDirs ()
        appReferences
        |> Seq.iter (fun p ->
            let dir = System.IO.Path.GetDirectoryName p
            printfn "--DIR-- '%s'" dir
            DotNetCli.Restore (fun p -> { p with WorkingDir = dir } )
            let outputDir = (buildDir + "/" + v) |> FullName
            // let outputDir = (buildDir) |> FullName
            // let args =
            //     sprintf
            //         "build %s -f %s -o %s --configuration Debug -v m --version-suffix alpha.001"
            //             p v outputDir
            let args =
                sprintf
                    "build %s -o %s --configuration Debug -v m --version-suffix alpha.001"
                    p outputDir
            runDotnet dir args
        )
    )
)

// --------------------------------------------------------------------------------------
// Targets
// --------------------------------------------------------------------------------------


Target "Clean" (fun _ ->
  killMSBuild()
  deleteObjectDirs ()
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
  deleteObjectDirs ()

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

Target "Package" (fun _ ->
  let nugetParams p =
    { p with
        ToolPath = nugetExe
        Version = nugetVersionNumber
        OutputPath = artifactsDir
        WorkingDir = rootDir
        AccessKey = nugetAccessKey
        NoDefaultExcludes = true
    }
  NuGetPack nugetParams (Path.Combine(rootDir, "gurkburk.nuspec"))
)

Target "Publish" (fun _ ->
  let nugetParams p project =
    { p with
          WorkingDir = rootDir
          ToolPath = nugetExe.Replace(rootDir, "")
          AccessKey = nugetAccessKey
          OutputPath = "./" + artifactsDir.Replace(rootDir, "")
          Project = project
          Version = nugetVersionNumber
          PublishTrials = 1
    }
  let publish (pkg : String) =
    let project = Path.GetFileName(pkg).Replace(nugetVersionNumber, "").Replace(Path.GetExtension(pkg), "").TrimEnd([|'.'|])
    NuGetPublish (fun p ->
      nugetParams p project)
    ()

  Directory.GetFiles(artifactsDir, "*.nupkg")
  |> Array.iter publish
)

// Dependencies
// "Clean"
//   ==> "Set teamcity buildnumber"
//   ==> "AssemblyInfo"
//   ==> "Compile"
//   ==> "Test"
//   ==> "Package"
//   ==> "Publish"

"Clean"
//   ==> "Restore"
  ==> "Build"
  ==> "Test"

// Start build
RunTargetOrDefault "Test"
