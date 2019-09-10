using System;
using BlueSky.CommandBase;
using BSky.Lifetime.Interfaces;
using BSky.Lifetime;
using BlueSky.Windows;
using Microsoft.Practices.Unity;
using BSky.Statistics.Common;
using System.Windows;
using BSky.RecentFileHandler;
using BSky.Interfaces.Interfaces;
using BSky.ConfService.Intf.Interfaces;
using BlueSky.Services;

namespace BlueSky.Commands.File
{
    class FileNewCommand : BSkyCommandBase
    {
        protected override void OnPreExecute(object param)
        {
        }

        public const String FileNameFilter = "IBM SPSS (*.sav)|*.sav| Excel 2003 (*.xls)|*.xls|Excel 2007-2010 (*.xlsx)|*.xlsx|Comma Seperated (*.csv)|*.csv|DBF (*.dbf)|*.dbf|R Obj (*.RData)|*.RData";
        IConfigService confService = LifetimeService.Instance.Container.Resolve<IConfigService>();//12Dec2013
        RecentDocs recentfiles = LifetimeService.Instance.Container.Resolve<RecentDocs>();//21Dec2013
        bool UseFlexSheetForNewDataframe = false;

        protected override void OnExecute(object param)
        {
            //// Get initial Dir 12Feb2013 ////
            string initDir = confService.GetConfigValueForKey("InitialDirectory");
            if (UseFlexSheetForNewDataframe)
            {
                initGlobalObjects();
                int rowsize = 70, colsize = 30;
                NewDataframeWindow newDF = new NewDataframeWindow() { RowSize = rowsize, ColSize = colsize };
                if (appwindow != null) newDF.Owner = appwindow;
                newDF.ShowDialog();
                string dfname = newDF.DFName;
                bool loadInGrid = newDF.LoadInGrid;
                if (!string.IsNullOrEmpty(dfname))
                {
                    PasteClipboardDataset pasteds = new PasteClipboardDataset();
                    pasteds.PasteDatasetFromClipboard(dfname, loadInGrid);
                }
            }
            else//use C1Datagrid that we originally had
            {
                NewFileOpen("");
            }
        }



        protected override void OnPostExecute(object param)
        {
        }

        IUnityContainer container = null;
        IDataService service = null;
        IUIController controller = null;
        Window1 appwindow = null;
        private void initGlobalObjects()
        {
            container = LifetimeService.Instance.Container;
            service = container.Resolve<IDataService>();
            controller = container.Resolve<IUIController>();
            appwindow = LifetimeService.Instance.Container.Resolve<Window1>();//for refeshing recent files list

        }

        public void NewFileOpen(string filename)//21Feb 2013 For opening Dataset from File > Open & File > Recent
        {
            //if (filename != null && filename.Length > 0)
            {
                //IUnityContainer container = LifetimeService.Instance.Container;
                //IDataService service = container.Resolve<IDataService>();
                //IUIController controller = container.Resolve<IUIController>();
                //Window1 appwindow = LifetimeService.Instance.Container.Resolve<Window1>();//for refeshing recent files list

                initGlobalObjects();
                if (!AreDefaultRPackagesLoaded())
                    return;

                //if (System.IO.File.Exists(filename))
                {

                    DataSource ds = service.NewDataset();//filename);
                    if (ds != null)
                    {
                        controller.LoadNewDataSet(ds);
                        //recentfiles.AddXMLItem(filename);//adding to XML file for recent docs
                    }
                    else
                    {
                        MessageBox.Show(appwindow, BSky.GlobalResources.Properties.Resources.cantopen+" " + filename +
                            ".\n"+BSky.GlobalResources.Properties.Resources.reasonsAre +
                            "\n"+BSky.GlobalResources.Properties.Resources.ReqRPkgNotInstalled +
                            "\n"+BSky.GlobalResources.Properties.Resources.FormatNotSupported+
                            //"\nOR."+
                            "\n"+BSky.GlobalResources.Properties.Resources.OldSessionRunning +
                            //"\nOR."+
                            "\n"+BSky.GlobalResources.Properties.Resources.RSideIssue);
                        SendToOutputWindow( BSky.GlobalResources.Properties.Resources.ErrOpeningDataset, filename);
                    }
                }
                //else
                //{
                //    MessageBox.Show(filename + " does not exist!", "File not found!", MessageBoxButton.OK, MessageBoxImage.Warning);
                //    //If file does not exist. It should be removed from the recent files list.
                //    recentfiles.RemoveXMLItem(filename);
                //}
                //18OCt2013 move up for using in msg box   Window1 appwindow = LifetimeService.Instance.Container.Resolve<Window1>();//for refeshing recent files list
                appwindow.RefreshRecent();
            }
            //08Apr2015 bring main window in front after file open, instead of output window
            Window1 window = LifetimeService.Instance.Container.Resolve<Window1>();
            window.Activate();
        }

        //Check for default pacakges those are required for opening datasets
        private bool AreDefaultRPackagesLoaded()
        {
            if (Window1.DatasetReqRPackagesLoaded)//Check global if TRUE no need to get results from R
            {
                return true;
            }

            bool alldefaultloaded = true;
            UAReturn retn = service.getMissingDefaultRPackages();
            string missinglist = (string)retn.SimpleTypeData;
            if (missinglist != null && missinglist.Length > 0)
            {
                alldefaultloaded = false;
            }
            if (!alldefaultloaded)
            {
                string firstmsg = BSky.GlobalResources.Properties.Resources.BSkyNeedsRPkgs+"\n\n";
                string msg = "\n\n" + BSky.GlobalResources.Properties.Resources.InstallReqRPkgFrmCRAN + "\n"+BSky.GlobalResources.Properties.Resources.InstallUpdatePkgMenuPath;
                MessageBox.Show(firstmsg + missinglist + msg, BSky.GlobalResources.Properties.Resources.ErrReqRPkgMissing, MessageBoxButton.OK, MessageBoxImage.Error);
            }

            //set global flag. if TRUE is set next line will not execute next time.
            // as the 'if' in the beginning will return the control
            Window1.DatasetReqRPackagesLoaded = alldefaultloaded;
            Window1.DatasetReqPackages = missinglist;

            return alldefaultloaded;
        }
   
    }
}
