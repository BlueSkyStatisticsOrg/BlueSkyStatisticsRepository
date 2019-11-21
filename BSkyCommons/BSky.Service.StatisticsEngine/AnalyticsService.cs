using System;
using System.Collections.Generic;
using System.Linq;
using BSky.Statistics.Common;
using BSky.Statistics.R;
using BSky.Statistics.Common.Interfaces;
using BSky.Statistics.Service.Engine.Interfaces;
using BSky.Lifetime;
using BSky.Lifetime.Interfaces;
using RDotNet;

namespace BSky.Service.Engine
{
    // NOTE: If you change the class name "Service1" here, you must also update the reference to "Service1" in App.config.
    public class AnalyticsService : IAnalyticsService
    {
        Session _userSession;
        ILoggerService logService = LifetimeService.Instance.Container.Resolve<ILoggerService>();//17Dec2012
        public AnalyticsService()
        {
            UAPackageAPI BSkypkgapi = new BSky.Statistics.R.UAPackageAPI();//06Nov2014
            _userSession = new Session(BSkypkgapi);
            DefPkgMessage = BSkypkgapi.DefPkgMsg;//06Nov2014 retrieving message for R pkg load failed.
        }

        public string DefPkgMessage { get; set; }//06Nov2014

        public DataFrame GetDataFrame(DataSource ds)
        {
            DataFrame _DF = _userSession.DefaultDispatcher.GetDF(ds);
            return _DF;
        }

        public UAReturn Execute(CommandRequest cmd)
        {
            UAReturn r;
            ServerCommand srvCMD = new ServerCommand(_userSession.DefaultDispatcher, "", cmd.CommandSyntax);
            srvCMD.Execute();
            return srvCMD.Result;
        }

        public object ExecuteR(CommandRequest cmd, bool hasReturn, bool hasUAReturn)
        {
            UAReturn r;
            ServerCommand srvCMD = new ServerCommand(_userSession.DefaultDispatcher, "", cmd.CommandSyntax);
            return srvCMD.ExecuteR(hasReturn, hasUAReturn);
        }

        public UAReturn LogCommandNotExecute(CommandRequest cmd)//16Aug2016
        {
            UAReturn r;
            ServerCommand srvCMD = new ServerCommand(_userSession.DefaultDispatcher, "", cmd.CommandSyntax);
            srvCMD.JustLogCommandDoNotExecute();
            return srvCMD.Result;
        }

        //public void ResetOutput()
        //{ 
        //}

        //03Jan2014
        public UAReturn EmptyDataSourceLoad(string datasetName, string fileName, string sheetname)
        {
            UAReturn r;

            ServerDataSource dataSource = new ServerDataSource(_userSession.DefaultDispatcher, fileName, datasetName, sheetname);
            r = _userSession.DefaultDispatcher.EmptyDataSourceLoad(dataSource);
            //08Jun2013
            //if ( r.Success)
            //{ 
            //    return r;                
            //}

            //return null;
            return r; //08Jun2013
        }

        //25Oct2016 Added 'replace' parameter (to overwrite a dataset with new values)
        public UAReturn DataSourceLoad(string datasetName, string fileName, string sheetname, bool removeSpacesSPSS=false,  bool replace = false, IOpenDataFileOptions odfo = null)
        {
            UAReturn r;

            ServerDataSource dataSource = new ServerDataSource(_userSession.DefaultDispatcher, fileName, datasetName, sheetname, removeSpacesSPSS,  replace, odfo);
            r = _userSession.DefaultDispatcher.DataSourceLoad(dataSource, sheetname, removeSpacesSPSS );//,odfo
            //08Jun2013
            //if ( r.Success)
            //{ 
            //    return r;                
            //}

            //return null;
            return r; //08Jun2013
        }

        public UAReturn DataFrameLoad(string dframename, string datasetName, string sheetname)//13Feb2014
        {
            UAReturn r;

            ServerDataSource dataSource = new ServerDataSource(_userSession.DefaultDispatcher, dframename, datasetName, sheetname); //empty filename sent.
            r = _userSession.DefaultDispatcher.DataFrameLoad(dataSource, datasetName);
            return r;
        }

        public UAReturn GetSQLTableList(string sqlcommand)//24Nov2015
        {
            UAReturn r = _userSession.DefaultDispatcher.GetSQLTablelist(sqlcommand);
            return r;
        }

