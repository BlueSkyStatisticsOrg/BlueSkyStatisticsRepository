using BSky.Interfaces.Model;

namespace BSky.Interfaces.Interfaces
{
    public interface IOutputWindow
    {
        string WindowName
        { get; set; }

        void Show();
        void AddAnalyis(AnalyticsData data);
        void loadGettingStarted();//18Sep2017 loadgetting started.R that comes with BSky setup
        void ExportC1FlexGridToPDF(string s, string s2, object o);//Object O here is always AUXgrid
        //No need void AddAnalyisFromFile(string fullpathfilename);//30May2012
        //void SaveAnalysisBinary();
        //void LoadAnalysisBinary();
    }
}
