using System;
using System.ComponentModel;
using System.Windows.Forms;
using BSky.Interfaces.Controls;
namespace BSky.Controls
{
    [TypeConverter(typeof(PropertySorter))]
    [DefaultPropertyAttribute("Type")]
    public class BSkyBrowse : System.Windows.Controls.Button, IBSkyControl, IBSkyEnabledControl
    {
        public BSkyBrowse()
        {

        }


      

        [Category("Control Settings"), PropertyOrder(1)]
        //[Description(" The browse control allows you select files from your desktop. This is a read only property. Click on each property in the grid to see the configuration options for this browse control. ")]
        [BSkyLocalizedDescription("BSkyBrowse_TypeDescription", typeof(BSky.GlobalResources.Properties.Resources))]
        public string Type
        {
            get
            {
                return "Browse Control";
            }
        }

        
        [Category("Control Settings"), PropertyOrder(2)]
        //[Description("Required property. You must specify a unique name for every control added to the dialog. You will not be able to save a dialog definition unless every control on the dialog and containing sub-dialogs has a unique name. ")]
        [BSkyLocalizedDescription("BSkyBrowse_NameDescription", typeof(BSky.GlobalResources.Properties.Resources))]
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

        //[Description("Optional property. Enter a caption for this browse control.")]
        [BSkyLocalizedDescription("BSkyBrowse_TextDescription", typeof(BSky.GlobalResources.Properties.Resources))]
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
        [BSkyLocalizedDescription("BSkyBrowse_WidthDescription", typeof(BSky.GlobalResources.Properties.Resources))]
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
        [BSkyLocalizedDescription("BSkyBrowse_HeightDescription", typeof(BSky.GlobalResources.Properties.Resources))]
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
        [BSkyLocalizedDescription("BSkyBrowse_LeftDescription", typeof(BSky.GlobalResources.Properties.Resources))]
        public double Left
        {
            get;
            set;
        }

        [Category("Layout Properties"), PropertyOrder(4)]
        //[Description("Default value is the Y coordinate of the top left corner of this control. To change drag the control to a different position or enter a Y coordinate.")]
        [BSkyLocalizedDescription("BSkyBrowse_TopDescription", typeof(BSky.GlobalResources.Properties.Resources))]
        public double Top
        {
            get;
            set;
        }

        [Category("Control Properties"), PropertyOrder(4)]
       [Browsable(false)]
        public string path
        {
            get;

            set;

        }

        private bool _enabled = true;
        [Category("Control Settings"), PropertyOrder(5)]

        //[Description("Default is True(enabled). This property controls whether the default state of this browse control is enabled or disabled. For enabled, select True, for disabled select False. When enabled, you can click this control to select a file, when disabled, you cannot click the control.For example, you may want the initial state of the control to be disabled, however you may want to enable it based on a selection made in another control")]
        [BSkyLocalizedDescription("BSkyBrowse_EnabledDescription", typeof(BSky.GlobalResources.Properties.Resources))]
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
        private bool canExecute = true;
        [Category("Control Settings"), PropertyOrder(5)]
        //[Description("Default value is True. This property controls whether the OK button on the dialog is enabled or disabled. If CanExecute =true for all controls on the dialog and contained sub-dialogs, the OK button is enabled, if CanExecute =false for any one control on the dialog or contained sub-dialogs, the OK button is disabled.   To define a rule, click in the property and then click the ellipses button.")]
        [BSkyLocalizedDescription("BSkyBrowse_CanExecuteDescription", typeof(BSky.GlobalResources.Properties.Resources))]
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

        protected override void OnClick()
        {
            //base.OnClick();
            
            OpenFileDialog fdlg = new OpenFileDialog();
            fdlg.Title = "C# Corner Open File Dialog";
            fdlg.InitialDirectory = @"c:\";
            fdlg.Filter = "All files (*.*)|*.*|All files (*.*)|*.*";
            fdlg.FilterIndex = 2;
            fdlg.RestoreDirectory = true;
            if (fdlg.ShowDialog() == DialogResult.OK)
            {
                path = fdlg.FileName;
            }
            
        }
    }

     
}




