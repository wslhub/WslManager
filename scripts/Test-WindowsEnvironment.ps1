[CmdletBinding()]
param(
    [string]$OutputDirectory = 'artifacts/windows-environment',
    [string]$DistroName,
    [switch]$RequireServerCoreRdp
)
$ErrorActionPreference = 'Stop'
$output = [IO.Path]::GetFullPath($OutputDirectory)
New-Item -ItemType Directory -Force $output | Out-Null
$os = Get-CimInstance Win32_OperatingSystem
$installationType = (Get-ItemProperty 'HKLM:\SOFTWARE\Microsoft\Windows NT\CurrentVersion').InstallationType
$isRdp = $env:SESSIONNAME -like 'RDP-*'
$evidence = [ordered]@{
    TimestampUtc = [DateTime]::UtcNow.ToString('o')
    OS = $os.Caption
    Build = $os.BuildNumber
    Architecture = $os.OSArchitecture
    InstallationType = $installationType
    RdpSession = $isRdp
}
$evidence | ConvertTo-Json | Set-Content (Join-Path $output 'environment.json')
if ($RequireServerCoreRdp -and ($installationType -ne 'Server Core' -or -not $isRdp)) {
    throw 'Issue #16 requires an actual Server Core installation in an RDP session. This environment does not satisfy that condition.'
}
& dotnet --info | Out-File (Join-Path $output 'dotnet.txt')
if ($LASTEXITCODE -ne 0) { throw '.NET SDK was not found.' }
& wsl.exe --version 2>&1 | Out-File (Join-Path $output 'wsl-version.txt')
if ($LASTEXITCODE -ne 0) { throw 'Current WSL is not available. Review wsl-version.txt.' }
& wsl.exe --list --verbose 2>&1 | Out-File (Join-Path $output 'wsl-list.txt')
if ($LASTEXITCODE -ne 0) { throw 'WSL enumeration failed.' }
if ($DistroName) {
    & wsl.exe --distribution $DistroName --exec uname -a | Out-File (Join-Path $output 'linux.txt')
    if ($LASTEXITCODE -ne 0) { throw 'The selected distribution could not execute uname.' }
}
& dotnet run --project (Join-Path $PSScriptRoot '../tests/WslManager.UiTests') -c Release -- (Join-Path $output 'ui')
if ($LASTEXITCODE -ne 0) { throw 'WPF window verification failed.' }
Write-Host "Environment and window evidence saved to $output. Complete the real-distro interaction checklist in docs/validation.md."
