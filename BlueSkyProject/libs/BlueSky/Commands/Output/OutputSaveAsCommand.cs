using System;
using Microsoft.Win32;
using System.Windows;
using BlueSky.Services;
using BSky.Lifetime;
using BlueSky.CommandBase;
using BSky.Interfaces.Interfaces;
using BSky.Lifetime.Interfaces;
using System.Drawing;
using System.IO;
using BSky.Interfaces.Services;
namespace BlueSky.Commands.Output
{
    public class OutputSaveAsCommand : BSkyCommandBase
    {
        ILoggerService logService = LifetimeService.Instance.Container.Resolve<ILoggerService>();//17Dec2012

        protected override void OnPreExecute(object param)
        {
        }

        public const String FileNameFilter = "BSky Format, that can be opened in Output Window later (*.bsoz)|*.bsoz|Comma Seperated (*.csv)|*.csv|HTML (*.html)|*.html|PDF (*.pdf)|*.pdf"; //BSkyOutput

        public const String FileNamePDFFilter = "PDF (*.pdf)|*.pdf"; //21Mar2016 Save As PDF only for menu item.

        protected override void OnExecute(object param)
        {
            /// Get the refrence of the output window container
            OutputWindowContainer owc = (LifetimeService.Instance.Container.Resolve<IOutputWindowContainer>()) as OutputWindowContainer;
            OutputWindow ow ;
            if (owc.Count < 1) //28Feb2013 If there is no output window, an error/wrning message must b shown
            {
                MessageBox.Show("There is no output window.");
                return;
            }
             ow = owc.ActiveOutputWindow as OutputWindow; //get currently active window
            //29OCt2013 Saving all output in .bso
            ////if (!ow.IsOneOrMoreSelected())//24Jan2013 
            ////{
            ////    MessageBox.Show("Please select at least one of the output, for saving.");
            ////    ow.BringOnTop();
            ////    return;
            ////}

             SaveFileDialog saveasFileDialog = new SaveFileDialog();
             saveasFileDialog.Filter = FileNameFilter; //default filter
             bool isSaveAsPDF = false;

            // if 'Save' is invoked from specific output window. (it can be active or non-active output window)
            // Then dump should be performed on this specific output window only.
            if (param != null)
            {
                UAMenuCommand uamc = (UAMenuCommand)param;
                if (uamc.commandformat.Length > 0)
                    ow = owc.GetOuputWindow(uamc.commandformat) as OutputWindow;// get specific output window.

                //21Mar2016 //setting right kind of filter
                if (uamc.commandtype.Equals("PDF"))
                {
                    isSaveAsPDF = true;
                }
            }

            if (isSaveAsPDF)
            {
                saveasFileDialog.Filter = FileNamePDFFilter; 
            }

            bool? output = saveasFileDialog.ShowDialog(ow);// (Application.Current.MainWindow);//
            if (output.HasValue && output.Value)
            {
                C1.WPF.FlexGrid.FileFormat fileformat = C1.WPF.FlexGrid.FileFormat.Html;
                bool extratags = false; // false means dont save extratags.(only for .CSV .HTML)

                if (saveasFileDialog.FilterIndex == 1)
                    extratags = true;// save as HTML with extratags (.bso)
                else if (saveasFileDialog.FilterIndex == 2)
                    fileformat = C1.WPF.FlexGrid.FileFormat.Csv; // save as CSV(.csv)
                else if (saveasFileDialog.FilterIndex == 3)
                    fileformat = C1.WPF.FlexGrid.FileFormat.Html;// save as HTML without extratags (.html)
                else if (saveasFileDialog.FilterIndex == 4)// save first as HTML and then use 3rd party library to change it to PDF and delte HTML file
                    fileformat = C1.WPF.FlexGrid.FileFormat.Html;

                //following three line for testing only. Remove or comment them later
                //IConfigService conService = LifetimeService.Instance.Container.Resolve<IConfigService>();
                //string myfname=conService.AppSettings.Get("tempfolder");
                //saveasFileDialog.FileName = myfname;

                //23May2015 Delete file if it already exists. We need this because for HTML file format the same file is
                //opened and more output is appended to it and thus added more HTML and BODY tags to same files, which is
                //not a valid Html syantax. HTML file must have only one HTML and BODY tags.
                //Even though HTML browsers may not report this error and may ignore this completely, but in long run
                //wrong syntax may cause some issues.
                //If you want to save output to a file and keep appending to it, then you must write some extra logic to
                //fix this multiple HTML/BODY tag issue. May be edit file yourself before saving it again after appending.
                //Right now easy option is to delete the file and create a new one.
                bool fileExists = System.IO.File.Exists(saveasFileDialog.FileName);
                if (fileExists && saveasFileDialog.OverwritePrompt)
                {
                    //try deleting file, if allowed( if you have previliges)
                    try
                    {
                        System.IO.File.Delete(saveasFileDialog.FileName);
                    }
                    catch (Exception ex)
                    {
                        logService.WriteToLogLevel("Save output: Deleting existing file error: " + ex.Message, LogLevelEnum.Error);
                    }
                }

                //1. SaveAs PDF from save as dialog dropdown index 4.
                //2. SaveAs PDF from menu item and saveas dialog index will be 1.
                if (saveasFileDialog.FilterIndex == 4 || (saveasFileDialog.FilterIndex == 1 && isSaveAsPDF))
                {
                    ow.SaveAsPDFAllAnalyisOuput(saveasFileDialog.FileName, fileformat, extratags);//Save as PDF using iTextSharp //04Mar2016
                }
                else
                {
                    //dump currently active window in the choosen format and specified filename
                    ow.DumpAllAnalyisOuput(saveasFileDialog.FileName, fileformat, extratags);
                }

                ////28May2015 for PDF. Save as HTML done above. Now take HTML and convert to PDF using 3rd party dlls
                ////Now delete HTML
                //if (saveasFileDialog.FilterIndex == 4)
                //{
                //    //chose right extension (ie.. PDF)
                //    string pdfFileName = saveasFileDialog.FileName.Substring(0,saveasFileDialog.FileName.LastIndexOf(".html"))+".pdf";
                //    if (pdfFileName != null)
                //    {
                //        HTML2PDF(saveasFileDialog.FileName, pdfFileName);
                //    }
                //    //delete HTML
                //    if(System.IO.File.Exists(saveasFileDialog.FileName))
                //    {
                //        try
                //        {
                //            System.IO.File.Delete(saveasFileDialog.FileName);
                //            string imagefilename;
                //            for (int i = 1; i <= 100; i++)//delete images if any
                //            {
                //                imagefilename = saveasFileDialog.FileName + "image" + i+".png";
                //                if (System.IO.File.Exists(imagefilename))
                //                {
                //                    System.IO.File.Delete(imagefilename);
                //                }
                //                else
                //                {
                //                    break;
                //                }
                //            }
                //        }
                //        catch (Exception ex)
                //        {
 
                //        }
                //    }
                //}
            }

        }

