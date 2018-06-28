using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.IO;
using BSky.Lifetime;
using Microsoft.Practices.Unity;
using BSky.Interfaces.Interfaces;
using BSky.Statistics.Common;
using Microsoft.Win32;

namespace BlueSky.Dialogs
{
    /// <summary>
    /// Interaction logic for SaveLoadRObjectsWindow.xaml
    /// </summary>
    public partial class SaveLoadRObjectsWindow : Window
    {
        IUnityContainer container = LifetimeService.Instance.Container;
        IDataService service ;
        IUIController controller;
        public SaveLoadRObjectsWindow()
        {
            InitializeComponent();
            service = container.Resolve<IDataService>();
            controller = container.Resolve<IUIController>();

            UAReturn result = service.GetAllRObjects();
            if (result.SimpleTypeData.GetType().Name.Equals("String[]"))
            {
                robjlist.ItemsSource = result.SimpleTypeData as String[];
            }
        }



        public const String FileNameFilter = "R Obj (*.RData)|*.RData";
        private void browsebutton1_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.SaveFileDialog sfd = new Microsoft.Win32.SaveFileDialog();
            sfd.Filter = FileNameFilter;
            Window1 appwin = LifetimeService.Instance.Container.Resolve<Window1>();
            bool? output = sfd.ShowDialog(appwin);//Application.Current.MainWindow);
            if (output.HasValue && output.Value)
            {
                //string objname = string.Empty;//assign selected object from the list
                //service.SaveRObjects(objname, sfd.FileName);
                savefilepathtxt.Text = sfd.FileName;
            }
            
        }

        private void savebutton_Click(object sender, RoutedEventArgs e)
        {
            string objname = robjlist.SelectedItem.ToString();//assign selected object from the list
            string fulpathfilename = savefilepathtxt.Text;
            if (fulpathfilename != null && fulpathfilename.Trim().Length > 0)
            {
                service.SaveRObjects(objname, fulpathfilename);
            }
        }

        private void cancelbutton1_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void browsebutton2_Click(object sender, RoutedEventArgs e)
        {

            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = FileNameFilter;
            Window1 appwin = LifetimeService.Instance.Container.Resolve<Window1>();
            bool? output = ofd.ShowDialog(appwin);//Application.Current.MainWindow);
            if (output.HasValue && output.Value)
            {
                //service.LoadRObjects(ofd.FileName);
                loadfilepathtxt.Text = ofd.FileName;
            }
        }

        private void loadbutton_Click(object sender, RoutedEventArgs e)
        {
            string fulpathfilename = loadfilepathtxt.Text;
            if (fulpathfilename != null && fulpathfilename.Trim().Length > 0)
            {
                service.LoadRObjects(fulpathfilename);
            }
        }

        private void cancelbutton2_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private bool IsValidDirectory(string path)
        {
            if (Directory.Exists(path) || path.Trim().Length == 0)// valid folder or blank if defaults are needed
            {
                return true;
            }
            else
            {
                MessageBox.Show("Path does not exist! " + path);
                return false;
            }
        }

    }
}
