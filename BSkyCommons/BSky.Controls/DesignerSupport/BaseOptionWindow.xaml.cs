using System;
using System.Windows;
using BSky.Controls;
using BSky.Statistics.Common;
using BSky.Lifetime;
using System.IO;
using BSky.Interfaces.Interfaces;
using System.Collections.Generic;
using System.Windows.Input;
using System.Windows.Media;
using BSky.Statistics.Service.Engine.Interfaces;

//Thus is used by AUanalysiscommandbase.cs to create the base dialog in the main application
//Thisis not used by the open command in dialog editor as the OK Syntax buttons are not displayed on the canvas
//Preview in dialog editor mode does not use this as well as we should not display syntax       

namespace BSky.Interfaces.Commands
{
    /// <summary>
    /// Interaction logic for BaseOptionWindow.xaml
    /// </summary>
    /// 

    public partial class BaseOptionWindow : Window
    {
        //IUIController UIController;
        IUIController UIController = LifetimeService.Instance.Container.Resolve<IUIController>();
        IAnalyticsService analytics = LifetimeService.Instance.Container.Resolve<IAnalyticsService>();
        public static RoutedCommand KillCommand = new RoutedCommand();

        //Added by Aaron 07/20/2014
        //The GetOverwrittenVars() function below (in this class)  returns the overwrittervars variable. If overwrittenvars =true tells me that a user has selected a dataset or variable to be created by the analytical command but that 
        //variable already exists. Hence the command should not be executed.
        public bool overwrittenvars = false;

        private int expanderCollapsedWidth = 55;
        private int gapBetweenCanvasAndHelp = 3;
        private int gapBetweenCanvasAndOK = 3;
        private int OKCanceHelpHeight = 75;

        public BaseOptionWindow()
        {
            InitializeComponent();
            WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
            KillCommand.InputGestures.Add(new KeyGesture(Key.K, ModifierKeys.Control));
        }
        public FrameworkElement Template
        {
            get
            {
                if (Host.Children.Count > 0)
                    return Host.Children[0] as FrameworkElement;
                else
                    return null;
            }
            set
            {
                if (value == null)
                {
                    Host.Children.Clear();//13Feb2013 Delete all children. for testing and developing dialog session logic
                    return;
                }
                BSkyCanvas canvas = value as BSkyCanvas;
                canvas.CanExecuteChanged += new EventHandler<BSkyBoolEventArgs>(canvas_CanExecuteChanged);
                if (canvas == null)
                    return;
                if (!string.IsNullOrEmpty(canvas.Title))
                    this.Title = canvas.Title;
                else
                    this.Title = "Untitled Dialog";

                if (value.Width != double.NaN)
                {
                    this.Host.Width = value.Width + gapBetweenCanvasAndHelp;
                    this.Width = value.Width + expanderCollapsedWidth; //14May2015 expander added by Anil ( for expander button)
                }
                if (value.Height != double.NaN)
                {
                    this.Host.Height = value.Height + gapBetweenCanvasAndOK;
                    this.Height = value.Height + OKCanceHelpHeight;//85
                }
                Host.Children.Add(value); //10Feb2017 Q's subset crash.
                //Added 01/15/2014
                //This disables the help button on the canvas when the help file is empty
                if (canvas.Helpfile == null || canvas.Helpfile == string.Empty) help.IsEnabled = false;
            }
        }

        void canvas_CanExecuteChanged(object sender, BSkyBoolEventArgs e)
        {
            Ok.IsEnabled = e.Value;
            //Added by Aaron 01/02/2014
            //This is to ensure that unless all canexecutes are set to yes on all the controls on the dialog
            //the OK and Syntax buttons will not be enabled
            Paste.IsEnabled = e.Value;
        }
        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            this.Tag = "Ok";
            this.DialogResult = true;
            object obj = this.Template;
            BSkyCanvas cs = obj as BSkyCanvas;
            overwrittenvars = CheckForOverwrittenVars(cs);
            // string varnames = "";
            // List<DataSourceVariable> varlst = UIController.GetActiveDocument().Variables;
            //// List<DataSourceVariable> varlst = UIController.
            // foreach (DataSourceVariable dsv in varlst)
            // {
            //     varnames = varnames +", "+ dsv.Name;
            // }
            // MessageBox.Show("Variables:\n" + varnames);
            this.Close();
        }

        //Added by Aaron 07/20/2014
        //The GetOverwrittenVars() function below (in this class)  returns the overwrittervars variable. If overwrittenvars =true tells me that a user has selected a dataset or variable to be created by the analytical command but that 
        //variable already exists. Hence the command should not be executed.
        public Boolean GetOverwrittenVars()
        {
            return overwrittenvars;
        }