        protected override void OnPostExecute(object param)
        {
        }

        ////Send executed command to output window. So, user will know what he executed
        //protected override void SendToOutputWindow(string command, string title)//13Dec2013
        //{
        //    #region Get Active output Window
        //    //////// Active output window ///////
        //    OutputWindowContainer owc = (LifetimeService.Instance.Container.Resolve<IOutputWindowContainer>()) as OutputWindowContainer;
        //    OutputWindow ow = owc.ActiveOutputWindow as OutputWindow; //get currently active window
        //    #endregion
        //    ow.AddMessage(command, title);
        //}
        #region Save HTML as PDF  (04Mar2016 This section may not be required as we switched to iTextSharp. So no more HTML to PDF conversion.
        //private void HTML2PDF(string htmlFilename, string outFilePath) //outFilePath is full path filename of PDF to be generated
        //{
        //    bool addHeader = false, addFooter = false;
        //    bool createSelecteablePDF = true;
        //    //this.Cursor = Cursors.WaitCursor;

        //    //string outFilePath = Path.Combine(Application.StartupPath, "htmltopdf.pdf");

        //    try
        //    {
        //        //set the license key
        //        //LicensingManager.LicenseKey = "put your license key here";

        //        //create a PDF document
        //        Document document = new Document();

        //        //optional settings for the PDF document like margins, compression level,
        //        //security options, viewer preferences, document information, etc
        //        document.CompressionLevel = CompressionLevel.NormalCompression;
        //        document.Margins = new Margins(10, 10, 0, 0);
        //        document.Security.CanPrint = true;
        //        document.Security.UserPassword = "";
        //        document.DocumentInformation.Author = "HTML to PDF Converter";
        //        document.ViewerPreferences.HideToolbar = false;

        //        //Add a first page to the document. The next pages will inherit the settings from this page 
        //        PdfPage page = document.Pages.AddNewPage(PageSize.A4, new Margins(10, 10, 0, 0), PageOrientation.Portrait);

        //        // the code below can be used to create a page with default settings A4, document margins inherited, portrait orientation
        //        //PdfPage page = document.Pages.AddNewPage();

        //        // add a font to the document that can be used for the texts elements 
        //        PdfFont font = document.Fonts.Add(new Font(new FontFamily("Times New Roman"), 10, GraphicsUnit.Point));

        //        // add header and footer before renderng the content
        //        //if (addHeader)
        //        //    AddHtmlHeader(document);
        //        //if (addFooter)
        //        //    AddHtmlFooter(document, font);

        //        // the result of adding an element to a PDF page
        //        AddElementResult addResult;

        //        // Get the specified location and size of the rendered content
        //        // A negative value for width and height means to auto determine
        //        // The auto determined width is the available width in the PDF page
        //        // and the auto determined height is the height necessary to render all the content
        //        float xLocation = 0;// float.Parse(textBoxXLocation.Text.Trim());
        //        float yLocation = 0;// float.Parse(textBoxYLocation.Text.Trim());
        //        float width = -1;// float.Parse(textBoxWidth.Text.Trim());
        //        float height = -1;// float.Parse(textBoxHeight.Text.Trim());

