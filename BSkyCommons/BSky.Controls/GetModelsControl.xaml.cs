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
using System.Windows.Navigation;
using System.Windows.Shapes;
using BSky.XmlDecoder;
using BSky.Interfaces.Interfaces;
using BSky.Lifetime;
using BSky.Statistics.Service.Engine.Interfaces;
using System.Collections.ObjectModel;
using Microsoft.Win32;
using BSky.Statistics.Common;
using BSky.Interfaces.Services;
using BSky.Interfaces.DashBoard;
using BSky.Lifetime.Interfaces;

namespace BSky.Controls
{
    /// <summary>
    /// Interaction logic for GetModelsControl.xaml
    /// </summary>
    public partial class GetModelsControl : UserControl
    {
        ILoggerService logService = LifetimeService.Instance.Container.Resolve<ILoggerService>();//17Dec2012
        IUIController UIController;

        public GetModelsControl(DashBoardItem item)
        {
            InitializeComponent();
            UIController = LifetimeService.Instance.Container.Resolve<IUIController>();
            RefreshModelClassList();

            //binding score dialog (Make Predictions) to score button
            scoreButton.Command = item.Command;
            scoreButton.CommandParameter = item.CommandParameter;

            //set initial value
            //classtypecombo.SelectedIndex = 0; //18Sep2017 Bsky crashes during launch if you do this here
        }
        string allmodels = "All_Models";

        public string Model { get; set; }

        public void setDefaultModel() //to set All_Models as selected item
        {
            classtypecombo.SelectedIndex = 0;
        }

        #region Events


        private void modelnamescombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //set global %MODEL%
            Model = modelnamescombo.SelectedItem as string;
            //MessageBox.Show("Selected model =" + Model);
            UIController.ActiveModel = Model;
        }

        //clicking on modelname combo will refresh the list to capture new models, if any.
        private void modelnamescombo_DropDownOpened(object sender, EventArgs e)
        {
            RefreshModelNames();
        }

