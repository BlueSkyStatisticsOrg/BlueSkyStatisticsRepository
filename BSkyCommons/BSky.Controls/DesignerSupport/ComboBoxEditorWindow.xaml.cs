using System.Windows;
using System.Linq;
using System.Collections.ObjectModel;

namespace BSky.Controls.DesignerSupport
{
    /// <summary>
    /// Interaction logic for ListBoxEditorControl.xaml
    /// </summary>
    public partial class ComboBoxEditorWindow : Window
    {
        public ComboBoxEditorWindow()
        {
            InitializeComponent();
        }


        public ObservableCollection<ComboBoxEntry> ComboBoxVals { get; set; }

        private ComboBoxValueCollection _comboboxentries;

        public ComboBoxValueCollection ComboBoxEntries
        {
            get
            {
                ComboBoxValueCollection clcton = new ComboBoxValueCollection();
                clcton.AddRange(ComboBoxVals.ToList());
                return clcton;
            }
            set
            {
                _comboboxentries = value;
                ComboBoxVals = new ObservableCollection<ComboBoxEntry>(_comboboxentries);
                gridforComboBox.ItemsSource = ComboBoxVals;
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

