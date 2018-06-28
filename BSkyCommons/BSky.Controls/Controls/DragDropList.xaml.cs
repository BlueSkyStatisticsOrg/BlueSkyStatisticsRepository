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


namespace BSky.Controls
{
    /// <summary>
    /// Interaction logic for DragDropList.xaml
    /// </summary>
    /// 
  
   // [DefaultProperty("Name")]

    public partial class DragDropList : ListBox
    {


        public DragDropList()
        {
            InitializeComponent();
           // filter abc = new filter();
            Filter = "String|Numeric|Date|Logical|Ordinal|Nominal|Scale";
            //Added by Aaron 10/01/2013
            //Captures initial substitution settings
            SubstituteSettings = "NoPrefix|UseComma";
            //This is not used. This was entered erroreously and if removed, dialogs will not open
            summaryCtrl = false;
			base.SelectionMode = SelectionMode.Extended;
            base.Focusable = true;
            /////xxx Dont need if DataTemplate is in use xxx/////base.DisplayMemberPath = "Name";
            //autovar=true means we don't autopopulate
            autovar = true;
            // CornerRadius = 5;
            //BorderThickness = "1";
            MoveVariables = true;
          //  filter filterCheck = new filter();
           // this.Style = "{StaticResource vvv}";
         // this.Border.Style = (Style)this.FindResource("vvv");


          
        }

        //This is not used. This was entered erroreously and if removed, dialogs will not open
        public bool summaryCtrl
    {
         get;
            set;
    }
        
        public string PrefixTxt
        {
            get;
            set;
        }

        public string SepCharacter
        {
            get;
            set;
        }
        

        protected override void OnItemsSourceChanged(IEnumerable oldValue, IEnumerable newValue)
        {
            base.OnItemsSourceChanged(oldValue, newValue);
            ListCollectionView lvw = newValue as ListCollectionView;
            if (lvw != null)
                lvw.Filter = new Predicate<object>(CheckForFilter);
            else
            {
                if (newValue is IList)
                    base.ItemsSource = new ListCollectionView(newValue as IList);
            }
            this.SelectedIndex = 0;
        }

        //public virtual bool CheckForFilter(object o)
        //{
        //    return true;
        //}

        //Aaron 09/12/2013
        //commented the lines above to change the virtual bool function to a non virtual function 
        //commented the override in BSkySourceList

        public bool CheckForFilter(object o)
        {
            //01/07/2013 Aaron

            //changed the if condition below from !base.Autovar to base.Autovar
            //Autovar = true means I am auromatrcally populating variables, also means its the source list
            //Aaron 03/31/2013 Autovar =true means that its the target and not the source

            //Aaron 09/03/2013
            //Commented the 2 lines below. This is becuase we are calling this functionwhen the source variable list is loaded 
            //and also when you are dragging a source variable to a target or a source variable from target back to source
            //   if (base.AutoVar)
            //     return true;

            DataSourceVariable var = o as DataSourceVariable;
            double result;
            bool dataresult = false;
            bool measureresult = false;
            bool isdouble;
            switch (var.DataType)
            {
                case  DataColumnTypeEnum.Character:
                    if (Filter.Contains("String"))
                    {
                        dataresult = true;
                    }
                    break;
                case DataColumnTypeEnum.Numeric:
                case DataColumnTypeEnum.Double:
                case DataColumnTypeEnum.Integer:
                //case DataColumnTypeEnum.Int:
                    if (Filter.Contains("Numeric"))
                    {
                        dataresult = true;
                    }
                    break;
                    //08Feb2016 following Date and Logical code added
                case DataColumnTypeEnum.Date:
                case DataColumnTypeEnum.POSIXct:
                case DataColumnTypeEnum.POSIXlt:
                    if (Filter.Contains("Date"))
                    {
                        dataresult = true;
                    }
                    break;
                case DataColumnTypeEnum.Logical:
                    if (Filter.Contains("Logical"))
                    {
                        dataresult = true;
                    }
                    break;
            }

            if (!dataresult)
                return false;

            switch (var.Measure)
            {
                case DataColumnMeasureEnum.Nominal:
                    if (Filter.Contains("Nominal"))
                    {
                        if (nomlevels != "" && nomlevels !=null)
                        {
                            //var.Values.Count)
                           
                            isdouble = Double.TryParse(nomlevels, out result);

                            //Added by Aaron on 10/19/2013
                            //decremented the count by 1 to reflect the fact that var.values.count is one more than the 
                            //expected number of levels to handle the .that we add in the grid so that users ca
                            if ((var.Values.Count-1) == result) measureresult = true;
                            else measureresult = false;
                            break;
                        }
                        measureresult = true;
                        measureresult = true;
                    }
                    break;
                case DataColumnMeasureEnum.Ordinal:
                    if (Filter.Contains("Ordinal"))
                    {
                       if (ordlevels !="" && nomlevels !=null)
                       {
                            //var.Values.Count)
                           
                          isdouble = Double.TryParse(ordlevels, out result);
                          if ((var.Values.Count - 1) == result) measureresult = true;
                           else measureresult = false;
                           break;
                       }
                        measureresult = true;
                    }
                    break;
                case DataColumnMeasureEnum.Scale:
                    if (Filter.Contains("Scale"))
                    {
                        measureresult = true;
                    }
                    break;

                case DataColumnMeasureEnum.String: //05Feb2017
                    if (Filter.Contains("String"))
                    {
                        measureresult = true;
                    }
                    break;
                case DataColumnMeasureEnum.Date: //08Feb2017
                    if (Filter.Contains("Date"))
                    {
                        measureresult = true;
                    }
                    break;
                case DataColumnMeasureEnum.Logical: //08Feb2017
                    if (Filter.Contains("Logical"))
                    {
                        measureresult = true;
                    }
                    break;
            }

            return measureresult;
        }


