using System;
using System.Windows.Controls;
using System.ComponentModel;
using System.Windows;
using System.Reflection;
using System.Windows.Media;
using BSky.Interfaces.Controls;
using System.Collections.Generic;
using System.IO;
using System.Windows.Input;

using System.Windows.Shapes;

namespace BSky.Controls
{
    public class helpfileset
    {

        public string originalhelpfilepath;
        public string newhelpfilepath;

    }
    public class BSkyCanvas : Canvas
    {
        static public ListBox sourceDrag = null;
        static public BSkyListBoxwBorderForDatasets sourceDataset = null;



        //this.Background = (Brush)bc.ConvertFrom("#FFXXXXXX");


        //Added by Aaron 01/23/2013
        // Added by Aaron 03/31/2013
        // The internalhelpfilename holds the name of the help file that will be saved to the bin/config directory
        //In the function  private bool deployXmlXaml(string Commandname, bool overwrite) in file menueditor.xaml.cs, called when we install the dialog, we use this property to name the help files
        //IN the BSky file, the help files have their original name. This is so that if you unzip the dialog definition, so you see what you expect
        //We only change the help file names to the dialog name with a suffix of a number (as a single dialog can have multiple help files
        //If there is a valid help file associated with the canvas, then this function automaticaly associates it with an internalhelpfilename

        //The parameter 1 starts the suffix which is used to name the help files with 1
        public string internalhelpfilename { get; set; }

        //Aaron 06/10/2014
        //This global is set to true when you don't want the applybehaviours call to fire
        //This is done when we load the dialog the first time
        //THis happens on opening the dialog in the dialog editor and launching it from the application the first time
       //It does not make sense to fire the apply behavioron the text changed event when setting a default value for the textbox
        //This is becuase the text change event on the text box in dialog 1 could change a selection state of a checkbox
        //in dialog 2, a child of dialog 1, but dialog 2 has not been created
        //INFACT WE SHOULD ALWAYS ENFORCE THAT THE BEHAVIOR OF A CONTROL SHOULD NOT FIRE ON INITIALIZATION. ONLY ON CHANGE FROM DEFAULT STATE
        //THE GLOBAL IS USED IN THE FUNCTION bskytextbox.xaml.cs to prevent applybehaviour from being called when setting the default text in a textbox

        static public bool applyBehaviors = false;
        static public BSkyCanvas first = null;


        //Added 11/21/2013
        //Commented line below
        //static public object globaldata=null;
        // static public ListBox destDrag = null;

        // 11/21/2013
        //Added  by Aaron to keep track of 2 things
        //1. wherether we need to check the properties (that point to objects) in behaviour.cs.
        // 2. This is also used in the Enabled property in every control//This is used in dialog editor mode 

        //1. wherether we need to check the properties (that point to objects) in behaviour.cs.
        //when entering property names for the conditions and when entering the controls and property names for the setters
        //In dialog editor mode, we want to make sure that we are using the correct property and control names
        //Note, we are working on the setters and getters on each individual behavior and not the collection. When not in dialog edito
        //mode i.e. execute mode, we store the values of the behaviors
        //
        //2. This is also used in the Enabled property in every control
        //Enabled sets the IsEnabled property. In dialog editor mode IsEnabled is always true
        //This is important as if the IsEnabled property is false, you cannnot click on that control in dialog editor mode
        //If you cannot click on that control you cannot delete the control, you cannot define behavior on that control
        //In dialog editor mode, IsEnabled is always true
        //On save, we take Enabled and store it into IsEnabled
        //On open, we set IsEnabled to true, irrespective of the value of Enabled


        static public bool dialogMode = false;

        //Added by Aaron 05/06/2014
        //When I am inspecting a dialog definition or previewing a dialog in dialog editor. I want to click on the HELP button and access the Help files
        //However the help files are not in the bin/config directory
        //Its only whenI install the dialog that the help files are in the binn/config directory
        //Also when you install the help files, we rename the help files to dialogname_1, dialogname_2. This ensures that we don't 
        //accidently overide the help files of another command
        //This means when I am in the main application, I want the Help button to work differently for 2 cases
        //Case 1: click help on the dialog definition when inspecting the command. Here I launch the help files with their original name
        //from the temp directory
        //Case 2: click help on dialog displayed when executing an installed command
        //Here I launch the help files from the bin/config directory
        //This also allows me to use the same code to create the sub dialog mode in the execution of the dialog and the inspection
        //Case 3:
        //When I am in dialog editor and I am previewing a dialog definition, I just created
        //The help files need to be loaded from the original llocation where they exist
        static public bool previewinspectMode = false;


