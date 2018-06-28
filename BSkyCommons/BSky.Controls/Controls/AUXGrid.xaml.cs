using BSky.ConfigService.Services;
using BSky.ConfService.Intf.Interfaces;
using BSky.Interfaces.Controls;
using BSky.Interfaces.Interfaces;
using BSky.Lifetime;
using BSky.Lifetime.Interfaces;
using C1.WPF.FlexGrid;
using Microsoft.Win32;
using MSExcelInterop;
using MSWordInteropLib;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace BSky.Controls
{
    /// <summary>
    /// Interaction logic for AUXGrid.xaml
    /// </summary>
    //[DataContract(Name = "Customer", Namespace = "http://www.contoso.com")]
    public partial class AUXGrid : UserControl, IAUControl
    {
        ILoggerService logService = LifetimeService.Instance.Container.Resolve<ILoggerService>();//17Dec2012
        IConfigService confService = LifetimeService.Instance.Container.Resolve<IConfigService>();//23nov2012
        bool AdvancedLogging;

        public AUXGrid()
        {
            InitializeComponent();
            //this.GotFocus += new RoutedEventHandler(AUXGrid_GotFocus);
            //this.MouseDown += new MouseButtonEventHandler(AUXGrid_MouseDown);
            //this.txtHeader.Focusable = true;
            //this.LostFocus += new RoutedEventHandler(AUXGrid_LostFocus);
            AdvancedLogging = AdvancedLoggingService.AdvLog;//08May2017
        }

        void AUXGrid_GotFocus(object sender, RoutedEventArgs e)
        {
            //txtHeader.Focus();
            //ShowBorder = true; 
        }

        void AUXGrid_LostFocus(object sender, RoutedEventArgs e)
        {
            //ShowBorder = false;
            //CellRange cr = new CellRange();
            //cr.Row = -1;
            //cr.Column = -1;
            //cr.Row2 = -1; 
            //cr.Column2 = -1;
            augrid.Select(0, 0, true);
        }

        void AUXGrid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //this.Focus();
            //ShowBorder = true;
            
        }

        #region  Context Menu

        //Export to excel
        private void ContextMenuExportExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            ExportToExcel();
        }
        private void ContextMenuExportCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        //Copy to clipboard
        private void ContextMenuCopyExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            augrid.ClipboardCopyMode = ClipboardCopyMode.IncludeAllHeaders;
            augrid.SelectAll();
            try
            {
                augrid.Copy(); //Old logic of selecting grid and copy that part in clipboard.
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
                MessageBox.Show("Copy to Clipboard not working!", "Clipboard issue", MessageBoxButton.OK, MessageBoxImage.Stop);
            }
        }

        private void ContextMenuCopyCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

