using C1.WPF.FlexGrid;
using System.Windows.Controls;

namespace BSky.Controls.XmlDecoder.Model
{
    class FlexGridCellFormatFactory : CellFactory
    {

        public override void ApplyCellStyles(C1FlexGrid grid, CellType cellType, CellRange range, Border bdr)
        {
            // Data cells only
            if (cellType == CellType.Cell)
            {
                object pwr = "";
                var col = grid.Columns[range.Column];
                //same superscript for all cells of a column  
                //pwr = (col.Tag != null) ? col.Tag : "";

                //diff superscript for all cells of a column  
                //pwr = (((grid.Cells[range.Row, range.Column])as GridPanel).Tag != null) ? ((grid.Cells[range.Row, range.Column])as GridPanel).Tag : "";

                //diff superscript for all cells as a 1D array
                if (col.Tag != null)
                {
                    object pwrs = (col.Tag != null) ? col.Tag : "";
                    string[] sprscrow = pwrs as string[];
                    if (sprscrow!=null && (range.Row < sprscrow.Length))
                        pwr = sprscrow[range.Row];
                }
                // retrieve cell value
                var value = grid[range.Row, col];

                ; //Now Format Cell
                Grid g = new Grid();
                StackPanel sp = new StackPanel();
                sp.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
                TextBlock tb1 = new TextBlock(); tb1.Text = (value != null) ? value.ToString() : "-";
                tb1.VerticalAlignment = System.Windows.VerticalAlignment.Center;
                tb1.Margin = new System.Windows.Thickness(0, 0, 4, 0);
                TextBlock tb2 = new TextBlock(); tb2.Text = (value == null) ? pwr.ToString() : ""; tb2.FontSize = 10;
                tb2.VerticalAlignment = System.Windows.VerticalAlignment.Top;
                sp.Orientation = Orientation.Horizontal;
                sp.Children.Add(tb1);
                sp.Children.Add(tb2);
                g.Children.Add(sp);
                bdr.Child = g;
            }
        }
    
    }

}
