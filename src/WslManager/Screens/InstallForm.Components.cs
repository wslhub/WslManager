using System;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using WslManager.Extensions;
using WslManager.ViewModels;

namespace WslManager.Screens
{
    // Components
    partial class InstallForm
    {
        private ErrorProvider errorProvider;
        private FolderBrowserDialog distroInstallDirOpenDialog;
        private BackgroundWorker rootFsDownloadWorker;

        protected override void InitializeComponents(IContainer components)
        {
            base.InitializeComponents(components);

            errorProvider = new ErrorProvider(this)
            {
                BlinkStyle = ErrorBlinkStyle.BlinkIfDifferentError,
            };
            components.Add(errorProvider);

            distroInstallDirOpenDialog = new FolderBrowserDialog()
            {
                ShowNewFolderButton = true,
                AutoUpgradeEnabled = true,
                Description = "Select a directory to install WSL distro.",
                UseDescriptionForTitle = true,
            };
            components.Add(distroInstallDirOpenDialog);

            rootFsDownloadWorker = new BackgroundWorker()
            {
                WorkerSupportsCancellation = true,
                WorkerReportsProgress = true,
            };
            components.Add(rootFsDownloadWorker);

            rootFsDownloadWorker.DoWork += RootFsDownloadWorker_DoWork;
            rootFsDownloadWorker.ProgressChanged += RootFsDownloadWorker_ProgressChanged;
            rootFsDownloadWorker.RunWorkerCompleted += RootFsDownloadWorker_RunWorkerCompleted;
        }

        private void RootFsDownloadWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e.ProgressPercentage < 3)
                downloadProgressBar.Style = ProgressBarStyle.Marquee;
            else if (e.ProgressPercentage < 100)
                downloadProgressBar.Style = ProgressBarStyle.Continuous;
            else
                downloadProgressBar.Style = ProgressBarStyle.Blocks;

            downloadProgressBar.Value = e.ProgressPercentage;
        }

        private void RootFsDownloadWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            var cancellationSource = new CancellationTokenSource();
            var context = e.Argument as AsyncDownloadContext;
            e.Result = context;

            rootFsDownloadWorker.ReportProgress(0, context);

            var progressCallback = new Action<long, long?>((read, total) =>
            {
                if (rootFsDownloadWorker.CancellationPending)
                {
                    cancellationSource.Cancel();
                    return;
                }

                rootFsDownloadWorker.ReportProgress(
                    (int)(total.HasValue ? (double)read / total.Value * 100d : 50),
                    context);
            });

            using var outputStream = File.OpenWrite(context.DownloadedFilePath);

            try
            {
                context.Url.CopyStreamAsync(outputStream,
                    cancellationToken: cancellationSource.Token,
                    progressCallback: progressCallback).Wait();
            }
            catch (AggregateException ae)
            {
                switch (ae.InnerException)
                {
                    case TaskCanceledException _:
                        e.Cancel = true;
                        break;

                    case OperationCanceledException _:
                        e.Cancel = true;
                        break;

                    default:
                        throw ae.InnerException;
                }
            }
        }

        private void RootFsDownloadWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (IsDisposed)
                return;

            ViewModel.DownloadInProgress = false;

            if (e.Cancelled)
            {
                MessageBox.Show(this, "Task was cancelled.", Text,
                    MessageBoxButtons.OK, MessageBoxIcon.Stop, MessageBoxDefaultButton.Button1);
                return;
            }

            var context = e.Result as AsyncDownloadContext;

            if (e.Error != null)
            {
                MessageBox.Show(this, "Task was interrupted due to error. " + e.Error.Message, Text,
                    MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                return;
            }

            if (context == null)
            {
                MessageBox.Show(this, "Task was completed with no result.", Text,
                    MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                return;
            }

            if (!File.Exists(context.DownloadedFilePath))
            {
                MessageBox.Show(this, "File was downloaded, but no file was created.", Text,
                    MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                return;
            }

            ViewModel.DownloadedTarFilePath = context.DownloadedFilePath;
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
