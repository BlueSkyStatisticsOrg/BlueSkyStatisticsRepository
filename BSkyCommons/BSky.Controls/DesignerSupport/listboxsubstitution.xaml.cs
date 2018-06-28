using System.Linq;
using System.Windows;

namespace BSky.Controls.DesignerSupport
{
    /// <summary>
    /// Interaction logic for substitutionsettings.xaml
    /// </summary>
    public partial class listboxsubstitution : Window
    {
        public listboxsubstitution()
        {
            InitializeComponent();
        }

        public string SubstituteSettings
        {
            get
            {
                string str = string.Empty;
               
                if (UsePlus.IsChecked.HasValue & UsePlus.IsChecked.Value)
                    str += "UsePlus" + "|";
                if (UseComma.IsChecked.HasValue & UseComma.IsChecked.Value)
                    str += "UseComma" + "|";
                if (encloseByCharacters.IsChecked.HasValue & encloseByCharacters.IsChecked.Value)
                    str += "Enclosed" + "|";
                // if (chkNominal.IsChecked.HasValue & chkNominal.IsChecked.Value)
                //   str += "Nominal" + "|";
                //if (chkScale.IsChecked.HasValue & chkScale.IsChecked.Value)
                //  str += "Scale";

                str = str.Trim('|');
                return str;
            }
            set
            {
                string[] strs = value.Split('|');
                if (strs.Contains("UsePlus"))
                    UsePlus.IsChecked = true;
                if (strs.Contains("UseComma"))
                    UseComma.IsChecked = true;
                if (strs.Contains("Enclosed"))
                    encloseByCharacters.IsChecked = true;
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
