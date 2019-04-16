using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Drawing;

namespace BSky.Controls
{
    /// <summary>
    /// Singleton Factory to create BSkyObjects
    /// </summary>
    class BSkyControlFactory
    {
        private static readonly BSkyControlFactory instance = new BSkyControlFactory();

        private static Dictionary<string, int> controlsCount = new Dictionary<string,int>();
        private BSkyControlFactory()
        {
        }

        public static void SetCanvasCount(int canvasCount)
        {
            controlsCount["Canvas"] = canvasCount + 1;
        }

        public static BSkyControlFactory Instance
        {
            get
            {
                return instance;
            }
        }


        //Added by Aaron 05/11/2015
        //This function is no longer called
        public string GetName(string type)
        {
            if (controlsCount.ContainsKey(type))
            {
                int i = controlsCount[type];
                controlsCount[type] = i+1;
                return type + i;
            }
            else
            {
                controlsCount[type] = 1;
                return type + 1;
            }
        }

        

        public FrameworkElement CreateControl(string objtype)
        {
            FrameworkElement b = null;
            switch (objtype)
            {
                case "SourceListBox":
                    //When invoked from the dialog creator, renderVars is set to true
                 //   CommonFunctions cf = new CommonFunctions();
                    b = new BSkySourceList(true,true);
                   BSkySourceList c = b as BSkySourceList;
                  //  cf.DisplayNameGridProperty(c, "Type", "The source variable list displays all the variables in the active dataset.");
                  //  cf.DisplayNameGridProperty(c, "CanExecute", "Default value is True. This property controls whether the OK button on the dialog is enabled or disabled.If CanExecute =true for all controls on the dialog and contained sub-dialogs, the OK button is enabled, if CanExecute =false for any one control on the dialog or contained sub-dialogs, the OK button is disabled.");
                  //cf.DisplayNameGridProperty(c, "SelectionChangeBehaviour", "Default is empty(no rule). Use this optional property to define rules that trigger property changes in this or other controls, based on the change in state of this variable list control.");
                    
       //     [Description("Default is empty(no rule). Use this optional property to define rules that trigger property changes in this or other controls, based on the change in state of this checkbox control. For example, if a user checks the checkbox, enable a textbox control to capture additional parameters. To define a rule, click in the property and click the elipses button.")]
                    b.Width = 135;
                    b.Height = 240;
                    break;
                case "TargetListBox":
                    b = new BSkyTargetList(false,true);
                    BSkyTargetList c1 = b as BSkyTargetList;
                    //CommonFunctions cf1 = new CommonFunctions();
                    //cf1.DisplayNameGridProperty(c1, "Type", "The target variable list contains the variables you want to analyze. Drag and drop the variables you want to analyze from the source variable list to one or more target variable lists.");

                    //cf1.DisplayNameGridProperty(c1, "CanExecute", "Default value is True. This property controls whether the OK button on the dialog is enabled or disabled. If CanExecute =true for all controls on the dialog and contained sub-dialogs, the OK button is enabled, if CanExecute =false for any one control on the dialog or contained sub-dialogs, the OK button is disabled. For example, if a user adds a variable to this variable list, set the canexecute property to 'true', which enables the OK button on the dialog (remember to set another rule to set canexecute to false when All items are removed from this variable list and the value of the itemscount property is 0). This ensures that the dialog cannot be executed unless one or more items are dragged and dropped into this variable list control. To define a rule, click in the property and then click the elipses button.");

                    //cf1.DisplayNameGridProperty(c1, "SelectionChangeBehaviour", "Default is empty(no rule). Use this optional property to define rules that trigger property changes in this or other controls, based on the change in state of this variable list control.For example, if a user adds a variable to this variable list, set the canexecute property to 'true', which enables the OK button on the dialog (remember to set another rule to set canexecute to false when All items are removed from this variable list and the value of the itemscount property is 0). This ensures that the dialog cannot be executed unless one or more items are dragged and dropped into this variable list control. To define a rule, click in the property and then click the elipses button.");
                    b.Width = 135;
                    b.Height = 240;
                    break;
                case "Button":
                    b = new BSkyButton();
                    b.Width = 100;
                    b.Height = 30;
                    break;
                case "textbox":
                    b = new BSkyTextBox();
                    b.Width = 135;
                    b.Height = 30;
                    break;
                case "GrpBox":
                    b = new BSkyGroupBox();
                    b.Width = 150;
                    b.Height = 100;
                    break;
                case "ChkBox":
                    b = new BSkyCheckBox();
                    b.Width = 75;
                    b.Height = 20;
                    break;
                case "RdBtn":
                    b = new BSkyRadioButton();
                    b.Width = 75;
                    b.Height = 20;
                    break;
                case "EditCombo":
                    b = new BSkyEditableComboBox();
                    b.Width = 100;
                    b.Height = 25;
                    break;

                case "NonEditCombo":
                    b = new BSkyNonEditableComboBox();
                    b.Width = 100;
                    b.Height = 25;
                    break;
                case "Label":
                    b = new BSkyLabel();
                    b.Width = 50;
                    b.Height = 25;
                    break;

                case "LabelForRequiredFld":
                    b = new BSkyLabelReqdField();
                    b.Width = 15;
                    b.Height = 25;
                    break;

                case "MultiLabel":
                    b = new BSkyMultiLineLabel();
                    b.Width = 100;
                    b.Height = 25;
                    break;
                case "Canvas":
                    b = new BSkyCanvas();
                    BSkyCanvas.dialogMode = true;
                    b.Width = 470;
                   b.Height = 300;
                //    b.Width = 0;
                  //  b.Height = 0;
                    break;
                case "MoveButton":
                   // b = new BSkyVariableMoveButton(true);
                    b = new BSkyVariableMoveButton();
                    b.Width = 35;
                    b.Height = 35;
                    break;
                case "RadioGroup":
                    b = new BSkyRadioGroup();
                    b.Width = 100;
                    b.Height = 100;
                    break;
                // Added by Aaron 03/31
                case "Browse":
                    b = new BSkyBrowse();
                    b.Width = 100;
                    b.Height = 30;
                    break;
                case "GroupingVariable":
                    b = new BSkyGroupingVariable();
                    b.Width = 135;
                   // BSkyGroupingVariable bc = b as BSkyGroupingVariable;
                   // bc.oneItemList.Width = 100;
                   // bc.Height = 50;
                    b.Height = 30;
                    break;

                case "gridForSymbols":
                    // b = new BSkyScrollTextBox();
                    b = new BSkygridForSymbols();
                    //b.Width = 135;
                    // BSkyGroupingVariable bc = b as BSkyGroupingVariable;
                    // bc.oneItemList.Width = 100;
                    // bc.Height = 50;
                    //b.Height = 40;
                    break;

                case "GridforCompute":
                    // b = new BSkyScrollTextBox();
                    b = new BSkygridForCompute();
                    b.Width = 300;
                    b.Height = 300;
                    //b.Width = 135;
                    // BSkyGroupingVariable bc = b as BSkyGroupingVariable;
                    // bc.oneItemList.Width = 100;
                    // bc.Height = 50;
                    //b.Height = 40;
                    break;


                case "ListBox":
                    // b = new BSkyScrollTextBox();
                    b = new BSkyListBox();
                    b.Width = 135;
                    b.Height = 120;
                    // BSkyGroupingVariable bc = b as BSkyGroupingVariable;
                    // bc.oneItemList.Width = 100;
                    // bc.Height = 50;
                    //b.Height = 40;
                    break;

                case "SourceDatasetList":
                    b = new BSkyListBoxwBorderForDatasets(true, true);
                    b.Width = 135;
                    b.Height = 120;
                    // BSkyGroupingVariable bc = b as BSkyGroupingVariable;
                    // bc.oneItemList.Width = 100;
                    // bc.Height = 50;
                    //b.Height = 40;
                    break;

                case "DestinationDatasetList":
                    b = new BSkyListBoxwBorderForDatasets(false, true);
                    b.Width = 135;
                    b.Height = 120;
                    // BSkyGroupingVariable bc = b as BSkyGroupingVariable;
                    // bc.oneItemList.Width = 100;
                    // bc.Height = 50;
                    //b.Height = 40;
                    break;


                case "MasterListBox":
                    // b = new BSkyScrollTextBox();
                    b = new BSkyMasterListBox();
                    b.Width = 135;
                    b.Height = 120;
                    // BSkyGroupingVariable bc = b as BSkyGroupingVariable;
                    // bc.oneItemList.Width = 100;
                    // bc.Height = 50;
                    //b.Height = 40;
                    break;

                case "AggregateCtrl":
                       b = new BSkyAggregateCtrl();
                    b.Width = 235;
                    b.Height = 335;
                    // BSkyGroupingVariable bc = b as BSkyGroupingVariable;
                    // bc.oneItemList.Width = 100;
                    // bc.Height = 50;
                    //b.Height = 40;
                    break;
                case "SortCtrl":
                    b = new BSkySortCtrl();
                    b.Width = 280;
                    b.Height = 202;
                    // BSkyGroupingVariable bc = b as BSkyGroupingVariable;
                    // bc.oneItemList.Width = 100;
                    // bc.Height = 50;
                    //b.Height = 40;
                    break;
                case "SliderCtrl":
                    b = new BSkySlider();
                    // BSkySlider b1 = b as BSkySlider;
                    // b1.Ticks = null;
                    b.Width = 135;
                    b.Height = 40;
                    //  b1.TickFrequency = 2;
                    //  b1.Maximum = 10;
                    //  b1.Minimum = 0;
                    // b1.TickPlacement = System.Windows.Controls.Primitives.TickPlacement.Both;
                    //TickFrequency = 2;
                    // b1.Ticks = null;

                    //DoubleCollection tickMarks = new DoubleCollection();
                    //tickMarks.Add(0.5);
                    //tickMarks.Add(1.5);
                    //tickMarks.Add(2.5);
                    //tickMarks.Add(3.5);
                    //tickMarks.Add(4.5);
                    //tickMarks.Add(5.5);
                    //tickMarks.Add(6.5);
                    //tickMarks.Add(7.5);
                    //tickMarks.Add(8.5);
                    //tickMarks.Add(9.5);
                    //b1.Ticks = tickMarks;
                    //  b1.AutoToolTipPlacement = System.Windows.Controls.Primitives.AutoToolTipPlacement.BottomRight;

                    // BSkyGroupingVariable bc = b as BSkyGroupingVariable;
                    // bc.oneItemList.Width = 100;
                    // bc.Height = 50;
                    //b.Height = 40;
                    break;

                case "AdvancedSliderCtrl":
                    b = new BSkyAdvancedSlider();
                    // BSkyAdvancedSlider b1 = b as BSkyAdvancedSlider;
                    // b1.Ticks = null;
                    b.Width = 135;
                    b.Height = 45;
                    break;

                case "BSkySpinnerCtrl":
                    b = new BSkySpinnerCtrl();
                    // BSkyAdvancedSlider b1 = b as BSkyAdvancedSlider;
                    // b1.Ticks = null;
                    b.Width = 80;
                    b.Height = 22;
                    break;
            }
            
            //b.Name = GetName(objtype);
            return b;
        }

    }
}
