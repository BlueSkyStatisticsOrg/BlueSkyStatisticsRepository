
namespace BSky.Interfaces.Controls
{
    
    //The following controls inherit from IBSKyInputControl
    //BSKyCheckBox, BSKyComboBox, BSkyGroupingVariable, BSkyRadioButton, BSkyRadioGroup, BSkyTextBox, BSkyTargetList, BSkySourceList
    public interface IBSkyInputControl
    {
        string Syntax { get; set; }
        string Name { get; set; }
        
    }
}
