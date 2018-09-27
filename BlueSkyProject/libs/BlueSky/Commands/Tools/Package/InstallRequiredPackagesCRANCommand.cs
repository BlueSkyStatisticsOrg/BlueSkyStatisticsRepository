using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BlueSky.CommandBase;
using BSky.Lifetime.Interfaces;
using BSky.Lifetime;
using BlueSky.Commands.Tools.Package.Dialogs;
using System.Windows;
using System.IO;
using BSky.Statistics.Common;
using System.Windows.Input;
namespace BlueSky.Commands.Tools.Package
{
    //Installs required packages from CRAN. XML file is maintains a list of packages
    class InstallRequiredPackagesCRANCommand : BSkyCommandBase
    {
                ILoggerService logService = LifetimeService.Instance.Container.Resolve<ILoggerService>();//17Dec2012
        protected override void OnPreExecute(object param)
        {

        }
        public const String FileNameFilter = "Package (*.zip)|*.zip";
        protected override void OnExecute(object param)
        {
            Window1 appwindow = LifetimeService.Instance.Container.Resolve<Window1>();
            try
            {
                appwindow.setLMsgInStatusBar(BSky.GlobalResources.Properties.Resources.PlzWait+" "+BSky.GlobalResources.Properties.Resources.InstallReqRPkgFrmCRAN2);
                ShowMouseBusy();
                //Get list of required pacakges from RequiredPackages.xml
                List<string> reqPkgList = GetReqRPackageList();
                PackageHelperMethods phm = new PackageHelperMethods();
                UAReturn r = null;
                foreach (string pkgname in reqPkgList)
                {
                    r = phm.InstallReqPackageFrmCRAN(pkgname);
                    if (r != null)
                    {
                        // It is not error message. It could be success/failure msg. A status message basically.
                        SendToOutputWindow(BSky.GlobalResources.Properties.Resources.RPkgInstallStatus, r.Error, false);
                    }

                }
                ShowMouseFree();
            }
            catch (Exception ex)
            {
                ShowMouseFree();
                MessageBox.Show(BSky.GlobalResources.Properties.Resources.ErrInstallReqPkgFrmCRAN, BSky.GlobalResources.Properties.Resources.ErrorOccurred);
                logService.WriteToLogLevel("Error:", LogLevelEnum.Error, ex);
            }
            appwindow.setLMsgInStatusBar("");
        }

        protected override void OnPostExecute(object param)
        {

        }


        #region Mouse Busy/Free
        Cursor defaultcursor;
        private void ShowMouseBusy()
        {
            defaultcursor = Mouse.OverrideCursor;
            Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
        }

        private void ShowMouseFree()
        {
            Mouse.OverrideCursor = null;
        }

        #endregion

        #region Get The required R pacakge list
        private List<string> GetReqRPackageList()
        {
            //read RequiredPackage.xml from Config and prepare a list
            XMLitemsProcessor requiredpackages = new XMLitemsProcessor();
            requiredpackages.MaxRecentItems = 1000;
            CopyIfNotExists(string.Format(@"{0}RequiredPackages.xml", BSkyAppData.BSkyAppDirConfigPath), string.Format(@"{0}ModelClasses.xml", BSkyAppData.RoamingUserBSkyConfigPath));
            requiredpackages.XMLFilename = string.Format(@"{0}RequiredPackages.xml", BSkyAppData.RoamingUserBSkyConfigPath);
            requiredpackages.RefreshXMLItems();
            List<string> rpkglist = requiredpackages.RecentFileList;

            return rpkglist;
        }

        //Exact same function inf App.xaml.cs
        //This code is suppose to run first time only. Once files are copied
        //copy xml files those have prepopulated data to Roaming( ModelClasses, DefaultPackages, RequiredPackages)
        private void CopyIfNotExists(string source, string destination)
        {
            //ILoggerService logService = container.Resolve<ILoggerService>();
            try
            {
                if (!System.IO.File.Exists(destination)) //if file does not exists. Copy it in destination
                {
                    System.IO.File.Copy(source, destination);
                }
                else //File alreasy exists. We will only check the size difference coz our files are not so critical ones.
                {
                    FileInfo sf = new FileInfo(source);
                    long slen = sf.Length;

                    FileInfo df = new FileInfo(destination);
                    long dlen = df.Length;

                    if (slen != dlen)
                    {
                        //make a back of current file in destination (with milisecs couting from 1970-1-1)
                        //it is imp to back up current file in destination. During backup the file with 
                        //same name(other old backup) must not exist otherwise whole process will fail.
                        //So we are adding millisecs to backup filename to make it unique, which ensures
                        //that this logic should not fail.
                        TimeSpan milsec = DateTime.Now - new DateTime(1970, 1, 1);
                        System.IO.File.Move(destination, destination + milsec.TotalMilliseconds.ToString() + ".bak");

                        System.IO.File.Copy(source, destination);
                    }
                }

            }
            catch (Exception ex)
            {
                logService.WriteToLogLevel("Error copying " + Path.GetFileName(source) + " to Roaming", LogLevelEnum.Error);
                logService.WriteToLogLevel("ERROR: " + ex.Message, LogLevelEnum.Error);
            }
        }

        #endregion
    }
}
