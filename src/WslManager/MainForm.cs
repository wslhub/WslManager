using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace WslManager
{
    internal static class MainForm
    {
        public static Form Create()
        {
            var mainForm = new Form()
            {
                ClientSize = new Size(640, 480),
                Text = "WSL Manager",
                StartPosition = FormStartPosition.WindowsDefaultBounds,
            };

            var layout = new ToolStripContainer()
            {
                Parent = mainForm,
                Dock = DockStyle.Fill,
            };

            var listView = new ListView()
            {
                Parent = layout.ContentPanel,
                Dock = DockStyle.Fill,
                View = View.Details,
                MultiSelect = false,
                Sorting = SortOrder.None,
                FullRowSelect = true,
            };

            listView.Columns.Add(string.Empty, "Distro Name", 200);
            listView.Columns.Add("status", "Distro Status", 120);
            listView.Columns.Add("wslver", "WSL Version", 120);
            listView.Columns.Add("default", "Is Default", 120);

            var statusStrip = new StatusStrip()
            {
                Parent = layout.BottomToolStripPanel,
                Dock = DockStyle.Fill,
            };

            var statusItem = new ToolStripStatusLabel()
            {
                Spring = true,
                Text = "Ready",
                TextAlign = ContentAlignment.MiddleLeft,
            };

            statusStrip.Items.Add(statusItem);

            RefreshListView(listView, statusItem, GetDistroList());

            var backupWorker = new BackgroundWorker()
            {
                WorkerReportsProgress = true,
                WorkerSupportsCancellation = true,
            };

            backupWorker.DoWork += new DoWorkEventHandler((s, e) =>
            {
                var request = (DistroBackupRequest)e.Argument;

                var startInfo = new ProcessStartInfo("wsl.exe", $"--export \"{request.DistroName}\" \"{request.SaveFilePath}\"")
                {
                    UseShellExecute = false,
                };

                var process = new Process()
                {
                    StartInfo = startInfo,
                    EnableRaisingEvents = true,
                };

                process.Start();

                var list = GetDistroList();
                var convertingItem = list.DistroList.Where(x => string.Equals(x.DistroName, request.DistroName, StringComparison.Ordinal)).FirstOrDefault();
                backupWorker.ReportProgress(0, convertingItem);

                while (!process.HasExited && !backupWorker.CancellationPending)
                {
                    list = GetDistroList();
                    convertingItem = list.DistroList.Where(x => string.Equals(x.DistroName, request.DistroName, StringComparison.Ordinal)).FirstOrDefault();
                    backupWorker.ReportProgress(50, convertingItem);
                    Thread.Sleep(TimeSpan.FromSeconds(1d));
                }

                list = GetDistroList();
                convertingItem = list.DistroList.Where(x => string.Equals(x.DistroName, request.DistroName, StringComparison.Ordinal)).FirstOrDefault();
                backupWorker.ReportProgress(100, convertingItem);
                request.Succeed = true;
                e.Result = request;
            });

            backupWorker.ProgressChanged += new ProgressChangedEventHandler((s, e) =>
            {
                var targetItem = (DistroInfo)e.UserState;

                foreach (ListViewItem eachItem in listView.Items)
                {
                    var boundItem = (DistroInfo)eachItem.Tag;

                    if (!string.Equals(boundItem.DistroName, targetItem.DistroName))
                        continue;

                    eachItem.SubItems["status"].Text = targetItem.DistroStatus;
                    break;
                }
            });

            backupWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler((s, e) =>
            {
                if (e.Error != null)
                {
                    MessageBox.Show(mainForm,
                        "Unexpected error occurred. " + e.Error.Message,
                        mainForm.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (e.Cancelled)
                {
                    MessageBox.Show(mainForm,
                        "User cancelled the task",
                        mainForm.Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var result = e.Result as DistroBackupRequest;

                if (result == null)
                {
                    MessageBox.Show(mainForm,
                        "Cannot obtain task result. It seems like a bug.",
                        mainForm.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (result.Succeed)
                {
                    RefreshListView(listView, statusItem, GetDistroList());
                    var itemPath = result.SaveFilePath.Replace(@"/", @"\");
                    Process.Start("explorer.exe", "/select," + itemPath);
                }
                else
                {
                    MessageBox.Show(mainForm,
                        "Task does not succeed.",
                        mainForm.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            });

            var restoreWorker = new BackgroundWorker()
            {
                WorkerReportsProgress = true,
                WorkerSupportsCancellation = true,
            };

            restoreWorker.DoWork += new DoWorkEventHandler((s, e) =>
            {
                var request = (DistroRestoreRequest)e.Argument;

                var startInfo = new ProcessStartInfo("wsl.exe", $"--import \"{request.DistroName}\" \"{request.RestoreDirPath}\" \"{request.TarFilePath}\"")
                {
                    UseShellExecute = false,
                };

                var process = new Process()
                {
                    StartInfo = startInfo,
                    EnableRaisingEvents = true,
                };

                process.Start();

                var list = GetDistroList();
                var installingItem = list.DistroList.Where(x => string.Equals(x.DistroName, request.DistroName, StringComparison.Ordinal)).FirstOrDefault();

                if (installingItem == null)
                    installingItem = new DistroInfo() { DistroName = request.DistroName, DistroStatus = "Installing", IsDefault = false, WSLVersion = "?" };

                restoreWorker.ReportProgress(0, installingItem);

                while (!process.HasExited && !restoreWorker.CancellationPending)
                {
                    list = GetDistroList();
                    installingItem = list.DistroList.Where(x => string.Equals(x.DistroName, request.DistroName, StringComparison.Ordinal)).FirstOrDefault();
                    restoreWorker.ReportProgress(50, installingItem);
                    Thread.Sleep(TimeSpan.FromSeconds(1d));
                }

                list = GetDistroList();
                installingItem = list.DistroList.Where(x => string.Equals(x.DistroName, request.DistroName, StringComparison.Ordinal)).FirstOrDefault();

                if (request.SetAsDefault)
                {
                    startInfo = new ProcessStartInfo("wsl.exe", $"--set-default \"{request.DistroName}\"")
                    {
                        UseShellExecute = false,
                    };

                    process = new Process()
                    {
                        StartInfo = startInfo,
                        EnableRaisingEvents = true,
                    };

                    process.Start();
                    process.WaitForExit();
                }

                restoreWorker.ReportProgress(100, installingItem);
                request.Succeed = true;
                e.Result = request;
            });

            restoreWorker.ProgressChanged += new ProgressChangedEventHandler((s, e) =>
            {
                var targetItem = (DistroInfo)e.UserState;

                if (targetItem == null)
                    return;

                var found = false;

                foreach (ListViewItem eachItem in listView.Items)
                {
                    var boundItem = (DistroInfo)eachItem.Tag;

                    if (!string.Equals(boundItem.DistroName, targetItem.DistroName))
                        continue;

                    eachItem.SubItems["status"].Text = targetItem.DistroStatus;
                    found = true;
                    break;
                }

                if (!found)
                    AddDistroInfoIntoListView(listView, targetItem);
            });

            restoreWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler((s, e) =>
            {
                if (e.Error != null)
                {
                    MessageBox.Show(mainForm,
                        "Unexpected error occurred. " + e.Error.Message,
                        mainForm.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (e.Cancelled)
                {
                    MessageBox.Show(mainForm,
                        "User cancelled the task",
                        mainForm.Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var result = e.Result as DistroRestoreRequest;

                if (result == null)
                {
                    MessageBox.Show(mainForm,
                        "Cannot obtain task result. It seems like a bug.",
                        mainForm.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (result.Succeed)
                {
                    RefreshListView(listView, statusItem, GetDistroList());

                    var startInfo = new ProcessStartInfo("cmd.exe", $"/c wsl.exe --distribution {result.DistroName}")
                    {
                        UseShellExecute = false,
                        WorkingDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                    };

                    var process = new Process()
                    {
                        StartInfo = startInfo,
                        EnableRaisingEvents = true,
                    };

                    process.Start();
                }
                else
                {
                    MessageBox.Show(mainForm,
                        "Task does not succeed.",
                        mainForm.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            });

            var menuStrip = new MenuStrip()
            {
                Parent = layout.TopToolStripPanel,
                Dock = DockStyle.Fill,
            };

            var appMenu = menuStrip.Items.AddMenuItem("&App");

            var aboutMenuItem = appMenu.DropDownItems.AddMenuItem("&About...");

            aboutMenuItem.Click += new EventHandler((s, e) =>
            {
                var message = string.Join(Environment.NewLine,
                    "WSL Manager v0.1",
                    "(c) 2019 rkttu.com, All rights reserved.");
                
                MessageBox.Show(mainForm, message, mainForm.Text,
                    MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
            });

            appMenu.DropDownItems.AddSeparator();

            var exitMenuItem = appMenu.DropDownItems.AddMenuItem("E&xit");

            exitMenuItem.Click += new EventHandler((s, e) =>
            {
                mainForm.Close();
            });

            var pointContextMenuStrip = new ContextMenuStrip();

            var openDistroContextMenuItem = pointContextMenuStrip.Items.AddMenuItem("&Open Distro...");

            openDistroContextMenuItem.Click += new EventHandler((s, e) =>
            {
                var hitTest = pointContextMenuStrip.Tag as ListViewHitTestInfo;
                var targetItem = hitTest?.Item?.Tag as DistroInfo;

                if (targetItem == null)
                    return;

                var startInfo = new ProcessStartInfo("cmd.exe", $"/c wsl.exe --distribution {targetItem.DistroName}")
                {
                    UseShellExecute = false,
                    WorkingDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                };

                var process = new Process()
                {
                    StartInfo = startInfo,
                    EnableRaisingEvents = true,
                };

                var result = process.Start();
            });

            pointContextMenuStrip.Items.AddSeparator();

            var backupDistroContextMenuItem = pointContextMenuStrip.Items.AddMenuItem("&Backup Distro...");

            backupDistroContextMenuItem.Click += new EventHandler((s, e) =>
            {
                if (backupWorker.IsBusy)
                {
                    MessageBox.Show(
                        mainForm, "Already one or more backup in progress. Please try again later.",
                        mainForm.Text, MessageBoxButtons.OK, MessageBoxIcon.Stop);

                    return;
                }

                var hitTest = pointContextMenuStrip.Tag as ListViewHitTestInfo;
                var targetItem = hitTest?.Item?.Tag as DistroInfo;

                if (targetItem == null)
                    return;

                using var saveFileDialog = new SaveFileDialog()
                {
                    Title = $"Backup {targetItem.DistroName}",
                    SupportMultiDottedExtensions = true,
                    Filter = "Tape Archive File|*.tar",
                    DefaultExt = ".tar",
                    FileName = $"backup-{targetItem.DistroName.ToLowerInvariant()}-{DateTime.Now:yyyy-MM-dd-HH-mm-ss}.tar",
                };

                if (saveFileDialog.ShowDialog(mainForm) != DialogResult.OK)
                    return;

                backupWorker.RunWorkerAsync(new DistroBackupRequest()
                {
                    DistroName = targetItem.DistroName,
                    SaveFilePath = saveFileDialog.FileName,
                });
            });

            var unregisterDistroContextMenuItem = pointContextMenuStrip.Items.AddMenuItem("&Unregister Distro...");

            unregisterDistroContextMenuItem.Click += new EventHandler((s, e) =>
            {
                var hitTest = pointContextMenuStrip.Tag as ListViewHitTestInfo;
                var targetItem = hitTest?.Item?.Tag as DistroInfo;

                if (targetItem == null)
                    return;

                if (MessageBox.Show(mainForm, $"Really unregister `{targetItem.DistroName}` distro? This cannot be undone.",
                    mainForm.Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button2) != DialogResult.Yes)
                    return;

                var startInfo = new ProcessStartInfo("wsl.exe", $"--unregister {targetItem.DistroName}")
                {
                    UseShellExecute = false,
                };

                var process = new Process()
                {
                    StartInfo = startInfo,
                    EnableRaisingEvents = true,
                };

                process.Start();
                process.WaitForExit();
                RefreshListView(listView, statusItem, GetDistroList());
            });

            pointContextMenuStrip.Items.AddSeparator();

            var setAsDefaultDistroContextMenuItem = pointContextMenuStrip.Items.AddMenuItem("&Set as default distro");

            setAsDefaultDistroContextMenuItem.Click += new EventHandler((s, e) =>
            {
                var hitTest = pointContextMenuStrip.Tag as ListViewHitTestInfo;
                var targetItem = hitTest?.Item?.Tag as DistroInfo;

                if (targetItem == null)
                    return;

                var startInfo = new ProcessStartInfo("wsl.exe", $"--set-default {targetItem.DistroName}")
                {
                    UseShellExecute = false,
                };

                var process = new Process()
                {
                    StartInfo = startInfo,
                    EnableRaisingEvents = true,
                };

                process.Start();
                process.WaitForExit();
                RefreshListView(listView, statusItem, GetDistroList());
            });

            var defaultContextMenuStrip = new ContextMenuStrip();

            var refreshListContextMenuItem = defaultContextMenuStrip.Items.AddMenuItem("Refresh &List");

            refreshListContextMenuItem.Click += new EventHandler((s, e) =>
            {
                RefreshListView(listView, statusItem, GetDistroList());
            });

            defaultContextMenuStrip.Items.AddSeparator();

            var restoreDistroContextMenuItem = defaultContextMenuStrip.Items.AddMenuItem("&Restore Distro...");

            restoreDistroContextMenuItem.Click += new EventHandler((s, e) =>
            {
                if (restoreWorker.IsBusy)
                {
                    MessageBox.Show(
                        mainForm, "Already one or more restore in progress. Please try again later.",
                        mainForm.Text, MessageBoxButtons.OK, MessageBoxIcon.Stop);

                    return;
                }

                using var dialog = RestoreForm.Create();

                if (dialog.ShowDialog(mainForm) != DialogResult.OK)
                    return;

                var restoreRequest = dialog.Tag as DistroRestoreRequest;

                if (restoreRequest == null)
                    return;

                restoreWorker.RunWorkerAsync(restoreRequest);
            });

            listView.KeyUp += new KeyEventHandler((s, e) =>
            {
                if (e.KeyCode == Keys.F5)
                {
                    RefreshListView(listView, statusItem, GetDistroList());
                    return;
                }
            });

            listView.MouseDown += new MouseEventHandler((s, e) =>
            {
                if (e.Button == MouseButtons.Right)
                {
                    var hitTest = listView.HitTest(e.Location);

                    if (hitTest.Location == ListViewHitTestLocations.None)
                        defaultContextMenuStrip.Show(Cursor.Position);
                    else
                    {
                        pointContextMenuStrip.Show(Cursor.Position);
                        pointContextMenuStrip.Tag = hitTest;
                    }
                }
            });

            listView.ItemActivate += new EventHandler((s, e) =>
            {
                if (listView.SelectedItems.Count != 1)
                    return;

                var targetItem = listView.SelectedItems[0].Tag as DistroInfo;

                if (targetItem == null)
                    return;

                var startInfo = new ProcessStartInfo("cmd.exe", $"/c wsl.exe --distribution {targetItem.DistroName}")
                {
                    UseShellExecute = false,
                    WorkingDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                };

                var process = new Process()
                {
                    StartInfo = startInfo,
                    EnableRaisingEvents = true,
                };

                var result = process.Start();
            });

            return mainForm;
        }

        public static DistroInfoList GetDistroList()
        {
            var processStartInfo = new ProcessStartInfo("wsl.exe", "-l -v")
            {
                LoadUserProfile = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                StandardOutputEncoding = Encoding.ASCII,
                CreateNoWindow = true,
            };

            using var process = new Process()
            {
                StartInfo = processStartInfo,
                EnableRaisingEvents = true,
            };

            if (!process.Start())
                throw new Exception("Cannot start the WSL process.");

            process.WaitForExit();
            var output = process.StandardOutput.ReadToEnd().Replace("\0", string.Empty);
            return new DistroInfoList(output);
        }

        public static void AddDistroInfoIntoListView(
            ListView listView,
            DistroInfo distroInfo)
        {
            var lvItem = new ListViewItem(distroInfo.DistroName) { Tag = distroInfo };

            foreach (ColumnHeader eachSubItem in listView.Columns)
            {
                switch (eachSubItem.Name)
                {
                    case "status":
                        lvItem.SubItems.Add(new ListViewItem.ListViewSubItem() { Name = "status", Text = distroInfo.DistroStatus, });
                        break;

                    case "wslver":
                        lvItem.SubItems.Add(new ListViewItem.ListViewSubItem() { Name = "wslver", Text = distroInfo.WSLVersion, });
                        break;

                    case "default":
                        lvItem.SubItems.Add(new ListViewItem.ListViewSubItem() { Name = "default", Text = distroInfo.IsDefault ? "*" : string.Empty, });
                        break;
                }
            }

            listView.Items.Add(lvItem);
            lvItem.Selected = distroInfo.IsDefault;
        }

        public static void RefreshListView(
            ListView listView,
            ToolStripStatusLabel stateLabel,
            DistroInfoList distroInfoList)
        {
            if (listView.InvokeRequired)
            {
                listView.Invoke(
                    new Action<ListView, ToolStripStatusLabel, DistroInfoList>(RefreshListView),
                    listView, stateLabel, distroInfoList);
                return;
            }

            listView.BeginUpdate();

            if (listView.Items.Count > 0)
                listView.Items.Clear();

            foreach (var eachDistro in distroInfoList.DistroList)
                AddDistroInfoIntoListView(listView, eachDistro);

            stateLabel.Text = $"Total {distroInfoList.DistroList.Count()} distros found.";
            listView.EndUpdate();
        }

    }
}
