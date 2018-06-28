using BSky.Interfaces.Commands;
using BSky.Lifetime;
using BSky.Interfaces.Interfaces;

namespace BlueSky.CommandBase
{
    public abstract class BSkyCommandBase : AUCommandBase
    {
        //Send executed command to output window. So, user will know what he executed
        protected override void SendToOutputWindow(string title, string command, bool isCommand=true)//13Dec2013
        {
            #region Get Active output Window
            //////// Active output window ///////
            OutputWindowContainer owc = (LifetimeService.Instance.Container.Resolve<IOutputWindowContainer>()) as OutputWindowContainer;
            OutputWindow ow = owc.ActiveOutputWindow as OutputWindow; //get currently active window
            #endregion
            ow.AddMessage(title, command, isCommand);
        }
    }
}
