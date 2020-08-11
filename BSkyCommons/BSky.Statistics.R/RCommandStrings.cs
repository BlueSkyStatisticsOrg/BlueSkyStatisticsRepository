using System.Collections.Generic;
using BSky.Statistics.Common;
using System.IO;
using BSky.Lifetime.Interfaces;
using BSky.Lifetime;
using BSky.ConfService.Intf.Interfaces;

namespace BSky.Statistics.R
{
    public class RCommandStrings
    {
        static IConfigService confService = LifetimeService.Instance.Container.Resolve<IConfigService>();//21Apr2014

        #region Dataframe

        private static ServerDataSourceTypeEnum GetDataSetTypeFromExtension(string ext)
        {
            switch (ext.ToLower())
            {
                case "csv": return ServerDataSourceTypeEnum.CSV;
                case "sav": return ServerDataSourceTypeEnum.SPSS;
                //following by Anil.
                case "sas7bdat": return ServerDataSourceTypeEnum.SAS;
                case "dbf": return ServerDataSourceTypeEnum.DBF;
                case "xls": return ServerDataSourceTypeEnum.XLS;
                case "xlsx": return ServerDataSourceTypeEnum.XLSX;
                case "rdata": return ServerDataSourceTypeEnum.RDATA;
                case "dat": return ServerDataSourceTypeEnum.DAT;
                case "txt": return ServerDataSourceTypeEnum.TXT;
                default: return ServerDataSourceTypeEnum.Unknown;
            }
        }

        public static string LoadEmptyDataSource(ServerDataSource dataSource)
        {
            dataSource.DataSetType = GetDataSetTypeFromExtension(dataSource.Extension);

            if (dataSource.DataSetType == ServerDataSourceTypeEnum.Unknown)
            {
                //for Excel sheet name must also be set before opening. Anil
                //16Nov2017 No need do it in LoadDataSourceExt     dataSource.HasHeader = true;
                //16Nov2017 No need do it in LoadDataSourceExt     dataSource.FieldSeparator = ",";
                string datasetname = dataSource.Name;
                return string.Format("BSkyOpenNewDataset( datasetName='{0}')", datasetname);
            }
            return string.Empty;
        }

        public static string LoadDataSource(ServerDataSource dataSource, string tableOrSheetname = "Sheet1", bool removeSpacesSPSS=false)//, IOpenDataFileOptions odfo=null
        {
            dataSource.DataSetType = GetDataSetTypeFromExtension(dataSource.Extension);

            if (dataSource.DataSetType != ServerDataSourceTypeEnum.Unknown)
            {
                //for Excel sheet name must also be set before opening. Anil
                //16Nov2017 No need do it in LoadDataSourceExt     dataSource.HasHeader = true;
                //16Nov2017 No need do it in LoadDataSourceExt     dataSource.FieldSperator = ",";
                return LoadDataSourceExt(dataSource, tableOrSheetname, removeSpacesSPSS);//, odfo
            }
            return string.Empty;
        }

        public static string LoadDataframe(ServerDataSource dataSource, string dframe)
        {
            dataSource.DataSetType = GetDataSetTypeFromExtension(dataSource.Extension);

            if (dataSource.DataSetType == ServerDataSourceTypeEnum.Unknown ||
                dataSource.DataSetType == ServerDataSourceTypeEnum.RDATA)
            {
                //for Excel sheet name must also be set before opening. Anil
                //16Nov2017 No need do it in LoadDataSourceExt     dataSource.HasHeader = true;
                //16Nov2017 No need do it in LoadDataSourceExt     dataSource.FieldSeparator = ",";
                string datasetname = dataSource.Name;
                return string.Format("BSky.LoadRefresh.Dataframe('{0}')", dframe); // prefixing with .GlobalEnv$  can get you to right 'summary' 20Dec2015
            }
            return string.Empty;
        }

