using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using WslManager.Core;

namespace WslManager;

public sealed partial class MainWindow : Window
{
    private readonly WslService service;
    private readonly SettingsStore settingsStore;
    private readonly AppSettings settings;
    private readonly ObservableCollection<Distro> distros = [];
    private readonly DataGrid grid = new();
    private readonly TextBlock status = new() { Text = "Loading distributions…", TextWrapping = TextWrapping.Wrap };
    private readonly TextBlock count = new();
    private readonly DockPanel actions = new();
    private readonly TextBox search = new();
    private readonly CancellationTokenSource lifetime = new();
    private readonly DispatcherTimer debounce = new() { Interval = TimeSpan.FromMilliseconds(350) };
    private readonly DispatcherTimer stateTimer = new() { Interval = TimeSpan.FromSeconds(30) };
    private RegistryChangeWatcher? watcher;
    private bool busy, refreshing, refreshAgain, closed;
    private readonly bool observeSystem;

    public MainWindow(ICommandRunner? runner = null, SettingsStore? store = null, bool observeSystem = true)
    {
        service = new(runner ?? new ProcessRunner());
        settingsStore = store ?? new SettingsStore(SettingsStore.DefaultPath(AppContext.BaseDirectory));
        var loaded = settingsStore.Load();
        settings = loaded.Settings;
        this.observeSystem = observeSystem;
        Title = "WslManager";
        Width = settings.Width;
        Height = settings.Height;
        MinWidth = 760;
        MinHeight = 480;
        WindowStartupLocation = WindowStartupLocation.CenterScreen;
        FontFamily = new FontFamily("Segoe UI");
        FontSize = 14;
        Background = Brushes.White;
        Icon = System.Windows.Media.Imaging.BitmapFrame.Create(new Uri("pack://application:,,,/WslManager;component/App.ico"));
        RestorePosition();
        BuildWindow();
        Loaded += async (_, _) =>
        {
            if (loaded.Warning is not null) MessageBox.Show(this, loaded.Warning, Title, MessageBoxButton.OK, MessageBoxImage.Warning);
            await RefreshAsync();
            if (closed) return;
            if (observeSystem)
            {
                watcher = new RegistryChangeWatcher(() =>
                {
                    if (!Dispatcher.HasShutdownStarted)
                        Dispatcher.BeginInvoke(() => { if (!closed) { debounce.Stop(); debounce.Start(); } });
                });
                stateTimer.Start();
            }
        };
        debounce.Tick += async (_, _) => { debounce.Stop(); await RefreshAsync(); };
        stateTimer.Tick += async (_, _) => { if (IsActive && !busy) await RefreshAsync(); };
        Activated += (_, _) => { if (IsLoaded && observeSystem) { debounce.Stop(); debounce.Start(); } };
        Closing += OnClosing;
        Closed += (_, _) =>
        {
            closed = true;
            debounce.Stop();
            stateTimer.Stop();
            watcher?.Dispose();
            lifetime.Cancel();
        };
    }