        public UAReturn GetOdbcTables(string fileName)//27Jan2014
        {
            UAReturn r = _userSession.DefaultDispatcher.GetRodbcTables(fileName);
            return r;
        }

        public UAReturn GetRDataDframeObjList(string fileName)//23May2018
        {
            UAReturn r = _userSession.DefaultDispatcher.GetRDataDfObjList(fileName);
            return r;
        }

        public object GetDgridFindResults(string findtext, string[] selectedcols, bool matchcase, string datasetnames)//24Jun2016
        {
            object r = _userSession.DefaultDispatcher.GetDatagridFindResults(findtext, selectedcols, matchcase, datasetnames);
            return r;
        }

        public object GetAllModels(string classtype)//09Sep2016
        {
            object r = _userSession.DefaultDispatcher.GetAllModels(classtype);
            return r;
        }

        public UAReturn DataSourceRefresh(string datasetName, string fileName, string sheetname)//25Mar2013 refresh on new row added by compute
        {
            UAReturn r;

            ServerDataSource dataSource = new ServerDataSource(_userSession.DefaultDispatcher, fileName, datasetName, sheetname);

            if ((r = _userSession.DefaultDispatcher.DataSourceRefresh(dataSource)).Success)
            {
                return r;
            }

            return null;
        }

        public UAReturn DataSourceReadRows(string datasetName, int startRow, int endRow)
        {
            ServerDataSource dataSource = _userSession.DefaultDispatcher.DataSources.Where(s => string.Compare(s.Name, datasetName, StringComparison.InvariantCultureIgnoreCase) == 0).FirstOrDefault();

            return dataSource.ReadRows(startRow, endRow);
        }

        public UAReturn DataSourceReadCell(string datasetName, int rowIndex, int colIndex)
        {
            ServerDataSource dataSource = _userSession.DefaultDispatcher.DataSources.Where(s => string.Compare(s.Name, datasetName, StringComparison.InvariantCultureIgnoreCase) == 0).FirstOrDefault();

            return dataSource.ReadCell(rowIndex, colIndex);
        }

        public UAReturn DataSourceReadRow(string datasetName, int rowIndex)//23Jan2014 Read a row
        {
            ServerDataSource dataSource = _userSession.DefaultDispatcher.DataSources.Where(s => string.Compare(s.Name, datasetName, StringComparison.InvariantCultureIgnoreCase) == 0).FirstOrDefault();

            return dataSource.ReadRow(rowIndex);
        }

        public UAReturn Binomial(string datasetName, List<string> vars, double p, string alternative, double confidenceLevel, bool descriptives, bool quartiles, int missing)
        {
            try
            {
                ServerDataSource dataSource = _userSession.DefaultDispatcher.DataSources.Where(s => string.Compare(s.Name, datasetName, StringComparison.InvariantCultureIgnoreCase) == 0).FirstOrDefault();

                IAnalyticCommands cm = _userSession.DefaultDispatcher as IAnalyticCommands;

                return cm.UABinomial(dataSource, vars, p, alternative, confidenceLevel, descriptives, quartiles, missing);
            }
            catch (Exception ex)
            {
                logService.WriteToLogLevel(ex.Message, LogLevelEnum.Error);
                return new UAReturn() { Success = false, Error = ex.Message };
            }
        }

        public UAReturn OneSample(string datasetName, List<string> vars, double mu, double confidenceLevel, int missing)
        {
            try
            {
                ServerDataSource dataSource = _userSession.DefaultDispatcher.DataSources.Where(s => string.Compare(s.Name, datasetName, StringComparison.InvariantCultureIgnoreCase) == 0).FirstOrDefault();

                IAnalyticCommands cm = _userSession.DefaultDispatcher as IAnalyticCommands;

                return cm.UAOneSample(dataSource, vars, mu, confidenceLevel, missing);
            }
            catch (Exception ex)
            {
                logService.WriteToLogLevel(ex.Message, LogLevelEnum.Error);
                return new UAReturn() { Success = false, Error = ex.Message };
            }
        }

        public UAReturn DatasetSaveAs(string fileName, string filetype, string sheetname, string datasetnameorindex)//Anil #2
        {
            UAReturn r;

            ServerDataSource dataSource = new ServerDataSource(_userSession.DefaultDispatcher, fileName, datasetnameorindex, sheetname);

            if ((r = _userSession.DefaultDispatcher.DatasetSaveas(dataSource)).Success)//2
            {
                return r;
            }

            return null;
        }

