using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using BSky.Interfaces.Controls;
using MSExcelInterop;
using BSky.Lifetime;
using BSky.Lifetime.Interfaces;
using System;
using System.Globalization;
using BSky.ConfService.Intf.Interfaces;

namespace BSky.Controls.Controls
{
    /// <summary>
    /// Interaction logic for BSkyNotes.xaml
    /// </summary>
    public partial class BSkyNotes : UserControl, IAUControl
    {
        ILoggerService logService = LifetimeService.Instance.Container.Resolve<ILoggerService>();//17Dec2012
        IConfigService confService = LifetimeService.Instance.Container.Resolve<IConfigService>();//23nov2012

        public BSkyNotes()
        {
            InitializeComponent();

            //default visibility of Notes text and notes expanded grid
            notes.Visibility = Visibility.Hidden; ;// Visibility.Visible;
            myborder.Visibility = Visibility.Visible;

            this.BSkyControlVisibility = System.Windows.Visibility.Collapsed;//23Oct2013

            //already set in xaml
            //this.Focusable = true; //for treeview. to set focus on BSkyNotes on click.
            lostfocus = false;
        }

        bool lostfocus;

        public bool NotesLostFocus
        {
            get { return lostfocus; }
            set
            {
                lostfocus = value;
                ShrinkControl();
            }
        }

        string[,] notesdata;
        public string[,] NotesData
        {
            get { return notesdata; }
            set { notesdata = value; }
        }

        string headertext;
        public string HearderText
        {
            get { return headertext; }
            set { headertext = value; }
        }

        string summaryText; // when this component is shrinked, the text shown at that time.
        public string SummaryText
        {
            get { return notes.Text; }
        }
        uint notessplitposition;
        public uint NotesSplitPosition//vertical line location
        {
            get { return notessplitposition; }
            set { notessplitposition = value; }
        }

        int showrow_index; /// show row text when control is in collapsed mode. set it to -1 for not using it.
        public int ShowRow_Index
        {
            get { return showrow_index; }
            set { showrow_index = value; }
        }

        string collapsedText; /// Text to be shown when component is in collapsed modes. 
        public string CollapsedText
        {
            get { return collapsedText; }
            set { collapsedText = value; }   //"+ " + value + "[Double-Click to Expand]";
        }

        uint leftpart; // cols having small strings 
        public uint LeftPart
        {
            get { return leftpart; }
            set { leftpart = value; }
        }

        uint rightpart; // cols having large string 
        public uint RightPart
        {
            get { return rightpart; }
            set { rightpart = value; }
        }

        public void FillData()
        {
            FillData(notesdata, notessplitposition, showrow_index, headertext);
        }

