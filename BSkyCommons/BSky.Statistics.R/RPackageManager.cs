using System.Text;
using BSky.Statistics.Common;
using BSky.Lifetime.Interfaces;
using BSky.Lifetime;
using System.IO;
using System.Collections.Generic;
using ICSharpCode.SharpZipLib.Zip;
using System;

namespace BSky.Statistics.R
{
    public class RPackageManager
    {
        Journal _journal;
        ILoggerService logService = LifetimeService.Instance.Container.Resolve<ILoggerService>();//17Dec2012
        RecentItems userPackageList = LifetimeService.Instance.Container.Resolve<RecentItems>();//06Feb2014
        RService _dispatcher;



        public RPackageManager(RService dispatcher, Journal journal)
        {
            _journal = journal;
            _dispatcher = dispatcher;

            //can be executed once here and then no need to use it methods below.
            string errmsg = _dispatcher.EvaluateNoReturn("library(tools)");
            if (errmsg != null && errmsg.Trim().Length < 1) //error occurred
            {

            }
        }

        #region Query Packages
        //Currently Installed Packages.  pk <- installed.packages()
        public string[] GetInstalledPackages()
        {
            string commandstring = "tmp <- installed.packages(noCache = TRUE)";
            _journal.WriteLine(commandstring);
            _dispatcher.EvaluateToObject(commandstring, false);

            object o = _dispatcher.EvaluateToObject("rownames(tmp)", true); // returns row-header which are packagenames.
            string[] pkgs = this.ObjectToStringArray(o);
            return pkgs;
        }

        //returns the version of a insalled(no need to load R package). [Not the zip package file]
        public string GetInstalledPacakgeVersion(string RPackageName)
        {
            string commandstring = "packageDescription('" + RPackageName + "', fields = c('Version'))";
            _journal.WriteLine(commandstring);
            object o = _dispatcher.EvaluateToObject(commandstring, true);
            string pkgver = o.ToString();
            return pkgver;
        }

        //for comparing two version numbers. return 1 if Aversion is latest else return -1. 
        public int CompareVersion(string Aversion, string Bversion)
        {
            string commandstring = "compareVersion('" + Aversion + "', '" + Bversion + "')";
            _journal.WriteLine(commandstring);
            object o = _dispatcher.EvaluateToObject(commandstring, true);
            string latestversion = o.ToString();
            if (latestversion != null && latestversion.Trim().Length > 0 && latestversion.Trim().Equals("1"))
                return 1;
            else
                return -1;
        }
        //is package already installed on drive or not
        public bool isPackageInstalled(string packagename)//not the zip name but package name like "BlueSky"
        {
            bool isInstalled = false;
            string[] currentlyInstalled = GetInstalledPackages();//get currently installed R package list
            foreach (string s in currentlyInstalled)
            {
                if (packagename.Equals(s))
                {
                    isInstalled = true;
                    break;
                }
            }
            return isInstalled;
        }
        //Currently Loaded Packages. allst <- search()
        public string[] GetCurrentlyLoadedPackages()
        {
            string commandstring = "tmp <- search()";
            _journal.WriteLine(commandstring);
            _dispatcher.EvaluateToObject(commandstring, false);
            // tmp contains packages and .GlbalEnv etc..
            object o = _dispatcher.EvaluateToObject("tmp", true);
            string[] allstuff = this.ObjectToStringArray(o);
            int count = 0;
            foreach (string s in allstuff) // counting packages in all the strings returned by search()
            {
                if (s.StartsWith("package:"))
                    count++;
            }

            string[] pkgs = new string[count];
            int i = 0;
            foreach (string s in allstuff)
            {
                if (s.StartsWith("package:"))
                {
                    pkgs[i] = s.Replace("package:", string.Empty).Trim();
                    i++;
                }
            }
            return pkgs;
        }

