rem Argument 1: path to packages directory

if "%python%"=="" (
	powershell -ExecutionPolicy ByPass -File %1\chocolatey.0.9.8.27\tools\chocolateyInstall.ps1
	choco install python2
)