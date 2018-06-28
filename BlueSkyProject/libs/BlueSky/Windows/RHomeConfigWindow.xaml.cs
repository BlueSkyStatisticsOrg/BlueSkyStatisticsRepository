using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace BlueSky.Windows
{
    /// <summary>
    /// Interaction logic for RHomeConfigWindow.xaml
    /// </summary>
    public partial class RHomeConfigWindow : Window
    {
        public RHomeConfigWindow()
        {
            InitializeComponent();
        }

        private void browseBtn_Click(object sender, RoutedEventArgs e)
        {
            //Browse functionality.
            System.Windows.Forms.FolderBrowserDialog folderBrowseDialog = new System.Windows.Forms.FolderBrowserDialog();
            folderBrowseDialog.SelectedPath = RHomeText.Text != null ? RHomeText.Text : string.Empty;
            System.Windows.Forms.DialogResult result = folderBrowseDialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                if(Directory.Exists(folderBrowseDialog.SelectedPath) || 
                    folderBrowseDialog.SelectedPath.Trim().Length == 0)// R path or blank 
                {
                    //set R Home Dir
                    string unixpath = folderBrowseDialog.SelectedPath.Replace('\\', '/');
                    RHomeText.Text= unixpath;//set Unix style path
                }
            }
        }

        private void okBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void cancelBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
