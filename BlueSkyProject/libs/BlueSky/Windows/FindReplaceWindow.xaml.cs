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
using ScintillaNET;

namespace BlueSky.Windows
{
    /// <summary>
    /// Interaction logic for FindReplaceWindow.xaml
    /// </summary>
    public partial class FindReplaceWindow : Window
    {
        OutputWindow _ow;

        public FindReplaceWindow()
        {
            InitializeComponent();
            _findReplaceSearchFlags = SearchFlags.None;
        }



        public FindReplaceWindow(OutputWindow ow, bool InSelecteionChecked=false)
        {
            InitializeComponent();
            _ow = ow;
            findtxt.Focus();
            _findReplaceSearchFlags = SearchFlags.None;

            
            //if some text is selected just before launching Find/Replace then we CHECK the InSelectionCheckbox
            // else we keep it UNCHECKED.
            // If user already launched Find-Replace and then trying to selected some text then it wont work. For
            //that we need to do binding and write some little complex code.
            InSelectionCheck.IsChecked = InSelecteionChecked;
        }

        //based on what option user has checked, flag will be set
        private SearchFlags _findReplaceSearchFlags;
        private void SetFindReplaceSearchFlag()
        {
            //start with none and attach flags those are ON
            _findReplaceSearchFlags = SearchFlags.None;

            if (MatchCaseCheckbox.IsChecked == true)
                _findReplaceSearchFlags |= SearchFlags.MatchCase;
            if (MatchWordCheckbox.IsChecked == true)
                _findReplaceSearchFlags |= SearchFlags.WholeWord;
        }

        private void findnextbutton_Click(object sender, RoutedEventArgs e)
        {
            SetFindReplaceSearchFlag();

            if (_ow == null) return;

            string findtext = findtxt.Text != null ? findtxt.Text : string.Empty;
            _ow.FindText(findtext, _findReplaceSearchFlags);
        }

        private void replacebutton_Click(object sender, RoutedEventArgs e)
        {
            SetFindReplaceSearchFlag();

            if (_ow == null) return;
            bool foundanother = true;
            string replacetext = replacetxt.Text != null ? replacetxt.Text : string.Empty; ;

            string findtext = findtxt.Text != null ? findtxt.Text : string.Empty;

            foundanother = _ow.ReplaceWith(findtext, replacetext, _findReplaceSearchFlags);
            //if (!foundanother)
            //{
            //    MessageBox.Show(this,"No more match found to replace.");
            //}
        }

        private void replaceallbutton_Click(object sender, RoutedEventArgs e)
        {
            /////Common code
                SetFindReplaceSearchFlag();
                if (_ow == null) return;
                string replacetext = replacetxt.Text != null ? replacetxt.Text : string.Empty; ;
                string findtext = findtxt.Text != null ? findtxt.Text : string.Empty;
            /////Common Code

            if (ReplaceInSelection) // Replace All in selected text block
            {
                _ow.ReplaceAllInSelectedTextBlock(findtext, replacetext, _findReplaceSearchFlags);
            }
            else // Replace all in whole text
            {
                //SetFindReplaceSearchFlag();
                //if (_ow == null) return;
                bool foundanother = true;
                //string replacetext = replacetxt.Text != null ? replacetxt.Text : string.Empty; ;
                //string findtext = findtxt.Text != null ? findtxt.Text : string.Empty;
                do
                {
                    foundanother = _ow.ReplaceWith(findtext, replacetext, _findReplaceSearchFlags);
                } while (foundanother);
            }
            //MessageBox.Show(this,"All replaced.");
        }

        public bool ReplaceInSelection { get; set; }
        //if replacement is only needed to be done in selected text
        private void InSelectionCheck_Checked(object sender, RoutedEventArgs e)
        {
            ReplaceInSelection = InSelectionCheck.IsChecked == true ? true : false;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            if (_ow == null) return;

            _ow.CloseFindReplace();
        }








    }
}
