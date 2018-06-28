using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.ComponentModel;
using System.Windows.Markup;
using System.IO;
using BSky.Controls.Commands;
using System.Windows.Media;
using BSky.Interfaces.Commands;
using System.Text;
using ICSharpCode.SharpZipLib.Zip;
using System.Windows.Data;
using BSky.Statistics.Common;
using System.Collections.Generic;
using BSky.Interfaces.Controls;
using System.Reflection;
using BSky.Lifetime.Interfaces;
using BSky.Lifetime;
using BSky.RecentFileHandler;
using BSky.Controls.DesignerSupport;
using BSky.ConfService.Intf.Interfaces;

namespace BSky.Controls
{
    public partial class Window1 : Window
    {

        IConfigService confService = LifetimeService.Instance.Container.Resolve<IConfigService>();//12Dec2013
        RecentDocs recentfiles = LifetimeService.Instance.Container.Resolve<RecentDocs>();//21Dec2013
        ILoggerService logService = LifetimeService.Instance.Container.Resolve<ILoggerService>();
        bool gridLines = false;
       // public ResourceDictionary originalCanvas =new ResourceDictionary();

        AdornerLayer aLayer;
        CommonFunctions cf = new CommonFunctions();
        public const String FileNameFilter = "BSky Commands (*.bsky)|*.bsky";
        //Aaron 01/12/2012
        //Store name of file associated with the window. When BSky dialog is opened, nameofFile is set to name and path of file, when closed, set to empty 

        private double initialPosition = 10;

        private string nameOfFile = string.Empty;
        private string justTheName=string.Empty;
        //Aaron 01/20/2013
        //Added the global below to track whether the canvas has been saved;
        public static bool saved = true;
        public static bool checkSelectionproperties = true;
        public static BSkyCanvas firstCanvas = null;
        bool mainDialog = true;
        bool _isDown;
        bool _isDragging;

        //Aaron 04/28/2013
        // I believe that the line below keeps track of whether there is an item that is selected on the window
        //This is used in myCanvas_PreviewMouseLeftButtonDown to remove the adorner on the selected item 
        // Also used in  Window1_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) to remove the adorner on the selected item 
        //
        bool selected = false;
        private int subCanvasCount = 0;
        FrameworkElement selectedElement = null;

        //Added by Aaron 10/29/2013
        //Created this global to give me access to the selected object in the window in dialog editor mode.
        //This allows me to check that the properties I am entering in the condition for selection change behavior are the correct properties
        //The behavior class has no access to the selected object. Since the behavior class/object is instantiated from the dragdroplist class
        //one could pass the dragdroplist class to the behavior class by invoking the constructer of the behavior class with this as a parameter.
        //However this will not work as I need to store this (instantiation of the dragdroplist class as a property of the behavior class)
        //Storing an additional property will clutter the right hand side of the property grid with a property that makes no sense to the user
        public static FrameworkElement selectedElementRef = null;

        Point _startPoint;
        private double _originalLeft;
        private double _originalTop;


        // private System.Windows.Forms.PropertyGrid OptionsPropertyGrid;
        private FilteredPropertyGrid OptionsPropertyGrid;
        private System.Windows.Forms.PropertyGrid CanvasPropertyGrid;
        private BSkyCanvas myCanvas = null;

        //BSkyCanvas copy = new BSkyCanvas();
        BSkyCanvas copy = null;

        //Added by Aaron 05/13/2014

        private List<BSkyCanvas> saveChainOpenCanvas = new List<BSkyCanvas>();

        private void savelistofOpenCanvases()
        {
            foreach (BSkyCanvas obj in BSkyCanvas.chainOpenCanvas) saveChainOpenCanvas.Add(obj);
            BSkyCanvas.chainOpenCanvas.Clear();
        }

        private void restorelistofOpenCanvases()
        {
            BSkyCanvas.chainOpenCanvas.Clear();
            foreach (BSkyCanvas obj in saveChainOpenCanvas) BSkyCanvas.chainOpenCanvas.Add(obj);
            saveChainOpenCanvas.Clear();
        }


        public BSkyCanvas Canvas
        {
            get
            {
                return myCanvas;
            }
        }

        //Aaron: 12/08/2013
        //The constructer below is not called when the application is initialized
        //This is called if there is a sub dialog
        //There are 2 ways to call teh constructor. When main dialog is true, we display a canvas property grid. But certain values in the options property grid
        //are set as not required for a subdialog like command string, menulocation and output definition.
        //Also, for subdialogs (maindialog =false), on close we return back to the main dialog. This is not the same behavior when close is clicked from the 
        public Window1(BSkyCanvas obj, bool mainDialogparam)
        {
            InitializeComponent();

            mainDialog = mainDialogparam;
           // obj.Background = new SolidColorBrush(Colors.White);
            var converter = new System.Windows.Media.BrushConverter();
            obj.Background = (Brush)converter.ConvertFrom("#FFEEefFf");
            CanSave = true;
            CanOpen = true;
            CanSaveSubDialog = false;
            CanClose = true;

            //  obj.is = false;
            //Canvashost is the border around the canvas area in the XAML

            CanvasHost.Child = obj;
            //forcanvas.Children.Add(obj);

            myCanvas = obj;

            this.Title = nameOfFile = "Dialog1";
            //this.Title = nameOfFile = "Dialog1 - BlueSky Analytics Dialog Designer";
            //Aaron 09/02/2014 commented line below, causes title of sib dialogs to not save
         //   myCanvas.Title = "";
            //Added by Aaron 11/21/2013
            //This is to enable the checking of properties as defined in behavior.cs

            //BSkyCanvas.dialogMode = true;
            //Aaron: cleanup
            myCanvas.KeyDown += new KeyEventHandler(b_KeyDown);
            if (mainDialog == false)
            {
                myCanvas.OutputDefinition = "No entry required for a subdialog";
                myCanvas.MenuLocation = "No entry required for a subdialog";
                myCanvas.CommandString = "No entry required for a subdialog";

            }
            else
            {
                firstCanvas = obj;
            }
            myCanvas.Focusable = true;
            //Options propertygrid is the RHS that displays all the object properties
            OptionsPropertyGrid = new FilteredPropertyGrid();
            // OptionsPropertyGrid.Size = new System.Drawing.Size(300, 250);
            // ChangeDescriptionHeight(OptionsPropertyGrid, 180);
            //OptionsPropertyGrid.BrowsableAttributes = new AttributeCollection(
            //                                new Attribute[]
            //                                                {
            //                                                   new CategoryAttribute("Variable Settings"),  
            //                                                });

            OptionsPropertyGrid.BrowsableAttributes = new AttributeCollection(
                                            new Attribute[]
                                                          {
                                                             new CategoryAttribute("Variable Settings"),  new CategoryAttribute("Control Settings"), new CategoryAttribute("Syntax Settings"),new CategoryAttribute("Layout Properties"),new CategoryAttribute("Dataset Settings")
                                                          });


            // AttributeCollection testc = new AttributeCollection();
            // Attribute a =new Attribute[];

            //Host is the RHS property grid in the XAML file. The child holds the properties of all the items dropped on the Canvas
            host.Child = OptionsPropertyGrid;

            CanvasPropertyGrid = new System.Windows.Forms.PropertyGrid();
            CanvasPropertyGrid.Size = new System.Drawing.Size(300, 250);
            // Bottom part of the screen
            CanvasPropHost.Child = CanvasPropertyGrid;
            CanvasPropertyGrid.SelectedObject = myCanvas;
            CanvasPropertyGrid.HelpVisible = false;

            CanvasPropertyGrid.BrowsableAttributes = new AttributeCollection(
                                                        new Attribute[]
                                                            {
                                                                new CategoryAttribute("Dialog Properties") 
                                                            });

            //CommandBinding Savebinding = new CommandBinding(ApplicationCommands.Save);
            //Savebinding.Executed += new ExecutedRoutedEventHandler(Savebinding_Executed);

            // Anil Added following 21Jan2013
            CanvasPropertyGrid.PropertyValueChanged += new System.Windows.Forms.PropertyValueChangedEventHandler(CanvasPropertyGrid_PropertyValueChanged);
            OptionsPropertyGrid.PropertyValueChanged += new System.Windows.Forms.PropertyValueChangedEventHandler(OptionsPropertyGrid_PropertyValueChanged);


            //Save command
            CommandBinding SavePackbinding = new CommandBinding(ApplicationCommands.Save);
            SavePackbinding.CanExecute += new CanExecuteRoutedEventHandler(SubDialogCommands_CanExecute);
            SavePackbinding.Executed += new ExecutedRoutedEventHandler(SavePackbinding_Executed);

            //new command
            CommandBinding newBinding = new CommandBinding(ApplicationCommands.New);
            newBinding.CanExecute += new CanExecuteRoutedEventHandler(newBinding_CanExecute);
            newBinding.Executed += new ExecutedRoutedEventHandler(newBinding_Executed);

            ////Save as command
            CommandBinding SaveAsbinding = new CommandBinding(ApplicationCommands.SaveAs);
            SaveAsbinding.CanExecute += new CanExecuteRoutedEventHandler(SaveAs_CanExecute);
            SaveAsbinding.Executed += new ExecutedRoutedEventHandler(SaveAs_Executed);

            CommandBinding Closebinding = new CommandBinding(ApplicationCommands.Close);
            Closebinding.CanExecute += new CanExecuteRoutedEventHandler(Closebinding_CanExecute);
            Closebinding.Executed += new ExecutedRoutedEventHandler(Closebinding_Executed);

            CommandBinding SaveSubDialogBinding = new CommandBinding(DesignerCommands.SaveSubDialog);
            SaveSubDialogBinding.Executed += new ExecutedRoutedEventHandler(SaveSubDialogBinding_Executed);
            SaveSubDialogBinding.CanExecute += new CanExecuteRoutedEventHandler(SaveSubDialogBinding_CanExecute);

            CommandBinding AppExitBinding = new CommandBinding(DesignerCommands.Exit);
            AppExitBinding.Executed += new ExecutedRoutedEventHandler(AppExitBinding_Executed);
            AppExitBinding.CanExecute += new CanExecuteRoutedEventHandler(AppExitBinding_CanExecute);

            CommandBinding OpenBinding = new CommandBinding(ApplicationCommands.Open);
            OpenBinding.Executed += new ExecutedRoutedEventHandler(OpenBinding_Executed);
            OpenBinding.CanExecute += new CanExecuteRoutedEventHandler(OpenBinding_CanExecute);

            // CommandBinding propertiesBinding = new CommandBinding(ApplicationCommands.Properties);
            // propertiesBinding.Executed += new ExecutedRoutedEventHandler(propertiesBinding_Executed);

            CommandBinding commandBinding = new CommandBinding(DesignerCommands.RCommand);
            commandBinding.CanExecute += new CanExecuteRoutedEventHandler(commandBinding_CanExecute);
            commandBinding.Executed += new ExecutedRoutedEventHandler(commandBinding_Executed);

            CommandBinding previewBinding = new CommandBinding(DesignerCommands.Preview);
            previewBinding.CanExecute += new CanExecuteRoutedEventHandler(previewBinding_CanExecute);
            previewBinding.Executed += new ExecutedRoutedEventHandler(previewBinding_Executed);

            CommandBinding enableGridLinesBinding = new CommandBinding(DesignerCommands.EnableGridLines);
            enableGridLinesBinding.CanExecute += new CanExecuteRoutedEventHandler(enableGridLinesBinding_CanExecute);
            enableGridLinesBinding.Executed += new ExecutedRoutedEventHandler(enableGridLinesBinding_Executed);

            CommandBinding removeGridLinesBinding = new CommandBinding(DesignerCommands.removeGridLines);
            removeGridLinesBinding.CanExecute += new CanExecuteRoutedEventHandler(removeGridLinesBinding_CanExecute);
            removeGridLinesBinding.Executed += new ExecutedRoutedEventHandler(removeGridLinesBinding_Executed);


            CommandBinding locationBinding = new CommandBinding(DesignerCommands.MenuLocation);
            locationBinding.CanExecute += new CanExecuteRoutedEventHandler(menu_CanExecute);
            locationBinding.Executed += new ExecutedRoutedEventHandler(locationBinding_Executed);

            //CommandBinding InspectBinding = new CommandBinding(DesignerCommands.Inspection);
            //InspectBinding.CanExecute += new CanExecuteRoutedEventHandler(InspectBinding_CanExecute);
            //InspectBinding.Executed += new ExecutedRoutedEventHandler(InspectBinding_Executed);

            

            //   CommandBinding outputFile = new CommandBinding(DesignerCommands.OutputDefinition);
            // outputFile.CanExecute += new CanExecuteRoutedEventHandler(SubDialogCommands_CanExecute);
            // outputFile.Executed += new ExecutedRoutedEventHandler(outputFile_Executed);

            //  this.CommandBindings.Add(InspectBinding);
            //this.CommandBindings.Add(Savebinding);
            this.CommandBindings.Add(OpenBinding);
            // this.CommandBindings.Add(propertiesBinding);
            this.CommandBindings.Add(commandBinding);
            this.CommandBindings.Add(previewBinding);
            this.CommandBindings.Add(locationBinding);
            // this.CommandBindings.Add(outputFile);
            this.CommandBindings.Add(SavePackbinding);
            this.CommandBindings.Add(SaveSubDialogBinding);
            this.CommandBindings.Add(Closebinding);
            this.CommandBindings.Add(AppExitBinding);
            this.CommandBindings.Add(SaveAsbinding);
            this.CommandBindings.Add(newBinding);
            this.CommandBindings.Add(enableGridLinesBinding);
            this.CommandBindings.Add(removeGridLinesBinding);

        }





        //Aaron
        //12/07/2013
        //Added code below to support creation of a window without a canvas
        //This is called when the application is initialized
        public Window1()
        {
            InitializeComponent();

            recentfiles.recentitemclick = RecentItem_Click;
            RefreshRecent();//
            mainDialog = true;
            //obj.Background = new SolidColorBrush(Colors.White);
            CanSave = false;
            CanOpen = true;
            CanSaveSubDialog = false;
            CanClose = false;
            c1ToolbarStrip1.IsEnabled = false;

            //  obj.is = false;
            //Canvashost is the border around the canvas area in the XAML
            //CanvasHost.Child = obj;
            //myCanvas = obj;
            //this.Title = nameOfFile = "Dialog1";
            //myCanvas.Title = "";
            //Added by Aaron 11/21/2013
            //This is to enable the checking of properties as defined in behavior.cs

            //BSkyCanvas.dialogMode = true;
            //Aaron: cleanup
            //myCanvas.KeyDown += new KeyEventHandler(b_KeyDown);
            //if (mainDialog == false)
            //{
            //    myCanvas.OutputDefinition = "No entry required for a subdialog";
            //    myCanvas.MenuLocation = "No entry required for a subdialog";
            //    myCanvas.CommandString = "No entry required for a subdialog";

            //}
            //else
            //{
            //    firstCanvas = obj;
            //}
            //myCanvas.Focusable = true;
            //Options propertygrid is the RHS that displays all the object properties
            OptionsPropertyGrid = new FilteredPropertyGrid();
            //  ChangeDescriptionHeight(OptionsPropertyGrid, 20);
            OptionsPropertyGrid.Size = new System.Drawing.Size(400, 400);
            ChangeDescriptionHeight(OptionsPropertyGrid, 280);
            //OptionsPropertyGrid.BrowsableAttributes = new AttributeCollection(
            //                                new Attribute[]
            //                                                {
            //                                                   new CategoryAttribute("Variable Settings"),  
            //                                                });

            OptionsPropertyGrid.BrowsableAttributes = new AttributeCollection(
                                            new Attribute[]
                                                          {
                                                             new CategoryAttribute("Variable Settings"),  new CategoryAttribute("Control Settings"), new CategoryAttribute("Syntax Settings"),new CategoryAttribute("Layout Properties"),new CategoryAttribute("Dataset Settings")
                                                          });


            // AttributeCollection testc = new AttributeCollection();
            // Attribute a =new Attribute[];

            //Host is the RHS property grid in the XAML file. The child holds the properties of all the items dropped on the Canvas
            host.Child = OptionsPropertyGrid;

            CanvasPropertyGrid = new System.Windows.Forms.PropertyGrid();
            CanvasPropertyGrid.Size = new System.Drawing.Size(300, 250);
            // Bottom part of the screen
            CanvasPropHost.Child = CanvasPropertyGrid;
            CanvasPropertyGrid.SelectedObject = myCanvas;
            CanvasPropertyGrid.HelpVisible = false;

            CanvasPropertyGrid.BrowsableAttributes = new AttributeCollection(
                                                        new Attribute[]
                                                            {
                                                                new CategoryAttribute("Dialog Properties") 
                                                            });

            //CommandBinding Savebinding = new CommandBinding(ApplicationCommands.Save);
            //Savebinding.Executed += new ExecutedRoutedEventHandler(Savebinding_Executed);

            ////Anil Added following 21Jan2013
            CanvasPropertyGrid.PropertyValueChanged += new System.Windows.Forms.PropertyValueChangedEventHandler(CanvasPropertyGrid_PropertyValueChanged);

            OptionsPropertyGrid.PropertyValueChanged += new System.Windows.Forms.PropertyValueChangedEventHandler(OptionsPropertyGrid_PropertyValueChanged);


            //Save command
            CommandBinding SavePackbinding = new CommandBinding(ApplicationCommands.Save);
            SavePackbinding.CanExecute += new CanExecuteRoutedEventHandler(SubDialogCommands_CanExecute);
            SavePackbinding.Executed += new ExecutedRoutedEventHandler(SavePackbinding_Executed);

            //new command
            CommandBinding newBinding = new CommandBinding(ApplicationCommands.New);
            newBinding.CanExecute += new CanExecuteRoutedEventHandler(newBinding_CanExecute);
            newBinding.Executed += new ExecutedRoutedEventHandler(newBinding_Executed);

            ////Save as command
            CommandBinding SaveAsbinding = new CommandBinding(ApplicationCommands.SaveAs);
            SaveAsbinding.CanExecute += new CanExecuteRoutedEventHandler(SaveAs_CanExecute);
            SaveAsbinding.Executed += new ExecutedRoutedEventHandler(SaveAs_Executed);

            CommandBinding Closebinding = new CommandBinding(ApplicationCommands.Close);
            Closebinding.CanExecute += new CanExecuteRoutedEventHandler(Closebinding_CanExecute);
            Closebinding.Executed += new ExecutedRoutedEventHandler(Closebinding_Executed);

            CommandBinding SaveSubDialogBinding = new CommandBinding(DesignerCommands.SaveSubDialog);
            SaveSubDialogBinding.Executed += new ExecutedRoutedEventHandler(SaveSubDialogBinding_Executed);
            SaveSubDialogBinding.CanExecute += new CanExecuteRoutedEventHandler(SaveSubDialogBinding_CanExecute);

            CommandBinding AppExitBinding = new CommandBinding(DesignerCommands.Exit);
            AppExitBinding.Executed += new ExecutedRoutedEventHandler(AppExitBinding_Executed);
            AppExitBinding.CanExecute += new CanExecuteRoutedEventHandler(AppExitBinding_CanExecute);

            CommandBinding OpenBinding = new CommandBinding(ApplicationCommands.Open);
            OpenBinding.Executed += new ExecutedRoutedEventHandler(OpenBinding_Executed);
            OpenBinding.CanExecute += new CanExecuteRoutedEventHandler(OpenBinding_CanExecute);

            // CommandBinding propertiesBinding = new CommandBinding(ApplicationCommands.Properties);
            // propertiesBinding.Executed += new ExecutedRoutedEventHandler(propertiesBinding_Executed);

            CommandBinding commandBinding = new CommandBinding(DesignerCommands.RCommand);
            commandBinding.CanExecute += new CanExecuteRoutedEventHandler(commandBinding_CanExecute);
            commandBinding.Executed += new ExecutedRoutedEventHandler(commandBinding_Executed);

            CommandBinding previewBinding = new CommandBinding(DesignerCommands.Preview);
            previewBinding.CanExecute += new CanExecuteRoutedEventHandler(previewBinding_CanExecute);
            previewBinding.Executed += new ExecutedRoutedEventHandler(previewBinding_Executed);

            CommandBinding locationBinding = new CommandBinding(DesignerCommands.MenuLocation);
            locationBinding.CanExecute += new CanExecuteRoutedEventHandler(menu_CanExecute);
            locationBinding.Executed += new ExecutedRoutedEventHandler(locationBinding_Executed);

            //    CommandBinding InspectBinding = new CommandBinding(DesignerCommands.Inspection);
            //      InspectBinding.CanExecute += new CanExecuteRoutedEventHandler(InspectBinding_CanExecute);
            //   InspectBinding.Executed += new ExecutedRoutedEventHandler(InspectBinding_Executed);


            //   CommandBinding outputFile = new CommandBinding(DesignerCommands.OutputDefinition);
            // outputFile.CanExecute += new CanExecuteRoutedEventHandler(SubDialogCommands_CanExecute);
            // outputFile.Executed += new ExecutedRoutedEventHandler(outputFile_Executed);

            // this.CommandBindings.Add(InspectBinding);

            //   CommandBinding outputFile = new CommandBinding(DesignerCommands.OutputDefinition);
            // outputFile.CanExecute += new CanExecuteRoutedEventHandler(SubDialogCommands_CanExecute);
            // outputFile.Executed += new ExecutedRoutedEventHandler(outputFile_Executed);

            CommandBinding enableGridLinesBinding = new CommandBinding(DesignerCommands.EnableGridLines);
            enableGridLinesBinding.CanExecute += new CanExecuteRoutedEventHandler(enableGridLinesBinding_CanExecute);
            enableGridLinesBinding.Executed += new ExecutedRoutedEventHandler(enableGridLinesBinding_Executed);

            CommandBinding removeGridLinesBinding = new CommandBinding(DesignerCommands.removeGridLines);
            removeGridLinesBinding.CanExecute += new CanExecuteRoutedEventHandler(removeGridLinesBinding_CanExecute);
            removeGridLinesBinding.Executed += new ExecutedRoutedEventHandler(removeGridLinesBinding_Executed);

            //this.CommandBindings.Add(Savebinding);
            this.CommandBindings.Add(OpenBinding);
            // this.CommandBindings.Add(propertiesBinding);
            this.CommandBindings.Add(commandBinding);
            this.CommandBindings.Add(previewBinding);
            this.CommandBindings.Add(locationBinding);
            // this.CommandBindings.Add(outputFile);
            this.CommandBindings.Add(SavePackbinding);
            this.CommandBindings.Add(SaveSubDialogBinding);
            this.CommandBindings.Add(Closebinding);
            this.CommandBindings.Add(AppExitBinding);
            this.CommandBindings.Add(SaveAsbinding);
            this.CommandBindings.Add(newBinding);
            this.CommandBindings.Add(enableGridLinesBinding);
            this.CommandBindings.Add(removeGridLinesBinding);


        }

        // RecentDocs recentfiles = LifetimeService.Instance.Container.Resolve<RecentDocs>();//21Dec2013
        //17Jan2014

        #region refresh recent file list 21feb2013
        public void RefreshRecent()
        {
            MenuItem recent = GetMenuItemByHeaderPath("_File>Recent");
            try
            {
                recentfiles.RecentMI = recent;
            }
            catch (Exception ex)//17Jan2014
            {
                MessageBox.Show("Recent.xml not found...");
                logService.WriteToLogLevel("Recent.xml not found.\n" + ex.StackTrace, LogLevelEnum.Fatal);
            }
        }

