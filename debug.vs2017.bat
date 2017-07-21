@echo off

for /f "usebackq tokens=*" %%i in (`vswhere -latest -products * -requires Microsoft.Component.MSBuild -property installationPath`) do (
  set InstallDir=%%i
)

if exist "%InstallDir%\MSBuild\15.0\Bin\MSBuild.exe" (
  set msBuildExe="%InstallDir%\MSBuild\15.0\Bin\MSBuild.exe"
)
@echo on

set VSToolsPath="%InstallDir%\MSBuild\Microsoft\VisualStudio\v15.0"
set EnableNuGetPackageRestore=true
call %msBuildExe% AxoCover.sln /t:clean /p:VSToolsPath=%VSToolsPath%
call %msBuildExe% AxoCover.sln /p:VSToolsPath=%VSToolsPath%

if exist AxoCover\bin\Debug\AxoCover.vsix (
  "%InstallDir%\Common7\IDE\VSIXInstaller.exe" /u:"26901782-38e1-48d4-94e9-557d44db052e"
  "%InstallDir%\Common7\IDE\VSIXInstaller.exe" AxoCover\bin\Debug\AxoCover.vsix
  "%InstallDir%\Common7\IDE\devenv.exe"
)