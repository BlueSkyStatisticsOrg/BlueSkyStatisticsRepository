using System;
using System.Windows;
using System.Windows.Controls;
using System.ComponentModel;
//using System.Windows.Forms;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;
using System.Collections.Generic;
using BSky.Interfaces.Controls;

namespace BSky.Controls
{
    public class CommonFunctions
    {
        public void SaveToLocation(string inputfile, string outputDir, out string xamlFile, out string outputFile)
        {

            FileStream fileStreamOut;
            FileStream fileStreamIn = new FileStream(inputfile, FileMode.Open, FileAccess.Read);
            ZipInputStream zipInStream = new ZipInputStream(fileStreamIn);
            ZipEntry entry = zipInStream.GetNextEntry();
            outputFile = xamlFile = string.Empty;
            string tempDir = Path.GetTempPath();
            //Extract the files
            while (entry != null)
            {
                // FileStream fileStreamOut = new FileStream(outputDir +Path.GetFileName( entry.Name), FileMode.Create, FileAccess.Write);
                //Aaron 04/20/2013
                //Made a minor change to above
                //The original was
                // FileStream fileStreamOut = new FileStream(outputDir + entry.Name, FileMode.Create, FileAccess.Write);

                fileStreamOut = new FileStream(outputDir + Path.GetFileName(entry.Name), FileMode.Create, FileAccess.Write);

                if (entry.Name.EndsWith(".xaml")) xamlFile = entry.Name;
                if (entry.Name.EndsWith(".xml")) outputFile = entry.Name;
                int size;
                byte[] buffer = new byte[1024];
                do
                {
                    size = zipInStream.Read(buffer, 0, buffer.Length);
                    fileStreamOut.Write(buffer, 0, size);
                } while (size > 0);
                fileStreamOut.Close();
                entry = zipInStream.GetNextEntry();
            }
            zipInStream.Close();
            fileStreamIn.Close();
           


        }

        public void DisplayNameGridProperty(object obj, string propertyName, string displayName)
        {
            PropertyDescriptor descriptor = TypeDescriptor.GetProperties(obj.GetType())[propertyName];
            DescriptionAttribute attribute = (DescriptionAttribute)
                                          descriptor.Attributes[typeof(DescriptionAttribute)];
            System.Reflection.FieldInfo fieldToChange = attribute.GetType().GetField("description",
                                             System.Reflection.BindingFlags.NonPublic |
                                             System.Reflection.BindingFlags.Instance);

            if (fieldToChange != null) fieldToChange.SetValue(attribute, displayName);
            // propertyGrid1.Refresh();
        }


        public void SaveToLocationwhelpfiles(string inputfile, string outputDir, out string xamlFile, out string outputFile, out List<string>helpfilenames)
        {

             helpfilenames=new List<string>();
            FileStream fileStreamIn = new FileStream(inputfile, FileMode.Open, FileAccess.Read);
            ZipInputStream zipInStream = new ZipInputStream(fileStreamIn);
            ZipEntry entry = zipInStream.GetNextEntry();
            outputFile = xamlFile = string.Empty;
            string tempDir = Path.GetTempPath();
            //Extract the files
            while (entry != null)
            {
                // FileStream fileStreamOut = new FileStream(outputDir +Path.GetFileName( entry.Name), FileMode.Create, FileAccess.Write);
                //Aaron 04/20/2013
                //Made a minor change to above
                //The original was
                // FileStream fileStreamOut = new FileStream(outputDir + entry.Name, FileMode.Create, FileAccess.Write);

                FileStream fileStreamOut = new FileStream(outputDir + Path.GetFileName(entry.Name), FileMode.Create, FileAccess.Write);

                if (entry.Name.EndsWith(".xaml")) xamlFile = entry.Name;
                if (entry.Name.EndsWith(".xml")) outputFile = entry.Name;
                int size;
                byte[] buffer = new byte[1024];
                do
                {
                    size = zipInStream.Read(buffer, 0, buffer.Length);
                    fileStreamOut.Write(buffer, 0, size);
                } while (size > 0);
                fileStreamOut.Close();
                entry = zipInStream.GetNextEntry();
            }
            zipInStream.Close();
            fileStreamIn.Close();

        }


       


    }

