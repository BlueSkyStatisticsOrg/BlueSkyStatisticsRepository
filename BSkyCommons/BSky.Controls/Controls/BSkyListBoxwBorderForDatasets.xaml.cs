
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using BSky.Statistics.Common;
using System.Windows.Controls.Primitives;
using BSky.Interfaces.Controls;

namespace BSky.Controls
{
    /// <summary>
    /// Interaction logic for ListBoxwBorderForDatasets.xaml
    /// </summary>
    [TypeConverter(typeof(PropertySorter))]
    [DefaultPropertyAttribute("Type")]
    public partial class BSkyListBoxwBorderForDatasets : ListBox, IBSkyAffectsExecute, IBSkyInputControl, IBSkyControl, IBSkyEnabledControl
    {
        
        public BSkyListBoxwBorderForDatasets()
        {
            InitializeComponent();
            
            SelectionChangeBehaviour = new BehaviourCollection();
            base.SelectionMode = SelectionMode.Extended;
        }

        //dialogmode =true means the dataset listbox is invoked from dialog editor
        //populate =true means I automatically populate the dataset variable list with existing datasets
        public BSkyListBoxwBorderForDatasets(bool populate, bool dialogmode)
        {
            //Code below sets up the binding for displaying the icon and dataset in the listbox
            InitializeComponent();
            base.SelectionMode = SelectionMode.Extended;
            SelectionChangeBehaviour = new BehaviourCollection();
            AutoPopulate = populate;
            DialogEditorMode = dialogmode;
            SubstituteSettings = "UseComma";
            MoveVariables = true;
            
            Syntax = "%%VALUE%%";
           
            
        }


        public string SepCharacter
        {
            get;
            set;
        }


        public string PrefixTxt
        {
            get;
            set;
        }
        private bool autopopulate = true;

        //[Category("Control Settings"), PropertyOrder(2)]

        //[Description("Required property. You must specify a unique name for every control added to the dialog. You will not be able to save a dialog definition unless every control in the dialog and contained sub-dialogs has a unique name.")]

        public bool AutoPopulate
        {
            get
            {
                return autopopulate;
            }
            set
            {
                autopopulate = value;
            }
        }

        private bool dialogeditormode = true;
       

        public bool DialogEditorMode
        {
            get
            {
                return dialogeditormode;
            }
            set
            {
                dialogeditormode = value;
                //I am in dialog editor
                if (dialogeditormode == true && AutoPopulate == true)
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

                    //List<DataSourceVariable> preview = new List<DataSourceVariable>();


                   // if (Filter.Contains("Scale"))
                    //{
                   //  List<DataSourceVariable> preview = new List<DataSourceVariable>();
                   //     DataSourceVariable var1 = new DataSourceVariable();
                   //     var1.Name = "var1";
                   //     var1.DataType = DataColumnTypeEnum.Numeric;
                   //     var1.Width = 4;
                   //     var1.Decimals = 0;
                   //     var1.Label = "var1";
                   //     var1.Alignment = DataColumnAlignmentEnum.Left;
                   //     var1.Measure = DataColumnMeasureEnum.Scale;
                   //  //   var1.ImgURL = "C:/aaron/08042014/Client/libs/BSky.Controls/Resources/nominal.png";
                   //     var1.ImgURL = "C:/aaron/08042014/Client/libs/BSky.Controls/Resources/nominal.png";
                   //     preview.Add(var1);
                   // //}

                   // //if (Filter.Contains("Nominal"))
                   // //{
                   //     DataSourceVariable var2 = new DataSourceVariable();
                   //     var2.Name = "var2";
                   //     var2.DataType = DataColumnTypeEnum.String;
                   //     var2.Width = 4;
                   //     var2.Decimals = 0;
                   //     var2.Label = "var2";
                   //     var2.Alignment = DataColumnAlignmentEnum.Left;
                   //     var2.Measure = DataColumnMeasureEnum.Nominal; ;
                   //     var2.ImgURL = "../Resources/nominal.png";
                   //     preview.Add(var2);
                   // //}

                   // //if (Filter.Contains("Ordinal"))
                   // //{
                   //     DataSourceVariable var3 = new DataSourceVariable();
                   //     var3.Name = "var3Name";
                   //     var3.DataType = DataColumnTypeEnum.String;
                   //     var3.Width = 4;
                   //     var3.Decimals = 0;
                   //     var3.Label = "var3lab";
                   //     var3.Alignment = DataColumnAlignmentEnum.Left;
                   //     var3.Measure = DataColumnMeasureEnum.Ordinal;
                   //     var3.ImgURL = "../Resources/ordinal.png";
                   //     preview.Add(var3);
                   ////}

                   //     //Bariables = new ObservableCollection<DataSourceVariable>(preview);
                   //    //Bariables= new ListCollectionView(preview as IList);
                   //  //   this.ItemsSource = Bariables; 
                   // //this.ItemsSource = listOfDisplayStrings;

                    this.ItemsSource = listOfDisplayStrings;

                }
                //I am in execute mode
                else
                {
                    if (autopopulate == true)
                    {
                        Binding binding = new Binding("Datasets");
                        this.SetBinding(BSkyListBoxwBorderForDatasets.ItemsSourceProperty, binding);
                    }
                    else
                    {
                        this.ItemsSource = new ListCollectionView(new ObservableCollection<DatasetDisplay>());
                    }
                }
            }
        }


