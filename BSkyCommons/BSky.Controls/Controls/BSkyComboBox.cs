using System;
using System.Windows.Controls;
using System.ComponentModel;
using BSky.Interfaces.Controls;
//using System.Windows.Data.Binding;

namespace BSky.Controls
{
    [TypeConverter(typeof(PropertySorter))]
    [DefaultPropertyAttribute("Type")]
    public class BSkyComboBox : ComboBox, IBSkyAffectsExecute, IBSkyInputControl, IBSkyControl, IBSkyEnabledControl
    {
        public BSkyComboBox()
        {
           _DisplayItems = new StringCollection();
           
           // List<string> DisplayItems = new System.Collections.Generic.List<string>();
           //System.Windows.Data.Binding b = new System.Windows.Data.Binding();
           //b.Source = base.ItemsSource;
           //this.SetBinding(b, DisplayItems);
           base.ItemsSource = _DisplayItems;
            // 12/31/2012 Aaron
            //Added the line below to support the text property
          // IsEditable = true;
            SelectionChangeBehaviour = new BehaviourCollection();
            Syntax = "%%VALUE%%";

        }


        private StringCollection _DisplayItems;
        private string _SelectedObject;
        private string _NoItemsSelected =null;

        //[Description("A ComboBox Control is a combination of a drop-down list or list box and a single-line editable textbox. A combobox control allows the user to either type a value directly into the control or pick a value from a listbox.This is a read only property. Click on each property in the grid to see the configuration options for the combobox control.")]
        [BSkyLocalizedDescription("BSkyComboBox_TypeDescription", typeof(BSky.GlobalResources.Properties.Resources))]
        [Category("Control Settings"), PropertyOrder(1)]
        public string Type
        {
            get
            {
                return "ComboBox Control";
            }
        }

        [Category("Control Settings"), PropertyOrder(2)]
        //[Description("Required property. You must specify a unique name for every control added to the dialog. You will not be able to save a dialog definition unless every control on the dialog and containing sub-dialogs has a unique name. ")]
        [BSkyLocalizedDescription("BSkyComboBox_NameDescription", typeof(BSky.GlobalResources.Properties.Resources))]
        public new string Name
        {
            get
            {
                return base.Name;
            }
            set
            {
                base.Name = value;
            }
        }

        [Category("Control Settings"), PropertyOrder(3)]
        //[Description("Optional property. This is the item that will be displayed by default in the combobox. ")]
        [BSkyLocalizedDescription("BSkyComboBox_DefaultSelectionDescription", typeof(BSky.GlobalResources.Properties.Resources))]
        public new string DefaultSelection
        {
            get
            {
                return base.Text;
            }
            set
            {
                base.Text = value;
            }
        }


        //Aaron Added 01/01/2013
        //Added a new property to specify the value that should be passed if no selection is made

         [Category("Control Settings"), PropertyOrder(4)]
        // [Description("This is a read only property. Its value gets set automatically to the number of items in the combobox that are selected. This property can be used for defining rules. For example, you want the OK button disabled unless there are one or more items selected in the combobox. If nothing is selected in the combobox you define a rule that checks the NoItemsSelected property, if it is 0, set canexecute to false.")]
        [BSkyLocalizedDescription("BSkyComboBox_NoItemsSelectedDescription", typeof(BSky.GlobalResources.Properties.Resources))]
        public new string NoItemsSelected
        {
            get
            {
                return _NoItemsSelected;
            }
            set
            {
                _NoItemsSelected = value;
            }
        }



        [Category("Layout Properties"), PropertyOrder(1)]
        //[Description("Default value is the width of this control. To change drag the adorners(corner of the control) or enter a width.")]
        [BSkyLocalizedDescription("BSkyComboBox_WidthDescription", typeof(BSky.GlobalResources.Properties.Resources))]
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
       //[Description("Default value is the height of this control. To change, drag the adorners(corner of the control) or enter a height.")]
        [BSkyLocalizedDescription("BSkyComboBox_HeightDescription", typeof(BSky.GlobalResources.Properties.Resources))]
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
       //[Description("Default value is the X coordinate of the top left corner of this control. To change, drag the control to a different position or enter a X coordinate.")]
        [BSkyLocalizedDescription("BSkyComboBox_LeftDescription", typeof(BSky.GlobalResources.Properties.Resources))]
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
       //[Description("Default value is the Y coordinate of the top left corner of this control. To change drag the control to a different position or enter a Y coordinate.")]
        [BSkyLocalizedDescription("BSkyComboBox_TopDescription", typeof(BSky.GlobalResources.Properties.Resources))]
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






//        [Category("Control Settings"), PropertyOrder(5)]
//        [Editor(@"System.Windows.Forms.Design.StringCollectionEditor, 
//System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a",
//typeof(System.Drawing.Design.UITypeEditor))]
//        [DisplayName("ComboBox Entries")]
//        [Description("Click the property name and then click the lookup button to enter entries that will display when you click the combobox control")]
//        // 12/31 Aaron: created private _DisplayItems and set base.ItemsSource to the private property _DisplayItems
//        // Aaron: I need to determine why we need to set base.ItemsSource = _DisplayItems; in the constructor and in the 
//            // code below
//        public StringCollection DisplayItems
//        {
//            get
//            {
//                return _DisplayItems;   
//            }

           
//            set
//            {
//                _DisplayItems = value;
//                base.ItemsSource = _DisplayItems;
//                //The code below, we are selecting the specified value
                
//                if (DefaultSelection != null)
//                {
//                    int index = this.DisplayItems.IndexOf(DefaultSelection);
//                    if (index > -1)
//                    {
//                        this.SelectedIndex = index;
                       
//                    }
//                }
//            }

         

//        }






