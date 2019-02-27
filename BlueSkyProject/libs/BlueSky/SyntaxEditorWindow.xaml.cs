using BlueSky.Commands;
using BlueSky.Commands.File;
using BlueSky.Commands.Output;
using BlueSky.Services;
using BSky.ConfigService.Services;
using BSky.ConfService.Intf.Interfaces;
using BSky.Controls;
using BSky.Controls.Controls;
using BSky.DynamicClassCreator;
using BSky.Interfaces.Commands;
using BSky.Interfaces.Interfaces;
using BSky.Interfaces.Services;
using BSky.Lifetime;
using BSky.Lifetime.Interfaces;
using BSky.OutputGenerator;
using BSky.Statistics.Common;
using BSky.Statistics.Service.Engine.Interfaces;
using C1.WPF.FlexGrid;
using Microsoft.Win32;
using RDotNet;
using ScintillaNET;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml;

namespace BlueSky
{
    public enum RCommandType { RCOMMAND, CONDITIONORLOOP, BSKYFORMAT, BSKYLOADREFRESHDATAFRAME, BSKYREMOVEREFRESHDATAFRAME, GRAPHIC, GRAPHICXML, SPLIT, REFRESHGRID, RDOTNET }
    /// <summary>
    /// Interaction logic for SyntaxEditorWindow.xaml
    /// </summary>
    public partial class SyntaxEditorWindow : Window
    {
        IAnalyticsService analytics = LifetimeService.Instance.Container.Resolve<IAnalyticsService>();
        IConfigService confService = LifetimeService.Instance.Container.Resolve<IConfigService>();//23nov2012
        ILoggerService logService = LifetimeService.Instance.Container.Resolve<ILoggerService>();//13Dec2012

        bool AdvancedLogging;

        CommandRequest sinkcmd = new CommandRequest();
        C1.WPF.FlexGrid.FileFormat fileformat = C1.WPF.FlexGrid.FileFormat.Html;
        bool extratags = true; // true means default file type will be .BSO
        string fullfilepathname = string.Empty;

        Dictionary<string, ImageDimensions> registeredGraphicsList = new Dictionary<string, ImageDimensions>();//28May2015

        OutputMenuHandler omh = new OutputMenuHandler();//Output menu
        bool SEForceClose = false;//05Feb2013
        bool Modified = false; //19Feb2013 to track if any modification has been done after last save
        bool bsky_no_row_header; //14Jul2014 for supressing the default rowheaders "1","2","3" 
        long EMPTYIMAGESIZE = 318;// bytes
        int _currentGraphicWidth = 600;//current width of the image
        int _currentGraphicHeight = 600;//current height of the image

        string doubleClickedFilename;//17May2013
        public string DoubleClickedFilename
        {
            get;
            set;
        }

        BSkyDialogProperties DlgProp; // for storing dialog properties.
        FrameworkElement felement;
        public FrameworkElement FElement //for some commands it is important to set this property
        {
            get { return felement; }
            set { felement = value; }
        }

        object menuParameter;
        public object MenuParameter
        {
            get { return menuParameter; }
            set { menuParameter = value; }
        }
        //15Nov2013 for storing all commands those got executed when RUN button was cliced once
        SessionOutput sessionlst;
        OutputWindow ow;

        //07Nov2014 To Get SessionList items count at any point.
        public int SesssionListItemCount
        {
            get { return sessionlst.Count; }
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
            inputTextbox.Margins[0].Width = 16;
        }

        #region Scintilla Configuration Methods

