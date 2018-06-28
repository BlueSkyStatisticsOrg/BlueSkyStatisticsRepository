using System;
using System.Collections.Generic;

namespace BSky.XmlDecoder
{
    public class TextExpansionEventArgs : EventArgs
    {
        public string Delimiter { get; set; }
        public string Result { get; set; }
    }

    public class ParamExpansionNeededEventArgs : TextExpansionEventArgs
    {
        public string DataMember { get; set; }
    }
    public class MacroExpansionNeededEventArgs : TextExpansionEventArgs
    {
        public string Macro { get; set; }
    }
    public class LabelExpansionEventArgs : EventArgs
    {
        public List<string> Result { get; set; }
        public bool Factors { get; set; }
        public string ExpansionString { get; set; }
    }
}
