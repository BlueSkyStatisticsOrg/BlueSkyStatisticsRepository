using BlueSky.Windows;
using System.Collections;
using BlueSky.Model;
using System.IO;
using BSky.Statistics.Common;
using BSky.Statistics.Service.Engine.Interfaces;
using System.Windows.Controls;
using BSky.Interfaces.Model;
using System.Windows.Input;
using System.Windows;
using System.Windows.Media;
using BSky.Lifetime;
using Microsoft.Practices.Unity;
using System.Windows.Data;
using System.Collections.Generic;
using BSky.Interfaces.Interfaces;
using BSky.Lifetime.Interfaces;


namespace BlueSky.Services
{
    class UIControllerService : IUIController
    {
        ILoggerService logService = LifetimeService.Instance.Container.Resolve<ILoggerService>();//17Dec2012
        /// <summary>
        /// //Microsoft
        /// </summary>
        TabControl docGroup; /// set from window documentContainer

        public TabControl DocGroup
        {
            get { return docGroup; }
            set { docGroup = value; }
        }

        string _activemodel; //set from Model combobox
        public string ActiveModel
        {
            get { return _activemodel; }

            set { _activemodel = value; }
        }
        //public List<string> sortcolnames { get; set; } //11Apr2014
        //public string sortorder { get; set; }//14Apr2014


        public List<string> sortasccolnames { get; set; } //18Oct2015 names of all ascending cols
        public List<string> sortdesccolnames { get; set; } //18Oct2015 names of all descending cols

        IAnalyticsService _analyticsService;
        IOutputWindow _outputWindow;

        public UIControllerService(IAnalyticsService analytics)//, IOutputWindowContainer windowcontainer)
        {
            _analyticsService = analytics;
            //if (GetActiveDocument() == null)//no data set is open. No need to create ouput window.
            //    _outputWindow = null;
            //else
            _outputWindow = null;// windowcontainer.ActiveOutputWindow;

        }

        #region IUIController Members

        //Added by Anil. Just for testing. May not be used in actual
        public void ShowAllOpenDatasetsInGrid()
        {
            string allDatasetNames = "";
            DataPanel dp;
            DataSource ds;
            ItemCollection itc = DocGroup.Items;
            foreach (TabItem ti in itc)
            {
                dp = ti.Content as DataPanel;
                ds = dp.DS;
                string datasetname = ds.FileName;
                string dataset = ds.Name;
                allDatasetNames = allDatasetNames + "\n " + ds.Name + " : " + datasetname;
            }
            MessageBox.Show("All Open Datasets:\n" + allDatasetNames);
        }

        //Added by Anil. Just for testing. May not be used in actual
        public List<string> GetAllOpenDatasetsInGrid()
        {
            List<string> dslist = new List<string>();
            string allDatasetNames = "";
            DataPanel dp;
            DataSource ds;
            ItemCollection itc = DocGroup.Items;
            foreach (TabItem ti in itc)
            {
                dp = ti.Content as DataPanel;
                ds = dp.DS;
                string datasetname = ds.FileName;
                string dataset = ds.Name;
                if (ds != null && ds.FileName != null) 
                {
                    if(!string.IsNullOrEmpty(ds.FileName))//preferably add disk filename
                        dslist.Add(ds.FileName);
                    else if(!string.IsNullOrEmpty(ds.Name))//if no disk filename then add in memory objec name(data.frame name)
                        dslist.Add(ds.Name);
                }
            }
            return dslist;
        }

        // following is not needed. GetTabItem will do the job.
        //Get Datapanel, so that we can run find on the datagrid. This will return the refrence of the current/active datapanel(datagrid)
        public DataPanel GetDataPanel(string datasetname)
        {
            DataPanel dp = null;
            DataSource ds;
            ItemCollection itc = DocGroup.Items;
            foreach (TabItem ti in itc)
            {
                dp = ti.Content as DataPanel;
                ds = dp.DS;
                if (ds.Name.Equals(datasetname))
                {
                    break;
                }
            }
            return dp;
        }

