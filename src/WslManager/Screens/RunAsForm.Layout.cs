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
    partial class RunAsForm
    {
        private TableLayoutPanel layout;

        private Label distroNameLabel;
        private ComboBox distroNameValue;

        private Label userLabel;
        private TextBox user;
        private Button searchUserButton;

        private Label execCommandLabel;
        private TextBox execCommand;

        private FlowLayoutPanel actionPanel;
        private Button cancelButton;
        private Button confirmButton;

        protected override void InitializeUserInterface()
        {
            base.InitializeUserInterface();

            this.SetupAsDialog(480, 160, "Run As Distro");

            layout = new TableLayoutPanel()
            {
                Parent = this,
                Dock = DockStyle.Fill,
            }

            .SetupLayout(
                columnStyles: "180px 65% 90px",
                rowStyles: "20% 20% 20% 20%")

            .Place(new object[,]
            {
                {
                    distroNameLabel = new Label()
                    {
                        Text = "Distro Name: ",
                        TextAlign = ContentAlignment.MiddleRight,
                        Anchor = AnchorStyles.Right,
                    },

                    distroNameValue = new ComboBox()
                    {
                        Anchor = AnchorStyles.Left | AnchorStyles.Right,
                        DropDownStyle = ComboBoxStyle.DropDownList,
                    }
                    .AssociateLabel(distroNameLabel)
                    .SetComboBoxBinding(this.ViewModel, m => m.DistroName, ViewModel.DistroList),

                    default,
                },
                {
                    userLabel = new Label()
                    {
                        Text = "Username: ",
                        TextAlign = ContentAlignment.MiddleRight,
                        Anchor = AnchorStyles.Right,
                    },

                    user = new TextBox()
                    {
                        Anchor = AnchorStyles.Left | AnchorStyles.Right,
                    }
                    .AssociateLabel(userLabel)
                    .SetTextBoxBinding(this.ViewModel, m => m.User),

                    searchUserButton = new Button()
                    {
                        Text = "&Search...",
                        Anchor = AnchorStyles.Left,
                        Height = user.Height,
                    },
                },
                {
                    execCommandLabel = new Label()
                    {
                        Text = "Command Line: ",
                        TextAlign = ContentAlignment.MiddleRight,
                        Anchor = AnchorStyles.Right,
                    },

                    execCommand = new TextBox()
                    {
                        Anchor = AnchorStyles.Left | AnchorStyles.Right,
                    }
                    .AssociateLabel(execCommandLabel)
                    .SetTextBoxBinding(this.ViewModel, m => m.ExecCommandLine),

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
                        }
                        .SetupAsActionPanel(
                            confirmButton = new Button()
                            {
                                Text = "&Run",
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

                    default,
                },
            });

            searchUserButton.Click += SearchUserButton_Click;

            FormClosing += RunAsForm_FormClosing;
        }

        private void SearchUserButton_Click(object sender, EventArgs e)
        {
            var model = new DistroUserFindRequest()
            {
                DistroName = ViewModel.DistroName,
                User = ViewModel.User,
            };

            using var userSearchForm = new UserFindForm(model);

            if (userSearchForm.ShowDialog(this) != DialogResult.OK)
                return;

            ViewModel.User = model.User;
        }

        private void RunAsForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (this.DialogResult != DialogResult.OK)
                return;

            errorProvider.Clear();

            if (string.IsNullOrWhiteSpace(distroNameValue.Text))
            {
                errorProvider.SetError(distroNameValue, "Distro name cannot be empty.");
                distroNameValue.Focus();
                e.Cancel = true;
                return;
            }

            if (string.IsNullOrWhiteSpace(user.Text))
            {
                errorProvider.SetError(user, "Username cannot be empty.");
                user.Focus();
                e.Cancel = true;
                return;
            }
        }
    }
}
