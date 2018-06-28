using System;
using System.Drawing.Design;
using System.Windows.Controls;
using System.Windows;

namespace BSky.Controls.DesignerSupport
{
    public abstract class PropertyEditorBase : UITypeEditor
    {
        protected abstract Control GetEditControl(string PropName, object CurrentValue, object currentObj);
        protected abstract object GetEditedValue(Control EditControl, string PropertyName, object oldValue,object currentObj);
        

        public override UITypeEditorEditStyle GetEditStyle(System.ComponentModel.ITypeDescriptorContext context)
        {
                return UITypeEditorEditStyle.Modal;
        }

        public override object EditValue(System.ComponentModel.ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            if (context != null && provider != null)
            {
                    string propName = context.PropertyDescriptor.Name;
                    Window _editControl = GetEditControl(propName, value,context.Instance) as Window;
                    if (_editControl == null)
                        return null;
                    _editControl.ShowDialog();
                    return GetEditedValue(_editControl, propName, value,context.Instance);
            }
            return null;
        }
    }
}