        private static string LoadDataSourceExt(ServerDataSource dataSource, string sheetOrTablename, bool removeSpacesSPSS=false) //, IOpenDataFileOptions odfo = null
        {
            //uaopendataset(typeoffile, directory,filename,uaperformance)
            string dirPath = Path.GetDirectoryName(dataSource.FileNameWithPath);
            string fileName = Path.GetFileName(dataSource.FileNameWithPath);
            string fullpathfilename = Path.Combine(dirPath, fileName); //dirPath + "\\" + fileName;
            string datasetname = dataSource.Name;
            string filetype = dataSource.Extension.ToUpper();

            string worksheetname = null;
            bool replaceDataset = false;
            bool csvHeader = true; //No need of this...similar option below : HasHeader
            string loadMissingValue = "FALSE";//21Apr2014
            string TrimSPSSTrailing = "FALSE";

            //16Nov2017 Options for opening data file //
            bool HasHeader = false;
            bool IsBasketData = false;
            string sepChar = ",";
            string deciChar = ".";

            //16Nov2017
            HasHeader = dataSource.HasHeader;
            IsBasketData = dataSource.IsBasketData;
            sepChar = dataSource.FieldSeparator;
            deciChar = dataSource.DecimalCharacter;

            //04Aug2016 //Character to Factor conversion in R while loading dataset in R.
            //Right now it is used for CSV files but it can also be used for all other formats 
            //where we need to change "charcter" col to "factor" col while loading dataset.
            string characterToFactor = "FALSE";
            string charToFactor = confService.AppSettings.Get("CSVcharToFactor");
            // load default value if no value is set 
            if (charToFactor.Trim().Length == 0)
                charToFactor = confService.DefaultSettings["CSVcharToFactor"];
            characterToFactor = charToFactor.ToLower().Equals("true") ? "TRUE" : "FALSE"; /// 

            if (filetype.Equals("SAV"))
            {
                filetype = "SPSS";

                //21Apr2014 Missing Values loading is optional.
                string LoadMisVal = confService.AppSettings.Get("loadSavMissingValue");
                // load default value if no value is set 
                if (LoadMisVal.Trim().Length == 0)
                    LoadMisVal = confService.DefaultSettings["loadSavMissingValue"];
                loadMissingValue = LoadMisVal.ToLower().Equals("true") ? "TRUE" : "FALSE"; /// 
                TrimSPSSTrailing = removeSpacesSPSS.ToString().ToUpper();
            }

            if (sheetOrTablename != null)// filetype.Equals("XLS") || filetype.Equals("XLSX"))
            {
                worksheetname = sheetOrTablename;// "Sheet1";
            }

            //25Oct2016 replace dataset(may be it became NULL and now we want ot load it from File>recent)
            if (dataSource.Replace)
            {
                // return string.Format("BSkyloadDataset(fullpathfilename='{0}', filetype='{1}', worksheetName='{2}',replace_ds=TRUE, load.missing={4}, character.to.factor={5}, datasetName='{3}')", FormatFileName(fullpathfilename), filetype, worksheetname, datasetname, loadMissingValue, characterToFactor);
                return string.Format("BSkyloadDataset(fullpathfilename='{0}', filetype='{1}', worksheetName='{2}',replace_ds=TRUE," +
                    " load.missing={4}, character.to.factor={5}, csvHeader={6}, isBasketData={7}, trimSPSStrailing={8}, sepChar='{9}', deciChar='{10}',  datasetName='{3}')",
                    FormatFileName(fullpathfilename), filetype, worksheetname, datasetname, loadMissingValue, characterToFactor,
                    HasHeader.ToString().ToUpper(), IsBasketData.ToString().ToUpper(), TrimSPSSTrailing, sepChar, deciChar);
            }
            else
            {
                //return string.Format("BSkyloadDataset(fullpathfilename='{0}', filetype='{1}', worksheetName='{2}',load.missing={4}, character.to.factor={5}, datasetName='{3}')", FormatFileName(fullpathfilename), filetype, worksheetname, datasetname, loadMissingValue, characterToFactor);
                return string.Format("BSkyloadDataset(fullpathfilename='{0}', filetype='{1}', worksheetName='{2}',load.missing={4}, " +
                    "character.to.factor={5}, csvHeader={6}, isBasketData={7}, trimSPSStrailing={8}, sepChar='{9}', deciChar='{10}', datasetName='{3}')",
                    FormatFileName(fullpathfilename), filetype, worksheetname, datasetname, loadMissingValue, characterToFactor,
                    HasHeader.ToString().ToUpper(), IsBasketData.ToString().ToUpper(), TrimSPSSTrailing, sepChar, deciChar);
            }
        }

