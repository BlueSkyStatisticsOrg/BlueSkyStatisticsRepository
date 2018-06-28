using System;
using System.Collections.Generic;

namespace BSky.Service.StatisticsEngine
{
    public class Serialization
    {
        
       public static List<object> SerialiseResult(object data)
       {
           if (data is Object[]) return (List<object>)ParseResult(data);

           List<object> list = new List<object>();
           list.Add(ParseResult(data));

           return list;

       }
    
        private static object ParseResult(object data)
        {
            string typeName = data.GetType().Name;

            switch (typeName)
            {
                case "String":
                    return (string)data;
                case "Double":
                    return (double)data;
                case "Int16":
                case "Int32":
                case "Int64":
                    return (int)data;

                case "Int32[]":
                    return data;

                case "Int32[,]":
                case "Double[]":
                case "Double[,]":
                case "String[,]":
                case "String[]":
                    return data;

                case "Object[]":
                    {

                        List<object> list = new List<object>();
                        Object[] rdata = (Object[])data;
                        foreach (Object o in rdata)
                        {
                            list.Add(ParseResult(o));
                        }

                        return list;
                    }

            }
            return data;
        }
  
    }
}