        public void LoadNewDataSet(DataSource ds)
        {
            logService.Info("Grid Loading started." + ds.Name);
            bool bo = false; //These two lines for testing refresh on new row addition. Remove them later.
            if (bo) closeTab();

            if (!DataSourceExists(ds))
            {

                ///DocumentPanel panel = new DocumentPanel();
                //C1DockControlPanel c1panel = new C1DockControlPanel();//Anil
                StackPanel sp = new StackPanel(); //sp.Background = Brushes.Red;
                sp.Orientation = Orientation.Horizontal;
                sp.HorizontalAlignment = HorizontalAlignment.Center;
                sp.VerticalAlignment = VerticalAlignment.Center;
                //sp.Margin = new Thickness(1, 1, 1, 1);

                string sheetname = string.Empty;//29Apr2015
                if (ds.SheetName != null && ds.SheetName.Trim().Length > 0)
                    sheetname = "{" + ds.SheetName + "}";
                Label lb = new Label(); lb.FontWeight = FontWeights.DemiBold;
                var tabtextcolor = new SolidColorBrush(Color.FromArgb(255, (byte)48, (byte)88, (byte)144));//Foreground="#FF1579DA"
                lb.Foreground = tabtextcolor;
                lb.Content = Path.GetFileName(ds.FileName) + sheetname + " (" + ds.Name + ")";//06Aug2012
                lb.ToolTip = ds.FileName + sheetname;
                lb.Margin = new Thickness(1, 1, 1, 1);
                //Button b = new Button(); b.Content="x";
                Image b = new Image(); b.MouseLeftButtonUp += new System.Windows.Input.MouseButtonEventHandler(b_MouseLeftButtonUp);
                b.ToolTip = "Close this dataset"; b.Height = 16.0; b.Width = 22.0;

                string packUri = "pack://application:,,,/BlueSky;component/Images/closetab.png";
                b.Source = new ImageSourceConverter().ConvertFromString(packUri) as ImageSource;
                sp.Children.Add(lb);
                sp.Children.Add(b); //sp.Children.Add(b); 

                TabItem panel = new TabItem();
                //panel.Margin = new Thickness(1, 1, 1, 1);
                b.Tag = panel;//for remove 
                ///////triggers///08Feb2013
                Binding bind = new Binding();//for close image to hide for inactive tabs
                bind.Source = panel;
                bind.Path = new PropertyPath("IsSelected");
                bind.Converter = new BooleanToVisibilityConverter();
                b.SetBinding(Image.VisibilityProperty, bind);

                Style st = new Style(typeof(TabItem));
                var brush1 = new SolidColorBrush(Color.FromArgb(255, (byte)212, (byte)227, (byte)242));//#FFD4E3F2
                st.Setters.Add(new Setter() { Property = TabItem.BackgroundProperty, Value = brush1 });

                Trigger tg = new Trigger { Property = TabItem.IsSelectedProperty, Value = true };
                var brush2 = new SolidColorBrush(Color.FromArgb(255, (byte)234, (byte)239, (byte)245));//#FFEAEFF5
                tg.Setters.Add(new Setter() { Property = TabItem.BackgroundProperty, Value = brush2 });

                st.Triggers.Add(tg);

                panel.Style = st;
                ///triggers///

                DataPanel datapanel = new DataPanel();
                //datapanel.sortcolnames = sortcolnames;//11Apr2014
                //datapanel.sortorder = sortorder; //14Apr2014
                datapanel.sortasccolnames = sortasccolnames;
                datapanel.sortdesccolnames = sortdesccolnames;

                VirtualListDynamic vld = new VirtualListDynamic(_analyticsService, ds);
                vld.DataF = _analyticsService.GetDataFrame(ds);
                if (vld.DataF == null) return; //03Aug2015 When SaveAs fails and tries to load the dataset closing the current one.
                IList list = vld;
                panel.Tag = ds; //panel.Tag = ds;
                datapanel.DS = ds; //sending reference
                datapanel.Data = list;

                datapanel.Variables = ds.Variables;

                datapanel.statusbar.Text = "";// "No Split";//03Dec2013 Status bar
                datapanel.DisableEnableAllNavButtons();
                panel.Header = sp;///Path.GetFileName(ds.FileName);

                panel.Content = datapanel;
                docGroup.Items.Add(panel);//panel
                //panel.IsSelected = true;
                docGroup.SelectedItem = panel;
                //docGroup.SelectedIndex = docGroup.Items.Count - 1;
                //docGroup.Background = Brushes.Red; /color around open dataset
                ///layoutManager.Activate(panel);

                panel.Focus(); //15Oct2015 

                //foreach (TabItem tbit in docGroup.Items)
                //{
                //    tbit.IsSelected = false;
                //}
                //panel.IsSelected = true;
                //docGroup.SelectedIndex = docGroup.Items.Count - 1;
            }
            logService.Info("Grid Loading finished.");
        }

