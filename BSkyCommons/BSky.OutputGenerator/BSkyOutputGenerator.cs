using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media.Imaging;
using C1.WPF.FlexGrid;
using System.Text.RegularExpressions;
using System.IO;
using System.Xml;
using BSky.Controls;
using BSky.Interfaces.Commands;
using BSky.Controls.Controls;
using ICSharpCode.SharpZipLib.Zip;
using System.Windows.Media;
using System;
using BSky.Lifetime;
using BSky.Lifetime.Interfaces;
using BSky.ConfService.Intf.Interfaces;

namespace BSky.OutputGenerator
{
    public class BSkyOutputGenerator
    {
        ILoggerService logService = LifetimeService.Instance.Container.Resolve<ILoggerService>();//28Mar2018
        IConfigService confService = LifetimeService.Instance.Container.Resolve<IConfigService>();//28Mar2018
        bool APA = false;
        /// Read .bsoz file and create all the displayable objects in allanalysis
        public List<SessionOutput> GenerateOutput(string fullpathzipfilename)//12Sep2012
        {
            if (!File.Exists(fullpathzipfilename))
            {
                MessageBox.Show(BSky.GlobalResources.Properties.Resources.BSkyOutFileNotFound);
                return null;
            }
            string inputfile = fullpathzipfilename;
            string tempDir = System.IO.Path.GetTempPath();//C:\Users\Anil\AppData\Local\Temp\
            string filePath = System.IO.Path.GetDirectoryName(fullpathzipfilename);
            string fileNamewithoutExt = System.IO.Path.GetFileNameWithoutExtension(fullpathzipfilename);
            string outputDir = tempDir + fileNamewithoutExt + "\\";// @"\temp2\";

            if (!Directory.Exists(outputDir))
            {
                Directory.CreateDirectory(outputDir);
            }

            FileStream fileStreamIn=null;
            ZipInputStream zipInStream=null;
            ZipEntry entry=null;
            try
            {

                fileStreamIn = new FileStream(inputfile, FileMode.Open, FileAccess.Read);
                zipInStream = new ZipInputStream(fileStreamIn);
                entry = zipInStream.GetNextEntry();
            }
            catch (Exception ex)
            {
                MessageBox.Show(BSky.GlobalResources.Properties.Resources.BSkyOutFileBadFormat);
                return null;
            }
            //Extracting the files one by one
            while (entry != null)
            {
                FileStream fileStreamOut = new FileStream(outputDir + entry.Name, FileMode.Create, FileAccess.Write);
                int size;
                byte[] buffer = new byte[1024];
                do
                {
                    size = zipInStream.Read(buffer, 0, buffer.Length);
                    fileStreamOut.Write(buffer, 0, size);
                } while (size > 0);
                fileStreamOut.Close();
                entry = zipInStream.GetNextEntry();
            }
            zipInStream.Close();
            fileStreamIn.Close();
            string bsofilename = System.IO.Path.Combine(outputDir, fileNamewithoutExt + ".bso");
            return html2xml(bsofilename);
        }