        private bool autovar = false;
        private bool _renderVars =false;
        private string maxItems =string.Empty;
        public bool dialogMode = true;

        //Aaron 08/27//2014
        //I don't think this is used, however I won't delete as dialogs will fail
        public string[] propertylist = { "maxNoOfVariables", "SubstituteSettings" ,"Name","Filter","Width","Height","Left","Top","ItemsCount","MoveVariables","CanExecute"};

        [Category("Variable Settings"), PropertyOrder(3)]
        [Description("Optional property. Sets the maximum no of variables that this variable list control can contain. The default is empty which means that there is no maximum limit set.")]
      //   [Category("Variable Settings")]
        public string maxNoOfVariables { 
            get { return maxItems;}
            set { maxItems=value;}
             }

        //Aaron 10/01/2013
        //Added code below to handle substitute settings





       // [Category("Syntax Settings"), PropertyOrder(1)]
         [CategoryAttribute("Syntax Settings")]
         [Description("Default value is NoPrefix|UseComma. This setting allows you to set how the variables contained in this variable list control will replace the control name in the syntax string. To see additional details, click in the property and click the ellipses button. ")]
        [Editor(@"BSky.Controls.DesignerSupport.ManageSubstitution, BSky.Controls, Culture=neutral",
                typeof(System.Drawing.Design.UITypeEditor))]
        public string SubstituteSettings { get; set; }


        public string ordlevels{ get;set;}
        public string nomlevels { get; set; }

        //[Category("Control Settings"), PropertyOrder(1)]

      



        [Category("Control Settings"), PropertyOrder(2)]

        [Description("Required property. You must specify a unique name for every control added to the dialog. You will not be able to save a dialog definition unless every control in the dialog and contained sub-dialogs has a unique name.")]
        public new string Name
        {
            get { return base.Name; }
            set { base.Name = value; }
        }

//        public filter f;
        //Changed by Aaron 11/04/2012
        public bool AutoVar
        {
            get
            {
                return autovar;
            }
            set
            {
                //The 2 lines below were commented on 11/18 at 3.23pm
                //if (AutoVar)
                //    this.ItemsSource = new ListCollectionView(new ObservableCollection<object>());

                //else
                //{
                //    Binding binding = new Binding("Variables");
                //    this.SetBinding(DragDropList.ItemsSourceProperty, binding);
                //}
                autovar = value; 

               
            }
        }

       




