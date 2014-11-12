param($installPath, $toolsPath, $package)

Write-Host install path = $installPath
Write-Host install path = $toolsPath
& "$toolsPath\init.bat" "$installPath\.." | Write-Host