using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Collections;
using C1.WPF.DataGrid;
using BSky.Statistics.Common;
using BSky.Statistics.Service.Engine.Interfaces;
using System.Globalization;
using System.Collections.ObjectModel;
using Microsoft.Practices.Unity;
using BSky.Lifetime;
using BSky.Interfaces.Model;
using BSky.XmlDecoder;
using BSky.Interfaces.Interfaces;
using BlueSky.Model;
using System.Reflection;
using System.Text;
using BSky.Lifetime.Interfaces;
using System.Windows.Controls.Primitives;
using RDotNet;
using BSky.Lifetime.Services;
using BSky.ConfigService.Services;

namespace BlueSky.Windows
{
    /// <summary>
    /// Interaction logic for DataPanel.xaml
    /// </summary>
    public partial class DataPanel : UserControl
    {
        ILoggerService logService = LifetimeService.Instance.Container.Resolve<ILoggerService>();//17Dec2012
        IUnityContainer container = null;
        IUIController controller = null;
        bool AdvancedLogging;
        public DataPanel()
        {
            InitializeComponent();

            container = LifetimeService.Instance.Container;
            controller = container.Resolve<IUIController>();

            InitializeDynamicColIndexes();

            AdvancedLogging = AdvancedLoggingService.AdvLog;//01May2015
            logService.WriteToLogLevel("Adv Log Flag:" + AdvancedLogging.ToString(), LogLevelEnum.Info);
        }

        #region Dynamic Col loading and Scrolling

        //initializes the start and end indexes 
        public void InitializeDynamicColIndexes()
        {
            startcolidx = 0;
            endcolidx = colsToLoad - 1;//zero based index
            if (AdvancedLogging)
                logService.WriteToLogLevel("Re-Initialized: Start Col idx  = " + startcolidx + " : End Col idx  = " + endcolidx, LogLevelEnum.Info);
        }

        ScrollBar sb;

        void gridControl1_Loaded(object sender, RoutedEventArgs e)
        {
            bool DynamicColLoadingByHorizScrollbar = false;

            if (DynamicColLoadingByHorizScrollbar)
            {
                if ((VisualTreeHelper.GetChildrenCount(gridControl1)) > 0)
                {
                    sb = ((((VisualTreeHelper.GetChild(gridControl1, 0) as Border).Child as Grid).Children[0] as Grid).Children[4] as Border).Child as ScrollBar;
                    sb.Scroll += sb_Scroll;

                }
            }

        }

        int startcolidx, endcolidx;
        int totalCols;//total cols a dataset have. 
        int maxcolidx = 0;//zero based index
        readonly int colsToLoad = 15;//at any point this many cols will be loaded in the grid

        readonly int reserved = 15;

        double val = 0;
        int scrollTicks = 1;
        readonly int speedcontroller = 10; //lasrger the value, smaller distance it jumps from the current col index.
        bool speedScrollingON = true;

        void sb_Scroll(object sender, ScrollEventArgs e)
        {
            totalCols = ds.Variables.Count;
            maxcolidx = totalCols - 1;

            //Collecting some property values
            double min = (sender as ScrollBar).Minimum;
            double max = (sender as ScrollBar).Maximum;
            double viewport = (sender as ScrollBar).ViewportSize;
            double thumb = (sender as ScrollBar).Track.Thumb.RenderSize.Width;
            double tracksize = 196;// (thumb / viewport) * (max - min + viewport);

            bool method1 = false;

            logService.WriteToLogLevel("Max = " + max + " : Value = " + (sender as ScrollBar).Value, LogLevelEnum.Info);

            //ScrollType
            ScrollEventType scrolltype = e.ScrollEventType;

            switch (scrolltype)
            {
                case ScrollEventType.SmallIncrement:
                case ScrollEventType.SmallDecrement:
                case ScrollEventType.EndScroll:
                    logService.WriteToLogLevel("Stopped scrolling. Now new Col set will be fetched :", LogLevelEnum.Info);
                    logService.WriteToLogLevel("Current: Start Col idx  = " + startcolidx + " : End Col idx  = " + endcolidx, LogLevelEnum.Info);
                    //Scrolling to right
                    if ((sender as ScrollBar).Value >= max - reserved && (endcolidx < totalCols - 1))
                    {
                        logService.WriteToLogLevel("Fwd ScrollTicks = " + scrollTicks, LogLevelEnum.Info);
                        scrollTicks = scrollTicks / speedcontroller;
                        if (scrollTicks <= 0) scrollTicks = 1; 
                        logService.WriteToLogLevel("Fwd ScrollTicks speedcontrolled = " + scrollTicks, LogLevelEnum.Info);

                        preserveVerticalScroll();
                        IncrementStartEnd(colsToLoad * scrollTicks);
                        GetColsInRange(startcolidx, endcolidx);
                        logService.Info("Current: Start Col idx  = " + startcolidx + " : End Col idx  = " + endcolidx);
                        RefreshGridOnScroll();
                        restoreVerticalScroll();
                        scrollTicks = 1;

                    }
                    else if ((sender as ScrollBar).Value < reserved && (startcolidx > 0))//Scrolling to left
                    {
                        logService.WriteToLogLevel("Bkwd ScrollTicks = " + scrollTicks, LogLevelEnum.Info);
                        scrollTicks = scrollTicks / speedcontroller;
                        if (scrollTicks <= 0) scrollTicks = 1; 
                        logService.WriteToLogLevel("Bkwd ScrollTicks speedcontrolled = " + scrollTicks, LogLevelEnum.Info);

                        preserveVerticalScroll();
                        DecrementStartEnd(colsToLoad * scrollTicks);
                        GetColsInRange(startcolidx, endcolidx);
                        RefreshGridOnScroll();
                        restoreVerticalScroll();
                        scrollTicks = 1;

                    }
                    logService.Info("After Fetching: Start Col idx = " + startcolidx + " : End Col idx = " + endcolidx);
                    break;
            }

            if (speedScrollingON)
            {
                if (((sender as ScrollBar).Value >= max - reserved && (endcolidx < totalCols - 1)) || ((sender as ScrollBar).Value < reserved && (startcolidx > 0)))
                {
                    scrollTicks += 1;
                    logService.WriteToLogLevel("ScrollTicks incremented = " + scrollTicks, LogLevelEnum.Info);
                }
            }
        }

        #region Vertical Scroll: preserve position and restore it
        int selectedrow = -1;
        DataGridViewport dgvp;

        private void preserveVerticalScroll()
        {
            dgvp = gridControl1.Viewport;
            if (AdvancedLogging)
            {
                logService.WriteToLogLevel("Saving Vertical Scroll row index: First:" + dgvp.FirstVisibleRow, LogLevelEnum.Info);
                logService.WriteToLogLevel("Saving Vertical Scroll row index: Last:" + dgvp.LastVisibleRow, LogLevelEnum.Info);
            }
        }

        private void restoreVerticalScroll()
        {
            try
            {
                int currentFirstRowIdx = gridControl1.Viewport.FirstVisibleRow;
                int currentLastRowIdx = gridControl1.Viewport.LastVisibleRow;

                if (AdvancedLogging)
                {
                    logService.WriteToLogLevel("Current First Row index : " + currentFirstRowIdx, LogLevelEnum.Info);
                    logService.WriteToLogLevel("Current Last Row index : " + currentLastRowIdx, LogLevelEnum.Info);
                }

                {
                    if (dgvp != null && dgvp.LastVisibleRow >= 0 && dgvp.LastVisibleColumn >= 0)
                    {
                        if (AdvancedLogging)
                        {
                            logService.WriteToLogLevel("ScrollIntoView: row index: First: " + dgvp.FirstVisibleRow, LogLevelEnum.Info);
                            logService.WriteToLogLevel("ScrollIntoView: row index: Last: " + dgvp.LastVisibleRow, LogLevelEnum.Info);
                        }
                        gridControl1.ScrollIntoView(dgvp.LastVisibleRow, 0);
                    }
                }
            }
            catch (Exception ex)
            {
                logService.WriteToLogLevel("RestoreVerticalScroll: " + ex.Message, LogLevelEnum.Fatal);
            }
        }

        private void restoreVerticalScroll2()
        {
            try
            {
                int currentFirstRowIdx = gridControl1.Viewport.FirstVisibleRow;
                int currentLastRowIdx = gridControl1.Viewport.LastVisibleRow;

                if (AdvancedLogging)
                {
                    logService.WriteToLogLevel("Current First Row index : " + currentFirstRowIdx, LogLevelEnum.Info);
                    logService.WriteToLogLevel("Current Last Row index : " + currentLastRowIdx, LogLevelEnum.Info);
                }
                
                {
                    if (dgvp != null && dgvp.LastVisibleRow >= 0 && dgvp.LastVisibleColumn >= 0)
                    {
                        if (AdvancedLogging)
                        {
                            logService.WriteToLogLevel("ScrollIntoView: row index: First: " + dgvp.FirstVisibleRow, LogLevelEnum.Info);
                            logService.WriteToLogLevel("ScrollIntoView: row index: Last: " + dgvp.LastVisibleRow, LogLevelEnum.Info);
                        }
                        gridControl1.ScrollIntoView(dgvp.LastVisibleRow+1, 0);
                    }
                }
            }
            catch (Exception ex)
            {
                logService.WriteToLogLevel("RestoreVerticalScroll: " + ex.Message, LogLevelEnum.Fatal);
            }
        }


        #endregion

        private void RefreshGridOnScroll()
        {
            ds.StartColindex = startcolidx;
            ds.EndColindex = endcolidx;
            ds.IsPaginationClicked = true; 
            controller.RefreshBothGrids(ds);
            ds.IsPaginationClicked = false;
        }

        //On right scroll load right cols from right of the dataset
        private void IncrementStartEnd(int changeby)
        {
            if (startcolidx < (maxcolidx - changeby) && endcolidx <= (maxcolidx - changeby))
            {
                startcolidx += changeby;
                endcolidx = startcolidx + colsToLoad;  

                if (AdvancedLogging)
                    logService.WriteToLogLevel("New/Next col indexes : " + startcolidx + " : " + endcolidx + ". ChangeBy : " + changeby, LogLevelEnum.Info);
            }
            else //if out of bounds
            {
                endcolidx = maxcolidx;
                startcolidx = endcolidx - colsToLoad;  

                if (AdvancedLogging)
                    logService.WriteToLogLevel("New/Last col indexes : " + startcolidx + " : " + endcolidx + ". ChangeBy : " + changeby, LogLevelEnum.Info);
            }
        }

        //On left scroll load left cols from left of the dataset
        private void DecrementStartEnd(int changeby)
        {
            if (startcolidx > (changeby) && endcolidx > (changeby))
            {
                startcolidx -= changeby;
                endcolidx = startcolidx + colsToLoad; 

                if (AdvancedLogging)
                    logService.WriteToLogLevel("New/Previous col indexes : " + startcolidx + " : " + endcolidx + ". ChangeBy : " + changeby, LogLevelEnum.Info);
            }
            else //if out of bounds
            {
                endcolidx = colsToLoad;
                startcolidx = 0;

                if (AdvancedLogging)
                    logService.WriteToLogLevel("New/First col indexes : " + startcolidx + " : " + endcolidx + ". ChangeBy : " + changeby, LogLevelEnum.Info);
            }
        }

        private void GetColsInRange(int startcolidx, int endcolidx)
        {
            int totalColCount = ds.Variables.Count;

            if (startcolidx >= 0 && startcolidx < totalColCount)
            {
                if (AdvancedLogging)
                    logService.WriteToLogLevel("Getting cols in range : [" + startcolidx + " : " + endcolidx + "]", LogLevelEnum.Info);

                ds.FewVariables.Clear();

                int currentcolidx = 0;

                foreach (DataSourceVariable var in ds.Variables)
                {
                    if (currentcolidx >= startcolidx && currentcolidx <= endcolidx)
                    {
                        DataSourceVariable newdsv = new DataSourceVariable()
                        {
                            Name = var.Name,
                            RName = var.RName,
                            Label = var.Label,
                            DataType = var.DataType,
                            DataClass = var.DataClass,
                            Measure = var.Measure,
                            Width = 4,
                            Decimals = 0,
                            Columns = 8,
                            MissType = var.MissType,
                            RowCount = var.RowCount,
                            Alignment = var.Alignment
                        };
                        ds.FewVariables.Add(newdsv);
                    }
                    currentcolidx++;
                }
            }
        }

        #endregion

        #region Find in Datagrid
        public FindDatagridWindow FindWindow { get; set; }

        private void GetMatchingRowColIndexes()
        {
        }

        private void NextMatch_Click(object sender, RoutedEventArgs e)
        {
            NextMatchClick(matchindex);
            matchindex++;
        }

        DataFrame findResultsDF = null;
        //make a R call to fill the list with matched values
        public void FindGridText(string text, string[] colnames, bool matchcase)
        {
            if (text != null && text.Length > 0)
            {
                MatchIndex = 0; //resetting for the next search.

                object ob = null;
                IAnalyticsService analyticServ = LifetimeService.Instance.Container.Resolve<IAnalyticsService>();

                if (colnames != null && colnames.Length > 0)
                {
                    ob = analyticServ.GetDgridFindResults(text, colnames, matchcase, ds.Name);
                }
                else 
                {
                    ob = analyticServ.GetDgridFindResults(text, null, matchcase, ds.Name);
                }
                if (ob != null)
                {
                    findResultsDF = ob as DataFrame;
                }
            }
        }

        int matchindex = 0;
        public int MatchIndex 
        {
            set { matchindex = value; }
        }
        //FineNext:No R call. 
        public void FindNextGridText()
        {
            NextMatchClick(matchindex);
            matchindex++; //keep track of where we are in the search result list.
        }

