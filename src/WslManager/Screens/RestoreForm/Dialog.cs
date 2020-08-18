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
            var form = this.SetupAsDialog(640, 220, "Restore Distro");

            layout = new TableLayoutPanel()
            {
                Parent = this,
                Dock = DockStyle.Fill,
            }
            .SetupLayout(
                columnStyles: "180px 65% 90px",
                rowStyles: "20% 20% 20% 20% 20%");

            tarFileLabel = new Label()
            {
                Parent = layout,
                Text = "Backup File: ",
                AutoEllipsis = true,
                TextAlign = ContentAlignment.MiddleRight,
                Anchor = AnchorStyles.Right,
            }
            .PlaceAt(layout, column: 0, row: 0);

            tarFilePath = new TextBox()
            {
                Parent = layout,
                Anchor = AnchorStyles.Left | AnchorStyles.Right,
                AutoCompleteMode = AutoCompleteMode.SuggestAppend,
                AutoCompleteSource = AutoCompleteSource.FileSystem,
            }
            .PlaceAt(layout, column: 1, row: 0);
            tarFilePath.DataBindings.Add(nameof(tarFilePath.Text), Model, nameof(Model.TarFilePath), false, DataSourceUpdateMode.OnPropertyChanged);

            tarFileOpenButton = new Button()
            {
                Parent = layout,
                Text = "&Open...",
                Anchor = AnchorStyles.Left,
                Height = tarFilePath.Height,
            }
            .PlaceAt(layout, column: 2, row: 0);

            tarFileOpenButton.Click += TarFileOpenButton_Click;

            installDirLabel = new Label()
            {
                Parent = layout,
                Text = "Install Directory: ",
                AutoEllipsis = true,
                TextAlign = ContentAlignment.MiddleRight,
                Anchor = AnchorStyles.Right,
            }
            .PlaceAt(layout, column: 0, row: 1);

            installDirPath = new TextBox()
            {
                Parent = layout,
                Anchor = AnchorStyles.Left | AnchorStyles.Right,
                AutoCompleteMode = AutoCompleteMode.SuggestAppend,
                AutoCompleteSource = AutoCompleteSource.FileSystemDirectories,
            }
            .PlaceAt(layout, column: 1, row: 1);
            installDirPath.DataBindings.Add(nameof(installDirPath.Text), Model, nameof(Model.RestoreDirPath), false, DataSourceUpdateMode.OnPropertyChanged);

            installDirBrowseButton = new Button()
            {
                Parent = layout,
                Text = "&Browse...",
                Anchor = AnchorStyles.Left,
            }
            .PlaceAt(layout, column: 2, row: 1);
            installDirBrowseButton.Click += InstallDirBrowseButton_Click;

            distroNameLabel = new Label()
            {
                Parent = layout,
                Text = "Distro Name: ",
                AutoEllipsis = true,
                TextAlign = ContentAlignment.MiddleRight,
                Anchor = AnchorStyles.Right,
            }
            .PlaceAt(layout, column: 0, row: 2);

            distroNameValue = new TextBox()
            {
                Parent = layout,
                Anchor = AnchorStyles.Left | AnchorStyles.Right,
                Text = NameGenerator.Value.GetRandomName(),
            }
            .PlaceAt(layout, column: 1, row: 2);
            distroNameValue.DataBindings.Add(nameof(distroNameValue.Text), Model, nameof(Model.DistroName), false, DataSourceUpdateMode.OnPropertyChanged);

            distroNameSuggestButton = new Button()
            {
                Parent = layout,
                Text = "&Suggest",
                Anchor = AnchorStyles.Left,
            }
            .PlaceAt(layout, column: 2, row: 2);

            distroNameSuggestButton.Click += DistroNameSuggestButton_Click;

            setAsDefaultLabel = new Label()
            {
                Parent = layout,
                Text = "Set As Default: ",
                AutoEllipsis = true,
                TextAlign = ContentAlignment.MiddleRight,
                Anchor = AnchorStyles.Right,
            }
            .PlaceAt(layout, column: 0, row: 3);

            setAsDefaultCheckBox = new CheckBox()
            {
                Parent = layout,
                Text = "&Check to set as a default",
                AutoEllipsis = true,
                TextAlign = ContentAlignment.MiddleLeft,
                Anchor = AnchorStyles.Left | AnchorStyles.Right,
            }
            .PlaceAt(layout, column: 1, row: 3, columnSpan: 2);
            setAsDefaultCheckBox.DataBindings.Add(nameof(setAsDefaultCheckBox.Checked), Model, nameof(Model.SetAsDefault), false, DataSourceUpdateMode.OnPropertyChanged);

            actionPanel = new FlowLayoutPanel()
            {
                Parent = layout,
                Anchor = AnchorStyles.Left | AnchorStyles.Right,
                FlowDirection = FlowDirection.RightToLeft,
            }
            .PlaceAt(layout, column: 0, row: 4, columnSpan: 3);

            confirmButton = new Button()
            {
                Parent = actionPanel,
                Text = "&Restore",
                DialogResult = DialogResult.OK,
                AutoSize = true,
            };
            AcceptButton = confirmButton;

            cancelButton = new Button()
            {
                Parent = actionPanel,
                Text = "&Cancel",
                DialogResult = DialogResult.Cancel,
                AutoSize = true,
            };
            CancelButton = cancelButton;

            actionPanel.ReverseOrder();

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