        //11/24/2013
        //Keeps references to open canvases. As the open canvases that have not been saved cannot be accessed through the resources of the the button, I need a reference as I need to search these canvases for controls and duplicate names
        //When I am in dialog editor, I create the first dialog of a command, then I click on the designer and create the next sub dialog
        //The new sub dialog has not been added to the resources of the button. Now I create a rule that references a control on the newly 
        //created sub dialog, I need to acccess the newly created dialog
        //As soon as I click save and close on the sub dialog I just created, I remove it from Chaonopencanvas, this is done in dialogdesigner.cs
        //Duplicate detection is also based on based on chainopencanvas so this is very important
        //files effected windows1.xaml.cs, behavious.cs, dialogdesigner.cs, inspectorcommand.cs

        //There are also some situations where I have to write some code to handle conditions below in dialod editor
        //There are 3 conditions here, when I launch dialog editor
        //1. The first thing I do is open an existing dialog. Chainopencanvas.count=0
        //2. I have an existing dialog open and I click open again.  Chainopencanvas.count=2. The first dialog is empty as I clean the existing canvas and open a new one
        //3. I hit new the first time I open dialogeditor and then I hit open. Again,Chainopencanvas.count=2. The first dialog is empty as I clean the existing canvas and open a new one 
        static public List<BSkyCanvas> chainOpenCanvas = new List<BSkyCanvas>();

        //Aaron added 12/15/2013
        //This is the flag that Anil will use to determine whether this is a command only dialog. This will get set in the new command dialog 
        public bool commandOnly = false;

        public BSkyCanvas()
        {
            base.Loaded += new RoutedEventHandler(BSkyCanvas_Loaded);

            ///Setting shades ///
            //Color c = Color.FromArgb(255, 237, 239, 243);
            var converter = new System.Windows.Media.BrushConverter();
            base.Background = (Brush)converter.ConvertFromString("#FFEDefFf"); //Brushes.Honeydew;//"#FFEDEFF3";// #FFEDefFf
            //  base.Background = Brushes.LightSteelBlue;
            // base.Background = (Brush)converter.ConvertFrom("#FFEEefFf");
            // base.Color = Brushes.LightSteelBlue;

            //if (chainOpenCanvas != null)
            //{
            //  length = chainOpenCanvas.Count;

            //
            //Aaron 05/12/2014
            // Commented the Code below
            //  chainOpenCanvas.Add(this);
            //''}
            //else chainOpenCanvas[length] = this;
        }

        public void drawLines ()
        {
            int i = 0;
            while (i < this.Height)
            {
                Line line = new Line();
                line.Stroke = Brushes.LightGray;
                line.StrokeThickness = 0.5;
               
                line.Y1 = i;
                line.X1 = 0;
                line.Y2 = i;
                line.X2 = this.Width;
                i = i + 10;
                Canvas.SetZIndex(line, -1);
                
                this.Children.Add(line);
            }
            i = 0;
            while (i < this.Width)
            {
                Line line = new Line();
                line.Stroke = Brushes.LightGray;
                line.StrokeThickness = 0.5;

                line.Y1 = 0;
                line.X1 = i;
                line.Y2 = this.Height;
                line.X2 = i;
                i = i + 10;
                Canvas.SetZIndex(line, -1);
                this.Children.Add(line);
            }
        }



        public void removeLines()
        {
            int count  =this.Children.Count;
            for (int i = count - 1; i >= 0; i--)
            {
                if (this.Children[i].GetType().Name == "Line") this.Children.RemoveAt(i);
            }
           
        }


        public FrameworkElement getCtrl(string ControlName)
        {
            FrameworkElement retval = null;
            int len = 0;
            int i = 0;
            //if (BSkyCanvas.chainOpenCanvas.Count > 0)
            //{
            //    len = BSkyCanvas.chainOpenCanvas.Count;
            //    while (i < len)
            //    {
            //        retval = returnCtrl(ControlName, BSkyCanvas.chainOpenCanvas[i]);
            //        if (retval != null) return retval;
            //        i = i + 1;

            //    }
            //}
            retval = returnCtrl(ControlName, BSkyCanvas.first);

            return retval;
        }


