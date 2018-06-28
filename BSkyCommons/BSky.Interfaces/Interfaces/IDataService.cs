using BSky.Lifetime.Interfaces;
using BSky.Statistics.Common;

namespace BSky.Interfaces.Interfaces
{
    public interface IDataService
    {
        string GetFreshDatasetName();//t ot get next available Dataframename( Dataset2, Dataset3 and so on)
        DataSource NewDataset();//03Jan2014
        DataSource Open(string filename, string sheetname, IOpenDataFileOptions odfo=null);
        DataSource OpenDataframe(string dframename, string sheetname, string fname=""); //31May2018 for passing RData filename //13Feb2014
        UAReturn ImportTableListFromSQL(string sqlcommand);
        object GetOdbcTableList(string filename);//27Jan2014
        object GetRDataDataframeObjList(string filename);//23May2018
        DataSource Refresh(DataSource dsname);//25Mar2013
        void SaveAs(string filname, DataSource ds);//Anil
        void Close(DataSource ds);
        bool isDatasetNew(string dsname);//17Feb2014

        //27Oct2016
        #region R Object save / Load

        UAReturn GetAllRObjects();
        UAReturn SaveRObjects(string objname, string fullpathfilename);
        UAReturn LoadRObjects(string fullpathfilename);

        #endregion

        //06Dec2013
        #region Package Related
        UAReturn installPackage(string[] pkgfilenames, bool autoLoad = true, bool overwrite = false);//(string package, string filepath);
        UAReturn installCRANPackage(string packagename);
        UAReturn installReqPackageCRAN(string packagename);//27Aug2015
        UAReturn setCRANMirror();


        UAReturn loadPackage(string package);
        UAReturn loadPackageFromList(string[] packagenames);

        UAReturn showInstalledPackages();
        UAReturn showLoadedPackages();
        UAReturn getMissingDefaultRPackages();

        UAReturn unloadPackage(string[] packagenames);
        UAReturn uninstallPackage(string[] packagenames);

        #endregion
    }

}