        protected override void OnSelectionChanged(SelectionChangedEventArgs e)
        {


            //A control is associated with a behavior collection
            //A behavior collection is a collection of behaviors
            //A behavior can contains one condition e.g. itemscount = 0 and one or more setters e.g. set the property canexecute =true on control destination. The setter also stores the
            //value of the property its bound to (the control and the propert name)
            //Behaviors are invoked on the control when a particular activity takes place for e.g. when a selection changes on a listbox control, when text is entered 
            //on a text control.
            //Typical use case for behaviors is as follows. Every control has a CanExecute property. This can be enabled or disabled by default.
            //The OK button will only be enabled when the can execute property of all the controls on the canvas are set to true.
            //
            //For example, when the
            //one sample t.test dialog is rendered for the first time, the destination listbox will always be empty. We would then set the default value of canexecute to false.
            //We would define a condition for when items count is > than 0 and set canexecute for the destination property to true only when items count is > than 0. 
            //We would also add another condition to set Canexecute to false when items count is 0 to account for the case when users move variables from the destination
            //back to the source.
            //Also note that you can have canexecute to true on the destination control of one sample t.test but have the canexecute property on the textbox set to false 
            //as no valid entry has be entered tocompare the dialog against. In this situation the OK button on the dialog is disabled


            // Aaron 12/25 code below ensures that the events don't fire when I am in dialog editor mode.
            //This is to address the defect when in dialog mode, the itemscount =3 and the event on the destination list fires to set CanExecute to true
            //even though the intention is to save the dialog with CanExecute to False. This will disable the OK button when running the application unless 
            //one item is in the destination list

            if (!DialogEditorMode)
            {
                base.OnSelectionChanged(e);
                BSkyCanvas parent = UIHelper.FindVisualParent<BSkyCanvas>(this);
                if (parent != null && SelectionChangeBehaviour != null && BSkyCanvas.applyBehaviors == true)
                    parent.ApplyBehaviour(this, SelectionChangeBehaviour);
            }
        }





        protected override void OnItemsSourceChanged(IEnumerable oldValue, IEnumerable newValue)
        {
            base.OnItemsSourceChanged(oldValue, newValue);
            ListCollectionView lvw = newValue as ListCollectionView;
            if (lvw == null)
            {
                if (newValue is IList)
                    base.ItemsSource = new ListCollectionView(newValue as IList);
            }
            this.SelectedIndex = 0;
        }



