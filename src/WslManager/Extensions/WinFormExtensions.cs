using System;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace WslManager.Extensions
{
    internal static class WinFormExtensions
    {
        public static ListViewItem GetSelectedItem(this ListView listView)
        {
            var items = listView.SelectedItems;

            if (items == null || items.Count < 1)
                return null;

            return items[0];
        }

        public static ToolStripMenuItem AddMenuItem(this ToolStripItemCollection parent, string text)
            => (ToolStripMenuItem)parent.Add(text);

        public static void AddSeparator(this ToolStripItemCollection parent)
            => parent.Add(new ToolStripSeparator());

        public static TFlowLayoutPanel ReverseOrder<TFlowLayoutPanel>(this TFlowLayoutPanel flowLayoutPanel)
            where TFlowLayoutPanel : FlowLayoutPanel
        {
            foreach (var eachControl in flowLayoutPanel.Controls.OfType<Control>().OrderBy(c => flowLayoutPanel.Controls.GetChildIndex(c)))
                eachControl.BringToFront();

            return flowLayoutPanel;
        }

        public static TForm SetupAsDialog<TForm>(
            this TForm form,
            int width, int height, string dialogTitle)
            where TForm : Form
        {
            form.ClientSize = new Size(width, height);
            form.Text = dialogTitle;
            form.StartPosition = FormStartPosition.CenterParent;
            form.Padding = new Padding(5);
            form.ShowIcon = false;
            form.ShowInTaskbar = false;
            form.MinimizeBox = false;
            form.MaximizeBox = false;
            form.FormBorderStyle = FormBorderStyle.FixedDialog;
            form.MinimumSize = new Size(form.Width, form.Height);
            form.MaximumSize = new Size(form.Width, form.Height);
            return form;
        }

        public static TControl PlaceAt<TControl>(this TControl control, TableLayoutPanel layout,
            int row = 0, int column = 0, int rowSpan = 1, int columnSpan = 1)
            where TControl : Control
        {
            layout.SetCellPosition(control, new TableLayoutPanelCellPosition(
                column: Math.Min(layout.ColumnCount, Math.Max(0, column)),
                row: Math.Min(layout.RowCount, Math.Max(0, row))));
            layout.SetColumnSpan(control, Math.Max(1, columnSpan));
            layout.SetRowSpan(control, Math.Max(1, rowSpan));
            return control;
        }

        public static TTableLayoutPanel SetupLayout<TTableLayoutPanel>(
            this TTableLayoutPanel tableLayoutPanel,
            string columnStyles = default,
            string rowStyles = default)
            where TTableLayoutPanel: TableLayoutPanel
        {
            if (tableLayoutPanel.ColumnStyles.Count > 0)
                tableLayoutPanel.ColumnStyles.Clear();

            if (tableLayoutPanel.RowStyles.Count > 0)
                tableLayoutPanel.RowStyles.Clear();

            var columnStyleFragments = (columnStyles ?? string.Empty).Split(new char[] { ' ', '\t', }, StringSplitOptions.RemoveEmptyEntries);
            var rowStyleFragments = (rowStyles ?? string.Empty).Split(new char[] { ' ', '\t', }, StringSplitOptions.RemoveEmptyEntries);
            var floatRegex = new Regex(@"^(^[0-9]*(?:\.[0-9]*)?)", RegexOptions.Compiled);

            foreach (var eachFragment in columnStyleFragments)
            {
                switch (eachFragment)
                {
                    case var absolute when absolute.EndsWith("px", StringComparison.OrdinalIgnoreCase) &&
                        float.TryParse(floatRegex.Match(absolute).Value, out float parsedAbsolute):
                        tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, parsedAbsolute));
                        break;

                    case var percentage when percentage.EndsWith("%", StringComparison.Ordinal) &&
                        float.TryParse(floatRegex.Match(percentage).Value, out float parsedPercentage):
                        tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, parsedPercentage / 100f));
                        break;

                    case var onlyNumeric when float.TryParse(floatRegex.Match(onlyNumeric).Value, out float pasredNumericValue):
                        tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, pasredNumericValue));
                        break;

                    case var auto when auto.Trim().Equals("*"):
                        tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
                        break;

                    default:
                        throw new InvalidDataException($"Unexpected column expression `{eachFragment}` found.");
                }
            }

            foreach (var eachFragment in rowStyleFragments)
            {
                switch (eachFragment)
                {
                    case var absolute when absolute.EndsWith("px", StringComparison.OrdinalIgnoreCase) &&
                        float.TryParse(floatRegex.Match(absolute).Value, out float parsedAbsolute):
                        tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, parsedAbsolute));
                        break;

                    case var percentage when percentage.EndsWith("%", StringComparison.Ordinal) &&
                        float.TryParse(floatRegex.Match(percentage).Value, out float parsedPercentage):
                        tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, parsedPercentage / 100f));
                        break;

                    case var onlyNumeric when float.TryParse(floatRegex.Match(onlyNumeric).Value, out float pasredNumericValue):
                        tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, pasredNumericValue));
                        break;

                    case var auto when auto.Trim().Equals("*"):
                        tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                        break;

                    default:
                        throw new InvalidDataException($"Unexpected row style expression `{eachFragment}` found.");
                }
            }

            tableLayoutPanel.ColumnCount = tableLayoutPanel.ColumnStyles.Count;
            tableLayoutPanel.RowCount = tableLayoutPanel.RowStyles.Count;
            return tableLayoutPanel;
        }
    }
}
