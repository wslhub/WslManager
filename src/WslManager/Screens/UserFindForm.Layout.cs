using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using WslManager.Extensions;
using static WslManager.Extensions.WinFormExtensions;

namespace WslManager.Screens
{
    // Layout
    partial class UserFindForm
    {
        private TableLayoutPanel layout;

        private Label distroNameLabel;
        private TextBox distroNameValue;

        private Label userLabel;
        private ComboBox userList;

        private FlowLayoutPanel actionPanel;
        private Button cancelButton;
        private Button confirmButton;

        protected override void InitializeUserInterface()
        {
            base.InitializeUserInterface();

            this.SetupAsDialog(480, 120, "Find User");

            layout = new TableLayoutPanel()
            {
                Parent = this,
                Dock = DockStyle.Fill,
            }

            .SetupLayout(
                columnStyles: "180px 65% 90px",
                rowStyles: "20% 20% 20%")

            .Place(new object[,]
            {
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
                        ReadOnly = true,
                    }
                    .AssociateLabel(distroNameLabel)
                    .SetTextBoxBinding(this.ViewModel, m => m.DistroName),

                    default,
                },
                {
                    userLabel = new Label()
                    {
                        Text = "Username: ",
                        TextAlign = ContentAlignment.MiddleRight,
                        Anchor = AnchorStyles.Right,
                    },

                    userList = new ComboBox()
                    {
                        Anchor = AnchorStyles.Left | AnchorStyles.Right,
                        DropDownStyle = ComboBoxStyle.DropDownList,
                    }
                    .AssociateLabel(userLabel)
                    .SetComboBoxBinding(this.ViewModel, m => m.User),

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
                                Text = "&Select",
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

            userQueryWorker.RunWorkerAsync(ViewModel);
            FormClosing += RunAsForm_FormClosing;
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

            if (string.IsNullOrWhiteSpace(userList.Text))
            {
                errorProvider.SetError(userList, "Username cannot be empty.");
                userList.Focus();
                e.Cancel = true;
                return;
            }
        }
    }
}
