using System;
using System.Windows.Forms;
using WslManager.Extensions;

namespace WslManager.Screens
{
    // Features
    partial class MainForm
    {
        private void Feature_ShutdownWsl(object sender, EventArgs e)
        {
            if (MessageBox.Show(this, $"Really shutdown WSL entirely? This operation can cause unintentional data loss.",
                    Text, MessageBoxButtons.YesNo, MessageBoxIcon.Warning,
                    MessageBoxDefaultButton.Button2) != DialogResult.Yes)
                return;

            var process = WslHelpers.CreateShutdownDistroProcess();
            process.Start();
            process.WaitForExit();
            AppContext.RefreshDistroList();
        }

        private void Feature_AboutApp(object sender, EventArgs e)
        {
            var message = string.Join(Environment.NewLine,
                "WSL Manager v0.1",
                "(c) 2019 rkttu.com, All rights reserved.");

            MessageBox.Show(this, message, Text,
                MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
        }

        private void Feature_ExitApp(object sender, EventArgs e)
        {
            Close();
        }
    }
}
