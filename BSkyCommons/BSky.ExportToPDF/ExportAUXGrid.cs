using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using C1.WPF.FlexGrid;
using System.Windows;

namespace BSky.ExportToPDF
{
    public class ExportAUXGrid
    {
        readonly static MigraDoc.DocumentObjectModel.Color TableBorder = new MigraDoc.DocumentObjectModel.Color(81, 125, 192);
        readonly static MigraDoc.DocumentObjectModel.Color TableBlue = new MigraDoc.DocumentObjectModel.Color(235, 240, 249);
        readonly static MigraDoc.DocumentObjectModel.Color TableGray = new MigraDoc.DocumentObjectModel.Color(242, 242, 242);

        readonly static int MaxRowPerSplitTable = 9000; // 18 was tested default
        readonly static int MaxColPerSplitTable = 8; // 8 was tested default

        private void splitExportC1FlexgridButton_Click(object sender, RoutedEventArgs e)
        {

            double PDFPageHeight;
            C1FlexGrid augrid = new C1FlexGrid();// PopulateTable();
            string strMaxTblCol = "7", strMaxTblRow = "20", strPDFfontsize = "12";

            MigraDoc.DocumentObjectModel.Document doc = new MigraDoc.DocumentObjectModel.Document();
            PDFPageHeight = doc.DefaultPageSetup.PageHeight.Centimeter;

            MigraDoc.DocumentObjectModel.Section sec = doc.AddSection();
            sec.AddParagraph("BSky Flexgrid Output");
            sec.AddParagraph(); //empty space


            List<MigraDoc.DocumentObjectModel.Tables.Table> tableParts = ExportMultiHeaderFlexgridToPDF(PDFPageHeight, augrid, strMaxTblCol, strMaxTblRow, strPDFfontsize);

            //finally rendring doc and then saving to disk file
            foreach (MigraDoc.DocumentObjectModel.Tables.Table t in tableParts)
            {
                //add table part to doc
                doc.LastSection.Add(t); //table.Format.KeepWithNext = true;
                sec.AddParagraph();
            }
            //rendering doc
            MigraDoc.Rendering.PdfDocumentRenderer docrender = new MigraDoc.Rendering.PdfDocumentRenderer(false);
            docrender.Document = doc;
            docrender.RenderDocument();
            string filename = "D:\\BSkyFG6.pdf";
            docrender.PdfDocument.Save(filename);

            MessageBox.Show("Export Finished");

        }

