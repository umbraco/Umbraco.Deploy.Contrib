$ubuild.DefineMethod("GetUmbracoVersion",
{
  # parse SolutionInfo and retrieve the version string
  $filepath = "$($this.SolutionRoot)\src\SolutionInfo.cs"

  $solutionInfoExists = [System.IO.File]::Exists($filepath)

  if ($solutionInfoExists) 
  {
    $text = [System.IO.File]::ReadAllText($filepath)
    $match = [System.Text.RegularExpressions.Regex]::Matches($text, "AssemblyInformationalVersion\(`"(.+)?`"\)")
  }
  else
  {
    $filepath = "$($this.SolutionRoot)\src\Directory.Build.props"
    $text = [System.IO.File]::ReadAllText($filepath)
    $match = [System.Text.RegularExpressions.Regex]::Matches($text, "<InformationalVersion>(.+)?<\/InformationalVersion>")
  }
  
  $version = $match.Groups[1].ToString()

  # clear
  $pos = $version.IndexOf(' ')
  if ($pos -gt 0) { $version = $version.Substring(0, $pos) }

  # semver-parse the version string
  $semver = [SemVer.SemVersion]::Parse($version)
  $release = "" + $semver.Major + "." + $semver.Minor + "." + $semver.Patch

  $versions = @{
    Semver = $semver
    Release = $release
    Comment = $semver.PreRelease
    Build = $semver.Build
  }

  return $versions
})
