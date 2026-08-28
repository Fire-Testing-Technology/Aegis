# Running Aegis as a Windows Service

The AspNetCore sample can run interactively (`dotnet run`) or as a Windows service for always-on licencing admin/API.

## What was added

| Piece | Purpose |
|-------|---------|
| `Microsoft.Extensions.Hosting.WindowsServices` | Host lifetime integration with SCM |
| `Program.UseWindowsService()` | Registers as Windows service (`AegisLicensingServer`) |
| `WindowsServiceInfo` | Shared SCM service name / display name |
| `ServicePaths` | SQLite/logs under ProgramData when Production or service |
| `appsettings.Production.json` | HTTP `0.0.0.0:8888`, HTTPS `0.0.0.0:4443`, ProgramData log path |
| `scripts/Install-AegisService.ps1` | Publish + create LocalSystem auto-start service + start |
| `scripts/Uninstall-AegisService.ps1` | Stop + delete service |

## Paths

| Item | Location |
|------|----------|
| Binaries (default) | `C:\Program Files\Fire Testing Technology\Aegis\` |
| Database | `C:\ProgramData\Fire Testing Technology\Aegis\aegis.db` |
| Logs | `C:\ProgramData\Fire Testing Technology\Aegis\logs\` |
| HTTPS certificate | `C:\ProgramData\Fire Testing Technology\Aegis\https.pfx` (Production/service) |
| Signing secrets | `C:\ProgramData\Fire Testing Technology\Aegis\aegis-signature.bin` (Production/service) |

Development still uses relative `aegis-papakura.db` / `logs/` under the content root.

## Install

From an elevated PowerShell:

```powershell
cd samples\Aegis.Server.AspNetCore\scripts
.\Install-AegisService.ps1
```

Then open https://localhost:4443 (or http://localhost:8888).

Optional parameters:

```powershell
.\Install-AegisService.ps1 -InstallPath "D:\Apps\Aegis" -ServiceName "AegisLicensingServer"
```

Requires the .NET 8 runtime on the machine (`--self-contained false`). For a fully self-contained publish, change the script to `--self-contained true`.

## Uninstall

```powershell
.\Uninstall-AegisService.ps1
# also delete binaries:
.\Uninstall-AegisService.ps1 -RemoveInstallPath
```

ProgramData is left in place so licences/users are not wiped accidentally.

## Service management

```powershell
Get-Service AegisLicensingServer
Restart-Service AegisLicensingServer
Stop-Service AegisLicensingServer
```

## Notes

- Install registers an **auto-start** Windows service under **Local System**, with restart-on-failure. The host calls `UseWindowsService()` so SCM start/stop works (do not run the published exe as a plain console for production).
- When detected as a Windows service, the host uses the **Production** environment (unless `ASPNETCORE_ENVIRONMENT` is already set).
- Content root is pinned to the exe directory so static files and `appsettings*.json` resolve correctly (service cwd is otherwise `System32`).
- HTTPS redirection is enabled. Production binds HTTP **8888** and HTTPS **4443**, loading `https.pfx` from ProgramData (`Kestrel:Endpoints:Https:Certificate:Path`). The install script creates a self-signed PFX there if missing (password must match `Certificate:Password` in `appsettings.Production.json`). Replace that file/password for real deployments.
- Change `JwtSettings:Secret` / `Salt` before any real deployment.