        //AUGrid c1fg = null;
        /// Read .bso file and create all the displayable objects in allanalysis
        public List<SessionOutput> html2xml(string fullpathfilename)
        {
            #region read html and
            if (!File.Exists(fullpathfilename))
            {
                MessageBox.Show(BSky.GlobalResources.Properties.Resources.BSkyOutFileNotFound);
                return null;
            }
            string fulltext = File.ReadAllText(fullpathfilename);
            string s = Regex.Replace(fulltext, @"\0", "");
            //////////////Put <BSKYOUTPUT> check here //////////////
            if (Regex.Matches(s, "<bskyoutput>").Count < 1 || Regex.Matches(s, "</bskyoutput>").Count < 1)
            {
                MessageBox.Show(BSky.GlobalResources.Properties.Resources.BSkyOutFileBadFormat);
                return null;
            }

            string r = Regex.Replace(s, @"<head>", "");
            string t = r;

            /// fixing class attribute with quotes
            string patternh = @"\bclass=h[0-9]+";
            string u = Regex.Replace(t, patternh, " class='h'");
            string patternc = @"\bclass=c[0-9]+";
            string v = Regex.Replace(u, patternc, " class='c'");

            List<SessionOutput> allSessions = new List<SessionOutput>();
            XmlDocument sessXd = new XmlDocument();
            //New Session tag which is parent of bskyanalysis tag
            ////// No of sessions/// ana is abbrivation for analysis
            MatchCollection sessmc = Regex.Matches(v, "<sessoutput");
            MatchCollection sessmc2 = Regex.Matches(v, "</sessoutput>");
            for (int ss = 0; ss < sessmc.Count; ss++)///loop on number of sessions
            {                                      ///
                int sessStart = sessmc[ss].Index;
                int sessEnd = sessmc2[ss].Index;
                string onesession = v.Substring(sessStart, sessEnd - sessStart + 14);
            ////// No of Analysis/// ana is abbrivation for analysis
                MatchCollection anamc = Regex.Matches(onesession, "<bskyanalysis>");
                MatchCollection anamc2 = Regex.Matches(onesession, "</bskyanalysis>");

                sessXd.LoadXml(onesession);
                string sessionheader = sessXd.SelectSingleNode("sessoutput").Attributes["Header"] != null ? sessXd.SelectSingleNode("sessoutput").Attributes["Header"].Value : string.Empty;
                string isRsession    = sessXd.SelectSingleNode("sessoutput").Attributes["isRsession"] != null ? sessXd.SelectSingleNode("sessoutput").Attributes["isRsession"].Value : "false";
                bool isrsessionoutput = false;
                if (isRsession.ToLower().Trim().Equals("true"))
                {
                    isrsessionoutput = true;
                }
            int anaStart = 0; int anaEnd = 0;
            XmlDocument anaXd = new XmlDocument();
            CommandOutput lst = null;
            SessionOutput allanalysis = new SessionOutput();
            allanalysis.NameOfSession = sessionheader;
            allanalysis.isRSessionOutput = isrsessionoutput;

            AUParagraph AUP;//09Jul2013
            for (int j = 0; j < anamc.Count; j++)///loop on number of analysis
            {
                lst = new CommandOutput();///for treeview. New analysis
                anaStart = anamc[j].Index;
                anaEnd = anamc2[j].Index;
                string oneAnalysis = onesession.Substring(anaStart, anaEnd - anaStart + 15);
                anaXd.LoadXml(oneAnalysis);    

                lst.NameOfAnalysis = anaXd.SelectSingleNode("bskyanalysis/analysisname").InnerText.Trim();//For Parent Node name 02Aug2012

                int noofaup = anaXd.SelectNodes("bskyanalysis/aup").Count;// should be 2
                int leafcount = anaXd.ChildNodes.Item(0).ChildNodes.Count; //29Oct2013 no. of leaves in an analysis
                string[] contTyp = { "Header", "Dataset" }; // should be 2
                int noofnotes = anaXd.SelectNodes("bskyanalysis/bskynotes").Count;//06nov2012

                foreach (XmlNode ln in anaXd.ChildNodes.Item(0).ChildNodes)
                {
                    if (ln.Name.Trim().Equals("analysisname"))
                        continue;
                    XmlNode xn = null;
                    switch (ln.Name.Trim())
                    {

                        case "aup":
                            #region AUPara
                            AUP = createAUPara(ln, "", ""); 
                            if (AUP != null)
                                lst.Add(AUP);

                            #endregion
                            break;
                        case "bskynotes":
                            #region BSkyNotes
                            if (noofnotes >= 1)
                            {
                                BSkyNotes bsnotes = createBSkyNotes(ln, "", "Notes"); 
                                if (bsnotes != null)
                                    lst.Add(bsnotes);
                            }
                            #endregion
                            break;
                        case "graphic":
                            #region Graphic
                                string tempDir = System.IO.Path.GetDirectoryName(fullpathfilename);
                                string fullpathimgfilename = string.Empty;
                                XmlNode imgxn = ln;//anaXd.SelectSingleNode("bskyanalysis/graphic");
                                string pathimgfilename = (imgxn != null) ? imgxn.InnerText.Trim() : "";

                                string imgfileNamewithExt = System.IO.Path.GetFileName(pathimgfilename);
                                string outputDir = tempDir + "\\";// @"\temp2\";
                                fullpathimgfilename = System.IO.Path.Combine(outputDir + imgfileNamewithExt);

                                if (fullpathimgfilename.Trim().Length > 0)
                                {
                                    lst.Add(CreateBSkyGraphicControl(fullpathimgfilename));
                                }
                            #endregion
                            break;
                        case "auxgrid":
                            #region FlexGrid

                            oneAnalysis = ln.OuterXml;
                            ////// No. of Grids in one analysis: for C1FlexGrid header/footer err/warning generation //////
                            MatchCollection aux1 = Regex.Matches(oneAnalysis, "<auxgrid>");
                            MatchCollection aux2 = Regex.Matches(oneAnalysis, "</auxgrid>");
                            XmlDocument anagrid = new XmlDocument();//this will be used to get contents of auxgrid(excluding flexgrid)

                            ////// No. of FlexGrids in one analysis: for C1FlexGrid generation //////
                            MatchCollection mc = Regex.Matches(oneAnalysis, "<html>");
                            MatchCollection mc2 = Regex.Matches(oneAnalysis, "</html>");
                            int start = 0; int end = 0;
                            XmlDocument xd = new XmlDocument();
                            for (int k = 0; k < mc.Count; k++) /// loop till all C1FlexGrid in current Analysis get generated
                            {
                                AUXGrid xgrid = new AUXGrid();
                                AUGrid c1fg = xgrid.Grid;// new C1flexgrid.
                                
                                anagrid.LoadXml(oneAnalysis.Substring(aux1[k].Index, aux2[k].Index - aux1[k].Index + 10));
                                //flexgrid header
                                xn = anagrid.SelectSingleNode("auxgrid/fgheader[1]");
                                xgrid.Header.Text = (xn != null) ? xn.InnerText : "";
                                ///AUXGrid  error/warning
                                Dictionary<char, string> Metadata = new Dictionary<char, string>();
                                char ch = ' '; int erri = 2;
                                string mesg = "", fullmsg = "";
                                xn = anagrid.SelectSingleNode("auxgrid/errm[1]");
                                while (xn != null)
                                {
                                    /// err msg is like :-   "a:. Warning: .... 
                                    fullmsg = xn.InnerText.ToString().Replace('"', ' ').Trim();
                                    ch = fullmsg.Substring(0, xn.InnerText.ToString().IndexOf(':')).ToCharArray()[0];///extract key char.
                                    mesg = fullmsg.Substring(fullmsg.IndexOf(':') + 1);
                                    Metadata.Add(ch, mesg);
                                    xn = anagrid.SelectSingleNode("auxgrid/errm[" + erri + "]");
                                    erri++; //error index. next line.
                                }
                                xgrid.Metadata = Metadata;
                                    /////// AUXGrid Footer /////

                                    bool templatedDialog = false;
                                    if (templatedDialog)
                                    {
                                        Dictionary<char, string> Footer = new Dictionary<char, string>();

                                        erri = 2;
                                        xn = anagrid.SelectSingleNode("auxgrid/footermsg[1]");
                                        while (xn != null)
                                        {
                                            /// err msg is like :-   "a:. Warning: .... 
                                            fullmsg = xn.InnerText.ToString().Replace('"', ' ').Trim();
                                            ch = fullmsg.Substring(0, xn.InnerText.ToString().IndexOf(':')).ToCharArray()[0];///extract key char.
                                            mesg = fullmsg.Substring(fullmsg.IndexOf(':') + 1);
                                            Footer.Add(ch, mesg);
                                            xn = anagrid.SelectSingleNode("auxgrid/errm[" + erri + "]");
                                            erri++; //error index. next line.
                                        }
                                        xgrid.FootNotes = Footer;
                                    }
                                    else
                                    {
                                        //This works for non-templated dialogs
                                        xn = anagrid.SelectSingleNode("auxgrid/footermsg[1]");
                                        if (xn != null)
                                        {
                                            fullmsg = xn.InnerText.ToString().Replace('"', ' ').Trim();
                                            xgrid.StarFootNotes = fullmsg;
                                        }
                                    }



                                ////////////get index of <html> and </html> //////
                                start = mc[k].Index;
                                end = mc2[k].Index;

                                //// create xmldoc loaind string from <html> to </html> ////
                                xd.LoadXml(oneAnalysis.Substring(start, end - start + 7));

                                html2flex(xd, c1fg);//create rows/cols/headers and populate
                                ///////////// find C1Flexgrid Generate it //////E/////
                                xgrid.Margin = new Thickness(10);
                                lst.Add(xgrid);

                            }
                            #endregion
                            break;
                    }//switch
                }//for each leave

                allanalysis.Add(lst);
            }
            allSessions.Add(allanalysis);
        }//for session
            #endregion

            return allSessions;
        }


