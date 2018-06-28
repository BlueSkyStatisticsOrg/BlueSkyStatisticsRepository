using System.Windows.Controls;
using System.ComponentModel;
using BSky.Interfaces.Controls;
using System.Windows.Media;
using System;

namespace BSky.Controls
{
    /// <summary>
    /// Interaction logic for BSkyMultiLineLabel.xaml
    /// </summary>
    [TypeConverter(typeof(PropertySorter))]
    [DefaultPropertyAttribute("Type")]
    public partial class BSkyMultiLineLabel : TextBox, IBSkyControl

    {
        public BSkyMultiLineLabel()
        {
            InitializeComponent();
            this.FontFamily = new FontFamily("Segoe UI");
            this.FontSize = 12;
            
            //  RequiredSyntax = true;

            //TemplateKey = "TextBoxBaseControlTemplate";
            //Added by Aaron 08/31/2014
            //3.	The multiline label control (which is actually a textbox)must have a light grey color to match the canvas. 
            //However the textbox needs to have a background color of white to contrast with the light blue back ground of the canvas
            //This creates a problem, as I cant have the textbox show with a white background and the multi-line label with light blue
            //One way to do this is have the textbox show with a white background and the multi-line label show with a while back ground in dialog editor mode (originally the canvas had a white back ground in dialog editor mode)
            //But when I am in execute mode, the canvas has a light blue color, and I would have to switch the back ground of the multi-line label
            //in dialog editor mode to light blue instead of transparent of the textbox

            //The reason it needs to have a light grey color is 
            //This looks weird on canvas of dialog editor mode as color is white.WE may want the canvas in dialog editor mode to be light grey. 
            //I have made this change
            this.SetResourceReference(TemplateProperty, "TextBoxControlTemplateMultiline");
        }

        [ReadOnly(true)]
        [Category("Control Settings"), PropertyOrder(1)]
        [Description("Allows you to add descriptive text (that wraps across multiple lines)associated with another control, for example you can use the label control to display a caption above your source variable list titled 'Source Variable List'. ")]


        public string Type
        {
            get
            {
                return "Multi-line label Control";
            }
        }

          [Category("Control Settings"), PropertyOrder(2)]
        [Description("Required property. You must specify a unique name for every control added to the dialog. You will not be able to save a dialog definition unless every control on the dialog and containing sub-dialogs has a unique name. ")]
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

        [Description("Optional property. Enter a caption for this textbox.")]
        public new string Text
        {
            get
            {
                return base.Text;
            }
            set
            {
                base.Text = value;
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


        // Following CanExecute is added by Anil 16Sep2016. This will be used to disable the dialog's OK button if Prerequisite fails
        // Anil: 17Sep2016 we should comment following. Its of no use. 
        //  But I am not sure how it is going to effect existing dialog when they are opened in dialog designer.
        private bool canExecute = true;

        [Category("Control Settings"), PropertyOrder(4)]
        [Description("Default value is True. This property controls whether the OK button on the dialog is enabled or disabled. If CanExecute =true for all controls on the dialog and contained sub-dialogs, the OK button is enabled, if CanExecute =false for any one control on the dialog or contained sub-dialogs, the OK button is disabled. For example, to ensure that the dialog cannot be executed unless text is entered into this textbox control, set the default value of the canexecute property for this control to false, then define a rule that triggers when a user enters text in the textbox control (The rule is defined on the text property for example having a valid string or numeric value) that sets the canexecute property to 'true' which enables the OK button on the dialog (remember to set another rule to set canexecute to false when the user clears the textbox). This ensures that the dialog cannot be executed unless text is entered in the textbox control. To define a rule, click in the property and then click the elipses button.")]
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
