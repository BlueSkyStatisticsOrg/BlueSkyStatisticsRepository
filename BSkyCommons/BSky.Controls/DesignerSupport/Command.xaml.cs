using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using BSky.Controls;
using BSky.Interfaces.Controls;

namespace BSky.Interfaces.Commands
{
    /// <summary>
    /// Interaction logic for BaseOptionWindow.xaml
    /// </summary>
    public partial class RCommandDialog : Window
    {
        /// <summary>
        /// class to display controls
        /// </summary>
        class ControlObject
        {
            public string Name { get; set; }
            public string Type { get; set; }
            public string Parent { get; set; }
        }

        public RCommandDialog()
        {
            InitializeComponent();
        }

        public string CommandString
        {
            get
            {
                return txtCommand.Text;
            }
            set
            {
                txtCommand.Text = value;
            }
        }

        private List<ControlObject> GetControls(BSkyCanvas canvas)
        {
            List<ControlObject> lst = new List<ControlObject>();
            foreach (Object obj in canvas.Children)
            {
                if (obj is IBSkyInputControl)
                {
                    IBSkyInputControl ib = obj as IBSkyInputControl;
                    lst.Add(new ControlObject() { Name = ib.Name, Type = ib.GetType().Name });
                }
                if (obj is BSkyButton)
                {
                    FrameworkElement fe = obj as FrameworkElement;
                    BSkyCanvas cs = fe.Resources["dlg"] as BSkyCanvas;
                    if (cs != null)
                    {
                        List<ControlObject> lstTemp = GetControls(cs);
                        foreach (ControlObject co in lstTemp)
                        {
                            co.Parent = ((BSkyButton)obj).Text;
                        }
                        lst.AddRange(lstTemp);
                    }
                }
            }
            return lst;
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
                mylist.ItemsSource = GetControls(value);
            }
        }

        public string GetCommand(BSkyCanvas canvas)
        {
            mylist.ItemsSource = GetControls(canvas);
            string val = txtCommand.Text;
            this.ShowDialog();
            if (this.DialogResult.HasValue && this.DialogResult.Value)
                return txtCommand.Text;
            else
                return val;
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
        protected void HandleDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ListBoxItem li = e.Source as ListBoxItem;
            ControlObject o = li.Content as ControlObject;
            //Aaron 02/10/2012
            //Commented the line below
            //txtCommand.Text.Se += "{" + o.Name + "}";
            //Added the line below to add text to the cursor
            txtCommand.Text = txtCommand.Text.Insert(txtCommand.SelectionStart, "{{" + o.Name + "}}");
        }
    }
}
