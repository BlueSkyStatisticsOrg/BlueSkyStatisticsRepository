using System.Windows;
using BlueSky.Services;
using BSky.Lifetime;
using BlueSky.CommandBase;
using BSky.Interfaces.Interfaces;
using BSky.Interfaces.Services;

namespace BlueSky.Commands.Output
{
    class SelectOutputWindowCommand : BSkyCommandBase
    {
        string windowname;
        protected override void OnPreExecute(object param)
        {
            UAMenuCommand command = (UAMenuCommand)param;
            windowname = command.commandformat;// getting windowname from commandformat
        }

        protected override void OnExecute(object param)
        {
            if (param != null)
            {
                OnPreExecute(param);//set the windowname which needs to be activated

                //////// get the container and activate required window /////////
                OutputWindowContainer owc = (LifetimeService.Instance.Container.Resolve<IOutputWindowContainer>()) as OutputWindowContainer;
                owc.SetActiveOuputWindow(windowname);

                Window1 window = LifetimeService.Instance.Container.Resolve<Window1>();
                window.OMH.CheckOutputMenuItem(windowname);
            }
            else
            {
                MessageBox.Show("Can't Activate Output. UAMenuCommand properties not set.");
            }
            
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
    }
}
