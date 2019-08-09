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

namespace BSky.Controls
{
    [TypeConverter(typeof(PropertySorter))]
    [DefaultPropertyAttribute("Type")]
    public partial class BSkyBaseButtonCtrl : Button, IBSkyControl
    {
        public const string TO_SOURCE = "To Source";
        public const string TO_DEST = "To Dest";

        public BitmapImage imgSource = new BitmapImage();
        public Image imageBtn = new Image();
        public Grid g = new Grid();
        public BitmapImage imgDest = new BitmapImage();
        bool dialogDesigner = false;

        //public BSkyVariableMoveButton(bool designer):this()
        //{
        //    dialogDesigner = designer;

        //}

        public BSkyBaseButtonCtrl()
        {
            InitializeComponent();

            this.Click += new RoutedEventHandler(BSkyBaseButtonCtrl_Click);

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
        public DragDropList vInputList;

        //# private DragDropListForSummarize vInputDragDropListForSummarize; 
        //Added by Aaron 09/01/2014
        //Holds the source dataset list
        //# private BSkyListBoxwBorderForDatasets vInputListBoxDatasets;

       



        //  private string moveButtonName;
        public string targetListName;
        public DragDropList vTargetList;

        //# private BSkyTextBox vTargetTextBox;

        //# private DragDropListForSummarize vTargetDragDropListForSummarize;
        //Added by Aaron 09/01/2014
        //Holds the destination dataset list
        //  private BSkyListBoxwBorderForDatasets vTargetListBoxDatasets;

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
                if (obj == null || (!(obj is DragDropList)))
                {
                    MessageBox.Show("Unable to associate this control with the proper source variable list control");
                    return;
                }

                //Added by Aaron 09/01/2014
                //the function below makes sure that the move button is setup with the correct source and destination for a valid move
                //
                //  validInputtargets=validateInputTarget(value, "", obj.GetType().Name, "");
                //if (validInputtargets == false) return;

                //09/14/2013
                //Added by Aaron to support a Grouping variable
                if (obj is DragDropList)
                {
                    inputListName = value;

                    //Added by Aaron 05/29/2014
                    //Commented line below and added line below it
                    // vInputList = obj as BSkyVariableList;
                    vInputList = obj as DragDropList;
                    //vInputList.GotFocus += new RoutedEventHandler(vInputList_GotFocus);
                    //vInputList.SelectionChanged += new SelectionChangedEventHandler(vInputList_SelectionChanged);
                }

                //   vInputList.GotFocus += new RoutedEventHandler(vInputList_GotFocus);
                //  vInputList.SelectionChanged += new SelectionChangedEventHandler(vInputList_SelectionChanged);
            }
        }

        //Added by Aaron 09/01/2014
        //the function below makes sure that the move button is setup with the correct source and destination for a valid move
        //When this function is called, we pass either the source dataset name and the source type or the destination dataset name 
        //and type, what ever is just entered by the user. WE then check if the values entered are correct values for the existing source or destination control that the move button is already associated with. If you enter a source name and the destination name is not entered, we will return true


        

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

                if (obj == null || (!(obj is DragDropList)))
                {
                    MessageBox.Show("Unable to associate this control with the proper target variable list control, target must be of type variable list");
                    return;
                }
                // validInputtargets=validateInputTarget("", value, "",obj.GetType().Name);
                //f (validInputtargets == false) return;



                if (obj is DragDropList)
                {
                    targetListName = value;
                    vTargetList = obj as DragDropList;
                //    vTargetList.GotFocus += new RoutedEventHandler(vTargetList_GotFocus);
                //    vTargetList.SelectionChanged += new SelectionChangedEventHandler(vTargetList_SelectionChanged);
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

        //private void SetImage(bool toDest)
        //{
        //    imageBtn = new Image()
        //    {
        //        Width = 20,
        //        Height = 20
        //    };
        //    imageBtn.Source = imgSource;
        //    imageBtn.Stretch = Stretch.Fill;
        //    Grid theStackPanel = this.Content as Grid;
        //    theStackPanel.Children.Clear();
        //    if (toDest)
        //    {
        //        imageBtn.Source = imgDest;
        //    }
        //    else
        //    {
        //        imageBtn.Source = imgSource;
        //    }
        //    theStackPanel.Children.Add(imageBtn);
        //}



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
            // this.Tag = TO_SOURCE;
            // SetImage(false);
            //this.Content = ;
        }

        void vInputList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //if (vInputList.SelectedItems.Count == 0 && (this.Tag.ToString() == TO_DEST))
            //    this.IsEnabled = false;
            //else
            //    this.IsEnabled = true;
        }

        //void vInputList_GotFocus(object sender, RoutedEventArgs e)
        //{
        //    this.Tag = TO_DEST;
        //    SetImage(true);
        //    //this.Content = Resource.Actions_go_next_icon;
        //}

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




        public virtual void BSkyBaseButtonCtrl_Click(object sender, RoutedEventArgs e)
        {

        }



        }



    }
    //Moving from destination to source, there are following cases, moving from dataset list to source, moving from destination variable list to source variable list, moving from grouping variable to source

