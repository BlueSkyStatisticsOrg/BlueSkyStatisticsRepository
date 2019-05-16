using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BSky.Statistics.Common;
using System.IO;
using System.Reflection;
using BSky.Statistics.Common.Interfaces;
using System.Xml;
using BSky.Lifetime;
using BSky.Lifetime.Interfaces;
using RDotNet;
using BSky.XmlDecoder;
using System.Diagnostics;
using BSky.Lifetime.Services;
using BSky.ConfigService.Services;
using BSky.ConfService.Intf.Interfaces;

namespace BSky.Statistics.R
{
    public class UAPackageAPI : CommandDispatcher, IAnalyticCommands  //, IDisposable
    {
        private static string[] _RPackageFnames = new string[] { "BlueSky_4.4.zip" };
        private static string[] RPackages = new string[] { packageName, packageName3, "foreign" };

        Journal _journal; //For All R commands
        Journal _userJournal; //12Aug2016 Only for user executed commands (no sink or other background commands)
        ILoggerService logService = LifetimeService.Instance.Container.Resolve<ILoggerService>();//17Dec2012
        IConfigService confService = LifetimeService.Instance.Container.Resolve<IConfigService>();//15Jan2019
        RecentItems userPackageList = LifetimeService.Instance.Container.Resolve<RecentItems>();//06Feb2014 
        XMLitemsProcessor defaultPackageList = LifetimeService.Instance.Container.Resolve<XMLitemsProcessor>("defaultpackages");//06Oct2014 extra packages those are mentioned in DefaultPackages.xml
        //logService.DefaultSettings[""]

        RService dispatcher;
        //RDotNetService dispatcher;
        RPackageManager rpm;//30Jan2014

        private const string packageName = "BlueSky";
        //private const string packageName2 = "ua";
        private const string packageName3 = "car";//03Apr2013

        //06Nov2014 Default Package message for unsuccessfull packages
        public string DefPkgMsg
        {
            get;
            set;
        }

        //if you have 200cols in a dataset, you may only want to load 15 
        private int _maxcolstoload = 15;
        public int MaxColsToLoad
        {
            get { return _maxcolstoload; }
            set { _maxcolstoload = value; }
        }
        bool AdvancedLogging;

        public override void onLoad()
        {
            dispatcher = new RService();
            _journal = new Journal()
            {
                FileName = DirectoryHelper.GetJournalFileName(),
                MaxFileSize = 50,
                MaxBackupFiles = 10
            };

            //12Aug2016 For user Journal
            _userJournal = new Journal()
            {
                FileName = DirectoryHelper.GetUserJournalFileName(),
                MaxFileSize = 50,
                MaxBackupFiles = 10
            };
            rpm = new RPackageManager(dispatcher, _journal);

            AdvancedLogging = AdvancedLoggingService.AdvLog;//08Aug2016
        }

        public override DataFrame GetDF(DataSource ds)
        {
            DataFrame _DF = dispatcher.GetDataFrame(ds.Name);
            return _DF;
        }

        private bool UserLogFilter(string stmt)
        {
            bool writetolog = true;

            if (stmt.Contains("exists('") ||
                stmt.Contains("is.null(") ||
                //stmt.Contains("is.data.frame(") ||
                stmt.Contains("write(\"Error: Cannot Load / Refresh Dataset.\",fp)") ||
                stmt.Contains("write(\" - Dataframe does not exist(or is NULL or has no rows / columns).\",fp)") ||
                stmt.Contains("write(\" - OR not 'data.frame' type.\",fp)") ||
                stmt.Contains("write(\" - OR required R package(s) are missing.\",fp)") ||
                stmt.Equals("TRUE") ||
                stmt.Contains("BSkyQueue = BSkyGetHoldFormatObjList()") ||
                stmt.Contains("BSkyQueue") ||
                stmt.Equals("FALSE") ||
                stmt.Contains("sink()") ||
                stmt.Contains("flush(fp)") ||
                stmt.Contains("close(fp)") ||
                stmt.Contains("tmp <- installed.packages(noCache = TRUE)") ||
                stmt.Contains("tmp <- search()") ||
                stmt.Contains("fp<- file(\"") ||
                stmt.Contains("sink(fp, append=FALSE, type=c(\"output\"))") ||
                stmt.Contains("sink(fp, append=FALSE, type=c(\"message\"))") ||
                stmt.Contains("sink(stderr(), type=c(\"message\"))") ||
                stmt.Contains("rsink.txt") ||
                stmt.Contains("if(dev.cur()[[1]] == 2) dev.off()") ||
                stmt.Contains("png(\"") ||
                stmt.Contains("image%03d.png") ||
                stmt.Contains("bskytempvarname") ||
                stmt.Contains("BSkyBatchCommand") ||
                stmt.Contains("bskyfrmtobj") ||
                stmt.Contains("BSkySetCurrentDatasetName") ||
                stmt.Contains("New.version.BSkyComputeSplitdataset") 

                )
                writetolog = false;
            return writetolog;
        }

        #region Analytic Functions

        //uabinomial : function (vars, p = 0.5, alternative = "two.sided", conf.level = 0.95, descriptives = TRUE, quartiles = TRUE, datasetname, missing = 0)  
        string PrototypeBinomial = "uabinomial({0}, p = {1}, alternative = '{2}', conf.level = {3}, descriptives = {4}, quartiles = {5}, '{6}', missing = {7})";

        //ua2relatedsamples : function (uapairs, descriptives = TRUE, quartiles = TRUE, datasetname, missing = 0)          
        string Prototype2relatedsamples = "ua2relatedsamples(uapairs, descriptives = TRUE, quartiles = TRUE, datasetname, missing = 0)";

        //uakrelatedsam : function (vars, descriptives = TRUE, quartiles = TRUE, datasetname)  

        string PrototypeOneSample = "uaonesmt.test(c({0}), mu = {1}, conf.level = {2}, '{3}', missing = {4})";

        public UAReturn UABinomial(ServerDataSource dataSource, List<string> vars, double p, string alternative, double confidenceLevel, bool descriptives, bool quartiles, int missing)
        {
            UAReturn result = new UAReturn();

            if (string.IsNullOrEmpty(alternative)) alternative = "two.sided";

            result.CommandString = string.Format(PrototypeBinomial, toRCollection(vars), p, alternative, confidenceLevel, descriptives, quartiles, dataSource.FileName, missing);

            result.Data = EvaluateToObjectXml(result.CommandString);

            result.Success = true;

            return result;
        }

        public UAReturn UAOneSample(ServerDataSource dataSource, List<string> vars, double mu, double confidenceLevel, int missing)
        {
            UAReturn result = new UAReturn();

            result.CommandString = string.Format(PrototypeOneSample, toRCollection(vars), mu, confidenceLevel, dataSource.FileName, missing);

            result.Data = EvaluateToObjectXml(result.CommandString);

            result.Success = true;

            return result;
        }

        private string toRCollection(List<string> list)
        {
            StringBuilder sb = new StringBuilder();

            foreach (string s in list)
            {
                sb.Append("'").Append(s).Append("'").Append(',');
            }

            return sb.ToString().Substring(0, sb.Length - 1);
        }

        #endregion

        #region Dataset
        public UAReturn OpenEmptyDataset(ServerDataSource dataSource)//03Jan2014
        {
            string commstr = string.Empty;
            UAReturn result = new UAReturn() { Success = false };

            result.CommandString = RCommandStrings.LoadEmptyDataSource(dataSource);
            commstr = result.CommandString;
            result = this.Evaluate(commstr);

            UAReturn result2 = RefreshNewDataset(dataSource);//26Mar2013 to avoid code redundancy.(code here was separated out to a function)

            result.Datasource = result2.Datasource;
            result.Success = result2.Success;
            result.CommandString = commstr;//21Oct2013
            return result;
        }

        public UAReturn OpenDataset(ServerDataSource dataSource, string sheetname, bool removeSpacesSPSS)//, IOpenDataFileOptions odfo=null
        {
            string fulpathfilename = dataSource.FileNameWithPath.Replace("\\", "/");
            string commstr = string.Empty;
            UAReturn result = new UAReturn() { Success = false };
            result.CommandString = RCommandStrings.LoadDataSource(dataSource, sheetname, removeSpacesSPSS);//, odfo
            commstr = result.CommandString;

            //07Jul2015 if file type is not supported commstr will be empty(may be null)
            //No need to process further as file type is not supported
            if (commstr == null || commstr.Trim().Length == 0)

            {
                logService.WriteToLogLevel("Unable to open file: ", LogLevelEnum.Error);
                result.Datasource = null;
                result.Success = false;
                result.CommandString = commstr;//21Oct2013
                result.Error = "Error opening file: File format not supported (or corrupt file or duplicate column names).";
                return result;
            }

            logService.Info("Before R call to Open Dataset.");
            Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();
            result = this.Evaluate(commstr);//Execute command, if supported file type is passed.
            stopwatch.Stop();
            long elapsed = stopwatch.ElapsedMilliseconds;

            if (AdvancedLogging) logService.WriteToLogLevel("PERFORMANCE:Loading in R using R.NET: Time taken: " + elapsed, LogLevelEnum.Info);

            logService.Info("After R call to Open Dataset.");

            if (result.Data == null)//this checks finds if R was able to successfully open the dataset.
            {
                logService.WriteToLogLevel("Unable to open file: " + commstr, LogLevelEnum.Error);
                result.Datasource = null;
                result.Success = false;
                result.CommandString = commstr;//21Oct2013
                return result;
            }
            else if (result.Data != null)//07Jul2015
            {
                //06Sep2015 Open app with data.table missing. Install req. from CRAN. Now File > Open (app crashes)
                if (OutputHelper.AnalyticsData == null)
                    OutputHelper.AnalyticsData = new Interfaces.Model.AnalyticsData();

                OutputHelper.AnalyticsData.Result = result;

                string[,] errwarn = OutputHelper.GetBSkyErrorsWarning(1, "normal"); //for getting errors and warning from BSkyReturnStruct.

                if (errwarn != null)
                {
                    string[] RErrors = new string[errwarn.GetLength(0)];

                    for (int i = 0; i < errwarn.GetLength(0); i++)
                    {
                        RErrors[i] = errwarn[i, 2];
                    }
                    result.Error = string.Join(". ", RErrors);
                }
            }

            if (result != null && result.Error != null && result.Error.Trim().Length > 0)
            {
                //17Mar2016
                //If there is any more "error" string other than filepath or filename then there is actual error while
                // trying to open a dataset in R and there is no sense in trying to load it in a grid.
                bool errorStr = false;

                if (result.Error.Trim().Replace(fulpathfilename, "").ToLower().Contains("error"))//Path removed then cheking for "error"
                {
                    errorStr = true;
                }

                //07Jul2015 IF Error occurred the nno need to do further processing. Push this error message to top layers so that
                // it can be dispalyed in UI (outpuwindow or message box)
                if ((result.Error.Trim().Contains("R Err Msg : ") || errorStr)) //result.Error.Trim().ToLower().Contains("error")
                {
                    return result; //Return from here. No further processing (like RefreshDataset).
                }
            }

            logService.WriteToLogLevel("Before RefreshDataset:", LogLevelEnum.Info);
            stopwatch.Restart();

            UAReturn result2 = RefreshDataset(dataSource);//26Mar2013 to avoid code redundancy.(code here was separated out to a function)
            stopwatch.Stop();
            elapsed = stopwatch.ElapsedMilliseconds;
            if (AdvancedLogging) logService.WriteToLogLevel("PERFORMANCE:Reading Col Attributes and Creating DataSourceVariable: Time taken: " + elapsed, LogLevelEnum.Info);

            logService.WriteToLogLevel("After RefreshDataset:", LogLevelEnum.Info);

            result.Datasource = result2.Datasource;
            result.Success = result2.Success;
            result.Error = result2.Error;
            result.CommandString = commstr;//21Oct2013
            return result;
        }

