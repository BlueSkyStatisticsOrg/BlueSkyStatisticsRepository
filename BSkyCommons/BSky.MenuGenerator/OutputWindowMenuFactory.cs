using BSky.ConfigService.Services;
using BSky.ConfService.Intf.Interfaces;
using BSky.Interfaces;
using BSky.Interfaces.DashBoard;
using BSky.Lifetime;
using BSky.Lifetime.Interfaces;
using BSky.Lifetime.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace BSky.MenuGenerator
{
    public class OutputWindowMenuFactory
    {
        ILoggerService logService = LifetimeService.Instance.Container.Resolve<ILoggerService>();//21Jun2016
        IConfigService confService = LifetimeService.Instance.Container.Resolve<IConfigService>();//21Jun2016
        bool AdvancedLogging;//21Jun2016

        ToolBar _dialogtoolbar;
        Menu _outputmenu;
        string strversion;
        readonly string _commcode;
        public OutputWindowMenuFactory(Menu outputwinmenu, ToolBar outputwintoolbar, string commcode = "dsrew324$%#")
        {
            AdvancedLogging = AdvancedLoggingService.AdvLog;//21Jun2016

            //control the generation of menuitems based on version
            Version version = Assembly.GetCallingAssembly().GetName().Version;
            strversion = version.ToString();
            _commcode = commcode;
            _outputmenu = outputwinmenu;
            _dialogtoolbar = outputwintoolbar;
            CreateToolbarIcons();
        }

        //////////////////////////////////////////////////////////////////////////////////
        //21Jul2015 Get DashBoardItems for creating toolbar icons
        private void CreateToolbarIcons()
        {
            IDashBoardService dashBoardService = LifetimeService.Instance.Container.Resolve<IDashBoardService>();
            
            List<DashBoardItem> dbis = dashBoardService.GetDashBoardItems();//Creates menu from menu.xml
            CreateSomeMenus(dbis);// generate Menu 
           
            foreach (DashBoardItem dbi in dbis) //generate toolbar icons
            {
                AddToolbarIcon(dbi);
            }
        }

        private void CreateSomeMenus(List<DashBoardItem> dbis)
        {
            foreach (DashBoardItem dbi in dbis)
            {
                if (dbi.ID.Equals("analysisMenu") || dbi.ID.Equals("graphicMenu") || dbi.ID.Equals("splitMenu") || dbi.ID.Equals("dataMenu") 
                || dbi.ID.Equals("ModelFittingMenu") || dbi.ID.Equals("ModelTuningMenu") || dbi.ID.Equals("ModelStatsMenu"))
                    _outputmenu.Items.Insert(_outputmenu.Items.Count - 1, CreateItem(dbi)); 
            }
        }

        OpenSourceDialogContainer osdc = new OpenSourceDialogContainer();
        //Create Analysis Graphic and Data menu etc.
        private MenuItem CreateItem(DashBoardItem item)
        {
            bool enableDialog = true; //
            if (!item.isGroup)// if its a command and not catagory(folder)
            {
                if (!_commcode.Equals("test"))
                {
                    if (!osdc.ContainsDialog(item)) //for Opensource
                    {
                        enableDialog = false;
                    }
                }

            }

            MenuItem menuitem = new MenuItem();
            menuitem.Header = item.Name;

            if (item.ID != null && item.ID.Trim().Length > 0 && !item.ID.Equals("new_id"))
            {

                menuitem.Name = RemoveSpecialCharsFromString(item.ID); //02Oct2017 remove special chars and then assign to Name property
            }

            if (item.isGroup)
            {
                foreach (DashBoardItem i in item.Items)
                {
                    if (i.Name.ToLower() == "---------")
                    {

                        if (menuitem.Items.Count > 0)//'if' added for line below. So separator is not the first item in menu
                            menuitem.Items.Add(new Separator());
                    }
                    else
                    {
                        MenuItem dlgmi = CreateItem(i);
                        if (dlgmi != null)
                            menuitem.Items.Add(dlgmi);
                    }
                }
            }
            else
            {
                menuitem.Command = item.Command;
                menuitem.IsEnabled = enableDialog;
                menuitem.CommandParameter = item.CommandParameter;
                if (AdvancedLogging) logService.WriteToLogLevel("Menu item :" + menuitem.Name + ": Enable = " + enableDialog, LogLevelEnum.Info);//21Jun2016
            }
            return menuitem;

        }

        //This will modify a given string 
        private string RemoveSpecialCharsFromString(string str)
        {
            StringBuilder sb = new StringBuilder();
            foreach (char c in str)
            {
                if ((c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || c == '_')
                {
                    sb.Append(c);
                }
                else if ((c >= '0' && c <= '9'))//if first char is number
                {
                    if (sb.Length == 0)//prefix is added first
                        sb.Append('V');
                    sb.Append(c);
                }
            }

            return sb.ToString();
        }


        //21Jun2015 Adds analysis dialog icon to toolbar
        private void AddToolbarIcon(DashBoardItem item)
        {
            if (item.Items != null && item.Items.Count > 0)//Parent Node
            {
                foreach (DashBoardItem dbi in item.Items)
                {
                    AddToolbarIcon(dbi);
                }
            }
            else 
            {
                if (item.showshortcuticon)
                {
                    string icontooltip = item.Name;
                    string imgsource = item.iconfullpathfilename;
                    if (!File.Exists(imgsource))
                    {
                        imgsource = "images/noimage.png";
                    }

                    bool enableDialog = true; 
                    if (!item.isGroup)
                    {
                        if (!_commcode.Equals("test"))
                        {
                            if (!osdc.ContainsDialog(item))
                            {
                                enableDialog = false;
                            }
                        }
                    }
                    Button iconButton = new Button();
                    if (enableDialog)
                    {
                        iconButton.Command = item.Command;
                        iconButton.CommandParameter = item.CommandParameter;//all XAML XML info
                    }
                    iconButton.IsEnabled = enableDialog;
                    if (AdvancedLogging) logService.WriteToLogLevel("icon: " + item.Name + ": Enable = " + enableDialog, LogLevelEnum.Info);//21Jun2016

                    StackPanel sp = new StackPanel();

#region create icon
                    Image iconImage = new Image();
                    iconImage.ToolTip = icontooltip;
                    var bitmap = new BitmapImage();
                    try
                    {
                        var stream = File.OpenRead(imgsource);
                        bitmap.BeginInit();
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.StreamSource = stream;
                        bitmap.EndInit();
                        stream.Close();
                        stream.Dispose();
                        iconImage.Source = bitmap;
                        bitmap.StreamSource.Close();
                    }
                    catch (Exception ex)
                    {

                    }
#endregion

                    //add to stackpanel
                    sp.Children.Add(iconImage);

                    //add stackpanel to button content
                    iconButton.Content = sp;

                    //add iconbutton to toolbar
                    _dialogtoolbar.Items.Add(iconButton);
                }
            }
        }

    }
}
