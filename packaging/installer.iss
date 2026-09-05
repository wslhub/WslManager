#ifndef AppVersion
  #define AppVersion "0.2.0"
#endif
[Setup]
AppId={{74D17D1C-5B2E-479A-85C7-2025F2B6B261}
AppName=WslManager
AppVersion={#AppVersion}
AppPublisher=WSLHub
AppPublisherURL=https://github.com/wslhub/WslManager
DefaultDirName={localappdata}\Programs\WslManager
DefaultGroupName=WslManager
PrivilegesRequired=lowest
UninstallDisplayIcon={app}\WslManager.exe
OutputDir={#PackageOutput}
OutputBaseFilename=WslManager-{#AppVersion}-{#AppArch}-Setup
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
MinVersion=10.0.19041
#if AppArch == "x64"
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
#else
ArchitecturesAllowed=arm64
ArchitecturesInstallIn64BitMode=arm64
#endif
[Files]
Source: "{#PublishDirectory}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
[Icons]
Name: "{group}\WslManager"; Filename: "{app}\WslManager.exe"
[Run]
Filename: "{app}\WslManager.exe"; Description: "Open WslManager"; Flags: nowait postinstall skipifsilent
