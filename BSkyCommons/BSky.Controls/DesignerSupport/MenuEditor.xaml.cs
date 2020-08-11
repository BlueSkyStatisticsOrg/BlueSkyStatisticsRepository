using System.Windows;
using System;
using System.Windows.Data;
using System.Xml;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using BSky.Controls.Dialogs;
using BSky.Lifetime;
using System.Collections.Generic;
using Microsoft.Win32;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;
using System.Windows.Markup;
using Microsoft.Practices.Unity;
using BSky.Interfaces;
using System.Threading;

namespace BSky.Controls
{
    /// <summary>
    /// Interaction logic for XML_To_TreeView.xaml
    /// </summary>
    public partial class MenuEditor : Window
    {
        enum NodeClass { OWNERCATEGORY, OWNERCOMMAND, USERCATEGORY, USERCOMMAND, SEPERATOR, NONE }; // NONE means unique node name
        // ILoggerService logService = LifetimeService.Instance.Container.Resolve<ILoggerService>();//17Dec2012
        Point _lastMouseDown;
        TreeViewItem draggedItem, _target;
        XmlNode newNode;
        XmlNode removed;// for 1 step undo on treenode
        XmlNode parentofremoved;
        XmlNode presib, nxtsib;
        string newcommlocXpath;
        string newCommandID = "newcommid";//id of element for new command. used in 2 places.
        List<string> commandList = new List<string>();//25Jan2013 For holding all existing user command names
        List<string> categorylist = new List<string>();//10Oct2013 For holding user's category names
        List<string> ownercommandList = new List<string>();//30Jan2013 For holding all existing owner command names
        List<string> extractedfiles = null;//08May2015 for making a list of fullpathfilenames those got extrated in temp, so as to clean them later
        //string xamlFile = string.Empty; //06Mar2013 For XAML Dialog
        //string outputFile = string.Empty; //06Mar2013 for XML output Template
        string CultureName = string.Empty;

        public MenuEditor()
        {
            InitializeComponent();
            ////tv.Style = (Style)this.FindResource("TV_AllExpanded");
            tv.Loaded += new RoutedEventHandler(tv_Loaded);
            ////XmlDataProvider dp = (XmlDataProvider)this.FindResource("xmlDP");
            ////dp.DataChanged += new EventHandler(dp_DataChanged);
            newcommlocXpath = "//menu[@id='analyisMenu']";// for new command default location 

            //Added by Aaron 07/31/2020
            //Comented line below
            //CultureName = Thread.CurrentThread.CurrentCulture.Name;
            //Added line below
            string CultureName = "en-US";
        }

        public MenuEditor(string commandname)
            : this()
        {
            this.NewCommandName = commandname;
            if (commandname == null || commandname.Trim().Length < 1) /// for menu editor not for dialog installer.
                newcommlocXpath = string.Empty;
        }

        void dp_DataChanged(object sender, EventArgs e) //06Mar2013 Code moved to a function and function is called instead
        {
            // AddNewCommandNode(newcommlocXpath);
        }

        #region Some Properties
        private string menuLocation;

        public string ElementLocation
        {
            get
            {
                return menuLocation;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                    return;
                menuLocation = value;
                SetElementLocaton(value);
            }
        }

        public string CommandnameBeforeRename { get; set; } //03Oct2013 When a node is double clicked to rename, this will store the old name

        public object NodeToRename { get; set; }

        public string NewCommandName { get; set; }

        public string XamlFile { get; set; } //06Mar2013 XAML dialog filename

        public string XmlFile { get; set; } //06Mar2013 XML output template filename


        //Added by Aaron 01/19/2014
        List<string> helpfilenames = new List<string>();

        private string aboveBelow; //06Feb2013
        public string NewCommandAboveBelowSibling
        {
            get { return aboveBelow; }
        }

        private bool ismodified = false;
        #endregion

        #region menu.xml related and other helper functions


        private void SetElementLocaton(string val)
        {
            string[] nodes = val.Split('>');
            XmlDataProvider dp = (XmlDataProvider)this.FindResource("xmlDP");
            if (dp.Document == null)
                return;
            XmlNode element = dp.Document.SelectSingleNode("//menus");
            foreach (string node in nodes)
            {
                if (node == "Root")
                    continue;
                if (element == null)
                    return;
                element = element.SelectSingleNode("./menu[@text='" + node + "']");
            }
            if (element == null)
                return;
            if (newNode != null) //15Mar2013 if new command is installed. Not for Overwrite
            {
                if (newNode.ParentNode != null)
                {
                    newNode.ParentNode.RemoveChild(newNode);
                }
                if (element.HasChildNodes)
                {
                    element.AppendChild(this.newNode);
                }
                else
                {
                    element.ParentNode.InsertAfter(this.newNode, element);
                }
            }
        }

        public void LoadXml(string xml)
        {
            newcommlocXpath = string.Empty; // reset location.
            menuLocation = string.Empty; // reset
            try
            {
                XmlDocument doc = new XmlDocument();
                ///doc.Load(xml);/// For loading full xml document

                /// For loading partial/full xml doc
                /// For Graphic Menu:   "//menus/menu[@id='graphicmenu']"  // is same as  "//menu[@id='graphicmenu']" 
                /// For Full XML:       "*"
                /// For Analysis Menu:  "//menus/menu[@id='analyisMenu']"
                string xpath = "*";//"//menu[@id='graphicmenu']";//
                //// for new command default location ////
                if (xpath != "*")
                    newcommlocXpath = xpath;

                doc = (LoadPartial(xml, xpath));

                //Use the XDP that has been created as one of the Window's resources ...
                XmlDataProvider dp = (XmlDataProvider)this.FindResource("xmlDP");
                //... and assign the XDoc to it, using the XDoc's root.
                dp.Document = doc;
                dp.XPath = doc.DocumentElement.Name;// "menus";//or menu
                dp.DataChanged += new EventHandler(dp_DataChanged);
                tv.Style = (Style)this.FindResource("TV_AllExpanded");
            }
            catch
            {
                MessageBox.Show(this, "Couldn't Load Menu");

                //  logService.WriteToLogLevel("Couldn't Load Menu.", LogLevelEnum.Error);

            }
            CreateRefreshTextAttrList();//list of all existing commands 01Feb2013
        }

        public XmlDocument LoadPartial(string xmlfilename, string xpath)
        {
            XmlDocument xd = new XmlDocument();
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(xmlfilename);

                XmlNode xn = doc.SelectSingleNode(xpath);
                xd.LoadXml(xn.OuterXml);
            }
            catch (Exception ex)
            {
                //logService.WriteToLogLevel("Loading partial menu failed. ", LogLevelEnum.Error, ex);
            }
            return xd;
        }

