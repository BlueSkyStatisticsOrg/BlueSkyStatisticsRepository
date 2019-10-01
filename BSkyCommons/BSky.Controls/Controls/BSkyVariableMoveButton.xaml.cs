using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows;
using System.ComponentModel;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows.Media.Effects;
using BSky.Interfaces.Controls;
using BSky.Statistics.Common;
using System.Windows.Data;
using System.Linq;
using BSky.Interfaces.Interfaces;
using BSky.Lifetime;

namespace BSky.Controls
{

    public class Item
    {

        public int Id
        {
            get; set;
        }

        public DataSourceVariable Vars
        {
            get; set;
        }

    }

    [TypeConverter(typeof(PropertySorter))]
    [DefaultPropertyAttribute("Type")]
    public partial class BSkyVariableMoveButton : Button, IBSkyControl
    {
        private const string TO_SOURCE = "To Source";
        private const string TO_DEST = "To Dest";

        private BitmapImage imgSource = new BitmapImage();
       Image imageBtn =new Image();
       Grid g = new Grid();
        private BitmapImage imgDest = new BitmapImage();
        bool dialogDesigner = false;

        //public BSkyVariableMoveButton(bool designer):this()
        //{
        //    dialogDesigner = designer;
            
        //}

        public BSkyVariableMoveButton()
        {
            InitializeComponent();

            base.Content = "test";
            imgDest.BeginInit();
            imgDest.UriSource = new Uri(@"pack://application:,,,/BSky.Controls;component/Resources/left.png");
            imgDest.EndInit();

            this.Margin = new Thickness(2);
            imgSource.BeginInit();
            imgSource.UriSource = new Uri(@"pack://application:,,,/BSky.Controls;component/Resources/right.png");
            imgSource.EndInit();

            this.Width = 40;
            this.Height = 40;

            imageBtn.Source = imgDest;

            this.Tag = TO_DEST;

            //Sets the content of the move button to the grid. We add image to the grid
            this.Content = g;
            this.g.Children.Add(imageBtn); 
            this.Resources.MergedDictionaries.Clear();
            this.Click += new RoutedEventHandler(BSkyVariableMoveButton_Click);

            ///Setting shades ///
            this.Effect =
                new DropShadowEffect
                {
                    Color = new Color { R = 155, A = 200, B = 0, G = 95 },
                    Direction = 320,
                    ShadowDepth = 0,
                    Opacity = 1
                };
        }

        public string inputListName;
        private DragDropList vInputList;

        private DragDropListForSummarize vInputDragDropListForSummarize; 
        //Added by Aaron 09/01/2014
        //Holds the source dataset list
        private BSkyListBoxwBorderForDatasets vInputListBoxDatasets;

        
        

        [Description("Copy/Move variable control allows you copy/move variables from a input variable list to a target variable list. The movevariables property on the source variable list controls whether variables get copied or moved. This is a read only property. Click on each property in the grid to see the configuration options for this  control. ")]

        [Category("Control Settings"), PropertyOrder(1)]
        public string Type
        {
            get
            {
                return "Copy/Move Variable Control";
            }
        }



      //  private string moveButtonName;
        public string targetListName;
        private DragDropList vTargetList;
        
        private BSkyTextBox vTargetTextBox;

        private DragDropListForSummarize vTargetDragDropListForSummarize;
        //Added by Aaron 09/01/2014
        //Holds the destination dataset list
        private BSkyListBoxwBorderForDatasets vTargetListBoxDatasets;
        [Category("Control Settings"), PropertyOrder(2)]
        [Description("Required property. You must specify a unique name for every control added to the dialog. You will not be able to save a dialog definition unless every control on the dialog and containing sub-dialogs has a unique name. ")]
        //The line below sets the name of every moce button so that we can uniquely identify each button
        public new string Name
        {
            get { return base.Name; }
            set { base.Name = value; }
        }



        [Category("Control Settings"), PropertyOrder(3)]
        //The move button is associated with a source and destination target list.
        //There can be 2 or more move buttons on a single dialog
        //Basically sets the vInputList property of the move button to the source list
        [Description("This is the variable list that variables will be copied/moved from.")]
        public string InputList
        {
            get
            {
                return inputListName;
            }
            set
            {
                //Added by Aaron 09/01/2014
                //This boolean tells me whether the source dataset, variable or grouping variable list is associated with the proper destination control
                //The rules are as follows
                //a source dataset list must be associated with a destination dataset list or textbox
                //a source variable list must be associated with a destination variable or grouping variable or textbox
                bool validInputtargets = true;
                object obj = GetResource(value);
                if (obj == null || (!(obj is DragDropList) && !(obj is BSkyGroupingVariable) && !(obj is BSkyListBoxwBorderForDatasets) && !(obj is BSkyAggregateCtrl) && !(obj is BSkySortCtrl)))
                {
                    MessageBox.Show("Unable to associate the move button with proper source variable list control");
                    return;
                }
                
                //Added by Aaron 09/01/2014
                //the function below makes sure that the move button is setup with the correct source and destination for a valid move
                //
                validInputtargets=validateInputTarget(value, "", obj.GetType().Name, "");
                if (validInputtargets == false) return;

                //09/14/2013
                //Added by Aaron to support a Grouping variable
                if (obj is DragDropList)
                {
                    inputListName = value;

                    //Added by Aaron 05/29/2014
                    //Commented line below and added line below it
                   // vInputList = obj as BSkyVariableList;
                    vInputList = obj as DragDropList;
                    vInputList.GotFocus += new RoutedEventHandler(vInputList_GotFocus);
                    vInputList.SelectionChanged += new SelectionChangedEventHandler(vInputList_SelectionChanged);
                }

                if (obj is BSkyAggregateCtrl)
                {
                    inputListName = value;

                    BSkyAggregateCtrl objGrid = obj as BSkyAggregateCtrl;
                    foreach (object child1 in objGrid.Children)
                    {
                        if (child1 is DragDropListForSummarize)
                        {
                            vInputDragDropListForSummarize = child1 as DragDropListForSummarize;
                            //inputListName = value;
                        }

                    }
                    
                    vInputList.GotFocus += new RoutedEventHandler(vInputList_GotFocus);
                    vInputList.SelectionChanged += new SelectionChangedEventHandler(vInputList_SelectionChanged);
                }

                if (obj is BSkySortCtrl)
                {
                    inputListName = value;

                    BSkySortCtrl objGrid = obj as BSkySortCtrl;
                    foreach (object child1 in objGrid.Children)
                    {
                        if (child1 is DragDropListForSummarize)
                        {
                            vInputDragDropListForSummarize = child1 as DragDropListForSummarize;
                            //inputListName = value;
                        }

                    }

                    vInputList.GotFocus += new RoutedEventHandler(vInputList_GotFocus);
                    vInputList.SelectionChanged += new SelectionChangedEventHandler(vInputList_SelectionChanged);
                }





                if (obj is BSkyGroupingVariable)
                {
                    BSkyGroupingVariable objtemp =obj as BSkyGroupingVariable;

                    inputListName = objtemp.Name;
                  //  vInputList = objtemp.oneItemList as SingleItemList;

                    vInputList.GotFocus += new RoutedEventHandler(vInputList_GotFocus);
                    vInputList.SelectionChanged += new SelectionChangedEventHandler(vInputList_SelectionChanged);
                }
                if (obj is BSkyListBoxwBorderForDatasets)
                {

                    inputListName = value;
                    vInputListBoxDatasets = obj as BSkyListBoxwBorderForDatasets;
                    vInputListBoxDatasets.GotFocus += new RoutedEventHandler(vInputList_GotFocus);
                    vInputListBoxDatasets.SelectionChanged += new SelectionChangedEventHandler(vInputList_SelectionChanged);
                }
               
             //   vInputList.GotFocus += new RoutedEventHandler(vInputList_GotFocus);
              //  vInputList.SelectionChanged += new SelectionChangedEventHandler(vInputList_SelectionChanged);
            }
        }

