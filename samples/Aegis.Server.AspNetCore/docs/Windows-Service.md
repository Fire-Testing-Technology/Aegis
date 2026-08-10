# Running Aegis as a Windows Service

The AspNetCore sample can run interactively (`dotnet run`) or as a Windows service for always-on licencing admin/API.

## What was added

| Piece | Purpose |
|-------|---------|
| `Microsoft.Extensions.Hosting.WindowsServices` | Host lifetime integration |
| `Program.UseWindowsService()` | Service name + proper start/stop |
| `ServicePaths` | SQLite/logs under ProgramData when Production or service |
| `appsettings.Production.json` | HTTP `0.0.0.0:8888`, HTTPS `0.0.0.0:4443`, ProgramData log path |
| `scripts/Install-AegisService.ps1` | Publish + create + start service |
| `scripts/Uninstall-AegisService.ps1` | Stop + delete service |

## Paths

| Item | Location |
|------|----------|
| Binaries (default) | `C:\Program Files\Fire Testing Technology\Aegis\` |
| Database | `C:\ProgramData\Fire Testing Technology\Aegis\aegis.db` |
| Logs | `C:\ProgramData\Fire Testing Technology\Aegis\logs\` |
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

- When detected as a Windows service, the host uses the **Production** environment (unless `ASPNETCORE_ENVIRONMENT` is already set).
- Content root is pinned to the exe directory so static files and `appsettings*.json` resolve correctly (service cwd is otherwise `System32`).
- HTTPS redirection is enabled. Production binds HTTP **8888** and HTTPS **4443**, using a LocalMachine certificate with subject `CN=Aegis Licencing Server` (`AllowInvalid` so the install script’s self-signed cert works). Replace that cert for real deployments.
- Run the service under an account that can write to ProgramData (Local System is fine for the default install).
- Change `JwtSettings:Secret` / `Salt` before any real deployment.
