using BlueSky.Commands.Tools.Package;
using BSky.ConfService.Intf.Interfaces;
using BSky.Controls;
using BSky.Interfaces.Commands;
using BSky.Interfaces.Interfaces;
using BSky.Lifetime;
using BSky.Lifetime.Interfaces;
using BSky.Statistics.Common;
using System;
using System.Collections.Generic;
using System.Globalization;
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

namespace BlueSky.Commands.File
{
    /// <summary>
    /// Interaction logic for LoadDatasetFromRPkgWindow.xaml
    /// </summary>
    public partial class LoadDatasetFromRPkgWindow : Window
    {
        ILoggerService logService = LifetimeService.Instance.Container.Resolve<ILoggerService>();
        IConfigService confService = LifetimeService.Instance.Container.Resolve<IConfigService>();

        public LoadDatasetFromRPkgWindow()
        {
            InitializeComponent();
            LoadInstalledRPkgNames();
        }

        //Load names of all (installed) R packages in the RpkgCombo Combobox
        private void LoadInstalledRPkgNames()
        {
            List<string> insPkgLst= GetListOfInstalledRPkgs();
            RpkgCombo.ItemsSource = insPkgLst;
        }

        //As soon as user select a R package, load all the dataset in the DatasetCombo Combobox
        private void LoadDatasetNames()
        {
            //char[] sep = new char[1];
            //sep[0] = '-';
            string rpkgname = RpkgCombo.SelectedValue as string;
            if (rpkgname == null)
                return;
            List<string> dsnamelist = GetListOfDatasetNamesInRPackage(rpkgname);
            DatasetCombo.ItemsSource = dsnamelist;

            //List<RPkgDatasetDetails> RpkgdsLst = new List<RPkgDatasetDetails>();
            //foreach (string s in dsnamelist)
            //{
            //    string[] parts = s.Split(sep);
            //    RPkgDatasetDetails rpkgds = new RPkgDatasetDetails() { DSName = parts[0], Title = parts[1] };
            //    RpkgdsLst.Add(rpkgds);
            //}
            //DatasetCombo.ItemsSource = RpkgdsLst;
        }

        //Load selected R pkg and then load the select dataset in the grid
        private void OKBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
            if (RpkgCombo.SelectedValue != null && RpkgCombo.SelectedValue != null)
            {
                string rpkgname = (RpkgCombo.SelectedValue as string).Trim();
                string datasetname = (DatasetCombo.SelectedItem as string).Trim();
                int idx = datasetname.IndexOf(" ");
                datasetname = datasetname.Substring(0, idx);
                LoadDatasetFromRPackage(rpkgname, datasetname);
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

        private List<string> GetListOfDatasetNamesInRPackage(string RPkgName)
        {
            List<string> PkgDatsetNames = new List<string>();
            try
            {
                PackageHelperMethods phm = new PackageHelperMethods();
                UAReturn r = phm.GetDatasetListFromRPkg(RPkgName);
                if (r != null && r.Success && r.SimpleTypeData != null)
                {
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
                        PkgDatsetNames.Add(s);
                }
                else
                {
                    if(r.Error!=null && r.Error.Length>1)
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
            return PkgDatsetNames;
        }

        private void LoadRPackage(string packagename)
        {
            try
            {
                if (string.IsNullOrEmpty(packagename))
                {
                    //MessageBox.Show("Title/Command cannot be empty, Exiting Dialog install", "Info: Dialog Title Empty.");
                    return;
                }

                PackageHelperMethods phm = new PackageHelperMethods();
                UAReturn r = phm.PackageFileLoad(packagename);// PackageFileLoad(packagename);
                if (r != null && r.Success)
                {
                    ////SendToOutputWindow(BSky.GlobalResources.Properties.Resources.LoadLibrary, r.CommandString, false);
                }
                else
                {
                    ////SendToOutputWindow(BSky.GlobalResources.Properties.Resources.ErrLoadingUsrSessionPkg, "", false);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(BSky.GlobalResources.Properties.Resources.ErrLoadingRPkg2, BSky.GlobalResources.Properties.Resources.ErrorOccurred);
                logService.WriteToLogLevel("Error:", LogLevelEnum.Error, ex);
            }
        }


        private void LoadDatasetFromRPackage(string RPkgName, string DSName)
        {
            OutputWindowContainer owc = (LifetimeService.Instance.Container.Resolve<IOutputWindowContainer>()) as OutputWindowContainer;
            //it was found in R-GUI that loading R pkg is not necessary to load dataset from it
            //LoadRPackage(RPkgName);
            BSkyMouseBusyHandler.ShowMouseBusy();
            string commands= "TmP=data('"+DSName+"', package='"+RPkgName+"');"+
                " BSkyLoadRefreshDataframe("+DSName+")";
            PrintDialogTitle("Open R package dataset");
            SyntaxEditorWindow sewindow = LifetimeService.Instance.Container.Resolve<SyntaxEditorWindow>();
            sewindow.RunCommands(commands, null);
            sewindow.DisplayAllSessionOutput("Open R package dataset", (owc.ActiveOutputWindow as OutputWindow));
            BSkyMouseBusyHandler.HideMouseBusy();// HideProgressbar_old();// HideStatusProgressbar();//29Oct2014 

            //08Apr2015 bring main window in front after file open, instead of output window
            Window1 window = LifetimeService.Instance.Container.Resolve<Window1>();
            window.Activate(); 
        }

        private void PrintDialogTitle(string title)
        {
            CommandOutput batch = new CommandOutput();
            batch.NameOfAnalysis = "Open R package dataset";
            batch.IsFromSyntaxEditor = false;

            string rcommcol = confService.GetConfigValueForKey("dctitlecol");//23nov2012 //before was syntitlecol
            byte red = byte.Parse(rcommcol.Substring(3, 2), NumberStyles.HexNumber);
            byte green = byte.Parse(rcommcol.Substring(5, 2), NumberStyles.HexNumber);
            byte blue = byte.Parse(rcommcol.Substring(7, 2), NumberStyles.HexNumber);
            Color c = Color.FromArgb(255, red, green, blue);
            AUParagraph aup = new AUParagraph();
            aup.Text = title; // dialogTitle;
            aup.FontSize = BSkyStyler.BSkyConstants.HEADER_FONTSIZE;//16;// before it was 16
            aup.FontWeight = FontWeights.DemiBold;
            aup.textcolor = new SolidColorBrush(c); //Colors.Blue);//SlateBlue //DogerBlue
            aup.ControlType = "Header";
            batch.Add(aup);
            SyntaxEditorWindow sewindow = LifetimeService.Instance.Container.Resolve<SyntaxEditorWindow>();
            sewindow.AddToSession(batch);
        }
        #endregion

        private void RpkgCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadDatasetNames();
        }

    }

    public class RPkgDatasetDetails
    {
        public string DSName { get; set; }
        public string Title { get; set; }

        public override string ToString()
        {
            return DSName + Title;
        }
    }
}
