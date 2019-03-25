using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using BSky.Interfaces.Model;
using System.Collections.ObjectModel;
using System.Collections;
using BSky.Interfaces.Commands;
using BlueSky.Services;
using BSky.Interfaces.Controls;
using BSky.Controls;
using System.IO;
using System.Text;
using System;
using BlueSky.Commands.Output;
using System.Windows.Media;
using BSky.Controls.Controls;
using System.Windows.Media.Imaging;
using ICSharpCode.SharpZipLib.Zip;
using BSky.Lifetime;
using BSky.Lifetime.Interfaces;
using BSky.OutputGenerator;
using System.Globalization;
using MSExcelInterop;
using BSky.Interfaces.Interfaces;
using C1.WPF.FlexGrid;
using Microsoft.Win32;
using BlueSky.Windows;
using BSky.RecentFileHandler;
using BlueSky.Commands.History;
using BSky.Interfaces.DashBoard;
using BSky.Interfaces;
using BSky.MenuGenerator;
using BSky.Interfaces.Services;
using BlueSky.Dialogs;
using ScintillaNET;
using System.Linq;
using System.Windows.Input;
using BSky.ConfService.Intf.Interfaces;
using System.Windows.Data;

//using System.Windows.Forms;


namespace BlueSky
{

    /// <summary>
    /// Interaction logic for OutputWindow.xaml
    /// </summary>

    public partial class OutputWindow : Window, IOutputWindow
    {
        ObservableCollection<AnalyticsData> outputDataList = new ObservableCollection<AnalyticsData>();
        ObservableCollection<AnalyticsData> SynEdtDataList = new ObservableCollection<AnalyticsData>();//08Aug2012
        ILoggerService logService = LifetimeService.Instance.Container.Resolve<ILoggerService>();//13Dec2012
        IConfigService confService = null;//23nov2012
        MSExportToExcel _MSExcelObj;
        SyntaxEditorWindow sewindow = null;
        RecentDocs recentSyntaxfiles = null;//19May2015
        CommandHistoryMenuHandler chmh = new CommandHistoryMenuHandler();//17Jul2015

        int imgnamecounter;//11Sep2012
        //public static int wincount=0;

        public CommandHistoryMenuHandler History //17Jul2015 accessed form outside for refreshing
        {
            get { return chmh; }
            set { chmh = value; } /// check and remove if not needed
        }

        #region Scintilla Textbox

        private List<string> Keywords1 = null;
        private List<string> Keywords2 = null;
        private string AutoCompleteKeywords = null;
        Scintilla inputTextbox = null;


        private void ConfigureScintilla()
        {
            PrepareKeywords();

            ConfigureRScriptSyntaxHighlight();
            ConfigureRScriptAutoFolding();
            ConifugreRScriptAutoComplete();

            inputTextbox.Margins[0].Width = 30; //Width of the line numbers on the left of the Scintilla text editor.
        }

        #region Scintilla Configuration Methods

        private void PrepareKeywords()
        {
            GetAllAutoCompleteWords();
        }

        //Follwing method creates a sorted list of AutoComplete items and also sets the separator charater to be used
        List<string> autoCList = null;
        char autoCSeparator = ' ';
        private void GetAllAutoCompleteWords()
        {
            string autocomplete = string.Empty;
            autoCList = new List<string>();

            //Read text file 1 line is one entry for autocomplete list
            int counter = 0;
            string line;
            bool isComment = false, isSeparator = false;
            // Read the file and display it line by line.
            System.IO.StreamReader file = new System.IO.StreamReader(".\\AutoComplete.lst");
            while ((line = file.ReadLine()) != null)
            {
                if (line.Trim().StartsWith("#")) // this line has comments
                {
                    isComment = true;
                }
                else if (line.Trim().StartsWith("$")) // character after $ is a character that will be used for separator
                {
                    if (line.Length > 1)
                    {
                        autoCSeparator = line.ToCharArray()[1];
                    }
                    isSeparator = true;
                }
                else
                {
                    isComment = false;
                    isSeparator = false;
                }

                //if its not separator or comment line then add to the list
                if (!isComment && !isSeparator)
                {
                    if (!autoCList.Contains(line.Trim()))//no duplicates are added.
                    {
                        autoCList.Add(line.Trim());
                    }
                }
                counter++;
            }

            file.Close();

            //sort items in templist
            autoCList.Sort();
        }

        private void ConfigureRScriptSyntaxHighlight()
        {
            //StyleCollection

            inputTextbox.StyleResetDefault();
            inputTextbox.Styles[ScintillaNET.Style.Default].Font = "Consolas";
            inputTextbox.Styles[ScintillaNET.Style.Default].Size = 10;
            inputTextbox.StyleClearAll();

            inputTextbox.Styles[ScintillaNET.Style.R.Default].ForeColor = System.Drawing.Color.Brown;
            inputTextbox.Styles[ScintillaNET.Style.R.Comment].ForeColor = System.Drawing.Color.FromArgb(0, 128, 0); // Green
            inputTextbox.Styles[ScintillaNET.Style.R.Number].ForeColor = System.Drawing.Color.Olive;
            inputTextbox.Styles[ScintillaNET.Style.R.BaseKWord].ForeColor = System.Drawing.Color.Purple;
            inputTextbox.Styles[ScintillaNET.Style.R.Identifier].ForeColor = System.Drawing.Color.Black;
            inputTextbox.Styles[ScintillaNET.Style.R.String].ForeColor = System.Drawing.Color.FromArgb(163, 21, 21); // Red
            inputTextbox.Styles[ScintillaNET.Style.R.KWord].ForeColor = System.Drawing.Color.Blue;
            inputTextbox.Styles[ScintillaNET.Style.R.OtherKWord].ForeColor = System.Drawing.Color.Blue;
            inputTextbox.Styles[ScintillaNET.Style.R.String2].ForeColor = System.Drawing.Color.OrangeRed;
            inputTextbox.Styles[ScintillaNET.Style.R.Operator].ForeColor = System.Drawing.Color.Purple;


            inputTextbox.Lexer = Lexer.R;
            inputTextbox.SetKeywords(0, string.Join(" ", autoCList));
        }

        private void ConifugreRScriptAutoComplete()
        {
            inputTextbox.CharAdded += scintilla_CharAdded;
        }

        private void scintilla_CharAdded(object sender, CharAddedEventArgs e)
        {
            Scintilla scintilla = inputTextbox;

            // Find the word start
            var currentPos = scintilla.CurrentPosition;
            var wordStartPos = scintilla.WordStartPosition(currentPos, false);

            scintilla.AutoCSeparator = autoCSeparator;
            scintilla.AutoCMaxWidth = 24;//max number of chars to show
            scintilla.AutoCIgnoreCase = true;

            //Get a filtered list of strings, based on what is entered by user
            string wordchars = scintilla.GetWordFromPosition(wordStartPos);
            GetFilteredAutoCList(wordchars);//("t.t");

            // Display the autocompletion list
            var lenEntered = currentPos - wordStartPos;
            if (lenEntered > 0)
            {
                scintilla.AutoCShow(lenEntered, AutoCompleteKeywords);
            }
        }

        //Filters a list of keywords to match the chars entered by user from the beginning of the word.
        private void GetFilteredAutoCList(string chars)
        {
            string charr;
            List<string> filteredList = new List<string>();
            var sele = autoCList.Where((string x) => x.StartsWith(chars, true, null));
            if (sele != null)
            {

            }
            AutoCompleteKeywords = string.Join(autoCSeparator.ToString(), sele);
        }

        private void ConfigureRScriptAutoFolding()
        {
            Scintilla scintilla = inputTextbox;

            //Instruct the lexer to calculate folding
            scintilla.SetProperty("fold", "1");
            scintilla.SetProperty("fold.compact", "1");

            //Configure a margin to display folding symbols
            scintilla.Margins[2].Type = MarginType.Symbol;
            scintilla.Margins[2].Mask = Marker.MaskFolders;
            scintilla.Margins[2].Sensitive = true;
            scintilla.Margins[2].Width = 20;

            //Set colors for all folding markers
            for (int i = 25; i <= 31; i++)
            {
                scintilla.Markers[i].SetForeColor(System.Drawing.SystemColors.ControlLightLight);
                scintilla.Markers[i].SetBackColor(System.Drawing.SystemColors.ControlDark);
            }

            //Configure folding markers with respective symbols
            scintilla.Markers[Marker.Folder].Symbol = MarkerSymbol.BoxPlus;
            scintilla.Markers[Marker.FolderOpen].Symbol = MarkerSymbol.BoxMinus;
            scintilla.Markers[Marker.FolderEnd].Symbol = MarkerSymbol.BoxPlusConnected;
            scintilla.Markers[Marker.FolderMidTail].Symbol = MarkerSymbol.TCorner;
            scintilla.Markers[Marker.FolderOpenMid].Symbol = MarkerSymbol.BoxMinusConnected;
            scintilla.Markers[Marker.FolderSub].Symbol = MarkerSymbol.VLine;
            scintilla.Markers[Marker.FolderTail].Symbol = MarkerSymbol.LCorner;

            // Enable automatic folding
            scintilla.AutomaticFold = (AutomaticFold.Show | AutomaticFold.Click | AutomaticFold.Change);
        }

        private void HighlightWord(string text)
        {
            Scintilla scintilla = inputTextbox;
            // Indicators 0-7 could be in use by a lexer
            // so we'll use indicator 8 to highlight words.
            const int NUM = 8;

            // Remove all uses of our indicator
            scintilla.IndicatorCurrent = NUM;
            scintilla.IndicatorClearRange(0, scintilla.TextLength);

            // Update indicator appearance
            scintilla.Indicators[NUM].Style = IndicatorStyle.StraightBox;
            scintilla.Indicators[NUM].Under = true;
            scintilla.Indicators[NUM].ForeColor = System.Drawing.Color.Green;
            scintilla.Indicators[NUM].OutlineAlpha = 50;
            scintilla.Indicators[NUM].Alpha = 30;

            // Search the document
            scintilla.TargetStart = 0;
            scintilla.TargetEnd = scintilla.TextLength;
            scintilla.SearchFlags = SearchFlags.None;
            while (scintilla.SearchInTarget(text) != -1)
            {
                // Mark the search results with the current indicator
                scintilla.IndicatorFillRange(scintilla.TargetStart, scintilla.TargetEnd - scintilla.TargetStart);

                // Search the remainder of the document
                scintilla.TargetStart = scintilla.TargetEnd;
                scintilla.TargetEnd = scintilla.TextLength;
            }

        }

        #region ScintillaNET Events
        private void Scintilla_Click(object sender, EventArgs e)
        {
            inputTextbox.IndicatorClearRange(0, inputTextbox.TextLength);
        }

        private void Scintilla_TextChanged(object sender, EventArgs e)
        {
            if (currentScriptFname == null)
                currentScriptFname = string.Empty;
            Modified = true;
            SyntaxTitle.Text = syntitle + " " + currentScriptFname + " < unsaved script >";
        }
        #endregion

        //Highlight Matched words
        private void ScintillaOnSelectionChanged(object sender, EventArgs e)
        {
           
        }

        #endregion


        #endregion


        public OutputWindow()
        {
            InitializeComponent();

            inputTextbox = windowsFormsHost1.Child as Scintilla;
            inputTextbox.Text = "";
            ConfigureScintilla();

            //storing Output Window refrence. this can be useful for many things 
            mypanel.Tag = this;

            confService = LifetimeService.Instance.Container.Resolve<IConfigService>();//23nov2012
            this.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
            _MSExcelObj = new MSExportToExcel(); //initialize

            #region Dialog toolbar
            OutputWindowMenuFactory mf = new OutputWindowMenuFactory(menu1, dialogtoolbar, "test");//, dashBoardService);
            #endregion
            menu1.Items.Insert(menu1.Items.Count - 1, chmh.CommandHistMenu);//17Jul2015 //place output menu just before last item

            #region Syntax section related init (Expand/Collapse , Vertical/Horizontal , Find/Replace , Recent Scripts etc..)
            CollapseSyntax();
            sbfultxt = new StringBuilder();
            initRecentSyntaxFileHandler();
            #endregion
        }

