using System.Windows;

namespace WslManager;

public static class Program
{
    [STAThread]
    public static void Main()
    {
        var app = new Application();
        try { app.Run(new MainWindow()); }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "WslManager startup error", MessageBoxButton.OK, MessageBoxImage.Error);
            Environment.ExitCode = 1;
        }
    }
}
