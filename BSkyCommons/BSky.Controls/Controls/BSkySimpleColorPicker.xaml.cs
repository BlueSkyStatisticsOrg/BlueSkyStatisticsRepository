using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace BSky.Controls.Controls
{
    /// <summary>
    /// Interaction logic for BSkySimpleColorPicker.xaml
    /// </summary>
    public partial class BSkySimpleColorPicker : UserControl
    {
        public BSkySimpleColorPicker()
        {
            InitializeComponent();
            //selectedColor.Color = Color.FromRgb(0, 0, 0);//initial color
        }

        SolidColorBrush selectedColor = new SolidColorBrush();
        public SolidColorBrush SelectedColor
        {
            get { return selectedColor; }
            set 
            { 
                    selectedColor = value; 
                    rect.Fill = value;
                    byte r = value.Color.R;
                    byte g = value.Color.G;
                    byte b = value.Color.B;
                    redbyte.Text = Convert.ToString( r,10);
                    greenbyte.Text = Convert.ToString(g, 10);
                    bluebyte.Text = Convert.ToString(b, 10);
            }
        }

        private void colorslider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            byte red = (byte)redslider.Value;
            byte green = (byte)greenslider.Value;
            byte blue = (byte)blueslider.Value;
            
            //SolidColorBrush scb = new SolidColorBrush(Color.FromRgb(red, green, blue));
            //selectedColor = scb;
            //redslider.Background = scb;
            //greenslider.Background = scb;
            //blueslider.Background = scb;

            rect.Fill = new SolidColorBrush(Color.FromRgb(red, green, blue));
            selectedColor.Color = Color.FromRgb(red, green, blue);
            redbyte.Text = Convert.ToString( red, 10);
            greenbyte.Text = Convert.ToString(green,10);
            bluebyte.Text = Convert.ToString(blue,10);

        }

        private void colorbyte_TextChanged(object sender, TextChangedEventArgs e)
        {
            byte red=0;
            byte green=0;
            byte blue=0;
            if (Byte.TryParse(redbyte.Text, out red))
            { red = Byte.Parse(redbyte.Text);  }

            if (Byte.TryParse(greenbyte.Text, out green))
            { green = Byte.Parse(greenbyte.Text); }

            if (Byte.TryParse(bluebyte.Text, out blue))
            { blue = Byte.Parse(bluebyte.Text); }

            redslider.Value = red;
            greenslider.Value = green;
            blueslider.Value = blue;
        }
    }
}
