using System;
using System.Linq;
using BSky.Lifetime.Interfaces;
using BSky.Lifetime;
using BSky.Statistics.Common;
using System.Windows.Forms;
using System.Xml;
using Microsoft.VisualBasic.CompilerServices;
using System.Collections;
using System.IO;
using System.Security.AccessControl;
using System.Security.Principal;
using RDotNet.NativeLibrary;
using Microsoft.Win32;
using RDotNet;
using System.Security;
using System.Text;
using System.Collections.Generic;
using BSky.Lifetime.Services;
using BSky.ConfService.Intf.Interfaces;
using BSky.ConfigService.Services;

namespace BSky.Statistics.R
{
    public class RService
    {
        ILoggerService logService = LifetimeService.Instance.Container.Resolve<ILoggerService>();//17Dec2012
        IConfigService confService = LifetimeService.Instance.Container.Resolve<IConfigService>();//23nov2012
        bool AdvancedLogging;
        string tableheader = null;
        public class RCommandReturn
        {
            public const string Success = "no error";
        }

        RDotNetConsoleLogDevice _log;//for logging

        public Exception LastException { get; set; }

        private string _lastCommand = string.Empty;

        private REngine _RServer = null;
        private DataFrame _DF = null;

        #region Ctor...

        public RService()
            : base()
        {
            AdvancedLogging = AdvancedLoggingService.AdvLog;
            string RLogFilename = DirectoryHelper.GetLogFileName();
            this._log = new RDotNetConsoleLogDevice();
            this._log.LogDevice = new Journal() { FileName = RLogFilename };
            logService.WriteToLogLevel("R DotNet Server (deepest function call) initialization started.", LogLevelEnum.Info);
            try
            {
                bool LoadSelectedRVersion = false;
                string rHome = null;
                string rPath = null;

                //R install path: Reading from Registry
                string rinstallpath = string.Empty;// GetLatestRInstallDirectory();
                //logService.WriteToLogLevel("Registry's R IntallPath = '" + rinstallpath + "'", LogLevelEnum.Info);

                //R install path: Reading from User's Configuration
                string rhomeconfig = confService.GetConfigValueForKey("rhome");//05Jul2015
                logService.WriteToLogLevel("BlueSky Statistics config setting, R install path = '" + rhomeconfig + "'", LogLevelEnum.Info);
                bool isuserconfigvalid = IsValidRInstallPath(rhomeconfig, out rHome, out rPath);

                //if (rhomeconfig != null && rhomeconfig.Length > 0 && !isuserconfigvalid) // user configured path is invalid. 
                //{
                //    string m1 = BSky.GlobalResources.Properties.Resources.MsgChosingRFromRegistry;
                //    string m2 = "\n" + BSky.GlobalResources.Properties.Resources.MsgSetRHome;
                //    MessageBox.Show(m1 + m2, BSky.GlobalResources.Properties.Resources.warning, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                //    logService.WriteToLogLevel("BlueSky Statistics config setting, R install path is not valid = '" + rhomeconfig + "'", LogLevelEnum.Info);
                //}

                if (isuserconfigvalid)//rhomeconfig set and path exists
                {
                    logService.WriteToLogLevel("Using R install path from the BlueSky Statistics config setting = '" + rhomeconfig + "'", LogLevelEnum.Info);
                }
                //else if (IsValidRInstallPath(rinstallpath, out rHome, out rPath))//registry path exists
                //{
                //    logService.WriteToLogLevel("Using R install path from the registry = '" + rinstallpath + "'", LogLevelEnum.Info);
                //}
                else
                {
                    rHome = null;
                    rPath = null;
                    logService.WriteToLogLevel("BlueSky Statistics config setting, R install path is not valid = '" + rhomeconfig + "'", LogLevelEnum.Info);
                }


                //11Sep2016 Try setting the currently selected path back to rhome
                //if (rHome != null && rHome.Length > 0)
                    //confService.ModifyConfig("rhome", rHome.Replace('\\', '/'));

                logService.WriteToLogLevel("Using RHome = '" + rHome + "'", LogLevelEnum.Info);
                logService.WriteToLogLevel("Using RPath = '" + rPath + "'", LogLevelEnum.Info);

                StartupParameter sp = new StartupParameter(); sp.RHome = rHome;
                logService.WriteToLogLevel("Configuring and Initialization R Server...", LogLevelEnum.Info);

                if (rHome != null && rPath != null)
                {
                    logService.WriteToLogLevel("Setting R Environment Variables...", LogLevelEnum.Info);
                    REngine.SetEnvironmentVariables(rPath, rHome);
                }

                logService.WriteToLogLevel("Getting R instance...", LogLevelEnum.Info);
                this._RServer = REngine.GetInstance();

                logService.WriteToLogLevel("Initializing R Server...", LogLevelEnum.Info);
                this._RServer.Initialize();
                this._RServer.AutoPrint = false;

                logService.WriteToLogLevel("Setting R Personal Lib...", LogLevelEnum.Info);
                TrySettingUserPersonalLibrary(); ///TrySettingShippedRLibraryInFirstLocation

                logService.WriteToLogLevel("Setting R default Lib in the first location...", LogLevelEnum.Info);
                TrySettingShippedRLibraryInFirstLocation();

                logService.WriteToLogLevel("R DotNet initialized R server.", LogLevelEnum.Info);
                _log.WriteConsole("R.Net Initialized R server!!!", 1024, RDotNet.Internals.ConsoleOutputType.None);
            }
            catch (Exception ex)
            {
                string mss = "Binary compatibility between BlueSky Statistics and R. 64bit BlueSky Statistics requires 64bit R and 32bit " +
               "BlueSky Statistics requires 32bit R. (To find about BlueSky version : Go to 'Help > About' in BlueSky Statistics.";
                _log.WriteConsole("Unable to initialize R Server.(note: " + mss + ")", 5, RDotNet.Internals.ConsoleOutputType.None);

                logService.WriteToLogLevel("Unable to initialize R Server.", LogLevelEnum.Error, ex);
                throw new Exception();
            }
            logService.WriteToLogLevel("R  DotNet Server (deepest function call) initialization ended.", LogLevelEnum.Info);
        }

        //this method checks the RHome path string.
        private bool IsValidRInstallPath(string rHome, out string rinstallpath, out string rbinpath)
        {
            bool validInstallPath = false;
            rinstallpath = null;
            rbinpath = null;
            if (rHome != null && rHome.Length > 0 && Directory.Exists(rHome))//rHome path exists
            {
                string rPath = System.Environment.Is64BitProcess ? rHome + "\\bin\\x64" : rHome + "\\bin\\i386";
                bool direxists = Directory.Exists(rPath);
                bool dllexists = File.Exists(Path.Combine(rPath, "R.dll"));
                logService.WriteToLogLevel("RPath exists = " + direxists, LogLevelEnum.Info);
                logService.WriteToLogLevel("R.DLL exists = " + dllexists, LogLevelEnum.Info);
                if (direxists && dllexists)
                {
                    rinstallpath = rHome;
                    rbinpath = rPath;
                    validInstallPath = true;
                }
                else
                {
                    rinstallpath = null;
                    rbinpath = null;
                    validInstallPath = false;
                }
            }
            return validInstallPath;
        }

        //gets the latest R version
        private string GetLatestRInstallDirectory()
        {
            string rInstallPath = string.Empty;
            string rBinPath = string.Empty;
            string excepmsg = string.Empty;
            RegistryKey registryKeyLocal = null;
            RegistryKey registryKeyCurrent = null;
            RegistryKey registryKey = null;
            bool is64 = System.Environment.Is64BitProcess;
            logService.WriteToLogLevel("64-bit Process =" + is64, LogLevelEnum.Info);
            if (is64)
            {
                registryKeyLocal = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\R-core\R64");
                registryKeyCurrent = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\R-core\R64");
            }
            else
            {
                registryKeyLocal = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\R-core\R");
                registryKeyCurrent = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\R-core\R");
            }
            if (registryKeyCurrent != null)//priorty
            {
                registryKey = registryKeyCurrent;
            }
            else if (registryKeyLocal != null)
            {
                registryKey = registryKeyLocal;
            }
            else
            {
                logService.WriteToLogLevel("RegKey not found for 'Local Machine' or 'Current User':", LogLevelEnum.Info);
            }

            if (registryKey == null)
            {
                return (string.Empty);
            }

            logService.WriteToLogLevel("RegKey Local Machine: '" + registryKeyLocal + "'", LogLevelEnum.Info);
            logService.WriteToLogLevel("RegKey Current User: '" + registryKeyCurrent + "'", LogLevelEnum.Info);
            logService.WriteToLogLevel("Parent RegKey Chosen: '" + registryKey + "'", LogLevelEnum.Info);

            /////// Compare R Versions //////
            string[] RVersions = registryKey.GetSubKeyNames();
            string LatestRVer = FindHighestBuildVersion(RVersions);
            if (LatestRVer == null || LatestRVer.Trim().Length == 0)
                return string.Empty;
            logService.WriteToLogLevel("Highest R Version. '" + LatestRVer + "'", LogLevelEnum.Info);
            registryKey = registryKey.OpenSubKey(LatestRVer);

            logService.WriteToLogLevel("RegKey Chosen: '" + registryKey + "'", LogLevelEnum.Info);

            if (registryKey != null)
            {
                try
                {
                    rInstallPath = (string)registryKey.GetValue("InstallPath");
                    logService.WriteToLogLevel("R InstallPath: '" + rInstallPath + "'", LogLevelEnum.Info);
                }
                catch (SecurityException se)
                {
                    excepmsg = "The user does not have the permissions required to read from the registry key. ";
                    logService.WriteToLogLevel(excepmsg, LogLevelEnum.Info);
                    logService.WriteToLogLevel(se.Message, LogLevelEnum.Info);
                }
                catch (ObjectDisposedException ode)
                {
                    excepmsg = "The RegistryKey that contains the specified value is closed (closed keys cannot be accessed).";
                    logService.WriteToLogLevel(excepmsg, LogLevelEnum.Info);
                    logService.WriteToLogLevel(ode.Message, LogLevelEnum.Info);
                }
                catch (IOException ioe)
                {
                    excepmsg = "The RegistryKey that contains the specified value has been marked for deletion.  ";
                    logService.WriteToLogLevel(excepmsg, LogLevelEnum.Info);
                    logService.WriteToLogLevel(ioe.Message, LogLevelEnum.Info);
                }
                catch (ArgumentException ae)
                {
                    excepmsg = "options is not a valid RegistryValueOptions value; for example, an invalid value is cast to RegistryValueOptions. ";
                    logService.WriteToLogLevel(excepmsg, LogLevelEnum.Info);
                    logService.WriteToLogLevel(ae.Message, LogLevelEnum.Info);
                }
                catch (UnauthorizedAccessException uae)
                {
                    excepmsg = "The user does not have the necessary registry rights. ";
                    logService.WriteToLogLevel(excepmsg, LogLevelEnum.Info);
                    logService.WriteToLogLevel(uae.Message, LogLevelEnum.Info);
                }
                catch (Exception ex)
                {
                    excepmsg = "Error getting 'InstallPath' or 'Current Version' from Registry:";
                    logService.WriteToLogLevel(excepmsg, LogLevelEnum.Info);
                    logService.WriteToLogLevel(ex.Message, LogLevelEnum.Info);
                }
            }


            rBinPath = System.Environment.Is64BitProcess ? rInstallPath + "\\bin\\x64" : rInstallPath + "\\bin\\i386";
            logService.WriteToLogLevel("R bin path: '" + rBinPath + "'", LogLevelEnum.Info);

            return rInstallPath;
        }

