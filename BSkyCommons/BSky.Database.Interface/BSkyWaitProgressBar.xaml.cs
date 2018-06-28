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

namespace BSky.Database.Interface
{
    /// <summary>
    /// Interaction logic for BSkyWaitProgressBar.xaml
    /// </summary>
    public partial class BSkyWaitProgressBar : Window
    {
        public BSkyWaitProgressBar()
        {
            InitializeComponent();
            waitmsg.Text = BSky.GlobalResources.Properties.UICtrlResources.PlzWaitSQL;
        }


        public BSkyWaitProgressBar(string message)
        {
            InitializeComponent();
            waitmsg.Text = message;
        }

        //public string WaitMessage 
        //{
        //    set { waitmsg.Text = value; }//Connection in progress. Please wait...
        //}
    }
}
