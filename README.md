# WslManager

[한국어](README.ko-kr.md)

WslManager manages Windows Subsystem for Linux distributions through a native WPF interface. The 0.2 preview uses .NET 10 LTS and replaces the old .NET 5 / Windows Forms implementation.

Manage distributions, import and export archives, select terminal/editor applications, persist window preferences, map network drives and mount ext4 disks. The [release notes](docs/release-notes.md) describe the current implementation and validation limits.

## Windows and WSL requirements

Use a supported Windows installation with WSL enabled. Builds target Windows x64 and ARM64. The installer requires Windows build 19041 or later; actual support also depends on the [.NET 10 supported OS policy](https://github.com/dotnet/core/blob/main/release-notes/10.0/supported-os.md).

The default-user action uses current WSL's `--manage --set-default-user` and checks command availability. Disk mounting requires a current WSL installation and administrator approval for that operation. Other actions run as the current Windows user. See [Microsoft's WSL commands](https://learn.microsoft.com/windows/wsl/basic-commands) and [disk mounting requirements](https://learn.microsoft.com/windows/wsl/wsl2-mount-disk).

Windows Server Core + RDP support is **not verified**. See [issue #16 and the validation procedure](docs/validation.md#windows-server-core-and-rdp-evidence-for-issue-16).

## Distribution management

- Open a terminal, open distro files, run as an existing Linux user, or execute a shell script
- Install from the current WSL catalog instead of a hardcoded distro download list
- Import `.tar`, `.tar.gz` and `.tgz` archives into a new or empty directory, including another local drive
- Export a distro, choose the default distro/user, create a shortcut and inspect Linux users
- Map/disconnect a network drive to `\\wsl$\<DistroName>`
- Mount/unmount a specified ext4 disk or virtual disk with a UAC prompt
- Terminate or unregister a distro with confirmation

Drag one supported archive onto the window to open the import dialog. The trust warning can be disabled and restored in Settings. Unregister requires typing the selected distribution name. Read [WSL command behavior](https://learn.microsoft.com/windows/wsl/basic-commands) before using data-changing operations.

## Build and test

Install a stable .NET 10 SDK. From the repository root, build the solution and run the core regression suite.

```sh
dotnet build src/WslManager.slnx -c Release
dotnet test tests/WslManager.Core.Tests -c Release
```

Core tests run on Windows, Linux and macOS. The WPF application only runs on Windows. The [Windows UI test executable](tests/WslManager.UiTests) opens and manipulates real windows with an injected fake WSL runner. The [CI workflow](.github/workflows/build.yml) also builds packages and verifies x64 installer installation/uninstallation.

## Packages and release channels

The [Releases page](https://github.com/wslhub/WslManager/releases) contains published versions when available. The **Build and test** workflow provides CI package artifacts. The **Package release** workflow creates a draft release containing x64/ARM64 portable ZIPs, per-user EXE installers, unsigned MSIX files and SHA-256 hashes.

Self-contained packages include .NET. EXE/ZIP builds do not use a Store certificate. MSIX artifacts require signing for sideloading. [Distribution setup](docs/distribution.md) describes the package commands and optional Microsoft Store workflow. Store registration, credentials, certification and publication are external steps; the presence of the workflow does not establish a Store release.

## Local settings and refresh behavior

Installed builds save settings to `%LOCALAPPDATA%\WslManager\settings.json`. A `portable.flag` file next to the executable keeps settings in that directory instead. JSON saves use a temporary file and replacement. Invalid settings are preserved in a timestamped backup before defaults are used.

Settings include window size/position, maximized state, column order/widths, sorting, terminal/editor executables and prefix arguments, and the import warning. Custom terminal arguments precede the WSL executable and its arguments. Editor arguments precede the file path. These are argument arrays, not shell command strings. Windows Terminal falls back to the system console for scripts containing semicolons to preserve their semantics.

The app refreshes after actions and on activation, watches registry changes, debounces bursts and retains the last successful list on errors. An active-window check every 30 seconds covers running-state changes that do not produce registry notifications. See the [implementation and regression coverage](docs/validation.md).

## License and contributions

WslManager follows the [MIT license](License.txt). PR [#30](https://github.com/wslhub/WslManager/pull/30) contributed the original Korean README; its author history is preserved in this update. Package artwork comes from the existing project assets. Original icon credits: [Icons8](https://www.icons8.com) and [Penguin window by mimooh](https://commons.wikimedia.org/wiki/File:Penguin_window_by_mimooh.svg).

Track remaining work in [GitHub issues](https://github.com/wslhub/WslManager/issues). Verified build/UI checks and environment-dependent WSL checks are recorded separately in [validation](docs/validation.md).