        public static List<MigraDoc.DocumentObjectModel.Tables.Table> ExportMultiHeaderFlexgridToPDF0(double PDFPageHeight, C1FlexGrid augrid, string strMaxTblCol, string strMaxTblRow, string strPDFfontsize)
        {
            List<MigraDoc.DocumentObjectModel.Tables.Table> PartialPDFTables = new List<MigraDoc.DocumentObjectModel.Tables.Table>();

            //Find dimentions of col header area
            int colheaderRowCount = augrid.ColumnHeaders.Rows.Count;
            int colheaderColCount = augrid.ColumnHeaders.Columns.Count;

            //Find dimentions of row header area
            int rowheaderRowCount = augrid.RowHeaders.Rows.Count;
            int rowheaderColCount = augrid.RowHeaders.Columns.Count;

            //Find dimentions of data area
            int colcount = augrid.Columns.Count;
            int rowcount = augrid.Rows.Count;

            //Collect Flexgrid table Col/Row Headers and Data too
            string[,] FGColHdrs = new string[colheaderRowCount, colheaderColCount];
            string[,] FGRowHdrs = new string[rowheaderRowCount, rowheaderColCount];
            string[,] FGData = new string[rowcount, colcount];


            //Collect the Column headers
            for (int i = 0; i < colheaderRowCount; i++)
            {
                //Col Headers of current row
                for (int j = 0; j < colheaderColCount; j++)
                {
                    if (augrid.ColumnHeaders[i, j] != null)
                        FGColHdrs[i, j] = augrid.ColumnHeaders[i, j].ToString();
                    else
                        FGColHdrs[i, j] = "";
                }
            }

            //Collect the Row headers
            string rowheaderstring = string.Empty;
            for (int i = 0; i < rowheaderRowCount; i++)// for all rows
            {
                //write row headers
                for (int j = 0; j < rowheaderColCount; j++)// Row Headers of current row
                {
                    if (augrid.RowHeaders[i, j] != null)
                        FGRowHdrs[i, j] = augrid.RowHeaders[i, j].ToString();
                    else
                        FGRowHdrs[i, j] = "";
                }
            }

            //Collect the Data
            for (int i = 0; i < rowcount; i++)// for all rows
            {
                //Write the data
                for (int c = 0; c < colcount; c++)  //data cells of current row
                {
                    try
                    {
                        if (augrid[i, c] != null )
                            FGData[i, c] = augrid[i, c].ToString();
                        else
                        {
                            if (augrid.Columns[c].Tag != null)
                            {
                                FGData[i, c] = "BSkySupScript" + i.ToString();
                            }
                            else
                            {
                                FGData[i, c] = "";
                            }
                        }
                    }
                    catch { }
                }

            }

            MigraDoc.DocumentObjectModel.Tables.Table pdfTable;

            int MaxColperpage, MaxRowperpage;

            //for bad data we set defaults
            if (!Int32.TryParse(strMaxTblCol, out MaxColperpage)) MaxColperpage = rowheaderColCount + 5;
            if (!Int32.TryParse(strMaxTblRow, out MaxRowperpage)) MaxRowperpage = colheaderRowCount + 15;

            int MaxDataColPerPage = MaxColperpage - rowheaderColCount;
            int MaxDataRowPerPage = MaxRowperpage - colheaderRowCount;

            if (MaxDataColPerPage < 1) MaxDataColPerPage = 1;
            if (MaxDataRowPerPage < 1) MaxDataRowPerPage = 1;

            int startcolidx = 0;
            int endcolidx = -1;
            int startrowidx = 0;
            int endrowidx = -1;
            int TotalRowsInColHeaders = colheaderRowCount;//same for each page ////zero based index, so -1
            int TotalColsInRowHeaders = rowheaderColCount;//same for each page ////zero based index, so -1
            int remainingRows, remainingCols, colidx, rowidx;
            double currentTableHeight, remainingPageHeight = PDFPageHeight;// Doc.PageSize.Height;
            int remainder, quotient, NoOfPagesRequired;

            string[,] partRowHeaders = null;
            string[,] partColHeaders = null;
            string[,] partData = null;

            //For large FlexGrid : Creating page
            for (int row = startrowidx; row < rowheaderRowCount; row = endrowidx + 1)
            {
                remainingRows = rowheaderRowCount - (endrowidx + 1);//calculating count so zero based index has to be converted to count
                endrowidx = row + ((MaxDataRowPerPage < remainingRows) ? MaxDataRowPerPage : remainingRows) - 1;//zero based index, so -1
                colidx = TotalColsInRowHeaders - 1; //changing count to zero based index
                partRowHeaders = GetArraySubset(FGRowHdrs, row, 0, endrowidx, colidx);
                endcolidx = -1;
                for (int col = startcolidx; col < colheaderColCount; col = endcolidx + 1)
                {
                    remainingCols = colheaderColCount - (endcolidx + 1);
                    endcolidx = col + ((MaxDataColPerPage < remainingCols) ? MaxDataColPerPage : remainingCols) - 1;//zero based index, so -1
                    rowidx = TotalRowsInColHeaders - 1;
                    partColHeaders = GetArraySubset(FGColHdrs, 0, col, rowidx, endcolidx);

                    partData = GetArraySubset(FGData, row, col, endrowidx, endcolidx);

                    pdfTable = CreatePDFTable(partRowHeaders, partColHeaders, partData, strPDFfontsize);

                    // Page and Table Height calculations
                    currentTableHeight = 50;
                    //For the first row there is no need to add new page. Its a fresh/blank document already.
                    if (row > 0 && remainingPageHeight < currentTableHeight)
                    {
                        PartialPDFTables.Add(null);// Here 'null' will refer to new page  //Doc.NewPage();
                    }
                    PartialPDFTables.Add(pdfTable);

                    //Find remaining space left after adding the current table. This will be used for the next table.
                    remainder = (int)((PDFPageHeight) % (currentTableHeight));
                    quotient = (int)((PDFPageHeight) / (currentTableHeight));
                    if (remainder > 0) quotient = quotient + 1;

                    NoOfPagesRequired = quotient;// quotient is total pages required to fit a very large(or small table)

                    // remaining height after adding current table.
                    remainingPageHeight = (PDFPageHeight * NoOfPagesRequired) % currentTableHeight;//This remaining space will be used for next iteration.

                }

            }

            ////End///// Generating Multi-paged PDF with Multi-row/col headers in each page /////////

            return PartialPDFTables; // return all partial tables.
        }

