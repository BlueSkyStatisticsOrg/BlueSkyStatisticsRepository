using BlueSky.Commands.Tools.Package;
using BSky.ConfigService.Services;
using BSky.ConfService.Intf.Interfaces;
using BSky.Controls;
using BSky.Interfaces.Commands;
using BSky.Interfaces.Interfaces;
using BSky.Interfaces.Model;
using BSky.Interfaces.Services;
using BSky.Lifetime;
using BSky.Lifetime.Interfaces;
using BSky.Lifetime.Services;
using BSky.Statistics.Common;
using BSky.Statistics.Service.Engine.Interfaces;
using BSky.UsageLogger;
using BSky.XmlDecoder;
using Microsoft.Practices.Unity;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml;
using System.Windows.Controls;
using BlueSky.Windows;
using BSky.Controls.Controls;
using BlueSky.Services;

namespace BlueSky.Commands.Analytics.TTest
{


    public class AUAnalysisCommandBase : ICommand
    {
        ILoggerService logService = LifetimeService.Instance.Container.Resolve<ILoggerService>();//17Dec2012
        IAnalyticsService analytics = LifetimeService.Instance.Container.Resolve<IAnalyticsService>();
        IConfigService confService = LifetimeService.Instance.Container.Resolve<IConfigService>();//23nov2012
        SessionDialogContainer sdc = LifetimeService.Instance.Container.Resolve<SessionDialogContainer>();//13Feb2013
        UsageLogger usglog = LifetimeService.Instance.Container.Resolve<UsageLogger>();

        bool AdvancedLogging;

        protected string TemplateFileName
        { get; set; }

        bool canExecute = true;
        DataSource ds = null;
        IUIController UIController;
        protected object commandwindow;
        //following few vars made global //28Mar2013
        CommandRequest cmd;
        BaseOptionWindow window;
        FrameworkElement element;
        UAReturn retval;
        object parameter;
        bool selectedForDump;

        List<DataSourceVariable> dsvs;
        enum PreReqDialogcontrol { STATUSTEXTBOX, COMBOBOX, NONE };
        /////////////  'Dialog type' related variables  ///////////////////
        //for getting the XML template info from dialog. If it exists then only show the formatted output.
        // xml template will not be provided for non analytic commands
        //26Jun2014 protected bool isXmlTemplateDefined
        //26Jun2014 { get; set; }
        //26Jun2014 bool isCommandOnlyDialog = false;
        bool handleSplitForCommandOnly = false;
        string dialogTitle = string.Empty;
        //26Jun2014 bool isBatchCommand = false;
        int CommandCountInBatch = 0;
        BSkyDialogProperties dlgprop;//26Jun2014 


        #region ICommand Members

        public bool CanExecute(object parameter)
        {
            //if (System.Windows.Input.Mouse.OverrideCursor == System.Windows.Input.Cursors.Wait)//29Sep2015 disable if mouse is busy
            //    return false;
            //else
            //return canExecute;
            return canExecute;
        }

        public ObservableCollection<DataSourceVariable> Variables
        {
            get;
            set;
        }

        public List<DatasetDisplay> Datasets
        {
            get;
            set;
        }

        protected string WrapInputVariable(string key, string val)
        {
            return string.Format(@"<Variable key=""{0}"">{1}</Variable>", key, val);
        }

        public event EventHandler CanExecuteChanged;

