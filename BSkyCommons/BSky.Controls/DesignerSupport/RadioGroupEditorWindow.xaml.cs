using System.Windows;
using System.Linq;
using System.Collections.ObjectModel;

namespace BSky.Controls
{
    public partial class RadioGroupEditorWindow : Window
    {
        public RadioGroupEditorWindow()
        {
            InitializeComponent();           
        }

        public class test
        {
            public string Name { get; set; }
            public string Text { get; set; }
            public bool IsSelected { get; set; }
        }
        public ObservableCollection<BSkyRadioButton> RadioCollection { get; set; }

        private BSkyRadioButtonCollection _radioButtons;

        public BSkyRadioButtonCollection RadioButtons
        {
            get
            {
                BSkyRadioButtonCollection clcton = new BSkyRadioButtonCollection();
                clcton.AddRange(RadioCollection.ToList());
                return clcton;
            }
            set
            {
                _radioButtons = value;
                RadioCollection = new ObservableCollection<BSkyRadioButton>(_radioButtons);
                dataGrid1.ItemsSource = RadioCollection;
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
