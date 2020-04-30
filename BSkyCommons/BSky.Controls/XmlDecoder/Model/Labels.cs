using System;
using System.Collections.Generic;
using System.Xml;

namespace BSky.XmlDecoder
{
    public class Labels : Base
    {
        public Grouping Grouping { get; set; }
        public bool Factors { get; set; }

        public List<string> LableList
        {
            get
            {
                List<string> lst = null;
                switch (this.Grouping)
                {
                    case Grouping.list:
                        lst = OutputHelper.GetList(listname,varname, Factors);
                        break;
                    case Grouping.options:
                        lst = ExpandOptions(optionList); // crosstab "Count" etc..
                        break;
                }
                if (lst != null)
                {
                    if (lst.Count > 0 && ShowGroupTotal)
                        lst.Add("Total");
                    return lst;
                }
                return null;
            }
        }

        public int GetWidth()
        {
            if (this.Grouping != XmlDecoder.Grouping.nesting)
            {
                if (LableList != null) //19Sep2013 else if added for "Count"
                    return LableList.Count;
                else
                    return 0;
            }
            List<string> lst = OutputHelper.GetList(listname,varname,Factors);

            int depth = 1;

            foreach (string str in lst)
            {
                List<string> temp = OutputHelper.GetFactors(str);
                depth *= temp.Count;
            }
            return depth;
        }

        public int GetDepth()
        {
            List<string> lst = OutputHelper.GetList(listname,varname,Factors);

            int headerRow = this.ShowHeader ? 1 : 0; 
            if (this.Grouping != XmlDecoder.Grouping.nesting)
            {
                return 1 + headerRow;
            }
            return lst.Count + headerRow;
        }

        private List<string> ExpandOptions(List<OptionItem> options)
        {
            List<string> temp = new List<string>();
            foreach (OptionItem item in options)
            {
                if (OutputHelper.Evaluate(item.condition)) // Should also take multiple conditions here 18Mar2013
                    temp.Add(OutputHelper.ExpandMacro(item.Text.Text));
            }

            return temp;
        }
        private string listname = string.Empty;
        private string varname = string.Empty;

        public string Varname
        {
            get { return varname; }
            set { varname = value; }
        }

        struct OptionItem
        {
            public string condition { get; set; }
            public ComplexText Text { get; set; }
        }
        private List<OptionItem> optionList;

        public bool ShowGroupTotal { get; set; }

        public bool ShowHeader { get; set; }

        public void Initialize(List<string> lst, bool showheader, bool showTotal)
        {
            this.ShowGroupTotal = showTotal;
            this.ShowHeader = showheader;

            Grouping = XmlDecoder.Grouping.options;
            optionList = new List<OptionItem>();
            foreach (string str in lst)
            {
                OptionItem item = new OptionItem();
                item.condition = "";
                item.Text = new ComplexText();
                item.Text.Text = str;
                optionList.Add(item);
            }

        }
        public override void Initialize(System.Xml.XmlNode input)
        {
            if (input == null)
                return;

            string grouping = input.Attributes[NodeNames.GROUPING] == null ? "list" : input.Attributes[NodeNames.GROUPING].Value;
            ShowHeader = input.Attributes[NodeNames.SHOW_HEADER] != null && input.Attributes[NodeNames.SHOW_HEADER].Value == "yes";
            ShowGroupTotal = input.Attributes[NodeNames.SHOW_GROUP_TOTAL] != null && input.Attributes[NodeNames.SHOW_GROUP_TOTAL].Value == "yes";

            try
            {
                this.Grouping = (Grouping)Enum.Parse(typeof(Grouping), grouping);
            }
            catch
            {
                return;
            }
            switch (Grouping)
            {
                case Grouping.list:
                    XmlNode list = input.SelectSingleNode(NodeNames.LIST);
                    if(list == null)
                        return;
                    listname = list.Attributes[NodeNames.LIST_NAME] == null ? string.Empty : list.Attributes[NodeNames.LIST_NAME].Value;
                    varname = list.Attributes[NodeNames.VAR_NAME] == null ? string.Empty : list.Attributes[NodeNames.VAR_NAME].Value;
                    this.Factors = list.Attributes[NodeNames.VAR_NAME] == null || list.Attributes[NodeNames.VAR_NAME].Value == "no" ? false : true;
                    break;
                case Grouping.options:
                    optionList = new List<OptionItem>();
                    XmlNode optionnodeList = input.SelectSingleNode(NodeNames.OPTION_LIST);
                    if(optionList == null)
                        return;

                    foreach (XmlNode onode in optionnodeList.ChildNodes)
                    {
                        if (onode.Name != NodeNames.OPTION)
                            continue;
                        OptionItem item = new OptionItem();
                        item.condition = onode.Attributes[NodeNames.OPTION_CONDITION] == null ? string.Empty :  onode.Attributes[NodeNames.OPTION_CONDITION].Value;
                        item.Text = new ComplexText();
                        if (item.condition.Equals("gpbox2"))// || item.condition.Equals("test2") || item.condition.Equals("test3"))
                        {
                            string tagRes = OutputHelper.EvaluateRadioGrpBxTagValue(OutputHelper.AnalyticsData.InputElement, item.condition);
                            switch (tagRes)//values are also enclosed in single quotes (may be normal in dialog-procession)
                            {
                                case "'two.sided'":
                                    item.Text.Text = "Sig.(2-tail)";//Sig.(two-tailed)
                                    break;
                                case "'greater'":
                                    item.Text.Text = "Sig.(1-tail, >)";//Sig.(upper-tailed)
                                    break;
                                case "'less'":
                                    item.Text.Text = "Sig.(1-tail, <)"; //Sig.(lower-tailed)
                                    break;
                                default:
                                    break;
                            }
                            //it has also been found that if item.condition has value after this point then
                            // the column is completely getting skipped and does not appear in output table
                            // so we will forcefully make it empty.
                            //All we wanted to achieve using item.condition was the correct text and
                            //we do not have any other use for item.condition value.
                            item.condition = "";

                        }
                        else
                        {
                            item.Text.Initialize(onode);
                        }
                        optionList.Add(item);
                    }
                    break;
            }
        }

        public void InitializeForList(string grouping, bool showheader, bool showtotal, bool factors,string lst, string variablename)
        {
            
            ShowHeader = showheader;
            ShowGroupTotal = ShowGroupTotal;

            try
            {
                this.Grouping = (Grouping)Enum.Parse(typeof(Grouping), grouping);
            }
            catch
            {
                return;
            }
            switch (Grouping)
            {
                case Grouping.list:
                    listname = lst;
                    varname = variablename;
                    break;
            }
        }
    }

}