        public UAReturn DatasetClose(string fullfilename, string datasetnameorindex, string sheetname)//Anil #2
        {
            UAReturn r;

            ServerDataSource dataSource = new ServerDataSource(_userSession.DefaultDispatcher, fullfilename, datasetnameorindex, sheetname);

            if ((r = _userSession.DefaultDispatcher.CloseDataset(dataSource)).Success)//2
            {
                return r;
            }

            return null;
        }

        #region vargrid
        public UAReturn EditVarGrid(string datasetnameorindex, string colName, string colProp, string newValue, List<string> colLevels)//Anil. 
        {
            UAReturn r;

            ServerDataSource dataSource = new ServerDataSource(_userSession.DefaultDispatcher, "", datasetnameorindex, null);
            r = _userSession.DefaultDispatcher.editDatasetVarGrid(dataSource, colName, colProp, newValue, colLevels);
            if (r.Success)//2
            {
                return r;
            }

            return r; //20Jun2013 return null;
        }

        public UAReturn MakeColFactor(string datasetnameorindex, string colName)//Anil. 
        {
            UAReturn r;

            ServerDataSource dataSource = new ServerDataSource(_userSession.DefaultDispatcher, "", datasetnameorindex, null);
            r = _userSession.DefaultDispatcher.makeColumnFactor(dataSource, colName);
            if (r.Success)//2
            {
                return r;
            }

            return r; //20Jun2013 return null;
        }

        public UAReturn MakeColString(string datasetnameorindex, string colName)//Anil. 
        {
            UAReturn r;

            ServerDataSource dataSource = new ServerDataSource(_userSession.DefaultDispatcher, "", datasetnameorindex, null);
            r = _userSession.DefaultDispatcher.makeColumnString(dataSource, colName);
            if (r.Success)//2
            {
                return r;
            }

            return r; //20Jun2013 return null;
        }

        //11Oct2017
        public UAReturn MakeColNumeric(string datasetnameorindex, string colName)//Anil. 
        {
            UAReturn r;

            ServerDataSource dataSource = new ServerDataSource(_userSession.DefaultDispatcher, "", datasetnameorindex, null);
            r = _userSession.DefaultDispatcher.makeColumnNumeric(dataSource, colName);
            if (r.Success)//2
            {
                return r;
            }

            return r; //20Jun2013 return null;
        }

        public UAReturn addNewVariable(string colName, string rdataType, string dgridval, int rowindex, string datasetnameorindex)//15Oct2015 modified
        {
            UAReturn r;

            ServerDataSource dataSource = new ServerDataSource(_userSession.DefaultDispatcher, "", datasetnameorindex, null);

            if ((r = _userSession.DefaultDispatcher.addNewColDatagrid(colName, rdataType, dgridval, rowindex, dataSource)).Success)//2
            {
                return r;
            }

            return null;
        }

        public UAReturn removeVargridColumn(string colName, string datasetnameorindex)
        {
            UAReturn r;
            ServerDataSource dataSource = new ServerDataSource(_userSession.DefaultDispatcher, "", datasetnameorindex, null);

            if ((r = _userSession.DefaultDispatcher.removeVarGridCol(colName, dataSource)).Success)
            {
                return r;
            }
            return null;
        }

        //04Aug2016 Delete multiple rows from Vargrid (ie delete multiple columns from dataset in R)
        //public UAReturn removeMultipleVargridColumn(string[] colNames, string datasetnameorindex)
        //{
        //    UAReturn r;
        //    ServerDataSource dataSource = new ServerDataSource(_userSession.DefaultDispatcher, "", datasetnameorindex);
        //    if ((r = _userSession.DefaultDispatcher.removeMultipleVarGridCol(colNames, dataSource)).Success)
        //    {
        //        return r;
        //    }
        //    return null;
        //}

        public UAReturn ChangeColumnLevels(string colName, List<ValLvlListItem> finalLevelList, string datasetnameorindex)
        {
            UAReturn r;
            ServerDataSource dataSource = new ServerDataSource(_userSession.DefaultDispatcher, "", datasetnameorindex, null);

            if ((r = _userSession.DefaultDispatcher.changeColLevels(colName, finalLevelList, dataSource)).Success)
            {
                return r;
            }
            return null;
        }

