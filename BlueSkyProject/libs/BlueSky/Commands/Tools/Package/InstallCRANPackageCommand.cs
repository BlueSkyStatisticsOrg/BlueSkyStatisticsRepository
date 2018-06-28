using System;
using System.Windows;
using BSky.Lifetime;
using BSky.Lifetime.Interfaces;
using BSky.Statistics.Common;
using BlueSky.CommandBase;
using System.Windows.Input;


namespace BlueSky.Commands.Tools.Package
{
    class InstallCRANPackageCommand : BSkyCommandBase
    {
        ILoggerService logService = LifetimeService.Instance.Container.Resolve<ILoggerService>();//17Dec2012
        protected override void OnPreExecute(object param)
        {

        }

        protected override void OnExecute(object param)
        {
            Window1 appwindow = LifetimeService.Instance.Container.Resolve<Window1>();
            try
            {
                appwindow.setLMsgInStatusBar(BSky.GlobalResources.Properties.Resources.PlzWait+" "+BSky.GlobalResources.Properties.Resources.InstallingFromCRAN);
                PackageHelperMethods phm = new PackageHelperMethods();
                UAReturn r = phm.InstallCRANPackage();// InstallCRANPackage();

                if (r != null)
                {
                    if (r.Success)
                    {
                        SendToOutputWindow(BSky.GlobalResources.Properties.Resources.InstallingFromCRAN, r.CommandString);
                    }
                    else if (!r.Success)
                    {
                        SendToOutputWindow(BSky.GlobalResources.Properties.Resources.InstallingFromCRAN, r.Error);
                    }
                }
                else
                {
                    if(r != null) // if user didn't press 'Cancel'
                    SendToOutputWindow(BSky.GlobalResources.Properties.Resources.ErrInstallingFromCRAN, BSky.GlobalResources.Properties.Resources.RPkgNameCaseSensitive, false);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(BSky.GlobalResources.Properties.Resources.ErrLoadingRPkg2, BSky.GlobalResources.Properties.Resources.ErrorOccurred);
                logService.WriteToLogLevel("Error:", LogLevelEnum.Error, ex);
            }
            appwindow.setLMsgInStatusBar("");
        }

        protected override void OnPostExecute(object param)
        {

        }

        //public UAReturn InstallCRANPackage()//06Dec2013 For loading package in R memory for use
        //{
        //        IUnityContainer container = LifetimeService.Instance.Container;
        //        IDataService service = container.Resolve<IDataService>();
        //        AskPackageNameWindow apn = new AskPackageNameWindow();
        //        apn.Title = "Install Package From CRAN";
        //        apn.ShowDialog();
        //        string pkgname = apn.PackageName;
        //        if (pkgname.Length < 1)
        //            return null;

        //        return service.installCRANPackage(pkgname);
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


        #region Progressbar
        Cursor defaultcursor;
        //Shows Mouse Busy
        private void ShowMouseBusy()
        {
            defaultcursor = Mouse.OverrideCursor;
            Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
        }
        //Hides Busy mouse and changes to normal mouse
        private void ShowMouseFree()
        {
            Mouse.OverrideCursor = defaultcursor;
        }

        #endregion

    }
}