        public void SaveXml(string xmlfilename)
        {
            try
            {
                //Use the XDP that has been created as one of the Window's resources ...
                XmlDataProvider dp = (XmlDataProvider)this.FindResource("xmlDP");
                ///dp.Document.Save(xmlfilename);

                XmlDocument fulldoc = (LoadPartial(xmlfilename, "*")); //original doc
                XmlDocument doc = dp.Document;///modified full or part
                XmlNode newNodes = doc.SelectSingleNode("*").Clone(); ;
                string XPath = doc.DocumentElement.Name;
                string attr = string.Empty, attrval = string.Empty, attpart = string.Empty;
                if (doc.DocumentElement.Attributes.Count > 0)
                {
                    attr = doc.DocumentElement.Attributes["id"].Name;
                    attrval = doc.DocumentElement.Attributes["id"].Value;
                    attpart = "[@" + attr + "='" + attrval + "']";
                }
                XPath = "//" + XPath + attpart;
                XmlNode oldNode = fulldoc.SelectSingleNode(XPath);
                XmlNode previous = oldNode.PreviousSibling;
                XmlNode parentNode = oldNode.ParentNode;

                //XmlElement xe = fulldoc.CreateElement("menu");xe.SetAttribute("id", "test");
                //xe.AppendChild(oldNode);
                //xe.AppendChild(newNode);//
                if (parentNode != null)
                {
                    parentNode.RemoveChild(oldNode);///remove old child node
                    //fulldoc.CreateElement("menu");   
                    if (parentNode.OwnerDocument != null)
                    {
                        XmlNode imported = parentNode.OwnerDocument.ImportNode(newNodes, true);
                        parentNode.InsertAfter(imported, previous);///add new child node after previous node
                        //23Apr2015 fulldoc.Save(@"./Config/Menu.xml");
                        //fulldoc.Save(string.Format(@"{0}Menu.xml", BSkyAppDir.BSkyAppDirConfigPath));//23Apr2015 

                        fulldoc.Save(string.Format(@"{0}menu.xml", BSkyAppDir.RoamingUserBSkyConfigL18nPath));// + CultureName + "/"));//02Oct2017
                    }
                    else
                    {
                        XmlDocument doc1 = new XmlDocument();
                        doc1.LoadXml(newNodes.OuterXml);
                        //23Apr2015  doc1.Save(@"./Config/Menu.xml");
                        //doc1.Save(string.Format(@"{0}Menu.xml", BSkyAppDir.BSkyAppDirConfigPath));//23Apr2015  

                        doc1.Save(string.Format(@"{0}menu.xml", BSkyAppDir.RoamingUserBSkyConfigL18nPath));// + CultureName + "/"));//02Oct2017
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Couldn't Save Menu");
                //logService.WriteToLogLevel("Couldn't save menu.", LogLevelEnum.Error, ex);
            }
        }

        //17Oct2013 Fixed for:same location can have command name = category name. 
        //But 2 commands and 2 categories cant have same name in same location
        // added string leafnode to see if say D(leaf) is dropped in D(category) in current category.
        private string GetPathFromRoot(XmlNode element)
        {
            string leafnode = string.Empty;//17Oct2013
            if (element.Name == "menus")
            {
                return "Root";
            }
            else
            {
                XmlNode parentNode = element.ParentNode;
                if (element.Attributes["nodetype"] != null && element.Attributes["nodetype"].Value.Equals("Leaf")//17Oct2013
                    && !element.HasChildNodes)
                    leafnode = "leaf";
                if (parentNode != null)
                {
                    return GetPathFromRoot(parentNode) + ">" + element.Attributes["text"].Value + leafnode;
                }
                else
                {
                    if (element.Attributes != null)//if(element.NodeType.Equals("Document"))
                        return element.Attributes["text"].Value;
                    else
                        return "Root";
                }
            }
        }


        //Only rename text attribute. No need to rename XAML/XML files. For Overwrite backup current XAML

        //Renames as well as remove duplcate command names too.
        void RenameCommandText(XmlElement modxe, string oldcommandname, string newcommandname, bool overwrite)
        {
            //23Apr2015 const string FileName = @"./Config/menu.xml";
            //string FileName = string.Format(@"{0}menu.xml", BSkyAppDir.BSkyAppDirConfigPath);//23Apr2015 
            string FileName = string.Format(@"{0}menu.xml", BSkyAppDir.RoamingUserBSkyConfigL18nPath);// + CultureName + "/");//02Oct2017

            //13Mar2013 XmlElement modifiedelement = tblk.DataContext as XmlElement;
            XmlElement modifiedelement = modxe;//13Mar2013
            XmlDocument xdc = modifiedelement.OwnerDocument;

            string nodetype = modifiedelement.GetAttribute("nodetype");
            // will be used in overwrite to carefully delete other node that does not have same xaml name
            // 'commandtemplate' is unique, 'id' may not be unique in menu.xml
            //string xamlname = modifiedelement.GetAttribute("commandtemplate");


            if (nodetype.Trim().Equals("Leaf") || nodetype.Trim().Equals("Parent"))
            {
                if (overwrite) //existing name thats why overwrite
                {
                    //move old XAML/XML to some backup folder

                    XmlNode alreadyExistingNode = null;
                    XmlNodeList xnl = xdc.SelectNodes("//menus//menu[@text='" + newcommandname.Trim() + "']");
                    //string othernodexamlname = string.Empty; // node to be overwritten. The other node which was there already.
                    for (int i = 0; i < xnl.Count; i++) //count should be 2 always. One is old node and one is new node with same name.
                    {
                        if (modifiedelement.Equals(xnl[i]))
                        {
                            //MessageBox.Show("exactly same node");
                            continue;
                        }
                        else // diferent node. That already exists and is to be overwritten.
                        {
                            alreadyExistingNode = xnl[i];
                            break;
                        }

                        ///following is the second way. But this does not work when category name matches another already category name.
                        ///all other 3 cases work well.
                        //if (xnl[i] != null)
                        //{
                        //    othernodexamlname = xnl[i].Attributes["commandtemplate"] != null ? xnl[i].Attributes["commandtemplate"].Value : string.Empty;
                        //    //if xamlname is different then current node in iteration then it is the old node to be removed(overwritten)
                        //    if ( !(othernodexamlname.Trim().Equals(xamlname.Trim())))
                        //    {
                        //        alreadyExistingNode = xnl[i];
                        //        break;
                        //    }
                        //}
                    }

                    // if node exists, remove it from menu.xml
                    if (alreadyExistingNode != null)
                    {
                        alreadyExistingNode.ParentNode.RemoveChild(alreadyExistingNode);
                    }

                }

                modifiedelement.SetAttribute("text", newcommandname);//if unique name is given, just execute this

            }

            //save menu.xml state for new attribute values ///
            //xdc.Save(FileName);
            //LoadXml(FileName);//reload file again, to have latest installed command.
        }

        //  Reading all 'menu' node and creating a list of 'text' attribute list. This list can be used to
        //  find duplicate command names while adding new command.
        private void CreateRefreshTextAttrList()
        {
            XmlDataProvider dp = (XmlDataProvider)this.FindResource("xmlDP");
            XmlDocument xmld = dp.Document;
            ownercommandList.Clear();
            commandList.Clear();
            categorylist.Clear();
            //List<string> commandList = new List<string>();
            //skip 'separator' and duplicate values if any
            XmlNodeList allmenus = xmld.GetElementsByTagName("menu");
            for (int i = 0; i < allmenus.Count; i++)
            {
                //menuItemName can be command or category. Name shown in UI menu
                string menuItemName = (allmenus[i].Attributes["text"] != null) ? allmenus[i].Attributes["text"].Value : "";
                string commandCategory = (allmenus[i].Attributes["owner"] != null) ? allmenus[i].Attributes["owner"].Value : "";
                string nodetype = (allmenus[i].Attributes["nodetype"] != null) ? allmenus[i].Attributes["nodetype"].Value : "";
                if (!menuItemName.Trim().Equals("---------") && !commandList.Contains(menuItemName.Trim())) //skip separator & duplicate command
                {
                    if (commandCategory.Trim().Equals("BSky"))
                    {
                        ownercommandList.Add(menuItemName.Trim());//Owner commands list
                    }
                    else // its user's category name or command name
                    {
                        if (nodetype.Trim().Equals("Leaf"))
                            commandList.Add(menuItemName.Trim());//User commands list
                        else if (nodetype.Trim().Equals("Parent"))
                            categorylist.Add(menuItemName.Trim()); // add user category name in this list
                    }
                }
            }
            //MessageBox.Show("Done");
        }




        #endregion

        #region New Command
        //06Mar2013 To install multiple new dialogs. XAML and XML are copied to Config after choosing name for them.
        private void InstallNewCommand(string NewCommLocXpath)
        {

            ///set location///
            XmlDataProvider dp = (XmlDataProvider)this.FindResource("xmlDP");
            XmlNode Location = null;
            if (NewCommLocXpath != null && NewCommLocXpath.Trim().Length > 0)
                Location = dp.Document.SelectSingleNode(NewCommLocXpath);//"//menu[@id='analyisMenu']");            
            ElementLocation = GetPathFromRoot(Location);

            string sInput = this.NewCommandName;
            string location = this.ElementLocation;
            string tempDir = Path.GetTempPath();
            string abvBlw = (this.NewCommandAboveBelowSibling != null) ? this.NewCommandAboveBelowSibling : string.Empty;
            //sInput is prefixed to XAML and XML filenames. Its the command name as shown in UI
            if (!string.IsNullOrEmpty(location))
            {
                //06Mar2013 Following will actually modify menu.xml to add new command
                //23Apr2015 AddNewCommandXmlEntry(location, sInput, Path.Combine(@".\Config\", sInput + XamlFile), true, abvBlw);
                AddNewCommandXmlEntry(location, sInput, Path.Combine(string.Format(@"{0}", BSkyAppDir.RoamingUserBSkyConfigL18nPath), sInput + XamlFile), true, abvBlw);//23Apr2015
                ///output template filename will be same as dialog filename. No matter whats in zip file.
                //23Apr2015 string xmlinstallpath = Path.GetFullPath(@".\Config\" + sInput + XamlFile.Replace("xaml", "xml"));
                string xmlinstallpath = Path.GetFullPath(string.Format(@"{0}", BSkyAppDir.RoamingUserBSkyConfigL18nPath) + sInput + XamlFile.Replace("xaml", "xml"));//23Apr2015

                System.IO.File.Copy(Path.Combine(tempDir, XmlFile), xmlinstallpath, true);

                //23Apr2015 string xamlinstallpath = Path.GetFullPath(@".\Config\" + sInput + XamlFile);
                string xamlinstallpath = Path.GetFullPath(string.Format(@"{0}", BSkyAppDir.RoamingUserBSkyConfigL18nPath) + sInput + XamlFile);//23Apr2015 

                System.IO.File.Copy(Path.Combine(tempDir, XamlFile), xamlinstallpath, true);

                //MessageBox.Show("Dialog(s) modified. Changes will take effect after restarting the application!", "Info: Dialog installed.");
                ////mainWin.Window_Refresh_Menus();//15Jan2013
            }
        }

        //Modify menu.xml for new command
        private bool? AddNewCommandXmlEntry(string val, string Title, string commandFile, bool forcePlace, string AboveBelowSibling)
        {
            //23Apr2015 const string FileName = @"./Config/menu.xml";
            //string FileName = string.Format(@"{0}menu.xml", BSkyAppDir.BSkyAppDirConfigPath);//23Apr2015 
            string FileName = string.Format(@"{0}menu.xml", BSkyAppDir.RoamingUserBSkyConfigL18nPath);// + CultureName + "/");//02Oct2017

            XmlDocument document = new XmlDocument(); ;

            if (string.IsNullOrEmpty(val))
                return null;
            string[] nodes = val.Split('>');

            ////reloading a latest document. Modified by Install dialog window ///
            document.Load(FileName);

            XmlElement newelement = document.CreateElement("menu");

            XmlAttribute attrib = document.CreateAttribute("id");
            attrib.Value = Title.Replace(' ', '_');
            newelement.Attributes.Append(attrib);

            attrib = document.CreateAttribute("text");
            attrib.Value = Title;
            newelement.Attributes.Append(attrib);

            attrib = document.CreateAttribute("commandtemplate");
            attrib.Value = commandFile + ".xaml"; // commandfile should be filename/commandname without extension
            newelement.Attributes.Append(attrib);

            attrib = document.CreateAttribute("commandoutputformat");
            attrib.Value = commandFile + ".xml"; //.Replace("xaml", "xml");//same filenames(dialog and out-template) but diff extensions
            newelement.Attributes.Append(attrib);

            attrib = document.CreateAttribute("owner");
            attrib.Value = "";//Newly Insalled commands are always leaf node
            newelement.Attributes.Append(attrib);

            attrib = document.CreateAttribute("nodetype");
            attrib.Value = "Leaf";//Newly Insalled commands are always leaf node
            newelement.Attributes.Append(attrib);

            XmlNode element = document.SelectSingleNode("//menus");

            foreach (string node in nodes)//Traverse to target parent, where new command should be added
            {
                if (node == "Root")
                    continue;
                if (element == null)
                    return null;
                element = element.SelectSingleNode("./menu[@text='" + node + "']");
            }
            if (element == null)//parent not found.
                return null;

            //if parent node or new node then add as child //04Oct2013 added nodetype condition in place of older one with HasChild
            if ((element.Attributes["nodetype"] != null && element.Attributes["nodetype"].Value == "Parent") || (element.Attributes["id"] != null && element.Attributes["id"].Value == "new_id"))
            {
                XmlNode temp = element.SelectSingleNode("./menu[@text='" + Title + "']");
                if (temp == null || forcePlace)
                {
                    element.AppendChild(newelement);//Add as a last leaf node.(last sibling)
                }
                else
                    return false;
            }
            else /// add as sibling below target node.(target node is leaf node here)
            {
                XmlNode temp = element.ParentNode.SelectSingleNode("./menu[@text='" + Title + "']");
                if (temp == null || forcePlace)
                {
                    //if (AboveBelowSibling.Trim().Equals("Below"))

                    if (AboveBelowSibling.Trim().Equals("Above"))
                        element.ParentNode.InsertBefore(newelement, element);
                    else if (AboveBelowSibling.Trim().Equals("Below"))
                        element.ParentNode.InsertAfter(newelement, element);//06Feb2013
                    else
                        element.AppendChild(newelement); // 'else' is not needed here. No harm keeping it. this can work for parent
                    // those are not owner="BSky" and not having id="new_id" and do not have any children. (if you try to overwrite
                    // "Data File" command )

                }
                else
                    return false;
            }
            document.Save(FileName);

            LoadXml(FileName);//reload file again, to have lates installed command.
            return true;
        }
        #endregion

        #region TextBox and TextBlock Events

        bool editmode = false;
        private void tb_MouseDown(object sender, MouseButtonEventArgs e)// New Command naming logic
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (e.ClickCount == 2)
                {
                    NodeToRename = null;// not needed for double click. Needed for rt clk context menu
                    switchToEditMode(sender, string.Empty);

                    //following moved to function called above
                    //editmode = true;
                    ////MessageBox.Show("you double-clicked");
                    //TextBox txtb = (TextBox)((Grid)((TextBlock)sender).Parent).Children[2]; // [0] is image, [1] is textblock, [2] is textbox
                    //((TextBlock)sender).Tag = ((TextBlock)sender).Text;//05Mar2013 Store original value. Later you can see if it was modified or not
                    //if (!txtb.IsReadOnly)
                    //{
                    //    ((TextBlock)sender).Visibility = Visibility.Collapsed;
                    //    txtb.Visibility = Visibility.Visible;
                    //    txtb.Text = ((TextBlock)sender).Text;
                    //    txtb.IsEnabled = true;
                    //    CommandnameBeforeRename = txtb.Text; //03Oct2013
                    //    //// Focus on text box ////
                    //    Dispatcher.BeginInvoke(DispatcherPriority.Input, (System.Threading.ThreadStart)delegate()
                    //    {
                    //        txtb.SelectAll();
                    //        txtb.Focus();
                    //    });
                    //}
                }
                //else /// change foreground of seleted node to white //31Jan2013
                //{
                //    TextBlock tbl =  ((TextBlock)sender);
                //    tbl.Foreground = Brushes.White;
                //    tbl.Focus();//forecfully setting focus so that lost-focus can work.
                //    e.Handled = true;
                //}
            }
            if (e.RightButton == MouseButtonState.Pressed) //04Oct2013
            {
                NodeToRename = sender;
                
            }
        }

        private void tb_LostFocus(object sender, RoutedEventArgs e) // reset node color back to original
        {
            //((TextBlock)sender).Foreground = Brushes.Black;

        }

        //finally after entring new command name, you press Enter to finalise that name /// See abv. 2 events works on focus too //
        private void txtbox_KeyDown(object sender, KeyEventArgs e)
        {
            //TextBlock tb = (TextBlock)((Grid)((TextBox)sender).Parent).Children[1];
            if (e.Key == Key.Enter)
            {
                textbox_focuslost(sender);
            }
        }

        //finally after entring new command name, you click else where to lose focus and to finalise that name ///
        private void txtbox_LostFocus(object sender, RoutedEventArgs e)
        {
            textbox_focuslost(sender);
        }

        //Adding new command or overwriting old one
        //finally after entring new command name, you click else where to lose focus and to finalise that name ///

