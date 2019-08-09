using BSky.Interfaces.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BSky.Controls
{
    [TypeConverter(typeof(PropertySorter))]
    [DefaultPropertyAttribute("Type")]
    public partial class BSkySpinnerCtrl : UserControl, IBSkyAffectsExecute, IBSkyInputControl, IBSkyControl, IBSkyEnabledControl
    {
       
       // public bool Enabled { get; set; }

        public BSkySpinnerCtrl()
        {
            InitializeComponent();
            upbtn.Click  += new RoutedEventHandler(Upbtn_Click);
            downbtn.Click += new RoutedEventHandler(Downbtn_Click);
        }

        private void Upbtn_Click(object sender, RoutedEventArgs e)
        {
            string strval = text.Text;
            double result;
            if (double.TryParse(strval, out result))
            {
                result += Step;
                text.Text = result.ToString();
            }

        }

        private void Downbtn_Click(object sender, RoutedEventArgs e)
        {
            string strval = text.Text;
            double result;
            if (double.TryParse(strval, out result))
            {
                result -= Step;
                text.Text = result.ToString();
            }
        }

        [Category("Control Settings"), PropertyOrder(1)]
        //[Description("Allows you to add descriptive text associated with another control, for example you can use the label control to display a caption above your source variable list titled 'Source Variable List' . ")]
       
        public string Type
        {
            get
            {
                return "Spinner Control";
            }
        }

        [Category("Syntax Settings"), PropertyOrder(1)]
        [Description("The control name in the syntax string will be replaced by the content of the spinner control. These values will be used to parameterize the syntax string created when the dialog is executed. ")]
        public string Syntax
        {
            get;
            set;
        }

        private bool _enabled = true;
        [Category("Control Settings"), PropertyOrder(6)]
        // [Description("Default is True(enabled). This property controls whether the default state of this radiobutton control is enabled or disabled. For enabled, select True, for disabled select False. When enabled, you can select this radiobutton, when disabled, you cannot select the radiobutton.")]
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


        [Category("Control Settings"), PropertyOrder(2)]
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

        [Category("Control Settings"), PropertyOrder(2)]
        public  string Text
        {
            get
            {
                return text.Text;
            }
            set
            {
                text.Text = value;
            }
        }

        private double _step=1;
        [Category("Control Settings"), PropertyOrder(2)]
        public double Step
        {
            get
            {
                return _step;
            }
            set
            {
                _step = value;
            }
        }

        [Category("Layout Properties"), PropertyOrder(1)]
        [Description("Default value is the width of this control. To change drag the adorners(corner of the control) or enter a width.")]
        // [BSkyLocalizedDescription("BSkyCheckBox_WidthDescription", typeof(BSky.GlobalResources.Properties.Resources))]
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
        [Description("Default value is the height of this control. To change, drag the adorners(corner of the control) or enter a height.")]
        //   [BSkyLocalizedDescription("BSkyCheckBox_HeightDescription", typeof(BSky.GlobalResources.Properties.Resources))]
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
        [Description("Default value is the X coordinate of the top left corner of this control. To change, drag the control to a different position or enter a X coordinate.")]
        //  [BSkyLocalizedDescription("BSkyCheckBox_LeftDescription", typeof(BSky.GlobalResources.Properties.Resources))]
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
        //    [BSkyLocalizedDescription("BSkyCheckBox_TopDescription", typeof(BSky.GlobalResources.Properties.Resources))]
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
                //if (CanExecuteChanged != null)
                //{
                //    BSkyBoolEventArgs b = new BSkyBoolEventArgs();
                //    b.Value = value;
                //    CanExecuteChanged(this, b);
                //}
            }
        }


    }
}
