using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using C1.WPF.FlexGrid;
using System.Xml;
using BSky.Controls;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.IO;
using BSky.Controls.Controls;
using BSky.Lifetime.Interfaces;
using BSky.Lifetime;
using BSky.Controls.XmlDecoder.Model;
using BSky.Statistics.Common;
using System.Windows.Media;
using System.Globalization;
using BSky.Lifetime.Services;
using BSky.Controls.XmlDecoder;
using BSky.ConfService.Intf.Interfaces;
using BSky.ConfigService.Services;

namespace BSky.XmlDecoder
{
    public class OutputReader
    {
        ILoggerService logService = LifetimeService.Instance.Container.Resolve<ILoggerService>();//15Dec2012
        IConfigService confService = LifetimeService.Instance.Container.Resolve<IConfigService>();//15Dec2012

        public string Hdr
        {
            get;
            set;
        }

        bool AdvancedLogging;
        bool APA = false;
        /// <summary>
        /// Creates output from xml file
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public List<DependencyObject> GetOutput(string filename)
        {
            AdvancedLogging = AdvancedLoggingService.AdvLog;//01May2015
            List<DependencyObject> lst = new List<DependencyObject>();
            bool XMLexists = true;
            //Check later why this is here  OutputHelper.UpdateMacro("%TEST%", "mytest");

            Output output = new Output();
            XmlDocument doc = new XmlDocument();
            string hdr = Hdr != null ? Hdr : string.Empty;
            string blankTemplate = "<?xml version=\"1.0\" encoding=\"utf-8\"?>" +
            "<output> <output.header> <text stringformat=\"{0}\"> <statictext>" + hdr + "</statictext> </text> </output.header> </output>";
            try
            {
                if (filename != null && filename.Length > 0)//&& File.Exists(filename)
                {
                    if (File.Exists(filename))
                    {
                        doc.Load(filename); //loads output template .xml file if exists else goes to catch
                        if (AdvancedLogging) logService.WriteToLogLevel("ExtraLogs: Output Xml template file:" + filename, LogLevelEnum.Info);
                    }
                    else
                    {
                        if (AdvancedLogging) logService.WriteToLogLevel("ExtraLogs: Output Xml template file does not exist:" + filename, LogLevelEnum.Info);
                        XMLexists = false;
                        GetStatResult(lst);// for stat results, in case output template XML is does not exist
                        GetErrorsWarnings(lst);// Now adding error warning info to output
                        GetUserResult(lst);
                        return lst;
                    }
                }
                else
                {
                    doc.LoadXml(blankTemplate);//08Jun2013 Minimum template works if filename is not provided at all (is null)
                    if (AdvancedLogging) logService.WriteToLogLevel("ExtraLogs: blank templated xml:" + blankTemplate, LogLevelEnum.Info);
                }
                object clas = this; string s = clas.GetType().FullName;
            }
            catch (Exception e)
            {
                //for testing. MessageBox.Show(e.Message); //show the name of template file thats probable wrong name or missing in config
                if (filename != null && filename.Length > 0) // XML template has errors
                {
                    //18Aug2014 Supressing this message box as we dont need it. But we still pass message in log.
                    //MessageBox.Show("Output Xml template not correctly formatted.");
                    if (File.Exists(filename))
                    {
                        logService.WriteToLogLevel(filename + " Output Xml template not correctly formatted. ", LogLevelEnum.Error);
                    }
                    else
                    {
                        logService.WriteToLogLevel(filename + " Output Xml template does not exist. ", LogLevelEnum.Error);
                    }
                }

                XMLexists = false;
                GetStatResult(lst);// for stat results, in case output template XML is does not exist
                GetErrorsWarnings(lst);// Now adding error warning info to output
                GetUserResult(lst);
                return lst;
            }

            if (XMLexists)
            {
                if (AdvancedLogging) logService.WriteToLogLevel("ExtraLogs: XML exists:", LogLevelEnum.Info);
                //Initialize the data model for output file.
                output.Initialize(doc.SelectSingleNode(NodeNames.OUTPUT_NODE)); 


                //moved to the begining 22Aug2013 List<DependencyObject> lst = new List<DependencyObject>();
                if (output.Header.Text != null && output.Header.Text.Trim().Length > 0)
                {
                    if (AdvancedLogging) logService.WriteToLogLevel("ExtraLogs: XML has Header define:", LogLevelEnum.Info);
                    string rcommcol = confService.GetConfigValueForKey("dctitlecol");//23nov2012
                    byte red = byte.Parse(rcommcol.Substring(3, 2), NumberStyles.HexNumber);
                    byte green = byte.Parse(rcommcol.Substring(5, 2), NumberStyles.HexNumber);
                    byte blue = byte.Parse(rcommcol.Substring(7, 2), NumberStyles.HexNumber);
                    Color c = Color.FromArgb(255, red, green, blue);
                    //Add headers 
                    AUParagraph Title = new AUParagraph();
                    Title.Text = output.Header.Text.Trim();
                    Title.FontSize = BSkyStyler.BSkyConstants.HEADER_FONTSIZE;
                    Title.FontWeight = FontWeights.DemiBold;
                    Title.textcolor = new SolidColorBrush(c);

                    Title.ControlType = "Header";
                    lst.Add(Title);
                }
                ////// NOTES /////
                object notesdataobj = OutputHelper.GetNotes();
                if (notesdataobj != null)
                {
                    BSkyNotes bsn = new BSkyNotes();
                    bsn.NotesData = (string[,])notesdataobj;//23Oct2012 uasummary
                    bsn.NotesSplitPosition = 1; bsn.RightPart = 6; bsn.LeftPart = 1;
                    bsn.HearderText = "Notes";//  header tex shown in notes grid
                    bsn.CollapsedText = "Notes.";
                    bsn.ShowRow_Index = -1; 
                    bsn.FillData();

                    bsn.ControlType = "Notes";

                    lst.Add(bsn);
                }
                if (OutputHelper.AnalyticsData != null)
                {
                    string dsname = null;
                    string dsnameinapp = null;
                    if (OutputHelper.AnalyticsData.AnalysisType.Contains("BSkyloadDataset(") //30Oct2013 for putting Dataset even when we are in the middle of opening it
                        || OutputHelper.AnalyticsData.AnalysisType.Contains("BSkysaveDataset(")  //01Aug2016 for sending command to output window
                        || OutputHelper.AnalyticsData.AnalysisType.Contains("BSkycloseDataset("))//01Aug2016 for sending command to output window
                    {
                        UAReturn uar = OutputHelper.AnalyticsData.Result;
                        dsname = uar != null && uar.Datasource != null ? uar.Datasource.FileNameWithPath : "";
                        dsnameinapp = uar != null && uar.Datasource != null ? uar.Datasource.Name : "";
                    }
                    else
                    {
                        dsname = OutputHelper.AnalyticsData.DataSource != null ? OutputHelper.AnalyticsData.DataSource.FileName : "";//dietstudy.sav, satisf.sav etc.. with path
                        dsnameinapp = OutputHelper.AnalyticsData.DataSource != null ? OutputHelper.AnalyticsData.DataSource.Name : "";//Dataset1, Dataset2 etc..
                    }
                    if (dsname != null && dsname.Trim().Length > 0)
                    {
                        //31May2015 Show dataset open command also
                        AUParagraph datasetcomm = new AUParagraph();
                        datasetcomm.FontSize = BSkyStyler.BSkyConstants.TEXT_FONTSIZE;//10Nov2014//13;
                        datasetcomm.Text = OutputHelper.AnalyticsData.AnalysisType;
                        datasetcomm.ControlType = "Command";
                        lst.Add(datasetcomm);

                        AUParagraph dataset = new AUParagraph();
                        dataset.FontSize = BSkyStyler.BSkyConstants.TEXT_FONTSIZE;//10Nov2014//13;
                        dataset.Text = "[" + dsnameinapp + "] - " + dsname;
                        dataset.ControlType = "DataSet";
                        lst.Add(dataset);
                    }
                }

                if (output != null && output.TableList != null)//05Sep2012 only 'if' condition introduced for code below
                {
                    OutputHelper.TotalOutputTables = output.TableList.Values.Count;
                    if (OutputHelper.TotalOutputTables > 0)// 04Jun2013 for analytic R function calls
                    {
                        if (AdvancedLogging) logService.WriteToLogLevel("ExtraLogs: Total tables to create:"+OutputHelper.TotalOutputTables, LogLevelEnum.Info);
                        bool myrun = false;
                        if (OutputHelper.GetGlobalMacro(string.Format("GLOBAL.{0}.SPLIT", OutputHelper.AnalyticsData.DataSource.Name), "") != null)//OutputHelper.GetGlobalMacro(string.Format("GLOBAL.{0}.SPLIT",OutputHelper.AnalyticsData.DataSource.Name), "Showgroups") == "TRUE")
                        {
                            List<string> splitVars = OutputHelper.GetList(string.Format("GLOBAL.{0}.SPLIT.SelectedVars", OutputHelper.AnalyticsData.DataSource.Name), "", false);//List<string> splitVars = new List<string>();// 
                            int i = 1;
                            CreateSplitIteration(output, ref i, new List<string>(), splitVars, 0, lst); /// if there is split output creation is diff
                        }
                        else
                        {
                            int i = 1;
                            CreateTables(output, ref i, lst); /// non-split output generation
                        }
                    }
                }
                if (output.isGraphic)//05Sep2012
                {
                    string source = confService.AppSettings.Get("tempimage");
                    // load default value if no path is set or invalid path is set
                    if (source.Trim().Length == 0)
                        source = confService.DefaultSettings["tempimage"];

                    string sourcepath = Path.Combine(BSkyAppDir.RoamingUserBSkyTempPath, source);
                    GetGraphic(lst, sourcepath);
                }
            }
            ///25Jun2013 creating UI for erros and warning returned in the return structure
            GetErrorsWarnings(lst);

            ///25Jun2013 creating UI for user Rsults added at the the end of the return structure
            GetUserResult(lst);

            return lst;
        }

