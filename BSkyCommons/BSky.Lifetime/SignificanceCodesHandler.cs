using BSky.Lifetime.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace BSky.Lifetime
{
    public static class SignificanceCodesHandler
    {
        //public SignificanceCodesHandler()
        //{
        //    CreateSignifColList();
        //}
        

        private static List<ColSignifCodes> _sigCodeList;
        public static List<ColSignifCodes> SigCodeList
        {
            get { return _sigCodeList; }
        }


        //Read signif codes for each col from SignifCodes.xml
        private static void ReadXMLSignifCodes()
        {
            ILoggerService logService = LifetimeService.Instance.Container.Resolve<ILoggerService>();
            try
            {
                _sigCodeList = new List<ColSignifCodes>();//list of cols each having its signif codes

                XmlDocument xdoc = new XmlDocument();
                xdoc.Load(@"SignifCodes.xml");

                string sigfrom = string.Empty;
                string sigto = string.Empty;
                string sigstarchar = string.Empty;

                double dsigfrom = 0;
                double dsigto = 0;

                //Get all column names
                XmlNodeList allcols = xdoc.GetElementsByTagName("colsigcodes");
                foreach (XmlNode colnode in allcols)
                {
                    ColSignifCodes colsigcod = new ColSignifCodes();

                    //first child is colname 
                    string colname = colnode["colname"].InnerText;

                    colsigcod.ColName = colname;

                    //second child is sigcodes
                    XmlNodeList sigcodlst = colnode["sigcodes"].ChildNodes;// child should be colname and sigcodes
                    foreach (XmlNode sigcode in sigcodlst)
                    {
                        sigfrom = sigcode["from"].InnerText;
                        sigto = sigcode["to"].InnerText;
                        sigstarchar = sigcode["starchar"].InnerText;

                        SignifCode colcode = new SignifCode();

                        Double.TryParse(sigfrom, out dsigfrom);
                        Double.TryParse(sigto, out dsigto);

                        //populating modal
                        colcode.From = dsigfrom;
                        colcode.To = dsigto;
                        colcode.StarChars = sigstarchar;
                        colsigcod.SignifCodeList.Add(colcode);
                    }
                    _sigCodeList.Add(colsigcod);
                }
               //for testing if list is empty, what happens.  _sigCodeList.Clear();
            }
            catch (Exception ex)
            {
                logService.WriteToLogLevel("Error occurred reading SignifCodes.xml.", LogLevelEnum.Error);
                logService.WriteToLogLevel(ex.Message, LogLevelEnum.Error);
                _sigCodeList.Clear();//cleraring data so as to protect app from crashing because of incorrect strings/values
            }
        }
        
        //in following we can try to get list os col names and its signif codes from XML and poupulate sigcodlist
        //This can be executed at launch time, once
        public static void CreateSignifColList()
        {
            ReadXMLSignifCodes();//At BSky launch time reads xml and then creates signif codes for current session.

			//Use above one -OR- hardcoded one below
			
            //_sigCodeList = new List<ColSignifCodes>();//list of cols each having its signif codes

            //#region Creating for one col with colname as "Pr(>F)"
            //ColSignifCodes colsigcod = new ColSignifCodes();

            //colsigcod.ColName = "Pr(>F)";
            ////create few codes for the col above 'Pr(>F)'
            //SignifCode colcode = new SignifCode();

            //colcode.From = 0;
            //colcode.To = 0.001;
            //colcode.StarChars = "***";
            //colsigcod.SignifCodeList.Add(colcode);

            //colcode = new SignifCode();
            //colcode.From = 0.001;
            //colcode.To = 0.01;
            //colcode.StarChars = "**";
            //colsigcod.SignifCodeList.Add(colcode);

            //colcode = new SignifCode();
            //colcode.From = 0.01;
            //colcode.To = 0.05;
            //colcode.StarChars = "*";
            //colsigcod.SignifCodeList.Add(colcode);

            //colcode = new SignifCode();
            //colcode.From = 0.05;
            //colcode.To = 0.1;
            //colcode.StarChars = ".";
            //colsigcod.SignifCodeList.Add(colcode);

            //colcode = new SignifCode();
            //colcode.From = 0.1;
            //colcode.To = 1;
            //colcode.StarChars = " ";
            //colsigcod.SignifCodeList.Add(colcode);

            //_sigCodeList.Add(colsigcod);
            //#endregion
        }

        //To refresh the list at run time if user adds/removes/modifies the colname and its signif codes.
        //This can be executed just after resetting the values(may be in config settings or similar place)
        public static void RefreshSignifCodeList()
        {
            //no logic yet but
            //I think its just two step process. Not thinking deeply though.
            _sigCodeList.Clear();
            CreateSignifColList();
        }

        //Get all (current) signif col names
        public static List<string> GetAllSignifColNames()
        {
            List<string> SignifColNames = new List<string>();
            if (SigCodeList != null && SigCodeList.Count > 0)
            {
                foreach (ColSignifCodes sc in SigCodeList)
                {
                    if (!SignifColNames.Contains(sc.ColName))
                    {
                        SignifColNames.Add(sc.ColName);
                    }
                }
            }
            return SignifColNames;
        }
    }

    public class ColSignifCodes
    {
        string _colName;
        List<SignifCode> _signifCodeList = new List<SignifCode>();

        public string ColName
        {
            get { return _colName; }
            set { _colName = value; }
        }

        public List<SignifCode> SignifCodeList
        {
            get { return _signifCodeList; }
            set { _signifCodeList = value; }
        }

        //based on the range within which celldata appears, return the star characters
        public string getStarChars(double celldata)
        {
            string starchar = string.Empty;
            foreach (SignifCode sc in SignifCodeList)
            {
                if (celldata >= sc.From && celldata < sc.To)
                {
                    starchar = sc.StarChars;
                }
            }
            return starchar;
        }

        //based on colname form a footer string
        // 0 *** .001 ** .01
        public string getFooterStarMessage()
        {
            StringBuilder footerStarMsg = new StringBuilder("Signif. codes: ");
            int totalCols = SignifCodeList.Count;
            SignifCode sc;
            for (int i = 0; i < totalCols; i++)// SignifCode sc in SignifCodeList)
            {
                sc = SignifCodeList[i];

                footerStarMsg.Append(sc.From);
                footerStarMsg.Append(" '");
                footerStarMsg.Append(sc.StarChars);
                footerStarMsg.Append("' ");

                if (i == (totalCols - 1))//last object
                {
                    footerStarMsg.Append(sc.To);
                }

            }
            return footerStarMsg.ToString();
        }
    }

    public class SignifCode
    {
        double _from;
        double _to;
        string _starChars;

        public double From
        {
            get
            {
                return _from;
            }

            set
            {
                _from = value;
            }
        }

        public double To
        {
            get
            {
                return _to;
            }

            set
            {
                _to = value;
            }
        }

        public string StarChars
        {
            get
            {
                return _starChars;
            }

            set
            {
                _starChars = value;
            }
        }


    }
}
