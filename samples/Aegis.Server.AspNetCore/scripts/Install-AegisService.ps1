#Requires -RunAsAdministrator
<#
.SYNOPSIS
  Publishes Aegis.Server.AspNetCore and installs it as a Windows service.

.PARAMETER InstallPath
  Folder for the published binaries. Default: C:\Program Files\Fire Testing Technology\Aegis

.PARAMETER ServiceName
  Windows service name. Default: AegisLicensingServer

.PARAMETER DisplayName
  Service display name. Default: Aegis Licencing Server
#>
[CmdletBinding()]
param(
    [string]$InstallPath = "C:\Program Files\Fire Testing Technology\Aegis",
    [string]$ServiceName = "AegisLicensingServer",
    [string]$DisplayName = "Aegis Licencing Server"
)

$ErrorActionPreference = "Stop"
$projectRoot = Split-Path -Parent $PSScriptRoot
$projectFile = Join-Path $projectRoot "Aegis.Server.AspNetCore.csproj"

Write-Host "Publishing Aegis server to $InstallPath ..."
New-Item -ItemType Directory -Force -Path $InstallPath | Out-Null

dotnet publish $projectFile `
    -c Release `
    -r win-x64 `
    --self-contained false `
    -o $InstallPath

if ($LASTEXITCODE -ne 0) {
    throw "dotnet publish failed with exit code $LASTEXITCODE"
}

$exePath = Join-Path $InstallPath "Aegis.Server.AspNetCore.exe"
if (-not (Test-Path $exePath)) {
    throw "Published executable not found at $exePath"
}

$dataPath = "C:\ProgramData\Fire Testing Technology\Aegis"
New-Item -ItemType Directory -Force -Path $dataPath | Out-Null
New-Item -ItemType Directory -Force -Path (Join-Path $dataPath "logs") | Out-Null

$certSubject = "CN=Aegis Licencing Server"
$existingCert = Get-ChildItem Cert:\LocalMachine\My |
    Where-Object { $_.Subject -eq $certSubject } |
    Select-Object -First 1
if (-not $existingCert) {
    Write-Host "Creating self-signed HTTPS certificate ($certSubject) ..."
    New-SelfSignedCertificate `
        -DnsName "localhost", $env:COMPUTERNAME `
        -Subject $certSubject `
        -CertStoreLocation "Cert:\LocalMachine\My" `
        -KeyExportPolicy Exportable `
        -NotAfter (Get-Date).AddYears(5) |
        Out-Null
}
else {
    Write-Host "Using existing HTTPS certificate: $($existingCert.Thumbprint)"
}

$existing = Get-Service -Name $ServiceName -ErrorAction SilentlyContinue
if ($existing) {
    Write-Host "Stopping existing service $ServiceName ..."
    Stop-Service -Name $ServiceName -Force -ErrorAction SilentlyContinue
    sc.exe delete $ServiceName | Out-Null
    Start-Sleep -Seconds 2
}

Write-Host "Creating service $ServiceName ..."
# Environment is selected in Program.cs when running as a Windows service (Production).
# Listen URLs come from appsettings.Production.json (HTTP 8888, HTTPS 4443).
New-Service `
    -Name $ServiceName `
    -BinaryPathName "`"$exePath`"" `
    -DisplayName $DisplayName `
    -Description "FTT Aegis licencing admin UI and API." `
    -StartupType Automatic | Out-Null

Write-Host "Starting service $ServiceName ..."
Start-Service -Name $ServiceName

Write-Host ""
Write-Host "Installed and started."
Write-Host "  Service : $ServiceName"
Write-Host "  Binary  : $exePath"
Write-Host "  Data    : $dataPath"
Write-Host "  HTTP    : http://localhost:8888"
Write-Host "  HTTPS   : https://localhost:4443"
Write-Host ""
Write-Host "Open services.msc or: Get-Service $ServiceName"
