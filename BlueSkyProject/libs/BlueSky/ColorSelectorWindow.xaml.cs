using System.Windows;
using System.Windows.Media;

namespace BlueSky
{
    /// <summary>
    /// Interaction logic for ColorSelectorWindow.xaml
    /// </summary>
    public partial class ColorSelectorWindow : Window
    {
        
        public ColorSelectorWindow()
        {
            InitializeComponent();
            
        }

        #region Properties
        private SolidColorBrush oldcolor;
        public SolidColorBrush OldColor
        {
            get
            {
                return oldcolor;
            }
            set
            {
                oldcolor = new SolidColorBrush(value.Color);
                SimpleColorPicker.SelectedColor = value;
            }
        }

        private SolidColorBrush currentcolor;
        public SolidColorBrush CurrentColor
        {
            get
            {
                return currentcolor;
            }
        }

        private string hexcolor;
        public string HexColor
        {
            get { return hexcolor; }
            set { hexcolor = value; }
        }
        #endregion

        private void ok_Click(object sender, RoutedEventArgs e)
        {
            currentcolor = SimpleColorPicker.SelectedColor;
            Color c = SimpleColorPicker.SelectedColor.Color;
            hexcolor ="#FF"+ c.R.ToString("X2") + c.G.ToString("X2") + c.B.ToString("X2");
            this.Close();
        }

        private void cancel_Click(object sender, RoutedEventArgs e)
        {
            currentcolor = oldcolor;
            this.Close();
        }


    }
}
