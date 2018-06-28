using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;


namespace BSky.Controls
{
    public class FormulaComputeButton : Button
    {

        public FormulaComputeButton()
        {

        }

      
        protected override void OnClick()
        {
            string buttonText = this.Content as string;
            
            BSkygridForCompute g = UIHelper.FindVisualParent<BSkygridForCompute>(this);
           // g.list1.Items.Add("Ba ba ba ba");
            //FrameworkElement fe = GetResource(g.textBoxName);
            //BSkyTextBox tb = fe as BSkyTextBox;
           // tb.AppendText(buttonText);
            g.setHelpText(buttonText);
                

        }
        protected override void OnMouseDoubleClick(System.Windows.Input.MouseButtonEventArgs e)
        {
            base.OnMouseDoubleClick(e);
            string buttonText = this.Content as string;
            BSkygridForCompute g = UIHelper.FindVisualParent<BSkygridForCompute>(this);
            g.populateFormulaTextbox(buttonText);
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
