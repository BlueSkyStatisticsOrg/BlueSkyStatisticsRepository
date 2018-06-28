using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BSky.Interfaces.Services
{
    public struct UAMenuCommand
    {
        public string bskycommand; // for BSky command in Syntax Editor 01Aug2012
        public string commandtype; // points to AUAnalysisCommandBase or AUCommandBase
        public string commandtemplate; // XAML
        public string commandformat; // BSky function. But this is not in use.
        public string commandoutputformat; // XML
        public string text; ////04mar2013  will be used in "Command History" for displaying command name
        //public string id;//04mar2013 not in use right now.
        // public string owner; ////04mar2013 not in use right now.
    }
}
