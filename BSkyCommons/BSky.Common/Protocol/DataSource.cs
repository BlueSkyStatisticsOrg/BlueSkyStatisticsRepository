using System.Collections.Generic;
using BSky.Lifetime;
using BSky.Lifetime.Interfaces;
using System;
using System.Text;
using System.IO;
using BSky.ConfService.Intf.Interfaces;

namespace BSky.Statistics.Common
{
    
    public enum UADataType : uint { UAUnKnown, UAInt, UAIntList, UAIntMatrix, UAString, UAStringList, UAStringMatrix, UADouble, UADoubleList, UADoubleMatrix, UAList, UATableList, UADataFrame }
    public enum DataColumnTypeEnum : uint { Integer, Numeric, Double, Factor, Ordinal, Character, Logical, POSIXlt, POSIXct, Date, Unknown }
    public enum DataColumnAlignmentEnum : uint { Left, Right, Center }
    public enum DataColumnMeasureEnum : uint { Scale, Ordinal, Nominal, String, Logical, Date }
    public enum DataColumnRole : uint { Input, Target, Both, None, Partition, Split }

    public class DataSource
    {
        //public int counter = 0; Just for testing howmany time _DF in virtualList was accessed to fetch visible area of the grid.

        //for getting maxfactor from config options settings
        IConfigService confService = LifetimeService.Instance.Container.Resolve<IConfigService>();//03Feb2015

        private object data;

        public object Data
        {
            get { return data; }
            set { data = value; }
        }
        private bool hasHeader;

        public bool HasHeader
        {
            get { return hasHeader; }
            set { hasHeader = value; }
        }
        private string fieldSperator;

        public string FieldSeparator
        {
            get { return fieldSperator; }
            set { fieldSperator = value; }
        }

        private string deciChar;
        public string DecimalCharacter
        {
            get { return deciChar; }
            set { deciChar = value; }
        }

        private bool isBasketData;
        public bool IsBasketData
        {
            get { return isBasketData; }
            set { isBasketData = value; }
        }

        private string extension;


        public string Extension
        {
            get { return extension; }
            set { extension = value; }
        }
        private bool isLoaded;


        public bool IsLoaded
        {
            get { return isLoaded; }
            set { isLoaded = value; }
        }
        private string name;

        public string Name
        {
            get { return name; }
            set { name = value; }
        }
        private string fileName;

        public string FileName
        {
            get { return fileName; }
            set { fileName = value; }
        }

        private string sheetName; //29Apr2015 Added so that, multiple sheets from same Excel file can be opened in Datagrid tabs

        public string SheetName
        {
            get { return sheetName; }
            set { sheetName = value; }
        }

        private int rowCount;

        public int RowCount
        {
            get { return rowCount; }
            set { rowCount = value; }
        }

        //15Apr2014. For reloading dataset, we need to know filetype. This is best place to do it.
        public string FileType 
        {
            get
            {
                string extn = string.Empty;//before 03Jul2016 extension.ToLower(); 
                if(FileName!=null && FileName.Length>0)//03Jul2016 Better way to find extension
                    extn = Path.GetExtension(fileName).Replace('.', ' ').Trim().ToLower();
                string ftype = string.Empty;
                if (extn.Equals("sav")) ftype = "SPSS";
                else if (extn.Equals("xls")) ftype = "XLS";
                else if (extn.Equals("xlsx")) ftype = "XLSX";
                else if (extn.Equals("csv")) ftype = "CSV";
                else if (extn.Equals("dbf")) ftype = "DBF";
                else if (extn.Equals("rdata")) ftype = "RDATA";
                else if (extn.Equals("rda")) ftype = "RDA";
                else if (extn.Equals("txt")) ftype = "TXT";

                return ftype;
            }
        }
        private List<DataSourceVariable> variables = new List<DataSourceVariable>();

        public List<DataSourceVariable> Variables
        {
            get { return variables; }
            set { variables = value; }
        }

        //For  a subset of variables those are currently in the viewport of the datagrid.
        private List<DataSourceVariable> fewvariables = new List<DataSourceVariable>();

        public List<DataSourceVariable> FewVariables
        {
            get { return fewvariables; }
            set { fewvariables = value; }
        }


        #region Start and End Col indexes to be loaded in the grid in Dynamic Col loading feature
        private int _startColindex;
        public int StartColindex
        {
            get { return _startColindex; }
            set { _startColindex = value; }
        }

        private int _endColindex;
        public int EndColindex
        {
            get { return _endColindex; }
            set { _endColindex = value; }
        }

