ECHO OFF
IF NOT EXIST version.txt (
	ECHO version.txt missing!
	GOTO :showerror
)

REM Get the version and comment from version.txt lines 2 and 3
SET "release="
SET "comment="
FOR /F "skip=1 delims=" %%i IN (version.txt) DO IF NOT DEFINED release SET "release=%%i"
FOR /F "skip=2 delims=" %%i IN (version.txt) DO IF NOT DEFINED comment SET "comment=%%i"

REM If there's arguments on the command line overrule version.txt and use that as the version
IF [%1] NEQ [] (SET release=%1)
IF [%2] NEQ [] (SET comment=%2) ELSE (IF [%1] NEQ [] (SET "comment="))

SET version=%release%

IF [%comment%] EQU [] (SET version=%release%) ELSE (SET version=%release%-%comment%)
ECHO Building UmbracoDeploy.Contrib %version%

.\tools\nuget.exe restore .\..\src\Umbraco.Deploy.Contrib.sln

RMDIR _BuildOutput /s /q
DEL UmbracoDeploy.Contrib.*.zip /q
DEL UmbracoDeploy.Contrib.*.nupkg /q
DEL UmbracoDeploy.Export.*.nupkg /q

ECHO ################################################################
ECHO Building Umbraco Deploy Contrib
ECHO ################################################################

for /f "usebackq delims=" %%i in (`tools\vswhere -latest -version "[15.0,17.0)" -requires Microsoft.Component.MSBuild -property installationPath`) do (
  set InstallDir=%%i
)
SET MSBUILDPath="%programfiles(x86)%"\MSBuild\14.0\Bin\MsBuild.exe

if exist "%InstallDir%\MSBuild\15.0\Bin\MSBuild.exe" (
  SET MSBUILDPath="%InstallDir%\MSBuild\15.0\Bin\MSBuild.exe"
)

if exist "%programfiles(x86)%\Microsoft Visual Studio\2017\BuildTools\MSBuild\15.0\Bin\MSBuild.exe" (
  SET MSBUILDPath="%programfiles(x86)%\Microsoft Visual Studio\2017\BuildTools\MSBuild\15.0\Bin\MsBuild.exe"
)

ECHO MSBUILD is here %MSBUILDPath%
%MSBUILDPath% "Build.proj" /p:BUILD_RELEASE=%release% /p:BUILD_COMMENT=%comment%

ECHO Packing the NuGet release files
.\tools\NuGet.exe Pack NuSpecs\UmbracoDeploy.Contrib.nuspec -Version %version% -Verbosity quiet
.\tools\NuGet.exe Pack NuSpecs\UmbracoDeploy.Export.nuspec -Version %version% -Verbosity quiet

if %errorlevel% neq 0 exit /b %errorlevel%

GOTO :EOF

PAUSE
