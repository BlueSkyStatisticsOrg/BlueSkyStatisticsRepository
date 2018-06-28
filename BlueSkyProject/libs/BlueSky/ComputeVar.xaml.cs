using System.Windows;

using BSky.Statistics.Common;
using BSky.Lifetime;
using System.Collections.ObjectModel;
using BSky.Interfaces.Interfaces;

namespace BlueSky
{
    /// <summary>
    /// Interaction logic for ComputeVar.xaml
    /// </summary>
    public partial class ComputeVar : Window
    {
        IUIController UIController;
        DataSource ds = null;
        bool canExecute = true;

        public ComputeVar()
        {
            InitializeComponent();
            fillVars();
        }

        public bool CanExecute(object parameter)
        {
            return canExecute;
        }

        public ObservableCollection<DataSourceVariable> Variables
        {
            get;
            set;
        }
      
        void fillVars()
        {
            UIController = LifetimeService.Instance.Container.Resolve<IUIController>();
            ds = UIController.GetActiveDocument();
            if (ds == null)
            {
                canExecute = false;
                return;
            }
            Variables = new ObservableCollection<DataSourceVariable>(ds.Variables);
            source.ItemsSource = ds.Variables;
            //source.Items.Add("AA");
            //source.Items.Add("BB");
            //source.Items.Add("CC");
            //source.Items.Add("DD");
            //source.Items.Add("EE");
            //source.Items.Add("FF");
            //source.Items.Add("GG");
            //source.Items.Add("HH");
            //source.Items.Add("II");
            //source.Items.Add("JJ");
            //source.Items.Add("KK");
            //source.Items.Add("LL");
            //source.Items.Add("MM");
            //source.Items.Add("NN");
            //source.Items.Add("OO");
            ////////////// Function catagory /////////
            funcat.Items.Add("All");
            funcat.Items.Add("Arithmetic");
            funcat.Items.Add("CDF & Noncentral CDF");
            funcat.Items.Add("Conversion");
            funcat.Items.Add("Current Date/Time");
            funcat.Items.Add("Date Arithmetic");
            funcat.Items.Add("Date Creation");
        }

    }
}
