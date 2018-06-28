using System;
using System.Windows.Controls;
using System.ComponentModel;
using BSky.Interfaces.Controls;
using System.Windows.Media;

namespace BSky.Controls
{
    [TypeConverter(typeof(PropertySorter))]
    [DefaultPropertyAttribute("Type")]
    public class BSkyGroupBox : GroupBox, IBSkyControl
    {
        public EventHandler Click;
        public BSkyGroupBox()
        {
            this.Header = "GroupBox";
            IsHitTestVisible = true;
            var converter = new System.Windows.Media.BrushConverter();
            this.BorderBrush = (Brush)converter.ConvertFrom("#FFA9A9A9");
        }
        // BSkyGroupBox.Click += new System.EventHandler(BSkyGroupBox_Click);

        
  //[Description("GroupBox Control allows you to group related controls. This is a read only property. Click on each property in the grid to see the configuration options for the groupbox control. ")]
        [BSkyLocalizedDescription("BSkyGroupBox_TypeDescription", typeof(BSky.GlobalResources.Properties.Resources))]
        [Category("Control Settings"), PropertyOrder(1)]
        public string Type
        {
            get
            {
                 return "Groupbox Control";
            }
        }

         [Category("Control Settings"), PropertyOrder(2)]
         //[Description("Required property. You must specify a unique name for every control added to the dialog. You will not be able to save a dialog definition unless every control on the dialog and containing sub-dialogs has a unique name. ")]
        [BSkyLocalizedDescription("BSkyGroupBox_NameDescription", typeof(BSky.GlobalResources.Properties.Resources))]
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
        // [Description("Optional property. Enter a caption for this groupbox.")]
        [BSkyLocalizedDescription("BSkyGroupBox_HeaderTextDescription", typeof(BSky.GlobalResources.Properties.Resources))]
        public string HeaderText
        {
            get
            {
                if (base.Header != null)
                {
                    return base.Header.ToString();
                }
                return "TestString";
            }
            set
            {
                base.Header = value;
            }
        }
        [Category("Control Settings"), PropertyOrder(4)]
        [Browsable(false)]
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
        [BSkyLocalizedDescription("BSkyGroupBox_WidthDescription", typeof(BSky.GlobalResources.Properties.Resources))]
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

        // [Description("Default value is the height of this control. To change, drag the adorners(corner of the control) or enter a height.")]
        [BSkyLocalizedDescription("BSkyGroupBox_HeightDescription", typeof(BSky.GlobalResources.Properties.Resources))]
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
        [BSkyLocalizedDescription("BSkyGroupBox_LeftDescription", typeof(BSky.GlobalResources.Properties.Resources))]
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
        [BSkyLocalizedDescription("BSkyGroupBox_TopDescription", typeof(BSky.GlobalResources.Properties.Resources))]
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



    }
}