        public void Load_Dataframe(DataSource ds)
        {
            logService.Info("Grid Loading started." + ds.Name);
            bool bo = false; //These two lines for testing refresh on new row addition. Remove them later.
            if (bo) closeTab();

            ///DocumentPanel panel = new DocumentPanel();
            //C1DockControlPanel c1panel = new C1DockControlPanel();//Anil
            StackPanel sp = new StackPanel(); //sp.Background = Brushes.Black;
            sp.Orientation = Orientation.Horizontal;
            //sp.HorizontalAlignment = HorizontalAlignment.Center;
            sp.VerticalAlignment = VerticalAlignment.Center;
            //sp.Margin = new Thickness(1, 1, 1, 1);

            Label lb = new Label(); lb.FontWeight = FontWeights.DemiBold;
            var tabtextcolor = new SolidColorBrush(Color.FromArgb(255, (byte)48, (byte)88, (byte)144));//Foreground="#FF1579DA"
            lb.Foreground = tabtextcolor;
            lb.Content = Path.GetFileName(ds.FileName) + " (" + ds.Name + ")";//06Aug2012
            lb.ToolTip = ds.FileName; lb.Margin = new Thickness(1, 1, 1, 1);
            //Button b = new Button(); b.Content="x";

            Image b = new Image(); b.MouseLeftButtonUp += new System.Windows.Input.MouseButtonEventHandler(b_MouseLeftButtonUp);
            b.ToolTip = "Close this dataset"; b.Height = 16.0; b.Width = 22.0;
            string packUri = "pack://application:,,,/BlueSky;component/Images/closetab.png";
            b.Source = new ImageSourceConverter().ConvertFromString(packUri) as ImageSource;

            sp.Children.Add(lb);
            sp.Children.Add(b); //sp.Children.Add(b); 

            TabItem panel = new TabItem();
            //panel.Margin = new Thickness(1, 1, 1, 1);
            b.Tag = panel;//for remove 
                          ///////triggers///08Feb2013
            Binding bind = new Binding();//for close image to hide for inactive tabs
            bind.Source = panel;
            bind.Path = new PropertyPath("IsSelected");
            bind.Converter = new BooleanToVisibilityConverter();
            b.SetBinding(Image.VisibilityProperty, bind);

            Style st = new Style(typeof(TabItem));
            var brush1 = new SolidColorBrush(Color.FromArgb(255, (byte)212, (byte)227, (byte)242));//#FFD4E3F2
            st.Setters.Add(new Setter() { Property = TabItem.BackgroundProperty, Value = brush1 });

            Trigger tg = new Trigger { Property = TabItem.IsSelectedProperty, Value = true };
            var brush2 = new SolidColorBrush(Color.FromArgb(255, (byte)234, (byte)239, (byte)245));//#FFEAEFF5
            tg.Setters.Add(new Setter() { Property = TabItem.BackgroundProperty, Value = brush2 });

            st.Triggers.Add(tg);

            panel.Style = st;
            ///triggers///

            DataPanel datapanel = new DataPanel();
            datapanel.sortasccolnames = sortasccolnames;
            datapanel.sortdesccolnames = sortdesccolnames;

            //IList list = new VirtualListDynamic(_analyticsService, ds);
            VirtualListDynamic vld = new VirtualListDynamic(_analyticsService, ds);
            vld.DataF = _analyticsService.GetDataFrame(ds);
            IList list = vld;

            panel.Tag = ds; //panel.Tag = ds;
            datapanel.DS = ds; //sending reference
            datapanel.Data = list;

            datapanel.Variables = ds.Variables;

            datapanel.statusbar.Text = "";// "No Split";//03Dec2013 Status bar
            datapanel.DisableEnableAllNavButtons();
            panel.Header = sp;///Path.GetFileName(ds.FileName);
            panel.Content = datapanel;
            docGroup.Items.Add(panel);//panel
            panel.IsSelected = true;
            docGroup.SelectedItem = panel;

            ///layoutManager.Activate(panel);
            //docGroup.UpdateLayout();
            //panel.UpdateLayout();

            //Following focus will not work until the main window is active. Say you called this function 
            //from some dialog window created in C#(not analysis dialog). And the dialog is still on top 
            //of the app while datagrid window will be in back. So following line will have no effect.
            panel.Focus();//15Oct2015 
            logService.Info("Grid Loading finished.");
        }