        public static List<MigraDoc.DocumentObjectModel.Tables.Table> ExportMultiHeaderFlexgridToPDF(double PDFPageHeight, C1FlexGrid augrid, string strMaxTblCol, string strMaxTblRow, string strPDFfontsize)
        {
            //Will contain all the portions/parts of the tables those were generated by splitting 1 FlexGrid table.
            List<MigraDoc.DocumentObjectModel.Tables.Table> PartialPDFTables = new List<MigraDoc.DocumentObjectModel.Tables.Table>();

            //Find dimentions of col header area
            int colheaderRowCount = augrid.ColumnHeaders.Rows.Count;
            int colheaderColCount = augrid.ColumnHeaders.Columns.Count;

            //Find dimentions of row header area
            int rowheaderRowCount = augrid.RowHeaders.Rows.Count;
            int rowheaderColCount = augrid.RowHeaders.Columns.Count;

            //Find dimentions of data area
            int colcount = augrid.Columns.Count;
            int rowcount = augrid.Rows.Count;

            //top left corner dimentions
            int topleftrowCount = colheaderColCount;
            int topleftcolCount = rowheaderColCount;

            //Collect Flexgrid table Col/Row Headers and Data too
            string[,] FGColHdrs = new string[colheaderRowCount, colheaderColCount];
            string[,] FGRowHdrs = new string[rowheaderRowCount, rowheaderColCount];
            string[,] FGData = new string[rowcount, colcount];


            //Collect the Column headers
            for (int i = 0; i < colheaderRowCount; i++)
            {
                //Col Headers of current row
                for (int j = 0; j < colheaderColCount; j++)
                {
                    if (augrid.ColumnHeaders[i, j] != null)
                        FGColHdrs[i, j] = augrid.ColumnHeaders[i, j].ToString();
                    else
                        FGColHdrs[i, j] = "";
                }
            }

            //Collect the Row headers
            string rowheaderstring = string.Empty;
            for (int i = 0; i < rowheaderRowCount; i++)// for all rows
            {
                //write row headers
                for (int j = 0; j < rowheaderColCount; j++)// Row Headers of current row
                {
                    if (augrid.RowHeaders[i, j] != null)
                        FGRowHdrs[i, j] = augrid.RowHeaders[i, j].ToString();
                    else
                        FGRowHdrs[i, j] = "";
                }
            }

            //Collect the Data
            for (int i = 0; i < rowcount; i++)// for all rows
            {
                //Write the data
                for (int c = 0; c < colcount; c++)  //data cells of current row
                {
                    try
                    {
                        if (augrid[i, c] != null)
                            FGData[i, c] = augrid[i, c].ToString();
                        else
                        {
                            if (augrid.Columns[c].Tag != null)
                            {
                                FGData[i, c] = "BSkySupScript" +(augrid.Columns[c].Tag as string[])[i]; //superscript text thats visible in FlexGrid table cell
                            }
                            else
                            {
                                FGData[i, c] = "";
                            }
                        }
                    }
                    catch { }
                }

            }

            bool tablewithRowheaders = false;
            if (rowheaderColCount < 8)
            {
                tablewithRowheaders = true;//each table will have row header and so will be meaningful.
            }



            //Creating a PDF doc to which we will add PDFTables
            MigraDoc.DocumentObjectModel.Tables.Table pdfTable;

            ////Start///// Generating Multi-paged PDF with Multi-row/col headers in each page /////////

            int MaxColperTable = MaxColPerSplitTable; // 8 was tested default
            int MaxRowperTable = MaxRowPerSplitTable; // 18 was tested default

            int MaxDataColPerPage = MaxColperTable - rowheaderColCount;
            int MaxDataRowPerPage = MaxRowperTable - colheaderRowCount;

            int startcolidx = 0;
            int endcolidx = -1;// startcolidx + (MaxColPerPage - 1);//zero based index, so -1
            int startrowidx = 0;
            int endrowidx = -1;// startrowidx + (MaxRowPerPage - 1);//zero based index, so -1
            int TotalRowsInColHeaders = colheaderRowCount;//same for each page ////zero based index, so -1
            int TotalColsInRowHeaders = rowheaderColCount;//same for each page ////zero based index, so -1
            int remainingRows, remainingCols, colidx, rowidx;
            double currentTableHeight, remainingPageHeight = PDFPageHeight;// Doc.PageSize.Height;
            int remainder, quotient, NoOfPagesRequired;

            string[,] partRowHeaders = null;
            string[,] partColHeaders = null;
            string[,] partData = null;



            //For large FlexGrid : Creating page
            if (tablewithRowheaders)
            {
                for (int row = startrowidx; row < rowheaderRowCount; row = endrowidx + 1)
                {
                    remainingRows = rowheaderRowCount - (endrowidx + 1);//calculating count so zero based index has to be converted to count
                    endrowidx = row + ((MaxDataRowPerPage < remainingRows) ? MaxDataRowPerPage : remainingRows) - 1;//zero based index, so -1
                    colidx = TotalColsInRowHeaders - 1; //changing count to zero based index
                    partRowHeaders = GetArraySubset(FGRowHdrs, row, 0, endrowidx, colidx);
                    endcolidx = -1;
                    for (int col = startcolidx; col < colheaderColCount; col = endcolidx + 1)
                    {
                        remainingCols = colheaderColCount - (endcolidx + 1);
                        endcolidx = col + ((MaxDataColPerPage < remainingCols) ? MaxDataColPerPage : remainingCols) - 1;//zero based index, so -1
                        rowidx = TotalRowsInColHeaders - 1;
                        partColHeaders = GetArraySubset(FGColHdrs, 0, col, rowidx, endcolidx);

                        partData = GetArraySubset(FGData, row, col, endrowidx, endcolidx);

                        pdfTable = CreatePDFTable(partRowHeaders, partColHeaders, partData, strPDFfontsize);

                        // Page and Table Height calculations
                        currentTableHeight = 50;// pdfTable.TotalHeight; // current table height
                        //For the first row there is no need to add new page. Its a fresh/blank document already.
                        if (row > 0 && remainingPageHeight < currentTableHeight)
                        {
                            PartialPDFTables.Add(null);// Here 'null' will refer to new page  //Doc.NewPage();
                        }
                        PartialPDFTables.Add(pdfTable);

                        //Find remaining space left after adding the current table. This will be used for the next table.
                        remainder = (int)((PDFPageHeight) % (currentTableHeight));
                        quotient = (int)((PDFPageHeight) / (currentTableHeight));
                        if (remainder > 0) quotient = quotient + 1;

                        NoOfPagesRequired = quotient;// quotient is total pages required to fit a very large(or small table)

                        // remaining height after adding current table.
                        remainingPageHeight = (PDFPageHeight * NoOfPagesRequired) % currentTableHeight;//This remaining space will be used for next iteration.

                    }

                }
                ////End///// Generating Multi-paged PDF with Multi-row/col headers in each page /////////
            }
            else //table broken in parts without managing row headers
            {
                int RHrowcount = rowheaderRowCount;
                int RHcolcount = rowheaderColCount;

                int CHrowcount = colheaderRowCount;
                int CHcolcount = colheaderColCount;

                int DTrowcount = rowcount;
                int DTcolcount = colcount;

                int TLrowcount = CHrowcount;
                int TLcolcount = RHcolcount;

                int TotalRows = CHrowcount + DTrowcount;
                int TotalCols = RHcolcount + DTcolcount;

                int SmallTableRowCount = MaxRowperTable;
                int SmallTableColCount = MaxColperTable;

                //these are helpful to put a partial tables unique id(row,col based)
                int tableRowIndex = 0, tableColIndex = 0;

                MigraDoc.DocumentObjectModel.Tables.Column col;
                MigraDoc.DocumentObjectModel.Tables.Row row;
                MigraDoc.DocumentObjectModel.Tables.Cell cell;
                MigraDoc.DocumentObjectModel.Paragraph paragraph;

                int fsiz = 9;//default
                int.TryParse(strPDFfontsize, out fsiz);//user configured font size
                string text, celltext;
                for (int R = 0; R < TotalRows; R = R + SmallTableRowCount, tableRowIndex++)
                {

                    for (int C = 0; C < TotalCols; C = C + SmallTableColCount, tableColIndex++)
                    {
                        pdfTable = new MigraDoc.DocumentObjectModel.Tables.Table();
                        pdfTable.Borders.Width = 0.5;
                        pdfTable.Tag = "Partial Table: (" + tableRowIndex + "," + tableColIndex + ")";


                        //resetting for next loop
                        SmallTableRowCount = MaxRowperTable;
                        SmallTableColCount = MaxColperTable;

                        //No of cols in a row
                        int colsInOneRow = SmallTableColCount;
                        if (TotalCols - C < SmallTableColCount)
                        {
                            SmallTableColCount = TotalCols - C;
                        }

                        //No of rows in a col
                        int rowsInOneCol = SmallTableRowCount;
                        if (TotalRows - R < SmallTableRowCount)
                        {
                            SmallTableRowCount = TotalRows - R;
                        }

                        //def column
                        for (int i = 0; i < SmallTableColCount; i++)
                        {
                            col = pdfTable.AddColumn();
                            col.Format.Alignment = MigraDoc.DocumentObjectModel.ParagraphAlignment.Left;
                        }

                        //small table row/col loop
                        for (int r = 0; r < SmallTableRowCount; r++)
                        {
                            row = pdfTable.AddRow();
                            row.Format.Font.Size = fsiz;
                            row.HeadingFormat = true;
                            for (int c = 0; c < SmallTableColCount; c++)
                            {
                                //leftTop
                                if (R + r < TLrowcount && C + c < TLcolcount) //TopLeft region
                                {
                                    cell = row.Cells[c];
                                    cell.AddParagraph(" ");
                                    cell.Shading.Color = TableBlue;

                                }

                                //colheaders
                                if (C + c >= TLcolcount)
                                {
                                    if (R + r < CHrowcount && C + c - TLcolcount < CHcolcount)
                                    {
                                        text = FGColHdrs[R + r, C + c - TLcolcount];
                                        celltext = InsertSpace(text, 10);
                                        cell = row.Cells[c];
                                        cell.AddParagraph(celltext);
                                        cell.Format.Font.Bold = true;
                                        cell.Shading.Color = TableBlue;

                                    }
                                }

                                //rowheaders
                                if (R + r >= TLrowcount)
                                {
                                    if (R + r - TLrowcount < RHrowcount && C + c < RHcolcount)
                                    {
                                        text = FGRowHdrs[R + r - TLrowcount, C + c];
                                        celltext = InsertSpace(text, 10);
                                        cell = row.Cells[c];
                                        cell.AddParagraph(celltext);
                                        cell.Format.Font.Bold = true;
                                        cell.Shading.Color = TableBlue;
                                    }
                                }

                                //data
                                if ((R + r) >= TLrowcount && (C + c) >= TLcolcount)
                                {
                                    if (R + r - TLrowcount < DTrowcount && C + c - TLcolcount < DTcolcount)
                                    {
                                        text = FGData[R + r - TLrowcount, C + c - TLcolcount];
                                        celltext = InsertSpace(text, 10);
                                        cell = row.Cells[c];

                                        if (text.Contains("BSkySupScript")) //its superscript text
                                        {
                                            paragraph = cell.AddParagraph();
                                            paragraph.AddFormattedText("- ");
                                            celltext = text.Replace("BSkySupScript", "");
                                            paragraph.AddFormattedText(celltext, MigraDoc.DocumentObjectModel.TextFormat.Italic).Font.Superscript = true;
                                            cell.Format.Font.Size = fsiz;
                                        }
                                        else
                                        {
                                            cell.AddParagraph(celltext);
                                        }
                                    }
                                }
                            }//for c
                            //sbtable.AppendLine();
                        }//for r
                        PartialPDFTables.Add(pdfTable);
                    }//for C
                }//for R
            }

            return PartialPDFTables; // return all partial tables.
        }


