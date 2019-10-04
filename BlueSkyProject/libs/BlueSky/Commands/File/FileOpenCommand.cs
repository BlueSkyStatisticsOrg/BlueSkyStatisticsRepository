using System;
using Microsoft.Practices.Unity;
using Microsoft.Win32;
using System.Windows;
using BSky.Statistics.Common;
using BSky.Lifetime;
using BSky.Lifetime.Interfaces;
using System.IO;
using BlueSky.Windows;
using BlueSky.CommandBase;
using System.Windows.Input;
using BSky.RecentFileHandler;
using BSky.Interfaces.Interfaces;

using System.Diagnostics;
using BSky.Lifetime.Services;
using BlueSky.Dialogs;
using System.Text;
using BSky.Interfaces.Commands;
using System.Windows.Media;
using BSky.Controls;
using System.Globalization;
using BSky.ConfService.Intf.Interfaces;
using BSky.ConfigService.Services;
using BSky.Controls.Controls;

namespace BlueSky.Commands.File
{
    public class FileOpenCommand : BSkyCommandBase
    {
        protected override void OnPreExecute(object param)
        {
        }

        public const String FileNameFilter = "All Files (*.*)|*.*|IBM SPSS (*.sav)|*.sav|Excel 2003 (*.xls)|*.xls|Excel 2007-2010 (*.xlsx)|*.xlsx|Comma Seperated (*.csv)|*.csv|DBF (*.dbf)|*.dbf|R Object (*.RData)|*.RData|Dat (*.Dat)|*.Dat|SAS (*.sas7bdat)|*.sas7bdat|Txt (*.Txt)|*.Txt";
        IConfigService confService = LifetimeService.Instance.Container.Resolve<IConfigService>();//12Dec2013
        ILoggerService logService = LifetimeService.Instance.Container.Resolve<ILoggerService>();//17Dec2012
        RecentDocs recentfiles = LifetimeService.Instance.Container.Resolve<RecentDocs>();//21Dec2013
        //XMLitemsProcessor defaultpackges = LifetimeService.Instance.Container.Resolve<XMLitemsProcessor>();//06Nov2014

        bool AdvancedLogging;

        DatasetLoadingBusyWindow bw = null;

        protected override void OnExecute(object param)
        {
            initGlobalObjects();
            if (!AreDefaultRPackagesLoaded()) //Check before showing file open dialog
                return;
            AdvancedLogging = AdvancedLoggingService.AdvLog;//08May2017

            OpenFileDialog openFileDialog = new OpenFileDialog();
            //// Get initial Dir 12Feb2013 ////
            string initDir = confService.GetConfigValueForKey("InitialDirectory");

            if (AdvancedLogging) logService.WriteToLogLevel("Initial Directory: " + initDir, LogLevelEnum.Info);

            if (!string.IsNullOrEmpty(initDir) && Directory.Exists(initDir))
                openFileDialog.InitialDirectory = initDir;
            else
                openFileDialog.InitialDirectory = "";
            openFileDialog.Filter = FileNameFilter;

            Window1 appwin = LifetimeService.Instance.Container.Resolve<Window1>();
            bool? output = openFileDialog.ShowDialog(appwin);//Application.Current.MainWindow);

            if (output.HasValue && output.Value)
            {
                //some code from here moved to FileOpen //
                //FileOpen(openFileDialog.FileName);

                OpenDataset(openFileDialog.FileName);

                /// Stop the animation after loading ///

                ///Set initial Dir. 12Feb2013///
                initDir = Path.GetDirectoryName(openFileDialog.FileName);
                confService.ModifyConfig("InitialDirectory", initDir);
                confService.RefreshConfig();
            }
            logService.WriteToLogLevel("Done File Loading: Now Grid Takes Over. ", LogLevelEnum.Info);

            //bring dataset window in front
            //MainWindow mwindow = LifetimeService.Instance.Container.Resolve<MainWindow>();
            //mwindow.Activate();
        }