        private void FillData(string[,] mynotes, uint splitposition, int showoneindex, string headertxt)
        {
            int rows = mynotes.GetLength(0);
            int cols = mynotes.GetLength(1);

            ///// Filling textblock /// if text takes priority over index
            if (collapsedText != null && collapsedText.Length > 0)//27Jun2013 text to set
                notes.Text = collapsedText.Trim();//"+ " + collapsedText + " [Double-Click to Expand]";
            else /// text from particular index
            {
                if (showoneindex < 0 || showoneindex > rows)
                {
                    collapsedText =  mynotes[0, 0] + " " + mynotes[0, 1];// +"[Double-Click to Expand]";
                    
                }
                else
                {
                    collapsedText =  mynotes[showoneindex, 0] + " " + mynotes[showoneindex, 1];// +"[Double-Click to Expand]";
                }
                notes.Text = collapsedText.Trim();// "+ " + collapsedText + "[Double-Click to Expand]"; ;
            }
            //// Filling Header text /////
            heading.Text = headertext;

            //// checking if splitposition is valid or not ////
            if (splitposition < 1 || splitposition >= cols)
            {
                TextBlock errortb = new TextBlock();
                errortb.Text = "Split-bar position should be, between 0 and max. no. of columns in your supplied string.";
                errortb.Foreground = Brushes.CornflowerBlue;
                splitgrid.Children.Add(errortb);
                return;
            }
            //Creating no of rows/cols in grid
            RowDefinition rd = null; ///// ROWS /////
            for (int i = 0; i < rows; i++)
            {
                rd = new RowDefinition(); //rd.Height =GridLength.Auto;

                splitgrid.RowDefinitions.Add(rd);
            }
            ColumnDefinition cd = null;
            
            for (int i = 0; i <= cols; i++) ///// COLS /////
            {
                cd = new ColumnDefinition(); //cd.Width = GridLength.Auto;
                if (i > splitposition)//i == (cols - 1) || i == cols)
                {
                    cd.Width = new GridLength(rightpart, GridUnitType.Star);
                }
                else if (i == splitposition)
                {
                    cd.Width = new GridLength(.1, GridUnitType.Star);
                }
                else
                {
                    cd.Width = new GridLength(leftpart+.3, GridUnitType.Star);
                }
                splitgrid.ColumnDefinitions.Add(cd);
            }


            /// Filling grid ///
            TextBlock tb = null;
            Viewbox vb = null;
            int ci;//col index for mynotes only, during reading data from it.
            for (int r = 0; r < rows; r++)
            {
                ci = 0;
                for (int c = 0; c <= cols; c++)
                {
                    if (c != splitposition)
                    {
                        tb = new TextBlock(); //tb.Background = Brushes.Blue;
                        tb.Text = mynotes[r, ci]; ci++;
                        tb.TextWrapping = TextWrapping.Wrap;
                        
                        tb.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
                        ///////////// Setting Margin /////////////
                        //vb = new Viewbox(); vb.Height = 12; vb.Width = 100;//vb.Stretch = Stretch.Uniform;
                        tb.Margin = new Thickness(7);

                        /////////// Setting which row col will contain this textblock(UIElement) ///////////
                        tb.SetValue(Grid.RowProperty, r);
                        tb.SetValue(Grid.ColumnProperty, c);
                        //vb.Child = tb;

                        splitgrid.Children.Add(tb);
                    }
                    else
                    {
                        System.Windows.Shapes.Rectangle rec = new System.Windows.Shapes.Rectangle();
                        rec.Width = .2; //rec.Height = 3; rec.Margin = new Thickness(1, 1, 1, 1);
                        rec.Fill = Brushes.Black;
                        rec.SetValue(Grid.RowProperty, r);
                        rec.SetValue(Grid.ColumnProperty, c);
                        splitgrid.Children.Add(rec);
                    }
                }
            }
             //splitgrid.ShowGridLines = true;
        }

        #region Mouse events

        private void notes_MouseDown(object sender, MouseButtonEventArgs e)
        {
            ///23Oct2013 Not doing anything on double click on Notes Text
            //if (e.ClickCount == 2)//double click to expand 
            //{
            //    e.Handled = true;
            //    notes.Visibility = Visibility.Hidden;
            //    myborder.Visibility = Visibility.Visible;
            //}
            //else if(e.ChangedButton == MouseButton.Right)//right click to copy command
            //{
            //    Clipboard.SetText(notes.Text);
            //}
        }

        private void splitgrid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            ///23Oct2013 Not doing anything on right click on Notes Expanded control
            //if (e.ClickCount == 1 && e.ChangedButton == MouseButton.Right)//Right click anywhere inside, to collapse
            //{
            //    notes.Visibility = Visibility.Visible;
            //    //splitgrid.Visibility = Visibility.Hidden;
            //    myborder.Visibility = Visibility.Collapsed;
            //}
        }

