using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace BSky.Controls
{
    /// <summary>
    /// Interaction logic for AUParagraph.xaml
    /// </summary>
    public partial class AUColorSelection : UserControl
    {
        public AUColorSelection()
        {
            InitializeComponent();
        }

        public Color Color
        {
            get { return (Color)GetValue(ColorProperty); }
            set { SetValue(ColorProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Color.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ColorProperty =
            DependencyProperty.Register("Color", typeof(Color), typeof(AUColorSelection), new UIPropertyMetadata(Colors.Black, OnColorPropertyChanged));

        private static void OnColorPropertyChanged(DependencyObject source,DependencyPropertyChangedEventArgs e)
        {
            Color c = (Color)e.NewValue ;
            AUColorSelection AUselect = source as AUColorSelection;
            AUselect.AdjustSliders(c);
        }

        public void AdjustSliders(Color c)
        {
            sldBlu.Value = c.B;
            sldGrn.Value = c.G;
            sldRed.Value = c.R;
        }
        private void sldRed_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            this.Color = Color.FromRgb((byte)sldRed.Value, (byte)sldGrn.Value, (byte)sldBlu.Value);            
        }

        
    }
}
