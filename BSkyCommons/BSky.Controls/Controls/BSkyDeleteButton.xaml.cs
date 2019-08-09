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
    public partial class BSkyDeleteButton : Button, IBSkyControl
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

        public BSkyDeleteButton()
        {
            InitializeComponent();
            imgDest.BeginInit();
            imgDest.UriSource = new Uri(@"pack://application:,,,/BSky.Controls;component/Resources/left.png");
            imgDest.EndInit();

            this.Width = 40;
            this.Height = 40;

            imageBtn.Source = imgDest;

            this.Tag = TO_DEST;

            //Sets the content of the move button to the grid. We add image to the grid
            this.Content = g;
            this.g.Children.Add(imageBtn);
            this.Resources.MergedDictionaries.Clear();

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

            double maxnoofvars = 0;
            int noSelectedItems = 0;
            int i = 0;
            System.Windows.Forms.DialogResult diagResult;
            string varList = "";
            string message = "";
            DataSourceVariable o;

            //Aaron 09/07/2013
            //I had to use a list object as I could not create a variable size array object
            List<object> validVars = new List<object>();
            List<object> invalidVars = new List<object>();

            //Added by Aaron 12/24/2013
            //You have the ability to move items to a textbox. When moving items to a textbox you don't have to check for filters
            //All we do is append the items selected separated by + into the textbox
            //We always copy the selected items to the textbox, items are never moved
            //We don't have to worry about tag

            //Destination is a BSkytargetlist 
            noSelectedItems = vTargetList.SelectedItems.Count;
            string newvar = "";
            //Checking whether variables moved are allowed by the destination filter
            //validVars meet filter requirements
            //invalidVars don't meet filter requirements


            if (vTargetList != null)
            {

                if (noSelectedItems == 0)
                {
                    diagResult = System.Windows.Forms.MessageBox.Show("You need to select a variable from the target variable list before clicking the delete button", "Error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Question);
                    return;
                }



                //Added 10/19/2013
                //Added the code below to support listboxes that only allow a pre-specified number of items or less
                //I add the number of preexisting items to the number of selected items and if it is greater than limit, I show an error



                //for (i = 0; i < noSelectedItems; i++)
                //{
                //    validVars.Add(vTargetList.SelectedItems[i] as object);

                //}


                //Preferred way


                //  validVars.Add(ds as object);
                //  vTargetList.AddItems(validVars);

                vTargetList.RemoveSelectedItems();
                 //   (validVars);


                // vInputList.SelectedItems[i]
                //    validVars.Add(vInputList.SelectedItems[1]);


                // vTargetList.AddItems(validVars);
                //The code below unselects everything
                vTargetList.UnselectAll();
                //The code below selects all the items that are moved
            //    vTargetList.SetSelectedItems(validVars);
                //vTargetList.SetSelectedItems(arr1);
                //Added by Aaron on 12/24/2012 to get the items moved scrolled into view
                //Added by Aaron on 12/24/2012. Value is 0 as you want to scroll to the top of the selected items
              //  vTargetList.ScrollIntoView(validVars[0]);
               
                vTargetList.Focus();


            }
            //Added by Aaron 07/22/2015
            //This is a valid point
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
         

            //Added by Aaron 08/13/2014
            //This is for the case that I am moving a variable year to a target list that already contains year
        
            //If there are variables that don't meet filter criteria, inform the user
          


        }



    }



}
//Moving from destination to source, there are following cases, moving from dataset list to source, moving from destination variable list to source variable list, moving from grouping variable to source


