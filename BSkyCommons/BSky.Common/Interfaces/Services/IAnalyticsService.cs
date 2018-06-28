using System;
using System.Collections.Generic;
using System.ServiceModel;
using BSky.Statistics.Common;
using RDotNet;

using BSky.Lifetime.Interfaces;

namespace BSky.Statistics.Service.Engine.Interfaces
{
    [ServiceContract]
    public interface IAnalyticsService
    {
        [OperationContract]
        UAReturn Execute(CommandRequest cmd);

        [OperationContract]
        object ExecuteR(CommandRequest cmd, bool hasReturn, bool hasUAReturn);

        [OperationContract]
        UAReturn LogCommandNotExecute(CommandRequest cmd);//16Aug2016

        [OperationContract]
        DataFrame GetDataFrame(DataSource ds);

        [OperationContract]
        UAReturn EmptyDataSourceLoad(string datasetName, string fileName);//03Jan2014

        [OperationContract]
        UAReturn DataSourceLoad(string datasetName, string fileName, string sheetname, bool replace = false, IOpenDataFileOptions odfo = null);

        [OperationContract]
        UAReturn DataFrameLoad(string dframename, string datasetName, string sheetname);//13Feb2014

        [OperationContract]
        UAReturn GetSQLTableList(string sqlcommand);//24Nov2015

        [OperationContract]
        UAReturn GetOdbcTables(string fileName);//27Jan2014

        [OperationContract]
        UAReturn GetRDataDframeObjList(string fileName);//23May2018

        [OperationContract]
        object GetDgridFindResults(string findtext, string[] selectedcols, bool matchcase, string datasetnames);//24Jun2016

        [OperationContract]
        object GetAllModels(string classtype);//09Sep2016

        [OperationContract]
        UAReturn DataSourceRefresh(string datasetName, string fileName);//25Mar2013

        [OperationContract]
        UAReturn DataSourceReadRows(string datasetName, int startRow, int endRow);

        [OperationContract]
        UAReturn DataSourceReadCell(string datasetName, int rowIndex, int colIndex);

        [OperationContract]
        UAReturn DataSourceReadRow(string datasetName, int rowIndex); //23Jan2014

        [OperationContract]
        UAReturn OneSample(string datasetName, List<string> vars, double mu, double confidenceLevel, int missing);

        [OperationContract]
        UAReturn Binomial(string datasetName, List<string> vars, double p, string alternative, double confidenceLevel, bool descriptives, bool quartiles, int missing);

        [OperationContract]
        UAReturn DatasetSaveAs(string fileName, string filetype, string sheetname, string datasetnameorindex);//Anil. 

        [OperationContract]
        UAReturn DatasetClose(string fullfilename, string datasetnameorindex, string sheetname);//Anil. 

        [OperationContract]
        UAReturn EditVarGrid(string datasetnameorindex, string colName, string colProp, string newValue, List<string> colLevels);//Anil. 

        [OperationContract]
        UAReturn MakeColFactor(string datasetnameorindex, string colName);//Anil.

        [OperationContract]
        UAReturn MakeColString(string datasetnameorindex, string colName);//Anil.

        [OperationContract]
        UAReturn MakeColNumeric(string datasetnameorindex, string colName);//Anil. 11Oct2017

        [OperationContract]

        UAReturn addNewVariable(string colName, string rdataType, string dgridval, int rowindex, string datasetnameorindex);//15Oct2015 modified

        [OperationContract]
        UAReturn removeVargridColumn(string colName, string datasetnameorindex);

        //[OperationContract]
        //UAReturn removeMultipleVargridColumn(string[] colNames, string datasetnameorindex);//04Aug2016

        [OperationContract]
        UAReturn ChangeColumnLevels(string colName, List<ValLvlListItem> finalList, string datasetnameorindex);

        [OperationContract]
        UAReturn AddFactorLevels(string colName, List<string> finalList, string datasetnameorindex);

        [OperationContract]
        UAReturn EditDatagridCell(string colName, string celdata, int rowindex, string datasetnameorindex);

        [OperationContract]
        UAReturn AddNewDatagridRow(string colName, string celdata, string rowdata, int rowindex, string datasetnameorindex);

        [OperationContract]
        UAReturn RemoveDatagridRow(int rowindex, string datasetnameorindex);

        [OperationContract]
        UAReturn SortDatagridCol(string colName, string sortorder, string datasetnameorindex);

        [OperationContract]
        UAReturn ChangeMissingVals(string colName, string colProp, List<string> newMisVal, string mistype, string datasetnameorindex);

        [OperationContract]
        Object GetColNumFactors(string colName, string datasetnameorindex);

        [OperationContract]
        UAReturn ChangeScaleToNominalOrOrdinal(string colName, List<FactorMap> fmap, string changeTo, string datasetnameorindex);

        [OperationContract]
        List<FactorMap> GetColumnFactormap(string colName, string datasetnameorindex);

        [OperationContract]
        UAReturn ChangeNominalOrOrdinalToScale(string colName, List<FactorMap> fmap, string changeTo, string datasetnameorindex);

        //27Oct2016
        #region R Object save / Load

        [OperationContract]
        UAReturn GetAllRObjs();

        [OperationContract]
        UAReturn SaveRObjs(string objname, string fullpathfilename);

        [OperationContract]
        UAReturn LoadRObjs(string fullpathfilename);

        #endregion

        //06Dec2013
        #region Package Related

        [OperationContract]
        void LoadDefPackages();//30Mar2015 For loading default packages after showing main window

        [OperationContract]
        UAReturn PackageInstall(string[] pkgfilenames, bool autoLoad = true, bool overwrite = false);//(string package, string filepath);

        [OperationContract]
        UAReturn CRANPackageInstall(string packagename);

        [OperationContract]
        UAReturn CRANReqPackageInstall(string packagename);//27Aug2015

        [OperationContract]
        UAReturn setCRANMirror();

        [OperationContract]
        UAReturn PackageLoad(string package);

        [OperationContract]
        UAReturn ListPackageLoad(string[] packagenames);

        [OperationContract]
        UAReturn ShowPackageInstalled();

        [OperationContract]
        UAReturn ShowPackageLoaded();

        [OperationContract]
        UAReturn GetMissingDefPackages();

        [OperationContract]
        UAReturn PackageUnload(string[] packagenames);

        [OperationContract]
        UAReturn PackageUninstall(string[] packagenames);
        #endregion
    }
}