        public List<string> GetDatasetNames()
        {
            List<string> datasetnames = new List<string>();
            DataSource ds;
            int i = 0;
            foreach (TabItem ti in docGroup.Items)
            {
                ds = ti.Tag as DataSource;
                datasetnames.Add(ds.Name);
                i = 1 + 1;
            }
            return datasetnames;
        }


        /// <summary>
        /// Check if DataSet is already open in one of the tabs
        /// </summary>
        /// <param name="ds"></param>
        private bool DataSourceExists(DataSource ds)
        {
            bool dsexists = false;

            foreach (TabItem ti in docGroup.Items)
            {
                if ((ti.Tag != null) && (ti.Tag as DataSource).FileName != null)//07Feb2016 fix for empty tabs, Filename = null. App crashes.
                {
                    if ((ti.Tag as DataSource).FileName.ToLower().Equals(ds.FileName.ToLower()) &&
                        (ti.Tag as DataSource).SheetName != null && (ti.Tag as DataSource).SheetName.ToLower().Equals(ds.SheetName.ToLower())    //29Apr2015 Sheetname
                        )
                    {
                        dsexists = true;
                        docGroup.SelectedItem = ti;
                        break;
                    }
                }
            }
            return dsexists;
        }

        //Refresh DataSet
        public void RefreshDataSet(DataSource ds)//A.
        {
            TabItem panel = GetTabItem(ds);//04Sep2014
            //04Sep2014 TabItem panel = docGroup.SelectedItem as TabItem;//we dont want active tab. Instead we need tab that matches the title "Dataset1"
            DataPanel datapanel = panel.Content as DataPanel;

            if (!ds.IsPaginationClicked) //if the refresh is called from refresh icon and not from pagination
            {
                ds.StartColindex = 0;
                ds.EndColindex = 15;
                datapanel.InitializeDynamicColIndexes(); // then reset the start and end indexes (to 0 and 15)
            }


            //IList list = new VirtualListDynamic(_analyticsService, ds);
            VirtualListDynamic vld = new VirtualListDynamic(_analyticsService, ds);
            vld.DataF = _analyticsService.GetDataFrame(ds);
            IList list = vld;

            panel.Tag = ds; //panel.Tag = ds;
            datapanel.DS = ds;//sending reference
            //datapanel.sortcolnames = sortcolnames; //11Apr2014
            //datapanel.sortorder = sortorder; //14Apr2014
            datapanel.sortasccolnames = sortasccolnames;
            datapanel.sortdesccolnames = sortdesccolnames;
            datapanel.Data = list;
            datapanel.Variables = ds.Variables; //Refresh Var grid
            datapanel.DisableEnableAllNavButtons();
            datapanel.arrangeVarGridCols(); // arranging col positions
            datapanel.RefreshStatusBar();

            // comment following variable refresh, if dont want to update var grid from R datasets. 
            // Arrangement changing. Calling the arrangeVarGridCols() will solve the arrangement problem.
            // but I guess ObservableCollection is causing problem and  throwing error for text and numeric cols
            // datapanel.Variables = ds.Variables; //uncommented for Compute 22Mar2013

            //05Mar2013 commented b'coz Tab header changed to normal header when added var in vargrid
            //panel.Header = Path.GetFileName(ds.FileName);
        }

