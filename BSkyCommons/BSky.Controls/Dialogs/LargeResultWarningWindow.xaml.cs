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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BSky.Controls.Dialogs
{
    /// <summary>
    /// Interaction logic for LargeResultWarningWindow.xaml
    /// </summary>
    public partial class LargeResultWarningWindow : Window
    {
        public LargeResultWarningWindow()
        {
            InitializeComponent();
            msg2.Text = BSky.GlobalResources.Properties.UICtrlResources.LargeResultWarningSecMsg2;
        }
        public LargeResultWarningWindow(int allrowcount, int rowcount, int configcellcount)
        {
            InitializeComponent();

            string s1 = string.Format(BSky.GlobalResources.Properties.UICtrlResources.LargeResultWarningMsg1, configcellcount);
            msg1.Text = s1;
            msg1b.Text = BSky.GlobalResources.Properties.UICtrlResources.LargeResultWarningMsg1b;
            msg2.Text = string.Format( BSky.GlobalResources.Properties.UICtrlResources.LargeResultWarningMsg2, rowcount, allrowcount);
        }
        //private int _rowcount;
        //public int RowCount 
        //{
        //    get { return _rowcount; }
        //    set { _rowcount = value; } 
        //}
        private string _keypressed;
        public string KeyPressed 
        {
            get { return _keypressed; }
        }
        private void FullButton_Click(object sender, RoutedEventArgs e)
        {
            _keypressed = "Full";
            this.Close();
        }

        private void PartialButton_Click(object sender, RoutedEventArgs e)
        {
            _keypressed = "Part";
            this.Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            _keypressed = "Cancel";
            this.Close();
        }
    }
}