        public UAReturn LoadDataFrame(ServerDataSource dataSource, string dframename) //13Feb2014 
        {
            string commstr = string.Empty;
            UAReturn result = new UAReturn() { Success = false };

            result.CommandString = RCommandStrings.LoadDataframe(dataSource, dframename);
            commstr = result.CommandString;
            result = this.Evaluate(commstr);
            if (result.Data != null)
            {
                UAReturn result2 = RefreshNewDataset(dataSource);

                result.Datasource = result2.Datasource;
                result.Success = result2.Success;
                result.Error = result2.Error;
                result.CommandString = commstr;//21Oct2013
            }
            return result;
        }

        public object GetSQLListOfTables(string sqlcomm)//24Nov2015
        {
            object otblst = dispatcher.EvaluateToObject(sqlcomm, true);
            return otblst;
        }

        public object GetODBCTableList(string filename, bool isxlsx)//27Jan2014
        {
            string command = RCommandStrings.GetODBCTableList(filename, isxlsx);
            object otblst = dispatcher.EvaluateToObject(command, true);
            return otblst;
        }

        public object GetRDataDfObjectList(string fname) //23May2018
        {
            string command = RCommandStrings.GetRobjDFList(fname);
            object otblst = dispatcher.EvaluateToObject(command, true);
            return otblst;
        }

        public override object GetDatagridFindResults(string findtext, string[] selectedcols, bool matchcase, string datasetname)//24Jun2016
        {
            string rcommand = string.Empty;
            string colnamecollection = string.Empty; ;// "NULL";
            string ignoreCase = matchcase ? "FALSE" : "TRUE";

            if (selectedcols != null && selectedcols.Length > 0)//if selected cols. Create : c('tg0','tg1','tg2')
            {
                colnamecollection = "c(";
                for (int i = 0; i < selectedcols.Length; i++)//(string colname in selectedcols)
                {
                    if (selectedcols[i] != null)
                    {
                        if (i < selectedcols.Length - 1)//put comma
                        {
                            colnamecollection = colnamecollection + "\'" + selectedcols[i] + "\',";
                        }
                        else
                        {
                            colnamecollection = colnamecollection + "\'" + selectedcols[i] + "\')";
                        }
                    }
                }
                rcommand = string.Format("FindInDataset(searchtext = '{0}', colNames = {1}, ignorecase={2}, dataSetNameOrIndex='{3}')", findtext, colnamecollection, ignoreCase, datasetname);
            }
            else
            {
                rcommand = string.Format("FindInDataset(searchtext = '{0}',  ignorecase={1}, dataSetNameOrIndex='{2}')", findtext, ignoreCase, datasetname);
            }

            SymbolicExpression symex = null;
            symex = dispatcher.EvaluateToSymExp(rcommand);
            DataFrame df = symex.AsDataFrame(); 
            return df;
        }

        public override object GetAllModels(string classtype)//09Sep2016
        {
            string rcommand = string.Empty;
            string colnamecollection = string.Empty; ;// "NULL";

            if (classtype != null && classtype.Length > 0)//if selected cols. Create : c('tg0','tg1','tg2')
            {
                rcommand = string.Format("BSkyGetAvailableModels(objclasslist = c('{0}'))", classtype);
            }

            SymbolicExpression symex = null;
            symex = dispatcher.EvaluateToSymExp(rcommand);
            DynamicVector retobj = symex.AsVector();
            GenericVector gv = symex.AsList();
            int count = gv.Count();
            string modname = string.Empty;
            string[] modelnames = new string[count];

            for (int i = 0; i < count; i++)
            {
                modname = (gv[i] != null && gv[i].AsCharacter() != null && gv[i].AsCharacter()[0] != null) ? gv[i].AsCharacter()[0].ToString() : string.Empty;
                modelnames[i] = modname;
            }

            return (modelnames);
        }

        public UAReturn RefreshNewDataset(ServerDataSource dataSource)//03Jan2014
        {
            UAReturn result = new UAReturn() { Success = false };

            if (true)
            {
                //Get matrix columns
                string subCommand = RCommandStrings.GetDataFrameColumnNames(dataSource);

                object colnames = dispatcher.EvaluateToObject(subCommand, false);
                //if colnames are null. Because we were unable to open the dataset because of any reason.
                if (colnames == null)
                {
                    CloseDataset(dataSource);
                    result.Success = false;
                    result.Error = "Error Opening Dataset.";
                    return result;
                }

                if (isDuplicateColnames(colnames))
                {
                    CloseDataset(dataSource);
                    result.Success = false;
                    result.Error = "Duplicate Column Names in Dataset";
                    return result;
                }
                if (colnames != null)
                {
                    string[] columnNames = null;
                    Type retType = colnames.GetType();

                    if (retType.Name == "String[]")//for multicols
                    {
                        columnNames = (String[])colnames;
                    }
                    else if (retType.Name == "String")//for single col
                    {
                        columnNames = new string[1];
                        columnNames[0] = (String)colnames;
                    }
                    else
                    {
                        return new UAReturn() { Success = false };
                    }

                    //maximum factors allowed
                    object maxf = (getMaxFactors(dataSource));

                    //sym = maxf as SymbolicExpression;

                    int mxf;//sym.AsInteger()[0];
                    bool parseSuccess = int.TryParse(maxf.ToString(), out mxf);
                    dataSource.MaxFactors = parseSuccess ? mxf : 40; //Hardcoded Default max factor count 

                    dataSource.Variables.Clear();
                    int rowcount = GetRowCount(dataSource);//get number of rows in dataset/dataframe

                    //14Apr2016
                    //Sometimes GetRowCount is not fetching count from R. Instead there is error. 

                    if (rowcount < 1) //Avoid app crash
                    {
                        CloseDataset(dataSource);
                        result.Success = false;
                        result.Error = "No rows in Dataset"; //19Oct2016. fixed the message
                        return result;
                    }

                    int columnindex = 1;
                    string colnm = string.Empty;
                    SymbolicExpression symex = null;

                    foreach (object s in columnNames)
                    {
                        if (s != null)
                            colnm = s.ToString();
                        else
                        {
                            //The custom name can be an issue if the table already has got same name as the custom generated one.
                            colnm = "VAR" + columnindex.ToString();
                        }
                        symex = dispatcher.EvaluateToSymExp(string.Format("UAgetColProperties(dataSetNameOrIndex='{0}', colNameOrIndex={1}, asClass=FALSE)", dataSource.Name, columnindex));

                        GenericVector gv = symex.AsList();

                        //19Oct2016 for ArgumentOutOfRangeException
                        if (gv == null || gv.Length < 14) //col props not fetched correctly(may be R side issue). So doesn't make sense to load/refresh dataset
                        {
                            CloseDataset(dataSource);
                            result.Success = false;
                            result.Error = "Error fetching column properties. Dataset can't be loaded/refreshed.";
                            return result;
                        }

                        string colclass = dispatcher.RawEvaluateGetstring(string.Format("class({0}[[{1}]])", dataSource.Name, columnindex));//,true);

                        if (colclass == null)
                        {
                            colclass = "";
                        }

                        string lab = (gv[2] != null && gv[2].AsCharacter() != null && gv[2].AsCharacter()[0] != null) ? gv[2].AsCharacter()[0].ToString() : string.Empty;
                        DataColumnTypeEnum dtyp = (gv[1] != null && gv[1].AsCharacter() != null && gv[1].AsCharacter()[0] != null) ? GetCovertedDataType(gv[1].AsCharacter()[0].ToString()) : DataColumnTypeEnum.Character;
                        string mistyp = (gv[5] != null && gv[5].AsCharacter() != null && gv[5].AsCharacter()[0] != null) ? gv[5].AsCharacter()[0].ToString() : string.Empty;
						
                        DataSourceVariable var = new DataSourceVariable()
                        {
                            RName = colnm,
                            Label = lab,
                            DataType = dtyp,
                            DataClass = colclass,
                            Measure = DataColumnMeasureEnum.Scale,
                            Width = 4,
                            Decimals = 0,
                            Columns = 8,
                            MissType = mistyp,
                            RowCount = rowcount 
                        };

                        if (symex != null)
                        {
                            {
                                ////Set Measure
                                switch (gv[7].AsCharacter()[0].ToString())
                                {
                                    case "factor":
                                        var.Measure = DataColumnMeasureEnum.Nominal;
                                        break;

                                    case "ordinal":
                                        var.Measure = DataColumnMeasureEnum.Ordinal;
                                        break;

                                    default:
                                        if (var.DataType == DataColumnTypeEnum.Character) //06Feb2017 Treating character as String. //02Jun2015 treating "character" type as Nominal in UI. In R its not factor
                                        {
                                            var.Measure = DataColumnMeasureEnum.String;
                                        }
                                        else if (var.DataType == DataColumnTypeEnum.Logical
                                            || var.DataClass == "logical")
                                        {
                                            var.Measure = DataColumnMeasureEnum.Logical;
                                        }
                                        else if (var.DataType == DataColumnTypeEnum.Date
                                           || var.DataClass == "POSIXct" || var.DataClass == "Date")
                                        {
                                            var.Measure = DataColumnMeasureEnum.Date;
                                        }
                                        else
                                            var.Measure = DataColumnMeasureEnum.Scale;
                                        break;
                                }

                                CharacterVector cv = gv[3].AsCharacter();
                                string[] vals = cv.ToArray();

                                if (var.DataClass == "logical") vals = new string[] { "True", "False", "<NA>" };//10Oct2015 Fix for logical DataClass

                                if (vals != null && vals.Length > 0)
                                {
                                    if (vals.Length > 1)
                                    {
                                        var.Values.AddRange(vals);//more than 1 strings
                                    }
                                    else if (vals[0].Trim().Length > 0)
                                    {
                                        var.Values.Add(vals[0]);//1 string
                                    }
                                }

                                if (!(var.MissType == "none"))
                                {
                                    CharacterVector cvv = gv[4].AsCharacter();
                                    string[] misvals = cvv.ToArray();

                                    if (misvals != null && misvals.Length > 0)
                                    {
                                        if (misvals.Length > 1)
                                        {
                                            var.Missing.AddRange(misvals);//more than 1 strings
                                        }
                                        else if (misvals[0].Trim().Length > 0)
                                        {
                                            var.Missing.Add(misvals[0]);//1 string
                                        }
                                    }
                                }
                                else
                                {
                                    string misval = "none";
                                    var.Missing.Add(misval);
                                }
                            }
                        }

                        if (dataSource.Extension == "rdata")// if filetype is RDATA.
                        {
                            if (gv[9].AsCharacter() != null && gv[9].AsCharacter()[0].ToString() != "-2146826288")
                                var.Width = Int32.Parse(gv[9].AsCharacter()[0].ToString());

                            if (gv[10].AsCharacter() != null && gv[10].AsCharacter()[0].ToString() != "-2146826288")
                                var.Decimals = Int32.Parse(gv[10].AsCharacter()[0].ToString());

                            if (gv[11].AsCharacter() != null && gv[11].AsCharacter()[0].ToString() != "-2146826288")
                                var.Columns = UInt32.Parse(gv[11].AsCharacter()[0].ToString());
                        }

                        try
                        {
                            ////////// Alignment  ////////////
                            string align = gv[6].AsCharacter()[0].ToString();

                            if (align == "-2146826288") align = "Left";

                            DataColumnAlignmentEnum alignVal = (DataColumnAlignmentEnum)Enum.Parse(typeof(DataColumnAlignmentEnum), align);

                            if (Enum.IsDefined(typeof(DataColumnAlignmentEnum), alignVal))
                                var.Alignment = alignVal;
                            else
                                var.Alignment = DataColumnAlignmentEnum.Left;

                            var.Role = DataColumnRole.Input;// Role is not used, I guess, so 'if' is commented above
                        }
                        catch (ArgumentException ex)
                        {
                            logService.WriteToLogLevel("Not a member of enum(Alignment) ", LogLevelEnum.Error);
                        }

                        if (dataSource.Variables.Count < _maxcolstoload)
                        {
                            DataSourceVariable var2 = new DataSourceVariable()
                            {
                                RName = var.Name,
                                Label = var.Label,
                                DataType = var.DataType,
                                DataClass = var.DataClass,
                                Measure = var.Measure,
                                Width = 4,
                                Decimals = 0,
                                Columns = 8,
                                MissType = var.MissType,
                                RowCount = var.RowCount,
                                Alignment = var.Alignment

                            };
                            dataSource.FewVariables.Add(var2);
                        }

                        dataSource.Variables.Add(var);
                        columnindex++;
                        dataSource.RowCount = Math.Max(dataSource.RowCount, dataSource.Variables.Last().RowCount);
                    }

                    result.Datasource = dataSource;
                    result.Success = true;

                    //26Oct2016 this.DataSources.Add(dataSource);
                    UpdateDataSources(dataSource);//26Oct2016 use this instead of line above
                }
            }
            else // no need of this 'else' unless you want to put custom error message in result
            {
            }
            return result;
        }

