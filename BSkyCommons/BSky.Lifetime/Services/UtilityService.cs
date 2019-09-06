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
                    logService.WriteToLogLevel("Path does not exist. "+pstrPath, LogLevelEnum.Error);
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

        //in programing languages we usually have certain rules to declare a name for an object
        //here we will check following
        // -must not contain spaces or special characters
        // -must begin with an alphabet
        //this basic function can be enhanced further by putting a check for 
        //programing language key-words/reserve-words and may be other checks.
        public bool isValidObjectname(string objname)
        {
            bool isValid = true;
            if (string.IsNullOrEmpty(objname))
                isValid = false;
            else if (!char.IsLetter(objname[0]))//&& objname[0] != '_'
                isValid = false;
            else
            {
                for (int ix = 1; ix < objname.Length; ++ix)
                    if (!char.IsLetterOrDigit(objname[ix]))// && objname[ix] != '_'
                        isValid = false;
            }
            return isValid;
        }

    }
}
