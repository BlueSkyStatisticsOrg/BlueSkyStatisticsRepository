using System.Collections.Generic;
using System.Windows;

namespace BlueSky.Commands.Tools.Package.Dialogs
{
    /// <summary>
    /// Interaction logic for SelectPacakgesWindow.xaml
    /// </summary>
    public partial class SelectPackagesWindow : Window
    {
        public SelectPackagesWindow(string[] strarr)
        {
            InitializeComponent();
            _listItems = strarr;
            loadListBox();
        }

        public string header { set { title.Text = value; } }

        string[] _listItems;
        public string[] ListItems 
        {
            get { return _listItems; }
            set { _listItems = value; } 
        }

        IList<string> _selectedItems;
        public IList<string> SelectedItems 
        {
            get { return _selectedItems; } 
            //set; 
        }

        public void loadListBox()
        {
            foreach(string s in _listItems)
                pkgListbox.Items.Add(s);
        }

        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            _selectedItems = new List<string>();
            foreach (var item in pkgListbox.SelectedItems)
            {
                _selectedItems.Add(item.ToString());
            }

            this.Close();
        }

        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {

            this.Close();
        }
    }
}
