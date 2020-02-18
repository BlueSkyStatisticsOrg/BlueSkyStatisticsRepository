using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using BSky.Statistics.Common;
using BSky.Statistics.Service.Engine.Interfaces;
using System.Collections;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;
using System.Xml;
using RDotNet;

namespace BlueSky.Model
{
    public class VirtualPropertyDescriptorDynamic : PropertyDescriptor
    {
        string fPropertyName;
        Type fPropertyType;
        bool fIsReadOnly;
        VirtualListDynamic fList; 

        int fIndex;

        public VirtualPropertyDescriptorDynamic(VirtualListDynamic fList, int fIndex, string fPropertyName, Type fPropertyType, bool fIsReadOnly)
            : base(fPropertyName, null)
        {

            this.fPropertyName = fPropertyName;
            this.fPropertyType = fPropertyType;
            this.fIsReadOnly = fIsReadOnly;
            this.fList = fList;
            this.fIndex = fIndex;
        }

        public override bool CanResetValue(object component)
        {
            return false;
        }

        public override object GetValue(object component)
        {
            return fList.GetCellValue(component, fIndex);
        }

        public override void SetValue(object component, object val)
        {
            fList.SetCellValue(component, fIndex, val);
        }

        public override bool IsReadOnly { get { return fIsReadOnly; } }

        public override string Name { get { return fPropertyName; } }

        public override Type ComponentType { get { return typeof(VirtualListDynamic); } }

        public override Type PropertyType { get { return fPropertyType; } }

        public override void ResetValue(object component)
        {
        }

        public override bool ShouldSerializeValue(object component) { return true; }
    }

    public class VirtualListDynamic : IBindingList, ITypedList, IEnumerator
    {
        IAnalyticsService _service;
        DataSource _dataSource;
        DataFrame _DF;

        int fRecordCount;
        int fColumnCount;

        Hashtable fValues;
        PropertyDescriptorCollection fColumnCollection;
        bool fUseDataStore = true;
        ListChangedEventHandler listChangedHandler;
        private Type type;
        public Type RowClassType 
        {
            get { return type; }
        }

        public VirtualListDynamic(IAnalyticsService service, DataSource dataSource)
        {
            _service = service;
            _dataSource = dataSource;
            fRecordCount = _dataSource.RowCount;
            fColumnCount = _dataSource.FewVariables.Count;

            fValues = new Hashtable();
            CreateColumnCollection();
            type = GetObjectType(dataSource.FewVariables);
        }

        Dictionary<object, int> dict = new Dictionary<object, int>();
        Dictionary<int, object> dictIndex = new Dictionary<int, object>();

