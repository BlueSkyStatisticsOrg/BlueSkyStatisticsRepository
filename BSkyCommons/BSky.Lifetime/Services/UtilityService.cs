using BSky.Lifetime.Interfaces;
using System;
using System.Security.Principal;
using System.Security.AccessControl;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BSky.Lifetime.Services
{
    public class UtilityService
    {
        ILoggerService logService = LifetimeService.Instance.Container.Resolve<ILoggerService>();//17Dec2012

        //Best so far.
        public bool isWritableDirectory(string pstrPath)
        {
            try
            {
                if (!Directory.Exists(pstrPath)) //check if dir exeists. before chcking the access rights
                {
                    return false; //dir does not exist so no write access;
                }

                bool writeable = false;
                WindowsPrincipal principal = new WindowsPrincipal(WindowsIdentity.GetCurrent());
                DirectorySecurity security = Directory.GetAccessControl(pstrPath);
                AuthorizationRuleCollection authRules = security.GetAccessRules(true, true, typeof(SecurityIdentifier));

                foreach (FileSystemAccessRule accessRule in authRules)
                {

                    if (principal.IsInRole(accessRule.IdentityReference as SecurityIdentifier))
                    {
                        if ((FileSystemRights.WriteData & accessRule.FileSystemRights) == FileSystemRights.WriteData)
                        {
                            if (accessRule.AccessControlType == AccessControlType.Allow)
                            {
                                writeable = true;
                                break;
                            }
                            else if (accessRule.AccessControlType == AccessControlType.Deny)
                            {
                                //Deny usually overrides any Allow
                                return false;
                            }

                        }
                    }
                }
                return writeable;
            }
            catch (Exception ex)// (UnauthorizedAccessException)
            {
                string m1 = "Exception in isWritableDirectory, for : " + pstrPath;
                logService.WriteToLogLevel(m1, LogLevelEnum.Error);
                logService.WriteToLogLevel(ex.Message, LogLevelEnum.Error);
                return false;
            }
        }

        public string GetDirectoryFromFullPathFilename(string fullpathfilename)
        {
            string directory = string.Empty;
            directory = Path.GetDirectoryName( Path.GetFullPath(fullpathfilename));
            return directory;
        }

    }
}
