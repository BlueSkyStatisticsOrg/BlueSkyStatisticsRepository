using System;
using System.Windows;
using System.Windows.Controls;
using System.ComponentModel;
using BSky.Interfaces.Controls;

namespace BSky.Controls
{
    /// <summary>
    /// Interaction logic for SymbolsForModel.xaml
    /// </summary>
     [TypeConverter(typeof(PropertySorter))]
     [DefaultPropertyAttribute("Type")]
    public partial class BSkygridForSymbols : Grid, IBSkyControl
    {
        public BSkygridForSymbols()
        {
            InitializeComponent();
            
        }



        [Description("The Mathematical operator controls give you the ability to create a grid of mathematical operators. The operator you click on automatically gets inserted in the textbox that captures the formula. This is a read only property. Click on each property in the grid to see the configuration options for this mathematical operator control. ")]

        [Category("Control Settings"), PropertyOrder(1)]
        public string Type
        {
            get
            {
                return "Mathematical Operator Control";
            }
        }
         
         
         
         
         
         [Category("Control Settings"), PropertyOrder(2)]
         [Description("Required property. You must specify a unique name for every control added to the dialog. You will not be able to save a dialog definition unless every control on the dialog and containing sub-dialogs has a unique name. ")]
        public string name
        {
            get
            { return base.Name; }

            set
            { base.Name = value; }
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
          [Description("Default value is the Y coordinate of the top left corner of this control. To change drag the control to a different position or enter a Y coordinate.")]
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


     
        [Category("Control Settings"), PropertyOrder(3)]
        [Description("This is the name of the textbox control where the mathematical operators clicked on in the grid will be added.")]
        public string textBoxName { get; set; }



        //private bool _enabled = true;
        //[Category("Control Settings"), PropertyOrder(3)]
        //public bool Enabled
        //{
        //    get
        //    {
        //        if (BSkyCanvas.dialogMode == true)
        //            return _enabled;
        //        else return base.IsEnabled;
        //    }

        //    set
        //    {
        //        if (BSkyCanvas.dialogMode == true)
        //            _enabled = value;
        //        else
        //            base.IsEnabled = value;
        //    }

        //}

        public event EventHandler<BSkyBoolEventArgs> CanExecuteChanged;
        private bool canExecute = true;
        [Category("Control Settings"), PropertyOrder(4)]
        [Description("Default value is True. This property controls whether the OK button on the dialog is enabled or disabled. If CanExecute =true for all controls on the dialog and contained sub-dialogs, the OK button is enabled, if CanExecute =false for any one control on the dialog or contained sub-dialogs, the OK button is disabled. To define a rule, click in the property and then click the ellipses button.")]
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


        

        public bool checkValidTextBox(string name)
        {
            
            BSkyCanvas canvas = UIHelper.FindVisualParent<BSkyCanvas>(this);
            foreach (FrameworkElement fe in canvas.Children)
            {
                if (fe.Name == name && fe is BSkyTextBox) return true;
            }
            return false;
        }


    }
}
