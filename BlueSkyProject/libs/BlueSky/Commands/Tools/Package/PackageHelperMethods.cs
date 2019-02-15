using BSky.Statistics.Common;
using Microsoft.Practices.Unity;
using BSky.Lifetime;
using BlueSky.Commands.Tools.Package.Dialogs;
using System.IO;
using BSky.Interfaces.Interfaces;
using System.Windows.Input;
using System.Text;

namespace BlueSky.Commands.Tools.Package
{
    public class PackageHelperMethods
    {

        IUnityContainer container;
        IDataService service;
        public PackageHelperMethods()
        {
            container = LifetimeService.Instance.Container;
            service = container.Resolve<IDataService>();
        }

        // Show Installed Packages
        public UAReturn ShowInstalledPackages()//06Dec2013 For loading package in R memory for use
        {
            return service.showInstalledPackages();
        }

        //Show Laoded Packages
        public UAReturn ShowLoadedPackages()//06Dec2013 For loading package in R memory for use
        {
            IUnityContainer container = LifetimeService.Instance.Container;
            IDataService service = container.Resolve<IDataService>();
            return service.showLoadedPackages();
        }

        //Install from CRAN
        public UAReturn InstallCRANPackage()//06Dec2013 For loading package in R memory for use
        {
            //set CRAN
            UAReturn setcran = service.setCRANMirror();
            if (setcran != null)
            { 
            }
            
            //enter case sensitive package name to install from CRAN
            AskPackageNameWindow apn = new AskPackageNameWindow();
            apn.Title = BSky.GlobalResources.Properties.Resources.InstallPkgsFromCRAN;
            apn.ShowDialog();

            ShowMouseBusy();

            string pkgname = apn.PackageName;
            if (pkgname==null || pkgname.Length < 1)
            {
                ShowMouseFree();
                return null;
            }
            UAReturn instpkg  = service.installCRANPackage(pkgname);

            //Now try to load the pacakge, just installed by CRAN
            string retmsg = string.Empty;
            string[] pkgs = pkgname.Split(',');
            UAReturn loadpkg;
            string loadComm = string.Empty;
            if (instpkg != null)
            {
                if (instpkg.Success)//CRAN install was success
                {
                    loadpkg = service.loadPackageFromList(pkgs);
                    retmsg = loadpkg.Error;
                    loadComm = loadpkg.CommandString;
                }
                else if (!instpkg.Success)//CRAN install prbably failed
                {
                    retmsg = "";
                    loadComm ="";
                }
            }
            else//CRAN install prbably failed
            {
                retmsg = "";
                loadComm = "";
            }

            instpkg.Error = instpkg.Error + "\n" + retmsg;
            instpkg.CommandString = instpkg.CommandString + "\n" + loadComm;
            ShowMouseFree();
            return instpkg;
        }

        //27Aug2015 //Install REquired R pacakge from CRAN (using RequiredPacakge.xml)
        public UAReturn InstallReqPackageFrmCRAN(string pkgname)
        {
            //set CRAN
            //UAReturn setcran = service.setCRANMirror();
            //if (setcran != null)
            //{
            //}

            ShowMouseBusy();

            //string pkgname = null;
            if (pkgname == null || pkgname.Length < 1)
            {
                ShowMouseFree();
                return null;
            }
            UAReturn instpkg = service.installReqPackageCRAN(pkgname);

            //Now try to load the pacakge, just installed by CRAN
            string retmsg = string.Empty;
            string[] pkgs = pkgname.Split(',');
            UAReturn loadpkg;
            string loadComm = string.Empty;
            if (instpkg != null)
            {
                if (instpkg.Success)//CRAN install was success
                {
                    loadpkg = service.loadPackageFromList(pkgs);
                    retmsg = loadpkg.Error;
                    loadComm = loadpkg.CommandString;
                }
                else if (!instpkg.Success)//CRAN install prbably failed. May be package already present or some other issue.
                {
                    retmsg = "";
                    loadComm = "";
                }
            }
            else//CRAN install prbably failed
            {
                retmsg = "";
                loadComm = "";
            }

            instpkg.Error = instpkg.Error + "\n" + retmsg;
            instpkg.CommandString = instpkg.CommandString + "\n" + loadComm;
            ShowMouseFree();
            return instpkg;
        }


        //Install from local
        public UAReturn PackageFileInstall(string[] pkgfilenames, bool autoLoad = true, bool overwrite = false)//06Dec2013 For installing package
        {
            if (pkgfilenames != null && pkgfilenames.Length > 0)
            {
                return service.installPackage(pkgfilenames, autoLoad , overwrite);//(packagename, filepath);
            }
            return null;
        }

        //Load from zip
        public UAReturn PackageFileLoad(string filename)//06Dec2013 For loading package in R memory for use
        {
            if (filename != null && filename.Length > 0)
            {
                string filepath = Path.GetDirectoryName(filename);
                string packagename = Path.GetFileName(filename);
                return service.loadPackage(packagename);
                //appwindow.RefreshRecent();
            }
            return null;
        }

        //Load from list
        public UAReturn LoadPackageFromList(string[] packagenames)//06Dec2013 For loading package in R memory for use
        {
            return service.loadPackageFromList(packagenames);
        }

        //Unload packages
        public UAReturn PackageFileUnload(string[] packagenames)//06Dec2013 For loading package in R memory for use
        {
            return service.unloadPackage(packagenames);
            //appwindow.RefreshRecent();
        }

        //Uninstall packages
        public UAReturn PackageFileUninstall(string[] packagenames)//06Dec2013 For uninstalling package
        {
            return service.uninstallPackage(packagenames);
            //appwindow.RefreshRecent();
        }

        //Get List of Datasets in a R pacakge
        public UAReturn GetDatasetListFromRPkg(string packagename)//12Feb2019 Get list of datasets available in a R package
        {
            return service.loadRPkgDatasetList(packagename);
        }

        #region Mouse Busy/Free
        Cursor defaultcursor;
        private void ShowMouseBusy()
        {
            defaultcursor = Mouse.OverrideCursor;
            Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
        }

        private void ShowMouseFree()
        {
            Mouse.OverrideCursor = null;
        }

        #endregion


        #region Others
        // Returns string[] by converting SimplDataType object that holds results
        public string[] GetUAReturnStringResult(UAReturn rlst)
        {
            string[] strarr = null;
            if (rlst != null && rlst.Success && rlst.SimpleTypeData != null)
            {
                if (rlst.SimpleTypeData.GetType().Name.Equals("String"))
                {
                    strarr = new string[1];
                    strarr[0] = rlst.SimpleTypeData as string;
                }
                else if (rlst.SimpleTypeData.GetType().Name.Equals("String[]"))
                {
                    strarr = rlst.SimpleTypeData as string[];
                }
            }
            return strarr;
        }

        //Return an array of strings those are not present in small array ( return array difference )
        public string[] GetStringArrayUncommon(string[] largearr, string[] smallarr)
        {
            int diff = largearr.Length - smallarr.Length;
            string[] strarr = new string[diff];
            int i = 0;
            bool found = false;
            foreach (string s in largearr)
            {
                found = false;
                foreach (string ss in smallarr)
                {
                    if (s.Equals(ss))
                    {
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    strarr[i] = s;
                    i++;
                }
            }
            return strarr;
        }
        #endregion
    }
}
