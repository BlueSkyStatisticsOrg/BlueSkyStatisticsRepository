using BSky.Lifetime.Interfaces;
using System.Collections.Generic;
using System.IO;

namespace BSky.Statistics.Common
{
    public enum ServerDataSourceTypeEnum : uint { Unknown=0, SPSS = 1, CSV = 2, XLS = 3, XLSX = 4, DBF = 5, RDATA = 6, ROBJ = 7, SAS=8, DAT=9, TXT=10 } //TXT = 7
    // Remove ROBJ in above line when new uadatapackage is in use

    public class ServerDataSource
    {

        public ServerDataSourceTypeEnum DataSetType { get; set; }
        public int RowCount { get; set; }

        public string Extension { get; private set; }
        public bool HasHeader { get; set; }
        public string FieldSeparator { get; set; }
        public string DecimalCharacter { get; set; }
        public bool IsBasketData { get; set; }
        
        public CommandDispatcher Dispatcher { get; private set; }
        public string Name { get; private set; }
        public string FileNameWithPath { get; private set; }
        public string FileName { get; private set; }
        public string SheetName { get; private set; }//29Apr2015
        public bool TrimSPSSTrailing { get; set; }//remove trailing spaces from SPSS vars(factor/character) while loading	 
        public int MaxFactors { get; set; }

        //25Oct2016 If TRUE that means we need to replace the dataset(may be because it became NULL).
        private bool replace;
        public bool Replace
        {
            get { return replace; }
            set { replace = value; }
        }

        public List<DataSourceVariable> Variables = new List<DataSourceVariable>();

        public List<DataSourceVariable> FewVariables = new List<DataSourceVariable>();

        public ServerDataSource(CommandDispatcher dispatcher, string fileName, string datasetname, string sheetname, bool removeSpacesSPSS=false, bool replace=false, IOpenDataFileOptions odfo=null)
        {
            fileName = fileName != null ? fileName : string.Empty;//to avoid crash
            this.Dispatcher = dispatcher;
            this.FileNameWithPath = fileName;//.ToLower();
            this.FileName = System.IO.Path.GetFileName(FileNameWithPath);//filename
            this.SheetName = !string.IsNullOrEmpty(sheetname) ? sheetname : string.Empty;//20Jul2018
			this.TrimSPSSTrailing = removeSpacesSPSS;										 
            this.Name = datasetname;//dataset name assigned by application to the opened dataset(.sav) file
            this.Extension = Path.GetExtension(fileName).Replace('.', ' ').Trim(); //fileName.Substring(fileName.LastIndexOf(".")+1);
            this.Replace = replace;//25Oct2016

            //16Nov2017  extra options for opening CSV/TXT/DAT flat files. 
            if (odfo == null)
            {

            }
            else
            {
                this.DecimalCharacter = odfo.DecimalPointChar.ToString();
                this.FieldSeparator = odfo.FieldSeparatorChar.ToString();
                this.HasHeader = odfo.HasHeader;
                this.IsBasketData = odfo.IsBasketData;
            }
        }

        public void Save()
        {
            
        }
       
        public void SaveAs(string fileName)//was empty function earlier
        {
            this.Dispatcher.DataSourceLoad(this, null);//Anil
        }
        
        public void Load()
        {
            this.Dispatcher.DataSourceLoad(this, null);
        }
        
        public void Close(bool saveChanges)
        {
            Dispatcher.DataSourceClose(this);
        }

        public DataSource ToClientDataSource()
        {
           return new DataSource()
            {
                IsLoaded = true,
                Name = this.Name,
                SheetName = this.SheetName,  //29Apr2015
                Extension = this.Extension,
                FieldSeparator = this.FieldSeparator,
                HasHeader = this.HasHeader,
                FileName = this.FileNameWithPath,
                Variables = this.Variables,
                FewVariables = this.FewVariables,
                RowCount = this.RowCount,
                maxfactor = this.MaxFactors,
                Replace = this.Replace,          //25Oct2016
                DecimalCharacter = this.DecimalCharacter, //16Nov2017
                IsBasketData = this.IsBasketData  //16Nov2017

           };
        }

        public UAReturn ReadRows(int startRow, int endRow)
        {
            return this.Dispatcher.DataSourceReadRows(this, startRow, endRow);

        }
      
        public UAReturn ReadCell(int row, int col) { return  this.Dispatcher.DataSourceReadCell(this, row, col); }

        public UAReturn ReadRow(int row) { return this.Dispatcher.DataSourceReadRow(this, row); } //23Jan2014 read a row, at once
    }
}
