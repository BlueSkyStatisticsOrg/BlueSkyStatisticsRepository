using BlueSky.CommandBase;
using BlueSky.Windows;
using BSky.ConfService.Intf.Interfaces;
using BSky.Database.Interface;
using BSky.Interfaces.Interfaces;
using BSky.Lifetime;
using BSky.Lifetime.Interfaces;
using BSky.RecentFileHandler;
using BSky.Statistics.Common;
using Microsoft.Practices.Unity;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace BlueSky.Commands.File
{
    class ImportFromSQLCommand : BSkyCommandBase
    {

        protected override void OnPreExecute(object param)
        {

        }

        public const String FileNameFilter = "All Files (*.*)|*.*|IBM SPSS (*.sav)|*.sav|Excel 2003 (*.xls)|*.xls|Excel 2007-2010 (*.xlsx)|*.xlsx|Comma Seperated (*.csv)|*.csv|DBF (*.dbf)|*.dbf|R Obj (*.RData)|*.RData|Dat (*.Dat)|*.Dat|SAS (*.sas7bdat)|*.sas7bdat";
        IConfigService confService = LifetimeService.Instance.Container.Resolve<IConfigService>();//12Dec2013
        ILoggerService logService = LifetimeService.Instance.Container.Resolve<ILoggerService>();//17Dec2012
        RecentDocs recentfiles = LifetimeService.Instance.Container.Resolve<RecentDocs>();//21Dec2013
        //XMLitemsProcessor defaultpackges = LifetimeService.Instance.Container.Resolve<XMLitemsProcessor>();//06Nov2014

        DatasetLoadingBusyWindow bw = null;
        protected override void OnExecute(object param)
        {
            initGlobalObjects();
            if (!AreDefaultRPackagesLoaded()) //Check before showing file open dialog
                return;


            //Window1 appwindow = LifetimeService.Instance.Container.Resolve<Window1>();

            ///Show first dialog where user selects the SQL database ( MS-SQL, MySQL, PostgreSLQ, Oracle etc..)
            DataSourceSelectorWindow dssw = new DataSourceSelectorWindow();
            dssw.Owner = appwindow;
            dssw.ShowDialog();

            logService.WriteToLogLevel("Done SQL table Loading: Now Grid Takes Over. ", LogLevelEnum.Info);

            //bring dataset window in front
            //MainWindow mwindow = LifetimeService.Instance.Container.Resolve<MainWindow>();
            //mwindow.Activate();
            BSkyMouseBusyHandler.HideMouseBusy();//HideProgressbar_old();
            appwindow.Activate();

            //This is going to be last tab so we can safely call Focus() on last tab 
            //without going deeper in DataSource or UIControllerService
            int totTabs = appwindow.documentContainer.Items.Count;
            TabItem ti = (appwindow.documentContainer.Items[totTabs - 1] as TabItem);//zero based index so -1
            ti.Focus();
        }

#region Progressbar
        Cursor defaultcursor;
        //Shows Progressbar
        private void ShowProgressbar_old()
        {
            defaultcursor = Mouse.OverrideCursor;
            Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
        }
        //Hides Progressbar
        private void HideProgressbar_old()
        {
            Mouse.OverrideCursor = null;
        }

        //in App Main Window stausbar
        //Shows Progressbar in statusbar
        private void ShowStatusProgressbar_old()
        {
            Window1 window = LifetimeService.Instance.Container.Resolve<Window1>();//27Oct2014
            //window.ProgressStatusPanel.Visibility = Visibility.Visible;
            defaultcursor = Mouse.OverrideCursor;
            Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
        }
        //Hides Progressbar in statusbar
        private void HideStatusProgressbar_old()
        {
            Window1 window = LifetimeService.Instance.Container.Resolve<Window1>();//27Oct2014
            //window.ProgressStatusPanel.Visibility = Visibility.Hidden;
            Mouse.OverrideCursor = defaultcursor;
        }
#endregion

        protected override void OnPostExecute(object param)
        {
        }

        IUnityContainer container = null;
        IDataService service = null;
        IUIController controller = null;
        Window1 appwindow = null;
        //bool isGlobalInitialized = false;
        private void initGlobalObjects()
        {
            // if (!isGlobalInitialized)
            {
                container = LifetimeService.Instance.Container;
                service = container.Resolve<IDataService>();
                controller = container.Resolve<IUIController>();
                //controller.sortasccolnames = null;//These 2 lines will make sure this is reset. Fix for issue with sort icon
                //controller.sortdesccolnames = null;// Sort dataset col. Close it. Reopen it and you still see sort icons.
                appwindow = LifetimeService.Instance.Container.Resolve<Window1>();//for refeshing recent files list
                //isGlobalInitialized = true;
            }
        }

        public void FileOpen(string filename)//21Feb 2013 For opening Dataset from File > Open & File > Recent
        {
            initGlobalObjects();
            if (!AreDefaultRPackagesLoaded())
                return;
            OpenDataset(filename);
        }

        //this is common. Called from FileOpen and OnExecute, only after checking default R packages
        private void OpenDataset(string filename)
        {
            // Start Some animation for loading dataset ///
            BSkyMouseBusyHandler.ShowMouseBusy();//ShowProgressbar_old();//ShowStatusProgressbar();//29Oct2014 

            if (filename != null && filename.Length > 0)
            {

                if (System.IO.File.Exists(filename))
                {
                    string sheetname = null;
                    if (filename.EndsWith(".xls") || filename.EndsWith(".xlsx"))//27Jan2014
                    {
                        object tbls = service.GetOdbcTableList(filename);
                        if (tbls != null)
                        {
                            SelectTableWindow stw = new SelectTableWindow();
                            string[] tlist = null;

                            if (tbls.GetType().Name.Equals("String"))
                            {
                                tlist = new string[1];
                                tlist[0] = tbls as string;
                            }
                            else if (tbls.GetType().Name.Equals("String[]"))
                            {
                                tlist = tbls as string[];

                            }

                            stw.FillList(tlist);
                            BSkyMouseBusyHandler.HideMouseBusy();//HideProgressbar_old();
                            stw.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                            stw.ShowDialog();
                            BSkyMouseBusyHandler.ShowMouseBusy();//ShowProgressbar_old();
                            if (stw.SelectedTableName == null)//cancel clicked
                            {
                                BSkyMouseBusyHandler.HideMouseBusy();//HideProgressbar_old();//HideStatusProgressbar();//29Oct2014 
                                return;
                            }
                            else
                                sheetname = stw.SelectedTableName;
                        }
                    }
                    logService.WriteToLogLevel("Setting DataSource: ", LogLevelEnum.Info);
                    DataSource ds = service.Open(filename, sheetname);
                    string errormsg = string.Empty;
                    if (ds != null && ds.Message != null && ds.Message.Length > 0) //message that is related to error
                    {
                        errormsg = "\n" + ds.Message;
                        ds = null;//making it null so that we stop executing further
                    }

                    if (ds != null)//03Dec2012
                    {
                        logService.WriteToLogLevel("Start Loading: " + ds.Name, LogLevelEnum.Info);
                        controller.LoadNewDataSet(ds);
                        logService.WriteToLogLevel("Finished Loading: " + ds.Name, LogLevelEnum.Info);
                        recentfiles.AddXMLItem(filename);//adding to XML file for recent docs
                    }
                    else
                    {
                        BSkyMouseBusyHandler.HideMouseBusy();//HideProgressbar_old();

                        //Following block is not needed
                        //StringBuilder sb = new StringBuilder();
                        //List<string> defpacklist = defaultpackges.RecentFileList;
                        //foreach(string s in defpacklist)
                        //{
                        //    sb.Append(s+", ");

                        //}
                        //sb.Remove(sb.Length - 1, 1);//removing last comma
                        //string defpkgs = sb.ToString();


                        MessageBox.Show(errormsg, BSky.GlobalResources.Properties.Resources.ErrOpeningFile + "(" + filename + ")", MessageBoxButton.OK, MessageBoxImage.Warning);
                        SendToOutputWindow(BSky.GlobalResources.Properties.Resources.ErrOpeningDataset, filename + errormsg);
                    }
                }
                else
                {
                    BSkyMouseBusyHandler.HideMouseBusy();//HideProgressbar_old();
                    MessageBox.Show(filename + " " + BSky.GlobalResources.Properties.Resources.DoesNotExist,
                        BSky.GlobalResources.Properties.Resources.FileNotFound, MessageBoxButton.OK, MessageBoxImage.Warning);
                    //If file does not exist. It should be removed from the recent files list.
                    recentfiles.RemoveXMLItem(filename);
                }
                //18OCt2013 move up for using in msg box   Window1 appwindow = LifetimeService.Instance.Container.Resolve<Window1>();//for refeshing recent files list
                appwindow.RefreshRecent();
            }
            BSkyMouseBusyHandler.HideMouseBusy();//HideProgressbar_old();// HideStatusProgressbar();//29Oct2014 

            //08Apr2015 bring main window in front after file open, instead of output window
            Window1 window = LifetimeService.Instance.Container.Resolve<Window1>();
            window.Activate();
        }

        ////Send executed command to output window. So, user will know what he executed
        //protected override void SendToOutputWindow(string command, string title)//13Dec2013
        //{
        //    #region Get Active output Window
        //    //////// Active output window ///////
        //    OutputWindowContainer owc = (LifetimeService.Instance.Container.Resolve<IOutputWindowContainer>()) as OutputWindowContainer;
        //    OutputWindow ow = owc.ActiveOutputWindow as OutputWindow; //get currently active window
        //    #endregion
        //    ow.AddMessage(command, title);
        //}

        public bool OpenDataframe(string dframename)
        {
            initGlobalObjects();
            if (!AreDefaultRPackagesLoaded())
                return false;
            BSkyMouseBusyHandler.ShowMouseBusy();//ShowProgressbar_old();//ShowStatusProgressbar();//29Oct2014
            bool isSuccess = false;


            string filename = controller.GetActiveDocument().FileName;
            //For Excel
            string sheetname = controller.GetActiveDocument().SheetName;
            if (sheetname == null) sheetname = string.Empty;
            // if dataset was already loaded last time then this time we want to refresh it
            bool isDatasetNew = service.isDatasetNew(dframename + sheetname);
            try
            {
                DataSource ds = service.OpenDataframe(dframename, sheetname);
                string errormsg = string.Empty;
                if (ds != null && ds.Message != null && ds.Message.Length > 0) //message that is related to error
                {
                    errormsg = "\n" + ds.Message;
                    ds = null;//making it null so that we do execute further
                }
                if (ds != null)//03Dec2012
                {
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
                    BSkyMouseBusyHandler.HideMouseBusy();//HideProgressbar_old();
                    MessageBox.Show(appwindow, BSky.GlobalResources.Properties.Resources.cantopen + " '" + dframename + "'" +
                        "\n" + BSky.GlobalResources.Properties.Resources.reasonsAre +
                        "\n" + BSky.GlobalResources.Properties.Resources.NotDataframe2 +
                        "\n" + BSky.GlobalResources.Properties.Resources.FormatNotSupported2 +
                        "\n" + BSky.GlobalResources.Properties.Resources.NoRowsColsPresent +
                        "\n" + BSky.GlobalResources.Properties.Resources.OldSessionRunning2 +
                        "\n" + BSky.GlobalResources.Properties.Resources.RSideIssue2,
                        BSky.GlobalResources.Properties.Resources.warning, MessageBoxButton.OK, MessageBoxImage.Warning);
                    SendToOutputWindow(BSky.GlobalResources.Properties.Resources.ErrOpeningDataset2, dframename + errormsg);
                }
            }
            catch (Exception ex)
            {
                logService.WriteToLogLevel("Error:" + ex.Message, LogLevelEnum.Error);
            }
            finally
            {
                //18OCt2013 move up for using in msg box   Window1 appwindow = LifetimeService.Instance.Container.Resolve<Window1>();//for refeshing recent files list
                //appwindow.RefreshRecent();
                BSkyMouseBusyHandler.HideMouseBusy();//HideProgressbar_old();//HideStatusProgressbar();//29Oct2014 
            }

            //if (isSuccess)
            //{
            //08Apr2015 bring main window in front after file open, instead of output window
            //Window1 window = LifetimeService.Instance.Container.Resolve<Window1>();
            //window.Activate();
            //}
            return isSuccess;
        }

        //Check for default pacakges those are required for opening datasets
        private bool AreDefaultRPackagesLoaded()
        {
            if (Window1.DatasetReqRPackagesLoaded)//Check global if TRUE no need to get results from R
            {
                return true;
            }

            bool alldefaultloaded = true;
            UAReturn retn = service.getMissingDefaultRPackages();
            string missinglist = (string)retn.SimpleTypeData;
            if (missinglist != null && missinglist.Length > 0)
            {
                alldefaultloaded = false;
            }
            if (!alldefaultloaded)
            {
                string firstmsg = BSky.GlobalResources.Properties.Resources.BSkyNeedsRPkgs + "\n\n";
                string msg = "\n\n" + BSky.GlobalResources.Properties.Resources.InstallReqRPkgFrmCRAN + "\n" + BSky.GlobalResources.Properties.Resources.InstallUpdatePkgMenuPath;
                MessageBox.Show(firstmsg + missinglist + msg, BSky.GlobalResources.Properties.Resources.MisingRPkgs, MessageBoxButton.OK, MessageBoxImage.Error);
            }

            //set global flag. if TRUE is set next line will not execute next time.
            // as the 'if' in the beginning will return the control
            Window1.DatasetReqRPackagesLoaded = alldefaultloaded;
            Window1.DatasetReqPackages = missinglist;

            return alldefaultloaded;
        }
    }
}
