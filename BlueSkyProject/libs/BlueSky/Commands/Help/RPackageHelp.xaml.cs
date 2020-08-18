using BlueSky.Commands.Tools.Package;
using BSky.ConfService.Intf.Interfaces;
using BSky.Interfaces.Interfaces;
using BSky.Lifetime;
using BSky.Lifetime.Interfaces;
using BSky.Statistics.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace BlueSky.Commands.Help
{
    /// <summary>
    /// Interaction logic for RPackageHelp.xaml
    /// </summary>
    public partial class RPackageHelp : Window
    {
        public RPackageHelp()
        {
            InitializeComponent();
            LoadInstalledRPkgNames();
            //LoadDatasetNames(true);//load datasets from all R installed packages at launch.
        }
        ILoggerService logService = LifetimeService.Instance.Container.Resolve<ILoggerService>();
        IConfigService confService = LifetimeService.Instance.Container.Resolve<IConfigService>();

        //Load names of all (installed) R packages in the RpkgCombo Combobox
        private void LoadInstalledRPkgNames()
        {
            List<string> insPkgLst = GetListOfInstalledRPkgs();
            RpkgCombo.ItemsSource = insPkgLst;
        }

        private void OKBtn_Click(object sender, RoutedEventArgs e)
        {
            string rpkgname = string.Empty;
            if (RpkgCombo.SelectedValue != null)
                rpkgname = (RpkgCombo.SelectedValue as string).Trim();
            else if (!String.IsNullOrEmpty(RpkgCombo.Text))
                rpkgname = (RpkgCombo.Text).Trim();

            if (rpkgname != null && rpkgname.Length > 0)
            {
                this.Close();
                LoadPackageHelp(rpkgname);
            }
        }

        //cancel out do nothing.
        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        #region calls to R

        private List<string> GetListOfInstalledRPkgs()
        {
            List<string> installed = new List<string>();
            try
            {
                PackageHelperMethods phm = new PackageHelperMethods();
                UAReturn r = phm.ShowInstalledPackages();// ShowInstalledPackages();
                if (r != null && r.Success && r.SimpleTypeData != null)
                {
                    //SendToOutputWindow(r.CommandString, "Show Installed Packages");
                    string[] strarr = null;

                    if (r.SimpleTypeData.GetType().Name.Equals("String"))
                    {
                        strarr = new string[1];
                        strarr[0] = r.SimpleTypeData as string;
                    }
                    else if (r.SimpleTypeData.GetType().Name.Equals("String[]"))
                    {
                        strarr = r.SimpleTypeData as string[];
                    }

                    //strarr to list
                    foreach (string s in strarr)
                        installed.Add(s);
                }
                else
                {
                    ////SendToOutputWindow(BSky.GlobalResources.Properties.Resources.ErrGettingInstalledPkgs, "", false);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(BSky.GlobalResources.Properties.Resources.ErrGettingInstalledPkgs2, BSky.GlobalResources.Properties.Resources.ErrorOccurred);
                logService.WriteToLogLevel("Error:", LogLevelEnum.Error, ex);
            }
            return installed;
        }

        private void LoadPackageHelp(string RPkgName)
        {
            OutputWindowContainer owc = (LifetimeService.Instance.Container.Resolve<IOutputWindowContainer>()) as OutputWindowContainer;
            BSkyMouseBusyHandler.ShowMouseBusy();

            string commands = "require(" + RPkgName + "); help(package='" + RPkgName + "'); ";

            SyntaxEditorWindow sewindow = LifetimeService.Instance.Container.Resolve<SyntaxEditorWindow>();
            sewindow.RunCommands(commands, null);
            sewindow.DisplayAllSessionOutput("R Package Help", (owc.ActiveOutputWindow as OutputWindow));
            BSkyMouseBusyHandler.HideMouseBusy();// HideProgressbar_old();// HideStatusProgressbar();//29Oct2014 

            //bring main window in front after file open, instead of output window
            Window1 window = LifetimeService.Instance.Container.Resolve<Window1>();
            window.Activate();
        }

        #endregion

    }
}
