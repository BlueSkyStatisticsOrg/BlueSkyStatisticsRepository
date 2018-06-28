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
using DevExpress.Xpf.Core;

namespace AnalyticsUnlimited.Client.Interfaces.Commands
{
    /// <summary>
    /// Interaction logic for BaseOptionWindow.xaml
    /// </summary>
    public partial class BaseOptionWindow : DXWindow
    {
        public BaseOptionWindow()
        {
            InitializeComponent();
        }
        public FrameworkElement Template
        {
            get
            {
                if (Host.Children.Count > 0)
                    return Host.Children[0] as FrameworkElement;
                else
                    return null;
            }
            set
            {
                if (value.Width != double.NaN)  
                {
                    this.Host.Width = value.Width + 10;
                    this.Width = value.Width + 30;
                }
                if (value.Height != double.NaN)
                {
                    this.Host.Height = value.Height + 25;
                    this.Height = value.Height + 85;
                }
                Host.Children.Add(value);
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
    }
}
