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

        public void PasteDatasetFromClipboard()
        {
            container = LifetimeService.Instance.Container;
            service = container.Resolve<IDataService>();
            OutputWindowContainer owc = (LifetimeService.Instance.Container.Resolve<IOutputWindowContainer>()) as OutputWindowContainer;
            BSkyMouseBusyHandler.ShowMouseBusy();
            string DSName =  service.GetUniqueNewDatasetname();
            string sheetname = string.Empty;//no sheetname for empty dataset(new dataset)

            string commands = "CreateDFfromClipboard('" + DSName + "'); " +
                "BSkyLoadRefreshDataframe(" + DSName + ")";
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
