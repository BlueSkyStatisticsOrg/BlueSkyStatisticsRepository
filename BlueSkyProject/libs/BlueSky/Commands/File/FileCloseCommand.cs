using BSky.Statistics.Common;
using Microsoft.Practices.Unity;
using System.Windows.Forms;
using BSky.Lifetime;
using System.Windows;
using BlueSky.CommandBase;
using BSky.Interfaces.Interfaces;


namespace BlueSky.Commands.File
{
    public class FileCloseCommand : BSkyCommandBase
    {
        protected override void OnPreExecute(object param)
        {
        }

        protected override void OnExecute(object param)
        {

            CloseDataset();
        }

        protected override void OnPostExecute(object param)
        {
        }

        //close from menu or clicking on cross icon 'X' 
        public void CloseDataset(bool confirm = true)
        {
            IUnityContainer container = LifetimeService.Instance.Container;
            IDataService service = container.Resolve<IDataService>();
            IUIController controller = container.Resolve<IUIController>();

            //Get current filetype from loaded dataset. This is file extension and Filter
            DataSource actds = controller.GetActiveDocument();//06Nov2012
            if (actds == null)
                return;
            string datasetName = "" + actds.Name;//uadatasets$lst$
            //Also try to get the filename of currently loaded file. This is FileName.
            string extension = controller.GetActiveDocument().Extension;
            string filename = controller.GetActiveDocument().FileName;
            bool cancel = false;
            if (confirm) //confirm from use about closing and about saving the modified dataset
            {
                if (System.Windows.MessageBox.Show(BSky.GlobalResources.Properties.Resources.CloseConfirmation+" " + filename + " "+BSky.GlobalResources.Properties.Resources.dataset,
                  BSky.GlobalResources.Properties.Resources.CloseDatasetConfirmation, MessageBoxButton.YesNo) == MessageBoxResult.No)
                {
                    // Dont Close the window
                    return;
                }


                if (controller.GetActiveDocument().Changed)//Changes has been done. Do you want to save or Discard
                {
                    DialogResult result = System.Windows.Forms.MessageBox.Show(BSky.GlobalResources.Properties.Resources.SaveConfirmation,
                                                            BSky.GlobalResources.Properties.Resources.SaveChanges,
                                                             MessageBoxButtons.YesNoCancel,
                                                             MessageBoxIcon.Question);
                    if (result == DialogResult.Yes)//save
                    {
                        //If filetype=SPSS then save in RDATA format
                        //For other filetypes data grid can be saved but not the variable grid.
                        // For saving data grid and var grid only save in RDATA format
                        if (extension.Trim().Length < 1 || extension.Equals("sav")) //if no extension or if sav file. no extension in case of new dataset created.
                        {
                            Microsoft.Win32.SaveFileDialog saveasFileDialog = new Microsoft.Win32.SaveFileDialog();
                            saveasFileDialog.Filter = "R Obj (*.RData)|*.RData";
                            bool? output = saveasFileDialog.ShowDialog(System.Windows.Application.Current.MainWindow);
                            if (output.HasValue && output.Value)
                            {
                                service.SaveAs(saveasFileDialog.FileName, controller.GetActiveDocument());// #0
                            }
                        }
                        else
                        {
                            service.SaveAs(filename, controller.GetActiveDocument());// #0
                        }

                    }
                    else if (result == DialogResult.No)//Dont save
                    {

                        //Do nothing
                    }
                    else // Dont close the dataset/tab
                    {
                        cancel = true;
                    }


                }
            }
            if (!cancel)
            {
                service.Close(controller.GetActiveDocument());
                controller.closeTab();
                //container.Dispose();//added to cleanup.

            }
        }

        //02Aug2016 Closing from syntax editor
        public void CloseDatasetFromSyntax(string datasetname)
        {
            IUnityContainer container = LifetimeService.Instance.Container;
            IDataService service = container.Resolve<IDataService>();
            IUIController controller = container.Resolve<IUIController>();

            //Get current filetype from loaded dataset. This is file extension and Filter
            DataSource targetds = controller.GetDocumentByName(datasetname);//06Nov2012
            if (targetds == null)
                return;

            service.Close(targetds);
            controller.closeTab(datasetname);
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
    }
}
