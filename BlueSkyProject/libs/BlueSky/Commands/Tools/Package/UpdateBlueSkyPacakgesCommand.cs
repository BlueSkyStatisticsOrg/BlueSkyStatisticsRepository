using System;
using System.Windows;
using BSky.Lifetime;
using BSky.Lifetime.Interfaces;
using Microsoft.Win32;
using BSky.Statistics.Common;
using BlueSky.CommandBase;

namespace BlueSky.Commands.Tools.Package
{
    //initially it was copied from InstallPackageCommand.cs //04May2015
    class UpdateBlueSkyPacakgesCommand : BSkyCommandBase
    {
        ILoggerService logService = LifetimeService.Instance.Container.Resolve<ILoggerService>();
        protected override void OnPreExecute(object param)
        {

        }
        public const String FileNameFilter = "Package (BlueSky*.zip)|BlueSky*.zip";
        protected override void OnExecute(object param)
        {
            //Window1 appwindow = LifetimeService.Instance.Container.Resolve<Window1>();
            try
            {
                bool autoLoad = true, overwrite = true;
                OpenFileDialog openFileDialog = new OpenFileDialog();
                //// Get initial Dir ////
                //string initDir = confService.GetConfigValueForKey("InitialDirectory");
                //openFileDialog.InitialDirectory = initDir;
                openFileDialog.Filter = FileNameFilter;
                openFileDialog.Multiselect = true;
                bool? output = openFileDialog.ShowDialog(Application.Current.MainWindow);
                if (output.HasValue && output.Value)
                {
                    string[] pkgfilenames = openFileDialog.FileNames;
                    PackageHelperMethods phm = new PackageHelperMethods();
                    UAReturn r = phm.PackageFileInstall(pkgfilenames, autoLoad, overwrite);// PackageFileInstall(pkgfilenames);//openFileDialog.FileName);
                    if (r != null && r.Success)
                    {
                        SendToOutputWindow(BSky.GlobalResources.Properties.Resources.InstallPkg, r.SimpleTypeData.ToString());
                        MessageBox.Show(BSky.GlobalResources.Properties.Resources.RestartToApplyChanges2, BSky.GlobalResources.Properties.Resources.RestartBSkyApp, MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                    else
                    {
                        if (r != null)
                        {
                            string msg = r.SimpleTypeData as string;
                            SendToOutputWindow(BSky.GlobalResources.Properties.Resources.ErrInstallingRPkg, msg);
                        }
                    }
                    ///Set initial Dir.///
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

    }
}
