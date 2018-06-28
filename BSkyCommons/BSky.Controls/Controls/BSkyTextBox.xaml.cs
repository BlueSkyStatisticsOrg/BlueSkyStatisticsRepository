using System;
using System.Windows.Controls;
using System.ComponentModel;
using System.Windows;
using BSky.Interfaces.Controls;
using System.Windows.Media;
using BSky.Statistics.Common;

namespace BSky.Controls
{
    [TypeConverter(typeof(PropertySorter))]
    [DefaultPropertyAttribute("Type")]
    public partial class BSkyTextBox : TextBox, IBSkyAffectsExecute, IBSkyInputControl, IBSkyControl, IBSkyEnabledControl
    {
        public BSkyTextBox()
        {
            InitializeComponent();
          //  base.Style = (Style)this.FindResource("lala");
            Syntax = "%%VALUE%%";
            //Standardizing the textbox button font type and font sizes to protect against changes in themes
            this.FontFamily = new FontFamily("Segoe UI");
            this.FontSize = 12;
            SubstituteSettings = "TextAsIs";
            OverwriteSettings = "DontPrompt";
          //  RequiredSyntax = true;
            
            //TemplateKey = "TextBoxBaseControlTemplate";

            this.SetResourceReference(TemplateProperty,"TextBoxBaseControlTemplate");
            ///Setting shades ///
            //  var converter = new System.Windows.Media.BrushConverter();
            // this.BorderBrush = (Brush)converter.ConvertFrom("#FFA9A9A9");

            //base.Background = Brushes.Beige;
            //this.Effect =
            //    new DropShadowEffect
            //    {
            //        Color = new Color { R = 155, A = 200, B = 0, G = 95 },
            //        Direction = 320,
            //        ShadowDepth = 0,
            //        Opacity = 1
            //    };
            //Added by Aaron 12/23/2012
            TextChangedBehaviour = new BehaviourCollection();
        }


        public string PrefixTxt
        {
            get;
            set;
        }
        //Added by Aaron 07/20/2014
        //This variable below is not used and should be ignored. Its around only because existing dialogs like  bin numeric variables
        //were created with this variable
        public Boolean OverWriteExistingVariables
        {
            get;
            set;
        }


        //Added by Aaron 07/20/2014
        //If values entered in this control create new variables or datasets and these variables or dataset already exist, select whether the user should be prompted before overwriting 
        [Category("Control Settings"), PropertyOrder(6)]
        [Description("The default value of this property is 'DontOverwrite'. If values entered in this control create new variable(s) or dataset(s) and these variable(s) or dataset(s) already exist, select whether the user should be prompted before overwriting. Click within the control and then click on the lookup button to additional information.")]
        [Editor(@"BSky.Controls.DesignerSupport.ManageOverwriteSettings, BSky.Controls,Culture=neutral",
                typeof(System.Drawing.Design.UITypeEditor))]
        public String OverwriteSettings
        {
            get;
            set;
        }


        [ReadOnly(true)]
        [Category("Control Settings"), PropertyOrder(1)]
 [Description("The TextBox control allows you to enter a value in a textbox. This textbox controls supports word wrapping and multiple lines. The scroll bar will be displayed only when text is entered and scrolling is required. This is a read only property. Click on each property in the grid to learn how to configure the textbox control.")]
        
      
        public string Type
        {
            get
            {
                 return "TextBox Control";
            }
        }
        

        [Category("Control Settings"), PropertyOrder(2)]

        [Description("Required property. You must specify a unique name for every control added to the dialog . You will not be able to save a dialog definition unless every control on the dialog and containing sub-dialogs has a unique name. ")]
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


        [Category("Control Settings"), PropertyOrder(3)]
         [DisplayName("Define Rules")]
         [Description("Default is empty(no rules). Use this optional property to define rules that trigger property changes in this or other controls, when ever a change is made to the text in this control. For example, to ensure that the dialog cannot be executed unless text is entered into this textbox control, set the default value of the canexecute property for this control to false, then define a rule that triggers when a user enters text in the textbox control (The rule is defined on the text property for example having a valid string or numeric value) that sets the canexecute property to 'true' which enables the OK button on the dialog (remember to set another rule to set canexecute to false when the user clears the textbox). This ensures that the dialog cannot be executed unless text is entered in the textbox control. To define a rule, click in the property and then click the elipses button.")]
        public BehaviourCollection TextChangedBehaviour
        {
            get;
            set;
        }



        [CategoryAttribute("Syntax Settings")]
        [Description("This setting allows you to set how the text in this control will replace the control name in the syntax string. To see additional details, click in the property and click the ellipses button. ")]
        [Editor(@"BSky.Controls.DesignerSupport.ManageTextBoxSubstitution, BSky.Controls,  Culture=neutral",
                typeof(System.Drawing.Design.UITypeEditor))]
        public string SubstituteSettings { get; set; } 


        public virtual void TextBox_Drop(object sender, DragEventArgs e)
        {
            string[] formats = e.Data.GetFormats();
            object sourcedata = e.Data.GetData(formats[0]) as object;
            DataSourceVariable var = sourcedata as DataSourceVariable;
            int curcursorpos = this.SelectionStart;//23Feb2017 saving it beacuse it is getting lost after running insert()
            this.Text = this.Text.Insert(this.SelectionStart, var.Name);  //23Feb2017 this.AppendText(var.Name);
            this.SelectionStart = curcursorpos + var.Name.Length;//23Feb2017 move the cursor to the end of the text that was just inserted.
        }





