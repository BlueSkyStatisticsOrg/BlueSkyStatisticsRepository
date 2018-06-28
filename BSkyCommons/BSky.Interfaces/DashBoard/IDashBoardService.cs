using System;
using BSky.Interfaces.DashBoard;
using System.Collections.Generic;

namespace BSky.Interfaces
{
    public class DashBoardEventArgs : EventArgs
    {
        public DashBoardItem DashBoardItem {get;set;}
    }
    public interface IDashBoardService
    {
        void Configure();
        List<DashBoardItem> GetDashBoardItems();
        bool? SetElementLocaton(string val, string Title, string commandFile, bool forcePlace, string LocationAboveBelowSibling);
        event EventHandler<DashBoardEventArgs> AddDashBoardItem;
        string SelectLocation(ref string commandname, ref string LocationAboveBelowSibling);

        string XamlFile { get; set; } //06Mar2013 for menu editor(Dialog installer)
        string XmlFile { get; set; } //06Mar2013 for menu editor(Dialog installer)
    }
}
