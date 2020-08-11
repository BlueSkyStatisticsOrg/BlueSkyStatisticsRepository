using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace BSky.Statistics.Common
{
   

    public static class DateformatConvertor
    {
        public static string DateEnumToString(DateFormatsEnum enumDateFormat)
        {
            
            //mSLdSLy, dSLmSLy, ySLmSLd, ySLdSLm, mDAdDAy, dDAmDAy, yDAmDAd, yDAdDAm
            if (enumDateFormat.ToString() == "NotApplicable")
                return "Not Applicable";
            else if (enumDateFormat.ToString() == "mSLdSLy")
                return "%m/%d/%y";
          
            else if (enumDateFormat.ToString() == "dSLmSLy")
                return "%d/%m/%y";

            else if (enumDateFormat.ToString() == "ySLmSLd")
                return "%y/%m/%d";
            else if (enumDateFormat.ToString() == "ySLdSLm")
                return "%y/%d/%m";

            else if (enumDateFormat.ToString() == "mDAdDAy")
                return "%m-%d-%y";

            else if (enumDateFormat.ToString() == "dDAmDAy")
                return "%d-%m-%y";
            else if (enumDateFormat.ToString() == "yDAmDAd")
                return "%y-%m-%d";

            else if (enumDateFormat.ToString() == "yDAdDAm")
                return "%y-%d-%m";
            if (enumDateFormat.ToString() == "mSLdSLY")
                return "%m/%d/%Y";
            else if (enumDateFormat.ToString() == "dSLmSLY")
                return "%d/%m/%Y";

            else if (enumDateFormat.ToString() == "YSLmSLd")
                return "%Y/%m/%d";
            else if (enumDateFormat.ToString() == "YSLdSLm")
                return "%Y/%d/%m";

            else if (enumDateFormat.ToString() == "mDAdDAY")
                return "%m-%d-%Y";

            else if (enumDateFormat.ToString() == "dDAmDAY")
                return "%d-%m-%Y";
            else if (enumDateFormat.ToString() == "YDAmDAd")
                return "%Y-%m-%d";

            else if (enumDateFormat.ToString() == "YDAdDAm")
                return "%Y-%d-%m";

            else if (enumDateFormat.ToString() == "mSLdSLySPHCOMCOS")
                return "%m/%d/%y %H:%M:%S";
           
            else if (enumDateFormat.ToString() == "dSLmSLySPHCOMCOS")
                return "%d/%m/%y %H:%M:%S";

            else if (enumDateFormat.ToString() == "ySLmSLdSPHCOMCOS")
                return "%y/%m/%d %H:%M:%S";
            else if (enumDateFormat.ToString() == "ySLdSLmSPHCOMCOS")
                return "%y/%d/%m %H:%M:%S";

            else if (enumDateFormat.ToString() == "mDAdDAySPHCOMCOS")
                return "%m-%d-%y %H:%M:%S";

            else if (enumDateFormat.ToString() == "dDAmDAySPHCOMCOS")
                return "%d-%m-%y %H:%M:%S";
            else if (enumDateFormat.ToString() == "yDAmDAdSPHCOMCOS")
                return "%y-%m-%d %H:%M:%S";

            else if (enumDateFormat.ToString() == "yDAdDAmSPHCOMCOS")
                return "%y-%d-%m %H:%M:%S";
            if (enumDateFormat.ToString() == "mSLdSLYSPHCOMCOS")
                return "%m/%d/%Y %H:%M:%S";
            else if (enumDateFormat.ToString() == "dSLmSLYSPHCOMCOS")
                return "%d/%m/%Y %H:%M:%S";

            else if (enumDateFormat.ToString() == "YSLmSLdSPHCOMCOS")
                return "%Y/%m/%d %H:%M:%S";
            else if (enumDateFormat.ToString() == "YSLdSLmSPHCOMCOS")
                return "%Y/%d/%m %H:%M:%S";

            else if (enumDateFormat.ToString() == "mDAdDAYSPHCOMCOS")
                return "%m-%d-%Y %H:%M:%S";

            else if (enumDateFormat.ToString() == "dDAmDAYSPHCOMCOS")
                return "%d-%m-%Y %H:%M:%S";
            else if (enumDateFormat.ToString() == "YDAmDAdSPHCOMCOS")
                return "%Y-%m-%d %H:%M:%S";

            else if (enumDateFormat.ToString() == "YDAdDAmSPHCOMCOS")
                return "%Y-%d-%m %H:%M:%S";


            else return "%m/%d/%y %H:%M:%S";


         

        }

        public static DateFormatsEnum DateStringToEnum(string datestring)
        {
            //mSLdSLy, dSLmSLy, ySLmSLd, ySLdSLm, mDAdDAy, dDAmDAy, yDAmDAd, yDAdDAm
                if (datestring == "Not Applicable")
                    return DateFormatsEnum.NotApplicable;
                if (datestring == "%m/%d/%y")
                    return DateFormatsEnum.mSLdSLy;
                else if (datestring == "%d/%m/%y")
                    return DateFormatsEnum.dSLmSLy;

                else if (datestring == "%y/%m/%d")
                    return DateFormatsEnum.ySLmSLd;
                else if (datestring == "%y/%d/%m")
                    return DateFormatsEnum.ySLdSLm;

                else if (datestring == "%m-%d-%y")
                    return DateFormatsEnum.mDAdDAy;

                else if (datestring == "%d-%m-%y")
                    return DateFormatsEnum.dDAmDAy;
                else if (datestring == "%y-%m-%d")
                    return DateFormatsEnum.yDAmDAd;

                else if (datestring == "%y-%d-%m")
                    return DateFormatsEnum.yDAdDAm;

               else if (datestring == "%m/%d/%Y")
                    return DateFormatsEnum.mSLdSLY;
                else if (datestring == "%d/%m/%Y")
                    return DateFormatsEnum.dSLmSLY;

                else if (datestring == "%Y/%m/%d")
                    return DateFormatsEnum.YSLmSLd;
                else if (datestring == "%Y/%d/%m")
                    return DateFormatsEnum.YSLdSLm;

                else if (datestring == "%m-%d-%Y")
                    return DateFormatsEnum.mDAdDAY;

                else if (datestring == "%d-%m-%Y")
                    return DateFormatsEnum.dDAmDAY;
                else if (datestring == "%Y-%m-%d")
                    return DateFormatsEnum.YDAmDAd;

                else if (datestring == "%Y-%d-%m")
                    return DateFormatsEnum.YDAdDAm;

            else if (datestring == "%m/%d/%y %H:%M:%S")
                return DateFormatsEnum.mSLdSLySPHCOMCOS;
            else if (datestring == "%d/%m/%y %H:%M:%S")
                return DateFormatsEnum.dSLmSLySPHCOMCOS;

            else if (datestring == "%y/%m/%d %H:%M:%S")
                return DateFormatsEnum.ySLmSLdSPHCOMCOS;
            else if (datestring == "%y/%d/%m %H:%M:%S")
                return DateFormatsEnum.ySLdSLmSPHCOMCOS;

            else if (datestring == "%m-%d-%y %H:%M:%S")
                return DateFormatsEnum.mDAdDAySPHCOMCOS;

            else if (datestring == "%d-%m-%y %H:%M:%S")
                return DateFormatsEnum.dDAmDAySPHCOMCOS;
            else if (datestring == "%y-%m-%d %H:%M:%S")
                return DateFormatsEnum.yDAmDAdSPHCOMCOS;

            else if (datestring == "%y-%d-%m %H:%M:%S")
                return DateFormatsEnum.yDAdDAmSPHCOMCOS;

            else if (datestring == "%m/%d/%Y %H:%M:%S")
                return DateFormatsEnum.mSLdSLYSPHCOMCOS;
            else if (datestring == "%d/%m/%Y %H:%M:%S")
                return DateFormatsEnum.dSLmSLYSPHCOMCOS;

            else if (datestring == "%Y/%m/%d %H:%M:%S")
                return DateFormatsEnum.YSLmSLdSPHCOMCOS;
            else if (datestring == "%Y/%d/%m %H:%M:%S")
                return DateFormatsEnum.YSLdSLmSPHCOMCOS;

            else if (datestring == "%m-%d-%Y %H:%M:%S")
                return DateFormatsEnum.mDAdDAYSPHCOMCOS;

            else if (datestring == "%d-%m-%Y %H:%M:%S")
                return DateFormatsEnum.dDAmDAYSPHCOMCOS;
            else if (datestring == "%Y-%m-%d %H:%M:%S")
                return DateFormatsEnum.YDAmDAdSPHCOMCOS;

            else if (datestring == "%Y-%d-%m %H:%M:%S")
                return DateFormatsEnum.YDAdDAmSPHCOMCOS;

            else return DateFormatsEnum.mSLdSLySPHCOMCOS;

        }

        public static string DateFormatsEnumtoCSharpDate(DateFormatsEnum dateEnum)
        {
            string cSharpDateFormat = "";
                if (dateEnum == DateFormatsEnum.NotApplicable)
                        {
                            //DataPanel.getDateFormat

                            cSharpDateFormat = "MM/dd/yyyy HH:mm:ss";
                        }

                        if (dateEnum == DateFormatsEnum.mSLdSLy)
                        {

                            cSharpDateFormat = "MM/dd/y";
                        }
                        if (dateEnum == DateFormatsEnum.dSLmSLy)
                        {

                            cSharpDateFormat = "dd/MM/yy";
                        }
                        if (dateEnum == DateFormatsEnum.ySLmSLd)
                        {

                            cSharpDateFormat = "y/MM/dd";
                        }
                        if (dateEnum == DateFormatsEnum.ySLdSLm)
                        {

                            cSharpDateFormat = "y/dd/MM";
                        }
                        if (dateEnum == DateFormatsEnum.mDAdDAy)
                        {

                            cSharpDateFormat = "MM-dd-y";
                        }
                        if (dateEnum == DateFormatsEnum.dDAmDAy)
                        {

                            cSharpDateFormat = "dd-MM-y";
                        }
                        if (dateEnum == DateFormatsEnum.yDAmDAd)
                        {

                            cSharpDateFormat = "y-MM-dd";
                        }
                        if (dateEnum == DateFormatsEnum.yDAdDAm)
                        {

                            cSharpDateFormat = "y-dd-MM";
                        }

                        //Handling 4 year format

                        if (dateEnum == DateFormatsEnum.mSLdSLY)
                        {

                            cSharpDateFormat = "MM/dd/yyyy";
                        }
                        if (dateEnum == DateFormatsEnum.dSLmSLY)
                        {

                            cSharpDateFormat = "dd/MM/yyyy";
                        }
                        if (dateEnum == DateFormatsEnum.YSLmSLd)
                        {

                            cSharpDateFormat = "yyyy/MM/dd";
                        }
                        if (dateEnum == DateFormatsEnum.YSLdSLm)
                        {

                            cSharpDateFormat = "yyyy/dd/MM";
                        }
                        if (dateEnum == DateFormatsEnum.mDAdDAY)
                        {

                            cSharpDateFormat = "MM-dd-yyyy";
                        }
                        if (dateEnum == DateFormatsEnum.dDAmDAY)
                        {

                            cSharpDateFormat = "dd-MM-yyyy";
                        }
                        if (dateEnum == DateFormatsEnum.YDAmDAd)
                        {

                            cSharpDateFormat = "yyyy-MM-dd";
                        }
                        if (dateEnum == DateFormatsEnum.YDAdDAm)
                        {

                            cSharpDateFormat = "yyyy-dd-MM";
                        }

                      //Added by Aaron 07/25/2020
                      //Handling hour, minute and secs

                        if (dateEnum == DateFormatsEnum.mSLdSLySPHCOMCOS)
                        {

                            cSharpDateFormat = "MM/dd/y HH:mm:ss";
                        }
                        if (dateEnum == DateFormatsEnum.dSLmSLySPHCOMCOS)
                        {

                            cSharpDateFormat = "dd/MM/yy HH:mm:ss";
                        }
                        if (dateEnum == DateFormatsEnum.ySLmSLdSPHCOMCOS)
                        {

                            cSharpDateFormat = "y/MM/dd HH:mm:ss";
                        }
                        if (dateEnum == DateFormatsEnum.ySLdSLmSPHCOMCOS)
                        {

                            cSharpDateFormat = "y/dd/MM HH:mm:ss";
                        }
                        if (dateEnum == DateFormatsEnum.mDAdDAySPHCOMCOS)
                        {

                            cSharpDateFormat = "MM-dd-y HH:mm:ss";
                        }
                        if (dateEnum == DateFormatsEnum.dDAmDAySPHCOMCOS)
                        {

                            cSharpDateFormat = "dd-MM-y HH:mm:ss";
                        }
                        if (dateEnum == DateFormatsEnum.yDAmDAdSPHCOMCOS)
                        {

                            cSharpDateFormat = "y-MM-dd HH:mm:ss";
                        }
                        if (dateEnum == DateFormatsEnum.yDAdDAmSPHCOMCOS)
                        {

                            cSharpDateFormat = "y-dd-MM HH:mm:ss";
                        }

                        //Handling 4 year format

                        if (dateEnum == DateFormatsEnum.mSLdSLYSPHCOMCOS)
                        {

                            cSharpDateFormat = "MM/dd/yyyy HH:mm:ss";
                        }
                        if (dateEnum == DateFormatsEnum.dSLmSLYSPHCOMCOS)
                        {

                            cSharpDateFormat = "dd/MM/yyyy HH:mm:ss";
                        }
                        if (dateEnum == DateFormatsEnum.YSLmSLdSPHCOMCOS)
                        {

                            cSharpDateFormat = "yyyy/MM/dd HH:mm:ss";
                        }
                        if (dateEnum == DateFormatsEnum.YSLdSLmSPHCOMCOS)
                        {

                            cSharpDateFormat = "yyyy/dd/MM HH:mm:ss";
                        }
                        if (dateEnum == DateFormatsEnum.mDAdDAYSPHCOMCOS)
                        {

                            cSharpDateFormat = "MM-dd-yyyy HH:mm:ss";
                        }
                        if (dateEnum == DateFormatsEnum.dDAmDAYSPHCOMCOS)
                        {

                            cSharpDateFormat = "dd-MM-yyyy HH:mm:ss";
                        }
                        if (dateEnum == DateFormatsEnum.YDAmDAdSPHCOMCOS)
                        {

                            cSharpDateFormat = "yyyy-MM-dd HH:mm:ss";
                        }
                        if (dateEnum == DateFormatsEnum.YDAdDAmSPHCOMCOS)
                        {

                            cSharpDateFormat = "yyyy-dd-MM HH:mm:ss";
                        }
            return cSharpDateFormat;
    }


        public static string getDateFormat( string variableClass)
        {
            //Thread.CurrentThread.CurrentCulture.ClearCachedData();
           // SystemLanguage currentLanguage = Application.systemLanguage;
            string culturename = Thread.CurrentThread.CurrentUICulture.Name;
            string s = CultureInfo.CurrentUICulture.Name;
            string ss = CultureInfo.CurrentUICulture.DisplayName;
            int sss = CultureInfo.CurrentUICulture.LCID;
            string tttt = CultureInfo.CurrentUICulture.EnglishName;
            string t = CultureInfo.CurrentCulture.Name;
            string tt = CultureInfo.CurrentCulture.DisplayName;
            int ttt = CultureInfo.CurrentCulture.LCID;
          //  string tttt = CultureInfo.CurrentCulture.EnglishName;
            string region = RegionInfo.CurrentRegion.DisplayName;
           // string lang = 
            DateTimeFormatInfo dtfi = CultureInfo.GetCultureInfo(culturename).DateTimeFormat;
            Type typ = dtfi.GetType();
            PropertyInfo[] props = typ.GetProperties();
            //DateTime value = new DateTime(2012, 5, 28, 11, 35, 0);
            string shortDatePropName = "";
            foreach (var prop in props)
            {
                // Is this a format pattern-related property?
                if (prop.Name.Contains("ShortDatePattern"))
                {
                    string fmt = prop.GetValue(dtfi, null).ToString();
                    //Console.WriteLine("{0,-33} {1} \n{2,-37}Example: {3}\n",
                    //          prop.Name + ":", fmt, "",
                    //        value.ToString(fmt));
                    // shortDatePropName = prop.Name;
                    //shortDatePropName = value.ToString(fmt);
                    shortDatePropName = fmt;
                }
            }
            //Added by Aaron on 07/25/2021
            //This ensures that by default we show hour, minute and seconds in the data grid
            //This is the default display that can be changed in the UI
            if (variableClass == "POSIXct")
            {
                shortDatePropName = shortDatePropName + " HH:mm:ss";
            }
            string dateformat = "";
            if (shortDatePropName == "MM/dd/y")
            {

                dateformat = "%m/%d/%y";
            }
            if (shortDatePropName == "MM/dd/yyyy")
            {

                dateformat = "%m/%d/%Y";
            }
            if (shortDatePropName == "dd/MM/y")
            {

                dateformat = "%d/%m/%y";
            }
            if (shortDatePropName == "dd/MM/yyyy")
            {

                dateformat = "%d/%m/%Y";
            }
            if (shortDatePropName == "y/MM/dd")
            {

                dateformat = "%y/%m/%d";
            }
            if (shortDatePropName == "yyyy/MM/dd")
            {

                dateformat = "%Y/%m/%d";
            }
            if (shortDatePropName == "y/dd/MM")
            {
                dateformat = "%y/%d/%m";

            }
            if (shortDatePropName == "yyyy/dd/MM")
            {
                dateformat = "%Y/%d/%m";

            }
            if (shortDatePropName == "MM-dd-y")
            {
                dateformat = "%m-%d-%y";

            }
            if (shortDatePropName == "MM-dd-yyyy")
            {
                dateformat = "%m-%d-%Y";

            }
            if (shortDatePropName == "dd-MM-y")
            {

                dateformat = "%d-%m-%y";
            }
            if (shortDatePropName == "dd-MM-yyyy")
            {

                dateformat = "%d-%m-%Y";
            }
            if (shortDatePropName == "y-MM-dd")
            {

                dateformat = "%y-%m-%d";
            }
            if (shortDatePropName == "yyyy-MM-dd")
            {

                dateformat = "%Y-%m-%d";
            }
            if (shortDatePropName == "y-dd-MM")
            {
                dateformat = "%y-%d-%m";

            }
            if (shortDatePropName == "yyyy-dd-MM")
            {
                dateformat = "%Y-%d-%m";

            }
            if (shortDatePropName == "M/dd/y")
            {

                dateformat = "%m/%d/%y";
            }
            if (shortDatePropName == "M/dd/yyyy")
            {

                dateformat = "%m/%d/%Y";
            }
            if (shortDatePropName == "dd/M/y")
            {

                dateformat = "%d/%m/%y";
            }
            if (shortDatePropName == "dd/M/yyyy")
            {

                dateformat = "%d/%m/%Y";
            }
            if (shortDatePropName == "y/M/dd")
            {

                dateformat = "%y/%m/%d";
            }
            if (shortDatePropName == "yyyy/M/dd")
            {

                dateformat = "%Y/%m/%d";
            }
            if (shortDatePropName == "y/dd/M")
            {
                dateformat = "%y/%d/%m";

            }
            if (shortDatePropName == "yyyy/dd/M")
            {
                dateformat = "%Y/%d/%m";

            }
            if (shortDatePropName == "M-dd-y")
            {
                dateformat = "%m-%d-%y";

            }
            if (shortDatePropName == "M-dd-yyyy")
            {
                dateformat = "%m-%d-%Y";

            }
            if (shortDatePropName == "dd-M-y")
            {

                dateformat = "%d-%m-%y";
            }
            if (shortDatePropName == "dd-M-yyyy")
            {

                dateformat = "%d-%m-%Y";
            }
            if (shortDatePropName == "y-M-dd")
            {

                dateformat = "%y-%m-%d";
            }
            if (shortDatePropName == "yyyy-M-dd")
            {

                dateformat = "%Y-%m-%d";
            }
            if (shortDatePropName == "y-dd-M")
            {
                dateformat = "%y-%d-%m";

            }
            if (shortDatePropName == "yyyy-dd-M")
            {
                dateformat = "%Y-%d-%m";

            }
            if (shortDatePropName == "MM/d/y")
            {

                dateformat = "%m/%d/%y";
            }
            if (shortDatePropName == "MM/d/yyyy")
            {

                dateformat = "%m/%d/%Y";
            }
            if (shortDatePropName == "d/MM/y")
            {

                dateformat = "%d/%m/%y";
            }
            if (shortDatePropName == "d/MM/yyyy")
            {

                dateformat = "%d/%m/%Y";
            }
            if (shortDatePropName == "y/MM/d")
            {

                dateformat = "%y/%m/%d";
            }
            if (shortDatePropName == "yyyy/MM/d")
            {

                dateformat = "%Y/%m/%d";
            }
            if (shortDatePropName == "y/d/MM")
            {
                dateformat = "%y/%d/%m";

            }
            if (shortDatePropName == "yyyy/d/MM")
            {
                dateformat = "%Y/%d/%m";

            }
            if (shortDatePropName == "MM-d-y")
            {
                dateformat = "%m-%d-%y";

            }
            if (shortDatePropName == "MM-d-yyyy")
            {
                dateformat = "%m-%d-%Y";

            }
            if (shortDatePropName == "d-MM-y")
            {

                dateformat = "%d-%m-%y";
            }
            if (shortDatePropName == "d-MM-yyyy")
            {

                dateformat = "%d-%m-%Y";
            }
            if (shortDatePropName == "y-MM-d")
            {

                dateformat = "%y-%m-%d";
            }
            if (shortDatePropName == "yyyy-MM-d")
            {

                dateformat = "%Y-%m-%d";
            }
            if (shortDatePropName == "y-d-MM")
            {
                dateformat = "%y-%d-%m";

            }
            if (shortDatePropName == "yyyy-d-MM")
            {
                dateformat = "%Y-%d-%m";

            }
            if (shortDatePropName == "M/d/y")
            {

                dateformat = "%m/%d/%y";
            }
            if (shortDatePropName == "M/d/yyyy")
            {

                dateformat = "%m/%d/%Y";
            }
            if (shortDatePropName == "d/M/y")
            {

                dateformat = "%d/%m/%y";
            }
            if (shortDatePropName == "d/M/yyyy")
            {

                dateformat = "%d/%m/%Y";
            }
            if (shortDatePropName == "y/M/d")
            {

                dateformat = "%y/%m/%d";
            }
            if (shortDatePropName == "yyyy/M/d")
            {

                dateformat = "%Y/%m/%d";
            }
            if (shortDatePropName == "y/d/M")
            {
                dateformat = "%y/%d/%m";

            }
            if (shortDatePropName == "yyyy/d/M")
            {
                dateformat = "%Y/%d/%m";

            }
            if (shortDatePropName == "M-d-y")
            {
                dateformat = "%m-%d-%y";

            }
            if (shortDatePropName == "M-d-yyyy")
            {
                dateformat = "%m-%d-%Y";

            }
            if (shortDatePropName == "d-M-y")
            {

                dateformat = "%d-%m-%y";
            }
            if (shortDatePropName == "d-M-yyyy")
            {

                dateformat = "%d-%m-%Y";
            }
            if (shortDatePropName == "y-M-d")
            {

                dateformat = "%y-%m-%d";
            }
            if (shortDatePropName == "yyyy-M-d")
            {

                dateformat = "%Y-%m-%d";
            }
            if (shortDatePropName == "y-d-M")
            {
                dateformat = "%y-%d-%m";

            }
            if (shortDatePropName == "yyyy-d-M")
            {
                dateformat = "%Y-%d-%m";

            }

            if (shortDatePropName == "MM/dd/y HH:mm:ss")
            {

                dateformat = "%m/%d/%y %H:%M:%S";
            }
            if (shortDatePropName == "MM/dd/yyyy HH:mm:ss")
            {

                dateformat = "%m/%d/%Y %H:%M:%S";
            }
            if (shortDatePropName == "dd/MM/y HH:mm:ss")
            {

                dateformat = "%d/%m/%y %H:%M:%S";
            }
            if (shortDatePropName == "dd/MM/yyyy HH:mm:ss")
            {

                dateformat = "%d/%m/%Y %H:%M:%S";
            }
            if (shortDatePropName == "y/MM/dd HH:mm:ss")
            {

                dateformat = "%y/%m/%d %H:%M:%S";
            }
            if (shortDatePropName == "yyyy/MM/dd HH:mm:ss")
            {

                dateformat = "%Y/%m/%d %H:%M:%S";
            }
            if (shortDatePropName == "y/dd/MM HH:mm:ss")
            {
                dateformat = "%y/%d/%m %H:%M:%S";

            }
            if (shortDatePropName == "yyyy/dd/MM HH:mm:ss")
            {
                dateformat = "%Y/%d/%m %H:%M:%S";

            }
            if (shortDatePropName == "MM-dd-y HH:mm:ss")
            {
                dateformat = "%m-%d-%y %H:%M:%S";

            }
            if (shortDatePropName == "MM-dd-yyyy HH:mm:ss")
            {
                dateformat = "%m-%d-%Y %H:%M:%S";

            }
            if (shortDatePropName == "dd-MM-y HH:mm:ss")
            {

                dateformat = "%d-%m-%y %H:%M:%S";
            }
            if (shortDatePropName == "dd-MM-yyyy HH:mm:ss")
            {

                dateformat = "%d-%m-%Y %H:%M:%S";
            }
            if (shortDatePropName == "y-MM-dd HH:mm:ss")
            {

                dateformat = "%y-%m-%d %H:%M:%S";
            }
            if (shortDatePropName == "yyyy-MM-dd HH:mm:ss")
            {

                dateformat = "%Y-%m-%d %H:%M:%S";
            }
            if (shortDatePropName == "y-dd-MM HH:mm:ss")
            {
                dateformat = "%y-%d-%m %H:%M:%S";

            }
            if (shortDatePropName == "yyyy-dd-MM HH:mm:ss")
            {
                dateformat = "%Y-%d-%m %H:%M:%S";

            }
            if (shortDatePropName == "M/dd/y HH:mm:ss")
            {

                dateformat = "%m/%d/%y %H:%M:%S";
            }
            if (shortDatePropName == "M/dd/yyyy HH:mm:ss")
            {

                dateformat = "%m/%d/%Y %H:%M:%S";
            }
            if (shortDatePropName == "dd/M/y HH:mm:ss")
            {

                dateformat = "%d/%m/%y %H:%M:%S";
            }
            if (shortDatePropName == "dd/M/yyyy HH:mm:ss")
            {

                dateformat = "%d/%m/%Y %H:%M:%S";
            }
            if (shortDatePropName == "y/M/dd HH:mm:ss")
            {

                dateformat = "%y/%m/%d %H:%M:%S";
            }
            if (shortDatePropName == "yyyy/M/dd HH:mm:ss")
            {

                dateformat = "%Y/%m/%d %H:%M:%S";
            }
            if (shortDatePropName == "y/dd/M HH:mm:ss")
            {
                dateformat = "%y/%d/%m %H:%M:%S";

            }
            if (shortDatePropName == "yyyy/dd/M HH:mm:ss")
            {
                dateformat = "%Y/%d/%m %H:%M:%S";

            }
            if (shortDatePropName == "M-dd-y HH:mm:ss")
            {
                dateformat = "%m-%d-%y %H:%M:%S";

            }
            if (shortDatePropName == "M-dd-yyyy HH:mm:ss")
            {
                dateformat = "%m-%d-%Y %H:%M:%S";

            }
            if (shortDatePropName == "dd-M-y HH:mm:ss")
            {

                dateformat = "%d-%m-%y %H:%M:%S";
            }
            if (shortDatePropName == "dd-M-yyyy HH:mm:ss")
            {

                dateformat = "%d-%m-%Y %H:%M:%S";
            }
            if (shortDatePropName == "y-M-dd HH:mm:ss")
            {

                dateformat = "%y-%m-%d %H:%M:%S";
            }
            if (shortDatePropName == "yyyy-M-dd HH:mm:ss")
            {

                dateformat = "%Y-%m-%d %H:%M:%S";
            }
            if (shortDatePropName == "y-dd-M HH:mm:ss")
            {
                dateformat = "%y-%d-%m %H:%M:%S";

            }
            if (shortDatePropName == "yyyy-dd-M HH:mm:ss")
            {
                dateformat = "%Y-%d-%m %H:%M:%S";

            }
            if (shortDatePropName == "MM/d/y HH:mm:ss")
            {

                dateformat = "%m/%d/%y %H:%M:%S";
            }
            if (shortDatePropName == "MM/d/yyyy HH:mm:ss")
            {

                dateformat = "%m/%d/%Y %H:%M:%S";
            }
            if (shortDatePropName == "d/MM/y HH:mm:ss")
            {

                dateformat = "%d/%m/%y %H:%M:%S";
            }
            if (shortDatePropName == "d/MM/yyyy HH:mm:ss")
            {

                dateformat = "%d/%m/%Y %H:%M:%S";
            }
            if (shortDatePropName == "y/MM/d HH:mm:ss")
            {

                dateformat = "%y/%m/%d %H:%M:%S";
            }
            if (shortDatePropName == "yyyy/MM/d HH:mm:ss")
            {

                dateformat = "%Y/%m/%d %H:%M:%S";
            }
            if (shortDatePropName == "y/d/MM HH:mm:ss")
            {
                dateformat = "%y/%d/%m %H:%M:%S";

            }
            if (shortDatePropName == "yyyy/d/MM HH:mm:ss")
            {
                dateformat = "%Y/%d/%m %H:%M:%S";

            }
            if (shortDatePropName == "MM-d-y HH:mm:ss")
            {
                dateformat = "%m-%d-%y %H:%M:%S";

            }
            if (shortDatePropName == "MM-d-yyyy HH:mm:ss")
            {
                dateformat = "%m-%d-%Y %H:%M:%S";

            }
            if (shortDatePropName == "d-MM-y HH:mm:ss")
            {

                dateformat = "%d-%m-%y %H:%M:%S";
            }
            if (shortDatePropName == "d-MM-yyyy HH:mm:ss")
            {

                dateformat = "%d-%m-%Y %H:%M:%S";
            }
            if (shortDatePropName == "y-MM-d HH:mm:ss")
            {

                dateformat = "%y-%m-%d %H:%M:%S";
            }
            if (shortDatePropName == "yyyy-MM-d HH:mm:ss")
            {

                dateformat = "%Y-%m-%d %H:%M:%S";
            }
            if (shortDatePropName == "y-d-MM HH:mm:ss")
            {
                dateformat = "%y-%d-%m %H:%M:%S";

            }
            if (shortDatePropName == "yyyy-d-MM HH:mm:ss")
            {
                dateformat = "%Y-%d-%m %H:%M:%S";

            }
            if (shortDatePropName == "M/d/y HH:mm:ss")
            {

                dateformat = "%m/%d/%y %H:%M:%S";
            }
            if (shortDatePropName == "M/d/yyyy HH:mm:ss")
            {

                dateformat = "%m/%d/%Y %H:%M:%S";
            }
            if (shortDatePropName == "d/M/y HH:mm:ss")
            {

                dateformat = "%d/%m/%y %H:%M:%S";
            }
            if (shortDatePropName == "d/M/yyyy HH:mm:ss")
            {

                dateformat = "%d/%m/%Y %H:%M:%S";
            }
            if (shortDatePropName == "y/M/d HH:mm:ss")
            {

                dateformat = "%y/%m/%d %H:%M:%S";
            }
            if (shortDatePropName == "yyyy/M/d HH:mm:ss")
            {

                dateformat = "%Y/%m/%d %H:%M:%S";
            }
            if (shortDatePropName == "y/d/M HH:mm:ss")
            {
                dateformat = "%y/%d/%m %H:%M:%S";

            }
            if (shortDatePropName == "yyyy/d/M HH:mm:ss")
            {
                dateformat = "%Y/%d/%m %H:%M:%S";

            }
            if (shortDatePropName == "M-d-y HH:mm:ss")
            {
                dateformat = "%m-%d-%y %H:%M:%S";

            }
            if (shortDatePropName == "M-d-yyyy HH:mm:ss")
            {
                dateformat = "%m-%d-%Y %H:%M:%S";

            }
            if (shortDatePropName == "d-M-y HH:mm:ss")
            {

                dateformat = "%d-%m-%y %H:%M:%S";
            }
            if (shortDatePropName == "d-M-yyyy HH:mm:ss")
            {

                dateformat = "%d-%m-%Y %H:%M:%S";
            }
            if (shortDatePropName == "y-M-d HH:mm:ss")
            {

                dateformat = "%y-%m-%d %H:%M:%S";
            }
            if (shortDatePropName == "yyyy-M-d HH:mm:ss")
            {

                dateformat = "%Y-%m-%d %H:%M:%S";
            }
            if (shortDatePropName == "y-d-M HH:mm:ss")
            {
                dateformat = "%y-%d-%m %H:%M:%S";

            }
            if (shortDatePropName == "yyyy-d-M HH:mm:ss")
            {
                dateformat = "%Y-%d-%m %H:%M:%S";

            }

            return dateformat;
        }


    }
}
