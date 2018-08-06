using BlueSky.Commands.File;
using BlueSky.Services;
using BlueSky.Windows;
using BSky.ConfigService.Services;
using BSky.ConfService.Intf.Interfaces;
using BSky.Interfaces;
using BSky.Interfaces.Interfaces;
using BSky.Lifetime;
using BSky.Lifetime.Interfaces;
using BSky.Lifetime.Services;
using BSky.RecentFileHandler;
using BSky.ServerBridge;
using BSky.Statistics.Common;
using BSky.Statistics.Service.Engine.Interfaces;
using Microsoft.Practices.Unity;
using System;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Xml;

namespace BlueSky
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        UnityContainer container = new UnityContainer();

        public App()
        {
            //25Aug2017 To see how Date fomatting changes in the Datagrid
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");//US English en-US
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-US");

            //Calling order found was:: Dispatcher -> Main Window -> XAML Application Dispatcher -> Unhandeled
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
            Dispatcher.UnhandledException += new System.Windows.Threading.DispatcherUnhandledExceptionEventHandler(Dispatcher_UnhandledException);
            Application.Current.DispatcherUnhandledException += new System.Windows.Threading.DispatcherUnhandledExceptionEventHandler(Application_DispatcherUnhandledException);

            MainWindow mwindow = container.Resolve<MainWindow>();
            container.RegisterInstance<MainWindow>(mwindow);///new line
            mwindow.Show();
            mwindow.Visibility = Visibility.Hidden;

            ShowProgressbar();

            bool BlueSkyFound = true;

            string upath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "BlueSky"); ;//Per User path
            string applogpath = BSkyAppData.RoamingUserBSkyLogPath ;

            DirectoryHelper.UserPath =  upath ;

            LifetimeService.Instance.Container = container;
            container.RegisterInstance<ILoggerService>(new LoggerService(applogpath));

            //Check if user profies has got old files. If so overwrite all(dialogs, modelclasses and other xmls, txt etc.)
            bool hasOldUserContent = CheckIfOldInUserProfile();

            //Copy folders inside Bin/Config to User profile BlueSky\config.
            if(hasOldUserContent)
                CopyL18nDialogsToUserRoamingConfig();

            //copy config file to user profile. 
            CopyNewAndRetainOldSettings(string.Format(@"{0}BlueSky.exe.config", "./"),
                string.Format(@"{0}BlueSky.exe.config", BSkyAppData.RoamingUserBSkyConfigPath));

            container.RegisterInstance<IConfigService>(new ConfigService());//For App Config file

            container.RegisterInstance<IAdvancedLoggingService>(new AdvancedLoggingService());//For Advanced Logging
            ////////////// TRY LOADING BSKY R PACKAGES HERE  /////////

            ILoggerService logService = container.Resolve<ILoggerService>();
            logService.SetLogLevelFromConfig();//loading log level from config file
            logService.WriteToLogLevel("R.Net,Logger and Config loaded:", LogLevelEnum.Info);/// 

            ////Recent default packages. This code must appear before loading any R package. (including BlueSky R package)
            XMLitemsProcessor defaultpackages = container.Resolve<XMLitemsProcessor>();//06Feb2014
            defaultpackages.MaxRecentItems = 100;
            if(hasOldUserContent)
                CopyIfNotExistsOrOld(string.Format(@"{0}DefaultPackages.xml", BSkyAppData.BSkyAppDirConfigPath), string.Format(@"{0}DefaultPackages.xml", BSkyAppData.RoamingUserBSkyConfigPath));
            defaultpackages.XMLFilename = string.Format(@"{0}DefaultPackages.xml", BSkyAppData.RoamingUserBSkyConfigPath);//23Apr2015 ;BSkyAppData.BSkyDataDirConfigFwdSlash
            defaultpackages.RefreshXMLItems();
            container.RegisterInstance<XMLitemsProcessor>("defaultpackages", defaultpackages);

            //Recent user packages. This code must appear before loading any R package. (including uadatapackage)
            RecentItems userpackages = container.Resolve<RecentItems>();//06Feb2014
            userpackages.MaxRecentItems = 100;
            userpackages.XMLFilename = string.Format(@"{0}UserPackages.xml", BSkyAppData.RoamingUserBSkyConfigPath);//23Apr2015 @"./Config/UserPackages.xml";
            userpackages.RefreshXMLItems();
            container.RegisterInstance<RecentItems>(userpackages);

            ////User listed model classes.
            XMLitemsProcessor modelClasses = container.Resolve<XMLitemsProcessor>();//
            modelClasses.MaxRecentItems = 100;
            if (hasOldUserContent)
                CopyIfNotExistsOrOld(string.Format(@"{0}ModelClasses.xml", BSkyAppData.BSkyAppDirConfigPath), string.Format(@"{0}ModelClasses.xml", BSkyAppData.RoamingUserBSkyConfigPath));
            modelClasses.XMLFilename = string.Format(@"{0}ModelClasses.xml", BSkyAppData.RoamingUserBSkyConfigPath);//23Apr2015 ;BSkyAppData.BSkyDataDirConfigFwdSlash
            modelClasses.RefreshXMLItems();
            container.RegisterInstance<XMLitemsProcessor>("modelClasses", modelClasses);

            if (hasOldUserContent)
                CopyIfNotExistsOrOld(string.Format(@"{0}GraphicCommandList.txt", BSkyAppData.BSkyAppDirConfigPath), string.Format(@"{0}GraphicCommandList.txt", BSkyAppData.RoamingUserBSkyConfigPath));

            try
            {
                BridgeSetup.ConfigureContainer(container);
            }
            catch (Exception ex)
            {
                bool anothersessionrunning = false;
                if (ex.Message.Contains("used by another process"))
                    anothersessionrunning = true;
				
                string s1 = "\n" + BSky.GlobalResources.Properties.Resources.MakeSureRInstalled;
                string s2 = "\n" + BSky.GlobalResources.Properties.Resources.MakeSure32x64Compatibility;
                string s3 = "\n" + BSky.GlobalResources.Properties.Resources.MakeSureAnotherBSkySession;
                string s4 = "\n" + BSky.GlobalResources.Properties.Resources.MakeSureRHOME2LatestR;
                string s5 = "\n" + BSky.GlobalResources.Properties.Resources.PleaseMakeSure;
                string mboxtitle0 = "\n" + BSky.GlobalResources.Properties.Resources.CantLaunchBSkyApp;

                //MessageBox.Show(s5 + s3 + s4, mboxtitle0, MessageBoxButton.OK, MessageBoxImage.Stop);
                if(anothersessionrunning) MessageBox.Show(s5 + s3, mboxtitle0, MessageBoxButton.OK, MessageBoxImage.Stop);
                else MessageBox.Show(s5 + s4, mboxtitle0, MessageBoxButton.OK, MessageBoxImage.Stop);

                #region R Home Dir edit prompt
                if (!anothersessionrunning)
                {
                    //Provide R Home Dir check/modify option to the user so that he can have a chance to fix this issue.
                    //Otherwise app will not launch until reinstalled or manually modify the config file. 
                    //So following is much better and easier way to fix the issue.
                    HideMouseBusy();
                    HideProgressbar();
                    ChangeConfigForRHome();
							  
                }
                #endregion
                logService.WriteToLogLevel("Unable to launch the BlueSky Statistics Application." + s1 + s3, LogLevelEnum.Error);
                logService.WriteToLogLevel("Exception:" + ex.Message, LogLevelEnum.Fatal);
                Environment.Exit(0);
            }
            finally
            {
                HideProgressbar();
            }
            container.RegisterInstance<IDashBoardService>(container.Resolve<XmlDashBoardService>());
            container.RegisterInstance<IDataService>(container.Resolve<DataService>());

            IOutputWindowContainer iowc = container.Resolve<OutputWindowContainer>();
            container.RegisterInstance<IOutputWindowContainer>(iowc);

            SessionDialogContainer sdc = container.Resolve<SessionDialogContainer>();//13Feb2013
            //Recent Files settings
            RecentDocs rdoc = container.Resolve<RecentDocs>();//21Feb2013
            rdoc.MaxRecentItems = 7;
            rdoc.XMLFilename = string.Format(@"{0}Recent.xml", BSkyAppData.RoamingUserBSkyConfigPath);//23Apr2015 @"./Config/Recent.xml";
            container.RegisterInstance<RecentDocs>(rdoc);

            Window1 window = container.Resolve<Window1>();
            container.RegisterInstance<Window1>(window);///new line
            window.Closed += new EventHandler(window_Closed);  //28Jan2013                                                      
            window.Owner = mwindow;     //28Jan2013     

            //17Apr2017 Showing version number on titlebar of main window
            Version ver = Assembly.GetExecutingAssembly().GetName().Version;
            string strfullversion = ver.ToString(); //Full version with four parts
            string shortversion = string.Format("{0}.{1}", ver.Major.ToString(), ver.Minor.ToString());
            string titlemsg = string.Empty;

            titlemsg = string.Format("BlueSky Statistics (Open Source Desktop Edition. Ver- {0})", shortversion);

            window.Title = titlemsg;

            window.Show();
            window.Activate();
            ShowMouseBusy();//02Apr2015 show mouse busy
            //// one Syntax Editor window for one session ////29Jan2013
            SyntaxEditorWindow sewindow = container.Resolve<SyntaxEditorWindow>();
            container.RegisterInstance<SyntaxEditorWindow>(sewindow);///new line
            sewindow.Owner = mwindow;

