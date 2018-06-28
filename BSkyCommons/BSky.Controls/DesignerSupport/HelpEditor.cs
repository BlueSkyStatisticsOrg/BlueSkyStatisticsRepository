using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BSky.Controls.DesignerSupport
{
    class HelpEditor : PropertyEditorBase
    {
        protected override System.Windows.Controls.Control GetEditControl(string PropName, object CurrentValue, object currentObj)
        {
            DialogHelpTextWindow helpwin = new DialogHelpTextWindow();
            if (CurrentValue != null)
                helpwin.FormattedHelpHTMLText = CurrentValue.ToString();
            if (currentObj != null)
                helpwin.Canvas = currentObj as BSkyCanvas;
            return helpwin;
        }

        protected override object GetEditedValue(System.Windows.Controls.Control EditControl, string PropertyName, object oldValue, object currentObj)
        {
            if (EditControl is DialogHelpTextWindow)
            {
                DialogHelpTextWindow helpwin = EditControl as DialogHelpTextWindow;

                if (helpwin.DialogResult.HasValue && helpwin.DialogResult.Value)
                {
                    return helpwin.FormattedHelpHTMLText;
                }
                return oldValue;
            }
            return oldValue;
        }
    }
}