        private void NextMatchClick(int matchindx)
        {
            int matchcount = 0;

            if (findResultsDF != null && findResultsDF.RowCount > 0) 
            {
                matchcount = findResultsDF.RowCount;
            }

            if (matchcount == 0)
            {
                MessageBox.Show(BSky.GlobalResources.Properties.UICtrlResources.NoMatchFound, BSky.GlobalResources.Properties.UICtrlResources.NoMatch, MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                if (matchindx < matchcount)
                {
                    int ri;
                    Int32.TryParse(findResultsDF[matchindx, 0].ToString(), out ri);//fetch row idx
                    int ci;
                    Int32.TryParse(findResultsDF[matchindx, 1].ToString(), out ci);//fetch col idx
                    ScrollToNextMatch(ri - 1, ci - 1);
                }
                else
                {
                    MessageBox.Show(BSky.GlobalResources.Properties.UICtrlResources.MatchEndReached, BSky.GlobalResources.Properties.UICtrlResources.NoMatch, MessageBoxButton.OK, MessageBoxImage.Information);
                    this.matchindex = -1;
                }
            }
        }

        //This method will scroll the next matched cell into the visible area, and will also select it.
        private void ScrollToNextMatch(int rowidx, int colidx)
        {
            //load grid if col index is not yet loaded
            bool success = loadMatchedCellColumn(ref colidx);

            if (success)
            {
                if (gridControl1.Rows.Count > rowidx && gridControl1.Columns.Count > colidx) //if row col indexes are within bounds
                {
                    //now bring the matched cell into view and make it selected
                    gridControl1.ScrollIntoView(rowidx, colidx);
                    SwitchSelectionMode("cell");
                    gridControl1.Selection.Clear();
                    gridControl1.CurrentCell = gridControl1[rowidx, colidx];

                    C1.WPF.DataGrid.DataGridCell dgc = gridControl1[rowidx, colidx];
                    gridControl1.Selection.Add(dgc);
                }
                else
                {
                    logService.WriteToLogLevel("Find data grid error : index out of bounds : Row : " + rowidx + " - Col : " + colidx + ".", LogLevelEnum.Error);
                }
            }
        }

        //Datagrid selection mode. Right now we only need 2 modes.
        public void SwitchSelectionMode(string mode)
        {
            if (mode.Equals("row")) //select whole row. I think this is default from C1.
            {
                gridControl1.SelectionMode = C1.WPF.DataGrid.DataGridSelectionMode.SingleRow;
            }
            else if (mode.Equals("cell"))
            {
                gridControl1.SelectionMode = C1.WPF.DataGrid.DataGridSelectionMode.SingleCell;
            }
        }

        //load the column which is supposed to have the 'Find' match
        private bool loadMatchedCellColumn(ref int matchedColidx)
        {
            if (matchedColidx < DS.Variables.Count)
            {
                if (matchedColidx >= startcolidx && matchedColidx <= endcolidx)
                {
                    matchedColidx = matchedColidx - startcolidx;
                }
                else 
                {
                    startcolidx = matchedColidx;
                    endcolidx = matchedColidx + colsToLoad;
                    GetColsInRange(startcolidx, endcolidx);
                    RefreshGridOnScroll();
                    matchedColidx = 0;
                }
                return true;
            }
            else
            {
                MessageBox.Show(BSky.GlobalResources.Properties.UICtrlResources.NoMoreMatch);
                return false; 
            }
        }

        #endregion

        #region Find in Vargrid. Right now we only look for col name ie Name prop of DataSourceVariable

        List<int> matchedColnameIndexes = new List<int>();

        public void FindColNameText(string findtext, bool matchcase)
        {
            int i = 0;
            matchedColnameIndexes.Clear();
            foreach (DataSourceVariable dsv in ds.Variables)
            {
                if (matchcase)
                {
                    if (dsv.Name.Contains(findtext)) 
                        matchedColnameIndexes.Add(i);
                }
                else
                {
                    if (dsv.Name.ToLower().Contains(findtext.ToLower()))  
                        matchedColnameIndexes.Add(i);
                }
                i++;
            }
        }

        int matchvarindex = 0;
        public int MatchVarIndex //This will be used to reset the match counter back to zero
        {
            set { matchvarindex = value; }
        }

        public void FindNextColMatch()
        {
            if (matchedColnameIndexes.Count < 1)
            {
                MessageBox.Show(BSky.GlobalResources.Properties.UICtrlResources.NoMatchFound, BSky.GlobalResources.Properties.UICtrlResources.NoMatch, MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                if (matchvarindex < matchedColnameIndexes.Count)
                {
                    int ri = matchedColnameIndexes[matchvarindex];//fetch row idx
                    int ci = 0; 
                    ScrollToNextColMatch(ri, ci);
                    matchvarindex++;
                }
                else //resetting back to first matched index
                {
                    MessageBox.Show(BSky.GlobalResources.Properties.UICtrlResources.MatchEndReached, BSky.GlobalResources.Properties.UICtrlResources.NoMatch, MessageBoxButton.OK, MessageBoxImage.Information);
                    this.matchvarindex = 0;
                }
            }
        }

        private void ScrollToNextColMatch(int rowidx, int colidx)
        {
            variableGrid.Focus();
            variableGrid.ScrollIntoView(rowidx, 2);
            variableGrid.Selection.Clear();
            variableGrid.CurrentCell = variableGrid[rowidx, colidx];

            C1.WPF.DataGrid.DataGridCell dgc = variableGrid[rowidx, colidx];
            variableGrid.Selection.Add(dgc);
        }
        #endregion

        DatasetLoadingBusyWindow dslbw = null; 

        private DataSource ds;//A. to get ref. of current datasource

        public DataSource DS
        {
            get { return ds; }
            set
            {
                ds = value;
                ds.StartColindex = startcolidx;
                ds.EndColindex = endcolidx;
            }
        }

        private IList data;

        public IList Data
        {
            get { return data; }
            set
            {
                data = value;
                gridControl1.ItemsSource = null;
                gridControl1.AutoGenerateColumns = true;
                gridControl1.ItemsSource = data;
                gridControl1.CanUserAddRows = true;
                gridControl1.CanUserEditRows = true;
            }
        }

        private ObservableCollection<DataSourceVariable> variables;
        //private IList variables;
        public IList Variables
        {
            get { return variables; }
            set
            {
                variables = new ObservableCollection<DataSourceVariable>(value as List<DataSourceVariable>);
                variableGrid.ItemsSource = variables;
            }
        }


        public List<string> sortasccolnames { get; set; } //18Oct2015 names of all ascending cols
        public List<string> sortdesccolnames { get; set; } //18Oct2015 names of all descending cols

        #region Variablegrid
        /// Events related to varaible grid ////

        private string rowid;
        private int rowindex;
        private int varcount = 1;

        private void variableGrid_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            rowid = variableGrid.CurrentCell.Row.DataItem.ToString();//gender
            rowindex = variableGrid.CurrentRow.Index;
        }

        private void variableGrid_BeganEdit(object sender, DataGridBeganEditEventArgs e)
        {
        }

        //bool committedVarCell = false;
        private void variableGrid_CommittingEdit(object sender, DataGridEndingEditEventArgs e)
        {
            // //on cell Edit and clicking elsewhere gives the info about edited cell
            List<string> colLevels = null;
            string cellVal = variableGrid.CurrentCell.Text;//eg..Male or Female
            string cellValue = cellVal != null ? cellVal.Replace("'", @"\'").Replace("\"", @"\'") : string.Empty;

            if (cellValue == null || cellValue.Trim().Length < 1) //Do not create new variable row if variable name is not provided
            {
                return;
            }

            //15Oct2015 Following two lines to access DataType col value from a current row
            DataSourceVariable dd = variableGrid.CurrentRow.DataItem as DataSourceVariable;
            string rcoltype = ((dd.DataType == DataColumnTypeEnum.Numeric) ||
                (dd.DataType == DataColumnTypeEnum.Double) ||
                (dd.DataType == DataColumnTypeEnum.Integer)) ? "double" : "character";

            //duplicate col name
            if (isDuplicateColNameOnRename(cellValue))
            {
                MessageBox.Show(BSky.GlobalResources.Properties.UICtrlResources.DupColMsg, BSky.GlobalResources.Properties.UICtrlResources.DupColNameTitle, MessageBoxButton.OK, MessageBoxImage.Hand);
                dd.Name = dd.RName;
                return;
            }

            string colid = variableGrid.CurrentCell.Column.Header.ToString();//eg..Label

            switch (e.Column.Name)
            {
                case "Name":
                    break;
                case "DataType":
                    if (colid.Equals("DataType")) colid = "Type"; 
                    if (cellValue.Equals("String")) cellValue = "character";
                    if (cellValue.Equals("Numeric") || cellValue.Equals("Int") || cellValue.Equals("Float") || cellValue.Equals("Double")) cellValue = "numeric";
                    if (cellValue.Equals("Bool")) cellValue = "logical";
                    break;
                case "Width":
                    break;
                case "Decimals":
                    break;
                case "Label":
                    break;
                case "Values":
                    colid = "Levels"; 
                    break;

                case "Missing":
                    break;
                case "Columns":
                    break;
                case "Alignment":
                    colid = "Align"; 

                    C1.WPF.DataGrid.DataGridComboBoxColumn col = e.Column as C1.WPF.DataGrid.DataGridComboBoxColumn;
                    C1.WPF.C1ComboBox combo = e.EditingElement as C1.WPF.C1ComboBox;
                    string value = combo.Text;
                    break;
                case "Measure":
                    colLevels = getLevels();

                    break;
                case "Role":
                    break;
                default:
                    break;
            }

            ///// Modifying R side Dataset ////////
            IAnalyticsService analyticServ = LifetimeService.Instance.Container.Resolve<IAnalyticsService>();

            if (rowid == null)//new row
            {
                int rowindex = 0;
                string datagridcolval = ".";
                analyticServ.addNewVariable(cellValue, rcoltype, datagridcolval, rowindex, ds.Name);

                //// Insert on UI side dataset ///
                DataSourceVariable var = new DataSourceVariable();
                var.Name = cellValue; 
                var.Measure = DataColumnMeasureEnum.Nominal;//15Oct2015
                DS.Variables.Add(var);
            }
            else
            {//edit existing row

                string rcolname = (DS.Variables[rowindex] as DataSourceVariable).RName;//23Jun2016 
                UAReturn retval = analyticServ.EditVarGrid(ds.Name, rcolname, colid, cellValue, colLevels);//23Jun2016 
                retval.Success = true;
            }
            ds.Changed = true;
            if (e.Column.Name.Equals("Name"))
            {
                //it would be good if we could verify SUCCESS in 'retval' above and then only this line should execute.
                (Variables[rowindex] as DataSourceVariable).RName = cellValue;
                refreshDataGrid();
            }
        }

        private void variableGrid_SelectionChanged(object sender, DataGridSelectionChangedEventArgs e)
        {

        }

        #region value label popup dialog

        private List<string> getLevels()
        {
            string cellValue = variableGrid.CurrentCell.Text;
            string rowid = variableGrid.CurrentCell.Row.DataItem.ToString();

            /////testing Value Lable popup /////22Sep2011
            foreach (var v in ds.Variables)//search for col name
            {
                if (v.Name.Equals(rowid))//if colname is found
                {
                    return (v.Values);
                }
            }
            return (null);
        }

        private void valueLabelDialog()
        {
            string selectedData = "";
            string cellValue = variableGrid.CurrentCell.Text;
            string rowid = variableGrid.CurrentCell.Row.DataItem.ToString();

            IAnalyticsService analyticServ = LifetimeService.Instance.Container.Resolve<IAnalyticsService>();

            bool hasmap = false;
            bool varfound = false;//variable found in ds.Variables or not
            int varidx = 0;
            /////testing Value Lable popup /////22Sep2011
            ValueLablesDialog fm = new ValueLablesDialog();
            fm.colName = rowid;
            fm.datasetName = ds.Name;
            fm.maxfactors = ds.maxfactor;//setting maximum factor limit.
            string[] dsvals = null;
            DataColumnMeasureEnum measure = DataColumnMeasureEnum.Scale;
            int i = 0;
            ValueLabelDialogMatrix vlmatrix = new ValueLabelDialogMatrix();

            foreach (var v in ds.Variables)//search for col name
            {
                if (v.Name.Equals(rowid))
                {
                    varfound = true;
                    if (v.Values != null && v.Values.Count > 0)//if colname is found
                    {
                        bool isdot = false;
                        bool isblank = false; //18Mar2014
                        int unwanteditems = 0;

                        foreach (var lb in v.Values)
                        {
                            if (lb.ToString().Equals("."))
                            { unwanteditems++; }
                            if (lb.ToString().Trim().Equals(""))
                            { unwanteditems++; }
                        }

                        dsvals = new string[v.Values.Count - unwanteditems];// '.' & '' is excluded
                        foreach (var lbls in v.Values)//get value lables for a column
                        {
                            if (!lbls.ToString().Equals(".") && !lbls.ToString().Trim().Equals("")) // dot should be shown in value lable dialog
                            {
                                dsvals[i] = lbls.ToString();
                                vlmatrix.addLevel(lbls.ToString(), i, true);
                            }
                            i++;
                        }
                        measure = v.Measure;

                        fm.ValueLableListBoxValues = dsvals;//setting in popup
                        fm.oldfactcount = dsvals.Length;

                        //17Apr2014 // for retrieveing stored factor map
                        if (v.factormapList.Count > 0)
                            hasmap = true;
                    }
                    break;
                }
                if (!varfound)
                    varidx++;
            }

            fm.colMeasure = measure;
            fm.changeFrom = measure.ToString();
            fm.OKclicked = false;
            fm.modified = false; 
            fm.vlmatrix = vlmatrix;
            fm.ShowDialog();

            bool isOkclick = fm.OKclicked;
            bool ismodified = fm.modified;

            if (hasmap && fm.changeTo == "Scale")
            {
                foreach (FactorMap fcm in ds.Variables[varidx].factormapList)
                {
                    FactorMap cpyfm = new FactorMap();
                    cpyfm.labels = fcm.textbox; 
                    cpyfm.textbox = fcm.labels;
                    fm.factormapList.Add(cpyfm);
                }
            }
            List<FactorMap> fctmaplst = fm.factormapList;
            measure = fm.colMeasure;
            bool OK_subdialog = false;

            if (isOkclick)
            {
                if (fctmaplst != null && fctmaplst.Count <= DS.maxfactor)//OK from main dailog.
                {
                    fm.Close();
                    //show sub dialog
                    ValueLabelsSubDialog vlsd = null;

                    if (fm.changeFrom == "Scale")
                        vlsd = new ValueLabelsSubDialog(fctmaplst, "Existing Values", "New Labels", "Numeric levels");
                    else
                        vlsd = new ValueLabelsSubDialog(fctmaplst, "Existing Values", "New Labels", "Numeric levels");// reverse text and labels

                    vlsd.ShowDialog();
                    OK_subdialog = vlsd.OKclicked;
                    fctmaplst = vlsd.factormap;
                    vlsd.Close(); 
                    if (OK_subdialog)//ok from sub-dialog
                    {
                        if (fm.changeFrom == "Scale")
                        {
                            //// read changes from UI and set UI vars to take changes ////
                            List<string> vlst = new List<string>();

                            foreach (FactorMap newlvl in fctmaplst)//get value lables for a column
                            {
                                if (!newlvl.textbox.Trim().Equals(""))//blanks are ignored from sub-dialog
                                    vlst.Add(newlvl.textbox);
                            }
                            updateVargridValuesCol(rowid, measure, vlst); // update Values Col using common function

                            //17Apr2014 Saving factormap along with other col porps of DataSourceVariable
                            int varcount = ds.Variables.Count;

                            if (ds.Variables[varidx].Name.Equals(rowid) && ds.Variables[varidx].Values != null && ds.Variables[varidx].Values.Count > 0)//if colname is found
                            {
                                ds.Variables[varidx].factormapList.Clear();
                                foreach (FactorMap fcm in fctmaplst)
                                {
                                    FactorMap copyfm = new FactorMap();
                                    copyfm.labels = fcm.labels;
                                    copyfm.textbox = fcm.textbox;

                                    ds.Variables[varidx].factormapList.Add(copyfm);
                                }
                            }
                        }
                        else
                            updateVargridValuesCol(rowid, measure, null); // update Values Col using common function
                        if (fm.changeFrom == "Scale" && (fm.changeTo == "Nominal" || fm.changeTo == "Ordinal"))
                        {
                            if (OK_subdialog)//ok from sub-dialog
                            {
                                analyticServ.ChangeScaleToNominalOrOrdinal(rowid, fctmaplst, fm.changeTo, ds.Name);
                            }
                        }
                        else if ((fm.changeFrom == "Nominal" || fm.changeFrom == "Ordinal") && fm.changeTo == "Scale")// Nom, Ord to Scale
                        {
                            if (OK_subdialog)//ok from sub-dialog
                            {
                                analyticServ.ChangeNominalOrOrdinalToScale(rowid, fctmaplst, fm.changeTo, ds.Name);
                            }
                        }
                    }//OK from sub-dialog
                }

                else 
                {
                    if (ismodified)//values changed
                    {

                        List<string> vlst = new List<string>();

                        foreach (string newlvl in fm.ValueLableListBoxValues)//get value lables for a column
                        {
                            vlst.Add(newlvl);
                        }

                        List<ValLvlListItem> finalList = vlmatrix.getFinalList(vlst);
                        updateVargridValuesCol(rowid, measure, vlst); // update Values Col using common function
                        //set this new list of levels to R for update.
                        analyticServ.ChangeColumnLevels(rowid, finalList, ds.Name);
                    }//if modified
                }//else .. n2s n2o o2n o2s
            }//if OK on main dialog

            fm.Close();//release resources held by this popup

            ////refreshing datagrid. 
            if (ismodified || OK_subdialog)
            {
                refreshDataGrid();
                variableGrid.Refresh();
            }
        }

        private void updateVargridValuesCol(string rowvarname, DataColumnMeasureEnum measure, List<string> vlst)
        {
            ////setting UI side vars and datasets////
            foreach (var v in ds.Variables)//search for col name
            {
                if (v.Name.Equals(rowvarname))//if colname is found
                {
                    //Update UI side datasource
                    v.Measure = measure;
                    v.Values = vlst;
                    ds.Changed = true;
                    break;
                }
            }
        }

        private void valLabel_Click(object sender, RoutedEventArgs e)
        {
            variableGrid.CancelEdit();//if someone clicked editable cells in var grid and immediately after clicked on value-lable, we must cancel the cell edit.
            object selectedrow = (sender as FrameworkElement).DataContext; // fixed on 07Feb2014 for New C1 DLLs
            int idx = variableGrid.Rows.IndexOf(selectedrow);
            variableGrid.CurrentRow = variableGrid.Rows[idx];
            ChangeLabels(idx);//05Jun2015 valueLabelDialog();

        }

        #endregion

        #region missing val popup dialog

        private void missingValueDialog()
        {
            MissingValuesDialog mv = new MissingValuesDialog();
            int i = 0;
            rowid = variableGrid.CurrentCell.Row.DataItem.ToString();//gender
            foreach (var v in ds.Variables)//search for col name
            {
                if (v.Name.Equals(rowid))//if colname is found
                {
                    if (v.MissType == null) v.MissType = "none"; //replace null with "none"
                    if (!v.MissType.Equals("none"))
                    {

                        mv.misvals = v.Missing;//this must come before setting mv.mistype
                        mv.oldmisvals = v.Missing;//keeping an original copy for tracing changes
                    }
                    mv.mistype = v.MissType;// missing type. "none", "three", "range+1"
                    mv.oldMisType = v.MissType;//keeping an original copy for tracing changes
                    break;
                }
            }

            mv.ShowDialog();

            //check if values are changed and then set ds.Changed = true;
            if (mv.OKClicked && mv.isModified)//missing changed
            {
                foreach (var v in ds.Variables)//search for col name
                {
                    if (v.Name.Equals(rowid))//if colname is found
                    {
                        v.Missing.Clear();
                        if (!mv.mistype.Equals("none"))
                        {
                            v.Missing.AddRange(mv.misvals);
                        }
                        v.MissType = mv.mistype;// missing type. "none", "three", "range+1"
                        break;
                    }
                }

                //set this new list of levels to R for update.
                IAnalyticsService analyticServ = LifetimeService.Instance.Container.Resolve<IAnalyticsService>();
                analyticServ.ChangeMissingVals(rowid, "Missing", mv.misvals, mv.mistype, ds.Name);
                ds.Changed = true;
            }
            mv.Close();//release resources held by this popup

            refreshDataGrid();
            variableGrid.Refresh();
        }


        private void misval_Click(object sender, RoutedEventArgs e)
        {
            object selectedrow = (sender as FrameworkElement).DataContext; // fixed on 07Feb2014 for New C1 DLLs
            int idx = variableGrid.Rows.IndexOf(selectedrow);
            variableGrid.CurrentRow = variableGrid.Rows[idx];

            missingValueDialog();

        }

        #endregion

        //Controlling columns generation. This will set columns based on datatype.
        private void variableGrid_AutoGeneratingColumn(object sender, C1.WPF.DataGrid.DataGridAutoGeneratingColumnEventArgs e)
        {
            e.Column.EditOnSelection = true;//one click edit mode.

            if (e.Property.Name == "Name")
            {
                e.Cancel = false;
            }
            if (e.Property.Name == "DataType")
            {
                e.Column.IsReadOnly = true;
                e.Cancel = false;
            }
            if (e.Property.Name == "DataClass")
            {
                e.Column.IsReadOnly = true;
                e.Cancel = false;
            }
            if (e.Property.Name == "Width")
            {
                e.Cancel = true;
            }
            if (e.Property.Name == "Decimals")
            {
                e.Cancel = true;
            }
            if (e.Property.Name == "Label")
            {
                e.Column.MaxWidth = 200;
                e.Cancel = false;
            }
            if (e.Property.Name == "Values")
            {
                e.Cancel = true;
            }
            if (e.Property.Name == "Missing")
            {
                e.Cancel = true;
            }

            if (e.Property.Name == "MissType")
            {
                e.Cancel = true;
            }
            if (e.Property.Name == "Columns")
            {
                e.Cancel = true;
            }
            if (e.Property.Name == "Alignment")
            {
                C1.WPF.DataGrid.DataGridComboBoxColumn col = (C1.WPF.DataGrid.DataGridComboBoxColumn)e.Column;

                List<string> lst = new List<string>();
                lst.Add("Left");
                lst.Add("Right");
                lst.Add("Center");
                col.ItemsSource = lst;
                col.ItemTemplate = this.FindResource("ComboTemplate") as DataTemplate;
                col.Binding.Converter = new AlignConvertor();
                e.Cancel = true;//hide this
            }
            if (e.Property.Name == "Measure")
            {
                C1.WPF.DataGrid.DataGridComboBoxColumn col = (C1.WPF.DataGrid.DataGridComboBoxColumn)e.Column;

                List<string> lst = new List<string>();
                lst.Add("Nominal");
                lst.Add("Ordinal");
                lst.Add("Scale");
                lst.Add("Too Many Levels");
                col.ItemsSource = lst;
                col.ItemTemplate = this.FindResource("ComboTemplate") as DataTemplate;
                col.Binding.Converter = new MeasureConvertor();
                e.Column.IsReadOnly = true;
                e.Cancel = false;
            }
            if (e.Property.Name == "Role")
            {
                e.Cancel = true; 
            }
            if (e.Property.Name == "RowCount")
            {
                e.Cancel = true;
            }
            if (e.Property.Name == "ImgURL") 
            {
                e.Cancel = true;
            }
            if (e.Property.Name == "SortType") 
            {
                e.Cancel = true;
            }
            if (e.Property.Name == "factormapList") 
            {
                e.Cancel = true;
            }
            if (e.Property.Name == "RName") 
            {
                e.Cancel = true;
            }
            if (e.Property.Name == "XName") 
            {
                e.Cancel = true;
            }
        }

        private void variableGrid_BeginningNewRow(object sender, DataGridBeginningNewRowEventArgs e)
        {
            DataSourceVariable var = new DataSourceVariable();

            string varname = "newvar";
            //getRightClickRowIndex();
            int rowindex = variableGrid.SelectedIndex;

            //checking duplicate var names
            foreach (DataSourceVariable dsv in this.Variables)
            {
                varname = "newvar" + varcount.ToString();
                if (dsv.Name == varname)
                    varcount++;
            }
            var.Name = varname;
            var.Label = varname;
            var.RName = varname;

            IAnalyticsService analyticServ = LifetimeService.Instance.Container.Resolve<IAnalyticsService>();
            analyticServ.addNewVariable(var.Name, "double", ".", rowindex + 1, ds.Name);

            this.Variables.Insert(rowindex, var);
            DS.Variables.Insert(rowindex, var);//one more refresh needed. I guess

            renumberRowHeader(variableGrid);
            ds.Changed = true;
            refreshDataGrid();
        }

        private void variableGrid_CommittingNewRow(object sender, DataGridEndingNewRowEventArgs e)
        {
        }

        private void variableGrid_CurrentCellChanged(object sender, DataGridCellEventArgs e)
        {
        }

        private string delcolname;
        private int delvarindex;

        private void variableGrid_DeletingRows(object sender, DataGridDeletingRowsEventArgs e)
        {
            delvarindex = variableGrid.SelectedIndex;
            delcolname = DS.Variables.ElementAt(variableGrid.SelectedIndex).RName;
        }

        private void variableGrid_RowsDeleted(object sender, DataGridRowsDeletedEventArgs e)
        {
            removeVarGridVariable();
        }

        private void removeVarGridVariable()
        {
            IAnalyticsService analyticServ = LifetimeService.Instance.Container.Resolve<IAnalyticsService>();
            analyticServ.removeVargridColumn(delcolname, ds.Name);//removing R side
            //renumbering
            renumberRowHeader(variableGrid);
            //remove var in UI side datasets
            DataSourceVariable dsv = new DataSourceVariable();
            dsv = ds.Variables.ElementAt(delvarindex);
            ds.Variables.Remove(dsv);
            //refresh
            refreshDataGrid();
        }

        private void variableGrid_Loaded(object sender, RoutedEventArgs e)
        {
            arrangeVarGridCols();
        }

        public void arrangeVarGridCols()
        {
            variableGrid.Columns.ElementAt(0).DisplayIndex = 5;
            variableGrid.Columns.ElementAt(1).DisplayIndex = 5;
            variableGrid.Columns.ElementAt(5).DisplayIndex = 1;
        }

        private void variableGrid_AutoGeneratedColumns(object sender, EventArgs e)
        {

        }

        private void alignCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox combo = (ComboBox)sender;
        }

