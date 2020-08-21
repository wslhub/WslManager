using BrightIdeasSoftware;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Forms;

namespace WslManager.Controls
{
    [DesignerCategory(default)]
    public class CustomListView : DataListView
    {
        public CustomListView()
            : base()
        {
            _columnScaleList = new Collection<float>();
        }

        private bool _enableAutoScaleColumn;
        private Collection<float> _columnScaleList;
        private bool _scaling;

        public bool EnableAutoScaleColumn
        {
            get => _enableAutoScaleColumn;
            set
            {
                if (_enableAutoScaleColumn != value)
                    _enableAutoScaleColumn = value;
            }
        }

        public Collection<float> ColumnScaleList
        {
            get => _columnScaleList;
            set => _columnScaleList = value;
        }

        private int[] CalculateAutoColumnWidth()
        {
            var scaleList = new List<float>(_columnScaleList);
            var results = new List<int>();

            for (var i = 0; i < Columns.Count - _columnScaleList.Count; i++)
                scaleList.Add(1f);

            var totalColumnWidth = 0f;

            // Get the sum of all column tags
            for (var i = 0; i < Columns.Count; i++)
                totalColumnWidth += Convert.ToInt32(scaleList[i]);

            for (var i = 0; i < Columns.Count; i++)
            {
                var colPercentage = (Convert.ToInt32(scaleList[i]) / totalColumnWidth);
                results.Add((int)(colPercentage * (ClientRectangle.Width - SystemInformation.VerticalScrollBarWidth)));
            }

            return results.ToArray();
        }

        protected override void OnColumnWidthChanging(ColumnWidthChangingEventArgs e)
        {
            if (_enableAutoScaleColumn)
            {
                var results = CalculateAutoColumnWidth();
                e.Cancel = true;
                e.NewWidth = results[e.ColumnIndex];
            }

            base.OnColumnWidthChanging(e);
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);

            if (_enableAutoScaleColumn &&
                this.View == View.Details)
            {
                if (!_scaling)
                {
                    _scaling = true;

                    var results = CalculateAutoColumnWidth();
                    for (var i = 0; i < Columns.Count; i++)
                        Columns[i].Width = results[i];
                }

                _scaling = false;
            }
        }
    }
}