        //search in direction File>Open... to find specific item in path
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
            if (ParentItem == null)//for Root node which is mainmenu
            {
                foreach (MenuItem itm in mainmenu.Items)
                {
                    if (itm.Header.ToString().Equals(ChildHeader))
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
                        if (itm.Header.ToString().Equals(ChildHeader))
                        {
                            mi = itm;
                            break;
                        }
                    }
                }
            }
            return mi;
        }

        private void RecentItem_Click(string fullpathfilename)
        {
            //here you need to write code to open dialog file.
            //dialog file name (and full path) is stored in parameter passed to this method
            // that is fullpathfilename
            // example: fullpathfilename = "C:\\Aaron\\Dialogs\\GML.bsky"

            //No need here. OpenDialogFile has it.
            if (!File.Exists(fullpathfilename))
            {
                recentfiles.RemoveXMLItem(fullpathfilename);
                MessageBox.Show(this, "File does not exists!", "File not found", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            //Use this to in your open file code, inside of this method.
            OpenDialogFile(fullpathfilename);//passing recent filename with full path, from recent file list
        }

        #endregion

        //Aaron
        //04/13/2013
        //This constructor is called in design mode when creating a sub dialog for the first time when clicking on the 
        //dialog designer property. 
        //
        public Window1(bool mainDialog)
            : this(BSkyControlFactory.Instance.CreateControl("Canvas") as BSkyCanvas, mainDialog)
        {

        }



        private bool checkDialog()
        {
            string filename = string.Empty;
            //Out are reference variables. So we pass filename to the SaveXaml function and even though SaveXaml does
            //not pass filename back, we can still access it. 
            //bool saveAs = false;
            List<string> lst;
            string message;
            bool retval = false;
            int outputFile = 0;
            //The function below recursively looks through the canvas looking for any objects with a missing name
            //The function returns a list of all the BSky dialog controls that have a missing name
            if (firstCanvas.Title == null || firstCanvas.Title == "")
            {
                message = string.Format("You need to enter a valid Title. The title will display in the output when command or dialog is run. It will also display in the title of the dialog");
                MessageBox.Show(message);
                return false;
            }

            if (gridLines == true)
            {
                message = "You neen to turn off the grid lines before saving. Click on Dialog->Remove Grid Lines.";
                MessageBox.Show(message);
                return false;
            }


            lst = checkMissingName(firstCanvas);

            if (lst.Count > 0)
            {
                if (lst.Count == 1) message = string.Format("You need to enter the Name property for the following control \n{0} ", string.Join(",", lst));
                else message = string.Format("You need to enter the Name property for the following controls \n{0} ", string.Join("\n", lst));
                MessageBox.Show(message);
                return false;
            }
            //Added by Aaron 10/24/2013
            //Code below checks the BSkyVariableMoveButton and ensures that the source and destination lists are valid
            //This is important as I can setup a variablemovebutton with valid source and destination variable lists and the delete any one list
            //and then click save
            retval = checkMoveButton(firstCanvas);
            //Holds the griditem in the canvasPropertyGrid for the outputDefinition
            if (!retval) return false;
            System.Windows.Forms.GridItem outputDefGridItem;


            if (firstCanvas.OutputDefinition != null)
            {
                if (firstCanvas.OutputDefinition != "")
                {
                    if (!File.Exists(firstCanvas.OutputDefinition))
                    {
                        //MessageBox
                        System.Windows.Forms.MessageBox.Show("The Output Definition File cannot be accessed. Check the path and file name");
                        return false;
                    }
                }
            }
            //  outputFile = checkMissingOutputFile(firstCanvas);

            //Case where I want to enter an output file or the file specified is invalid, so I return with the outputfile highlighted
            //In all other cases I proceed without an output file definition
            //  if (outputFile == 1 || outputFile == 3)
            // {
            //   outputDefGridItem = GetSelectedGridItem("OutputDefinition", CanvasPropertyGrid);
            // CanvasPropertyGrid.Focus();
            // outputDefGridItem.Select();
            // return false;
            //}

            //Since we support master slave listboxes, we need to handle the following scenarios
            //1. The case that a master listbox points to a slave that does not exist or was deleted
            //2. Master listbox points has an empty slave, this is not fine and we will throw an error 
            //3. One or more master listboxes point to the same slave
            //4. NOT AN ERROR CONDITION Master listbox points to a valid slave but no master slave mappings created. In this situation as there are no mappings as soon as an entry in the master is selected, the slave blanks out as there are no entries in the slave that map to the master. The initial settings for the slave is to display all entries
            //5. NOT AN ERROR CONDITION In the case that the entry selected does not have any mapping to the slave, the slave blanks out
            if (!checkmasterslave()) return false;

            //Added by Aaron 02/14/2014
            //I need to check every textBoxName property for gridForSymbols to ensure that it points to a valid textBox
            if (!checkTextBoxNameinGridforSym(firstCanvas)) return false;
            if (!checkTextBoxNameinGridforCompute(firstCanvas)) return false;
            else return true;

        }

        private static void ChangeDescriptionHeight(System.Windows.Forms.PropertyGrid grid, int height)
        {
            if (grid == null) throw new ArgumentNullException("grid");

            foreach (System.Windows.Forms.Control control in grid.Controls)
                if (control.GetType().Name == "DocComment")
                {
                    FieldInfo fieldInfo = control.GetType().BaseType.GetField("userSized",
                      BindingFlags.Instance |
                      BindingFlags.NonPublic);
                    fieldInfo.SetValue(control, true);
                    control.Height = height;
                    return;
                }
        }

        //Anil Added following 21Jan2013
        //Checks if you have entered a duplicate name control 
        void OptionsPropertyGrid_PropertyValueChanged(object s, System.Windows.Forms.PropertyValueChangedEventArgs e)
        {
            if (e.ChangedItem.Label == "Name") checkDuplicateName(e);

            //Aaron 10/21/2013
            //Added code to ensure that valid integer value is added in the maxNoOfVariables field
            if (e.ChangedItem.Label == "maxNoOfVariables")
                verifyType(e, "double");
            //Added 10/23/2013
            //Added code to ensure that filtering works in dialog editor mode
            //When changing the filter in dialog editor mode, we will filter the 3 variablesin the variable list

            if (e.ChangedItem.Label == "Filter")
            {
                if (selectedElement is DragDropList)
                {
                    DragDropList templist = selectedElement as DragDropList;
                    ListCollectionView lvw = templist.ItemsSource as ListCollectionView;
                    if (lvw != null)
                        lvw.Filter = new Predicate<object>(templist.CheckForFilter);
                }
                else if (selectedElement is BSkyGroupingVariable)
                {
                    BSkyGroupingVariable tempgrpvar = selectedElement as BSkyGroupingVariable;
                    foreach (object child in tempgrpvar.Children)
                    {
                        if (child is SingleItemList)
                        {
                            //vTargetList = child12 as DragDropList;
                            // targetListName = value;
                            //DragList child123;
                            SingleItemList child1 = child as SingleItemList;
                            //child1.Filter = value;
                            //child1.nomlevels = nomlevels;
                            //child1.ordlevels = ordlevels;
                            ListCollectionView lvw = child1.ItemsSource as ListCollectionView;
                            if (lvw != null)
                                lvw.Filter = new Predicate<object>(child1.CheckForFilter);
                        }

                    }

                }
            }

            //Added by Aaron
            //when entering a slave listbox name, I check if the name entered points to a valid listbox
            if (e.ChangedItem.Label == "SlaveListBoxName")
            {

                bool validslavename = false;
                BSkyMasterListBox mslb = selectedElement as BSkyMasterListBox;
                validslavename = mslb.checkIfValidChild(e.ChangedItem.Value as string);
                if (!validslavename) MessageBox.Show("You have entered an incorrect value for the SlaveListBoxName, please make sure that you enter a the name of a valid ListBox");
            }

            //Added by Aaron 02/14/2014
            //when entering a textBoxName for a gridofsymbols, I check if the name entered points to a valid textbox
            if (e.ChangedItem.Label == "textBoxName")
            {
                BSkygridForSymbols symGrid = selectedElement as BSkygridForSymbols;
                if (!symGrid.checkValidTextBox(e.ChangedItem.Value as string))
                {
                    MessageBox.Show("The textBoxName value must be the name of a valid TextBox, please make sure you enter the name of a valid textbox");
                    //Added by Aaron, code below resets the textBoxName property
                    symGrid.textBoxName = "";
                    this.OptionsPropertyGrid.ResetSelectedProperty();
                }
            }

            if (e.ChangedItem.Label == "TextBoxNameForSyntaxSubstitution")
            {
                BSkygridForCompute symGrid = selectedElement as BSkygridForCompute;
                if (!symGrid.checkValidTextBox(e.ChangedItem.Value as string))
                {
                    MessageBox.Show("The textBoxNameForSyntax substitution value must be the name of a valid TextBox, please make sure you enter the name of a valid textbox");
                    //Added by Aaron, code below resets the textBoxName property
                    symGrid.TextBoxNameForSyntaxSubstitution = "";
                    this.OptionsPropertyGrid.ResetSelectedProperty();
                }
            }

            //  if (e.ChangedItem.Label == "CanExecute" || e.ChangedItem.Label == "Enabled" )
            //    verifyType(e.ChangedItem.Label, "double");
            //  if (e.ChangedItem.Label == "CanExecute" || e.ChangedItem.Label == "Enabled")
            //    verifyType(e.ChangedItem.Label, "bool");
            // if (e.ChangedItem.Label == "SubstituteSettings" || e.ChangedItem.Label == "Filter" || e.ChangedItem.Label == "Syntax")
            //   verifyType(e.ChangedItem.Label, "string");
            saved = false;
        }


        ////Aaron Added following 02/02/2014
        //Checking whether the help file and output definition values are valid        


        void CanvasPropertyGrid_PropertyValueChanged(object s, System.Windows.Forms.PropertyValueChangedEventArgs e)
        {
            if (e.ChangedItem.Label == "Helpfile")
            {
                if (!File.Exists(e.ChangedItem.Value as string))
                {
                    Uri myUri;
                    string url = e.ChangedItem.Value as string;
                    if (!url.StartsWith("http://"))
                        url = "http://" + url;
                    if (!Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out myUri))
                    {
                        MessageBox.Show("You have entered an invalid entry for Helpfile, Helpfile must be a valid file path or URL");
                    }
                }
            }
            if (e.ChangedItem.Label == "OutputDefinition")
            {
                if (!File.Exists(e.ChangedItem.Value as string))
                {
                    MessageBox.Show("You have entered an incorrect file path for the OutputDefinition entry of the file does not exist.");
                }
            }
            saved = false;
        }


        //Added by Aaron 10/21/2013
        //Code below verifies that a valid value is entered 
        public void verifyType(System.Windows.Forms.PropertyValueChangedEventArgs e, string type)
        {
            bool isdouble = false;
            bool isstring = false;
            bool isbool = false;
            double result = -1;
            bool booleanresult;
            string message = string.Empty;
            string label = e.ChangedItem.Label;
            //System.Windows.Forms.DialogResult diagResult;
            if (type == "double")
            {
                isdouble = Double.TryParse(e.ChangedItem.Value.ToString(), out result);

                message = "You need to enter a valid numeric for the label " + label;
                if (!isdouble)
                {
                    message = "You need to enter a valid numeric for the label " + label;
                    MessageBox.Show(message);
                    DragDropList melist = this.OptionsPropertyGrid.SelectedObject as DragDropList;
                    melist.maxNoOfVariables = string.Empty;
                    this.OptionsPropertyGrid.Refresh();
                    //Added by Aaron 10/21/2013
                    //The line of code below should replace the above 3 lines, however I don't know why the code is not working
                    // this.OptionsPropertyGrid.ResetSelectedProperty();
                }
                return;

            }
            //if (type == "string")
            //{
            //    isdouble = string.TryParse(label, out result);
            //    message = "You need to enter a valid numeric for the label " + label;
            //    if (!isdouble) diagResult = System.Windows.Forms.MessageBox.Show(message, "Error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Question);

            //}
            if (type == "bool")
            {
                isbool = Boolean.TryParse(label, out booleanresult);
                message = "You need to enter a valid boolean value for the label " + label;
                if (!isbool) MessageBox.Show(message);
            }
        }

        public void checkDuplicateName(System.Windows.Forms.PropertyValueChangedEventArgs e)
        {

            int len = 0;
            int i = 0;
            //11/24/2013
            //Keeps references to open canvases. As the open canvases that have not been saved cannot be accessed through the resources of the the button, I need a reference as I need to serach these canvases for controls and duplicate names
            //All I need to do is look for duplicates in the chain of open canvases. The saved resources are automatically accounted for
            if (BSkyCanvas.chainOpenCanvas.Count > 0)
            {
                len = BSkyCanvas.chainOpenCanvas.Count;
                while (i < len)
                {
                    findDuplicate(e, BSkyCanvas.chainOpenCanvas[i]);

                    i = i + 1;

                }
            }
        }

        // 03/25/2013
        //The function below checks for duplicate names in the dialogs and sub dialogs created
        //If a duplicate name is detected, a message is displayed and the duplicate name is cleared from the selected control
        //I have created a new Interface, IBSkyControl that contains a single property name, this simplifies the code greatly as I check whether
        //the control is a IBSkyControl and then match the name property

        private void findDuplicate(System.Windows.Forms.PropertyValueChangedEventArgs e, BSkyCanvas canvas)
        {
            string message;
            foreach (Object obj in canvas.Children)
            {
                if (obj is IBSkyControl && obj != selectedElement)
                {
                    IBSkyControl ib = obj as IBSkyControl;
                    if (ib.Name == e.ChangedItem.Value.ToString())
                    {
                        message = string.Format("You have already created a control with the name \"{0}\", please enter a unique name", e.ChangedItem.Value.ToString());
                        MessageBox.Show(message);
                        this.OptionsPropertyGrid.ResetSelectedProperty();
                        return;
                    }
                    //05/18/2013
                    //Added by Aaron
                    //Code below checks the radio buttons within each radiogroup looking for duplicate names
                    if (obj is BSkyRadioGroup)
                    {
                        BSkyRadioGroup ic = obj as BSkyRadioGroup;
                        StackPanel stkpanel = ic.Content as StackPanel;

                        foreach (object obj1 in stkpanel.Children)
                        {
                            BSkyRadioButton btn = obj1 as BSkyRadioButton;
                            //Added by Aaron 09/15/2014
                            //added condition btn != selectedElement 
                            if (btn.Name == e.ChangedItem.Value.ToString() && obj1 != selectedElement)
                            {
                                message = string.Format("You have already created a control with the name \"{0}\", please enter a unique name", e.ChangedItem.Value.ToString());
                                MessageBox.Show(message);
                                this.OptionsPropertyGrid.ResetSelectedProperty();
                                return;
                            }
                        }

                    }

                }
                if (obj is BSkyButton)
                {
                    FrameworkElement fe = obj as FrameworkElement;
                    BSkyCanvas cs = fe.Resources["dlg"] as BSkyCanvas;
                    if (cs != null) findDuplicate(e, cs);
                }
            }
            return;
        }

        // Added by Aaron 03/31/2013
        // The function returns all the controls with missing names.
        //I have created a new Interface, IBSkyControl that contains a single property name, this simplifies the code greatly as I check whether
        //the control is a IBSkyControl and then match the name property

        private List<string> checkMissingName(BSkyCanvas canvas)
        {

            List<string> lst = new List<string>();
            foreach (Object obj in canvas.Children)
            {
                IBSkyControl ib = obj as IBSkyControl;
                if (ib.Name == "" || ib.Name==null)
                {
                    switch (ib.GetType().Name)
                    {
                        case "BSkySourceList":
                           // BSkySourceList vl = ib as BSkySourceList;
                            //if (vl.AutoVar == true) lst.Add("Target list");
                             lst.Add("Source List");
                            break;

                        case "BSkyTargetList":
                            //BSkyTargetList v2 = ib as BSkyTargetList;
                            //if (v2.AutoVar == true) lst.Add("Target list");
                            lst.Add("Target List");
                            break;

                        case "BSkyListBoxwBorderForDatasets":
                            BSkyListBoxwBorderForDatasets v3 = ib as BSkyListBoxwBorderForDatasets;
                            if (v3.AutoPopulate == true) lst.Add("Source Dataset List");
                            else lst.Add("Destination Dataset List");
                            break;

                       
                        case "BSkyButton":
                            lst.Add("Button");
                            break;

                        case "BSkyTextBox":
                            lst.Add("Textbox");
                            break;

                        case "BSkyGroupBox":
                            lst.Add("Group Box");
                            break;

                        case "BSkyCheckBox":
                            lst.Add("Check Box");
                            break;

                        case "BSkyRadioButton":
                            lst.Add("Radio Button");
                            break;

                        case "BSkyComboBox":
                            lst.Add("Combo Box");
                            break;

                        case "BSkyLabel":
                            lst.Add("Label");
                            break;

                        case "BSkyCanvas":
                            lst.Add("Canvas");
                            break;

                        case "BSkyMultiLineLabel":
                            lst.Add("BSkyMultiLineLabel");
                            break;

                        case "BSkyVariableMoveButton":
                            lst.Add("Move Button");
                            break;

                        case "BSkyRadioGroup":
                            lst.Add("Radio Group");
                            break;

                        case "BSkyBrowse":
                            lst.Add("Browse");
                            break;
                    }
                } //end of if
                if (obj is BSkyButton)
                {
                    FrameworkElement fe = obj as FrameworkElement;
                    BSkyCanvas cs = fe.Resources["dlg"] as BSkyCanvas;
                    if (cs != null) lst.AddRange(checkMissingName(cs));
                }
            } //end of for
            return lst;
        }


        void outputFile_Executed(object sender, ExecutedRoutedEventArgs e)
        {

            OutputFileLocationDlg dlg = new OutputFileLocationDlg();
            dlg.FileLocation = myCanvas.OutputDefinition;
            dlg.ShowDialog();
            if (dlg.DialogResult.HasValue && dlg.DialogResult.Value)
            {
                myCanvas.OutputDefinition = dlg.FileLocation;
            }
        }


        void previewBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (mainDialog)
            {
                if (CanClose)
                    e.CanExecute = true;
            }
            else
                e.CanExecute = false;
        }


        void removeGridLinesBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (gridLines == true)
            {
                e.CanExecute = true;

            }
            else
                e.CanExecute = false;

        }



        void enableGridLinesBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (gridLines ==true)
            {
              e.CanExecute = false; 
                    
            }
            else
                e.CanExecute = true;
                
        }


        void enableGridLinesBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            myCanvas.drawLines();
            gridLines = true;

        }

        void removeGridLinesBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            myCanvas.removeLines();
            gridLines = false;

        }


        void menu_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (mainDialog)
            {
                if (CanClose)
                    e.CanExecute = true;
            }
            else
                e.CanExecute = false;
        }


        void commandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (mainDialog)
            {
                if (CanOpen)
                    e.CanExecute = true;
            }
            else
                e.CanExecute = false;
        }

        void newBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (mainDialog)
            {
                e.CanExecute = true;
            }
            else
                e.CanExecute = true;
        }


        // I can call new when there are no open dialogs or I have an open dialog
        //Here I have to reconstruct the canvas and add it to the window
        void newBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {

            System.Windows.Forms.DialogResult result;
            //tHIS is the case when I call new and there is no open canvas i.e. I have just called close.
            if (CanClose == false)
            {
                BSkyCanvas obj = BSkyControlFactory.Instance.CreateControl("Canvas") as BSkyCanvas;
                initialPosition = 10;
                mainDialog = true;
               // obj.Background = new SolidColorBrush(Colors.White);
                //Added by Aaron 08/31/2014
                //3.	The multiline label control (which is actually a textbox)must have a light grey color to match the canvas. 
                //However the textbox needs to have a background color of white to contrast with the light blue back ground of the canvas
                //This creates a problem, as I cant have the textbox show with a white background and the multi-line label with light blue
                //One way to do this is have the textbox show with a white background and the multi-line label show with a while back ground in dialog editor mode (originally the canvas had a white back ground in dialog editor mode)
                //But when I am in execute mode, the canvas has a light blue color, and I would have to switch the back ground of the multi-line label
                //in dialog editor mode to light blue instead of transparent of the textbox

                //The reason it needs to have a light grey color is 
                //This looks weird on canvas of dialog editor mode as color is white.WE may want the canvas in dialog editor mode to be light grey. 
                //I have made this change

                var converter = new System.Windows.Media.BrushConverter();
                //base.Background = (Brush)converter.ConvertFromString("#FFEDefFf");
                obj.Background = (Brush)converter.ConvertFrom("#FFEEefFf");
                CanSave = true;
                CanOpen = true;
                CanSaveSubDialog = false;

                //Added by Aaron 05/12/2014
                BSkyCanvas.chainOpenCanvas.Add(obj);


                //Canvashost is the border around the canvas area in the XAML
                CanvasHost.Child = obj;
                //forcanvas.Children.Add(obj);
                myCanvas = obj;

                //Added by Aaron 11/21/2013
                //This is to enable the checking of properties as defined in behavior.cs

                //BSkyCanvas.dialogMode = true;
                //Aaron: cleanup
                myCanvas.KeyDown += new KeyEventHandler(b_KeyDown);
                if (mainDialog == false)
                {
                    myCanvas.OutputDefinition = "No entry required for a subdialog";
                    myCanvas.MenuLocation = "No entry required for a subdialog";
                    myCanvas.CommandString = "No entry required for a subdialog";

                }
                else
                {
                    firstCanvas = obj;
                }
                myCanvas.Focusable = true;
                //OptionsPropertyGrid = new FilteredPropertyGrid();
                //OptionsPropertyGrid.Size = new System.Drawing.Size(300, 250);
                ////OptionsPropertyGrid.BrowsableAttributes = new AttributeCollection(
                ////                                new Attribute[]
                ////                                                {
                ////                                                   new CategoryAttribute("Variable Settings"),  
                ////                                                });

                //OptionsPropertyGrid.BrowsableAttributes = new AttributeCollection(
                //                                new Attribute[]
                //                                              {
                //                                                 new CategoryAttribute("Variable Settings"),  new CategoryAttribute("Control Settings"), new CategoryAttribute("Syntax Settings"),new CategoryAttribute("Layout Properties")
                //                                              });


                //// AttributeCollection testc = new AttributeCollection();
                //// Attribute a =new Attribute[];

                ////Host is the RHS property grid in the XAML file. The child holds the properties of all the items dropped on the Canvas
                //host.Child = OptionsPropertyGrid;

                //CanvasPropertyGrid = new System.Windows.Forms.PropertyGrid();
                //CanvasPropertyGrid.Size = new System.Drawing.Size(300, 250);
                //// Bottom part of the screen
                //CanvasPropHost.Child = CanvasPropertyGrid;
                CanvasPropertyGrid.SelectedObject = myCanvas;
                //CanvasPropertyGrid.HelpVisible = false;

                //CanvasPropertyGrid.BrowsableAttributes = new AttributeCollection(
                //                                            new Attribute[]
                //                                                {
                //                                                    new CategoryAttribute("BlueSky") 
                //                                                });

                //CommandBinding Savebinding = new CommandBinding(ApplicationCommands.Save);
                //Savebinding.Executed += new ExecutedRoutedEventHandler(Savebinding_Executed);

                ////Anil Added following 21Jan2013
                //CanvasPropertyGrid.PropertyValueChanged += new System.Windows.Forms.PropertyValueChangedEventHandler(CanvasPropertyGrid_PropertyValueChanged);
                //OptionsPropertyGrid.PropertyValueChanged += new System.Windows.Forms.PropertyValueChangedEventHandler(OptionsPropertyGrid_PropertyValueChanged);


                ////Save command
                //CommandBinding SavePackbinding = new CommandBinding(ApplicationCommands.Save);
                //SavePackbinding.CanExecute += new CanExecuteRoutedEventHandler(SubDialogCommands_CanExecute);
                //SavePackbinding.Executed += new ExecutedRoutedEventHandler(SavePackbinding_Executed);

                ////new command
                //CommandBinding newBinding = new CommandBinding(ApplicationCommands.New);
                //newBinding.CanExecute += new CanExecuteRoutedEventHandler(newBinding_CanExecute);
                //newBinding.Executed += new ExecutedRoutedEventHandler(newBinding_Executed);

                //////Save as command
                //CommandBinding SaveAsbinding = new CommandBinding(ApplicationCommands.SaveAs);
                //SaveAsbinding.CanExecute += new CanExecuteRoutedEventHandler(SaveAs_CanExecute);
                //SaveAsbinding.Executed += new ExecutedRoutedEventHandler(SaveAs_Executed);

                //CommandBinding Closebinding = new CommandBinding(ApplicationCommands.Close);
                //Closebinding.CanExecute += new CanExecuteRoutedEventHandler(Closebinding_CanExecute);
                //Closebinding.Executed += new ExecutedRoutedEventHandler(Closebinding_Executed);

                //CommandBinding SaveSubDialogBinding = new CommandBinding(DesignerCommands.SaveSubDialog);
                //SaveSubDialogBinding.Executed += new ExecutedRoutedEventHandler(SaveSubDialogBinding_Executed);
                //SaveSubDialogBinding.CanExecute += new CanExecuteRoutedEventHandler(SaveSubDialogBinding_CanExecute);

                //CommandBinding AppExitBinding = new CommandBinding(DesignerCommands.Exit);
                //AppExitBinding.Executed += new ExecutedRoutedEventHandler(AppExitBinding_Executed);
                //AppExitBinding.CanExecute += new CanExecuteRoutedEventHandler(AppExitBinding_CanExecute);

                //CommandBinding OpenBinding = new CommandBinding(ApplicationCommands.Open);
                //OpenBinding.Executed += new ExecutedRoutedEventHandler(OpenBinding_Executed);
                //OpenBinding.CanExecute += new CanExecuteRoutedEventHandler(OpenBinding_CanExecute);

                //CommandBinding propertiesBinding = new CommandBinding(ApplicationCommands.Properties);
                //propertiesBinding.Executed += new ExecutedRoutedEventHandler(propertiesBinding_Executed);

                //CommandBinding commandBinding = new CommandBinding(DesignerCommands.RCommand);
                //commandBinding.Executed += new ExecutedRoutedEventHandler(commandBinding_Executed);

                //CommandBinding previewBinding = new CommandBinding(DesignerCommands.Preview);
                //previewBinding.Executed += new ExecutedRoutedEventHandler(previewBinding_Executed);

                //CommandBinding locationBinding = new CommandBinding(DesignerCommands.MenuLocation);
                //locationBinding.Executed += new ExecutedRoutedEventHandler(locationBinding_Executed);

                //CommandBinding outputFile = new CommandBinding(DesignerCommands.OutputDefinition);
                //outputFile.CanExecute += new CanExecuteRoutedEventHandler(SubDialogCommands_CanExecute);
                //outputFile.Executed += new ExecutedRoutedEventHandler(outputFile_Executed);


                ////this.CommandBindings.Add(Savebinding);
                //this.CommandBindings.Add(OpenBinding);
                //this.CommandBindings.Add(propertiesBinding);
                //this.CommandBindings.Add(commandBinding);
                //this.CommandBindings.Add(previewBinding);
                //this.CommandBindings.Add(locationBinding);
                //this.CommandBindings.Add(outputFile);
                //this.CommandBindings.Add(SavePackbinding);
                //this.CommandBindings.Add(SaveSubDialogBinding);
                //this.CommandBindings.Add(Closebinding);
                //this.CommandBindings.Add(AppExitBinding);
                //this.CommandBindings.Add(SaveAsbinding);
                myCanvas.KeyUp += new KeyEventHandler(myCanvas_KeyUp);
                myCanvas.PreviewKeyUp += new KeyEventHandler(myCanvas_PreviewKeyUp);
                myCanvas.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(myCanvas_PreviewMouseLeftButtonDown);
                myCanvas.PreviewMouseLeftButtonUp += new MouseButtonEventHandler(DragFinishedMouseHandler);
                c1ToolbarStrip1.IsEnabled = true;
                CanClose = true;
                nameOfFile = "Dialog1";
                Title = "Dialog1";
                myCanvas.Title = "";
                return;
            }//myCanvas.Focus();
            //Aaron
            //Added 02/10/2013
            //This ensures that the close command is invoked which handles whether the existing dialog definition should be saved 
            //I commented the line below as I would have liked ApplicationCommands.Close.Execute
            //to return a value 0,1,2 to indicate yes, no, cancel. If yes, I would not open a new browse window to select the file
            //  if (saved == false) ApplicationCommands.Close.Execute(null, this);
            //Aaron added 06/16/2013
            //Code below closes the existing open and saved canvas before opening the new one

            //Case of calling new when there is a saved dialog open
            if (saved == true)
            {
                int count = myCanvas.Children.Count;
                initialPosition = 10;
                myCanvas.Children.RemoveRange(0, count);
                //Added by Aaron 05/12/2014
                BSkyCanvas.chainOpenCanvas.Clear();
                myCanvas.Background = new SolidColorBrush(Colors.White);
                c1ToolbarStrip1.IsEnabled = true;
                //If this is the subdialog, we return to the main dialog
                if (mainDialog == false)
                {
                    this.DialogResult = false;
                    //this.Close();
                }
                // selectedElement = null;
                //We reset the main dialog
                //Resetting the name of the file
                nameOfFile = string.Empty;
                OptionsPropertyGrid.SelectedObject = null;
                myCanvas.Width = 470;
                myCanvas.Height = 300;
                Title = nameOfFile = "Dialog1";
                myCanvas.Title = "";
                myCanvas.CommandString = "";
                myCanvas.OutputDefinition = "";

                //myCanvas.Children.Clear();

                CanvasPropertyGrid.SelectedObject = myCanvas;
                //OptionsPropertyGrid.SelectedObject = myCanvas;
                //Added by Aaron
                //02/10/2013
                //Setting saved =true so that when I open another dialog, you are not prompted to save
                saved = true;
                selectedElement = null;
                //Aaron 10/29/2013
                //added line below
                selectedElementRef = null;


            }

            //Calling new when there is an open dialog that I don't want to save
            if (saved == false)
            {

                result = System.Windows.Forms.MessageBox.Show("Do you want to save changes?", "Save Changes", System.Windows.Forms.MessageBoxButtons.YesNoCancel, System.Windows.Forms.MessageBoxIcon.Question);
                if (result == System.Windows.Forms.DialogResult.Yes)//save
                {
                    fileSave(false);
                    CanClose = true;
                    return;
                }
                if (result == System.Windows.Forms.DialogResult.Cancel)//save
                {
                    return;
                }

                if (result == System.Windows.Forms.DialogResult.No)//save
                {

                    int count = myCanvas.Children.Count;
                    myCanvas.Children.RemoveRange(0, count);
                    initialPosition = 10;


                    //If this is the subdialog, we return to the main dialog
                    if (mainDialog == false)
                    {
                        this.DialogResult = false;
                        //this.Close();
                    }
                    // selectedElement = null;
                    //We reset the main dialog
                    //Resetting the name of the file
                    c1ToolbarStrip1.IsEnabled = true;
                    nameOfFile = string.Empty;
                    OptionsPropertyGrid.SelectedObject = null;
                    myCanvas.Width = 470;
                    myCanvas.Height = 300;
                    Title = nameOfFile = "Dialog1";
                    myCanvas.Title = "";
                    myCanvas.CommandString = "";
                    myCanvas.OutputDefinition = "";

                    //myCanvas.Children.Clear();

                    CanvasPropertyGrid.SelectedObject = myCanvas;
                    //OptionsPropertyGrid.SelectedObject = myCanvas;
                    //Added by Aaron
                    //02/10/2013
                    //Setting saved =true so that when I open another dialog, you are not prompted to save
                    saved = true;
                    selectedElement = null;
                    //Aaron 10/29/2013
                    //added line below
                    selectedElementRef = null;

                }

            }
        }

        void SubDialogCommands_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (!CanSave)
            {
                e.CanExecute = false;
            }
            else
                e.CanExecute = true;
        }

        void SaveSubDialogBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (!CanSaveSubDialog)
            {
                e.CanExecute = false;
            }
            else
                e.CanExecute = true;

        }


        void AppExitBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (mainDialog == true)
                e.CanExecute = true;
            else e.CanExecute = false;

        }

        void AppExitBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            this.Close();//Anil: I have commented(multi-line comment) code block below and instead using this statement

            /* Anil: I think we can call window closing event here(written above) instead which 
             * already contains following code to get SAVE confirmation from user to save or exit 
             * without saving.
             * If I do not comment the code below, the window close event is also raised as soon as 
             * Application.Current.Shutdown(); executes below and SAVE confirmation 
             * message box appears again from window closing event.
             * First confrmation message box is below second one is inside window closing event handler.
             * So if I comment following I will see only one SAVE confirmation message box.
            if (saved == false)
            {

                System.Windows.Forms.DialogResult result = System.Windows.Forms.MessageBox.Show("Do you want to save changes?", "Save Changes", System.Windows.Forms.MessageBoxButtons.YesNoCancel, System.Windows.Forms.MessageBoxIcon.Question);
                if (result == System.Windows.Forms.DialogResult.Yes)//save
                {
                    fileSave(false);
                    return;
                }
                if (result == System.Windows.Forms.DialogResult.Cancel)//save
                {
                    return;
                }


            }
            Application.Current.Shutdown();
            */


            // MessageBox.Show("the rain in spain");//Application.Exit();
            //MenuEditor editor = new MenuEditor();
            //editor.ElementLocation = myCanvas.MenuLocation;
            //editor.ShowDialog();
            //if (editor.DialogResult.HasValue && editor.DialogResult.Value)
            //{
            //    string str = editor.ElementLocation;
            //    myCanvas.MenuLocation = str;
            //}
        }

        void locationBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            MenuEditor editor = new MenuEditor();
            editor.ElementLocation = myCanvas.MenuLocation;
            editor.ShowDialog();
            if (editor.DialogResult.HasValue && editor.DialogResult.Value)
            {
                string str = editor.ElementLocation;
                myCanvas.MenuLocation = str;
            }
        }



        void previewBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            //Added by Aaron 05/06/2015
            //Lets close the original canvas
            //After preview, we will open it
            // bool isCanvasSaved;
            // isCanvasSaved = checkIfCanvasIsSaved();
            // if (isCanvasSaved == true) closeOpenCanvas();
            string message = "";
            if (gridLines == true)
            {
                message = "You need to turn off the grid lines before saving. Click on Dialog->Remove Grid Lines.";
                MessageBox.Show(message);
                return ;
            }


