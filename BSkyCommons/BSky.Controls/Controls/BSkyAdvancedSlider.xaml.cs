using BSky.Interfaces.Controls;
using System;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Data;

namespace BSky.Controls
{
    /// <summary>
    /// Interaction logic for BSkyAdvancedSlider.xaml
    /// </summary>
    /// 
    [TypeConverter(typeof(PropertySorter))]
    [DefaultPropertyAttribute("Type")]
    public partial class BSkyAdvancedSlider : UserControl, IBSkyAffectsExecute, IBSkyInputControl, IBSkyControl, IBSkyEnabledControl , INotifyPropertyChanged
    {
        public BSkyAdvancedSlider()
        {
            InitializeComponent();
            //MyProperty = "s";//bind text and slider
            SetTextAndSliderBinding();
           // valtxt.TextChanged += new System.Windows.Controls.TextChangedEventHandler(valtext_TextChanged);
            //this.valtext.TextChanged += new System.EventHandler(valtext_TextChanged);
        }

        //private void valtext_TextChanged(object sender, EventArgs e)
        //{
        //    try
        //    {
        //        // Convert the text to a Double and assign to slider
        //        double value;
        //        if (double.TryParse(valtxt.Text, out value))
        //        {
        //            slValueqwGxzplHapppqa129aM.Value = value;

        //            // If the number is valid, display it in Black.
        //            //valtxt.ForeColor = Color.Black;
        //          //  valtxt.Foreground = 
        //        }
        //        else
        //        {
        //            // If the number is invalid, display it in Red.
        //            //textBox.ForeColor = Color.Red;
        //        }
        //    }
        //    catch
        //    {
        //        // If there is an error, display the text using the system colors.
        //       // textBox.ForeColor = SystemColors.ControlText;
        //    }
        //}
        private void SetTextAndSliderBinding()
        {
            //Added by Aaron
            //Added a binding delay

            DataContext = this;
            Binding bin = new Binding("SliderValue");
            bin.Mode = BindingMode.TwoWay;
            bin.Delay = 1000;
            bin.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            valtxt.SetBinding(TextBox.TextProperty, bin);


            Binding bin2 = new Binding("SliderValue");
            bin2.Mode = BindingMode.TwoWay;
            bin2.Delay = 1000;
            bin2.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            slValueqwGxzplHapppqa129aM.SetBinding(Slider.ValueProperty, bin2);
        }
        //private string _myProp; //this property is not used for anything. This is just to apply binding
        //public string MyProperty
        //{
        //    get { return _myProp; }
        //    set
        //    {
        //        _myProp = value;
        //        DataContext = this;
        //        Binding bin = new Binding("SliderValue");
        //        bin.Mode = BindingMode.TwoWay;
        //        bin.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
        //        valtxt.SetBinding(TextBox.TextProperty, bin);


        //        Binding bin2 = new Binding("SliderValue");
        //        bin2.Mode = BindingMode.TwoWay;
        //        bin2.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
        //        slValueqwGxzplHapppqa129aM.SetBinding(Slider.ValueProperty, bin2);
        //    }
        //}

     

        private double _sliderValue;

        [Category("Control Settings"), PropertyOrder(8)]
        public double SliderValue
        {
            get { return  Math.Round(_sliderValue,2); }
            set
            {
                if (_sliderValue != value)
                {
                    _sliderValue = value;
                    OnPropertyChanged("SliderValue");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string info)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(info));
            }
        }

        [Category("Control Settings"), PropertyOrder(1)]
        [Description("Gives you a slider control with maximum and minimum values and a text box to display and the slider value")]
      
        public string Type
        {
            get
            {
                return "Advanced Slider Control";
            }
        }

        [Category("Control Settings"), PropertyOrder(2)]
        [Description("Required property. You must specify a unique name for every control added to the canvas. You will not be able to save a dialog definition unless every control on the dialog has a unique name. ")]
      
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
        [Description("The maximum value displayed by slider contol")]
        public Double Maximum
        {
            get
            {
                return slValueqwGxzplHapppqa129aM.Maximum;
            }
            set
            {
                slValueqwGxzplHapppqa129aM.Maximum = value;
            }
        }

        [Category("Control Settings"), PropertyOrder(4)]
        [Description("The Minimum value displayed by slider contol")]
        public Double Minimum
        {
            get
            {
                return slValueqwGxzplHapppqa129aM.Minimum;
            }
            set
            {
                slValueqwGxzplHapppqa129aM.Minimum = value;
            }
        }

        [Category("Control Settings"), PropertyOrder(5)]

        [Description("The Tick Frequency value displayed by slider contol")]
        public Double TickFrequency
        {
            get
            {
                return slValueqwGxzplHapppqa129aM.TickFrequency;
            }
            set
            {
                slValueqwGxzplHapppqa129aM.TickFrequency = value;
            }
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
                base.Width = value;
            }
        }

        [Category("Layout Properties"), PropertyOrder(2)]
        [Description("Default value is the height of this control. To change, drag the adorners(corner of the control) or enter a height.")]
     
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

        private bool _enabled = true;
        [Category("Control Settings"), PropertyOrder(9)]
        [Description("Default is True(enabled). This property controls whether the default state of the control is enabled or disabled. For enabled, select True, for disabled select False. For example, you may want the initial state of the checkbox to be disabled, however you may want to enable it based on a selection made in another control")]
  
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

        private bool canExecute = true;

        [Category("Control Settings"), PropertyOrder(10)]
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
        [Description("We will always replace the control in the syntax by the slider value")]
        public string Syntax
        {
            get;
            set;
        }

        #endregion


    }
}
