using Microsoft.Win32;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using WslManager.Core;
using WslManager.External;

namespace WslManager;

public static class WindowsIntegration
{
    public static void Launch(Command command, AppSettings settings)
    {
        if (settings.Terminal != TerminalKind.SystemConsole)
        {
            var path = settings.Terminal == TerminalKind.WindowsTerminal ? "wt.exe" : settings.TerminalPath;
            if (string.IsNullOrWhiteSpace(path)) throw new ArgumentException("Select a terminal executable in Settings.");
            // Windows Terminal treats semicolons as separators even inside a child command.
            // Use the system console for scripts containing them instead of changing script semantics.
            if (settings.Terminal == TerminalKind.WindowsTerminal && command.Arguments.Any(a => a.Contains(';')))
            {
                Start(command.CreateStartInfo(capture: false));
                return;
            }
            string[] prefix = settings.Terminal == TerminalKind.WindowsTerminal ? ["new-tab"] : settings.TerminalArguments;
            command = new(path, [..prefix, command.FileName, ..command.Arguments]);
        }
        Start(command.CreateStartInfo(capture: false));
    }

    public static void Edit(string path, AppSettings settings)
    {
        var executable = string.IsNullOrWhiteSpace(settings.EditorPath)
            ? Path.Combine(Environment.SystemDirectory, "notepad.exe") : settings.EditorPath;
        Start(new Command(executable, [..settings.EditorArguments, path]).CreateStartInfo(capture: false));
    }

    public static void Open(string path) => Start(new ProcessStartInfo(path) { UseShellExecute = true });
    private static void Start(ProcessStartInfo info)
    {
        using var process = Process.Start(info) ?? throw new InvalidOperationException($"Could not start {info.FileName}.");
    }

    public static async Task RunElevatedAsync(Command command)
    {
        var info = command.CreateStartInfo(capture: false);
        info.Verb = "runas";
        using var process = Process.Start(info) ?? throw new InvalidOperationException("Could not start the elevated command.");
        await process.WaitForExitAsync();
        if (process.ExitCode != 0)
            throw new InvalidOperationException($"The elevated WSL command failed (exit code {process.ExitCode}). Verify the disk is offline and not in use by Windows. Run 'wsl --help' in an administrator terminal for available options.");
    }

    public static void MapDrive(string distro, string drive, bool persistent)
    {
        if (drive.Length != 2 || drive[1] != ':' || drive[0] is < 'D' or > 'Z')
            throw new ArgumentException("Choose a drive letter from D: through Z:.");
        var resource = new NetResource { Type = 1, LocalName = drive, RemoteName = WslCommands.NetworkPath(distro) };
        var result = WNetAddConnection2(ref resource, null, null, persistent ? 1 : 0);
        if (result != 0) throw new Win32Exception(result);
    }

    public static void UnmapDrive(string distro, string drive)
    {
        var length = 1024;
        var remote = new StringBuilder(length);
        var result = WNetGetConnection(drive, remote, ref length);
        if (result != 0) throw new Win32Exception(result);
        if (!string.Equals(remote.ToString().TrimEnd('\\'), WslCommands.NetworkPath(distro), StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("This drive letter is not mapped to the selected distribution.");
        result = WNetCancelConnection2(drive, 1, false);
        if (result != 0) throw new Win32Exception(result);
    }

    public static string? DistroLocation(string name)
    {
        using var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Lxss");
        if (key is null) return null;
        foreach (var child in key.GetSubKeyNames())
        {
            using var distro = key.OpenSubKey(child);
            if (string.Equals(distro?.GetValue("DistributionName") as string, name, StringComparison.OrdinalIgnoreCase))
                return distro?.GetValue("BasePath") as string;
        }
        return null;
    }

    public static void CreateShortcut(string distro, string target)
    {
        var link = (IShellLink)new ShellLink();
        try
        {
            link.SetPath(WslCommands.Executable);
            link.SetArguments("--distribution " + QuoteArgument(WslCommands.DistroName(distro)) + " --cd ~");
            link.SetDescription("WSL - " + distro);
            link.SetIconLocation(Environment.ProcessPath!, 0);
            ((IPersistFile)link).Save(target, false);
        }
        finally { Marshal.FinalReleaseComObject(link); }
    }

    public static string QuoteArgument(string value)
    {
        var output = new StringBuilder("\"");
        var slashes = 0;
        foreach (var c in value)
        {
            if (c == '\\') { slashes++; continue; }
            output.Append('\\', c == '"' ? slashes * 2 + 1 : slashes).Append(c);
            slashes = 0;
        }
        return output.Append('\\', slashes * 2).Append('"').ToString();
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct NetResource
    {
        public int Scope, Type, DisplayType, Usage;
        public string? LocalName, RemoteName, Comment, Provider;
    }
    [DllImport("mpr.dll", CharSet = CharSet.Unicode)]
    private static extern int WNetAddConnection2(ref NetResource resource, string? password, string? username, int flags);
    [DllImport("mpr.dll", CharSet = CharSet.Unicode)]
    private static extern int WNetCancelConnection2(string name, int flags, bool force);
    [DllImport("mpr.dll", CharSet = CharSet.Unicode)]
    private static extern int WNetGetConnection(string name, StringBuilder remoteName, ref int length);
}

public sealed class RegistryChangeWatcher : IDisposable
{
    private readonly EventWaitHandle stop = new(false, EventResetMode.ManualReset);
    private readonly Task worker;
    public RegistryChangeWatcher(Action changed)
    {
        worker = Task.Factory.StartNew(() => Watch(changed), CancellationToken.None,
            TaskCreationOptions.LongRunning, TaskScheduler.Default);
    }
    private void Watch(Action changed)
    {
        // The parent exists even when no distribution has ever been registered.
        using var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion");
        if (key is null) return;
        using var signal = new EventWaitHandle(false, EventResetMode.AutoReset);
        while (!stop.WaitOne(0))
        {
            if (RegNotifyChangeKeyValue(key.Handle, true, 1 | 4, signal.SafeWaitHandle, true) != 0) return;
            if (WaitHandle.WaitAny([stop, signal]) == 0) return;
            changed();
        }
    }
    public void Dispose()
    {
        stop.Set();
        worker.GetAwaiter().GetResult();
        stop.Dispose();
    }
    [DllImport("advapi32.dll")]
    private static extern int RegNotifyChangeKeyValue(Microsoft.Win32.SafeHandles.SafeRegistryHandle key,
        bool watchSubtree, uint filter, Microsoft.Win32.SafeHandles.SafeWaitHandle signal, bool asynchronous);
}