        //This bool will be set to true if any of the FIRST PREV NEXT LAST pagination button is clicked.
        //This is needed because we have a common grid refresh method for pagination and Refresh icon(or loadRefreshDataFrame)
        //Now when refresh icon is clicked we want start and end index to be reset to 0 and 15
        //But when pagination is clicked that time we need to refresh grid based on where pagination is suppose to take us. That time
        // we do not want indexes to be reset to 0 and 15.
        //This bool will be set to true just before calling refresh (when pagination clicked) and will be made false after returning from refresh.
        private bool _paginationButtonClicked;
        public bool IsPaginationClicked 
        {
            get { return _paginationButtonClicked; }
            set { _paginationButtonClicked = value; }
        }
        #endregion


        //03Dec2013 Stausbar Split info:
        //public string SplitInfo
        //{
        //    get;
        //    set;
        //}

        //private ObservableCollection<DataSourceVariable> _ObservableVariables = new ObservableCollection<DataSourceVariable>();
        //public ObservableCollection<DataSourceVariable> ObservableVariables
        //{
        //    get { return _ObservableVariables; }
        //    set { _ObservableVariables = ObservableVariables; }
        //}


        private bool changed;// Added by Anil to track if dataset is changed. Confirmation popup appears while closing dataset.

        public bool Changed
        {
            get { return changed; }
            set { changed = value; }
        }

        private int _maxfactor;// Maximum factors allowed.
        public int maxfactor
        {
            get
            {
                //get latest maxfactor from config settings, if available.
                string maxfac = confService.GetConfigValueForKey("maxfactorcount");
                if (maxfac.Trim().Length != 0)
                {
                    Int32.TryParse(maxfac, out _maxfactor);
                }
                //setting maximum limit. Later we can figure out what is the limits for R or Rcmdr
                // and the set accordingly
                if (_maxfactor > 100)
                    _maxfactor = 100;

                return _maxfactor;
            }
            set
            {
                _maxfactor = value;
            }
        }

        //for storing error/warning or info message to pass it to different layer
        // WE can also have Enum to set if message is for ERROR, WARNING or INFO
        private string message;
        public string Message 
        {
            get { return message; }
            set { message = value; } 
        }

        //25Oct2016 If TRUE that means we need to replace the dataset(may be because it became NULL).
        private bool replace;
        public bool Replace 
        {
            get { return replace; }
            set { replace = value; }
        }

        //True = New blank dataset. This new blank dataset will be processed by removing empty rows and cols
        //and then this property will be changed to false;
        private bool _isUnprocessed;
        public bool isUnprocessed
        {
            get { return _isUnprocessed; }
            set { _isUnprocessed = value; }
        }
    }
    public class DataSourceVariable
    {

        private string _Name;

        public string Name
        {
            get { return _Name; }
            set
            {
                //_RName = value;//19Sep2014 This is the original R variable name. It may contain special chars
                //_Name = _RName.Replace(".", "_").Replace("(", "_").Replace(")", "_"); //19Jul2015 putting back replace. //value;//Looks like the issue fixed by component1 and we do not need to do this -> .Replace(".", "");//19Sep2014
                _Name = ReplaceSpecialCharacters(value); // this is the property name derived from RName and does not contain any special chars.
                //_XName = value; // This is same as RName and can be used somewhere. used for aggregate control.
            }
        }

