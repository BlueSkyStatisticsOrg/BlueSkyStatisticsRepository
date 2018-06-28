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
    /// Interaction logic for SplashWindow.xaml
    /// </summary>
    public partial class SplashWindow : Window
    {
        public SplashWindow()
        {
            InitializeComponent();
        }

        public SplashWindow(string busymessage) // Sending custom message
        {
            InitializeComponent();
            //Binding b = new Binding(busymessage);
            //label1.SetBinding(Label.ContentProperty, b);
            label1.Content = busymessage;
            //System.Windows.Threading.DispatcherTimer dis = new System.Windows.Threading.DispatcherTimer();
            //dis.Interval = new TimeSpan(1000);
        }
    }
}
