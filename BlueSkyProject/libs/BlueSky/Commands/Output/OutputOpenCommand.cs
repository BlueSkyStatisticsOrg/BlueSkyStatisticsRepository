using System;
using Microsoft.Win32;
using System.Windows;
using BlueSky.Services;
using BSky.Lifetime;
using BlueSky.CommandBase;
using BSky.Interfaces.Interfaces;
using BSky.Interfaces.Services;

namespace BlueSky.Commands.Output
{
    class OutputOpenCommand : BSkyCommandBase
    {

        IUIController UIController;

        protected override void OnPreExecute(object param)
        {
        }

        public const String FileNameFilter = "BSky Output (*.bsoz)|*.bsoz";

        protected override void OnExecute(object param)
        {
            //get the reference of output window container. If container is empty, a new output window
            // will be created by Resolve() [ it was found that its created on the fly, you cant use debug to
            // step into output window creation ]
            OutputWindowContainer owc = (LifetimeService.Instance.Container.Resolve<IOutputWindowContainer>()) as OutputWindowContainer;
            OutputWindow ow = owc.ActiveOutputWindow as OutputWindow; //get currently active window

            // if 'open' is invoked from specific output window. (it can be active or non-active output window)
            // Then output needs to thrown to this specific output window only.
            if (param != null)
            {
                UAMenuCommand uamc = (UAMenuCommand)param;
                if (uamc.commandformat.Length > 0)
                    ow = owc.GetOuputWindow(uamc.commandformat) as OutputWindow;// get specific output window.
            }



            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = FileNameFilter;
            bool? output = openFileDialog.ShowDialog(ow);// (Application.Current.MainWindow);
            if (output.HasValue && output.Value)
            {
                // Adding analysis from file to the active output window
                ow.AddAnalyisFromFile(openFileDialog.FileName);
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
