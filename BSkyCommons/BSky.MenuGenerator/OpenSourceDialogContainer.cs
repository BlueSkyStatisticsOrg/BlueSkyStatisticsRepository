using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BSky.Interfaces.DashBoard;
using BSky.Interfaces.Services;

namespace BSky.MenuGenerator
{
    public class OpenSourceDialogContainer
    {
        //IBSkyLicService licService = LifetimeService.Instance.Container.Resolve<IBSkyLicService>();
        List<string> _dialoglist = new List<string>();

        public OpenSourceDialogContainer()
        {
            CreateHardCodedDialogList();
        }

        private void CreateHardCodedDialogList()
        {
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
            _dialoglist.Add(@"./Config/Remove NAs.xaml");
            _dialoglist.Add(@"./Config/Missing Values (Basic).xaml");
            _dialoglist.Add(@"./Config/Missing Values (Formula).xaml");
            _dialoglist.Add(@"./Config/Replace All Missing Values in Factor and String variables.xaml");

            _dialoglist.Add(@"./Config/Compute.xaml");
            _dialoglist.Add(@"./Config/Conditional Compute.xaml");

            _dialoglist.Add(@"./Config/Bin Numeric Variables.xaml");
            _dialoglist.Add(@"./Config/Recode.xaml");
            _dialoglist.Add(@"./Config/Make Factor Variable.xaml");//12Apr2017
            _dialoglist.Add(@"./Config/Transpose.xaml");
            _dialoglist.Add(@"./Config/Transform.xaml");
            _dialoglist.Add(@"./Config/Sample Dataset.xaml");

            _dialoglist.Add(@"./Config/Delete Variables.xaml");

            _dialoglist.Add(@"./Config/Standardize Variables.xaml");

            _dialoglist.Add(@"./Config/Aggregate.xaml");
            _dialoglist.Add(@"./Config/Aggregate (Outputs results to a table).xaml");
            _dialoglist.Add(@"./Config/Subset.xaml");
            _dialoglist.Add(@"./Config/Subset (outputs results to a table).xaml");
            _dialoglist.Add(@"./Config/Merge Datasets.xaml");

            _dialoglist.Add(@"./Config/Sort.xaml");

            _dialoglist.Add(@"./Config/Reload Dataset from File.xaml");
            _dialoglist.Add("Refresh Grid");

            _dialoglist.Add(@"./Config/Concatenate Multiple Variables (handling missing values).xaml");

            _dialoglist.Add(@"./Config/Aggregate (legacy).xaml");
            _dialoglist.Add(@"./Config/Subset (legacy).xaml");
            _dialoglist.Add(@"./Config/Sort(legacy).xaml");
            #endregion

            #region  Analysis menu
            _dialoglist.Add(@"./Config/Analysis of Missing Values.xaml");
            _dialoglist.Add(@"./Config/Frequency Table.xaml");
            _dialoglist.Add(@"./Config/Summary Statistics for All Variables.xaml");
            _dialoglist.Add(@"./Config/Summary Statistics by Variable.xaml");
            _dialoglist.Add(@"./Config/Summary Statistics by Group .xaml");
            _dialoglist.Add(@"./Config/Numerical Statistical Analysis.xaml");
            _dialoglist.Add(@"./Config/Factor Variable Analysis.xaml");
            _dialoglist.Add(@"./Config/Correlation Test (One Variable).xaml");
            _dialoglist.Add(@"./Config/Correlation Test (Multi-Variable).xaml");
            _dialoglist.Add(@"./Config/Correlation Matrix.xaml");
            _dialoglist.Add(@"./Config/Shapiro-Wilk Normality Test.xaml");
			_dialoglist.Add(@"./Config/Q-Q plot.xaml");
            _dialoglist.Add(@"./Config/BSkyOneSmTTest.xaml");
            _dialoglist.Add(@"./Config/One Sample T-Test.xaml");
            _dialoglist.Add(@"./Config/BSkyIndSmTTest.xaml");
            _dialoglist.Add(@"./Config/Independent Samples T-Test.xaml");
            _dialoglist.Add(@"./Config/Independent Samples T-Test by Factor.xaml");
            _dialoglist.Add(@"./Config/Paired T-Test.xaml");
            _dialoglist.Add(@"./Config/One Way Anova.xaml");
            _dialoglist.Add(@"./Config/Multi Way Anova.xaml");

            _dialoglist.Add(@"./Config/One Way Anova with Blocks.xaml");
            _dialoglist.Add(@"./Config/One Way Anova with Random Blocks.xaml");

            _dialoglist.Add(@"./Config/Single Sample Proportion Test.xaml");
            _dialoglist.Add(@"./Config/Single Sample Exact Binomial Proportion Test.xaml");
            _dialoglist.Add(@"./Config/Two Sample Proportion Test.xaml");
            _dialoglist.Add(@"./Config/Two Variance F-Test.xaml");
            _dialoglist.Add(@"./Config/Bartletts Test.xaml");
            _dialoglist.Add(@"./Config/Levene Test.xaml");
            _dialoglist.Add(@"./Config/Two Sample Wilcoxon Test.xaml");
            _dialoglist.Add(@"./Config/Paired Wilcoxon Test.xaml");
            _dialoglist.Add(@"./Config/Friedman Test.xaml");
            _dialoglist.Add(@"./Config/Kruskal Wallis Test.xaml");
            _dialoglist.Add(@"./Config/Chisq Test.xaml");
            _dialoglist.Add(@"./Config/Two-Way Crosstab.xaml");
            _dialoglist.Add(@"./Config/KMeans Cluster.xaml");

            _dialoglist.Add(@"./Config/Factor Analysis.xaml");
            _dialoglist.Add(@"./Config/Principal Component Analysis.xaml");

            _dialoglist.Add(@"./Config/Reliability Analysis.xaml");
            #endregion

            #region Graphics menu
            _dialoglist.Add(@"./Config/Plot.xaml");
            _dialoglist.Add(@"./Config/Histogram.xaml");
            _dialoglist.Add(@"./Config/BoxPlot.xaml");
            _dialoglist.Add(@"./Config/Stem and Leaf Plot.xaml");
            _dialoglist.Add(@"./Config/Plot of Means.xaml");
            _dialoglist.Add(@"./Config/Strip Plot.xaml");
            _dialoglist.Add(@"./Config/3D Scatterplot.xaml");
            _dialoglist.Add(@"./Config/Bar Graph.xaml");
            _dialoglist.Add(@"./Config/Density.xaml");
            _dialoglist.Add(@"./Config/Density Counts.xaml");
            _dialoglist.Add(@"./Config/Strip Chart.xaml");
            _dialoglist.Add(@"./Config/Scatterplot.xaml");
            _dialoglist.Add(@"./Config/Boxplot (lists outliers).xaml");
            _dialoglist.Add(@"./Config/Line Chart.xaml");

            _dialoglist.Add(@"./Config/Frequency Charts (numeric).xaml");
            _dialoglist.Add(@"./Config/Frequency Charts(factor).xaml");

            _dialoglist.Add(@"./Config/Histogram (legacy).xaml");
            _dialoglist.Add(@"./Config/Plot of Means (legacy).xaml");
            _dialoglist.Add(@"./Config/Scatterplot (legacy).xaml");
            _dialoglist.Add(@"./Config/Strip Plot (legacy).xaml");
#endregion

#region Split menu
            _dialoglist.Add(@"./Config/Simple Split.xaml");
            _dialoglist.Add(@"./Config/Stratified Sample.xaml");
#endregion

#region Model Fitting menu
            _dialoglist.Add(@"./Config/Linear Regression.xaml");
            _dialoglist.Add(@"./Config/GLZM.xaml");
            _dialoglist.Add(@"./Config/Naive Bayes.xaml");
            _dialoglist.Add(@"./Config/Set Contrasts.xaml");
            _dialoglist.Add(@"./Config/Display Contrasts.xaml");
			_dialoglist.Add(@"./Config/Decision Trees.xaml");
            _dialoglist.Add(@"./Config/KNN.xaml");
            #endregion

            #region Model Statistics menu
            _dialoglist.Add(@"./Config/Stepwise.xaml");
            _dialoglist.Add(@"./Config/AIC.xaml");
			_dialoglist.Add(@"./Config/BIC.xaml");
            _dialoglist.Add(@"./Config/Confidence Interval.xaml");
            _dialoglist.Add(@"./Config/Variance Inflation Factors.xaml");
			_dialoglist.Add(@"./Config/Bonferroni Outlier Test.xaml");
#endregion

#region Score button
            _dialoglist.Add(@"./Config/Make Predictions.xaml");
#endregion

#region Tools -> Package menu
            _dialoglist.Add(@"./Config/R version details.xaml");
            _dialoglist.Add(@"./Config/R Package Details.xaml");
            _dialoglist.Add("Show installed packages");
            _dialoglist.Add("Show currently loaded packages");
            _dialoglist.Add("Update BlueSky package from zip (Restart App)");
            _dialoglist.Add(@"./Config/Install ALL required R packages from CRAN.xaml");// ("Install required package(s) from CRAN");
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
            _dialoglist.Add("Visit BlueSky Website");
            _dialoglist.Add("About");
#endregion


        }

        public bool ContainsDialog(DashBoardItem di)
        {
            UAMenuCommand uamc = (UAMenuCommand)di.CommandParameter; //BlueSky.Services.UAMenuCommand
            string name = (uamc.commandtemplate!=null && uamc.commandtemplate.Length>1) ? uamc.commandtemplate : uamc.text;
            if( _dialoglist.Contains(name))
                return true;
            else
                return false;
        }

    }
}
