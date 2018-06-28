using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BSky.Interfaces.DashBoard;
using BSky.Interfaces.Services;
using System.Threading;

namespace BSky.MenuGenerator
{
    public class StudentDialogContainer
    {
        //IBSkyLicService licService = LifetimeService.Instance.Container.Resolve<IBSkyLicService>();
        List<string> _dialoglist = new List<string>();

        public StudentDialogContainer()
        {
            CreateHardCodedDialogList();
        }

        private void CreateHardCodedDialogList()
        {
            string CultureName = Thread.CurrentThread.CurrentCulture.Name;//Test this for locale that is notsupported yet.
            string confDir = @"./Config/" + CultureName + "/";
            #region File menu
            _dialoglist.Add("New");
            _dialoglist.Add("Open");
            _dialoglist.Add("Close");
            _dialoglist.Add("Save");
            _dialoglist.Add("Save As");
            _dialoglist.Add("Save As Excel");
            _dialoglist.Add("---------");
            _dialoglist.Add("Recent");
            _dialoglist.Add("Exit");
            #endregion

            #region Data menu
            _dialoglist.Add(confDir + "Remove NAs.xaml");
            _dialoglist.Add(confDir + "Missing Values (Basic).xaml");
            _dialoglist.Add(confDir + "Missing Values (Formula).xaml");
            _dialoglist.Add(confDir + "Replace All Missing Values in Factor and String variables.xaml");

            _dialoglist.Add(confDir + "Compute.xaml");
            _dialoglist.Add(confDir + "Conditional Compute.xaml");

            _dialoglist.Add(confDir + "Bin Numeric Variables.xaml");
            _dialoglist.Add(confDir + "Recode.xaml");
            _dialoglist.Add(confDir + "Make Factor Variable.xaml");//12Apr2017
            _dialoglist.Add(confDir + "Transpose.xaml");
            _dialoglist.Add(confDir + "Transform.xaml");
            _dialoglist.Add(confDir + "Sample Dataset.xaml");

            _dialoglist.Add(confDir + "Standardize Variables.xaml");

            _dialoglist.Add(confDir + "Aggregate.xaml");
            _dialoglist.Add(confDir + "Aggregate (Outputs results to a table).xaml");
            _dialoglist.Add(confDir + "Subset.xaml");
            _dialoglist.Add(confDir + "Subset (outputs results to a table).xaml");
            _dialoglist.Add(confDir + "Merge Datasets.xaml");

            _dialoglist.Add(confDir + "Sort.xaml");

            _dialoglist.Add(confDir + "Reload Dataset from File.xaml");
            _dialoglist.Add("Refresh Grid");

            _dialoglist.Add(confDir + "Concatenate Multiple Variables (handling missing values).xaml");

            _dialoglist.Add(confDir + "Aggregate (legacy).xaml");
            _dialoglist.Add(confDir + "Subset (legacy).xaml");
            _dialoglist.Add(confDir + "Sort(legacy).xaml");
            #endregion

            #region  Analysis menu
            _dialoglist.Add(confDir + "Analysis of Missing Values.xaml");
            _dialoglist.Add(confDir + "Frequency Table.xaml");
            _dialoglist.Add(confDir + "Summary Statistics for All Variables.xaml");
            _dialoglist.Add(confDir + "Summary Statistics by Variable.xaml");
            _dialoglist.Add(confDir + "Summary Statistics by Group .xaml");
            _dialoglist.Add(confDir + "Numerical Statistical Analysis.xaml");
            _dialoglist.Add(confDir + "Factor Variable Analysis.xaml");
            _dialoglist.Add(confDir + "Correlation Test (One Variable).xaml");
            _dialoglist.Add(confDir + "Correlation Test (Multi-Variable).xaml");
            _dialoglist.Add(confDir + "Correlation Matrix.xaml");
            _dialoglist.Add(confDir + "Shapiro-Wilk Normality Test.xaml");
            _dialoglist.Add(confDir + "Q-Q plot.xaml");
            _dialoglist.Add(confDir + "BSkyOneSmTTest.xaml");
            _dialoglist.Add(confDir + "One Sample T-Test.xaml");
            _dialoglist.Add(confDir + "BSkyIndSmTTest.xaml");
            _dialoglist.Add(confDir + "Independent Samples T-Test.xaml");
            _dialoglist.Add(confDir + "Independent Samples T-Test by Factor.xaml");
            _dialoglist.Add(confDir + "Paired T-Test.xaml");
            _dialoglist.Add(confDir + "One Way Anova.xaml");
            _dialoglist.Add(confDir + "Multi Way Anova.xaml");
            _dialoglist.Add(confDir + "Single Sample Proportion Test.xaml");
            _dialoglist.Add(confDir + "Single Sample Exact Binomial Proportion Test.xaml");
            _dialoglist.Add(confDir + "Two Sample Proportion Test.xaml");
            _dialoglist.Add(confDir + "Two Variance F-Test.xaml");
            _dialoglist.Add(confDir + "Bartletts Test.xaml");
            _dialoglist.Add(confDir + "Levene Test.xaml");
            _dialoglist.Add(confDir + "Two Sample Wilcoxon Test.xaml");
            _dialoglist.Add(confDir + "Paired Wilcoxon Test.xaml");
            _dialoglist.Add(confDir + "Friedman Test.xaml");
            _dialoglist.Add(confDir + "Kruskal Wallis Test.xaml");
            _dialoglist.Add(confDir + "Chisq Test.xaml");
            _dialoglist.Add(confDir + "Two-Way Crosstab.xaml");
            _dialoglist.Add(confDir + "KMeans Cluster.xaml");

            _dialoglist.Add(confDir + "Plot TimeSeries (with Correlations).xaml");
            _dialoglist.Add(confDir + "Plot Time Series(separate or combined).xaml");
            _dialoglist.Add(confDir + "Automated Arima.xaml");
            _dialoglist.Add(confDir + "Exponential Smoothing.xaml");
            _dialoglist.Add(confDir + "Holt Winters Seasonal.xaml");
            _dialoglist.Add(confDir + "Non Seasonal Holt Winters.xaml");



            #endregion

            #region Graphics menu
            _dialoglist.Add(confDir + "Plot.xaml");
            _dialoglist.Add(confDir + "Histogram.xaml");
            _dialoglist.Add(confDir + "BoxPlot.xaml");
            _dialoglist.Add(confDir + "Stem and Leaf Plot.xaml");
            _dialoglist.Add(confDir + "Plot of Means.xaml");
            _dialoglist.Add(confDir + "Strip Plot.xaml");
            _dialoglist.Add(confDir + "3D Scatterplot.xaml");
            _dialoglist.Add(confDir + "Bar Graph.xaml");
            _dialoglist.Add(confDir + "Density.xaml");
            _dialoglist.Add(confDir + "Density Counts.xaml");
            _dialoglist.Add(confDir + "Strip Chart.xaml");
            _dialoglist.Add(confDir + "Scatterplot.xaml");
            _dialoglist.Add(confDir + "Boxplot (lists outliers).xaml");
            _dialoglist.Add(confDir + "Line Chart.xaml");

            _dialoglist.Add(confDir + "Histogram (legacy).xaml");
            _dialoglist.Add(confDir + "Plot of Means (legacy).xaml");
            _dialoglist.Add(confDir + "Scatterplot (legacy).xaml");
            _dialoglist.Add(confDir + "Strip Plot (legacy).xaml");
            #endregion

            #region Split menu
            _dialoglist.Add(confDir + "Simple Split.xaml");
            _dialoglist.Add(confDir + "Stratified Sample.xaml");
            #endregion

            #region Model Fitting menu
            _dialoglist.Add(confDir + "Linear Regression.xaml");
            _dialoglist.Add(confDir + "Set Contrasts.xaml");
            _dialoglist.Add(confDir + "Display Contrasts.xaml");
            _dialoglist.Add(confDir + "Decision Trees.xaml");
            #endregion

            #region Model Statistics menu
            _dialoglist.Add(confDir + "Stepwise.xaml");
            _dialoglist.Add(confDir + "AIC.xaml");
            _dialoglist.Add(confDir + "BIC.xaml");
            _dialoglist.Add(confDir + "Confidence Interval.xaml");
            _dialoglist.Add(confDir + "Variance Inflation Factors.xaml");
            _dialoglist.Add(confDir + "Bonferroni Outlier Test.xaml");
            #endregion

            #region Score button
            _dialoglist.Add(confDir + "Make Predictions.xaml");
            #endregion

            #region Tools -> Package menu
            _dialoglist.Add(confDir + "R version details.xaml");
            _dialoglist.Add(confDir + "R Package Details.xaml");
            _dialoglist.Add("Show installed packages");
            _dialoglist.Add("Show currently loaded packages");
            _dialoglist.Add("Update BlueSky package from zip (Restart App)");
            _dialoglist.Add(confDir + "Install ALL required R packages from CRAN.xaml");// ("Install required package(s) from CRAN");
            //_dialoglist.Add("Install required package(s) from a folder");
            _dialoglist.Add("Install package(s) from zipped file(s)");
            _dialoglist.Add("Install/Update package(s) from CRAN");
            _dialoglist.Add("Load package(s)");
            _dialoglist.Add("Load user session package(s)");
            _dialoglist.Add("Unload package(s)");
            _dialoglist.Add("Uninstall package(s)");
            _dialoglist.Add("Configuration Settings");
            #endregion

            #region Help Menu
            _dialoglist.Add("Licensing");
            _dialoglist.Add("Visit BlueSky Website");
            _dialoglist.Add("About");
            #endregion


        }

        public bool ContainsDialog(DashBoardItem di)
        {
            UAMenuCommand uamc = (UAMenuCommand)di.CommandParameter; //BlueSky.Services.UAMenuCommand
            string name = (uamc.commandtemplate != null && uamc.commandtemplate.Length > 1) ? uamc.commandtemplate : uamc.text;
            if (_dialoglist.Contains(name))
                return true;
            else
                return false;
        }

        //private void LicCheck()
        //{
        //    if (!licService.ValidLic)//no trial not even full
        //    {
        //        MessageBox.Show("You do not have a valid license to run this application. For help, go to Help -> Licensing.", "No Valid License", MessageBoxButton.OK, MessageBoxImage.Stop);
        //        //refresh lic status in dataset main window 
        //        appWindow.setLicInfoInStatusBar(licService.LicMessage);
        //        return;
        //    }

        //    //refresh Dataset window status bar for license info.
        //    if (licService.hasLicStatusChanged)
        //    {
        //        //refresh lic status in dataset main window 
        //        appWindow.setLicInfoInStatusBar(licService.LicMessage);
        //    }
        //}
    }
}