        //Refresh DataSet //15Jul2015 dont refresh Variable grid 25MAr2013
        public void RefreshGrids(DataSource ds)//A.
        {
            RefreshDataSet(ds); //Refresh DataGrid
            TabItem panel = GetTabItem(ds);//04Sep2014
            //04Sep2014 TabItem panel = docGroup.SelectedItem as TabItem;//we dont want active tab. Instead we need tab that matches the title "Dataset1"
            DataPanel datapanel = panel.Content as DataPanel;
            //datapanel.sortcolnames = sortcolnames; //11Apr2014
            //datapanel.sortorder = sortorder; //14Apr2014
            //datapanel.Variables = ds.Variables; //Refresh Var grid
            //datapanel.arrangeVarGridCols(); // arranging col positions
            //datapanel.RefreshStatusBar();

            //04Aug2014 Remove old dialogs if cols changed in dataset
            DataSource removedds = panel.Tag as DataSource;
            string semiKey = removedds.FileName + removedds.Name;
            RemoveOldSessionDialogsFromMemory(semiKey);
        }

        //Refresh Data and Var grids from Refresh icon in main window
        //Refresh DataSet and Variable grid 25MAr2013
        public void RefreshBothGrids(DataSource ds)//A.
        {
            RefreshDataSet(ds); //Refresh DataGrid
            TabItem panel = GetTabItem(ds);//04Sep2014
            //04Sep2014 TabItem panel = docGroup.SelectedItem as TabItem;//we dont want active tab. Instead we need tab that matches the title "Dataset1"
            DataPanel datapanel = panel.Content as DataPanel;

            //if (!ds.IsPaginationClicked) //if the refresh is called from refresh icon and not from pagination
            //{
            //    ds.StartColindex = 0;
            //    ds.EndColindex = 15;
            //    datapanel.InitializeDynamicColIndexes(); // then reset the start and end indexes (to 0 and 15)
            //}

            //31May2018
            //refresh datagrid tab. (RData files diff names but same dataframe name, so needs tab title refresh too)
            StackPanel sp = panel.Header as StackPanel;
            Label lb = sp.Children[0] as Label;
            lb.ToolTip = ds.FileName;
            var tabtextcolor = new SolidColorBrush(Color.FromArgb(255, (byte)48, (byte)88, (byte)144));//Foreground="#FF1579DA"
            lb.Foreground = tabtextcolor;
            lb.Content = Path.GetFileName(ds.FileName) + ds.SheetName + " (" + ds.Name + ")";

            //datapanel.sortcolnames = sortcolnames; //11Apr2014
            //datapanel.sortorder = sortorder; //14Apr2014
            datapanel.sortasccolnames = sortasccolnames;
            datapanel.sortdesccolnames = sortdesccolnames;
            datapanel.Variables = ds.Variables; //Refresh Var grid
            datapanel.DisableEnableAllNavButtons();
            datapanel.arrangeVarGridCols(); // arranging col positions
            datapanel.RefreshStatusBar();


            //04Aug2014 Remove old dialogs if cols changed in dataset
            DataSource removedds = panel.Tag as DataSource;
            string semiKey = removedds.FileName + removedds.Name;
            RemoveOldSessionDialogsFromMemory(semiKey);
        }
        //05Dec2013 refreshing status bar for showing split info
        public void RefreshStatusbar()
        {
            TabItem panel = docGroup.SelectedItem as TabItem;
            DataPanel datapanel = panel.Content as DataPanel;
            datapanel.RefreshStatusBar();
        }

        public DataSource GetActiveDocument()
        {
            if (docGroup == null)
                return null;
            ///DocumentPanel panel = docGroup.SelectedItem as DocumentPanel;
            TabItem panel = docGroup.SelectedItem as TabItem;
            if (panel == null)
                return null;
            DataSource source = panel.Tag as DataSource;
            return source;
        }

        //20Feb2014 Getting Tab's Datasource by passing Dataset name
        public DataSource GetDocumentByName(string datasetname) // Dataset1, Dataset2
        {
            DataSource source = null;
            if (docGroup == null)
                return null;
            ///DocumentPanel panel = docGroup.SelectedItem as DocumentPanel;
            TabItem panel = docGroup.SelectedItem as TabItem;
            foreach (TabItem ti in docGroup.Items)
            {
                if (ti == null)
                    continue;
                source = ti.Tag as DataSource;
                if (source.Name.Trim().Equals(datasetname))
                {
                    break;
                }
                source = null;
            }
            return source;
        }