    private void BuildWindow()
    {
        var root = new DockPanel { LastChildFill = true };
        var menu = new Menu { Background = new SolidColorBrush(Color.FromRgb(244, 246, 249)), Padding = new Thickness(8, 4, 8, 4) };
        var file = AddMenu(menu, "_File");
        AddItem(file, "_Install distribution…", "Install", InstallAsync);
        AddItem(file, "_Import archive…", "Import", () => ImportAsync());
        file.Items.Add(new Separator());
        AddItem(file, "_Settings…", "Settings", SettingsAsync, false);
        AddItem(file, "E_xit", "Exit", () => { Close(); return Task.CompletedTask; }, false);
        var distro = AddMenu(menu, "_Distribution");
        AddDistroItems(distro.Items);
        var tools = AddMenu(menu, "_Tools");
        AddItem(tools, "Edit global .wslconfig", "EditConfig", EditConfigAsync, false);
        AddItem(tools, "Mount ext4 disk…", "Mount", () => DiskAsync(false));
        AddItem(tools, "Unmount disk…", "Unmount", () => DiskAsync(true));
        AddItem(tools, "Shut down WSL…", "Shutdown", ShutdownAsync);
        var help = AddMenu(menu, "_Help");
        AddItem(help, "WSL documentation", "Help", () => { WindowsIntegration.Open("https://learn.microsoft.com/windows/wsl/"); return Task.CompletedTask; }, false);
        AddItem(help, "About WslManager", "About", () => { MessageBox.Show(this, "WslManager 0.2\nWSLHub contributors\nMIT License", Title); return Task.CompletedTask; }, false);
        DockPanel.SetDock(menu, Dock.Top);
        actions.Children.Add(menu);
        var toolbar = new WrapPanel { Margin = new Thickness(20, 12, 20, 12) };
        Button(toolbar, "Open terminal", "LaunchButton", LaunchAsync);
        Button(toolbar, "Open files", "FilesButton", FilesAsync);
        Button(toolbar, "Import…", "ImportButton", () => ImportAsync());
        Button(toolbar, "Refresh", "RefreshButton", RefreshAsync, false);
        Button(toolbar, "Settings", "SettingsButton", SettingsAsync, false);
        actions.Children.Add(toolbar);
        DockPanel.SetDock(actions, Dock.Top);
        root.Children.Add(actions);

        var heading = new StackPanel { Margin = new Thickness(24, 8, 24, 16) };
        heading.Children.Add(new TextBlock { Text = "Linux distributions", FontSize = 28, FontWeight = FontWeights.SemiBold });
        count.Margin = new Thickness(0, 5, 0, 14);
        count.Foreground = Brushes.DimGray;
        heading.Children.Add(count);
        var searchLabel = new Label { Content = "_Find distribution", Target = search, Padding = new Thickness(0, 0, 0, 5) };
        heading.Children.Add(searchLabel);
        search.Padding = new Thickness(8);
        search.MaxWidth = 480;
        search.HorizontalAlignment = HorizontalAlignment.Left;
        search.MinWidth = 280;
        AutomationProperties.SetAutomationId(search, "DistroSearch");
        search.TextChanged += (_, _) => CollectionViewSource.GetDefaultView(distros).Refresh();
        heading.Children.Add(search);
        DockPanel.SetDock(heading, Dock.Top);
        root.Children.Add(heading);

        var footer = new Border { Background = new SolidColorBrush(Color.FromRgb(244, 246, 249)), Padding = new Thickness(24, 12, 24, 12), Child = status };
        AutomationProperties.SetAutomationId(status, "Status");
        DockPanel.SetDock(footer, Dock.Bottom);
        root.Children.Add(footer);
        grid.Margin = new Thickness(24, 0, 24, 20);
        grid.AutoGenerateColumns = false;
        grid.IsReadOnly = true;
        grid.CanUserAddRows = false;
        grid.CanUserDeleteRows = false;
        grid.SelectionMode = DataGridSelectionMode.Single;
        grid.HeadersVisibility = DataGridHeadersVisibility.Column;
        grid.RowHeight = 38;
        grid.GridLinesVisibility = DataGridGridLinesVisibility.Horizontal;
        grid.HorizontalGridLinesBrush = new SolidColorBrush(Color.FromRgb(235, 238, 242));
        grid.AlternatingRowBackground = new SolidColorBrush(Color.FromRgb(249, 250, 252));
        grid.BorderBrush = new SolidColorBrush(Color.FromRgb(220, 225, 232));
        grid.ItemsSource = distros;
        AutomationProperties.SetAutomationId(grid, "DistroGrid");
        grid.Columns.Add(Column("Name", nameof(Distro.Name), 340));
        grid.Columns.Add(Column("State", nameof(Distro.State), 180));
        grid.Columns.Add(Column("WSL version", nameof(Distro.Version), 120));
        grid.Columns.Add(new DataGridCheckBoxColumn { Header = "Default", Binding = new Binding(nameof(Distro.IsDefault)), SortMemberPath = nameof(Distro.IsDefault), Width = 90 });
        foreach (var saved in settings.Columns.OrderBy(c => c.DisplayIndex))
        {
            var column = grid.Columns.FirstOrDefault(c => c.SortMemberPath == saved.Name);
            if (column is null) continue;
            column.Width = saved.Width;
            column.DisplayIndex = Math.Clamp(saved.DisplayIndex, 0, grid.Columns.Count - 1);
        }
        var view = CollectionViewSource.GetDefaultView(distros);
        view.Filter = item => item is Distro d && d.Name.Contains(search.Text, StringComparison.OrdinalIgnoreCase);
        var direction = settings.SortDescending ? ListSortDirection.Descending : ListSortDirection.Ascending;
        view.SortDescriptions.Add(new(settings.SortColumn, direction));
        grid.Columns.First(c => c.SortMemberPath == settings.SortColumn).SortDirection = direction;
        grid.Sorting += (_, e) =>
        {
            settings.SortColumn = e.Column.SortMemberPath;
            settings.SortDescending = e.Column.SortDirection == ListSortDirection.Ascending;
        };
        var context = new ContextMenu();
        AddDistroItems(context.Items);
        grid.ContextMenu = context;
        grid.PreviewMouseRightButtonDown += (_, e) =>
        {
            var row = FindParent<DataGridRow>(e.OriginalSource as DependencyObject);
            if (row is not null) grid.SelectedItem = row.Item;
        };
        grid.MouseDoubleClick += async (_, e) =>
        {
            if (FindParent<DataGridRow>(e.OriginalSource as DependencyObject) is not null)
                await RunUiAsync("Open terminal", LaunchAsync);
        };
        grid.AllowDrop = true;
        AllowDrop = true;
        PreviewDragOver += (_, e) =>
        {
            e.Effects = !busy && ArchiveDrop(e.Data) is not null ? DragDropEffects.Copy : DragDropEffects.None;
            e.Handled = true;
        };
        Drop += async (_, e) =>
        {
            e.Handled = true;
            if (ArchiveDrop(e.Data) is string path) await RunUiAsync("Import archive", () => ImportAsync(path));
        };
        InputBindings.Add(new KeyBinding(new ActionCommand(() => _ = RunUiAsync("Refresh", RefreshAsync, false)), Key.F5, ModifierKeys.None));
        root.Children.Add(grid);
        Content = root;
    }