        //recursively looks at every canvas for the control. Recursion is used when there is a button on the canvas
        private FrameworkElement returnCtrl(string ControlName, BSkyCanvas cs)
        {

            IBSkyEnabledControl objcast = null;
            FrameworkElement retval = null; ;

            foreach (Object obj in cs.Children)
            {
                if (obj is IBSkyControl)
                {
                    //All controls that can be dropped on the canvas inherit from IBSkyControl
                    IBSkyControl ib = obj as IBSkyControl;
                    if (ib.Name == ControlName)
                    {
                        //Checking if the control is valid
                        //The following controls inherit from IBSKyInputControl
                        //BSKyCheckBox, BSKyComboBox, BSkyGroupingVariable, BSkyRadioButton, BSkyRadioGroup, BSkyTextBox, BSkySourceList, BSkyTargetList
                        objcast = obj as IBSkyEnabledControl;
                        if (objcast != null) return ib as FrameworkElement;
                        else return null;
                    }
                    //05/18/2013
                    //Added by Aaron
                    //Code below checks the radio buttons within each radiogroup looking for duplicate names
                    if (obj is BSkyRadioGroup)
                    {
                        BSkyRadioGroup ic = obj as BSkyRadioGroup;
                        StackPanel stkpanel = ic.Content as StackPanel;

                        foreach (object obj1 in stkpanel.Children)
                        {
                            BSkyRadioButton btn = obj1 as BSkyRadioButton;
                            if (btn.Name == ControlName)
                            {
                                return btn as FrameworkElement;
                            }
                        }

                    }

                }
                if (obj is BSkyButton)
                {
                    FrameworkElement fe = obj as FrameworkElement;
                    BSkyCanvas canvas = fe.Resources["dlg"] as BSkyCanvas;
                    if (canvas != null)
                    {
                        retval = returnCtrl(ControlName, canvas);
                        if (retval != null) return retval;
                    }
                }

            }

            return null;
        }

        void BSkyCanvas_Loaded(object sender, RoutedEventArgs e)
        {
            CheckCanExecute();
        }


        // public ListBox dragSource;

        [Category("Dialog Properties")]
        // [DescriptionAttribute("Bla bla bla")]
        [DisplayName("Dialog Title*")]
        public string Title
        {
            get;
            set;
        }

        [Category("Dialog Properties")]
        // [DescriptionAttribute("Bla bla bla")]
        [DisplayName("Model Classes")]
        public string ModelClasses
        {
            get;
            set;
        }

        [Category("Dialog Properties")]
        [DisplayName("Status text box name")]
        public string StatusTextBoxName
        {
            get;
            set;
        }

        //[Category("Dialog Properties")]
        //[DisplayName("Title*")]

        public string TitleOfDialog
        {
            get
            {
                return Title;
            }
        }




        //Added by Aaron 03/02/2014
        //This code below sets up a filter for the file open dialog used to select a package in the dialog installed
        //The Rpackages is a property of the canvas
        //Code for FilteredZipFileNameEditor is in outputselector.cs
        private string rpackages;
        [Category("Dialog Properties")]
        [DisplayName("R Packages Required")]
        [Editor(typeof(BSky.Controls.DesignerSupport.FilteredZipFileNameEditor),
      typeof(System.Drawing.Design.UITypeEditor))]
        public string RPackages
        {
            get
            {
                return rpackages;
            }
            set
            {
                //if (rpackages == null || rpackages == "")
                //    rpackages = value;
                //else

                //    rpackages = rpackages + ";" + value;
                //Added by Aaron 05/11/2015
                //Added lines below, commented 4 lines above
                rpackages = value;

            }
        }



        //[Category("Dialog Properties")]
        //[DisplayName("R Packages Required")]

        public string RPackagesforinspection
        {
            get
            {
                return RPackages;
            }

        }



        private bool canExecute = true;

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

