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
    /// Interaction logic for RDataWarningMessageBox.xaml
    /// </summary>
    public partial class RDataWarningMessageBox : Window
    {
        public RDataWarningMessageBox()
        {
            InitializeComponent();
        }

        public string Msg
        {
            set
            {
                MsgText.Text = value;
            }
        }

        public string AdvMsg
        {
            set
            {
                MsgTextAdv.Text = value;
            }
        }

        public string ButtonClicked { get; set; }
        public bool NotShowCheck { get; set; }

        private void okbutton_Click(object sender, RoutedEventArgs e)
        {
            this.Close(); //this should be the first to execute because this calls Window_Closing() which makes
            // ButtonClicked = "Cancel", even though it is "OK" here. So we set "OK" below after this.Close() above.
            ButtonClicked = "OK";
            NotShowCheck = notshowcheckbox.IsChecked == true ? true : false;
            //this.Close();
        }

        private void cancelbutton_Click(object sender, RoutedEventArgs e)
        {
            ButtonClicked = "Cancel";
            NotShowCheck = notshowcheckbox.IsChecked == true ? true : false;
            this.Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            ButtonClicked = "Cancel";
            NotShowCheck = notshowcheckbox.IsChecked == true ? true : false;
        }
    }
}