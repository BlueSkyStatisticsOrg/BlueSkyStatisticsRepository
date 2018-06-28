using System;

using System.Windows;
using BSky.Lifetime;
using BSky.Lifetime.Interfaces;
using BSky.Statistics.Common;
using BlueSky.CommandBase;
using BlueSky.Commands.Tools.Package.Dialogs;
using System.Collections.Generic;

namespace BlueSky.Commands.Tools.Package
{
    class UninstallPackageCommand : BSkyCommandBase
    {
        ILoggerService logService = LifetimeService.Instance.Container.Resolve<ILoggerService>();//17Dec2012
        protected override void OnPreExecute(object param)
        {

        }

        protected override void OnExecute(object param)
        {
            try
            {
                //string packagename = Microsoft.VisualBasic.Interaction.InputBox("Enter package name that you want to uninstall.", "Load Library", "");
                //if (string.IsNullOrEmpty(packagename))
                //{
                //    //MessageBox.Show("Title/Command cannot be empty, Exiting Dialog install", "Info: Dialog Title Empty.");
                //    return;
                //}

                PackageHelperMethods phm = new PackageHelperMethods();
                UAReturn rlst = phm.ShowInstalledPackages();
                string[] strarr = phm.GetUAReturnStringResult(rlst);


                //Create UI show list of installed packges so that user can select and load them
                SelectPackagesWindow spw = new SelectPackagesWindow(strarr);
                spw.header = "Uninstall Library(s)";
                spw.ShowDialog();
                IList<string> sel = spw.SelectedItems;
                if (sel == null)
                    return;

                string[] selectedpackages = new string[sel.Count];
                int i = 0;
                foreach (string s in sel)
                {
                    selectedpackages[i] = s;
                    i++;
                }

                phm = new PackageHelperMethods();
                UAReturn r = phm.PackageFileUninstall(selectedpackages);// PackageFileUninstall(packagename);
                if (r != null && r.Success)
                {
                    SendToOutputWindow(BSky.GlobalResources.Properties.Resources.UninstallPkg, r.CommandString);
                }
                else
                {
                    SendToOutputWindow(BSky.GlobalResources.Properties.Resources.ErrUninstallingPkg, "");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(BSky.GlobalResources.Properties.Resources.ErrUninstallingPkg, BSky.GlobalResources.Properties.Resources.ErrorOccurred);
                logService.WriteToLogLevel("Error:", LogLevelEnum.Error, ex);
            }
        }

        protected override void OnPostExecute(object param)
        {

        }

        //public UAReturn PackageFileUninstall(string filename)//06Dec2013 For uninstalling package
        //{
        //    if (filename != null && filename.Length > 0)
        //    {
        //        IUnityContainer container = LifetimeService.Instance.Container;
        //        IDataService service = container.Resolve<IDataService>();

        //            string filepath = Path.GetDirectoryName(filename);
        //            string packagename = Path.GetFileName(filename);
        //            return service.uninstallPackage(packagename);

        //        //appwindow.RefreshRecent();
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
