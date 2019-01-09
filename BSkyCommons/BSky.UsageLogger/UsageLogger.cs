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
            usageFreqDict.Clear();
            if (File.Exists(fullpathlogfilename))
            {
                var lines = File.ReadLines(fullpathlogfilename);
                foreach (var line in lines)
                {
                    string[] KeyValue = line.Split(',');
                    int freq = 0;
                    if (!string.IsNullOrEmpty(KeyValue[0]))
                    {
                        if (string.IsNullOrEmpty(KeyValue[1]) || !Int32.TryParse(KeyValue[1], out freq))
                            freq = 0;
                        usageFreqDict.Add(KeyValue[0], freq);
                    }
                }
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
                    sb.Append(kvp.Key + "," + kvp.Value + "\n");
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
                    string line = kvp.Key + "," + kvp.Value.ToString();
                    file.WriteLine(line);
                }
            }

        }

    }
}
