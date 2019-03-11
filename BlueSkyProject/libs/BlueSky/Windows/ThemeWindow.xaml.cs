using BSky.ConfService.Intf.Interfaces;
using BSky.Lifetime;
using BSky.Lifetime.Interfaces;
using BSky.Statistics.Service.Engine.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace BlueSky.Windows
{
    /// <summary>
    /// Interaction logic for ThemeWindow.xaml
    /// </summary>
    public partial class ThemeWindow : Window
    {
        IAnalyticsService analytics = LifetimeService.Instance.Container.Resolve<IAnalyticsService>();//18Sep2014
        IConfigService confService = LifetimeService.Instance.Container.Resolve<IConfigService>();//18Sep2014
        ILoggerService logService = LifetimeService.Instance.Container.Resolve<ILoggerService>();//18Sep2014
        Window1 mainwin = LifetimeService.Instance.Container.Resolve<Window1>();//11Sep2016

        bool ShowConfirmation;
        bool hasGraphicImageSizeEdited;
        bool isSyntaxGraphicRefreshed = false;

        public ThemeWindow()
        {
            InitializeComponent();
            LoadComboboxes();//loading some hard-coded values
            LoadCustom(); //getting defaults from config file
            this.DataContext = AllAppSettings;
            hasGraphicImageSizeEdited = false;
            ShowConfirmation = true;
        }

        NameValueCollection AllAppSettings;
        IConfigService conService;

        #region Load AppSettings

        private void LoadCustom() //for user's custom settings
        {
            conService = LifetimeService.Instance.Container.Resolve<IConfigService>();
            conService.LoadConfig();//load new settings  
            AllAppSettings = conService.AppSettings;
            //tempfolder.Text = conService.AppSettings.Get("tempfolder");
            //04Mar2016
            SetComboBoxes();
        }

        private void SetComboBoxes()
        {
            string fontfamily = AllAppSettings.Get("FontFamily");
            string fontface = AllAppSettings.Get("FontFace");
            string theme = AllAppSettings.Get("PlotTheme");

            SetSelectedItemInComboBox(fontfamilyCombo, fontfamily);
            SetSelectedItemInComboBox(fontfaceCombo, fontface);
            SetSelectedItemInComboBox(themeCombo, theme);
        }

        private void SetSelectedItemInComboBox(ComboBox comb, string selectedText)
        {
            foreach (string cbi in comb.Items)
            {
                if (cbi.Equals(selectedText))
                {
                    comb.SelectedValue=cbi;
                }
            }
        }



        #endregion

        #region Click events
        //Load defaults
        private void DefaultBtn_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult mbre = MessageBox.Show(this,
                BSky.GlobalResources.Properties.UICtrlResources.ConfSettingOverwriteDefaults,
                BSky.GlobalResources.Properties.UICtrlResources.ConfSettingOverwriteDefaultsTitle,
                MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (mbre == MessageBoxResult.No)
            {
                return;
            }

            //SET DEFAULTS IN MEMORY BUT CLICK SAVE AFTERWARDS TO SAVE TO CONFIG FILE
            conService.LoadDefaultsinUI("themeWindow");
            if (!conService.Success)
                MessageBox.Show(conService.Message);

            //11Jan2018 To refresh with defaults on the fly
            {
                AllAppSettings = conService.AppSettings;

                //Default Settings is a Dictionary object so no need of INotifyPropertyChanged. 
                //AllAppSettings is NameValueCollection so it needs INotify.... 
                this.DataContext = conService.DefaultSettings;// AllAppSettings;

                SetComboBoxes();

                hasGraphicImageSizeEdited = false;
                ShowConfirmation = true;//11Jan2018
            }
        }

        //save current setting to file
        private void ApplyBtn_Click(object sender, RoutedEventArgs e)
        {
            ShowConfirmation = false;
            SaveSettings();
            this.Close();// close Options Window
        }

        //Discard changes and close window.
        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            ShowConfirmation = false;
            this.Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (ShowConfirmation)
            {
                MessageBoxResult mbres = MessageBox.Show(this, BSky.GlobalResources.Properties.UICtrlResources.ConfSettingSaveConfirmation,
                    BSky.GlobalResources.Properties.UICtrlResources.ConfSettingSaveConfirmTitle,
                    MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (mbres == MessageBoxResult.Yes)
                {
                    SaveSettings();
                }
                else
                {
                    //no need to call this.Close(); here as it will become recursive.
                }
            }
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            hasGraphicImageSizeEdited = true;//set to true if image size fields are edited
        }

        private void SaveSettings()
        {
            #region Save all tabs settings at once
            AllAppSettings.Set("FontFamily", (fontfamilyCombo.SelectedItem as string));
            AllAppSettings.Set("FontFace", (fontfaceCombo.SelectedItem as string));
            AllAppSettings.Set("PlotTheme", (themeCombo.SelectedItem as string));
            conService.RefreshConfig();//save all
            #endregion

            //refresh graphic image size, if modified.
            if (hasGraphicImageSizeEdited)
            {
                //image height width edited. Set flag in Syntax telling to refresh image dementions.
                //Launch Syntax Editor window with command pasted 
                if (!isSyntaxGraphicRefreshed) //refresh once
                {
                    ////// Get Syntax Editor  //////
                    SyntaxEditorWindow sewindow = LifetimeService.Instance.Container.Resolve<SyntaxEditorWindow>();
                    //sewindow.Owner = mwindow;
                    sewindow.RefreshImgSizeForGraphicDevice();
                    isSyntaxGraphicRefreshed = true;
                }
                hasGraphicImageSizeEdited = false;
            }
        }

        #endregion

        #region Color related
        //Aaron: no need to make color text editable. User may enter 'red' instead of code and there will a need to verify the value.
        private void FontcolorTxt_TextChanged(object sender, TextChangedEventArgs e)
        {
            string fontcolorcode = fontcolorTxt.Text;
            string R, G, B;
            int red, green, blue;
            try
            {
                if (IsValidColorCode(fontcolorcode))
                {
                    R = fontcolorcode.Substring(1, 2);
                    G = fontcolorcode.Substring(3, 2);
                    B = fontcolorcode.Substring(5, 2);

                    red = Convert.ToInt32(R, 16);
                    green = Convert.ToInt32(G, 16);
                    blue = Convert.ToInt32(B, 16);

                    System.Drawing.Color SelColor = System.Drawing.Color.FromArgb(red, green, blue);

                    System.Windows.Media.Color palettcolor = new System.Windows.Media.Color();
                    palettcolor.A = SelColor.A;
                    palettcolor.R = SelColor.R;
                    palettcolor.G = SelColor.G;
                    palettcolor.B = SelColor.B;
                    palett.Fill = new SolidColorBrush(palettcolor); 
                }
            }
            catch (Exception ex)
            {
                //some error occurred. may be user entered invalid color code
            }
        }

        //This can be enhanced further to verify the color code based on some complex logic.
        private bool IsValidColorCode(string strcolorcode)
        {
            bool isValid = false;
            if (strcolorcode.Length == 7 && strcolorcode.StartsWith("#"))
            {
                return true;
            }

            return isValid;

        }

        private void Pallet_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            string controlname = (sender as FrameworkElement).Name; 
            System.Windows.Shapes.Rectangle r = (System.Windows.Shapes.Rectangle)sender;

            //Get Current color
            SolidColorBrush scb = r.Fill as SolidColorBrush;
            var DrColor = System.Drawing.Color.FromArgb(scb.Color.A, scb.Color.R, scb.Color.G, scb.Color.B);

            //Windows Forms color picker tool
            System.Windows.Forms.ColorDialog cd = new System.Windows.Forms.ColorDialog();
            cd.FullOpen = true;
            cd.Color = DrColor;
            cd.ShowDialog();
            System.Windows.Media.Color selcolor = new System.Windows.Media.Color();
            selcolor.A = cd.Color.A;
            selcolor.R = cd.Color.R;
            selcolor.G = cd.Color.G;
            selcolor.B = cd.Color.B;
            string hexcolor = "#" + selcolor.R.ToString("X2") + selcolor.G.ToString("X2") + selcolor.B.ToString("X2");
            r.Fill = new SolidColorBrush(selcolor);
            fontcolorTxt.Text = hexcolor;
            //AllAppSettings.Set(controlname, hexcolor);
            cd.Dispose();
        }
        #endregion

        #region Populate combo boxes
        //Load all combo boxes
        private void LoadComboboxes()
        {
            LoadFontFamily();
            LoadFontFace();
            LoadThemes();
        }

        //fill Font Family
        private void LoadFontFamily()
        {
            List<string> fontsfamily = new List<string>();
            fontsfamily.Add("sans");
            fontsfamily.Add("serif");
            fontsfamily.Add("mono");
            fontsfamily.Add("Calibri");
            fontsfamily.Add("Times");//short name : serif
            fontsfamily.Add("Helvetica");//short name : sans
            fontsfamily.Add("Courier");//short name : mono

            //http://www.cookbook-r.com/Graphs/Fonts/
            //fontsfamily.Add("AvantGarde");
            //fontsfamily.Add("Bookman");
            //fontsfamily.Add("Helvetica-Narrow");
            //fontsfamily.Add("NewCenturySchoolbook");
            //fontsfamily.Add("Palatino");
            //fontsfamily.Add("URWGothic");
            //fontsfamily.Add("URWBookman");
            //fontsfamily.Add("NimbusMon");
            //fontsfamily.Add("NimbusSan");//short name : URWHelvetica
            //fontsfamily.Add("NimbusSanCond");
            //fontsfamily.Add("CenturySch");
            //fontsfamily.Add("URWPalladio");
            //fontsfamily.Add("NimbusRom");//short name : URWTimes

            fontfamilyCombo.ItemsSource = fontsfamily;
        }

        //fill Font Face
        private void LoadFontFace()
        {
            List<string> fontface = new List<string>();
            fontface.Add("plain");
            fontface.Add("bold");
            fontface.Add("italic");
            fontface.Add("bold.italic");

            fontfaceCombo.ItemsSource = fontface;
        }

        //fill Plot Themes
        private void LoadThemes()
        {
            List<string> themes = new List<string>();
            themes.Add("theme_grey()");
            themes.Add("theme_bw()");
            themes.Add("theme_few()");
            themes.Add("theme_calc()");
            themes.Add("theme_economist()");
            themes.Add("theme_excel()");
            themes.Add("theme_fivethirtyeight()");
            themes.Add("theme_gdocs()");
            themes.Add("theme_hc()");
            themes.Add("theme_pander()");
            themes.Add("theme_solarized()");
            themes.Add("theme_stata()");
            themes.Add("theme_tufte()");
            themes.Add("theme_wsj()");

            themeCombo.ItemsSource = themes;
        }

        #endregion
    }
}
