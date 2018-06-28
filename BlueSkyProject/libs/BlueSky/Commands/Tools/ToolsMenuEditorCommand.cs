using System;

using System.Windows;
using BSky.Lifetime;
using BSky.Lifetime.Interfaces;
using BSky.Controls;
using BlueSky.CommandBase;
using System.Threading;

namespace BlueSky.Commands.Tools
{
    class ToolsMenuEditorCommand : BSkyCommandBase
    {
        ILoggerService logService = LifetimeService.Instance.Container.Resolve<ILoggerService>();//17Dec2012
        protected override void OnPreExecute(object param)
        {
            
        }

        protected override void OnExecute(object param)
        {
            string CultureName = string.Empty;//02Oct2017
            string FileName = string.Empty;//02Oct2017

            //23Apr2015 const string FileName = @"./Config/menu.xml";
            //FileName = string.Format(@"{0}menu.xml", BSkyAppData.BSkyAppDirConfigPath);//23Apr2015 


            CultureName = Thread.CurrentThread.CurrentCulture.Name; //02Oct2017
            FileName = string.Format(@"{0}menu.xml", BSkyAppData.RoamingUserBSkyConfigL18nPath);// + CultureName + "/");//02Oct2017


            Window1 appwindow = LifetimeService.Instance.Container.Resolve<Window1>();
            try
            {
                MenuEditor editor = new MenuEditor("");
                editor.Owner = appwindow;
                editor.LoadXml(FileName);
                editor.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
                editor.Activate();
                editor.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show(BSky.GlobalResources.Properties.Resources.ErrModifyingMenu, BSky.GlobalResources.Properties.Resources.ErrorOccurred);
                logService.WriteToLogLevel("Error:", LogLevelEnum.Error, ex);
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
