using System.Windows.Controls;
using System.Windows;


namespace BSky.Controls.DesignerSupport
{
    public class ListBoxEditor : PropertyEditorBase
    {
        private static int count = 0;
        public ListBoxEditor()
        {
        }
        protected override Control GetEditControl(string PropName, object CurrentValue,object CurrentObj)
        {
            ListBoxValueCollection col = new ListBoxValueCollection();
            //Aaron added 11/11/2013
                //Object wrapper is a wrapper object used to show categories in the grid
                //We need to extract the button object from the wrapper
            ObjectWrapper placeHolder = CurrentObj as ObjectWrapper;
           CtrlListBox rg = placeHolder.SelectedObject as CtrlListBox;
            //Aaron added 11/11/2013
            //Commented code below
          //  BSkyRadioGroup rg = CurrentObj as BSkyRadioGroup;
            if (string.IsNullOrEmpty(rg.Name))
            {
                MessageBox.Show(BSky.GlobalResources.Properties.Resources.EnterValidValue);
                return null;
            }

            //Aaron 05/05/2013
            //Did not change code only added comment below
            //This launches the radio group editor window that allows the use to enter details about the radio group

            ListBoxEditorWindow w = new ListBoxEditorWindow();
            //StackPanel sp = rg.Content as StackPanel;
            //Added by Aaron 05/05/2013
            //Did not change code only added comment below
            //If there were existing radio buttons, the code below populates the windows with existing radio buttons  
            foreach (string obj in rg.Items)
            {
                ListBoxEntry entry = new ListBoxEntry();
                entry.entryName = obj;

                col.Add(entry);
            }
            //Added by Aaron 05/05/2013
            //Did not change code only added comment below
            //the line below adds the BSkyRadioButtonCollection to the observable collection of radio buttons
            //public ObservableCollection<BSkyRadioButton> RadioCollection
            //The observable collection is populated in the setter of 
            w.ListBoxEntries = col;
            //Aaron 12/26 ADDED AND commented lines below
            // rg.RadioButtons = col;
            return w;
        }


        //05/18/2013
        //oldvalue is null initially
        //you don't have to return any values
        //The main purpose of this function is to populate sp.Children.Add(tmp); with the radio buttons

        protected override object GetEditedValue(Control EditControl, string PropertyName, object oldValue,object currentObj)
        {
            if (EditControl is ListBoxEditorWindow)
            {
                ListBoxEditorWindow w = EditControl as ListBoxEditorWindow;
                //Aaron added 11/11/2013
                //Object wrapper is a wrapper object used to show categories in the grid
                //We need to extract the button object from the wrapper
                ObjectWrapper placeHolder = currentObj as ObjectWrapper;
                FrameworkElement selectedElement = placeHolder.SelectedObject as FrameworkElement;
                //Aaron added 11/11/2013
                //Commented code below
                //FrameworkElement selectedElement = currentObj as FrameworkElement;
                if (w.DialogResult.HasValue && w.DialogResult.Value)
                {
                    CtrlListBox rg = selectedElement as CtrlListBox;
                   // StackPanel sp = rg.Content as StackPanel;
                    ListBoxValueCollection col = w.ListBoxEntries;
                    int count = col.Count;
                    rg.Items.Clear();
                    int i=0;

                    foreach (ListBoxEntry entry in col)
                        rg.Items.Add(entry.entryName);
                    if (col.Count > 0) rg.SelectedIndex = 1;      
                    
                    //05/18/2013
                    //Added by Aaron
                    //Code below works as follows
                    //Step 1: Outer loop, for each item in radio group, loop through the rest of the radio group, looking for a 
                    //duplicate name. If not found, look through the entire dialog and all sub-dialogs looking for a duplicate name.
                    //If a duplicate name is  found, show an error message and revert to the original state of the radiogroup.

                    //Note: THIS FUNCTION STOPS PROCESSING AS SOON AS A DUPLICATE IS FOUND. THIS MEANS IF YOU HAVE ENTERED 6, VALUES AND THE 
                    //6TH VALUE IS A DUPLICATE, 5 VALUES WILL BE SAVED.
                    //IF THE 1ST VALUE IS A DUPLICATE, ALL 6 VALUES ENTERED WILL BE LOST AND YOU WILL HAVE TO REENTER
                   
                   // for (i = 0; i < count; i++)
                   //// foreach (object obj in col)
                   // {
                   //     int j = 0;
                   //     j = i;
                   //     string message;
                   //     BSkyRadioButton tmp = col[i] as BSkyRadioButton;
                   //     while(j <count -1)
                   //     {
                   //        BSkyRadioButton tmp2 = col[j + 1] as BSkyRadioButton;
                   //        if (tmp.Name == tmp2.Name)
                   //        {
                   //           message = string.Format("You have already created a control with the name \"{0}\", please enter a unique name", tmp.Name);
                   //           MessageBox.Show(message);
                   //           //  this.OptionsPropertyGrid.ResetSelectedProperty();
                   //           return oldValue;
                   //        }
                   //        j = j + 1;
                   //     }
                   //     tmp.Margin = new Thickness(2);
                   //     tmp.GroupName = rg.Name;
                   //     if (!checkDuplicateNameInRdGrp(BSky.Controls.Window1.firstCanvas, tmp.Name))
                   //         sp.Children.Add(tmp);
                   //     else return oldValue;
                   // }
                    
                 //   rg.Height = sp.Children.Count * 30 + 20; 
                    return null;
                }
                return oldValue;
            }
            return false;
        }


        //private bool checkDuplicateNameInRdGrp(BSkyCanvas canvas, string name)
        //{
        //    string message;
        //    foreach (Object obj in canvas.Children)
        //    {
        //       // if (obj is IBSkyControl && obj != selectedElement)
        //        if (obj is IBSkyControl)
        //        {
        //            IBSkyControl ib = obj as IBSkyControl;
        //            //if (ib.Name == e.ChangedItem.Value.ToString())
        //            if (ib.Name == name)
        //            {
        //                message = string.Format("You have already created a control with the name \"{0}\", please enter a unique name", name);
        //                MessageBox.Show(message);
        //              //  this.OptionsPropertyGrid.ResetSelectedProperty();
        //                return true;
        //            }
        //        }

        //        //05/18/2013
        //        //Added by Aaron
        //        //Code below checks the radio buttons within each radiogroup looking for duplicate names
        //        if (obj is BSkyRadioGroup)
        //        {
        //            BSkyRadioGroup ic = obj as BSkyRadioGroup;
        //            StackPanel stkpanel = ic.Content as StackPanel;

        //            foreach (object obj1 in stkpanel.Children)
        //            {
        //                BSkyRadioButton btn = obj1 as BSkyRadioButton;
        //                if (btn.Name == name)
        //                {
        //                    message = string.Format("You have already created a control with the name \"{0}\", please enter a unique name", name);
        //                    MessageBox.Show(message);
        //                    return true;
        //                }
        //            }

        //        }
        //        if (obj is BSkyButton)
        //        {
        //            FrameworkElement fe = obj as FrameworkElement;
        //            BSkyCanvas cs = fe.Resources["dlg"] as BSkyCanvas;
        //            if (cs != null)
        //            {
        //                if (checkDuplicateNameInRdGrp(cs, name)) return true;
        //            }
        //        }
        //    }
        //    return false;
        //}


    }
}

