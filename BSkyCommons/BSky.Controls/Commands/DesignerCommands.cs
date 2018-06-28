using System.Windows.Input;

namespace BSky.Controls.Commands
{
    public static class DesignerCommands
    {
        public static readonly RoutedUICommand DesignDialog = new RoutedUICommand("Design Dialog", "DialogDesigner", typeof(Window1));
        public static readonly RoutedUICommand SaveAndPackage = new RoutedUICommand("Save & Package", "PackageSave", typeof(Window1));
        public static readonly RoutedUICommand RCommand = new RoutedUICommand("Command", "Command", typeof(Window1));
        public static readonly RoutedUICommand Preview = new RoutedUICommand("Preview1", "Preview1", typeof(Window1));
        public static readonly RoutedUICommand MenuLocation = new RoutedUICommand("Menu Location", "MenuLocation", typeof(Window1));
        public static readonly RoutedUICommand OutputDefinition = new RoutedUICommand("Output Definition File", "OutputDefinition", typeof(Window1));
        public static readonly RoutedUICommand SaveSubDialog = new RoutedUICommand("Save Sub Dialog", "Save & Close", typeof(Window1));
        public static readonly RoutedUICommand Exit = new RoutedUICommand("Exit Application", "Exit", typeof(Window1));
        public static readonly RoutedUICommand Inspection = new RoutedUICommand("InspectDlg", "InspectDlg", typeof(Window1));
        public static readonly RoutedUICommand EnableGridLines = new RoutedUICommand("EnableGridLines1", "EnableGridLines1", typeof(Window1));
        public static readonly RoutedUICommand removeGridLines = new RoutedUICommand("removeGridLines1", "removeGridLines1", typeof(Window1));
    }
}
    