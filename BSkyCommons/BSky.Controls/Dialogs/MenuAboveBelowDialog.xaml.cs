using System.Windows;

namespace BSky.Controls.Dialogs
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

        public MenuAboveBelowDialog(string source, string target) //24Jan2013
        {
            InitializeComponent();
            selectedOption = "Below";//default selection
            radioAbove.Content = "Above \'" + target + "\'(as a sibling)";
            radioBelow.Content = "Below \'" + target + "\'(as a sibling)";
            radioInside.Content = "Inside \'" + target + "\' (as a child)";
            string titlemessage = "Where would you like to drop \'" + source + "\' in \'" + target+"\'?";
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
            else if (radioBelow.IsChecked == true)
                selectedOption = "Below";
            else if (radioInside.IsChecked == true)//10Sep2014
                selectedOption = "Inside";
            else
                selectedOption = string.Empty;
            this.Close();
        }

        private void CancelBut_Click(object sender, RoutedEventArgs e)
        {
            selectedOption = string.Empty;
            this.Close(); 
        }
    }
}
