using System;
using System.Windows;
using BSky.Lifetime;
using BSky.Lifetime.Interfaces;
using BlueSky.Windows;
using BlueSky.CommandBase;
using System.Windows.Input;
using BSky.RecentFileHandler;
using BlueSky.Commands.Analytics.TTest;
using BSky.ConfService.Intf.Interfaces;

namespace BlueSky.Commands.Data
{
    class RefreshDatagridCommand : BSkyCommandBase
    {

        public const String FileNameFilter = "IBM SPSS (*.sav)|*.sav| Excel 2003 (*.xls)|*.xls|Excel 2007-2010 (*.xlsx)|*.xlsx|Comma Seperated (*.csv)|*.csv|DBF (*.dbf)|*.dbf|R Obj (*.RData)|*.RData";
        IConfigService confService = LifetimeService.Instance.Container.Resolve<IConfigService>();//12Dec2013
        ILoggerService logService = LifetimeService.Instance.Container.Resolve<ILoggerService>();//17Dec2012
        //RecentDocs recentfiles = LifetimeService.Instance.Container.Resolve<RecentDocs>();//21Dec2013
        //DefaultPackages defaultpackges = LifetimeService.Instance.Container.Resolve<DefaultPackages>();//06Nov2014

        DatasetLoadingBusyWindow bw = null;
        protected override void OnPreExecute(object param)
        {
            //throw new NotImplementedException();
        }

        protected override void OnExecute(object param)
        {
            AUAnalysisCommandBase auacb = new AUAnalysisCommandBase();
            auacb.RefreshGrids();
        }

        protected override void OnPostExecute(object param)
        {
            //throw new NotImplementedException();
        }

        #region Progressbar .. Can be used in future
        Cursor defaultcursor;
        //Shows Progressbar
        private void ShowProgressbar()
        {
            bw = new DatasetLoadingBusyWindow(BSky.GlobalResources.Properties.Resources.PlzWaitDatasetLoading);
            bw.Owner = (Application.Current.MainWindow);
            //bw.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            //bw.Visibility = Visibility.Visible;
            bw.Show();
            bw.Activate();
            defaultcursor = Mouse.OverrideCursor;
            Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
        }
        //Hides Progressbar
        private void HideProgressbar()
        {
            if (bw != null)
            {
                bw.Close(); // close window if it exists
                //bw.Visibility = Visibility.Hidden;
                //bw = null;
            }
            Mouse.OverrideCursor = defaultcursor;
        }

        //in App Main Window stausbar
        //Shows Progressbar in statusbar
        private void ShowStatusProgressbar()
        {
            Window1 window = LifetimeService.Instance.Container.Resolve<Window1>();//27Oct2014
            //window.ProgressStatusPanel.Visibility = Visibility.Visible;
            defaultcursor = Mouse.OverrideCursor;
            Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
        }
        //Hides Progressbar in statusbar
        private void HideStatusProgressbar()
        {
            Window1 window = LifetimeService.Instance.Container.Resolve<Window1>();//27Oct2014
            //window.ProgressStatusPanel.Visibility = Visibility.Hidden;
            Mouse.OverrideCursor = defaultcursor;
        }
        #endregion

    }
}
