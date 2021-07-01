
$ubuild.DefineMethod("SetUmbracoVersion",
{
  param (
    [Parameter(Mandatory=$true)]
    [string] $version
  )

  try
  {
    Add-Type -Path $this.BuildEnv.Semver > $null
  }
  catch
  {
    throw "Failed to load $this.BuildEnv.Semver."
  }

  # validate input
  $ok = [Regex]::Match($version, "^[0-9]+\.[0-9]+\.[0-9]+(\-[a-z0-9\.]+)?(\+[0-9]+)?$")
  if (-not $ok.Success)
  {
    throw "Invalid version $version."
  }

  # parse input
  try
  {
    $semver = [SemVer.SemVersion]::Parse($version)
  }
  catch
  {
    throw "Invalid version $version."
  }

  #
  $release = "" + $semver.Major + "." + $semver.Minor + "." + $semver.Patch

  $filePath = "$($this.SolutionRoot)\src\SolutionInfo.cs"
  $solutionInfoExists = [System.IO.File]::Exists($filepath)
  if($solutionInfoExists)
  {
    # edit files and set the proper versions and dates
    Write-Host "Update SolutionInfo.cs"
    $this.ReplaceFileText($filePath, `
      "AssemblyFileVersion\(`".+`"\)", `
      "AssemblyFileVersion(`"$release`")")
    $this.ReplaceFileText($filePath, `
      "AssemblyInformationalVersion\(`".+`"\)", `
      "AssemblyInformationalVersion(`"$semver`")")
    $year = [System.DateTime]::Now.ToString("yyyy")
    $this.ReplaceFileText($filePath, `
      "AssemblyCopyright\(`"Copyright © Umbraco (\d{4})`"\)", `
      "AssemblyCopyright(`"Copyright © Umbraco $year`")")
  }else{
    $filePath = "$($this.SolutionRoot)\src\Directory.Build.props"

    # edit files and set the proper versions and dates
    Write-Host "Update Directory.Build.props"
    $this.ReplaceFileText($filePath, `
      "<Version>(.+)?</Version>", `
      "<Version>$release</Version>")

    $this.ReplaceFileText($filePath, `
      "<AssemblyVersion>(.+)?</AssemblyVersion>", `
      "<AssemblyVersion>$release</AssemblyVersion>")

    $this.ReplaceFileText($filePath, `
      "<InformationalVersion>(.+)?</InformationalVersion>", `
      "<InformationalVersion>$semver</InformationalVersion>")


    $this.ReplaceFileText($filePath, `
      "<FileVersion>(.+)?</FileVersion>", `
      "<FileVersion>$release</FileVersion>")    
    $year = [System.DateTime]::Now.ToString("yyyy")

    $this.ReplaceFileText($filePath, `
      "<Copyright>(.+)?</Copyright>", `
      "<Copyright>Copyright " + [char]0x00A9 + " Umbraco $year</Copyright>")
  }
  $this.Version = @{
    Semver = $semver
    Release = $release
    Comment = $semver.PreRelease
    Build = $semver.Build
  }

  if ($this.HasMethod("SetMoreUmbracoVersion"))
  {
    $this.SetMoreUmbracoVersion($semver)
  }

  return $this.Version
})
