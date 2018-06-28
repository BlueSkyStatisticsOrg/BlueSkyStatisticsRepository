using System.Windows;
using System.Linq;
using System.Collections.ObjectModel;

namespace BSky.Controls.DesignerSupport
{
    /// <summary>
    /// Interaction logic for DependentListBoxWindow.xaml
    /// </summary>
    public partial class DependentListBoxEditorWindow :Window
    {
      //  public List <string> OrderStatus;

        
        //public OrderStatus myEnum;
        public DependentListBoxEditorWindow()
        {
            InitializeComponent();
        //    OrderStatus=new List<string>();
         //   OrderStatus.Add("one");
            
        }
        public ObservableCollection<MasterSlaveEntry> ListBoxVals { get; set; }

        private MasterSlaveValueCollection _masterslaveentries;

        public MasterSlaveValueCollection MasterSlaveEntries
        {
            get
            {
                MasterSlaveValueCollection clcton = new MasterSlaveValueCollection();
                clcton.AddRange(ListBoxVals.ToList());
                return clcton;
            }
            set
            {
                _masterslaveentries = value;
                ListBoxVals = new ObservableCollection<MasterSlaveEntry>(_masterslaveentries);
                gridforListBox.ItemsSource = ListBoxVals;
                xyz.ItemsSource = MasterSlaveEditor.slaveList;
                pqr.ItemsSource = MasterSlaveEditor.masterList;
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