        private void alignCombo_DropDownClosed(object sender, EventArgs e)
        {
            ComboBox combo = (ComboBox)sender;
        }

        private void variableGrid_LoadedRowHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
        {
            UpdateRow(e.Row);
        }

        private void variableGrid_CommittedNewRow(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
        {

        }

        #endregion

        private bool IsNumericStr(string celltxt)
        {
            bool isnumber = false;
            double i = 0;


            isnumber = double.TryParse(celltxt, out i); //isnumber = int.TryParse(celltxt, out i);
            return isnumber;
        }

        #region Datagrid
        /// Events related to Datagrid /////
        private bool isnewdatarow = false;

        private void gridControl1_BeginningNewRow(object sender, DataGridBeginningNewRowEventArgs e)
        {
            preserveVerticalScroll();//31Jan2018

            isnewdatarow = true;

            int curRowindex = gridControl1.CurrentRow.Index;
            int colcount = ds.Variables.Count;
            string newemptyrow = CreateEmptyRowCollection(colcount);

            {
                string s = gridControl1.CurrentCell.Text;
                IAnalyticsService analyticServ = LifetimeService.Instance.Container.Resolve<IAnalyticsService>();
                analyticServ.AddNewDatagridRow("", s, newemptyrow, gridControl1.SelectedIndex, ds.Name);
                ds.RowCount++;

                ds.Changed = true;
                refreshDataGrid();
                isnewdatarow = false;
            }

            restoreVerticalScroll2();
            e.Cancel = true;
        }

        object rowclassobject = null;
        string newrowdata = null;

        private void gridControl1_CommittingNewRow(object sender, DataGridEndingNewRowEventArgs e)
        {

            Type classtype = (Data as VirtualListDynamic).RowClassType;

            rowclassobject = gridControl1.CurrentRow.DataItem;

            PropertyInfo[] properties = classtype.GetProperties();
            int propcount = properties.Length;
            string[] strrowdata = new string[propcount];
            object colstr = null;

            try
            {
                for (int i = 0; i < propcount; i++)
                {
                    colstr = properties[i].GetValue(rowclassobject, null);
                    if (colstr != null)
                    {
                        strrowdata[i] = colstr.ToString();
                    }
                    else
                    {
                        strrowdata[i] = string.Empty;// or NA or NaN
                    }
                }
            }
            catch (Exception ex)
            {
                logService.WriteToLogLevel("Error reading values from new row: ", LogLevelEnum.Error);
                logService.WriteToLogLevel(ex.Message, LogLevelEnum.Error);
            }
            //using strrowdata create something like c(4,2, "Male", 34, "Good") for passing as an argument in R call
            StringBuilder sb = new StringBuilder("c(");
            string comma = ","; // for separating diff values
            double temp;

            for (int i = 0; i < strrowdata.Length; i++)
            {
                if (i + 1 == strrowdata.Length) //for last item comma is not required.
                    comma = string.Empty;

                if (Double.TryParse(strrowdata[i], out temp))//if  its number
                {
                    sb.Append(strrowdata[i] + comma);
                }
                else//its string
                {
                    sb.Append("'" + strrowdata[i] + "'" + comma);
                }
            }
            sb.Append(")"); //add closing round bracket.

            if (sb.Length > 0)
            {
                newrowdata = sb.ToString();
            }
        }

        private void gridControl1_CommittedNewRow(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
        {

            bool dontReloadWholeDataframeFromR = false;
            if (dontReloadWholeDataframeFromR)
            {
                RefreshGridWithoutReloadingFromR(e.Row.Index);
            }
            else
            {
                RefreshGridFromRAfterAddingNewRowToR(e.Row.Index);
            }
        }

        //Function to update UI grid and send same data to R and there is no need to reload whole grid
        private void RefreshGridWithoutReloadingFromR(int rindex)
        {
            string s = gridControl1.CurrentCell.Text;
            UAReturn result = null;

            ///// Modifying R side data in Dataset ////////
            IAnalyticsService analyticServ = LifetimeService.Instance.Container.Resolve<IAnalyticsService>();

            //19Jun2015 Get R side varname(diff set of valid chars for defining vars) of a variable in C#
            string RVarName = GetRVarName(gridControl1.CurrentColumn.Name);

            //19Jun2015 following adds new row in UI. 
            if (rowclassobject != null)
            {
                (gridControl1.ItemsSource as IList).Add(rowclassobject); //Add new UI row with new values to UI grid
                gridControl1.Reload(true);
                rowclassobject = null;
            }

            if (newrowdata == null)
            {
                newrowdata = "c()";
            }
            else
            {
                //Add new row to R dataframe.
                result = analyticServ.AddNewDatagridRow(RVarName, s, newrowdata, rindex, ds.Name);//19Jun2015
                isnewdatarow = false;
            }
        }

        //Add row to R side and refresh dataset from R dataframe again.
        private void RefreshGridFromRAfterAddingNewRowToR(int rindex)
        {
            string s = gridControl1.CurrentCell.Text;
            UAReturn result = null;

            ///// Modifying R side data in Dataset ////////
            IAnalyticsService analyticServ = LifetimeService.Instance.Container.Resolve<IAnalyticsService>();

            string RVarName = GetRVarName(gridControl1.CurrentColumn.Name);

            if (newrowdata == null)
            {
                newrowdata = "c()";
            }
            else
            {
                result = analyticServ.AddNewDatagridRow(RVarName, s, newrowdata, rindex, ds.Name);//19Jun2015
                isnewdatarow = false;

                ds.RowCount++;
            }

            //23Jun2015
            //refresh grid.
            refreshDataGrid();
        }

        private void gridControl1_CommittingEdit(object sender, DataGridEndingEditEventArgs e)
        {
            string s = gridControl1.CurrentCell.Text;

            if (s.Equals(oldcelldata))
            {
                gridControl1.CancelEdit();
                return;
            }

            string currcolclass = ds.Variables[GetRVarNameIndex(GetRVarName(e.Column.Name))].DataClass;

            if (currcolclass.Equals("POSIXct") || currcolclass.Equals("Date"))
            {
                string dateformat = currcolclass.Equals("POSIXct") ? "yyyy-MM-dd HH:mm:ss" : "yyyy-MM-dd";//handles POSIXct and Date type only.
                DateTime dtt;
                bool b = DateTime.TryParseExact(s, dateformat, CultureInfo.InvariantCulture, DateTimeStyles.None, out dtt);

                if (!b)//invalid date 
                {
                    Window ow = Window.GetWindow(this);

                    MessageBox.Show(ow, BSky.GlobalResources.Properties.UICtrlResources.InvalidDateTimeMsg,
                        BSky.GlobalResources.Properties.UICtrlResources.InvalidDateTimeTitle, MessageBoxButton.OK, MessageBoxImage.Error);
                    gridControl1.CancelEdit();
                    return;
                }
            }

            UAReturn result = null;

            if (s.Trim() == "" || s.Trim() == "<NA>") s = "";//22Jun2015 blanks will be converted to NAs  

            ///// Modifying R side data in Dataset ////////
            IAnalyticsService analyticServ = LifetimeService.Instance.Container.Resolve<IAnalyticsService>();

            //19Sep2014 Get R side varname(diff set of valid chars for defining vars) of a variable in C#
            string RVarName = GetRVarName(e.Column.Name);
            int varidx = GetRVarNameIndex(RVarName);

            if (isnewdatarow)//Add new row
            {

            }
            else //Edit existing
            {
                if (s.Trim().Length > 0 && !IsNumericStr(s) &&
                    ((ds.Variables[varidx].DataClass.Equals("numeric") && ds.Variables[varidx].DataType == DataColumnTypeEnum.Integer) ||
                    (ds.Variables[varidx].DataClass.Equals("numeric") && ds.Variables[varidx].DataType == DataColumnTypeEnum.Double) ||
                    (ds.Variables[varidx].DataClass.Equals("integer") && ds.Variables[varidx].DataType == DataColumnTypeEnum.Integer)
                    ))
                {

                    #region if use types in char value in numeric field ask to convert the col to character type
                    //
                    string msg = "You have entered a non-numeric value for a numeric variable.\n\nDo you want to convert this column to character type?.";
                    MessageBoxResult mbr = MessageBox.Show(msg, "Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                    if (mbr == MessageBoxResult.Yes)//convert to character type
                    {
                        string columnname = (ds.Variables[varidx] as DataSourceVariable).RName;
                        analyticServ.MakeColString(ds.Name, columnname);

                        //refresh only single row or vargrid. 
                        variables[varidx].DataType = DataColumnTypeEnum.Character;
                        variables[varidx].DataClass = "character";
                        //need to update following objects too.
                        ds.Variables[varidx].DataType = DataColumnTypeEnum.Character;
                        ds.Variables[varidx].DataClass = "character";
                    }
                    else
                    {
                        gridControl1.CancelEdit();
                        s = "";
                        return;
                    }
                    #endregion

                }
                BSkyMouseBusyHandler.ShowMouseBusy();
                //19Sep2014 result=analyticServ.EditDatagridCell(e.Column.Name, s, e.Row.Index, ds.Name);
                result = analyticServ.EditDatagridCell(RVarName, s, e.Row.Index, ds.Name);//19Sep2014
                BSkyMouseBusyHandler.HideMouseBusy();
            }
            ds.Changed = true;

        }

        //Anil.19Sep2014
        //This method returns R side variable name for a C# side var name passed
        private string GetRVarName(string Name)
        {
            string RVarname = Name;

            foreach (DataSourceVariable tdsv in ds.Variables)
            {
                if (tdsv.Name.Equals(Name))//if found get RName, else default is already set.
                {
                    RVarname = tdsv.RName;
                    break;
                }
            }
            return RVarname;
        }

        //Get var name index
        private int GetRVarNameIndex(string Name)
        {
            int varidx = -1;//not found
            string RVarname = Name;

            foreach (DataSourceVariable tdsv in ds.Variables)
            {
                varidx++;
                if (tdsv.Name.Equals(Name))//if found get RName, else default is already set.
                {
                    break;
                }
            }
            return varidx;
        }

        private int datagridrowindex;
        private string oldcelldata;

        private void gridControl1_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            datagridrowindex = gridControl1.CurrentCell.Row.Index;//gender
            rowid = gridControl1.CurrentCell.Row.DataItem.ToString();//gender
            rowindex = gridControl1.CurrentRow.Index;
            oldcelldata = gridControl1.CurrentCell.Text;
        }

        private void gridControl1_SelectionChanged(object sender, DataGridSelectionChangedEventArgs e)
        {

            if (gridControl1.CurrentCell != null)//CurrentRow earlier
                datagridrowindex = gridControl1.CurrentCell.Row.Index;

            if ((gridControl1.CurrentCell != null) &&
                (e.AddedRanges.Count == 1 && selectedrow == -1) ||   //first time selection. No row previously selected
                (e.AddedRanges.Count == 1 && e.RemovedRanges.Count == 1)) //changing current selection by clicking. Some row was previously selected
            {
                selectedrow = gridControl1.CurrentCell.Row.Index;
            }
        }


        private void gridControl1_RowsDeleted(object sender, DataGridRowsDeletedEventArgs e)
        {
            IAnalyticsService analyticServ = LifetimeService.Instance.Container.Resolve<IAnalyticsService>();

            analyticServ.RemoveDatagridRow(datagridrowindex, ds.Name, ds.SheetName);//removing R side

            //renumbering
            renumberRowHeader(gridControl1);
        }

        private void gridControl1_LoadedRowHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
        {
            UpdateRow(e.Row);
        }

        private void gridControl1_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
        {
            bool runit = false;

            if (runit)
                foreach (DataSourceVariable dsv in DS.Variables)
                {
                    //////////// Col header sort icon logic ///////18Apr2013
                    StackPanel sp = new StackPanel(); //sp.Background = Brushes.Black;
                    sp.Orientation = Orientation.Horizontal;
                    sp.HorizontalAlignment = HorizontalAlignment.Left;
                    sp.VerticalAlignment = VerticalAlignment.Center;
                    //sp.Margin = new Thickness(1, 1, 1, 1);

                    System.Windows.Controls.Label lb = new System.Windows.Controls.Label(); 
                    lb.Content = dsv.Name;
                    Image b = new Image();
                    b.ToolTip = BSky.GlobalResources.Properties.UICtrlResources.ToolTipCloseDataset; b.Height = 16.0; b.Width = 22.0;

                    string packUri = string.Empty;

                    if (dsv.Name == "accid")
                    {
                        packUri = "pack://application:,,,/BlueSky;component/Images/sorted_check.png";
                    }
                    else if (dsv.SortType < 0)//descending
                    {
                        packUri = "pack://application:,,,/BlueSky;component/Images/cut.png";
                    }
                    else
                    {
                        packUri = "pack://application:,,,/BlueSky;component/Images/center.png";
                    }
                    b.Source = new ImageSourceConverter().ConvertFromString(packUri) as ImageSource;
                    sp.Children.Add(lb);
                    sp.Children.Add(b);
                    e.Column.Header = sp;
                    /////////////////////sort icon logic ends//////////////////
                }
        }
        
        #region Left Mouse click on Col Header for sort
        //06Jun2018
        private void Sp_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            string colname = ((sender as StackPanel).Children[0] as TextBlock).Text;
            LeftMouseDownOnCol(colname);
        }

        private void LeftMouseDownOnCol(string colname)
        {

            colname = GetRVarName(colname); //R side variable name
            string sortorder = "asc";

            #region Sort icon logic
            //get asc colnames from command

            List<string> asccols = new List<string>(); 
            List<string> desccols = new List<string>(); 

            if (sortasccolnames != null && sortasccolnames.Contains(colname))
            {
                desccols.Add(colname);//add to desc list
                sortorder = "desc";
            }
            else if (sortdesccolnames != null && sortdesccolnames.Contains(colname))
            {
                asccols.Add(colname);//add to asc list
                sortorder = "asc";
            }
            else
            {
                asccols.Add(colname);
                sortorder = "asc";
            }

            CommandExecutionHelper che = new CommandExecutionHelper();
            che.SetSortColForSortIcon(asccols, desccols);
            #endregion

            #region Execute Sort in R
            IAnalyticsService analyticServ = LifetimeService.Instance.Container.Resolve<IAnalyticsService>();
            analyticServ.SortDatagridCol(colname, sortorder, ds.Name);//sort on R side
            #endregion

            #region Refreshing datagrid
            //renumbering
            renumberRowHeader(gridControl1);
            ds.Changed = true;
            refreshDataGrid(true);

            #endregion
        }

        #endregion

        private void gridControl1_AutoGeneratingColumn(object sender, C1.WPF.DataGrid.DataGridAutoGeneratingColumnEventArgs e)
        {
            bool issortedcol = false;
            e.Column.EditOnSelection = true;//one click edit mode.
            DataSourceVariable dsv = GetDataSourceVAriable(e.Property.Name);

            if (dsv != null)
            {
                if ((dsv.Name == e.Property.Name) && (dsv.Measure == DataColumnMeasureEnum.Nominal || dsv.Measure == DataColumnMeasureEnum.Ordinal || dsv.Measure == DataColumnMeasureEnum.Logical))//08Feb2017 We only process Nominal/Ordinal
                {
                    List<string> droplst = dsv.Values;
                    ///09Oct2013 We may not need '.'. This topic is under discussion.
                    if (!droplst.Contains("<NA>"))
                        droplst.Add("<NA>");

                    var comboCol = new C1.WPF.DataGrid.DataGridComboBoxColumn(e.Property);

                    comboCol.ItemsSource = droplst;
                    comboCol.EditOnSelection = true;

                    e.Column = comboCol;
                    e.Cancel = false;
                }

                ////  Creating column header with sort icon  ////10Apr2014
                StackPanel colheaderpanel = new StackPanel(); 
                colheaderpanel.Background = Brushes.Transparent;//if you do not set this, click is not working in empty area (around header text).
                colheaderpanel.Orientation = Orientation.Horizontal; 
                colheaderpanel.MouseLeftButtonDown += Sp_MouseLeftButtonDown;
                colheaderpanel.HorizontalAlignment = HorizontalAlignment.Stretch;
                colheaderpanel.VerticalAlignment = VerticalAlignment.Stretch;

                //putting text before sort icon
                TextBlock txb = new TextBlock();
                txb.Text = GetRVarName(e.Property.Name);
                txb.Margin = new Thickness(2);

                /// Putting Sort icon in each column header
                issortedcol = false;
                Image sortico = new Image(); 
                sortico.ToolTip = BSky.GlobalResources.Properties.UICtrlResources.ToolTipSorted; sortico.Height = 16.0; sortico.Width = 22.0; sortico.Margin = new Thickness(2);
                string packUri = null;

                if (sortasccolnames != null && sortasccolnames.Contains(e.Property.Name))
                {
                    packUri = "pack://application:,,,/BlueSky;component/Images/angle-arrow-up.png";
                    sortico.ToolTip = BSky.GlobalResources.Properties.UICtrlResources.ToolTipSorted;
                    issortedcol = true;
                }
                else if (sortdesccolnames != null && sortdesccolnames.Contains(e.Property.Name))
                {
                    packUri = "pack://application:,,,/BlueSky;component/Images/angle-arrow-down.png";
                    sortico.ToolTip = BSky.GlobalResources.Properties.UICtrlResources.ToolTipSorted;
                    issortedcol = true;
                }
                else
                    packUri = "pack://application:,,,/BlueSky;component/Images/sorted_check.png";

                sortico.Source = new ImageSourceConverter().ConvertFromString(packUri) as ImageSource;

                if (!issortedcol)
                    sortico.Visibility = System.Windows.Visibility.Collapsed;

                colheaderpanel.Children.Add(txb);
                colheaderpanel.Children.Add(sortico); 

                e.Column.Header = colheaderpanel;
            }
        }

        private DataSourceVariable GetDataSourceVAriable(string name)
        {
            foreach (DataSourceVariable dsv in DS.Variables)
            {
                if (dsv.Name == name)
                    return dsv;
            }
            return null;
        }

        #endregion

        #region Re-Numbering / Refreshing Grid

        private void renumberRowHeader(C1DataGrid c1grid)//for refresh row header numbers on add/delete.
        {
            ///// renumbering////////
            if (c1grid.Viewport != null)
            {
                foreach (var row in c1grid.Viewport.Rows)
                {
                    if (row.Index != -1 && row != null)
                        UpdateRow(row);
                }
            }
        }

        private static void UpdateRow(C1.WPF.DataGrid.DataGridRow row)
        {
            row.HeaderPresenter.Content = new TextBlock()
            {
                Text = (row.Index + 1).ToString(),
                HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                VerticalAlignment = System.Windows.VerticalAlignment.Center
            };
        }

        private void refreshDataGrid(bool ColClickSort=false)
        {
            BSkyMouseBusyHandler.ShowMouseBusy();
            IDataService service = container.Resolve<IDataService>();


            if (!ColClickSort && variableGrid.CurrentCell != null) 
            {
                string rowid = string.Empty;

                if (variableGrid.CurrentCell.Row.DataItem != null)
                {
                    rowid = variableGrid.CurrentCell.Row.DataItem.ToString();
                }
                else
                {
                    int rowidx = variableGrid.CurrentCell.Row.Index;

                    if (rowidx >= 0)
                        rowid = (this.Variables[rowindex] as DataSourceVariable).RName;
                }

                if (controller.sortasccolnames != null && controller.sortasccolnames.Contains(rowid))
                    controller.sortasccolnames = null;
                if (controller.sortdesccolnames != null && controller.sortdesccolnames.Contains(rowid))
                    controller.sortdesccolnames = null;
            }

            string DSFname = ds.FileName;//12Apr2017
            string DSname = ds.Name;//12Apr2017
            string DSSheet =  !string.IsNullOrEmpty(ds.SheetName) ? ds.SheetName : string.Empty;			
            ds = service.Refresh(ds); //ds becom,es null in this step if right-click delete all rows from UI datagrid.
            if (ds == null)//12Apr2017
            {
                ds = new DataSource();
                ds.Variables = new List<DataSourceVariable>();
                ds.FileName = DSFname;
                ds.Name = DSname;
                ds.SheetName = DSSheet;
            }
            controller.RefreshGrids(ds);

            InitializeDynamicColIndexes();
            BSkyMouseBusyHandler.HideMouseBusy();
        }

        public void ReloadRefreshC1Grid()//26Mar2013
        {
            gridControl1.Refresh();
        }

        #endregion

        //3Dec2013
        #region Statusbar

        public void RefreshStatusBar()
        {
            string name = DS.Name;

            string splitVars = (string)OutputHelper.GetGlobalMacro(string.Format("GLOBAL.{0}.SPLIT", name), "SelectedVars");

            if (splitVars != null)//&& splitVars.Count > 1)
            {
                name = BSky.GlobalResources.Properties.UICtrlResources.StatusbarSplitInfo + " " + splitVars.Replace('\'', ' ');//[0];
            }
            else
            {
                name = "";// "No Split";
            }

            statusbar.Text = name;
        }
        #endregion

        #region ContextMenu

        private void variableGrid_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            foreach (var item in variableGrid.Rows)
            {
                if (item.IsMouseOver)
                {
                    variableGrid.SelectedIndex = item.Index;
                    break;
                }
            }
        }

