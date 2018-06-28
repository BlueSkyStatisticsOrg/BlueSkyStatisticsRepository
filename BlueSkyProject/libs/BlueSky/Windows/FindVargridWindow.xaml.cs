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
using BSky.Statistics.Common;
using BSky.Lifetime;

namespace BlueSky.Windows
{
    /// <summary>
    /// Interaction logic for FindVargridWindow.xaml
    /// </summary>
    public partial class FindVargridWindow : Window
    {
        IUnityContainer container = null;
        IUIController controller = null;
        DataPanel dp = null;
        DataSource ds = null;
        Window1 appWin = null;

        public FindVargridWindow()
        {
            InitializeComponent();
            container = LifetimeService.Instance.Container;
            controller = container.Resolve<IUIController>();
            ds = controller.GetActiveDocument();
            TabItem ti = controller.GetTabItem(ds);
            dp = ti.Content as DataPanel;
            //dp.datavartabs.SelectedIndex = 1;//switch to DATA tab
            appWin = LifetimeService.Instance.Container.Resolve<Window1>();
            this.Owner = appWin;
            searchtext.Focus();
        }


        public FindVargridWindow(Window1 appwin)
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
            //dp.datavartabs.SelectedIndex = 1;//switch to DATA tab
            appWin = appwin;
            this.Owner = appWin;
            searchtext.Focus();
        }

        //12Sep2016 For NULL dataset we can disable the textfield and the Next button
        private void DisableDialog()
        {
            searchtext.IsEnabled = false;
            findnextbutton.IsEnabled = false;
        }

        private void findnextbutton_Click(object sender, RoutedEventArgs e)
        {
            findnextbutton.IsEnabled = false;
            if (searchModified) //if text was changed (search modified) then run find and make R call
            {
                string findtext = searchtext.Text;
                bool matchcase = (matchcaseChkbox.IsChecked == true) ? true : false;
                dp.FindColNameText(findtext, matchcase);

                searchModified = false;//now until someone types new string or modifies, we will not make R calls. We will do NEXT.

                //Also very first time try to jump to the first match.
                dp.FindNextColMatch();
            }
            else // No need to make search again. Rather just loop through the results unless the search is modified.
            {
                dp.FindNextColMatch();
            }
            findnextbutton.IsEnabled = true;
        }


        #region Search modifying events
        //These events will confirm that the search is modified and 
        //we should discard the old search result and get new results to work on
        bool searchModified = false;
        private void searchtext_TextChanged(object sender, TextChangedEventArgs e)
        {
            searchModified = true;
        }

        private void matchcaseChkbox_Changed(object sender, RoutedEventArgs e)
        {
            searchModified = true;
        }
        #endregion

        #region Close Find Window
        //Close : closes this window
        private void closebutton_Click(object sender, RoutedEventArgs e)
        {
            //dp.SwitchSelectionMode("row");//single row selection mode is set for the vargrid. It was default.
            dp.MatchVarIndex = 0;//resetting back to zero so that next time it should not start with the last value
            this.Close();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            //dp.SwitchSelectionMode("row");//single row selection mode is set for the vargrid. It was default.
            dp.MatchVarIndex = 0;//resetting back to zero so that next time it should not start with the last value
            if (appWin == null) return;
            appWin.CloseFindVargrid();
        }
        #endregion
    }
}
