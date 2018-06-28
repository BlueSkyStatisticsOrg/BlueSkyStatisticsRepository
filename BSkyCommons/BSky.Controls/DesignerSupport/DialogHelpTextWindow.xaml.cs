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
using C1.WPF.SpellChecker;

namespace BSky.Controls.DesignerSupport
{
    /// <summary>
    /// Interaction logic for DialogHelpTextWindow.xaml
    /// </summary>
    public partial class DialogHelpTextWindow : Window
    {
        public DialogHelpTextWindow()
        {
            InitializeComponent();
            var spell = new C1SpellChecker();
            spell.MainDictionary.LoadAsync("C1Spell_en-US.dct");
            this.c1RichTextBox1.SpellChecker = spell;
            this.DataContext = this;
        }

        string _formattedHelpHTMLText; // This is Rich text wich is encoded as HTML.
        public string FormattedHelpHTMLText 
        {
            get { //return c1RichTextBox1.Html; 
                return _formattedHelpHTMLText;
            }
            set { _formattedHelpHTMLText = value; } 
        }


        string _unformattedText; //this is unformatted help text. But initially we are not implementing
        public string UnFormattedText 
        {
            get { return c1RichTextBox1.Text; }
            set { _unformattedText = value; } 
        }

        BSkyCanvas _canvas;
        public BSkyCanvas Canvas
        {
            get
            {
                return _canvas;
            }
            set
            {
                _canvas = value;
            }
        }

        private void ok_Click(object sender, RoutedEventArgs e)
        {
            _formattedHelpHTMLText = c1RichTextBox1.Html;
            this.DialogResult = true;
            this.Close();
        }

        private void cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}
