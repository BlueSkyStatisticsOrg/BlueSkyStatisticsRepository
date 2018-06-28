using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Office.Interop.Excel;

namespace MSExcelInterop
{
    public class MSExportToExcel
    {

        //private fields
        Microsoft.Office.Interop.Excel._Application app = null;
        Microsoft.Office.Interop.Excel.Workbooks workbooks = null;
        Microsoft.Office.Interop.Excel._Workbook workbook = null;
        Microsoft.Office.Interop.Excel._Worksheet worksheet = null;
        Range ewrange=null;
        Range titlerow=null;
        Range tlr=null;
        Range chr=null;
        Range rhr=null;
        Range dr=null;
        Range fnrange=null;

        Range range=null;

        Range nrange=null;
        Range nrhr =null;

        Range usedrange=null;
        Range used_range = null;

        Range subrange = null;

        string strdata;
        public string StrDataTable 
        {
            get { return strdata; } 
        }
        

        public void CreateAUXGrid(string fullpathexcelfilename, string sheetname,string Title, string[,] ColHdrArr, string[,] RowHdrArr, string[,] dataArr, string errorwarn, string footnotes, string[,] superscript) 
        {
            OpenCOMObject();

            if (errorwarn == null) errorwarn = string.Empty;
            try
            {
                app.Interactive = false;

                string ew = GetNewHomeCellAfterUsedCells(); 
                string ewcol0;
                int ewrow0;
                GetNextCol(ew, 0, out ewcol0, out ewrow0);
                string ewtxtrange = GetStringRange(ewcol0, ewrow0, 1, RowHdrArr.GetLength(1) + ColHdrArr.GetLength(1)); // 1 is because title is 1 row only
                ewrange = worksheet.Range[ewtxtrange];
                string[,] ewtext = new string[1, 1];
                ewtext[0, 0] = errorwarn;

                MergeCells2(ewrange, ewtext[0, 0]);

                ///// Title of the FlexGrid in App
                string tcol;
                int trow;
                GetNextRow(ew, 1, out tcol, out trow);
                string titlehome = tcol + trow.ToString();
                string newcol0;
                int newrow0;
                GetNextCol(titlehome, 0, out newcol0, out newrow0);
                string titlerange = GetStringRange(newcol0, newrow0, 1, RowHdrArr.GetLength(1) + ColHdrArr.GetLength(1));
                titlerow = worksheet.Range[titlerange];
                string[,] title = new string[1, 1];
                title[0, 0] = Title != null ? Title : string.Empty;
                FillDataInRange(titlerow, title);
                MergeAdjacent(titlerow);

                string hcol;
                int hrow;
                // from titlehome we need to find home for grid. 
                GetNextRow(titlehome, 1, out hcol, out hrow);
                string home = hcol + hrow.ToString();//"B7";

                string topleftrrange = GetStringRange(hcol, hrow, ColHdrArr.GetLength(0), RowHdrArr.GetLength(1)); // 
                tlr = worksheet.Range[topleftrrange]; //top left range
                MergeCells(tlr, "");

                //Create col headers
                string newcol1;
                int newrow1;
                GetNextCol(home, RowHdrArr.GetLength(1), out newcol1, out newrow1);
                string colhdrrange = GetStringRange(newcol1, newrow1, ColHdrArr.GetLength(0), ColHdrArr.GetLength(1)); 
                chr = worksheet.Range[colhdrrange];
                FillDataInRange(chr, ColHdrArr);

                MergeAdjacent(chr);

                //Create row headers
                string newcol2;
                int newrow2;
                GetNextRow(home, ColHdrArr.GetLength(0), out newcol2, out newrow2);
                string rowhdrrange = GetStringRange(newcol2, newrow2, RowHdrArr.GetLength(0), RowHdrArr.GetLength(1)); 
                rhr = worksheet.Range[rowhdrrange];
                FillDataInRange(rhr, RowHdrArr);
                MergeAdjacent(rhr);

                //now fill data in data cells
                string datarange = GetStringRange(newcol1, newrow2, dataArr.GetLength(0), dataArr.GetLength(1)); 
                dr = worksheet.Range[datarange];
                FillDataInRange2(dr, dataArr);
                //Box around Grid Data
                BorderAroundRange(dr);

                FillSuperscriptInRange(dr, superscript);

                if (footnotes == null) footnotes = string.Empty;
                string fncol;
                int fnrow;
                GetNextRow(home, ColHdrArr.GetLength(0) + RowHdrArr.GetLength(0), out fncol, out fnrow);
                string fn = fncol + fnrow.ToString(); 
                string fncol0;
                int fnrow0;
                GetNextCol(fn, 0, out fncol0, out fnrow0);
                string fntxtrange = GetStringRange(fncol0, fnrow0, 1, RowHdrArr.GetLength(1) + ColHdrArr.GetLength(1)); 
                fnrange = worksheet.Range[fntxtrange];
                string[,] fntext = new string[1, 1];
                fntext[0, 0] = footnotes;

                MergeCells2(fnrange, fntext[0, 0]);

                ColAutofit();
                // Exit from the application
                ReleaseRange();
            }
            catch (Exception ex)
            {
                System.Console.Beep();
            }
            finally
            {
                app.Interactive = true;
                app.Visible = true;
            }
        }

