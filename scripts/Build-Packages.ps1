[CmdletBinding()]
param(
    [ValidatePattern('^\d+\.\d+\.\d+$')][string]$Version = '0.2.0',
    [string]$OutputDirectory = 'artifacts/packages',
    [string]$IdentityName = '826b17a3-34b3-4ca0-bc35-d44061193ba9',
    [string]$Publisher = 'CN=WSLHUB'
)
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
if (-not $IsWindows) { throw 'Packaging requires Windows and the Windows SDK.' }
$repo = Split-Path $PSScriptRoot -Parent
$output = [IO.Path]::GetFullPath($OutputDirectory)
New-Item -ItemType Directory -Force $output | Out-Null
$sdk = Get-ChildItem "${env:ProgramFiles(x86)}\Windows Kits\10\bin" -Directory |
    Where-Object { $_.Name -match '^10\.0\.\d+\.0$' -and (Test-Path (Join-Path $_.FullName 'x64\makeappx.exe')) } |
    Sort-Object { [version]$_.Name } -Descending | Select-Object -First 1
if (-not $sdk) { throw 'Windows SDK makeappx.exe was not found.' }
$makeappx = Join-Path $sdk.FullName 'x64\makeappx.exe'
$iscc = "${env:ProgramFiles(x86)}\Inno Setup 6\ISCC.exe"
if (-not (Test-Path $iscc)) { throw 'Inno Setup 6 is required to build the installer.' }
foreach ($arch in @('x64', 'arm64')) {
    $stage = Join-Path $repo "artifacts/staging/$Version/$arch"
    # Only generated staging directories are replaced; output packages remain available.
    if (Test-Path $stage) { Remove-Item $stage -Recurse -Force }
    New-Item -ItemType Directory -Force $stage | Out-Null
    & dotnet publish (Join-Path $repo 'src/WslManager/WslManager.csproj') -c Release -r "win-$arch" --self-contained true -p:Version=$Version -o $stage
    if ($LASTEXITCODE -ne 0) { throw "Publish failed for $arch." }
    Copy-Item (Join-Path $repo 'License.txt') $stage
    & $iscc "/DAppVersion=$Version" "/DAppArch=$arch" "/DPublishDirectory=$stage" "/DPackageOutput=$output" (Join-Path $repo 'packaging/installer.iss')
    if ($LASTEXITCODE -ne 0) { throw "Installer build failed for $arch." }
    New-Item -ItemType File -Force (Join-Path $stage 'portable.flag') | Out-Null
    Compress-Archive -Path "$stage/*" -DestinationPath (Join-Path $output "WslManager-$Version-win-$arch-portable.zip") -Force
    Remove-Item (Join-Path $stage 'portable.flag')
    New-Item -ItemType Directory -Force (Join-Path $stage 'Images') | Out-Null
    Copy-Item (Join-Path $repo 'src/WslManager.Package/Images/StoreLogo.png') (Join-Path $stage 'Images/StoreLogo.png')
    Copy-Item (Join-Path $repo 'src/WslManager.Package/Images/Square150x150Logo.scale-200.png') (Join-Path $stage 'Images/Square150x150Logo.scale-200.png')
    Copy-Item (Join-Path $repo 'src/WslManager.Package/Images/Square44x44Logo.scale-200.png') (Join-Path $stage 'Images/Square44x44Logo.scale-200.png')
    [xml]$manifest = Get-Content (Join-Path $repo 'packaging/AppxManifest.xml') -Raw
    $manifest.Package.Identity.Name = $IdentityName
    $manifest.Package.Identity.Publisher = $Publisher
    $manifest.Package.Identity.Version = "$Version.0"
    $manifest.Package.Identity.ProcessorArchitecture = $arch
    $manifest.Save((Join-Path $stage 'AppxManifest.xml'))
    & $makeappx pack /d $stage /p (Join-Path $output "WslManager-$Version-$arch.msix") /o
    if ($LASTEXITCODE -ne 0) { throw "MSIX build failed for $arch." }
}
Get-ChildItem $output -File | Where-Object Name -ne 'SHA256SUMS.txt' | Get-FileHash -Algorithm SHA256 |
    ForEach-Object { "$($_.Hash.ToLowerInvariant())  $(Split-Path $_.Path -Leaf)" } |
    Set-Content (Join-Path $output 'SHA256SUMS.txt') -Encoding utf8NoBOM
Write-Host "Packages created in $output. MSIX packages are unsigned; Store identity and signing are configured separately."
