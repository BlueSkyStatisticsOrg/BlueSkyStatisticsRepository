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

namespace BlueSky.Commands.Tools.Package.Dialogs
{
    /// <summary>
    /// Interaction logic for InstallRequiredPackagesFromZip.xaml
    /// </summary>
    public partial class InstallRequiredPackagesFromZip : Window
    {
        public InstallRequiredPackagesFromZip()
        {
            InitializeComponent();
        }

        string pathselected = string.Empty;
        public string SelectedZipPath
        {
            get { return pathselected; }
        }

        private void browse_button_Click(object sender, RoutedEventArgs e)
        {
            var browseDlg = new System.Windows.Forms.FolderBrowserDialog();
            //browseDlg.RootFolder =  Environment.CurrentDirectory as Environment.SpecialFolder;
            string executionPath = Environment.CurrentDirectory;
            string zipFolderPath = "R Packages\\R 3.2.1 packages";
            string fullpath = System.IO.Path.Combine(executionPath, zipFolderPath);
            browseDlg.SelectedPath = System.IO.Directory.Exists(fullpath) ? fullpath : executionPath;
            System.Windows.Forms.DialogResult result = browseDlg.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                pathselected = browseDlg.SelectedPath;
            }
            pathTextBox.Text = pathselected;
        }

        private void ok_button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void cancel_button_Click(object sender, RoutedEventArgs e)
        {
            pathselected = string.Empty;
            this.Close();
        }
    }
}