        [Category("Control Settings"), PropertyOrder(1)]
        [Description("The source and destination dataset controls display active datasets. The source dataset control automatically displays all datasets open in the application. Drag and drop datasets from the source dataset to the destination dataset control to perform an analytical function using one or more datasets. This is a read only property. Click on each property in the grid to see the configuration options for this control.")]
        // [DisplayName("This is the source")]
        public string Type
        {
            get
            {
                if (AutoPopulate)
                {
                    // DisplayNameGridProperty(this, Type, "rain in spain");
                    return "Source Dataset List";
                }
                else
                {
                    //DisplayNameGridProperty(this, Type, "rain in the drain spain");
                    return "Destination Dataset List";
                }
            }
        }

      


        [CategoryAttribute("Syntax Settings")]
        [Description("Default value is UseComma. This setting allows you to set how the datasets contained in this control will replace the control name in the syntax string. To see additional details, click in the property and click the ellipses button.")]
        [Editor(@"BSky.Controls.DesignerSupport.ManageSubstitution, BSky.Controls,  Culture=neutral",
                typeof(System.Drawing.Design.UITypeEditor))]
        public string SubstituteSettings { get; set; }


        #region IBSkyInputControl Members

        [CategoryAttribute("Syntax Settings")]
        [Description("Default value of %%VALUE%% indicates that all the datasets in this dataset list control will be replaced by the control name in the syntax. These values will be used to parameterize the syntax string created when the dialog is executed. If you want a different value, for example 'test' to replace the control name, replace %%VALUE%% with 'test' (you don't need to enter the single quotes) ")]
        public string Syntax
        {
            get;
            set;
        }

        #endregion

        [Category("Control Settings"), PropertyOrder(2)]

        [Description("Required property. You must specify a unique name for every control added to the dialog. You will not be able to save a dialog definition unless every control in the dialog and contained sub-dialogs has a unique name.")]
        public new string Name
        {
            get { return base.Name; }
            set { base.Name = value; }
        }


        [Category("Control Settings"), PropertyOrder(3)]
        [Description("The number of datasets in this dataset list control. This is a read only control, its value is automatically set to the number of datasets in the control. This property can be used for defining rules. For example, if the destination dataset control is empty and you want the OK button disabled unless there are one or more datasets in this control, you will define a rule that checks the ItemsCount property, if it is 0, set canexecute to false.")]
        // [Category("Variable Settings")]
        public int ItemsCount
        {
            get
            {
                return this.Items.Count;
            }
        }


        private bool canExecute = true;

        [Category("Control Settings"), PropertyOrder(5)]
        [Description("Default value is True. This property controls whether the OK button on the dialog is enabled or disabled. For example, if you don't want the user to click the OK button of the dialog unless a checkbox control is checked, set canexecute to False, then define a rule to set canexecute to True when the checkbox is checked.")]
        public bool CanExecute
        {
            get
            {
                return canExecute;
            }
            set
            {
                canExecute = value;
                if (CanExecuteChanged != null)
                {
                    BSkyBoolEventArgs b = new BSkyBoolEventArgs();
                    b.Value = value;
                    CanExecuteChanged(this, b);
                }
            }
        }

        public event EventHandler<BSkyBoolEventArgs> CanExecuteChanged;


        private bool _enabled = true;
        [Category("Control Settings"), PropertyOrder(4)]
        [Description("Default is True(enabled). This property controls whether the default state of this dataset list control is enabled or disabled. For enabled, select True, for disabled select False. When enabled, you can drag & drop/move items into this control, when disabled, you cannot interact with the control. For example, you may want the initial state of the dataset list control to be disabled, however you may want to enable it based on a selection made in another control")]
        public bool Enabled
        {
            get
            {
                if (BSkyCanvas.dialogMode == true)
                    return _enabled;
                else return base.IsEnabled;
            }

            set
            {
                if (BSkyCanvas.dialogMode == true)
                    _enabled = value;
                else
                    base.IsEnabled = value;
            }

        }

        [Category("Dataset Settings"), PropertyOrder(1)]
        [Description("Controls whether datasets dragged and dropped from this control are copied(Move Datasets =false) or moved(Move Datasets=true). ")]
        [DisplayName("Move Datasets")]
        //  [Category("Variable Settings")]
        public bool MoveVariables
        {
            get;
            set;
        }


