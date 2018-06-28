
namespace BSky.Controls.XmlDecoder.Model
{
    public class MetadataTableRow
    {
        //                  Str                                         Str     Str
        //varIndex, type, varName, dataTableRow, startCol, endCol,   BSkyMsg, RMsg,

        int varIndex;
        string infoType;
        string varName;
        int dataTableRow;
        int startCol;
        int endCol;
        string bSkyMsg;
        string rMsg;


        public int VarIndex
        {
            get { return varIndex; }
            set { varIndex = value; }
        }
        public string InfoType
        {
            get { return infoType; }
            set { infoType = value; }
        }
        public string VarName
        {
            get { return varName; }
            set { varName = value; }
        }
        public int DataTableRow
        {
            get { return dataTableRow; }
            set { dataTableRow = value; }
        }
        public int StartCol
        {
            get { return startCol; }
            set { startCol = value; }
        }
        public int EndCol
        {
            get { return endCol; }
            set { endCol = value; }
        }
        public string BSkyMsg
        {
            get { return bSkyMsg; }
            set { bSkyMsg = value; }
        }
        public string RMsg
        {
            get { return rMsg; }
            set { rMsg = value; }
        }



    }
}