        public void loadGettingStarted() 
        {
            string gettingstartedFilename = "./Sample R Scripts/sample.R";

            if (!File.Exists(gettingstartedFilename))
            {

                string synmsg1 = BSky.GlobalResources.Properties.UICtrlResources.SynGetStrtMsg1 + "\n";
                string synmsg2 = "\n" + BSky.GlobalResources.Properties.UICtrlResources.SynGetStrtMsg2 + "\n";
                string synmsg3 = "\n" + BSky.GlobalResources.Properties.UICtrlResources.SynGetStrtMsg3;
                string synmsg4 = "\n" + BSky.GlobalResources.Properties.UICtrlResources.SynGetStrtMsg4;
                string synmsg5 = "\n" + BSky.GlobalResources.Properties.UICtrlResources.SynGetStrtMsg5;
                string synmsg6 = "\n" + BSky.GlobalResources.Properties.UICtrlResources.SynGetStrtMsg6;
                string synmsg = synmsg1 + synmsg2 + synmsg3 + synmsg4 + synmsg5 + synmsg6;
                inputTextbox.Text = synmsg;
                SyntaxTitle.Text = syntitle;
                return;
            }
            using (System.IO.StreamReader file = new System.IO.StreamReader(gettingstartedFilename))
            {
                inputTextbox.Text = file.ReadToEnd();
                file.Close();
                currentScriptFname = gettingstartedFilename;//26May2015
                recentSyntaxfiles.AddXMLItem(gettingstartedFilename);//19May2015
            }
            SyntaxTitle.Text = syntitle + gettingstartedFilename;
            Modified = false;
        }

        //21Jul2015 Get DashBoardItems for creating toolbar icons
        private void CreateToolbarIcons()
        {
            IDashBoardService dashBoardService = LifetimeService.Instance.Container.Resolve<IDashBoardService>();
            List<DashBoardItem> dbis = dashBoardService.GetDashBoardItems();//Creates menu from menu.xml
            foreach (DashBoardItem dbi in dbis)
            {
                AddToolbarIcon(dbi);
            }
        }

        //21Jun2015 Adds analysis dialog icon to toolbar
        private void AddToolbarIcon(DashBoardItem item)
        {
            if (item.Items != null && item.Items.Count > 0)//Parent Node
            {
                foreach (DashBoardItem dbi in item.Items)
                {
                    AddToolbarIcon(dbi);
                }
            }
            else 
            {
                if (item.showshortcuticon)
                {
                    string icontooltip = item.Name;
                    string imgsource = item.iconfullpathfilename;
                    if (!File.Exists(imgsource))
                    {
                        imgsource = "images/noimage.png";
                    }

                    Button iconButton = new Button();
                    iconButton.Command = item.Command;
                    iconButton.CommandParameter = item.CommandParameter;//all XAML XML info
                    StackPanel sp = new StackPanel();

                    #region create icon
                    System.Windows.Controls.Image iconImage = new System.Windows.Controls.Image();

                    iconImage.ToolTip = icontooltip;
                    var bitmap = new BitmapImage();
                    try
                    {
                        var stream = File.OpenRead(imgsource);
                        bitmap.BeginInit();
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.StreamSource = stream;
                        bitmap.EndInit();
                        stream.Close();
                        stream.Dispose();
                        iconImage.Source = bitmap;
                        bitmap.StreamSource.Close();
                    }
                    catch (Exception ex)
                    {
                        logService.WriteToLogLevel("Error reading Image file while creating shortcut icon " + imgsource + "\n" + ex.Message, LogLevelEnum.Error);
                        MessageBox.Show(this, ex.Message);
                    }
                    #endregion

                    //add to stackpanel
                    sp.Children.Add(iconImage);
                    iconButton.Content = sp;

                    //add iconbutton to toolbar
                    dialogtoolbar.Items.Add(iconButton);
                }
            }
        }

        //Adds all menus and submenu items. But not history or output menu.
        void dashBoardService_AddDashBoardItem(object sender, DashBoardEventArgs e)
        {
            DashBoardItem item = e.DashBoardItem;
            AddToolbarIcon(item);

        }

        #region IOutputWindow Members

        string _windowname;
        public string WindowName
        {
            get { return _windowname; }
            set { _windowname = value; }
        }

        bool tooutputwindow = true;
        public bool ToOutputWindow
        {
            get { return tooutputwindow; }
            set { tooutputwindow = value; }
        }

        bool todiskfile = false;
        public bool ToDiskFile
        {
            get { return todiskfile; }
            set { todiskfile = value; }
        }


        public void AddAnalyis(AnalyticsData analysisdata)
        {
            ICommandAnalyser analyser = CommandAnalyserFactory.GetClientAnalyser(analysisdata);
            CommandOutput output = analyser.Decode(analysisdata);
            output.NameOfAnalysis = analysisdata.AnalysisType;//For Parent Node name 02Aug2012
            if (analysisdata.AnalysisType != null && (analysisdata.AnalysisType.Contains("BSkyFormat") || analysisdata.AnalysisType.Contains("bskyfrmtobj")))
            {
                output.SelectedForDump = analysisdata.SelectedForDump;
                AppendToSyntaxEditorSessionList(output);//18Nov2013
                return;
            }
            if (IsSyntaxSession())
            {
                output.SelectedForDump = analysisdata.SelectedForDump;
                AppendToSyntaxEditorSessionList(output);
                return;
            }

            analysisdata.Output = output;//30May2012

            double extraspaceinbeginning = 0;
            if (mypanel.Children.Count > 0)//if its not the first item on panel
                extraspaceinbeginning = 40;

            FrameworkElement lastElement = null; //for bringing last element into the view. Scroll to last line in output
            if (ToOutputWindow)
            {

                foreach (DependencyObject obj in output)
                {
                    FrameworkElement element = obj as FrameworkElement;
                    element.Margin = new Thickness(10, 2 + extraspaceinbeginning, 0, 2);

                    AUParagraph _aup = obj as AUParagraph;
                    if (_aup != null)
                    {
                        SetTextDynamicWidth(element);
                    }

                    mypanel.Children.Add(element);
                    extraspaceinbeginning = 0;
                    if(element!=null)
                        lastElement = element;
                }

                PopulateTree(output);
                outputDataList.Add(analysisdata);
            }
            if (ToDiskFile)
            {
                SynEdtDataList.Add(analysisdata);
            }

            if (lastElement != null)
                lastElement.BringIntoView();
            BringOnTop();

        }

        //18Nov2013
        public void AppendToSyntaxEditorSessionList(CommandOutput co)
        {
            MainWindow mwindow = LifetimeService.Instance.Container.Resolve<MainWindow>();
            ////// Start Syntax Editor  //////
            SyntaxEditorWindow sewindow = LifetimeService.Instance.Container.Resolve<SyntaxEditorWindow>();
            sewindow.AddToSession(co);
        }

        //07Nov2014 Is there anything in sessionlist
        public bool IsSyntaxSession()
        {
            SyntaxEditorWindow sewindow = LifetimeService.Instance.Container.Resolve<SyntaxEditorWindow>();
            if (sewindow.SesssionListItemCount > 0)
            {
                return true;
            }
            return false;
        }

        public void AddAnalyisFromFile(string fullpathfilename)
        {
            FrameworkElement lastElement = null;
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
                if (mypanel.Children.Count > 0)
                    extraspaceinbeginning = 40;
                foreach (CommandOutput co in so)
                {
                    analysisdata = new AnalyticsData();
                    analysisdata.Output = co;
                    analysisdata.AnalysisType = co.NameOfAnalysis;


                    foreach (DependencyObject obj in co)
                    {
                        FrameworkElement element = obj as FrameworkElement;
                        element.Margin = new Thickness(10, 2 + extraspaceinbeginning, 0, 2); ;

                        AUParagraph _aup = obj as AUParagraph;
                        if (_aup != null)
                        {
                            SetTextDynamicWidth(element);
                        }

                        mypanel.Children.Add(element);
                        extraspaceinbeginning = 0;
                        if (element != null)
                            lastElement = element;
                    }
                    PopulateTree(co, isRSession);
                    outputDataList.Add(analysisdata);
                }
                if (isRSession)
                    NavTree.Items.Add(SessionItem);//15Nov2013
            }
            if (lastElement != null)
                lastElement.BringIntoView();
            BringOnTop();
        }

        //single analysis output from syn edt
        public void AddAnalyisFromSyntaxEditor(CommandOutput lst, string title = "")
        {
            AnalyticsData analysisdata = null;
            SessionOutput allanalysis = new SessionOutput();//15Nov2013
            allanalysis.isRSessionOutput = true;//26Nov2013
            allanalysis.Add(lst); //add to all analysis list
            allanalysis.NameOfSession = title;
            allanalysis.isRSessionOutput = true;
            AddSynEdtSessionOutput(allanalysis);

            BringOnTop();
        }

        TreeViewItem SessionItem;//15Nov2013
        //multiple analysis output from syn edt
        public void AddSynEdtSessionOutput(SessionOutput allanalysis)
        {
            AnalyticsData analysisdata = null;
            if (allanalysis.isRSessionOutput)
            {
                SessionItem = new TreeViewItem();//15Nov2013
                SessionItem.Header = (allanalysis.NameOfSession != null && allanalysis.NameOfSession.Length > 0) ? allanalysis.NameOfSession : "R-Session";//18Nov2013 cb;// 
                SessionItem.IsExpanded = true;

                analysisdata = new AnalyticsData();
                analysisdata.SessionOutput = allanalysis;
                analysisdata.AnalysisType = allanalysis.NameOfSession;
            }

            if (allanalysis == null)// || !last)
            {
                return;
            }
            double extraspaceinbeginning = 0;
            if (mypanel.Children.Count > 0)//if its not the first item on panel
                extraspaceinbeginning = 40;
            foreach (CommandOutput co in allanalysis)
            {
                if (ToOutputWindow) /// sending output to OutputWindow
                {
                    //Original c1flexgrid logic.
                    foreach (DependencyObject obj in co)
                    {
                        FrameworkElement element = obj as FrameworkElement;
                        element.Margin = new Thickness(10, 2 + extraspaceinbeginning, 0, 2); ;


                        AUParagraph _aup = obj as AUParagraph;
                        if (_aup != null)
                        {
                            SetTextDynamicWidth(element);
                        }

                        mypanel.Children.Add(element);
                        extraspaceinbeginning = 0;
                    }
                    PopulateTree(co, allanalysis.isRSessionOutput);
                }

            }
            if (ToOutputWindow)
            {
                outputDataList.Add(analysisdata);
            }
            if (ToDiskFile) 
            {
                SynEdtDataList.Add(analysisdata);
            }

            if (allanalysis.isRSessionOutput)
                NavTree.Items.Add(SessionItem);//15Nov2013

            BringLastVisibleLeafIntoView(SessionItem);
            BringOnTop();
        }

        //Dynamic sizing for text when output window is resized or syntax/nav-tree is resized.
        private void SetTextDynamicWidth(FrameworkElement felement)
        {
            AUParagraph _aup = felement as AUParagraph;
            if (_aup != null)
            {
                TextWidthMultiBind mulbinconvter = new TextWidthMultiBind();
                MultiBinding mulbin = new MultiBinding();//
                mulbin.Converter = mulbinconvter;

                mulbin.Bindings.Add(new Binding("ActualWidth") { Source = this });
                mulbin.Bindings.Add(new Binding("ActualWidth") { Source = windowsFormsHost1 });
                mulbin.Bindings.Add(new Binding("ActualWidth") { Source = navgrid });

                mulbin.NotifyOnSourceUpdated = true;
                felement.SetBinding(FrameworkElement.WidthProperty, mulbin);
            }
        }

        //21Oct2014 
        private void BringFirstLeafIntoView(TreeViewItem t)
        {
            if (t.Tag == null && t.Items.Count > 0)
            {
                BringFirstLeafIntoView(t.Items.GetItemAt(0) as TreeViewItem);
            }
            else
            {
                (t.Tag as FrameworkElement).BringIntoView();
            }
        }

        //08Dec2014 
        //SessionItems are scanned for the last child and that element is brought into view. 
        private void BringLastLeafIntoView_old(TreeViewItem t)
        {
            if (t.Tag == null && t.Items.Count > 0)
            {
                int lastitem = t.Items.Count;
                TreeViewItem temptvi = t.Items.GetItemAt(lastitem - 1) as TreeViewItem;
                BringLastLeafIntoView_old(temptvi);//go one level deeper and to the last child
            }
            else
            {
                (t.Tag as FrameworkElement).BringIntoView();
                t.BringIntoView();//scroll left tree to latest item in tree.(deepest child node at lowest level)
            }
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
            else 
            {
                if ((t.Tag as IAUControl).BSkyControlVisibility == System.Windows.Visibility.Visible)//leaf is set to visible
                {
                    (t.Tag as FrameworkElement).BringIntoView();
                    t.BringIntoView();
                    found = true;
                }
                else 
                {
                    found = false;
                    return found;
                }
            }
            return found;
        }


