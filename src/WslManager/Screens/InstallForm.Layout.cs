using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using WslManager.Extensions;
using WslManager.ViewModels;
using static WslManager.Extensions.WinFormExtensions;

namespace WslManager.Screens
{
    // Layout
    partial class InstallForm
    {
        private TableLayoutPanel layout;

        private Label rootFsUrlLabel;
        private TextBox rootFsUrl;
        private Button rootFsSelectButton;

        private Label installDirLabel;
        private TextBox installDirPath;
        private Button installDirBrowseButton;

        private Label distroNameLabel;
        private TextBox distroNameValue;
        private Button distroNameSuggestButton;

        private Label setAsDefaultLabel;
        private CheckBox setAsDefaultCheckBox;

        private ProgressBar downloadProgressBar;
        private FlowLayoutPanel actionPanel;
        private Button cancelButton;
        private Button confirmButton;

        protected override void InitializeUserInterface()
        {
            base.InitializeUserInterface();

            this.SetupAsDialog(640, 220, "Install Distro");

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
                    rootFsUrlLabel = new Label()
                    {
                        Text = "RootFS URL: ",
                        TextAlign = ContentAlignment.MiddleRight,
                        Anchor = AnchorStyles.Right,
                    },

                    rootFsUrl = new TextBox()
                    {
                        Anchor = AnchorStyles.Left | AnchorStyles.Right,
                        AutoCompleteMode = AutoCompleteMode.SuggestAppend,
                        AutoCompleteSource = AutoCompleteSource.FileSystem,
                    }
                    .AssociateLabel(rootFsUrlLabel)
                    .SetTextBoxBinding(this.ViewModel, m => m.RootFsUrl),

                    rootFsSelectButton = new Button()
                    {
                        Text = "&Catalog...",
                        Anchor = AnchorStyles.Left,
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
                    .SetTextBoxBinding(this.ViewModel, m => m.InstallDirPath),

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
                    .SetTextBoxBinding(this.ViewModel, m => m.NewName),

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
                        .SetCheckBoxBinding(this.ViewModel, m => m.SetAsDefault),
                    },

                    default,
                },
                {
                    downloadProgressBar = new ProgressBar()
                    {
                        Anchor = AnchorStyles.Left | AnchorStyles.Right,
                        Visible = false,
                    },

                    new TableLayoutCell
                    {
                        ColumnSpan = 2,
                        Control = actionPanel = new FlowLayoutPanel()
                        {
                            Anchor = AnchorStyles.Left | AnchorStyles.Right,
                            FlowDirection = FlowDirection.RightToLeft,
                        }
                        .SetupAsActionPanel(
                            confirmButton = new Button()
                            {
                                Text = "&Install",
                                AutoSize = true,
                            }
                            .SetAsConfirmButton(this),

                            cancelButton = new Button()
                            {
                                Text = "&Cancel",
                                AutoSize = true,
                            }
                            .SetAsCancelButton(this)),
                    },

                    default,
                },
            });

            rootFsSelectButton.Click += RootFsSelectButton_Click;
            installDirBrowseButton.Click += InstallDirBrowseButton_Click;
            distroNameSuggestButton.Click += DistroNameSuggestButton_Click;

            FormClosing += InstallForm_FormClosing;
        }

        private void RootFsSelectButton_Click(object sender, EventArgs e)
        {
            var model = new DistroRootFsFindRequest();

            using var distroFindForm = new DistroFindForm(model);

            if (distroFindForm.ShowDialog(this) != DialogResult.OK)
                return;

            ViewModel.RootFsUrl = model.DistroRootFsUrl;
        }

        private void InstallDirBrowseButton_Click(object sender, EventArgs e)
        {
            if (Directory.Exists(installDirPath.Text))
                distroInstallDirOpenDialog.SelectedPath = installDirPath.Text;
            else
                distroInstallDirOpenDialog.SelectedPath = Path.GetPathRoot(Environment.CurrentDirectory);

            if (distroInstallDirOpenDialog.ShowDialog(this) != DialogResult.OK)
                return;

            installDirPath.Text = distroInstallDirOpenDialog.SelectedPath;
        }

        private void DistroNameSuggestButton_Click(object sender, EventArgs e)
        {
            distroNameValue.Text = NameGenerator.Value.GetRandomName();
        }

        private void InstallForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (this.DialogResult != DialogResult.OK)
                return;

            errorProvider.Clear();

            if (!File.Exists(rootFsUrl.Text))
            {
                errorProvider.SetError(rootFsUrl, "Selected file does not exists.");
                rootFsUrl.Focus();
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
            else if (WslHelpers.GetDistroNames().Contains(distroNameValue.Text, StringComparer.Ordinal))
            {
                errorProvider.SetError(distroNameValue, "Already taken distro name.");
                distroNameValue.Focus();
                e.Cancel = true;
                return;
            }
        }
    }
}