        // 'colnames' here, appears to working as pass by reference.
        private bool isDuplicateColnames(object colnames)
        {
            bool isDuplicate = true;

            string[] columnNames = null;//var columnNames = new string[] { "aaa", "bbb" };
            Type retType = colnames.GetType();

            if (retType.Name == "String[]")//for multicols
            {
                columnNames = (String[])colnames;
            }
            else if (retType.Name == "String")//for single col
            {
                columnNames = new string[1];
                columnNames[0] = (String)colnames;
            }

            string varname = "Var";
            //14Apr2016
            //// Change 'null' colnames to Var+idx ///
            for (int i = 0; i < columnNames.Length; i++)
            {
                //generate a unique col name if there is empty/null colname
                if (columnNames[i] == null || columnNames[i].Length == 0)// null or empty string
                {
                    for (int idx = 1; ; idx++)
                    {
                        varname = "Var" + idx;

                        int foundAt = Array.IndexOf(columnNames, varname);

                        if (foundAt > -1)
                        {
                            continue;//there was a colname that matches the generated (varname) colname so generate another name.
                        }
                        else
                        {
                            break;//unique name got generated successfully
                        }
                    }
                    columnNames[i] = varname;
                }
            }

            //find does array has duplicates
            if (columnNames.Distinct().Count() == columnNames.Count())
                isDuplicate = false;

            return isDuplicate;
        }

        //get properties of a col at a time
        public UAReturn RefreshDataset(ServerDataSource dataSource)//04Mar2015 refresh on new row added by compute
        {
            Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();
            Stopwatch stopwatch2 = System.Diagnostics.Stopwatch.StartNew();

            logService.Info("Before getting column attributes for all columns.");
            UAReturn result = new UAReturn() { Success = false };

            if (true)
            {

                //Get matrix columns
                string subCommand = RCommandStrings.GetDataFrameColumnNames(dataSource);

                object colnames = dispatcher.EvaluateToObject(subCommand, false);
                //if colnames are null. Because we were unable to open the dataset because of any reason.
                if (colnames == null)
                {
                    result.Success = false;
                    result.Error = "Error Opening Dataset.";
                    return result;
                }

                //if colnames are duplicate then we dont load the dataset and show error message. Also we clean R
                if (isDuplicateColnames(colnames))// 'colnames' here, appears to working as pass by reference.
                {
                    result.Success = false;
                    result.Error = "Duplicate Column Names in Dataset";
                    return result;
                }
                if (colnames != null)
                {
                    string[] columnNames = null;//var columnNames = new string[] { "aaa", "bbb" };
                    Type retType = colnames.GetType();

                    if (retType.Name == "String[]")//for multicols
                    {
                        columnNames = (String[])colnames;
                    }
                    else if (retType.Name == "String")//for single col
                    {
                        columnNames = new string[1];
                        columnNames[0] = (String)colnames;
                    }
                    else
                    {
                        return new UAReturn() { Success = false };
                    }

                    //maximum factors allowed
                    object maxf = (getMaxFactors(dataSource));
                    //sym = maxf as SymbolicExpression;

                    int mxf;//sym.AsInteger()[0];
                    bool parseSuccess = int.TryParse(maxf.ToString(), out mxf);
                    dataSource.MaxFactors = parseSuccess ? mxf : 40; //Hardcoded Default max factor count   //int.Parse(maxf.ToString());

                    dataSource.Variables.Clear();
                    int rowcount = GetRowCount(dataSource);//31Dec2014

                    //14Apr2016

                    if (rowcount < 1) //Avoid app crash
                    {
                        result.Success = false;
                        result.Error = "No rows in Dataset"; //19Oct2016. fixed the message
                        return result;
                    }

                    int columnindex = 1;
                    string colnm = string.Empty;
                    SymbolicExpression symex = null;
                    string DSvalidated = dispatcher.RawEvaluateGetstring(string.Format("BSkyValidateDataset('{0}')", dataSource.Name));//,true);
                    stopwatch.Restart(); long elapsed2 = 0; bool once = true;

                    foreach (object s in columnNames)
                    {
                        if (s != null)
                            colnm = s.ToString();
                        else
                        {
                            colnm = "VAR" + columnindex.ToString();
                        }
                        //logService.WriteToLogLevel("Get Col Props from R, start : "+s.ToString(), LogLevelEnum.Info);
                        if (DSvalidated != null)
                        {
                            symex = dispatcher.EvaluateToSymExp(string.Format("UAgetColProperties(dataSetNameOrIndex='{0}', colNameOrIndex={1}, asClass=FALSE, isDSValidated=TRUE)", DSvalidated, columnindex));
                        }
                        else//ELSE may not run at all. Just for readability we have kept it here
                        {
                            symex = dispatcher.EvaluateToSymExp(string.Format("UAgetColProperties(dataSetNameOrIndex='{0}', colNameOrIndex={1}, asClass=FALSE)", dataSource.Name, columnindex));
                        }

                        GenericVector gv = symex.AsList();

                        //19Oct2016 for ArgumentOutOfRangeException
                        if (gv == null || gv.Length < 14) //col props not fetched correctly(may be R side issue). So doesn't make sense to load/refresh dataset
                        {
                            result.Success = false;
                            result.Error = "Error fetching column properties. Dataset can't be loaded/refreshed.";
                            return result;
                        }

                        string colclass = (gv[13] != null && gv[13].AsCharacter() != null && gv[13].AsCharacter()[0] != null) ? gv[13].AsCharacter()[0].ToString() : string.Empty;

                        if (colclass == null)
                        {
                            colclass = "";
                        }

                        string lab = (gv[2] != null && gv[2].AsCharacter() != null && gv[2].AsCharacter()[0] != null) ? gv[2].AsCharacter()[0].ToString() : string.Empty;
                        DataColumnTypeEnum dtyp = (gv[1] != null && gv[1].AsCharacter() != null && gv[1].AsCharacter()[0] != null) ? GetCovertedDataType(gv[1].AsCharacter()[0].ToString()) : DataColumnTypeEnum.Character;
                        string mistyp = (gv[5] != null && gv[5].AsCharacter() != null && gv[5].AsCharacter()[0] != null) ? gv[5].AsCharacter()[0].ToString() : string.Empty;

                        DataSourceVariable var = new DataSourceVariable()
                        {
                            RName = colnm,
                            Label = lab,
                            DataType = dtyp,
                            DataClass = colclass,
                            Measure = DataColumnMeasureEnum.Scale,
                            Width = 4,
                            Decimals = 0,
                            Columns = 8,
                            MissType = mistyp,
                            RowCount = rowcount

                        };

                        if (symex != null)
                        {
                            {
                                ////Set Measure
                                switch (gv[7].AsCharacter()[0].ToString())
                                {
                                    case "factor":
                                        var.Measure = DataColumnMeasureEnum.Nominal;
                                        break;

                                    case "ordinal":
                                        var.Measure = DataColumnMeasureEnum.Ordinal;
                                        break;

                                    default:
                                        if (var.DataType == DataColumnTypeEnum.Character) //06Feb2017 Treating character as String. //02Jun2015 treating "character" type as Nominal in UI. In R its not factor
                                        {
                                            var.Measure = DataColumnMeasureEnum.String;
                                        }
                                        else if (var.DataType == DataColumnTypeEnum.Logical
                                            || var.DataClass == "logical")
                                        {
                                            var.Measure = DataColumnMeasureEnum.Logical;
                                        }
                                        else if (var.DataType == DataColumnTypeEnum.Date
                                            || var.DataClass == "POSIXct" || var.DataClass == "Date")
                                        {
                                            var.Measure = DataColumnMeasureEnum.Date;
                                        }
                                        else
                                            var.Measure = DataColumnMeasureEnum.Scale;
                                        break;
                                }
                                CharacterVector cv = gv[3].AsCharacter();
                                string[] vals = cv.ToArray();

                                if (var.DataClass == "logical") vals = new string[] { "True", "False", "<NA>" };//10Oct2015 Fix for logical DataClass

                                if (vals != null && vals.Length > 0)
                                {
                                    if (vals.Length > 1)
                                    {
                                        var.Values.AddRange(vals);//more than 1 strings
                                    }
                                    else if (vals[0].Trim().Length > 0)
                                    {
                                        var.Values.Add(vals[0]);//1 string
                                    }

                                    //replace true by  AdvancedLogging in following
                                    if (true && (var.Measure == DataColumnMeasureEnum.Nominal ||
                                        var.Measure == DataColumnMeasureEnum.Ordinal ||
                                        var.Measure == DataColumnMeasureEnum.Logical))
                                    {
                                        StringBuilder sbm = new StringBuilder(var.Name + ": {");
                                        sbm.Append(string.Join(",", vals));
                                        sbm.Append("}");
                                        if(AdvancedLogging) logService.WriteToLogLevel("Set levels for a factor variable : " + sbm.ToString(), LogLevelEnum.Info);
                                    }
                                }
                                

                                //Set Missing values
                                if (!(var.MissType == "none"))
                                {
                                    CharacterVector cvv = gv[4].AsCharacter();
                                    string[] misvals = cvv.ToArray();

                                    if (misvals != null && misvals.Length > 0)
                                    {
                                        if (misvals.Length > 1)
                                        {
                                            var.Missing.AddRange(misvals);//more than 1 strings
                                        }
                                        else if (misvals[0].Trim().Length > 0)
                                        {
                                            var.Missing.Add(misvals[0]);//1 string
                                        }
                                    }
                                }
                                else
                                {
                                    string misval = "none";
                                    var.Missing.Add(misval);
                                }
                            }
                        }

                        if (dataSource.Extension == "rdata")// if filetype is RDATA.
                        {
                            if (gv[9].AsCharacter() != null && gv[9].AsCharacter()[0].ToString() != "-2146826288")
                                var.Width = Int32.Parse(gv[9].AsCharacter()[0].ToString());

                            if (gv[10].AsCharacter() != null && gv[10].AsCharacter()[0].ToString() != "-2146826288")
                                var.Decimals = Int32.Parse(gv[10].AsCharacter()[0].ToString());

                            if (gv[11].AsCharacter() != null && gv[11].AsCharacter()[0].ToString() != "-2146826288")
                                var.Columns = UInt32.Parse(gv[11].AsCharacter()[0].ToString());
                        }

                        try
                        {
                            ////////// Alignment  ////////////
                            string align = gv[6].AsCharacter()[0].ToString();

                            if (align == "-2146826288") align = "Left";

                            DataColumnAlignmentEnum alignVal = (DataColumnAlignmentEnum)Enum.Parse(typeof(DataColumnAlignmentEnum), align);

                            if (Enum.IsDefined(typeof(DataColumnAlignmentEnum), alignVal))
                                var.Alignment = alignVal;
                            else
                                var.Alignment = DataColumnAlignmentEnum.Left;

                            var.Role = DataColumnRole.Input;// Role is not used, I guess, so 'if' is commented above
                        }
                        catch (ArgumentException)
                        {
                            logService.WriteToLogLevel("Not a member of enum(Alignment) ", LogLevelEnum.Error);
                        }


                        //Add a clone of the var object
                        if (dataSource.Variables.Count < _maxcolstoload)
                        {
                            DataSourceVariable var2 = new DataSourceVariable()
                            {
                                RName = var.RName,   //23Jan2017 may fix filter_. in layers issue in Crosstab
                                Label = var.Label,
                                DataType = var.DataType,
                                DataClass = var.DataClass,
                                Measure = var.Measure,
                                Width = 4,
                                Decimals = 0,
                                Columns = 8,
                                MissType = var.MissType,
                                RowCount = var.RowCount,
                                Alignment = var.Alignment

                            };
                            dataSource.FewVariables.Add(var2);
                        }

                        dataSource.Variables.Add(var);
                        columnindex++;
                        dataSource.RowCount = Math.Max(dataSource.RowCount, dataSource.Variables.Last().RowCount);
                    }
                    stopwatch.Stop();
                    long elapsed = stopwatch.ElapsedMilliseconds;

                    if (AdvancedLogging) logService.WriteToLogLevel("PERFORMANCE:for loop Reading Col Attributes and Creating DataSourceVariable: Time taken: " + elapsed, LogLevelEnum.Info);

                    result.Datasource = dataSource;
                    result.Success = true;

                    UpdateDataSources(dataSource);//26Oct2016 use this instead of line above
                }
                else
                {
                }
            }
            logService.Info("After getting column attributes for all columns.");

            return result;
        }