        public void HardCodedGrid()
        {
            int nrows = 1400; 
            C1FlexGrid c1fgrid = new C1FlexGrid();
            List<Employee> emplist = new List<Employee>();
            Employee tmp;
            //creating data
            for (int r = 0; r < nrows; r++)
            {
                tmp = new Employee() { name = "Name" + r, age = r, city = "City" + r };
                emplist.Add(tmp);
            }


            if (emplist != null && emplist.Count > 0)
            {
                c1fgrid.ItemsSource = emplist;
            }
            mypanel.Children.Add(c1fgrid);

        }

        public void AddMessage(string title, string commandoroutput, bool isCommand = true) 
        {
            // Set custom message
            CommandOutput co = new CommandOutput();
            co.NameOfAnalysis = title;
            co.IsFromSyntaxEditor = false;


            string rcommcol = confService.GetConfigValueForKey("errorcol");//23nov2012
            byte red = byte.Parse(rcommcol.Substring(3, 2), NumberStyles.HexNumber);
            byte green = byte.Parse(rcommcol.Substring(5, 2), NumberStyles.HexNumber);
            byte blue = byte.Parse(rcommcol.Substring(7, 2), NumberStyles.HexNumber);
            Color c = Color.FromArgb(255, red, green, blue);


            AUParagraph aup = new AUParagraph(); 
            aup.Text = title;
            aup.ControlType = "Header";
            aup.FontSize = BSkyStyler.BSkyConstants.HEADER_FONTSIZE;//10Nov2014;
            aup.FontWeight = FontWeights.DemiBold;
            aup.textcolor = new SolidColorBrush(c);
            co.Add(aup);

            AUParagraph aup2 = new AUParagraph(); 
            aup2.FontSize = BSkyStyler.BSkyConstants.TEXT_FONTSIZE;//10Nov2014
            aup2.Text = commandoroutput;
            aup2.ControlType = isCommand ? "Command" : "Output";
            co.Add(aup2);
            ///send output to output window//
            if (co.Count > 0)
                this.AddAnalyisFromSyntaxEditor(co, title);/// send to output 
            BringOnTop();
        }

        #region Read all Analysis in output.
        UTF8Encoding uniEncoding = new UTF8Encoding();
        public void DumpAllAnalyisOuput(string fullpathzipcsvhtmfilename, C1.WPF.FlexGrid.FileFormat ff, bool extratags)
        {
            ////// Start Mouse Busy ////////
            BSkyMouseBusyHandler.ShowMouseBusy();

            string newlinechar = (ff == C1.WPF.FlexGrid.FileFormat.Html && !extratags) ? " <br> " : "\r\n";
            string tabchar = (ff == C1.WPF.FlexGrid.FileFormat.Html) ? " &nbsp; " : "\t";

            imgnamecounter = 0;//11Sep2012
            List<string> filelist = new List<string>();//12Sep2012
            ObservableCollection<AnalyticsData> DataList = null;
            if (SynEdtDataList.Count > 0)
            {
                DataList = SynEdtDataList;
            }
            else
            {
                DataList = outputDataList;
            }
            ///// Creating filename ////
            string filePath = Path.GetDirectoryName(fullpathzipcsvhtmfilename);
            string fileExtension = Path.GetExtension(fullpathzipcsvhtmfilename);
            if (fileExtension.Equals(".bsoz"))
            {
                fileExtension = ".bso";
            }

            string fileNamewithoutExt = Path.GetFileNameWithoutExtension(fullpathzipcsvhtmfilename);
            string fullpathbsocsvhtmfilename = Path.Combine(filePath, fileNamewithoutExt + fileExtension);
            filelist.Add(fileNamewithoutExt + fileExtension);//myout.bso
            ////// root tag/////
            bool savebskytag = extratags;
            FileStream fileStream = new FileStream(fullpathbsocsvhtmfilename, FileMode.Append);
            bool fileExists = File.Exists(fullpathbsocsvhtmfilename);
            string tempString = string.Empty;
            bool oneormorechecked = false;
            ////opening tag/////
            tempString = "<bskyoutput>" + newlinechar;
            if (savebskytag)
                fileStream.Write(uniEncoding.GetBytes(tempString),
                                0, uniEncoding.GetByteCount(tempString));
            /////
            if (ff == FileFormat.Html && !extratags)//21May2015
            {
                string styl = "<style>table td, table th { border: 1px solid #666;} table{border: 1px solid #666;} body > span{ line-height: 30px;} </style>";
                tempString = "<!doctype html><html><head><title>" + fullpathbsocsvhtmfilename + " - BlueSky Output</title>" + styl + "</head> <body>";
                fileStream.Write(uniEncoding.GetBytes(tempString),
                                    0, uniEncoding.GetByteCount(tempString));
            }
            /////

            try
            {
                //////// looping thru all analysis one by one //////
                foreach (AnalyticsData analysisdata in DataList)
                {
                    CommandOutput output = analysisdata.Output;// getting refrence of already generated objects.
                    SessionOutput sessionoutput = analysisdata.SessionOutput;//27Nov2013 if there is session output
                    if (output != null)
                        output.NameOfAnalysis = analysisdata.AnalysisType;//For Parent Node name 02Aug2012
                    if (sessionoutput != null)
                        sessionoutput.NameOfSession = analysisdata.AnalysisType;

                    this.ToDiskFile = false;

                    /////// dumping output //if chkbx based dumping then use commented condition///
                    if (output != null)
                    {
                        ////opening tag/////
                        tempString = newlinechar + "<sessoutput Header= \"\"   isRsession = \"false\">" + newlinechar;
                        if (savebskytag)
                            fileStream.Write(uniEncoding.GetBytes(tempString),
                                            0, uniEncoding.GetByteCount(tempString));

                        ExportOutput(output, ff, fileStream, extratags, filelist);
                        oneormorechecked = true;
                        ////closing tag/////
                        tempString = newlinechar + "</sessoutput>" + newlinechar;
                        if (savebskytag)
                            fileStream.Write(uniEncoding.GetBytes(tempString),
                                            0, uniEncoding.GetByteCount(tempString));
                    }
                    else if (sessionoutput != null)
                    {
                        ////opening tag/////
                        tempString = newlinechar + "<sessoutput Header= \"" + sessionoutput.NameOfSession + "\"  isRsession = \"" + sessionoutput.isRSessionOutput + "\">" + newlinechar;
                        if (savebskytag)
                            fileStream.Write(uniEncoding.GetBytes(tempString),
                                            0, uniEncoding.GetByteCount(tempString));
                        foreach (CommandOutput cout in sessionoutput)
                        {
                            ExportOutput(cout, ff, fileStream, extratags, filelist, true);
                            oneormorechecked = true;
                        }
                        ////closing tag/////
                        tempString = newlinechar + "</sessoutput>" + newlinechar;
                        if (savebskytag)
                            fileStream.Write(uniEncoding.GetBytes(tempString),
                                            0, uniEncoding.GetByteCount(tempString));
                    }

                }//foreach
            }//try
            catch (Exception ex)
            {
                logService.WriteToLogLevel("Error Occurred in exporting output.", LogLevelEnum.Error);
                logService.WriteToLogLevel(ex.Message, LogLevelEnum.Info);
            }
            finally
            {
                ////// End Mouse Busy ////////
                BSkyMouseBusyHandler.HideMouseBusy();// ShowHideBusy_old(false);
            }

            ////closing tag/////
            tempString = "</bskyoutput>" + newlinechar;
            if (savebskytag)
                fileStream.Write(uniEncoding.GetBytes(tempString),
                                0, uniEncoding.GetByteCount(tempString));

            /////
            if (ff == FileFormat.Html && !extratags)//21May2015
            {
                tempString = "</body></html>";
                fileStream.Write(uniEncoding.GetBytes(tempString),
                                    0, uniEncoding.GetByteCount(tempString));
            }
            /////

            fileStream.Close();
            SynEdtDataList.Clear();//Clearing local list after dumping. 08Aug2012
            if (extratags)
                CreateBSkyZipOutput(fullpathzipcsvhtmfilename, filelist);
        }


        public void SaveAsPDFAllAnalyisOuput(string fullpathzipcsvhtmfilename, C1.WPF.FlexGrid.FileFormat ff, bool extratags)
        {
            ////// Start Mouse Busy ////////
            BSkyMouseBusyHandler.ShowMouseBusy();// ShowHideBusy_old(true);

            imgnamecounter = 0;//11Sep2012
            List<string> filelist = new List<string>();//12Sep2012
            ObservableCollection<AnalyticsData> DataList = null;
            if (SynEdtDataList.Count > 0)
            {
                DataList = SynEdtDataList;
            }
            else
            {
                DataList = outputDataList;
            }

            bool fileExists = File.Exists(fullpathzipcsvhtmfilename);

            //Get User configuration from config
            string strPDFPageSize = confService.GetConfigValueForKey("PDFpageSize");//04Mar2016
            string strPDFPageMargin = confService.GetConfigValueForKey("PDFPageMargins");//04Mar2016
            string strMaxTblCol = confService.GetConfigValueForKey("PDFMaxColCount");//04Mar2016
            string strMaxTblRow = confService.GetConfigValueForKey("PDFMaxRowCount");//04Mar2016
            string strPDFfontsize = confService.GetConfigValueForKey("PDFTblFontSize");//04Mar2016                
            string tempDir = BSkyAppData.RoamingUserBSkyTempPath;//;confService.GetConfigValueForKey("tempfolder");
            //get APA config
            string APAconfig = confService.GetConfigValueForKey("outTableInAPAStyle");
            bool APA = (APAconfig.ToLower().Equals("true")) ? true : false;

            //Set User Configuration settings for current PDF generation 
            BSky.ExportToPDF.ExportBSkyOutputToPDF.strPDFPageSize = strPDFPageSize;
            BSky.ExportToPDF.ExportBSkyOutputToPDF.strPDFPageMargin = strPDFPageMargin;
            BSky.ExportToPDF.ExportBSkyOutputToPDF.strMaxTblCol = strMaxTblCol;
            BSky.ExportToPDF.ExportBSkyOutputToPDF.strMaxTblRow = strMaxTblRow;
            BSky.ExportToPDF.ExportBSkyOutputToPDF.strPDFfontsize = strPDFfontsize;
            BSky.ExportToPDF.ExportBSkyOutputToPDF.tempDir = tempDir;
            BSky.ExportToPDF.ExportBSkyOutputToPDF.APAStyle = APA;
            try
            {
                BSky.ExportToPDF.ExportBSkyOutputToPDF.SaveAsPDFAllAnalyisOuput(DataList, fullpathzipcsvhtmfilename);
            }
            catch (Exception ex)
            {
                logService.WriteToLogLevel("Error Occurred in exporting to PDF", LogLevelEnum.Error);
                logService.WriteToLogLevel(ex.Message, LogLevelEnum.Info);
            }
            finally
            {
                ////End Mouse Busy //////
                BSkyMouseBusyHandler.HideMouseBusy();// ShowHideBusy_old(false);
            }
            

            SynEdtDataList.Clear();//Clearing local list after dumping. 08Aug2012

        }



        public void ExportC1FlexGridToPDF(string fullpathzipcsvhtmfilename, string TblTitle, Object obj)
        {
            AUXGrid FGrid = obj as AUXGrid;
            ////// Start Mouse Busy ////////
            BSkyMouseBusyHandler.ShowMouseBusy();// ShowHideBusy_old(true);

            bool fileExists = File.Exists(fullpathzipcsvhtmfilename);

            //Get User configuration from config
            string strPDFPageSize = confService.GetConfigValueForKey("PDFpageSize");//02May2018
            string strPDFPageMargin = confService.GetConfigValueForKey("PDFPageMargins");//02May2018
            string strMaxTblCol = confService.GetConfigValueForKey("PDFMaxColCount");//02May2018
            string strMaxTblRow = confService.GetConfigValueForKey("PDFMaxRowCount");//02May2018
            string strPDFfontsize = confService.GetConfigValueForKey("PDFTblFontSize");//02May2018               
            string tempDir = BSkyAppData.RoamingUserBSkyTempPath;//;confService.GetConfigValueForKey("tempfolder");
            //get APA config
            string APAconfig = confService.GetConfigValueForKey("outTableInAPAStyle");
            bool APA = (APAconfig.ToLower().Equals("true")) ? true : false;

            //Set User Configuration settings for current PDF generation 
            BSky.ExportToPDF.ExportBSkyOutputToPDF.strPDFPageSize = strPDFPageSize;
            BSky.ExportToPDF.ExportBSkyOutputToPDF.strPDFPageMargin = strPDFPageMargin;
            BSky.ExportToPDF.ExportBSkyOutputToPDF.strMaxTblCol = strMaxTblCol;
            BSky.ExportToPDF.ExportBSkyOutputToPDF.strMaxTblRow = strMaxTblRow;
            BSky.ExportToPDF.ExportBSkyOutputToPDF.strPDFfontsize = strPDFfontsize;
            BSky.ExportToPDF.ExportBSkyOutputToPDF.tempDir = tempDir;
            BSky.ExportToPDF.ExportBSkyOutputToPDF.APAStyle = APA;
            try
            {
                BSky.ExportToPDF.ExportBSkyOutputToPDF.ExportFlexGridToPDF(fullpathzipcsvhtmfilename, TblTitle, FGrid);
            }
            catch (Exception ex)
            {
                logService.WriteToLogLevel("Error Occurred in exporting to PDF", LogLevelEnum.Error);
                logService.WriteToLogLevel(ex.Message, LogLevelEnum.Info);
            }
            finally
            {
                ////End Mouse Busy //////
                BSkyMouseBusyHandler.HideMouseBusy();// ShowHideBusy_old(false);
            }

            SynEdtDataList.Clear();//Clearing local list after dumping. 08Aug2012

        }

