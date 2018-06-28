using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Controls;
using BSky.Interfaces.Controls;
using System.Windows.Media;

namespace BSky.Controls
{
    public class BSkyRadioButtonCollection : List<BSkyRadioButton>
    {
    }
     [TypeConverter(typeof(PropertySorter))]
    [DefaultPropertyAttribute("Type")]
    public class BSkyRadioGroup : GroupBox, IBSkyInputControl, IBSkyControl
    {
      

        private BSkyRadioButtonCollection _radioButtons = new BSkyRadioButtonCollection();
        private StackPanel panel = null;

        public BSkyRadioGroup()
        {
            panel = new StackPanel();
            this.Content = panel;
            this.Syntax = "%%VALUE%%";
            this.Header = "GroupBox";
            IsHitTestVisible = true;
            var converter = new System.Windows.Media.BrushConverter();
            this.BorderBrush = (Brush)converter.ConvertFrom("#FFA9A9A9");
        }

        //[Description("The RadioGroup control allows you to setup a group of radiobuttons. This is a read only property. Click on each property in the grid to see the configuration options for the radiogroup control. ")]
        [BSkyLocalizedDescription("BSkyRadioGroup_TypeDescription", typeof(BSky.GlobalResources.Properties.Resources))]
        [Category("Control Settings"), PropertyOrder(1)]
        public string Type
        {
            get
            {
                return "RadioGroup Control";
            }
        }



        [Category("Control Settings"), PropertyOrder(2)]
        //[Description("Required property. You must specify a unique name for every control added to the dialog. You will not be able to save a dialog definition unless every control on the dialog and containing sub-dialogs has a unique name. ")]
        [BSkyLocalizedDescription("BSkyRadioGroup_NameDescription", typeof(BSky.GlobalResources.Properties.Resources))]
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
        //[Description("Optional property. Enter a caption for this RadioGroup control.")]
        [BSkyLocalizedDescription("BSkyRadioGroup_HeaderTextDescription", typeof(BSky.GlobalResources.Properties.Resources))]
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
        [BSkyLocalizedDescription("BSkyRadioGroup_WidthDescription", typeof(BSky.GlobalResources.Properties.Resources))]
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
        [BSkyLocalizedDescription("BSkyRadioGroup_HeightDescription", typeof(BSky.GlobalResources.Properties.Resources))]
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
        [BSkyLocalizedDescription("BSkyRadioGroup_LeftDescription", typeof(BSky.GlobalResources.Properties.Resources))]
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
        [BSkyLocalizedDescription("BSkyRadioGroup_TopDescription", typeof(BSky.GlobalResources.Properties.Resources))]
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
          [DisplayName("Add RadioButtons")]
         // [Description("Add one or more radiobuttons to the radio group control by clicking in the property and clicking the ellipses button.  When adding radiobuttons, you need to enter a name, a descriptive text and select the default state (selected or unselected) for each radiobutton. Hit the Enter key to add the next radiobutton.")]
        [BSkyLocalizedDescription("BSkyRadioGroup_RadioButtonsDescription", typeof(BSky.GlobalResources.Properties.Resources))]
        [Editor(@"BSky.Controls.DesignerSupport.RadioGroupEditor, AnalyticsUnlimited.Controls,  Culture=neutral",
                typeof(System.Drawing.Design.UITypeEditor))]
        public BSkyRadioButtonCollection RadioButtons
        {
            get
            {
                
                return null;
               // return _radioButtons;
            }
            set
            {
                //01/03/2013
                //Added line below
                //_radioButtons = value;
            }
            //get 
            //{
            //    _radioButtons.Clear();
            //    foreach(object obj in this.panel.Children)
            //        _radioButtons.Add((BSkyRadioButton)obj);
            //    return _radioButtons; 
            //}
            //set 
            //{
            //    //_radioButtons.Clear();
            //    //_radioButtons.AddRange(value);
            //    panel.Children.Clear();
            //    foreach (BSkyRadioButton btn in value)
            //    {
            //        btn.GroupName = this.Name;
            //        panel.Children.Add(btn);
            //    }
            //}
        }



        public string Value
        {
            get
            {
                string result = string.Empty;
                // 12/24/2012 Added by Aaron
                StackPanel stkpanel = this.Content as StackPanel;

                // 12/24/2012 Commented the code below

                //foreach (object obj in this.Content.Children)
                //{
                //    BSkyRadioButton btn = obj as BSkyRadioButton;
                //    if (btn.IsChecked.HasValue && btn.IsChecked.Value)
                //    {
                //        // Added by Aaron on 12/16
                //        // Commented the lines below
                //       // result = btn.Syntax.Replace("%%VALUE%%", "true");
                //        //result = btn.Syntax;

                //        if (btn.Syntax == "%%VALUE%%") result = btn.Syntax.Replace("%%VALUE%%", "true");
                //        else result = btn.Syntax;
                //        return result;
                //    }
                //}

                // Added the code below 

                foreach (object obj in stkpanel.Children)
                {
                    BSkyRadioButton btn = obj as BSkyRadioButton;
                    if (btn.IsChecked.HasValue && btn.IsChecked.Value)
                    {
                        // Added by Aaron on 12/16
                        // Commented the lines below
                       // result = btn.Syntax.Replace("%%VALUE%%", "true");
                        //result = btn.Syntax;

                        //Added by Aaron 05/04/2015
                        //Changed true to TRUE as true was causing problems in the heatmap, it needed full upper case TRUE
                       if (btn.Syntax == "%%VALUE%%") result = btn.Syntax.Replace("%%VALUE%%", "TRUE");
                       else result = btn.Syntax;
                       return result;
                   }
               }



                //Aaron 12/16
                //This is thecase when none of the radio buttons are selected
                //Aaron 09/09/2014
                //Change result from returning false to returning ""
                //This is because I want to support the scatterplot where I have several radio groups where no radio buttons are selected and I have to return nothing or empty string instead of false
                result = "";
                this.Syntax.Replace("%%VALUE%%", result);
                return result;
            }
        }

        #region IBSkyInputControl Members

        
        public string Syntax
        {
            get;
            set;
        }

        #endregion
    }
}
