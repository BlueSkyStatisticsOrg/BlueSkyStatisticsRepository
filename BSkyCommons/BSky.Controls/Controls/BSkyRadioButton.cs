using System;
using System.Windows.Controls;
using System.ComponentModel;
using BSky.Interfaces.Controls;
using System.Windows.Media;

namespace BSky.Controls
{
    [TypeConverter(typeof(PropertySorter))]
    [DefaultPropertyAttribute("Type")]
    public class BSkyRadioButton : RadioButton, IBSkyAffectsExecute, IBSkyInputControl, IBSkyControl, IBSkyEnabledControl
    {
        public BSkyRadioButton()
        {
            CheckedChangeBehaviour = new BehaviourCollection();
            //Standardizing the radio button font type and font sizes to protect against changes in themes
            this.FontFamily = new FontFamily("Segoe UI");
            this.FontSize = 12;
            
            this.FontSize = 12;
            Syntax = "%%VALUE%%";
            this.SetResourceReference(StyleProperty, "RadioButtonTemplate");
        }

        
        
       
[Category("Control Settings"), PropertyOrder(1)]
//[Description("RadioButton Control. This is a read only property. Click on each property in the grid to see the configuration options for the radiobutton control.")]
        [BSkyLocalizedDescription("BSkyRadioButton_TypeDescription", typeof(BSky.GlobalResources.Properties.Resources))]
        public string Type
        {
            get
            {
                 return "RadioButton Control";
            }
        }


 // [Description("This is a required property. This is the name of the radio group control that contains the radiobutton. All radiobuttons in the same radio group control must have the same name. For example radiobuttons A, B and C belong to the same radio group control, if B is selected, and you then click on A, A gets selected and B deselected.")]
        [BSkyLocalizedDescription("BSkyRadioButton_GroupNameDescription", typeof(BSky.GlobalResources.Properties.Resources))]

        [Category("Control Settings"), PropertyOrder(4)]
        public new string GroupName
        {
            get
            {
                return base.GroupName;
            }
            set
            {
                base.GroupName = value;
                //BSkyCanvas parent1 = UIHelper.FindVisualParent<BSkyCanvas>(this);

                //System.Windows.FrameworkElement element1 = parent1.FindName(value) as System.Windows.FrameworkElement;
                //if (element1 != null)
                //{
                //    BSkyRadioGroup radioGroup = element1 as BSkyRadioGroup;
                //    StackPanel stack1 = radioGroup.Content as StackPanel;
                //    stack1.Children.Add(this);
                //}
            }
        }

        [Category("Control Settings"), PropertyOrder(2)]
        //[Description("Required property. You must specify a unique name for every control added to the dialog. You will not be able to save a dialog definition unless every control on the dialog and containing sub-dialogs has a unique name. ")]
        [BSkyLocalizedDescription("BSkyRadioButton_NameDescription", typeof(BSky.GlobalResources.Properties.Resources))]
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
        //[Description("Optional property. Enter a caption for this radiobutton.")]
        [BSkyLocalizedDescription("BSkyRadioButton_TextDescription", typeof(BSky.GlobalResources.Properties.Resources))]
        public string Text
        {
            get
            {
                return base.Content as string;
            }
            set
            {
                base.Content = value as string;
            }
        }

            [Category("Layout Properties")]
           // [Description("Default value is the width of this control. To change drag the adorners(corner of the control) or enter a width.")]
        [BSkyLocalizedDescription("BSkyRadioButton_WidthDescription", typeof(BSky.GlobalResources.Properties.Resources))]
        [Browsable(false)]
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

            [Category("Layout Properties")]
        //[Description("Default value is the X coordinate of the top left corner of this control. To change, drag the control to a different position or enter a X coordinate.")]
        [BSkyLocalizedDescription("BSkyRadioButton_LeftDescription", typeof(BSky.GlobalResources.Properties.Resources))]
        [Browsable(false)]
        public double Left
        {
            get;
            set;
        }

             [Category("Layout Properties")]
        //[Description("Default value is the Y coordinate of the top left corner of this control. To change drag the control to a different position or enter a Y coordinate.")]
        [BSkyLocalizedDescription("BSkyRadioButton_TopDescription", typeof(BSky.GlobalResources.Properties.Resources))]
        [Browsable(false)]
        public double Top
        {
            get;
            set;
        }

           [Category("Layout Properties")]
          // [Description("Default value is the height of this control. To change, drag the adorners(corner of the control) or enter a height.")]
        [BSkyLocalizedDescription("BSkyRadioButton_HeightDescription", typeof(BSky.GlobalResources.Properties.Resources))]
        [Browsable(false)]
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

