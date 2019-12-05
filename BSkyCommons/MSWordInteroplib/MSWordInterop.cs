using Microsoft.Office.Interop.Word;
using System;
using System.Text;
using System.Windows;

namespace MSWordInteropLib
{
    public class MSWordInterop
    {
        _Application WrdAppObj = null;
        _Document WrdDocObj=null;

        public void GenerateAPATableInWord(string fullpathfilename, string[,] colHeaders, string[,] rowHeaders, 
            string[,] tblData, string tblFirstTitle="", string tblSecTitle="", string tblFootnote="", string tblNo="")
        {
            //StringBuilder sb = new StringBuilder();
            
            //sb.Append("A");
            
            //sb.Append("B");
            try
            {
                object missvalobj = System.Reflection.Missing.Value;
                OpenCOMObject(missvalobj);
                
                //sb.Append("C");
                object wrdDocEndObj = "\\endofdoc"; /* predefined bookmark to find end of the document*/
                                                    //sb.Append("D");
                                                    //create a new Word document.
                //if (WrdAppObj == null)
                //{
                //    WrdAppObj = new Microsoft.Office.Interop.Word.Application();

                //    //sb.Append("E");
                //    WrdAppObj.Visible = false;
                //    //sb.Append("F");
                //    WrdDocObj = WrdAppObj.Documents.Add(ref missvalobj, ref missvalobj, ref missvalobj, ref missvalobj);
                //    //Docobj.Activate();
                //    //sb.Append("G");
                //}
                #region Inserting First and Second Title

                Paragraph wrdParaObj; //define paragraph object
                //sb.Append("H");
                object rangeObj;//define range object
                                //sb.Append("I");

                //Insert a line (gap)
                if (true)
                {
                    rangeObj = WrdDocObj.Bookmarks.get_Item(ref wrdDocEndObj).Range; //go to end of the page
                    wrdParaObj = WrdDocObj.Content.Paragraphs.Add(ref rangeObj); //add paragraph at end of document
                    wrdParaObj.Range.Font.Italic = 1;
                    wrdParaObj.Format.SpaceAfter = 15; 
                    wrdParaObj.Range.InsertParagraphAfter(); //insert paragraph
                }

                //Insert First Title
                if (!string.IsNullOrEmpty(tblFirstTitle))
                {
                    //sb.Append("x2");
                    rangeObj = WrdDocObj.Bookmarks.get_Item(ref wrdDocEndObj).Range; //go to end of the page
                    //sb.Append("a.");
                    wrdParaObj = WrdDocObj.Content.Paragraphs.Add(ref rangeObj); //add paragraph at end of document
                    //sb.Append("b.");
                    wrdParaObj.Range.Font.Italic = 1;
                    //sb.Append("c.");
                    wrdParaObj.Range.Text = tblFirstTitle; //in CamelCase, italic and no fullstop for APA
                    //sb.Append("d.");
                    wrdParaObj.Format.SpaceAfter = 10; //defind some style
                    //sb.Append("e.");
                    wrdParaObj.Range.InsertParagraphAfter(); //insert paragraph
                    //sb.Append("f.");
                }
                //sb.Append("J");
                //Insert Second Title
                if (!string.IsNullOrEmpty(tblSecTitle))
                {
                    //sb.Append("x1");
                    rangeObj = WrdDocObj.Bookmarks.get_Item(ref wrdDocEndObj).Range; //go to end of the page
                    //sb.Append("g.");
                    wrdParaObj = WrdDocObj.Content.Paragraphs.Add(ref rangeObj); //add paragraph at end of document
                    //sb.Append("h.");
                    wrdParaObj.Range.Font.Italic = 1;
                    //sb.Append("i.");
                    wrdParaObj.Range.Text = tblSecTitle;
                    //sb.Append("j.");
                    wrdParaObj.Format.SpaceAfter = 10;
                    //sb.Append("k.");
                    wrdParaObj.Range.InsertParagraphAfter(); //insert paragraph
                    //sb.Append("l.");
                }
                #endregion
                //sb.Append("K");
                //Get table dimensions and try to insert a new table in APA style(later normal style can also be implemented)
                int colHeadersRowCount = colHeaders.GetLength(0);
                int colHeadersColCount = colHeaders.GetLength(1);

                int rowHeadersRowCount =  rowHeaders.GetLength(0);
                int rowHeadersColCount =  rowHeaders.GetLength(1);

                int dataRowCount =  tblData.GetLength(0);
                int dataColCount =  tblData.GetLength(1);

                /* By now
                 * colHeadersRowCount equals rowHeadersColCount
                 * colHeadersColCount equals dataColCount
                 * rowHeadersRowCount equals dataRowCount
                 */

                //Total Rows = ColheaderRows + DataRows
                int TotalRows = colHeadersRowCount + dataRowCount;

                //Total Cols = RowHeaderCols + DataCols
                int TotalCols = rowHeadersColCount + dataColCount;

                //An Exception message reported that dataColCount must be between 1 and 63. 
                if (TotalCols < 1 || TotalCols > 63)
                {
                    //sb.Append("m.");
                    MessageBox.Show("Number of columns must be between 1 and 63", "Too many columns!", MessageBoxButton.OK, MessageBoxImage.Stop);
                    return;
                }
                //sb.Append("L");
                #region Creating APA style table and filling it
                Table tblobj; //create table object
                Range objWordRng = WrdDocObj.Bookmarks.get_Item(ref wrdDocEndObj).Range; //go to end of document
                tblobj = WrdDocObj.Tables.Add(objWordRng, TotalRows, TotalCols, ref missvalobj, ref missvalobj); //add table object in word document
                //sb.Append("M");
                tblobj.Range.ParagraphFormat.SpaceAfter = 0;
                tblobj.Range.ParagraphFormat.Alignment = WdParagraphAlignment.wdAlignParagraphCenter;
                for (int r = 1; r <= colHeadersRowCount; r++)//make col header rows BOLD and UNDERLINE
                {
                    tblobj.Range.Rows[r].Range.Font.Bold = 1;
                    //tblobj.Range.Rows[r].Range.Font.Underline = WdUnderline.wdUnderlineSingle;
                }
                // tblobj.Spacing = 10f;//added spaceing but then bottom border extended in spacing area too.
                //tblobj.Range.Borders[Microsoft.Office.Interop.Word.WdBorderType.wdBorderVertical].LineWidth = WdLineWidth.wdLineWidth050pt;

                //sb.Append("N");
                ///// FILLING COL, ROW, DATA //////
                int ridx, cidx;//for word table
                int tmpCHcidx =0,CHcolidx;//for ColHdr's col index and Colhdr's temp col index
                int mergecellcount = 0;//numer of ColHdr cells to merge if adj cell has same text.
                int currowcellcount = TotalCols;
                string strText, cellText;
                for (ridx = 1; ridx <= TotalRows; ridx++)
                {
                    CHcolidx = 0;
                    for (cidx = 1; cidx <= currowcellcount; cidx++)
                    {
                        tmpCHcidx = CHcolidx;//initialise
                        
                        //One row will have following
                        //1. TopLeft or RowHeaders in first few cols
                        //2. Colheaders or Data in remaining cols

                        if ((ridx >= 1 && ridx <= colHeadersRowCount) &&
                            (cidx >= 1 && cidx <= rowHeadersColCount))//**TOP LEFT** (blank cells)
                        {
                            // tblobj.Cell(ridx, cidx).Range.Text = "."; //add some text to TopLeft cell
                        }
                        else if ((ridx >= 1 && ridx <= colHeadersRowCount) && //**Filling COLUMN HEADERS**
                            (cidx >= rowHeadersColCount + 1 && cidx <= rowHeadersColCount + colHeadersColCount))
                        {
                            cellText = string.IsNullOrEmpty(colHeaders[ridx-1, CHcolidx])?"": colHeaders[ridx - 1, CHcolidx];//(cidx-1 - rowHeadersColCount)
                            tblobj.Cell(ridx, cidx).Range.Text = cellText; //add some text to ColHeader cell

                            ////tblobj.Cell(ridx, cidx).Range.Font.Bold = 1;//make col headers BOLD
                            ////tblobj.Cell(ridx, cidx).Range.Font.Underline = WdUnderline.wdUnderlineSingle;//make col headers Underline
                            /*tblobj.Cell(ridx, cidx).Range.Borders[Microsoft.Office.Interop.Word.WdBorderType.wdBorderHorizontal].LineWidth = WdLineWidth.wdLineWidth075pt;
                            //find adjecent cells(left to right) having same text and merge them in one like in C1FlexGrid*/
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

                            //if (tmpCHcidx >= colHeadersColCount)//means it reached end(last col) and all cell matched
                            //    tmpCHcidx--; //to bring index down to last index. Last iteration above increamented beyond limit

                            if(mergecellcount>0)//no merge if cell is single. Exception is thrown
                                tblobj.Rows[ridx].Cells[cidx].Merge(tblobj.Rows[ridx].Cells[cidx + mergecellcount]);//Merge

                            //full cell line below header. Problem is line joins to the line of adjacent cell whithout
                            //tblobj.Cell(ridx, cidx).Range.Borders[Microsoft.Office.Interop.Word.WdBorderType.wdBorderBottom].LineStyle = WdLineStyle.wdLineStyleSingle;

                            //right size of line but it appears above the heading instead of below.
                            //tblobj.Cell(ridx, cidx).Range.Text = cellText; //add some text to ColHeader cell
                            //tblobj.Cell(ridx, cidx).Range.Font.Bold = 1;//make col headers BOLD
                            //tblobj.Cell(ridx, cidx).Range.Collapse(WdCollapseDirection.wdCollapseStart);
                            //tblobj.Cell(ridx, cidx).Range.InlineShapes.AddHorizontalLineStandard();
                        }
                        else if ((ridx >= colHeadersRowCount + 1 && ridx <= colHeadersRowCount + rowHeadersRowCount) &&
                            (cidx >= 1 && cidx <= rowHeadersColCount)) //** Filling ROW HEADERS**
                        {
                            cellText = string.IsNullOrEmpty(rowHeaders[(ridx-1 - colHeadersRowCount ), cidx-1])?"": rowHeaders[(ridx - 1 - colHeadersRowCount), cidx - 1];
                            tblobj.Cell(ridx, cidx).Range.Text = cellText; //add some text to RowHeader cell
                            tblobj.Cell(ridx, cidx).Range.Font.Bold = 1;//make row headers BOLD
                        }
                        else //** Filling TABLE DATA**
                        {
                            cellText = string.IsNullOrEmpty(tblData[(ridx-1 - colHeadersRowCount), (cidx -1 -rowHeadersColCount)])?"": tblData[(ridx - 1 - colHeadersRowCount), (cidx - 1 - rowHeadersColCount)];
                            tblobj.Cell(ridx, cidx).Range.Text = cellText; //add some text to Data cell
                        }

                        //Center Align text in each cells(RowHeader, ColHeader and Data)
                        ////tblobj.Cell(ridx, cidx).Range.ParagraphFormat.Alignment = Microsoft.Office.Interop.Word.WdParagraphAlignment.wdAlignParagraphCenter;

                        #region Adding Horizontal lines to APA
                        /*
                         * Putting line 
                         * -below second title of the table, i.e. above first col header row 
                         * -below colheaders after which data row appears. i.e. below last col header row
                         * -after last data row
                         */
                        if (ridx == 1)//Above First row of col headers
                        {
                            tblobj.Cell(ridx, cidx).Range.Borders[Microsoft.Office.Interop.Word.WdBorderType.wdBorderTop].LineStyle = WdLineStyle.wdLineStyleSingle;
                        }
                        if (ridx == colHeadersRowCount) //line below last row of col headers
                        {
                            if(cidx > rowHeadersColCount) //to put line only under col headers and not in row header section.
                            tblobj.Cell(ridx, cidx).Range.Borders[Microsoft.Office.Interop.Word.WdBorderType.wdBorderBottom].LineStyle = WdLineStyle.wdLineStyleSingle;
                        }
                        if (ridx == colHeadersRowCount + dataRowCount)//line below last data row
                        {
                            tblobj.Cell(ridx, cidx).Range.Borders[Microsoft.Office.Interop.Word.WdBorderType.wdBorderBottom].LineStyle = WdLineStyle.wdLineStyleSingle;
                        }
                        #endregion

                        currowcellcount = tblobj.Rows[ridx].Cells.Count;//number of cells in current row after merging
                    }
                }

                //Hardcode merging Column header's col1 to col4
                //tblobj.Rows[1].Cells[4].Merge(tblobj.Rows[1].Cells[7]);
                //tblobj.Cell(1, 4).Range.Text = "CH";
                //tblobj.Cell(1, 4).Range.Borders[Microsoft.Office.Interop.Word.WdBorderType.wdBorderBottom].LineStyle = WdLineStyle.wdLineStyleSingle;

                //tblobj.Rows[1].Range.Font.Bold = 1; //make first row of table BOLD
                //tblobj.Columns[1].Width = WrdAppObj.InchesToPoints(3); //increase first column width

                //string longtext = "This is some long text to test the footnote of the insterted APA table." +
                //    "Lets see how well this appears at the bottom of the inserted APA table.";

                #endregion
                //sb.Append("O");
                #region Adding NOTES below table
                //Add NOTES text after table
                if (!string.IsNullOrEmpty(tblFootnote))
                {
                    Paragraph objNotesPara; //define paragraph object
                    object NoteRng = WrdDocObj.Bookmarks.get_Item(ref wrdDocEndObj).Range; //go to end of the page
                    objNotesPara = WrdDocObj.Content.Paragraphs.Add(ref NoteRng); //add paragraph at end of document
                    //sb.Append("n");
                    //Only Note. should be italic with fullstop for APA style
                    objNotesPara.Range.Text = "Note. " + tblFootnote;// +longtext; //tblFootnote 
                    object obstart = objNotesPara.Range.Start; //italics starts
                    object obend = objNotesPara.Range.Start + 6; //italics ends
                    Microsoft.Office.Interop.Word.Range italics = WrdDocObj.Range(ref obstart, ref obend);
                    italics.Italic = 1;
                    //sb.Append("o");
                    objNotesPara.Format.SpaceAfter = 2; //Table no should not be too far
                    objNotesPara.Range.InsertParagraphAfter(); //insert paragraph
                }

                #endregion

                #region Adding table number below NOTES
                //Insert Table Number
                if (!string.IsNullOrEmpty(tblNo))
                {
                    //sb.Append("p.");
                    objWordRng = WrdDocObj.Bookmarks.get_Item(ref wrdDocEndObj).Range;
                    objWordRng.InsertAfter(tblNo);//tblNo
                    //sb.Append("q.");
                }

                #endregion

                #region Saving Word Doc. Not used
                //No need of following
                //objWordRng.InsertParagraphAfter(); //put enter in document
                //objWordRng.InsertAfter("---x---   ---x---   ---x---");

                //for saving as docx file
                //object szPath = "test.docx"; //your file gets saved with name 'test.docx'
                //WrdDocObj.SaveAs(ref szPath);
                #endregion
                //sb.Append("P");
                WrdAppObj.Visible = true;
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message,"Error Code:"+//sb.ToString());
                MessageBox.Show("Error exporting to Microsoft Word. Make sure that Microsoft Word is intalled.");
            }
            finally
            {
                //you can dispose object here
            }

        }