        //Converting R column name to valid C# variable/property name(replacing spaces and special chars with underscore)
        private string ReplaceSpecialCharacters(string str)
        {
            int i = 0;
            StringBuilder sb = new StringBuilder();
            foreach (char c in str)
            {
                if(i==0 && (c >= '0' && c <= '9')) // if the first character is numeric then replace it with underscore
                {
                    sb.Append("_");
                }
                else if ((i>0 && (c >= '0' && c <= '9')) || (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || (c == '_'))//valid characters
                {
                    sb.Append(c);
                }
                else // All invalid/special characters are converted to underscore
                {
                    sb.Append("_");
                }
                i++;
            }
            return sb.ToString();
        }

        //01Feb2017 XName is used in some aggregate control to change say mpg to mean(mpg)
        private string _XName;

        public string XName
        {
            get { return _XName; }
            set
            {
                _XName = value;//19Sep2014
                //_Name = _RName.Replace(".", "_"); //19Jul2015 putting back replace. //value;//Looks like the issue fixed by component1 and we do not need to do this -> .Replace(".", "");//19Sep2014
            }
        }

        private DataColumnTypeEnum _dataType = DataColumnTypeEnum.Numeric; //typeof of the col

        public DataColumnTypeEnum DataType
        {
            get { return _dataType; }
            set { _dataType = value; }
        }

        private string _dataClass = ""; //class of the column

        public string DataClass
        {
            get { return _dataClass; }
            set { _dataClass = value; }
        }

        private int _width = 4;

        public int Width
        {
            get { return _width; }
            set { _width = value; }
        }

        private int _decimals = 0;

        public int Decimals
        {
            get { return _decimals; }
            set { _decimals = value; }
        }

        private string _Label;

        public string Label
        {
            get { return _Label; }
            set { _Label = value; }
        }

        private List<string> _Values = new List<string>();

        public List<string> Values
        {
            get { return _Values; }
            set { _Values = value; }
        }

        //////15Jan2018 Numeric Factor Levels (for Joao)
        ////private List<string> _NumFacLevels = new List<string>();
        ////public List<string> NumFacLevels
        ////{
        ////    get { return _NumFacLevels; }
        ////    set { _NumFacLevels = value; }
        ////}

        private List<string> _Missing = new List<string>();

        public List<string> Missing
        {
            get { return _Missing; }
            set { _Missing = value; }
        }

        private string _missingType;//none, three, range+1

        public string MissType
        {
            get { return _missingType; }
            set { _missingType = value; }
        }

        private uint _Columns = 8;//changed from object type to uint. Always +ve int.  Anil

        public uint Columns
        {
            get { return _Columns; }
            set { _Columns = value; }
        }

        private DataColumnAlignmentEnum _Alignment = DataColumnAlignmentEnum.Left;

        public DataColumnAlignmentEnum Alignment
        {
            get { return _Alignment; }
            set { _Alignment = value; }
        }
        private DataColumnMeasureEnum _Measure = DataColumnMeasureEnum.Scale;

        public DataColumnMeasureEnum Measure
        {
            get { return _Measure; }
            set
            {
                _Measure = value; /// if following path does not work then use converters ///
                if (_Measure == DataColumnMeasureEnum.Nominal) _ImgURL = "/Images/nominal.png";
                else if (_Measure == DataColumnMeasureEnum.Ordinal) _ImgURL = "/Images/ordinal.png";
                else if (_Measure == DataColumnMeasureEnum.Scale) _ImgURL = "/Images/scale.png";
                else if (_Measure == DataColumnMeasureEnum.String) _ImgURL = "/Images/String.png";
                else if (_Measure == DataColumnMeasureEnum.Logical) _ImgURL = "/Images/Logical.png";
                else if (_Measure == DataColumnMeasureEnum.Date) _ImgURL = "/Images/Date.png";
                else _ImgURL = "/Images/none.png";
            }
        }
        private DataColumnRole _Role = DataColumnRole.Input;

        public DataColumnRole Role
        {
            get { return _Role; }
            set { _Role = value; }
        }

        private int _RowCount;// remove this property. Anil

        public int RowCount
        {
            get { return _RowCount; }
            set { _RowCount = value; }
        }

        public override string ToString()
        {
            //30Sep2014 return this.Name;
            return this.RName; //30Sep2014
        }

        private string _ImgURL;
        public string ImgURL
        {
            get { return _ImgURL; }
            set { _ImgURL = value; }
        }

        //17Apr2014 Map was getting lost earlier and must be saved with each column, if exists(if Scale to Nominal/Ordinal is done)
        private List<FactorMap> _factormapList = new List<FactorMap>();
        public List<FactorMap> factormapList
        {
            get
            {
                return _factormapList;
            }
            set
            {
                _factormapList = factormapList;
            }
        }

        //18Apr2013//
        private int _sortType;
        public int SortType
        {
            get { return _sortType; }
            set
            {
                if (value < 0)
                    _sortType = -1;//Descending
                else if (value > 0)
                    _sortType = 1; //Ascending
                else
                    _sortType = 0; // No Sorting
            }
        }

        //01Feb2017 This property will hold the R side var name with special chars in it. Since var with 
        // special chars are not alloed in C#(for property names), we will replace spl char with underscore
        // and put this version of var name in the DataSourceVariable.Name property. Now DataSourceVariable.Name 
        // property have all spl chars replaced with underscore.So it can now be used in C#(in virtual list property name)
        private string _RName;
        public string RName
        {
            get { return _RName; }
            set
            {
                _RName = value;//19Sep2014 This is the original R variable name. It may contain special chars
                _Name = ReplaceSpecialCharacters(value); // this is the property name derived from RName and does not contain any special chars.
                
                //_XName = value; // This is used for target list and other stuff where we can to enclose the varname in some function. like sum(mpg)
                // And is set only when we move variable from sourcelist to aggregate target listbox.
            }
        }

        private double _UTCOffset;//Holds UTC offset if class is POSIXct. May be later Date class will be supported too.

        public double UTCOffset
        {
            get { return _UTCOffset; }
            set { _UTCOffset = value; }
        }

        private bool _isAllNA;

        public bool isAllNA
        {
            get { return _isAllNA; }
            set { _isAllNA = value; }
        }

    }

    //Added by Aaron 08/26/2014
    //Added by Aaron to capture the name and icon for a dataset
    //We are going to use the same code for variable lists and datasets. So we create a stack panel in the listbox to
    //display the ico and text(name) of the dataset

    public class DatasetDisplay
    {
        private string _name;
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        private string _imgurl;
        public string ImgURL
        {
            get { return _imgurl; }
            set { _imgurl =value; }
        }

    }

}
