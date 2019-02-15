using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace BlueSky.Commands.File
{
    /// <summary>
    /// Interaction logic for OpenSPSSoptionsWindow.xaml
    /// </summary>
    public partial class OpenSPSSoptionsWindow : Window
    {
        public OpenSPSSoptionsWindow()
        {
            InitializeComponent();
        }

        public bool TrimSpaces { get; set; }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
            if (RmvSpsChkbox.IsChecked ?? false) //using null coalescing operator
            {
                TrimSpaces = true;
            }
            else
                TrimSpaces = false;
            //MessageBox.Show("Trim trailing spaces : " + TrimSpaces.ToString());

        }
    }
}