        //26Oct2016 Remove duplicate item and add new one instead
        private void UpdateDataSources(ServerDataSource newdataSource)
        {
            //Following two will be used as a key
            string filenamepath = newdataSource.FileNameWithPath;
            string datasetname = newdataSource.Name;
            int count = this.DataSources.Count;
            ServerDataSource temp;

            for (int i = 0; i < count; i++) //find a match and remove it
            {
                temp = this.DataSources[i];
                if (temp.Name.Trim().Equals(datasetname) && temp.FileNameWithPath.Trim().Equals(filenamepath))
                {
                    this.DataSources.RemoveAt(i);
                    break;
                }
            }
            //Add new datasource
            this.DataSources.Add(newdataSource);
        }

        //get all col properties at once.
        public UAReturn RefreshDataset_allcol(ServerDataSource dataSource)//04Mar2015 refresh on new row added by compute
        {
            Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();
            Stopwatch stopwatch2 = System.Diagnostics.Stopwatch.StartNew();

            logService.Info("Before getting column attributes for all columns.");
            UAReturn result = new UAReturn() { Success = false };

            if (true)
            {

                //Get matrix columns
                string subCommand = RCommandStrings.GetDataFrameColumnNames(dataSource);

                object colnames = dispatcher.EvaluateToObject(subCommand, false);
                //if colnames are null. Because we were unable to open the dataset because of any reason.
                if (colnames == null)
                {
                    CloseDataset(dataSource);
                    result.Success = false;
                    result.Error = "Error Opening Dataset.";
                    return result;
                }

                //if colnames are duplicate then we dont load the dataset and show error message. Also we clean R
                if (isDuplicateColnames(colnames))// 'colnames' here, appears to working as pass by reference.
                {
                    CloseDataset(dataSource);
                    result.Success = false;
                    result.Error = "Duplicate Column Names in Dataset";
                    return result;
                }
                if (colnames != null)
                {
                    string[] columnNames = null;
                    Type retType = colnames.GetType();

                    if (retType.Name == "String[]")//for multicols
                    {
                        columnNames = (String[])colnames;
                    }
                    else if (retType.Name == "String")//for single col
                    {
                        columnNames = new string[1];
                        columnNames[0] = (String)colnames;
                    }
                    else
                    {
                        return new UAReturn() { Success = false };
                    }

                    //maximum factors allowed
                    object maxf = (getMaxFactors(dataSource));
                    //sym = maxf as SymbolicExpression;

                    int mxf;//sym.AsInteger()[0];
                    bool parseSuccess = int.TryParse(maxf.ToString(), out mxf);
                    dataSource.MaxFactors = parseSuccess ? mxf : 40; //Hardcoded Default max factor count   //int.Parse(maxf.ToString());

                    dataSource.Variables.Clear();
                    int rowcount = GetRowCount(dataSource);//31Dec2014

                    //14Apr2016
                    if (rowcount < 1) 
                    {
                        CloseDataset(dataSource);
                        result.Success = false;
                        result.Error = "Duplicate Column Names OR NULL Dataset";
                        return result;
                    }

                    int columnindex = 1; long elapsed2 = 0;
                    string colnm = string.Empty;
                    SymbolicExpression symex = null;
                    string DSvalidated = dispatcher.RawEvaluateGetstring(string.Format("BSkyValidateDataset('{0}')", dataSource.Name));//,true);

                    //Get All col Prop at once
                    SymbolicExpression subsymex = null;

                    if (DSvalidated != null)
                    {
                        stopwatch2.Restart();
                        symex = dispatcher.EvaluateToSymExp(string.Format("BSkyGetAllColsProperties(datasetNameOrIndex='{0}', isDSValidated=TRUE)", dataSource.Name));
                        stopwatch2.Stop();
                        elapsed2 = stopwatch2.ElapsedMilliseconds;
                        if (AdvancedLogging) logService.WriteToLogLevel("PERFORMANCE: outside 'for'. get all col properties for only first  Col=" + colnm + " :: Time taken: " + elapsed2, LogLevelEnum.Info);
                    }
                    else
                    {
                        symex = dispatcher.EvaluateToSymExp(string.Format("BSkyGetAllColsProperties(datasetNameOrIndex='{0}')", dataSource.Name));
                    }

                    stopwatch.Restart();

                    bool once = true;

                    foreach (object s in columnNames)
                    {
                        if (s != null)
                            colnm = s.ToString();
                        else
                        {
                            colnm = "VAR" + columnindex.ToString();
                        }
                        subsymex = symex.AsList()[columnindex - 1] as SymbolicExpression;

                        GenericVector gv = subsymex.AsList();// symex.AsList();

                        string colclass = (gv[13] != null && gv[13].AsCharacter() != null && gv[13].AsCharacter()[0] != null) ? gv[13].AsCharacter()[0].ToString() : string.Empty;

                        if (colclass == null)
                        {
                            colclass = "";
                        }

                        string lab = (gv[2] != null && gv[2].AsCharacter() != null && gv[2].AsCharacter()[0] != null) ? gv[2].AsCharacter()[0].ToString() : string.Empty;
                        DataColumnTypeEnum dtyp = (gv[1] != null && gv[1].AsCharacter() != null && gv[1].AsCharacter()[0] != null) ? GetCovertedDataType(gv[1].AsCharacter()[0].ToString()) : DataColumnTypeEnum.Character;
                        string mistyp = (gv[5] != null && gv[5].AsCharacter() != null && gv[5].AsCharacter()[0] != null) ? gv[5].AsCharacter()[0].ToString() : string.Empty;

                        DataSourceVariable var = new DataSourceVariable()
                        {
                            RName = colnm,
                            Label = lab,
                            DataType = dtyp,
                            DataClass = colclass,
                            Measure = DataColumnMeasureEnum.Scale,
                            Width = 4,
                            Decimals = 0,
                            Columns = 8,
                            MissType = mistyp,
                            RowCount = rowcount 
                        };

                        if (symex != null)
                        {
                            {
                                ////Set Measure
                                switch (gv[7].AsCharacter()[0].ToString())
                                {
                                    case "factor":
                                        var.Measure = DataColumnMeasureEnum.Nominal;
                                        break;

                                    case "ordinal":
                                        var.Measure = DataColumnMeasureEnum.Ordinal;
                                        break;

                                    default:
                                        if (var.DataType == DataColumnTypeEnum.Character) //06Feb2017 Treating character as String. //02Jun2015 treating "character" type as Nominal in UI. In R its not factor
                                        {
                                            var.Measure = DataColumnMeasureEnum.String;
                                        }
                                        else if (var.DataType == DataColumnTypeEnum.Logical
                                            || var.DataClass == "logical")
                                        {
                                            var.Measure = DataColumnMeasureEnum.Nominal;
                                        }
                                        else
                                            var.Measure = DataColumnMeasureEnum.Scale;
                                        break;
                                }

                                CharacterVector cv = gv[3].AsCharacter();
                                string[] vals = cv.ToArray();

                                if (var.DataClass == "logical") vals = new string[] { "True", "False", "<NA>" };//10Oct2015 Fix for logical DataClass

                                if (vals != null && vals.Length > 0)
                                {
                                    if (vals.Length > 1)
                                    {
                                        var.Values.AddRange(vals);//more than 1 strings
                                    }
                                    else if (vals[0].Trim().Length > 0)
                                    {
                                        var.Values.Add(vals[0]);//1 string
                                    }
                                }

                                if (!(var.MissType == "none"))
                                {
                                    CharacterVector cvv = gv[4].AsCharacter();
                                    string[] misvals = cvv.ToArray();

                                    if (misvals != null && misvals.Length > 0)
                                    {
                                        if (misvals.Length > 1)
                                        {
                                            var.Missing.AddRange(misvals);//more than 1 strings
                                        }
                                        else if (misvals[0].Trim().Length > 0)
                                        {
                                            var.Missing.Add(misvals[0]);//1 string
                                        }
                                    }
                                }
                                else
                                {
                                    string misval = "none";
                                    var.Missing.Add(misval);
                                }
                            }
                        }

                        if (dataSource.Extension == "rdata")// if filetype is RDATA.
                        {
                            if (gv[9].AsCharacter() != null && gv[9].AsCharacter()[0].ToString() != "-2146826288")
                                var.Width = Int32.Parse(gv[9].AsCharacter()[0].ToString());

                            if (gv[10].AsCharacter() != null && gv[10].AsCharacter()[0].ToString() != "-2146826288")
                                var.Decimals = Int32.Parse(gv[10].AsCharacter()[0].ToString());

                            if (gv[11].AsCharacter() != null && gv[11].AsCharacter()[0].ToString() != "-2146826288")
                                var.Columns = UInt32.Parse(gv[11].AsCharacter()[0].ToString());
                        }

                        try
                        {
                            ////////// Alignment  ////////////
                            string align = gv[6].AsCharacter()[0].ToString();

                            if (align == "-2146826288") align = "Left";

                            DataColumnAlignmentEnum alignVal = (DataColumnAlignmentEnum)Enum.Parse(typeof(DataColumnAlignmentEnum), align);

                            if (Enum.IsDefined(typeof(DataColumnAlignmentEnum), alignVal))
                                var.Alignment = alignVal;
                            else
                                var.Alignment = DataColumnAlignmentEnum.Left;

                            var.Role = DataColumnRole.Input;// Role is not used, I guess, so 'if' is commented above

                        }
                        catch (ArgumentException)
                        {
                            logService.WriteToLogLevel("Not a member of enum(Alignment) ", LogLevelEnum.Error);
                        }

                        //Add a clone of the var object
                        if (dataSource.Variables.Count < _maxcolstoload)
                        {
                            DataSourceVariable var2 = new DataSourceVariable()
                            {
                                RName = var.Name,
                                Label = var.Label,
                                DataType = var.DataType,
                                DataClass = var.DataClass,
                                Measure = var.Measure,
                                Width = 4,
                                Decimals = 0,
                                Columns = 8,
                                MissType = var.MissType,
                                RowCount = var.RowCount,
                                Alignment = var.Alignment

                            };
                            dataSource.FewVariables.Add(var2);
                        }

                        dataSource.Variables.Add(var);
                        columnindex++;
                        dataSource.RowCount = Math.Max(dataSource.RowCount, dataSource.Variables.Last().RowCount);
                    }
                    stopwatch.Stop();
                    long elapsed = stopwatch.ElapsedMilliseconds;

                    if (AdvancedLogging) logService.WriteToLogLevel("PERFORMANCE:for loop Reading Col Attributes and Creating DataSourceVariable: Time taken: " + elapsed, LogLevelEnum.Info);

                    result.Datasource = dataSource;
                    result.Success = true;

                    //26Oct2016 this.DataSources.Add(dataSource);
                    UpdateDataSources(dataSource);//26Oct2016 use this instead of line above
                }
                else 
                {
                }
            }
            logService.Info("After getting column attributes for all columns.");

            return result;
        }