       [Category("Control Settings"), PropertyOrder(5)]
       [DisplayName("ComboBox Entries")]
       //[Description("Click the property name and then click the lookup button to enter entries that will display when you click the combobox control")]
        [BSkyLocalizedDescription("BSkyComboBox_ComboBoxEntriesDescription", typeof(BSky.GlobalResources.Properties.Resources))]

        [Editor(@"BSky.Controls.DesignerSupport.ComboBoxEditor, AnalyticsUnlimited.Controls,  Culture=neutral",
               typeof(System.Drawing.Design.UITypeEditor))]
       public ComboBoxValueCollection ComboBoxEntries
       {
           get
           {

               return null;
               // return _radioButtons;
           }
           set
           {
               //01/03/2013
               //Added line below
               //_radioButtons = value;
           }
       }



        // 01/01/2012 This code is no longer necessary
        //[Category("BlueSky")]
        //public new string SelectedObject
        //{
        //    get
        //    {
        //        if (this.SelectedValue != null)
        //        {
        //            _SelectedObject = this.SelectedValue.ToString();
        //            return _SelectedObject;
        //        }
        //        else
        //        {
                 
        //            return _SelectedObject;
        //        }
        //    }
        //    set
        //    {
            
        //        _SelectedObject = value;
        //    }
        //}

        [Category("Control Settings"), PropertyOrder(6)]

        
        [DisplayName("Define Rules")]
        //[Description("Default is empty(no rules). Use this optional property to define rules that trigger property changes in this or other controls, based on the changes in the items selected in this combobox control. For example, to ensure that the dialog cannot be executed unless one or more items are selected in this combobox control, set the default value of the canexecute property for this control to false, then define a rule that triggers when the NoItemsSelected property is greater than 0 (This happens when one or more items are selected in this combobox control) and sets the canexecute property to 'true' which enables the OK button on the dialog. (Remember to set another rule to set canexecute to false when all items are deselected from this combobox control and the value of the itemscount property is 0). This ensures that the dialog cannot be executed unless there are one or more items selected in this control. To define a rule, click in the property and then click the ellipses button.")]
        [BSkyLocalizedDescription("BSkyComboBox_SelectionChangeBehaviourDescription", typeof(BSky.GlobalResources.Properties.Resources))]

        public BehaviourCollection SelectionChangeBehaviour
        {
            get;
            set;
        }

        protected override void OnSelectionChanged(SelectionChangedEventArgs e)
        {
            base.OnSelectionChanged(e);
            BSkyCanvas parent = UIHelper.FindVisualParent<BSkyCanvas>(this);
            //Added by Aaron 06/10/2014
            //Added the condition BSkyCanvas.applyBehaviors == true to make sure that apply behavior does not get called
            //in dialog editor mode
            //This is because we don't want events getting firect in dialog editor mode
            //This is because it may change properties of other controls whose initial values we want to preserve
            //We only want behaviors to fire after the dialog has been fully loaded.
            if (parent != null && SelectionChangeBehaviour != null && BSkyCanvas.applyBehaviors == true)
                parent.ApplyBehaviour(this, SelectionChangeBehaviour);
        }

        private bool canExecute = true;

        [Category("Control Settings"), PropertyOrder(7)]
        //[Description("Default value is True. This property controls whether the OK button on the dialog is enabled or disabled. If CanExecute =true for all controls on the dialog and contained sub-dialogs, the OK button is enabled, if CanExecute =false for any one control on the dialog or contained sub-dialogs, the OK button is disabled. For example, to ensure that the dialog cannot be executed unless one or more items are selected in this combobox control, set the default value of canexecute for this control to false, then define a rule that triggers when the NoItemsSelected property is greater than 0 (This happens when one or more variables are selected in this combobox control) and sets the canexecute property to 'true' which enables the OK button on the dialog . Remember to set another rule to set canexecute to false when All items are deselected from this control and the value of the NoItemsSelected property is 0).  To define a rule, click in the property and then click the ellipses button.")]
        [BSkyLocalizedDescription("BSkyComboBox_CanExecuteDescription", typeof(BSky.GlobalResources.Properties.Resources))]

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

        private bool _enabled = true;
        [Category("Control Settings"), PropertyOrder(8)]
        //[Description("Default is True(enabled). This property controls whether the default state of this combobox control is enabled or disabled. For enabled, select True, for disabled select False. When enabled, you can select items in this control, when disabled, you cannot interact with the control. For example, you may want the initial state of the combobox control to be disabled, however you may want to enable it based on a selection made in another control")]
        [BSkyLocalizedDescription("BSkyComboBox_EnabledDescription", typeof(BSky.GlobalResources.Properties.Resources))]

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

        public event EventHandler<BSkyBoolEventArgs> CanExecuteChanged;

        #region IBSkyInputControl Members
        
        [Category("BlueSky")]
        //[Description("Default value of %%VALUE%% indicates that all the selected items in the combobox control will be replaced by the control name in the syntax. These values will be used to parameterize the syntax string created when the dialog is executed. If you want a different value, for example 'test' to replace the control name, replace %%VALUE%% with 'test' (you don't need to enter the single quotes) ")]
        [BSkyLocalizedDescription("BSkyComboBox_SyntaxDescription", typeof(BSky.GlobalResources.Properties.Resources))]
        public string Syntax
        {
            get;
            set;
        }

        #endregion
    }
}
