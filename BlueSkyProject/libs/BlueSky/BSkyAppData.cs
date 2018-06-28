using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using System.Globalization;

namespace BlueSky
{
    static class BSkyAppData
    {
        //ILoggerService logService = LifetimeService.Instance.Container.Resolve<ILoggerService>();

        private static string BSkyFolder = "BlueSky Statistics"; //Not in Use
        private static string BSkyConfig = "Config";
        private static string BSkyTemp = "Temp";
        private static string BSkyLog = "Log";
        private static string BSkyJournal = "Journal";

        //LocalApplicationData folder -> C:\\Users\\AD\\AppData\\Local
        //ApplicationData folder -> C:\\Users\\AD\\AppData\\Roaming
        //CommonApplicationData folder -> C:\\ProgramData
        //MyDocuments folder D:\\Work\\MyDocuments

        //#region Root Folder of BlueSky application in MyDocuments for Data
        //// returns BSky data root path (MyDocuments/BlueSky Statistics/) having forward slashes (Unix styl e)
        //public static string BSkyDataDirRootFwdSlash_not_inuse
        //{
        //    get { return BSkyRoamingUserPath; }
        //}

        //// returns BSky data root path (MyDocuments\BlueSky Statistics\)  having back slashes (DOS style)
        //public static string BSkyDataDirRootBkSlash_not_inuse
        //{
        //    get
        //    {
        //        //return BSkyMyDocuments.Replace(@"/", @"\");
        //        string rootpath = BSkyRoamingUserPath.Replace(@"/", @"\");
        //        bool b = CreatePathIfNotExists(rootpath);
        //        return rootpath;

        //    }
        //}
        //#endregion



        #region Application 'Config' path(that contains XAML/XML)

        public static string BSkyAppDirConfigPath //BSkyAppDirConfigFwdSlash
        {
            get
            {
                string CultureName = Thread.CurrentThread.CurrentCulture.Name;
                if (!CultureName.Equals("en-US") && !CultureName.Equals("ko-KR"))
                {
                    Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");//US English
                    Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-US");
                    CultureName = Thread.CurrentThread.CurrentCulture.Name;
                }
                //return string.Format(@"{0}{1}/", BSkyDataDirRootFwdSlash, BSkyConfig);
                //string appconfpath = string.Format(@"{0}{1}/", "./", BSkyConfig);
                string appconfpath = string.Format(@"{0}{1}/{2}/", "./", BSkyConfig,CultureName);
                bool b = CreatePathIfNotExists(appconfpath);
                return appconfpath;
            }
        }

        //// returns BSky 'Config' path (MyDocuments/BlueSky Statistics/Config/)  having forward slashes (Unix style)
        //public static string BSkyDataDirConfigFwdSlash_not_inUse
        //{
        //    get
        //    {
        //        //return string.Format(@"{0}{1}/", BSkyDataDirRootFwdSlash, BSkyConfig);
        //        string confpath = string.Format(@"{0}{1}/", RoamingUserBSkyPath, BSkyConfig);
        //        bool b = CreatePathIfNotExists(confpath);
        //        return confpath;
        //    }
        //}

        //// returns BSky 'Config' path (MyDocuments\BlueSky Statistics\Config\) having back slashes (DOS style)
        //public static string BSkyDataDirConfigBkSlash_not_inUse
        //{
        //    get
        //    {
        //        //return string.Format(@"{0}{1}\", BSkyDataDirRootBkSlash, BSkyConfig);
        //        string confpath = string.Format(@"{0}{1}\", RoamingUserBSkyPath, BSkyConfig);
        //        bool b = CreatePathIfNotExists(confpath);
        //        return confpath;
        //    }
        //}

        #endregion

        #region User's Roaming Location for BlueSky ie.. "C:\Users\SharpShooter\AppData\Roaming"
        private static string RoamingUserPath
        {
            get
            {
                //string MyDocs = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                //string MyDocsFwdSlashPath = MyDocs.Replace(@"\", @"/");
                //string path = string.Format(@"{0}/{1}/", MyDocsFwdSlashPath, BSkyFolder);
                // if (PathExists(path)) // if this path does not exist, means BSky app is not installed & is running from V-Studio
                //    return path;
                //else
                //return "./"; // or return current location ie from where the exe launched. Will be used when Bsky executed from VS

                string UserPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                return UserPath;
            }
        }
        #endregion

