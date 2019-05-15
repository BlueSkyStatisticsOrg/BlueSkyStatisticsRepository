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
            string nodataset = "If you do not see a dataset in the dropdown, the package does not contain one or more dataset(s).";
            string nopkg = "The package is not installed. To install the package, see help above.";
            List<string> dsnamelist = new List<string>();
            //char[] sep = new char[1];
            //sep[0] = '-';
            string rpkgname = RpkgCombo.SelectedValue as string;
            if (rpkgname != null)
            {
                dsnamelist = GetListOfDatasetNamesInRPackage(rpkgname);
                //DatasetCombo.ItemsSource = dsnamelist;
            }
            DatasetCombo.ItemsSource = dsnamelist;

            //set proper message
            if (rpkgname == null)
            {
                status.Text = nopkg;
            }
            else if(dsnamelist.Count < 1)
            {
                status.Text = nodataset;
            }
            //List<RPkgDatasetDetails> RpkgdsLst = new List<RPkgDatasetDetails>();
            //foreach (string s in dsnamelist)
            //{
            //    string[] parts = s.Split(sep);
            //    RPkgDatasetDetails rpkgds = new RPkgDatasetDetails() { DSName = parts[0], Title = parts[1] };
            //    RpkgdsLst.Add(rpkgds);
            //}
            //DatasetCombo.ItemsSource = RpkgdsLst;
            if (rpkgname==null || dsnamelist.Count < 1)
                status.Visibility = Visibility.Visible;
            else
                status.Visibility = Visibility.Collapsed;
        }

        //Load selected R pkg and then load the select dataset in the grid
        private void OKBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
            if (RpkgCombo.SelectedValue != null && RpkgCombo.SelectedValue != null)
            {
                string rpkgname = (RpkgCombo.SelectedValue as string).Trim();
                string selectedItem = (DatasetCombo.SelectedItem as string).Trim();
                //NOTE: it was found that if datasetname has two parts like, bock.table (bock)
                //then in this case dataset-name will be 'bock' which should be passed to util::data()
                //and the part that is outside parenthesis i.e. bock.table is the dataset object(actual dataset)
                //If there are no two part in datasetname (e.g. income) then dataset-name to be passed 
                //in data() is 'income' as well as the dataset-object is also 'income'.
                //Above datasets are from 'psych' R package.

                string datasetname = string.Empty;// say bock
                string datasetobjname = string.Empty;//say bock.table
                int idx = selectedItem.IndexOf("-["); //format is : "dataset-obj (dataset-name) -[Description]"
                string datasetObjAndName = selectedItem.Substring(0, idx).Trim();
                int parenopenidx = datasetObjAndName.IndexOf("(");
                int parencloseidx = datasetObjAndName.IndexOf(")");
                if (datasetObjAndName.Contains(" ") && parenopenidx > 0 && parencloseidx > 0)//two part name e.g. "dataset-obj (dataset-name)"
                {
                    datasetname = datasetObjAndName.Substring(parenopenidx + 1, (parencloseidx - parenopenidx - 1)).Trim();
                    datasetobjname = datasetObjAndName.Substring(0, parenopenidx).Trim();
                }
                else
                {
                    datasetname = datasetObjAndName;
                    datasetobjname = datasetObjAndName;
                }
                LoadDatasetFromRPackage(rpkgname, datasetname, datasetobjname);
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


        private void LoadDatasetFromRPackage(string RPkgName, string DSName, string DSObj)
        {
            OutputWindowContainer owc = (LifetimeService.Instance.Container.Resolve<IOutputWindowContainer>()) as OutputWindowContainer;
            //it was found in R-GUI that loading R pkg is not necessary to load dataset from it
            //LoadRPackage(RPkgName);
            BSkyMouseBusyHandler.ShowMouseBusy();
            //string commands = "TmP=data('" + DSName + "', package='" + RPkgName + "');" +
            //    "eval(parse(text = paste('"+ DSObj + "<- as.data.frame( "+DSObj+")', sep = '')));" +
            //    " BSkyLoadRefreshDataframe("+ DSObj + ")";

            string commands = "BSkyLoadRpkgDataset('"+ DSName+"','"+ DSObj + "','"+RPkgName+"'); "+ 
                "BSkyLoadRefreshDataframe(" + DSObj + ")";
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

        private void RpkgCombo_KeyUp(object sender, KeyEventArgs e)
        {
            if ((e.Key >= Key.D0 && e.Key <= Key.D9) ||
                (e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9) ||
                (e.Key >= Key.A && e.Key <= Key.Z) ||
                e.Key == Key.Decimal || e.Key == Key.Separator
                )
            {
                LoadDatasetNames();
            }
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
