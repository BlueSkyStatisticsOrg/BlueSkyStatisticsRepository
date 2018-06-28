using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace BlueSky.Windows
{
    /// <summary>
    /// Interaction logic for AddModelClassWindow.xaml
    /// </summary>
    public partial class AddModelClassWindow : Window
    {
        public AddModelClassWindow()
        {
            InitializeComponent();
            modelclasstxt.Focus();
        }

        public string ModelClass { get; set; }

        private void addbutton_Click(object sender, RoutedEventArgs e)
        {
            ModelClass = modelclasstxt.Text;
            this.Close();
        }

        private void cancelbutton_Click(object sender, RoutedEventArgs e)
        {
            ModelClass = string.Empty;
            this.Close();
        }
    }
}
