using System;
using System.Collections.Generic;
using System.Xml;
using BSky.Lifetime.Interfaces;

namespace BSky.Lifetime
{
    public class RecentItems  // class is same as RecentDocs except the MenuItem Logic is removed.
    {
        ILoggerService logService = LifetimeService.Instance.Container.Resolve<ILoggerService>();//17Dec2012
        List<string> FilesList = new List<string>();

        #region XML recent list

        //MenuItem Recent;
        //public MenuItem RecentMI //Recent in File, will have list of recent files as sub menu items
        //{
        //    set
        //    {
        //        Recent = value;
        //        RefreshXMLItems();//as soon as filename is set. List will refresh
        //    }
        //}

        public List<string> RecentFileList
        {
            get { return FilesList; }
        }

        int MaxItems = 10;//default
        public int MaxRecentItems
        {
            get { return MaxItems; }
            set { MaxItems = value; }
        }

        string RecentListXMLFilename;

        /// <summary>
        /// XML structure must match:
        /// <recent> 
        /// <item>string</item>
        /// <item>string</item>
        /// </recent>
        /// </summary>
        public string XMLFilename
        {
            get { return RecentListXMLFilename; }
            set
            {
                RecentListXMLFilename = value;
                //RefreshXMLItems();//as soon as filename is set. List will refresh
                //check if fileexists in that location. Else create it.
                if (!System.IO.File.Exists(value))//17Jan2014
                {
                    string text = "<?xml version=\"1.0\" encoding=\"utf-8\"?> " +
                                  "<recent>" +
                                  "</recent>";
                    System.IO.File.WriteAllText(value, text);
                }
            }
        }

        //loads items from XML to menu
        public void RefreshXMLItems()
        {
            XmlDocument xd = new XmlDocument();
            try
            {
                xd.Load(XMLFilename);//root element must exist in xml file
            }
            catch (Exception ex)
            {
                logService.WriteToLogLevel("Error adding XML entry in " + XMLFilename, LogLevelEnum.Error);
                logService.WriteToLogLevel(ex.Message, LogLevelEnum.Error);
            }
            ///Read all string from XML and load in FileList ///
            FilesList.Clear();
            XmlNodeList xnlst = xd.GetElementsByTagName("item");
            foreach (XmlNode xn in xnlst)
            {
                FilesList.Add(xn.InnerText);
            }
            //RefereshRecentList();//generate UI menu items under 'Recent'
        }

        //Adds single item on top
        public void AddXMLItem(string item)
        {

            //check if duplicate//
            if (IsDuplicateInXml(item))
            {
                RemoveXMLItem(item);//remove duplicate item. later add it so that it will be (move) up on the list
            }
            //no duplicate. open XML for writing. Add to list //
            XmlDocument xd = new XmlDocument();
            try
            {
                xd.Load(XMLFilename);//root element must exist in xml file
            }
            catch (Exception ex)
            {
                logService.WriteToLogLevel("Error adding XML entry in " + XMLFilename, LogLevelEnum.Error);
                logService.WriteToLogLevel(ex.Message, LogLevelEnum.Error);
            }
            ///get root element
            XmlNode xn = xd.SelectSingleNode("//recent");
            //if (xn == null)//if there is no <recent> tag
            //{
            //    XmlNode rootNode = xd.CreateElement("recent");
            //    xd.AppendChild(rootNode);
            //}

            //check if MaxItems is reached. if yes then remove last item then add new
            XmlNodeList xnlst = xd.GetElementsByTagName("item");
            if (xnlst.Count == MaxItems)
            {
                xn.RemoveChild(xn.LastChild);
            }

            //Add new Node//
            XmlNode itemnode = xd.CreateElement("item");
            itemnode.InnerText = item;
            xn.InsertBefore(itemnode, xn.FirstChild);//insert on top
            xd.Save(XMLFilename);

            //Refesh
            RefreshXMLItems();

        }

