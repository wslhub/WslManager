using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using WslManager.Extensions;
using WslManager.Models;

namespace WslManager.Screens.RestoreForm
{
    // Dialog
    partial class RestoreForm
    {
        private TableLayoutPanel layout;

        private Label tarFileLabel;
        private TextBox tarFilePath;
        private Button tarFileOpenButton;

        private Label installDirLabel;
        private TextBox installDirPath;
        private Button installDirBrowseButton;

        private Label distroNameLabel;
        private TextBox distroNameValue;
        private Button distroNameSuggestButton;

        private Label setAsDefaultLabel;
        private CheckBox setAsDefaultCheckBox;

        private FlowLayoutPanel actionPanel;
        private Button cancelButton;
        private Button confirmButton;

        partial void InitializeDialog()
        {
            ClientSize = new Size(640, 220);
            Text = "Restore Distro";
            StartPosition = FormStartPosition.CenterParent;
            Padding = new Padding(5);
            ShowIcon = false;
            ShowInTaskbar = false;
            MinimizeBox = false;
            MaximizeBox = false;
            FormBorderStyle = FormBorderStyle.FixedDialog;

            MinimumSize = new Size(Width, Height);
            MaximumSize = new Size(Width * 2, Height);

            layout = new TableLayoutPanel()
            {
                Parent = this,
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

            tarFileLabel = new Label()
            {
                Parent = layout,
                Text = "Backup File: ",
                AutoEllipsis = true,
                TextAlign = ContentAlignment.MiddleRight,
                Dock = DockStyle.Fill,
            };
            layout.SetCellPosition(tarFileLabel, new TableLayoutPanelCellPosition(0, 0));

            tarFilePath = new TextBox()
            {
                Parent = layout,
                Dock = DockStyle.Fill,
                AutoCompleteMode = AutoCompleteMode.SuggestAppend,
                AutoCompleteSource = AutoCompleteSource.FileSystem,
            };
            tarFilePath.DataBindings.Add(nameof(tarFilePath.Text), Model, nameof(Model.TarFilePath), false, DataSourceUpdateMode.OnPropertyChanged);
            layout.SetCellPosition(tarFilePath, new TableLayoutPanelCellPosition(1, 0));

            tarFileOpenButton = new Button()
            {
                Parent = layout,
                Text = "&Open...",
                Dock = DockStyle.Fill,
            };
            layout.SetCellPosition(tarFileOpenButton, new TableLayoutPanelCellPosition(2, 0));

            tarFileOpenButton.Click += TarFileOpenButton_Click;

            installDirLabel = new Label()
            {
                Parent = layout,
                Text = "Install Directory: ",
                AutoEllipsis = true,
                TextAlign = ContentAlignment.MiddleRight,
                Dock = DockStyle.Fill,
            };
            layout.SetCellPosition(installDirLabel, new TableLayoutPanelCellPosition(0, 1));

            installDirPath = new TextBox()
            {
                Parent = layout,
                Dock = DockStyle.Fill,
                AutoCompleteMode = AutoCompleteMode.SuggestAppend,
                AutoCompleteSource = AutoCompleteSource.FileSystemDirectories,
            };
            installDirPath.DataBindings.Add(nameof(installDirPath.Text), Model, nameof(Model.RestoreDirPath), false, DataSourceUpdateMode.OnPropertyChanged);
            layout.SetCellPosition(installDirPath, new TableLayoutPanelCellPosition(1, 1));

            installDirBrowseButton = new Button()
            {
                Parent = layout,
                Text = "&Browse...",
                Dock = DockStyle.Fill,
            };

            installDirBrowseButton.Click += InstallDirBrowseButton_Click;

            layout.SetCellPosition(installDirBrowseButton, new TableLayoutPanelCellPosition(2, 1));

            distroNameLabel = new Label()
            {
                Parent = layout,
                Text = "Distro Name: ",
                AutoEllipsis = true,
                TextAlign = ContentAlignment.MiddleRight,
                Dock = DockStyle.Fill,
            };
            layout.SetCellPosition(distroNameLabel, new TableLayoutPanelCellPosition(0, 2));

            distroNameValue = new TextBox()
            {
                Parent = layout,
                Dock = DockStyle.Fill,
                Text = NameGenerator.Value.GetRandomName(),
            };
            distroNameValue.DataBindings.Add(nameof(distroNameValue.Text), Model, nameof(Model.DistroName), false, DataSourceUpdateMode.OnPropertyChanged);
            layout.SetCellPosition(distroNameValue, new TableLayoutPanelCellPosition(1, 2));

            distroNameSuggestButton = new Button()
            {
                Parent = layout,
                Text = "&Suggest",
                Dock = DockStyle.Fill,
            };

            distroNameSuggestButton.Click += DistroNameSuggestButton_Click;

            layout.SetCellPosition(distroNameSuggestButton, new TableLayoutPanelCellPosition(2, 2));

            setAsDefaultLabel = new Label()
            {
                Parent = layout,
                Text = "Set As Default: ",
                AutoEllipsis = true,
                TextAlign = ContentAlignment.MiddleRight,
                Dock = DockStyle.Fill,
            };
            layout.SetCellPosition(setAsDefaultLabel, new TableLayoutPanelCellPosition(0, 3));

            setAsDefaultCheckBox = new CheckBox()
            {
                Parent = layout,
                Text = "&Check to set as a default",
                AutoEllipsis = true,
                TextAlign = ContentAlignment.MiddleLeft,
                Dock = DockStyle.Fill,
            };
            setAsDefaultCheckBox.DataBindings.Add(nameof(setAsDefaultCheckBox.Checked), Model, nameof(Model.SetAsDefault), false, DataSourceUpdateMode.OnPropertyChanged);
            layout.SetCellPosition(setAsDefaultCheckBox, new TableLayoutPanelCellPosition(1, 3));
            layout.SetColumnSpan(setAsDefaultCheckBox, 2);

            actionPanel = new FlowLayoutPanel()
            {
                Parent = layout,
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft,
            };
            layout.SetCellPosition(actionPanel, new TableLayoutPanelCellPosition(0, 4));
            layout.SetColumnSpan(actionPanel, 3);

            cancelButton = new Button()
            {
                Parent = actionPanel,
                Text = "&Cancel",
                DialogResult = DialogResult.Cancel,
                AutoSize = true,
            };
            CancelButton = cancelButton;

            confirmButton = new Button()
            {
                Parent = actionPanel,
                Text = "&Restore",
                DialogResult = DialogResult.OK,
                AutoSize = true,
            };
            AcceptButton = confirmButton;

            FormClosing += RestoreForm_FormClosing;
        }

        private void TarFileOpenButton_Click(object sender, EventArgs e)
        {
            var selectedFilePath = tarFilePath.Text;

            if (File.Exists(selectedFilePath))
                distroBackupFileOpenDialog.FileName = selectedFilePath;

            if (distroBackupFileOpenDialog.ShowDialog(this) != DialogResult.OK)
                return;

            tarFilePath.Text = distroBackupFileOpenDialog.FileName;
        }

        private void InstallDirBrowseButton_Click(object sender, EventArgs e)
        {
            if (Directory.Exists(installDirPath.Text))
                distroRestoreDirOpenDialog.SelectedPath = installDirPath.Text;
            else
                distroRestoreDirOpenDialog.SelectedPath = Path.GetPathRoot(Environment.CurrentDirectory);

            if (distroRestoreDirOpenDialog.ShowDialog(this) != DialogResult.OK)
                return;

            installDirPath.Text = distroRestoreDirOpenDialog.SelectedPath;
        }

        private void DistroNameSuggestButton_Click(object sender, EventArgs e)
        {
            distroNameValue.Text = NameGenerator.Value.GetRandomName();
        }

        private void RestoreForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (this.DialogResult != DialogResult.OK)
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
                errorProvider.SetError(installDirPath, "Install path required.");
                installDirPath.Focus();
                e.Cancel = true;
                return;
            }

            if (Directory.GetFileSystemEntries(installDirPath.Text, "*.*", SearchOption.TopDirectoryOnly).Length > 0)
            {
                errorProvider.SetError(installDirPath, "Selected directory is not an empty directory.");
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
            else if (WslExtensions.GetDistroNames().Contains(distroNameValue.Text, StringComparer.Ordinal))
            {
                errorProvider.SetError(distroNameValue, "Already taken distro name.");
                distroNameValue.Focus();
                e.Cancel = true;
                return;
            }
        }
    }
}
