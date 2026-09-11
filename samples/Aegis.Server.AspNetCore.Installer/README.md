# Aegis.Server.AspNetCore Installer

WiX v3.11 MSI for **Aegis.Server.AspNetCore**. Publishes a self-contained build and registers the **AegisLicensingServer** Windows service (display name **FTT Aegis Licencing v{version}**) via native `ServiceInstall` / `ServiceControl` (injected by `HeatTransform.xslt`).

## Prerequisites

- [WiX Toolset v3.11](https://wixtoolset.org/releases/v3.11/stable)
- .NET SDK that can build `net8.0`

## Build

```powershell
& "${env:ProgramFiles(x86)}\Microsoft Visual Studio\2022\BuildTools\MSBuild\Current\Bin\MSBuild.exe" `
  Aegis.Server.AspNetCore.Installer.wixproj /p:Configuration=Release /p:Platform=x64
```

Or from a VS Developer Command Prompt:

```powershell
msbuild Aegis.Server.AspNetCore.Installer.wixproj /p:Configuration=Release /p:Platform=x64
```

Output MSI: `bin\x64\Release\FTTAegisLicencing-1.0.0-BETA0001-x64.msi` (version from the `.wixproj`).

## Service

After install, Apps & Features shows **FTT Aegis Licencing v{version} (x64)**.

In **services.msc** look for **FTT Aegis Licencing v{version}** (service name `AegisLicensingServer`).

Start Menu / Desktop shortcuts open `https://localhost:4443/`.

HTTPS uses `C:\ProgramData\Fire Testing Technology\Aegis\https.pfx` (created automatically on first service start if missing).