        private string maxItems = string.Empty;

        [Category("Dataset Settings"), PropertyOrder(2)]
        [Description("Optional property. Sets the maximum no of datasets that this dataset list control can contain. The default is empty which means that there is no maximum limit set.")]
        [DisplayName("Maximum number of datasets")]
        //   [Category("Variable Settings")]
        public string maxNoOfVariables
        {
            get { return maxItems; }
            set { maxItems = value; }
        }


        [Category("Dataset Settings"), PropertyOrder(3)]
        [DisplayName("Define Rules")]
        [Description("Default is empty(no rule). Use this optional property to define rules that trigger property changes in this or other controls, based on a selection change in this dataset list control.")]


        //  [Category("Variable Settings")]
        public BehaviourCollection SelectionChangeBehaviour
        {
            get;
            set;
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

        //[Category("Layout Properties"), PropertyOrder(2)]
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

        // [Category("Layout Properties"), PropertyOrder(3)]
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

        //  [Category("Layout Properties"), PropertyOrder(4)]
        [Category("Layout Properties"), PropertyOrder(4)]
        [Description("Default value is the Y coordinate of the top left corner of this control. To change drag the control to a different position or enter a Y coordinate.")]
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


        //Added by Aaron 02/24/2013
        //The code below is modified to not fire the drag and drop when I am dragging the scroll bar
        //If I am dragging and dropping the scroll bar and I accidently mouse over an item in the variable  list,
        //the problem was the drag and drop was initiated
        protected override void OnMouseMove(MouseEventArgs e)
        {

            base.OnMouseMove(e);
            if (DialogEditorMode) return;
            BSkyCanvas.sourceDataset = this;
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                //Added by Aaron 02/24/2013
                //The function below checks whether the origin of the drag and drop was the scroll bar 
                if (isscrollbarclickedon(e) == true) return;
                object data = GetDataFromListBox(this, e.GetPosition(this)) as object;

                //02/23/2013 Aaron
                //Code below could be used for multi drag and drop
                //object data = ((ListBox)(FrameworkElement)this).SelectedItem;

                //If the drag and drop was initiated on the scroll bar,data =null
                if (data != null)
                {
                    DragDropEffects effs;
                    ListCollectionView lst = this.ItemsSource as ListCollectionView;

                    //We have the option to move or copy variables from a listbox
                    //This is governed by the movevariables property
                    if (this.MoveVariables)
                    {
                        effs = DragDrop.DoDragDrop(this, data, DragDropEffects.Copy | DragDropEffects.Move);

                        //
                        if (effs == DragDropEffects.Copy)
                        {
                            if (lst.IsAddingNew)
                            {
                                lst.CommitNew();
                            }
                            lst.Remove(data);
                        }
                    }
                    else
                    {
                        effs = DragDrop.DoDragDrop(this, data, DragDropEffects.Copy);

                    }

                }
            }

        }

        //Added by Aaron 02/24/2013
        //This function checks whether the original source of the drag and drop was the scroll bar

        private bool isscrollbarclickedon(MouseEventArgs e)
        {
            object original = e.OriginalSource;
            //keep iterating until you get a Scrollbar , if not found, return false
            if (!original.GetType().Equals(typeof(ScrollViewer)))
            {
                if (FindVisualParent<ScrollBar>(original as DependencyObject) != null)
                {
                    return true;
                }
                else return false;

            }
            else return true;
        }

        //Added by Aaron 02/24/2013
        //This function checks whether the original source of the drag and drop was the scroll bar
        //This code was copied from
        //  http://social.msdn.microsoft.com/Forums/vstudio/en-US/9a73b1b0-ec76-4b2c-8da6-91c71e3c406f/wpf-mouse-click-event-on-scrollbar-issue?forum=wpf