        public void Execute(object param)
        {
            IAdvancedLoggingService advlog = LifetimeService.Instance.Container.Resolve<IAdvancedLoggingService>(); ;//01May2015
            advlog.RefreshAdvancedLogging();
            AdvancedLogging = AdvancedLoggingService.AdvLog;//01May2015
            bool reloaddiskfiledata = false;
            //26Jun2014 isXmlTemplateDefined = false;
            parameter = param;////set Global var
            selectedForDump = false; //this parameter is only for Analytics(or non-Analytics)commands exected from Syntax Editor
            OnPreExecute(parameter);
            if (!canExecute)
            {
                canExecute = true;//06Nov2012.Fixed. if no dataset loaded.run one smpl. then load dataset. cant run one sampl now.
                return;
            }

            #region Get Active Dataset 
            string DSFilename = UIController.GetActiveDocument() != null ? UIController.GetActiveDocument().FileName : string.Empty;
            string DSName = UIController.GetActiveDocument() != null ? UIController.GetActiveDocument().Name : string.Empty;
            if (DSName.Trim().Length < 1)
            {
                string msg0 = "You must have atleast one dataset open in the data grid to run any analysis.";
                string ttl = "No active dataset in the datagrid";
                MessageBox.Show(msg0, ttl, MessageBoxButton.OK, MessageBoxImage.Information);
                logService.WriteToLogLevel(msg0+" XAML name : " + TemplateFileName, LogLevelEnum.Info);
                return;
            }
            #endregion

            //for getting dialog xaml filename in logs.
            logService.WriteToLogLevel("XAML name : " + TemplateFileName, LogLevelEnum.Info);
            //MessageBox.Show("Launching: "+ TemplateFileName, "XAML filename");//to find actual dialog XAML filename
            element = null;//fix for preRequsite code getting modifed in GetCommand() call.
            object obj = null;
            string dialogkey = string.Empty;//13Feb2013
            bool isOldDialog = false;//13Feb2013
            try
            {
                //Added by Aaron 06/15/2014
                BSkyCanvas.applyBehaviors = false;
                //UIController.GetActiveDocument().Filename ///for .sav filename
                //UIController.GetActiveDocument().Name // for Dataset1, name

                /// TemplateFilename /// for XAML filename
                dialogkey = TemplateFileName + DSFilename + DSName;
                //if obj is already in dictionary for particular dataset which is currently open
                //then load obj from dictionary
                if (sdc.SessionDialogList.ContainsKey(dialogkey))
                {
                    obj = sdc.SessionDialogList[dialogkey];//gets the exisiting dialog
                    isOldDialog = true;
                    //BSkyCanvas.applyBehaviors = true;
                }
                else//else use XAML dialog file and create obj and then store obj in dictionary with dataset name.
                {

                    //Added by Aaron 06/15/2014
                    //BSkyCanvas.applyBehaviors = false;//taking it out. lets see
                    string CurrentDatasetName = DSName;
                    string changedxaml = string.Empty;

                    ///////  Using XML logic for modifying GroupName attribute of BskyRadioGroup///////////////
                    //changedxaml = FixRadioGroupNameUsingXML(TemplateFileName, CurrentDatasetName);
                    ////////////////////

                    //////// Using Simple Filing logic for modifying GroupName attribute of BskyRadioGroup//////////
                    //There can be 2 or more dialogs with the same group name run against the same dataset.
                    //Prefixing the group name by the datasetname handles 2 different dialogs with same groupname against 
                    //different datasets
                    //Prefixing the group name by datasetname and dialog name handles N dialogs with same group name
                    changedxaml = FixRadioGroupNameUsingFiling(TemplateFileName, CurrentDatasetName + Path.GetFileName(TemplateFileName));
                    /////////////////////////////

                    XmlReader xmlReader = XmlReader.Create(new StringReader(changedxaml));
                    obj = System.Windows.Markup.XamlReader.Load(xmlReader);

                    //04Aug2014 Loading XAML dialog without modifying GroupName attraibute of BskyRadioGroup
                    //obj = System.Windows.Markup.XamlReader.Load(XmlReader.Create(TemplateFileName));

                    //Added by Aaron 03/06/2013
                    //Code below sets the background of the canvas to match the background of Baseoptionwindow
                    //The canvas in dialog editor needs to be white for the contrast with the grey
                    //However when the command is executed, we need to change the color of the canvas to match the background of the dialog
                    //Anil Comented BSkyCanvas canvas = obj as BSkyCanvas;
                    //Anil Comented BSkyCanvas.applyBehaviors = true;
                    //Anil Comented BSkyCanvas.first = canvas;
                    //Anil Comented var converter = new System.Windows.Media.BrushConverter();
                    //Anil Comented canvas.Background = (Brush)converter.ConvertFrom("#FFEEefFf");
                    //now after showdialog(), store obj in memory for later use when user invokes same analysis dialog for same dataset.
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(BSky.GlobalResources.Properties.Resources.CantCreateTemplate+" " + TemplateFileName);
                logService.WriteToLogLevel("Could not create template from " + TemplateFileName, LogLevelEnum.Error, ex);
                //GenerateOutputTablesForNonXAML(param);
                BSkyMouseBusyHandler.HideMouseBusy();//HideMouseBusy_old();
                return;
            }

            BSkyCanvas cs = obj as BSkyCanvas;
            BSkyCanvas.applyBehaviors = true;
            BSkyCanvas.first = cs; 
            var converter = new System.Windows.Media.BrushConverter();
            //base.Background = (Brush)converter.ConvertFromString("#FFEDefFf");
            cs.Background = (Brush)converter.ConvertFrom("#FFEEefFf");

            #region Process empty dataset
            if (UIController.GetActiveDocument()!= null && UIController.GetActiveDocument().isUnprocessed)
            {
                NewDatasetProcessor procDS = new NewDatasetProcessor();
                bool isProcessed = procDS.ProcessNewDataset("", true);
                if (isProcessed)//true:empty rows cols removed successfully. False: whole dataset was empty and nothing was removed.
                {
                    UIController.GetActiveDocument().isUnprocessed = false;
                    PreExecuteSub();// ds = UIController.GetActiveDocument();//processed dataset
                }
                else
                {
                    if (cs.DatasetRequired)
                    {
                        string msg1 = "Analysis cannot be run on an empty dataset.";
                        string msg2 = "Enter data in the grid or open a non-empty dataset.";
                        string cap = "Analysis cannot be run on an empty dataset.";
                        MessageBox.Show(msg1 + "\n" + msg2, cap, MessageBoxButton.OK, MessageBoxImage.Asterisk);
                        return;
                    }
                }
            }
            #endregion
            
            #region Check for ModelClass required by the dialog and currently active Model's class
            string dlgmodelclasses = cs.ModelClasses;
            if (dlgmodelclasses != null)
            {
                bool classMatchFound = false;
                string[] dlgmodclassarr = dlgmodelclasses.Split(new char[] { ';' });
                if (!string.IsNullOrEmpty(dlgmodelclasses))
                {

                    string[] activeModelClasses = getCurrentlyActiveModelClass();//get the classes of the currently active model.
                    if (activeModelClasses != null)
                    {
                        foreach (string s1 in dlgmodclassarr)
                        {
                            foreach (string s2 in activeModelClasses) 
                            {
                                if (s1.Trim().Equals(s2.Trim()))//break out of inner loop if match is found
                                {
                                    classMatchFound = true; 
                                    break; 
                                }
                            }
                            if (classMatchFound)//no need to loop any more if match is found in inner loop
                                break;
                        }
                    }
                    if (!classMatchFound)
                    {
                        
                        // statmsg has message like : XYZ package not found. Please install it from CRAN
                        string firstmsg = BSky.GlobalResources.Properties.Resources.IncorrectModelOrModelType+"\n";
                        string secmsg = BSky.GlobalResources.Properties.Resources.ModelsSupportedAre+"\n\n" + dlgmodelclasses;
                        string hlpmsg = "\n\n"+BSky.GlobalResources.Properties.Resources.SelectModelTypeAndModel;
                        string lastmsg = firstmsg + secmsg + hlpmsg;// +" to be selected";
                        //string fstmsg = "Scoring requires a model type and model to be selected";
                        MessageBox.Show(lastmsg, BSky.GlobalResources.Properties.Resources.ErrReqModelNotSelected, MessageBoxButton.OK, MessageBoxImage.Stop);
                        logService.WriteToLogLevel(lastmsg, LogLevelEnum.Error);
                        if (window != null)
                            window.Template = null; //13Feb2013 release the XAML object. ie obj is no more child of window.
                        BSkyMouseBusyHandler.HideMouseBusy();// HideMouseBusy_old();
                        return;//Skip dialog execution further.(as all required packages failed to load. And dialog commands will not execute properly)
                    }
                }
            }

            #endregion

            //16Sep2016 Because of Pre-Requisite syntax following 3 line are moved here from down somewhere.
            OutputHelper.Reset();
            OutputHelper.UpdateMacro("%DATASET%", DSName);
            OutputHelper.UpdateMacro("%MODEL%", UIController.GetActiveModelName());

            #region Run Dialogs prerequisite syntax and see if it generates any error
            PreReqDialogcontrol preReqDlgCtrl = PreReqDialogcontrol.NONE;
            string PreReqCommand = cs.PrereqCommandString;
            if (PreReqCommand != null && PreReqCommand.Contains("{{%DATASET%}}"))
            {
                PreReqCommand=PreReqCommand.Replace("{{%DATASET%}}", DSName);//current dataset substituted
            }			
            string statusTextControlName = cs.StatusTextBoxName;
			string EditableComboBoxName = cs.EditableComboBoxName;
            //Aaron: either its status-text or comboBox (or none) but not both.
            if (statusTextControlName != null && statusTextControlName.Trim().Length > 0)
            {
                preReqDlgCtrl = PreReqDialogcontrol.STATUSTEXTBOX;
            }
            else if (EditableComboBoxName != null && EditableComboBoxName.Trim().Length > 0)
            {
                preReqDlgCtrl = PreReqDialogcontrol.COMBOBOX;
            }
            bool enableDialog = true;
            if (PreReqCommand != null)
            {
                if (PreReqCommand != "")
                {
                    #region Execute Prereq commands that should return status[TRUE or a message(error/warning)] or combo entries
                    string msg = "---";
                    string[] comboBoxVals = null;
                    CommandRequest cmdreq = new CommandRequest();
                    cmdreq.CommandSyntax = OutputHelper.GetCommand(PreReqCommand, element);
                    object ocnt = analytics.ExecuteR(cmdreq, true, false);
                    if (ocnt != null)
                    {
                        //Error processing common for Status and Combo
                        msg = ocnt as string;
                        if (msg!=null && (msg.Contains("ERROR") || msg.Contains("Error") )) //09Mar2017 'Error' may occur in R own messages
                        {
                            enableDialog = false;
                        }

                        if (preReqDlgCtrl == PreReqDialogcontrol.STATUSTEXTBOX)
                        {
                            if (ocnt.GetType().Name.Equals("String"))
                            {
                                //msg = ocnt as string;
                                //if (msg.Contains("ERROR") || msg.Contains("Error")) //09Mar2017 'Error' may occur in R own messages
                                //{
                                //    enableDialog = false;
                                //}
                                object el = cs.FindName(statusTextControlName);
                                BSkyMultiLineLabel bslbl = el as BSkyMultiLineLabel;
                                if (bslbl != null)
                                {
                                    bslbl.Text = msg;
                                }
                            }
                        }
                        else if (preReqDlgCtrl == PreReqDialogcontrol.COMBOBOX && enableDialog)//if combo and no error
                        {
                            string[] editableComboBoxNames = null;
                            char[] sep = new char[] { ';' };

                            //single or multiple combos in dialog property
                            if (EditableComboBoxName.Trim().Contains(";"))
                            {
                                editableComboBoxNames = EditableComboBoxName.Split(sep);
                            }
                            else
                            {
                                editableComboBoxNames = new string[1];
                                editableComboBoxNames[0] = EditableComboBoxName;
                            }

                            //returned single model or multiple model
                            if (ocnt.GetType().Name.Equals("String"))
                            {
                                comboBoxVals = new string[1];
                                comboBoxVals[0] = ocnt as string;
                            }
                            else if (ocnt.GetType().Name.Equals("String[]"))
                            {
                                comboBoxVals = ocnt as string[];
                            }

                            //Fill combos with same data
                            foreach (string comboname in editableComboBoxNames)
                            {
                                if (string.IsNullOrEmpty(comboname))
                                    continue;
                                object e2 = cs.FindName(comboname.Trim());
                                BSkyEditableComboBox edtCmbBx = e2 as BSkyEditableComboBox;
                                edtCmbBx.IsTextSearchEnabled = true;
                                edtCmbBx.IsTextSearchCaseSensitive = true;

                                if (edtCmbBx != null)
                                {
                                    //edtCmbBx.Items.Add(""); //edtCmbBx.IsEditable = false;
                                    foreach (string s in comboBoxVals)
                                    {
                                        if (!edtCmbBx.Items.Contains(s))
                                            edtCmbBx.Items.Add(s);
                                    }
                                }
                            }
                        }

                    }

                    #endregion
                }
            }
            #endregion

            //Added by Aaron 05/13/2014
            //Added 3 lines below
            //commenting this as we have added 3 line above BSkyCanvas cs = obj as BSkyCanvas;
            //Added by Aaron 06/15/2014
            //Commented 2 Lines below  
            // BSkyCanvas.chainOpenCanvas.Clear();
            // BSkyCanvas.chainOpenCanvas.Add(cs);
            
            cmd = new CommandRequest();//set Global var
            element = obj as FrameworkElement;////set Global var
            window = new BaseOptionWindow();////set Global var
            window.Template = element;
            element.DataContext = this; // loading vars in left listbox(source)
            window.Topmost = true;
            window.IsEnabled = enableDialog;//this will disable if prerequisite are not met
            
            #region For Spark dialogs only
            //31Jul2017 here we are using package name to detect Spark dialog but 
            //we can add a new bool property to canvas say isSparkDialog to do the same
            ////if (cs.RPackages.Contains("SparkR"))
            ////{

            ////}

            #endregion

            // If a form is displayed as modal, the code following the ShowDialog method is not executed  
            // until the dialog box is closed. However, when a form is shown as modeless, the code following 
            // the Show method is executed immediately after the form is displayed.
            if (!cs.Command)//26Jun2014 !isCommandOnlyDialog)
                window.ShowDialog();
            //Added by Aaron 07/20/2014
            //If GetOverwrittenVars returns true tells me that a user has selected a dataset or variable to be created by the analytical command but that 
            //variable already exists. Hence the command should not be executed.
            if (window.GetOverwrittenVars())
            {
                if (window != null)
                    window.Template = null; //16Feb2017 release the XAML object. ie obj is no more child of window.
                return;
            }
            /////13Feb2013 Store obj in dictionary if not already there ////
            if (!(isOldDialog || sdc.SessionDialogList.ContainsKey(dialogkey)))
            {
                sdc.SessionDialogList.Add(dialogkey, obj);
            }

            commandwindow = element;

            //27Apr2016
            bool DatasetHasSplit = false;
            object ooo = OutputHelper.GetGlobalMacro(string.Format("GLOBAL.{0}.SPLIT", DSName), string.Empty);//27Apr2016 get split details
            if (ooo != null)
            {
                DatasetHasSplit = true;
            }

            if ((window.DialogResult.HasValue && window.DialogResult.Value) || cs.Command)
            {
                BSkyCanvas canvas = element as BSkyCanvas;

                if (canvas != null && !string.IsNullOrEmpty(canvas.CommandString))
                {
                    //following is important as its substituting params with values.
                    cmd.CommandSyntax = OutputHelper.GetCommand(canvas.CommandString, element);// can be used for "Paste" for syntax editor
                    if (cmd.CommandSyntax.Contains("BSkyReloadDataset("))// if its relaod dataset commmand then prepare some parameter before executing the command
                    {
                        reloaddiskfiledata = true;
                        SubstituteFilenameAndType();//get filename and file type of disk file that is supposed to get reloaded.

                        //execute above sinlge line(execute in Syntax) OR following 4 lines(old way of execution

                        //ExeuteSingleCommandWithtoutXML();
                        //if (window != null)
                        //    window.Template = null; //25Mar2013 release the XAML object. ie obj is no more child of window.
                        //return;
                    }
                    CollectDialogProperties(canvas); // get all dialog properties

                    BSkyMouseBusyHandler.ShowMouseBusy();// ShowMouseBusy_old();

#region Loading Dialog's required R Packages //30Apr2015
                    //following single line just for testing. Comment it and uncomment next line for production.
                    //string pkgs = "zoo;sem;aa;;nlme;bb;class;"; 
                    string pkgs = dlgprop.RequiredRPackages;
                    if (!string.IsNullOrEmpty(pkgs))
                    {
                        bool packageMissing = false;
                        string statmsg = LoadDialogRPacakges(pkgs, out packageMissing);//pass semi colon separated list of R packages.
                        if (statmsg.Trim().Length > 0 && (statmsg.ToLower().Contains("error") || packageMissing))
                        {
                            // statmsg has message like : XYZ package not found. Please install it from CRAN
                            string lastmsg = "\n"+BSky.GlobalResources.Properties.Resources.InstallAbvMentionedRPkgs 
                                + "\n"+BSky.GlobalResources.Properties.Resources.InstallUpdatePkgMenuPath +
                                //"\n\nOR if its one of the required R package then"+
                                //"\n\nGo to: Installation -> Install All required R packages from CRAN"+
                                "\n"+BSky.GlobalResources.Properties.Resources.InstallRPkgRunDialogAgain;
                            MessageBox.Show(canvas.Title + " "+BSky.GlobalResources.Properties.Resources.DlgNeedTheseRPkgs + "\n\n" + statmsg + lastmsg, BSky.GlobalResources.Properties.Resources.MisingRPkgs, MessageBoxButton.OK, MessageBoxImage.Stop);
                            logService.WriteToLogLevel("Dialog's required R package loading error:\n" + statmsg, LogLevelEnum.Error);
                            if (window != null)
                                window.Template = null; //13Feb2013 release the XAML object. ie obj is no more child of window.
                            BSkyMouseBusyHandler.HideMouseBusy();// HideMouseBusy_old();
                            return;//Skip dialog execution further.(as all required packages failed to load. And dialog commands will not execute properly)
                        }
                    }
#endregion

#region Dialog's Kill button to remove cached dialog and next time get a fresh one.
                    //Check if 'Kill' was clicked
                    //bool kill = false;
                    //if (kill) window.Tag = "Kill";
                    if (window.Tag != null && window.Tag.ToString().Equals("Kill"))//20Aug2014 Kill cached copy of this dialog
                    {
                        if (sdc.SessionDialogList.ContainsKey(dialogkey))
                        {
                            bool removed = sdc.SessionDialogList.Remove(dialogkey);//removes the exisiting dialog
                            logService.WriteToLogLevel("Dialog Remove Status : " + removed.ToString(), LogLevelEnum.Info);
                        }
                    }
#endregion

#region PASTE clicked on the dialog
                    //Check if OK or Syntax was clicked
                    if (window.Tag != null && window.Tag.ToString().Equals("Syntax"))//21Jan2013
                    {
                        PasteInOutputSyntax();//06May2015 PasteSyntax();
                        SaveInHistory();
                    }
#endregion

#region OK clicked on the dialog
                    if ((window.Tag != null && window.Tag.ToString().Equals("Ok")) || dlgprop.IsCommandOnly)//26Jun2014 isCommandOnlyDialog)//21Jan2013 Dont use else if is Syntax stay dialog workaround is in use.
                    {
                        dialogTitle = (window.Title != null) ? window.Title : string.Empty;
                        string batchcommtitle = dialogTitle;
                        bool handlesplits = cs.splitProcessing;//dlgprop.HandleSplits; //Handle split in C# code
                        bool DialogHasHandleSplitFlag = dlgprop.HandleSplits; //if we decided to switch to old style(no handleSplit prop from dialog)
                        
						bool xmlTemplateBasedDialog = false;
                        bool isSplitDialog = false; // 27Apr2016 split dialog is neither analytical nor data manipulation dialog
                        if(UIController.GetActiveDocument()!=null)
                            dsvs = UIController.GetActiveDocument().Variables;//list of exisiting vars in active dataset

                        string dlgcommands = string.Join(";", dlgprop.Commands);
                        if (dlgcommands.Contains("BSkyIndSmTTest") || dlgcommands.Contains("BSkyOneSmTTest") || dlgcommands.Contains("BSkyCrossTable"))
                            xmlTemplateBasedDialog = true;

                        //27Apr2016 see if the dialog is split dialog
                        if (dlgcommands.Contains("BSkySetDataFrameSplit"))
                            isSplitDialog = true;                        
                        try
                        {
                            if (DialogHasHandleSplitFlag)
                            {
                                if (handlesplits && ! xmlTemplateBasedDialog) //handle splits in C#
                                {
                                    if (AdvancedLogging) logService.WriteToLogLevel("ExtraLogs: before executing as BatchCommand.", LogLevelEnum.Info);
                                    //this can handle template based dialogs but split handling may get tricky. To make it
                                    //work here, probably, turn off split handling from within these template based dialogs.
                                    //this is just ahint, we nned to try and make sure.
                                    ExecuteBatch(dlgprop.Commands); //Execute single and multiple commands as batch commands(ie..execute in Syntax)
                                    if (AdvancedLogging) logService.WriteToLogLevel("ExtraLogs: after executing as BatchCommand.", LogLevelEnum.Info);
                                }
                                else //R handles splits
                                {
                                    //30Apr2015 This is block is probably needed because 3 of our Template based dialogs have internal split handler.
                                    cmd.CommandSyntax = GetSliceSubstitutedCommands(dlgprop.Commands, 1);
                                    PrintDialogTitle(dialogTitle);//30Apr2015
                                    if (AdvancedLogging) logService.WriteToLogLevel("ExtraLogs: before executing in Syntax.", LogLevelEnum.Info);
                                    ExecuteInSyntaxEditor(true, dialogTitle);
                                    if (AdvancedLogging) logService.WriteToLogLevel("ExtraLogs: after executing in Syntax.", LogLevelEnum.Info);
                                }
                            }
                            else
                            {
                                //24Apr2016 Show popup if there is split and its data manipulation command.
                                //Dont execute data manipulation command on the split dataset
                                // Later you can change some of your code to run data manipulation command on split dataset but
                                // out side of the split loop in C#
                                if (DatasetHasSplit && !isSplitDialog) //Dataset has split but the current dialog is not split dialog
                                {
                                    Window1 appWindow = LifetimeService.Instance.Container.Resolve<Window1>();
                                    string manipu = BSky.GlobalResources.Properties.Resources.RemoveSplitForDataManipulation;
                                    MessageBox.Show(appWindow,manipu, BSky.GlobalResources.Properties.Resources.RemoveSplit, MessageBoxButton.OK, MessageBoxImage.Asterisk);
                                    if (window != null)
                                        window.Template = null; //25Mar2013 release the XAML object. ie obj is no more child of window.
                                    BSkyMouseBusyHandler.HideMouseBusy();// HideMouseBusy_old();
                                    return;
                                }
                                else
                                {
                                    cmd.CommandSyntax = GetSliceSubstitutedCommands(dlgprop.Commands, 1);
                                    PrintDialogTitle(dialogTitle);//30Apr2015
                                    if (AdvancedLogging) logService.WriteToLogLevel("ExtraLogs: before executing data manipulation as BatchCommand.", LogLevelEnum.Info);
                                    //this can handle template based dialogs but split handling may get tricky. To make it
                                    //work here, probably, turn off split handling from within these template based dialogs.
                                    //this is just ahint, we nned to try and make sure.
                                    ExecuteInSyntaxEditor(true, dialogTitle); //ExecuteBatch(dlgprop.Commands); //Execute single and multiple commands as batch commands(ie..execute in Syntax)
                                    if (AdvancedLogging) logService.WriteToLogLevel("ExtraLogs: after executing data manipulation as BatchCommand.", LogLevelEnum.Info);
                                }


                                ///////////////Old code//////////////
                                //string dlgcommands = string.Join(";", dlgprop.Commands);
                                //if (dlgcommands.Contains("BSkyIndSmTTest") || dlgcommands.Contains("BSkyOneSmTTest") || dlgcommands.Contains("BSkyCrossTable"))
                                //{
                                //    //30Apr2015 This is block is probably needed because 3 of our Template based dialogs have internal split handler.
                                //    cmd.CommandSyntax = GetSliceSubstitutedCommands(dlgprop.Commands, 1);
                                //    PrintDialogTitle(dialogTitle);//30Apr2015
                                //    if (AdvancedLogging) logService.WriteToLogLevel("ExtraLogs: before executing in Syntax.", LogLevelEnum.Info);
                                //    ExecuteInSyntaxEditor(true, dialogTitle);
                                //    if (AdvancedLogging) logService.WriteToLogLevel("ExtraLogs: after executing in Syntax.", LogLevelEnum.Info);
                                //}
                                //else
                                //{
                                //    if (AdvancedLogging) logService.WriteToLogLevel("ExtraLogs: before executing as BatchCommand.", LogLevelEnum.Info);
                                //    //this can handle template based dialogs but split handling may get tricky. To make it
                                //    //work here, probably, turn off split handling from within these template based dialogs.
                                //    //this is just ahint, we nned to try and make sure.
                                //    ExecuteBatch(dlgprop.Commands); //Execute single and multiple commands as batch commands(ie..execute in Syntax)
                                //    if (AdvancedLogging) logService.WriteToLogLevel("ExtraLogs: after executing as BatchCommand.", LogLevelEnum.Info);
                                //}

                            }
                            //15Jan2015
                            //Following was the original code.But 'if/else' was introduced above just to resctrict XML templated 
                            //dialog to run differently in case of split set.
                            //ExecuteBatch(dlgprop.Commands); //Execute single and multiple commands as batch commands(ie..execute in Syntax)
                            if (AdvancedLogging) logService.WriteToLogLevel("ExtraLogs: successful execution.", LogLevelEnum.Info);
                        }
                        catch (Exception e)
                        {
                            MessageBox.Show(BSky.GlobalResources.Properties.Resources.CantExecuteCommand);
                            logService.WriteToLogLevel("Couldn't Execute the command", LogLevelEnum.Error, e);
                            if (window != null)
                                window.Template = null; //25Mar2013 release the XAML object. ie obj is no more child of window.
                            BSkyMouseBusyHandler.HideMouseBusy();// HideMouseBusy_old();
                            return;
                        }
                        BSkyMouseBusyHandler.HideMouseBusy();//this is needed here otherwise History menu items are getting disabled.
                        //save current dialog in 'History' menu
                        SaveInHistory();

                        //log the executed command/dialog in usage-log
                        usglog.AddKey(Path.GetFileNameWithoutExtension(TemplateFileName));
                        usglog.SaveToFile();
                    }
#endregion

                    BSkyMouseBusyHandler.HideMouseBusy();// HideMouseBusy_old();
                }
            }

            // if its relaod dataset commmand then we need to refresh grid and status bar
            if (reloaddiskfiledata)// (cmd.CommandSyntax.Contains("BSkyReloadDataset("))
            {
                RefreshGridAndStatus();//uncomment this line if executing in Syntax. Comment it if executing using old method.
            }

            window.Template = null; //13Feb2013 release the XAML object. ie obj is no more child of window.
            OnPostExecute(parameter);
        }


        public void ExecutePackageRelatedDialogs(object param)
        {
            IAdvancedLoggingService advlog = LifetimeService.Instance.Container.Resolve<IAdvancedLoggingService>(); ;//01May2015
            advlog.RefreshAdvancedLogging();
            AdvancedLogging = AdvancedLoggingService.AdvLog;//01May2015
            bool reloaddiskfiledata = false;
            OutputHelper.Reset();
            //26Jun2014 isXmlTemplateDefined = false;

            //for getting dialog xaml filename in logs.
            logService.WriteToLogLevel("XAML name : " + TemplateFileName, LogLevelEnum.Info);
            //MessageBox.Show("Launching: "+ TemplateFileName, "XAML filename");//to find actual dialog XAML filename

            object obj = null;
            string dialogkey = string.Empty;//13Feb2013
            bool isOldDialog = false;//13Feb2013
            try
            {
                //Added by Aaron 06/15/2014
                BSkyCanvas.applyBehaviors = false;

                    string changedxaml = string.Empty;

                    XmlReader xmlReader = XmlReader.Create(new StringReader(System.IO.File.ReadAllText(TemplateFileName)));
                    obj = System.Windows.Markup.XamlReader.Load(xmlReader);

            }
            catch (Exception ex)
            {
                MessageBox.Show(BSky.GlobalResources.Properties.Resources.CantCreateTemplate+" " + TemplateFileName);
                logService.WriteToLogLevel("Could not create template from " + TemplateFileName, LogLevelEnum.Error, ex);
                //GenerateOutputTablesForNonXAML(param);
                return;
            }

            BSkyCanvas cs = obj as BSkyCanvas;
            BSkyCanvas.applyBehaviors = true;
            BSkyCanvas.first = cs;
            var converter = new System.Windows.Media.BrushConverter();
            //base.Background = (Brush)converter.ConvertFromString("#FFEDefFf");
            cs.Background = (Brush)converter.ConvertFrom("#FFEEefFf");

            cmd = new CommandRequest();//set Global var
            element = obj as FrameworkElement;////set Global var
            window = new BaseOptionWindow();////set Global var
            window.Template = element;
            element.DataContext = this; // loading vars in left listbox(source)


            // If a form is displayed as modal, the code following the ShowDialog method is not executed  
            // until the dialog box is closed. However, when a form is shown as modeless, the code following 
            // the Show method is executed immediately after the form is displayed.
            if (!cs.Command)//26Jun2014 !isCommandOnlyDialog)
                window.ShowDialog();
            //Added by Aaron 07/20/2014
            //If GetOverwrittenVars returns true tells me that a user has selected a dataset or variable to be created by the analytical command but that 
            //variable already exists. Hence the command should not be executed.
            if (window.GetOverwrittenVars()) return;
            /////13Feb2013 Store obj in dictionary if not already there ////
            //if (!isOldDialog)
            //{
            //    sdc.SessionDialogList.Add(dialogkey, obj);
            //}

            commandwindow = element;

            if ((window.DialogResult.HasValue && window.DialogResult.Value) || cs.Command)
            {
                BSkyCanvas canvas = element as BSkyCanvas;

                if (canvas != null && !string.IsNullOrEmpty(canvas.CommandString))
                {
                    //following is important as its substituting params with values.
                    cmd.CommandSyntax = OutputHelper.GetCommand(canvas.CommandString, element);// can be used for "Paste" for syntax editor
                    CollectDialogProperties(canvas); // get all dialog properties
                    if (canvas.Title != null && canvas.Title.Contains("required R packages")) //"Install required R packages from CRAN" dialog. Do not change for localization.
                    {
                        string waitmsg = BSky.GlobalResources.Properties.Resources.BSkyDownloadingReqPkgs +
                            "\n"+BSky.GlobalResources.Properties.Resources.MayTake1530Min;
                        SendToOutputWindow(BSky.GlobalResources.Properties.Resources.PlzWait, waitmsg, false);
                    }
                    BSkyMouseBusyHandler.ShowMouseBusy();// ShowMouseBusy_old();

#region PASTE clicked on the dialog
                    //Check if OK or Syntax was clicked
                    if (window.Tag != null && window.Tag.ToString().Equals("Syntax"))//21Jan2013  Do not change for localization.
                    {
                        PasteInOutputSyntax();//06May2015 PasteSyntax();
                    }
                    #endregion

                    #region OK clicked on the dialog
                                            // Do not change for localization. The Ok below
                    if ((window.Tag != null && window.Tag.ToString().Equals("Ok")) || dlgprop.IsCommandOnly)//26Jun2014 isCommandOnlyDialog)//21Jan2013 Dont use else if is Syntax stay dialog workaround is in use.
                    {
                        dialogTitle = (window.Title != null) ? window.Title : string.Empty;
                        string batchcommtitle = dialogTitle;

                        try
                        {
                            string dlgcommands = string.Join(";", dlgprop.Commands);

                                PrintDialogTitle(dialogTitle);//30Apr2015


                                ExecuteInSyntaxEditor(true, dialogTitle);

                            if (AdvancedLogging) logService.WriteToLogLevel("ExtraLogs: successful execution.", LogLevelEnum.Info);


                        }
                        catch (Exception e)
                        {
                            MessageBox.Show(BSky.GlobalResources.Properties.Resources.CantExecuteCommand);
                            logService.WriteToLogLevel("Couldn't Execute the command", LogLevelEnum.Error, e);
                            if (window != null)
                                window.Template = null; //25Mar2013 release the XAML object. ie obj is no more child of window.
                            return;
                        }
                        //save current dialog in 'History' menu
                        SaveInHistory();
                    }
#endregion

                    BSkyMouseBusyHandler.HideMouseBusy();// HideMouseBusy_old();
                }
            }

            // if its relaod dataset commmand then we need to refresh grid and status bar
            if (reloaddiskfiledata)// (cmd.CommandSyntax.Contains("BSkyReloadDataset("))
            {
                RefreshGridAndStatus();//uncomment this line if executing in Syntax. Comment it if executing using old method.
            }

            window.Template = null; //13Feb2013 release the XAML object. ie obj is no more child of window.
            OnPostExecute(parameter);
        }

        //reads XAML and prefix GroupName attribute of each BSkyRadioButton with provided prefix.
        protected string FixRadioGroupNameUsingXML(string fullpathfilename, string prefix)
        {
            string changedxaml = string.Empty;
            XmlDocument doc = new XmlDocument();
            XmlTextReader xmlReader = new XmlTextReader(fullpathfilename);
            doc.Load(xmlReader);

            ////Finding all RadioGroups in a dialog and its subdialog ////Not developed fully
            //XmlNodeList nodes = doc.GetElementsByTagName("BSkyRadioGroup");
            //foreach (XmlNode node in nodes) // find name of each radio group and prefix it with current datasetname
            //{
            //    if (node.Attributes["Name"] != null)
            //    {
            //        // Set the new value
            //        node.Attributes["Name"].Value = "ItShouldWork";
            //    }
            //}

            ////Finding all Radio Buttons and prefix the value of GroupName attribute by current datasetname
            XmlNodeList nodes = doc.GetElementsByTagName("BSkyRadioButton");
            string grpname = string.Empty;
            foreach (XmlNode node in nodes)
            {
                if (node.Attributes["GroupName"] != null)
                {
                    grpname = node.Attributes["GroupName"].Value.ToString();
                    // Set the new value
                    node.Attributes["GroupName"].Value = prefix + grpname;
                }
            }
            changedxaml = doc.OuterXml;
            xmlReader.Close();
            return changedxaml;
        }

        protected string FixRadioGroupNameUsingFiling(string fullpathfilename, string prefix)
        {
            string content = string.Empty;
            if (System.IO.File.Exists(fullpathfilename))
            {
                content = System.IO.File.ReadAllText(fullpathfilename);
            }
            int len = content.Length;
            int next = 0;
            int idx;
            int doublequotesindex;
            while (true)
            {
                idx = content.IndexOf("GroupName", next);//find GroupName after "next" index in string
                if (idx < 0)//if GroupName not found, break out of the loop
                    break;
                doublequotesindex = content.IndexOf("\"", idx);//first double quotes after GroupName
                content = content.Insert(doublequotesindex + 1, prefix);
                next = doublequotesindex;
            }
            return content;
        }
        //only for re-loading dataset those are loaded from disk file. BSkyReloadDataset()
        protected void SubstituteFilenameAndType()
        {
            DataSource tempds = UIController.GetActiveDocument();
            string filename = tempds.FileName.Replace("\\", "/").Trim();
            string name = tempds.Name;
            string filetype = tempds.FileType.Trim();
            string sheetname = tempds.SheetName != null ? tempds.SheetName : string.Empty;// BugId #JM12Apr16 and below wherever sheetname is used

            //16Nov2017
            bool HasHeader = tempds.HasHeader;
            bool IsMarketBasket = tempds.IsBasketData;
            string FieldSepChar = tempds.FieldSeparator;
            string DecimalChar = tempds.DecimalCharacter;

            //////string temp = new string(cmd.CommandSyntax.ToCharArray());
            ////////better : make use of following two lines and try to follow similar approach for other subtitutions 'filetype' etc..
            //////int idx1 = temp.IndexOf("sheetname");
            //////idx1 = temp.IndexOf("NULL", idx1);//index of NULL (this null is after 'sheetname'
            //////temp = temp.Remove(idx1, 4).Insert(idx1, "'"+sheetname+"'");//Null removed and sheetname added
            //////cmd.CommandSyntax = temp.Replace("fullpathfilename", "fullpathfilename='" + filename + "'").Replace("filetype", "filetype='" + filetype + "'");

            ////////Restting split if all data and attributes are reloaded 
            //////int idx = temp.IndexOf("=", temp.IndexOf("loaddataonly")); // index of  '=' after 'loaddataonly'
            //////int idxcomma = temp.IndexOf(",", temp.IndexOf("loaddataonly")); // index of  ',' after 'loaddataonly'
            //////string boolvalue = temp.Substring(idx + 1, idxcomma - idx - 1).Trim();
            //////if (boolvalue.Equals("FALSE"))//loaddataonly = false then we need to reset SPLIT in C# also
            //////{
            //////    OutputHelper.DeleteGlobalObject(string.Format("GLOBAL.{0}.SPLIT", UIController.GetActiveDocument().Name));
            //////}

            //16Nov2017 command is now created as following and not like above commented code.
            cmd.CommandSyntax=(string.Format("BSkyReloadDataset(fullpathfilename='{0}',  "+
                "filetype='{1}', sheetname='{2}', csvHeader={3}, loaddataonly={4},"+
                "isBasketData={5}, sepChar='{6}', deciChar='{7}', datasetname='{8}')",
                filename, filetype, sheetname, HasHeader.ToString().ToUpper(), "FALSE", 
                IsMarketBasket.ToString().ToUpper(), FieldSepChar, DecimalChar, tempds.Name)
                );
            OutputHelper.DeleteGlobalObject(string.Format("GLOBAL.{0}.SPLIT", UIController.GetActiveDocument().Name));


            /// if someone tried refreshing a dataset that was not loaded from disk file then
            if( (filetype.Trim().Length < 1) || filename.Equals(name) )
            {
                MessageBox.Show(BSky.GlobalResources.Properties.Resources.MsgForReloadFromDiskForMemoryDF +
                    "\n"+ BSky.GlobalResources.Properties.Resources.MsgForReloadFromDiskForMemoryDF2,
                    BSky.GlobalResources.Properties.Resources.MsgboxTitleForReloadFromDisk,
                    MessageBoxButton.OK, MessageBoxImage.Information);
                cmd.CommandSyntax = "BSkyLoadRefreshDataframe(" + filename + ")";//Fire this command instead.
            }
        }

        //for refreshing grid and status bar on relaoding data file from disk. Only used with BSkyReloadDataset()
        protected void RefreshGridAndStatus()
        {
            RefreshGrids();
            Refresh_Statusbar();
            //Finally show messages in output
            //SendToOutputWindow(dialogTitle, cmd.CommandSyntax);
        }

        protected void CollectDialogProperties(BSkyCanvas canvas)
        {
            dlgprop = new BSkyDialogProperties();

            dlgprop.RequiredRPackages = canvas.RPackages;//30Apr2015

            dlgprop.ModelClasses = canvas.ModelClasses;

            dlgprop.HandleSplits = canvas.splitProcessing;// canvas.PROPERTYFORSPLITHERE; //if true handle splits for command only dialog, else not

            dlgprop.IsCommandOnly = canvas.Command; // command(no dialog) or dialog

            dlgprop.IsGraphic = false;

            dlgprop.IsGridRefresh = true;

            dlgprop.IsMacroUpdate = true;

            dlgprop.IsStatusBarRefresh = true;

            dlgprop.IsXMLDefined = (canvas.OutputDefinition != null && canvas.OutputDefinition.Length > 0) ? true : false;

            //string[] commands = cmd.CommandSyntax.Replace('\n', ';').Split(';');
            if (cmd.CommandSyntax == null)
                cmd.CommandSyntax = string.Empty;

            dlgprop.Commands = cmd.CommandSyntax.Replace('\n', ';').Split(';');

            dlgprop.DialogTitle = canvas.Title;

            CommandCountInBatch = dlgprop.Commands.Length;
            dlgprop.IsBatchCommand = (CommandCountInBatch > 1) ? true : false;


        }

        //For commands : Dialog may or may not be shown but surely XML template is not present.
        public void ExeuteSingleCommandWithtoutXML(string command = "")
        {
            if (command != null && command.Length > 0)
            {
                cmd = new CommandRequest();
                cmd.CommandSyntax = command;
            }
            if (cmd.CommandSyntax == null || cmd.CommandSyntax.Length < 1)
            {
                cmd = new CommandRequest();
                cmd.CommandSyntax = "print('"+BSky.GlobalResources.Properties.Resources.NoCommToExecute + "')";
            }

            if (cmd.CommandSyntax.Contains("BSkyReloadDataset("))// if its relaod dataset commmand then prepare some parameter before executing the command
            {
                DataSource tempds = UIController.GetActiveDocument();
                string filename = tempds.FileName.Replace("\\", "/").Trim();
                string filetype = tempds.FileType.Trim();
                string temp = new string(cmd.CommandSyntax.ToCharArray());

                cmd.CommandSyntax = temp.Replace("fullpathfilename", "fullpathfilename='" + filename + "'").Replace("filetype", "filetype='" + filetype + "'"); ;

                //Restting split if all data and attributes are loaded 
                int idx = temp.IndexOf("=", temp.IndexOf("loaddataonly")); // index of  '=' after 'loaddataonly'
                int idxcomma = temp.IndexOf(",", temp.IndexOf("loaddataonly")); // index of  ',' after 'loaddataonly'
                string boolvalue = temp.Substring(idx + 1, idxcomma - idx - 1).Trim();
                if (boolvalue.Equals("FALSE"))//loaddataonly = false then we need to reset SPLIT in C# also
                {
                    OutputHelper.DeleteGlobalObject(string.Format("GLOBAL.{0}.SPLIT", UIController.GetActiveDocument().Name));
                }
            }
            //retval = analytics.Execute(cmd);
            if (dlgprop != null)
            {
                if (!dlgprop.IsBatchCommand && dlgprop.IsCommandOnly && !dlgprop.IsXMLDefined && !cmd.CommandSyntax.Contains("BSkySetDataFrameSplit("))
                {
                    SendToOutputWindow(dialogTitle, cmd.CommandSyntax);
                    ExecuteInSyntaxEditor(true, dialogTitle);//GenerateOutputTablesForNonXAML(null);// ExecuteXMLDefinedDialog();
                }
            }
            else
            {
                retval = analytics.Execute(cmd);

                if (cmd.CommandSyntax.Contains("BSkySetDataFrameSplit(")) //split sets some MACRO in memory too. So its different from others.
                {
                    ExecuteSplit();
                }
                else
                {
                    if (cmd.CommandSyntax.Contains("BSkySortDataframe")) //11Apr2014 putting sort icon
                    {
                        //single column logic
                        //int startidx = cmd.CommandSyntax.IndexOf("'");
                        //int endidx= cmd.CommandSyntax.IndexOf("'", startidx+1);
                        //int leng = endidx - startidx-1;
                        //string colname = cmd.CommandSyntax.Substring(startidx+1, leng);

                        //sort order 14Apr2014
                        string srtodr = string.Empty;
                        // descending=FALSE in command. There is just 1 boolean in sort so this 'if' will work
                        if (cmd.CommandSyntax.Contains("FALSE"))
                            srtodr = "asc";
                        else
                            srtodr = "desc";

                        //mulitiple col logic
                        List<string> collst = new List<string>();
                        int startidx = cmd.CommandSyntax.IndexOf("c(");
                        if (startidx == -1) //no items in target listbox. No need of this if sort dialog has OK disabled when no items in target
                        {
                            return;
                        }
                        int endidx = cmd.CommandSyntax.IndexOf(")", startidx + 1);
                        int leng = endidx - startidx - 1;
                        string selcols = cmd.CommandSyntax.Substring(startidx + 2, leng - 1).Replace("'", "");// +2 is the length of "c(", -1 for )
                        string[] cols = selcols.Split(',');
                        for (int j = 0; j < cols.Length; j++) //string s in cols)
                        {
                            collst.Add(cols[j]);
                        }

                        if(srtodr=="asc")
                            RefreshGrids(collst, null);
                        if (srtodr == "desc")
                            RefreshGrids(null, collst);
                    }
                    else
                    {
                        RefreshGrids();
                    }

                    //16Apr2014
                    //must be excuted at the end after data is reloaded otherwise split is not refresh in statusbar. 
                    if (cmd.CommandSyntax.Contains("BSkyReloadDataset("))
                    {
                        Refresh_Statusbar();
                    }

                    //Finally show messages in output
                    SendToOutputWindow(dialogTitle, cmd.CommandSyntax);
                }
            }
        }

        protected void ExecuteXMLDefinedDialog()
        {
            //UAReturn retval = new UAReturn();
            retval.Success = true;
            AnalyticsData data = new AnalyticsData();
            data.SelectedForDump = selectedForDump;//10Jan2013
            data.PreparedCommand = cmd.CommandSyntax;//storing command
            data.Result = retval;
            //18Nov2013 replared by following data.AnalysisType = cmd.CommandSyntax;
            data.AnalysisType = cmd.CommandSyntax.Equals("bskyfrmtobj") ? ((UAMenuCommand)parameter).commandtype : cmd.CommandSyntax; //"T-Test"; For Parent Node name 02Aug2012
            data.InputElement = element;
            data.DataSource = ds;
            data.OutputTemplate = ((UAMenuCommand)parameter).commandoutputformat;
            UIController.AnalysisComplete(data);
        }

        //split sort compute recode
        protected void ExecuteNonXMLDefined()
        { }

        protected void ExecuteBatch(string[] commands)
        {
            commands = RemoveSplitCommandFromBatch(commands);
            bool IsSplitCommand= false;
            string allcommands = string.Join(";", commands);
            if(allcommands.Contains("BSkySetDataFrameSplit"))
            {
                IsSplitCommand = true;//current command is split/unsplit command and so we dont run split/unsplit in split loop.
            }
            string datasetname = UIController.GetActiveDocument().Name;//'Dataset1'
            string modifiedCommand = string.Empty;
            int splititrationcount = 1;
            try
            {

                if(AdvancedLogging)logService.WriteToLogLevel("ExtraLogs: BatchCommand begins.", LogLevelEnum.Info);

#region R Batch Command invoker
                //11Jul2014 need dialog title for commands those are single lined. if (CommandCountInBatch > 1)
                {
                    //BSkyFunctionInit()  // Execute in R
                    cmd.CommandSyntax = "BSkyBatchCommand(1)"; // starting batch command
                    analytics.ExecuteR(cmd, false, false);

                    //BSkySetCurrentDatasetName(datasetname, setDatasetIndex ="y") // Execute in R
                    cmd.CommandSyntax = "BSkySetCurrentDatasetName('" + datasetname + "', setDatasetIndex = \"y\")";
                    analytics.ExecuteR(cmd, false, false);

                    //uaSplitIterationCount = uaComputeSplitdataset(vars, datasetname) // Execute in R
                    //cmd.CommandSyntax = "BSkyComputeSplitdataset(" + selvars + ", '" + datasetname + "')";
                    cmd.CommandSyntax = "New.version.BSkyComputeSplitdataset('" + datasetname + "')";
                    object ocnt = analytics.ExecuteR(cmd, true, false);
                    if (ocnt != null)// && (ocnt is int))
                    {
                        //09Aug2016
                        //ISSUE: dialog fails to run and show erro message "Can't execute Command"
                        //sometime the above command fails to ge tsplit count. The reason that I found was UAgetDataframeSplitProp() returning NULL for DFsplit
                        //So DFsplit is NULL then the New.version.BSkyComputerSplitdataset() fails and 'ocnt', which is suppose to containg split count
                        // now contains error message. Which cant be converted to number.
                        //Every time we load / loadrefresh dataframe or datasets we create extra attributes by calling UAcreateExtraAttributes() in R.
                        // IF for some reason the attributes(in this case DFsplit ) get deleted then problem like this may occur.
                        //For easy fix here: we will see if 'ocnt' is not number then we assign 1 to splititrationcount below which means(no split)
                        //if count is higer than 1 means there is split and count represents the number of splits.

                        //if ocnt is numeric then splititerationcount will have the value else b will be false.
                        bool _success = Int32.TryParse(ocnt.ToString(), out splititrationcount);
                        if (!_success) splititrationcount = 1;
                       //09Aug2016 splititrationcount = Convert.ToInt32(ocnt);
                    }

                    ///ADDing right side Title for batch commands and associate it with left side leaf node////
                    PrintDialogTitle(dialogTitle);
                }
#endregion

                bool islastslice = false;
                bool isemptyslice = false;

                object slicename;
                string curslice;

                string treenocharscount = confService.GetConfigValueForKey("nooftreechars");//16Dec2013
                int treenodecharlen;
                bool result = Int32.TryParse(treenocharscount, out treenodecharlen);
                if (!result)
                    treenodecharlen = 15;


                if (splititrationcount > 1 && !IsSplitCommand) //dlgprop.HandleSplits)
                {
#region Split itrations
                    for (int i = 1; i <= splititrationcount; i++)
                    {
                        CommandOutput sliceco = null;//21Nov2013 Slicename, if any
                        isemptyslice = false;
                        slicename = null;
                        curslice = null;
                        //int uaGlobalDataSliceIndexToWorkOn = 1;
                        if (true)//04Jul2014 all dialogs get executed in Syntax   // !commands[0].Contains("bsky") && !commands[0].Contains("BSky")) // FOR NON-BSKY COMMANDS
                        {
                            cmd.CommandSyntax = "uaDatasetSliceIndex = New.version.BSkyGetNextDatasetSplitSlice('" + datasetname + "')";
                            analytics.ExecuteR(cmd, false, false);

                            //cmd.CommandSyntax = "uaDatasetSliceIndex$datasetSliceIndex";
                            //object ro = analytics.ExecuteR(cmd, true, false);
                            //uaGlobalDataSliceIndexToWorkOn = Int32.Parse(ro.ToString());

                            //Current Slice
                            cmd.CommandSyntax = "paste(BSkyComputeCurrentVarNamesAndFactorValues('" + datasetname + "'))";
                            slicename = analytics.ExecuteR(cmd, true, false).ToString();
                            curslice = slicename.ToString();
                            //21Nov2013 SendToOutputWindow("<<-- " + curslice +" -->>", "Slice:" + i.ToString());
                            //following block in place of above commented line
                            // Set slicename for syn edt session list
                            if (!curslice.Contains("Split = N"))
                            {
                                int startindex = curslice.IndexOf(',');
                                startindex = curslice.IndexOf(',', startindex + 1) + 2;
                                string treenodename = curslice.Substring(startindex).Trim();

                                sliceco = new CommandOutput();
                                sliceco.NameOfAnalysis = "Split:" + i.ToString();
                                sliceco.IsFromSyntaxEditor = false;

                                AUParagraph aup = new AUParagraph();
                                aup.FontSize = BSkyStyler.BSkyConstants.HEADER_FONTSIZE3;//10Nov2014
                                aup.FontWeight = FontWeights.DemiBold;
                                string tmpcurslice = curslice.Replace("Split = Y", "Split = True");
                                aup.Text = "\n\n<<-- " + tmpcurslice + " -->>";
                                aup.ControlType = treenodename.Length < treenodecharlen ? treenodename : treenodename.Substring(0, treenodecharlen);
                                sliceco.Add(aup);
                            }
                        }

#region /////// Checking if slice is empty or not /////// Do not process empty slice //////

                        //check uadatasets$lst[[index]] exists or not

                        //cmd.CommandSyntax = "eval(parse(text=paste('length(uadatasets$lst)', sep='')))";
                        //object dscount = analytics.ExecuteR(cmd, true, false);
                        //int lstdscount = 0; //total lst datasets
                        //if (dscount != null)
                        //{
                        //    Type t = dscount.GetType();
                        //    lstdscount = Convert.ToInt32(dscount);
                        //}
                        //if (uaGlobalDataSliceIndexToWorkOn <= lstdscount)
                        //{
                        //cmd.CommandSyntax = "eval(parse(text=paste('nrow(uadatasets$lst[[" + uaGlobalDataSliceIndexToWorkOn + "]])', sep='')))";
                        cmd.CommandSyntax = "eval(parse(text=paste('nrow(bskyCurrentDatasetSplitSliceObj)', sep='')))";
                        object rcont = analytics.ExecuteR(cmd, true, false);
                        int rowcount = rcont != null ? Int32.Parse(rcont.ToString()) : -1;
                        if (rowcount < 1)//empty slice. There should be empty slice. If no slice error "subscript out of bounds" will occur
                        {
                            isemptyslice = true;
                            //SendToOutputWindow("!!!!!!! Empty Slice !!!!!!! ", "Slice:" + i.ToString());
                            //continue;
                        }
                        //}
#endregion

                        if (!isemptyslice)
                        {
                            cmd.CommandSyntax = GetSliceSubstitutedCommands(commands, splititrationcount);
                        }
                        else
                        {
                            if (sliceco != null) // There is no slice when slice==null.(No Split dataset)
                            {
                                cmd.CommandSyntax = null; // no command is to be executed for empty slice.

                                //adding one more AUPara to sliceco for empty slice
                                string treenodename = "Empty Slice : " + i.ToString();
                                AUParagraph aup = new AUParagraph();
                                aup.FontSize = BSkyStyler.BSkyConstants.TEXT_FONTSIZE;//10Nov2014
                                aup.Text = "!!!!!!! Empty Slice !!!!!!!";
                                aup.ControlType = treenodename.Length < treenodecharlen ? treenodename : treenodename.Substring(0, treenodecharlen);
                                sliceco.Add(aup);
                            }
                        }
                        if (i == splititrationcount)//last slice under execution
                            islastslice = true;
                        ExecuteInSyntaxEditor(true, dialogTitle, sliceco, islastslice); //Execute command batch in syntax editor in one session.
                    }//split itration on command batch
#endregion
                }
                else//No Split
                {
                    cmd.CommandSyntax = GetSliceSubstitutedCommands(commands, splititrationcount);
                    ExecuteInSyntaxEditor(true, dialogTitle);
                }
                //till this point command has already been executed. So now we store this command for "History"
            }//Try
            finally
            {
                ////////  THIS MUST EXECUTE AT THE END OTHERWISE THIS WILL IMPACT BADLY ON R SIDE //////////
                //if (CommandCountInBatch > 1)
                {

                    if (AdvancedLogging) logService.WriteToLogLevel("ExtraLogs: BatchCommand ends.", LogLevelEnum.Info);

                    cmd.CommandSyntax = "BSkyBatchCommand(0)"; // ending batch command
                    analytics.ExecuteR(cmd, false, false);
                }
            }
        }

        //30Apr2015 Output Dialog Title
        private void PrintDialogTitle(string title)
        {
            CommandOutput batch = new CommandOutput();
            batch.NameOfAnalysis = "Batch Command";
            batch.IsFromSyntaxEditor = false;
            batch.Insert(0, new BSkyOutputOptionsToolbar());

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
            aup.ControlType = "Title";
            batch.Add(aup);

            //printing dataset name below main title
            string dsname = UIController.GetActiveDocument().FileName;
            string dsnameinapp = UIController.GetActiveDocument().Name;
            AUParagraph dataset = new AUParagraph();
            dataset.FontSize = BSkyStyler.BSkyConstants.TEXT_FONTSIZE;
            dataset.Text = "[" + dsnameinapp + "] - " + dsname;
            dataset.ControlType = "Dataset Name";
            batch.Add(dataset);

            SyntaxEditorWindow sewindow = LifetimeService.Instance.Container.Resolve<SyntaxEditorWindow>();
            sewindow.AddToSession(batch); 
        }

        //If there are more than 1 command in a batch and one of the command among them is BSkySetDataFrameSplit
        //Then remove this BSkySetDataFrameSplit command from batch.
        //As we  expect to run split/unsplit(BSkySetDataFrameSplit) command separately, and not inside of command batch
        protected string[] RemoveSplitCommandFromBatch(string[] commands)
        {
            //BSkySetDataFrameSplit
            int commandcount = commands.Length;
            if (commandcount > 1)
            {
                for (int i = 0; i < commandcount; i++)
                {
                    if (commands[i].Contains("BSkySetDataFrameSplit"))
                    {
                        commands[i] = string.Empty;
                    }
                }
            }
            return commands;
        }

        protected void ExecuteSplit()
        {
            if (cmd.CommandSyntax.Contains("BSkySetDataFrameSplit("))///executes when SPLIT is fired from menu
            {
                bool setsplit = false;
                int startind = 0; int endind = 0;
                if (cmd.CommandSyntax.Contains("col.names"))
                {
                    startind = cmd.CommandSyntax.IndexOf("c(", cmd.CommandSyntax.IndexOf("col.names"));// index of c(
                }
                else
                {
                    startind = cmd.CommandSyntax.IndexOf("c(");// index of c(
                }
                if (startind > 0)
                    endind = cmd.CommandSyntax.IndexOf(")", startind);
                if (startind > 0 && endind > startind)
                {
                    int len = endind - startind + 1; // finding the length of  c(......)
                    string str = cmd.CommandSyntax.Substring(startind, len); // this will contain c('tg0','tg1') or just c()
                    string ch = null;
                    if (str.Contains("'")) ch = "'";
                    if (str.Contains('"')) ch = "\"";
                    if (ch != null && ch.Length > 0)
                    {
                        int i = str.IndexOf(ch);
                        int j = -1;
                        if (i >= 0) j = str.IndexOf(ch, i + 1);
                        if (j < 0) j = i + 1;
                        string sub = str.Substring(i + 1, (j - i - 1)).Trim();
                        if (i < 0)
                            i = str.IndexOf("'");
                        if (i >= 0)
                        {
                            if (sub.Length > 0)
                                setsplit = true;
                        }
                    }
                }
                //////////  Setting/Unsetting Macro  for SPLIT //////////
                if (setsplit)
                {
                    OutputHelper.AddGlobalObject(string.Format("GLOBAL.{0}.SPLIT", UIController.GetActiveDocument().Name), element);
                    window.Template = null; //11Mar2013 release the XAML object. ie obj is no more child of window.
                    SendToOutputWindow(dialogTitle, cmd.CommandSyntax);
                    Refresh_Statusbar();// RefreshGrids();
                    return;// no need to do any thing further
                }
                else // unset split
                {
                    OutputHelper.DeleteGlobalObject(string.Format("GLOBAL.{0}.SPLIT", UIController.GetActiveDocument().Name));
                    if (window != null && window.Template != null)
                        window.Template = null; //11Mar2013 release the XAML object. ie obj is no more child of window.
                    SendToOutputWindow(dialogTitle, cmd.CommandSyntax);
                    Refresh_Statusbar();// RefreshGrids();
                    return;// no need to do any thing further
                }
            }

        }

        protected void ExecuteInSyntaxEditor(bool ExecuteCommand, string sessionTitle = "", CommandOutput sliceco = null, bool islastslice = true)
        {

            if (AdvancedLogging) logService.WriteToLogLevel("ExtraLogs: Before executing in Syntax.", LogLevelEnum.Info);


            //Launch Syntax Editor window with command pasted /// 29Jan2013
            MainWindow mwindow = LifetimeService.Instance.Container.Resolve<MainWindow>();
            ////// Start Syntax Editor  //////
            SyntaxEditorWindow sewindow = LifetimeService.Instance.Container.Resolve<SyntaxEditorWindow>();
            sewindow.FElement = element;//set current dialog.
            sewindow.MenuParameter = parameter; //sending menu.xml command attributes for current command
            sewindow.Owner = mwindow;
            //21Nov2013. if there is slicename add it first to the syntax editor output session list
            if (sliceco != null)
                sewindow.AddToSession(sliceco);
            if (cmd.CommandSyntax != null && cmd.CommandSyntax.Length > 0)
                sewindow.RunCommands(cmd.CommandSyntax, dlgprop);//, sessionheader);

            //22Nov2013
            //if sessionTitle is empty that means there are more (split)slices to execute
            //when the last slice is ready for execution that time sessionTitle 
            //will have the main title for whole session
            if (islastslice)//sessionTitle != null && sessionTitle.Length > 0)
                sewindow.DisplayAllSessionOutput(sessionTitle);
            else
                return;//go get another slice. Do not process rest of the code till last slice comes in.

            if (AdvancedLogging) logService.WriteToLogLevel("ExtraLogs: After executing in Syntax.", LogLevelEnum.Info);

 
        }

        //19Nov2013 substitute slicename$varname in place of varname
        private string GetSliceSubstitutedCommands(string[] commands, int splititrationcount)
        {
            //StringBuilder modifiedCommand = null;
            //07Jan2015 lines are commented because we dont want to restrict execution to only non-bsky command 
            //      and batch with more than 1 commands
            // And Split command should never be a part of split processing like other commands (eg.. running split/unsplit must not run on all the slices)

            string finalModifiedCommand = string.Join(";", commands);// string.Empty;
            string allcommands = string.Join(";", commands);
            //modifiedCommand = new StringBuilder(string.Join(";", commands));
            ////07Jan2015 if (!commands[0].Contains("bsky"))// FOR NON-BSKY COMMANDS
            {
                //Colnames must be uadatasets$lst[[uaGlobalDataSliceIndexToWorkOn]]$COLNAME
                ///Write some code to conver tg0 to uadatasets$lst$DatasetX$tg0 in case of command batch
                //07Jan2015 if (CommandCountInBatch > 1)//Assuming that there will be more than 1 command in command batch
                {
                    if (!allcommands.Contains("BSkySetDataFrameSplit"))
                    {
                        string currentDatasetName = UIController.GetActiveDocument().Name.Trim();
                        string slicedatasetname = splititrationcount == 1 ? currentDatasetName : "bskyCurrentDatasetSplitSliceObj";//"uadatasets$lst[[" + uaGlobalDataSliceIndexToWorkOn + "]]$";
//modifiedCommand = modifiedCommand.Replace(currentDatasetName, slicedatasetname);//9jun2014 for slice datasetname is different which is bskyCurrentDAtasetSplitSliceObj

                        //using regular exression for find/replace in commands. There could be say
                        // 'summary; dataset and 'summary' function. Diff will be 'summary' function will have '(' after it.
            //string currentDatasetName="summary";
            //string slicedatasetname = "{REPLACEMENT}";
            string pattern1 = currentDatasetName + @"\s*";//pattern with space. To remove extra spaces
            string pattern2 = @"[^:]" + currentDatasetName + @"\s*[^\(]";//patter to remove dataset names only and not other matching wordslike function names
            string spaceRemoved = Regex.Replace(allcommands, pattern1, currentDatasetName);//removing extra space after words that matches dataset name
            finalModifiedCommand = Regex.Replace(spaceRemoved, pattern2, delegate(Match match)
            {
                string mstr = match.ToString();
                return mstr.Replace(currentDatasetName, slicedatasetname);
            });
                    }
                    //09Jun2014 There may not be a need to run following loop
                    //foreach (DataSourceVariable singlevar in dsvs)
                    //{
                    //    if (!allcommands.Contains(singlevar.Name.Trim())) //if singlevar not present goto next iteration.
                    //        continue;
                    //    //Later we ned to fix this to substitute vars those are not having any quotaion marks eg ... accid
                    //    //modifiedCommand = modifiedCommand.Replace("'" + singlevar.Name + "'", slicedatasetname + "$" + singlevar.Name);
                    //    if (slicedatasetname.Equals("bskyCurrentDatasetSplitSliceObj"))
                    //    {
                    //        //Replace all DatasetN$ with bskyCurrentDatasetSplitSliceObj$ ( eg in Dataset1$gender )
                    //        //9jun2014 modifiedCommand = modifiedCommand.Replace(currentDatasetName + "$", slicedatasetname + "$");

                    //    }
                    //}
                }
            }
            return finalModifiedCommand; //24Jan2016 modifiedCommand.ToString();
        }

        protected virtual void OnPreExecute(object param)
        {
            PreExecuteSub();
            UAMenuCommand command = (UAMenuCommand)param;
            ///Store command for "History" menu // 04Mar2013
            TemplateFileName = command.commandtemplate;// @".\Config\OneSampleCommand.xaml";

            
            //MessageBox.Show("Executing Dialog name: " + Path.GetFileNameWithoutExtension(TemplateFileName));

            ///04Sep2015 If DefaultPacakges R pacakge(s) are not loaded we should not execute any dialog. (not sure if this also stops
            /// execution from SyntaxEditor BSkyAppData.BSkyAppDirConfigPath
            string InstReqPkg = string.Format(@"{0}Install ALL required R packages from CRAN.xaml", BSkyAppData.RoamingUserBSkyConfigL18nPath);
            string RVerDetails = string.Format(@"{0}R version details.xaml", BSkyAppData.RoamingUserBSkyConfigL18nPath);
            if (TemplateFileName.Equals(InstReqPkg) || TemplateFileName.Equals(RVerDetails))
            {
                //MessageBox.Show("It Worked!!!!");
                ExecutePackageRelatedDialogs(param);
                canExecute = false;
                return;
            }
            else
            {
                if (!Window1.DatasetReqRPackagesLoaded)
                {
                    canExecute = false;

                    string misspkgs = Window1.DatasetReqPackages;
                    string firstmsg = BSky.GlobalResources.Properties.Resources.BSkyNeedsRPkgs+"\n\n";
                    string msg = "\n\n" + BSky.GlobalResources.Properties.Resources.InstallReqRPkgFrmCRAN + "\n"+BSky.GlobalResources.Properties.Resources.InstallUpdatePkgMenuPath;
                    MessageBox.Show(firstmsg + misspkgs + msg, BSky.GlobalResources.Properties.Resources.ErrReqRPkgMissing, MessageBoxButton.OK, MessageBoxImage.Error);

                    return;
                }
            }
        }

        private void PreExecuteSub()//16May2013
        {
            UIController = LifetimeService.Instance.Container.Resolve<IUIController>();

            ds = UIController.GetActiveDocument();
            
            ////24Apr2014  If no dataset is open, we still need to run some commands from Syntax, those do not require Dataset
            //so following 'if' is commented out
            //if (ds == null)
            //{
            //    canExecute = false;
            //    return;
            //}



            if (ds != null) //24Apr2014 
            {
                List<string> datasetnames = new List<string>();
                int i = 0;

                Variables = new ObservableCollection<DataSourceVariable>(ds.Variables);
                datasetnames = UIController.GetDatasetNames();
                int count = datasetnames.Count;
                Datasets = new List<DatasetDisplay>();
                for (i = 0; i < count; i++)
                {
                    DatasetDisplay temp = new DatasetDisplay();
                    temp.Name = datasetnames[i];
                    temp.ImgURL = "../Resources/ordinal.png";
                    Datasets.Add(temp);


                }
            }
        }

        protected virtual void OnExecute(object param) { }

        protected virtual void OnPostExecute(object param)
        {
            // SaveInHistory(param);
            //ComputeVar cv = new ComputeVar();
            //cv.ShowDialog();
        }

#endregion

#region Other Methods - Common code (History_save, Send executed command to output etc..)

        //string mystr = "something + c(\"\'tg0\', \'tg1\',\'tg2\'\",\'tg3\') + another + c(\"\'tg4\', \'tg5\',\'tg6\'\",\'tg7\') + thing"; // c("'tg0', 'tg1','tg2'",'tg3')
        // c("'tg0', 'tg1', 'tg2'", 'tg3')  ---> c("tg0", "tg1", "tg2", 'tg3')
        // tg3 was not within double quotes so it should not be converted to title ( should not have  double quotes)
        private string VarnamesToTitle(string str) //23Apr2013
        {

            char[] chrarr = str.ToCharArray();

            bool titleexists = (Regex.IsMatch(str, @"""\s*\'") && Regex.IsMatch(str, @"\'\s*""")) ? true : false;
            if (titleexists)
            {
                MatchCollection mcstrt = Regex.Matches(str, @"""\s*'");
                MatchCollection mcend = Regex.Matches(str, @"'\s*""");
                char ch;
                if (mcstrt.Count == mcend.Count) // if formatting is correct, ie.. opening " has a matching closing "
                {
                    for (int i = 0; i < mcstrt.Count; i++)
                    {
                        int start = mcstrt[i].Index;
                        int end = mcend[i].Index;

                        /// now remove old double quotes & from start to end change all single quotes to double ///
                        for (int j = start; ; j++)
                        {
                            ch = chrarr[j];
                            if (chrarr[j] == '"')
                            {
                                chrarr[j] = ' ';
                            }
                            else if (chrarr[j] == '\'')
                            {
                                chrarr[j] = '"';
                            }
                            if (j > start && ch == '"')
                                break;
                        }
                    }
                    str = new string(chrarr);
                }
            }
            return str;
        }

        //This will paste in original Syntax window, that was separate from output window.
        private void PasteSyntax()
        {
            //copy to clipboard and return for this function
            //Clipboard.SetText(cmd.CommandSyntax);

            //Launch Syntax Editor window with command pasted /// 29Jan2013
            MainWindow mwindow = LifetimeService.Instance.Container.Resolve<MainWindow>();
            ////// Start Syntax Editor  //////
            SyntaxEditorWindow sewindow = LifetimeService.Instance.Container.Resolve<SyntaxEditorWindow>();
            sewindow.Owner = mwindow;
            //31May2015. No need to paste the R commented '#' commands in syntax
            //string syncomment = "# Use BSkyFormat(obj) to format the output.\n" +
            //    "# UAloadDataset(\'" + UIController.GetActiveDocument().FileName.Replace('\\', '/') +
            //    "\',  filetype=\'SPSS\', worksheetName=NULL, replace_ds=FALSE, csvHeader=TRUE, datasetName=\'" +
            //    UIController.GetActiveDocument().Name + "\' )\n";
            string syncomment = "\n";//31May2015
            sewindow.PasteSyntax(syncomment + cmd.CommandSyntax);//paste command
            sewindow.Show();
            sewindow.WindowState = WindowState.Normal;
            sewindow.Activate();
        }

        //This will paste in the syantax window that is attached to output
        private void PasteInOutputSyntax()
        {
            //copy to clipboard and return from this function
            //Clipboard.SetText(cmd.CommandSyntax);

            #region Create required R package loading syntax for paste
            string pkgs = dlgprop.RequiredRPackages;
            StringBuilder RPkgLoadSyntax = new StringBuilder();
            string title = "## [" + dlgprop.DialogTitle + "]\n";
            string ReqRPkgSyntax = string.Empty; 
            if (!string.IsNullOrEmpty(pkgs))
            {
                char[] chars = new char[1] { ';' };
                string[] dlgpkgarr = pkgs.Split(chars);
                string current = string.Empty;
                for (int i = 0; i < dlgpkgarr.Length; i++)
                {
                    current = dlgpkgarr[i].Trim();
                    if (current.Length < 1)//may be someone added too many semi-colons in between. So package name will be just "".
                        continue; // dont consider empty string package name and jump to next.
                    RPkgLoadSyntax.Append("require(" + current + ");\n");
                }
                ReqRPkgSyntax = RPkgLoadSyntax.Append("\n").ToString();
            }
            #endregion
            #region Get Active output Window
            //////// Active output window ///////
            OutputWindowContainer owc = (LifetimeService.Instance.Container.Resolve<IOutputWindowContainer>()) as OutputWindowContainer;
            OutputWindow ow = owc.ActiveOutputWindow as OutputWindow; //get currently active window
#endregion

            //string filetype = UIController.GetActiveDocument().FileType;
            //31May2015. No need to paste the R commented '#' commands in syntax
            //string syncomment = "#UIController.GetActiveDocument() Use BSkyFormat(obj) to format the output.\n" +
            //    "# UAloadDataset(\'" + UIController.GetActiveDocument().FileName.Replace('\\', '/') +
            //    "\',  filetype=\'"+filetype+"\', worksheetName=NULL, replace_ds=FALSE, csvHeader=TRUE, datasetName=\'" +
            //    UIController.GetActiveDocument().Name + "\' )\n";
            string syncomment = "\n";//31May2015
            ow.PasteSyntax(syncomment + title + ReqRPkgSyntax  + cmd.CommandSyntax);//paste command
            ow.Show();
            if(ow.WindowState == WindowState.Minimized)
                ow.WindowState = WindowState.Normal;
            ow.ActivateSyntax();
        }

#region Save in History of MainWindow and OutputWindow(s)
        //Save executed command in History menu in Main window
        private void SaveInHistory()
        {
            SaveInMainWinHistory();
            SaveInAllOutputWinHistory();
        }
        //04Aug2014 Remove command from History menu in MainWindow
        private void RemoveFromHistory()
        {
            RemoveFromMainWinHistory();
            RemoveFromAllOutputWinHistory();
        }

#region////////////////   MAIN-WINDOW(s) HISTORY MENU   //////////////////
        //Save executed command in History menu in Main window
        private void SaveInMainWinHistory()
        {
            ///Store executed command in "History" menu /// 04March2013
            UAMenuCommand command = (UAMenuCommand)parameter;
            Window1 appWindow = LifetimeService.Instance.Container.Resolve<Window1>();
            if (ds != null)
            {
                appWindow.History.AddCommand(ds.Name, command);
            }
        }
        //04Aug2014 Remove command from History menu in MainWindow
        private void RemoveFromMainWinHistory()
        {
            ///Store executed command in "History" menu /// 04March2013
            UAMenuCommand command = (UAMenuCommand)parameter;
            Window1 appWindow = LifetimeService.Instance.Container.Resolve<Window1>();
            if (ds != null)
                appWindow.History.RemoveCommand(ds.Name, command);
        }
#endregion

#region////////////////   OUTPUT-WINDOW(s) HISTORY MENU   //////////////////

        //17Jul2015 Save in History menu of OutputWindow(s)
        private void SaveInAllOutputWinHistory()
        {
            OutputWindowContainer owc = (LifetimeService.Instance.Container.Resolve<IOutputWindowContainer>()) as OutputWindowContainer;
            foreach(string key in owc.AllOutputWindows.Keys)
            {
                if (owc.AllOutputWindows.ContainsKey(key))
                {
                    OutputWindow ow = owc.AllOutputWindows[key] as OutputWindow;
                    SaveInOutputWinHistory(ow);
                }
 
            }
        }


        //17Jul2015 Remove command from History menu in OutputWindow(s)
        private void RemoveFromAllOutputWinHistory()
        {
            OutputWindowContainer owc = (LifetimeService.Instance.Container.Resolve<IOutputWindowContainer>()) as OutputWindowContainer;
            foreach (string key in owc.AllOutputWindows.Keys)
            {
                if (owc.AllOutputWindows.ContainsKey(key))
                {
                    OutputWindow ow = owc.AllOutputWindows[key] as OutputWindow;
                    RemoveFromOutputWinHistory(ow);
                }

            }
        }


        //17Jul2015 Save in History menu of OutputWindow whose reference is passed
        private void SaveInOutputWinHistory(OutputWindow ow )
        {
            UAMenuCommand command = (UAMenuCommand)parameter;
            if (ds != null)
            {
                ow.History.AddCommand(ds.Name, command);
            }
        }


        //17Jul2015 Remove command from History menu in OutputWindow whose reference is passed
        private void RemoveFromOutputWinHistory(OutputWindow ow )
        {
            ///Store executed command in "History" menu /// 04March2013
            UAMenuCommand command = (UAMenuCommand)parameter;
            if (ds != null)
                ow.History.RemoveCommand(ds.Name, command);
        }

#endregion

#endregion

        //Send executed command to output window. So, user will know what he executed
        private void SendToOutputWindow(string title, string message, bool iscommand=true)//26Mar2013
        {
#region Get Active output Window
            //////// Active output window ///////
            OutputWindowContainer owc = (LifetimeService.Instance.Container.Resolve<IOutputWindowContainer>()) as OutputWindowContainer;
            OutputWindow ow = owc.ActiveOutputWindow as OutputWindow; //get currently active window
#endregion
            ow.AddMessage(title, message, iscommand);
        }

#endregion

#region Analysis Commands ( One Sample, Crosstab etc..)

        //selectedForDump is only for SyntaxEditor. (if we want to dump command executed from Syntax Editor or not)
        private void ExecuteAnalysisCommands()
        {
            //UAReturn retval = new UAReturn();
            retval.Success = true;
            AnalyticsData data = new AnalyticsData();
            data.SelectedForDump = selectedForDump;//10Jan2013
            data.PreparedCommand = cmd.CommandSyntax;//storing command
            data.Result = retval;
            //18Nov2013 replared by following data.AnalysisType = cmd.CommandSyntax;
            data.AnalysisType = cmd.CommandSyntax.Equals("bskyfrmtobj") ? ((UAMenuCommand)parameter).commandtype : cmd.CommandSyntax; //"T-Test"; For Parent Node name 02Aug2012
            data.InputElement = element;
            data.DataSource = ds;
            data.OutputTemplate = ((UAMenuCommand)parameter).commandoutputformat;
            UIController.AnalysisComplete(data);
        }

#endregion

#region Refreshing Grids etc..

        //Refreshes Both Data and Variable Grids and echoes the command in output window.
        public void DatasetRefreshAndPrintTitle(string title) //This can be called to refresh Grids from Syn Editor
        {
            RefreshGrids();
            if (window != null)
                window.Template = null; //19Mar2013 release the XAML object. ie obj is no more child of window.

            if (cmd != null && title != null)//16May2013
                SendToOutputWindow(title, cmd.CommandSyntax);
            return;
        }

        //16May2013 //This can/may be called to refresh Grids from App Main window
        public void RefreshGrids(List<string> sortasccolnames = null, List<string> sortdesccolnames = null)
        {
            PreExecuteSub();
            if (ds != null)//19Mar2013
            {

                IUnityContainer container = LifetimeService.Instance.Container;
                IDataService service = container.Resolve<IDataService>();
                //17Nov2017 We must store CSV file Options first because they may get lost(actually they are).
                OpenDataFileOptions odfo = new OpenDataFileOptions();
                try
                {
                    odfo.DecimalPointChar = ds.DecimalCharacter.ToCharArray()[0];
                    odfo.FieldSeparatorChar = ds.FieldSeparator.ToCharArray()[0];
                    odfo.HasHeader = ds.HasHeader;
                    odfo.IsBasketData = ds.IsBasketData;
                }
                catch (Exception ex)
                {
                    logService.WriteToLogLevel("CSV options not found(or lost):", LogLevelEnum.Error, ex);
                    odfo = null;//setting it to null as it does not have CSV options. Also null is good for easy comparision below. But load Refresh will also abort.
                }
                ds = service.Refresh(ds);//we are restoring CSV realted new options in this Refresh() so need to do below

                //31May2018 if (ds != null && odfo!=null)  //if (ds != null)// REFRESH ABORTS BECAUSE OF ODFO=NULL
                if( (ds != null) && ( (ds.FileType == "CSV" && odfo != null) || ds.FileType != "CSV") )
                {
                    //No harm in using following. But similar to following has been done in service.Refresh(sd) statement above.
                    //17Nov2017 Restoring
                    //ds.DecimalCharacter= odfo.DecimalPointChar.ToString();
                    //ds.FieldSeparator=odfo.FieldSeparatorChar.ToString() ;
                    //ds.HasHeader= odfo.HasHeader;
                    //ds.IsBasketData=odfo.IsBasketData;


                    UIController.sortasccolnames = sortasccolnames;//11Apr2014
                    UIController.sortdesccolnames = sortdesccolnames;//11Apr2014
                    //UIController.sortorder = sortorder; //14Apr2014
                    UIController.RefreshGrids(ds);
                    //04Aug2014 Now as Dataset is refreshed there is a need to remove existing dialog for that dataset
                    //as, now we may have different cols, we dont want to use dialog in memory that was refering to
                    // column names those are not anymore present
                    //RemoveOldDialogsFromMemory();
                    DataPanel dpnl = ((UIController.GetTabItem(ds) as TabItem).Content as DataPanel);
                    dpnl.Variables = ds.Variables;
                    dpnl.arrangeVarGridCols();
                }
            }
        }

        //16Jul2015 refresh both the grids when 'refresh' icon is clicked
        public void RefreshBothGrids(List<string> sortasccolnames = null, List<string> sortdesccolnames = null)
        {
            BSkyMouseBusyHandler.ShowMouseBusy();// ShowMouseBusy_old();
            PreExecuteSub();
            if (ds != null)//19Mar2013
            {
                IUnityContainer container = LifetimeService.Instance.Container;
                IDataService service = container.Resolve<IDataService>();

                //03Jul2016 taking a backup
                string DSname = ds.Name;
                //string Ftype = ds.FileType;
                string DSFname = ds.FileName; 
                DataSource ds_backup = ds;

                ds = service.Refresh(ds); //we are restoring CSV realted new options in this Refresh() so need to do below
                if (ds != null)
                {
                    //for fixing Load-from-disk after refresh icon clicked.
                    //Open carsales.sav. Make some Factor & numeric cols cell NA. Click refresh icon. Click Reload from disk option.
                    //this does note reloads from dis because following info went missing due to which odfo==null in
                    // RefreshGrids() function (in this file) and Refresh aborts. So by following we are trying to make odfo!=null.
                    //No harm in using following. But similar to following has been done in service.Refresh(sd) statement above.
                    //ds.DecimalCharacter = ds_backup.DecimalCharacter;
                    //ds.FieldSeparator = ds_backup.FieldSeparator;
                    //ds.HasHeader = ds_backup.HasHeader;
                    //ds.IsBasketData = ds_backup.IsBasketData;

                    UIController.sortasccolnames = sortasccolnames;//11Apr2014
                    UIController.sortdesccolnames = sortdesccolnames;//11Apr2014
                    //UIController.sortorder = sortorder; //14Apr2014
                    UIController.RefreshBothGrids(ds);
                    //04Aug2014 Now as Dataset is refreshed there is a need to remove existing dialog for that dataset
                    //as, now we may have different cols, we dont want to use dialog in memory that was refering to
                    // column names those are not anymore present
                    //RemoveOldDialogsFromMemory();
                }
                else //03Jul2016 In case Dataset becomes NULL we need to empty the datagrid.
                {
                    ds = new DataSource();
                    ds.Variables = new List<DataSourceVariable>();
                    ds.FileName = ds_backup.FileName;// DSFname;
                    ds.Name = ds_backup.Name;// DSname;
                    ds.SheetName = UtilFunctions.GetSheetname(ds_backup); 

                    ds.DecimalCharacter = ds_backup.DecimalCharacter;
                    ds.FieldSeparator = ds_backup.FieldSeparator;
                    ds.HasHeader = ds_backup.HasHeader;
                    ds.IsBasketData = ds_backup.IsBasketData;

                    //ds.FileType = Ftype;
                    UIController.RefreshBothGrids(ds);

                    //12Sep2016 Generating the message for null dataset
                    CommandRequest msg = new CommandRequest();

                    //msg.CommandSyntax = ds_backup.Name + " been set to NULL. The reason may be, running some analysis or command on the dataset.";
                    //string title = "NULL Dataset.";

                    msg.CommandSyntax = DSname+ BSky.GlobalResources.Properties.Resources.DatasetBecameNull;
                    string title = BSky.GlobalResources.Properties.Resources.DatasetNull;

                    if (msg != null && title != null)//16May2013
                        SendToOutputWindow(title, msg.CommandSyntax);
                }
            }
            BSkyMouseBusyHandler.HideMouseBusy();// HideMouseBusy_old();
        }

        //no need of this function. Its already running eleswhere on grid refresh
        protected void RemoveOldDialogsFromMemory()
        {
            string partialkey = UIController.GetActiveDocument().FileName + UIController.GetActiveDocument().Name;
            Dictionary<string, object> SessionDialogs = sdc.SessionDialogList;
            List<string> keylist = new List<string>();
            KeyValuePair<string, object> kvp;

            //collecting all the keys those are supposed to be removed
            for (int idx = 0; idx < SessionDialogs.Count; idx++)
            {
                kvp = SessionDialogs.ElementAt(idx);
                if (kvp.Key.Contains(partialkey))
                    keylist.Add(kvp.Key);
            }
            //removing all the dialogs whose keys were stored in keylist.
            for (int i = 0; i < keylist.Count; i++)
            {
                SessionDialogs.Remove(keylist.ElementAt(i));
            }
        }

        //05Dec2013 refresh statusbar in main grid for split info
        public void Refresh_Statusbar()
        {
            UIController.RefreshStatusbar();
        }

#endregion

#region Syntax Editor Analysis-NonAnalysis Commands ( One Sample, Crosstab etc..)

        ////// For Analysis Command Execution from Syntax Editor /////28Mar2013 Using this one and not the other one below this method
        public void ExecuteSyntaxEditorx(object param, bool SelectedForDump)
        {
            parameter = param;//set Global var.
            selectedForDump = SelectedForDump;//set Global var
            OnPreExecute(param);
            if (!canExecute) return;
            object obj = null;
            string dialogcommandstr = null;
            string HistMenuText = string.Empty;//29Mar2013
            try
            {
                //here TemplateFileName xaml will have same name as the analysis command function name
                // say- function called frm SynEdtr was 'bsky.my.func()' then in bin\Config\ 
                // dialog xaml, 'bsky.my.func.xaml' and
                // output template file 'bsky.my.func.xml' must exist
                // ie.. func name = xaml name = xml name
                XmlReader xmlr = XmlReader.Create(TemplateFileName);
                xmlr.ReadToFollowing("BSkyCanvas");

                //Following commented code can be used to Add command to "History" menu when executed from Syntax Editor
                //But "Title" from XAML may not same as "text" attr from menu.xml
                //So the same command executed from Syntax Editor and Main App's menu will show duplicate
                ////xmlr.MoveToAttribute("Title");//29Mar2013 For "History" Text
                ////HistMenuText = xmlr.Value;//29Mar2013 For "History" Text
                ////UAMenuCommand uam = (UAMenuCommand)param;
                ////uam.text = HistMenuText;
                ////parameter = uam;//set Global var

                xmlr.MoveToAttribute("CommandString");
                dialogcommandstr = xmlr.Value.Replace(" ", string.Empty).Replace('\"', '\'');
                xmlr.Close();
                BSkyCanvas.applyBehaviors = false; //21Jul2014
                obj = System.Windows.Markup.XamlReader.Load(XmlReader.Create(TemplateFileName));
            }
            catch (Exception ex)
            {
                //18Aug2014 Supressing this message box as we dont need it. But we still pass message in log.
                //MessageBox.Show("XAML missing or has improper format! Could not create template from " + TemplateFileName);
                logService.WriteToLogLevel("SynEdtr:Could not create template from " + TemplateFileName, LogLevelEnum.Error, ex);
                GenerateOutputTablesForNonXAML(param);
                return;
            }

            //BSkyCanvas cs = obj as BSkyCanvas;
            //BSkyCanvas.applyBehaviors = true;
            //BSkyCanvas.first = cs;
            //var converter = new System.Windows.Media.BrushConverter();
            ////base.Background = (Brush)converter.ConvertFromString("#FFEDefFf");
            //cs.Background = (Brush)converter.ConvertFrom("#FFEEefFf");


            BSkyCanvas.applyBehaviors = true;//21Jul2014
            element = obj as FrameworkElement;
            window = new BaseOptionWindow();
            window.Template = element;
            element.DataContext = this; // loading vars in left listbox(source)
            ///window.ShowDialog();
            commandwindow = element;
            ////////////test///////
            //// take two strings and then try to make merged dictionary. remove extra spaces. replace " with '
            //string bksytemplate="bsky.CrossTable(x=c({Rows}),y=c({columns}),layers=c({layers}),datasetname='{%DATASET%}',chisq={chisq})";
            //string bskycommand="bsky.CrossTable(x=c('store','contact'),y='regular',layers=c('gender'),datasetname='Dataset1',chisq=FALSE)";
            //string dialogcommandstr = "bsky.one.sm.t.test(vars=c({SelectedVars}),mu={testValue},conf.level=0.89,datasetname='{%DATASET%}',missing=0)";
            string bskycommand = ((UAMenuCommand)parameter).bskycommand.Replace(" ", string.Empty);//"bsky.one.sm.t.test(vars=c('tg0','tg2','tg3'),mu=30,conf.level=0.89,datasetname='Dataset1',missing=0)";

            Dictionary<string, string> dialogkeyvalpair = new Dictionary<string, string>();//like: key=mu, val= {testValue}
            Dictionary<string, string> bskycommandkeyvalpair = new Dictionary<string, string>();//like: key=mu, val= 30
            Dictionary<string, string> merged = new Dictionary<string, string>();//like: key=testValue, val = 30

            OutputHelper.getArgumentSetDictionary(dialogcommandstr, dialogkeyvalpair);
            OutputHelper.getArgumentSetDictionary(bskycommand, bskycommandkeyvalpair);
            OutputHelper.MergeTemplateCommandDictionary(dialogkeyvalpair, bskycommandkeyvalpair, merged);

            foreach (KeyValuePair<string, string> pair in merged)
            {
                if (!pair.Key.Contains("%"))// This should only skip macros(words enclosed within %) and not other formats.
                {
                    OutputHelper.SetValueFromSynEdt(element, pair.Key, pair.Value); //Filling dialog with values
                }
            }
            //foreach (Match m in mc)
            //{
            //    //Console.WriteLine(s.Index + " : " + s.ToString());// {SelectedVars} {testValue} {%DATASET%}
            //    if (!m.ToString().Contains("%"))
            //    {
            //        args = OutputHelper.getArgument(bskycommand, m.Index);
            //        uiElementName = m.ToString().Replace('{', ' ').Replace('}', ' ').Trim();
            //        OutputHelper.SetValueFromSynEdt(element, uiElementName, args);
            //    }
            //}
            //OutputHelper.SetValueFromSynEdt(element, "SelectedVars");
            //OutputHelper.SetValueFromSynEdt(element, "testValue");

            //For Chisq check box only
            //FrameworkElement chkElement = element.FindName("chisq") as FrameworkElement;
            if (true)//window.DialogResult.HasValue && window.DialogResult.Value)
            {
                //analytics can be sent from parent function(in SyntaxEditorWindow)
                cmd = new CommandRequest();

                OutputHelper.Reset();
                OutputHelper.UpdateMacro("%DATASET%", UIController.GetActiveDocument().Name);
                OutputHelper.UpdateMacro("%MODEL%", UIController.GetActiveModelName());
                ///////////for chisq //// 29Mar2012 ///
                //if ((chkElement != null) && (bool)((chkElement as CheckBox).IsChecked))
                //    OutputHelper.UpdateMacro("%CHISQ%", "chisq");
                /////////////for chisq //// 29Mar2012 ///

                BSkyCanvas canvas = element as BSkyCanvas;
                if (canvas != null && !string.IsNullOrEmpty(canvas.CommandString))
                {
                    UAMenuCommand command = (UAMenuCommand)parameter;
                    cmd.CommandSyntax = command.commandformat;//OutputHelper.GetCommand(canvas.CommandString, element);// can be used for "Paste" for syntax editor

                    retval = analytics.Execute(cmd); //ExecuteBSkyCommand(true);
                    ExecuteXMLDefinedDialog();
                }
                //16Apr2013 till this point command has already been executed. So now we store this command for "History"
                // moved here from common function ExecuteBSkyCommand b'coz for command batch dialog SaveHistory should execute once.
                //think and uncomment SaveInHistory();
            }

            OnPostExecute(parameter);
        }


        private void GenerateOutputTablesForNonXAML(object param)
        {
            if (param != null)
            {
                UAMenuCommand command = (UAMenuCommand)param;
                cmd = new CommandRequest();
                cmd.CommandSyntax = command.commandformat;
            }
            retval = analytics.Execute(cmd); //ExecuteBSkyCommand(true);
            ExecuteAnalysisCommands(); //fixed on 19Apr2014 for list of list formatting using BSkyFormat and BSkyFormat2
        }

        ////// For Analysis Command Execution from Syntax Editor ///// 
        public void ExecuteSyntaxEditor3(object param, bool selectedForDump)
        {
            parameter = param;
            OnPreExecute(parameter);
            if (!canExecute) return;
            object obj = null;
            string dialogcommandstr = null;
            try
            {
                //here TemplateFileName xaml will have same name as the analysis command function name
                // say- function called frm SynEdtr was 'bsky.my.func()' then in bin\Config\ 
                // dialog xaml, 'bsky.my.func.xaml' and
                // output template file 'bsky.my.func.xml' must exist
                // ie.. func name = xaml name = xml name
                XmlReader xmlr = XmlReader.Create(TemplateFileName);
                xmlr.ReadToFollowing("BSkyCanvas");
                xmlr.MoveToAttribute("CommandString");
                dialogcommandstr = xmlr.Value.Replace(" ", string.Empty).Replace('\"', '\'');
                xmlr.Close();
                obj = System.Windows.Markup.XamlReader.Load(XmlReader.Create(TemplateFileName));
            }
            catch (Exception ex)
            {
                MessageBox.Show(BSky.GlobalResources.Properties.Resources.CantCreateTemplate + TemplateFileName);
                logService.WriteToLogLevel("SynEdtr:Could not create template from " + TemplateFileName, LogLevelEnum.Error, ex);
                return;
            }
            element = obj as FrameworkElement;
            window = new BaseOptionWindow();
            window.Template = element;
            element.DataContext = this; // loading vars in left listbox(source)
            ///window.ShowDialog();
            commandwindow = element;
            ////////////test///////
            //// take two strings and then try to make merged dictionary. remove extra spaces. replace " with '
            //string bksytemplate="bsky.CrossTable(x=c({Rows}),y=c({columns}),layers=c({layers}),datasetname='{%DATASET%}',chisq={chisq})";
            //string bskycommand="bsky.CrossTable(x=c('store','contact'),y='regular',layers=c('gender'),datasetname='Dataset1',chisq=FALSE)";
            //string dialogcommandstr = "bsky.one.sm.t.test(vars=c({SelectedVars}),mu={testValue},conf.level=0.89,datasetname='{%DATASET%}',missing=0)";
            string bskycommand = ((UAMenuCommand)parameter).bskycommand.Replace(" ", string.Empty);//"bsky.one.sm.t.test(vars=c('tg0','tg2','tg3'),mu=30,conf.level=0.89,datasetname='Dataset1',missing=0)";

            Dictionary<string, string> dialogkeyvalpair = new Dictionary<string, string>();//like: key=mu, val= {testValue}
            Dictionary<string, string> bskycommandkeyvalpair = new Dictionary<string, string>();//like: key=mu, val= 30
            Dictionary<string, string> merged = new Dictionary<string, string>();//like: key=testValue, val = 30

            OutputHelper.getArgumentSetDictionary(dialogcommandstr, dialogkeyvalpair);
            OutputHelper.getArgumentSetDictionary(bskycommand, bskycommandkeyvalpair);
            OutputHelper.MergeTemplateCommandDictionary(dialogkeyvalpair, bskycommandkeyvalpair, merged);

            foreach (KeyValuePair<string, string> pair in merged)
            {
                if (!pair.Key.Contains("%"))
                {
                    OutputHelper.SetValueFromSynEdt(element, pair.Key, pair.Value);
                }
            }
            //foreach (Match m in mc)
            //{
            //    //Console.WriteLine(s.Index + " : " + s.ToString());// {SelectedVars} {testValue} {%DATASET%}
            //    if (!m.ToString().Contains("%"))
            //    {
            //        args = OutputHelper.getArgument(bskycommand, m.Index);
            //        uiElementName = m.ToString().Replace('{', ' ').Replace('}', ' ').Trim();
            //        OutputHelper.SetValueFromSynEdt(element, uiElementName, args);
            //    }
            //}
            //OutputHelper.SetValueFromSynEdt(element, "SelectedVars");
            //OutputHelper.SetValueFromSynEdt(element, "testValue");

            //For Chisq check box only
            //FrameworkElement chkElement = element.FindName("chisq") as FrameworkElement;
            if (true)//window.DialogResult.HasValue && window.DialogResult.Value)
            {
                //analytics can be sent from parent function(in SyntaxEditorWindow)
                //IAnalyticsService analytics = LifetimeService.Instance.Container.Resolve<IAnalyticsService>();
                //IConfigService confService = LifetimeService.Instance.Container.Resolve<IConfigService>();//23nov2012
                cmd = new CommandRequest();

                OutputHelper.Reset();
                OutputHelper.UpdateMacro("%DATASET%", UIController.GetActiveDocument().Name);
                OutputHelper.UpdateMacro("%MODEL%", UIController.GetActiveModelName());
                ///////////for chisq //// 29Mar2012 ///
                //if ((chkElement != null) && (bool)((chkElement as CheckBox).IsChecked))
                //    OutputHelper.UpdateMacro("%CHISQ%", "chisq");
                /////////////for chisq //// 29Mar2012 ///

                BSkyCanvas canvas = element as BSkyCanvas;
                if (canvas != null && !string.IsNullOrEmpty(canvas.CommandString))
                {
                    UAMenuCommand command = (UAMenuCommand)parameter;
                    cmd.CommandSyntax = command.commandformat;//OutputHelper.GetCommand(canvas.CommandString, element);// can be used for "Paste" for syntax editor
                    UAReturn retval = null; //retval = new UAReturn(); retval.Data = LoadAnalysisBinary();
#region Execute BSky command
                    try
                    {
                        retval = analytics.Execute(cmd); // RService called and DOM returned for Analysis commands
                        cmd.CommandSyntax = command.commandtype;////for header area ie NOTES
                        //SaveAnalysisBinary(retval.Data);
                        ///Added by Anil///07Mar2012
                        bool myrun = false;
                        if (cmd.CommandSyntax.Contains("BSkySetDataFrameSplit("))///executes when SPLIT is fired from menu
                        {
                            bool setsplit = false;
                            int startind = 0;
                            if (cmd.CommandSyntax.Contains("col.names"))
                            {
                                startind = cmd.CommandSyntax.IndexOf("c(", cmd.CommandSyntax.IndexOf("col.names"));// index of c(
                            }
                            else
                            {
                                startind = cmd.CommandSyntax.IndexOf("c(");// index of c(
                            }

                            int endind = cmd.CommandSyntax.IndexOf(")", startind);
                            int len = endind - startind + 1; // finding the length of  c(......)
                            string str = cmd.CommandSyntax.Substring(startind, len); // this will contain c('tg0','tg1') or just c()
                            string ch = null;
                            if (str.Contains("'")) ch = "'";
                            if (str.Contains('"')) ch = "\"";
                            if (ch != null && ch.Length > 0)
                            {
                                int i = str.IndexOf(ch);
                                int j = -1;
                                if (i >= 0) j = str.IndexOf(ch, i + 1);
                                if (j < 0) j = i + 1;
                                string sub = str.Substring(i + 1, (j - i - 1)).Trim();
                                if (i < 0)
                                    i = str.IndexOf("'");
                                if (i >= 0)
                                {
                                    if (sub.Length > 0)
                                        setsplit = true;
                                }
                            }

                            //////////  Setting/Unsetting Macro  for SPLIT //////////
                            if (setsplit)
                            {
                                OutputHelper.AddGlobalObject(string.Format("GLOBAL.{0}.SPLIT", UIController.GetActiveDocument().Name), element);
                                return;// no need to do any thing further
                            }
                            else // unset split
                            {
                                OutputHelper.DeleteGlobalObject(string.Format("GLOBAL.{0}.SPLIT", UIController.GetActiveDocument().Name));
                                return;// no need to do any thing further
                            }
                        }
                        ////////////////////////////
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(BSky.GlobalResources.Properties.Resources.CantExecuteCommand);
                        logService.WriteToLogLevel("Couldn't Execute the command", LogLevelEnum.Error, ex);
                        return;
                    }
#endregion
                    //UAReturn retval = new UAReturn();
                    retval.Success = true;
                    AnalyticsData data = new AnalyticsData();
                    data.SelectedForDump = selectedForDump;//10Jan2013
                    data.PreparedCommand = cmd.CommandSyntax;//storing command
                    data.Result = retval;
                    data.AnalysisType = cmd.CommandSyntax; //"T-Test"; For Parent Node name 02Aug2012
                    data.InputElement = element;
                    data.DataSource = ds;
                    data.OutputTemplate = ((UAMenuCommand)parameter).commandoutputformat;
                    UIController.AnalysisComplete(data);
                }
            }

            OnPostExecute(parameter);
        }

        public void ExecuteSynEdtrNonAnalysis(object commparam)
        {
            OnPreExecute(commparam);
            parameter = commparam;
            ////UIController = LifetimeService.Instance.Container.Resolve<IUIController>();
            ////ds = UIController.GetActiveDocument();
            ////if (ds == null)
            ////{
            ////    canExecute = false;
            ////    return;
            ////}
            ////Variables = new ObservableCollection<DataSourceVariable>(ds.Variables);
            cmd = new CommandRequest();
            cmd.CommandSyntax = ((UAMenuCommand)commparam).bskycommand;

            //ExecuteBSkyCommand(false); //False because command already got executed in Syntax Editor code. No need to execute again
            //if ((cmd.CommandSyntax.Contains("BSkySortDataframe(")))
            //{
            //    DatasetRefreshAndPrintTitle("Sort");
            //}
        }

#endregion

#region Check and Load Dialog Required R Packages //30Apr2015

        //Semi-colon separated R pacakge names (in Dialog property)
        private string LoadDialogRPacakges(string commaSeparatedPacakgeNames, out bool packageMissing)
        {
            StringBuilder message = new StringBuilder(string.Empty); ;
            UAReturn result = new UAReturn();
            char[] chars = new char[1]{';'};
            string[] dlgpkgarr = commaSeparatedPacakgeNames.Split(chars);
            string current;
            packageMissing = false;
            //Get List of Installed R Pacakges
            List<string> installedPackages = GetInstalledRPackages();

            //Get List of currently Loaded R Packages
            List<string> loadedPackages = GetLoadedRPackages();

            //Create a list of packages those should be loaded
            List<string> loadList = new List<string>();

            //First check if package is installed, if it is then load it
            // If not installed report error.
            for(int i=0; i<dlgpkgarr.Length;i++)
            {
                current = dlgpkgarr[i].Trim();
                if (current.Length < 1)//may be someone added too many semi-colons in between. So package name will be just "".
                    continue; // dont consider empty string package name and jump to next.
                if (installedPackages.Contains(current)) //if found load in memory in not already loaded
                {
                    if (!loadedPackages.Contains(current))//load R pacakge if not already loaded
                    {
                        loadList.Add(current);
                    }
                }
                else
                {
                    message.Append(current+"\n");//package not found on hard drive
                    packageMissing = true;
                }
            }

            //removing last comma
            //string messStr = message.ToString().Trim();
            //if(messStr.EndsWith(","))
            //{
            //    int idx = messStr.LastIndexOf(",");
            //    message.Clear();
            //    message.Append(messStr.Substring(0, idx));
            //}


            if (loadList.Count > 0)
            {
                result = LoadPackages(loadList);
                if (!result.Success)
                {
                    message.Append("\nPackage Loading Status:"+result.CommandString);//Got some issue while loading packages
                }
            }
            //if (message.Length < 1) message.Append("Success!");//just for testing
            return message.ToString();
        }

        private List<string> GetInstalledRPackages()
        {
            List<string> installed = new List<string>();
            ShowInstalledPackagesCommand instlcomm = new ShowInstalledPackagesCommand();
            installed = instlcomm.GetInstalledRPacakges();
            return installed;
        }

        private List<string> GetLoadedRPackages()
        {
            List<string> loaded = new List<string>();
            ShowLoadedPackagesCommand loddcomm = new ShowLoadedPackagesCommand();
            loaded = loddcomm.GetLoadedRPackages();
            return loaded;
        }

        private UAReturn LoadPackages(List<string> pkglist)
        {
            string[] strarr = new string[pkglist.Count];
            int idx = 0;
            foreach (string s in pkglist)
            {
                strarr[idx] = s;
                idx++;
            }
            PackageHelperMethods phm = new PackageHelperMethods();
            UAReturn res = phm.LoadPackageFromList(strarr);
            return res;
        }
       
#endregion

#region Get class of currently active model
        private string[] getCurrentlyActiveModelClass()
        {
            string modelname = UIController.GetActiveModelName();
            if(string.IsNullOrEmpty(modelname))
            {
                return null;
            }
            CommandRequest cmdreq = new CommandRequest();
            cmdreq.CommandSyntax = "class(" + modelname + ")";
            object ocnt = analytics.ExecuteR(cmdreq, true, false);

            string[] classes;
            if(ocnt.GetType().Name.Equals("String[]"))
            {
                classes = ocnt as string[];
            }
            else if (ocnt.GetType().Name.Equals("String"))
            {
                classes = new string[1];
                classes[0] = ocnt as string;

            }
            else
            {
                return null;
            }
            return classes;
        }
#endregion

#region Mouse Busy - Mouse Free
        //OutputWindowContainer owc = (LifetimeService.Instance.Container.Resolve<IOutputWindowContainer>()) as OutputWindowContainer;//new line
        Cursor defaultcursor;
        private void ShowMouseBusy_old()
        {
            defaultcursor = Mouse.OverrideCursor;
            Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
        }
        //Hides Progressbar
        private void HideMouseBusy_old()
        {
            Mouse.OverrideCursor = null;
        }
#endregion
    }
}