        //Added by Aaron 09/01/2014
        //the function below makes sure that the move button is setup with the correct source and destination for a valid move
        //When this function is called, we pass either the source dataset name and the source type or the destination dataset name 
        //and type, what ever is just entered by the user. WE then check if the values entered are correct values for the existing source or destination control that the move button is already associated with. If you enter a source name and the destination name is not entered, we will return true
        

        private bool validateInputTarget(string source, string destination, string sourcetype, string destinationtype)
        {
            //if the source/input string is specified, this is the source control name that the user just entered
            if (source != null && source != string.Empty)
            {
                //Check if there here is a target/destination. if not entered, return true as invalid case is not detected
                if (TargetList != null && TargetList != string.Empty)
                {

                    object obj = GetResource(TargetList);
                    string detectedDestinationType = obj.GetType().Name;
                    if (sourcetype == "BSkySourceList" || sourcetype == "BSkyGroupingVariable" || sourcetype == "BSkyTargetList" || sourcetype == "BSkyAggregateCtrl" || sourcetype == "BSkySortCtrl")
                    {
                        if (detectedDestinationType == "BSkyListBoxwBorderForDatasets")
                        {
                            MessageBox.Show("The input control specified is not a valid control for the destination dataset control. Please specify a valid source dataset control.");
                            return false;
                        }
                    }
                    if (sourcetype == "BSkyListBoxwBorderForDatasets" )
                    {
                        if (detectedDestinationType == "BSkySourceList" || detectedDestinationType == "BSkyGroupingVariable" || detectedDestinationType == "BSkySourceList" || sourcetype == "BSkyAggregateCtrl" || sourcetype == "BSkySortCtrl")
                        {
                            MessageBox.Show("The input control specified is not a valid control for the destination Variable or grouping variable control. Please specify a valid input variable listbox or grouping variable control.");
                            return false;
                        }
                    }
                }
                return true;
            }
            

            //valid target/destination string, this is the value the user just entered
            else if (destination != null && destination != string.Empty)
            {

                //If there is an input list specified
                if (InputList != null && InputList != string.Empty)
                {

                    object obj = GetResource(InputList);
                    string detectedInputListType = obj.GetType().Name;
                    if (destinationtype == "BSkyTargetList" || destinationtype == "BSkySourceList" || destinationtype == "BSkyGroupingVariable" || destinationtype == "BSkyAggregateCtrl" || destinationtype == "BSkySortCtrl")
                    {
                        if (detectedInputListType == "BSkyListBoxwBorderForDatasets")
                        {
                            MessageBox.Show("The destination control specified is not a valid control for the source dataset list control. Please specify a valid destination dataset control.");
                            return false;
                        }
                    }
                    if (destinationtype == "BSkyListBoxwBorderForDatasets")
                    {
                        if (detectedInputListType == "BSkyTargetList" || detectedInputListType == "BSkySourceList" || detectedInputListType == "BSkyGroupingVariable" || destinationtype == "BSkyAggregateCtrl" || destinationtype == "BSkySortCtrl")
                        {
                            MessageBox.Show("The destination control specified is not a valid control for the source variable list control. Please specify a valid destination variable list control.");
                            return false;
                        }
                    }
                }
                return true;
            }
            //Added by Aaron 09/01/2014
            //This is actually an invalid condition
            return true;
        }


//Basically sets the vTargetList property of the move button to the target list
        [Category("Control Settings"), PropertyOrder(4)]
        [Description("This is the variable list that variables will be moved to.")]
        public string TargetList
        {
            get
            {
                return targetListName;
            }
            set
            {

                bool validInputtargets = true;
                object obj = GetResource(value);

                if (obj == null || (!(obj is DragDropList) && !(obj is BSkyGroupingVariable) && !(obj is BSkyTextBox) && !(obj is BSkyListBoxwBorderForDatasets) && !(obj is BSkyAggregateCtrl) && !(obj is BSkySortCtrl)))
                {
                    MessageBox.Show("Unable to associate the move button with proper target variable list controls, target must be of type variable list, grouping variable, dataset list or textbox");
                    return;
                }
                validInputtargets=validateInputTarget("", value, "",obj.GetType().Name);
                if (validInputtargets == false) return;
              

                if (obj is DragDropList)
                {
                    targetListName = value;
                    vTargetList = obj as DragDropList;
                    vTargetList.GotFocus += new RoutedEventHandler(vTargetList_GotFocus);
                    vTargetList.SelectionChanged += new SelectionChangedEventHandler(vTargetList_SelectionChanged);
                }




                if (obj is BSkyGroupingVariable)
                {
                    BSkyGroupingVariable objtemp = obj as BSkyGroupingVariable;
                    foreach (object child1 in objtemp.Children)
                    {
                        if (child1 is SingleItemList)
                        {
                            vTargetList = child1 as DragDropList;
                            targetListName = value;
                        }

                    }

                    vTargetList.GotFocus += new RoutedEventHandler(vTargetList_GotFocus);
                    vTargetList.SelectionChanged += new SelectionChangedEventHandler(vTargetList_SelectionChanged);
                }

                if (obj is BSkyAggregateCtrl)
                {
                    
                    BSkyAggregateCtrl objtemp = obj as BSkyAggregateCtrl;
                    targetListName = value;
                    foreach (object child1 in objtemp.Children)
                    {
                        if (child1 is DragDropListForSummarize)
                        {
                            vTargetDragDropListForSummarize = child1 as DragDropListForSummarize;
                            
                        }

                    }

                    vTargetDragDropListForSummarize.GotFocus += new RoutedEventHandler(vTargetList_GotFocus);
                    vTargetDragDropListForSummarize.SelectionChanged += new SelectionChangedEventHandler(vTargetList_SelectionChanged);
                }


                if (obj is BSkySortCtrl)
                {

                    BSkySortCtrl objtemp = obj as BSkySortCtrl;
                    targetListName = value;
                    foreach (object child1 in objtemp.Children)
                    {
                        if (child1 is DragDropListForSummarize)
                        {
                            vTargetDragDropListForSummarize = child1 as DragDropListForSummarize;

                        }

                    }

                    vTargetDragDropListForSummarize.GotFocus += new RoutedEventHandler(vTargetList_GotFocus);
                    vTargetDragDropListForSummarize.SelectionChanged += new SelectionChangedEventHandler(vTargetList_SelectionChanged);
                }





                if (obj is BSkyTextBox)
                {
                    BSkyTextBox objtemp = obj as BSkyTextBox;
                    vTargetTextBox = objtemp;
                    targetListName = objtemp.Name;
                   
                    //  vInputList = objtemp.oneItemList as SingleItemList;
                    //vInputList.GotFocus += new RoutedEventHandler(vInputList_GotFocus);
                    //vInputList.SelectionChanged += new SelectionChangedEventHandler(vInputList_SelectionChanged);
                }
                if (obj is BSkyListBoxwBorderForDatasets)
                {

                    targetListName = value;
                    vTargetListBoxDatasets = obj as BSkyListBoxwBorderForDatasets;
                    vTargetListBoxDatasets.GotFocus += new RoutedEventHandler(vTargetList_GotFocus);
                    vTargetListBoxDatasets.SelectionChanged += new SelectionChangedEventHandler(vTargetList_SelectionChanged);
                }

            }
        }
        void vTargetList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //if (vTargetList.SelectedItems.Count == 0 && (this.Tag.ToString() == TO_SOURCE))
            //    this.IsEnabled = false;
            //else
            //    this.IsEnabled = true;
        }