#region Copy currently selected cell data to clipboard. Not in use
        private void ContextMenuCopyCellExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            augrid.ClipboardCopyMode = ClipboardCopyMode.None;
            //augrid.SelectedItem as C1FledGridRow         ;
            try
            {
                augrid.Copy(); //Old logic of selecting grid and copy that part in clipboard.
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
                MessageBox.Show("Copy to Clipboard not working!", "Clipboard issue", MessageBoxButton.OK, MessageBoxImage.Stop);
            }
        }

        private void ContextMenuCopyCellCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }
        #endregion

        //Export APA style table to Word
        private void ContextMenuExportAPAWordExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            ExportAPAToWord();
        }
        private void ContextMenuExportAPAWordCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }


        // // // // Export FGrid to PDF (based on APA flag normal or APA will be exported)
        //Export APA style table to Word
        private void ContextMenuExportToPDFExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            ExportFGToPDF();
        }
        private void ContextMenuExportToPDFCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        #endregion

        public bool ShowBorder
        {
            get { return (bool)GetValue(ShowBorderProperty); }
            set { SetValue(ShowBorderProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ShowBorder.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ShowBorderProperty =
            DependencyProperty.Register("ShowBorder", typeof(bool), typeof(AUXGrid), new UIPropertyMetadata(false));


        public AUParagraph Header 
        {
            get
            {
                return txtHeader;
            }
        }

        public AUGrid Grid 
        {
            get
            { 
                return augrid;
            }
        }


        #region IAUControl Members

        public string ControlType
        {
            get { return this.Header.Text; }
            set { }
        }
        public string NodeText
        {
            get { return this.Header.NodeText; }
            set { }
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

        #region Footnotes and Error Section
        private Dictionary<char, string> footnotes;//I guess this was used with templated dialogs
        public Dictionary<char, string> FootNotes
        {
            get
            {
                return footnotes;
            }
            set
            {
                footnotes = value;
                if (value.Keys.Count > 0)
                {
                    AUParagraph footnoteTitle = new AUParagraph();
                    ////gridpanel.Children.Clear();08mar2012
                    ////gridpanel.Children.Add(augrid);
                    footnoteTitle.Margin = new Thickness(0); //3
                    footnoteTitle.Text = "Footnotes:";
                    footnoteTitle.FontSize = 14;
                    gridpanel.Children.Add(footnoteTitle);
                    foreach (KeyValuePair<char, string> keyval in value)
                    {
                        AUParagraph paragraph = new AUParagraph();
                        paragraph.Margin = new Thickness(0);//1
                        paragraph.Text = keyval.Key.ToString().Trim() + ": " + keyval.Value;
                        paragraph.FontSize = 11;
                        gridpanel.Children.Add(paragraph);
                    }
                }
            }
        }

        //private string starFootNotes;
        public string StarFootNotes //for non templated dialogs. For showing Significance Codes as table footer
        {
            get { return this.starText.Text != null ? this.starText.Text : string.Empty; }
            set { this.starText.Text = value; } // this is need when .bsoz file is loaded
        }


        private Dictionary<char, string> metadata;// for error messages. AD 02Mar2012
        public Dictionary<char, string> Metadata
        {
            get
            {
                return metadata;
            }
            set
            {
                metadata = value;
                if (value.Keys.Count > 0)
                {
                    AUParagraph metadataTitle = new AUParagraph();
                    metadataTitle.Margin = new Thickness(0);//3
                    gridpanel.Children.Clear();
                    //1
                    metadataTitle.Text = "Error/Warning:";
                    metadataTitle.FontSize = 14;
                    gridpanel.Children.Add(metadataTitle);
                    //2
                    foreach (KeyValuePair<char, string> keyval in value)
                    {
                        AUParagraph paragraph = new AUParagraph();
                        paragraph.Margin = new Thickness(0);//1
                        paragraph.Text = keyval.Key.ToString().Trim() + ": " + keyval.Value;
                        paragraph.FontSize = 11;
                        gridpanel.Children.Add(paragraph);
                    }
                    //3
                    gridpanel.Children.Add(augrid);//mov this up if error are to b displayed down.n vicce versa
                }
            }
        }


        #endregion

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
            outerborder.BorderBrush = controlsselectedcolor;// new SolidColorBrush(Colors.Transparent);
        }

        #region (last week May2014) New Properties and Methods those can help drawing grid and filling row/col headers and data

        private string[,] _RowHeaders;
        public string[,] RowHeaders 
        {
            // we can also check for null and then if so we can return GetRowHeaders(), in case prop was not set while creating FlexGrid
            // and same for other properties below.
            get { return _RowHeaders; } 
            set { _RowHeaders = value; } 
        }

        private string[,] _ColHeaders;
        public string[,] ColHeaders 
        {
            get { return _ColHeaders; }
            set { _ColHeaders = value; } 
        }

        private string[,] _GridData;
        public string[,] GridData 
        {
            get { return _GridData; }
            set { _GridData = value; } 
        }

        private bool _ShowZeroRows; // not programmed yet
        public bool ShowZeroRows 
        {
            get { return _ShowZeroRows; }
            set { _ShowZeroRows = value; } 
        }

        //Draw and Fill Grid based on values sent in properties above
        public void DrawFillFlexgrid() 
        {
            //string[,] ColHdrArr = new string[4, 7] { {"Test Value = 1", "Test Value = 1", "Test Value = 1","Test Value = 1","Test Value = 1","Test Value = 1", "Total"},
            //                                {"t", "df", "Sig.(2-tailed)", "Mean Diff", "Confidence : 0.95", "Confidence : 0.95", "Total"},
            //                                {"t", "df", "Sig.(2-tailed)", "Mean Diff", "Lower", "Upper", "Total"},
            //                                {"Total", "Total", "Total", "Total", "Total", "Total", "Total"}
            //};

            //string[,] RowHdrArr = new string[8, 5]{ {"A", "M", "S", "S1","Total"},
            //                                        {"A", "M", "S", "S2","Total"},
            //                                        {"A", "M", "S", "S3","Total"},
            //                                        {"A", "F", "S", "S1","Total"},
            //                                        {"A", "F", "S", "S2","Total"},
            //                                        {"A", "F", "S", "S3","Total"},
            //                                        {"A", "F", "S", "S4","Total"},
            //                                        {"Total", "Total", "Total", "Total","Total"}
            //};

            //////// FILLING STRING ARRAY DATA ///// 7 row from rowhdr count, 6 from colhdr count
            //string[,] dataArr = new string[7, 6] {  {"a", "b", "c", "d", "e", "f"},
            //                                        {"a", "b", "c", "d", "e", "f"},
            //                                        {"a", "b", "c", "d", "e", "f"},
            //                                        {"a", "b", "c", "d", "e", "f"},
            //                                        {"a", "b", "c", "d", "e", "f"},
            //                                        {"a", "b", "c", "d", "e", "f"},
            //                                        {"a", "b", "c", "d", "e", "f"}
            //};

            //values set in properties first then those values are used here to generate FlexGrid
            string[,] ColHdrArr = ColHeaders;
            string[,] RowHdrArr = RowHeaders;
            string[,] dataArr = GridData;

            //Create Headers
            AUGrid c1FlexGrid1 = this.Grid;
            ///////////// merge and sizing /////
            c1FlexGrid1.AllowMerging = AllowMerging.ColumnHeaders | AllowMerging.RowHeaders;
            c1FlexGrid1.AllowSorting = true;

            var rowheaders = c1FlexGrid1.RowHeaders;
            var colheaders = c1FlexGrid1.ColumnHeaders;


            colheaders.Rows[0].AllowMerging = true;
            colheaders.Rows[0].HorizontalAlignment = HorizontalAlignment.Center;

            rowheaders.Columns[0].AllowMerging = true; 
            rowheaders.Columns[0].VerticalAlignment = VerticalAlignment.Top;


            /////////////Col Headers//////////
            for (int i = colheaders.Rows.Count; i < ColHdrArr.GetLength(0); i++) //datamatrix.GetLength(0)
            {
                C1.WPF.FlexGrid.Row row = new C1.WPF.FlexGrid.Row();
                colheaders.Rows.Add(row);
                row.AllowMerging = true;
                row.HorizontalAlignment = HorizontalAlignment.Center;
            }
            for (int i = colheaders.Columns.Count; i < ColHdrArr.GetLength(1); i++) // creating col headers
            {
                C1.WPF.FlexGrid.Column col = new C1.WPF.FlexGrid.Column();
                colheaders.Columns.Add(col);
                col.AllowMerging = true; 
            }

            //fill col headers
            bool colheadersexists = ((ColHdrArr != null) && (ColHdrArr.Length >= dataArr.GetLength(1))) ? true : false;//length should be same
            for (int i = 0; i < ColHdrArr.GetLength(0); i++)  //datamatrix.GetLength(0)
                for (int j = 0; j < ColHdrArr.GetLength(1); j++)
                {
                    if (colheadersexists)
                        colheaders[i, j] = ColHdrArr[i, j];
                    else
                        colheaders[i, j] = j + 1;//colheadermatrix[i, j];
                }

            /////////////Row Headers///////////
            for (int i = rowheaders.Columns.Count; i < RowHdrArr.GetLength(1); i++) //datamatrix.GetLength(1)
            {
                C1.WPF.FlexGrid.Column col = new C1.WPF.FlexGrid.Column();
                col.AllowMerging = true; 
                col.VerticalAlignment = VerticalAlignment.Top;
                rowheaders.Columns.Add(col);
            }

            for (int i = rowheaders.Rows.Count; i < RowHdrArr.GetLength(0); i++)
            {
                C1.WPF.FlexGrid.Row row = new C1.WPF.FlexGrid.Row();
                rowheaders.Rows.Add(row);
                row.AllowMerging = true;
            }

            //fill row headers
            bool rowheadersexists = ((RowHdrArr != null) && (RowHdrArr.Length >= dataArr.GetLength(0))) ? true : false;//length should be same

            for (int i = 0; i < RowHdrArr.GetLength(0); i++)
                for (int j = 0; j < RowHdrArr.GetLength(1); j++)  //datamatrix.GetLength(1)
                {
                    if (rowheadersexists)
                        rowheaders[i, j] = RowHdrArr[i, j];
                    else
                        rowheaders[i, j] = i + 1;//colheadermatrix[i, j];
                }


            bool isemptyrow;
            for (int rw = 0; rw < dataArr.GetLength(0); rw++)
            {
                isemptyrow = true;//assuming row is empty
                for (int c = 0; c < dataArr.GetLength(1); c++)
                {
                    if (dataArr[rw, c] != null && dataArr[rw, c].Trim().Length > 0)
                    {
                        c1FlexGrid1[rw, c] = dataArr[rw, c];
                        isemptyrow = false;// if it has atleast one column filled then row is not empty
                    }
                }
                //// hide or remove empty row////
                if (isemptyrow)
                    c1FlexGrid1.Rows[rw].Visible = false;
            }
        }


        //Now if row/col headers  and data is not provided thru above properties and are populated
        //manually, as we were originally doing. Then we need to read all row/col headers and data
        //from the manually created grid.

        private string[,] GetRowHeaders()
        {
            AUGrid c1FlexGrid1 = this.Grid;
            var rowheaders = c1FlexGrid1.RowHeaders;
            int rcount = rowheaders.Rows.Count;
            int ccount = rowheaders.Columns.Count;
            string[,] RHeaders = new string[rcount, ccount];
            for (int i = 0; i < rcount; i++)
            {
                for (int j = 0; j < ccount; j++)
                {
                    if(rowheaders[i, j]!=null)
                    RHeaders[i, j] = rowheaders[i, j].ToString();
                }
            }

            return RHeaders;
        }

        private string[,] GetColHeaders()
        {
            AUGrid c1FlexGrid1 = this.Grid;
            var colheaders = c1FlexGrid1.ColumnHeaders;
            int rcount = colheaders.Rows.Count;
            int ccount = colheaders.Columns.Count;
            string[,] CHeaders = new string[rcount, ccount];
            for (int i = 0; i < rcount; i++)
            {
                for (int j = 0; j < ccount; j++)
                {
                    if(colheaders[i, j]!=null)
                        CHeaders[i, j] = colheaders[i, j].ToString();
                }
            }

            return CHeaders;
        }

        private string[,] GetGridData()
        {
            AUGrid c1FlexGrid1 = this.Grid;
            int rcount = c1FlexGrid1.Rows.Count;
            int ccount = c1FlexGrid1.Columns.Count;
            string[,] GData = new string[rcount, ccount];
            for (int i = 0; i < rcount; i++)
            {
                for (int j = 0; j < ccount; j++)
                {
                    if(c1FlexGrid1[i, j]!=null)
                        GData[i, j] = c1FlexGrid1[i, j].ToString();
                }
            }
            return GData;
        }

        private string[,] GetSuperScript()
        {
            AUGrid c1FlexGrid1 = this.Grid;
            int rcount = c1FlexGrid1.Rows.Count;
            int ccount = c1FlexGrid1.Columns.Count;
            string[,] supscr = new string[rcount, ccount];
            string[] colsprscrpt=null;// = new string[rcount];// col of superscripts
            //for (int i = 0; i < rcount; i++) //rows
            //{
                for (int j = 0; j < ccount; j++) //cols
                {
                    //if (c1FlexGrid1[i, j] != null)
                        colsprscrpt = c1FlexGrid1.Columns[j].Tag != null ? c1FlexGrid1.Columns[j].Tag as string[]: null;//c1FlexGrid1[i, j].ToString();
                    if (colsprscrpt != null)
                    {
                        for (int ri = 0; ri < rcount; ri++)
                        {
                            if(ri < colsprscrpt.Length) // to avoid index out of bounds. Flexgrid has "total" area outside data range
                                supscr[ri, j] = colsprscrpt[ri];
                        }
                    }
                }
            //}
            return supscr;
        }

        #endregion

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
            IAdvancedLoggingService advlog = LifetimeService.Instance.Container.Resolve<IAdvancedLoggingService>(); ;//08May2017
            advlog.RefreshAdvancedLogging();
            AdvancedLogging = AdvancedLoggingService.AdvLog;//08May2017
            //Dynamically reading Flexgrid and getting row/col headers and data. (Not reading them from properties)
            string title = Header.Text;
            string[,] rh = GetRowHeaders();
            string[,] ch = GetColHeaders();
            string[,] gd = GetGridData();
            string[,] supscr = GetSuperScript();
            string errwrn = Metadata!=null ? DictionaryToString(Metadata):null;
            string fotnotes = string.Empty;
            bool templatedDialog = false;//There are only 4-5 templated dialogs and they do not have footer text. If they do we can fix this line.
            if (templatedDialog)
            {
                fotnotes = FootNotes !=null ? DictionaryToString(FootNotes):null;//I think this works for templated dialogs
            }
            else
            {
                fotnotes = this.starText.Text != null ? this.starText.Text : string.Empty;//This works for non-templated dialogs
            }
            if (AdvancedLogging) logService.WriteToLogLevel("Values to be passed to excel: ", LogLevelEnum.Info);
            string allrh = Join2DStringArray("#$", rh);
            string allch = Join2DStringArray("#$", ch);
            string allgd = Join2DStringArray("#$", gd);
            string allsupscr = Join2DStringArray("#$", supscr);
            string ALLVals = string.Format("title= {0} \n ColHeaders={1} \n RowHeaders={2} \n GridData={3} \n ErrWarn={4} \n Footnotes={5} \n SupScr={6}", title, allch, allrh, allgd, errwrn, fotnotes, allsupscr);
            if (AdvancedLogging) logService.WriteToLogLevel("\n"+ALLVals+"\n", LogLevelEnum.Info);

            Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
            if (AdvancedLogging) logService.WriteToLogLevel("Creating object to make call to excel: ", LogLevelEnum.Info);
            try
            {
                if (_MSExcelObj == null)//|| _MSExcelObj.ExcelApp == null || !(_MSExcelObj.ExcelApp.Visible))
                {
                    _MSExcelObj = new MSExportToExcel();
                }
                if (AdvancedLogging) logService.WriteToLogLevel("Before making call to excel: ", LogLevelEnum.Info);
                _MSExcelObj.CreateAUXGrid("D:/test.xls", "testsheet", title, ch, rh, gd, errwrn, fotnotes, supscr);//dynamically read from flexgrid and export to Excel
                if (AdvancedLogging) logService.WriteToLogLevel("After making call to excel: ", LogLevelEnum.Info);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error exporting to Excel. Make sure Excel is installed.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                logService.WriteToLogLevel("Error exporting to Excel. Detailed Message: " + ex.StackTrace, LogLevelEnum.Warn);
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }
            
        }

        private string Join2DStringArray(string joinchar,string[,] twodimarr)
        {
            StringBuilder sb = new StringBuilder();
            int row = twodimarr.GetLength(0);
            int col = twodimarr.GetLength(1);
            for (int i = 0; i < row; i++)
            {
                for (int j = 0; j < col; j++)
                {
                    sb.Append(twodimarr[i, j]);
                    sb.Append(joinchar);
                }
            }
            return sb.ToString();
        }

        // Use Google DLL for exporting to Excel
        private void GoogleExportToExcel()
        {
            //Dynamically reading Flexgrid and getting row/col headers and data. (Not reading them from properties)
            string[,] rh = GetRowHeaders();
            string[,] ch = GetColHeaders();
            string[,] gd = GetGridData();

        }

        private string DictionaryToString(Dictionary<char, string> dict)
        {
            StringBuilder merged = new StringBuilder();
            int size = dict.Count;
            char akey;
            string aval;

            if (size < 1)
                return null;
            //for (int i = 0; i < size; i++)
            //{
            //    KeyValuePair
            //}
            int i = 1;
            foreach(KeyValuePair<char, string> kvp in dict)
            {
                akey = kvp.Key;
                aval = kvp.Value;
                if(i<size)
                    merged.Append(akey + ":" + aval + "\n");
                else
                    merged.Append(akey + ":" + aval);
                i++;

            }
            return merged.ToString();
        }
        #endregion

        #region Export To Word (right now APA style only) -19Mar2018

        MSWordInterop _MSWordObj;
        public MSWordInterop MSWordObj
        {
            //get; 
            set { _MSWordObj = value; }
        }

        private void ExportAPAToWord()
        {
            bool isMSWordExport = true;
            if (isMSWordExport)
            {
                MSExportAPAToWord();
            }
            else
            {
                //Export using Google DLL
            }
        }

        // Use MicroSoft Interop DLL for exporting to Excel
        private void MSExportAPAToWord()
        {
            IAdvancedLoggingService advlog = LifetimeService.Instance.Container.Resolve<IAdvancedLoggingService>(); ;//19Mar2018
            advlog.RefreshAdvancedLogging();
            AdvancedLogging = AdvancedLoggingService.AdvLog;//19Mar2018
            //Dynamically reading Flexgrid and getting row/col headers and data. (Not reading them from properties)
            string title = string.Empty;//"First:"+Header.Text;
            string secTitle = Header.Text;
            string[,] rh = GetRowHeaders();
            string[,] ch = GetColHeaders();
            string[,] gd = GetGridData();
            string tblNo = string.Empty;//"Table 1.1";
            string[,] supscr = GetSuperScript();
            string errwrn = Metadata != null ? DictionaryToString(Metadata) : null;
            string starfootnotes = string.Empty;
            bool templatedDialog = false;//There are only 4-5 templated dialogs and they do not have footer text. If they do we can fix this line.
            if (templatedDialog)
            {
                starfootnotes = FootNotes != null ? DictionaryToString(FootNotes) : null;//I think this works for templated dialogs
            }
            else
            {
                starfootnotes = this.starText.Text != null ? this.starText.Text : string.Empty;//This works for non-templated dialogs
            }
            if (AdvancedLogging) logService.WriteToLogLevel("Values to be passed to MSWord: ", LogLevelEnum.Info);
            string allrh = Join2DStringArray("#$", rh);
            string allch = Join2DStringArray("#$", ch);
            string allgd = Join2DStringArray("#$", gd);
            string allsupscr = Join2DStringArray("#$", supscr);
            string ALLVals = string.Format("title= {0} \n ColHeaders={1} \n RowHeaders={2} \n GridData={3} \n ErrWarn={4} \n Footnotes={5} \n SupScr={6}", title, allch, allrh, allgd, errwrn, starfootnotes, allsupscr);
            if (AdvancedLogging) logService.WriteToLogLevel("\n" + ALLVals + "\n", LogLevelEnum.Info);

            Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
            if (AdvancedLogging) logService.WriteToLogLevel("Creating object to make call to MSWord: ", LogLevelEnum.Info);
            try
            {
                if (_MSWordObj == null)
                {
                    _MSWordObj = new MSWordInterop();
                }
                if (AdvancedLogging) logService.WriteToLogLevel("Before making call to MSWord: ", LogLevelEnum.Info);
                _MSWordObj.GenerateAPATableInWord("test.docx",  ch, rh, gd, title, secTitle, starfootnotes, tblNo);//dynamically read from flexgrid and export APA to MSWord
                if (AdvancedLogging) logService.WriteToLogLevel("After making call to MSWord: ", LogLevelEnum.Info);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error exporting to MSWord. Make sure MSWord is installed.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                logService.WriteToLogLevel("Error exporting to MSWord. Detailed Message: " + ex.StackTrace, LogLevelEnum.Warn);
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }

        }
        #endregion

        #region Export To PDF

        private void ExportFGToPDF()
        {
            //Get OutpuWin ref
            FrameworkElement fe = this.Parent as FrameworkElement;
            if (fe == null)
                return;

            StackPanel outputSP = (fe as StackPanel);
            if (outputSP == null)
                return;

            IOutputWindow owin = outputSP.Tag as IOutputWindow;
            if (owin == null)
                return;

            
            //Get new PDF filename from User
            string newPDFfilename = "ouptut.pdf"; //default

            SaveFileDialog saveasFileDialog = new SaveFileDialog();
            saveasFileDialog.Filter = "Portable Document Format (*.PDF)|*.PDF";
            saveasFileDialog.DefaultExt = ".pdf";
            //CheckBox cbox = new CheckBox();

            //saveasFileDialog.FileName = filename;//////
            bool? result = saveasFileDialog.ShowDialog(owin as Window);//Application.Current.MainWindow);
            if (result.HasValue && result.Value)
            {
                try
                {
                    newPDFfilename = saveasFileDialog.FileName;
                    Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
                    //Export FlexGRid to PDF
                    owin.ExportC1FlexGridToPDF(newPDFfilename, "", this);
                    Mouse.OverrideCursor = null;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(BSky.GlobalResources.Properties.Resources.SaveAsFailed + saveasFileDialog.FileName, BSky.GlobalResources.Properties.Resources.InternalError, MessageBoxButton.OK, MessageBoxImage.Asterisk);
                }
                finally
                {
                    Mouse.OverrideCursor = null;
                }
            }

        }

        #endregion

        private void CommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {

        }

        private void CommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {

        }
    }
}