        //Find the highest build versions among few.
        private string FindHighestBuildVersion(string[] verArr)
        {
            List<string> OriginalFormat = verArr.ToList<string>();

            int TotalRVer = verArr.Length;
            if (TotalRVer == 0) 
                return string.Empty;
            //Find count of numeric parts
            int maxnumparts = 0;
            for (int idx = 0; idx < TotalRVer; idx++)
            {
                if (verArr[idx] != null)
                {
                    int spcidx = verArr[idx].IndexOf(' ');
                    if (maxnumparts < verArr[idx].Split('.').Length)
                    {
                        maxnumparts = verArr[idx].Split('.').Length;
                    }
                }
            }

            int highestParts = maxnumparts + 1;

            for (int idx = 0; idx < TotalRVer; idx++)
            {
                if (verArr[idx] != null && verArr[idx].Contains(' '))
                {
                    int spcidx = verArr[idx].IndexOf(' ');
                    if (spcidx >= 0)
                    {
                        verArr[idx] = verArr[idx].Remove(spcidx, 1).Insert(spcidx, ".");
                    }
                }
            }


            int vernum = 0;
            int numpartsAdded = 0;
            bool hasstringpart = false;
            int stringpartindex = -1;
            StringBuilder newVerStr = new StringBuilder();
            //insert numparts if a version string has lesser numeparts. So all versions have equal numparts
            for (int idx = 0; idx < TotalRVer; idx++)
            {
                string[] verParts = verArr[idx].Split('.');
                newVerStr.Clear();//clear old values
                numpartsAdded = 0;
                hasstringpart = false;
                stringpartindex = -1;

                int currentVerParts = verParts.Length;
                for (int partidx = 0; partidx < currentVerParts; partidx++)//extract numpart
                {
                    if (int.TryParse(verParts[partidx], out vernum))//if the part was numeric then append it to final
                    {
                        newVerStr.Append(verParts[partidx]); //add numpart
                        newVerStr.Append("."); //add dot
                        numpartsAdded++;
                    }
                    else
                    {
                        hasstringpart = true;
                        stringpartindex = partidx;
                    }
                }

                int addParts = maxnumparts - numpartsAdded;
                //Add missing numparts to make all versions having same structure
                for (int partcount = 0; partcount < addParts; partcount++)//add numparts
                {
                    newVerStr.Append("0"); //add numpart
                    newVerStr.Append("."); //add dot
                }

                //Finally add string part.
                if (hasstringpart && stringpartindex > -1)
                {
                    newVerStr.Append(verParts[stringpartindex]); //add stringpart
                }
                else
                {
                    newVerStr.Append(" "); //add empty stringpart
                }

                verArr[idx] = newVerStr.ToString();
            }



            int selHigVeridx = 0;
            string higherVer = verArr[0];
            int res = 0;
            for (int i = 0; i < verArr.Length - 1; i++)
            {

                res = CompareBuildVersions(higherVer, verArr[i + 1]);
                if (res > 0)
                {
                    //do nothing
                }
                else
                {
                    higherVer = verArr[i + 1];
                    selHigVeridx = i + 1;
                }
            }

            //Now we need to get the registry version format
            if (selHigVeridx < OriginalFormat.Count)
                higherVer = OriginalFormat[selHigVeridx];
            return higherVer;
        }

        //Find the highest of two build versions. 
        private int CompareBuildVersions(string verA, string verB)
        {
            int result = 0;

            string[] verAarr = verA.Split('.');
            string[] verBarr = verB.Split('.');

            int arrlenA = verAarr.Length;
            int arrlenB = verBarr.Length;

            int loopcount = arrlenA > arrlenB ? arrlenA : arrlenB;// A is greater or B is greater/equal

            int number1 = 0;
            bool canConvert = false;

            List<string> verOrder = new List<string>();

            verOrder.Add("Patched");
            verOrder.Add("");
            verOrder.Add("RC");
            verOrder.Add("Pre-release");

            int highestPriorityNumber = 100;

            for (int i = 0; i < loopcount; i++)
            {
                int Aver = -1;
                int Bver = -1;
                //For A
                canConvert = int.TryParse(verAarr[i], out number1);
                if (!canConvert)
                {
                    if (verOrder.Contains(verAarr[i].Trim()))
                        Aver = highestPriorityNumber - verOrder.IndexOf(verAarr[i].Trim()); //100-1=99, 100-2=98
                    else if (verAarr[i].Trim().Length == 0)
                        Aver = 0;

                }
                else
                {
                    Aver = Int32.Parse(verAarr[i]); //if valid array index then extrct the number.
                }

                //For B
                canConvert = int.TryParse(verBarr[i], out number1);
                if (!canConvert)
                {
                    if (verOrder.Contains(verBarr[i].Trim()))
                        Bver = highestPriorityNumber - verOrder.IndexOf(verBarr[i].Trim());
                    else if (verBarr[i].Trim().Length == 0)
                        Bver = 0;
                }
                else
                {
                    Bver = Int32.Parse(verBarr[i]);
                }

                //finally compare versions number of a part
                if (Aver == Bver) continue;
                else if (Aver > Bver) { result = 1; break; }
                else { result = -1; break; }
            }
            return result;
        }


        public void Close()
        {
            logService.WriteToLogLevel("Closing R R.Net Server...", LogLevelEnum.Info);
            this._RServer.Dispose();
        }

        public DataFrame GetDataFrame(string dsname)
        {
            if (dsname == null)
                return null;
            bool dsexists = _RServer.Evaluate("exists(\"" + dsname + "\")").AsLogical()[0];
            if (!dsexists)
                return null;

            DataFrame _df = _RServer.GetSymbol(dsname).AsDataFrame();
            return _df;
        }

        //This function is only used to fetch two cols of a matrix
        //specific to fecthing dataset names from a R package.
        //This function can be made generic to use it for other things if needed.
        public CharacterMatrix GetChrMatrix(string command)
        {
            if (command == null)
                return null;
            //bool dsexists = _RServer.Evaluate("exists(\"" + dsname + "\")").AsLogical()[0];
            //if (!dsexists)
            //    return null;
            CharacterMatrix _chrmatrix = _RServer.Evaluate(command).AsCharacterMatrix();
            return _chrmatrix;
        } 

        #endregion

        #region Graphics Support
        public void AddGraphicsDevice(string DeviceName, object Device)
        {

        }
        #endregion

        #region XML
        public XmlDocument ParseToXmlDocument(string objectName)
        {
            XmlDocument doc = new XmlDocument();
            XmlNode rootNode = doc.CreateElement("Root");
            doc.AppendChild(rootNode);

            ParseToXmlNode(rootNode, objectName);
            //Fill UASUMMARY to existing DOM
            ParseUASummary(rootNode, objectName);
            return doc; ///XML DOM object created for several R command output
        }

        private void ParseObjectToXmlNode(XmlNode parent, object objectName)
        {

        }

