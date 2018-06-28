using System.Collections.Generic;
using System.Xml;

namespace BSky.XmlDecoder
{
    public class TableRepeat : Base
    {
        public string Condition { get; set; }
        public string RepeatOn { get; set; }
        public string VariableName { get; set; }
        public string TableToRepeat { get; set; }
        public List<InnerRepeat> InnerRepeats { get; set; }

        public override void Initialize(XmlNode input)
        {
            Condition = input.Attributes[NodeNames.CONDITION].Value;
            RepeatOn = input.Attributes[NodeNames.REPEAT_ON].Value;
            VariableName = input.Attributes[NodeNames.VAR_NAME].Value;
            TableToRepeat = input.Attributes[NodeNames.TABLE_TO_REPEAT].Value;

            XmlNodeList forlist = input.SelectNodes(NodeNames.FOR_EACH);
            this.InnerRepeats = new List<InnerRepeat>();
            foreach (XmlNode node in forlist)
            {
                if (node.Name != NodeNames.FOR_EACH)
                    continue;
                InnerRepeat t = new InnerRepeat();
                t.Initialize(node);
                this.InnerRepeats.Add(t);
            }
        }
    }
}
