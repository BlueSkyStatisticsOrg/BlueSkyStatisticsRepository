using System.Collections.Generic;
using BSky.Interfaces.DashBoard;
using BlueSky.Services;
using System.Windows.Controls;
using BlueSky.Commands.Analytics.TTest;
using BSky.Interfaces.Interfaces;
using System.Windows.Media;
using BSky.Interfaces.Services;

namespace BlueSky.Commands.History
{
    public class CommandHistoryMenuHandler
    {
        
        MenuItem commandhistmenu;
        bool IsMainApp = true;
        IUIController UIController;//for getting active dataset and filename.

        public CommandHistoryMenuHandler() //This constructor is meant to used for Main App only
        {
            //Giving error here UIController = LifetimeService.Instance.Container.Resolve<IUIController>();
            DashBoardItem item = new DashBoardItem();
            item.Command = null;
            item.CommandParameter = null;
            item.isGroup = true;
            item.Name = BSky.GlobalResources.Properties.UICtrlResources.HistoryMenuName;// MenuName
            item.Items = new List<DashBoardItem>();

            commandhistmenu = CreateItem(item);
        }

        public MenuItem CommandHistMenu
        {
            get { return commandhistmenu; }
        }

        //Add executed command per Dataset
        public void AddCommand(string DatasetName, UAMenuCommand Command)
        {
            //Creating a copy of command for putting it in "History" menu
            UAMenuCommand uamc = new UAMenuCommand(); // new obj needed, because same obj can't be child at two places.
            uamc.commandformat = Command.commandformat;
            uamc.commandoutputformat = Command.commandoutputformat; 
            uamc.commandtemplate = Command.commandtemplate;
            uamc.commandtype = Command.commandtype;
            //29Mar2013 //If History Menu text is not present. Dont add this command to "History" menu
            if(Command.text == null || Command.text.Length < 1)
            { return; }
            uamc.text = Command.text;//Command name shown in "History" menu

            //now create menuitem for each command and add to "History" menu
            DashBoardItem item = new DashBoardItem();
            item.Command = new AUAnalysisCommandBase(); // should point to AUAnalysisCommandBase
            item.CommandParameter = uamc;
            item.isGroup = false;
            item.Name = uamc.text;// MenuName
            MenuItem newmi = CreateItem(item);

            //Check if command already in history menu ///
            bool ExistsInHistory = false;
            int miIndex = 0;
            foreach (MenuItem mi in commandhistmenu.Items)
            {
                if (mi.Header == newmi.Header)//command already in History
                {
                    ExistsInHistory = true;
                    break;
                }
                miIndex++;
            }

            // Adding command with "latest executed on top" in menu ///
            if (ExistsInHistory)
            {
                commandhistmenu.Items.RemoveAt(miIndex);
            }
            commandhistmenu.Items.Insert(0, newmi);//Adding to "History" menu
        }

        //remove executed command per Dataset
        public void RemoveCommand(string DatasetName, UAMenuCommand Command)
        {
            //Check if command already in history menu ///
            bool ExistsInHistory = false;
            int miIndex = 0;
            foreach (MenuItem mi in commandhistmenu.Items)
            {
                if (mi.Header.ToString() == Command.text)//command already in History
                {
                    ExistsInHistory = true;
                    break;
                }
                miIndex++;
            }

            // Adding command with "latest executed on top" in menu ///
            if (ExistsInHistory)
            {
                commandhistmenu.Items.RemoveAt(miIndex);
            }
        }


        ///Creating Menu Items///
        private MenuItem CreateItem(DashBoardItem item)
        {
            MenuItem menuitem = new MenuItem();
            menuitem.Header = item.Name;
            if (item.isGroup)
            {
                foreach (DashBoardItem i in item.Items)
                {
                    if (i.Name.ToLower() == "---------") // 9 hyphens
                    {
                        MenuItem mnuitem = new MenuItem();
                        mnuitem.Header = new Separator();
                        menuitem.Items.Add(mnuitem);
                        //menuitem.Items.Add(new Separator());// { Background = new SolidColorBrush(Color.FromArgb(0xFF, 0xFF, 0, 0)) }
                    }
                    else
                        menuitem.Items.Add(CreateItem(i));
                }
            }
            else
            {
                menuitem.Command = item.Command;
                menuitem.CommandParameter = item.CommandParameter;
            }
            return menuitem;

        }

    }
}