#region Closing the canvas
            string tempStorage = nameOfFile;

            if (saved == false)
            {
                MessageBox.Show("You must save your changes before previewing the dialog. Please select File ->Save or File->Save As from the menu.");
                return;

            }
            Closebinding_Executed(sender, e);

#endregion

  #region Previewing the saved dialog
            nameOfFile = tempStorage;
            //  FileStream stream = File.Open(nameOfFile), FileMode.Open);
            justTheName = nameOfFile;

            object obj = null;
            string xamlfile = string.Empty, outputfile = string.Empty;
            cf.SaveToLocation(justTheName, Path.GetTempPath(), out xamlfile, out outputfile);
            if (!string.IsNullOrEmpty(xamlfile))
            {
                FileStream stream = File.Open(Path.Combine(Path.GetTempPath(), Path.GetFileName(xamlfile)), FileMode.Open);
                try
                {
                    //Added by Aaron 11/24/2013
                    //Added line below to enable checking of properties in behaviour.cs i.e. propertyname and control name in dialog 
                    //editor mode to make sure that the property names are generated correctly
                    //The line below is important as when the dialog editor is launched for some reason and I DONT know why
                    //BSKYCanvas is invoked from the controlfactor and hence dialogMode is true, we need to reset
                    BSkyCanvas.dialogMode = false;
                    BSkyCanvas.applyBehaviors = false;
                    obj = XamlReader.Load(stream);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }

                BSkyCanvas.first = obj as BSkyCanvas;
                BSkyCanvas canvasobj = obj as BSkyCanvas;
                var converter = new System.Windows.Media.BrushConverter();
                //base.Background = (Brush)converter.ConvertFromString("#FFEDefFf");
                canvasobj.Background = (Brush)converter.ConvertFrom("#FFEEefFf");
                BSkyCanvas.applyBehaviors = true;
                BSkyCanvas.previewinspectMode = true;

                //Added by Aaron 12/08/2013
                //if (canvasobj.Command == true) c1ToolbarStrip1.IsEnabled = false;
                //else c1ToolbarStrip1.IsEnabled = true;

                //Added by Aaron 11/21/2013
                //Added line below to enable checking of properties in behaviour.cs i.e. propertyname and control name in dialog 
                //editor mode to make sure that the property names are generated correctly
                //  BSkyCanvas.dialogMode = true;


                //Added 04/07/2013
                //The properties like outputdefinition are not set for firstCanvas 
                //The line below sets it
                // firstCanvas = canvasobj;
                foreach (UIElement child in canvasobj.Children)
                {
                    // Code below has to be written as we have saved BSKYvariable list with rendervars=False.
                    //BSKyvariablelist has already been created with the default constructore and we need to 
                    //do some work to point the itemsource properties to the dummy variables we create
                    if (child.GetType().Name == "BSkySourceList" || child.GetType().Name == "BSkyTargetList")
                    {

                        List<DataSourceVariable> preview = new List<DataSourceVariable>();
                        DataSourceVariable var1 = new DataSourceVariable();
                        var1.Name = "var1";

                        //02Aug2016 We made change to DataSource so RName is not initialised by Name prop any more. So do it like this.
                        //Old dialogs will not work, that is, variable name var1 var2 will not be show. However in application they will work.
                        var1.RName = "var1";
                        
                        var1.DataType = DataColumnTypeEnum.Numeric;
                        var1.Width = 4;
                        var1.Decimals = 0;
                        var1.Label = "var1";
                        var1.Alignment = DataColumnAlignmentEnum.Left;
                        var1.Measure = DataColumnMeasureEnum.Scale;
                        var1.ImgURL = "../Resources/scale.png";
                        //  var1.ImgURL = "C:/Users/Aiden/Downloads/Client/libs/BSky.Controls/Resources/scale.png";

                        DataSourceVariable var2 = new DataSourceVariable();
                        var2.Name = "var2";

                        //02Aug2016 We made change to DataSource so RName is not initialised by Name prop any more. So do it like this.
                        //Old dialogs will not work, that is, variable name var1 var2 will not be show. However in application they will work.
                        var2.RName = "var2";

                        var2.DataType = DataColumnTypeEnum.Character;
                        var2.Width = 4;
                        var2.Decimals = 0;
                        var2.Label = "var2";
                        var2.Alignment = DataColumnAlignmentEnum.Left;
                        var2.Measure = DataColumnMeasureEnum.Nominal; ;
                        var2.ImgURL = "../Resources/nominal.png";

                        DataSourceVariable var3 = new DataSourceVariable();
                        var3.Name = "var3";

                        //02Aug2016 We made change to DataSource so RName is not initialised by Name prop any more. So do it like this.
                        //Old dialogs will not work, that is, variable name var1 var2 will not be show. However in application they will work.
                        var3.RName = "var3";

                        var3.DataType = DataColumnTypeEnum.Character;
                        var3.Width = 4;
                        var3.Decimals = 0;
                        var3.Label = "var3";
                        var3.Alignment = DataColumnAlignmentEnum.Left;
                        var3.Measure = DataColumnMeasureEnum.Ordinal;
                        var3.ImgURL = "../Resources/ordinal.png";


                        //05Feb2017

                            DataSourceVariable var4 = new DataSourceVariable();
                            var4.Name = "var4";

                            //02Aug2016 We made change to DataSource so RName is not initialised by Name prop any more. So do it like this.
                            //Old dialogs will not work, that is, variable name var1 var2 will not be show. However in application they will work.
                            var4.RName = "var4";

                            var4.DataType = DataColumnTypeEnum.Character;
                            var4.Width = 4;
                            var4.Decimals = 0;
                            var4.Label = "var4";
                            var4.Alignment = DataColumnAlignmentEnum.Left;
                            var4.Measure = DataColumnMeasureEnum.String;
                            var4.ImgURL = "../Resources/String.png";
                           


                        //08Feb2017

                            DataSourceVariable var5 = new DataSourceVariable();
                            var5.Name = "var5";

                            //02Aug2016 We made change to DataSource so RName is not initialised by Name prop any more. So do it like this.
                            //Old dialogs will not work, that is, variable name var1 var2 will not be show. However in application they will work.
                            var5.RName = "var5";

                            var5.DataType = DataColumnTypeEnum.Date;
                            var5.Width = 4;
                            var5.Decimals = 0;
                            var5.Label = "var5";
                            var5.Alignment = DataColumnAlignmentEnum.Left;
                            var5.Measure = DataColumnMeasureEnum.Date;
                            var5.ImgURL = "../Resources/Date.png";



                        //08Feb2017

                            DataSourceVariable var6 = new DataSourceVariable();
                            var6.Name = "var6";

                            //02Aug2016 We made change to DataSource so RName is not initialised by Name prop any more. So do it like this.
                            //Old dialogs will not work, that is, variable name var1 var2 will not be show. However in application they will work.
                            var6.RName = "var6";

                            var6.DataType = DataColumnTypeEnum.Logical;
                            var6.Width = 4;
                            var6.Decimals = 0;
                            var6.Label = "var6";
                            var6.Alignment = DataColumnAlignmentEnum.Left;
                            var6.Measure = DataColumnMeasureEnum.Logical;
                            var6.ImgURL = "../Resources/Logical.png";




                        preview.Add(var1);
                        preview.Add(var2);
                        preview.Add(var3);

                        preview.Add(var4);
                        preview.Add(var5);
                        preview.Add(var6);

                        if (child.GetType().Name == "BSkySourceList")
                        {
                            BSkySourceList temp;
                            temp = child as BSkySourceList;
                            temp.renderVars = false;
                            temp.ItemsSource = preview;
                        }
                        else
                        {
                            BSkyTargetList temp;
                            temp = child as BSkyTargetList;
                            temp.renderVars = false;
                            //temp.ItemsSource = preview;
                        }



                        //BSkyVariableList temp;
                        //temp = child as BSkyVariableList;
                        //// 12/25/2012 
                        ////renderVars =TRUE meanswe will render var1, var2 and var3 as listed above. This means that we are in the Dialog designer
                        //temp.renderVars = false;
                        //temp.ItemsSource = preview;


                    }

                    if (child.GetType().Name == "BSkyListBoxwBorderForDatasets")
                    {
                        BSkyListBoxwBorderForDatasets listfordatasets = child as BSkyListBoxwBorderForDatasets;
                        if (listfordatasets.AutoPopulate == true)
                        {
                            List<DatasetDisplay> listOfDisplayStrings = new List<DatasetDisplay>();

                            DatasetDisplay temp = new DatasetDisplay();
                            temp.Name = "Dataset1";
                            temp.ImgURL = "../Resources/ordinal.png";
                            listOfDisplayStrings.Add(temp);

                            DatasetDisplay temp1 = new DatasetDisplay();
                            temp1.Name = "Dataset2";
                            temp1.ImgURL = "../Resources/ordinal.png";
                            listOfDisplayStrings.Add(temp1);


                            listfordatasets.ItemsSource = listOfDisplayStrings;

                        }

                        //temp.DialogEditorMode = true;

                        //else
                        //{
                        //    BSkyTargetList temp;
                        //    temp = child as BSkyTargetList;
                        //    temp.renderVars = false;
                        //    temp.ItemsSource = preview;
                        //}

                    }

                    //Added by Aaron 04/06/2014
                    //Code below ensures that when the dialog is opened in dialog editor mode, the IsEnabled from
                    //the base class is set to true. This ensures that the control can never be disabled in dialog editor mode
                    //Once the dialog is saved, the Enabled property is saved to the base ISEnabled property to make sure that
                    //the proper state of the dialog is saved
                    //if (child is IBSkyEnabledControl)
                    //{
                    //    IBSkyEnabledControl bskyCtrl = child as IBSkyEnabledControl;
                    //    bskyCtrl.Enabled = bskyCtrl.IsEnabled;
                    //    bskyCtrl.IsEnabled = true;
                    //}
                }

                //Changed by Aaron on 06/02/2015
                //I removed the Cancel button on sub dialogs. I did this because the cancel button allowed you to save
                //the settings on the sub dialog even though the OK button was disabled as some rule criteria was not met.
                //This could generate a wierd error in output and confusion for the user as incorrect syntax was generated.
                //Now the preview was using the sub dialog. I need the cancel on the preview so I had to create a new main window for preview namely MainDialogForPreviewWindow.xaml

                MainDialogForPreviewWindow window = new MainDialogForPreviewWindow();
                window.Template = obj as FrameworkElement;
                // Aaron
                //03/05/2012
                //Making the preview window a fixed size window
                if (obj.GetType().Name == "BSkyCanvas") window.ResizeMode = ResizeMode.NoResize;
                window.ShowDialog();
                //Resetting dialog mode to true to allow for checking for valid property names and control names
                //when setting up setters
                //Added by Aaron 06/14/2014
                //Commented line below
                //restorelistofOpenCanvases(); 
                //window.DetachCanvas();
                window.Template = null;
                BSkyCanvas.dialogMode = true;
                BSkyCanvas.previewinspectMode = false;
                BSkyCanvas.applyBehaviors = false;


            }

  #endregion

            #region OPening the original dialog to preserve state before the preview
            OpenDialogFile(nameOfFile);

            #endregion

        }   

             
        

        void previewBinding_Executedold(object sender, ExecutedRoutedEventArgs e)
        {
            
            


            string filename = string.Empty;
            if (checkDialog() == false) return;
            //Aaron
            //I use the SubDialog as the syntax needs to be disabled in subdialog mode
            SubDialogWindow window = new SubDialogWindow();
            //Gencopy gets bskycanvas.dialog mode to true and then sets it to false
            //After gencopy runs, bskycanvas.dialogmode is false
            gencopy();


            //Aaron 11/25/2012, the line below needs to be uncommented once the serialization issue is addressed with Anil
            // string xaml = GetXamlforpreview();
            string xaml = GetXaml();
            if (xaml == string.Empty)
            {
                MessageBox.Show("Could not create template from current object");
                return;
            }
            object obj = null;
            // bskycanvas.dialogmode is false at this point
            MemoryStream ms = new MemoryStream(Encoding.ASCII.GetBytes(xaml));
            try
            {
                //Added by Aaron 06/14/2014
                //Commented the line below
                // savelistofOpenCanvases();
                obj = System.Windows.Markup.XamlReader.Load(ms);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not create object");
                return;
            }
            //Added by Aaron 11/21/2013
            //Added line below to disable checking of properties in behaviour.cs i.e. propertyname and control name in execute
            // mode to make sure that the property names are generated correctly
            BSkyCanvas.dialogMode = false;
            //if (BSkyCanvas.chainOpenCanvas.Count > 1)
            //    BSkyCanvas.chainOpenCanvas.RemoveAt(0);
            // 11/18/2012, code inserted by Aaron to display the predetermined variable list
            BSkyCanvas canvasobj;
            canvasobj = obj as BSkyCanvas;
            var converter = new System.Windows.Media.BrushConverter();
            //base.Background = (Brush)converter.ConvertFromString("#FFEDefFf");
            canvasobj.Background = (Brush)converter.ConvertFrom("#FFEEefFf");
            // BSkyCanvas.chainOpenCanvas.Add(canvasobj);
            BSkyCanvas.first = canvasobj;
            BSkyCanvas.applyBehaviors = true;
            BSkyCanvas.previewinspectMode = true;

            //Added by Aaron 12/08/2013
            //if (canvasobj.Command == true) c1ToolbarStrip1.IsEnabled = false;
            //else c1ToolbarStrip1.IsEnabled = true;

            //Added by Aaron 11/21/2013
            //Added line below to enable checking of properties in behaviour.cs i.e. propertyname and control name in dialog 
            //editor mode to make sure that the property names are generated correctly
            //  BSkyCanvas.dialogMode = true;


            //Added 04/07/2013
            //The properties like outputdefinition are not set for firstCanvas 
            //The line below sets it
            // firstCanvas = canvasobj;
            foreach (UIElement child in canvasobj.Children)
            {
                // Code below has to be written as we have saved BSKYvariable list with rendervars=False.
                //BSKyvariablelist has already been created with the default constructore and we need to 
                //do some work to point the itemsource properties to the dummy variables we create
                if (child.GetType().Name == "BSkySourceList" || child.GetType().Name == "BSkyTargetList")
                {

                    List<DataSourceVariable> preview = new List<DataSourceVariable>();
                    DataSourceVariable var1 = new DataSourceVariable();
                    var1.Name = "var1";
                    var1.DataType = DataColumnTypeEnum.Numeric;
                    var1.Width = 4;
                    var1.Decimals = 0;
                    var1.Label = "var1";
                    var1.Alignment = DataColumnAlignmentEnum.Left;
                    var1.Measure = DataColumnMeasureEnum.Scale;
                    var1.ImgURL = "../Resources/scale.png";
                    //  var1.ImgURL = "C:/Users/Aiden/Downloads/Client/libs/BSky.Controls/Resources/scale.png";

                    DataSourceVariable var2 = new DataSourceVariable();
                    var2.Name = "var2";
                    var2.DataType = DataColumnTypeEnum.Character;
                    var2.Width = 4;
                    var2.Decimals = 0;
                    var2.Label = "var2";
                    var2.Alignment = DataColumnAlignmentEnum.Left;
                    var2.Measure = DataColumnMeasureEnum.Nominal; ;
                    var2.ImgURL = "../Resources/nominal.png";

                    DataSourceVariable var3 = new DataSourceVariable();
                    var3.Name = "var3";
                    var3.DataType = DataColumnTypeEnum.Character;
                    var3.Width = 4;
                    var3.Decimals = 0;
                    var3.Label = "var3";
                    var3.Alignment = DataColumnAlignmentEnum.Left;
                    var3.Measure = DataColumnMeasureEnum.Ordinal;
                    var3.ImgURL = "../Resources/ordinal.png";



                    //05Feb2017

                    DataSourceVariable var4 = new DataSourceVariable();
                    var4.Name = "var4";

                    //02Aug2016 We made change to DataSource so RName is not initialised by Name prop any more. So do it like this.
                    //Old dialogs will not work, that is, variable name var1 var2 will not be show. However in application they will work.
                    var4.RName = "var4";

                    var4.DataType = DataColumnTypeEnum.Character;
                    var4.Width = 4;
                    var4.Decimals = 0;
                    var4.Label = "var4";
                    var4.Alignment = DataColumnAlignmentEnum.Left;
                    var4.Measure = DataColumnMeasureEnum.String;
                    var4.ImgURL = "../Resources/String.png";



                    //08Feb2017

                    DataSourceVariable var5 = new DataSourceVariable();
                    var5.Name = "var5";

                    //02Aug2016 We made change to DataSource so RName is not initialised by Name prop any more. So do it like this.
                    //Old dialogs will not work, that is, variable name var1 var2 will not be show. However in application they will work.
                    var5.RName = "var5";

                    var5.DataType = DataColumnTypeEnum.Date;
                    var5.Width = 4;
                    var5.Decimals = 0;
                    var5.Label = "var5";
                    var5.Alignment = DataColumnAlignmentEnum.Left;
                    var5.Measure = DataColumnMeasureEnum.Date;
                    var5.ImgURL = "../Resources/Date.png";



                    //08Feb2017

                    DataSourceVariable var6 = new DataSourceVariable();
                    var6.Name = "var6";

                    //02Aug2016 We made change to DataSource so RName is not initialised by Name prop any more. So do it like this.
                    //Old dialogs will not work, that is, variable name var1 var2 will not be show. However in application they will work.
                    var6.RName = "var6";

                    var6.DataType = DataColumnTypeEnum.Logical;
                    var6.Width = 4;
                    var6.Decimals = 0;
                    var6.Label = "var6";
                    var6.Alignment = DataColumnAlignmentEnum.Left;
                    var6.Measure = DataColumnMeasureEnum.Logical;
                    var6.ImgURL = "../Resources/Logical.png";




                    preview.Add(var1);
                    preview.Add(var2);
                    preview.Add(var3);

                    preview.Add(var4);
                    preview.Add(var5);
                    preview.Add(var6);

                    if (child.GetType().Name == "BSkySourceList")
                    {
                        BSkySourceList temp;
                        temp = child as BSkySourceList;
                        temp.renderVars = false;
                        temp.ItemsSource = preview;
                    }
                    else
                    {
                        BSkyTargetList temp;
                        temp = child as BSkyTargetList;
                        temp.renderVars = false;
                        //temp.ItemsSource = preview;
                    }



                    //BSkyVariableList temp;
                    //temp = child as BSkyVariableList;
                    //// 12/25/2012 
                    ////renderVars =TRUE meanswe will render var1, var2 and var3 as listed above. This means that we are in the Dialog designer
                    //temp.renderVars = false;
                    //temp.ItemsSource = preview;


                }

                if (child.GetType().Name == "BSkyListBoxwBorderForDatasets")
                {
                    BSkyListBoxwBorderForDatasets listfordatasets = child as BSkyListBoxwBorderForDatasets;
                    if (listfordatasets.AutoPopulate == true)
                    {
                        List<DatasetDisplay> listOfDisplayStrings = new List<DatasetDisplay>();

                        DatasetDisplay temp = new DatasetDisplay();
                        temp.Name = "Dataset1";
                        temp.ImgURL = "../Resources/ordinal.png";
                        listOfDisplayStrings.Add(temp);

                        DatasetDisplay temp1 = new DatasetDisplay();
                        temp1.Name = "Dataset2";
                        temp1.ImgURL = "../Resources/ordinal.png";
                        listOfDisplayStrings.Add(temp1);


                        listfordatasets.ItemsSource = listOfDisplayStrings;

                    }

                    //temp.DialogEditorMode = true;
                   
                    //else
                    //{
                    //    BSkyTargetList temp;
                    //    temp = child as BSkyTargetList;
                    //    temp.renderVars = false;
                    //    temp.ItemsSource = preview;
                    //}

                }

                //Added by Aaron 04/06/2014
                //Code below ensures that when the dialog is opened in dialog editor mode, the IsEnabled from
                //the base class is set to true. This ensures that the control can never be disabled in dialog editor mode
                //Once the dialog is saved, the Enabled property is saved to the base ISEnabled property to make sure that
                //the proper state of the dialog is saved
                //if (child is IBSkyEnabledControl)
                //{
                //    IBSkyEnabledControl bskyCtrl = child as IBSkyEnabledControl;
                //    bskyCtrl.Enabled = bskyCtrl.IsEnabled;
                //    bskyCtrl.IsEnabled = true;
                //}
            }


            window.Template = obj as FrameworkElement;
            // Aaron
            //03/05/2012
            //Making the preview window a fixed size window
            if (obj.GetType().Name == "BSkyCanvas") window.ResizeMode = ResizeMode.NoResize;
            window.ShowDialog();
            //Resetting dialog mode to true to allow for checking for valid property names and control names
            //when setting up setters
            //Added by Aaron 06/14/2014
            //Commented line below
            //restorelistofOpenCanvases(); 
            //window.DetachCanvas();
            window.Template = null;
            BSkyCanvas.dialogMode = true;
            BSkyCanvas.previewinspectMode = false;
            BSkyCanvas.applyBehaviors = false;
        }


        //Aaron added 12/08/2013
        //The code below creates a command. There is no dialog displayed when you run a command
        //You cannot add items to the canvas
        void commandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            //RCommandDialog dlg = new RCommandDialog();
            //dlg.CommandString = myCanvas.CommandString;
            ////dlg.ShowDialog();
            ////if (dlg.DialogResult.HasValue && dlg.DialogResult.Value)
            ////{
            ////    myCanvas.CommandString = dlg.CommandString;
            ////}
            //myCanvas.CommandString = dlg.GetCommand(myCanvas);
            System.Windows.Forms.DialogResult result;
            //tHIS is the case when I call new and there is no open canvas i.e. I have just called close.
            if (CanClose == false)
            {
                BSkyCanvas obj = BSkyControlFactory.Instance.CreateControl("Canvas") as BSkyCanvas;
                obj.commandOnly = true;
                initialPosition = 10;
                mainDialog = true;
                obj.Background = new SolidColorBrush(Colors.DarkGray);
                CanSave = true;
                CanOpen = true;
                CanSaveSubDialog = false;

                //Canvashost is the border around the canvas area in the XAML
                CanvasHost.Child = obj;
                myCanvas = obj;
                obj.Command = true;
                //Added by Aaron 11/21/2013
                //This is to enable the checking of properties as defined in behavior.cs

                //BSkyCanvas.dialogMode = true;
                //Aaron: cleanup
                myCanvas.KeyDown += new KeyEventHandler(b_KeyDown);
                firstCanvas = obj;

                myCanvas.Focusable = true;
                CanvasPropertyGrid.SelectedObject = myCanvas;
                myCanvas.KeyUp += new KeyEventHandler(myCanvas_KeyUp);
                myCanvas.PreviewKeyUp += new KeyEventHandler(myCanvas_PreviewKeyUp);
                myCanvas.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(myCanvas_PreviewMouseLeftButtonDown);
                myCanvas.PreviewMouseLeftButtonUp += new MouseButtonEventHandler(DragFinishedMouseHandler);
                c1ToolbarStrip1.IsEnabled = true;
                CanClose = true;
                nameOfFile = "Command1";
                Title = "Command1";
                myCanvas.Title = "";
                c1ToolbarStrip1.IsEnabled = false;
                return;
            }//myCanvas.Focus();
            //Aaron
            //Added 02/10/2013
            //This ensures that the close command is invoked which handles whether the existing dialog definition should be saved 
            //I commented the line below as I would have liked ApplicationCommands.Close.Execute
            //to return a value 0,1,2 to indicate yes, no, cancel. If yes, I would not open a new browse window to select the file
            //  if (saved == false) ApplicationCommands.Close.Execute(null, this);
            //Aaron added 06/16/2013
            //Code below closes the existing open and saved canvas before opening the new one

            //Case of clicking on command when there is a saved dialog open

            if (saved == true)
            {
                int count = myCanvas.Children.Count;
                initialPosition = 10;
                myCanvas.Children.RemoveRange(0, count);
                myCanvas.Command = true;
                myCanvas.Background = new SolidColorBrush(Colors.DarkGray);
                c1ToolbarStrip1.IsEnabled = false;
                //If this is the subdialog, we return to the main dialog
                if (mainDialog == false)
                {
                    this.DialogResult = false;
                    //this.Close();
                }
                // selectedElement = null;
                //We reset the main dialog
                //Resetting the name of the file
                nameOfFile = string.Empty;
                OptionsPropertyGrid.SelectedObject = null;
                myCanvas.Width = 470;
                myCanvas.Height = 300;
                Title = nameOfFile = "Command1";
                myCanvas.Title = "";
                myCanvas.CommandString = "";
                myCanvas.OutputDefinition = "";

                //myCanvas.Children.Clear();

                CanvasPropertyGrid.SelectedObject = myCanvas;
                //OptionsPropertyGrid.SelectedObject = myCanvas;
                //Added by Aaron
                //02/10/2013
                //Setting saved =true so that when I open another dialog, you are not prompted to save
                saved = true;
                selectedElement = null;
                //Aaron 10/29/2013
                //added line below
                selectedElementRef = null;


            }

            //Calling new when there is an open dialog that I don't want to save
            if (saved == false)
            {

                result = System.Windows.Forms.MessageBox.Show("Do you want to save changes?", "Save Changes", System.Windows.Forms.MessageBoxButtons.YesNoCancel, System.Windows.Forms.MessageBoxIcon.Question);
                if (result == System.Windows.Forms.DialogResult.Yes)//save
                {
                    fileSave(false);
                    CanClose = true;
                    return;
                }
                if (result == System.Windows.Forms.DialogResult.Cancel)//save
                {
                    return;
                }

                if (result == System.Windows.Forms.DialogResult.No)//save
                {

                    int count = myCanvas.Children.Count;
                    myCanvas.Children.RemoveRange(0, count);
                    initialPosition = 10;
                    myCanvas.Command = true;
                    myCanvas.Background = new SolidColorBrush(Colors.DarkGray);
                    c1ToolbarStrip1.IsEnabled = false;
                    //If this is the subdialog, we return to the main dialog
                    if (mainDialog == false)
                    {
                        this.DialogResult = false;
                        //this.Close();
                    }
                    // selectedElement = null;
                    //We reset the main dialog
                    //Resetting the name of the file
                    nameOfFile = string.Empty;
                    OptionsPropertyGrid.SelectedObject = null;
                    myCanvas.Width = 470;
                    myCanvas.Height = 300;
                    Title = nameOfFile = "Command1";
                    myCanvas.Title = "";
                    myCanvas.CommandString = "";
                    myCanvas.OutputDefinition = "";

                    //myCanvas.Children.Clear();

                    CanvasPropertyGrid.SelectedObject = myCanvas;
                    //OptionsPropertyGrid.SelectedObject = myCanvas;
                    //Added by Aaron
                    //02/10/2013
                    //Setting saved =true so that when I open another dialog, you are not prompted to save
                    saved = true;
                    selectedElement = null;
                    //Aaron 10/29/2013
                    //added line below
                    selectedElementRef = null;

                }

            }

        }

        // This is the default constructor






        void designDialog_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            //Window1 w = new Window1();
            //w.CanOpen = false;
            //w.CanSave = false;
            //if (selectedElement.Resources.Count > 0)
            //{
            //    w.OpenCanvas(selectedElement.Resources["dlg"]);
            //}
            //w.ShowDialog();
            //if (w.DialogResult.HasValue && w.DialogResult.Value)
            //{
            //    selectedElement.Resources.Remove("dlg");
            //    selectedElement.Resources.Add("dlg", w.DialogElement);
            //}
        }

        //Lets me know if the dialg can be saved. Cansave is disabled on for subdialogs
        //When I have closed a dialog, save should be disabled
        public bool CanSave { get; set; }

        //Determines when I can open a dialog. When I am in a sub dialog, I cannot open it
        public bool CanOpen { get; set; }

        //When I am in a sub dialog, I cannot close it, I can only save and close
        public bool CanClose { get; set; }

        //Lets me know when I can save a SUB DIALOG
        public bool CanSaveSubDialog { get; set; }

        public object DialogElement { get; set; }

        void OpenBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = CanOpen;
        }


        void Closebinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            //Close is only enabled on the main dialog. 
            if (mainDialog == true)
            {
                if (CanClose == true)
                    e.CanExecute = true;
            }
            else
                e.CanExecute = false;

        }


        void propertiesBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            DialogProperties props = new DialogProperties();
            props.Width = myCanvas.Width;
            props.Height = myCanvas.Height;
            props.Title = myCanvas.Title;
            props.ShowDialog();
            if (props.DialogResult.HasValue && props.DialogResult.Value)
            {
                myCanvas.Width = props.Width;
                myCanvas.Height = props.Height;
                myCanvas.Title = props.Title;
            }
        }

        public void OpenCanvas(object obj)
        {
            if (obj != null)
            {
                CanvasHost.Child = obj as UIElement;
                myCanvas = obj as BSkyCanvas;
                myCanvas.Focusable = true;

                //Aaron:Cleanup
                myCanvas.KeyUp += new KeyEventHandler(myCanvas_KeyUp);
                myCanvas.PreviewKeyUp += new KeyEventHandler(myCanvas_PreviewKeyUp);
                myCanvas.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(myCanvas_PreviewMouseLeftButtonDown);
                myCanvas.PreviewMouseLeftButtonUp += new MouseButtonEventHandler(DragFinishedMouseHandler);

                CanvasPropertyGrid.SelectedObject = myCanvas;

            }
        }

        /// <summary>
        /// Detaches the object from current window
        /// </summary>
        /// <param name="obj"></param>
        public void DetachCanvas()
        {
            // int count1 = myCanvas.Children.Count;
            // myCanvas.Children.RemoveRange(0, count1);
            CanvasHost.Child = null;
            myCanvas.KeyUp -= myCanvas_KeyUp;
            myCanvas.PreviewKeyUp -= myCanvas_PreviewKeyUp;
            myCanvas.PreviewMouseLeftButtonDown -= myCanvas_PreviewMouseLeftButtonDown;
            myCanvas.PreviewMouseLeftButtonUp -= DragFinishedMouseHandler;
           // myCanvas.Children.Clear();
            //myCanvas = null;
            //04/20/2013
            //Aaron
            //The code below detaches the canvas from the window of the subdialog on closing the sub dialog window
            //If this code is not run, we get a defect on opening the sub dialog (need to verify) saying the canvas is already a child of a window
            this.RemoveVisualChild(myCanvas);
            //Canvas = null;
        }


        // 03/23/2013 Added by Aaron
        // Returns the number of sub dialogs present in a main dialog
        private int GetCanvasObjectCount(BSkyCanvas canvas)
        {
            int canvasCount = 1;
            foreach (FrameworkElement fe in canvas.Children)
            {
                if (fe is BSkyButton)
                {
                    BSkyButton btn = fe as BSkyButton;
                    if (btn.Resources.Count > 0)
                        canvasCount++;
                }
            }
            return canvasCount;
        }

        void OpenBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            OpenDialogFile();//not passing filename, so file open dialog will be shown
        }

        //Added by Anil. Common Function to open dialog file through File > Open  --OR--  File > Recent
        void OpenDialogFile(string fullpathfilename=null)
        {

            //Added by Aaron 02/10/2013
            //Code added to prompt user to save when there was an existing dialog that was unsaved already opened
            System.Windows.Forms.DialogResult result;

            //Aaron
            //Added 02/10/2013
            //This ensures that the close command is invoked which handles whether the existing dialog definition should be saved 
            //I commented the line below as I would have liked ApplicationCommands.Close.Execute
            //to return a value 0,1,2 to indicate yes, no, cancel. If yes, I would not open a new browse window to select the file
            //  if (saved == false) ApplicationCommands.Close.Execute(null, this);
            //Aaron added 06/16/2013
            //Code below closes the existing open and saved canvas before opening the new one
            #region 
            if (saved == true)
            
            {
                if (myCanvas != null)
                {
                    int count = myCanvas.Children.Count;
                    myCanvas.Children.RemoveRange(0, count);

                    //Added by Aaron 05/12/2014
                    BSkyCanvas.chainOpenCanvas.Clear();
                    initialPosition = 10;
                    //If this is the subdialog, we return to the main dialog
                    if (mainDialog == false)
                    {
                        this.DialogResult = false;
                        //this.Close();
                    }
                    // selectedElement = null;
                    //We reset the main dialog
                    //Resetting the name of the file
                    nameOfFile = string.Empty;
                    OptionsPropertyGrid.SelectedObject = null;
                    myCanvas.Width = 470;
                    myCanvas.Height = 300;
                    Title = nameOfFile = "Dialog1";
                    myCanvas.Title = "";
                    myCanvas.CommandString = "";
                    myCanvas.OutputDefinition = "";

                    //myCanvas.Children.Clear();

                    CanvasPropertyGrid.SelectedObject = myCanvas;
                    //OptionsPropertyGrid.SelectedObject = myCanvas;
                    //Added by Aaron
                    //02/10/2013
                    //Setting saved =true so that when I open another dialog, you are not prompted to save
                    saved = true;
                    selectedElement = null;
                    //Aaron 10/29/2013
                    //added line below
                    selectedElementRef = null;
                }

            }
            #endregion
            if (saved == false)
            {

                result = System.Windows.Forms.MessageBox.Show("Do you want to save changes?", "Save Changes", System.Windows.Forms.MessageBoxButtons.YesNoCancel, System.Windows.Forms.MessageBoxIcon.Question);
                if (result == System.Windows.Forms.DialogResult.Yes)//save
                {
                    fileSave(false);
                    CanClose = true;

                    return;
                }
                if (result == System.Windows.Forms.DialogResult.Cancel)//save
                {
                    return;
                }

                if (result == System.Windows.Forms.DialogResult.No)//save
                {

                    int count = myCanvas.Children.Count;
                    myCanvas.Children.RemoveRange(0, count);
                    //Added by Aaron 05/12/2014
                    BSkyCanvas.chainOpenCanvas.Clear();

                    initialPosition = 10;
                    //If this is the subdialog, we return to the main dialog
                    if (mainDialog == false)
                    {
                        this.DialogResult = false;
                        //this.Close();
                    }
                    // selectedElement = null;
                    //We reset the main dialog
                    //Resetting the name of the file
                    nameOfFile = string.Empty;
                    OptionsPropertyGrid.SelectedObject = null;
                    myCanvas.Width = 470;
                    myCanvas.Height = 300;
                    Title = nameOfFile = "Dialog1";
                    myCanvas.Title = "";
                    myCanvas.CommandString = "";
                    myCanvas.OutputDefinition = "";

                    //myCanvas.Children.Clear();

                    CanvasPropertyGrid.SelectedObject = myCanvas;
                    //OptionsPropertyGrid.SelectedObject = myCanvas;
                    //Added by Aaron
                    //02/10/2013
                    //Setting saved =true so that when I open another dialog, you are not prompted to save
                    saved = true;
                    selectedElement = null;
                    //Aaron 10/29/2013
                    //added line below
                    selectedElementRef = null;

                }

            }

            //XamlDesignerSerializationManager manager= new XamlDesignerSerializationManager(new System.Xml.XmlWriter();
            System.Windows.Forms.OpenFileDialog dialog = new System.Windows.Forms.OpenFileDialog();
            dialog.Filter = "Xaml Document (*.bsky)|*.bsky";
            //16Jul2014 Following 'if' added by Anil. Before, there was only 1 statement that's in else right now.
            if (fullpathfilename != null && fullpathfilename.Trim().Length > 0) //someone passed filename(ie.. clicked file from recent list)
            {
                dialog.FileName = @fullpathfilename;
                result = System.Windows.Forms.DialogResult.OK;
            }
            else
            {
                result = dialog.ShowDialog();//without if-else this was the statement outside if-else
            }

            if (result == System.Windows.Forms.DialogResult.OK)
            {
                string xamlfile = string.Empty, outputfile = string.Empty;

                justTheName =@dialog.FileName;

                cf.SaveToLocation(@dialog.FileName, Path.GetTempPath(), out xamlfile, out outputfile);
                if (!string.IsNullOrEmpty(xamlfile))
                {
                    FileStream stream = File.Open(Path.Combine(Path.GetTempPath(), Path.GetFileName(xamlfile)), FileMode.Open);

                    //FileStream stream = File.Open(Path.Combine(Path.GetTempPath(), xamlfile,FileMode.Open);
                    try
                    {
                        //Added by Aaron 11/24/2013
                        //Added line below to enable checking of properties in behaviour.cs i.e. propertyname and control name in dialog 
                        //editor mode to make sure that the property names are generated correctly
                        //The line below is important as when the dialog editor is launched for some reason and I DONT know why
                        //BSKYCanvas is invoked from the controlfactor and hence dialogMode is true, we need to reset
                        BSkyCanvas.dialogMode = false;
                        BSkyCanvas.applyBehaviors = false;
                        object obj = XamlReader.Load(stream);

                        //Added by Aaron 04/13/2014
                        //There are 3 conditions here, when I launch dialog editor
                        //1. The first thing I do is open an existing dialog. Chainopencanvas.count=0
                        //2. I have an existing dialog open and I click open again.  Chainopencanvas.count=2. The first dialog is empty as I clean the existing canvas and open a new one
                        //3. I hit new the first time I open dialogeditor and then I hit open. Again,Chainopencanvas.count=2. The first dialog is empty as I clean the existing canvas and open a new one 

                        //The chainopencanvas at this point has 2 entries, one with an empty canvas as we have removed all the children
                        //the other with the dialog we just opened
                        //we remove the empty one
                        //Added by Aaron 05/12/2014
                        //Commented the 2 lines below
                        // if (BSkyCanvas.chainOpenCanvas.Count >1)
                        //BSkyCanvas.chainOpenCanvas.RemoveAt(0);


                        // 11/18/2012, code inserted by Aaron to display the predetermined variable list
                        BSkyCanvas canvasobj;
                        canvasobj = obj as BSkyCanvas;
                        //Added by Aaron 05/12/2014
                        BSkyCanvas.chainOpenCanvas.Add(canvasobj);

                        //Added by Aaron 12/08/2013
                        if (canvasobj.Command == true) c1ToolbarStrip1.IsEnabled = false;
                        else c1ToolbarStrip1.IsEnabled = true;

                        //Added by Aaron 11/21/2013
                        //Added line below to enable checking of properties in behaviour.cs i.e. propertyname and control name in dialog 
                        //editor mode to make sure that the property names are generated correctly
                        BSkyCanvas.dialogMode = true;


                        //Added 04/07/2013
                        //The properties like outputdefinition are not set for firstCanvas 
                        //The line below sets it
                        firstCanvas = canvasobj;
                        foreach (UIElement child in canvasobj.Children)
                        {
                            // Code below has to be written as we have saved BSKYvariable list with rendervars=False.
                            //BSKyvariablelist has already been created with the default constructore and we need to 
                            //do some work to point the itemsource properties to the dummy variables we create
                            if (child.GetType().Name == "BSkySourceList" || child.GetType().Name == "BSkyTargetList")
                            {
                                List<DataSourceVariable> preview = new List<DataSourceVariable>();
                                DataSourceVariable var1 = new DataSourceVariable();
                                var1.Name = "var1";

                                //02Aug2016 We made change to DataSource so RName is not initialised by Name prop any more. So do it like this.
                                //Old dialogs will not work, that is, variable name var1 var2 will not be show. However in application they will work.
                                var1.RName = "var1";

                                var1.DataType = DataColumnTypeEnum.Numeric;
                                var1.Width = 4;
                                var1.Decimals = 0;
                                var1.Label = "var1";
                                var1.Alignment = DataColumnAlignmentEnum.Left;
                                var1.Measure = DataColumnMeasureEnum.Scale;
                                var1.ImgURL = "../Resources/scale.png";
                                //  var1.ImgURL = "C:/Users/Aiden/Downloads/Client/libs/BSky.Controls/Resources/scale.png";

                                DataSourceVariable var2 = new DataSourceVariable();
                                var2.Name = "var2";

                                //02Aug2016 We made change to DataSource so RName is not initialised by Name prop any more. So do it like this.
                                //Old dialogs will not work, that is, variable name var1 var2 will not be show. However in application they will work.
                                var2.RName = "var2";

                                var2.DataType = DataColumnTypeEnum.Character;
                                var2.Width = 4;
                                var2.Decimals = 0;
                                var2.Label = "var2";
                                var2.Alignment = DataColumnAlignmentEnum.Left;
                                var2.Measure = DataColumnMeasureEnum.Nominal; ;
                                var2.ImgURL = "../Resources/nominal.png";

                                DataSourceVariable var3 = new DataSourceVariable();
                                var3.Name = "var3";

                                //02Aug2016 We made change to DataSource so RName is not initialised by Name prop any more. So do it like this.
                                //Old dialogs will not work, that is, variable name var1 var2 will not be show. However in application they will work.
                                var3.RName = "var3";

                                var3.DataType = DataColumnTypeEnum.Character;
                                var3.Width = 4;
                                var3.Decimals = 0;
                                var3.Label = "var3";
                                var3.Alignment = DataColumnAlignmentEnum.Left;
                                var3.Measure = DataColumnMeasureEnum.Ordinal;
                                var3.ImgURL = "../Resources/ordinal.png";


                                //05Feb2017

                                DataSourceVariable var4 = new DataSourceVariable();
                                var4.Name = "var4";

                                //02Aug2016 We made change to DataSource so RName is not initialised by Name prop any more. So do it like this.
                                //Old dialogs will not work, that is, variable name var1 var2 will not be show. However in application they will work.
                                var4.RName = "var4";

                                var4.DataType = DataColumnTypeEnum.Character;
                                var4.Width = 4;
                                var4.Decimals = 0;
                                var4.Label = "var4";
                                var4.Alignment = DataColumnAlignmentEnum.Left;
                                var4.Measure = DataColumnMeasureEnum.String;
                                var4.ImgURL = "../Resources/String.png";



                                //08Feb2017

                                DataSourceVariable var5 = new DataSourceVariable();
                                var5.Name = "var5";

                                //02Aug2016 We made change to DataSource so RName is not initialised by Name prop any more. So do it like this.
                                //Old dialogs will not work, that is, variable name var1 var2 will not be show. However in application they will work.
                                var5.RName = "var5";

                                var5.DataType = DataColumnTypeEnum.Date;
                                var5.Width = 4;
                                var5.Decimals = 0;
                                var5.Label = "var5";
                                var5.Alignment = DataColumnAlignmentEnum.Left;
                                var5.Measure = DataColumnMeasureEnum.Date;
                                var5.ImgURL = "../Resources/Date.png";



                                //08Feb2017

                                DataSourceVariable var6 = new DataSourceVariable();
                                var6.Name = "var6";

                                //02Aug2016 We made change to DataSource so RName is not initialised by Name prop any more. So do it like this.
                                //Old dialogs will not work, that is, variable name var1 var2 will not be show. However in application they will work.
                                var6.RName = "var6";

                                var6.DataType = DataColumnTypeEnum.Logical;
                                var6.Width = 4;
                                var6.Decimals = 0;
                                var6.Label = "var6";
                                var6.Alignment = DataColumnAlignmentEnum.Left;
                                var6.Measure = DataColumnMeasureEnum.Logical;
                                var6.ImgURL = "../Resources/Logical.png";




                                preview.Add(var1);
                                preview.Add(var2);
                                preview.Add(var3);

                                preview.Add(var4);
                                preview.Add(var5);
                                preview.Add(var6);

                                if (child.GetType().Name == "BSkySourceList")
                                {
                                    BSkySourceList temp;
                                    temp = child as BSkySourceList;
                                    temp.renderVars = true;
                                    temp.ItemsSource = preview;
                                }
                                else
                                {
                                    BSkyTargetList temp;
                                    temp = child as BSkyTargetList;
                                    temp.renderVars = true;
                                    temp.ItemsSource = preview;
                                }
                                // 12/25/2012 
                                //renderVars =TRUE meanswe will render var1, var2 and var3 as listed above. This means that we are in the Dialog designer



                            }


                            if (child.GetType().Name == "BSkyListBoxwBorderForDatasets" )
                            {
                                BSkyListBoxwBorderForDatasets temp;
                                temp = child as BSkyListBoxwBorderForDatasets;
                                temp.DialogEditorMode=true;
                                // 12/25/2012 
                                //renderVars =TRUE meanswe will render var1, var2 and var3 as listed above. This means that we are in the Dialog designer
                            }

                            
                            //Added by Aaron 04/06/2014
                            //Code below ensures that when the dialog is opened in dialog editor mode, the IsEnabled from
                            //the base class is set to true. This ensures that the control can never be disabled in dialog editor mode
                            //Once the dialog is saved, the Enabled property is saved to the base ISEnabled property to make sure that
                            //the proper state of the dialog is saved
                            if (child is IBSkyEnabledControl)
                            {
                                IBSkyEnabledControl bskyCtrl = child as IBSkyEnabledControl;
                                bskyCtrl.Enabled = bskyCtrl.IsEnabled;
                                bskyCtrl.IsEnabled = true;
                            }
                        }
                        subCanvasCount = GetCanvasObjectCount(obj as BSkyCanvas);
                        BSkyControlFactory.SetCanvasCount(subCanvasCount);
                        OpenCanvas(obj);
                        //Aaron 01/12/2013
                        //This stores the file name so that we don't have to display the file save dialog on file save and we can save to the file directly;
                        nameOfFile = @dialog.FileName;

                        //01/26/2013 Aaron
                        //Setting the title of the window
                        Title = nameOfFile;
                        //Aaron: Commented this on 12/15
                        // myCanvas.OutputDefinition = Path.Combine(Path.GetTempPath(), outputfile);
                        //Aaron 01/26/2013
                        //Making sure saved is set to true as soon as you open the dialog.
                        saved = true;
                        //Aaron
                        //02/10/2013
                        //Added code below to ensure that the RoutedUICommand gets fired correctly and the menu items under file and Dialog (top level menu items)
                        //are properly opened and closed
                        c1ToolbarStrip1.IsEnabled = true;
                        CanClose = true;
                        CanSave = true;
                        recentfiles.AddXMLItem(@dialog.FileName);
                        this.Focus();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                }
                else
                {
                    MessageBox.Show("Cannot open the file");
                }
            }

        }

        // bool saveAs just tells me whether the user has invoked a save or a saveas command
        //With Save As, we always propt the user with the file location
        //With save, if the file location is available, we save the file directly

        private bool SaveXaml(out string fileName, bool saveAs)
        {

            if (!CanSave)
            {
                DialogElement = myCanvas;
                DialogResult = true;
                this.Close();
                fileName = string.Empty;
                return false;
            }
            ////Added by Aaron 11/21/2013
            ////Added the line below to ensure that dialogMode the flag that enables or disables the checking of property and control 
            ////names in behaviour.cs is set to false when saving the dialog. This prevents the property setters from firing
            //BSkyCanvas.dialogMode = false;
            //// The function below makes a copy of the canvas

            //gencopy();


            //bool result = false;
            ////XamlDesignerSerializationManager manager= new XamlDesignerSerializationManager(new System.Xml.XmlWriter();
            //string xaml = GetXaml();
            ////Added by Aaron 11/21/2013
            ////Added the line below to ensure that dialogMode the flag that enables or disables the checking of property and control 
            ////names in behaviour.cs is set to true after saving (the xaml is created)
            //BSkyCanvas.dialogMode = true;



            //Added by Aaron 01/13/2012
            //2 cases, the first the BSKY file exists
            //the second, its a brand new dialog
            bool result = false;
            if (nameOfFile == "Dialog1" || saveAs == true || nameOfFile == "Command1")
            {
                System.Windows.Forms.SaveFileDialog dialog = new System.Windows.Forms.SaveFileDialog();
                dialog.Filter = FileNameFilter;
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    try
                    {

                        //Added by Aaron 11/21/2013
                        //Added the line below to ensure that dialogMode the flag that enables or disables the checking of property and control 
                        //names in behaviour.cs is set to false when saving the dialog. This prevents the property setters from firing
                        //Added by Aaron 04/12/2014
                        // BSkyCanvas.dialogMode = false;

                        fileName = @dialog.FileName;

                        //Added by Aaron 01/23/2013
                        // Added by Aaron 03/31/2013
                        // The internalhelpfilename holds the name of the help file that will be saved to the bin/config directory
                        //In the function  private bool deployXmlXaml(string Commandname, bool overwrite) in file menueditor.xaml.cs, called when we install the dialog, we use this property to name the help files
                        //IN the BSky file, the help files have their original name. This is so that if you unzip the dialog definition, so you see what you expect
                        //We only change the help file names to the dialog name with a suffix of a number (as a single dialog can have multiple help files
                        //If there is a valid help file associated with the canvas, then this function automaticaly associates it with an internalhelpfilename

                        //The parameter 1 starts the suffix which is used to name the help files with 1
                        firstCanvas.processhelpfiles(firstCanvas, Path.GetFileNameWithoutExtension(fileName), 1);
                        // The function below makes a copy of the canvas
                        gencopy();




                        //XamlDesignerSerializationManager manager= new XamlDesignerSerializationManager(new System.Xml.XmlWriter();
                        string xaml = GetXaml();
                        //Added by Aaron 11/21/2013
                        //Added the line below to ensure that dialogMode the flag that enables or disables the checking of property and control 
                        //names in behaviour.cs is set to true after saving (the xaml is created). This is so property names and control 
                        //names are verified in dialog editor mode
                        BSkyCanvas.dialogMode = true;




                        //03/16/2013
                        //The code below ensures that the title of the dialog and the nameOfFile gets set correctly.
                        //This makes sure that if you clicj save again, you are not prompted to select the location
                        //fileName = @dialog.FileName;
                        nameOfFile = fileName;
                        Title = nameOfFile;
                        //fileName = Path.GetFileNameWithoutExtension(fileName);
                        FileStream stream = File.Create(Path.Combine(Path.GetDirectoryName(fileName), Path.GetFileNameWithoutExtension(fileName) + ".xaml"));
                        TextWriter writer = new StreamWriter(stream);
                        writer.Write(xaml);
                        writer.Close();
                        result = true;

                    }
                    catch (Exception ex)
                    {
                        fileName = string.Empty;
                        MessageBox.Show("An error was encountered when creating the XAML file to store the dialog definition");
                    }
                }
                else
                {

                    fileName = string.Empty;
                    return result = false;
                }
            }
            //Aaron 01/12/2013
            //Dialog was created from an already existing BSKY dialog
            else
            {
                //Added by Aaron 11/21/2013
                //Added the line below to ensure that dialogMode the flag that enables or disables the checking of property and control 
                //names in behaviour.cs is set to false when saving the dialog. This prevents the property setters from firing
                BSkyCanvas.dialogMode = false;
                fileName = nameOfFile;

                firstCanvas.processhelpfiles(firstCanvas, Path.GetFileNameWithoutExtension(fileName), 1);
                gencopy();

                //XamlDesignerSerializationManager manager= new XamlDesignerSerializationManager(new System.Xml.XmlWriter();
                string xaml = GetXaml();
                //Added by Aaron 11/21/2013
                //Added the line below to ensure that dialogMode the flag that enables or disables the checking of property and control 
                //names in behaviour.cs is set to true after saving (the xaml is created)
                BSkyCanvas.dialogMode = true;

                FileStream stream = File.Create(Path.Combine(Path.GetDirectoryName(fileName), Path.GetFileNameWithoutExtension(fileName) + ".xaml"));
                TextWriter writer = new StreamWriter(stream);
                writer.Write(xaml);
                writer.Close();
                result = true;
            }
            return result;
        }



        //Generates a copy of myCanvas without the var1, var2 and var3 available in the variable list that displays in preview
        private void gencopy()
        {
            copy = new BSkyCanvas();
            BSkyCanvas.dialogMode = true;
            copy.Title = myCanvas.Title;
            copy.Helpfile = myCanvas.Helpfile;
            copy.HelpText = myCanvas.HelpText;
            copy.RHelpText = myCanvas.RHelpText;
            copy.ModelClasses = myCanvas.ModelClasses;
            copy.PrereqCommandString = myCanvas.PrereqCommandString;//15Sep2016
            copy.StatusTextBoxName = myCanvas.StatusTextBoxName;//15Sep2016 // This will contain the textbox control name in which you want to show a message
            copy.splitProcessing = myCanvas.splitProcessing;
            copy.CommandString = myCanvas.CommandString;
            copy.OutputDefinition = myCanvas.OutputDefinition;
            copy.MenuLocation = myCanvas.MenuLocation;
            copy.Background = myCanvas.Background;
            copy.Height = myCanvas.Height;
            copy.Width = myCanvas.Width;
            copy.RPackages = myCanvas.RPackages;
            copy.Command = myCanvas.Command;
            copy.commandOnly = myCanvas.commandOnly;
            copy.internalhelpfilename = myCanvas.internalhelpfilename;
            //Added by Aaron 10/08/2013
            //The code below was developed about 2 months before 10/08/2013
            //The move variable button requires a source and destination variable list.
            //When copying and creating the move button, the destination variable may not be created yet. We therefore create 
            //all the variables first
            copyVariables(copy);
            copyAllExceptVars(copy);
            //Added by Aaron 4/14/2014
            //Makes sure that dialog is saved to XAML with dialogmode =false
            BSkyCanvas.dialogMode = false;
            //Added by Aaron 11/17/2013
            //New function that sets the SelectionChangeBehavior property after all the objects have been created on the copied canvas
            // copySelectionChangeBehaviour(copy);

        }

        //Aaron 11/24/2013
        //Wrote this function to get the control from the completely finished copied dialog 
        //This was done to get the controls that had a selectionchange behaviour after all controls were created

        //public FrameworkElement getCtrl(string ControlName, BSkyCanvas cs)
        //{

        //    FrameworkElement retval;

        //    foreach (Object obj in cs.Children)
        //    {
        //        if (obj is IBSkyControl)
        //        {
        //            IBSkyControl ib = obj as IBSkyControl;
        //            if (ib.Name == ControlName)
        //            {
        //                return ib as FrameworkElement;
        //            }
        //            //05/18/2013
        //            //Added by Aaron
        //            //Code below checks the radio buttons within each radiogroup looking for duplicate names
        //            if (obj is BSkyRadioGroup)
        //            {
        //                BSkyRadioGroup ic = obj as BSkyRadioGroup;
        //                StackPanel stkpanel = ic.Content as StackPanel;

        //                foreach (object obj1 in stkpanel.Children)
        //                {
        //                    BSkyRadioButton btn = obj1 as BSkyRadioButton;
        //                    if (btn.Name == ControlName)
        //                    {
        //                        return btn as FrameworkElement;
        //                    }
        //                }

        //            }

        //        }
        //        if (obj is BSkyButton)
        //        {
        //            FrameworkElement fe = obj as FrameworkElement;
        //            BSkyCanvas canvas = fe.Resources["dlg"] as BSkyCanvas;
        //            if (cs != null)
        //            {
        //                retval = getCtrl(ControlName, canvas);
        //                if (retval != null) return retval;
        //            }
        //        }
        //    }
        //    return null;

        //}




        //Aaron 11/24/2013
        //Wrote this function to copy the selection behaviours on the variable lists after the entire copy of the dialog is generated
        //This this to prevent the setters of the behaviour collection from firing when the controls were not yet copied to the canvas


        //private void copySelectionChangeBehaviour(BSkyCanvas copy)
        //{
        //    foreach (UIElement child in myCanvas.Children)
        //    {
        //        if (child.GetType().Name == "BSkyVariableList" || child.GetType().Name == "BSkyGroupingVariable")
        //        {

        //            if (child.GetType().Name == "BSkyVariableList")
        //            {
        //              BSkyVariableList child1 = child as BSkyVariableList;
        //              BSkyVariableList variableFromCopycopy = null;
        //              variableFromCopycopy = getCtrl(child1.Name, copy) as BSkyVariableList;
        //              variableFromCopycopy.SelectionChangeBehaviour = child1.SelectionChangeBehaviour;

        //            }
        //            if (child.GetType().Name == "BSkyGroupingVariable")
        //            {
        //                BSkyGroupingVariable variableFromCopycopy = null;
        //                BSkyGroupingVariable child1;
        //                child1 = child as BSkyGroupingVariable;
        //                variableFromCopycopy = getCtrl(child1.Name, copy) as BSkyGroupingVariable;
        //                variableFromCopycopy.SelectionChangeBehaviour = child1.SelectionChangeBehaviour;
        //            }

        //        }

        //    }
        //}


        void copyVariables(BSkyCanvas copy)
        {
            foreach (UIElement child in myCanvas.Children)
            {
                if (child.GetType().Name == "BSkySourceList" || child.GetType().Name == "BSkyTargetList" || child.GetType().Name == "BSkyGroupingVariable" || child.GetType().Name == "BSkyListBoxwBorderForDatasets" || child.GetType().Name == "BSkyAggregateCtrl" || child.GetType().Name == "BSkySortCtrl")
                {

                    if (child.GetType().Name == "BSkySourceList")
                    {
                        BSkySourceList b = null;
                        // Using the BSkyVariableList default constructor sets renderVars to False
                        //This ensures that when saving and subsequent open in the Application var1, var2 and var3 are not created
                        b = new BSkySourceList();

                        b.Resources.Clear();
                        BSkySourceList child1 = null;
                        child1 = child as BSkySourceList;
                        b.Filter = child1.Filter;
                        b.nomlevels = child1.nomlevels;
                        b.ordlevels = child1.ordlevels;
                        b.Syntax = child1.Syntax;
                        b.SubstituteSettings = child1.SubstituteSettings;
                        b.AutoVar = child1.AutoVar;
                        b.Name = child1.Name;



                        //b.DisplayMemberPath = child1.DisplayMemberPath;

                        //Added by Aaron 11/17/2013
                        //Commented the lines below as the getters and setters associated with the SelectonChangeBehaviour property
                        //getfired when all the controls are not yet loaded onthe canvas

                        // 
                        b.Enabled = child1.Enabled;
                        b.IsEnabled = child1.Enabled;
                        b.SelectionChangeBehaviour = child1.SelectionChangeBehaviour;
                        b.MoveVariables = child1.MoveVariables;
                        b.CanExecute = child1.CanExecute;
                        b.PrefixTxt = child1.PrefixTxt;
                        b.SepCharacter = child1.SepCharacter;
                        b.OverwriteSettings = child1.OverwriteSettings;
                        b.Width = child1.Width;
                        b.ItemTemplate = null;
                        b.Height = child1.Height;
                        b.dialogMode = false;
                        b.maxNoOfVariables = child1.maxNoOfVariables;
                        BSkyCanvas.SetTop(b, BSkyCanvas.GetTop(child1));
                        BSkyCanvas.SetLeft(b, BSkyCanvas.GetLeft(child1));
                        copy.Children.Add(b);
                    }

                    if (child.GetType().Name == "BSkyTargetList")
                    {
                        BSkyTargetList b = null;
                        // Using the BSkyVariableList default constructor sets renderVars to False
                        //This ensures that when saving and subsequent open in the Application var1, var2 and var3 are not created
                        b = new BSkyTargetList();

                        b.Resources.Clear();
                        BSkyTargetList child1 = null;
                        child1 = child as BSkyTargetList;
                        b.Filter = child1.Filter;
                        b.nomlevels = child1.nomlevels;
                        b.ordlevels = child1.ordlevels;
                        b.Syntax = child1.Syntax;
                        b.SubstituteSettings = child1.SubstituteSettings;
                        b.AutoVar = child1.AutoVar;
                        b.Name = child1.Name;
                        b.PrefixTxt = child1.PrefixTxt;
                        b.SepCharacter = child1.SepCharacter;
                        b.OverwriteSettings = child1.OverwriteSettings;

                        //b.DisplayMemberPath = child1.DisplayMemberPath;

                        //Added by Aaron 11/17/2013
                        //Commented the lines below as the getters and setters associated with the SelectonChangeBehaviour property
                        //getfired when all the controls are not yet loaded onthe canvas

                        // 
                        b.Enabled = child1.Enabled;
                        b.IsEnabled = child1.Enabled;
                        b.SelectionChangeBehaviour = child1.SelectionChangeBehaviour;
                        b.MoveVariables = child1.MoveVariables;
                        b.CanExecute = child1.CanExecute;
                        b.Width = child1.Width;
                        b.ItemTemplate = null;
                        b.Height = child1.Height;
                        b.dialogMode = false;
                        b.maxNoOfVariables = child1.maxNoOfVariables;
                        BSkyCanvas.SetTop(b, BSkyCanvas.GetTop(child1));
                        BSkyCanvas.SetLeft(b, BSkyCanvas.GetLeft(child1));
                        copy.Children.Add(b);
                    }

                    if (child.GetType().Name == "BSkyListBoxwBorderForDatasets")
                    {
                        BSkyListBoxwBorderForDatasets b = null;
                        // Using the BSkyVariableList default constructor sets renderVars to False
                        //This ensures that when saving and subsequent open in the Application var1, var2 and var3 are not created
                        b = new BSkyListBoxwBorderForDatasets();

                        b.Resources.Clear();
                        BSkyListBoxwBorderForDatasets child1 = null;
                        child1 = child as BSkyListBoxwBorderForDatasets;
                       // b.Filter = child1.Filter;
                       // b.nomlevels = child1.nomlevels;
                       // b.ordlevels = child1.ordlevels;
                        b.Syntax = child1.Syntax;
                        b.SubstituteSettings = child1.SubstituteSettings;
                       // b.AutoVar = child1.AutoVar;
                        b.Name = child1.Name;
                        b.SelectionChangeBehaviour = child1.SelectionChangeBehaviour;
                        b.PrefixTxt = child1.PrefixTxt;
                       
                        b.SepCharacter = child1.SepCharacter;
                       // b.OverwriteSettings = child1.OverwriteSettings;

                        //b.DisplayMemberPath = child1.DisplayMemberPath;

                        //Added by Aaron 11/17/2013
                        //Commented the lines below as the getters and setters associated with the SelectonChangeBehaviour property
                        //getfired when all the controls are not yet loaded onthe canvas

                        b.Enabled = child1.Enabled;
                        b.IsEnabled = child1.Enabled;
                      
                        b.MoveVariables = child1.MoveVariables;
                        b.CanExecute = child1.CanExecute;
                      
                        b.ItemTemplate = null;
                        b.Height = child1.Height;
                        b.Width = child1.Width;
                        b.AutoPopulate = child1.AutoPopulate;
                        b.DialogEditorMode = false;
                        b.maxNoOfVariables = child1.maxNoOfVariables;
                        BSkyCanvas.SetTop(b, BSkyCanvas.GetTop(child1));
                        BSkyCanvas.SetLeft(b, BSkyCanvas.GetLeft(child1));
                        copy.Children.Add(b);
                    }


                    if (child.GetType().Name == "BSkyGroupingVariable")
                    {
                        DragDropList oriDragDropList = null; 
                        BSkyGroupingVariable ori =child as BSkyGroupingVariable;
                        foreach (object orichild in ori.Children)
                        {
                            if (orichild is SingleItemList)
                            {
                                //vTargetList = child12 as DragDropList;
                                // targetListName = value;
                                //DragList child123;
                                oriDragDropList = orichild as DragDropList;
                                
                            }

                        }
                        BSkyGroupingVariable b = null;
                        // StackPanel child1;
                        // b = child as StackPanel;
                        BSkyGroupingVariable child1;
                        child1 = child as BSkyGroupingVariable;
                        // BSky.Controls.Controls.GroupingVariable b = null;
                        // Using the BSkySourceList default constructor sets renderVars to False
                        // This ensures that when saving and subsequent open in the Application var1, var2 and var3 are not created

                        b = new BSkyGroupingVariable();
                        b.nomlevels = child1.nomlevels;
                        b.ordlevels = child1.ordlevels;
                        b.SubstituteSettings = child1.SubstituteSettings;

                        b.CanExecute = child1.CanExecute;

                        b.Syntax = child1.Syntax; //09Jul2015

                        //Added by Aaron 11/17/2013
                        //Commented the lines below as the getters and setters associated with the SelectonChangeBehaviour property
                        //getfired when all the controls are not yet loaded onthe canvas
                        //b.SelectionChangeBehaviour = child1.SelectionChangeBehaviour;

                        //b.BSkySourceList.Resources.Clear();
                        //  StackPanel b1;
                        // b1 = b as StackPanel;

                        // Aaron 09/21/2013
                        //Clearing the ItemTemplate and resources of the objects withing the Grouping Variable
                        foreach (object child12 in b.Children)
                        {
                            if (child12 is SingleItemList)
                            {
                                //vTargetList = child12 as DragDropList;
                                // targetListName = value;
                                //DragList child123;
                                DragDropList child123 = child12 as DragDropList;
                                child123.dialogMode = false;
                                child123.Resources.Clear();
                                child123.ItemTemplate = null;
                                if (oriDragDropList !=null)
                                {
                                    child123.SepCharacter = oriDragDropList.SepCharacter;
                                    child123.PrefixTxt = oriDragDropList.PrefixTxt;
                                }
                            }

                        }



                        //  int count = b.Children.Count;
                        //  b.Children.RemoveRange(0, count);



                        //foreach (object childx in b.Children)
                        //{
                        //    if (childx.GetType().Name == "SingleItemList")
                        //    {
                        //        SingleItemList childxy= childx as SingleItemList;
                        //        childxy.Resources.Clear();
                        //        childxy.ItemTemplate = null;
                        //    }
                        //}

                        // b.Syntax = child1.Syntax;
                        // b.Filter = child1.Filter;
                        //  b.AutoVar = child1.AutoVar;

                        b.Name = child1.Name;
                        b.Enabled = child1.Enabled;
                        b.IsEnabled = child1.Enabled;
                        //b.DisplayMemberPath = child1.DisplayMemberPath;
                        b.SelectionChangeBehaviour = child1.SelectionChangeBehaviour;
                        // b.MoveVariables = child1.MoveVariables;
                        // b.CanExecute = child1.CanExecute;
                        b.Width = child1.Width;
                        // b.ItemTemplate = null;
                        b.Height = child1.Height;
                        b.Filter = child1.Filter;

                        BSkyCanvas.SetTop(b, BSkyCanvas.GetTop(child1));
                        BSkyCanvas.SetLeft(b, BSkyCanvas.GetLeft(child1));
                        copy.Children.Add(b);
                    }



                    if (child.GetType().Name == "BSkySortCtrl")
                    {
                        DragDropListForSummarize oriDragDropList = null;
                        BSkySortCtrl ori = child as BSkySortCtrl;
                        foreach (object orichild in ori.Children)
                        {
                            if (orichild is DragDropListForSummarize)
                            {
                                //vTargetList = child12 as DragDropList;
                                // targetListName = value;
                                //DragList child123;
                                oriDragDropList = orichild as DragDropListForSummarize;

                            }

                        }
                        BSkySortCtrl b = null;
                        // StackPanel child1;
                        // b = child as StackPanel;
                        BSkySortCtrl child1;
                        child1 = child as BSkySortCtrl;
                        // BSky.Controls.Controls.GroupingVariable b = null;
                        // Using the BSkySourceList default constructor sets renderVars to False
                        // This ensures that when saving and subsequent open in the Application var1, var2 and var3 are not created

                        b = new BSkySortCtrl();
                        b.nomlevels = child1.nomlevels;
                        b.ordlevels = child1.ordlevels;
                        b.SubstituteSettings = child1.SubstituteSettings;

                        b.CanExecute = child1.CanExecute;

                        b.Syntax = child1.Syntax; //09Jul2015

                        //Added by Aaron 11/17/2013
                        //Commented the lines below as the getters and setters associated with the SelectonChangeBehaviour property
                        //getfired when all the controls are not yet loaded onthe canvas
                        //b.SelectionChangeBehaviour = child1.SelectionChangeBehaviour;

                        //b.BSkySourceList.Resources.Clear();
                        //  StackPanel b1;
                        // b1 = b as StackPanel;

                        // Aaron 09/21/2013
                        //Clearing the ItemTemplate and resources of the objects withing the Grouping Variable
                        foreach (object child12 in b.Children)
                        {
                            if (child12 is DragDropListForSummarize)
                            {
                                //vTargetList = child12 as DragDropList;
                                // targetListName = value;
                                //DragList child123;
                                DragDropListForSummarize child123 = child12 as DragDropListForSummarize;
                                child123.dialogMode = false;
                                child123.Resources.Clear();
                                child123.ItemTemplate = null;
                                if (oriDragDropList != null)
                                {
                                    child123.SepCharacter = oriDragDropList.SepCharacter;
                                    child123.PrefixTxt = oriDragDropList.PrefixTxt;
                                }
                            }

                        }



                        //  int count = b.Children.Count;
                        //  b.Children.RemoveRange(0, count);



                        //foreach (object childx in b.Children)
                        //{
                        //    if (childx.GetType().Name == "SingleItemList")
                        //    {
                        //        SingleItemList childxy= childx as SingleItemList;
                        //        childxy.Resources.Clear();
                        //        childxy.ItemTemplate = null;
                        //    }
                        //}

                        // b.Syntax = child1.Syntax;
                        // b.Filter = child1.Filter;
                        //  b.AutoVar = child1.AutoVar;

                        b.Name = child1.Name;
                        b.Enabled = child1.Enabled;
                        b.IsEnabled = child1.Enabled;
                        //b.DisplayMemberPath = child1.DisplayMemberPath;
                        b.SelectionChangeBehaviour = child1.SelectionChangeBehaviour;
                        // b.MoveVariables = child1.MoveVariables;
                        // b.CanExecute = child1.CanExecute;
                        b.Width = child1.Width;
                        // b.ItemTemplate = null;
                        b.Height = child1.Height;
                        b.Filter = child1.Filter;

                        BSkyCanvas.SetTop(b, BSkyCanvas.GetTop(child1));
                        BSkyCanvas.SetLeft(b, BSkyCanvas.GetLeft(child1));
                        copy.Children.Add(b);
                    }

                    if (child.GetType().Name == "BSkyAggregateCtrl")
                    {
                        DragDropListForSummarize oriDragDropList = null;
                        BSkyAggregateCtrl ori = child as BSkyAggregateCtrl;
                        foreach (object orichild in ori.Children)
                        {
                            if (orichild is DragDropListForSummarize)
                            {
                                //vTargetList = child12 as DragDropList;
                                // targetListName = value;
                                //DragList child123;
                                oriDragDropList = orichild as DragDropListForSummarize;

                            }

                        }
                        BSkyAggregateCtrl b = null;
                        // StackPanel child1;
                        // b = child as StackPanel;
                        BSkyAggregateCtrl child1;
                        child1 = child as BSkyAggregateCtrl;
                        // BSky.Controls.Controls.GroupingVariable b = null;
                        // Using the BSkySourceList default constructor sets renderVars to False
                        // This ensures that when saving and subsequent open in the Application var1, var2 and var3 are not created

                        b = new BSkyAggregateCtrl();
                        b.nomlevels = child1.nomlevels;
                        b.ordlevels = child1.ordlevels;
                        b.SubstituteSettings = child1.SubstituteSettings;

                        b.CanExecute = child1.CanExecute;

                        b.Syntax = child1.Syntax; //09Jul2015

                        //Added by Aaron 11/17/2013
                        //Commented the lines below as the getters and setters associated with the SelectonChangeBehaviour property
                        //getfired when all the controls are not yet loaded onthe canvas
                        //b.SelectionChangeBehaviour = child1.SelectionChangeBehaviour;

                        //b.BSkySourceList.Resources.Clear();
                        //  StackPanel b1;
                        // b1 = b as StackPanel;

                        // Aaron 09/21/2013
                        //Clearing the ItemTemplate and resources of the objects withing the Grouping Variable
                        foreach (object child12 in b.Children)
                        {
                            if (child12 is DragDropListForSummarize)
                            {
                                //vTargetList = child12 as DragDropList;
                                // targetListName = value;
                                //DragList child123;
                                DragDropListForSummarize child123 = child12 as DragDropListForSummarize;
                                child123.dialogMode = false;
                                child123.Resources.Clear();
                                child123.ItemTemplate = null;
                                if (oriDragDropList != null)
                                {
                                    child123.SepCharacter = oriDragDropList.SepCharacter;
                                    child123.PrefixTxt = oriDragDropList.PrefixTxt;
                                }
                            }

                        }



                        //  int count = b.Children.Count;
                        //  b.Children.RemoveRange(0, count);



                        //foreach (object childx in b.Children)
                        //{
                        //    if (childx.GetType().Name == "SingleItemList")
                        //    {
                        //        SingleItemList childxy= childx as SingleItemList;
                        //        childxy.Resources.Clear();
                        //        childxy.ItemTemplate = null;
                        //    }
                        //}

                        // b.Syntax = child1.Syntax;
                        // b.Filter = child1.Filter;
                        //  b.AutoVar = child1.AutoVar;

                        b.Name = child1.Name;
                        b.Enabled = child1.Enabled;
                        b.IsEnabled = child1.Enabled;
                        //b.DisplayMemberPath = child1.DisplayMemberPath;
                        b.SelectionChangeBehaviour = child1.SelectionChangeBehaviour;
                        // b.MoveVariables = child1.MoveVariables;
                        // b.CanExecute = child1.CanExecute;
                        b.Width = child1.Width;
                        // b.ItemTemplate = null;
                        b.Height = child1.Height;
                        b.Filter = child1.Filter;

                        BSkyCanvas.SetTop(b, BSkyCanvas.GetTop(child1));
                        BSkyCanvas.SetLeft(b, BSkyCanvas.GetLeft(child1));
                        copy.Children.Add(b);
                    }



                }

            }
        }

        void copyAllExceptVars(BSkyCanvas copy)
        {
            foreach (UIElement child in myCanvas.Children)
            {
                if (child.GetType().Name == "BSkyButton")
                {
                    BSkyButton copybutton = null;
                    copybutton = new BSkyButton();
                    BSkyButton child1 = null;
                    child1 = child as BSkyButton;
                    copybutton.Name = child1.Name;
                    copybutton.Text = child1.Text;
                    copybutton.Name = child1.Name;
                    //copybutton.Resources["dlg"] = child1.Resources["dlg"];
                    //originalCanvas.Add(child1.Resources["dlg"], "dlg");
                    copybutton.Resources = new ResourceDictionary();
                  //  copybutton.Resources["dlg"] = child1.Resources["dlg"];

                    copybutton.Resources.Add("dlg", child1.Resources["dlg"]);

                 //   originalCanvas.Add("dlg", child1.Resources["dlg"]);
                    copybutton.ClickBehaviour = child1.ClickBehaviour;
                    copybutton.Designer = child1.Designer;
                    copybutton.Top = child1.Top;
                    //Aaron figure out this later
                    //copybutton.baseWindow =child1.baseWindow
                    copybutton.Enabled = child1.Enabled;
                    copybutton.IsEnabled = child1.Enabled;
                    copybutton.Width = child1.Width;
                    copybutton.Height = child1.Height;
                    BSkyCanvas.SetTop(copybutton, BSkyCanvas.GetTop(child1));
                    BSkyCanvas.SetLeft(copybutton, BSkyCanvas.GetLeft(child1));
                    copy.Children.Add(copybutton);
                }

                if (child.GetType().Name == "BSkyTextBox")
                {
                    BSkyTextBox copytextbox = null;
                    copytextbox = new BSkyTextBox();
                    BSkyTextBox child1 = null;
                    child1 = child as BSkyTextBox;
                    copytextbox.SubstituteSettings = child1.SubstituteSettings;
                    copytextbox.Name = child1.Name;
                    copytextbox.Text = child1.Text;
                    copytextbox.Name = child1.Name;
                    copytextbox.Width = child1.Width;
                    copytextbox.Height = child1.Height;
                    copytextbox.TextChangedBehaviour = child1.TextChangedBehaviour;
                    copytextbox.Top = child1.Top;
                    copytextbox.PrefixTxt = child1.PrefixTxt;
                   // copytextbox.OverWriteExistingVariables = child1.OverWriteExistingVariables;
                    copytextbox.OverwriteSettings = child1.OverwriteSettings;
                    //Added by Aaron 04/06/2014

                    copytextbox.Enabled = child1.Enabled;
                    copytextbox.IsEnabled = child1.Enabled;
                    //  copytextbox.RequiredSyntax = copytextbox.RequiredSyntax;

                    //Added by Aaron 08/25/2013
                    copytextbox.CanExecute = child1.CanExecute;
                    // copytextbox.dialogMode = false;
                    BSkyCanvas.SetTop(copytextbox, BSkyCanvas.GetTop(child1));
                    BSkyCanvas.SetLeft(copytextbox, BSkyCanvas.GetLeft(child1));
                    copy.Children.Add(copytextbox);
                }
                if (child.GetType().Name == "BSkyLabel")
                {
                    BSkyLabel copyLabel = null;
                    copyLabel = new BSkyLabel();
                    BSkyLabel child1 = null;
                    child1 = child as BSkyLabel;

                    copyLabel.Left = child1.Left;
                    copyLabel.Top = child1.Top;
                    copyLabel.Name = child1.Name;
                    copyLabel.Text = child1.Text;
                    copyLabel.Width = child1.Width;
                    copyLabel.Height = child1.Height;
                    copyLabel.CanExecute = child1.CanExecute;
                    //copyMoveButton.Name = child1.Name;
                    BSkyCanvas.SetTop(copyLabel, BSkyCanvas.GetTop(child1));
                    BSkyCanvas.SetLeft(copyLabel, BSkyCanvas.GetLeft(child1));
                    copy.Children.Add(copyLabel);

                }


                if (child.GetType().Name == "BSkyMultiLineLabel")
                {
                    BSkyMultiLineLabel copyLabel = null;
                    copyLabel = new BSkyMultiLineLabel();
                    BSkyMultiLineLabel child1 = null;
                    child1 = child as BSkyMultiLineLabel;
                   
                    copyLabel.Left = child1.Left;
                    copyLabel.Top = child1.Top;
                    copyLabel.Name = child1.Name;
                    copyLabel.Text = child1.Text;
                    copyLabel.Width = child1.Width;
                    copyLabel.Height = child1.Height;
                   // copyLabel.CanExecute = child1.CanExecute;
                    //copyMoveButton.Name = child1.Name;
                    BSkyCanvas.SetTop(copyLabel, BSkyCanvas.GetTop(child1));
                    BSkyCanvas.SetLeft(copyLabel, BSkyCanvas.GetLeft(child1));
                    copy.Children.Add(copyLabel);

                }
                if (child.GetType().Name == "BSkyVariableMoveButton")
                {
                    BSkyVariableMoveButton copyMoveButton = null;
                    copyMoveButton = new BSkyVariableMoveButton();
                    BSkyVariableMoveButton child1 = null;
                    child1 = child as BSkyVariableMoveButton;

                    copyMoveButton.Name = child1.Name;

                    //copyMoveButton.Name = child1.Name;

                    copyMoveButton.Name = child1.Name;
                    copyMoveButton.Width = child1.Width;
                    copyMoveButton.Height = child1.Height;
                    //copyMoveButton.Top = child1.Top;
                    BSkyCanvas.SetTop(copyMoveButton, BSkyCanvas.GetTop(child1));
                    BSkyCanvas.SetLeft(copyMoveButton, BSkyCanvas.GetLeft(child1));
                    copy.Children.Add(copyMoveButton);
                    // 11/26/2012 the lines below can be executed only after the movebutton is added to the canvas
                    copyMoveButton.InputList = child1.InputList;
                    copyMoveButton.TargetList = child1.TargetList;

                }

                if (child.GetType().Name == "BSkyRadioButton")
                {
                    BSkyRadioButton copyRadioButton = null;
                    copyRadioButton = new BSkyRadioButton();
                    BSkyRadioButton child1 = null;
                    child1 = child as BSkyRadioButton;

                    copyRadioButton.GroupName = child1.GroupName;

                    //copyMoveButton.Name = child1.Name;

                    copyRadioButton.Name = child1.Name;
                    copyRadioButton.Width = child1.Width;
                    copyRadioButton.Height = child1.Height;
                    copyRadioButton.CheckedChangeBehaviour = child1.CheckedChangeBehaviour;
                    copyRadioButton.Syntax = child1.Syntax;
                    copyRadioButton.Text = child1.Text;
                    copyRadioButton.Left = child1.Left;
                    copyRadioButton.Top = child1.Top;
                    copyRadioButton.IsSelected = child1.IsSelected;
                    copyRadioButton.Enabled = child1.Enabled;
                    copyRadioButton.IsEnabled = child1.Enabled;

                    //copyMoveButton.Top = child1.Top;
                    BSkyCanvas.SetTop(copyRadioButton, BSkyCanvas.GetTop(child1));
                    BSkyCanvas.SetLeft(copyRadioButton, BSkyCanvas.GetLeft(child1));
                    copy.Children.Add(copyRadioButton);
                    // 11/26/2012 the lines below can be executed only after the movebutton is added to the canvas

                }

                if (child.GetType().Name == "BSkyRadioGroup")
                {
                    //NOTE: THE TEXT (MAPS TO THE TEXT PROPERTY IN BASE GROUPBOX CLASS)AND RADIOBUTTONS PROPERTY
                    //OVERWRITE EACH OTHER.
                    //YOU CAN EITHER CREATE RADIO BUTTONS BY 
                    BSkyRadioGroup copyRadioGroup = null;
                    copyRadioGroup = new BSkyRadioGroup();
                    BSkyRadioGroup child1 = null;
                    child1 = child as BSkyRadioGroup;

                    copyRadioGroup.RadioButtons = child1.RadioButtons;

                    //Source radiogroup's content
                    StackPanel stkpnl = child1.Content as StackPanel;
                    StackPanel copystkpnl = copyRadioGroup.Content as StackPanel;
                    //This code creates new radio buttons and adds it to the stackpanel
                    foreach (object obj in stkpnl.Children)
                    {
                        //BSkyRadioButton tmp = obj as BSkyRadioButton;
                        BSkyRadioButton oriRdbBtn = obj as BSkyRadioButton;
                        BSkyRadioButton copyobj = new BSkyRadioButton();

                        copyobj.GroupName = oriRdbBtn.GroupName;

                        //copyMoveButton.Name = child1.Name;

                        copyobj.Name = oriRdbBtn.Name;
                        copyobj.Width = oriRdbBtn.Width;
                        copyobj.Height = oriRdbBtn.Height;
                        copyobj.CheckedChangeBehaviour = oriRdbBtn.CheckedChangeBehaviour;
                        copyobj.Syntax = oriRdbBtn.Syntax;
                        copyobj.Text = oriRdbBtn.Text;
                        copyobj.Left = oriRdbBtn.Left;
                        copyobj.Top = oriRdbBtn.Top;
                        copyobj.IsSelected = oriRdbBtn.IsSelected;
                        copyobj.Enabled = oriRdbBtn.Enabled;
                        copyobj.IsEnabled = oriRdbBtn.Enabled;


                        copystkpnl.Children.Add(copyobj);
                    }

                    copyRadioGroup.Syntax = child1.Syntax;

                    copyRadioGroup.Header = child1.Header;
                    copyRadioGroup.Name = child1.Name;
                    copyRadioGroup.HeaderText = child1.HeaderText;

                    //12/29/2012
                    //The Text property below and the copystkpnl both point to the same Content property
                    //of BSkyRadioButton. Hence setting the text property below overwrites the stackpanel.
                    //We need to choose one over the other
                    if (child1.Text != null) copyRadioGroup.Text = child1.Text;
                    copyRadioGroup.Width = child1.Width;
                    copyRadioGroup.Height = child1.Height;
                    copyRadioGroup.Left = child1.Left;
                    copyRadioGroup.Top = child1.Top;

                    //copyMoveButton.Top = child1.Top;
                    BSkyCanvas.SetTop(copyRadioGroup, BSkyCanvas.GetTop(child1));
                    BSkyCanvas.SetLeft(copyRadioGroup, BSkyCanvas.GetLeft(child1));
                    copy.Children.Add(copyRadioGroup);
                    // 11/26/2012 the lines below can be executed only after the movebutton is added to the canvas

                }

                if (child.GetType().Name == "BSkyCheckBox")
                {
                    BSkyCheckBox copyCheckBox = null;
                    copyCheckBox = new BSkyCheckBox();
                    BSkyCheckBox child1 = null;
                    child1 = child as BSkyCheckBox;

                    //copyMoveButton.Name = child1.Name;
                    copyCheckBox.CanExecute = child1.CanExecute;
                    copyCheckBox.Name = child1.Name;
                    copyCheckBox.Width = child1.Width;
                    copyCheckBox.Height = child1.Height;
                    copyCheckBox.CheckedChangeBehaviour = child1.CheckedChangeBehaviour;
                    copyCheckBox.Syntax = child1.Syntax;
                    copyCheckBox.Text = child1.Text;
                    copyCheckBox.Left = child1.Left;
                    copyCheckBox.Top = child1.Top;
                    copyCheckBox.uncheckedsyntax = child1.uncheckedsyntax;
                    copyCheckBox.IsSelected = child1.IsSelected;
                    copyCheckBox.Enabled = child1.Enabled;
                    copyCheckBox.IsEnabled = child1.Enabled;


                    //copyMoveButton.Top = child1.Top;
                    BSkyCanvas.SetTop(copyCheckBox, BSkyCanvas.GetTop(child1));
                    BSkyCanvas.SetLeft(copyCheckBox, BSkyCanvas.GetLeft(child1));
                    copy.Children.Add(copyCheckBox);

                }

                if (child.GetType().Name == "BSkyGroupBox")
                {
                    BSkyGroupBox copyGroupBox = null;
                    copyGroupBox = new BSkyGroupBox();
                    BSkyGroupBox child1 = null;
                    child1 = child as BSkyGroupBox;

                    //copyMoveButton.Name = child1.Name;

                    copyGroupBox.Name = child1.Name;
                    copyGroupBox.Width = child1.Width;
                    copyGroupBox.Height = child1.Height;
                    copyGroupBox.Text = child1.Text;
                    copyGroupBox.Left = child1.Left;
                    copyGroupBox.Top = child1.Top;
                    copyGroupBox.HeaderText = child1.HeaderText;


                    //copyMoveButton.Top = child1.Top;
                    BSkyCanvas.SetTop(copyGroupBox, BSkyCanvas.GetTop(child1));
                    BSkyCanvas.SetLeft(copyGroupBox, BSkyCanvas.GetLeft(child1));
                    copy.Children.Add(copyGroupBox);

                }

                if (child.GetType().Name == "BSkyEditableComboBox")
                {
                    BSkyEditableComboBox copyComboBox = null;
                    copyComboBox = new BSkyEditableComboBox();
                    copyComboBox.Resources.Clear();
                    BSkyEditableComboBox child1 = null;
                    child1 = child as BSkyEditableComboBox;
                    //copyMoveButton.Name = child1.Name;
                    copyComboBox.prefixSelectedValue = child1.prefixSelectedValue;
                    copyComboBox.Name = child1.Name;
                    copyComboBox.Width = child1.Width;
                    copyComboBox.Height = child1.Height;
                    copyComboBox.DefaultSelection = child1.DefaultSelection;
                    copyComboBox.SelectionChangeBehaviour = child1.SelectionChangeBehaviour;
                    copyComboBox.NothingSelected = child1.NothingSelected;
                    copyComboBox.SelectedValue = child1.SelectedValue;
                    copyComboBox.CanExecute = child1.CanExecute;
                    foreach (string Item in child1.Items)
                    {
                        copyComboBox.Items.Add(Item);
                    }


                    //As we are referencing the text property in Display items, the code below must execute after the Text property is set
                   // foreach (string displayItem in child1.DisplayItems) copyComboBox.DisplayItems.Add(displayItem);
                    // copyComboBox.SelectedObject = child1.SelectedObject;
                  //  copyComboBox.DisplayItems = child1.DisplayItems;
                    copyComboBox.Syntax = child1.Syntax;
                    copyComboBox.Enabled = child1.Enabled;
                    copyComboBox.IsEnabled = child1.Enabled;

                    //copyMoveButton.Top = child1.Top;
                    BSkyCanvas.SetTop(copyComboBox, BSkyCanvas.GetTop(child1));
                    BSkyCanvas.SetLeft(copyComboBox, BSkyCanvas.GetLeft(child1));
                    copy.Children.Add(copyComboBox);
                }

                if (child.GetType().Name == "BSkyNonEditableComboBox")
                {
                    BSkyNonEditableComboBox copyComboBox = null;
                    copyComboBox = new BSkyNonEditableComboBox();
                    BSkyNonEditableComboBox child1 = null;
                    child1 = child as BSkyNonEditableComboBox;
                    //copyMoveButton.Name = child1.Name;

                    copyComboBox.Name = child1.Name;
                    copyComboBox.Width = child1.Width;
                    copyComboBox.Height = child1.Height;
                    //  copyComboBox.DefaultSelection = child1.DefaultSelection;
                    copyComboBox.SelectionChangeBehaviour = child1.SelectionChangeBehaviour;
                    copyComboBox.NothingSelected = child1.NothingSelected;
                    copyComboBox.SelectedValue = child1.SelectedValue;
                  //  copyComboBox.DefaultSelection = child1.DefaultSelection;
                    copyComboBox.CanExecute = child1.CanExecute;
                    //As we are referencing the text property in Display items, the code below must execute after the Text property is set
                 //   foreach (string displayItem in child1.DisplayItems) copyComboBox.DisplayItems.Add(displayItem);
                    foreach (string Item in child1.Items)
                    {
                        copyComboBox.Items.Add(Item);
                    }
                    // copyComboBox.SelectedObject = child1.SelectedObject;
                    copyComboBox.Syntax = child1.Syntax;
                    copyComboBox.Enabled = child1.Enabled;
                    copyComboBox.IsEnabled = child1.Enabled;

                    //copyMoveButton.Top = child1.Top;
                    BSkyCanvas.SetTop(copyComboBox, BSkyCanvas.GetTop(child1));
                    BSkyCanvas.SetLeft(copyComboBox, BSkyCanvas.GetLeft(child1));
                    copy.Children.Add(copyComboBox);
                }

                if (child.GetType().Name == "BSkygridForSymbols")
                {

                    BSkygridForSymbols copygridForSymbols = null;
                    copygridForSymbols = new BSkygridForSymbols();
                    BSkygridForSymbols child1 = null;
                    child1 = child as BSkygridForSymbols;
                    //copyMoveButton.Name = child1.Name;
                    copygridForSymbols.Name = child1.Name;
                    copygridForSymbols.Width = child1.Width;
                    copygridForSymbols.Height = child1.Height;
                    copygridForSymbols.textBoxName = child1.textBoxName;
                    copygridForSymbols.name = child1.name;

                    //copyMoveButton.Top = child1.Top;
                    BSkyCanvas.SetTop(copygridForSymbols, BSkyCanvas.GetTop(child1));
                    BSkyCanvas.SetLeft(copygridForSymbols, BSkyCanvas.GetLeft(child1));
                    copy.Children.Add(copygridForSymbols);
                }

                if (child.GetType().Name == "BSkygridForCompute")
                {

                    BSkygridForCompute copygridForCompute = null;
                    copygridForCompute = new BSkygridForCompute();
                    BSkygridForCompute child1 = null;
                    child1 = child as BSkygridForCompute;
                    //copyMoveButton.Name = child1.Name;
                    copygridForCompute.Name = child1.Name;
                    copygridForCompute.Width = child1.Width;
                    copygridForCompute.Height = child1.Height;
                    copygridForCompute.TextBoxNameForSyntaxSubstitution = child1.TextBoxNameForSyntaxSubstitution;

                   // copygridForSymbols.textBoxName = child1.textBoxName;
                  //  copygridForCompute.name = child1.name;

                    //copyMoveButton.Top = child1.Top;
                    copygridForCompute.tab1.SelectedItem = null;
                    copygridForCompute.tab1.Items.Clear();
                    copygridForCompute.Children.Clear();
                      
                   
                    BSkyCanvas.SetTop(copygridForCompute, BSkyCanvas.GetTop(child1));
                    BSkyCanvas.SetLeft(copygridForCompute, BSkyCanvas.GetLeft(child1));
                    copy.Children.Add(copygridForCompute);
                }

                //if (child.GetType().Name == "BSkyAggregateCtrl")
                //{

                //    BSkyAggregateCtrl copyAggregateCtrl = null;
                //    copyAggregateCtrl = new BSkyAggregateCtrl();
                //    BSkyAggregateCtrl child1 = child as BSkyAggregateCtrl;
                //    child1 = child as BSkyAggregateCtrl;
                //    //copyMoveButton.Name = child1.Name;
                //    copyAggregateCtrl.Name = child1.Name;
                //    copyAggregateCtrl.Width = child1.Width;
                //    copyAggregateCtrl.Height = child1.Height;
                //   // copyAggregateCtrl.TextBoxNameForSyntaxSubstitution = child1.TextBoxNameForSyntaxSubstitution;

                //    // copygridForSymbols.textBoxName = child1.textBoxName;
                //    //  copygridForCompute.name = child1.name;

                //    //copyMoveButton.Top = child1.Top;
                //   // copygridForCompute.tab1.SelectedItem = null;
                //   // copygridForCompute.tab1.Items.Clear();
                //    copyAggregateCtrl.Children.Clear();


                //    BSkyCanvas.SetTop(copyAggregateCtrl, BSkyCanvas.GetTop(child1));
                //    BSkyCanvas.SetLeft(copyAggregateCtrl, BSkyCanvas.GetLeft(child1));
                //    copy.Children.Add(copyAggregateCtrl);
                //}

                if (child.GetType().Name == "BSkyMasterListBox")
                {
                    BSkyMasterListBox copyListBox = null;
                    copyListBox = new BSkyMasterListBox();
                    copyListBox.Resources.Clear();
                    BSkyMasterListBox child1 = null;
                    child1 = child as BSkyMasterListBox;
                    //copyMoveButton.Name = child1.Name;
                    copyListBox.Name = child1.Name;
                    copyListBox.Width = child1.Width;
                    copyListBox.Height = child1.Height;
                    copyListBox.MultiSelect = child1.MultiSelect;
                    copyListBox.SubstituteSettings = child1.SubstituteSettings;
                    copyListBox.SlaveListBoxName = child1.SlaveListBoxName;
                    foreach (string Item in child1.Items)
                    {
                        copyListBox.Items.Add(Item);
                    }
                    copyListBox.Top = child1.Top;
                    copyListBox.CanExecute = child1.CanExecute;
                    copyListBox.SelectionChangeBehaviour = child1.SelectionChangeBehaviour;
                    copyListBox.MappingMasterSlaveEntries = child1.MappingMasterSlaveEntries;

                    BSkyCanvas.SetTop(copyListBox, BSkyCanvas.GetTop(child1));
                    BSkyCanvas.SetLeft(copyListBox, BSkyCanvas.GetLeft(child1));
                    copy.Children.Add(copyListBox);
                }




                if (child.GetType().Name == "BSkyListBox")
                {
                    BSkyListBox copyListBox = null;
                    copyListBox = new BSkyListBox();
                    copyListBox.Resources.Clear();
                    BSkyListBox child1 = null;
                    child1 = child as BSkyListBox;
                    //copyMoveButton.Name = child1.Name;
                    copyListBox.Name = child1.Name;
                    copyListBox.Width = child1.Width;
                    copyListBox.Height = child1.Height;
                    copyListBox.MultiSelect = child1.MultiSelect;
                    copyListBox.SubstituteSettings = child1.SubstituteSettings;
                    foreach (string Item in child1.Items)
                    {
                        copyListBox.Items.Add(Item);
                    }
                    copyListBox.Top = child1.Top;
                    copyListBox.CanExecute = child1.CanExecute;
                    copyListBox.SelectionChangeBehaviour = child1.SelectionChangeBehaviour;
                    BSkyCanvas.SetTop(copyListBox, BSkyCanvas.GetTop(child1));
                    BSkyCanvas.SetLeft(copyListBox, BSkyCanvas.GetLeft(child1));
                    copy.Children.Add(copyListBox);
                }
            }

        }



        //Added by Aaron 12/08/2012
        void SaveSubDialogBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            //Added by Aaron 05/01/2015
            //When I was in a sub dialog and I selected an item e.g. radio button control rb, I then closed the sub dialog I then opened the same sub dialog, added a control, then directly pressed the delete key
            //the item that I selected previously i.e. radio button control rb got selected
            string message = "";
            if (gridLines == true)
            {
                message = "You need to turn off the grid lines before saving. Click on Dialog->Remove Grid Lines.";
                MessageBox.Show(message);
                return;
            }
            //The code below prevents that
            setIsEnabled(Canvas);
            selectedElement = null;
            selectedElementRef=null;
            this.DialogResult = true;

        }

        private void setIsEnabled(BSkyCanvas Canvas)
        {
            foreach (object obj in Canvas.Children)
            {
                if (obj is IBSkyEnabledControl)
                {

                    IBSkyEnabledControl bskyCtrl = obj as IBSkyEnabledControl;
                    bskyCtrl.IsEnabled = bskyCtrl.Enabled;
                }
            }
        }


        //When I click close I disconnect the canvas from the dialog
        void Closebinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            //Window1 w = new Window1(true);
            //w.ShowDialog();
            //this.DialogResult = false;
            // CanvasHost.Child = myCanvas;
            // CanvasPropertyGrid.SelectedObject = myCanvas;
            // OptionsPropertyGrid.SelectedObject = myCanvas;
            // myCanvas.Children.Remove(selectedElement);
            if (saved == false)
            {

                System.Windows.Forms.DialogResult result = System.Windows.Forms.MessageBox.Show("Do you want to save changes?", "Save Changes", System.Windows.Forms.MessageBoxButtons.YesNoCancel, System.Windows.Forms.MessageBoxIcon.Question);
                if (result == System.Windows.Forms.DialogResult.Yes)//save
                {
                    fileSave(false);
                    return;
                }
                if (result == System.Windows.Forms.DialogResult.Cancel)//save
                {
                    return;
                }


            }
            int count1 = myCanvas.Children.Count;

            //Clearing the first canvas as we are closong the dialog
            myCanvas.Children.RemoveRange(0, count1);

            //Added by Aaron 05/12/2014
            BSkyCanvas.chainOpenCanvas.Clear();


            //Added by Aaron 12/07/2013
            //This was done to remove the situation where I open a dialog, selected an item on the canvas by click on it, moved the item
            //since the control was moved, selected was set to true. I know close the dialog, selected remains true. 
            //I then click down and there is a crash
            selected = false;
            CanvasHost.Child = null;
            c1ToolbarStrip1.IsEnabled = false;
            CanvasPropertyGrid.SelectedObject = null;
            OptionsPropertyGrid.SelectedObject = null;
            nameOfFile = string.Empty;
            //Added 04/07/2013
            //added as the title of the window was not getting reset on closing a file
            Title = string.Empty;
            CanOpen = true;

            saved = true;
            CanSave = false;
            CanClose = false;
            this.Focus();
            return;



        }


        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)///15Jan2013
        {

            if (mainDialog == true)
            {
                if (saved == false)
                {

                    System.Windows.Forms.DialogResult result = System.Windows.Forms.MessageBox.Show("Do you want to save changes?", "Save Changes", System.Windows.Forms.MessageBoxButtons.YesNoCancel, System.Windows.Forms.MessageBoxIcon.Question);
                    if (result == System.Windows.Forms.DialogResult.Yes)//save
                    {
                        fileSave(false);
                        e.Cancel = true; //Anil:Abort window closing event.
                        return;
                    }
                    if (result == System.Windows.Forms.DialogResult.No)//Don't save
                    {
                        return;
                    }
                    if (result == System.Windows.Forms.DialogResult.Cancel)//Cancel exit
                    {
                        e.Cancel = true; //Anil:Abort window closing event.
                        return;
                    }


                }
                Application.Current.Shutdown();
            }
            else
            {
                this.DialogResult = true;
            }
            // MessageBox.Show("the rain in spain");//Application.Exit();
            //MenuEditor editor = new MenuEditor();
            //editor.ElementLocation = myCanvas.MenuLocation;
            //editor.ShowDialog();
            //if (editor.DialogResult.HasValue && editor.DialogResult.Value)
            //{
            //    string str = editor.ElementLocation;
            //    myCanvas.MenuLocation = str;
            //}
        }




        void SaveAs_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (!CanSave)
            {
                e.CanExecute = false;
            }
            else
                e.CanExecute = true;
        }


        void SaveAs_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            fileSave(true);
            //string filename = string.Empty;
            ////Out are reference variables. So we pass filename to the SaveXaml function and even though SaveXaml does
            ////not pass filename back, we can still access it. 
            //bool saveAs = true;
            //List<string> lst;
            //string message;
            //int outputFile = 0;
            ////The function below recursively looks through the canvas looking for any objects with a missing name
            ////The function returns a list of all the BSky dialog controls that have a missing name
            //lst = checkMissingName(firstCanvas);
            //if (lst.Count > 0)
            //{
            //    if (lst.Count == 1) message = string.Format("You need to enter the Name property for the following control \n{0} ", string.Join(",", lst));
            //    else message = string.Format("You need to enter the Name property for the following controls \n{0} ", string.Join("\n", lst));
            //    MessageBox.Show(message);
            //    return;
            //}
            ////Holds the griditem in the canvasPropertyGrid for the outputDefinition
            //System.Windows.Forms.GridItem outputDefGridItem;
            //outputFile = checkMissingOutputFile(firstCanvas);

            ////Case where I want to enter an output file, so I return with the outputfile highlighted
            ////In all other cases I proceed without an output file definition
            //if (outputFile == 1)
            //{
            //    outputDefGridItem = GetSelectedGridItem("OutputDefinition");
            //    CanvasPropertyGrid.Focus();
            //    outputDefGridItem.Select();
            //    return;
            //}
            //if (SaveXaml(out filename, saveAs))
            //{
            //    string fileNamewithoutExt = Path.GetFileNameWithoutExtension(filename);
            //    string filePath = Path.GetDirectoryName(filename);
            //    string zipFileName = fileNamewithoutExt;
            //    zipFileName = Path.Combine(filePath, zipFileName + ".bsky");

            //    ZipFile zf = ZipFile.Create(zipFileName);
            //    zf.BeginUpdate();
            //    zf.Add(Path.Combine(filePath, fileNamewithoutExt + ".xaml"), fileNamewithoutExt + ".xaml");
            //    if (File.Exists(myCanvas.OutputDefinition))
            //    {
            //        zf.Add(myCanvas.OutputDefinition, Path.GetFileName(myCanvas.OutputDefinition));
            //    }
            //    else
            //    {
            //        MessageBox.Show("Output Definition is not defined");
            //    }
            //    zf.CommitUpdate();
            //    zf.Close();
            //    File.Delete(Path.Combine(filePath, fileNamewithoutExt + ".xaml"));
            //}
        }


        //Added by Aaron 10/24/2013
        //Code below checks the BSkyVariableMoveButton and ensures that the source and destination lists are valid

        bool checkMoveButton(BSkyCanvas canvas)
        {
            string message = string.Empty;
            foreach (Object obj in canvas.Children)
            {
                if (obj is BSkyVariableMoveButton)
                {
                    BSkyVariableMoveButton objcast = obj as BSkyVariableMoveButton;
                    if (objcast.GetResource(objcast.TargetList) == null)
                    {
                        message = "The move button " + objcast.Name + " is not associated with a proper target variable list control. Please click on the move button and check the target variable list name ";
                        MessageBox.Show(message);
                        //this.selectedElement = objcast;
                        return false;
                    }
                    if (objcast.GetResource(objcast.InputList) == null)
                    {
                        message = "The Move Button " + objcast.Name + " is not associated with a proper input variable list control. Please click on the move button and check the input variable list name ";
                        MessageBox.Show(message);
                        //this.selectedElement = objcast;
                        return false;
                    }

                }
                if (obj is BSkyButton)
                {
                    FrameworkElement fe = obj as FrameworkElement;
                    BSkyCanvas cs = fe.Resources["dlg"] as BSkyCanvas;
                    // if (cs != null) lst.AddRange(checkMissingName(cs));
                    //Added by Aaron 12/27/2013
                    //Code was added to handle the case of a buttom that is placed on the canvas but we have not defined a subdialog for that button i.e. I have not clicked on designer in the property grid
                    if (cs != null) checkMoveButton(cs);
                }
            }
            return true;

        }

        //Added by Aaron 10/24/2013
        //this function is not used
        //Code below is called by BSkyVariableMoveButton
        //We look for valid and ensures that the source and destination lists are valid
        //bool GetResource(string name, BSkyCanvas canvas)
        //{
        //    foreach (FrameworkElement fe in canvas.Children)
        //    {
        //        //Checking if it is a valid variable list
        //        if (fe.Name == name)
        //        {
        //            DragDropList obj = fe as DragDropList;
        //            if (obj != null) return true;
        //            else
        //            {
        //                //Checking if its a valid grouping variable list
        //                BSkyGroupingVariable objcast = fe as BSkyGroupingVariable;
        //                if (objcast != null) return true;
        //            }
        //        }

        //    }
        //    return false;
        //}









        //Aaron 04/28/2013
        //saveAs tells us whether the file save is invoked from saveAs. 

        void fileSave(bool saveAs)
        {
            //copy = new BSkyCanvas();
            string filename = string.Empty;
            if (checkDialog() == false) return;

            if (SaveXaml(out filename, saveAs))
            {
                List<helpfileset> lstHelpFiles = new List<helpfileset>();
                lstHelpFiles = firstCanvas.gethelpfilenames(firstCanvas);
                string fileNamewithoutExt = Path.GetFileNameWithoutExtension(filename);
                string filePath = Path.GetDirectoryName(filename);
                string zipFileName = fileNamewithoutExt;
                zipFileName = Path.Combine(filePath, zipFileName + ".bsky");

                ZipFile zf = ZipFile.Create(zipFileName);
                zf.BeginUpdate();

                string zfilename = Path.Combine(filePath, fileNamewithoutExt + ".xaml");
                string zentryname = fileNamewithoutExt + ".xaml";

                //XAML, XML & help file should be in same location.Doesn't matter what sub-folder they are in.
                zf.Add(zfilename, "dialog.xaml");//System.IO.Path.GetFileName(zfilename));//11Sep2014

                //zf.Add(zfilename, zfilename);//Modified by Anil 11Apr2013 
                //zf.Add(zfilename, zentryname);//oldest
                if (File.Exists(myCanvas.OutputDefinition))
                {
                    zfilename = myCanvas.OutputDefinition;
                    zentryname = Path.GetFileName(myCanvas.OutputDefinition);

                    //XAML, XML & help file should be in same location.Doesn't matter what sub-folder they are in.
                    zf.Add(zfilename, "template.xml");//System.IO.Path.GetFileName(zfilename));//11Sep2014

                    //zf.Add(zfilename, zfilename); //Modified by Anil 11Apr2013
                    //zf.Add(zfilename, zentryname);//oldest
                }


                // int count = lstHelpFiles.Count

                foreach (helpfileset s in lstHelpFiles)
                {
                    if (s.newhelpfilepath != "urlOrUri")
                    {
                        zfilename = s.originalhelpfilepath;

                        //zentryname = Path.GetFileName(myCanvas.OutputDefinition);

                        //XAML, XML & help file should be in same location.Doesn't matter what sub-folder they are in.
                        zf.Add(zfilename, System.IO.Path.GetFileName(zfilename));//11Sep2014

                        //zf.Add(zfilename, zfilename); //Modified by Anil 11Apr2013
                        //zf.Add(zfilename, zentryname);//oldest
                    }
                }

                zf.CommitUpdate();
                zf.Close();
                File.Delete(Path.Combine(filePath, fileNamewithoutExt + ".xaml"));
                //01/20/2013
                //Making saved true as the file is saved
                saved = true;

            }
        }





        private bool checkTextBoxNameinGridforSym(BSkyCanvas canvas)
        {
            bool validtextboxname = true;
            foreach (Object obj in canvas.Children)
            {
                if (obj is BSkygridForSymbols)
                {
                    BSkygridForSymbols gfs = obj as BSkygridForSymbols;
                    if (!gfs.checkValidTextBox(gfs.textBoxName))
                    {
                        string message = "The textbox property '" + gfs.textBoxName + "' of the grid of symbols control named '" + gfs.Name + "' is invalid, the textbox property must reference  a valid textbox";
                        MessageBox.Show(message);
                        validtextboxname = false;
                        return validtextboxname;
                    }

                }
                if (obj is BSkyButton)
                {
                    FrameworkElement fe = obj as FrameworkElement;
                    BSkyCanvas cs = fe.Resources["dlg"] as BSkyCanvas;
                    if (cs != null) { validtextboxname = checkTextBoxNameinGridforSym(cs); }
                }
            }
            return validtextboxname;
        }


        private bool checkTextBoxNameinGridforCompute(BSkyCanvas canvas)
        {
            bool validtextboxname = true;
            foreach (Object obj in canvas.Children)
            {
                if (obj is BSkygridForCompute)
                {
                    BSkygridForCompute gfs = obj as BSkygridForCompute;
                    if (!gfs.checkValidTextBox(gfs.TextBoxNameForSyntaxSubstitution))
                    {
                        string message = "The textbox property '" + gfs.TextBoxNameForSyntaxSubstitution + "' of the grid of compute control named '" + gfs.Name + "' is invalid, the TextBoxNameForSyntaxSubstitution property must reference  a valid textbox";
                        MessageBox.Show(message);
                        validtextboxname = false;
                        return validtextboxname;
                    }

                }
                if (obj is BSkyButton)
                {
                    FrameworkElement fe = obj as FrameworkElement;
                    BSkyCanvas cs = fe.Resources["dlg"] as BSkyCanvas;
                    if (cs != null) { validtextboxname = checkTextBoxNameinGridforSym(cs); }
                }
            }
            return validtextboxname;
        }





        private bool checkmasterslave()
        {
            //Since we support master slave listboxes, we need to handle the following scenarios
            //1. The case that a master listbox points to a slave that does not exist or was deleted
            //2. Master listbox points to an empty slave, this is not fine and we will throw an error 
            bool isvalidslave = checkvalidslave(firstCanvas);
            if (!isvalidslave) return false;
            bool masterslaveuniquenessOK = masterslaveuniqueness(firstCanvas);
            if (!masterslaveuniquenessOK) return false;
            else return true;
        }

        //This function is called by checkmasterslave
        //Since we support master slave listboxes, we need to handle the following scenarios
        //1. The case that a master listbox points to a slave that does not exist or was deleted
        //2. Master listbox points to an empty slave, this is not fine and we will throw an error 
        private bool checkvalidslave(BSkyCanvas canvas)
        {
            bool isvalidchild = true;
            foreach (Object obj in canvas.Children)
            {
                if (obj is BSkyMasterListBox)
                {
                    BSkyMasterListBox mlb = obj as BSkyMasterListBox;
                    isvalidchild = mlb.checkIfValidChild();
                    if (isvalidchild == false)
                    {
                        string message = "The Master ListBox " + mlb.Name + " has an invalid value" + mlb.SlaveListBoxName + " set for the slave listbox name ";
                        MessageBox.Show(message);

                        return isvalidchild;
                    }
                }
                if (obj is BSkyButton)
                {
                    FrameworkElement fe = obj as FrameworkElement;
                    BSkyCanvas cs = fe.Resources["dlg"] as BSkyCanvas;
                    if (cs != null) { isvalidchild = checkvalidslave(cs); }
                }

            }
            return isvalidchild;
        }


        //3. One or more master listboxes point to the same slave
        //as we have alreadh ensured that master can point to a slave on the same canvas ONLY, there is no need to build the
        //list of child slave names (validchild below) asross recursive invocations
        private bool masterslaveuniqueness(BSkyCanvas canvas)
        {
            bool masterslaveuniquenessOK = true;
            List<string> validchild = new List<string>();
            foreach (Object obj in canvas.Children)
            {
                if (obj is BSkyMasterListBox)
                {
                    BSkyMasterListBox mlb = obj as BSkyMasterListBox;
                    if (mlb.SlaveListBoxName != null)
                    {
                        if (validchild.Contains(mlb.SlaveListBoxName))
                        {
                            string message = "The Master ListBox " + mlb.Name + " has a slave" + mlb.SlaveListBoxName + " that is referenced by another master listbox. A slave listbox must be referenced by a unique master listbox ";
                            MessageBox.Show(message);
                            masterslaveuniquenessOK = false;
                            return masterslaveuniquenessOK;
                        }
                        else validchild.Add(mlb.SlaveListBoxName);
                    }
                }
                if (obj is BSkyButton)
                {
                    FrameworkElement fe = obj as FrameworkElement;
                    BSkyCanvas cs = fe.Resources["dlg"] as BSkyCanvas;
                    if (cs != null) { masterslaveuniquenessOK = masterslaveuniqueness(cs); }
                }
            }
            return masterslaveuniquenessOK;
        }

        private int checkMissingOutputFile(BSkyCanvas firstCanvas)
        {

            // MessageBox.Show("Output Definition is not defined");
            //             System.Windows.Forms.DialogResult result = System.Windows.Forms.MessageBox.Show(@"You have not specified a output definition file
            //            to control the formatting of the results of the analytical function. /n To, specify a file, select yes /n To continue without 
            //            specifying a file, select no /n To cancel, select cancel ", "Warning", System.Windows.Forms.MessageBoxButtons.YesNoCancel, System.Windows.Forms.MessageBoxIcon.Question);
            //             if (result == System.Windows.Forms.DialogResult.Yes)//save

            // Aaron 05/04/2013
            //We return 1, when youwant to cancel the operation and enter a output definition file
            //We return 2 when the user has selected to continue, i.e. an output definition file will not be specified
            //We return 3 when the user has specified an invalid path to the output definition file
            //We return 4 when the output definition file is valid

            if (firstCanvas.OutputDefinition == null || firstCanvas.OutputDefinition == "")
            {
                Form1 message = new Form1();
                System.Windows.Forms.DialogResult result = message.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.Cancel)//save 
                {
                    return 1;
                }
                else if (result == System.Windows.Forms.DialogResult.OK)//save
                {
                    return 2;
                }

            }
            //The condition checks whether a valid output definition file exists
            if (!File.Exists(firstCanvas.OutputDefinition))
            {
                //MessageBox
                System.Windows.Forms.MessageBox.Show("The Output Definition File cannot be accessed. Check the path and file name");
                return 3;
            }
            return 4;
        }


        //Aaron 04/03/2013
        // Code below, gets the row of the property grid. Now a property grid will have entries for all the objects placed onthe property grid
        //In our case, there is a single object of type BSkyCanvas , since we are dealing with the CanvasPropertyGrid. 
        //So the hierarchy is as follows
        //Top level object i.e. BSKYCanvas
        //THen there is a collection of categories (BlueSKY)
        //Within each category is a collection of objects/properties (THere are the properties with caregory BlueSky

        private System.Windows.Forms.GridItem GetSelectedGridItem(String fieldName, System.Windows.Forms.PropertyGrid propertyGrid)
        {
            try
            {
                System.Windows.Forms.GridItem rootGridItem = null;
                System.Windows.Forms.GridItem gridItem = propertyGrid.SelectedGridItem;
                if (gridItem != null)
                {
                    while (gridItem.Parent != null)
                    {
                        gridItem = gridItem.Parent;
                    }

                    foreach (System.Windows.Forms.GridItem childGridItem in gridItem.GridItems)
                    {
                        //Aaron 04/06/2013 Here are the code that handles all the categories within an object e.g. BSkyCanvas 
                        while (rootGridItem == null && childGridItem.GridItems.Count > 0)
                        {
                            rootGridItem = GetGridItem(childGridItem.GridItems, fieldName);
                        }
                    }
                }
                return rootGridItem;
            }
            catch (Exception Ex)
            {
                throw Ex;
            }

        }


        public void showHelp()
        {

            bool helper = true;//show help
            string helppopup = "true";
            //ANIL:this helppopup will be null if someone launches DialogDesiner just after finishing install without launching BlueSky main app.
            //When the main app is launched the config file gets copied to user location(if it as not there arleady) and so following 
            //setting can be read afterwards.
            //We can fix this by doing exactly what we do in main app to copy config. OR
            //I have following code modified that if helppopup is null we show the help dialog. This is not so critical in my opinion.
            if (confService.AppSettings != null)
            {
                helppopup = confService.AppSettings.Get("helppopup");
            }
            else
            {
                helppopup = confService.DefaultSettings["helppopup"];
            }
            // load default value if no value is set 
            if (helppopup != null)
            {
                if (helppopup.Trim().Length == 0)//|| !IsValidFullPathFilename(source))
                    helppopup = confService.DefaultSettings["helppopup"];
                helper = helppopup.Equals("true") ? true : false; /// for testing purpose only. Make it false while testing.
            }
            //  confService.ModifyConfig("helppopup", "anil");
            if (helper)
            {
                    dialogHelp message = new dialogHelp();
                    message.ShowDialog();
            }
            
            //  System.Windows.Forms.DialogResult result = message.ShowDialog();
        }

        //Method called by above method

        //Aaron 04/06/2013
        //We are at the category level here. We always start at the object level which is BSKYCanvas, we then enumerate through the
        //objects within the category looking for the name.
        //Basically we are at the collection level

        private System.Windows.Forms.GridItem GetGridItem(System.Windows.Forms.GridItemCollection gridItemCollection, String fieldName)
        {
            System.Windows.Forms.GridItem rootGridItem = null;
            try
            {
                foreach (System.Windows.Forms.GridItem gridItem in gridItemCollection)
                {
                    if (rootGridItem == null)
                    {
                        if (gridItem.Label == fieldName)
                        {
                            rootGridItem = gridItem;
                        }
                    }
                }
                return rootGridItem;
            }
            catch (Exception Ex)
            {
                throw Ex;
            }
        }


        void SavePackbinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            fileSave(false);
        }



        //Generates the preview with var1, var2 and var3
        private string GetXamlforpreview()
        {
            string xaml = string.Empty;
            try
            {
                xaml = XamlWriter.Save(myCanvas);
                // xaml =xaml.Replace("ItemTemplate=\"{assembly:Null}\"","");
                xaml = xaml.Replace("ItemTemplate=\"{assembly:Null}\"", "");
                xaml = xaml.Replace("ItemTemplate=\"{x:Null}\"", "");
                xaml = xaml.Replace("<assembly:Null />","");
                xaml = removeTags(xaml, "<BSkyListBox.Resources>", "</BSkyListBox.Resources>");
                
                XamlReader.Parse(xaml);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Cannot Parse xaml for the preview\n" + ex.Message);
                return string.Empty;
            }
            return xaml;
        }




         private string removeTags(string sourceText, string beginTag, string endTag)
        {
            string removePara = string.Empty;
            do
            {
                removePara = string.Empty;
                int startindex = sourceText.IndexOf(beginTag);
                int endindex = sourceText.IndexOf(endTag);
                if (startindex >= 0 && endindex >= 0 && startindex < endindex) //if tag is found
                {
                    removePara = sourceText.Substring(startindex, (endindex - startindex +  endTag.Length)); //get para that needs to be removed
                }
                if (removePara.Length > 0)
                {
                    sourceText = sourceText.Replace(removePara, "");//modify source text by removing the para thats not needed
                }
            } while (sourceText.Contains(beginTag)); //keep removing PARAs till all are gone. Just checking begin tag bcoz its XAML

            return sourceText; // return final string.
        }

        //remove text method
        private string removeTxt(string sourceText, string removetext)
        {
            string removePara = string.Empty;
            do
            {
                removePara = string.Empty;
                int index = sourceText.IndexOf(removetext);
                if (index >= 0) //if tag is found
                {
                    sourceText = sourceText.Replace(removetext, "");//modify source text by removing the para thats not needed
                }
            } while (sourceText.Contains(removetext)); //keep removing TEXT till all are gone.

            return sourceText; // return final string.
        }


    





        private string GetXaml()
        {
            string xaml = string.Empty;
            int count = 0;
            try
            {
                xaml = XamlWriter.Save(copy);
                // xaml = XamlWriter.Save(myCanvas);
                // xaml =xaml.Replace("ItemTemplate=\"{assembly:Null}\"","");
                //Aaron 11/25/2012 One of the 2 lines below are redundant and need to be removed
                xaml = xaml.Replace("ItemTemplate=\"{assembly:Null}\"", "");
                xaml = xaml.Replace("ItemTemplate=\"{x:Null}\"", "");
                // xaml = xaml.Replace("assembly:Key=\"{assembly:Type av:Border}\"", "av:Key=\"{av:Type av:Border}\"");
                //Aaron 11/14/2013
                //The code below adds new canvases to BSKyCanvas.chainOpencanvas

                //Aaron 05/12/2014
                //There is an issue with the line of code below, this line of code invokes the OnTextChanged event
                //this happens as follows, you have a text box with a value ="xsxs", you define a rule saying that 
                //if the textbox contains a valid string, then set another property in a control in another canvas to false.
                //In the parse call we are triggering the ontextchanged event when the value of the text box is set to "xsxs"
                //This in turn calls parent.ApplyBehaviour(this, TextChangedBehaviour);
                //Now the function call fe = this.FindName(setter.ControlName); in the parent.ApplyBehaviour(this, TextChangedBehaviour);
                //fails
                //To reproduce the problem, uncomment the above line and try and save the dialog definition test103 in c:\aaron\dialog editor\special folder
                //XamlReader.Parse(xaml);
                //BSkyCanvas.chainOpenCanvas[0].Children.RemoveRange(0, count);
                //Checking the chain of open canvases. THE OBVIOUS THOUGHT IS THIS SHOULD BE 1, however gencopy and XamlReader.Parse(xaml);
                //create additional copies that I don't need
                count = BSkyCanvas.chainOpenCanvas.Count;
                if (count > 1) BSkyCanvas.chainOpenCanvas.RemoveRange(1, count - 1);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Cannot Parse xaml\n" + ex.Message);
                return string.Empty;
            }
            return xaml;
        }



        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.MouseLeftButtonDown += new MouseButtonEventHandler(Window1_MouseLeftButtonDown);
            this.MouseLeftButtonUp += new MouseButtonEventHandler(DragFinishedMouseHandler);
            this.MouseMove += new MouseEventHandler(Window1_MouseMove);
            this.MouseLeave += new MouseEventHandler(Window1_MouseLeave);
            //    this.KeyDown += new KeyEventHandler(Window1_KeyDown);

            if (myCanvas != null)
            {
                myCanvas.Focusable = true;
                myCanvas.KeyUp += new KeyEventHandler(myCanvas_KeyUp);
                myCanvas.PreviewKeyUp += new KeyEventHandler(myCanvas_PreviewKeyUp);
                myCanvas.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(myCanvas_PreviewMouseLeftButtonDown);
                myCanvas.PreviewMouseLeftButtonUp += new MouseButtonEventHandler(DragFinishedMouseHandler);
            }
        }

        void myCanvas_PreviewKeyUp(object sender, KeyEventArgs e)
        {

        }


        void Window1_KeyDown(object sender, KeyEventArgs e)
        {
            
        }



        //01/05/2013 Added by Aaron
        //This code gets invoked when pressing the delete key from the main canvas

        void myCanvas_KeyUp(object sender, KeyEventArgs e)
        {
            //// Aaron 01/03/2013
            ////  The code below was the original code that I commented
            // if (e.Key == Key.Delete && selectedElement != null)
            // {
            //     myCanvas.Children.Remove(selectedElement);
            //     selectedElement = null;
            //     OptionsPropertyGrid.SelectedObject = null;
            // }

            if (e.Key == Key.Delete && selectedElement != null)
            {
                if (selectedElement.GetType().Name == "BSkyRadioButton")
                {
                    DependencyObject parentObject = VisualTreeHelper.GetParent(selectedElement);
                    //Aaron 01/03/2013
                    //Not sure why the code would ever reach here
                    if (parentObject == null)
                    {
                        myCanvas.Children.Remove(selectedElement);
                        selectedElement = null;
                        //Aaron 10/29/2013
                        //added line below
                        selectedElementRef = null;
                        OptionsPropertyGrid.SelectedObject = null;
                        //Aaron 01/26/2013, setting saved to false as the control is removed 
                        saved = false;
                        return;
                    }

                    // Aaron 01/03/2013
                    //This is the case of the Radiogroup that is created from the dialog 
                    StackPanel parent = parentObject as StackPanel;
                    //The case below is for when the radio button is placed directly on the Canvas
                    if (parent == null)
                    {
                        myCanvas.Children.Remove(selectedElement);
                        selectedElement = null;
                        //Aaron 10/29/2013
                        //added line below
                        selectedElementRef = null;
                        OptionsPropertyGrid.SelectedObject = null;
                        //Aaron 01/26/2013, setting saved to false as the control is removed 
                        saved = false;
                        return;
                    }


                    //Code below is executed when the radio button is placed on a stackpanel
                    BSkyRadioButton radioButton = selectedElement as BSkyRadioButton;
                    string parentName = radioButton.GroupName;
                    // DependencyObject parentObject1 = VisualTreeHelper.GetParent(parent);

                    //BSkyRadioGroup radiogrp1 = parentObject1 as BSkyRadioGroup;
                    // We are looking for the parent RadioGroup that contains this radiobutton as we know that the radiobutton was placed on a stackpanel
                    BSkyRadioGroup element = UIHelper.FindVisualParent<BSkyRadioGroup>(selectedElement);
                    // FrameworkElement fe = element as FrameworkElement;

                    // FrameworkElement radiogrp = myCanvas.FindName(parentName) as FrameworkElement;
                    BSkyRadioGroup radiogrp1 = element as BSkyRadioGroup;

                    //Parent is a stackpanel
                    if (parent != null)
                    {
                        parent.Children.Remove(selectedElement);
                        //Aaron 01/26/2013, setting saved to false as the control is removed 
                        saved = false;
                        selectedElement = null;
                        //Aaron 10/29/2013
                        //added line below
                        selectedElementRef = null;
                        OptionsPropertyGrid.SelectedObject = null;
                        // radiogrp1.RadioButtons.Remove(radioButton);
                    }

                    return;
                } //End of if ==BSkyRadioButton
                else
                {
                    myCanvas.Children.Remove(selectedElement);
                    selectedElement = null;
                    OptionsPropertyGrid.SelectedObject = null;
                    //Aaron 01/26/2013, setting saved to false as the control is removed 
                    saved = false;
                    return;
                }
            } //End of if delete key is pressed
        }

        void myCanvas_KeyDown(object sender, KeyEventArgs e)
        {

        }

        // Handler for drag stopping on leaving the window
        void Window1_MouseLeave(object sender, MouseEventArgs e)
        {
            StopDragging();
            e.Handled = true;
        }

        // Handler for drag stopping on user choise
        void DragFinishedMouseHandler(object sender, MouseButtonEventArgs e)
        {
            StopDragging();
            e.Handled = true;
        }

        // Method for stopping dragging
        private void StopDragging()
        {
            if (_isDown)
            {
                _isDown = false;
                _isDragging = false;
            }
        }

        // Hanler for providing drag operation with selected element

        // 03/16/2013
        //Code comes here and selectedElement is null
        void Window1_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isDown)
            {
                //03/16/2013
                //Added this code to prevent crash when moving and no item is selected
                if (selectedElement == null) return;

                if ((_isDragging == false) &&
                    ((Math.Abs(e.GetPosition(myCanvas).X - _startPoint.X) > SystemParameters.MinimumHorizontalDragDistance) ||
                    (Math.Abs(e.GetPosition(myCanvas).Y - _startPoint.Y) > SystemParameters.MinimumVerticalDragDistance)))
                    _isDragging = true;

                if (_isDragging)
                {
                    Point position = Mouse.GetPosition(myCanvas);
                    double top = position.Y - (_startPoint.Y - _originalTop);
                    double left = position.X - (_startPoint.X - _originalLeft);

                    if (top < 0 || left < 0 || left + selectedElement.ActualWidth > myCanvas.ActualWidth
                        || top + selectedElement.ActualHeight > myCanvas.ActualHeight)
                    {
                        return;
                    }

                    //01/26/2013
                    //setting saved ==false if I moved a control
                    saved = false;
                    BSkyCanvas.SetTop(selectedElement, top);
                    BSkyCanvas.SetLeft(selectedElement, left);
                }
            }
        }

        // Handler for clearing element selection, adorner removal

        void Window1_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (selected)
            {
                selected = false;
                if (selectedElement != null)
                {
                    aLayer.Remove(aLayer.GetAdorners(selectedElement)[0]);
                    selectedElement = null;
                    //Aaron 10/29/2013
                    //added line below
                    selectedElementRef = null;
                    DialogDesign.IsEnabled = false;
                }
            }
            //Added by Aaron 10/23/2013
            //This ensures where there is nothing selected on the canvas, the optionsproperty grid is set to blank
            //This prevents me from inadvertently setting properties on the property grid when there is no corresponding selected item
            //I made this change in the code because we are applying the filter on the variable list in dialog editor mode as soon as 
            //the user sets the filter. However if there is no selected variable list which can happen if I click on the canvas after 
            //selecting the variable list, I encounter a crash. The code that applies the filter on the variable list, looks for the selected object
            else OptionsPropertyGrid.SelectedObject = null;
        }

        // Handler for element selection on the canvas providing resizing adorner
        void myCanvas_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Remove selection on clicking anywhere the window
            if (selected)
            {
                selected = false;
                if (selectedElement != null)
                {
                    // Remove the adorner from the selected element
                    //Added Aaron 11/17/2012
                    if (aLayer.GetAdorners(selectedElement) != null)
                        aLayer.Remove(aLayer.GetAdorners(selectedElement)[0]);
                    selectedElement = null;
                    //Aaron 10/29/2013
                    //added line below
                    selectedElementRef = null;
                    DialogDesign.IsEnabled = false;
                }
            }




            //Aaron added 02/04/2012
            //Case for handling the click within the groupbox and RadioGropBox
            //When clicking within a GroupBox, eSource is equal to myCanvas
            //The way the algorithm works is I check the mouse position and see whether the mouse is 
            //within a groupbox, I then select the groupbox.
            //If it is not within the Groupbox, we assume the user has selected the Canvas and return
            if (e.Source == myCanvas)
            {
                _isDown = true;
                _startPoint = e.GetPosition(myCanvas);
                Point mouseRelToGroupBox;

                //Added by Aaron 02/05/2013
                //Algorithm for finding out whether the Mouse is on the Groupbox.
                //Loop through the objects on the canvas, Looking for Groupboxes on the canvas.
                //Check the starting coordinates of the GroupBox on the canvas, get the width and height of the canvas


                foreach (UIElement child in myCanvas.Children)
                {
                    if (child.GetType().Name == "BSkyGroupBox" || child.GetType().Name == "BSkyRadioGroup")
                    {
                        FrameworkElement source = e.Source as FrameworkElement;
                        //  BSkyGroupBox b = child as BSkyGroupBox;
                        FrameworkElement b = child as FrameworkElement;
                        mouseRelToGroupBox = e.GetPosition(b);
                        // mouseRelToGroupBox = e.GetPosition(b);
                        // if (mouseRelToGroupBox.X > 1 && mouseRelToGroupBox.X < source.Width)
                        if (mouseRelToGroupBox.X > 1 && mouseRelToGroupBox.X < b.Width)
                        {
                            if (mouseRelToGroupBox.Y > 1 && mouseRelToGroupBox.Y < b.Height)
                            {
                                // selectedElement = source;
                                //selectedElementRef = source;
                                selectedElement = b;
                                selectedElementRef = b;
                            }
                        }
                    }


                }
                //Added by Aaron 02/05/2013
                //This is the case that the canvas is clicked and you have not clicked within a GroupBox
                if (selectedElement == null)
                {
                   
                    //Addedby Aaron 08/31/2014
                    //When you click on a control, you see the properties of that control on the lhs optionsgrid
                    //However when you click on the canvas, the ghost of the last control you clicked on remains 
                    //on the options grid. 
                    //You can click on the name property on the ghost grid and enter a name, the duplication detection
                    //fails.Hence it makes sense to clear the optionsgrid when clicking the canvas
                    OptionsPropertyGrid.SelectedObject = null;
                    return;
                }

                if (selectedElement is BSkyButton)
                    DialogDesign.IsEnabled = true;

                OptionsPropertyGrid.SelectedObject = selectedElement;
                myCanvas.Focus();

                //c1PropertyGrid1.SelectedObject = selectedElement;
                //c1PropertyGrid1.PropertyBoxesLoaded += new EventHandler(c1PropertyGrid1_PropertyBoxesLoaded);
                _originalLeft = BSkyCanvas.GetLeft(selectedElement);
                _originalTop = BSkyCanvas.GetTop(selectedElement);

                aLayer = AdornerLayer.GetAdornerLayer(selectedElement);
                ResizingAdorner ad = new ResizingAdorner(selectedElement);
                ad.KeyDown += new KeyEventHandler(ad_KeyDown);
                aLayer.Add(ad);
                aLayer.KeyDown += new KeyEventHandler(aLayer_KeyDown);
                selected = true;
                e.Handled = true;
            }
            // If any element except canvas is clicked, 
            // assign the selected element and add the adorner

            //Aaron added 12/16/2013
            //The code below loops through starting fromthe object the mouse is over, moving higher in the hierarchy looking for a BSky 
            //object. The BSky object is returned as the object that is selected and has the adorners
            //The only exception to the rule is BSkyTextBox that can be within BSkyScrollTextBox i.e. one BSky object that
            //canappear by itself in a canvas inside another BSky object (scrollable textbox)

            if (e.Source != myCanvas)
            {
                _isDown = true;
                _startPoint = e.GetPosition(myCanvas);
                FrameworkElement source = e.Source as FrameworkElement;
                //Added by Aaron 06/16/2015
                //When clicking within the grid for compute, we sometimes get a windows run object
                //in this case source cannot be converted to a FrameworkElement, hence we return
                if (source == null) return;
                if (!e.Source.GetType().Name.ToLower().StartsWith("bsky"))
                {
                    FrameworkElement temp = VisualTreeHelper.GetParent(source) as FrameworkElement;
                    while (temp != null && !temp.GetType().Name.ToLower().StartsWith("bsky"))
                    {
                        temp = VisualTreeHelper.GetParent(temp) as FrameworkElement;
                    }
                    if (temp != null)
                        source = temp;
                    else
                        return;
                }
                //Added by Aaron on 12/16/2013
                //
                //Code below checks whether the textbox is within a Scrollviewer i.e. this is s scrollable textbox
                //If so, and you click on the textbox, we highlight the scrollable textbox and not the textbox itself that looks awkard
                //if (e.Source.GetType().Name == "BSkyTextBox")
                //{

                //    FrameworkElement temp = LogicalTreeHelper.GetParent(source) as FrameworkElement;
                //    while (temp != null)
                //    {
                //        ScrollViewer refScrollTextBox = temp as ScrollViewer;
                //        if (refScrollTextBox != null)
                //        {
                //            source = refScrollTextBox;
                //            break;
                //        }
                //        else temp = LogicalTreeHelper.GetParent(temp) as FrameworkElement;
                //    }

                //}

                selectedElement = source;
                //Aaron 10/29/2013
                //added line below
                selectedElementRef = source;

                if (selectedElement is BSkyButton)
                    DialogDesign.IsEnabled = true;

                OptionsPropertyGrid.SelectedObject = selectedElement;
                myCanvas.Focus();

                //c1PropertyGrid1.SelectedObject = selectedElement;
                //c1PropertyGrid1.PropertyBoxesLoaded += new EventHandler(c1PropertyGrid1_PropertyBoxesLoaded);
                _originalLeft = BSkyCanvas.GetLeft(selectedElement);
                _originalTop = BSkyCanvas.GetTop(selectedElement);

                aLayer = AdornerLayer.GetAdornerLayer(selectedElement);
                ResizingAdorner ad = new ResizingAdorner(selectedElement);
                ad.KeyDown += new KeyEventHandler(ad_KeyDown);
                aLayer.Add(ad);
                aLayer.KeyDown += new KeyEventHandler(aLayer_KeyDown);
                selected = true;
                e.Handled = true;
            }
        }

        void ad_KeyDown(object sender, KeyEventArgs e)
        {

        }

        void aLayer_KeyDown(object sender, KeyEventArgs e)
        {

        }

        void c1PropertyGrid1_PropertyBoxesLoaded(object sender, EventArgs e)
        {

        }

        //01/05/2013 Added by Aaron
        //This code gets invoked when pressing the delete key from the canvas created from a button

        void b_KeyDown(object sender, KeyEventArgs e)
        {
            //if (e.Key == Key.Delete && selectedElement != null)
            //{
            //    myCanvas.Children.Remove(selectedElement);
            //    selectedElement = null;
            //    OptionsPropertyGrid.SelectedObject = null;
            //}

            if (e.Key == Key.Delete && selectedElement != null)
            {
                if (selectedElement.GetType().Name == "BSkyRadioButton")
                {
                    DependencyObject parentObject = VisualTreeHelper.GetParent(selectedElement);
                    // Aaron 01/03/2013
                    //Not sure why the code would ever reach here
                    if (parentObject == null)
                    {
                        myCanvas.Children.Remove(selectedElement);
                        selectedElement = null;
                        //Aaron 10/29/2013
                        //added line below
                        selectedElementRef = null;
                        OptionsPropertyGrid.SelectedObject = null;

                        //Aaron 01/26/2013, setting saved =false as a control is removed
                        saved = false;
                        return;
                    }

                    // Aaron 01/03/2013
                    //This is the case of the Radiogroup that is created from the dialog 
                    StackPanel parent = parentObject as StackPanel;
                    //The case below is for when the radio button is placed directly on the Canvas
                    if (parent == null)
                    {
                        myCanvas.Children.Remove(selectedElement);
                        selectedElement = null;
                        OptionsPropertyGrid.SelectedObject = null;
                        //Aaron 01/26/2013, setting saved =false as a control is removed
                        saved = false;
                        return;
                    }


                    //Code below is executed when the radio button is placed on a stackpanel
                    BSkyRadioButton radioButton = selectedElement as BSkyRadioButton;
                    string parentName = radioButton.GroupName;
                    // DependencyObject parentObject1 = VisualTreeHelper.GetParent(parent);

                    //BSkyRadioGroup radiogrp1 = parentObject1 as BSkyRadioGroup;
                    // We are looking for the parent RadioGroup that contains this radiobutton as we know that the radiobutton was placed on a stackpanel
                    BSkyRadioGroup element = UIHelper.FindVisualParent<BSkyRadioGroup>(selectedElement);
                    // FrameworkElement fe = element as FrameworkElement;

                    // FrameworkElement radiogrp = myCanvas.FindName(parentName) as FrameworkElement;
                    BSkyRadioGroup radiogrp1 = element as BSkyRadioGroup;

                    //Parent is a stackpanel
                    if (parent != null)
                    {
                        //Aaron 01/26/2013, setting saved =false as a control is removed
                        parent.Children.Remove(selectedElement);
                        selectedElement = null;
                        OptionsPropertyGrid.SelectedObject = null;

                        //  radiogrp1.RadioButtons.Remove(radioButton);
                        saved = false;
                    }

                    return;
                } //End of if ==BSkyRadioButton

                //else if (selectedElement.GetType().Name == "BSkySourceList" || selectedElement.GetType().Name == "BSkyGroupingVariable")
                //{
                //    DragDropList selVarlist =selectedElement as DragDropList;
                //    foreach (Object obj in myCanvas.Children)
                //    {
                //        if (obj.GetType().Name=="BSkyVariableMoveButton")
                //        {
                //            BSkyVariableMoveButton objMove = obj as BSkyVariableMoveButton;
                //            if (objMove.InputList == selVarlist.Name)
                //            {
                //                objMove.inputListName = "";
                //                objMove.vInputList = null;
                //            }
                //            if (objMove.TargetList == selVarlist.Name)
                //            {
                //                objMove.targetListName = "";
                //                objMove.vTargetList = null;
                //            }
                //        }
                //    }
                //    myCanvas.Children.Remove(selectedElement);
                //    selectedElement = null;
                //    OptionsPropertyGrid.SelectedObject = null;
                //    saved = false;
                //    return;

                //}
                else
                {
                    //Aaron 01/26/2013, setting saved =false as a control is removed
                    myCanvas.Children.Remove(selectedElement);
                    selectedElement = null;
                    OptionsPropertyGrid.SelectedObject = null;
                    saved = false;
                    return;
                }
            } //End of if delete key is pressed
        }


        private void groupBox_Click(object sender, System.EventArgs e)
        {
            MessageBox.Show("The click event!");
        }



        private void GrpBox_Click(object sender, RoutedEventArgs e)
        {
            Button btnSender = sender as Button;
            System.Windows.Forms.GridItem nameGridItem;
            FrameworkElement b = BSkyControlFactory.Instance.CreateControl(btnSender.Name);
            //if (btnSender.Name == "GrpBox")
            //{
            //    BSkyGroupBox tmpBSkyGrpbox = b as BSkyGroupBox;
            //    tmpBSkyGrpbox.Click += new System.EventHandler(groupBox_Click);
            //}
            b.KeyDown += new KeyEventHandler(b_KeyDown);
            myCanvas.Children.Add(b);
            //01/20/2013
            //Added by Aaron to track the fact that a control has been added and hence saved = false;
            saved = false;

            BSkyCanvas.SetTop(b, initialPosition);
            BSkyCanvas.SetLeft(b, initialPosition);
            initialPosition = initialPosition + 5;
            // this.selectedElement = b;


            // Aaron 04/28/2013
            //Added code below to remove the adorners on a previously selected item as we are applying the adorners on the 
            //item most recently added to canvas
            if (selected)
            {
                selected = false;
                if (selectedElement != null)
                {
                    // Remove the adorner from the selected element
                    //Added Aaron 11/17/2012
                    if (aLayer.GetAdorners(selectedElement) != null)
                        aLayer.Remove(aLayer.GetAdorners(selectedElement)[0]);
                    selectedElement = null;
                    DialogDesign.IsEnabled = false;
                }
            }

            //Aaron 04/28/2013
            //Line below is added so that when I press the delete key, the canvas is in focus and the event can be handled by my canvas_key up
            myCanvas.Focus();

            OptionsPropertyGrid.SelectedObject = b;
            OptionsPropertyGrid.Refresh();
            selectedElement = b;
            //Aaron
            //Added on 10/29/2013
            //Stored a reference to the selected element that will be used in the selectionchangebehavior to validate the properties being set
            selectedElementRef = b;
            _originalLeft = BSkyCanvas.GetLeft(selectedElement);
            _originalTop = BSkyCanvas.GetTop(selectedElement);
            aLayer = AdornerLayer.GetAdornerLayer(selectedElement);
            ResizingAdorner ad = new ResizingAdorner(selectedElement);
            ad.KeyDown += new KeyEventHandler(ad_KeyDown);
            aLayer.Add(ad);
            aLayer.KeyDown += new KeyEventHandler(aLayer_KeyDown);
            selected = true;

            //Aaron 04/28/2013
            //Code below selects the name property of all the objects
            //Aaron 01/12/2014
            //Commented the line below as focus is on the object added to the canvas
            //Even when the name is selected in the property grid,it is barely noticable as the optionsproperty grid does not have focus, also when the options property grid is clicked on, the item you clicked on gets focus
            //nameGridItem = GetSelectedGridItem("Name", OptionsPropertyGrid);
            // OptionsPropertyGrid.Focus();

            //nameGridItem.Select();
            return;
        }



        void InspectBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }



        //void InspectBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        //{
        //    InspectDialog window = new InspectDialog();
        //    System.Windows.Forms.DialogResult result;
        //    System.Windows.Forms.OpenFileDialog dialog = new System.Windows.Forms.OpenFileDialog();
        //    dialog.Filter = "Xaml Document (*.bsky)|*.bsky";
        //    result = dialog.ShowDialog();
        //    object obj;

        //    if (result == System.Windows.Forms.DialogResult.OK)
        //    {
        //        string xamlfile = string.Empty, outputfile = string.Empty;
        //        cf.SaveToLocation(@dialog.FileName, Path.GetTempPath(), out xamlfile, out outputfile);
        //        if (!string.IsNullOrEmpty(xamlfile))
        //        {
        //            FileStream stream = File.Open(Path.Combine(Path.GetTempPath(), Path.GetFileName(xamlfile)), FileMode.Open);

        //            //FileStream stream = File.Open(Path.Combine(Path.GetTempPath(), xamlfile,FileMode.Open);
        //            try
        //            {
        //                //Added by Aaron 11/24/2013
        //                //The line below is important as when the dialog editor is launched for some reason and I DONT know why
        //                //BSKYCanvas is invoked from the controlfactor and hence dialogMode is true, we need to reset
        //                BSkyCanvas.dialogMode = false;
        //                obj = XamlReader.Load(stream);
        //                //Added by Aaron 04/13/2014
        //                //There are 3 conditions here, when I launch dialog editor
        //                //1. The first thing I do is open an existing dialog. Chainopencanvas.count=0
        //                //2. I have an existing dialog open and I click open again.  Chainopencanvas.count=2. The first dialog is empty as I clean the existing canvas and open a new one
        //                //3. I hit new the first time I open dialogeditor and then I hit open. Again,Chainopencanvas.count=2. The first dialog is empty as I clean the existing canvas and open a new one 

        //                //The chainopencanvas at this point has 2 entries, one with an empty canvas as we have removed all the children
        //                //the other with the dialog we just opened
        //                //we remove the empty one
        //                if (BSkyCanvas.chainOpenCanvas.Count > 1)
        //                    BSkyCanvas.chainOpenCanvas.RemoveAt(0);
        //                // 11/18/2012, code inserted by Aaron to display the predetermined variable list
        //                BSkyCanvas canvasobj;
        //                canvasobj = obj as BSkyCanvas;

        //                //Added by Aaron 12/08/2013
        //                //if (canvasobj.Command == true) c1ToolbarStrip1.IsEnabled = false;
        //                //else c1ToolbarStrip1.IsEnabled = true;

        //                //Added by Aaron 11/21/2013
        //                //Added line below to enable checking of properties in behaviour.cs i.e. propertyname and control name in dialog 
        //                //editor mode to make sure that the property names are generated correctly
        //                //  BSkyCanvas.dialogMode = true;


        //                //Added 04/07/2013
        //                //The properties like outputdefinition are not set for firstCanvas 
        //                //The line below sets it
        //                firstCanvas = canvasobj;
        //                foreach (UIElement child in canvasobj.Children)
        //                {
        //                    // Code below has to be written as we have saved BSKYvariable list with rendervars=False.
        //                    //BSkySourceList has already been created with the default constructore and we need to 
        //                    //do some work to point the itemsource properties to the dummy variables we create
        //                    if (child.GetType().Name == "BSkySourceList")
        //                    {
        //                        List<DataSourceVariable> preview = new List<DataSourceVariable>();
        //                        DataSourceVariable var1 = new DataSourceVariable();
        //                        var1.Name = "var1";
        //                        var1.DataType = DataColumnTypeEnum.Numeric;
        //                        var1.Width = 4;
        //                        var1.Decimals = 0;
        //                        var1.Label = "var1";
        //                        var1.Alignment = DataColumnAlignmentEnum.Left;
        //                        var1.Measure = DataColumnMeasureEnum.Scale;
        //                        var1.ImgURL = "../Resources/scale.png";
        //                        //  var1.ImgURL = "C:/Users/Aiden/Downloads/Client/libs/BSky.Controls/Resources/scale.png";

        //                        DataSourceVariable var2 = new DataSourceVariable();
        //                        var2.Name = "var2";
        //                        var2.DataType = DataColumnTypeEnum.String;
        //                        var2.Width = 4;
        //                        var2.Decimals = 0;
        //                        var2.Label = "var2";
        //                        var2.Alignment = DataColumnAlignmentEnum.Left;
        //                        var2.Measure = DataColumnMeasureEnum.Nominal; ;
        //                        var2.ImgURL = "../Resources/nominal.png";

        //                        DataSourceVariable var3 = new DataSourceVariable();
        //                        var3.Name = "var3";
        //                        var3.DataType = DataColumnTypeEnum.String;
        //                        var3.Width = 4;
        //                        var3.Decimals = 0;
        //                        var3.Label = "var3";
        //                        var3.Alignment = DataColumnAlignmentEnum.Left;
        //                        var3.Measure = DataColumnMeasureEnum.Ordinal;
        //                        var3.ImgURL = "../Resources/ordinal.png";


        //                        preview.Add(var1);
        //                        preview.Add(var2);
        //                        preview.Add(var3);
        //                        BSkySourceList temp;
        //                        temp = child as BSkySourceList;
        //                        // 12/25/2012 
        //                        //renderVars =TRUE meanswe will render var1, var2 and var3 as listed above. This means that we are in the Dialog designer
        //                        temp.renderVars = true;
        //                        temp.ItemsSource = preview;


        //                    }
        //                    //Added by Aaron 04/06/2014
        //                    //Code below ensures that when the dialog is opened in dialog editor mode, the IsEnabled from
        //                    //the base class is set to true. This ensures that the control can never be disabled in dialog editor mode
        //                    //Once the dialog is saved, the Enabled property is saved to the base ISEnabled property to make sure that
        //                    //the proper state of the dialog is saved
        //                    //if (child is IBSkyEnabledControl)
        //                    //{
        //                    //    IBSkyEnabledControl bskyCtrl = child as IBSkyEnabledControl;
        //                    //    bskyCtrl.Enabled = bskyCtrl.IsEnabled;
        //                    //    bskyCtrl.IsEnabled = true;
        //                    //}
        //                }
        //                subCanvasCount = GetCanvasObjectCount(obj as BSkyCanvas);
        //                BSkyControlFactory.SetCanvasCount(subCanvasCount);
        //               // OpenCanvas(obj);
        //                //Aaron 01/12/2013
        //                //This stores the file name so that we don't have to display the file save dialog on file save and we can save to the file directly;
        //                nameOfFile = @dialog.FileName;

        //                //01/26/2013 Aaron
        //                //Setting the title of the window
        //                Title = nameOfFile;
        //                //Aaron: Commented this on 12/15
        //                // myCanvas.OutputDefinition = Path.Combine(Path.GetTempPath(), outputfile);
        //                //Aaron 01/26/2013
        //                //Making sure saved is set to true as soon as you open the dialog.
        //                // saved = true;
        //                //Aaron
        //                //02/10/2013
        //                //Added code below to ensure that the RoutedUICommand gets fired correctly and the menu items under file and Dialog (top level menu items)
        //                //are properly opened and closed
        //                //   c1ToolbarStrip1.IsEnabled = true;
        //                // CanClose = true;
        //                // CanSave = true;
        //                //this.Focus();
        //               // window.Template = obj as FrameworkElement;

        //                System.Windows.Forms.PropertyGrid CanvasPropertyGrid;
        //               // window.CanvasPropHost = obj as BSkyCanvas;
        //                CanvasPropertyGrid = window.CanvasPropHost.Child as System.Windows.Forms.PropertyGrid;
        //                CanvasPropertyGrid.SelectedObject = obj as BSkyCanvas;
        //                window.CanvasHost.Child = obj as BSkyCanvas;
        //                // Aaron
        //                //03/05/2012
        //                //Making the preview window a fixed size window
        //                if (obj.GetType().Name == "BSkyCanvas") window.ResizeMode = ResizeMode.NoResize;
        //                window.ShowDialog();
        //            }
        //            catch (Exception ex)
        //            {
        //                MessageBox.Show(ex.Message);
        //            }
        //        }
        //        else
        //        {
        //            MessageBox.Show("Cannot open the file");
        //        }


        //        // *******************************************************************************************************
        //        //gencopy();
        //        //Aaron 11/25/2012, the line below needs to be uncommented once the serialization issue is addressed with Anil
        //        // string xaml = GetXamlforpreview();
        //        //string xaml = GetXaml();


        //        //*************************************************************************************************
        //    }
        //}



    }
}