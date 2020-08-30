using System;
using System.Windows.Forms;
using WslManager.Models;

namespace WslManager.Screens
{
    // List view features
    partial class MainForm
    {
        private void Feature_SetListView_LargeIcon(object sender, EventArgs e)
        {
            listView.View = View.LargeIcon;
        }

        private void Feature_SetListView_SmallIcon(object sender, EventArgs e)
        {
            listView.View = View.SmallIcon;
        }

        private void Feature_SetListView_List(object sender, EventArgs e)
        {
            listView.View = View.List;
        }

        private void Feature_SetListView_Details(object sender, EventArgs e)
        {
            listView.View = View.Details;
        }

        private void Feature_SetListView_Tile(object sender, EventArgs e)
        {
            listView.View = View.Tile;
        }

        private void Feature_SortBy_DistroName(object sender, EventArgs e)
        {
            var targetColumn = listView.AllColumns.Find(
                x => string.Equals(x.Name, nameof(WslDistro.DistroName), StringComparison.Ordinal));

            if (targetColumn != null)
                listView.Sort(targetColumn, listView.PrimarySortOrder);
        }

        private void Feature_SortBy_DistroStatus(object sender, EventArgs e)
        {
            var targetColumn = listView.AllColumns.Find(
                x => string.Equals(x.Name, nameof(WslDistro.DistroStatus), StringComparison.Ordinal));

            if (targetColumn != null)
                listView.Sort(targetColumn, listView.PrimarySortOrder);
        }

        private void Feature_SortBy_WSLVersion(object sender, EventArgs e)
        {
            var targetColumn = listView.AllColumns.Find(
                x => string.Equals(x.Name, nameof(WslDistro.WSLVersion), StringComparison.Ordinal));

            if (targetColumn != null)
                listView.Sort(targetColumn, listView.PrimarySortOrder);
        }

        private void Feature_SortBy_IsDefaultDistro(object sender, EventArgs e)
        {
            var targetColumn = listView.AllColumns.Find(
                x => string.Equals(x.Name, nameof(WslDistro.IsDefault), StringComparison.Ordinal));

            if (targetColumn != null)
                listView.Sort(targetColumn, listView.PrimarySortOrder);
        }

        private void Feature_SortBy_Ascending(object sender, EventArgs e)
        {
            listView.Sort(listView.PrimarySortColumn, SortOrder.Ascending);
        }

        private void Feature_SortBy_Descending(object sender, EventArgs e)
        {
            listView.Sort(listView.PrimarySortColumn, SortOrder.Descending);
        }
    }
}