        private void SetImage(bool toDest)
        {
            imageBtn = new Image()
            {
                Width = 20,
                Height = 20
            };
            imageBtn.Source = imgSource;
            imageBtn.Stretch = Stretch.Fill;
            Grid theStackPanel = this.Content as Grid;
            theStackPanel.Children.Clear();
            if(toDest)
            {
                imageBtn.Source = imgDest;
            }
            else
            {
                imageBtn.Source = imgSource;
            }
            theStackPanel.Children.Add(imageBtn);
        }



        [Category("Layout Properties"), PropertyOrder(1)]
        [Description("Default value is the width of this control. To change drag the adorners(corner of the control) or enter a width.")]
        public new double Width
        {
            get
            {
                return base.Width;
            }
            set
            {
                base.Width = value;
            }
        }

        [Category("Layout Properties"), PropertyOrder(2)]

        [Description("Default value is the height of this control. To change, drag the adorners(corner of the control) or enter a height.")]
        public new double Height
        {
            get
            {
                return base.Height;
            }
            set
            {
                base.Height = value;
            }
        }

        [Category("Layout Properties"), PropertyOrder(3)]
        [Description("Default value is the X coordinate of the top left corner of this control. To change, drag the control to a different position or enter a X coordinate.")]
        //[Category("Layout Properties")]
        public double Left
        {
            get
            {

                return BSkyCanvas.GetLeft(this);

            }
            set
            {
                BSkyCanvas.SetLeft(this, value);
            }
        }

        [Category("Layout Properties"), PropertyOrder(4)]
        [Description("Default value is the Y coordinate of the top left corner of this control. To change drag the control to a different position or enter a Y coordinate.")]
        //[Category("Layout Properties")]
        public double Top
        {
            get
            {

                return BSkyCanvas.GetTop(this);

            }
            set
            {
                BSkyCanvas.SetTop(this, value);
            }
        }


        void vTargetList_GotFocus(object sender, RoutedEventArgs e)
        {

            //this.Content = Resource.Actions_go_next_icon;
            this.Tag = TO_SOURCE;
            SetImage(false);
            //this.Content = ;
        }

