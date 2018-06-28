using System.Collections.Generic;
using System.Linq;

namespace BSky.Statistics.Common
{
    public class ValueLabelDialogMatrix
    {
        //Dictionary<string, ValLabelMarixRow> vldMatrix = new Dictionary<string, ValLabelMarixRow>();
        List<ValLabelMarixRow> vldMatrix = new List<ValLabelMarixRow>();

        public bool addLevel(string newLevel, int atIndex, bool isExisting)// add level
        {
            bool successfullyAdded = false;
            ValLabelMarixRow row = null;//create ref. for new row 
            if (isExisting)
            {
                row = new ValLabelMarixRow(newLevel, atIndex, newLevel, "");// create new row
                vldMatrix.Add(row);
                successfullyAdded = true;
            }
            else// its a new level
            {

                DuplicateInfo dupResult = getDuplicateCatagory(newLevel);
                switch (dupResult.duplicateType)
                {
                    case 1: //duplicate found in original list. check if Change prop is blank or contains value
                        // if blank(removed earlier), then simply add newLevel in Change prop.
                        // else create new entry in Change with current prop name and overwrite current Change value
                        ValLabelMarixRow curr = vldMatrix.ElementAt(dupResult.rowindex);
                        if (curr.ChangeDeleteLevel.Trim().Length == 0) // empty change value. That means user earlier dropped this level.
                        {
                            curr.ChangeDeleteLevel = newLevel;
                            successfullyAdded = true;
                        }
                        else if (curr.ChangeDeleteLevel.Trim() != newLevel) // user changed the name of this level earlier. We need to create new level with changed name and reset old level
                        {
                            string oldLevel = newLevel;// newLevel is old level actully in this case
                            newLevel = curr.ChangeDeleteLevel;
                            row = new ValLabelMarixRow(newLevel, atIndex, "", newLevel);// create new row
                            curr.ChangeDeleteLevel = oldLevel;
                            vldMatrix.Add(row);
                            successfullyAdded = true;
                        }
                        else
                        {
                            //its a duplicate 
                            successfullyAdded = false;
                        }
                        break;
                    case 2:
                        //its a duplicate
                        successfullyAdded = false;
                        break;
                    case 3:
                        //its a duplicate
                        successfullyAdded = false;
                        break;
                    default:
                        row = new ValLabelMarixRow(newLevel, atIndex, "", newLevel);// create new row
                        vldMatrix.Add(row);
                        successfullyAdded = true;
                        break;
                }
            }
            return successfullyAdded;
        }

        public bool delLevel(string level, int index) // remove level
        {
            bool delSuccess = false;
            foreach (ValLabelMarixRow lvl in vldMatrix)
            {
                if (lvl.OriginalLevel.Trim().Equals(level))//match found
                {
                    if (lvl.NewLevel.Trim().Equals(level))//it was new level
                    {
                        vldMatrix.Remove(lvl);
                        delSuccess = true;
                        break;
                    }
                    else // it is old level from original list. reset some properties. Dont remove original(old) levels.
                    {
                        lvl.NewLevel = "";
                        lvl.ListboxIndex = -1;
                        lvl.ChangeDeleteLevel = "";
                        delSuccess = true;
                    }
                }
            }
            return delSuccess;
        }

        public bool changeLevel(string oldlevel, string newlevel) // change level
        {
            bool changeSuccess = false;
            DuplicateInfo dupResult = getDuplicateCatagory(newlevel);
            switch (dupResult.duplicateType)
            {
                case 1:
                    break;
                case 2:
                    break;
                case 3:
                    break;
                default:
                    foreach (ValLabelMarixRow lvl in vldMatrix)
                    {
                        if (lvl.ChangeDeleteLevel.Trim().Equals(oldlevel))//match found
                        {
                            lvl.NewLevel = "";
                            //lvl.ListboxIndex = -1;
                            lvl.ChangeDeleteLevel = newlevel;
                            changeSuccess = true;
                        }
                    }
                    break;
            }

            return changeSuccess;
        }

        public DuplicateInfo getDuplicateCatagory(string level)
        {
            DuplicateInfo di = new DuplicateInfo();
            foreach (ValLabelMarixRow lvl in vldMatrix)
            {
                di.rowindex = vldMatrix.IndexOf(lvl);
                if (level.Equals(lvl.OriginalLevel))
                {
                    di.duplicateType = 1;//duplicate as in original list
                    break;
                }
                else if (level.Equals(lvl.ChangeDeleteLevel))
                {
                    di.duplicateType = 1;//duplicate as in Change/Del
                    break;
                }
                else if (level.Equals(lvl.NewLevel))
                {
                    di.duplicateType = 3;//duplicate as in new
                    break;
                }
                else
                {
                    di.rowindex = -1;
                    di.duplicateType = 0; //no duplicate
                }
            }
            return di; //no duplicate
        }

