<# 
 .SYNOPSIS
 Updates TeamMate's published ClickOnce application to enable Visual Styles on the WPF app.

 .DESCRIPTION
 This kludgy workaround is an automation of the steps described in
 http://msdn.microsoft.com/en-us/library/hh323463.aspx

 The ONLY reason why we need to introduce Microsoft.Windows.Common-Controls
 in our custom manifest is to support previewing Outlook message files.

 .PARAMETER publishDir
 The location of the ClickOnce published files built by MSBuild.

 .PARAMETER manifestFile
 A custom .manifest file that will be embedded in the resulting TeamMate.exe.deploy file

 .PARAMETER certFile
 Specifies the name of an X509 certificate file with which to sign a manifest or license file.
#>

param($publishDir, $manifestFile, $certFile);

# Constants used below
$keyContainer = "TEAMMATE";
$password = "TeamMate";

function Get-ScriptDirectory()
{
    $invocation = (Get-Variable MyInvocation -Scope 1).Value;
    return Split-Path $invocation.MyCommand.Path;
}

function Join-Path-And-Test($p1, $p2)
{
    $result = Join-Path $p1 $p2;
    if(-not (Test-Path $result))
    {
        $message = "Expected file {0} was not found" -f $result;
        Write-Host -ForegroundColor Red $message;
        exit 1;
    }
    
    return $result;
}

$buildFolder = Get-ScriptDirectory;
$mageExe = Join-Path-And-Test $buildFolder "Tools\mage.exe";
$mtExe = Join-Path-And-Test $buildFolder "Tools\mt.exe";
$snExe = Join-Path-And-Test $buildFolder "Tools\sn.exe";

if(-not (Test-Path -PathType Container $publishDir)) 
{
    throw ("Publish directory {0} was not found." -f $publishDir);
}

if(-not (Test-Path -PathType Leaf $manifestFile)) 
{
    throw ("Manifest file {0} was not found." -f $manifestFile);
}

if(-not (Test-Path -PathType Leaf $certFile)) 
{
    throw ("Certificate file {0} was not found." -f $certFile);
}

$applicationFile = (dir $publishDir -filter *.application)[0].FullName;
$appFiles = Join-Path-And-Test $publishDir "Application Files";

$latestPublishPath = (dir $appFiles| sort -Descending LastWriteTime )[0].FullName;
$exeDeployFile = (dir $latestPublishPath -filter *.exe.deploy)[0].FullName; 
$publishedManifestFile = (dir $latestPublishPath -filter *.manifest)[0].FullName; 

Write-Host "Updating published ClickOnce application at $latestPublishPath";
Write-Host "";

Write-Host "Embedding custom manifest file for published TeamMate.exe.deploy...";
& $mtExe -nologo -manifest "$manifestFile" "-outputresource:$exeDeployFile";

if($LastExitCode -ne 0)
{
    Write-Error "$mtExe -nologo -manifest `"$manifestFile`" `"-outputresource:$exeDeployFile`"";
    Write-Error "Failed with exit code $LastExitCode";
    exit $LastExitCode;
}

Write-Host "Re-signing TeamMate.exe.deploy...";
& $snExe -q -Rca $exeDeployFile $keyContainer;

if($LastExitCode -ne 0)
{
    Write-Error "$snExe -q -Rca `"$exeDeployFile`" $keyContainer";
    Write-Error "Failed with exit code $LastExitCode";
    exit $LastExitCode;
}

Write-Host "Updating published manifest TeamMate.exe.manifest...";
$deployFiles = dir "$latestPublishPath\*.deploy";
$deployFiles | %{ $newName = $_.FullName.Substring(0, $_.FullName.Length - 7); ren $_.FullName $newName };
& $mageExe -u "$publishedManifestFile" -cf "$certFile" -password "$password";
$deployFiles | %{ $oldName = $_.FullName.Substring(0, $_.FullName.Length - 7); ren $oldName $_.FullName };

if($LastExitCode -ne 0)
{
    Write-Error "$mageExe -u `"$publishedManifestFile`" -cf `"$certFile`" -password pwd";
    Write-Error "Failed with exit code $LastExitCode";
    exit $LastExitCode;
}

Write-Host "Updating published TeamMate.application...";
& $mageExe -u "$applicationFile" -appm "$publishedManifestFile" -cf "$certFile" -password "$password";

if($LastExitCode -ne 0)
{
    Write-Error "$mageExe -u `"$applicationFile`" -appm `"$publishedManifestFile`" -cf `"$certFile`" -password pwd";
    Write-Error "Failed with exit code $LastExitCode";
    exit $LastExitCode;
}

Write-Host "Done";
