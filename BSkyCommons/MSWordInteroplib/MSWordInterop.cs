using Microsoft.Office.Interop.Word;
using System;
using System.Windows;

namespace MSWordInteropLib
{
    public class MSWordInterop
    {


        public void GenerateAPATableInWord(string fullpathfilename, string[,] colHeaders, string[,] rowHeaders, 
            string[,] tblData, string tblFirstTitle="", string tblSecTitle="", string tblFootnote="", string tblNo="")
        {

            _Application WrdAppObj;
            _Document WrdDocObj;
            try
            {
                object missvalobj = System.Reflection.Missing.Value;
                object wrdDocEndObj = "\\endofdoc";

                //create a new Word document.
                WrdAppObj = new Microsoft.Office.Interop.Word.Application();
                WrdAppObj.Visible = false;
                WrdDocObj = WrdAppObj.Documents.Add(ref missvalobj, ref missvalobj, ref missvalobj, ref missvalobj);
                
                #region Inserting First and Second Title

                Paragraph wrdParaObj; 
                object rangeObj;
                //Insert First Title
                if (!string.IsNullOrEmpty(tblFirstTitle))
                {
                    rangeObj = WrdDocObj.Bookmarks.get_Item(ref wrdDocEndObj).Range; 
                    wrdParaObj = WrdDocObj.Content.Paragraphs.Add(ref rangeObj); 
                    wrdParaObj.Range.Font.Italic = 1;
                    wrdParaObj.Range.Text = tblFirstTitle; 
                    wrdParaObj.Format.SpaceAfter = 10; 
                    wrdParaObj.Range.InsertParagraphAfter(); 
                }

                //Insert Second Title
                if (!string.IsNullOrEmpty(tblSecTitle))
                {
                    rangeObj = WrdDocObj.Bookmarks.get_Item(ref wrdDocEndObj).Range; //go to end of the page
                    wrdParaObj = WrdDocObj.Content.Paragraphs.Add(ref rangeObj); //add paragraph at end of document
                    wrdParaObj.Range.Font.Italic = 1;
                    wrdParaObj.Range.Text = tblSecTitle;
                    wrdParaObj.Format.SpaceAfter = 10;
                    wrdParaObj.Range.InsertParagraphAfter(); //insert paragraph
                }
                #endregion

                //Get table dimensions and try to insert a new table in APA style
                int colHeadersRowCount = colHeaders.GetLength(0);
                int colHeadersColCount = colHeaders.GetLength(1);

                int rowHeadersRowCount =  rowHeaders.GetLength(0);
                int rowHeadersColCount =  rowHeaders.GetLength(1);

                int dataRowCount =  tblData.GetLength(0);
                int dataColCount =  tblData.GetLength(1);

                //Total Rows = ColheaderRows + DataRows
                int TotalRows = colHeadersRowCount + dataRowCount;

                //Total Cols = RowHeaderCols + DataCols
                int TotalCols = rowHeadersColCount + dataColCount;

                //An Exception message reported that dataColCount must be between 1 and 63. 
                if (TotalCols < 1 || TotalCols > 63)
                {
                    MessageBox.Show("Number of columns must be between 1 and 63", "Too many columns!", MessageBoxButton.OK, MessageBoxImage.Stop);
                    return;
                }

                #region Creating APA style table and filling it
                Table tblobj; //create table object
                Range objWordRng = WrdDocObj.Bookmarks.get_Item(ref wrdDocEndObj).Range; //go to end of document
                tblobj = WrdDocObj.Tables.Add(objWordRng, TotalRows, TotalCols, ref missvalobj, ref missvalobj); 

                tblobj.Range.ParagraphFormat.SpaceAfter = 0;
                tblobj.Range.ParagraphFormat.Alignment = WdParagraphAlignment.wdAlignParagraphCenter;
                for (int r = 1; r <= colHeadersRowCount; r++)
                {
                    tblobj.Range.Rows[r].Range.Font.Bold = 1;
                }

                ///// FILLING COL, ROW, DATA //////
                int ridx, cidx;//for word table
                int tmpCHcidx =0,CHcolidx;
                int mergecellcount = 0;
                int currowcellcount = TotalCols;
                string strText, cellText;
                for (ridx = 1; ridx <= TotalRows; ridx++)
                {
                    CHcolidx = 0;
                    for (cidx = 1; cidx <= currowcellcount; cidx++)
                    {
                        tmpCHcidx = CHcolidx;//initialise

                        if ((ridx >= 1 && ridx <= colHeadersRowCount) &&
                            (cidx >= 1 && cidx <= rowHeadersColCount))
                        {
                        }
                        else if ((ridx >= 1 && ridx <= colHeadersRowCount) && //**Filling COLUMN HEADERS**
                            (cidx >= rowHeadersColCount + 1 && cidx <= rowHeadersColCount + colHeadersColCount))
                        {
                            cellText = string.IsNullOrEmpty(colHeaders[ridx-1, CHcolidx])?"": colHeaders[ridx - 1, CHcolidx];
                            tblobj.Cell(ridx, cidx).Range.Text = cellText; //add some text to ColHeader cell

                            mergecellcount = -1;
                            for (; tmpCHcidx < colHeadersColCount; tmpCHcidx++)
                            {
                                string tcolhdr = string.IsNullOrEmpty(colHeaders[ridx - 1, tmpCHcidx]) ? "" : colHeaders[ridx - 1, tmpCHcidx];
                                if (cellText.Equals(tcolhdr))
                                {
                                    mergecellcount++;
                                    continue;
                                }
                                else
                                {
                                    break;
                                }
                            }
                            CHcolidx = tmpCHcidx;

                            if(mergecellcount>0)
                                tblobj.Rows[ridx].Cells[cidx].Merge(tblobj.Rows[ridx].Cells[cidx + mergecellcount]);//Merge

                        }
                        else if ((ridx >= colHeadersRowCount + 1 && ridx <= colHeadersRowCount + rowHeadersRowCount) &&
                            (cidx >= 1 && cidx <= rowHeadersColCount))
                        {
                            cellText = string.IsNullOrEmpty(rowHeaders[(ridx-1 - colHeadersRowCount ), cidx-1])?"": rowHeaders[(ridx - 1 - colHeadersRowCount), cidx - 1];
                            tblobj.Cell(ridx, cidx).Range.Text = cellText; 
                            tblobj.Cell(ridx, cidx).Range.Font.Bold = 1;//make row headers BOLD
                        }
                        else 
                        {
                            cellText = string.IsNullOrEmpty(tblData[(ridx-1 - colHeadersRowCount), (cidx -1 -rowHeadersColCount)])?"": tblData[(ridx - 1 - colHeadersRowCount), (cidx - 1 - rowHeadersColCount)];
                            tblobj.Cell(ridx, cidx).Range.Text = cellText; //add some text to Data cell
                        }

                        #region Adding Horizontal lines to APA

                        if (ridx == 1)
                        {
                            tblobj.Cell(ridx, cidx).Range.Borders[Microsoft.Office.Interop.Word.WdBorderType.wdBorderTop].LineStyle = WdLineStyle.wdLineStyleSingle;
                        }
                        if (ridx == colHeadersRowCount)
                        {
                            if(cidx > rowHeadersColCount)
                            tblobj.Cell(ridx, cidx).Range.Borders[Microsoft.Office.Interop.Word.WdBorderType.wdBorderBottom].LineStyle = WdLineStyle.wdLineStyleSingle;
                        }
                        if (ridx == colHeadersRowCount + dataRowCount)
                        {
                            tblobj.Cell(ridx, cidx).Range.Borders[Microsoft.Office.Interop.Word.WdBorderType.wdBorderBottom].LineStyle = WdLineStyle.wdLineStyleSingle;
                        }
                        #endregion

                        currowcellcount = tblobj.Rows[ridx].Cells.Count;//number of cells in current row after merging
                    }
                }

                #endregion

                #region Adding NOTES below table
                //Add NOTES text after table
                if (!string.IsNullOrEmpty(tblFootnote))
                {
                    Paragraph objNotesPara; //define paragraph object
                    object NoteRng = WrdDocObj.Bookmarks.get_Item(ref wrdDocEndObj).Range;
                    objNotesPara = WrdDocObj.Content.Paragraphs.Add(ref NoteRng); 

                    //Only Note. should be italic with fullstop for APA style
                    objNotesPara.Range.Text = "Note. " + tblFootnote;
                    object obstart = objNotesPara.Range.Start; 
                    object obend = objNotesPara.Range.Start + 6; 
                    Microsoft.Office.Interop.Word.Range italics = WrdDocObj.Range(ref obstart, ref obend);
                    italics.Italic = 1;

                    objNotesPara.Format.SpaceAfter = 2; 
                    objNotesPara.Range.InsertParagraphAfter(); 
                }

                #endregion

                #region Adding table number below NOTES
                //Insert Table Number
                if (!string.IsNullOrEmpty(tblNo))
                {
                    objWordRng = WrdDocObj.Bookmarks.get_Item(ref wrdDocEndObj).Range;
                    objWordRng.InsertAfter(tblNo);
                }

                #endregion

                WrdAppObj.Visible = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error exporting to Microsoft Word. Make sure that Microsoft Word is intalled.");
            }
            finally
            {
            }


        }

    }
}