        void vInputList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //if (vInputList.SelectedItems.Count == 0 && (this.Tag.ToString() == TO_DEST))
            //    this.IsEnabled = false;
            //else
            //    this.IsEnabled = true;
        }

        void vInputList_GotFocus(object sender, RoutedEventArgs e)
        {
            this.Tag = TO_DEST;
            SetImage(true);
            //this.Content = Resource.Actions_go_next_icon;
        }

        //Added by Aaron 12/25/2013
        //Called when saving te dialog to check that the source and target of the move button point to valid objects
        //Note: we don't need to check whether they point to valid dragdrop lists or textboxes, this check is done on dialog creation
        //as soon as a user enters a value for a source and destination list for the move

        public FrameworkElement GetResource(string name)
        {
            BSkyCanvas canvas = UIHelper.FindVisualParent<BSkyCanvas>(this);
            foreach (FrameworkElement fe in canvas.Children)
            {
                if (fe.Name == name)
                    return fe;
            }
            return null;
        }

      //Added by Aaron 10/02/2015
        //The function checkIfAggregateOptionExists checks all the items within the target listbox looking to see if the user
        //has already selected the aggregate option
      public  bool checkIfAggregateOptionExists( DragDropListForSummarize target, DataSourceVariable sourceitem, string functionName)
        {
            string aggregateFunction ="";
            int count =0;
            int i=0;
            DataSourceVariable ds=null;
            //bool retval = false;
            aggregateFunction =functionName+"("+ sourceitem.Name +")";
         // target.ite
            count =target.ItemsCount;
            foreach ( object obj  in target.Items)
            {
	            
                
                ds =obj as DataSourceVariable;
	            if (ds.XName ==aggregateFunction) return true;
	            //else return false;	 
            }
            return false;
        }


        private List<string> GetAllVars()        {            IUIController UIController;            UIController = LifetimeService.Instance.Container.Resolve<IUIController>();            List<string> originalvarlist = new List<string>();            DataSource ds = UIController.GetActiveDocument();            List<DataSourceVariable> org = ds.Variables;            foreach (DataSourceVariable dsv in org)            {                originalvarlist.Add(dsv.RName);            }            return originalvarlist;        }



        void BSkyVariableMoveButton_Click(object sender, RoutedEventArgs e)
        {
            int noSelectedItems =0;
            int i = 0;
            int selectionIndex = 0;
            bool filterResults = false;
            System.Windows.Forms.DialogResult diagResult;
            string varList = "";
            string message = "";
            DataSourceVariable o;
            DatasetDisplay datasetdis;
            //Aaron 09/07/2013
            //I had to use a list object as I could not create a variable size array object
            List<object> validVars = new List<object>();
            List<object> invalidVars = new List<object>();
            int firstpos = 0;
            int lastpos = 0;
            bool doesAggOptionExist=false;
            //Added by Aaron 12/24/2013
            //You have the ability to move items to a textbox. When moving items to a textbox you don't have to check for filters
            //All we do is append the items selected separated by + into the textbox
            //We always copy the selected items to the textbox, items are never moved
            //We don't have to worry about tag

            if (vTargetTextBox!=null)
            {
               //Added this line as I may have dragged and droppped items into the destination textbox and thenmay have
                //decided to use the multi-select move control
               // varList = vTargetTextBox.Text;
                //moving from source variable list to textbox
                if (vInputList != null)
                {
                    noSelectedItems = vInputList.SelectedItems.Count;
                    selectionIndex = vTargetTextBox.SelectionStart;
                    if (selectionIndex != 0)
                    {
                        for (i = 0; i < noSelectedItems; i++)
                        {
                            o = vInputList.SelectedItems[i] as DataSourceVariable;
                            //var insertText = "Text";
                            //selectionIndex = vTargetTextBox.SelectionStart;
                            vTargetTextBox.Text = vTargetTextBox.Text.Insert(selectionIndex, o.Name);
                            vTargetTextBox.SelectionStart = selectionIndex + o.Name.Length;//23Feb2017 move the cursor to the end of the text that was just inserted.                            

                            selectionIndex = selectionIndex + o.Name.Length;
                            if ((i + 1) < noSelectedItems)
                            {
                                vTargetTextBox.Text = vTargetTextBox.Text.Insert(selectionIndex, " + ");
                                vTargetTextBox.SelectionStart = selectionIndex + 3;//23Feb2017 move the cursor to the end of the text that was just inserted.                            
                                selectionIndex = selectionIndex + 3;
                            }
                            // varList = varList + o.Name;
                            //  vTargetTextBox.Text = vTargetTextBox.Text + '+';
                        }
                    }
                    else
                    {
                        for (i = 0; i < noSelectedItems; i++)
                        {
                            o = vInputList.SelectedItems[i] as DataSourceVariable;
                            vTargetTextBox.Text = vTargetTextBox.Text + o.Name;
                            vTargetTextBox.SelectionStart = selectionIndex + o.Name.Length;//23Feb2017 move the cursor to the end of the text that was just inserted.                            
                            selectionIndex = selectionIndex + o.Name.Length;//23Feb2017
                            vTargetTextBox.Text = vTargetTextBox.Text + " + ";
                            vTargetTextBox.SelectionStart = selectionIndex + 3;//23Feb2017 move the cursor to the end of the text that was just inserted.                            
                            selectionIndex = selectionIndex + 3;//23Feb2017 " " "

                        }
                    }
                    int current_cursor_postion = vTargetTextBox.SelectionStart;//05Mar2017
                    int len_before_trmming_last = vTargetTextBox.Text.Length;//05Mar2017

                    vTargetTextBox.Text = vTargetTextBox.Text.Trim().TrimEnd('+');

                    int len_after_trimming = vTargetTextBox.Text.Length;//05Mar2017
                    int diff_in_len = len_before_trmming_last - len_after_trimming;//05Mar2017
                    int new_cursor_position = current_cursor_postion - diff_in_len;//05Mar2017
                    vTargetTextBox.SelectionStart = new_cursor_position;//05Mar2017
                   // vTargetTextBox.Text = varList;
                    return;
                }
                //moving from dataset list to textbox
                if (vInputListBoxDatasets != null)
                {
                    noSelectedItems = vInputListBoxDatasets.SelectedItems.Count;
                    for (i = 0; i < noSelectedItems; i++)
                    {
                        datasetdis = vInputListBoxDatasets.SelectedItems[i] as DatasetDisplay;
                        varList = varList + datasetdis.Name;
                        varList = varList + '+';
                    }
                    varList = varList.TrimEnd('+');
                    vTargetTextBox.Text = varList;
                    return;
                }
            }

            //If I am moving to destination, 2 cases, 1 moving from a dataset list, 2 moving from variable list
            if (Tag.ToString() == TO_DEST)
            {
                double maxnoofvars = -1;
              
                ///This is the case of moving to a dataset list
                if(vInputListBoxDatasets !=null)
                {
                    noSelectedItems = vInputListBoxDatasets.SelectedItems.Count;
                    if (vTargetListBoxDatasets.maxNoOfVariables != string.Empty && vTargetListBoxDatasets.maxNoOfVariables != null)
                    {
                        try
                        {
                            maxnoofvars = Convert.ToDouble(vTargetListBoxDatasets.maxNoOfVariables);
                            //Console.WriteLine("Converted '{0}' to {1}.", vTargetList.maxNoOfVariables, maxnoofvars);

                            // diagResult = System.Windows.Forms.MessageBox.Show("An invalid value has been entered for the maximum number of variables in the destination variable list" , "Message", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Question);

                        }
                        catch (FormatException)
                        {
                            diagResult = System.Windows.Forms.MessageBox.Show("An invalid value has been entered for the maximum number of datasets in the target dataset list", "Error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Question);
                        }
                        catch (OverflowException)
                        {
                            diagResult = System.Windows.Forms.MessageBox.Show("An invalid value has been entered for the maximum number of datasets in the target dataset list", "Error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Question);
                        }
                        if (maxnoofvars < (noSelectedItems + vTargetListBoxDatasets.ItemsCount))
                        {
                            //e.Effects = DragDropEffects.None;
                            //e.Handled = true;
                            message = "The target dataset list cannot have more than " + vTargetListBoxDatasets.maxNoOfVariables + "variables. Please reduce your selection or remove datasets from the target dataset list";
                            diagResult = System.Windows.Forms.MessageBox.Show(message, "Error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Question);
                            return;

                        }

                    }
                    for (i = 0; i < noSelectedItems; i++)
                    {
                        //Added by Aaron 08/12/2014
                        //Line below ensures that I move items from teh source to the target only when the items are not in the target
                        //If Item already exists in target I ignore
                        if (!vTargetListBoxDatasets.Items.Contains(vInputListBoxDatasets.SelectedItems[i]))
                        {
                           // filterResults = vTargetList.CheckForFilter(vInputList.SelectedItems[i]);
                           // if (filterResults) validVars.Add(vInputList.SelectedItems[i]);
                          //  else invalidVars.Add(vInputList.SelectedItems[i]);

                            


                            validVars.Add(vInputListBoxDatasets.SelectedItems[i]);
                        }
                    }


                    //If there are valid variables then move them
                    if (validVars.Count != 0)
                    {
                        vTargetListBoxDatasets.AddItems(validVars as List<object>);
                        
                        //The code below unselects everything
                        vTargetListBoxDatasets.UnselectAll();
                        //The code below selects all the items that are moved
                        vTargetListBoxDatasets.SetSelectedItems(validVars as List<object> );
                      
                        //Added by Aaron on 12/24/2012 to get the items moved scrolled into view
                        //Added by Aaron on 12/24/2012. Value is 0 as you want to scroll to the top of the selected items
                        vTargetListBoxDatasets.ScrollIntoView(validVars[0]);
                        if (vInputListBoxDatasets.MoveVariables)
                        //The compiler is not allowing me to use vInputList.Items.Remove() so I have to use ItemsSource
                        {
                            ListCollectionView lcw = vInputListBoxDatasets.ItemsSource as ListCollectionView;
                            foreach (object obj in validVars) lcw.Remove(obj);
                        }
                        vTargetListBoxDatasets.Focus();
                    }

                    //Added by Aaron 08/13/2014
                    //This is for the case that I am moving a variable year to a target list that already contains year
                    //validvars.count is 0 as I have already detercted its in the target variable. I now want to high light it in the targetvariable
                    if (validVars.Count == 0)
                    {
                        List<object> firstitem = new List<object>();
                        firstitem.Add(vInputListBoxDatasets.SelectedItems[0]);
                        if (vTargetListBoxDatasets.Items.Contains(vInputListBoxDatasets.SelectedItems[0]))
                        {
                            vTargetListBoxDatasets.SetSelectedItems(firstitem);
                            vTargetListBoxDatasets.Focus();
                        }
                    }
          

                }
                //Destination is a BSkytargetlist 
                else
                {
                    noSelectedItems = vInputList.SelectedItems.Count;
                    //Checking whether variables moved are allowed by the destination filter
                    //validVars meet filter requirements
                    //invalidVars don't meet filter requirements

                    if (vTargetList != null)
                    {

                        if (noSelectedItems == 0)
                        {
                            diagResult = System.Windows.Forms.MessageBox.Show("You need to select a variable from the  list before clicking the move button", "Error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Question);
                            return;
                        }


                        if (vTargetList.GetType().Name == "SingleItemList" && noSelectedItems > 1)
                        {
                            diagResult = System.Windows.Forms.MessageBox.Show("You cannot move more than 1 variable into a grouping variable list", "Error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Question);
                            return;
                        }
                        //Added 10/19/2013
                        //Added the code below to support listboxes that only allow a pre-specified number of items or less
                        //I add the number of preexisting items to the number of selected items and if it is greater than limit, I show an error

                        if (vTargetList.maxNoOfVariables != string.Empty && vTargetList.maxNoOfVariables != null)
                        {
                            try
                            {
                                maxnoofvars = Convert.ToDouble(vTargetList.maxNoOfVariables);
                                //Console.WriteLine("Converted '{0}' to {1}.", vTargetList.maxNoOfVariables, maxnoofvars);

                                // diagResult = System.Windows.Forms.MessageBox.Show("An invalid value has been entered for the maximum number of variables in the destination variable list" , "Message", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Question);

                            }
                            catch (FormatException)
                            {
                                diagResult = System.Windows.Forms.MessageBox.Show("An invalid value has been entered for the maximum number of variables in the target variable list", "Error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Question);
                            }
                            catch (OverflowException)
                            {
                                diagResult = System.Windows.Forms.MessageBox.Show("An invalid value has been entered for the maximum number of variables in the target variable list", "Error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Question);
                            }
                            if (maxnoofvars < (noSelectedItems + vTargetList.ItemsCount))
                            {
                                //e.Effects = DragDropEffects.None;
                                //e.Handled = true;
                                message = "The target variable list cannot have more than " + vTargetList.maxNoOfVariables + " variable(s). Please reduce your selection or remove variables from the target list";
                                diagResult = System.Windows.Forms.MessageBox.Show(message, "Error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Question);
                                return;

                            }

                        }


                        for (i = 0; i < noSelectedItems; i++)
                        {
                            //Added by Aaron 08/12/2014
                            //Line below ensures that I move items from teh source to the target only when the items are not in the target
                            //If Item already exists in target I ignore
                            if (!vTargetList.Items.Contains(vInputList.SelectedItems[i]))
                            {
                                filterResults = vTargetList.CheckForFilter(vInputList.SelectedItems[i]);
                                if (filterResults)
                                {

                                    validVars.Add(vInputList.SelectedItems[i]);
                                }
                                else
                                {
                                    invalidVars.Add(vInputList.SelectedItems[i]);
                                }
                            }
                        }
                        //If there are valid variables then move them
                        if (validVars.Count != 0)
                        {
                            if (vTargetList.GetType().Name == "SingleItemList")
                            {
                                //I check if there items in the target list and I move them to the source or display an error message

                                if (vTargetList.ItemsCount > 0)
                                {
                                    //If the input is the source variable list, I check whether the there are entries in the singleitem target
                                    //if there I entries, i move them back to the source if there are not already in the source
                                    //and then move the selected item to the target
                                    if (vInputList.AutoVar == false)
                                    {
                                        int noofitems = vTargetList.ItemsCount;
                                        object[] arr = new object[noofitems];

                                        //Adding the items in the target to the source before adding new item to the target
                                        for (i = 0; i < noofitems; i++)
                                        {
                                            //Added by Aaron 08/12/2014
                                            //Added line below, this ensures that if I have a singleitemlist and the target is full, 
                                            //I add items to the source/input list only if that item is not in the input list
                                            if (!vInputList.Items.Contains(vTargetList.Items[i]))
                                                arr[i] = vTargetList.Items[i];
                                        }
                                        vInputList.AddItems(arr);
                                        //Removing all items from the target

                                        //The compiler is not allowing me to use vInputList.Items.Remove() so I have to use ItemsSource

                                        ListCollectionView lcw = vTargetList.ItemsSource as ListCollectionView;
                                        foreach (object obj in arr) lcw.Remove(obj);
                                    }
                                    //Aaron 09/15/2013
                                    //There is no need of an else as the move button is alwats between a source and destination
                                }
                            }

                            vTargetList.AddItems(validVars as List<object>);
                            //The code below unselects everything
                            vTargetList.UnselectAll();
                            //The code below selects all the items that are moved
                            vTargetList.SetSelectedItems(validVars as List<object>);
                            //Added by Aaron on 12/24/2012 to get the items moved scrolled into view
                            //Added by Aaron on 12/24/2012. Value is 0 as you want to scroll to the top of the selected items
                            vTargetList.ScrollIntoView(validVars[0]);
                            if (vInputList.MoveVariables)
                            //The compiler is not allowing me to use vInputList.Items.Remove() so I have to use ItemsSource
                            {
                                ListCollectionView lcw = vInputList.ItemsSource as ListCollectionView;
                                foreach (object obj in validVars) lcw.Remove(obj);
                            }
                            vTargetList.Focus();

                        }
                    }
                        //Added by Aaron 07/22/2015
                        //This is a valid point
                        if (vTargetDragDropListForSummarize != null)
                        {
                            
                            List<object> tempVars = new List<object>();
                            string functionName = "";
                            functionName = vTargetDragDropListForSummarize.getFunctionFromComboBox();


                            if (noSelectedItems == 0)
                            {
                                diagResult = System.Windows.Forms.MessageBox.Show("You need to select a variable from the  list before clicking the move button", "Error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Question);
                                return;
                            }


                            //if (vTargetList.GetType().Name == "SingleItemList" && noSelectedItems > 1)
                            //{
                            //    diagResult = System.Windows.Forms.MessageBox.Show("You cannot move more than 1 variable into a grouping variable list", "Error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Question);
                            //    return;
                            //}
                            //Added 10/19/2013
                            //Added the code below to support listboxes that only allow a pre-specified number of items or less
                            //I add the number of preexisting items to the number of selected items and if it is greater than limit, I show an error

                            if (vTargetDragDropListForSummarize.maxNoOfVariables != string.Empty && vTargetDragDropListForSummarize.maxNoOfVariables != null)
                            {
                                try
                                {
                                    maxnoofvars = Convert.ToDouble(vTargetDragDropListForSummarize.maxNoOfVariables);
                                    //Console.WriteLine("Converted '{0}' to {1}.", vTargetList.maxNoOfVariables, maxnoofvars);

                                    // diagResult = System.Windows.Forms.MessageBox.Show("An invalid value has been entered for the maximum number of variables in the destination variable list" , "Message", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Question);

                                }
                                catch (FormatException)
                                {
                                    diagResult = System.Windows.Forms.MessageBox.Show("An invalid value has been entered for the maximum number of variables in the target variable list", "Error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Question);
                                }
                                catch (OverflowException)
                                {
                                    diagResult = System.Windows.Forms.MessageBox.Show("An invalid value has been entered for the maximum number of variables in the target variable list", "Error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Question);
                                }
                                if (maxnoofvars < (noSelectedItems + vTargetList.ItemsCount))
                                {
                                    //e.Effects = DragDropEffects.None;
                                    //e.Handled = true;
                                    message = "The target variable list cannot have more than " + vTargetDragDropListForSummarize.maxNoOfVariables + " variable(s). Please reduce your selection or remove variables from the target list";
                                    diagResult = System.Windows.Forms.MessageBox.Show(message, "Error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Question);
                                    return;

                                }

                            }

                            tempVars = validVars as List<object>;
                            for (i = 0; i < noSelectedItems; i++)
                            {
                                //Added by Aaron 08/12/2014
                                //Line below ensures that I move items from teh source to the target only when the items are not in the target
                                //If Item already exists in target I would typically ignore, however I CANNOT as I can have mean(var1) and median(var1)
                                
                               
                                if (vTargetDragDropListForSummarize.Items.Contains(vInputList.SelectedItems[i]))
                                {
                                     
                                    //Added by Aaron 10/02/2015
                                    //It gets a little tricky for the aggregare control. The reason is as follows
                                    //You want the mean(var1),median(var1), max(var1)
                                    //Now when you move copy variable, the source and destination controls point to the same datasource variable object
                                    //However, if you want mean(var1) and median(var1) you cannot have the same object
                                    //You need to create a duplicate object
                                    //The function checkIfAggregateOptionExists checks all the items within the target listbox looking to see if the user
                                    //has already selected the aggregate option
                                    //if I have already selected, I do nothing
                                    //If its not already selected, I create a new datasource variable with the option
                                    
                                    doesAggOptionExist =checkIfAggregateOptionExists(vTargetDragDropListForSummarize, vInputList.SelectedItems[i] as DataSourceVariable, functionName);
                                    
                                    
                                    //We need to check if the function name matches
                                    DataSourceVariable ds = vInputList.SelectedItems[i] as DataSourceVariable;
                                    ////string varNameWithFunction = ds.XName;
                                    ////copyFunctionName =var
                                    //string copyFunctionName = "";
                                    //firstpos = ds.XName.IndexOf(@"(");
                                    ////lastpos = ds2.XName.IndexOf(@")");
                                    ////if (firstpos != -1 || lastpos != -1)
                                    //if (firstpos != -1)
                                    //{
                                    //    copyFunctionName = ds.XName.Substring(0, firstpos);
                                    //}
                                  // ds2.XName = ds2.XName.Substring(firstpos + 1, (lastpos - (firstpos + 1)));
                                     if (!doesAggOptionExist)
                                    {
                                        DataSourceVariable newds = new DataSourceVariable();
                                        //31Jul2016 9:43AM :Anil: line will not work as we moved RNAME out of the Name property in DataSource:  newds.Name = ds.RName; //RName can support A.2 as col name
                                        newds.RName = ds.RName;  //Anil: This fixes: 31Jul2016 9:43AM 
                                        newds.XName = ds.XName;
                                        newds.DataType = ds.DataType;
                                        newds.DataClass = ds.DataClass;
                                        newds.Width = ds.Width;
                                        newds.Decimals = ds.Decimals;
                                        newds.Label = ds.Label;
                                        newds.Values = ds.Values;
                                        newds.Missing = ds.Missing;
                                        newds.MissType = ds.MissType;
                                        newds.Columns = ds.Columns;
                                        newds.Measure = ds.Measure;
                                        newds.ImgURL = ds.ImgURL;
                                        filterResults = vTargetDragDropListForSummarize.CheckForFilter(vInputList.SelectedItems[i]);
                                        if (filterResults)
                                        {

                                            validVars.Add(newds);
                                        }
                                       // else
                                       // {
                                       //     invalidVars.Add(vInputList.SelectedItems[i]);
                                       // }



                                    }

                                }

                                //if (!vTargetDragDropListForSummarize.Items.Contains(vInputList.SelectedItems[i]))
                                else
                                {
                                    filterResults = vTargetDragDropListForSummarize.CheckForFilter(vInputList.SelectedItems[i]);
                                    if (filterResults)
                                    {

                                        validVars.Add(vInputList.SelectedItems[i]);
                                    }
                                    else
                                    {
                                        invalidVars.Add(vInputList.SelectedItems[i]);
                                    }
                                }
                            }

                          if (validVars.Count >0)
                           {

                                foreach (object obj in tempVars)
                                {
                                    
                                    DataSourceVariable ds = obj as DataSourceVariable;
                                    if (functionName != "asc")
                                    {
                                        ds.XName = functionName + "(" + ds.RName + ")";
                                    }
                                    else ds.XName = ds.RName;
                                
                                }
                                vTargetDragDropListForSummarize.AddItems(tempVars);
                                vTargetDragDropListForSummarize.UnselectAll();
                                //The code below selects all the items that are moved
                                vTargetDragDropListForSummarize.SetSelectedItems(tempVars);
                                vTargetDragDropListForSummarize.ScrollIntoView(tempVars[0]);
                            }
                        }
                        //else
                        //{

                        //    vTargetList.AddItems(validVars);

                        //    //The code below unselects everything
                        //    vTargetList.UnselectAll();
                        //    //The code below selects all the items that are moved
                        //    vTargetList.SetSelectedItems(validVars);
                        //    //Added by Aaron on 12/24/2012 to get the items moved scrolled into view
                        //    //Added by Aaron on 12/24/2012. Value is 0 as you want to scroll to the top of the selected items
                        //    vTargetList.ScrollIntoView(validVars[0]);
                        //}
                        if (vInputList.MoveVariables)
                        //The compiler is not allowing me to use vInputList.Items.Remove() so I have to use ItemsSource
                        {
                            ListCollectionView lcw = vInputList.ItemsSource as ListCollectionView;
                            foreach (object obj in validVars) lcw.Remove(obj);
                        }
                    if (vTargetList!=null)    
                    vTargetList.Focus();
                    if (vTargetDragDropListForSummarize != null)
                        vTargetDragDropListForSummarize.Focus();
                    
                    //Added by Aaron 08/13/2014
                    //This is for the case that I am moving a variable year to a target list that already contains year
                    //validvars.count is 0 as I have already detercted its in the target variable. I now want to high light it in the targetvariable
                    if (validVars.Count == 0)
                    {
                        List<object> firstitem = new List<object>();
                        firstitem.Add(vInputList.SelectedItems[0]);

                        if (vTargetList != null)
                        {

                            if (vTargetList.Items.Contains(vInputList.SelectedItems[0]))
                            {
                                vTargetList.SetSelectedItems(firstitem);
                                vTargetList.Focus();
                            }
                        }
                        if (vTargetDragDropListForSummarize != null)
                        {
                            if (vTargetDragDropListForSummarize.Items.Contains(vInputList.SelectedItems[0]))
                            {
                                vTargetDragDropListForSummarize.SetSelectedItems(firstitem);
                                vTargetDragDropListForSummarize.Focus();
                            }
                        }
                    }
                    //If there are variables that don't meet filter criteria, inform the user
                    if (invalidVars.Count > 0)
                    {
                        List<object> ls = invalidVars as List<object>; 
                        string cantMove = string.Join(",", ls.ToArray());
                        System.Windows.Forms.DialogResult result = System.Windows.Forms.MessageBox.Show("The variable(s) \"" + cantMove + "\" cannot be moved, the destination variable list does not allow variables of that type", "Save Changes", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Question);
                    }
                }

            }
            //Moving from destination to source, there are following cases, moving from dataset list to source, moving from destination variable list to source variable list, moving from grouping variable to source
            else if (Tag.ToString() == TO_SOURCE)
            {

                //string originalOrder;
                //List <string> originalOrder = new List <String> {"subject","gender","scenario","attitude","frequency","gendeXattitude","subject_attitude" ,"scenaio_attitude" };
                //Case of moving to a dataset list
                List<string> originalOrder = GetAllVars();
                if (vTargetListBoxDatasets != null)
                {
                    noSelectedItems = vTargetListBoxDatasets.SelectedItems.Count;

                    double maxnoofvars = -1;
                    if (vInputListBoxDatasets.maxNoOfVariables != string.Empty && vInputListBoxDatasets.maxNoOfVariables != null)
                    {
                        
                        try
                        {
                            maxnoofvars = Convert.ToDouble(vInputListBoxDatasets.maxNoOfVariables);
                            //Console.WriteLine("Converted '{0}' to {1}.", vTargetList.maxNoOfVariables, maxnoofvars);

                            // diagResult = System.Windows.Forms.MessageBox.Show("An invalid value has been entered for the maximum number of variables in the destination variable list" , "Message", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Question);
                            // return;
                        }
                        catch (FormatException)
                        {
                            diagResult = System.Windows.Forms.MessageBox.Show("An invalid value has been entered for the maximum number of datasets in the target dataset list", "Error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Question);
                        }
                        catch (OverflowException)
                        {
                            diagResult = System.Windows.Forms.MessageBox.Show("An invalid value has been entered for the maximum number of datasets in the target dataset list", "Error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Question);
                        }
                        if (maxnoofvars < (noSelectedItems + vInputList.ItemsCount))
                        {
                            //e.Effects = DragDropEffects.None;
                            //e.Handled = true;
                            message = "The target dataset list cannot have more than " + vTargetListBoxDatasets.maxNoOfVariables + "datasets. Please reduce your selection or remove variables from the target dataset list";
                            diagResult = System.Windows.Forms.MessageBox.Show(message, "Error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Question);
                            return;

                        }
                    }


                    for (i = 0; i < noSelectedItems; i++)
                    {
                        //Added by Aaron 08/12/2014
                        //Line below ensures that I move items from the target to the source only when the items are not in the source
                        //If Item already exists in target I ignore
                        if (!vInputListBoxDatasets.Items.Contains(vTargetListBoxDatasets.SelectedItems[i]))
                        {
                           // filterResults = vInputList.CheckForFilter(vTargetList.SelectedItems[i]);
                            //if (filterResults) validVars.Add(vTargetList.SelectedItems[i]);
                            //else invalidVars.Add(vTargetList.SelectedItems[i]);
                            validVars.Add(vTargetListBoxDatasets.SelectedItems[i]);
                        }
                    }
                    if (validVars.Count != 0)
                    {
                        vInputListBoxDatasets.AddItems(validVars as List<object>);
                        vInputListBoxDatasets.UnselectAll();
                        vInputListBoxDatasets.SetSelectedItems(validVars as List<object>);
                        vInputListBoxDatasets.ScrollIntoView(validVars[0]);


                        if (vTargetListBoxDatasets.MoveVariables)
                        {
                            ListCollectionView lcw = vTargetListBoxDatasets.ItemsSource as ListCollectionView;
                            foreach (object obj in validVars) lcw.Remove(obj);
                        }

                        vInputListBoxDatasets.Focus();
                    }

                    //Added by Aaron 08/13/2014
                    //This is for the case that I am moving a variable year to a source list that already contains year
                    //validvars.count is 0 as I have already detected its in the source variable. I now want to high light it in the source variable
                    //Also I need to remove it from the target as the target can only contain a single item
                    if (validVars.Count == 0)
                    {
                        List<object> firstitem = new List<object>();
                        firstitem.Add(vTargetListBoxDatasets.SelectedItems[0]);
                        if (vInputListBoxDatasets.Items.Contains(vTargetListBoxDatasets.SelectedItems[0]))
                        {
                            vInputListBoxDatasets.SetSelectedItems(firstitem);
                            vInputListBoxDatasets.Focus();
                            //removing it from the target
                            ListCollectionView lcw = vTargetListBoxDatasets.ItemsSource as ListCollectionView;
                            lcw.Remove(vTargetListBoxDatasets.SelectedItems[0]);
                        }
                    }
                }

                //Moving to BSkySource
                else
                {

                    //I am moving from a target list
                    if (vTargetList !=null)
                    {

                        noSelectedItems = vTargetList.SelectedItems.Count;

                        if (noSelectedItems == 0)
                        {
                            diagResult = System.Windows.Forms.MessageBox.Show("You need to select a variable from the  list before clicking the move button", "Error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Question);
                            return;
                        }


                        if (vInputList.GetType().Name == "SingleItemList" && noSelectedItems > 1)
                        {
                            diagResult = System.Windows.Forms.MessageBox.Show("You cannot move more than 1 variable into a grouping variable list", "Message", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Question);
                            return;
                        }


                        double maxnoofvars = -1;
                        if (vInputList.maxNoOfVariables != string.Empty && vInputList.maxNoOfVariables != null)
                        {
                             try
                                    {
                                        maxnoofvars = Convert.ToDouble(vInputList.maxNoOfVariables);
                                        //Console.WriteLine("Converted '{0}' to {1}.", vTargetList.maxNoOfVariables, maxnoofvars);

                                        // diagResult = System.Windows.Forms.MessageBox.Show("An invalid value has been entered for the maximum number of variables in the destination variable list" , "Message", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Question);
                                       // return;
                                    }
                                    catch (FormatException)
                                    {
                                        diagResult = System.Windows.Forms.MessageBox.Show("An invalid value has been entered for the maximum number of variables in the target variable list", "Error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Question);
                                    }
                                    catch (OverflowException)
                                    {
                                        diagResult = System.Windows.Forms.MessageBox.Show("An invalid value has been entered for the maximum number of variables in the target  variable list", "Error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Question);
                                    }
                                    if (maxnoofvars < (noSelectedItems + vInputList.ItemsCount))
                                    {
                                        //e.Effects = DragDropEffects.None;
                                        //e.Handled = true;
                                        message = "The target variable list cannot have more than " + vTargetList.maxNoOfVariables + "variables. Please reduce your selection or remove variables from the target list";
                                        diagResult = System.Windows.Forms.MessageBox.Show(message, "Error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Question);
                                        return;

                                    }
                                }
                            for (i = 0; i < noSelectedItems; i++)
                            {
                                    //Added by Aaron 08/12/2014
                                    //Line below ensures that I move items from the target to the source only when the items are not in the source
                                    //If Item already exists in target I ignore
                                    if (!vInputList.Items.Contains(vTargetList.SelectedItems[i]))
                                    {
                                        filterResults = vInputList.CheckForFilter(vTargetList.SelectedItems[i]);
                                        if (filterResults) validVars.Add(vTargetList.SelectedItems[i]);
                                        else invalidVars.Add(vTargetList.SelectedItems[i]);
                                    }
                            }
                 
                if (validVars.Count != 0)
                {

                            //if (vTargetDragDropListForSummarize != null)
                            //{
                            //    List<object> tempVars = new List<object>();
                            //    tempVars = validVars;

                            //    foreach (object obj1 in tempVars)
                            //    {
                            //        DataSourceVariable ds2 = obj1 as DataSourceVariable;
                            //        firstpos = ds2.Name.IndexOf(@"(");
                            //        lastpos = ds2.Name.IndexOf(@")");
                            //        ds2.XName = ds2.XName.Substring(firstpos + 1, (lastpos - (firstpos + 1)));
                            //    }
                            //    vInputList.AddItems(tempVars);
                            //    vInputList.UnselectAll();
                            //    vInputList.SetSelectedItems(tempVars);
                            //    vInputList.ScrollIntoView(tempVars[0]);

                            //}
                            //else
                            //{

                            //  vInputList.AddItems(validVars);

                            ///////////////////////////////////////////////////////////////////////////////////////////
                            ListCollectionView view = vInputList.ItemsSource as ListCollectionView;

                            IList<DataSourceVariable> srcVars = view.SourceCollection as IList<DataSourceVariable>;

                            List<string> unorderedSourceVariables = new List<string>();

                        
                            IList<Item> Items = new List<Item>();
                        
                            foreach ( DataSourceVariable obj1 in srcVars)
                            {
                                Items.Add(new Item() { Id = originalOrder.IndexOf(obj1.RName), Vars= obj1 });
                            }
                            foreach (DataSourceVariable obj2 in validVars)
                            {
                                Items.Add(new Item() { Id = originalOrder.IndexOf(obj2.RName), Vars = obj2 });
                            }

                            //09/30/2019
                            Items = Items.OrderBy(f=>f.Id).ToList();

                            List < DataSourceVariable > newSrcVars = new List<DataSourceVariable>(); 

                            foreach( Item obj4 in Items)
                            {
                                newSrcVars.Add(obj4.Vars);
                            }


                             vInputList.ItemsSource = new ListCollectionView(newSrcVars);

                           // vInputList.Items.Insert()
                            vInputList.UnselectAll();
                            vInputList.SetSelectedItems(validVars as List<object>);
                            vInputList.ScrollIntoView(validVars[0]);
                    }
                    
                    if (vTargetList.MoveVariables) 
                    {
                        ListCollectionView lcw = vTargetList.ItemsSource as ListCollectionView;
                        foreach (object obj in validVars) lcw.Remove(obj);
                        
                    }

                    vInputList.Focus();
                }

                    if (vTargetDragDropListForSummarize != null)
                    {

                        noSelectedItems = vTargetDragDropListForSummarize.SelectedItems.Count;

                        if (vInputList.GetType().Name == "SingleItemList" && noSelectedItems > 1)
                        {
                            diagResult = System.Windows.Forms.MessageBox.Show("You cannot move more than 1 variable into a grouping variable list", "Message", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Question);
                            return;
                        }


                        double maxnoofvars = -1;
                        if (vInputList.maxNoOfVariables != string.Empty && vInputList.maxNoOfVariables != null)
                        {
                            try
                            {
                                maxnoofvars = Convert.ToDouble(vInputList.maxNoOfVariables);
                                //Console.WriteLine("Converted '{0}' to {1}.", vTargetList.maxNoOfVariables, maxnoofvars);

                                // diagResult = System.Windows.Forms.MessageBox.Show("An invalid value has been entered for the maximum number of variables in the destination variable list" , "Message", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Question);
                                // return;
                            }
                            catch (FormatException)
                            {
                                diagResult = System.Windows.Forms.MessageBox.Show("An invalid value has been entered for the maximum number of variables in the target variable list", "Error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Question);
                            }
                            catch (OverflowException)
                            {
                                diagResult = System.Windows.Forms.MessageBox.Show("An invalid value has been entered for the maximum number of variables in the target  variable list", "Error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Question);
                            }
                            if (maxnoofvars < (noSelectedItems + vInputList.ItemsCount))
                            {
                                //e.Effects = DragDropEffects.None;
                                //e.Handled = true;
                                message = "The target variable list cannot have more than " + vTargetDragDropListForSummarize.maxNoOfVariables + "variables. Please reduce your selection or remove variables from the target list";
                                diagResult = System.Windows.Forms.MessageBox.Show(message, "Error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Question);
                                return;

                            }
                        }
                        for (i = 0; i < noSelectedItems; i++)
                        {
                            //Added by Aaron 08/12/2014
                            //Line below ensures that I move items from the target to the source only when the items are not in the source
                            //If Item already exists in target I ignore
                            if (!vInputList.Items.Contains(vTargetDragDropListForSummarize.SelectedItems[i]))
                            {
                                filterResults = vInputList.CheckForFilter(vTargetDragDropListForSummarize.SelectedItems[i]);
                                if (filterResults)
                                {
                                    //Added by Aaron 10/02/2015
                                    //Dragdroplist for summarize is used in 2 places
                                    //The aggregatre control and the sort control
                                    //In the aggregate control, I need to support copying variables as I need                                    //to support mean(var1), median(var1)...
                                    //To support the above use case, I create a new data variable object
                                    //The new object when moved from the target to the source can create a 
                                    //duplicate variable
                                    //The line below, prevents the addition of the new variable and gets triggered only on the sort
                                    //SO BASICALLY IF I AM MOVING ITEMS TO THE SOURCE VARIABLE FROM THE AGGREGATE CONTROL NO NEW VARIABLES ARE EVER CREATED IN THE SOURCE VARIABLE
                                    //NEW VARIABLES ARE CREATED ONLY WITH THE SORT CONTROL

                                    if (vInputList.MoveVariables==true)
                                    validVars.Add(vTargetDragDropListForSummarize.SelectedItems[i]);

                                }
                                else invalidVars.Add(vTargetDragDropListForSummarize.SelectedItems[i]);
                            }
                        }
                        if (validVars.Count != 0)
                        {

                            //if (vTargetDragDropListForSummarize != null)
                            //{
                            List<object> tempVars = new List<object>();
                            tempVars = validVars as List<object>;
                            foreach (object obj1 in tempVars)
                            {
                                    DataSourceVariable ds2 = obj1 as DataSourceVariable;
                                    firstpos = ds2.XName.IndexOf(@"(");
                                    lastpos = ds2.XName.IndexOf(@")");
                                if (firstpos !=-1 || lastpos !=-1)
                                    ds2.XName = ds2.XName.Substring(firstpos + 1, (lastpos - (firstpos + 1)));
                            }
                            vInputList.AddItems(tempVars);
                            vInputList.UnselectAll();
                            vInputList.SetSelectedItems(tempVars);
                            vInputList.ScrollIntoView(tempVars[0]);

                            //}
                            //else
                            //{
                            
                        }

                        if (vTargetDragDropListForSummarize.MoveVariables)
                        {
                            ListCollectionView lcw = vTargetDragDropListForSummarize.ItemsSource as ListCollectionView;
                            foreach (object obj in validVars) lcw.Remove(obj);
                        }

                        vInputList.Focus();


                    }

                //Added by Aaron 08/13/2014
                //This is for the case that I am moving a variable year to a source list that already contains year
                //validvars.count is 0 as I have already detected its in the source variable. I now want to high light it in the source variable
                //Also I need to remove it from the target as the target can only contain a single item
                if (validVars.Count == 0)
                {

                    if (vTargetDragDropListForSummarize != null)
                    {
                        List<object> firstitem = new List<object>();
                        firstitem.Add(vTargetDragDropListForSummarize.SelectedItems[0]);
                        if (vInputList.Items.Contains(vTargetDragDropListForSummarize.SelectedItems[0]))
                        {
                            vInputList.SetSelectedItems(firstitem);
                            vInputList.Focus();
                            //removing it from the target
                            // ListCollectionView lcw = vTargetDragDropListForSummarize.ItemsSource as ListCollectionView;
                            // lcw.Remove(vTargetList.SelectedItems[0]);
                        }

                        //Added by Aaron 07/30/2015
                        //This is added for the special case where I am copying from a source to a destination
                        //The variable var 1 is copied to destination. Now I try to move it back. The destination
                        //is set to move variables, however the variables are in the source so valid vars is empty
                        //if code below does not run, the items live in destination and cannot be removed
                        if (vTargetDragDropListForSummarize.MoveVariables)
                        {
                            //ListCollectionView lcw = vTargetDragDropListForSummarize.ItemsSource as ListCollectionView;
                         //   foreach (object obj in validVars) lcw.Remove(obj);
                            
                            //Added by Aaron 09/16/2015
                            //vTargetList.RemoveSelectedItems();
                            vTargetDragDropListForSummarize.RemoveSelectedItems();

                        }


                    }
                    if (vTargetList != null)
                    {
                        List<object> firstitem = new List<object>();
                        firstitem.Add(vTargetList.SelectedItems[0]);

                        if (vInputList.Items.Contains(vTargetList.SelectedItems[0]))
                        {
                            vInputList.SetSelectedItems(firstitem);
                            vInputList.Focus();
                            //removing it from the target
                            // ListCollectionView lcw = vTargetDragDropListForSummarize.ItemsSource as ListCollectionView;
                            // lcw.Remove(vTargetList.SelectedItems[0]);
                         }

                        //Added by Aaron 07/30/2015
                        //This is added for the special case where I am copying from a source to a destination
                        //The variable var 1 is copied to destination. Now I try to move it back. The destination
                        //is set to move variables, however the variables are in the source so valid vars is empty
                        //if code below does not run, the items live in destination and cannot be removed
                        if (vTargetList.MoveVariables)
                        {
                            // ListCollectionView lcw = vTargetList.ItemsSource as ListCollectionView;
                            // foreach (object obj in validVars) lcw.Remove(obj);
                            vTargetList.RemoveSelectedItems();
                        }


                    }

                }

                
                if (invalidVars.Count > 0)
                {
                        List <object> ls1 = invalidVars as List<object>;
                        string cantMove = string.Join(",", ls1.ToArray());
                    //string cantMove = invalidVars.ToString();
                    System.Windows.Forms.DialogResult result = System.Windows.Forms.MessageBox.Show("The variable(s) \"" + cantMove + "\" cannot be moved, the destination variable list does not allow variables of that type", "Save Changes", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Question);
                }
            }
            }
        }
    }
}