        private void UserControl_MouseEnter(object sender, MouseEventArgs e)
        {
            string mousehovercol = confService.GetConfigValueForKey("outputmousehovercol");//23nov2012
            byte red = byte.Parse(mousehovercol.Substring(3, 2), NumberStyles.HexNumber);
            byte green = byte.Parse(mousehovercol.Substring(5, 2), NumberStyles.HexNumber);
            byte blue = byte.Parse(mousehovercol.Substring(7, 2), NumberStyles.HexNumber);
            Color c = Color.FromArgb(255, red, green, blue);

            controlsselectedcolor = (SolidColorBrush)outerborder.BorderBrush;//11Nov2013 storing current
            outerborder.BorderBrush = new SolidColorBrush(c);// (Colors.DarkOrange);
        }

        private void UserControl_MouseLeave(object sender, MouseEventArgs e)
        {
            outerborder.BorderBrush = controlsselectedcolor;//putting back earlier stored color //new SolidColorBrush(Colors.Transparent);
        }

        /// <summary>
        /// collapse control
        /// </summary>
        private void ShrinkControl() /// called when lostfocus is set
        {
            if (lostfocus)
            {
                notes.Visibility = Visibility.Visible;
                //splitgrid.Visibility = Visibility.Hidden;
                myborder.Visibility = Visibility.Collapsed;
            }
        }

        private void maingrid_MouseEnter(object sender, MouseEventArgs e)
        {
            lostfocus = false; 
        }

        private void maingrid_MouseLeave(object sender, MouseEventArgs e)
        {
            lostfocus = true; 
        }

        #endregion

        #region IAUControl Members

        public string ControlType
        {
            get;
            set;
        }
        public string NodeText
        {
            get;
            set;
        }

        public Thickness outerborderthickness
        {
            get { return outerborder.BorderThickness; }
            set { outerborder.BorderThickness = value; }
        }

        //05Jun2013
        public SolidColorBrush controlsselectedcolor
        {
            get;
            set;
        }

        //11Nov2013
        public SolidColorBrush controlsmouseovercolor
        {
            get;
            set;
        }

        //11Nov2013
        public SolidColorBrush bordercolor
        {
            get { return (SolidColorBrush)outerborder.BorderBrush; }
            set { outerborder.BorderBrush = value; }
        }

        //23Sep2013 To set visiblity in output window
        public System.Windows.Visibility BSkyControlVisibility
        {
            get { return this.Visibility; }
            set { this.Visibility = value; }
        }

        #endregion

