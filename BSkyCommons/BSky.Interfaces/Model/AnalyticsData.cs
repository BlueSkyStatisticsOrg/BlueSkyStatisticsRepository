using BSky.Statistics.Common;
using System.Xml;
using System.Windows;
using BSky.Interfaces.Commands;

namespace BSky.Interfaces.Model
{
    public class AnalyticsData
    {
        string analysisType;

        public string AnalysisType
        {
            get { return analysisType; }
            set { analysisType = value; }
        }
        XmlDocument inputXml;

        public XmlDocument InputXml
        {
            get { return inputXml; }
            set { inputXml = value; }
        }
        DataSource dataSource;

        public DataSource DataSource
        {
            get { return dataSource; }
            set { dataSource = value; }
        }
        UAReturn result;

        public UAReturn Result
        {
            get { return result; }
            set { result = value; }
        }

        FrameworkElement inputElement;

        public FrameworkElement InputElement
        {
            get { return inputElement; }
            set { inputElement = value; }
        }

        string outputTemplate;

        public string OutputTemplate
        {
            get { return outputTemplate; }
            set { outputTemplate = value; }
        }

        string preparedommand; //// AD. for storing ready to execute command of current analysis. 01Jun2012
        public string PreparedCommand
        {
            get { return preparedommand; }
            set { preparedommand = value; }
        }

        CommandOutput output;//A.D for storing ouput of current analysis. 30May2012
        public CommandOutput Output
        {
            get { return output; }
            set { output = value; }
        }

        SessionOutput sessionoutput;//A.D for storing ouput of current session output analysis. 27Nov2013
        public SessionOutput SessionOutput
        {
            get { return sessionoutput; }
            set { sessionoutput = value; }
        }


        //10Jan2013 uncommented
        bool selectedfordump;//A.D for checking if this output is selected(frm SynEdt) by user for dumping. 30May2012
        public bool SelectedForDump
        {
            get { return selectedfordump; }
            set { selectedfordump = value; }
        }
    }
}
