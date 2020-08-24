using BSky.ConfService.Intf.Interfaces;
using BSky.Lifetime;
using BSky.Lifetime.Interfaces;
using BSky.Statistics.Common;
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
            SetCurrentValues();//for keeping old values for tracking changes.
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

        #region Values modified or not
        string[] CurrentValues;
        private void SetCurrentValues()
        {
            CurrentValues = new string[9];
            CurrentValues[0] = AllAppSettings["FontFamily"];// fontfamilyCombo.SelectedValue.ToString();//font-family
            CurrentValues[1] = AllAppSettings["FontFace"];// fontfaceCombo.SelectedValue.ToString();//font-face

            CurrentValues[2] = AllAppSettings["LabelFontSize"];// fontsizeTxt.Text;//font-size
            CurrentValues[3] = AllAppSettings["LabelFontColor"];// fontcolorTxt.Text.ToString();//font-color

            CurrentValues[4] = AllAppSettings["HorizAdjust"];// HorizAdjTxt.Text.ToString();//Horizontal adjust
            CurrentValues[5] = AllAppSettings["VertiAdjust"];// VertiAdjTxt.Text.ToString();//Vertical adjust

            CurrentValues[6] = AllAppSettings["PlotTheme"];// themeCombo.SelectedValue.ToString();//Theme

            CurrentValues[7] = AllAppSettings["imagewidth"];// imgWidthTxt.Text.ToString();//image width
            CurrentValues[8] = AllAppSettings["imageheight"];// imgHeightTxt.Text.ToString();//image height
        }

        string[] ModifiedValues;
        private void SetModifiedValues()
        {
            ModifiedValues = new string[9];
            ModifiedValues[0] = fontfamilyCombo.SelectedValue.ToString();//font-family
            ModifiedValues[1] = fontfaceCombo.SelectedValue.ToString();//font-face

            ModifiedValues[2] = fontsizeTxt.Text;//font-size
            ModifiedValues[3] = fontcolorTxt.Text.ToString();//font-color

            ModifiedValues[4] = HorizAdjTxt.Text.ToString();//Horizontal adjust
            ModifiedValues[5] = VertiAdjTxt.Text.ToString();//Vertical adjust

            ModifiedValues[6] = themeCombo.SelectedValue.ToString();//Theme

            ModifiedValues[7] = imgWidthTxt.Text.ToString();//image width
            ModifiedValues[8] = imgHeightTxt.Text.ToString();//image height
        }

        private bool isValueModified()
        {
            bool isModified = false;
            if (CurrentValues != null && ModifiedValues != null && CurrentValues.Length==ModifiedValues.Length)
            {
                for(int i = 0; i < 9; i++)
                {
                    if (!(CurrentValues[i].Equals(ModifiedValues[i])))
                    {
                        isModified = true;
                    }
                }
            }
            return isModified;
        }

        #endregion

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
        bool isSaveClicked = false;
        private void ApplyBtn_Click(object sender, RoutedEventArgs e)
        {
            isSaveClicked = true;
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
            if (!isSaveClicked)
            {
                SetModifiedValues();
                ShowConfirmation = isValueModified();
            }
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
        IAnalyticsService analyticServ = LifetimeService.Instance.Container.Resolve<IAnalyticsService>();
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
            string[] fonts;
            CommandRequest cr = new CommandRequest();
            cr.CommandSyntax = "GetFonts()";
            object obj = analyticServ.ExecuteR(cr, true, false);
            if (obj != null && (obj as string[]) != null)
            {
                fonts = obj as string[];
                foreach (string s in fonts)
                {
                    if (!string.IsNullOrEmpty(s))
                            fontsfamily.Add(s);
                }
            }
            
            if(obj== null || (obj as string[]) == null || fontsfamily.Count<1)
            { 
                //hardcoded values if fetch from R fails.
                fontsfamily.Add("sans");
                fontsfamily.Add("serif");
                fontsfamily.Add("mono");
                //fontsfamily.Add("Calibri");
                //fontsfamily.Add("Times");//short name : serif
                //fontsfamily.Add("Helvetica");//short name : sans
                //fontsfamily.Add("Courier");//short name : mono

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
            }
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
            string[] themelist;
            CommandRequest cr = new CommandRequest();
            cr.CommandSyntax = "GetThemes()";
            object obj = analyticServ.ExecuteR(cr, true, false);
            if (obj != null && (obj as string[])!=null )
            {
                themelist = obj as string[];
                foreach (string s in themelist)
                {
                    if (!string.IsNullOrEmpty(s))
                        themes.Add(s+"()");
                }

                //this is an extra theme that we want to add by hardcoding
                themes.Add("theme_grey()");
                //More from B
                themes.Add("theme_bw()");//from B
                themes.Add("theme_classic()");//from B
                themes.Add("theme_dark()");//from B
                themes.Add("theme_gray()"); //from B
                themes.Add("theme_light()");//from B
                themes.Add("theme_linedraw()");//from B
                themes.Add("theme_minimal()");//from B
                themes.Add("theme_test()");//from B
                themes.Add("theme_void()");//from B

                themes.Sort();
            }

            if (obj == null || (obj as string[]) == null || themes.Count < 1)
            {
                //http://www.rpubs.com/Mentors_Ubiqum/ggthemes_1
                themes.Add("theme_base()");//
                themes.Add("theme_bw()");//from B
                themes.Add("theme_calc()");
                themes.Add("theme_classic()");//from B
                themes.Add("theme_dark()");//from B
                themes.Add("theme_economist()");
                themes.Add("theme_economist_white()");//
                themes.Add("theme_excel()");
                themes.Add("theme_few()");
                themes.Add("theme_fivethirtyeight()");
                themes.Add("theme_foundation()");
                themes.Add("theme_gdocs()");
                themes.Add("theme_gray()"); //from B
                themes.Add("theme_grey()");
                themes.Add("theme_hc()");
                themes.Add("theme_igray()");//
                themes.Add("theme_light()");//from B
                themes.Add("theme_linedraw()");//from B
                themes.Add("theme_map()");//
                themes.Add("theme_minimal()");//from B
                themes.Add("theme_pander()");
                themes.Add("theme_par()");//
                themes.Add("theme_solarized()");
                themes.Add("theme_solarized()_2");//
                themes.Add("theme_solid()");//
                themes.Add("theme_stata()");
                themes.Add("theme_test()");//from B
                themes.Add("theme_tufte()");
                themes.Add("theme_void()");//from B
                themes.Add("theme_wsj()");
            }
            themeCombo.ItemsSource = themes;
        }

        #endregion

    }
}
