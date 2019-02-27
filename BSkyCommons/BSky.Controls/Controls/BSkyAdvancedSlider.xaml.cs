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
    public partial class BSkyAdvancedSlider : UserControl, IBSkyAffectsExecute, IBSkyInputControl, IBSkyControl, IBSkyEnabledControl , INotifyPropertyChanged
    {
        public BSkyAdvancedSlider()
        {
            InitializeComponent();
            //MyProperty = "s";//bind text and slider
            SetTextAndSliderBinding();
        }

        private void SetTextAndSliderBinding()
        {
            DataContext = this;
            Binding bin = new Binding("SliderValue");
            bin.Mode = BindingMode.TwoWay;
            bin.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            valtxt.SetBinding(TextBox.TextProperty, bin);


            Binding bin2 = new Binding("SliderValue");
            bin2.Mode = BindingMode.TwoWay;
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
        public double SliderValue
        {
            get { return _sliderValue; }
            set
            {
                _sliderValue = value;
                OnPropertyChanged("SliderValue");
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



        [Category("Control Settings"), PropertyOrder(2)]
        //[Description("Required property. You must specify a unique name for every control added to the canvas. You will not be able to save a dialog definition unless every control on the dialog has a unique name. ")]
        //  [BSkyLocalizedDescription("BSkyCheckBox_NameDescription", typeof(BSky.GlobalResources.Properties.Resources))]
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


        [Category("Control Settings"), PropertyOrder(4)]
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

        [Category("Layout Settings"), PropertyOrder(1)]
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

        [Category("Layout Settings"), PropertyOrder(2)]
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
        [Category("Layout Settings"), PropertyOrder(3)]
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

        [Category("Layout Settings"), PropertyOrder(4)]
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

        private bool _enabled = true;
        [Category("Control Settings"), PropertyOrder(7)]
        [Description("Default is True(enabled). This property controls whether the default state of the control is enabled or disabled. For enabled, select True, for disabled select False. For example, you may want the initial state of the checkbox to be disabled, however you may want to enable it based on a selection made in another control")]
        //    [BSkyLocalizedDescription("BSkyCheckBox_EnabledDescription", typeof(BSky.GlobalResources.Properties.Resources))]
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


    }
}