        private void _addfactorlevel_Click(object sender, RoutedEventArgs e)
        {
            int rowindex = variableGrid.SelectedIndex;
            string colname = (this.Variables[rowindex] as DataSourceVariable).RName;
            DataColumnMeasureEnum measure = (this.Variables[rowindex] as DataSourceVariable).Measure;

            if (measure == DataColumnMeasureEnum.Scale)//dont let user run "add factor " on scale col.
            {
                MessageBox.Show(BSky.GlobalResources.Properties.UICtrlResources.CantAddFactorToScaleVar,
                    "", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            List<string> oldlvls = new List<string>();
            List<string> newlvls = new List<string>();
            List<string> levels = new List<string>();

            foreach (var v in ds.Variables[rowindex].Values)
            {
                levels.Add(v);
                oldlvls.Add(v);
            }

            AddFactorLevelsDialog fld = new AddFactorLevelsDialog();
            fld.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            fld.FactorLevels = levels;
            fld.ShowDialog();

            foreach (string s in fld.FactorLevels)
            {
                if (s != "<NA>" && !oldlvls.Contains(s))
                {
                    newlvls.Add(s);
                }
            }

            //Pass new levels only
            IAnalyticsService analyticServ = LifetimeService.Instance.Container.Resolve<IAnalyticsService>();
            analyticServ.AddFactorLevels(colname, newlvls, ds.Name);
            refreshDataGrid();
        }

        private void _changelabel_Click(object sender, RoutedEventArgs e)
        {
            int rowindex = variableGrid.SelectedIndex;

            int validrowidx = rowindex;

            if (validrowidx >= 0)
            {
                ChangeLabels(validrowidx);
            }
        }

        private void ChangeLabels(int rowindex)
        {
            if (rowindex < 0)
            {
                //Wrong row index
                return;
            }
            string colname = (DS.Variables[rowindex] as DataSourceVariable).RName;//11Sep2018 (this.Variables[rowindex] as DataSourceVariable).RName;
            DataColumnMeasureEnum measure = (this.Variables[rowindex] as DataSourceVariable).Measure;

            if (measure == DataColumnMeasureEnum.Scale)
            {
                MessageBox.Show(BSky.GlobalResources.Properties.UICtrlResources.InvalidOperationOnScale,
                    BSky.GlobalResources.Properties.UICtrlResources.WarningChangeLevel, MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            bool hasNA = false;
            IAnalyticsService analyticServ = LifetimeService.Instance.Container.Resolve<IAnalyticsService>();
            List<FactorMap> fctmaplst = new List<FactorMap>();

            int ii = 1;
            foreach (var v in ds.Variables[rowindex].Values)
            {
                if (v.Equals("<NA>"))
                    hasNA = true;

                FactorMap cpyfm = new FactorMap();
                cpyfm.labels = v;
                cpyfm.textbox = v;
                cpyfm.numlevel = !hasNA?ii.ToString():"<NA>" ;
                ii++;
                fctmaplst.Add(cpyfm);
            }

            //04Aug2016 Adding <NA> if its missing while editing levels using Value col ellipses buttom
            if (!hasNA)
            {
                FactorMap nafm = new FactorMap();
                nafm.labels = "<NA>";
                nafm.textbox = "<NA>";
                nafm.numlevel = "<NA>";
                fctmaplst.Add(nafm);
            }

            //Adding extra field so that user can enter new level for factor.
            {
                FactorMap blankfm = new FactorMap();
                blankfm.labels = BSky.GlobalResources.Properties.UICtrlResources.AddFactorLevelMsg;
                blankfm.textbox = "";
                blankfm.numlevel = "";
                fctmaplst.Add(blankfm);
            }

            ValueLabelsSubDialog vlsd = new ValueLabelsSubDialog(fctmaplst, 
                BSky.GlobalResources.Properties.UICtrlResources.ExistingLabel, 
                BSky.GlobalResources.Properties.UICtrlResources.NewLabel,
                BSky.GlobalResources.Properties.UICtrlResources.NumericLevels);
            vlsd.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            vlsd.ShowDialog();
            if (!vlsd.OKclicked)
            {
                return;
            }
            List<ValLvlListItem> finalList = new List<ValLvlListItem>();
            ValLvlListItem vlit;

            foreach (FactorMap fm in fctmaplst)
            {
                if (fm.labels != "<NA>" || fm.textbox != "<NA>")
                {
                    if (fm.labels != null && fm.labels.Contains("Enter new level(s) separated by comma") && fm.textbox.Trim().Length < 1)
                    {
                        //skip because its a blank field and no body entered anything there. 
                        //User only renamed existing labels/levels
                    }
                    else
                    {
                        vlit = new ValLvlListItem();
                        vlit.OriginalLevel = fm.labels.Contains("Enter new level(s) separated by comma")?string.Empty:fm.labels;
                        vlit.NewLevel = fm.textbox;

                        finalList.Add(vlit);
                    }
                }
            }
            analyticServ.ChangeColumnLevels(colname, finalList, ds.Name);
            refreshDataGrid();
        }

        private void _makeFactor_Click(object sender, RoutedEventArgs e)
        {
            int rowindex = variableGrid.SelectedIndex;
            //27Jun2016 remaining code was moved to makeVariableFactor();
            makeVariableFactor(rowindex);
        }

        private void makeVariableFactor(int row_index_)
        {
            if (row_index_ >= 0 && !(row_index_ < this.Variables.Count) && (!(row_index_ < ((variableGrid.ItemsSource) as IList).Count))) //if row_index_ is out of bounds 
            {
                return;
            }

            string colname = (this.Variables[row_index_] as DataSourceVariable).RName;
            DataColumnMeasureEnum measure = (this.Variables[row_index_] as DataSourceVariable).Measure;

            int varidx = 0;

            if (row_index_ > 0)
                varidx = row_index_;

            string colid = "Measure";
            List<string> colLevels = getLevels();

            ///// Modifying R side Dataset ////////
            IAnalyticsService analyticServ = LifetimeService.Instance.Container.Resolve<IAnalyticsService>();

            if (colname != null)//new row
            {//edit existing row

                UAReturn retval = analyticServ.MakeColFactor(ds.Name, colname);

            }

            ds.Changed = true;
            refreshDataGrid();

            (((variableGrid.ItemsSource) as IList)[row_index_] as DataSourceVariable).Values = DS.Variables[row_index_].Values;//Values
            (((variableGrid.ItemsSource) as IList)[row_index_] as DataSourceVariable).DataType = DS.Variables[row_index_].DataType;//DataType
            (((variableGrid.ItemsSource) as IList)[row_index_] as DataSourceVariable).DataClass = DS.Variables[row_index_].DataClass;//DataClass
            (((variableGrid.ItemsSource) as IList)[row_index_] as DataSourceVariable).Measure = DS.Variables[row_index_].Measure;//Measure
            (((variableGrid.ItemsSource) as IList)[row_index_] as DataSourceVariable).Label = DS.Variables[row_index_].Label;//Label
            (((variableGrid.ItemsSource) as IList)[row_index_] as DataSourceVariable).RName = DS.Variables[row_index_].RName;//RName
            variableGrid.Refresh();

        }

        private void _makeString_Click(object sender, RoutedEventArgs e)
        {
            int rowindex = variableGrid.SelectedIndex;

            makeVariableString(rowindex);
        }

        private void makeVariableString(int row_index_)
        {
            if (row_index_ >= 0 && !(row_index_ < this.Variables.Count)) //if row_index_ is out of bounds 
            {
                return;
            }

            string colname = (this.Variables[row_index_] as DataSourceVariable).RName;
            DataColumnMeasureEnum measure = (this.Variables[row_index_] as DataSourceVariable).Measure;
          

            int varidx = 0;

            if (row_index_ > 0)
                varidx = row_index_;

            string colid = "Measure";
            List<string> colLevels = getLevels();

            ///// Modifying R side Dataset ////////
            IAnalyticsService analyticServ = LifetimeService.Instance.Container.Resolve<IAnalyticsService>();

            if (colname != null)//new row
            {//edit existing row

                UAReturn retval = analyticServ.MakeColString(ds.Name, colname);

            }

            ds.Changed = true;
            refreshDataGrid();

            (((variableGrid.ItemsSource) as IList)[row_index_] as DataSourceVariable).Values = DS.Variables[row_index_].Values;//Values
            (((variableGrid.ItemsSource) as IList)[row_index_] as DataSourceVariable).DataType = DS.Variables[row_index_].DataType;//DataType
            (((variableGrid.ItemsSource) as IList)[row_index_] as DataSourceVariable).DataClass = DS.Variables[row_index_].DataClass;//DataClass
            (((variableGrid.ItemsSource) as IList)[row_index_] as DataSourceVariable).Measure = DS.Variables[row_index_].Measure;//Measure
            (((variableGrid.ItemsSource) as IList)[row_index_] as DataSourceVariable).Label = DS.Variables[row_index_].Label;//Label
            (((variableGrid.ItemsSource) as IList)[row_index_] as DataSourceVariable).RName = DS.Variables[row_index_].RName;//RName
            variableGrid.Refresh();

        }

        //Make Numeric 11Oct2017
        private void _makeNumeric_Click(object sender, RoutedEventArgs e)
        {
            int rowindex = variableGrid.SelectedIndex;

            makeVariableNumeric(rowindex);
        }

        private void makeVariableNumeric(int row_index_)
        {
            if (row_index_ >= 0 && !(row_index_ < this.Variables.Count)) //if row_index_ is out of bounds 
            {
                return;
            }

            string colname = (this.Variables[row_index_] as DataSourceVariable).RName;
            DataColumnMeasureEnum measure = (this.Variables[row_index_] as DataSourceVariable).Measure;

            int varidx = 0;

            if (row_index_ > 0)
                varidx = row_index_;

            string colid = "Measure";
            List<string> colLevels = getLevels();

            ///// Modifying R side Dataset ////////
            IAnalyticsService analyticServ = LifetimeService.Instance.Container.Resolve<IAnalyticsService>();

            if (colname != null)//new row
            {//edit existing row

                UAReturn retval = analyticServ.MakeColNumeric(ds.Name, colname);

            }

            ds.Changed = true;
            refreshDataGrid();

            (((variableGrid.ItemsSource) as IList)[row_index_] as DataSourceVariable).Values = DS.Variables[row_index_].Values;//Values
            (((variableGrid.ItemsSource) as IList)[row_index_] as DataSourceVariable).DataType = DS.Variables[row_index_].DataType;//DataType
            (((variableGrid.ItemsSource) as IList)[row_index_] as DataSourceVariable).DataClass = DS.Variables[row_index_].DataClass;//DataClass
            (((variableGrid.ItemsSource) as IList)[row_index_] as DataSourceVariable).Measure = DS.Variables[row_index_].Measure;//Measure
            (((variableGrid.ItemsSource) as IList)[row_index_] as DataSourceVariable).Label = DS.Variables[row_index_].Label;//Label
            (((variableGrid.ItemsSource) as IList)[row_index_] as DataSourceVariable).RName = DS.Variables[row_index_].RName;//RName
            variableGrid.Refresh();

        }

        private void _nomOrd2Scale_Click(object sender, RoutedEventArgs e)
        {
        }

        private void _nomToOrd_Click(object sender, RoutedEventArgs e)
        {
        }

        private void _ordToNom_Click(object sender, RoutedEventArgs e)
        {
        }

        //Numeric Variable inserted in the middle
        private void _insertNewVar_Click(object sender, RoutedEventArgs e)
        {
            DataSourceVariable var = new DataSourceVariable();

            string varname = "newvar";

            int rowindex = variableGrid.SelectedIndex;

            //checking duplicate var names
            do
            {
                varname = "newvar" + varcount.ToString();
                varcount++;
            } while (isDuplicateColNameAddingNew(varname));//28Jun2016 fixed
            var.Name = varname;
            var.Label = varname;
            var.RName = varname;

            IAnalyticsService analyticServ = LifetimeService.Instance.Container.Resolve<IAnalyticsService>();
            analyticServ.addNewVariable(var.Name, "double", ".", rowindex + 1, ds.Name);

            this.Variables.Insert(rowindex, var);
            DS.Variables.Insert(rowindex, var);//one more refresh needed. I guess

            renumberRowHeader(variableGrid);
            ds.Changed = true;
            refreshDataGrid();
        }

        //Numeric Variable inserted at the end
        private void _insertNewVarAtEnd_Click(object sender, RoutedEventArgs e)
        {
            DataSourceVariable var = new DataSourceVariable();

            string varname = "newvar";

            int rowindex = Variables.Count;

            //checking duplicate var names
            do
            {
                varname = "newvar" + varcount.ToString();
                varcount++;
            } while (isDuplicateColNameAddingNew(varname));//28Jun2016 fixed
            var.Name = varname;
            var.Label = varname;
            var.RName = varname;

            IAnalyticsService analyticServ = LifetimeService.Instance.Container.Resolve<IAnalyticsService>();
            analyticServ.addNewVariable(var.Name, "double", ".", rowindex + 1, ds.Name);

            this.Variables.Insert(rowindex, var);
            DS.Variables.Insert(rowindex, var);//one more refresh needed. I guess

            renumberRowHeader(variableGrid);
            ds.Changed = true;
            refreshDataGrid();
        }

        //Character Variable inserted in the middle
        private void _insertNewCharVar_Click(object sender, RoutedEventArgs e)
        {
            DataSourceVariable var = new DataSourceVariable();

            string varname = "newvar";
            int rowindex = variableGrid.SelectedIndex;

            //checking duplicate var names
            do
            {
                varname = "newvar" + varcount.ToString();
                varcount++;
            } while (isDuplicateColNameAddingNew(varname));//28Jun2016  fixed
            var.Name = varname;
            var.Label = varname;
            var.RName = varname;
            var.DataType = DataColumnTypeEnum.Character;
            var.Measure = DataColumnMeasureEnum.Nominal;

            IAnalyticsService analyticServ = LifetimeService.Instance.Container.Resolve<IAnalyticsService>();
            analyticServ.addNewVariable(var.Name, "character", ".", rowindex + 1, ds.Name);

            this.Variables.Insert(rowindex, var);
            DS.Variables.Insert(rowindex, var);//one more refresh needed. I guess

            renumberRowHeader(variableGrid);
            ds.Changed = true;
            refreshDataGrid();

            _makeFactor_Click(sender, e); // making it a factor
        }

        //Character Variable inserted at the end
        private void _insertNewCharVarAtEnd_Click(object sender, RoutedEventArgs e)
        {
            DataSourceVariable var = new DataSourceVariable();

            string varname = "newvar";
            int rowindex = Variables.Count;

            //checking duplicate var names
            do
            {
                varname = "newvar" + varcount.ToString();
                varcount++;
            } while (isDuplicateColNameAddingNew(varname));//28Jun2016 fixed
            var.Name = varname;
            var.Label = varname;
            var.RName = varname;
            var.DataType = DataColumnTypeEnum.Character;
            var.Measure = DataColumnMeasureEnum.Nominal;

            IAnalyticsService analyticServ = LifetimeService.Instance.Container.Resolve<IAnalyticsService>();
            analyticServ.addNewVariable(var.Name, "character", ".", rowindex + 1, ds.Name);

            this.Variables.Insert(rowindex, var);
            DS.Variables.Insert(rowindex, var);

            renumberRowHeader(variableGrid);
            ds.Changed = true;
            refreshDataGrid();

            //Now make new variable a factor.
            makeVariableFactor(rowindex);
        }

        private void _deleteVar_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show(BSky.GlobalResources.Properties.UICtrlResources.DeleteVarConfirmation,
                BSky.GlobalResources.Properties.UICtrlResources.confirmation, MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                DataSourceVariable var = new DataSourceVariable();
                int rowindex = variableGrid.SelectedIndex;

                variableGrid.RemoveRow(rowindex);//two things .grid remove UI dataset side remove. sec is R side remove
                variableGrid.Refresh();

                ds.Changed = true;

            }
        }

        private void _insertNewData_Click(object sender, RoutedEventArgs e)
        {
            preserveVerticalScroll();

            int colcount = ds.Variables.Count;
            string newemptyrow = CreateEmptyRowCollection(colcount); 

            string s = gridControl1.CurrentCell.Text;
            IAnalyticsService analyticServ = LifetimeService.Instance.Container.Resolve<IAnalyticsService>();
            analyticServ.AddNewDatagridRow("", s, newemptyrow, gridControl1.SelectedIndex, ds.Name);
            ds.RowCount++;
            //data.Add(new object());
            ds.Changed = true;
            refreshDataGrid();
            restoreVerticalScroll();
        }

        //returns something like c('','','','') 
        private string CreateEmptyRowCollection(int colcount)
        {
            StringBuilder emptyrow = new StringBuilder("c(");

            for (int i = 0; i < colcount; i++)
            {
                if (i + 1 == colcount)//if we are on last col
                {
                    emptyrow.Append("'')");
                }
                else
                {
                    emptyrow.Append("'',");
                }
            }
            return emptyrow.ToString();
        }

        private void _deleteData_Click(object sender, RoutedEventArgs e)
        {
            preserveVerticalScroll();

            IAnalyticsService analyticServ = LifetimeService.Instance.Container.Resolve<IAnalyticsService>();
            analyticServ.RemoveDatagridRow(gridControl1.SelectedIndex, ds.Name, ds.SheetName);//removing R side

            ds.RowCount--;
            //renumbering
            renumberRowHeader(gridControl1);
            ds.Changed = true;
            refreshDataGrid();
            restoreVerticalScroll();
        }

        //From C1forum start. but modified a bit
        private void RightClickCopyCmdExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if(gridControl1.Selection != null && gridControl1.Selection.SelectedCells.Count > 1)//more than one cell must be selected. 
            {
                DataGridDefaultInputHandlingStrategy gridContent = new DataGridDefaultInputHandlingStrategy(gridControl1);
                Clipboard.SetText(gridContent.GetClipboardContent());
            }
        }

        private void RightClickCopyCmdCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }
        //from C1forum ends

        void datagridContextMenu_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            c1gridContextMenuOpening(sender, e, gridControl1);
        }

