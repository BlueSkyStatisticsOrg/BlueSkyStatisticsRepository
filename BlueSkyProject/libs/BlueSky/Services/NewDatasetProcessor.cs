using BSky.Interfaces.Interfaces;
using BSky.Lifetime;
using Microsoft.Practices.Unity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlueSky.Services
{
    class NewDatasetProcessor
    {
        IUnityContainer container = null;
        IDataService service = null;
        IUIController UIController;

        public bool ProcessNewDataset(string dfName = null, bool loadDFinGrid = true)
        {
            container = LifetimeService.Instance.Container;
            service = container.Resolve<IDataService>();
            UIController = LifetimeService.Instance.Container.Resolve<IUIController>();
            string CurrentDatasetName = UIController.GetActiveDocument().Name;
            OutputWindowContainer owc = (LifetimeService.Instance.Container.Resolve<IOutputWindowContainer>()) as OutputWindowContainer;
            SyntaxEditorWindow sewindow = LifetimeService.Instance.Container.Resolve<SyntaxEditorWindow>();
            
            string DSName = CurrentDatasetName;//string.IsNullOrEmpty(dfName) ? service.GetUniqueNewDatasetname() : dfName.Trim();
            string sheetname = string.Empty;//no sheetname for empty dataset(new dataset)

            string isEmpty = sewindow.ExecuteBoolCommand("BSkyIsEmptyDataset('" + DSName + "')");
            bool isProcessed = false;//false:dataset not processed may be it was empty.

            if (!isEmpty.Equals("TRUE"))
            {
                BSkyMouseBusyHandler.ShowMouseBusy();
                string createCommand = "BsKyTeM<-BSkyProcessNewDataset('" + DSName + "'); ";
                string loadInGridCommand = string.Empty;

                if (loadDFinGrid) loadInGridCommand = "BSkyLoadRefreshDataframe(" + DSName + ")";

                string commands = createCommand + loadInGridCommand;
                //PrintDialogTitle("Dataset loaded from the clipboard.");

                sewindow.RunCommands(commands, null);
                sewindow.DisplayAllSessionOutput("Dataset loaded.", (owc.ActiveOutputWindow as OutputWindow));

                BSkyMouseBusyHandler.HideMouseBusy();
                isProcessed = true; //dataset was processed
            }
            //bring main window in front
            Window1 window = LifetimeService.Instance.Container.Resolve<Window1>();
            window.Activate();
            return isProcessed;
        }
    }
}