        #endregion

        #region Save as CSV or HTML or PDF

        private void ExportOutput(CommandOutput output, C1.WPF.FlexGrid.FileFormat ff, FileStream fileStream, bool extratags, List<string> filelist, bool issessionout = false)//csv of excel
        {
            if (output.NameOfAnalysis == null)
                output.NameOfAnalysis = string.Empty;

            string newlinechar = (ff == C1.WPF.FlexGrid.FileFormat.Html && !extratags) ? " <br> " : " \r\n ";
            string tabchar = (ff == C1.WPF.FlexGrid.FileFormat.Html) ? " &nbsp; " : " \t ";

            ////for export to excel///B/ 
            string tempString = "<bskyanalysis>" + newlinechar + "<analysisname> " + output.NameOfAnalysis.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;") + " </analysisname>" + newlinechar;


            //////Writing header tag for each analysis//////
            if (extratags)
                fileStream.Write(uniEncoding.GetBytes(tempString),
                                    0, uniEncoding.GetByteCount(tempString));

            foreach (DependencyObject obj in output)
            {
                FrameworkElement element = obj as FrameworkElement;
                if ((element as AUParagraph) != null)
                {
                    AUParagraph aup = element as AUParagraph;
                    string ctrltype = (aup.ControlType != null) ? aup.ControlType.Replace("\"", "&quot;").Replace("\'", "&apos;") : "-";//09Jul2013
                    if (aup.Text != null)///// <aup> means AUParagraph
                    {
                        //controltype
                        string CONTROLTYPE = (" controltype = \"" + ctrltype + "\" ").Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;");

                        //saving color also//
                        SolidColorBrush scb = (SolidColorBrush)aup.textcolor;
                        string hexcol = scb.Color.ToString();
                        if (hexcol == null) hexcol = "#FF000000";
                        string TEXTCOL = (" textcolor = \"" + hexcol + "\" ").Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;");

                        //saving font size//
                        string fontsize = Convert.ToString(aup.FontSize);
                        if (fontsize == null) fontsize = "14";
                        string FONTSIZE = (" fontsize = \"" + fontsize + "\" ").Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;");

                        //saving font weight//
                        FontWeight fw = aup.FontWeight;
                        FontWeightConverter fwc = new FontWeightConverter();
                        string fontwt = fwc.ConvertToString(fw);
                        if (fontwt == null) fontwt = "{Normal}";
                        string FONTWT = (" fontweight = \"" + fontwt + "\" ").Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;");
                        if (ff == FileFormat.Html && !extratags)
                        {
                            FONTWT = fontwt.Trim().Equals("{Normal}") ? "normal" : "bold";
                        }
                        //text
                        string TEXT = (aup.Text).Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("\"", "&quot;").Replace("\'", "&apos;");
                        if (ff == FileFormat.Html && !extratags)
                            TEXT = "<span style=\"color:" + hexcol + "; font-weight:" + FONTWT + "; font-size:" + fontsize + "px;\">" + TEXT + "</span>";

                        tempString = (extratags) ? " <aup " + CONTROLTYPE + TEXTCOL + FONTSIZE + FONTWT + "> " + TEXT + " </aup>" + newlinechar : TEXT + newlinechar;
                    }
                    byte[] arr = uniEncoding.GetBytes(tempString);
                    fileStream.Write(arr, 0, uniEncoding.GetByteCount(tempString));
                }
                else if ((element as AUXGrid) != null)
                {
                    AUXGrid xgrid = element as AUXGrid; //31Aug2012
                    //////opening auxgrid////<auxgrid>
                    tempString = "<auxgrid>" + newlinechar;
                    if (extratags)
                        fileStream.Write(uniEncoding.GetBytes(tempString.Trim()),
                                            0, uniEncoding.GetByteCount(tempString.Trim()));

                    ////////// Printing Header //////////  <fgheader> means flexgrid header
                    string header = (extratags) ? newlinechar + "<fgheader> " + xgrid.Header.Text.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;") + " </fgheader>" + newlinechar : xgrid.Header.Text.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;");// +newlinechar;
                    string htmlfgheader = "<p><span style=\"color:" + "#FF000000" + "; font-weight:" + "bold" + "; font-size:" + 16 + "px;\">" + header + "</span>";
                    if (ff == FileFormat.Html && !extratags)//23May2015
                    {
                        fileStream.Write(uniEncoding.GetBytes(htmlfgheader),
                                0, uniEncoding.GetByteCount(htmlfgheader));
                    }
                    else
                    {
                        fileStream.Write(uniEncoding.GetBytes(header),
                                0, uniEncoding.GetByteCount(header));
                    }

                    //////////////// Printing Errors ///////////
                    if (xgrid.Metadata != null)//// <errhd> means error heading
                    {
                        header = (extratags) ? "<errhd> Errors/Warnings: </errhd>" + newlinechar : " Errors/Warnings: " + newlinechar; ////error header
                        fileStream.Write(uniEncoding.GetBytes(header),
                        0, uniEncoding.GetByteCount(header));

                        AUParagraph paragraph = new AUParagraph();
                        foreach (KeyValuePair<char, string> keyval in xgrid.Metadata)
                        {
                            paragraph.Text = keyval.Key.ToString() + ":" + keyval.Value; ///// <errm> means error/warning message
                            header = (extratags) ? "<errm> \"" + paragraph.Text.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;") + "\" </errm> " + newlinechar : "\"" + paragraph.Text.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;") + "\" " + newlinechar;
                            fileStream.Write(uniEncoding.GetBytes(header),
                            0, uniEncoding.GetByteCount(header));
                        }
                    }

                    //////// Printing  Grid ////////////
                    AUGrid grid = xgrid.Grid;
                    if (ff == FileFormat.Html && !extratags) //if save as HTML is choosen
                    {
                        ////getting FlexGrid HTML and modifying it then saving to output HTML file////
                        using (var fStream = new MemoryStream())
                        {
                            grid.Save(fStream, C1.WPF.FlexGrid.FileFormat.Html);//change file format here for csv to any other
                            byte[] fgdatarr = new byte[fStream.Length];
                            fStream.Position = 0;//3 or 4
                            fStream.Read(fgdatarr, 0, fgdatarr.Length);
                            string fgstrdata = System.Text.Encoding.UTF8.GetString(fgdatarr);
                            int idxhtml = fgstrdata.IndexOf("<html>");
                            fgstrdata = fgstrdata.Substring(idxhtml).Replace("<html>", "&nbsp;").Replace("<head>", "&nbsp;").Replace("<body>", "&nbsp;").Replace("</body>", "&nbsp;").Replace("</html>", "&nbsp;");
                            fileStream.Write(uniEncoding.GetBytes(fgstrdata), 0, uniEncoding.GetByteCount(fgstrdata));

                            fStream.Close();
                        }
                    }
                    else
                    {

                        grid.Save(fileStream, ff);//change file format here for csv to any other
                    }
                    fileStream.WriteByte(13);
                    fileStream.WriteByte(10);

                    /////////////////Printing Footer  ///////////////
                    //string starfootnotes = string.Empty;
                    bool templatedDialog = false;//There are only 4-5 templated dialogs and they do not have footer text. If they do we can fix this line.
                    if (templatedDialog)
                    {
                        if (xgrid.FootNotes != null)
                        {
                            if (xgrid.FootNotes.Count > 0)
                            {
                                header = (extratags) ? "<ftheader> Footnotes </ftheader>" + newlinechar : "Footnotes " + newlinechar; ////error header
                                fileStream.Write(uniEncoding.GetBytes(header),
                                0, uniEncoding.GetByteCount(header));
                            }
                            AUParagraph footnote = new AUParagraph();
                            foreach (KeyValuePair<char, string> keyval in xgrid.FootNotes)
                            {
                                footnote.Text = keyval.Key.ToString() + ":" + keyval.Value;
                                header = (extratags) ? "<footermsg> \"" + footnote.Text.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;") + "\" </footermsg> " + newlinechar : "\"" + footnote.Text.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;") + "\" " + newlinechar;
                                fileStream.Write(uniEncoding.GetBytes(header),
                                        0, uniEncoding.GetByteCount(header));
                            }
                        }
                    }
                    else
                    {
                        if (xgrid.StarFootNotes != null)
                        {
                            AUParagraph starfootnote = new AUParagraph();
                            starfootnote.Text = xgrid.StarFootNotes;
                            header = (extratags) ? "<footermsg> \"" + starfootnote.Text.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;") + "\" </footermsg> " + newlinechar : "\"" + starfootnote.Text.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;") + "\" " + newlinechar;
                            fileStream.Write(uniEncoding.GetBytes(header),
                                    0, uniEncoding.GetByteCount(header));
                        }
                    }

                    /////closing auxgrid////<auxgrid>
                    tempString = " </auxgrid> " + newlinechar;
                    if (extratags)
                        fileStream.Write(uniEncoding.GetBytes(tempString.Trim()),
                                            0, uniEncoding.GetByteCount(tempString.Trim()));
                }
                else if ((element as BSkyGraphicControl) != null)//Graphics 31Aug2012
                {
                    BSkyGraphicControl bsgc = element as BSkyGraphicControl;

                    //Create image filename
                    string imgfilename = fileStream.Name + bsgc.ImageName + ".png";
                    filelist.Add(Path.GetFileNameWithoutExtension(imgfilename) + ".png");//*.png
                    imgnamecounter++;

                    //Saving Image separately
                    BSkyGraphicControlToImageFile(bsgc, imgfilename);

                    //10Nov2014
                    string imgtag = "<img src=\"" + imgfilename + "\" alt=\"Graphic Here\" >";
                    //saving tag in .BSO file
                    string grpcomm = (extratags) ? "<graphic>" + imgfilename + "</graphic>" + newlinechar : imgtag + newlinechar; ////error header
                    fileStream.Write(uniEncoding.GetBytes(grpcomm),
                    0, uniEncoding.GetByteCount(grpcomm));

                }
                else if ((element as BSkyNotes) != null && extratags) // Notes Control 
                {
                    BSkyNotes bsn = element as BSkyNotes;
                    string ctrltype = (bsn.ControlType != null) ? bsn.ControlType : "-";//09Jul2013
                    string colltext = (bsn.CollapsedText != null) ? bsn.CollapsedText : "";//09Jul2013
                    int disprowindex = bsn.ShowRow_Index;
                    uint splitposi = bsn.NotesSplitPosition;
                    string[,] notesdata = bsn.NotesData;
                    string heading = bsn.HearderText;
                    string collapsedText = bsn.SummaryText;
                    ///Opening tag///
                    tempString = "<bskynotes> " + newlinechar;
                    if (extratags)
                        fileStream.Write(uniEncoding.GetBytes(tempString.Trim()),
                                            0, uniEncoding.GetByteCount(tempString.Trim()));

                    /// set control type display ///
                    tempString = "<controltype>" + ctrltype.ToString().Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;") + "</controltype> " + newlinechar;
                    if (extratags)
                        fileStream.Write(uniEncoding.GetBytes(tempString.Trim()),
                                            0, uniEncoding.GetByteCount(tempString.Trim()));

                    /// set collapse text display ///
                    tempString = "<collapsetext>" + colltext.ToString().Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;") + "</collapsetext> " + newlinechar;
                    if (extratags)
                        fileStream.Write(uniEncoding.GetBytes(tempString.Trim()),
                                            0, uniEncoding.GetByteCount(tempString.Trim()));

                    /// set row index to display ///
                    tempString = "<showrow>" + disprowindex.ToString().Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;") + "</showrow> " + newlinechar;
                    if (extratags)
                        fileStream.Write(uniEncoding.GetBytes(tempString.Trim()),
                                            0, uniEncoding.GetByteCount(tempString.Trim()));

                    /// set split index for drawing vertical line in middle ///
                    tempString = "<splitposi>" + splitposi.ToString().Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;") + "</splitposi> " + newlinechar;
                    if (extratags)
                        fileStream.Write(uniEncoding.GetBytes(tempString.Trim()),
                                            0, uniEncoding.GetByteCount(tempString.Trim()));

                    /// set row index to display /////written to CSV also
                    tempString = (extratags) ? "<notesheading>" + heading.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;") + "</notesheading> " + newlinechar : heading + newlinechar;
                    //if (extratags) 
                    fileStream.Write(uniEncoding.GetBytes(tempString),
                                        0, uniEncoding.GetByteCount(tempString));
                    ///// Notes Data /////not written to CSV but written to HTML and BSO
                    if (ff == C1.WPF.FlexGrid.FileFormat.Html)//04Feb2013  only if condition add. Body code is old
                    {
                        string celldata = string.Empty;
                        for (int row = 0; row < notesdata.GetLength(0); row++)
                        {
                            //start row//
                            tempString = "<notesrow> " + newlinechar;
                            fileStream.Write(uniEncoding.GetBytes(tempString.Trim()),
                                                0, uniEncoding.GetByteCount(tempString.Trim()));
                            ///columns inside each row ///
                            for (int col = 0; col < notesdata.GetLength(1); col++)
                            {
                                celldata = notesdata[row, col];
                                if (celldata != null && celldata.Trim().Length > 0)
                                {
                                    tempString = (extratags) ? " <notescol> " + celldata.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;") + " </notescol> " + newlinechar : celldata.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;") + " &nbsp;&nbsp;&nbsp;&nbsp; ";
                                }
                                else
                                {
                                    tempString = "<notescol> </notescol>" + newlinechar;
                                }
                                fileStream.Write(uniEncoding.GetBytes(tempString.Trim()),
                                    0, uniEncoding.GetByteCount(tempString.Trim()));
                            }
                            //end row//
                            tempString = "</notesrow> " + newlinechar;
                            fileStream.Write(uniEncoding.GetBytes(tempString.Trim()),
                                                0, uniEncoding.GetByteCount(tempString.Trim()));
                        }
                    }
                    else// write collapsed text of Notes in CSV //04Feb2013
                    {
                        tempString = collapsedText.Trim() + newlinechar; /// \n first must
                        fileStream.Write(uniEncoding.GetBytes(tempString),
                                            0, uniEncoding.GetByteCount(tempString));
                    }
                    ///Closing tag///
                    tempString = "</bskynotes> " + newlinechar;
                    if (extratags)
                        fileStream.Write(uniEncoding.GetBytes(tempString.Trim()),
                                            0, uniEncoding.GetByteCount(tempString.Trim()));
                }
            }

            /////ending tag for  each analysis//////
            tempString = " </bskyanalysis> " + newlinechar;
            if (extratags)
                fileStream.Write(uniEncoding.GetBytes(tempString.Trim()),
                                    0, uniEncoding.GetByteCount(tempString.Trim()));
        }

        private void BSkyGraphicControlToImageFile(BSkyGraphicControl bsgc, string fullpathimgfilename)
        {
            System.Windows.Controls.Image myImage = new System.Windows.Controls.Image();
            myImage.Source = bsgc.BSkyImageSource;

            System.Windows.Media.Imaging.BitmapImage bitmapImage = new System.Windows.Media.Imaging.BitmapImage();
            bitmapImage = ((System.Windows.Media.Imaging.BitmapImage)myImage.Source);
            System.Windows.Media.Imaging.PngBitmapEncoder pngBitmapEncoder = new System.Windows.Media.Imaging.PngBitmapEncoder();
            System.IO.FileStream stream = new System.IO.FileStream(fullpathimgfilename, FileMode.Create);

            pngBitmapEncoder.Interlace = PngInterlaceOption.On;
            pngBitmapEncoder.Frames.Add(System.Windows.Media.Imaging.BitmapFrame.Create(bitmapImage));
            pngBitmapEncoder.Save(stream);
            stream.Flush();
            stream.Close();
        }

        #endregion

        # region Zip output files
        private void CreateBSkyZipOutput(string fullpathzipfilename, List<string> filelist)
        {
            string filename = fullpathzipfilename;
            if (true)//files exists those we need to zip
            {
                string fileNamewithoutExt = Path.GetFileNameWithoutExtension(filename);
                string filePath = Path.GetDirectoryName(filename);
                string zipFileName = fileNamewithoutExt;
                zipFileName = Path.Combine(filePath, zipFileName + ".bsoz");


                ///// Creating/Overwriting zip file with fresh entries(ie. all entries in filelist)///
                ZipFile zf = ZipFile.Create(zipFileName);
                zf.BeginUpdate();
                foreach (string fname in filelist)
                {
                    if (File.Exists(Path.Combine(filePath, fname)))
                        zf.Add(Path.Combine(filePath, fname), fname);
                }

                zf.CommitUpdate();
                zf.Close();

                //17Mar2015
                //////// Deleting after zipping //////
                try
                {
                    foreach (string fname in filelist)//remove files those are already begin zipped
                    {
                        File.Delete(Path.Combine(filePath, fname));
                    }
                }
                catch (Exception e)
                {
                    MessageBox.Show("Cleaning temporary file failed. Not Authorized.");
                }
            }
        }
        #endregion

        private void PopulateTree(CommandOutput output, bool synedtsession = false)
        {
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
            ///\TreeViewItem MainItem = new TreeViewItem();
            ///\MainItem.Header = analysisName;
            ///\MainItem.IsExpanded = true;
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
                ////23Oct2013. for show hide leaf nodes based on checkbox //
                StackPanel treenodesp = new StackPanel();
                treenodesp.Orientation = Orientation.Horizontal;

                int treenodecharlen;
                bool result = Int32.TryParse(treenocharscount, out treenodecharlen);
                if (!result)
                    treenodecharlen = 15;

                TextBlock nodetb = new TextBlock();
                nodetb.Tag = control;

                //maxlen is need to avoid indexoutofbounds when finding Substring()
                int maxlen = control.ControlType.Length < treenodecharlen ? control.ControlType.Length : (treenodecharlen);
                //if (maxlen > 100) maxlen = 100; //this could be used for putting restriction for max. length

                string dots = maxlen < control.ControlType.Length ? " ..." : "";//add dots only if text are getting trimmed.

                //Show node text with or without dots based on condition.
                if (maxlen <= 0) //show full length
                    nodetb.Text = control.ControlType;
                else
                    nodetb.Text = control.ControlType.Substring(0, maxlen) + dots;
                nodetb.Margin = new Thickness(1);
                nodetb.GotFocus += new RoutedEventHandler(nodetb_GotFocus);
                nodetb.LostFocus += new RoutedEventHandler(nodetb_LostFocus);
                nodetb.ToolTip = BSky.GlobalResources.Properties.UICtrlResources.NavTreeNodeTBTooltip;

                CheckBox cbleaf = new CheckBox();
                cbleaf.Content = "";// control.ControlType;
                cbleaf.Tag = control;

                cbleaf.Checked += new RoutedEventHandler(cbleaf_Checked);
                cbleaf.Unchecked += new RoutedEventHandler(cbleaf_Checked);

                cbleaf.Visibility = System.Windows.Visibility.Visible;///unhide to see it on output window.
                cbleaf.ToolTip = BSky.GlobalResources.Properties.UICtrlResources.NavTreeCheckboxTooltip;
				
                if (isRunFromSyntaxEditor)
                {
                    control.BSkyControlVisibility = Visibility.Visible;
                }				
				cbleaf.IsChecked = (control.BSkyControlVisibility== Visibility.Visible) ? true : false;

                treenodesp.Children.Add(cbleaf);
                treenodesp.Children.Add(nodetb);

                tvi.Header = treenodesp;// cbleaf;//.Substring(0,openbracketindex);/// Leaf Node Text
                tvi.Tag = control;
				(control as FrameworkElement).Tag = tvi;

                tvi.Selected += new RoutedEventHandler(tvi_Selected);
                tvi.Unselected += new RoutedEventHandler(tvi_Unselected);//29Jan2013
				
                ///\MainItem.Items.Add(tvi);
                if (synedtsession)
                    SessionItem.Items.Add(tvi);
                else
                {
                    MainItem.Items.Add(tvi);
                }
				
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
        }


        void tvi_Selected(object sender, RoutedEventArgs e)
        {
            FrameworkElement fe = sender as FrameworkElement;
            TreeViewItem tvi = fe as TreeViewItem;
            ((tvi.Header as StackPanel).Children[0] as CheckBox).IsChecked = true;
            FrameworkElement tag = fe.Tag as FrameworkElement;

            IAUControl control = tag as IAUControl;

            string navtreeselcom = confService.GetConfigValueForKey("navtreeselectedcol");//23nov2012
            byte red = byte.Parse(navtreeselcom.Substring(3, 2), NumberStyles.HexNumber);
            byte green = byte.Parse(navtreeselcom.Substring(5, 2), NumberStyles.HexNumber);
            byte blue = byte.Parse(navtreeselcom.Substring(7, 2), NumberStyles.HexNumber);
            Color c = Color.FromArgb(255, red, green, blue);
            control.bordercolor = new SolidColorBrush(c);// (Colors.Gold);//05Jun2013
			control.outerborderthickness = new Thickness(2);
            tag.BringIntoView(); 

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

                        bool ischked = cb.IsChecked == true ? true : false;
                        bool match = true; // all leaf are in matching checked/unchecked state or not
                        foreach (TreeViewItem tvi in tvparentnode.Items)
                        {
                            leafnodesp = (StackPanel)tvi.Header;
                            leafcb = (CheckBox)leafnodesp.Children[0];
                            if (leafcb.IsChecked != ischked)//if at least one mismatch leaf is found. Parent state will not change and will to match to current leaf state.
                            {
                                match = false;
                                break;
                            }

                        }

                    }
                }
            }
        }


        //23Oct2013 for leaf nodes. When you check golden border will appear for easy finding
        void nodetb_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBlock tb = sender as TextBlock;
            (tb.Tag as IAUControl).bordercolor = new SolidColorBrush(Colors.Gold);
            (tb.Tag as UserControl).BringIntoView();
        }

        //23Oct2013 for leaf nodes. When you check another item, the old item having golden border will not have that border anymore.
        void nodetb_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBlock tb = sender as TextBlock;
            (tb.Tag as IAUControl).bordercolor = new SolidColorBrush(Colors.Transparent);

        }

        void DeleteOutputItem(FrameworkElement fe)
        {
            mypanel.Children.Remove(fe);
            //NavTree.Items.Remove();
        }
		
        #endregion

        #region Output Window closing/closed events
        private void outwin_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.Activate();
            System.Windows.Forms.DialogResult dresult = System.Windows.Forms.MessageBox.Show( 
                      BSky.GlobalResources.Properties.UICtrlResources.OutputWinSavePromptPart1   
                                                                                                 
                                                                                                 
                      ,
                       BSky.GlobalResources.Properties.UICtrlResources.OutputWinSavePromptTitle,
                      System.Windows.Forms.MessageBoxButtons.YesNoCancel,
                      System.Windows.Forms.MessageBoxIcon.Question);
            if (dresult == System.Windows.Forms.DialogResult.Yes)//SaveAll and close
            {
                ToggleSelectAllItems(true);// select all items for dumping
                SaveAs();
            }
            if (dresult == System.Windows.Forms.DialogResult.Cancel)
            {
                e.Cancel = true;
            }

            #region Syntax Save and Close //06May2015
            //// Also provide save option for syntax section
            if (!e.Cancel)
            {
                bool isClose = SynWindow_Closing();
                if (!isClose) 
                {
                    e.Cancel = true;
                }
            }
            #endregion

        }


        private void outwin_Closed(object sender, EventArgs e)
        {
            OutputWindowContainer owc = (LifetimeService.Instance.Container.Resolve<IOutputWindowContainer>()) as OutputWindowContainer;
            IOutputWindow iow = sender as IOutputWindow;
            string windowname = iow.WindowName;
            owc.RemoveOutputWindow(windowname);//Maybe we can also use _windowname. 
        }
        #endregion



        private void open_Click(object sender, RoutedEventArgs e)
        {
            UAMenuCommand uamc = new UAMenuCommand();
            uamc.commandformat = _windowname;//using commandformat as temporary for storing windowname

            OutputOpenCommand osac = new OutputOpenCommand();
            osac.Execute(uamc);
        }

        private void dump_Click(object sender, RoutedEventArgs e)
        {
            SaveAs();
        }

        //21Mar2016
        private void SaveAsPDF_Click(object sender, RoutedEventArgs e)
        {
            SaveAs("PDF");
        }

        private void SaveAs(string filetype = "")
        {

            // Dumping starts ////
            UAMenuCommand uamc = new UAMenuCommand();
            uamc.commandformat = _windowname;//using commandformat as temporary for storing windowname

            uamc.commandtype = filetype;

            OutputSaveAsCommand osac = new OutputSaveAsCommand();
            osac.Execute(uamc);
        }

        //Checking if at least one output is selected/checked for dumping 
        public bool IsOneOrMoreSelected()//24Jan2013
        {
            bool oneormoreslected = false;
            foreach (TreeViewItem tvm in NavTree.Items)
            {
                CheckBox cb = null;
                if (tvm.Header is CheckBox)
                {
                    cb = tvm.Header as CheckBox;
                    if (cb.IsChecked == true)
                    {
                        oneormoreslected = true;
                        break;
                    }
                }
            }
            return oneormoreslected;
        }

        #region Left Navigation Tree Checkbox ( Select All/None/Default )

        #region Old logic. Not in Use
        //Selecting Deselecting all output
        private void selectall_Click(object sender, RoutedEventArgs e)//24Jan2013
        {
            //For MenuItem SelectAll in output window menu
            MenuItem mi = (sender as MenuItem);
            if (NavTree.Items.Count > 0) //if there are items in tree
            {
                bool toggle = (mi.Header as string).Equals("Select All") ? true : false;
                ToggleSelectAllItems(toggle);

                if (toggle)
                {
                    mi.Header = "Deselect All";
                }
                else
                {
                    mi.Header = "Select All";
                }
            }
        }

        private void ToggleSelectAllItems(bool toggle) // Toggle=True(select all), Toggle=False(deselectAll)
        {
            foreach (TreeViewItem tvm in NavTree.Items)
            {
                CheckBox cb = null;
                if (tvm.Header is CheckBox)
                {
                    cb = tvm.Header as CheckBox;
                    cb.IsChecked = toggle;
                }
            }
        }
        #endregion

        #region New Logic. For nested navigation tree


        private void ToggleNavTreeCheckboxes(string selectMode, ItemCollection itc) 
        {
            foreach (TreeViewItem tvm in itc)
            {
                if (tvm.Items.Count > 0)
                {
                    ToggleNavTreeCheckboxes(selectMode, tvm.Items); 
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
                            if (control is BSkyNotes)
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

        #region  Select Mode Checkbox events //22Mar2016 check / uncheck left navigation tree checkboxes
        private void SelModeCheckbox_Checked(object sender, RoutedEventArgs e)
        {
            if (SelModeTxt != null)
            {
                SelModeTxt.Text = "All";
                ToggleNavTreeCheckboxes("All", NavTree.Items);// select all checkboxes
            }
        }

        private void SelModeCheckbox_Unchecked(object sender, RoutedEventArgs e)
        {
            if (SelModeTxt != null)
            {
                SelModeTxt.Text = "None";
                ToggleNavTreeCheckboxes("None", NavTree.Items);// select none ( clear all checkboxes)
            }
        }

        private void SelModeCheckbox_Indeterminate(object sender, RoutedEventArgs e)
        {
            if (SelModeTxt != null)
            {
                SelModeTxt.Text = "Default";
                ToggleNavTreeCheckboxes("Default", NavTree.Items);// select all but BSkyNote, which is BSky default setting
            }
        }
        #endregion

        #endregion

        #endregion

        /// Bring this window on Top///
        public void BringOnTop()
        {
            if (this.WindowState == System.Windows.WindowState.Minimized)
                this.WindowState = System.Windows.WindowState.Normal;

            this.Activate();//bring it to front
        }

        private void MenuItemClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }


        #region Syntax Window Events and other related stuff

        string syntitle = BSky.GlobalResources.Properties.UICtrlResources.CommandEditorPanelTitle + " - ";
        bool SEForceClose = false;
        bool Modified = false; //19Feb2013 to track if any modification has been done after last save

        private void SyntaxtInit() //syntax init
        {

        }

        public bool SynEdtForceClose
        {
            get { return SEForceClose; }
            set { SEForceClose = value; }
        }

		public bool isRunFromSyntaxEditor { get; set; }

        private void runButton_Click(object sender, RoutedEventArgs e)
        {
			isRunFromSyntaxEditor = true;
            BSkyMouseBusyHandler.ShowMouseBusy();// ShowHideBusy_old(true);
            ////// Start Syntax Editor  //////
            sewindow = LifetimeService.Instance.Container.Resolve<SyntaxEditorWindow>();
            int totallines = inputTextbox.Lines.Count;
            int lno=0;
            string commands = inputTextbox.SelectedText;//selected text
            try
            {
                if (commands != null && commands.Length > 0)
                {
                    //one of these two is at the starting and other at the end of the selection
					if (inputTextbox.CurrentPosition > inputTextbox.RectangularSelectionAnchor) 
                    {
                        inputTextbox.GotoPosition(inputTextbox.CurrentPosition);
                    }
                    else
                    {
                        inputTextbox.GotoPosition(inputTextbox.RectangularSelectionAnchor);
                    }

                    lno = inputTextbox.CurrentLine;					
                }
                else
                {
                    lno = inputTextbox.CurrentLine;
                    commands = inputTextbox.Lines[lno].Text; //Current line
                }
                if (commands.Trim().Length > 0)
                {
                    sewindow.RunCommands(commands);
                    sewindow.DisplayAllSessionOutput("", this);
                }
            }
            catch (Exception ex)
            {
                logService.WriteToLogLevel("Exeception:" + ex.Message, LogLevelEnum.Error);
            }
            finally
            {
                BSkyMouseBusyHandler.HideMouseBusy();// ShowHideBusy_old(false);
				isRunFromSyntaxEditor = false;
            }
            //bring focus back and move cursor to the next line
            inputTextbox.Focus();
            int currentpos = 0;
            int linelen = 0;
            int linecol = 0;
			int endofcurline = 0;
            int nextlinestartpos = 0;
            for(; lno < totallines;)
            {
                endofcurline = inputTextbox.Lines[lno].EndPosition; //this includes \n\r
                nextlinestartpos = endofcurline;
                inputTextbox.GotoPosition(nextlinestartpos);
                if (lno == inputTextbox.CurrentLine)//if cursor is not advancing to the next line number(still on old line) then it's end of text
                {
                    break;
                }
                lno = inputTextbox.CurrentLine;
                if (  (inputTextbox.Lines[lno].Text.Trim().Length > 0) && !(inputTextbox.Lines[lno].Text.Trim().StartsWith("#"))  )
                {
                    break;
                }
            }			
        }

        private void browse_Click(object sender, RoutedEventArgs e)
        {
            string FileNameFilter = "BSky Format, that can be opened in Output Window later (*.bsoz)|*.bsoz|Comma Seperated (*.csv)|*.csv|HTML (*.html)|*.html"; //BSkyOutput
            Microsoft.Win32.SaveFileDialog saveasFileDialog = new Microsoft.Win32.SaveFileDialog();
            saveasFileDialog.Filter = FileNameFilter;

            bool? output = saveasFileDialog.ShowDialog(System.Windows.Application.Current.MainWindow);
            if (output.HasValue && output.Value)
            {
                try
                {
                    if (File.Exists(saveasFileDialog.FileName))
                    {
                        File.Delete(saveasFileDialog.FileName);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, ex.Message);
                    logService.WriteToLogLevel(ex.Message, LogLevelEnum.Error);
                    return;
                }
            }
        }

        public void PasteSyntax(string command)//29Jan2013
        {
            if (isSyntaxCollapsed)
            {
                CollapseExpandSyntax();
            }
            int oldlinecount = inputTextbox.Lines.Count;// LineCount;// -1;
            int charCountOfOldText = inputTextbox.Text.Length;

            string newlines = (inputTextbox.Text != null && inputTextbox.Text.Trim().Length > 0) ? "\n" : string.Empty;
            if (command != null && command.Length > 0)
                inputTextbox.AppendText(newlines + command);
            int linecount = inputTextbox.Lines.Count - 1;
            inputTextbox.LineScroll(linecount, 0);

            inputTextbox.GotoPosition(inputTextbox.Text.Length);

            inputTextbox.SetSelection(charCountOfOldText + 1, inputTextbox.Text.Length);

        }

        //New : clears the command area
        private void MenuItemNew_Click(object sender, RoutedEventArgs e)
        {
            if (Modified)//if any modification done to command scripts after last Save
            {
                CloseCurrentScript();
            }
            else
            {
                ResetValues();
            }

        }

        private void MenuItemOpen_Click(object sender, RoutedEventArgs e)
        {
            SyntaxEditorOpen();
            inputTextbox.Focus();// this.Activate();
        }

        //26May2015
        private void MenuItemSave_Click(object sender, RoutedEventArgs e)
        {
            SyntaxEditorSave();
            inputTextbox.Focus();
        }

        private void MenuItemSaveAs_Click(object sender, RoutedEventArgs e)
        {
            SyntaxEditorSaveAs();
            inputTextbox.Focus();// this.Activate();
        }

        private void SyntaxEditorOpen()
        {
            bool isClosed = true;
            if (Modified)//if any modification done to command scripts after last Save
            {
                isClosed = CloseCurrentScript();
            }

            if (isClosed)//if current doc is closed finally then ask for opening another
            {
                const string FileNameFilter = "BSky R scripts (*.r)|*.r|Text (*.txt)|*.txt"; //BSkyR
                Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
                openFileDialog.Filter = FileNameFilter;
                bool? output = openFileDialog.ShowDialog(System.Windows.Application.Current.MainWindow);
                if (output.HasValue && output.Value)
                {
                    string extension = Path.GetExtension(openFileDialog.FileName);
                    if (extension.ToLower().Equals(".r") || extension.ToLower().Equals(".txt"))
                    {
                        using (System.IO.StreamReader file = new System.IO.StreamReader(openFileDialog.FileName))
                        {
                            inputTextbox.Text = file.ReadToEnd();
                            file.Close();
                            currentScriptFname = openFileDialog.FileName;//26May2015
                            recentSyntaxfiles.AddXMLItem(openFileDialog.FileName);//19May2015
                        }
                    }
                    else
                    {
                        MessageBox.Show(BSky.GlobalResources.Properties.UICtrlResources.REditorFileTypeSupportedMsg);
                    }
                }
                Modified = false;//19Feb2013 Newly loaded script can only be modified after loading finishes.
                SyntaxTitle.Text = syntitle + openFileDialog.FileName; //19Feb2013
            }
        }

        private bool CloseCurrentScript()
        {
            bool isClosed = false;
            System.Windows.Forms.DialogResult dresult = System.Windows.Forms.MessageBox.Show(
                      BSky.GlobalResources.Properties.UICtrlResources.REditorSaveScriptPrompt,
                      BSky.GlobalResources.Properties.UICtrlResources.REditorSavePromptMsgBoxTitle,
                      System.Windows.Forms.MessageBoxButtons.YesNoCancel,
                      System.Windows.Forms.MessageBoxIcon.Question);
            if (dresult == System.Windows.Forms.DialogResult.Yes)//Yes Save- and Close
            {
                bool isSaved;
                if (currentScriptFname != null && currentScriptFname.Length > 0)
                    isSaved = SyntaxEditorSave();
                else
                    isSaved = SyntaxEditorSaveAs();

                if (isSaved) // reset values if script saved.
                {
                    ResetValues();
                    isClosed = true;//close current script
                }

            }
            else if (dresult == System.Windows.Forms.DialogResult.No)//No Save- but Close
            {
                ResetValues();
                isClosed = true;
            }
            else//no Save no Close
            {
            }
            return isClosed;
        }

        string currentScriptFname = null;
        private bool SyntaxEditorSaveAs()
        {
            bool isSaved = false; //not saved.

            const string FileNameFilter = "BSky R scripts, (*.r)|*.r"; //BSkyR. Extension is changed to .r

            SaveFileDialog saveasFileDialog = new SaveFileDialog();
            saveasFileDialog.Filter = FileNameFilter;
            bool? output = saveasFileDialog.ShowDialog(Application.Current.MainWindow);
            if (output.HasValue && output.Value)
            {
                currentScriptFname = saveasFileDialog.FileName;//26May2015
                SaveScript(currentScriptFname);
                isSaved = true; //saved
            }
            return isSaved;
        }

        ////26May2015 Saves to current file.
        private bool SyntaxEditorSave()
        {
            bool isSaved = false;//not saved
            if (currentScriptFname == null || currentScriptFname.Trim().Length < 1)
            {
                const string FileNameFilter = "BSky R scripts, (*.r)|*.r"; //BSkyR. Extension is changed to .r

                SaveFileDialog saveasFileDialog = new SaveFileDialog();
                saveasFileDialog.Filter = FileNameFilter;
                bool? output = saveasFileDialog.ShowDialog(Application.Current.MainWindow);
                if (output.HasValue && output.Value)
                {
                    currentScriptFname = saveasFileDialog.FileName;
                    SaveScript(currentScriptFname);
                    isSaved = true;//saved
                }
            }
            else
            {
                SaveScript(currentScriptFname);
                isSaved = true;
            }
            return isSaved;
        }

        ////26May2015 save current script to a file.
        private void SaveScript(string fullpathfilename)
        {
            System.IO.StreamWriter file = new System.IO.StreamWriter(fullpathfilename);
            file.WriteLine(inputTextbox.Text);
            file.Close();

            Modified = false;
            SyntaxTitle.Text = syntitle + currentScriptFname; //19Feb2013
        }

        //26May2015 Reset vars and control values
        private void ResetValues()
        {
            currentScriptFname = string.Empty;//26May2015
            SyntaxTitle.Text = syntitle; //26May2015
            inputTextbox.Text = string.Empty;
            inputTextbox.Focus();// this.Activate();
        }

        //Not in use by Scintilla 
        private void inputTextbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (currentScriptFname == null)
                currentScriptFname = string.Empty;
            Modified = true;
            SyntaxTitle.Text = syntitle + " " + currentScriptFname + " < unsaved script >";
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            BSkyMouseBusyHandler.ShowMouseBusy();// ShowHideBusy_old(true);
            ////// Start Syntax Editor  //////
            SyntaxEditorWindow sewindow = LifetimeService.Instance.Container.Resolve<SyntaxEditorWindow>();
            sewindow.RefreshBothgrids();//16Jul2015 refreshes both the grids
            BSkyMouseBusyHandler.HideMouseBusy();// ShowHideBusy_old(false);
        }

        public void ActivateSyntax()
        {
            this.Activate(); //16Aug2016 bring outpuwindow in front after PASTE from dialog
            inputTextbox.Focus();
        }

        private bool SynWindow_Closing()
        {
            System.Windows.Forms.DialogResult dresult = System.Windows.Forms.DialogResult.OK;
            if (Modified)
            {
                dresult = System.Windows.Forms.MessageBox.Show(
                          BSky.GlobalResources.Properties.UICtrlResources.REditorCloseSaveScriptPrompt,
                          BSky.GlobalResources.Properties.UICtrlResources.REditorSavePromptMsgBoxTitle,
                          System.Windows.Forms.MessageBoxButtons.YesNoCancel,
                          System.Windows.Forms.MessageBoxIcon.Question);
                if (dresult == System.Windows.Forms.DialogResult.Cancel)//dont close
                {
                    return false; //abort closing
                }
                else 
                {
                    ///before closing save R scripts in Syntax Editor text area..13Feb2013
                    if (dresult == System.Windows.Forms.DialogResult.Yes)//Save
                        SyntaxEditorSaveAs();
                }

            }
            return true;
        }

        #region Find Replace
        FindReplaceWindow frw = null;
        private void findreplace_Click(object sender, RoutedEventArgs e)
        {
            //Check if user already selected some text, if so then, in Find-Replace dialog CHECK the 'in-selection' option
            // otherwise keep it UNCHECKED
            bool InSelectedChecked = false;
            string selectedtext = inputTextbox.SelectedText;//text from the block selected
            if (selectedtext != null && selectedtext.Trim().Length > 0)
            {
                InSelectedChecked = true;
            }


            if (frw == null)
            {
                frw = new FindReplaceWindow(this, InSelectedChecked);
                frw.Owner = this;
                frw.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner;
            }
            frw.Show();
        }

        int currentidx = 0;
        StringBuilder sbfultxt = null; // initialized in constructor
        string findtext = string.Empty;
        public bool FindText(string texttofind, SearchFlags sf, bool calledFromReplace = false)
        {
            int pos = FindNext(texttofind, sf);
            if (pos >= 0)
                return true; //found
            else
                return false; //not found

        }

        public int FindNext(string text, SearchFlags searchFlags)
        {

            inputTextbox.SearchFlags = searchFlags;
            inputTextbox.TargetStart = Math.Max(inputTextbox.CurrentPosition, inputTextbox.AnchorPosition);
            inputTextbox.TargetEnd = inputTextbox.TextLength;

            var pos = inputTextbox.SearchInTarget(text);
            if (pos >= 0)
                inputTextbox.SetSel(inputTextbox.TargetStart, inputTextbox.TargetEnd);
            else
            {
                MessageBox.Show(BSky.GlobalResources.Properties.UICtrlResources.REditorFindNextEndsMsg,
                    BSky.GlobalResources.Properties.UICtrlResources.REditorFindNextEndsMsgbxTitle, MessageBoxButton.OK, MessageBoxImage.Asterisk);
                inputTextbox.GotoPosition(0);
            }

            return pos;
        }

        public bool ReplaceWith(string texttofind, string replacewith, SearchFlags sf)
        {
            string selectedtext = inputTextbox.SelectedText;
            bool doReplace = selectedtext.Equals(texttofind, StringComparison.InvariantCultureIgnoreCase);

            if (doReplace)//pos >= 0 && 
                inputTextbox.ReplaceSelection(replacewith);

            // Find next occurrence of the text. // and, if found, replace the selection
            var pos = FindNext(texttofind, sf);
            if (pos < 0)
                inputTextbox.GotoPosition(0);

            if (pos >= 0) return true;
            else return false;
        }


        //This method will replace all the occurrences of a matching word with another word
        public void ReplaceAllInSelectedTextBlock(string texttofind, string replacewith, SearchFlags sf)
        {
            string selectedtext = inputTextbox.SelectedText;//text from the block selected
            if (selectedtext == null || selectedtext.Length < 1)
            {
                MessageBox.Show(frw,
                    BSky.GlobalResources.Properties.UICtrlResources.REditorReplaceAllInSelectedMsg,
                    BSky.GlobalResources.Properties.UICtrlResources.REditorReplaceAllInSelectedMsgboxTitle,
                    MessageBoxButton.OK, MessageBoxImage.Hand);
            }
            else
            {
                StringComparison sc;
                if (sf == SearchFlags.MatchCase)
                {
                    sc = StringComparison.InvariantCulture; //match case
                }
                else
                {
                    sc = StringComparison.InvariantCultureIgnoreCase; //ignore case
                }

                string resultText = Replace1(selectedtext, texttofind, replacewith, sc);
                inputTextbox.ReplaceSelection(resultText);//replacing whole selected text with new one.
            }
        }

        //From net. Replaces all occerrences of old text with new with match-case or ignore-case
        public string Replace1(string selBlockText, string texttofind, string replacewith, StringComparison strcomp)
        {
            int startIdx = 0;
            while (true)
            {
                startIdx = selBlockText.IndexOf(texttofind, startIdx, strcomp);
                if (startIdx == -1)
                    break;

                selBlockText = selBlockText.Substring(0, startIdx) + replacewith + selBlockText.Substring(startIdx + texttofind.Length);

                startIdx += replacewith.Length;
            }

            return selBlockText;
        }


        public void CloseFindReplace()
        {
            frw = null;
        }
        #endregion

        #region refresh recent file list 21feb2013

        private void initRecentSyntaxFileHandler()
        {
            recentSyntaxfiles = new RecentDocs();//21Feb2013
            recentSyntaxfiles.MaxRecentItems = 7;
            recentSyntaxfiles.XMLFilename = string.Format(@"{0}SyntaxRecent.xml", BSkyAppData.RoamingUserBSkyConfigL18nPath);
            recentSyntaxfiles.recentitemclick = SyntaxRecentItem_Click;
            RefreshRecent();//
        }

        public void RefreshRecent()
        {
            MenuItem recent = GetMenuItemByHeaderPath("Synfile>Synrecent");//11Oct2017 ("_File>Recent"); 
            try
            {
                recentSyntaxfiles.RecentMI = recent;
            }
            catch (Exception ex)//17Jan2014
            {
                MessageBox.Show(BSky.GlobalResources.Properties.UICtrlResources.SynRecentXmlNotFound);
                logService.WriteToLogLevel("SyntaxRecent.xml not found.\n" + ex.StackTrace, LogLevelEnum.Fatal);
            }
        }

        private MenuItem GetMenuItemByHeaderPath(string headerpath)
        {
            MenuItem mi = null;
            string[] patharr = headerpath.Split('>');// File, Open

            ///search MenuItem by searching Header
            foreach (string itm in patharr)
            {
                mi = FindItemInBranch(mi, itm);
            }

            return mi;
        }

        ///Find Item travesing thru a selected branch // this method will work with above funtion 'GetMenuByHeaderPath'
        private MenuItem FindItemInBranch(MenuItem ParentItem, string ChildHeader) //eg.. in 'File' look for 'Open'
        {
            MenuItem mi = null;
            if (ParentItem == null)
            {
                foreach (MenuItem itm in SMenu.Items)
                {
                    if (itm.Name.ToString().Equals(ChildHeader))// Header is replaced by Name. Header will change when user lang changes
                    {
                        mi = itm;
                        break;
                    }
                }
            }
            else
            {
                foreach (object oitm in ParentItem.Items)
                {
                    var casted = oitm as MenuItem;//if cast is possible or not
                    if (casted != null)
                    {
                        MenuItem itm = oitm as MenuItem;
                        if (itm.Name.ToString().Equals(ChildHeader))// Header is replaced by Name. Header will change when user lang changes
                        {
                            mi = itm;
                            break;
                        }
                    }
                }
            }
            return mi;
        }

        private void SyntaxRecentItem_Click(string fullpathfilename)
        {
            if (System.IO.File.Exists(fullpathfilename))
            {
                System.IO.StreamReader file = new System.IO.StreamReader(fullpathfilename);
                inputTextbox.Text = file.ReadToEnd();
                file.Close();
                Modified = false;//19Feb2013 Newly loaded script can only be modified after loading finishes.
                currentScriptFname = fullpathfilename;
                SyntaxTitle.Text = syntitle + fullpathfilename; //19Feb2013
            }
            else
            {
                MessageBox.Show(fullpathfilename + " " + BSky.GlobalResources.Properties.UICtrlResources.FileRecentRScriptNotFound,
                    BSky.GlobalResources.Properties.UICtrlResources.RecentScriptNotFoundMsgBoxTitle, MessageBoxButton.OK, MessageBoxImage.Warning);
                //If file does not exist. It should be removed from the recent files list.
                recentSyntaxfiles.RemoveXMLItem(fullpathfilename);
            }
        }

        #endregion

        #endregion

        #region Split orientation handler
        bool isHorizontal = false;

        private void flip_Click(object sender, RoutedEventArgs e)
        {
            MenuItem mi = sender as MenuItem;
            if (mi.Name.Trim().Equals("vertical"))
            {
                toVerticalSplit();
                horizontalsplit.Visibility = System.Windows.Visibility.Collapsed;
                verticalsplit.Visibility = System.Windows.Visibility.Visible;
                isHorizontal = false;
                vertical.IsEnabled = false;
                horizontal.IsEnabled = true;
            }
            else if (mi.Name.Trim().Equals("horizontal"))
            {
                toHorizontalSplit();
                horizontalsplit.Visibility = System.Windows.Visibility.Visible;
                verticalsplit.Visibility = System.Windows.Visibility.Collapsed;
                isHorizontal = true;
                vertical.IsEnabled = true;
                horizontal.IsEnabled = false;
            }
            else
            {
                return;
            }


            e.Handled = true;
        }

        private void toVerticalSplit()
        {
            double winwidth = this.Width;
            double outpanelWidth = (.7 * winwidth); 
            rightmost.Width = new GridLength(.55, GridUnitType.Star);//star
            leftmost.Width = new GridLength(1, GridUnitType.Star);
            top.Height = new GridLength(1, GridUnitType.Star);
            bottom.Height = new GridLength(0, GridUnitType.Pixel);
            rightmost.MinWidth = 25;


            Grid.SetRow(syntaxgrid, 0);
            Grid.SetColumn(syntaxgrid, 1);
            //try adding col and row span
            syntaxgrid.SetValue(Grid.ColumnSpanProperty, 1);
            syntaxgrid.SetValue(Grid.RowSpanProperty, 2);
            outputgrid.SetValue(Grid.ColumnSpanProperty, 1);
            outputgrid.SetValue(Grid.RowSpanProperty, 2);

        }

        private void toHorizontalSplit()
        {
            rightmost.Width = new GridLength(0, GridUnitType.Pixel);
            leftmost.Width = new GridLength(1, GridUnitType.Star);
            top.Height = new GridLength(1, GridUnitType.Star);
            bottom.Height = new GridLength(.4, GridUnitType.Star);//star
            bottom.MinHeight = 25;


            Grid.SetRow(syntaxgrid, 1);
            Grid.SetColumn(syntaxgrid, 0);
            //try adding col and row span
            syntaxgrid.SetValue(Grid.ColumnSpanProperty, 2);
            syntaxgrid.SetValue(Grid.RowSpanProperty, 1);
            outputgrid.SetValue(Grid.ColumnSpanProperty, 2);
            outputgrid.SetValue(Grid.RowSpanProperty, 1);

        }
        #endregion

        #region Collapse Syntax

        double oldbottomheight;
        double oldrightmost;
        double oldleftmost;
        bool isSyntaxCollapsed = false;

        private void CollapseExpandSyntax()
        {
			int collapsedWidthHeight = 0;// 25;								   
            //is horizontal rowdefsyntax
            if (isHorizontal)
            {
                if (syntaxgrid.ActualHeight < 40) isSyntaxCollapsed = true; else isSyntaxCollapsed = false;
                if (!isSyntaxCollapsed)
                {
                    oldbottomheight = bottom.ActualHeight;//store old height

                    bottom.Height = new GridLength(collapsedWidthHeight, GridUnitType.Pixel);//star
                    bottom.MinHeight = collapsedWidthHeight;//25;
                    isSyntaxCollapsed = true;
                }
                else
                {
                    top.Height = new GridLength(1, GridUnitType.Star);//star
                    bottom.Height = new GridLength(oldbottomheight, GridUnitType.Pixel);//star
                    isSyntaxCollapsed = false;
                }
            }
            else //is vertical
            {
                if (syntaxgrid.ActualWidth < 40) isSyntaxCollapsed = true; else isSyntaxCollapsed = false;

                if (!isSyntaxCollapsed)
                {
                    rightmost.MinWidth = collapsedWidthHeight;//25;
                    oldrightmost = rightmost.ActualWidth;
                    oldleftmost = leftmost.ActualWidth;
                    rightmost.Width = new GridLength(collapsedWidthHeight, GridUnitType.Pixel);
                    leftmost.Width = new GridLength(1, GridUnitType.Star);
                    isSyntaxCollapsed = true;
                }
                else
                {
                    rightmost.Width = new GridLength(oldrightmost, GridUnitType.Pixel);
                    leftmost.Width = new GridLength(1, GridUnitType.Star);
                    isSyntaxCollapsed = false;
                }
            }
        }

        private void CollapseSyntax()
        {
            int collapsedWidthHeight = 0;// 25;	
            rightmost.MinWidth = collapsedWidthHeight;//25;
            oldrightmost = rightmost.ActualWidth;
            oldleftmost = leftmost.ActualWidth;
            rightmost.Width = new GridLength(collapsedWidthHeight, GridUnitType.Pixel);
            leftmost.Width = new GridLength(1, GridUnitType.Star);
            isSyntaxCollapsed = true;
        }

        private void Image_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            CollapseExpandSyntax();
        }


        #endregion

        #region Navigation tree Show/hide
        private bool navtreehidden = true;
        private void navtreemi_Click(object sender, RoutedEventArgs e)
        {
            if (navtreehidden)
            {
                navtreecol.Width = new GridLength(150, GridUnitType.Pixel);
                navtreecol.MinWidth = 10;
                navtreemi.Header = BSky.GlobalResources.Properties.UICtrlResources.HideNavTreeText;
                navtreehidden = false;
            }
            else
            {
                navtreecol.MinWidth = 0;
                navtreecol.Width = new GridLength(0, GridUnitType.Pixel);

                navtreemi.Header = BSky.GlobalResources.Properties.UICtrlResources.ShowNavTreeText;
                navtreehidden = true;
            }
        }
        #endregion

        #region Change Graphic Image Size
        private void changeimagesize_Click(object sender, RoutedEventArgs e)
        {
            //Dimension keys
            string wdkey = "imagewidth";
            string htkey = "imageheight";

            //Try get current values so as to set them in the dialog fields
            string currwd = confService.GetConfigValueForKey(wdkey);
            string currht = confService.GetConfigValueForKey(htkey);

            //Launch Image size change dialog and read user's values
            ChangeImageSizeDialog cisd = new ChangeImageSizeDialog();
            cisd.Owner = this;
            cisd.imgwidthtxt.Text = currwd;
            cisd.imgheighttxt.Text = currht;
            cisd.ShowDialog();
            string usrht = cisd.ImgHeight;
            string usrwd = cisd.ImgWidth;

            int outresult; //only used for checking
            if (!int.TryParse(usrwd, out outresult)) //if user entered non-numeric then user the current width
            {
                usrwd = currwd;
            }
            if (!int.TryParse(usrht, out outresult)) //if user entered non-numeric then user the current width
            {
                usrht = currht;
            }

            //Set new Dimensions
            confService.ModifyConfig(wdkey, usrwd);
            confService.ModifyConfig(htkey, usrht);

            confService.RefreshConfig();
            SyntaxEditorWindow sewindow = LifetimeService.Instance.Container.Resolve<SyntaxEditorWindow>();
            sewindow.RefreshImgSizeForGraphicDevice();
        }

        private void defaulimagesize_Click(object sender, RoutedEventArgs e)
        {
            //Dimension keys
            string wdkey = "imagewidth";
            string htkey = "imageheight";
            string defwd = "600";
            string defht = "600";
            //Set new Dimensions
            confService.ModifyConfig(wdkey, defwd);
            confService.ModifyConfig(htkey, defht);

            confService.RefreshConfig();
            SyntaxEditorWindow sewindow = LifetimeService.Instance.Container.Resolve<SyntaxEditorWindow>();
            sewindow.RefreshImgSizeForGraphicDevice();

            //12Jan2018
            MessageBox.Show(BSky.GlobalResources.Properties.UICtrlResources.ImgDefaultDimSetMsg,
                BSky.GlobalResources.Properties.UICtrlResources.ImgDefaultDimSetMsgTitle,
                 MessageBoxButton.OK, MessageBoxImage.Information);
        }
        #endregion

        #region Window Loaded  & Maximised/Restore
        private void outwin_SizeChanged(object sender, SizeChangedEventArgs e)
        {

        }

        private void outwin_Loaded(object sender, RoutedEventArgs e)
        {
            oldrightmost = this.Width * .4;
            oldbottomheight = this.Height * .4;
        }

        #endregion


        //01Oct2015 If mouse is busy all the clicks are ignored. Same thing should be done to Main window.
        private void outwin_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (System.Windows.Input.Mouse.OverrideCursor == System.Windows.Input.Cursors.Wait)//29Sep2015 disable if mouse is busy
                e.Handled = true;
            else
                e.Handled = false;
        }

        #region Syntax Clipboard Events
        private void cut_button_Click(object sender, RoutedEventArgs e)
        {
            inputTextbox.Cut();
        }

        private void copy_button_Click(object sender, RoutedEventArgs e)
        {
            inputTextbox.Copy();
        }

        private void paste_button_Click(object sender, RoutedEventArgs e)
        {
            inputTextbox.Paste();
        }

        private void undo_button_Click(object sender, RoutedEventArgs e)
        {
            inputTextbox.Undo();
        }

        private void redo_button_Click(object sender, RoutedEventArgs e)
        {
            inputTextbox.Redo();
        }

        #endregion

        #region Show/Hide Busy :

        Cursor defaultcursor;
        private void ShowHideBusy_old(bool makebusy)
        {
            if (makebusy)
            {
                defaultcursor = Mouse.OverrideCursor;
                Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
            }
            else
            {
                Mouse.OverrideCursor = null;
            }
            ScintillaMouseBusyShowHide(makebusy);
        }

        //Mouse busy for Scintilla(Windows Form)
        public void ScintillaMouseBusyShowHide(bool makebusy)
        {
            if (makebusy)
            {
                windowsFormsHost1.IsEnabled = false;
            }
            else
            {
                windowsFormsHost1.IsEnabled = true;
            }
        }

        #endregion

        // brings Datagrid (Main) window in front
        private void gotoMainWindow_button_Click(object sender, RoutedEventArgs e)
        {
            Window1 window = LifetimeService.Instance.Container.Resolve<Window1>();
            if (window.WindowState == System.Windows.WindowState.Minimized)
                window.WindowState = System.Windows.WindowState.Normal;
            window.Activate();
        }

        private void SyntaxWinShowHideBtn_Click(object sender, RoutedEventArgs e)
        {
            if (isSyntaxCollapsed)
            {
                isSyntaxCollapsed = false;
                expandImg.Visibility = Visibility.Collapsed;
                collapseImg.Visibility = Visibility.Visible;
            }
            else
            {
                isSyntaxCollapsed = true;
                expandImg.Visibility = Visibility.Visible;
                collapseImg.Visibility = Visibility.Collapsed;
            }
            CollapseExpandSyntax();
        }
		
        private void mypanel_ContextMenuClosing(object sender, ContextMenuEventArgs e)
        {
            //MessageBox.Show("Context Menu is closing");
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
                TreeViewItem tvi = fe.Tag as TreeViewItem;
                TreeViewItem parent = tvi.Parent as TreeViewItem;

                int pidx = NavTree.Items.IndexOf(parent);//this index will be same for the datalist
                int leafidx = parent.Items.IndexOf(tvi);

                parent.Items.Remove(tvi);
                if (parent.Items.Count == 0)
                {
                    NavTree.Items.Remove(parent);
                }

                mypanel.Children.Remove(fe);

                //remove from datalist too 
                AnalyticsData ad = outputDataList[pidx];
                if (aup != null || bsn != null || bsgc != null || auxg != null)
                {
                    if (ad.SessionOutput != null && ad.SessionOutput.Count > 0)
                    {
                        //ad.SessionOutput[0] as CommandOutput)[0]
                        CommandOutput co = null;//for traversing and holding the reference of the element to be deleted

                        //Loop thru SessionOutput to find the right element to be deleted. leafidx is not accurate
                        int session_leafidx = -1;
                        bool found = false;
                        for (int j = 0; j < ad.SessionOutput.Count; j++)//parent in sessionoutput
                        {
                            co = (ad.SessionOutput[j] as CommandOutput);
                            for (int k = 0; k < co.Count; k++)//leaf in sessionoutput
                            {
                                session_leafidx++;
                                if (leafidx == session_leafidx)//this is the element we need to delete
                                {
                                    found = true;
                                    //del_co = co[k] as CommandOutput;
                                    break; //co has the element that is to be deleted
                                }
                            }
                            if (found) break;
                        }

                        if (found)
                        {
                            //CommandOutput co = (ad.SessionOutput[leafidx] as CommandOutput);
                            if (!co.Remove(fe))
                            {
                                MessageBox.Show("Can't remove from session");
                            }

                            //remove co if its empty
                            if (co.Count == 0)
                            {
                                ad.SessionOutput.Remove(co);

                                if (ad.SessionOutput.Count == 0)
                                {
                                    ad.SessionOutput = null;
                                }
                            }
                        }
                        else
                        {
                            MessageBox.Show("Could not locate the element for deletion");
                        }
                    }
                    else
                    {
                        if (ad.Output != null)
                        {
                            if (!ad.Output.Remove(fe))
                            {
                                MessageBox.Show("Can't remove from output");
                            }
                            //remove co if its empty
                            if (ad.Output.Count == 0)
                            {
                                //ad.Output.Remove(ad.Output);
                            }
                        }
                    }
                }
            }
        }

        private void Comingsoonbtn_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("www.blueskystatistics.com");
        }

        private void ThemeBtn_Click(object sender, RoutedEventArgs e)
        {
            ThemeWindow thwin = new ThemeWindow();
            thwin.Owner = this;
            thwin.ShowDialog();
        }	
	}
	
    public class PropertyDataTemplateSelector : DataTemplateSelector
    {
        public DataTemplate DefaultnDataTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item,
                   DependencyObject container)
        {
            AnalyticsData dpi = item as AnalyticsData;

            return DefaultnDataTemplate;
        }
    }

    public class Employee
    {
        public string name { get; set; }
        public int age { get; set; }
        public string city { get; set; }
    }

    public class TextWidthMultiBind : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            double buffer = 50;
            double wid = (double)values[0] - (double)values[1] - (double)values[2] - buffer;

            return wid;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }



}