        ////04Sep2014 From DataSource you can get TabItem that has the particular dataset open
        // And that is the one that needs refresh. That may or may not be the active dataset
        public TabItem GetTabItem(DataSource ds)
        {

            if (ds == null)
                return null;

            string Filename = ds.FileName.Trim();// eg.. "C://...//...//dietstudy.sav"
            string Name = ds.Name.Trim();// eg.. "Dataset1"

            string tabFilename = string.Empty;
            string tabName = string.Empty;

            TabItem ti = null;
            bool found = false;

            int tabcount = docGroup.Items.Count;
            for (int idx = 0; idx < tabcount; idx++)
            {
                ti = docGroup.Items.GetItemAt(idx) as TabItem;
                tabFilename = (ti.Tag as DataSource).FileName.Trim();
                tabName = (ti.Tag as DataSource).Name.Trim();
                if (tabFilename.Equals(Filename) && tabName.Equals(Name))
                {
                    found = true;
                    break;
                }
                
            }
            if (!found)
                ti = null;//for loop might have set it to something.

            return ti;
        }

        //closes tab-dataset from 'X' icon
        void b_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            FrameworkElement fe = sender as FrameworkElement;
            FrameworkElement tag = fe.Tag as FrameworkElement;
            TabItem panel = tag as TabItem;
            string fullpathdatasetname = (panel.Tag as DataSource).FileName;
            if (System.Windows.MessageBox.Show("Do you want to close " + fullpathdatasetname + " Dataset?",
              "Do you want to close Dataset?", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                //Close Dataset in R first
                IUnityContainer container = LifetimeService.Instance.Container;
                IDataService service = container.Resolve<IDataService>();
                IUIController controller = container.Resolve<IUIController>();
                DataSource actds = controller.GetActiveDocument();//06Nov2012
                if (actds == null)
                    return;
                /////Save Prompt////13Mar2014
                bool cancel = false;
                string extension = controller.GetActiveDocument().Extension;
                string filename = controller.GetActiveDocument().FileName;
                if (controller.GetActiveDocument().Changed)//Changes has been done. Do you want to save or Discard
                {
                    System.Windows.Forms.DialogResult result = System.Windows.Forms.MessageBox.Show("Do you want to save changes?",
                                                            "Save Changes?",
                                                             System.Windows.Forms.MessageBoxButtons.YesNoCancel,
                                                             System.Windows.Forms.MessageBoxIcon.Question);
                    if (result == System.Windows.Forms.DialogResult.Yes)//save
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
                    else if (result == System.Windows.Forms.DialogResult.No)//Dont save
                    {

                        //Do nothing
                    }
                    else // Dont close the dataset/tab
                    {
                        cancel = true;
                    }

                    //Sort icon fix
                    controller.sortasccolnames = null;//These 2 lines will make sure this is reset. Fix for issue with sort icon
                    controller.sortdesccolnames = null;// Sort dataset col. Close it. Reopen it and you still see sort icons.

                }

                if (!cancel)
                {
                    //// Dataset Closing in UI //////
                    service.Close(controller.GetActiveDocument());
                    // Close the Tab
                    docGroup.Items.Remove(tag);//OR//closeTab(panel);
                    ////13Feb2013 Also remove related dialogs from sessiondialog list
                    RemoveSessionDialogs(panel);
                }
            }
        }

        //AD. to close the active tab document from File>close
        public void closeTab()
        { // Close dataset warning message is shown in FileCloseCommand.cs. And service.Close() is also called there.
            TabItem panel = docGroup.SelectedItem as TabItem;
            ///docGroup.Remove(panel);
            ///docGroup.SelectedTabIndex=0;
            docGroup.Items.Remove(panel); //OR// closeTab(panel);
            ////13Feb2013 Also remove related dialogs from sessiondialog list
            RemoveSessionDialogs(panel);
        }