        public void ParseToXmlNode(XmlNode parent, string objectName)
        {
            XmlNode thisNode;
            if (!(bool)this._RServer.Evaluate("is.null(" + objectName + ")").AsLogical()[0])//"is.null( tmp )"
            {
                object data = null;
                object executionStat = null; //19Jun2013  exeution status from return structure
                object returnval = null; //20Jun2013  result table value.
                string typeName = string.Empty;
                string classtype = string.Empty;
                try
                {
                    string cmd = "is.na(" + objectName + ")";
                    LogicalVector tms = this._RServer.Evaluate(cmd).AsLogical();

                    if (tms == null || tms.Length == 0)//no data to process
                    {
                        return;
                    }
                    if (tms.Length == 1)//single elements !tms.GetType().IsArray)
                    {
                        if (tms[0])
                        {
                            UADataType dataType = getUADataTypeFromName("String");
                            thisNode = parent.OwnerDocument.CreateElement(getElementTypeName(dataType));
                            parent.AppendChild(thisNode);
                            thisNode.InnerText = ".";
                            return;
                        }
                        else
                        {
                            object b = this._RServer.Evaluate("class(" + objectName + ")").AsCharacter()[0];
                            if (true)//factor
                                data = this._RServer.Evaluate("as.character(" + objectName + ")").AsCharacter()[0];//this lines read factor values ToString numeric
                            else
                                data = this._RServer.Evaluate(objectName);
                            if (b != null && b.ToString().ToLower().Equals("matrix"))
                                typeName = "String[,]";
                            else
                                typeName = data.GetType().Name;

                        }
                    }
                    else //For those commands that return multiple tables
                    {
                        classtype = this._RServer.Evaluate("class(" + objectName + ")").AsCharacter()[0];
                        data = this._RServer.Evaluate(objectName).AsList();

                        if (classtype.Equals("data.frame") && !objectName.Contains("metadatatable"))
                        {
                            typeName = "DataFrame";
                            data = this._RServer.Evaluate(objectName).AsCharacter();
                        }
                        else if (classtype.Trim().Equals("matrix"))//"character"
                        {
                            typeName = "String[,]";
                            if (typeName.Equals("String[,]"))
                            {
                                data = this._RServer.Evaluate(objectName).AsCharacterMatrix();
                            }
                            else if (typeName.Equals("String[]"))
                            {
                                data = this._RServer.Evaluate(objectName).AsCharacter();
                            }
                        }
                        else if (classtype.Equals("table") && !objectName.Contains("metadatatable"))
                        {
                            classtype = this._RServer.Evaluate("typeof(" + objectName + ")").AsCharacter()[0];
                            if (classtype.Trim().Equals("double")) //20Sep2013 for 2wayfreq
                            {
                                typeName = "Double[,]";
                                data = this._RServer.Evaluate(objectName).AsNumericMatrix();
                            }
                            else if (classtype.Trim().Equals("matrix"))//"character"
                            {
                                typeName = "String[,]";
                                if (typeName.Equals("String[,]"))
                                {
                                    data = this._RServer.Evaluate(objectName).AsCharacterMatrix();
                                }
                                else if (typeName.Equals("String[]"))
                                {
                                    data = this._RServer.Evaluate(objectName).AsCharacter();
                                }
                            }
                            else
                            {
                                typeName = "Int32[,]";//OR "matrix";
                                data = this._RServer.Evaluate(objectName).AsIntegerMatrix();
                            }
                        }
                        else if (classtype.Equals("data.frame") && objectName.Contains("metadatatable"))
                        {
                            typeName = "Object[]";
                            data = new string[8];
                        }
                        else if (classtype.Equals("list") && (data as GenericVector).Length == 13)
                        {
                            typeName = "String[]";
                            data = this._RServer.Evaluate(objectName).AsCharacter();
                        }
                        else if (classtype.Equals("character") || classtype.Equals("numeric") || classtype.Equals("integer"))
                        {
                            typeName = "String[]";
                            data = this._RServer.Evaluate(objectName).AsCharacter();
                        }
                        else
                        {
                            typeName = data.GetType().Name;
                            if (typeName.Equals("GenericVector"))
                                classtype = "list";
                        }
                    }
                }
                catch (Exception ex)
                {

                    if (data != null)
                        typeName = "Table[]";
                }
                ///////////Fix for One Smpl 13Feb2012 ///////
                if (classtype.Equals("list") && parent.Name.Equals("Root"))
                {
                    try
                    {
                        data = this._RServer.Evaluate(string.Format("{0}[[7]]", objectName)).AsCharacter()[0];
                        if (data != null && data.ToString().Length > 0)
                            typeName = "Table[]";
                    }
                    catch (Exception ee)
                    {
                        logService.WriteToLogLevel(ee.Message, LogLevelEnum.Error);
                    }
                }
                //////////////13Feb2012///////
                if (null != data)
                {
                    UADataType dataType = getUADataTypeFromName(typeName);
                    thisNode = parent.OwnerDocument.CreateElement(getElementTypeName(dataType));
                    parent.AppendChild(thisNode);

                    switch (typeName)
                    {
                        case "String":
                            thisNode.InnerText = (string)data;
                            break;

                        case "String[]":
                            string[] sList = (data as CharacterVector).ToArray();
                            foreach (String r in sList)
                            {
                                XmlNode row = thisNode.OwnerDocument.CreateElement("row");
                                row.InnerText = r;
                                thisNode.AppendChild(row);
                            }
                            returnVal = sList;//added for R.NET
                            break;

                        case "Double":
                            if (((double)data).ToString() == "-2146826246")
                                thisNode.InnerText = "NA";
                            else
                                thisNode.InnerText = ((double)data).ToString();
                            break;
                        case "Double[]":
                            double[] dList = (double[])data;
                            foreach (double r in dList)
                            {
                                XmlNode row = thisNode.OwnerDocument.CreateElement("row");
                                if (r.ToString() == "-2146826246")
                                    row.InnerText = "NA";
                                else
                                    row.InnerText = r.ToString();
                                thisNode.AppendChild(row);
                            }
                            break;
                        case "Double[,]":
                            GenerateSlicenameAndRowColHeaders(objectName, thisNode);

                            // Creating DOM using matrix data
                            NumericMatrix tempNM = (data as NumericMatrix);
                            long rowCount = tempNM != null ? tempNM.RowCount : 1;
                            long colCount = tempNM != null ? tempNM.ColumnCount : 1;
                            double[,] dMatrix = new double[rowCount, colCount]; ;
                            for (int mi = 0; mi < rowCount; mi++)
                            {
                                for (int mj = 0; mj < colCount; mj++)
                                {
                                    dMatrix[mi, mj] = tempNM != null ? tempNM[mi, mj] : Double.Parse(data.ToString());
                                }
                            }


                            XmlNode rows = thisNode.OwnerDocument.CreateElement("rows");
                            thisNode.AppendChild(rows);
                            for (long rIndex = 0; rIndex < rowCount; rIndex++)
                            {
                                XmlNode row = rows.OwnerDocument.CreateElement("row");
                                rows.AppendChild(row);
                                XmlNode columns = row.OwnerDocument.CreateElement("columns");
                                row.AppendChild(columns);
                                for (long cIndex = 0; cIndex < colCount; cIndex++)
                                {
                                    XmlNode col = columns.OwnerDocument.CreateElement("column");
                                    columns.AppendChild(col);
                                    if (dMatrix[rIndex, cIndex].ToString() == "-2146826246")
                                    {
                                        col.InnerText = "NA";
                                        continue;
                                    }
                                    double val = 0;
                                    double.TryParse(dMatrix[rIndex, cIndex].ToString(), out val);
                                    col.InnerText = val.ToString();
                                }
                            }
                            break;

                        case "String[,]":
                            CreateTableRowColumnHeaders(objectName, thisNode);

                            CharacterMatrix tempCM = (data as CharacterMatrix);//tempCM will be null when there is 1 row 1 col(1 cell only)
                            int mtxrcount = tempCM != null ? tempCM.RowCount : 1;//multi or single cell
                            int mtxccount = tempCM != null ? tempCM.ColumnCount : 1; //multi or single cell
                            String[,] sMatrix = new string[mtxrcount, mtxccount];
                            for (int mi = 0; mi < mtxrcount; mi++)
                            {
                                for (int mj = 0; mj < mtxccount; mj++)
                                {
                                    sMatrix[mi, mj] = tempCM != null ? tempCM[mi, mj] : (String)data;
                                }
                            }
                            ////////////


                            rowCount = sMatrix.GetLongLength(0);
                            colCount = sMatrix.GetLongLength(1);
                            rows = thisNode.OwnerDocument.CreateElement("rows");
                            thisNode.AppendChild(rows);
                            for (long rIndex = 0; rIndex < rowCount; rIndex++)
                            {
                                XmlNode row = rows.OwnerDocument.CreateElement("row");
                                rows.AppendChild(row);
                                XmlNode columns = row.OwnerDocument.CreateElement("columns");
                                row.AppendChild(columns);
                                for (long cIndex = 0; cIndex < colCount; cIndex++)
                                {
                                    XmlNode col = columns.OwnerDocument.CreateElement("column");
                                    columns.AppendChild(col);
                                    if (sMatrix[rIndex, cIndex].ToString() == "-2146826246")
                                    {
                                        col.InnerText = "NA";
                                        continue;
                                    }
                                    col.InnerText = sMatrix[rIndex, cIndex].ToString();
                                }
                            }
                            break;
                        case "Object[,]":

                            GenerateSlicenameAndRowColHeaders(objectName, thisNode);

                            object[,] oMatrix = (object[,])data;
                            rowCount = oMatrix.GetLongLength(0);
                            colCount = oMatrix.GetLongLength(1);
                            rows = thisNode.OwnerDocument.CreateElement("rows");
                            thisNode.AppendChild(rows);
                            for (long rIndex = 0; rIndex < rowCount; rIndex++)
                            {
                                XmlNode row = rows.OwnerDocument.CreateElement("row");
                                rows.AppendChild(row);
                                XmlNode columns = row.OwnerDocument.CreateElement("columns");
                                row.AppendChild(columns);
                                for (long cIndex = 0; cIndex < colCount; cIndex++)
                                {
                                    XmlNode col = columns.OwnerDocument.CreateElement("column");
                                    columns.AppendChild(col);
                                    if (oMatrix[rIndex, cIndex].ToString() == "-2146826246")
                                    {
                                        col.InnerText = "NA";
                                        continue;
                                    }
                                    col.InnerText = oMatrix[rIndex, cIndex].ToString();
                                }
                            }
                            break;

                        case "Int16":
                        case "Int32":
                        case "Int64":
                            if (((int)data).ToString() == "-2146826246")
                                thisNode.InnerText = "NA";
                            else
                                thisNode.InnerText = ((int)data).ToString();
                            break;

                        case "Int16[]":
                        case "Int32[]":
                        case "Int64[]":
                            int[] iList = (int[])data;
                            try
                            {
                                CreateTableRowColumnHeaders(objectName, thisNode);
                                //filling data in DOM
                                rows = thisNode.OwnerDocument.CreateElement("rows");
                                thisNode.AppendChild(rows);

                                foreach (double r in iList)
                                {
                                    XmlNode row = rows.OwnerDocument.CreateElement("row");
                                    if (r.ToString() == "-2146826246")
                                    {
                                        row.InnerText = "NA";
                                        continue;
                                    }
                                    row.InnerText = r.ToString();
                                    rows.AppendChild(row);
                                }
                                break;
                            }
                            catch { }

                            //filling data in DOM
                            foreach (double r in iList)
                            {
                                XmlNode row = thisNode.OwnerDocument.CreateElement("row");
                                if (r.ToString() == "-2146826246")
                                {
                                    row.InnerText = "NA";
                                    continue;
                                }
                                row.InnerText = r.ToString();
                                thisNode.AppendChild(row);
                            }
                            break;

                        case "Int16[,]":
                        case "Int32[,]":
                        case "Int64[,]":
                            CreateTableRowColumnHeaders(objectName, thisNode, classtype);

                            int[,] iMatrix;
                            //for single row R 'table'.
                            string datatype = data.GetType().Name;
                            if (datatype.Equals("Int16[]") || datatype.Equals("Int32[]") || datatype.Equals("Int64[]"))
                            {
                                int[] tmparr = (int[])data;
                                int colsize = tmparr.Length;
                                iMatrix = new int[1, colsize];
                                int i = 0;
                                foreach (int v in tmparr)
                                {
                                    iMatrix[0, i] = tmparr[i];
                                    i++;
                                }
                            }
                            else
                            {
                                iMatrix = (int[,])data;
                            }
                            ////// Creating DOM using matrix data
                            rowCount = iMatrix.GetLongLength(0);
                            colCount = iMatrix.GetLongLength(1);
                            rows = thisNode.OwnerDocument.CreateElement("rows");
                            thisNode.AppendChild(rows);
                            for (long rIndex = 0; rIndex < rowCount; rIndex++)
                            {
                                XmlNode row = rows.OwnerDocument.CreateElement("row");
                                rows.AppendChild(row);
                                XmlNode columns = row.OwnerDocument.CreateElement("columns");
                                row.AppendChild(columns);
                                for (long cIndex = 0; cIndex < colCount; cIndex++)
                                {
                                    XmlNode col = columns.OwnerDocument.CreateElement("column");
                                    columns.AppendChild(col);
                                    if (iMatrix[rIndex, cIndex].ToString() == "-2146826246")
                                    {
                                        col.InnerText = "NA";
                                        continue;
                                    }
                                    col.InnerText = iMatrix[rIndex, cIndex].ToString();
                                }
                            }
                            break;

                        case "Object[]":
                            {
                                Object[] oList = (Object[])data;

                                int len = oList.Length;

                                for (int index = 1; index <= len; index++)
                                {
                                    if (index == 3 || index == 7 || index == 8)
                                        ParseToXmlNode(thisNode, "as.character(" + objectName + "[[" + index.ToString() + "]])");
                                    else
                                        ParseToXmlNode(thisNode, objectName + "[[" + index.ToString() + "]]");
                                }

                                break;
                            }
                        case "Table[]":
                            {
                                data = this._RServer.Evaluate(string.Format("{0}[[7]]", objectName)).AsCharacter()[0];
                                int numberOfTables = 0;
                                if (Int32.TryParse(data.ToString(), out numberOfTables))//try converting to number.
                                { }

                                int objectcountintable = 0;
                                bool isewtable = false;
                                for (int i = 1; i <= numberOfTables; ++i)
                                {
                                    tableheader = null;
                                    string tabletype = "resulttable";


                                    ///// finding the table type  //////
                                    bool tablepropsnamesexists = this._RServer.Evaluate(string.Format("!is.null(names({0}$tables[[{1}]]))", objectName, i)).AsLogical()[0];
                                    objectcountintable = this._RServer.Evaluate(string.Format("length({0}$tables[[{1}]])", objectName, i)).AsInteger()[0];
                                    if (this._RServer.Evaluate(string.Format("!is.null(names({0}$tables)[[{1}]])", objectName, i)).AsLogical()[0])//01May2014
                                    {
                                        tableheader = this._RServer.Evaluate(string.Format("names({0}$tables)[[{1}]]", objectName, i)).AsCharacter()[0];
                                    }
                                    if (objectcountintable > 1 && tablepropsnamesexists)
                                    {

                                        bool hasType = false;
                                        bool isatomic = this._RServer.Evaluate(string.Format("is.atomic({0}$tables[[{1}]])", objectName, i)).AsLogical()[0];
                                        if (!isatomic)
                                            hasType = this._RServer.Evaluate(string.Format("!is.null({0}$tables[[{1}]]$type)", objectName, i)).AsLogical()[0];

                                        if (hasType)
                                            tabletype = this._RServer.Evaluate(string.Format("{0}$tables[[{1}]]$type", objectName, i)).AsCharacter()[0];

                                        bool hasColNames = this._RServer.Evaluate(string.Format("!is.null({0}$tables[[{1}]]$columnNames)", objectName, i)).AsLogical()[0];
                                        if (hasColNames)
                                        {
                                            XmlElement colnames = parent.OwnerDocument.CreateElement("ColNames");
                                            colnames.SetAttribute("tablenumber", i.ToString());
                                            ParseToXmlNode(colnames, string.Format("{0}$tables[[{1}]]$columnNames", objectName, i));
                                            thisNode.AppendChild(colnames);
                                        }

                                    }


                                    if (tabletype.Equals("table") || tabletype.Equals("ewtable")) // error warning table
                                    {
                                        isewtable = true;//21sep2013
                                        ParseToXmlNode(thisNode, string.Format("{0}$tables[[{1}]]$datatable", objectName, i));

                                        dynamic res = this._RServer.Evaluate(string.Format("{0}$tables[[{1}]]$metadata", objectName, i)).AsCharacter()[0];///tmp[[8]][[1]]$metadata
                                        string isMetadata = res;
                                        if (isMetadata == "yes")
                                        {
                                            int noMetadata = int.Parse(this._RServer.Evaluate(string.Format("{0}$tables[[{1}]]$nometadatatables", objectName, i)).AsInteger()[0].ToString());
                                            string[] Metadatanames = null;
                                            if (noMetadata == 1)
                                            {
                                                Metadatanames = new string[1];
                                                Metadatanames[0] = (string)this._RServer.Evaluate(string.Format("{0}$tables[[{1}]]$metadatatabletype", objectName, i)).AsCharacter()[0];
                                            }
                                            else
                                            {
                                                string str = (string)this._RServer.Evaluate(string.Format("{0}$tables[[{1}]]$metadatatabletype", objectName, i)).AsCharacter()[0];
                                                Metadatanames = new string[noMetadata];
                                                for (int j = 0; j < noMetadata; j++)
                                                    Metadatanames[j] = str + (j + 1).ToString();//right now we are assigning same name + index to each matadatatable related to single datatable

                                            }

                                            XmlElement metadatanodes = null;
                                            if (tabletype.Equals("table"))
                                                metadatanodes = parent.OwnerDocument.CreateElement("Metadata");//for analytic
                                            else if (tabletype.Equals("ewtable"))
                                                metadatanodes = parent.OwnerDocument.CreateElement("BSkyErrorWarn");//for error warning

                                            metadatanodes.SetAttribute("tablenumber", i.ToString());
                                            for (int metatableId = 1; metatableId <= noMetadata; ++metatableId)
                                            {
                                                XmlElement metanode = parent.OwnerDocument.CreateElement(Metadatanames[metatableId - 1]);//);[0]+metatableId.ToString()// each metadatatable will hv diff name in DOM
                                                ParseToXmlNode(metanode, string.Format("{0}$tables[[{1}]]$metadatatable[[{2}]]", objectName, i, metatableId));
                                                metadatanodes.AppendChild(metanode);
                                            }
                                            thisNode.AppendChild(metadatanodes);
                                        }
                                    }//tabletype table or ewtable

                                    else
                                    {
                                        returnVal = this._RServer.Evaluate(string.Format("{0}$tables[[{1}]]", objectName, i));

                                        if (!isewtable)
                                        {
                                            ParseToXmlNode(thisNode, string.Format("{0}$tables[[{1}]]", objectName, i));
                                            continue;//move to next table
                                        }


                                        ////Putting User Results in DOM
                                        int totalusertables = numberOfTables - i + 1;
                                        XmlElement userresult = null;
                                        userresult = parent.OwnerDocument.CreateElement("UserResult");//for analytic

                                        for (int tno = 1; tno <= totalusertables; ++tno, i++)
                                        {
                                            //Following will check if ewtable exists in between user tables
                                            tablepropsnamesexists = this._RServer.Evaluate(string.Format("!is.null(names({0}$tables[[{1}]]))", objectName, i)).AsLogical()[0];
                                            objectcountintable = this._RServer.Evaluate(string.Format("length({0}$tables[[{1}]])", objectName, i)).AsInteger()[0];

                                            if (objectcountintable > 1 && tablepropsnamesexists)
                                            {
                                                bool hasType = false;
                                                bool isatomic = this._RServer.Evaluate(string.Format("is.atomic({0}$tables[[{1}]])", objectName, i)).AsLogical()[0];
                                                if (!isatomic)
                                                    hasType = this._RServer.Evaluate(string.Format("!is.null({0}$tables[[{1}]]$type)", objectName, i)).AsLogical()[0];

                                                if (hasType)
                                                {
                                                    tabletype = this._RServer.Evaluate(string.Format("{0}$tables[[{1}]]$type", objectName, i)).AsCharacter()[0];
                                                    if (tabletype.Equals("ewtable"))
                                                    {
                                                        i--;
                                                        break;
                                                    }
                                                }
                                            }

                                            ////// Processing one of the user tables ////
                                            XmlElement udata = parent.OwnerDocument.CreateElement("UserData");
                                            udata.SetAttribute("tablenumber", tno.ToString());
                                            ParseToXmlNode(udata, string.Format("{0}$tables[[{1}]]", objectName, i));
                                            userresult.AppendChild(udata);
                                        }
                                        thisNode.AppendChild(userresult);
                                    }
                                }// for loop on tables

                            }
                            break;
                        case "DataFrame":
                            {
                                rowCount = this._RServer.Evaluate("nrow(" + objectName + ")").AsInteger()[0];
                                colCount = this._RServer.Evaluate("ncol(" + objectName + ")").AsInteger()[0];
                                String[,] dfMatrix = new string[rowCount, colCount];
                                for (int jj = 0; jj < colCount; jj++)
                                {
                                    string[] coldata = null;


                                    int colscount = this._RServer.Evaluate("length(" + objectName + "[," + (jj + 1) + "])").AsInteger()[0];
                                    if (colscount > 1)
                                    {
                                        CharacterVector cv = this._RServer.Evaluate("as.character(" + objectName + "[," + (jj + 1) + "])").AsCharacter();
                                        int siz = cv.Count();
                                        coldata = new string[siz];
                                        for (int ic = 0; ic < siz; ic++)
                                        {
                                            coldata[ic] = cv[ic];
                                        }

                                    }
                                    else if (colscount == 1)
                                    {
                                        string coname = this._RServer.Evaluate("as.character(" + objectName + "[," + (jj + 1) + "])").AsCharacter()[0];
                                        coldata = new string[1];
                                        coldata[0] = coname;
                                    }
                                    else
                                    {
                                        break;
                                    }

                                    for (int ii = 0; ii < coldata.Length; ii++)
                                    {
                                        dfMatrix[ii, jj] = coldata[ii];
                                    }
                                }

                                //Creating DOM for row col data, using array from above
                                rows = thisNode.OwnerDocument.CreateElement("rows");
                                thisNode.AppendChild(rows);
                                for (long rIndex = 0; rIndex < rowCount; rIndex++)
                                {
                                    XmlNode row = rows.OwnerDocument.CreateElement("row");
                                    rows.AppendChild(row);
                                    XmlNode columns = row.OwnerDocument.CreateElement("columns");
                                    row.AppendChild(columns);
                                    for (long cIndex = 0; cIndex < colCount; cIndex++)
                                    {
                                        XmlNode col = columns.OwnerDocument.CreateElement("column");
                                        columns.AppendChild(col);
                                        if (dfMatrix[rIndex, cIndex].ToString() == "-2146826246")
                                        {
                                            col.InnerText = "NA";
                                            continue;
                                        }
                                        col.InnerText = dfMatrix[rIndex, cIndex].ToString();
                                    }
                                }

                            }
                            break;
                    }//switch
                }
            }
        }

