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
    partial class PropertiesForm
    {
        private TableLayoutPanel layout;

        private TabControl tabControl;

        private TabPage generalPage;

        private TabPage usersPage;

        private TabPage detailsPage;
        private PropertyGrid propertyGrid;

        private FlowLayoutPanel actionPanel;
        private Button cancelButton;
        private Button confirmButton;

        protected override void InitializeUserInterface()
        {
            base.InitializeUserInterface();

            this.SetupAsDialog(480, 520, "Distro Properties");

            layout = new TableLayoutPanel()
            {
                Parent = this,
                Dock = DockStyle.Fill,
            }

            .SetupLayout(
                columnStyles: "180px 95%",
                rowStyles: "92% 8%")

            .Place(new object[,]
            {
                {
                    new TableLayoutCell
                    {
                        ColumnSpan = 2,
                        Control = tabControl = new TabControl()
                        {
                            Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom,
                        },
                    },

                    default,
                },
                {
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
                                Text = "&OK",
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

            generalPage = new TabPage()
            {
                Parent = tabControl,
                Text = "General",
                Padding = new Padding(10),
            };

            usersPage = new TabPage()
            {
                Parent = tabControl,
                Text = "Users",
                Padding = new Padding(10),
            };

            detailsPage = new TabPage()
            {
                Parent = tabControl,
                Text = "Details",
                Padding = new Padding(10),
            };

            propertyGrid = new PropertyGrid()
            {
                Parent = detailsPage,
                SelectedObject = ViewModel,
                ToolbarVisible = false,
                Dock = DockStyle.Fill,
                PropertySort = PropertySort.CategorizedAlphabetical,
            };

            FormClosing += RestoreForm_FormClosing;
        }

        private void RestoreForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (this.DialogResult != DialogResult.OK)
                return;

            errorProvider.Clear();
        }
    }
}
