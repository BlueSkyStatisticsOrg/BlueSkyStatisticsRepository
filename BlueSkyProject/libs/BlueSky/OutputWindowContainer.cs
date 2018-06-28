using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using BSky.Lifetime;
using BSky.Interfaces.Interfaces;

namespace BlueSky
{
    class OutputWindowContainer : IOutputWindowContainer
    {
        /// <summary>
        /// The container of all the output windows.
        /// </summary>
        Dictionary<string, IOutputWindow> outputlist = new Dictionary<string, IOutputWindow>();

        void OutPutWindowContainer()
        {
        }


        IOutputWindow activeoutputwindow;// has the reference of currently active window
        public IOutputWindow ActiveOutputWindow /// during Resolve<IOutputWindowContainer>, this property is called automatically.
        {
            get 
            {
                if (outputlist.Count == 0)// if there are no outputwindow then create one.
                {
                    IOutputWindow iow = new OutputWindow();// output window created
                    AddOutputWindow(iow);/// default (or First) output window added to container
                    activeoutputwindow = iow;//making that first window as the active window, for poulating output
                    iow.loadGettingStarted();//18Sep2017
                }
                return activeoutputwindow; // returning the reference of currently acvtive output window
            }
            set 
            {
                activeoutputwindow = value; // setting some window as active
            }
        }

        int count;
        public int Count
        {
            get 
            {
                count = outputlist.Count;
                return count;
            }
        }

        int wincount=0;//for naming windows. This can only increase.
        public int WinCount
        {
            get { return wincount; }
        }

        //window name and window object
        public Dictionary<string, IOutputWindow> AllOutputWindows//05Feb2013
        {
            get { return outputlist; }
        }

        #region IOutputWindowContainer members

        //Adding new output window to the container by provinding the reference of that output window.
        //This method will name that window automatically.
        public void AddOutputWindow(IOutputWindow iow)
        {
            wincount++;
            iow.WindowName = BSky.GlobalResources.Properties.UICtrlResources.OutputWindowMainTitle + wincount.ToString(); 
            outputlist.Add(iow.WindowName, iow);
            SetActiveOuputWindow(iow.WindowName);
            ////////
            //if (outputlist.Count > 1)
            //{
                MainWindow mwindow = LifetimeService.Instance.Container.Resolve<MainWindow>();//28Jan2013
                Window1 window = LifetimeService.Instance.Container.Resolve<Window1>();
                window.OMH.AddOutputMenuItem(iow.WindowName);/// adding window name to outputmenu. And putting check 
                 //Do same for window menu when you create that menu

                Window temp = iow as Window;
                temp.Height = 650;
                temp.Width = 1024;

                //SolidColorBrush bgcol = new SolidColorBrush();
                //bgcol.Color = Color.FromArgb(150, 255, 0, 0);
                //temp.ModalBackground = bgcol;  

                // comment this if you want to bring app-window in front on click. 
                //temp.Owner = window;//But then output window will not close as you close the app-window.

                temp.Owner = mwindow;// Main Window invisible one is parent and not the app-window that has menus 'File' ...

                //24Jan2013 temp.Title = iow.WindowName; ///Title of output window must match to output menu items. Userfriendly
                temp.Show();
            //}
        }

        // Removing output window from the container( that contains all the outputwindows), by providing windowname
        public void RemoveOutputWindow(string Windowname)
        {
            if (Windowname!=null && outputlist.ContainsKey(Windowname))
            {
                outputlist.Remove(Windowname);

                Window1 window = LifetimeService.Instance.Container.Resolve<Window1>();
                window.OMH.RemoveOutputMenuItem(Windowname);//remove from Output menu And Window menu.

                //set the last window in sequence as a active window.
                if (outputlist.Count > 0)
                {
                    SetActiveOuputWindow(outputlist.ElementAt(outputlist.Count - 1).Value.WindowName);
                    ////putting check on another item in menu
                    window.OMH.CheckOutputMenuItem(outputlist.ElementAt(outputlist.Count - 1).Value.WindowName);
                }
            }
        }

        // Setting output window as active window for populating output, by providing its name //
        public void SetActiveOuputWindow(string Windowname)
        {
            //string WinName = Windowname.Replace("(Active)", "").Trim();
            if (outputlist.ContainsKey(Windowname))
            {
                outputlist.TryGetValue(Windowname, out activeoutputwindow);//get ref of output window
                ///Defaulting title of all windows ////
                foreach(KeyValuePair<String,IOutputWindow> itm in outputlist)
                {
                    Window tempow = itm.Value as Window; 
                    tempow.Title = itm.Key;///Key is WindowName
                }
                // Add only (Active) to only one output window
                (activeoutputwindow as Window).Title = Windowname + " "+BSky.GlobalResources.Properties.UICtrlResources.OutputWindowTitleStatus;
            }
            else
                activeoutputwindow = null;
        }

        // Get output window reference whose name is provided.
        public IOutputWindow GetOuputWindow(string Windowname)
        {
            IOutputWindow iow = null;
            outputlist.TryGetValue(Windowname, out iow);
            return iow;
        }
        #endregion



    }
}
