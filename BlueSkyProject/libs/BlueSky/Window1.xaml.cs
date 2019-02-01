using BlueSky.Commands.Analytics.TTest;
using BlueSky.Commands.History;
using BlueSky.Commands.Output;
using BlueSky.Services;
using BlueSky.Windows;
using BSky.ConfService.Intf.Interfaces;
using BSky.Controls;
using BSky.Interfaces;
using BSky.Interfaces.DashBoard;
using BSky.Interfaces.Interfaces;
using BSky.Interfaces.Services;
using BSky.Lifetime;
using BSky.Lifetime.Interfaces;
using BSky.MenuGenerator;
using BSky.RecentFileHandler;
using BSky.Statistics.Common;
using BSky.Statistics.Service.Engine.Interfaces;
using Microsoft.Practices.Unity;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace BlueSky
{
    /// <summary>
    /// Interaction logic for Window1.xaml, the main application window.
    /// </summary>
    public partial class Window1 : Window
    {
        ILoggerService logService = LifetimeService.Instance.Container.Resolve<ILoggerService>();//17Jan2014

        IAnalyticsService _analyticService;
        OutputMenuHandler omh = new OutputMenuHandler();
        CommandHistoryMenuHandler chmh = new CommandHistoryMenuHandler();//04Mar2013

        bool _IsException = false;//19feb2013there is no Exception state in the begining.
        RecentDocs recentfiles = LifetimeService.Instance.Container.Resolve<RecentDocs>();//21Dec2013

        public static bool DatasetReqRPackagesLoaded { get; set; } //keeps track of minimum required packages to open Datasets
        public static string DatasetReqPackages { get; set; }// list of packages

        public bool IsCommercial 
        {
            get { return true; }
        }
        public Window1(IUnityContainer container, IAnalyticsService analytics, IDashBoardService dashBoardService)
        {
            InitializeComponent();
            try
            {

                //// rest of the old code //
                AssociateShortcutToolbarCommands();//18Mar2014
                recentfiles.recentitemclick = RecentItem_Click;//
                _analyticService = analytics;
                MainWindowMenuFactory mf = new MainWindowMenuFactory( Menu, maintoolbar, dashBoardService, "test");
                Menu.Items.Insert(Menu.Items.Count - 2, omh.OutputMenu);
                RefreshRecent();//recent menu
                Menu.Items.Insert(Menu.Items.Count - 2, chmh.CommandHistMenu);

                RDataShowWarningDialogCheck = false;


            }
            catch (Exception ex)//17Jan2014
            {
                MessageBox.Show(BSky.GlobalResources.Properties.UICtrlResources.MenuGenerationFailedMsg);
                logService.WriteToLogLevel("Menus can't be generated.\n" + ex.StackTrace, LogLevelEnum.Fatal);
                this.Close();
                return;
            }

            this.WindowState = System.Windows.WindowState.Normal;
            this.Dispatcher.UnhandledException += new DispatcherUnhandledExceptionEventHandler(Dispatcher_UnhandledException);
            UIControllerService layoutController = container.Resolve<UIControllerService>();

            layoutController.DocGroup = documentContainer;
            container.RegisterInstance<IUIController>(layoutController);

            AddGetModelControlToGrid();//Adding to the right of toolbar in its own grid.
        }

        #region Adding GetModelControl to the toolbar and also refesh option that can be invoked from anywhere(CustomSettingWindow's Apply)
        GetModelsControl modelcontrol; 
        private void AddGetModelControlToToolbar() 
        {

        }

        private void AddGetModelControlToGrid() 
        {
            DashBoardItem dbi = ScoreCommand();
            modelcontrol = new GetModelsControl(dbi);
            scoringGrid.Children.Add(modelcontrol);
        }

        //this is binding the 'Make Predictions' dialog to 'Score' button in GetModelsControl control.
        private DashBoardItem ScoreCommand()
        {
            DashBoardItem item = new DashBoardItem();
            UAMenuCommand cmd = new UAMenuCommand();
            cmd.commandtype = "BlueSky.Commands.Analytics.TTest.AUAnalysisCommandBase";

            cmd.commandtemplate = BSkyAppData.RoamingUserBSkyConfigL18nPath + "Make Predictions.xaml";
            cmd.commandformat = "";
            cmd.commandoutputformat = BSkyAppData.RoamingUserBSkyConfigL18nPath + "Make Predictions.xml";
            cmd.text = "Make Predictions";

            item.Command = CreateCommand(cmd);
            item.CommandParameter = cmd;

            return item;
        }

        private ICommand CreateCommand(UAMenuCommand cmd)
        {
            Type commandTypeObject = null;
            ICommand command = null;

            try
            {
                commandTypeObject = Type.GetType(cmd.commandtype);
                command = (ICommand)Activator.CreateInstance(commandTypeObject);
            }
            catch
            {
                //Create new command instance using default command dispatcher
                logService.WriteToLogLevel("Could not create command. " + cmd.commandformat, LogLevelEnum.Error);
            }

            return command;
        }

        public void setInitialAllModels()//Select All_Models as default
        {
            modelcontrol.setDefaultModel();
        }

        public void RefreshModelClassDropdown()
        {
            modelcontrol.RefreshModelClassList();
        }
        #endregion




        private void AssociateShortcutToolbarCommands()//18Mar2014
        {
            bNew.Command = new BlueSky.Commands.File.FileNewCommand();
            bOpen.Command = new BlueSky.Commands.File.FileOpenCommand();
            bSave.Command = new BlueSky.Commands.File.FileSaveCommand();
            bCut.Command = ApplicationCommands.Cut;
            bCopy.Command = ApplicationCommands.Copy;
            bPaste.Command = ApplicationCommands.Paste;
        }

        private void Dispatcher_UnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            ILoggerService logService = LifetimeService.Instance.Container.Resolve<ILoggerService>();
            logService.WriteToLogLevel("Unhandled Exception Occured in App's main window :", LogLevelEnum.Fatal, e.Exception);
            MessageBox.Show("Main Window:" + e.Exception.ToString());
            e.Handled = true;// if you false this and comment following line, then, exception goes to XAML Application Exception
            Environment.Exit(0);
        }

        bool _RDataShowWarningDialogCheck;

        public bool IsException 
        {
            set { _IsException = value; }
        }

        public OutputMenuHandler OMH
        {
            get { return omh; }
            set { omh = value; } /// check and remove if not needed
        }

        public CommandHistoryMenuHandler History 
        {
            get { return chmh; }
            set { chmh = value; } /// check and remove if not needed
        }

        public bool RDataShowWarningDialogCheck
        {
            get
            {
                return _RDataShowWarningDialogCheck;
            }

            set
            {
                _RDataShowWarningDialogCheck = value;
            }
        }

        

        #region refresh recent file list 21feb2013
        public void RefreshRecent()
        {
            MenuItem recent = GetMenuItemByHeaderPath("FileMenu>FileMenuRecent");
            try
            {
                if(recent!=null)
                    recentfiles.RecentMI = recent;
            }
            catch (Exception ex)//17Jan2014
            {
                MessageBox.Show(this, BSky.GlobalResources.Properties.UICtrlResources.RecentXMLnotfound);
                logService.WriteToLogLevel("Recent.xml not found.\n" + ex.StackTrace, LogLevelEnum.Fatal);
            }
        }

        //search in direction File>Open
        private MenuItem GetMenuItemByHeaderPath(string headerpath)
        {
            MenuItem mi = null;
            string[] patharr = headerpath.Split('>');// File, Open

            ///search MenuItem by searching Header
            foreach (string itm in patharr)
            {
                mi = FindItemInBranch(mi, itm);
            }

            return mi;
        }

        ///Find Item travesing thru a selected branch // this method will work with above funtion 'GetMenuByHeaderPath'
        private MenuItem FindItemInBranch(MenuItem ParentItem, string ChildHeader) 
        {
            MenuItem mi = null;
            if (ParentItem == null)//for Root node which is mainmenu
            {
                foreach (MenuItem itm in Menu.Items)
                {
                    if(itm.Name.Equals(ChildHeader))
                    {
                        mi = itm;
                        break;
                    }
                }
            }
            else
            {
                foreach (object oitm in ParentItem.Items)
                {
                    var casted = oitm as MenuItem;//if cast is possible or not
                    if (casted != null)
                    {
                        MenuItem itm = oitm as MenuItem;
                        if (itm.Name.Equals(ChildHeader))
                        {
                            mi = itm;
                            break;
                        }
                    }
                }
            }
            return mi;
        }

        private void RecentItem_Click(string fullpathfilename)
        {
            BlueSky.Commands.File.FileOpenCommand fopen = new BlueSky.Commands.File.FileOpenCommand();
            fopen.FileOpen(fullpathfilename);
        }

        #endregion

        #region Window operations
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)///15Jan2013
        {
            try
            {
                MainWindow mwindow = LifetimeService.Instance.Container.Resolve<MainWindow>();//28Jan2013
                mwindow.Activate();
                if (mwindow.OwnedWindows.Count > 1)
                {
                    System.Windows.Forms.DialogResult dresult = ExitAppMessageBox();//19feb2013
                    if (dresult == System.Windows.Forms.DialogResult.Yes)//save & exit
                    {

                        #region CLose Output Windows and then Close Syntax Eiditor
                        //// Close output window and synedtr window /// 05Feb2013
                        OutputWindowContainer owc = (LifetimeService.Instance.Container.Resolve<IOutputWindowContainer>()) as OutputWindowContainer;
                        SyntaxEditorWindow sewindow = LifetimeService.Instance.Container.Resolve<SyntaxEditorWindow>();

                        // First collect window names
                        string[] outwinnames = new string[owc.Count];
                        int i = 0;
                        foreach (KeyValuePair<String, IOutputWindow> item in owc.AllOutputWindows)
                        {
                            outwinnames[i] = item.Key;
                            i++;
                        }
                        //close each output window one by one.
                        foreach (string winname in outwinnames)
                        {
                            (owc.GetOuputWindow(winname) as Window).Close();//invoke close {Closing then Closed }
                        }

                        //now close Syntax Editor window
                        sewindow.SynEdtForceClose = true;
                        sewindow.Close();
                        if (owc.Count > 0 || sewindow != null && !sewindow.SynEdtForceClose)// if any of the output window is open
                        {
                            e.Cancel = true; // abort closing.
                        }
                        #endregion
                    }
                    else if (dresult == System.Windows.Forms.DialogResult.No) //Exit without save
                    {
                        
                    }
                    else //cancel exit. Keep the app open.
                    {
                        e.Cancel = true;
                        return;
                    }
                    
                }
            }
            catch (Exception ex)
            {
                logService.WriteToLogLevel("Error while closing:" + ex.Message, LogLevelEnum.Error);
            }
        }

        //load newly installed command //
        public void Window_Refresh_Menus()//15Jan2013
        {


        }

        //19feb2013 To show appropriate message while App Exit.
        private System.Windows.Forms.DialogResult ExitAppMessageBox()
        {
            System.Windows.Forms.DialogResult dresult;

            if (!_IsException) // If App Exit when users shuts down the App
            {
                dresult = System.Windows.Forms.MessageBox.Show(
                            BSky.GlobalResources.Properties.UICtrlResources.AppExitConfirmMsg+"\n"+
                            BSky.GlobalResources.Properties.UICtrlResources.AppExitConfirmMsg2,
                            BSky.GlobalResources.Properties.UICtrlResources.AppExitConfirmMsgTitle,
                            System.Windows.Forms.MessageBoxButtons.YesNoCancel,
                            System.Windows.Forms.MessageBoxIcon.Question);
            }
            else 
            {
                dresult = System.Windows.Forms.MessageBox.Show(
                            BSky.GlobalResources.Properties.UICtrlResources.AppExitOnErrorMsg
                            +"\n" + BSky.GlobalResources.Properties.UICtrlResources.AppExitConfirmMsg + "\n" +
                            BSky.GlobalResources.Properties.UICtrlResources.AppExitConfirmMsg2,
                            BSky.GlobalResources.Properties.UICtrlResources.AppExitOnErrorMsgTitle,
                            System.Windows.Forms.MessageBoxButtons.YesNoCancel,
                             System.Windows.Forms.MessageBoxIcon.Error);
            }
            return dresult;
        }
        #endregion

        private void Window_Activated(object sender, EventArgs e)
        {
            documentContainer.UpdateLayout();
            int totalDatagridTabs = documentContainer.Items.Count;
            if (totalDatagridTabs > 0)
            {
                (documentContainer.Items[totalDatagridTabs - 1] as TabItem).Focus();
                documentContainer.SelectedItem = documentContainer.Items[totalDatagridTabs - 1] as TabItem;// make last tab active
            }

        }

        private void bRefreshGrids_Click(object sender, RoutedEventArgs e)//Refresh GRids
        {
            AUAnalysisCommandBase auacb = new AUAnalysisCommandBase();
            auacb.RefreshBothGrids();
        }


        #region Find text in the Datagrid
        FindDatagridWindow fdw = null;
        FindVargridWindow fvw = null;
        private void bFindDataGrids_Click(object sender, RoutedEventArgs e)
        {
            string activetab = GetActiveTabOfActiveDataset(); //get active tab
            switch (activetab)
            {
                case "datagrid":
                    if (fdw == null)
                    {
                        fdw = new FindDatagridWindow(this);
                        fdw.Owner = this;
                        fdw.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner;
                    }
                    fdw.Show();
                    break;
                case "vargrid":
                    if (fvw == null)
                    {
                        fvw = new FindVargridWindow(this);
                        fvw.Owner = this;
                        fvw.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner;
                    }
                    fvw.Show();
                    break;
                default:
                    break;
            }
        }
        public void CloseFindDatagrid()
        {
            fdw = null;
        }

        public void CloseFindVargrid()
        {
            fvw = null;
        }
        #endregion

        #region Get Currently Active tab (data or variable tab)
        IUnityContainer container = null;
        IUIController controller = null;
        DataPanel dp = null;
        DataSource ds = null;

        private string GetActiveTabOfActiveDataset()
        {
            container = LifetimeService.Instance.Container;
            controller = container.Resolve<IUIController>();
            ds = controller.GetActiveDocument();
            TabItem ti = controller.GetTabItem(ds);
            dp = ti.Content as DataPanel;
            if (dp.datavartabs.SelectedIndex == 0)//datagrid is active
            {
                return "datagrid";
            }
            else if (dp.datavartabs.SelectedIndex == 1) //vargrid is active
            {
                return "vargrid";
            }
            else //something strange
            {
                return string.Empty;
            }
        }
        #endregion
        //30Sep2014 Refresh R side global vars etc..
        public void SetRDefaults()
        {
            IAnalyticsService analytics = LifetimeService.Instance.Container.Resolve<IAnalyticsService>();
            IConfigService confService = LifetimeService.Instance.Container.Resolve<IConfigService>();//23nov2012
            CommandRequest rcmd = new CommandRequest();

            //16Dec2013 read from config (this has priority, because it was set before closing BSky App
            string configdecidigits = confService.GetConfigValueForKey("noofdecimals");

            //Call R function to get Decimal digit. 
            rcmd.CommandSyntax = "BSkyGetDecimalDigitSetting()"; //get Decimal Digit
            object retres = analytics.ExecuteR(rcmd, true, false);
            string rdecidigits = retres != null ? retres.ToString() : string.Empty;

            int noofdecimaldigits;
            bool parsed;
            //Now use configdecidigits. 
            if (configdecidigits != null && configdecidigits.Trim().Length > 0)
            {
                parsed = Int32.TryParse(configdecidigits, out noofdecimaldigits);
            }
            else
            {
                parsed = Int32.TryParse(rdecidigits, out noofdecimaldigits);
            }

            //Call R function to set Decimal digit
            rcmd.CommandSyntax = "BSkySetDecimalDigitSetting(decimalDigitSetting = " + noofdecimaldigits + ")"; //Set Decimal Digit
            retres = analytics.ExecuteR(rcmd, false, false);


            ///20Oct2015 Set Scientific notation in R using config value set in C# config file.
            string configSciNotation = confService.GetConfigValueForKey("scientific");
            bool isSciNotation = (configSciNotation.ToLower().Equals("true")) ? true : false;
            string CapBoolStr = (isSciNotation) ? "TRUE" : "FALSE";
            //Call R function to set Scientific Notaion flag
            rcmd.CommandSyntax = "BSkySetEngNotationSetting( " + CapBoolStr + ")"; //Set Scientific Notation
            retres = analytics.ExecuteR(rcmd, false, false); 
        }

#region Statusbar custom message (messages other than license info)
        
        public void setLMsgInStatusBar(string message)
        {
            if (message == null || message.Trim().Length < 1)
                message = string.Empty; 

            licstatus.Text = message;
        }
#endregion

        //01Oct2015 If mouse is busy all the clicks are ignored. Same thing should be done to Output window.
        private void Window_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (System.Windows.Input.Mouse.OverrideCursor == System.Windows.Input.Cursors.Wait)//29Sep2015 disable if mouse is busy
                e.Handled = true;
            else
                e.Handled = false;
        }

        private void Comingsoonbtn_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("www.blueskystatistics.com");
        }
    }
}