        //is package already loaded in memory
        public bool isPackageLoaded(string packagename)//not the zip name but package name like "BlueSky"
        {
            bool isLoaded = false;
            string[] currentlyInstalled = GetCurrentlyLoadedPackages();//get currently loaded R package list
            foreach (string s in currentlyInstalled)
            {
                if (packagename.Equals(s))
                {
                    isLoaded = true;
                    break;
                }
            }
            return isLoaded;
        }

public string[] GetDatasetListFromRPkg(string packagename)//12Feb2011 Get names of datasets in a R pkg
        {
            string joinCharacter = "-";
            StringBuilder sb = new StringBuilder(); 
            string[] datasetlist = null;
            // data()$results is a matrix with dimnames [Package, LibPath, Item, Title]
            // we will need "Item" and "Title" ( data(package="pkgname")$results[, 3:4] ) which we will join using joinCharacter
            ////try may help in avoiding crash
            string commandstring = "try(data(package='"+ packagename + "')$results[, 3:4])"; 
            _journal.WriteLine(commandstring);
            RDotNet.CharacterMatrix chrmatrix = _dispatcher.GetChrMatrix(commandstring);
            if (chrmatrix != null)
            {
                if (chrmatrix.ColumnNames != null)
                {
                    datasetlist = new string[chrmatrix.RowCount];
                    //datasetlist[0] = string.Empty;
                    for (int r = 0; r < chrmatrix.RowCount; r++)
                    {
                        sb.Clear();
                        for (int c = 0; c < chrmatrix.ColumnCount; c++)
                        {
                            sb.Append(chrmatrix[r, c]);
                            if (c < chrmatrix.ColumnCount - 1)
                            {
                                sb.Append(" ");
                                sb.Append(joinCharacter);
                                sb.Append("[");
                            }
                        }
                        sb.Append("]");
                        datasetlist[r] = sb.ToString();
                    }
                }
                else
                {
                    if (chrmatrix.RowCount == 1 && chrmatrix.ColumnCount == 1)
                    {
                        string rmsg = chrmatrix[0, 0];
                        _journal.WriteLine(rmsg);
                        datasetlist = new string[1];
                        datasetlist[0] = "ReRRoE"+rmsg; //to easily identify error message in uber function
                    }
                }
            }

            return datasetlist;
        }

        #endregion

        #region Methods for Single Package operations

        //Install Package from CRAN.
        public UAReturn InstallPackageFromCRAN(string packagename)
        {
            UAReturn result = new UAReturn() { Success = false };
            string command = string.Empty;
            string retmsg = string.Empty;
            string[] pkgs = packagename.Split(',');
            string packagenames = GetCommaSeparatedWithSingleQuotes(pkgs);
            if (packagenames == null)//bad string
            {
                result.CommandString = packagename;
                retmsg = " "+BSky.GlobalResources.Properties.Resources.MultiRPkgInstallMsg;
            }
            else
            {
                foreach (string pkgnam in pkgs)//uninstall package(s)
                {
                    //UAReturn result = new UAReturn() { Success = false };
                    if (_dispatcher.IsLoaded(pkgnam)) // if package is already loaded. Unload and uninstall it
                    {
                        UnLoadPackage(pkgnam);
                        UninstallPackage(pkgnam);
                    }
                }
                string CranUrl = "http://cran.us.r-project.org";
                  command = string.Format("install.packages(c({0}))", packagenames);
                retmsg = _dispatcher.EvaluateToString(command);
            }

            _journal.WriteLine(command);
            result.CommandString = command;
            result.Error = retmsg;
            if (retmsg.Contains("Check Command.") || retmsg.ToLower().Contains("error") || retmsg.ToLower().Contains("warning"))
                result.Success = false;
            else
                result.Success = true;
            return result;
        }

