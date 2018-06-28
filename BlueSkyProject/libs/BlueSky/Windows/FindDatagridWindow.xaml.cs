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
using Microsoft.Practices.Unity;
using BSky.Interfaces.Interfaces;
using BSky.Lifetime;
using BSky.Statistics.Common;

namespace BlueSky.Windows
{
    /// <summary>
    /// Interaction logic for FindDatagridWindow.xaml
    /// </summary>
    public partial class FindDatagridWindow : Window
    {
        IUnityContainer container = null;
        IUIController controller = null;
        DataPanel dp = null;
        DataSource ds = null;
        Window1 appWin = null;

        public FindDatagridWindow()
        {
            InitializeComponent();

            container = LifetimeService.Instance.Container;
            controller = container.Resolve<IUIController>();
            ds = controller.GetActiveDocument();
            TabItem ti = controller.GetTabItem(ds);
            dp = ti.Content as DataPanel;

            appWin = LifetimeService.Instance.Container.Resolve<Window1>();
            this.Owner = appWin;
            //fill listbox with colnames of currently active dataset
            FillColnames();
            searchtext.Focus();
        }

        public FindDatagridWindow(Window1 appwin)
        {
            InitializeComponent();

            container = LifetimeService.Instance.Container;
            controller = container.Resolve<IUIController>();
            ds = controller.GetActiveDocument();
            if (ds == null || ds.Variables == null || ds.Variables.Count < 1)//12Sep2016 For NULL dataset we can disable the textfield and the Next button
            {
                DisableDialog();
            }

            TabItem ti = controller.GetTabItem(ds);
            dp = ti.Content as DataPanel;
            dp.datavartabs.SelectedIndex = 0;//switch to DATA tab
            appWin = appwin;//LifetimeService.Instance.Container.Resolve<Window1>();
            this.Owner = appWin;
            //fill listbox with colnames of currently active dataset
            FillColnames();
            searchtext.Focus();
        }

        //12Sep2016 For NULL dataset we can disable the textfield and the Next button
        private void FillColnames()
        {
            string datasetname = ds.Name;
            List<DataSourceVariable> colnames = ds.Variables;
            selectedColslistbox.ItemsSource = colnames;
        }

        private void DisableDialog()
        {
            searchtext.IsEnabled = false;
            gridfindbutton.IsEnabled = false;
        }
        //Find : finds all occurrences by making R call once. And then selects the grid cell for the first search result
        private void gridfindbutton_Click(object sender, RoutedEventArgs e)
        {
            BSkyMouseBusyHandler.ShowMouseBusy();
            gridfindbutton.IsEnabled = false;
            if (searchModified) //if text was changed (search modified) then run find and make R call
            {
                string findtext = searchtext.Text;
                bool matchcase = (matchcasecheckbox.IsChecked == true) ? true : false;
                int selectedcolcount = selectedColslistbox.SelectedItems.Count;
                //List<DataSourceVariable> selcolnames = selectedColslistbox.SelectedItems as List<DataSourceVariable>;
                string[] selcolumnnames = new string[selectedcolcount];
                int i = 0;
                foreach (DataSourceVariable dsv in selectedColslistbox.SelectedItems)
                {
                    selcolumnnames[i] = dsv.RName;
                    i++;
                }
                dp.FindGridText(findtext, selcolumnnames, matchcase);

                searchModified = false;//now until someone types new string or modifies, we will not make R calls. We will do NEXT.

                //Also very first time try to jump to the first match.
                dp.FindNextGridText();
            }
            else // No need to make R call. Rather just loop through the results unless the search is modified.
            {
                dp.FindNextGridText();
            }
            gridfindbutton.IsEnabled = true;
            BSkyMouseBusyHandler.HideMouseBusy();
        }


        //This button may not be required any more. as Find will also do the job of FindNext.
        //Rather we will rename "Find" to "Find Next"
        //FindNext : iterates through all search results. Does not make call to R
        private void gridfindnextbutton_Click(object sender, RoutedEventArgs e)
        {
            dp.FindNextGridText();

            //Bring window in front
            //appwin.Activate();
        }


        #region Search modifying events
        //These events will confirm that the search is modified and 
        //we should discard the old search result and get new results to work on
        bool searchModified = false;
        private void searchtext_TextChanged(object sender, TextChangedEventArgs e)
        {
            searchModified = true;
        }

        private void selectedColslistbox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            searchModified = true;
        }

        private void matchcasecheckbox_Changed(object sender, RoutedEventArgs e)
        {
            searchModified = true;
        }
        #endregion

        #region Close Window
        //Close : closes this window
        private void gridfindclosebutton_Click(object sender, RoutedEventArgs e)
        {
            dp.SwitchSelectionMode("row");//single row selection mode is set for the datagrid. It was default.
            dp.MatchIndex = 0;//resetting back to zero so that next time it should not start with the last value
            this.Close();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            dp.SwitchSelectionMode("row");//single row selection mode is set for the datagrid. It was default.
            dp.MatchIndex = 0;//resetting back to zero so that next time it should not start with the last value
            if (appWin == null) return;
            appWin.CloseFindDatagrid();
        }

        #endregion


    }
}
