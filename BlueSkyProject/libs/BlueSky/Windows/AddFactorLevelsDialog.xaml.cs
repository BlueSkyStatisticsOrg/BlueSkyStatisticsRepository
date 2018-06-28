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
    /// Interaction logic for AddFactorLevelsDialog.xaml
    /// </summary>
    public partial class AddFactorLevelsDialog : Window
    {
        public AddFactorLevelsDialog()
        {
            InitializeComponent();
        }


        private List<string> factorLevels;

        public List<string> FactorLevels
        {
            get
            {
                return factorLevels;
            }
            set
            {
                factorLevels = value;
                FillLevelListBox();
            }
        }


        private void FillLevelListBox()
        {
            levelsListBox.ItemsSource = FactorLevels;
        }

        private bool isDuplicate(string newlvl)
        {
            return FactorLevels.Contains(newlvl);
        }

        private void addlvlButton_Click(object sender, RoutedEventArgs e)
        {
            string newlevel = faclvltxt.Text.Trim();
            if (newlevel == null || newlevel.Length < 1)//Blanks not allwed as factors
            {
                MessageBox.Show("Blank Spaces Not Allowed! ","Blank Level", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            else if (!isDuplicate(newlevel))
            {
                FactorLevels.Add(newlevel);
                levelsListBox.ItemsSource = null;
                levelsListBox.ItemsSource = FactorLevels;
                levelsListBox.SelectedItem = newlevel;
                levelsListBox.ScrollIntoView(newlevel);
                
                faclvltxt.Text = string.Empty;
            }
            else
            {
                MessageBox.Show("Duplicate Level Not Allowed! ", "Duplicate Level", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void removelvlbutton_Click(object sender, RoutedEventArgs e)
        {
            string selectedlevel = levelsListBox.SelectedItem.ToString();
            if (FactorLevels.Contains(selectedlevel))
            {
                FactorLevels.Remove(selectedlevel);
                levelsListBox.ItemsSource = null;
                levelsListBox.ItemsSource = FactorLevels;
            }
            else
            {
                MessageBox.Show("Cannot Remove Level! ", "Remove Level", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void okbutton_Click(object sender, RoutedEventArgs e)
        {
            FactorLevels = levelsListBox.ItemsSource as List<string>;
            if (FactorLevels.Count > 0)
            { }
            Close();
        }

        private void cancelbutton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }


    }
}