        public UAReturn RefreshDataset_old(ServerDataSource dataSource)//25Mar2013 refresh on new row added by compute
        {
            UAReturn result = new UAReturn() { Success = false };

            if (true)
            {
                logService.WriteToLogLevel("GetColnames Start:", LogLevelEnum.Info);
                //Get matrix columns
                string subCommand = RCommandStrings.GetDataFrameColumnNames(dataSource);

                object colnames = dispatcher.EvaluateToObject(subCommand, false);
                string[] columnNames = null;//var columnNames = new string[] { "aaa", "bbb" };
                Type retType = colnames.GetType();

                if (retType.Name == "String[]")//for multicols
                {
                    columnNames = (String[])colnames;
                }
                else if (retType.Name == "String")//for single col
                {
                    columnNames = new string[1];
                    columnNames[0] = (String)colnames;
                }
                else
                {
                    return new UAReturn() { Success = false };
                }

                logService.WriteToLogLevel("GetColnames End", LogLevelEnum.Info);

                logService.WriteToLogLevel("Get Max factor start", LogLevelEnum.Info);
                //maximum factors allowed
                object maxf = (getMaxFactors(dataSource));

                int mxf;
                bool parseSuccess = int.TryParse(maxf.ToString(), out mxf);
                dataSource.MaxFactors = parseSuccess ? mxf : 40; 
                logService.WriteToLogLevel("Get Max factor end", LogLevelEnum.Info);

                dataSource.Variables.Clear();
                int rowcount = GetRowCount(dataSource);//31Dec2014
                int columnindex = 1;

                foreach (object s in columnNames)
                {
                    logService.WriteToLogLevel("Get Col Prop start : " + s.ToString(), LogLevelEnum.Info);

                    object resobj = GetColProp(dataSource, s.ToString()).SimpleTypeData;
                    object[] cprops = (object[])resobj;
                    logService.WriteToLogLevel("Get Col Prop end : " + s.ToString(), LogLevelEnum.Info);

                    logService.WriteToLogLevel("Set Col Prop start : " + s.ToString(), LogLevelEnum.Info);
                    DataSourceVariable var = new DataSourceVariable()
                    {
                        Name = s.ToString(),
                        Label = cprops[2].ToString(),
                        DataType = GetCovertedDataType(cprops[1].ToString()),
                        Measure = DataColumnMeasureEnum.Scale,
                        Width = 4,
                        Decimals = 0,
                        Columns = 8,
                        MissType = cprops[5].ToString(),
                        RowCount = rowcount 
                    };
                    logService.WriteToLogLevel("Set Col Prop end : " + s.ToString(), LogLevelEnum.Info);

                    logService.WriteToLogLevel("Get-Set Col factors start : " + s.ToString(), LogLevelEnum.Info);

                    bool isfactors = (bool)dispatcher.EvaluateToObject(string.Format("is.factor({0}[,{1}])", dataSource.Name, columnindex), false);

                    if (isfactors)
                    {    

                        bool isOrdered = (bool)dispatcher.EvaluateToObject(string.Format("is.ordered({0}[,{1}])", dataSource.Name, columnindex), false);

                        if (isOrdered)
                            var.Measure = DataColumnMeasureEnum.Ordinal;
                        else
                            var.Measure = DataColumnMeasureEnum.Nominal;
                        //reading all levels/factors
                        object tempO = (object)GetFactorValues(dataSource, s.ToString()).SimpleTypeData;

                        if (tempO != null)
                        {
                            if (tempO.GetType().Name.Equals("String[]"))
                            {
                                string[] vals = tempO as string[];
                                var.Values.AddRange(vals);//adding all values to list
                            }
                            else if (tempO.GetType().Name.Equals("String"))
                            {
                                string vals = tempO as string;
                                var.Values.Add(vals);//adding all values to list
                            }
                            else
                            {
                                //some other unexpected type was returned in tempO.
                                //can print an error message here.
                                string[] charfactors = (tempO as SymbolicExpression).AsCharacter().ToArray();
                                var.Values.AddRange(charfactors);//adding all values to list
                            }
                        }
                    }
                    logService.WriteToLogLevel("Get-Set Col factors end : " + s.ToString(), LogLevelEnum.Info);

                    logService.WriteToLogLevel("Get-Set Col Missing start : " + s.ToString(), LogLevelEnum.Info);
                    if (!(var.MissType == "none"))
                    {
                        object tempObj = (object)GetMissingValues(dataSource, s.ToString()).SimpleTypeData;

                        if (tempObj != null)
                        {
                            double[] misval;

                            if (tempObj.GetType().Name.Equals("Double[]"))
                            {
                                misval = tempObj as double[];
                                foreach (double mv in misval)
                                    var.Missing.Add(mv.ToString());
                            }
                            else if (tempObj.GetType().Name.Equals("Double"))
                            {
                                double misvalue = (double)tempObj;
                                var.Missing.Add(misvalue.ToString());
                            }
                            else
                            {
                                var.Missing.Add("");//adding blank
                            }
                        }
                    }
                    else
                    {
                        string misval = "none";
                        var.Missing.Add(misval);
                    }
                    logService.WriteToLogLevel("Get-Set Col Missing end : " + s.ToString(), LogLevelEnum.Info);

                    logService.WriteToLogLevel("Get-Set others start : " + s.ToString(), LogLevelEnum.Info);
                    if (dataSource.Extension == "rdata")// if filetype is RDATA.
                    {
                        if (cprops[9].ToString() != "-2146826288")
                            var.Width = Int32.Parse(cprops[9].ToString());

                        if (cprops[10].ToString() != "-2146826288")
                            var.Decimals = Int32.Parse(cprops[10].ToString());

                        if (cprops[11].ToString() != "-2146826288")
                            var.Columns = UInt32.Parse(cprops[11].ToString());
                    }
                    try
                    {
                        ////////// Alignment  ////////////
                        if (cprops[6].ToString() == "-2146826288") cprops[6] = "Left";
                        DataColumnAlignmentEnum alignVal = (DataColumnAlignmentEnum)Enum.Parse(typeof(DataColumnAlignmentEnum), cprops[6].ToString());

                        if (Enum.IsDefined(typeof(DataColumnAlignmentEnum), alignVal))
                            var.Alignment = alignVal;
                        else
                            var.Alignment = DataColumnAlignmentEnum.Left;
                        var.Role = DataColumnRole.Input;// Role is not used, I guess, so 'if' is commented above
                    }
                    catch (ArgumentException)
                    {
                        //Console.WriteLine("Not a member of the enumeration.");
                        logService.WriteToLogLevel("Not a member of enum(Alignment) ", LogLevelEnum.Error);
                    }
                    logService.WriteToLogLevel("Get-Set others end : " + s.ToString(), LogLevelEnum.Info);
                    dataSource.Variables.Add(var);
                    columnindex++;
                    dataSource.RowCount = Math.Max(dataSource.RowCount, dataSource.Variables.Last().RowCount);
                }

                result.Datasource = dataSource;
                result.Success = true;

                this.DataSources.Add(dataSource);
            }
            return result;
        }

        public override UAReturn DataSourceReadRows(ServerDataSource dataSource, int start, int end)
        {
            UAReturn result = new UAReturn();
            start++; end++;

            List<object> data = new List<object>();

            for (int i = start; i < end; i++)
            {
                data.Add(dispatcher.EvaluateToObject(RCommandStrings.GetDataFrameRowValues(dataSource, i), false));
            }
            return result;
        }

        public override UAReturn DataSourceReadCell(ServerDataSource dataSource, int row, int col)
        {
            UAReturn result = new UAReturn();
            //R is now 0 Based
            row++; col++;
            result.Data = dispatcher.EvaluateToXml(RCommandStrings.GetDataFrameCellValue(dataSource, row, col));
            return result;
        }

        public override UAReturn DataSourceReadRow(ServerDataSource dataSource, int row)//23Jan2014 Read a Row at once
        {
            UAReturn result = new UAReturn();
            //R is now 0 Based
            row++;
            try
            {
                string commnd = RCommandStrings.GetDataFrameRowValues(dataSource, row);
                result.SimpleTypeData = dispatcher.GetRow(commnd);
            }
            catch (Exception ex)
            {
                logService.WriteToLogLevel("Error in row : " + row + " " + ex.Message, LogLevelEnum.Error);
            }
            return result;
        }

        public override UAReturn EmptyDataSourceLoad(ServerDataSource dataSource)//03Jan2014
        {
            return this.OpenEmptyDataset(dataSource);
        }

        public override UAReturn DataSourceLoad(ServerDataSource dataSource, string sheetname, bool removeSpacesSPSS=false)//, IOpenDataFileOptions odfo
        {
            return this.OpenDataset(dataSource, sheetname, removeSpacesSPSS);//, odfo
        }

        public override UAReturn DataFrameLoad(ServerDataSource dataSource, string dframename) //13Feb2014 
        {
            return this.LoadDataFrame(dataSource, dframename);
        }

        public override UAReturn GetRodbcTables(string fname) //27Jan2014
        {
            bool isxlsx = fname.ToLower().EndsWith(".xlsx") ? true : false;
            UAReturn result = new UAReturn();
            result.SimpleTypeData = GetODBCTableList(fname, isxlsx);
            return result;
        }

        public override UAReturn GetRDataDfObjList(string fname) //27Jan2014
        {
            UAReturn result = new UAReturn();
            result.SimpleTypeData = GetRDataDfObjectList(fname);
            return result;
        }

        public override UAReturn GetSQLTablelist(string sqlcom) //27Jan2014
        {
            _journal.WriteLine(sqlcom);

            if(UserLogFilter(sqlcom))
            _userJournal.WriteUserCommands(sqlcom);//12Aug2016

            UAReturn result = new UAReturn();
            result.Success = true;
            result.Data = dispatcher.EvaluateToXml(sqlcom);
            return result;
        }

        public override UAReturn DataSourceRefresh(ServerDataSource dataSource)//25Mar2013
        {
            return this.RefreshDataset(dataSource);
        }

        public override UAReturn DatasetSaveas(ServerDataSource dataSource)//Anil #3
        {
            return this.saveDataset(dataSource);
        }

        public override UAReturn CloseDataset(ServerDataSource dataSource)//Anil #3
        {
            return this.Datasetclose(dataSource);
        }

        public override UAReturn DataSourceSave(ServerDataSource dataSource)
        {
            return new UAReturn() { Datasource = dataSource };
        }

        #endregion

        #region ServerCommand Execution Support
        public override UAReturn Execute(string commandString)
        {
            UAReturn result = new UAReturn();

            try
            {
                _journal.WriteLine(commandString);//06Jul2015
                if (UserLogFilter(commandString))
                    _userJournal.WriteUserCommands(commandString);//12Aug2016

                result.Data = dispatcher.EvaluateToXml(commandString);
                result.Success = true;
            }
            catch (Exception ex)
            {
                logService.WriteToLogLevel("Could not evaluate : <" + commandString + " > ", LogLevelEnum.Error);
            }
            return result;
        }

