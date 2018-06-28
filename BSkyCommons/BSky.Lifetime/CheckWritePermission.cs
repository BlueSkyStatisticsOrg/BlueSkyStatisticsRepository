using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Security.AccessControl;
using System.Security.Permissions;
using System.Text;

namespace BSky.Lifetime
{
    public static class CheckWritePermission
    {
        public static bool hasWritePermission(string filepath)
        {
            bool isWriteable = false;
            try
            {
                //This code returns write permission true but fails to write.
                //FileIOPermission writePermission = new FileIOPermission(
                //     FileIOPermissionAccess.Write, filepath);
                //if (SecurityManager.IsGranted(writePermission))
                //{
                //    // you have permission
                //    isWriteable = true;
                //}
                //else
                //{
                //    // permission is required!
                //}


                //Same as above. Showing write permission but not writing.
                //PermissionSet permissionSet = new PermissionSet(PermissionState.None);
                //FileIOPermission writePermission = new FileIOPermission(FileIOPermissionAccess.Write, filepath);
                //permissionSet.AddPermission(writePermission);
                //if (permissionSet.IsSubsetOf(AppDomain.CurrentDomain.PermissionSet))
                //{
                //    // You have write permissions
                //    isWriteable = true;
                //}
                //else
                //{
                //    // You don't have write permissions
                //    isWriteable = false;
                //}

                //This seems to work
                DirectoryInfo dirInfo = new DirectoryInfo(filepath);// (@"C:\Program Files\bin");
                // Get a DirectorySecurity object that represents the
                // current security settings.
                DirectorySecurity dSecurity = dirInfo.GetAccessControl();
                // Add the FileSystemAccessRule to the security settings.
                dSecurity.AddAccessRule(new FileSystemAccessRule(Environment.UserName,
                FileSystemRights.Delete, AccessControlType.Deny));

                // Set the new access settings.
                dirInfo.SetAccessControl(dSecurity);

                isWriteable = true;//this line will execute if above does not throw exception.

            }
            catch(Exception ex)
            {
                //error occurred beacause there is no write access.
                isWriteable = false;
            }
            return isWriteable;
        }
    }
}
