using System;
using System.Windows.Controls;
using System.ComponentModel;
using System.Windows;
using BSky.Interfaces.Commands;
using System.Reflection;
using System.Windows.Media;
using System.Windows.Media.Effects;
using BSky.Interfaces.Controls;
using BSky.Interfaces.Commands;



namespace BSky.Controls
{
     [TypeConverter(typeof(PropertySorter))]
     [DefaultPropertyAttribute("Type")]
    public class BSkyButton : Button, IBSkyControl, IBSkyEnabledControl
    {
        public BSkyButton()
        {
            ClickBehaviour = new BehaviourCollection();
            ///Setting shades ///
            //LinearGradientBrush lgb = new LinearGradientBrush(new Color() { R = 221, A = 200, B = 239, G = 255 }, new Color() { R = 155, B = 95, G = 200 }, 70.5);
            //this.Background = lgb;///
            //this.Effect =
            //    new DropShadowEffect
            //    {
            //        Color = new Color { R = 153, A = 100, B = 234, G = 214 },
            //        Direction = 100,
            //        ShadowDepth = 0,
            //        Opacity = 1
            //    };
        }


        [Category("Control Settings"), PropertyOrder(1)]
        //[Description("The Button control allows you to create a button which when clicked brings up a sub-dialog. The sub-dialog property allows you to define a sub-dialog. Click on each property in the grid to see the configuration options for the button control.")]
        [BSkyLocalizedDescription("BSkyButton_TypeDescription", typeof(BSky.GlobalResources.Properties.Resources))]
        // [DisplayName("This is the source")]
        public string Type
        {
            get
            {
                return "Button Control";
            }

        }




        [Category("Control Settings"), PropertyOrder(2)]

        //[Description("Required property. You must specify a unique name for every control added to the dialog . You will not be able to save a dialog definition unless every control on the dialog and containing sub-dialogs has a unique name. ")]
        [BSkyLocalizedDescription("BSkyButton_NameDescription", typeof(BSky.GlobalResources.Properties.Resources))]
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
        //[Description("Optional property. Enter a caption for the button.")]
        [BSkyLocalizedDescription("BSkyButton_TextDescription", typeof(BSky.GlobalResources.Properties.Resources))]
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

        [Category("Layout Properties") ,PropertyOrder(1)]

        //[Description("Default value is the width of this control. To change drag the adorners(corner of the control) or enter a width.")]
        [BSkyLocalizedDescription("BSkyButton_WidthDescription", typeof(BSky.GlobalResources.Properties.Resources))]
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
        [BSkyLocalizedDescription("BSkyButton_HeightDescription", typeof(BSky.GlobalResources.Properties.Resources))]
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
        [BSkyLocalizedDescription("BSkyButton_LeftDescription", typeof(BSky.GlobalResources.Properties.Resources))]
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
        [BSkyLocalizedDescription("BSkyButton_TopDescription", typeof(BSky.GlobalResources.Properties.Resources))]
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
        [DisplayName("Define Rules")]

        public BehaviourCollection ClickBehaviour
        {
            get;
            set;
        }

        [Category("Control Settings"), PropertyOrder(6)]
        //[Description("This property allows you to create a sub dialog. Click the property and then click the ellipses button to create a sub-dialog.")]
        [BSkyLocalizedDescription("BSkyButton_DesignerDescription", typeof(BSky.GlobalResources.Properties.Resources))]
        [DisplayName("Sub-Dialog")]
        [Editor(@"BSky.Controls.DesignerSupport.DialogDesigner, AnalyticsUnlimited.Controls, Culture=neutral",
                typeof(System.Drawing.Design.UITypeEditor))]

        public string Designer
        {
            get;
            set;
        }

        public event EventHandler<BSkyBoolEventArgs> CanExecuteChanged;
        private bool canExecute = true;

        [Category("Control Settings"), PropertyOrder(7)]
        //[Description("Default value is True. This property controls whether the OK button on the dialog is enabled or disabled. If CanExecute =true for all controls on the dialog and contained sub-dialogs, the OK button is enabled, if CanExecute =false for any one control on the dialog or contained sub-dialogs, the OK button is disabled.")]
        [BSkyLocalizedDescription("BSkyButton_CanExecuteDescription", typeof(BSky.GlobalResources.Properties.Resources))]
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
        //[Description("Default is True(enabled). This property controls whether the default state of this button control is enabled or disabled. For enabled, select True, for disabled select False. When enabled, you can click on the button, when disabled, you cannot click on the button.For example, you may want the initial state of the button to be disabled, however you may want to enable it based on an entry made in a textbox control.")]
        [BSkyLocalizedDescription("BSkyButton_EnabledDescription", typeof(BSky.GlobalResources.Properties.Resources))]
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



        private SubDialogWindow subDlgWindow = null;
        BSkyCanvas fe = null;//Anil 04Mar2013. To store sub-dialogs in session. Each option button should have only one sub-dialog
        protected override void OnClick()
        {
            base.OnClick();
            //Anil 04Mar2013 if (baseWindow == null)
            {
                BSkyCanvas parent = UIHelper.FindVisualParent<BSkyCanvas>(this);
                
                // Aaron fe is not the parent
                if (fe == null)//Anil 04Mar2013. Very first time sub-dialog will be null and so will be created. next time it will hold the object from first run.
                    fe = this.Resources["dlg"] as BSkyCanvas;
                // 01/20/2013
                //Added this code to address a defect when resources are empty
                if (fe == null)
                {
                    MessageBox.Show(BSky.GlobalResources.Properties.Resources.SubDialogNotCreated);
                    return;
                }
                //Aaron 05/05/2014
                //Commented the line below
                //We don't want users to be able to click syntax on a sub-dialog
                //I hence removed syntax on a sub-dialog
              //  baseWindow = new BaseOptionWindow();
                    subDlgWindow = new SubDialogWindow();
                //Added the line below

                //Added by Aaron 10/22/2013
                //Added the code below to disable the syntax button on subdialogs
                
               // baseWindow.Paste.IsEnabled = false;
                subDlgWindow.Template = fe;
                var converter = new System.Windows.Media.BrushConverter();
                //Added 08/10/2014
                //This is so that the canvas dialog in sub dialog matches color of canvas
                fe.Background = (Brush)converter.ConvertFrom("#FFEEefFf");
                fe.DataContext = this.DataContext;

                //Added aaron 05/05/2015
             //   subDlgWindow.Closing += new CancelEventHandler(Window_Closing);
                subDlgWindow.Owner = UIHelper.FindVisualParent<BaseOptionWindow>(this);
            }

            if (fe.GetType().Name == "BSkyCanvas") subDlgWindow.ResizeMode = ResizeMode.NoResize;//Aaron's

            subDlgWindow.ShowDialog();
            //Added by Aaron 05/05/2015
            subDlgWindow.DetachCanvas();
            subDlgWindow.Template = null; //Anil 04Mar2013 Parent Child relation is un-set.
        }

        void Window_Closing(object sender, CancelEventArgs e)
        {
            (typeof(Window)).GetField("_isClosing", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(sender, false);

            e.Cancel = true;
            (sender as Window).Hide();
        }

    }
}

