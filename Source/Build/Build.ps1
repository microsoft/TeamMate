<# 
 .SYNOPSIS
 A script for building and publishing TeamMate to Toolbox.

 .DESCRIPTION
 This scripts takes care of versioning TeamMate, performing a clean build, and generating
 ClickOnce files. It can also publish the ClickOnce files to the Toolbox file share.

 .PARAMETER debug
 Builds the Debug flavor as opposed to the (default) Relase flavor.

 .PARAMETER updateVersion
 If true, generates a new version and updates source files with that value

 .PARAMETER build
 If true, performs a build (default value is true unless upload is specified).

 .PARAMETER upload
 If true, uploads the previous build to Toolbox. This must be invoked after having performed a build.

 .PARAMETER noresign
 If specified, avoids re-signing the ClickOnce manifest with the custom patched manifest.

 .PARAMETER local
 If specified, generates a published build targetted for local testing only, NOT FOR UPLOADING TO TOOLBOX.
#>

param([switch]$debug, [switch] $updateVersion, [switch] $build, [switch] $upload, [switch]$noresign, [switch]$local)

###############################################################################
# Helper Functions
###############################################################################

function Get-ScriptDirectory()
{
    $invocation = (Get-Variable MyInvocation -Scope 1).Value;
    return Split-Path $invocation.MyCommand.Path;
}

function Write-Info($message)
{
    Write-Host -ForegroundColor Cyan $message;
}

function Write-Warning($message)
{
    Write-Host -ForegroundColor Yellow $message;
}

function Write-Error($message)
{
    Write-Host -ForegroundColor Red $message;
}

# Updates a file with a new content, only if the content has changed.
# Also adds the file to a list of files to checkout if the content did change.

function UpdateFile($file, $content, [switch] $UTF8)
{
    $oldContent = [System.IO.File]::ReadAllText($file);

    if( $content -ne $oldContent )
    {
        # Ensure version file is writeable
        $fileInfo = New-Object System.IO.FileInfo $file
        $fileInfo.IsReadOnly = $false;
        if( $utf8 )
        {
            Set-Content -path $file -value $content -encoding UTF8;
        }
        else
        {
            Set-Content -path $file -value $content;
        }
        
        Write-Host "Updated $file with new version info";
        
        return $true;
    }
    
    return $false;
}

# Generates a new build version based on the current date

function GenerateBuildVersion()
{
    # We will calculate the 3rd component (upgrade segment) of the version number
    # based on the date. The number has to be less than 65355, so the first digit
    # should be a number between 1-6 (if we use 0 we mess up stuff). The first number
    # will be given by the year mod 6 + 1, e.g. 2009 % 6 -> 5 + 1 -> 6
    
    $date = Get-Date;
    $firstDigit = ($date.Year % 6) + 1;
    $buildVersion = "{0}{1:00}{2:00}" -f $firstDigit, $date.Month, $date.Day;
    return $buildVersion;
}

# Parses a full version, and returns an object with properties for each version component

function ParseVersion($version)
{
    $regex = [regex] "(\d+)\.(\d+)\.(\d+)\.(\d+)";
    $match = $regex.Match($version);

    if(!$match.Success)
    {
        throw ("String does not match expected version format: {0}" -f $version);
    }
    
    $versionObject = new-object psobject;
    $versionObject | add-member noteproperty "Major" ([int] $match.Groups[1].Value);
    $versionObject | add-member noteproperty "Minor" ([int] $match.Groups[2].Value);
    $versionObject | add-member noteproperty "Build" ([int] $match.Groups[3].Value);
    $versionObject | add-member noteproperty "Revision" ([int] $match.Groups[4].Value);
    return $versionObject;
}

# Updates the build version (3rd component) of a given full version

function UpdateBuildVersion($currentVersion, $buildVersion)
{
	$currentBuildVersion = $currentVersion.Build;

    if($buildVersion -ne $currentBuildVersion)
    {
        $currentVersion.Build = $buildVersion;
        $currentVersion.Revision = 0;

        if($buildVersion -lt $currentBuildVersion) 
        {
          $currentVersion.Minor += 1;
          Write-Warning ("WARNING: IMPORTANT!!!! Build version rolled over from {0} to {1} so incrementing minor version to {2}" -f $currentBuildVersion,$buildVersion,$currentVersion.Minor);
        }
        else
        {
          $currentVersion.Build = $buildVersion;
        }
    }
    else
    {
        $currentVersion.Revision = $version.Revision + 1;
    }
}

###############################################################################
# Script
###############################################################################

$msbuild = "C:\Program Files (x86)\MSBuild\14.0\Bin\MSBuild.exe"
$scriptFolder = Get-ScriptDirectory;
$versionFile = "$scriptFolder\version.txt";
$buildInfoFile = "$scriptFolder\BuildInfo.cs";
$versionTargetsFile = "$scriptFolder\Microsoft.Internal.Tools.TeamMate.Version.targets";
$project = "$scriptFolder\..\TeamMate\TeamMate.csproj";
$resign = (-not $noresign);

if(-not ($updateVersion -or $build -or $upload))
{
    Write-Error "Please specify one or more of: -updateVersion, -build, -upload";
    exit 1
}

if($debug)
{
    $configs = ( "Debug" );
}
else
{
    $configs = ( "Release" );
}

if($updateVersion) 
{
    $buildVersion = GenerateBuildVersion
    $version = ParseVersion (gc $versionFile);
    UpdateBuildVersion $version $buildVersion; 
    $newVersion = "{0}.{1}.{2}.{3}" -f $version.Major,$version.Minor,$version.Build,$version.Revision;

    Write-Info "Updating Build Version to $newVersion..."

    $ignore = UpdateFile $versionFile $newVersion;

    $buildInfo = Get-Content $buildInfoFile;
    $buildInfo = $buildInfo -replace "AssemblyVersion\(.*", "AssemblyVersion(""$newVersion"")]";
    $buildInfo = $buildInfo -replace "AssemblyFileVersion\(.*", "AssemblyFileVersion(""$newVersion"")]";
    $ignore = UpdateFile $buildInfoFile $buildInfo;

    $versionTargets = Get-Content $versionTargetsFile;
    $versionTargets = $versionTargets -replace "<ApplicationVersion>.*</ApplicationVersion>", "<ApplicationVersion>$newVersion</ApplicationVersion>";
    $ignore = UpdateFile $versionTargetsFile $versionTargets -UTF8;
}

if($build)
{
    foreach($config in $configs)
    {
        Write-Info "Building $config...";
        . "$msbuild" "$project" "/p:Configuration=$config" "/p:Publish=true" "/p:Local=$local" /nologo /v:m /t:rebuild 

        if($LastExitCode -ne 0 )
        {
          Write-Error "MSBuild failed. Exiting early!";
          exit 1
        }
    }

    foreach($config in $configs)
    {
        Write-Info "Publishing ClickOnce files for $config...";
        . "$msbuild" "$project" "/p:Configuration=$config" "/p:Publish=true" "/p:Resign=$resign" "/p:Local=$local" /nologo /v:m /t:publish

        if($LastExitCode -ne 0 )
        {
          Write-Error "Publishing failed!";
          exit 1;
        }
    }
}

if($upload)
{
    foreach($config in $configs)
    {
        Write-Info "Uploading ClickOnce files for $config...";
        . "$msbuild" "$project" "/p:Configuration=$config" "/p:Publish=true" /nologo /v:m /t:upload

        if($LastExitCode -ne 0 )
        {
          Write-Error "Uploading failed!";
          exit 1;
        }
    }
}

Write-Host ""
Write-Host -ForegroundColor Green "Done...";