using System.Windows.Controls;
using BSky.Interfaces.Commands;

namespace BSky.Controls.DesignerSupport
{
    public class CommandSelector : PropertyEditorBase
    {
        private static int count = 0;
        public CommandSelector()
        {
        }
        protected override Control GetEditControl(string PropName, object CurrentValue,object CurrentObj)
        {
            RCommandDialog w = new RCommandDialog();
            if(CurrentValue != null)
                w.CommandString = CurrentValue.ToString();
            if (CurrentObj != null)
                w.Canvas = CurrentObj as BSkyCanvas;
            return w;
        }

        protected override object GetEditedValue(Control EditControl, string PropertyName, object oldValue,object currentObj)
        {
            if (EditControl is RCommandDialog)
            {
                RCommandDialog w = EditControl as RCommandDialog;
                
                if (w.DialogResult.HasValue && w.DialogResult.Value)
                {
                    return w.CommandString;
                }
                return oldValue;
            }
            return oldValue;
        }
    }
}
