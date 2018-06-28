using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace BSky.ExportToPDF
{
    public class ExportNotes
    {

        readonly static MigraDoc.DocumentObjectModel.Color TableBorder = new MigraDoc.DocumentObjectModel.Color(81, 125, 192);
        readonly static MigraDoc.DocumentObjectModel.Color TableBlue = new MigraDoc.DocumentObjectModel.Color(235, 240, 249);
        readonly static MigraDoc.DocumentObjectModel.Color TableGray = new MigraDoc.DocumentObjectModel.Color(242, 242, 242);

        public static MigraDoc.DocumentObjectModel.Tables.Table ExportToPDF(string[,] NotesData, string fontsize)
        {
            //Find dimentions of data area
            int rowcount = NotesData.GetLength(0);
            int colcount = NotesData.GetLength(1);

            //def table
            MigraDoc.DocumentObjectModel.Tables.Table table = new MigraDoc.DocumentObjectModel.Tables.Table();
            table.Borders.Width = 0.5;

            string celltext;

            MigraDoc.DocumentObjectModel.Tables.Column col;
            MigraDoc.DocumentObjectModel.Tables.Row row;
            MigraDoc.DocumentObjectModel.Tables.Cell cell;

            //def column
            for (int i = 0; i < colcount; i++)
            {
                col = table.AddColumn(MigraDoc.DocumentObjectModel.Unit.FromCentimeter(5*(1+i)));
                col.Format.Alignment = MigraDoc.DocumentObjectModel.ParagraphAlignment.Left;
            }

            //creating left top corner and col headers of a row
            for (int i = 0; i < 1; i++)
            {
                row = table.AddRow();
                row.HeadingFormat = true;
                //Empty Left top corner
                for (int a = 0; a < 1; a++)
                {
                    cell = row.Cells[a];
                    cell.AddParagraph(" ");
                    cell.Shading.Color = TableBlue;
                }


                //Col headers
                for (int b = 0; b < colcount; b++)
                {
                    celltext = "";
                    cell = row.Cells[b];
                    cell.AddParagraph(celltext);
                    cell.Format.Font.Bold = true;
                    cell.Shading.Color = TableBlue;
                    cell.Format.Alignment = MigraDoc.DocumentObjectModel.ParagraphAlignment.Center;
                    cell.VerticalAlignment = MigraDoc.DocumentObjectModel.Tables.VerticalAlignment.Center;
                }
            }


            //Creating rowheaders and Data cols of a row
            for (int i = 0; i < rowcount; i++)
            {
                row = table.AddRow();

                MigraDoc.DocumentObjectModel.Paragraph paragraph;
                //data in current row
                for (int b = 0; b < colcount; b++)
                {
                    celltext =  NotesData[i, b];
                    cell = row.Cells[b];

                    cell.AddParagraph(NotesData[i, b]);
                    cell.Format.Alignment = MigraDoc.DocumentObjectModel.ParagraphAlignment.Center;
                    cell.VerticalAlignment = MigraDoc.DocumentObjectModel.Tables.VerticalAlignment.Center;
                }
            }

            return table;
         
        }

    }
}
