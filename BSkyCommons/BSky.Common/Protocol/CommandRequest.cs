using System;

namespace BSky.Statistics.Common
{
    [Serializable]
    public class CommandRequest
    {
        private string _name;
        private string _CommandSyntax;

        public string CommandSyntax { get { return _CommandSyntax; } set { _CommandSyntax = value; } }
       
        public CommandRequest() { }
        public CommandRequest(string name, string commandSyntax) { _name = name; _CommandSyntax = commandSyntax; }
    }
}
