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
    class UnloadPackageCommand : BSkyCommandBase
    {
       ILoggerService logService = LifetimeService.Instance.Container.Resolve<ILoggerService>();//17Dec2012
        protected override void OnPreExecute(object param)
        {

        }

        protected override void OnExecute(object param)
        {
            try
            {
                //string packagename = Microsoft.VisualBasic.Interaction.InputBox("Enter package name that you want to unload.", "Load Library", "");
                //if (string.IsNullOrEmpty(packagename))
                //{
                //    //MessageBox.Show("Title/Command cannot be empty, Exiting Dialog install", "Info: Dialog Title Empty.");
                //    return;
                //}

                PackageHelperMethods phm = new PackageHelperMethods();
                UAReturn rlst = phm.ShowLoadedPackages();
                string[] strarr = phm.GetUAReturnStringResult(rlst);

                //Create UI show list of installed packges so that user can select and load them
                SelectPackagesWindow spw = new SelectPackagesWindow(strarr);
                spw.header = "UnLoad Library(s)";
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


                //PackageHelperMethods phm = new PackageHelperMethods();
                UAReturn r = phm.PackageFileUnload(selectedpackages);// PackageFileUnload(packagename);
                if (r != null && r.Success)
                {
                    SendToOutputWindow( BSky.GlobalResources.Properties.Resources.UnloadPkg, r.CommandString, false);
                }
                else
                {
                    string error = string.Empty;
                    if(r!=null && r.Error!=null && r.Error.Length > 0)
                        error = r.Error;
                    SendToOutputWindow(BSky.GlobalResources.Properties.Resources.ErrUnloadingPkg, error, false);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(BSky.GlobalResources.Properties.Resources.ErrUnloadingPkg2, BSky.GlobalResources.Properties.Resources.ErrorOccurred);
                logService.WriteToLogLevel("Error:", LogLevelEnum.Error, ex);
            }
        }

        protected override void OnPostExecute(object param)
        {

        }

        //public UAReturn PackageFileUnload(string filename)//06Dec2013 For loading package in R memory for use
        //{
        //    if (filename != null && filename.Length > 0)
        //    {
        //        IUnityContainer container = LifetimeService.Instance.Container;
        //        IDataService service = container.Resolve<IDataService>();

        //            string filepath = Path.GetDirectoryName(filename);
        //            string packagename = Path.GetFileName(filename);
        //            return service.unloadPackage(packagename);
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
