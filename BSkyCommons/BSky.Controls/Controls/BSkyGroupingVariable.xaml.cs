using System.Windows;
using System.Windows.Controls;
using BSky.Interfaces.Controls;
using System.ComponentModel;

namespace BSky.Controls
{
    /// <summary>
    /// Interaction logic for GroupVariable.xaml
    /// </summary>
    [TypeConverter(typeof(PropertySorter))]
    [DefaultPropertyAttribute("Type")]
     
    public partial class BSkyGroupingVariable : Grid, IBSkyControl, IBSkyInputControl, IBSkyAffectsExecute, IBSkyEnabledControl
    {
        [TypeConverter(typeof(PropertySorter))]
        public BSkyGroupingVariable()
        {
            InitializeComponent();

            Syntax = "%%VALUE%%";

            //Filter = "String|Numeric|Date|Ordinal|Nominal|Scale";
            SingleItemList singleItemVar = new SingleItemList();
            singleItemVar.HorizontalAlignment = HorizontalAlignment.Stretch;
            singleItemVar.VerticalAlignment = VerticalAlignment.Stretch;
           // singleItemVar.AutoVar = true;
           // singleItemVar.MoveVariables = true;
           // singleItemVar.renderVars=false;
           //// singleItemVar.HorizontalAlignment=
           //// singleItemVar.Width = double.NaN;
           //// singleItemVar.Height = double.NaN;
           // oneItemList = singleItemVar;
           this.Children.Add(singleItemVar);

           // // Aaron 09/16/2013
           // //A simple way to access the singleitemlist oject
           // foreach (object child in this.Children)
           // {
           //     if (child.GetType().Name == "SingleItemList")
           //         oneItemList = child as SingleItemList;
           // }
        }

        [Category("Control Settings"), PropertyOrder(1)]
        [Description("The grouping variable allows you to perform your analyses separately on distint groups. This control can contain a single variable. The factors of this level defines the groups which is used to  ")]
        // [DisplayName("This is the source")]
        public string Type
        {
            get
            {
                return "Grouping Variable";
            }
            
        }


         [Category("Control Settings"), PropertyOrder(2)]
          [Description("Required property. You must specify a unique name for every control dropped on the dialog. You will not be able to save a dialog definition unless every control in the dialog and contained sub-dialogs has a unique name.")]
       
        public new string Name
        {
            get { return base.Name; }
            set { base.Name = value; }
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
               // if (oneItemList != null) oneItemList.Width = value;
                base.Width = value;
            }
        }