        public override UAReturn Execute(ServerCommand Command)
        {
            _journal.WriteLine(Command.CommandSyntax);
            if (UserLogFilter(Command.CommandSyntax))
                _userJournal.WriteUserCommands(Command.CommandSyntax);//12Aug2016

            UAReturn result = new UAReturn();
            result.Success = true;
            result.Data = dispatcher.EvaluateToXml(Command.CommandSyntax);

            return result;
        }

        public override object ExecuteR(ServerCommand Command, bool hasReturn, bool hasUAReturn)
        {
            _journal.WriteLine(Command.CommandSyntax);
            if (UserLogFilter(Command.CommandSyntax))
                _userJournal.WriteUserCommands(Command.CommandSyntax);//12Aug2016

            UAReturn result = new UAReturn();
            result.Success = true;
            result.Data = null;
            object o = dispatcher.SyntaxEditorEvaluateToObject(Command.CommandSyntax, hasReturn, hasUAReturn);

            return o;
        }

        public override UAReturn DontExecuteJustLogCommand(string commandString)//16Aug2016
        {
            UAReturn result = new UAReturn();

            try
            {
                _journal.WriteLine(commandString);//06Jul2015
                if (UserLogFilter(commandString))
                    _userJournal.WriteUserCommands(commandString);//12Aug2016
            }
            catch (Exception ex)
            {
                logService.WriteToLogLevel("Could not evaluate : <" + commandString + " > ", LogLevelEnum.Error);
            }
            return result;
        }

        #endregion

        //27Oct2016
        #region R Object save / Load

        public override UAReturn GetAll_RObjects()
        {
            UAReturn result = new UAReturn() { Success = false };
            string rcommand = string.Empty;
            string colnamecollection = string.Empty; ;// "NULL";
            rcommand = string.Format("ls()");

            SymbolicExpression symex = null;
            symex = dispatcher.EvaluateToSymExp(rcommand);
            DynamicVector retobj = symex.AsVector();
            GenericVector gv = symex.AsList();
            int count = gv.Count();
            string objname = string.Empty;
            string[] objnames = new string[count];

            for (int i = 0; i < count; i++)
            {
                objname = (gv[i] != null && gv[i].AsCharacter() != null && gv[i].AsCharacter()[0] != null) ? gv[i].AsCharacter()[0].ToString() : string.Empty;
                objnames[i] = objname;
            }

            result.SimpleTypeData = objnames;
            result.Success = true;
            return (result);
        }

        public override UAReturn Save_RObjects(string objname, string fullpathfilename)
        {
            UAReturn result = new UAReturn() { Success = false };
            string rcommand = string.Empty;
            string colnamecollection = string.Empty; ;// "NULL";
            rcommand = string.Format("base::save({0}, file='{1}')", objname, fullpathfilename.Replace("\\", "/"));

            SymbolicExpression symex = null;
            symex = dispatcher.EvaluateToSymExp(rcommand);

            result.Success = true;
            return (result);
        }

        public override UAReturn Load_RObjects(string fullpathfilename)
        {
            string modelname = string.Empty;
            string modelclass = string.Empty;
            UAReturn result = new UAReturn() { Success = false };
            string rcommand = string.Empty;
            string colnamecollection = string.Empty; ;// "NULL";
            rcommand = string.Format("base::load(file='{0}', verbose=TRUE)", fullpathfilename.Replace("\\", "/"));

            SymbolicExpression symex = null;
            symex = dispatcher.EvaluateToSymExp(rcommand);
            if (symex.Type.ToString().Equals("CharacterVector"))
            {
                //0 index for modelname and remaining for its class names(right now only first class is considered but later we can enhance)
                int arrsize = 2; 
                string[] retval = new string[arrsize];
                modelname = symex.AsCharacter()[0];
                retval[0] = modelname;
                rcommand = "class(" + modelname + ")";
                symex = dispatcher.EvaluateToSymExp(rcommand);
                if (symex.Type.ToString().Equals("CharacterVector"))
                {
                    modelclass = symex.AsCharacter()[0];
                    retval[1] = modelclass;//here we can use loop to add more classes if a model has multiple classnames.

                    result.SimpleTypeData = retval;//return model name and class(later multiple classes) so that dropdowns can be refreshed.
                }
            }

            result.Success = true;
            return (result);
        }

        #endregion

        #region Package related

        private bool isNewPackage = true;

        private UAReturn LoadPackage()
        {
            UAReturn result = new UAReturn() { Success = false };

            foreach (string package in RPackages)
            {
                if (isNewPackage) UnloadPackage(package);
                //Load Package
                if (!dispatcher.IsLoaded(package))
                {
                    string parentDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location).Replace('\\', '/') + "/R Packages";

                    this.EvaluateNoReturn("library(tools)");

                    string command = string.Format("write_PACKAGES('{0}')", parentDir);
                    this.EvaluateNoReturn(command);
                    this.EvaluateNoReturn(string.Format("install.packages('{0}', repos=NULL, contriburl='file:///{1}')", package, parentDir));

                    result.Success = true;
                    this.EvaluateNoReturn(string.Format("library({0})", package));
                }
            }

            return result;
        }

        private void LoadDefaultPackages() //uadatapackage(or bskypackage) AND other minimum required packages like foreign, car etc.
        {
            string installBSkyRpkg = confService.AppSettings.Get("installBSkyRPkg");
            if (installBSkyRpkg != null && installBSkyRpkg.ToLower().Equals("true"))
            {
                UAReturn res = CheckAndInstallBlueSkyRPackage(); //04May2015 Installs BlueSky R package(s) if latest not already installed

                if (res != null && !res.Success && res.SimpleTypeData != null && res.SimpleTypeData.ToString().Length > 0)
                {
                    logService.WriteToLogLevel("Error Loading BlueSky R package(s):", LogLevelEnum.Info);
                    logService.WriteToLogLevel(res.SimpleTypeData.ToString(), LogLevelEnum.Info);
                }
            }
            //06Oct2014 Also install DefaultPackages from DefaultPackages.xml
            logService.WriteToLogLevel("Loading Default R packages:", LogLevelEnum.Info);
            DefPkgMsg = LoadDefaultPackagesFromXML().CommandString; //06Nov2014 load package and store message for failed packages.
            logService.WriteToLogLevel("All minimum required R packages status:" + DefPkgMsg, LogLevelEnum.Info);
        }

        //06Oct2014 /bin/Config/DefaultPackages.xml
        private UAReturn LoadDefaultPackagesFromXML()
        {
            List<string> dfltpkgs = defaultPackageList.RecentFileList;

            string[] packagenames = new string[dfltpkgs.Count];
            int i = 0;

            foreach (string pkgname in dfltpkgs)
            {
                packagenames[i] = pkgname;
                i++;
            }

            UAReturn uar = rpm.LoadMultiplePackages(packagenames, false);
            return uar;
        }

        //Check if minimum required R packages are loaded or not. 
        private string GetListOfDefPkgsNotLoaded()
        {
            StringBuilder defpkgsnotloaded = new StringBuilder();
            bool isLoaded = true;//assuming all are loaded
            List<string> dfltpkgs = defaultPackageList.RecentFileList;
            string[] currentlyloaded = rpm.GetCurrentlyLoadedPackages();

            foreach (string deflt in dfltpkgs)
            {
                if (!currentlyloaded.Contains(deflt))
                {
                    isLoaded = false;
                    defpkgsnotloaded.Append(deflt + " - R package not loaded(or not installed)\n");
                }
            }
            if (!isLoaded)
            {
                return defpkgsnotloaded.ToString();
            }
            else
            {
                return string.Empty;
            }
        }

        private void LoadUserSessionPackages() //06Feb2014
        {
            List<string> usrsesspkgs = userPackageList.RecentFileList;

            string[] packagenames = new string[usrsesspkgs.Count];
            int i = 0;

            foreach (string pkgname in usrsesspkgs)
            {
                packagenames[i] = pkgname;
                i++;
            }
            rpm.LoadMultiplePackages(packagenames, false);
        }

        private void UnloadPackage(string packageName)//Parameter added by Anil
        {
            string parentDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location).Replace('\\', '/') + "/R Packages";
            this.EvaluateNoReturn("library(tools)");

