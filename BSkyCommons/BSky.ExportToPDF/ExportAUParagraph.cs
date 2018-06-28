using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Globalization;


namespace BSky.ExportToPDF
{
    public class ExportAUParagraph
    {

        public static MigraDoc.DocumentObjectModel.Paragraph ExportFormattedText(string Text, FontWeight fntwt, double FontSize, Brush textcolor, FontStyle fstyle)
        {
            bool isSignifCode = false;
            if (Text.Trim().StartsWith("Signif. codes:"))
            {
                isSignifCode = true;
            }


            string text = Text.Replace(Environment.NewLine, String.Empty).Replace("  ", String.Empty);

            //Font Weight
            MigraDoc.DocumentObjectModel.TextFormat txtformat;
            int fwt;
            FontWeight fw = fntwt;
            FontWeightConverter fwc = new FontWeightConverter();
            string fontwt = fwc.ConvertToString(fw);

            bool isItalic = false;
            if (fstyle != null)
            {
                string s = fwc.ConvertToString(fstyle);
                if (s != null && s.Equals("Italic"))
                {
                    isItalic = true;
                }
            }
            switch (fontwt)
            {
                case "SemiBold":
                    if (isItalic)
                        txtformat = MigraDoc.DocumentObjectModel.TextFormat.Bold | MigraDoc.DocumentObjectModel.TextFormat.Italic;
                    else
                        txtformat = MigraDoc.DocumentObjectModel.TextFormat.Bold;
                    break;
                case "Normal":
                    if (isItalic)
                        txtformat = MigraDoc.DocumentObjectModel.TextFormat.NotBold | MigraDoc.DocumentObjectModel.TextFormat.Italic;
                    else
                        txtformat = MigraDoc.DocumentObjectModel.TextFormat.NotBold;
                    break;
                default:
                    if (isItalic)
                        txtformat = MigraDoc.DocumentObjectModel.TextFormat.NotBold | MigraDoc.DocumentObjectModel.TextFormat.Italic;
                    else
                        txtformat = MigraDoc.DocumentObjectModel.TextFormat.NotBold;
                    break;
            }


            //Font Color
            System.Windows.Media.Color fcolor = (textcolor as SolidColorBrush).Color;
            MigraDoc.DocumentObjectModel.Color fontcolor = new MigraDoc.DocumentObjectModel.Color(fcolor.A, fcolor.R, fcolor.G, fcolor.B);

            //Font Size
            if (FontSize == 0) FontSize = 14;

            // Create a new MigraDoc document
            MigraDoc.DocumentObjectModel.Document document = new MigraDoc.DocumentObjectModel.Document();

            // Add a section to the document
            MigraDoc.DocumentObjectModel.Section section = document.AddSection();

            // Add a paragraph to the section
            MigraDoc.DocumentObjectModel.Paragraph paragraph = section.AddParagraph();

            if (isSignifCode)//add 'Notes.' in italics before 'Signif. codes:'
            {
                paragraph.AddFormattedText("Note. ", MigraDoc.DocumentObjectModel.TextFormat.Italic);
            }

            // Add some text to the paragraph
            paragraph.AddFormattedText(Text, txtformat);
            paragraph.Format.Font.Name = "Times New Roman";
            paragraph.Format.Font.Size = FontSize;
            paragraph.Format.Font.Color = fontcolor;
            paragraph.AddLineBreak();
            if (isSignifCode)
            {
                paragraph.AddLineBreak(); //add extra linebreak if 'signif code' is printed
            }

            return paragraph;
        }

    }
}