        public static string GetODBCTableList(string filename, bool xlsx) //27Jan2014 Getting the list of tables/sheets present( Access, Excel, dBase)
        {
            if (xlsx)
                return string.Format("GetTableList('{0}', TRUE)", filename.Replace("\\", "/"));
            else
                return string.Format("GetTableList('{0}', FALSE)", filename.Replace("\\", "/"));
        }

        public static string GetRobjDFList(string filename) //23May2018 Getting the list of Rdata objs those are data.frame or tbl_df
        {
            //BSkyCurrentRObj can be used by user as a handle to dig in further to find other object loaded with
            // .RData loading. For Now we don't expose this obj as suggested by Aaron.
            // Also, 'matrix' is not supported in R side code, as suggested by Aaron.
            // Our current logic does not go deeper to find data.frames/tbl_df, it only looks for these in the top layer.
                return string.Format("BSky.GetDataframeObjNames('{0}','BSkyCurrentRObj')", filename.Replace("\\", "/"));
        }

        public static string SaveDatasetToFile(ServerDataSource dataSource)//string fullpathfilename, string filetype, string datasetName)
        {
            string dirPath = Path.GetDirectoryName(dataSource.FileNameWithPath);
            string fileName = Path.GetFileName(dataSource.FileNameWithPath);//.Replace("'","\'");
            string fullpathfilename = Path.Combine(dirPath, fileName); //dirPath + "\\" + fileName;
            string datasetname = dataSource.Name;
            string filetype = dataSource.Extension.ToUpper();

            string worksheetname = null;

            if (filetype.Equals("XLS") || filetype.Equals("XLSX"))
            {
                worksheetname = dataSource.SheetName != null && dataSource.SheetName.Length > 0 ? dataSource.SheetName : "Sheet1";//worksheetname = "Sheet1";
            }

            // set some extra parameter here if needed. Like Excel sheetname. replace etc..
            if (true) //06Feb2018 now we support SPSS saving.     !filetype.Equals("SAV"))
                return string.Format("BSkysaveDataset(fullpathfilename=\"{0}\",filetype='{1}', newWorksheetName='{2}', dataSetNameOrIndex='{3}')", FormatFileName(fullpathfilename), filetype, worksheetname, datasetname);
            else
                return "";
        }//Anil #5

        public static string closeDataset(ServerDataSource dataSource)
        {
            //string dirPath = Path.GetDirectoryName(dataSource.FileNameWithPath);
            //string fileName = Path.GetFileName(dataSource.FileNameWithPath);
            //string fullpathfilename = Path.Combine(dirPath, fileName);// dirPath + "\\" + fileName;
            string datasetname = dataSource.Name;
            return string.Format("BSkycloseDataset('{0}')", datasetname);
        }//Anil #5
        private static string LoadDataSource2(ServerDataSource dataSource) { return string.Format("{0}<-uaopendataset({1}, '{2}', {3}, '{4}')", dataSource.Name, (uint)dataSource.DataSetType, FormatFileName(dataSource.FileNameWithPath), dataSource.HasHeader.ToString().ToUpper(), dataSource.FieldSeparator); }
        public static string EditDatasource(ServerDataSource dataSource) { return string.Format("{0}$value<-edit({1})", dataSource.Name, dataSource.Name); }
        public static string SaveDataSource(string sourceName, string fileName) { return string.Format("{1}<-save({0}$value)", sourceName, fileName); }
        public static string CloseDataSource(string sourceName) { return string.Format("<-close({0})", sourceName); }

        //following alls modified by Anil. added uadatasets$lst$
        public static string GetDataFrameColumnNames(ServerDataSource dataSource) { return string.Format("names({0})", dataSource.Name); }//names(uadatasets$lst${0})