        //15Sep2016 Added by Anil: to have a syntax that can be run before showing the dialog. (syntax for Checking prerequisite)
        [Category("Dialog Properties")]
        [DisplayName("Dialog Prerequisite Syntax")]
        [Editor(@"BSky.Controls.DesignerSupport.CommandSelector, BSky.Controls, Culture=neutral",
                typeof(System.Drawing.Design.UITypeEditor))]
        public string PrereqCommandString
        {
            get;
            set;
        }

        // [Category("Dialog Properties")]
        [Category("Dialog Properties")]
        [DisplayName("Dialog Syntax")]
        [Editor(@"BSky.Controls.DesignerSupport.CommandSelector, BSky.Controls, Culture=neutral",
                typeof(System.Drawing.Design.UITypeEditor))]
        public string CommandString
        {
            get;
            set;
        }


        //[Category("Dialog Properties")]
        //[DisplayName("Syntax")]
        //[Editor(@"BSky.Controls.DesignerSupport.CommandSelector, BSky.Controls, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null",
        //        typeof(System.Drawing.Design.UITypeEditor))]
        public string dlgsyntax
        {
            get { return CommandString; }
            set { CommandString = value; }
        }


        [Category("Dialog Properties")]
        [DisplayName("Help File")]
        [EditorAttribute(typeof(System.Windows.Forms.Design.FileNameEditor),
                        typeof(System.Drawing.Design.UITypeEditor))]
        public string Helpfile
        {
            get;
            set;
        }


        private bool _splitprocessing = true;
        [Category("Dialog Properties")]
        //[DescriptionAttribute("Determines whether split processing is enabled or disabled. For example for data manipulation commands, split processing should be turned off.")]
        [BSkyLocalizedDescription("BSkyCanvas_splitProcessingDescription", typeof(BSky.GlobalResources.Properties.Resources))]
        [DisplayName("Split processing")]
        public bool splitProcessing
        {
            get
            {
                return _splitprocessing;
            }
            set
            {
                _splitprocessing = value;
            }
        }

        [Category("Dialog Properties")]
        [DisplayName("Path in menu where dialog is installed")]
        [Editor(@"BSky.Controls.DesignerSupport.MenuSelector, BSky.Controls, Culture=neutral",
                typeof(System.Drawing.Design.UITypeEditor))]
        public string MenuLocation
        {
            get;
            set;
        }

        [Category("Dialog Properties")]
        [DisplayName("Output Definition file")]
        // [Editor(typeof(System.Windows.Forms.Design.FilteredFileNameEditor),
        // typeof(System.Drawing.Design.UITypeEditor))]
        [Editor(typeof(BSky.Controls.DesignerSupport.FilteredFileNameEditor),
        typeof(System.Drawing.Design.UITypeEditor))]
        public string OutputDefinition
        {
            get;
            set;
        }


        //[Category("Dialog Properties")]
        //[DisplayName("Output Definition file")]
        //// [Editor(typeof(System.Windows.Forms.Design.FilteredFileNameEditor),
        //// typeof(System.Drawing.Design.UITypeEditor))]

        public string OutputDefinitionFile
        {
            get
            {
                return OutputDefinition;
            }

        }

        [Category("Dialog Properties")]
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

        //Aaron added 12/08/2013
        //captures whether this is a dialog or command (dialog is not brought up and syntax is directly run)

        private bool command = false;
        public bool Command
        {
            get
            {
                return command;
            }

            set { command = value; }
        }

        [Category("Dialog Properties")]
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

        ////14May2015 Added by Anil
        [Category("Dialog Properties")]
        [Editor(@"BSky.Controls.DesignerSupport.HelpEditor, BSky.Controls,  Culture=neutral",
                        typeof(System.Drawing.Design.UITypeEditor))]
        public new string HelpText
        {
            get;
            set;
        }

        ////14May2015 Added by Anil
        [Category("Dialog Properties")]
        public new string RHelpText
        {
            get;
            set;
        }

        /// <summary>
        /// Checks if canvas as a whole can be executed 
        /// </summary>
        public void CheckCanExecute()
        {
            bool IsCanExecute = true;
            foreach (FrameworkElement fe in this.Children)
            {
                IBSkyAffectsExecute temp = fe as IBSkyAffectsExecute;
                if (temp != null && !temp.CanExecute)
                {
                    IsCanExecute = false;
                    break;
                }
            }
            this.CanExecute = IsCanExecute;
        }