#region Create Column Significance codes List
            SignificanceCodesHandler.CreateSignifColList();
#endregion

            //load default packages
            window.setLMsgInStatusBar(BSky.GlobalResources.Properties.Resources.StatMsgPkgLoad);
            IAnalyticsService IAService = LifetimeService.Instance.Container.Resolve<IAnalyticsService>();
            BridgeSetup.LoadDefaultRPackages(IAService);
            string PkgLoadStatusMessage = BridgeSetup.PkgLoadStatusMessage;

            if (PkgLoadStatusMessage != null && PkgLoadStatusMessage.Trim().Length > 0)
            {
                StringBuilder sb = new StringBuilder();
                string[] defpacklist = PkgLoadStatusMessage.Split('\n');

                foreach (string s in defpacklist)
                {
                    if (s != null && (s.ToLower().Contains("error")))//|| s.ToLower().Contains("warning")))
                    {
                        //sb.Append(s.Substring(0, s.IndexOf(". ")) + "\n");
                        sb.Append(s.Replace("Error loading R package:", "") + "\n");
                    }
                }
                if (sb.Length > 0)
                {
                    sb.Remove(sb.Length - 1, 1);//removing last comma

                    string defpkgs = sb.ToString();
                    string firstmsg = BSky.GlobalResources.Properties.Resources.ErrLoadingRPkg + "\n\n";
                    
                    string msg = "\n\n" + BSky.GlobalResources.Properties.Resources.InstallReqRPkgFrmCRAN + "\n" + BSky.GlobalResources.Properties.Resources.RegPkgsMenuPath;// +

                    HideMouseBusy();
                    string mboxtitle1 = BSky.GlobalResources.Properties.Resources.ErrReqRPkgMissing;
                    MessageBox.Show(firstmsg + defpkgs + msg, mboxtitle1, MessageBoxButton.OK, MessageBoxImage.Error);

                    Window1.DatasetReqPackages = defpkgs;

                    BlueSkyFound = false;
                }
            }

            //deimal default should be set here as now BlueSky is loaded
            window.SetRDefaults();
            IAdvancedLoggingService advlog = container.Resolve<IAdvancedLoggingService>(); ;//01May2015
            advlog.RefreshAdvancedLogging();

            window.setInitialAllModels();//select All_Models in model class dropdown.
            window.setLMsgInStatusBar("");
            HideMouseBusy();//02Apr2015 hide mouse busy
            if (BlueSkyFound)
            {
                try
                {
                    FileNewCommand newds = new FileNewCommand();
                    newds.NewFileOpen("");
                }
                catch (Exception ex)
                {
                    string so = BSky.GlobalResources.Properties.Resources.ErrLoadingNewDS;
                    string mboxtitle2 = BSky.GlobalResources.Properties.Resources.BSkyPkgMissing;
                    logService.WriteToLogLevel("ERROR: " + ex.Message, LogLevelEnum.Error);
                    MessageBox.Show(so, mboxtitle2, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

#region Check a flag if new files are copied to user profile

        private string[] GetAppVersionParts()
        {
            string appversion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            string[] verparts = appversion.Split('.');
            return verparts;
        }
        private bool CheckIfOldInUserProfile()
        {
            ILoggerService logService = container.Resolve<ILoggerService>();
            bool CopyfilesToUserProfile = true;
            string appversion = string.Empty;
            string[] verparts = GetAppVersionParts();
            if (verparts != null && verparts.Length >= 3)
            {
                appversion = verparts[0] + verparts[1]+ verparts[2];
            }
            else
            {
                return CopyfilesToUserProfile;
            }
            
            string versionline= "0000";//putting low version number
            string verfile = string.Format(@"{0}ver.txt", BSkyAppData.RoamingUserBSkyConfigPath);
            bool verfileExists = File.Exists(verfile);
            string[] lines = null;
            try
            {
                if (verfileExists)
                    lines = System.IO.File.ReadAllLines(@verfile);
            }
            catch (Exception ex)
            {
                logService.WriteToLogLevel("Error reading user profile version-file", LogLevelEnum.Error);
                logService.WriteToLogLevel("ERROR: " + ex.Message, LogLevelEnum.Error);


            }
            if (!verfileExists)
            {
                createUserProfileVerFile();
                CopyfilesToUserProfile = true;
            }
            else
            {
                int numOflines = lines.Length;
                if (numOflines >= 2)
                {
                    string[] parts = lines[1].Split('.');
                    if (parts != null)
                        versionline = parts[0] + parts[1] + parts[2];//no dots;
                }

                //conver to numeric
                long appver, verline;
                bool appversuccess = long.TryParse(appversion, out appver);
                bool verlinesuccess = long.TryParse(versionline, out verline);

                if (appver > verline)
                {
                    CopyfilesToUserProfile = true;
                    createUserProfileVerFile();
                }
                else
                {
                    CopyfilesToUserProfile = false;
                }
            }
            return CopyfilesToUserProfile;
        }

        private void createUserProfileVerFile()
        {
            ILoggerService logService = container.Resolve<ILoggerService>();
            string ver = Assembly.GetExecutingAssembly().GetName().Version.ToString();//"5.35.12345";
            string[] lines = { "Do not modify this file.", ver };
            string verfile = string.Format(@"{0}ver.txt", BSkyAppData.RoamingUserBSkyConfigPath);
            System.IO.StreamWriter file = null;
            try
            {
                file = new System.IO.StreamWriter(verfile,false);
                foreach (string line in lines)
                {
                    file.WriteLine(line);
                }
            }
            catch (Exception ex)
            {
                logService.WriteToLogLevel("Error creating user profile version-file", LogLevelEnum.Error);
                logService.WriteToLogLevel("ERROR: " + ex.Message, LogLevelEnum.Error);
            }
            finally
            {
                file.Close(); 
            }

        }
#endregion

#region Copy config
        private void CopyL18nDialogsToUserRoamingConfig()
        {
            string sourceDirectory = @".\Config";
            string targetDirectory = BSkyAppData.RoamingUserBSkyConfigPath;

            DirectoryInfo diSource = new DirectoryInfo(sourceDirectory);
            DirectoryInfo diTarget = new DirectoryInfo(targetDirectory);

            foreach (DirectoryInfo diSourceSubDir in diSource.GetDirectories())
            {
                DirectoryInfo nextTargetSubDir =  diTarget.CreateSubdirectory(diSourceSubDir.Name);
                CopyAll(diSourceSubDir, nextTargetSubDir);
            }
        }

        public static void CopyAll(DirectoryInfo source, DirectoryInfo target)
        {
            if (Directory.Exists(target.FullName) == false)
            {
                Directory.CreateDirectory(target.FullName);
            }

            foreach (FileInfo fi in source.GetFiles())
            {
                fi.CopyTo(Path.Combine(target.ToString(), fi.Name), true);
            }
        }

#endregion


        //copy xml files 
        private void CopyIfNotExistsOrOld(string source, string destination)
        {
            ILoggerService logService = container.Resolve<ILoggerService>();
            try
            {
                if (!File.Exists(destination)) 
                {
                    File.Copy(source, destination);
                }
                else 
                {
                        File.Copy(source, destination, true);
                }

            }
            catch (Exception ex)
            {
                logService.WriteToLogLevel("Error copying " + Path.GetFileName(source) + " to Roaming", LogLevelEnum.Error);
                logService.WriteToLogLevel("ERROR: " + ex.Message, LogLevelEnum.Error);
            }
        }

        private void CopyNewAndRetainOldSettings(string source, string destination)
        {
            bool userconfigexists = File.Exists(destination);
            
            if (userconfigexists)
            {
                NameValueCollection usrconfigs;
                usrconfigs = ReadUserProfConfig(destination) as NameValueCollection;

                
                NameValueCollection binconfigs;
                binconfigs = ReadUserProfConfig(source) as NameValueCollection;

                
                string usrconfigver = usrconfigs.Get("version");

                
                string binconfigver = binconfigs.Get("version");

                
                double usrver = 0.0;
                bool usrsuccess = Double.TryParse(usrconfigver, out usrver);

                double binver = 0.0;
                bool binsuccess = Double.TryParse(binconfigver, out binver);

                
                bool isbinnew = false;
                if (usrsuccess && binsuccess)
                {
                    if (binver > 0 && binver > usrver)
                    {
                        //bin has newer version
                        isbinnew = true;
                    }
                }
                else 
                {
                    isbinnew = true;
                }

                if (isbinnew)
                {
                    //copy bin config to user profile.
                    File.Copy(source, destination, true);

                    SaveConfigToFile(usrconfigs, destination);
                }
            }
            else
            {
                File.Copy(source, destination);
            }
        }

        //read config from user profile and get all settings
        private NameValueCollection ReadUserProfConfig(string destination)
        {
            NameValueCollection kvpairs = new NameValueCollection();

            XmlDocument xmldoc = new XmlDocument();
            XmlNodeList xmlnode;
            int i = 0;
            string str = null;
            string appConfigFullPathFilename = @destination;
            FileStream fs = new FileStream(appConfigFullPathFilename, FileMode.Open, FileAccess.Read);
            xmldoc.Load(fs);

            xmlnode = xmldoc.SelectNodes("/configuration/appSettings/add");
            for (i = 0; i <= xmlnode.Count - 1; i++)
            {
                string attrkey = xmlnode[i].Attributes["key"].Value;
                string attrVal = xmlnode[i].Attributes["value"].Value;
                kvpairs.Add(attrkey, attrVal);
            }
            fs.Close();
            return kvpairs;
        }

        //save config setting to a file
        private void SaveConfigToFile(NameValueCollection _appSettings, string destination)
        {
            XmlDocument xmldoc = new XmlDocument();
            string appConfigFullPathFilename = destination;
            xmldoc.Load(appConfigFullPathFilename);
            XmlNodeList xmlnode = xmldoc.SelectNodes("/configuration/appSettings/add");
            string key = string.Empty;
            string vals = string.Empty;
            string binvals = string.Empty; //user configuration is overwritten with bin config value.
            for (int i = 0; i <= xmlnode.Count - 1; i++)
            {
                
                key = xmlnode[i].Attributes["key"].Value;
                if(key.Equals("version"))
                {
                    continue;
                }

                vals = _appSettings.Get(key);

                binvals = xmlnode[i].Attributes["value"].Value;
                if (binvals != null && binvals.Contains("BSkyOverwrite"))
                {
                    vals = binvals.Replace("BSkyOverwrite", "").Trim();
                }

                xmlnode[i].Attributes["value"].Value = vals;
            }

            xmldoc.Save(appConfigFullPathFilename);

        }

        private void ChangeConfigForRHome()
        {
            IConfigService confService = container.Resolve<IConfigService>();
            string rhome = confService.GetConfigValueForKey("rhome");
            RHomeConfigWindow rhmconfwin = new RHomeConfigWindow();
            //rhmconfwin.Owner = 
            rhmconfwin.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            rhmconfwin.RHomeText.Text = rhome;

            rhmconfwin.ShowDialog();
            string newRHome = rhmconfwin.RHomeText.Text;
            confService.ModifyConfig("rhome", newRHome);
        }

#region Exit/Close related
        // App-win closed. Now close invisible owner(parent).
        void window_Closed(object sender, EventArgs e)//28Jan2013
        {
            MainWindow mwindow = LifetimeService.Instance.Container.Resolve<MainWindow>();
            mwindow.Close();//close invisible parent and all child windows(app-window, out-win, syntax-win) should go.
        }

        void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            ILoggerService logService = LifetimeService.Instance.Container.Resolve<ILoggerService>();
            logService.WriteToLogLevel("Unhandled Exception Occured :", LogLevelEnum.Fatal, e.ExceptionObject as Exception);
            MessageBox.Show("Unhandled:" + e.ExceptionObject.ToString(), "Fatal Error", MessageBoxButton.OK, MessageBoxImage.Error);
            //LifetimeService.Instance.Container.Dispose();
            //logService.WriteToLogLevel("Lifetime Service Disposed!", LogLevelEnum.Info);
            GracefulExit();
            Environment.Exit(0);//We can remove this and can try to recover APP and keep it running.
        }

        static void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            //ILoggerService logService = LifetimeService.Instance.Container.Resolve<ILoggerService>();
            //LifetimeService.Instance.Container.Dispose();
            //logService.WriteToLogLevel("Lifetime Service Disposed!", LogLevelEnum.Info);
        }

        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            ILoggerService logService = LifetimeService.Instance.Container.Resolve<ILoggerService>();
            logService.WriteToLogLevel("XAML Application Dispatcher Unhandled Exception Occured :", LogLevelEnum.Fatal, e.Exception);
            MessageBox.Show("XAML Application Dispatcher:" + e.Exception.ToString());
            e.Handled = true;/// if you false this and comment following line, then, exception goes to Unhandeled
            GracefulExit();
            Environment.Exit(0);//We can remove this and can try to recover APP and keep it running.
        }

        private void Dispatcher_UnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            ILoggerService logService = LifetimeService.Instance.Container.Resolve<ILoggerService>();
            logService.WriteToLogLevel("Dispatcher Unhandled Exception Occured :", LogLevelEnum.Fatal, e.Exception);
            MessageBox.Show("Dispatcher:" + e.Exception.ToString(), "Fatal Error", MessageBoxButton.OK, MessageBoxImage.Error);
            e.Handled = true;/// if you false this and comment following line, then, exception goes to App's main window
            GracefulExit();
            Environment.Exit(0); //We can remove this and can try to recover APP and keep it running.
        }

        //19Feb2013 For closing mainwindow which should close child and each child should ask for "save". This is not working this way.
        private void GracefulExit()
        {
            System.Windows.Input.Mouse.OverrideCursor = null;//make mouse free

            Window1 mwindow = LifetimeService.Instance.Container.Resolve<Window1>();
            mwindow.IsException = true;
            mwindow.Close();
        }

