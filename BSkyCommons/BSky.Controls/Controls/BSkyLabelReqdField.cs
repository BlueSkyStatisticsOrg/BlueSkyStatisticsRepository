using System;
using System.Linq;
using System.Windows.Controls;
using System.ComponentModel;
using BSky.Interfaces.Controls;
using System.Drawing;
using System.Windows.Media;

namespace BSky.Controls
{
    [TypeConverter(typeof(PropertySorter))]
    [DefaultPropertyAttribute("Type")]
    public class BSkyLabelReqdField : Label, IBSkyControl
    {
        public BSkyLabelReqdField()
        {
            this.Text = "Label";
            //  Standardizing the label font type and font sizes to protect against changes in themes
            this.FontFamily = new System.Windows.Media.FontFamily("Segoe UI");
            this.Foreground = System.Windows.Media.Brushes.Red;
            this.Content = "!";
            this.FontSize = 12;
            // base.Font = new Font("Arial", 12);
            // base.MaximumSize = new Size(100, 0);
            // base.AutoSize = true;
        }



        [Category("Control Settings"), PropertyOrder(1)]
        //[Description("Allows you to add descriptive text associated with another control, for example you can use the label control to display a caption above your source variable list titled 'Source Variable List' . ")]
       // [BSkyLocalizedDescription("BSkyLabel_TypeDescription", typeof(BSky.GlobalResources.Properties.Resources))]
        public string Type
        {
            get
            {
                return "Required field Indicator";
            }
        }



        [Category("Control Settings"), PropertyOrder(2)]
        //[Description("Required property. You must specify a unique name for every control added to the dialog. You will not be able to save a dialog definition unless every control on the dialog and containing sub-dialogs has a unique name. ")]
     //   [BSkyLocalizedDescription("BSkyLabel_NameDescription", typeof(BSky.GlobalResources.Properties.Resources))]
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
        //[Description("Optional property. Enter a caption (descriptive text) that will be displayed.")]
      //  [BSkyLocalizedDescription("BSkyLabel_TextDescription", typeof(BSky.GlobalResources.Properties.Resources))]
        public string Text
        {
            get
            {
                
                // }

                return base.Content as string;

            }
            set
            {
                base.Content = value as string;
            }
        }
        [Category("Layout Properties")]
        //[Description("Default value is the width of this control. To change drag the adorners(corner of the control) or enter a width.")]
     //   [BSkyLocalizedDescription("BSkyLabel_WidthDescription", typeof(BSky.GlobalResources.Properties.Resources))]
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
        //[Description("Default value is the height of this control. To change, drag the adorners(corner of the control) or enter a height.")]
     //   [BSkyLocalizedDescription("BSkyLabel_HeightDescription", typeof(BSky.GlobalResources.Properties.Resources))]
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
        [Category("Layout Properties")]
        //[Description("Default value is the X coordinate of the top left corner of this control. To change, drag the control to a different position or enter a X coordinate.")]
      //  [BSkyLocalizedDescription("BSkyLabel_LeftDescription", typeof(BSky.GlobalResources.Properties.Resources))]
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

        [Category("Layout Properties")]
        //[Description("Default value is the Y coordinate of the top left corner of this control. To change drag the control to a different position or enter a Y coordinate.")]
     //   [BSkyLocalizedDescription("BSkyLabel_TopDescription", typeof(BSky.GlobalResources.Properties.Resources))]
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


        private bool canExecute;

        [Category("Control Settings"), PropertyOrder(3)]
        [Browsable(false)]
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
    }
}
