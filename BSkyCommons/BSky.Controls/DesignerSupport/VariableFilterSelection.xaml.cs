using System;
using System.Windows;

namespace BSky.Controls
{
    public partial class VariableFilterSelection : Window
    {
        public VariableFilterSelection(string nomlevels, string ordlevels)
        {
            InitializeComponent();
            //Aaron 10/08/2013
            //Code below is used to bring up the variable filter settings dialog with the correct number of levels when the number of levels are set previously
            //The number of levels are saved in a property of the dragdrop list
            chkNomlevels.Text = nomlevels;
            chkordlevels.Text = ordlevels;

        }

        public string Filter
        {
            get
            {
                double result;
                string str =string.Empty;
                bool isdouble =false;
                string message = string.Empty;

                if (chkString.IsChecked.HasValue & chkString.IsChecked.Value)
                    str += "String" + "|";
                if (chkNumeric.IsChecked.HasValue & chkNumeric.IsChecked.Value)
                    str += "Numeric" + "|";
                if (chkDate.IsChecked.HasValue & chkDate.IsChecked.Value)
                    str += "Date" + "|";
                if (chkLogical.IsChecked.HasValue & chkLogical.IsChecked.Value)
                    str += "Logical" + "|";
                if (chkOrdinal.IsChecked.HasValue & chkOrdinal.IsChecked.Value)
                {
                    if (chkordlevels.Text != null && chkordlevels.Text != string.Empty)
                    {
                        isdouble = Double.TryParse(chkordlevels.Text, out result);
                        if (!isdouble)
                        {
                            message = "You need to enter a valid numeric for the number of ordinal levels";
                            MessageBox.Show(message);
                        }
                        else
                        {
                            str += "Ordinal";
                            str += "(with " + chkordlevels.Text + " levels)" + "|";
                        }
                    }
                    else
                    {
                        str += "Ordinal";
                        str += "|";
                    }
                }
                if (chkNominal.IsChecked.HasValue & chkNominal.IsChecked.Value)
                {
                    if (chkNomlevels.Text != null && chkNomlevels.Text != string.Empty)
                    {
                        isdouble = Double.TryParse(chkNomlevels.Text, out result);
                        if (!isdouble)
                        {
                            message = "You need to enter a valid numeric for the number of nominal levels";
                            MessageBox.Show(message);
                        }
                        else
                        {
                            str += "Nominal";
                            str += "(with " + chkNomlevels.Text + " levels)" + "|";
                        }
                    }
                    else
                    {
                        str += "Nominal";
                        str += "|";
                    }
               }
             
                if (chkScale.IsChecked.HasValue & chkScale.IsChecked.Value)
                    str += "Scale";

                str=str.Trim('|');
                return str;
            }
            set
            {
               // string[] strs = value.Split('|');
                string strs = value;
                if (strs.Contains("String"))
                    chkString.IsChecked = true;
                if (strs.Contains("Numeric"))
                    chkNumeric.IsChecked = true;
                if (strs.Contains("Date"))
                    chkDate.IsChecked = true;
                if (strs.Contains("Logical"))
                    chkLogical.IsChecked = true;
                if (strs.Contains("Ordinal"))
                    chkOrdinal.IsChecked = true;
                if (strs.Contains("Nominal"))
                    chkNominal.IsChecked = true;
                if (strs.Contains("Scale"))
                    chkScale.IsChecked = true;
            }
        }


        private void resetnomlevels(object sender, RoutedEventArgs e)
        {
            chkNomlevels.Text = string.Empty;
        }

        private void resetordlevels(object sender, RoutedEventArgs e)
        {
            chkordlevels.Text = string.Empty;
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
