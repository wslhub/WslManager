using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows.Forms;
using WslManager.Extensions;

namespace WslManager.Screens
{
    // Features
    partial class MainForm
    {
        private void Feature_EditWslConfiguration(object sender, EventArgs e)
        {
            var notepadPath = Path.Combine(Environment.SystemDirectory, "notepad.exe");

            if (!File.Exists(notepadPath))
            {
                MessageBox.Show(this, $"Cannot find notepad.exe for editing purpose.",
                    Text, MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                return;
            }

            var targetPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                ".wslconfig");

            if (!File.Exists(targetPath))
                File.WriteAllText(targetPath, @"", Encoding.ASCII);

            var notepadProcess = new Process()
            {
                StartInfo = new ProcessStartInfo(notepadPath, targetPath) { UseShellExecute = false, },
                EnableRaisingEvents = true,
            };

            notepadProcess.Exited += NotepadProcess_Exited;

            if (!notepadProcess.Start())
            {
                MessageBox.Show(this, $"Cannot start notepad.exe for editing purpose.",
                    Text, MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                return;
            }
        }

        private void NotepadProcess_Exited(object sender, EventArgs e)
        {
            if (InvokeRequired)
            {
                Invoke(new EventHandler(NotepadProcess_Exited), sender, e);
                return;
            }

            if (MessageBox.Show(this, $"To apply change, you need to shutdown the LXSS service. Please save all of the files before shutdown. Shtudown now?",
                Text, MessageBoxButtons.YesNo, MessageBoxIcon.Warning,
                MessageBoxDefaultButton.Button2) != DialogResult.Yes)
                return;

            var process = WslHelpers.CreateShutdownDistroProcess();
            process.Start();
            process.WaitForExit();
            AppContext.RefreshDistroList();
        }

        private void Feature_ShutdownWsl(object sender, EventArgs e)
        {
            if (MessageBox.Show(this, $"Really shutdown WSL entirely? Please save all of the files before shutdown.",
                Text, MessageBoxButtons.YesNo, MessageBoxIcon.Warning,
                MessageBoxDefaultButton.Button2) != DialogResult.Yes)
            return;

            var process = WslHelpers.CreateShutdownDistroProcess();
            process.Start();
            process.WaitForExit();
            AppContext.RefreshDistroList();
        }

        private void Feature_ExitApp(object sender, EventArgs e)
        {
            Close();
        }
    }
}