        private void CommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void CommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ExportToExcel();
        }

        #region Export To Excel

        MSExportToExcel _MSExcelObj;
        public MSExportToExcel MSExcelObj 
        { 
            //get; 
            set { _MSExcelObj = value; } 
        }

        private void ExportToExcel()
        {
            bool isMSExport = true;
            if (isMSExport)
            {
                MSExportToExcel();
            }
            else
            {
                //Export using Google DLL
            }
        }

        // Use MicroSoft Interop DLL for exporting to Excel
        private void MSExportToExcel()
        {
            string header = HearderText;
            string[,] notesdataarr = notesdata;
            try
            {
                if (_MSExcelObj == null)// || _MSExcelObj.ExcelApp == null || !(_MSExcelObj.ExcelApp.Visible))
                {
                    _MSExcelObj = new MSExportToExcel();
                }
                _MSExcelObj.ExportBSkyNotes(header, notesdataarr);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error exporting to Excel. Make sure Excel is installed.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                logService.WriteToLogLevel("Error exporting to Excel. Detailed Message: " + ex.Message, LogLevelEnum.Warn);
            }
        }

        // Use Google DLL for exporting to Excel
        private void GoogleExportToExcel()
        {
            string header = HearderText;
            string[,] notesdataarr = notesdata;

        }
        #endregion

        #region Export to PDF

        //public PdfPTable ExportToPDF()
        //{
        //    //Convert NotesData to PDFptable

        //    ////Find dimentions of row header area
        //    //int rowheaderRowCount = RowHeaders.GetLength(0);
        //    //int rowheaderColCount = RowHeaders.GetLength(1);

        //    ////Find dimentions of col header area
        //    //int colheaderRowCount = ColumnHeaders.GetLength(0);
        //    //int colheaderColCount = ColumnHeaders.GetLength(1);

        //    //Find dimentions of data area
        //    int rowcount = NotesData.GetLength(0);
        //    int colcount = NotesData.GetLength(1);

        //    PdfPTable pdfTable = new PdfPTable(colcount);
        //    float[] colWidths = new float[] { 10f, 30f };
        //    pdfTable.SetWidths(colWidths);
        //    pdfTable.SpacingAfter = 30f;
        //    //pdfTable.SplitLate = true;
        //    //pdfTable.SplitRows = false;
        //    //pdfTable.WidthPercentage = 100;


        //    short configFontsize = 10;
        //    string strPDFfontsize = confService.GetConfigValueForKey("PDFTblFontSize");//04Mar2016
        //    if (!Int16.TryParse(strPDFfontsize, out configFontsize)) configFontsize = 10;

        //    Font tblFont = FontFactory.GetFont("Courier New", configFontsize);//,  BaseColor.GRAY);


        //    //Write the Column headers
        //    PdfPCell cell;
        //    //string colheaderstring = string.Empty;
        //    //for (int i = 0; i < colheaderRowCount; i++)
        //    //{
        //    //    //To left blank corner's current row
        //    //    for (int b = 0; b < rowheaderColCount; b++)
        //    //    {
        //    //        cell = new PdfPCell(new Phrase(""));

        //    //        cell.BackgroundColor = new BaseColor(System.Drawing.ColorTranslator.FromHtml("#d1dbe0"));
        //    //        pdfTable.AddCell(cell);
        //    //    }

        //    //    //Col Headers of current row
        //    //    for (int j = 0; j < colheaderColCount; j++)
        //    //    {
        //    //        colheaderstring = ColumnHeaders[i, j].ToString();
        //    //        cell = new PdfPCell(new Phrase(colheaderstring));
        //    //        //default is false.  cell.NoWrap = false;
        //    //        //cell.FixedHeight = 25f;
        //    //        cell.BackgroundColor = new BaseColor(System.Drawing.ColorTranslator.FromHtml("#d1dbe0"));
        //    //        pdfTable.AddCell(cell);
        //    //    }
        //    //}
        //    //pdfTable.HeaderRows = colheaderRowCount;

        //    //Write the Row headers
        //    string rowheaderstring = string.Empty;
        //    for (int i = 0; i < rowcount; i++)// for all rows
        //    {
        //        ////write row headers
        //        //for (int j = 0; j < rowheaderColCount; j++)// Row Headers of current row
        //        //{
        //        //    rowheaderstring = RowHeaders[i, j].ToString();
        //        //    cell = new PdfPCell(new Phrase(rowheaderstring));
        //        //    //cell.FixedHeight = 25f;

        //        //    cell.BackgroundColor = new BaseColor(System.Drawing.ColorTranslator.FromHtml("#d1dbe0"));
        //        //    pdfTable.AddCell(cell);
        //        //}

        //        //Write the data
        //        for (int c = 0; c < colcount; c++)  //data cells of current row
        //        {
        //            try
        //            {
        //                if (NotesData[i, c] != null)
        //                    cell = new PdfPCell(new Phrase(NotesData[i, c].ToString(), tblFont));
        //                else
        //                    cell = new PdfPCell(new Phrase(""));

        //                //cell.FixedHeight = 25f;
        //                if (c == 0)
        //                {
        //                    //cell.Width = 150f;
        //                    cell.BackgroundColor = new BaseColor(System.Drawing.ColorTranslator.FromHtml("#d1dbe0"));
        //                }
        //                pdfTable.AddCell(cell);
        //            }
        //            catch { }
        //        }
        //        pdfTable.CompleteRow();//used when there are less cells (coulumns) in a current row, to mark the end of a row.
        //    }
        //    return pdfTable;
        
        //}

        #endregion
    }
}