        private Type GetObjectType(List<DataSourceVariable> vars)
        {
            AssemblyName assemblyName = new AssemblyName();
            assemblyName.Name = "tmpAssembly";
            AssemblyBuilder assemblyBuilder = Thread.GetDomain().DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
            ModuleBuilder module = assemblyBuilder.DefineDynamicModule("tmpModule");
            int varnameidx = 0;
            // create a new type builder
            TypeBuilder typeBuilder = module.DefineType("BindableRowCellCollection", TypeAttributes.Public | TypeAttributes.Class);

            foreach (DataSourceVariable var in vars)
            {
                Type type = (var.DataClass.Equals("POSIXct") || var.DataClass.Equals("Date")) ? typeof(string) : typeof(string);//28Aug2017 Date as string in grid

                string propertyName = var.Name.Replace(".", "_").Replace("(", "_").Replace(")", "_"); 

                // Generate a private field
                FieldBuilder field = typeBuilder.DefineField("_" + propertyName, typeof(string), FieldAttributes.Private);
                // Generate a public property
                PropertyBuilder property =
                    typeBuilder.DefineProperty(propertyName,
                                     PropertyAttributes.None,
                                     type,
                                     new Type[] { type });

                // The property set and property get methods require a special set of attributes:

                MethodAttributes GetSetAttr =
                    MethodAttributes.Public |
                    MethodAttributes.HideBySig;

                // Define the "get" accessor method for current private field.
                MethodBuilder currGetPropMthdBldr =
                    typeBuilder.DefineMethod("get_value",
                                               GetSetAttr,
                                               type,
                                               Type.EmptyTypes);

                // Intermediate Language stuff...
                ILGenerator currGetIL = currGetPropMthdBldr.GetILGenerator();
                currGetIL.Emit(OpCodes.Ldarg_0);
                currGetIL.Emit(OpCodes.Ldfld, field);
                currGetIL.Emit(OpCodes.Ret);

                // Define the "set" accessor method for current private field.
                MethodBuilder currSetPropMthdBldr =
                    typeBuilder.DefineMethod("set_value",
                                               GetSetAttr,
                                               null,
                                               new Type[] { type });

                // Again some Intermediate Language stuff...
                ILGenerator currSetIL = currSetPropMthdBldr.GetILGenerator();
                currSetIL.Emit(OpCodes.Ldarg_0);
                currSetIL.Emit(OpCodes.Ldarg_1);
                currSetIL.Emit(OpCodes.Stfld, field);
                currSetIL.Emit(OpCodes.Ret);

                property.SetGetMethod(currGetPropMthdBldr);
                property.SetSetMethod(currSetPropMthdBldr);
            }
            Type generetedType = typeBuilder.CreateType();
            return generetedType;
        }

        public virtual Hashtable Values { get { return fValues; } }

        public virtual object GetRowKey(int rowIndex, int colIndex)
        {
            return string.Format("{0},{1}", rowIndex, colIndex);
        }

        public DataFrame DataF
        {
            get { return _DF; }
            set { _DF = value; }
        }

        public virtual bool UseDataStore
        {
            get { return fUseDataStore; }
            set { fUseDataStore = value; }
        }

        public int RecordCount
        {
            get { return fRecordCount; }///8 ;
            set
            {
                if (value < 1) value = 0;
                if (RecordCount == value) return;
                fRecordCount = value;
            }
        }

        public int ColumnCount
        {
            get { return fColumnCount; }
            set
            {
                if (value < 1) value = 0;
                if (ColumnCount == value) return;
                fColumnCount = value;
                CreateColumnCollection();
            }
        }

        protected virtual void CreateColumnCollection()
        {
            VirtualPropertyDescriptorDynamic[] pds = new VirtualPropertyDescriptorDynamic[ColumnCount];
            for (int n = 0; n < ColumnCount; n++)
            {
                pds[n] = new VirtualPropertyDescriptorDynamic(this, n, GetColumnName(n), typeof(string), false);
            }
            fColumnCollection = new PropertyDescriptorCollection(pds);
        }

        // Renaming this method from GetRowValue to suitable name GelCellValue
        internal object GetCellValue(object row, int colIndex)
        {
            if (row.GetType().Name.Equals("BindableRowCellCollection"))
                return "";
            int fIndex = (int)row;
            if (!UseDataStore) return GetRowKey(fIndex, colIndex);
            UAReturn uar = _service.DataSourceReadCell(_dataSource.Name, fIndex, colIndex);
            XmlNodeList xnl = uar.Data.SelectNodes("/Root/*");
            object obj = xnl.Item(0).InnerXml;

            return obj;

        }

        // Renaming this method from SetRoValue to suitable name SetCellValue
        internal void SetCellValue(object row, int colIndex, object val)
        {

            if (!UseDataStore) return;
            int fIndex = (int)row;
            Values[GetRowKey(fIndex, colIndex)] = val;
            RaiseListChanged(new ListChangedEventArgs(ListChangedType.ItemChanged, fIndex, fIndex));
        }

