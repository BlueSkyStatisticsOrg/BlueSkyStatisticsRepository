using System;
using System.Collections.Generic;
using BSky.Interfaces;
using BSky.Interfaces.DashBoard;
using System.Xml;
using System.Windows.Input;
using BlueSky.Commands.Analytics.TTest;
using BSky.Controls;
using BSky.Lifetime;
using BSky.Lifetime.Interfaces;
using System.IO;
using System.Windows;
using BSky.Interfaces.Services;
using System.Threading;

namespace BlueSky.Services
{
    //public struct UAMenuCommand
    //{
    //    public string bskycommand; // for BSky command in Syntax Editor 01Aug2012
    //    public string commandtype ; // points to AUAnalysisCommandBase or AUCommandBase
    //    public string commandtemplate; // XAML
    //    public string commandformat; // BSky function. But this is not in use.
    //    public string commandoutputformat; // XML
    //    public string text; ////04mar2013  will be used in "Command History" for displaying command name
    //    //public string id;//04mar2013 not in use right now.
    //    // public string owner; ////04mar2013 not in use right now.
    //}
    class XmlDashBoardService : IDashBoardService
    {
        ILoggerService logService = LifetimeService.Instance.Container.Resolve<ILoggerService>();//17Dec2012

        string CultureName = string.Empty;
        //23Apr2015 const string FileName = @"./Config/menu.xml";
        string FileName = string.Empty;
        List<string> idAttrValList;
        int namecounter = 0;

        public XmlDashBoardService()
        {
            CultureName = Thread.CurrentThread.CurrentCulture.Name;
            //23Apr2015 const string FileName = @"./Config/menu.xml";
            FileName = string.Format(@"{0}menu.xml", BSkyAppData.RoamingUserBSkyConfigL18nPath);/// + CultureName + "/");//23Apr2015 

            idAttrValList = new List<string>();
            namecounter = 0;
        }

        #region IDashBoardService Members


        XmlDocument document;
        public string XamlFile { get; set; } //06Mar2013 XAML dialog filename

        public string XmlFile { get; set; } //06Mar2013 XML output template filename

        public void Configure()
        {
            document = new XmlDocument();
            bool success = false;
            try
            {
                document.Load(FileName);
                success = true;
            }
            catch (XmlException xe)
            {
                MessageBox.Show("XmlException while reading menu.xml");
                logService.WriteToLogLevel("XmlException.\n" + xe.StackTrace, LogLevelEnum.Fatal);
            }
            catch (DirectoryNotFoundException dnfe)
            {
                MessageBox.Show("DirectoryNotFoundException while reading menu.xml");
                logService.WriteToLogLevel("DirectoryNotFoundException.\n" + dnfe.StackTrace, LogLevelEnum.Fatal);
            }
            catch (FileNotFoundException fnfx)
            {
                MessageBox.Show("FileNotFoundException while reading menu.xml");
                logService.WriteToLogLevel("FileNotFoundException.\n" + fnfx.StackTrace, LogLevelEnum.Fatal);
            }
            catch (Exception e)
            {
                MessageBox.Show("Exception while reading menu.xml");
                logService.WriteToLogLevel("Exception.\n" + e.StackTrace, LogLevelEnum.Fatal);
            }

            if(success)
                InitializeLocalMenu(document);
            
        }
        
        private void InitializeLocalMenu(XmlDocument menuDocument)
        {
            foreach (XmlNode nd in menuDocument.SelectNodes("//menus/*"))
            {
                DashBoardItem item = CreateItem(nd);
                OnAddDashBoardItem(item);
            }
        }

