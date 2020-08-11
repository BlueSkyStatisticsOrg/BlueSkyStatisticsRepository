using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using BSky.Statistics.Common;

namespace BlueSky.Windows
{
    /// <summary>
    /// Interaction logic for ValueLabelsSubDialog.xaml
    /// </summary>
    /// 
        
    public partial class ValueLabelsSubDialog : Window
    {
        public bool OKclicked;
        
        private List<FactorMap> _factormap;
       
        public List<FactorMap> factormap
        {
            get
            {
                return _factormap;
            }
            set
            {
                _factormap = value;
            }
        }

        public ValueLabelsSubDialog()
        {
            InitializeComponent();
            createList();
        }

        public ValueLabelsSubDialog( List<FactorMap> flist, string firstHeader, string secondHeader, string thirdHeader)
        {
            InitializeComponent();
            //if(firstHeader.Equals("Level Names")) // reverse flist
            //{

            //}

            factormap = flist;
            listHeader1.Content = firstHeader;
            listHeader2.Content = secondHeader;
            listHeader3.Content = thirdHeader;
            createList();
        }
       
        public void createList()
        {
            Listbox.ItemsSource = factormap; 
            Listbox.Width = 500;
            KeyboardNavigation.SetTabNavigation(Listbox, KeyboardNavigationMode.Cycle);

            //Scroll to the bottom
            Listbox.SelectedIndex = Listbox.Items.Count - 1;
            Listbox.ScrollIntoView(Listbox.SelectedItem);
        }

        private void ok_button_Click(object sender, RoutedEventArgs e)
        {
            if (!isTextChanged) // if text is not changed
            {
                ValueLabelsSubDialog.GetWindow(this).Close();
            }
            else
            {
                if (factormap != null && factormap.Count < 1)//if list is empty. just close the dialog
                {
                    ValueLabelsSubDialog.GetWindow(this).Close();
                    return;
                }
                bool isEmpty = false;
                bool isDuplicateLvlName = false;
                ////bool isDuplicateLvlNum = false;
                int i = 0;
                int len = factormap.Count;

                #region if user enters more than one levels separated by comma
                //The last item in the list is the one that has the new level(s)
                FactorMap lastitem = factormap.Last();
                if (lastitem.textbox.Contains(','))
                {
                    char[] sepa = { ',' };
                    string[] newlevels = lastitem.textbox.Split(sepa);
                    bool isfirstItem = true;

                    //add new levels but remember to modify the 'lastitem' that was having all the new levels. 
                    //First new level can be fed to 'lasitem'. Second onwards create new item per new level.
                    for (int newi = 0; newi < newlevels.Length; newi++)
                    {
                        //ignore blanks
                        if (newlevels[newi].Trim() == null || newlevels[newi].Trim().Length < 1)
                            continue;

                        //ignore duplicates
                        {
                            isDuplicateLvlName = false;
                            for (int j = 0; j < len; j++)
                            {
                                //duplicate level names
                                if ((newlevels[newi].Trim().Length > 0) && (newlevels[newi].Trim().Equals(factormap.ElementAt(j).textbox.Trim())))//blank fields should not be checked
                                {
                                    isDuplicateLvlName = true;
                                    break;
                                }
                            }
                            if (isDuplicateLvlName)//ignore duplicates. When multiple new levels are entered by user with duplicates
                            {
                                continue;
                            }
                        }

                        if (isfirstItem)//first item goes to 'lastitem' object
                        {
                            lastitem.textbox = newlevels[newi].Trim();
                            isfirstItem = false;
                        }
                        else
                        {
                            FactorMap newitemNewlevel = new FactorMap();
                            newitemNewlevel.textbox = newlevels[newi].Trim();
                            newitemNewlevel.labels = "";
                            factormap.Add(newitemNewlevel);
                        }
                    }
                }
                #endregion
                foreach (FactorMap m in factormap)
                {
                    string s = m.labels + ":" + m.textbox;
                    //MessageBox.Show(s);

                    ////checking duplicates ////
                    i++;
                    for (int j = i; j < len; j++)
                    {
                        //duplicate level names
                        if ((m.textbox.Trim().Length > 0) && (m.textbox == factormap.ElementAt(j).textbox.Trim()))//blank fields should not be checked
                        {
                            isDuplicateLvlName = true;
                            break;
                        }

                        //////Duplicate level number
                        ////if ((m.numlevel.ToString().Trim().Length > 0) && (m.numlevel.ToString() == factormap.ElementAt(j).textbox.Trim()))//blank fields should not be checked
                        ////{
                        ////    isDuplicateLvlNum = true;
                        ////    break;
                        ////}
                    }

                    ////checking empty for all except the blank field(which is for new level)///
                    if (m.textbox.Trim().Length == 0 && 
                        !m.labels.Trim().Equals(BSky.GlobalResources.Properties.UICtrlResources.AddFactorLevelMsg.Trim()) )
                    {
                        isEmpty = true;
                    }

                }
                OKclicked = true;
                if (isDuplicateLvlName ) //// || isDuplicateLvlNum)
                {
                    ////MessageBox.Show("Duplicate Level Names(or level numbers) are not allowed.", "Error! Duplicate not allowed.", MessageBoxButton.OK, MessageBoxImage.Error);
                    MessageBox.Show("Duplicate Level Names are not allowed.", "Error! Duplicate not allowed.", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                if (isEmpty)
                {
                    if (MessageBox.Show("Empty values not allowed.", "Warning.", MessageBoxButton.OK, MessageBoxImage.Warning) == MessageBoxResult.OK)
                    {
                        //restore cells with blank values
                        foreach (FactorMap m in factormap)
                        {
                            if (m.textbox == null || m.textbox.Trim().Length == 0)
                            {
                                if(!m.labels.Contains("Enter new level(s) separated by comma"))//bug fixed
                                    m.textbox = m.labels;
                            }
                        }
                        Listbox.ItemsSource = null;
                        Listbox.ItemsSource = factormap;
                        return;
                    }
                    else if (MessageBox.Show("Empty factors will be converted to NAs.", "Warning.", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                    {
                        ValueLabelsSubDialog.GetWindow(this).Close();
                    }
                    else
                    {
                        //try to put back original value that was replaced with space. 
                        //If there are multiple levels(fields) made as blank, it will be trickier to get all of them back in UI dialog.
                        foreach (FactorMap m in factormap)
                        {
                            if (m.textbox == null || m.textbox.Trim().Length == 0)
                            {
                                m.textbox = m.labels;
                            }
                        }
                        Listbox.ItemsSource = null;
                        Listbox.ItemsSource = factormap;
                    }
                }
                else
                    ValueLabelsSubDialog.GetWindow(this).Close();
            }
        }

        private void cancel_button_Click(object sender, RoutedEventArgs e)
        {
            ValueLabelsSubDialog.GetWindow(this).Close();
        }

        bool isTextChanged = false;
        private void TextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            isTextChanged = true;
        }



    }

}
