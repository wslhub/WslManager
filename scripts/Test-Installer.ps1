$ErrorActionPreference = 'Stop'
$installer = Get-ChildItem artifacts/packages/*-x64-Setup.exe | Select-Object -First 1
if (-not $installer) { throw 'x64 installer not found.' }
$destination = Join-Path $env:RUNNER_TEMP 'WslManager-installer-smoke'
if (-not $env:RUNNER_TEMP) { throw 'Run this isolated installer smoke test in GitHub Actions.' }
$install = Start-Process $installer.FullName -ArgumentList @('/VERYSILENT', '/SUPPRESSMSGBOXES', '/NORESTART', "/DIR=`"$destination`"") -PassThru -Wait
if ($install.ExitCode -ne 0 -or -not (Test-Path "$destination/WslManager.exe")) { throw 'Installer smoke test failed.' }
if (Test-Path "$destination/portable.flag") { throw 'Installed build must use local application data settings.' }
$uninstall = Start-Process "$destination/unins000.exe" -ArgumentList @('/VERYSILENT', '/SUPPRESSMSGBOXES', '/NORESTART') -PassThru -Wait
if ($uninstall.ExitCode -ne 0 -or (Test-Path "$destination/WslManager.exe")) { throw 'Uninstall smoke test failed.' }
Write-Host 'PASS: per-user install and uninstall.'
