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
    /// Interaction logic for SelectRdataDataframesWindow.xaml
    /// </summary>
    public partial class SelectRdataDataframesWindow : Window
    {
        public SelectRdataDataframesWindow()
        {
            InitializeComponent();
        }

        string[] _selDFlist;
        public string[] SelectedDFList
        {
            get { return _selDFlist; }
            set { _selDFlist = value; }
        }

        public string DlgResult
        {
            get;
            set;
        }
        public void LoadListbox(string[] dflist)
        {
            foreach (string dfname in dflist)
            {
                DFListbox.Items.Add(dfname);
            }
        }

        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            DlgResult = "Ok";
            int count = DFListbox.SelectedItems.Count;
            string[] selected = new string[count];
            int i = 0;
            foreach (string s in DFListbox.SelectedItems)
            {
                selected[i] = s;
                i++;
            }
            SelectedDFList = selected;
            this.Close();
        }

        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            DlgResult = "Cancel";
            this.Close();
        }
    }
}
