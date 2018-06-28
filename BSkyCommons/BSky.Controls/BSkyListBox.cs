using System.ComponentModel;

namespace BSky.Controls
{
    [TypeConverter(typeof(PropertySorter))]
    [DefaultPropertyAttribute("Type")]
    public class BSkyListBox:CtrlListBox
    {
        public BSkyListBox()
        {
            
         }

        //[Description("ListBox Control allows you to display a listbox with pre-configured items. This is a read only property. Click on each property in the grid to see the configuration options for this listbox control. ")]
        
        [BSkyLocalizedDescription("BSkyListBox_TypeDescription", typeof(BSky.GlobalResources.Properties.Resources))]
        //[BSkyLocalizedCategory("BSkyListBox_TypeCategory", typeof(BSky.GlobalResources.Properties.Resources)), PropertyOrder(1)]
        [Category("Control Settings"), PropertyOrder(1)]
        public string Type
        {
            get
            {
                return "ListBox Control";
            }
        }
    }
}
