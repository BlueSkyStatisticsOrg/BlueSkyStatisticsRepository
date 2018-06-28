using System.Collections.Generic;

namespace BlueSky
{
    class SessionDialogContainer
    {
        static Dictionary<string, object> sessionDialogs = new Dictionary<string, object>(); //container for the dialogs in current session.
        public Dictionary<string, object> SessionDialogList
        {
            get { return sessionDialogs; }
        }

    }
}