        public static string GetDataFrameColumnLength(ServerDataSource dataSource, string columnName) { return string.Format("length({0}${1})", dataSource.Name, columnName); }//length(uadatasets$lst${0}${1})

        public static string GetDataFrameColumnType(ServerDataSource dataSource, string columnName) { return string.Format("UAgetColProp(colNameOrIndex='{1}', propname='Type', Type.as.Class=FALSE, dataSetNameOrIndex='{0}')", dataSource.Name, columnName); }//A. mod(for var view)

        public static string GetDataFrameColumnLabel(ServerDataSource dataSource, string columnName) { return string.Format("UAgetColProp(colNameOrIndex='{1}', propname='Label', Type.as.Class=FALSE, dataSetNameOrIndex='{0}')", dataSource.Name, columnName); }//A. added(for var view)
                                                                                                                                                                                                                                                                  //public static string SetDataFrameColumnLabel(ServerDataSource dataSource, string columnName, string newLabel) { return string.Format("UAsetColDesc('{0}', '{1}', '{2}')", dataSource.Name, columnName, newLabel); }//A. added(for var view label modification)

        //public static string GetDataFrameColumnMissing(ServerDataSource dataSource, string columnName) { return string.Format("UAgetColProp(colNameOrIndex='{1}', Type.as.Class=FALSE, dataSetNameOrIndex='{0}')$Missing", dataSource.Name, columnName); }//A. added(for var view)

        public static string GetDataFrameColumnAlignment(ServerDataSource dataSource, string columnName) { return string.Format("UAgetColProp(colNameOrIndex='{1}', propname='Align', Type.as.Class=FALSE, dataSetNameOrIndex='{0}')", dataSource.Name, columnName); }//A. added(for var view)

        public static string GetDataFrameColumnRole(ServerDataSource dataSource, string columnName) { return string.Format("UAgetColProp(colNameOrIndex='{1}', propname='Role', Type.as.Class=FALSE, dataSetNameOrIndex='{0}')", dataSource.Name, columnName); }//A. added(for var view)

        public static string GetDataFrameColumnValues(ServerDataSource dataSource, string columnName) { return string.Format("UAgetColProp(colNameOrIndex='{1}', propname='Levels', Type.as.Class=FALSE, dataSetNameOrIndex='{0}')", dataSource.Name, columnName); }//A. added(for var view)

        public static string getMaximumFactorCount(ServerDataSource dataSource) { return string.Format("attr({0},'maxfactor')", dataSource.Name); }//attr(uadatasets$lst${0},'maxfactor')

        public static string GetColMissingValues(ServerDataSource dataSource, string columnName) { return string.Format("UAgetColProp(colNameOrIndex='{1}', propname='Missing', Type.as.Class=FALSE, dataSetNameOrIndex='{0}')", dataSource.Name, columnName); }//A. Missing values

        public static string GetDataFrameColumnMeasure(ServerDataSource dataSource, string columnName) { return string.Format("UAgetColProp(colNameOrIndex='{1}', propname='Measure', Type.as.Class=FALSE, dataSetNameOrIndex='{0}')", dataSource.Name, columnName); }//A. added(for var view)

        public static string GetDataFrameColumnProp(ServerDataSource dataSource, string columnName) { return string.Format("UAgetColProp(colNameOrIndex='{1}', Type.as.Class=FALSE, dataSetNameOrIndex='{0}')", dataSource.Name, columnName); }//A. added(for var view)

        public static string GetDataFrameCellValue(ServerDataSource dataSource, int row, int column) { return string.Format("{0}[{1},{2}]", dataSource.Name, row, column); }//uadatasets$lst${0}[{1},{2}]

        public static string GetDataFrameRowValues(ServerDataSource dataSource, int row) { return string.Format("{0}[{1},]", dataSource.Name, row); }//uadatasets$lst${0}[{1},]

        public static string SetDataFrameColumnProp(ServerDataSource dataSource, string columnName, string columnProp, string newValue) { return string.Format("UAsetColProp(colNameOrIndex='{0}', propertyName='{1}', propertyValue=\"{2}\", dataSetNameOrIndex='{3}')", columnName, columnProp, newValue, dataSource.Name); }//Anil added(for var view modification)