        internal string[] GetRowDataOld(object row)
        {
            int fIndex = (int)row;
            UAReturn uar = _service.DataSourceReadRow(_dataSource.Name, fIndex);//gets the one row in xml DOM

            XmlNodeList xnl = uar.Data.SelectNodes("/Root/UADoubleMatrix/rows/row/columns/column");
            int datacount = xnl.Count;
            string[] rowdata = new string[datacount];
            for (int i = 0; i < datacount; i++)
            {
                rowdata[i] = xnl.Item(i).InnerXml;
            }

            //For R Dot net only
            if (rowdata.Length != _dataSource.FewVariables.Count)
            {
                rowdata = new string[_dataSource.FewVariables.Count];

                for (int i = 0; i < datacount; i++)
                {
                    rowdata[i] = "Can't get Data"; ;
                }
            }
            return rowdata;
        }

        internal string[] GetRowData(object row)//My function to get single row at once
        {
            int fIndex = (int)row;
            if (fIndex < 0)
                return null; 

            CommandRequest cr = new CommandRequest();
            string rcommand = string.Empty;
            object datobj = null;
            string ds_name = _dataSource.Name;
            bool isDFcellNonNull;
            int colcount = _dataSource.EndColindex - _dataSource.StartColindex +1; 
            string[] rdata = new string[colcount];

            for (int i = _dataSource.StartColindex, j = 0; j < _dataSource.FewVariables.Count; i++, j++)
            {
                if (_DF != null)
                {
                    if (fIndex < _DF.RowCount && i < _DF.ColumnCount)
                    {
                        isDFcellNonNull = false;
                        try
                        {

                            isDFcellNonNull = (_DF[fIndex, i] != null);
                        }
                        catch(Exception dfex)
                        { }

                        if (isDFcellNonNull)
                        {
                            if (!isAtomicValue(fIndex, i))
                            {
                                rdata[j] = "Warning: Non-Atomic value";
                            }
                            else
                            {

                                if (_DF[fIndex, i].ToString() == "NA" ||
                                   _DF[fIndex, i].ToString().Trim().Equals("-2147483648"))
                                {
                                    rdata[j] = "NA";// "<NA>"; 07Oct2019 in integer missing value was<NA> in UI while NA in R. So I replaced <NA> with NA.
                                }
                                else if (_DF[fIndex, i].ToString() == "NaN")
                                {
                                     rdata[j] = "NA"; //old code. code above is new
                                }
                                else
                                {
                                    if ((_dataSource.FewVariables[j].DataClass.Equals("POSIXct")
                                        ))
                                    {
                                        rdata[j] = _DF[fIndex, i].ToString();
                                    }
                                    else if (_dataSource.FewVariables[j].DataClass.Equals("Date"))
                                    {
                                        double _adddays = 0.0;
                                        int adddays = 0;
                                        DateTime begindt = Convert.ToDateTime("01/01/1970");
                                        DateTime dt2 = Convert.ToDateTime("01/01/1970");

                                        if (Double.TryParse(_DF[fIndex, i].ToString(), out _adddays))
                                        {
                                            //08Feb2017 Added logic to fix and exception related to date range
                                            long beginTicks = begindt.Ticks;
                                            long perDayTicks = TimeSpan.TicksPerDay;
                                            long addticks =(long) ( perDayTicks * _adddays);
                                            if((beginTicks+addticks) <= DateTime.MaxValue.Ticks && (beginTicks+addticks) >= DateTime.MinValue.Ticks)
                                                dt2 = begindt.AddDays(_adddays);
                                        }
                                        rdata[j] = dt2.ToString();
                                    }
                                    else if (_dataSource.FewVariables[j].DataClass.Equals("logical"))
                                    {
                                        bool allNA = _dataSource.FewVariables[j].isAllNA;
                                        //if all the values in this col are 'True' then change those to NA
                                        if (allNA)
                                        {
                                            rdata[j] = "<NA>";
                                        }
                                        else
                                            rdata[j] = _DF[fIndex, i].ToString();
                                    }
                                    else
                                    {
                                        rdata[j] = _DF[fIndex, i].ToString();
                                    }

                                    if ((_dataSource.FewVariables[j].DataClass.Equals("POSIXct")))
                                    {
                                        if (rdata[j] != null && rdata[j].Contains('.'))//From R sometimes the seconds has a fraction(.07) part which should be removed for POSIXct.
                                        {
                                            int dotidx = rdata[j].IndexOf('.');
                                            rdata[j] = rdata[j].Substring(0, dotidx);
                                        }
                                    }
                                }
                            }
                        }
                        else 
                        {
                            rdata[j] = "<NA>";
                        }
                    }
                }
                else
                    rdata[j] = "<NA>";
            }
            return rdata;
        }


