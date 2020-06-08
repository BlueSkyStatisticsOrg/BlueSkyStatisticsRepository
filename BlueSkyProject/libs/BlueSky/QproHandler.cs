using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlueSky
{
    public static class QproHandler
    {
        private static Dictionary<string, QProDatasetInfo> QproDatasetInfoList = new Dictionary<string, QProDatasetInfo>();
        public static string htmltext = string.Empty;

        //public static bool overwriteDataset = true;//since QPro dialog will prompt user for overwrite, we always overwrite.
        //If user clicks NO when overwrite prompt is shown, we never come to this code anyway. So we always overwrite.

        //whenever Qpro dataset is opened, execute this
        public static void AddQPDatasetInfo(string filename, string apikey, string datasetid,
            string surveyid, string userid, string datasetname)
        {
            QProDatasetInfo qpdsi = new QProDatasetInfo();
            qpdsi.Filename = filename;
            qpdsi.ApiKey = apikey;
            qpdsi.DatasetId = datasetid;//this should be the key but for user 'datasetname' is easier so
            qpdsi.SurveyId = surveyid;
            qpdsi.UserId = userid;
            qpdsi.DatasetName = datasetname; //this is the key for user convenience

            if(RemoveQPDatasetInfo(datasetname))//remove the key if it exists
                QproDatasetInfoList.Add(datasetname, qpdsi);//add the key. This is always a non existing key 
            //as we have deleted the exising one in the above line. 
        }

        public static void AddQPDatasetInfo(QProDatasetInfo qpdi)
        {
            if(RemoveQPDatasetInfo(qpdi.DatasetName))//remove the key if it exists
                QproDatasetInfoList.Add(qpdi.DatasetName, qpdi);//add the key. This is always a non existing key 
            //as we have deleted the exising one in the above line. 
        }


        //Get and item from the dictionary
        public static QProDatasetInfo GetQPDatasetInfo(string datasetname)
        {
            if (!string.IsNullOrEmpty(datasetname))
            {
                QProDatasetInfo qpdsi = new QProDatasetInfo();
                bool successs = QproDatasetInfoList.TryGetValue(datasetname, out qpdsi);
                if (successs)
                    return qpdsi;
                else
                    return null;
            }
            else
                return null;
        }

        //Get QPro filename from the dictionary
        public static string GetQPDatasetFileName(string datasetname)
        {
            if (!string.IsNullOrEmpty(datasetname))
            {
                QProDatasetInfo qpdsi = new QProDatasetInfo();
                bool successs = QproDatasetInfoList.TryGetValue(datasetname, out qpdsi);
                if (successs)
                    return qpdsi.Filename;
                else
                    return null;
            }
            else
                return null;
        }

        //Get all the keys (i.e. datasetnames) currently available.
        public static List<string> GetKeys()
        {
            List<string> keys = new List<string>();
            Dictionary<string, QProDatasetInfo>.KeyCollection keycoll = QproDatasetInfoList.Keys;
            foreach (string key in keycoll)
                keys.Add(key);
            return keys;
        }

        //whenever Qpro dataset is closed, we can execute this
        public static bool RemoveQPDatasetInfo(string datasetname) //datasetid is the key
        {
            bool isSuccess = true;
            if (!string.IsNullOrEmpty(datasetname) && QproDatasetInfoList.ContainsKey(datasetname))
                isSuccess=QproDatasetInfoList.Remove(datasetname);
            return isSuccess;
        }

        //may be required. Remoes all the keys in one shot.
        public static void RemoveAllQPDatasetInfo(string datasetname) 
        {
                QproDatasetInfoList.Clear();
        }

    }

    public class QProDatasetInfo
    {
        private string filename;

        public string Filename
        {
            get { return filename; }
            set { filename = value; }
        }

        private string apikey;

        public string ApiKey
        {
            get { return apikey; }
            set { apikey = value; }
        }

        private string datasetid;

        public string DatasetId
        {
            get { return datasetid; }
            set { datasetid = value; }
        }

        private string surveyid;

        public string SurveyId
        {
            get { return surveyid; }
            set { surveyid = value; }
        }

        private string userid;

        public string UserId
        {
            get { return userid; }
            set { userid = value; }
        }

        private string datasetname; //provided by user from the Qpro GET dialog

        public string DatasetName
        {
            get { return datasetname; }
            set { datasetname = value; }
        }

        private string errorMsg;

        public string ErrorMsg
        {
            get { return errorMsg; }
            set { errorMsg = value; }
        }

    }
}
