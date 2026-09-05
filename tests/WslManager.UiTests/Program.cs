using System.IO;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using WslManager;
using WslManager.Core;

internal static class Program
{
    [STAThread]
    private static int Main(string[] args)
    {
        var output = Path.GetFullPath(args.FirstOrDefault() ?? "artifacts/ui");
        Directory.CreateDirectory(output);
        var settings = new SettingsStore(Path.Combine(output, "settings.json"));
        if (File.Exists(settings.FilePath)) File.Delete(settings.FilePath);
        var runner = new FakeRunner();
        var app = new Application { ShutdownMode = ShutdownMode.OnExplicitShutdown };
        var window = new MainWindow(runner, settings, observeSystem: false);
        var result = 0;
        var watchdog = new DispatcherTimer { Interval = TimeSpan.FromSeconds(45) };
        watchdog.Tick += (_, _) => { Console.Error.WriteLine("UI smoke test timed out."); Environment.Exit(1); };
        watchdog.Start();
        window.Loaded += async (_, _) =>
        {
            try
            {
                await window.Dispatcher.InvokeAsync(() => { }, DispatcherPriority.ApplicationIdle);
                var grid = Find<DataGrid>(window, "DistroGrid");
                Require(grid.Items.Count == 2, "Initial list contains two distributions");
                grid.SelectedIndex = 0;
                var original = grid.SelectedItem;
                runner.Running = true;
                await window.RefreshAsync();
                Require(ReferenceEquals(original, grid.SelectedItem), "Refresh preserves selected object");
                Require(((Distro)original).State == "Running", "Refresh updates running state");
                var search = Find<TextBox>(window, "DistroSearch");
                search.Text = "Debian";
                Require(grid.Items.Count == 1, "Search filters list");
                search.Text = "";
                runner.Fail = true;
                await window.RefreshAsync();
                Require(grid.Items.Count == 2, "Failed refresh retains list");
                Require(Find<TextBlock>(window, "Status").Text.Contains("Refresh failed"), "Failure is visible");
                runner.Fail = false;
                await window.RefreshAsync();
                window.Width = 1100;
                grid.Columns[0].Width = 380;
                grid.Columns[0].DisplayIndex = 1;
                await window.Dispatcher.InvokeAsync(() => { }, DispatcherPriority.ApplicationIdle);
                Capture(window, Path.Combine(output, "main-window.png"));
                // Exercise the actual settings button and modal dialog on the dispatcher.
                _ = window.Dispatcher.BeginInvoke(() =>
                {
                    var dialog = app.Windows.OfType<FormDialog>().Single();
                    Require(dialog.Title == "Settings", "Settings dialog opened");
                    Capture(dialog, Path.Combine(output, "settings-window.png"));
                    Find<Button>(dialog, "DialogSubmit").RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                }, DispatcherPriority.ApplicationIdle);
                Find<Button>(window, "SettingsButton").RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                window.Close();
                Require(File.Exists(settings.FilePath), "Window settings persisted");
                var saved = settings.Load().Settings;
                Require(saved.Width == 1100 && saved.Columns.Single(c => c.Name == "Name").DisplayIndex == 1, "Window size and column order persist");
                var reopened = new MainWindow(runner, settings, observeSystem: false);
                reopened.Show();
                await reopened.Dispatcher.InvokeAsync(() => { }, DispatcherPriority.ApplicationIdle);
                Require(reopened.Width == 1100, "Window size restored");
                Require(Find<DataGrid>(reopened, "DistroGrid").Columns[0].DisplayIndex == 1, "Column order restored");
                reopened.Close();
                File.WriteAllText(Path.Combine(output, "result.txt"), "PASS: WPF window, refresh, selection, filter, failure retention, settings dialog, save and reload. WSL commands used an injected fake runner.\n");
            }
            catch (Exception ex) { Console.Error.WriteLine(ex); result = 1; }
            finally { watchdog.Stop(); app.Shutdown(); }
        };
        window.Show();
        app.Run();
        return result;
    }
    private static void Require(bool condition, string description)
    {
        if (!condition) throw new InvalidOperationException(description);
        Console.WriteLine("PASS: " + description);
    }
    private static T Find<T>(DependencyObject parent, string id) where T : DependencyObject
    {
        if (parent is T value && AutomationProperties.GetAutomationId(parent) == id) return value;
        for (var i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            try { return Find<T>(VisualTreeHelper.GetChild(parent, i), id); }
            catch (KeyNotFoundException) { }
        }
        throw new KeyNotFoundException(id);
    }
    private static void Capture(Window window, string path)
    {
        window.UpdateLayout();
        var bitmap = new RenderTargetBitmap((int)window.ActualWidth, (int)window.ActualHeight, 96, 96, PixelFormats.Pbgra32);
        bitmap.Render(window);
        var png = new PngBitmapEncoder();
        png.Frames.Add(BitmapFrame.Create(bitmap));
        using var stream = File.Create(path);
        png.Save(stream);
    }
    private sealed class FakeRunner : ICommandRunner
    {
        public bool Running { get; set; }
        public bool Fail { get; set; }
        public Task<CommandResult> RunAsync(Command command, CancellationToken cancellationToken = default)
        {
            if (!command.Arguments.SequenceEqual(new[] { "--list", "--verbose" }))
                throw new InvalidOperationException("Unexpected command in UI smoke test.");
            return Task.FromResult(Fail ? new CommandResult(1, "", "Simulated WSL failure")
                : new CommandResult(0, $"  NAME       STATE       VERSION\n* Ubuntu     {(Running ? "Running" : "Stopped")}     2\n  Debian     Stopped     2", ""));
        }
    }
}
