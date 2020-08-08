using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace WslManager
{
    internal static class RestoreForm
    {
        public static readonly Lazy<NamesGenerator> NameGenerator =
            new Lazy<NamesGenerator>(false);

        public static Form Create()
        {
            var inputForm = new Form()
            {
                ClientSize = new Size(640, 220),
                Text = "Restore Distro",
                StartPosition = FormStartPosition.CenterParent,
                Padding = new Padding(5),
                ShowIcon = false,
                ShowInTaskbar = false,
                MinimizeBox = false,
                MaximizeBox = false,
                FormBorderStyle = FormBorderStyle.FixedDialog,
            };
            inputForm.MinimumSize = new Size(inputForm.Width, inputForm.Height);
            inputForm.MaximumSize = new Size(inputForm.Width * 2, inputForm.Height);

            var layout = new TableLayoutPanel()
            {
                Parent = inputForm,
                Dock = DockStyle.Fill,
            };

            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 180f));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 0.65f));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90f));

            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 0.2f));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 0.2f));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 0.2f));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 0.2f));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 0.2f));

            var tarFileLabel = new Label()
            {
                Parent = layout,
                Text = "Backup File: ",
                AutoEllipsis = true,
                TextAlign = ContentAlignment.MiddleRight,
                Dock = DockStyle.Fill,
            };
            layout.SetCellPosition(tarFileLabel, new TableLayoutPanelCellPosition(0, 0));

            var tarFilePath = new TextBox()
            {
                Parent = layout,
                Dock = DockStyle.Fill,
                AutoCompleteMode = AutoCompleteMode.SuggestAppend,
                AutoCompleteSource = AutoCompleteSource.FileSystem,
            };
            layout.SetCellPosition(tarFilePath, new TableLayoutPanelCellPosition(1, 0));

            var tarFileOpenButton = new Button()
            {
                Parent = layout,
                Text = "&Open...",
                Dock = DockStyle.Fill,
            };
            layout.SetCellPosition(tarFileOpenButton, new TableLayoutPanelCellPosition(2, 0));

            tarFileOpenButton.Click += new EventHandler((s, e) =>
            {
                using var fileDialog = new OpenFileDialog()
                {
                    Title = "Open WSL Backup File",
                    SupportMultiDottedExtensions = true,
                    DefaultExt = ".tar",
                    Filter = "Tape Archive File|*.tar",
                    AutoUpgradeEnabled = true,
                };

                var selectedFilePath = tarFilePath.Text;

                if (File.Exists(selectedFilePath))
                    fileDialog.FileName = selectedFilePath;

                if (fileDialog.ShowDialog(inputForm) != DialogResult.OK)
                    return;

                tarFilePath.Text = fileDialog.FileName;
            });

            var installDirLabel = new Label()
            {
                Parent = layout,
                Text = "Install Directory: ",
                AutoEllipsis = true,
                TextAlign = ContentAlignment.MiddleRight,
                Dock = DockStyle.Fill,
            };
            layout.SetCellPosition(installDirLabel, new TableLayoutPanelCellPosition(0, 1));

            var installDirPath = new TextBox()
            {
                Parent = layout,
                Dock = DockStyle.Fill,
                AutoCompleteMode = AutoCompleteMode.SuggestAppend,
                AutoCompleteSource = AutoCompleteSource.FileSystemDirectories,
            };
            layout.SetCellPosition(installDirPath, new TableLayoutPanelCellPosition(1, 1));

            var installDirBrowseButton = new Button()
            {
                Parent = layout,
                Text = "&Browse...",
                Dock = DockStyle.Fill,
            };

            installDirBrowseButton.Click += new EventHandler((s, e) =>
            {
                using var dirDialog = new FolderBrowserDialog()
                {
                    ShowNewFolderButton = true,
                    AutoUpgradeEnabled = true,
                    Description = "Select a directory to restore WSL distro.",
                    UseDescriptionForTitle = true,
                };

                if (Directory.Exists(installDirPath.Text))
                    dirDialog.SelectedPath = installDirPath.Text;
                else
                    dirDialog.SelectedPath = Path.GetPathRoot(Environment.CurrentDirectory);

                if (dirDialog.ShowDialog(inputForm) != DialogResult.OK)
                    return;

                installDirPath.Text = dirDialog.SelectedPath;
            });

            layout.SetCellPosition(installDirBrowseButton, new TableLayoutPanelCellPosition(2, 1));

            var distroNameLabel = new Label()
            {
                Parent = layout,
                Text = "Distro Name: ",
                AutoEllipsis = true,
                TextAlign = ContentAlignment.MiddleRight,
                Dock = DockStyle.Fill,
            };
            layout.SetCellPosition(distroNameLabel, new TableLayoutPanelCellPosition(0, 2));

            var distroNameValue = new TextBox()
            {
                Parent = layout,
                Dock = DockStyle.Fill,
                Text = NameGenerator.Value.GetRandomName(),
            };
            layout.SetCellPosition(distroNameValue, new TableLayoutPanelCellPosition(1, 2));

            var distroNameSuggestButton = new Button()
            {
                Parent = layout,
                Text = "&Suggest",
                Dock = DockStyle.Fill,
            };

            distroNameSuggestButton.Click += new EventHandler((s, e) =>
            {
                distroNameValue.Text = NameGenerator.Value.GetRandomName();
            });

            layout.SetCellPosition(distroNameSuggestButton, new TableLayoutPanelCellPosition(2, 2));

            var setAsDefaultLabel = new Label()
            {
                Parent = layout,
                Text = "Set As Default: ",
                AutoEllipsis = true,
                TextAlign = ContentAlignment.MiddleRight,
                Dock = DockStyle.Fill,
            };
            layout.SetCellPosition(setAsDefaultLabel, new TableLayoutPanelCellPosition(0, 3));

            var setAsDefaultCheckBox = new CheckBox()
            {
                Parent = layout,
                Text = "&Check to set as a default",
                AutoEllipsis = true,
                TextAlign = ContentAlignment.MiddleLeft,
                Dock = DockStyle.Fill,
            };
            layout.SetCellPosition(setAsDefaultCheckBox, new TableLayoutPanelCellPosition(1, 3));
            layout.SetColumnSpan(setAsDefaultCheckBox, 2);

            var actionPanel = new FlowLayoutPanel()
            {
                Parent = layout,
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft,
            };
            layout.SetCellPosition(actionPanel, new TableLayoutPanelCellPosition(0, 4));
            layout.SetColumnSpan(actionPanel, 3);

            var cancelButton = new Button()
            {
                Parent = actionPanel,
                Text = "&Cancel",
                DialogResult = DialogResult.Cancel,
                AutoSize = true,
            };
            inputForm.CancelButton = cancelButton;

            var confirmButton = new Button()
            {
                Parent = actionPanel,
                Text = "&Restore",
                DialogResult = DialogResult.OK,
                AutoSize = true,
            };
            inputForm.AcceptButton = confirmButton;

            var errorProvider = new ErrorProvider(inputForm)
            {
                BlinkStyle = ErrorBlinkStyle.BlinkIfDifferentError,
            };

            inputForm.FormClosing += new FormClosingEventHandler((s, e) =>
            {
                if (inputForm.DialogResult != DialogResult.OK)
                    return;

                errorProvider.Clear();

                if (!File.Exists(tarFilePath.Text))
                {
                    errorProvider.SetError(tarFilePath, "Selected file does not exists.");
                    tarFilePath.Focus();
                    e.Cancel = true;
                    return;
                }

                if (string.IsNullOrWhiteSpace(installDirPath.Text))
                {
                    errorProvider.SetError(distroNameValue, "Install path required.");
                    installDirPath.Focus();
                    e.Cancel = true;
                    return;
                }

                if (string.IsNullOrWhiteSpace(distroNameValue.Text))
                {
                    errorProvider.SetError(distroNameValue, "Distro name required.");
                    distroNameValue.Focus();
                    e.Cancel = true;
                    return;
                }
                else if (MainForm.GetDistroList().DistroList
                    .Count(x => string.Equals(x.DistroName, distroNameValue.Text, StringComparison.Ordinal))
                    > 0)
                {
                    errorProvider.SetError(distroNameValue, "Already taken distro name.");
                    distroNameValue.Focus();
                    e.Cancel = true;
                    return;
                }

                inputForm.Tag = new DistroRestoreRequest()
                {
                    DistroName = distroNameValue.Text,
                    TarFilePath = tarFilePath.Text,
                    RestoreDirPath = installDirPath.Text,
                    SetAsDefault = setAsDefaultCheckBox.Checked,
                };
            });

            return inputForm;
        }
    }
}