        //initializing several objects to open Word app, Doc etc..
        private void OpenCOMObject(object missvalobj)
        {
            try
            {
                if ((WrdAppObj != null && WrdAppObj.Visible == false) || WrdDocObj.Characters.Count == 0)//if word was closed by user. This throws RPC server exception. 
                {                                                   //catch{} block will freeup the resource and then we reinitialize.
                    CloseCOMObject(false);
                }
            }
            catch (Exception ex)
            {
                CloseCOMObject(false);
            }
            if (WrdAppObj == null)
            {
                //object missvalobj = System.Reflection.Missing.Value;

                WrdAppObj = new Microsoft.Office.Interop.Word.Application();

                WrdAppObj.Visible = false;

                WrdDocObj = WrdAppObj.Documents.Add(ref missvalobj, ref missvalobj, ref missvalobj, ref missvalobj);
            }
        }

        //Freeing up resources allocated by Word and Doc etc..
        private void CloseCOMObject(bool quit)
        {
            if (quit)
            {
                WrdDocObj.Close(true, null, null);
                WrdAppObj.Quit();
            }

            releaseObject(WrdDocObj); WrdDocObj = null;
            releaseObject(WrdAppObj); WrdAppObj = null;
        }

        private void releaseObject(object obj)
        {
            try
            {
                System.Runtime.InteropServices.Marshal.FinalReleaseComObject(obj);// ReleaseComObject(obj);
                obj = null;
            }
            catch (Exception ex)
            {
                obj = null;
                // MessageBox.Show("Unable to release the Object " + ex.ToString());
            }
            finally
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
        }
    }
}
