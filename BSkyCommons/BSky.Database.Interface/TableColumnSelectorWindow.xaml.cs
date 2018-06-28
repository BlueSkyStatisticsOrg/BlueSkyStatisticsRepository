using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Data.SqlClient;
using System.Data;
using Microsoft.Practices.Unity;
using BSky.Interfaces.Interfaces;
using BSky.Lifetime;
using RDotNet;
using BSky.Statistics.Common;
using BSky.Lifetime.Interfaces;
using BSky.XmlDecoder;

namespace BSky.Database.Interface
{
    /// <summary>
    /// Interaction logic for TableColumnSelectorWindow.xaml
    /// </summary>
    public partial class TableColumnSelectorWindow : Window
    {
        ILoggerService logService = LifetimeService.Instance.Container.Resolve<ILoggerService>();//17Dec2012

        public TableColumnSelectorWindow()
        {
            InitializeComponent();
            container = LifetimeService.Instance.Container;
            service = container.Resolve<IDataService>();
            controller = container.Resolve<IUIController>();
            IsOKClicked = false;
        }

        //Parameterised is need to fill Database dropdown
        public TableColumnSelectorWindow(string servertype, string ConnStr, string SelDatabase)
        {
            InitializeComponent();
            ServerType = servertype;
            ConnectionString = ConnStr;
            SelectedDatabase = SelDatabase;
            container = LifetimeService.Instance.Container;
            service = container.Resolve<IDataService>();
            controller = container.Resolve<IUIController>();
            IsOKClicked = false;
            LoadDatabasesDropdown();
        }

        IUnityContainer container = null;
        IDataService service = null;
        IUIController controller = null;
        //Window1 appwindow = null;


        //This flag tells us if connection was successful and the shows this window else this window is not shown on failure.
        bool _isConnectionSuccessful = false;
        public bool IsConnectionSuccessful 
        {
            get { return _isConnectionSuccessful; }
        }

        string _serverType;
        public string ServerType 
        {
            get { return _serverType; }
            set { _serverType=value;} 
        }

        //Connectionstring ServerType/Server/User/Pass
        string _connStr;
        public string ConnectionString 
        {
            get 
            {
                //if (_selectedDatabase != null && _selectedDatabase.Length > 0)
                //    return _connStr + ", dbname=" + _selectedDatabase;
                //else
                    return _connStr;
            }
            set { _connStr = value;} 
        }

        string _selectedDatabase;
        public string SelectedDatabase 
        {
            get { return _selectedDatabase; }
            set { _selectedDatabase=value; }
        }


        public bool IsOKClicked { get; set; }

        #region Click event handlers
        
