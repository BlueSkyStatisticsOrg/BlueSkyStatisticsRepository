
namespace BSky.Interfaces.Controls
{

  
    //All controls that can be enabled inherit from IBSkyControl 
    //BSKyCheckBox, BSKyComboBox, BSkyGroupingVariable, BSkyRadioButton,BSkyTextBox, BSkySourceList, BSkyTargetList
    //BSkyButton, BSkyBrowse, 
    public interface IBSkyEnabledControl
    {
        string Name { get; set; }
        bool Enabled { get; set; }
        bool IsEnabled { get; set; }


    }
}
