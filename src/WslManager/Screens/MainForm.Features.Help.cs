using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace WslManager.Screens
{
    partial class MainForm
    {
        private void Feature_OpenWslHelp(object sender, EventArgs e)
            => Process.Start(new ProcessStartInfo("https://docs.microsoft.com/en-us/windows/wsl/") { UseShellExecute = true, });

        private void Feature_OpenGnuLinuxHelp(object sender, EventArgs e)
            => Process.Start(new ProcessStartInfo("https://www.debian.org/doc/manuals/debian-reference/ch01.en.html") { UseShellExecute = true, });

        private void Feature_OpenGeneralFaqHelp(object sender, EventArgs e)
            => Process.Start(new ProcessStartInfo("https://docs.microsoft.com/en-us/windows/wsl/faq") { UseShellExecute = true, });

        private void Feature_OpenWSLV2FaqHelp(object sender, EventArgs e)
            => Process.Start(new ProcessStartInfo("https://docs.microsoft.com/en-us/windows/wsl/wsl2-faq") { UseShellExecute = true, });

        private void Feature_OpenWslTroubleshoot(object sender, EventArgs e)
            => Process.Start(new ProcessStartInfo("https://docs.microsoft.com/en-us/windows/wsl/troubleshooting") { UseShellExecute = true, });

        private void Feature_OpenGlobalWsl2ConfigOptionHelp(object sender, EventArgs e)
            => Process.Start(new ProcessStartInfo("https://docs.microsoft.com/en-us/windows/wsl/wsl-config#configure-global-options-with-wslconfig") { UseShellExecute = true, });

        private void Feature_AboutApp(object sender, EventArgs e)
        {
            var message = string.Join(Environment.NewLine,
                "WSL Manager v0.1",
                "(c) 2019 rkttu.com, All rights reserved.");

            MessageBox.Show(this, message, Text,
                MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
        }
    }
}
