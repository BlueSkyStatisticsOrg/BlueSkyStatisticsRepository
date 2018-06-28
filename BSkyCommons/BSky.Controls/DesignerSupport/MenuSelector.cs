using System.Windows.Controls;

namespace BSky.Controls.DesignerSupport
{
    public class MenuSelector : PropertyEditorBase
    {
        private static int count = 0;
        public MenuSelector()
        {
        }
        protected override Control GetEditControl(string PropName, object CurrentValue,object CurrentObj)
        {
            
            MenuEditor w = new MenuEditor();
            if(CurrentValue != null)
                w.ElementLocation = CurrentValue.ToString();
            return w;
        }

        protected override object GetEditedValue(Control EditControl, string PropertyName, object oldValue,object currentObj)
        {
            if (EditControl is MenuEditor)
            {
                MenuEditor w = EditControl as MenuEditor;
                if (w.DialogResult.HasValue && w.DialogResult.Value)
                {
                    return w.ElementLocation;
                }
                return oldValue;
            }
            return oldValue;
        }
    }
}
