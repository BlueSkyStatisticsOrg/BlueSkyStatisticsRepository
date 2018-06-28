using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.ComponentModel;
using BSky.Interfaces.Controls;

namespace BSky.Controls
{

    public class ListBoxEntry
    {
        public string entryName { get; set; }
    }


    public class ListBoxValueCollection : List<ListBoxEntry>
    {
    }

     
    public class CtrlListBox : ListBoxwithBorder, IBSkyControl, IBSkyInputControl, IBSkyAffectsExecute, IBSkyEnabledControl
    {
         
        public CtrlListBox()
        {
            Syntax = "%%VALUE%%";
            SubstituteSettings = "UseComma";
            SelectionChangeBehaviour = new BehaviourCollection();
        }


      

        [Category("Variable Settings"), PropertyOrder(2)]
        [DisplayName("Define Rules")]
        //[Description("Default is empty(no rules). Use this optional property to define rules that trigger property changes in this or other controls, based on the changes in state of this listbox control. To define a rule, click in the property and then click the ellipses button.")]
        [BSkyLocalizedDescription("CtrlListBox_SelectionChangeBehaviourDescription", typeof(BSky.GlobalResources.Properties.Resources))]
        //  [Category("Variable Settings")]
        public BehaviourCollection SelectionChangeBehaviour
        {
            get;
            set;
        }


        [Category("Control Settings"), PropertyOrder(2)]
        //[Description("Required property. You must specify a unique name for every control added to the dialog. You will not be able to save a dialog definition unless every control on the dialog and containing sub-dialogs has a unique name. ")]
        [BSkyLocalizedDescription("CtrlListBox_NameDescription", typeof(BSky.GlobalResources.Properties.Resources))]
        public new string Name
        {
            get { return base.Name; }
            set { base.Name = value; }
        }


        [Category("Control Settings"), PropertyOrder(3)]
        //[Description("Default is false(you can only select a single item). This property controls whether you can select a single item or multiple items in this control. You can select a single item if MultiSelect =false, multiple items if MultiSelect =true.")]
        [BSkyLocalizedDescription("CtrlListBox_MultiSelectDescription", typeof(BSky.GlobalResources.Properties.Resources))]
        public bool MultiSelect
        {
            get
            {
                if (base.SelectionMode == SelectionMode.Extended)
                    return true;
                else return false;
            }
            set
            {
                if (value == true)
                    base.SelectionMode = SelectionMode.Extended;
                else base.SelectionMode = SelectionMode.Single;

            }
        }
        


        [Category("Control Settings"), PropertyOrder(4)]
        //[Description("This property allows you to enter the items that will be displayed in this listbox. Click in the property and then click the button to enter values.")]
        [BSkyLocalizedDescription("CtrlListBox_ListBoxEntriesDescription", typeof(BSky.GlobalResources.Properties.Resources))]
        [Editor(@"BSky.Controls.DesignerSupport.ListBoxEditor, BSky.Controls, Culture=neutral",
                typeof(System.Drawing.Design.UITypeEditor))]
        public ListBoxValueCollection ListBoxEntries
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


        private bool canExecute = true;
        public event EventHandler<BSkyBoolEventArgs> CanExecuteChanged;


        [Category("Control Settings"), PropertyOrder(7)]
        //[Description("This property keep tracks off the number of items that are selected. You can use this property to define rules. This is a readonly property.")]
        [BSkyLocalizedDescription("CtrlListBox_SelectedItemsCountDescription", typeof(BSky.GlobalResources.Properties.Resources))]
        public int SelectedItemsCount
        {
            get
            {
                return this.SelectedItems.Count;
            }
           
        }

        private bool _enabled = true;
        [Category("Control Settings"), PropertyOrder(8)]
        //[Description("Default is True(enabled). This property controls whether the default state of this variable list control is enabled or disabled. For enabled, select True, for disabled select False. When enabled, you can select items in this control, when disabled, you cannot interact with the control. For example, you may want the initial state of the variable list control to be disabled, however you may want to enable it based on a selection made in another control")]
        [BSkyLocalizedDescription("CtrlListBox_EnabledDescription", typeof(BSky.GlobalResources.Properties.Resources))]
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

        // [Category("Control Settings"), PropertyOrder(2)]
        [Category("Control Settings"), PropertyOrder(6)]
        //[Description("Default value is True. This property controls whether the OK button on the dialog is enabled or disabled. If CanExecute =true for all controls on the dialog and contained sub-dialogs, the OK button is enabled, if CanExecute =false for any one control on the dialog or contained sub-dialogs, the OK button is disabled.")]
        [BSkyLocalizedDescription("CtrlListBox_CanExecuteDescription", typeof(BSky.GlobalResources.Properties.Resources))]
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


        [CategoryAttribute("Syntax Settings")]
        //[Description("Default value is UseComma. This setting allows you to set how the variables contained in this variable list control will replace the control name in the syntax string. To see additional details, click in the property and click the ellipses button. ")]
        [BSkyLocalizedDescription("CtrlListBox_SubstituteSettingsDescription", typeof(BSky.GlobalResources.Properties.Resources))]
        [Editor(@"BSky.Controls.DesignerSupport.ManageListBoxSubstitution, BSky.Controls,  Culture=neutral",
                typeof(System.Drawing.Design.UITypeEditor))]
        public string SubstituteSettings { get; set; } 



        [Category("Layout Properties"), PropertyOrder(1)]
        //[Description("Default value is the width of this control. To change drag the adorners(corner of the control) or enter a width.")]
        [BSkyLocalizedDescription("CtrlListBox_WidthDescription", typeof(BSky.GlobalResources.Properties.Resources))]
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
        [BSkyLocalizedDescription("CtrlListBox_HeightDescription", typeof(BSky.GlobalResources.Properties.Resources))]
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
        [BSkyLocalizedDescription("CtrlListBox_LeftDescription", typeof(BSky.GlobalResources.Properties.Resources))]
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
        [BSkyLocalizedDescription("CtrlListBox_TopDescription", typeof(BSky.GlobalResources.Properties.Resources))]
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


          [Category("Syntax Settings"), PropertyOrder(1)]

          //[Description("Default value of %%VALUE%% indicates that all the items in the listbox will be replaced by the control name in the syntax. These values will be used to parameterize the syntax string created when the dialog is executed. If you want a different value, for example 'test' to replace the control name, replace %%VALUE%% with 'test' (you don't need to enter the single quotes) ")]
        [BSkyLocalizedDescription("CtrlListBox_SyntaxDescription", typeof(BSky.GlobalResources.Properties.Resources))]
        public string Syntax
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

             // if (!renderVars)
              //{
                  base.OnSelectionChanged(e);
                  BSkyCanvas parent = UIHelper.FindVisualParent<BSkyCanvas>(this);
                  if (parent != null && SelectionChangeBehaviour != null)
                      parent.ApplyBehaviour(this, SelectionChangeBehaviour);
              //}
          }



    }
}
