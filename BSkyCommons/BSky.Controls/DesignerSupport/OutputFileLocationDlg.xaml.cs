using System.Windows;

namespace BSky.Interfaces.Commands
{
    /// <summary>
    /// Interaction logic for BaseOptionWindow.xaml
    /// </summary>
    public partial class OutputFileLocationDlg : Window
    {
        public OutputFileLocationDlg()
        {
            InitializeComponent();
        }

        public string FileLocation
        {
            get
            {
                return txtCommand.Text;
            }
            set
            {
                txtCommand.Text = value;
            }
        }
   
        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog dialog = new System.Windows.Forms.OpenFileDialog();
            dialog.Filter = "Xml Document (*.xml)|*.xml";
            System.Windows.Forms.DialogResult result = dialog.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.OK)
            {
                txtCommand.Text = dialog.FileName;
            }
        }
    }
}
