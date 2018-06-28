using System.Collections.Generic;
using System.Xml;

namespace BSky.XmlDecoder
{
    public class Table : Base
    {
        public string ID { get; set; }
        public ComplexText Header { get; set; }
        public List<FootNote> Errormessages { get; set; } // for error messages on top of each data table. AD 02Mar2012
        public List<Column> Columns { get; set; }
        public List<Row> Rows { get; set; }
        public List<FootNote> FootNotes { get; set; }

        public Table()
        {
            this.Header = new ComplexText();
            this.Errormessages = new List<FootNote>();//adding one more header for error and warning messages. AD 02Mar2012

            Columns = new List<Column>();
            Rows = new List<Row>();
            FootNotes = new List<FootNote>();
        }

        public override void Initialize(System.Xml.XmlNode input)
        {
            this.ID = input.Attributes[NodeNames.ID] == null ? string.Empty : input.Attributes[NodeNames.ID].Value;
            XmlNode headernode = input.SelectSingleNode(NodeNames.TABLE_HEADER);
            this.Header.Initialize(headernode);

            XmlNode columnlist = input.SelectSingleNode(NodeNames.COLUMN_LIST);

            foreach (XmlNode node in columnlist.ChildNodes)
            {
                if (node.Name != NodeNames.COLUMN)
                    continue;
                Column c = new Column();
                c.Initialize(node);
                Columns.Add(c);
            }

            XmlNode rowlist = input.SelectSingleNode(NodeNames.ROW_LIST);

            foreach (XmlNode node in rowlist.ChildNodes)
            {
                if (node.Name != NodeNames.ROW)
                    continue;
                Row r = new Row();
                r.Initialize(node);
                if (r.Labels != null)//19Sep2013 corsstab "Count"
                Rows.Add(r);
            }

            XmlNode footnotes = input.SelectSingleNode(NodeNames.FOOTNOTE_LIST);
            if (footnotes != null)
            {
                foreach (XmlNode node in footnotes.ChildNodes)
                {
                    if (node.Name != NodeNames.FOOTNOTE)
                        continue;
                    FootNote fn = new FootNote();
                    fn.Initialize(node);
                    FootNotes.Add(fn);
                }
            }
        }

    }
}
