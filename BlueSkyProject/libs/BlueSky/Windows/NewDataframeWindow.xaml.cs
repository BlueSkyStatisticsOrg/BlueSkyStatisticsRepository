using BSky.Lifetime.Services;
using C1.WPF.FlexGrid;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace BlueSky.Windows
{
    /// <summary>
    /// Interaction logic for NewDataframeWindow.xaml
    /// </summary>
    public partial class NewDataframeWindow : Window
    {
        public NewDataframeWindow()
        {
            InitializeComponent();
        }

        private void FlexSheet1_Loaded(object sender, RoutedEventArgs e)
        {
            if (_rowSize > 0 && _colSize > 0)
                FlexSheet1.AddSheet("Sheet 1", _rowSize, _rowSize);

            //FlexSheet1.Sheets.RemoveAt(0);
        }

        private string _dfName;

        public string DFName
        {
            get { return _dfName; }
            set { _dfName = value; }
        }

        private bool _loadInGrid;

        public bool LoadInGrid
        {
            get { return _loadInGrid; }
            set { _loadInGrid = value; }
        }

        private int _rowSize;

        public int RowSize
        {
            get { return _rowSize; }
            set { _rowSize = value; }
        }

        private int _colSize;

        public int ColSize
        {
            get { return _colSize; }
            set { _colSize = value; }
        }

        private void CreateDataframeBtn_Click(object sender, RoutedEventArgs e)
        {
            CreateLoadDataframe();
        }

        private void CreateLoadDataframeBtn_Click(object sender, RoutedEventArgs e)
        {
            CreateLoadDataframe(true);
        }

        private void CreateLoadDataframe(bool loadInGrid = false)
        {
            DFName = dfname.Text;
            LoadInGrid = loadInGrid;
            UtilityService util = new UtilityService();
            if (!util.isValidObjectname(DFName))
            {
                string m1 = "Please enter a valid dataset name.";
                string m2 = "(no spaces, no special characters and begin with an alphabet)";
                MessageBox.Show(m1 + "\n" + m2, "Invalid Data frame name!", MessageBoxButton.OK, MessageBoxImage.Stop);
                return;
            }
            CopyEditedRange();
            this.Close();
        }


        private void HelpBtn_Click(object sender, RoutedEventArgs e)
        {
            string m1 = "Please make sure:";
            string m2 = "1. Always start from the top left cell";
            string m3 = "2. First row should contain column names only";
            string m4 = "3. Provide a name for your data frame in the field given at the bottom of the window";
            MessageBox.Show(m1 + "\n" + m2 + "\n" + m3 + "\n" + m4, "Help", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        #region Create DF by only selecting the range where user entered values
        int top = 0, left = 0, right, bottom;
        private void FlexSheet1_CellEditEnded(object sender, CellEditEventArgs e)
        {
            CellRange r = FlexSheet1.EditorRange;
            if (r.IsValid && r.IsSingleCell && FlexSheet1[r.BottomRow, r.RightColumn].ToString().Trim().Length > 0)
            {
                if (right < r.RightColumn) right = r.RightColumn;
                if (bottom < r.BottomRow) bottom = r.BottomRow;
            }
        }

        //Copy the range where data is entered by the user
        private void CopyEditedRange()
        {
            FlexSheet1.Selection = new CellRange(0, 0, bottom, right);
            FlexSheet1.Copy();
        }
        #endregion

    }
}