        #region Progressbar
        Cursor defaultcursor;
        //Shows Progressbar
        private void ShowProgressbar_old()
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
        private void HideProgressbar_old()
        {
            //if (bw != null)
            //{
            //    bw.Close(); // close window if it exists
            //    //bw.Visibility = Visibility.Hidden;
            //    //bw = null;
            //}
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

        public void FileOpen(string filename, bool afterSaveAs = false)//21Feb 2013 For opening Dataset from File > Open & File > Recent
        {
            initGlobalObjects();
            if (!AreDefaultRPackagesLoaded())
                return;
            OpenDataset(filename, afterSaveAs);
        }

        //this is common. Called from FileOpen and OnExecute, only after checking default R packages
        private void OpenDataset(string filename, bool afterSaveAs = false)
        {
            AdvancedLogging = AdvancedLoggingService.AdvLog;//08Aug2016
            // Start Some animation for loading dataset ///
            BSkyMouseBusyHandler.ShowMouseBusy();// ShowProgressbar_old();//ShowStatusProgressbar();//29Oct2014 

            string showRDataWarningStr = confService.GetConfigValueForKey("RDataOpenWarning");
            bool showRDataWarning = showRDataWarningStr.ToLower().Equals("true") ? true : false;

            string errormsg = string.Empty;
            DataSource ds = null;
            IOpenDataFileOptions csvo = new OpenDataFileOptions();//
			bool removeSpacesSPSS = false;//for SPSS files.											   

            if (filename != null && filename.Length > 0)
            {
                if (System.IO.File.Exists(filename))
                {
                    //23Oct2016(2) Blank not working in the following.
                    string sheetname = "";//23Oct2016(1) null replaced by empty. File>recent opens aonther grid tab if the current grid has become null

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
                            BSkyMouseBusyHandler.HideMouseBusy();// HideProgressbar_old();
                            stw.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                            stw.ShowDialog();
                            BSkyMouseBusyHandler.ShowMouseBusy();// ShowProgressbar_old();
                            if (stw.SelectedTableName == null)//cancel clicked
                            {
                                BSkyMouseBusyHandler.HideMouseBusy();// HideProgressbar_old();//HideStatusProgressbar();//29Oct2014 
                                return;
                            }
                            else
                                sheetname = stw.SelectedTableName;
                        }
                    }
                    else if (filename.ToLower().EndsWith(".rdata") && showRDataWarning && !afterSaveAs)//dont show warning if file is auto opened after 'SaveAs'
                    {
                        if (!appwindow.RDataShowWarningDialogCheck) //for first time and also when checkbox is not checked
                        {
                            string s1 = BSky.GlobalResources.Properties.Resources.OpenRDataWarnMsgLine1; //Not in use. Replaced by s1a, s1b
                            string s2 = "\n\n" + BSky.GlobalResources.Properties.Resources.OpenRDataWarnMsgLine2;
                            string s3 = "\n" + BSky.GlobalResources.Properties.Resources.OpenRDataWarnMsgLine3;
                            string s4 = "\n\n" + BSky.GlobalResources.Properties.Resources.OpenRDataWarnMsgLine4;
                            string s5 = "\n\n" + BSky.GlobalResources.Properties.Resources.OpenRDataWarnMsgLine5;
                            string s6 = "\n\n" + BSky.GlobalResources.Properties.Resources.OpenRDataWarnMsgLine6;
                            string s7 = "\n\n" + BSky.GlobalResources.Properties.Resources.OpenRDataWarnMsgLine7;

                            //string s1 = "Loading RDATA file may overwrite your current variables/objects in memory if variables/objects with the same name are already present in the RDATA file.";
                            string s1a = BSky.GlobalResources.Properties.Resources.OpenRDataWarnMsgLine1a;//
                            string s1b = BSky.GlobalResources.Properties.Resources.OpenRDataWarnMsgLine1b; ;//

                            //string s2 = "\n\nYou can cancel out of here and save your all your current variables/objects which you can load later if required.";
                            //string s3 = "\nTo save/load your current variables/object run following commands from BlueSky R command editor.";
                            //string s4 = "\n\nTo save:  save.image(file='filename.rdata') # filename.rdata is the file name where objects will be saved.";
                            //string s5 = "\n\nTo load:  load(file='filename.rdata') # filename.rdata is the file name from which objects will be loaded.";
                            //string s6 = "\n\n'filename.rdata' is filename with forward slashed path e.g. 'C:/myfolder/myobjects.rdata' ";
                            //string s7 = "\n\nDo you want to proceed? [Variables with matching names will be overwritten].";
                            //MessageBoxResult messageBoxResult = MessageBox.Show(s1a + s1b + s2 + s3 + s4 + s5 + s6 + s7, "Please Confirm...", MessageBoxButton.YesNo, MessageBoxImage.Question);
                            //if (messageBoxResult == MessageBoxResult.Yes)
                            //{
                            //    //MessageBox.Show("You selected to proceed");
                            //}
                            //else
                            //{
                            //    //MessageBox.Show("You selected to cancel");
                            //    BSkyMouseBusyHandler.HideMouseBusy();
                            //    return;
                            //}

                            RDataWarningMessageBox rdataMBox = new RDataWarningMessageBox();
                            rdataMBox.Msg = s1a;
                            rdataMBox.AdvMsg = s1b + s2 + s3 + s4 + s5 + s6 + s7;
                            BSkyMouseBusyHandler.HideMouseBusy();
                            rdataMBox.ShowDialog();
                            BSkyMouseBusyHandler.ShowMouseBusy();
                            if (rdataMBox.NotShowCheck)//do not show the checkbox again
                            {
                                appwindow.RDataShowWarningDialogCheck = true;
                            }
                            if (rdataMBox.ButtonClicked == "Cancel")
                            {
                                BSkyMouseBusyHandler.HideMouseBusy();
                                return;
                            }
                        }
                    }
                    else if (filename.ToLower().EndsWith(".r"))// its R script
                    {
                        //errormsg = "To open a R script, you need to switch to the 'Output and Syntax' window.\n" +
                        //    "Go to the right hand pane of the 'Output and Syntax' window and click File -> Open in the 'R Command Editor'.";

                        errormsg = BSky.GlobalResources.Properties.Resources.OpenRScriptInSynWin + "\n" +
                            BSky.GlobalResources.Properties.Resources.OpenRScriptMenuPath;
                        ds = null;

                        BSkyMouseBusyHandler.HideMouseBusy();// HideProgressbar_old();
                        MessageBox.Show(errormsg, BSky.GlobalResources.Properties.Resources.ErrOpeningFile + "(" + filename + ")", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                        SendToOutputWindow(BSky.GlobalResources.Properties.Resources.ErrOpeningDataset, filename + errormsg);
                        return;
                    }
                    else if (filename.ToLower().EndsWith(".txt") || filename.ToLower().EndsWith(".csv") || filename.ToLower().EndsWith(".dat"))
                    {
                        OpenCSVTXTOptionsWindow csvopwin = new OpenCSVTXTOptionsWindow();
                        csvopwin.Owner = appwindow;
                        csvopwin.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                        BSkyMouseBusyHandler.HideMouseBusy();
                        csvopwin.ShowDialog();
                        //get all parameters
                        csvo = csvopwin.csvtxtOptions;
                        if (csvo == null)//user clicked cancel
                        {
                            return;
                        }

                        BSkyMouseBusyHandler.ShowMouseBusy();
                        //do further processing by passing it into service.open()
                    }
                    else if (filename.EndsWith(".sav"))
                    {
                        OpenSPSSoptionsWindow spssOpt = new OpenSPSSoptionsWindow();
                        spssOpt.Owner=appwindow;
                        BSkyMouseBusyHandler.HideMouseBusy();
                        spssOpt.ShowDialog();
                        BSkyMouseBusyHandler.ShowMouseBusy();
                        removeSpacesSPSS = spssOpt.TrimSpaces;
                    }
                    logService.WriteToLogLevel("Setting DataSource: ", LogLevelEnum.Info);

                    Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();
                    long elapsed = 0;

                    // if RData file then get all the data.frame and tbl_df objects. 
                    //Give users choice to load one or more of available data.frames in the grid.
                    if (filename.ToLower().EndsWith(".rdata")) //&& !afterSaveAs)
                    {
                        bool isSingleDFinFile = false;//if there is a single data.frame object in a file and no other obj.
                        bool isOneBlank = false;
                        string[] selectedDF = null;
                        //get list of data.frame or tbl_df objects from R
                        object tbls = service.GetRDataDataframeObjList(filename);
                        if (tbls != null && !tbls.ToString().Equals("No Result - Check Command"))
                        {
                            string[] tlist = null;

                            if (tbls.GetType().Name.Equals("String"))
                            {
                                tlist = new string[1];
                                tlist[0] = tbls as string;
                                isSingleDFinFile = true;
                            }
                            else if (tbls.GetType().Name.Equals("String[]"))
                            {
                                tlist = tbls as string[];
                                if (tlist.Length == 2)
                                {
                                    if (tlist[1].Trim().Length == 0)//blank entry. Also represent there are other objs in Rdata
                                    {
                                        isOneBlank = true;
                                        //remove blank entry
                                        string tempentry = tlist[0];
                                        tlist = new string[1];
                                        tlist[0] = tempentry;
                                    }
                                }
                            }

                            if (tlist != null && tlist.Length > 1)
                            {
                                BSkyMouseBusyHandler.HideMouseBusy();
                                SelectRdataDataframesWindow srdataw = new SelectRdataDataframesWindow();
                                srdataw.LoadListbox(tlist);
                                srdataw.ShowDialog();
                                string DlgRes = srdataw.DlgResult;
                                if (DlgRes.Equals("Ok"))
                                {
                                    BSkyMouseBusyHandler.ShowMouseBusy();
                                    selectedDF = srdataw.SelectedDFList;

                                }
                                else // 'Cancel' clicked
                                {
                                    //no code here. User aborted the loading in the grid but he already have RData file loaded.
                                    return;
                                }
                            }
                            else if (tlist != null && tlist.Length == 1)
                            {
                                selectedDF = tlist;
                            }

                            #region Load seleted data.frames
                            if (selectedDF != null || selectedDF.Length > 0)
                            {
                                string title = "RData file loaded : " + filename;
                                //SendToOutputWindow("", title, false);
                                PrintTitle(title);

                                //load all the selected dataframes
                                StringBuilder sb = new StringBuilder();
                                StringBuilder selectedDSnames = new StringBuilder("Loaded ");
                                int selidx = 1;
                                foreach (string s in selectedDF)
                                {
                                    if (s.Equals("UAObj$obj")) //if it is old RData BSky proprietary format.
                                    {
                                        string RDataFilename = Path.GetFileNameWithoutExtension(filename);
                                        //remove special chars if any in the RDataFilename.
                                        RDataFilename = RemoveSplChars(RDataFilename);

                                        sb.Append(RDataFilename + " <- as.data.frame(" + s + ") ;");
                                        sb.Append("BSkyLoadRefreshDataframe(" + RDataFilename + ");");
                                        selectedDSnames.Append(RDataFilename);
                                    }
                                    else
                                    {
                                        sb.Append("BSkyLoadRefreshDataframe(" + s + ");"); //s must be data.frame name and not the nested data.frame obj.
                                        selectedDSnames.Append(s);
                                        if (selidx < selectedDF.Length)
                                        {
                                            selectedDSnames.Append(", ");
                                            selidx++;
                                        }
                                    }
                                }
                                selectedDSnames.Append("Data frames.");

                                string commands = sb.ToString();
                                SyntaxEditorWindow sewindow = LifetimeService.Instance.Container.Resolve<SyntaxEditorWindow>();
                                if(isSingleDFinFile)
                                    sewindow.RunCommands(commands, null,filename);
                                else
                                    sewindow.RunCommands(commands, null);

                                //sewindow.SendCommandToOutput("RData file loaded :: " + filename, "RData Loaded");
                                sewindow.DisplayAllSessionOutput("Load RData file");
                                //SendToOutputWindow(filename + " loaded", selectedDSnames.ToString(), false);
                                recentfiles.AddXMLItem(filename);//adding to XML file for recent docs
                                appwindow.RefreshRecent();
                                return;
                            }
                            #endregion
                        }
                        //else
                        //{
                        //    MessageBox.Show("Data frame object not found!");
                        //    SendToOutputWindow(BSky.GlobalResources.Properties.Resources.ErrOpeningDataset, filename + errormsg);
                        //}
                    }
                    else
                    {

                        ds = service.Open(filename, sheetname, removeSpacesSPSS, csvo);
                        stopwatch.Stop();
                        elapsed = stopwatch.ElapsedMilliseconds;

                        if (AdvancedLogging) logService.WriteToLogLevel("PERFORMANCE:Both Dataset Opened and Col Attributes read: Time taken: " + elapsed, LogLevelEnum.Info);
                    }

                    if (ds != null && ds.Message != null && ds.Message.Length > 0) //message that is related to error
                    {
                        errormsg = "\n" + ds.Message;
                        ds = null;//making it null so that we stop executing further

                        // errormsg = BSky.GlobalResources.Properties.Resources.OpenRScriptInSynWin + "\n"+
                        //     BSky.GlobalResources.Properties.Resources.OpenRScriptMenuPath;
                        // ds = null;
                    }

                    if (ds != null && ds.Variables.Count > 0)//07Aug2016 added count condition to not open dataset if there are no variables //03Dec2012
                    {
                        logService.WriteToLogLevel("Start Loading: " + ds.Name, LogLevelEnum.Info);
                        //controller.LoadNewDataSet(ds);

                        stopwatch = System.Diagnostics.Stopwatch.StartNew();

                        if (ds.Replace)//if dataset became NULL(dataset exists in UI grid) and now we want to replace it by reading data from file
                        {
                            controller.RefreshBothGrids(ds);
                        }
                        else
                        {
                            controller.LoadNewDataSet(ds);
                        }
                        stopwatch.Stop();
                        elapsed = stopwatch.ElapsedMilliseconds;
                        if (AdvancedLogging) logService.WriteToLogLevel("PERFORMANCE:Creating virtual-Class and grid stuff: Time taken: " + elapsed, LogLevelEnum.Info);

                        logService.WriteToLogLevel("Finished Loading: " + ds.Name, LogLevelEnum.Info);
                        recentfiles.AddXMLItem(filename);//adding to XML file for recent docs
                    }
                    else
                    {
                        BSkyMouseBusyHandler.HideMouseBusy();// HideProgressbar_old();

                        //Following block is not needed
                        //StringBuilder sb = new StringBuilder();
                        //List<string> defpacklist = defaultpackges.RecentFileList;
                        //foreach(string s in defpacklist)
                        //{
                        //    sb.Append(s+", ");
                        //}
                        //sb.Remove(sb.Length - 1, 1);//removing last comma
                        //string defpkgs = sb.ToString();

                        //MessageBox.Show(errormsg, "Unable to open the file(" + filename + ")", MessageBoxButton.OK, MessageBoxImage.Warning);
                        //SendToOutputWindow("Error Opening Dataset", filename + errormsg);

                        MessageBox.Show(errormsg, BSky.GlobalResources.Properties.Resources.ErrOpeningFile + "(" + filename + ")", MessageBoxButton.OK, MessageBoxImage.Warning);
                        SendToOutputWindow(BSky.GlobalResources.Properties.Resources.ErrOpeningDataset, filename + errormsg);
                    }
                }
                else
                {
                    BSkyMouseBusyHandler.HideMouseBusy();// HideProgressbar_old();
                    MessageBox.Show(filename + " " + BSky.GlobalResources.Properties.Resources.DoesNotExist,
                        BSky.GlobalResources.Properties.Resources.FileNotFound, MessageBoxButton.OK, MessageBoxImage.Warning);
                    //If file does not exist. It should be removed from the recent files list.
                    recentfiles.RemoveXMLItem(filename);
                }
                //18OCt2013 move up for using in msg box   Window1 appwindow = LifetimeService.Instance.Container.Resolve<Window1>();//for refeshing recent files list
                appwindow.RefreshRecent();
            }
            BSkyMouseBusyHandler.HideMouseBusy();// HideProgressbar_old();// HideStatusProgressbar();//29Oct2014 

            //08Apr2015 bring main window in front after file open, instead of output window
            Window1 window = LifetimeService.Instance.Container.Resolve<Window1>();
            window.Activate();
        }

