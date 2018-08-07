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
        }

        private void testImageLoad()
        {
            Image myImage = new Image();
            myImage.Source = new BitmapImage(new Uri(@"D:\temp\graph3.png", UriKind.RelativeOrAbsolute));

            mypanel.Children.Add(myImage);
        }

        CommandOutput co;
        TreeViewItem SessionItem;
        public void showOutput_old(string fullpathfilename)
        {
            List<SessionOutput> allAnalysis = null;
            BSkyOutputGenerator bsog = new BSkyOutputGenerator();// create generator
            // read .bso file and create
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
                    SessionItem.Header = so.NameOfSession;
                    SessionItem.IsExpanded = true;
                }

                double extraspaceinbeginning = 0;
                if (mypanel.Children.Count > 0)//if its not the first item on panel
                    extraspaceinbeginning = 10;

            foreach (CommandOutput co in so)
            {
                foreach (DependencyObject obj in co)
                {
                    FrameworkElement element = obj as FrameworkElement;
                    element.Margin = new Thickness(10, 2 + extraspaceinbeginning, 0, 2);
                    mypanel.Children.Add(element);
                }
                PopulateTree(co, isRSession);
            }
        }
        }

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
                    SessionItem.Header = so.NameOfSession;
                    SessionItem.IsExpanded = true;
                }

                double extraspaceinbeginning = 0;
                if (mypanel.Children.Count > 0)//if its not the first item on panel
                    extraspaceinbeginning = 10;
                foreach (CommandOutput co in so)
                {
                    analysisdata = new AnalyticsData();//blank entry. 
                    analysisdata.Output = co;//saving reference. 
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

            //25Feb2015 bring last into focus
            if(lastElement!=null)
                lastElement.BringIntoView();
        }

        private void PopulateTreeOld(CommandOutput output)
        {
            TreeViewItem MainItem = new TreeViewItem();
            MainItem.Header = (output.NameOfAnalysis == null || output.NameOfAnalysis.Trim().Length < 1) ? "Out-put" : output.NameOfAnalysis;// "Output"; /// Parent node text
            MainItem.IsExpanded = true;
            List<string> Headers = new List<string>();
            if (MainItem.Header.ToString().Contains("Execution Started"))
                MainItem.Background = Brushes.LawnGreen;
            if (MainItem.Header.ToString().Contains("Execution Ended"))
                MainItem.Background = Brushes.SkyBlue;
            foreach (DependencyObject obj in output)
            {
                IAUControl control = obj as IAUControl;
                if (control == null) continue;//for non IAUControl
                Headers.Add(control.ControlType);
                TreeViewItem tvi = new TreeViewItem();
                tvi.Header = control.ControlType;/// Leaf Node Text
                tvi.Tag = control;

                tvi.Selected += new RoutedEventHandler(tvi_Selected);
                tvi.Unselected += new RoutedEventHandler(tvi_Unselected);//29Jan2013
                MainItem.Items.Add(tvi);
            }

            NavTree.Items.Add(MainItem);

            if (MainItem.Items.Count > 0)//if analysis has something //17Jan2013
                ((MainItem.Items.GetItemAt(0) as TreeViewItem).Tag as FrameworkElement).BringIntoView(); //bring to focus, the latest output.
        }

        private void PopulateTreeOld2(CommandOutput output, bool synedtsession = false)
        {
            int openbracketindex;
            string analysisName = string.Empty;

            if (output.NameOfAnalysis != null && output.NameOfAnalysis.Trim().Length > 0) // For shortening left tree parent node name.
            {
                openbracketindex = output.NameOfAnalysis.Contains("(") ? output.NameOfAnalysis.IndexOf('(') : output.NameOfAnalysis.Length;
                analysisName = output.NameOfAnalysis.Substring(0, openbracketindex);//18Nov2013 (0, max);
                if (output.NameOfAnalysis.Contains("BSkyFormat("))//it is output
                    analysisName = "BSkyFormat-Output";
            }
            else
            {
                analysisName = "Output";
            }

            //// Main logic to populate tree ////
            TreeViewItem MainItem = new TreeViewItem();
            MainItem.Header = analysisName;
            MainItem.IsExpanded = true;
            List<string> Headers = new List<string>();
            if (MainItem.Header.ToString().Contains("Execution Started"))
            {
                MainItem.Background = Brushes.LawnGreen;
            }
            if (MainItem.Header.ToString().Contains("Execution Ended"))
                MainItem.Background = Brushes.SkyBlue;
            //bool setFocus = true;

            foreach (DependencyObject obj in output)
            {
                IAUControl control = obj as IAUControl;
                if (control == null) continue;//for non IAUControl
                Headers.Add(control.ControlType);
                TreeViewItem tvi = new TreeViewItem();


                ////23Oct2013. for show hide leaf nodes based on checkbox //
                StackPanel treenodesp = new StackPanel();
                treenodesp.Orientation = Orientation.Horizontal;

                TextBlock nodetb = new TextBlock();
                nodetb.Tag = control;
                int maxlen = control.ControlType.Length <= 15 ? control.ControlType.Length : 16;
                string dots = maxlen <= 15 ? "..." : "...";
                nodetb.Text = control.ControlType.Substring(0, maxlen) + dots;
                nodetb.Margin = new Thickness(1);
                nodetb.GotFocus += new RoutedEventHandler(nodetb_GotFocus);
                nodetb.LostFocus += new RoutedEventHandler(nodetb_LostFocus);
                nodetb.ToolTip = "Click to bring the item in the view";

                CheckBox cbleaf = new CheckBox();
                cbleaf.Content = "";
                cbleaf.Tag = control;

                cbleaf.Checked += new RoutedEventHandler(cbleaf_Checked);
                cbleaf.Unchecked += new RoutedEventHandler(cbleaf_Checked);

                cbleaf.Visibility = System.Windows.Visibility.Visible;///unhide to see it on output window.
                cbleaf.ToolTip = "Select/Unselect this node to show/hide in right pane";
                if (!(control is BSkyNotes))
                    cbleaf.IsChecked = true;

                treenodesp.Children.Add(cbleaf);
                treenodesp.Children.Add(nodetb);

                tvi.Header = treenodesp;
                tvi.Tag = control;

                tvi.Selected += new RoutedEventHandler(tvi_Selected);
                tvi.Unselected += new RoutedEventHandler(tvi_Unselected);//29Jan2013
                MainItem.Items.Add(tvi);
            }
            if (synedtsession)
                SessionItem.Items.Add(MainItem);
            else
                NavTree.Items.Add(MainItem);

            if (MainItem.Items.Count > 0)
            {
                ((MainItem.Items.GetItemAt(0) as TreeViewItem).Tag as FrameworkElement).BringIntoView(); //bring to focus, the latest output.
            }
        }

        public bool isRunFromSyntaxEditor { get; set; }
        //25Feb2015 exact copy of PopulateTree from OutputWindow.xaml. Not Sure if in future they will differ 
        private void PopulateTree(CommandOutput output, bool synedtsession = false)
        {
            isRunFromSyntaxEditor = false;
            string treenocharscount = confService.GetConfigValueForKey("nooftreechars");//16Dec2013
            int openbracketindex, max;
            string analysisName = string.Empty;

            if (output.NameOfAnalysis != null && output.NameOfAnalysis.Trim().Length > 0) // For shortening left tree parent node name.
            {
                openbracketindex = output.NameOfAnalysis.Contains("(") ? output.NameOfAnalysis.IndexOf('(') : output.NameOfAnalysis.Length;

                analysisName = output.NameOfAnalysis.Substring(0, openbracketindex);//18Nov2013 (0, max);
                if (output.NameOfAnalysis.Contains("BSkyFormat("))//it is output
                    analysisName = "BSkyFormat-Output";
            }
            else
            {
                analysisName = "Output";
            }


            //// Main logic to populate tree ////
            TreeViewItem MainItem = new TreeViewItem();
            MainItem.Header = analysisName;
            MainItem.IsExpanded = true;
            List<string> Headers = new List<string>();
            if (MainItem.Header.ToString().Contains("Execution Started"))
            {
                MainItem.Background = Brushes.LawnGreen;
            }
            if (MainItem.Header.ToString().Contains("Execution Ended"))
                MainItem.Background = Brushes.SkyBlue;
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
                    _aux.MSExcelObj = _MSExcelObj;


                ////23Oct2013. for show hide leaf nodes based on checkbox //
                StackPanel treenodesp = new StackPanel();
                treenodesp.Orientation = Orientation.Horizontal;

                int treenodecharlen;
                bool result = Int32.TryParse(treenocharscount, out treenodecharlen);
                if (!result)
                    treenodecharlen = 15;

                TextBlock nodetb = new TextBlock();
                nodetb.Tag = control;
                int maxlen = control.ControlType.Length < treenodecharlen ? control.ControlType.Length : (treenodecharlen);
                string dots = maxlen < control.ControlType.Length ? " ..." : "";//add dots only if text are getting trimmed.
                //Show node text with or without dots based on condition.
                if (maxlen <= 0) //show full length
                    nodetb.Text = control.ControlType;
                else
                    nodetb.Text = control.ControlType.Substring(0, maxlen) + dots;
                nodetb.Margin = new Thickness(1);
                nodetb.GotFocus += new RoutedEventHandler(nodetb_GotFocus);
                nodetb.LostFocus += new RoutedEventHandler(nodetb_LostFocus);
                nodetb.ToolTip = "Click to bring the item in the view";

                CheckBox cbleaf = new CheckBox();
                cbleaf.Content = "";// control.ControlType;
                cbleaf.Tag = control;
 
                cbleaf.Checked += new RoutedEventHandler(cbleaf_Checked);
                cbleaf.Unchecked += new RoutedEventHandler(cbleaf_Checked);

                cbleaf.Visibility = System.Windows.Visibility.Visible;///unhide to see it on output window.
                cbleaf.ToolTip = "Select/Unselect this node to show/hide in right pane";

                if (isRunFromSyntaxEditor)
                {
                    control.BSkyControlVisibility = Visibility.Visible;
                }
                cbleaf.IsChecked = (control.BSkyControlVisibility == Visibility.Visible) ? true : false;

                treenodesp.Children.Add(cbleaf);
                treenodesp.Children.Add(nodetb);

                tvi.Header = treenodesp;// cbleaf;//.Substring(0,openbracketindex);/// Leaf Node Text
                tvi.Tag = control;
				(control as FrameworkElement).Tag = tvi;										

                tvi.Selected += new RoutedEventHandler(tvi_Selected);
                tvi.Unselected += new RoutedEventHandler(tvi_Unselected);//29Jan2013

                if (synedtsession)
                    SessionItem.Items.Add(tvi);
                else
                {
                    MainItem.Items.Add(tvi);
                }
            }
			
            if (!synedtsession)
            {
				
                NavTree.Items.Add(MainItem);
            }

        }

        // Logic should'nt be same as in OutputWindow.xaml.cs because this is only meant for displaying output///
        void tvi_Selected(object sender, RoutedEventArgs e)
        {
            FrameworkElement fe = sender as FrameworkElement;
            FrameworkElement tag = fe.Tag as FrameworkElement;

            IAUControl control = tag as IAUControl;

            control.bordercolor = new SolidColorBrush(Colors.Gold);//05Jun2013
			control.outerborderthickness = new Thickness(2);
            tag.BringIntoView(); //treeview leaf node will appear selected as oppose to Focus()
        }



        void tvi_Unselected(object sender, RoutedEventArgs e)
        {
            FrameworkElement fe = sender as FrameworkElement;
            FrameworkElement tag = fe.Tag as FrameworkElement;
            IAUControl control = tag as IAUControl;
            control.bordercolor = new SolidColorBrush(Colors.Transparent);//05Jun2013
        }

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

        //23Oct2013 for leaf nodes. When you check right panel will show associated item.
        void cbleaf_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox cb = sender as CheckBox;

            if (cb.IsChecked == true)
            {
                (cb.Tag as IAUControl).BSkyControlVisibility = System.Windows.Visibility.Visible;
            }
            else
            {
                (cb.Tag as IAUControl).BSkyControlVisibility = System.Windows.Visibility.Collapsed;
            }

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
                        // state of currently checked/unchecked leaf. 
                        bool ischked = cb.IsChecked == true ? true : false;
                        bool match = true; // all leaf are in matching checked/unchecked state or not
                        foreach (TreeViewItem tvi in tvparentnode.Items)
                        {
                            leafnodesp = (StackPanel)tvi.Header;
                            leafcb = (CheckBox)leafnodesp.Children[0];
                            if (leafcb.IsChecked != ischked)
                            {
                                match = false;
                                break;
                            }

                        }

                    }
                }
            }
        }

        void nodetb_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBlock tb = sender as TextBlock;
            (tb.Tag as IAUControl).bordercolor = new SolidColorBrush(Colors.Gold);
            (tb.Tag as UserControl).BringIntoView();
        }

        void nodetb_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBlock tb = sender as TextBlock;
            (tb.Tag as IAUControl).bordercolor = new SolidColorBrush(Colors.Transparent);

        }


        //15Jan2015
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
                        break; 
                    }

                } while (!found);

            }
            else //leaf node
            {
                if ((t.Tag as IAUControl).BSkyControlVisibility == System.Windows.Visibility.Visible)//leaf is set to visible
                {
                    (t.Tag as FrameworkElement).BringIntoView();
                    t.BringIntoView();//scroll left tree to latest item in tree.(deepest child node at lowest level)
                    found = true;//leaf found and set for scroll into view.
                }
                else 
                {
                    found = false;
                    return found;
                }
            }
            return found;
        }


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
