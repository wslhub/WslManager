using Microsoft.Win32;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;

namespace WslManager;

public sealed class FormDialog : Window
{
    private readonly StackPanel fields = new() { Margin = new Thickness(20) };
    public FormDialog(Window owner, string title, string submit = "OK")
    {
        Owner = owner;
        Title = title;
        Width = 560;
        SizeToContent = SizeToContent.Height;
        MaxHeight = Math.Max(400, SystemParameters.WorkArea.Height - 60);
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        ResizeMode = ResizeMode.NoResize;
        var layout = new DockPanel();
        var buttons = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(20, 0, 20, 20) };
        var ok = new Button { Content = submit, IsDefault = true, MinWidth = 100, Padding = new Thickness(12, 7, 12, 7) };
        AutomationProperties.SetAutomationId(ok, "DialogSubmit");
        ok.Click += (_, _) => DialogResult = true;
        var cancel = new Button { Content = "Cancel", IsCancel = true, MinWidth = 90, Margin = new Thickness(8, 0, 0, 0) };
        buttons.Children.Add(ok);
        buttons.Children.Add(cancel);
        DockPanel.SetDock(buttons, Dock.Bottom);
        layout.Children.Add(buttons);
        layout.Children.Add(new ScrollViewer { Content = fields, VerticalScrollBarVisibility = ScrollBarVisibility.Auto });
        Content = layout;
    }

    public void Note(string text) => fields.Children.Add(new TextBlock
    {
        Text = text, TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 0, 0, 16)
    });

    public TextBox TextField(string label, string value = "", bool multiline = false)
    {
        var box = new TextBox { Text = value, Padding = new Thickness(6), MinHeight = multiline ? 80 : 30,
            AcceptsReturn = multiline, TextWrapping = multiline ? TextWrapping.Wrap : TextWrapping.NoWrap,
            VerticalScrollBarVisibility = multiline ? ScrollBarVisibility.Auto : ScrollBarVisibility.Hidden };
        Field(label, box);
        return box;
    }
    public TextBox PathField(string label, string value = "", bool directory = false, string filter = "All files|*.*")
    {
        var box = new TextBox { Text = value, Padding = new Thickness(6) };
        var panel = new DockPanel();
        var browse = new Button { Content = "Browse…", Margin = new Thickness(8, 0, 0, 0), Padding = new Thickness(8, 4, 8, 4) };
        DockPanel.SetDock(browse, Dock.Right);
        browse.Click += (_, _) =>
        {
            if (directory)
            {
                var dialog = new OpenFolderDialog { Title = label };
                if (dialog.ShowDialog(this) == true) box.Text = dialog.FolderName;
            }
            else
            {
                var dialog = new OpenFileDialog { Title = label, Filter = filter };
                if (dialog.ShowDialog(this) == true) box.Text = dialog.FileName;
            }
        };
        panel.Children.Add(browse);
        panel.Children.Add(box);
        Field(label, panel);
        AutomationProperties.SetName(box, label);
        return box;
    }
    public ComboBox Choice(string label, IEnumerable<string> options, int index = 0)
    {
        var box = new ComboBox { ItemsSource = options, SelectedIndex = index, Padding = new Thickness(6) };
        Field(label, box);
        return box;
    }
    public CheckBox Check(string label, bool value)
    {
        var box = new CheckBox { Content = label, IsChecked = value, Margin = new Thickness(0, 0, 0, 16) };
        fields.Children.Add(box);
        return box;
    }
    private void Field(string label, FrameworkElement control)
    {
        fields.Children.Add(new Label { Content = label, Target = control });
        control.Margin = new Thickness(0, 0, 0, 12);
        AutomationProperties.SetName(control, label);
        fields.Children.Add(control);
    }
}
