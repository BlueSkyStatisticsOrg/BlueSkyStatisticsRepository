using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BSky.UsageLogger
{
    public class UsageLogger
    {
        public string fullpathlogfilename { get; set; }

        Dictionary<string, int> usageFreqDict = new Dictionary<string, int>();
        //string fullpathfilename = @"D:\WinDirs\usglog.txt";

        public void readUsageLogsIfAny()
        {
            int len;
            usageFreqDict.Clear();

            try
            {
                if (File.Exists(fullpathlogfilename))
                {
                    var lines = File.ReadLines(fullpathlogfilename);
                    foreach (var line in lines)
                    {
                        string[] KeyValue = line.Split(';');
                        int freq = 0;
                        if (!string.IsNullOrEmpty(KeyValue[0]))
                        {
                            len = KeyValue.Length;//last item is frequency
                            if (len > 1)
                            {
                                if (string.IsNullOrEmpty(KeyValue[len - 1]) || !Int32.TryParse(KeyValue[len - 1], out freq))
                                    freq = 0;
                                if (!usageFreqDict.ContainsKey(KeyValue[0]))
                                    usageFreqDict.Add(KeyValue[0], freq);
                            }
                        }
                    }
                    if (usageFreqDict.Count == 0)
                    {
                        RenameUsageLogFile();
                    }
                }
            }
            catch (Exception ex)
            {
                //if we land here then something might have gone wrong reading file contents
                //we must rename current file and let app create a new one with right format.
                RenameUsageLogFile();
            }
        }

        public string DisplayUsageDetails()
        {
            string details = string.Empty;
            if (usageFreqDict.Count > 0)
            {
                StringBuilder sb = new StringBuilder();
                foreach (KeyValuePair<string, int> kvp in usageFreqDict)
                {
                    sb.Append(kvp.Key + ";" + kvp.Value + "\n");
                }
                details = (sb.ToString());
            }
            return details;
        }

        public void AddKey(string command)
        {
            if (usageFreqDict.ContainsKey(command))
            {
                //fetch freq and increment it then save it back in the dictionary
                int freqvalue = usageFreqDict[command];
                usageFreqDict[command] = freqvalue + 1;
            }
            else
            {
                usageFreqDict.Add(command, 1);
            }

            SaveToFile();
        }

        public void SaveToFile()
        {
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(fullpathlogfilename))
            {
                foreach (KeyValuePair<string, int> kvp in usageFreqDict)
                {
                    string line = kvp.Key + ";" + kvp.Value.ToString();
                    file.WriteLine(line);
                }
            }

        }

        public void RenameUsageLogFile()
        {
            DateTime startDate = new DateTime(1970, 1, 1);
            TimeSpan diff = DateTime.Now - startDate;
            string milistr = diff.TotalMilliseconds.ToString();
            string newfname = fullpathlogfilename.Replace(".txt", milistr + ".txt");
            try
            {
                System.IO.File.Move(fullpathlogfilename, newfname);
            }
            catch (Exception ex2)
            {
                //can't rename
            }
        }
    }
}
