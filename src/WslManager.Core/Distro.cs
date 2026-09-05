using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace WslManager.Core;

public sealed class Distro : INotifyPropertyChanged
{
    public required string Name { get; init; }
    private string state = "";
    public string State { get => state; set { state = value; Changed(); } }
    private int version;
    public int Version { get => version; set { version = value; Changed(); } }
    private bool isDefault;
    public bool IsDefault { get => isDefault; set { isDefault = value; Changed(); } }
    public event PropertyChangedEventHandler? PropertyChanged;
    private void Changed([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new(name));
}

public static partial class DistroParser
{
    public static IReadOnlyList<Distro> Parse(string output)
    {
        var result = new List<Distro>();
        foreach (var line in output.Replace("\0", "").TrimStart('\uFEFF').Split('\n'))
        {
            var match = RowPattern().Match(line.TrimEnd('\r'));
            // Headers and the localized "no distributions" message cannot end with a WSL version.
            if (!match.Success) continue;
            result.Add(new Distro
            {
                Name = match.Groups["name"].Value.Trim(),
                State = match.Groups["state"].Value.Trim(),
                Version = int.Parse(match.Groups["version"].Value),
                IsDefault = match.Groups["default"].Success
            });
        }
        return result;
    }
    [GeneratedRegex(@"^\s*(?<default>\*)?\s*(?<name>\S.*?)\s{2,}(?<state>\S.*?)\s+(?<version>[12])\s*$")]
    private static partial Regex RowPattern();

    public static void Synchronize(IList<Distro> target, IReadOnlyList<Distro> source)
    {
        var byName = source.ToDictionary(x => x.Name, StringComparer.OrdinalIgnoreCase);
        for (var i = target.Count - 1; i >= 0; i--)
        {
            if (!byName.Remove(target[i].Name, out var next)) { target.RemoveAt(i); continue; }
            if (target[i].State != next.State) target[i].State = next.State;
            if (target[i].Version != next.Version) target[i].Version = next.Version;
            if (target[i].IsDefault != next.IsDefault) target[i].IsDefault = next.IsDefault;
        }
        foreach (var next in source)
            if (byName.ContainsKey(next.Name)) target.Add(next);
    }
}

public sealed class WslService(ICommandRunner runner)
{
    public async Task<IReadOnlyList<Distro>> ListAsync(CancellationToken cancellationToken = default)
    {
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(TimeSpan.FromSeconds(20));
        var result = await runner.RunAsync(WslCommands.List(), timeout.Token);
        return DistroParser.Parse(result.EnsureSuccess());
    }
    public async Task<string> ExecuteAsync(Command command, CancellationToken cancellationToken = default)
        => (await runner.RunAsync(command, cancellationToken)).EnsureSuccess();

    public async Task SetDefaultUserAsync(string distro, string user, CancellationToken cancellationToken = default)
    {
        // Check the account before changing the distribution's default login.
        await ExecuteAsync(WslCommands.Linux(distro, user, "id", "-u"), cancellationToken);
        var help = await ExecuteAsync(WslCommands.Create("--help"), cancellationToken);
        if (!help.Contains("--set-default-user", StringComparison.Ordinal))
            throw new NotSupportedException("This WSL version does not support changing the default user. Update WSL with 'wsl --update', or configure [user] default in /etc/wsl.conf.");
        await ExecuteAsync(WslCommands.DefaultUser(distro, user), cancellationToken);
    }
}