        public static string MakeDatasetColFactor(ServerDataSource dataSource, string columnName) { return string.Format("BSkyMakeColumnFactor(colNameOrIndex='{0}', dataSetNameOrIndex='{1}')", columnName, dataSource.Name); }//Anil added(for var view modification)

        public static string MakeDatasetColString(ServerDataSource dataSource, string columnName) { return string.Format("BSkyMakeColumnString(colNameOrIndex='{0}', dataSetNameOrIndex='{1}')", columnName, dataSource.Name); }//Anil added(for var view modification)

        public static string MakeDatasetColNumeric(ServerDataSource dataSource, string columnName) { return string.Format("BSkyMakeColumnNumeric(colNameOrIndex='{0}', dataSetNameOrIndex='{1}')", columnName, dataSource.Name); }//Anil added(for var view modification) 11Oct2017

        public static string AddNewDatagridCol(string colName, string rdatatype, string dgridval, int rowindex, string dateformat, ServerDataSource dataSource) { return string.Format("BSkyAddVarRow('{0}', '{1}', '{2}', {3}, '{4}','{5}' )", colName, rdatatype, dgridval, rowindex,dateformat, dataSource.Name); }//add new col in datagrid and new row in var grid //15Oct2015 modified
        public static string RemoveVargridrow(string colName, ServerDataSource dataSource) { return string.Format("BSkyRemoveVarRow(delcolname='{0}', dataSetNameOrIndex='{1}')", colName, dataSource.Name); }
        //public static string RemoveVargridrow(string colName, ServerDataSource dataSource) { return string.Format("BSkyRemoveMultipleVarRows(delcolname='{0}', dataSetNameOrIndex='{1}')", colName, dataSource.Name); }

        public static string ChangeColumnLevels(string colName, List<ValLvlListItem> finalLevelList, ServerDataSource dataSource)
        {
            string oldLevels = "c(";
            string newLevels = "c(";
            int i = 0;

            foreach (ValLvlListItem vllst in finalLevelList)
            {
                if (vllst != null)
                {
                    if (i < finalLevelList.Count - 1)//put comma
                    {
                        oldLevels = oldLevels + "\"" + vllst.OriginalLevel.Trim() + "\",";
                        newLevels = newLevels + "\"" + vllst.NewLevel + "\",";
                    }
                    else
                    {
                        oldLevels = oldLevels + "\"" + vllst.OriginalLevel.Trim() + "\")";
                        newLevels = newLevels + "\"" + vllst.NewLevel + "\")";
                    }
                    i++;
                }
            }
            return string.Format("BSkyChangeLevels(colNameOrIndex='{0}', oldLevels={1}, newLevels={2}, dataSetNameOrIndex='{3}')", colName, oldLevels, newLevels, dataSource.Name);
        }

        public static string AddColumnLevels(string colName, List<string> finalLevelList, ServerDataSource dataSource)
        {
            string oldLevels = "c(";
            string newLevels = "c(";
            int i = 0;

            foreach (string vllst in finalLevelList)
            {
                if (vllst != null)
                {
                    if (i < finalLevelList.Count - 1)//put comma
                    {
                        //oldLevels = oldLevels + "\'" + vllst.Trim() + "\',";
                        newLevels = newLevels + "\'" + vllst + "\',";
                    }
                    else
                    {
                        //oldLevels = oldLevels + "\'" + vllst.OriginalLevel.Trim() + "\')";
                        newLevels = newLevels + "\'" + vllst + "\')";
                    }
                    i++;
                }
            }
            return string.Format("BSkyAddLevels(colNameOrIndex='{0}', newLevels={1}, dataSetNameOrIndex='{2}')", colName, newLevels, dataSource.Name);
        }

