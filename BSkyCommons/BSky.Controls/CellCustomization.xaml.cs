using System;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Globalization;

namespace BSky.Controls
{
    /// <summary>
    /// Interaction logic for CellCustomization.xaml
    /// </summary>
    public partial class CellCustomization : Window
    {
        public CellCustomization(string fgColor, string bgColor, string FontFamily, string style,string size,string HAlign, string Valign)
        {
            InitializeComponent();
            ColorConverter cc = new ColorConverter();
            
            if (!string.IsNullOrEmpty(fgColor))
            {
                FgColor = (Color)cc.ConvertFrom(fgColor);
                sldRed.Value = FgColor.R;
                sldGrn.Value = FgColor.G;
                sldBlu.Value = FgColor.B;
            }
            if (!string.IsNullOrEmpty(bgColor))
            {
                BgColor = (Color)cc.ConvertFrom(bgColor);
                sldbgRed.Value = BgColor.R;
                sldbgGrn.Value = BgColor.G;
                sldbgBlu.Value = BgColor.B;
            }
            if(!string.IsNullOrEmpty(FontFamily))
            {
                family.SelectedValue = new FontFamily(FontFamily); ;
            }
            
            cmbsize.Items.Add("4");
            cmbsize.Items.Add("6");
            cmbsize.Items.Add("8");
            cmbsize.Items.Add("10");
            cmbsize.Items.Add("12");
            cmbsize.Items.Add("14");
            cmbsize.Items.Add("16");
            cmbsize.Items.Add("18");
            cmbsize.Items.Add("20");
            cmbsize.Items.Add("22");
            cmbsize.Items.Add("24");
            if (!string.IsNullOrEmpty(size))
            {
                cmbsize.SelectedValue = size;
            }
            else
            {
                cmbsize.SelectedValue = "10";
            }

            switch (Valign)
            {
                case "Top":
                default:
                    rdVTop.IsChecked = true;
                    break;
                case "Center":
                    rdVCenter.IsChecked = true;
                    break;
                case "Bottom":
                    rdvBottom.IsChecked = true;
                    break;
            }

            switch (HAlign)
            {
                case "Right":
                    rdRight.IsChecked = true;
                    break;
                case "Center":
                    rdCenter.IsChecked = true;
                    break;
                case "Left":
                default:
                    rdLeft.IsChecked = true;
                    break;
            }
        }

        public Color FgColor { get; set; }
        public Color BgColor { get; set; }

        public string VAlignment { get; set; }

        public string HAlignment { get; set; }

        public bool IsForeground { get; set; }

        private byte Red { get; set; }
        private byte Green { get; set; }
        private byte Blue { get; set; }

        public string MarginString { get; set; }


        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            if (rdBack.IsChecked.HasValue && rdBack.IsChecked.Value)
                IsForeground = false;
            else
                IsForeground = true;

            Red = (byte)sldRed.Value;
            Green = (byte)sldGrn.Value;
            Blue = (byte)sldBlu.Value;

            FgColor = Color.FromRgb(Red, Green, Blue);

            Red = (byte)sldbgRed.Value;
            Green = (byte)sldbgGrn.Value;
            Blue = (byte)sldbgBlu.Value;

            BgColor = Color.FromRgb(Red, Green, Blue);


            if (rdVTop.IsChecked == true)
                VAlignment = rdVTop.Content.ToString();
            else if (rdVCenter.IsChecked == true)
                VAlignment = rdVCenter.Content.ToString();
            else if (rdvBottom.IsChecked == true)
                VAlignment = rdvBottom.Content.ToString();


            if (rdLeft.IsChecked == true)
                HAlignment = rdLeft.Content.ToString();
            else if (rdRight.IsChecked == true)
                HAlignment = rdRight.Content.ToString();
            else if (rdCenter.IsChecked == true)
                HAlignment = rdCenter.Content.ToString();

            //devx MarginString = string.Format("{0},{1},{2},{3}", spnLeft.Value.ToString(), spnTop.Value.ToString(), spnRight.Value.ToString(), spnBottom.Value.ToString());

            this.DialogResult = true;
            this.Close();
                 
        }

        private void rdFor_Checked(object sender, RoutedEventArgs e)
        {
            if (rdBack != null)
            {
                if (rdBack.IsChecked.HasValue && rdBack.IsChecked.Value)
                {
                    colorSelectionbg.Visibility = System.Windows.Visibility.Visible;
                    colorSelectionfg.Visibility = System.Windows.Visibility.Collapsed;
                }
                else
                {
                    colorSelectionbg.Visibility = System.Windows.Visibility.Collapsed;
                    colorSelectionfg.Visibility = System.Windows.Visibility.Visible;
                }
            }
        }
    }
    public class BrushConverter : IMultiValueConverter
    {
        public object Converter(object[] values, Type targetType, object parameter, CultureInfo culture)
        {

            return new SolidColorBrush(Color.FromArgb(255, System.Convert.ToByte(values[0]),
                                       System.Convert.ToByte(values[1]),
                                       System.Convert.ToByte(values[2])));
        }
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }

        #region IMultiValueConverter Members

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            return new SolidColorBrush(Color.FromArgb(255, System.Convert.ToByte(values[0]),
                                      System.Convert.ToByte(values[1]),
                                      System.Convert.ToByte(values[2])));
        }

        #endregion
    } 
    
}
