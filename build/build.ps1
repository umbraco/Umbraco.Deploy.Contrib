
  param (
    # get, don't execute
    [Parameter(Mandatory=$false)]
    [Alias("g")]
    [switch] $get = $false,

    # run local, don't download, assume everything is ready
    [Parameter(Mandatory=$false)]
    [Alias("l")]
    [Alias("loc")]
    [switch] $local = $false,

    # keep the build directories, don't clear them
    [Parameter(Mandatory=$false)]
    [Alias("c")]
    [Alias("cont")]
    [switch] $continue = $false
  )

  # ################################################################
  # BOOTSTRAP
  # ################################################################

  # create and boot the buildsystem
  $ubuild = &"$PSScriptRoot\build-bootstrap.ps1"
  if (-not $?) { return }
  $ubuild.Boot($PSScriptRoot,
    @{ Local = $local; },
    @{ Continue = $continue })
  if ($ubuild.OnError()) { return }

  Write-Host "Umbraco Deploy Contrib Build"
  Write-Host "Umbraco.Build v$($ubuild.BuildVersion)"

  # ################################################################
  # TASKS
  # ################################################################

  $ubuild.DefineMethod("CompileUmbracoDeployContrib",
  {
    $buildConfiguration = "Release"

    $src = "$($this.SolutionRoot)\src"
    $log = "$($this.BuildTemp)\dotnet.build.umbraco.log"

    if ($this.BuildEnv.VisualStudio -eq $null)
    {
      throw "Build environment does not provide VisualStudio."
    }

    Write-Host "Compile Umbraco Deploy Contrib"
    Write-Host "Logging to $log"

    # Umbraco.Deploy.Contrib.Connectors compile
    &dotnet build "$src\Umbraco.Deploy.Contrib.Connectors\Umbraco.Deploy.Contrib.Connectors.csproj" `
      --configuration $buildConfiguration `
      --output "$($this.BuildTemp)\bin\\" `
      > $log

    &dotnet publish "$src\Umbraco.Deploy.Contrib.Connectors\Umbraco.Deploy.Contrib.Connectors.csproj" `
      --configuration Release --output "$($this.BuildTemp)\Connectors\bin\\" `
      > $log

    $webAppBin = "$($this.BuildTemp)\Connectors\bin"
    $excludeDirs = @("$($webAppBin)\refs","$($webAppBin)\runtimes","$($webAppBin)\Umbraco","$($webAppBin)\wwwroot")
    $excludeFiles = @("$($webAppBin)\appsettings.*","$($webAppBin)\*.deps.json","$($webAppBin)\*.exe","$($webAppBin)\*.config","$($webAppBin)\*.runtimeconfig.json")
    $this.RemoveDirectory($excludeDirs)
    $this.RemoveFile($excludeFiles)

    if (-not $?) { throw "Failed to compile Umbraco.Deploy.Contrib.Connectors." }

    # /p:UmbracoBuild tells the csproj that we are building from PS, not VS
  })

  $ubuild.DefineMethod("CompileTests",
  {
    $buildConfiguration = "Release"
    $log = "$($this.BuildTemp)\msbuild.tests.log"

    if ($this.BuildEnv.VisualStudio -eq $null)
    {
      throw "Build environment does not provide VisualStudio."
    }

    Write-Host "Compile Tests"
    Write-Host "Logging to $log"

    # beware of the weird double \\ at the end of paths
    # see http://edgylogic.com/blog/powershell-and-external-commands-done-right/
    &$this.BuildEnv.VisualStudio.MsBuild "$($this.SolutionRoot)\src\Umbraco.Deploy.Contrib.Tests\Umbraco.Deploy.Contrib.Tests.csproj" `
      /p:WarningLevel=0 `
      /p:Configuration=$buildConfiguration `
      /p:Platform=AnyCPU `
      /p:UseWPP_CopyWebApplication=True `
      /p:PipelineDependsOnBuild=False `
      /p:OutDir="$($this.BuildTemp)\tests\\" `
      /p:Verbosity=minimal `
      /t:Build `
      /tv:"$($this.BuildEnv.VisualStudio.ToolsVersion)" `
      /p:UmbracoBuild=True `
      > $log

    if (-not $?) { throw "Failed to compile tests." }

    # /p:UmbracoBuild tells the csproj that we are building from PS
  })

  $ubuild.DefineMethod("PrepareBuild",
  {
    Write-Host "============ PrepareBuild ============"

    Write-host "Set environment"
    $env:UMBRACO_VERSION=$this.Version.Semver.ToString()
    $env:UMBRACO_RELEASE=$this.Version.Release
    $env:UMBRACO_COMMENT=$this.Version.Comment
    $env:UMBRACO_BUILD=$this.Version.Build

    if ($args -and $args[0] -eq "vso")
    {
      Write-host "Set VSO environment"
      # set environment variable for VSO
      # https://github.com/Microsoft/vsts-tasks/issues/375
      # https://github.com/Microsoft/vsts-tasks/blob/master/docs/authoring/commands.md
      Write-Host ("##vso[task.setvariable variable=UMBRACO_VERSION;]$($this.Version.Semver.ToString())")
      Write-Host ("##vso[task.setvariable variable=UMBRACO_RELEASE;]$($this.Version.Release)")
      Write-Host ("##vso[task.setvariable variable=UMBRACO_COMMENT;]$($this.Version.Comment)")
      Write-Host ("##vso[task.setvariable variable=UMBRACO_BUILD;]$($this.Version.Build)")

      Write-Host ("##vso[task.setvariable variable=UMBRACO_TMP;]$($this.SolutionRoot)\build.tmp")
    }
  })

  $ubuild.DefineMethod("RestoreNuGet",
  {
    Write-Host "Restore NuGet"
    Write-Host "Logging to $($this.BuildTemp)\nuget.restore.log"
    Write-Host "Restoring solution $($this.SolutionRoot)\src\Umbraco.Deploy.Contrib.sln"
    &$this.BuildEnv.NuGet restore "$($this.SolutionRoot)\src\Umbraco.Deploy.Contrib.sln" > "$($this.BuildTemp)\nuget.restore.log"
    if (-not $?) { throw "Failed to restore NuGet packages." }
  })

  $ubuild.DefineMethod("PackageNuGet",
  {
    Write-Host "Create NuGet packages"

    $src = "$($this.SolutionRoot)\src"
    $log = "$($this.BuildTemp)\dotnet.pack.umbraco.log"
    Write-Host "Logging to $log"

    &dotnet pack "$($src)\Umbraco.Deploy.Contrib.Pack.sln" `
        --output "$($this.BuildOutput)" `
        --verbosity detailed `
        -c Release `
        -p:PackageVersion="$($this.Version.Semver.ToString())" > $log

    # run hook
    if ($this.HasMethod("PostPackageNuGet"))
    {
      Write-Host "Run PostPackageNuGet hook"
      $this.PostPackageNuGet();
      if (-not $?) { throw "Failed to run hook." }
    }
  })

  $ubuild.DefineMethod("Build",
  {
    $error.Clear()

    $this.RestoreNuGet()
    if ($this.OnError()) { return }
    $this.CompileUmbracoDeployContrib()
    if ($this.OnError()) { return }
    $this.PackageNuGet()
    if ($this.OnError()) { return }
    Write-Host "Done"
  })

  # ################################################################
  # RUN
  # ################################################################

  # configure
  $ubuild.ReleaseBranches = @( "master" )

  # run
  if (-not $get)
  {
    $ubuild.Build()
    if ($ubuild.OnError()) { return }
  }
  if ($get) { return $ubuild }