        public void ExportAUParagraph(string auptext)
        {
            OpenCOMObject();

            string home = GetNewHomeCellAfterUsedCells(); 
            string col;
            int row;
            int noofcells = auptext.Length / 8; 
            GetNextCol(home, 0, out col, out row);
            string txtrange = GetStringRange(col, row, 1, noofcells); 
            range = worksheet.Range[txtrange];
            string[,] text = new string[1, 1];
            text[0, 0] = auptext;
            MergeCells2(range, text[0, 0]);
            ColAutofit();
            ReleaseRange();
        }

        public void ExportBSkyNotes(string header, string[,] notesdata)
        {
            OpenCOMObject();

            string home = GetNewHomeCellAfterUsedCells(); //"B5";
            string col;
            int row;

            int noofcells = 2;

            GetNextCol(home, 0, out col, out row);
            string txtrange = GetStringRange(col, row, 1, noofcells);
            nrange = worksheet.Range[txtrange];
            string[,] text = new string[1, 1];
            text[0, 0] = header;
            MergeCells2(nrange, text[0, 0], false);

            ///Notes Data. Populate in 2 cols
            string newcol2;
            int newrow2;
            GetNextRow(home, 1, out newcol2, out newrow2);
            string rowhdrrange = GetStringRange(newcol2, newrow2, notesdata.GetLength(0), notesdata.GetLength(1)); 
            nrhr = worksheet.Range[rowhdrrange];
            FillDataInRange(nrhr, notesdata, true);
            BorderAroundRange(nrhr);
            ColAutofit();
            ReleaseRange();
        }


        #region  Excel procedures for exporting

        private string GetNewHomeCellAfterUsedCells()
        {
            string newhomecell="A1";
            string newcol, col;
            int newrow, row;
            string usedrangeaddress = null;
            usedrange = null;

            if (worksheet != null)
            {
                try
                {
                    usedrange = worksheet.UsedRange;
                }
                catch (Exception ex) // there is no sheet
                {
                    return "A1";
                }

                usedrangeaddress = usedrange.Address;
                GetCellCoordinates2(usedrangeaddress.Substring(usedrangeaddress.IndexOf(":")+1), out row, out col);
                GetNextRow("A"+row.ToString(), 2, out newcol, out newrow);
                newhomecell = newcol + newrow.ToString();
            }

            return newhomecell;
        }

        private void ColAutofit(Range currnetrange=null)
        {
            used_range = currnetrange != null ? currnetrange : worksheet.UsedRange;
            string ur = used_range.Address;
            string[] range = ur.Split(':');

            int beginCol;
            int endCol;
            int temp;
            GetCellCoordinates(range[0], out temp, out beginCol);
            GetCellCoordinates(range[1], out temp, out endCol);

            for (int i = beginCol; i < endCol; i++)
            {
                worksheet.Columns[i].AutoFit();
            }
        }