        //29Aug 2013 For generating extra tags 
        private void GenerateSlicenameAndRowColHeaders(string objectName, XmlNode thisNode)
        {
            //Creating row col headers if any present on R side object
            string[] objcolheaders = null;
            string[] objrowheaders = null;
            string objslicetitlecommand = string.Empty;
            string objslicetitle = string.Empty;
            //finding slice name
            if (objectName.Contains("$datatable"))
            {
                objslicetitlecommand = objectName.Replace("$datatable", "$cartlevel");
                if (!this._RServer.Evaluate("is.null(" + objslicetitlecommand + ")").AsLogical()[0])
                    objslicetitle = this._RServer.Evaluate(objslicetitlecommand).AsCharacter()[0];
                XmlElement sliceTitletag = thisNode.OwnerDocument.CreateElement("slicename");
                sliceTitletag.InnerText = objslicetitle.Replace("<", "&lt;").Replace(">", "&gt;").Replace("<=", "&le;").Replace(">=", "&ge;");
                thisNode.AppendChild(sliceTitletag);

            }
            if (!this._RServer.Evaluate("is.null(colnames(" + objectName + "))").AsLogical()[0])
            {
                CharacterVector cv = this._RServer.Evaluate("colnames(" + objectName + ")").AsCharacter();
                int siz = cv.Count();
                objcolheaders = new string[siz];
                for (int ic = 0; ic < siz; ic++)
                {
                    objcolheaders[ic] = cv[ic];
                }
            }
            if (!this._RServer.Evaluate("is.null(rownames(" + objectName + "))").AsLogical()[0])
            {
                CharacterVector cv = this._RServer.Evaluate("rownames(" + objectName + ")").AsCharacter();
                int siz = cv.Count();
                objrowheaders = new string[siz];
                for (int ic = 0; ic < siz; ic++)
                {
                    objrowheaders[ic] = cv[ic];
                }
            }

            //01May2014 table header
            XmlElement strtableheader = thisNode.OwnerDocument.CreateElement("tableheader");
            if (tableheader != null && tableheader.Length > 0)
                strtableheader.InnerText = tableheader;//Table header assigned
            thisNode.AppendChild(strtableheader);

            XmlElement objcolnames = thisNode.OwnerDocument.CreateElement("colheaders");
            if (objcolheaders != null)
            {
                string innertxt = string.Join(",", objcolheaders);//Array to comma separated string
                objcolnames.InnerText = innertxt.Replace("<", "&lt;").Replace(">", "&gt;").Replace("<=", "&le;").Replace(">=", "&ge;");
            }
            XmlElement objrownames = thisNode.OwnerDocument.CreateElement("rowheaders");
            if (objrowheaders != null)
            {
                string innertxt = string.Join(",", objrowheaders);//Array to comma separated string
                objrownames.InnerText = innertxt.Replace("<", "&lt;").Replace(">", "&gt;").Replace("<=", "&le;").Replace(">=", "&ge;");
            }
            thisNode.AppendChild(objcolnames);
            thisNode.AppendChild(objrownames);
        }

