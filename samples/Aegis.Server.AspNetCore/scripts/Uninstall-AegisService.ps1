#Requires -RunAsAdministrator
<#
.SYNOPSIS
  Stops and removes the Aegis Licencing Server Windows service.

.PARAMETER ServiceName
  Windows service name. Default: AegisLicensingServer

.PARAMETER RemoveInstallPath
  If set, also deletes the published binaries folder.

.PARAMETER InstallPath
  Folder removed when -RemoveInstallPath is specified.
#>
[CmdletBinding()]
param(
    [string]$ServiceName = "AegisLicensingServer",
    [switch]$RemoveInstallPath,
    [string]$InstallPath = "C:\Program Files\Fire Testing Technology\Aegis"
)

$ErrorActionPreference = "Stop"

$existing = Get-Service -Name $ServiceName -ErrorAction SilentlyContinue
if (-not $existing) {
    Write-Host "Service '$ServiceName' is not installed."
}
else {
    Write-Host "Stopping $ServiceName ..."
    Stop-Service -Name $ServiceName -Force -ErrorAction SilentlyContinue
    Write-Host "Removing $ServiceName ..."
    sc.exe delete $ServiceName | Out-Null
    Start-Sleep -Seconds 2
    Write-Host "Service removed."
}

if ($RemoveInstallPath -and (Test-Path $InstallPath)) {
    Write-Host "Removing install folder $InstallPath ..."
    Remove-Item -LiteralPath $InstallPath -Recurse -Force
}

Write-Host "Done. ProgramData under 'Fire Testing Technology\Aegis' was left in place."