    private static DataGridTextColumn Column(string label, string property, double width)
        => new() { Header = label, Binding = new Binding(property), SortMemberPath = property, Width = width };
    private static T? FindParent<T>(DependencyObject? current) where T : DependencyObject
    {
        while (current is not null) { if (current is T match) return match; current = VisualTreeHelper.GetParent(current); }
        return null;
    }
    private static string? ArchiveDrop(IDataObject data)
        => data.GetData(DataFormats.FileDrop) is string[] { Length: 1 } files && WslCommands.IsArchive(files[0]) ? files[0] : null;
    private static MenuItem AddMenu(Menu parent, string title)
    {
        var item = new MenuItem { Header = title };
        parent.Items.Add(item);
        return item;
    }
    private void AddItem(MenuItem parent, string text, string id, Func<Task> action, bool refresh = true)
        => AddItem(parent.Items, text, id, action, refresh);
    private void AddItem(ItemCollection parent, string text, string id, Func<Task> action, bool refresh = true)
    {
        var item = new MenuItem { Header = text };
        AutomationProperties.SetAutomationId(item, id);
        if (id == "Exit") item.Click += (_, _) => { if (!busy) Close(); };
        else item.Click += async (_, _) => await RunUiAsync(text.Replace("_", ""), action, refresh);
        parent.Add(item);
    }
    private void Button(Panel parent, string text, string id, Func<Task> action, bool refresh = true)
    {
        var button = new Button { Content = text, Padding = new Thickness(14, 8, 14, 8), Margin = new Thickness(0, 0, 8, 0) };
        AutomationProperties.SetAutomationId(button, id);
        button.Click += async (_, _) => await RunUiAsync(text, action, refresh);
        parent.Children.Add(button);
    }
    private void AddDistroItems(ItemCollection items)
    {
        AddItem(items, "Open terminal", "Launch", LaunchAsync);
        AddItem(items, "Run as user / run script…", "RunAs", RunAsAsync);
        AddItem(items, "Open files", "Files", FilesAsync);
        AddItem(items, "Set as default distribution", "Default", () => service.ExecuteAsync(WslCommands.Default(Selected().Name)));
        AddItem(items, "Change default user…", "DefaultUser", DefaultUserAsync);
        AddItem(items, "Export archive…", "Export", ExportAsync);
        AddItem(items, "Create shortcut…", "Shortcut", ShortcutAsync, false);
        AddItem(items, "Map network drive…", "MapDrive", () => DriveAsync(false), false);
        AddItem(items, "Disconnect mapped drive…", "UnmapDrive", () => DriveAsync(true), false);
        AddItem(items, "Properties / Linux users…", "Properties", PropertiesAsync, false);
        items.Add(new Separator());
        AddItem(items, "Terminate…", "Terminate", TerminateAsync);
        AddItem(items, "Unregister and delete data…", "Unregister", UnregisterAsync);
    }

