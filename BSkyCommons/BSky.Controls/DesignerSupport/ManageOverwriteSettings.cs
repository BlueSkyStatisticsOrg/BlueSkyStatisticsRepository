using System.Windows.Controls;
using System.Windows;

namespace BSky.Controls.DesignerSupport
{


    public class ManageOverwriteSettings : PropertyEditorBase
    {
        private static int count = 0;
        public ManageOverwriteSettings()
        {
        }
        protected override Control GetEditControl(string PropName, object CurrentValue, object CurrentObj)
        {
            OverwriteSettings w;
            ObjectWrapper placeHolder = CurrentObj as ObjectWrapper;
            // DragDropList selectedElement = placeHolder.SelectedObject as DragDropList;

            if (placeHolder.SelectedObject is BSkyTextBox)
            {

                BSkyTextBox tb = placeHolder.SelectedObject as BSkyTextBox;
                w = new OverwriteSettings(tb.OverwriteSettings);
            }
            else
            {
                DragDropList dd = placeHolder.SelectedObject as DragDropList;
                w = new OverwriteSettings(dd.OverwriteSettings);
            }


            //if (CurrentValue == null) w.SubstituteSettings = "";
            //else
            //    w.SubstituteSettings = CurrentValue.ToString();
            return w;
        }

        protected override object GetEditedValue(Control EditControl, string PropertyName, object oldValue, object currentObj)
        {
            ObjectWrapper placeHolder = currentObj as ObjectWrapper;
            if (placeHolder.SelectedObject is BSkyTextBox)
            {
                BSkyTextBox tb = placeHolder.SelectedObject as BSkyTextBox;
                //if (w.OverwriteVariables.is

                //tb.PromptforOverwrite=
                //w = new OverwriteSetting(tb.PromptforOverwrite);

            }
            else if (placeHolder.SelectedObject is DragDropList)
            {
                DragDropList dd = placeHolder.SelectedObject as DragDropList;
                //  w = new OverwriteSetting(dd.PromptforOverwrite);
            }

            if (EditControl is OverwriteSettings)
            {
                OverwriteSettings w = EditControl as OverwriteSettings;


                //tb.PrefixTxt = w.PrefixString.Text;
                FrameworkElement selectedElement = currentObj as FrameworkElement;
                if (w.DialogResult.HasValue && w.DialogResult.Value)
                {
                    return w.OverwriteSetting;
                }
                return oldValue;
            }
            return false;
        }
    }
}
