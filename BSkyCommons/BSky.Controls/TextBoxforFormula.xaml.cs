using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using BSky.Statistics.Common;


namespace BSky.Controls
{
    /// <summary>
    /// Interaction logic for TextBoxforFormula.xaml
    /// </summary>
    public partial class TextBoxforFormula : TextBox
    {
        public TextBoxforFormula()
        {
            InitializeComponent();
        }
        private void TextBox_DragOver(object sender, DragEventArgs e)
        {

            string[] formats = e.Data.GetFormats();
            DataSourceVariable sourcedata = e.Data.GetData(formats[0]) as DataSourceVariable;
            if (sourcedata != null)
                e.Effects = DragDropEffects.Copy;
            e.Handled = true;
        }

        public virtual void TextBox_Drop(object sender, DragEventArgs e)
        {
           // int selindex = 0;
            string[] formats = e.Data.GetFormats();
            DataSourceVariable sourcedata = e.Data.GetData(formats[0]) as DataSourceVariable;
            //txtCommand.Text = txtCommand.Text.Insert(txtCommand.SelectionStart, "{" + o.Name + "}");
            //this.Text = this.Text.Insert(this.SelectionStart, sourcedata.Name);
            //selindex = this.SelectionStart;
            //selindex = this.Text.Length;
            //this.Text = this.Text.Insert(selindex, sourcedata.Name);
            //this.ScrollToEnd();
            //this.Focus();
           this.AppendText(sourcedata.Name);
           // this.SelectionStart = this.Text.Length;
            this.ScrollToEnd();
            //this.Focus();
            //this.ScrollToCaret();
            //  txt.

        }

        //Added 12/22/2013
        //Added this code to prevent users from being able to select something from a textbox and darg and drop it to another control i.e. listbox

        protected override void OnMouseMove(MouseEventArgs e)
        {
            //We set BSkyCanvas.sourceDrag to null and check it in the listbox dragover function
            //If it is set to null, we don't allow drag and drop
            
            BSkyCanvas.sourceDrag = null;
            base.OnMouseMove(e);
           
            return;
        }
    }
}