        //27Aug2015 //Install Required package from CRAN, using a function from BlueSky R package
        public UAReturn InstallReqPackageFromCRAN(string packagename)
        {
            UAReturn result = new UAReturn() { Success = false };
            string command = string.Empty;
            string retmsg = string.Empty;
            string[] pkgs = packagename.Split(',');
            string packagenames = GetCommaSeparatedWithSingleQuotes(pkgs);
            if (packagenames == null)//bad string
            {
                result.CommandString = packagename;
                retmsg = " " + BSky.GlobalResources.Properties.Resources.MultiRPkgInstallMsg;
            }
            else
            {
                StringBuilder sbmsgs = new StringBuilder();
                //
                foreach (string pkgnam in pkgs)//uninstall package(s)
                {
                    if (!_dispatcher.IsLoaded(pkgnam) && !isPackageInstalled(pkgnam)) // if package is not already loaded and installed. then install it.
                    {
                        command = string.Format("BSkyInstallPkg('{0}')", pkgnam); //string.Format("install.packages(c({0}))", packagenames);
                        string retstr = _dispatcher.EvaluateToString(command);
                        if (retstr != null && retstr.Trim().Length > 0)
                            sbmsgs.Append(retstr);
                        else
                            sbmsgs.Append(pkgnam + " installed successfully.");
                    }
                    else //if you wish, you can unload andi nstall and then reinstall the R pacakge from CRAN
                    {
                        sbmsgs.Append("Warning: "+pkgnam + " is already present.");
                    }
                }
                retmsg = sbmsgs.ToString();// _dispatcher.EvaluateToString(command);
            }

            _journal.WriteLine(command);
            result.CommandString = command;
            result.Error = retmsg;
            if (retmsg.Contains("Check Command.") || retmsg.ToLower().Contains("error") || retmsg.ToLower().Contains("warning"))
                result.Success = false;
            else
                result.Success = true;

            return result;
        }

        public UAReturn setCRANMirror()
        {
            UAReturn result = new UAReturn() { Success = false };
            string command = "chooseCRANmirror()";
            string retmsg = _dispatcher.EvaluateToString(command);
            _journal.WriteLine(command);
            result.CommandString = command;
            if (retmsg.Contains("Check Command."))
                result.Success = false;
            else
                result.Success = true;
            return result;
        }

        //Install Package from ZIP.
        public UAReturn InstallPackageFromZip(string fullpathpackagefilename, bool autoLoad = true, bool overwrite=false)//autoload loads package in memory after install
        {
            UAReturn result = new UAReturn() { Success = false };
            string pkgversion = string.Empty;
            string packagename = GetPackageNameFromZip(fullpathpackagefilename, out pkgversion);// get package name.
            if (packagename.Trim().Length < 1) // no packagename, may be because of error.
            {
                result.Error = "Error:No Packages Found!";
                return result;
            }



            //See if package is already installed, then dont install it. OR Install if Overwrite is true
            if (!isPackageInstalled(packagename) || overwrite)
            {
                //See if you really want to execute following.
                if (overwrite)
                {
                    if (_dispatcher.IsLoaded(packagename)) // if package is already loaded. Unload 
                    {
                        UnLoadPackage(packagename);
                    }
                    if (isPackageInstalled(packagename)) // and uninstall it
                    {
                        UninstallPackage(packagename);
                    }
                }

                //string AppRPackageDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location).Replace('\\', '/') + "/RPackages";
                string parentDir = Path.GetDirectoryName(fullpathpackagefilename).Replace('\\', '/');
                string unixstylefullpathzipfilename = fullpathpackagefilename.Replace('\\', '/');
                string command = string.Empty;
                string errmsg = null;
                StringBuilder sb = new StringBuilder("");

                //23Oct2015. We found there is a shorter way of following command
                command = string.Format("install.packages('{0}')", unixstylefullpathzipfilename);
                errmsg = _dispatcher.EvaluateNoReturn(command);
                _journal.WriteLine(command);
                if (errmsg != null && errmsg.Trim().Length > 1) //error occurred
                {
                    sb.Append(errmsg);
                }
                if (!(errmsg.ToLower().Contains("error") || errmsg.ToLower().Contains("warning")))
                {
                    result.Success = true;
                    pkgnamelist.Add(packagename);//"BlueSky" or "MASS" or "car" not zip filenames
                }

                if (autoLoad)
                {
                    sb.Append("Loading package: ");
                    UAReturn rr = LoadPackage(packagename, false); 
                    sb.Append(rr.Error); //package loading error or warning.
                }
                result.Error = sb.ToString();
            }
            else
            {
                result.Error = packagename + " already installed.";
                pkgnamelist.Add(packagename);
            }
            return result;
        }