        private parentItem FindVisualParent<parentItem>(DependencyObject obj) where parentItem : DependencyObject
        {
            DependencyObject parent = VisualTreeHelper.GetParent(obj);
            while (parent != null && !parent.GetType().Equals(typeof(parentItem)))
            {
                parent = VisualTreeHelper.GetParent(parent);
            }
            return parent as parentItem;
        }

        ////02/23 Aaron
        ////This code is no longet used
        //private void ListBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        //{
        //    object data = GetDataFromListBox(this, e.GetPosition(this)) as object;
        //    BSkyCanvas.sourceDrag = (ListBox)sender;
        //    if (data != null)
        //    {
        //        this.SelectedItem = data;
        //        DragDropEffects effs = DragDrop.DoDragDrop(this, data, DragDropEffects.Move);
        //        if (effs == DragDropEffects.Copy && this.MoveVariables)
        //        {
        //            ListCollectionView lst = this.ItemsSource as ListCollectionView;
        //            if (lst.IsAddingNew)
        //            {
        //                lst.CommitNew();
        //            }
        //            lst.Remove(data);
        //        }
        //    }
        //}

        public static object GetDataFromListBox(ListBox source, Point point)
        {
            UIElement element = source.InputHitTest(point) as UIElement;
            if (element != null)
            {
                object data = DependencyProperty.UnsetValue;
                while (data == DependencyProperty.UnsetValue)
                {
                    data = source.ItemContainerGenerator.ItemFromContainer(element);

                    if (data == DependencyProperty.UnsetValue)
                    {
                        element = VisualTreeHelper.GetParent(element) as UIElement;
                    }

                    if (element == source)
                    {
                        return null;
                    }
                }

                if (data != DependencyProperty.UnsetValue)
                {
                    return data;
                }
            }

            return null;
        }