        //for getting fresh APA style configuration
        private void RefreshAPAConfig()
        {
            string APAconfig = confService.GetConfigValueForKey("outTableInAPAStyle");
            //MessageBox.Show("APA Style : " + APAconfig;
            APA = (APAconfig.ToLower().Equals("true")) ? true : false;
        }
        /// using the xml doc xd [ <html>...</html> ] create row/col headers and datamatrix
        /// for the referenced flexgrid and then populate the grid
        public void html2flex(XmlDocument xd, AUGrid c1FlexGrid1)
        {
            RefreshAPAConfig();
            //AUXGrid xgrid = (AUXGrid)c1FlexGrid1.Tag;
            #region create Row, Col header and data matrix
            string[,] colheadermatrix = null;
            string[,] rowheadermatrix = null;
            string[,] datamatrix = null;
            {
                /////////generate col header////////
                #region
                XmlNode thead = xd.SelectSingleNode("/html/body/table/thead");
                if (thead == null)
                {
                    
                }
                int colheaderrows = thead.ChildNodes.Count;//no. of rows in colheader
                int colheadercols = 0;//no.cols in each row of colheader
                //////No of Cols in Colheader///////////
                XmlNode tr = xd.SelectSingleNode("/html/body/table/thead/tr");
                foreach (XmlNode th in tr.ChildNodes)
                {
                    //if (th.InnerText.Trim().Length > 0) colheadercols++;
                    colheadercols++;
                }
                if (colheadercols > 0 && colheaderrows > 0)
                {
                    ///////Colheader Matrix//////////
                    colheadermatrix = new string[colheaderrows, colheadercols];
                    ////fill Matrix////////
                    int row = 0, col;
                    foreach (XmlNode trr in thead.ChildNodes)// <tr>
                    {
                        col = 0;
                        foreach (XmlNode th in trr.ChildNodes)
                        {
                            if (th.InnerText.Trim().Length > 0)
                                colheadermatrix[row, col++] = th.InnerText.Trim();
                        }
                        row++;
                    }
                }
                #endregion
                /////////generate col header and data////////
                #region
                XmlNode tbody = xd.SelectSingleNode("/html/body/table/tbody");
                if (tbody == null)
                {
                    
                }
                int rowheaderrows = tbody.ChildNodes.Count;//no. of rows in rowheader. Also, No of data rows
                int rowheadercols = 0;//no.cols in each row of rowheader
                int datacols = 0;//no. of data columns
                //////No of Cols in Rowheader and data////////
                XmlNode tr2 = xd.SelectSingleNode("/html/body/table/tbody/tr");
                if (tr2 != null)
                {
                    foreach (XmlNode td in tr2.ChildNodes)//<td>
                    {
                        XmlAttributeCollection xac = td.Attributes;
                        foreach (XmlAttribute xa in xac)
                        {
                            if (xa.Name.Equals("class") && xa.Value.Equals("h")) //row header cols
                            { rowheadercols++; }
                            else if (xa.Name.Equals("class") && xa.Value.Equals("c"))// data col
                            { datacols++; }
                            else
                            { }
                        }
                    }
                }

                if (rowheadercols > 0 && rowheaderrows > 0)
                {
                    /////// Rowheader/Data Matrix//////////
                    rowheadermatrix = new string[rowheaderrows, rowheadercols];
                    datamatrix = new string[rowheaderrows, datacols];
                    ////fill Row header Matrix and Data Matrix////////
                    int row2 = 0, rhcol, dtcol;
                    foreach (XmlNode trr in tbody.ChildNodes)// <tr>
                    {
                        rhcol = 0; dtcol = 0;
                        foreach (XmlNode td in trr.ChildNodes)
                        {
                            if (rhcol < rowheadercols) // row header col of a row
                            {
                                rowheadermatrix[row2, rhcol++] = td.InnerText.Trim();
                            }
                            else 
                            {
                                datamatrix[row2, dtcol++] = td.InnerText.Trim();
                            }
                        }
                        row2++;
                    }
                }
                #endregion
            }
            #endregion

            #region populating C1flexGrid
            ////// Populate Row/Col Headers and Data //////

            //// creating hreaders ////

            ///////////// merge and sizing /////
            c1FlexGrid1.AllowMerging = AllowMerging.ColumnHeaders | AllowMerging.RowHeaders;
            c1FlexGrid1.AllowSorting = true;

            //trying to fix the size of the grid so that rendering does not take much time calculating these
            c1FlexGrid1.MaxHeight = 800;// NoOfRows* EachRowHeight;
            c1FlexGrid1.MaxWidth = 1000;

            var rowheaders = c1FlexGrid1.RowHeaders;
            var colheaders = c1FlexGrid1.ColumnHeaders;

            colheaders.Rows[0].AllowMerging = true; 
            colheaders.Rows[0].HorizontalAlignment = HorizontalAlignment.Center;

            rowheaders.Columns[0].AllowMerging = true; 
            rowheaders.Columns[0].VerticalAlignment = VerticalAlignment.Top;


            if (APA)
            {
                c1FlexGrid1.GridLinesVisibility = GridLinesVisibility.None;
                c1FlexGrid1.HeaderGridLinesBrush = Brushes.White;
                c1FlexGrid1.ColumnHeaderBackground = Brushes.White;
                c1FlexGrid1.RowHeaderBackground = Brushes.White;
                c1FlexGrid1.TopLeftCellBackground = Brushes.White;
                c1FlexGrid1.BorderBrush = Brushes.WhiteSmoke;
                c1FlexGrid1.Background = Brushes.White;
                c1FlexGrid1.RowBackground = Brushes.White;
                c1FlexGrid1.BorderThickness = new Thickness(0, 3, 0, 0);
                //find border of flexgrid and set it to APA style
                DependencyObject border = VisualTreeHelper.GetParent(VisualTreeHelper.GetParent(c1FlexGrid1));
                (border as Border).BorderThickness = new Thickness(0, 1, 0, 1);

            }
            else
            {

                c1FlexGrid1.ColumnHeaderBackground = Brushes.LightBlue;
                c1FlexGrid1.RowHeaderBackground = Brushes.LightBlue;
                c1FlexGrid1.TopLeftCellBackground = Brushes.LightBlue;

                c1FlexGrid1.BorderThickness = new Thickness(1);

            }

            /////////////Col Headers//////////
            if (colheadermatrix != null)
            {
                for (int i = colheaders.Rows.Count; i < colheadermatrix.GetLength(0); i++)
                {
                    C1.WPF.FlexGrid.Row row = new C1.WPF.FlexGrid.Row();
                    colheaders.Rows.Add(row);
                    row.AllowMerging = true;
                    row.HorizontalAlignment = HorizontalAlignment.Center;
                }
                for (int i = colheaders.Columns.Count; i < colheadermatrix.GetLength(1); i++) // creating col headers
                {
                    C1.WPF.FlexGrid.Column col = new C1.WPF.FlexGrid.Column();
                    colheaders.Columns.Add(col);
                    col.AllowMerging = true;

                    //for APA Style table
                    if (APA)
                    {

                    }
                }

                //fill col headers
                for (int i = 0; i < colheadermatrix.GetLength(0); i++)
                    for (int j = 0; j < colheadermatrix.GetLength(1); j++)
                    {
                        if (colheadermatrix[i, j] != null && colheadermatrix[i, j].Trim().Equals(".-."))
                            colheaders[i, j] = "";//14Jul2014 filling empty header
                        else
                            colheaders[i, j] = colheadermatrix[i, j];
                    }
            }
            /////////////Row Headers///////////
            if (rowheadermatrix != null)
            {
                for (int i = rowheaders.Columns.Count; i < rowheadermatrix.GetLength(1); i++)
                {
                    C1.WPF.FlexGrid.Column col = new C1.WPF.FlexGrid.Column();
                    col.AllowMerging = true; 
                    col.VerticalAlignment = VerticalAlignment.Top;
                    rowheaders.Columns.Add(col);
                }

                for (int i = rowheaders.Rows.Count; i < rowheadermatrix.GetLength(0); i++)
                {
                    C1.WPF.FlexGrid.Row row = new C1.WPF.FlexGrid.Row();
                    rowheaders.Rows.Add(row);
                    row.AllowMerging = true;

                    //for APA Style table
                    if (APA)
                    {

                    }
                }
            }
            //fill row headers
            if (IsEmptyOrNullArray(rowheadermatrix))//rowheader are empty or null
            {
                c1FlexGrid1.HeadersVisibility = HeadersVisibility.Column;
            }
            else
            {
                for (int i = 0; i < rowheadermatrix.GetLength(0); i++)
                    for (int j = 0; j < rowheadermatrix.GetLength(1); j++)
                    {
                        if (rowheadermatrix[i, j] != null && rowheadermatrix[i, j].Trim().Equals(".-."))
                            rowheaders[i, j] = "";//14Jul2014 filling empty header
                        else
                            rowheaders[i, j] = rowheadermatrix[i, j];
                    }
            }
            //Filling Data
            if (datamatrix != null)
            {
                bool isemptyrow;
                for (int rw = 0; rw < datamatrix.GetLength(0); rw++)
                {
                    isemptyrow = true;//assuming row is empty
                    for (int c = 0; c < datamatrix.GetLength(1); c++)
                    {
                        if (datamatrix[rw, c].Trim().Length > 0)
                        {
                            if(c1FlexGrid1.Columns.Count>c && c1FlexGrid1.Rows.Count > rw)
                            c1FlexGrid1[rw, c] = datamatrix[rw, c];
                            isemptyrow = false;// if it has atleast one column filled then row is not empty
                        }
                    }
                    //// hide or remove empty row////
                    if (isemptyrow)
                        c1FlexGrid1.Rows[rw].Visible = false;
                }
            }
            #endregion
        }

