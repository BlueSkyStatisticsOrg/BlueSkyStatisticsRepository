using System.Windows.Controls;
using System.Windows;

namespace BSky.Controls.DesignerSupport
{


    public class ManageListBoxSubstitution : PropertyEditorBase
    {
        private static int count = 0;
        public ManageListBoxSubstitution()
        {
        }
        protected override Control GetEditControl(string PropName, object CurrentValue, object CurrentObj)
        {
            listboxsubstitution w = new listboxsubstitution();
            w.SubstituteSettings = CurrentValue.ToString();
            return w;
        }

        protected override object GetEditedValue(Control EditControl, string PropertyName, object oldValue, object currentObj)
        {
            if (EditControl is listboxsubstitution)
            {
                listboxsubstitution w = EditControl as listboxsubstitution;
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
