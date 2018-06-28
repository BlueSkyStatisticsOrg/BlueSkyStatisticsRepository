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
    /// Interaction logic for ChangeImageSizeDialog.xaml
    /// </summary>
    public partial class ChangeImageSizeDialog : Window
    {
        public ChangeImageSizeDialog()
        {
            InitializeComponent();
        }

        public string ImgHeight 
        {
            get { return imgheighttxt.Text;}
            set { imgheighttxt.Text=value;}
        }
        public string ImgWidth
        {
            get { return imgwidthtxt.Text;}
            set { imgwidthtxt.Text = value;}
        }

        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            ImgWidth = string.Empty;
            ImgHeight = string.Empty;
            this.Close();
        }

        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
