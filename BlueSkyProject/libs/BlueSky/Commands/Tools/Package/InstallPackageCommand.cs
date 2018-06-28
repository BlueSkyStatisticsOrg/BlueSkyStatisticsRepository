using System;
using System.Windows;
using BSky.Lifetime;
using BSky.Lifetime.Interfaces;
using Microsoft.Win32;
using BSky.Statistics.Common;
using BlueSky.CommandBase;

namespace BlueSky.Commands.Tools.Package
{
    class InstallPackageCommand : BSkyCommandBase
    {
        ILoggerService logService = LifetimeService.Instance.Container.Resolve<ILoggerService>();//17Dec2012
        protected override void OnPreExecute(object param)
        {

        }
        public const String FileNameFilter = "Package (*.zip)|*.zip";
        protected override void OnExecute(object param)
        {
            //Window1 appwindow = LifetimeService.Instance.Container.Resolve<Window1>();
            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                //// Get initial Dir 12Feb2013 ////
                //string initDir = confService.GetConfigValueForKey("InitialDirectory");
                //openFileDialog.InitialDirectory = initDir;
                openFileDialog.Filter = FileNameFilter;
                openFileDialog.Multiselect = true;
                bool? output = openFileDialog.ShowDialog(Application.Current.MainWindow);
                if (output.HasValue && output.Value)
                {
                    string[] pkgfilenames = openFileDialog.FileNames;
                    PackageHelperMethods phm = new PackageHelperMethods();
                    UAReturn r = phm.PackageFileInstall(pkgfilenames);// PackageFileInstall(pkgfilenames);//openFileDialog.FileName);
                    if (r != null && r.Success)
                    {
                        SendToOutputWindow( BSky.GlobalResources.Properties.Resources.InstallPkg, r.SimpleTypeData.ToString());
                    }
                    else
                    {
                        if(r != null)
                        {
                            string msg = r.SimpleTypeData as string;
                            SendToOutputWindow(BSky.GlobalResources.Properties.Resources.ErrInstallingRPkg, msg);
                        }
                    }
                    ///Set initial Dir. 12Feb2013///
                    //initDir = Path.GetDirectoryName(openFileDialog.FileName);
                    //confService.ModifyConfig("InitialDirectory", initDir);
                    //confService.RefreshConfig();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(BSky.GlobalResources.Properties.Resources.ErrInstallingRPkg2, BSky.GlobalResources.Properties.Resources.ErrorOccurred);
                logService.WriteToLogLevel("Error:", LogLevelEnum.Error, ex);
            }
        }

        protected override void OnPostExecute(object param)
        {

        }

        //public UAReturn PackageFileInstall(string[] pkgfilenames)//06Dec2013 For installing package
        //{
        //    if (pkgfilenames != null && pkgfilenames.Length > 0)
        //    {
        //        IUnityContainer container = LifetimeService.Instance.Container;
        //        IDataService service = container.Resolve<IDataService>();
        //        return service.installPackage(pkgfilenames);//(packagename, filepath);
        //    }
        //    return null;
        //}


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

    }
}