    private Distro Selected() => grid.SelectedItem as Distro ?? throw new InvalidOperationException("Select a distribution first.");

    private async Task RunUiAsync(string label, Func<Task> action, bool refresh = true)
    {
        if (busy || closed) return;
        busy = true;
        actions.IsEnabled = false;
        status.Text = label + "…";
        try
        {
            await action();
            if (closed) return;
            if (refresh) await RefreshAsync();
            if (!status.Text.StartsWith("Refresh failed", StringComparison.Ordinal)) status.Text = "Ready.";
        }
        catch (OperationCanceledException) when (closed) { }
        catch (Exception ex)
        {
            status.Text = ex.Message;
            if (!closed) MessageBox.Show(this, ex.Message, label, MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally { busy = false; actions.IsEnabled = true; }
    }

    public async Task RefreshAsync()
    {
        if (closed) return;
        if (refreshing) { refreshAgain = true; return; }
        refreshing = true;
        try
        {
            do
            {
                refreshAgain = false;
                var updated = await service.ListAsync(lifetime.Token);
                if (closed) return;
                var selected = (grid.SelectedItem as Distro)?.Name;
                DistroParser.Synchronize(distros, updated);
                if (selected is not null) grid.SelectedItem = distros.FirstOrDefault(d => d.Name == selected);
                CollectionViewSource.GetDefaultView(distros).Refresh();
                count.Text = $"{distros.Count} registered distribution(s). Select a row to manage it.";
                if (!busy) status.Text = distros.Count == 0 ? "No distributions registered. Use Install or Import to add one." : "Ready.";
            } while (refreshAgain && !closed);
        }
        catch (OperationCanceledException) when (closed) { }
        catch (Exception ex) { if (!closed) status.Text = "Refresh failed. The last successful list is retained. " + ex.Message; }
        finally { refreshing = false; }
    }

    private void RestorePosition()
    {
        if (settings.Left is double left && settings.Top is double top)
        {
            var saved = new Rect(left, top, Width, Height);
            var desktop = new Rect(SystemParameters.VirtualScreenLeft, SystemParameters.VirtualScreenTop,
                SystemParameters.VirtualScreenWidth, SystemParameters.VirtualScreenHeight);
            var visible = Rect.Intersect(saved, desktop);
            if (visible.Width >= 100 && visible.Height >= 60)
            {
                WindowStartupLocation = WindowStartupLocation.Manual;
                Left = Math.Clamp(left, desktop.Left, Math.Max(desktop.Left, desktop.Right - 100));
                Top = Math.Clamp(top, desktop.Top, Math.Max(desktop.Top, desktop.Bottom - 60));
            }
        }
        if (settings.Maximized) WindowState = WindowState.Maximized;
    }
    private void OnClosing(object? sender, CancelEventArgs e)
    {
        if (busy) { e.Cancel = true; status.Text = "Wait for the current operation to finish before closing."; return; }
        var bounds = WindowState == WindowState.Normal ? new Rect(Left, Top, ActualWidth, ActualHeight) : RestoreBounds;
        settings.Left = bounds.Left;
        settings.Top = bounds.Top;
        settings.Width = bounds.Width;
        settings.Height = bounds.Height;
        settings.Maximized = WindowState == WindowState.Maximized;
        settings.Columns = grid.Columns.Select(c => new ColumnSettings(c.SortMemberPath, c.ActualWidth, c.DisplayIndex)).ToList();
        try { settingsStore.Save(settings); }
        catch (Exception ex) { MessageBox.Show(this, "Could not save settings. " + ex.Message, Title, MessageBoxButton.OK, MessageBoxImage.Warning); }
    }
    private sealed class ActionCommand(Action action) : ICommand
    {
        public event EventHandler? CanExecuteChanged { add { } remove { } }
        public bool CanExecute(object? parameter) => true;
        public void Execute(object? parameter) => action();
    }
}
