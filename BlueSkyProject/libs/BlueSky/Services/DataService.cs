using System.Collections.Generic;
using System.Linq;
using BSky.Statistics.Service.Engine.Interfaces;
using BSky.Statistics.Common;
using BSky.Lifetime;
using BSky.Lifetime.Interfaces;
using System;
using BSky.XmlDecoder;
using BSky.Interfaces.Model;
using BSky.Interfaces.Interfaces;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;

namespace BlueSky.Services
{
    public class DataService : IDataService
    {
        private Dictionary<string, DataSource> _datasources = new Dictionary<string, DataSource>();
        private Dictionary<string, string> _datasourcenames = new Dictionary<string, string>();//05Mar2014
        ILoggerService logService = LifetimeService.Instance.Container.Resolve<ILoggerService>();//17Dec2012

        IUIController UIController;//20Oct2016 for making UI datagrid emty for null dataset.

        int SessionDatasetCounter = 1;
        private IAnalyticsService _analyticService;
        public DataService(IAnalyticsService analyticsService)
        {
            _analyticService = analyticsService;
        }


        #region IDataService Members

        //To get a freshly created datasetname to load in grid and refer to that dataframe object in R memeory using this name.
        //Dataset name will be like Dataset1, Dataset2, Dataset3 .... and so on.
        public string GetFreshDatasetName()
        {
            string datasetname = "Dataset" + SessionDatasetCounter;//(_datasources.Keys.Count + 1);//can also be filename without path and extention
            //SessionDatasetCounter++;
            return datasetname;
        }

        public DataSource NewDataset()//03Jan2014
        {
            string datasetname = GetUniqueNewDatasetname();//(_datasources.Keys.Count + 1);//can also be filename without path and extention
            string sheetname = string.Empty;//no sheetname for empty dataset(new dataset)
            //15Jun2015 if Dataset is created and loaded from syntax UI SessionDatasetCounter can have issues as it may not be increamented when
            // datasetset is loaded from syntax
            if (_datasources.Keys.Contains(datasetname + sheetname))
                return _datasources[datasetname + sheetname];

            UAReturn datasrc = _analyticService.EmptyDataSourceLoad(datasetname, datasetname, null);//second pram was full path filename on disk
            if (datasrc.Datasource == null)
            {
                logService.WriteToLogLevel("Could not open: " + datasetname + ".\nInvalid format OR issue related to R.Net server.", LogLevelEnum.Error);
                //string[,] errwarnmsg=OutputHelper.GetMetaData(1, "normal");//08Jun2013
                ////AnalyticsData data = new AnalyticsData();
                ////data.Result = datasrc;
                ////if (!data.Result.Success)
                ////{
                ////    OutputWindowContainer owc = (LifetimeService.Instance.Container.Resolve<IOutputWindowContainer>()) as OutputWindowContainer;//new line
                ////    IOutputWindow _outputWindow = owc.ActiveOutputWindow;//To get active ouput window to populate analysis.AD.
                ////    _outputWindow.Show();
                ////    OutputHelper.Reset();
                ////    _outputWindow.AddAnalyis(data);
                ////}
                //SendToOutput(datasrc);

                return null;
            }
            //datasrc.CommandString = "Open Dataset";//21Oct2013
            DataSource ds = datasrc.Datasource.ToClientDataSource();
            if (ds != null)//03Dec2012
            {
                //_datasources.Add(ds.FileName, ds);///key filename
                ////_datasourcenames.Add(datasetname, ds.FileName);

                /// key filename + Sheetname is imp if different sheet from same file 
                /// are opened there will be no issue. Diff sheetname will force generating diff keys
                /// duplicate keys cause exeception
                if (!_datasources.ContainsKey(ds.FileName + ds.SheetName))//for avoiding crash
                    _datasources.Add(ds.FileName + ds.SheetName, ds);

                //Here looks like sheetname is not so imp. rather it causes issue if memory object with same name
                //exists but it does not have sheetname. Say Dataset2Sheet1 is the key for excel sheet1.
                //Now Rdata file is opened that has Dataset2. Excel Dataset2 is already overwritten but if you check
                //following dictionay you will not find a key 'Dataset2'. Because of sheetname the key for tha dataset
                //with same name was not found.. That's why we should not put sheet as a part of key.
                if (!_datasourcenames.ContainsKey(datasetname))//for avoiding crash
                    _datasourcenames.Add(datasetname /*+ ds.SheetName*/, ds.FileName);//5Mar2014
            }
            ///incrementing dataset counter //// 14Feb2013
            SessionDatasetCounter++;
            SendToOutput(datasrc);
            return ds;
        }

