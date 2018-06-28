using System;
using System.Windows;
using System.Windows.Controls;
using System.ComponentModel;
//using System.Windows.Forms;
using System.IO;
using System.Windows.Forms.Integration;
using BSky.Lifetime;


using BSky.Controls;

using BSky.Interfaces.Interfaces;


namespace BlueSky
{
    /// <summary>
    /// Interaction logic for InspectDialog.xaml
    /// </summary>
    /// 
      
    public partial class InspectDialog : Window
    {
        IUIController UIController;
        private System.Windows.Forms.PropertyGrid CanvasPropertyGrid;
        public WindowsFormsHost CanvasPropHost
        {
            get { return _CanvasPropHost; }
            set { _CanvasPropHost = value; }
        }



       
        public Border CanvasHost
        {
            get { return _CanvasHost; }
            set { _CanvasHost = value; }
        }

        


        public InspectDialog()
        {
            InitializeComponent();
            WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;

            this.Title = BSky.GlobalResources.Properties.UICtrlResources.InspectWinTitle;
            CanvasPropertyGrid = new System.Windows.Forms.PropertyGrid();
           // CanvasPropertyGrid.Size = new System.Drawing.Size(300, 250);
            // Bottom part of the screen
            CanvasPropHost.Child = CanvasPropertyGrid;
           // CanvasPropertyGrid.SelectedObject = myCanvas;
            CanvasPropertyGrid.HelpVisible = false;
            helpMessage.Text = BSky.GlobalResources.Properties.UICtrlResources.InspectHelpText;

            CanvasPropertyGrid.BrowsableAttributes = new AttributeCollection(
                                                        new Attribute[]
                                                            {
                                                                new CategoryAttribute("Dialog Properties") 
                                                            });


        }

        void canvas_CanExecuteChanged(object sender, BSkyBoolEventArgs e)
        {
            Ok.IsEnabled = e.Value;
            //Added by Aaron 01/02/2014
            //This is to ensure that unless all canexecutes are set to yes on all the controls on the dialog
            //the OK and Syntax buttons will not be enabled
            //Paste.IsEnabled = e.Value;
        }

        public FrameworkElement Template
        {
            get
            {
                return CanvasHost.Child as BSkyCanvas;
            }

            set
            {
                BSkyCanvas canvas = value as BSkyCanvas;
                canvas.CanExecuteChanged += new EventHandler<BSkyBoolEventArgs>(canvas_CanExecuteChanged);
                CanvasHost.Child = canvas;
            }

        }





        //public FrameworkElement Template
        //{
        //    get
        //    {
        //        if (Host.Children.Count > 0)
        //            return Host.Children[0] as FrameworkElement;
        //        else
        //            return null;
        //    }
        //    set
        //    {
        //        if (value == null)
        //        {
        //            Host.Children.Clear();//13Feb2013 Delete all children. for testing and developing dialog session logic
        //            return;
        //        }
        //        BSkyCanvas canvas = value as BSkyCanvas;
        //        // canvas.CanExecuteChanged += new EventHandler<BSkyBoolEventArgs>(canvas_CanExecuteChanged);
        //        if (canvas == null)
        //            return;
        //        if (!string.IsNullOrEmpty(canvas.Title))
        //            this.Title = canvas.Title;
        //        else
        //            this.Title = "Untitled Dialog";

        //        if (value.Width != double.NaN)
        //        {
        //            this.Host.Width = value.Width + 10;
        //            this.Width = value.Width + 30;
        //        }
        //        if (value.Height != double.NaN)
        //        {
        //            this.Host.Height = value.Height + 25;
        //            this.Height = value.Height + 85;
        //        }
        //        Host.Children.Add(value);
        //        //Added 01/15/2014
        //        //This disables the help button on the canvas when the help file is empty
        //        // if (canvas.Helpfile == null || canvas.Helpfile == string.Empty) help.IsEnabled = false;
        //    }
        //}

        public void setSizeOfPropertyGrid()
        {
            BSkyCanvas cs = CanvasHost.Child as BSkyCanvas;
            int width = Convert.ToInt32(cs.Width);
           // int height = 100;
            CanvasPropertyGrid.Width = width;
          //  CanvasPropertyGrid.Height = 200;

           // CanvasPropertyGrid.Size = new System.Drawing.Size(width,height );
           // CanvasPropertyGrid.Width = width;
        }



        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            this.Tag = "Ok";
            
            this.DialogResult = true;
            this.Close();
        }


        private void Ok_Click1(object sender, RoutedEventArgs e)
        {
            this.Tag = "Ok";
           
            this.DialogResult = true;
            this.Close();
        }

        private void Syntax_Click(object sender, RoutedEventArgs e)
        {
            
            PasteSyntax();
        }


        //This function returns the command string from the canvas of the dialog being inspected
        private string getCommand()
        {
            BSkyCanvas cs=CanvasHost.Child as BSkyCanvas;
            return cs.CommandString;
        }

         //This function returns the title of the canvas of the dialog being inspected
        private string getTitle()
        {
            BSkyCanvas cs=CanvasHost.Child as BSkyCanvas;
            return cs.Title;
        }

        private void Help_Click(object sender, RoutedEventArgs e)
        {

            string path;
            BSkyCanvas cs=CanvasHost.Child as BSkyCanvas;
            //string path="c:\aaron\ab.bc";
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


        private void PasteSyntax()
        {
           //copy to clipboard and return for this function
           // Clipboard.SetText(cmd.CommandSyntax);
           //Clipboard.SetText("asd dddddw");
           //Launch Syntax Editor window with command pasted /// 29Jan2013
           MainWindow mwindow = LifetimeService.Instance.Container.Resolve<MainWindow>();
           ////// Start Syntax Editor  //////
           SyntaxEditorWindow sewindow = LifetimeService.Instance.Container.Resolve<SyntaxEditorWindow>();
            sewindow.Owner = mwindow;
            //string syncomment = "# Use BSkyFormat(obj) to format the output.\n" +
            //    "# UAloadDataset(\'" + UIController.GetActiveDocument().FileName.Replace('\\', '/') +
            //    "\',  filetype=\'SPSS\', worksheetName=NULL, replace_ds=FALSE, csvHeader=TRUE, datasetName=\'" +
            //    UIController.GetActiveDocument().Name + "\' )\n";
          //  sewindow.PasteSyntax(syncomment + "asd dddddw");//paste command
            string syncomment = BSky.GlobalResources.Properties.UICtrlResources.syncomment+" " +"\""+ getTitle() +"\"";
            if (string.IsNullOrEmpty(getCommand())) syncomment = syncomment = BSky.GlobalResources.Properties.UICtrlResources.syncomment2 +"\""+ getTitle() +"\"";
            else  syncomment = syncomment + "\n" + getCommand();

            sewindow.PasteSyntax(syncomment);
            sewindow.Show();
            sewindow.WindowState = WindowState.Normal;
            sewindow.Activate();
            this.Close();
        }

    }
}