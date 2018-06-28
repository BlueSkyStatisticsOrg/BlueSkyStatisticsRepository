using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace BSky.Controls.Dialogs
{
    /// <summary>
    /// Interaction logic for AddCategoryDialog.xaml
    /// </summary>
    public partial class AddCategoryDialog : Window
    {
        public AddCategoryDialog()
        {
            InitializeComponent();
            errmsg.Foreground = Brushes.Black;
            errmsg.Text = BSky.GlobalResources.Properties.UICtrlResources.AddCategoryErrMsg1;
            categoryname.Focus();
        }

        private string _categoryname;
        public string CategoryName
        {
            get { return _categoryname; }
            set { _categoryname = value; }
        }

        private void addCatOk_Click(object sender, RoutedEventArgs e)
        {
            if (categoryname.Text != null && categoryname.Text.Trim().Length > 0)
            {
                CategoryName = categoryname.Text.Trim();
            }
            else
            {
                //please provide category name
                errmsg.Text = BSky.GlobalResources.Properties.UICtrlResources.AddCategoryErrMsg2;
                return;
            }

            this.Close();
        }

        private void catCancel_Click(object sender, RoutedEventArgs e)
        {
            CategoryName = null; // canceling the operation
            this.Close();
        }

        private void categoryname_TextChanged(object sender, TextChangedEventArgs e)
        {
            errmsg.Text = string.Empty;
        }

        private void categoryname_GotFocus(object sender, RoutedEventArgs e)
        {
            errmsg.Text = BSky.GlobalResources.Properties.UICtrlResources.AddCategoryErrMsg1;
        }
    }
}
