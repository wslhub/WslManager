# Validation and remaining environment checks

The .NET 10 implementation separates command construction and process execution from the WPF window. Automated tests use an injected WSL runner unless explicitly stated otherwise. The automated WPF test opens real Windows windows and manipulates their controls; it does not operate on installed Linux distributions.

The [Windows/Linux CI run for commit cee45ec](https://github.com/wslhub/WslManager/actions/runs/33951401490) passed on September 5, 2026, including all 26 core cases, five Store-script cases, real WPF windows and x64 installer installation/uninstallation. Both x64 and ARM64 packages built successfully. This is build validation for ARM64; ARM64 hardware execution has not been tested.

## Automated coverage

| Layer | Checks | Execution environment |
| --- | --- | --- |
| Core | Localized list parsing, stable objects, argument boundaries, validation, settings round trip and corruption backup, default-user sequencing | macOS locally, Windows and Linux in CI |
| OS process runner | Concurrent stdout/stderr draining, nonzero exit codes, cancellation and child process termination | Real child processes on each test OS |
| WPF | Window creation, list selection, refresh, filtering, error retention, Settings dialog, column order and geometry persistence | Windows CI with fake WSL responses |
| Packaging | Self-contained x64 and ARM64 publish, Inno Setup EXE, unsigned MSIX, portable ZIP and SHA-256 | Windows CI |
| Installer | Per-user silent install and uninstall | Disposable Windows CI runner, x64 |
| Store automation | Preserve listings and pricing, reject pending submissions, upload before commit, report errors and incomplete publication | Python tests with a fake Store API |

Run the cross-platform regression suite with the following command.

```sh
dotnet test tests/WslManager.Core.Tests -c Release
```

The [build workflow](../.github/workflows/build.yml) uploads UI screenshots and package artifacts. Its success establishes these checks, not physical-disk compatibility or Store certification.

## Real WSL interaction checklist

Use disposable distributions and a spare disk for data-changing checks. Keep exports outside the import directory. Microsoft documents the command semantics in [basic WSL commands](https://learn.microsoft.com/windows/wsl/basic-commands) and [disk mounting](https://learn.microsoft.com/windows/wsl/wsl2-mount-disk).

1. Start the app with no registered distributions and with WSL unavailable. Confirm the empty state and actionable error message respectively.
2. Install a distribution from the current WSL catalog. Launch it and complete account setup. Confirm the manager discovers it without losing other selections.
3. Launch an existing distribution in the system console, Windows Terminal and a configured custom terminal. Test a name and a Windows path containing spaces.
4. Run a command as an existing user, change the default user, then start a new shell and verify `id -un`. Check an invalid user and an older WSL installation.
5. Export a disposable distribution. Import the archive under a new name into an empty directory on a second drive. Test `.tar`, `.tar.gz` and `.tgz`, a missing file, a duplicate name and a nonempty directory.
6. Drop one supported archive on the main window. Confirm the import dialog appears. Cancel the trust warning and verify no distribution is created. Opt out, restart, then re-enable the warning in Settings.
7. Map a free drive letter to the selected distribution. Verify file access and disconnect it. Confirm the app refuses to disconnect a letter mapped to a different share.
8. Mount and unmount a spare offline ext4 disk and a VHDX through the UAC prompt. Cancel UAC once and verify the app remains usable. Never use the Windows system disk for this test.
9. Terminate and unregister only disposable distributions. Confirm cancellation and the typed-name check prevent deletion.
10. Edit `.wslconfig` with the selected editor. Close and reopen the app after moving the window and reordering/resizing/sorting columns. Disconnect a monitor and confirm the title bar stays reachable.

## Windows Server Core and RDP evidence for issue #16

Issue [#16](https://github.com/wslhub/WslManager/issues/16) asks for a specific environment. A GitHub Windows desktop runner does not satisfy it. No Server Core + RDP environment has been verified in this modernization run.

From an actual RDP session on Server Core with the required WPF/.NET and WSL components available, run the environment collector from the repository root.

```powershell
./scripts/Test-WindowsEnvironment.ps1 -RequireServerCoreRdp -DistroName '<disposable-distro>'
```

The script records OS/build/installation type, WSL version/list and real WPF window screenshots. It deliberately fails when the host is not Server Core or the session is not RDP. Complete the real-WSL checklist above before treating #16 as verified. If the intended Server Core installation cannot provide the required desktop/WPF components, record that result and decide the support policy explicitly.

## Review results

| Dimension | Result | Evidence and limit |
| --- | --- | --- |
| Security | Improved | Argument arrays replace `cmd /c` interpolation; import trust prompt; typed unregister confirmation; disk path validation; mapping ownership check |
| Performance | Improved | Async WSL processes, concurrent pipe draining, incremental rows, registry notifications and debouncing; active-window status fallback every 30 seconds |
| Correctness | Automated coverage added | Tests exercise success and failure paths; full WSL, disk and Server Core checks remain environment-dependent |
| Maintainability | Simplified | WPF plus a dependency-free .NET core; no EF Core/SQLite cache, ObjectListView or Newtonsoft.Json runtime dependency |

The refreshed CI does not load a signing certificate for pull requests. Store credentials are limited to the explicitly dispatched Store workflow. A Store commit accepted for processing is reported separately from publication.
