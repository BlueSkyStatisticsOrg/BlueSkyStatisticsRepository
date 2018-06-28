using System.Linq;
using System.Windows;

namespace BSky.Controls.DesignerSupport
{
    /// <summary>
    /// Interaction logic for substitutionsettings.xaml
    /// </summary>
    public partial class textboxsubstitution : Window
    {
        public textboxsubstitution(string prefix)
        {
            InitializeComponent();
            PrefixString.Text = prefix;

        }
       

        public string SubstituteSettings
        {
            get
            {
                string str = string.Empty;
                if (TextAsIs.IsChecked.HasValue & TextAsIs.IsChecked.Value)
                    str += "TextAsIs" + "|";
                if (PrefixByDataset.IsChecked.HasValue & PrefixByDataset.IsChecked.Value)
                    str += "PrefixByDatasetName" + "|";
                if (CreateArray.IsChecked.HasValue & CreateArray.IsChecked.Value)
                    str += "CreateArray" + "|";

                if (PrefixByString.IsChecked.HasValue & PrefixByString.IsChecked.Value)
                    str += "PrefixByString" + "|";
                // if (chkNominal.IsChecked.HasValue & chkNominal.IsChecked.Value)
                //   str += "Nominal" + "|";
                //if (chkScale.IsChecked.HasValue & chkScale.IsChecked.Value)
                //  str += "Scale";
                if (EncloseBrackets.IsChecked.HasValue & EncloseBrackets.IsChecked.Value)
                    str += "Brackets" + "|";

                str = str.Trim('|');
                return str;
            }
            set
            {
                string[] strs = value.Split('|');
                if (strs.Contains("TextAsIs"))
                    TextAsIs.IsChecked = true;
                if (strs.Contains("PrefixByDatasetName"))
                    PrefixByDataset.IsChecked = true;
                if (strs.Contains("CreateArray"))
                    CreateArray.IsChecked = true;
                if (strs.Contains("PrefixByString"))
                    PrefixByString.IsChecked = true;
                //if (strs.Contains("Scale"))
                //    chkScale.IsChecked = true;
                if (strs.Contains("Brackets"))
                    EncloseBrackets.IsChecked = true;
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