        //        if (createSelecteablePDF)
        //        {
        //            // convert HTML to PDF
        //            HtmlToPdfElement htmlToPdfElement;


        //            // convert a URL to PDF
        //            string urlToConvert = htmlFilename;// textBoxWebPageURL.Text.Trim();

        //            htmlToPdfElement = new HtmlToPdfElement(xLocation, yLocation, width, height, urlToConvert);


        //            //optional settings for the HTML to PDF converter
        //            htmlToPdfElement.FitWidth = true;// cbFitWidth.Checked;
        //            htmlToPdfElement.EmbedFonts = false;// cbEmbedFonts.Checked;
        //            htmlToPdfElement.LiveUrlsEnabled = false;// cbLiveLinks.Checked;
        //            htmlToPdfElement.RightToLeftEnabled = false;// cbRTLEnabled.Checked;
        //            htmlToPdfElement.ScriptsEnabled = false;// cbScriptsEnabled.Checked;
        //            htmlToPdfElement.ActiveXEnabled = false;// cbActiveXEnabled.Checked;

        //            // add theHTML to PDF converter element to page
        //            addResult = page.AddElement(htmlToPdfElement);
        //        }
        //        else
        //        {
        //            HtmlToImageElement htmlToImageElement;

        //            // convert HTML to image and add image to PDF document

        //            // convert a URL to PDF
        //            string urlToConvert = htmlFilename;// textBoxWebPageURL.Text.Trim();

        //            htmlToImageElement = new HtmlToImageElement(xLocation, yLocation, width, height, urlToConvert);

        //            //optional settings for the HTML to PDF converter
        //            htmlToImageElement.FitWidth = true;// cbFitWidth.Checked;
        //            htmlToImageElement.ScriptsEnabled = false;// cbScriptsEnabled.Checked;
        //            htmlToImageElement.ActiveXEnabled = false;// cbActiveXEnabled.Checked;

        //            addResult = page.AddElement(htmlToImageElement);
        //        }

        //        if (false)//cbAdditionalContent.Checked)
        //        {
        //            // The code below can be used add some other elements right under the conversion result 
        //            // like texts or another HTML to PDF conversion

        //            // add a text element right under the HTML to PDF document
        //            PdfPage endPage = document.Pages[addResult.EndPageIndex];
        //            TextElement nextTextElement = new TextElement(0, addResult.EndPageBounds.Bottom + 10, "Below there is another HTML to PDF Element", font);
        //            nextTextElement.ForeColor = Color.Green;
        //            addResult = endPage.AddElement(nextTextElement);

        //            // add another HTML to PDF converter element right under the text element
        //            endPage = document.Pages[addResult.EndPageIndex];
        //            HtmlToPdfElement nextHtmlToPdfElement = new HtmlToPdfElement(0, addResult.EndPageBounds.Bottom + 10, "http://www.google.com");
        //            addResult = endPage.AddElement(nextHtmlToPdfElement);
        //        }

        //        // save the PDF document to disk
        //        document.Save(outFilePath);

        //    }
        //    finally
        //    {
        //        //this.Cursor = Cursors.Arrow;
        //    }

        //    //DialogResult dr = MessageBox.Show("Open the saved file in an external viewer?", "Open Rendered File", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
        //    //if (dr == DialogResult.Yes)
        //    //{
        //    //    try
        //    //    {
        //    //        System.Diagnostics.Process.Start(outFilePath);
        //    //    }
        //    //    catch (Exception ex)
        //    //    {
        //    //        MessageBox.Show(ex.Message);
        //    //        return;
        //    //    }
        //    //}
        //}

        //private void AddHtmlHeader(Document document)
        //{
        //    string headerAndFooterHtmlUrl =  @"HeaderAndFooterHtml.htm";

        //    //create a template to be added in the header and footer
        //    document.HeaderTemplate = document.AddTemplate(document.Pages[0].ClientRectangle.Width, 100);
        //    // create a HTML to PDF converter element to be added to the header template
        //    HtmlToPdfElement headerHtmlToPdf = new HtmlToPdfElement(headerAndFooterHtmlUrl);
        //    document.HeaderTemplate.AddElement(headerHtmlToPdf);
        //}

        //private void AddHtmlFooter(Document document, PdfFont footerPageNumberFont)
        //{
        //    string headerAndFooterHtmlUrl = @"HeaderAndFooterHtml.htm";

        //    //create a template to be added in the header and footer
        //    document.FooterTemplate = document.AddTemplate(document.Pages[0].ClientRectangle.Width, 100);
        //    // create a HTML to PDF converter element to be added to the header template
        //    HtmlToPdfElement footerHtmlToPdf = new HtmlToPdfElement(headerAndFooterHtmlUrl);
        //    document.FooterTemplate.AddElement(footerHtmlToPdf);

        //    // add page number to the footer
        //    TextElement pageNumberText = new TextElement(document.FooterTemplate.ClientRectangle.Width - 100, 30,
        //                        "This is page &p; of &P; pages", footerPageNumberFont);
        //    document.FooterTemplate.AddElement(pageNumberText);
        //}
    
        #endregion
    }
}