        public virtual void ListBox_Drop(object sender, DragEventArgs e)
        {
            string[] formats = e.Data.GetFormats();

            //BSkyCanvas.destDrag = (ListBox)sender;
            //The code below disables drag and drop on the source list, added June 16th


            int destindex, i, j, sourceindex, noofitems;


            //object[] newlist =null;
            
            //Aaron Modified 11/04
            if (AutoPopulate == true && this == BSkyCanvas.sourceDataset)
            {
                e.Effects = DragDropEffects.None;
                return;
            }


            if (formats.Length > 0)
            {
                object sourcedata = e.Data.GetData(formats[0]) as object;

               // List<DatasetDisplay> listOfDisplayStrings = new List<DatasetDisplay>();
              //  this.ItemsSource = new ListCollectionView(new ObservableCollection<object>());
                ListCollectionView list = this.ItemsSource as ListCollectionView;

               

                if (sourcedata != null)
                {

                    //Soure and destination are different
                    //if (this != BSkyCanvas.sourceDrag)
                    if (this != BSkyCanvas.sourceDataset)
                    {
                        if (list.IndexOf(sourcedata) < 0)
                        {
                          //  this.ItemsSource = new ListCollectionView(new ObservableCollection<DatasetDisplay>());
                           // ListCollectionView list = this.ItemsSource as ListCollectionView;
                            list.AddNewItem(sourcedata);
                            list.CommitNew();

                            //this.SelectedItem = d;
                            //e.Effects =  DragDropEffects.All;
                            this.ScrollIntoView(sourcedata);//AutoScroll
                        }

                        //Aaron 09/11/2013 Commented 2 lines below
                        //else
                        //   e.Effects =  DragDropEffects.None;
                        //this.UnselectAll();
                        //02/24 Aaron
                        //This is to signify that since the source and destination are different, we have finished the copy. 
                        //We will go back to the initiation of the drag and drop to see if the source needs to be removed or kept
                        // in the source listbox. This will be determined by the value of movevariables property
                        e.Effects = DragDropEffects.Copy;
                        this.SelectedItem = sourcedata;
                        Focus();

                    }

                    //The source and the destination are the same i.e. the target variable
                    else
                    {
                        object destdata = GetDataFromListBox(this, e.GetPosition(this)) as object;
                        if (destdata == sourcedata) return;
                        destindex = list.IndexOf(destdata);
                        noofitems = list.Count;
                        object[] newlist = new object[noofitems];
                        sourceindex = list.IndexOf(sourcedata);
                        if (destindex != sourceindex)
                        {

                            //This is the case of move to the end

                            if (destindex == -1)
                            {

                                for (i = 0; i < noofitems; i++)
                                {

                                    if (i == sourceindex)
                                    {

                                        while (i <= (noofitems - 2))
                                        {
                                            newlist[i] = list.GetItemAt(i + 1);
                                            i = i + 1;
                                        }
                                        newlist[i] = list.GetItemAt(sourceindex);
                                        i = i + 1;
                                    }

                                    else newlist[i] = list.GetItemAt(i);
                                }
                            } //End of move to the end

                            else
                            {

                                if (destindex < sourceindex)
                                {
                                    for (i = 0; i < noofitems; i++)
                                    {
                                        j = i;


                                        if (i == destindex)
                                        {
                                            newlist[i] = list.GetItemAt(sourceindex);
                                            i = i + 1;


                                            while (j < noofitems)
                                            {


                                                if (j != sourceindex)
                                                {
                                                    newlist[i] = list.GetItemAt(j);
                                                    i = i + 1;
                                                }
                                                j = j + 1;
                                            }
                                        }

                                        else newlist[i] = list.GetItemAt(i);
                                    }
                                } ////End when sourceindex > destindex

                                else if (sourceindex < destindex) //I have tested this
                                {
                                    for (i = 0; i < noofitems; i++)
                                    {
                                        j = i;


                                        if (i == sourceindex)
                                        {
                                            j = j + 1;


                                            while (i < noofitems)
                                            {


                                                if (i == destindex)
                                                {
                                                    newlist[i] = list.GetItemAt(sourceindex);
                                                    i = i + 1;
                                                }


                                                else
                                                {
                                                    newlist[i] = list.GetItemAt(j);
                                                    j = j + 1;
                                                    i = i + 1;
                                                }
                                            }
                                        }


                                        else newlist[i] = list.GetItemAt(i);
                                    }

                                    //End of for loop
                                }  //end of destindex > sourceindex

                                //end of else if
                            }

                            //end of else


                            //End of the case move to end


                        }

                        //end of case destindex !=source index
                        if (list.IsAddingNew)
                        {
                            list.CommitNew();
                        }

                        for (i = 0; i < noofitems; i++)
                            list.Remove(newlist[i]);

                        for (i = 0; i < noofitems; i++)
                        {
                            list.AddNewItem(newlist[i]);
                            list.CommitNew();
                            //this.ScrollIntoView(newlist[i]);//AutoScroll
                        }
                        list.Refresh();
                        this.SelectedItem = sourcedata;
                        //02/24 Aaron
                        //This is to signify that since the source and destination is the same, we have performed a move
                        e.Effects = DragDropEffects.Move;
                    }

                    //end of source and destination and the same (target listbox
                }

                //sourcedata |=null
            }

            //formats.length >0

        }

        // Aaron 09/02/2013
        //Modified the command to check for filters on drag and drop
        //the drag over command provides visible indication of whether you can drag and drop an item over the control
        //If e.Effects =DragDropEffects.Move you can move the item, if e.Effects = DragDropEffects.Copy you can copy
        //If e.Effects =DragDropEffects.None, you cannot do anything AND THE DRAG AND DROP TERMINATES. THE function ListBox_Drop is not called

