using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Collections.ObjectModel;
using BSky.Interfaces.Model;
using BSky.Interfaces.Commands;
using System.Windows;
using BSky.Controls;
using BSky.Controls.Controls;
using System.Windows.Media.Imaging;


namespace BSky.ExportToPDF
{
    public class ExportBSkyOutputToPDF
    {

        public static string strPDFPageSize, strPDFPageMargin, strMaxTblCol, strMaxTblRow, strPDFfontsize, tempDir;

        public static void SaveAsPDFAllAnalyisOuput(ObservableCollection<AnalyticsData> DataList, string fullpathzipcsvhtmfilename)
        {
            int imgnamecounter = 0;
            List<string> filelist = new List<string>();
            bool extratags = false;
            bool fileExists = File.Exists(fullpathzipcsvhtmfilename);

            double left, top, right, bottom;
            GetPDFpageMargins(strPDFPageMargin, out left, out top, out right, out bottom);

            //Creating a PDF doc to which we will add PDFTables
            MigraDoc.DocumentObjectModel.Document Doc = new MigraDoc.DocumentObjectModel.Document();
            MigraDoc.DocumentObjectModel.Tables.Table pdfTable;

            //Set Page margins from configuration
            Doc.DefaultPageSetup.LeftMargin = MigraDoc.DocumentObjectModel.Unit.FromMillimeter(left);//left margin
            Doc.DefaultPageSetup.TopMargin = MigraDoc.DocumentObjectModel.Unit.FromMillimeter(top);//top margin
            Doc.DefaultPageSetup.RightMargin = MigraDoc.DocumentObjectModel.Unit.FromMillimeter(right);//right margin
            Doc.DefaultPageSetup.BottomMargin = MigraDoc.DocumentObjectModel.Unit.FromMillimeter(bottom);//bottom margin

            Doc.AddSection();

            //////// looping thru all analysis one by one //////
            foreach (AnalyticsData analysisdata in DataList)
            {
                //03Aug2012 ICommandAnalyser analyser = CommandAnalyserFactory.GetClientAnalyser(analysisdata);
                CommandOutput output = analysisdata.Output;// getting refrence of already generated objects.
                SessionOutput sessionoutput = analysisdata.SessionOutput;//27Nov2013 if there is session output
                if (output != null)
                    output.NameOfAnalysis = analysisdata.AnalysisType;//For Parent Node name 02Aug2012
                if (sessionoutput != null)
                    sessionoutput.NameOfSession = analysisdata.AnalysisType;

                /////// dumping output //
                if (output != null)
                {

                    ExportOutputPDF(output,  Doc, extratags, filelist);
                }
                else if (sessionoutput != null)
                {

                    foreach (CommandOutput cout in sessionoutput)
                    {
                        ExportOutputPDF(cout, Doc, extratags, filelist, true);
                    }
                }
            }

            ////rendering doc
            MigraDoc.Rendering.PdfDocumentRenderer docrender = new MigraDoc.Rendering.PdfDocumentRenderer(false);
            docrender.Document = Doc;
            docrender.RenderDocument();
            docrender.PdfDocument.Save(fullpathzipcsvhtmfilename);
        }

        // converts margin string float and split all 4 values from single comma separated string.

        private static void GetPDFpageMargins(string strMargins, out double left, out double top, out double right, out double bottom)
        {
            char[] sep = new char[] { ',' };
            left = top = right = bottom = 10;
            string[] mar = strMargins.Split(sep);

            if (mar.Length == 4)
            {
                if (!double.TryParse(mar[0], out left)) left = 10;
                if (!double.TryParse(mar[1], out top)) top = 10;
                if (!double.TryParse(mar[2], out right)) right = 10;
                if (!double.TryParse(mar[3], out bottom)) bottom = 10;
            }
        }

        public static bool APAStyle = false;

