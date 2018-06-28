using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BSky.ExportToPDF
{
    public class ExportGraphics
    {
        public static MigraDoc.DocumentObjectModel.Paragraph  ExportToPDF(string imagefulpathname)
        {

            // Add a paragraph to the section
            MigraDoc.DocumentObjectModel.Paragraph paragraph = new MigraDoc.DocumentObjectModel.Paragraph();

            MigraDoc.DocumentObjectModel.Shapes.Image img1 = paragraph.AddImage(imagefulpathname);
            img1.LockAspectRatio = true;
            return paragraph;
        }
    }
}