        #region User 'BlueSky' path ie.. "C:\Users\Username\AppData\Roaming\BlueSky\"
        private static string RoamingUserBSkyPath //BSkyMyDocuments <- old name
        {
            get
            {
                string UserBSkyPath = Path.Combine(RoamingUserPath, @"BlueSky\");
                bool b = CreatePathIfNotExists(UserBSkyPath);
                return UserBSkyPath;
            }
        }
        #endregion

        #region User 'Config' path ie.. "C:\Users\Username\AppData\Roaming\BlueSky\Config\"
        public static string RoamingUserBSkyConfigPath //BSkyDataDirLogFwdSlash
        {
            get
            {
                //return string.Format(@"{0}{1}/", BSkyDataDirRootFwdSlash, BSkyConfig);
                string confpath = string.Format(@"{0}{1}\", RoamingUserBSkyPath, BSkyConfig);
                bool b = CreatePathIfNotExists(confpath);
                return confpath;
            }
        }
        #endregion

        #region User 'Config\l18n\' path ie.. "C:\Users\Username\AppData\Roaming\BlueSky\Config\<lang>\"
        public static string RoamingUserBSkyConfigL18nPath //BSkyDataDirLogFwdSlash
        {
            get
            {
                string CultureName = Thread.CurrentThread.CurrentCulture.Name;
                if (!CultureName.Equals("en-US") && !CultureName.Equals("ko-KR"))
                {
                    Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");//US English
                    Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-US");
                    CultureName = Thread.CurrentThread.CurrentCulture.Name;
                }
                //return string.Format(@"{0}{1}/{2}/", RoamingUserBSkyPath, BSkyConfig, CultureName);
                string confL18npath = string.Format(@"{0}{1}\{2}\", RoamingUserBSkyPath, BSkyConfig, CultureName);
                bool b = CreatePathIfNotExists(confL18npath);
                return confL18npath;
            }
        }
        #endregion

        #region User 'Temp' path  ie.. "C:\Users\Username\AppData\Roaming\BlueSky\Temp\"
        public static string RoamingUserBSkyTempPath //BSkyTempDirFwdSlash
        {
            get
            {
                //return string.Format(@"{0}{1}/", BSkyDataDirRootFwdSlash, BSkyConfig);
                string temppath = string.Format(@"{0}{1}\", RoamingUserBSkyPath, BSkyTemp).Replace(@"\", @"/"); //uniz style path
                bool b = CreatePathIfNotExists(temppath);
                return temppath;
            }
        }
        #endregion

        #region User 'Log' path ie.. "C:\Users\Username\AppData\Roaming\BlueSky\Log\"
        public static string RoamingUserBSkyLogPath //BSkyDataDirLogFwdSlash
        {
            get
            {
                //return string.Format(@"{0}{1}/", BSkyDataDirRootFwdSlash, BSkyConfig);
                string logpath = string.Format(@"{0}{1}\", RoamingUserBSkyPath, BSkyLog);
                bool b = CreatePathIfNotExists(logpath);
                return logpath;
            }
        }
        #endregion

        #region User 'Journal' path ie.. "C:\Users\Username\AppData\Roaming\BlueSky\Journal\"
        public static string RoamingUserBSkyJournalPath
        {
            get
            {
                //return string.Format(@"{0}{1}/", BSkyDataDirRootFwdSlash, BSkyJournal);
                string journalpath = string.Format(@"{0}{1}\", RoamingUserBSkyPath, BSkyJournal);
                bool b = CreatePathIfNotExists(journalpath);
                return journalpath;
            }
        }
        #endregion

        #region Other util methods

        //Checks if path exists or not. Tries to create the path. If exists or created successfully, return true else returns false.
        private static bool PathExists(string dirpath)
        {
            bool locationExists = true;
            try
            {
                if (!Directory.Exists(dirpath)) // if directory does not exist, create it
                {
                    //dont want to create this path as BlueSky setup should do this instead.
                    //Directory.CreateDirectory(dirpath);
                    locationExists = false;
                }

            }
            catch (Exception ex)
            {

            }
            return (locationExists);
        }


        //Check if path does not exist then create it. This is app user local path(user profile)
        private static bool CreatePathIfNotExists(string dirpath)
        {
            bool locationExists = true;
            try
            {
                if (!Directory.Exists(dirpath)) // if directory does not exist, create it
                {
                    Directory.CreateDirectory(dirpath);
                    locationExists = false;
                }

            }
            catch (Exception ex)
            {

            }
            return (locationExists);
        }

        #endregion

    }

}
