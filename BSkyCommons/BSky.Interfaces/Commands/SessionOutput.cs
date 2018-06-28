using System.Collections.Generic;

namespace BSky.Interfaces.Commands
{
    public class SessionOutput : List<CommandOutput>
    {
        public string NameOfSession // GrandParent name in output left tree
        {
            get;
            set;
        }


        //to see if the output is R-Session output. That has a gran parent node in output tree.
        public bool isRSessionOutput 
        {
            get;
            set;

        }
    }
}