        private void CreateTableRowColumnHeadersOld(string objectName, XmlNode thisNode, string classtype = "")
        {
            //Creating row col headers if any present on R side object. 
            string[] strcolheaders = null;
            string[] strrowheaders = null;
            int srdim = 1, scdim = 1;//array with at least 1 row 1 col

            if (!this._RServer.Evaluate("is.na(ncol(" + objectName + "))").AsLogical()[0]) // if no. of col does exists 
            {
                scdim = this._RServer.Evaluate("ncol(" + objectName + ")").AsInteger()[0];
                if (!this._RServer.Evaluate("is.null(colnames(" + objectName + "))").AsLogical()[0])
                {
                    CharacterVector cv = this._RServer.Evaluate("colnames(" + objectName + ")").AsCharacter();
                    int siz = cv.Count();
                    strcolheaders = new string[siz];
                    for (int ic = 0; ic < siz; ic++)
                    {
                        strcolheaders[ic] = cv[ic];
                    }
                }
            }
            if (!this._RServer.Evaluate("is.na(nrow(" + objectName + "))").AsLogical()[0]) // if no. of row does exists 
            {
                srdim = this._RServer.Evaluate("nrow(" + objectName + ")").AsInteger()[0];
                if (!this._RServer.Evaluate("is.null(rownames(" + objectName + "))").AsLogical()[0])
                {
                    CharacterVector cv = this._RServer.Evaluate("rownames(" + objectName + ")").AsCharacter();
                    int siz = cv.Count();
                    strrowheaders = new string[siz];
                    for (int ic = 0; ic < siz; ic++)
                    {
                        strrowheaders[ic] = cv[ic];
                    }
                }
            }

            //this section for int16[,], in32[,], int64[,].
            if (srdim > 1 && scdim == 1 && classtype.Equals("table"))
            {
                strcolheaders = strrowheaders;
                strrowheaders = null;
            }

            //01May2014 table header
            XmlElement strtableheader = thisNode.OwnerDocument.CreateElement("tableheader");
            if (tableheader != null && tableheader.Length > 0)
                strtableheader.InnerText = tableheader;//Table header assigned
            thisNode.AppendChild(strtableheader);


            //Col Row Headers
            XmlElement strcolnames = thisNode.OwnerDocument.CreateElement("colheaders");
            if (strcolheaders != null)
                strcolnames.InnerText = string.Join(",", strcolheaders);//Array to comma separated string
            XmlElement strrownames = thisNode.OwnerDocument.CreateElement("rowheaders");
            if (strrowheaders != null)
                strrownames.InnerText = string.Join(",", strrowheaders);//Array to comma separated string
            thisNode.AppendChild(strcolnames);
            thisNode.AppendChild(strrownames);
        }

        private void CreateTableRowColumnHeaders(string objectName, XmlNode thisNode, string classtype = "")
        {
            //Creating row col headers if any present on R side object. 
            string[] strcolheaders = null;
            string[] strrowheaders = null;
            int srdim = 1, scdim = 1;//array with at least 1 row 1 col

            if (!this._RServer.Evaluate("is.na(ncol(" + objectName + "))").AsLogical()[0]) // if no. of col does exists 
            {
                scdim = this._RServer.Evaluate("ncol(" + objectName + ")").AsInteger()[0];
                if (!this._RServer.Evaluate("is.null(colnames(" + objectName + "))").AsLogical()[0])
                {
                    CharacterVector cv = this._RServer.Evaluate("colnames(" + objectName + ")").AsCharacter();
                    int siz = cv.Count();
                    strcolheaders = new string[siz];
                    for (int ic = 0; ic < siz; ic++)
                    {
                        strcolheaders[ic] = cv[ic];
                    }
                }
            }
            if (!this._RServer.Evaluate("is.na(nrow(" + objectName + "))").AsLogical()[0]) // if no. of row does exists 
            {
                srdim = this._RServer.Evaluate("nrow(" + objectName + ")").AsInteger()[0];
                if (!this._RServer.Evaluate("is.null(rownames(" + objectName + "))").AsLogical()[0])
                {
                    CharacterVector cv = this._RServer.Evaluate("rownames(" + objectName + ")").AsCharacter();
                    int siz = cv.Count();
                    strrowheaders = new string[siz];
                    for (int ic = 0; ic < siz; ic++)
                    {
                        strrowheaders[ic] = cv[ic];
                    }
                }
            }

            //this section for int16[,], in32[,], int64[,]. 
            if (srdim > 1 && scdim == 1 && classtype.Equals("table"))
            {
                strcolheaders = strrowheaders;
                strrowheaders = null;
            }

            //01May2014 table header
            XmlElement strtableheader = thisNode.OwnerDocument.CreateElement("tableheader");
            if (tableheader != null && tableheader.Length > 0)
                strtableheader.InnerText = tableheader;//Table header assigned
            thisNode.AppendChild(strtableheader);


            //Col Row Headers
            XmlElement strcolnames = thisNode.OwnerDocument.CreateElement("colheaders");
            if (strcolheaders != null)
                strcolnames.InnerText = string.Join(",", strcolheaders);//Array to comma separated string
            XmlElement strrownames = thisNode.OwnerDocument.CreateElement("rowheaders");
            if (strrowheaders != null)
                strrownames.InnerText = string.Join(",", strrowheaders);//Array to comma separated string
            thisNode.AppendChild(strcolnames);
            thisNode.AppendChild(strrownames);
        }

