using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using BSkyStyler;

namespace BSky.Controls
{
    static class BSkyAppDir
    {
        private static string BSkyFolder = "BlueSky Statistics"; //Not in Use
        private static string BSkyConfig = "Config";
        private static string BSkyTemp = "Temp";
        private static string BSkyLog = "Log";
        private static string BSkyJournal = "Journal";

        //LocalApplicationData folder -> C:\\Users\\AD\\AppData\\Local
        //ApplicationData folder -> C:\\Users\\AD\\AppData\\Roaming
        //CommonApplicationData folder -> C:\\ProgramData
        //MyDocuments folder D:\\Work\\MyDocuments

        #region Application 'Config' path(that contains XAML/XML)

        public static string BSkyAppDirConfigPath //BSkyAppDirConfigFwdSlash
        {
            get
            {
                //Added by Aaron 07/31/2020
                //Comented line below
                //string CultureName = Thread.CurrentThread.CurrentCulture.Name;
                //Added line below
                string CultureName = "en-US";
                //return string.Format(@"{0}{1}/", BSkyDataDirRootFwdSlash, BSkyConfig);
                //string appconfpath = string.Format(@"{0}{1}/", "./", BSkyConfig);
                string appconfpath = string.Format(@"{0}{1}/{2}/", "./", BSkyConfig, CultureName);
                //bool b = CreatePathIfNotExists(appconfpath);
                return appconfpath;
            }
        }
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

        #region User 'Config' path  ie.. "C:\Users\Username\AppData\Roaming\BlueSky\Config\"
        public static string RoamingUserBSkyConfigPath //BSkyTempDirFwdSlash
        {
            get
            {
                //return string.Format(@"{0}{1}/", BSkyDataDirRootFwdSlash, BSkyConfig);
                string temppath = string.Format(@"{0}{1}\", RoamingUserBSkyPath, BSkyConfig).Replace(@"\", @"/"); //uniz style path
                bool b = CreatePathIfNotExists(temppath);
                return temppath;
            }
        }
        #endregion


        #region User 'Config\L18n' path  ie.. "C:\Users\Username\AppData\Roaming\BlueSky\Config\<lang>"
        public static string RoamingUserBSkyConfigL18nPath //BSkyTempDirFwdSlash
        {
            get
            {
                //Added by Aaron 07/31/2020
                //Comented line below
                //string CultureName = Thread.CurrentThread.CurrentCulture.Name;
                //Added line below
                string CultureName = "en-US";

                //return string.Format(@"{0}{1}/", BSkyDataDirRootFwdSlash, BSkyConfig);
                string temppath = string.Format(@"{0}{1}\{2}\", RoamingUserBSkyPath, BSkyConfig, CultureName).Replace(@"\", @"/"); //uniz style path
                bool b = CreatePathIfNotExists(temppath);
                return temppath;
            }
        }
        #endregion

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

    }
}