        //Creates a valid variable name for R/C# from the filename.
        private string RemoveSplChars(string str)
        {
            bool firstcharValid = true;
            int count = 0;
            StringBuilder newstr = new StringBuilder();
            foreach (char c in str)
            {
                if (count==0 && !(c >= 'A' && c <= 'Z') && !(c >= 'a' && c <= 'z'))
                {
                    newstr.Append("Rdata");
                }
                else if ((c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || ((c >= '0' && c <= '9') || (c == '_'))) //numbers and underscore can't be first chars
                {
                    newstr.Append(c);
                }
                else
                {
                    newstr.Append('_');
                }
                count++;
            }
            return newstr.ToString();
        }

        private void PrintTitle(string title)
        {
            CommandOutput batch = new CommandOutput();
            batch.NameOfAnalysis = "RData Load Command";
            batch.IsFromSyntaxEditor = false;
            batch.Insert(0, new BSkyOutputOptionsToolbar());

            string rcommcol = confService.GetConfigValueForKey("dctitlecol");//23nov2012 //before was syntitlecol
            byte red = byte.Parse(rcommcol.Substring(3, 2), NumberStyles.HexNumber);
            byte green = byte.Parse(rcommcol.Substring(5, 2), NumberStyles.HexNumber);
            byte blue = byte.Parse(rcommcol.Substring(7, 2), NumberStyles.HexNumber);
            Color c = Color.FromArgb(255, red, green, blue);
            AUParagraph aup = new AUParagraph();
            aup.Text = title; // dialogTitle;
            aup.FontSize = BSkyStyler.BSkyConstants.HEADER_FONTSIZE;//16;// before it was 16
            aup.FontWeight = FontWeights.DemiBold;
            aup.textcolor = new SolidColorBrush(c); //Colors.Blue);//SlateBlue //DogerBlue
            aup.ControlType = "Title";
            batch.Add(aup);
            SyntaxEditorWindow sewindow = LifetimeService.Instance.Container.Resolve<SyntaxEditorWindow>();
            sewindow.AddToSession(batch);
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

        public bool OpenDataframe(string dframename, string fname)
        {
            initGlobalObjects();
            AdvancedLogging = AdvancedLoggingService.AdvLog;//08Aug2016

            Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();

            if (!AreDefaultRPackagesLoaded())
            {
                stopwatch.Stop();
                return false;
            }
            stopwatch.Stop();

            long elapsed = stopwatch.ElapsedMilliseconds;

            if (AdvancedLogging) logService.WriteToLogLevel("PERFORMANCE:Are Req Package loaded: Time taken: " + elapsed, LogLevelEnum.Info);

            BSkyMouseBusyHandler.ShowMouseBusy();// ShowProgressbar_old();//ShowStatusProgressbar();//29Oct2014

            bool isSuccess = false;
            string filename = null, sheetname=null;
            DataSource tempds = controller.GetActiveDocument();
            if (tempds != null)
            {
                filename = tempds.FileName;
                //For Excel
                sheetname = tempds.SheetName;
            }
            if (filename == null) filename = string.Empty;
            if (sheetname == null) sheetname = string.Empty;
            // if dataset was already loaded last time then this time we want to refresh it
            bool isDatasetNew = service.isDatasetNew(dframename /*+ sheetname*/);

            try
            {
                stopwatch.Restart();
                if (fname.Length == 0)
                {
                    fname = dframename;
                }
                DataSource ds = service.OpenDataframe(dframename, sheetname, fname);
                //ds.FileName = fname;
                stopwatch.Stop();
                elapsed = stopwatch.ElapsedMilliseconds;
                if (AdvancedLogging) logService.WriteToLogLevel("PERFORMANCE:Open Dataset in R: Time taken: " + elapsed, LogLevelEnum.Info);

                string errormsg = string.Empty;

                if (ds != null && ds.Message != null && ds.Message.Length > 0) //message that is related to error
                {
                    errormsg = "\n" + ds.Message;
                    ds = null;//making it null so that we do execute further
                }
                if (ds != null && ds.Variables.Count > 0)//07Aug2016 added count condition to not open dataset if there are no variables //03Dec2012
                {
                    logService.WriteToLogLevel("Start Loading Dataframe: " + ds.Name, LogLevelEnum.Info);
                    if (isDatasetNew)
                    {
                        stopwatch.Restart();
                        controller.Load_Dataframe(ds);
                        stopwatch.Stop();
                        elapsed = stopwatch.ElapsedMilliseconds;
                        if (AdvancedLogging) logService.WriteToLogLevel("PERFORMANCE:Grid loading: Time taken: " + elapsed, LogLevelEnum.Info);
                    }
                    else
                    {
                        stopwatch.Restart();
                        controller.RefreshBothGrids(ds);//23Jul2015 .RefreshGrids(ds);//.RefreshDataSet(ds);
                        stopwatch.Stop();
                        elapsed = stopwatch.ElapsedMilliseconds;
                        if (AdvancedLogging) logService.WriteToLogLevel("PERFORMANCE:Grid loading: Time taken: " + elapsed, LogLevelEnum.Info);
                    }
                    //ds.Changed = true; // keep track of change made, so that it can prompt for saving while closing dataset tab.
                    logService.WriteToLogLevel("Finished Loading Dataframe: " + ds.Name, LogLevelEnum.Info);
                    //recentfiles.AddXMLItem(dframename);//adding to XML file for recent docs
                    isSuccess = true;
                }
                else
                {
                    BSkyMouseBusyHandler.HideMouseBusy();// HideProgressbar_old();

                    //MessageBox.Show(appwindow, "Unable to open '" + dframename + "'..." +
                    //    "\nReasons could be one or more of the following:" +
                    //    "\n1. Not a data frame object." +
                    //    "\n2. File format not supported (or corrupt file or duplicate column names)." +
                    //    "\n3. Dataframe does not have row(s) or column(s)." +
                    //    "\n4. R.Net server from the old session still running (use task manager to kill it)." +
                    //    "\n5. Some issue on R side (like: required library not loaded, incorrect syntax).",
                    //    "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    //SendToOutputWindow("Error Opening Dataset.(probably not a data frame)", dframename + errormsg);

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
                BSkyMouseBusyHandler.HideMouseBusy();// HideProgressbar_old();//HideStatusProgressbar();//29Oct2014 
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