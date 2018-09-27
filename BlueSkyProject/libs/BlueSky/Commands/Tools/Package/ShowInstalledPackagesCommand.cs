using System;
using System.Windows;
using BSky.Lifetime;
using BSky.Lifetime.Interfaces;
using BSky.Statistics.Common;
using BlueSky.CommandBase;
using System.Text;
using System.Collections.Generic;


namespace BlueSky.Commands.Tools.Package
{
    class ShowInstalledPackagesCommand : BSkyCommandBase
    {
        ILoggerService logService = LifetimeService.Instance.Container.Resolve<ILoggerService>();//17Dec2012
        protected override void OnPreExecute(object param)
        {

        }

        protected override void OnExecute(object param)
        {
            List<string> strlst = GetInstalledRPacakges();
            StringBuilder allpackages = new StringBuilder("");
            int i = 1;
            foreach (string s in strlst)
            {
                //if(s.StartsWith("package:"))
                {
                    allpackages.Append("\"" + s + "\"  ");
                    //allpackages.Append(" ");
                    if (i % 4 == 0) //  4 package in one line
                    {
                        allpackages.Append("\n");
                        i = 0;
                    }
                    i++;
                }
            }

            SendToOutputWindow(BSky.GlobalResources.Properties.Resources.ShowInstalledRPkgs, allpackages.ToString(),false);
        }

        protected override void OnPostExecute(object param)
        {

        }

        public List<string> GetInstalledRPacakges()
        {
            List<string> installed = new List<string>();
            try
            {
                PackageHelperMethods phm = new PackageHelperMethods();
                UAReturn r = phm.ShowInstalledPackages();// ShowInstalledPackages();
                if (r != null && r.Success && r.SimpleTypeData != null)
                {
                    //SendToOutputWindow(r.CommandString, "Show Installed Packages");
                    string[] strarr = null;

                    if (r.SimpleTypeData.GetType().Name.Equals("String"))
                    {
                        strarr = new string[1];
                        strarr[0] = r.SimpleTypeData as string;
                    }
                    else if (r.SimpleTypeData.GetType().Name.Equals("String[]"))
                    {
                        strarr = r.SimpleTypeData as string[];
                    }

                    //strarr to list
                    foreach (string s in strarr)
                        installed.Add(s);
                }
                else
                {
                    SendToOutputWindow(BSky.GlobalResources.Properties.Resources.ErrGettingInstalledPkgs, "", false);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(BSky.GlobalResources.Properties.Resources.ErrGettingInstalledPkgs2, BSky.GlobalResources.Properties.Resources.ErrorOccurred);
                logService.WriteToLogLevel("Error:", LogLevelEnum.Error, ex);
            }
            return installed;
        }
        //public UAReturn ShowInstalledPackages()//06Dec2013 For loading package in R memory for use
        //{
        //         IUnityContainer container = LifetimeService.Instance.Container;
        //        IDataService service = container.Resolve<IDataService>();
        //        return service.showInstalledPackages();
        //}


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