        //21Jul2015 For creating toolbar dialog icons
        public List<DashBoardItem> GetDashBoardItems()//returns all DashBoardItems
        {
            List<DashBoardItem> alldashboardItems = new List<DashBoardItem>();

            //This 'idAttrValList' list is populated each time menus are generated i.e. when main window menus are generated 
            //as well as when menus for each output window are generated from same menu.xml.
            //If we do not clean se what will happen:
            //say first menus are generated for main window (using menu.xml) and all the IDs from menu.xml are stored in idAttrValList.
            //Now when menus are generated again for OutputWindow1 (using menu.xml), the duplicate IDs will be seen but our duplicate
            //handler logic will postfix a number so as to make it unique and then stored in idAttrValList.
            //Say first time ID 'analysisMenu' was add for Analysis menu in the main window. Later for output window the ID will be
            // modified as analysisMenuXXX, where XXX is any number. 
            //Now for output window we have a check to see if menu should be generated or not. That check is comparing hardcoded IDs
            // It is expecting that ID for say Analysis menu is 'analysisMenu' but our duplicate handling logic already renamed 'analysisMenu' ID
            // to analysisMenuXXX and comparison fails. And menu is not generated for outputwindow. This Happens for all the Output windows we try to open.
            //So we need to clear the idArrValList each time before generating menus for any next window.
            //
            //If in any case this starts behaving weird then find an appropriate place to clear this list or
            // put a separate list for each window(main or output(s))
            idAttrValList.Clear();

            document = new XmlDocument();
            bool success = false;
            try
            {
                document.Load(FileName);
                success = true;
            }
            catch (XmlException xe)
            {
                MessageBox.Show("XmlException while reading menu.xml");
                logService.WriteToLogLevel("XmlException.\n" + xe.StackTrace, LogLevelEnum.Fatal);
            }
            catch (DirectoryNotFoundException dnfe)
            {
                MessageBox.Show("DirectoryNotFoundException while reading menu.xml");
                logService.WriteToLogLevel("DirectoryNotFoundException.\n" + dnfe.StackTrace, LogLevelEnum.Fatal);
            }
            catch (FileNotFoundException fnfx)
            {
                MessageBox.Show("FileNotFoundException while reading menu.xml");
                logService.WriteToLogLevel("FileNotFoundException.\n" + fnfx.StackTrace, LogLevelEnum.Fatal);
            }
            catch (Exception e)
            {
                MessageBox.Show("Exception while reading menu.xml");
                logService.WriteToLogLevel("Exception.\n" + e.StackTrace, LogLevelEnum.Fatal);
            }
            if (success)
            {
                foreach (XmlNode nd in document.SelectNodes("//menus/*"))
                {
                    DashBoardItem item = CreateItem(nd); 
                    alldashboardItems.Add(item);
                }
            }
            return alldashboardItems;
        }

        /// <summary>
        /// Val is target parent location. Title is new command name.commandFile is the XAML filename.
        /// </summary>
        /// <param name="val"></param>
        /// <param name="Title"></param>
        /// <param name="commandFile"></param>
        /// <param name="forcePlace"></param>
        /// <returns></returns>
        public bool? SetElementLocaton(string val, string Title, string commandFile,bool forcePlace, string AboveBelowSibling)
        {
            if(string.IsNullOrEmpty(val))
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
            attrib.Value = commandFile;
            newelement.Attributes.Append(attrib);

            attrib = document.CreateAttribute("commandoutputformat");
            attrib.Value = commandFile.Replace("xaml","xml");//same filenames(dialog and out-template) but diff extensions
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

            //if parent node or new node then add as child
            if (element.HasChildNodes || (element.Attributes["id"]!=null && element.Attributes["id"].Value == "new_id"))
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
            //23Apr2015 document.Save(@"./Config/menu.xml");
            document.Save(string.Format(@"{0}Menu.xml", BSkyAppData.RoamingUserBSkyConfigL18nPath));//23Apr2015
            return true;
        }