         [Category("Control Settings"), PropertyOrder(4)]
        // [Description("Default value is False(unchecked). This property determines whether the radiobutton is checked or unchecked.  For checked, select True, for unchecked, select False")]
        [BSkyLocalizedDescription("BSkyRadioButton_IsSelectedDescription", typeof(BSky.GlobalResources.Properties.Resources))]
        public bool IsSelected
        {
            //Commented 01/20
            // get { return IsChecked.HasValue ? IsChecked.Value : false; ; }
            //added lines below
            get
            {
                if (IsChecked.HasValue) return IsChecked.Value;
                else return false;
            }
            set { IsChecked = value; }
        }

          [Category("Control Settings"), PropertyOrder(5)]
          [DisplayName("Define Rules")]
        //[Description("Default is empty(no rules). Use this optional property to define rules that trigger property changes in this or other controls, based on the changes in state of this radiobutton control. For example, lets assume that a certain textbox control needs to be enabled when this radiobutton is selected, you will set the default state of this radio button as unselected and then define a rule that gets triggered when the radiobutton is selected (the radiobuttons isselected property is set to true) and sets the Enabled property on the textbox control to true.  To define a rule, click in the property and then click the ellipses button.")]
        [BSkyLocalizedDescription("BSkyRadioButton_CheckedChangeBehaviourDescription", typeof(BSky.GlobalResources.Properties.Resources))]

        public BehaviourCollection CheckedChangeBehaviour
        {
            get;
            set;
        }

        protected override void OnChecked(System.Windows.RoutedEventArgs e)
        {
            base.OnChecked(e);
            BSkyCanvas parent = UIHelper.FindVisualParent<BSkyCanvas>(this);
            //Added by Aaron 06/10/2014
            //Added the condition BSkyCanvas.applyBehaviors == true to make sure that apply behavior does not get called
            //in dialog editor mode
            //This is because we don't want events getting firect in dialog editor mode
            //This is because it may change properties of other controls whose initial values we want to preserve
            //We only want behaviors to fire after the dialog has been fully loaded.
            if (parent != null && CheckedChangeBehaviour != null && BSkyCanvas.applyBehaviors == true)
                parent.ApplyBehaviour(this, CheckedChangeBehaviour);
        }


        //Added by Aaron 12/25/2012
        //This code is not required as it is handled in window1.xaml.cs
        // THe function name is void b_KeyDown(object sender, KeyEventArgs e)

        //protected  void OnPreviewKeyUp(System.Windows.Input.KeyEventArgs e)
        //{
        //    if (e.KeyboardDevice.FocusedElement.GetType().Name == "BSkyRadioButton")
        //    {
        //        DependencyObject parentObject = VisualTreeHelper.GetParent(this);

        //        if (parentObject == null)
        //        {
        //            base.OnKeyUp(e);
        //            return;
        //        }
        //        StackPanel parent = parentObject as StackPanel;
        //        string parentName = this.GroupName;
        //        if (parent != null)
        //        {
        //            parent.Children.Remove(this);

        //            FrameworkElement fe = parentObject as FrameworkElement;

        //            BSkyRadioGroup element = fe.FindName(parentName) as BSkyRadioGroup;
        //            element._radioButtons.Remove(this);
        //        }

        //        base.OnKeyUp(e);
        //        return;
        //    }
        //}


        private bool canExecute = true;

        [Category("Control Settings"), PropertyOrder(6)]
        //[Description("Default value is True. This property controls whether the OK button on the dialog is enabled or disabled. If CanExecute =true for all controls on the dialog and contained sub-dialogs, the OK button is enabled, if CanExecute =false for any one control on the dialog or contained sub-dialogs, the OK button is disabled.  To define a rule, click in the property and then click the ellipses button.")]
        [BSkyLocalizedDescription("BSkyRadioButton_CanExecuteDescription", typeof(BSky.GlobalResources.Properties.Resources))]
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
        [Category("Control Settings"), PropertyOrder(6)]
        // [Description("Default is True(enabled). This property controls whether the default state of this radiobutton control is enabled or disabled. For enabled, select True, for disabled select False. When enabled, you can select this radiobutton, when disabled, you cannot select the radiobutton.")]
        [BSkyLocalizedDescription("BSkyRadioButton_EnabledDescription", typeof(BSky.GlobalResources.Properties.Resources))]
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
        //[Description("Default value of %%VALUE%% indicates that the name of the radiogroup control that this radiobutton belongs to will be replaced by TRUE in the syntax when this radiobutton is selected. If you want a different value to replace the name of the radiogroup control i.e. instead of TRUE replace 1, then replace %%VALUE%% by 1. You can also type in %DATASET% to replace the name of the current dataset when the radiobutton is selected. The syntax property allows you to parameterize the syntax string created when the dialog is executed.")]
        [BSkyLocalizedDescription("BSkyRadioButton_SyntaxDescription", typeof(BSky.GlobalResources.Properties.Resources))]
        public string Syntax
        {
            get;
            set;
        }




        #endregion


    }
}