        private void classtypecombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RefreshModelNames();
        }


        //Class Type combo when clicked should refresh the Model combo rather than just refreshing when selection is made.
        //Explaination: Say sometimes you may have "lm" selected. Its selected so you can't select it again to refresh your models combo
        // Workaround would be to select something else from class combo (say "gml") and then select "lm" back again.
        //To fix this issue in a right way we can refresh model combo whenever class combo is clicked and also whenever selections is changed.
        // selection changed event is already there(abv), we just need an event for class combo click.
        private void classtypecombo_DropDownOpened(object sender, EventArgs e)
        {
            RefreshModelNames();
        }

        private void RefreshModelNames()
        {
            string selectedclass = classtypecombo.SelectedItem as string;

            if (selectedclass != null && selectedclass.Trim().Length > 0 && !selectedclass.Equals(allmodels))
            {
                //here you can create a R command
                IAnalyticsService analyticServ = LifetimeService.Instance.Container.Resolve<IAnalyticsService>();
                object ob = analyticServ.GetAllModels(selectedclass);
                if (ob != null)
                {
                    string[] modelnames = ob as string[];

                    ObservableCollection<string> items = new ObservableCollection<string>();
                    foreach (string s in modelnames)
                    {
                        items.Add(s);
                    }
                    modelnamescombo.ItemsSource = items;// modelnames;
                }

                //set first item in second dropdown to be selected
                if (modelnamescombo.Items.Count > 0)
                    modelnamescombo.SelectedIndex = 0;
            }
            else if (selectedclass != null && selectedclass.Equals(allmodels))//17Sep2017
            {
                ObservableCollection<string> allmodelnames = getModelClasses();//get all model calsses
                ObservableCollection<string> items = new ObservableCollection<string>();
                //here you can create a R command
                IAnalyticsService analyticServ = LifetimeService.Instance.Container.Resolve<IAnalyticsService>();

                object ob = null;
                foreach (string modname in allmodelnames)
                {
                    if (modname.Equals(allmodels))
                        continue;
                    ob = analyticServ.GetAllModels(modname);
                    if (ob != null)
                    {
                        string[] modelnames = ob as string[];
                        foreach (string s in modelnames)
                        {
                            if (!items.Contains(s))//if item is not already present
                                items.Add(s);
                        }
                    }
                }
                modelnamescombo.ItemsSource = items;// modelnames;

                //set first item in second dropdown to be selected
                if (modelnamescombo.Items.Count > 0)
                    modelnamescombo.SelectedIndex = 0;
            }
            else //Not sure what to do when user selects nothing from the first box. This ELSE may not be required at all.
            {
                modelnamescombo.ItemsSource = null;
            }
        }

        #endregion

        XMLitemsProcessor modelclassnameList = LifetimeService.Instance.Container.Resolve<XMLitemsProcessor>("modelClasses");//11Sep2016
        private ObservableCollection<string> getModelClasses()
        {
            List<string> li = modelclassnameList.RecentFileList;//trying to fetch from /Config/ModelClasses.xml

            ObservableCollection<string> items = new ObservableCollection<string>();

            //addiing item 'All Models' so that all models can also be seen together. Basically no filtering.
            if (!items.Contains(allmodels))//if item is not already present
                items.Add(allmodels);

            foreach (string s in li)
            {
                if(!items.Contains(s))//if item is not already present
                    items.Add(s);
            }

            if (items == null || items.Count == 0)//generated default hard coded model class names if .xml does not have any
            {
               //Default hard coded list
                items.Add("lm");
                items.Add("glm");
                items.Add("randomForest");
            }
            return items;
        }

        //This public method is not only called from the constructor here, but it must also be called when user modifies the 
        // list (of model class names) in the CustomSettigsWindow -> ModelsTab
        public void RefreshModelClassList()
        {
            classtypecombo.ItemsSource = getModelClasses();

            //if you try following, BSky app crashes because some object are still not present. I reconfirmed craash on 17Sep2017.
            //so nothing should be selected automatically for first dropdown. clicking on it will do refresh.
            //classtypecombo.SelectedIndex = 0; 
        }


        #region Save/Load models 

        public const String FileNameFilter = "R Obj (*.RData)|*.RData";

        //NOTE the model will get loaded in the R memory but will only be visible in the model dropdown(second dropdown) if 
        // the matching class is selected from the 'class' dropdown (first dropdown)
        private void loadmodelButton_Click(object sender, RoutedEventArgs e)
        {
            IAnalyticsService analyticServ = LifetimeService.Instance.Container.Resolve<IAnalyticsService>();
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = FileNameFilter;
            //Window1 appwin = LifetimeService.Instance.Container.Resolve<Window1>();
            bool? output = ofd.ShowDialog();//ShowDialog(appwin);//Application.Current.MainWindow);
            if (output.HasValue && output.Value)
            {
                try
                {
                    string mdlclass = string.Empty;
                    string mdlname = string.Empty;
                    UAReturn uar = analyticServ.LoadRObjs(ofd.FileName);
                    if (uar!= null && uar.SimpleTypeData!=null)
                    {
                        object ob = uar.SimpleTypeData;
                        if(ob.GetType().Name.Equals("String[]"))
                        {
                            string[] vals = ob as string[];
                            int vcount = vals.Count();
                            if (vcount >= 2)
                            {
                                mdlname = vals[0];
                                mdlclass = vals[1];

                                //Find if the model class is present in the modelclass dropdown. 
                                //IF not show message about, how to add it from configuration
                                if (classtypecombo.Items.IndexOf(mdlclass) < 0)//Model not in dropdown
                                {
                                    string s0 = "Loaded R Object Details:\nName: " + mdlname + "\nClass: " + mdlclass+"\n\n";
                                    string s1 = "'" + mdlclass + "' class not found in the 'Select Model Type' dropdown in the main window toolbar.\nIf it is a valid model class then add it to the models list.\n\n";
                                    string s2 = "To add a new model class go to:\n\nOptions -> Configuration Settings -> Models tab, and add it.";
                                    string s3 = "\nNOTE: Type exact class name, it is case-sensitive.";
                                    string s4 = "\n\nAfter adding the model, you can select it from the 'Select Model Type' dropdown in the main window toolbar.";
                                    string s5 = "\nAfter selecting the model type, select model using the 'Select Model' dropdown.";
                                    MessageBox.Show(s0 + s1 + s2+s3+s4+s5, "Model class not found", MessageBoxButton.OK, MessageBoxImage.Warning);
                                }
                                else
                                {
                                    //Now refresh model dropdowns with newly loaded values
                                    classtypecombo.SelectedValue = (mdlclass);
                                    modelnamescombo.SelectedValue = (mdlname);
                                }
                            }
                        }
 
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error! " + ex.Message, "Error occurred while loading the model", MessageBoxButton.OK, MessageBoxImage.Error);
                }

                //set first item in second dropdown to be selected
                if (modelnamescombo.Items.Count > 0)
                    modelnamescombo.SelectedIndex = 0;
            }

        }

        private void savemodelButton_Click(object sender, RoutedEventArgs e)
        {
            IAnalyticsService analyticServ = LifetimeService.Instance.Container.Resolve<IAnalyticsService>();
            //set global %MODEL%
            Model = modelnamescombo.SelectedItem as string;
            //MessageBox.Show("Selected model =" + Model);

            if (Model == null || Model.Trim().Length < 1)//No meodel selected
            {
                MessageBoxResult mbr1 = MessageBox.Show("Please select a model from the dropdown.", "No Model Selected", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            MessageBoxResult mbr = MessageBox.Show("Currently selected model(ie. "+Model+") will be saved.", "Save Current Model?", MessageBoxButton.OKCancel, MessageBoxImage.Question);
            if (mbr == MessageBoxResult.Cancel)
                return;

            Microsoft.Win32.SaveFileDialog sfd = new Microsoft.Win32.SaveFileDialog();
            sfd.Filter = FileNameFilter;
            //Window1 appwin = LifetimeService.Instance.Container.Resolve<Window1>();
            bool? output = sfd.ShowDialog();// ShowDialog(appwin);//Application.Current.MainWindow);
            if (output.HasValue && output.Value)
            {
                try
                {
                    object ob = analyticServ.SaveRObjs(Model, sfd.FileName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error! "+ ex.Message, "Error occurred while saving the model", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }


        //private void ScoreCommand()
        //{
        //    DashBoardItem item = new DashBoardItem();
        //    UAMenuCommand cmd = new UAMenuCommand();
        //    cmd.commandtype = "BlueSky.Commands.Analytics.TTest.AUAnalysisCommandBase";

        //    cmd.commandtemplate = "./Config/Make Predictions.xaml";
        //    cmd.commandformat = "";
        //    cmd.commandoutputformat = "./Config/Make Predictions.xml";
        //    cmd.text = "Make Predictions";
        //    //cmd.id = GetAttributeString(node, "id"); //04mar2013
        //    //cmd.owner = GetAttributeString(node, "owner"); //04mar2013
        //    item.Command = CreateCommand(cmd);
        //    item.CommandParameter = cmd;
        //    scoreButton.Command = item.Command;
        //    scoreButton.CommandParameter = item.CommandParameter;
        //}

        //private ICommand CreateCommand(UAMenuCommand cmd)
        //{
        //    Type commandTypeObject = null;
        //    ICommand command = null;

        //    try
        //    {
        //        commandTypeObject = Type.GetType(cmd.commandtype);
        //        command = (ICommand)Activator.CreateInstance(commandTypeObject);
        //    }
        //    catch
        //    {
        //        //Create new command instance using default command dispatcher
        //        logService.WriteToLogLevel("Could not create command. " + cmd.commandformat, LogLevelEnum.Error);
        //    }

        //    return command;
        //}

        #endregion

            /// <summary>
            ///  Scoring Help
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
        private void ScoreHelpButton_Click(object sender, RoutedEventArgs e)
        {
            //show help window
            System.Diagnostics.Process.Start("https://www.blueskystatistics.com/Articles.asp?ID=312");
        }
    }
}
