using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using WslManager.Extensions;
using WslManager.Models;
using static WslManager.Extensions.WinFormExtensions;

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
                rowStyles: "20% 20% 20% 20% 20%")

            .Place(new object[,]
            {
                {
                    tarFileLabel = new Label()
                    {
                        Text = "Backup File: ",
                        TextAlign = ContentAlignment.MiddleRight,
                        Anchor = AnchorStyles.Right,
                    },

                    tarFilePath = new TextBox()
                    {
                        Anchor = AnchorStyles.Left | AnchorStyles.Right,
                        AutoCompleteMode = AutoCompleteMode.SuggestAppend,
                        AutoCompleteSource = AutoCompleteSource.FileSystem,
                    }
                    .AssociateLabel(tarFileLabel)
                    .SetTextBoxBinding(this.Model, m => m.TarFilePath),

                    tarFileOpenButton = new Button()
                    {
                        Text = "&Open...",
                        Anchor = AnchorStyles.Left,
                        Height = tarFilePath.Height,
                    },
                },
                {
                    installDirLabel = new Label()
                    {
                        Text = "Install Directory: ",
                        TextAlign = ContentAlignment.MiddleRight,
                        Anchor = AnchorStyles.Right,
                    },

                    installDirPath = new TextBox()
                    {
                        Anchor = AnchorStyles.Left | AnchorStyles.Right,
                        AutoCompleteMode = AutoCompleteMode.SuggestAppend,
                        AutoCompleteSource = AutoCompleteSource.FileSystemDirectories,
                    }
                    .AssociateLabel(installDirLabel)
                    .SetTextBoxBinding(this.Model, m => m.RestoreDirPath),

                    installDirBrowseButton = new Button()
                    {
                        Text = "&Browse...",
                        Anchor = AnchorStyles.Left,
                    },
                },
                {
                    distroNameLabel = new Label()
                    {
                        Text = "Distro Name: ",
                        TextAlign = ContentAlignment.MiddleRight,
                        Anchor = AnchorStyles.Right,
                    },

                    distroNameValue = new TextBox()
                    {
                        Anchor = AnchorStyles.Left | AnchorStyles.Right,
                        Text = NameGenerator.Value.GetRandomName(),
                    }
                    .AssociateLabel(distroNameLabel)
                    .SetTextBoxBinding(this.Model, m => m.DistroName),

                    distroNameSuggestButton = new Button()
                    {
                        Text = "&Suggest",
                        Anchor = AnchorStyles.Left,
                    },
                },
                {
                    setAsDefaultLabel = new Label()
                    {
                        Text = "Set As Default: ",
                        TextAlign = ContentAlignment.MiddleRight,
                        Anchor = AnchorStyles.Right,
                    },

                    new TableLayoutCell
                    {
                        ColumnSpan = 2,
                        Control = setAsDefaultCheckBox = new CheckBox()
                        {
                            Text = "&Check to set as a default",
                            AutoEllipsis = true,
                            TextAlign = ContentAlignment.MiddleLeft,
                            Anchor = AnchorStyles.Left | AnchorStyles.Right,
                        }
                        .AssociateLabel(setAsDefaultLabel)
                        .SetCheckBoxBinding(this.Model, m => m.SetAsDefault),
                    },

                    default,
                },
                {
                    new TableLayoutCell
                    {
                        ColumnSpan = 3,
                        Control = actionPanel = new FlowLayoutPanel()
                        {
                            Anchor = AnchorStyles.Left | AnchorStyles.Right,
                            FlowDirection = FlowDirection.RightToLeft,
                        },
                    },

                    default,

                    default,
                },
            });

            tarFileOpenButton.Click += TarFileOpenButton_Click;
            installDirBrowseButton.Click += InstallDirBrowseButton_Click;
            distroNameSuggestButton.Click += DistroNameSuggestButton_Click;

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
