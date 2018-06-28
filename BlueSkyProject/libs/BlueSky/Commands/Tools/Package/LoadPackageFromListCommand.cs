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
    class LoadPackageFromListCommand : BSkyCommandBase
    {
        ILoggerService logService = LifetimeService.Instance.Container.Resolve<ILoggerService>();//17Dec2012
        protected override void OnPreExecute(object param)
        {

        }

        protected override void OnExecute(object param)
        {
            try
            {
                PackageHelperMethods phm = new PackageHelperMethods();
                UAReturn rlst = phm.ShowInstalledPackages();
                string[] installedpkgs = phm.GetUAReturnStringResult(rlst);

                UAReturn rlst2 = phm.ShowLoadedPackages();
                string[] loadededpkgs = phm.GetUAReturnStringResult(rlst2);
                string[] strarr = phm.GetStringArrayUncommon(installedpkgs, loadededpkgs);

                //Create UI show list of installed packges so that user can select and load them
                SelectPackagesWindow spw = new SelectPackagesWindow(strarr);
                spw.header = "Load Library(s)";
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


                // Pass List of selected packages to load them
                UAReturn r = phm.LoadPackageFromList(selectedpackages);// LoadPackageFromList();
                if (r != null && r.Success)
                {
                    SendToOutputWindow( BSky.GlobalResources.Properties.Resources.LoadPackages, r.CommandString);
                }
                else
                {
                    if (r.CommandString != null && r.CommandString.Trim().Length < 1)
                    { }
                    else if (r.CommandString != null && r.CommandString.Trim().Length > 0)
                        SendToOutputWindow(BSky.GlobalResources.Properties.Resources.ErrLoadingRPkgs, r.CommandString);
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(BSky.GlobalResources.Properties.Resources.ErrLoadingRPkg2, BSky.GlobalResources.Properties.Resources.ErrorOccurred);
                logService.WriteToLogLevel("Error:", LogLevelEnum.Error, ex);
            }
        }

        protected override void OnPostExecute(object param)
        {

        }

        //public UAReturn LoadPackageFromList()//06Dec2013 For loading package in R memory for use
        //{
        //        IUnityContainer container = LifetimeService.Instance.Container;
        //        IDataService service = container.Resolve<IDataService>();
        //        return service.loadPackageFromList();
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
