using System;
using System.ComponentModel;
using BSky.Interfaces.Controls;
using System.Windows.Controls;

namespace BSky.Controls
{

    [TypeConverter(typeof(PropertySorter))]
    [DefaultPropertyAttribute("Type")]
    public class BSkyTargetList : DragDropList, IBSkyInputControl, IBSkyControl, IBSkyEnabledControl, IBSkyAffectsExecute
    {
        public BSkyTargetList()
        {
            SelectionChangeBehaviour = new BehaviourCollection();
        }
        public BSkyTargetList(bool SourceList, bool dialogDesigner)
        {
            base.AutoVar = !SourceList;

            base.renderVars = dialogDesigner;

            Syntax = "%%VALUE%%";
            SelectionChangeBehaviour = new BehaviourCollection();
            // if (SourceList==1)
            if (SourceList && !dialogDesigner)
            {
                if (this.ItemsCount > 0) this.SelectedIndex = 0;
                this.Focus();
            }
        }


        [Category("Variable Settings"), PropertyOrder(4)]
        [DisplayName("Define Rules")]
        //[Description("Default is empty(no rules). Use this optional property to define rules that trigger property changes in this or other controls, based on the changes in state of this variable list control. For example, to ensure that the dialog cannot be executed unless one or more items are dragged and dropped into this variable list control, set the default value of the canexecute property for this control to false, then define a rule that triggers when the itemscount property is greater than 0 (This happens when one or more variables are dragged and droped into this variable control) and sets the canexecute property to 'true' which enables the OK button on the dialog (remember to set another rule to set canexecute to false when All items are removed from this variable list and the value of the itemscount property is 0). This ensures that the dialog cannot be executed unless there are one or more items in this variable list control. To define a rule, click in the property and then click the elipses button.")]
        [BSkyLocalizedDescription("BSkyTargetList_SelectionChangeBehaviourDescription", typeof(BSky.GlobalResources.Properties.Resources))]
        //  [Category("Variable Settings")]
        public BehaviourCollection SelectionChangeBehaviour
        {
            get;
            set;
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

            if (!renderVars)
            {
                base.OnSelectionChanged(e);
                BSkyCanvas parent = UIHelper.FindVisualParent<BSkyCanvas>(this);
                if (parent != null && SelectionChangeBehaviour != null && BSkyCanvas.applyBehaviors == true)
                    parent.ApplyBehaviour(this, SelectionChangeBehaviour);
            }
        }


        //private void ListBox_MouseMove(object sender, MouseButtonEventArgs e)
        //{
        //    if (e.LeftButton == MouseButtonState.Pressed)
        //    {
        //        object data = ((ListBox)(FrameworkElement)sender).SelectedItem;
        //        if (data != null)
        //            DragDrop.DoDragDrop(this, data, DragDropEffects.Copy);
        //    }
        //}



        //  [ReadOnly(true)]
        [Category("Control Settings"), PropertyOrder(1)]
        //[Description("The destination variable list contains the variables you want to analyze. Variables are copied/moved from the source variable list to the destination variable list. This is a read only property. Click on each property in the grid to see the configuration options for the destination variable list control.")]
        [BSkyLocalizedDescription("BSkyTargetList_TypeDescription", typeof(BSky.GlobalResources.Properties.Resources))]
        // [DisplayName("This is the source")]
        public string Type
        {
            get
            {
                if (!base.AutoVar)
                {
                    // DisplayNameGridProperty(this, Type, "rain in spain");
                    return "Source Variable List Control";
                }
                else
                {
                    //DisplayNameGridProperty(this, Type, "rain in the drain spain");
                    return "Destination Variable List Control";
                }
            }
        }

        private bool canExecute = true;

        // [Category("Control Settings"), PropertyOrder(2)]
        [Category("Control Settings"), PropertyOrder(3)]
        //[Description("Default value is True. This property controls whether the OK button on the dialog is enabled or disabled. If CanExecute =true for all controls on the dialog and contained sub-dialogs, the OK button is enabled, if CanExecute =false for any one control on the dialog or contained sub-dialogs, the OK button is disabled. For example, to ensure that the dialog cannot be executed unless one or more items are dragged and dropped into this variable list control, set the default value of canexecute for this control to false, then define a rule that triggers when the itemscount property is greater than 0 (This happens when one or more variables are dragged and droped into this variable control) and sets the canexecute property to 'true' which enables the OK button on the dialog . Remember to set another rule to set canexecute to false when All items are removed from this variable list and the value of the itemscount property is 0).  To define a rule, click in the property and then click the elipses button.")]
        [BSkyLocalizedDescription("BSkyTargetList_CanExecuteDescription", typeof(BSky.GlobalResources.Properties.Resources))]
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


        #region IBSkyInputControl Members

        [CategoryAttribute("Syntax Settings")]
        //[Description("Default value of %%VALUE%% indicates that all the variables in the variable list control will be replaced by the control name in the syntax. These values will be used to parameterize the syntax string created when the dialog is executed. If you want a different value, for example 'test' to replace the control name, replace %%VALUE%% with 'test' (you don't need to enter the single quotes) ")]
        [BSkyLocalizedDescription("BSkyTargetList_SyntaxDescription", typeof(BSky.GlobalResources.Properties.Resources))]
        public string Syntax
        {
            get;
            set;
        }

        #endregion


    }
}