        //Load Packages from installed ones.
        public UAReturn LoadPackage(string packagename, bool AddToUserPackageList, bool confirmPackageInstalled=true)
        {
            string command = string.Format("library({0})", packagename);

            UAReturn result = new UAReturn() { Success = false };

            if (confirmPackageInstalled && !isPackageInstalled(packagename))
            {
                result.Error = "Error loading R package: " + packagename + " (package not installed)";
            }
            else
            {
                //Load Package
                if (true) // (!_dispatcher.IsLoaded(packagename))
                {
                    string errm = _dispatcher.EvaluateNoReturn("options('warn'=1)"); 
                    string errmsg = _dispatcher.EvaluateNoReturn(command);//  "Load Package";
                    if (errmsg != null && !(errmsg.ToLower().Contains("error") || errmsg.ToLower().Contains("warning")))
                        result.Success = true;
                    if (AddToUserPackageList)//Also add this package to user package list
                        AddUserSessionPackage(packagename); //creates an entry in user session package list.
                    if (errmsg != null && errmsg.Trim().Length > 1) //error occurred
                    {
                        result.Error = errmsg;
                    }
                    result.CommandString = command;
                }
                else
                {
                    result.Success = true;
                    result.CommandString = command + " : Already loaded";

                }
            }
            return result;
        }


        //Unload Package. detach(package:uadatapackage)
        public UAReturn UnLoadPackage(string packagename)
        {
            string command = string.Format("detach(package:{0})", packagename);

            UAReturn result = new UAReturn() { Success = false };
            //UnLoad Package
            if (_dispatcher.IsLoaded(packagename))
            {
                string errmsg = _dispatcher.EvaluateNoReturn(command);// "Unload Package";
                _journal.WriteLine(command);
                if (errmsg != null && errmsg.Trim().Length > 1) //error occurred
                {
                    result.Error = errmsg;
                }
                result.Success = true;
                result.CommandString = command;
            }
            return result;
        }


        //Uninstall package. detach first and then remove.packages("uadatapackage")
        public UAReturn UninstallPackage(string packagename)
        {
            //first find the lib where package is installed
            string lib = _dispatcher.EvaluateToObject("find.package('"+packagename.Trim()+"')", true).ToString();//this must be path with forward slash(Unix style)
            lib = lib.Substring(0, lib.IndexOf(packagename) - 1);
            //Now the pacakge will get removed from right location
            string command = "remove.packages('" + packagename + "', lib='"+lib+"')";

            UAReturn result = new UAReturn() { Success = false };
            UnLoadPackage(packagename); // if its loaded, unload it first.
            // now remove package
            string errmsg = _dispatcher.EvaluateNoReturn(command);//  "Uninstall Package";
            _journal.WriteLine(command);
            if (errmsg != null && errmsg.Trim().Length > 1) //error occurred
            {
                result.Error = errmsg;
            }


            result.Success = true;
            result.CommandString = command;
            return result;
        }

        #endregion

        #region Methods for Multi-Package operation