        //Added by Aaron 07/20/2014
        //this function looks at all the objects on the canvas recursively (for subdialogs) that contain the Overwrite settings property
        //The overwrite setting value controls whether the user should be prompted when creating a new variable or dataset and that variable or dataset already exists.
        public Boolean CheckForOverwrittenVars(BSkyCanvas cs)
        {
            System.Windows.Forms.DialogResult result;
            Boolean stopexecution = false;
            string message;
            foreach (object obj in cs.Children)
            {
                if (obj.GetType().Name == "BSkyTextBox")
                {
                    BSkyTextBox tb = obj as BSkyTextBox;
                    if (tb.OverwriteSettings == "PromptBeforeOverwritingVariables")
                    {
                        List<DataSourceVariable> varlst = UIController.GetActiveDocument().Variables;
                       // UIController.
                        //as=UIController.Re
                        // List<DataSourceVariable> varlst = UIController.
                        foreach (DataSourceVariable dsv in varlst)
                        {
                            // varnames = varnames + ", " + dsv.Name;
                            if (dsv.Name == tb.Text)
                            {
                                message = "Do you want to overwrite variable " + tb.Text;
                                result = System.Windows.Forms.MessageBox.Show(message, "Save Changes", System.Windows.Forms.MessageBoxButtons.YesNoCancel, System.Windows.Forms.MessageBoxIcon.Question);
                                if (result == System.Windows.Forms.DialogResult.Yes)//save
                                {

                                }
                                if (result == System.Windows.Forms.DialogResult.Cancel)//save
                                {
                                    return stopexecution = true;
                                }
                                if (result == System.Windows.Forms.DialogResult.No)//save
                                {
                                    return stopexecution = true;
                                }
                            }
                        }
                    }


                    if (tb.OverwriteSettings == "PromptBeforeOverwritingDatasets")
                    {
                       // List<DataSourceVariable> varlst = UIController.GetActiveDocument().Variables;
                        // UIController.
                        //as=UIController.Re
                        // List<DataSourceVariable> varlst = UIController.
                        List<string> datasetnames;
                       datasetnames = UIController.GetDatasetNames();

                        foreach (string dataset in datasetnames)
                        {
                            if (dataset ==tb.Text)
                            {
                             message = "Do you want to overwrite dataset " + tb.Text;
                             result = System.Windows.Forms.MessageBox.Show(message, "Save Changes", System.Windows.Forms.MessageBoxButtons.YesNoCancel, System.Windows.Forms.MessageBoxIcon.Question);
                                if (result == System.Windows.Forms.DialogResult.Yes)//save
                                {

                                }
                                if (result == System.Windows.Forms.DialogResult.Cancel)//save
                                {
                                    return stopexecution = true;
                                }
                                if (result == System.Windows.Forms.DialogResult.No)//save
                                {
                                    return stopexecution = true;
                                }
                            }
                        }
                    }

                }
                else if (obj.GetType().Name == "BSkyButton")
                {
                    BSkyButton btn = obj as BSkyButton;
                    FrameworkElement fe = obj as FrameworkElement;
                    BSkyCanvas cnvs = fe.Resources["dlg"] as BSkyCanvas;
                    // if (cs != null) ls.AddRange(gethelpfilenames(cs));
                    stopexecution = CheckForOverwrittenVars(cnvs);
                }
            }
            return stopexecution;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Tag = "Cancel";
            this.DialogResult = false;
            this.Close();
        }

        private void Paste_Click(object sender, RoutedEventArgs e)
        {
            this.Tag = "Syntax";

            this.DialogResult = true;
            //this.Close();

            /// Following code does not work because of cyclic dependancy //
            //IAnalyticsService analytics = LifetimeService.Instance.Container.Resolve<IAnalyticsService>();
            ////IConfigService confService = LifetimeService.Instance.Container.Resolve<IConfigService>();//23nov2012
            ////CommandRequest cmd = new CommandRequest();

            ////OutputHelper.Reset();
            ////OutputHelper.UpdateMacro("%DATASET%", UIController.GetActiveDocument().Name);

            ////FrameworkElement element = this.Template;
            ////BSkyCanvas canvas = element as BSkyCanvas;
            ////if (canvas != null && !string.IsNullOrEmpty(canvas.CommandString))
            ////{
            ////    cmd.CommandSyntax = OutputHelper.GetCommand(canvas.CommandString, element);// can be used for "Paste" for syntax editor
            ////    //Check if OK or Syntax was clicked
            ////    if (this.Tag.ToString().Equals("Syntax"))//21Jan2013
            ////    {
            ////        //copy to clipboard and return for this function
            ////        Clipboard.SetText(cmd.CommandSyntax);

            ////        //Launch Syntax Editor window with command pasted /// 29Jan2013
            ////        MainWindow mwindow = LifetimeService.Instance.Container.Resolve<MainWindow>();
            ////        ////// Start Syntax Editor  //////
            ////        SyntaxEditorWindow sewindow = LifetimeService.Instance.Container.Resolve<SyntaxEditorWindow>();
            ////        sewindow.Owner = mwindow;
            ////        string syncomment = "# Use BSkyFormat(obj) to format the output.\n";
            ////        sewindow.PasteSyntax(syncomment + cmd.CommandSyntax);//paste command
            ////        sewindow.Show();
            ////        sewindow.Focus();
            ////        return;
            ////    }
            ////}
        }


