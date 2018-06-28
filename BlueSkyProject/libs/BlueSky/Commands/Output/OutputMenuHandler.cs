using System.Collections.Generic;
using BSky.Interfaces.DashBoard;
using BlueSky.Services;
using System.Windows.Controls;
using System.Windows.Media;
using BSky.Interfaces.Services;

namespace BlueSky.Commands.Output
{
    public class OutputMenuHandler
    {
        static List<MenuItem> allOutputMenus = new List<MenuItem>();//container for all output menus( App's Output, Syn Editr's Output SynEdt2's Output etc..)
        MenuItem outputmenu;

        bool IsMainApp = true; 
        public OutputMenuHandler() //This constructor is meant to used for Main App only
        {
            DashBoardItem item = new DashBoardItem();
            item.Command = null;
            item.CommandParameter = null;
            item.isGroup = true;
            item.Name = BSky.GlobalResources.Properties.Resources.OutputMenu;//"Output";// MenuName
            item.Items = new List<DashBoardItem>();

            outputmenu = CreateItem(item); 
            allOutputMenus.Add(outputmenu);//adding output menu to common list
            AddDefaultOutputMenuItem();
            if (allOutputMenus.Count > 1) // first item is the output menu from App's main window. When App launches, it is first one to create Output menu
                CreateClone();/// for newly opened Syntax Editor window ///
        }