       // Aaron 12/25 removed this. End use does not have to see this variable
        //[Category("BlueSky")]
        public new bool renderVars
        {
            get { return _renderVars; }
            set
            {
                //Code below runs when the dialog edit is run and you drag and drop a listbox to the canvas
                if (value)
                {
                    _renderVars = value;
                    //DataSourceVariable preview =new DataSourceVariable();
                    List<DataSourceVariable> preview = new List<DataSourceVariable>();
                    

                    if (Filter.Contains("Scale") )
                    {
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
                        preview.Add(var1);
                    }

                    if (Filter.Contains("Nominal"))
                    {
                        DataSourceVariable var2 = new DataSourceVariable();
                        var2.Name = "var2";

                        //02Aug2016 We made change to DataSource so RName is not initialised by Name prop any more. So do it like this.
                        //Old dialogs will not work, that is, variable name var1 var2 will not be show. However in application they will work.
                        var2.RName = "var2";

                        var2.DataType = DataColumnTypeEnum.Numeric;//before 06Feb2017 it was Character
                        var2.Width = 4;
                        var2.Decimals = 0;
                        var2.Label = "var2";
                        var2.Alignment = DataColumnAlignmentEnum.Left;
                        var2.Measure = DataColumnMeasureEnum.Nominal; ;
                        var2.ImgURL = "../Resources/nominal.png";
                        preview.Add(var2);
                    }

                    if (Filter.Contains("Ordinal"))
                    {
                        DataSourceVariable var3 = new DataSourceVariable();
                        var3.Name = "var3";

                        //02Aug2016 We made change to DataSource so RName is not initialised by Name prop any more. So do it like this.
                        //Old dialogs will not work, that is, variable name var1 var2 will not be show. However in application they will work.
                        var3.RName = "var3";

                        var3.DataType = DataColumnTypeEnum.Numeric;//before 06Feb2017 it was Character
                        var3.Width = 4;
                        var3.Decimals = 0;
                        var3.Label = "var3";
                        var3.Alignment = DataColumnAlignmentEnum.Left;
                        var3.Measure = DataColumnMeasureEnum.Ordinal;
                        var3.ImgURL = "../Resources/ordinal.png";
                         preview.Add(var3);
                    }

                    //05Feb2017
                    if (Filter.Contains("String"))
                    {
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
                        preview.Add(var4);
                    }

                    //08Feb2017
                    if (Filter.Contains("Date"))
                    {
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
                        preview.Add(var5);
                    }
                    //08Feb2017
                    if (Filter.Contains("Logical"))
                    {
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
                        preview.Add(var6);
                    }

                    this.ItemsSource = preview;
                }
                else
                {
                    //code below runs when renderVars =False i.e. dialog editor is not involved
                    if (BSkyCanvas.previewinspectMode == true)
                    {
                        _renderVars = value;
                        return;
                    }
                    else
                    {

                        _renderVars = value;
                        // 12/25 This is the target
                        if (AutoVar)
                            this.ItemsSource = new ListCollectionView(new ObservableCollection<object>());

                        else
                        {
                            Binding binding = new Binding("Variables");
                            this.SetBinding(DragDropList.ItemsSourceProperty, binding);
                        }
                    }
                }
            }

        }




        [Category("Variable Settings"), PropertyOrder(2)]
        [Description("The default value of 'String|Numeric|Date|Ordinal|Nominal|Scale' indicates this variable list control allows variables of all types.Click the property and then click the ellipses button to restrict this control to one or more variable types.")]
       // [Category("Variable Settings")]
        [Editor(@"BSky.Controls.DesignerSupport.VariableFilter, BSky.Controls, Culture=neutral",
                typeof(System.Drawing.Design.UITypeEditor))]
        public string Filter { get; set;}
            
            //set
            //{
            //    if (renderVars)
            //    {
            //       // List<DataSourceVariable> preview = new List<DataSourceVariable>();
            //      //  ListCollectionView previewtemp = this.ItemsSource as ListCollectionView;
            //      // previewtemp.Filter =check

                    
            //    }

            //}
            
         //    }

       // [Category("Layout Properties"),PropertyOrder(1)]
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

        [Category("Control Settings"), PropertyOrder(4)]
        [Description("The number of variables in the variable list control. This is a read only control, its value is automatically set to the number of variables in the control. This property can be used for defining rules. For example, if the variable list is empty and you want the OK button disabled unless there are one or more variables in the control, you will define a rule that checks the ItemsCount property, if it is 0, set canexecute to false.")]
       // [Category("Variable Settings")]
        public int ItemsCount
        {
            get
            {
                return this.Items.Count;
            }
        }


        private bool _enabled = true;
        [Category("Control Settings"), PropertyOrder(7)]
        [Description("Default is True(enabled). This property controls whether the default state of this variable list control is enabled or disabled. For enabled, select True, for disabled select False. When enabled, you can drag & drop/move items into this control, when disabled, you cannot interact with the control.For example, you may want the initial state of the variable list control to be disabled, however you may want to enable it based on a selection made in another control")]
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

       // [Category("Variable Settings"), PropertyOrder(4)]
       // [DisplayName("Define Rules")]
       // [Description("test")]
        