        public string GetUniqueNewDatasetname()
        {
            string newdatasetname = string.Empty;
            for(; ;)
            {
                newdatasetname = "Dataset" + SessionDatasetCounter;
                if (_datasourcenames.Keys.Contains(newdatasetname))
                    SessionDatasetCounter++;
                else
                    break;
            }
            return newdatasetname;
        }

        //if Excel sheet is loaded we also need sheetname so that we
        //can create dataset name like ExcelFilename+Sheetname
        //other logic below will make sure dataset name does not have 
        //any other special characters other than _ and .
        //spaces are also replaced with underscores.
        //If there is a number in the begining then we prefix 'D' to it.
        //if fname is empty then we generate Dataset1, Dataset2 etc.

        public string GetDatasetName(string fname = "", string sheetname="")
        {
            string datasetname = string.Empty;

            //generate name from filename
            if (fname.Length > 0)
            {
                if (sheetname.Length > 0)
                    datasetname = Path.GetFileName(fname) + "." + sheetname;
                else
                    datasetname = Path.GetFileName(fname);
                //drop special characters except dots and underscrores with underscores
                datasetname = Regex.Replace(datasetname, @"[^a-zA-Z0-9.]+", ".").Replace(" ", ".");
                //  @"[^\w\._]" //should work like above.
                //@"[^a-zA-Z0-9_.] supports underscore in filename.
                if (char.IsNumber(datasetname.ToCharArray().ElementAt(0)))
                    datasetname = "D" + datasetname;
            }
            else
                datasetname = GetUniqueNewDatasetname();
            return datasetname;
        }
        
        //Find existing(already loaded) DataSource by using dataset name
        public DataSource FindDataSourceFromDatasetname(string datasetname)
        {
            DataSource loadedDS = null;
            foreach (KeyValuePair<string, DataSource> kvp in _datasources)
            {
                loadedDS = kvp.Value as DataSource;
                if (loadedDS.Name.Trim().Equals(datasetname))//loaded dataset found
                    break;
            }
            return loadedDS;
        }

