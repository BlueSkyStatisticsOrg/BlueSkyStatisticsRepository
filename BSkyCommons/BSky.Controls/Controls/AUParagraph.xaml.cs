using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using BSky.Interfaces.Controls;
using MSExcelInterop;
using BSky.Lifetime;
using BSky.Lifetime.Interfaces;
using System.Globalization;
using BSky.ConfService.Intf.Interfaces;

namespace BSky.Controls
{
    /// <summary>
    /// Interaction logic for AUParagraph.xaml
    /// </summary>

    public partial class AUParagraph : UserControl, IAUControl
    {
        ILoggerService logService = LifetimeService.Instance.Container.Resolve<ILoggerService>();//17Dec2012
        IConfigService confService = LifetimeService.Instance.Container.Resolve<IConfigService>();//23nov2012

        public AUParagraph()
        {
            InitializeComponent();
            base.Focusable = true;
            //MyBlock.FontSize = 16;
            //MyBox.FontSize = 16;
        }
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public Brush textcolor
        {
            get { return MyBlock.Foreground; } // for saving to output file
            set
            {
                MyBlock.Foreground = value;
            }

        }

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string),
              typeof(AUParagraph), new UIPropertyMetadata("Sample Text"));

        private void MyBlock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                MyBlock.Visibility = Visibility.Collapsed; 
                MyBox.Visibility = Visibility.Visible;
                MyBox.Focus();
                e.Handled = true;
            }
        }

        private void MyBox_LostFocus(object sender, RoutedEventArgs e)
        {
            MyBox.Visibility = Visibility.Collapsed;
            MyBlock.Visibility = Visibility.Visible;
        }


        #region IAUControl Members

        private string _ControlType;
        public string ControlType
        {
            get {return _ControlType; }
            set
            {
                _ControlType = value;
                //if (_ControlType.Equals("Header") || _ControlType.Equals("DataSet"))
                //    this.BSkyControlVisibility = Visibility.Visible;
                //else
                //    this.BSkyControlVisibility = Visibility.Collapsed;
                switch (_ControlType)
                {
                    case "Header":
                        this.BSkyControlVisibility = Visibility.Visible;
                        break;
                    case "DataSet":
                        this.BSkyControlVisibility = Visibility.Visible;
                        break;
                    case "Command":
                        string showSyntaxInOutput = confService.GetConfigValueForKey("ShowSyntaxInOutput");
                        if (showSyntaxInOutput.Trim().ToLower().Equals("true"))
                        {
                            this.BSkyControlVisibility = Visibility.Visible;
                        }
                        else
                        {
                            this.BSkyControlVisibility = Visibility.Collapsed;
                        }
                        break;
                    //case "Header":
                    //    this.BSkyControlVisibility = Visibility.Visible;
                    //    break;
                    default:
                        this.BSkyControlVisibility = Visibility.Visible;
                        break;
                }



            }
        }
        public string NodeText
        {
            get;
            set;
        }
        public Thickness outerborderthickness
        {
            get { return outerborder.BorderThickness; }
            set { outerborder.BorderThickness = value; }
        }

        //05Jun2013
        public SolidColorBrush controlsselectedcolor
        {
            get;
            set;
        }

        //11Nov2013
        public SolidColorBrush controlsmouseovercolor
        {
            get;
            set;
        }

        //11Nov2013
        public SolidColorBrush bordercolor
        {
            get { return (SolidColorBrush)outerborder.BorderBrush; }
            set{ outerborder.BorderBrush = value; }
        }

        //23Sep2013 To set visiblity in output window
        public System.Windows.Visibility BSkyControlVisibility
        {
            get { return this.Visibility; }
            set { this.Visibility = value; }
        }

		public bool DeleteControl { get; set; }
        #endregion

        private void aupara_MouseEnter(object sender, MouseEventArgs e)
        {
            string mousehovercol = confService.GetConfigValueForKey("outputmousehovercol");//23nov2012
            byte red = byte.Parse(mousehovercol.Substring(3, 2), NumberStyles.HexNumber);
            byte green = byte.Parse(mousehovercol.Substring(5, 2), NumberStyles.HexNumber);
            byte blue = byte.Parse(mousehovercol.Substring(7, 2), NumberStyles.HexNumber);
            Color c = Color.FromArgb(255, red, green, blue);
            //outerborder.BorderThickness = new Thickness(1);
            controlsselectedcolor = (SolidColorBrush)outerborder.BorderBrush;//11Nov2013 storing current
            outerborder.BorderBrush = new SolidColorBrush(c);//Colors.DarkOrange);
        }

        private void aupara_MouseLeave(object sender, MouseEventArgs e)
        {
           // outerborder.BorderThickness = new Thickness(0);
            outerborder.BorderBrush = controlsselectedcolor;// new SolidColorBrush(Colors.Transparent);
        }

        private void CommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            //06Apr2018 ExportToExcel();

            //06Apr2018
            if (MyBlock.Text != null || MyBlock.Text.Length > 0)
            {
                Clipboard.SetText(MyBlock.Text);
            }
        }
        private void ContextMenuCopyCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        #region Export To Excel

        MSExportToExcel _MSExcelObj;
        public MSExportToExcel MSExcelObj
        {
            //get; 
            set { _MSExcelObj = value; }
        }

        private void ExportToExcel()
        {
            bool isMSExport = true;
            if (isMSExport)
            {
                MSExportToExcel();
            }
            else
            {
                //Export using Google DLL
            }
        }

        // Use MicroSoft Interop DLL for exporting to Excel
        private void MSExportToExcel()
        {
            string auptext = this.Text;
            try
            {

                if (_MSExcelObj == null)// || _MSExcelObj.ExcelApp == null || !(_MSExcelObj.ExcelApp.Visible))
                {
                    _MSExcelObj = new MSExportToExcel();
                }
                _MSExcelObj.ExportAUParagraph(auptext);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error exporting to Excel. Make sure Excel is installed.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                logService.WriteToLogLevel("Error exporting to Excel. Detailed Message: " + ex.Message, LogLevelEnum.Warn);
            }
                
        }

        // Use Google DLL for exporting to Excel
        private void GoogleExportToExcel()
        {
            string auptext = this.Text;
        }
        #endregion

        #region Export To PDF

        //public Paragraph ExportPDFParagraph()
        //{

        //    string text = Text.Replace(Environment.NewLine, String.Empty).Replace("  ", String.Empty);

        //    //Font Weight
        //    int  fwt;
        //    FontWeight fw = FontWeight;
        //    FontWeightConverter fwc = new FontWeightConverter();
        //    string fontwt = fwc.ConvertToString(fw);
        //    //if (fontwt == null)
        //    //{
        //    //    fontwt = "{Normal}";
        //    //    fwt = Font.NORMAL;
        //    //}

        //    switch (fontwt)
        //    {
        //        case "SemiBold":
        //            fwt = Font.BOLD;
        //            break;
        //        case "Normal":
        //            fwt = Font.NORMAL;
        //            break;
        //        default:
        //            fwt = Font.NORMAL;
        //            break;
        //    }


        //    //Font Color
        //    Color fontcolor = (textcolor as SolidColorBrush).Color;

        //    //Font Size
        //    if (FontSize == 0) FontSize = 14;

        //    Font textfont = new Font(Font.FontFamily.COURIER, (float)FontSize, fwt, new BaseColor(fontcolor.R, fontcolor.G, fontcolor.B, fontcolor.A)); //textcolor);  FontWeight
        //    //Font lightblue = new Font(Font.COURIER, 9f, Font.NORMAL, new Color(43, 145, 175));
        //    //Font courier = new Font(Font.COURIER, 9f);
        //    //Font georgia = FontFactory.GetFont("georgia", 10f);

        //    Chunk beginning = new Chunk(Text, textfont);

        //    Phrase p1 = new Phrase(beginning);
        //    Paragraph p = new Paragraph();

        //    p.Add(p1);
        //    p.SpacingAfter = 20f;
        //    return p;
        //}
        #endregion

            //05Apr2018
            //I dont want context menu opening when AUPara control is a part of FlexGrid table like
            // First title, second title, signinf codes, and table number.
        private void MyBlock_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            if (this.Name.Equals("tbltitle") ||
                this.Name.Equals("txtHeader") ||
                this.Name.Equals("starText") ||
                this.Name.Equals("tableno")
                )
            {
                e.Handled = true; //do not display context menu
            }
        }
		
        private void _delete_Click(object sender, RoutedEventArgs e)
        {
            DeleteControl = true;
            //MessageBox.Show("Deleting this item");
            //OutputWindowContainer owc = (LifetimeService.Instance.Container.Resolve<IOutputWindowContainer>()) as OutputWindowContainer;
            //ow = owc.ActiveOutputWindow as OutputWindow; //get currently active window            
        }		
    }
}
