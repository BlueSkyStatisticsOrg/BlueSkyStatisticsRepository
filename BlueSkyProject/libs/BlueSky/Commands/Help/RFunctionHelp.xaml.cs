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
using System.Threading;
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
    /// Interaction logic for RFunctionHelp.xaml
    /// </summary>
    public partial class RFunctionHelp : Window
    {
        ILoggerService logService = LifetimeService.Instance.Container.Resolve<ILoggerService>();
        IConfigService confService = LifetimeService.Instance.Container.Resolve<IConfigService>();
        public RFunctionHelp()
        {
            InitializeComponent();
            LoadInstalledRPkgNames();
            //LoadfunctionNames(true);//load datasets from all R installed packages at launch.
        }


        //Load names of all (installed) R packages in the RpkgCombo Combobox
        private void LoadInstalledRPkgNames()
        {
            List<string> insPkgLst = GetListOfInstalledRPkgs();
            RpkgCombo.ItemsSource = insPkgLst;
        }

        //As soon as user select a R package, load all the function in the FunctionCombo Combobox
        private void LoadfunctionNames(bool loadFrmAllPkgs = false)
        {
            string nofunctions = "Functions not found.";
            string nopkg = "R Package not found. You can install a new R package from CRAN";
            string nopkg2 = "Go to: Tools > Package > Install/Update package(s) from CRAN";
            List<string> dsnamelist = new List<string>();
            List<string> PkgDatsetDetailList = null;
            FunctionCombo.ItemsSource = null;
            //char[] sep = new char[1];
            //sep[0] = '-';
            string rpkgname = string.Empty;
            if (!loadFrmAllPkgs)
                rpkgname = RpkgCombo.SelectedValue as string; //if user typed item is in the combo then only rpkgname is not null.

            if (rpkgname != null && rpkgname.Trim().Length>0)
            {
                PkgDatsetDetailList = GetFunctionNamesInRPackage(rpkgname);
            }
            if (PkgDatsetDetailList == null)
            {
                PkgDatsetDetailList = new List<string>();
            }
            FunctionCombo.ItemsSource = PkgDatsetDetailList;// function name list.

            //set proper message
            if (rpkgname == null)//user typed garbage text that is not in combo
            {
                status.Text = nopkg+"\n"+nopkg2;
            }
            //else if (PkgDatsetDetailList.Count < 1)
            //{
            //    status.Text = nofunctions;
            //}
            if (rpkgname == null)// || PkgDatsetDetailList.Count < 1)
                status.Visibility = Visibility.Visible;
            else
                status.Visibility = Visibility.Collapsed;
        }

        //Load selected R pkg and then load the select dataset in the grid
        private void OKBtn_Click(object sender, RoutedEventArgs e)
        {
            string rpkgname = string.Empty;
            if (RpkgCombo.SelectedValue != null)
                rpkgname = (RpkgCombo.SelectedValue as string).Trim();
            string functionname = string.Empty;

            if (FunctionCombo.SelectedValue != null)//user selected from combo. Priority 1
            {
                functionname = (FunctionCombo.SelectedItem as string).Trim();
            }
            else if (!String.IsNullOrEmpty(FunctionCombo.Text))//user typed
            {
                functionname = FunctionCombo.Text;
            }
            else
            {
                return;
            }

            if (!String.IsNullOrEmpty(functionname))
            {
                GetFunctionHelp(rpkgname, functionname);
                this.Close();
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
            //installed.Add("");//adding blank so that if the combo field is made blank
            ////it does not say "package not found"
            //// When no value in the combo matches with
            ////what user typed, then the selectedvalue of combo is null. So if we add a blank to the combo
            ////and user deletes the field text, then the blank entry will be selected from the combo.
            ////and selectedvalue will not be null (it will be blank though) and will not produce an error 
            ////'pkg not found' because this msg comes when field text is not in the combo.
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

        private List<string> GetFunctionNamesInRPackage(string RPkgName)
        {
            List<string> PkgDatsetNames = new List<string>();
            List<string> PkgFunctionNames = null;
            try
            {
                PackageHelperMethods phm = new PackageHelperMethods();
                UAReturn r = phm.GetFunctionnamesFromRPkg(RPkgName);
                if (r != null && r.Success && r.SimpleTypeData != null)
                {
                    PkgFunctionNames = r.SimpleTypeData as List<string>;
                }
                else
                {
                    if (r.Error != null && r.Error.Length > 1)
                        MessageBox.Show(r.Error);
                    ////SendToOutputWindow(BSky.GlobalResources.Properties.Resources.ErrGettingInstalledPkgs, "", false);
                }
            }
            catch (Exception ex)
            {
                //MessageBox.Show(BSky.GlobalResources.Properties.Resources.ErrGettingInstalledPkgs2, BSky.GlobalResources.Properties.Resources.ErrorOccurred);
                MessageBox.Show(ex.Message);
                logService.WriteToLogLevel("Error:", LogLevelEnum.Error, ex);
            }
            return PkgFunctionNames;//PkgDatsetNames;
        }

        private void GetFunctionHelp(string RPkgName, string funcname)
        {
            OutputWindowContainer owc = (LifetimeService.Instance.Container.Resolve<IOutputWindowContainer>()) as OutputWindowContainer;
            BSkyMouseBusyHandler.ShowMouseBusy();
            string commands = string.Empty;

            if (RPkgName != null && RPkgName.Length > 0)
                commands = "require(" + RPkgName + "); help('" + funcname + "'); ";
            else
                commands = "help('" + funcname + "'); ";
            SyntaxEditorWindow sewindow = LifetimeService.Instance.Container.Resolve<SyntaxEditorWindow>();
            sewindow.RunCommands(commands, null);
            sewindow.DisplayAllSessionOutput("R Function Help", (owc.ActiveOutputWindow as OutputWindow));
            BSkyMouseBusyHandler.HideMouseBusy();// HideProgressbar_old();// HideStatusProgressbar();//29Oct2014 

            //bring main window in front after file open, instead of output window
            Window1 window = LifetimeService.Instance.Container.Resolve<Window1>();
            window.Activate();
        }
        #endregion

        private void RpkgCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //Thread.Sleep(1000);
            //LoadfunctionNames();
        }

        private void RpkgCombo_KeyUp(object sender, KeyEventArgs e)
        {
            if ((e.Key >= Key.D0 && e.Key <= Key.D9) ||
                (e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9) ||
                (e.Key >= Key.A && e.Key <= Key.Z) ||
                e.Key == Key.Decimal || e.Key == Key.Separator
                )
            {
                LoadfunctionNames();
            }
        }

        private void RpkgCombo_LostFocus(object sender, RoutedEventArgs e)
        {
            LoadfunctionNames();
        }

        private void RpkgCombo_GotFocus(object sender, RoutedEventArgs e)
        {
            status.Visibility = Visibility.Collapsed;
        }
    }
}
