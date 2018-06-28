using System.Collections.Generic;
using System.Xml;

namespace BSky.XmlDecoder
{
    public class ComplexText : Base
    {
        private string text = string.Empty;
        public string Text 
        { 
            get
            {
                if (!string.IsNullOrEmpty(text))
                {
                    return text;
                }
                else
                {
                    List<string> paramlist = new List<string>();
                    foreach (TextItem item in itemlist)
                    {
                        switch (item.Type)
                        {
                            case 0:
                                paramlist.Add(item.value);
                                break;
                            case 1:
                                paramlist.Add(OutputHelper.ExpandMacro(item.value));
                                break;
                            case 2:
                                string val = OutputHelper.ExpandParam(item.value, item.param);
                                paramlist.Add(val);
                                break;
                        }
                    }
                    return string.Format(StringFormat, paramlist.ToArray());
                }

            }
            set
            {
                text = value;
            }
        }
        
        struct TextItem
        {
            public int Type {get;set;}
            public string value{get;set;}
            public string param{get;set;}
        }

        private string StringFormat{get;set;}
        private List<TextItem> itemlist = null;
        public override void Initialize(XmlNode input)
        {
            
            if (input.Attributes[NodeNames.TEXT] != null)
            {
                text = input.Attributes[NodeNames.TEXT].Value;
            }
            else
            {
                XmlNode textnode = input.SelectSingleNode(NodeNames.TEXT);
                
                if (textnode == null || textnode.Attributes[NodeNames.STRING_FORMAT] == null)
                {
                    text = "NOSTRINGFORMAT";
                    return;
                }
                itemlist = new List<TextItem>();
                StringFormat = textnode.Attributes[NodeNames.STRING_FORMAT].Value;
                List<string> paramlist = new List<string>();
                foreach (XmlNode node in textnode.ChildNodes)
                {
                    switch(node.Name)
                    {
                        case NodeNames.STATIC_TEXT:
                            itemlist.Add(new TextItem() { Type = 0, value = node.InnerText, param = string.Empty });
                            break;
                        case NodeNames.MACRO:
                            string macroname = node.InnerText;
                            itemlist.Add(new TextItem() { Type = 1, value = macroname, param = string.Empty });
                            break;
                        case NodeNames.PARAM:
                            string datamember = node.Attributes[NodeNames.DATA_MEMBER] == null ? string.Empty : node.Attributes[NodeNames.DATA_MEMBER].Value;
                            string delimiter= node.Attributes[NodeNames.DELIMITER] == null ? string.Empty : node.Attributes[NodeNames.DELIMITER].Value;
       List<string> lst = OutputHelper.GetList(datamember, string.Empty, false);//13Sep2013. For layer NA 3rd part of heading is made blank
       int ittype = 2;
       if (lst.Count < 1)
       {
           ittype = 0;
           datamember = "";
           delimiter = "";
           //here, you can write some logic to remove last delimeter from StringFormat
       }

                            
                            itemlist.Add(new TextItem() { Type = ittype, value = datamember, param = delimiter });
                            break;

                    }
                }
                ///// Add some logic here to fill the table header area 22Feb2012
                ///// with error/warning messages from metadata in DOM
            }
        }
    }
}