        private DashBoardItem CreateItem(XmlNode node)
        {
            DashBoardItem item = new DashBoardItem();

            item.ID = GetAttributeString(node, "id");//02Oct2017
            item.Name = GetAttributeString(node, "text");
            item.isGroup = node.HasChildNodes;

            if (node.HasChildNodes)
            {
                item.Items = new List<DashBoardItem>();
                foreach (XmlNode child in node.ChildNodes)
                    item.Items.Add(CreateItem(child));
            }
            else
            {
                UAMenuCommand cmd = new UAMenuCommand();
                cmd.commandtype = GetAttributeString(node, "command");
                if (string.IsNullOrEmpty(cmd.commandtype))
                {
                    cmd.commandtype = typeof(AUAnalysisCommandBase).FullName;
                }

                //09Oct2017
                //First we need to convert dialogs for support localization. Say for supporting German (de-DE) first
                //the source dialogs must be opened in Dialog Designer and saved after changing to German.
                //Then Using Dialog-Installer user with German locale should install the dialogs by Overwriting the
                //exisitng ones. This will overwrite the dialogs in 'Config/de-DE' folder replacing US dialogs.
                //Now about the following code:
                //During developement and testing phase we need postfix CulterName below But ones the German user overwrite-install
                //his dialogs the menu.xml will have 'de-DE' included in menu.xml for XAML/XML location (which was not there and 
                //so we postfixed CultureName). Now after overwriting dialogs by German user we do not need CulterName postfixed.
                //So we can achieve this in two ways:
                //1) In Develop/Test phase keep the CultureName in following and once German Dialogs are installed/overwritten
                //then we can drop this CulterName postfix because now menu.xml will inculde 'de-De' for XAML/XML dialog path.
                //2) In this method we need to check if commandtemplate/commandoutputformat in following have two '/' or three '/'
                //if there are two '/' that means language foldernames are not yet included, so postfix CultureName. 
                //If there ar three '/' that means language folder name is included and there is not need to postfix with CultureName.
                //
                string dialogPath = BSkyAppData.RoamingUserBSkyConfigPath + CultureName;
                string CmdTemplate = GetAttributeString(node, "commandtemplate");
                //if (CountCharacter(CmdTemplate, '/') == 2)
                //{
                //    cmd.commandtemplate = GetAttributeString(node, "commandtemplate").Replace("/Config", "/Config/" + CultureName);
                //}
                //else
                //{
                //    cmd.commandtemplate = GetAttributeString(node, "commandtemplate");
                //}
                //cmd.commandtemplate=cmd.commandtemplate.Replace("./Config", BSkyAppData.RoamingUserBSkyConfigPath);
                cmd.commandtemplate = string.Format(@"{0}\{1}", dialogPath, CmdTemplate);

                string CmdOutTemplate = GetAttributeString(node, "commandoutputformat");
                //if (CountCharacter(CmdTemplate, '/') == 2)
                //{
                //    cmd.commandoutputformat = GetAttributeString(node, "commandoutputformat").Replace("/Config", "/Config/" + CultureName);
                //}
                //else
                //{
                //    cmd.commandoutputformat = GetAttributeString(node, "commandoutputformat");
                //}
                //cmd.commandoutputformat = cmd.commandoutputformat.Replace("./Config", BSkyAppData.RoamingUserBSkyConfigPath);
                cmd.commandoutputformat = string.Format(@"{0}\{1}", dialogPath, CmdOutTemplate);


                //cmd.commandtemplate = GetAttributeString(node, "commandtemplate");//no localization
                //if-else above  cmd.commandtemplate = GetAttributeString(node, "commandtemplate").Replace("/Config", "/Config/"+CultureName);

                //cmd.commandoutputformat = GetAttributeString(node, "commandoutputformat");//no localization
                //if-else above  cmd.commandoutputformat = GetAttributeString(node, "commandoutputformat").Replace("/Config", "/Config/" + CultureName);

                cmd.commandformat = GetAttributeString(node, "commandformat");

                cmd.text = GetAttributeString(node, "text"); //04mar2013
                //cmd.id = GetAttributeString(node, "id"); //04mar2013
                //cmd.owner = GetAttributeString(node, "owner"); //04mar2013
                item.Command = CreateCommand(cmd);
                item.CommandParameter = cmd;

                #region 11Jun2015 Set icon fullpathfilename 
                item.iconfullpathfilename = GetAttributeString(node, "icon");
                string showicon = GetAttributeString(node, "showtoolbaricon"); 
                if(showicon==null || showicon.Trim().Length == 0 || !showicon.ToLower().Equals("true"))
                {
                    item.showshortcuticon = false;
                }
                else
                {
                    item.showshortcuticon = true;
                }
                #endregion
            }
            return item;
        }

