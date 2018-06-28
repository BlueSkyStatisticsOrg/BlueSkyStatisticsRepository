using System.Windows;
using System.Linq;
using System.Collections.ObjectModel;

namespace BSky.Controls.DesignerSupport
{
    /// <summary>
    /// Interaction logic for ListBoxEditorControl.xaml
    /// </summary>
    public partial class ListBoxEditorWindow : Window
    {
        public ListBoxEditorWindow()
        {
            InitializeComponent();
        }


        public ObservableCollection<ListBoxEntry> ListBoxVals { get; set;}

        private ListBoxValueCollection _listboxentries;

        public ListBoxValueCollection ListBoxEntries
        {
            get
            {
                ListBoxValueCollection clcton = new ListBoxValueCollection();
                clcton.AddRange(ListBoxVals.ToList());
                return clcton;
            }
            set
            {
                _listboxentries = value;
                ListBoxVals = new ObservableCollection<ListBoxEntry>(_listboxentries);
                gridforListBox.ItemsSource = ListBoxVals;
            }
        }


        private void button1_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        //05/18/2013
        //Added by Aaron
        private void button2_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
    }


}
