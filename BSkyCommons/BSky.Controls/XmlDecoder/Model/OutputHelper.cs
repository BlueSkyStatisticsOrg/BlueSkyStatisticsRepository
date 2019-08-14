using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using BSky.Controls;
using BSky.Interfaces.Model;
using BSky.Statistics.Common;
using System.Xml;
using System.Windows;
using BSky.Interfaces.Controls;
using System.Text.RegularExpressions;
using BSky.Controls.XmlDecoder.Model;
using BSky.Controls.Dialogs;
using System.Windows.Input;
using BSky.ConfService.Intf.Interfaces;
using BSky.Lifetime;

namespace BSky.XmlDecoder
{
    /// <summary>
    /// Helper file to evaluate variable names to strings.
    /// </summary>
    public static class OutputHelper
    {
        private static Dictionary<string, object> GlobalList = new Dictionary<string, object>();
        public static int TotalOutputTables { get; set; }

        public static void AddGlobalObject(string macro, object obj)
        {
            if (GlobalList.ContainsKey(macro))
            {
                GlobalList.Remove(macro);
            }
            GlobalList[macro] = obj;
        }

        public static void DeleteGlobalObject(string macro)
        {
            if (GlobalList.ContainsKey(macro))
                GlobalList.Remove(macro);
        }

        public static object GetGlobalMacro(string Macro, string variablename)
        {
            if (!GlobalList.ContainsKey(Macro))
            {
                return null;
            }
            if (string.IsNullOrEmpty(variablename))
            {
                return GlobalList[Macro];
            }
            return EvaluateValue(GlobalList[Macro] as DependencyObject, variablename);
        }

        private static Dictionary<string, string> MacroList;

        public static string ExpandMacro(string Macro)
        {
            if (Macro == null)
                return "";
            if (MacroList.ContainsKey(Macro))
                return MacroList[Macro];
            else
                return Macro;
        }

        public static void UpdateMacro(string Macro, string value)
        {
            MacroList[Macro] = value;
        }

        public static void DeleteMacro(string Macro)
        {
            if (MacroList.ContainsKey(Macro))
                MacroList.Remove(Macro);
        }

        public static string ExpandParam(string datasource, string delimiter)
        {
            string output = string.Empty;

            List<string> lst = OutputHelper.GetList(datasource, string.Empty, false);

            if (lst == null || lst.Count == 0)
            {
                lst = OutputHelper.GetFactors(OutputHelper.ExpandMacro(datasource));
            }
            if (lst != null)
            {
                foreach (string str in lst)
                {
                    output += delimiter + str;
                }
            }
            return output;
        }

        //Added by Aaron 12/11/2013
        //This function is called for every 
        //Added by Aaron 12/11/2013
        //This function is called for every 
        public static string EvaluateValue(DependencyObject obj, string objname) // Group Variable shoul also be added to this function
        {
            //13Sep2013 col name and col object /// Starts
            int paramtype = 0; // 0 -> default, 1-> col name, 2-> col type
            bool datasetnameprefix = false;

            // Modified by Aaron 12/29/2015
            //This to support the use of one control in multiple lines of syntax
            //just like how the target control when used in multi-way anova
            //needs to return var1*var2 with * being the seperator and var1,var2 with , being the seperator //for Anova and plotMeans respectively
            bool forceUseCommas = false;

            //Aaron Commented code below on 10/01/2013
            //Reason is how variables are subsituted is controlled through the subsititue settings dialog
            //  if (objname.IndexOf("#") == 0 && objname.LastIndexOf("#") == (objname.Length - 1)) //its #target#. Col name
            // {
            //     paramtype = 1;
            // }
            // else if (objname.IndexOf("$") == 0 && objname.LastIndexOf("$") == (objname.Length - 1))//its $target$. Col obj
            // {
            //     paramtype = 2;
            // }

            string currentDataset = ExpandMacro("%DATASET%");
            string activemodel = ExpandMacro("%MODEL%");
            //objname = objname.Replace('$', ' ').Replace('#', ' ').Trim();

            //Added by Aaron 12/11/2013
            //In the name of a control, I look for $
            //If found, I ignore the setting of how the value in the control should be constructed and append a dataset name
            //and a $ sign to the variable name
            //This works only for BSKyVariable and BSkyGrouping Variable

            bool boolTilda = false;
            bool overrideSep = false;
            string oriSepCharacter = "";
            string newSepCharacter = "";
            bool prefixEachVariable = false;
            string newPrefixCharacter = "";

            if (objname.Contains('@'))
            {
                //I need objname to get the control from the canvas. Hence I need to remove the $
                objname = objname.Replace('@', ' ').Replace('#', ' ').Trim();
                boolTilda = true;
            }

            if (objname.Contains('&'))
            {
                // oriSubstituteSettings = list.SubstituteSettings;
                // oriSepCharacter = list.SepCharacter;
                int indexofAmbersand = objname.IndexOf("&");
                //  list.SubstituteSettings = list.SubstituteSettings.Replace("UseComma", "UseSeperator").Trim();
                int indexofNewSep = indexofAmbersand + 1;
                // list.SepCharacter = objname[indexofNewSep].ToString();
                newSepCharacter = objname[indexofNewSep].ToString();
                // string stringToReplace = "";
                // stringToReplace = "&" + list.SepCharacter;
                //I need objname to get the control from the canvas. Hence I need to remove the $
                objname = objname.Replace('&', ' ').Replace('#', ' ').Trim();
                objname = objname.Replace(newSepCharacter[0], ' ').Replace('#', ' ').Trim();
                overrideSep = true;
            }


            if (objname.Contains('$'))
            {
                datasetnameprefix = true;
                //I need objname to get the control from the canvas. Hence I need to remove the $
                //07/24/28 Commented the line below and inseted the one below the line below
                // objname = objname.Replace('$', ' ').Replace('#', ' ').Trim();
                objname = objname.Replace('$', ' ').Trim();
            }

            //Added by Aaron 02/11/2014
            //This code was added to support displaying the variables selected in a listbox control in the format
            //gender =dataset$gender, income =dataset$income
            //If a control name is prefixed by !, we will always display variables in that control in the format
            //gender =dataset$gender, income =dataset$income
            bool custDispFormat = false;

            if (objname.Contains('!'))
            {
                custDispFormat = true;
                //I need objname to get the control from the canvas. Hence I need to remove the $
                objname = objname.Replace('!', ' ').Replace('#', ' ').Trim();
            }

            if (objname.Contains('#'))
            {
                forceUseCommas = true;
                //I need objname to get the control from the canvas. Hence I need to remove the $
                objname = objname.Replace('#', ' ').Trim();
            }

            //Added by Aaron 07/27/2018
            //Handling  -var1,-var2 for reshape, see line 707
            //we only have to do this when variable names are not enclosed in "", and when variable names are not prefixed by a dataset 
            if (objname.Contains('^'))
            {
                prefixEachVariable = true;
                //I need objname to get the control from the canvas. Hence I need to remove the $
                int indexofCarrot = objname.IndexOf("^");
                //  list.SubstituteSettings = list.SubstituteSettings.Replace("UseComma", "UseSeperator").Trim();
                int indexofPrefix = indexofCarrot + 1;
                // list.SepCharacter = objname[indexofNewSep].ToString();
                newPrefixCharacter = objname[indexofPrefix].ToString();
                // string stringToReplace = "";
                // stringToReplace = "&" + list.SepCharacter;
                //I need objname to get the control from the canvas. Hence I need to remove the $
                objname = objname.Replace('^', ' ').Replace('#', ' ').Trim();
                objname = objname.Replace(newPrefixCharacter[0], ' ').Replace('#', ' ').Trim();
                prefixEachVariable = true;
            }





            //13Sep2013 col name and col object//// ends

            FrameworkElement fe = obj as FrameworkElement;
            FrameworkElement element = fe.FindName(objname) as FrameworkElement;

            if (element == null)
                return string.Empty;

            IBSkyInputControl ctrl = element as IBSkyInputControl;
            string result = string.Empty;

            if (typeof(BSkyGroupingVariable).IsAssignableFrom(element.GetType()))
            {
                string vals = string.Empty;
                DragDropList list = null;
                BSkyGroupingVariable groupVar = element as BSkyGroupingVariable;

                foreach (object child in groupVar.Children)
                {
                    if (child is SingleItemList)
                        list = child as DragDropList;
                }

                if (list != null)
                {
                    foreach (object o in list.Items)
                    {
                        if (datasetnameprefix == true)
                        {
                            if (list.SubstituteSettings.Contains("Enclosed")) vals += "'" + currentDataset + "$" + o.ToString() + "'";
                            else vals += currentDataset + "$" + o.ToString();
                        }
                        else if (custDispFormat == true)
                        {
                            // if (list.SubstituteSettings.Contains("Enclosed")) vals += "'" + currentDataset + "$" + o.ToString() + "'";
                            vals += o.ToString() + " = " + currentDataset + "$" + o.ToString();
                        }
                        else
                        {
                            if (list.SubstituteSettings.Contains("NoPrefix"))
                            {
                                if (list.SubstituteSettings.Contains("Enclosed")) vals += "'" + o.ToString() + "'";
                                else vals += o.ToString();
                            }
                            //Added by Aaron 02/12/2014
                            //handles the case of var1=dataset$var1
                            else if (list.SubstituteSettings.Contains("CustomFormat"))
                            {
                                if (list.SubstituteSettings.Contains("Enclosed")) vals += "'" + o.ToString() + " = " + currentDataset + "$" + o.ToString() + "'";
                                else vals += o.ToString() + " = " + currentDataset + "$" + o.ToString();
                            }
                            else
                            // Added by Aaron 10/26/2013
                            // This is the case where the prefix is enabled
                            {
                                if (list.SubstituteSettings.Contains("Enclosed")) vals += "'" + currentDataset + "$" + o.ToString() + "'";
                                else vals += currentDataset + "$" + o.ToString();
                            }
                        }
                        if (list.SubstituteSettings.Contains("UsePlus")) vals += "+";
                        if (list.SubstituteSettings.Contains("UseComma")) vals += ",";
                        if (list.SubstituteSettings.Contains("UseSeperator")) vals += list.SepCharacter;
                    }
                    if (list.SubstituteSettings.Contains("UseComma")) vals = vals.TrimEnd(',');
                    if (list.SubstituteSettings.Contains("UsePlus")) vals = vals.TrimEnd('+');
                    if (list.SubstituteSettings.Contains("UseSeperator"))
                    {
                        if (list.SepCharacter != null)
                        {
                            if (list.SepCharacter != string.Empty)
                            {
                                char charsToTrim = list.SepCharacter[0];
                                vals = vals.TrimEnd(list.SepCharacter.ToCharArray());
                                //  vals = vals.TrimEnd(charsToTrim);
                                //  vals.TrimEnd(list.SepCharacter as chars[]);
                            }
                        }
                    }
                }

                //if (ctrl.Syntax == "%%VALUE%%")
                //{
                //    vals = ctrl.Syntax.Replace("%%VALUE%%", vals);
                //    return vals;
                //}
                //else return ctrl.Syntax;

                if (vals == string.Empty | vals == null)
                {
                    if (ctrl.Syntax == "%%VALUE%%")
                    {
                        //vals = txt.Text;
                        result = ctrl.Syntax.Replace("%%VALUE%%", string.Empty);
                    }
                    else
                        result = ctrl.Syntax;

                    return result;
                }
                else
                {
                    if (list.SubstituteSettings.Contains("CreateArray")) vals = "c(" + vals + ")";
                    if (list.SubstituteSettings.Contains("Brackets")) vals = "(" + vals + ")";
                    if (list.PrefixTxt != string.Empty)
                    {
                        vals = list.PrefixTxt + vals;
                    }

                    //09Jul2015
                    //if (ctrl.Syntax == "%%VALUE%%")
                    //{
                    //    vals = ctrl.Syntax.Replace("%%VALUE%%", vals);
                    //    return vals;
                    //}
                    //else return ctrl.Syntax;

                    return vals;
                }

            }

            else if (typeof(BSkyAggregateCtrl).IsAssignableFrom(element.GetType()))
            {
                // datasetnameprefix = true;
                string vals = string.Empty;
                DragDropListForSummarize list = null;
                BSkyAggregateCtrl groupVar = element as BSkyAggregateCtrl;
                bool displayCounts = false;
                CheckBox temp = null;
                TextBox tb = null;
                string[] tokens = null;
                int nooftokens = 0;
                int iterator = 0;

                foreach (object child in groupVar.Children)
                {
                    if (child is DragDropListForSummarize)
                        list = child as DragDropListForSummarize;
                    if (child is CheckBox)
                    {
                        temp = child as CheckBox;
                        displayCounts = temp.IsChecked.Value;
                    }
                    if (child is TextBox)
                    {
                        tb = child as TextBox;
                        if (tb.Text != "")
                            tokens = tb.Text.Split(',');
                    }

                }
                if (displayCounts) iterator = 1;
                else iterator = 0;
                if (tokens == null)
                    nooftokens = 0;
                else
                    nooftokens = tokens.Length;
                // list.SubstituteSettings = "NoPrefix|UseComma|StringPrefix|Brackets";
                // list.PrefixTxt = "%>% summarize";
                if (list != null)
                {
                    foreach (DataSourceVariable o in list.Items)
                    {
                        if (datasetnameprefix == true)
                        {
                            if (list.SubstituteSettings.Contains("Enclosed")) vals += "'" + currentDataset + "$" + o.ToString() + "'";
                            else vals += currentDataset + "$" + o.ToString();
                        }
                        else if (custDispFormat == true)
                        {
                            // if (list.SubstituteSettings.Contains("Enclosed")) vals += "'" + currentDataset + "$" + o.ToString() + "'";
                            vals += o.ToString() + " = " + currentDataset + "$" + o.ToString();
                        }
                        else
                        {
                            if (list.SubstituteSettings.Contains("NoPrefix"))
                            {
                                if (list.SubstituteSettings.Contains("Enclosed")) vals += "'" + o.ToString() + "'";
                                else
                                {
                                    if (nooftokens > 0)
                                    {
                                        if (iterator < nooftokens)
                                        {
                                            //na.rm=TRUE should not be applied to n_distinct
                                            if (!o.XName.Contains("n_distinct"))
                                            {
                                                vals = vals + tokens[iterator] + "=" + o.XName.Replace(")", ",na.rm =TRUE)");
                                            }
                                            else
                                            {
                                                vals = vals + tokens[iterator] + "=" + o.XName;
                                            }
                                            iterator = iterator + 1;
                                        }
                                        //Case where iterator == nooftokens as all tokens are used
                                        //We create a default variable name e.g. mean_mpg instead of the default mean(mpg)
                                        else
                                        {
                                            //na.rm=TRUE should not be applied to n_distinct
                                            if (!o.XName.Contains("n_distinct"))
                                            {
                                                vals += o.XName.Replace("(", "_").Replace(")", "") + "=" + o.XName.Replace(")", ",na.rm =TRUE)");
                                            }
                                            else
                                            {
                                                vals += o.XName.Replace("(", "_").Replace(")", "") + "=" + o.XName;
                                            }
                                        }
                                    }
                                    //No tokens specified

                                    else
                                    {
                                        //na.rm=TRUE should not be applied to n_distinct
                                        if (!o.XName.Contains("n_distinct"))
                                        {
                                            //We create a default variable name e.g. mean_mpg instead of the default mean(mpg)
                                            vals += o.XName.Replace("(", "_").Replace(")", "") + "=" + o.XName.Replace(")", ",na.rm =TRUE)");
                                        }
                                        else
                                        {
                                            vals += o.XName.Replace("(", "_").Replace(")", "") + "=" + o.XName;
                                        }
                                    }
                                }
                            }
                            //Added by Aaron 02/12/2014
                            //handles the case of var1=dataset$var1
                            else if (list.SubstituteSettings.Contains("CustomFormat"))
                            {
                                if (list.SubstituteSettings.Contains("Enclosed")) vals += "'" + o.ToString() + " = " + currentDataset + "$" + o.ToString() + "'";
                                else vals += o.ToString() + " = " + currentDataset + "$" + o.ToString();
                            }
                            else
                            // Added by Aaron 10/26/2013
                            // This is the case where the prefix is enabled
                            {
                                if (list.SubstituteSettings.Contains("Enclosed")) vals += "'" + currentDataset + "$" + o.ToString() + "'";
                                else vals += currentDataset + "$" + o.ToString();
                            }
                        }
                        if (list.SubstituteSettings.Contains("UsePlus")) vals += "+";
                        if (list.SubstituteSettings.Contains("UseComma")) vals += ",";
                        if (list.SubstituteSettings.Contains("UseSeperator")) vals += list.SepCharacter;
                    }
                    if (list.SubstituteSettings.Contains("UseComma")) vals = vals.TrimEnd(',');
                    if (list.SubstituteSettings.Contains("UsePlus")) vals = vals.TrimEnd('+');
                    if (list.SubstituteSettings.Contains("UseSeperator"))
                    {
                        if (list.SepCharacter != null)
                        {
                            if (list.SepCharacter != string.Empty)
                            {
                                char charsToTrim = list.SepCharacter[0];
                                vals = vals.TrimEnd(list.SepCharacter.ToCharArray());
                                //  vals = vals.TrimEnd(charsToTrim);
                                //  vals.TrimEnd(list.SepCharacter as chars[]);
                            }
                        }
                    }
                }

                //if (ctrl.Syntax == "%%VALUE%%")
                //{
                //    vals = ctrl.Syntax.Replace("%%VALUE%%", vals);
                //    return vals;
                //}
                //else return ctrl.Syntax;

                //02Oct2016 following 'if' is commented and the 'else' block is used but 'else' is commented.
                // This is done because target box may or may not have any value. And in both th cases it should 
                // execute the 'else' block (without 'else' keyword) below.
                ////if (vals == string.Empty | vals == null)
                ////{
                ////    if (ctrl.Syntax == "%%VALUE%%")
                ////    {
                ////        vals = txt.Text;
                ////        result = ctrl.Syntax.Replace("%%VALUE%%", string.Empty);
                ////    }
                ////    else
                ////        result = ctrl.Syntax;

                ////    return result;
                ////}
                ////else
                {
                    if (displayCounts)
                    {
                        if (nooftokens > 0)
                            vals = tokens[0] + "=" + "n(), " + vals;
                        else
                            vals = "Count" + "=" + "n(), " + vals;
                    }
                    if (list.SubstituteSettings.Contains("CreateArray")) vals = "c(" + vals + ")";

                    if (list.SubstituteSettings.Contains("Brackets")) vals = "(" + vals + ")";
                    if (list.PrefixTxt != string.Empty)
                    {
                        vals = list.PrefixTxt + vals;
                    }

                    //09Jul2015
                    //if (ctrl.Syntax == "%%VALUE%%")
                    //{
                    //    vals = ctrl.Syntax.Replace("%%VALUE%%", vals);
                    //    return vals;
                    //}
                    //else return ctrl.Syntax;

                    return vals;
                }

            }

            else if (typeof(BSkySortCtrl).IsAssignableFrom(element.GetType()))
            {
                // datasetnameprefix = true;
                string vals = string.Empty;
                DragDropListForSummarize list = null;
                BSkySortCtrl groupVar = element as BSkySortCtrl;

                foreach (object child in groupVar.Children)
                {
                    if (child is DragDropListForSummarize)
                        list = child as DragDropListForSummarize;
                }
                // list.SubstituteSettings = "NoPrefix|UseComma|StringPrefix|Brackets";
                // list.PrefixTxt = "%>% summarize";
                if (list != null)
                {
                    foreach (DataSourceVariable o in list.Items)
                    {
                        if (datasetnameprefix == true)
                        {
                            if (list.SubstituteSettings.Contains("Enclosed")) vals += "'" + currentDataset + "$" + o.ToString() + "'";
                            else vals += currentDataset + "$" + o.ToString();
                        }
                        else if (custDispFormat == true)
                        {
                            // if (list.SubstituteSettings.Contains("Enclosed")) vals += "'" + currentDataset + "$" + o.ToString() + "'";
                            vals += o.ToString() + " = " + currentDataset + "$" + o.ToString();
                        }
                        else
                        {
                            if (list.SubstituteSettings.Contains("NoPrefix"))
                            {
                                if (list.SubstituteSettings.Contains("Enclosed")) vals += "'" + o.ToString() + "'";
                                else
                                {
                                    vals = vals + o.XName;
                                }
                            }
                            //Added by Aaron 02/12/2014
                            //handles the case of var1=dataset$var1
                            else if (list.SubstituteSettings.Contains("CustomFormat"))
                            {
                                if (list.SubstituteSettings.Contains("Enclosed")) vals += "'" + o.ToString() + " = " + currentDataset + "$" + o.ToString() + "'";
                                else vals += o.ToString() + " = " + currentDataset + "$" + o.ToString();
                            }
                            else
                            // Added by Aaron 10/26/2013
                            // This is the case where the prefix is enabled
                            {
                                if (list.SubstituteSettings.Contains("Enclosed")) vals += "'" + currentDataset + "$" + o.ToString() + "'";
                                else vals += currentDataset + "$" + o.ToString();
                            }
                        }
                        if (list.SubstituteSettings.Contains("UsePlus")) vals += "+";
                        if (list.SubstituteSettings.Contains("UseComma")) vals += ",";
                        if (list.SubstituteSettings.Contains("UseSeperator")) vals += list.SepCharacter;
                    }
                    if (list.SubstituteSettings.Contains("UseComma")) vals = vals.TrimEnd(',');
                    if (list.SubstituteSettings.Contains("UsePlus")) vals = vals.TrimEnd('+');
                    if (list.SubstituteSettings.Contains("UseSeperator"))
                    {
                        if (list.SepCharacter != null)
                        {
                            if (list.SepCharacter != string.Empty)
                            {
                                char charsToTrim = list.SepCharacter[0];
                                vals = vals.TrimEnd(list.SepCharacter.ToCharArray());
                                //  vals = vals.TrimEnd(charsToTrim);
                                //  vals.TrimEnd(list.SepCharacter as chars[]);
                            }
                        }
                    }
                }

                //if (ctrl.Syntax == "%%VALUE%%")
                //{
                //    vals = ctrl.Syntax.Replace("%%VALUE%%", vals);
                //    return vals;
                //}
                //else return ctrl.Syntax;

                if (vals == string.Empty | vals == null)
                {
                    if (ctrl.Syntax == "%%VALUE%%")
                    {
                        //vals = txt.Text;
                        result = ctrl.Syntax.Replace("%%VALUE%%", string.Empty);
                    }
                    else
                        result = ctrl.Syntax;

                    return result;
                }
                else
                {
                    if (list.SubstituteSettings.Contains("CreateArray")) vals = "c(" + vals + ")";

                    if (list.SubstituteSettings.Contains("Brackets")) vals = "(" + vals + ")";
                    if (list.PrefixTxt != string.Empty)
                    {
                        vals = list.PrefixTxt + vals;
                    }

                    //09Jul2015
                    //if (ctrl.Syntax == "%%VALUE%%")
                    //{
                    //    vals = ctrl.Syntax.Replace("%%VALUE%%", vals);
                    //    return vals;
                    //}
                    //else return ctrl.Syntax;

                    return vals;
                }

            }

            //Added by Aaron 12/27/2013
            //Changed typeof(ListBox) to type of BSkyVariableList
            //Added by Aaron 05/29/2014
            //changed typeof(BSkyVariableList).IsAssignableFrom(element.GetType()) to line below
            else if (typeof(BSkySourceList).IsAssignableFrom(element.GetType()) || typeof(BSkyTargetList).IsAssignableFrom(element.GetType())) // mostly colname should be in listbox rather than any other control
            {

                string vals = string.Empty;
                DragDropList list = element as DragDropList;
                string oriSubstituteSettings = "";
                oriSubstituteSettings = list.SubstituteSettings;


                if (boolTilda)
                {
                    oriSubstituteSettings = list.SubstituteSettings;
                    list.SubstituteSettings = "NoPrefix|UseComma|Enclosed";

                }

                if (overrideSep)
                {
                    oriSubstituteSettings = list.SubstituteSettings;
                    oriSepCharacter = list.SepCharacter;
                    list.SubstituteSettings = list.SubstituteSettings.Replace("UseComma", "UseSeperator").Trim();
                    list.SepCharacter = newSepCharacter;

                }

                if (list != null)
                {
                    foreach (object o in list.Items)
                    {
                        //       if(paramtype==1 || paramtype==0 ) //13Sep2013 if-else added. old logic had just one line which is under 'if'
                        //         vals += "'" + o.ToString() + "',";
                        //   else if(paramtype==2)
                        //     vals += currentDataset+"$" + o.ToString() + ",";

                        if (datasetnameprefix == true)
                        {
                            if (list.SubstituteSettings.Contains("Enclosed")) vals += "'" + currentDataset + "$" + o.ToString() + "'";
                            else vals += currentDataset + "$" + o.ToString();
                        }
                        else if (custDispFormat == true)
                        {
                            // if (list.SubstituteSettings.Contains("Enclosed")) vals += "'" + currentDataset + "$" + o.ToString() + "'";
                            vals += o.ToString() + " = " + currentDataset + "$" + o.ToString();
                        }
                        else
                        {
                            if (list.SubstituteSettings.Contains("NoPrefix"))
                            {
                                if (list.SubstituteSettings.Contains("Enclosed")) vals += "'" + o.ToString() + "'";
                                else
                                {
                                    //Added by Aaron 07/27/2018
                                    //This is the only place where I need to pass -var1,-var2
                                    if (!prefixEachVariable)
                                        vals += o.ToString();
                                    else vals = vals + newPrefixCharacter + o.ToString();
                                }
                            }
                            //Added by Aaron 02/12/2014
                            //handles the case of var1=dataset$var1
                            else if (list.SubstituteSettings.Contains("CustomFormat"))
                            {
                                if (list.SubstituteSettings.Contains("Enclosed")) vals += "'" + o.ToString() + " = " + currentDataset + "$" + o.ToString() + "'";
                                else vals += o.ToString() + " = " + currentDataset + "$" + o.ToString();
                            }
                            else
                            // Added by Aaron 10/26/2013
                            // This is the case where the prefix is enabled
                            {
                                if (list.SubstituteSettings.Contains("Enclosed")) vals += "'" + currentDataset + "$" + o.ToString() + "'";
                                else vals += currentDataset + "$" + o.ToString();
                            }
                        }

                        if (list.SubstituteSettings.Contains("UsePlus")) vals += "+";
                        if (list.SubstituteSettings.Contains("UseComma")) vals += ",";
                        //if (list.SubstituteSettings.Contains("UseSeperator")) vals += list.SepCharacter;
                        // Modified by Aaron 12/29/2015
                        //This to support the use of one control in multiple lines of syntax
                        //just like how the target control when used in multi-way anova
                        //needs to return var1*var2 with * being the seperator and var1,var2 with , being the seperator //for Anova and plotMeans respectively

                        if (list.SubstituteSettings.Contains("UseSeperator"))
                        {
                            if (forceUseCommas == true)
                            {
                                vals += ",";
                            }
                            else

                                vals += list.SepCharacter;
                        }
                    }
                    // vals = vals.TrimEnd(',');
                    if (list.SubstituteSettings.Contains("UseComma")) vals = vals.TrimEnd(',');
                    if (list.SubstituteSettings.Contains("UsePlus")) vals = vals.TrimEnd('+');
                    if (list.SubstituteSettings.Contains("UseSeperator"))
                    {
                        //if (list.SepCharacter != null)
                        //{
                        //    if (list.SepCharacter != string.Empty)
                        //    {
                        //        char charsToTrim = list.SepCharacter[0];
                        //        vals = vals.TrimEnd(list.SepCharacter.ToCharArray());
                        //        //  vals = vals.TrimEnd(charsToTrim);
                        //        //  vals.TrimEnd(list.SepCharacter as chars[]);
                        //    }
                        //}
                        if (list.SepCharacter != null)
                        {
                            // Modified by Aaron 12/29/2015
                            //This to support the use of one control in multiple lines of syntax
                            //just like how the target control when used in multi-way anova
                            //needs to return var1*var2 with * being the seperator and var1,var2 with , being the seperator //for Anova and plotMeans respectively

                            if (forceUseCommas == true)
                            {
                                vals = vals.TrimEnd('+');
                            }
                            else if (list.SepCharacter != string.Empty)
                            {
                                char charsToTrim = list.SepCharacter[0];
                                vals = vals.TrimEnd(list.SepCharacter.ToCharArray());
                                //  vals = vals.TrimEnd(charsToTrim);
                                //  vals.TrimEnd(list.SepCharacter as chars[]);
                            }
                        }
                    }
                }



                if (overrideSep == true)
                {
                    list.SubstituteSettings = oriSubstituteSettings;
                    list.SepCharacter = oriSepCharacter;
                }

                if (boolTilda == true)
                {
                    list.SubstituteSettings = oriSubstituteSettings;

                }



                if (vals == string.Empty | vals == null)
                {
                    if (ctrl.Syntax == "%%VALUE%%")
                    {
                        //vals = txt.Text;
                        result = ctrl.Syntax.Replace("%%VALUE%%", vals);
                    }
                    else
                        result = ctrl.Syntax;

                    return result;
                }
                else
                {
                    if (list.SubstituteSettings.Contains("CreateArray")) vals = "c(" + vals + ")";
                    if (list.SubstituteSettings.Contains("Brackets")) vals = "(" + vals + ")";
                    if (list.PrefixTxt != string.Empty)
                    {
                        vals = list.PrefixTxt + vals;
                    }

                    if (ctrl.Syntax == "%%VALUE%%")
                    {
                        vals = ctrl.Syntax.Replace("%%VALUE%%", vals);
                        return vals;
                    }
                    else return ctrl.Syntax;
                }

            }

            if (typeof(BSkyListBoxwBorderForDatasets).IsAssignableFrom(element.GetType())) // mostly colname should be in listbox rather than any other control
            {
                string vals = string.Empty;
                BSkyListBoxwBorderForDatasets list = element as BSkyListBoxwBorderForDatasets;

                if (list != null)
                {
                    foreach (DatasetDisplay o in list.Items)
                    {
                        //       if(paramtype==1 || paramtype==0 ) //13Sep2013 if-else added. old logic had just one line which is under 'if'
                        //         vals += "'" + o.ToString() + "',";
                        //   else if(paramtype==2)
                        //     vals += currentDataset+"$" + o.ToString() + ",";

                        //if (datasetnameprefix == true)
                        //{
                        //    if (list.SubstituteSettings.Contains("Enclosed")) vals += "'" + currentDataset + "$" + o.ToString() + "'";
                        //    else vals += currentDataset + "$" + o.ToString();
                        //}
                        //else if (custDispFormat == true)
                        //{
                        //    // if (list.SubstituteSettings.Contains("Enclosed")) vals += "'" + currentDataset + "$" + o.ToString() + "'";
                        //    vals += o.ToString() + " = " + currentDataset + "$" + o.ToString();
                        //}

                        if (list.SubstituteSettings.Contains("Enclosed")) vals += "'" + o.Name + "'";
                        else vals += o.Name;

                        //Added by Aaron 02/12/2014
                        //handles the case of var1=dataset$var1
                        //else if (list.SubstituteSettings.Contains("CustomFormat"))
                        //{
                        //    if (list.SubstituteSettings.Contains("Enclosed")) vals += "'" + o.ToString() + " = " + currentDataset + "$" + o.ToString() + "'";
                        //    else vals += o.ToString() + " = " + currentDataset + "$" + o.ToString();
                        //}
                        //else
                        //// Added by Aaron 10/26/2013
                        //// This is the case where the prefix is enabled
                        //{
                        //    if (list.SubstituteSettings.Contains("Enclosed")) vals += "'" + currentDataset + "$" + o.ToString() + "'";
                        //    else vals += currentDataset + "$" + o.ToString();
                        //}

                        if (list.SubstituteSettings.Contains("UsePlus")) vals += "+";
                        if (list.SubstituteSettings.Contains("UseComma")) vals += ",";
                        if (list.SubstituteSettings.Contains("UseSeperator")) vals += list.SepCharacter;
                    }
                    // vals = vals.TrimEnd(',');
                    if (list.SubstituteSettings.Contains("UseComma")) vals = vals.TrimEnd(',');
                    if (list.SubstituteSettings.Contains("UsePlus")) vals = vals.TrimEnd('+');
                    if (list.SubstituteSettings.Contains("UseSeperator"))
                    {
                        if (list.SepCharacter != null)
                        {
                            if (list.SepCharacter != string.Empty)
                            {
                                char charsToTrim = list.SepCharacter[0];
                                vals = vals.TrimEnd(list.SepCharacter.ToCharArray());
                                //  vals = vals.TrimEnd(charsToTrim);
                                //  vals.TrimEnd(list.SepCharacter as chars[]);
                            }
                        }
                    }
                }

                if (vals == string.Empty | vals == null)
                {
                    if (ctrl.Syntax == "%%VALUE%%")
                    {
                        //vals = txt.Text;
                        result = ctrl.Syntax.Replace("%%VALUE%%", vals);
                    }
                    else
                        result = ctrl.Syntax;

                    return result;
                }
                else
                {
                    if (list.SubstituteSettings.Contains("CreateArray")) vals = "c(" + vals + ")";
                    if (list.PrefixTxt != string.Empty)
                    {
                        vals = list.PrefixTxt + vals;
                    }

                    if (ctrl.Syntax == "%%VALUE%%")
                    {
                        vals = ctrl.Syntax.Replace("%%VALUE%%", vals);
                        return vals;
                    }
                    else return ctrl.Syntax;
                }

            }
            // Added12/27/2013 by Aaron
            //Added code below to support BSkyListBox
            //else if ((typeof(ListBox).IsAssignableFrom(element.GetType())))
            //{
            //    ListBox lst = element as ListBox;
            //    string vals = string.Empty;
            //    foreach (string str in lst.SelectedItems)
            //        vals = vals + str+",";
            //    vals = vals.TrimEnd(',');
            //    result = ctrl.Syntax.Replace("%%VALUE%%", vals);
            //    return result;
            //}

            else if (typeof(BSkyListBox).IsAssignableFrom(element.GetType()) || typeof(BSkyMasterListBox).IsAssignableFrom(element.GetType())) // mostly colname should be in listbox rather than any other control
            {
                string vals = string.Empty;
                CtrlListBox list = element as CtrlListBox;

                if (list != null)
                {
                    foreach (object o in list.SelectedItems)
                    {
                        //       if(paramtype==1 || paramtype==0 ) //13Sep2013 if-else added. old logic had just one line which is under 'if'
                        //         vals += "'" + o.ToString() + "',";
                        //   else if(paramtype==2)
                        //     vals += currentDataset+"$" + o.ToString() + ",";

                        if (list.SubstituteSettings.Contains("Enclosed")) vals += "'" + o.ToString() + "'";
                        else vals += o.ToString();

                        //Added by Aaron 02/12/2014
                        //handles the case of var1=dataset$var1
                        if (list.SubstituteSettings.Contains("UsePlus")) vals += "+";
                        if (list.SubstituteSettings.Contains("UseComma")) vals += ",";
                    }
                    // vals = vals.TrimEnd(',');
                    if (list.SubstituteSettings.Contains("UseComma")) vals = vals.TrimEnd(',');
                    if (list.SubstituteSettings.Contains("UsePlus")) vals = vals.TrimEnd('+');
                }
                if (ctrl.Syntax == "%%VALUE%%")
                {
                    vals = ctrl.Syntax.Replace("%%VALUE%%", vals);
                    return vals;
                }
                else return ctrl.Syntax;
            }

            //07/13/2014
            //Aaron
            //Added support for the following use cases
            //1. an option portion of the syntax subset=var1>60|var2=10
            //In above case if nothing is entered in textbox, the optional part of syntax is not created
            //2. Added support for an optional control that creates a character array of all the labels that will be used
            //to generate values for a binned variable. If 3 bins are chosen and the labels of the bins are 20-30,30-40,40-50, then we create a array as c("20-30","30-40","40-50")
            else if (typeof(BSkyTextBox).IsAssignableFrom(element.GetType()))
            {
                BSkyTextBox txt = element as BSkyTextBox;
                string vals = string.Empty;

                //Added by Aaron 09/07/2014
                //Added this code to amke sure that an empty string is returned if the textbox is disabled
                if (txt.Enabled == false)
                {
                    if (ctrl.Syntax == "%%VALUE%%")
                    {
                        vals = "";
                        result = ctrl.Syntax.Replace("%%VALUE%%", vals);
                    }
                    else
                        result = ctrl.Syntax;

                    return result;
                }

                currentDataset = ExpandMacro("%DATASET%");
                activemodel = ExpandMacro("%MODEL%");
                //Aaron 07/12/2014
                // This line ensures that the prefix text gets returned only when a valid value  is entered in the textbox
                //This ensures that an empty string retured when text is empty
                //This allows you to create an optional syntax sub-string like subset=var1>10
                if (txt.Text == string.Empty | txt.Text == null)
                {
                    if (ctrl.Syntax == "%%VALUE%%")
                    {
                        vals = txt.Text;
                        result = ctrl.Syntax.Replace("%%VALUE%%", vals);
                    }
                    else
                        result = ctrl.Syntax;

                    return result;
                }
                if (txt.SubstituteSettings != null)
                {
                    if (txt.SubstituteSettings.Contains("TextAsIs"))
                    {
                        //vals += currentDataset + "$" + o.ToString();
                        vals += txt.Text;
                    }
                    if (txt.SubstituteSettings.Contains("PrefixByDatasetName"))
                    {
                        //vals += currentDataset + "$" + o.ToString();
                        vals += currentDataset + "$" + txt.Text;
                    }
                    else if (txt.SubstituteSettings.Contains("CreateArray"))
                    {
                        string[] strs = txt.Text.Split(',');

                        for (int i = 0; i < strs.Length; i++)
                        {
                            strs[i] = "'" + strs[i] + "'";
                        }
                        vals = "c(" + string.Join(",", strs) + ")";
                    }
                    if (txt.SubstituteSettings.Contains("Brackets")) vals = "(" + vals + ")";
                    //having the if below allows you to create a prefix for an enclosed string
                    if (txt.SubstituteSettings.Contains("PrefixByString"))
                    {
                        // vals += txt.PrefixTxt + txt.Text;
                        vals = txt.PrefixTxt + vals;
                    }
                }
                //Case where you don't have to prefix text by dataset name and you don't have to create an array or enclose it
                else vals = txt.Text;
                if (ctrl.Syntax == "%%VALUE%%")
                {
                    result = ctrl.Syntax.Replace("%%VALUE%%", vals);
                    // if (!txt.RequiredSyntax) vals = vals + ",";
                }
                else
                {
                    result = ctrl.Syntax;
                    // if (!txt.RequiredSyntax) vals = vals + ",";
                }

                return result;
            }

            else if (typeof(CheckBox).IsAssignableFrom(element.GetType()))
            {
                BSkyCheckBox txt = element as BSkyCheckBox;

                if (txt.IsChecked.HasValue && txt.IsChecked.Value)
                {
                    if (ctrl.Syntax == "%%VALUE%%") return "TRUE";
                    else return ctrl.Syntax;
                }
                else
                {
                    if (txt.uncheckedsyntax != null)
                        return txt.uncheckedsyntax;
                    else return "";
                }
            }
            else if (typeof(RadioButton).IsAssignableFrom(element.GetType()))
            {
                RadioButton txt = element as RadioButton;
                result = txt.IsChecked.HasValue && txt.IsChecked.Value ? "TRUE" : "FALSE";

                //Added by Aaron
                //Made the changes below to support the aggregate command
                //If there is a string in syntax e.g. MEAN, if the radio button is selected, MEAN will be passed
                //If unselected, nothing or empty string will be passed
                //This will allow me to create syntax as follows
                //FUN ={Mean}{Sum}{txt2} where Mean, Sum are both radio buttons that are part of a group "rd"
                //Note: That txt2 is a text box control that returns a value only when the "other" radio button is enabled
                //"Other" is a part of the group rd
                //If the other radio button is selected, we pass the value in the textbox txt2
                //Other is not represented as only when other radio button  is selected that the string in txt2 is passed

                if (result == "TRUE")
                {
                    if (ctrl.Syntax == "%%VALUE%%")
                    {
                        return ctrl.Syntax.Replace("%%VALUE%%", result);
                    }
                    else if (ctrl.Syntax == "%DATASET%")
                    {
                        return currentDataset;
                    }
                    else if (ctrl.Syntax == "%MODEL%")
                    {
                        return activemodel;
                    }
                    else return ctrl.Syntax;
                }

                else return "";
                //result = ctrl.Syntax.Replace("%%VALUE%%", result);
                //return result;
            }
            else if (typeof(BSkyRadioGroup).IsAssignableFrom(element.GetType()))
            {
                // Aaron 12/24/2012
                // The radio buttons belonging to the radio group are either placed directly on the canvas (without using the dialog builder
                // that creates the radio buttons) or they are placed on the stackpanel that the radiogroup contains (The content property of the radiogroup
                // will point to the stack panel when we use the dialog builder)
                BSkyRadioGroup txt = element as BSkyRadioGroup;

                StackPanel stkpanl = txt.Content as StackPanel;
                //We are checking the count of the stack panel. Positive count indicates that radio buttons have been placed on stack panel
                if (stkpanl.Children.Count != 0)
                {
                    return txt.Value;
                }
                else
                {
                    BSkyCanvas parentofRdGrp = UIHelper.FindVisualParent<BSkyCanvas>(txt);

                    //System.Windows.FrameworkElement element1 = parent1.FindName(value) as System.Windows.FrameworkElement;
                    foreach (UIElement child in parentofRdGrp.Children)
                    {
                        if (child.GetType().Name == "BSkyRadioButton")
                        {
                            BSkyRadioButton rdBtn = child as BSkyRadioButton;

                            if (rdBtn.GroupName == objname)
                            {
                                if (rdBtn.IsChecked.HasValue && rdBtn.IsChecked.Value)
                                {
                                    if (rdBtn.Syntax == "%%VALUE%%") return "TRUE";

                                    else return rdBtn.Syntax;
                                }
                            }
                        }
                    }
                }
                //System.Windows.FrameworkElement element1 = parent1.FindName(value) as System.Windows.FrameworkElement;
                //if (element1 != null)
                //{
                //    BSkyRadioGroup radioGroup = element1 as BSkyRadioGroup;
                //    StackPanel stack1 = radioGroup.Content as StackPanel;
                //    stack1.Children.Add(this);
                //}
                //return txt.Value;
            }

            else if (typeof(BSkyEditableComboBox).IsAssignableFrom(element.GetType()))
            {
                string txtwithprefix = "";

                BSkyEditableComboBox txt = element as BSkyEditableComboBox;

                if (txt.Syntax != "%%VALUE%%")
                {
                    if (txt.prefixSelectedValue != "")
                    {
                        txt.Syntax = txt.prefixSelectedValue + txt.Syntax;
                    }
                    return txt.Syntax;
                }
                else if (txt.Text == "") return txt.NothingSelected;
                else if (txt.Syntax == "%%VALUE%%")
                {
                    txtwithprefix = txt.Text;
                    if (txt.prefixSelectedValue != "")
                    {
                        txtwithprefix = txt.prefixSelectedValue + txtwithprefix;
                    }
                    return txtwithprefix;
                }

            }
            else if (typeof(BSkyNonEditableComboBox).IsAssignableFrom(element.GetType()))
            {
                BSkyNonEditableComboBox txt = element as BSkyNonEditableComboBox;

                if (txt.Syntax != "%%VALUE%%") return txt.Syntax;
                else if (txt.SelectedValue == null) return txt.NothingSelected;
                else if (txt.Syntax == "%%VALUE%%") return txt.SelectedValue as string;

            }
            //Added by Aaron 03/03/2019
            else if (typeof(BSkyAdvancedSlider).IsAssignableFrom(element.GetType()))
            {
                BSkyAdvancedSlider advslider = element as BSkyAdvancedSlider;

                return advslider.SliderValue.ToString();

            }
            //Added by Aaron 03/04/2019
            else if (typeof(BSkySpinnerCtrl).IsAssignableFrom(element.GetType()))
            {
                BSkySpinnerCtrl spinner = element as BSkySpinnerCtrl;

                return spinner.Text;


            }



            return string.Empty;
        }