        private static string[,] GetArraySubset(string[,] arr, int starti, int startj, int endi, int endj)
        {
            string[,] result = new string[endi - starti + 1, endj - startj + 1];
            int i = 0;
            int j = 0;
            for (int r = starti; r <= endi; r++, i++)
            {
                j = 0;
                for (int c = startj; c <= endj; c++, j++)
                {
                    result[i, j] = arr[r, c];
                }
            }
            return result;
        }

        public static bool APAStyle = false;

        //Only creates tables with row-col headers.
        private static MigraDoc.DocumentObjectModel.Tables.Table CreatePDFTable(string[,] RowHeaders, string[,] ColumnHeaders, string[,] data, string strPDFfontsize)
        {
            
            int fsiz = 9;//default
            int.TryParse(strPDFfontsize, out fsiz);//user configured font size

            MigraDoc.DocumentObjectModel.Tables.Table table = new MigraDoc.DocumentObjectModel.Tables.Table();
            if (APAStyle)
            {
                table.Borders.Left.Clear();
                table.Borders.Right.Clear();
            }

            int rowsInColheader = ColumnHeaders.GetLength(0);
            int colsInColheader = ColumnHeaders.GetLength(1);

            int rowsInRowheader = RowHeaders.GetLength(0);
            int colsInRowheader = RowHeaders.GetLength(1);

            int totalColsInWholeTable = colsInColheader + colsInRowheader; //col count of colheader and rowheaders
            int totalRowsInWholeTable = rowsInColheader + rowsInRowheader; // row count of colheader and rowheader
            string celltext;

            MigraDoc.DocumentObjectModel.Tables.Column col;
            MigraDoc.DocumentObjectModel.Tables.Row row;
            MigraDoc.DocumentObjectModel.Tables.Cell cell;

            //def column
            for (int i = 0; i < totalColsInWholeTable; i++)
            {
                col = table.AddColumn();
                if (APAStyle)
                {
                    col.Format.Alignment = MigraDoc.DocumentObjectModel.ParagraphAlignment.Center;
                }
                else
                {
                    col.Format.Alignment = MigraDoc.DocumentObjectModel.ParagraphAlignment.Left;
                }

            }

            //creating left top corner and col headers of a row
            for (int i = 0; i < rowsInColheader; i++)
            {
                row = table.AddRow(); 
                row.Format.Font.Size = fsiz;
                row.HeadingFormat = true;
                if (APAStyle)
                {
                    if (i == 0) //if first row then add a horiz line above
                    {
                        row.Borders.Top.Width = .5;
                    }
                    if ( i == (rowsInColheader - 1)) //if last row header then add a horiz line below
                    {
                        row.Borders.Bottom.Width = .5;
                    }
                }
                //Empty Left top corner
                for (int a = 0; a < colsInRowheader; a++)
                {
                    cell = row.Cells[a];
                    cell.AddParagraph(" ");
                    cell.Shading.Color = APAStyle? (new MigraDoc.DocumentObjectModel.Color(255, 255, 255)) : TableBlue;
                    cell.Borders.Bottom.Color = APAStyle ? (new MigraDoc.DocumentObjectModel.Color(255, 255, 255)) : TableBlue; ;
                }


                //Col headers
                for (int b = 0; b < colsInColheader; b++)
                {
                    celltext = InsertSpace(ColumnHeaders[i, b], 10);//
                    cell = row.Cells[b + colsInRowheader];

                    cell.AddParagraph(celltext);
                    cell.Format.Font.Bold = true;
                    cell.Shading.Color = APAStyle ? (new MigraDoc.DocumentObjectModel.Color(255, 255, 255)) : TableBlue;
                }
            }


            //Creating rowheaders and Data cols of a row
            for (int i = 0; i < rowsInRowheader; i++)
            {
                row = table.AddRow();
                row.Format.Font.Size = fsiz;
                //rowheader cols of a row
                for (int a = 0; a < colsInRowheader; a++)
                {
                    celltext = InsertSpace(RowHeaders[i, a], 10);//
                    cell = row.Cells[a];
                    cell.AddParagraph(celltext);
                    cell.Format.Font.Bold = true;
                    cell.Shading.Color = APAStyle ? (new MigraDoc.DocumentObjectModel.Color(255, 255, 255)) : TableBlue;
                }

                MigraDoc.DocumentObjectModel.Paragraph paragraph;
                //data in current row
                for (int b = 0; b < colsInColheader; b++)
                {
                    celltext = InsertSpace(data[i, b], 10);//
                    cell = row.Cells[b + colsInRowheader];

                    if (data[i, b].Contains("BSkySupScript")) //its superscript text
                    {
                        paragraph = cell.AddParagraph();
                        paragraph.AddFormattedText("- ");
                        celltext = data[i, b].Replace("BSkySupScript", "");
                        paragraph.AddFormattedText(celltext, MigraDoc.DocumentObjectModel.TextFormat.Italic).Font.Superscript = true;
                        cell.Format.Font.Size = fsiz;
                    }
                    else
                    {
                        cell.AddParagraph(celltext);
                    }
                }

                if (APAStyle)
                {
                    if (i == (rowsInRowheader - 1)) //if last row header then add a horiz line below
                    {
                        row.Borders.Bottom.Width = .5;
                    }
                }
            }
            return table;
        }

        //inserts a space after every 10 continuous character. 
        private static string InsertSpace(string s, int spaceafter)
        {
            StringBuilder sb = new StringBuilder(s);

            int idxOfNextSpace;
            int idxtracker = 0;
            for (int i = 0; i < sb.Length; )
            {
                idxOfNextSpace = s.IndexOf(" ", i);
                if (idxOfNextSpace - i > spaceafter) //length of continuous non-space chars is greater than spaceafter.
                {
                    sb.Insert(i + spaceafter + idxtracker, " ");
                    i = i + spaceafter;

                    idxtracker++;
                }
                else if (idxOfNextSpace > 0)
                {
                    i = idxOfNextSpace + 1;
                }
                else
                {
                    if (s.Length - i > spaceafter)
                    {
                        sb.Insert(i + spaceafter + idxtracker, " ");
                        i = i + spaceafter;

                        idxtracker++;
                    }
                    else
                    {
                        break;
                    }
                }
            }

            return sb.ToString();
        }
       
    }
}
