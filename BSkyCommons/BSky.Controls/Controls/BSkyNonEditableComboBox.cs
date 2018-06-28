using System;
using System.Windows.Controls;
using System.ComponentModel;
using BSky.Interfaces.Controls;



namespace BSky.Controls
{
    [TypeConverter(typeof(PropertySorter))]
    [DefaultPropertyAttribute("Type")]
    public class BSkyNonEditableComboBox : ComboBox, IBSkyAffectsExecute, IBSkyInputControl, IBSkyControl, IBSkyEnabledControl
    {
        public BSkyNonEditableComboBox()
        {
            _DisplayItems = new StringCollection();
            //IsEditable = true;
            // List<string> DisplayItems = new System.Collections.Generic.List<string>();
            //System.Windows.Data.Binding b = new System.Windows.Data.Binding();
            //b.Source = base.ItemsSource;
            //this.SetBinding(b, DisplayItems);
          //  base.ItemsSource = _DisplayItems;
            // 12/31/2012 Aaron
            //Added the line below to support the text property
            // IsEditable = true;
            SelectionChangeBehaviour = new BehaviourCollection();
            Syntax = "%%VALUE%%";
            this.SelectedIndex = 0;

        }


        private StringCollection _DisplayItems;
        private string _SelectedObject;
        private string _NoItemsSelected = null;

        //[Description("A Non Editable ComboBox Control allows you to select a single item from predefined items. This is a read only property. Click on each property in the grid to see the configuration options for the combobox control.")]
        [BSkyLocalizedDescription("BSkyNonEditableComboBox_TypeDescription", typeof(BSky.GlobalResources.Properties.Resources))]
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
        [BSkyLocalizedDescription("BSkyNonEditableComboBox_NameDescription", typeof(BSky.GlobalResources.Properties.Resources))]
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

        //Added by Aaron 05/01/2015
        //Non editable combo boxes cannot have a default property
        //THey will always display empty


        //[Category("Control Settings"), PropertyOrder(3)]
        //[Description("Optional property. This is the item that will be displayed by default in the combobox. ")]
        //public new string DefaultSelection
        //{
        //    get
        //    {
        //        return base.Text;
        //    }
        //    set
        //    {
        //        base.Text = value;
        //    }
        //}


        //Aaron Added 01/01/2013
        //Added a new property to specify the value that should be passed if no selection is made

        [Category("Control Settings"), PropertyOrder(4)]
        //[Description("Default value is the empty string. This is the value that substitutes the control name in the syntax when no items are selected in the combobox. ")]
        [BSkyLocalizedDescription("BSkyNonEditableComboBox_NothingSelectedDescription", typeof(BSky.GlobalResources.Properties.Resources))]
        public new string NothingSelected
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
        [BSkyLocalizedDescription("BSkyNonEditableComboBox_WidthDescription", typeof(BSky.GlobalResources.Properties.Resources))]
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
        [BSkyLocalizedDescription("BSkyNonEditableComboBox_HeightDescription", typeof(BSky.GlobalResources.Properties.Resources))]
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
        [BSkyLocalizedDescription("BSkyNonEditableComboBox_LeftDescription", typeof(BSky.GlobalResources.Properties.Resources))]
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
        [BSkyLocalizedDescription("BSkyNonEditableComboBox_TopDescription", typeof(BSky.GlobalResources.Properties.Resources))]
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



        [Category("Control Settings"), PropertyOrder(5)]
        [DisplayName("ComboBox Entries")]
        //[Description("Click the property name and then click the lookup button to enter entries that will display when you click the combobox control")]
        [BSkyLocalizedDescription("BSkyNonEditableComboBox_ComboBoxEntriesDescription", typeof(BSky.GlobalResources.Properties.Resources))]
        [Editor(@"BSky.Controls.DesignerSupport.ComboBoxEditor, AnalyticsUnlimited.Controls, Culture=neutral",
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


//        [Category("Control Settings"), PropertyOrder(5)]
//        [Editor(@"System.Windows.Forms.Design.StringCollectionEditor, 
//System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a",
//typeof(System.Drawing.Design.UITypeEditor))]
//        [DisplayName("ComboBox Entries")]
//        [Description("Click the property name and then click the lookup button to enter entries that will display when you click the combobox control")]
//        // 12/31 Aaron: created private _DisplayItems and set base.ItemsSource to the private property _DisplayItems
//        // Aaron: I need to determine why we need to set base.ItemsSource = _DisplayItems; in the constructor and in the 
//        // code below
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

//                //if (DefaultSelection != null)
//                //{
//                //    int index = this.DisplayItems.IndexOf(DefaultSelection);
//                //    if (index > -1)
//                //    {
//                //        this.SelectedIndex = index;

//                //    }
//                //}
//            }



//        }

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
        //[Description("Default is empty(no rules). Use this optional property to define rules that trigger property changes in this or other controls, based on a change in the item selected in this combobox control. For example, to ensure that the dialog cannot be executed unless one item is selected in this combobox control, set the default value of the canexecute property for this control to false, then define a rule that triggers when an item is selected that sets the canexecute property to 'true' which enables the OK button on the dialog. This ensures that the dialog cannot be executed unless there is one item selected in this control. To define a rule, click in the property and then click the ellipses button.")]
        [BSkyLocalizedDescription("BSkyNonEditableComboBox_SelectionChangeBehaviourDescription", typeof(BSky.GlobalResources.Properties.Resources))]

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
        //[Description("Default value is True. This property controls whether the OK button on the dialog is enabled or disabled. If CanExecute =true for all controls on the dialog and contained sub-dialogs, the OK button is enabled, if CanExecute =false for any one control on the dialog or contained sub-dialogs, the OK button is disabled. For example, to ensure that the dialog cannot be executed unless a selection is made in this combobox control, set the default value of canexecute for this control to false, then define a rule that triggers when a selection is made and sets the canexecute property to 'true' which enables the OK button on the dialog. To define a rule, click in the property and then click the ellipses button.")]
        [BSkyLocalizedDescription("BSkyNonEditableComboBox_CanExecuteDescription", typeof(BSky.GlobalResources.Properties.Resources))]
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
        [BSkyLocalizedDescription("BSkyNonEditableComboBox_EnabledDescription", typeof(BSky.GlobalResources.Properties.Resources))]
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

        [Category("Syntax Settings")]
        //[Description("Default value of %%VALUE%% indicates that the selected item in the combobox control will be replaced by the control name in the syntax. These values will be used to parameterize the syntax string created when the dialog is executed. If you want a different value, for example 'test' to replace the control name, replace %%VALUE%% with 'test' (you don't need to enter the single quotes) ")]
        [BSkyLocalizedDescription("BSkyNonEditableComboBox_SyntaxDescription", typeof(BSky.GlobalResources.Properties.Resources))]
        public string Syntax
        {
            get;
            set;
        }

        #endregion
    }
}