        #region For Syntax Editor
        //SynEditor set values back to dialog UI elements
        public static void SetValueFromSynEdt(DependencyObject obj, string objname, string args)
        {
            FrameworkElement fe = obj as FrameworkElement;
            FrameworkElement element = fe.FindName(objname) as FrameworkElement;

            if (element == null)//28Mar2013
            {
                MessageBox.Show(BSky.GlobalResources.Properties.Resources.CtrlNotInXaml + " " + objname +
                    ".\n " + BSky.GlobalResources.Properties.Resources.CtrlNameUsedInSyntax,
                    BSky.GlobalResources.Properties.Resources.UndefCtrl, MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            IBSkyInputControl ctrl = element as IBSkyInputControl;
            string result = string.Empty;
            //// take two strings and then try to replace
            ////bsky.one.sm.t.test(vars =c({SelectedVars}), mu={testValue}, conf.level=0.89, datasetname ='{%DATASET%}', missing=0)
            ////bsky.one.sm.t.test(vars =c('tg0'), mu=3, conf.level=0.89, datasetname ='Dataset1', missing=0)

            if (typeof(ListBox).IsAssignableFrom(element.GetType()))
            {
                string vals = string.Empty;
                //17Jan2014 string[] arr = args != null ? stringToArray(args) : null;//{"tg0","tg1","tg2"}; //10Sep2013 modified for NA
                DataSourceVariable[] dsv = args != null ? stringToDSV(args) : null;
                //ListBox list = element as ListBox;
                //list.ItemsSource = dsv;// arr;
                BSkyTargetList list = element as BSkyTargetList;
                list.ItemsSource = null;
                if (dsv != null)//Fix for if any list box in dialog is left empty
                {
                    foreach (DataSourceVariable svar in dsv)
                    {
                        list.Items.Add(svar);
                    }
                }
            }
            else if (typeof(TextBox).IsAssignableFrom(element.GetType()))
            {
                TextBox txt = element as TextBox;
                txt.Text = args;// "123";
            }
            else if (typeof(CheckBox).IsAssignableFrom(element.GetType()))
            {
                CheckBox txt = element as CheckBox;

                if (args.Equals("TRUE"))
                    txt.IsChecked = true;
                else
                    txt.IsChecked = false;
                ////result = txt.IsChecked.HasValue && txt.IsChecked.Value ? "TRUE" : "FALSE";
                ////result = ctrl.Syntax.Replace("%%VALUE%%", result);
                ////return result;
            }
            else if (typeof(RadioButton).IsAssignableFrom(element.GetType()))
            {
                RadioButton txt = element as RadioButton;

                if (args.Equals("TRUE"))
                    txt.IsChecked = true;
                else
                    txt.IsChecked = false;
                ////result = txt.IsChecked.HasValue && txt.IsChecked.Value ? "TRUE" : "FALSE";
                ////result = ctrl.Syntax.Replace("%%VALUE%%", result);
                ////return result;
            }
            if (typeof(BSkyRadioGroup).IsAssignableFrom(element.GetType()))
            {
                BSkyRadioGroup txt = element as BSkyRadioGroup;
                //txt.Value = args;
                ////return txt.Value;
            }

            ////return string.Empty;
        }

        private static string[] stringToArray(string str)
        {
            str = str.Replace("c(", " ").Replace(')', ' ').Trim(); // c( must not have space in between

            MatchCollection vars = Regex.Matches(str, "[A-Za-z0-9_.]+");
            int size = vars.Count;
            string[] arr = new string[size];
            int i = 0;

            foreach (Match m in vars)
            {
                arr[i++] = m.ToString();
                //Console.Write(" " + m.ToString());
            }
            return arr;
        }

        //17Jan2014 String to DataSourceVariable
        private static DataSourceVariable[] stringToDSV(string str)
        {
            str = str.Replace("c(", " ").Replace(')', ' ').Trim(); // c( must not have space in between

            MatchCollection vars = Regex.Matches(str, "[A-Za-z0-9_.]+");
            int size = vars.Count;
            DataSourceVariable[] arr = new DataSourceVariable[size];
            int i = 0;

            foreach (Match m in vars)
            {
                DataSourceVariable dsv = GetDataSourceVariable(m.ToString());
                arr[i++] = dsv != null ? dsv : new DataSourceVariable() { Name = m.ToString() };//03Apr2014
                //arr[i++] = new DataSourceVariable() { Name = m.ToString() };
                //Console.Write(" " + m.ToString());
            }
            return arr;
        }

        //Get datasourcevariable with all attributes like MEASURE, TYPE etc..
        private static DataSourceVariable GetDataSourceVariable(string variablename)
        {
            List<DataSourceVariable> lstdsv = null;

            if (AnalyticsData.DataSource != null)
                lstdsv = AnalyticsData.DataSource.Variables;
            else if (AnalyticsData.Result != null && AnalyticsData.Result.Datasource != null)
                lstdsv = AnalyticsData.Result.Datasource.Variables;

            if (lstdsv == null)
                return null;

            //01Feb2017 Instead of var.Name I changed it to var.RName to fix filter_. issue with cross tab.
            var variable = from var in lstdsv
                           where var.RName == variablename
                           select var;
            DataSourceVariable dv = variable.FirstOrDefault();

            if (dv != null)
            {
                return dv;
            }
            else
            {
                return null;
            }
        }

        public static void getArgumentSetDictionary(string bskcommand, Dictionary<string, string> varValuePair)
        {
            //"bsky.one.sm.t.test(vars=c('tg0','tg2','tg3'),mu=3,conf.level=0.89,datasetname='Dataset1',missing=0)";
            //working for command @"[A-Za-z0-9._]+=( ?(c\()(['A-Za-z0-9._,]+[\)]?)|([A-Za-z0-9._]+))";
            string pattern = @"[A-Za-z0-9._]+=( ?(c\()(['A-Za-z0-9._,{}%#$]+[\)]?)|(['A-Za-z0-9._{}%]+))";
            MatchCollection mc = Regex.Matches(bskcommand, pattern);

            foreach (Match m in mc)
            {
                Console.WriteLine("Index : " + m.Index + "   String : " + m.ToString());
                SplitAndAddToDictionary(m.ToString(), varValuePair);
            }
        }

        private static void SplitAndAddToDictionary(string str, Dictionary<string, string> vvp)
        {
            int endlen = str.IndexOf('=');
            string key = str.Substring(0, endlen).Trim();
            //10Sep2013 handle NA, replace null in its place
            string val = string.Empty;

            if (!str.Contains("'NA'") && str.Contains("NA")) // if there is NA which is R NA and not any value 'NA'
                val = null;
            else
                val = str.Substring(endlen).Replace('=', ' ').Replace("c(", " ").Replace(')', ' ').Trim();
            vvp.Add(key, val);
        }

        public static void MergeTemplateCommandDictionary(Dictionary<string, string> template, Dictionary<string, string> command, Dictionary<string, string> merged)
        {
            string tval = null;
            string cval = null;
            Dictionary<string, string>.KeyCollection kc = template.Keys;

            foreach (string k in kc)
            {
                template.TryGetValue(k, out tval);
                command.TryGetValue(k, out cval);
                if (!tval.Equals(cval))//if they are unequal.( Add only some values which are diff )
                {
                    //27Mar2014 if '{ctrl name}' is there but not just value like 3.5
                    //No need to substitute when there is value provided directly.
                    if (tval.Contains("{") && tval.Contains("}")) //just this if is added, following line were already there
                    {
                        if (cval != null)//10sep2013 mod for NA. if else added. Earlier there was just following stmt here
                            merged.Add(tval.Replace('{', ' ').Replace('}', ' ').Replace("'", " ").Replace('#', ' ').Replace('$', ' ').Trim(), cval.Trim());
                        else
                            merged.Add(tval.Replace('{', ' ').Replace('}', ' ').Replace("'", " ").Replace('#', ' ').Replace('$', ' ').Trim(), cval);//cval is null here so no Trim()
                    }
                }
            }
        }
        #endregion

        // private static readonly Regex re = new Regex(@"\{([^\}]+)\}", RegexOptions.Compiled);
        //Added by Aaron 06/18/2014
        //Added to handle {{ }} sourrounding control names
        private static readonly Regex re = new Regex(@"\{\{([^\}]+)\}\}", RegexOptions.Compiled);

        //The function below replaces the control name by the values in the commmand syntax


        public static string GetCommand(string commandformat, DependencyObject obj)
        {
            IConfigService confService = LifetimeService.Instance.Container.Resolve<IConfigService>();
            string themeSyntax  = confService.GetThemeParams();

            Dictionary<string, string> CommandKeyValDict = new Dictionary<string, string>();
            BSkyCanvas obj1 = null;
            string customsyntax = string.Empty;
            if (obj != null)//when prerequisite command is processed 'obj' is blank. obj will not be null for main syntax.
            {
                obj1 = obj as BSkyCanvas;
                customsyntax = obj1.customsyntax;
            }
            // string KEYWORD = "NoReplaCE";
            string output = string.Empty;
            if (customsyntax == "" || customsyntax == null)
            {
                output = re.Replace(commandformat,
                                delegate (Match match)
                                {
                                    //Aaron 12/11/2013
                                    //Not sure how the class match is constructed but every control name in the dialog that is 
                                    //referenced in the syntax command i.e. every thing enclosed in curly braces 
                                    //is contained in the match.Groups[1].Value structure
                                    //So line of code below is called for every string contained in the curly brace e.g. {source}
                                    //in the command syntax
                                    string matchedText = match.Groups[1].Value;
                                    //Aaron 12/11/2013
                                    //The function below gets the values we are going to replace the control name in the syntax command by
                                    //For example the control named source is reference in the command as {source}. In the command syntax
                                    //source gets replaced by var1,var2
                                    //The function below gets the string to replace the control name in the command syntax
                                    return GetParam(obj, matchedText);
                                });
            }
            else if (customsyntax == "Rank")
            {
                MatchCollection mcol = re.Matches(commandformat);
                foreach (Match m in mcol)
                {
                    string matchedText = m.Groups[1].Value;
                    string result = GetParam(obj, matchedText);
                    if (!CommandKeyValDict.ContainsKey(matchedText))
                    {
                        CommandKeyValDict.Add(matchedText, result);
                    }
                }

                string rdgrp1 = "";
                string txt1 = "";
                string dest = "";
                string rankby = "";
                string rankfn = "";
                string nooftiles = "";
                string dataset = "";

                //At this point CommandKeyValDict has key and values
                //Aaron will arrange key-vals as wanted
                foreach (KeyValuePair<string, string> kv in CommandKeyValDict)
                {
                    string key = kv.Key;
                    string value = kv.Value;
                    //create final syntac in 'output'
                    // output = output+","+ key + "=c(" + value + ")";
                    if (key == "rdgrp1")
                    {
                        rdgrp1 = value;

                    }
                    if (key == "txt1")
                    {
                        txt1 = value;
                    }

                    if (key == "dest")
                    {
                        dest = value;
                    }
                    if (key == "rankby")
                    {
                        rankby = value;
                    }
                    if (key == "rankfn")
                    {
                        rankfn = value;
                    }
                    if (key == "nooftiles")
                    {
                        nooftiles = value;
                    }
                    if (key == "%DATASET%")
                    {
                        dataset = value;
                    }
                }
                output = dataset + " <- " + dataset + " ";

                // output = output + "mutate(";

                if (rankby != "")
                {
                    output = output + "%>%" + " group_by(" + rankby + ")";
                }
                output = output + " %>%  mutate(";
                string[] values = dest.Split(',');

                int value1;

                if (rankfn == "ntile")

                {
                    if (!int.TryParse(nooftiles, out value1))
                    {

                        output = "cat('Error: When selecting the ntile ranking function, you must specify the number of tiles. Re-run the analysis after you have specified the number of tiles')";
                        return (output);

                    }
                }

                if (rdgrp1 == "Prefix")
                {
                    for (int i = 0; i < values.Length; i++)
                    {
                        // values[i] = values[i].Trim();

                        if (rankfn != "ntile")
                        {
                            output = output + txt1 + "_" + values[i] + "=" + rankfn + "(" + values[i] + ")" + ",";
                        }
                        else
                        {
                            // string x = "42";

                            ////  if (int.TryParse(nooftiles, out value))
                            //  {
                            output = output + txt1 + "_" + values[i] + "=" + rankfn + "(" + values[i] + "," + nooftiles + ")" + ",";
                            //  }
                            //  else
                            //  {
                            //      // MessageBox("The number of tiles must be an integer value");
                            //      // MessageBox()
                            //      string message = "Simple MessageBox";
                            //      MessageBox.Show(message);


                            //  }
                        }
                    }
                    output = output.TrimEnd(',');
                    output = output + ")";
                }
                else
                {
                    for (int i = 0; i < values.Length; i++)
                    {
                        // values[i] = values[i].Trim();
                        // output = output +  values[i] + "=" + txt1 + "_" + values[i] + ",";
                        if (rankfn != "ntile")
                        {
                            output = output + values[i] + "_" + txt1 + "=" + rankfn + "(" + values[i] + ")" + ",";
                        }
                        else
                        {
                            //if (nooftiles )
                            output = output + values[i] + "_" + txt1 + "=" + rankfn + "(" + values[i] + "," + nooftiles + ")" + ",";
                        }
                    }
                    output = output.TrimEnd(',');
                    output = output + ")";
                }

                output = output + "\nBSkyLoadRefreshDataframe(" + dataset + ")";
            }

            else if (customsyntax == "Graphics-scatterplot")
            {
                //print(ggplot({ {% DATASET %} }, aes(x = { { GroupingVariable} }, y = eval(parse(text = paste(vars))) ,color = { { GroupBy} },size ={ { size} } ,alpha ={ { opacity} })) +geom_point() + labs(x = "{{GroupingVariable}}", y = vars, color = "{{GroupBy}}", title = paste("Scatter plot for variable ", "{{GroupingVariable}}", " by ", vars, sep = '')) + xlab("{{xlab}}") + ylab("{{ylab}}") + ggtitle("{{maintitle}}") { { themes} }
                //+facet_grid({ { Facetcolumn} }
                //~{ { Facetrow} }, scales ={ { Facetscale} })  +facet_wrap(  { { Facetwrap} } )+geom_smooth(method = "{{sm}}", color = "{{color}}"))

                MatchCollection mcol = re.Matches(commandformat);
                foreach (Match m in mcol)
                {
                    string matchedText = m.Groups[1].Value;
                    string result = GetParam(obj, matchedText);
                    if (!CommandKeyValDict.ContainsKey(matchedText))
                    {
                        CommandKeyValDict.Add(matchedText, result);
                    }
                }

                string GroupingVariable = "";
                string Destination = "";
                string GroupBy = "";
                string size = "";
                string opacity = "";
                string Facetcolumn = "";
                string Facetrow = "";
                string Facetscale = "";
                string Facetwrap = "";
                string sm = "";
                string color = "";
                string dataset = "";
                string xlab = "";
                string ylab = "";
                string maintitle = "";
                string jitter = "";
                string flipaxis = "";
                string shape = "";
                string se = "";

                foreach (KeyValuePair<string, string> kv in CommandKeyValDict)
                {
                    string key = kv.Key;
                    string value = kv.Value;
                    //create final syntac in 'output'
                    // output = output+","+ key + "=c(" + value + ")";
                    if (key == "GroupingVariable")
                    {
                        GroupingVariable = value;

                    }
                    if (key == "color")
                    {
                        color = value;

                    }
                    if (key == "Destination")
                    {
                        Destination = value;
                    }

                    if (key == "GroupBy")
                    {
                        GroupBy = value;
                    }
                    if (key == "size")
                    {
                        size = value;
                    }
                    if (key == "opacity")
                    {
                        opacity = value;
                    }
                    if (key == "Facetcolumn")
                    {
                        Facetcolumn = value;
                    }
                    if (key == "Facetrow")
                    {
                        Facetrow = value;
                    }
                    if (key == "Facetscale")
                    {
                        Facetscale = value;
                    }
                    if (key == "Facetwrap")
                    {
                        Facetwrap = value;
                    }
                    if (key == "%DATASET%")
                    {
                        dataset = value;
                    }
                    if (key == "xlab")
                    {
                        xlab = value;
                    }
                    if (key == "ylab")
                    {
                        ylab = value;
                    }
                    if (key == "maintitle")
                    {
                        maintitle = value;
                    }
                    if (key == "sm")
                    {
                        sm = value;
                    }
                    if (key == "jitter")
                    {
                        jitter = value;
                    }
                    if (key == "flipaxis")
                    {
                        flipaxis = value;
                    }
                    if (key == "shape")
                    {
                        shape = value;
                    }
                    if (key == "se")
                    {
                        se = value;
                    }
                }
                string tempoutput = "";
                string[] variables = Destination.Split(',');

                foreach (string var in variables)
                {
                    tempoutput = tempoutput + "ggplot(data=" + dataset + ", aes(x =" + GroupingVariable + "," + "y=" + var;
                    if (GroupBy != "")
                    {
                        tempoutput = tempoutput + ",color=" + GroupBy;
                    }
                    if (size != "")
                    {
                        tempoutput = tempoutput + ",size=" + size;
                    }
                    if (opacity != "")
                    {
                        tempoutput = tempoutput + ",alpha=" + opacity;
                    }
                    if (shape != "")
                    {
                        tempoutput = tempoutput + ",shape=" + shape;
                    }

                    tempoutput = tempoutput + "))";

                    if (jitter != "TRUE")
                    {
                        tempoutput = tempoutput + " +\n\t geom_point()";
                    }
                    else
                    {
                        tempoutput = tempoutput + " +\n\t geom_point( position=\"jitter\")";
                    }
                    if (flipaxis == "TRUE")
                    {
                        tempoutput = tempoutput + " +\n\t coord_flip()";
                    }

                    tempoutput = tempoutput + " +\n\t labs(x =" + "\"" + GroupingVariable + "\"" + ", y =" + "\"" + var + "\"" + "," + "color =" + "\"" + GroupBy + "\"" + ", title= " + "\"Scatter plot for variable " + GroupingVariable + " by " + var + "\")";

                    if (xlab != "")
                    {
                        tempoutput = tempoutput + " +\n\t xlab(" + "\"" + xlab + "\"" + ")";
                    }

                    if (ylab != "")
                    {
                        tempoutput = tempoutput + " +\n\t ylab(" + "\"" + ylab + "\"" + ")";
                    }

                    if (maintitle != "")
                    {
                        tempoutput = tempoutput + " +\n\t ggtitle(" + "\"" + maintitle + "\"" + ")";
                    }

                    //+geom_smooth(method ="{{sm}}", color= "{{color}}")
                    if (!(sm == "" || sm == null))
                    {
                        tempoutput = tempoutput + " +\n\t geom_smooth(method =\"" + sm + "\"";

                        if (!(color == "" || color == null))
                        {
                            tempoutput = tempoutput + ",color=" + "\"" + color + "\"";

                        }

                        tempoutput = tempoutput + ",se=" + se;

                        tempoutput = tempoutput + ")";
                    }
                    tempoutput = tempoutput + createfacets(Facetwrap, Facetcolumn, Facetrow, Facetscale);
                    // tempoutput = Wrapinbrackets(tempoutput);
                    tempoutput = tempoutput + "\n\n";
                    output = output + tempoutput;

                    tempoutput = "";

                    output = output.TrimEnd(Environment.NewLine.ToCharArray());
                    output = output.TrimEnd(Environment.NewLine.ToCharArray());
                    output = output + " +\n" + themeSyntax + "\n\n";
                }

                //+facet_grid({ { Facetcolumn} } ~ {{ Facetrow} }, scales ={ { Facetscale} })  +facet_wrap(  { { Facetwrap} } )
            }

            else if (customsyntax == "Graphics-stripchart")
            {
                //print(ggplot({ {% DATASET %} }, aes(x = { { GroupingVariable} }, y = eval(parse(text = paste(vars))) ,color = { { GroupBy} },size ={ { size} } ,alpha ={ { opacity} })) +geom_point() + labs(x = "{{GroupingVariable}}", y = vars, color = "{{GroupBy}}", title = paste("Scatter plot for variable ", "{{GroupingVariable}}", " by ", vars, sep = '')) + xlab("{{xlab}}") + ylab("{{ylab}}") + ggtitle("{{maintitle}}") { { themes} }
                //+facet_grid({ { Facetcolumn} }
                //~{ { Facetrow} }, scales ={ { Facetscale} })  +facet_wrap(  { { Facetwrap} } )+geom_smooth(method = "{{sm}}", color = "{{color}}"))

                MatchCollection mcol = re.Matches(commandformat);
                foreach (Match m in mcol)
                {
                    string matchedText = m.Groups[1].Value;
                    string result = GetParam(obj, matchedText);
                    if (!CommandKeyValDict.ContainsKey(matchedText))
                    {
                        CommandKeyValDict.Add(matchedText, result);
                    }
                }

                string GroupingVariable = "";
                string Destination = "";
                string GroupBy = "";
                string size = "";
                string opacity = "";
                string Facetcolumn = "";
                string Facetrow = "";
                string Facetscale = "";
                string Facetwrap = "";
                string sm = "";
                string color = "";
                string dataset = "";
                string xlab = "";
                string ylab = "";
                string maintitle = "";
                string jitter = "";
                string flipaxis = "";
                string shape = "";

                foreach (KeyValuePair<string, string> kv in CommandKeyValDict)
                {
                    string key = kv.Key;
                    string value = kv.Value;
                    //create final syntac in 'output'
                    // output = output+","+ key + "=c(" + value + ")";
                    if (key == "GroupingVariable")
                    {
                        GroupingVariable = value;

                    }
                    if (key == "Destination")
                    {
                        Destination = value;
                    }

                    if (key == "GroupBy")
                    {
                        GroupBy = value;
                    }
                    if (key == "size")
                    {
                        size = value;
                    }
                    if (key == "opacity")
                    {
                        opacity = value;
                    }
                    if (key == "Facetcolumn")
                    {
                        Facetcolumn = value;
                    }
                    if (key == "Facetrow")
                    {
                        Facetrow = value;
                    }
                    if (key == "Facetscale")
                    {
                        Facetscale = value;
                    }
                    if (key == "Facetwrap")
                    {
                        Facetwrap = value;
                    }
                    if (key == "%DATASET%")
                    {
                        dataset = value;
                    }
                    if (key == "xlab")
                    {
                        xlab = value;
                    }
                    if (key == "ylab")
                    {
                        ylab = value;
                    }
                    if (key == "maintitle")
                    {
                        maintitle = value;
                    }
                    if (key == "sm")
                    {
                        sm = value;
                    }
                    if (key == "jitter")
                    {
                        jitter = value;
                    }
                    if (key == "flipaxis")
                    {
                        flipaxis = value;
                    }
                    if (key == "shape")
                    {
                        shape = value;
                    }
                }
                string tempoutput = "";
                string[] variables = Destination.Split(',');

                foreach (string var in variables)
                {
                    tempoutput = tempoutput + "ggplot(data=" + dataset + ", aes(x =" + GroupingVariable + "," + "y=" + var;
                    if (GroupBy != "")
                    {
                        tempoutput = tempoutput + ",color=" + GroupBy;
                    }
                    if (size != "")
                    {
                        tempoutput = tempoutput + ",size=" + size;
                    }
                    if (opacity != "")
                    {
                        tempoutput = tempoutput + ",alpha=" + opacity;
                    }
                    if (shape != "")
                    {
                        tempoutput = tempoutput + ",shape=" + shape;

                    }

                    tempoutput = tempoutput + "))";

                    if (jitter != "TRUE")
                    {
                        tempoutput = tempoutput + " +\n\t geom_jitter()";
                    }
                    else
                    {
                        tempoutput = tempoutput + " +\n\t geom_jitter( position=\"jitter\")";
                    }
                    if (flipaxis == "TRUE")
                    {
                        tempoutput = tempoutput + " +\n\t coord_flip()";
                    }

                    tempoutput = tempoutput + " +\n\t labs(x =" + "\"" + GroupingVariable + "\"" + ", y =" + "\"" + var + "\"" + "," + "color =" + "\"" + GroupBy + "\"" + ", title= " + "\"Strip chart for variable " + GroupingVariable + " by " + var + "\")";

                    if (xlab != "")
                    {
                        tempoutput = tempoutput + " +\n\t xlab(" + "\"" + xlab + "\"" + ")";
                    }

                    if (ylab != "")
                    {
                        tempoutput = tempoutput + " +\n\t ylab(" + "\"" + ylab + "\"" + ")";
                    }

                    if (maintitle != "")
                    {
                        tempoutput = tempoutput + " +\n\t ggtitle(" + "\"" + maintitle + "\"" + ")";
                    }

                    //+geom_smooth(method ="{{sm}}", color= "{{color}}")
                    if (!(sm == "" || sm == null))
                    {
                        tempoutput = tempoutput + " +\n\t geom_smooth(method =\"" + sm + "\"";

                        if (!(color == "" || color == null))
                        {
                            tempoutput = tempoutput + ",color=" + "\"" + color + "\"";

                        }
                        tempoutput = tempoutput + ")";

                    }
                    tempoutput = tempoutput + createfacets(Facetwrap, Facetcolumn, Facetrow, Facetscale);
                    // tempoutput = Wrapinbrackets(tempoutput);
                    tempoutput = tempoutput + "\n\n";
                    output = output + tempoutput;

                    tempoutput = "";

                    output = output.TrimEnd(Environment.NewLine.ToCharArray());
                    output = output.TrimEnd(Environment.NewLine.ToCharArray());
                    output = output + " +\n" + themeSyntax + "\n\n";
                }

                //+facet_grid({ { Facetcolumn} } ~ {{ Facetrow} }, scales ={ { Facetscale} })  +facet_wrap(  { { Facetwrap} } )
            }

            //   ggplot(data = { {% DATASET %} }, aes(x = eval(parse(text = paste(vars))), fill = { { Groupby} }))  + xlab("{{xlab}}") + ylab("{{ylab}}") + ggtitle("{{maintitle}}") { { themes} }
            // +geom_density(position = "{{rdgrp1}}", fill = "{{barcolor}}", alpha = 0.5) 
            //  +labs(x = vars, y = "Density", fill = "{{Groupby}}", paste("Density plot for variable ", vars, sep = ''))
            //  +facet_grid({ { yfacet} }
            //  ~{ { xfacet} })

            else if (customsyntax == "Graphics-density")
            {
                MatchCollection mcol = re.Matches(commandformat);
                foreach (Match m in mcol)
                {
                    string matchedText = m.Groups[1].Value;
                    string result = GetParam(obj, matchedText);
                    if (!CommandKeyValDict.ContainsKey(matchedText))
                    {
                        CommandKeyValDict.Add(matchedText, result);
                    }
                }

                string destination = "";
                string binwidth = "";

                string opacity = "";

                string Facetcolumn = "";
                string Facetrow = "";
                string Facetscale = "";
                string Facetwrap = "";
                string Groupby = "";
                string barcolor = "";
                string dataset = "";
                string xlab = "";
                string ylab = "";
                string maintitle = "";
                string jitter = "";
                string flipaxis = "";
                string rdgrp1 = "";

                string bordercolor = "";

                foreach (KeyValuePair<string, string> kv in CommandKeyValDict)
                {
                    string key = kv.Key;
                    string value = kv.Value;
                    //create final syntac in 'output'
                    // output = output+","+ key + "=c(" + value + ")";

                    if (key == "rdgrp1")
                    {
                        rdgrp1 = value;
                    }

                    if (key == "destination")
                    {
                        destination = value;
                    }
                    if (key == "Groupby")
                    {
                        Groupby = value;
                    }
                    if (key == "binwidth")
                    {
                        binwidth = value;
                    }

                    if (key == "opacity")
                    {
                        opacity = value;
                    }
                    if (key == "Facetcolumn")
                    {
                        Facetcolumn = value;
                    }
                    if (key == "Facetrow")
                    {
                        Facetrow = value;
                    }
                    if (key == "Facetscale")
                    {
                        Facetscale = value;
                    }
                    if (key == "Facetwrap")
                    {
                        Facetwrap = value;
                    }
                    if (key == "%DATASET%")
                    {
                        dataset = value;
                    }
                    if (key == "xlab")
                    {
                        xlab = value;
                    }
                    if (key == "ylab")
                    {
                        ylab = value;
                    }
                    if (key == "maintitle")
                    {
                        maintitle = value;
                    }

                    if (key == "jitter")
                    {
                        jitter = value;
                    }
                    if (key == "flipaxis")
                    {
                        flipaxis = value;
                    }
                    if (key == "bordercolor")
                    {
                        bordercolor = value;
                    }
                    if (key == "barcolor")
                    {
                        barcolor = value;
                    }
                }
                string tempoutput = "";
                string[] variables = destination.Split(',');

                foreach (string var in variables)
                {
                    //   ggplot(data = { {% DATASET %} }, aes(x = eval(parse(text = paste(vars))), fill = { { Groupby} })) 
                    tempoutput = tempoutput + "ggplot(data=" + dataset + ", aes(x =" + var;

                    if (Groupby != "")
                    {
                        tempoutput = tempoutput + ",fill=" + Groupby;
                    }

                    tempoutput = tempoutput + "))";

                    //+geom_density(position = "{{rdgrp1}}", fill = "{{barcolor}}", alpha = 0.5)
                    tempoutput = tempoutput + " +\n\t geom_density( ";

                    if (rdgrp1 != "")
                    {
                        tempoutput = tempoutput + ",position = \"" + rdgrp1 + "\"";
                    }


                    if (!(barcolor == "" || barcolor == null))
                    {
                        tempoutput = tempoutput + ",fill =" + "\"" + barcolor + "\"";
                    }
                    if (opacity != "")
                    {
                        tempoutput = tempoutput + ",alpha=" + opacity;
                    }

                    tempoutput = tempoutput + ")";
                    // End of geom_density

                    if (flipaxis == "TRUE")
                    {
                        tempoutput = tempoutput + " +\n\t coord_flip()";
                    }

                    //   +labs(x = vars, y = "Count", fill = "{{Groupby}}", title = paste("Density plot for variable ", vars, sep = '')) +

                    if (Groupby != "")
                    {
                        tempoutput = tempoutput + " +\n\t labs(x =" + "\"" + var + "\"" + ", y =" + "\"" + "Counts" + "\"" + ", fill =" + "\"" + Groupby + "\"" + ", title= " + "\"Density plot for variable " + var + " separated by variable " + Groupby + "\")";
                    }
                    else
                    {
                        tempoutput = tempoutput + " +\n\t labs(x =" + "\"" + var + "\"" + ", y =" + "\"" + "Counts" + "\"" + ", fill =" + "\"" + Groupby + "\"" + ", title= " + "\"Density plot for variable " + var + "\")";
                    }

                    if (xlab != "")
                    {
                        tempoutput = tempoutput + " +\n\t xlab(" + "\"" + xlab + "\"" + ")";
                    }

                    if (ylab != "")
                    {
                        tempoutput = tempoutput + " +\n\t ylab(" + "\"" + ylab + "\"" + ")";
                    }

                    if (maintitle != "")
                    {
                        tempoutput = tempoutput + " +\n\t ggtitle(" + "\"" + maintitle + "\"" + ")";
                    }

                    //+geom_smooth(method ="{{sm}}", color= "{{color}}")

                    tempoutput = tempoutput + createfacets(Facetwrap, Facetcolumn, Facetrow, Facetscale);
                    // tempoutput = Wrapinbrackets(tempoutput);
                    tempoutput = tempoutput + "\n\n";
                    output = output + tempoutput;

                    tempoutput = "";

                    output = output.TrimEnd(Environment.NewLine.ToCharArray());
                    output = output.TrimEnd(Environment.NewLine.ToCharArray());
                    output = output + " +\n" + themeSyntax + "\n\n";
                }

                //+facet_grid({ { Facetcolumn} } ~ {{ Facetrow} }, scales ={ { Facetscale} })  +facet_wrap(  { { Facetwrap} } )
            }

            // ggplot(data = { {% DATASET %} }, aes(x = eval(parse(text = paste(vars))), y =..count.., fill = { { Groupby} })) +geom_density(fill = "{{barcolor}}", position = "fill", alpha = 0.5) + labs(x = vars, y = "Count", fill = "{{Groupby}}", title = paste("Density plot for variable ", vars, sep = '')) + xlab("{{xlab}}") + ylab("{{ylab}}") + ggtitle("{{maintitle}}") { { themes} }
            //+geom_density(fill = "{{barcolor}}", position = "fill", alpha = 0.5) + labs(x = vars, y = "Count", fill = "{{Groupby}}", title = paste("Density plot for variable ", vars, sep = '')) + xlab("{{xlab}}") + ylab("{{ylab}}") + ggtitle("{{maintitle}}") { { themes} }
            // +facet_grid({ { yfacet} }
            // ~{ { xfacet} })

            else if (customsyntax == "Graphics-densitycounts")
            {
                MatchCollection mcol = re.Matches(commandformat);
                foreach (Match m in mcol)
                {
                    string matchedText = m.Groups[1].Value;
                    string result = GetParam(obj, matchedText);
                    if (!CommandKeyValDict.ContainsKey(matchedText))
                    {
                        CommandKeyValDict.Add(matchedText, result);
                    }
                }

                string destination = "";
                string binwidth = "";

                string opacity = "";

                string Facetcolumn = "";
                string Facetrow = "";
                string Facetscale = "";
                string Facetwrap = "";
                string Groupby = "";
                string barcolor = "";
                string dataset = "";
                string xlab = "";
                string ylab = "";
                string maintitle = "";
                string jitter = "";
                string flipaxis = "";

                string bordercolor = "";

                foreach (KeyValuePair<string, string> kv in CommandKeyValDict)
                {
                    string key = kv.Key;
                    string value = kv.Value;
                    //create final syntac in 'output'
                    // output = output+","+ key + "=c(" + value + ")";

                    if (key == "destination")
                    {
                        destination = value;
                    }
                    if (key == "Groupby")
                    {
                        Groupby = value;
                    }
                    if (key == "binwidth")
                    {
                        binwidth = value;
                    }

                    if (key == "opacity")
                    {
                        opacity = value;
                    }
                    if (key == "Facetcolumn")
                    {
                        Facetcolumn = value;
                    }
                    if (key == "Facetrow")
                    {
                        Facetrow = value;
                    }
                    if (key == "Facetscale")
                    {
                        Facetscale = value;
                    }
                    if (key == "Facetwrap")
                    {
                        Facetwrap = value;
                    }
                    if (key == "%DATASET%")
                    {
                        dataset = value;
                    }
                    if (key == "xlab")
                    {
                        xlab = value;
                    }
                    if (key == "ylab")
                    {
                        ylab = value;
                    }
                    if (key == "maintitle")
                    {
                        maintitle = value;
                    }

                    if (key == "jitter")
                    {
                        jitter = value;
                    }
                    if (key == "flipaxis")
                    {
                        flipaxis = value;
                    }
                    if (key == "bordercolor")
                    {
                        bordercolor = value;
                    }
                    if (key == "barcolor")
                    {
                        barcolor = value;
                    }
                }
                string tempoutput = "";
                string[] variables = destination.Split(',');

                foreach (string var in variables)
                {
                    //    ggplot(data = { {% DATASET %} }, aes(x = eval(parse(text = paste(vars))), y =..count.., fill = { { Groupby} }))
                    tempoutput = tempoutput + "ggplot(data=" + dataset + ", aes(x =" + var + ", y =..count..";

                    if (Groupby != "")
                    {
                        tempoutput = tempoutput + ",fill=" + Groupby;
                    }

                    tempoutput = tempoutput + "))";

                    //   +geom_density(fill = "{{barcolor}}", position = "fill", alpha = 0.5)
                    tempoutput = tempoutput + " +\n\t geom_density( ";

                    if (Groupby != "")
                    {
                        tempoutput = tempoutput + ",position = \"fill\"";
                    }

                    if (!(barcolor == "" || barcolor == null))
                    {
                        tempoutput = tempoutput + ",fill =" + "\"" + barcolor + "\"";
                    }
                    if (opacity != "")
                    {
                        tempoutput = tempoutput + ",alpha=" + opacity;
                    }

                    tempoutput = tempoutput + ")";
                    // End of geom_density

                    if (flipaxis == "TRUE")
                    {
                        tempoutput = tempoutput + " +\n\t coord_flip()";
                    }


                    //   +labs(x = vars, y = "Count", fill = "{{Groupby}}", title = paste("Density plot for variable ", vars, sep = '')) +

                    if (Groupby != "")
                    {
                        tempoutput = tempoutput + " +\n\t labs(x =" + "\"" + var + "\"" + ", y =" + "\"" + "Counts" + "\"" + ", fill =" + "\"" + Groupby + "\"" + ", title= " + "\"Density plot for variable " + var + " separated by variable " + Groupby + "\")";
                    }
                    else
                    {
                        tempoutput = tempoutput + " +\n\t labs(x =" + "\"" + var + "\"" + ", y =" + "\"" + "Counts" + "\"" + ", fill =" + "\"" + Groupby + "\"" + ", title= " + "\"Density plot for variable " + var + "\")";
                    }

                    if (xlab != "")
                    {
                        tempoutput = tempoutput + " +\n\t xlab(" + "\"" + xlab + "\"" + ")";
                    }

                    if (ylab != "")
                    {
                        tempoutput = tempoutput + " +\n\t ylab(" + "\"" + ylab + "\"" + ")";
                    }

                    if (maintitle != "")
                    {
                        tempoutput = tempoutput + " +\n\t ggtitle(" + "\"" + maintitle + "\"" + ")";
                    }

                    //+geom_smooth(method ="{{sm}}", color= "{{color}}")

                    tempoutput = tempoutput + createfacets(Facetwrap, Facetcolumn, Facetrow, Facetscale);
                    // tempoutput = Wrapinbrackets(tempoutput);
                    tempoutput = tempoutput + "\n\n";
                    output = output + tempoutput;

                    tempoutput = "";

                    output = output.TrimEnd(Environment.NewLine.ToCharArray());
                    output = output.TrimEnd(Environment.NewLine.ToCharArray());
                    output = output + " +\n" + themeSyntax + "\n\n";
                }

                //+facet_grid({ { Facetcolumn} } ~ {{ Facetrow} }, scales ={ { Facetscale} })  +facet_wrap(  { { Facetwrap} } )
            }

            else if (customsyntax == "Graphics-plotofmeans")
            {
                MatchCollection mcol = re.Matches(commandformat);
                foreach (Match m in mcol)
                {
                    string matchedText = m.Groups[1].Value;
                    string result = GetParam(obj, matchedText);
                    if (!CommandKeyValDict.ContainsKey(matchedText))
                    {
                        CommandKeyValDict.Add(matchedText, result);
                    }
                }

                string destination = "";
                string binwidth = "";

                string opacity = "";

                string Facetcolumn = "";
                string Facetrow = "";
                string Facetscale = "";
                string Facetwrap = "";

                string color = "";
                string dataset = "";
                string xlab = "";
                string ylab = "";
                string maintitle = "";
                string jitter = "";
                string flipaxis = "";
                string xaxis = "";

                string bordercolor = "";
                string Groupby = "";
                string GroupbyCommandSection = string.Empty;
                string conflevel = "";
                string stddev = "";
                string confinterval = "";
                string noerrbars = "";
                string stderr = "";
                string yaxis = "";
                //  { { stddev} }
                // { { confinterval} }
                // { { noerrbars} }

                foreach (KeyValuePair<string, string> kv in CommandKeyValDict)
                {
                    string key = kv.Key;
                    string value = kv.Value;
                    //create final syntac in 'output'
                    // output = output+","+ key + "=c(" + value + ")";

                    if (key == "Groupby")
                    {
                        Groupby = value;
                        if (string.IsNullOrEmpty(Groupby))
                        {
                            GroupbyCommandSection = string.Empty;
                        }
                        else
                        {
                            GroupbyCommandSection = ",\"" + Groupby + "\"";
                        }
                    }


                    if (key == "stderr")
                    {
                        stderr = value;
                    }

                    if (key == "stddev")
                    {
                        stddev = value;
                    }

                    if (key == "confinterval")
                    {
                        confinterval = value;
                    }
                    if (key == "conflevel")
                    {
                        conflevel = value;
                    }

                    if (key == "noerrbars")
                    {
                        noerrbars = value;
                    }

                    if (key == "xaxis")
                    {
                        xaxis = value;
                    }


                    if (key == "yaxis")
                    {
                        yaxis = value;
                    }
                    if (key == "binwidth")
                    {
                        binwidth = value;
                    }

                    if (key == "opacity")
                    {
                        opacity = value;
                    }
                    if (key == "Facetcolumn")
                    {
                        Facetcolumn = value;
                    }
                    if (key == "Facetrow")
                    {
                        Facetrow = value;
                    }
                    if (key == "Facetscale")
                    {
                        Facetscale = value;
                    }
                    if (key == "Facetwrap")
                    {
                        Facetwrap = value;
                    }
                    if (key == "%DATASET%")
                    {
                        dataset = value;
                    }
                    if (key == "xlab")
                    {
                        xlab = value;
                    }
                    if (key == "ylab")
                    {
                        ylab = value;
                    }
                    if (key == "maintitle")
                    {
                        maintitle = value;
                    }

                    if (key == "jitter")
                    {
                        jitter = value;
                    }
                    if (key == "flipaxis")
                    {
                        flipaxis = value;
                    }
                    if (key == "bordercolor")
                    {
                        bordercolor = value;
                    }
                    if (key == "color")
                    {
                        color = value;
                    }
                }

                string tempoutput = "";
                string[] variables = yaxis.Split(',');

                string tempFacets = "";


                //  if (!(Facetcolumn == "" || Facetrow == "" || Facetwrap == "" || Facetcolumn == null || Facetrow == null || Facetwrap == null))
                if (!(string.IsNullOrEmpty(Facetcolumn) && string.IsNullOrEmpty(Facetrow) && string.IsNullOrEmpty(Facetwrap)))
                {
                    int count = 0;
                    // tempFacets += "c(";
                    if (!(Facetcolumn == null || Facetcolumn == ""))
                    {
                        // tempFacets +=  "\"" + Facetcolumn + "\"";
                        tempFacets += Facetcolumn;
                        count++;
                    }

                    if (!(Facetrow == null || Facetrow == ""))
                    {
                        // tempFacets += "\"" + Facetrow + "\"";
                        tempFacets += Facetrow;
                        count++;
                    }

                    if (!(Facetwrap == null || Facetwrap == ""))
                    {
                        //tempFacets += "\"" + Facetwrap + "\"";
                        tempFacets += Facetwrap;
                        count++;
                    }
                    // tempFacets += ")";
                    if (count > 1)
                    {
                        string msg = "Error: You have specified more than one facet. You can only specify one facet at a time (namely one of Facetrow, Facetcol, Facetwrap)";
                        MessageBox.Show(msg);
                        return string.Empty;
                    }
                }

                foreach (string var in variables)
                {
                    //# Create a dataset of summaries
                    //  { { datasetForSum} } <- summarySE({ {% DATASET %} }, measurevar = vars, groupvars = c("{{xaxis}}", "{{groupby}}"),conf.interval = { { conflevel} },na.rm = TRUE)
                    //string tempdataset = "";

                    if (tempFacets == "")
                    {
                        tempoutput += "temp <-Rmisc::summarySE( " + dataset + ", measurevar = " + "\"" + var + "\"" + ", groupvars = c(" + "\"" + xaxis + "\"" + GroupbyCommandSection + ")" + ",conf.interval = " + conflevel + ",na.rm = TRUE,.drop = TRUE)";
                    }
                    else
                    {
                        tempoutput += "temp <-Rmisc::summarySE( " + dataset + ", measurevar = " + "\"" + var + "\"" + ", groupvars = c(" + "\"" + xaxis + "\"" + GroupbyCommandSection + ",\"" + tempFacets + "\"" + ")" + ",conf.interval = " + conflevel + ",na.rm = TRUE,.drop = TRUE)";
                    }
                    tempoutput = tempoutput + "\n";

                    //  ggplot({ { datasetForSum} }, aes(x ={ { xaxis} }, y = eval(parse(text = paste(vars))), colour ={ { groupby} },group ={ { groupby} })) +geom_errorbar(aes(ymin = eval(parse(text = paste(vars))) -{ { stderr} }
                    //  { { stddev} }
                    // { { confinterval} }
                    // { { noerrbars} }, ymax = eval(parse(text = paste(vars))) +{ { stderr} }
                    // { { stddev} }
                    // { { confinterval} }
                    // { { noerrbars} }), width = .1,position = pd) +geom_line(position = pd) + geom_point(position = pd) + labs(x = "{{xaxis}}", y = vars, fill = "{{Groupby}}", title = paste("Plot for variable ", vars, sep = '')) + xlab("{{xlab}}") + ylab("{{ylab}}" + ggtitle("{{maintitle}}") { { themes} })+facet_grid({ { yfacet} }
                    // ~{ { xfacet} })

                    tempoutput += "pd <- position_dodge(0.3)";
                    tempoutput = tempoutput + "\n";

                    tempoutput = tempoutput + "ggplot(data=temp" + ", aes(x = as.numeric(" + xaxis + ")";

                    tempoutput += ", y = " + var;

                    if (Groupby != "")
                    {
                        tempoutput += ", colour = " + Groupby;
                    }
                    if (Groupby != "")
                    {
                        tempoutput += ", group = " + Groupby;
                    }

                    tempoutput = tempoutput + "))";

                    // +geom_errorbar(aes(ymin = eval(parse(text = paste(vars))) -{ { stderr} }
                    //  { { stddev} }
                    // { { confinterval} }
                    // { { noerrbars} }, ymax = eval(parse(text = paste(vars))) +{ { stderr} }
                    // { { stddev} }
                    // { { confinterval} }
                    // { { noerrbars} }), width = .1,position = pd)

                    //tempoutput = tempoutput + " + \n\t geom_errorbar( aes(ymin =" + var + "-";
                    if (noerrbars != "0")
                    {
                        tempoutput = tempoutput + " + \n\t geom_errorbar( aes(ymin =" + var + "-";

                        if (stderr != "")
                        {
                            tempoutput += stderr;
                        }

                        if (stddev != "")
                        {
                            tempoutput += stddev;
                        }

                        if (confinterval != "")
                        {
                            tempoutput += confinterval;
                        }

                        //   if (noerrbars != "")
                        //  {
                        //     tempoutput += noerrbars;
                        // }


                        tempoutput = tempoutput + ", ymax =" + var + "+";

                        if (stderr != "")
                        {
                            tempoutput += stderr;
                        }

                        if (stddev != "")
                        {
                            tempoutput += stddev;
                        }

                        if (confinterval != "")
                        {
                            tempoutput += confinterval;
                        }

                        //     if (noerrbars != "")
                        //    {
                        //       tempoutput += noerrbars;
                        // }

                        tempoutput += " ), width = .1, position = pd)";

                        // tempoutput += " + \n\t geom_line(position = pd";
                    }

                    

                    tempoutput += " + \n\t geom_line(position = pd";

                    if (opacity != "")
                    {
                        tempoutput = tempoutput + ",alpha=" + opacity;
                    }

                    tempoutput += ")  +\n\t geom_point(position = pd) ";

                    // tempoutput = tempoutput + ")";
                    // End of heom_histogram

                    if (flipaxis == "TRUE")
                    {
                        tempoutput = tempoutput + " +\n\t coord_flip()";
                    }

                    //    labs(x = "{{xaxis}}", y = vars, fill = "{{Groupby}}", title = paste("Plot for variable ", vars, sep = ''))

                    tempoutput = tempoutput + " +\n\t labs(x =" + "\"" + xaxis + "\"" + ", y =" + "\"" + var + "\"" + ",fill = " + "\"" + Groupby + "\"" + ", title= " + "\"Plot of means for variable " + var + " by variable " + xaxis + "\")";

                    if (xlab != "")
                    {
                        tempoutput = tempoutput + " +\n\t xlab(" + "\"" + xlab + "\"" + ")";
                    }

                    if (ylab != "")
                    {
                        tempoutput = tempoutput + " +\n\t ylab(" + "\"" + ylab + "\"" + ")";
                    }

                    if (maintitle != "")
                    {
                        tempoutput = tempoutput + " +\n\t ggtitle(" + "\"" + maintitle + "\"" + ")";
                    }

                    //+geom_smooth(method ="{{sm}}", color= "{{color}}")

                    if (!(string.IsNullOrEmpty(Facetcolumn) && string.IsNullOrEmpty(Facetrow) && string.IsNullOrEmpty(Facetwrap)))
                    {
                        int count = 0;

                        if (!(Facetcolumn == null || Facetcolumn == ""))
                            count++;

                        if (!(Facetrow == null || Facetrow == ""))
                            count++;

                        if (!(Facetwrap == null || Facetwrap == ""))
                            count++;

                        if (count > 1)
                        {
                            string msg = "Error: You have specified more than one facet. You can only specify one facet at a time (namely one of Facetrow, Facetcol, Facetwrap).";
                            MessageBox.Show(msg);
                            return string.Empty;
                        }
                    }


                    tempoutput = tempoutput + createfacets(Facetwrap, Facetcolumn, Facetrow, Facetscale);
                    // tempoutput = Wrapinbrackets(tempoutput);
                    tempoutput = tempoutput + "\n\n";
                    output = output + tempoutput;

                    tempoutput = "";

                    output = output.TrimEnd(Environment.NewLine.ToCharArray());
                    output = output.TrimEnd(Environment.NewLine.ToCharArray());
                    output = output + " +\n" + themeSyntax + "\n\n";
                }

                //+facet_grid({ { Facetcolumn} } ~ {{ Facetrow} }, scales ={ { Facetscale} })  +facet_wrap(  { { Facetwrap} } )
            }

            else if (customsyntax == "Graphics-barplotmeans")
            {
                MatchCollection mcol = re.Matches(commandformat);
                foreach (Match m in mcol)
                {
                    string matchedText = m.Groups[1].Value;
                    string result = GetParam(obj, matchedText);
                    if (!CommandKeyValDict.ContainsKey(matchedText))
                    {
                        CommandKeyValDict.Add(matchedText, result);
                    }
                }
                string hide = "";
                string barcolor = "";
                string binwidth = "";
                string rdgrp1 = "";
                string opacity = "";

                string Facetcolumn = "";
                string Facetrow = "";
                string Facetscale = "";
                string Facetwrap = "";

                string color = "";
                string dataset = "";
                string xlab = "";
                string ylab = "";
                string maintitle = "";
                string jitter = "";
                string flipaxis = "";
                string xaxis = "";

                string bordercolor = "";
                string Groupby = "";
                string GroupbyCommandSection = string.Empty;
                string conflevel = "";
                string stddev = "";
                string confinterval = "";
                string noerrbars = "";
                string stderr = "";
                string yaxis = "";
                //  { { stddev} }
                // { { confinterval} }
                // { { noerrbars} }

                foreach (KeyValuePair<string, string> kv in CommandKeyValDict)
                {
                    string key = kv.Key;
                    string value = kv.Value;
                    //create final syntac in 'output'
                    // output = output+","+ key + "=c(" + value + ")";

                    if (key == "Groupby")
                    {
                        Groupby = value;
                        if (string.IsNullOrEmpty(Groupby))
                        {
                            GroupbyCommandSection = string.Empty;
                        }
                        else
                        {
                            GroupbyCommandSection = ",\"" + Groupby + "\"";
                        }
                    }


                    if (key == "stderr")
                    {
                        stderr = value;
                    }

                    if (key == "stddev")
                    {
                        stddev = value;
                    }

                    if (key == "confinterval")
                    {
                        confinterval = value;
                    }
                    if (key == "conflevel")
                    {
                        conflevel = value;
                    }

                    if (key == "noerrbars")
                    {
                        noerrbars = value;
                    }

                    if (key == "barcolor")
                    {
                        barcolor = value;
                    }

                    if (key == "xaxis")
                    {
                        xaxis = value;
                    }


                    if (key == "yaxis")
                    {
                        yaxis = value;
                    }
                    if (key == "binwidth")
                    {
                        binwidth = value;
                    }

                    if (key == "opacity")
                    {
                        opacity = value;
                    }
                    if (key == "Facetcolumn")
                    {
                        Facetcolumn = value;
                    }
                    if (key == "Facetrow")
                    {
                        Facetrow = value;
                    }
                    if (key == "Facetscale")
                    {
                        Facetscale = value;
                    }
                    if (key == "Facetwrap")
                    {
                        Facetwrap = value;
                    }
                    if (key == "%DATASET%")
                    {
                        dataset = value;
                    }
                    if (key == "xlab")
                    {
                        xlab = value;
                    }
                    if (key == "ylab")
                    {
                        ylab = value;
                    }
                    if (key == "maintitle")
                    {
                        maintitle = value;
                    }

                    if (key == "jitter")
                    {
                        jitter = value;
                    }
                    if (key == "flipaxis")
                    {
                        flipaxis = value;
                    }
                    if (key == "bordercolor")
                    {
                        bordercolor = value;
                    }
                    if (key == "color")
                    {
                        color = value;
                    }
                    if (key == "rdgrp1")
                    {
                        rdgrp1 = value;
                    }
                    if (key == "hide")
                    {
                        hide = value;
                    }
                }

                string tempoutput = "";
                string[] variables = yaxis.Split(',');
                string tempFacets = "";


                //  if (!(Facetcolumn == "" || Facetrow == "" || Facetwrap == "" || Facetcolumn == null || Facetrow == null || Facetwrap == null))
                if (!( string.IsNullOrEmpty(Facetcolumn) && string.IsNullOrEmpty(Facetrow) && string.IsNullOrEmpty(Facetwrap) ))
                {
                    int count = 0;
                   // tempFacets += "c(";
                    if (!(Facetcolumn == null || Facetcolumn == ""))
                    {
                        // tempFacets +=  "\"" + Facetcolumn + "\"";
                        tempFacets += Facetcolumn;
                        count++;
                    }

                    if (!(Facetrow == null || Facetrow == ""))
                    {
                        // tempFacets += "\"" + Facetrow + "\"";
                        tempFacets += Facetrow;
                        count++;
                    }

                    if (! (Facetwrap == null || Facetwrap == ""))
                    {
                        //tempFacets += "\"" + Facetwrap + "\"";
                        tempFacets += Facetwrap;
                        count++;
                    }
                    // tempFacets += ")";
                    if (count > 1)
                    {
                        string msg = "Error: Two or more facets selected. Select only one of the Facetrow, Facetcol or Facetwrap.";
                        MessageBox.Show(msg);
                        return string.Empty;
                    }
                }

                foreach (string var in variables)
                {
                    //# Create a dataset of summaries
                    //  { { datasetForSum} } <- summarySE({ {% DATASET %} }, measurevar = vars, groupvars = c("{{xaxis}}", "{{groupby}}"),conf.interval = { { conflevel} },na.rm = TRUE)
                    //string tempdataset = "";

                    if (tempFacets == "")
                    {
                        tempoutput += "temp <-Rmisc::summarySE( " + dataset + ", measurevar = " + "\"" + var + "\"" + ", groupvars = c(" + "\"" + xaxis + "\"" + GroupbyCommandSection + ")" + ",conf.interval = " + conflevel + ",na.rm = TRUE,.drop = TRUE)";
                    }
                    else
                    {
                        tempoutput += "temp <-Rmisc::summarySE( " + dataset + ", measurevar = " + "\"" + var + "\"" + ", groupvars = c(" + "\"" + xaxis + "\"" + GroupbyCommandSection + ",\"" + tempFacets  +"\"" + ")" + ",conf.interval = " + conflevel + ",na.rm = TRUE,.drop = TRUE)";
                    }

                    tempoutput = tempoutput + "\n";

                    //  ggplot({ { datasetForSum} }, aes(x ={ { xaxis} }, y = eval(parse(text = paste(vars))), colour ={ { groupby} },group ={ { groupby} })) +geom_errorbar(aes(ymin = eval(parse(text = paste(vars))) -{ { stderr} }
                    //  { { stddev} }
                    // { { confinterval} }
                    // { { noerrbars} }, ymax = eval(parse(text = paste(vars))) +{ { stderr} }
                    // { { stddev} }
                    // { { confinterval} }
                    // { { noerrbars} }), width = .1,position = pd) +geom_line(position = pd) + geom_point(position = pd) + labs(x = "{{xaxis}}", y = vars, fill = "{{Groupby}}", title = paste("Plot for variable ", vars, sep = '')) + xlab("{{xlab}}") + ylab("{{ylab}}" + ggtitle("{{maintitle}}") { { themes} })+facet_grid({ { yfacet} }
                    // ~{ { xfacet} })

                    tempoutput += "pd <- position_dodge(0.9)";
                    tempoutput = tempoutput + "\n";

                    tempoutput = tempoutput + "ggplot(data=temp" + ", aes(x = as.numeric(" + xaxis + ")";

                    tempoutput += ", y = " + var;

                    if (Groupby != "")
                    {
                        tempoutput += ", fill = " + Groupby;
                    }
                    //if (Groupby != "")
                    //{
                    //    tempoutput += ", group = " + Groupby;
                    //}

                    tempoutput = tempoutput + "))";

                    //Constructing geom_bar

                    if (hide == "FALSE")
                    {

                        tempoutput = tempoutput + " +\n\t geom_bar( position=\"dodge\" ";

                        if (opacity != "")
                        {
                            tempoutput = tempoutput + ",alpha=" + opacity;
                        }

                        if (barcolor != "")
                        {
                            tempoutput = tempoutput + ",fill =" + "\"" + barcolor + "\"";
                        }

                        tempoutput = tempoutput + " ,stat=\"identity\"";

                        tempoutput = tempoutput + ")";


                        // +geom_errorbar(aes(ymin = eval(parse(text = paste(vars))) -{ { stderr} }
                        //  { { stddev} }
                        // { { confinterval} }
                        // { { noerrbars} }, ymax = eval(parse(text = paste(vars))) +{ { stderr} }
                        // { { stddev} }
                        // { { confinterval} }
                        // { { noerrbars} }), width = .1,position = pd)

                        if (noerrbars != "0")
                        {
                            tempoutput = tempoutput + " + \n\t geom_errorbar( aes(ymin =" + var + "-";

                            if (stderr != "")
                            {
                                tempoutput += stderr;
                            }

                            if (stddev != "")
                            {
                                tempoutput += stddev;
                            }

                            if (confinterval != "")
                            {
                                tempoutput += confinterval;
                            }

                            //   if (noerrbars != "")
                            //  {
                            //     tempoutput += noerrbars;
                            // }


                            tempoutput = tempoutput + ", ymax =" + var + "+";

                            if (stderr != "")
                            {
                                tempoutput += stderr;
                            }

                            if (stddev != "")
                            {
                                tempoutput += stddev;
                            }

                            if (confinterval != "")
                            {
                                tempoutput += confinterval;
                            }

                            //     if (noerrbars != "")
                            //    {
                            //       tempoutput += noerrbars;
                            // }

                            tempoutput += " ), width = .1, position = pd)";

                            // tempoutput += " + \n\t geom_line(position = pd";
                        }
                    }


                    if (hide =="TRUE")
                    {

                        if (noerrbars != "0")
                        {
                            tempoutput = tempoutput + " + \n\t geom_errorbar( aes(ymin =" + var + "-";

                            if (stderr != "")
                            {
                                tempoutput += stderr;
                            }

                            if (stddev != "")
                            {
                                tempoutput += stddev;
                            }

                            if (confinterval != "")
                            {
                                tempoutput += confinterval;
                            }

                            //   if (noerrbars != "")
                            //  {
                            //     tempoutput += noerrbars;
                            // }


                            tempoutput = tempoutput + ", ymax =" + var + "+";

                            if (stderr != "")
                            {
                                tempoutput += stderr;
                            }

                            if (stddev != "")
                            {
                                tempoutput += stddev;
                            }

                            if (confinterval != "")
                            {
                                tempoutput += confinterval;
                            }

                            //     if (noerrbars != "")
                            //    {
                            //       tempoutput += noerrbars;
                            // }

                            tempoutput += " ), width = .1, position = pd)";

                            // tempoutput += " + \n\t geom_line(position = pd";
                        }

                        tempoutput = tempoutput + " +\n\t geom_bar( position=\"dodge\" ";

                        if (opacity != "")
                        {
                            tempoutput = tempoutput + ",alpha=" + opacity;
                        }

                        if (barcolor != "")
                        {
                            tempoutput = tempoutput + ",fill =" + "\"" + barcolor + "\"";
                        }

                        tempoutput = tempoutput + " ,stat=\"identity\"";

                        tempoutput = tempoutput + ")";


                        // +geom_errorbar(aes(ymin = eval(parse(text = paste(vars))) -{ { stderr} }
                        //  { { stddev} }
                        // { { confinterval} }
                        // { { noerrbars} }, ymax = eval(parse(text = paste(vars))) +{ { stderr} }
                        // { { stddev} }
                        // { { confinterval} }
                        // { { noerrbars} }), width = .1,position = pd)



                    }





                    //if (opacity != "")
                    //{
                    //    tempoutput = tempoutput + ",alpha=" + opacity;
                    //}


                    //tempoutput += ")  +\n\t geom_point(position = pd) ";

                    // tempoutput = tempoutput + ")";
                    // End of heom_histogram

                    if (flipaxis == "TRUE")
                    {
                        tempoutput = tempoutput + " +\n\t coord_flip()";
                    }

                    //    labs(x = "{{xaxis}}", y = vars, fill = "{{Groupby}}", title = paste("Plot for variable ", vars, sep = ''))

                    tempoutput = tempoutput + " +\n\t labs(x =" + "\"" + xaxis + "\"" + ", y =" + "\"" + var + "\"" + ",fill = " + "\"" + Groupby + "\"" + ", title= " + "\"Bar Chart (with means) for Y axis variable " + var + "  , X axis variable " + xaxis + "\")";

                    if (xlab != "")
                    {
                        tempoutput = tempoutput + " +\n\t xlab(" + "\"" + xlab + "\"" + ")";
                    }

                    if (ylab != "")
                    {
                        tempoutput = tempoutput + " +\n\t ylab(" + "\"" + ylab + "\"" + ")";
                    }

                    if (maintitle != "")
                    {
                        tempoutput = tempoutput + " +\n\t ggtitle(" + "\"" + maintitle + "\"" + ")";
                    }

                    //+geom_smooth(method ="{{sm}}", color= "{{color}}")

                    tempoutput = tempoutput + createfacets(Facetwrap, Facetcolumn, Facetrow, Facetscale);
                    // tempoutput = Wrapinbrackets(tempoutput);
                    tempoutput = tempoutput + "\n\n";
                    output = output + tempoutput;

                    tempoutput = "";

                    output = output.TrimEnd(Environment.NewLine.ToCharArray());
                    output = output.TrimEnd(Environment.NewLine.ToCharArray());
                    output = output + " +\n" + themeSyntax + "\n\n";
                }

                //+facet_grid({ { Facetcolumn} } ~ {{ Facetrow} }, scales ={ { Facetscale} })  +facet_wrap(  { { Facetwrap} } )
            }

            else if (customsyntax == "Graphics-histogram")
            {
                //  ggplot(data ={ {% DATASET %} }, aes(eval(parse(text = paste(vars))))) +geom_histogram(col = "{{bordercolor}}",{ { binwidth} }, fill = "{{color}}", alpha = { { alpha} }) 
                //+labs(x = vars, y = "Counts", title = "{{maintitle}}", title = paste("Histogram for variable ", vars, sep = '')) + xlab("{{xlab}}") + ylab("{{ylab}}") + ggtitle("{{maintitle}}") { { themes} }

                MatchCollection mcol = re.Matches(commandformat);
                foreach (Match m in mcol)
                {
                    string matchedText = m.Groups[1].Value;
                    string result = GetParam(obj, matchedText);
                    if (!CommandKeyValDict.ContainsKey(matchedText))
                    {
                        CommandKeyValDict.Add(matchedText, result);
                    }
                }

                string destination = "";
                string binwidth = "";

                string opacity = "";

                string Facetcolumn = "";
                string Facetrow = "";
                string Facetscale = "";
                string Facetwrap = "";

                string color = "";
                string dataset = "";
                string xlab = "";
                string ylab = "";
                string maintitle = "";
                string jitter = "";
                string flipaxis = "";

                string bordercolor = "";

                foreach (KeyValuePair<string, string> kv in CommandKeyValDict)
                {
                    string key = kv.Key;
                    string value = kv.Value;
                    //create final syntac in 'output'
                    // output = output+","+ key + "=c(" + value + ")";

                    if (key == "destination")
                    {
                        destination = value;
                    }
                    if (key == "binwidth")
                    {
                        binwidth = value;
                    }

                    if (key == "opacity")
                    {
                        opacity = value;
                    }
                    if (key == "Facetcolumn")
                    {
                        Facetcolumn = value;
                    }
                    if (key == "Facetrow")
                    {
                        Facetrow = value;
                    }
                    if (key == "Facetscale")
                    {
                        Facetscale = value;
                    }
                    if (key == "Facetwrap")
                    {
                        Facetwrap = value;
                    }
                    if (key == "%DATASET%")
                    {
                        dataset = value;
                    }
                    if (key == "xlab")
                    {
                        xlab = value;
                    }
                    if (key == "ylab")
                    {
                        ylab = value;
                    }
                    if (key == "maintitle")
                    {
                        maintitle = value;
                    }

                    if (key == "jitter")
                    {
                        jitter = value;
                    }
                    if (key == "flipaxis")
                    {
                        flipaxis = value;
                    }
                    if (key == "bordercolor")
                    {
                        bordercolor = value;
                    }
                    if (key == "color")
                    {
                        color = value;
                    }
                }
                string tempoutput = "";
                string[] variables = destination.Split(',');

                foreach (string var in variables)
                {
                    tempoutput = tempoutput + "ggplot(data=" + dataset + ", aes(x =" + var;

                    tempoutput = tempoutput + "))";

                    //Constructing geom_Histogram
                    //  +geom_histogram(col = "{{bordercolor}}",{ { binwidth} }, fill = "{{color}}", alpha = { { alpha} }) 

                    tempoutput = tempoutput + " +\n\t geom_histogram(";

                    if (!(bordercolor == "" || bordercolor == null))
                    {

                        tempoutput = tempoutput + "col =" + "\"" + bordercolor + "\"";
                    }

                    if (binwidth != "0")
                    {

                        tempoutput = tempoutput + ",binwidth =" + binwidth;
                    }

                    if (!(color == "" || color == null))
                    {
                        tempoutput = tempoutput + ",fill =" + "\"" + color + "\"";
                    }
                    if (opacity != "")
                    {
                        tempoutput = tempoutput + ",alpha=" + opacity;
                    }

                    tempoutput = tempoutput + ")";
                    // End of heom_histogram

                    if (flipaxis == "TRUE")
                    {
                        tempoutput = tempoutput + " +\n\t coord_flip()";
                    }

                    //+labs(x = vars, y = "Counts", title = "{{maintitle}}", title = paste("Histogram for variable ", vars, sep = ''))
                    tempoutput = tempoutput + " +\n\t labs(x =" + "\"" + var + "\"" + ", y =" + "\"" + "Counts" + "\"" + ", title= " + "\"Histogram for variable " + var + "\")";

                    if (xlab != "")
                    {
                        tempoutput = tempoutput + " +\n\t xlab(" + "\"" + xlab + "\"" + ")";
                    }

                    if (ylab != "")
                    {
                        tempoutput = tempoutput + " +\n\t ylab(" + "\"" + ylab + "\"" + ")";
                    }

                    if (maintitle != "")
                    {
                        tempoutput = tempoutput + " +\n\t ggtitle(" + "\"" + maintitle + "\"" + ")";
                    }

                    //+geom_smooth(method ="{{sm}}", color= "{{color}}")

                    tempoutput = tempoutput + createfacets(Facetwrap, Facetcolumn, Facetrow, Facetscale);
                    // tempoutput = Wrapinbrackets(tempoutput);
                    tempoutput = tempoutput + "\n\n";
                    output = output + tempoutput;

                    tempoutput = "";

                    output = output.TrimEnd(Environment.NewLine.ToCharArray());
                    output = output.TrimEnd(Environment.NewLine.ToCharArray());
                    output = output + " +\n" + themeSyntax + "\n\n";
                }

                //+facet_grid({ { Facetcolumn} } ~ {{ Facetrow} }, scales ={ { Facetscale} })  +facet_wrap(  { { Facetwrap} } )
            }

            else if (customsyntax == "Graphics-ppplot")
            {
                MatchCollection mcol = re.Matches(commandformat);
                foreach (Match m in mcol)
                {
                    string matchedText = m.Groups[1].Value;
                    string result = GetParam(obj, matchedText);
                    if (!CommandKeyValDict.ContainsKey(matchedText))
                    {
                        CommandKeyValDict.Add(matchedText, result);
                    }
                }

                string destination = "";
                string GroupingVariable = "";
                string GroupBy = "";
                string alpha = "";

                string Facetcolumn = "";
                string Facetrow = "";
                string Facetscale = "";
                string Facetwrap = "";

                string color = "";
                string dataset = "";
                string xlab = "";
                string ylab = "";
                string maintitle = "";
                string jitter = "";
                string flipaxis = "";
                string referenceline = "";

                string detrend = "";
                string band = "";

                string distribution = "";
                string dparams = "";

                string bordercolor = "";

                foreach (KeyValuePair<string, string> kv in CommandKeyValDict)
                {
                    string key = kv.Key;
                    string value = kv.Value;
                    //create final syntac in 'output'
                    // output = output+","+ key + "=c(" + value + ")";

                    if (key == "band")
                    {
                        band = value;

                    }
                    if (key == "referenceline")
                    {
                        referenceline = value;

                    }
                    if (key == "destination")
                    {
                        destination = value;
                    }
                    if (key == "distribution")
                    {
                        distribution = value;
                    }
                    if (key == "dparams")
                    {
                        dparams = value;
                    }
                    if (key == "GroupingVariable")
                    {
                        GroupingVariable = value;
                    }
                    if (key == "GroupBy")
                    {
                        GroupBy = value;
                    }

                    if (key == "alpha")
                    {
                        alpha = value;
                    }
                    if (key == "Facetcolumn")
                    {
                        Facetcolumn = value;
                    }
                    if (key == "Facetrow")
                    {
                        Facetrow = value;
                    }
                    if (key == "Facetscale")
                    {
                        Facetscale = value;
                    }
                    if (key == "Facetwrap")
                    {
                        Facetwrap = value;
                    }
                    if (key == "%DATASET%")
                    {
                        dataset = value;
                    }
                    if (key == "xlab")
                    {
                        xlab = value;
                    }
                    if (key == "ylab")
                    {
                        ylab = value;
                    }
                    if (key == "maintitle")
                    {
                        maintitle = value;
                    }

                    if (key == "jitter")
                    {
                        jitter = value;
                    }
                    if (key == "flipaxis")
                    {
                        flipaxis = value;
                    }
                    if (key == "bordercolor")
                    {
                        bordercolor = value;
                    }

                    if (key == "color")
                    {
                        color = value;
                    }
                    if (key == "detrend")
                    {
                        detrend = value;
                    }
                }
                string tempoutput = "";
                string[] variables = destination.Split(',');

                foreach (string var in variables)
                {
                    //ggplot(data = { {% DATASET %} }, mapping = aes(sample = eval(parse(text = paste(vars))), shape = { { Groupby} }))
                    tempoutput = tempoutput + "ggplot(data=" + dataset + ", aes(sample = " + var + ", y = ";

                    if (GroupBy != "")
                    {
                        tempoutput = tempoutput + ",shape = " + GroupBy;
                    }

                    if (color != "")
                    {
                        tempoutput = tempoutput + ",color = " + color;
                    }

                    tempoutput = tempoutput + "))";

                    //end of aes

                    // tempoutput = tempoutput + " +\n\t stat_pp_band()  +\n\t stat_pp_line()  +\n\t stat_pp_point()";

                    if (band == "TRUE")
                    {

                        tempoutput += " +\n\t stat_pp_band(";
                        if (!(distribution == "" || distribution == null))
                        {
                            tempoutput += "distribution=" + "\"" + distribution + "\"";
                        }
                        if (!(dparams == "" || dparams == null))
                        {
                            tempoutput += ",dparams= list(" + dparams + ")";
                        }

                        if (detrend == "TRUE")
                        {
                            tempoutput += ",detrend = TRUE";
                        }

                        tempoutput += ")";
                    }

                    if (referenceline == "TRUE")
                    {

                        tempoutput += " +\n\t stat_pp_line(";
                        if (!(distribution == "" || distribution == null))
                        {
                            tempoutput += "distribution=" + "\"" + distribution + "\"";
                        }
                        if (!(dparams == "" || dparams == null))
                        {
                            tempoutput += ",dparams= list(" + dparams + ")";
                        }

                        if (detrend == "TRUE")
                        {
                            tempoutput += ",detrend = TRUE";
                        }

                        tempoutput += ")";
                    }

                    tempoutput += " +\n\t stat_pp_point(";
                    if (!(distribution == "" || distribution == null))
                    {
                        tempoutput += "distribution=" + "\"" + distribution + "\"";
                    }
                    if (!(dparams == "" || dparams == null))
                    {
                        tempoutput += ",dparams= list(" + dparams + ")";
                    }

                    if (detrend == "TRUE")
                    {
                        tempoutput += ",detrend = TRUE";
                    }

                    tempoutput += ")";

                    if (flipaxis == "TRUE")
                    {
                        tempoutput = tempoutput + " +\n\t coord_flip()";
                    }

                    //+labs(x = "Theoretical Quantiles", y = "Sample Quantiles", title = paste("pp Plot for variable ", vars, sep = '')) + xlab("{{xlab}}") + ylab("{{ylab}}") + ggtitle("{{maintitle}}") { { themes} }
                    tempoutput = tempoutput + " +\n\t labs(x = \"Probability Points\"" + ", y =" + "\"Cumulative Probability\", title = " + "\"PP Plot for variable " + var + "\")";

                    if (xlab != "")
                    {
                        tempoutput = tempoutput + " +\n\t xlab(" + "\"" + xlab + "\"" + ")";
                    }

                    if (ylab != "")
                    {
                        tempoutput = tempoutput + " +\n\t ylab(" + "\"" + ylab + "\"" + ")";
                    }

                    if (maintitle != "")
                    {
                        tempoutput = tempoutput + " +\n\t ggtitle(" + "\"" + maintitle + "\"" + ")";
                    }

                    //+geom_smooth(method ="{{sm}}", color= "{{color}}")

                    tempoutput = tempoutput + createfacets(Facetwrap, Facetcolumn, Facetrow, Facetscale);
                    // tempoutput = Wrapinbrackets(tempoutput);
                    tempoutput = tempoutput + "\n\n";
                    output = output + tempoutput;

                    tempoutput = "";

                    output = output.TrimEnd(Environment.NewLine.ToCharArray());
                    output = output.TrimEnd(Environment.NewLine.ToCharArray());
                    output = output + " +\n" + themeSyntax + "\n\n";
                }

                //+facet_grid({ { Facetcolumn} } ~ {{ Facetrow} }, scales ={ { Facetscale} })  +facet_wrap(  { { Facetwrap} } )
            }

            // print(ggplot(data = { {% DATASET %} }, mapping = aes(sample = eval(parse(text = paste(vars))), shape = { { Groupby} }))  +stat_qq_band() + stat_qq_line() + stat_qq_point() { { flip} }
            //+labs(x = "Theoretical Quantiles", y = "Sample Quantiles", title = paste("QQ Plot for variable ", vars, sep = '')) + xlab("{{xlab}}") + ylab("{{ylab}}") + ggtitle("{{maintitle}}") { { themes} }
            //+facet_grid({ { yfacet} }
            //~{ { xfacet} }))

            else if (customsyntax == "Graphics-qqplot")
            {
                MatchCollection mcol = re.Matches(commandformat);
                foreach (Match m in mcol)
                {
                    string matchedText = m.Groups[1].Value;
                    string result = GetParam(obj, matchedText);
                    if (!CommandKeyValDict.ContainsKey(matchedText))
                    {
                        CommandKeyValDict.Add(matchedText, result);
                    }
                }

                string destination = "";
                string GroupingVariable = "";
                string GroupBy = "";
                string alpha = "";

                string Facetcolumn = "";
                string Facetrow = "";
                string Facetscale = "";
                string Facetwrap = "";

                string color = "";
                string dataset = "";
                string xlab = "";
                string ylab = "";
                string maintitle = "";
                string jitter = "";
                string flipaxis = "";
                string referenceline = "";

                string detrend = "";
                string band = "";

                string distribution = "";
                string dparams = "";

                string bordercolor = "";

                foreach (KeyValuePair<string, string> kv in CommandKeyValDict)
                {
                    string key = kv.Key;
                    string value = kv.Value;
                    //create final syntac in 'output'
                    // output = output+","+ key + "=c(" + value + ")";

                    if (key == "band")
                    {
                        band = value;

                    }
                    if (key == "referenceline")
                    {
                        referenceline = value;

                    }
                    if (key == "destination")
                    {
                        destination = value;
                    }
                    if (key == "distribution")
                    {
                        distribution = value;
                    }
                    if (key == "dparams")
                    {
                        dparams = value;
                    }
                    if (key == "GroupingVariable")
                    {
                        GroupingVariable = value;
                    }
                    if (key == "GroupBy")
                    {
                        GroupBy = value;
                    }

                    if (key == "alpha")
                    {
                        alpha = value;
                    }
                    if (key == "Facetcolumn")
                    {
                        Facetcolumn = value;
                    }
                    if (key == "Facetrow")
                    {
                        Facetrow = value;
                    }
                    if (key == "Facetscale")
                    {
                        Facetscale = value;
                    }
                    if (key == "Facetwrap")
                    {
                        Facetwrap = value;
                    }
                    if (key == "%DATASET%")
                    {
                        dataset = value;
                    }
                    if (key == "xlab")
                    {
                        xlab = value;
                    }
                    if (key == "ylab")
                    {
                        ylab = value;
                    }
                    if (key == "maintitle")
                    {
                        maintitle = value;
                    }

                    if (key == "jitter")
                    {
                        jitter = value;
                    }
                    if (key == "flipaxis")
                    {
                        flipaxis = value;
                    }
                    if (key == "bordercolor")
                    {
                        bordercolor = value;
                    }

                    if (key == "color")
                    {
                        color = value;
                    }
                    if (key == "detrend")
                    {
                        detrend = value;
                    }
                }
                string tempoutput = "";
                string[] variables = destination.Split(',');

                foreach (string var in variables)
                {

                    //ggplot(data = { {% DATASET %} }, mapping = aes(sample = eval(parse(text = paste(vars))), shape = { { Groupby} }))
                    tempoutput = tempoutput + "ggplot(data=" + dataset + ", aes(sample = " + var + ", y = ";

                    if (GroupBy != "")
                    {
                        tempoutput = tempoutput + ",shape = " + GroupBy;
                    }

                    if (color != "")
                    {
                        tempoutput = tempoutput + ",color = " + color;
                    }

                    tempoutput = tempoutput + "))";

                    //end of aes

                    // tempoutput = tempoutput + " +\n\t stat_qq_band()  +\n\t stat_qq_line()  +\n\t stat_qq_point()";

                    if (band == "TRUE")
                    {
                        tempoutput += " +\n\t stat_qq_band(";
                        if (!(distribution == "" || distribution == null))
                        {
                            tempoutput += "distribution=" + "\"" + distribution + "\"";
                        }
                        if (!(dparams == "" || dparams == null))
                        {
                            tempoutput += ",dparams= list(" + dparams + ")";
                        }

                        if (detrend == "TRUE")
                        {
                            tempoutput += ",detrend = TRUE";
                        }

                        tempoutput += ")";
                    }

                    if (referenceline == "TRUE")
                    {

                        tempoutput += " +\n\t stat_qq_line(";
                        if (!(distribution == "" || distribution == null))
                        {
                            tempoutput += "distribution=" + "\"" + distribution + "\"";
                        }
                        if (!(dparams == "" || dparams == null))
                        {
                            tempoutput += ",dparams= list(" + dparams + ")";
                        }

                        if (detrend == "TRUE")
                        {
                            tempoutput += ",detrend = TRUE";
                        }

                        tempoutput += ")";
                    }

                    tempoutput += " +\n\t stat_qq_point(";
                    if (!(distribution == "" || distribution == null))
                    {
                        tempoutput += "distribution=" + "\"" + distribution + "\"";
                    }
                    if (!(dparams == "" || dparams == null))
                    {
                        tempoutput += ",dparams= list(" + dparams + ")";
                    }

                    if (detrend == "TRUE")
                    {
                        tempoutput += ",detrend = TRUE";
                    }

                    tempoutput += ")";

                    if (flipaxis == "TRUE")
                    {
                        tempoutput = tempoutput + " +\n\t coord_flip()";
                    }

                    //+labs(x = "Theoretical Quantiles", y = "Sample Quantiles", title = paste("qq Plot for variable ", vars, sep = '')) + xlab("{{xlab}}") + ylab("{{ylab}}") + ggtitle("{{maintitle}}") { { themes} }
                    tempoutput = tempoutput + " +\n\t labs(x = \"Theoretical Quantiles\"" + ", y =" + "\"Sample Quantiles\", title = " + "\"QQ Plot for variable " + var + "\")";

                    if (xlab != "")
                    {
                        tempoutput = tempoutput + " +\n\t xlab(" + "\"" + xlab + "\"" + ")";
                    }

                    if (ylab != "")
                    {
                        tempoutput = tempoutput + " +\n\t ylab(" + "\"" + ylab + "\"" + ")";
                    }

                    if (maintitle != "")
                    {
                        tempoutput = tempoutput + " +\n\t ggtitle(" + "\"" + maintitle + "\"" + ")";
                    }

                    //+geom_smooth(method ="{{sm}}", color= "{{color}}")

                    tempoutput = tempoutput + createfacets(Facetwrap, Facetcolumn, Facetrow, Facetscale);
                    // tempoutput = Wrapinbrackets(tempoutput);
                    tempoutput = tempoutput + "\n\n";
                    output = output + tempoutput;

                    tempoutput = "";

                    output = output.TrimEnd(Environment.NewLine.ToCharArray());
                    output = output.TrimEnd(Environment.NewLine.ToCharArray());
                    output = output + " +\n" + themeSyntax +"\n\n";
                }

                //+facet_grid({ { Facetcolumn} } ~ {{ Facetrow} }, scales ={ { Facetscale} })  +facet_wrap(  { { Facetwrap} } )
            }

            //  ggplot({ {% DATASET %} }, aes(x = { { GroupingVariable} }, y = eval(parse(text = paste(vars))),fill = { { GroupBy} })) +geom_boxplot(fill = "{{barcolor}}")  { { flipAxes} }
            //   +labs(x = "{{GroupingVariable}}", y = vars, fill = "{{GroupBy}}", title = paste("Box plot for variable ", vars, sep = '')) + xlab("{{xlab}}") + ylab("{{ylab}}") + ggtitle("{{maintitle}}") { { themes} }

            else if (customsyntax == "Graphics-boxplot")
            {
                MatchCollection mcol = re.Matches(commandformat);
                foreach (Match m in mcol)
                {
                    string matchedText = m.Groups[1].Value;
                    string result = GetParam(obj, matchedText);
                    if (!CommandKeyValDict.ContainsKey(matchedText))
                    {
                        CommandKeyValDict.Add(matchedText, result);
                    }
                }

                string destination = "";
                string GroupingVariable = "";
                string GroupBy = "";
                string alpha = "";

                string Facetcolumn = "";
                string Facetrow = "";
                string Facetscale = "";
                string Facetwrap = "";

                string color = "";
                string dataset = "";
                string xlab = "";
                string ylab = "";
                string maintitle = "";
                string jitter = "";
                string flipaxis = "";
                string barcolor = "";
                bool addcomma = false;
                string outliers = "";
                string plotDataPoints = "";
                string notch = "";

                string bordercolor = "";

                foreach (KeyValuePair<string, string> kv in CommandKeyValDict)
                {
                    string key = kv.Key;
                    string value = kv.Value;
                    //create final syntac in 'output'
                    // output = output+","+ key + "=c(" + value + ")";

                    if (key == "plotDataPoints")
                    {
                        plotDataPoints = value;

                    }
                    if (key == "outliers")
                    {
                        outliers = value;

                    }
                    if (key == "destination")
                    {
                        destination = value;
                    }
                    if (key == "GroupingVariable")
                    {
                        GroupingVariable = value;
                    }
                    if (key == "GroupBy")
                    {
                        GroupBy = value;
                    }

                    if (key == "alpha")
                    {
                        alpha = value;
                    }
                    if (key == "Facetcolumn")
                    {
                        Facetcolumn = value;
                    }
                    if (key == "Facetrow")
                    {
                        Facetrow = value;
                    }
                    if (key == "Facetscale")
                    {
                        Facetscale = value;
                    }
                    if (key == "Facetwrap")
                    {
                        Facetwrap = value;
                    }
                    if (key == "%DATASET%")
                    {
                        dataset = value;
                    }
                    if (key == "xlab")
                    {
                        xlab = value;
                    }
                    if (key == "ylab")
                    {
                        ylab = value;
                    }
                    if (key == "maintitle")
                    {
                        maintitle = value;
                    }

                    if (key == "jitter")
                    {
                        jitter = value;
                    }
                    if (key == "flipaxis")
                    {
                        flipaxis = value;
                    }
                    if (key == "bordercolor")
                    {
                        bordercolor = value;
                    }
                    if (key == "barcolor")
                    {
                        barcolor = value;
                    }
                    if (key == "color")
                    {
                        color = value;
                    }
                    if (key == "notch")
                    {
                        notch = value;
                    }
                }

                string varTitle = string.Empty;
                string grpTitle = string.Empty;
                string fillTitle = string.Empty;

                string tempoutput = "";
                string[] variables = destination.Split(',');

                foreach (string var in variables)
                {
                    //ggplot({ {% DATASET %} }, aes(x = { { GroupingVariable} }, y = eval(parse(text = paste(vars))),fill = { { GroupBy} }))

                    tempoutput = tempoutput + "ggplot(data=" + dataset + ", aes(x =" + GroupingVariable + ", y = " + var;

                    if (GroupBy != "")
                    {
                        tempoutput = tempoutput + ",fill = " + GroupBy;
                    }

                    tempoutput = tempoutput + "))";

                    //Construct boxplot
                    //    +geom_boxplot(fill = "{{barcolor}}")  { { flipAxes} }


                    tempoutput = tempoutput + " +\n\t geom_boxplot(";

                    if (!(barcolor == "" || barcolor == null))
                    {
                        tempoutput = tempoutput + "col =" + "\"" + barcolor + "\"";
                        addcomma = true;
                    }

                    if (outliers == "TRUE")
                    {
                        if (addcomma == true)
                        {
                            tempoutput = tempoutput + ",";
                        }
                        addcomma = true;
                        tempoutput = tempoutput + "outlier.colour= \"red \", outlier.shape=8, outlier.size = 2";
                    }

                    if (alpha != "")
                    {
                        if (addcomma == true)
                        {
                            tempoutput = tempoutput + ",";
                        }
                        addcomma = true;
                        tempoutput = tempoutput + "alpha =" + alpha;
                    }

                    if (notch == "TRUE")
                    {
                        if (addcomma == true)
                        {
                            tempoutput = tempoutput + ",";
                        }
                        addcomma = true;
                        tempoutput = tempoutput + "notch = TRUE";
                    }

                    tempoutput = tempoutput + ")";

                    // End of geomboxplot

                    if (plotDataPoints == "Stacked")
                    {
                        tempoutput = tempoutput + " +\n\t geom_dotplot(binaxis = 'y', stackdir = 'center', dotsize = 0.1)";

                    }
                    else if (plotDataPoints == "Jitter")
                    {
                        tempoutput = tempoutput + " +\n\t geom_jitter(shape = 16, position = position_jitter(0.2))";

                    }

                    if (flipaxis == "TRUE")
                    {
                        tempoutput = tempoutput + " +\n\t coord_flip()";
                    }

                    varTitle = string.Empty;
                    if (var != null && var.Trim().Length > 0)
                        varTitle = "Boxplot for variable " + var ;

                    grpTitle = string.Empty;
                    if (GroupingVariable != null && GroupingVariable.Trim().Length > 0)
                        grpTitle = ", group by " + GroupingVariable;

                    fillTitle = string.Empty;
                    if (GroupBy != null && GroupBy.Trim().Length > 0)
                        fillTitle = ", filled by " + GroupBy;

                    //+labs(x = vars, y = "Counts", title = "{{maintitle}}", title = paste("Histogram for variable ", vars, sep = ''))
                    tempoutput = tempoutput + " +\n\t labs(x =" + "\"" + GroupingVariable + "\"" + ", y =" + "\"" + var + "\"" + ", title= " + "\""+varTitle + grpTitle + fillTitle + "\")";

                    if (xlab != "")
                    {
                        tempoutput = tempoutput + " +\n\t xlab(" + "\"" + xlab + "\"" + ")";
                    }

                    if (ylab != "")
                    {
                        tempoutput = tempoutput + " +\n\t ylab(" + "\"" + ylab + "\"" + ")";
                    }

                    if (maintitle != "")
                    {
                        tempoutput = tempoutput + " +\n\t ggtitle(" + "\"" + maintitle + "\"" + ")";
                    }

                    //+geom_smooth(method ="{{sm}}", color= "{{color}}")

                    tempoutput = tempoutput + createfacets(Facetwrap, Facetcolumn, Facetrow, Facetscale);
                    // tempoutput = Wrapinbrackets(tempoutput);
                    tempoutput = tempoutput + "\n\n";
                    output = output + tempoutput;

                    tempoutput = "";

                    output = output.TrimEnd(Environment.NewLine.ToCharArray());
                    output = output.TrimEnd(Environment.NewLine.ToCharArray());
                    output = output + " +\n" + themeSyntax + "\n\n";
                }

                //+facet_grid({ { Facetcolumn} } ~ {{ Facetrow} }, scales ={ { Facetscale} })  +facet_wrap(  { { Facetwrap} } )
            }

            //   ggplot({ {% DATASET %} }, aes(x ={ { xaxis} }, y = eval(parse(text = paste(vars))), colour ={ { groupby} }, group ={ { groupby} })) +geom_line() + geom_point() + labs(x = "{{xaxis}}", y = vars, colour = "{{Groupby}}", title = paste("Plot for variable ", vars, sep = '')) + xlab("{{xlab}}") + ylab("{{ylab}}") + ggtitle("{{maintitle}}") { { themes} }
            //  +facet_grid({ { yfacet} }
            // ~{ { xfacet} })

            else if (customsyntax == "Graphics-linechart(xaxis)")
            {
                MatchCollection mcol = re.Matches(commandformat);
                foreach (Match m in mcol)
                {
                    string matchedText = m.Groups[1].Value;
                    string result = GetParam(obj, matchedText);
                    if (!CommandKeyValDict.ContainsKey(matchedText))
                    {
                        CommandKeyValDict.Add(matchedText, result);
                    }
                }

                string xaxis = "";
                string yaxis = "";
                string groupby = "";
                string shape = "";
                string opacity = "";

                string Facetcolumn = "";
                string Facetrow = "";
                string Facetscale = "";
                string Facetwrap = "";
                string flipaxis = "";


                string dataset = "";
                string xlab = "";
                string ylab = "";
                string maintitle = "";

                foreach (KeyValuePair<string, string> kv in CommandKeyValDict)
                {
                    string key = kv.Key;
                    string value = kv.Value;
                    //create final syntac in 'output'
                    // output = output+","+ key + "=c(" + value + ")";

                    if (key == "xaxis")
                    {
                        xaxis = value;
                    }

                    if (key == "yaxis")
                    {
                        yaxis = value;
                    }
                    if (key == "groupby")
                    {
                        groupby = value;
                    }
                    if (key == "shape")
                    {
                        shape = value;
                    }

                    if (key == "opacity")
                    {
                        opacity = value;
                    }
                    if (key == "Facetcolumn")
                    {
                        Facetcolumn = value;
                    }
                    if (key == "Facetrow")
                    {
                        Facetrow = value;
                    }
                    if (key == "Facetscale")
                    {
                        Facetscale = value;
                    }
                    if (key == "Facetwrap")
                    {
                        Facetwrap = value;
                    }
                    if (key == "%DATASET%")
                    {
                        dataset = value;
                    }
                    if (key == "xlab")
                    {
                        xlab = value;
                    }
                    if (key == "ylab")
                    {
                        ylab = value;
                    }
                    if (key == "maintitle")
                    {
                        maintitle = value;
                    }
                }
                string tempoutput = "";
                bool addcomma = false;
                string[] variables = yaxis.Split(',');

                foreach (string var in variables)
                {
                    //   ggplot(data = { {% DATASET %} }, aes(x = eval(parse(text = paste(vars))), fill = { { Groupby} })) 
                    tempoutput = tempoutput + "ggplot(data=" + dataset + ", aes(x =" + xaxis + ", y =" + var + "))";

                    //+geom_density(position = "{{rdgrp1}}", fill = "{{barcolor}}", alpha = 0.5)
                    tempoutput = tempoutput + " +\n\t geom_line( stat = \"identity\", position = \"identity\"";

                    if (groupby != "" || shape != "")
                    {

                        tempoutput += ", aes (";

                        if (groupby != "")

                        {
                            tempoutput = tempoutput + "color = " + groupby;
                            addcomma = true;
                        }


                        if (shape != "")
                        {
                            if (!addcomma)
                            {
                                tempoutput = tempoutput + ", shape = " + shape;
                            }
                            else
                            {
                                tempoutput = tempoutput + " shape = " + shape;
                            }
                        }

                        tempoutput = tempoutput + ")";
                    }

                    if (opacity != "")
                    {
                        tempoutput = tempoutput + ", alpha=" + opacity;
                    }

                    tempoutput = tempoutput + ")";

                    if (flipaxis == "TRUE")
                    {
                        tempoutput = tempoutput + " +\n\t coord_flip()";
                    }

                    //   +labs(x = vars, y = "Count", fill = "{{Groupby}}", title = paste("Density plot for variable ", vars, sep = '')) +

                    if (groupby != "")
                    {
                        tempoutput = tempoutput + " +\n\t labs(x =" + "\"" + xaxis + "\"" + ", y =" + "\"" + var + "\"" + ", title= " + "\"Line chart (observations connected by order of values on x axis) \\nfor X axis variable: " + xaxis + ", Y axis variable: " + var + ", grouped in colors by \\nvariable: " + groupby + "\")";
                    }
                    else
                    {
                        tempoutput = tempoutput + " +\n\t labs(x =" + "\"" + xaxis + "\"" + ", y =" + "\"" + var + "\"" + ", title= " + "\"Line chart (observations connected by order of values on x axis) \\nfor X axis variable: " + xaxis + ", Y axis variable: " + var + "\")";
                    }

                    if (xlab != "")
                    {
                        tempoutput = tempoutput + " +\n\t xlab(" + "\"" + xlab + "\"" + ")";
                    }

                    if (ylab != "")
                    {
                        tempoutput = tempoutput + " +\n\t ylab(" + "\"" + ylab + "\"" + ")";
                    }

                    if (maintitle != "")
                    {
                        tempoutput = tempoutput + " +\n\t ggtitle(" + "\"" + maintitle + "\"" + ")";
                    }

                    tempoutput = tempoutput + createfacets(Facetwrap, Facetcolumn, Facetrow, Facetscale);
                    // tempoutput = Wrapinbrackets(tempoutput);
                    tempoutput = tempoutput + "\n\n";
                    output = output + tempoutput;

                    tempoutput = "";

                    output = output.TrimEnd(Environment.NewLine.ToCharArray());
                    output = output.TrimEnd(Environment.NewLine.ToCharArray());
                    output = output + " +\n" + themeSyntax + "\n\n";
                }

                //+facet_grid({ { Facetcolumn} } ~ {{ Facetrow} }, scales ={ { Facetscale} })  +facet_wrap(  { { Facetwrap} } )
            }

            else if (customsyntax == "Graphics-linechart(path)")
            {
                MatchCollection mcol = re.Matches(commandformat);
                foreach (Match m in mcol)
                {
                    string matchedText = m.Groups[1].Value;
                    string result = GetParam(obj, matchedText);
                    if (!CommandKeyValDict.ContainsKey(matchedText))
                    {
                        CommandKeyValDict.Add(matchedText, result);
                    }
                }

                string xaxis = "";
                string yaxis = "";
                string groupby = "";
                string shape = "";
                string opacity = "";

                string Facetcolumn = "";
                string Facetrow = "";
                string Facetscale = "";
                string Facetwrap = "";
                string flipaxis = "";

                string dataset = "";
                string xlab = "";
                string ylab = "";
                string maintitle = "";

                foreach (KeyValuePair<string, string> kv in CommandKeyValDict)
                {
                    string key = kv.Key;
                    string value = kv.Value;
                    //create final syntac in 'output'
                    // output = output+","+ key + "=c(" + value + ")";

                    if (key == "xaxis")
                    {
                        xaxis = value;
                    }


                    if (key == "yaxis")
                    {
                        yaxis = value;
                    }
                    if (key == "groupby")
                    {
                        groupby = value;
                    }
                    if (key == "shape")
                    {
                        shape = value;
                    }

                    if (key == "opacity")
                    {
                        opacity = value;
                    }
                    if (key == "Facetcolumn")
                    {
                        Facetcolumn = value;
                    }
                    if (key == "Facetrow")
                    {
                        Facetrow = value;
                    }
                    if (key == "Facetscale")
                    {
                        Facetscale = value;
                    }
                    if (key == "Facetwrap")
                    {
                        Facetwrap = value;
                    }
                    if (key == "%DATASET%")
                    {
                        dataset = value;
                    }
                    if (key == "xlab")
                    {
                        xlab = value;
                    }
                    if (key == "ylab")
                    {
                        ylab = value;
                    }
                    if (key == "maintitle")
                    {
                        maintitle = value;
                    }
                }
                string tempoutput = "";
                bool addcomma = false;
                string[] variables = yaxis.Split(',');

                foreach (string var in variables)
                {
                    //   ggplot(data = { {% DATASET %} }, aes(x = eval(parse(text = paste(vars))), fill = { { Groupby} })) 
                    tempoutput = tempoutput + "ggplot(data=" + dataset + ", aes(x =" + xaxis + ", y =" + var + "))";

                    //+geom_density(position = "{{rdgrp1}}", fill = "{{barcolor}}", alpha = 0.5)
                    tempoutput = tempoutput + " +\n\t geom_path( stat = \"identity\", position = \"identity\"";

                    if (groupby != "" || shape != "")
                    {
                        tempoutput += ", aes (";

                        if (groupby != "")

                        {
                            tempoutput = tempoutput + "color = " + groupby;
                            addcomma = true;
                        }


                        if (shape != "")
                        {
                            if (!addcomma)
                            {
                                tempoutput = tempoutput + ", shape = " + shape;
                            }
                            else
                            {
                                tempoutput = tempoutput + " shape = " + shape;
                            }
                        }

                        tempoutput = tempoutput + ")";
                    }

                    if (opacity != "")
                    {
                        tempoutput = tempoutput + ", alpha=" + opacity;
                    }

                    tempoutput = tempoutput + ")";

                    if (flipaxis == "TRUE")
                    {
                        tempoutput = tempoutput + " +\n\t coord_flip()";
                    }


                    //   +labs(x = vars, y = "Count", fill = "{{Groupby}}", title = paste("Density plot for variable ", vars, sep = '')) +

                    if (groupby != "")
                    {
                        tempoutput = tempoutput + " +\n\t labs(x =" + "\"" + xaxis + "\"" + ", y =" + "\"" + var + "\"" + ", title= " + "\"Line chart (ordered by occurance of variable values in data) \\n for X axis variable: " + xaxis + ", Y axis variable: " + var + ", grouped in colors by variable: " + groupby + "\")";
                    }
                    else
                    {
                        tempoutput = tempoutput + " +\n\t labs(x =" + "\"" + xaxis + "\"" + ", y =" + "\"" + var + "\"" + ", title= " + "\"Line chart (ordered by occurance of variable values in data) \\n for X axis variable: " + xaxis + ", Y axis variable: " + var + "\")";
                    }

                    if (xlab != "")
                    {
                        tempoutput = tempoutput + " +\n\t xlab(" + "\"" + xlab + "\"" + ")";
                    }

                    if (ylab != "")
                    {
                        tempoutput = tempoutput + " +\n\t ylab(" + "\"" + ylab + "\"" + ")";
                    }

                    if (maintitle != "")
                    {
                        tempoutput = tempoutput + " +\n\t ggtitle(" + "\"" + maintitle + "\"" + ")";
                    }

                    tempoutput = tempoutput + createfacets(Facetwrap, Facetcolumn, Facetrow, Facetscale);
                    // tempoutput = Wrapinbrackets(tempoutput);
                    tempoutput = tempoutput + "\n\n";
                    output = output + tempoutput;

                    tempoutput = "";

                    output = output.TrimEnd(Environment.NewLine.ToCharArray());
                    output = output.TrimEnd(Environment.NewLine.ToCharArray());
                    output = output + " +\n" + themeSyntax + "\n\n";
                }

                //+facet_grid({ { Facetcolumn} } ~ {{ Facetrow} }, scales ={ { Facetscale} })  +facet_wrap(  { { Facetwrap} } )
            }

            else if (customsyntax == "Graphics-linechart(stairstep)")
            {
                MatchCollection mcol = re.Matches(commandformat);
                foreach (Match m in mcol)
                {
                    string matchedText = m.Groups[1].Value;
                    string result = GetParam(obj, matchedText);
                    if (!CommandKeyValDict.ContainsKey(matchedText))
                    {
                        CommandKeyValDict.Add(matchedText, result);
                    }
                }

                string xaxis = "";
                string yaxis = "";
                string groupby = "";
                string shape = "";
                string opacity = "";

                string Facetcolumn = "";
                string Facetrow = "";
                string Facetscale = "";
                string Facetwrap = "";
                string flipaxis = "";


                string dataset = "";
                string xlab = "";
                string ylab = "";
                string maintitle = "";

                foreach (KeyValuePair<string, string> kv in CommandKeyValDict)
                {
                    string key = kv.Key;
                    string value = kv.Value;
                    //create final syntac in 'output'
                    // output = output+","+ key + "=c(" + value + ")";

                    if (key == "xaxis")
                    {
                        xaxis = value;
                    }

                    if (key == "yaxis")
                    {
                        yaxis = value;
                    }
                    if (key == "groupby")
                    {
                        groupby = value;
                    }
                    if (key == "shape")
                    {
                        shape = value;
                    }

                    if (key == "opacity")
                    {
                        opacity = value;
                    }
                    if (key == "Facetcolumn")
                    {
                        Facetcolumn = value;
                    }
                    if (key == "Facetrow")
                    {
                        Facetrow = value;
                    }
                    if (key == "Facetscale")
                    {
                        Facetscale = value;
                    }
                    if (key == "Facetwrap")
                    {
                        Facetwrap = value;
                    }
                    if (key == "%DATASET%")
                    {
                        dataset = value;
                    }
                    if (key == "xlab")
                    {
                        xlab = value;
                    }
                    if (key == "ylab")
                    {
                        ylab = value;
                    }
                    if (key == "maintitle")
                    {
                        maintitle = value;
                    }
                }
                string tempoutput = "";
                bool addcomma = false;
                string[] variables = yaxis.Split(',');

                foreach (string var in variables)
                {
                    //   ggplot(data = { {% DATASET %} }, aes(x = eval(parse(text = paste(vars))), fill = { { Groupby} })) 
                    tempoutput = tempoutput + "ggplot(data=" + dataset + ", aes(x =" + xaxis + ", y =" + var + "))";

                    //+geom_density(position = "{{rdgrp1}}", fill = "{{barcolor}}", alpha = 0.5)
                    tempoutput = tempoutput + " +\n\t geom_step( stat = \"identity\", position = \"identity\"";

                    if (groupby != "" || shape != "")
                    {
                        tempoutput += ", aes (";

                        if (groupby != "")

                        {
                            tempoutput = tempoutput + "color = " + groupby;
                            addcomma = true;
                        }

                        if (shape != "")
                        {
                            if (!addcomma)
                            {
                                tempoutput = tempoutput + ", shape = " + shape;
                            }
                            else
                            {
                                tempoutput = tempoutput + " shape = " + shape;
                            }
                        }

                        tempoutput = tempoutput + ")";
                    }

                    if (opacity != "")
                    {
                        tempoutput = tempoutput + ", alpha=" + opacity;
                    }

                    tempoutput = tempoutput + ")";

                    if (flipaxis == "TRUE")
                    {
                        tempoutput = tempoutput + " +\n\t coord_flip()";
                    }

                    //   +labs(x = vars, y = "Count", fill = "{{Groupby}}", title = paste("Density plot for variable ", vars, sep = '')) +

                    if (groupby != "")
                    {
                        tempoutput = tempoutput + " +\n\t labs(x =" + "\"" + xaxis + "\"" + ", y =" + "\"" + var + "\"" + ", title= " + "\"Line chart (stair step) for X axis variable: " + xaxis + "\\nY axis variable: " + var + ", grouped in colors by variable: " + groupby + "\")";
                    }
                    else
                    {
                        tempoutput = tempoutput + " +\n\t labs(x =" + "\"" + xaxis + "\"" + ", y =" + "\"" + var + "\"" + ", title= " + "\"Line chart (stair step) for X axis variable: " + xaxis + "\\nY axis variable: " + var + "\")";
                    }

                    if (xlab != "")
                    {
                        tempoutput = tempoutput + " +\n\t xlab(" + "\"" + xlab + "\"" + ")";
                    }

                    if (ylab != "")
                    {
                        tempoutput = tempoutput + " +\n\t ylab(" + "\"" + ylab + "\"" + ")";
                    }

                    if (maintitle != "")
                    {
                        tempoutput = tempoutput + " +\n\t ggtitle(" + "\"" + maintitle + "\"" + ")";
                    }

                    tempoutput = tempoutput + createfacets(Facetwrap, Facetcolumn, Facetrow, Facetscale);
                    // tempoutput = Wrapinbrackets(tempoutput);
                    tempoutput = tempoutput + "\n\n";
                    output = output + tempoutput;

                    tempoutput = "";

                    output = output.TrimEnd(Environment.NewLine.ToCharArray());
                    output = output.TrimEnd(Environment.NewLine.ToCharArray());
                    output = output + " +\n" + themeSyntax + "\n\n";
                }

                //+facet_grid({ { Facetcolumn} } ~ {{ Facetrow} }, scales ={ { Facetscale} })  +facet_wrap(  { { Facetwrap} } )
            }

            else if (customsyntax == "Graphics-contour")
            {
                MatchCollection mcol = re.Matches(commandformat);
                foreach (Match m in mcol)
                {
                    string matchedText = m.Groups[1].Value;
                    string result = GetParam(obj, matchedText);
                    if (!CommandKeyValDict.ContainsKey(matchedText))
                    {
                        CommandKeyValDict.Add(matchedText, result);
                    }
                }

                string xaxis = "";
                string yaxis = "";
                string groupby = "";
                string shape = "";
                string opacity = "";

                string Facetcolumn = "";
                string Facetrow = "";
                string Facetscale = "";
                string Facetwrap = "";
                string flipaxis = "";

                string dataset = "";
                string xlab = "";
                string ylab = "";
                string maintitle = "";


                foreach (KeyValuePair<string, string> kv in CommandKeyValDict)
                {
                    string key = kv.Key;
                    string value = kv.Value;
                    //create final syntac in 'output'
                    // output = output+","+ key + "=c(" + value + ")";

                    if (key == "xaxis")
                    {
                        xaxis = value;
                    }


                    if (key == "yaxis")
                    {
                        yaxis = value;
                    }
                    if (key == "groupby")
                    {
                        groupby = value;
                    }
                    if (key == "shape")
                    {
                        shape = value;
                    }

                    if (key == "opacity")
                    {
                        opacity = value;
                    }
                    if (key == "Facetcolumn")
                    {
                        Facetcolumn = value;
                    }
                    if (key == "Facetrow")
                    {
                        Facetrow = value;
                    }
                    if (key == "Facetscale")
                    {
                        Facetscale = value;
                    }
                    if (key == "Facetwrap")
                    {
                        Facetwrap = value;
                    }
                    if (key == "%DATASET%")
                    {
                        dataset = value;
                    }
                    if (key == "xlab")
                    {
                        xlab = value;
                    }
                    if (key == "ylab")
                    {
                        ylab = value;
                    }
                    if (key == "maintitle")
                    {
                        maintitle = value;
                    }
                }
                string tempoutput = "";
                bool addcomma = false;
                string[] variables = yaxis.Split(',');

                foreach (string var in variables)
                {
                    //   ggplot(data = { {% DATASET %} }, aes(x = eval(parse(text = paste(vars))), fill = { { Groupby} })) 
                    tempoutput = tempoutput + "ggplot(data=" + dataset + ", aes(x = " + xaxis + ", y =" + var + "))";

                    //+geom_density(position = "{{rdgrp1}}", fill = "{{barcolor}}", alpha = 0.5)
                    tempoutput = tempoutput + " +\n\t geom_density2d( stat = \"density2d\", position = \"identity\"";

                    if (opacity != "")
                    {
                        tempoutput = tempoutput + ", alpha=" + opacity;
                    }

                    tempoutput = tempoutput + ")";

                    if (flipaxis == "TRUE")
                    {
                        tempoutput = tempoutput + " +\n\t coord_flip()";
                    }

                    //   +labs(x = vars, y = "Count", fill = "{{Groupby}}", title = paste("Density plot for variable ", vars, sep = '')) +

                    if (groupby != "")
                    {
                        tempoutput = tempoutput + " +\n\t labs(x =" + "\"" + xaxis + "\"" + ", y =" + "\"" + var + "\"" + ", title= " + "\"Contour plot for X axis variable: " + xaxis + ", Y axis variable: " + var + "\\ngrouped in colors by variable: " + groupby + "\")";
                    }
                    else
                    {

                        tempoutput = tempoutput + " +\n\t labs(x =" + "\"" + xaxis + "\"" + ", y =" + "\"" + var + "\"" + ", title= " + "\"Contour plot  for X axis variable: " + xaxis + ", Y axis variable: " + var + "\")";
                    }

                    if (xlab != "")
                    {
                        tempoutput = tempoutput + " +\n\t xlab(" + "\"" + xlab + "\"" + ")";
                    }

                    if (ylab != "")
                    {
                        tempoutput = tempoutput + " +\n\t ylab(" + "\"" + ylab + "\"" + ")";
                    }

                    if (maintitle != "")
                    {
                        tempoutput = tempoutput + " +\n\t ggtitle(" + "\"" + maintitle + "\"" + ")";
                    }

                    tempoutput = tempoutput + createfacets(Facetwrap, Facetcolumn, Facetrow, Facetscale);
                    // tempoutput = Wrapinbrackets(tempoutput);
                    tempoutput = tempoutput + "\n\n";
                    output = output + tempoutput;

                    tempoutput = "";

                    output = output.TrimEnd(Environment.NewLine.ToCharArray());
                    output = output.TrimEnd(Environment.NewLine.ToCharArray());
                    output = output + " +\n" + themeSyntax + "\n\n";
                }

                //+facet_grid({ { Facetcolumn} } ~ {{ Facetrow} }, scales ={ { Facetscale} })  +facet_wrap(  { { Facetwrap} } )
            }

            else if (customsyntax == "Graphics-violin")
            {
                MatchCollection mcol = re.Matches(commandformat);
                foreach (Match m in mcol)
                {
                    string matchedText = m.Groups[1].Value;
                    string result = GetParam(obj, matchedText);
                    if (!CommandKeyValDict.ContainsKey(matchedText))
                    {
                        CommandKeyValDict.Add(matchedText, result);
                    }
                }

                string xaxis = "";
                string yaxis = "";
                string groupby = "";
                string shape = "";
                string opacity = "";

                string Facetcolumn = "";
                string Facetrow = "";
                string Facetscale = "";
                string Facetwrap = "";
                string flipaxis = "";


                string dataset = "";
                string xlab = "";
                string ylab = "";
                string maintitle = "";

                foreach (KeyValuePair<string, string> kv in CommandKeyValDict)
                {
                    string key = kv.Key;
                    string value = kv.Value;
                    //create final syntac in 'output'
                    // output = output+","+ key + "=c(" + value + ")";

                    if (key == "xaxis")
                    {
                        xaxis = value;
                    }


                    if (key == "yaxis")
                    {
                        yaxis = value;
                    }
                    if (key == "groupby")
                    {
                        groupby = value;
                    }
                    if (key == "shape")
                    {
                        shape = value;
                    }

                    if (key == "opacity")
                    {
                        opacity = value;
                    }
                    if (key == "Facetcolumn")
                    {
                        Facetcolumn = value;
                    }
                    if (key == "Facetrow")
                    {
                        Facetrow = value;
                    }
                    if (key == "Facetscale")
                    {
                        Facetscale = value;
                    }
                    if (key == "Facetwrap")
                    {
                        Facetwrap = value;
                    }
                    if (key == "%DATASET%")
                    {
                        dataset = value;
                    }
                    if (key == "xlab")
                    {
                        xlab = value;
                    }
                    if (key == "ylab")
                    {
                        ylab = value;
                    }
                    if (key == "maintitle")
                    {
                        maintitle = value;
                    }
                }
                string tempoutput = "";
                bool addcomma = false;
                string[] variables = yaxis.Split(',');

                foreach (string var in variables)
                {

                    //   ggplot(data = { {% DATASET %} }, aes(x = eval(parse(text = paste(vars))), fill = { { Groupby} })) 
                    tempoutput = tempoutput + "ggplot(data=" + dataset + ", aes(x =as.factor(" + xaxis + ")" + ", y =" + var + "))";

                    //+geom_density(position = "{{rdgrp1}}", fill = "{{barcolor}}", alpha = 0.5)
                    tempoutput = tempoutput + " +\n\t geom_violin( stat = \"ydensity\", position = \"dodge\" , trim=TRUE, scale=\"area\"";

                    if (groupby != "" || shape != "")
                    {
                        tempoutput += ", aes (";

                        if (groupby != "")

                        {
                            tempoutput = tempoutput + "fill = " + groupby;
                            addcomma = true;
                        }
                        
                        if (shape != "")
                        {
                            if (!addcomma)
                            {
                                tempoutput = tempoutput + ", shape = " + shape;
                            }
                            else
                            {
                                tempoutput = tempoutput + " shape = " + shape;
                            }
                        }

                        tempoutput = tempoutput + ")";
                    }

                    if (opacity != "")
                    {
                        tempoutput = tempoutput + ", alpha=" + opacity;
                    }

                    tempoutput = tempoutput + ")";

                    if (flipaxis == "TRUE")
                    {
                        tempoutput = tempoutput + " +\n\t coord_flip()";
                    }


                    //   +labs(x = vars, y = "Count", fill = "{{Groupby}}", title = paste("Density plot for variable ", vars, sep = '')) +

                    if (groupby != "")
                    {
                        tempoutput = tempoutput + " +\n\t labs(x =" + "\"" + xaxis + "\"" + ", y =" + "\"" + var + "\"" + ", title= " + "\"Violin chart for X axis variable: " + xaxis + ", Y axis variable: " + var + "\\ngrouped in colors by variable: " + groupby + "\")";
                    }
                    else
                    {

                        tempoutput = tempoutput + " +\n\t labs(x =" + "\"" + xaxis + "\"" + ", y =" + "\"" + var + "\"" + ", title= " + "\"Violin chart for X axis variable: " + xaxis + ", Y axis variable: " + var + "\")";
                    }

                    if (xlab != "")
                    {
                        tempoutput = tempoutput + " +\n\t xlab(" + "\"" + xlab + "\"" + ")";
                    }

                    if (ylab != "")
                    {
                        tempoutput = tempoutput + " +\n\t ylab(" + "\"" + ylab + "\"" + ")";
                    }

                    if (maintitle != "")
                    {
                        tempoutput = tempoutput + " +\n\t ggtitle(" + "\"" + maintitle + "\"" + ")";
                    }

                    tempoutput = tempoutput + createfacets(Facetwrap, Facetcolumn, Facetrow, Facetscale);
                    // tempoutput = Wrapinbrackets(tempoutput);
                    tempoutput = tempoutput + "\n\n";
                    output = output + tempoutput;

                    tempoutput = "";

                    output = output.TrimEnd(Environment.NewLine.ToCharArray());
                    output = output.TrimEnd(Environment.NewLine.ToCharArray());
                    output = output + " +\n" + themeSyntax + "\n\n";
                }

                //+facet_grid({ { Facetcolumn} } ~ {{ Facetrow} }, scales ={ { Facetscale} })  +facet_wrap(  { { Facetwrap} } )
            }

            else if (customsyntax == "Graphics-bin2d")
            {
                MatchCollection mcol = re.Matches(commandformat);
                foreach (Match m in mcol)
                {
                    string matchedText = m.Groups[1].Value;
                    string result = GetParam(obj, matchedText);
                    if (!CommandKeyValDict.ContainsKey(matchedText))
                    {
                        CommandKeyValDict.Add(matchedText, result);
                    }
                }

                string xaxis = "";
                string yaxis = "";
                string groupby = "";
                string shape = "";
                string opacity = "";
                string bins = "";

                string Facetcolumn = "";
                string Facetrow = "";
                string Facetscale = "";
                string Facetwrap = "";
                string flipaxis = "";


                string dataset = "";
                string xlab = "";
                string ylab = "";
                string maintitle = "";

                foreach (KeyValuePair<string, string> kv in CommandKeyValDict)
                {
                    string key = kv.Key;
                    string value = kv.Value;
                    //create final syntac in 'output'
                    // output = output+","+ key + "=c(" + value + ")";

                    if (key == "xaxis")
                    {
                        xaxis = value;
                    }

                    if (key == "bins")
                    {
                        bins = value;
                    }


                    if (key == "yaxis")
                    {
                        yaxis = value;
                    }
                    if (key == "groupby")
                    {
                        groupby = value;
                    }
                    if (key == "shape")
                    {
                        shape = value;
                    }

                    if (key == "opacity")
                    {
                        opacity = value;
                    }
                    if (key == "Facetcolumn")
                    {
                        Facetcolumn = value;
                    }
                    if (key == "Facetrow")
                    {
                        Facetrow = value;
                    }
                    if (key == "Facetscale")
                    {
                        Facetscale = value;
                    }
                    if (key == "Facetwrap")
                    {
                        Facetwrap = value;
                    }
                    if (key == "%DATASET%")
                    {
                        dataset = value;
                    }
                    if (key == "xlab")
                    {
                        xlab = value;
                    }
                    if (key == "ylab")
                    {
                        ylab = value;
                    }
                    if (key == "maintitle")
                    {
                        maintitle = value;
                    }
                }
                string tempoutput = "";
                bool addcomma = false;
                string[] variables = yaxis.Split(',');

                foreach (string var in variables)
                {

                    //   ggplot(data = { {% DATASET %} }, aes(x = eval(parse(text = paste(vars))), fill = { { Groupby} })) 
                    tempoutput = tempoutput + "ggplot(data=" + dataset + ", aes(x =" + xaxis + ", y =" + var + "))";

                    //+geom_density(position = "{{rdgrp1}}", fill = "{{barcolor}}", alpha = 0.5)
                    tempoutput = tempoutput + " +\n\t geom_tile( stat = \"bin2d\", position = \"identity\"";

                    if (groupby != "" || shape != "")
                    {
                        tempoutput += ", aes (";

                        if (groupby != "")

                        {
                            tempoutput = tempoutput + "fill = " + groupby;
                            addcomma = true;
                        }


                        if (shape != "")
                        {
                            if (!addcomma)
                            {
                                tempoutput = tempoutput + ", shape = " + shape;
                            }
                            else
                            {
                                tempoutput = tempoutput + " shape = " + shape;
                            }
                        }
                        tempoutput = tempoutput + ")";
                    }

                    if (opacity != "")
                    {
                        tempoutput = tempoutput + ", alpha=" + opacity;
                    }

                    if (bins != "")

                    {
                        if (bins != "0")
                        {
                            tempoutput = tempoutput + ", bins = " + bins;
                        }
                    }

                    tempoutput = tempoutput + ")";


                    if (flipaxis == "TRUE")
                    {
                        tempoutput = tempoutput + " +\n\t coord_flip()";
                    }


                    //   +labs(x = vars, y = "Count", fill = "{{Groupby}}", title = paste("Density plot for variable ", vars, sep = '')) +

                    if (groupby != "")
                    {
                        tempoutput = tempoutput + " +\n\t labs(x =" + "\"" + xaxis + "\"" + ", y =" + "\"" + var + "\"" + ", title= " + "\"Binned scatterplot (square) for X axis variable: " + xaxis + ", Y axis variable: " + var + "\\ngrouped in colors by variable: " + groupby + "\")";
                    }
                    else
                    {
                        tempoutput = tempoutput + " +\n\t labs(x =" + "\"" + xaxis + "\"" + ", y =" + "\"" + var + "\"" + ", title= " + "\"Binned scatterplot (square) for X axis variable: " + xaxis + ", Y axis variable: " + var + "\")";
                    }

                    if (xlab != "")
                    {
                        tempoutput = tempoutput + " +\n\t xlab(" + "\"" + xlab + "\"" + ")";
                    }

                    if (ylab != "")
                    {
                        tempoutput = tempoutput + " +\n\t ylab(" + "\"" + ylab + "\"" + ")";
                    }

                    if (maintitle != "")
                    {
                        tempoutput = tempoutput + " +\n\t ggtitle(" + "\"" + maintitle + "\"" + ")";
                    }

                    tempoutput = tempoutput + createfacets(Facetwrap, Facetcolumn, Facetrow, Facetscale);
                    // tempoutput = Wrapinbrackets(tempoutput);
                    tempoutput = tempoutput + "\n\n";
                    output = output + tempoutput;

                    tempoutput = "";

                    output = output.TrimEnd(Environment.NewLine.ToCharArray());
                    output = output.TrimEnd(Environment.NewLine.ToCharArray());
                    output = output + " +\n" + themeSyntax + "\n\n";
                }

                //+facet_grid({ { Facetcolumn} } ~ {{ Facetrow} }, scales ={ { Facetscale} })  +facet_wrap(  { { Facetwrap} } )
            }

            else if (customsyntax == "Graphics-binhex")
            {
                MatchCollection mcol = re.Matches(commandformat);
                foreach (Match m in mcol)
                {
                    string matchedText = m.Groups[1].Value;
                    string result = GetParam(obj, matchedText);
                    if (!CommandKeyValDict.ContainsKey(matchedText))
                    {
                        CommandKeyValDict.Add(matchedText, result);
                    }
                }

                string xaxis = "";
                string yaxis = "";
                string groupby = "";
                string shape = "";
                string opacity = "";
                string bins = "";

                string Facetcolumn = "";
                string Facetrow = "";
                string Facetscale = "";
                string Facetwrap = "";
                string flipaxis = "";


                string dataset = "";
                string xlab = "";
                string ylab = "";
                string maintitle = "";

                foreach (KeyValuePair<string, string> kv in CommandKeyValDict)
                {
                    string key = kv.Key;
                    string value = kv.Value;
                    //create final syntac in 'output'
                    // output = output+","+ key + "=c(" + value + ")";

                    if (key == "xaxis")
                    {
                        xaxis = value;
                    }

                    if (key == "bins")
                    {
                        bins = value;
                    }


                    if (key == "yaxis")
                    {
                        yaxis = value;
                    }
                    if (key == "groupby")
                    {
                        groupby = value;
                    }
                    if (key == "shape")
                    {
                        shape = value;
                    }

                    if (key == "opacity")
                    {
                        opacity = value;
                    }
                    if (key == "Facetcolumn")
                    {
                        Facetcolumn = value;
                    }
                    if (key == "Facetrow")
                    {
                        Facetrow = value;
                    }
                    if (key == "Facetscale")
                    {
                        Facetscale = value;
                    }
                    if (key == "Facetwrap")
                    {
                        Facetwrap = value;
                    }
                    if (key == "%DATASET%")
                    {
                        dataset = value;
                    }
                    if (key == "xlab")
                    {
                        xlab = value;
                    }
                    if (key == "ylab")
                    {
                        ylab = value;
                    }
                    if (key == "maintitle")
                    {
                        maintitle = value;
                    }
                }
                string tempoutput = "";
                bool addcomma = false;
                string[] variables = yaxis.Split(',');

                foreach (string var in variables)
                {

                    //   ggplot(data = { {% DATASET %} }, aes(x = eval(parse(text = paste(vars))), fill = { { Groupby} })) 
                    tempoutput = tempoutput + "ggplot(data=" + dataset + ", aes(x =" + xaxis + ", y =" + var + "))";

                    //+geom_density(position = "{{rdgrp1}}", fill = "{{barcolor}}", alpha = 0.5)
                    tempoutput = tempoutput + " +\n\t geom_hex( stat = \"binhex\", position = \"identity\"";

                    if (groupby != "" || shape != "")
                    {
                        tempoutput += ", aes (";

                        if (groupby != "")

                        {
                            tempoutput = tempoutput + "fill = " + groupby;
                            addcomma = true;
                        }


                        if (shape != "")
                        {
                            if (!addcomma)
                            {
                                tempoutput = tempoutput + ", shape = " + shape;
                            }
                            else
                            {
                                tempoutput = tempoutput + " shape = " + shape;
                            }
                        }
                        tempoutput = tempoutput + ")";
                    }

                    if (opacity != "")
                    {
                        tempoutput = tempoutput + ", alpha=" + opacity;
                    }

                    if (bins != "")

                    {
                        if (bins != "0")
                        {
                            tempoutput = tempoutput + ", bins = " + bins;
                        }
                    }

                    tempoutput = tempoutput + ")";


                    if (flipaxis == "TRUE")
                    {
                        tempoutput = tempoutput + " +\n\t coord_flip()";
                    }

                    //   +labs(x = vars, y = "Count", fill = "{{Groupby}}", title = paste("Density plot for variable ", vars, sep = '')) +

                    if (groupby != "")
                    {
                        tempoutput = tempoutput + " +\n\t labs(x =" + "\"" + xaxis + "\"" + ", y =" + "\"" + yaxis + "\"" + ", title= " + "\"Binned Scatterplot (Hex) for X axis variable: " + xaxis + " ,Y axis variable: " + yaxis + "\\ngrouped in colors by variable: " + groupby + "\")";
                    }
                    else
                    {

                        tempoutput = tempoutput + " +\n\t labs(x =" + "\"" + xaxis + "\"" + ", y =" + "\"" + yaxis + "\"" + ", title= " + "\"Binned Scatterplot (Hex) for X axis variable: " + xaxis + " ,Y axis variable: " + yaxis + "\")";
                    }

                    if (xlab != "")
                    {
                        tempoutput = tempoutput + " +\n\t xlab(" + "\"" + xlab + "\"" + ")";
                    }

                    if (ylab != "")
                    {
                        tempoutput = tempoutput + " +\n\t ylab(" + "\"" + ylab + "\"" + ")";
                    }

                    if (maintitle != "")
                    {
                        tempoutput = tempoutput + " +\n\t ggtitle(" + "\"" + maintitle + "\"" + ")";
                    }

                    tempoutput = tempoutput + createfacets(Facetwrap, Facetcolumn, Facetrow, Facetscale);
                    // tempoutput = Wrapinbrackets(tempoutput);
                    tempoutput = tempoutput + "\n\n";
                    output = output + tempoutput;

                    tempoutput = "";

                    output = output.TrimEnd(Environment.NewLine.ToCharArray());
                    output = output.TrimEnd(Environment.NewLine.ToCharArray());
                    output = output + " +\n" + themeSyntax + "\n\n";
                }

                //+facet_grid({ { Facetcolumn} } ~ {{ Facetrow} }, scales ={ { Facetscale} })  +facet_wrap(  { { Facetwrap} } )
            }


            else if (customsyntax =="Multi-Way Anova")
            {
                string target = "";
                string dest = "";
                string interaction = "";
                string type = "";
                string levene = "";
                //Pairwise comparisons
                string combon = "";
                string adjust = "";
                string compactly = "";
                string alpha = "";
                string diag = "";
                string plot1 = "";
                string plot2 = "";
                string dataset = "";


                MatchCollection mcol = re.Matches(commandformat);
                foreach (Match m in mcol)
                {
                    string matchedText = m.Groups[1].Value;
                    string result = GetParam(obj, matchedText);
                    if (!CommandKeyValDict.ContainsKey(matchedText))
                    {
                        CommandKeyValDict.Add(matchedText, result);
                    }

                }

                foreach (KeyValuePair<string, string> kv in CommandKeyValDict)
                {
                    string key = kv.Key;
                    string value = kv.Value;
                    //create final syntac in 'output'
                    // output = output+","+ key + "=c(" + value + ")";

                    if (key == "%DATASET%")
                    {
                        dataset = value;
                    }

                    if (key == "dest")
                    {
                        dest = value;
                    }
                    if (key == "target")
                    {
                        target = value;
                    }
                    if (key == "Interaction")
                    {
                        interaction = value;
                    }
                    if (key == "type")
                    {
                        type = value;
                    }
                    if (key == "levene")
                    {
                        levene = value;
                    }
                    if (key == "combon")
                    {
                        combon = value;
                    }
                    if (key == "adjust")
                    {
                        adjust = value;
                    }
                    if (key == "compactly")
                    {
                        compactly = value;
                    }
                    if (key == "alpha")
                    {
                        alpha = value;
                    }
                    if (key == "diag")
                    {
                        diag = value;
                    }
                    if (key == "plot1")
                    {
                        plot1 = value;
                    }
                    if (key == "plot2")
                    {
                        plot2 = value;
                    }
                  
                }
                string tempoutput = "";

                string[] thevars =null;
                string variables = "";
                string group = "";
                string nointeraction = "";
                string dependentVars = "";
                string interactionPlotString = "";

                if (dest.ToLower().Contains(','))
                {
                   thevars = dest.Split(',');
                }
                else
                {
                    variables = dest;
                }
                bool multiway = false;

                if ( thevars != null)
                {
                    multiway = true;
                    group = dest.Replace(",", "*");
                    nointeraction = dest.Replace(",", "+");
                    interactionPlotString = dest.Replace(",", "~");
                }
                else
                {
                    multiway = false;
                }
               
                if (multiway == false)
                {
                    tempoutput = tempoutput + "#Generating summaries";
                    tempoutput = tempoutput + "\ntemp <-" + dataset + "%>%\t group_by(" + variables + ") %>%\t summarise(n = n(), mean = mean("
                        + target + " , na.rm = TRUE), median = median("
                        + target + ", na.rm = TRUE), min = min("
                        + target + ", na.rm = TRUE), max = max("
                        + target + ", na.rm = TRUE), sd = sd("
                        + target + ", na.rm = TRUE), variance = var("
                        + target + ", na.rm = TRUE))\n";
                    tempoutput = tempoutput + "\nnames(temp)[1] =" + "\""+ variables + "\"";
                    tempoutput = tempoutput + "\nBSkyFormat( as.data.frame(temp), singleTableOutputHeader = \"Summaries for "
                        + target + " by factor variable " + variables + " \")";

                    tempoutput = tempoutput + "\n\n#Setting contrasts";
                    tempoutput = tempoutput + "\ncontrasts(" + dataset + "$" + variables + ") <- contr.sum";

                    tempoutput = tempoutput + "\n\n#Creating the model";
                    tempoutput = tempoutput + "\nBSkyMultiAnova =as.data.frame(summary(MultiAnova <-aov("
                        + target + "~" + variables + ", data =" + dataset + "))[[1]])";

                    if (diag == "TRUE")
                    {
                        tempoutput = tempoutput + "\n\n#Displaying diagnostic plots";
                        //tempoutput = tempoutput + "\nBSkyGraphicsFormat(noOfGraphics = 4)";
                        tempoutput = tempoutput + "\nplot(MultiAnova)";
                    }

                    tempoutput = tempoutput + "\n\n#Creating the Anova table with type I/II/III sum of squares";

                    if (type =="I")
                    {
                        tempoutput = tempoutput + "\n\nanovaTable =as.data.frame(stats::anova(MultiAnova))";
                        tempoutput = tempoutput + "\nBSkyFormat(BSkyMultiAnova, singleTableOutputHeader = \"Anova table with type I sum of squares for " + target
                             + " by " + variables +"\")";

                    }
                    else if (type =="II")
                    {
                        tempoutput = tempoutput + "\nanovaTable =as.data.frame(car::Anova(MultiAnova, type =\"II\"))";
                        tempoutput = tempoutput + "\nBSkyFormat(BSkyMultiAnova, singleTableOutputHeader = \"Anova table with type II sum of squares for " + target
                             + " by " + variables + "\")";
                    }
                    else if (type =="III")
                    {
                        tempoutput = tempoutput + "\nanovaTable =as.data.frame(car::Anova(MultiAnova, type =\"III\"))";
                        tempoutput = tempoutput + "\nBSkyFormat(BSkyMultiAnova, singleTableOutputHeader = \"Anova table with type III sum of squares for " + target
                             + " by " + variables + "\")";
                    }

                    tempoutput = tempoutput + "\n\n#Displaying estimated marginal means";

                    tempoutput += "\nresultsEmmeans = list()";

                    tempoutput += "\nresultsEmmeans<-emmeans::emmeans(MultiAnova, ~" + variables + ")";
                    tempoutput += "\nBSkyFormat( as.data.frame(resultsEmmeans), singleTableOutputHeader =\"Estimated Marginal Means for " +
                        target + " by " + variables + "\")";

                    if (levene == "TRUE")
                    {
                        tempoutput = tempoutput + "\n\n#Levene's Test";
                        tempoutput +=  "\nBSky_Levene_Test <-with(" + dataset+ ",car::leveneTest(" + target + ","+  variables+ "))";
                        tempoutput += "\nBSkyFormat(as.data.frame(BSky_Levene_Test), singleTableOutputHeader = \"Levene's test for homogenity of variances (center=mean) for "
                            + target+ " against " +variables+ "\")";

                    }

                    //  tempoutput += "\nresultsContrasts = list()";
                    tempoutput = tempoutput + "\n\n#Post-hoc tests";
                    tempoutput += "\nresContrasts <-emmeans::contrast(resultsEmmeans,method =  \"" + combon + "\" , adjust = \"" + adjust + "\")";
                    tempoutput += "\nresSummary <-summary(resContrasts)";

                    tempoutput += "\ncat(\"\\n\\n\\n\")";
                    tempoutput += "\ncat(attributes(resSummary)$mesg, sep = \"\\n\")";
                    tempoutput += "\nBSkyFormat(as.data.frame(resContrasts), singleTableOutputHeader = \"Post-hoc tests for " + target +
                    " by "+ variables + " (using method = " + combon + ")\")";


                    if (compactly =="TRUE")
                    {
                        tempoutput = tempoutput + "\n\n#Compare means compactly";
                        tempoutput += "\nresultsContrasts = list()";
                        tempoutput += "\nresultsContrasts <-multcomp::cld(resultsEmmeans, level = "  +alpha + ")";
                        tempoutput += "\nBSkyFormat( as.data.frame(resultsContrasts), singleTableOutputHeader = \"Comparing means compactly for " + target + " by " + variables + " using " + combon +" comparison"+ " (p values adjusted using "+ adjust + ")\")";

                    }

                    if (plot1=="TRUE")
                    {
                        tempoutput = tempoutput + "\n\n#Plot all comparisons";
                        //tempoutput += "\nBSkyGraphicsFormat(noOfGraphics = 1)";
                        // tempoutput += "\nprint(plot(contrast(resultsEmmeans, method = \""+ combon + "\" , adjust = \"" + adjust + "\") + \"geom_vline(xintercept = 0) + ggtitle(\"Plotting all comparisons(\"" +  combon + ") for  " {target}}", "by", vars)))";
                        tempoutput += "\nplot( contrast(resultsEmmeans, method= \"" + combon + "\", adjust=\"" + adjust + "\") ) +   geom_vline(xintercept = 0) + ggtitle ( \"Plotting all comparisons " +combon + " for " + target + " by " + variables + "\")";
                    }

                   tempoutput = tempoutput + "\n\n";
                    output = output + tempoutput;

                    tempoutput = "";

                    output = output.TrimEnd(Environment.NewLine.ToCharArray());
                    output = output.TrimEnd(Environment.NewLine.ToCharArray());

                    

                }
                else
                {

                    //dest has all the variables as a string
                    //variables  has each variable
                    //thevars has a string [] of variables
                    tempoutput = tempoutput + "#Generating summaries";
                    if (interaction == "TRUE")
                    {
                        dependentVars = nointeraction;

                    }
                    else
                    {
                        dependentVars = group;

                    }


                    foreach (string vars in thevars)
                    {
                     
                        tempoutput = tempoutput + "\ntemp <-" + dataset + "%>%\t group_by(" + vars + ") %>%\t summarise(n = n(), mean = mean("
                            + target + " , na.rm = TRUE), median = median("
                            + target + ", na.rm = TRUE), min = min("
                            + target + ", na.rm = TRUE), max = max("
                            + target + ", na.rm = TRUE), sd = sd("
                            + target + ", na.rm = TRUE), variance = var("
                            + target + ", na.rm = TRUE))\n";
                        tempoutput = tempoutput + "\nnames(temp)[1] =" + "\"" + vars + "\"";
                        tempoutput = tempoutput + "\nBSkyFormat( as.data.frame(temp), singleTableOutputHeader = \"Summaries for "
                            + target + " by factor variable " + vars + " \")\n";
                    }
                  
                    tempoutput = tempoutput + "\ntemp <-" + dataset + "%>%\t group_by(" + dest + ") %>%\t summarise(n = n(), mean = mean("
                           + target + " , na.rm = TRUE), median = median("
                           + target + ", na.rm = TRUE), min = min("
                           + target + ", na.rm = TRUE), max = max("
                           + target + ", na.rm = TRUE), sd = sd("
                           + target + ", na.rm = TRUE), variance = var("
                           + target + ", na.rm = TRUE))\n";
                   // tempoutput = tempoutput + "\nnames(temp)[1] =" + "\"" +  + "\"";
                    tempoutput = tempoutput + "\nBSkyFormat( as.data.frame(temp), singleTableOutputHeader = \"Summaries for "
                        + target + " by factor variables " + dependentVars + " \")";

                    tempoutput = tempoutput + "\n\n#Setting contrasts";
                    foreach (string vars in thevars)
                    {
                         tempoutput = tempoutput + "\ncontrasts(" + dataset + "$" + vars + ") <- contr.sum";
                    }

                    

                    tempoutput = tempoutput + "\n\n#Creating the model";
                   
                         tempoutput = tempoutput + "\nBSkyMultiAnova =as.data.frame(summary(MultiAnova <-aov("
                            + target + "~" + dependentVars + ", data =" + dataset + "))[[1]])";
                  

                    if (diag == "TRUE")
                    {

                        tempoutput = tempoutput + "\n\n#Displaying diagnostic plots";
                      //  tempoutput = tempoutput + "\nBSkyGraphicsFormat(noOfGraphics = 4)";
                        tempoutput = tempoutput + "\nplot(MultiAnova)";
                    }

                    tempoutput = tempoutput + "\n\n#Creating the Anova table with type I/II/III sum of squares";

                   if (type == "I")
                   {

                        tempoutput = tempoutput + "\n\nanovaTable =as.data.frame(stats::anova(MultiAnova))";
                        tempoutput = tempoutput + "\nBSkyFormat(BSkyMultiAnova, singleTableOutputHeader = \"Anova table with type I sum of squares for " + target
                             + " by " + dependentVars + "\")";

                    }
                    else if (type == "II")
                    {

                        tempoutput = tempoutput + "\nanovaTable =as.data.frame(car::Anova(MultiAnova, type =\"II\"))";

                        tempoutput = tempoutput + "\nBSkyFormat(BSkyMultiAnova, singleTableOutputHeader = \"Anova table with type II sum of squares for " + target
                             + " by " + dependentVars + "\")";
                    }
                    else if (type == "III")
                    {
                        tempoutput = tempoutput + "\nanovaTable =as.data.frame(car::Anova(MultiAnova, type =\"III\"))";

                        tempoutput = tempoutput + "\nBSkyFormat(BSkyMultiAnova, singleTableOutputHeader = \"Anova table with type III sum of squares for " + target
                             + " by " + dependentVars + "\")";

                    }

                    tempoutput = tempoutput + "\n\n#Displaying estimated marginal means";
                    
                    tempoutput += "\nresEmmeans = list()";
                    int i = 1;
                    string index = "";
                    foreach (string vars in thevars)
                    {
                        index = i.ToString();

                        tempoutput += "\nresEmmeans[[" + index +"]]<-emmeans::emmeans(MultiAnova, ~" + vars + ")";
                        tempoutput += "\nBSkyFormat( as.data.frame(resEmmeans[[" + index + "]]), singleTableOutputHeader =\"Estimated Marginal Means for " +
                            target + " by " + vars + "\")";
                        tempoutput += "\n";

                        i++;

                    }

                    if (interaction != "TRUE")
                        {
                        //  tempoutput += "\nresultsEmmeans = list()";
                        tempoutput += "\nresultsEmmeans<-emmeans::emmeans(MultiAnova, ~" + dependentVars + ")";
                        tempoutput += "\nBSkyFormat( as.data.frame(resultsEmmeans), singleTableOutputHeader =\"Estimated Marginal Means for " +
                            target + " by " + dependentVars + "\")";
                    }
                  
                    if (levene == "TRUE")
                    {
                        tempoutput = tempoutput + "\n\n#Levene's Test";
                        foreach (string vars in thevars)
                        {
                            tempoutput += "\nBSky_Levene_Test <-with(" + dataset + ",car::leveneTest(" + target + "," + vars + "))";
                            tempoutput += "\nBSkyFormat(as.data.frame(BSky_Levene_Test), singleTableOutputHeader = \"Levene's test for homogenity of variances (center=mean) for "
                                + target + " against " + vars + "\")";
                            tempoutput += "\n";
                        }

                    }

                    tempoutput = tempoutput + "\n\n#Post-hoc tests";

                    tempoutput += "\nresultsContrasts = list()";
                    i = 1;
                    index = "0";
                    foreach (string vars in thevars)
                    {
                        
                        index = i.ToString();
                        tempoutput += "\nresultsContrasts[[" + index+ "]]<-emmeans::contrast(resEmmeans[[" + index+ "]],method =  \"" + combon + "\" , adjust = \"" + adjust + "\")";
                        tempoutput += "\nresSummary <-summary(resultsContrasts[[" +  index + "]])";

                        tempoutput += "\ncat(\"\\n\\n\\n\")";
                        tempoutput += "\ncat(attributes(resSummary)$mesg, sep = \"\\n\")";
                        tempoutput += "\nBSkyFormat(as.data.frame(resultsContrasts[[" + index+ "]]), singleTableOutputHeader = \"Post-hoc tests for " + target +
                        " by " + vars + " (using method = " + combon + ")\")";
                        tempoutput += "\n";
                        i++;

                    }

                    if (interaction != "TRUE")
                    {
                        tempoutput += "\nresContrasts <-emmeans::contrast(resultsEmmeans,method =  \"" + combon + "\" , adjust = \"" + adjust + "\")";
                        tempoutput += "\nresSummary <-summary(resContrasts)";

                        tempoutput += "\ncat(\"\\n\\n\\n\")";
                        tempoutput += "\ncat(attributes(resSummary)$mesg, sep = \"\\n\")";
                        tempoutput += "\nBSkyFormat(as.data.frame(resContrasts), singleTableOutputHeader = \"Simple effects for " + target +
                        " by " + dependentVars + " (using method = " + combon + ")\")";
                    }   
                    
                    // end contrasts

                    //Compactly

                    if (compactly == "TRUE")
                    {
                        i = 1;
                        tempoutput = tempoutput + "\n\n#Compare means compactly";

                        foreach (string vars in thevars)
                        {
                        
                            index = i.ToString();
                            tempoutput += "\nresultsContrasts = list()";
                            tempoutput += "\nresultsContrasts[[" + index + "]] <-multcomp::cld(resEmmeans[[" + index + "]], level = "  + alpha + ")";
                            tempoutput += "\nBSkyFormat( as.data.frame(resultsContrasts[[" + index + "]]), singleTableOutputHeader = \"Comparing means compactly for " + target + " by " + vars + " using " + combon + " comparison" + " (p values adjusted using " + adjust + ")\")\n";

                            i++;

                        }

                        if (interaction != "TRUE")
                        {

                            tempoutput += "\nresultsContrasts = list()";
                            tempoutput += "\nresultsContrasts <-multcomp::cld(resultsEmmeans, level = "  + alpha + ")";
                            tempoutput += "\nBSkyFormat( as.data.frame(resultsContrasts), singleTableOutputHeader = \"Comparing means compactly for " + target + " by " + dependentVars + " using " + combon + " comparison" + " (p values adjusted using " + adjust + ")\")";
                        }
                    }


                    if (plot1 == "TRUE")
                    {
                        tempoutput = tempoutput + "\n\n#Plot all comparisons";
                        i = 1;

                        foreach (string vars in thevars)
                        {

                            index = i.ToString();
                         //   tempoutput += "\nBSkyGraphicsFormat(noOfGraphics = 1)";
                            // tempoutput += "\nprint(plot(contrast(resultsEmmeans, method = \""+ combon + "\" , adjust = \"" + adjust + "\") + \"geom_vline(xintercept = 0) + ggtitle(\"Plotting all comparisons(\"" +  combon + ") for  " {target}}", "by", vars)))";
                            tempoutput += "\nplot( contrast(resEmmeans[[" + index + "]], method= \"" + combon + "\", adjust=\"" + adjust + "\") ) +   geom_vline(xintercept = 0) + ggtitle ( \"Plotting all comparisons " + combon + " for " + target + " by " + vars + "\")";
                                                        
                            i++;
                        }


                        // tempoutput += "\nBSkyGraphicsFormat(noOfGraphics = 1)";

                        // tempoutput += "\nprint(plot(contrast(resultsEmmeans, method = \""+ combon + "\" , adjust = \"" + adjust + "\") + \"geom_vline(xintercept = 0) + ggtitle(\"Plotting all comparisons(\"" +  combon + ") for  " {target}}", "by", vars)))";
                        if (interaction != "TRUE")
                        {
                            tempoutput += "\nplot( contrast(resultsEmmeans, method= \"" + combon + "\", adjust=\"" + adjust + "\") ) +   geom_vline(xintercept = 0) + ggtitle ( \"Plotting all comparisons " + combon + " for " + target + " by " + dependentVars + "\")";
                        }
                    }

                    
                    if (plot2=="TRUE" )
                    {
                        if (interaction != "TRUE")
                        {
                            tempoutput = tempoutput + "\n\n#Interaction Plots";
                            tempoutput += "\nBSkyFormat(\"Interaction plot with Confidence Intervals\")";
                            //  tempoutput +=  "\nBSkyGraphicsFormat(noOfGraphics = 1)";
                            tempoutput += "\nemmeans::lsmip(MultiAnova," + interactionPlotString + ", CIs = TRUE)";
                        }

                    }

                    tempoutput = tempoutput + "\n\n";
                    output = output + tempoutput;

                    tempoutput = "";

                    output = output.TrimEnd(Environment.NewLine.ToCharArray());
                    output = output.TrimEnd(Environment.NewLine.ToCharArray());


                }


            }

            else if (customsyntax == "Graphics-barplot")
            {
                //print(ggplot({ {% DATASET %} }, aes(x = { { GroupingVariable} }, y = eval(parse(text = paste(vars))) ,color = { { GroupBy} },size ={ { size} } ,alpha ={ { opacity} })) +geom_point() + labs(x = "{{GroupingVariable}}", y = vars, color = "{{GroupBy}}", title = paste("Scatter plot for variable ", "{{GroupingVariable}}", " by ", vars, sep = '')) + xlab("{{xlab}}") + ylab("{{ylab}}") + ggtitle("{{maintitle}}") { { themes} }
                //+facet_grid({  { Facetcolumn} }
                //~{ { Facetrow} }, scales ={ { Facetscale} })  +facet_wrap(  { { Facetwrap} } )+geom_smooth(method = "{{sm}}", color = "{{color}}"))

                MatchCollection mcol = re.Matches(commandformat);
                foreach (Match m in mcol)
                {
                    string matchedText = m.Groups[1].Value;
                    string result = GetParam(obj, matchedText);
                    if (!CommandKeyValDict.ContainsKey(matchedText))
                    {
                        CommandKeyValDict.Add(matchedText, result);
                    }
                }

                string Destination = "";
                string Groupby = "";
                string size = "";
                string opacity = "";
                string Facetcolumn = "";
                string Facetrow = "";
                string Facetscale = "";
                string Facetwrap = "";
                string fill = "";
                // string color = "";
                string dataset = "";
                string xlab = "";
                string ylab = "";
                string maintitle = "";
                string jitter = "";
                string flipaxis = "";
                string shape = "";
                string rdgrp1 = "";
                string barcolor = "";
                string yvariable = "";

                foreach (KeyValuePair<string, string> kv in CommandKeyValDict)
                {
                    string key = kv.Key;
                    string value = kv.Value;
                    //create final syntac in 'output'
                    // output = output+","+ key + "=c(" + value + ")";

                    if (key == "destination")
                    {
                        Destination = value;
                    }
                    if (key == "yvariable")
                    {
                        yvariable = value;
                    }
                    if (key == "Groupby")
                    {
                        Groupby = value;
                    }
                    if (key == "size")
                    {
                        size = value;
                    }
                    if (key == "opacity")
                    {
                        opacity = value;
                    }
                    if (key == "Facetcolumn")
                    {
                        Facetcolumn = value;
                    }
                    if (key == "Facetrow")
                    {
                        Facetrow = value;
                    }
                    if (key == "Facetscale")
                    {
                        Facetscale = value;
                    }
                    if (key == "Facetwrap")
                    {
                        Facetwrap = value;
                    }
                    if (key == "%DATASET%")
                    {
                        dataset = value;
                    }
                    if (key == "xlab")
                    {
                        xlab = value;
                    }
                    if (key == "ylab")
                    {
                        ylab = value;
                    }
                    if (key == "maintitle")
                    {
                        maintitle = value;
                    }

                    if (key == "jitter")
                    {
                        jitter = value;
                    }
                    if (key == "flipaxis")
                    {
                        flipaxis = value;
                    }
                    if (key == "barcolor")
                    {
                        barcolor = value;
                    }
                    if (key == "rdgrp1")
                    {
                        rdgrp1 = value;
                    }
                    if (key == "fill")
                    {
                        fill = value;
                    }
                }
                string tempoutput = "";
                string[] variables = yvariable.Split(',');
                string ylabel = "Count";
                foreach (string var in variables)
                {
                    //x='' handles the case where you want a bar graph for mpg filled by whether the car is automatic or not
                    //library(ggplot2)
                    // bp < -ggplot(df, aes(x = "", y = value, fill = group)) +
                    // geom_bar(width = 1, stat = "identity")

                    if (Destination == "")
                    {
                        tempoutput = tempoutput + "ggplot(data=" + dataset + ", aes(x ='' ";
                    }
                    else
                    {
                        tempoutput = tempoutput + "ggplot(data=" + dataset + ", aes(x =" + Destination;
                    }

                    if (var != "")
                    {
                        tempoutput = tempoutput + ",y=" + var;
                        ylabel = var;
                    }
                    if (Groupby != "")
                    {
                        tempoutput = tempoutput + ",fill=" + Groupby;
                    }

                    tempoutput = tempoutput + "))";

                    //Constructing geom_bar

                    if (rdgrp1 == "stack")
                    {
                        tempoutput = tempoutput + " +\n\t geom_bar( position =\"stack\" ";
                    }
                    else if (rdgrp1 == "dodge")
                    {
                        tempoutput = tempoutput + " +\n\t geom_bar( position=\"dodge\" ";
                    }
                    else if (rdgrp1 == "fill")
                    {
                        tempoutput = tempoutput + " +\n\t geom_bar( position=\"fill\" ";
                    }

                    if (opacity != "")
                    {
                        tempoutput = tempoutput + ",alpha=" + opacity;
                    }

                    if (barcolor != "")
                    {
                        tempoutput = tempoutput + ",fill =" + "\"" + barcolor + "\"";
                    }
                    if (yvariable != "")
                    {
                        tempoutput = tempoutput + " ,stat=\"identity\"";

                    }

                    tempoutput = tempoutput + ")";

                    if (flipaxis == "TRUE")
                    {
                        tempoutput = tempoutput + " +\n\t coord_flip()";
                    }

                    //we add Fill to the Barplot title only when it exists
                    if (Groupby == "")
                    {
                        tempoutput = tempoutput + " +\n\t labs(x =" + "\"" + Destination + "\"" + ", y =" + "\"" + ylabel + "\"" + "," + "fill =" + "\"" + Groupby + "\"" + ", title= " + "\"Bar chart for X axis: " + Destination + "  ,Y axis: " + ylabel + "\")";
                    }
                    else
                    {
                        tempoutput = tempoutput + " +\n\t labs(x =" + "\"" + Destination + "\"" + ", y =" + "\"" + ylabel + "\"" + "," + "fill =" + "\"" + Groupby + "\"" + ", title= " + "\"Bar chart for X axis: " + Destination + "  ,Y axis: " + ylabel + "  ,Fill: " + Groupby + "\")";
                    }
                    if (xlab != "")
                    {
                        tempoutput = tempoutput + " +\n\t xlab(" + "\"" + xlab + "\"" + ")";
                    }

                    if (ylab != "")
                    {
                        tempoutput = tempoutput + " +\n\t ylab(" + "\"" + ylab + "\"" + ")";
                    }

                    if (maintitle != "")
                    {
                        tempoutput = tempoutput + " +\n\t ggtitle(" + "\"" + maintitle + "\"" + ")";
                    }

                    //+geom_smooth(method ="{{sm}}", color= "{{color}}")

                    tempoutput = tempoutput + createfacets(Facetwrap, Facetcolumn, Facetrow, Facetscale);
                    // tempoutput = Wrapinbrackets(tempoutput);
                    tempoutput = tempoutput + "\n\n";
                    output = output + tempoutput;

                    tempoutput = "";

                    output = output.TrimEnd(Environment.NewLine.ToCharArray());
                    output = output.TrimEnd(Environment.NewLine.ToCharArray());
                    output = output + " +\n" + themeSyntax + "\n\n";
                }

                //+facet_grid({ { Facetcolumn} } ~ {{ Facetrow} }, scales ={ { Facetscale} })  +facet_wrap(  { { Facetwrap} } )
            }

            else if (customsyntax == "Graphics-piechart")
            {
                //print(ggplot({ {% DATASET %} }, aes(x = { { GroupingVariable} }, y = eval(parse(text = paste(vars))) ,color = { { GroupBy} },size ={ { size} } ,alpha ={ { opacity} })) +geom_point() + labs(x = "{{GroupingVariable}}", y = vars, color = "{{GroupBy}}", title = paste("Scatter plot for variable ", "{{GroupingVariable}}", " by ", vars, sep = '')) + xlab("{{xlab}}") + ylab("{{ylab}}") + ggtitle("{{maintitle}}") { { themes} }
                //+facet_grid({  { Facetcolumn} }
                //~{ { Facetrow} }, scales ={ { Facetscale} })  +facet_wrap(  { { Facetwrap} } )+geom_smooth(method = "{{sm}}", color = "{{color}}"))

                MatchCollection mcol = re.Matches(commandformat);
                foreach (Match m in mcol)
                {
                    string matchedText = m.Groups[1].Value;
                    string result = GetParam(obj, matchedText);
                    if (!CommandKeyValDict.ContainsKey(matchedText))
                    {
                        CommandKeyValDict.Add(matchedText, result);
                    }
                }

                string Destination = "";
                string Groupby = "";
                string size = "";
                string opacity = "";
                string Facetcolumn = "";
                string Facetrow = "";
                string Facetscale = "";
                string Facetwrap = "";
                string fill = "";
                // string color = "";
                string dataset = "";
                string xlab = "";
                string ylab = "";
                string maintitle = "";
                string jitter = "";
                string flipaxis = "";
                string shape = "";
                string rdgrp1 = "";
                string width = "";
                string yvariable = "";

                foreach (KeyValuePair<string, string> kv in CommandKeyValDict)
                {
                    string key = kv.Key;
                    string value = kv.Value;
                    //create final syntac in 'output'
                    // output = output+","+ key + "=c(" + value + ")";

                    if (key == "destination")
                    {
                        Destination = value;
                    }
                    if (key == "yvariable")
                    {
                        yvariable = value;
                    }
                    if (key == "Groupby")
                    {
                        Groupby = value;
                    }
                    if (key == "size")
                    {
                        size = value;
                    }
                    if (key == "opacity")
                    {
                        opacity = value;
                    }
                    if (key == "Facetcolumn")
                    {
                        Facetcolumn = value;
                    }
                    if (key == "Facetrow")
                    {
                        Facetrow = value;
                    }
                    if (key == "Facetscale")
                    {
                        Facetscale = value;
                    }
                    if (key == "Facetwrap")
                    {
                        Facetwrap = value;
                    }
                    if (key == "%DATASET%")
                    {
                        dataset = value;
                    }
                    if (key == "xlab")
                    {
                        xlab = value;
                    }
                    if (key == "ylab")
                    {
                        ylab = value;
                    }
                    if (key == "maintitle")
                    {
                        maintitle = value;
                    }

                    if (key == "jitter")
                    {
                        jitter = value;
                    }
                    if (key == "flipaxis")
                    {
                        flipaxis = value;
                    }
                    if (key == "width")
                    {
                        width = value;
                    }
                    if (key == "rdgrp1")
                    {
                        rdgrp1 = value;
                    }
                    if (key == "fill")
                    {
                        fill = value;
                    }
                }
                string tempoutput = "";
                string[] variables = Destination.Split(',');
                string ylabel = "Count";
                foreach (string var in variables)
                {
                    //x='' handles the case where you want a bar graph for mpg filled by whether the car is automatic or not
                    //library(ggplot2)
                    // bp < -ggplot(df, aes(x = "", y = value, fill = group)) +
                    // geom_bar(width = 1, stat = "identity")

                    if (var == "")
                    {
                        tempoutput = tempoutput + "ggplot(data=" + dataset + ", aes(x ='' ";
                    }
                    else
                    {
                        tempoutput = tempoutput + "ggplot(data=" + dataset + ", aes(x =" + var;
                    }

                    if (yvariable != "")
                    {
                        tempoutput = tempoutput + ",y=" + yvariable;
                        ylabel = yvariable;
                    }
                    if (Groupby != "")
                    {
                        tempoutput = tempoutput + ",fill=" + Groupby;
                    }

                    tempoutput = tempoutput + "))";

                    //Constructing geom_bar

                    //if (rdgrp1 == "stack")
                    //{
                    //    tempoutput = tempoutput + " +\n\t geom_bar( position =\"stack\" ";
                    //}
                    //else if (rdgrp1 == "dodge")
                    //{
                    //    tempoutput = tempoutput + " +\n\t geom_bar( position=\"dodge\" ";
                    //}

                    if (rdgrp1 == "fill")
                    {
                        tempoutput = tempoutput + " +\n\t geom_bar( position=\"fill\" ";
                    }
                    else
                    {
                        tempoutput = tempoutput + " +\n\t geom_bar( ";

                    }

                    if (opacity != "")
                    {
                        tempoutput = tempoutput + ",alpha=" + opacity;
                    }

                    if (width != "")
                    {
                        tempoutput = tempoutput + ",width =" + width;
                    }
                    if (yvariable != "")
                    {
                        tempoutput = tempoutput + " ,stat=\"identity\"";
                    }

                    tempoutput = tempoutput + ")";

                    tempoutput = tempoutput + " +\n\t coord_polar(\"y\")";


                    if (flipaxis == "TRUE")
                    {
                        tempoutput = tempoutput + " +\n\t coord_flip()";
                    }

                    //we add Fill to the Barplot title only when it exists
                    if (Groupby == "")
                    {
                        tempoutput = tempoutput + " +\n\t labs(x =" + "\"" + var + "\"" + ", y =" + "\"" + ylabel + "\"" + "," + "fill =" + "\"" + Groupby + "\"" + ", title= " + "\"Pie chart for X aesthetic: " + var + "  ,Y aesthetic: " + ylabel + "\")";
                    }
                    else
                    {
                        tempoutput = tempoutput + " +\n\t labs(x =" + "\"" + var + "\"" + ", y =" + "\"" + ylabel + "\"" + "," + "fill =" + "\"" + Groupby + "\"" + ", title= " + "\"Pie Chart  with X aesthetic: " + var + "  ,Y aesthetic: " + ylabel + "  ,Fill: " + Groupby + "\")";
                    }
                    if (xlab != "")
                    {
                        tempoutput = tempoutput + " +\n\t xlab(" + "\"" + xlab + "\"" + ")";
                    }

                    if (ylab != "")
                    {
                        tempoutput = tempoutput + " +\n\t ylab(" + "\"" + ylab + "\"" + ")";
                    }

                    if (maintitle != "")
                    {
                        tempoutput = tempoutput + " +\n\t ggtitle(" + "\"" + maintitle + "\"" + ")";
                    }

                    //+geom_smooth(method ="{{sm}}", color= "{{color}}")

                    tempoutput = tempoutput + createfacets(Facetwrap, Facetcolumn, Facetrow, Facetscale);
                    // tempoutput = Wrapinbrackets(tempoutput);
                    tempoutput = tempoutput + "\n\n";
                    output = output + tempoutput;

                    tempoutput = "";

                    output = output.TrimEnd(Environment.NewLine.ToCharArray());
                    output = output.TrimEnd(Environment.NewLine.ToCharArray());
                    output = output + " +\n" + themeSyntax + "\n\n";
                }

                //+facet_grid({ { Facetcolumn} } ~ {{ Facetrow} }, scales ={ { Facetscale} })  +facet_wrap(  { { Facetwrap} } )
            }

            else if (customsyntax == "Graphics-bullseye")
            {
                //print(ggplot({ {% DATASET %} }, aes(x = { { GroupingVariable} }, y = eval(parse(text = paste(vars))) ,color = { { GroupBy} },size ={ { size} } ,alpha ={ { opacity} })) +geom_point() + labs(x = "{{GroupingVariable}}", y = vars, color = "{{GroupBy}}", title = paste("Scatter plot for variable ", "{{GroupingVariable}}", " by ", vars, sep = '')) + xlab("{{xlab}}") + ylab("{{ylab}}") + ggtitle("{{maintitle}}") { { themes} }
                //+facet_grid({  { Facetcolumn} }
                //~{ { Facetrow} }, scales ={ { Facetscale} })  +facet_wrap(  { { Facetwrap} } )+geom_smooth(method = "{{sm}}", color = "{{color}}"))

                MatchCollection mcol = re.Matches(commandformat);
                foreach (Match m in mcol)
                {
                    string matchedText = m.Groups[1].Value;
                    string result = GetParam(obj, matchedText);
                    if (!CommandKeyValDict.ContainsKey(matchedText))
                    {
                        CommandKeyValDict.Add(matchedText, result);
                    }
                }

                string Destination = "";
                string Groupby = "";
                string size = "";
                string opacity = "";
                string Facetcolumn = "";
                string Facetrow = "";
                string Facetscale = "";
                string Facetwrap = "";
                string fill = "";
                // string color = "";
                string dataset = "";
                string xlab = "";
                string ylab = "";
                string maintitle = "";
                string jitter = "";
                string flipaxis = "";
                string shape = "";
                string rdgrp1 = "";
                string width = "";
                string yvariable = "";

                foreach (KeyValuePair<string, string> kv in CommandKeyValDict)
                {
                    string key = kv.Key;
                    string value = kv.Value;
                    //create final syntac in 'output'
                    // output = output+","+ key + "=c(" + value + ")";

                    if (key == "destination")
                    {
                        Destination = value;
                    }
                    if (key == "yvariable")
                    {
                        yvariable = value;
                    }
                    if (key == "Groupby")
                    {
                        Groupby = value;
                    }
                    if (key == "size")
                    {
                        size = value;
                    }
                    if (key == "opacity")
                    {
                        opacity = value;
                    }
                    if (key == "Facetcolumn")
                    {
                        Facetcolumn = value;
                    }
                    if (key == "Facetrow")
                    {
                        Facetrow = value;
                    }
                    if (key == "Facetscale")
                    {
                        Facetscale = value;
                    }
                    if (key == "Facetwrap")
                    {
                        Facetwrap = value;
                    }
                    if (key == "%DATASET%")
                    {
                        dataset = value;
                    }
                    if (key == "xlab")
                    {
                        xlab = value;
                    }
                    if (key == "ylab")
                    {
                        ylab = value;
                    }
                    if (key == "maintitle")
                    {
                        maintitle = value;
                    }

                    if (key == "jitter")
                    {
                        jitter = value;
                    }
                    if (key == "flipaxis")
                    {
                        flipaxis = value;
                    }
                    if (key == "width")
                    {
                        width = value;
                    }
                    if (key == "rdgrp1")
                    {
                        rdgrp1 = value;
                    }
                    if (key == "fill")
                    {
                        fill = value;
                    }
                }
                string tempoutput = "";
                string[] variables = Destination.Split(',');
                string ylabel = "Count";
                foreach (string var in variables)
                {
                    //x='' handles the case where you want a bar graph for mpg filled by whether the car is automatic or not
                    //library(ggplot2)
                    // bp < -ggplot(df, aes(x = "", y = value, fill = group)) +
                    // geom_bar(width = 1, stat = "identity")

                    if (var == "")
                    {
                        tempoutput = tempoutput + "ggplot(data=" + dataset + ", aes(x ='' ";
                    }
                    else
                    {
                        tempoutput = tempoutput + "ggplot(data=" + dataset + ", aes(x =" + var;
                    }

                    if (yvariable != "")
                    {
                        tempoutput = tempoutput + ",y=" + yvariable;
                        ylabel = yvariable;
                    }
                    if (Groupby != "")
                    {
                        tempoutput = tempoutput + ",fill=" + Groupby;
                    }

                    tempoutput = tempoutput + "))";

                    //Constructing geom_bar

                    //if (rdgrp1 == "stack")
                    //{
                    //    tempoutput = tempoutput + " +\n\t geom_bar( position =\"stack\" ";
                    //}
                    //else if (rdgrp1 == "dodge")
                    //{
                    //    tempoutput = tempoutput + " +\n\t geom_bar( position=\"dodge\" ";
                    //}

                    if (rdgrp1 == "fill")
                    {
                        tempoutput = tempoutput + " +\n\t geom_bar( position=\"fill\" ";
                    }
                    else
                    {
                        tempoutput = tempoutput + " +\n\t geom_bar( ";

                    }

                    if (opacity != "")
                    {
                        tempoutput = tempoutput + ",alpha=" + opacity;
                    }

                    if (width != "")
                    {
                        tempoutput = tempoutput + ",width =" + width;
                    }
                    if (yvariable != "")
                    {
                        tempoutput = tempoutput + " ,stat=\"identity\"";

                    }

                    tempoutput = tempoutput + ")";

                    tempoutput = tempoutput + " +\n\t coord_polar(\"x\")";


                    if (flipaxis == "TRUE")
                    {
                        tempoutput = tempoutput + " +\n\t coord_flip()";
                    }

                    //we add Fill to the Barplot title only when it exists
                    if (Groupby == "")
                    {
                        tempoutput = tempoutput + " +\n\t labs(x =" + "\"" + var + "\"" + ", y =" + "\"" + ylabel + "\"" + "," + "fill =" + "\"" + Groupby + "\"" + ", title= " + "\"Bulls Eye chart for X aesthetic: " + var + "  ,Y aesthetic: " + ylabel + "\")";
                    }
                    else
                    {
                        tempoutput = tempoutput + " +\n\t labs(x =" + "\"" + var + "\"" + ", y =" + "\"" + ylabel + "\"" + "," + "fill =" + "\"" + Groupby + "\"" + ", title= " + "\"Bulls Eye chart for X aesthetic: " + var + "  ,Y aesthetic: " + ylabel + "  ,Fill: " + Groupby + "\")";
                    }
                    if (xlab != "")
                    {
                        tempoutput = tempoutput + " +\n\t xlab(" + "\"" + xlab + "\"" + ")";
                    }

                    if (ylab != "")
                    {
                        tempoutput = tempoutput + " +\n\t ylab(" + "\"" + ylab + "\"" + ")";
                    }

                    if (maintitle != "")
                    {
                        tempoutput = tempoutput + " +\n\t ggtitle(" + "\"" + maintitle + "\"" + ")";
                    }

                    //+geom_smooth(method ="{{sm}}", color= "{{color}}")

                    tempoutput = tempoutput + createfacets(Facetwrap, Facetcolumn, Facetrow, Facetscale);
                    // tempoutput = Wrapinbrackets(tempoutput);
                    tempoutput = tempoutput + "\n\n";
                    output = output + tempoutput;

                    tempoutput = "";

                    output = output.TrimEnd(Environment.NewLine.ToCharArray());
                    output = output.TrimEnd(Environment.NewLine.ToCharArray());
                    output = output + " +\n" + themeSyntax + "\n\n";
                }

                //+facet_grid({ { Facetcolumn} } ~ {{ Facetrow} }, scales ={ { Facetscale} })  +facet_wrap(  { { Facetwrap} } )
            }

            else if (customsyntax == "Graphics-frequencynumeric")
            {
                //  ggplot(data = { {% DATASET %} }, aes(x = eval(parse(text = paste(vars))), colour = { { Groupby} }, group = { { Groupby} })) +geom_freqpoly(binwidth ={ { binwidth} }) { { flip} }
                //  +labs(x = vars, y = "Count", colour = "{{Groupby}}", title = paste("Frequency chart for variable ", vars, sep = '')) + xlab("{{xlab}}") + ylab("{{ylab}}") + ggtitle("{{maintitle}}") { { themes} })

                MatchCollection mcol = re.Matches(commandformat);
                foreach (Match m in mcol)
                {
                    string matchedText = m.Groups[1].Value;
                    string result = GetParam(obj, matchedText);
                    if (!CommandKeyValDict.ContainsKey(matchedText))
                    {
                        CommandKeyValDict.Add(matchedText, result);
                    }
                }

                string destination = "";
                string binwidth = "";
                string Groupby = "";
                string opacity = "";

                string Facetcolumn = "";
                string Facetrow = "";
                string Facetscale = "";
                string Facetwrap = "";

                string color = "";
                string dataset = "";
                string xlab = "";
                string ylab = "";
                string maintitle = "";
                string jitter = "";
                string flipaxis = "";

                string bordercolor = "";

                foreach (KeyValuePair<string, string> kv in CommandKeyValDict)
                {
                    string key = kv.Key;
                    string value = kv.Value;
                    //create final syntac in 'output'
                    // output = output+","+ key + "=c(" + value + ")";

                    if (key == "destination")
                    {
                        destination = value;
                    }
                    if (key == "binwidth")
                    {
                        binwidth = value;
                    }

                    if (key == "opacity")
                    {
                        opacity = value;
                    }
                    if (key == "Facetcolumn")
                    {
                        Facetcolumn = value;
                    }
                    if (key == "Facetrow")
                    {
                        Facetrow = value;
                    }
                    if (key == "Facetscale")
                    {
                        Facetscale = value;
                    }
                    if (key == "Facetwrap")
                    {
                        Facetwrap = value;
                    }
                    if (key == "%DATASET%")
                    {
                        dataset = value;
                    }
                    if (key == "xlab")
                    {
                        xlab = value;
                    }
                    if (key == "ylab")
                    {
                        ylab = value;
                    }
                    if (key == "maintitle")
                    {
                        maintitle = value;
                    }

                    if (key == "jitter")
                    {
                        jitter = value;
                    }
                    if (key == "flipaxis")
                    {
                        flipaxis = value;
                    }
                    if (key == "bordercolor")
                    {
                        bordercolor = value;
                    }
                    if (key == "color")
                    {
                        color = value;
                    }
                    if (key == "Groupby")
                    {
                        Groupby = value;
                    }
                }
                string tempoutput = "";
                string[] variables = destination.Split(',');

                foreach (string var in variables)
                {
                    tempoutput = tempoutput + "ggplot(data=" + dataset + ", aes(x =" + var;

                    //   ggplot(data = { {% DATASET %} }, aes(x = eval(parse(text = paste(vars))), colour = { { Groupby} }, group = { { Groupby} }))
                    if (Groupby != "")
                    {
                        tempoutput = tempoutput + ", colour =" + Groupby + ", group =" + Groupby;
                    }

                    tempoutput = tempoutput + "))";

                    //      +geom_freqpoly(binwidth ={ { binwidth} }) { { flip} }
                    tempoutput = tempoutput + " +\n\t geom_freqpoly(";



                    if (binwidth != "0")
                    {

                        tempoutput = tempoutput + ", binwidth =" + binwidth;
                    }

                    if (opacity != "")
                    {
                        tempoutput = tempoutput + ", alpha=" + opacity;
                    }

                    tempoutput = tempoutput + ")";


                    if (flipaxis == "TRUE")
                    {
                        tempoutput = tempoutput + " +\n\t coord_flip()";
                    }


                    //  +labs(x = vars, y = "Count", colour = "{{Groupby}}", title = paste("Frequency chart for variable ", vars, sep = '')) + xlab("{{xlab}}") + ylab("{{ylab}}") + ggtitle("{{maintitle}}") { { themes} })                   


                    if (Groupby == "")
                    {
                        tempoutput = tempoutput + " +\n\t labs(x =" + "\"" + var + "\"" + ", y =" + "\"" + "Counts" + "\"" + ", colour =" + "\"" + Groupby + "\"" + ", title= " + "\"Frequency chart for variable " + var + "\")";
                    }
                    else
                    {
                        tempoutput = tempoutput + " +\n\t labs(x =" + "\"" + var + "\"" + ", y =" + "\"" + "Counts" + "\"" + ", colour =" + "\"" + Groupby + "\"" + ", title= " + "\"Frequency chart for variable " + var + " in colors groups created by variable " + Groupby + "\")";
                    }

                    if (xlab != "")
                    {
                        tempoutput = tempoutput + " +\n\t xlab(" + "\"" + xlab + "\"" + ")";
                    }

                    if (ylab != "")
                    {
                        tempoutput = tempoutput + " +\n\t ylab(" + "\"" + ylab + "\"" + ")";
                    }

                    if (maintitle != "")
                    {
                        tempoutput = tempoutput + " +\n\t ggtitle(" + "\"" + maintitle + "\"" + ")";
                    }

                    //+geom_smooth(method ="{{sm}}", color= "{{color}}")

                    tempoutput = tempoutput + createfacets(Facetwrap, Facetcolumn, Facetrow, Facetscale);
                    // tempoutput = Wrapinbrackets(tempoutput);
                    tempoutput = tempoutput + "\n\n";
                    output = output + tempoutput;

                    tempoutput = "";

                    output = output.TrimEnd(Environment.NewLine.ToCharArray());
                    output = output.TrimEnd(Environment.NewLine.ToCharArray());
                    output = output + " +\n" + themeSyntax + "\n\n";
                }

                //+facet_grid({ { Facetcolumn} } ~ {{ Facetrow} }, scales ={ { Facetscale} })  +facet_wrap(  { { Facetwrap} } )
            }

            else if (customsyntax == "Graphics-frequencyfactor")
            {
                //  ggplot(data = { {% DATASET %} }, aes(x = eval(parse(text = paste(vars))), colour = { { Groupby} }, group = { { Groupby} })) +geom_freqpoly(binwidth ={ { binwidth} }) { { flip} }
                //  +labs(x = vars, y = "Count", colour = "{{Groupby}}", title = paste("Frequency chart for variable ", vars, sep = '')) + xlab("{{xlab}}") + ylab("{{ylab}}") + ggtitle("{{maintitle}}") { { themes} })

                MatchCollection mcol = re.Matches(commandformat);
                foreach (Match m in mcol)
                {
                    string matchedText = m.Groups[1].Value;
                    string result = GetParam(obj, matchedText);
                    if (!CommandKeyValDict.ContainsKey(matchedText))
                    {
                        CommandKeyValDict.Add(matchedText, result);
                    }
                }

                string destination = "";
                string binwidth = "";
                string Groupby = "";
                string opacity = "";

                string Facetcolumn = "";
                string Facetrow = "";
                string Facetscale = "";
                string Facetwrap = "";

                string color = "";
                string dataset = "";
                string xlab = "";
                string ylab = "";
                string maintitle = "";
                string jitter = "";
                string flipaxis = "";

                string bordercolor = "";

                foreach (KeyValuePair<string, string> kv in CommandKeyValDict)
                {
                    string key = kv.Key;
                    string value = kv.Value;
                    //create final syntac in 'output'
                    // output = output+","+ key + "=c(" + value + ")";

                    if (key == "destination")
                    {
                        destination = value;
                    }
                    if (key == "binwidth")
                    {
                        binwidth = value;
                    }

                    if (key == "opacity")
                    {
                        opacity = value;
                    }
                    if (key == "Facetcolumn")
                    {
                        Facetcolumn = value;
                    }
                    if (key == "Facetrow")
                    {
                        Facetrow = value;
                    }
                    if (key == "Facetscale")
                    {
                        Facetscale = value;
                    }
                    if (key == "Facetwrap")
                    {
                        Facetwrap = value;
                    }
                    if (key == "%DATASET%")
                    {
                        dataset = value;
                    }
                    if (key == "xlab")
                    {
                        xlab = value;
                    }
                    if (key == "ylab")
                    {
                        ylab = value;
                    }
                    if (key == "maintitle")
                    {
                        maintitle = value;
                    }

                    if (key == "jitter")
                    {
                        jitter = value;
                    }
                    if (key == "flipaxis")
                    {
                        flipaxis = value;
                    }
                    if (key == "bordercolor")
                    {
                        bordercolor = value;
                    }
                    if (key == "color")
                    {
                        color = value;
                    }
                    if (key == "Groupby")
                    {
                        Groupby = value;
                    }
                }
                string tempoutput = "";
                string[] variables = destination.Split(',');

                foreach (string var in variables)
                {
                    tempoutput = tempoutput + "ggplot(data=" + dataset + ", aes(x =" + var;

                    //   ggplot(data = { {% DATASET %} }, aes(x = eval(parse(text = paste(vars))), colour = { { Groupby} }, group = { { Groupby} }))
                    if (Groupby != "")
                    {
                        tempoutput = tempoutput + ", colour =" + Groupby + ", group =" + Groupby;
                    }


                    tempoutput = tempoutput + "))";

                    //      +geom_freqpoly(binwidth ={ { binwidth} }) { { flip} }
                    tempoutput = tempoutput + " +\n\t geom_freqpoly(";

                    tempoutput = tempoutput + ", stat = \"Count\"";

                    if (opacity != "")
                    {
                        tempoutput = tempoutput + ", alpha=" + opacity;
                    }

                    tempoutput = tempoutput + ")";


                    if (flipaxis == "TRUE")
                    {
                        tempoutput = tempoutput + " +\n\t coord_flip()";
                    }

                    //  +labs(x = vars, y = "Count", colour = "{{Groupby}}", title = paste("Frequency chart for variable ", vars, sep = '')) + xlab("{{xlab}}") + ylab("{{ylab}}") + ggtitle("{{maintitle}}") { { themes} })                   

                    if (Groupby == "")
                    {
                        tempoutput = tempoutput + " +\n\t labs(x =" + "\"" + var + "\"" + ", y =" + "\"" + "Counts" + "\"" + ", colour =" + "\"" + Groupby + "\"" + ", title= " + "\"Frequency chart for variable " + var + "\")";
                    }
                    else
                    {
                        tempoutput = tempoutput + " +\n\t labs(x =" + "\"" + var + "\"" + ", y =" + "\"" + "Counts" + "\"" + ", colour =" + "\"" + Groupby + "\"" + ", title= " + "\"Frequency chart for variable " + var + " in groups of different colors defined by \\n levels of variable " + Groupby + "\")";
                    }

                    if (xlab != "")
                    {
                        tempoutput = tempoutput + " +\n\t xlab(" + "\"" + xlab + "\"" + ")";
                    }

                    if (ylab != "")
                    {
                        tempoutput = tempoutput + " +\n\t ylab(" + "\"" + ylab + "\"" + ")";
                    }

                    if (maintitle != "")
                    {
                        tempoutput = tempoutput + " +\n\t ggtitle(" + "\"" + maintitle + "\"" + ")";
                    }

                    //+geom_smooth(method ="{{sm}}", color= "{{color}}")

                    tempoutput = tempoutput + createfacets(Facetwrap, Facetcolumn, Facetrow, Facetscale);
                    // tempoutput = Wrapinbrackets(tempoutput);
                    tempoutput = tempoutput + "\n\n";
                    output = output + tempoutput;

                    tempoutput = "";

                    output = output.TrimEnd(Environment.NewLine.ToCharArray());
                    output = output.TrimEnd(Environment.NewLine.ToCharArray());
                    output = output + " +\n" + themeSyntax +"\n\n";
                }

                //+facet_grid({ { Facetcolumn} } ~ {{ Facetrow} }, scales ={ { Facetscale} })  +facet_wrap(  { { Facetwrap} } )
            }

            else
            {
                MessageBox.Show("The key used to invoke custom C# code is not supported. You are either using an old version of BlueSky Statistics or have created a dialog incorrectly. Contact support at support@blueskystatistics.com and attach the dialog you are using.");

            }

            //Added by Aaron 09/01/2013
            //A variable list control does not have to return a value. For example the layers variable of a crosstab can be empty. 
            //in this case layers returns "". This gets replaced in the command syntax as layers = c("")
            //What R understands is layers =NA
            // finaloutput = finaloutput.Replace("c()", "NA");
            output = handleLayersInCrosstabs(output); // This removes all lines with c()
            //output = Regex.Replace(output, @"%>%\s*\n", "%>% ");//removes new line after a pipe sign (newline-tab will be added later/below to format it)
            //output = output.Replace("BSkyLoadRefreshDataframe", "\n\nBSkyLoadRefreshDataframe");//output = Regex.Replace(output, @"\n+", "\n\n");//one blank line between statements
            //output = Regex.Replace(output, @"\n{", "{");//remove extra newline before open curly (got inserted because of the above line)
            output = output.Replace("%>%", "%>%\n\t");//R statement is broken in pipe and second part of the statment goes to next line and is indented with tab
            output = RemoveParametersWithNoValuesInCommandSyntax(output);
            output = FixExtraCommasInCommandSyntax(output);//14Jul2014
            return output;
        }

        private static string Wrapinbrackets(string output)
        {

            return ("print(" + output + ")");

        }

        private static string createfacets(string Facetwrap, string Facetcolumn, string Facetrow, string Facetscale)
        {
            string output = "";
            if (Facetscale == "free_x_and_y")
            {
                Facetscale = "free";
            }

            if (!(Facetwrap == "" || Facetwrap == null))
            {

                if (Facetscale == "none")
                {
                    output = output + " +\n\t facet_wrap(~" + Facetwrap + ")";
                }
                else
                {
                    output = output + " +\n\t facet_wrap(~" + Facetwrap + ",scales=" + "\"" + Facetscale + "\"" + ")";
                }


            }
            else if (!(Facetrow == "" || Facetrow == null) && !(Facetcolumn == "" || Facetcolumn == null))
            {

                if (Facetscale == "none")
                {
                    output = output + " +\n\t facet_grid(" + Facetcolumn + "~" + Facetrow + ")";
                }
                else
                {
                    output = output + " +\n\t facet_grid(" + Facetcolumn + "~" + Facetrow + ",scales=" + "\"" + Facetscale + "\"" + ")";

                }
            }
            else
            {
                if (!(Facetrow == "" || Facetrow == null))
                {

                    if (Facetscale == "none")
                    {
                        output = output + " +\n\t facet_grid(" + Facetrow + "~ ." + ")";

                    }
                    else
                    {
                        output = output + " +\n\t facet_grid(" + Facetrow + "~ ." + ",scales =" + "\"" + Facetscale + "\"" + ")";
                    }


                }
                else if ((!(Facetcolumn == "" || Facetcolumn == null)))
                {
                    if (Facetscale == "none")
                    {
                        output = output + " +\n\t facet_grid(" + ". ~" + Facetcolumn + ")";
                    }
                    else
                    {
                        output = output + " +\n\t facet_grid(" + ". ~" + Facetcolumn + ",scales =" + "\"" + Facetscale + "\"" + ")";
                    }
                }
            }
            return output;
        }

        //Added by Aaron 09/01/2013
        //A variable list control does not have to return a value. For example the layers variable of a crosstab can be empty. 
        //in this case layers returns "". This gets replaced in the command syntax as layers = c("")
        //What R understands is layers =NA

        //Added by Aaron 06/16/2015
        //Cross tab does not expect to see layers =c(). We can remove layers completely and crosstab runs correctly
        //I have tested the above. The code that generates the table causes a problem. THe R code runs correctly when x is store, y is overall. But the XML fails
        //To recreate
        //  BSky_Multiway_Cross_Tab = BSkyCrossTable(x=c('store'),y=c('overall'),datasetname='Dataset2',chisq = FALSE,prop.r=FALSE,prop.c=FALSE,resid=FALSE,sresid=FALSE,expected=FALSE,asresid=FALSE)
        //BSkyFormat(BSky_Multiway_Cross_Tab) fails
        private static string handleLayersInCrosstabs(string inputtext)
        {
            string pattern = @"[A-Za-z0-9_.]+\s*=\s*c\(\)";
            //  string pattern = @"\s*=\s*c\(\)";
            bool str = Regex.IsMatch(inputtext, pattern);
            MatchCollection mc = null;

            if (str)
            {
                mc = Regex.Matches(inputtext, pattern);

                foreach (Match m in mc)
                {
                    inputtext = Regex.Replace(inputtext, pattern, "", RegexOptions.None);
                }
            }
            return inputtext;
        }

        private static string FixExtraCommasInCommandSyntax(string inputtext) //14Jul2014 remove extra commas from command
        {
            //Step 1.// Pattern to match in phase 1
            string pattern = @"\s*\,+\s*";//@"\s*[\,]\s*[\,]"; //set of adjecent commas(eg. ,,, , ,,,,, are 3 sets)

            //// Finding pattern and replacing it with something
            bool str = Regex.IsMatch(inputtext, pattern);
            MatchCollection mc = null;
            MatchCollection mcsubstring = null;

            while (str)
            {
                mc = Regex.Matches(inputtext, pattern);
                if (mc.Count > 0)
                {
                    inputtext = Regex.Replace(inputtext, pattern, ",", RegexOptions.None);
                }
                str = Regex.IsMatch(inputtext, @"\s*\,\s*\,");//look if, after replacement, adjacent commas are present 
            }

            //Step 2.// Pattern to match in phase 2
            pattern = @"\s*[\,]\s*[\)]"; // comma and closing ')'

            //// Finding pattern and replacing it with something
            str = Regex.IsMatch(inputtext, pattern);
            mc = Regex.Matches(inputtext, pattern);
            if (mc.Count > 0)
            {
                inputtext = Regex.Replace(inputtext, pattern, ")", RegexOptions.None);
            }

            //Step 3.// Pattern to match in phase 3
            pattern = @"[\(]\s*[\,]\s*"; // opening '(' and comma

            //// Finding pattern and replacing it with something
            str = Regex.IsMatch(inputtext, pattern);
            mc = Regex.Matches(inputtext, pattern);
            if (mc.Count > 0)
            {
                inputtext = Regex.Replace(inputtext, pattern, "(", RegexOptions.None);
            }

            //Aaron 07/09/2015
            //stripping out facet_grid(.~.)
            pattern = @"\+\s*facet_grid\(\s*\.\s*~\s*\.\s*\)";
            str = Regex.IsMatch(inputtext, pattern);
            mc = Regex.Matches(inputtext, pattern);
            if (mc.Count > 0)
            {
                inputtext = Regex.Replace(inputtext, pattern, "", RegexOptions.None);
            }

            //Handling the challenge posed by plotMeans
            //The syntax below shows the problem with plot means
            //test123 <- summarySE(Dataset2,measurevar="hp",groupvars=c("transmission",""))
            //ggplot(test123,aes(x=transmission,y=hp)) + geom_errorbar(aes(ymin=hp-se,ymax=hp+se),width=.1) +geom_line() +geom_point() 
            //the issue is that summarySE requireds "" around the grouping variable which is optional but 
            //ggplot does not
            //with the "" in the groupvars in the summarySE, the summarySE fails.
            //What we will do is look for ,"" in c() and strip it out

            //This looks for c("XXX","")
            pattern = @"c\s*\(\s*\""[A-Za-z]+\""\,\""\""\s*\)";

            //This looks for ,""
            string replacepattern = @"\,\""\""";
            string replacestring = null;
            str = Regex.IsMatch(inputtext, pattern);
            mc = Regex.Matches(inputtext, pattern);
            string matchstring = null;

            foreach (Match match in mc)
            {
                matchstring = match.Value;
                mcsubstring = Regex.Matches(matchstring, replacepattern);
                //The replacestring strips out the ,"" from c("transmission","")
                replacestring = Regex.Replace(matchstring, replacepattern, "", RegexOptions.None);
                //This replaces c("XXX","") with c("XXX")
                inputtext = Regex.Replace(inputtext, pattern, replacestring, RegexOptions.None);
                // Console.WriteLine("Found '{0}' at position {1}",
                //                match.Value, match.Index);
            }

            return (inputtext);
        }

        private static string RemoveParametersWithNoValuesInCommandSyntax(string inputtext) //14Jul2014 remove extra commas from command
        {
            //Looking for pattern ,prefix=\"\"
            //BSkyFARes <-BSkyFactorAnalysis(vars=c('mpg','engine','horse','weight','accel'), factors=2, screeplot =FALSE,rotation=\"none\", saveScores =FALSE, prefixForScores=\"\",dataset=\"Dataset2\")
            string pattern = @"\,\s*[A-Za-z0-9_.]+\s*=\s*\""\"""; //18Jun2016 \s* added in the begining becuase there may be zero or more spaces after comma.
            //// Finding pattern and replacing it with something
            bool str = Regex.IsMatch(inputtext, pattern);
            MatchCollection mc = null;

            if (str) //No need of while here. before 18Jun2016 -> while (str)
            {
                mc = Regex.Matches(inputtext, pattern);
                if (mc.Count > 0)
                {
                    //inputtext = Regex.Replace(inputtext, pattern, "", RegexOptions.None); //before 18Jun2016
                    //18Jun2016. Following is added in place of above the statement. The reason being we dont want to remove 
                    // ,replacement="" (with comma) from our Compute dialog expression. Other than this we remove all other parameter
                    // those do not have any value, like ,prefix=""
                    inputtext = Regex.Replace(inputtext, pattern,
                    delegate (Match match)
                    {
                        string v = match.ToString();

                        if (!v.Contains("replacement=\"\"")) // if (!v.Equals(",replacement=\"\"")) // comma is a issue for Equals. There may b zero or more spaces after comma
                            return "";
                        else
                            return v;
                    });
                }
                str = Regex.IsMatch(inputtext, pattern);//look for more of the same pattern  
            }

            //Looking for pattern prefix=\"\"
            pattern = @"[A-Za-z0-9_.]+\s*=\s*\""\""";

            str = Regex.IsMatch(inputtext, pattern);
            if (str) //No need of while here. before 18Jun2016 -> while (str)
            {
                mc = Regex.Matches(inputtext, pattern);
                if (mc.Count > 0)
                {
                    //inputtext = Regex.Replace(inputtext, pattern, "", RegexOptions.None); //before 18Jun2016
                    //18Jun2016. Following is added in place of above the statement. The reason being we dont want to remove 
                    // replacement=""  (without comma)  from our Compute dialog expression. Other than this we remove all other parameter
                    // those do not have any value, like prefix=""
                    inputtext = Regex.Replace(inputtext, pattern,
                    delegate (Match match)
                    {
                        string v = match.ToString();

                        if (!v.Equals("replacement=\"\""))
                            return "";
                        else
                            return v;
                    });
                }
                str = Regex.IsMatch(inputtext, pattern);
            }

            //Looking for pattern prefix=,
            // state_choropleth(BSkyDfForMap, title=, legend=, zoom=)"
            //Above is in state map
            pattern = @"[A-Za-z0-9_.]+\s*=\s*,";
            //// Finding pattern and replacing it with something
            str = Regex.IsMatch(inputtext, pattern);
            mc = null;
            while (str)
            {
                mc = Regex.Matches(inputtext, pattern);
                if (mc.Count > 0)
                {
                    inputtext = Regex.Replace(inputtext, pattern, "", RegexOptions.None);
                }
                str = Regex.IsMatch(inputtext, pattern);//look for more of the same pattern  
            }

            //Looking for pattern 'prefix=)'
            //Example string is "BSkyDfForMap =data.frame(region=Dataset2[,c(\"region\")], value =Dataset2[,c(\"value\")])\nstate_choropleth(BSkyDfForMap,   zoom=)"
            pattern = @"[A-Za-z0-9_.]+\s*=\s*\)";
            //// Finding pattern and replacing it with something
            str = Regex.IsMatch(inputtext, pattern);
            mc = null;
            while (str)
            {
                mc = Regex.Matches(inputtext, pattern);
                if (mc.Count > 0)
                {
                    inputtext = Regex.Replace(inputtext, pattern, ")", RegexOptions.None);
                }
                str = Regex.IsMatch(inputtext, pattern);//look for more of the same pattern  
            }

            //Added(but not used) by Anil:10May2017 to remove occurrences of 'var=\n' from syntax. 
            //Pattern is same as the above case but no closing ')' , instead a new-line character is present.
            //Looking for pattern: prefix=
            //Example string is "fromidx ="
            ////pattern = @"[A-Za-z0-9_.]+\s*=\s*\n";
            //////// Finding pattern and replacing it with something
            ////str = Regex.IsMatch(inputtext, pattern);
            ////mc = null;
            ////while (str)
            ////{
            ////    mc = Regex.Matches(inputtext, pattern);
            ////    if (mc.Count > 0)
            ////    {
            ////        inputtext = Regex.Replace(inputtext, pattern, "", RegexOptions.None);
            ////    }
            ////    str = Regex.IsMatch(inputtext, pattern);//look for more of the same pattern  
            ////}

            //Added by Aaron 06/09/2015
            //handles +xlab() ,+ylab()+ggtitle() fot ggplots
            //when these strings are found they habe to be removed as they generate an error
            //However +coord_flip(), +theme_bw() etc should not be replaced

            pattern = @"\+\s*[A-Za-z0-9_.]+\s*\(\s*\""\""\)";
            str = Regex.IsMatch(inputtext, pattern);
            mc = null;

            if (str)
            {
                mc = Regex.Matches(inputtext, pattern);

                foreach (Match m in mc)
                {
                    if (m.Value.Contains("xlab") || m.Value.Contains("ylab") || m.Value.Contains("ggtitle"))
                    {
                        inputtext = Regex.Replace(inputtext, pattern, "", RegexOptions.None);
                    }
                }

            }

            return (inputtext);
        }

        public static string GetParam(DependencyObject obj, string paramname)
        {
            string tempVal = string.Empty;

            if (OutputHelper.ExpandMacro(paramname) != paramname)
            {
                tempVal = OutputHelper.ExpandMacro(paramname);
            }
            else
            {
                tempVal = EvaluateValue(obj, paramname);
            }

            return tempVal;
        }

        public static string[] GetParams(DependencyObject obj, string commandformat)
        {
            string[] splits = commandformat.Split(new string[] { "##" }, StringSplitOptions.None);
            string[] param = splits.Skip(1).ToArray();
            string[] paramvalues = new string[param.GetLength(0)];
            int i = 0;

            foreach (string s in param)
            {
                string tempVal = string.Empty;

                if (OutputHelper.ExpandMacro(s) != s)
                {
                    tempVal = OutputHelper.ExpandMacro(s);
                }
                else
                {
                    tempVal = EvaluateValue(obj, s);
                }
                paramvalues[i++] = tempVal;
            }
            return paramvalues;
        }

        private static void GetSplitsDataMatrix(string[] splits, int lstIndex, int datanumber, string[,] matrix, bool[] visibleRows)
        {
            if (lstIndex == splits.Count() - 1)
            {
                for (int index = 0; index < GetFactors(splits[lstIndex]).Count(); ++index)
                {
                    int tempIndex = (dataMatrixIndex) * TotalOutputTables + datanumber;
                    XmlDocument doc = AnalyticsData.Result.Data;
                    XmlNode rownode = doc.SelectSingleNode(string.Format("/Root/UATableList/UADoubleMatrix[{0}]/rows", tempIndex));

                    if (rownode == null)
                    {
                        visibleRows = new bool[1];
                        matrix = new string[1, 1];
                        return;
                    }

                    int rows = rownode.ChildNodes.Count;
                    XmlNode temp = doc.SelectSingleNode(string.Format("/Root/UATableList/UADoubleMatrix[{0}]/rows/row/columns", tempIndex));
                    int cols = temp.ChildNodes.Count;

                    XmlNode metadata = doc.SelectSingleNode(string.Format("/Root/UATableList/Metadata/crosstab/UADoubleMatrix/rows", tempIndex));

                    if (metadata != null)
                    {
                        rows = metadata.ChildNodes.Count;
                    }

                    int j = 0;
                    int rownodecounter = 0;

                    for (int i = 0; i < rows; ++i)
                    {
                        XmlNode node = rownode.ChildNodes[rownodecounter];

                        if (metadata != null)
                        {
                            XmlNode tempm = metadata.ChildNodes[i].SelectSingleNode("./columns/column[2]");

                            if (tempm.InnerText == "0")
                            {
                                continue;
                            }
                        }
                        visibleRows[i + dataMatrixIndex * rows] = true;
                        j = 0;
                        foreach (XmlNode cnode in node.FirstChild.ChildNodes)
                        {
                            matrix[i + dataMatrixIndex * rows, j] = cnode.InnerText;
                            j++;
                        }
                        rownodecounter++;
                    }
                    dataMatrixIndex++;
                }
            }
            else
            {
                for (int index = 0; index < GetFactors(splits[lstIndex]).Count(); ++index)
                {
                    GetSplitsDataMatrix(splits, lstIndex + 1, datanumber, matrix, visibleRows);
                }
            }

        }

        private static int dataMatrixIndex = 1;

        public static string[,] GetFootnotes(int datanumber)
        {
            //string[,] matrix = new string[1, 5];
            //matrix[0, 0] = "Foot notes from code";
            //return matrix;
            XmlDocument doc = AnalyticsData.Result.Data;
            string[,] matrix = null;
            XmlNode metadata = doc.SelectSingleNode(string.Format("/Root/UATableList/Metadata[@tablenumber={0}]/normal/UADoubleMatrix/rows", datanumber));

            if (metadata == null)
            {
                return matrix;
            }
            else
            {
                int rows = metadata.ChildNodes.Count;
                XmlNode temp = doc.SelectSingleNode(string.Format("/Root/UATableList/Metadata[@tablenumber={0}]/normal/UADoubleMatrix/rows/row/columns", datanumber));
                int cols = temp.ChildNodes.Count;
                matrix = new string[rows, cols];
                for (int i = 0; i < rows; ++i)
                {
                    for (int j = 0; j < cols; ++j)
                    {
                        XmlNode tempm = metadata.ChildNodes[i].SelectSingleNode(string.Format("./columns/column[{0}]", j + 1));
                        matrix[i, j] = tempm.InnerText;
                    }
                }
            }
            return matrix;
        }

        public static string[,] GetNotes()//use DOM and get uasummary strings
        {
            //string notes = string.Empty;
            string innertxt = string.Empty;
            XmlDocument doc = AnalyticsData.Result.Data;

            if (doc == null) return null;
            //listnode may be null if UASummary is not returned from R function
            XmlNode listnode = doc.SelectSingleNode(string.Format("/Root/UASummary/UAList"));
            int notescount = listnode != null ? listnode.ChildNodes.Count : 0; // 1 replaced by 0 on 03jul2013
            string[] rowheaders = { "File path", "Active dataset", "Filter", "Weights", "Split variables", "Total rows", "Command", "User.Self", "Elapsed", "?", "?" };//must be equal to notescount
            string[,] uasummary = new string[notescount, 2];

            if (notescount == 0) uasummary = null; //03Jul2013
            if (listnode != null)
            {
                for (int j = 0; j < notescount; ++j)
                { //headers can be set as an attribute in DOM of same UAString.Right now its hardcoded above.

                    XmlNode tempm = listnode.SelectSingleNode(string.Format("./UAString[{0}]", j + 1));
                    innertxt = tempm.InnerText;
                    if (innertxt == null || innertxt.Trim().Length < 1)
                        innertxt = "-none-";
                    //notes = notes + innertxt + "\n";
                    uasummary[j, 0] = rowheaders[j];
                    uasummary[j, 1] = innertxt;
                }
            }
            //return notes;
            return uasummary;
        }

        public static int FlexGridMaxCells;
        //public static int FlexGridMaxRows;
        //public static int FlexGridMaxCols;
        //returns the 2D matrix that contains the data
        public static string[,] GetDataMatrix(int datanumber, out bool[] visiblerows)
        {
            XmlDocument doc = AnalyticsData.Result.Data;

            if (doc == null)//24Dec2015 doc could be null when command fails. App should not crash is its null.
            {
                visiblerows = null;
                return null;
            }
            // Following XML string will only look for UADoubleMatrix so a temp fix is done in RService to make UAIntMatrix to UADoubleMatrix 09Jan2013
            XmlNode rownode = doc.SelectSingleNode(string.Format("/Root/UATableList/UADoubleMatrix[{0}]/rows", datanumber));

            if (rownode == null)
            {
                visiblerows = new bool[1];
                return null;
                //29Apr2014 return new string[1, 1];
            }

            int rows = rownode.ChildNodes.Count;
            XmlNode temp = doc.SelectSingleNode(string.Format("/Root/UATableList/UADoubleMatrix[{0}]/rows/row/columns", datanumber));
            int cols = temp.ChildNodes.Count;

            #region Setting Max Rows ( or Max Cols ) to process for painting in grid.
            int configtotalcells = FlexGridMaxCells; //right now this value is assumed as the number of cells to be shown in Flexgrid
            int actualtotalcell = rows * cols;
            int customMaxRows = configtotalcells / cols;
            //int customMaxRows = FlexGridMaxRows;//set it from options settings in OutputReader
            //int customMaxCols = FlexGridMaxCols;//set it from options settings in OutputReader
            if (actualtotalcell > configtotalcells)// || cols > customMaxCols)
            {
                //Mouse.OverrideCursor = null;
                LargeResultWarningWindow lrww = new LargeResultWarningWindow(rows, customMaxRows, configtotalcells);
                //lrww.RowCount = customMaxRows;//set max row for message
                lrww.ShowDialog();
                //Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
                string selectedoption = lrww.KeyPressed;//out of Full Part and Cancel

                switch (selectedoption)
                {
                    case "Full": //show full table
                        break;
                    case "Part": //show partial data
                        rows = customMaxRows;
                        //cols = customMaxCols;
                        break;
                    default:  //cancel out;

                        string[,] cancelmatrix = new string[1, 1];
                        cancelmatrix[0, 0] = "Abort";
                        //cancelmatrix[0, 1] = "";
                        visiblerows = null;
                        return cancelmatrix;
                }
                //Mouse.OverrideCursor = null;
            }

            #endregion
            XmlNode metadata = doc.SelectSingleNode(string.Format("/Root/UATableList/Metadata/crosstab/UADoubleMatrix/rows", datanumber));

            if (metadata != null)
            {
                rows = metadata.ChildNodes.Count;
            }

            if ((AnalyticsData.DataSource != null) && OutputHelper.GetGlobalMacro(string.Format("GLOBAL.{0}.SPLIT", AnalyticsData.DataSource.Name), "Comparegroups") == "TRUE")
            {
                List<string> splitVars = OutputHelper.GetList(string.Format("GLOBAL.{0}.SPLIT.SplitsVars", AnalyticsData.DataSource.Name), "", false);
                int totalvars = 1;

                foreach (string str in splitVars)
                {
                    totalvars = GetFactors(str).Count * totalvars;
                }

                int totalrows = totalvars * rows;
                string[,] matrix = new string[totalrows, cols];
                visiblerows = new bool[totalrows];
                dataMatrixIndex = 0;
                GetSplitsDataMatrix(splitVars.ToArray(), 0, datanumber, matrix, visiblerows);
                return matrix;
            }
            else
            {
                string[,] matrix = new string[rows, cols];
                visiblerows = new bool[rows];

                int j = 0;
                int rownodecounter = 0;

                for (int i = 0; i < rows; ++i)
                {
                    XmlNode node = rownode.ChildNodes[rownodecounter];

                    if (metadata != null)
                    {
                        XmlNode tempm = metadata.ChildNodes[i].SelectSingleNode("./columns/column[2]");

                        if (tempm.InnerText == "0")
                        {
                            continue;
                        }
                    }
                    //visiblerows[i] = true;//comm by Anil19feb2012
                    j = 0;
                    foreach (XmlNode cnode in node.FirstChild.ChildNodes)
                    {
                        if (!cnode.InnerText.Equals("NA"))//if cond. added by Anil 19Feb2012
                            visiblerows[i] = true;
                        if (cnode.InnerText.Contains("bskyCurrentDatasetSplitSliceObj"))//For changing slice object name with current dataset name
                        {
                            string currentDataset = ExpandMacro("%DATASET%");
                            matrix[i, j] = cnode.InnerText.Replace("bskyCurrentDatasetSplitSliceObj", currentDataset);  //currentDataset;
                        }
                        else
                        {
                            matrix[i, j] = cnode.InnerText;
                        }
                        j++;
                    }
                    rownodecounter++;
                }
                return matrix;
            }
        }

        //// //// 16Aug2013 Replace by GetFullMetadataTable. Following would not be used further.
        ////for getting error messages from metadata in DOM. AD 02Mar2012
        //public static string[,] GetMetaData(int datanumber, string metadatatabletype)
        //{
        //    XmlDocument doc = AnalyticsData.Result.Data;
        //    string[,] matrix = null;
        //    XmlNode metadata = doc.SelectSingleNode(string.Format("/Root/UATableList/Metadata[@tablenumber={0}]", datanumber));
        //    if (metadata == null || metadatatabletype == null)
        //    {
        //        return matrix;
        //    }
        //    else
        //    {
        //        int rows = 0;  
        //        XmlNode temp = doc.SelectSingleNode(string.Format("/Root/UATableList/Metadata[@tablenumber={0}]/{1}/UAList", datanumber, metadatatabletype));
        //        if (temp == null)
        //            return matrix;
        //        int cols = temp.ChildNodes.Count;
        //        for (int i = 0; i < cols; i++)
        //        {
        //            if (rows < temp.ChildNodes.Item(i).ChildNodes.Count)
        //                rows = temp.ChildNodes.Item(i).ChildNodes.Count;
        //        }

        //        matrix = new string[rows, cols]; 
        //        for (int i = 0; i < cols; ++i)
        //        {
        //            for (int j = 0; j < temp.ChildNodes.Item(i).ChildNodes.Count; ++j)
        //            {
        //                //XmlNode tempm = metadata.ChildNodes[i].SelectSingleNode(string.Format("./normal/UAString[{0}]", j + 1));////fix this 02Mar2012
        //                string s = temp.ChildNodes.Item(i).ChildNodes.Item(j).InnerText;
        //                if (s != null && !s.Trim().Equals("."))
        //                {
        //                    if (i == 1)
        //                    {
        //                        switch (s)
        //                        {
        //                            case "-1":
        //                                s = "Error:";
        //                                break;
        //                            case "-2":
        //                                s = "Critical Error:";
        //                                break;
        //                            case "1":
        //                                s = "Warning:";
        //                                break;
        //                            case "2": // Footer
        //                                s = "Footer:";
        //                                break;
        //                            default:
        //                                s = "";
        //                                break;
        //                        }
        //                    }
        //                    else if (i == 3)//row
        //                    { s = "Row " + s; }
        //                    else if (i == 4)//start
        //                    { s = "From col " + s; }
        //                    else if (i == 5)//ends
        //                    { s = "To col " + s; }

        //                    matrix[j, i] = s;
        //                }
        //                else
        //                {
        //                    matrix[j, i] = "";//space char
        //                }
        //            }
        //        }
        //    }
        //    return matrix;
        //}

        /// Get list of colnames. BSkyReturnStructure$Tables[[]]$columnNames
        public static List<string> GetKeepRemoveColNames(int datanumber)
        {
            List<string> ColNames = new List<string>();
            string innertxt = string.Empty;
            XmlDocument doc = AnalyticsData.Result.Data;

            if (doc == null) return null;
            //listnode may be null if $columnNames is not present in BSkyReturnStructure
            //11Jun2017 if there is just one colname in $columnNames then UAString appears instead of UAStringList and 
            // <row> will not appear as its a single item. So data is kept directly inside <UAString> tags for single value.

            //First get the tag inside of ColNames tag. It could be NULL, 1 or 1+
            XmlNode colNameNode = doc.SelectSingleNode(string.Format("/Root/UATableList/ColNames[@tablenumber={0}]", datanumber));

            if (colNameNode != null) //find if 1 or 1+ childnodes
            {
                XmlNode UAStrORList = colNameNode.SelectSingleNode(string.Format("UAStringList"));//is it a list. 1+ $columnNames?

                if (UAStrORList != null)//it is probably 1+ $columnName
                {
                    XmlNode listnode = doc.SelectSingleNode(string.Format("/Root/UATableList/ColNames[@tablenumber={0}]/UAStringList", datanumber));
                    int colcount = listnode != null ? listnode.ChildNodes.Count : 0; // 1 replaced by 0 on 03jul2013

                    if (listnode != null)
                    {
                        for (int j = 0; j < colcount; ++j)
                        { //headers can be set as an attribute in DOM of same UAString.Right now its hardcoded above.

                            XmlNode tempm = listnode.SelectSingleNode(string.Format("row[{0}]", j + 1));
                            innertxt = tempm.InnerText;
                            if (innertxt == null || innertxt.Trim().Length < 1)
                                innertxt = "-none-";

                            ColNames.Add(innertxt);
                        }
                    }
                }
                else //it is probably 1 $columnName
                {
                    UAStrORList = colNameNode.SelectSingleNode(string.Format("UAString"));// it is a single value in $columnNames?
                    if (UAStrORList != null)//its probably 1 $columnName
                    {
                        XmlNode singlenode = doc.SelectSingleNode(string.Format("/Root/UATableList/ColNames[@tablenumber={0}]/UAString", datanumber));
                        int colcount = singlenode != null ? singlenode.ChildNodes.Count : 0; // 1 replaced by 0 on 03jul2013

                        if (singlenode != null)
                        {
                            innertxt = UAStrORList.InnerText;
                            if (innertxt == null || innertxt.Trim().Length < 1)
                                innertxt = "-none-";

                            ColNames.Add(innertxt);
                        }
                    }
                }
            }
            else //if no ColNames tag is in DOM. Means do nothing with cols. Keep them as they are.(no remove no keep list)
            {
                return null;
            }
            return ColNames;
        }

        ////for getting error messages from metadata in DOM. AD 02Mar2012

        public static MetadataTable GetFullMetadataTable(int datanumber, string metadatatabletype)
        {
            XmlDocument doc = AnalyticsData.Result.Data;
            MetadataTable mat = null;
            XmlNode metadata = doc.SelectSingleNode(string.Format("/Root/UATableList/Metadata[@tablenumber={0}]", datanumber));

            if (metadata == null || metadatatabletype == null)
            {
                return mat;
            }
            else
            {
                int rows = 0;
                XmlNode temp = doc.SelectSingleNode(string.Format("/Root/UATableList/Metadata[@tablenumber={0}]/{1}/UAList", datanumber, metadatatabletype));

                if (temp == null)
                    return mat;

                int cols = temp.ChildNodes.Count;

                for (int i = 0; i < cols; i++)
                {
                    if (rows < temp.ChildNodes.Item(i).ChildNodes.Count)
                        rows = temp.ChildNodes.Item(i).ChildNodes.Count;
                }

                mat = new MetadataTable();
                mat.Metadatatable = new MetadataTableRow[rows]; // allocating required number of rows in metadatatable
                for (int k = 0; k < rows; k++)//creating rows objects
                {
                    mat.Metadatatable[k] = new MetadataTableRow();
                }
                for (int i = 0; i < cols; ++i)
                {
                    for (int j = 0; j < temp.ChildNodes.Item(i).ChildNodes.Count; ++j)
                    {
                        MetadataTableRow mtr = mat.Metadatatable[j]; // working in one row of the table
                        //XmlNode tempm = metadata.ChildNodes[i].SelectSingleNode(string.Format("./normal/UAString[{0}]", j + 1));////fix this 02Mar2012
                        string s = temp.ChildNodes.Item(i).ChildNodes.Item(j).InnerText;

                        if (s != null)// && !s.Trim().Equals("."))//now 'else' is not required.
                        {
                            int intout; //for few numeric fields
                            Int32.TryParse(s, out intout);

                            switch (i)
                            {
                                case 0:
                                    mtr.VarIndex = intout;
                                    break;
                                case 1:
                                    switch (s)
                                    {
                                        case "-1":
                                            s = "Error";
                                            break;
                                        case "-2":
                                            s = "Critical Error";
                                            break;
                                        case "1":
                                            s = "Warning";
                                            break;
                                        case "2": // Footer
                                            s = "Footer";
                                            break;
                                        default:
                                            s = "";
                                            break;
                                    }
                                    mtr.InfoType = s;
                                    break;
                                case 2:
                                    mtr.VarName = s;
                                    break;
                                case 3:
                                    mtr.DataTableRow = intout;
                                    break;
                                case 4:
                                    mtr.StartCol = intout;
                                    break;
                                case 5:
                                    mtr.EndCol = intout;
                                    break;
                                case 6:
                                    mtr.BSkyMsg = s;
                                    break;
                                case 7:
                                    mtr.RMsg = s;
                                    break;
                                default:
                                    break;
                            }
                        }
                        else
                        {
                            mtr.VarIndex = 0;
                            mtr.InfoType = "-";
                            mtr.VarName = "-";
                            mtr.DataTableRow = 0;
                            mtr.StartCol = 0;
                            mtr.EndCol = 0;
                            mtr.BSkyMsg = "-";
                            mtr.RMsg = "-";
                        }

                    }//for row
                }//for col
            }
            return mat;
        }

        //reading other metadata tables. ie. metadatatable[2] and [3]
        public static string[,] GetMetaData2(int datanumber, string metadatatabletype)//for getting error messages from metadata in DOM. AD 02Mar2012
        {
            XmlDocument doc = AnalyticsData.Result.Data;
            string[,] matrix = null;
            //XmlNode rownode = doc.SelectSingleNode(string.Format("/Root/UATableList/UADoubleMatrix[{0}]/rows", datanumber));
            XmlNode rownode = doc.SelectSingleNode(string.Format("/Root/UATableList/Metadata[@tablenumber={0}]/{1}/UADoubleMatrix/rows", datanumber, metadatatabletype));

            if (rownode == null)
            {
                return matrix;
            }

            int rows = rownode.ChildNodes.Count;
            XmlNode temp = doc.SelectSingleNode(string.Format("/Root/UATableList/Metadata[@tablenumber={0}]/{1}/UADoubleMatrix/rows/row/columns", datanumber, metadatatabletype));
            int cols = temp.ChildNodes.Count;

            //XmlNode metadata = doc.SelectSingleNode(string.Format("/Root/UATableList/Metadata/crosstab/UADoubleMatrix/rows", datanumber));

            //if (metadata != null)
            //{
            //    rows = metadata.ChildNodes.Count;
            //}

            matrix = new string[rows, cols];
            int j = 0;
            int rownodecounter = 0;

            for (int i = 0; i < rows; ++i)
            {
                XmlNode node = rownode.ChildNodes[rownodecounter];
                //if (metadata != null)
                //{
                //    XmlNode tempm = metadata.ChildNodes[i].SelectSingleNode("./columns/column[2]");
                //    if (tempm.InnerText == "0")
                //    {
                //        continue;
                //    }
                //}

                j = 0;
                foreach (XmlNode cnode in node.FirstChild.ChildNodes)
                {
                    matrix[i, j] = cnode.InnerText;
                    j++;
                }
                rownodecounter++;
            }
            return matrix;
        }

        //Returns a string telling the type of metadata///  like: crosstab / normal /( normal for chisq)
        public static string findMetaDataType(int datanumber)
        {
            XmlDocument doc = AnalyticsData.Result.Data;
            XmlNode normal = doc.SelectSingleNode(string.Format("/Root/UATableList/Metadata[@tablenumber={0}]/normal", datanumber));
            XmlNode crosstab = doc.SelectSingleNode(string.Format("/Root/UATableList/Metadata[@tablenumber={0}]/crosstab1", datanumber));
            XmlNode crosstab2 = doc.SelectSingleNode(string.Format("/Root/UATableList/Metadata[@tablenumber={0}]/crosstab2", datanumber));

            //For Chi-Sq table. And to get metadata (errors warnings about McNemar and Fisher)
            XmlNode normal1 = doc.SelectSingleNode(string.Format("/Root/UATableList/Metadata[@tablenumber={0}]/normal1", datanumber));
            XmlNode normal2 = doc.SelectSingleNode(string.Format("/Root/UATableList/Metadata[@tablenumber={0}]/normal2", datanumber));

            if (normal != null)
                return "normal";
            else if (crosstab != null)
                return "crosstab1";
            else if (crosstab2 != null)
                return "crosstab2";
            else if (normal1 != null)
                return "normal1";
            else if (normal2 != null)
                return "normal2";
            return null;
        }

        public static string[,] GetBSkyErrorsWarning(int datanumber, string metadatatabletype) //25Jun2013 for errors and warning outside analytic return results.
        {
            XmlDocument doc = AnalyticsData.Result.Data;

            if (doc == null) return null;

            string[,] matrix = null;
            XmlNode metadata = null;
            XmlNode errwarncount = doc.SelectSingleNode("/Root/UATableList/BSkyErrorWarn");

            if (errwarncount == null)
                return null;
            if (datanumber == 0) //25jun2013 if we do not know what is the table number for BAkyErrorWarning.
            {
                string tno = doc.SelectSingleNode("/Root/UATableList/BSkyErrorWarn/@tablenumber").Value;

                if (tno != null)
                    datanumber = Int16.Parse(tno);

                //for (int tcount = 1; tcount <= TotalOutputTables; tcount++)
                //{
                metadata = doc.SelectSingleNode(string.Format("/Root/UATableList/BSkyErrorWarn[@tablenumber={0}]", datanumber));//tcount
                //    if (metadata == null)
                //        continue; // if BSkyErrorwarning table not found in DOM
                //    else
                //    {
                //        datanumber = tcount;
                //        break;// if BSkyErrorwarning table found in DOM
                //    }
                //}

            }
            else
            {
                metadata = doc.SelectSingleNode(string.Format("/Root/UATableList/BSkyErrorWarn[@tablenumber={0}]", datanumber)); //before 25jun2013 this was the only line there was no 'if' above
            }

            // Table not found in DOM 
            if (metadata == null || metadatatabletype == null)
            {
                return matrix;
            }
            else// Table found in DOM and datanumber is already set
            {
                int rows = 0;
                XmlNode temp = doc.SelectSingleNode(string.Format("/Root/UATableList/BSkyErrorWarn[@tablenumber={0}]/{1}/UAList", datanumber, metadatatabletype));

                if (temp == null)
                    return matrix;

                int cols = temp.ChildNodes.Count;

                for (int i = 0; i < cols; i++)
                {
                    if (rows < temp.ChildNodes.Item(i).ChildNodes.Count)
                        rows = temp.ChildNodes.Item(i).ChildNodes.Count;
                }

                //use commented code to get all the info, instead of just getting RMsg and User Msg
                matrix = new string[rows, 3];//matrix = new string[rows, cols];

                int k = 0;

                for (int i = 0; i < cols; ++i)
                {
                    if ((i == 1) || (i == 6) || (i == 7))
                    {
                        for (int j = 0; j < temp.ChildNodes.Item(i).ChildNodes.Count; ++j)
                        {
                            //XmlNode tempm = metadata.ChildNodes[i].SelectSingleNode(string.Format("./normal/UAString[{0}]", j + 1));////fix this 02Mar2012
                            string s = temp.ChildNodes.Item(i).ChildNodes.Item(j).InnerText;

                            if (s != null && !s.Trim().Equals("."))
                            {
                                if (i == 1)
                                {
                                    switch (s)
                                    {
                                        case "-1":
                                            s = "Error:";
                                            break;
                                        case "-2":
                                            s = "Critical Error:";
                                            break;
                                        case "1":
                                            s = "Warning:";
                                            break;
                                        default:
                                            s = "";
                                            break;
                                    }
                                }
                                else if (i == 3)//row
                                { s = "Row " + s; }
                                else if (i == 4)//start
                                { s = "From col " + s; }
                                else if (i == 5)//ends
                                { s = "To col " + s; }

                                if (i == 6) s = "User Msg: " + s;

                                //use commented code to get all the info, instead of just getting RMsg and User Msg
                                if ((i == 1) || (i == 6) || (i == 7)) matrix[j, k] = s;//matrix[j, i] = s;
                            }
                            else
                            {
                                //use commented code to get all the info, instead of just getting RMsg and User Msg
                                if ((i == 1) || (i == 6) || (i == 7)) matrix[j, k] = "";//matrix[j, i] = "";//space char
                            }
                        }//row for
                        k++;
                    }//if
                }//col for
            }
            return matrix;
        }

        public static int GetStatsTablesCount() //23Aug2013 Stat result table count
        {
            int count = 0;
            XmlDocument doc = AnalyticsData.Result.Data;

            if (doc == null) return 0;
            //29Apr2014 will only count same type, here UADoubleMatrix. But we may have multiple types in UATableList
            //29Apr2014 XmlNode usrrescount = doc.SelectSingleNode("/Root/UATableList/UADoubleMatrix");
            XmlNodeList usrrescount = doc.SelectSingleNode("/Root/UATableList").ChildNodes;//29Apr2014

            if (usrrescount == null)
                return 0;
            //29Apr2014 count = doc.SelectNodes("/Root/UATableList/UADoubleMatrix").Count;
            count = doc.SelectSingleNode("/Root/UATableList").ChildNodes.Count;//29Apr2014
            return count;
        }

        public static int GetUserTablesCount() // User's result table count
        {
            int count = 0;
            XmlDocument doc = AnalyticsData.Result.Data;

            if (doc == null)
                return 0;

            XmlNode usrrescount = doc.SelectSingleNode("/Root/UATableList/UserResult/UserData");

            if (usrrescount == null)
                return 0;

            count = doc.SelectNodes("/Root/UATableList/UserResult/UserData").Count;
            return count;
        }

        //01May2014 Get table header from DOM if any
        public static string GetBSkyStatTableHeader(int datanumber)
        {
            string tabletitle = string.Empty;
            XmlDocument doc = AnalyticsData.Result.Data;
            XmlNode tablehead = null;//doc.SelectSingleNode(string.Format("/Root/UATableList/UADoubleMatrix"), datanumber);
            ////29Apr2014 works if all tags are same type UADoubleMatrix for multi tables
            //29Apr2014 XmlNodeList xnl = doc.SelectNodes("/Root/UATableList/UADoubleMatrix");
            XmlNodeList xnl = doc.SelectSingleNode("/Root/UATableList").ChildNodes;//29Apr2014

            if (xnl != null && xnl.Count > 0)
                tablehead = xnl[datanumber - 1];// datanumber =1 that mean I need find index 0
            if (tablehead != null)
            {
                if (tablehead.SelectSingleNode("tableheader") != null)
                    tabletitle = tablehead.SelectSingleNode("tableheader").InnerText.Replace("&gt;", ">").Replace("&lt;", "<").Replace("&ge;", ">=").Replace("&le;", "<=");
            }
            return tabletitle;
        }

        //23Aug2013 For stat results. like row col header if needed add one more param for table title.
        public static object GetBSkyStatResults(int datanumber, out string restype, out string[] colHeaders, out string[] rowHeaders, out string slicename)
        {
            restype = "";
            colHeaders = null; // It is difficult to set headers .As headers are not stored in DOM. They are in template XML
            rowHeaders = null; // And we are assuming that this code will run when XML is absent. Well if you need headers then that must be stored somewhere in stat result table ($datatable)
            slicename = string.Empty;

            bool[] visibleRows; // not using it yet
            /// Find col headers row headers and title(if exits)
            XmlDocument doc = AnalyticsData.Result.Data;
            XmlNode rowcolhead = null;//doc.SelectSingleNode(string.Format("/Root/UATableList/UADoubleMatrix"), datanumber);

            ////29Apr2014 works if all tags are same type UADoubleMatrix for multi tables
            //29Apr2014 XmlNodeList xnl = doc.SelectNodes("/Root/UATableList/UADoubleMatrix");
            XmlNodeList xnl = doc.SelectSingleNode("/Root/UATableList").ChildNodes;//29Apr2014

            if (xnl != null && xnl.Count > 0)
                rowcolhead = xnl[datanumber - 1];// datanumber =1 that mean I need find index 0
            if (rowcolhead != null)
            {
                if (rowcolhead.SelectSingleNode("colheaders") != null)
                    colHeaders = rowcolhead.SelectSingleNode("colheaders").InnerText.Replace("&gt;", ">").Replace("&lt;", "<").Replace("&ge;", ">=").Replace("&le;", "<=").Split(','); //comma separated string to Array
                if (rowcolhead.SelectSingleNode("rowheaders") != null)
                    rowHeaders = rowcolhead.SelectSingleNode("rowheaders").InnerText.Replace("&gt;", ">").Replace("&lt;", "<").Replace("&ge;", ">=").Replace("&le;", "<=").Split(','); //comma separated string to Array
                if (rowcolhead.SelectSingleNode("slicename") != null)
                    slicename = rowcolhead.SelectSingleNode("slicename").InnerText.Replace("&gt;", ">").Replace("&lt;", "<").Replace("&ge;", ">=").Replace("&le;", "<=");
            }

            //01Feb2016 substitute long slice name with datasetname in row/col header
            ChangeSlinameToDatasetNameInHeader(ref colHeaders);
            ChangeSlinameToDatasetNameInHeader(ref rowHeaders);

            slicename = SetSliceComponentOrder(slicename);
            // Table title if exists /// no code written for this yet

            string[,] BSkyStatData = GetDataMatrix(datanumber, out visibleRows);// table data
            return BSkyStatData;
        }

        //01Feb2016 Replace long slice name with current dataset name in flexgrid table headers
        private static void ChangeSlinameToDatasetNameInHeader(ref string[] header)
        {
            if (header == null)
                return;

            string currentDataset = ExpandMacro("%DATASET%");

            for (int i = 0; i < header.Length; i++)
            {
                if (header[i].Contains("bskyCurrentDatasetSplitSliceObj"))//For changing slice object name with current dataset name
                {
                    header[i] = header[i].Replace("bskyCurrentDatasetSplitSliceObj", currentDataset);  //currentDataset;
                }
            }
        }

        //04Sep2013 this function is only meant to be called from GetBSkyStatResults
        public static string SetSliceComponentOrder(string slicename)
        {
            //string orderedslicename = "Split Info : ";
            //string[] parts = slicename.Split(',');
            int indexoffirstcomma = slicename.IndexOf(',');
            int indexofseccomma = slicename.IndexOf(',', indexoffirstcomma + 1);

            if (indexoffirstcomma < 1 || indexofseccomma < 1) // if indexes of commas are not found because of missing commas. That means Split = N
                return string.Empty;

            string itrcount = slicename.Substring(indexoffirstcomma + 1, indexofseccomma - indexoffirstcomma - 1);//parts[1];
            string spliton = slicename.Substring(indexofseccomma + 1);

            return "Split Info : " + spliton.Trim() + ". " + itrcount.Trim() + ".";
        }

        public static object GetBSkyResults(int datanumber, out string restype, out string[] colHeaders, out string[] rowHeaders) //25Jun2013 for reslts those are at the end of return structure. May contain string, tables, array.
        {
            XmlDocument doc = AnalyticsData.Result.Data;
            colHeaders = null;
            rowHeaders = null;
            XmlNode metadata = null;
            XmlNode nextlvl = null;
            restype = "unknown";
            if (datanumber < 1)
                return null;
            metadata = doc.SelectSingleNode(string.Format("/Root/UATableList/UserResult/UserData[@tablenumber={0}]", datanumber)); //before 25jun2013 this was the only line there was no 'if' above
            if (metadata.HasChildNodes)
            {
                nextlvl = metadata.FirstChild;
            }
            // Table not found in DOM 
            if (metadata == null)
            {
                return null;
            }
            else if (nextlvl.Name == "UAString") //UAString
            {
                restype = "string";
                if (nextlvl.InnerText == null)
                    return null;
                return nextlvl.InnerText;
            }
            else if (nextlvl.Name == "UAStringList") //string array
            {
                restype = "stringlist";
                if (nextlvl.InnerText == null)
                    return null;
                string[] slist = new string[nextlvl.ChildNodes.Count];
                int ind = 0;

                foreach (XmlNode n in nextlvl.ChildNodes)
                {
                    slist[ind++] = n.InnerText;
                }
                return slist;
            }
            //else if (nextlvl.Name == "UADoubleMatrix") //double matrix .... matrix
            //{
            //    restype = "matrix";

            //    return getMatrix(nextlvl.SelectSingleNode("rows"));
            //}
            //trying to deprecate UADataFrame.
            else if (nextlvl.Name == "UADataFrame" || nextlvl.Name == "UADoubleMatrix") // .... data.frame and matrix
            {
                restype = (nextlvl.Name == "UADataFrame") ? "dataframe" : "matrix";
                colHeaders = nextlvl.SelectSingleNode("colheaders").InnerText.Split(','); //comma separated string to Array
                rowHeaders = nextlvl.SelectSingleNode("rowheaders").InnerText.Split(','); //comma separated string to Array
                if (colHeaders != null && colHeaders.Length == 1 && colHeaders[0].Trim().Length == 0)
                    colHeaders = null;
                if (rowHeaders != null && rowHeaders.Length == 1 && rowHeaders[0].Trim().Length == 0)
                    rowHeaders = null;
                return getMatrix(nextlvl.SelectSingleNode("rows"));
            }
            else if (nextlvl.Name == "UAList" || nextlvl.Name == "UAIntList")// UAList  .... 
            {
                restype = "matrix";
                colHeaders = nextlvl.SelectSingleNode("colheaders").InnerText.Split(','); //comma separated string to Array
                rowHeaders = nextlvl.SelectSingleNode("rowheaders").InnerText.Split(','); //comma separated string to Array
                if (colHeaders != null && colHeaders.Length == 1 && colHeaders[0].Trim().Length == 0)
                    colHeaders = null;
                if (rowHeaders != null && rowHeaders.Length == 1 && rowHeaders[0].Trim().Length == 0)
                    rowHeaders = null;
                return getListToMatrix(nextlvl.SelectSingleNode("rows"));
            }
            return null;
        }

        // converts same sized two or more lists to matrix /// data.frame
        public static string[,] getListToMatrix(XmlNode grandparentfromleaf) //grand parent from leaf node is to be passed
        {
            int rows = 0;
            string[,] matrix = null;
            int cols = grandparentfromleaf.ChildNodes.Count;

            if (cols == 0)
                return matrix;
            for (int i = 0; i < cols; i++)
            {
                if (rows < grandparentfromleaf.ChildNodes.Item(i).ChildNodes.Count)
                    rows = grandparentfromleaf.ChildNodes.Item(i).ChildNodes.Count;
            }

            matrix = new string[rows, cols];
            for (int i = 0; i < cols; ++i)
            {
                for (int j = 0; j < grandparentfromleaf.ChildNodes.Item(i).ChildNodes.Count; ++j)
                {
                    string s = grandparentfromleaf.ChildNodes.Item(i).ChildNodes.Item(j).InnerText;

                    if (s != null && !s.Trim().Equals("."))
                    {
                        matrix[j, i] = s;
                    }
                    else
                    {
                        matrix[j, i] = "";//space char
                    }
                }
            }
            return matrix;
        }

        // creates matrix out of row/col DOM structure
        public static string[,] getMatrix(XmlNode xmlrows) // rows contains multi row. each row contains multi cols
        {
            string[,] matrix = null;
            int rows = xmlrows.ChildNodes.Count;
            int cols = xmlrows.FirstChild.FirstChild.ChildNodes.Count; // rows > row > columns > col1..col3...colN

            matrix = new string[rows, cols];
            for (int i = 0; i < rows; ++i)
            {
                XmlNode row = xmlrows.ChildNodes[i];

                for (int j = 0; j < cols; ++j)
                {
                    string s = row.FirstChild.ChildNodes.Item(j).InnerText;

                    if (s != null && !s.Trim().Equals("."))
                    {
                        matrix[i, j] = s;
                    }
                    else
                    {
                        matrix[i, j] = "";//space char
                    }
                }
            }
            return matrix;
        }

        public static void Reset()
        {
            if (MacroList == null)
                MacroList = new Dictionary<string, string>();
            else
                MacroList.Clear();
        }

        public static AnalyticsData AnalyticsData
        {
            get;
            set;
        }

        public static event EventHandler FactorsNeeded;

        public static List<string> GetList(string listname, string varname, bool factors)
        {
            if (factors)
            {
                string strvar = OutputHelper.ExpandMacro(varname);
                return OutputHelper.GetFactors(strvar);
            }

            if (AnalyticsData == null || AnalyticsData.InputElement == null)
            {
                return null;
            }
            object obj = AnalyticsData.InputElement.FindName(listname);

            if (obj == null)
            {
                string[] parts = listname.Split('.');

                if (parts[0] == "GLOBAL")
                {
                    FrameworkElement fe = GetGlobalMacro(parts[0] + "." + parts[1] + "." + parts[2], string.Empty) as FrameworkElement;
                    obj = fe.FindName(parts[3]);
                }
            }
            if (obj != null)
            {
                List<string> temp = new List<string>();

                if (typeof(ListBox).IsAssignableFrom(obj.GetType()))//if its ListBox. Old code moved from above 'if'
                {
                    ListBox list = obj as ListBox;

                    if (list != null)
                    {
                        temp.Clear();
                        foreach (object o in list.Items)
                        {
                            temp.Add(o.ToString());
                        }
                    }
                }
                if (typeof(TextBox).IsAssignableFrom(obj.GetType()))//05Mar2013 if its TextBox. New code
                {
                    TextBox tb = obj as TextBox;

                    if (tb != null)
                    {
                        temp.Clear();
                        temp.Add(tb.Text);
                    }
                }
                return temp;
            }
            if (obj == null)//for error messages. 29Nov2012
            {
                //ILoggerService logService = LifetimeService.Instance.Container.Resolve<ILoggerService>();
                //logService.Error("Unhandled Exception Occured :", e.ExceptionObject as Exception);
            }
            return null;
        }

        public static List<string> GetFactors(string variablename)
        {
            //26Apr2016 Crosstab fix for variable name having special chars eg. filter_.
            // var.Name is changed to var.RName
            var variable = from var in AnalyticsData.DataSource.Variables
                           where var.RName == variablename
                           select var;
            DataSourceVariable dv = variable.FirstOrDefault();

            if (dv != null)
            {
                List<string> temp = new List<string>();
                temp.AddRange(dv.Values);
                temp.Remove("<NA>");// (".");//remove NAs. Anil 29Feb2012
                return temp;
            }
            else
            {
                List<string> temp = new List<string>();
                temp.Add(variablename + "X");
                temp.Add(variablename + "Y");
                return temp;
            }
        }

        public static bool Evaluate(string conditon)
        {
            if (OutputHelper.ExpandMacro(conditon) != conditon)
            {
                bool isTrue = false;

                if (bool.TryParse(OutputHelper.ExpandMacro(conditon), out isTrue))
                {
                    return isTrue;
                }
                else
                    return false;
            }
            if (AnalyticsData == null || AnalyticsData.InputElement == null)
            {
                return true;
            }

            ///// Checking multiple &&  or multiplpe || condition ////
            string condtype = CheckMulticondition(conditon);

            if (condtype.Length > 0) // multi condition exists
            {
                return GetMultiConditionResult(conditon);
            }
            else // single condition
            {
                return GetUIElementResult(conditon);
            }
            //return true;
        }

        //Right now only && or || condition can be checked. Mix of && and || is not entertained.
        public static string CheckMulticondition(string condition)//18Sep2013
        {
            string conditiontype = string.Empty; // no multi condition

            if ((condition.Contains("AND") && !condition.Contains("OR")))
            {
                conditiontype = "&&";
            }
            else if ((condition.Contains("OR") && !condition.Contains("AND")))
            {
                conditiontype = "||";
            }
            else if ((condition.Contains("OR") && condition.Contains("AND")))
            {
                conditiontype = "&|";
            }
            return conditiontype;
        }

        private static bool GetMultiConditionResult(string mcondition)
        {
            // replace and also put space around so that exact match could easily be found while replcing UI names with true/false
            string tempstr = " " + mcondition.Replace("AND", " & ").Replace("OR", " | ") + " ";
            string[] operands; // queue of operands
            //string[] operators; // queue of operators
            char[] separators = { '&', '|' };
            operands = tempstr.Split(separators);
            bool UIresult;

            foreach (string opr in operands)
            {
                UIresult = GetUIElementResult(opr);
                // replacing each UI element names with true/false based on UIresult
                tempstr = tempstr.Replace(opr.Trim(), UIresult.ToString());
            }

            //adding spaces around operators
            tempstr = tempstr.Replace("&", " & ").Replace("|", " | "); // adding spaces again as ealier ones should already be gone.

            //now mcondition  should have expression containing true and false in place of UI element names
            // now remove extra spaces and  just have single space between all opernads and operators of the expression(mcondition)
            do
            {
                tempstr = tempstr.Replace("  ", " ");//replace multispace by single space
            } while (tempstr.Contains("  "));

            // now evaluate true & true to true. Basically replace in mcondition.
            //// & processor
            do
            {
                tempstr = tempstr.Replace("True & True", "True").Replace("True & False", "False").Replace("False & True", "False").Replace("False & False", "False");
            } while (tempstr.Contains('&'));
            //// | processor
            do
            {
                tempstr = tempstr.Replace("True | True", "True").Replace("True | False", "True").Replace("False | True", "True").Replace("False | False", "False");
            } while (tempstr.Contains('|'));

            if (tempstr.Trim().Equals("True"))
                return true;
            else
                return false;
        }

        public static bool GetUIElementResult(string condition)
        {
            object obj = AnalyticsData.InputElement.FindName(condition.Trim()); // trim important here

            if (obj != null && typeof(CheckBox).IsAssignableFrom(obj.GetType()))
            {
                CheckBox chkbox = obj as CheckBox;
                return chkbox.IsChecked.HasValue ? chkbox.IsChecked.Value : false;
            }

            else if (obj != null && typeof(RadioButton).IsAssignableFrom(obj.GetType()))
            {
                RadioButton txt = obj as RadioButton;
                return txt.IsChecked.HasValue ? txt.IsChecked.Value : false;
            }

            else if (obj != null && typeof(ListBox).IsAssignableFrom(obj.GetType()))
            {
                ListBox lb = obj as ListBox;
                return lb.Items.Count > 0 ? true : false;
            }
            else if (condition != null && condition.Trim().Length == 0) // for condition="".// think and modify if needed
            {
                return true;
            }

            return false; // no such element in dialog
        }
    }
}