using System.Windows.Controls;
using System.Windows;

namespace BSky.Controls.DesignerSupport
{


    public class ManageTextBoxSubstitution : PropertyEditorBase
    {
        private static int count = 0;
        public ManageTextBoxSubstitution()
        {
        }
        protected override Control GetEditControl(string PropName, object CurrentValue, object CurrentObj)
        {
            textboxsubstitution w;
            ObjectWrapper placeHolder = CurrentObj as ObjectWrapper;
            // DragDropList selectedElement = placeHolder.SelectedObject as DragDropList;
          BSkyTextBox tb = placeHolder.SelectedObject as BSkyTextBox;
                //Added by Aaron 10/10/2013
                //This ensures that the variable filer dialog is opened with the correct filter settings for the number of 
                //ordinal and nominal levels
                w = new textboxsubstitution(tb.PrefixTxt);
           
            if (CurrentValue == null) w.SubstituteSettings = "";
            else
            w.SubstituteSettings = CurrentValue.ToString();
            return w;
        }

        protected override object GetEditedValue(Control EditControl, string PropertyName, object oldValue, object currentObj)
        {
            ObjectWrapper placeHolder = currentObj as ObjectWrapper;
           
            // DragDropList selectedElement = placeHolder.SelectedObject as DragDropList;
            BSkyTextBox tb = placeHolder.SelectedObject as BSkyTextBox;

            if (EditControl is textboxsubstitution)
            {
                textboxsubstitution w = EditControl as textboxsubstitution;
                tb.PrefixTxt = w.PrefixString.Text;
                FrameworkElement selectedElement = currentObj as FrameworkElement;
                if (w.DialogResult.HasValue && w.DialogResult.Value)
                {
                    return w.SubstituteSettings;
                }
                return oldValue;
            }
            return false;
        }
    }
}