        List<string> pkgnamelist = new List<string>();
        //Install Mulitple Packages from ZIP.
        public UAReturn InstallMultiPackageFromZip(string[] fullpathpackagefilenames, bool autoLoad = true, bool overwrite = false)
        {
            pkgnamelist.Clear();
            UAReturn result = new UAReturn() { Success = false };

            StringBuilder sb = new StringBuilder("");
            bool allInstalled = true;
            foreach (string pkgfname in fullpathpackagefilenames)
            {
                UAReturn r = InstallPackageFromZip(pkgfname, false, overwrite);//false means do not load them immediately
                if (r.Success)
                {
                    if (sb.Length > 0)
                        sb.Append("\n");

                    sb.Append(pkgfname + " Installed.");

                }
                else
                {
                    if (sb.Length > 0)
                        sb.Append("\n");
                    sb.Append(pkgfname + " Couldn't Install.\n" + r.Error);
                    allInstalled = false;
                }
            }
            result.Success = allInstalled;
            //By Now We have all the packages installed. Now we load them all.
            string[] pkgs = new string[pkgnamelist.Count];
            int p = 0;
            foreach (string s in pkgnamelist)
                pkgs[p++] = s;
            UAReturn res = LoadMultiplePackages(pkgs, false);
            sb.Append("\nLoading Package(s):\n");
            sb.Append(res.CommandString);//Add result from 'Load'

            result.SimpleTypeData = sb.ToString();
            return result;
        }

        //Load Multiple Packages
        public UAReturn LoadMultiplePackages(string[] packagenames, bool AddToUserPackageList)
        {
            UAReturn res = new UAReturn() { Success = false };
            UAReturn tmp = null; //11May2014 for temporary return value
            StringBuilder sb = new StringBuilder("");
            StringBuilder comm = new StringBuilder(""); //11May2014 for storing multiple commands in single place

            foreach (string pkgname in packagenames)
            {
                tmp = LoadPackage(pkgname.Trim(), AddToUserPackageList, false);//false: no verification

                if (tmp != null)
                {
                    if (tmp.CommandString != null) //11May2014
                    {
                        if (comm.Length > 1) comm.Append("\n"); // if there is already something then only add new line
                        comm.Append(tmp.CommandString);
                    }
                    if (tmp.Error != null && tmp.Error.Length > 1) // if there is some error msg
                    {
                        sb.Append(tmp.Error);
                        if (comm.Length > 1) comm.Append("\n"); // if there is already something then only add new line

                        comm.Append(tmp.Error);
                    }
                }
            }
            res.Success = sb.Length > 0 ? false : true;
            res.Error = sb.ToString();
            res.CommandString = comm.ToString();//11May2014
            return res;
        }

        //Unload mulitple packages
        public UAReturn UnLoadMultiPackage(string[] packagenames)
        {
            UAReturn res = new UAReturn() { Success = false };
            UAReturn tmp = null; //11May2014 for temporary return value
            StringBuilder sb = new StringBuilder("");
            StringBuilder comm = new StringBuilder(""); //11May2014 for storing multiple commands in single place
            foreach (string pkgname in packagenames)
            {
                tmp = UnLoadPackage(pkgname.Trim());
                if (tmp != null)
                {
                    if (tmp.CommandString != null) //11May2014
                    {
                        if (comm.Length > 1) comm.Append("\n"); // if there is already something then only add new line
                        comm.Append(tmp.CommandString);
                    }
                    if (tmp.Error != null && tmp.Error.Length > 1) // if there is some error msg
                    {
                        sb.Append(tmp.Error);
                    }
                }
            }
            res.Success = sb.Length > 0 ? false : true;
            res.Error = sb.ToString();
            res.CommandString = comm.ToString();//11May2014
            return res;
        }

        //Uninstall multiple packages.
        public UAReturn UninstallMultiPakckage(string[] packagenames)
        {
            UAReturn res = new UAReturn() { Success = false };
            UAReturn tmp = null;//11May2014 for temporary return value
            StringBuilder sb = new StringBuilder("");
            StringBuilder comm = new StringBuilder(""); //11May2014 for storing multiple commands in single place
            foreach (string pkgname in packagenames)
            {
                tmp = UninstallPackage(pkgname.Trim());
                if (tmp != null)
                {
                    if (tmp.CommandString != null) //11May2014
                    {
                        if (comm.Length > 1) comm.Append("\n"); // if there is already something then only add new line
                        comm.Append(tmp.CommandString);
                    }
                    if (tmp.Error != null && tmp.Error.Length > 1) // if there is some error msg
                    {
                        sb.Append(tmp.Error);
                    }
                }
            }
            res.Success = sb.Length > 0 ? false : true;
            res.Error = sb.ToString();
            res.CommandString = comm.ToString();//11May2014
            return res;
        }
        #endregion