        private void ListBox_DragOver(object sender, DragEventArgs e)
        {
            // if ((AutoVar == false) && (BSkyCanvas.sourceDrag == (ListBox)sender)) e.Effects = DragDropEffects.None;
            // //else
            //// e.Effects = null != e.Data.GetData(typeof(object)) ? DragDropEffects.Move : DragDropEffects.None;

            //02/24/2013
            //Added by Aaron
            //THis ensures that the move icon is displayed when dragging and dropping within the same listbox
            //If we are not dragging and dropping within the same listbox, the copy icon is displayed

            //if ((AutoVar == true) && (BSkyCanvas.sourceDrag == (ListBox)sender)) e.Effects = DragDropEffects.Move;
            //Added 10/19/2013
            //Added the code below to support listboxes that only allow a pre-specified number of items or less
            //

            string[] formats = e.Data.GetFormats();
            DatasetDisplay sourcedata = e.Data.GetData(formats[0]) as DatasetDisplay;
            //Added 04/03/2014
            //If I highlight and drag something in a textbox and drag and drop to the variable list the application will hang
            //In code below I check whether sourcedata is of type datasource variable. If it is not, I drag allow the drop
            if (sourcedata == null)
            {
                e.Effects = DragDropEffects.None;
                e.Handled = true;
                return;
            }

            System.Windows.Forms.DialogResult diagResult;
            double result = -1;
            if (this.maxNoOfVariables != string.Empty && this.maxNoOfVariables != null)
            {
                try
                {
                    result = Convert.ToDouble(this.maxNoOfVariables);
                    //Console.WriteLine("Converted '{0}' to {1}.", this.maxNoOfVariables, result);
                }
                catch (FormatException)
                {
                    diagResult = System.Windows.Forms.MessageBox.Show("An invalid value has been entered for the maximum number of variables in the destination variable list", "Error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Question);
                }
                catch (OverflowException)
                {
                    diagResult = System.Windows.Forms.MessageBox.Show("An invalid value has been entered for the maximum number of variables in the destination variable list", "Error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Question);
                }
                if (this.ItemsCount == result)
                {
                    e.Effects = DragDropEffects.None;
                    e.Handled = true;
                    return;
                }

            }


            //BSky.Controls.filter f=new filter();
            //Added by Aaron 09/02/2013

            //Added code below to check filters when dragging and dropping
            //
            //bool filterResults = CheckForFilter(sourcedata);

            //if (!filterResults)
            //{
            //    e.Effects = DragDropEffects.None;
            //    e.Handled = true;
            //    return;
            //}


           // if ((BSkyCanvas.sourceDrag == (ListBox)sender))
           // if ((sender == (ListBox)this))
           if ( this == BSkyCanvas.sourceDataset)
            {
                e.Effects = DragDropEffects.Move;
            }
            else
            {
                e.Effects = DragDropEffects.Copy;
                int i = 10;
            }
            // else e.Effects = DragDropEffects.Copy;
            // //else e.Effects = DragDropEffects.Move;
            // e.Effects = DragDropEffects.Copy;
            e.Handled = true;
        }





        //Added by Aaron 09/07/2013
        //SetSelectedItems is a method of Listbox that cannot be accessed directly due to its protection level.
        //I believe that this cannot be invoked directly, I had an issue calling vTargetList.SetSelectedItems directly
        //The function below was created by Vishal, I modified it to accept a list of objects
        internal void SetSelectedItems(List<object> arr)
        {
            //   // throw new NotImplementedException();
            //   int i =0;
            //   int j=0;
            //   int p=0;
            //   j =this.ItemsCount;
            ////ListBoxItem temp=this.Items[i] as ListBoxItem;
            //    ListBoxItem temp=null;
            //   for (i = 0; i < j; i++)
            //   {
            //       if (this.Items[i] == arr[p])
            //       {
            //           temp = this.Items[i] as ListBoxItem;
            //           temp.IsSelected = true;
            //           p = p + 1;

            //       }
            //   }
            //}
            base.SetSelectedItems(arr);
        }


        public void AddItems(IList lst)
        {
            ListCollectionView lcw = this.ItemsSource as ListCollectionView;
            foreach (object obj in lst)
            {
                lcw.AddNewItem(obj);
            }
            lcw.CommitNew();
        }

        public void RemoveSelectedItems()
        {
            ListCollectionView lcw = this.ItemsSource as ListCollectionView;
            object[] arr = new object[this.SelectedItems.Count];
            int i = 0;
            foreach (object obj in this.SelectedItems)
                arr[i++] = obj;

            foreach (object obj in arr)
            {
                lcw.Remove(obj);
            }
        }

    }
}
