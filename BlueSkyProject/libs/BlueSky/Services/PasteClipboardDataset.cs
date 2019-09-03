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
    class PasteClipboardDataset
    {
        IUnityContainer container = null;
        IDataService service = null;

        public void PasteDatasetFromClipboard(string dfName = null, bool loadDFinGrid = false)
        {
            container = LifetimeService.Instance.Container;
            service = container.Resolve<IDataService>();
            OutputWindowContainer owc = (LifetimeService.Instance.Container.Resolve<IOutputWindowContainer>()) as OutputWindowContainer;
            BSkyMouseBusyHandler.ShowMouseBusy();
            string DSName = string.IsNullOrEmpty(dfName) ? service.GetUniqueNewDatasetname() : dfName.Trim();
            string sheetname = string.Empty;//no sheetname for empty dataset(new dataset)

            string createCommand = "CreateDFfromClipboard('" + DSName + "'); ";
            string loadInGridCommand = string.Empty;

            if (loadDFinGrid) loadInGridCommand = "BSkyLoadRefreshDataframe(" + DSName + ")";

            string commands = createCommand + loadInGridCommand;
            //PrintDialogTitle("Dataset loaded from the clipboard.");
            SyntaxEditorWindow sewindow = LifetimeService.Instance.Container.Resolve<SyntaxEditorWindow>();
            sewindow.RunCommands(commands, null);
            sewindow.DisplayAllSessionOutput("Dataset loaded from the clipboard.", (owc.ActiveOutputWindow as OutputWindow));
            BSkyMouseBusyHandler.HideMouseBusy();

            //bring main window in front
            Window1 window = LifetimeService.Instance.Container.Resolve<Window1>();
            window.Activate();
        }
    }
}