        private void variableGrid_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            c1gridContextMenuOpening(sender, e, variableGrid);
        }

        private void c1gridContextMenuOpening(object sender, ContextMenuEventArgs e, C1DataGrid gridname)
        {

            DependencyObject dobj = (DependencyObject)e.OriginalSource;
            dobj = VisualTreeHelper.GetParent(dobj);
            dobj = VisualTreeHelper.GetParent(dobj);
            dobj = VisualTreeHelper.GetParent(dobj);
            if ( (dobj.DependencyObjectType.Name.Equals("DataGridRowHeaderPresenter") || 
                dobj.DependencyObjectType.Name.Equals("DataGridRowsHeaderPanel") ||
                dobj.DependencyObjectType.Name.Equals("DataGridCellPresenter") ||//this and**   
                dobj.DependencyObjectType.Name.Equals("Grid"))
                && (gridControl1.SelectedItems != null && gridControl1.SelectedItems.Count() <=1))
            {
                e.Handled = false;

                DataGridViewport vp = gridname.Viewport;
                int firstrowidx = vp.FirstVisibleRow;
                int lastrowidx = vp.LastVisibleRow;

                for (int i = firstrowidx; i <= lastrowidx; i++)
                {
                    var item = gridname.Rows[i];

                    if (item.IsMouseOver)
                    {
                        gridname.SelectedIndex = item.Index;
                        break;
                    }
                }

                //Enable-disble context menu items
                if (datagridContextMenu.Items.Count == 3)
                {
                    (datagridContextMenu.Items[0] as MenuItem).IsEnabled = true;
                    (datagridContextMenu.Items[1] as MenuItem).IsEnabled = true;
                    (datagridContextMenu.Items[2] as MenuItem).IsEnabled = false;
                }
            }
            else if (gridControl1.Selection != null && gridControl1.Selection.SelectedCells.Count > 1)//more than one cell must be selected. 
            {
                if (dobj.DependencyObjectType.Name.Equals("DataGridCellPresenter")
                    || VisualTreeHelper.GetParent(dobj).DependencyObjectType.Name.Equals("DataGridCellPresenter")) //if "Grid" then find parent
                {
                    //Enable-disble context menu items
                    if (datagridContextMenu.Items.Count == 3)
                    {
                        (datagridContextMenu.Items[0] as MenuItem).IsEnabled = false;
                        (datagridContextMenu.Items[1] as MenuItem).IsEnabled = false;
                        (datagridContextMenu.Items[2] as MenuItem).IsEnabled = true;
                    }

                }
                else
                { 
                    e.Handled = true;
                    return;
                }
            }
            else
            {
                e.Handled = true;
                return;
            }
        }