        private void TextBox_DragOver(object sender, DragEventArgs e)
        {
            // if ((AutoVar == false) && (BSkyCanvas.sourceDrag == (ListBox)sender)) e.Effects = DragDropEffects.None;
            // //else
            //// e.Effects = null != e.Data.GetData(typeof(object)) ? DragDropEffects.Move : DragDropEffects.None;

            //02/24/2013
            //Added by Aaron
            //THis ensures that the move icon is displayed when dragging and dropping within the same listbox
            //If we are not dragging and dropping within the same listbox, the copy icon is displayed

            //if ((AutoVar == true) && (BSkyCanvas.sourceDrag == (ListBox)sender)) e.Effects = DragDropEffects.Move;
            //Added 10/19/2013
            //Added the code below to support listboxes that only allow a pre-specified number of items or less
            //
            //System.Windows.Forms.DialogResult diagResult;
            //double result = -1;
          
            string[] formats = e.Data.GetFormats();
            DataSourceVariable sourcedata = e.Data.GetData(formats[0]) as DataSourceVariable;
            //Added 04/03/2014
            //If I highlight and drag something in a textbox to the textbox itself the application will hand
            //Allow drop isenabled on a textbox so there is nothing that prevents me from draging text in one textbox to another
            if (sourcedata == null)
            {
                e.Effects = DragDropEffects.None;
                e.Handled = true;
                return;
            }
            //BSky.Controls.filter f=new filter();
            //Added by Aaron 09/02/2013

            //Added code below to check filters when dragging and dropping
            //
           

           



                e.Effects = DragDropEffects.Copy;
           
            // else e.Effects = DragDropEffects.Copy;
            // //else e.Effects = DragDropEffects.Move;
            // e.Effects = DragDropEffects.Copy;
            e.Handled = true;
        }


        protected override void OnTextChanged(TextChangedEventArgs e)
        {

            //A control is associated with a behavior collection
            //A behavior collection is a collection of behaviors
            //A behavior can contains one condition e.g. itemscount = 0 and one or more setters e.g. set the property canexecute =true on control destination. The setter also stores the
            //value of the property its bound to (the control and the propert name)
            //Behaviors are invoked on the control when a particular activity takes place for e.g. when a selection changes on a listbox control, when text is entered 
            //on a text control.
            //Typical use case for behaviors is as follows. Every control has a CanExecute property. This can be enabled or disabled by default.
            //The OK button will only be enabled when the can execute property of all the controls on the canvas are set to true.
            //
            //For example, when the
            //one sample t.test dialog is rendered for the first time, the destination listbox will always be empty. We would then set the default value of canexecute to false.
            //We would define a condition for when items count is > than 0 and set canexecute for the destination property to true only when items count is > than 0. 
            //We would also add another condition to set Canexecute to false when items count is 0 to account for the case when users move variables from the destination
            //back to the source.
            //Also note that you can have canexecute to true on the destination control of one sample t.test but have the canexecute property on the textbox set to false 
            //as no valid entry has be entered tocompare the dialog against. In this situation the OK button on the dialog is disabled
            base.OnTextChanged(e);
            BSkyCanvas parent = UIHelper.FindVisualParent<BSkyCanvas>(this);
            //Added by Aaron 06/10/2014
            //Added the condition BSkyCanvas.applyBehaviors == true to make sure that apply behavior does not get called
            //in dialog editor mode
            //This is because we don't want events getting firect in dialog editor mode
            //This is because it may change properties of other controls whose initial values we want to preserve
            //We only want behaviors to fire after the dialog has been fully loaded.
            if (parent != null && TextChangedBehaviour != null &&BSkyCanvas.applyBehaviors==true)
                parent.ApplyBehaviour(this, TextChangedBehaviour);
        }

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

        private bool _enabled = true;

        [Category("Control Settings"), PropertyOrder(5)]
         [Description("Default is True(enabled). This property controls whether the default state of this textbox control is enabled or disabled. For enabled, select True, for disabled select False. When enabled, you can enter text into this control, when disabled, you cannot enter text into the control. For example, you may want the initial state of the control to be disabled, however you may want to enable it based on a selection made in another control")]
        public  bool Enabled
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
                    _enabled  = value;
                else 
                base.IsEnabled = value;
            }

        }






        public event EventHandler<BSkyBoolEventArgs> CanExecuteChanged;

        #region IBSkyInputControl Members

        [Category("Syntax Settings"), PropertyOrder(1)]
        [Description("Default value of %%VALUE%% indicates that all the text entered in this textbox will replace the control name in the syntax.")]
        public string Syntax
        {
            get;
            set;
        }

        //[Category("Syntax Settings"), PropertyOrder(2)]
        //[Description("Default value of this property is true. This means that this control must generate syntax for the command to run. If this control generates an optionsyntax substring and you want a , appended automatically if the control generates syntax set this property to true. ")]
        //public bool RequiredSyntax
        //{
        //    get;
        //    set;
        //}

        #endregion
    }
}