        private static void ExportOutputPDF(CommandOutput output, MigraDoc.DocumentObjectModel.Document Doc, bool extratags, List<string> filelist, bool issessionout = false)//csv of excel
        {
            if (output.NameOfAnalysis == null)
                output.NameOfAnalysis = string.Empty;

            foreach (DependencyObject obj in output)
            {
                FrameworkElement element = obj as FrameworkElement;
                //31Aug2012 AUXGrid xgrid = element as AUXGrid; 
                if ((element as AUParagraph) != null)
                {
                    AUParagraph aup = element as AUParagraph;

                    if (!aup.IsVisible)
                    {
                        continue;
                    }

                    if (aup.Text != null)///// <aup> means AUParagraph
                    {
                        MigraDoc.DocumentObjectModel.Paragraph PDFpara = BSky.ExportToPDF.ExportAUParagraph.ExportFormattedText(aup.Text, aup.FontWeight, aup.FontSize, aup.textcolor,aup.FontStyle); //aup.ExportPDFParagraph();
                        PDFpara.AddLineBreak();
                        Doc.LastSection.Add(PDFpara.Clone());
                        
                    }

                }
                else if ((element as AUXGrid) != null)
                {
                    AUXGrid xgrid = element as AUXGrid; //31Aug2012


                    if (!xgrid.IsVisible)
                    {
                        continue;
                    }
                    ////////// Printing Header //////////  <fgheader> means flexgrid header
                    string header = xgrid.Header.Text;
                    AUParagraph FGTitle = new AUParagraph();
                    FGTitle.Text = header;
                    FGTitle.FontWeight = FontWeights.SemiBold;
                    if (APAStyle)
                    {
                        FGTitle.FontStyle = FontStyles.Italic;
                    }
                    //FGTitle.textcolor = 
                    MigraDoc.DocumentObjectModel.Paragraph PDFpara = BSky.ExportToPDF.ExportAUParagraph.ExportFormattedText(FGTitle.Text, FGTitle.FontWeight, FGTitle.FontSize, FGTitle.textcolor, FGTitle.FontStyle); //FGTitle.ExportPDFParagraph();
                    PDFpara.AddLineBreak();
                    Doc.LastSection.Add(PDFpara.Clone());

                    //////////////// Printing Errors ///////////
                    if (xgrid.Metadata != null)//// <errhd> means error heading
                    {
                        // Error/Warning Title
                        AUParagraph ErrWarnTitle = new AUParagraph();
                        ErrWarnTitle.Text = "Errors/Warnings: "; ;
                        ErrWarnTitle.FontWeight = FontWeights.SemiBold;
                        //ErrWarnTitle.FontStyle = FontStyles.Normal;
                        //ErrWarnTitle.textcolor = 
                        MigraDoc.DocumentObjectModel.Paragraph PDFparaErrWarnTitle = BSky.ExportToPDF.ExportAUParagraph.ExportFormattedText(ErrWarnTitle.Text, ErrWarnTitle.FontWeight, ErrWarnTitle.FontSize, ErrWarnTitle.textcolor, ErrWarnTitle.FontStyle); //ErrWarnTitle.ExportPDFParagraph();
                        PDFparaErrWarnTitle.AddLineBreak();
                        Doc.LastSection.Add(PDFparaErrWarnTitle.Clone());

                        // Error/Warning Messages
                        AUParagraph ErrWarnMsgPara = null;
                        foreach (KeyValuePair<char, string> keyval in xgrid.Metadata)
                        {
                            ErrWarnMsgPara = new AUParagraph();
                            ErrWarnMsgPara.Text = keyval.Key.ToString() + ":" + keyval.Value; 

                            MigraDoc.DocumentObjectModel.Paragraph PDFparaErrWarnMsg = BSky.ExportToPDF.ExportAUParagraph.ExportFormattedText(ErrWarnMsgPara.Text, ErrWarnMsgPara.FontWeight, ErrWarnMsgPara.FontSize, ErrWarnMsgPara.textcolor, ErrWarnMsgPara.FontStyle); //ErrWarnMsgPara.ExportPDFParagraph();
                            PDFparaErrWarnMsg.AddLineBreak();
                            Doc.LastSection.Add(PDFparaErrWarnMsg.Clone());
                        }
                    }

                    //////// Printing  Grid ////////////
                    AUGrid grid = xgrid.Grid;
                    float remainingpagesize = 0.0f;//remaining page height
                    float currentTblHt = 0.0f;
                    float PDFpageHeight = Doc.DefaultPageSetup.PageHeight;

                    BSky.ExportToPDF.ExportAUXGrid.APAStyle = APAStyle;
                    List<MigraDoc.DocumentObjectModel.Tables.Table> TablePortions = BSky.ExportToPDF.ExportAUXGrid.ExportMultiHeaderFlexgridToPDF(PDFpageHeight, grid, strMaxTblCol, strMaxTblRow, strPDFfontsize);  //xgrid.ExportMultiHeaderFlexgridToPDF(PDFpageHeight);

                    foreach (MigraDoc.DocumentObjectModel.Tables.Table ptbl in TablePortions)
                    {
                        if (ptbl == null)
                            continue;
                        
                        //Add Partial Table ID (so that printouts can be arranged in proper order)
                        if(ptbl.Tag!=null) Doc.LastSection.AddParagraph(ptbl.Tag.ToString());

                        //add table part to doc
                        Doc.LastSection.Add(ptbl);

                        Doc.LastSection.AddParagraph().AddLineBreak();
                        
                    }

                    /////////////////Printing Footer  ///////////////
                    string starfootnotes = string.Empty;
                    bool templatedDialog = false;
                    if (templatedDialog)
                    {
                        //I think this works for templated dialogs
                        if (xgrid.FootNotes != null)
                        {
                            // Printing Foonotes Title
                            if (xgrid.FootNotes.Count > 0)
                            {
                                AUParagraph FooterTitle = new AUParagraph();
                                FooterTitle.Text = "Footnotes: "; //Footnote Title
                                FooterTitle.FontWeight = FontWeights.SemiBold;
                                MigraDoc.DocumentObjectModel.Paragraph PDFparaFooterTitle = BSky.ExportToPDF.ExportAUParagraph.ExportFormattedText(FooterTitle.Text, FooterTitle.FontWeight, FooterTitle.FontSize, FooterTitle.textcolor,FooterTitle.FontStyle); //FooterTitle.ExportPDFParagraph();
                                PDFparaFooterTitle.AddLineBreak();
                                Doc.LastSection.Add(PDFparaFooterTitle.Clone());
                            }
                            AUParagraph footnote = null;
                            foreach (KeyValuePair<char, string> keyval in xgrid.FootNotes)
                            {
                                footnote = new AUParagraph();
                                footnote.Text = keyval.Key.ToString() + ":" + keyval.Value; 

                                MigraDoc.DocumentObjectModel.Paragraph PDFparaFootnotesMsg = BSky.ExportToPDF.ExportAUParagraph.ExportFormattedText(footnote.Text, footnote.FontWeight, footnote.FontSize, footnote.textcolor,footnote.FontStyle); //footnote.ExportPDFParagraph();
                                PDFparaFootnotesMsg.AddLineBreak();
                                Doc.LastSection.Add(PDFparaFootnotesMsg.Clone());
                            }
                        }
                    }
                    else //This works for non-templated dialogs
                    {
                        AUParagraph starfootnote = new AUParagraph();
                        starfootnote.Text = xgrid.StarFootNotes;
                        starfootnote.FontSize = 9; //table cell text(& R syntax) looks size 12. So I want it to be smaller than other text.

                        MigraDoc.DocumentObjectModel.Paragraph PDFparaStarFooterText = BSky.ExportToPDF.ExportAUParagraph.ExportFormattedText(starfootnote.Text, starfootnote.FontWeight, starfootnote.FontSize, starfootnote.textcolor,starfootnote.FontStyle); //FooterTitle.ExportPDFParagraph();
                        PDFparaStarFooterText.AddLineBreak();
                        Doc.LastSection.Add(PDFparaStarFooterText.Clone());
                    }

                }
                else if ((element as BSkyGraphicControl) != null)//Graphics 31Aug2012
                {
                    BSkyGraphicControl bsgc = element as BSkyGraphicControl;

                    //To only Export Graphics those are visible (checked in the left navigation tree, in output window)
                    if (!bsgc.IsVisible)
                    {
                        continue;
                    }
                    //Create image filename
                    string imgfilename = "PDF" + bsgc.ImageName + ".png";

                    string synedtimg = Path.Combine(tempDir, imgfilename);

                    //Saving Image separately
                    BSkyGraphicControlToImageFile(bsgc, synedtimg);//not imgfilename

                    MigraDoc.DocumentObjectModel.Paragraph imgPara = BSky.ExportToPDF.ExportGraphics.ExportToPDF(synedtimg); //bsgc.ExportToPDF(synedtimg);// not imgfilename

                    //finally add image to the PDF doc.
                    Doc.LastSection.Add(imgPara);

                }
                else if ((element as BSkyNotes) != null) // Notes Control 05Nov2012. 
                {
                    BSkyNotes bsn = element as BSkyNotes;
                    //To only Export Notes those are visible (checked in the left navigation tree, in output window)
                    if (!bsn.IsVisible)
                    {
                        continue;
                    }

                    //Put a title
                    AUParagraph NotesTitle = new AUParagraph();
                    NotesTitle.Text = "Notes"; //Footnote Title
                    NotesTitle.FontWeight = FontWeights.SemiBold;

                    MigraDoc.DocumentObjectModel.Paragraph PDFparaNotesTitle = BSky.ExportToPDF.ExportAUParagraph.ExportFormattedText(NotesTitle.Text, NotesTitle.FontWeight, NotesTitle.FontSize, NotesTitle.textcolor, NotesTitle.FontStyle); //NotesTitle.ExportPDFParagraph();
                    PDFparaNotesTitle.AddLineBreak();
                    Doc.LastSection.Add(PDFparaNotesTitle.Clone());

                    //Now Notes data in PdfPTable
                    MigraDoc.DocumentObjectModel.Tables.Table notestable = BSky.ExportToPDF.ExportNotes.ExportToPDF(bsn.NotesData, strPDFfontsize);// bsn.ExportToPDF();
                    Doc.LastSection.Add(notestable);
                    Doc.LastSection.AddParagraph().AddLineBreak();
                }
            }

            ////for export to excel////E
        }

