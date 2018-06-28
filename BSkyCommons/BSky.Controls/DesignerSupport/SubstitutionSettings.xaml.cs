using System.Linq;
using System.Windows;

namespace BSky.Controls.DesignerSupport
{
    /// <summary>
    /// Interaction logic for substitutionsettings.xaml
    /// </summary>
    public partial class SubstitutionSettings : Window
    {
        public SubstitutionSettings(string prefixtxt, string SepCharacter)
        {
            InitializeComponent();
            this.PrefixString.Text = prefixtxt;
            this.SepCharacter.Text = SepCharacter;
            
            
        }

        public string SubstituteSettings
        {
            get
            {
                string str = string.Empty;
                if (NoPrefix.IsChecked.HasValue & NoPrefix.IsChecked.Value)
                    str += "NoPrefix" + "|";
                if (Prefix.IsChecked.HasValue & Prefix.IsChecked.Value)
                    str += "Prefix" + "|";
                if (custFormat.IsChecked.HasValue & custFormat.IsChecked.Value)
                    str += "CustomFormat" + "|";
                if (UsePlus.IsChecked.HasValue & UsePlus.IsChecked.Value)
                    str += "UsePlus" + "|";
                if (UseComma.IsChecked.HasValue & UseComma.IsChecked.Value)
                    str += "UseComma" + "|";
                if (encloseByCharacters.IsChecked.HasValue & encloseByCharacters.IsChecked.Value)
                    str += "Enclosed" + "|";
                if (StringPrefix.IsChecked.HasValue & StringPrefix.IsChecked.Value)
                    str += "StringPrefix" + "|";
                if (CreateArray.IsChecked.HasValue & CreateArray.IsChecked.Value)
                    str += "CreateArray" + "|";
                if (UseSeperator.IsChecked.HasValue & UseSeperator.IsChecked.Value)
                    str += "UseSeperator" + "|";
                if (EncloseBrackets.IsChecked.HasValue & EncloseBrackets.IsChecked.Value)
                    str += "Brackets" + "|";
               // if (chkNominal.IsChecked.HasValue & chkNominal.IsChecked.Value)
                 //   str += "Nominal" + "|";
                //if (chkScale.IsChecked.HasValue & chkScale.IsChecked.Value)
                  //  str += "Scale";

               str= str.Trim('|');
                return str;
            }
            set
            {
                string[] strs = value.Split('|');
                if (strs.Contains("NoPrefix"))
                    NoPrefix.IsChecked = true;
                if (strs.Contains("Prefix"))
                    Prefix.IsChecked = true;
                if (strs.Contains("CustomFormat"))
                    custFormat.IsChecked = true;
                if (strs.Contains("UsePlus"))
                    UsePlus.IsChecked = true;
                if (strs.Contains("UseComma"))
                    UseComma.IsChecked = true;
                if (strs.Contains("Enclosed"))
                 encloseByCharacters.IsChecked = true;
                if (strs.Contains("StringPrefix"))
                    StringPrefix.IsChecked = true;
                if (strs.Contains("CreateArray"))
                    CreateArray.IsChecked = true;
                if (strs.Contains("UseSeperator"))
                    UseSeperator.IsChecked = true;
                if (strs.Contains("Brackets"))
                    EncloseBrackets.IsChecked = true;
                
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

        private void checkBox1_Checked(object sender, RoutedEventArgs e)
        {

        }



    }
}
