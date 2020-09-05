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
    partial class DistroFindForm
    {
        private TableLayoutPanel layout;

        private Label distroRootFsLabel;
        private ComboBox distroRootFsList;

        private FlowLayoutPanel actionPanel;
        private Button cancelButton;
        private Button confirmButton;

        protected override void InitializeUserInterface()
        {
            base.InitializeUserInterface();

            this.SetupAsDialog(480, 120, "Find Distro");

            layout = new TableLayoutPanel()
            {
                Parent = this,
                Dock = DockStyle.Fill,
            }

            .SetupLayout(
                columnStyles: "180px 65% 50px",
                rowStyles: "20% 20%")

            .Place(new object[,]
            {
                {
                    distroRootFsLabel = new Label()
                    {
                        Text = "Distro RootFS: ",
                        TextAlign = ContentAlignment.MiddleRight,
                        Anchor = AnchorStyles.Right,
                    },

                    distroRootFsList = new ComboBox()
                    {
                        Anchor = AnchorStyles.Left | AnchorStyles.Right,
                        DropDownStyle = ComboBoxStyle.DropDownList,
                    }
                    .AssociateLabel(distroRootFsLabel)
                    .SetComboBoxBinding(this.ViewModel, m => m.DistroRootFsUrl,
                        this.ViewModel.RootFsCandidates, nameof(RootFsModel.DisplayName), nameof(RootFsModel.Url)),

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

            distroRootFsList.DisplayMember = nameof(RootFsModel.DisplayName);
            distroRootFsList.ValueMember = nameof(RootFsModel.Url);

            distroCatalogQueryWorker.RunWorkerAsync(ViewModel);
            FormClosing += DistroFindForm_FormClosing;
        }

        private void DistroFindForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (this.DialogResult != DialogResult.OK)
                return;

            errorProvider.Clear();

            if (string.IsNullOrWhiteSpace(distroRootFsList.Text))
            {
                errorProvider.SetError(distroRootFsList, "RootFS URL cannot be empty.");
                distroRootFsList.Focus();
                e.Cancel = true;
                return;
            }
        }
    }
}
