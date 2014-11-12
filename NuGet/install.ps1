param($installPath, $toolsPath, $package, $project)

$projectDirectory = (Get-Item $project.FullName).DirectoryName
Copy-Item $toolsPath\x86\node.dll $projectDirectory\edge\x86\node.dll -Force
Copy-Item $toolsPath\x64\node.dll $projectDirectory\edge\x64\node.dll -Force
& "$toolsPath\install.bat" "$installPath\.." "$projectDirectory" | Write-Host

function MarkDirectoryAsCopyToOutputRecursive($item)
{
    $item.ProjectItems | ForEach-Object { MarkFileASCopyToOutputDirectory($_) }
}

function MarkFileASCopyToOutputDirectory($item)
{
    Try
    {
        Write-Host Try set $item.Name
        $item.Properties.Item("CopyToOutputDirectory").Value = 2
    }
    Catch
    {
        Write-Host RecurseOn $item.Name
        MarkDirectoryAsCopyToOutputRecursive($item)
    }
}


MarkDirectoryAsCopyToOutputRecursive($project.ProjectItems.Item("edge"))

$nodeTargetsFile = "$projectDirectory\App_Build\node.targets"
$nodeTargetsText = Get-Content $nodeTargetsFile
$hasPackageJsonFile = Test-Path $projectDirectory\package.json
Write-Host HasPackageJsonFile = $hasPackageJsonFile
if (("$nodeTargetsText" -eq (Get-Content $installPath\tools\node.targets)) -and !$hasPackageJsonFile) 
{
    Write-Host Updating $nodeTargetsFile
    $oldText = "Target Name=""Npm"""
    $newText = "Target Name=""Npm"" Condition=""Exists('`$(MSBuildThisFileDirectory)..\package.json')"""
    Set-Content $nodeTargetsFile $nodeTargetsText.replace($oldText, $newText)
}