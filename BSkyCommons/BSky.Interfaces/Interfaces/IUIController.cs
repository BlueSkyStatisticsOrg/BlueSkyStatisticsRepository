using System.Collections.Generic;
using BSky.Statistics.Common;
using BSky.Interfaces.Model;
using System.Windows.Controls;

namespace BSky.Interfaces.Interfaces
{
    public interface IUIController
    {
        //List<string> sortcolnames { get; set; } //11Apr2014
        //string sortorder { get; set; }//14Apr2014

        List<string> sortasccolnames { get; set; } //18Oct2015 names of all ascending cols
        List<string> sortdesccolnames { get; set; } //18Oct2015 names of all descending cols

        void GetAllOpenDatasetsInGrid();//Added by Anil for testing. May not be needed in actual. Actual function is GetDatasetNames(), below
        DataSource GetActiveDocument();
        TabItem GetTabItem(DataSource ds);
        DataSource GetDocumentByName(string datasetname);
        void LoadNewDataSet(DataSource list);
        void Load_Dataframe(DataSource list);
        void RefreshDataSet(DataSource list);
        void RefreshGrids(DataSource list);//25Mar2013 refresh both grids //16Jul2015 Now it only refreshes Datagrid.
        void RefreshBothGrids(DataSource list);//16Jul2015 refresh both grid when main window 'refresh' icon is clicked.
        void RefreshStatusbar();//05Dec2013 for refreshing split info at bottom right
        void closeTab();
        void closeTab(string datasetname); //02Aug2016
        void AnalysisComplete(AnalyticsData data);
        void LoadOutputWindow(IOutputWindow outputwindow);
        List<string> GetDatasetNames();
        //OutputWindow getActiveOutputWindow(IOutputWindowContainer windowcontainer);

        //No need void LoadAnalysisFromFile(string fullpathfilename);//30May2012 for getting output from .bso file (BSkyOutput)

        string ActiveModel { get; set; }
        string GetActiveModelName();
    }
}