        public void ParseUASummary(XmlNode parent, string objectName)
        {
            object data = null;
            if (!(bool)this._RServer.Evaluate("is.null(" + objectName + ")").AsLogical()[0])//"is.null( tmp )"
            {
                try
                {
                    if (int.Parse(this._RServer.Evaluate("length(" + objectName.Trim() + ")").AsInteger()[0].ToString()) >= 6)
                    {
                        if (!(bool)this._RServer.Evaluate("is.na(" + "names(" + objectName.Trim() + "[6])" + ")").AsLogical()[0])
                        {
                            if (this._RServer.Evaluate("names(" + objectName.Trim() + "[6])").AsCharacter()[0].ToString() == "uasummary")
                            {
                                data = this._RServer.Evaluate("length(" + objectName.Trim() + "[[6]])").AsInteger()[0];
                                int notesize = int.Parse(data.ToString());
                                //create node for UASummary
                                XmlElement xe_uas = parent.OwnerDocument.CreateElement("UASummary");
                                XmlElement xe_ual = parent.OwnerDocument.CreateElement("UAList");
                                XmlElement xe_str = null;
                                string innrtxt = string.Empty;
                                data = this._RServer.Evaluate(string.Format("{0}[[6]]", objectName)).AsCharacter().ToArray();///uasummary [[6]]
                                if (data.GetType().IsArray)
                                {
                                    object[] newarr = (object[])data;
                                    for (int i = 0; i < notesize; ++i)
                                    {
                                        if (newarr[i].ToString() == "-2146826288" ||
                                            newarr[i].ToString() == "-2146826246")
                                            innrtxt = string.Empty;
                                        else
                                            innrtxt = newarr[i].ToString();
                                        xe_str = parent.OwnerDocument.CreateElement("UAString");
                                        xe_str.InnerText = innrtxt;
                                        xe_ual.AppendChild(xe_str);
                                        xe_str = null;
                                    }
                                    xe_uas.AppendChild(xe_ual);
                                    parent.AppendChild(xe_uas);//finally adding to main DOM
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    logService.WriteToLogLevel("Could not parse:", LogLevelEnum.Error);
                }
            }
        }

        public object GetReturnValues(string objname)//14jun2013
        {
            object returnval = null;

            try
            {
                this._RServer.Evaluate("tmp<-" + objname);
            }
            catch (Exception e)
            {
                logService.WriteToLogLevel("Could not execute: < " + objname + " >", LogLevelEnum.Error);
            }
            if (false)
            {

            }
            else if (!Conversions.ToBoolean(this._RServer.Evaluate("is.null(tmp)").AsLogical()[0]))
            {
                returnval = this._RServer.Evaluate("tmp").AsList();
            }
            return returnval;
        }

        private UADataType getUADataTypeFromName(string typeName)
        {
            UADataType DataType = UADataType.UAUnKnown;
            switch (typeName)
            {
                case "String":
                    DataType = UADataType.UAString;
                    break;
                case "String[]":
                    DataType = UADataType.UAStringList;
                    break;

                case "Double":
                    DataType = UADataType.UADouble;
                    break;
                case "Double[]":
                    DataType = UADataType.UADoubleList;
                    break;
                case "Double[,]":
                case "Object[,]":
                case "String[,]":
                    DataType = UADataType.UADoubleMatrix;
                    break;

                case "Int16":
                case "Int32":
                case "Int64":
                    DataType = UADataType.UAInt;
                    break;

                case "Int16[]":
                case "Int32[]":
                case "Int64[]":
                    DataType = UADataType.UAIntList;
                    break;

                case "Int16[,]":
                case "Int32[,]":
                case "Int64[,]":
                    DataType = UADataType.UADoubleMatrix; ;
                    break;

                case "Object[]":
                    DataType = UADataType.UAList;
                    break;
                case "Table[]":
                    DataType = UADataType.UATableList;
                    break;
                case "DataFrame":  ///03Jul2013

                    DataType = UADataType.UADoubleMatrix;
                    break;
            }

            return DataType;
        }

        private string getElementTypeName(UADataType dataType)
        {
            return dataType.ToString();
        }
        #endregion

        #region Command Execution
        public XmlDocument EvaluateToXml(string commandString)
        {
            XmlDocument returnValue;
            try
            {
                //16Apr2013///
                bool batchcommand = false;

                try
                {
                    if (!batchcommand)
                    {

                        bool runit = true;
                        if (runit)
                            this._RServer.Evaluate("tmp<-" + commandString); // executing R Command with no left-hand var
                    }
                    else
                        this._RServer.Evaluate(commandString);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(BSky.GlobalResources.Properties.Resources.ErrorExecuting + "\n"
                        + commandString + "\n"
                        + ex.Message,
                        BSky.GlobalResources.Properties.Resources.ErrorExecutingTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    logService.WriteToLogLevel("Could not execute:\n " + commandString, LogLevelEnum.Error);
                    return null;
                }
            }
            catch (Exception e)
            {
                string errm = "RDotNet: Error message not implemented";
                logService.WriteToLogLevel("Could not execute: < " + commandString + " >", LogLevelEnum.Error);
            }
            if (false)
            {
                returnValue = null;

            }
            else if (!Conversions.ToBoolean(this._RServer.Evaluate("is.null(tmp)").AsLogical()[0]))
            {
                returnValue = ParseToXmlDocument("tmp");
            }
            else
            {
                returnValue = null;
            }

            return returnValue;
        }

        //for R Dot net only
        public string[] GetRow(string command)
        {
            string[] rowdata = null;
            SymbolicExpression se = null;
            CharacterVector cv = null;
            try
            {

                se = this._RServer.Evaluate(command);
                cv = se.AsCharacter();
                rowdata = cv.ToArray();

            }
            catch (Exception ex)
            {
                logService.WriteToLogLevel("command: '" + command + "' " + ex.Message, LogLevelEnum.Error);
            }
            return rowdata;
        }
        object returnVal = null;
        ///14Jun2013 For new R framework. 
        public UAReturn EvaluateToUAReturn(string commandString)
        {
            UAReturn returnRecults = new UAReturn();
            returnRecults.CommandString = commandString;
            XmlDocument returnErrWarn;

            try
            {
                //16Apr2013///
                bool batchcommand = false;
                if (!batchcommand)
                    this._RServer.Evaluate("tmp<-" + commandString); // executing R Command with no left-hand var
                else
                    this._RServer.Evaluate(commandString);
            }
            catch (Exception e)
            {
                string errm = "R.NET Error Msg not implemented";
                logService.WriteToLogLevel("Could not execute: < " + commandString + " >", LogLevelEnum.Error);
            }
            if (false)
            {
                returnErrWarn = null;

            }
            else if (!Conversions.ToBoolean(this._RServer.Evaluate("is.null(tmp)").AsLogical()[0]))
            {
                returnErrWarn = ParseToXmlDocument("tmp");

            }
            else
            {
                returnErrWarn = null;
            }

            returnRecults.Data = returnErrWarn;
            returnRecults.SimpleTypeData = returnVal;// "Put results here. 
            returnVal = null;
            return returnRecults;
        }


        public object EvaluateToObject(string commandString, bool hasReturn)
        {
            object returnValue;

            try
            {
                this._RServer.Evaluate("tmp <- " + commandString);
            }
            catch (Exception e)
            {
                logService.WriteToLogLevel("Could not execute: < " + commandString + " >", LogLevelEnum.Error);
            }
            if (false)
            {
                returnValue = "Error: " + "R.Net's error not impl";
            }
            else if (!Conversions.ToBoolean(this._RServer.Evaluate("is.null(tmp)").AsLogical()[0]))
            {
                SymbolicExpression se = this._RServer.Evaluate("tmp");
                switch (se.Type.ToString())
                {
                    case "CharacterVector":
                        CharacterVector cv = se.AsCharacter();
                        if (cv.Length == 0)
                        {
                            returnValue = null;
                        }
                        else if (cv.Length == 1)
                        {
                            returnValue = cv[0];
                        }
                        else
                        {
                            returnValue = cv.ToArray();
                        }
                        break;
                    case "IntegerVector":
                        IntegerVector iv = se.AsInteger();
                        if (iv.Length == 0)
                        {
                            returnValue = null;
                        }
                        else if (iv.Length == 1)
                        {
                            returnValue = iv[0];
                        }
                        else
                        {
                            returnValue = iv.ToArray();
                        }
                        break;
                    case "NumericVector":
                        NumericVector nv = se.AsNumeric();
                        if (nv.Length == 0)
                        {
                            returnValue = null;
                        }
                        else if (nv.Length == 1)
                        {
                            returnValue = nv[0];
                        }
                        else
                        {
                            returnValue = nv.ToArray();
                        }
                        break;
                    case "LogicalVector":
                        LogicalVector lv = se.AsLogical();
                        if (lv.Length == 0)
                        {
                            returnValue = null;
                        }
                        else if (lv.Length == 1)
                        {
                            returnValue = lv[0];
                        }
                        else
                        {
                            returnValue = lv.ToArray();
                        }
                        break;
                    case "List"://24Nov2015
                        returnValue = null;
                        if (se.IsList() && se.IsDataFrame())
                        {
                            DataFrame tmpdf = se.AsDataFrame();
                            returnValue = tmpdf;
                        }

                        break;
                    default:
                        returnValue = null;
                        break;
                }
            }
            else
            {
                returnValue = "No Result - Check Command";
            }

            return returnValue;
        }

        //05Mar2015 for fetching all column properties at once
        public SymbolicExpression EvaluateToSymExp(string commandString)
        {
            SymbolicExpression returnValue;

            try
            {
                this._RServer.Evaluate("tmp<-" + commandString);
            }
            catch (Exception e)
            {
                logService.WriteToLogLevel("Could not execute: < " + commandString + " >", LogLevelEnum.Error);
            }
            if (!Conversions.ToBoolean(this._RServer.Evaluate("is.null(tmp)").AsLogical()[0]))
            {
                returnValue = this._RServer.Evaluate("tmp");
            }
            else
            {
                returnValue = null;
            }

            return returnValue;
        }
        /// <summary>
        /// Syntax Editor will use this
        /// </summary>
        /// <param name="commandString"></param>
        public object SyntaxEditorEvaluateToObject_old(string commandString, bool hasReturn, bool hasUAReturn)
        {
            object returnValue = null;
            dynamic dy = null;
            try
            {
                if (hasReturn)
                {
                    dy = this._RServer.Evaluate(commandString).AsList();
                    if (dy != null)
                        returnValue = dy;
                }
                else if (hasUAReturn)
                {
                    returnValue = this.EvaluateToUAReturn(commandString);
                }
                else
                {
                    string serr = "R.Net Error not imple";
                    if (serr != null && serr.Length > 0)
                    { }
                    this._RServer.Evaluate(commandString);
                }

            }
            catch (Exception e)
            {
                if (e != null)
                {

                }
                if (commandString.Contains("readLines("))
                {
                    returnValue = "EOF";
                }
                else if (false)
                {
                    returnValue = "Error: " + "No Err impl, R.net";// this._RServer.GetErrorText();
                }
            }

            return returnValue;
        }

        //for syntax
        public object SyntaxEditorEvaluateToObject(string commandString, bool hasReturn, bool hasUAReturn)
        {
            AdvancedLogging = AdvancedLoggingService.AdvLog;
            if (AdvancedLogging) logService.WriteToLogLevel("Command executing in R : " + commandString, LogLevelEnum.Info);

            object returnValue = null;
            dynamic dy = null;
            CharacterVector cvec;
            try
            {
                if (hasReturn)
                {
                    if (AdvancedLogging) logService.WriteToLogLevel("Executing in hasReturn:", LogLevelEnum.Info);
                    cvec = this._RServer.Evaluate(commandString).AsCharacter();
                    if (cvec != null)
                    {
                        if (cvec.Length > 1)
                        {
                            returnValue = cvec.ToArray();
                        }
                        if (cvec.Length == 1)
                        {
                            returnValue = cvec[0];
                        }

                    }
                }
                else if (hasUAReturn)
                {
                    if (AdvancedLogging) logService.WriteToLogLevel("Executing in hasUAReturn:", LogLevelEnum.Info);
                    returnValue = this.EvaluateToUAReturn(commandString);
                }
                else//no return
                {
                    if (AdvancedLogging) logService.WriteToLogLevel("Executing in no-return:", LogLevelEnum.Info);
                    string serr = "R.Net Error not imple";// this._RServer.GetErrorText();
                    if (serr != null && serr.Length > 0)
                    { }
                    if (AdvancedLogging) logService.WriteToLogLevel("Just before command Execution in R.NET", LogLevelEnum.Info);
                    this._RServer.Evaluate(commandString);

                    if (AdvancedLogging) logService.WriteToLogLevel("Just after command Execution in R.NET", LogLevelEnum.Info);
                }

            }
            catch (Exception e)
            {
                if (AdvancedLogging) logService.WriteToLogLevel("Exception occurred while executing in R.NET (Does not always means there is an error)", LogLevelEnum.Info);
                if (AdvancedLogging) logService.WriteToLogLevel("ExtraLogs: Exception Executing : " + commandString, LogLevelEnum.Info);
                if (AdvancedLogging) logService.WriteToLogLevel("ExtraLogs: Exception : " + e.Message, LogLevelEnum.Info);
                if (e != null)
                {
                    string msg = e.Message.Replace("\"", "'");

                    returnValue = "Error:" + msg;
                }
                if (commandString.Contains("readLines("))
                {
                    returnValue = "EOF";
                }
                else if (false)
                {
                    returnValue = "Error: " + "No Err impl, R.net";
                }
            }
            if (AdvancedLogging) logService.WriteToLogLevel("Return Value:" + returnValue, LogLevelEnum.Info);
            return returnValue;
        }

        public string EvaluateNoReturn(string commandString)
        {
            string errmsg = string.Empty;
            try
            {
                this._log.ClearErrorConsole();
                this._RServer.Evaluate(commandString);
                if (this._log.LastError != null && this._log.LastError.Length > 0)
                    errmsg = this.GetErrorText();//get error 
            }
            catch (Exception ex)
            {
                if (this._log.LastError != null && this._log.LastError.Length > 0)
                    errmsg = this.GetErrorText();//get error
                else
                {
                    errmsg = ex.Message;
                }
                LastException = ex;
                logService.WriteToLogLevel("Could not execute: < " + commandString + " >", LogLevelEnum.Error);
            }
            return errmsg;
        }

        public string EvaluateToString(string commandString)
        {
            bool flag = true;

            string resultValueString = string.Empty;

            bool hasReturnVariable = false;

            string subCommand = string.Empty;
            string subCommandReturnVariable = string.Empty;

            try
            {
                subCommand = commandString.Substring(0, commandString.IndexOf("("));
            }
            catch (Exception exception1)
            {
                resultValueString = _log.LastError;
                ProjectData.SetProjectError(exception1);
                subCommand = "";
                ProjectData.ClearProjectError();
            }
            if (commandString.Contains("<-"))
            {
                subCommand = subCommand.Substring(subCommand.IndexOf("-") + 1).Trim();
                hasReturnVariable = true;
                subCommandReturnVariable = commandString.Substring(0, commandString.IndexOf("<")).Trim();
            }
            if (subCommand.Contains("="))
            {
                subCommand = subCommand.Substring(subCommand.IndexOf("=") + 1).Trim();
                hasReturnVariable = true;
                subCommandReturnVariable = commandString.Substring(0, commandString.IndexOf("=")).Trim();
            }

            if (commandString.StartsWith("#"))
            {
                subCommand = "Comment";
            }

            switch (subCommand)
            {
                case "Comment":
                    resultValueString = "";
                    break;

                case "help":
                case "help.search":
                    try
                    {
                        resultValueString = Conversions.ToString(this._RServer.Evaluate(commandString).AsCharacter()[0]);
                    }
                    catch (Exception exception2)
                    {
                        ProjectData.SetProjectError(exception2);
                        Exception exception = exception2;
                        resultValueString = "Error!";
                        flag = true;
                        ProjectData.ClearProjectError();
                        logService.WriteToLogLevel("Could not execute and convert to string: < " + commandString + " >", LogLevelEnum.Error);
                    }
                    break;

                case "library":
                    try
                    {
                        if (false)
                        {
                            resultValueString = "Could not find library!";
                        }
                        else
                        {
                            resultValueString = "Library Loaded";
                        }
                    }
                    catch (Exception exception3)
                    {
                        ProjectData.SetProjectError(exception3);
                        resultValueString = "Could not find library!";
                        ProjectData.ClearProjectError();
                        logService.WriteToLogLevel("Could not execute: < " + commandString + " >", LogLevelEnum.Error);
                    }
                    break;

                case "rm":
                case "plot":
                case "hist":
                case "scatterplot3d":
                case "scatter3d":
                case "abline":
                case "edit":
                case "legend":
                case "par":
                case "barplot":
                    try
                    {
                        this._RServer.Evaluate(commandString);
                        resultValueString = "Done!";
                        if (false)
                        {
                            resultValueString = "Error: " + "not imple for RDotNet";
                        }
                    }
                    catch (Exception exception4)
                    {
                        ProjectData.SetProjectError(exception4);
                        resultValueString = "There was an error!";
                        flag = true;
                        ProjectData.ClearProjectError();
                        logService.WriteToLogLevel("Could not execute:< " + commandString + " >", LogLevelEnum.Error);
                    }
                    break;

                default:
                    if (subCommand == "model")
                    {
                        object obj4 = null;
                        try
                        {
                            if (subCommandReturnVariable == "")
                            {
                                subCommandReturnVariable = "Temporary_Model";
                                this._RServer.Evaluate("Temporary_Model<-" + commandString);
                            }
                            else
                            {
                                this._RServer.Evaluate(commandString);
                            }
                            if (false)
                            {
                                resultValueString = "Error: " + "not imple in RDotNet";// this._RServer.GetErrorText();
                            }
                            else
                            {

                            }
                            break;
                        }
                        catch (Exception exception5)
                        {
                            logService.WriteToLogLevel("Could not execute:< " + commandString + " >", LogLevelEnum.Error);
                            ProjectData.SetProjectError(exception5);
                            resultValueString = "There was an error!";
                            flag = true;
                            ProjectData.ClearProjectError();
                            break;
                        }
                    }
                    if (hasReturnVariable)
                    {
                        try
                        {
                            this._RServer.Evaluate(commandString);
                            resultValueString = "Done!";
                            if (false)
                            {
                                resultValueString = "Error: " + "no err impl in R.Net";
                                flag = true;
                            }
                            break;
                        }
                        catch (Exception exception6)
                        {
                            logService.WriteToLogLevel("Could not execute: < " + commandString + " >", LogLevelEnum.Error);
                            ProjectData.SetProjectError(exception6);
                            resultValueString = "There was an error!";
                            ProjectData.ClearProjectError();
                            break;
                        }
                    }
                    try
                    {
                        this._RServer.Evaluate("tmp<-" + commandString);
                        if (_log.LastError != null && _log.LastError.Trim().Length > 0)
                            resultValueString = GetErrorText();
                        else
                            resultValueString = string.Empty;
                        if (false)
                        {
                            resultValueString = "Error: " + "No error R.net";
                            flag = true;
                        }
                        else if (!Conversions.ToBoolean(this._RServer.Evaluate("is.null(tmp)").AsLogical()[0]))
                        {

                        }
                        else
                        {
                            if (string.IsNullOrEmpty(resultValueString))
                                resultValueString = "No Result";
                        }
                    }
                    catch (Exception exception7)
                    {
                        if (_log.LastError != null && _log.LastError.Trim().Length > 0)
                            resultValueString = GetErrorText();
                        logService.WriteToLogLevel("Could not execute: < " + commandString + " > ", LogLevelEnum.Error, exception7);
                        ProjectData.SetProjectError(exception7);
                        resultValueString = "There was an error!";
                        flag = true;
                        ProjectData.ClearProjectError();
                    }
                    break;
            }
            if (hasReturnVariable)
            {
                try
                {
                    if (Conversions.ToBoolean(this._RServer.Evaluate("is.data.frame(" + subCommandReturnVariable + ")").AsLogical()[0]))
                    {

                    }
                }
                catch (Exception exception8)
                {
                    logService.WriteToLogLevel("Could not execute and convert to bool:< " + "is.data.frame(" + subCommandReturnVariable + ")" + " >", LogLevelEnum.Error);
                    ProjectData.SetProjectError(exception8);
                    flag = true;
                    ProjectData.ClearProjectError();
                }
            }
            if (flag)
            {
                return resultValueString;
            }
            else
            {
                return "Done!";
            }
        }

        private string InterpretReturn(object objRtr, string Command)
        {
            int num;
            object instance = null;
            string str2;
            try
            {
                this._RServer.Evaluate("tmp<-names(" + Command + ")");
                if (!(bool)this._RServer.Evaluate("is.null(tmp)").AsLogical()[0])
                {
                    instance = (this._RServer.Evaluate("tmp").AsList());
                }
            }
            catch (Exception exception1)
            {
                logService.WriteToLogLevel("Could not execute : < " + Command + " >", LogLevelEnum.Error);
                LastException = exception1;
            }

            switch (objRtr.GetType().ToString())
            {
                case "System.String":
                    return objRtr.ToString();

                case "System.Double":
                    return Conversions.ToDouble(objRtr).ToString();

                case "System.Int32":
                    return Conversions.ToInteger(objRtr).ToString();

                case "System.Int32[]":
                    {
                        str2 = "<table border=1 cellspacing=1>";
                        if ((instance != null) && (instance.GetType().ToString() == "System.String[]"))
                        {
                            str2 = str2 + "<tr>";
                            int num7 = ((Array)instance).Length - 1;
                            for (num = 0; num <= num7; num++)
                            {
                                str2 = str2 + "<th>" + NewLateBinding.LateIndexGet(instance, new object[] { num }, null).ToString() + "</th>";
                            }
                            str2 = str2 + "</tr>";
                        }
                        str2 = str2 + "<tr>";
                        int num8 = ((int[])objRtr).Length - 1;
                        for (num = 0; num <= num8; num++)
                        {
                            str2 = str2 + "<td align='right'>" + NewLateBinding.LateIndexGet(objRtr, new object[] { num }, null).ToString() + "</td>";
                        }
                        return (str2 + "</tr></table>");
                    }
                case "System.Int32[,]":
                    {
                        Array array = (Array)objRtr;
                        str2 = "<table border=1 cellspacing=1>";
                        int upperBound = array.GetUpperBound(0);
                        for (num = 0; num <= upperBound; num++)
                        {
                            str2 = str2 + "<tr>";
                            int num10 = array.GetUpperBound(1);
                            for (int i = 0; i <= num10; i++)
                            {
                                str2 = Conversions.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject(str2 + "<td>", NewLateBinding.LateIndexGet(objRtr, new object[] { num, i }, null)), "</td>"));
                            }
                            str2 = str2 + "</tr>";
                        }
                        return (str2 + "</table>");
                    }
                case "System.Double[]":
                    {
                        str2 = "<table border=1 cellspacing=1>";
                        if ((instance != null) && (instance.GetType().ToString() == "System.String[]"))
                        {
                            str2 = str2 + "<tr>";
                            int num11 = ((Array)instance).Length - 1;
                            for (num = 0; num <= num11; num++)
                            {
                                str2 = str2 + "<th>" + NewLateBinding.LateIndexGet(instance, new object[] { num }, null).ToString() + "</th>";
                            }
                            str2 = str2 + "</tr>";
                        }
                        str2 = str2 + "<tr>";
                        int num12 = ((double[])objRtr).Length - 1;
                        for (num = 0; num <= num12; num++)
                        {
                            str2 = str2 + "<td align='right'>" + NewLateBinding.LateIndexGet(objRtr, new object[] { num }, null).ToString() + "</td>";
                        }
                        return (str2 + "</tr></table>");
                    }
                case "System.Double[,]":
                    {
                        Array array2 = (Array)objRtr;
                        str2 = "<table border=1 cellspacing=1>";
                        int num13 = array2.GetUpperBound(0);
                        for (num = 0; num <= num13; num++)
                        {
                            str2 = str2 + "<tr>";
                            int num14 = array2.GetUpperBound(1);
                            for (int j = 0; j <= num14; j++)
                            {
                                str2 = Conversions.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject(str2 + "<td align='right'>", NewLateBinding.LateIndexGet(objRtr, new object[] { num, j }, null)), "</td>"));
                            }
                            str2 = str2 + "</tr>";
                        }
                        return (str2 + "</table>");
                    }
                case "System.String[]":
                    {
                        str2 = "<table border=1 cellspacing=1>";
                        if ((instance != null) && (instance.GetType().ToString() == "System.String[]"))
                        {
                            str2 = str2 + "<tr>";
                            int num15 = ((Array)instance).Length - 1;
                            for (num = 0; num <= num15; num++)
                            {
                                str2 = str2 + "<th>" + NewLateBinding.LateIndexGet(instance, new object[] { num }, null).ToString() + "</th>";
                            }
                            str2 = str2 + "</tr>";
                        }
                        str2 = str2 + "<tr>";
                        int num16 = ((string[])objRtr).Length - 1;
                        for (num = 0; num <= num16; num++)
                        {
                            str2 = str2 + "<td>" + NewLateBinding.LateIndexGet(objRtr, new object[] { num }, null).ToString() + "</td>";
                        }
                        return (str2 + "</tr></table>");
                    }
                case "System.String[,]":
                    {
                        Array array3 = (Array)objRtr;
                        str2 = "<table border=1 cellspacing=1>";
                        int num17 = array3.GetUpperBound(0);
                        for (num = 0; num <= num17; num++)
                        {
                            str2 = str2 + "<tr>";
                            int num18 = array3.GetUpperBound(1);
                            for (int k = 0; k <= num18; k++)
                            {
                                str2 = Conversions.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject(str2 + "<td>", NewLateBinding.LateIndexGet(objRtr, new object[] { num, k }, null)), "</td>"));
                            }
                            str2 = str2 + "</tr>";
                        }
                        return (str2 + "</table>");
                    }
                case "System.Object[]":
                    {
                        object objectValue = (this._RServer.Evaluate("names(" + Command + ")").AsCharacter());
                        str2 = "<table border=1 cellspacing=1>";
                        if (objectValue.GetType().ToString() == "System.String[]")
                        {
                            str2 = str2 + "<tr>";
                            int num19 = ((Array)objectValue).Length - 1;
                            for (num = 0; num <= num19; num++)
                            {
                                str2 = str2 + "<th>" + NewLateBinding.LateIndexGet(objectValue, new object[] { num }, null).ToString() + "</th>";
                            }
                            str2 = str2 + "</tr>";
                        }
                        str2 = str2 + "<tr>";
                        int num20 = ((object[])objRtr).Length - 1;
                        for (num = 0; num <= num20; num++)
                        {
                            str2 = str2 + "<td>" + NewLateBinding.LateIndexGet(objRtr, new object[] { num }, null).ToString() + "</td>";
                        }
                        return (str2 + "</tr></table>");
                    }
            }
            return objRtr.ToString();
        }