        public static string ChangeDatagridCell(string colName, string celdata, int rowindex, ServerDataSource dataSource, string rdateformat)
        {
            if (celdata.Trim().Equals(""))//if someone types blank in datagrid cell, we make that NA in R
            {
                //celdata = "NA";
                //return string.Format("BSkyEditDatagrid(colname='{0}', colceldata={1}, rowindex={2}, dataSetNameOrIndex='{3}')", colName, celdata, rowindex, dataSource.Name);
                return string.Format("  (colname='{0}', rowindex={1}, dataSetNameOrIndex='{2}')", colName, rowindex, dataSource.Name);
            }
            else
                return string.Format("BSkyEditDatagrid(colname='{0}', colceldata='{1}', rowindex={2}, dataSetNameOrIndex='{3}', rdateformat='{4}')", colName, celdata, rowindex, dataSource.Name, rdateformat);
        }

        public static string AddNewDatagridRow(string colName, string celdata, string rowdata, int rowindex, ServerDataSource dataSource)
        {
            //return string.Format("BSkyAddNewDatagridRow(colname='{0}', colceldata='{1}', rowdata={2}, rowindex={3}, dataSetNameOrIndex='{4}')", colName, celdata, rowdata, rowindex, dataSource.Name); 
            return string.Format("BSkyAddNewDatagridRowAR(rowdata={0}, rowindex={1}, dataSetNameOrIndex='{2}')", rowdata, rowindex, dataSource.Name);
        }
        public static string DeleteDatagridRow(int rowindex, ServerDataSource dataSource) { return string.Format("BSkyRemoveDatagridRow(rowindex={0}, dataSetNameOrIndex='{1}')", rowindex, dataSource.Name); }

        public static string SortDatagridCol(string colname, string sortorder, ServerDataSource dataSource) { return string.Format("BSkySortDatagridCol(colNameOrIndex='{0}', sortorder='{1}', dataSetNameOrIndex='{2}')", colname, sortorder, dataSource.Name); }

        //set Missing vals. UAsetColProp(colNameOrIndex=1, propertyName="Missing", propertyValue=c(1,2,3), mistype="three", dataSetNameOrIndex=1)
        public static string SetDatasetMissingProp(string columnName, string columnProp, List<string> newMisVals, string mtype, ServerDataSource dataSource)
        {
            string vector = "c(";
            int i = 0;

            if (!mtype.Equals("none"))
            {
                foreach (string misv in newMisVals)
                {
                    if (i < newMisVals.Count - 1)//put comma
                        vector = vector + misv + ",";
                    else
                        vector = vector + misv + ")";
                    i++;
                }
            }
            else
                vector = "c()";
            return string.Format("UAsetColProp('{0}', '{1}', propertyValue= {2}, '{3}', c(), '{4}')", columnName, columnProp, vector, mtype, dataSource.Name);
        }//for setting missing vals
        ///edit following
        public static string SetDatasetMeasureProp(string columnName, string measure, List<string> newOrder, ServerDataSource dataSource)
        {
            string vector = "c(";
            int i = 0;

            if (newOrder.Count > 0)
            {
                foreach (string fact in newOrder)
                {
                    if (i < newOrder.Count - 1)//put comma
                        vector = vector + "'" + fact + "',";
                    else
                        vector = vector + "'" + fact + "')";
                    i++;
                }
            }
            else
                vector = "";//will put <NA> Data lost. Bad
            return string.Format("UAsetColProp('{0}', '{1}', '{2}','{3}', {4}, '{5}')", columnName, "Measure", measure, "none", vector, dataSource.Name);
        }//for setting missing vals
        //UAsetColProp(colNameOrIndex, propertyName, propertyValue, dataSetNameOrIndex)
        public static string GetColNumericFactors(string columnName, ServerDataSource dataSource) { return string.Format("BSkyGetColNumericFactors('{0}', '{1}')", columnName, dataSource.Name); }