        private bool IsEmptyOrNullArray(string[,] arr)
        {
            bool isEmpty = true;
            if (arr == null)
                isEmpty=true;
            else
            {
                for(int i=0;i<arr.GetLength(0); i++)
                {
                    for(int j=0; j<arr.GetLength(1); j++)
                    {
                        if(arr[i,j]!=null && arr[i,j].Length>0)
                            isEmpty=false;
                    }
                }
            }
            return isEmpty;
        }

        public AUParagraph createAUPara(XmlNode xd, string selectNode, string controType)
        {
            XmlNode xn = null;
            AUParagraph AUP = new AUParagraph();
            xn = selectNode.Trim().Length > 0 ? xd.SelectSingleNode(selectNode) : xd;
            if (xn == null) return null; //09Jul2013
            AUP.Text = (xn != null) ? xn.InnerText.Trim() : "";
            AUP.FontSize = BSkyStyler.BSkyConstants.TEXT_FONTSIZE;//10Nov2014//12;//04Nov2014
            //Controltype
            if ((controType == null || controType.Trim().Length < 1))//03Aug2012
            {

                if (xn.Attributes["controltype"] != null && xn.Attributes["controltype"].Value.Length > 0)//29Oct2013
                {
                    controType = xn.Attributes["controltype"].Value;
                }
                else if (AUP.Text.Trim().Length > 0 && controType.Length < 1)
                {
                    int len = AUP.Text.IndexOf('\n', 1);//if \n not found
                    if (len < 1) len = AUP.Text.Length;
                    controType = AUP.Text.Substring(0, len).Replace("\n", string.Empty);
                }
                else
                    controType = "-";
            }

            //textcolor
            if (xn.Attributes["textcolor"] != null && xn.Attributes["textcolor"].Value.Length > 0)//29Oct2013
            {
                string hexstr = xn.Attributes["textcolor"].Value; //"#AARRGGBB"
                if (hexstr == null || hexstr.Length < 1) hexstr = "#FF000000"; // black color
                var color = (Color)ColorConverter.ConvertFromString(hexstr);
                AUP.textcolor = new SolidColorBrush(color);
            }
            //fontsize
            if (xn.Attributes["fontsize"] != null && xn.Attributes["fontsize"].Value.Length > 0)//29Oct2013
            {
                double fsize;
                if (double.TryParse(xn.Attributes["fontsize"].Value, out fsize))
                    AUP.FontSize = fsize;
                else
                    AUP.FontSize = 14;//set default in case something goes wrong
            }
            //fontweight
            if (xn.Attributes["fontweight"] != null && xn.Attributes["fontweight"].Value.Length > 0)//29Oct2013
            {
                string fWt = xn.Attributes["fontweight"].Value; // remove curly braces eg.. "{Bold}"
                FontWeight fw = (FontWeight)new FontWeightConverter().ConvertFromString(fWt);
                AUP.FontWeight = fw;
            }

            //fontsize added above so this is no needed //if (controType.Equals("Header")) AUP.FontSize = 16;
            AUP.ControlType = controType; //// Leaf node name in treeview
            return AUP;
        }

