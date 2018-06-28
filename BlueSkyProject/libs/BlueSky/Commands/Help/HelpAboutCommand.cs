using System;

using BlueSky.CommandBase;
using System.Windows;
using System.Reflection;

namespace BlueSky.Commands.Help
{
    public class HelpAboutCommand : BSkyCommandBase
    {
        protected override void OnPreExecute(object param)
        {
            
        }

        protected override void OnExecute(object param)
        {
            Version version = Assembly.GetExecutingAssembly().GetName().Version;
            string strfullversion = version.ToString(); //Full version with four parts
            string shortversion = version.Major.ToString() +"."+ version.Minor.ToString();//Short version, just first two sections.
            //string BSkyStr = "BlueSky Application x64 (Commercial Edtion).\nVersion: "+ strfullversion;

            string BSkyStr = string.Empty;

            BSkyStr = string.Format("BlueSky Statistics x64 (Open Source Desktop Edition).\nVersion: {0}", strfullversion);

            string newlines = "\n\n";
            string Partner = string.Empty; /* "Please contact Smart Vision Europe Ltd.\n" +
                              "Level 17, Dashwood House, 69 Old Broad Street, London. EC2M 1QS.\n" +
                              "Email: support@sv-europe.com\n" +
                              "Switchboard: 0207 786 3568\n";*/
            MessageBox.Show(BSkyStr+ newlines + Partner, "About BlueSky Statistics", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        protected override void OnPostExecute(object param)
        {
            
        }

    }
}
