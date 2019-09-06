using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using BSky.Interfaces.Controls;
using BSky.Interfaces.Commands;
using Microsoft.Win32;
using BSky.OutputGenerator;
using BSky.Controls.Controls;
using BSky.Controls;
using BSky.Lifetime.Interfaces;
using MSExcelInterop;
using BSky.Lifetime;
using BSky.Interfaces.Model;
using System.Collections.ObjectModel;
using BSky.ConfService.Intf.Interfaces;
using System.Globalization;
using System.Windows.Input;

namespace BSky.Output.Viewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class OuputViewerWindow : Window
    {
        ObservableCollection<AnalyticsData> outputDataList = new ObservableCollection<AnalyticsData>();
        IConfigService confService = null;//25Feb2015
        MSExportToExcel _MSExcelObj; //25Feb2015 one MS excel object for this output window, for all the BSkyControl to share

        public OuputViewerWindow()
        {
            InitializeComponent();
            confService = LifetimeService.Instance.Container.Resolve<IConfigService>();//25Feb2015
            this.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
            _MSExcelObj = new MSExportToExcel(); //25Feb2015initialize
            //testImageLoad();//test image load in mypanel
            //showOutput();// show output window
        }

        private void testImageLoad()
        {
            Image myImage = new Image();
            myImage.Source = new BitmapImage(new Uri(@"D:\temp\graph3.png", UriKind.RelativeOrAbsolute));

            //MemoryStream ms = new MemoryStream();
            //myImage.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg); 

            mypanel.Children.Add(myImage);
        }

        CommandOutput co;
        TreeViewItem SessionItem;

        //25Feb2015 exact copy of AddAnalyisFromFile from OutputWindow.xaml Not Sure if in future they will differ
        public void showOutput(string fullpathfilename)
        {
            FrameworkElement lastElement = null;//25Feb2015
            AnalyticsData analysisdata = null;
            List<SessionOutput> allAnalysis = null;
            BSkyOutputGenerator bsog = new BSkyOutputGenerator();
            allAnalysis = bsog.GenerateOutput(fullpathfilename);
            if (allAnalysis == null)
            {
                return;
            }
            foreach (SessionOutput so in allAnalysis)
            {
                bool isRSession = so.isRSessionOutput;
                if (isRSession)
                {
                    SessionItem = new TreeViewItem();//15Nov2013
                    SessionItem.Header = so.NameOfSession;// (sessionheader != null && sessionheader.Length > 0) ? sessionheader : "R-Session";//18Nov2013 cb;// 
                    SessionItem.IsExpanded = true;
                }

                double extraspaceinbeginning = 0;
                if (mypanel.Children.Count > 0)//if its not the first item on panel
                    extraspaceinbeginning = 10;
                foreach (CommandOutput co in so)
                {
                    analysisdata = new AnalyticsData();//blank entry. There is no open dataset for old/saved ouput
                    analysisdata.Output = co;//saving reference. so that whole outputwindo can be saved again
                    analysisdata.AnalysisType = co.NameOfAnalysis;//For Parent Node name 02Aug2012


                    foreach (DependencyObject obj in co)
                    {
                        FrameworkElement element = obj as FrameworkElement;
                        element.Margin = new Thickness(10, 2 + extraspaceinbeginning, 0, 2); ;
                        mypanel.Children.Add(element);
                        extraspaceinbeginning = 0;
                        lastElement = element;
                    }
                    PopulateTree(co, isRSession);
                    outputDataList.Add(analysisdata);
                }
                if (isRSession)
                    NavTree.Items.Add(SessionItem);//15Nov2013
            }
            //20Oct2014 Did not Work
            //int itcnt = NavTree.Items.Count;
            //if (itcnt > 0 && (NavTree.Items.GetItemAt(itcnt - 1) as TreeViewItem).Tag != null)
            //{
            //    ((NavTree.Items.GetItemAt(itcnt - 1) as TreeViewItem).Tag as FrameworkElement).BringIntoView();
            //}

            //25Feb2015 bring last into focus
            if(lastElement!=null)
                lastElement.BringIntoView();
            //BringOnTop();
        }

        public bool isRunFromSyntaxEditor { get; set; }

        //05Sept2019 exact copy from OutputWindow.xaml. Not Sure if in future they will differ 
        private void PopulateTree(CommandOutput output, bool synedtsession = false)
        {
            string treenocharscount = confService.GetConfigValueForKey("nooftreechars");//16Dec2013
            int openbracketindex, max;
            string analysisName = string.Empty;


            if (output.NameOfAnalysis != null && output.NameOfAnalysis.Trim().Length > 0) // For shortening left tree parent node name.
            {
                openbracketindex = output.NameOfAnalysis.Contains("(") ? output.NameOfAnalysis.IndexOf('(') : output.NameOfAnalysis.Length;
                //if (output.NameOfAnalysis.Trim().Length > 15)
                //    max = 15;
                //else
                //    max = output.NameOfAnalysis.Trim().Length;
                analysisName = output.NameOfAnalysis.Substring(0, openbracketindex);//18Nov2013 (0, max);
                if (output.NameOfAnalysis.Contains("BSkyFormat("))//it is output
                    analysisName = "BSkyFormat-Output";
            }
            else
            {
                analysisName = "Output";
            }

            List<string> Headers = new List<string>();
            TreeViewItem MainItem = new TreeViewItem();
            if (!synedtsession)		///\if (MainItem.Header.ToString().Contains("Execution Started"))
            {
                ///\MainItem.Background = Brushes.LawnGreen;
                MainItem.Header = analysisName;
                MainItem.IsExpanded = true;

                if (MainItem.Header.ToString().Contains("Execution Started"))
                {
                    MainItem.Background = Brushes.LawnGreen;
                }
                if (MainItem.Header.ToString().Contains("Execution Ended"))
                    MainItem.Background = Brushes.SkyBlue;
                //bool setFocus = true;
            }
            ///\if (MainItem.Header.ToString().Contains("Execution Ended"))
            ///\MainItem.Background = Brushes.SkyBlue;
            //bool setFocus = true;

            foreach (DependencyObject obj in output)
            {
                IAUControl control = obj as IAUControl;
                if (control == null) continue;//for non IAUControl
                Headers.Add(control.ControlType);
                TreeViewItem tvi = new TreeViewItem();

                ////Setting common Excel sheet/////
                AUParagraph _aup = obj as AUParagraph;
                if (_aup != null)
                    _aup.MSExcelObj = _MSExcelObj;
                BSkyNotes _note = obj as BSkyNotes;
                if (_note != null)
                    _note.MSExcelObj = _MSExcelObj;
                AUXGrid _aux = obj as AUXGrid;
                if (_aux != null)
                {
                    _aux.MSExcelObj = _MSExcelObj;
                }

                ///Toolbar delete icon saves reference of the analysis output controls ///
                BSkyOutputOptionsToolbar toolbar = obj as BSkyOutputOptionsToolbar;
                if (toolbar != null)
                {
                    toolbar.AnalysisOutput = output;
                }
                ////23Oct2013. for show hide leaf nodes based on checkbox //
                StackPanel treenodesp = new StackPanel();
                treenodesp.Orientation = Orientation.Horizontal;

                int treenodecharlen;//for config char count
                bool result = Int32.TryParse(treenocharscount, out treenodecharlen);
                if (!result)
                    treenodecharlen = 20;

                TextBlock nodetb = new TextBlock();
                nodetb.Tag = control;

                string nodetext = control.ControlType; ;// string.Empty;
                if (_aup != null && _aup.ControlType.Equals("Command"))
                {
                    nodetext = _aup.Text;
                }
                else
                {
                    nodetext = control.ControlType;
                }

                //maxlen is need to avoid indexoutofbounds when finding Substring()
                int maxlen = nodetext.Length < treenodecharlen ? nodetext.Length : (treenodecharlen);
                //if (maxlen > 20) maxlen = 20; //this could be used for putting restriction for max. length

                string dots = maxlen < nodetext.Length ? " ..." : "";//add dots only if text are getting trimmed.

                //Show node text with or without dots based on condition.
                if (maxlen >= nodetext.Length) //show full length
                    nodetb.Text = nodetext.Replace("\n", " ").Replace("\r", " ");
                else
                    nodetb.Text = nodetext.Substring(0, maxlen).Replace("\n", " ").Replace("\r", " ") + dots;

                nodetb.Margin = new Thickness(1, 0, 0, 2);
                nodetb.GotFocus += new RoutedEventHandler(nodetb_GotFocus);
                nodetb.LostFocus += new RoutedEventHandler(nodetb_LostFocus);
                nodetb.ToolTip = BSky.GlobalResources.Properties.UICtrlResources.NavTreeNodeTBTooltip;

                CheckBox cbleaf = new CheckBox();
                cbleaf.Content = "";// control.ControlType;
                cbleaf.Tag = control;
                //cbleaf.Click += new RoutedEventHandler(cbleaf_Checked);
                cbleaf.Checked += new RoutedEventHandler(cbleaf_Checked);
                cbleaf.Unchecked += new RoutedEventHandler(cbleaf_Checked);
                //cbleaf.LostFocus +=new RoutedEventHandler(cbleaf_LostFocus);
                //cbleaf.GotFocus +=new RoutedEventHandler(cbleaf_GotFocus);
                cbleaf.Visibility = System.Windows.Visibility.Visible;///unhide to see it on output window.
                cbleaf.ToolTip = BSky.GlobalResources.Properties.UICtrlResources.NavTreeCheckboxTooltip;

                ///if (!(control is BSkyNotes) && !((control is AUParagraph) && (control.ControlType.Equals("Header"))))
                if (isRunFromSyntaxEditor)
                {
                    //show/hide BSkyNote in the output if templated dialog syntax is run from the command editor
                    if (_note != null)
                        control.BSkyControlVisibility = Visibility.Collapsed;
                    else
                        control.BSkyControlVisibility = Visibility.Visible;
                }
                cbleaf.IsChecked = (control.BSkyControlVisibility == Visibility.Visible) ? true : false;

                #region putting icon in the tree
                Image img = GetImage(control);
                img.Margin = new Thickness(0, 0, 2, 2);
                #endregion

                treenodesp.Children.Add(cbleaf);
                treenodesp.Children.Add(img);
                treenodesp.Children.Add(nodetb);

                tvi.Header = treenodesp;// cbleaf;//.Substring(0,openbracketindex);/// Leaf Node Text
                tvi.Tag = control;
                (control as FrameworkElement).Tag = tvi;

                ////following lines does not show any effect ///
                //FrameworkElement fe = obj as FrameworkElement;
                //fe.GotFocus += new RoutedEventHandler(delegate(object sender, RoutedEventArgs e) { tvi.IsSelected = true; /*Bold or background */ });

                tvi.Selected += new RoutedEventHandler(tvi_Selected);
                tvi.Unselected += new RoutedEventHandler(tvi_Unselected);//29Jan2013

                ///\MainItem.Items.Add(tvi);
                if (synedtsession) //analysis run
                {
                    if (control.ControlType.Equals("Title") || control.ControlType.Equals("Header"))// 'Header' for backward compatibilty
                    {
                        SessionItem.Tag = control;//SessionItem.Header= R-Session and SessionItem.Count==0
                        AddEventsAndContextMenu(SessionItem);
                    }
                    SessionItem.Items.Add(tvi);

                }
                else //dataset opened
                {
                    if (control.ControlType.Equals("Title") || control.ControlType.Equals("Header"))// 'Header' for backward compatibilty
                    {
                        MainItem.Tag = control;
                        AddEventsAndContextMenu(MainItem);
                    }
                    MainItem.Items.Add(tvi);

                }

                //if (setFocus) { fe.Focus();tvi.IsSelected = true; setFocus = false; }//setting focus to start time for each RUN
            }
            /*
            if (synedtsession)
                SessionItem.Items.Add(MainItem);
            else
                NavTree.Items.Add(MainItem);
			*/
            if (!synedtsession)
            {
                NavTree.Items.Add(MainItem);
            }

            //follwing 'if' block could be commented because BringLastLeaf is doing its job. Test, if not scrolling to latest output then uncomment.
            //if (MainItem.Items.Count > 0)//if analysis has something //17Jan2013
            //{
            //    ((MainItem.Items.GetItemAt(0) as TreeViewItem).Tag as FrameworkElement).BringIntoView(); //bring to focus, the latest output.
            //}

            //if (NavTree!=null && NavTree.Items.Count > 0)//11Dec2014 scroll to bottom if there are items present
            //{
            //    int itcount = NavTree.Items.Count;
            //    (NavTree.Items.GetItemAt(itcount - 1) as TreeViewItem).BringIntoView();
            //}
        }

        //05Sept2019 exact copy from OutputWindow.xaml. Not Sure if in future they will differ 
        private Image GetImage(IAUControl ctrl)
        {
            Uri imgUri = null;
            string controlType = ctrl.ControlType;
            string tooltip = string.Empty;
            AUXGrid _aux = ctrl as AUXGrid;
            if (_aux != null)
            {
                controlType = "Table";
            }
            switch (controlType)
            {
                case "Title":
                    imgUri = new Uri("/Images/tree-title.png", UriKind.Relative);
                    tooltip = "Title";
                    break;
                case "Notes":
                    imgUri = new Uri("/Images/tree-notes.png", UriKind.Relative);
                    tooltip = "Notes";
                    break;
                case "Dataset Name":
                    imgUri = new Uri("/Images/tree-dataset.png", UriKind.Relative);
                    tooltip = "Dataset Name";
                    break;
                case "Graphic":
                    imgUri = new Uri("/Images/tree-graphs.png", UriKind.Relative);
                    tooltip = "Plot";
                    break;
                case "Error/Warnings":
                    imgUri = new Uri("/Images/tree-errorwarnings.png", UriKind.Relative);
                    tooltip = "Error/Warnings";
                    break;
                case "Command":
                    imgUri = new Uri("/Images/tree-syntax.png", UriKind.Relative);
                    tooltip = "R Syntax";
                    break;
                case "Table":
                    imgUri = new Uri("/Images/tree-tables.png", UriKind.Relative);
                    tooltip = "Output Table";
                    break;
                case "Toolbar":
                    imgUri = new Uri("/Images/tree-toolbar.png", UriKind.Relative);
                    tooltip = "Analysis toolbar";
                    break;
                default:
                    imgUri = new Uri("/Images/tree-info.png", UriKind.Relative);
                    tooltip = "Info";
                    break;
            }
            //Uri imgUri = new Uri("/Images/input.png", UriKind.Relative);
            Image img = new Image(); img.Source = new BitmapImage(imgUri);
            img.ToolTip = tooltip;
            return img;
        }

        //05Sept2019 exact copy from OutputWindow.xaml. Not Sure if in future they will differ 
        #region Parent node events and context menu
        private void AddEventsAndContextMenu(TreeViewItem TVI)
        {
            //select / unselect
            TVI.Selected += new RoutedEventHandler(MainItem_Selected);
            TVI.Unselected += new RoutedEventHandler(MainItem_UnSelected);

            //Right click event. used only for selecting treeviewitem when right clicked.
            TVI.MouseRightButtonUp += MainItem_MouseRightButtonUp;

            //Context menu
            MenuItem mi1 = new MenuItem();
            mi1.Header = "Select all";
            mi1.Click += Mi1_Click;
            mi1.Tag = TVI;
            MenuItem mi2 = new MenuItem();
            mi2.Header = "Unselect all";
            mi2.Click += Mi2_Click;
            mi2.Tag = TVI;
            MenuItem mi3 = new MenuItem();
            mi3.Header = "Default";
            mi3.Click += Mi3_Click;
            mi3.Tag = TVI;
            ContextMenu cmenu = new ContextMenu();
            cmenu.Items.Add(mi1);
            cmenu.Items.Add(mi2);
            cmenu.Items.Add(mi3);
            TVI.ContextMenu = cmenu;
            TVI.ContextMenuOpening += TVI_ContextMenuOpening;

        }

        private void Mi1_Click(object sender, RoutedEventArgs e)
        {
            SetParentNodeSelectionMode(sender, "All");
        }

        private void Mi2_Click(object sender, RoutedEventArgs e)
        {
            SetParentNodeSelectionMode(sender, "None");
        }

        private void Mi3_Click(object sender, RoutedEventArgs e)
        {
            SetParentNodeSelectionMode(sender, "Default");
        }

        private void SetParentNodeSelectionMode(object sender, string mode)
        {
            FrameworkElement fe = sender as FrameworkElement;
            MenuItem mi = fe as MenuItem;
            TreeViewItem tvi = mi.Tag as TreeViewItem;
            ToggleNavTreeCheckboxes(mode, tvi.Items);
        }

        private void MainItem_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            bool isleafRightClicked = false;
            string clickeditem = (e.OriginalSource.GetType().Name);
            if (!string.IsNullOrEmpty(clickeditem) &&
                (clickeditem.Equals("Rectangle") || clickeditem.Equals("CheckBox") ||
                clickeditem.Equals("Image") ||
                (clickeditem.Equals("TextBlock") && ((e.OriginalSource as TextBlock).Parent != null) &&
                (e.OriginalSource as TextBlock).Parent.GetType().Name.Equals("StackPanel")))
                )
            {
                isleafRightClicked = true;
            }

            if (!isleafRightClicked)
            {
                //MessageBox.Show("for select the parent node in tree");
                FrameworkElement fe = sender as FrameworkElement;
                TreeViewItem tvi = fe as TreeViewItem;
                tvi.IsSelected = true;
            }
        }

        //leaf node Header is stackpanel with checkbox, image and textblock
        //while parent node Header is just a textBlock and textblock parent is null
        private void TVI_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            bool isleafRightClicked = false;
            string clickeditem = (e.OriginalSource.GetType().Name);
            if (!string.IsNullOrEmpty(clickeditem) &&
                (clickeditem.Equals("Rectangle") || clickeditem.Equals("CheckBox") ||
                clickeditem.Equals("Image") ||
                (clickeditem.Equals("TextBlock") && ((e.OriginalSource as TextBlock).Parent != null) &&
                (e.OriginalSource as TextBlock).Parent.GetType().Name.Equals("StackPanel")))
                )
            {
                isleafRightClicked = true;
            }

            if (isleafRightClicked)
                e.Handled = true;
        }

        private void MainItem_Selected(object sender, RoutedEventArgs e)
        {
            FrameworkElement fe = sender as FrameworkElement;
            FrameworkElement tag = fe.Tag as FrameworkElement;
            IAUControl control = tag as IAUControl;
            string navtreeselcom = confService.GetConfigValueForKey("navtreeselectedcol");//23nov2012
            byte red = byte.Parse(navtreeselcom.Substring(3, 2), NumberStyles.HexNumber);
            byte green = byte.Parse(navtreeselcom.Substring(5, 2), NumberStyles.HexNumber);
            byte blue = byte.Parse(navtreeselcom.Substring(7, 2), NumberStyles.HexNumber);
            Color c = Color.FromArgb(255, red, green, blue);
            control.bordercolor = new SolidColorBrush(c);// (Colors.Gold);//05Jun2013
            control.outerborderthickness = new Thickness(2);
            tag.BringIntoView(); //treeview leaf node will appear selected as oppose to Focus()
        }

        private void MainItem_UnSelected(object sender, RoutedEventArgs e)
        {
            FrameworkElement fe = sender as FrameworkElement;
            FrameworkElement tag = fe.Tag as FrameworkElement;
            IAUControl control = tag as IAUControl;
            control.bordercolor = new SolidColorBrush(Colors.Transparent);//05Jun2013
        }

        #endregion

        //05Sept2019 exact copy from OutputWindow.xaml. Not Sure if in future they will differ 
        private void ToggleNavTreeCheckboxes(string selectMode, ItemCollection itc) // "All", "None", "Default"
        {
            foreach (TreeViewItem tvm in itc)
            {
                if (tvm.Items.Count > 0)
                {
                    ToggleNavTreeCheckboxes(selectMode, tvm.Items); // recursive call
                }

                StackPanel sp = null;
                CheckBox cb = null;
                IAUControl control;
                if (tvm.Header is StackPanel)
                {
                    sp = tvm.Header as StackPanel;
                    cb = sp.Children[0] as CheckBox;
                    control = cb.Tag as IAUControl;

                    switch (selectMode)
                    {
                        case "All":
                            cb.IsChecked = true;
                            break;

                        case "None":
                            cb.IsChecked = false;
                            break;

                        default:
                            if (control is BSkyNotes || control.ControlType.Equals("Command"))
                            {
                                cb.IsChecked = false;
                            }
                            else
                                cb.IsChecked = true;
                            break;
                    }
                }
            }
        }

        //05Sept2019 exact copy from OutputWindow.xaml. Not Sure if in future they will differ 
        private void tvi_Selected(object sender, RoutedEventArgs e)
        {
            FrameworkElement fe = sender as FrameworkElement;
            TreeViewItem tvi = fe as TreeViewItem;
            ((tvi.Header as StackPanel).Children[0] as CheckBox).IsChecked = true;
            FrameworkElement tag = fe.Tag as FrameworkElement;
            //scrollviewer.BringIntoView(tag.GetVisualBounds(this));
            IAUControl control = tag as IAUControl;
            ////05Jun2013 control.outerborderthickness = new Thickness(1);
            string navtreeselcom = confService.GetConfigValueForKey("navtreeselectedcol");//23nov2012
            byte red = byte.Parse(navtreeselcom.Substring(3, 2), NumberStyles.HexNumber);
            byte green = byte.Parse(navtreeselcom.Substring(5, 2), NumberStyles.HexNumber);
            byte blue = byte.Parse(navtreeselcom.Substring(7, 2), NumberStyles.HexNumber);
            Color c = Color.FromArgb(255, red, green, blue);
            control.bordercolor = new SolidColorBrush(c);// (Colors.Gold);//05Jun2013
            control.outerborderthickness = new Thickness(2);
            //Rect rct = new Rect(1, 1, tag.ActualWidth,tag.ActualHeight);
            tag.BringIntoView(); //treeview leaf node will appear selected as oppose to Focus()
            e.Handled = true;
            //tag.Focus();
            //(fe as TreeViewItem).Foreground = Brushes.LightBlue; //17Jan2013

            //ScrollViewer sv = this.scrollviewer;
            //sv.ScrollToVerticalOffset(NavTree.Items.IndexOf(tag));

            //ScrollViewer myScrollViewer = (ScrollViewer)NavTree.Template.FindName("_tv_scrollviewer_", NavTree);
            //myScrollViewer.ScrollToHome();
        }

        //05Sept2019 exact copy from OutputWindow.xaml. Not Sure if in future they will differ 
        private void tvi_Unselected(object sender, RoutedEventArgs e)
        {
            FrameworkElement fe = sender as FrameworkElement;
            FrameworkElement tag = fe.Tag as FrameworkElement;
            //scrollviewer.BringIntoView(tag.GetVisualBounds(this));
            IAUControl control = tag as IAUControl;
            ////05Jun2013 control.outerborderthickness = new Thickness(0);
            control.bordercolor = new SolidColorBrush(Colors.Transparent);//05Jun2013
            e.Handled = true;
            //(fe as TreeViewItem).Foreground = Brushes.Black;
            //tag.Focus();
        }

        //this too should be a copy. not sure though
        void cb_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox cb = sender as CheckBox;
            CommandOutput tag = cb.Tag as CommandOutput;

            if (cb.IsChecked == true)
            {
                tag.SelectedForDump = true; // dump this analysis
            }
            else
            {
                tag.SelectedForDump = false;
            }

            if (cb != null)
            {
                TreeViewItem tvparentnode = (TreeViewItem)cb.Parent;
                StackPanel leafnodesp;
                CheckBox leafcb;
                foreach (TreeViewItem tvi in tvparentnode.Items)
                {
                    leafnodesp = (StackPanel)tvi.Header;
                    leafcb = (CheckBox)leafnodesp.Children[0];
                    leafcb.IsChecked = cb.IsChecked;

                }
            }
        }

        //this too should be a copy. not sure though
        //23Oct2013 for leaf nodes. When you check right panel will show associated item.
        void cbleaf_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox cb = sender as CheckBox;

            if (cb.IsChecked == true)
            {
                (cb.Tag as IAUControl).BSkyControlVisibility = System.Windows.Visibility.Visible;
                //(cb.Tag as IAUControl).controlsbordercolor = new SolidColorBrush(Colors.Gold);
                //(cb.Tag as UserControl).BringIntoView(); //scroll to the item in right pane, only when checked
            }
            else
            {
                (cb.Tag as IAUControl).BSkyControlVisibility = System.Windows.Visibility.Collapsed;
                //(cb.Tag as IAUControl).controlsbordercolor = new SolidColorBrush(Colors.Transparent);
            }
            //(cb.Tag as UserControl).BringIntoView(); // scroll to the item in right pane, when you check or uncheck the leaf node

            ///Now Check if all leaf in current analysis are checked. 
            ///If checked = Parent node should be checked
            ///If at least one leaf is unchecked then parent node should also be unchecked.
            /// This may cause one problem. If one child is unchecked then you may go and uncheck parent node,
            /// Which will invode uncheck on parent and further go and uncheck all leafnodes.
            /// Rather,
            /// I think rule should be if all leafnodes are checked/unchecked then its parent should also be checked/unchecked resp.
            if (cb != null)
            {
                StackPanel leafsp = (StackPanel)cb.Parent;
                if (leafsp == null) return;
                TreeViewItem tvleafnode = (TreeViewItem)leafsp.Parent;
                if (tvleafnode != null)
                {
                    TreeViewItem tvparentnode = (TreeViewItem)tvleafnode.Parent;
                    if (tvparentnode != null)
                    {
                        StackPanel leafnodesp;
                        CheckBox leafcb;
                        // state of currently checked/unchecked leaf. Now see if all leaf matches to this.
                        // if current checked and all other leafs are checked then parent node should be checked and vice versa.
                        bool ischked = cb.IsChecked == true ? true : false;
                        bool match = true; // all leaf are in matching checked/unchecked state or not
                        foreach (TreeViewItem tvi in tvparentnode.Items)
                        {
                            leafnodesp = (StackPanel)tvi.Header;
                            leafcb = (CheckBox)leafnodesp.Children[0];
                            if (leafcb.IsChecked != ischked)//if at least one mismatch leaf is found. Parent state will not change and will to match to current leaf state.
                            {
                                //ischked = !ischked; //reversed
                                match = false;
                                break;
                            }

                        }

                        //if (match)//if all leaves match then change the parent node state
                        //{
                        //    //set parentnode checked/unchecked state
                        //    if((tvparentnode.Header as CheckBox)!=null)
                        //    (tvparentnode.Header as CheckBox).IsChecked = ischked;
                        //}
                    }
                }
            }
        }

        //this too should be a copy. not sure though
        //23Oct2013 for leaf nodes. When you check golden border will appear for easy finding
        void nodetb_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBlock tb = sender as TextBlock;
            (tb.Tag as IAUControl).bordercolor = new SolidColorBrush(Colors.Gold);
            (tb.Tag as UserControl).BringIntoView();
        }

        //this too should be a copy. not sure though
        //23Oct2013 for leaf nodes. When you check another item, the old item having golden border will not have that border anymore.
        void nodetb_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBlock tb = sender as TextBlock;
            (tb.Tag as IAUControl).bordercolor = new SolidColorBrush(Colors.Transparent);

        }


        //15Jan2015
        //If deepest leaf (TreeViewItem) is set to invisible (Notes and Error/Warning controls by default set to 'collapsed' or 'hidden') 
        // then from this deepest child we move upwards till visible leaf (control like AUParagraph, BSkyNotes, AUGrid, BSkyGraphic)
        // is found under current parent node. If non is found, we move up and consider another parent with all its child nodes.
        //In case we do not find any visible leaf nodes in SessionItem then we do not try to set BringIntoView at all, 
        // and there will be no output for such SessionItem on right pane in output window, so no scrolling is needed.
        private bool BringLastVisibleLeafIntoView(TreeViewItem t)
        {
            bool found = false;
            if (t.Tag == null && t.Items.Count > 0)// non leaf node
            {
                int childcount = t.Items.Count;
                int indexoflastchild = childcount - 1;//zero based index
                do
                {
                    if (indexoflastchild >= 0)
                    {
                        found = BringLastVisibleLeafIntoView(t.Items.GetItemAt(indexoflastchild) as TreeViewItem);
                        indexoflastchild--;
                    }
                    else
                    {
                        break; // no more children, break out of loop and go one level up.
                    }

                } while (!found);//loop current siblings until visible child is found

            }
            else //leaf node
            {
                if ((t.Tag as IAUControl).BSkyControlVisibility == System.Windows.Visibility.Visible)//leaf is set to visible
                {
                    (t.Tag as FrameworkElement).BringIntoView();
                    t.BringIntoView();//scroll left tree to latest item in tree.(deepest child node at lowest level)
                    found = true;//leaf found and set for scroll into view.
                }
                else // else go to sibling leaf which is above this 't'
                {
                    found = false;
                    return found;
                }
            }
            return found;
        }


        /// <summary>
        /// For opening .bso file in  outputwindow. Many files can be opened in one output window.
        /// It will append output of another .bso, if multiple file are being opened
        /// </summary>
        public const String FileNameFilter = "BSky Output (*.bsoz)|*.bsoz";
      
        private void open_Click(object sender, RoutedEventArgs e)
        {
            
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = FileNameFilter;
            bool? output = openFileDialog.ShowDialog(Application.Current.MainWindow);
            if (output.HasValue && output.Value)
            {
                // Adding analysis from file to the output window
                showOutput(openFileDialog.FileName);
            }
        }
       
        private void MenuItemClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void mypanel_ContextMenuClosing(object sender, ContextMenuEventArgs e)
        {
            bool deleteControl = false;
            FrameworkElement fe = e.Source as FrameworkElement;

            #region find control type
            AUParagraph aup = fe as AUParagraph;
            if (aup != null)
            {
                deleteControl = aup.DeleteControl;
            }

            BSkyNotes bsn = fe as BSkyNotes;
            if (bsn != null)
            {
                deleteControl = bsn.DeleteControl;
            }
            BSkyGraphicControl bsgc = fe as BSkyGraphicControl;
            if (bsgc != null)
            {
                deleteControl = bsgc.DeleteControl;
            }

            AUXGrid auxg = fe as AUXGrid;
            if (auxg != null)
            {
                deleteControl = auxg.DeleteControl;
            }

            #endregion

            if (deleteControl)
            {
				MessageBox.Show("Delete is not allowed in the Output Viewer");
            }
        }

    }
}
