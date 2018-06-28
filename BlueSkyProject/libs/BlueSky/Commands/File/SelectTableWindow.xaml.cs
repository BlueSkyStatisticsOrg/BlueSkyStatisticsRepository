using System.Windows;

namespace BlueSky.Commands.File
{
    /// <summary>
    /// Interaction logic for SelectTableWindow.xaml
    /// </summary>
    public partial class SelectTableWindow : Window
    {
        public SelectTableWindow()
        {
            InitializeComponent();
        }

        string selectedTablename;
        public string SelectedTableName 
        {
            get { return selectedTablename; } 
        }

        public void FillList(string[] items)
        {
            string strnoquote=string.Empty;
            foreach (string s in items)
            {   
                strnoquote=s;
                //if (!s.EndsWith("$"))
                //    continue;
                if (!tablelist.Items.Contains(s.Replace("$", "")))
                {
                    //28Apr2015 remove single quotes around string s
                    if (s.StartsWith("'") && s.EndsWith("'"))// sinlge quotes present
                    {
                        strnoquote = s.Substring(1, s.Length - 2);
                    }
                    tablelist.Items.Add(strnoquote.Replace("$", ""));
                }
            }
            tablelist.SelectedIndex = 0;//first item is selected by default
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            selectedTablename = tablelist.SelectedItem as string;
            this.Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            selectedTablename = null;
            this.Close();
        }
    }
}
