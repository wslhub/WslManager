using System;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using WslManager.Extensions;
using WslManager.ViewModels;

namespace WslManager.Screens.MainForm
{
    // Restore Worker
    partial class MainForm
    {
        private BackgroundWorker restoreWorker;

        partial void InitializeRestoreWorker(IContainer components)
        {
            restoreWorker = new BackgroundWorker()
            {
                WorkerReportsProgress = true,
                WorkerSupportsCancellation = true,
            };
            components.Add(restoreWorker);

            restoreWorker.DoWork += RestoreWorker_DoWork;
            restoreWorker.ProgressChanged += RestoreWorker_ProgressChanged;
            restoreWorker.RunWorkerCompleted += RestoreWorker_RunWorkerCompleted;
        }

        private void RestoreWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            var request = (DistroRestoreRequest)e.Argument;
            var process = request.CreateImportDistroProcess(request.RestoreDirPath, request.TarFilePath);
            process.Start();

            var list = WslExtensions.GetDistroList();
            var installingItem = list.Where(x => string.Equals(x.DistroName, request.DistroName, StringComparison.Ordinal)).FirstOrDefault();

            if (installingItem == null)
                installingItem = new DistroInfo() { DistroName = request.DistroName, DistroStatus = "Installing", IsDefault = false, WSLVersion = "?" };

            restoreWorker.ReportProgress(0, installingItem);

            while (!process.HasExited && !restoreWorker.CancellationPending)
            {
                list = WslExtensions.GetDistroList();
                installingItem = list.Where(x => string.Equals(x.DistroName, request.DistroName, StringComparison.Ordinal)).FirstOrDefault();
                restoreWorker.ReportProgress(50, installingItem);
                Thread.Sleep(TimeSpan.FromSeconds(1d));
            }

            list = WslExtensions.GetDistroList();
            installingItem = list.Where(x => string.Equals(x.DistroName, request.DistroName, StringComparison.Ordinal)).FirstOrDefault();

            if (request.SetAsDefault)
            {
                process = request.CreateSetAsDefaultProcess();
                process.Start();
                process.WaitForExit();
            }

            restoreWorker.ReportProgress(100, installingItem);
            request.Succeed = true;
            e.Result = request;
        }

        private void RestoreWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (IsDisposed)
                return;

            var targetItem = (DistroInfo)e.UserState;

            if (targetItem == null)
                return;

            //var found = false;

            foreach (ListViewItem eachItem in listView.Items)
            {
                var boundItem = (DistroInfo)eachItem.Tag;

                if (!string.Equals(boundItem.DistroName, targetItem.DistroName))
                    continue;

                eachItem.SubItems["status"].Text = targetItem.DistroStatus;
                //found = true;
                break;
            }

            //if (!found)
                //AddDistroInfoIntoListView(listView, targetItem);
        }

        private void RestoreWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (IsDisposed)
                return;

            if (e.Error != null)
            {
                MessageBox.Show(this,
                    "Unexpected error occurred. " + e.Error.Message,
                    Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (e.Cancelled)
            {
                MessageBox.Show(this,
                    "User cancelled the task",
                    Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var result = e.Result as DistroRestoreRequest;

            if (result == null)
            {
                MessageBox.Show(this,
                    "Cannot obtain task result. It seems like a bug.",
                    Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (result.Succeed)
            {
                //RefreshListView(listView, statusItem, WslExtensions.GetDistroList());
                var process = result.CreateLaunchSpecificDistroProcess();
                process.Start();
            }
            else
            {
                MessageBox.Show(this,
                    "Task does not succeed.",
                    Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