        /// <summary>
        /// Creates split iterations
        /// </summary>
        /// <param name="output"></param>
        /// <param name="tnumber"></param>
        /// <param name="vars"></param>
        /// <param name="lst"></param>
        private void CreateSplitIteration(Output output, ref int tnumber, List<string> currentvars, List<string> vars, int varindex, List<DependencyObject> lst)
        {
            if (varindex == vars.Count - 1)
            {
                List<string> factors = OutputHelper.GetFactors(vars[varindex]);
                string currentVar = string.Join(",", currentvars.ToArray());
                if (currentVar != null && currentVar.Trim().Length > 0)//if there vurrentvars is non-empty then add a comma.
                    currentVar = currentVar + ", ";
                else
                    currentVar = string.Empty;
                foreach (string str in factors)
                {
                    AUParagraph ap = new AUParagraph();
                    ap.FontSize = BSkyStyler.BSkyConstants.HEADER_FONTSIZE3;//10Nov2014
                    ap.FontWeight = FontWeights.DemiBold;
                    ap.Text = "\n\n<<-- Split = True, "+currentVar + vars[varindex] + " = " + str+" -->>";
                    ap.ControlType = ap.Text;
                    lst.Add(ap);

                    CreateTables(output, ref tnumber, lst);
                }
            }
            else
            {
                List<string> factors = OutputHelper.GetFactors(vars[varindex]);
                foreach (string str in factors)
                {
                    List<string> tempVars = currentvars.ToList();
                    tempVars.Add(vars[varindex] + "=" + str);
                    CreateSplitIteration(output, ref tnumber, tempVars, vars, varindex + 1, lst);
                }
            }
        }

