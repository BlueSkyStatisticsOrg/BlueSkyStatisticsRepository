using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using BSky.Interfaces.Controls;
using Microsoft.Win32;
using System.IO;
using MSExcelInterop;
using BSky.Lifetime;
using BSky.Lifetime.Interfaces;
using System.Globalization;
using BSky.ConfService.Intf.Interfaces;

namespace BSky.Controls.Controls
{
    /// <summary>
    /// Interaction logic for BSkyGraphicControl.xaml
    /// </summary>, 
    public partial class BSkyGraphicControl : UserControl, IAUControl
    {
        ILoggerService logService = LifetimeService.Instance.Container.Resolve<ILoggerService>();//17Dec2012
        IConfigService confService = LifetimeService.Instance.Container.Resolve<IConfigService>();//23nov2012

        public static int imagecount = 0;//for dynamic naming //13Sep2012
        public BSkyGraphicControl()
        {
            InitializeComponent();
            base.Focusable = true;
            /// Auto-naming the image as soon as it is created //13Sep2012
            imagecount++;
            imagename = "image" + imagecount.ToString();
         }
        
        public ImageSource BSkyImageSource
        {
            get { return graphicImage.Source;}
            set { graphicImage.Source = value; }
        }

        private string imagename;//13Sep2012
        public string ImageName
        {
            get
            {
                return imagename;
            }
        }

        #region IAUControl Members

        public string ControlType
        {
            get;
            set;
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
            set { outerborder.BorderBrush = value; }
        }

        //23Sep2013 To set visiblity in output window
        public System.Windows.Visibility BSkyControlVisibility
        {
            get { return this.Visibility; }
            set { this.Visibility = value; }
        }

        public bool DeleteControl { get; set; }		
        #endregion


        private void BSkyGraphic_MouseEnter(object sender, MouseEventArgs e)
        {
            string mousehovercol = confService.GetConfigValueForKey("outputmousehovercol");//23nov2012
            byte red = byte.Parse(mousehovercol.Substring(3, 2), NumberStyles.HexNumber);
            byte green = byte.Parse(mousehovercol.Substring(5, 2), NumberStyles.HexNumber);
            byte blue = byte.Parse(mousehovercol.Substring(7, 2), NumberStyles.HexNumber);
            Color c = Color.FromArgb(255, red, green, blue);
            //outerborder.BorderThickness = new Thickness(1);
            controlsselectedcolor = (SolidColorBrush)outerborder.BorderBrush;//11Nov2013 storing current
            outerborder.BorderBrush = new SolidColorBrush(c);// (Colors.DarkOrange);
        }

        private void BSkyGraphic_MouseLeave(object sender, MouseEventArgs e)
        {
            // outerborder.BorderThickness = new Thickness(0);
            outerborder.BorderBrush = controlsselectedcolor;// new SolidColorBrush(Colors.Transparent);
        }

        private void SaveAs_Click(object sender, RoutedEventArgs e)
        {
            //string source = this.BSkyImageSource.ToString();
            string filter = "PNG|*.PNG";
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.FileName = this.imagename;
            sfd.Filter = filter; 
            Nullable<bool> dlgresult = sfd.ShowDialog();
            if (dlgresult == true)
            {
                string destination = sfd.FileName;
               // System.IO.File.Copy(source, destination, true);

                FileStream stream = new FileStream(destination, FileMode.Create);
                JpegBitmapEncoder encoder = new JpegBitmapEncoder();
                //TextBlock myTextBlock = new TextBlock();
                //myTextBlock.Text = "Codec Author is: " + encoder.CodecInfo.Author.ToString();
                //encoder.FlipHorizontal = true;
                //encoder.FlipVertical = false;
                //encoder.QualityLevel = 30;
                //encoder.Rotation = Rotation.Rotate90;
                encoder.Frames.Add(BitmapFrame.Create((BitmapSource)this.BSkyImageSource));
                encoder.Save(stream);
                stream.Close();
            }
            
        }

        private void Copy_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetImage((BitmapSource)this.BSkyImageSource);
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
            string auptext = "";//modify for graphiccontrol
            try
            {

                if (_MSExcelObj == null)// || _MSExcelObj.ExcelApp == null || !(_MSExcelObj.ExcelApp.Visible))
                {
                    _MSExcelObj = new MSExportToExcel();
                }
                _MSExcelObj.ExportAUParagraph(auptext);//modify for graphiccontrol
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
            //string auptext = this.Text;//modify for graphiccontrol
        }
        #endregion


        #region Export To PDF

        //public iTextSharp.text.Image ExportToPDF(string imagefulpathname)
        //{
        //    iTextSharp.text.Image graphic = iTextSharp.text.Image.GetInstance(imagefulpathname);
        //    return graphic;
        //}
        #endregion
		
        private void _delete_Click(object sender, RoutedEventArgs e)
        {
            DeleteControl = true;
        }		
    }
}
