using System.Collections.Generic;
using System.Xml;
using System.Windows.Controls;

namespace BSky.XmlDecoder
{
    public class Output : Base
    {
        public ComplexText Header { get; set; }
        public Dictionary<string, Table> TableList { get; set; }
        public List<TableRepeat> Repeats { get; set; }
        public bool isGraphic{ get; set; }//05Sep2012
        public bool repeatall { get; set; }

        public Output()
        {
            Header = new ComplexText();
        }
        public override void Initialize(XmlNode input)
        {
            XmlNode header = input.SelectSingleNode(NodeNames.OUTPUT_HEADER);
            this.Header.Initialize(header);

            XmlNode tablelist = input.SelectSingleNode(NodeNames.TABLE_LIST);
            if (tablelist != null) //05Sep2012 for empty template XMLs. 'if' introduce for code below
            {
                this.TableList = new Dictionary<string, Table>();
                foreach (XmlNode node in tablelist.ChildNodes)
                {
                    if (node.Name != NodeNames.TABLE)
                        continue;

                    ///check here to see if Table is to be displayed or not //////
                    if (dispalyTable(node))
                    {
                        Table t = new Table();
                        t.Initialize(node);
                        this.TableList.Add(t.ID, t);
                    }
                }

                XmlNode repeats = input.SelectSingleNode(NodeNames.REPEAT_LIST);
                Repeats = new List<TableRepeat>();
                repeatall = true;
                if (repeats != null)
                {
                    //bool repeatall = true;
                    foreach (XmlNode node in repeats.ChildNodes)
                    {
                        if (node.Name != NodeNames.REPEATALL)//look for repeatall. if not then look for repeatselective
                        {
                            repeatall = false;
                            if (node.Name != NodeNames.REPEATSELECTIVE)
                            {
                                continue;
                            }
                        }
                        foreach (XmlNode rnode in node.ChildNodes)
                        {
                            //XmlNode rnode = node.SelectSingleNode(NodeNames.REPEAT);
                            if (rnode.Name != NodeNames.REPEAT)
                                continue;

                            TableRepeat repeat = new TableRepeat();
                            repeat.Initialize(rnode);
                            Repeats.Add(repeat);//for selective. Should have info about all repeat tags
                        }
                        if (repeatall)
                            break;//onlyone tag. Either repeatall of repeatselective. Fix logic so that if selective comes first then also it works.
                    }
                }
            }//if any table exists in output template

            XmlNode imagelist = input.SelectSingleNode(NodeNames.IMAGE_LIST);
            if (imagelist != null) //05Sep2012 
            { isGraphic = true; }
        }

        /// <summary>
        /// Adding following struct, List and displayTable() for optional tables
        /// </summary>
        struct OptionItem
        {
            public string condition { get; set; }
            public ComplexText Text { get; set; }
        }
        private List<OptionItem> optionList;
        public bool dispalyTable(System.Xml.XmlNode input)
        {
            bool flag = true;
            
            if (input == null)
                return flag;

            string grouping = input.Attributes[NodeNames.GROUPING] == null ? "list" : input.Attributes[NodeNames.GROUPING].Value;

            optionList = new List<OptionItem>();
            XmlNode optionnodeList = input.SelectSingleNode(NodeNames.OPTION_LIST);
            if (optionList == null || optionnodeList == null)
                return flag;

            foreach (XmlNode onode in optionnodeList.ChildNodes)
            {
                if (onode.Name != NodeNames.OPTION)
                    continue;
                //condition = "chkbox1 <conditional operator> chkbox2" ; where conditional operator can be OR/AND
                string condition = onode.Attributes[NodeNames.OPTION_CONDITION] == null ? string.Empty : onode.Attributes[NodeNames.OPTION_CONDITION].Value;

                //Write some logic to separate out each operand(ckhbox1, chkbox2) to get final result of the condition based on operator used.
                object obj = OutputHelper.AnalyticsData.InputElement.FindName(condition);

                if(flag && (obj != null && typeof(CheckBox).IsAssignableFrom(obj.GetType())))
                {
                    CheckBox chkbox = obj as CheckBox;
                    flag = chkbox.IsChecked.HasValue ? chkbox.IsChecked.Value : false;
                }                

            }


            return flag;
        }
   
    }

}
