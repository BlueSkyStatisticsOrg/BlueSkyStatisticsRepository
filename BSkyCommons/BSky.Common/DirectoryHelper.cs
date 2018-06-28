using System.IO;
using System.Reflection;
using System;

namespace BSky.Statistics.Common
{

    public class DirectoryHelper
    {
        public static string UserPath;
        public static string GetRootDirectory()
        {
            if(UserPath!=null && UserPath.Length>0)
                return UserPath;  
            else 
                return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        }
        public static string GetJournalFileName()
        {
            return Path.Combine(GetJournalDirectory(), "BSkyJournal.txt");
        }
        public static string GetUserJournalFileName()
        {
            return Path.Combine(GetJournalDirectory(), "BSkyUserJournal.txt");
        }
        public static string GetLogFileName()
        {
            //string rlogfname = Path.Combine(GetLogDirectory(), "RLog.txt");
            return Path.Combine(GetLogDirectory(), "RLog.txt");
        }
        public static string GetErrorFileName()//12Sep2014
        {
            //string rlogfname = Path.Combine(GetLogDirectory(), "RLog.txt");
            return Path.Combine(GetLogDirectory(), "RError.txt");
        }
        
        public static string GetJournalDirectory()
        {
            return GetRelativeDirectory("Journal");
        }
        public static string GetLogDirectory()
        {
            return GetRelativeDirectory("Log");
        }
        private static string GetRelativeDirectory(string dirName)
        {
            string dirPath = Path.Combine(GetRootDirectory(), dirName);
            if (!Directory.Exists(dirPath))
                Directory.CreateDirectory(dirPath);

            return dirPath;
        }


    }


}