        public event EventHandler<BSkyBoolEventArgs> CanExecuteChanged;



        public void ApplyBehaviour(object sender, BehaviourCollection behaviours)
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



            foreach (Behaviour behaviour in behaviours)
            {
                //Get the type of object where the selection has changed
                //Added by Aaron 04/12/2014
                //The code below to make sure that the property type matches the value type. This insures that the
                //condition can be applied without any exceptionsto which the property 
                IBSkyInputControl ctrl = sender as IBSkyInputControl;
                Type t = sender.GetType();
                if (behaviour.Condition.PropertyName == null)
                {
                    System.Windows.Forms.MessageBox.Show(BSky.GlobalResources.Properties.Resources.CanvasPropNotSet1+" \"" + ctrl.Name + "\""+
                        BSky.GlobalResources.Properties.Resources.CanvasPropNotSet2 + "  \"" + ctrl.Name + "\" " +
                        BSky.GlobalResources.Properties.Resources.CanvasPropNotSet3);
                    return;
                }

                //Get information of the property on which the condition is specified. The property belongs to the object 
                PropertyInfo pInfo = t.GetProperty(behaviour.Condition.PropertyName);
                if (pInfo == null)
                    continue;
                //Get the value of the property for which the condition is triggered
                object value = pInfo.GetValue(sender, null);
                bool pass = false;
                //why can't I call pInfo.GetType() below?
                //Store the type of the property for which the condition is defined
                Type propType = pInfo.PropertyType;


                
                if (propType == typeof(int) || propType == typeof(double) || propType == typeof(long))
                {
                    try
                    {
                        double lvalue = Convert.ToDouble(value);
                        double rvalue = Convert.ToDouble(behaviour.Condition.Value);
                    }
                    catch (FormatException e)
                    {
                        System.Windows.Forms.MessageBox.Show(BSky.GlobalResources.Properties.Resources.CtrlBehaviourIncorrect1+"  \"" + ctrl.Name + "\" "+
                            BSky.GlobalResources.Properties.Resources.CtrlBehaviourIncorrect2 + " \n"+
                            BSky.GlobalResources.Properties.Resources.CtrlBehaviourIncorrect3 +
                            "  \"" + behaviour.Condition.PropertyName + "\" "+
                             BSky.GlobalResources.Properties.Resources.CtrlBehaviourIncorrect4 +
                            " \"" + behaviour.Condition.Value + "\"\n"+
                            BSky.GlobalResources.Properties.Resources.CtrlBehaviourIncorrect5);
                        return;
                    }

                }

                if (propType == typeof(Boolean))
                {
                    try
                    {
                        Boolean lvalue = Convert.ToBoolean(value);
                        Boolean rvalue = Convert.ToBoolean(behaviour.Condition.Value);
                    }
                    catch (FormatException e)
                    {
                        System.Windows.Forms.MessageBox.Show(BSky.GlobalResources.Properties.Resources.CtrlBehaviourIncorrect1 +
                            " \"" + ctrl.Name + "\" "+
                            BSky.GlobalResources.Properties.Resources.CtrlBehaviourIncorrect2 + "\n"+
                            BSky.GlobalResources.Properties.Resources.CtrlBehaviourIncorrect3 +
                            "  \"" + behaviour.Condition.PropertyName + "\" "+
                            BSky.GlobalResources.Properties.Resources.CtrlBehaviourIncorrect4 +
                            " \"" + behaviour.Condition.Value + "\"\n"+
                            BSky.GlobalResources.Properties.Resources.CtrlBehaviourIncorrect5);
                        return;
                    }

                }



                //Code below sets pass =true if the condition specified is triggered
                switch (behaviour.Condition.Operator)
                {
                    //Added by Aaron: 08/25/2013
                    //the equals operator appliesto strings (textboxes) and numeric types (itemscount on a listbox)

                    case ConditionalOperator.Equals:
                        if (propType == typeof(int) || propType == typeof(double) || propType == typeof(long) || propType == typeof(string) || propType == typeof(Boolean))
                        {
                            if (propType == typeof(int) || propType == typeof(double) || propType == typeof(long))
                            {
                                double lvalue = Convert.ToDouble(value);
                                double rvalue = Convert.ToDouble(behaviour.Condition.Value);
                                if (lvalue == rvalue)
                                    pass = true;
                            }
                            if (propType == typeof(string))
                            {
                                string lvalue = value as string;
                                string rvalue = behaviour.Condition.Value;
                                if (lvalue == rvalue)
                                    pass = true;
                            }
                            if (propType == typeof(Boolean))
                            {
                                Boolean lvalue = Convert.ToBoolean(value);
                                Boolean rvalue = Convert.ToBoolean(behaviour.Condition.Value);
                                if (lvalue == rvalue)
                                    pass = true;
                            }

                        }
                        break;
                    case ConditionalOperator.GreaterThan:
                        if (propType == typeof(int) || propType == typeof(double) || propType == typeof(long))
                        {
                            double lvalue = Convert.ToDouble(value);
                            double rvalue = Convert.ToDouble(behaviour.Condition.Value);
                            if (lvalue > rvalue)
                                pass = true;
                        }
                        break;
                    case ConditionalOperator.GreaterThanEqualsTo:
                        if (propType == typeof(int) || propType == typeof(double) || propType == typeof(long))
                        {
                            double lvalue = Convert.ToDouble(value);
                            double rvalue = Convert.ToDouble(behaviour.Condition.Value);
                            if (lvalue >= rvalue)
                                pass = true;
                        }
                        break;
                    case ConditionalOperator.LessThan:
                        if (propType == typeof(int) || propType == typeof(double) || propType == typeof(long))
                        {
                            double lvalue = Convert.ToDouble(value);
                            double rvalue = Convert.ToDouble(behaviour.Condition.Value);
                            if (lvalue < rvalue)
                                pass = true;
                        }
                        break;
                    case ConditionalOperator.LessThanEqualsTo:
                        if (propType == typeof(int) || propType == typeof(double) || propType == typeof(long))
                        {
                            double lvalue = Convert.ToDouble(value);
                            double rvalue = Convert.ToDouble(behaviour.Condition.Value);
                            if (lvalue >= rvalue)
                                pass = true;
                        }
                        break;
                    case ConditionalOperator.Contains:
                        if (propType == typeof(string))
                        {
                            string lvalue = value.ToString();
                            string rvalue = behaviour.Condition.Value.ToString();
                            if (lvalue.Contains(rvalue))
                                pass = true;
                        }
                        break;
                    case ConditionalOperator.Like:
                        if (propType == typeof(string))
                        {
                            string lvalue = value.ToString();
                            string rvalue = behaviour.Condition.Value.ToString();
                            if (lvalue.StartsWith(rvalue))
                                pass = true;
                        }
                        break;

                    //Added by Aaron on 08/25/2013
                    //We want the textbox on One Sample T.test disabled unless a valid numeric value is entered to compare against
                    case ConditionalOperator.IsNumeric:
                        if (propType == typeof(string))
                        {
                            decimal number = 0;
                            string valueasstring = value as string;
                            bool canConvert = Decimal.TryParse(valueasstring, out number);
                            if (canConvert == true)
                                pass = true;
                        }
                        break;
                    case ConditionalOperator.NotNumeric:
                        if (propType == typeof(string))
                        {
                            decimal number = 0;
                            string valueasstring = value as string;
                            bool canConvert = Decimal.TryParse(valueasstring, out number);
                            if (canConvert == false)
                                pass = true;
                        }
                        break;

                    //Added by Aaron 12/12/2013
                    //This is to support a condition of valid string to ensure a textbox has a valid string entered
                    case ConditionalOperator.ValidString:
                        if (propType == typeof(string))
                        {
                            string valueasstring = value as string;
                            if (valueasstring != null && valueasstring != "")
                                pass = true;
                        }
                        break;

                    case ConditionalOperator.isNullOrEmpty:
                        if (propType == typeof(string))
                        {
                            string valueasstring = value as string;
                            if (valueasstring == null || valueasstring == "")
                                pass = true;
                        }
                        break;


                }
                if (pass)
                {
                    foreach (PropertySetter setter in behaviour.Setters)
                    {
                       
                        //Added by Aaron 05/16/2015
                        //Its very easily to create a null property and control name and value set
                        //you click add a setter and we create a control name, property name and value to null
                        //The if look ensures that we don't crash
                        if (setter.ControlName != null || setter.PropertyName !=null)
                        {
                        
                        object fe = null;
                        if (string.IsNullOrEmpty(setter.ControlName))
                        {
                            fe = this;
                        }
                        else
                        {
                            fe = this.getCtrl(setter.ControlName);
                           // fe = this.FindName(setter.ControlName);
                        }
                        //Aaron 04/06/2014
                        // Error message needs to inserted here
                        if (fe == null)
                        {

                            System.Windows.Forms.MessageBox.Show(BSky.GlobalResources.Properties.Resources.CtrlBehaviourIncorrect1 +
                                "  \"" + ctrl.Name + "\" "+
                                BSky.GlobalResources.Properties.Resources.CtrlBehaviourIncorrect2 + "\n"+
                                BSky.GlobalResources.Properties.Resources.TheCtrlName +
                                " \"" + setter.ControlName + "\" "+
                                BSky.GlobalResources.Properties.Resources.CantFind+
                                BSky.GlobalResources.Properties.Resources.CheckBehaviour+
                                " \"" + ctrl.Name + "\" "+
                                BSky.GlobalResources.Properties.Resources.InDialogEditor);

                            return;
                        }


                        Type feType = fe.GetType();
                        //Aaron 11/03/2013
                        //fepinfo has has the destination property that needs to be set 
                        PropertyInfo fepInfo = feType.GetProperty(setter.PropertyName);
                        if (fepInfo == null)
                        {

                            System.Windows.Forms.MessageBox.Show(BSky.GlobalResources.Properties.Resources.CtrlBehaviourIncorrect1 +
                                "  \"" + ctrl.Name + "\" "+
                                BSky.GlobalResources.Properties.Resources.CtrlBehaviourIncorrect2 + " \n"+
                                BSky.GlobalResources.Properties.Resources.ThePropName +
                                " \"" + setter.PropertyName + "\" "+
                                BSky.GlobalResources.Properties.Resources.NotValidCtrlProp+
                                " \"" + setter.ControlName + "\" "+
                                BSky.GlobalResources.Properties.Resources.CheckBehaviour +
                                " \"" + ctrl.Name + "\" "+
                                BSky.GlobalResources.Properties.Resources.InDialogEditor);



                            return;
                        }
                        // Added by Aaron11/03/2013
                        //Commented the section below

                        //if (!setter.IsBinding)
                        //{
                        //    //Aaron 11/03/2013
                        //    //sets the value of the destination property
                        //    fepInfo.SetValue(fe, Convert.ChangeType(setter.Value, fepInfo.PropertyType), null);
                        //}
                        //else
                        //{
                        //    //Aaron 11/03/2013
                        //    //gets the source control name
                        //    object source = this.FindName(setter.SourceControlName);
                        //    if (source == null)
                        //        return;
                        //    Type sourceType = source.GetType();
                        //    //gets the property from the source control
                        //    PropertyInfo sourcePropInfo = sourceType.GetProperty(setter.SourcePropertyName);
                        //    //gets the value of the property from the source control
                        //    object val = sourcePropInfo.GetValue(source, null);

                        //    //Sets the destination value to the value of the property from the source control
                        //    fepInfo.SetValue(fe, Convert.ChangeType(val, fepInfo.PropertyType), null);
                        //}

                        try
                        {
                            fepInfo.SetValue(fe, Convert.ChangeType(setter.Value, fepInfo.PropertyType), null);
                            if (fepInfo == null)
                            {

                                System.Windows.Forms.MessageBox.Show(BSky.GlobalResources.Properties.Resources.TheValue+
                                    " \"" + setter.Value + "\" "+
                                    BSky.GlobalResources.Properties.Resources.CantAssignToProp+
                                    " \"" + setter.PropertyName + "\" "+
                                    BSky.GlobalResources.Properties.Resources.CheckBehaviour +
                                    " \"" + ctrl.Name + "\" "+
                                    BSky.GlobalResources.Properties.Resources.InDialogEditor);


                            }
                        }


                        catch (FormatException e)
                        {

                            System.Windows.Forms.MessageBox.Show(BSky.GlobalResources.Properties.Resources.TheExceptionGenerated+
                                " \"" + e.Message + "\"" + "\n"+
                                BSky.GlobalResources.Properties.Resources.ThePropName+
                                " \"" + setter.PropertyName + "\" "+
                                BSky.GlobalResources.Properties.Resources.InCtrl+
                                " \"" + setter.ControlName + "\" "+
                                BSky.GlobalResources.Properties.Resources.CantSetValue+
                                " \"" + setter.Value + "\" "+
                                BSky.GlobalResources.Properties.Resources.PropCantAcceptVal + "\n"+
                                BSky.GlobalResources.Properties.Resources.CheckBehaviour +
                                " \"" + ctrl.Name + "\" "+
                                BSky.GlobalResources.Properties.Resources.InDialogEditor);
                        }
                    }

                    } //Added by Aaron 05/16/2015, this is the end of the for loop
                }

            }
            CheckCanExecute();
        }