        //Adds multiple items in a given sequence
        public void AddXMLItems(string[] items)
        {
            //no duplicate. open XML for writing. Add to list //
            XmlDocument xd = new XmlDocument();
            try
            {
                xd.Load(XMLFilename);//root element must exist in xml file
            }
            catch (Exception ex)
            {
                logService.WriteToLogLevel("Error adding XML entry in " + XMLFilename, LogLevelEnum.Error);
                logService.WriteToLogLevel(ex.Message, LogLevelEnum.Error);
            }
            ///get root element
            XmlNode xn = xd.SelectSingleNode("//recent");
            //if (xn == null)//if there is no <recent> tag
            //{
            //    XmlNode rootNode = xd.CreateElement("recent");
            //    xd.AppendChild(rootNode);
            //}

            //check if MaxItems is reached. if yes then remove last item then add new
            XmlNodeList xnlst = xd.GetElementsByTagName("item");
            if (xnlst.Count == MaxItems)
            {
                xn.RemoveChild(xn.LastChild);
            }

            XmlNode itemnode = null;
            foreach (string item in items)
            {
                //Add new Node//
                itemnode = xd.CreateElement("item");
                itemnode.InnerText = item;
                xn.AppendChild(itemnode); //append ( add to last )
            }
            xd.Save(XMLFilename);

            //Refesh
            RefreshXMLItems();

        }

        public void RemoveXMLItem(string item) // remove from XML and from menu
        {
            //no duplicate. open XML for writing. Add to list //
            XmlDocument xd = new XmlDocument();
            try
            {
                xd.Load(XMLFilename);//root element must exist in xml file
            }
            catch (Exception ex)
            {
                logService.WriteToLogLevel("Error adding XML entry in " + XMLFilename, LogLevelEnum.Error);
                logService.WriteToLogLevel(ex.Message, LogLevelEnum.Error);
            }
            ///get root element
            XmlNode xn = xd.SelectSingleNode("//recent");

            //check if MaxItems is reached. if yes then remove last item then add new
            XmlNodeList xnlst = xd.GetElementsByTagName("item");
            XmlNode removethis = null;
            foreach (XmlNode xnit in xnlst)
            {
                if (xnit.InnerText == item) // if item is found in XML
                {
                    removethis = xnit; // XmlNode_item
                }
            }
            if (removethis != null)
            {
                xn.RemoveChild(removethis);
            }
            xd.Save(XMLFilename);

            //Refesh
            RefreshXMLItems();
        }

        public void RemoveAllItems() //06Feb2014
        {
            List<string> templist = new List<string>();
            foreach (string item in FilesList) // create as temp item list.
            {
                templist.Add(item);
            }
            foreach (string item in templist)
            {
                RemoveXMLItem(item);
            }
        }
        
        private bool IsDuplicateInXml(string item)
        {
            return FilesList.Contains(item); // FilesList is the latest copy of what XML has.
        }
        
        #endregion

        #region Creating UI menu items
        //private void RefereshRecentList()//creates actual items in "Recent..."
        //{
        //    ///creating menu items for each recent list string //
        //    MenuItem recentfile;
        //    Recent.Items.Clear();//clear old entries
        //    foreach (string s in this.RecentFileList)
        //    {
        //        recentfile = new MenuItem();
        //        recentfile.Header = s;
        //        recentfile.Click += new RoutedEventHandler(recentfile_Click);
        //        Recent.Items.Add(recentfile); //add new entries
        //    }
        //}

        //void recentfile_Click(object sender, RoutedEventArgs e)
        //{
        //    MenuItem clickeditem = sender as MenuItem;
        //    // MessageBox.Show("Opening..." + clickeditem.Header);
        //    FileOpenCommand fopen = new BlueSky.Commands.File.FileOpenCommand();
        //    fopen.FileOpen(clickeditem.Header.ToString());
        //}

        #endregion

    }
}