        private void RowAutofit()
        { }

        //move across and return address of that cell
        private void GetNextCol(string currentcell, int moveright, out string newcol, out int newrow)
        {
            char[] numbrs = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };
            int indexOfNum = currentcell.IndexOfAny(numbrs);
            newcol = currentcell.Substring(0, indexOfNum); 
            char c = newcol.ToCharArray()[0]; 
            int newc = (int)c + moveright;
            newcol = ((char)newc).ToString();

            string rownum = currentcell.Substring(indexOfNum); 
            if (!Int32.TryParse(rownum, out newrow))
            { 
                newrow = 0; 
            }
            
        }

        //move down and return address of that cell
        private void GetNextRow(string currentcell, int movedown, out string newcol, out int newrow)
        {
            char[] numbrs = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };
            int indexOfNum = currentcell.IndexOfAny(numbrs);
            newcol = currentcell.Substring(0, indexOfNum); 


            string rownum = currentcell.Substring(indexOfNum);
            if (!Int32.TryParse(rownum, out newrow)) 
            { 
                newrow = 0; 
            }
            else
                newrow = newrow + movedown; 
            
        }

        //get cell name (eg.. F11) 
        private void GetCellName(string currentcell, int rowsdown, int colsright, out string newcolname, out int newrownumber)
        {
            string newcellname = string.Empty;
            string tempcol;
            int temprow;
            GetNextCol(currentcell, colsright, out newcolname, out temprow); 
            GetNextRow(currentcell, rowsdown, out tempcol, out newrownumber);
        }

        //Get Range 
        private string GetStringRange(string colname, int rownumber, int dataArrRowCount, int dataArrColCount)
        {
            char startcolname =  colname.ToCharArray()[0];
            int offset = rownumber;
            int topleft = offset + 0;
            int bottright = offset + dataArrRowCount - 1;

            int last = (startcolname-65)+ dataArrColCount - 1;
            string endcol = GetExcelColumnName(last);

            string startcol = startcolname.ToString();

            string range = startcol + topleft + ":" + endcol + bottright;
            return range;
        }

        private string GetExcelColumnName(int columnNumber)
        {
            int dividend = columnNumber+1;
            string columnName = String.Empty;
            int modulo;

            while (dividend > 0)
            {
                modulo = (dividend - 1) % 26;
                columnName = Convert.ToChar(65 + modulo).ToString() + columnName;
                dividend = (int)((dividend - modulo) / 26);
            }

            return columnName;
        }

        //Merge Range of cells into one
        private void MergeCells(Range range, string str)
        {
            range.Merge();
            if(str!=null && str.Trim().Length > 0)
                range.Cells[1]=str;
            range.HorizontalAlignment = XlHAlign.xlHAlignCenter;
            range.VerticalAlignment = XlVAlign.xlVAlignCenter;
            range.Font.Bold = true;
            range.BorderAround();
        }

        //Merge and set text left aligned
        private void MergeCells2(Range range, string str, bool isLeftAligned=true, bool textwrap=true, bool isBold=false)
        {
            range.Merge();
            if (str != null && str.Trim().Length > 0)
                range.Cells[1] = str;
            if (isLeftAligned)
            {
                range.HorizontalAlignment = XlHAlign.xlHAlignLeft;
                range.VerticalAlignment = XlVAlign.xlVAlignCenter;
            }
            else
            {
                range.HorizontalAlignment = XlHAlign.xlHAlignCenter;
                range.VerticalAlignment = XlVAlign.xlVAlignCenter;
            }
            range.WrapText = textwrap;
            range.Font.Bold = isBold;
            if(str.Contains('\n'))
                range.RowHeight = str.Split('\n').Length * 15;
            range.BorderAround();
        }

        //Put Border around the given range
        private void BorderAroundRange(Range range)
        {
            range.BorderAround();
        }

        private string GetRangeData(Range alldata)
        {
            string mesg = "";
            try
            {
                for (int r = 1; r <= alldata.Rows.Count; r++)
                {
                    for (int c = 1; c <= alldata.Columns.Count; c++)
                    {
                        mesg = mesg + ":::" + (string)(alldata.Cells[r, c] as Microsoft.Office.Interop.Excel.Range).Value2 ;
                    }
                    mesg = mesg + "\n";
                }
            }
            catch (Exception ex)
            { }
            return mesg;
        }

        //merge adjacent cell 
        private void MergeAdjacent(Range alldata)
        {
            string currentcelldata = "";
            string rightcelldata = "";
            string downcelldata = "";
            int RangeTotalRows = alldata.Rows.Count;
            int RangeTotalCols = alldata.Columns.Count;

            int[,] mergestatus = new int[RangeTotalRows, RangeTotalCols];
            for (int i = 0; i < RangeTotalRows; i++)
                for (int j = 0; j < RangeTotalCols; j++)
                    mergestatus[i, j] = 1;
            try
            {
                int r, c;
                for (int rcur = 1; rcur <=RangeTotalRows ; rcur++)
                {
                    for (int ccur = 1; ccur <= RangeTotalCols ; ccur++)
                    {
                        r = rcur;
                        c = ccur;

                        if (mergestatus[r - 1, c - 1] == 0)
                            continue;

                        currentcelldata = (string)(alldata.Cells[r, c] as Microsoft.Office.Interop.Excel.Range).Value2;
                        if (currentcelldata == null)
                            continue;
                        while (c < RangeTotalCols)
                        {
                            c++;

                            rightcelldata = (string)(alldata.Cells[r, c] as Microsoft.Office.Interop.Excel.Range).Value2;
                            if (rightcelldata == null)
                            {
                                c--;
                                break;
                            }
                            if (! rightcelldata.Trim().Equals(currentcelldata))
                            {
                                c--; 
                                break;
                            }
                            (alldata.Cells[r, c] as Microsoft.Office.Interop.Excel.Range).Value2 = null;
                            mergestatus[r - 1, c - 1] = 0;
                        }
                        
                        while (r < RangeTotalRows)
                        {
                            r++;
                            downcelldata = (string)(alldata.Cells[r, c] as Microsoft.Office.Interop.Excel.Range).Value2;
                            if (downcelldata == null)
                            {
                                r--;
                                break;
                            }
                            if (!downcelldata.Trim().Equals(currentcelldata))
                            {
                                r--; 
                                break;
                            }
                            (alldata.Cells[r, c] as Microsoft.Office.Interop.Excel.Range).Value2 = null;
                            mergestatus[r - 1, c - 1] = 0;
                        }
                        
                        string address_r1 = alldata.Cells[rcur, ccur].Address;
                        string address_r2 = alldata.Cells[r, c].Address;

                        subrange = worksheet.get_Range(address_r1, address_r2);
                        MergeCells(subrange, currentcelldata); 

                    }
                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex.StackTrace);
                System.Console.Beep();
            }
             
        }
       
        //Fill Data in cells 
        private void FillDataInRange(Range range, string[,] data, bool textwrap = false)
        {
            //Filling Numeric or text data
            double[,] doubledata=null;
            double resdouble;

            bool isdouble = true;// assuming data is double type
            for (int dr = 0; dr < data.GetLength(0); dr++)
            {
                for (int dc = 0; dc < data.GetLength(1); dc++)
                {
                    if (data[dr, dc] != null)
                        if (!double.TryParse(data[dr, dc], out resdouble))//if data is of type double then convert all to double
                        {
                            isdouble = false;
                            break;
                        }
                }
                if (!isdouble) 
                    break;
            }
            if (isdouble)
            {
                doubledata = ConverStringArrToDoubleArr(data);
            }
            if (doubledata != null)
            {
                range.Value = doubledata;
            }
            else
            {
                range.Value = data;
            }

            /////// Setting Col Width //////
            double max = 0;
            int maxcolInRange = 0;
            for(int i=0;i<data.GetLength(0);i++)
                for (int j = 0; j < data.GetLength(1); j++)
                {
                    if (data[i,j]!=null && max < data[i,j].Length)
                    {
                        max = data[i, j].Length;
                        maxcolInRange = j;
                    }
                }

            double datamaxwidth = max * .8;
            double currwidth=8;
            if(range.ColumnWidth.GetType().FullName=="System.Double")
                currwidth = range.ColumnWidth; 
            if (textwrap)
            {
                range.WrapText = textwrap;
                range.ColumnWidth = 16;
            }

        }

        //Fill Data in cells
        private void FillDataInRange2(Range range, string[,] data, bool textwrap = false)
        {
            //variable that has converted value
            double resdouble;

            for (int dr = 0; dr < data.GetLength(0); dr++)
            {
                for (int dc = 0; dc < data.GetLength(1); dc++)
                {
                    if (data[dr, dc] != null)
                        if (double.TryParse(data[dr, dc], out resdouble))//if data is of type double then convert all to double
                        {
                            range[dr + 1, dc + 1] = double.Parse(data[dr, dc]);

                        }
                        else
                        {
                            range[dr + 1, dc + 1] = data[dr, dc];
                        }
                }
            }

            /////// Setting Col Width //////
            double max = 0;
            int maxcolInRange = 0;
            for (int i = 0; i < data.GetLength(0); i++)
                for (int j = 0; j < data.GetLength(1); j++)
                {
                    if (data[i, j] != null && max < data[i, j].Length)
                    {
                        max = data[i, j].Length;
                        maxcolInRange = j;
                    }
                }

            double datamaxwidth = max * .8;
            double currwidth = 8;
            if (range.ColumnWidth.GetType().FullName == "System.Double")
                currwidth = range.ColumnWidth; 

            if (textwrap)
            {
                range.WrapText = textwrap;
                range.ColumnWidth = 16;
            }

        }


        //Fill Subscript in Range
        private void FillSuperscriptInRange(Range range, string[,] data, bool textwrap = false)
        {
            Range r;
            string rangeaddress = range.get_Address();
            int colonidx = rangeaddress.IndexOf(":");
            string currentcell = rangeaddress.Substring(0, colonidx).Replace("$","");
            string newcolname;
            int newrownumber;
            string newcellname;

            for (int ri = 0; ri < data.GetLength(0); ri++) // rows
            {
                for (int ci = 0; ci < data.GetLength(1); ci++) //cols
                {
                    if (data[ri, ci] != null && (data[ri,ci].Trim().Length > 0) && !data[ri, ci].Equals("0") )
                    {
                        range[ri + 1, ci + 1] = data[ri, ci];

                        GetCellName(currentcell, ri, ci, out newcolname, out newrownumber);
                        newcellname = newcolname + newrownumber.ToString();

                        //Go to Particular cell and make it superscript
                        r = worksheet.Range[newcellname];
                        r.get_Characters(0, 1).Font.Superscript = true;
                        r.HorizontalAlignment = XlHAlign.xlHAlignCenter;
                        r.VerticalAlignment = XlVAlign.xlVAlignCenter;
                        releaseObject(r);
                        r = null;
                    }
                }
            }
        }

        //Conver String 2D array to Double 2D array
        private double[,] ConverStringArrToDoubleArr(string[,] data)
        {
            bool isconverted;
            int r = data.GetLength(0);
            int c = data.GetLength(1);
            double[,] darr = new double[r, c];
            for (int i = 0; i < r; i++)
            {
                for (int j = 0; j < c; j++)
                {
                    if (data[i, j] != null)
                    {
                        isconverted = double.TryParse(data[i, j], out darr[i, j]);

                        if (!isconverted)
                        {
                            darr[i, j] = 0;
                        }
                    }
                }
            }
            return darr;
        }
       
        // say pass B20 it will set row = 20, col = 2 (for B)
        private void GetCellCoordinates(string cellname, out int row, out int col)//, bool nonnumeric=false
        {
            if (cellname.Contains("$"))
            {
                cellname = cellname.Replace("$", "");
            }
            char[] numbrs = {'0','1','2','3','4','5','6','7','8','9'};
            int indexOfNum = cellname.IndexOfAny(numbrs);
            string colname = cellname.Substring(0, indexOfNum);

            col = GetColNum(colname);

            string rownum = cellname.Substring(indexOfNum);
            if(!Int32.TryParse(rownum, out row))
            { row = 0; }


        }

        // say pass B20 it will set row = 20, col =  "B"
        private void GetCellCoordinates2(string cellname, out int row, out string col)
        {
            if (cellname.Contains("$"))
            {
                cellname = cellname.Replace("$", "");
            }
            char[] numbrs = {'0','1','2','3','4','5','6','7','8','9'};
            int indexOfNum = cellname.IndexOfAny(numbrs);
            col = cellname.Substring(0, indexOfNum);

            string rownum = cellname.Substring(indexOfNum);
            if(!Int32.TryParse(rownum, out row))
            { row = 0; }
        }
    
        private int GetColNum(string colname)
        {
            int index=0;
            string[] ExcelColnames = {"A","B","C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" };
            for (int i = 0; i < ExcelColnames.Length; i++)
            {
                if (colname.ToUpper().Trim().Equals(ExcelColnames[i]))
                {
                    index = i + 1; // excel index is not zero based
                    break;
                }
            }
            return index;
        }
        #endregion


        # region allocating and freeing up resource

        //initializing several objects to open excel app, workbook, sheet etc..
        private void OpenCOMObject() 
        {
            if (app != null && app.Visible == false)
            {
                CloseCOMObject(false);
            }
            if (app == null)
            {
                app = new Microsoft.Office.Interop.Excel.Application();
                app.DisplayAlerts = false;
            }
            if (workbooks == null)
            {
                workbooks = app.Workbooks;
            }
            if (workbook == null)
                workbook = workbooks.Add(Type.Missing);

            if (worksheet == null)
            {
                worksheet = workbook.Sheets["Sheet1"]; // default sheetname
                worksheet = workbook.ActiveSheet;
            } 
        }

        private void ReleaseRange()
        {
            releaseObject(ewrange); ewrange = null;
            releaseObject(titlerow); titlerow = null;
            releaseObject(tlr); tlr = null;
            releaseObject(chr); chr = null;
            releaseObject(rhr); rhr = null;
            releaseObject(dr); dr = null;
            releaseObject(fnrange); fnrange = null;

            releaseObject(range); range = null;

            releaseObject(nrange); nrange = null;
            releaseObject(nrhr); nrhr = null;

            releaseObject(usedrange); usedrange = null;
            releaseObject(used_range); used_range = null;

            releaseObject(subrange); subrange = null;
        }

        //Freeing up resources allocated by app, workbook and sheets etc..
        private void CloseCOMObject(bool quit)
        {
            if (quit)
            {
                // save the application
                workbook.SaveAs("bskytest.xls",Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing,Microsoft.Office.Interop.Excel.XlSaveAsAccessMode.xlExclusive , Type.Missing, Type.Missing, Type.Missing, Type.Missing);

                workbook.Close(true, null, null);
                app.Quit();
            }

            ReleaseRange();

            releaseObject(worksheet); worksheet = null;
            releaseObject(workbook); workbook = null;
            releaseObject(workbooks); workbooks = null;
            releaseObject(app); app = null;
        }

        private void releaseObject(object obj)
        {
            try
            {
                System.Runtime.InteropServices.Marshal.FinalReleaseComObject(obj);
                obj = null;
            }
            catch (Exception ex)
            {
                obj = null;
            }
            finally
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
        } 
        #endregion
    }
}