        //02Aug2016 Close any tab providing its name
        public void closeTab(string datasetname)
        {
            DataSource closeDS = GetDocumentByName(datasetname);
            TabItem panel = GetTabItem(closeDS);
            ///docGroup.Remove(panel);
            ///docGroup.SelectedTabIndex=0;
            docGroup.Items.Remove(panel); //OR// closeTab(panel);
            ////13Feb2013 Also remove related dialogs from sessiondialog list
            RemoveSessionDialogs(panel);
        }
        /// <summary>
        /// Its not in use, I guess
        /// </summary>
        /// <param name="panel"></param>
        private void closeTab(TabItem panel)//21Jan2013
        {

            string fullpathdatasetname = (panel.Tag as DataSource).FileName;
            if (MessageBox.Show("Do you want to close " + fullpathdatasetname + " Dataset?",
              "Do you want to close Dataset?", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                // Close the window
                docGroup.Items.Remove(panel);
                ////13Feb2013 Also remove related dialogs from sessiondialog list
                RemoveSessionDialogs(panel);
            }
        }

        /// <summary>
        /// Remove session dialog if dataset is closed //13Feb2013
        /// </summary>
        /// <param name="data"></param>
        private void RemoveSessionDialogs(TabItem panel)
        {
            DataSource removedds = panel.Tag as DataSource;
            string semiKey = removedds.FileName + removedds.Name;
            List<string> fullkeylist = new List<string>();
            SessionDialogContainer sdc = LifetimeService.Instance.Container.Resolve<SessionDialogContainer>();
            foreach (string fullkey in sdc.SessionDialogList.Keys) //collect all dialogkeys related to dataset that is currently closed
            {
                if (fullkey.Contains(semiKey))
                    fullkeylist.Add(fullkey);
            }

            //Remove items(dialogs) from dictionary //
            foreach (string dlgfullkey in fullkeylist)
            {
                if (sdc.SessionDialogList.ContainsKey(dlgfullkey))//if it has that key
                    sdc.SessionDialogList.Remove(dlgfullkey); // then remove it
            }
        }

        //Remove session dialogs if dataset column names are added or removed.
        //04Aug2014 All the dialogs those have matching partial key, will be removed from dialog dictionary memory
        private void RemoveOldSessionDialogsFromMemory(string partialkey)
        {
            SessionDialogContainer sdc = LifetimeService.Instance.Container.Resolve<SessionDialogContainer>();
            //string partialkey = GetActiveDocument().FileName + GetActiveDocument().Name;
            Dictionary<string, object> SessionDialogs = sdc.SessionDialogList;
            List<string> keylist = new List<string>();
            KeyValuePair<string, object> kvp;

            //collecting all the keys those are supposed to be removed
            foreach (string fullkey in sdc.SessionDialogList.Keys) //collect all dialogkeys related to dataset that is currently closed
            {
                if (fullkey.Contains(partialkey))
                    keylist.Add(fullkey);
            }

            //removing all the dialogs whose keys were stored in keylist.
            foreach (string dlgfullkey in keylist)
            {
                if (sdc.SessionDialogList.ContainsKey(dlgfullkey))//if it has that key
                    sdc.SessionDialogList.Remove(dlgfullkey); // then remove it
            }
        }
        private void RemoveCommandsFromHistory(string Datasetname)
        {
            //Window1 appWindow = LifetimeService.Instance.Container.Resolve<Window1>();
            //appWindow.History.RemoveCommands(Datasetname);
        }

        public void AnalysisComplete(AnalyticsData data)
        {
            if (data.Result.Success)
            {
                OutputWindowContainer owc = (LifetimeService.Instance.Container.Resolve<IOutputWindowContainer>()) as OutputWindowContainer;//new line
                _outputWindow = owc.ActiveOutputWindow;//To get active ouput window to populate analysis.AD.
                _outputWindow.Show();
                _outputWindow.AddAnalyis(data);
            }
        }

        public void LoadOutputWindow(IOutputWindow outputwindow)
        {
            _outputWindow = outputwindow;
        }

        //public OutputWindow getActiveOutputWindow(IOutputWindowContainer windowcontainer)
        //{
        //    return windowcontainer.ActiveOutputWindow as OutputWindow;
        //}
        //No Need
        //public void LoadAnalysisFromFile(string fullpathfilename)//30May2012 loading .bso file
        //{
        //    _outputWindow.AddAnalyisFromFile(fullpathfilename);
        //}
        #endregion


        //can be called from several places to get currelty active model
        public string GetActiveModelName()
        {
            return ActiveModel;
        }
    }
}