        private void backButton_Click(object sender, RoutedEventArgs e)
        {
            _selectedDatabase = string.Empty;
            IsOKClicked = false;
            this.Close();
        }

        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedDatabase == null || _selectedDatabase.Length < 1)//No Database selected. Select one first
            {
                MessageBox.Show(this, "Please select a database from the dropdown.", "No Database Selected.", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                return;
            }
            
            
            string finalquery = querytxt.Text;

            if (finalquery == null || finalquery.Length < 5) // < 1 may be used
            {
                MessageBox.Show(this, "Please type in a valid query in a textbox provided.", "Empty query.", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                return;
            }
            string availableDataframename = service.GetFreshDatasetName();//get new dataframe name, to be created in R after SQL executes
            ShowProgressbar();
            bool success = ExecuteQuery(finalquery, availableDataframename);
            HideProgressbar();
            if (success)
            {
                IsOKClicked = true;
                this.Close();
            }
        }

        private void tablesChkbox_Click(object sender, RoutedEventArgs e)
        {
            //if (tablesChkbox.IsChecked == true || viewsChkbox.IsChecked == true) //tables-views checked = true
            //{
            //    if (_selectedDatabase == null || _selectedDatabase.Length < 1)//No Database selected. Select one first
            //    {
            //        MessageBox.Show(this, "Please select a database from the dropdown.", "No Database Selected.", MessageBoxButton.OK, MessageBoxImage.Asterisk);
            //        return;
            //    }
            //    ShowProgressbar();
            //    //MessageBox.Show("Tables checkbox Checked = " + tablesChkbox.IsChecked);
            //    FillTablesViewsListBox();
            //    HideProgressbar();
            //}
            //else
            //{
            //    TablesViewsListBox.ItemsSource = null;
            //    ColumnsListBox.ItemsSource = null;
            //}
            AnyCheckBoxClick();
        }

        private void viewsChkbox_Click(object sender, RoutedEventArgs e)
        {
            AnyCheckBoxClick();
        }

        private void AnyCheckBoxClick()
        {
            if (tablesChkbox.IsChecked == true || viewsChkbox.IsChecked == true) //views checked =true
            {
                if (_selectedDatabase == null || _selectedDatabase.Length < 1)//No Database selected. Select one first
                {
                    MessageBox.Show(this, "Please select a database from the dropdown.", "No Database Selected.", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                    return;
                }
                ShowProgressbar();
                //MessageBox.Show("Views checkbox Checked = " + viewsChkbox.IsChecked);
                FillTablesViewsListBox();
                HideProgressbar();
            }
            else
            {
                TablesViewsListBox.ItemsSource = null;
                ColumnsListBox.ItemsSource = null;
            }
        }

        private void databaseSelectorDrop_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string selectedDatabase = databaseSelectorDrop.SelectedItem.ToString();
            if (selectedDatabase != null)
            {
                _selectedDatabase = selectedDatabase;

                //Refresh Tables/Views list in listbox as the database name is changed.
                ShowProgressbar();
                FillTablesViewsListBox();
                HideProgressbar();
            }
        }

        private void TablesViewsListBox_ItemClick(object sender, EventArgs e)
        {
            if (TablesViewsListBox.SelectedItem != null)
            {
                string selectedItem = TablesViewsListBox.SelectedItem.ToString();
                string tablename = selectedItem;

                ////For TABLE_NAME(TABLE_SCHEMA) format
                //if (selectedItem.Contains("("))
                //    tablename = selectedItem.Substring(0, selectedItem.IndexOf("(")).Trim();// extraced "student" from "student (dbo)"

                //For TABLE_SCHEMA.TABLE_NAME format
                if (selectedItem.Contains("."))
                    tablename = selectedItem.Substring(selectedItem.IndexOf(".")+1).Trim();// extraced "student" from "dbo.student"

                ColumnsListBox.ItemsSource = GetColumns(tablename);
            }
        }


        #endregion

        #region Get Databases

        private void LoadDatabasesDropdown()
        {
            string command = string.Empty;
            // "MSSQL","PostgreSQL","Oracle""MySQL","SQLite", "Access"
            switch (ServerType)
            {
                case "PostgreSQL":
                    command = "GetSQLDatabases(" + ConnectionString + ", databasename = '" + SelectedDatabase + "')";
                    break;
                case "MSSQL":
                case "MySQL":
                case "Oracle":
                case "SQLite":
                case "MS-ACCESS":
                    command = "GetSQLDatabases(" + ConnectionString + ")";
                    break;
                default:
                    command = string.Empty;
                    break;
            }

            if (command != null && command.Length > 0)
            {
                 List<string> databaseNames = GetListOfItemsExecutingQuery(command);
                 if (databaseNames != null) // SUCCESS
                 {
                     databaseSelectorDrop.ItemsSource = databaseNames;
                     _isConnectionSuccessful = true;
                 }
                 else //FAILURE
                 {
                     //HideProgressbar();
                     //_selectedDatabase = string.Empty;
                     //IsOKClicked = false;
                     //this.Close();
                     _isConnectionSuccessful = false;
                 }
            }
            HideProgressbar();
        }

        #endregion

        #region Get Tables and/or Views
        private void FillTablesViewsListBox()
        {
            TablesViewsListBox.ItemsSource = null;
            TablesViewsListBox.ItemsSource = GetTablesViews();
        }

        private List<string> GetTablesViews()
        {
            List<string> result = null;

            if (tablesChkbox.IsChecked == true && viewsChkbox.IsChecked == true) //get both, tables and views
            {
                result = GetBothTableAndViews();
            }
            else if (tablesChkbox.IsChecked == true) // get tables only
            {
                result = GetTables();
            }
            else if (viewsChkbox.IsChecked == true) // get views only
            {
                result = GetViews();
            }
            else // nothing is checked so clean the listbox
            {
                TablesViewsListBox.ItemsSource = null;
                ColumnsListBox.ItemsSource = null;
            }
            return result;
        }

        private List<string> GetBothTableAndViews()
        {
            //Get list of Tables in a database.
            string command = "GetSQLTablesAndViews(" + ConnectionString + ", databasename ='" + SelectedDatabase + "')";

            if (ServerType == "MS-ACCESS")
            {
                command = "GetSQLTablesAndViews(" + ConnectionString + ")";
            }
            else
            {
                command = "GetSQLTablesAndViews(" + ConnectionString + ", databasename ='" + SelectedDatabase + "')";
            }

            HideProgressbar();
            return(GetListOfItemsExecutingQuery(command));
            
        }

        private List<string> GetTables()
        {
            //Get list of Tables in a database.
            string command = null;// "GetSQLTables(" + ConnectionString + ", databasename ='" + SelectedDatabase + "')";


            if (ServerType == "MS-ACCESS")
            {
                command = "GetSQLTables(" + ConnectionString + ")";
            }
            else
            {
                command = "GetSQLTables(" + ConnectionString + ", databasename ='" + SelectedDatabase + "')";
            }

            HideProgressbar();
            return (GetListOfItemsExecutingQuery(command));
        }

        private List<string> GetViews()
        {
            //Get list of Tables in a database.
            string command = null;// "GetSQLViews(" + ConnectionString + ", databasename ='" + SelectedDatabase + "')";
            if (ServerType == "MS-ACCESS")
            {
                command = "GetSQLViews(" + ConnectionString + ")";
            }
            else
            {
                command = "GetSQLViews(" + ConnectionString + ", databasename ='" + SelectedDatabase + "')";
            }

            HideProgressbar();
            return (GetListOfItemsExecutingQuery(command));
        }

        #endregion

        #region Get Columns of Table or View

        SqlCommand myCommand = null;
        SqlDataReader myreader = null;


        private List<string> GetColumns(string tableOrViewName)//select * from information_schema.columns where table_name = 'tableName'
        {
            ////ShowProgressbar();
            ////List<string> allcolumns = new List<string>();
            ////string[] colList = null;
            //////Get List of columns of a table
            ////string connStr = _connStr;//"server=ANIL-FUJITSU\\SQLSERVER;uid=sa;pwd=P@ssw0rd;database=bskytestdb";

            //Get list of Tables in a database.

            string command = null;// "GetSQLTableColumns(" + ConnectionString + ", databasename ='" + SelectedDatabase + "', '" + tableOrViewName + "')";

            if (ServerType == "MS-ACCESS")
            {
                command = "GetSQLTableColumns(" + ConnectionString + ", tablename ='" + tableOrViewName + "')";
            }
            else
            {
                command = "GetSQLTableColumns(" + ConnectionString + ", databasename ='" + SelectedDatabase + "', '" + tableOrViewName + "')";
            }
            ////UAReturn uaret = service.ImportTableListFromSQL(command);

            ////if(uaret!=null)
            ////{
            ////    MessageBox.Show("UARET not null");
            ////}


            ////object allcols = null;
            ////if (allcols.GetType().Name == "String[]")
            ////{
            ////    colList = allcols as string[];
            ////}

            ////StringBuilder sb = new StringBuilder();
            ////List<string> collist = new List<string>();

            ////if (colList != null)
            ////{
            ////    int rowcount = 0, colcount = 0;
            ////    rowcount = colList.Length;

            ////    for (int i = 0; i < rowcount; i++)
            ////    {
            ////        collist.Add(colList[i]);
            ////    }
            ////}
            ////HideProgressbar();
            ////return collist;

            HideProgressbar();
            return (GetListOfItemsExecutingQuery(command));
        }

        #endregion


        private List<string> GetListOfItemsExecutingQuery(string query)
        {
            UAReturn uaret = service.ImportTableListFromSQL(query);
            OutputHelper.AnalyticsData.Result = uaret;//putting DOM
            string[,] ew = OutputHelper.GetBSkyErrorsWarning(1, "normal");
            if (ew != null)
            {
                StringBuilder errmsg = new StringBuilder();
                int rcount = ew.GetLength(0);
                int ccount = ew.GetLength(1);
                for (int r = 0; r < rcount; r++)
                {
                    for (int c = 0; c < ccount; c++)
                    {
                        if (ew[r, c].Contains("pass="))
                        {
                            int pstartidx = ew[r, c].IndexOf("pass=");
                            int pendidx = ew[r, c].IndexOf(",", pstartidx);
                            string pasSubstr = ew[r, c].Substring(pstartidx, (pendidx - pstartidx));
                            ew[r,c]=ew[r, c].Replace(pasSubstr, "pass='******'");
                        }
                        errmsg.Append(ew[r, c] + "\n");
                    }
                }

                //Removing Password so that its not visible in error message
                //if (errmsg..Contains("pass="))
                //{
                //    int pstartidx = ew[r, c].IndexOf("pass=");
                //    int pendidx = ew[r, c].IndexOf(",", pstartidx);
                //    string pasSubstr = ew[r, c].Substring(pstartidx, (pendidx - pstartidx - 1));
                //    ew[r, c].Replace(pasSubstr, "pass='******'");
                //}


                MessageBox.Show(errmsg.ToString(), "Errors/Warnings:", MessageBoxButton.OK, MessageBoxImage.Error);
                HideProgressbar();
                return null;
            }
            //No error so process further
            bool[] visibleRows;
            int datanumber = 1; // there is just one table that we are intrested in. Its first table in  /Root/UATableList

            int backup = OutputHelper.FlexGridMaxCells;//backup current value
            OutputHelper.FlexGridMaxCells = 1000; //change to required value
            string[,] allDatabases = OutputHelper.GetDataMatrix(datanumber, out visibleRows);// table data. 
            OutputHelper.FlexGridMaxCells = backup; //restore original value back again;

            StringBuilder sb = new StringBuilder();
            List<string> databaseList = new List<string>();

            if (allDatabases != null) 
            {
                int rowcount = 0, colcount = 0;
                rowcount = allDatabases.GetLength(0);
                colcount = allDatabases.GetLength(1);
                for (int i = 0; i < rowcount; i++)
                {
                    for (int j = 0; j < colcount; j++)
                    {
                        //TABLE_NAME(TABLE_SCHEMA) format
                        //if (j == 0) sb.Append(allDatabases[i, j].ToString());
                        //else sb.Append(" (" + allDatabases[i, j].ToString() + ")");

                        //TABLE_SCHEMAN.TABLE_NAME format
                        if (j == 0) sb.Append(allDatabases[i, j].ToString());
                        else sb.Insert(0,allDatabases[i, j].ToString() + ".");
                    }
                    databaseList.Add(sb.ToString());
                    sb.Clear();
                }
            }

            return(databaseList);
        }

        //This is to finally execute a query that will return a data frame that will be loaded in a grid.
        private bool ExecuteQuery(string query, string dframename)
        {
            //Get table using custom query
            DataFrame alltablesDF = null;
            //Get list of Tables in a database.
            string command = null;// "GetDataframe(" + ConnectionString + ", databasename ='" + SelectedDatabase + "', '" + query + "','" + dframename + "')";

            if (ServerType.Equals("MS-ACCESS"))
            {
                command = "GetDataframe(" + ConnectionString + ", query= '" + query + "', datasetname='" + dframename + "')";
            }
            else
            {
                command = "GetDataframe(" + ConnectionString + ", databasename ='" + SelectedDatabase + "', query= '" + query + "', datasetname='" + dframename + "')";
            }
            //object alltables = service.ImportTableListFromSQL(command);


            UAReturn uaret = service.ImportTableListFromSQL(command);
            OutputHelper.AnalyticsData.Result = uaret;//putting DOM

            //A table is always returned with single bool value that shows whether Dataset 
            //creation in R memory was success or not
            bool[] visibleRows;
            int datanumber = 1; // there is just one table that we are intrested in. Its first table in  /Root/UATableList
            bool isDatasetReady = false;
            int backup = OutputHelper.FlexGridMaxCells;//backup current value
            OutputHelper.FlexGridMaxCells = 1000; //change to required value
            string[,] datasetReady = OutputHelper.GetDataMatrix(datanumber, out visibleRows);
            OutputHelper.FlexGridMaxCells = backup; //restore original value back again;
            if (datasetReady != null)
            {
                isDatasetReady = datasetReady[0, 0].ToLower().Equals("true") ? true : false;
            }
            //// Now fetching errors if any
            string[,] ew = OutputHelper.GetBSkyErrorsWarning(0, "normal");//0 for automatically finding the tablenumber
            if (ew != null)
            {
                StringBuilder errmsg = new StringBuilder();
                int rcount = ew.GetLength(0);
                int ccount = ew.GetLength(1);
                for (int r = 0; r < rcount; r++)
                {
                    for (int c = 0; c < ccount; c++)
                    {
                        errmsg.Append(ew[r, c] + "\n");
                    }
                }

                MessageBox.Show(errmsg.ToString(), "Errors/Warnings:", MessageBoxButton.OK, MessageBoxImage.Error);
                HideProgressbar();
            }

            if (!isDatasetReady) // if dataset was not created in R memory due to wrong query or other reasons.
                return false;


            DataSource ds = service.OpenDataframe(dframename, "");
            bool isSuccess = false;
            string errormsg = string.Empty;
            if (ds != null && ds.Message != null && ds.Message.Length > 0) //message that is related to error
            {
                errormsg = "\n" + ds.Message;
                ds = null;//making it null so that we do execute further
            }
            if (ds != null)//03Dec2012
            {
                bool isDatasetNew = true;// service.isDatasetNew(dframename + "");
                logService.WriteToLogLevel("Start Loading Dataframe: " + ds.Name, LogLevelEnum.Info);
                if (isDatasetNew)
                    controller.Load_Dataframe(ds);
                else
                    controller.RefreshBothGrids(ds);//23Jul2015 .RefreshGrids(ds);//.RefreshDataSet(ds);
                ds.Changed = true; // keep track of change made, so that it can prompt for saving while closing dataset tab.
                logService.WriteToLogLevel("Finished Loading Dataframe: " + ds.Name, LogLevelEnum.Info);
                //recentfiles.AddXMLItem(dframename);//adding to XML file for recent docs
                isSuccess = true;
            }
            else
            {
                HideProgressbar();
                MessageBox.Show("Unable to open SQL dataframe'" + dframename + "'..." +
                    "\nReasons could be one or more of the following:" +
                    "\n1. Not a data frame object." +
                    "\n2. File format not supported (or corrupt file or duplicate column names)." +
                    "\n3. Dataframe does not have row(s) or column(s)." +
                    "\n4. R.Net server from the old session still running (use task manager to kill it)." +
                    "\n5. Some issue on R side (like: required library not loaded, incorrect syntax)." +
                    "\n6. Incorrect values provided while connecting to the SQL server.",
                    "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                //SendToOutputWindow("Error Opening Dataset.(probably not a data frame)", dframename + errormsg);
            }
            
            HideProgressbar();
            return true;
        }

              
        #region Progressbar is now busy cursor
        Cursor defaultcursor;
        //Shows Progressbar
        private void ShowProgressbar()
        {
            //bw = new DatasetLoadingBusyWindow("Please wait while Dataset is Loading...");
            //bw.Owner = (Application.Current.MainWindow);
            //bw.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            //bw.Visibility = Visibility.Visible;
            //bw.Show();
            //bw.Activate();
            defaultcursor = Mouse.OverrideCursor;
            Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
        }
        //Hides Progressbar
        private void HideProgressbar()
        {
            //if (bw != null)
            //{
            //    bw.Close(); // close window if it exists
            //    //bw.Visibility = Visibility.Hidden;
            //    //bw = null;
            //}
            Mouse.OverrideCursor = null;
        }

        #endregion



    }
}
