using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AnalyticsUnlimited.Client_WPF.Services;

namespace AnalyticsUnlimited.Client_WPF.Commands.History
{
    class DatasetExecutedCommands
    {
        string _DatasetName;
        List<UAMenuCommand> _ExecutedCommands = new List<UAMenuCommand>();

        public string DatasetName
        {
            get { return _DatasetName; }
            set { _DatasetName = value; }
        }

        public List<UAMenuCommand> ExecutedCommands
        {
            get { return _ExecutedCommands; }
        }

    }
}
