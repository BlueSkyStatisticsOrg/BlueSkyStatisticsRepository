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
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using BSky.Lifetime;
using BSky.Lifetime.Interfaces;
using BSky.ConfService.Intf.Interfaces;

namespace BSky.Database.Interface
{
    /// <summary>
    /// Interaction logic for DataSourceSelectorWindow.xaml
    /// </summary>
    public partial class DataSourceSelectorWindow : Window
    {

        IConfigService confService = LifetimeService.Instance.Container.Resolve<IConfigService>();//23nov2012

        public DataSourceSelectorWindow()
        {
            InitializeComponent();
            FillProviders();
        }

        private void GetProviders()
        {
            List<string> allProviders = new List<string>();
            // Retrieve the installed providers and factories.
            DataTable table = DbProviderFactories.GetFactoryClasses();

            foreach (DataRow row in table.Rows)
            {
                allProviders.Add(row[table.Columns[0]].ToString());
            }
            datasourceListBox.ItemsSource = allProviders;
        }

        private void FillProviders()
        {
            List<string> providers = new List<string>();
            //providers.Add("MSSQL(Full)");
            //providers.Add("MSSQL(Express)");

            providers.Add("MSSQL");
            providers.Add("PostgreSQL");
            providers.Add("Oracle");
            providers.Add("MySQL");
            providers.Add("SQLite");
            providers.Add("MS-ACCESS");
            datasourceListBox.ItemsSource = providers;
        }

        private void helpButton_Click(object sender, RoutedEventArgs e)
        {
            string path = @".\Config\Import SQL Configurations.pdf";
            try
            {
                System.Diagnostics.Process p1 = System.Diagnostics.Process.Start(path);
            }
            catch (System.ComponentModel.Win32Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
            }
        }

        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }


        // This is connect button.
        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            //Validations. If all required fields are populated by user then we proceed else we show message
            if (!AreRequiredFieldsPopulated())
            {
                return;
            }

            string serverType = datasourceListBox.SelectedItem.ToString();
            string conStr = CreateConnectionString();
            if (conStr == null)
            {
                return;
            }
            string selectedDatabase = databasetxt.Text;

            //BSkyWaitProgressBar bswpb = new BSkyWaitProgressBar("Connection in progress. Please wait...");
            //bswpb.Owner = this;
            //bswpb.Show();


            ShowProgressbar();
            TableColumnSelectorWindow tcsw = new TableColumnSelectorWindow(serverType, conStr, selectedDatabase);
            HideProgressbar();

            //bswpb.Close();

