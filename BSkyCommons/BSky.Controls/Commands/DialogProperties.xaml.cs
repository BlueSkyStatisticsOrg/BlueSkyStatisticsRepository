using System.Windows;

namespace BSky.Controls.Commands
{
    /// <summary>
    /// Interaction logic for DialogProperties.xaml
    /// </summary>
    public partial class DialogProperties : Window
    {
        public DialogProperties()
        {
            InitializeComponent();
        }

        public double Width 
        {
            get
            {
                double width = 0;
                if (double.TryParse(txtWidth.Text,out width))
                {
                    return width;
                }
                return double.NaN;
            }
            set
            {
                txtWidth.Text = value.ToString();
            }
        }

        public string Title
        {
            get
            {
                return txtTitle.Text;
            }
            set
            {
                txtTitle.Text = value;
            }
        }

        public double Height
        {
            get
            {
                double height = 0;
                if (double.TryParse(txtHeight.Text, out height))
                {
                    return height;
                }
                return double.NaN;
            }
            set
            {
                txtHeight.Text = value.ToString();
            }        
        }


        private void button1_Click(object sender, RoutedEventArgs e)
        {
            double width, height;
            if (double.TryParse(txtWidth.Text, out width) && double.TryParse(txtHeight.Text, out height))
            {
                this.DialogResult = true;
            }
            else
            {
                MessageBox.Show("Enter Valid values for width and height");
                this.DialogResult = false;
            }
        }
    }
}