        public UAReturn AddFactorLevels(string colName, List<string> finalLevelList, string datasetnameorindex)
        {
            UAReturn r;
            ServerDataSource dataSource = new ServerDataSource(_userSession.DefaultDispatcher, "", datasetnameorindex, null);

            if ((r = _userSession.DefaultDispatcher.addColLevels(colName, finalLevelList, dataSource)).Success)
            {
                return r;
            }
            return null;
        }

        public UAReturn ChangeMissingVals(string colName, string colProp, List<string> newMisVal, string mistype, string datasetnameorindex)
        {
            UAReturn r;
            ServerDataSource dataSource = new ServerDataSource(_userSession.DefaultDispatcher, "", datasetnameorindex, null);

            if ((r = _userSession.DefaultDispatcher.changeMissing(colName, colProp, newMisVal, mistype, dataSource)).Success)
            {
                return r;
            }
            return null;
        }

        public object GetColNumFactors(string colName, string datasetnameorindex)
        {
            object r;
            ServerDataSource dataSource = new ServerDataSource(_userSession.DefaultDispatcher, "", datasetnameorindex, null);
            r = _userSession.DefaultDispatcher.getColNumFactors(colName, dataSource);
            return r;
        }

        public UAReturn ChangeScaleToNominalOrOrdinal(string colName, List<FactorMap> fmap, string changeTo, string datasetnameorindex)
        {
            UAReturn r;
            ServerDataSource dataSource = new ServerDataSource(_userSession.DefaultDispatcher, "", datasetnameorindex, null);

            if ((r = _userSession.DefaultDispatcher.setScaleToNominalOrOrdinal(colName, fmap, changeTo, dataSource)).Success)
            {
                return r;
            }
            return null;
        }

        public List<FactorMap> GetColumnFactormap(string colName, string datasetnameorindex)
        {
            List<FactorMap> r;
            ServerDataSource dataSource = new ServerDataSource(_userSession.DefaultDispatcher, "", datasetnameorindex, null);
            r = _userSession.DefaultDispatcher.getColFactormap(colName, dataSource);
            return r;
        }

        public UAReturn ChangeNominalOrOrdinalToScale(string colName, List<FactorMap> fmap, string changeTo, string datasetnameorindex)
        {
            UAReturn r;
            ServerDataSource dataSource = new ServerDataSource(_userSession.DefaultDispatcher, "", datasetnameorindex, null);

            if ((r = _userSession.DefaultDispatcher.setNominalOrOrdinalToScale(colName, fmap, changeTo, dataSource)).Success)
            {
                return r;
            }
            return null;
        }

        #endregion

        #region datagrid
        public UAReturn EditDatagridCell(string colName, string celdata, int rowindex, string datasetnameorindex)
        {
            UAReturn r = null;
            ServerDataSource dataSource = new ServerDataSource(_userSession.DefaultDispatcher, "", datasetnameorindex, null);
            r = _userSession.DefaultDispatcher.editDatagridCell(colName, celdata, rowindex, dataSource);//move out of following 'if' condition part
            if (r.Success)
            {
                return r;
            }
            return null;
        }

        public UAReturn AddNewDatagridRow(string colName, string celdata, string rowdata, int rowindex, string datasetnameorindex)
        {
            if (rowdata == null || rowdata.Trim().Length < 2)
                rowdata = ""; // or c() or NA

            UAReturn r;
            ServerDataSource dataSource = new ServerDataSource(_userSession.DefaultDispatcher, "", datasetnameorindex, null);
            r = _userSession.DefaultDispatcher.addNewDataRow(colName, celdata, rowdata, rowindex, dataSource);//move out of following 'if' condition part
            if (r.Success)
            {
                return r;
            }
            return null;
        }

        public UAReturn RemoveDatagridRow(int rowindex, string datasetnameorindex, string sheetname)
        {
            UAReturn r;
            ServerDataSource dataSource = new ServerDataSource(_userSession.DefaultDispatcher, "", datasetnameorindex, sheetname);

            if ((r = _userSession.DefaultDispatcher.removeDatagridRow(rowindex, dataSource)).Success)
            {
                return r;
            }
            return null;
        }

