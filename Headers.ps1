$header = "// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.`r`n"
function Write-Header ($file)
{
    $content = Get-Content $file
    $filename = Split-Path -Leaf $file
    $fileheader = $header -f $filename,$companyname,$date
    Set-Content $file $fileheader
    Add-Content $file $content
}

Get-ChildItem $target -Recurse | ? { $_.Extension -like ".cs" } | % `
{
    Write-Header $_.PSPath.Split(":", 3)[2]
}