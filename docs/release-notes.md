# WslManager 0.2 preview

WslManager moves from .NET 5 and Windows Forms to .NET 10 LTS and WPF. This preview adds the outstanding desktop management features and replaces the obsolete build pipeline.

- Default terminal and text editor selection with persistent arguments
- Window geometry, column widths/order and sort persistence
- Asynchronous distro discovery with stable rows, registry notifications, refresh on activation/actions and a 30-second active-window state fallback
- Archive drag and drop, trust warning with a persistent opt-out, and import to another local drive
- Default-user changes using current WSL's `--manage --set-default-user`
- Network drive mapping/disconnection and elevated ext4 physical-disk/VHD mounting
- Working default-distribution selection, launch/run-as, export, shortcut, properties, terminate and unregister actions
- Current distro installation catalog through `wsl --list --online`
- Self-contained x64 and ARM64 portable packages, per-user EXE installers and unsigned MSIX artifacts
- Windows UI, core regression, installer and Store-script tests

The Korean README contribution from PR #30 is included with the original author's commit history.

Requires Windows with WSL installed. .NET 10 self-contained builds do not require a separate runtime installation. Default-user changes and disk mounting require a recent WSL version. MSIX downloads are unsigned build artifacts and cannot be installed without an appropriate trusted signature.

Validation boundaries: automated WPF tests use a fake WSL runner. Windows Server Core + RDP, real physical-disk operations and Microsoft Store ingestion/publication have not been verified. See [validation](https://github.com/wslhub/WslManager/blob/master/docs/validation.md) and [distribution setup](https://github.com/wslhub/WslManager/blob/master/docs/distribution.md).
