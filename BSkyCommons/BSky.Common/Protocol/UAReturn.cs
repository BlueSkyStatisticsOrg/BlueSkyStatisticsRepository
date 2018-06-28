using System.Xml;

namespace BSky.Statistics.Common
{

    public class UAReturn
    {
        public string CommandString { get; set; }
        public string Error { get; set; }
        public string StackTrace { get; set; }
        public ServerDataSource Datasource { get; set; }
        public bool Success { get; set; }
        public XmlDocument Data { get; set; }
        public object SimpleTypeData { get; set; }
        //public object ReturnResult { get; set; }//14Jun2013 This will store the return results. If SimpleTypeData can be used instead, no need of this property.
        public UAReturn() { Success = false; }
    }
}