    public abstract class propCtrlMethods
    {
        //public void propCtrlMethods()
        //{

        //}

        //Is the property a valid property of the control
        public bool valPropofCtrl(FrameworkElement obj, string propertyName)
        {
            string[] validProperties = null;
            if (obj != null)
            {
                if (obj is BSkySourceList || obj is BSkyTargetList )
                {
                    validProperties = new string[] { "maxNoOfVariables", "ItemsCount", "MoveVariables", "CanExecute", "Enabled" };
                    foreach (string x in validProperties)
                    {
                        if (x == propertyName)
                        {
                            // Type t = obj.GetType();
                            //Get information of the property on which the condition is specified. The property belongs to the object 
                            //PropertyInfo pInfo = t.GetProperty(propertyName);
                            //Type propType = pInfo.PropertyType;

                            return true;
                        }
                    }
                    return false;
                }
                if (obj is BSkyListBoxwBorderForDatasets )
                {
                    validProperties = new string[] { "maxNoOfVariables", "ItemsCount", "MoveVariables", "CanExecute", "Enabled" };
                    foreach (string x in validProperties)
                    {
                        if (x == propertyName)
                        {
                            // Type t = obj.GetType();
                            //Get information of the property on which the condition is specified. The property belongs to the object 
                            //PropertyInfo pInfo = t.GetProperty(propertyName);
                            //Type propType = pInfo.PropertyType;

                            return true;
                        }
                    }
                    return false;
                }
                else if (obj is BSkyListBox)
                {
                    validProperties = new string[] { "SelectedItemsCount", "CanExecute", "Enabled" };
                    foreach (string x in validProperties)
                    {
                        if (x == propertyName) return true;
                    }
                    return false;
                }
                else if (obj is BSkyTextBox)
                {
                    validProperties = new string[] { "Text", "CanExecute", "Enabled" };
                    foreach (string x in validProperties)
                    {
                        if (x == propertyName) return true;
                    }
                    return false;


                }
                else if (obj is BSkySpinnerCtrl)
                {
                    validProperties = new string[] { "Text", "CanExecute", "Enabled" };
                    foreach (string x in validProperties)
                    {
                        if (x == propertyName) return true;
                    }
                    return false;


                }

                else if (obj is BSkySlider)
                {
                    validProperties = new string[] { "Value", "CanExecute", "Enabled" };
                    foreach (string x in validProperties)
                    {
                        if (x == propertyName) return true;
                    }
                    return false;


                }
                else if (obj is BSkyAdvancedSlider)
                {
                    validProperties = new string[] { "SliderValue", "CanExecute", "Enabled" };
                    foreach (string x in validProperties)
                    {
                        if (x == propertyName) return true;
                    }
                    return false;


                }

                else if (obj is BSkyEditableComboBox)
                {
                    validProperties = new string[] { "DefaultSelection", "NoItemsSelected", "CanExecute", "Enabled" };
                    foreach (string x in validProperties)
                    {
                        if (x == propertyName) return true;
                    }
                    return false;
                }
                else if (obj is BSkyNonEditableComboBox)
                {
                    validProperties = new string[] { "NoItemsSelected", "CanExecute", "Enabled" };
                    foreach (string x in validProperties)
                    {
                        if (x == propertyName) return true;
                    }
                    return false;
                }


                else if (obj is BSkyCheckBox)
                {
                    validProperties = new string[] { "Text", "IsSelected", "CanExecute", "Enabled" };
                    foreach (string x in validProperties)
                    {
                        if (x == propertyName) return true;
                    }
                    return false;
                }
                else if (obj is BSkyRadioButton)
                {
                    validProperties = new string[] { "Text", "IsSelected", "CanExecute", "Enabled" };
                    foreach (string x in validProperties)
                    {
                        if (x == propertyName) return true;
                    }
                    return false;
                }
                else if (obj is BSkyRadioGroup)
                {
                    validProperties = new string[] { "Text", "IsSelected", "CanExecute", "Enabled" };
                    BSkyRadioGroup ic = obj as BSkyRadioGroup;
                    StackPanel stkpanel = ic.Content as StackPanel;
                    foreach (object obj1 in stkpanel.Children)
                    {
                        BSkyRadioButton btn = obj1 as BSkyRadioButton;
                        foreach (string x in validProperties)
                        {
                            if (x == propertyName) return true;
                        }
                    }
                    return false;
                }


                else if (obj is BSkyButton)
                {
                    validProperties = new string[] { "CanExecute", "Enabled" };
                    foreach (string x in validProperties)
                    {
                        if (x == propertyName) return true;
                    }
                    return false;
                }
                else if (obj is BSkyBrowse)
                {
                    validProperties = new string[] { "CanExecute", "Enabled" };
                    foreach (string x in validProperties)
                    {
                        if (x == propertyName) return true;
                    }
                    return false;
                }
                else if (obj is BSkyGroupingVariable)
                {
                    validProperties = new string[] { "CanExecute", "Enabled", "ItemsCount" };
                    foreach (string x in validProperties)
                    {
                        if (x == propertyName) return true;
                    }
                    return false;
                }
                else if (obj is BSkyAggregateCtrl)
                {
                    validProperties = new string[] { "CanExecute", "Enabled", "ItemsCount" };
                    foreach (string x in validProperties)
                    {
                        if (x == propertyName) return true;
                    }
                    return false;
                }

                else if (obj is BSkySortCtrl)
                {
                    validProperties = new string[] { "CanExecute", "Enabled", "ItemsCount" };
                    foreach (string x in validProperties)
                    {
                        if (x == propertyName) return true;
                    }
                    return false;
                }
                else if (obj is BSkygridForSymbols)
                {
                    validProperties = new string[] { "CanExecute", "Enabled" };
                    foreach (string x in validProperties)
                    {
                        if (x == propertyName) return true;
                    }
                    return false;
                }
                else return false;
            }
            else return false;
            //  PropertyInfo [] propertyholder =   BSky.Controls.Window1.selectedElementRef.GetType().GetProperties(); 
            // PropertyInfo[] prop_info_array = obj.GetType().GetProperties();
            //PropertyInfo proptemp = obj.GetType().GetProperty("propertylist");
            //  foreach (PropertyInfo pInfo in prop_info_array)
            // {
            //m = (CategoryAttribute[])pInfo.GetCustomAttributes(typeof(CategoryAttribute), false);
            // AttributeCollection att_coll = TypeDescriptor.GetAttributes(pInfo);

            //   if (pInfo.Name == propertyName) return true;
            // }
            //    if (success == false)
            //      //  System.Windows.Forms.MessageBox.Show("You have entered an invalid property name, please review the property names in                        the grid and enter a valid value");
            //    return false;
            //}
            //return false;

        }