        public DataSource Open(string filename, string sheetname, bool removeSpacesSPSS=false,  IOpenDataFileOptions odfo=null)
        {
            if (sheetname==null || sheetname.Trim().Length == 0) //29Apr2015 just to make sure sheetname should have valid chars and not spaces.
                sheetname = string.Empty; 
            //int i = 10, j = 0;
            //if (i > 0) i = i / j;
            //filename = filename.ToLower();
            string datasetname = "Dataset" + SessionDatasetCounter;
            string fileExtension = Path.GetExtension(filename).ToLower();
            if (fileExtension.Equals(".xls") || fileExtension.Equals(".xlsx"))
                datasetname = GetDatasetName(filename, sheetname);
            else
                datasetname = GetDatasetName(filename);
            //28May2018
            //Since now we read rdata files and load multiple data.frames sometimes dataframe
            //names are like Dataset2 or Dataset10 as they were saved-as from BSky grid earlier.
            //Now if rdata file loads a data that we Dataset10 at the time of creating RData file and then
            // we open several other files of other types(except rdata or xlsx) then at some point Dataset10
            //will be assigned to some file. But Dataset10 is already loaded from RData. Here the exception 
            //occurs because _datasourcenames hold the old key and we try to add new key with same name. Duplicate
            //key cant be added. 
            //Earlier we use to save the filename to make sure keys are different but now RData gives us memory dataframe
            // and we do not save the filename anymore so the uniqu key check we run on _datasources shows that key is
            // not found but when we try to add it to _datasourcenames it already contains the key.
            // basically we now check key in both _datasources  as well as _datasourcenames.
            // if _datasourcenames has 'Dataset10' as key and as a value(same to same) that means its memory dataframe 
            // so the key that we generate above using SessionDatasetCounter should be incremented to get new name 
            // like Dataset11 or Dataset12 until unique dataset name is not found. and then we follow the exisitng
            //  logic of adding new name to _datasources  as well as _datasourcenames

            ////StringBuilder DSName = new StringBuilder();
            ////for (; SessionDatasetCounter < 1000;)
            ////{
            ////    DSName.Clear();
            ////    DSName.Append("Dataset" + SessionDatasetCounter);//(GetDatasetName(filename));// 
            ////    if (_datasourcenames.Keys.Contains(DSName.ToString()))
            ////    {
            ////        //Folowing code may not be needed but can be use to differentiate between memory and file datasets
            ////        // memory datasets have same string for key-values pair
            ////        string val = _datasourcenames[DSName.ToString()];
            ////        if (val.Equals(DSName.ToString()))
            ////        {

            ////        }

            ////        //This is what we need to do to make current dataset name (DatasetNN) unique
            ////        //from the memory loaded datasets
            ////        SessionDatasetCounter++;
            ////    }
            ////    else
            ////    {
            ////        datasetname = DSName.ToString();
            ////        break;
            ////    }
            ////}

           

            bool keyfound = false, replaceDS=false;
            DataSource uids = null;
            if (_datasources.Keys.Contains(filename + sheetname)) //Check if filename is same and dataset is already loaded in the grid
            {
                //25Oct2016
                uids = _datasources[filename + sheetname];
                if (uids.RowCount == null || uids.RowCount == 0 || uids.Variables == null || uids.Variables.Count == 0) //if the dataset is NULL dataset
                {
                    keyfound = true;
                    replaceDS = true;
                    datasetname = uids.Name; //Dataset1, Dataset2 etc..
                }
                else
                    return _datasources[filename + sheetname];
            }

            DataSource loadedDS = null;//already loaded DataSource whose dataset name is same as the new one
            bool datasetkeyfound = _datasourcenames.Keys.Contains(datasetname);
            if (!keyfound && datasetkeyfound)// different file but same dataset name (as already loaded in UI)
            {
                //find the existing DataSource that is to be returned (for NO) and overwritten (for YES)
                loadedDS = FindDataSourceFromDatasetname(datasetname);

                //warn user and get user's choice to overwrite already loaded data.frame or not
                string msg1 = "Dataset with the same name is already loaded in the datagrid.\n";
                string msg2 = "Do you want to replace it with the new one?";

                MessageBoxResult mbr = MessageBox.Show(msg1+msg2, "Overwrite exisiting dataset?", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (mbr == MessageBoxResult.Yes)
                {
                    replaceDS = true;//Overwrite DataSource if user selects YES
                }
                else
                {
                    return loadedDS;// User selects NO, return this DataSource
                }
            }
            //25Oct2016 string datasetname = "Dataset" + SessionDatasetCounter;//(_datasources.Keys.Count + 1);//can also be filename without path and extention
            //AnalyticsData data = new AnalyticsData();
            //data.Result = datasrc;
            //data.AnalysisType = datasrc.CommandString;//21Oct2013
            UAReturn datasrc = _analyticService.DataSourceLoad(datasetname, filename, sheetname, removeSpacesSPSS, replaceDS, odfo);
            if (datasrc == null)
            {
                logService.WriteToLogLevel("Could not open: " + filename , LogLevelEnum.Error);
                return null;
            }
            else if (datasrc!= null && datasrc.Datasource == null)
            {
                if (datasrc.Error != null && datasrc.Error.Length > 0)
                {
                    logService.WriteToLogLevel("Could not open: " + filename + ".\n" + datasrc.Error, LogLevelEnum.Error);
                }
                else
                {
                    logService.WriteToLogLevel("Could not open: " + filename + ".\nInvalid format OR issue related to R.Net server.", LogLevelEnum.Error);
                    datasrc.Error = "Could not open: " + filename + ".\nInvalid format OR issue related to R.Net server.";
                }
                //string[,] errwarnmsg=OutputHelper.GetMetaData(1, "normal");//08Jun2013
    ////AnalyticsData data = new AnalyticsData();
    ////data.Result = datasrc;
    ////if (!data.Result.Success)
    ////{
    ////    OutputWindowContainer owc = (LifetimeService.Instance.Container.Resolve<IOutputWindowContainer>()) as OutputWindowContainer;//new line
    ////    IOutputWindow _outputWindow = owc.ActiveOutputWindow;//To get active ouput window to populate analysis.AD.
    ////    _outputWindow.Show();
    ////    OutputHelper.Reset();
    ////    _outputWindow.AddAnalyis(data);
    ////}
                //SendToOutput(datasrc);
                DataSource dsnull = new DataSource(){ Message = datasrc.Error};

                //fix for emptydataset.sav. This dataset does not have any rows but has colnames.
                //So when we open it, it is added to uadatasets (say "Dataset2") because read.spss() opens it.
                //But in the UI grid logic it fails to load because of no rows. And so SessionDatasetCounter is not incremented.
                // So for next dataset the same name will be used ( say "Dataset2")
                //Now, if we try to open another dataset (with Dataset2) it causes some issue and end up 
                //showing "no rows" message. And now we need to restart the BlueSky app, else we can't open any other dataset
                //EASY FIX is, increment SessionDatasetCounter if "no rows" condition appears. "Dataset3" will be used for
                //another dataset and so everything should be fine on R and C# side.
                //But uadataset will contain an entry for emtydataset, i.e. "Dataset2"
                //LITTLE COMPLICATED FIX is when you see no row(or detect that while opening the dataset in
                // R and stop  it there), you
                // should clean uadataset(for error:'Dataset with the same name already on the global list')
                // and also clean some C# side objects(the dictionary) those map to this empty dataset.
                if (datasrc!=null && datasrc.Error!= null && datasrc.Error.Equals("No rows in Dataset"))
                {
                    SessionDatasetCounter++;
                }

                return dsnull;
            }


            /////14Jun2015 ADD alogic here to check for duplicate keys before moving further


            //datasrc.CommandString = "Open Dataset";//21Oct2013
            DataSource ds = datasrc.Datasource.ToClientDataSource();
            if(ds!=null)//03Dec2012
            {
                if (keyfound )//25Oct2016 dataset already existed but was set to null, somehow
                {
                    //remove its old key/val and then add new(outside 'if' )
                    _datasources.Remove(ds.FileName + ds.SheetName);///key filename
                    //21OCt2019 moved in 'if' below _datasourcenames.Remove(datasetname /*+ ds.SheetName*/);//5Mar2014
                }
                else
                {
                    if(loadedDS!=null)
                        _datasources.Remove(loadedDS.FileName + loadedDS.SheetName);///key filename
                }

                if (datasetkeyfound)
                    _datasourcenames.Remove(datasetname);

                //Add new keys
                if(!_datasources.ContainsKey(ds.FileName + ds.SheetName))//for avoiding crash
                    _datasources.Add(ds.FileName + ds.SheetName, ds);///key filename
                if (!_datasourcenames.ContainsKey(datasetname))//for avoiding crash
                    _datasourcenames.Add(datasetname /*+ ds.SheetName*/, ds.FileName);//5Mar2014
            }
                                                  
            ///incrementing dataset counter //// 14Feb2013
            //16OCt2019 if(!keyfound) SessionDatasetCounter++;
            SendToOutput(datasrc);
            return ds;
        }

        //In Datagrid tabs the tab title looks like this : Cars.sav(DatasetN), where N is any integer.
        //So here DatasetN is the object in memory that is dataframe( or similar).
        //Cars.sav is just extra info about the Dataset you tried to open. This extra info could be a disk filename
        // or it could also be just a memory object name if memory object name is different from 'DatasetN' style of naming
        // eg.. mydf(mydf) , Dataset1(Dataset1), Cars.sav(Dataset2), mydata.rdata(Dataset3) 
        // in above example mydf, Dataset1 Dataset2, Dataset3 , all R objects exists in R memory.

        //Wrong way of naming would look something like this : mydf(Dataset1)
        // mydf is not a disk file, so lets assume it may be R dataframe obj in memory, but then we always put memory object 
        // name within round brackets, so, if (Dataset1) is a memory object, the extra info 'mydf' does not make sense. 
        //So 'mydf' should be named as 'Dataset1' instead.
        //Right thing would be either one of two mydf(mydf) or Dataset1(Dataset1) for non-disk datasets

        public DataSource OpenDataframe(string dframename, string sheetname, string fname="") //13Feb2014
        {
            UAReturn datasrc = null;
            DataSource ds = null;
            if (!isDatasetNew(dframename /*+ sheetname*/)) // if data.frame with same name already loaded in C1DataGrid
            {
                string filename = _datasourcenames[dframename /*+ sheetname*/];
            //////////    datasrc = new UAReturn();
            //////////    datasrc = _analyticService.DataFrameLoad(filename, dframename, sheetname);
            //////////    //////////////
            //////////    if (datasrc == null)
            //////////    {
            //////////        logService.WriteToLogLevel("Could not open: " + filename, LogLevelEnum.Error);
            //////////        return null;
            //////////    }
            //////////    else if (datasrc != null && datasrc.Datasource == null)
            //////////    {
            //////////        if (datasrc.Error != null && datasrc.Error.Length > 0)
            //////////        {
            //////////            logService.WriteToLogLevel("Could not refresh/open: " + filename + ".\n" + datasrc.Error, LogLevelEnum.Error);
            //////////        }
            //////////        else
            //////////            logService.WriteToLogLevel("Could not refresh/open: " + dframename + ".\nInvalid format OR issue related to R.Net server.", LogLevelEnum.Error);

            //////////        DataSource dsnull = new DataSource() { Message = datasrc.Error };
            //////////        return dsnull;
            //////////    }

            //////////    datasrc.CommandString = "Refresh Dataframe";
                try
                {
                    if (_datasources.Keys.Contains(filename + sheetname))
                        ds = _datasources[filename + sheetname];
                    else //no need to check if it exists as we alreay checked if its new dataset or not in code above
                        ds = _datasources[_datasourcenames[dframename /*+ sheetname*/].Trim()+sheetname];

                    //So by now we know we know that Dataset name is same. 
                    //And new one already overwrote old data.frame in memory. 
                
                    
                    
                    
                    
                    
                    
                    
                    //Now we need to figure out if the disk filename is same or not
                    //if same no issue. If different then we need to replace old with new filename
                    #region Overwrite with new filename if Filename is different from old one
                    //if ( !filename.ToLower().Equals(fname)) //fname.Length > 0 && filename.Length > 0 &&
                    //{
                    //    //remove old keys in _datasources _datasourcenames
                    //    if (_datasources.Keys.Contains(filename + sheetname)) //Check if dataset is already loaded in the grid
                    //    {
                    //        _datasources.Remove(filename + sheetname);//Remove old
                    //        //_datasources.Add(dsourceName.FileName + sheetname, dsourceName);///Replace ds with new one

                    //        //5Mar2014 No need to do following but we can still do it
                    //        _datasourcenames.Remove(dframename /*+ sheetname*/);
                    //        //_datasourcenames.Add(dsourceName.Name + sheetname, dsourceName.FileName);
                    //    }
                    //}

                    //update ds. if new filename then overwrite old. if blank then use DatasetNN for filename
                    if (fname.Trim().Length > 0)
                        ds.FileName = fname;
                    else
                    {
                        // ds.FileName = dframename;
                        ds.SheetName = UtilFunctions.GetSheetname(ds);
                    }
                    #endregion
                }
                catch (Exception ex)
                {
                    logService.WriteToLogLevel("Error getting existing DataSource handle", LogLevelEnum.Fatal);
                }
                string existingDatasetname = ds.Name;

                //20Oct2016 save a copy for using in 'if' below
                DataSource ds_for_cleanup = ds;

ds = Refresh(ds); 

if (ds == null)//20Oct2016 Making UI grid NULL
{
    ds = new DataSource();
    ds.Variables = new List<DataSourceVariable>();//making it blank
    ds.FileName = ds_for_cleanup.FileName;
    ds.Name = ds_for_cleanup.Name;
    ds.SheetName = UtilFunctions.GetSheetname(ds_for_cleanup);
                    ds.DecimalCharacter = ds_for_cleanup.DecimalCharacter;
                    ds.FieldSeparator = ds_for_cleanup.FieldSeparator;
                    ds.HasHeader = ds_for_cleanup.HasHeader;
                    ds.IsBasketData = ds_for_cleanup.IsBasketData;
    UIController = LifetimeService.Instance.Container.Resolve<IUIController>();
    UIController.RefreshBothGrids(ds);
    //here I can also close before or after above line(whereever works)
    //Close(ds_for_cleanup);

    //Generating the message for null dataset
    CommandRequest msg = new CommandRequest();
    msg.CommandSyntax = ds_for_cleanup.Name + " been set to NULL. The reason may be, running some analysis or command on the dataset.";
    //string title = "NULL Dataset.";
    //if (msg != null && title != null)//16May2013
    //    SendToOutputWindow(title, msg.CommandSyntax);
    logService.WriteToLogLevel(msg.CommandSyntax, LogLevelEnum.Error);
}
            }
            else // Its New Data.Frame . 
            {
                string datasetname = "Dataset" + SessionDatasetCounter;//dframename;// //(_datasources.Keys.Count + 1);//can also be filename without path and extention

                //15Jun2015 dframename exists in memory so use that name for datagrid tab name enclosed in round brackets (Dataset1) or (df1)
                if (!dframename.Equals(datasetname)) //df2 and Dataset2
                {
                    datasetname = dframename;
                    //use df2 for both because df2 exists in R memory
                    //no need to increament the SessionDatasetCounter as when you try to open a disk file it will have (Dataset2) 
                    // as name and it will not clash with the (df2) name.
                    //incrementing may not harm but there is no need to increment. 

                    //ELSE dframename.Equals(datasetname) like both are say "Dataset2"
                    // use (Dataset2) for both because Dataset2 exists in memory
                    //Also increament the SessionDatasetCounter in this case because now later 
                    //when you open Dataset from disk is should have name Dataset3 and not Dataset2
                }
                if (fname.ToLower().EndsWith(".rdata"))
                {
                    dframename = fname; //pass RData filename
                }
                datasrc = _analyticService.DataFrameLoad(dframename, datasetname, "");

                if (datasrc == null)
                {
                    logService.WriteToLogLevel("Could not open: " + dframename, LogLevelEnum.Error);
                    return null;
                }
                else if (datasrc != null && datasrc.Datasource == null)
                {
                    if (datasrc.Error != null && datasrc.Error.Length > 0)
                    {
                        logService.WriteToLogLevel("Could not open: " + dframename + ".\n" + datasrc.Error, LogLevelEnum.Error);
                    }
                    else
                        logService.WriteToLogLevel("Could not open: " + dframename + ".\nInvalid format OR issue related to R.Net server.", LogLevelEnum.Error);

                    DataSource dsnull = new DataSource() { Message = datasrc.Error };
                    return dsnull;
                }

                datasrc.CommandString = "Open Dataset";//21Oct2013
                ds = datasrc.Datasource.ToClientDataSource();

                if (ds != null)//03Dec2012
                {
                    //_datasources.Add(ds.FileName, ds);///key filename
                    if (!_datasources.ContainsKey(ds.FileName + ds.SheetName))//for avoiding crash
                        _datasources.Add(ds.FileName + ds.SheetName, ds);///key filename
                    if (!_datasourcenames.ContainsKey(datasetname))//for avoiding crash
                        _datasourcenames.Add(datasetname /*+ ds.SheetName*/, ds.FileName);//5Mar2014
                }
                ///incrementing dataset counter //// 15Jun2015
                /// if the name of the Dataset created in syntax matches to the Dataset name generated for UI grid.
                if(dframename.Equals(datasetname))
                {
                    SessionDatasetCounter++;
                }
            }
            if (fname.Length == 0)
            {
                ds.Extension = "";
                ds.FileName = "";
            }
//04Nov2014. Dont show "Open Dataset" before subset command title.   SendToOutput(datasrc);
            return ds;
        }

        public UAReturn ImportTableListFromSQL(string sqlcommand)//24Nov2015
        {
            UAReturn uret = _analyticService.GetSQLTableList(sqlcommand);
            //return uret.SimpleTypeData;
            return uret;
        }

        public object GetOdbcTableList(string filename)//27Jan2014
        {
            UAReturn tbls = _analyticService.GetOdbcTables(filename);
            return tbls.SimpleTypeData;
        }

        public object GetRDataDataframeObjList(string filename)//23May2018
        {
            string datasetname = "Dataset" + SessionDatasetCounter;
            UAReturn tbls = _analyticService.GetRDataDframeObjList(filename);
            return tbls.SimpleTypeData;
        }

        public DataSource Refresh(DataSource dsourceName)//25Mar2013
        {
            string sheetname = dsourceName.SheetName != null ? dsourceName.SheetName : string.Empty;// BugId #JM12Apr16
            //filename = filename.ToLower();
            //if (_datasources.Keys.Contains(filename.ToLower()))
            //    return _datasources[filename];
            //string datasetname = "Dataset" + SessionDatasetCounter;
            UAReturn datasrc = _analyticService.DataSourceRefresh(dsourceName.Name, dsourceName.FileName, sheetname);
            if (datasrc == null)
            {
                //25Oct2016 If key is found then you must update the dictionary(with empty vars and rowcount) when dataset becomes NULL
                dsourceName.Variables = new List<DataSourceVariable>();
                dsourceName.RowCount = 0;
                if (_datasources.Keys.Contains(dsourceName.FileName + sheetname)) //Check if dataset is already loaded in the grid
                {
                    _datasources.Remove(dsourceName.FileName + sheetname);//Remove old
                    if (!_datasources.ContainsKey(dsourceName.FileName + sheetname))//for avoiding crash
                        _datasources.Add(dsourceName.FileName + sheetname, dsourceName);///Replace ds with new one

                    //5Mar2014 No need to do following but we can still do it
                    _datasourcenames.Remove(dsourceName.Name /*+ sheetname*/);
                    if (!_datasourcenames.ContainsKey(dsourceName.Name))//for avoiding crash
                        _datasourcenames.Add(dsourceName.Name /*+sheetname*/, dsourceName.FileName);
                }

                logService.WriteToLogLevel("Could not open:" + dsourceName.FileName + ".Invalid format OR issue related to R.Net server.", LogLevelEnum.Error);
                return null;
            }
            DataSource ds = datasrc.Datasource.ToClientDataSource();
            //CSV-Options
            ds.DecimalCharacter = dsourceName.DecimalCharacter;
            ds.FieldSeparator = dsourceName.FieldSeparator;
            ds.HasHeader = dsourceName.HasHeader;
            ds.IsBasketData = dsourceName.IsBasketData;

            ds.SheetName = UtilFunctions.GetSheetname(dsourceName);//fix for Excel dataset becomes null and reload from disk does not work
            ds.isUnprocessed = dsourceName.isUnprocessed;//for new dataset

            if (ds != null)//03Dec2012
            {
                
                //03jun2018 _datasources.Remove(dsourceName.FileName + sheetname);//Remove old
                _datasources.Remove(_datasourcenames[dsourceName.Name] + sheetname);//Remove old
                //////if (ds.FileName.Equals(ds.Name))//its a memory dataframe. Sheetname should be blank
                //////{
                //////    sheetname = "";
                //////}
                if (!_datasources.ContainsKey(ds.FileName + sheetname))//for avoiding crash
                    _datasources.Add(ds.FileName + sheetname, ds);///Replace ds with new one //25Oct2016 added sheetname
                                                  
                //5Mar2014 No need to do following but we can still do it
                _datasourcenames.Remove(ds.Name /*+ sheetname*/);
                if (!_datasourcenames.ContainsKey(ds.Name))//for avoiding crash
                    _datasourcenames.Add(ds.Name /*+ sheetname*/, ds.FileName);//25Oct2016 added sheetname
            }

            //Fix for 'Data > Reload Dataset from File' crash, when run on excel dataset. Missing sheetname was causing the problem.
            //This fix needed modification in few locations. So here is BugID for easy search #JM12Apr16
            //////ds.SheetName = sheetname;
            ///incrementing dataset counter //// 14Feb2013
            //SessionDatasetCounter++;

            return ds;
        }

        public void SaveAs(string filename, DataSource ds)//#1
        {
            string worksheetname = string.Empty;
            string s = ds.Name;//Dataset Name of currently open grid. I guess. So no need to specifically provide it.-Anil
            ds.Changed = false; // dont show popup while closing. Coz its already saved
            string filetype = filename.Substring(filename.LastIndexOf(".") + 1).ToUpper();
            if (filetype.Equals("XLS") || filetype.Equals("XLSX"))//07Jul2016 Providing original sheetname
            {
                worksheetname = ds.SheetName != null && ds.SheetName.Length > 0 ? ds.SheetName : "Sheet1";
            }
            UAReturn datasrc = _analyticService.DatasetSaveAs(filename, filetype, worksheetname, ds.Name);//Anil. 
            SendToOutput(datasrc);
            return;
        }

        public void Close(DataSource ds)//#1
        {
            string datasetName = ds.Name;//Dataset Name of currently open grid. I guess. So no need to specify.-Anil
            //UAReturn datasrc = _analyticService.DatasetClose(datasetName);//Anil
            UAReturn datasrc = _analyticService.DatasetClose(ds.FileName, datasetName, ds.SheetName);//Anil. //01Aug2016 : for sending dataset close command to output
            if (_datasources.ContainsKey(ds.FileName + ds.SheetName))
            {
                _datasources.Remove(ds.FileName + ds.SheetName);//remove from the dictionary ///filename
                //5Mar2014
                _datasourcenames.Remove(ds.Name /*+ ds.SheetName*/);
                SendToOutput(datasrc);
            }
            else
            {
                logService.WriteToLogLevel("Key not found. Unable to close:" + ds.FileName, LogLevelEnum.Error);
            }

            return;
        }

        public bool isDatasetNew_old(string dskey)//17Feb2014
        {
            bool isNew=true;
            if (_datasources.Keys.Contains(dskey)) // if true not new 
                isNew = false;
            else if (_datasourcenames.Keys.Contains(dskey))
            {
                // check partial name ie 'Dataset1'
                //int keycount = _datasources.Keys.Count;
                //for (int i = 0; i < keycount;i++ )
                //{

                //}
                isNew = false;
            }
            return isNew;
        }

        public bool isDatasetNew(string dskey)//17Feb2014
        {
            bool isNew = true;

            if (_datasourcenames.Keys.Contains(dskey))
            {
                // check partial name ie 'Dataset1'
                //int keycount = _datasources.Keys.Count;
                //for (int i = 0; i < keycount;i++ )
                //{

                //}
                isNew = false;
            }
            else if (_datasources.Keys.Contains(dskey)) // if true not new 
                isNew = false;
            return isNew;
        }

        public void editVarGrid(DataSource ds, string colName, string colProp, string newLabel, List<string> newOrder)
        {
            _analyticService.EditVarGrid(ds.Name, colName, colProp, newLabel, newOrder);//Anil. 
            return;
        }

        public void SendToOutput(UAReturn datasrc) // if possible move this function to global space so that it can be used by
        {                                           // open/close/ edit vargrid / edit datagrid etc.. you might want to add few more lines here.

            //24May2017 Crash fix
            if (datasrc == null || datasrc.CommandString == null)
                return;

            AnalyticsData data = new AnalyticsData();
            data.Result = datasrc;
            data.AnalysisType = datasrc.CommandString;//21Oct2013

            OutputWindowContainer owc = (LifetimeService.Instance.Container.Resolve<IOutputWindowContainer>()) as OutputWindowContainer;//new line
            IOutputWindow _outputWindow = owc.ActiveOutputWindow;//To get active ouput window to populate analysis.AD.
            _outputWindow.Show();
            OutputHelper.Reset();
            _outputWindow.AddAnalyis(data);
        }
        #endregion


        //27Oct2016
        #region R Object save / Load

        public UAReturn GetAllRObjects()
        {
            UAReturn r = _analyticService.GetAllRObjs();
            //SendToOutput(r);
            return r;
        }
        public UAReturn SaveRObjects(string objname, string fullpathfilename)
        {
            UAReturn r = _analyticService.SaveRObjs(objname, fullpathfilename);
            //SendToOutput(r);
            return r;
        }
        public UAReturn LoadRObjects(string fullpathfilename)
        {
            UAReturn r = _analyticService.LoadRObjs(fullpathfilename);
            //SendToOutput(r);
            return r;
        }

        #endregion
        // 06Dec2013
        #region Package Related
        public UAReturn installPackage(string[] pkgfilenames, bool autoLoad = true, bool overwrite = false)//(string package, string filepath)
        {
           UAReturn r =  _analyticService.PackageInstall(pkgfilenames, autoLoad, overwrite);//(package, filepath);
           //SendToOutput(r);
           return r;
        }
        public UAReturn installCRANPackage(string packagename)
        {
            UAReturn r = _analyticService.CRANPackageInstall(packagename);
            //SendToOutput(r);
            return r;
        }
        public UAReturn installReqPackageCRAN(string packagename)//27Aug2015
        {
            UAReturn r = _analyticService.CRANReqPackageInstall(packagename);
            //SendToOutput(r);
            return r;
        }
        public UAReturn setCRANMirror()
        {
            UAReturn r = _analyticService.setCRANMirror();
            //SendToOutput(r);
            return r;
        }
        public UAReturn loadPackage(string package)
        {
            UAReturn r = _analyticService.PackageLoad(package);
            //SendToOutput(r);
            return r;
        }
        public UAReturn loadPackageFromList(string[] packagenames)
        {
            UAReturn r = _analyticService.ListPackageLoad(packagenames);
            //SendToOutput(r);           
            return r;
        }
        public UAReturn showInstalledPackages()
        {
            UAReturn r = _analyticService.ShowPackageInstalled();
            //SendToOutput(r);
            return r;
        }
        public UAReturn showUserRLibInstalledPackages()
        {
            UAReturn r = _analyticService.ShowUserRlibPackageInstalled();
            //SendToOutput(r);
            return r;
        }
        public UAReturn showLoadedPackages()
        {
            UAReturn r = _analyticService.ShowPackageLoaded();
            //SendToOutput(r);
            return r;
        }
        public UAReturn getMissingDefaultRPackages()
        {
            UAReturn r = _analyticService.GetMissingDefPackages();
            //SendToOutput(r);
            return r;
        }
        public UAReturn unloadPackage(string[] packagenames)
        {
            UAReturn r = _analyticService.PackageUnload(packagenames);
            //SendToOutput(r);
            return r;
        }
        public UAReturn uninstallPackage(string[] packagenames)
        {
            UAReturn r = _analyticService.PackageUninstall(packagenames);
            //SendToOutput(r);
            return r;
        }

        public UAReturn loadRPkgDatasetList(string packagename) //12Feb2019
        {
            UAReturn r = _analyticService.GetPkgDatasetList(packagename);
            return r;
        }
        #endregion
    }
}