       ////  [Category("Variable Settings")]
       // public BehaviourCollection SelectionChangeBehaviour
       // {
       //     get;
       //     set;
       // }


        //Added by Aaron on 11/7/2013
        //Commented line below
       // [Category("BlueSky"), PropertyOrder(17)]

        [Category("Control Settings"), PropertyOrder(6)]
        [Editor(@"BSky.Controls.DesignerSupport.ManageOverwriteSettings, BSky.Controls,  Culture=neutral",
                typeof(System.Drawing.Design.UITypeEditor))]
        public String OverwriteSettings
        {
            get;
            set;
        }
        

        [Category("Variable Settings"), PropertyOrder(1)]
        [Description("Controls whether variables dragged and dropped from this control are copied(MoveVariables =false) or moved(MoveVariables=true). ")]
       //  [Category("Variable Settings")]
        public bool MoveVariables
        {
            get;
            set;
        }

      



      //Added by Aaron 02/24/2013
      //The code below is modified to not fire the drag and drop when I am dragging the scroll bar
      //If I am dragging and dropping the scroll bar and I accidently mouse over an item in the variable  list,
      //the problem was the drag and drop was initiated
        protected override void OnMouseMove(MouseEventArgs e)
        {
           
        base.OnMouseMove(e);
           if (renderVars) return;
           BSkyCanvas.sourceDrag = (DragDropList)this;
           if (e.LeftButton == MouseButtonState.Pressed )
           {
               //Added by Aaron 02/24/2013
               //The function below checks whether the origin of the drag and drop was the scroll bar 
               if (isscrollbarclickedon(e)==true) return;
                object data = GetDataFromListBox(this, e.GetPosition(this)) as object;
                
                //02/23/2013 Aaron
                //Code below could be used for multi drag and drop
                //object data = ((ListBox)(FrameworkElement)this).SelectedItem;
                
               //If the drag and drop was initiated on the scroll bar,data =null
                if (data != null )
                {
                    DragDropEffects effs;
                    ListCollectionView lst = this.ItemsSource as ListCollectionView;

                    //We have the option to move or copy variables from a listbox
                    //This is governed by the movevariables property
                    if (this.MoveVariables)
                    {
                        effs = DragDrop.DoDragDrop(this, data, DragDropEffects.Copy |DragDropEffects.Move);
                        
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


        public virtual  void ListBox_Drop(object sender, DragEventArgs e)
        {
            string[] formats = e.Data.GetFormats();

            //BSkyCanvas.destDrag = (ListBox)sender;
            //The code below disables drag and drop on the source list, added June 16th


            int destindex, i, j, sourceindex, noofitems;


            //object[] newlist =null;

            //Aaron Modified 11/04
            if (AutoVar == false && this == BSkyCanvas.sourceDrag)
            {
                e.Effects = DragDropEffects.None;
                return;
            }

			
            if (formats.Length > 0)
            {
                object sourcedata = e.Data.GetData(formats[0]) as object;
                ListCollectionView list = this.ItemsSource as ListCollectionView;

                if (sourcedata != null)
                {
                 
                    //Soure and destination are different
                    if (this != BSkyCanvas.sourceDrag)
                    {

                        //AARON ADD YOUR CODE HERE
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

                        if (this.MoveVariables == true)
                        {
                            if (list.IndexOf(sourcedata) < 0)
                            {
                                list.AddNewItem(sourcedata);
                                list.CommitNew();

                                //this.SelectedItem = d;
                                //e.Effects =  DragDropEffects.All;
                                this.ScrollIntoView(sourcedata);//AutoScroll
                            }
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
            DataSourceVariable sourcedata = e.Data.GetData(formats[0]) as DataSourceVariable;
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
            bool filterResults = CheckForFilter(sourcedata);

            if (!filterResults)
            {
                e.Effects = DragDropEffects.None;
                e.Handled = true;
                return;
            }
            

            if ((BSkyCanvas.sourceDrag == (ListBox)sender))
            {
                e.Effects = DragDropEffects.Move;
            }
            else
            {
                e.Effects = DragDropEffects.Copy;
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
            int i=0;
            foreach (object obj in this.SelectedItems)
                arr[i++] = obj;

            foreach (object obj in arr)
            {
                lcw.Remove(obj);
            }
        }

        //02/23 Done by Aaron
        //private void ListBox_MouseMove(object sender, MouseEventArgs e)
        //{

        //}
    }

}