        //Added by Aaron 11/14/2013
        //Looks for a valid property name on the canvas and all its chrildren
        public bool valPropName(string name, BSkyCanvas canvas)
        {
            foreach (FrameworkElement obj in canvas.Children)
            {
                if (valPropofCtrl(obj, name) == true) return true;
                if (obj is BSkyRadioGroup)
                {
                    BSkyRadioGroup ic = obj as BSkyRadioGroup;
                    StackPanel stkpanel = ic.Content as StackPanel;
                    foreach (FrameworkElement obj1 in stkpanel.Children)
                    {
                        if (valPropofCtrl(obj, name) == true) return true;
                    }
                }
                if (obj is BSkyButton)
                {
                    FrameworkElement fe = obj as FrameworkElement;
                    BSkyCanvas cs = fe.Resources["dlg"] as BSkyCanvas;
                    if (cs != null) valPropName(name, cs);
                }
            }
            return false;
        }

        //Gets the control name
        //While looking at the resources of a button recursively works, it does not capture new sub dialofs whose
        //canvas has not been saved to the button resources. The canvas gets saved to the button resources on close of the sub dialog
        //chainOpenCanvas contains the chain of open canvas. The first canvas is always open and is the starting point.
        public FrameworkElement getCtrl(string ControlName)
        {
            FrameworkElement retval = null;
            int len = 0;
            int i = 0;
            if (BSkyCanvas.chainOpenCanvas.Count > 0)
            {
                len = BSkyCanvas.chainOpenCanvas.Count;
                while (i < len)
                {
                    retval = returnCtrl(ControlName, BSkyCanvas.chainOpenCanvas[i]);
                    if (retval != null) return retval;
                    i = i + 1;

                }
            }
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
    }
}