        #endregion

        #region R Environment Manipulation
        public bool IsLoaded(string Package)
        {
            bool isLoaded = false;
            try
            {
                object objectValue = (this.RawEvaluate("search()"));
                if (objectValue != null)
                {
                    IEnumerator enumerator = null;
                    try
                    {
                        enumerator = ((IEnumerable)objectValue).GetEnumerator();
                        while (enumerator.MoveNext())
                        {
                            string str = enumerator.Current.ToString();
                            if (str.StartsWith("package:") && (str.Substring(str.IndexOf(":") + 1) == Package))
                            {
                                isLoaded = true;
                                break;
                            }
                        }
                    }
                    finally
                    {
                        if (enumerator is IDisposable)
                        {
                            (enumerator as IDisposable).Dispose();
                        }
                    }
                }

            }
            catch
            {
                isLoaded = false;
                logService.WriteToLogLevel("Could not execute : search/package commands ", LogLevelEnum.Error);
            }
            return isLoaded;
        }

        public object RawEvaluate(string command)
        {
            object obj;
            try
            {
                obj = this._RServer.Evaluate(command).AsCharacter().ToArray();//.AsList();
            }
            catch (Exception ex)
            {
                logService.WriteToLogLevel("Could not execute :< " + command + " >", LogLevelEnum.Error);
                LastException = ex;
                obj = null;
            }
            return obj;
        }