        //Added by Aaron 01/23/2013
        // Added by Aaron 03/31/2013
        // The internalhelpfilename holds the name of the help file that will be saved to the bin/config directory
        //In the function  private bool deployXmlXaml(string Commandname, bool overwrite) in file menueditor.xaml.cs, called when we install the dialog, we use this property to name the help files
        //IN the BSky file, the help files have their original name. This is so that if you unzip the dialog definition, so you see what you expect
        //We only change the help file names to the dialog name with a suffix of a number (as a single dialog can have multiple help files
        //If there is a valid help file associated with the canvas, then this function automaticaly associates it with an internalhelpfilename

        //The parameter 1 starts the suffix which is used to name the help files with 1

        public void processhelpfiles(BSkyCanvas canvas, string dialogName, int count)
        {
            if (canvas.Helpfile != null && canvas.Helpfile != string.Empty)
            {
                //helpfile hf = new helpfile();
                //hf.add(count, canvas.Helpfile);
                //count ++;
                //lst.Add(hf);
                //count =count +1;

                if (File.Exists(canvas.Helpfile))
                {
                    canvas.internalhelpfilename = dialogName + '_' + count.ToString() + System.IO.Path.GetExtension(canvas.Helpfile);
                    count = count + 1;
                }
                else
                {
                    Uri myUri;
                    string url = canvas.Helpfile;
                    if (!url.StartsWith("http://"))
                        url = "http://" + url;
                    if (Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out myUri))
                    {
                        canvas.internalhelpfilename = "urlOrUri";
                        canvas.Helpfile = url;
                    }
                    else
                    {
                        MessageBox.Show(BSky.GlobalResources.Properties.Resources.HelpMustBeValidFileURL);
                    }
                }
            }
            foreach (Object obj in canvas.Children)
            {
                if (obj is BSkyButton)
                {
                    FrameworkElement fe = obj as FrameworkElement;
                    BSkyCanvas cs = fe.Resources["dlg"] as BSkyCanvas;
                    if (cs != null) processhelpfiles(cs, dialogName, count);
                }
            } //end of for
        }


        //Used by the function private bool deployXmlXaml(string Commandname, bool overwrite) in file menueditor.xaml.cs, called when we install the dialog, we use this to get the original help file names and the name that will be used when installing these files to the config directory (The name of the command followed by the prefix
        //

        public List<helpfileset> gethelpfilenames(BSkyCanvas canvas)
        {
            List<helpfileset> ls = new List<helpfileset>();

            if (canvas.Helpfile != null && canvas.Helpfile != string.Empty)
            {
                //helpfile hf = new helpfile();
                //hf.add(count, canvas.Helpfile);
                //count ++;
                //lst.Add(hf);
                //count =count +1;
                helpfileset hs = new helpfileset();
                hs.originalhelpfilepath = canvas.Helpfile;
                hs.newhelpfilepath = canvas.internalhelpfilename;
                ls.Add(hs);

            }
            foreach (Object obj in canvas.Children)
            {
                if (obj is BSkyButton)
                {
                    FrameworkElement fe = obj as FrameworkElement;
                    BSkyCanvas cs = fe.Resources["dlg"] as BSkyCanvas;
                    if (cs != null) ls.AddRange(gethelpfilenames(cs));
                }
            } //end of for
            return ls;
        }



        //public class helpfile {
        //    int ordernum; 
        //    string helpfilename;

        //    void public add(int order, string name)
        //    {
        //        ordernum = order;
        //        helpfilename = name;
        //    }


        //};

    }

}
