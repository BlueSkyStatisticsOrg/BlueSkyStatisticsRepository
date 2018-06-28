using BSky.Interfaces;
using BSky.Interfaces.DashBoard;
using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace BSky.MenuGenerator
{
    public class MainWindowMenuFactory
    {
        IDashBoardService _dashBoardService;//15Jan2013
        Menu _menu;
        ToolBar _mainToolbar;//toolbar in Main Window
        string strversion;
        readonly string _commcode;

        public MainWindowMenuFactory(Menu menu, ToolBar maintoolbar, IDashBoardService dashBoardService, string commcode="dsrew324$%#")
        {
            //control the generation of menuitems based on version
            Version version = Assembly.GetCallingAssembly().GetName().Version;
            strversion = version.ToString();
            _commcode = commcode;
            _dashBoardService = dashBoardService;
            _menu = menu;
            _mainToolbar = maintoolbar;
            dashBoardService.AddDashBoardItem += new EventHandler<DashBoardEventArgs>(dashBoardService_AddDashBoardItem);
            dashBoardService.Configure();//Creates menu from menu.xml
        }


        //Adds all menus and submenu items. But not history or output menu.
        void dashBoardService_AddDashBoardItem(object sender, DashBoardEventArgs e)
        {
            DashBoardItem item = e.DashBoardItem;
            _menu.Items.Add(CreateItem(item));

        }

        OpenSourceDialogContainer osdc = new OpenSourceDialogContainer();
        private MenuItem CreateItem(DashBoardItem item)
        {
             bool enableDialog = true; //
            if (!item.isGroup)// if its a command and not catagory(folder)
            {
                if (!_commcode.Equals("test"))
                {
                    if (!osdc.ContainsDialog(item))//for Opensource
                    {
                        enableDialog = false;
                    }
                }
            }

            MenuItem menuitem = new MenuItem();
            
            menuitem.Header = item.Name;

            if (item.ID != null && item.ID.Trim().Length > 0 && !item.ID.Equals("new_id"))
            {

                menuitem.Name = RemoveSpecialCharsFromString( item.ID); //02Oct2017 remove special chars and then assign to Name property
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
                        if(dlgmi!=null)
                            menuitem.Items.Add(dlgmi);
                    }
                }
            }
            else
            {
                menuitem.Command = item.Command;
                menuitem.IsEnabled = enableDialog;
                menuitem.CommandParameter = item.CommandParameter;
                AddToolbarIcon(item); // icon for leaf icons
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

        //11Jun2015 Adds analysis dialog icon to toolbar
        private void AddToolbarIcon(DashBoardItem item)
        {
            if (item.showshortcuticon)
            {
                string icontooltip = item.Name;
                string imgsource = item.iconfullpathfilename;
                if (!File.Exists(imgsource))
                {
                    imgsource = "images/noimage.png";
                }

                bool enableDialog = true; //
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
                _mainToolbar.Items.Add(iconButton);
            }
        }

    }
}