        private void CreateTables(Output output, ref int tablenumber, List<DependencyObject> lst)
        {
            string outstub = confService.AppSettings.Get("outputstub");
            // load default value if no value is set 
            if (outstub.Trim().Length == 0)
                outstub = confService.DefaultSettings["outputstub"];
            bool outputstub = outstub.Equals("true") ? true : false; /// for testing purpose only. Make it false while testing.
            //bool repeatall = true;
            TableRepeat repeat = null;
            foreach (Table table in output.TableList.Values)
            {
                var v = from r in output.Repeats            
                        where r.TableToRepeat == table.ID   
                        select r;                           
                repeat = v.FirstOrDefault();
                if (repeat == null)
                    continue; // test it properly
                break;
            }

            if (repeat == null) // in case of one sample repeat is null. Means no repeat
            {
                foreach (Table table in output.TableList.Values)
                {
                    lst.Add(CreateTable(table, tablenumber));
                    if (outputstub) FillTableData(lst.Last(), tablenumber);
                    tablenumber++;
                }
            }
            else if (output.repeatall == false) 
            {
                int tno = tablenumber;
                foreach (Table table in output.TableList.Values)
                {
                    var v = from r in output.Repeats
                            where r.TableToRepeat == table.ID
                            select r;
                    repeat = v.FirstOrDefault();
                    if (repeat == null)
                    {
                        lst.Add(CreateTable(table, tno));
                        if (outputstub)
                        {
                            FillTableData(lst.Last(), tno);
                            tno += output.TableList.Count;
                        }
                        continue;
                    }
                    bool ifRepeat = OutputHelper.Evaluate(repeat.Condition);//
                    if (ifRepeat)
                    {

                        List<string> varList = OutputHelper.GetList(repeat.RepeatOn, string.Empty, false);
                        if (AdvancedLogging) logService.WriteToLogLevel("ExtraLogs: Repeat var count:" + varList.Count, LogLevelEnum.Info);
                        foreach (string str in varList) // for each row variable
                        {
                            if (AdvancedLogging) logService.WriteToLogLevel("ExtraLogs: Repeat row var: '" + str+"'", LogLevelEnum.Info);
                            OutputHelper.UpdateMacro(repeat.VariableName, str);
                            if (repeat.InnerRepeats.Count > 0)
                            {
                                foreach (InnerRepeat innerRepeat in repeat.InnerRepeats) // for all, 'foreach' tag
                                {
                                    bool condition = OutputHelper.Evaluate(innerRepeat.Condition);
                                    if (condition)
                                    {
                                        List<string> labelList = OutputHelper.GetList(innerRepeat.RepeatOn, string.Empty, false);

                                        foreach (string str1 in labelList) // for each col variable
                                        {
                                            OutputHelper.UpdateMacro(innerRepeat.VariableName, str1);
                                            lst.Add(CreateTable(table, tno));// to ouput list 'lst' add a table after creating
                                            if (outputstub)
                                            {
                                                FillTableData(lst.Last(), tno);
                                                tno += output.TableList.Count;
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                lst.Add(CreateTable(table, tno));
                                if (outputstub)
                                {
                                    FillTableData(lst.Last(), tno);
                                    tno += output.TableList.Count;
                                }
                            }
                        }
                    }
                    tablenumber++;
                }
            }
            else if (output.repeatall)
            {

                foreach (Table table in output.TableList.Values)
                {
                    var v = from r in output.Repeats
                            where r.TableToRepeat == table.ID
                            select r;
                    repeat = v.FirstOrDefault();
                    if (repeat == null)
                        continue;
                    break;
                }

                bool ifRepeat = OutputHelper.Evaluate(repeat.Condition);// tabletorepeat should not be considered.
                if (ifRepeat)
                {
                    List<string> varList = OutputHelper.GetList(repeat.RepeatOn, string.Empty, false);
                    foreach (string str in varList) // for each row variable
                    {
                        OutputHelper.UpdateMacro(repeat.VariableName, str);
                        if (repeat.InnerRepeats.Count > 0)
                        {
                            foreach (InnerRepeat innerRepeat in repeat.InnerRepeats) // for all, 'foreach' tag
                            {
                                bool condition = OutputHelper.Evaluate(innerRepeat.Condition);
                                if (condition)
                                {
                                    List<string> labelList = OutputHelper.GetList(innerRepeat.RepeatOn, string.Empty, false);
                                    foreach (string str1 in labelList) // for each col variable
                                    {
                                        OutputHelper.UpdateMacro(innerRepeat.VariableName, str1);

                                        foreach (Table tabl in output.TableList.Values)
                                        {
                                            lst.Add(CreateTable(tabl, tablenumber));// to ouput list 'lst' add a table after creating
                                            if (outputstub) FillTableData(lst.Last(), tablenumber);
                                            tablenumber++; ;
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            foreach (Table tabl in output.TableList.Values)
                            {
                                lst.Add(CreateTable(tabl, tablenumber));// to ouput list 'lst' add a table after creating
                                if (outputstub) FillTableData(lst.Last(), tablenumber);
                                tablenumber++;
                            }
                        }
                    }
                }
            }
        }

        private void FillTableData(DependencyObject table, int datanumber)
        {
            AUXGrid xgrid = table as AUXGrid;
            AUGrid grid = xgrid.Grid;
            bool[] visibleRows; // its dependent on datarow. Not metadata
            string metadatatype = OutputHelper.findMetaDataType(datanumber); //get metadatatype "normal" "crosstab1"
            
            MetadataTable mt = OutputHelper.GetFullMetadataTable(datanumber, metadatatype);

            #region Flexgrid Max rows and Max Col setting from config options
            int customMaxCells = 10;//get it from options settings
            int customMaxCols = 10;//get it from options settings
            string maxgridcells = confService.GetConfigValueForKey("maxflexgridcells");//
            // load default value if no value is set or invalid value is set
            if (maxgridcells.Trim().Length != 0)
            {
                Int32.TryParse(maxgridcells, out customMaxCells);
            }
            OutputHelper.FlexGridMaxCells = customMaxCells;
            #endregion
               

            string[,] matrix = OutputHelper.GetDataMatrix(datanumber, out visibleRows); // get table data
            if (matrix == null)//29Apr2014
            {
                logService.WriteToLogLevel("ExtraLogs: XML template logic : data-matrix is null", LogLevelEnum.Info);
                return; 
            }
            else if (matrix != null && matrix[0, 0].Contains("Abort")) //user aborted the large table generation
            {
                string rcommcol = confService.GetConfigValueForKey("rcommcol");//23nov2012
                byte red = byte.Parse(rcommcol.Substring(3, 2), NumberStyles.HexNumber);
                byte green = byte.Parse(rcommcol.Substring(5, 2), NumberStyles.HexNumber);
                byte blue = byte.Parse(rcommcol.Substring(7, 2), NumberStyles.HexNumber);
                Color c = Color.FromArgb(255, red, green, blue);
                ////
                AUParagraph textresult = new AUParagraph();
                textresult.FontSize = BSkyStyler.BSkyConstants.TEXT_FONTSIZE;//10Nov2014
                textresult.Text = "<<---- Large table generation aborted by user ---->>";
                textresult.ControlType = "Result";
                textresult.textcolor = new SolidColorBrush(c);
                
            }

            string[,] footnotes = OutputHelper.GetFootnotes(datanumber);
            string[,] superscripttext = null;
            if (matrix != null)
                superscripttext = new string[matrix.GetLength(0), matrix.GetLength(1)];

            if (footnotes == null)
                if (AdvancedLogging) logService.WriteToLogLevel("ExtraLogs: footnotes is null", LogLevelEnum.Info);
            if (superscripttext == null)
                if (AdvancedLogging) logService.WriteToLogLevel("ExtraLogs: superscripttext is null", LogLevelEnum.Info);

            if (metadatatype == null)
                if (AdvancedLogging) logService.WriteToLogLevel("ExtraLogs: metadatatype is null", LogLevelEnum.Info);

            if (mt != null)// 'if' block AD 02Mar2012
            {
                if (AdvancedLogging) logService.WriteToLogLevel("ExtraLogs: Filling Table1:", LogLevelEnum.Info);
                char ch = 'A';
                char ch2 = 'a';
                Dictionary<char, string> errm = new Dictionary<char, string>();
                Dictionary<char, string> fs = new Dictionary<char, string>();///for footer
                int ridx = 0, cidx = 0;
                MetadataTableRow mtrow = null;
                for (int i = 0; i < mt.Metadatatable.GetLength(0); ++i)
                {
                    string msg = string.Empty;
                    mtrow = mt.Metadatatable[i];

                    if (mtrow.InfoType != null)
                    {
                        if (mtrow.InfoType.Equals("Footer"))
                        {

                            fs.Add(ch2, mtrow.BSkyMsg + "." + mtrow.RMsg); // ch2++ was here earlier
                            ridx = mtrow.DataTableRow;
                            cidx = mtrow.StartCol;
                            if (superscripttext != null &&
                                ridx>0 && ridx <= superscripttext.GetLength(0) && 
                                cidx>0 && cidx <= superscripttext.GetLength(1)
                                )
                                superscripttext[ridx - 1, cidx - 1] = Convert.ToString(ch2);
                            ch2++;
                        }
                        else
                        {
                            errm.Add(ch++, mtrow.InfoType + ": " + mtrow.BSkyMsg + "." + mtrow.RMsg);

                        }
                    }
                }
                xgrid.Metadata = errm;
                xgrid.FootNotes = fs;//footer
            }


            ///// Logic to remove extra rows from ourput grid, using metadata-2 /// for crosstab only right now ///
            bool isCrosstab = false;
            if (metadatatype != null)
                isCrosstab = (metadatatype.Equals("crosstab1")) ? true : false;
            string[,] crosstabMeta2;
            string[,] crosstabMeta3;
            if (isCrosstab)
            {
                if (AdvancedLogging) logService.WriteToLogLevel("ExtraLogs: Filling CrossTable1:", LogLevelEnum.Info);
                ///reading metadata-2
                crosstabMeta2 = OutputHelper.GetMetaData2(datanumber, "crosstab2");

                /// reading metadata-3
                crosstabMeta3 = OutputHelper.GetMetaData2(datanumber, "crosstab3");
                if ((crosstabMeta3 != null) || (crosstabMeta2 != null))//10Sep2013 only if added for follwing code
                {
                    List<string> meta = null;
                    int rowsIneachSection = 0;
                    if (crosstabMeta3 != null)
                    {
                        meta = new List<string>();
                        for (int i = 0; i < crosstabMeta3.GetLength(0); i++)// read each value from metadata2 and mix 
                        {                                                   //with metadata2 to produce valid list of rows
                            string meta3RowName = "";
                            for (int j = 0; j < crosstabMeta3.GetLength(1); j++)
                                meta3RowName += crosstabMeta3[i, j].Trim();

                            //consume metadata2 to pruduce full list of valid rows those can be displayed
                            if (crosstabMeta2 != null)
                            {
                                rowsIneachSection = Int32.Parse(crosstabMeta2[crosstabMeta2.GetLength(0) - 1, 0]);
                                for (int k = 0; k < rowsIneachSection; k++)
                                {
                                    int ri = i * rowsIneachSection + k;
                                    if (!crosstabMeta2[ri, 1].Equals("0"))// Add only those rows which are having non-zero value in metadata2
                                    {
                                        meta.Add(meta3RowName + (k + 1).ToString());
                                    }
                                }
                            }

                        }
                    }
                    else//10sep2013 else block added for 'layer' missing(NA)
                    {
                        int numberofsections = 0;
                        meta = new List<string>();
                        //consume metadata2 to pruduce full list of valid rows those can be displayed
                        if (crosstabMeta2 != null)
                        {
                            rowsIneachSection = Int32.Parse(crosstabMeta2[crosstabMeta2.GetLength(0) - 1, 0]);
                            numberofsections = crosstabMeta2.GetLength(0) / rowsIneachSection;
                            for (int i = 0; i < numberofsections; i++)
                            {
                                for (int k = 0; k < rowsIneachSection; k++)
                                {
                                    int ri = i * rowsIneachSection + k;
                                    if (!crosstabMeta2[ri, 1].Equals("0"))// Add only those rows which are having non-zero value in metadata2
                                    {
                                        meta.Add("" + (k + 1).ToString());
                                    }
                                }
                            }
                        }

                    }

                    //take one row from grid and compare with list if not found in list, delete that grid row.

                    var rowheader = grid.RowHeaders; 
                    int NoOfLayerVars = (crosstabMeta3 != null) ? crosstabMeta3.GetLength(1) : 0; //10Sep2013
                    int NoOfRowHeaderCols = rowheader.Columns.Count;
                    int NoOfRowHeaderRows = rowheader.Rows.Count;

                    int remainder = 0;
                    if (NoOfRowHeaderCols % 2 != 0) NoOfRowHeaderCols--;//if there are row% col% in rowheaders. We dont consider it.
                    for (int gri = NoOfRowHeaderRows - 1; gri >= 0; gri--)//gri is GRid Index
                    {
                        string gridRowName = ""; string secindex = "";
                        for (int j = 1; j < NoOfRowHeaderCols - 1; j += 2)//create a key sting from grid headers for a row
                        {
                            object test = rowheader[gri, j];//for testing. remove this line later
                            if (rowheader[gri, j] != null)
                                gridRowName += rowheader[gri, j].ToString().Trim(); ;
                        }
                        if (rowsIneachSection > 0)
                            remainder = (gri + 1) % rowsIneachSection;
                        if (remainder == 0)
                            secindex = rowsIneachSection.ToString().Trim();
                        else
                            secindex = remainder.ToString().Trim();


                        /// search key string in meta list (valid row list) and delete  current grid row if not found in meta list
                        if (!meta.Contains(gridRowName + secindex))
                            grid.Rows.RemoveAt(gri);
                    }

                }
            }

            string s = System.Reflection.Missing.Value.ToString();

            #region Signif Code handling for templated
            var ColHeaderMatrix = grid.ColumnHeaders; //grid's col header with multiple rows of headers
            int CHColCount = ColHeaderMatrix.Columns.Count;
            int CHRowCount = ColHeaderMatrix.Rows.Count;
            List<int> starColindexes = new List<int>();

            List<ColSignifCodes> sigcodlist = SignificanceCodesHandler.SigCodeList;//list of cols each having its significance codes
            List<string> allsigColNames = SignificanceCodesHandler.GetAllSignifColNames();
            ColSignifCodes csc = null;
            if (sigcodlist != null)
            {
                for(int C=0; C < CHColCount;C++)//find if col is in the list of col those have signif codes
                {
                    for (int R = 0; R < CHRowCount; R++)
                    {
                        if (allsigColNames.Contains(ColHeaderMatrix[R, C]))//see if colheader has signig code matching name in any row of that col
                        {
                            if (!starColindexes.Contains(C))//add unique items. No duplicates
                            {
                                starColindexes.Add(C);
                            }
                        }

                        if (starColindexes.Count > 0)//found index of col in colHeaders to which signif codes should be applied.
                        {
                            //right now first index is picked. So, we are not looking for codes based on colName beacuse they all are same for all colNames.
                            csc = sigcodlist[0];

                            //same signif codes for all colnames p.value, p-value, Sig.
                            //if you need diff codes for diff colNames than you may have to add more lines like below 
                            //for each colName that was found in the col-header.
                            //Plus you need to pick 'csc' above for matching colName not 0 index
                            xgrid.starText.Text = csc.getFooterStarMessage();

                            //Since in multi-row ColHeaders the matching colName can be found in any cell of 
                            //ColHeaderMatrix so we need to look the whole ColHeaderMatrix.
                            //break; 
                        }
                    }
                }
            }
            #endregion  
            if (matrix != null)
            {
                if (AdvancedLogging) logService.WriteToLogLevel("ExtraLogs: Filling matrix:", LogLevelEnum.Info);
                string stars = string.Empty;
                double celldata = 0.0;	  
                for (int i = 0; i < matrix.GetLength(0); i++)
                {
                    if (i < grid.Rows.Count)//05Mar2013. IndexoutOfBounds: Grid had lesser rows than martix. dont know the reason
                    {
                        if (!visibleRows[i])
                        {
                            grid.Rows[i].Visible = false;
                            continue;
                        }
                        for (int j = 0; j < matrix.GetLength(1); j++)
                        {
							stars = string.Empty;
                            if (j < grid.Columns.Count)//05Mar2013. could cause IndexoutOfBounds: 
                            {
                                if (matrix[i, j] != "NA")
                                {
                                    if (starColindexes.Count > 0 && starColindexes.Contains(j) && csc!=null)//if there is a col to which stars should be added then run code inside 'if'
                                    {
										//get number of stars from data
										if (Double.TryParse(matrix[i,j], out celldata))//convert if possible
										{
											stars = csc.getStarChars(celldata);
										}
										//grid[i, j] = matrix[i, j] + " " + stars;
                                    }
                                    if (stars.Trim().Equals("***"))
                                        stars = "(<.001)***";
                                    grid[i, j] = matrix[i, j] + " " + stars;
                                }
                                else
                                {
                                    if (superscripttext[i, j] == null)// for fixing -- appearing in cell where there is NA
                                        grid[i, j] = "";

                                    string[] sprscrcol = new string[superscripttext.GetLength(0)]; //col of superscripts
                                    for (int ri = 0; ri < superscripttext.GetLength(0); ri++)
                                    {
                                        sprscrcol[ri] = superscripttext[ri, j] != null ? superscripttext[ri, j] : "";
                                    }
                                    grid.Columns[j].Tag = (sprscrcol != null) ? sprscrcol : null; // filling superscript diff for all cells in a col. Assign col at atime
                                }
                            }
                        }
                    }
                }
            }
            grid.CellFactory = new FlexGridCellFormatFactory();

        }

        //tblno: its added so that we can find out a list of $columnNames for particular table
        private DependencyObject CreateTable(Table table, int tblno)
        {
            RefreshAPAConfig();
            if (OutputHelper.GetGlobalMacro(string.Format("GLOBAL.{0}.SPLIT", OutputHelper.AnalyticsData.DataSource.Name), "Comparegroups") == "TRUE")
            {
                List<Row> rows = table.Rows;
                table.Rows = new List<Row>();
                Row r = new Row();
                r.Initialize(true, rows);
                table.Rows.Add(r);
            }
            int maxrowdepth = 0;
            foreach (Row row in table.Rows)
            {
                if (maxrowdepth < row.DepthLevel)
                    maxrowdepth = row.DepthLevel;
            }

            int maxcoldepth = 0;
            foreach (Column col in table.Columns)
            {
                if (maxcoldepth < col.DepthLevel)
                    maxcoldepth = col.DepthLevel;
            }

            int maxrowwide = 0;
            foreach (Row row in table.Rows)
            {
                maxrowwide += row.WidthLevel;
            }

            int maxcolwide = 0;
            foreach (Column col in table.Columns)
            {
                maxcolwide += col.WidthLevel;
            }


            AUXGrid xgrid = new AUXGrid();
            AUGrid fg = xgrid.Grid;

            #region Choose theme APA or Normal
            if (APA) 
            {
                fg.GridLinesVisibility = GridLinesVisibility.None;
                fg.HeaderGridLinesBrush = Brushes.White;
                fg.ColumnHeaderBackground = Brushes.White;
                fg.RowHeaderBackground = Brushes.White;
                fg.TopLeftCellBackground = Brushes.White;
                fg.BorderBrush = Brushes.WhiteSmoke;
                fg.Background = Brushes.White;
                fg.RowBackground = Brushes.White;
                fg.BorderThickness = new Thickness(0, 3, 0, 0);
                //find border of flexgrid and set it to APA style
                xgrid.fgborder.BorderThickness = new Thickness(0, 1, 0, 1);

                xgrid.tbltitle.Text = string.Empty;// "Table Title";
                xgrid.starText.Text = string.Empty;// 
                xgrid.tableno.Text = string.Empty;// "Table No. 1.1";
            }
            else
            {

                fg.ColumnHeaderBackground = Brushes.LightBlue;
                fg.RowHeaderBackground = Brushes.LightBlue;
                fg.TopLeftCellBackground = Brushes.LightBlue;
                fg.BorderThickness = new Thickness(1);

                xgrid.tbltitle.Text = string.Empty;
                xgrid.starText.Text = string.Empty;
                xgrid.tableno.Text = string.Empty;

            }
            #endregion

            xgrid.Header.Text = table.Header.Text;

            fg.AllowMerging = AllowMerging.ColumnHeaders | AllowMerging.RowHeaders;
            fg.AllowSorting = true;

            var colheaders = fg.ColumnHeaders;
            var rowheader = fg.RowHeaders;

            colheaders.Rows[0].AllowMerging = true;
            colheaders.Rows[0].HorizontalAlignment = HorizontalAlignment.Center;
            if (colheaders.Rows.Count < maxcoldepth)
            {
                for (int i = colheaders.Rows.Count; i < maxcoldepth; ++i)
                {
                    C1.WPF.FlexGrid.Row row = new C1.WPF.FlexGrid.Row();
                    colheaders.Rows.Add(row);
                    row.AllowMerging = true;
                    row.HorizontalAlignment = HorizontalAlignment.Center;
                }
            }

            for (int i = colheaders.Columns.Count; i < maxcolwide; ++i) // creating col headers
            {
                C1.WPF.FlexGrid.Column col = new C1.WPF.FlexGrid.Column();
                colheaders.Columns.Add(col);

                //03Apr2018
                #region Fix for row-col headers merging incorrectly for templated dialogs
                //'Crosstab' requirement is opposite to that of 'Independent Samples Test' and 'One Sample Test'
                if (table.Header.Text.Contains("Cross Tabulation"))//if its crosstab dialog
                {
                    col.AllowMerging = false;
                }
                else
                { //for 'Independent Samples Test' and 'One Sample Test'(multivar)
                    col.AllowMerging = true;
                }
                #endregion
            }



            rowheader.Columns[0].AllowMerging = true;
            rowheader.Columns[0].VerticalAlignment = VerticalAlignment.Center;

            if (rowheader.Columns.Count < maxrowdepth)
            {
                for (int i = rowheader.Columns.Count; i < maxrowdepth; ++i)
                {
                    C1.WPF.FlexGrid.Column col = new C1.WPF.FlexGrid.Column();
                    col.AllowMerging = true; 
                    col.VerticalAlignment = VerticalAlignment.Center;
                    rowheader.Columns.Add(col);
                }
            }

            for (int i = rowheader.Rows.Count; i < maxrowwide; ++i)// Dont create extra/blank rows. Later deleting will slow down.
            {                                                      // right now we are  creating balnk rows and deleteing them later.
                C1.WPF.FlexGrid.Row row = new C1.WPF.FlexGrid.Row();
                rowheader.Rows.Add(row);

                //03Apr2018
                #region Fix for row-col headers merging incorrectly for templated dialogs
                if (table.Header.Text.Contains("Cross Tabulation"))//if its crosstab dialog
                {
                    row.AllowMerging = false;
                }
                else
                { 
                    row.AllowMerging = true;
                }
                #endregion
            }
            int rowindex = 0, columnindex = 0;

            foreach (Column column in table.Columns)
            {
                int index = columnindex;
                if (column.Labels.ShowHeader)
                {
                    for (int i = 0; i < column.WidthLevel; i++)
                    {
                        colheaders[rowindex, columnindex + i] = OutputHelper.ExpandMacro(column.Labels.Varname);

                    }
                    maxrowdepth -= 1;
                    index++;
                }
                foreach (string str in column.Labels.LableList)
                {
                    if (!column.Labels.Factors && !string.IsNullOrEmpty(column.Labels.Varname))
                    {
                        OutputHelper.UpdateMacro(column.Labels.Varname, str);
                    }
                    WriteCol(index, ref columnindex, str, column, colheaders, maxcoldepth);
                }
            }
            rowindex = 0;
            columnindex = 0;
            foreach (Row row in table.Rows)
            {
                int index = columnindex;
                if (row.Labels.ShowHeader)
                {
                    for (int i = 0; i < row.WidthLevel; i++)
                    {
                        rowheader[rowindex + i, columnindex] = OutputHelper.ExpandMacro(row.Labels.Varname);

                    }
                    maxrowdepth -= 1;
                    index++;
                }
                foreach (string str in row.Labels.LableList)
                {
                    if (!row.Labels.Factors && !string.IsNullOrEmpty(row.Labels.Varname))
                    {
                        OutputHelper.UpdateMacro(row.Labels.Varname, str);
                    }
                    WriteRow(ref rowindex, index, str, row, rowheader, maxrowdepth);
                }
            }


            #region Remove Unwanted Cols
            List<string> colnamelst = OutputHelper.GetKeepRemoveColNames(tblno);//new List<string>();
            if (colnamelst != null) // NULL is when there is no list (not remove nor keep list)
            {
                colnamelst.Add("Total");
                if (colnamelst.Count > 0)
                    RemoveUnlistedCols(colheaders, colnamelst);
            }
            #endregion

            return xgrid;
        }

        #region Remove unwanted cols (based on colnames)

        private void RemoveListedCols(GridPanel colheaders, List<string> colnamelist)
        {
            string currentColName = string.Empty;
            int ridx = colheaders.Rows.Count - 1; // always working with last row of col headers "Store 1", "Store 2" etc..

            for (int cidx = 0; cidx < colheaders.Columns.Count; cidx++) // since col deleting on the fly, col count will change.
            {
                currentColName = colheaders[ridx, cidx].ToString();
                if (colnamelist.Contains(currentColName))//remove col if its in the colnamelist.
                {
                    colheaders.Columns.RemoveAt(cidx);
                    cidx--; 
                }
            }

        }

        private void RemoveUnlistedCols(GridPanel colheaders, List<string> colnamelist)
        {
            string currentColName = string.Empty;
            int ridx = colheaders.Rows.Count - 1; // always working with last row of col headers "Store 1", "Store 2" etc..

            for (int cidx = 0; cidx < colheaders.Columns.Count; cidx++) // since col deleting on the fly, col count will change.
            {
                currentColName = colheaders[ridx, cidx].ToString();
                if (!colnamelist.Contains(currentColName))//remove the col that is not found in colnameslist
                {
                    colheaders.Columns.RemoveAt(cidx);
                    cidx--; 
                }
            }

        }
        #endregion

        #region Remove unwanted rows under 'Total' in Crosstab 27Oct2014
        //This is also not needed
        private List<int> totalRowIndexes = new List<int>(); //for storing unwanted row indexes
        private void RemoveUnwantedRowUnderTotal(GridPanel rowheaders)
        {
            int indexcount = totalRowIndexes.Count;
            if (indexcount > 0)
            {
                for (int ridx = indexcount - 1; ridx >= 0; ridx--)
                {
                    rowheaders.Rows.RemoveAt(ridx);//removing row from row headers, thus removing row from the grid.
                }
            }
            totalRowIndexes.Clear();//removing indexes once rows are removed.

        }
        #endregion

        //writing col header and merging them based on requirement.
        private void WriteCol(int RowIndex, ref  int ColIndex, string ParentLabel, Column col, GridPanel colheaders, int currentDepthLevel)
        {
            if (col.SubColumns.Count == 0 || ParentLabel == "Total")
            {
                colheaders[RowIndex, ColIndex] = ParentLabel;
                if (col.DepthLevel == 1 && col.DepthLevel < currentDepthLevel)
                {
                    for (int i = 1; i <= currentDepthLevel - col.DepthLevel; ++i)
                    {
                        colheaders[RowIndex + i, ColIndex] = ParentLabel;
                    }
                }
                ColIndex++;
            }
            else
            {
                int currentColIndex = ColIndex;
                foreach (Column ctemp in col.SubColumns)
                {
                    int index = RowIndex;
                    if (ctemp.Labels.ShowHeader)
                    {
                        for (int i = 0; i < ctemp.WidthLevel; i++)
                        {
                            colheaders[RowIndex + 1, ColIndex + i] = OutputHelper.ExpandMacro(ctemp.Labels.Varname);
                        }
                        currentDepthLevel -= 1;
                        index++;
                    }
                    foreach (string str in ctemp.Labels.LableList)
                    {

                        if (!ctemp.Labels.Factors && !string.IsNullOrEmpty(ctemp.Labels.Varname))
                        {
                            OutputHelper.UpdateMacro(ctemp.Labels.Varname, str);
                        }
                        WriteCol(index + 1, ref ColIndex, str, ctemp, colheaders, currentDepthLevel - 1);
                    }
                }

                for (int i = currentColIndex; i < ColIndex; ++i)
                    colheaders[RowIndex, i] = ParentLabel;
            }

        }

        //writing row header and merging them based on requirement.
        private void WriteRow(ref int RowIndex, int ColIndex, string ParentLabel, Row row, GridPanel rowheaders, int currentDepthLevel)
        {
            int totalsubrowCount = 0;
            foreach (Row rtemp in row.SubRows)
            {
                foreach (string str in rtemp.Labels.LableList)
                {
                    totalsubrowCount++;
                }
            }
            if (row.SubRows.Count == 0 || totalsubrowCount == 0)
            {
                rowheaders[RowIndex, ColIndex] = ParentLabel;
                RowIndex++;
                if (row.DepthLevel == 1 && row.DepthLevel < currentDepthLevel)
                {
                    for (int i = 1; i <= currentDepthLevel - row.DepthLevel; ++i)
                    {
                        rowheaders[RowIndex, ColIndex + i] = ParentLabel;
                    }
                }

            }
            else
            {
                int currentRowIndex = RowIndex;
                foreach (Row rtemp in row.SubRows)
                {
                    int index = ColIndex;
                    if (rtemp.Labels.ShowHeader)
                    {
                        for (int i = 0; i < rtemp.WidthLevel; i++)
                        {
                            rowheaders[RowIndex + i, ColIndex + 1] = OutputHelper.ExpandMacro(rtemp.Labels.Varname);
                        }
                        currentDepthLevel -= 1;
                        index++;
                    }
                    foreach (string str in rtemp.Labels.LableList)
                    {
                        bool myrun = false;
                        if (myrun)  ///// This place, we can use metadata-2 info for crosstab row rendering. More mods abv, needed.
                            continue;

                        if (ParentLabel == "Total" && (str.Contains("Residual"))) //str.Contains("Expected") ||
                        {
                            rowheaders.Rows.RemoveAt(RowIndex);
                            continue;
                        }
                        if (!rtemp.Labels.Factors && !string.IsNullOrEmpty(rtemp.Labels.Varname))
                        {
                            OutputHelper.UpdateMacro(rtemp.Labels.Varname, str);
                        }
                        WriteRow(ref RowIndex, index + 1, str, rtemp, rowheaders, currentDepthLevel - 1);
                    }
                }


                if (ParentLabel == "Total" && row.Labels.ShowHeader)
                {
                    for (int i = currentRowIndex; i < RowIndex; ++i)
                        rowheaders[i, ColIndex - 1] = ParentLabel;
                }
                else
                {
                    for (int i = currentRowIndex; i < RowIndex; ++i)
                        rowheaders[i, ColIndex] = ParentLabel;
                }
            }
        }

        private void GetGraphic(List<DependencyObject> lst, string fullpathfilename)//05Sep2012
        {
            ////// now add image to lst ////
            Image myImage = new Image();
            var bitmap = new BitmapImage();
            if (!File.Exists(fullpathfilename)) //25Jun2013
                return;
            var stream = File.OpenRead(fullpathfilename);
            bitmap.BeginInit();
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.StreamSource = stream;
            bitmap.EndInit();
            stream.Close();
            stream.Dispose();
            myImage.Source = bitmap;

            BSkyGraphicControl bsgc = new BSkyGraphicControl();
            bsgc.BSkyImageSource = myImage.Source;
            bsgc.ControlType = "Graphic";
            lst.Add(bsgc);
        }

        ///Getting Errors and Warnings for non-analytic R functions
        private void GetErrorsWarnings(List<DependencyObject> lst)//04Jun2013
        {
            string[,] warnerrmsg = OutputHelper.GetBSkyErrorsWarning(0, "normal");// For error messages.
            string msg = string.Empty;

            /////// For showing errors and warning in Notes control ////////////
            if (warnerrmsg != null)// 
            {
                BSkyNotes bsn = new BSkyNotes();
                bsn.NotesData = warnerrmsg;
                bsn.NotesSplitPosition = 1; bsn.RightPart = 7; bsn.LeftPart = 2;
                bsn.HearderText = "Errors/Warnings";//  header tex shown in notes grid
                bsn.ShowRow_Index = 1; // do not show any  row index as collapsed text
                bsn.CollapsedText = "Errors & Warnings."; 
                bsn.FillData();

                bsn.ControlType = "Errors/Warnings";
                lst.Add(bsn);
            }

        }

        //for getting fresh APA style configuration
        private void RefreshAPAConfig()
        {
            string APAconfig = confService.GetConfigValueForKey("outTableInAPAStyle");
            APA = (APAconfig.ToLower().Equals("true")) ? true : false;
        }

        //for stats tables when someone does not have output template XML
        private void GetStatResult(List<DependencyObject> lst)//23Aug2013
        {
            RefreshAPAConfig();
            int noofusertables = OutputHelper.GetStatsTablesCount();
            //loop thru all BSky STAT tables. 
            string restype = "";
            string[] colHeaders, rowHeaders;
            string slicename;
            string oldslicename=string.Empty;
            bool[] visibleRows;
            string tableheader = null;
            string tmp;
            for (int tno = 1; tno <= noofusertables; tno++)
            {
                ////01May2014 finding table header if any
                tableheader = "Results";
                tmp = OutputHelper.GetBSkyStatTableHeader(tno);
                if (tmp != null && tmp.Length > 0)
                    tableheader = tmp;

                #region Flexgrid Max rows and Max Col setting from config options
                int customMaxCells = 10;//get it from options settings
                int customMaxCols = 10;//get it from options settings
                string maxgridcells = confService.GetConfigValueForKey("maxflexgridcells");//

                if (maxgridcells.Trim().Length != 0)
                {
                    Int32.TryParse(maxgridcells, out customMaxCells);
                }
                OutputHelper.FlexGridMaxCells = customMaxCells;
                #endregion

                string[,] datamatrix = (string[,])OutputHelper.GetBSkyStatResults(tno, out restype, out colHeaders, out rowHeaders, out slicename);
                if (datamatrix == null)//29Apr2014
                {
                    return; 
                }
                else if (datamatrix != null && datamatrix[0, 0].Contains("Abort")) //user aborted the large table generation
                {
                    string rcommcol = confService.GetConfigValueForKey("rcommcol");//23nov2012
                    byte red = byte.Parse(rcommcol.Substring(3, 2), NumberStyles.HexNumber);
                    byte green = byte.Parse(rcommcol.Substring(5, 2), NumberStyles.HexNumber);
                    byte blue = byte.Parse(rcommcol.Substring(7, 2), NumberStyles.HexNumber);
                    Color c = Color.FromArgb(255, red, green, blue);
                    ////
                    AUParagraph textresult = new AUParagraph();
                    textresult.FontSize = BSkyStyler.BSkyConstants.TEXT_FONTSIZE;//10Nov2014
                    textresult.Text = "<<---- Large table generation aborted by user ---->>";
                    textresult.ControlType = "Result";
                    textresult.textcolor = new SolidColorBrush(c);
                    lst.Add(textresult);
                    continue; // go to next item(tno)                    
                }

                if (slicename != null && slicename.Trim().Length > 0 && !slicename.Equals(oldslicename))
                {

                    //Add headers 
                    AUParagraph Title = new AUParagraph();
                    Title.FontSize = BSkyStyler.BSkyConstants.HEADER_FONTSIZE;//10Nov2014
                    Title.Text = slicename;
                    Title.ControlType = "Header";
                    lst.Add(Title);
                    oldslicename = slicename;
                    slicename = string.Empty;
                }

                //11May2015 single cell, no title, no row header, no col header, then show it as AUPara rather than AUXGrid
                if(datamatrix!=null && datamatrix.GetLength(0)==1 && datamatrix.GetLength(1)==1)
                {
                    string domtabletitle = tmp.Trim();
                    string domcolheaders = colHeaders[0].Trim(); //if datamatrix is 1 row 1 col (1 cell) then row/col headers should be 1 only
                    string domrowheaders = rowHeaders[0].Trim();
                    if(string.IsNullOrEmpty(domtabletitle) && string.IsNullOrEmpty(domcolheaders) && string.IsNullOrEmpty(domrowheaders))//if no title no row/col headers
                    {
                        string rcommcol = confService.GetConfigValueForKey("rcommcol");//23nov2012
                        byte red = byte.Parse(rcommcol.Substring(3, 2), NumberStyles.HexNumber);
                        byte green = byte.Parse(rcommcol.Substring(5, 2), NumberStyles.HexNumber);
                        byte blue = byte.Parse(rcommcol.Substring(7, 2), NumberStyles.HexNumber);
                        Color c = Color.FromArgb(255, red, green, blue);
                        ////
                        AUParagraph textresult = new AUParagraph();
                        textresult.FontSize = BSkyStyler.BSkyConstants.TEXT_FONTSIZE;//10Nov2014
                        textresult.Text = datamatrix[0,0] != null ? datamatrix[0,0] : "No Text Result for table number: "+tno;
                        textresult.ControlType = "Result";
                        textresult.textcolor = new SolidColorBrush(c);
                        lst.Add(textresult);
                        continue; // go to next item(tno)
                    }
                }


                ///Dataframe, BSky Stat Tables
                {
                    int userchoice = datamatrix.GetLength(0) * datamatrix.GetLength(1);
                    if ( Math.Abs(customMaxCells - userchoice) < 10)
                    {
                        string rcommcol = confService.GetConfigValueForKey("rcommcol");//23nov2012
                        byte red = byte.Parse(rcommcol.Substring(3, 2), NumberStyles.HexNumber);
                        byte green = byte.Parse(rcommcol.Substring(5, 2), NumberStyles.HexNumber);
                        byte blue = byte.Parse(rcommcol.Substring(7, 2), NumberStyles.HexNumber);
                        Color c = Color.FromArgb(255, red, green, blue);

                        AUParagraph textresult = new AUParagraph();
                        textresult.FontSize = BSkyStyler.BSkyConstants.TEXT_FONTSIZE;//10Nov2014
                        textresult.Text = "<<---- Displaying Partial Results ---->>";
                        textresult.ControlType = "Result";
                        textresult.textcolor = new SolidColorBrush(c);
                        lst.Add(textresult);
                    }

                    AUXGrid xgrid = new AUXGrid();
                    AUGrid c1FlexGrid1 = xgrid.Grid;// new C1flexgrid.
                    xgrid.Header.Text = tableheader;// "Stat Result: ";// +slicename;//FlexGrid header as well as Tree leaf node text(ControlType)

                    ///////// Filling the FlexGrid with data but no headers(row/col) are present///// code from  html2flex() of BSOG class                                
                    ///////////// merge and sizing /////
                    c1FlexGrid1.AllowMerging = AllowMerging.ColumnHeaders | AllowMerging.RowHeaders;
                    c1FlexGrid1.AllowSorting = true;


                    var rowheaders = c1FlexGrid1.RowHeaders;
                    var colheaders = c1FlexGrid1.ColumnHeaders;


                    colheaders.Rows[0].AllowMerging = true; 
                    colheaders.Rows[0].HorizontalAlignment = HorizontalAlignment.Center;

                    rowheaders.Columns[0].AllowMerging = true; 
                    rowheaders.Columns[0].VerticalAlignment = VerticalAlignment.Center;


                    #region Choose theme APA or Normal
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
                        xgrid.fgborder.BorderThickness = new Thickness(0, 1, 0, 1);

                        xgrid.tbltitle.Text = string.Empty;// "Table Title";
                        xgrid.starText.Text = string.Empty;// 
                        xgrid.tableno.Text = string.Empty;// "Table No. 1.1";
                    }
                    else
                    {

                        c1FlexGrid1.ColumnHeaderBackground = Brushes.LightBlue;
                        c1FlexGrid1.RowHeaderBackground = Brushes.LightBlue;
                        c1FlexGrid1.TopLeftCellBackground = Brushes.LightBlue;

                        c1FlexGrid1.BorderThickness = new Thickness(1);

                        xgrid.tbltitle.Text = string.Empty;
                        xgrid.starText.Text = string.Empty;
                        xgrid.tableno.Text = string.Empty;

                    }
                    #endregion

                    #region Col Headers creation and Filling
                    /////////////Col Headers//////////
                    for (int i = colheaders.Rows.Count; i <1 ; i++) //datamatrix.GetLength(0)
                    {
                        C1.WPF.FlexGrid.Row row = new C1.WPF.FlexGrid.Row();
                        colheaders.Rows.Add(row);
                        row.AllowMerging = true;

                    }
                    for (int i = colheaders.Columns.Count; i < datamatrix.GetLength(1); i++) // creating col headers
                    {
                        C1.WPF.FlexGrid.Column col = new C1.WPF.FlexGrid.Column();
                        col.HorizontalAlignment = HorizontalAlignment.Center;//for center aligning data
                        colheaders.Columns.Add(col);
                        col.AllowMerging = true;

                        //for APA Style table
                        if (APA)
                        {

                        }
                    }

                    //fill col headers
                    bool colheadersexists = ((colHeaders != null) && (colHeaders.Length == datamatrix.GetLength(1))) ? true : false;//length should be same
                    for (int i = 0; i < 1; i++)  //datamatrix.GetLength(0)
                        for (int j = 0; j < datamatrix.GetLength(1); j++)
                        {
                            if (colheadersexists)
                                colheaders[i, j] = colHeaders[j];
                            else
                                colheaders[i, j] = "";//09Apr2015 No numeric col headers j + 1;    //colheadermatrix[i, j];
                        }

                    #endregion

                    #region Creating Row headers
                    /////////////Row Headers///////////
                    for (int i = rowheaders.Columns.Count; i <1 ; i++) //datamatrix.GetLength(1)
                    {
                        C1.WPF.FlexGrid.Column col = new C1.WPF.FlexGrid.Column();
                        col.AllowMerging = true; 
                        col.VerticalAlignment = VerticalAlignment.Center;
                        rowheaders.Columns.Add(col);
                    }

                    for (int i = rowheaders.Rows.Count; i < datamatrix.GetLength(0); i++)
                    {
                        C1.WPF.FlexGrid.Row row = new C1.WPF.FlexGrid.Row();
                        rowheaders.Rows.Add(row);
                        row.AllowMerging = true;

                        //for APA Style table
                        if (APA)
                        {

                        }
                    }
                    #endregion


                    #region Row header filling 
                    //05Oct2014 fill row headers. If numeric row headers, use config settings to show/hide them.
                    bool rowheadersexists = ((rowHeaders != null) && (rowHeaders.Length == datamatrix.GetLength(0))) ? true : false;//length should be same

                    if (!rowheadersexists) // if not exists then generate numeric row headers
                    {
                        rowHeaders = new string[datamatrix.GetLength(0)];
                        for (int i = 0; i < datamatrix.GetLength(0); i++)
                        {
                            rowHeaders[i] = ""+(i + 1);
                        }

                    }
                    //read configuration and then decide to pull row headers

                    string numrowheader = confService.AppSettings.Get("numericrowheaders");
                    // load default value if no value is set 
                    if (numrowheader.Trim().Length == 0)
                        numrowheader = confService.DefaultSettings["numericrowheaders"];
                    bool shownumrowheaders = numrowheader.ToLower().Equals("true") ? true : false; /// 

                    bool isnumericrowheaders = true; // assuming that row headers are numeric
                    short tnum;
                    for (int i = 0; i < rowHeaders.Length; i++)
                    {
                        if (!Int16.TryParse(rowHeaders[i], out tnum))
                        {
                            isnumericrowheaders = false; //row headers are non-numeric
                            break;
                        }
                    }

                    if (isnumericrowheaders && !shownumrowheaders)
                    {
                        //Type 2.//filling empty values for row header if rowheader is not present
                        for (int i = 0; i < rowHeaders.Length; i++)
                            rowHeaders[i] = "";
                    }

                    //finally fill row headers in AuxGrid
                    for (int i = 0; i < datamatrix.GetLength(0); i++)
                        for (int j = 0; j < 1; j++)  //datamatrix.GetLength(1)
                        {
                            rowheaders[i, j] = rowHeaders[i]; // rowHeaders, empty or not doesn't matter. It will be filled
                        }

                    ////////////////  Row header filling Ends ////////////////////
                    #endregion

                    #region Getting Col. Signif codes ready
                    List<ColSignifCodes> sigcodlist = SignificanceCodesHandler.SigCodeList;//list of cols each having its significance codes
                    #endregion

                    xgrid.starText.Text = string.Empty;
                    #region Filling Data
                    //Select column which will have start (if any). We need some logic here to figureout which col index should have stars
                    int latrowidx = colHeaders.GetLength(0) - 1;
                    int starcolidx = -1;
                    List<int> starColindexes = new List<int>();//list of col indexes. Signif code logic should be applied to all the col indexs.
                    ColSignifCodes csc=null;
                    if(sigcodlist!=null)
                    {
                       List<string> allsigColNames = SignificanceCodesHandler.GetAllSignifColNames();
                        int Cidx = 0;
                        foreach (string s in colHeaders) //getting all col indexes where signif code is to be applied.
                        {
                            if (allsigColNames.Contains(s))
                            {
                                if (!starColindexes.Contains(Cidx))//add unique items. No duplicates
                                {
                                    starColindexes.Add(Cidx);
                                }
                            }
                            Cidx++;
                        }

                        //Some dialogs like 'One Sample TTest and Independent Sample TTest' has text 'p-value' in the data cell area and not
                        //in the column header area(basically that text is not header) so it is difficult to apply signif code to that column.
                        //As a workaround we can go through the data and find the 'p-value' text(or other text) to which singnif code has to 
                        //be applied. Also we may have to hard code the dialog names here because so far these two dialogs are the exceptions.
                        //
                        //CHECK DATA CELL for text like Sig., p-value etc. and store the col-index where it was found so that 
                        //signif code can be applied later. Looks like right now only 'tableheader' is the one that can help.
                        bool isOneSmOrIndSM = (tableheader.Trim().Equals("One Sample t-test") || tableheader.Trim().Equals("Welch Two Sample t-test")) ? true : false;
                        if (isOneSmOrIndSM)
                        {
                            //dont want to check all the rows because there can be many rows in data area
                            //so we can just see like 3-4 rows max for header text (p.value, Sig. etc). We can put custom setting for this too.
                            int hardcodedvalue = 3;//or can be read from custom settings if created there.
                            int datamatrixRowCount = datamatrix.GetLength(0);

                            //pick whichever is less
                            int MaxRowsToCheck = datamatrixRowCount > hardcodedvalue ? hardcodedvalue : datamatrixRowCount;

                            for (int rw = 0; rw < MaxRowsToCheck; rw++)
                            {
                                for (int c = 0; c < datamatrix.GetLength(1); c++)
                                {
                                    if (datamatrix[rw, c] != null && allsigColNames.Contains(datamatrix[rw, c]))
                                    {
                                        if (!starColindexes.Contains(c))//add unique items. No duplicates
                                        {
                                            starColindexes.Add(c);
                                        }
                                    }
                                }
                            }
                        }
                    }

                    if (starColindexes.Count > 0)//found index of col in colHeaders to which signif codes should be applied.
                    {
                        //right now first index is picked. So, we are not looking for codes based on colName beacuse they all are same for all colNames.
                        csc = sigcodlist[0];

                        //same signif codes for all colnames p.value, p-value, Sig.
                        //if you need diff codes for diff colNames than you may have to add more lines like below 
                        //for each colName that was found in the col-header.
                        //Plus you need to pick 'csc' above for matching colName not 0 index
                        xgrid.starText.Text = csc.getFooterStarMessage();

                        //Since in multi-row ColHeaders the matching colName can be found in any cell of 
                        //ColHeaderMatrix so we need to look the whole ColHeaderMatrix.
                        //break; 
                    }                    
                    string stars = string.Empty;//for APA Style table
                    double celldata = 0.0;
                    /// Filling Data
                    bool isemptyrow;
                    for (int rw = 0; rw < datamatrix.GetLength(0); rw++)
                    {
                        isemptyrow = true;//assuming row is empty
                        for (int c = 0; c < datamatrix.GetLength(1); c++)
                        {
                            stars = string.Empty;
                            if (starColindexes.Count > 0 && starColindexes.Contains(c) && csc != null)//star col and calculating star count
                            {
                                //get data first
                                if (Double.TryParse(datamatrix[rw, c], out celldata))//convert if possible
                                {
                                    stars = csc.getStarChars(celldata);
                                }
                            }

                            if (datamatrix[rw, c] != null && datamatrix[rw, c].Trim().Length > 0)
                            {
                                if (stars.Trim().Equals("***"))
                                    stars = "(<.001)***";
                                c1FlexGrid1[rw, c] = datamatrix[rw, c]+" "+stars;
                                isemptyrow = false;// if it has atleast one column filled then row is not empty
                            }
                        }
                        //// hide or remove empty row////
                        if (isemptyrow)
                            c1FlexGrid1.Rows[rw].Visible = false;
                    }
                    #endregion

                    lst.Add(xgrid);
                }
            }
        }

        //for user tables
        private void GetUserResult(List<DependencyObject> lst)//25Jun2013
        {
            int noofusertables = OutputHelper.GetUserTablesCount();
            //loop thru all user results. They can be of different types from simple strings to arrays to matrix or dataframe.
            string restype = "";
            string[] colHeaders, rowHeaders;
            for (int tno = 1; tno <= noofusertables; tno++)
            {
                object res = OutputHelper.GetBSkyResults(tno, out restype, out colHeaders, out rowHeaders);
                if (restype != null)
                {
                    switch (restype)
                    {
                        case "string":
                            string simple = (string)res;
                            AUParagraph aupstr = new AUParagraph();
                            aupstr.Text = simple;
                            aupstr.ControlType = "String Output";
                            lst.Add(aupstr);
                            break;
                        case "stringlist":
                            string[] strarr = (string[])res;
                            AUParagraph aupstrarr = new AUParagraph();
                            string strlst = string.Empty;
                            foreach (string sa in strarr)
                            {
                                strlst = strlst + "\n" + sa;
                            }
                            aupstrarr.Text = strlst;
                            aupstrarr.ControlType = "String List Output";
                            lst.Add(aupstrarr);
                            break;
                        case "dataframe":
                        case "matrix":
                            string[,] datamatrix = (string[,])res;
                            AUXGrid xgrid = new AUXGrid();
                            AUGrid c1FlexGrid1 = xgrid.Grid;
                            xgrid.Header.Text = "" + restype;

                            ///////////// merge and sizing /////
                            c1FlexGrid1.AllowMerging = AllowMerging.ColumnHeaders | AllowMerging.RowHeaders;
                            c1FlexGrid1.AllowSorting = true;

                            var rowheaders = c1FlexGrid1.RowHeaders;
                            var colheaders = c1FlexGrid1.ColumnHeaders;


                            colheaders.Rows[0].AllowMerging = true;
                            colheaders.Rows[0].HorizontalAlignment = HorizontalAlignment.Center;

                            rowheaders.Columns[0].AllowMerging = true;  
                            rowheaders.Columns[0].VerticalAlignment = VerticalAlignment.Center;


                            /////////////Col Headers//////////
                            for (int i = colheaders.Rows.Count; i < 1; i++) 
                            {
                                C1.WPF.FlexGrid.Row row = new C1.WPF.FlexGrid.Row();
                                colheaders.Rows.Add(row);
                                row.AllowMerging = true;
                                row.HorizontalAlignment = HorizontalAlignment.Center;
                            }
                            for (int i = colheaders.Columns.Count; i < datamatrix.GetLength(1); i++) 
                            {
                                C1.WPF.FlexGrid.Column col = new C1.WPF.FlexGrid.Column();
                                colheaders.Columns.Add(col);
                                col.AllowMerging = true;
                            }

                            //fill col headers
                            bool colheadersexists = ((colHeaders != null) && (colHeaders.Length == datamatrix.GetLength(1))) ? true : false;
                            for (int i = 0; i < 1; i++)  
                                for (int j = 0; j < datamatrix.GetLength(1); j++)
                                {
                                    if (colheadersexists)
                                        colheaders[i, j] = colHeaders[j];
                                    else
                                        colheaders[i, j] = j + 1;
                                }

                            /////////////Row Headers///////////
                            for (int i = rowheaders.Columns.Count; i < 1; i++) 
                            {
                                C1.WPF.FlexGrid.Column col = new C1.WPF.FlexGrid.Column();
                                col.AllowMerging = true; 
                                col.VerticalAlignment = VerticalAlignment.Center;
                                rowheaders.Columns.Add(col);
                            }

                            for (int i = rowheaders.Rows.Count; i < datamatrix.GetLength(0); i++)
                            {
                                C1.WPF.FlexGrid.Row row = new C1.WPF.FlexGrid.Row();
                                rowheaders.Rows.Add(row);
                                row.AllowMerging = true;
                            }

                            //fill row headers
                            bool rowheadersexists = ((rowHeaders != null) && (rowHeaders.Length == datamatrix.GetLength(0))) ? true : false;

                            for (int i = 0; i < datamatrix.GetLength(0); i++)
                                for (int j = 0; j < 1; j++)  
                                {
                                    if (rowheadersexists)
                                        rowheaders[i, j] = rowHeaders[i];
                                    else
                                        rowheaders[i, j] = i + 1;
                                }


                            bool isemptyrow;
                            for (int rw = 0; rw < datamatrix.GetLength(0); rw++)
                            {
                                isemptyrow = true;//assuming row is empty
                                for (int c = 0; c < datamatrix.GetLength(1); c++)
                                {
                                    if (datamatrix[rw, c] != null && datamatrix[rw, c].Trim().Length > 0)
                                    {
                                        c1FlexGrid1[rw, c] = datamatrix[rw, c];
                                        isemptyrow = false;// if it has atleast one column filled then row is not empty
                                    }


                                }
                                //// hide or remove empty row////
                                if (isemptyrow)
                                    c1FlexGrid1.Rows[rw].Visible = false;
                            }
                            lst.Add(xgrid);
                            break;

                        default:
                            break;
                    }
                }//if result type is not null

            }
        }
    }


    //Actual Flexgrid logic

    class CustomMergeManager : C1.WPF.FlexGrid.IMergeManager
    {
        #region IMergeManager Members

        public C1.WPF.FlexGrid.CellRange GetMergedRange(C1.WPF.FlexGrid.C1FlexGrid grid, C1.WPF.FlexGrid.CellType cellType, C1.WPF.FlexGrid.CellRange range)
        {
            if (cellType == CellType.RowHeader)
            {
                var headers = cellType == CellType.ColumnHeader ? grid.ColumnHeaders : grid.RowHeaders;
                var row = range.Row;
                var col = range.Column;

                int min = int.MaxValue, max = -1;

                // merge up while all cells to the left have the same content
                for (int r = row - 1; r >= 0; r--)
                {
                    bool merge = true;
                    for (int c = 0; c < col && merge; c++)
                    {
                        if (!object.Equals(headers[r, c], headers[row, c]))
                        {
                            merge = false;
                        }
                    }
                    if (merge)
                    {
                        min = r;
                    }
                }

                // merge down while all cells to the left have the same content
                for (int r = row + 1; r < grid.RowHeaders.Rows.Count; r++)
                {
                    bool merge = true;
                    for (int c = 0; c < col && merge; c++)
                    {
                        if (!object.Equals(headers[r, c], headers[row, c]))
                        {
                            merge = false;
                        }
                    }
                    if (merge)
                    {
                        max = r;
                    }
                }

                int x = 0;
                for (int i = range.Row; i < grid.RowHeaders.Rows.Count - 1; i++)
                {

                    if (GetDataDisplay(grid, cellType, i, range.Column) != GetDataDisplay(grid, cellType, i + 1, range.Column)) break;
                    if (max >= i + 1)
                        range.Row2 = i + 1;
                    else
                        break;
                }
                for (int i = range.Row; i > 0; i--)
                {
                    if (GetDataDisplay(grid, cellType, i, range.Column) != GetDataDisplay(grid, cellType, i - 1, range.Column)) break;
                    if (min <= i - 1)
                        range.Row = i - 1;
                    else
                        break;
                }
                for (int i = range.Column; i < grid.RowHeaders.Columns.Count - 1; i++)
                {
                    if (GetDataDisplay(grid, cellType, range.Row, i) != GetDataDisplay(grid, cellType, range.Row, i + 1)) break;
                    range.Column2 = i + 1;
                }
                for (int i = range.Column; i > 0; i--)
                {
                    if (GetDataDisplay(grid, cellType, range.Row, i) != GetDataDisplay(grid, cellType, range.Row, i - 1)) break;
                    range.Column = i - 1;
                }
            }
            else if (cellType == CellType.ColumnHeader)
            {
                var headers = cellType == CellType.ColumnHeader ? grid.ColumnHeaders : grid.RowHeaders;
                var row = range.Row;
                var col = range.Column;

                int min = int.MaxValue, max = -1;
                int originalcol = range.Column;
                int originalRow = range.Row;

                for (int c = col - 1; c >= 0; c--)
                {
                    bool merge = true;
                    for (int r = 0; r < row && merge; r++)
                    {
                        if (!object.Equals(headers[r, c], headers[r, col]))
                        {
                            merge = false;
                        }
                    }
                    if (merge)
                    {
                        min = c;
                    }
                }

                // merge down while all cells to the left have the same content
                for (int c = col + 1; c < grid.ColumnHeaders.Columns.Count; c++)
                {
                    bool merge = true;
                    for (int r = 0; r < row && merge; r++)
                    {
                        if (!object.Equals(headers[r, c], headers[r, col]))
                        {
                            merge = false;
                        }
                    }
                    if (merge)
                    {
                        max = c;
                    }
                }

                int x = 0;
                for (int i = range.Column; i < grid.ColumnHeaders.Columns.Count - 1; i++)
                {

                    if (GetDataDisplay(grid, cellType, range.Row, i) != GetDataDisplay(grid, cellType, range.Row, i + 1)) break;
                    if (max >= i + 1)
                        range.Column2 = i + 1;
                    else
                        break;
                }
                for (int i = range.Column; i > 0; i--)
                {
                    if (GetDataDisplay(grid, cellType, range.Row, i) != GetDataDisplay(grid, cellType, range.Row, i - 1)) break;
                    if (min <= i - 1)
                        range.Column = i - 1;
                    else
                        break;
                }
                for (int i = range.Row; i < grid.ColumnHeaders.Rows.Count - 1; i++)
                {
                    if (GetDataDisplay(grid, cellType, i, range.Column) != GetDataDisplay(grid, cellType, i + 1, range.Column)) break;
                    range.Row2 = i + 1;
                }
                for (int i = range.Row; i > 0; i--)
                {
                    if (GetDataDisplay(grid, cellType, i, range.Column) != GetDataDisplay(grid, cellType, i - 1, range.Column)) break;
                    range.Row = i - 1;
                }

            }
            return range;
        }
        string GetDataDisplay(C1FlexGrid grid, CellType cellType, int r, int c)
        {

            if (cellType == CellType.RowHeader)
            {
                if (r >= grid.RowHeaders.Rows.Count || c >= grid.RowHeaders.Columns.Count || grid.RowHeaders[r, c] == null)
                    return string.Empty;
                return grid.RowHeaders[r, c].ToString();
            }
            else
            {
                if (r >= grid.ColumnHeaders.Rows.Count || c >= grid.ColumnHeaders.Columns.Count || grid.ColumnHeaders[r, c] == null)
                    return string.Empty;
                return grid.ColumnHeaders[r, c].ToString();
            }
        }
        #endregion
    }
}