        private bool isAtomicValue(int dsrowidx, int dscolidx)
        {
            bool isAtomic = true;
            object cellval = _DF[dsrowidx, dscolidx];
            string typename = cellval.GetType().FullName;
            if (typename.Contains("RDotNet"))
            {
                isAtomic = false;
            }
            return isAtomic;
        }

        #region IBindingList Members ( IBindingList : IList (:ICollection (:IEnumerable) ) )


        #region IList Members (IList :ICollection)

        #region ICollection Members ( ICollection : IEnumerable )

        #region IEnumerable Members

        //IEnumberable
        public virtual IEnumerator GetEnumerator()
        {
            return this;
        }
        #endregion

        //ICollection
        public virtual void CopyTo(System.Array array, int fIndex)
        {
        }

        public virtual int Count
        {
            get { return RecordCount; }
        }

        public virtual bool IsSynchronized
        {
            get { return true; }
        }

        public virtual object SyncRoot
        {
            get { return true; }
        }
        #endregion

        //IList

        public virtual bool IsFixedSize
        {
            get { return true; }
        }

        public virtual bool IsReadOnly
        {
            get { return false; }
        }

        object IList.this[int fIndex]
        {
            get
            {
                if (dictIndex.ContainsKey(fIndex))
                    return dictIndex[fIndex];

                // Now we have our type. Let's create an instance from it:
                object generetedObject = Activator.CreateInstance(type);

                // Loop over all the generated properties, and assign the values from our XML:
                PropertyInfo[] properties = type.GetProperties();

                int propertiesCounter = 0;

                bool rowatonce = true; 
                string[] rowdata = null;
                if (rowatonce)
                    rowdata = GetRowData(fIndex); //fetched row
                if (rowdata == null)
                    return null;
                long celldata = 0;
                bool isparsed = false;
                bool ToLocalDateTime = false; //25Aug2017: TRUE: Date will be converted to local date/time
                string dateformat = "yyyy-MM-dd HH:mm:ss"; //25Aug2017 yy-MMM-dd hh:mm:ss zzz

                DateTime dt = new DateTime(1970, 1, 1);
                for (int i = 0; i < _dataSource.FewVariables.Count; ++i)
                {
                    if ((_dataSource.FewVariables[i].DataClass.Equals("POSIXct") 
                        ))
                    {
                        if (rowdata[i] == "<NA>" || rowdata[i] == "NA")
                        {
                            //dt = ToLocalDateTime? new DateTime(0001, 01, 01).ToLocalTime() : new DateTime(0001, 01, 01) ;
                            properties[propertiesCounter].SetValue(generetedObject, "NA", null);
                        }
                        else if (rowdata[i].Contains("AM") || rowdata[i].Contains("PM"))//Date string
                        {
                            dt = ToLocalDateTime ? DateTime.Parse(rowdata[i]).ToLocalTime() : DateTime.Parse(rowdata[i]);
                            properties[propertiesCounter].SetValue(generetedObject, dt.ToString(dateformat), null);
                        }
                        else 
                        {
                            if (!Int64.TryParse(rowdata[i], out celldata))
                            {
                                celldata = 0;
                            }

                            try
                            {
                                //used before 16Feb2020 //dt = ToLocalDateTime ? new DateTime(1970, 1, 1).AddSeconds(celldata).ToLocalTime() : new DateTime(1970, 1, 1).AddSeconds(celldata + _dataSource.FewVariables[i].UTCOffset*3600);//UTC in secs +19800
                                //Excel dates are in UTC=0 if we do ToLocalTime() it will cause it to shift 
                                // So we do not do ToLocalTime() on UTC=0
                                if (_dataSource.FewVariables[i].UTCOffset == 0)
                                {
                                    dt = new DateTime(1970, 1, 1).AddSeconds(celldata + _dataSource.FewVariables[i].UTCOffset * 3600);
                                }
                                else
                                {
                                    dt = new DateTime(1970, 1, 1).AddSeconds(celldata).ToLocalTime();
                                }
                                properties[propertiesCounter].SetValue(generetedObject, dt.ToString(dateformat), null);
                            }
                            catch (Exception ex)
                            {
                                if (ex != null)
                                { }
                            }
                        }
                    }
                    else if ((_dataSource.FewVariables[i].DataClass.Equals("Date"))) //For Date type show only Date part
                    {
                        dateformat = "yyyy-MM-dd";
                        if (rowdata[i] == "<NA>" || rowdata[i] == "NA")
                        {
                            //dt = ToLocalDateTime ? new DateTime(0001, 01, 01).ToLocalTime() : new DateTime(0001, 01, 01);
                            properties[propertiesCounter].SetValue(generetedObject, "NA", null);
                        }
                        else if (rowdata[i].Contains("AM") || rowdata[i].Contains("PM"))//R Date type when read by R.NET(uses DateTime)
                        {
                            dt = ToLocalDateTime ? DateTime.Parse(rowdata[i]).ToLocalTime() : DateTime.Parse(rowdata[i]);
                            properties[propertiesCounter].SetValue(generetedObject, dt.Date.ToString(dateformat), null);
                        }
                        else // Total number of signed seconds
                        {
                            celldata = Convert.ToInt64(rowdata[i]);//, celldata);
                            try
                            {
                                dt = ToLocalDateTime ? new DateTime(1970, 1, 1).AddSeconds(celldata).ToLocalTime() : new DateTime(1970, 1, 1).AddSeconds(celldata);
                                properties[propertiesCounter].SetValue(generetedObject, dt.Date.ToString(dateformat), null);
                            }
                            catch (Exception ex)
                            {
                                if (ex != null)
                                { }
                            }
                        }
                    }
                    else
                    {
                        //01Jul2016
                        //This is codition is added because properties is having 15 elements and rowdat is having 16. 
                        if (i < properties.Length && i<rowdata.Length)
                        {
                            if (rowatonce)
                                properties[propertiesCounter].SetValue(generetedObject, rowdata[i], null); 
                            else
                                properties[propertiesCounter].SetValue(generetedObject, GetCellValue(fIndex, i), null);
                        }
                    }
                    propertiesCounter++;
                }
                dictIndex.Add(fIndex, generetedObject);//A. row(object) is added to dictionary. Key is index    
                dict.Add(generetedObject, fIndex);//A. row is added to dictionary. Key is row(object)
                return generetedObject;
            }
            set { }
        }