        private void PrepareKeywords()
        {
            Keywords1 = @"commandArgs detach length dev.off stop lm library predict lmer 
           plot print display anova read.table read.csv complete.cases dim attach as.numeric seq max 
           min data.frame lines curve as.integer levels nlevels ceiling sqrt ranef order
           AIC summary str head png tryCatch par mfrow interaction.plot qqnorm qqline".Split(new char[] { ' ', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).ToList();

            Keywords2 = @"TRUE FALSE if else for while in break continue function".Split(new char[] { ' ', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).ToList();

            List<string> keywords = Keywords1.ToList();
            keywords.AddRange(Keywords2);
            keywords.Sort();

            AutoCompleteKeywords = string.Join(" ", keywords);
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

            inputTextbox.SetKeywords(0, string.Join(" ", Keywords1));
            inputTextbox.SetKeywords(1, string.Join(" ", Keywords2));
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
            var wordStartPos = scintilla.WordStartPosition(currentPos, true);

            // Display the autocompletion list
            var lenEntered = currentPos - wordStartPos;

            if (lenEntered > 0)
            {
                scintilla.AutoCShow(lenEntered, AutoCompleteKeywords);
            }
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

        #endregion

        #endregion

        public SyntaxEditorWindow()
        {
            InitializeComponent();
            inputTextbox = windowsFormsHost1.Child as Scintilla;
            inputTextbox.Text = "";
            ConfigureScintilla();
            this.MinWidth = 440;// 384;
            this.MinHeight = 200;
            this.Width = 750;// 976;
            this.Height = 550;
            this.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
            SMenu.Items.Add(omh.OutputMenu);///Add Output menu ///
            inputTextbox.Focus();//set focus inside text box.
            //opening Graphics device with sytax Editor. //05May2013
            OpenGraphicsDevice();
            sessionlst = new SessionOutput();
        }

        //05Feb2013 For controlling forcefull close of Syn Edt.
        public bool SynEdtForceClose
        {
            get { return SEForceClose; }
            set { SEForceClose = value; }
        }

        private void runButton_Click(object sender, RoutedEventArgs e)
        {
            /////Selected or all commands from textbox////30Apr2013
            string commands = inputTextbox.SelectedText;//selected text

            if (commands != null && commands.Length > 0)
            {
                //MessageBox.Show(seltext);
            }
            else
            {
                commands = inputTextbox.Text;//All text
                //MessageBox.Show(seltext);
            }
            if (commands.Trim().Length > 0)
            {
                RunCommands(commands);
                DisplayAllSessionOutput();//22Nov2013
            }
        }

        public void RunCommands(string commands, BSkyDialogProperties dlgprop = null, string fname="") //30Apr2013
        {
            try
            {
                AdvancedLogging = AdvancedLoggingService.AdvLog;//01May2015
                logService.WriteToLogLevel("Adv Log Flag:" + AdvancedLogging.ToString(), LogLevelEnum.Info);

                DlgProp = dlgprop;

                #region Load registered graphic commands from GraphicCommandList.txt 18Sep2012
                // loads each time run is clicked. Performance will be effected.
                string grplstfullfilepath = string.Format(@"{0}GraphicCommandList.txt", BSkyAppData.RoamingUserBSkyConfigPath);
				
                //if graphic file does not exist the n create one.
                if (!IsValidFullPathFilename(grplstfullfilepath, true))//17Jan2014
                {
                    string text = "plot";
                    System.IO.File.WriteAllText(@grplstfullfilepath, text);
                }

                // load default value if no path is set or invalid path is set
                if (grplstfullfilepath.Trim().Length == 0 || !IsValidFullPathFilename(grplstfullfilepath, true))
                {
                    MessageBox.Show(this, BSky.GlobalResources.Properties.Resources.sinkregstrdgrphConfigKeyNotFound);
                }
                else
                {
                    LoadRegisteredGraphicsCommands(@grplstfullfilepath);
                }
                #endregion

                #region Save to Disk
                if (saveoutput.IsChecked == true)
                {
                    if (fullpathfilename.Text != null && fullpathfilename.Text.Trim().Length > 0)
                    {

                        fullfilepathname = fullpathfilename.Text;///setting filename
                        bool fileExists = File.Exists(fullfilepathname); fileExists = false;
                        if (fullfilepathname.Contains('.') && !fileExists)
                        {
                            string extension = Path.GetExtension(fullfilepathname).ToLower();

                            if (extension.Equals(".csv"))
                            { fileformat = C1.WPF.FlexGrid.FileFormat.Csv; extratags = false; }
                            else if (extension.Equals(".html"))
                            { fileformat = C1.WPF.FlexGrid.FileFormat.Html; extratags = false; }
                            else if (extension.Equals(".bsoz"))
                            { fileformat = C1.WPF.FlexGrid.FileFormat.Html; extratags = true; }
                            else
                            { fileformat = C1.WPF.FlexGrid.FileFormat.Html; extratags = true; fullfilepathname = fullfilepathname + ".bsoz"; }
                        }
                        else
                        {
                            MessageBox.Show(this, "Output File Already Exists! Provide different name in Command Editor window.");
                            return;
                        }
                    }
                    else
                    {
                        MessageBox.Show(this, "Please provide new output filename and fileformat by clicking 'Browse' in Command Editor for saving the output.", "Save Output is checked...", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                        return;
                    }
                }
                #endregion

                #region Get Active output Window
                //////// Active output window ///////
                OutputWindowContainer owc = (LifetimeService.Instance.Container.Resolve<IOutputWindowContainer>()) as OutputWindowContainer;
                ow = owc.ActiveOutputWindow as OutputWindow; //get currently active window
                if (saveoutput.IsChecked == true)
                {
                    ow.ToDiskFile = true;//save lst to disk. Dump
                }
                #endregion


                #region Executing Syntax Editors Commands
                ///// Now statements from Syntax Editor will be executed ////
                CommandOutput lst = new CommandOutput(); ////one analysis////////
                lst.IsFromSyntaxEditor = true;
                if (saveoutput.IsChecked == true)//10Jan2013
                    lst.SelectedForDump = true;

                ////17Nov2017 for opening a flat file that has ; as a field separator. this field separator is not for line termination in R syntax.
                commands = commands.Replace("\";\"", "'BSkySemiColon'").Replace("';'", "'BSkySemiColon'"); 

                ////03Oct2014 We should remove R comments right here, before proceeding with execution.
                string nocommentscommands = RemoveCommentsFromCommands(commands);

                ExecuteCommandsAndCreateSinkFile(ow, lst, nocommentscommands, fname);
                bool s = true;

                if (s) CreateOuput(ow); /// for last remaining few non BSkyFormat commands, if any.

                /// 

                #endregion


                #region Saving to Disk
                //////Dumping results from Syntax Editor ////08Aug2012
                if (saveoutput.IsChecked == true)
                    ow.DumpAllAnalyisOuput(fullfilepathname, fileformat, extratags);
                #endregion

            }
            catch (Exception ex)
            {
                SendCommandToOutput(BSky.GlobalResources.Properties.Resources.ErrExecutingCommand, BSky.GlobalResources.Properties.Resources.ErrInRCommand);
                logService.WriteToLogLevel("Exeception:" + ex.Message, LogLevelEnum.Error);
                //15Sep2015 Following may be needed to remove lock from sink file
                ResetSink();
                CloseSinkFile();
            }
            finally
            {
                BSkyMouseBusyHandler.HideMouseBusy(true);// true means forcing mousefree in just one call.
            }
        }

        #region Mouse Busy - Mouse Free
        Cursor defaultcursor;

        private void ShowMouseBusy_old()
        {
            defaultcursor = Mouse.OverrideCursor;
            Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
        }
        //Hides Progressbar
        private void HideMouseBusy_old()
        {
            Mouse.OverrideCursor = null;
        }
        #endregion

        //03Oct2014 Removing comments from the selection 
        private string RemoveCommentsFromCommands(string alltext)
        {
            StringBuilder final = new StringBuilder();
            char[] splitchars = { '\r', '\n', ';' };
            string[] lines = alltext.Split(splitchars);
            int len = lines.Length;

            for (int i = 0; i < len; i++)
            {
                if (lines[i] != null && lines[i].Length > 0)
                    final.AppendLine(RemoveComments(lines[i].Trim()));// +"\r\n";
            }

            return (final.ToString());
        }

        //Checks if number of different types of brakets have openbracket count == closebracket count.
        private bool AreBracketsBalanced(string commands, out string msg) 
        {
            bool balanced = true;
            int roundbrackets = 0;
            int curlybrackets = 0;
            int squarebrackets = 0;
            bool dblQuotes = false;
            bool sglQuotes = false;
            string brackets = "()[]{}";
			char prevchar='\0';
            //loop thru char by char to find each type
            foreach (char ch in commands)
            {
                if (ch == '\"' && !dblQuotes && !sglQuotes)
                    dblQuotes = true;
                else if (ch == '\"' && dblQuotes && prevchar != '\\')
                    dblQuotes = false;

                if (ch == '\'' && !sglQuotes && !dblQuotes)
                    sglQuotes = true;
                else if (ch == '\'' && sglQuotes && prevchar != '\\')
                    sglQuotes = false;

                if (!dblQuotes && !sglQuotes && brackets.Contains(ch))
                {
                    if (ch == '(') roundbrackets++;
                    else if (ch == ')') roundbrackets--;
                    else if (ch == '{') curlybrackets++;
                    else if (ch == '}') curlybrackets--;
                    else if (ch == '[') squarebrackets++;
                    else if (ch == ']') squarebrackets--;
                }
				prevchar = ch;
            }

            if (roundbrackets != 0 || curlybrackets != 0 || squarebrackets != 0)
                balanced = false;

            ////Generating error message based on counts
            string msg1 = string.Empty, msg2 = string.Empty, msg3 = string.Empty;

            if (roundbrackets > 0)
            {
                msg1 = BSky.GlobalResources.Properties.Resources.missing + " ')'";
            }
            if (roundbrackets < 0)
            {
                msg1 = BSky.GlobalResources.Properties.Resources.unexpected + " ')' ";
            }
            if (curlybrackets > 0)
            {
                msg2 = BSky.GlobalResources.Properties.Resources.missing + " '}'";
            }
            if (curlybrackets < 0)
            {
                msg2 = BSky.GlobalResources.Properties.Resources.unexpected + " '}' ";
            }
            if (squarebrackets > 0)
            {
                msg3 = BSky.GlobalResources.Properties.Resources.missing + " ']'";
            }
            if (squarebrackets < 0)
            {
                msg3 = BSky.GlobalResources.Properties.Resources.unexpected + " ']' ";
            }

            msg = msg1 + " " + msg2 + " " + msg3;

            return balanced;
        }

        //22Nov2013 for sending session contents to output window.
        public void DisplayAllSessionOutput(string sessionheader = "", OutputWindow selectedOW = null)
        {
            sessionlst.NameOfSession = sessionheader;
            sessionlst.isRSessionOutput = true;
            
            if (sessionlst.Count > 0)//07Nov2014
            {
                if (selectedOW == null)
                {
                    if (ow != null)
                    {
                        ow.AddSynEdtSessionOutput(sessionlst);
                    }
                }
                else
                {
                    selectedOW.AddSynEdtSessionOutput(sessionlst);
                }
            }
            //21Nov2013
            sessionlst = new SessionOutput();//28Nov2013 for creating  new instance and not deleting old one
        }

        //////////pull all the currently loaded datasets names //////////
        private string getActiveDatasetNames()
        {
            string allDatasetnames = string.Empty;
            UIControllerService layoutController = LifetimeService.Instance.Container.Resolve<IUIController>() as UIControllerService;

            foreach (TabItem ti in (layoutController.DocGroup.Items))
            {
                if (allDatasetnames.Trim().Length < 1)
                    allDatasetnames = "[" + (ti.Tag as DataSource).Name + "] - " + (ti.Tag as DataSource).FileName;
                else
                    allDatasetnames = allDatasetnames + "\n" + "[" + (ti.Tag as DataSource).Name + "] - " + (ti.Tag as DataSource).FileName;
            }
            return allDatasetnames;
        }

        /// Delete old existing imagexxx.png files just before launching graphic device (each time) ///06May2013
        private void DeleteOldGraphicFiles()
        {
            string synedtimgname = confService.GetConfigValueForKey("sinkimage");//23nov2012 
            string tempDir = BSkyAppData.RoamingUserBSkyTempPath;

            string synedtimg = Path.Combine(tempDir, synedtimgname);
            
            int percentindex = synedtimg.IndexOf("%");
            int dindex = synedtimg.IndexOf("d", percentindex);
            string percentstr = synedtimg.Substring(percentindex, (dindex - percentindex + 1));
            string tempsynedtimg = Path.GetFileName(synedtimg).Replace(percentstr, "*");
            //Delete all image Files in temp Folder ///
            foreach (FileInfo fi in new DirectoryInfo(tempDir).GetFiles(tempsynedtimg))
            {
                DeleteFileIfPossible(@fi.FullName);
            }
        }

        int GraphicDeviceImageCounter = 0;//to keep track of the next image file name.
        private void OpenGraphicsDevice(int imagewidth = 0, int imageheight = 0)//05May2013
        {
            DeleteOldGraphicFiles();//06May2013
            CommandRequest grpcmd = new CommandRequest();

            string synedtimgname = confService.GetConfigValueForKey("sinkimage");//23nov2012
            string synedtimg = Path.Combine(BSkyAppData.RoamingUserBSkyTempPath, synedtimgname);
            // load default value if no path is set or invalid path is set
            if (synedtimg.Trim().Length == 0 || !IsValidFullPathFilename(synedtimg, false))
            {
                MessageBox.Show(this, BSky.GlobalResources.Properties.Resources.sinkimageConfigKeyNotFound);
                return;
            }

            //27May2015. if parameters are passed, parameter values will take over ( overrides the values set through 'Options'
            if (imageheight > 0 && imagewidth > 0)
            {
                _currentGraphicWidth = imagewidth;
                _currentGraphicHeight = imageheight;
            }
            else // use dimenstions set in 'Options' config. IF thats absent then use 580 as default.
            {
                _currentGraphicWidth = 580;
                _currentGraphicHeight = 580;//defaults
                //get image size from config
                string imgwidth = confService.GetConfigValueForKey("imagewidth");//
                string imgheight = confService.GetConfigValueForKey("imageheight");//

                // load default value if no value is set or invalid value is set
                if (imgwidth.Trim().Length != 0)
                {
                    Int32.TryParse(imgwidth, out _currentGraphicWidth);
                }
                if (imgheight.Trim().Length != 0)
                {
                    Int32.TryParse(imgheight, out _currentGraphicHeight);
                }
            }

            ////Actually image height and width should be same(assumed). 
            grpcmd.CommandSyntax = "png(\"" + synedtimg + "\", width=" + _currentGraphicWidth + ",height=" + _currentGraphicHeight + ")";
            analytics.ExecuteR(grpcmd, false, false);

            //close graphic device to get the size of empty image.
            CloseGraphicsDevice();

            // Basically, make sure to find the exact first image name(with full path) that is created when graphic device is opened.
            string tempimgname = synedtimg.Replace("%03d", "001");

            if (File.Exists(tempimgname))
            {
                EMPTYIMAGESIZE = new FileInfo(tempimgname).Length;
            }
            EMPTYIMAGESIZE = EMPTYIMAGESIZE + 10;

            //Now finally open graphic device to actually wait for graphic command to execute and capture it.
            grpcmd.CommandSyntax = "png(\"" + synedtimg + "\", width=" + _currentGraphicWidth + ",height=" + _currentGraphicHeight + ")";
            analytics.ExecuteR(grpcmd, false, false);
            GraphicDeviceImageCounter = 0;//09Jun2015
        }

        //Closes current graphics device
        private void CloseGraphicsDevice()
        {
            CommandRequest grpcmd = new CommandRequest();
            grpcmd.CommandSyntax = "if(dev.cur()[[1]] == 2) dev.off()";//09Jun2015 "dev.off()"; // "graphic.off()"; //msg <- 
            analytics.ExecuteR(grpcmd, false, false);
        }

        //check file size of PNG file generated by R
        private long GetGraphicSize()//05May2013
        {
            long size = 0;
            CommandRequest grpcmd = new CommandRequest();

            string synedtimgname = confService.GetConfigValueForKey("sinkimage");//23nov2012
            string synedtimg = Path.Combine(BSkyAppData.RoamingUserBSkyTempPath, synedtimgname);
            // load default value if no path is set or invalid path is set
            if (synedtimg.Trim().Length == 0 || !IsValidFullPathFilename(synedtimg, false))
            {
                MessageBox.Show(this, BSky.GlobalResources.Properties.Resources.sinkimageConfigKeyNotFound);
                return 0;
            }
            if (File.Exists(synedtimg))
            {
                size = new FileInfo(synedtimg).Length;
            }

            return size;
        }

        //if in config window the image size has been altered,
        //we must close graphic device and open it again with new dimentions from config
        public void RefreshImgSizeForGraphicDevice()
        {
            int newwidth = 10;
            int newheight = 10;
            //get image size from config
            string imgwidth = confService.GetConfigValueForKey("imagewidth");//
            string imgheight = confService.GetConfigValueForKey("imageheight");//
            // load default value if no value is set or invalid value is set
            if (imgwidth.Trim().Length != 0)
            {
                Int32.TryParse(imgwidth, out newwidth);
            }
            if (imgheight.Trim().Length != 0)
            {
                Int32.TryParse(imgheight, out newheight);
            }

            if (_currentGraphicWidth != newwidth || _currentGraphicHeight != newheight) // if config setting modified
            {
                CloseGraphicsDevice();
                OpenGraphicsDevice();
            }
        }

        //18Nov2013 Add BSky OSMT and CrossTab to Session. This output is return back from OutputWindow
        public void AddToSession(CommandOutput co)
        {
            if (co != null && co.Count > 0)
            {
                sessionlst.Add(co);
                co = new CommandOutput();//after adding to session new object is allocated for futher output creation
            }
        }

        private void ExecuteCommandsAndCreateSinkFile(OutputWindow ow, CommandOutput lst, string seltext, string fname)//sending message and output to sink file
        {
            string objectname;

            seltext = seltext.Replace('\n', ';').Replace('\r', ' ').Trim();
            seltext = JoinCommaSeparatedStatment(seltext);
			seltext = JoinPlusSignSeparatedStatment(seltext);		
			seltext = JoinPipeSeparatedStatment(seltext);
            string stmt = "";
            //////wrap in sink////////

            string sinkfilename = confService.GetConfigValueForKey("tempsink");//23nov2012
            string sinkfilefullpathname = Path.Combine(BSkyAppData.RoamingUserBSkyTempPath, sinkfilename);
            // load default value if no path is set or invalid path is set
            if (sinkfilefullpathname.Trim().Length == 0 || !IsValidFullPathFilename(sinkfilefullpathname, false))
            {
                MessageBox.Show(this, BSky.GlobalResources.Properties.Resources.tempsinkConfigKeyNotFound);
                return;
            }
            OpenSinkFile(@sinkfilefullpathname, "wt");
            SetSink();

            string stm = string.Empty;
            string _command = string.Empty;//05May2013
            int bskyfrmtobjcount = 0, next = 0, strt = 0, eol = 0;
            bool breakfor = false;

            for (int start = 0, end = 0; start < seltext.Length; start = start + end + 1) //28Jan2013 final condition was start < seltext.Length-1
            {
                objectname = "";

                end = seltext.IndexOf(';', start) - start;  

                if (end < 0) // if ; not found
                    end = seltext.IndexOf('\n', start) - start;
                if (end < 0)// if new line not found
                    end = seltext.Length - start;
                stmt = seltext.Substring(start, end).Replace('\n', ' ').Replace('\r', ' ').Trim();

                stmt = HasFilePath(stmt);

                if (stmt.Trim().Length < 1 || stmt.Trim().IndexOf('#') == 0)
                    continue;

                if (AdvancedLogging) logService.WriteToLogLevel("ExtraLogs: Syntax going to execute : " + stmt, LogLevelEnum.Info);

                if (stmt.Trim().IndexOf('#') > 1) //12May2014 if any statment has R comments in the end in same line.
                    stmt = stmt.Substring(0, stmt.IndexOf("#"));

                object o = null;

                if (AdvancedLogging) logService.WriteToLogLevel("ExtraLogs: Categorizing command before execution.", LogLevelEnum.Info);

                _command = ExtractCommandName(stmt);//07sep2012

                RCommandType rct = GetRCommandType(_command);

                //17Nov2017 Putting back the semicolon in place of BSkySemiColon
                stmt = stmt.Replace("'BSkySemiColon'", "';'");

                if (AdvancedLogging) logService.WriteToLogLevel("ExtraLogs: Syntax command category : " + rct.ToString(), LogLevelEnum.Info);

                try
                {
                    switch (rct)
                    {
                        case RCommandType.CONDITIONORLOOP:  //Block Commands

                            int end2 = end;
                            stmt = CurlyBracketParser(seltext, start, ref end);

                            if (stmt.Equals("ERROR"))
                            {
                                breakfor = true;
                            }
                            else
                            {
                                string originalFormatSyn2 = seltext.Substring(start, end).Replace(';','\n');
                                SendCommandToOutput(originalFormatSyn2, "R-Command");
                                ExecuteOtherCommand(ow, stmt);
                            }

                            //02Dec2014
                            ResetSink();
                            CloseSinkFile();
                            CreateOuput(ow);
                            OpenSinkFile(@sinkfilefullpathname, "wt");
                            SetSink();
                            ///02Dec2015 Now add graphic (all of them, from temp location)
                            CreateAllGraphicOutput(ow);//get all grphics and send to output
                            break;
                        case RCommandType.GRAPHIC:

                            CommandRequest grpcmd = new CommandRequest();

                            grpcmd.CommandSyntax = "write(\"" + stmt.Replace("<", "&lt;").Replace('"', '\'') + "\",fp)";// http://www.w3schools.com/xml/xml_syntax.asp
                            o = analytics.ExecuteR(grpcmd, false, false); //for printing command in file

                            CloseGraphicsDevice();

                            ResetSink();
                            CloseSinkFile();
                            CreateOuput(ow);
                            OpenSinkFile(@sinkfilefullpathname, "wt");
                            SetSink();
                            OpenGraphicsDevice();//05May2013

                            break;
                        case RCommandType.GRAPHICXML:
                            ExecuteXMLTemplateDefinedCommands(stmt);
                            break;
                        case RCommandType.BSKYFORMAT:

                            ResetSink();
                            CloseSinkFile();
                            CreateOuput(ow);
                            SendCommandToOutput(stmt, "BSkyFormat");//26Aug2014 blue colored
                            ExecuteBSkyFormatCommand(stmt, ref bskyfrmtobjcount, ow); 
                            OpenSinkFile(@sinkfilefullpathname, "wt");
                            SetSink();
                            break;
                        case RCommandType.BSKYLOADREFRESHDATAFRAME: 
                            ResetSink();
                            CloseSinkFile();
                            CreateOuput(ow);
                            SendCommandToOutput(stmt, "Load-Refresh Dataframe");//26Aug2014 blue colored
                            bool success = ExecuteBSkyLoadRefreshDataframe(stmt, fname);

                            if (!success)
                            {
                                SendErrorToOutput(BSky.GlobalResources.Properties.Resources.ErrCantLodRefDataset, ow); //03Jul2013
                                SendErrorToOutput(BSky.GlobalResources.Properties.Resources.DFNotExists, ow); //03Jul2013
                                SendErrorToOutput(BSky.GlobalResources.Properties.Resources.NotDataframe, ow); //03Jul2013
                                SendErrorToOutput(BSky.GlobalResources.Properties.Resources.ReqRPkgMissing, ow); //03Jul2013
                            }
                            OpenSinkFile(@sinkfilefullpathname, "wt");
                            SetSink();
                            break;
                        case RCommandType.BSKYREMOVEREFRESHDATAFRAME: 
                            ResetSink();
                            CloseSinkFile();
                            CreateOuput(ow);
                            ExecuteBSkyRemoveRefreshDataframe(stmt);
                            OpenSinkFile(@sinkfilefullpathname, "wt");
                            SetSink();
                            break;
                        case RCommandType.SPLIT: 
                            ResetSink();
                            CloseSinkFile();
                            CreateOuput(ow);
                            ExecuteSplit(stmt);
                            OpenSinkFile(@sinkfilefullpathname, "wt");
                            SetSink();
                            break;
                        case RCommandType.RCOMMAND:
                            string originalFormatSyn = seltext.Substring(start, end).Replace(';', '\n');
                            SendCommandToOutput(originalFormatSyn, "R-Command");

                            if (AdvancedLogging) logService.WriteToLogLevel("ExtraLogs: Categorized. Before execution.", LogLevelEnum.Info);

                            ExecuteOtherCommand(ow, stmt);

                            if (AdvancedLogging) logService.WriteToLogLevel("ExtraLogs: Categorized. After execution.", LogLevelEnum.Info);

                            ResetSink();
                            CloseSinkFile();
                            CreateOuput(ow);
                            OpenSinkFile(@sinkfilefullpathname, "wt");
                            SetSink();

                            if (AdvancedLogging) logService.WriteToLogLevel("ExtraLogs: Categorized. Before getting graphic if any.", LogLevelEnum.Info);
                            ///02Dec2015 Now add graphic (all of them, from temp location)
                            CreateAllGraphicOutput(ow);//get all grphics and send to output
                            if (AdvancedLogging) logService.WriteToLogLevel("ExtraLogs: Categorized. After getting graphic if any.", LogLevelEnum.Info);

                            break;
                        
                        case RCommandType.RDOTNET:
                            InitializeRDotNet();
                            RDotNetOpenDataset();
                            RDotNetExecute(ow);
                            DisposeRDotNet();
                            break;
                        default:
                            break;
                    }//switch
                }
                catch (Exception exc)
                {
                    SendCommandToOutput(exc.Message, "Error:");
                    logService.WriteToLogLevel("Error executing: " + _command, LogLevelEnum.Error);
                    logService.WriteToLogLevel(exc.Message, LogLevelEnum.Error);
                }

                if (breakfor)
                    break;


            }/////for

            //////wrap in sink////////
            ResetSink();
            CloseSinkFile();
        }

        private bool AreBalanced(string stmt)
        {
            bool isBalanced = true;
            int stmtlen = stmt.Length;
            int parencount = 0, curlycount = 0;

            for (int i = 0; i < stmtlen; i++)
            {
                switch (stmt[i])
                {
                    case '(':
                        parencount++;
                        break;
                    case ')':
                        if (parencount > 0) //this helps to verify if brackets appeared in right order or not eg.. )(
                            parencount--;
                        break;
                    case '{':
                        curlycount++;
                        break;
                    case '}':
                        if (curlycount > 0) //this helps to verify if brackets appeared in right order or not eg.. }{
                            curlycount--;
                        break;
                    default:
                        break;
                }
            }

            if (parencount != 0 || curlycount != 0)
                return false;
            else
                return true;
        }

        private string RemoveSemicolonAfterBrackets(string stmt)
        {
            StringBuilder resultstr = new StringBuilder();
            bool beginwatch = false;

            for (int i = 0; i < stmt.Length; i++)
            {
                if (stmt[i] == '(' || stmt[i] == '{')
                {
                    beginwatch = true;
                }
                else if (stmt[i] != ' ' && stmt[i] != ';')
                {
                    beginwatch = false;
                }

                if (beginwatch && stmt[i] == ';')
                {
                    resultstr.Append(' ');
                }
                else
                {
                    resultstr.Append(stmt[i]);
                }
            }

            return resultstr.ToString();
        }

        private string RemoveSemicolonBetweenBrackets(String stmt)
        {
            StringBuilder sb = new StringBuilder();

            return sb.ToString();
        }

        //13Nov2013
        private RCommandType GetRCommandType(string _command)
        {
            RCommandType rct;

            if (isConditionalOrLoopingCommand(_command))
                rct = RCommandType.CONDITIONORLOOP;
            else if (isGraphicCommand(_command))
            {
                if (isXMLDefined())
                {
                    rct = RCommandType.GRAPHICXML;
                }
                else
                {
                    rct = RCommandType.RCOMMAND; //
                }
            }
            else if (_command.Contains("BSkyFormat("))
                rct = RCommandType.BSKYFORMAT;
            else if (_command.Contains("BSkyLoadRefreshDataframe("))
                rct = RCommandType.BSKYLOADREFRESHDATAFRAME;
            else if (_command.Contains("BSkyRemoveRefreshDataframe("))
                rct = RCommandType.BSKYREMOVEREFRESHDATAFRAME;
            else if (_command.Contains("BSkySetDataFrameSplit(")) //set or remove split
                rct = RCommandType.SPLIT;
            else if (_command.Contains("RDotNetTest"))
                rct = RCommandType.RDOTNET;
            else
                rct = RCommandType.RCOMMAND;

            return rct;
        }

        private string HasFilePath(string command)
        {
            //path string that begins and ends with single or double quote
            string pat = @"[""']((\\\\[a-zA-Z0-9-]+\\[a-zA-Z0-9`~!@#$%^&(){}'._-]+([ ]+[a-zA-Z0-9`~!@#$%^&(){}'._-]+)*)|([a-zA-Z]:))(\\[^ \\/:*?""<>|]+([ ]+[^'\\/:*?""<>|]+)*)*\\?[""']";
            bool found = false;
            string strfound = string.Empty;
            string resultstr = command; // if path not found then command should be returned as is
            MatchCollection mcol = Regex.Matches(command, pat);

            if (mcol.Count > 0)
            {
                found = true;
                strfound = mcol[0].ToString();

                string strreplace = strfound.Replace(@"\", @"/");
                resultstr = Regex.Replace(command, pat, strreplace);
            }
            return resultstr;
        }

        /// Join the statments Ends in comma
        private string JoinCommaSeparatedStatment(string comm)//, int start, ref int end)
        {
            comm = Regex.Replace(comm, @",\s*;", ",");
            return comm;
        }

        /// Join the statments Ends in a PLUS sign
        private string JoinPlusSignSeparatedStatment(string comm)//, int start, ref int end)
        {
            //string result = string.Empty;
            //for (int i = 0; i < comm.Length;i++ )
            //{
            //}
            comm = Regex.Replace(comm, @"\+\s*;", "+");
            return comm;
        }
        /// Join the statments Ends in Pipe '%<%'
        private string JoinPipeSeparatedStatment(string comm)//, int start, ref int end)
        {
            comm = Regex.Replace(comm, @"%>%\s*;", " %>% ");
            return comm;
        }
		
        ////curly block parser////
        private string CurlyBracketParser(string comm, int start, ref int end)
        {
            string str = string.Empty;
            int curlyopen = 0, curlyclose = 0;
            for (int i = comm.IndexOf('{', start); i < comm.Length; i++)
            {
                if (comm.ElementAt(i).Equals('{')) curlyopen++;
                else if (comm.ElementAt(i).Equals('}')) curlyclose++;
                if (curlyopen == curlyclose)
                {
                    end = i + 1 - start;
                    //if(start < comm.Length)
                    str = comm.Substring(start, end).Replace("}}", "} }").Replace(";{", "{").Replace("{;", "{").Replace("}", ";} ");
                    start = i + 1;
                    break;
                }
            }
            if (curlyopen != curlyclose)
            {
                //MessageBox.Show("Error in block declaration. Mismatch { or }");
                CommandRequest cmdprn = new CommandRequest();
                cmdprn.CommandSyntax = "write(\"" + BSky.GlobalResources.Properties.Resources.ErrCurlyMismatch + "\",fp)";
                analytics.ExecuteR(cmdprn, false, false); /// for printing command in file
                return "ERROR";
            }
            str = Regex.Replace(str, @";+", ";");//multi semicolon to one ( no space between them)
            //str = Regex.Replace(str, @"}\s+;", "} ");//semicolon after close }
            str = Regex.Replace(str, @";\s*;", ";");//multi semicolon to one(space between them)
            str = Regex.Replace(str, @"}\s*;\s*}", "} }");//semicolon between two closing } }
            str = Regex.Replace(str, @"{\s*;", "{ ");//semicolon immediatly after opening {
            str = Regex.Replace(str, @";\s*{", "{ ");//semicolon immediatly after opening {
            if (str.Contains("else"))
            {
                str = Regex.Replace(str, @"}\s*;*\s*else", "} else");//semicolon before for is needed. Fix for weird bug.
            }
            ///if .. else if logic ///
            if ((str.Trim().StartsWith("if") || str.Trim().StartsWith("else")) && comm.Length > end + 1)
            {
                string elsestr = string.Empty;

                if (start + 1 < comm.Length)
                    elsestr = comm.Substring(start + 1);

                int originalLen = elsestr.Length;
                elsestr = Regex.Replace(elsestr, @";*\s*else", " else").Trim();
                int newLen = elsestr.Length;

                if (elsestr.StartsWith("else"))
                {
                    int end2 = 0;
                    str = str + CurlyBracketParser(elsestr, 0, ref end2);
                    end = end + end2 + (originalLen - newLen + 1);
                }
            }

            if (isRoundBracketBlock(str))
            {
                //search closing round bracket from original string and append to str. Update start
                int idxclosinground = comm.IndexOf(")", start);
                string closinground = comm.Substring(start, idxclosinground - start + 1).Replace(";", " ").Trim();

                if (closinground.Equals(")"))
                {
                    end = end + idxclosinground + 1 - start;
                }
                return str + closinground;
            }
            return str;
        }

        private bool isRoundBracketBlock(string comm)
        {
            bool isroundblock = false;
            string subs = string.Empty;
            int roundbrktidx = comm.IndexOf("(");
            int curlybrktidx = comm.IndexOf("{");

            if (roundbrktidx > -1 && curlybrktidx > -1 && roundbrktidx < curlybrktidx)
            {
                subs = comm.Substring(roundbrktidx + 1, curlybrktidx - roundbrktidx - 1);//extract string between ( and {.eg.. local(;{
                subs = subs.Replace(";", " ");
                if (subs.Trim().Length > 0)//this is true is there was something in between ( and { eg.. if(condi){
                {
                    isroundblock = false;
                }
                else //there was nothing in between ( and {. eg.. local(   {
                {
                    isroundblock = true;
                }
            }
            return isroundblock;
        }

        ////round bracket block parser////
        private string RoundBracketParser(string comm, int start, ref int end)
        {
            string str = string.Empty;
            int roundopen = 0, roundclose = 0;

            for (int i = comm.IndexOf('(', start); i < comm.Length; i++)
            {
                if (i < 0)
                    continue;
                if (comm.ElementAt(i).Equals('(')) roundopen++;
                else if (comm.ElementAt(i).Equals(')')) roundclose++;

                if (roundopen == roundclose)
                {
                    int idx = comm.IndexOf(";", i);

                    if (idx > i)
                    {
                        end = idx - start;
                        str = comm.Substring(start, end).Replace("))", ") )").Replace(";(", "(").Replace("(;", "(");
                    }
                    else
                    {
                        end = comm.Length - start;
                        str = comm.Substring(start, end).Replace("))", ") )").Replace(";(", "(").Replace("(;", "(");
                    }
                    break;
                }
            }
            if (roundopen != roundclose)
            {
                //MessageBox.Show("Error in block declaration. Mismatch { or }");
                CommandRequest cmdprn = new CommandRequest();
                cmdprn.CommandSyntax = "write(\"" + BSky.GlobalResources.Properties.Resources.ErrParenthesisMismatch + "\",fp)";
                analytics.ExecuteR(cmdprn, false, false); /// for printing command in file
                return "ERROR";
            }

            str = str.Replace(";", " ");
            str = RemoveComments(str);
            return str;
        }

        private string RemoveComments_others(string str)//14May2014
        {
            if (str == null || str.Length < 1)
                return null;

            int len = str.Length;

            int sidx = str.IndexOf("#"); 
            int eidx = 0, remvlen = 0;

            if (sidx < 0) // if there is no comment
                return str;

            for (; ; )
            {
                eidx = str.IndexOf(";", sidx);
                remvlen = eidx - sidx;
                str = str.Remove(sidx, remvlen);

                //len = str.Length;
                sidx = str.IndexOf("#");
                if (sidx < 0)
                    break;
            }
            return str;
        }

        ////03Oct2014 New Remove comments logic
        private string RemoveComments(string text)
        {
            string nocommenttext = string.Empty;
            int openbracketcount = 0, singlequote = 0, doublequote = 0;

            if (text != null && text.Length > 0 && text.Contains('#'))
            {
                int idx = 0;

                for (idx = 0; idx < text.Length; idx++) // go character by character
                {
                    if (text[idx].Equals('(')) openbracketcount++;
                    else if (text[idx].Equals(')')) openbracketcount--;
                    else if (text[idx].Equals('\'')) singlequote++;
                    else if (text[idx].Equals('"')) doublequote++;
                    else if (text[idx].Equals('#'))
                    {
                        if (openbracketcount == 0 && singlequote % 2 == 0 && doublequote % 2 == 0) // # is outside any quotes or brackets
                        {
                            nocommenttext = text.Substring(0, idx);
                            break;
                        }
                    }
                }

            }
            else//that means there is no #-comment in that line.
            {
                nocommenttext = text;
            }
            return nocommenttext;
        }
        //// curly block logics ////NOT in Use
        private string BlockCodeParser(string seltext, int start, ref int end)
        {
            string stmt = string.Empty;
            string subs = seltext.Substring(start).Replace("}}", "} }").Replace(";{", "{").Replace("{;", "{");
            int blockendindex = 0;
            int curlyopen = 0;
            int indeOfFirstCloseCurly = subs.IndexOf('}');

            for (int st = 0; st < indeOfFirstCloseCurly;)//count opening curly brackets
            {
                int curindex = subs.IndexOf('{', st);

                if (curindex >= 0 && curindex < indeOfFirstCloseCurly)
                { curlyopen++; st = curindex + 1; }
                else break;
            }

            int curlyclose = 0;

            for (int st = 0; st < subs.Length - 1;)//count closing curly brackets
            {
                int curindex = subs.IndexOf('}', st);

                if (curindex >= 0)//if found
                { curlyclose++; st = curindex + 1; }
                else break;

                if (curlyclose == curlyopen)
                {
                    blockendindex = curindex;//length to be extracted
                    break;
                }
            }
            if (curlyopen != curlyclose)
            {
                MessageBox.Show(this, BSky.GlobalResources.Properties.Resources.ErrCurlyMismatch);
                return "";
            }

            string tmpstr = subs.Substring(0, blockendindex + 1).Replace('\n', ';').Replace('\r', ' ').Replace(" in ", "$#in#$").Replace(" ", string.Empty).Replace("$#in#$", " in ").Trim();

            do
            {
                stmt = tmpstr.Replace(";;", ";");///.Replace("}", ";};")
            } while (stmt.Contains(";;"));
            end = blockendindex + 1;
            stmt = stmt.Replace("}", ";} ");
            return stmt;
        }

        // reading back sink file and creating & displaying output; for non-BSkyFormat commands
        private void CreateOuput(OutputWindow ow)
        {

            if (AdvancedLogging) logService.WriteToLogLevel("ExtraLogs: Started creating output.", LogLevelEnum.Info);

            //////////////////// for fetching BSkyFormat object Queue and process each object //////////////////////
            int bskyformatobjectindex = 0;
            bool bskyQFetched = false;
            CommandRequest fetchQ = null;
            string sinkfileBSkyFormatMarker = "[1] \"BSkyFormatInternalSyncFileMarker\"";
            string sinkfileBSkyGraphicFormatMarker = "[1] \"BSkyGraphicsFormatInternalSyncFileMarker\""; //09Jun2015 
            bool isBlockCommand = false;
            bool isBlockGraphicCommand = false;

            bool isBlock = false;
            ////////////////////////////////////////////////////////////////////////////////////////////////////////

            //if (true) return;
            CommandOutput lst = new CommandOutput(); ////one analysis////////
            CommandOutput grplst = new CommandOutput();//21Nov2013 Separate for Graphic. So Parent node name will be R-Graphic
            lst.IsFromSyntaxEditor = true;//lst belongs to Syn Editor
            if (saveoutput.IsChecked == true)//10Jan2013
                lst.SelectedForDump = true;
            XmlDocument xd = null;
            //string auparas = "";
            StringBuilder sbauparas = new StringBuilder("");
            //////////////// Read output ans message from file and create output then display /////
            //// read line by line  /////

            string sinkfilename = confService.GetConfigValueForKey("tempsink");//23nov2012
            string sinkfilefullpathname = Path.Combine(BSkyAppData.RoamingUserBSkyTempPath, sinkfilename);
            // load default value if no path is set or invalid path is set
            if (sinkfilefullpathname.Trim().Length == 0 || !IsValidFullPathFilename(sinkfilefullpathname, true))
            {
                MessageBox.Show(this, BSky.GlobalResources.Properties.Resources.tempsinkConfigKeyNotFound);
                return;
            }

            try
            {
                FileStream fs = new FileStream(sinkfilefullpathname, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                System.IO.StreamReader file = new System.IO.StreamReader(fs);//, Encoding.Default);

                object linetext = null; string line;
                bool insideblock = false;//20May2014
                bool readSinkFile = true;

                if (AdvancedLogging) logService.WriteToLogLevel("ExtraLogs: Started reading sink", LogLevelEnum.Info);

                while ((line = file.ReadLine()) != null)//(readSinkFile)
                {
                    {
                        if (line.Length < 1)//to show blank line in the output newline should be printed instead.
                            line = "\t";
                        linetext = line;
                    }


                    if (linetext == null || linetext.ToString().Equals("EOF"))
                    {
                        break;
                    }
                    if (linetext != null && linetext.Equals("NULL") && lastcommandwasgraphic)
                    {
                        continue;
                    }
                    if (linetext.ToString().Trim().Contains(sinkfileBSkyFormatMarker))
                    {
                        isBlockCommand = true;
                    }
                    else if (linetext.ToString().Trim().Contains(sinkfileBSkyGraphicFormatMarker))
                    {
                        isBlockGraphicCommand = true;
                    }
                    else
                    {
                        isBlockCommand = false;
                    }
                    //////// create XML doc /////////
                    if (linetext != null)//06May2013 we need formatting so we print blank lines.. 
                    {
                        /////// Trying to extract command from print //////
                        string commnd = linetext.ToString();
                        int opncurly = commnd.IndexOf("{");
                        int closcurly = commnd.IndexOf("}");
                        int lencommnd = closcurly - opncurly - 1;

                        if (opncurly != -1 && closcurly != -1)
                            commnd = commnd.Substring(opncurly + 1, lencommnd);//could be graphic or BSkyFormat in sink file.
                        
                        if (false)
                        {
                            SendToOutput(sbauparas.ToString(), ref lst, ow);//22May2014
                            sbauparas.Clear();
                        }
                        else if (isBlockCommand)//14Jun2014 for Block BSkyFormat.
                        {
                            if (sbauparas.Length > 0)
                            {
                                createAUPara(sbauparas.ToString(), lst);//Create & Add AUPara to lst 
                                sbauparas.Clear();
                            }
                        }
                        else
                        {
                            if (sbauparas.Length < 1)
                            {
                                sbauparas.Append(linetext.ToString());//First Line of AUPara. Without \n
                                if (sbauparas.ToString().Trim().IndexOf("BSkyFormat(") == 0)//21Nov2013
                                    lst.NameOfAnalysis = "BSkyFormat-Command";
                            }
                            else
                            {
                                if(linetext.ToString().StartsWith("\n"))//if linetext already has new line then no need to add \n
                                    sbauparas.Append(linetext.ToString());
                                else
                                    sbauparas.Append("\n" + linetext.ToString());//all lines separated by new line
                            }
                        }

                        if (isBlockGraphicCommand)//for block graphics //09Jun2015
                        {
                            CloseGraphicsDevice();

                            insideblock = true;

                            string synedtimgname = confService.GetConfigValueForKey("sinkimage");//23nov2012
                            string synedtimg = Path.Combine(BSkyAppData.RoamingUserBSkyTempPath, synedtimgname);
                            /////03May2013  Create zero padding string //// %03d means 000,  %04d means 0000
                            int percentindex = synedtimg.IndexOf("%");
                            int dindex = synedtimg.IndexOf("d", percentindex);
                            string percentstr = synedtimg.Substring(percentindex, (dindex - percentindex + 1));
                            string numbr = synedtimg.Substring(percentindex + 1, (dindex - percentindex - 1));
                            int zerocount = Convert.ToInt16(numbr);

                            string zeropadding = string.Empty;

                            for (int zeros = 1; zeros <= zerocount; zeros++)
                            {
                                zeropadding = zeropadding + "0";
                            }

                            {
                                GraphicDeviceImageCounter++;//imgcount++;

                                string tempsynedtimg = synedtimg.Replace(percentstr, GraphicDeviceImageCounter.ToString(zeropadding));
                                // load default value if no path is set or invalid path is set
                                if (tempsynedtimg.Trim().Length == 0 || !IsValidFullPathFilename(tempsynedtimg, true))
                                {

                                    isBlockGraphicCommand = false; //09Jun2015 reset, as we dont know what next command is. May or may not be graphic marker
                                    break;
                                }

                                string source = @tempsynedtimg;

                                Image myImage = new Image();

                                var bitmap = new BitmapImage();

                                try
                                {
                                    var stream = File.OpenRead(source);
                                    bitmap.BeginInit();
                                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                                    bitmap.StreamSource = stream;
                                    bitmap.EndInit();
                                    stream.Close();
                                    stream.Dispose();
                                    myImage.Source = bitmap;
                                    bitmap.StreamSource.Close(); //trying to close stream 03Feb2014

                                    if (isBlockGraphicCommand)
                                        createBSkyGraphic(myImage, lst); //20May2014 If graphic is inside block or loop
                                    else
                                        createBSkyGraphic(myImage, grplst); //if graphic is outside block or loop

                                    DeleteFileIfPossible(@tempsynedtimg);
                                }
                                catch (Exception ex)
                                {
                                    logService.WriteToLogLevel("Error reading Image file " + source + "\n" + ex.Message, LogLevelEnum.Error);
                                    MessageBox.Show(this, ex.Message);
                                }
                            }
                            if (GraphicDeviceImageCounter < 1) ////03May2013 if no images were added to output lst. then return.
                            {
                                sbauparas.Clear();//resetting
                                isBlockGraphicCommand = false;
                                return;
                            }
                            sbauparas.Clear();//resetting
                            isBlockGraphicCommand = false;
                        }
                        else if (isBlockCommand)
                        {
                            int bskyfrmtobjcount = 0;

                            if (!bskyQFetched)
                            {
                                fetchQ = new CommandRequest();
                                fetchQ.CommandSyntax = "BSkyQueue = BSkyGetHoldFormatObjList()";
                                analytics.ExecuteR(fetchQ, false, false);

                                fetchQ.CommandSyntax = "is.null(BSkyQueue)";

                                object o = analytics.ExecuteR(fetchQ, true, false);

                                if (o.ToString().ToLower().Equals("false"))
                                {
                                    bskyQFetched = true;
                                }
                            }
                            if (bskyQFetched)
                            {
                                bskyformatobjectindex++;
                                commnd = "BSkyFormat(BSkyQueue[[" + bskyformatobjectindex + "]])";

                                ExecuteSinkBSkyFormatCommand(commnd, ref bskyfrmtobjcount, lst);
                                lst = new CommandOutput();//"Child already has parent" error, fix
                                isBlock = true;
                            }
                            isBlockCommand = false;//09Jun2015 next command may or may not be BSkyFormat marker.
                        }
                    }//if 
                }//while
                if (AdvancedLogging) logService.WriteToLogLevel("ExtraLogs: Finished reading sink", LogLevelEnum.Info);
                file.Close();
                SendToOutput(sbauparas.ToString(), ref lst, ow, isBlock);//send output to window or disk file
                SendToOutput(null, ref grplst, ow, isBlock);//21Nov2013. separate node for graphic
                if (lst != null && lst.Count > 0 && isBlock) // Exceutes when there is block command
                {
                    sessionlst.Add(lst);//15Nov2013
                    lst = new CommandOutput();//after adding to session new object is allocated for futher output creation
                }

                if (AdvancedLogging) logService.WriteToLogLevel("ExtraLogs: Finished creating output.", LogLevelEnum.Info);
            }
            catch (Exception ex)
            {
                logService.WriteToLogLevel("Error reading sink.", LogLevelEnum.Info);
                logService.WriteToLogLevel(ex.Message, LogLevelEnum.Info);
                SendToOutput("Error:" + ex.Message, ref lst, ow);
            }
        }

        //This method gets outputs all the graphics.
        private void CreateAllGraphicOutput(OutputWindow ow)
        {
            CommandOutput grplst = new CommandOutput();
            long EmptyImgSize = EMPTYIMAGESIZE;
            CloseGraphicsDevice();
            ////// now add image to lst ////

            string synedtimgname = confService.GetConfigValueForKey("sinkimage");//23nov2012
            string synedtimg = Path.Combine(BSkyAppData.RoamingUserBSkyTempPath, synedtimgname);

            int percentindex = synedtimg.IndexOf("%");
            int dindex = synedtimg.IndexOf("d", percentindex);
            string percentstr = synedtimg.Substring(percentindex, (dindex - percentindex + 1));
            string numbr = synedtimg.Substring(percentindex + 1, (dindex - percentindex - 1));
            int zerocount = Convert.ToInt16(numbr);

            string zeropadding = string.Empty;

            for (int zeros = 1; zeros <= zerocount; zeros++)
            {
                zeropadding = zeropadding + "0";
            }

            int imgcount = GraphicDeviceImageCounter;//number of images to load in output

            for (; ; )
            {
                imgcount++;

                string tempsynedtimg = synedtimg.Replace(percentstr, imgcount.ToString(zeropadding));
                // load default value if no path is set or invalid path is set
                if (tempsynedtimg.Trim().Length == 0 || !IsValidFullPathFilename(tempsynedtimg, true))
                {
                    break;
                }

                string source = @tempsynedtimg;
                long imgsize = new FileInfo(source).Length;//find size of the imagefile

                if (imgsize > EmptyImgSize)//if image is not an empty image
                {
                    Image myImage = new Image();

                    var bitmap = new BitmapImage();

                    try
                    {
                        var stream = File.Open(source, FileMode.OpenOrCreate, FileAccess.Read, FileShare.ReadWrite);

                        bitmap.BeginInit();
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.StreamSource = stream;
                        bitmap.EndInit();
                        stream.Close();
                        stream.Dispose();
                        myImage.Source = bitmap;
                        bitmap.StreamSource.Close(); //trying to close stream 03Feb2014

                        createBSkyGraphic(myImage, grplst); //add graphic

                        DeleteFileIfPossible(@tempsynedtimg);
                    }
                    catch (Exception ex)
                    {
                        logService.WriteToLogLevel("Error reading Image file " + source + "\n" + ex.Message, LogLevelEnum.Error);
                        MessageBox.Show(this, ex.Message);
                    }
                }
            }
            if (imgcount < 1) ////03May2013 if no images were added to output lst. then return.
            {
                return;
            }
            SendToOutput(null, ref grplst, ow, false);//send all graphic to output
            OpenGraphicsDevice();//in case of errors or no errors, you must open graphic device
        }

        private void DeleteFileIfPossible(string fulpathfilename)
        {
            try
            {
                File.Delete(fulpathfilename);
            }
            catch (IOException ex)
            {
                logService.Error("Unable to delete :" + fulpathfilename);
                logService.Error("IOException: " + ex.Message);
            }
            catch (Exception ex1) //Added this on 25Jan2016 for catching other execeptions(those may crash the app)
            {
                logService.Error("Unable to delete :" + fulpathfilename);
                logService.Error("Exception: " + ex1.Message);
            }
        }

        private void SendToOutput(string auparas, ref CommandOutput lst, OutputWindow ow, bool isBlockCommand = false)//, bool last=false)
        {
            if (auparas != null && auparas.Length > 0)
            {
                this.createAUPara(auparas, lst);//Create & Add AUPara to lst and empty dommid
                auparas = null;
            }
            //////// send output to current active window ////////outputopencommand.cs
            if (lst != null && lst.Count > 0 && !isBlockCommand) //if non block command, then sent to output
            {
                sessionlst.Add(lst);//15Nov2013
                lst = new CommandOutput();//after adding to session new object is allocated for futher output creation
            }
        }

        private void SetSink() // set desired sink
        {
            sinkcmd.CommandSyntax = "options('warn'=1)";// trying to flush old errors
            analytics.ExecuteR(sinkcmd, false, false);

            sinkcmd.CommandSyntax = "sink(fp, append=FALSE, type=c(\"output\"))";// command
            analytics.ExecuteR(sinkcmd, false, false);
            sinkcmd.CommandSyntax = "sink(fp, append=FALSE, type=c(\"message\"))";// command
            analytics.ExecuteR(sinkcmd, false, false);
        }

        private void ResetSink() 
        {
            sinkcmd.CommandSyntax = "sink(stderr(), type=c(\"message\"))";// command
            analytics.ExecuteR(sinkcmd, false, false);
            sinkcmd.CommandSyntax = "sink()";//stdout(), type=\"output\")";// command
            analytics.ExecuteR(sinkcmd, false, false);
        }

        private void OpenSinkFile(string fullpathfilename, string mode)
        {
            string unixstylepath = fullpathfilename.Replace("\\", "/");
            sinkcmd.CommandSyntax = "fp<- file(\"" + unixstylepath + "\",encoding = \"UTF-8\", open=\"" + "w+" + "\")";// command. use r+ for read/write
            analytics.ExecuteR(sinkcmd, false, false);
        }

        private void CloseSinkFile()
        {
            sinkcmd.CommandSyntax = "flush(fp)";// command
            analytics.ExecuteR(sinkcmd, false, false);
            sinkcmd.CommandSyntax = "close(fp)";// command
            analytics.ExecuteR(sinkcmd, false, false);
        }

        private string findHeaderName(string bskyformatcmd)
        {
            if (bskyformatcmd.Trim().Contains("data.frame"))
                return "data.frame";
            else if (bskyformatcmd.Trim().Contains("array"))
                return "array";
            else if (bskyformatcmd.Trim().Contains("matrix"))
                return "matrix";
            else
            {
                return (bskyformatcmd);
            }
        }

        private bool IsAnalyticsCommand(string command)
        {
            bool bskycomm = false;
            return bskycomm;
        }

        private void ExecuteSplit(string stmt)
        {
            CommandRequest cmd = new CommandRequest();
            CommandExecutionHelper ceh = new CommandExecutionHelper();
            UAMenuCommand uamc = new UAMenuCommand();
            uamc.bskycommand = stmt;
            uamc.commandtype = stmt;
            cmd.CommandSyntax = stmt;
            ceh.ExecuteSplit(stmt, FElement);
            ceh = null;
        }

        private void ExecuteBSkyCommand(string stmt)
        {
            CommandRequest cmd = new CommandRequest();

            if (IsAnalyticsCommand(stmt))
            {
                ResetSink();
                cmd.CommandSyntax = stmt;// command 

                object o = analytics.ExecuteR(cmd, false, false);//executing syntax editor commands
                SetSink();
            }
        }

        private void ExecuteBSkyFormatCommand(string stmt, ref int bskyfrmtobjcount, OutputWindow ow)
        {
            KillTempBSkyFormatObj("bskytempvarname");
            KillTempBSkyFormatObj("bskyfrmtobj");

            string originalCommand = stmt;

            CommandOutput lst = new CommandOutput(); ////one analysis////////
            lst.IsFromSyntaxEditor = true;//lst belongs to Syn Editor
            if (saveoutput.IsChecked == true)//10Jan2013
                lst.SelectedForDump = true;

            object o;
            CommandRequest cmd = new CommandRequest();

            cmd.CommandSyntax = originalCommand;
            //16Aug2016 Not executing the command but sending it to logs because 
            analytics.LogCommandNotExecute(cmd);//this command is modified below and then executed after several checks.

            string subcomm = string.Empty, varname = string.Empty, BSkyLeftVar = string.Empty, headername = string.Empty;
            string firstparam = string.Empty, restparams = string.Empty, leftvarname = string.Empty;//23Sep2014
            string userpassedtitle = string.Empty;
            //SplitBSkyFormat(stmt, out subcomm, out varname, out BSkyLeftVar);
            SplitBSkyFormatParams(stmt, out firstparam, out restparams, out userpassedtitle);//23Spe2014
            if (userpassedtitle.Trim().Length > 0)//user passed title has the highest priority
            {
                headername = userpassedtitle.Trim();
            }

            {
                //23Sep2014 if firstParam is of the type obj<-OSMT(...) OR obj<-obj2
                if (firstparam.Contains("<-") || firstparam.Contains("=")) //if it has assignment
                {
                    int idxassign = -1, idxopenbrket = -1;

                    if (firstparam.Contains("("))// if obj<-OSMT(...)
                    {
                        idxopenbrket = firstparam.IndexOf("(");

                        string firsthalf = firstparam.Substring(0, idxopenbrket);// "obj <- OSMT("
                        idxassign = firsthalf.IndexOf("<-");
                        if (idxassign == -1)// '<-' not present(found in half)
                            idxassign = firsthalf.IndexOf("=");
                    }

                    if (idxassign > -1 && idxopenbrket > -1 && idxopenbrket > idxassign)
                    {
                        leftvarname = firstparam.Substring(0, idxassign);
                        headername = leftvarname.Trim();
                        cmd.CommandSyntax = firstparam;// command: osmt<-one.smt.tt(...)
                        o = analytics.ExecuteR(cmd, false, false);//executing sub-command; osmt<-one.smt.tt(...)
                    }
                    else if (idxopenbrket < 0)//type obj <- obj2
                    {
                        idxassign = firstparam.IndexOf("<-");
                        if (idxassign == -1)// '<-' not present
                            idxassign = firstparam.IndexOf("=");
                        if (idxassign > -1)//if assignment is there
                        {
                            leftvarname = firstparam.Substring(0, idxassign);
                            headername = leftvarname.Trim();
                            cmd.CommandSyntax = firstparam;// command: osmt<-one.smt.tt(...)
                            o = analytics.ExecuteR(cmd, false, false);//executing sub-command; osmt<-one.smt.tt(...)
                        }
                    }
                }

                /////25Feb2013 for writing errors in OutputWindow////

                string sinkfilename = confService.GetConfigValueForKey("tempsink");//23nov2012
                string sinkfilefullpathname = Path.Combine(BSkyAppData.RoamingUserBSkyTempPath, sinkfilename);
                // load default value if no path is set or invalid path is set
                if (sinkfilefullpathname.Trim().Length == 0 || !IsValidFullPathFilename(sinkfilefullpathname, false))
                {
                    MessageBox.Show(this, BSky.GlobalResources.Properties.Resources.tempsinkConfigKeyNotFound);
                    return; //return type was void before 22May2014
                }
                OpenSinkFile(@sinkfilefullpathname, "wt"); //06sep2012
                SetSink(); //06sep2012

                ////////////////////////////////////////////////////////////////////////
                varname = "bskytempvarname";
                KillTempBSkyFormatObj(varname);

                //Now run command
                firstparam = (leftvarname.Trim().Length > 0 ? leftvarname : firstparam);
                cmd.CommandSyntax = varname + " <- " + firstparam;// varname <- obj OR OSMT()
                o = analytics.ExecuteR(cmd, false, false);//executing sub-command
                ////////////////////////////////////////////////////////////////////////

                /////25Feb2013 for writing errors in OutputWindow////
                ResetSink();
                CloseSinkFile();
                CreateOuput(ow);
            }

            //if var does not exist then there could be error in command execution.
            cmd.CommandSyntax = "exists('" + varname + "')";
            o = analytics.ExecuteR(cmd, true, false);
            if (o.ToString().ToLower().Equals("false"))//possibly some error occured
            {
                string ewmessage = BSky.GlobalResources.Properties.Resources.ObjCantBSkyFormat + " " + firstparam + ", " + BSky.GlobalResources.Properties.Resources.DoesNotExist;
                SendErrorToOutput(originalCommand + "\n" + ewmessage, ow); //03Jul2013
                return; //return type was void before 22May2014
            }

            //Check if varname is null
            cmd.CommandSyntax = "is.null(" + varname + ")";
            o = analytics.ExecuteR(cmd, true, false);
            if (o.ToString().ToLower().Equals("true"))//possibly some error occured
            {
                string ewmessage = BSky.GlobalResources.Properties.Resources.ObjCantBSkyFormat + " " + firstparam + ", " + BSky.GlobalResources.Properties.Resources.isNull;
                SendErrorToOutput(originalCommand + "\n" + ewmessage, ow); //03Jul2013
                return; //return type was void before 22May2014
            }

            bsky_no_row_header = false;
            cmd.CommandSyntax = "is.null(row.names(" + varname + ")[1])";
            o = analytics.ExecuteR(cmd, true, false);
            if (o.ToString().ToLower().Equals("false"))//row name at [1] exists
            {
                cmd.CommandSyntax = "row.names(" + varname + ")[1]";
                o = analytics.ExecuteR(cmd, true, false);
                if (o.ToString().Trim().ToLower().Equals("bsky_no_row_header"))
                {
                    bsky_no_row_header = true;
                }
            }

            //one mandatory parameter
            string mandatoryparamone = ", bSkyFormatAppRequest = TRUE";

            if (restparams.Trim().Length > 0 && restparams.Trim().Contains("bSkyFormatAppRequest"))
            {
                mandatoryparamone = string.Empty;
            }

            //second mandatory parameter
            string mandatoryparamtwo = ", singleTableOutputHeader = '" + headername + "'"; 

            if (restparams.Trim().Length > 0 && restparams.Trim().Contains("singleTableOutputHeader"))
            {
                mandatoryparamtwo = string.Empty;
            }

            //create BSkyFormat command for execution and execute
            if (restparams.Trim().Length > 0)
                stmt = "BSkyFormat(" + varname + mandatoryparamone + mandatoryparamtwo + "," + restparams + ")";
            else
                stmt = "BSkyFormat(" + varname + mandatoryparamone + mandatoryparamtwo + " )";

            stmt = BSkyLeftVar + stmt;
            /// BSkyLeftVar <- can be blank if user has no assigned leftvar to BSkyFormat

            if (AdvancedLogging) logService.WriteToLogLevel("ExtraLogs: Command reconstructed : " + stmt, LogLevelEnum.Info);

            string objclass = "", objectname = "";

            if (stmt.Contains("BSkyFormat("))// Array, Matrix, Data Frame or BSkyObject(ie..Analytic commands)
            {
                bskyfrmtobjcount++;

                stmt = "bskyfrmtobj <- " + stmt; 
                objectname = "bskyfrmtobj";
                cmd.CommandSyntax = stmt;// command 
                o = analytics.ExecuteR(cmd, false, false);//executing BSkyFormat

                ///Check if returned object is null
                cmd.CommandSyntax = "is.null(" + objectname + ")";
                o = analytics.ExecuteR(cmd, true, false);
                if (o.ToString().ToLower().Equals("true"))//possibly some error occured
                {
                    string ewmessage = BSky.GlobalResources.Properties.Resources.ObjCantBSkyFormat2 + "\n " + BSky.GlobalResources.Properties.Resources.BSkyFormatSupportedTypes;
                    SendErrorToOutput(originalCommand + "\n" + ewmessage, ow); //03Jul2013
                    return; //return type was void before 22May2014
                }

                #region Generate UI for data.frame/ matrix / array and analytics commands
                if (BSkyLeftVar.Trim().Length < 1) // if left var does not exist then generate UI tables
                {
                    lst.NameOfAnalysis = originalCommand.Contains("BSkyFormat") ? "BSkyFormat-Command" : originalCommand;
                    //cmd.CommandSyntax = "class(bskyfrmtobj" + bskyfrmtobjcount.ToString() + ")";
                    cmd.CommandSyntax = "class(bskyfrmtobj)";
                    objclass = (string)analytics.ExecuteR(cmd, true, false);

                    if (AdvancedLogging) logService.WriteToLogLevel("ExtraLogs: BSkyFormat object type : " + objclass, LogLevelEnum.Info);

                    //Following 2 lines commented because we o not want to show "data.frame", "matrix", "array" as headers.
                    //if (headername.Trim().Length < 1)//13Aug2012
                    //    headername = objclass.ToString();
                    if (objclass.ToString().ToLower().Equals("data.frame") || objclass.ToString().ToLower().Equals("matrix") || objclass.ToString().ToLower().Equals("array"))
                    {
                        //lst.NameOfAnalysis = originalCommand;//for tree Parent node 07Aug2012
                        if (headername != null && headername.Trim().Length < 1) //06May2014
                            headername = subcomm;
                        if (AdvancedLogging) logService.WriteToLogLevel("ExtraLogs: BSkyFormatting DF/Matrix/Arr : " + objclass, LogLevelEnum.Info);
                        BSkyFormatDFMtxArr(lst, objectname, headername, ow);
                    }
                    else if (objclass.ToString().ToLower().Equals("list"))//BSkyObject returns "list"
                    {
                        if (AdvancedLogging) logService.WriteToLogLevel("ExtraLogs: BSkyFormatting : " + objclass, LogLevelEnum.Info);
                        //if (ow != null)//22May2014
                        SendToOutput("", ref lst, ow);
                        ///tetsing whole else if
                        objectname = "bskyfrmtobj";// +bskyfrmtobjcount.ToString();
                        //cmd.CommandSyntax = objectname + "$uasummary[[7]]";//"bsky.histogram(colNameOrIndex=c('age'), dataSetNameOrIndex='Dataset1')";//

                        cmd.CommandSyntax = "is.null(" + objectname + "$BSkySplit)";//$BSkySplit or $split in return structure
                        if (AdvancedLogging) logService.WriteToLogLevel("ExtraLogs: Executing : " + cmd.CommandSyntax, LogLevelEnum.Info);
                        bool isNonBSkyList1 = false;
                        object isNonBSkyList1str = analytics.ExecuteR(cmd, true, false);

                        if (isNonBSkyList1str != null && isNonBSkyList1str.ToString().ToLower().Equals("true"))
                        {
                            isNonBSkyList1 = true;
                        }
                        cmd.CommandSyntax = "is.null(" + objectname + "$list2name)";//another type pf BSky list
                        if (AdvancedLogging) logService.WriteToLogLevel("ExtraLogs: Executing : " + cmd.CommandSyntax, LogLevelEnum.Info);

                        bool isNonBSkyList2 = false;
                        object isNonBSkyList2str = analytics.ExecuteR(cmd, true, false);

                        if (isNonBSkyList2str != null && isNonBSkyList2str.ToString().ToLower().Equals("true"))
                        {
                            isNonBSkyList2 = true;
                        }

                        if (!isNonBSkyList1)
                        {
                            //if there was error in execution, say because non scale field has scale variable
                            // so now if we first check if $executionstatus = -2, we know that some error has occured.
                            cmd.CommandSyntax = objectname + "$executionstatus";//$BSkySplit or $split in return structure
                            if (AdvancedLogging) logService.WriteToLogLevel("ExtraLogs: Executing : " + cmd.CommandSyntax, LogLevelEnum.Info);

                            object errstat = analytics.ExecuteR(cmd, true, false);

                            if (errstat != null && (errstat.ToString().ToLower().Equals("-2") || errstat.ToString().ToLower().Equals("-1")))
                            {
                                if (AdvancedLogging) logService.WriteToLogLevel("ExtraLogs: Execution Stat : " + errstat, LogLevelEnum.Info);

                                //if (errstat.ToString().ToLower().Equals("-2"))
                                //    SendErrorToOutput("Critical Error Occurred!", ow);//15Jan2015

                                if (errstat.ToString().ToLower().Equals("-2"))
                                    SendErrorToOutput(BSky.GlobalResources.Properties.Resources.CriticalError, ow);//15Jan2015

                                else
                                    SendErrorToOutput(BSky.GlobalResources.Properties.Resources.ErrorOccurred, ow);//03Jul2013
                            }

                            cmd.CommandSyntax = objectname + "$nooftables";//$nooftables=0, means no data to display. Not even error warning tables are there.
                            if (AdvancedLogging) logService.WriteToLogLevel("ExtraLogs: Executing : " + cmd.CommandSyntax, LogLevelEnum.Info);

                            object retval = analytics.ExecuteR(cmd, true, false);

                            if (retval != null && retval.ToString().ToLower().Equals("0"))
                            {
                                if (AdvancedLogging) logService.WriteToLogLevel("ExtraLogs: No of Tables : " + retval, LogLevelEnum.Info);
                                SendErrorToOutput(BSky.GlobalResources.Properties.Resources.NoTablesReturned, ow);//03Jul2013
                            }

                            if (AdvancedLogging) logService.WriteToLogLevel("ExtraLogs: Start creating actual UI tables : ", LogLevelEnum.Info);
                            //finally we can now format the tables of BSkyReturnStruture list
                            RefreshOutputORDataset(objectname, cmd, originalCommand, ow); //list like one sample etc..
                        }
                        else if (!isNonBSkyList2)
                        {
                            //if (ow != null)//22May2014
                            FormatBSkyList2(lst, objectname, headername, ow); //list that contains tables 2nd location onwards
                        }
                    }
                    else // invalid format
                    {
                        /// put it in right place
                        string ewmessage = BSky.GlobalResources.Properties.Resources.CantBSkyFormatErrorMsg;
                        //if (ow != null)//22May2014
                        SendErrorToOutput(originalCommand + "\n" + ewmessage, ow);//03Jul2013
                    }
                }/// if leftvar is not assigned generate UI
                #endregion
            }
            return;//22May2014
        }

        //30Sep2014
        // This var must be removed from R memory otherwise when next BSkyFormat fails.
        private void KillTempBSkyFormatObj(string varname)
        {
            object o;
            CommandRequest cmd = new CommandRequest();
            //Find if bskytempvarname already exists. If it exists then remove from memory
            cmd.CommandSyntax = "tiscuni <- exists('" + varname + "')";//removing var so that old obj from last session is not present.
            o = analytics.ExecuteR(cmd, true, false);
            if (o.ToString().ToLower().Equals("true")) // if found, remove it
            {
                cmd.CommandSyntax = "rm('" + varname + "')";//removing var so that old obj from last session is not present.
                o = analytics.ExecuteR(cmd, false, false);
            }
        }

        private void ExecuteSinkBSkyFormatCommand(string stmt, ref int bskyfrmtobjcount, CommandOutput lst)
        {
            string originalCommand = stmt;
            lst.IsFromSyntaxEditor = true;//lst belongs to Syn Editor
            if (saveoutput.IsChecked == true)//10Jan2013
                lst.SelectedForDump = true;

            object o;
            CommandRequest cmd = new CommandRequest();

            string subcomm = string.Empty, varname = string.Empty, BSkyLeftVar = string.Empty, headername = string.Empty;
            SplitBSkyFormat(stmt, out subcomm, out varname, out BSkyLeftVar);
            //string leftvarname = BSkyLeftVar;//UI ll not be generated in LeftVar case so no need for table header either
            if (BSkyLeftVar.Trim().Length > 0) // if left var exists
            {
                BSkyLeftVar = BSkyLeftVar + " <- "; // so that BSkyLeftVar <- BSkyFormat(...) )
            }
            ////now execute subcomm first then pass varname in BSkyFormat(varname)
            if (varname.Length > 0)
            {
                headername = varname.Trim();
                cmd.CommandSyntax = subcomm;// command: osmt<-one.smt.tt(...)
                if (!varname.Equals(subcomm))
                    o = analytics.ExecuteR(cmd, false, false);
            }
            else 
            {
                /////25Feb2013 for writing errors in OutputWindow////
                string sinkfilename = confService.GetConfigValueForKey("tempsink");//23nov2012
                string sinkfilefullpathname = Path.Combine(BSkyAppData.RoamingUserBSkyTempPath, sinkfilename);
                // load default value if no path is set or invalid path is set
                if (sinkfilefullpathname.Trim().Length == 0 || !IsValidFullPathFilename(sinkfilefullpathname, false))
                {
                    MessageBox.Show(this, BSky.GlobalResources.Properties.Resources.tempsinkConfigKeyNotFound);
                    return;
                }

                ////////////////////////////////////////////////////////////////////////
                varname = "bskytempvarname";
                //Find if bskytempvarname already exists. If it exists then remove from memory
                cmd.CommandSyntax = "exists('" + varname + "')";//removing var so that old obj from last session is not present.
                o = analytics.ExecuteR(cmd, true, false);
                if (o.ToString().ToLower().Equals("true")) // if found, remove it
                {
                    cmd.CommandSyntax = "rm('" + varname + "')";//removing var so that old obj from last session is not present.
                    o = analytics.ExecuteR(cmd, false, false);
                }

                //Now run command
                cmd.CommandSyntax = varname + " <- " + subcomm;// command: varname <- one.smt.tt(...)
                o = analytics.ExecuteR(cmd, false, false);//executing sub-command
                ////////////////////////////////////////////////////////////////////////
            }
            //if var does not exist then there could be error in command execution.
            cmd.CommandSyntax = "exists('" + varname + "')";
            o = analytics.ExecuteR(cmd, true, false);
            if (o.ToString().ToLower().Equals("false"))//possibly some error occured
            {
                string ewmessage = BSky.GlobalResources.Properties.Resources.ObjCantBSkyFormat + " " + varname + ", " + BSky.GlobalResources.Properties.Resources.DoesNotExist;
                SendErrorToOutput(originalCommand + "\n" + ewmessage, ow); //03Jul2013
                return;
            }

            if (subcomm.Contains("BSkyQueue"))//BSkyFormat already ran and this Q has the table. We just need to extract the table
            {
                stmt = subcomm;
            }
            else //for other cases BSkyForamt( onssm() ) or BSkyFormat(res<-onesm())
            {
                stmt = "BSkyFormat(" + varname + ", bSkyFormatAppRequest = TRUE)";

                stmt = BSkyLeftVar + stmt;// command is BSkyLeftVar <- BSkyFormat(varname)
            }
            string objclass = "", objectname = "";

            if (stmt.Contains("BSkyFormat(") || stmt.Contains("BSkyQueue"))// Array, Matrix, Data Frame or BSkyObject(ie..Analytic commands)
            {
                bskyfrmtobjcount++;
                stmt = "bskyfrmtobj <- " + stmt;
                objectname = "bskyfrmtobj";
                cmd.CommandSyntax = stmt;// command 

                o = analytics.ExecuteR(cmd, false, false);//executing syntax editor commands

                #region Generate UI for data.frame/ matrix / array and analytics commands
                if (BSkyLeftVar.Trim().Length < 1) // if left var does not exist then generate UI tables
                {
                    lst.NameOfAnalysis = originalCommand.Contains("BSkyFormat") ? "BSkyFormat-Command" : originalCommand;
                    cmd.CommandSyntax = "class(bskyfrmtobj)";
                    objclass = (string)analytics.ExecuteR(cmd, true, false);


                    if (objclass.ToString().ToLower().Equals("data.frame") || objclass.ToString().ToLower().Equals("matrix") || objclass.ToString().ToLower().Equals("array"))
                    {
                        //lst.NameOfAnalysis = originalCommand;//for tree Parent node 07Aug2012
                        if (headername != null && headername.Trim().Length < 1) //06May2014
                        {
                            headername = subcomm;
                        }

                        BSkyFormatDFMtxArr(lst, objectname, headername, null);
                    }
                    else if (objclass.ToString().Equals("list"))//BSkyObject returns "list"
                    {
                        SendToOutput("", ref lst, ow);
                        objectname = "bskyfrmtobj";

                        cmd.CommandSyntax = "is.null(" + objectname + "$BSkySplit)";//$BSkySplit or $split in return structure

                        bool isNonBSkyList1 = false;
                        object isNonBSkyList1str = analytics.ExecuteR(cmd, true, false);

                        if (isNonBSkyList1str != null && isNonBSkyList1str.ToString().ToLower().Equals("true"))
                        {
                            isNonBSkyList1 = true;
                        }
                        cmd.CommandSyntax = "is.null(" + objectname + "$list2name)";//another type pf BSky list

                        bool isNonBSkyList2 = false;
                        object isNonBSkyList2str = analytics.ExecuteR(cmd, true, false);

                        if (isNonBSkyList2str != null && isNonBSkyList2str.ToString().ToLower().Equals("true"))
                        {
                            isNonBSkyList2 = true;
                        }

                        /////////////////
                        if (!isNonBSkyList1)
                        {
                            RefreshOutputORDataset(objectname, cmd, originalCommand, ow); //list like one sample etc..
                        }
                        else if (!isNonBSkyList2)
                        {
                            FormatBSkyList2(lst, objectname, headername, ow); //list that contains tables 2nd location onwards
                        }
                    }
                    else // invalid format
                    {
                        /// put it in right place
                        string ewmessage = BSky.GlobalResources.Properties.Resources.CantBSkyFormatErrorMsg;
                        SendErrorToOutput(originalCommand + "\n" + ewmessage, ow);//03Jul2013
                    }
                }/// if leftvar is not assigned generate UI
                #endregion
            }
        }

        private bool ExecuteBSkyLoadRefreshDataframe(string stmt, string fname)//13Feb2014

        {
            CommandRequest cmd = new CommandRequest();
            int start = stmt.IndexOf('(');
            int end = stmt.IndexOf(')');

            string parameters = stmt.Substring(start + 1, end - start - 1);

            string[] eachparam = parameters.Split(',');
            string dataframename = string.Empty;
            string boolparam = string.Empty;

            if (eachparam.Length == 2)
            {
                //either of the two is dataframe name
                if (eachparam[1].Contains("load.dataframe") || eachparam[1].Contains("TRUE") || eachparam[1].Contains("FALSE"))
                {
                    dataframename = eachparam[0];
                    boolparam = eachparam[1];
                }
                else // if bool is passed as first parameter and dataframe name as second param.
                {
                    dataframename = eachparam[1];
                    boolparam = eachparam[0];
                }
            }
            else if (eachparam.Length == 1)//only one madatory param is passed which is dataframe name
            {
                dataframename = eachparam[0];
                if (dataframename.Trim().Equals("load.dataframe") || dataframename.Trim().Equals("FALSE") || dataframename.Trim().Equals("TRUE"))//Dataframe parameter not passed.
                {
                    dataframename = string.Empty;
                }
                boolparam = "TRUE";
            }
            /////get dataframe name
            if (dataframename.Contains("="))
            {
                dataframename = dataframename.Substring(dataframename.IndexOf("=") + 1);
            }
            dataframename = dataframename.Trim();

            //09Jun2015 if dataframename is not passed that means there is no need to load/refresh dataframe
            if (dataframename.Length < 1)
            {
                return true;
            }

            ///get bool parama value
            if (boolparam.Contains("="))//dframe="Dataset1"
            {
                boolparam = boolparam.Substring(boolparam.IndexOf("=") + 1);
            }
            boolparam = boolparam.Trim();

            //27Feb2017 check boolparam's value from R
            cmd.CommandSyntax = boolparam;//check if boolparam is TRUE or FALSE

            object oloaddf = analytics.ExecuteR(cmd, true, false);

            if (oloaddf.ToString().ToLower().Equals("true"))
            {
                boolparam = "TRUE";
            }
            else
            {
                boolparam = "FALSE";
            }

            if (boolparam.Contains("FALSE"))//do not refresh dataframe
            {
                return true;
            }

            cmd.CommandSyntax = "exists('" + dataframename + "')";//check if that dataset exists in memory.

            object o1 = analytics.ExecuteR(cmd, true, false);

            if (o1.ToString().ToLower().Equals("true")) // if found, check if data.frame type. then load it
            {
                //27Feb2017 Refresh NULL dataset. 
                cmd.CommandSyntax = "is.null(" + dataframename + ")";//check if dataframe object is null

                object odsnull = analytics.ExecuteR(cmd, true, false);

                if (odsnull.ToString().ToLower().Equals("true")) // if data.frame is null
                {
                    cmd.CommandSyntax = "length(which('" + dataframename + "'==uadatasets$name)) > 0";//check if dataframe existed before

                    object odsidx = analytics.ExecuteR(cmd, true, false);

                    if (odsidx.ToString().ToLower().Equals("true")) // if data.frame index is found
                    {
                        IUIController UIController;
                        UIController = LifetimeService.Instance.Container.Resolve<IUIController>();

                        DataSource oldDs = UIController.GetDocumentByName(dataframename);//oldDs help us to get the filename

                        DataSource ds = new DataSource();
                        ds.Variables = new List<DataSourceVariable>();
                        ds.FileName = oldDs.FileName;
                        ds.Name = dataframename;
                        ds.SheetName = "";

                        ds.DecimalCharacter = oldDs.DecimalCharacter;
                        ds.FieldSeparator = oldDs.FieldSeparator;
                        ds.HasHeader = oldDs.HasHeader;
                        ds.IsBasketData = oldDs.IsBasketData;

                        UIController.RefreshBothGrids(ds);

                        return false;
                    }
                    else
                    {
                        SendErrorToOutput(BSky.GlobalResources.Properties.Resources.NotdataframeType, ow);
                        return false;
                    }
                }
                else
                {
                    cmd.CommandSyntax = "is.data.frame(" + dataframename + ")";//check if its 'data.frame' type.
                    object o2 = analytics.ExecuteR(cmd, true, false);

                    if (o2.ToString().ToLower().Equals("true")) // if data.frame type
                    {
                        FileOpenCommand foc = new FileOpenCommand();
                        return foc.OpenDataframe(dataframename, fname);
                    }
                    else
                    {
                        SendErrorToOutput(BSky.GlobalResources.Properties.Resources.NotdataframeType, ow);
                        return false;
                    }
                }
            }
            else
            {
                SendErrorToOutput(BSky.GlobalResources.Properties.Resources.DatasetObjNotExists, ow);
                return false;
            }
        }

        private void ExecuteBSkyRemoveRefreshDataframe(string stmt)//20Feb2014
        {
            int start = stmt.IndexOf('(');
            int end = stmt.IndexOf(')');

            string dataframename = stmt.Substring(start + 1, end - start - 1);
            FileCloseCommand fcc = new FileCloseCommand();
            fcc.CloseDatasetFromSyntax(dataframename);
        }

        private void ExecuteXMLTemplateDefinedCommands(string stmt)//10Jul2014
        {
            CommandRequest xmlgrpcmd = new CommandRequest();

            xmlgrpcmd.CommandSyntax = stmt;

            UAMenuCommand uamc = new UAMenuCommand();
            uamc.bskycommand = stmt;
            uamc.commandtype = stmt;
            CommandExecutionHelper auacb = new CommandExecutionHelper();
            auacb.MenuParameter = menuParameter;
            auacb.RetVal = analytics.Execute(xmlgrpcmd);
            auacb.ExecuteXMLDefinedDialog(stmt);
        }
        //pulled out from ExecuteBSkyFromatCommand() method above. For BskyFormat DataFrame Matrix Array
        private void BSkyFormatDFMtxArr(CommandOutput lst, string objectname, string headername, OutputWindow ow)
        {
            CommandRequest cmddf = new CommandRequest();
            int dimrow = 1, dimcol = 1;
            bool rowexists = false, colexists = false;
            string dataclassname = string.Empty;

            //Find class of data passed. data.frame, matrix, or array
            cmddf.CommandSyntax = "class(" + objectname + ")"; // Row exists
            object retres = analytics.ExecuteR(cmddf, true, false);

            if (retres != null)
                dataclassname = retres.ToString();

            //find if dimension exists
            cmddf.CommandSyntax = "!is.na(dim(" + objectname + ")[1])"; // Row exists
            retres = analytics.ExecuteR(cmddf, true, false);
            if (retres != null && retres.ToString().ToLower().Equals("true"))
                rowexists = true;
            cmddf.CommandSyntax = "!is.na(dim(" + objectname + ")[2])";// Col exists
            retres = analytics.ExecuteR(cmddf, true, false);
            if (retres != null && retres.ToString().ToLower().Equals("true"))
                colexists = true;
            /// Find size of matrix(objectname) & initialize data array ///
            if (rowexists)
            {
                cmddf.CommandSyntax = "dim(" + objectname + ")[1]";
                retres = analytics.ExecuteR(cmddf, true, false);
                if (retres != null)
                    dimrow = Int16.Parse(retres.ToString());
            }
            if (colexists)
            {
                cmddf.CommandSyntax = "dim(" + objectname + ")[2]";
                retres = analytics.ExecuteR(cmddf, true, false);
                if (retres != null)
                    dimcol = Int16.Parse(retres.ToString());
            }

            string[,] data = new string[dimrow, dimcol];
            //// now create FlexGrid and add to lst ///
            /////////finding Col headers /////
            cmddf.CommandSyntax = "colnames(" + objectname + ")";
            object colhdrobj = analytics.ExecuteR(cmddf, true, false);
            string[] colheaders;

            if (colhdrobj != null && !colhdrobj.ToString().Contains("Error"))
            {
                if (colhdrobj.GetType().IsArray)
                    colheaders = (string[])colhdrobj;//{ "Aa", "Bb", "Cc" };//
                else
                {
                    colheaders = new string[1];
                    colheaders[0] = colhdrobj.ToString();
                }
            }
            else
            {
                colheaders = new string[dimcol];
                for (int i = 0; i < dimcol; i++)
                    colheaders[i] = (i + 1).ToString();
            }

            /////////finding Row headers /////

            string numrowheader = confService.AppSettings.Get("numericrowheaders");
            // load default value if no value is set 
            if (numrowheader.Trim().Length == 0)
                numrowheader = confService.DefaultSettings["numericrowheaders"];

            bool shownumrowheaders = numrowheader.ToLower().Equals("true") ? true : false; /// 

            cmddf.CommandSyntax = "rownames(" + objectname + ")";
            object rowhdrobj = analytics.ExecuteR(cmddf, true, false);
            string[] rowheaders;

            if (rowhdrobj != null && !rowhdrobj.ToString().Contains("Error"))
            {
                if (rowhdrobj.GetType().IsArray)
                    rowheaders = (string[])rowhdrobj;
                else
                {
                    rowheaders = new string[1];
                    rowheaders[0] = rowhdrobj.ToString();
                }
            }
            else
            {
                rowheaders = new string[dimrow];
                for (int i = 0; i < dimrow; i++)
                    rowheaders[i] = (i + 1).ToString();
            }

            bool isnumericrowheaders = true; // assuming that row headers are numeric
            short tnum;

            for (int i = 0; i < dimrow; i++)
            {
                if (!Int16.TryParse(rowheaders[i], out tnum))
                {
                    isnumericrowheaders = false; //row headers are non-numeric
                    break;
                }
            }

            if (isnumericrowheaders && !shownumrowheaders)
            {
                for (int i = 0; i < dimrow; i++)
                    rowheaders[i] = "";
            }

            /// Populating array using data frame data
            bool isRowEmpty = true;//for Virtual. 
            int emptyRowCount = 0;//for Virtual. 
            List<int> emptyRowIndexes = new List<int>(); //for Virtual.
            string cellData = string.Empty;

            for (int r = 1; r <= dimrow; r++)
            {
                isRowEmpty = true;//for Virtual. 
                for (int c = 1; c <= dimcol; c++)
                {
                    if (dimcol == 1 && !dataclassname.ToLower().Equals("data.frame"))
                        cmddf.CommandSyntax = "as.character(" + objectname + "[" + r + "])";
                    else
                        cmddf.CommandSyntax = "as.character(" + objectname + "[" + r + "," + c + "])";

                    object v = analytics.ExecuteR(cmddf, true, false);
                    cellData = (v != null) ? v.ToString().Trim() : "";
                    data[r - 1, c - 1] = cellData;// v.ToString().Trim();

                    if (cellData.Length > 0)
                        isRowEmpty = false;
                }

                //for Virtual. // counting empty rows for virtual
                if (isRowEmpty)
                {
                    emptyRowCount++;
                    emptyRowIndexes.Add(r - 1);//making it zero based as in above nested 'for'
                }
            }

            // whether you want C1Flexgrid to be generated by using XML DOM or by Virtual class(Dynamic)
            bool DOMmethod = false;

            if (DOMmethod)
            {
                //12Aug2014 Old way of creating grid using DOM and then creating and filling grid step by step
                XmlDocument xdoc = createFlexGridXmlDoc(colheaders, rowheaders, data);
                createFlexGrid(xdoc, lst, headername);// headername = 'varname' else 'leftvarname' else 'objclass'
            }
            else//virutal list method
            {
                if (emptyRowCount > 0)
                {
                    int nonemptyrowcount = dimrow - emptyRowCount;
                    string[,] nonemptyrowsdata = new string[nonemptyrowcount, dimcol];
                    string[] nonemptyrowheaders = new string[nonemptyrowcount];

                    for (int rr = 0, rrr = 0; rr < data.GetLength(0); rr++)
                    {
                        if (emptyRowIndexes.Contains(rr))//skip empty rows.
                            continue;
                        for (int cc = 0; cc < data.GetLength(1); cc++)
                        {
                            nonemptyrowsdata[rrr, cc] = data[rr, cc];//only copy non-empty rows
                        }
                        nonemptyrowheaders[rrr] = rowheaders[rr];//copying row headers too.
                        rrr++;
                    }
                    //Using Dynamic Class creation and then populating the grid. //12Aug2014
                    CreateDynamicClassFlexGrid(headername, colheaders, nonemptyrowheaders, nonemptyrowsdata, lst);
                }
                else
                {
                    //Using Dynamic Class creation and then populating the grid. //12Aug2014
                    CreateDynamicClassFlexGrid(headername, colheaders, rowheaders, data, lst);
                }
            }
            if (ow != null)//22May2014
                SendToOutput("", ref lst, ow);//send dataframe/matrix/array to output window or disk file
        }

        REngine engine;

        private void InitializeRDotNet()
        {
            REngine.SetEnvironmentVariables();
            engine = REngine.GetInstance();
            // REngine requires explicit initialization.
            // You can set some parameters.
            engine.Initialize();
            //load BSky and R packages
            engine.Evaluate("library(BlueSky)");
            engine.Evaluate("library(foreign)");
            engine.Evaluate("library(data.table)");
            engine.Evaluate("library(RODBC)");
            engine.Evaluate("library(car)");
            engine.Evaluate("library(aplpack)");
            engine.Evaluate("library(mgcv)");
            engine.Evaluate("library(rgl)");
            engine.Evaluate("library(gmodels)");
        }

        private void RDotNetOpenDataset()
        {
            //open dataset
            engine.Evaluate("d2 <- BSkyloadDataset('D:/BlueSky/Projects/Xtras_Required/Data_Files/cars.sav',  filetype='SPSS', worksheetName=NULL, replace_ds=FALSE, csvHeader=TRUE, datasetName='Dataset1' )");
        }

        private void DisposeRDotNet()
        {
            engine.Dispose();
        }

        private void RDotNetExecute(OutputWindow ow)
        {
            CommandOutput lst = new CommandOutput();
            lst.IsFromSyntaxEditor = true;

            engine.Evaluate("BSky_One_Way_Anova = as.data.frame (summary(dd <- aov(mpg ~ year,data=Dataset1))[[1]])");
            engine.Evaluate("bskyfrmtobj <- BSkyFormat(BSky_One_Way_Anova)");
            CharacterMatrix cmatrix = engine.Evaluate("bskyfrmtobj").AsCharacterMatrix();
            string[,] mtx = new string[cmatrix.RowCount, cmatrix.ColumnCount];

            for (int r = 0; r < cmatrix.RowCount; r++)
            {
                for (int c = 0; c < cmatrix.ColumnCount; c++)
                {
                    mtx[r, c] = cmatrix[r, c];
                }
            }

            string objectname = "bskyfrmtobj";
            string headername = "This is generated in R.NET";

            CommandRequest cmddf = new CommandRequest();
            int dimrow = 1, dimcol = 1;
            bool rowexists = false, colexists = false;
            string dataclassname = string.Empty;

            //Find class of data passed. data.frame, matrix, or array
            cmddf.CommandSyntax = "class(" + objectname + ")"; // Row exists
            object retres = engine.Evaluate(cmddf.CommandSyntax).AsCharacter()[0];

            if (retres != null)
                dataclassname = retres.ToString();

            //find if dimension exists
            cmddf.CommandSyntax = "!is.na(dim(" + objectname + ")[1])"; // Row exists
            rowexists = engine.Evaluate(cmddf.CommandSyntax).AsLogical()[0];

            cmddf.CommandSyntax = "!is.na(dim(" + objectname + ")[2])";// Col exists
            colexists = engine.Evaluate(cmddf.CommandSyntax).AsLogical()[0];

            /// Find size of matrix(objectname) & initialize data array ///
            if (rowexists)
            {
                cmddf.CommandSyntax = "dim(" + objectname + ")[1]";
                retres = engine.Evaluate(cmddf.CommandSyntax).AsInteger()[0];
                if (retres != null)
                    dimrow = Int16.Parse(retres.ToString());
            }
            if (colexists)
            {
                cmddf.CommandSyntax = "dim(" + objectname + ")[2]";
                retres = engine.Evaluate(cmddf.CommandSyntax).AsInteger()[0];
                if (retres != null)
                    dimcol = Int16.Parse(retres.ToString());
            }

            string[,] data = new string[dimrow, dimcol];
            //// now create FlexGrid and add to lst ///
            /////////finding Col headers /////
            cmddf.CommandSyntax = "colnames(" + objectname + ")";
            CharacterVector colhdrobj = engine.Evaluate(cmddf.CommandSyntax).AsCharacter();
            string[] colheaders;

            if (colhdrobj != null && !colhdrobj.ToString().Contains("Error"))
            {
                if (true)//colhdrobj.GetType().IsArray)
                {
                    int siz = colhdrobj.Count();
                    colheaders = new string[siz];
                    for (int ri = 0; ri < siz; ri++)
                    {
                        colheaders[ri] = colhdrobj[ri];
                    }
                }
                else
                {
                    colheaders = new string[1];
                    colheaders[0] = colhdrobj.ToString();
                }
            }
            else
            {
                colheaders = new string[dimcol];
                for (int i = 0; i < dimcol; i++)
                    colheaders[i] = (i + 1).ToString();
            }

           //read configuration and then decide to pull row headers

            bool shownumrowheaders = true; /// 

            cmddf.CommandSyntax = "rownames(" + objectname + ")";
            CharacterVector rowhdrobj = engine.Evaluate(cmddf.CommandSyntax).AsCharacter();
            string[] rowheaders;

            if (rowhdrobj != null && !rowhdrobj.ToString().Contains("Error"))
            {
                if (true)
                {
                    int siz = rowhdrobj.Count();
                    rowheaders = new string[siz];
                    for (int ri = 0; ri < siz; ri++)
                    {
                        rowheaders[ri] = rowhdrobj[ri];
                    }
                }
                else
                {
                    rowheaders = new string[1];
                    rowheaders[0] = rowhdrobj.ToString();
                }
            }
            else
            {
                rowheaders = new string[dimrow];
                for (int i = 0; i < dimrow; i++)
                    rowheaders[i] = (i + 1).ToString();
            }

            bool isnumericrowheaders = true; // assuming that row headers are numeric
            short tnum;

            for (int i = 0; i < dimrow; i++)
            {
                if (!Int16.TryParse(rowheaders[i], out tnum))
                {
                    isnumericrowheaders = false; //row headers are non-numeric
                    break;
                }
            }

            if (isnumericrowheaders && !shownumrowheaders)
            {
                for (int i = 0; i < dimrow; i++)
                    rowheaders[i] = "";
            }

            /// Populating array using data frame data
            bool isRowEmpty = true;//for Virtual. 
            int emptyRowCount = 0;//for Virtual. 
            List<int> emptyRowIndexes = new List<int>(); //for Virtual.
            string cellData = string.Empty;

            for (int r = 1; r <= dimrow; r++)
            {
                isRowEmpty = true;//for Virtual. 
                for (int c = 1; c <= dimcol; c++)
                {
                    if (dimcol == 1 && !dataclassname.ToLower().Equals("data.frame"))
                        cmddf.CommandSyntax = "as.character(" + objectname + "[" + r + "])";
                    else
                        cmddf.CommandSyntax = "as.character(" + objectname + "[" + r + "," + c + "])";

                    object v = engine.Evaluate(cmddf.CommandSyntax).AsCharacter()[0];
                    cellData = (v != null) ? v.ToString().Trim() : "";
                    data[r - 1, c - 1] = cellData;

                    if (cellData.Length > 0)
                        isRowEmpty = false;
                }

                //for Virtual. // counting empty rows for virtual
                if (isRowEmpty)
                {
                    emptyRowCount++;
                    emptyRowIndexes.Add(r - 1);//making it zero based as in above nested 'for'
                }
            }

            // whether you want C1Flexgrid to be generated by using XML DOM or by Virtual class(Dynamic)
            bool DOMmethod = false;

            if (DOMmethod)
            {
                //12Aug2014 Old way of creating grid using DOM and then creating and filling grid step by step
                XmlDocument xdoc = createFlexGridXmlDoc(colheaders, rowheaders, data);
                createFlexGrid(xdoc, lst, headername);// headername = 'varname' else 'leftvarname' else 'objclass'
            }
            else//virutal list method
            {
                if (emptyRowCount > 0)
                {
                    int nonemptyrowcount = dimrow - emptyRowCount;
                    string[,] nonemptyrowsdata = new string[nonemptyrowcount, dimcol];
                    string[] nonemptyrowheaders = new string[nonemptyrowcount];

                    for (int rr = 0, rrr = 0; rr < data.GetLength(0); rr++)
                    {
                        if (emptyRowIndexes.Contains(rr))//skip empty rows.
                            continue;
                        for (int cc = 0; cc < data.GetLength(1); cc++)
                        {
                            nonemptyrowsdata[rrr, cc] = data[rr, cc];//only copy non-empty rows
                        }
                        nonemptyrowheaders[rrr] = rowheaders[rr];//copying row headers too.
                        rrr++;
                    }
                    //Using Dynamic Class creation and then populating the grid. //12Aug2014
                    CreateDynamicClassFlexGrid(headername, colheaders, nonemptyrowheaders, nonemptyrowsdata, lst);
                }
                else
                {
                    //Using Dynamic Class creation and then populating the grid. //12Aug2014
                    CreateDynamicClassFlexGrid(headername, colheaders, rowheaders, data, lst);
                }
            }
            if (ow != null)//22May2014
                SendToOutput("", ref lst, ow);//send dataframe/matrix/array to output window or disk file
        }
        //15Apr2014. List location 1 will have listname and number of tables it caontains. Location 2 onwards are tables. 1 table per location.
        private void FormatBSkyList2(CommandOutput lst, string objectname, string headername, OutputWindow ow) // for BSky list2 processing
        {
            MessageBox.Show(this, "BSkyList2 Processing... close this box");
        }

        private void SendErrorToOutput(string ewmessage, OutputWindow ow) //03Jul2013 error warning message
        {
            object o;
            CommandRequest cmd = new CommandRequest();
            string sinkfilename = confService.GetConfigValueForKey("tempsink");//23nov2012
            string sinkfilefullpathname = Path.Combine(BSkyAppData.RoamingUserBSkyTempPath, sinkfilename);
            // load default value if no path is set or invalid path is set
            if (sinkfilefullpathname.Trim().Length == 0 || !IsValidFullPathFilename(sinkfilefullpathname, false))
            {
                MessageBox.Show(this, BSky.GlobalResources.Properties.Resources.tempsinkConfigKeyNotFound);
                return;
            }
            OpenSinkFile(@sinkfilefullpathname, "wt"); //06sep2012
            SetSink(); //06sep2012

            cmd.CommandSyntax = "write(\"" + ewmessage + ".\",fp)";// command 
            o = analytics.ExecuteR(cmd, false, false);

            ResetSink();//06sep2012
            CloseSinkFile();//06sep2012
            CreateOuput(ow);//06sep2012
        }

        //18Dec2013 Sending command to output to make them appear different than command-output
        private void SendCommandToOutput(string command, string NameOfAnalysis)
        {
            string rcommcol = confService.GetConfigValueForKey("rcommcol");//23nov2012
            byte red = byte.Parse(rcommcol.Substring(3, 2), NumberStyles.HexNumber);
            byte green = byte.Parse(rcommcol.Substring(5, 2), NumberStyles.HexNumber);
            byte blue = byte.Parse(rcommcol.Substring(7, 2), NumberStyles.HexNumber);
            Color c = Color.FromArgb(255, red, green, blue);

            CommandOutput lst = new CommandOutput();
            lst.NameOfAnalysis = NameOfAnalysis; // left tree Parent
            AUParagraph Title = new AUParagraph();
            Title.Text = command; //right side contents
            Title.FontSize = BSkyStyler.BSkyConstants.TEXT_FONTSIZE;
            Title.textcolor = new SolidColorBrush(c);
            Title.ControlType = "Command"; // left tree child
            lst.Add(Title);

            AddToSession(lst);
        }

        private void SplitBSkyFormat(string command, out string sub, out string varname, out string BSkyLeftVar)
        {
            /// patterns is like: "BSkyFormat(var.name <- func.name("
            string pattern = @"BSkyFormat\(\s*[A-Za-z0-9_\.]+\s*(<-|=)\s*[A-Za-z0-9_\.]+\(";
            int firstindex = command.IndexOf('(');
            int lastindex = command.LastIndexOf(')');
            sub = "";
            varname = "";
            BSkyLeftVar = "";

            if (firstindex == -1 || lastindex == -1)//21May2014 This check is important to stop app crash.
            {
                return;
            }

            #region Finding what's passed in 'BSkyFormat(param)' as parameter.
            sub = command.Substring(firstindex + 1, lastindex - firstindex - 1);
            #endregion

            #region Finding Leftvar in Leftvar <- BSkyFormat
            ///See if BSkyFormat is assigned to any leftvar. eg.. leftvar <- BSkyFormat(....)
            int assignmentIndex, BSkyIndex;
            BSkyLeftVar = "";
            BSkyIndex = command.Trim().IndexOf("BSkyFormat");
            assignmentIndex = command.Trim().LastIndexOf("<-", BSkyIndex);
            if (assignmentIndex < 0)//arrow is not there
            {
                assignmentIndex = command.Trim().LastIndexOf("=", BSkyIndex);
            }
            if (assignmentIndex > 0)
            {
                ///find 'leftvar' name now
                BSkyLeftVar = command.Trim().Substring(0, assignmentIndex).Trim();
            }
            #endregion

            #region Finding varname in BSkyFormat(varname <- data.frame(...) )
            /// looking for parameter of type df <- data.frame(...)
            bool str = Regex.IsMatch(command, pattern);
            MatchCollection mc = Regex.Matches(command, pattern);

            int asnmntindex = 0;

            if (str)
            {
                asnmntindex = sub.IndexOf("<-");
                if (asnmntindex < 0)
                {
                    asnmntindex = sub.IndexOf('=');
                }
                varname = sub.Substring(0, asnmntindex);
            }
            else // for BSkyFormat(m) then varname = m 
            {
                pattern = @"\s*\G[A-Za-z0-9_\.]+\s*[^\(\)\:,;=]?";
                str = Regex.IsMatch(sub, pattern);
                mc = Regex.Matches(sub, pattern);
                if (str && mc.Count == 1 && (sub.Trim().Length == mc[0].ToString().Trim().Length))
                {
                    varname = mc[0].ToString();
                }
            }
            #endregion
            varname = varname.Trim();//01Jul2013
            return;
        }

        private void SplitBSkyFormatParams(string command, out string first, out string rest, out string usertitle)
        {
            #region remove extra spaces between BSkyFormat and (
            //command = command.Replace(" ", "");//remove white spaces
            string pattern = @"BSkyFormat\s*\(";
            command = Regex.Replace(command, pattern, "BSkyFormat(");
            #endregion
			
            string firstParam = string.Empty;//for object to be formatted
            string restParams = string.Empty;//for remaining params
            usertitle = string.Empty; //for title if passed in function call by the user.

            //find header/title
            int ttlidx = command.IndexOf("singleTableOutputHeader");
            int eqlidx = 0;
            int commaidx = 0;
            int closebracketidx = 0;
            int headerstartidx = 0;
            int headerendidx = 0;//modify this to right value

            if (ttlidx > 0) //if title is provided
            {
                eqlidx = command.IndexOf("=", ttlidx);
                if (eqlidx > ttlidx)
                {
                    headerstartidx = eqlidx + 1;
                    headerendidx = eqlidx + 1;//modify this to right value
                    commaidx = command.IndexOf(",", eqlidx);
                    closebracketidx = command.IndexOf(")", eqlidx);

                    if (commaidx > eqlidx) //comma is present
                    {
                        headerendidx = commaidx - 1;
                    }
                    else //comma not present so end marker is closing bracket
                    {
                        headerendidx = closebracketidx - 1;
                    }
                }
                usertitle = command.Substring(headerstartidx, (headerendidx - headerstartidx) + 1);
            }

            ///Logic Starts
            //look for first comma and split the parameters in two
            int paramstart = command.IndexOf("BSkyFormat(") + 11;//11 is the length of BSkyFormat(
            string allParams = command.Substring(paramstart, command.Length - 12); //ignore last ')'

            int indexOfComma = -1;
            int brackets = 0;

            for (int idx = 0; idx < allParams.Length; idx++)
            {
                if (allParams.ElementAt(idx) == '(')
                    brackets++;
                else if (allParams.ElementAt(idx) == ')')
                    brackets--;
                else if (brackets == 0 && allParams.ElementAt(idx) == ',')
                    indexOfComma = idx;
                else
                    continue;

                if (brackets == 0 && indexOfComma > 0)
                    break;
            }
            ///Logic Ends
            if (indexOfComma < 0) // comma not found, means no other params are in this func call.
            {
                first = allParams;
                rest = string.Empty;
            }
            else
            {
                first = allParams.Substring(0, indexOfComma);
                rest = allParams.Substring(indexOfComma + 1);
            }
        }

        private void ExecuteOtherCommand(OutputWindow ow, string stmt)
        {
            if (AdvancedLogging) logService.WriteToLogLevel("ExtraLogs: Before Executing in R.", LogLevelEnum.Info);

            #region Check open close brackets before executing
            string unbalmsg;

            if (!AreBracketsBalanced(stmt, out unbalmsg))//if unbalanced brackets
            {
                CommandRequest errmsg = new CommandRequest();
                string fullmsg = "Error : " + unbalmsg;
                errmsg.CommandSyntax = "write(\"" + fullmsg.Replace("<", "&lt;").Replace('"', '\'') + "\",fp)";//
                analytics.ExecuteR(errmsg, false, false); //for printing command in file
                return;
            }
            #endregion
            ///if command is for loading dataset //
            if (stmt.Contains("BSkyloadDataset("))
            {
                int indexofopening = stmt.IndexOf('(');
                int indexofclosing = stmt.LastIndexOf(')');//27Apr2016 fixed. there could be more closing parenthesis.  we want closing ')' of BSkloadDataset()
                string[] parameters = stmt.Substring(indexofopening + 1, indexofclosing - indexofopening - 2).Split(',');
                string filename = string.Empty;

                foreach (string s in parameters)
                {
                    if (s.Contains('/') || s.Contains('\\'))
                        filename = s.Replace('\"', ' ').Replace('\'', ' ');
                }
                if (filename.Contains("="))
                {
                    filename = filename.Substring(filename.IndexOf("=") + 1);
                }

                FileOpenCommand fo = new FileOpenCommand();
                fo.FileOpen(filename.Trim());
                return;
            }
            ///02Aug2016 if command is for close dataset //
            if (stmt.Contains("BSkycloseDataset("))
            {
                string enclosedWithin = string.Empty;

                if (stmt.Contains("'"))
                    enclosedWithin = "'";
                else
                {
                    if (stmt.Contains('"'))
                        enclosedWithin = "\"";
                }

                int indexofopening = stmt.IndexOf(enclosedWithin);
                int indexofclosing = stmt.LastIndexOf(enclosedWithin);//27Apr2016 fixed. there could be more closing parenthesis.  we want closing ')' of BSkloadDataset()
                string datasetname = stmt.Substring(indexofopening + 1, indexofclosing - indexofopening - 1).Trim();

                FileCloseCommand fc = new FileCloseCommand();
                fc.CloseDatasetFromSyntax(datasetname);
                return;
            }

            //09Aug2016 Adding mouse busy for commands below
            BSkyMouseBusyHandler.ShowMouseBusy();

            //This code is not supposed to do any sort processing. It just to puts sort images in col headers
            if (stmt.Contains("%>% arrange(")) 
            {
                //get asc colnames from command
                List<string> asccols = null;
                //get desc colnames from command
                List<string> desccols = null; 

                GetAscDescCols(stmt, out asccols, out desccols);

                CommandExecutionHelper che = new CommandExecutionHelper();
                che.SetSortColForSortIcon(asccols, desccols);
            }

            object o = null;
            CommandRequest cmd = new CommandRequest();

            if (stmt.Contains("BSkySortDataframe(") ||
                stmt.Contains("BSkyComputeExpression(") || stmt.Contains("BSkyRecode("))
            {
                //RefreshOutputORDataset("", cmd, stmt);//ExecuteSynEdtrNonAnalysis
                CommandExecutionHelper auacb = new CommandExecutionHelper();
                UAMenuCommand uamc = new UAMenuCommand();
                uamc.bskycommand = stmt;
                uamc.commandtype = stmt;
                auacb.ExeuteSingleCommandWithtoutXML(stmt);//auacb.ExecuteSynEdtrNonAnalysis(uamc);
                auacb.Refresh_Statusbar();
                //auacb = null;
            }
            else
            {
                if (AdvancedLogging) logService.WriteToLogLevel("ExtraLogs: Executing in R.", LogLevelEnum.Info);

                isVariable(ref stmt);///for checking if its variable then it must be enclosed within print();

                //27May2015 check if command is graphic and get its height width and then reopen graphic device with new dimensions
                if (lastcommandwasgraphic)//listed graphic

                {
                    if (imgDim != null && imgDim.overrideImgDim)
                    {
                        CloseGraphicsDevice();
                        OpenGraphicsDevice(imgDim.imagewidth, imgDim.imageheight); // get image dimenstion from external source for this particular graphic.
                    }
                }

                cmd.CommandSyntax = stmt;// command 
                o = analytics.ExecuteR(cmd, false, false);   //// get Direct Result and write in sink file

                CommandRequest cmdprn = new CommandRequest();

                if (o != null && o.ToString().Contains("Error"))//for writing some of the errors those are not taken care by sink.
                {
                    cmdprn.CommandSyntax = "write(\"" + o.ToString() + "\",fp)";
                    o = analytics.ExecuteR(cmdprn, false, false); /// for printing command in file

                    ///if there is error in assignment, like RHS caused error and LHS var is never updated
                    ///Better make LHS var null.
                    string lhvar = string.Empty; //these statements are commented as we should not set any var to NULL, R doesn't.
                    GetLeftHandVar(stmt, out lhvar);
                    if (lhvar != null)
                    {
                        cmd.CommandSyntax = lhvar + " <- NULL";// assign null 
                        o = analytics.ExecuteR(cmd, false, false);
                    }
                }
            }

            BSkyMouseBusyHandler.HideMouseBusy();//This must be executed. Even if something fails above.

            if (AdvancedLogging) logService.WriteToLogLevel("ExtraLogs: After Executing in R.", LogLevelEnum.Info);

        }

        private void GetAscDescCols(string commandstmt, out List<string> asccolnames, out List<string> desccolnames)
        {
            asccolnames = new List<string>();
            desccolnames = new List<string>();
            int strtidx = commandstmt.IndexOf("arrange(");

            if (strtidx == -1) //arrange not found
            {
                return;
            }
            strtidx = strtidx + 8; // 8 = arrange(

            int lastidx = commandstmt.LastIndexOf(")");
            int len = lastidx - strtidx;

            string substr = commandstmt.Substring(strtidx, len);
            char[] separator = new char[] { ',' };
            string[] allparams = substr.Split(separator); // here allparams are col names some may be enclosed in desc()
            //Now separate them in ascending and descending
            foreach (string col in allparams)
            {
                if (col.Contains("desc("))//col sort is a descending type
                {
                    desccolnames.Add(col.Replace("desc(", "").Replace(")", "").Trim());//removing 'desc(' and ')'. Colname is left
                }
                else //ascending sort on these cols
                {
                    asccolnames.Add(col);
                }
            }
        }

        private void GetLeftHandVar(string stmt, out string lhvar)
        {
            lhvar = null;//null if no left hand var exists in stmt

            int eqidx = stmt.IndexOf("=");
            int arrowidx = stmt.IndexOf("<-");
            int lowestidx = 0;
            bool isvarname = true; // assuming lefthand substring is variable.

            if (eqidx < 0 && arrowidx < 0)//no "=" and no "<-"
                return;

            //if one of the index is -1 then better overwrite that with value of the other
            if (eqidx == -1) eqidx = arrowidx;
            else if (arrowidx == -1) arrowidx = eqidx;

            //find the lowest(leftmost) index between = and <-
            if (eqidx < arrowidx)
            {
                lowestidx = eqidx;// index of =
            }
            else
            {
                lowestidx = arrowidx;//index of <-
            }

            string subs = stmt.Substring(0, lowestidx).Trim();

            //you can add more invalid chars to following 'if'
            if (subs.Contains(" ") || subs.Contains("(") || subs.Contains(")") ||
                subs.Contains("{") || subs.Contains("}")
                )
            {
                isvarname = false;
            }
            if (isvarname)//if subs is variable
            {
                lhvar = subs;
            }
        }

        ///Finding if R command is a method call that can return some results.
        private bool IsMethodName(string stmt)//01May2013
        {
            string pattern = @"\s*[A-Za-z0-9_\.]+\s*\(\s*";// method name patthern, methodName(
            bool str = Regex.IsMatch(stmt, pattern);
            MatchCollection mc = Regex.Matches(stmt.Trim(), pattern);

            foreach (Match m in mc)
            {
                if (m.Index == 0)
                    return true;
            }
            return false;
        }

        //For Painting Output window for BSKyFormated object.
        private void RefreshOutputORDataset(string objectname, CommandRequest cmd, string originalCommand, OutputWindow ow)
        {
            UAMenuCommand uamc = new UAMenuCommand();
            cmd.CommandSyntax = "is.null(" + objectname + "$BSkySplit)";
            if (AdvancedLogging) logService.WriteToLogLevel("ExtraLogs: Executing : " + cmd.CommandSyntax, LogLevelEnum.Info);

            bool isNonBSkyList = false;
            object isNonBSkyListstr = analytics.ExecuteR(cmd, true, false);

            if (isNonBSkyListstr != null && isNonBSkyListstr.ToString().ToLower().Equals("true"))
            {
                isNonBSkyList = true;
            }
            if (isNonBSkyList)
            {
                string ewmessage = BSky.GlobalResources.Properties.Resources.CantBSkyFormatErrorMsg;
                SendErrorToOutput(ewmessage, ow);//03Jul2013
                return;
            }
            if (AdvancedLogging) logService.WriteToLogLevel("ExtraLogs: $BSkySplit Result (false means non-bsky list): " + isNonBSkyList, LogLevelEnum.Info);

            cmd.CommandSyntax = objectname + "$uasummary[[7]]";
            if (AdvancedLogging) logService.WriteToLogLevel("ExtraLogs: Executing : " + cmd.CommandSyntax, LogLevelEnum.Info);

            string bskyfunctioncall = (string)analytics.ExecuteR(cmd, true, false);//actual call with values

            if (bskyfunctioncall == null)
            {
                bskyfunctioncall = ""; //24Apr2014 This is when no Dataset is open. 
            }
            if (AdvancedLogging) logService.WriteToLogLevel("ExtraLogs: $uasummary[[7]] Result : " + bskyfunctioncall, LogLevelEnum.Info);

            string bskyfunctionname = "";

            if (bskyfunctioncall.Length > 0)
            {
                if (bskyfunctioncall.Contains("("))
                    bskyfunctionname = bskyfunctioncall.Substring(0, bskyfunctioncall.IndexOf('(')).Trim();
                else
                    bskyfunctionname = bskyfunctioncall;
            }
            uamc.commandformat = objectname;// object that stores the result of analysis
            uamc.bskycommand = bskyfunctioncall.Replace('\"', '\'');// actual BSkyFunction call. " quotes replaced by '
            uamc.commandoutputformat = bskyfunctionname.Length > 0 ? string.Format(@"{0}", BSkyAppData.RoamingUserBSkyConfigL18nPath) + bskyfunctionname + ".xml" : "";//23Apr2015 

            uamc.commandtemplate = bskyfunctionname.Length > 0 ? string.Format(@"{0}", BSkyAppData.RoamingUserBSkyConfigL18nPath) + bskyfunctionname + ".xaml" : "";//23Apr2015 

            uamc.commandtype = originalCommand;

            CommandExecutionHelper auacb = new CommandExecutionHelper();

            if (AdvancedLogging) logService.WriteToLogLevel("ExtraLogs: CommandExecutionHelper: ", LogLevelEnum.Info);
            auacb.ExecuteSyntaxEditor(uamc, saveoutput.IsChecked == true ? true : false);//10JAn2013 edit
            auacb = null;
        }

        //was private
        public void RefreshDatagrids()//16May2013
        {
            string stmt = "Refresh Grids";
            UAMenuCommand uamc = new UAMenuCommand();
            uamc.commandformat = stmt;
            uamc.bskycommand = stmt;
            uamc.commandtype = stmt;
            //cmd.CommandSyntax = stmt;
            CommandExecutionHelper auacb = new CommandExecutionHelper();
            auacb.DatasetRefreshAndPrintTitle("Refresh Data");
            //auacb = null;
        }

        //16Jul2015 refesh both grids when 'refresh' icon is clicked in output window
        public void RefreshBothgrids()//16Jul2015
        {
            BSkyMouseBusyHandler.ShowMouseBusy();// ShowMouseBusy_old();

            string stmt = "Refresh Grids";
            UAMenuCommand uamc = new UAMenuCommand();
            uamc.commandformat = stmt;
            uamc.bskycommand = stmt;
            uamc.commandtype = stmt;
            CommandExecutionHelper auacb = new CommandExecutionHelper();
            auacb.BothGridRefreshAndPrintTitle("Refresh Data");

            BSkyMouseBusyHandler.HideMouseBusy();// HideMouseBusy_old();
        }

        private void isVariable(ref string stmt) // checks if user want to print a variable. Or function call results.
        {
            //// Check if its conditional command. Then dont enclose in print  //// 
            bool iscondiORloop = false;
            string _command = ExtractCommandName(stmt);//07sep2012
            RCommandType rct = GetRCommandType(_command);

            if (rct == RCommandType.CONDITIONORLOOP)
                iscondiORloop = true;

            ////////////////
            int frstopenbrkt = -2, secopenbrkt = -2, closingbrkt = -2, arowassignidx = -2, eqassignidx, assignidx = -2;
            bool beginswithprint = stmt.StartsWith("print(");
            bool beginswithopenparenthesis = stmt.StartsWith("(");

            if (beginswithprint || beginswithopenparenthesis)
            {
                frstopenbrkt = stmt.IndexOf("(");
                secopenbrkt = stmt.IndexOf("(", frstopenbrkt + 1);
            }
            else
            {
                secopenbrkt = stmt.IndexOf("(");
            }

            closingbrkt = stmt.IndexOf(")", secopenbrkt + 1);

            bool hasassignmentoperator = (stmt.Contains("=") || stmt.Contains("<-"));

            if (hasassignmentoperator && secopenbrkt > 0)
            {
                eqassignidx = stmt.IndexOf("=");
                arowassignidx = stmt.IndexOf("<-");
                if (eqassignidx > 0 && arowassignidx > 0) // both assignment oprator exists. say: a<-func(q=TRUE)
                {
                    assignidx = eqassignidx < arowassignidx ? eqassignidx : arowassignidx; // whichever has less idx
                }
                else if (eqassignidx > 0) // only '=' exists
                {
                    assignidx = eqassignidx;
                }
                else if (arowassignidx > 0)//only "<-" exists
                {
                    assignidx = arowassignidx;
                }

                if (assignidx > secopenbrkt && closingbrkt > assignidx)//if closing to secopenbrkt closes after assignment'='
                    hasassignmentoperator = false;
            }

            bool beginswithcat = stmt.StartsWith("cat(");

            if ((beginswithprint || beginswithopenparenthesis) && hasassignmentoperator) // for print(a<-c('msg')
            {
                int indexofopeningprintparenthesis = stmt.IndexOf('(');// first (
                int indexofclosingprintparenthesis = stmt.LastIndexOf(')');// last )
                int indexofassignmentopr = stmt.IndexOf("<-") > 0 ? stmt.IndexOf("<-") : stmt.IndexOf('=');
                string varname = stmt.Substring(indexofopeningprintparenthesis + 1, (indexofassignmentopr - indexofopeningprintparenthesis - 1));// a

                if (beginswithprint)
                {
                    stmt = stmt.Remove(indexofclosingprintparenthesis).Replace("print(", "") + "; print(" + varname + ");";
                }
                else if (beginswithopenparenthesis)
                {
                    stmt = stmt + "; print(" + varname + ");";// (a<-c('msg');) print(a);
                }
            }
            if (!beginswithprint && !hasassignmentoperator && !beginswithcat && !iscondiORloop)// for a & a+ a+a+a*2 & help(...) & ?...
            {
                stmt = "print(" + stmt + ")";
            }

        }

        // Check if command is one of : if, for, while, function
        private bool isConditionalOrLoopingCommand(string extractedCommand)//07Sep2012
        {
            bool iscondloop = false;

            if (extractedCommand.Equals("function(") || extractedCommand.Equals("for(") ||
                extractedCommand.Equals("while(") || extractedCommand.Equals("if(") ||
                extractedCommand.Equals("{") ||
                extractedCommand.Equals("local("))

            {
                iscondloop = true;
            }
            return iscondloop;
        }

        // Creating global list of graphic command in begining //
        private void LoadRegisteredGraphicsCommands(string grpListPath)//07Sep2012
        {
            registeredGraphicsList.Clear();//clearing just as precaution. Not actually needed

            string line;
            string[] lineparts;

            char[] separators = new char[] { ',' };//{',', ' ', ';'};
            string keyGraphicCommand;
            int grphwidth = -1, grphheight = -1; // -1 means use defauls from the options setting.
            StreamReader f = null;

            try
            {
                f = new StreamReader(grpListPath);
                while ((line = f.ReadLine()) != null)
                {
                    grphwidth = -1; grphheight = -1;
                    if (line.Trim().Length > 0)//do not add blank lines
                    {
                        if (line.Trim().StartsWith("#"))//commented line in GraphicCommandList.txt. Only single line comment is supported.
                            continue;
                        lineparts = (line.Trim()).Split(separators);
                        keyGraphicCommand = lineparts[0].Trim();
                        if (lineparts.Length > 1 && lineparts[1] != null && lineparts[1].Trim().Length > 0)//get width from file(line)
                        {
                            Int32.TryParse(lineparts[1], out grphwidth);//try setting width from file.
                        }
                        if (lineparts.Length > 2 && lineparts[2] != null && lineparts[2].Trim().Length > 0)//get height from file(line)
                        {
                            if (!Int32.TryParse(lineparts[2], out grphheight))//if conversion fails
                            {
                                if (grphwidth > 0)
                                    grphheight = grphwidth; // make height same as width, if height is not provided
                            }
                        }

                        //now add three values to dictionary
                        if (!registeredGraphicsList.ContainsKey(keyGraphicCommand)) // if the key is not already present. Unique keys are entertained.
                            registeredGraphicsList.Add(keyGraphicCommand, new ImageDimensions() { imagewidth = grphwidth, imageheight = grphheight });
                    }
                }
                f.Close();
            }
            catch (Exception e)
            {
                MessageBox.Show(this, BSky.GlobalResources.Properties.Resources.ErrOpeningRegGrpLst+"\n" + e.Message);
                logService.WriteToLogLevel("Registered Graphics List not found!", LogLevelEnum.Error);
            }
        }

        ImageDimensions imgDim; //store image dimentions of current graphic, from graphiccommand.txt
        bool lastcommandwasgraphic = false;
        // Check if graphic command belongs to global list of graphic command //
        private bool isGraphicCommand(string extractedCommand)//07Sep2012
        {
            string command = extractedCommand.Replace('(', ' ').Trim(); //remove the open parenthesis from the end.
            //// Extracting command from statement /////

            //// searching list ////
            bool graphic = false;
            string tempcomm;

            if (registeredGraphicsList.ContainsKey(command))
            {
                registeredGraphicsList.TryGetValue(command, out imgDim);
                graphic = true;
            }
            lastcommandwasgraphic = graphic;
            return graphic;
        }

        //if command is single command(or may be 2 lines one for BSkyFormat) then find if XML template is defined
        private bool isXMLDefined()
        {
            bool hasXML = false;

            if (DlgProp != null)
                hasXML = DlgProp.IsXMLDefined;
            return hasXML;
        }

        // Extracts command name in format like :- "any.command("
        private string ExtractCommandName(string stmt)
        {
            string comm = string.Empty;

            stmt = RemoveExtraSpacesBeforeOpeningBracket(stmt);

            string pattern = @"[A-Za-z0-9_.]+\(";
            bool com = Regex.IsMatch(stmt, pattern);

            if (com)//remove extra spaces
            {
                MatchCollection mc = Regex.Matches(stmt, pattern);

                foreach (Match m in mc)
                {
                    comm = m.Value;//picking up the very first command only. Which should be one of
                    break;          // if(, for(, while(, function( or plot( etc..
                }
            }
            else//28Aug2014 if no command name was found return the same string that was received as a parameter
            {
                comm = stmt;
            }
            return comm.Trim();
        }

        private string RemoveExtraSpacesBeforeOpeningBracket(string stmt)
        {
            string spacelessstmt = stmt;
            string pattern = @"[A-Za-z0-9_.]+\s+\(";
            bool spaces = Regex.IsMatch(stmt, pattern);

            if (spaces)//remove extra spaces
            {
                MatchCollection mc = Regex.Matches(stmt, pattern);
                spacelessstmt = Regex.Replace(stmt, @"\s+\(", "(", RegexOptions.None);
            }
            return spacelessstmt;
        }

        private void createAUPara(string auparas, CommandOutput lst)
        {
            string startdatetime = string.Empty;

            if (lst.NameOfAnalysis == null || lst.NameOfAnalysis.Trim().Length < 1)
            {
                lst.NameOfAnalysis = "R-Output";//Parent Node name. 02Aug2012
            }
            if (auparas == null || auparas.Length < 1)
                return;

            string selectnode = "bskyoutput/bskyanalysis/aup";
            string AUPara = "<aup>" + auparas.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;") + "</aup>";
            XmlDocument xd = null;

            ///// Creating DOM for generation output ///////
            string fulldom = "<bskyoutput> <bskyanalysis>" + AUPara + "</bskyanalysis> </bskyoutput>";
            xd = new XmlDocument(); xd.PreserveWhitespace = true; xd.LoadXml(fulldom);

            //// for creating AUPara //////////////
            BSkyOutputGenerator bsog = new BSkyOutputGenerator();
            int noofaup = xd.SelectNodes(selectnode).Count;// should be 3

            for (int k = 1; k <= noofaup; k++)
            {
                if (lst.NameOfAnalysis.Equals("R-Output") || lst.NameOfAnalysis.Contains("Command Editor Execution"))
                {
                    lst.Add(bsog.createAUPara(xd, selectnode + "[" + (1) + "]", ""));
                }
                else if (lst.NameOfAnalysis.Equals("Datasets"))
                {
                    lst.Add(bsog.createAUPara(xd, selectnode + "[" + (1) + "]", "Open Datasets"));
                }
                else
                {
                    //for (int k = 1; k <= noofaup; k++)
                    lst.Add(bsog.createAUPara(xd, selectnode + "[" + k + "]", startdatetime));
                }
            }
        }

        private void createBSkyGraphic(Image img, CommandOutput lst)//30Aug2012
        {
            lst.NameOfAnalysis = "R-Graphics";

            BSkyGraphicControl bsgc = new BSkyGraphicControl();
            bsgc.BSkyImageSource = img.Source;
            bsgc.ControlType = "Graphic";
            lst.Add(bsgc);
        }

        private void createDiskFileFromImageSource(BSkyGraphicControl bsgc)
        {
            Image myImage = new System.Windows.Controls.Image();
            myImage.Source = bsgc.BSkyImageSource;

            string grpcntrlimgname = confService.GetConfigValueForKey("bskygrphcntrlimage");//23nov2012
            string grpctrlimg = Path.Combine(BSkyAppData.RoamingUserBSkyTempPath, grpcntrlimgname);
            // load default value if no path is set or invalid path is set
            if (grpctrlimg.Trim().Length == 0 || !IsValidFullPathFilename(grpctrlimg, false))
            {
                MessageBox.Show(this, BSky.GlobalResources.Properties.Resources.bskygrphctrlimgConfigKeyNotFound);
                return;
            }

            System.Windows.Media.Imaging.BitmapImage bitmapImage = new System.Windows.Media.Imaging.BitmapImage();
            bitmapImage = ((System.Windows.Media.Imaging.BitmapImage)myImage.Source);
            System.Windows.Media.Imaging.PngBitmapEncoder pngBitmapEncoder = new System.Windows.Media.Imaging.PngBitmapEncoder();
            System.IO.FileStream stream = new System.IO.FileStream(@grpctrlimg, FileMode.Create);

            pngBitmapEncoder.Interlace = PngInterlaceOption.On;
            pngBitmapEncoder.Frames.Add(System.Windows.Media.Imaging.BitmapFrame.Create(bitmapImage));
            pngBitmapEncoder.Save(stream);
            stream.Flush();
            stream.Close();
        }

        private XmlDocument createFlexGridXmlDoc(string[] colheaders, string[] rowheaders, string[,] data)
        {
            XmlDocument xd = new XmlDocument();
            int rc = rowheaders.Length;
            int cc = colheaders.Length;

            string colnames = "<table  cellpadding=\"5\" cellspacing=\"0\"><thead><tr><th class=\"h\"></th>";

            foreach (string s in colheaders)
            {
                if (s == null || s.Trim().Length < 1)
                    colnames = colnames + "<th class=\"c\">" + ".-." + "</th>";//".-.'" is spl char sequence that tells that there is no header
                else
                    colnames = colnames + "<th class=\"c\">" + s + "</th>";
            }
            colnames = colnames + "</tr></thead>";

            //// creating row headers with data ie.. one complete row ////
            string rowdata = "<tbody>";

            for (int r = 0; r < rc; r++)
            {
                //Putting Row Header in a row. ".-.'" is spl char sequence that tells that there is no header
                if (bsky_no_row_header)
                {
                    rowdata = rowdata + "<tr><td class=\"h\">" + ".-." + "</td>";//rowheader in row
                }
                else
                {
                    rowdata = rowdata + "<tr><td class=\"h\">" + rowheaders[r] + "</td>";//rowheader in row
                }
                //Putting Data in a row
                for (int c = 0; c < cc; c++)/// data in row
                {
                    rowdata = rowdata + "<td class=\"c\">" + data[r, c].Replace("<", "&lt;").Replace(">", "&gt;") + "</td>";
                }
                rowdata = rowdata + "</tr>";
            }
            rowdata = rowdata + "</tbody></table>";

            string fullxml = colnames + rowdata;
            xd.LoadXml(fullxml);
            return xd;
        }

        private void createFlexGrid(XmlDocument xd, CommandOutput lst, string header)
        {
            AUXGrid xgrid = new AUXGrid();
            AUGrid c1FlexGrid1 = xgrid.Grid;// new C1flexgrid.
            xgrid.Header.Text = header;//FlexGrid header as well as Tree leaf node text(ControlType)
            var gridfilter = new C1FlexGridFilter(xgrid.Grid);

            BSkyOutputGenerator bsog = new BSkyOutputGenerator();
            bsog.html2flex(xd, c1FlexGrid1);
            lst.Add(xgrid);

        }

        #region Dynamic Class creation and filling C1Flexgrid

        private void CreateDynamicClassFlexGrid(string header, string[] colheaders, string[] rowheaders, string[,] data, CommandOutput lst)
        {
            IList list;
            AUXGrid xgrid = new AUXGrid();
            AUGrid c1FlexGrid1 = xgrid.Grid;// new C1flexgrid.
            xgrid.Header.Text = header;//FlexGrid header as well as Tree leaf node text(ControlType)

            ///////////// merge and sizing /////
            c1FlexGrid1.AllowMerging = AllowMerging.ColumnHeaders | AllowMerging.RowHeaders;
            c1FlexGrid1.AllowSorting = true;

            c1FlexGrid1.MaxHeight = 800;// NoOfRows* EachRowHeight;
            c1FlexGrid1.MaxWidth = 1000;

            int nrows = data.GetLength(0);
            int ncols = data.GetLength(1);


            //// Dynamic class logic
            FGDataSource ds = new FGDataSource();
            ds.RowCount = nrows;
            ds.Data = data;
            foreach (string s in colheaders)
            {
                ds.Variables.Add(s.Trim());
            }
            list = new DynamicList(ds);
            if (list != null)
            {
                c1FlexGrid1.ItemsSource = list;
            }
            FillColHeaders(colheaders, c1FlexGrid1);
            FillRowHeaders(rowheaders, c1FlexGrid1);
            lst.Add(xgrid);
        }

        private void FillColHeaders(string[] colHeaders, AUGrid c1fgrid)
        {
            bool iscolheaderchecked = true;// rowheaderscheck.IsChecked == true ? true : false;

            //creating row headers
            string[,] colheadersdata = new string[1, colHeaders.Length];
            ////creating data
            for (int r = 0; r < colheadersdata.GetLength(0); r++)
            {
                for (int c = 0; c < colheadersdata.GetLength(1); c++)
                {
                    colheadersdata[r, c] = colHeaders[c];
                }
            }

            //create & fill row headers
            bool fillcolheaders = iscolheaderchecked;

            if (fillcolheaders)
            {
                var FGcolheaders = c1fgrid.ColumnHeaders;
                FGcolheaders.Rows[0].AllowMerging = true;
                FGcolheaders.Rows[0].HorizontalAlignment = HorizontalAlignment.Center;

                for (int i = FGcolheaders.Columns.Count; i < colheadersdata.GetLength(1); i++)
                {
                    C1.WPF.FlexGrid.Column col = new C1.WPF.FlexGrid.Column();
                    col.AllowMerging = true;
                    col.VerticalAlignment = VerticalAlignment.Top;
                    FGcolheaders.Columns.Add(col);
                }

                for (int i = FGcolheaders.Rows.Count; i < colheadersdata.GetLength(0); i++)
                {
                    C1.WPF.FlexGrid.Row row = new C1.WPF.FlexGrid.Row();
                    FGcolheaders.Rows.Add(row);
                    row.AllowMerging = true;
                }

                //fill row headers
                for (int i = 0; i < colheadersdata.GetLength(0); i++)
                    for (int j = 0; j < colheadersdata.GetLength(1); j++)
                    {
                        if (colheadersdata[i, j] != null && colheadersdata[i, j].Trim().Equals(".-."))
                            FGcolheaders[i, j] = "";//14Jul2014 filling empty header
                        else
                            FGcolheaders[i, j] = colheadersdata[i, j];
                    }
            }
        }

        private void FillRowHeaders(string[] rowHeaders, AUGrid c1fgrid)
        {
            bool isrowheaderchecked = true;// rowheaderscheck.IsChecked == true ? true : false;

            //creating row headers
            string[,] rowheadersdata = new string[rowHeaders.Length, 1];
            ////creating data
            for (int r = 0; r < rowheadersdata.GetLength(0); r++)
            {
                for (int c = 0; c < rowheadersdata.GetLength(1); c++)
                {
                    rowheadersdata[r, c] = rowHeaders[r];
                }
            }
            //create & fill row headers
            bool fillrowheaders = isrowheaderchecked;

            if (fillrowheaders)
            {
                var FGrowheaders = c1fgrid.RowHeaders;
                FGrowheaders.Columns[0].AllowMerging = true;
                FGrowheaders.Columns[0].VerticalAlignment = VerticalAlignment.Top;
                FGrowheaders.Columns[0].Width = new GridLength(70);

                for (int i = FGrowheaders.Columns.Count; i < rowheadersdata.GetLength(1); i++)
                {
                    C1.WPF.FlexGrid.Column col = new C1.WPF.FlexGrid.Column();
                    col.AllowMerging = true;
                    col.VerticalAlignment = VerticalAlignment.Top;
                    col.Width = new GridLength(70);
                    FGrowheaders.Columns.Add(col);
                }

                for (int i = FGrowheaders.Rows.Count; i < rowheadersdata.GetLength(0); i++)
                {
                    C1.WPF.FlexGrid.Row row = new C1.WPF.FlexGrid.Row();
                    row.AllowMerging = true;
                    FGrowheaders.Rows.Add(row);
                }

                //fill row headers
                for (int i = 0; i < rowheadersdata.GetLength(0); i++)
                    for (int j = 0; j < rowheadersdata.GetLength(1); j++)
                    {
                        if (rowheadersdata[i, j] != null && rowheadersdata[i, j].Trim().Equals(".-."))
                            FGrowheaders[i, j] = "";//14Jul2014 filling empty header
                        else
                            FGrowheaders[i, j] = rowheadersdata[i, j];
                    }
            }
        }
        #endregion

        private void browse_Click(object sender, RoutedEventArgs e)
        {
            string FileNameFilter = "BlueSky Format, that can be opened in Output Window later (*.bsoz)|*.bsoz|Comma Seperated (*.csv)|*.csv|HTML (*.html)|*.html"; //BSkyOutput
            SaveFileDialog saveasFileDialog = new SaveFileDialog();
            saveasFileDialog.Filter = FileNameFilter;

            bool? output = saveasFileDialog.ShowDialog(Application.Current.MainWindow);

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
                fullpathfilename.Text = saveasFileDialog.FileName;
            }
        }

        public void PasteSyntax(string command)//29Jan2013
        {
            string newlines = (inputTextbox.Text != null && inputTextbox.Text.Trim().Length > 0) ? "\n\n" : string.Empty;

            if (command != null && command.Length > 0)
                inputTextbox.AppendText(newlines + command);
        }

        //Not in Use
        private void save_Click(object sender, RoutedEventArgs e)
        {
            //////// Active output window ///////
            OutputWindowContainer owc = (LifetimeService.Instance.Container.Resolve<IOutputWindowContainer>()) as OutputWindowContainer;
            OutputWindow ow = owc.ActiveOutputWindow as OutputWindow; //get currently active window

            if (saveoutput.IsChecked == true)
                ow.DumpAllAnalyisOuput(fullfilepathname, fileformat, extratags);
        }

        private bool IsValidFullPathFilename(string path, bool filemustexist)
        {
            bool validDir, validFile;
            string message = string.Empty;
            string dir = Path.GetDirectoryName(path);
            string filename = Path.GetFileName(path);

            ///Check filename
            if (filemustexist && !File.Exists(path))
            {
                validFile = false;
                message = BSky.GlobalResources.Properties.Resources.invalidFilename+" " + filename;
            }
            else
                validFile = true;

            //// Check Directory path
            if (Directory.Exists(dir) || dir.Trim().Length == 0)
            {
                validDir = true;
            }
            else
            {
                message = message + " "+BSky.GlobalResources.Properties.Resources.invalidDir+" " + dir;
                validDir = false;
            }
            if (message.Trim().Length > 0)
            {
                //MessageBox.Show(message);
                logService.Warn(message);
            }
            return (validDir && validFile);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (this.Visibility == System.Windows.Visibility.Visible)//Do this only if Syn Edtr is visible.
            {
                this.Activate();

                System.Windows.Forms.DialogResult dresult = System.Windows.Forms.DialogResult.OK;

                if (Modified)
                {
                    dresult = System.Windows.Forms.MessageBox.Show(
                              BSky.GlobalResources.Properties.UICtrlResources.REditorCloseSaveScriptPrompt,
                              BSky.GlobalResources.Properties.Resources.SaveExitCommandEditor,
                              System.Windows.Forms.MessageBoxButtons.YesNoCancel,
                              System.Windows.Forms.MessageBoxIcon.Question);
                }
                if (dresult == System.Windows.Forms.DialogResult.Cancel)//dont close
                {
                    e.Cancel = true; // do not close this window
                    this.SEForceClose = false;
                }
                else
                {
                    ///before closing save R scripts in Syntax Editor text area..13Feb2013
                    if (dresult == System.Windows.Forms.DialogResult.Yes)//Save
                        SyntaxEditorSaveAs();

                    /// Do hide OR hide & close ///
                    inputTextbox.Text = string.Empty; //Clean window
                    this.Visibility = System.Windows.Visibility.Hidden;// hide window. If you close down you can't reopen it again.
                    if (!this.SEForceClose)
                        e.Cancel = true;//stop closing

                    Modified = false;
                    Title = BSky.GlobalResources.Properties.UICtrlResources.CommandEditorPanelTitle+" Window";
                }
            }
        }

        //clears the command area
        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (Modified)//if any modification done to command scripts after last Save
            {  //allow user to save changes before opening another command script

                System.Windows.Forms.DialogResult dresult = System.Windows.Forms.MessageBox.Show(
                          BSky.GlobalResources.Properties.UICtrlResources.REditorCloseSaveScriptPrompt,
                          BSky.GlobalResources.Properties.UICtrlResources.REditorSavePromptMsgBoxTitle,
                          System.Windows.Forms.MessageBoxButtons.YesNoCancel,
                          System.Windows.Forms.MessageBoxIcon.Question);

                if (dresult == System.Windows.Forms.DialogResult.Yes)//Yes Save- and Close
                {
                    SyntaxEditorSaveAs();
                }
                else if (dresult == System.Windows.Forms.DialogResult.No)//No Save- but Close
                {
                }
                else//no Save no Close
                {
                    return;
                }
            }
            inputTextbox.Text = string.Empty;
            this.Activate();
        }

        private void MenuItemOpen_Click(object sender, RoutedEventArgs e)
        {
            SyntaxEditorOpen();
            this.Activate();//19Feb2013
        }

        private void MenuItemSaveAs_Click(object sender, RoutedEventArgs e)
        {
            SyntaxEditorSaveAs();
            this.Activate();//19Feb2013
        }

        private void SyntaxEditorOpen()
        {
            if (Modified)//if any modification done to command scripts after last Save
            {  //allow user to save changes before opening another command script

                System.Windows.Forms.DialogResult dresult = System.Windows.Forms.MessageBox.Show(
                          BSky.GlobalResources.Properties.Resources.SaveScriptPromptBeforeOpen,
                          BSky.GlobalResources.Properties.UICtrlResources.REditorSavePromptMsgBoxTitle,
                          System.Windows.Forms.MessageBoxButtons.YesNoCancel,
                          System.Windows.Forms.MessageBoxIcon.Question);

                if (dresult == System.Windows.Forms.DialogResult.Yes)//Yes Save- and Close
                {
                    SyntaxEditorSaveAs();
                }
                else if (dresult == System.Windows.Forms.DialogResult.No)//No Save- but Close
                {
                }
                else//no Save no Close
                {
                    return;
                }
            }

            const string FileNameFilter = "BSky R scripts, (*.bsr)|*.bsr"; //BSkyR
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = FileNameFilter;
            bool? output = openFileDialog.ShowDialog(Application.Current.MainWindow);

            if (output.HasValue && output.Value)
            {
                System.IO.StreamReader file = new System.IO.StreamReader(openFileDialog.FileName);
                inputTextbox.Text = file.ReadToEnd();
                file.Close();
            }
            Modified = false;//19Feb2013 Newly loaded script can only be modified after loading finishes.
            Title = BSky.GlobalResources.Properties.UICtrlResources.CommandEditorPanelTitle + " Window"; //19Feb2013
        }

        private void SyntaxEditorSaveAs()
        {
            const string FileNameFilter = "BSky R scripts, (*.bsr)|*.bsr"; //BSkyR
            SaveFileDialog saveasFileDialog = new SaveFileDialog();
            saveasFileDialog.Filter = FileNameFilter;
            bool? output = saveasFileDialog.ShowDialog(Application.Current.MainWindow);

            if (output.HasValue && output.Value)
            {
                System.IO.StreamWriter file = new System.IO.StreamWriter(saveasFileDialog.FileName);
                file.WriteLine(inputTextbox.Text);
                file.Close();
            }
            Modified = false;//19Feb2013 currently saving. So immediately after save there are no new changes/modifications.
            Title = BSky.GlobalResources.Properties.UICtrlResources.CommandEditorPanelTitle + " Window"; //19Feb2013
        }

        //19Feb2013 If anybody edits/changes something in text-area
        private void inputTextbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            Modified = true;
            Title = BSky.GlobalResources.Properties.UICtrlResources.CommandEditorPanelTitle +" - < unsaved script >";
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            RefreshDatagrids();
        }

        //17May2013
        public void ShowWindowLoadFile()
        {
            try
            {
                System.IO.StreamReader file = new System.IO.StreamReader(DoubleClickedFilename);
                inputTextbox.Text = file.ReadToEnd();
                file.Close();
                Modified = false;
                Title = BSky.GlobalResources.Properties.UICtrlResources.CommandEditorPanelTitle + " Window";
                this.Show();
            }
            catch
            {
                logService.WriteToLogLevel("Error Opening file by double click", LogLevelEnum.Error);
            }
        }
    }

    public class ImageDimensions
    {
        public int imagewidth { get; set; }
        public int imageheight { get; set; }

        public bool overrideImgDim
        {
            get
            {
                if (imagewidth > -1 && imageheight > -1) 
                {
                    return true;
                }
                else 
                {
                    return false;
                }
            }
        }
    }
}