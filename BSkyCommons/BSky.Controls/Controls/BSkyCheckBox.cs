using System;
using System.Windows.Controls;
using System.ComponentModel;
using BSky.Interfaces.Controls;
using System.Windows.Media;

namespace BSky.Controls
{
    [TypeConverter(typeof(PropertySorter))]
    [DefaultPropertyAttribute("Type")]

    public class BSkyCheckBox : CheckBox, IBSkyAffectsExecute, IBSkyInputControl, IBSkyControl, IBSkyEnabledControl
    {
        public BSkyCheckBox()
        {
            CheckedChangeBehaviour = new BehaviourCollection();
            Syntax = "%%VALUE%%";
            this.SetResourceReference(StyleProperty, "CheckBoxTemplate");
            //Standardizing the checkbox button font type and font sizes to protect against changes in themes
            this.FontFamily = new FontFamily("Segoe UI");
            this.FontSize = 12;
            uncheckedsyntax = "FALSE";
            // this.SetResourceReference(TemplateProperty, "CheckBoxTemplate");
            ///Setting shades ///
            //this.Effect =
            //    new DropShadowEffect
            //    {
            //        Color = new Color { R = 155, A = 200, B = 0, G = 95 },
            //        Direction = 320,
            //        ShadowDepth = 0,
            //        Opacity = 1
            //    };            
        }


        [ReadOnly(true)]
        //[Description("Checkbox Control. This is a read only property. Click on each property in the grid to see the configuration options for the checkbox control. ")]
        [BSkyLocalizedDescription("BSkyCheckBox_TypeDescription", typeof(BSky.GlobalResources.Properties.Resources))]
        [Category("Control Settings"), PropertyOrder(1)]
        public string Type
        {
            get
            {
                 return "Checkbox Control";
            }
        }



        [Category("Control Settings"), PropertyOrder(2) ]
        
        //[Description("Required property. You must specify a unique name for every control added to the canvas. You will not be able to save a dialog definition unless every control on the dialog has a unique name. ")]
        [BSkyLocalizedDescription("BSkyCheckBox_NameDescription", typeof(BSky.GlobalResources.Properties.Resources))]
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
        //[Description("Optional property. Enter descriptive text to be displayed with this checkbox.")]
        [BSkyLocalizedDescription("BSkyCheckBox_TextDescription", typeof(BSky.GlobalResources.Properties.Resources))]
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

        [Category("Layout Properties"), PropertyOrder(1)]
        //[Description("Default value is the width of this control. To change drag the adorners(corner of the control) or enter a width.")]
        [BSkyLocalizedDescription("BSkyCheckBox_WidthDescription", typeof(BSky.GlobalResources.Properties.Resources))]
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
        [BSkyLocalizedDescription("BSkyCheckBox_HeightDescription", typeof(BSky.GlobalResources.Properties.Resources))]
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
        [BSkyLocalizedDescription("BSkyCheckBox_LeftDescription", typeof(BSky.GlobalResources.Properties.Resources))]
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
        [BSkyLocalizedDescription("BSkyCheckBox_TopDescription", typeof(BSky.GlobalResources.Properties.Resources))]
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
        //[Description("Default value is False(unchecked). This property sets the default state of the checkbox. For checked, select True, for unchecked, select False")]
        [BSkyLocalizedDescription("BSkyCheckBox_IsSelectedDescription", typeof(BSky.GlobalResources.Properties.Resources))]
        public bool IsSelected
        {
            get { return IsChecked.HasValue ? IsChecked.Value : false; ; }
            set { IsChecked = value; }
        }

        [Category("Control Settings"), PropertyOrder(6)]
        [DisplayName("Define Rules")]
        //[Description("Default is empty(no rule). Use this optional property to define rules that trigger property changes in other controls, based on the change in state of this checkbox control. For example, if a user checks the checkbox, enable a textbox control to capture additional parameters. To define a rule, click in the property and click the elipses button.")]
        [BSkyLocalizedDescription("BSkyCheckBox_CheckedChangeBehaviourDescription", typeof(BSky.GlobalResources.Properties.Resources))]
        public BehaviourCollection CheckedChangeBehaviour
        {
            get;
            set;
        }

        private bool _enabled = true;
        [Category("Control Settings"), PropertyOrder(7)]
        //[Description("Default is True(enabled). This property controls whether the default state of the control is enabled or disabled. For enabled, select True, for disabled select False. For example, you may want the initial state of the checkbox to be disabled, however you may want to enable it based on a selection made in another control")]
        [BSkyLocalizedDescription("BSkyCheckBox_EnabledDescription", typeof(BSky.GlobalResources.Properties.Resources))]
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


     




        protected override void OnUnchecked(System.Windows.RoutedEventArgs e)
        {
            base.OnUnchecked(e);
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

        private bool canExecute = true;

        [Category("Control Settings"), PropertyOrder(5)]
        [Description("Default value is True. This property controls whether the OK button on the dialog is enabled or disabled. For example, if you don't want the user to click the OK button of the dialog unless this checkbox is checked, set canexecute to False, then define a rule to set canexecute to True when the checkbox is checked. ")]
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

        [Category("Syntax Settings"), PropertyOrder(1)]
        [Description("Default value of %%VALUE%% indicates that the control name in the syntax string will be replaced by TRUE when the checkbox is checked and FALSE when the checkbox is unchecked. These values will be used to parameterize the syntax string created when the dialog is executed. If you want a different value, for example 'chisq' to replace the control name when the checkbox is checked, replace %%VALUE%% with 'chisq' (you don't need to enter the single quotes)")]
        public string Syntax
        {
            get;
            set;
        }

        #endregion

        //   private bool _enabled = true;
        [Category("Syntax Settings"), PropertyOrder(2)]
        [DisplayName("Syntax when unchecked")]
        [Description("Default is FALSE.This property sets the string that will replace this control in the syntax when this checkox is unselected.")]
        public string uncheckedsyntax
        {
            get;
            set;
        }
    }
}