#endregion

#region Progressbar
        SplashWindow sw = null;
        Cursor defaultcursor;
        //Shows Progressbar
        private void ShowProgressbar()
        {
            sw = new SplashWindow("Please wait. Loading BSky Environment...");
            sw.Owner = (Application.Current.MainWindow);
            //bw.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            //bw.Visibility = Visibility.Visible;
            sw.Show();
            sw.Activate();
            defaultcursor = Mouse.OverrideCursor;
            Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
        }
        //Hides Progressbar
        private void HideProgressbar()
        {
            if (sw != null)
            {
                sw.Close(); // close window if it exists
                //sw.Visibility = Visibility.Hidden;
                //sw = null;
            }
            Mouse.OverrideCursor = defaultcursor;
        }

#endregion

#region Mouse Busy
        //Cursor defaultcursor;
        //Shows busy mouse
        private void ShowMouseBusy()
        {
            defaultcursor = Mouse.OverrideCursor;
            Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
        }
        //Hides busy mouse
        private void HideMouseBusy()
        {
            Mouse.OverrideCursor = defaultcursor;
        }
#endregion

#region Citrix Server
        private bool IsCitrixServer()
        {
            bool isCitrix = false;
            XmlDocument xmldoc = new XmlDocument();

            if (File.Exists("CSettings.xml"))
            {
                xmldoc.Load("CSettings.xml");

                XmlNodeList xnlist = xmldoc.SelectNodes("configuration/appSettings/add[@key='BSkyCServer']");

                if (xnlist.Count > 0)
                {
                    string boolstr = xnlist[0].Attributes[1].Value;

                    if (boolstr.Equals("true"))
                        isCitrix = true;
                }
            }
            return isCitrix;
        }
#endregion
    }


}
