using System;
using System.Collections.Generic;
using System.Xml;
using System.Windows.Controls;
using System.Windows;
using BSky.Lifetime.Interfaces;
using BSky.Lifetime;
using System.Runtime.CompilerServices;

namespace BSky.RecentFileHandler
{
    public class RecentDocs
    {
        ILoggerService logService = LifetimeService.Instance.Container.Resolve<ILoggerService>();//17Dec2012
        List<string> FilesList = new List<string>();

        #region XML recent list

        MenuItem Recent;
        public MenuItem RecentMI //Recent in File, will have list of recent files as sub menu items
        {
            set { Recent = value; 
                RefreshXMLItems();//as soon as filename is set. List will refresh
            }
        }

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
                    string text = "<?xml version=\"1.0\" encoding=\"utf-8\"?> "+
                                  "<recent>"+
                                  "</recent>";
                    System.IO.File.WriteAllText(value, text);
                }
            }
        }

        //This will dynamically be assigned any function that we want to call when 
        //recent item from say File > Recent menu is clicked
        public delegate void RecentItemClick(string filename);
        private RecentItemClick _recentitemclick;
        public RecentItemClick recentitemclick 
        { 
            //get; 
            set { _recentitemclick =value;} 
        }
        //loads items from XML to menu
        private void RefreshXMLItems()
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
            RefereshRecentList();//generate UI menu items under 'Recent'
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
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

        [MethodImpl(MethodImplOptions.Synchronized)]
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

        private bool IsDuplicateInXml(string item)
        {
            return FilesList.Contains(item); // FilesList is the latest copy of what XML has.
        }
        #endregion
    
        #region Creating UI menu items
        
        private void RefereshRecentList()//creates actual items in "Recent..."
        {
            ///creating menu items for each recent list string //
            MenuItem recentfile;
            Recent.Items.Clear();//clear old entries
            foreach (string s in this.RecentFileList)
            {
                recentfile = new MenuItem();
                recentfile.Header = s;
                recentfile.Click += new RoutedEventHandler(recentfile_Click);
                Recent.Items.Add(recentfile); //add new entries
            }
        }

        void recentfile_Click(object sender, RoutedEventArgs e)
        {
            MenuItem clickeditem = sender as MenuItem;
            _recentitemclick(clickeditem.Header.ToString()); // calling external subscribed function on click of a recent item
            //MessageBox.Show("Opening..." + clickeditem.Header);
            //FileOpenCommand fopen =new BlueSky.Commands.File.FileOpenCommand();
            //fopen.FileOpen(clickeditem.Header.ToString());
        }

        #endregion

    }
}