        // Creating graphic control from an image file //
        public BSkyGraphicControl CreateBSkyGraphicControl(string fullpathimgfilename)//11Sep2012
        {
            BSkyGraphicControl bsgc = new BSkyGraphicControl();
            bsgc.ControlType = "Graphic";
            if (!File.Exists(fullpathimgfilename))
            {
                fullpathimgfilename = "../libs/BlueSky/Images/ImgErr.png";//@".\Images\ImgErr.png"; //return bsgc;
                if (!File.Exists(fullpathimgfilename))
                {
                    return bsgc;
                }
            }
            //Setting image source//
            Image myImage = new Image();
            var bitmap = new BitmapImage();
            var stream = File.OpenRead(fullpathimgfilename);
            bitmap.BeginInit();
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.StreamSource = stream;
            bitmap.EndInit();
            stream.Close();
            stream.Dispose();
            myImage.Source = bitmap;
            bsgc.BSkyImageSource = myImage.Source;

            return bsgc;
        }

        public BSkyNotes createBSkyNotes(XmlNode xd, string selectNode, string controType)//06Nov2012
        {
            BSkyNotes bsn = new BSkyNotes();
            //09Jul2013 bsn.ControlType = controType;
            /// read xml info and populate bsn ///
            XmlNode xn = null;/// showrow  spliposi   notesheading notesrow  notescol
            xn = xd.SelectSingleNode("controltype");//(selectNode + "/controltype");//09Jul2013
            if (xn == null) return null;
            bsn.ControlType = (xn != null) ? xn.InnerText.Trim() : "-";

            xn = xd.SelectSingleNode("collapsetext");
            bsn.CollapsedText = (xn != null) ? xn.InnerText.Trim() : "";

            xn = xd.SelectSingleNode("showrow");
            bsn.ShowRow_Index = int.Parse((xn != null) ? xn.InnerText.Trim() : "1");

            xn = xd.SelectSingleNode("splitposi");
            bsn.NotesSplitPosition = uint.Parse((xn != null) ? xn.InnerText.Trim() : "1"); bsn.RightPart = 6; bsn.LeftPart = 1;

            xn = xd.SelectSingleNode("notesheading");
            bsn.HearderText = (xn != null) ? xn.InnerText.Trim() : "";

            XmlNodeList xnlrow = xd.SelectNodes("notesrow");
            XmlNodeList xnlcol = xd.SelectNodes("notesrow[1]/notescol");
            if (xnlrow == null || xnlcol == null || xnlcol.Count < 1 || xnlrow.Count < 1) 
                return null;
            int rowcount = xnlrow.Count;
            int colcount = xnlcol.Count;

            string[,] notesdata = new string[rowcount, colcount];
            for (int i = 0; i < rowcount; i++)
            {
                for (int j = 0; j < colcount; j++)
                {
                    xn = xd.SelectSingleNode("notesrow[" + (i + 1) + "]/notescol[" + (j + 1) + "]");
                    notesdata[i, j] = (xn != null) ? xn.InnerText.Trim() : "";
                }
            }
            bsn.NotesData = notesdata;

            bsn.FillData();
            return bsn;
        }
    }
}