        //BSkyScaleToNominal <- function(ColNameOrIndex, numericValues, levelNames, dataSetNameOrIndex)
        public static string ScaleToNominalOrOrdinal(string colName, List<FactorMap> fmap, string changeTo, ServerDataSource dataSource)
        {
            string numericVector = "c(";
            string stringVector = "c(";
            int i = 0;

            foreach (FactorMap fm in fmap)
            {
                if (i < fmap.Count - 1)//put comma
                {
                    //numericVector = numericVector + fm.labels + ",";moved inside else block. so for balnks we are dropping numerics also
                    if (fm.textbox.Trim().Length == 0)
                    {
                        //stringVector = stringVector + "\'\',";//blank for NA // blanks are not sent to R. Drop them.
                    }
                    else
                    {
                        stringVector = stringVector + "\'" + fm.textbox + "\',";
                        numericVector = numericVector + fm.labels + ",";
                    }
                }
                else
                {
                    //numericVector = numericVector + fm.labels + ")"; moved inside else block. so for balnks we are dropping numerics also
                    if (fm.textbox.Trim().Length == 0)
                    {
                        //stringVector = stringVector + "\'\'";//blank for NA. // blanks are not sent to R. Drop them.
                    }
                    else
                    {
                        stringVector = stringVector + "\'" + fm.textbox + "\')";
                        numericVector = numericVector + fm.labels + ")";
                    }
                }
                i++;
            }

            return string.Format("BSkyScaleToNominalOrOrdinal(colNameOrIndex='{0}', numericValues={1}, levelNames={2}, changeto='{3}', dataSetNameOrIndex='{4}')", colName, numericVector, stringVector, changeTo, dataSource.Name);
        }

        public static string GetColFactorMap(string colName, bool numval, ServerDataSource dataSource)
        {
            if (numval)
                return string.Format("BSkyGetFactorMap('{0}', '{1}')$numvalues", colName, dataSource.Name); //return numeric factor
            else
                return string.Format("BSkyGetFactorMap('{0}', '{1}')$lvlnames", colName, dataSource.Name); //return string factor
        }

        //BSkyNominalOrOrdinalToScale <- function(colNameOrIndex, numericValues, levelNames, changeto, dataSetNameOrIndex)
        public static string NominalOrOrdinalToScale(string colName, List<FactorMap> fmap, string changeTo, ServerDataSource dataSource)
        {
            string numericVector = "c(";
            string stringVector = "c(";
            int i = 0;

            foreach (FactorMap fm in fmap)
            {
                if (i < fmap.Count - 1)//put comma
                {
                    numericVector = numericVector + fm.textbox + ",";
                    stringVector = stringVector + "\'" + fm.labels + "\',";// I think labels are not imp to send to R side function
                }
                else
                {
                    numericVector = numericVector + fm.textbox + ")";
                    stringVector = stringVector + "\'" + fm.labels + "\')";
                }
                i++;
            }
            //17Apr2014 Fix for crash. If due to some reason fmap is empty, numericVector and stringVector must have "c()" 
            //instead of just having "c(", which is assigned on top of this function.
            if (fmap.Count < 1)
            {
                numericVector = "c()";
                stringVector = "c()";
            }
            return string.Format("BSkyNominalOrOrdinalToScale(colNameOrIndex='{0}', numericValues={1}, levelNames={2}, changeto='{3}', dataSetNameOrIndex='{4}')", colName, numericVector, stringVector, changeTo, dataSource.Name);
        }
        #endregion

        #region Utility
        static string FormatFileName(string fileName)
        {
            return fileName.Replace('\\', '/');
        }

        static string toString(string[] array)
        {
            string varList = "";

            foreach (string s in array)
                varList += s + ",";

            return varList;
        }
        #endregion

        #region Analysis

        public class Analysis
        {
            #region Compare Means

            /// <summary>
            /// R: uaonesample<-function(vars, mu=0,conf.level=0.95,datasetname, missing=0)
            /// </summary>
            /// <param name="vars"></param>
            /// <param name="mu"></param>
            /// <param name="confidenceLeve"></param>
            /// <param name="dataset"></param>
            /// <param name="missing"></param>
            /// <returns></returns>
            public string OneSample(ServerDataSource dataSource, string[] vars, decimal mu, decimal confidenceLeve, int missing)
            {
                return string.Format("uaonesmt.test(c({0}), mu={1}, conf.level={2}, {3}, missing={4})", RCommandStrings.toString(vars), mu, confidenceLeve, dataSource.Name, missing);
            }

            #endregion
        }
        #endregion
    }
}