        public List<ValLvlListItem> getFinalList(List<string> vlst)//use values from listbox and create final list. Fix indexes basically.
        {

            List<ValLvlListItem> finalList = new List<ValLvlListItem>();
            ValLvlListItem itm = null;
            int i = 0;
            foreach (string lv in vlst)
            {
                foreach (ValLabelMarixRow lvl in vldMatrix)
                {
                    if (lvl.ChangeDeleteLevel.Trim().Equals(lv.Trim()) || lvl.NewLevel.Trim().Equals(lv.Trim()))//if level matches
                    {
                        lvl.ListboxIndex = i;
                        ///// creating List///
                        itm = new ValLvlListItem();
                        if (lvl.ChangeDeleteLevel.Trim().Length > 0)
                        {
                            itm.NewLevel = lvl.ChangeDeleteLevel.Trim();
                            itm.OriginalLevel = lvl.OriginalLevel;
                        }
                        else if (lvl.NewLevel.Trim().Length > 0)
                        {
                            itm.NewLevel = lvl.NewLevel.Trim();
                            itm.OriginalLevel = "";// keep it empty, for R. c("Chngd Level", "Exstng Lvl", "") #last one is new lvl
                        }
                        finalList.Add(itm);

                        break;
                    }

                }
                i++;
            }

            //creating a final list in order they exists.
            //foreach (ValLabelMarixRow lvl in vldMatrix)
            //{
            //    if ((lvl.ChangeDeleteLevel.Trim().Length > 0) || (lvl.NewLevel.Trim().Length > 0))
            //    {
            //        itm = new ValLvlListItem();
            //        if(lvl.ChangeDeleteLevel.Trim().Length>0)
            //        {
            //            itm.NewLevel = lvl.ChangeDeleteLevel.Trim();
            //            itm.OriginalLevel = lvl.OriginalLevel;
            //        }
            //        else if(lvl.NewLevel.Trim().Length >0)
            //        {
            //            itm.NewLevel = lvl.NewLevel.Trim();
            //            itm.OriginalLevel = "";// keep it empty, for R. c("Chngd Level", "Exstng Lvl", "") #last one is new lvl
            //        }

            //        //finalList.Insert(lvl.ListboxIndex, itm);
            //        finalList.Add(itm);
            //    }
            //}

            return finalList;
        }
    }

    public class ValLabelMarixRow
    {
        string _OriginalLevel;
        public string OriginalLevel
        {
            get { return _OriginalLevel; }
            set { _OriginalLevel = value; }
        }

        int _ListboxIndex;
        public int ListboxIndex
        {
            get { return _ListboxIndex; }
            set { _ListboxIndex = value; }
        }

        string _ChangeDeleteLevel;
        public string ChangeDeleteLevel
        {
            get { return _ChangeDeleteLevel; }
            set { _ChangeDeleteLevel = value; }
        }

        string _NewLevel;
        public string NewLevel
        {
            get { return _NewLevel; }
            set { _NewLevel = value; }
        }



        public ValLabelMarixRow()
        {
            OriginalLevel = "";
            ListboxIndex = -1; // -1 means not in listbox
            ChangeDeleteLevel = "";
            NewLevel = "";
        }

        public ValLabelMarixRow(string org, int index, string chndel, string add)
        {
            OriginalLevel = org;
            ListboxIndex = index; // -1 means not in listbox
            ChangeDeleteLevel = chndel;
            NewLevel = add;
        }
    }

    public class DuplicateInfo
    {
        int _duplicateType;//1,2,3,0 for (Orig, Change, New, NO Dup)
        public int duplicateType
        {
            get { return _duplicateType; }
            set { _duplicateType = value; }
        }

        int _rowindex; //index where duplicate exists.
        public int rowindex
        {
            get { return _rowindex; }
            set { _rowindex = value; }
        }

        public DuplicateInfo()
        {
            duplicateType = 0;
            rowindex = -1;
        }
    }

    public class ValLvlListItem
    {
        string _OriginalLevel;
        public string OriginalLevel
        {
            get { return _OriginalLevel; }
            set { _OriginalLevel = value; }
        }
        string _NewLevels;
        public string NewLevel
        {
            get { return _NewLevels; }
            set { _NewLevels = value; }
        }
    }
}