        private static void BSkyGraphicControlToImageFile(BSkyGraphicControl bsgc, string fullpathimgfilename)
        {
            System.Windows.Controls.Image myImage = new System.Windows.Controls.Image();
            myImage.Source = bsgc.BSkyImageSource;

            System.Windows.Media.Imaging.BitmapImage bitmapImage = new System.Windows.Media.Imaging.BitmapImage();
            bitmapImage = ((System.Windows.Media.Imaging.BitmapImage)myImage.Source);
            System.Windows.Media.Imaging.PngBitmapEncoder pngBitmapEncoder = new System.Windows.Media.Imaging.PngBitmapEncoder();
            System.IO.FileStream stream = new System.IO.FileStream(fullpathimgfilename, FileMode.Create);

            pngBitmapEncoder.Interlace = PngInterlaceOption.On;
            pngBitmapEncoder.Frames.Add(System.Windows.Media.Imaging.BitmapFrame.Create(bitmapImage));
            pngBitmapEncoder.Save(stream);
            stream.Flush();
            stream.Close();
        }

        #region Exporting single Flexgrid to PDF in normal or APA format

        public static void ExportFlexGridToPDF(string fullpathzipcsvhtmfilename, string TblTitle, AUXGrid fgrid)
        {
            int imgnamecounter = 0;
            List<string> filelist = new List<string>();
            bool fileExists = File.Exists(fullpathzipcsvhtmfilename);

            double left, top, right, bottom;
            GetPDFpageMargins(strPDFPageMargin, out left, out top, out right, out bottom);

            //Creating a PDF doc to which we will add PDFTables
            MigraDoc.DocumentObjectModel.Document Doc = new MigraDoc.DocumentObjectModel.Document();
            MigraDoc.DocumentObjectModel.Tables.Table pdfTable;

            //Set Page margins from configuration
            Doc.DefaultPageSetup.LeftMargin = MigraDoc.DocumentObjectModel.Unit.FromMillimeter(left);//left margin
            Doc.DefaultPageSetup.TopMargin = MigraDoc.DocumentObjectModel.Unit.FromMillimeter(top);//top margin
            Doc.DefaultPageSetup.RightMargin = MigraDoc.DocumentObjectModel.Unit.FromMillimeter(right);//right margin
            Doc.DefaultPageSetup.BottomMargin = MigraDoc.DocumentObjectModel.Unit.FromMillimeter(bottom);//bottom margin

            Doc.AddSection();

            ExportFlexgrid(Doc,  TblTitle,  fgrid);

            ////rendering doc
            MigraDoc.Rendering.PdfDocumentRenderer docrender = new MigraDoc.Rendering.PdfDocumentRenderer(false);
            docrender.Document = Doc;
            docrender.RenderDocument();
            docrender.PdfDocument.Save(fullpathzipcsvhtmfilename);
        }

