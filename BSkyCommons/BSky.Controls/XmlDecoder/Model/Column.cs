using System.Collections.Generic;
using System.Xml;

namespace BSky.XmlDecoder
{
    public class Column : Base
    {
        public int RowStart { get; set; }
        public int ColumnStart { get; set; }
        public Labels Labels { get; set; }
        public List<Column> SubColumns { get; set; }

        public int DepthLevel
        {
            get
            {
                int maxdepth = 0;
                foreach (Column col in SubColumns)
                {
                    if (maxdepth < col.DepthLevel)
                        maxdepth = col.DepthLevel;
                }
                return maxdepth + Labels.GetDepth();
            }
        }

        public int WidthLevel
        {
            get
            {
                int maxwide = 0;
                foreach (Column col in SubColumns)
                {
                    maxwide += col.WidthLevel;
                }

                if (maxwide == 0)
                {
                    return Labels.GetWidth();
                }
                else if (Labels.ShowGroupTotal)
                {
                    return (maxwide * (Labels.GetWidth() - 1)) + 1;
                }
                else
                {
                    return maxwide * Labels.GetWidth();
                }
            }
        }

        public override void Initialize(System.Xml.XmlNode input)
        {
            if (input == null)
                return;

            XmlNode node = input.SelectSingleNode(NodeNames.LABEL_LIST);

            this.Labels = new Labels();
            this.Labels.Initialize(node);

            XmlNode subcolumns = input.SelectSingleNode(NodeNames.SUBCOLUM_LIST);
            SubColumns = new List<Column>();
            if (subcolumns != null)
            {
                foreach (XmlNode sbnode in subcolumns.ChildNodes)
                {
                    if (sbnode.Name != NodeNames.COLUMN)
                        continue;
                    Column c = new Column();
                    c.Initialize(sbnode);
                    SubColumns.Add(c);
                }
            }
        }
    }
}
