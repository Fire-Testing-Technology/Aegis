#Requires -RunAsAdministrator
<#
.SYNOPSIS
  Publishes Aegis.Server.AspNetCore and installs it as a Windows service (Local System, auto-start).

.PARAMETER InstallPath
  Folder for the published binaries. Default: C:\Program Files\Fire Testing Technology\Aegis

.PARAMETER ServiceName
  Windows service name (SCM). Default: AegisLicensingServer

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

$pfxPath = Join-Path $dataPath "https.pfx"
# Must match Kestrel:Certificate:Password in appsettings.Production.json
$pfxPasswordPlain = "aegis-https"
if (-not (Test-Path $pfxPath)) {
    Write-Host "Creating self-signed HTTPS certificate at $pfxPath ..."
    $cert = New-SelfSignedCertificate `
        -DnsName "localhost", $env:COMPUTERNAME `
        -Subject "CN=Aegis Licencing Server" `
        -CertStoreLocation "Cert:\LocalMachine\My" `
        -KeyExportPolicy Exportable `
        -NotAfter (Get-Date).AddYears(5)
    $securePassword = ConvertTo-SecureString -String $pfxPasswordPlain -Force -AsPlainText
    Export-PfxCertificate -Cert $cert -FilePath $pfxPath -Password $securePassword | Out-Null
    Remove-Item -Path "Cert:\LocalMachine\My\$($cert.Thumbprint)" -Force
}
else {
    Write-Host "Using existing HTTPS certificate file: $pfxPath"
}

$existing = Get-Service -Name $ServiceName -ErrorAction SilentlyContinue
if ($existing) {
    Write-Host "Stopping existing service $ServiceName ..."
    Stop-Service -Name $ServiceName -Force -ErrorAction SilentlyContinue
    sc.exe delete $ServiceName | Out-Null
    Start-Sleep -Seconds 2
}

Write-Host "Creating Windows service $ServiceName (LocalSystem, auto-start) ..."
# Spaces after '=' are required by sc.exe.
# Environment is selected in Program.cs when hosted by SCM (Production).
# Listen URLs come from appsettings.Production.json (HTTP 8888, HTTPS 4443).
$create = sc.exe create $ServiceName `
    binPath= "`"$exePath`"" `
    start= auto `
    obj= LocalSystem `
    DisplayName= $DisplayName
if ($LASTEXITCODE -ne 0) {
    throw "sc create failed: $create"
}

sc.exe description $ServiceName "FTT Aegis licencing admin UI and API." | Out-Null
sc.exe failure $ServiceName reset= 86400 actions= restart/60000/restart/60000/restart/60000 | Out-Null
sc.exe failureflag $ServiceName 1 | Out-Null

Write-Host "Starting service $ServiceName ..."
Start-Service -Name $ServiceName

$svc = Get-Service -Name $ServiceName
if ($svc.Status -ne "Running") {
    throw "Service installed but status is $($svc.Status). Check Event Viewer / ProgramData logs."
}

Write-Host ""
Write-Host "Installed and running as a Windows service."
Write-Host "  Service : $ServiceName ($DisplayName)"
Write-Host "  Account : LocalSystem"
Write-Host "  Start   : Automatic"
Write-Host "  Binary  : $exePath"
Write-Host "  Data    : $dataPath"
Write-Host "  HTTPS cert: $pfxPath"
Write-Host "  HTTP    : http://localhost:8888"
Write-Host "  HTTPS   : https://localhost:4443"
Write-Host ""
Write-Host "Open services.msc or: Get-Service $ServiceName"
