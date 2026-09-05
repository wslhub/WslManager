using Microsoft.Win32;
using System.IO;
using System.Windows;
using WslManager.Core;

namespace WslManager;

public sealed partial class MainWindow
{
    private Task LaunchAsync()
    {
        WindowsIntegration.Launch(WslCommands.Launch(Selected().Name), settings);
        return Task.CompletedTask;
    }
    private Task FilesAsync()
    {
        WindowsIntegration.Open(WslCommands.NetworkPath(Selected().Name));
        return Task.CompletedTask;
    }
    private async Task<string[]> UsersAsync(string name)
    {
        var output = await service.ExecuteAsync(WslCommands.Linux(name, "root", "cat", "/etc/passwd"), lifetime.Token);
        return output.Split('\n').Select(line => line.Split(':')).Where(parts => parts.Length >= 7)
            .Select(parts => parts[0]).Distinct().Order().ToArray();
    }
    private async Task RunAsAsync()
    {
        var name = Selected().Name;
        var users = await UsersAsync(name);
        var dialog = new FormDialog(this, $"Run in {name}", "Run");
        var user = dialog.Choice("Linux user", users);
        user.IsEditable = true;
        var script = dialog.TextField("Shell command or script (optional)", multiline: true);
        dialog.Note("Leave the script empty to open an interactive shell. Scripts run with /bin/sh -lc as the selected Linux user.");
        if (dialog.ShowDialog() != true) return;
        WindowsIntegration.Launch(WslCommands.Launch(name, user.Text, script.Text), settings);
    }
    private async Task DefaultUserAsync()
    {
        var name = Selected().Name;
        var dialog = new FormDialog(this, $"Default user for {name}", "Apply");
        var user = dialog.Choice("Existing Linux user", await UsersAsync(name));
        user.IsEditable = true;
        dialog.Note("Uses the current WSL default-user command. Update WSL if this command is unavailable. Existing terminal sessions keep their current user.");
        if (dialog.ShowDialog() == true)
            await service.SetDefaultUserAsync(name, user.Text, lifetime.Token);
    }
    private async Task InstallAsync()
    {
        var online = await service.ExecuteAsync(WslCommands.Create("--list", "--online"), lifetime.Token);
        var dialog = new FormDialog(this, "Install distribution", "Install");
        var catalog = dialog.TextField("Available distributions from WSL", online, multiline: true);
        catalog.IsReadOnly = true;
        catalog.Height = 220;
        var name = dialog.TextField("Distribution name from the catalog");
        dialog.Note("WSL downloads the selected distribution. Its first launch completes the Linux account setup. Windows may request elevation.");
        if (dialog.ShowDialog() != true) return;
        // Keep the interactive install console visible for progress and WSL prompts.
        WindowsIntegration.Launch(WslCommands.Create("--install", "--distribution", WslCommands.DistroName(name.Text), "--no-launch"), settings);
    }
    private async Task ImportAsync(string? initialPath = null)
    {
        var dialog = new FormDialog(this, "Import distribution archive", "Import");
        var archive = dialog.PathField("Archive", initialPath ?? "", filter: "Linux root filesystem|*.tar;*.tar.gz;*.tgz");
        var name = dialog.TextField("New distribution name");
        var directory = dialog.PathField("Install directory (empty or new)", directory: true);
        var version = dialog.Choice("WSL version", ["2", "1"]);
        dialog.Note("The install directory may be on another local drive, including an external drive. Keep that drive connected while the distribution is running.");
        if (dialog.ShowDialog() != true) return;
        var command = WslCommands.Import(name.Text, directory.Text, archive.Text, version.SelectedIndex == 0 ? 2 : 1);
        if (!File.Exists(archive.Text)) throw new FileNotFoundException("Archive not found.", archive.Text);
        if (distros.Any(d => string.Equals(d.Name, name.Text, StringComparison.OrdinalIgnoreCase)))
            throw new ArgumentException("A distribution with this name already exists.");
        if (Directory.Exists(directory.Text) && Directory.EnumerateFileSystemEntries(directory.Text).Any())
            throw new ArgumentException("Choose an empty or new install directory.");
        if (settings.WarnBeforeImport)
        {
            var warning = new FormDialog(this, "Trust this archive", "Import");
            warning.Note($"Import only archives from a source you trust. A distribution can run programs and access your Windows files when launched.\n\nArchive: {Path.GetFullPath(archive.Text)}\nDistribution: {name.Text}\nInstall directory: {Path.GetFullPath(directory.Text)}");
            var suppress = warning.Check("Do not show this warning again", false);
            if (warning.ShowDialog() != true) return;
            if (suppress.IsChecked == true) { settings.WarnBeforeImport = false; settingsStore.Save(settings); }
        }
        await service.ExecuteAsync(command, lifetime.Token);
    }
    private async Task ExportAsync()
    {
        var name = Selected().Name;
        var dialog = new SaveFileDialog { Title = $"Export {name}", Filter = "Tar archive|*.tar", DefaultExt = ".tar", FileName = $"{name}-{DateTime.Now:yyyyMMdd}.tar", OverwritePrompt = true };
        if (dialog.ShowDialog(this) == true)
            await service.ExecuteAsync(WslCommands.Export(name, dialog.FileName), lifetime.Token);
    }
    private Task ShortcutAsync()
    {
        var name = Selected().Name;
        var dialog = new SaveFileDialog { Title = "Create distribution shortcut", Filter = "Windows shortcut|*.lnk", DefaultExt = ".lnk", FileName = name + ".lnk" };
        if (dialog.ShowDialog(this) == true) WindowsIntegration.CreateShortcut(name, dialog.FileName);
        return Task.CompletedTask;
    }
    private async Task TerminateAsync()
    {
        var name = Selected().Name;
        if (Confirm($"Terminate {name}? Running processes will stop. Save any open files first."))
            await service.ExecuteAsync(WslCommands.Terminate(name), lifetime.Token);
    }
    private async Task ShutdownAsync()
    {
        if (Confirm("Shut down all WSL distributions? Running processes will stop. Save any open files first."))
            await service.ExecuteAsync(WslCommands.Create("--shutdown"), lifetime.Token);
    }
    private async Task UnregisterAsync()
    {
        var name = Selected().Name;
        var dialog = new FormDialog(this, "Unregister distribution", "Delete distribution");
        dialog.Note($"This permanently deletes all files, software and settings in {name}. Export a backup first if you want to retain them.");
        var confirm = dialog.TextField($"Type {name} to confirm");
        if (dialog.ShowDialog() != true) return;
        if (!string.Equals(confirm.Text, name, StringComparison.Ordinal)) throw new ArgumentException("The confirmation name does not match. No distribution was deleted.");
        await service.ExecuteAsync(WslCommands.Unregister(name), lifetime.Token);
    }
    private bool Confirm(string message) => MessageBox.Show(this, message, Title, MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No) == MessageBoxResult.Yes;
    private async Task DriveAsync(bool disconnect)
    {
        var name = Selected().Name;
        var dialog = new FormDialog(this, disconnect ? "Disconnect network drive" : "Map network drive", disconnect ? "Disconnect" : "Map");
        dialog.Note(WslCommands.NetworkPath(name));
        var letters = Enumerable.Range('D', 'Z' - 'D' + 1).Select(c => ((char)c) + ":").Reverse().ToArray();
        var letter = dialog.Choice("Drive letter", letters);
        var persistent = dialog.Check("Reconnect at sign-in", true);
        persistent.Visibility = disconnect ? Visibility.Collapsed : Visibility.Visible;
        if (dialog.ShowDialog() == true)
        {
            var drive = letter.Text;
            var reconnect = persistent.IsChecked == true;
            await Task.Run(() =>
            {
                if (disconnect) WindowsIntegration.UnmapDrive(name, drive);
                else WindowsIntegration.MapDrive(name, drive, reconnect);
            });
        }
    }
    private async Task DiskAsync(bool unmount)
    {
        var help = await service.ExecuteAsync(WslCommands.Create("--help"), lifetime.Token);
        if (!help.Contains("--mount", StringComparison.Ordinal)) throw new NotSupportedException("This WSL installation does not support disk mounting. Update WSL first.");
        var dialog = new FormDialog(this, unmount ? "Unmount disk" : "Mount ext4 disk", unmount ? "Unmount" : "Mount");
        var path = dialog.PathField(@"Disk path (for example \\.\PHYSICALDRIVE2) or VHD", filter: "Virtual disk|*.vhd;*.vhdx");
        var vhd = dialog.Check("The path refers to a VHD or VHDX file", false);
        var partition = dialog.TextField("Partition number (optional)");
        partition.IsEnabled = !unmount;
        dialog.Note("Windows requests administrator access. Physical disks must be offline and not in use by Windows. The Windows system disk cannot be mounted. Only the specified disk is affected.");
        if (dialog.ShowDialog() != true) return;
        int? partitionNumber = string.IsNullOrWhiteSpace(partition.Text) ? null : int.Parse(partition.Text);
        var command = unmount ? WslCommands.Unmount(path.Text, vhd.IsChecked == true)
            : WslCommands.Mount(path.Text, partitionNumber, vhd.IsChecked == true);
        if (vhd.IsChecked == true && !File.Exists(path.Text)) throw new FileNotFoundException("Virtual disk not found.", path.Text);
        if (Confirm($"{(unmount ? "Unmount" : "Mount")} this disk?\n{path.Text}"))
            await WindowsIntegration.RunElevatedAsync(command);
    }
    private Task EditConfigAsync()
    {
        var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".wslconfig");
        if (!File.Exists(path)) File.WriteAllText(path, "");
        WindowsIntegration.Edit(path, settings);
        MessageBox.Show(this, "Save your changes in the editor. WSL applies them after a full shutdown and restart. Use Tools > Shut down WSL when your work is saved.", Title);
        return Task.CompletedTask;
    }
    private async Task PropertiesAsync()
    {
        var distro = Selected();
        var users = await UsersAsync(distro.Name);
        var dialog = new FormDialog(this, $"Properties: {distro.Name}");
        dialog.Note($"Name: {distro.Name}\nState: {distro.State}\nWSL version: {distro.Version}\nDefault: {distro.IsDefault}\nLocation: {WindowsIntegration.DistroLocation(distro.Name) ?? "Unavailable"}\nFiles: {WslCommands.NetworkPath(distro.Name)}");
        var list = dialog.TextField("Linux users", string.Join(Environment.NewLine, users), true);
        list.IsReadOnly = true;
        dialog.ShowDialog();
    }
    private Task SettingsAsync()
    {
        var dialog = new FormDialog(this, "Settings", "Save");
        var terminal = dialog.Choice("Default terminal", ["System console", "Windows Terminal", "Custom terminal"], (int)settings.Terminal);
        var terminalPath = dialog.PathField("Custom terminal executable", settings.TerminalPath, filter: "Executable|*.exe");
        var terminalArgs = dialog.TextField("Custom terminal prefix arguments (one argument per line)", string.Join("\n", settings.TerminalArguments), true);
        var editorPath = dialog.PathField("Text editor executable (empty uses Notepad)", settings.EditorPath, filter: "Executable|*.exe");
        var editorArgs = dialog.TextField("Editor prefix arguments (one argument per line)", string.Join("\n", settings.EditorArguments), true);
        var warn = dialog.Check("Show a security warning before importing archives", settings.WarnBeforeImport);
        dialog.Note("Custom terminals receive the WSL executable and its arguments after the prefix arguments. Editors receive the file path as the final argument. Enter arguments without shell quoting.\n\nSettings: " + settingsStore.FilePath);
        if (dialog.ShowDialog() != true) return Task.CompletedTask;
        if (terminal.SelectedIndex == (int)TerminalKind.Custom && !File.Exists(terminalPath.Text))
            throw new FileNotFoundException("Select an existing custom terminal executable.", terminalPath.Text);
        if (!string.IsNullOrWhiteSpace(editorPath.Text) && !File.Exists(editorPath.Text))
            throw new FileNotFoundException("Select an existing editor executable.", editorPath.Text);
        settings.Terminal = (TerminalKind)terminal.SelectedIndex;
        settings.TerminalPath = terminalPath.Text;
        settings.TerminalArguments = Lines(terminalArgs.Text);
        settings.EditorPath = editorPath.Text;
        settings.EditorArguments = Lines(editorArgs.Text);
        settings.WarnBeforeImport = warn.IsChecked == true;
        settingsStore.Save(settings);
        return Task.CompletedTask;
    }
    private static string[] Lines(string text) => text.Split('\n').Select(line => line.TrimEnd('\r')).Where(line => line.Length > 0).ToArray();
}
