using System.Windows.Media;
using System.Windows;

namespace BSky.Interfaces.Controls
{
    public interface IAUControl
    {
        string ControlType
        {
            get;
            set;
        }
        string NodeText { get; set; }  /// for treeview strings 02Aug2012
        Thickness outerborderthickness { get; set; } // border shown when element comes into focus
        SolidColorBrush controlsselectedcolor { get; set; } //set unset border color to show if itme is selected in left tree
        SolidColorBrush controlsmouseovercolor { get; set; } // mouseover color
        SolidColorBrush bordercolor { get; set; } // this will use above two color for different events.
        System.Windows.Visibility BSkyControlVisibility { get; set; } // set visibility in output window
        bool DeleteControl { get; set; } //marking it for deleting		
    }
}