        //06Jun2018
        public UAReturn SortDatagridCol(string colName, string sortorder, string datasetnameorindex)
        {
            UAReturn r;
            ServerDataSource dataSource = new ServerDataSource(_userSession.DefaultDispatcher, "", datasetnameorindex, null);

            if ((r = _userSession.DefaultDispatcher.sortDatagridColumn(colName, sortorder, dataSource)).Success)
            {
                return r;
            }
            return null;
        }
        #endregion

        //27Oct2016
        #region R Object save / Load

        public UAReturn GetAllRObjs()
        {
            UAReturn r = null;
            r = _userSession.DefaultDispatcher.GetAll_RObjects();
            return r;
        }

        public UAReturn SaveRObjs(string objname, string fullpathfilename)
        {
            UAReturn r = null;
            r = _userSession.DefaultDispatcher.Save_RObjects(objname, fullpathfilename);
            return r;
        }

        public UAReturn LoadRObjs(string fullpathfilename)
        {
            UAReturn r = null;
            r = _userSession.DefaultDispatcher.Load_RObjects(fullpathfilename);
            return r;
        }

        #endregion

        //06Dec2013
        #region Package Related

        public UAReturn GetDefaultUserRLib()
        {
            UAReturn r = null;
            r = _userSession.DefaultDispatcher.GenerateDefaultUserPersonalRLibPath();
            return r;
        }

        public void LoadDefPackages()
        {
            _userSession.DefaultDispatcher.LoadDefPacakges();
            DefPkgMessage = (_userSession.DefaultDispatcher as UAPackageAPI).DefPkgMsg;// fix for retrieveing load Default package messages
        }

        public UAReturn PackageInstall(string[] pkgfilenames, bool autoLoad = true, bool overwrite = false)//(string package, string filepath)
        {
            UAReturn r = null;
            r = _userSession.DefaultDispatcher.InstallLocalPackage(pkgfilenames, autoLoad, overwrite);//(package, filepath);
            return r;
        }

        public UAReturn CRANPackageInstall(string packagename)
        {
            UAReturn r = null;
            r = _userSession.DefaultDispatcher.InstallCRANPackage(packagename);
            return r;
        }

        //27Aug2015
        public UAReturn CRANReqPackageInstall(string packagename)
        {
            UAReturn r = null;
            r = _userSession.DefaultDispatcher.InstallReqPackageFromCRAN(packagename);
            return r;
        }

        public UAReturn setCRANMirror()
        {
            UAReturn r = null;
            r = _userSession.DefaultDispatcher.setCRANMirror();
            return r;
        }

        public UAReturn PackageLoad(string package)
        {
            UAReturn r = null;
            r = _userSession.DefaultDispatcher.LoadLocalPackage(package);
            return r;
        }

        public UAReturn ListPackageLoad(string[] packagenames)
        {
            UAReturn r = null;
            r = _userSession.DefaultDispatcher.LoadPackageFromList(packagenames);
            return r;
        }

        public UAReturn ShowPackageInstalled()
        {
            UAReturn r = null;
            r = _userSession.DefaultDispatcher.ShowInstalledPackages();
            return r;
        }

        public UAReturn ShowUserRlibPackageInstalled()
        {
            UAReturn r = null;
            r = _userSession.DefaultDispatcher.ShowUserRLibInstalledPackages();
            return r;
        }

        public UAReturn ShowPackageLoaded()
        {
            UAReturn r = null;
            r = _userSession.DefaultDispatcher.ShowLoadedPackages();
            return r;
        }

        public UAReturn GetMissingDefPackages()
        {
            UAReturn r = null;
            r = _userSession.DefaultDispatcher.GetMissingDefRPackages();
            return r;
        }

        public UAReturn PackageUnload(string[] packagenames)
        {
            UAReturn r = null;
            r = _userSession.DefaultDispatcher.UnloadPackages(packagenames);
            return r;
        }

        public UAReturn PackageUninstall(string[] packagenames)
        {
            UAReturn r = null;
            r = _userSession.DefaultDispatcher.UninstallPackages(packagenames);
            return r;
        }
        public UAReturn GetPkgDatasetList(string packagename)//12Feb2019
        {
            UAReturn r = null;
            r = _userSession.DefaultDispatcher.FetchRpkgDatasetList(packagename);
            return r;
        }
        #endregion
    }
}