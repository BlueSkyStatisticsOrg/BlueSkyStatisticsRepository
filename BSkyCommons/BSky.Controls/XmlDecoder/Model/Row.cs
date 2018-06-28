using System.Collections.Generic;
using System.Xml;

namespace BSky.XmlDecoder
{
    public class Row : Base
    {
        public int RowStart { get; set; }
        public int ColumnStart { get; set; }
        public Labels Labels { get; set; }
        public List<Row> SubRows { get; set; }

        public int DepthLevel
        {
            get
            {
               int maxdepth= 0;
               if (SubRows != null && SubRows.Count > 0)//19Sep2013 for crosstab "Count"
               foreach (Row row in SubRows)
               {
                   if (row.Labels==null || row.Labels.LableList==null || row.Labels.LableList.Count == 0)//19Sep2013 for crosstab "Count"
                       continue;
                   if (maxdepth < row.DepthLevel)
                       maxdepth = row.DepthLevel;
               }
               return maxdepth + Labels.GetDepth();
            }
        }

        public int WidthLevel
        {
            get
            {
                int maxwide = 0;
                if (SubRows!=null && SubRows.Count > 0)//19Sep2013 for crosstab "Count"
                foreach (Row row in SubRows)
                {
                    maxwide += row.WidthLevel;
                }

                if (maxwide == 0)
                {
                    if (Labels != null) //19Sep2013 for crosstab "Count" if else added
                        return Labels.GetWidth();
                    else
                        return 0;
                }
                //else if(Labels.ShowGroupTotal)
                //{
                //    return (maxwide * (Labels.GetWidth()-1)) + 1;
                //}
                else
                {
                    if (Labels != null)//19Sep2013 for crosstab "Count" if else added
                        return maxwide * Labels.GetWidth();
                    else
                        return 0;
                }
            }
        }

        private void BloatRow(Labels label, List<Row> lastSubRows)
        {
            bool showheader = label.ShowHeader;
            bool showtotal = label.ShowGroupTotal;

            label.ShowGroupTotal = false;
            if (label.LableList.Count > 0) // say for crosstab this 'if' will create labels for first var of 'layers' list
            {
                List<string> factors = OutputHelper.GetFactors(label.LableList[0]);
                this.Labels = CreateOptionList(factors, showheader, showtotal);
                this.Labels.Factors = true;
                this.Labels.Varname = label.LableList[0];
            }
            Row parent = this;
            for(int i =1; i<label.LableList.Count;i++) // rest of the vars of 'layers' list, will be taken and lable will be created here
            {
                string str = label.LableList[i];
                List<string> factors = OutputHelper.GetFactors(str);
                if (factors.Count != 0)
                {
                    Row r = new Row();
                    r.Labels = CreateOptionList(factors, showheader, showtotal);
                    r.Labels.Varname = str;
                    r.Labels.Factors = true;
                    parent.SubRows = new List<Row>();
                    parent.SubRows.Add(r);
                    parent = r;
                }
            }
            parent.SubRows = lastSubRows;  // at last, the last layer variable will have 'row' list as subrows
        }

        private Labels CreateOptionList(List<string> lst, bool showheader,bool showtotal)
        {
            Labels lbls = new Labels();
            lbls.Initialize(lst, showheader, showtotal);
            return lbls;
        }

        public override void Initialize(System.Xml.XmlNode input)
        {

            XmlNode node = null;
            bool nested = false;
            Labels lbls = null;
            bool isCurrentRowNA = false;
            // 10sep2013 for empty list, as in, Crosstab empty 'layers' list. Earlier there was code without 'do-while'and new code
            //trying to find root row tag.
            do 
            {
                if (input == null)
                    return;

                node = input.SelectSingleNode(NodeNames.LABEL_LIST);
                if (node == null)
                    return;

                nested = input.Attributes[NodeNames.NESTED_ROW] != null && input.Attributes[NodeNames.NESTED_ROW].Value == "yes";

                lbls = new Labels();
                lbls.Initialize(node);

                /// new code to chk NA
                isCurrentRowNA = lbls.LableList.Count == 0 ? true : false;
                if (isCurrentRowNA)//move to sub level row making it current row
                {
                    XmlNode subrows = input.SelectSingleNode(NodeNames.SUBROW_LIST);
                    if (subrows != null)
                    {
                        input = subrows.SelectSingleNode(NodeNames.ROW);
                    }
                    else
                    {
                        return;
                    }
                }
            } while (isCurrentRowNA);
            //10Sep2013    lbls.Varname == 'NA'
            // I think, here we can test if 'layers' is empty( ie NA). If so we must drop top level row
            // and make next level row as top level row. Since 'layers' is empty, this means next level is going to be 
            // 'row' list vars. And BloatRow should not be called, I guess. As bloatrow is supposed to work on layers
            // vars making table fatter and fatter for each layer variable that is present in 'layers' list.

            //based on current logic 'layers' is the only one that makes table fatter by including all vars of its list in single table.
            //The other two 'row' and 'col' list creates separate table for each var in their list.
            //eg.. layers vars = l1, l2. row vars = r1, r2. col vars = c1,c2 then..
            // Table t1 should have l1,l2,r1, c1
            // Table t2 should have l1,l2,r1, c2
            // Table t3 should have l1,l2,r2, c1
            // Table t4 should have l1,l2,r2, c2
            //// This is actually current behaviour.Better confirm this from Aaron..

            XmlNode subRows = input.SelectSingleNode(NodeNames.SUBROW_LIST);
            List<Row> lastRows = new List<Row>();
            if (subRows != null)
            {
                foreach (XmlNode sbnode in subRows.ChildNodes)
                {
                    if (sbnode.Name != NodeNames.ROW)
                        continue;
                    Row c = new Row();
                    c.Initialize(sbnode);
                    if(c.Labels!=null)
                    lastRows.Add(c);
                }
            }

            if (nested)
            {
                if (lbls != null && lastRows != null) //19Spe2013 if added for crosstab "Count"
                BloatRow(lbls, lastRows);
                return;
            }
            else 
            {
                if (lbls != null && lastRows != null) //19Spe2013 if added for crosstab "Count"
                {
                    this.Labels = lbls;
                    this.SubRows = lastRows;
                }
            }

        }

        public void Initialize(bool nested,List<Row> subrows)
        {

            Labels lbls = new Labels();
            lbls.InitializeForList("list", true, false, true, string.Format("GLOBAL.{0}.SPLIT.SplitsVars",OutputHelper.AnalyticsData.DataSource.Name), string.Empty);

            List<Row> lastRows = subrows;
            
            if (nested)
            {
                BloatRow(lbls, lastRows);
                return;
            }
            else
            {
                this.Labels = lbls;
                this.SubRows = lastRows;
            }
        }
    }
}