            //tcsw.ConnectionString = conStr;
            if (tcsw.IsConnectionSuccessful)
            {
                tcsw.Owner = this;
                tcsw.ShowDialog();
                if (tcsw.IsOKClicked) //if OK clicked then close the ServerType selection window. Else may be user want to select diff server
                    this.Close();
            }
        }

        private bool AreRequiredFieldsPopulated()
        {
            bool canProceed = true;
            //First find what SQL server is chosen. Based on that check what is required for which SQL server
            if (datasourceListBox.SelectedIndex < 0)
            {
                MessageBox.Show("SQL server not selected from the list", "Select SQL server", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                return false;
            }

            string serverType = datasourceListBox.SelectedItem.ToString();
            string host = hostNametxt.Text;
            string user = usertxt.Text;
            string pass = passwordBox1.Password;
            string databasename = null;

            StringBuilder sb = new StringBuilder();
            sb.Append("Following field(s) must not be left blank.");

            if (host == null || host.Trim().Length < 1) // no host/dsn
            {
                string hostlbl = label2.Content.ToString(); //dyanmically pic the label text
                sb.Append("\n"+hostlbl);
                canProceed = false;
            }
            if (user == null || user.Trim().Length < 1) // no user
            {
                if (serverType.Equals("MS-ACCESS")) //for MS-ACCESS user is optional
                {

                }
                else
                {
                    sb.Append("\nUser:");
                    canProceed = false;
                }
            }
            if (pass == null || pass.Trim().Length < 1) // no password
            {
                if (serverType.Equals("MS-ACCESS")) //for MS-ACCESS password is optional
                {

                }
                else
                {
                    sb.Append("\nPassword (if it exists):");
                }
            }
            if (serverType == "PostgreSQL") // database is mandatory for Postgres
            {
                databasename = databasetxt.Text;
                if (databasename == null || databasename.Trim().Length < 1) // no database name
                {
                    sb.Append("\nDatabase Name (if connecting to PostgreSQL server).");
                    canProceed = false;
                }
            }

            if (!canProceed)
            {
                MessageBox.Show(sb.ToString(), "Please fill mandatory fields.", MessageBoxButton.OK, MessageBoxImage.Asterisk);
            }
            return canProceed;
        }


        private string CreateConnectionString()
        {
            string serverType = datasourceListBox.SelectedItem.ToString();
            string mssqljdbcdrvpath = string.Empty;
            if (serverType.Contains("MSSQL")) //string MSSQL(Full) or MSSQL(Express) changes to MSSQL
            {
                serverType = "MSSQL";
                mssqljdbcdrvpath = confService.GetConfigValueForKey("mssqldrvpath");//12Jan2016
                if(!System.IO.File.Exists(mssqljdbcdrvpath))//if jar file does not exist(or path is wrong)
                {
                    MessageBox.Show("You must install MS-SQL Server Driver and set right path in configuration settings", "MS-SQL server JDBC driver not found", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return null;
                }
            }
            string host = hostNametxt.Text;

            string user = usertxt.Text;
            string pass = passwordBox1.Password;
            //string dbname = databasetxt.Text;
            //string connStr = "server=" + serveraddress + ";uid=" + user + ";pwd=" + pass + ";database=" + dbname;//for use in C#
            //string connStr = "'" + serverType + "', '" + host + "', '" + user + "', '" + pass + "', '" + dbname + "'";//for use in R DBI
            string connStr = "'" + serverType + "', '" + host + "', '" + user + "', '" + pass + "'";//for use in R DBI

            if (serverType.Equals("MSSQL"))
            {

                string fixstring = "jdbc:sqlserver://";
                
                //Let user enter the server instance name in the configuration and use that value here
                string serverInstanceName = confService.GetConfigValueForKey("servInstanceName");//11Dec2016

                //string boolconfig = confService.GetConfigValueForKey("mssqlfull");
                //string FullOrExpress = boolconfig.Trim().ToLower().Equals("true") ? "\\\\MSSQLSERVER" : "\\\\SQLEXPRESS";
                //string url = fixstring + host + FullOrExpress;
                string url = string.Empty;

                //09Aug2017 Following if/else was needed because if you try to use Amazon endpoint then there is no need of instance name.
                //earlier we tested with MSSQL on local PC and so Instance name was needed.
                if (serverInstanceName == null || serverInstanceName.Trim().Length == 0)
                {
                    url = fixstring + host;
                }
                else
                {
                    url = fixstring + host + "\\\\" + serverInstanceName;
                }


                ////09Aug2017  string url = fixstring + host + "\\\\" + serverInstanceName; //old code where server instance name was mandatory

                //string url = host;
                //connStr = "'" + serverType + "', '" + url + "', '" + user + "', '" + pass + "', '" + dbname + "'";//for use in R DBI
                connStr = "servertype='" + serverType + "', serveraddress='" + url + "', usr='" + user + "', pass='" + pass + "', mssqldrvjdbcpath='" + mssqljdbcdrvpath + "'";//for use in R DBI
            }

            return connStr;

        }


        SqlConnection conn;
        SqlCommand myCommand = null;
        SqlDataReader myreader = null;
        private void CreateConnection(string connStr)
        {
            conn = new SqlConnection(connStr);
            conn.Open();
        }

        private void CloseConnection()
        {
            conn.Close();
        }

        private void datasourceListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string serverType = datasourceListBox.SelectedItem.ToString();

            if (serverType != null && serverType.Equals("PostgreSQL"))
            {
                label5.Visibility = System.Windows.Visibility.Visible;
                databasetxt.Visibility = System.Windows.Visibility.Visible;
            }
            else if (serverType != null && serverType.Equals("MS-ACCESS")) //label2
            {
                label2.Content = "Data Source Name:";
            }
            else
            {
                label2.Content = "Host / Server:";

                label5.Visibility = System.Windows.Visibility.Hidden;
                databasetxt.Visibility = System.Windows.Visibility.Hidden;
            }

            //if (serverType != null && serverType.Equals("MSSQL(Full)"))
            //{
            //    hostNametxt.Text = "jdbc:sqlserver://127.0.0.1\\\\SQLSERVER";
            //}
            //else if (serverType != null && serverType.Equals("MSSQL(Express)"))
            //{
            //    hostNametxt.Text = "jdbc:sqlserver://127.0.0.1\\\\SQLEXPRESS";
            //}
            //else
            //    hostNametxt.Text = "";

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
            //ConnectionPB.Visibility = System.Windows.Visibility.Visible;
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
            //ConnectionPB.Visibility = System.Windows.Visibility.Hidden;
            Mouse.OverrideCursor = null;
        }

        #endregion


    }

}