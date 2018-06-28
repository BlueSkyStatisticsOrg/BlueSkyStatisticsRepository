using BSky.Lifetime.Interfaces;
using BSky.Lifetime.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace BlueSky.Dialogs
{
    /// <summary>
    /// Interaction logic for OpenCSVTXTOptionsWindow.xaml
    /// </summary>
    public partial class OpenCSVTXTOptionsWindow : Window
    {
        public OpenCSVTXTOptionsWindow()
        {
            InitializeComponent();
        }

        public IOpenDataFileOptions csvtxtOptions { get; set; }

        private void okbutton_Click(object sender, RoutedEventArgs e)
        {
            csvtxtOptions = new OpenDataFileOptions();
            csvtxtOptions.HasHeader = HeadersCheckbox.IsChecked==true ? true:false;
            csvtxtOptions.IsBasketData = BasketDataCheckbox.IsChecked == true ? true : false;
            csvtxtOptions.FieldSeparatorChar = GetSepChar();
            csvtxtOptions.DecimalPointChar = GetDeciChar();

            if(csvtxtOptions.FieldSeparatorChar != '\0') //close dialog if a non empty character is entered.
                this.Close();
        }


        private char GetSepChar()
        {
            if (commaRadio.IsChecked == true)
            {
                return ',';
            }
            else if (semicolonRadio.IsChecked == true)
            {
                return ';';
            }
            else if (tabRadio.IsChecked == true)
            {
                return '\t';
            }
            else if (spaceRadio.IsChecked == true)
            {
                return ' ';
            }
            else if (otherRadio.IsChecked == true)
            {
                if (string.IsNullOrEmpty(otherSepChar.Text))
                {
                    MessageBox.Show(this, "You need to specify a character if you chose 'other' option", "Other separator charcter not specified", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                    return '\0'; //null char
                }
                else
                {
                    return otherSepChar.Text.ToCharArray()[0];//first character is returned
                }
            }
            else
                return ','; //default
        }

        private char GetDeciChar()
        {
            if (decimalRadio.IsChecked == true)
            {
                return '.';
            }
            else if (comaRadio.IsChecked == true)
            {
                return ',';
            }
            return '.';//default
        }

        private void cancelbutton_Click(object sender, RoutedEventArgs e)
        {
            csvtxtOptions = null;
            this.Close();
        }
    }

    
}
