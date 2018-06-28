using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Collections.ObjectModel;
using BSky.Statistics.Common;

namespace BlueSky.UserControls
{
    /// <summary>
    /// Interaction logic for OptionSelection.xaml
    /// </summary>
    public partial class OptionSelection : Window
    {
        public OptionSelection()
        {
            InitializeComponent();
            //devx dXTabControl1.View.HeaderLocation = HeaderLocation.Right;
        }
        private ObservableCollection<DataSourceVariable> variablesList;
        
        public List<DataSourceVariable> VariablesList
        {
            get { return variablesList.ToList(); }
            set
            {
                variablesList = new ObservableCollection<DataSourceVariable>(value);
                lbOne.Items.Clear();
                lbOne.ItemsSource = variablesList;
            }
        }

        public UIElement Options
        {
            get
            {
                return this.OptionPanel.Children[0] as UIElement;
            }
            set
            {
                this.OptionPanel.Children.Clear();
                this.OptionPanel.Children.Add(value);
            }

        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
