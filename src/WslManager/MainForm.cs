using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using WslManager.Extensions;
using WslManager.Models;

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

            var components = new Container();

            var layout = new ToolStripContainer()
            {
                Parent = mainForm,
                Dock = DockStyle.Fill,
            };

            var largeImageList = new ImageList()
            {
                ColorDepth = ColorDepth.Depth32Bit,
                ImageSize = new Size(96, 96),
            };
            components.Add(largeImageList);

            var smallImageList = new ImageList()
            {
                ColorDepth = ColorDepth.Depth32Bit,
                ImageSize = new Size(32, 32),
            };
            components.Add(smallImageList);

            foreach (KeyValuePair<string, string> pairs in Resources.LogoImages)
            {
                using var memStream = new MemoryStream(Convert.FromBase64String(pairs.Value), false);
                var loadedImage = Image.FromStream(memStream, true);
                largeImageList.Images.Add(pairs.Key, loadedImage);
                var smallImage = ResizeImage(loadedImage, smallImageList.ImageSize.Width, smallImageList.ImageSize.Height);
                smallImageList.Images.Add(pairs.Key, smallImage);
            }

            var stateImageList = new ImageList()
            {
                ColorDepth = ColorDepth.Depth32Bit,
                ImageSize = new Size(16, 16),
            };
            components.Add(stateImageList);

            foreach (KeyValuePair<string, string> pairs in Resources.StateImages)
            {
                using var memStream = new MemoryStream(Convert.FromBase64String(pairs.Value), false);
                var loadedImage = Image.FromStream(memStream, true);
                stateImageList.Images.Add(pairs.Key, loadedImage);
            }

            var listView = new ListView()
            {
                Parent = layout.ContentPanel,
                Dock = DockStyle.Fill,
                View = View.Details,
                MultiSelect = false,
                Sorting = SortOrder.None,
                FullRowSelect = true,
                LargeImageList = largeImageList,
                SmallImageList = smallImageList,
                StateImageList = stateImageList,
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

            var backupWorker = new BackgroundWorker()
            {
                WorkerReportsProgress = true,
                WorkerSupportsCancellation = true,
            };
            components.Add(backupWorker);

            backupWorker.DoWork += new DoWorkEventHandler((s, e) =>
            {
                var request = (DistroBackupRequest)e.Argument;
                var process = request.CreateExportDistroProcess(request.SaveFilePath);
                process.Start();

                var list = WslExtensions.GetDistroList();
                var convertingItem = list.Where(x => string.Equals(x.DistroName, request.DistroName, StringComparison.Ordinal)).FirstOrDefault();
                backupWorker.ReportProgress(0, convertingItem);

                while (!process.HasExited && !backupWorker.CancellationPending)
                {
                    list = WslExtensions.GetDistroList();
                    convertingItem = list.Where(x => string.Equals(x.DistroName, request.DistroName, StringComparison.Ordinal)).FirstOrDefault();
                    backupWorker.ReportProgress(50, convertingItem);
                    Thread.Sleep(TimeSpan.FromSeconds(1d));
                }

                list = WslExtensions.GetDistroList();
                convertingItem = list.Where(x => string.Equals(x.DistroName, request.DistroName, StringComparison.Ordinal)).FirstOrDefault();
                backupWorker.ReportProgress(100, convertingItem);
                request.Succeed = true;
                e.Result = request;
            });

            backupWorker.ProgressChanged += new ProgressChangedEventHandler((s, e) =>
            {
                if (mainForm.IsDisposed)
                    return;

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
                if (mainForm.IsDisposed)
                    return;

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
                    RefreshListView(listView, statusItem, WslExtensions.GetDistroList());
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
            components.Add(restoreWorker);

            restoreWorker.DoWork += new DoWorkEventHandler((s, e) =>
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
            });

            restoreWorker.ProgressChanged += new ProgressChangedEventHandler((s, e) =>
            {
                if (mainForm.IsDisposed)
                    return;

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
                if (mainForm.IsDisposed)
                    return;

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
                    RefreshListView(listView, statusItem, WslExtensions.GetDistroList());
                    var process = result.CreateLaunchSpecificDistroProcess();
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

                var process = targetItem.CreateLaunchSpecificDistroProcess();
                var result = process.Start();
            });

            pointContextMenuStrip.Items.AddSeparator();

            var openDistroFolderContextMenuItem = pointContextMenuStrip.Items.AddMenuItem("E&xplore Distro File System...");

            openDistroFolderContextMenuItem.Click += new EventHandler((s, e) =>
            {
                var hitTest = pointContextMenuStrip.Tag as ListViewHitTestInfo;
                var targetItem = hitTest?.Item?.Tag as DistroInfo;

                if (targetItem == null)
                    return;

                var process = targetItem.CreateLaunchSpecificDistroExplorerProcess();
                var result = process.Start();
            });

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
                    mainForm.Text, MessageBoxButtons.YesNo, MessageBoxIcon.Warning,
                    MessageBoxDefaultButton.Button2) != DialogResult.Yes)
                    return;

                var process = targetItem.CreateUnregisterDistroProcess();
                process.Start();
                process.WaitForExit();
                RefreshListView(listView, statusItem, WslExtensions.GetDistroList());
            });

            pointContextMenuStrip.Items.AddSeparator();

            var setAsDefaultDistroContextMenuItem = pointContextMenuStrip.Items.AddMenuItem("Set as &default distro");

            setAsDefaultDistroContextMenuItem.Click += new EventHandler((s, e) =>
            {
                var hitTest = pointContextMenuStrip.Tag as ListViewHitTestInfo;
                var targetItem = hitTest?.Item?.Tag as DistroInfo;

                if (targetItem == null)
                    return;

                var process = targetItem.CreateSetAsDefaultProcess();
                process.Start();
                process.WaitForExit();
                RefreshListView(listView, statusItem, WslExtensions.GetDistroList());
            });

            var defaultContextMenuStrip = new ContextMenuStrip();

            var viewTypeContextMenuItem = defaultContextMenuStrip.Items.AddMenuItem("&View");

            var largeIconViewTypeContextMenuItem = viewTypeContextMenuItem.DropDownItems.AddMenuItem("&Large Icon");

            largeIconViewTypeContextMenuItem.Click += new EventHandler((s, e) =>
            {
                listView.View = View.LargeIcon;
            });

            var smallIconViewTypeContextMenuItem = viewTypeContextMenuItem.DropDownItems.AddMenuItem("&Small Icon");

            smallIconViewTypeContextMenuItem.Click += new EventHandler((s, e) =>
            {
                listView.View = View.SmallIcon;
            });

            var listViewTypeContextMenuItem = viewTypeContextMenuItem.DropDownItems.AddMenuItem("&List");

            listViewTypeContextMenuItem.Click += new EventHandler((s, e) =>
            {
                listView.View = View.List;
            });

            var detailViewTypeContextMenuItem = viewTypeContextMenuItem.DropDownItems.AddMenuItem("&Detail");

            detailViewTypeContextMenuItem.Click += new EventHandler((s, e) =>
            {
                listView.View = View.Details;
            });

            var tileViewTypeContextMenuItem = viewTypeContextMenuItem.DropDownItems.AddMenuItem("&Tile");

            tileViewTypeContextMenuItem.Click += new EventHandler((s, e) =>
            {
                listView.View = View.Tile;
            });

            var refreshListContextMenuItem = defaultContextMenuStrip.Items.AddMenuItem("Refresh &List");

            refreshListContextMenuItem.Click += new EventHandler((s, e) =>
            {
                RefreshListView(listView, statusItem, WslExtensions.GetDistroList());
            });

            viewTypeContextMenuItem.DropDownOpened += new EventHandler((s, e) =>
            {
                foreach (ToolStripMenuItem eachSubItem in viewTypeContextMenuItem.DropDownItems)
                    eachSubItem.Checked = false;

                switch (listView.View)
                {
                    case View.LargeIcon:
                        largeIconViewTypeContextMenuItem.Checked = true;
                        break;

                    case View.SmallIcon:
                        smallIconViewTypeContextMenuItem.Checked = true;
                        break;

                    case View.List:
                        listViewTypeContextMenuItem.Checked = true;
                        break;

                    case View.Details:
                        detailViewTypeContextMenuItem.Checked = true;
                        break;

                    case View.Tile:
                        tileViewTypeContextMenuItem.Checked = true;
                        break;
                }
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

            defaultContextMenuStrip.Items.AddSeparator();

            var shutdownContextMenuItem = defaultContextMenuStrip.Items.AddMenuItem("&Shutdown WSL...");

            shutdownContextMenuItem.Click += new EventHandler((s, e) =>
            {
                if (MessageBox.Show(mainForm, $"Really shutdown WSL entirely? This operation can cause unintentional data loss.",
                    mainForm.Text, MessageBoxButtons.YesNo, MessageBoxIcon.Warning,
                    MessageBoxDefaultButton.Button2) != DialogResult.Yes)
                    return;

                var process = WslExtensions.CreateShutdownDistroProcess();
                process.Start();
                process.WaitForExit();
                RefreshListView(listView, statusItem, WslExtensions.GetDistroList());
            });

            listView.KeyUp += new KeyEventHandler((s, e) =>
            {
                if (e.KeyCode == Keys.F5)
                {
                    RefreshListView(listView, statusItem, WslExtensions.GetDistroList());
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

                var process = targetItem.CreateLaunchSpecificDistroProcess();
                process.Start();
            });

            mainForm.Load += new EventHandler((s, e) =>
            {
                var wslHostPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.System),
                    "lxss", "wslhost.exe");

                if (!File.Exists(wslHostPath))
                {
                    MessageBox.Show(mainForm, "It looks like WSL does not available on this system. Please install WSL first.",
                        mainForm.Text, MessageBoxButtons.OK, MessageBoxIcon.Stop, MessageBoxDefaultButton.Button1);
                    mainForm.Close();
                    return;
                }

                RefreshListView(listView, statusItem, WslExtensions.GetDistroList());
            });

            mainForm.Disposed += new EventHandler((s, e) =>
            {
                if (components == null)
                    return;

                components.Dispose();
            });

            return mainForm;
        }

        public static ListViewItem AddDistroInfoIntoListView(
            ListView listView,
            DistroInfo distroInfo)
        {
            var lvItem = new ListViewItem(distroInfo.DistroName) { Tag = distroInfo, ImageKey = "linux", };
            var roughName = distroInfo?.DistroName?.Trim() ?? string.Empty;

            foreach (var eachKey in Resources.LogoImages.Keys)
            {
                if (roughName.Contains(eachKey, StringComparison.OrdinalIgnoreCase))
                    lvItem.ImageKey = eachKey;
            }

            if (distroInfo.IsDefault)
                lvItem.StateImageIndex = 0;
            else if (string.Equals(distroInfo.DistroStatus, "Installing", StringComparison.OrdinalIgnoreCase))
                lvItem.StateImageIndex = 1;

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
            return lvItem;
        }

        public static void RefreshListView(
            ListView listView,
            ToolStripStatusLabel stateLabel,
            IEnumerable<DistroInfo> distroInfoList)
        {
            if (listView.InvokeRequired)
            {
                listView.Invoke(
                    new Action<ListView, ToolStripStatusLabel, IEnumerable<DistroInfo>>(RefreshListView),
                    listView, stateLabel, distroInfoList);
                return;
            }

            listView.BeginUpdate();
            var selectedDistroName = default(string);

            if (listView.SelectedItems.Count > 0)
                selectedDistroName = (listView.SelectedItems[0]?.Tag as DistroInfo)?.DistroName;

            if (listView.Items.Count > 0)
                listView.Items.Clear();

            foreach (var eachDistro in distroInfoList)
            {
                var createdItem = AddDistroInfoIntoListView(listView, eachDistro);

                if (string.Equals(eachDistro.DistroName, selectedDistroName, StringComparison.Ordinal))
                    createdItem.Selected = true;
            }

            stateLabel.Text = $"Total {distroInfoList.Count()} distros found. - {DateTime.Now}";
            listView.EndUpdate();
        }

        public static Bitmap ResizeImage(Image image, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using var wrapMode = new ImageAttributes();
                wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
            }

            return destImage;
        }
    }
}
