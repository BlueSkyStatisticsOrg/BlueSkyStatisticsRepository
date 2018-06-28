using System.Windows;

namespace BlueSky.Commands.Tools.Package.Dialogs
{
    /// <summary>
    /// Interaction logic for AskPackageNameWindow.xaml
    /// </summary>
    public partial class AskPackageNameWindow : Window
    {
        public AskPackageNameWindow()
        {
            InitializeComponent();
            pkgname.Focus();
        }

        //public string Title 
        //{
        //    //get { return _title; }
        //    set { title.Text = value; }
        //}

        string _packagename;
        public string PackageName 
        {
            get { return _packagename; }
            //set { _packagename = value; } 
        }

        private void okbutton_Click(object sender, RoutedEventArgs e)
        {
            if (pkgname.Text != null && pkgname.Text.Trim().Length > 0)
            {
                _packagename = pkgname.Text.Trim();
            }
            else
                _packagename = string.Empty;
            this.Close();
        }

        private void cancelbutton_Click(object sender, RoutedEventArgs e)
        {
            _packagename = string.Empty;
            this.Close();
        }


    }
}
