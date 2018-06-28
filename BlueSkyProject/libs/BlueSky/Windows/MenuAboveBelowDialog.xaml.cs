using System.Windows;

namespace BlueSky.Windows
{
    /// <summary>
    /// Interaction logic for MenuAboveBelowDialog.xaml
    /// </summary>
    public partial class MenuAboveBelowDialog : Window
    {
        public MenuAboveBelowDialog()
        {
            InitializeComponent();
            selectedOption = "Below";//default selection
        }

        public void SetTitleMessage(string titlemessage) 
        {
            message.Text = titlemessage;
        }
        string selectedOption;
        public string SelectedOption
        {
            get { return selectedOption; }
        }

        private void okBut_Click(object sender, RoutedEventArgs e)
        {
            if (radioAbove.IsChecked == true)
                selectedOption = "Above";
            else if (radiodBelow.IsChecked == true)
                selectedOption = "Below";
            else
                selectedOption = string.Empty;
        }

        private void CancelBut_Click(object sender, RoutedEventArgs e)
        {
            selectedOption = string.Empty;
        }
    }
}