        //28Jun2016 Adding a new col. Check if new colname already exists.
        private bool isDuplicateColNameAddingNew(string newcolname)
        {
            bool alreadyExists = false;

            foreach (DataSourceVariable temp in this.Variables)
            {
                if (temp.Name.Equals(newcolname))
                {
                    alreadyExists = true;
                    break;
                }
            }
            return alreadyExists;
        }

        //28Jun2016 Check if the colname currently being renamed, already exists.
        private bool isDuplicateColNameOnRename(string newcolname)
        {
            bool alreadyExists = false;
            int counter = 0; 

            foreach (DataSourceVariable temp in this.Variables)
            {
                if (temp.Name.Equals(newcolname))
                {
                    counter++;
                    if (counter > 1)
                    {
                        alreadyExists = true;
                        break;
                    }
                }
            }
            return alreadyExists;
        }
        #endregion

        #region Show errors/warning in output window

        private void SendErrorWarningToOutput(UAReturn retval)//08jul2013
        {
            OutputWindowContainer owc = (LifetimeService.Instance.Container.Resolve<IOutputWindowContainer>()) as OutputWindowContainer;
            OutputWindow ow = owc.ActiveOutputWindow as OutputWindow; //get currently active window
            IUIController UIController = LifetimeService.Instance.Container.Resolve<IUIController>();

            OutputHelper.Reset();
            OutputHelper.UpdateMacro("%DATASET%", UIController.GetActiveDocument().Name);
            OutputHelper.UpdateMacro("%MODEL%", UIController.GetActiveModelName());
            retval.Success = true;
            AnalyticsData data = new AnalyticsData();

            data.Result = retval;
            data.AnalysisType = retval.CommandString; //"T-Test"; For Parent Node name 02Aug2012

            UIController.AnalysisComplete(data);

            Window1 window = LifetimeService.Instance.Container.Resolve<Window1>();
            window.Activate();
        }

        #endregion

        private void variableGrid_CancelingNewRow(object sender, DataGridEndingNewRowEventArgs e)
        {
        }

        #region Mouse Busy/Free and Keyboard events

        Cursor defaultcursor;

        private void ShowMouseBusy_old()
        {
            Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
        }

        private void HideMouseBusy_old()
        {
            Mouse.OverrideCursor = null;
        }