        public MenuItem OutputMenu
        {
            get { return outputmenu; }
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
                    if (i.Name.ToLower() == "---------")
                    {
                        //MenuItem mnuitem = new MenuItem();
                        //mnuitem.Header = new Separator();
                        //if (menuitem.Items.Count > 0) //'if' added for line below. So separator is not the first item in menu
                        //    menuitem.Items.Add(mnuitem);

                        if (menuitem.Items.Count > 0) //'if' added for line below. So separator is not the first item in menu
                            menuitem.Items.Add(new Separator());

                        //Separator spr = new Separator();
                        //spr.BorderBrush = new SolidColorBrush(Color.FromArgb(0xFF, 0, 0, 0));
                        //spr.Foreground = new SolidColorBrush(Color.FromArgb(0xFF, 0, 0xFF, 0));
                        //spr.Background = new SolidColorBrush(Color.FromArgb(0xFF, 0xFF, 0, 0));
                        //spr.Margin = new System.Windows.Thickness(3);
                        //menuitem.Items.Add(spr);
                        //menuitem.ItemContainerStyle = new System.Windows.Style(
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

        ///// Add Default items in Output Menu ////
        private void AddDefaultOutputMenuItem()//string outwindowname)
        {
            ///New Output////
            DashBoardItem item = new DashBoardItem();
            item.Command = new NewOutputWindow();
            item.Name = BSky.GlobalResources.Properties.Resources.NewOutputWin;
            item.isGroup = false;
            outputmenu.Items.Add(CreateItem(item));

            ////Open Output/////
            DashBoardItem item2 = new DashBoardItem();
            item2.Command = new OutputOpenCommand();
            item2.Name = BSky.GlobalResources.Properties.Resources.OpenOutput;
            item2.isGroup = false;
            outputmenu.Items.Add(CreateItem(item2));

            ////Save Output/////Output menu 'll hv "Save output" option in App's Main window.Other windows will not (like Syn Edtr window) hv.
            //count = 0 in begining. But when App's main window is shown count will become 1. That means 1 Output menu exists and thats
            // in App's Main window. Later when Syntax Editor window is shown Output count is not == 1 so "Save Output" is not created.
            if (allOutputMenus.Count == 1) 
            {
                DashBoardItem item3 = new DashBoardItem();
                item3.Command = new OutputSaveAsCommand();
                item3.Name = BSky.GlobalResources.Properties.Resources.SaveOutput;
                item3.isGroup = false;
                outputmenu.Items.Add(CreateItem(item3));
            }

            ///Adding separator////
            //MenuItem menuitem = new MenuItem();
            //menuitem.Header = new Separator();
            //outputmenu.Items.Add(menuitem);

            //MenuItem menuitem = new MenuItem();
            //menuitem.Header = "";
            //menuitem.Items.Add(new Separator());
            //outputmenu.Items.Add(menuitem);

            outputmenu.Items.Add(new Separator());
        }

        /// <summary>
        /// Creating clone of Output menu from App's main window. The section that shows names of each output window in current session.
        /// </summary>
        private void CreateClone()//List<DashBoardItem> di)
        {
            MenuItem output_menu=null;
            if (allOutputMenus.Count > 0)
                output_menu = allOutputMenus[0];//grab the first menu.(this should be from Apps main window)

                if (output_menu.Header.ToString() == BSky.GlobalResources.Properties.Resources.OutputMenu)
                {
                    MenuItem mi1=null;
                    //Separator spr;
                    foreach (object objmi in output_menu.Items)/// windownames
                    {
                        if (objmi.GetType().Name == "MenuItem")
                            mi1 = objmi as MenuItem;
                        //else if (objmi.GetType().Name == "Separator")
                        //    spr = objmi as Separator;

                        /// default items are already being created ///
                        if (mi1!=null && (mi1.Header.ToString().Equals(BSky.GlobalResources.Properties.Resources.NewOutputWin) ||
                            mi1.Header.ToString().Equals(BSky.GlobalResources.Properties.Resources.OpenOutput) ||
                            mi1.Header.ToString().Equals(BSky.GlobalResources.Properties.Resources.SaveOutput)) ||
                            (objmi.GetType().Name == "Separator"))
                            continue;
                        //////creating clone////
                        DashBoardItem item = new DashBoardItem();
                        item.Command = new SelectOutputWindowCommand();

                        UAMenuCommand uamc = new UAMenuCommand(); //01Aug2012. There was no 'new' before
                        uamc.commandformat = mi1.Header.ToString();//window name is key. Action shud b taken on this.
                        uamc.commandoutputformat = ""; uamc.commandtemplate = ""; uamc.commandtype = "";
                        item.CommandParameter = uamc;

                        item.isGroup = false;
                        item.Name = mi1.Header.ToString();//this should also be the key
                        MenuItem createdmi = CreateItem(item);
                        createdmi.Icon = mi1.Icon;

                        outputmenu.Items.Add(createdmi);
                    }
                }
        }

        //// Adding new output windows ////
        public void AddOutputMenuItem(string outwindowname)
        {
            DashBoardItem item = new DashBoardItem();
            item.Command = new SelectOutputWindowCommand();

            UAMenuCommand uamc = new UAMenuCommand(); //01Aug2012. There was no 'new' before
            uamc.commandformat = outwindowname;//window name is key. Action shud b taken on this.
            uamc.commandoutputformat = ""; uamc.commandtemplate = ""; uamc.commandtype = "";
            item.CommandParameter = uamc;

            item.isGroup = false;
            item.Name = outwindowname;//this should also be the key

            foreach (MenuItem output_menu in allOutputMenus)
            {
                if (output_menu.Header.ToString() == BSky.GlobalResources.Properties.Resources.OutputMenu)
                {
                    output_menu.Items.Add(CreateItem(item));
                }
            }
            CheckOutputMenuItem(outwindowname);//putting a check or alphabet to show which one is active
        }

        //// Remove output windowname ////
        public void RemoveOutputMenuItem(string outwindowname)
        {
            foreach (MenuItem output_menu in allOutputMenus)
            {
                if (output_menu.Header.ToString() == BSky.GlobalResources.Properties.Resources.OutputMenu)
                {
                    MenuItem mi1 = null;
                    foreach (object objmi in output_menu.Items)
                    {
                        if (objmi.GetType().Name == "MenuItem")
                            mi1 = objmi as MenuItem;
                        //else if (objmi.GetType().Name == "Separator")
                        //    spr = objmi as Separator;
                        if (mi1.Header.ToString() == outwindowname)
                        {
                            output_menu.Items.Remove(mi1);
                            break;
                        }
                    }
                }
            }
        }

        //// putting a check or alphabet to show which one is active ////
        public void CheckOutputMenuItem(string outwindowname)
        {
            foreach (MenuItem output_menu in allOutputMenus)
            {
                if (output_menu.Header.ToString() == BSky.GlobalResources.Properties.Resources.OutputMenu)
                {
                    MenuItem mi1 = null;
                    foreach (object objmi in output_menu.Items)/// windownames
                    {
                        if (objmi.GetType().Name == "MenuItem")
                            mi1 = objmi as MenuItem;
                        //else if (objmi.GetType().Name == "Separator")
                        //    spr = objmi as Separator;
                        if (mi1.Header.ToString() == outwindowname)
                        {
                            mi1.Icon = "A";
                        }
                        else
                        {
                            mi1.Icon = "";
                        }
                    }
                }
            }
        }

    }
}
