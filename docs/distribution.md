# Distribution and Microsoft Store setup

WslManager 0.2 builds self-contained Windows x64 and ARM64 packages with .NET 10. Building does not register an app with an external package catalog. Downloadable channels use the same application code and version.

## Build output

Run packaging from the repository root on Windows with the Windows SDK and Inno Setup 6 installed.

```powershell
./scripts/Build-Packages.ps1 -Version 0.2.0
```

The [packaging script](../scripts/Build-Packages.ps1) produces a per-user EXE installer, portable ZIP and unsigned MSIX for each architecture, plus `SHA256SUMS.txt`. The installer does not require administrator access. The portable ZIP includes `portable.flag`; settings stay next to the executable. Installed builds store settings in `%LOCALAPPDATA%\WslManager\settings.json`.

The [Inno Setup documentation](https://jrsoftware.org/ishelp/) describes installer behavior. MSIX artifacts require a trusted signature for sideloading or processing by Microsoft Store. The repository's placeholder package identity is suitable for build validation only.

## GitHub releases and package catalogs

The **Package release** workflow accepts a numeric three-part version and creates a **draft** GitHub release with installers, ZIPs, MSIX files and hashes after tests. The draft allows review of release notes and installation checks before publication. Once published, the EXE installer and portable ZIP provide two direct distribution channels for issue [#6](https://github.com/wslhub/WslManager/issues/6).

Chocolatey, Scoop and WinGet catalog submission is not configured. Issue #6 listed those as possible channels; creating their community listings requires separate package identifiers and catalog review. Winstall consumes WinGet listings. This repository does not claim those catalog entries exist.

## Microsoft Store prerequisites

The [official submission API prerequisites](https://learn.microsoft.com/windows/uwp/monetize/create-and-manage-submissions-using-windows-store-services) require an existing Partner Center app, a completed initial submission and an associated Entra application. The API cannot reserve the app name or perform the initial registration.

Configure these values on the GitHub **microsoft-store** environment or repository. Use the exact identity shown by Partner Center.

| Kind | Name | Source |
| --- | --- | --- |
| Variable | `STORE_IDENTITY_NAME` | Package identity name |
| Variable | `STORE_PUBLISHER` | Publisher identity, including `CN=` |
| Variable | `STORE_APP_ID` | Partner Center application ID |
| Secret | `STORE_TENANT_ID` | Associated Entra tenant |
| Secret | `STORE_CLIENT_ID` | Entra application ID |
| Secret | `STORE_CLIENT_SECRET` | Entra application credential |

Only the legacy `BASE64_ENCODED_PFX` repository secret existed when inspected on September 5, 2026. Its value was not read or used. The Store workflow uses the API settings above. The legacy certificate is not sufficient for Store API authentication.

## Store submission behavior

Dispatch **Microsoft Store submission** with a version higher than the existing Store package version. Keep `submit` off to build the correctly identified packages first. Set `submit` on when intending to upload and commit them to the existing app. Configure GitHub environment reviewers if the project requires a human release gate.

The [submission script](../scripts/store_submission.py) rejects existing pending submissions, preserves listing/pricing data, replaces package entries, uploads the ZIP and commits the new submission. It retains failed submissions for inspection and never deletes them automatically. It preserves the existing target publication mode. If that mode requires manual publication, Partner Center still controls that final step.

The script distinguishes `PreProcessing`, `Certification` and other in-progress states from `Published`, as documented in [Manage app submissions](https://learn.microsoft.com/windows/uwp/monetize/manage-app-submissions). Authentication, real package ingestion, certification and publication remain unverified until the project supplies the Partner Center configuration and runs this workflow. Issue [#5](https://github.com/wslhub/WslManager/issues/5) remains open for that end-to-end validation.