        private static void ExportFlexgrid(MigraDoc.DocumentObjectModel.Document Doc, string TblTitle, AUXGrid fgrid)
        {
                if ((fgrid) != null)
                {
                    AUXGrid xgrid = fgrid; //31Aug2012

                    ////////// Printing Header //////////  <fgheader> means flexgrid header
                    string header = xgrid.Header.Text;
                    AUParagraph FGTitle = new AUParagraph();
                    FGTitle.Text = header;
                    FGTitle.FontWeight = FontWeights.SemiBold;
                    if (APAStyle)
                    {
                        FGTitle.FontStyle = FontStyles.Italic;
                    }

                    MigraDoc.DocumentObjectModel.Paragraph PDFpara = BSky.ExportToPDF.ExportAUParagraph.ExportFormattedText(FGTitle.Text, FGTitle.FontWeight, FGTitle.FontSize, FGTitle.textcolor, FGTitle.FontStyle); //FGTitle.ExportPDFParagraph();
                    PDFpara.AddLineBreak();
                    Doc.LastSection.Add(PDFpara.Clone());

                    //////////////// Printing Errors ///////////
                    if (xgrid.Metadata != null)//// <errhd> means error heading
                    {
                        // Error/Warning Title
                        AUParagraph ErrWarnTitle = new AUParagraph();
                        ErrWarnTitle.Text = "Errors/Warnings: "; ;
                        ErrWarnTitle.FontWeight = FontWeights.SemiBold;

                        MigraDoc.DocumentObjectModel.Paragraph PDFparaErrWarnTitle = BSky.ExportToPDF.ExportAUParagraph.ExportFormattedText(ErrWarnTitle.Text, ErrWarnTitle.FontWeight, ErrWarnTitle.FontSize, ErrWarnTitle.textcolor, ErrWarnTitle.FontStyle); //ErrWarnTitle.ExportPDFParagraph();
                        PDFparaErrWarnTitle.AddLineBreak();
                        Doc.LastSection.Add(PDFparaErrWarnTitle.Clone());

                        // Error/Warning Messages
                        AUParagraph ErrWarnMsgPara = null;
                        foreach (KeyValuePair<char, string> keyval in xgrid.Metadata)
                        {
                            ErrWarnMsgPara = new AUParagraph();
                            ErrWarnMsgPara.Text = keyval.Key.ToString() + ":" + keyval.Value; 
                            MigraDoc.DocumentObjectModel.Paragraph PDFparaErrWarnMsg = BSky.ExportToPDF.ExportAUParagraph.ExportFormattedText(ErrWarnMsgPara.Text, ErrWarnMsgPara.FontWeight, ErrWarnMsgPara.FontSize, ErrWarnMsgPara.textcolor, ErrWarnMsgPara.FontStyle); //ErrWarnMsgPara.ExportPDFParagraph();
                            PDFparaErrWarnMsg.AddLineBreak();
                            Doc.LastSection.Add(PDFparaErrWarnMsg.Clone());
                        }
                    }

                    //////// Printing  Grid ////////////

                    AUGrid grid = xgrid.Grid;
                    float remainingpagesize = 0.0f;//remaining page height
                    float currentTblHt = 0.0f;
                    float PDFpageHeight = Doc.DefaultPageSetup.PageHeight;
                    BSky.ExportToPDF.ExportAUXGrid.APAStyle = APAStyle;
                    List<MigraDoc.DocumentObjectModel.Tables.Table> TablePortions = BSky.ExportToPDF.ExportAUXGrid.ExportMultiHeaderFlexgridToPDF(PDFpageHeight, grid, strMaxTblCol, strMaxTblRow, strPDFfontsize);  //xgrid.ExportMultiHeaderFlexgridToPDF(PDFpageHeight);
                                                                                                                                                                                                                     // MigraDoc.DocumentObjectModel.Section sec = Doc.AddSection();

                    foreach (MigraDoc.DocumentObjectModel.Tables.Table ptbl in TablePortions)
                    {
                        if (ptbl == null)
                            continue;

                        //Add Partial Table ID (so that printouts can be arranged in proper order)
                        if (ptbl.Tag != null) Doc.LastSection.AddParagraph(ptbl.Tag.ToString());

                        //add table part to doc
                        Doc.LastSection.Add(ptbl); //table.Format.KeepWithNext = true;
                        //sec.AddParagraph();  
                        Doc.LastSection.AddParagraph().AddLineBreak();

                    }

                    /////////////////Printing Footer  ///////////////
                    string starfootnotes = string.Empty;
                    bool templatedDialog = false;
                    if (templatedDialog)
                    {
                        //I think this works for templated dialogs
                        if (xgrid.FootNotes != null)
                        {
                            // Printing Foonotes Title
                            if (xgrid.FootNotes.Count > 0)
                            {
                                AUParagraph FooterTitle = new AUParagraph();
                                FooterTitle.Text = "Footnotes: "; //Footnote Title
                                FooterTitle.FontWeight = FontWeights.SemiBold;
                                //FooterTitle.textcolor = 
                                MigraDoc.DocumentObjectModel.Paragraph PDFparaFooterTitle = BSky.ExportToPDF.ExportAUParagraph.ExportFormattedText(FooterTitle.Text, FooterTitle.FontWeight, FooterTitle.FontSize, FooterTitle.textcolor, FooterTitle.FontStyle); //FooterTitle.ExportPDFParagraph();
                                PDFparaFooterTitle.AddLineBreak();
                                Doc.LastSection.Add(PDFparaFooterTitle.Clone());
                            }
                            AUParagraph footnote = null;
                            foreach (KeyValuePair<char, string> keyval in xgrid.FootNotes)
                            {
                                footnote = new AUParagraph();
                                footnote.Text = keyval.Key.ToString() + ":" + keyval.Value; 
                                MigraDoc.DocumentObjectModel.Paragraph PDFparaFootnotesMsg = BSky.ExportToPDF.ExportAUParagraph.ExportFormattedText(footnote.Text, footnote.FontWeight, footnote.FontSize, footnote.textcolor, footnote.FontStyle); //footnote.ExportPDFParagraph();
                                PDFparaFootnotesMsg.AddLineBreak();
                                Doc.LastSection.Add(PDFparaFootnotesMsg.Clone());
                            }
                        }
                    }
                    else //This works for non-templated dialogs
                    {
                        AUParagraph starfootnote = new AUParagraph();
                        starfootnote.Text = xgrid.StarFootNotes;
                        starfootnote.FontSize = 9; //table cell text(& R syntax) looks size 12. So I want it to be smaller than other text.

                        MigraDoc.DocumentObjectModel.Paragraph PDFparaStarFooterText = BSky.ExportToPDF.ExportAUParagraph.ExportFormattedText(starfootnote.Text, starfootnote.FontWeight, starfootnote.FontSize, starfootnote.textcolor, starfootnote.FontStyle); //FooterTitle.ExportPDFParagraph();
                        PDFparaStarFooterText.AddLineBreak();
                        Doc.LastSection.Add(PDFparaStarFooterText.Clone());
                    }

                }
        }

        #endregion
    }
}
