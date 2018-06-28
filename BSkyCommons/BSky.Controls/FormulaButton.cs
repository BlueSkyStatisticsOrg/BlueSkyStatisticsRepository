using System.Windows.Controls;
using System.Windows;

namespace BSky.Controls
{
    public class FormulaButton:Button
    {
        public FormulaButton()
        {

        }

        //public string name
        //{
        //    get
        //    { return base.Name; }

        //    set
        //    { base.Name = value; }
        //}

        protected override void OnClick()
        {
            string buttonText = " "+this.Content as string+" ";//23Feb2017 extra space around operator should be good for readability and editing
            BSkygridForSymbols g = UIHelper.FindVisualParent<BSkygridForSymbols>(this);
            FrameworkElement fe = GetResource(g.textBoxName);
            BSkyTextBox tb = fe as BSkyTextBox;
            int curcursorpos = tb.SelectionStart;//23Feb2017 saving it beacuse it is getting lost after running insert()
            tb.Text = tb.Text.Insert(tb.SelectionStart, buttonText);//23Feb2017 insert at cursor location. Old code -> //tb.AppendText(buttonText);
            tb.SelectionStart = curcursorpos + buttonText.Length;//23Feb2017 move the cursor to the end of the text that was just inserted.
        }


        public FrameworkElement GetResource(string name)
        {
            BSkyCanvas canvas = UIHelper.FindVisualParent<BSkyCanvas>(this);
            foreach (FrameworkElement fe in canvas.Children)
            {
                if (fe.Name == name)
                    return fe;
            }
            return null;
        }
    }

   
}
