using Microsoft.Practices.Unity;
using BSky.Statistics.Service.Engine.Interfaces;
using BSky.Service.Engine;
using BSky.Statistics.Common;

namespace BSky.ServerBridge
{
    public static class BridgeSetup
    {
        public static void ConfigureContainer(IUnityContainer container)
        {
            AnalyticsService a = new AnalyticsService();
            container.RegisterInstance<IAnalyticsService>(a);
            //30Mar2015 PkgLoadStatusMessage = a.DefPkgMessage;//06Nov2014
        }
        public static string PkgLoadStatusMessage;//06Nov2014

        //30Mar2015
        public static void LoadDefaultRPackages(IAnalyticsService ia)
        {
            AnalyticsService a = (AnalyticsService) ia ;
            ia.LoadDefPackages();
            PkgLoadStatusMessage = a.DefPkgMessage;
        }

        //Install .zip R packages from local drive if they are not present in user's system.
        public static void InstallDefaultRPackagesFromZip(IAnalyticsService ia, string[] zipPaths)
        {
            PkgLoadStatusMessage = string.Empty;
            AnalyticsService a = (AnalyticsService)ia;
            UAReturn res = ia.PackageInstall(zipPaths);
            PkgLoadStatusMessage = res.SimpleTypeData.ToString();
        }
    }
}
