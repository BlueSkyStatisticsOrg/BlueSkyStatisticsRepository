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

namespace BlueSky.Windows
{
    /// <summary>
    /// Interaction logic for SyntaxFontSettingsWindow.xaml
    /// </summary>
    public partial class SyntaxFontSettingsWindow : Window
    {
        public SyntaxFontSettingsWindow()
        {
            InitializeComponent();
        }

        private double _fontsize;
        public double FontSize
        {
            get { return _fontsize; }
            set {
                _fontsize = value;
                fontslider.Value = value;
            }
        }
        private void Fontsizecancelbtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Fontsizeokbtn_Click(object sender, RoutedEventArgs e)
        {
            _fontsize = fontslider.Value;
            this.Close();
        }
    }
}