            this.EvaluateNoReturn(@"detach(package:" + packageName + ")");
            this.EvaluateNoReturn(@"remove.packages('" + packageName + "')");
        }

        public override void LoadDefPacakges()
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();			
            LoadDefaultPackages();
            sw.Stop();
            long elapsed = sw.ElapsedMilliseconds;
            logService.WriteToLogLevel("Default R packages load time = " + elapsed.ToString(), LogLevelEnum.Info);
        }

        //06Dec2013 show installed packages
        public override UAReturn ShowInstalledPackages()
        {

            UAReturn result = new UAReturn() { Success = false };
            object obj = rpm.GetInstalledPackages();

            if (obj != null)
            {
                result.SimpleTypeData = obj;
                result.Success = true;
            }
            return result;
        }

        #region BlueSky R pacakge check and install

        //Install BlueSky R pacakge if its not already installed. Dont install on each launch. But check in each launch if installation is needed or not
        private UAReturn CheckAndInstallBlueSkyRPackage()
        {
            bool isinstalled = false;
            bool islatestinstalled = true; 
            UAReturn res = new UAReturn() { Success = false };

            //Get BlueSky R .zip package names and location
            string BSkyMainPackagePath = string.Empty;
            string BSkyMainPackagePath1 = string.Empty;
            string BSkyMainPackagePath2 = string.Empty;
            string[] RpackageFullPathFilenames = null;
            string parentDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location).Replace('\\', '/') + "/R Packages/BlueSky R Package(s)";
            bool isSingleBSkyPackageInDir = true;

            if (Directory.Exists(parentDir))
            {
                //find all the .zip file in RPacakges directory
                RpackageFullPathFilenames = Directory.GetFiles(parentDir.Replace('/', '\\'), "BlueSky*.zip");//only look for packages with names "BlueSky*.zip"

                if (RpackageFullPathFilenames != null && RpackageFullPathFilenames.Length > 1)
                    isSingleBSkyPackageInDir = false;

                if (isSingleBSkyPackageInDir)
                {
                    BSkyMainPackagePath = Array.Find(RpackageFullPathFilenames, element => element.Contains("BlueSky_"));
                }
                else 
                {
                    #region To handle multiple versions. Chose latest from the 'R Package(s)' directory.
                    BSkyMainPackagePath1 = Array.Find(RpackageFullPathFilenames, element => element.Contains("BlueSky_"));//this could be smallest version
                    BSkyMainPackagePath2 = Array.FindLast(RpackageFullPathFilenames, element => element.Contains("BlueSky_"));
                    string ver1, ver2;
                    string pkgname1 = rpm.GetPackageNameFromZip(BSkyMainPackagePath1, out ver1);
                    string pkgname2 = rpm.GetPackageNameFromZip(BSkyMainPackagePath2, out ver2);
                    int ans = rpm.CompareVersion(ver1, ver2);
                    if (ans == 1)//ver1 is latest
                    {
                        BSkyMainPackagePath = Array.Find(RpackageFullPathFilenames, element => element.Contains("BlueSky_"));
                    }
                    else //ver2 is latest or same
                    {
                        BSkyMainPackagePath = Array.FindLast(RpackageFullPathFilenames, element => element.Contains("BlueSky_"));
                    }
                    #endregion
                }
            }

            //Now check if BlueSky R pacakge is already installed.
            isinstalled = IsRPacakgeInstalled("BlueSky");
            logService.WriteToLogLevel("BlueSky R package found = " + isinstalled, LogLevelEnum.Info);
            if (isinstalled) 
            {
                string existingVersion = rpm.GetInstalledPacakgeVersion("BlueSky");
                if (existingVersion.ToLower().Equals("true"))
                {
                    isinstalled = false;
                    existingVersion = "0.0-0";
                }

                string newVersion = "0.0-0";
                if (!string.IsNullOrEmpty(BSkyMainPackagePath))
                {
                    string pkgname = rpm.GetPackageNameFromZip(BSkyMainPackagePath, out newVersion);
                }
                logService.WriteToLogLevel("Currently installed version: "+existingVersion, LogLevelEnum.Info);
                logService.WriteToLogLevel("Version in Setup: " + newVersion, LogLevelEnum.Info);

                if (rpm.CompareVersion(newVersion, existingVersion) == 1) //new version in BlueSky app and old is already installed
                {
                    islatestinstalled = false; //latest not already installed.
                }
            }

            //Install BlueSky R package(s) if not already installed OR if installed one is not the latest
            if (!isinstalled || !islatestinstalled)
            {
                logService.WriteToLogLevel("Install BlueSky R pacakge (if its not already installed or not latest):", LogLevelEnum.Info);

                if (Directory.Exists(parentDir) && RpackageFullPathFilenames!=null && RpackageFullPathFilenames.Length>0)
                {
                    logService.WriteToLogLevel("Installing BlueSky R package from zip.", LogLevelEnum.Info);
                    res = rpm.InstallMultiPackageFromZip(RpackageFullPathFilenames, true, true);
                }
                else
                {
                    res.Error = "Could not find BlueSky R package(s). Please install it by going to Tools > Package > Update BlueSky package from zip (Restart App)";
                }
            }
            else//set flag to success so that error message does not appear(BlueSky latest ver is already present.)
            {
                logService.WriteToLogLevel("BlueSky R package latest version already installed.", LogLevelEnum.Info);
                res.Success = true;
            }
            return res;
        }

        //04May2015 Gets a list a all installed R pacakges
        public List<string> GetInstalledPackageList()
        {
            UAReturn r = ShowInstalledPackages();
            List<string> installed = new List<string>();

            try
            {
                if (r != null && r.Success && r.SimpleTypeData != null)
                {
                    string[] strarr = null;

                    if (r.SimpleTypeData.GetType().Name.Equals("String"))
                    {
                        strarr = new string[1];
                        strarr[0] = r.SimpleTypeData as string;
                    }
                    else if (r.SimpleTypeData.GetType().Name.Equals("String[]"))
                    {
                        strarr = r.SimpleTypeData as string[];
                    }

                    //strarr to list
                    foreach (string s in strarr)
                        installed.Add(s);
                }
                else
                {
                    logService.WriteToLogLevel("Error Getting BlueSky install status!", LogLevelEnum.Error);
                }
            }
            catch (Exception ex)
            {
                logService.WriteToLogLevel("Error getting list of installed packages.", LogLevelEnum.Error);
                logService.WriteToLogLevel("Error:", LogLevelEnum.Error, ex);
            }
            return installed;
        }

        //04May2015 checks is particular R pacakge is installed or not
        public bool IsRPacakgeInstalled(string packagename)
        {
            bool found = false;

            List<string> installed = GetInstalledPackageList(); // get list off all installed R pacakges
            //now check the list
            if (installed.Contains(packagename.Trim()))
                found = true;

            return found;
        }

        #endregion

        //06Dec2013 show currently loaded packages
        public override UAReturn ShowLoadedPackages()
        {
            UAReturn result = new UAReturn() { Success = false };
            object obj = rpm.GetCurrentlyLoadedPackages();

            if (obj != null)
            {
                result.SimpleTypeData = obj;
                result.Success = true;
            }
            return result;
        }

        public override UAReturn GetMissingDefRPackages()
        {
            UAReturn result = new UAReturn() { Success = false };
            object obj = GetListOfDefPkgsNotLoaded();

            if (obj != null)
            {
                result.SimpleTypeData = obj;
                result.Success = true;
            }
            return result;
        }

        //06Dec2013 for installing custom package
        public override UAReturn InstallLocalPackage(string[] pkgfilenames, bool autoLoad = true, bool overwrite = false)//(string package, string filepath)
        {
            UAReturn result = null;
            result = rpm.InstallMultiPackageFromZip(pkgfilenames, autoLoad, overwrite);
            return result;
        }

        //06Dec2013 for installing CRAN package
        public override UAReturn InstallCRANPackage(string packagename)
        {
            UAReturn result = null;
            result = rpm.InstallPackageFromCRAN(packagename);
            return result;
        }

        //27Aug2015 for installing required package from CRAN using a function from BlueSky R pacakge
        public override UAReturn InstallReqPackageFromCRAN(string packagename)
        {
            UAReturn result = null;
            result = rpm.InstallReqPackageFromCRAN(packagename);
            return result;
        }

        public override UAReturn setCRANMirror()
        {
            UAReturn result = null;
            result = rpm.setCRANMirror();
            return result;
        }

        //06Dec2013 for loading custom package
        public override UAReturn LoadLocalPackage(string package)
        {
            string command = string.Format("library({0})", package);

            UAReturn result = new UAReturn() { Success = false };
            //Load Package
            if (!dispatcher.IsLoaded(package))
            {
                result.Success = true;
                this.EvaluateNoReturn(command);
                result.CommandString = command;//  "Load Package";
            }
            return result;
        }

        //06Dec2013 for loading package from list
        public override UAReturn LoadPackageFromList(string[] packagenames)
        {
            UAReturn result = new UAReturn() { Success = false };
            result = rpm.LoadMultiplePackages(packagenames, true);
            return result;
        }

        //06Dec2013 for unloading custom package
        public override UAReturn UnloadPackages(string[] packagenames)
        {
            UAReturn result = new UAReturn() { Success = false };
            result = rpm.UnLoadMultiPackage(packagenames);
            return result;
        }

        //06Dec2013 for uninstalling custom package
        public override UAReturn UninstallPackages(string[] packagenames)
        {
            UAReturn result = new UAReturn() { Success = false };
            result = rpm.UninstallMultiPakckage(packagenames);
            return result;
        }

        //12Feb2019 Fetch list of datasets in a R pkg
        public override UAReturn FetchRpkgDatasetList(string packagename)
        {
            UAReturn result = new UAReturn() { Success = false };
            List<RPkgDatasetDetails> dslist = rpm.GetDatasetListFromRPkg(packagename);
            if (dslist != null)
            {
                if (dslist.Count() == 1 && dslist[0].Title.Contains("ReRRoE"))
                {
                        result.Error = dslist[0].Title.Replace("ReRRoE","");
                }
                else
                {
                    result.Success = true;
                    result.SimpleTypeData = dslist;
                }
            }

            return result;
        }
        #endregion

        #region Helpers
        //get row count of dataset/dataframe ( each col should/must have same number of rows
        private int GetRowCount(ServerDataSource dataSource)
        {
            int rcount = 0;
            string cmd = string.Format("nrow({0})", dataSource.Name);
            object o = dispatcher.EvaluateToObject(cmd, false);

            if (o != null)
            {
                int.TryParse(o.ToString(), out rcount);
            }
            return rcount;
        }
        //get row count of each col
        private int GetVectorLength(ServerDataSource dataSource, string objectName)
        {
            int rcount = 0;
            string comm = RCommandStrings.GetDataFrameColumnLength(dataSource, objectName);
            object o = dispatcher.EvaluateToObject(comm, false);

            if (o != null)
                rcount = (int)o;
            return rcount;
        }

        private DataColumnTypeEnum GetDataType(ServerDataSource dataSource, string objectName)
        {
            //switch (dispatcher.EvaluateToXml(RCommandStrings.GetDataFrameColumnType(dataSource, objectName), false) as string)
            switch (dispatcher.EvaluateToObject(RCommandStrings.GetDataFrameColumnType(dataSource, objectName), false) as string)
            {
                case "integer": return DataColumnTypeEnum.Integer;
                case "numeric": return DataColumnTypeEnum.Numeric;
                case "double": return DataColumnTypeEnum.Double;//Anil added RDouble/Numeric in DataSources.cs
                case "factor": return DataColumnTypeEnum.Factor;
                case "character": return DataColumnTypeEnum.Character;
                case "logical": return DataColumnTypeEnum.Logical;
                case "POSIXlt": return DataColumnTypeEnum.POSIXlt;
                case "POSIXct": return DataColumnTypeEnum.POSIXct;
                case "Date": return DataColumnTypeEnum.Date;
                default: return DataColumnTypeEnum.Unknown;
            }
        }

        private DataColumnTypeEnum GetCovertedDataType(string objectName)
        {
            switch (objectName)
            {
                case "integer": return DataColumnTypeEnum.Integer;
                case "numeric": return DataColumnTypeEnum.Numeric;
                case "double": return DataColumnTypeEnum.Double;//Anil added RDouble/Numeric in DataSources.cs
                case "factor": return DataColumnTypeEnum.Factor;
                case "ordinal": return DataColumnTypeEnum.Ordinal;
                case "character": return DataColumnTypeEnum.Character;
                case "logical": return DataColumnTypeEnum.Logical;
                case "POSIXlt": return DataColumnTypeEnum.POSIXlt;
                case "POSIXct": return DataColumnTypeEnum.POSIXct;
                case "Date": return DataColumnTypeEnum.Date;
                default: return DataColumnTypeEnum.Unknown;
            }
        }


        private void EvaluateNoReturn(string commandString)
        {
            _journal.WriteLine(commandString);
            if(UserLogFilter(commandString))
            _userJournal.WriteUserCommands(commandString);//12Aug2016

            string errmsg = dispatcher.EvaluateNoReturn(commandString);

            if (errmsg != null && errmsg.Trim().Length < 1) //error occurred
            {
            }
        }

        private UAReturn Evaluate(string commandString)
        {
            _journal.WriteLine(commandString);
            if (UserLogFilter(commandString))
                _userJournal.WriteUserCommands(commandString);//12Aug2016

            //08jun2013
            UAReturn result = new UAReturn();
            //result.Success = true;
            result.Data = dispatcher.EvaluateToXml(commandString);
            result.Success = true;
            return result;
        }

        private object EvaluateToObject(string commandString, bool hasReturn)
        {
            try
            {
                _journal.WriteLine(commandString);
                if (UserLogFilter(commandString))
                    _userJournal.WriteUserCommands(commandString);//12Aug2016

                return dispatcher.EvaluateToObject(commandString, hasReturn);
            }
            catch (Exception ex)
            {
                logService.WriteToLogLevel(dispatcher.GetErrorText(), LogLevelEnum.Error, ex);
                throw new Exception(dispatcher.GetErrorText(), ex);
            }
        }

        private XmlDocument EvaluateToObjectXml(string commandString)
        {
            try
            {
                _journal.WriteLine(commandString);
                if (UserLogFilter(commandString))
                    _userJournal.WriteUserCommands(commandString);//12Aug2016

                return dispatcher.EvaluateToXml(commandString);
            }
            catch (Exception ex)
            {
                logService.WriteToLogLevel(dispatcher.GetErrorText(), LogLevelEnum.Error, ex);
                throw new Exception(dispatcher.GetErrorText(), ex);
            }
        }
        #endregion

        public UAReturn saveDataset(ServerDataSource dataSource)//, string fname, string ftype, string datasetNameOrIndex)//Anil #4
        {
            UAReturn result = new UAReturn() { Success = false };
            result.Datasource = dataSource;//need to send dataset name and filename to output
            result.CommandString = RCommandStrings.SaveDatasetToFile(dataSource);//fname,ftype,datasetNameOrIndex);
            logService.WriteToLogLevel("Before exeuting in R.NET. Commandstring: " + result.CommandString, LogLevelEnum.Info);
            if (result.CommandString != null)
            {
                UAReturn res1 = this.Evaluate(result.CommandString);
                result.Success = res1.Success;
            }

            logService.WriteToLogLevel("After exeuting in R.NET. Result: " + result, LogLevelEnum.Info);
            return result;
        }

        public UAReturn Datasetclose(ServerDataSource dataSource)//Anil #4
        {
            UAReturn result = new UAReturn() { Success = false };
            result.Datasource = dataSource;//need to send dataset name and filename to output
            result.CommandString = RCommandStrings.closeDataset(dataSource);
            if (result.CommandString != null)
            {
                UAReturn res1 = this.Evaluate(result.CommandString);
                result.Success = res1.Success;
            }
            return result;
        }

        #region Var grid Toplevel

        public override UAReturn editDatasetVarGrid(ServerDataSource dataSource, string colName, string colProp, string newValue, List<string> colLevels)//Anil
        {
            UAReturn result;

            if (colProp.Equals("Measure"))
                result = SetColMeasure(dataSource, colName, colProp, newValue, colLevels);
            else
                result = SetColProperty(dataSource, colName, colProp, newValue);
            return result;
        }

        public override UAReturn makeColumnFactor(ServerDataSource dataSource, string colName)//Anil
        {
            UAReturn result;
            result = makeColFactor(dataSource, colName);
            return result;
        }

        public override UAReturn makeColumnString(ServerDataSource dataSource, string colName)//Anil
        {
            UAReturn result;
            result = makeColString(dataSource, colName);
            return result;
        }

        //11Oct2017
        public override UAReturn makeColumnNumeric(ServerDataSource dataSource, string colName)//Anil
        {
            UAReturn result;
            result = makeColNumeric(dataSource, colName);
            return result;
        }

        public override UAReturn addNewColDatagrid(string colName, string rdataType, string dgridval, int rowindex, ServerDataSource dataSource)//add row in vargrid and col in datagrid
        {
            UAReturn result;

            if (colName == null || colName.Length < 1 || dgridval == null || dgridval.Length < 1)//12Jul2013
                return null;
            result = addNewCol(colName, rdataType, dgridval, rowindex, dataSource);//15Oct2015 modified

            return result;
        }

        public override UAReturn removeVarGridCol(string colName, ServerDataSource dataSource)
        {
            UAReturn result;
            result = removeVarCol(colName, dataSource);
            return result;
        }


        public override UAReturn changeColLevels(string colName, List<ValLvlListItem> finalLevelList, ServerDataSource dataSource)
        {
            UAReturn result;
            result = changeColLvl(colName, finalLevelList, dataSource);
            return result;
        }

        public override UAReturn addColLevels(string colName, List<string> finalLevelList, ServerDataSource dataSource)
        {
            UAReturn result;
            result = addColLvl(colName, finalLevelList, dataSource);
            return result;
        }

        public override UAReturn changeMissing(string colName, string colProp, List<string> newMisVal, string mistype, ServerDataSource dataSource)
        {
            UAReturn result;
            result = changeColMissing(colName, colProp, newMisVal, mistype, dataSource);
            return result;
        }

        public override object getColNumFactors(string colName, ServerDataSource dataSource)
        {
            return getColumnNumFactors(colName, dataSource); ;
        }

        public override UAReturn setScaleToNominalOrOrdinal(string colName, List<FactorMap> fmap, string changeTo, ServerDataSource dataSource)
        {
            return changeScaleToNominalOrOrdinal(colName, fmap, changeTo, dataSource); ;
        }

        public override List<FactorMap> getColFactormap(string colName, ServerDataSource dataSource)
        {
            UAReturn result = new UAReturn() { Success = false };
            List<FactorMap>
                factormap = getColumnFmap(colName, dataSource);

            return factormap;
            //
        }

        public override UAReturn setNominalOrOrdinalToScale(string colName, List<FactorMap> fmap, string changeTo, ServerDataSource dataSource)
        {
            return changeNominalOrOrdinalToScale(colName, fmap, changeTo, dataSource); ;
        }

        #endregion

        #region Data grid Toplevel

        public override UAReturn editDatagridCell(string colName, string celdata, int rowindex, ServerDataSource dataSource)
        {
            UAReturn result;
            result = changeDatagridCell(colName, celdata, rowindex, dataSource);
            return result;
        }

        public override UAReturn addNewDataRow(string colName, string celdata, string rowdata, int rowindex, ServerDataSource dataSource)
        {
            UAReturn result;
            result = addDatagridRow(colName, celdata, rowdata, rowindex, dataSource);
            return result;
        }

        public override UAReturn removeDatagridRow(int rowindex, ServerDataSource dataSource)
        {
            UAReturn result;
            result = delDatagridRow(rowindex, dataSource);
            return result;
        }


        public override UAReturn sortDatagridColumn(string colname, string sortorder, ServerDataSource dataSource)
        {
            UAReturn result;
            result = sortDatagridCol(colname, sortorder, dataSource);
            return result;
        }
        #endregion

        #region variablegrid Core

        private UAReturn GetLabel(ServerDataSource dataSource, string objectName)//Anil added
        {
            return (dispatcher.EvaluateToUAReturn(RCommandStrings.GetDataFrameColumnLabel(dataSource, objectName)));
        }

        private UAReturn SetColProperty(ServerDataSource dataSource, string colName, string colProp, string newValue)//Anil added
        {
            return (dispatcher.EvaluateToUAReturn(RCommandStrings.SetDataFrameColumnProp(dataSource, colName, colProp, newValue)));
        }

        private UAReturn makeColFactor(ServerDataSource dataSource, string colName)//Anil added
        {
            return (dispatcher.EvaluateToUAReturn(RCommandStrings.MakeDatasetColFactor(dataSource, colName)));
        }

        private UAReturn makeColString(ServerDataSource dataSource, string colName)//Anil added
        {
            return (dispatcher.EvaluateToUAReturn(RCommandStrings.MakeDatasetColString(dataSource, colName)));
        }

        //11Oct2017
        private UAReturn makeColNumeric(ServerDataSource dataSource, string colName)//Anil added
        {
            return (dispatcher.EvaluateToUAReturn(RCommandStrings.MakeDatasetColNumeric(dataSource, colName)));
        }

        private UAReturn SetColMeasure(ServerDataSource dataSource, string colName, string colProp, string newValue, List<string> colLevels)
        {
            return (dispatcher.EvaluateToUAReturn(RCommandStrings.SetDatasetMeasureProp(colName, newValue, colLevels, dataSource)));
        }

        private UAReturn addNewCol(string colName, string rdataType, string dgridval, int rowindex, ServerDataSource dataSource)
        {
            return (dispatcher.EvaluateToUAReturn(RCommandStrings.AddNewDatagridCol(colName, rdataType, dgridval, rowindex, dataSource)));
        }

        private UAReturn removeVarCol(string colName, ServerDataSource dataSource)
        {
            return (dispatcher.EvaluateToUAReturn(RCommandStrings.RemoveVargridrow(colName, dataSource)));
        }

        private UAReturn changeColLvl(string colName, List<ValLvlListItem> finalLevelList, ServerDataSource dataSource)
        {
            return (dispatcher.EvaluateToUAReturn(RCommandStrings.ChangeColumnLevels(colName, finalLevelList, dataSource)));
        }

        private UAReturn addColLvl(string colName, List<string> finalLevelList, ServerDataSource dataSource)
        {
            return (dispatcher.EvaluateToUAReturn(RCommandStrings.AddColumnLevels(colName, finalLevelList, dataSource)));
        }

        private UAReturn changeColMissing(string colName, string colProp, List<string> newMisVal, string mistype, ServerDataSource dataSource)
        {
            return (dispatcher.EvaluateToUAReturn(RCommandStrings.SetDatasetMissingProp(colName, colProp, newMisVal, mistype, dataSource)));
        }

        private object getColumnNumFactors(string colName, ServerDataSource dataSource)
        {
            return ((object)dispatcher.EvaluateToUAReturn(RCommandStrings.GetColNumericFactors(colName, dataSource)));
        }

        private UAReturn changeScaleToNominalOrOrdinal(string colName, List<FactorMap> fmap, string changeTo, ServerDataSource dataSource)
        {
            return (dispatcher.EvaluateToUAReturn(RCommandStrings.ScaleToNominalOrOrdinal(colName, fmap, changeTo, dataSource)));
        }

        private List<FactorMap> getColumnFmap(string colName, ServerDataSource dataSource)
        {
            Object num = ((object)dispatcher.EvaluateToObject(RCommandStrings.GetColFactorMap(colName, true, dataSource), false));//num vals
            Object str = ((object)dispatcher.EvaluateToObject(RCommandStrings.GetColFactorMap(colName, false, dataSource), false));//level names

            List<FactorMap> factormapList = new List<FactorMap>();

            if (num.GetType().Name == "Double[]" && str.GetType().Name == "String[]")
            {
                double[] numval = (double[])num;
                string[] strlvl = (string[])str;

                FactorMap fm;

                for (int i = 0; i < numval.Length; i++)
                {
                    fm = new FactorMap();
                    fm.labels = strlvl[i];
                    fm.textbox = numval[i].ToString();
                    factormapList.Add(fm);
                }
            }
            return factormapList;
        }

        private UAReturn changeNominalOrOrdinalToScale(string colName, List<FactorMap> fmap, string changeTo, ServerDataSource dataSource)
        {
            return (dispatcher.EvaluateToUAReturn(RCommandStrings.NominalOrOrdinalToScale(colName, fmap, changeTo, dataSource)));
        }

        private UAReturn GetAlign(ServerDataSource dataSource, string objectName)//Anil added
        {
            return (dispatcher.EvaluateToUAReturn(RCommandStrings.GetDataFrameColumnAlignment(dataSource, objectName)));
        }

        private UAReturn GetRole(ServerDataSource dataSource, string objectName)//Anil added
        {
            return (dispatcher.EvaluateToUAReturn(RCommandStrings.GetDataFrameColumnRole(dataSource, objectName)));
        }

        private UAReturn GetFactorValues(ServerDataSource dataSource, string objectName)//Anil added
        {
            return (dispatcher.EvaluateToUAReturn(RCommandStrings.GetDataFrameColumnValues(dataSource, objectName)));
        }

        private object getMaxFactors(ServerDataSource dataSource)
        {
            return (object)(dispatcher.EvaluateToObject(RCommandStrings.getMaximumFactorCount(dataSource), false)); //object
        }

        private UAReturn GetMissingValues(ServerDataSource dataSource, string objectName)//Anil added
        {
            return dispatcher.EvaluateToUAReturn(RCommandStrings.GetColMissingValues(dataSource, objectName));
        }

        private UAReturn GetColProp(ServerDataSource dataSource, string objectName)//Anil added
        {
            return dispatcher.EvaluateToUAReturn(RCommandStrings.GetDataFrameColumnProp(dataSource, objectName));
        }

        #endregion

        #region datagrid Core

        private UAReturn changeDatagridCell(string colName, string celdata, int rowindex, ServerDataSource dataSource)
        {
            return (dispatcher.EvaluateToUAReturn(RCommandStrings.ChangeDatagridCell(colName, celdata, rowindex, dataSource)));
        }

        private UAReturn addDatagridRow(string colName, string celdata, string rowdata, int rowindex, ServerDataSource dataSource)
        {
            return (dispatcher.EvaluateToUAReturn(RCommandStrings.AddNewDatagridRow(colName, celdata, rowdata, rowindex, dataSource)));
        }

        private UAReturn delDatagridRow(int rowindex, ServerDataSource dataSource)
        {
            return (dispatcher.EvaluateToUAReturn(RCommandStrings.DeleteDatagridRow(rowindex, dataSource)));
        }

        private UAReturn sortDatagridCol(string colname, string sortorder, ServerDataSource dataSource)
        {
            return (dispatcher.EvaluateToUAReturn(RCommandStrings.SortDatagridCol(colname, sortorder, dataSource)));
        }
        #endregion

        #region IDisposable //19Feb2013
        public void Dispose()
        {
            dispatcher.Dispose();
        }
        #endregion
    }
}