        private void textbox_focuslost(object sender)
        {
            editmode = false;
            TextBlock tb = (TextBlock)((Grid)((TextBox)sender).Parent).Children[1];
            NewCommandName = ((TextBox)sender).Text;
            tb.Text = NewCommandName;

            bool overwrite = false;
            bool canrename = false;
            string oldCommandName = tb.Tag.ToString().Trim();
            if (NewCommandName.Trim().Equals(oldCommandName))//if text has not been changed
            {
                //MessageBox.Show("No Change in Text");
                tb.Visibility = Visibility.Visible;
                ((TextBox)sender).Visibility = Visibility.Collapsed;
                return;
            }
            /// Look in Menu XML if the same name already exists and is user or BSky category.  //25Jan2013
            //else if (ownercommandList.Contains(NewCommandName))//Do not overwrite standard commands of an app
            //{
            //    MessageBox.Show("Please use unique name. \'" + NewCommandName + "\' is standard command of the application.");
            //    tb.Text = oldCommandName;//03oct2013 "New Command Node";
            //    ((TextBox)sender).Text = tb.Text;
            //}

            else
            {

                XmlElement xe = (((TreeViewItem)tv.Tag).Header as XmlElement);
                string clickednodetype = (xe as XmlNode).Attributes["nodetype"] != null ? (xe as XmlNode).Attributes["nodetype"].Value : string.Empty;

                string msg = string.Empty;
                //bool isownersibling;
                //bool isunique = isUniqueSibling(NewCommandName, (((((TreeViewItem)tv.Tag).Header as XmlElement).ParentNode) as XmlElement), false, out isownersibling);
                NodeClass nc = GetNodeClass(NewCommandName, ((xe.ParentNode) as XmlElement), false, clickednodetype);
                MessageBoxResult mbr;

                //Following switch has a new rule that allows us to have command name = category name in same location.
                switch (nc)
                {
                    case NodeClass.OWNERCATEGORY:
                        msg = "Standard (non- modifiable) Category with same name exists in current location! Can't Overwrite";
                        MessageBox.Show(this, NewCommandName + "\n" + msg, "Cannot Overwrite Standard Category.", MessageBoxButton.OK, MessageBoxImage.Error);
                        tb.Text = oldCommandName;//03oct2013 "New Command Node";
                        ((TextBox)sender).Text = tb.Text;

                        break;
                    case NodeClass.OWNERCOMMAND:

                        msg = "Standard (non- modifiable) Command with same name exists in current location! Can't Overwrite";
                        MessageBox.Show(this, NewCommandName + "\n" + msg, "Cannot Overwrite Standard Command", MessageBoxButton.OK, MessageBoxImage.Error);
                        tb.Text = oldCommandName;//03oct2013 "New Command Node";
                        ((TextBox)sender).Text = tb.Text;

                        break;
                    case NodeClass.USERCATEGORY:

                        msg = "Category with same name exists in current location! Everything under this category will be deleted.";
                        mbr = MessageBox.Show(this, "Overwrite " + NewCommandName + "?\n" + msg, "Confirm Category Overwrite", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                        if (mbr == MessageBoxResult.No)
                        {
                            tb.Text = oldCommandName;//03oct2013 "New Command Node";
                            ((TextBox)sender).Text = tb.Text;
                        }
                        else
                        {
                            overwrite = true;
                            canrename = true;
                        }


                        break;
                    case NodeClass.USERCOMMAND:

                        msg = "Command with same name exists in current location!";
                        mbr = MessageBox.Show(this, "Overwrite " + NewCommandName + "?\n" + msg, "Confirm Command Overwrite", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                        if (mbr == MessageBoxResult.No)
                        {
                            tb.Text = oldCommandName;//03oct2013 "New Command Node";
                            ((TextBox)sender).Text = tb.Text;
                        }
                        else
                        {
                            overwrite = true;
                            canrename = true;
                        }

                        break;
                    case NodeClass.NONE:
                        canrename = true;
                        break;
                    default:
                        break;
                }
            }
            tb.Visibility = Visibility.Visible;
            ((TextBox)sender).Visibility = Visibility.Collapsed;

            if (canrename)
            {
                ////Command renamed in UI. Also rename in menu.xml and dont rename XAML/XML files in Config.
                XmlElement modifiedelement = tb.DataContext as XmlElement;
                // command/category renamed with or w/o overwrite.
                RenameCommandText(modifiedelement, oldCommandName, NewCommandName, overwrite);
                //updating the command lists //01Feb2013
                CreateRefreshTextAttrList();
                tb.Tag = NewCommandName; //07Oct2013
            }
        }


        #endregion

        #region TreeView Events

        void tv_Loaded(object sender, RoutedEventArgs e)
        {
        }

        private void tv_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            try
            {
                if (e.LeftButton == MouseButtonState.Pressed
                    && editmode != true)
                {
                    Point currentPosition = e.GetPosition(tv);

                    // object data = GetDataFromTreeView(this, e.GetPosition(this)) as object;


                    if ((Math.Abs(currentPosition.X - _lastMouseDown.X) > 10.0) ||
                        (Math.Abs(currentPosition.Y - _lastMouseDown.Y) > 10.0))
                    {

                        draggedItem = GetNearestContainer(e.OriginalSource as UIElement);  //old code -> (TreeViewItem)tv.Tag;
                        //draggedItem = null;//for testing
                        if (draggedItem != null)
                        {
                            DragDropEffects finalDropEffect = DragDrop.DoDragDrop(tv, tv.SelectedItem, DragDropEffects.Move);
                            //Checking target is not null and item is 
                            //dragging(moving)
                            if ((finalDropEffect == DragDropEffects.Move) && (_target != null))
                            {
                                string dragOwnerAtt = string.Empty;
                                string dragTextAtt = string.Empty;
                                string dragNodetypeAtt = string.Empty;
                                string targetOwnerAtt = string.Empty;
                                string targetTextAtt = string.Empty;
                                string targetNodetypeAtt = string.Empty;
                                string dragIdAtt = string.Empty;

                                /// check if attribute exists in source or target ///
                                if (((XmlElement)draggedItem.Header).Attributes["owner"] != null)
                                    dragOwnerAtt = ((XmlElement)draggedItem.Header).Attributes["owner"].Value;
                                if (((XmlElement)draggedItem.Header).Attributes["text"] != null)
                                    dragTextAtt = ((XmlElement)draggedItem.Header).Attributes["text"].Value;
                                if (((XmlElement)draggedItem.Header).Attributes["nodetype"] != null)
                                    dragNodetypeAtt = ((XmlElement)draggedItem.Header).Attributes["nodetype"].Value;

                                if (((XmlElement)_target.Header).Attributes["owner"] != null)
                                    targetOwnerAtt = ((XmlElement)_target.Header).Attributes["owner"].Value;
                                if (((XmlElement)_target.Header).Attributes["text"] != null)
                                    targetTextAtt = ((XmlElement)_target.Header).Attributes["text"].Value;
                                if (((XmlElement)_target.Header).Attributes["nodetype"] != null)
                                    targetNodetypeAtt = ((XmlElement)_target.Header).Attributes["nodetype"].Value;

                                if (((XmlElement)draggedItem.Header).Attributes["id"] != null)
                                    dragIdAtt = ((XmlElement)draggedItem.Header).Attributes["id"].Value;
                                //string newCommandNodeText = ((TreeViewItem)tv.SelectedItem).Header.ToString();

                                /////// A Move drop was accepted or rejected/////////

                                /////// 17Oct2013 ////////////Starts///////////
                                //17Oct2013 Also see if the dragged item when dropped in a location must check following
                                // if dragged item is a command then in target location BSky command is present, we abort move.
                                // if dragged item is a category then in target location BSky category is present, we abort move.
                                // if user command is present in target category we ask for overwrite confirmation. 
                                // if user category is present in target category we ask for overwrite confirmation.
                                // user command can stay with BSky category or user category with same name in same location
                                // user category can stay with BSky command or user command with same name in same location
                                XmlElement trgxe = targetNodetypeAtt.Equals("Parent") ? ((XmlElement)_target.Header) : ((((XmlElement)_target.Header).ParentNode) as XmlElement);
                                NodeClass nc = GetNodeClass(dragTextAtt, trgxe, false, dragNodetypeAtt);
                                MessageBoxResult mbr;
                                string msg = string.Empty;
                                bool overwrite = false;
                                //Following switch has a new rule that allows us to have command name = category name in same location.
                                switch (nc)
                                {
                                    case NodeClass.OWNERCATEGORY:
                                        msg = "Standard (non- modifiable) Category with same name exists in current location! Can't Move.";
                                        MessageBox.Show(this, dragTextAtt + "\n" + msg, "Cannot Overwrite Standard Category.", MessageBoxButton.OK, MessageBoxImage.Error);
                                        return;
                                    case NodeClass.OWNERCOMMAND:

                                        msg = "Standard (non- modifiable) Command with same name exists in current location! Can't Move";
                                        MessageBox.Show(this, dragTextAtt + "\n" + msg, "Cannot Overwrite Standard Command", MessageBoxButton.OK, MessageBoxImage.Error);
                                        return;
                                    case NodeClass.USERCATEGORY:

                                        msg = "Category with same name exists in current location! Everything under this category will be deleted.";
                                        mbr = MessageBox.Show(this, "Overwrite " + dragTextAtt + "?\n" + msg, "Confirm Category Overwrite", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                                        if (mbr == MessageBoxResult.Yes)
                                        {
                                            overwrite = true;
                                        }


                                        break;
                                    case NodeClass.USERCOMMAND:

                                        msg = "Command with same name exists in current location!";
                                        mbr = MessageBox.Show(this, "Overwrite  " + dragTextAtt + "?\n" + msg, "Confirm Command Overwrite", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                                        if (mbr == MessageBoxResult.Yes)
                                        {
                                            overwrite = true;
                                        }

                                        break;
                                    case NodeClass.NONE:
                                        overwrite = true;
                                        break;
                                    default:
                                        break;
                                }

                                //remove user nodes ( command or category ) from target location, if overwrite is chosen by user.
                                // not-None means there is user node with same name, so overwrite if overwrite = true
                                if (overwrite && nc != NodeClass.NONE)
                                {
                                    if (trgxe.HasChildNodes)
                                    {

                                        XmlNodeList oldxn = trgxe.ChildNodes;//trgxe.SelectNodes("//menu[@text='" + dragTextAtt.Trim() + "' and @owner!='BSky']");
                                        if (oldxn != null)
                                        {
                                            for (int i = 0; i < oldxn.Count; i++)
                                            {
                                                if (oldxn[i] != null)
                                                {
                                                    //if old item is itself dragged item ( when you move(drag) your command up or down among other commands 
                                                    //under same parent category ). That time ignore this node. Otherwise it will be removed assuming its duplicate.
                                                    if (oldxn[i].Equals(((XmlNode)draggedItem.Header)))
                                                        continue;

                                                    //Remove old item if
                                                    // its not BSky. old item nodetype = nodetype of dragged item, name of old item = name of dragged item 
                                                    if ((oldxn[i].Attributes["owner"] != null && !oldxn[i].Attributes["owner"].Value.Equals("BSky")) || // if owner attr is present
                                                        oldxn[i].Attributes["owner"] == null) // if owner attribute is missing
                                                    {
                                                        if (oldxn[i].Attributes["nodetype"] != null && oldxn[i].Attributes["nodetype"].Value.Equals(dragNodetypeAtt) &&
                                                            oldxn[i].Attributes["text"] != null && oldxn[i].Attributes["text"].Value.Equals(dragTextAtt)
                                                            )
                                                        {
                                                            trgxe.RemoveChild(oldxn[i]);
                                                        }
                                                    }
                                                }

                                            }
                                        }
                                    }
                                }
                                /////// 17Oct2013 ////////////Ends/////////////


                                //if (dragIdAtt != newCommandID)//if dragged item is not new command
                                //return;
                                if ((dragOwnerAtt == "BSky" && dragIdAtt != newCommandID)//move rejected if dragged item is "BSky" and non-newcommand
                                    || targetOwnerAtt == "BSky")//"move" rejected if target is "BSky"
                                {
                                    MessageBox.Show(this, "Drag-Drop is not allowed in Standard (non- modifiable) items (in Gray).\n"
                                        , "Select proper source and target node.", MessageBoxButton.OK, MessageBoxImage.Information);
                                    return;
                                }

                                //17Oct2013 overwrite is allowed if target already has command or category with same name as the moved item
                                if (!(((XmlNode)draggedItem.Header).Equals(((XmlNode)_target.Header))))//17Oct2013 (dragTextAtt != targetTextAtt)//source and target must be different. 
                                {                                //move accepted for non-Bsky and new command
                                    CopyItem(draggedItem, _target);
                                    _target = null;
                                    draggedItem = null;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                //logService.WriteToLogLevel("Error in mouse move logic.", LogLevelEnum.Error, ex);
            }

        }

        private void tv_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                _lastMouseDown = e.GetPosition(tv);

            }
            tv.Focus();//raising an event to loose focus on textbox

            //06Mar2013 For context menu, on right click the treeview item will be selected
            TreeViewItem item = GetNearestContainer(e.OriginalSource as UIElement);
            if (item != null)
                item.IsSelected = true;

            //MessageBox.Show("TV MouseDown and item selected");
        }


        private void tv_DragOver(object sender, DragEventArgs e)
        {
            try
            {
                //if (true)
                //{
                //    e.Effects = DragDropEffects.None;
                //    return; // for testing scrollbar
                //}
                Point currentPosition = e.GetPosition(tv);

                if ((Math.Abs(currentPosition.X - _lastMouseDown.X) > 10.0) ||
                   (Math.Abs(currentPosition.Y - _lastMouseDown.Y) > 10.0))
                {
                    // Verify that this is a valid drop and then store the drop target
                    TreeViewItem item = GetNearestContainer(e.OriginalSource as UIElement);
                    if (CheckDropTarget(draggedItem, item))
                    {
                        e.Effects = DragDropEffects.Move;
                    }
                    else
                    {
                        e.Effects = DragDropEffects.Move;
                    }
                }
                e.Handled = true;
            }
            catch (Exception ex)
            {
                //logService.WriteToLogLevel("Error in drag over.", LogLevelEnum.Error, ex);
            }

        }

        private void tv_Drop(object sender, DragEventArgs e)
        {
            try
            {
                e.Effects = DragDropEffects.None;
                e.Handled = true;

                // Verify that this is a valid drop and then store the drop target
                TreeViewItem TargetItem = GetNearestContainer(e.OriginalSource as UIElement);
                if (TargetItem != null && draggedItem != null)
                {
                    _target = TargetItem;
                    e.Effects = DragDropEffects.Move;
                }
            }
            catch (Exception ex)
            {
                //logService.WriteToLogLevel("Error in drop.", LogLevelEnum.Error, ex);
            }

        }

        private void tv_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {

        }

        private void tv_Selected(object sender, RoutedEventArgs e)// Left Click > item selected
        {
            tv.Tag = e.OriginalSource;
            //MessageBox.Show("TV Selected");
        }

        private bool CheckDropTarget(TreeViewItem _sourceItem, TreeViewItem _targetItem)
        {
            //Check whether the target item is meeting your condition
            bool _isEqual = false;
            if (!_sourceItem.Header.ToString().Equals(_targetItem.Header.ToString()))
            {
                _isEqual = true;
            }
            return _isEqual;

        }

        private TreeViewItem GetNearestContainer(UIElement element)
        {
            // Walk up the element tree to the nearest tree view item.
            TreeViewItem container = element as TreeViewItem;
            while ((container == null) && (element != null))
            {
                element = VisualTreeHelper.GetParent(element) as UIElement;
                container = element as TreeViewItem;
            }
            return container;
        }

        private void CopyItem(TreeViewItem _sourceItem, TreeViewItem _targetItem)
        {
            //Asking user wether he want to drop the dragged TreeViewItem here or not
            if (true) //06Oct2013 MessageBox.Show("Would you like to drop " + ((XmlElement)_sourceItem.Header).Attributes["text"].Value + " into " + ((XmlElement)_targetItem.Header).Attributes["text"].Value + "", "Drop on target?", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                try
                {
                    //adding dragged TreeViewItem in target TreeViewItem
                    addChild(_sourceItem, _targetItem);
                }
                catch (Exception ex)
                {
                    //logService.WriteToLogLevel("Error in copying item to destination.", LogLevelEnum.Error, ex);
                }
            }
        }



        public void addChild(TreeViewItem _sourceItem, TreeViewItem _targetItem)
        {
            try
            {
                //MessageBox.Show(GetPathFromRoot(_sourceItem.Header as XmlNode));
                string sourceLocation = GetPathFromRoot(_sourceItem.Header as XmlNode);
                menuLocation = GetPathFromRoot(_targetItem.Header as XmlNode);
                //MessageBox.Show(menuLocation);
                XmlElement element = _sourceItem.Header as XmlElement;
                XmlElement tarElement = _targetItem.Header as XmlElement;
                if (menuLocation.Contains(sourceLocation))//parent(source) cant be child of its child 07Nov2012
                {
                    MessageBox.Show(this, "Parent catagory can't be placed under its own child");
                    return;
                }
                //element.ParentNode.RemoveChild(element);//if removed here. Could be a problem

                //10Sep2014 if source and targets nodes are "Parent" (non-leaf, ie. folders) then 
                // show popup. Ask if source goes above, below or inside of target folder.
                if ((element.Attributes["nodetype"] != null && element.Attributes["nodetype"].Value == "Parent") && 
                    (tarElement.Attributes["nodetype"] != null && tarElement.Attributes["nodetype"].Value == "Parent")

                    )
                {

                    //string titlemessage = "Where would you like to drop " + ((XmlElement)_sourceItem.Header).Attributes["text"].Value + " in " + ((XmlElement)_targetItem.Header).Attributes["text"].Value;//24Jan2013
                    string sourceN = ((XmlElement)_sourceItem.Header).Attributes["text"].Value;//N for Node
                    string targetN = ((XmlElement)_targetItem.Header).Attributes["text"].Value;
                    MenuAboveBelowDialog dragLocDialog = new MenuAboveBelowDialog(sourceN, targetN);//10Sep2014

                    dragLocDialog.Owner = this;
                    dragLocDialog.ShowDialog();
                    string choice = dragLocDialog.SelectedOption;
                    aboveBelow = choice;//06Feb2013 For setting location of new command relative to its sibling.

                    //12Sep2014Extra Check. There TEXT must not match. 
                    //Folder with same name in same location should not be allowed
                    if (!choice.Equals("Inside") && (element.Attributes["text"] != null && tarElement.Attributes["text"] != null) &&
                    (element.Attributes["text"].Value == tarElement.Attributes["text"].Value))
                    {
                        MessageBox.Show("Same named catagories cant be placed in same location.");
                        return;
                    }

                    ///show dialog box asking where to drop source node.(above or below target)
                    //MessageBox.Show(this,"Move it above or below target?", "Above or Below", MessageBoxButton.YesNoCancel, MessageBoxImage.Asterisk, MessageBoxResult.OK, MessageBoxOptions.None);
                    if (choice.Equals("Below"))//below target sibling
                    {
                        element.ParentNode.RemoveChild(element);//remove just before adding.
                        tarElement.ParentNode.InsertAfter(element, tarElement);
                    }
                    else if (choice.Equals("Above"))//above target silbling
                    {
                        element.ParentNode.RemoveChild(element);//remove just before adding.
                        tarElement.ParentNode.InsertBefore(element, tarElement);

                    }
                    else if (choice.Equals("Inside"))//inside target as child
                    {
                        element.ParentNode.RemoveChild(element);//remove just before adding.
                        tarElement.AppendChild(element);

                    }
                    else
                        return;
                }
                else //10Sep2014 Code in else was without 'else' before 10Sep2014
                {
                    /// Add as child if it has child OR if its new parent(new_id)
                    if ((tarElement.Attributes["nodetype"] != null && tarElement.Attributes["nodetype"].Value == "Parent") || (tarElement.Attributes["id"] != null && tarElement.Attributes["id"].Value == "new_id"))
                    {
                        element.ParentNode.RemoveChild(element);//remove just before adding.
                        tarElement.AppendChild(element);
                    }
                    else // add as sibling, otherwise
                    {
                        //string titlemessage = "Where would you like to drop " + ((XmlElement)_sourceItem.Header).Attributes["text"].Value + " in " + ((XmlElement)_targetItem.Header).Attributes["text"].Value;//24Jan2013
                        string sourceN = ((XmlElement)_sourceItem.Header).Attributes["text"].Value;//N for Node
                        string targetN = ((XmlElement)_targetItem.Header).Attributes["text"].Value;

                        //06Oct2013 No need to show dialog. Instead always add above (basically, target item is pushed down, new one
                        // takes its place. ).
                        //MenuAboveBelowDialog dragLocDialog = new MenuAboveBelowDialog(sourceN, targetN);//07Nov212

                        //dragLocDialog.Owner = this;
                        //dragLocDialog.ShowDialog();
                        //string choice = dragLocDialog.SelectedOption;

                        string choice = "Above";//06Oct2013
                        aboveBelow = choice;//06Feb2013 For setting location of new command relative to its sibling.
                        ///show dialog box asking where to drop source node.(above or below target)
                        //MessageBox.Show(this,"Move it above or below target?", "Above or Below", MessageBoxButton.YesNoCancel, MessageBoxImage.Asterisk, MessageBoxResult.OK, MessageBoxOptions.None);
                        if (choice.Equals("Below"))//below target sibling
                        {
                            element.ParentNode.RemoveChild(element);//remove just before adding.
                            tarElement.ParentNode.InsertAfter(element, tarElement);
                        }
                        else if (choice.Equals("Above"))//above target silbling
                        {
                            element.ParentNode.RemoveChild(element);//remove just before adding.
                            tarElement.ParentNode.InsertBefore(element, tarElement);

                        }
                        else
                            return;
                    }
                }
                ismodified = true;// 10Sep2014 flag for saving if commands are dragged to change location.
            }
            catch (Exception ex)
            {
                //logService.WriteToLogLevel("Error in adding child:", LogLevelEnum.Error, ex);
            }
        }


        #endregion

        #region Context Menu

        //Adds Category 
        private void addCategory_Click(object sender, RoutedEventArgs e)
        {
            AddCategoryDialog acd = new AddCategoryDialog();
            acd.Owner = this;
            acd.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner;
            acd.ShowDialog();
            string catname = acd.CategoryName;
            AddCategoryOrSeparator(false, catname); // if catname = "-sep-" it will not add separator coz other parameter is false. So we are safe.
            //switchToEditMode(null); its biringing clicked node to edit mode. But we need newly added category in edit mode.
        }

        //Adds separator as a next to (below) sibling wrt clicked node.
        private void addSeparator_Click(object sender, RoutedEventArgs e)
        {
            AddCategoryOrSeparator(true, "-sep-");
        }

        //Adds Category or separator. textbox_focuslost should be raised and should handle overwriting of command/category
        public void AddCategoryOrSeparator(bool isSeparator, string catname)
        {
            ismodified = true;
            if (catname == null)//cancel add opration
                return;
            bool isunique = false; //10Oct2013 Not Unique by default
            bool isoverwrite;
            //For separator <menu id="" text="separator">
            //bool isSeparator = false;
            //if (catname.Equals("-sep-"))
            //    isSeparator = true;

            string NewNodeName = isSeparator ? "---------" : catname; // New Node Name as entered in textbox

            //// Finding Parent Node ////
            TreeViewItem selectedItem = (TreeViewItem)tv.Tag;
            XmlElement selectedelementnode = null;
            if (selectedItem != null)
            {
                selectedelementnode = (selectedItem.Header as XmlElement);
            }
            ///// if nothing selected /////
            if (selectedelementnode == null)
            {
                MessageBox.Show(this, "Select a Root or user created category node to create new node under it.", "Info: Select proper node.");
                //isunique = false;
            }

            /// No addition allowed under standard menu items, and newcommand  //// 1feb2013
            //if (selectedelementnode.Attributes["owner"] != null &&
            //(selectedelementnode.Attributes["owner"].Value == "BSky" || selectedelementnode.Attributes["nodetype"].Value == "Leaf")
            //)
            //{
            //    MessageBox.Show("You cannot add under Standard items(in Gray), and under leaf nodes.", "Info: Select proper parent/category node.");
            //   // isunique = false;
            //}

            //// Here we can have logic to check duplicate menu item names /////
            //if (ownercommandList.Contains(NewNodeName))//Do not overwrite standard commands of an app
            //{
            //    MessageBox.Show("\'" + NewNodeName + "\' is standard menu item of the application. \n Please use different name.");
            //    //06Oct2013 Removed from UI txt.SelectAll();
            //    //txt.Text = "New Node";
            //    isunique = false;
            //}
            //else if (commandList.Contains(NewNodeName) || categorylist.Contains(NewNodeName))// check  duplicate user category/command names
            //{
            //    isunique = false;
            //    // Allow user to overwrite his custom commands/category. Right now we dont have overwrite logic here.
            //    MessageBox.Show("Please use unique name. \'" + NewNodeName + "\' already exists.");
            //    //06Oct2013 Removed from UI  txt.SelectAll();
            //    //txt.Text = "New Node";
            //}


            // check if category already exists in current location
            string msg = string.Empty;
            NodeClass nc = GetNodeClass(NewNodeName, selectedelementnode, false, "Parent");
            //bool isownersibling;
            //isunique = isUniqueSibling(NewNodeName, selectedelementnode, true, out isownersibling);// we can add another bool parameter for overwrite choice
            switch (nc)
            {
                case NodeClass.OWNERCATEGORY:
                    msg = "Standard (non- modifiable) Category with same name exists in current location! Can't Overwrite";
                    MessageBox.Show(this, NewNodeName + "\n" + msg, "Cannot Overwrite Standard Category.", MessageBoxButton.OK, MessageBoxImage.Error);
                    //isunique = false; //overwrite code is not yet present
                    break;
                case NodeClass.OWNERCOMMAND:
                    msg = "Standard (non- modifiable) Command with same name exists in current location! Can't Overwrite";
                    MessageBox.Show(this, NewNodeName + "\n" + msg, "Cannot Overwrite Standard Command", MessageBoxButton.OK, MessageBoxImage.Error);
                    //isunique = false; //overwrite code is not yet present

                    break;
                case NodeClass.USERCATEGORY:
                    msg = "Category with same name exists in current location! Everything under this category will be deleted.";
                    MessageBoxResult mbr = MessageBox.Show(this, "Overwrite? " + NewNodeName + "\n" + msg, "Confirm Category Overwrite", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                    if (mbr == MessageBoxResult.Yes)
                    {
                        MessageBox.Show(this, "Currently Overwrite logic is not implemented");
                        isoverwrite = true; //not Unique
                        //isunique = false; //overwrite code is not yet present
                    }
                    break;
                case NodeClass.USERCOMMAND:
                    msg = "Command with same name exists in current location!";
                    mbr = MessageBox.Show(this, "Overwrite? " + NewNodeName + "\n" + msg, "Confirm Command Overwrite", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                    if (mbr == MessageBoxResult.Yes)
                    {
                        MessageBox.Show(this, "Currently Overwrite logic is not implemented");
                        isoverwrite = true; //not Unique
                        //isunique = false; //overwrite code is not yet present
                    }


                    break;
                case NodeClass.NONE:
                    isunique = true;
                    break;
                default:
                    break;
            }

            //// Right now overwrite facility is not provided in following code////
            if (isunique) // if unique category name is given
            {
                ///Add Category if unique in current location ( under current parent category )
                XmlDataProvider dp = (XmlDataProvider)this.FindResource("xmlDP");
                XmlElement element = dp.Document.CreateElement("menu");

                XmlAttribute attrib = dp.Document.CreateAttribute("id");
                attrib.Value = isSeparator ? "sep" : "new_id";//+txt.Text == string.Empty ? "New Node" : txt.Text; //can append this
                element.Attributes.Append(attrib);

                attrib = dp.Document.CreateAttribute("text");
                attrib.Value = NewNodeName;//01Feb2013
                element.Attributes.Append(attrib);

                attrib = dp.Document.CreateAttribute("owner");
                attrib.Value = "";//01Feb2013
                element.Attributes.Append(attrib);

                attrib = dp.Document.CreateAttribute("nodetype");//06Mar2013 will be Parent always
                attrib.Value = isSeparator ? "Leaf" : "Parent";//01Feb2013
                element.Attributes.Append(attrib);


                ///// Adding new node or separator user the selected parent/////
                XmlElement selectedelement = selectedItem.Header as XmlElement;
                if (selectedelement == null)
                    return;
                if (isSeparator) // 06Oct2013 separator is added as a sibling (below) to a category.
                {
                    selectedelement.ParentNode.InsertAfter(element, selectedelement);
                }
                else
                    selectedelement.AppendChild(element); //04Oct2013 Category added under category

                //updating the command lists //01Feb2013
                CreateRefreshTextAttrList();

            }
        }

        private void addCommand_Click(object sender, RoutedEventArgs e)
        {
            object p = (sender as MenuItem).Parent;
            try
            {
                AddCommand("New Command Node");
            }
            catch (Exception ex)
            {
                MessageBox.Show(this,"Error adding command." + ex.StackTrace, "Error occurred!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Overwrite or Add new Command
        private void AddCommand(string text)
        {
            ismodified = true;
            bool success;
            XmlElement xe = tv.SelectedItem as XmlElement;
            string id = (xe.GetAttribute("id") != null) ? xe.GetAttribute("id") : string.Empty;
            string txt = (xe.GetAttribute("text") != null) ? xe.GetAttribute("text") : string.Empty;
            string nodetype = (xe.GetAttribute("nodetype") != null) ? xe.GetAttribute("nodetype") : string.Empty;
            string owner = (xe.GetAttribute("owner") != null) ? xe.GetAttribute("owner") : string.Empty;
            string NewCommLocXpath = "//menu[@id=\'" + id + "\']";

            this.NewCommandName = text;
            if (!owner.Equals("BSky"))
            {
                if (nodetype.Equals("Leaf"))//It means overwrite command
                {
                    newNode = null; //Not installing new node. Overwriting.
                    MessageBoxResult mbresult = MessageBox.Show(this, "Do you want to Overwrite " + txt + "?", "Overwrite Command?", MessageBoxButton.YesNo);
                    if (mbresult == MessageBoxResult.No)//No Overwrite
                        return;
                    success = AddNewOrOverwriteCommand(true); //Overwrite command
                    if (success)
                    {
                        //switchToEditMode(null, this.NewCommandName);
                        object sender = null;
                        editmode = true;
                        if (NodeToRename != null)//04Oct2013
                            sender = NodeToRename;

                        //Dialog installer: After renaming a leaf/dialog and then overwriting the dialog with new version
                        // it cause exception in following line.
                        //Did not try to reproduce so not sure yet if this is a real issue. 
                        TextBox txtb = (TextBox)((Grid)((TextBlock)sender).Parent).Children[2]; // [0] is image, [1] is textblock, [2] is textbox
                        ((TextBlock)sender).Tag = ((TextBlock)sender).Text;//05Mar2013 Store original value. Later you can see if it was modified or not
                        if (!txtb.IsReadOnly)
                        {
                            //((TextBlock)sender).Visibility = Visibility.Collapsed;
                            //txtb.Visibility = Visibility.Visible;
                            CommandnameBeforeRename = ((TextBlock)sender).Text; //03Oct2013
                            txtb.Text = string.IsNullOrEmpty(this.NewCommandName) ? ((TextBlock)sender).Text : this.NewCommandName;
                            ((TextBlock)sender).Text = string.IsNullOrEmpty(this.NewCommandName) ? ((TextBlock)sender).Text : this.NewCommandName;
                            //// Focus on text box ////
                            //Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, (System.Threading.ThreadStart)delegate()
                            //{
                            //    txtb.SelectAll();
                            //    txtb.Focus();
                            //});
                        }
                    }


                    ////now get the Name of leaf and use it as new commands name. backup old XAML/XML for undo
                    ////get the path (parent node) under which you will install new command
                    //XmlElement xep = xe.ParentNode as XmlElement;
                    //string pid = (xep.GetAttribute("id") != null) ? xep.GetAttribute("id") : string.Empty;
                    //NewCommLocXpath = "//menu[@id=\'" + pid + "\']";
                    //this.NewCommandName = txt;
                    //if (OpenCommandDialogFile()) // if dialog file is valid then proceed
                    //{
                    //    // backup old XAML XML with  same name for undo
                    //    // copy new dialog files from temp to config (renaming them as command name following naming convention)
                    //    // make changes in menu.xml. Edit exiting command's commandoutputformat, commandtemplate
                    //    RenameCommandText(xe, txt, txt, true);
                    //    AddNewOrOverwriteCommand(NewCommLocXpath, true);
                    //    //InstallNewCommand(NewCommLocXpath);
                    //}

                }
                else
                {
                    success = AddNewOrOverwriteCommand(false); // Add new Command
                }

            }
            else
            {
                MessageBox.Show(this, "You cannot install under statndard menu items (in Gray)");
            }

            this.Activate();
            CleanTempFiles();
        }

        private void CleanTempFiles()
        {
            //MessageBox.Show("Cleanup Started...");
            if (extractedfiles != null && extractedfiles.Count > 0)
            {
                foreach (string fulpathfilename in extractedfiles)
                {
                    //MessageBox.Show("Cleaning : " + fulpathfilename);
                    if (System.IO.File.Exists(fulpathfilename))
                    {
                        System.IO.File.Delete(fulpathfilename);
                        //MessageBox.Show("Cleaned : " + fulpathfilename);
                    }
                }
            }
        }
        private bool AddNewOrOverwriteCommand(bool isOverwriteCommand)
        {
            ismodified = true;
            bool success = false;
            string commandname;
            bool isunique = true;
            bool overwriteUsernode;//while installing or overwriting a command, this var hold the user choice about overwriting old command with same name in current location
            if (installCommandFiles(out commandname, out overwriteUsernode))// (OpenCommandDialogFile()) // if dialog file is valid then proceed
            {
                this.NewCommandName = commandname;//commandnaame from dialog xaml "Command" string
                //if (ownercommandList.Contains(commandname))//Do not overwrite standard commands of an app
                //{
                //    MessageBox.Show("\'" + commandname + "\' is the standard menu item. \n Please use different name. ", "Cannot Overwrite Standard Item.", MessageBoxButton.OK, MessageBoxImage.Error);
                //    //06Oct2013 Removed from UI txt.SelectAll();
                //    //txt.Text = "New Node";
                //    isunique = false;
                //}

                // following does not make sense as we already confirmed from user in deployXmlXaml()
                //else if (commandList.Contains(commandname) || categorylist.Contains(commandname))// check  duplicate user category/command names
                //{
                //    isunique = false;
                //    // Allow user to overwrite his custom commands/category. Right now we dont have overwrite logic here.
                //    MessageBox.Show("Please use unique name. \'" + commandname + "\' already exists.");
                //    //06Oct2013 Removed from UI  txt.SelectAll();
                //    //txt.Text = "New Node";
                //}

                if (isunique) // if unique category name is given
                {
                    TreeViewItem selectedItem = (TreeViewItem)tv.Tag;
                    XmlDataProvider dp = (XmlDataProvider)this.FindResource("xmlDP");

                    // Overwrite the menu command if user already confirmed(yes) in deployXmlXaml(). 
                    // No matter menu command name matches to new command name, because xaml file is already overwritten
                    // so existing menu command whatever name it has, if it  was pointing to same XAML file
                    // that current new command is pointing, now, then you can remove the old entry from menu.xml.
                    // Otherwise there will be two commands in menu pointing to same dialog.
                    // There are two things, the unique dialog name and the non-unique menu command name. We should
                    // remove old menu name pointing to same xaml, making one to one relationship between 
                    // menu command name and xaml filename.

                    /// Remove old menu command name entry (if exists in menu.xml), if it points to same .XAML ///
                    //23Apr2015 string commtemplate = Path.Combine(@".\Config\" + commandname + ".xaml");

                    //17Jun2018 string commtemplate = Path.Combine(string.Format(@"{0}", BSkyAppDir.RoamingUserBSkyConfigL18nPath) + commandname + ".xaml");//23Apr2015
                    string commtemplate = commandname.Replace('"','\'') + ".xaml";//17Jun2018

                    XmlNodeList oldxn = dp.Document.SelectNodes("//menus//menu[@commandtemplate=\"" + commtemplate + "\"]");
                    if (oldxn != null && !isOverwriteCommand)//if found, remove those nodes from menu.xml. AND not overwrite command
                    {

                        for (int i = 0; i < oldxn.Count; i++)
                        {
                            oldxn[i].ParentNode.RemoveChild(oldxn[i]);
                        }
                    }

                    ////// if old command with same name exists, remove it ///
                    if (overwriteUsernode && !isOverwriteCommand) //AND not overwrite command
                    {
                        XmlNode xnparent = selectedItem.Header as XmlNode; //get existing entry
                        //oldxn = dp.Document.SelectNodes("//menus//menu[@text=\'" + commandname + "\']");
                        oldxn = xnparent.SelectNodes("//menu[@text=\'" + commandname + "\' and @owner!=\'BSky\']");
                        if (oldxn != null)//if found, remove those nodes from menu.xml
                        {

                            for (int i = 0; i < oldxn.Count; i++)
                            {
                                //we can also check at this point(instead of above) if the owner = BSky or not before deleting. 
                                //BSky node must not be deleted.
                                oldxn[i].ParentNode.RemoveChild(oldxn[i]);
                            }
                        }
                    }

                    /// Add New Entry or modify entry in menu.xml ///

                    XmlElement element; // holds new node or existing
                    //right-click on command node and 'overwrite command'
                    if (isOverwriteCommand) //only replace menu.xml entry in same location as clicked command node
                    {
                        element = selectedItem.Header as XmlElement; //get existing entry
                        //if (element == null)
                        //{
                        //    element = dp.Document.CreateElement("menu"); // create new entry
                        //    selectedItem.Header = element;
                        //    tv.Tag = selectedItem;
                        //}
                        if(element!=null)
                            element.RemoveAllAttributes(); // remove all attributes and create new later with new values
                    }
                    else
                    {
                        element = dp.Document.CreateElement("menu"); // create new entry
                    }
                    XmlAttribute attrib = dp.Document.CreateAttribute("id");
                    attrib.Value = commandname.Replace(' ', '_');//+txt.Text == string.Empty ? "New Node" : txt.Text; //can append this
                    element.Attributes.Append(attrib);

                    attrib = dp.Document.CreateAttribute("text");
                    attrib.Value = commandname;//01Feb2013
                    element.Attributes.Append(attrib);

                    attrib = dp.Document.CreateAttribute("commandtemplate");
                    //23Apr2015 attrib.Value = Path.Combine(@".\Config\" + commandname + ".xaml");
                    attrib.Value = commandname + ".xaml";//17Jun2018  //Path.Combine(string.Format(@"{0}", BSkyAppDir.RoamingUserBSkyConfigL18nPath) + commandname + ".xaml");
                    element.Attributes.Append(attrib);

                    attrib = dp.Document.CreateAttribute("commandoutputformat");
                    //23Apr2015 attrib.Value = Path.Combine(@".\Config\" + commandname + ".xml");
                    attrib.Value = commandname + ".xml";//17Jun2018  //Path.Combine(string.Format(@"{0}", BSkyAppDir.RoamingUserBSkyConfigL18nPath) + commandname + ".xml");//23Apr2015
                    element.Attributes.Append(attrib);

                    attrib = dp.Document.CreateAttribute("owner");
                    attrib.Value = "";
                    element.Attributes.Append(attrib);

                    attrib = dp.Document.CreateAttribute("nodetype");//06Mar2013 will be Parent always
                    attrib.Value = "Leaf";//01Feb2013
                    element.Attributes.Append(attrib);

                    //For overwriting nothing needs to be done. Attribute values are already changed. Thats it.

                    if (!isOverwriteCommand)  // right-click on Category node and 'add new command'
                    { // Add new command in menu.xml

                        ///// Adding new node under the selected parent/////
                        XmlElement selectedelement = selectedItem.Header as XmlElement;
                        if (selectedelement == null)
                            return success;
                        selectedelement.AppendChild(element); //04Oct2013 Category added under category
                    }
                    success = true;
                    //updating the command lists //01Feb2013
                    CreateRefreshTextAttrList();
                }
                else // name is not unique. Revert back. You should rename back command xaml/xml
                {
                    //write revert logic.
                }

            }

            return success;
        }

        //removes category or command node
        private void removenode_Click(object sender, RoutedEventArgs e)
        {
            TreeViewItem selectedItem = (TreeViewItem)tv.Tag;
            XmlElement selectedelement = selectedItem.Header as XmlElement;
            RemoveSelectedNode(selectedelement);
            ismodified = true;
        }

        //Removes Selected treeviewitem/node
        private void RemoveSelectedNode(XmlElement selectedelement)
        {

            if (selectedelement != null)
            {
                ///restrict removal of BSky standard menu items  // 31Jan2013 and newcommand and Root too ///
                if (selectedelement.Attributes["owner"] != null &&
                (selectedelement.Attributes["owner"].Value == "BSky" || selectedelement.Attributes["owner"].Value == "newcommand")
                || selectedelement.Name == "menus")
                {
                    MessageBox.Show(this, "You cannot remove Standard (non- modifiable) items(in Gray) and Root(in Black).", "Info: Select proper node.");
                    return;
                }

                if (true)
                {
                    string nodetoremove = selectedelement.Attributes["text"] != null ? selectedelement.Attributes["text"].Value : string.Empty;
                    string catmsg = selectedelement.HasChildNodes ? "All sub-catagories and commands under this category will also be removed." : string.Empty;
                    MessageBoxResult mbr = MessageBox.Show(this, "Remove \'" + nodetoremove + "\'? \n" + catmsg, "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                    if (mbr == MessageBoxResult.No)
                    {
                        return; // do not remove;
                    }
                }
                ////  for undo  ////
                parentofremoved = selectedelement.ParentNode;
                presib = selectedelement.PreviousSibling;
                nxtsib = selectedelement.NextSibling;
                /// Remove node ///
                removed = selectedelement.ParentNode.RemoveChild(selectedelement);

            }
            else
            {
                //logService.WriteToLogLevel("Cannot remove item, selected item is null", LogLevelEnum.Warn);
            }

            //updating the command lists //01Feb2013
            CreateRefreshTextAttrList();
            ismodified = true;
        }


        //this is not a part of context menu but in future it can be
        private void cmdUndo_Click(object sender, RoutedEventArgs e)
        {
            if (parentofremoved != null && removed != null)
                UndoLast();
            ismodified = true;
        }

        private void UndoLast()//for undo
        {
            if (presib == null)//add as a first child
                parentofremoved.PrependChild(removed);
            else if (nxtsib == null)
                parentofremoved.AppendChild(removed);
            else
            {
                parentofremoved.InsertAfter(removed, presib);
                //parentofremoved.InsertBefore( removed, nxtsib);
            }
            removed = null; presib = null; nxtsib = null; parentofremoved = null; //resetting global vars
            ismodified = true;
        }

        private void renamenode_Click(object sender, RoutedEventArgs e)
        {
            switchToEditMode(sender, string.Empty);

        }

        //to rename tree nodes ( category or command ). Double clicked or right click > rename. Opens edit mode ( textbox ).
        private void switchToEditMode(object sender, string textboxvalue)
        {
            ismodified = true;
            editmode = true;
            if (NodeToRename != null)//04Oct2013
                sender = NodeToRename;

            TextBox txtb = (TextBox)((Grid)((TextBlock)sender).Parent).Children[2]; // [0] is image, [1] is textblock, [2] is textbox
            ((TextBlock)sender).Tag = ((TextBlock)sender).Text;//05Mar2013 Store original value. Later you can see if it was modified or not
            if (!txtb.IsReadOnly)
            {
                ((TextBlock)sender).Visibility = Visibility.Collapsed;
                txtb.Visibility = Visibility.Visible;
                CommandnameBeforeRename = ((TextBlock)sender).Text; //03Oct2013
                txtb.Text = string.IsNullOrEmpty(textboxvalue) ? ((TextBlock)sender).Text : textboxvalue;

                //// Focus on text box ////
                Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, (System.Threading.ThreadStart)delegate()
                {
                    txtb.SelectAll();
                    txtb.Focus();
                });
            }
        }

        private void nodeproperties_Click(object sender, RoutedEventArgs e)
        {

            //// Finding Parent Node ////
            TreeViewItem selectedItem = (TreeViewItem)tv.Tag;
            XmlElement selectedelementnode = null;
            if (selectedItem != null)
            {
                selectedelementnode = (selectedItem.Header as XmlElement);
            }

            string id = (selectedelementnode.Attributes["id"] != null) ? selectedelementnode.Attributes["id"].Value : string.Empty;
            string inmenu = (selectedelementnode.Attributes["text"] != null) ? selectedelementnode.Attributes["text"].Value : string.Empty;
            string comm_template = (selectedelementnode.Attributes["commandtemplate"] != null) ? selectedelementnode.Attributes["commandtemplate"].Value : string.Empty;
            string comm_format = (selectedelementnode.Attributes["commandformat"] != null) ? selectedelementnode.Attributes["commandformat"].Value : string.Empty;
            string comm_output_format = (selectedelementnode.Attributes["commandoutputformat"] != null) ? selectedelementnode.Attributes["commandoutputformat"].Value : string.Empty;
            string owner = (selectedelementnode.Attributes["owner"] != null) ? selectedelementnode.Attributes["owner"].Value : string.Empty;
            string nodetype = (selectedelementnode.Attributes["nodetype"] != null) ? selectedelementnode.Attributes["nodetype"].Value : string.Empty;

            MessageBox.Show(this,
                "ID         :  " + id + "\n" +
                "In Menu    :  " + inmenu + "\n" +
                "XAML File  :  " + comm_template + "\n" +
                "R Function :  " + comm_format + "\n" +
                "XML File   :  " + comm_output_format + "\n" +
                "Owner      :  " + owner + "\n" +
                "Node Type  :  " + nodetype,
                "Node Info:", MessageBoxButton.OK, MessageBoxImage.Information
                );
        }

        //checks for duplicates in currently selected node.
        //Returns the node class: NONE means no duplicate. others means duplicate and its type is returned
        //here nodetype is the node that is to be added (Parent or Leaf) in current location.
        // and two leaf nodes ( or two parent nodes) in current location can't have same name
        // but a leaf can have same name as category node in same location or vice versa.
        private NodeClass GetNodeClass(string NewNodeName, XmlElement selectedelementnode, bool showmessage, string nodetype)
        {
            NodeClass nc = NodeClass.NONE;//assuming: node name is unique in current category and doesn't match with any sibling.
            // check if category already exists in current location
            if (selectedelementnode != null)
            {
                if (selectedelementnode.HasChildNodes)
                {
                    XmlNodeList oldxn = selectedelementnode.ChildNodes;
                    if (oldxn != null)//if found, check if "text" attribute matches to the new category that we want to add
                    {
                        for (int i = 0; i < oldxn.Count; i++)
                        {
                            if ((((TreeViewItem)tv.Tag).Header as XmlNode).Equals(oldxn[i]))//for rename, ignore the renamed node
                                continue;

                            if (oldxn[i].Attributes["text"] != null && oldxn[i].Attributes["text"].Value.Equals(NewNodeName))
                            {
                                if (oldxn[i].Attributes["owner"] != null && oldxn[i].Attributes["owner"].Value.Equals("BSky")) //standard item
                                {
                                    if (oldxn[i].Attributes["nodetype"] != null && oldxn[i].Attributes["nodetype"].Value.Equals("Parent"))
                                        nc = NodeClass.OWNERCATEGORY;
                                    else
                                        nc = NodeClass.OWNERCOMMAND;
                                    if (showmessage)
                                        MessageBox.Show(this, "\'" + NewNodeName + "\' already exists in this category and is an standard (non- modifiable) item. \n Please use different name.", "Duplicate", MessageBoxButton.OK, MessageBoxImage.Error);
                                }
                                else
                                {
                                    if (oldxn[i].Attributes["nodetype"] != null && oldxn[i].Attributes["nodetype"].Value.Equals("Parent"))
                                        nc = NodeClass.USERCATEGORY;
                                    else
                                        nc = NodeClass.USERCOMMAND;
                                    if (showmessage)
                                        MessageBox.Show(this, "\'" + NewNodeName + "\' already exists in this category. \n Please use different name.", "Duplicate", MessageBoxButton.OK, MessageBoxImage.Error);
                                }

                                //17Oct2013 user can have command name = category name in same location
                                // but can't have 2 commands or 2 categories with same name
                                if ((nodetype.Trim().Equals("Parent") && (nc == NodeClass.OWNERCATEGORY || nc == NodeClass.USERCATEGORY)) ||
                                    (nodetype.Trim().Equals("Leaf") && (nc == NodeClass.OWNERCOMMAND || nc == NodeClass.USERCOMMAND)))
                                    break;

                                nc = NodeClass.NONE; // for resetting it back each time.
                            }
                        }
                    }
                }
            }
            return nc;
        }

        #endregion

        #region operations related to XAML/XML files in config

        // Install command = extract .bsky in windows temp + copy from temp to Config ( giving them name as command name ).
        private bool installCommandFiles(out string commandname, out bool overwriteexisting)
        {
            //string commandname;
            bool extracted = extractDialogZip(out commandname);
            bool deployedsuccessfully = false;
            overwriteexisting = false;
            bool noduplicate = false;
            if (extracted)
            {
                // you can write some logic to find out if XAML/XML already exists in "Config" and
                // then you can get confirmation from user whether to overwrite or not
                //MessageBoxResult mbr = MessageBox.Show("Remove \'" + commandname + ".xaml and xml\'? \n", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                //if (mbr == MessageBoxResult.No)
                //{
                //    return deployedsuccessfully; // do not overwrite xaml xml;
                //}

                // backupXmlXaml(@".\Config\" +commandname+".xaml"); use this for performing backup. But I think this is not needed

                string msg = string.Empty;
                //bool isownersibling;
                //bool isunique = isUniqueSibling(NewCommandName, (((((TreeViewItem)tv.Tag).Header as XmlElement).ParentNode) as XmlElement), false, out isownersibling);
                XmlElement xe = null;
                XmlNode xnclicked = ((TreeViewItem)tv.Tag).Header as XmlNode;
                if (xnclicked.Attributes["nodetype"] != null && xnclicked.Attributes["nodetype"].Value.Equals("Leaf"))
                    xe = (((((TreeViewItem)tv.Tag).Header as XmlElement).ParentNode) as XmlElement); // Overwrite by rt.clk on leaf
                else
                    xe = (((TreeViewItem)tv.Tag).Header as XmlElement);//Install new by rt. clk. on parent node

                NodeClass nc = GetNodeClass(commandname, xe, false, "Leaf");
                MessageBoxResult mbr;
                switch (nc)
                {
                    case NodeClass.OWNERCATEGORY:
                        msg = "Please rename your command/category.\nStandard (non- modifiable) Category with same name already exists in current location! Can't Overwrite";
                        MessageBox.Show(this, commandname + "\n" + msg, "Cannot Overwrite Standard Category.", MessageBoxButton.OK, MessageBoxImage.Error);

                        break;
                    case NodeClass.OWNERCOMMAND:
                        msg = "Please rename your command/category.\nStandard (non- modifiable) Command with same name exists in current location! Can't Overwrite";
                        MessageBox.Show(this, commandname + "\n" + msg, "Cannot Overwrite Standard Command", MessageBoxButton.OK, MessageBoxImage.Error);
                        break;
                    case NodeClass.USERCATEGORY:
                        msg = "Category with same name exists in current location! Everything under this category will be deleted.";
                        mbr = MessageBox.Show(this, "Overwrite? " + commandname + "\n" + msg, "Confirm Category Overwrite", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                        if (mbr == MessageBoxResult.Yes)
                        {
                            overwriteexisting = true;
                            //remove old duplicate from menu.xml, in AddNewOrOverwriteCommand()
                        }

                        break;
                    case NodeClass.USERCOMMAND:
                        msg = "Command with same name exists in current location!\n You can rename your dialog .bsky file, if you dont want to overwrite and then retry.\n";
                        mbr = MessageBox.Show(this, msg + "Overwrite? " + commandname, "Confirm Command Overwrite", MessageBoxButton.OKCancel, MessageBoxImage.Warning);
                        if (mbr == MessageBoxResult.OK)
                        {
                            overwriteexisting = true;
                            //remove old duplicate from menu.xml in AddNewOrOverwriteCommand()
                        }
                        break;
                    case NodeClass.NONE:
                        noduplicate = true;
                        break;
                    default:
                        break;
                }

                if (overwriteexisting || noduplicate)
                    deployedsuccessfully = deployXmlXaml(commandname, overwriteexisting); // true -> overwrite
            }
            return deployedsuccessfully;
        }

        //Extract XAML/XML from .bsky (dialog zip file) in windows "temp" folder.
        //you can find Command from .XAML file and use that string to rename your extracted files (xml/xaml from .bsky)
        // when you copy them to "Config". This function will set commandname that can used from where this function is called.
        //Basically "Config" will contain files with name as Command.xml and Command.xaml
        private bool extractDialogZip(out string commandname)
        {
            bool validDialogFile = false;//07Mar2013 If Dialog zip file(containing XAML and XML) is not valid then skip further processing
            IUnityContainer container = LifetimeService.Instance.Container;
            IDashBoardService dashboardService = container.Resolve<IDashBoardService>();

            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = FileNameFilter;
            bool? output = openFileDialog.ShowDialog(this); //(Application.Current.MainWindow);
            commandname = string.Empty;
            if (output.HasValue && output.Value)
            {
                extractedfiles = new List<string>();//08May2015
                FileStream fileStreamIn = new FileStream(openFileDialog.FileName, FileMode.Open, FileAccess.Read);
                ZipInputStream zipInStream = new ZipInputStream(fileStreamIn);

                ZipEntry entry = zipInStream.GetNextEntry();
                string tempDir = Path.GetTempPath();

                //Extract the files
                while (entry != null)
                {
                    //Added by Aaron 04/21/2013
                    //commented line below
                    //FileStream fileStreamOut = new FileStream(Path.Combine(tempDir, entry.Name), FileMode.Create, FileAccess.Write);
                    FileStream fileStreamOut = new FileStream(Path.Combine(tempDir, Path.GetFileName(entry.Name)), FileMode.Create, FileAccess.Write);
                    if (entry.Name.EndsWith(".xaml")) XamlFile = entry.Name;
                    else if (entry.Name.EndsWith(".xml")) XmlFile = entry.Name;
                    //Added by Aaron 01/19/2014
                    else helpfilenames.Add(entry.Name);

                    int size;
                    byte[] buffer = new byte[1024];
                    do
                    {
                        size = zipInStream.Read(buffer, 0, buffer.Length);
                        fileStreamOut.Write(buffer, 0, size);
                    } while (size > 0);
                    fileStreamOut.Close();

                    //MessageBox.Show("Extracted:" + entry.Name + " in " + tempDir);
                    //adding fullpath filename to list //08May2015
                    extractedfiles.Add(Path.Combine(tempDir, entry.Name));

                    entry = zipInStream.GetNextEntry();
                }
                zipInStream.Close();
                fileStreamIn.Close();
                //MessageBox.Show("Files Extracted : "+extractedfiles.Count);
                //01Oct2013 XML output mandatory condition removed
                //if (string.IsNullOrEmpty(XmlFile))
                //{
                //    MessageBox.Show("Dialog cannot be installed as output file is not mentioned");
                //    return validDialogFile;
                //}
                //Load the dialog object, check the location and modify menu file.
                if (!string.IsNullOrEmpty(XamlFile)) //01Oct2013  XML output mandatory condition removed  && !string.IsNullOrEmpty(XmlFile))
                {
                    //04/21/2013
                    //Commented the line below and added the line under it

                    // FileStream stream = System.IO.File.Open(Path.Combine(tempDir, XamlFile), FileMode.Open);
                    FileStream stream = System.IO.File.Open(Path.Combine(tempDir, Path.GetFileName(XamlFile)), FileMode.Open);
                    try
                    {
                        //Added by Aaron 06/18/2014
                        //Code below prevents the triggering of the applybehaviours
                        BSkyCanvas.applyBehaviors = false;
                        object obj = XamlReader.Load(stream);
                        BSky.Controls.BSkyCanvas canvas = obj as BSky.Controls.BSkyCanvas;

                        string menulocation = canvas.MenuLocation;
                        //string title = canvas.Title;
                        string title = canvas.Title != null && canvas.Title.Length > 0 ? canvas.Title : "No Title";
                        //following commented line will fail in case of batch commands
                        //commandname = canvas.CommandString.Substring(0,canvas.CommandString.IndexOf('('));//09Oct2013 Command string will be used as xaml/xml filename
                        commandname = Path.GetFileNameWithoutExtension(openFileDialog.FileName); // getting first filename from .bsky name
                        if (string.IsNullOrEmpty(canvas.Title) || string.IsNullOrEmpty(commandname))
                        {
                            title = Microsoft.VisualBasic.Interaction.InputBox("Get Item Name", "Item Name", "New Node");
                            if (!string.IsNullOrEmpty(title))
                            {
                                MessageBox.Show(this, "Title/Command cannot be empty, Exiting Dialog install", "Info: Dialog Title Empty.");
                                return validDialogFile;
                            }
                        }
                        //InstallNewCommand(NewCommLocXpath);
                        validDialogFile = true;
						stream.Close();
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show(this, "Couldn't install Dialog: "+e.Message);
						stream.Close();
                        //logService.WriteToLogLevel("Couldn't install Dialog", LogLevelEnum.Error);
                    }
                }
            }
            return validDialogFile;
        }

        //copy xaml/xml files from windows temp folder to Config
        //Added by Aaron 1/23/2014 This handles help files as well
        private bool deployXmlXaml(string Commandname, bool overwrite)
        {
            string tempDir = Path.GetTempPath();
            bool deploysuccess = false;

            FileStream stream = System.IO.File.Open(Path.Combine(tempDir, Path.GetFileName(XamlFile)), FileMode.Open);
            //Added by Aaron 06/18/2014
            //Code below prevents the triggering of the applybehaviours
            BSkyCanvas.applyBehaviors = false;
            object obj = XamlReader.Load(stream);
			stream.Close();
            BSky.Controls.BSkyCanvas canvas = obj as BSky.Controls.BSkyCanvas;
            List<helpfileset> lst = new List<helpfileset>();

            //Aaron 01/23/2014
           // Use this to get the original help file names and the name that will be used when installing these files to the config directory (The name of the command followed by the prefix
            lst = canvas.gethelpfilenames(canvas);

            try
            {
                ///output template filename will be same as dialog filename. No matter whats in zip file.
                //23Apr2015 string xmlinstallpathfilename = Path.GetFullPath(@".\Config\" + Commandname + ".xml");
                string xmlinstallpathfilename = Path.GetFullPath(string.Format(@"{0}", BSkyAppDir.RoamingUserBSkyConfigL18nPath) + Commandname + ".xml");//23Apr2015 

                //23Apr2015 string xamlinstallpathfilename = Path.GetFullPath(@".\Config\" + Commandname + ".xaml");
                string xamlinstallpathfilename = Path.GetFullPath(string.Format(@"{0}", BSkyAppDir.RoamingUserBSkyConfigL18nPath) + Commandname + ".xaml");//23Apr2015 
               // string xamlinstallpathfilename = Path.GetFullPath(@".\Config\" + Commandname + ".xaml");

                //Deployinmg XAML to config
                if (File.Exists(xamlinstallpathfilename)) // XAML file already exists. Ask for user confirmation
                {
                    if (!overwrite)//if already not confirmed by user to overwrite, the ask for confirmation.
                    {
                       //Added by Aaron 1/21/2014
                        //Changed message to include output definition and help files
                        MessageBoxResult mbresult = MessageBox.Show(this, Commandname + ".xaml, already exists! \n If you don't want to save the existing dialog definition, associated output definition files and help files rename your dialog (.bsky) filename and reinstall.\n Do you want to overwrite it?", "Overwrite XAML?", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                        if (mbresult == MessageBoxResult.No)//No Overwrite
                            return deploysuccess;
                        else
                        {
                            backupXmlXaml(xamlinstallpathfilename);//not deleting existing, but renaming them.
                            overwrite = true;
                        }
                    }
                    else // No need to confirm again, calling function already confirmed to overwrite
                    {
                        backupXmlXaml(xamlinstallpathfilename);//not deleting existing, but renaming them.
                    }
                }

                //Aaron 01/22/2014
                //Commented below
                //System.IO.File.Copy(Path.Combine(tempDir, Path.GetFileName(XamlFile)), xamlinstallpathfilename, overwrite);
                //Added line below
                System.IO.File.Copy(Path.Combine(tempDir, Path.GetFileName(XamlFile)), xamlinstallpathfilename, overwrite);

                //Deployinmg XML to config
                if (XmlFile != null && XmlFile.Trim().Length > 0)
                {
                    //Reason: Installing new command was not extracting the XML template in config. But when 
                    //next time you try to overwrite same command that time it was putting XML in config.
                    //commented by Anil 10Sep2014 if (overwrite)
                    {
                        if (File.Exists(xmlinstallpathfilename))
                        {
                            backupXmlXaml(xmlinstallpathfilename);
                            System.IO.File.Copy(Path.Combine(tempDir, Path.GetFileName(XmlFile)), xmlinstallpathfilename, overwrite);
                        }
                        //Added bby Aaron
                        //If file does not exist overwrite
                        //01Jun2015 Added if to the following else
                        else if(File.Exists(Path.Combine(tempDir, Path.GetFileName(XmlFile))))
                            System.IO.File.Copy(Path.Combine(tempDir, Path.GetFileName(XmlFile)), xmlinstallpathfilename, overwrite);
                    }
                }

                
                //Added by Aaron 1/23/2014
                //Handling for copying and backing up help files
                //Help files are more complex as I may have several help files associated with a single canvas

                //if (lst!=null && lst.Count > 0)
                {
                    //Deployinmg help to config
                    foreach (helpfileset hs in lst)
                    {
                        if (hs.newhelpfilepath != "urlOrUri")
                        {
                            //23Apr2015 string hlpinstallpathfilename = Path.GetFullPath(@".\Config\" + hs.newhelpfilepath);
                            string hlpinstallpathfilename = Path.GetFullPath(string.Format(@"{0}", BSkyAppDir.RoamingUserBSkyConfigL18nPath) + hs.newhelpfilepath);//23Apr2015 

                            if (overwrite)
                            {
                                //All help files even ifthere is only one, have a prefix to theit name. So if dialog is test.xaml, the help file will be
                                //test1.doc
                                if (File.Exists(hlpinstallpathfilename))
                                {
                                    backupXmlXaml(hlpinstallpathfilename);
                                    System.IO.File.Copy(Path.Combine(tempDir, Path.GetFileName(hs.originalhelpfilepath)), hlpinstallpathfilename, overwrite);
                                }
                                //If a help file with the same name does not exist overwrite
                                else System.IO.File.Copy(Path.Combine(tempDir, Path.GetFileName(hs.originalhelpfilepath)), hlpinstallpathfilename, overwrite);
                            }
                        }
                    }
                }
                deploysuccess = true;
            }
            catch (Exception e)
            {
                MessageBox.Show(this, "Error! Unable to deploy XAML/XML files to Config ", "Error Deploying", MessageBoxButton.OK, MessageBoxImage.Error);
                
            }
            return deploysuccess;
        }

        // Rename XAML/XML those reside in Config
        // xml and xaml files will have same first name so just one parameter is enough here.
        private void backupXmlXaml(string fullpathfilename)
        {
            string filenamewithoutextension = Path.GetFileNameWithoutExtension(fullpathfilename);
            string path = Path.GetDirectoryName(fullpathfilename);

            string xmlfullpathfilename = Path.Combine(path, filenamewithoutextension + ".xml");
            string xamlfullpathfilename = Path.Combine(path, filenamewithoutextension + ".xaml");

            bool xmlexists = File.Exists(xmlfullpathfilename);
            bool xamlexists = File.Exists(xamlfullpathfilename);
            bool helpexists =File.Exists(fullpathfilename);

            string extension = Path.GetExtension(fullpathfilename);

            if (extension != ".xml" && extension != ".xaml")
            {
                for (int i = 0; i < 10; i++)
                {
                    //create a backup filename ( full path but no extension )
                    string bakfilename = Path.Combine(path, filenamewithoutextension + i.ToString());

                    //check if there is already a backed up file from before. 
                    //if 1-8 already exists, 9th will be overwritten for rest of the backups done, without creating more backup files.
                    if (File.Exists(bakfilename + extension) && i < 9)
                        continue;
                    else //if free 'i' is found or else use last index
                    {
                        //if i=9 then delete those files first
                        if (i == 9)
                        {
                            File.Delete(bakfilename + extension);
                            // File.Delete(bakfilename + ".xaml");
                        }

                        //rename XAML XML files to the generated XAML/XML backup filename generated above.
                        if (helpexists) // if xml file was installed with dialog, exists then rename that too. XAML should always exist.
                        {
                            File.Move(fullpathfilename, bakfilename + extension);
                            break;
                        }

                    }
                }
            }




            // run a counter for backup file naming. Look for the available 'i' suffix thats free.
            // eg. i=4 may be free because filename1, filename2, filename3 already exists because of backup performed before.
            // suffix numeric in above example is actually value of 'i'
            for (int i = 0; i < 10; i++)
            {
                //create a backup filename ( full path but no extension )
                string bakfilename = Path.Combine(path, filenamewithoutextension + i.ToString());

                //check if there is already a backed up file from before. 
                //if 1-8 already exists, 9th will be overwritten for rest of the backups done, without creating more backup files.
                if (File.Exists(bakfilename + ".xaml") && i < 9)
                    continue;
                else //if free 'i' is found or else use last index
                {
                    //if i=9 then delete those files first
                    if (i == 9)
                    {
                        File.Delete(bakfilename + ".xml");
                        File.Delete(bakfilename + ".xaml");
                    }

                    //rename XAML XML files to the generated XAML/XML backup filename generated above.
                    if (xmlexists) // if xml file was installed with dialog, exists then rename that too. XAML should always exist.
                    {
                        File.Move(xmlfullpathfilename, bakfilename + ".xml");
                    }
                    if (xamlexists)
                    {
                        File.Move(xamlfullpathfilename, bakfilename + ".xaml");
                        break;
                    }
                }
            }
        }

        //removes xml and xaml files in config
        private void removeXmlXaml(string fullpathfilename)
        {
            //we do not delete xaml/xml files. 
            //Instead we backup max upto 10 files with same name with index suffixed in backup filename.
            backupXmlXaml(fullpathfilename);
        }

        #endregion

        #region Button click events

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            if (ismodified)
            {
                MessageBox.Show(this, "Changes will take effect after restarting the application!", "Info: Dialog installed.");
                if (newNode != null && newNode.ParentNode != null)
                    newNode.ParentNode.RemoveChild(newNode);//Remove place holder for new command
                //23Apr2015 SaveXml(@"./Config/Menu.xml");//save current state of treeview to XML
                //SaveXml(string.Format(@"{0}Menu.xml", BSkyAppDir.BSkyAppDirConfigPath));//23Apr2015

                //string CultureName = System.Threading.Thread.CurrentThread.CurrentCulture.Name;//02Oct2017
                SaveXml(string.Format(@"{0}menu.xml", BSkyAppDir.RoamingUserBSkyConfigL18nPath));// + CultureName + "/"));//02Oct2017
            }
            this.DialogResult = true;
            this.Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void cmdLoadXml_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //Use the Win32 OpenFileDialog to allow the user to pick a file ...
                Microsoft.Win32.OpenFileDialog ofd = new Microsoft.Win32.OpenFileDialog();
                ofd.DefaultExt = ".xml";
                ofd.Filter = "XML Documents (*.xml)|*.xml|All Files (*.*)|*.*";
                Nullable<bool> fUserPickedFile = ofd.ShowDialog(this);
                if (fUserPickedFile == true)
                {
                    //Create a new XDoc ...
                    XmlDocument doc = new XmlDocument();
                    //... and load the file that the user picked
                    doc.Load(ofd.FileName);
                    //Use the XDP that has been created as one of the Window's resources ...
                    XmlDataProvider dp = (XmlDataProvider)this.FindResource("xmlDP");
                    //... and assign the XDoc to it, using the XDoc's root.
                    dp.Document = doc;
                    dp.XPath = "//menus";

                    tv.Style = (Style)this.FindResource("TV_AllExpanded");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message);
                //logService.WriteToLogLevel("", LogLevelEnum.Error, ex);
            }
        }

        private void cmdExpandAll_Click(object sender, RoutedEventArgs e)
        {
            tv.Style = (Style)this.FindResource("TV_AllExpanded");
        }

        private void cmdCollapse_Click(object sender, RoutedEventArgs e)
        {
            tv.Style = (Style)this.FindResource("TV_AllCollapsed");
        }


        #endregion

        #region Not in Use functions, I guess


        // Renames XAML/XML residing in config and changes corresponding entry in menu.xml
        // Edit menu.xml for changes also rename XAML/XML files
        // overwrite is to detect if command was overwritten. If yes then backup XAML XML of old one and provide undo logic
        //void RenameCommandAndDialogFiles(XmlElement modxe, string oldcommandname, string newcommandname, bool overwrite)
        //{
        //    const string FileName = @"./Config/menu.xml";
        //    //13Mar2013 XmlElement modifiedelement = tblk.DataContext as XmlElement;
        //    XmlElement modifiedelement = modxe;//13Mar2013
        //    XmlDocument xdc = modifiedelement.OwnerDocument;

        //    string nodetype = modifiedelement.GetAttribute("nodetype");

        //    if (nodetype.Trim().Equals("Leaf"))
        //    {


        //        if (overwrite) //existing name thats why overwrite
        //        {
        //            //move old XAML/XML to some backup folder
        //            //Now Rename new ones
        //        }
        //        else //unique name
        //        {
        //            string oldfullpathxamlname = modifiedelement.GetAttribute("commandtemplate");
        //            string newfullpathxamlname = oldfullpathxamlname.Replace(oldcommandname, newcommandname);
        //            modifiedelement.SetAttribute("commandtemplate", newfullpathxamlname);//xaml

        //            string oldfullpathxmlname = modifiedelement.GetAttribute("commandoutputformat");
        //            string newfullpathxmlname = oldfullpathxmlname.Replace(oldcommandname, newcommandname);
        //            modifiedelement.SetAttribute("commandoutputformat", newfullpathxmlname); //xml

        //            modifiedelement.SetAttribute("id", newcommandname.Replace(' ', '_'));
        //            //Now rename  XML/XAML files in Config if Leafnode was renamed//
        //            if (oldfullpathxamlname.Trim().Length > 0 && newfullpathxamlname.Trim().Length > 0)
        //            {
        //                if(File.Exists(@oldfullpathxamlname))
        //                    System.IO.File.Move(@oldfullpathxamlname, @newfullpathxamlname);
        //            }
        //            if (oldfullpathxmlname.Trim().Length > 0 && newfullpathxmlname.Trim().Length > 0)
        //            {
        //                if(File.Exists(@oldfullpathxmlname))
        //                    System.IO.File.Move(@oldfullpathxmlname, @newfullpathxmlname);
        //            }
        //            ////System.IO.File.Copy(@oldfullpathxamlname, @newfullpathxamlname);
        //            ////System.IO.File.Copy(@oldfullpathxmlname, @newfullpathxmlname);
        //            ////System.IO.File.Delete(@oldfullpathxamlname);
        //            ////System.IO.File.Delete(@oldfullpathxmlname);
        //        }
        //    }
        //    if (nodetype.Trim().Equals("Parent"))
        //    {
        //        modifiedelement.SetAttribute("text", newcommandname);//xaml
        //    }

        //    //save menu.xml state for new attribute values ///
        //    //xdc.Save(FileName);
        //    //LoadXml(FileName);//reload file again, to have latest installed command.
        //}


        #region Overwrite Command
        //06Mar2013 To install multiple new dialogs. XAML and XML are copied to Config after choosing name for them.
        private void AddNewOrOverwriteCommand(string NewCommLocXpath, bool Overwrite)
        {

            ///set location///
            XmlDataProvider dp = (XmlDataProvider)this.FindResource("xmlDP");
            XmlNode Location = null;
            if (NewCommLocXpath != null && NewCommLocXpath.Trim().Length > 0)
                Location = dp.Document.SelectSingleNode(NewCommLocXpath);//"//menu[@id='analyisMenu']");            
            ElementLocation = GetPathFromRoot(Location);

            string sInput = this.NewCommandName;
            string location = this.ElementLocation;
            string tempDir = Path.GetTempPath();
            string abvBlw = (this.NewCommandAboveBelowSibling != null) ? this.NewCommandAboveBelowSibling : string.Empty;
            //sInput is prefixed to XAML and XML filenames. Its the command name as shown in UI

            //Added by Aaron 04/21/2013. added the 2 lines below
            XamlFile = Path.GetFileName(XamlFile);
            XmlFile = Path.GetFileName(XmlFile);
            if (!string.IsNullOrEmpty(location))
            {
                if (Overwrite) // Overwrite existing command
                {
                    //Change xml entry in menu.xml commandtemplate and commandoutputformat
                    //create backup of old dialog xaml/xml
                    // copy new xaml xml

                    //06Mar2013 Following will actually modify menu.xml to overwrite existing command
                    //23Apr2015 OverwriteCommandXmlEntry(location, sInput, Path.Combine(@".\Config\", sInput + XamlFile), true, abvBlw);
                    OverwriteCommandXmlEntry(location, sInput, Path.Combine(string.Format(@"{0}", BSkyAppDir.RoamingUserBSkyConfigL18nPath), sInput + XamlFile), true, abvBlw);//23Apr2015 

                    ///output template filename will be same as dialog filename. No matter whats in zip file.
                    //23Apr2015 string xmlinstallpath = Path.GetFullPath(@".\Config\" + sInput + XamlFile.Replace("xaml", "xml"));
                    string xmlinstallpath = Path.GetFullPath(string.Format(@"{0}", BSkyAppDir.RoamingUserBSkyConfigL18nPath) + sInput + XamlFile.Replace("xaml", "xml"));//23Apr2015 
                    System.IO.File.Copy(Path.Combine(tempDir, XmlFile), xmlinstallpath, true);

                    //23Apr2015 string xamlinstallpath = Path.GetFullPath(@".\Config\" + sInput + XamlFile);
                    string xamlinstallpath = Path.GetFullPath(string.Format(@"{0}", BSkyAppDir.RoamingUserBSkyConfigL18nPath) + sInput + XamlFile);//23Apr2015 
                    System.IO.File.Copy(Path.Combine(tempDir, XamlFile), xamlinstallpath, true);
                }
                else//New Command
                {
                    //06Mar2013 Following will actually modify menu.xml to add new command
                    //AddNewCommandXmlEntry(location, sInput, Path.Combine(@".\Config\", sInput + XamlFile), true, abvBlw);

                    //23Apr2015 AddNewCommandXmlEntry(location, sInput, Path.Combine(@".\Config\", sInput), true, abvBlw);
                    AddNewCommandXmlEntry(location, sInput, Path.Combine(string.Format(@"{0}", BSkyAppDir.RoamingUserBSkyConfigL18nPath), sInput), true, abvBlw);//23Apr2015 

                    /////output template filename will be same as dialog filename. No matter whats in zip file.
                    //string xmlinstallpath = Path.GetFullPath(@".\Config\" + sInput + XamlFile.Replace("xaml", "xml"));
                    //if (XmlFile != null) //01Oct2013 installing output XML, if present
                    //    System.IO.File.Copy(Path.Combine(tempDir, XmlFile), xmlinstallpath, true);

                    ////Added by Aaron 04/21/2013
                    ////Commented the line below and added the line below it
                    //                    //string xamlinstallpath = Path.GetFullPath(@".\Config\" + sInput + XamlFile);
                    ////string XamlFilename =Path.GetFileName(XamlFile);
                    //string xamlinstallpath = Path.GetFullPath(@".\Config\" + sInput +XamlFile );
                    //if (XamlFile != null) //01Oct2013 installing output XAML, if present
                    //    System.IO.File.Copy(Path.Combine(tempDir, XamlFile), xamlinstallpath, true);
                }
                //MessageBox.Show("Dialog(s) modified. Changes will take effect after restarting the application!", "Info: Dialog installed.");
                ////mainWin.Window_Refresh_Menus();//15Jan2013
            }
        }

        //Modify menu.xml for overwriting command
        private bool? OverwriteCommandXmlEntry(string val, string Title, string commandFile, bool forcePlace, string AboveBelowSibling)
        {
            //23Apr2015 const string FileName = @"./Config/menu.xml";
            //string FileName = string.Format(@"{0}menu.xml", BSkyAppDir.BSkyAppDirConfigPath);//23Apr2015 
            string FileName = string.Format(@"{0}menu.xml", BSkyAppDir.RoamingUserBSkyConfigL18nPath);// + CultureName + "/");//02Oct2017

            XmlDocument document = new XmlDocument(); ;

            if (string.IsNullOrEmpty(val))
                return null;
            string[] nodes = val.Split('>');

            ////reloading a latest document. Modified by Install dialog window ///
            document.Load(FileName);

            XmlNode element = document.SelectSingleNode("//menus");

            foreach (string node in nodes)//Traverse to target parent, where new command should be added
            {
                if (node == "Root")
                    continue;
                if (element == null)
                    return null;
                element = element.SelectSingleNode("./menu[@text='" + node + "']");
            }
            if (element == null)//parent not found.
                return null;
            XmlNode temp = element.SelectSingleNode("./menu[@text='" + Title + "']");
            XmlElement overwrite = temp as XmlElement;

            string oldfullpathxamlname = overwrite.GetAttribute("commandtemplate");
            string newfullpathxamlname = commandFile;
            overwrite.SetAttribute("commandtemplate", newfullpathxamlname);//xaml

            string oldfullpathxmlname = overwrite.GetAttribute("commandoutputformat");
            string newfullpathxmlname = commandFile.Replace("xaml", "xml");
            overwrite.SetAttribute("commandoutputformat", newfullpathxmlname); //xml

            document.Save(FileName);

            LoadXml(FileName);//reload file again, to have lates installed command.
            return true;
        }
        #endregion

        //Not in Use, I guess. Used in dp_DataChanged only and is commented. We may not need following function
        //Add New Command Node at specific location
        void AddNewCommandNode(string NewCommandLocationXPath)
        {
            XmlDataProvider dp = (XmlDataProvider)this.FindResource("xmlDP");

            XmlNode location = null;
            if (NewCommandLocationXPath != null && NewCommandLocationXPath.Trim().Length > 0)
                location = dp.Document.SelectSingleNode(NewCommandLocationXPath);//"//menu[@id='analyisMenu']");
            if (location != null)
            {
                XmlNode newElement = location.SelectSingleNode("//newnode");
                if (newElement == null)//&& !newElement.Value.Equals("new id"))
                {
                    XmlElement element = dp.Document.CreateElement("newnode");
                    XmlAttribute attrib = dp.Document.CreateAttribute("id");
                    attrib.Value = newCommandID;
                    element.Attributes.Append(attrib);

                    attrib = dp.Document.CreateAttribute("text");
                    attrib.Value = this.NewCommandName;
                    element.Attributes.Append(attrib);

                    attrib = dp.Document.CreateAttribute("nodetype");//06Mar2013 User installed command will always be leaf
                    attrib.Value = "Leaf";
                    element.Attributes.Append(attrib);

                    /// adding owner attribute to prevent accidental removal of this new command node
                    attrib = dp.Document.CreateAttribute("owner");
                    //attrib.Value = "BSky"; 

                    //31Jan2013 
                    //OR we can put owner value as newcommand and also add code so that this 
                    attrib.Value = "newcommand";// may not get deleted accidently

                    element.Attributes.Append(attrib);

                    location.AppendChild(element);
                    newNode = element;
                    ElementLocation = GetPathFromRoot(location);//"Root>Analyis";//set default menu location for NewCommand
                }
            }
            if (!string.IsNullOrEmpty(menuLocation))
                SetElementLocaton(menuLocation);
        }

        // Not in Use, I think
        static TObject FindVisualParent<TObject>(UIElement child) where TObject : UIElement
        {
            if (child == null)
            {
                return null;
            }

            UIElement parent = VisualTreeHelper.GetParent(child) as UIElement;

            while (parent != null)
            {
                TObject found = parent as TObject;
                if (found != null)
                {
                    return found;
                }
                else
                {
                    parent = VisualTreeHelper.GetParent(parent) as UIElement;
                }
            }

            return null;
        }

        /// Removing <menu> for overwriting(reinstalling) dialog
        private bool RemoveDialogFromMenuxml(string attribute)
        {
            XmlDataProvider dp = (XmlDataProvider)this.FindResource("xmlDP");
            XmlDocument xmld = dp.Document;

            XmlNodeList allmenus = xmld.GetElementsByTagName("menu");
            for (int i = 0; i < allmenus.Count; i++)
            {
                string commandName = (allmenus[i].Attributes["text"] != null) ? allmenus[i].Attributes["text"].Value : "";
                if (commandName.Trim().Equals(attribute.Trim()))
                {
                    if (allmenus[i].HasChildNodes)//The node has child node.
                    {
                        MessageBox.Show(this, "You cannot overwrite a parent node.");
                        return false;
                    }
                    XmlNode p = allmenus[i].ParentNode;
                    p.RemoveChild(allmenus[i]);//dont remove old command. Instead modify attributes.

                    //06Feb2013 Move New Command node to another loacation ///
                    if (newNode != null && newNode.ParentNode != null)
                        newNode.ParentNode.RemoveChild(newNode);//Remove place holder for new command
                    if (newNode != null && p != null)//05Mar2013 there was no 'if' for following line
                        p.AppendChild(newNode);//add in place of overwritten command
                    //// Modify NewCommand location now
                    menuLocation = GetPathFromRoot(p);

                    break;
                }
            }
            return true;
        }

        public const String FileNameFilter = "BSky Commands (*.bsky)|*.bsky";

        /// Open File dialog to open BSKY (dialog designer created) file, to install new Command.
        bool OpenCommandDialogFile()
        {
            bool validDialogFile = false;//07Mar2013 If Dialog zip file(containing XAML and XML) is not valid then skip further processing
            IUnityContainer container = LifetimeService.Instance.Container;
            IDashBoardService dashboardService = container.Resolve<IDashBoardService>();

            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = FileNameFilter;
            bool? output = openFileDialog.ShowDialog(Application.Current.MainWindow);
            if (output.HasValue && output.Value)
            {
                FileStream fileStreamIn = new FileStream(openFileDialog.FileName, FileMode.Open, FileAccess.Read);
                ZipInputStream zipInStream = new ZipInputStream(fileStreamIn);

                ZipEntry entry = zipInStream.GetNextEntry();
                string tempDir = Path.GetTempPath();

                //Extract the files
                while (entry != null)
                {
                    //Added by Aaron 04/21/2013
                    //commented line below
                    //FileStream fileStreamOut = new FileStream(Path.Combine(tempDir, entry.Name), FileMode.Create, FileAccess.Write);
                    FileStream fileStreamOut = new FileStream(Path.Combine(tempDir, Path.GetFileName(entry.Name)), FileMode.Create, FileAccess.Write);
                    if (entry.Name.EndsWith(".xaml")) XamlFile = entry.Name;
                    if (entry.Name.EndsWith(".xml")) XmlFile = entry.Name;
                    int size;
                    byte[] buffer = new byte[1024];
                    do
                    {
                        size = zipInStream.Read(buffer, 0, buffer.Length);
                        fileStreamOut.Write(buffer, 0, size);
                    } while (size > 0);
                    fileStreamOut.Close();
                    entry = zipInStream.GetNextEntry();
                }
                zipInStream.Close();
                fileStreamIn.Close();

                //01Oct2013 XML output mandatory condition removed
                //if (string.IsNullOrEmpty(XmlFile))
                //{
                //    MessageBox.Show("Dialog cannot be installed as output file is not mentioned");
                //    return validDialogFile;
                //}
                //Load the dialog object, check the location and modify menu file.
                if (!string.IsNullOrEmpty(XamlFile)) //01Oct2013  XML output mandatory condition removed  && !string.IsNullOrEmpty(XmlFile))
                {
                    //04/21/2013
                    //Commented the line below and added the line under it

                    // FileStream stream = System.IO.File.Open(Path.Combine(tempDir, XamlFile), FileMode.Open);
                    FileStream stream = System.IO.File.Open(Path.Combine(tempDir, Path.GetFileName(XamlFile)), FileMode.Open);
                    try
                    {
                        object obj = XamlReader.Load(stream);
                        BSky.Controls.BSkyCanvas canvas = obj as BSky.Controls.BSkyCanvas;

                        string menulocation = canvas.MenuLocation;
                        string title = canvas.Title;
                        if (string.IsNullOrEmpty(canvas.Title))
                        {
                            title = Microsoft.VisualBasic.Interaction.InputBox("Get Item Name", "Item Name", "New Node");
                            if (!string.IsNullOrEmpty(title))
                            {
                                MessageBox.Show(this, "Title cannot be empty, Exiting Dialog install", "Info: Dialog Title Empty.");
                                return validDialogFile;
                            }
                        }
                        //InstallNewCommand(NewCommLocXpath);
                        validDialogFile = true;
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show(this, "Couldn't install Dialog: "+e.Message);
                        //logService.WriteToLogLevel("Couldn't install Dialog", LogLevelEnum.Error);
                    }
                }
            }
            return validDialogFile;
        }



        #endregion

        private void tb_MouseUp(object sender, MouseButtonEventArgs e)
        {
           
        }

        private void ContextMenu_Loaded(object sender, RoutedEventArgs e)
        {
            (NodeToRename as TextBlock).Focus();
        }
    }


}