        #region Helpers Methods

        public string[] ObjectToStringArray(object obj)
        {
            string[] strarr = null;

            if (obj.GetType().Name.Equals("String"))
            {
                strarr = new string[1];
                strarr[0] = obj as string;
            }
            else if (obj.GetType().Name.Equals("String[]"))
            {
                strarr = obj as string[];

            }
            return strarr;
        }

        public string GetPackageNameFromZip(string fullpathpackagefilename, out string packageversion)
        {
            if (string.IsNullOrEmpty(fullpathpackagefilename))//no package name is provided 
            {
                packageversion = "0.0-0";
                return "nopackagename";
            }
            string packagename = string.Empty;
            packageversion = string.Empty;//to get the version
            FileStream fileStreamIn = new FileStream(fullpathpackagefilename, FileMode.Open, FileAccess.Read);//to fix the exception 'if' block is added above
            ZipInputStream zipInStream = new ZipInputStream(fileStreamIn);
            ZipEntry entry = zipInStream.GetNextEntry();
            string tempDir = Path.GetTempPath().Replace("\\", "/");
            StringBuilder path = new StringBuilder(tempDir);
            path.Append("DESCRIPTION");
            FileStream fileStreamOut = null;
            bool isFileExtracted = false;
            //Extract the file
            while (entry != null)
            {
                if (entry.Name.EndsWith("DESCRIPTION"))
                {
                    fileStreamOut = new FileStream(path.ToString(), FileMode.Create, FileAccess.Write);
                    int size;
                    byte[] buffer = new byte[1024];
                    do
                    {
                        size = zipInStream.Read(buffer, 0, buffer.Length);
                        fileStreamOut.Write(buffer, 0, size);
                    } while (size > 0);
                    fileStreamOut.Close();
                    isFileExtracted = true;
                    break;
                }
                entry = zipInStream.GetNextEntry();
            }
            zipInStream.Close();
            fileStreamIn.Close();

            //Read Line by line
            if (isFileExtracted)
            {
                string line;
                System.IO.StreamReader file = new System.IO.StreamReader(path.ToString());
                while ((line = file.ReadLine()) != null)
                {
                    if (line.Contains("Package:")) 
                    {
                        packagename = line.Replace("Package:", string.Empty).Trim();
                        //break;
                    }
                    if (line.Contains("Version:")) 
                    {
                        packageversion = line.Replace("Version:", string.Empty).Trim();
                        break;
                    }
                }
                file.Close();
            }
            return packagename;
        }

        public string GetCommaSeparatedWithSingleQuotes(string[] pkgs)
        {
            StringBuilder pkgnames = new StringBuilder();
            string finalpkgnames;
            for(int i=0; i<pkgs.Length; i++)
            {
                // package name has got single/double quotes. It must be single string without quotes
                if (pkgs[i].Trim().Contains("'") || pkgs[i].Trim().Contains("\"")) 
                {
                    return null;
                }
                if (i == pkgs.Length - 1) // if last item. Dont put comma at the end
                {
                    pkgnames.Append("'" + pkgs[i].Trim() + "'");
                }
                else //put comma because there are more items in the list
                {
                    pkgnames.Append("'" + pkgs[i].Trim() + "', ");
                }

            }
            finalpkgnames = pkgnames.ToString();
            return finalpkgnames;
        }

        #endregion

        #region User Session Packages
        public void AddUserSessionPackage(string packagename)
        {
            if (IsUserPackage(packagename))
                userPackageList.AddXMLItem(packagename);
        }


        public bool IsUserPackage(string packagename)
        {
            bool isUserRPackage;// = true;
            //logic herer
            switch (packagename)
            {
                case "foreign":
                case "car":
                case "RODBC":
                case "uadatapackage":
                    isUserRPackage = false;
                    break;
                default:
                    isUserRPackage = true;
                    break;
            }
            return isUserRPackage;
        }
        #endregion
    }
}