        public string RawEvaluateGetstring(string command)
        {
            object obj = null;
            try
            {
                SymbolicExpression se = this._RServer.Evaluate(command);

                if (se != null && se.AsCharacter() != null)
                {
                    obj = se.AsCharacter()[0];//.AsList();
                }
            }
            catch (Exception ex)
            {
                logService.WriteToLogLevel("Could not execute :< " + command + " >", LogLevelEnum.Error);
                LastException = ex;
                obj = null;
            }
            if (obj != null)
                return obj.ToString();
            else
                return null;
        }

        public bool SetVariable(string VariableName, string command)
        {
            bool flag = false;
            try
            {
                flag = true;
            }
            catch
            {
                logService.WriteToLogLevel("Could not execute :< " + command + " >", LogLevelEnum.Error);
            }
            return flag;
        }
        #endregion

        #region Error Handling
        public string GetErrorText()
        {
            string errorText;
            try
            {
                errorText = this._log.LastError;
                _log.ClearErrorConsole();
            }
            catch
            {
                logService.WriteToLogLevel("Could not find error text : ", LogLevelEnum.Error);
                errorText = string.Empty;
            }
            return errorText;
        }
        #endregion

        #region IDisposable Members

        public void Dispose()
        {

            logService.WriteToLogLevel("Disposing RService...", LogLevelEnum.Fatal);
            this._RServer.Dispose();
        }

        #endregion

        #region Set User Personal Library for R packages

        public void TrySettingUserPersonalLibrary()
        {
            //Current .libPaths before trying to set to user-libs
            logService.WriteToLogLevel("Current .libPaths(): ", LogLevelEnum.Info);
            LogCurrentLibPaths();

            bool userlibexists = false;
            bool writablepathfound = false;

            //Get Personal lib path from Env var
            //string usrpath = RawEvaluateGetstring("Sys.getenv('R_LIBS_USER')");
            //logService.WriteToLogLevel("Sys.getenv('R_LIBS_USER') = (" + usrpath + ").", LogLevelEnum.Info);

            string userRLib = string.Empty;
            //Get R User lib from config
            string userRlibConfig = confService.GetConfigValueForKey("userRlib");
            logService.WriteToLogLevel("User R Lib from configuration = (" + userRlibConfig + ").", LogLevelEnum.Info);
            if (userRlibConfig.Trim().Length > 0)
            {
                if (isWritableDirectory(userRlibConfig))
                {
                    userRLib = userRlibConfig;
                }
            }
            else if(userRLib.Length==0) // generate default r user lib path
            {
                //Fully formed path D:\\WinDirs\\Documents/R/win-library/3.4
                string userlib = GenerateUserRLibDefaultPath();
                logService.WriteToLogLevel("Generated R standard user lib:" + userlib, LogLevelEnum.Info);

                if (!Directory.Exists(userlib))
                {
                    string msg3 = "Path does not exists: "+userlib;
                    logService.WriteToLogLevel(msg3, LogLevelEnum.Warn);

                    //create path is possible
                    if (createLibPathDirectory(userlib))//if dirctory created successfully
                    {
                        logService.WriteToLogLevel("Setting R user personal library : " + userlib, LogLevelEnum.Info);
                    }
                    else
                    {
                        logService.WriteToLogLevel("R lib path initialization failed.", LogLevelEnum.Error);
                        logService.WriteToLogLevel("User personal library is not writable.", LogLevelEnum.Info);
                        //if path can't be created then return from this function
                        return;
                    }

                }

                bool hasModifyAccess = isWritableDirectory(userlib);
                logService.WriteToLogLevel("Is user personal library writable : " + hasModifyAccess.ToString(), LogLevelEnum.Info);
                if (hasModifyAccess)
                {
                    userRLib = userlib;
                }
                else
                {
                    logService.WriteToLogLevel("User R lib path initialization failed : ", LogLevelEnum.Error);
                    logService.WriteToLogLevel(userlib + " not writable.", LogLevelEnum.Info);
                }
            }

            if (userRLib.Length > 0)
            {
                string command = ".libPaths( c(.Library,'" + userRLib + "', .libPaths()))";
                this._RServer.Evaluate(command);
                logService.WriteToLogLevel("User personal library set in the second position.", LogLevelEnum.Info);
                confService.ModifyConfig("userRlib", userRLib.Replace('\\', '/'));
            }
            else//no path set because of permission issue
            {
                string msg3 = "No path set for user's R library. Set a valid path from configuration settings";
                logService.WriteToLogLevel(msg3, LogLevelEnum.Warn);
            }

            //.libPaths after trying to set to user-libs
            logService.WriteToLogLevel("Final .libPaths(): ", LogLevelEnum.Info);
            LogCurrentLibPaths();
        }

        //to generate default path for user personal R library
        public string GenerateUserRLibDefaultPath()
        {
            //Get Documents location
            string docpath = RawEvaluateGetstring("Sys.getenv('R_USER')");//"D:\\WinDirs\\Documents"
            logService.WriteToLogLevel("Sys.getenv('R_USER') = (" + docpath + ").", LogLevelEnum.Info);

            //Get R ver e.g. 3.4 or 3.6
            string Rver = RawEvaluateGetstring("paste(as.character(getRversion()$major),as.character(getRversion()$minor),sep='.')");
            logService.WriteToLogLevel("R version for doc path:" + Rver, LogLevelEnum.Info);

            //Fully formed path D:\\WinDirs\\Documents/R/win-library/3.4
            string userlib = docpath + "/R/win-library/" + Rver;
            logService.WriteToLogLevel("R standard user lib:" + userlib, LogLevelEnum.Info);
            return userlib;
        }

        // in .libPaths() we want to set library path of shipped/embedded R as the first path so that
        // all the required R packages gets loaded from this location(including BlueSky R pkg).
        // Normally, 'User R lib' in Documents is the first path and user may have installed same R packages that
        // we ship with embedded R. We dont want to load R packages from 'Documents', they may not be compatible with
        // our versions of required R pkgs. We always want to load R packages from our shipped R location only.
        public void TrySettingShippedRLibraryInFirstLocation()
        {
            //Current .libPaths before trying to set to user-libs
            logService.WriteToLogLevel("Current .libPaths(): ", LogLevelEnum.Info);
            LogCurrentLibPaths();

            logService.WriteToLogLevel("Setting default R library as a first item: ", LogLevelEnum.Info);
            string command = ".libPaths( c(.Library, .libPaths()))";
            this._RServer.Evaluate(command);
            logService.WriteToLogLevel("Default R library set as a first item.", LogLevelEnum.Info);

            //.libPaths after trying to set to default R lib(from where R was launched)
            logService.WriteToLogLevel("Final .libPaths(): ", LogLevelEnum.Info);
            LogCurrentLibPaths();
        }

        private void LogCurrentLibPaths()
        {
            //Get all the .libPaths
            object libpaths = RawEvaluate(".libPaths()");
            string[] allLibPaths = null;
            if (libpaths != null)
            {
                Type retType = libpaths.GetType();
                if (retType.Name == "String[]")//for multicols
                {
                    allLibPaths = (String[])libpaths;
                }
                else if (retType.Name == "String")//for single col
                {
                    allLibPaths = new string[1];
                    allLibPaths[0] = (String)libpaths;
                }
            }

            //check which ones are writable
            string wrtabllib = string.Empty;
            foreach (string ulib in allLibPaths)
            {
                if (isWritableDirectory(ulib))
                {
                    wrtabllib = ulib;
                    logService.WriteToLogLevel(ulib + " is writable :", LogLevelEnum.Info);
                }
                else
                    logService.WriteToLogLevel(ulib + " is not writable :", LogLevelEnum.Info);
            }

        }

        private static bool createLibPathDirectory(string dirpath)
        {
            bool iscreated = true;
            try
            {
                if (!Directory.Exists(dirpath)) //Create Directory if does not exist
                {
                    Directory.CreateDirectory(dirpath);
                }
            }
            catch (Exception ex)
            {
                iscreated = false;
            }
            return iscreated;
        }

        private bool isWritableDirectory(string pstrPath)
        {
            UtilityService util = new UtilityService();
            return util.isWritableDirectory(pstrPath);
        }


        #endregion

    }
}