        [Category("Layout Properties"), PropertyOrder(2)]
        [Description("Default value is the height of this control. To change, drag the adorners(corner of the control) or enter a height.")]
        public new double Height
        {
            get
            {
               // return oneItemList.Height;
                return base.Height;
            }
            set
            {
               // if (oneItemList != null) oneItemList.Height = value;
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


         [Category("Variable Settings"), PropertyOrder(1)]
         [Description("The default value of 'String|Numeric|Date|Ordinal|Nominal|Scale' indicates this grouping variable control allows variables of all types.Click the property and then click the ellipses button to restrict this control to one or more variable types.")]
        [Editor(@"BSky.Controls.DesignerSupport.VariableFilter, BSky.Controls, Culture=neutral",
                typeof(System.Drawing.Design.UITypeEditor))]
        public new string Filter
        {

            get
            {
               // return oneItemList.Filter;
                foreach (object child in this.Children)
                {
                    if (child is SingleItemList)
                    {
                        //vTargetList = child12 as DragDropList;
                        // targetListName = value;
                        //DragList child123;
                        SingleItemList child1 = child as SingleItemList;
                        return child1.Filter;
                    }
                    
                }
                return null;
            }
            set
            {
                //if (oneItemList != null) oneItemList.Filter = value;
                
                foreach (object child in this.Children)
                {
                    if (child is SingleItemList)
                    {
                        //vTargetList = child12 as DragDropList;
                        // targetListName = value;
                        //DragList child123;
                        SingleItemList child1 = child as SingleItemList;
                        child1.Filter = value;
                       child1.nomlevels = nomlevels;
                        child1.ordlevels = ordlevels;
                    }

                }

            }


        }


        [Category("Variable Settings"), PropertyOrder(2)]
        [DisplayName("Define Rules")]
        [Description("Default is empty(no rules). Use this optional property to define rules that trigger property changes in this or other controls, based on the changes in state of this grouping variable control. For example, to ensure that the dialog cannot be executed unless there is one variable in this grouping variable control, set the default value of the canexecute property for this control to false, then define a rule that triggers when the itemscount property is greater than 0 (This happens when a variable is dragged and dropped into this control) and sets the canexecute property to 'true' which enables the OK button on the dialog (remember to set another rule to set canexecute to false when the variable is removed from this control and the value of the itemscount property is 0). This ensures that the dialog cannot be executed unless there is one variable in this control. To define a rule, click in the property and then click the ellipses button.")]
        public BehaviourCollection SelectionChangeBehaviour
        {
            get
            {
                // return oneItemList.Filter;
                foreach (object child in this.Children)
                {
                    if (child is SingleItemList)
                    {
                        //vTargetList = child12 as DragDropList;
                        // targetListName = value;
                        //DragList child123;
                        SingleItemList child1 = child as SingleItemList;
                        return child1.SelectionChangeBehaviour;
                    }

                }
                return null;

            }
            set
            {
                //if (oneItemList != null) oneItemList.Filter = value;

                foreach (object child in this.Children)
                {
                    if (child is SingleItemList)
                    {
                        //vTargetList = child12 as DragDropList;
                        // targetListName = value;
                        //DragList child123;
                        SingleItemList child1 = child as SingleItemList;
                        child1.SelectionChangeBehaviour = value;
                    }

                }

            }
        }

        [Category("Control Settings"), PropertyOrder(5)]
        [Description("The number of variables in the grouping variable control. This is a read only control, its value is automatically set to the number of variables in the control. You use this property for defining rules. For example, if the grouping variable control is empty and you want the OK button disabled unless there are one variable in this control, you will define a rule that checks the ItemsCount property, if it is 0, set canexecute to false.")]
        public int ItemsCount
        {
            get
            {
                // return oneItemList.Filter;
                foreach (object child in this.Children)
                {
                    if (child is SingleItemList)
                    {
                        //vTargetList = child12 as DragDropList;
                        // targetListName = value;
                        //DragList child123;
                        SingleItemList child1 = child as SingleItemList;
                        return child1.ItemsCount;
                    }

                }
                return 0;

            }
           
        }

        private bool canExecute = true;

        [Category("Control Settings"), PropertyOrder(4)]
        [Description("Default value is True. This property controls whether the OK button on the dialog is enabled or disabled. If CanExecute =true for all controls on the dialog and contained sub-dialogs, the OK button is enabled, if CanExecute =false for any one control on the dialog or contained sub-dialogs, the OK button is disabled. For example, to ensure that the dialog cannot be executed unless one item is dragged and dropped into this control, set the default value of canexecute for this control to false, then define a rule that triggers when the itemscount property is greater than 0 (This happens when a variable is moved/copied into this control) and sets the canexecute property to 'true' which enables the OK button on the dialog. Remember to set another rule to set canexecute to false when the variable is removed from this control and the value of the itemscount property is 0). To define a rule, click in the property and then click the ellipses button.")]
        public bool CanExecute
        {

            get
            {
                // return oneItemList.Filter;
                foreach (object child in this.Children)
                {
                    if (child is SingleItemList)
                    {
                        //vTargetList = child12 as DragDropList;
                        // targetListName = value;
                        //DragList child123;
                        SingleItemList child1 = child as SingleItemList;
                        return child1.CanExecute;
                    }

                }
                return false;

            }

            set
            {
                //if (oneItemList != null) oneItemList.Filter = value;

                foreach (object child in this.Children)
                {
                    if (child is SingleItemList)
                    {
                        //vTargetList = child12 as DragDropList;
                        // targetListName = value;
                        //DragList child123;
                        SingleItemList child1 = child as SingleItemList;
                        child1.CanExecute = value;
                    }

                }

            }
            //get
            //{
            //    return canExecute;
            //}
            //set
            //{
            //    canExecute = value;
            //    if (CanExecuteChanged != null)
            //    {
            //        BSkyBoolEventArgs b = new BSkyBoolEventArgs();
            //        b.Value = value;
            //        CanExecuteChanged(this, b);
            //    }
            //}


        }

       


     //   public event EventHandler<BSkyBoolEventArgs> CanExecuteChanged;


          [CategoryAttribute("Syntax Settings")]
         [Description("Default value is NoPrefix|UseComma. This setting allows you to set how the variable contained in this grouping variable control will replace the control name in the syntax string. To see additional details, click in the property and click the ellipses button. ")]
        [Editor(@"BSky.Controls.DesignerSupport.ManageSubstitution, BSky.Controls,  Culture=neutral",
                typeof(System.Drawing.Design.UITypeEditor))]
        public string SubstituteSettings { 
            
            get
            {
                // return oneItemList.Filter;
                foreach (object child in this.Children)
                {
                    if (child is SingleItemList)
                    {
                        //vTargetList = child12 as DragDropList;
                        // targetListName = value;
                        //DragList child123;
                        SingleItemList child1 = child as SingleItemList;
                        return child1.SubstituteSettings;
                    }

                }
                return null;

            }

            set
            {
                //if (oneItemList != null) oneItemList.Filter = value;

                foreach (object child in this.Children)
                {
                    if (child is SingleItemList)
                    {
                        //vTargetList = child12 as DragDropList;
                        // targetListName = value;
                        //DragList child123;
                        SingleItemList child1 = child as SingleItemList;
                        child1.SubstituteSettings = value;
                    }

                }

            }
            
            
            }



         private bool _enabled = true;
         [Category("Control Settings"), PropertyOrder(6)]
         [Description("Default is True(enabled). This property controls whether the default state of this grouping variable control is enabled or disabled. For enabled, select True, for disabled select False. When enabled, you can drag & drop/move a variable list into this control, when disabled, you cannot interact with the control. For example, you may want the initial state of the control to be disabled, however you may want to enable it based on a selection made in another control")]
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


        // Aaron 09/16/2013
        //A simple way to access the singleitemlist oject
       // public SingleItemList oneItemList;
         public string PrefixTxt
         {
             get
             {
                 foreach (object child in this.Children)
                 {
                     if (child is SingleItemList)
                     {
                         //vTargetList = child12 as DragDropList;
                         // targetListName = value;
                         //DragList child123;
                         SingleItemList child1 = child as SingleItemList;
                         return child1.PrefixTxt;
                     }

                 }
                 return null;
             }
             set
             {
                 foreach (object child in this.Children)
                 {
                     if (child is SingleItemList)
                     {
                         //vTargetList = child12 as DragDropList;
                         // targetListName = value;
                         //DragList child123;
                         SingleItemList child1 = child as SingleItemList;
                         child1.PrefixTxt = value;
                     }

                 }
             }
         }

         public string SepCharacter
         {
             get
             {
                 {
                     foreach (object child in this.Children)
                     {
                         if (child is SingleItemList)
                         {
                             //vTargetList = child12 as DragDropList;
                             // targetListName = value;
                             //DragList child123;
                             SingleItemList child1 = child as SingleItemList;
                             return child1.SepCharacter;
                         }

                     }
                     return null;
                 }

             }
             set
             {
                 {
                     foreach (object child in this.Children)
                     {
                         if (child is SingleItemList)
                         {
                             //vTargetList = child12 as DragDropList;
                             // targetListName = value;
                             //DragList child123;
                             SingleItemList child1 = child as SingleItemList;
                             child1.SepCharacter = value;
                         }

                     }
                 }
             }
         }

        public string nomlevels { get; set; }
        public string ordlevels { get; set; }
        [Category("Syntax Settings")]
        [Description("Default value of %%VALUE%% indicates that the variables in this grouping variable control will be replaced by the control name in the syntax. These values will be used to parameterize the syntax string created when the dialog is executed. If you change the default value that value will be passed only when the control is empty, for example if you want 'test' to replace the control name when it is empty, replace %%VALUE%% with 'test' (you don't need to enter the single quotes) ")]
        public string Syntax
        {
            get;
            //{
            //    foreach (object child in this.Children)
            //    {
            //        if (child is SingleItemList)
            //        {
            //            //vTargetList = child12 as DragDropList;
            //            // targetListName = value;
            //            //DragList child123;
            //            SingleItemList child1 = child as SingleItemList;
            //            return child1.Syntax;
            //        }
            //        return null;
            //    }
            //    return null;
            //}
            set;
            //{
            //    foreach (object child in this.Children)
            //    {
            //        if (child is SingleItemList)
            //        {
            //            //vTargetList = child12 as DragDropList;
            //            // targetListName = value;
            //      

        }

    }
}
