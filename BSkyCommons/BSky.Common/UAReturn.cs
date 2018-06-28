using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AnalyticsUnlimited.Statistics.Common
{
    public class UAReturn
    {
        public string CommandString { get; set; }
        public string Error { get; set; }
        public string StackTrace { get; set; }
        public ServerDataSource Datasource { get; set; }
        public bool Success { get; set; }
        public object Data { get; set; }
        public UAReturn() { Success = false; }
    }
}
