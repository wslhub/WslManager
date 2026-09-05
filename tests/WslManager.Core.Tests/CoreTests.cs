using System.Diagnostics;
using WslManager.Core;

namespace WslManager.Core.Tests;

public sealed class CoreTests
{
    [Theory]
    [InlineData("  NAME                   STATE           VERSION\r\n* Ubuntu Work            Running         2\r\n  Debian                 Stopped         1", "Ubuntu Work", "Running")]
    [InlineData("  이름                   상태            버전\r\n* 개발 환경              실행 중         2\r\n  Debian                 중지됨          1", "개발 환경", "실행 중")]
    [InlineData("\uFEFF  NAME  STATE  VERSION\n* Ubuntu Work  Running  2\n  Debian  Stopped  1\n", "Ubuntu Work", "Running")]
    public void ParsesLocalizedRowsAndNamesWithSpaces(string output, string name, string state)
    {
        var rows = DistroParser.Parse(output);
        Assert.Equal(2, rows.Count);
        Assert.Equal(name, rows[0].Name);
        Assert.Equal(state, rows[0].State);
        Assert.True(rows[0].IsDefault);
        Assert.Equal(2, rows[0].Version);
        Assert.False(rows[1].IsDefault);
    }
    [Fact]
    public void IgnoresLocalizedEmptyListAndHeaders()
    {
        Assert.Empty(DistroParser.Parse("Windows Subsystem for Linux has no installed distributions."));
        Assert.Empty(DistroParser.Parse("  이름     상태       버전"));
    }
    [Fact]
    public void RefreshPreservesObjectsAndNotifiesOnlyChanges()
    {
        var original = new Distro { Name = "Ubuntu", State = "Stopped", Version = 2 };
        var removed = new Distro { Name = "Old" };
        var target = new List<Distro> { original, removed };
        var notifications = new List<string?>();
        original.PropertyChanged += (_, e) => notifications.Add(e.PropertyName);
        DistroParser.Synchronize(target, [new() { Name = "Ubuntu", State = "Running", Version = 2, IsDefault = true }, new() { Name = "Debian", Version = 1 }]);
        Assert.Same(original, target[0]);
        Assert.DoesNotContain(removed, target);
        Assert.Equal([nameof(Distro.State), nameof(Distro.IsDefault)], notifications);
        Assert.Equal("Debian", target[1].Name);
    }
    [Fact]
    public void CommandsKeepPathsAndMetacharactersInSeparateArguments()
    {
        var directory = Path.Combine(Path.GetTempPath(), "WSL & archives");
        var archive = Path.Combine(directory, "my distro.tar.gz");
        var info = WslCommands.Import("Linux & dev", directory, archive, 2).CreateStartInfo();
        Assert.False(info.UseShellExecute);
        Assert.Equal("", info.Arguments);
        Assert.Equal(["--import", "Linux & dev", directory, archive, "--version", "2"], info.ArgumentList);
        Assert.DoesNotContain("cmd.exe", info.FileName);
        var script = "printf '%s' 'a; & $(whoami)'";
        Assert.Equal(script, WslCommands.Launch("Linux & dev", "root", script).Arguments[^1]);
    }
    [Theory]
    [InlineData("")]
    [InlineData("--all")]
    [InlineData("..\\outside")]
    [InlineData("bad\nname")]
    [InlineData(" bad")]
    public void RejectsInvalidDistroNames(string name) => Assert.Throws<ArgumentException>(() => WslCommands.Default(name));
    [Theory]
    [InlineData("x; id")]
    [InlineData("-root")]
    [InlineData("x\nroot")]
    public void RejectsUnsafeUserNames(string name) => Assert.Throws<ArgumentException>(() => WslCommands.DefaultUser("Ubuntu", name));
    [Theory]
    [InlineData("backup.TAR")]
    [InlineData("backup.tar.gz")]
    [InlineData("backup.tgz")]
    public void AcceptsSupportedArchives(string name) => Assert.True(WslCommands.IsArchive(name));
    [Fact]
    public void MountTargetsOneValidatedDisk()
    {
        var mount = WslCommands.Mount(@"\\.\PHYSICALDRIVE2", 3, false);
        Assert.Equal(["--mount", @"\\.\PHYSICALDRIVE2", "--type", "ext4", "--partition", "3"], mount.Arguments);
        Assert.Throws<ArgumentException>(() => WslCommands.Unmount("", false));
        Assert.Throws<ArgumentException>(() => WslCommands.Mount(@"\\.\PHYSICALDRIVE2", 0, false));
        Assert.Throws<ArgumentException>(() => WslCommands.Mount("--all", null, false));
    }
    [Fact]
    public async Task WslFailureDoesNotBecomeAnEmptySuccessfulList()
    {
        var service = new WslService(new FakeRunner(new CommandResult(5, "", "WSL unavailable")));
        var error = await Assert.ThrowsAsync<InvalidOperationException>(() => service.ListAsync());
        Assert.Contains("WSL unavailable", error.Message);
    }
    [Fact]
    public async Task DefaultUserChecksTheAccountAndCapabilityBeforeMutation()
    {
        var runner = new FakeRunner(new(0, "1000", ""), new(0, "--manage --set-default-user", ""), new(0, "", ""));
        await new WslService(runner).SetDefaultUserAsync("Imported Linux", "alice");
        Assert.Equal(["--distribution", "Imported Linux", "--user", "alice", "--exec", "id", "-u"], runner.Commands[0].Arguments);
        Assert.Equal(["--manage", "Imported Linux", "--set-default-user", "alice"], runner.Commands[2].Arguments);
    }
    [Fact]
    public async Task MissingUserPreventsDefaultChange()
    {
        var runner = new FakeRunner(new CommandResult(1, "", "User not found"));
        await Assert.ThrowsAsync<InvalidOperationException>(() => new WslService(runner).SetDefaultUserAsync("Ubuntu", "missing"));
        Assert.Single(runner.Commands);
    }
    [Fact]
    public async Task OldWslReportsCapabilityWithoutModifyingConfiguration()
    {
        var runner = new FakeRunner(new(0, "1000", ""), new(0, "--list --help", ""));
        await Assert.ThrowsAsync<NotSupportedException>(() => new WslService(runner).SetDefaultUserAsync("Ubuntu", "alice"));
        Assert.Equal(2, runner.Commands.Count);
    }
    [Fact]
    public void SettingsRoundTripAndCorruptionBackup()
    {
        using var temporary = new TemporaryDirectory();
        var store = new SettingsStore(Path.Combine(temporary.Path, "settings.json"));
        var settings = new AppSettings { Width = 1250, Left = -1000, WarnBeforeImport = false, Terminal = TerminalKind.Custom,
            TerminalPath = "terminal.exe", TerminalArguments = ["-e"], EditorPath = "editor.exe", EditorArguments = ["--wait"],
            SortColumn = nameof(Distro.Version), SortDescending = true, Columns = [new(nameof(Distro.State), 250, 0)] };
        store.Save(settings);
        var result = store.Load();
        Assert.Null(result.Warning);
        Assert.Equal(1250, result.Settings.Width);
        Assert.Equal(-1000, result.Settings.Left);
        Assert.False(result.Settings.WarnBeforeImport);
        Assert.Equal(["--wait"], result.Settings.EditorArguments);
        Assert.Equal(250, result.Settings.Columns[0].Width);
        const string broken = "{unreadable";
        File.WriteAllText(store.FilePath, broken);
        var recovered = store.Load();
        Assert.NotNull(recovered.Warning);
        Assert.True(recovered.Settings.WarnBeforeImport);
        Assert.Equal(broken, File.ReadAllText(Directory.GetFiles(temporary.Path, "*.invalid-*").Single()));
        store.Save(recovered.Settings);
        Assert.Empty(Directory.GetFiles(temporary.Path, "*.tmp"));
    }
    [Fact]
    public void NormalizesMalformedSettingsAndSupportsPortableMode()
    {
        using var temporary = new TemporaryDirectory();
        var store = new SettingsStore(Path.Combine(temporary.Path, "settings.json"));
        File.WriteAllText(store.FilePath, "{\"Width\":-1,\"Columns\":null,\"EditorArguments\":null,\"SortColumn\":\"Invalid\"}");
        var settings = store.Load().Settings;
        Assert.Equal(760, settings.Width);
        Assert.Empty(settings.Columns);
        Assert.Empty(settings.EditorArguments);
        Assert.Equal(nameof(Distro.Name), settings.SortColumn);
        File.WriteAllText(Path.Combine(temporary.Path, "portable.flag"), "");
        Assert.Equal(store.FilePath, SettingsStore.DefaultPath(temporary.Path));
    }
    [Fact]
    public async Task ProcessRunnerDrainsBothPipesAndPropagatesExitCode()
    {
        // Exercise real OS pipes with enough output to fill their buffers.
        Command command = OperatingSystem.IsWindows()
            ? new("powershell.exe", ["-NoProfile", "-Command", "[Console]::OutputEncoding=[Text.Encoding]::UTF8; 1..8000 | % { [Console]::Out.WriteLine('stdout'); [Console]::Error.WriteLine('stderr') }; exit 7"], true)
            : new("/bin/sh", ["-c", "i=0; while [ $i -lt 8000 ]; do echo stdout; echo stderr >&2; i=$((i+1)); done; exit 7"], true);
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        var result = await new ProcessRunner().RunAsync(command, timeout.Token);
        Assert.Equal(7, result.ExitCode);
        Assert.Contains("stdout", result.Output);
        Assert.Contains("stderr", result.Error);
    }
    [Fact]
    public async Task CancellationStopsAnUnresponsiveProcess()
    {
        Command command = OperatingSystem.IsWindows()
            ? new("powershell.exe", ["-NoProfile", "-Command", "Start-Sleep -Seconds 60"], true)
            : new("/bin/sh", ["-c", "sleep 60"], true);
        using var timeout = new CancellationTokenSource(TimeSpan.FromMilliseconds(300));
        var timer = Stopwatch.StartNew();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => new ProcessRunner().RunAsync(command, timeout.Token));
        Assert.True(timer.Elapsed < TimeSpan.FromSeconds(15));
    }
    private sealed class FakeRunner(params CommandResult[] results) : ICommandRunner
    {
        private readonly Queue<CommandResult> queue = new(results);
        public List<Command> Commands { get; } = [];
        public Task<CommandResult> RunAsync(Command command, CancellationToken cancellationToken = default)
        { Commands.Add(command); return Task.FromResult(queue.Dequeue()); }
    }
    private sealed class TemporaryDirectory : IDisposable
    {
        public string Path { get; } = Directory.CreateTempSubdirectory("wslmanager-tests-").FullName;
        public void Dispose() => Directory.Delete(Path, true);
    }
}
