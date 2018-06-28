using System.Windows;
using System.Windows.Media;

namespace BSky.Controls
{
    /// <summary>
    /// Interaction logic for CellCustomization.xaml
    /// </summary>
    public partial class TableCustomization : Window
    {
        public TableCustomization()
        {
            InitializeComponent();
        }

        public bool AlternateRowColorEnabled { get; set; }

        public Color AlternateRowBackgroundColor{ get; set; }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            this.AlternateRowColorEnabled = AlternateEnabled.IsChecked.HasValue ? AlternateEnabled.IsChecked.Value : false;
            AlternateRowBackgroundColor = this.BackgroundColor.Color;
            this.Close();
                 
        }     
    } 
    
}
