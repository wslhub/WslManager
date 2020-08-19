using System.Windows.Forms;

namespace WslManager.Extensions
{
    public sealed class TableLayoutCell
    {
        public int ColumnSpan { get; set; } = 1;
        public int RowSpan { get; set; } = 1;
        public Control Control { get; set; } = default;
    }
}