        //Added by Aaron
        //01/15/2014
        //Code below launches the help file in the default application for the Help file


        //Added by Aaron 05/06/2014
        //When I am inspecting a dialog definition or previewing a dialog in dialog editor. I want to click on the HELP button and access the Help files
        //However the help files are not in the bin/config directory
        //Its only whenI install the dialog that the help files are in the binn/config directory
        //Also when you install the help files, we rename the help files to dialogname_1, dialogname_2. This ensures that we don't 
        //accidently overide the help files of another command
        //This means when I am in the main application, I want the Help button to work differently for 2 cases
        //Case 1: click help on the dialog definition when inspecting the command. Here I launch the help files with their original name
        //from the temp directory
        //Case 2: click help on dialog displayed when executing an installed command
        //Here I launch the help files from the bin/config directory
        //This also allows me to use the same code to create the sub dialog mode in the execution of the dialog and the inspection
        //Case 3:
        //When I am in dialog editor and I am previewing a dialog definition, I just created
        //The help files need to be loaded from the original llocation where they exist

        private void help_Click(object sender, RoutedEventArgs e)
        {
            BSkyCanvas cs = Template as BSkyCanvas;
            string path = null;
            if (BSkyCanvas.previewinspectMode == true)
            {
                if (cs.Helpfile != "urlOrUri")
                    path = Path.GetTempPath() + Path.GetFileName(cs.Helpfile);
                else path = cs.Helpfile;
                try
                {
                    System.Diagnostics.Process p1 = System.Diagnostics.Process.Start(path);
                }
                catch (System.ComponentModel.Win32Exception ex)
                {
                    System.Windows.MessageBox.Show(ex.Message);
                }
            }
            else
            {
                //string path="c:\aaron\ab.bc";
                if (cs.internalhelpfilename != "urlOrUri")
                {
                    //23Apr2015 path = Path.GetFullPath(@".\Config\") + cs.internalhelpfilename;
                    path = Path.GetFullPath(string.Format(@"{0}", BSkyAppDir.RoamingUserBSkyConfigL18nPath)) + cs.internalhelpfilename;//23Apr2015 
                }
                else path = cs.Helpfile;
                try
                {
                    System.Diagnostics.Process p1 = System.Diagnostics.Process.Start(path);
                }
                catch (System.ComponentModel.Win32Exception ex)
                {
                    System.Windows.MessageBox.Show(ex.Message);
                }
            }
        }

        private void kill_Click(object sender, RoutedEventArgs e)
        {
            //in case you rename the title or control name, do not rename here. It must be "kill"
            //or you have to make changes in AUAnalysisCommandBase
            this.Tag = "Kill"; 
            this.DialogResult = true;
            this.Close();
        }

        #region Help Expander  14May2015
        double expanderExpandedWidth = 400;
        private void dlgexpander_Expanded(object sender, RoutedEventArgs e)
        {
            this.Width = this.Width + expanderExpandedWidth;
            dlgexpander.BorderBrush = Brushes.Black;
            object obj = this.Template;
            BSkyCanvas cs = obj as BSkyCanvas;
            dialoghelptext.Html = cs.HelpText;
        }

        private void dlgexpander_Collapsed(object sender, RoutedEventArgs e)
        {
            this.Width = this.Width - expanderExpandedWidth;
            dlgexpander.BorderBrush = Brushes.Transparent;
        }

        private void rhelpbutton_Click(object sender, RoutedEventArgs e)
        {
            object obj = this.Template;
            BSkyCanvas cs = obj as BSkyCanvas;
            string rhelp = cs.RHelpText; //in future, this can be semicolon separated list( if help on multiple functions is required).

            //27Sep2016 
            //First make sure therespective R package is loaded. If its not, the help wont work
            string pkgs = cs.RPackages;
            LoadDialogRPacakges(pkgs);

            //now try launching R help
            CommandRequest comreq = new CommandRequest();
            comreq.CommandSyntax =  "print("+rhelp+")";//"print(help(library))";//
            analytics.ExecuteR(comreq, false, false);      
        }

        //This is trimmed down version of the function(same name) in the AUAnalysisCommandBase.
        //Semi-colon separated R pacakge names (in Dialog property)
        private void LoadDialogRPacakges(string commaSeparatedPacakgeNames)
        {
            //07Feb2017
            if (commaSeparatedPacakgeNames == null || commaSeparatedPacakgeNames.Length < 1)
            {
                return;
            }
            char[] chars = new char[1] { ';' };
            string[] dlgpkgarr = commaSeparatedPacakgeNames.Split(chars);
            string current;

            CommandRequest comreq = new CommandRequest();

            for (int i = 0; i < dlgpkgarr.Length; i++)
            {
                current = dlgpkgarr[i].Trim();
                if (current.Length < 1)//may be someone added too many semi-colons in between. So package name will be just "".
                    continue; // dont consider empty string package name and jump to next.
                else
                {
                    comreq.CommandSyntax = "require(" + current + ")";//load R package
                    analytics.ExecuteR(comreq, false, false); 
                }
            }
        }

        #endregion
    }
}
