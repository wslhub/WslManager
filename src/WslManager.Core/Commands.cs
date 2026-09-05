using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

namespace WslManager.Core;

public sealed record Command(string FileName, IReadOnlyList<string> Arguments, bool LinuxOutput = false)
{
    public ProcessStartInfo CreateStartInfo(bool capture = true)
    {
        var info = new ProcessStartInfo(FileName)
        {
            UseShellExecute = !capture,
            CreateNoWindow = capture,
            RedirectStandardOutput = capture,
            RedirectStandardError = capture,
        };
        if (capture)
        {
            info.StandardOutputEncoding = LinuxOutput ? Encoding.UTF8 : Encoding.Unicode;
            info.StandardErrorEncoding = info.StandardOutputEncoding;
        }
        foreach (var argument in Arguments)
            info.ArgumentList.Add(argument);
        return info;
    }
}

public sealed record CommandResult(int ExitCode, string Output, string Error)
{
    public string EnsureSuccess()
    {
        if (ExitCode != 0)
            throw new InvalidOperationException($"Command failed (exit code {ExitCode}).\n{Error}\n{Output}".Trim());
        return Output;
    }
}

public interface ICommandRunner
{
    Task<CommandResult> RunAsync(Command command, CancellationToken cancellationToken = default);
}

public sealed class ProcessRunner : ICommandRunner
{
    public async Task<CommandResult> RunAsync(Command command, CancellationToken cancellationToken = default)
    {
        using var process = new Process { StartInfo = command.CreateStartInfo() };
        if (!process.Start())
            throw new InvalidOperationException($"Could not start {command.FileName}.");
        // Drain both pipes concurrently; either pipe can fill before the child exits.
        var output = process.StandardOutput.ReadToEndAsync(cancellationToken);
        var error = process.StandardError.ReadToEndAsync(cancellationToken);
        try
        {
            await process.WaitForExitAsync(cancellationToken).ConfigureAwait(false);
            return new(process.ExitCode, await output.ConfigureAwait(false), await error.ConfigureAwait(false));
        }
        catch (OperationCanceledException)
        {
            if (!process.HasExited)
                process.Kill(entireProcessTree: true);
            await process.WaitForExitAsync(CancellationToken.None).ConfigureAwait(false);
            try { await Task.WhenAll(output, error).ConfigureAwait(false); }
            catch (OperationCanceledException) { }
            throw;
        }
    }
}

public static partial class WslCommands
{
    public static string Executable => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows),
        Environment.Is64BitOperatingSystem && !Environment.Is64BitProcess ? "Sysnative" : "System32", "wsl.exe");

    public static Command Create(params string[] arguments) => new(Executable, arguments);
    public static Command List() => Create("--list", "--verbose");
    public static Command Default(string distro) => Create("--set-default", DistroName(distro));
    public static Command Terminate(string distro) => Create("--terminate", DistroName(distro));
    public static Command Unregister(string distro) => Create("--unregister", DistroName(distro));
    public static Command Export(string distro, string archive) => Create("--export", DistroName(distro), FullPath(archive));
    public static Command Import(string distro, string directory, string archive, int version)
    {
        if (version is not (1 or 2)) throw new ArgumentException("WSL version must be 1 or 2.");
        if (!IsArchive(archive)) throw new ArgumentException("Select a .tar, .tar.gz or .tgz archive.");
        return Create("--import", DistroName(distro), FullPath(directory), FullPath(archive), "--version", version.ToString());
    }
    public static Command DefaultUser(string distro, string user)
        => Create("--manage", DistroName(distro), "--set-default-user", UserName(user));
    public static Command Linux(string distro, string user, params string[] command)
        => new(Executable, ["--distribution", DistroName(distro), "--user", UserName(user), "--exec", ..command], true);
    public static Command Launch(string distro, string? user = null, string? script = null)
    {
        var args = new List<string> { "--distribution", DistroName(distro), "--cd", "~" };
        if (!string.IsNullOrWhiteSpace(user)) args.AddRange(["--user", UserName(user)]);
        if (!string.IsNullOrWhiteSpace(script)) args.AddRange(["--exec", "/bin/sh", "-lc", script]);
        return new(Executable, args);
    }
    public static Command Mount(string disk, int? partition, bool vhd)
    {
        ValidateDisk(disk, vhd);
        if (partition is <= 0) throw new ArgumentException("Partition number must be positive.");
        var args = new List<string> { "--mount", disk };
        if (vhd) args.Add("--vhd");
        args.AddRange(["--type", "ext4"]);
        if (partition.HasValue) args.AddRange(["--partition", partition.Value.ToString()]);
        return Create([..args]);
    }
    public static Command Unmount(string disk, bool vhd)
    {
        ValidateDisk(disk, vhd);
        // Never allow an empty path, which would detach every disk.
        return Create("--unmount", disk);
    }
    private static void ValidateDisk(string disk, bool vhd)
    {
        if (vhd)
        {
            if (!Path.IsPathFullyQualified(disk) || !(disk.EndsWith(".vhd", StringComparison.OrdinalIgnoreCase)
                || disk.EndsWith(".vhdx", StringComparison.OrdinalIgnoreCase)))
                throw new ArgumentException("Select an absolute .vhd or .vhdx path.");
        }
        else if (!PhysicalDiskPattern().IsMatch(disk))
            throw new ArgumentException(@"Enter a physical disk path such as \\.\PHYSICALDRIVE2.");
    }
    public static string NetworkPath(string distro) => @"\\wsl$\" + DistroName(distro);
    public static string DistroName(string name)
    {
        if (string.IsNullOrWhiteSpace(name) || name != name.Trim() || name.StartsWith('-')
            || name.Any(c => char.IsControl(c) || "\\/:*?\"<>|".Contains(c)))
            throw new ArgumentException("Enter a distribution name without path separators or reserved characters.");
        return name;
    }
    public static string UserName(string user)
    {
        if (!UserPattern().IsMatch(user)) throw new ArgumentException("Enter a valid existing Linux username.");
        return user;
    }
    public static bool IsArchive(string path) => new[] { ".tar", ".tar.gz", ".tgz" }
        .Any(extension => path.EndsWith(extension, StringComparison.OrdinalIgnoreCase));
    private static string FullPath(string path)
    {
        if (string.IsNullOrWhiteSpace(path)) throw new ArgumentException("A file or directory path is required.");
        return Path.GetFullPath(path);
    }
    [GeneratedRegex(@"^\\\\\.\\PHYSICALDRIVE\d+$", RegexOptions.IgnoreCase)]
    private static partial Regex PhysicalDiskPattern();
    [GeneratedRegex(@"^[a-zA-Z_][a-zA-Z0-9_.-]*\$?$")]
    private static partial Regex UserPattern();
}
