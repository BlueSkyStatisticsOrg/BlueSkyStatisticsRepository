using System;
using C1.WPF.FlexGrid;
using System.Windows.Media;
using System.ComponentModel;
using System.Collections.Generic;
using System.Windows.Input;

namespace BSky.Controls
{
    public class AUGrid : C1FlexGrid
    {
        //16Apr2015 So far there is just 'save' function for flexgrid to save as CSV/HTML/text. No direct export to excel.
        public AUGrid()
        {
            this.Loaded += AUGrid_Loaded;//23Feb2017
            
            this.LayoutUpdated += new EventHandler(AUGrid_LayoutUpdated);
            this.BorderBrush = Brushes.DarkGray;
            this.HeaderGridLinesBrush = Brushes.Gray;
            this.GridLinesBrush = Brushes.LightGray;
            //this.Click += AUGrid_Click;

        }

        //23Feb2017 this should fix the row headers chopping issue
        private void AUGrid_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            //this.AutoSizeFixedColumns(0, 0, 0, true, false);
            this.AutoSizeFixedColumns(0, this.RowHeaders.Columns.Count, 10);//fix for chopped row headers
        }

    void AUGrid_LayoutUpdated(object sender, EventArgs e)
        {
            int extra = 10;
            this.LayoutUpdated -= AUGrid_LayoutUpdated;
            this.AutoSizeFixedColumns(0, this.RowHeaders.Columns.Count, extra); 
            this.AutoSizeColumns(0, this.Columns.Count-1, extra, true, true);
        }

        private void AUGrid_Click(object sender, MouseButtonEventArgs e)
        {
            var ht = this.HitTest(e);

            // sort column when user clicks a column header
            if (ht.CellType == CellType.ColumnHeader)
            {
                var col = this.Columns[ht.Column];
                var sortDirection = (col.Tag is ListSortDirection) && (ListSortDirection)col.Tag ==

                ListSortDirection.Ascending ? ListSortDirection.Descending : ListSortDirection.Ascending;
                col.Tag = sortDirection;
                //implement sort logic on this column
                SortColumn(col, sortDirection);
                foreach (C1.WPF.FlexGrid.Column column in this.Columns)
                {
                    if ((column.Index != col.Index))
                    {
                        column.Tag = null;
                    }
                }
            }

            // toggle Boolean cells even when the click is outside the box
            if (ht.CellType == CellType.Cell)
            {
                // check that it is a Boolean cell
                if (this.Columns[ht.Column].DataType == typeof(bool))
                {
                    // check that it is a single cell (not when extending the selection)
                    if (this.Selection.IsSingleCell)
                    {
                        // update Boolean value
                        var value = this[ht.Row, ht.Column];
                        this[ht.Row, ht.Column] = value is bool && Convert.ToBoolean(value) ? false : true;

                        // we're done with this event
                        e.Handled = true;
                    }
                }
            }
        }

        // sort a column given a sort direction
        private void SortColumn(Column col, ListSortDirection sortDirection)
        {
            // build row list
            var list = new List<Row>();
            foreach (C1.WPF.FlexGrid.Row row in this.Rows)
            {
                list.Add(row);
            }

            // sort row list
            list.Sort(new RowComparer(col, sortDirection));

            // re-populate grid with sorted rows
            this.Rows.Clear();
            foreach (C1.WPF.FlexGrid.Row row in list)
            {
                this.Rows.Add(row);
            }
        }

    }

    public class RowComparer : IComparer<Row>
    {
        private Column _col;
        private ListSortDirection _sortDirection;

        public RowComparer(Column col, ListSortDirection sortDirection)
        {
            _col = col;
            _sortDirection = sortDirection;
        }

        public int Compare(C1.WPF.FlexGrid.Row x, C1.WPF.FlexGrid.Row y)
        {
            var v1 = x[_col];
            var v2 = y[_col];

            // compare values
            int cmp = 0;
            if (v1 == null && v2 != null)
            {
                cmp = -1;
            }
            else if (v1 != null && v2 == null)
            {
                cmp = +1;
            }
            else if (v1 is IComparable && v2 is IComparable)
            {
                cmp = ((IComparable)v1).CompareTo(v2);
            }

            // honor sort direction
            if (_sortDirection == ListSortDirection.Descending)
            {
                cmp = -cmp;
            }

            // return result
            return cmp;
        }
    }
}
