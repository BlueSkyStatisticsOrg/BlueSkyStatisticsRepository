using System.Collections.Generic;
using System.Windows;

namespace BSky.Interfaces.Commands
{

    public class CommandOutput : List<DependencyObject>
    {
        bool selectedfordump=false;//A.D for checking if this output is selected by user for dumping. 30May2012
        public bool SelectedForDump
        {
            get { return selectedfordump; }
            set { selectedfordump = value; }
        }
        public string NameOfAnalysis // A.D For Tree view parent node name. 02Aug2012
        {
            get;
            set;
        }

        bool isfromsyntaxeditor;//for tracking if command output is generated from syntax editor or not.10Aug2012
        public bool IsFromSyntaxEditor
        {
            get{return isfromsyntaxeditor;}
            set{isfromsyntaxeditor=value;}
        }


    }


}
