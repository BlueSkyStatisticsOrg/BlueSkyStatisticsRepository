using System.Windows;

namespace BSky.Controls.DesignerSupport
{
    /// <summary>
    /// Interaction logic for substitutionsettings.xaml
    /// </summary>
    public partial class OverwriteSettings : Window
    {
        public OverwriteSettings(string settings)
        {
            InitializeComponent();
           // PrefixString.Text = settings;
            OverwriteSetting = settings;

        }


        public string OverwriteSetting
        {
            get
            {
                string str = string.Empty;
                if (dontprompt.IsChecked.HasValue & dontprompt.IsChecked.Value)
                    str += "DontPrompt";
                if (PromptOverwriteVars.IsChecked.HasValue & PromptOverwriteVars.IsChecked.Value)
                    str += "PromptBeforeOverwritingVariables";
                if (PromptOverwriteDatasets.IsChecked.HasValue & PromptOverwriteDatasets.IsChecked.Value)
                    str += "PromptBeforeOverwritingDatasets";
                // if (chkNominal.IsChecked.HasValue & chkNominal.IsChecked.Value)
                //   str += "Nominal" + "|";
                //if (chkScale.IsChecked.HasValue & chkScale.IsChecked.Value)
                //  str += "Scale";

                //str = str.Trim('|');
                return str;
            }
            set
            {
                string str = value as string;
                if (str.Contains("DontPrompt"))
                    dontprompt.IsChecked = true;
                if (str.Contains("PromptBeforeOverwritingVariables"))
                    PromptOverwriteVars.IsChecked = true;
                if (str.Contains("PromptBeforeOverwritingDatasets"))
                    PromptOverwriteDatasets.IsChecked = true;
                //if (strs.Contains("Scale"))
                //    chkScale.IsChecked = true;
            }
        }


        private void button1_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }



    }
}
