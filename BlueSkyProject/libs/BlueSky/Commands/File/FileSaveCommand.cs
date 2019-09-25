using System;
using Microsoft.Practices.Unity;
using Microsoft.Win32;
using System.Windows;
using BSky.Statistics.Common;
using BSky.Lifetime;
using BlueSky.CommandBase;
using BSky.Interfaces.Interfaces;
using System.Windows.Input;
using BlueSky.Services;

namespace BlueSky.Commands.File
{
    class FileSaveCommand : BSkyCommandBase
    {
        protected override void OnPreExecute(object param)
        {
        }

        public const String FileNameFilter = "Excel 2007-2010 (*.xlsx)|*.xlsx" +
                                            "|Comma Seperated (*.csv)|*.csv" +
                                            "|DBF (*.dbf)|*.dbf" +
                                            "|R Object (*.RData)|*.RData";//"Excel 2003 (*.xls)|*.xls " +

        protected override void OnExecute(object param)
        {
            BSkyMouseBusyHandler.ShowMouseBusy();//ShowProgressbar_old();
            IUnityContainer container = LifetimeService.Instance.Container;
            IDataService service = container.Resolve<IDataService>();
            IUIController controller = container.Resolve<IUIController>();


            if (controller.GetActiveDocument().isUnprocessed)
            {
                NewDatasetProcessor procDS = new NewDatasetProcessor();
                procDS.ProcessNewDataset("", true);
                controller.GetActiveDocument().isUnprocessed = false;
            }

            //Get current filetype from loaded dataset. This is file extension and Filter
            DataSource actds = controller.GetActiveDocument();//06Nov2012
            if (actds == null)
                return;
            string datasetName = "" + actds.Name;//uadatasets$lst$
            //string datasetName = "uadatasets$lst$"+controller.GetActiveDocument().Name;
            //Also try to get the filename of currently loaded file. This is FileName.
            string extension = controller.GetActiveDocument().Extension.ToLower();
            string filename = controller.GetActiveDocument().FileName;
            string filter = null;
            switch (extension)
            {
                case "csv": filter = "Comma Seperated (*.csv)|*.csv"; break;
                case "xls": filter = "Excel 2003 (*.xls)|*.xls"; break;
                case "xlsx": filter = "Excel 2007-2010 (*.xlsx)|*.xlsx"; break;
                case "dbf": filter = "DBF (*.dbf)|*.dbf"; break;
                case "rdata": filter = "R Object (*.RData)|*.RData"; break;
                default: filter = "All Files(*.*)|*.*"; break;
            }

            if (extension.Equals("sav") || extension.Trim().Length==0)//show save-as dialog if current loaded file is SPSS file or memory dataframe(RData file)
            {
                SaveFileDialog saveasFileDialog = new SaveFileDialog();
                saveasFileDialog.Filter = FileNameFilter;
                Window1 appwin = LifetimeService.Instance.Container.Resolve<Window1>();
                bool? output = saveasFileDialog.ShowDialog(appwin);//Application.Current.MainWindow);
                if (output.HasValue && output.Value)
                {

                    service.SaveAs(saveasFileDialog.FileName, controller.GetActiveDocument());// #0
                    controller.GetActiveDocument().Changed = false;//21Mar2014 during close it should not prompt again for saving

                    if (saveasFileDialog.FileName.ToLower().EndsWith("sav"))//12Feb2018 we dont want to open SaveAs-ed SAV files.
                    {
                        MessageBox.Show(BSky.GlobalResources.Properties.Resources.SaveAsSucces + saveasFileDialog.FileName, BSky.GlobalResources.Properties.Resources.Saved, MessageBoxButton.OK, MessageBoxImage.Asterisk);
                    }
                    else if (System.IO.File.Exists(saveasFileDialog.FileName))
                    {
                        //Close current Dataset on whic Save As was run
                        FileCloseCommand fcc = new FileCloseCommand();
                        fcc.CloseDataset(false);

                        //Open Dataset that was SaveAs-ed. 
                        FileOpenCommand fo = new FileOpenCommand();
                        fo.FileOpen(saveasFileDialog.FileName, true);
                    }
                    else
                    {
                        BSkyMouseBusyHandler.HideMouseBusy();//HideProgressbar_old();
                        MessageBox.Show(BSky.GlobalResources.Properties.Resources.SaveAsFailed + saveasFileDialog.FileName, BSky.GlobalResources.Properties.Resources.InternalError, MessageBoxButton.OK, MessageBoxImage.Asterisk);
                    }
                }
            }
            else if (extension.Equals("xls"))//we are using write.xlsx() from openxlsx package so we can save to XLSX format only and not to XLS.
            {
                MessageBoxResult mbr = MessageBox.Show(BSky.GlobalResources.Properties.Resources.CantSaveAsExcel,
                    BSky.GlobalResources.Properties.Resources.SaveToOtherFormat, MessageBoxButton.OKCancel, MessageBoxImage.Asterisk);
                if (mbr == MessageBoxResult.OK)
                {
                    SaveFileDialog saveasFileDialog = new SaveFileDialog();
                    saveasFileDialog.Filter = FileNameFilter;
                    Window1 appwin = LifetimeService.Instance.Container.Resolve<Window1>();
                    bool? output = saveasFileDialog.ShowDialog(appwin);//Application.Current.MainWindow);
                    if (output.HasValue && output.Value)
                    {

                        service.SaveAs(saveasFileDialog.FileName, controller.GetActiveDocument());// #0
                        controller.GetActiveDocument().Changed = false;//21Mar2014 during close it should not prompt again for saving

                        if (System.IO.File.Exists(saveasFileDialog.FileName))
                        {
                            //Close current Dataset on which Save As was run
                            FileCloseCommand fcc = new FileCloseCommand();
                            fcc.CloseDataset(false);

                            //Open Dataset that was SaveAs-ed. 
                            FileOpenCommand fo = new FileOpenCommand();
                            fo.FileOpen(saveasFileDialog.FileName);
                        }
                        else
                        {
                            BSkyMouseBusyHandler.HideMouseBusy();//HideProgressbar_old();
                            MessageBox.Show(BSky.GlobalResources.Properties.Resources.SaveAsFailed + saveasFileDialog.FileName, BSky.GlobalResources.Properties.Resources.InternalError, MessageBoxButton.OK, MessageBoxImage.Asterisk);
                        }
                    }
                }
                else
                {
                    return;
                }
            }
            else
            {

                service.SaveAs(filename, controller.GetActiveDocument());// #0
                controller.GetActiveDocument().Changed = false;//21Mar2014 during close it should not prompt again for saving
            }
            BSkyMouseBusyHandler.HideMouseBusy();//HideProgressbar_old();
        }

        protected override void OnPostExecute(object param)
        {
        }

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
        //Shows Progressbar
        private void ShowProgressbar_old()
        {
            defaultcursor = Mouse.OverrideCursor;
            Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
        }
        //Hides Progressbar
        private void HideProgressbar_old()
        {
            Mouse.OverrideCursor = null;
        }
        #endregion
    }
}
