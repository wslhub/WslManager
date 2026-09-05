using System.Text.Json;
using System.Text.Json.Serialization;

namespace WslManager.Core;

public enum TerminalKind { SystemConsole, WindowsTerminal, Custom }

public sealed class AppSettings
{
    public double Width { get; set; } = 1040;
    public double Height { get; set; } = 680;
    public double? Left { get; set; }
    public double? Top { get; set; }
    public bool Maximized { get; set; }
    public string SortColumn { get; set; } = nameof(Distro.Name);
    public bool SortDescending { get; set; }
    public List<ColumnSettings> Columns { get; set; } = [];
    public TerminalKind Terminal { get; set; }
    public string TerminalPath { get; set; } = "";
    public string[] TerminalArguments { get; set; } = [];
    public string EditorPath { get; set; } = "";
    public string[] EditorArguments { get; set; } = [];
    public bool WarnBeforeImport { get; set; } = true;

    public void Normalize()
    {
        Width = double.IsFinite(Width) ? Math.Clamp(Width, 760, 10000) : 1040;
        Height = double.IsFinite(Height) ? Math.Clamp(Height, 480, 10000) : 680;
        if (Left.HasValue && !double.IsFinite(Left.Value)) Left = null;
        if (Top.HasValue && !double.IsFinite(Top.Value)) Top = null;
        if (!Enum.IsDefined(Terminal)) Terminal = TerminalKind.SystemConsole;
        if (SortColumn is not (nameof(Distro.Name) or nameof(Distro.State) or nameof(Distro.Version) or nameof(Distro.IsDefault)))
            SortColumn = nameof(Distro.Name);
        Columns ??= [];
        Columns = Columns.Where(c => c is not null && double.IsFinite(c.Width) && c.Width is >= 40 and <= 4000).ToList();
        TerminalPath ??= "";
        EditorPath ??= "";
        TerminalArguments = (TerminalArguments ?? []).Where(a => a is not null).ToArray();
        EditorArguments = (EditorArguments ?? []).Where(a => a is not null).ToArray();
    }
}

public sealed record ColumnSettings(string Name, double Width, int DisplayIndex);
public sealed record SettingsLoadResult(AppSettings Settings, string? Warning = null);

public sealed class SettingsStore(string path)
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };
    public string FilePath { get; } = Path.GetFullPath(path);
    public static string DefaultPath(string applicationDirectory)
        => File.Exists(Path.Combine(applicationDirectory, "portable.flag"))
            ? Path.Combine(applicationDirectory, "settings.json")
            : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "WslManager", "settings.json");

    public SettingsLoadResult Load()
    {
        if (!File.Exists(FilePath)) return new(new());
        try
        {
            var settings = JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(FilePath), Options)
                ?? throw new JsonException("The settings document is empty.");
            settings.Normalize();
            return new(settings);
        }
        catch (Exception ex) when (ex is JsonException or IOException or UnauthorizedAccessException)
        {
            // Preserve the original bytes before a subsequent save replaces invalid JSON.
            var backup = FilePath + $".invalid-{DateTime.UtcNow:yyyyMMddHHmmssfff}";
            try { File.Copy(FilePath, backup); }
            catch (Exception backupError) when (backupError is IOException or UnauthorizedAccessException)
            { throw new IOException("Cannot preserve the unreadable settings file. Correct its permissions before starting WslManager.", backupError); }
            return new(new(), $"Could not load settings. Original saved to {backup}.\n{ex.Message}");
        }
    }

    public void Save(AppSettings settings)
    {
        settings.Normalize();
        Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
        var temporary = FilePath + "." + Guid.NewGuid().ToString("N") + ".tmp";
        try
        {
            File.WriteAllText(temporary, JsonSerializer.Serialize(settings, Options));
            File.Move(temporary, FilePath, overwrite: true);
        }
        finally { if (File.Exists(temporary)) File.Delete(temporary); }
    }
}