        //Cout specific character in a string
        private int CountCharacter(string s, char ch)
        {
            int count = 0;
            foreach (char chr in s)
            {
                if (chr == ch)
                    count++;
            }
            return count;
        }


        private ICommand CreateCommand(UAMenuCommand cmd)
        {
            Type commandTypeObject = null;
            ICommand command = null;

            try
            {
                commandTypeObject = Type.GetType(cmd.commandtype);
                command = (ICommand)Activator.CreateInstance(commandTypeObject);
            }
            catch
            {
                    //Create new command instance using default command dispatcher
                logService.WriteToLogLevel("Could not create command. "+cmd.commandformat, LogLevelEnum.Error);
            }

            return command;
        }

        private bool GetAttributeBool(XmlNode nd, string name)
        {
            bool result = false;

            bool.TryParse(GetAttributeString(nd, name), out result);

            return result;
        }

        //following two defined in the begining
        //List<string> idAttrValList = new List<string>();
        //int namecounter = 0;
        private string GetAttributeString(XmlNode nd, string name)
        {
            string attributeValue = string.Empty;//05Oct2017

            XmlAttribute att = null;
            att = nd.Attributes[name];
            
            attributeValue = (att != null) ? att.Value : ( name.Equals("id")?"id":string.Empty);//default value is set to 'id' for the nodes those do not have a id value.

            //05Oct2017 We can add some logic here to make sure ID attribute is unique even though its not unique in menu.xml
            if (!string.IsNullOrEmpty(name) && name.ToLower().Equals("id")) //if its 'id' attribute from menu.xml
            {
                //try to create unique value for 'id' attribute
                if (idAttrValList.Contains(attributeValue))//if duplicate
                {
                    attributeValue = attributeValue + namecounter;
                    namecounter++;
                }
                idAttrValList.Add(attributeValue);
            }

            return attributeValue;
        }

        public event EventHandler<DashBoardEventArgs> AddDashBoardItem;

        protected virtual bool OnAddDashBoardItem(DashBoardItem item)
        {
            if (AddDashBoardItem != null)
            {
                DashBoardEventArgs args = new DashBoardEventArgs();
                args.DashBoardItem = item;
                AddDashBoardItem(this, args);
                return true;
            }
            else
            {
                return false;
            }
        }

        public string SelectLocation(ref string newcommandname,  ref string AboveBelowSibling)
        {
            MenuEditor editor = new MenuEditor(newcommandname);
            editor.LoadXml(FileName);
            editor.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
            editor.Activate();
            editor.ShowDialog();
            
            if (editor.DialogResult.HasValue && editor.DialogResult.Value)
            {
                string str = editor.ElementLocation;
                newcommandname = editor.NewCommandName;
                AboveBelowSibling = (editor.NewCommandAboveBelowSibling != null)? editor.NewCommandAboveBelowSibling:string.Empty;//06Feb2013
                XamlFile = (editor.XamlFile!=null && editor.XamlFile.Length>0) ? editor.XamlFile:string.Empty; //06Mar2013
                XmlFile = (editor.XmlFile != null && editor.XmlFile.Length > 0) ? editor.XmlFile : string.Empty; //06Mar2013
                return str;
            }
            return string.Empty;
        }


        #endregion
    }
}