        // Showing a busy message while var grid is loading 08Aug2013
        private void TabItem_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (Mouse.OverrideCursor == System.Windows.Input.Cursors.Wait)
                e.Handled = true;
        }

        private void UserControl_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (Mouse.OverrideCursor == System.Windows.Input.Cursors.Wait)
                e.Handled = true;
        }

        //Which tab is clicked Data OR Variables
        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Mouse.OverrideCursor == System.Windows.Input.Cursors.Wait)
            {
                e.Handled = true;
            }
            else
            {
                string tabHeader = ((sender as TabControl).SelectedItem as TabItem).Header as string;

                if (tabHeader.Equals(BSky.GlobalResources.Properties.UICtrlResources.lblDataTab))
                    tabHeader = "Data";
                else if (tabHeader.Equals(BSky.GlobalResources.Properties.UICtrlResources.lblVariablesTab))
                    tabHeader = "Variables";
                else
                    tabHeader = string.Empty;

                switch (tabHeader)
                {
                    case "Data":
                        DatagridPaginationButtonShowHide(true);
                        break;

                    case "Variables":
                        DatagridPaginationButtonShowHide(false);
                        break;
                    default:
                        return;
                }
            }
        }

        // Shows and Hides the navigation buttons at the bottom of the datagrid.
        private void DatagridPaginationButtonShowHide(bool show)
        {
            if (show)
            {
                leftmostpagebutton.Visibility = System.Windows.Visibility.Visible;
                leftpagebutton.Visibility = System.Windows.Visibility.Visible;
                rightpagebutton.Visibility = System.Windows.Visibility.Visible;
                rightmostpagebutton.Visibility = System.Windows.Visibility.Visible;
                endpagestatus.Visibility = Visibility.Visible;
            }
            else
            {
                leftmostpagebutton.Visibility = System.Windows.Visibility.Hidden;
                leftpagebutton.Visibility = System.Windows.Visibility.Hidden;
                rightpagebutton.Visibility = System.Windows.Visibility.Hidden;
                rightmostpagebutton.Visibility = System.Windows.Visibility.Hidden;
                endpagestatus.Visibility = Visibility.Hidden;
            }
        }

        #endregion

        #region Dynamic col loading using buttons
        private void initForPageScroll()
        {
            totalCols = ds.Variables.Count;
            maxcolidx = totalCols - 1;
            endpagestatus.Text = string.Empty;
        }

        //disable all buttons if columns less than 16
        public void DisableEnableAllNavButtons()
        {
            int totalColumns = 0;
            if(DS!=null && DS.Variables!=null)
                totalColumns = this.DS.Variables.Count;
            if (totalColumns > 0 && totalColumns < 16)
            {
                DisableAllNavigationButtons();
                endpagestatus.Text = "There is only one page. Use the scrollbar if available under the data grid to see all columns.";
            }
            else
            {
                EnableAllNavigationButtons();
                endpagestatus.Text = string.Empty;
            }
        }

        //First Page
        private void leftmostpagebutton_Click(object sender, RoutedEventArgs e)
        {
            if (AdvancedLogging) logService.WriteToLogLevel("FIRST clicked. Mouse busy", LogLevelEnum.Info);
            BSkyMouseBusyHandler.ShowMouseBusy();
            initForPageScroll();
            EnableRightNavigationButtons();
            //For dataset with lesser numbers of columns (less than colsToLoad), there is no need to do pagination at all
            if (maxcolidx < colsToLoad)
            {
                if (AdvancedLogging)  logService.WriteToLogLevel("All cols fit in one page", LogLevelEnum.Info);
                BSkyMouseBusyHandler.HideMouseBusy();
                if (AdvancedLogging) logService.WriteToLogLevel("FIRST clicked. Mouse free", LogLevelEnum.Info);
                return;
            }

            //For performance improvement
            if (startcolidx == 0)
            {
                if (AdvancedLogging) logService.WriteToLogLevel("Already on the first page", LogLevelEnum.Info);

                endpagestatus.Text = "You have reached the first page. Use the scrollbar if available under the data grid, to view all the columns on the first page.";
                DisableLeftNavigationButtons();
                BSkyMouseBusyHandler.HideMouseBusy();
                if (AdvancedLogging) logService.WriteToLogLevel("FIRST clicked. Mouse free", LogLevelEnum.Info);
                return;
            }
            startcolidx = 0;
            endcolidx = colsToLoad - 1;//zero based index
            LoadPreviousColSet();
            BSkyMouseBusyHandler.HideMouseBusy();
            if (AdvancedLogging) logService.WriteToLogLevel("FIRST clicked. Mouse free", LogLevelEnum.Info);
        }

        //Previous Page
        private void leftpagebutton_Click(object sender, RoutedEventArgs e)
        {
            if (AdvancedLogging) logService.WriteToLogLevel("LEFT clicked. Mouse busy", LogLevelEnum.Info);
            BSkyMouseBusyHandler.ShowMouseBusy();
            initForPageScroll();
            EnableRightNavigationButtons();
            //For dataset with lesser numbers of columns (less than colsToLoad), there is no need to do pagination at all
            if (maxcolidx < colsToLoad)
            {
                if (AdvancedLogging) logService.WriteToLogLevel("All cols fit in one page", LogLevelEnum.Info);
                BSkyMouseBusyHandler.HideMouseBusy();
                if (AdvancedLogging) logService.WriteToLogLevel("LEFT clicked. Mouse free", LogLevelEnum.Info);
                return;
            }

            //For performance improvement
            if (startcolidx == 0)
            {
                if (AdvancedLogging) logService.WriteToLogLevel("Already on the first page", LogLevelEnum.Info);

                endpagestatus.Text = "You have reached the first page. Use the scrollbar if available under the data grid, to view all the columns on the first page.";
                DisableLeftNavigationButtons();
                BSkyMouseBusyHandler.HideMouseBusy();
                if (AdvancedLogging) logService.WriteToLogLevel("LEFT clicked. Mouse free", LogLevelEnum.Info);
                return;
            }

            LoadPreviousColSet();
            BSkyMouseBusyHandler.HideMouseBusy();
            if (AdvancedLogging) logService.WriteToLogLevel("LEFT clicked. Mouse free", LogLevelEnum.Info);
        }

        //Next Page
        private void rightpagebutton_Click(object sender, RoutedEventArgs e)
        {
            if (AdvancedLogging) logService.WriteToLogLevel("NEXT clicked. Mouse busy", LogLevelEnum.Info);
            BSkyMouseBusyHandler.ShowMouseBusy();
            initForPageScroll();
            EnableLeftNavigationButtons();
            //For dataset with lesser numbers of columns (less than colsToLoad), there is no need to do pagination at all
            if (maxcolidx < colsToLoad)
            {
                if (AdvancedLogging) logService.WriteToLogLevel("All cols fit in one page", LogLevelEnum.Info);
                BSkyMouseBusyHandler.HideMouseBusy();
                if (AdvancedLogging) logService.WriteToLogLevel("NEXT clicked. Mouse free", LogLevelEnum.Info);
                return;
            }

            //For performance improvement
            if (endcolidx == maxcolidx)
            {
                if (AdvancedLogging) logService.WriteToLogLevel("Already on the last page", LogLevelEnum.Info);

                endpagestatus.Text = "You have reached the last page. Use the scrollbar if available under the data grid, to view the last column.";
                DisableRightNavigationButtons();
                BSkyMouseBusyHandler.HideMouseBusy();
                if (AdvancedLogging) logService.WriteToLogLevel("NEXT clicked. Mouse free", LogLevelEnum.Info);
                return;
            }
            LoadNextColSet();
            BSkyMouseBusyHandler.HideMouseBusy();
            if (AdvancedLogging) logService.WriteToLogLevel("NEXT clicked. Mouse free", LogLevelEnum.Info);
        }

        //Last Page
        bool LASTclicked;

        private void rightmostpagebutton_Click(object sender, RoutedEventArgs e)
        {
            if (AdvancedLogging) logService.WriteToLogLevel("LAST clicked. Mouse busy", LogLevelEnum.Info);
            BSkyMouseBusyHandler.ShowMouseBusy();
            initForPageScroll();
            EnableLeftNavigationButtons();
            //For dataset with lesser numbers of columns (less than colsToLoad), there is no need to do pagination at all
            if (maxcolidx < colsToLoad)
            {
                if (AdvancedLogging)  logService.WriteToLogLevel("All cols fit in one page", LogLevelEnum.Info);
                BSkyMouseBusyHandler.HideMouseBusy();
                if (AdvancedLogging) logService.WriteToLogLevel("LAST clicked. Mouse free", LogLevelEnum.Info);
                return;
            }
            //For performance improvement
            if (endcolidx == maxcolidx)
            {
                if (AdvancedLogging)  logService.WriteToLogLevel("Already on the last page", LogLevelEnum.Info);

                endpagestatus.Text = "You have reached the last page. Use the scrollbar if available under the data grid, to view the last column.";
                DisableRightNavigationButtons();
                BSkyMouseBusyHandler.HideMouseBusy();
                if (AdvancedLogging) logService.WriteToLogLevel("LAST clicked. Mouse free", LogLevelEnum.Info);
                return;
            }

            LASTclicked = true;
            startcolidx = ds.Variables.Count - 10; 
            endcolidx = ds.Variables.Count;
            LoadNextColSet();
            BSkyMouseBusyHandler.HideMouseBusy();
            if (AdvancedLogging) logService.WriteToLogLevel("LAST clicked. Mouse free", LogLevelEnum.Info);
        }


        #region Datagrid Navigation Buttons disable/enable methods

        void DisableRightNavigationButtons()
        {
            rightpagebutton.IsEnabled = false;
            rightmostpagebutton.IsEnabled = false;

            nextimg.Visibility = Visibility.Collapsed;
            nextGrayimg.Visibility = Visibility.Visible;

            lastimg.Visibility = Visibility.Collapsed;
            lastGrayimg.Visibility = Visibility.Visible;
        }

        void EnableRightNavigationButtons()
        {
            rightpagebutton.IsEnabled = true;
            rightmostpagebutton.IsEnabled = true;

            nextimg.Visibility = Visibility.Visible;
            nextGrayimg.Visibility = Visibility.Collapsed;

            lastimg.Visibility = Visibility.Visible;
            lastGrayimg.Visibility = Visibility.Collapsed;
        }

        void DisableLeftNavigationButtons()
        {
            leftmostpagebutton.IsEnabled = false;
            leftpagebutton.IsEnabled = false;

            firstimg.Visibility = Visibility.Collapsed;
            firstGrayimg.Visibility = Visibility.Visible;

            previuosimg.Visibility = Visibility.Collapsed;
            previuosGrayimg.Visibility = Visibility.Visible;
        }

        void EnableLeftNavigationButtons()
        {
            leftmostpagebutton.IsEnabled = true;
            leftpagebutton.IsEnabled = true;

            firstimg.Visibility = Visibility.Visible;
            firstGrayimg.Visibility = Visibility.Collapsed;

            previuosimg.Visibility = Visibility.Visible;
            previuosGrayimg.Visibility = Visibility.Collapsed;
        }

        void DisableAllNavigationButtons()
        {
            rightpagebutton.IsEnabled = false;
            rightmostpagebutton.IsEnabled = false;

            nextimg.Visibility = Visibility.Collapsed;
            nextGrayimg.Visibility = Visibility.Visible;

            lastimg.Visibility = Visibility.Collapsed;
            lastGrayimg.Visibility = Visibility.Visible;

            leftmostpagebutton.IsEnabled = false;
            leftpagebutton.IsEnabled = false;

            firstimg.Visibility = Visibility.Collapsed;
            firstGrayimg.Visibility = Visibility.Visible;

            previuosimg.Visibility = Visibility.Collapsed;
            previuosGrayimg.Visibility = Visibility.Visible;


        }

        void EnableAllNavigationButtons()
        {
            rightpagebutton.IsEnabled = true;
            rightmostpagebutton.IsEnabled = true;

            nextimg.Visibility = Visibility.Visible;
            nextGrayimg.Visibility = Visibility.Collapsed;

            lastimg.Visibility = Visibility.Visible;
            lastGrayimg.Visibility = Visibility.Collapsed;
            rightpagebutton.IsEnabled = true;
            rightmostpagebutton.IsEnabled = true;

            nextimg.Visibility = Visibility.Visible;
            nextGrayimg.Visibility = Visibility.Collapsed;

            lastimg.Visibility = Visibility.Visible;
            lastGrayimg.Visibility = Visibility.Collapsed;

        }

        #endregion


        void LoadPreviousColSet()
        {
            preserveVerticalScroll();
            DecrementStartEnd(colsToLoad);
            GetColsInRange(startcolidx, endcolidx);
            RefreshGridOnScroll();// controller.RefreshBothGrids(ds);
            restoreVerticalScroll();
            scrollTicks = 1;
            if (selectedrow >= 0 && selectedrow < gridControl1.Rows.Count) gridControl1.SelectedIndex = selectedrow;//preserve the row selection
            logService.WriteToLogLevel("After Fetching: Start Col idx = " + startcolidx + " : End Col idx = " + endcolidx, LogLevelEnum.Info);
        }

        //08Jun2018 Selecting a block on cells and hitting DELETE key was deleting the selected rows. 
        private void gridControl1_DeletingRows(object sender, DataGridDeletingRowsEventArgs e)
        {
            string msg = "This will delete multiple rows. Are you sure?";
            MessageBoxResult mbr = MessageBox.Show(msg, "Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (mbr == MessageBoxResult.Yes)//yes delete multiple data rows
            {
                //no action here. 
            }
            else //dont delete
            {
                e.Cancel = true;
            }
        }

        void LoadNextColSet()
        {
            preserveVerticalScroll();
            IncrementStartEnd(colsToLoad);

            if (LASTclicked)
            {
                startcolidx = endcolidx - 3;
                LASTclicked = false;
            }

            GetColsInRange(startcolidx, endcolidx);
            RefreshGridOnScroll();
            restoreVerticalScroll();
            scrollTicks = 1;
            if (selectedrow >= 0 && selectedrow < gridControl1.Rows.Count) gridControl1.SelectedIndex = selectedrow;//preserve the row selection
            logService.WriteToLogLevel("After Fetching: Start Col idx = " + startcolidx + " : End Col idx = " + endcolidx + " LASTclicked = " + LASTclicked, LogLevelEnum.Info);
        }

        #endregion

    }

    #region Converters
    public class AlignConvertor : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Enum.Parse(typeof(DataColumnAlignmentEnum), value.ToString());
        }
    }

    public class MeasureConvertor : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Enum.Parse(typeof(DataColumnMeasureEnum), value.ToString());
        }
    }

    public class RoleConvertor : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Enum.Parse(typeof(DataColumnRole), value.ToString());
        }
    }

    public class ComboImageSourceConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            BitmapImage image = new BitmapImage();
            image.BeginInit();
            if (value == null)
            {
                image.UriSource = new Uri(@"pack://application:,,,/BlueSky;component/Images/left.png");
            }
            else
            {
                switch (value.ToString())
                {
                    case "Left":
                        image.UriSource = new Uri(@"pack://application:,,,/BlueSky;component/Images/Left.png");
                        break;
                    case "Center":
                        image.UriSource = new Uri(@"pack://application:,,,/BlueSky;component/Images/center.png");
                        break;
                    case "Right":
                        image.UriSource = new Uri(@"pack://application:,,,/BlueSky;component/Images/right.png");
                        break;
                    case "Nominal":
                        image.UriSource = new Uri(@"pack://application:,,,/BlueSky;component/Images/nominal.png");
                        break;
                    case "Ordinal":
                        image.UriSource = new Uri(@"pack://application:,,,/BlueSky;component/Images/ordinal.png");
                        break;
                    case "Scale":
                        image.UriSource = new Uri(@"pack://application:,,,/BlueSky;component/Images/scale.png");
                        break;
                    case "Input":
                        image.UriSource = new Uri(@"pack://application:,,,/BlueSky;component/Images/input.png");
                        break;
                    case "Target":
                        image.UriSource = new Uri(@"pack://application:,,,/BlueSky;component/Images/target.png");
                        break;
                    case "Both":
                        image.UriSource = new Uri(@"pack://application:,,,/BlueSky;component/Images/both.png");
                        break;
                    case "None":
                        image.UriSource = new Uri(@"pack://application:,,,/BlueSky;component/Images/none.png");
                        break;
                    case "Partition":
                        image.UriSource = new Uri(@"pack://application:,,,/BlueSky;component/Images/partition.png");
                        break;
                    case "Split":
                        image.UriSource = new Uri(@"pack://application:,,,/BlueSky;component/Images/split.png");
                        break;
                    default:
                        image.UriSource = new Uri(@"pack://application:,,,/BlueSky;component/Images/imagenotfound.png");
                        break;
                }
            }
            image.EndInit();

            return image;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }

        #endregion
    }

    public class ValueLabelConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string returnStr = string.Empty;

            if (value != null)
            {
                List<string> lst = (List<string>)value;
                string[] vals = lst.ToArray();

                for (int i = 0; i < vals.Length; i++)
                {
                    if (vals[i] != null && vals[i].Trim().Length == 0) // to avoid balnk values from getting in {}
                        continue;
                    returnStr = returnStr + ("{");//("{" + i + "-");
                    if (i + 1 == vals.Length)
                        returnStr = returnStr + vals[i] + "}";
                    else
                        returnStr = returnStr + vals[i] + "}..."; 

                    break;
                }
                return (string)returnStr;
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string strValue = value as string;
            DateTime resultDateTime;

            if (DateTime.TryParse(strValue, out resultDateTime))
            {
                return resultDateTime;
            }
            return DependencyProperty.UnsetValue;
        }
    }

    public class MissingValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string returnStr = string.Empty;

            if (value != null)
            {
                List<string> lst = (List<string>)value;
                string[] vals = lst.ToArray();

                for (int i = 0; i < vals.Length; i++)
                {
                    returnStr = returnStr + ("{");//("{" + i + "-");
                    if (i + 1 == vals.Length)
                        returnStr = returnStr + vals[i] + "}";
                    else
                        returnStr = returnStr + vals[i] + "}..."; 
                    break;
                }
                return (string)returnStr;
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string strValue = value as string;
            DateTime resultDateTime;

            if (DateTime.TryParse(strValue, out resultDateTime))
            {
                return resultDateTime;
            }
            return DependencyProperty.UnsetValue;
        }
    }

    public class DataRowConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            C1.WPF.DataGrid.DataGridRow row = value as C1.WPF.DataGrid.DataGridRow;

            if (row != null)
                return row.Index;
            else
                return 1;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }

    public class DataGridFactorConvertor : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Enum.Parse(typeof(string), value.ToString());
        }
    }

    #endregion

    #region var grid columns
    public class DataGridValueLablesCol : C1.WPF.DataGrid.DataGridColumn
    {
        // Height of each level
        public static int LevelHeaderHeight = 18;

        // Inner Columns
        public ObservableCollection<C1.WPF.DataGrid.DataGridColumn> InnerColumns { get; set; }

        // Global Header
        public object CompositeHeader { get; set; }

        // Nested Levels
        public int NestedLevels { get; private set; }

        public DataGridValueLablesCol()
        {
            InnerColumns = new ObservableCollection<C1.WPF.DataGrid.DataGridColumn>();

            // the following features are not implemented
            CanUserResize = false;
            CanUserSort = false;
            CanUserFilter = false;
            IsReadOnly = true; //edit on textfield is not allowed
        }

        public void Update()
        {
            double totalWidth = 0;
            int maxNestedLevels = 0;

            // initialize grid
            var panel = new Grid();
            panel.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(DataGridValueLablesCol.LevelHeaderHeight) });
            panel.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(DataGridValueLablesCol.LevelHeaderHeight) });
            foreach (var col in InnerColumns)
            {
                // support nested scenarios
                var cc = col as DataGridValueLablesCol;

                if (cc != null)
                {
                    cc.Update();
                    maxNestedLevels = Math.Max(cc.NestedLevels, maxNestedLevels);
                }

                panel.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(col.Width.Value) });
                totalWidth += col.Width.Value;
            }

            // add global header
            var globalHeader = new ContentControl() { Content = CompositeHeader };
            Grid.SetColumnSpan(globalHeader, InnerColumns.Count);
            panel.Children.Add(globalHeader);

            // add individual headers
            for (int i = 0; i < InnerColumns.Count; i++)
            {
                var col = InnerColumns[i];

                // add nested headers
                var content = new DataGridColumnHeaderPresenter() { Content = col.Header, Background = new SolidColorBrush(Colors.Transparent) };
                Grid.SetColumn(content, i);
                Grid.SetRow(content, 1);
                panel.Children.Add(content);
            }

            // update header & global width
            Header = panel;
            Width = new C1.WPF.DataGrid.DataGridLength(totalWidth);
            NestedLevels = 1 + maxNestedLevels;
        }

        #region Cell Content

        public override object GetCellContentRecyclingKey(C1.WPF.DataGrid.DataGridRow row)
        {
            return typeof(DataGridValueLablesCol);
        }

        public override FrameworkElement CreateCellContent(C1.WPF.DataGrid.DataGridRow row)
        {
            // initialize grid
            var panel = new Grid();

            foreach (var col in InnerColumns)
            {
                panel.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(col.Width.Value) });
            }

            // add individual content
            for (int i = 0; i < InnerColumns.Count; i++)
            {
                var col = InnerColumns[i];
                var content = col.CreateCellContent(row);
                Grid.SetColumn(content, i);
                panel.Children.Add(content);
            }
            return panel;
        }

        public override void BindCellContent(FrameworkElement cellContent, C1.WPF.DataGrid.DataGridRow row)
        {
            Panel panel = (Panel)cellContent;

            // bind individual cells
            for (int i = 0; i < InnerColumns.Count; i++)
            {
                var col = InnerColumns[i];
                var control = panel.Children[i] as FrameworkElement;
                col.BindCellContent(control, row);
            }
        }

        public override void UnbindCellContent(FrameworkElement cellContent, C1.WPF.DataGrid.DataGridRow row)
        {
            Panel panel = (Panel)cellContent;

            // bind individual cells
            for (int i = 0; i < InnerColumns.Count; i++)
            {
                var col = InnerColumns[i];
                var control = panel.Children[i] as FrameworkElement;
                col.UnbindCellContent(control, row);
            }
        }

        #endregion

        #region Editing

        public override FrameworkElement GetCellEditingContent(C1.WPF.DataGrid.DataGridRow row)
        {
            // initialize grid
            var panel = new Grid();

            foreach (var col in InnerColumns)
            {
                panel.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(col.Width.Value) });
            }

            //add individual content
            for (int i = 0; i < InnerColumns.Count; i++)
            {
                var col = InnerColumns[i];
                var content = col.GetCellEditingContent(row);
                Grid.SetColumn(content, i);
                panel.Children.Add(content);
            }
            return panel;
        }

        public override object PrepareCellForEdit(FrameworkElement editingElement)
        {
            // compose all the values into a list of objects
            List<object> values = new List<object>();
            var children = (editingElement as Panel).Children;

            for (int i = 0; i < InnerColumns.Count; i++)
            {
                var value = InnerColumns[i].PrepareCellForEdit(children[i] as FrameworkElement);
                values.Add(value);
            }
            return values;
        }

        public override void CancelCellEdit(FrameworkElement editingElement, object uneditedValue)
        {
            // decompose all the values from the list of objects
            // and invoke each of the cancels
            var values = (List<object>)uneditedValue;
            var children = (editingElement as Panel).Children;

            for (int i = 0; i < InnerColumns.Count; i++)
            {
                InnerColumns[i].CancelCellEdit(children[i] as FrameworkElement, values[i]);
            }
        }

        public override bool BeginEdit(FrameworkElement editingElement, RoutedEventArgs routedEventArgs)
        {
            // pass input to first column
            if (InnerColumns.Count > 0)
            {
                var children = (editingElement as Panel).Children;
                return InnerColumns[0].BeginEdit(children[0] as Control, routedEventArgs);
            }
            return false;
        }

        public override void EndEdit(FrameworkElement editingElement)
        {
            for (int i = 0; i < InnerColumns.Count; i++)
            {
                if (editingElement is Panel)
                {
                    var children = (editingElement as Panel).Children;

                    if (children.Count > i)
                    {
                        InnerColumns[i].EndEdit(children[i] as Control);
                    }
                }
            }
        }

        #endregion
    }

    class DataGridMissingCol : C1.WPF.DataGrid.DataGridColumn
    {
        // Height of each level
        public static int LevelHeaderHeight = 18;

        // Inner Columns
        public ObservableCollection<C1.WPF.DataGrid.DataGridColumn> InnerColumns { get; set; }

        // Global Header
        public object CompositeHeader { get; set; }

        // Nested Levels
        public int NestedLevels { get; private set; }

        public DataGridMissingCol()
        {
            InnerColumns = new ObservableCollection<C1.WPF.DataGrid.DataGridColumn>();

            // the following features are not implemented
            CanUserResize = false;
            CanUserSort = false;
            CanUserFilter = false;
            IsReadOnly = true;//fix for bug. Stop edit on text field
        }

        public void Update()
        {
            double totalWidth = 0;
            int maxNestedLevels = 0;

            // initialize grid
            var panel = new Grid();
            panel.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(DataGridValueLablesCol.LevelHeaderHeight) });
            panel.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(DataGridValueLablesCol.LevelHeaderHeight) });
            foreach (var col in InnerColumns)
            {
                // support nested scenarios
                var cc = col as DataGridValueLablesCol;

                if (cc != null)
                {
                    cc.Update();
                    maxNestedLevels = Math.Max(cc.NestedLevels, maxNestedLevels);
                }

                panel.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(col.Width.Value) });
                totalWidth += col.Width.Value;
            }

            // add global header
            var globalHeader = new ContentControl() { Content = CompositeHeader };
            Grid.SetColumnSpan(globalHeader, InnerColumns.Count);
            panel.Children.Add(globalHeader);

            // add individual headers
            for (int i = 0; i < InnerColumns.Count; i++)
            {
                var col = InnerColumns[i];

                // add nested headers
                var content = new DataGridColumnHeaderPresenter() { Content = col.Header, Background = new SolidColorBrush(Colors.Transparent) };
                Grid.SetColumn(content, i);
                Grid.SetRow(content, 1);
                panel.Children.Add(content);
            }

            // update header & global width
            Header = panel;
            Width = new C1.WPF.DataGrid.DataGridLength(totalWidth);
            NestedLevels = 1 + maxNestedLevels;
        }

        #region Cell Content

        public override object GetCellContentRecyclingKey(C1.WPF.DataGrid.DataGridRow row)
        {
            return typeof(DataGridMissingCol);
        }

        public override FrameworkElement CreateCellContent(C1.WPF.DataGrid.DataGridRow row)
        {
            // initialize grid
            var panel = new Grid();

            foreach (var col in InnerColumns)
            {
                panel.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(col.Width.Value) });
            }

            // add individual content
            for (int i = 0; i < InnerColumns.Count; i++)
            {
                var col = InnerColumns[i];
                var content = col.CreateCellContent(row);
                Grid.SetColumn(content, i);
                panel.Children.Add(content);
            }
            return panel;
        }

        public override void BindCellContent(FrameworkElement cellContent, C1.WPF.DataGrid.DataGridRow row)
        {
            Panel panel = (Panel)cellContent;

            // bind individual cells
            for (int i = 0; i < InnerColumns.Count; i++)
            {
                var col = InnerColumns[i];
                var control = panel.Children[i] as FrameworkElement;
                col.BindCellContent(control, row);
            }
        }

        public override void UnbindCellContent(FrameworkElement cellContent, C1.WPF.DataGrid.DataGridRow row)
        {
            Panel panel = (Panel)cellContent;

            // bind individual cells
            for (int i = 0; i < InnerColumns.Count; i++)
            {
                var col = InnerColumns[i];
                var control = panel.Children[i] as FrameworkElement;
                col.UnbindCellContent(control, row);
            }
        }

        #endregion

        #region Editing

        public override FrameworkElement GetCellEditingContent(C1.WPF.DataGrid.DataGridRow row)
        {
            // initialize grid
            var panel = new Grid();

            foreach (var col in InnerColumns)
            {
                panel.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(col.Width.Value) });
            }

            // add individual content
            for (int i = 0; i < InnerColumns.Count; i++)
            {
                var col = InnerColumns[i];
                var content = col.GetCellEditingContent(row);//////bug for Missing
                Grid.SetColumn(content, i);
                panel.Children.Add(content);
            }
            return panel;
        }

        public override object PrepareCellForEdit(FrameworkElement editingElement)
        {
            // compose all the values into a list of objects
            List<object> values = new List<object>();
            var children = (editingElement as Panel).Children;

            for (int i = 0; i < InnerColumns.Count; i++)
            {
                var value = InnerColumns[i].PrepareCellForEdit(children[i] as FrameworkElement);
                values.Add(value);
            }
            return values;
        }

        public override void CancelCellEdit(FrameworkElement editingElement, object uneditedValue)
        {
            //decompose all the values from the list of objects
            //and invoke each of the cancels
            var values = (List<object>)uneditedValue;
            var children = (editingElement as Panel).Children;

            for (int i = 0; i < InnerColumns.Count; i++)
            {
                InnerColumns[i].CancelCellEdit(children[i] as FrameworkElement, values[i]);
            }
        }

        public override bool BeginEdit(FrameworkElement editingElement, RoutedEventArgs routedEventArgs)
        {
            // pass input to first column
            if (InnerColumns.Count > 0)
            {
                var children = (editingElement as Panel).Children;
                return InnerColumns[0].BeginEdit(children[0] as Control, routedEventArgs);
            }
            return false;
        }

        public override void EndEdit(FrameworkElement editingElement)
        {
            for (int i = 0; i < InnerColumns.Count; i++)
            {
                if (editingElement is Panel)
                {
                    var children = (editingElement as Panel).Children;

                    if (children.Count > i)
                    {
                        InnerColumns[i].EndEdit(children[i] as Control);
                    }
                }
            }
        }

        #endregion
    }

    class DataGridAlignCol : C1.WPF.DataGrid.DataGridColumn
    {
        // Height of each level
        public static int LevelHeaderHeight = 18;

        // Inner Columns
        public ObservableCollection<C1.WPF.DataGrid.DataGridColumn> InnerColumns { get; set; }

        // Global Header
        public object CompositeHeader { get; set; }

        // Nested Levels
        public int NestedLevels { get; private set; }

        public DataGridAlignCol()
        {
            InnerColumns = new ObservableCollection<C1.WPF.DataGrid.DataGridColumn>();

            // the following features are not implemented
            CanUserResize = false;
            CanUserSort = false;
            CanUserFilter = false;
        }

        public void Update()
        {
            double totalWidth = 0;
            int maxNestedLevels = 0;

            // initialize grid
            var panel = new Grid();
            panel.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(DataGridValueLablesCol.LevelHeaderHeight) });
            panel.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(DataGridValueLablesCol.LevelHeaderHeight) });
            foreach (var col in InnerColumns)
            {
                // support nested scenarios
                var cc = col as DataGridValueLablesCol;

                if (cc != null)
                {
                    cc.Update();
                    maxNestedLevels = Math.Max(cc.NestedLevels, maxNestedLevels);
                }

                panel.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(col.Width.Value) });
                totalWidth += col.Width.Value;
            }

            // add global header
            var globalHeader = new ContentControl() { Content = CompositeHeader };
            Grid.SetColumnSpan(globalHeader, InnerColumns.Count);
            panel.Children.Add(globalHeader);

            // add individual headers
            for (int i = 0; i < InnerColumns.Count; i++)
            {
                var col = InnerColumns[i];

                // add nested headers
                var content = new DataGridColumnHeaderPresenter() { Content = col.Header, Background = new SolidColorBrush(Colors.Transparent) };
                Grid.SetColumn(content, i);
                Grid.SetRow(content, 1);
                panel.Children.Add(content);
            }

            // update header & global width
            Header = panel;
            Width = new C1.WPF.DataGrid.DataGridLength(totalWidth);
            NestedLevels = 1 + maxNestedLevels;
        }

        #region Cell Content

        public override object GetCellContentRecyclingKey(C1.WPF.DataGrid.DataGridRow row)
        {
            return typeof(DataGridValueLablesCol);
        }

        public override FrameworkElement CreateCellContent(C1.WPF.DataGrid.DataGridRow row)
        {
            // initialize grid
            var panel = new Grid();

            foreach (var col in InnerColumns)
            {
                panel.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(col.Width.Value) });
            }

            // add individual content
            for (int i = 0; i < InnerColumns.Count; i++)
            {
                var col = InnerColumns[i];
                var content = col.CreateCellContent(row);
                //e.Column = new C1.WPF.DataGrid.DataGridComboBoxColumn(e.Property);
                Grid.SetColumn(content, i);
                panel.Children.Add(content);
            }
            return panel;
        }

        public override void BindCellContent(FrameworkElement cellContent, C1.WPF.DataGrid.DataGridRow row)
        {
            Panel panel = (Panel)cellContent;

            // bind individual cells
            //for (int i = 0; i < InnerColumns.Count; i++)
            //{
            //    var col = InnerColumns[i];
            //    var control = panel.Children[i] as FrameworkElement;
            //    col.BindCellContent(control, row);
            //}
        }

        public override void UnbindCellContent(FrameworkElement cellContent, C1.WPF.DataGrid.DataGridRow row)
        {
            Panel panel = (Panel)cellContent;

            //// bind individual cells
            //for (int i = 0; i < InnerColumns.Count; i++)
            //{
            //    var col = InnerColumns[i];
            //    var control = panel.Children[i] as FrameworkElement;
            //    col.UnbindCellContent(control, row);
            //}
        }

        #endregion

        #region Editing

        public override FrameworkElement GetCellEditingContent(C1.WPF.DataGrid.DataGridRow row)
        {
            // initialize grid
            var panel = new Grid();

            foreach (var col in InnerColumns)
            {
                panel.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(col.Width.Value) });
            }

            // add individual content
            //    for (int i = 0; i < InnerColumns.Count; i++)
            //    {
            //        var col = InnerColumns[i];
            //        var content = col.GetCellEditingContent(row);
            //        Grid.SetColumn(content, i);
            //        panel.Children.Add(content);
            //    }
            return panel;
        }

        public override object PrepareCellForEdit(FrameworkElement editingElement)
        {
            // compose all the values into a list of objects
            List<object> values = new List<object>();
            var children = (editingElement as Panel).Children;

            //for (int i = 0; i < InnerColumns.Count; i++)
            //{
            //    var value = InnerColumns[i].PrepareCellForEdit(children[i] as FrameworkElement);
            //    values.Add(value);
            //}
            return values;
        }

        public override void CancelCellEdit(FrameworkElement editingElement, object uneditedValue)
        {
            // decompose all the values from the list of objects
            // and invoke each of the cancels
            //var values = (List<object>)uneditedValue;
            //var children = (editingElement as Panel).Children;

            //for (int i = 0; i < InnerColumns.Count; i++)
            //{
            //    InnerColumns[i].CancelCellEdit(children[i] as FrameworkElement, values[i]);
            //}
        }

        public override bool BeginEdit(FrameworkElement editingElement, RoutedEventArgs routedEventArgs)
        {
            // pass input to first column
            //if (InnerColumns.Count > 0)
            //{
            //    var children = (editingElement as Panel).Children;
            //    return InnerColumns[0].BeginEdit(children[0] as Control, routedEventArgs);
            //}
            return false;
        }

        public override void EndEdit(FrameworkElement editingElement)
        {
            //for (int i = 0; i < InnerColumns.Count; i++)
            //{
            //    if (editingElement is Panel)
            //    {
            //        var children = (editingElement as Panel).Children;
            //        if (children.Count > i)
            //        {
            //            InnerColumns[i].EndEdit(children[i] as Control);
            //        }
            //    }
            //}
        }

        #endregion
    }
    #endregion

}