        public virtual int Add(object val)//A.
        {
            int lastIndex = Count;
            dict.Add(val, lastIndex);
            dictIndex.Add(lastIndex, val);
            RecordCount += 1;
            return lastIndex;
        }

        public virtual void Clear()
        {
            throw new NotImplementedException();
        }

        public virtual bool Contains(object val)
        {
            return dict.Keys.Contains(val);
        }

        public virtual int IndexOf(object val)
        {
            if (dict.Keys.Contains(val))
                return dict[val];
            else
                return 0;
        }

        public virtual void Insert(int fIndex, object val)//A.
        {
            int lastIndex = Count;
            if (fIndex >= 0 && fIndex < Count)
            {
                dict.Add(val, lastIndex);  
                dictIndex.Add(lastIndex, val);
                RecordCount += 1;
            }

        }

        public virtual void Remove(object val)
        {
            if (dict.Keys.Contains(val))
            {
                foreach (KeyValuePair<object, int> kvpair in dict)
                {
                    if (kvpair.Value.Equals(val))
                    {
                        dict.Remove(kvpair.Key);
                        dictIndex.Remove(kvpair.Value);
                    }
                }
            }
        }

        public virtual void RemoveAt(int fIndex)
        {
            dict.Remove(fIndex);
            dictIndex.Remove(fIndex);
            RecordCount -= 1;
            RaiseListChanged(new ListChangedEventArgs(ListChangedType.ItemDeleted, fIndex, fIndex));
        }

