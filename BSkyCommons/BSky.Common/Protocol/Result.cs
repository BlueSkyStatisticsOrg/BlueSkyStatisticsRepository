using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AnalyticsUnlimited.Statistics.Common
{
    [Serializable]
    public class Result
    {
        public UAError Error { get; set; }

        public bool Success { get; set; }

        public ResultDataItem Data { get; set; }

        public Result() : this(null, new UAError(""), true) { }

        public Result(object resultData) : this(resultData, new UAError(""), true) { }
        public Result(object resultData, bool success) : this(resultData, new UAError(""), success) { }
        public Result(object resultData, UAError error, bool success) { Data = new ResultDataItem(resultData); this.Error = error; this.Success = success; }

    }
    [Serializable]
    public enum UADataType : uint {UAUnKnown, UAInt, UAIntList, UAIntMatrix, UAString, UAStringList, UAStringMatrix, UADouble, UADoubleList, UADoubleMatrix, UAList }
    [Serializable]
    public class ResultDataItem
    {
        public UADataType DataType { get; set; }

        public string StringValue { get; set; }
        public string[] StringList { get; set; }
        public string[,] StringMatrix { get; set; }

        public double DoubleValue { get; set; }
        public double[] DoubleList { get; set; }
        public double[,] DoubleMatrix { get; set; }

        public int IntValue { get; set; }
        public int[] IntList { get; set; }
        public int[,] IntMatrix { get; set; }

        public List<ResultDataItem> Children { get; set; }


        public ResultDataItem() : this(null)
        {
            
        }
        public ResultDataItem(object data)
        {
            this.Children = new List<ResultDataItem>();

            if ( null != data ) Parse(data);
        }
        private void Parse(object data)
        {
            string typeName = data.GetType().Name;

            switch (typeName)
            {
                case "String":
                    StringValue = (string)data;
                    DataType = UADataType.UAString;
                    break;
                case "String[,]":
                    StringMatrix = (string[,])data;
                    DataType = UADataType.UAStringMatrix;
                    break;
                case "String[]":
                    StringList = (string[])data;
                    DataType = UADataType.UAStringList;
                    break;

                case "Double":
                    DoubleValue = (double)data;
                    DataType = UADataType.UADouble;
                    break;
                case "Double[]":
                    DoubleList = (double[])data;
                    DataType = UADataType.UADoubleList;
                    break;
                case "Double[,]":
                    DoubleMatrix = (double[,])data;
                    DataType = UADataType.UADoubleMatrix;
                    break;

                case "Int16":
                case "Int32":
                case "Int64":
                    IntValue = (int)data;
                    DataType = UADataType.UAInt;
                    break;

                case "Int16[]":
                case "Int32[]":
                case "Int64[]":
                    IntList = (int[])data;
                    DataType = UADataType.UAIntList;
                    break;

                case "Int16[,]":
                case "Int32[,]":
                case "Int64[,]":
                    IntMatrix = (int[,])data;
                    DataType = UADataType.UAIntMatrix;
                    break;

                case "Object[]":
                    {
                        Children = new List<ResultDataItem>();
                        Object[] rdata = (Object[])data;
                        foreach (Object o in rdata)
                        {
                            Children.Add(new ResultDataItem(o));
                        }

                        DataType = UADataType.UAList;
                        break;
                    }

            }
        }


        public object RawData
        {
            get{
                switch (this.DataType)
                {
                    case UADataType.UAInt : return this.IntValue;
                    case UADataType.UAIntList : return this.IntList;
                    case UADataType.UAIntMatrix : return this.IntMatrix;
                    case UADataType.UADouble : return this.DoubleValue;
                    case UADataType.UADoubleList : return this.DoubleList;
                    case UADataType.UADoubleMatrix : return this.DoubleMatrix;
                    case UADataType.UAString : return this.StringValue;
                    case UADataType.UAStringList : return this.StringList;
                    case UADataType.UAStringMatrix : return this.StringMatrix;
                    case UADataType.UAList : return this.Children;
                }
                return null;
            }

        }
    }
}
