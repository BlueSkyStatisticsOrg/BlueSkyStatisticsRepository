using System;
using System.Windows;
using BSky.Controls;
using System.IO;
using BSky.Statistics.Common;
using System.Windows.Media;
using BSky.Lifetime;
using BSky.Statistics.Service.Engine.Interfaces;

//Aaron 01/12/2014
//Added this as it was difficult to disablethe syntax/paste on a subdialog. We never want any one to click paste on a sub dialog
//Note that we also use the subdialogwindow when I click a button on a canvas

namespace BSky.Interfaces.Commands
{
    /// <summary>
    /// Interaction logic for BaseOptionWindow.xaml
    /// </summary>
    public partial class SubDialogWindow : Window
    {
        private int expanderCollapsedWidth = 55;
        private int gapBetweenCanvasAndHelp = 3;
        private int gapBetweenCanvasAndOK = 3;
        private int OKCanceHelpHeight = 75;

        //IUIController UIController;
        public SubDialogWindow()
        {
            InitializeComponent();
            WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
            
        }

        private BSkyCanvas canvas;
        public FrameworkElement Template
        {
            get
            {
                //if (Host.Children.Count > 0)
                //    return Host.Children[0] as FrameworkElement;
                //else
                //    return null;

                if (Host.Child != null) return Host.Child as FrameworkElement;
                else return null;

            }
            set
            {
                if (value == null)
                {
                   // Host.Children.Clear();//13Feb2013 Delete all children. for testing and developing dialog session logic
                    Host.Child = null;
                    return;
                }
                canvas = value as BSkyCanvas;
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
                    this.Width = value.Width + expanderCollapsedWidth;//14May2015 last integer added by Anil ( for expander button)
                }
                if (value.Height != double.NaN)
                {
                    this.Host.Height = value.Height + gapBetweenCanvasAndOK;
                    this.Height = value.Height + OKCanceHelpHeight;//85
                }
                //Added by Aaron 05/05/2015
                //commented line below
               // Host.Children.Add(value);
                Host.Child = value;
                //Added  05/05/2015
                //Host = canvas;
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
            //Paste.IsEnabled = e.Value;
        }
        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            this.Tag = "Ok";
            this.DialogResult = true;
            BSkyCanvas.previewinspectMode = false;

            //Added aaron 05/05/2015
            DetachCanvas();
            this.Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            //Added aaron 05/05/2015
            DetachCanvas();
            this.Tag = "Cancel";
            this.DialogResult = true;
            //Added by Aaron 05/05
            //commented lines below
            //this.DialogResult = false;
            //BSkyCanvas.previewinspectMode = false;
            this.Close();
        }

        //Added aaron 05/05/2015
        public void DetachCanvas()
        {
            // int count1 = myCanvas.Children.Count;
            // myCanvas.Children.RemoveRange(0, count1);

           // Host.Child=null;
           // this.RemoveVisualChild(Host);
            //CanvasHost.Child = null;
          //  Host.KeyUp -= Host_KeyUp;
           // Host.PreviewKeyUp -= Host_PreviewKeyUp;
           // myCanvas.PreviewMouseLeftButtonDown -= myCanvas_PreviewMouseLeftButtonDown;
           // myCanvas.PreviewMouseLeftButtonUp -= DragFinishedMouseHandler;
            Host.Child = null;
            this.RemoveVisualChild(canvas);
            
            //myCanvas = null;
            //04/20/2013
            //Aaron
            //The code below detaches the canvas from the window of the subdialog on closing the sub dialog window
            //If this code is not run, we get a defect on opening the sub dialog (need to verify) saying the canvas is already a child of a window
           // this.RemoveVisualChild(Host);
            //Canvas = null;
        }









        //Added by Aaron
        //01/15/2014
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
            //System.Drawing.Image img = System.Drawing.Image.FromFile(@"C:\Users\Aiden\Documents\Gipin.jpg");
            //img.Tag = @"C:\Users\Aiden\Documents\Gipin.jpg";
            //System.Diagnostics.ProcessStartInfo info = new System.Diagnostics.ProcessStartInfo();
            //info.FileName = ("mspaint.exe");
            //info.Arguments = img.Tag.ToString(); // returns full path and name
            //System.Diagnostics.Process p1 = System.Diagnostics.Process.Start(info);
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
                    path = Path.GetFullPath(@".\Config\") + cs.internalhelpfilename;
                }
                else path = cs.Helpfile;

                // System.Diagnostics.Process p1 = System.Diagnostics.Process.Start(path);
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
        #region Help Expander 14May2015
        double expanderExpandedWidth = 300;
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
           
            //now try launching R help
            if(!BSkyCanvas.dialogMode)//if not a dialog designer mode. ( Dialog is in execution rather than creation mode)
            {
                IAnalyticsService analytics = LifetimeService.Instance.Container.Resolve<IAnalyticsService>();
                CommandRequest comreq = new CommandRequest();
                comreq.CommandSyntax = "print(" + rhelp + ")";//"print(help(library))";//
                analytics.ExecuteR(comreq, false, false);
            }
        }
        #endregion
    
    }
}