        #endregion


        //properties
        bool IBindingList.AllowEdit { get { return true; } }
        bool IBindingList.AllowNew { get { return true; } }
        bool IBindingList.AllowRemove { get { return true; } }
        bool IBindingList.IsSorted { get { return false; } }
        ListSortDirection IBindingList.SortDirection { get { return ListSortDirection.Ascending; } }
        PropertyDescriptor IBindingList.SortProperty { get { return null; } }
        bool IBindingList.SupportsChangeNotification { get { return true; } }
        bool IBindingList.SupportsSearching { get { return false; } }
        bool IBindingList.SupportsSorting { get { return false; } }

        //methods
        void IBindingList.AddIndex(PropertyDescriptor pd)
        {
            throw new NotImplementedException();
        }

        object IBindingList.AddNew()
        {
            //19Jun2015 May not be needed.++fRecordCount;
            RaiseListChanged(new ListChangedEventArgs(ListChangedType.ItemAdded, RecordCount - 1, -1));
            return RecordCount - 1;
        }

        void IBindingList.ApplySort(PropertyDescriptor pd, ListSortDirection dir)
        {
            throw new NotImplementedException();
        }

        int IBindingList.Find(PropertyDescriptor pd, object key)
        {
            throw new NotImplementedException();
        }

        void IBindingList.RemoveIndex(PropertyDescriptor pd)
        {
            throw new NotImplementedException();
        }

        void IBindingList.RemoveSort()
        {
            throw new NotImplementedException();
        }

        public event ListChangedEventHandler ListChanged
        {
            add { listChangedHandler += value; }
            remove { listChangedHandler -= value; }
        }

        #region virtual methods
        public virtual void AddColumn()
        {
            int cc = ColumnCount;
            ColumnCount++;
            if (cc != ColumnCount)
            {
                RaiseListChanged(new ListChangedEventArgs(ListChangedType.PropertyDescriptorAdded, ColumnCount - 1, -1));
            }
        }

        public virtual string GetColumnName(int columnIndex)
        {
            return _dataSource.FewVariables[columnIndex].Name;
        }

        public virtual void RemoveLastColumn()
        {
            int cc = ColumnCount;
            ColumnCount--;
            if (cc != ColumnCount)
            {
                RaiseListChanged(new ListChangedEventArgs(ListChangedType.PropertyDescriptorDeleted, -1, ColumnCount));
            }
        }

        public virtual void AddNew()
        {
            ((IBindingList)this).AddNew();
        }

        protected virtual void RaiseListChanged(ListChangedEventArgs args)
        {
            if (listChangedHandler != null)
            {
                listChangedHandler(this, args);
            }
        }

        #endregion

        #endregion

        #region ITypedList Interface

        PropertyDescriptorCollection ITypedList.GetItemProperties(PropertyDescriptor[] descs) { return fColumnCollection; }

        string ITypedList.GetListName(PropertyDescriptor[] descs) { return ""; }

        #endregion

        #region IEnumerator Members

        private int index = 0;

        public object Current
        {
            get
            {
                IList lst = this as IList;
                return lst[index];
            }
        }

        public bool MoveNext()
        {
            index++;
            if (index >= this.Count) //21Jan2014 Anil: there are not 10 rows always. old code --> (index == 10)
                return false;
            else
                return true;
        }

        public void Reset()
        {
            index = 0;
        }

        #endregion
    }

}
