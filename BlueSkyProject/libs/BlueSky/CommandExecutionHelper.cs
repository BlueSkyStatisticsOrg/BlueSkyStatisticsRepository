using System;
using System.Collections.Generic;
using System.Linq;
using BSky.Interfaces.Model;
using System.Windows;
using System.Xml;
using BSky.Statistics.Common;
using BlueSky.Services;
using BSky.XmlDecoder;
using BSky.Statistics.Service.Engine.Interfaces;
using System.Collections.ObjectModel;
using BSky.Interfaces.Commands;
using BSky.Controls;
using BSky.Lifetime;
using BSky.Lifetime.Interfaces;
using Microsoft.Practices.Unity;
using System.Text.RegularExpressions;
using BlueSky.Commands;
using BSky.Interfaces.Interfaces;
using BSky.Lifetime.Services;
using BSky.Interfaces.Services;
using System.Windows.Controls;
using BlueSky.Windows;
using BSky.ConfService.Intf.Interfaces;
using BSky.ConfigService.Services;
using BSky.Controls.Controls;

namespace BlueSky
{
    public class CommandExecutionHelper
    {

        ILoggerService logService = LifetimeService.Instance.Container.Resolve<ILoggerService>();//17Dec2012
        IAnalyticsService analytics = LifetimeService.Instance.Container.Resolve<IAnalyticsService>();
        IConfigService confService = LifetimeService.Instance.Container.Resolve<IConfigService>();//23nov2012
        SessionDialogContainer sdc = LifetimeService.Instance.Container.Resolve<SessionDialogContainer>();//13Feb2013
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
        bool AdvancedLogging;

        List<DataSourceVariable> dsvs;

        public UAReturn RetVal 
        {
            get { return retval; }
            set { retval = value; } 
        }

        public object MenuParameter 
        {
            get { return parameter; }

            set { parameter = value; }
        }
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
        BSkyDialogProperties bdt;//26Jun2014 

        public CommandExecutionHelper()
        {
            PreExecuteSub();
            AdvancedLogging = AdvancedLoggingService.AdvLog;//01May2015
        }


        #region ICommand Members

        public bool CanExecute(object parameter)
        {
            return canExecute;
        }

        public ObservableCollection<DataSourceVariable> Variables
        {
            get;
            set;
        }

        #endregion

        protected virtual void OnPreExecute(object param)
        {
            PreExecuteSub();
            UAMenuCommand command = (UAMenuCommand)param;
            ///Store command for "History" menu // 04Mar2013
            TemplateFileName = command.commandtemplate;// @".\Config\OneSampleCommand.xaml";

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
                Variables = new ObservableCollection<DataSourceVariable>(ds.Variables);
        }


        ////// For Analysis Command Execution from Syntax Editor /////28Mar2013 Using this one and not the other one below this method
        public void ExecuteSyntaxEditor(object param, bool SelectedForDump)
        {
            parameter = param;//set Global var.
            selectedForDump = SelectedForDump;//set Global var
            OnPreExecute(param);
            if (!canExecute) return;
            object obj = null;
            string dialogcommandstr = null;
            string HistMenuText = string.Empty;//29Mar2013
            //01Feb2017
            //Adding a code to refresh OutputHelpers copy of DataSource. Crosstab fails if newly generated col 
            //is used because it is not found in unrefreshed copy of the dataset.
            AnalyticsData ad = new AnalyticsData();
            ad.DataSource = ds;//new refreshed copy is assiged.
            OutputHelper.AnalyticsData = ad;
            ////This code will make sure new cols are available when setting values in background dialog.

            try
            {
                if (AdvancedLogging) logService.WriteToLogLevel("ExtraLogs: dialog xaml:" + TemplateFileName, LogLevelEnum.Info);

                if (System.IO.File.Exists(TemplateFileName))
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
                    if (AdvancedLogging) logService.WriteToLogLevel("ExtraLogs: dialog command string:" + dialogcommandstr, LogLevelEnum.Info);
                    xmlr.Close();
                    obj = System.Windows.Markup.XamlReader.Load(XmlReader.Create(TemplateFileName));
                }
                else
                {
                    GenerateOutputTablesForNonXAML(param);
                    return;
                }
            }
            catch (Exception ex)
            {
                //18Aug2014 Supressing this message box as we dont need it. But we still pass message in log.
                //MessageBox.Show("XAML missing or has improper format! Could not create template from " + TemplateFileName);
                logService.WriteToLogLevel("SynEdtr:Could not create template from " + TemplateFileName, LogLevelEnum.Error, ex);
                GenerateOutputTablesForNonXAML(param);
                return;
            }
            //obj = GetSessionDialog(); // same dialog cant be used as its child of the another parent in AUAnalysisCommandBase
            element = obj as FrameworkElement;
            window = new BaseOptionWindow();
            window.Template = element;
            element.DataContext = this; // loading vars in left listbox(source)
            ///window.ShowDialog();
            commandwindow = element;
            
            {
                if (AdvancedLogging) logService.WriteToLogLevel("ExtraLogs: Dialog controls value mapping.", LogLevelEnum.Info);
                ////////////test///////
                //// take two strings and then try to make merged dictionary. remove extra spaces. replace " with '
                //string bksytemplate="bsky.CrossTable(x=c({Rows}),y=c({columns}),layers=c({layers}),datasetname='{%DATASET%}',chisq={chisq})";
                //string bskycommand="bsky.CrossTable(x=c('store','contact'),y='regular',layers=c('gender'),datasetname='Dataset1',chisq=FALSE)";
                //string dialogcommandstr = "bsky.one.sm.t.test(vars=c({SelectedVars}),mu={testValue},conf.level=0.89,datasetname='{%DATASET%}',missing=0)";
                string bskycommand = ((UAMenuCommand)parameter).bskycommand.Replace(" ", string.Empty);//"bsky.one.sm.t.test(vars=c('tg0','tg2','tg3'),mu=30,conf.level=0.89,datasetname='Dataset1',missing=0)";
                if (AdvancedLogging) logService.WriteToLogLevel("ExtraLogs: Command:" + bskycommand, LogLevelEnum.Info);

                Dictionary<string, string> dialogkeyvalpair = new Dictionary<string, string>();//like: key=mu, val= {testValue}
                Dictionary<string, string> bskycommandkeyvalpair = new Dictionary<string, string>();//like: key=mu, val= 30
                Dictionary<string, string> merged = new Dictionary<string, string>();//like: key=testValue, val = 30

                OutputHelper.getArgumentSetDictionary(dialogcommandstr, dialogkeyvalpair);
                OutputHelper.getArgumentSetDictionary(bskycommand, bskycommandkeyvalpair);
                OutputHelper.MergeTemplateCommandDictionary(dialogkeyvalpair, bskycommandkeyvalpair, merged);

                foreach (KeyValuePair<string, string> pair in merged)
                {
                    if (AdvancedLogging)
                    {
                        string mapping = "Element:" + element.Name + ". Key:" + pair.Key + ". Value:" + pair.Value;
                        logService.WriteToLogLevel("ExtraLogs:\n" + mapping, LogLevelEnum.Info);
                    }
                    if (!pair.Key.Contains("%"))// This should only skip macros(words enclosed within %) and not other formats.
                    {
                        OutputHelper.SetValueFromSynEdt(element, pair.Key, pair.Value); //Filling dialog with values
                    }
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
                    if (AdvancedLogging) logService.WriteToLogLevel("ExtraLogs: Getting DOM", LogLevelEnum.Info);
                    retval = analytics.Execute(cmd); //ExecuteBSkyCommand(true);
                    ExecuteXMLDefinedDialog(cmd.CommandSyntax);//no need to pass parameters here. Just to match func signature
                }
                //16Apr2013 till this point command has already been executed. So now we store this command for "History"
                // moved here from common function ExecuteBSkyCommand b'coz for command batch dialog SaveHistory should execute once.
                //SaveInHistory(); // not sure if needed to be commented
            }

            //OnPostExecute(parameter);

            //Cleanup. Remove Canvas children and make it null. then make Window.templace null
            BSkyCanvas canv = element as BSkyCanvas;
            int count = canv.Children.Count;
            canv.Children.RemoveRange(0, count);
            window.Template = null;
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
                string s1 = BSky.GlobalResources.Properties.Resources.CantCreateTemplate;
                MessageBox.Show(s1+" " + TemplateFileName);
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

            //OnPostExecute(parameter);
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

        public void ExecuteXMLDefinedDialog(string command)
        {
            if (AdvancedLogging) logService.WriteToLogLevel("ExtraLogs: templated dialog execution.", LogLevelEnum.Info);
            //UAReturn retval = new UAReturn();
            retval.Success = true;
            AnalyticsData data = new AnalyticsData();
            data.SelectedForDump = selectedForDump;//10Jan2013
            data.PreparedCommand = command;// cmd.CommandSyntax;//storing command
            data.Result = retval;
            //18Nov2013 replared by following data.AnalysisType = cmd.CommandSyntax;
            //data.AnalysisType = cmd.CommandSyntax.Equals("bskyfrmtobj") ? ((UAMenuCommand)parameter).commandtype : cmd.CommandSyntax; //"T-Test"; For Parent Node name 02Aug2012
            data.AnalysisType = command.Equals("bskyfrmtobj") ? ((UAMenuCommand)parameter).commandtype : command; //"T-Test"; For Parent Node name 02Aug2012
            data.InputElement = element;
            data.DataSource = ds;
            data.OutputTemplate = ((UAMenuCommand)parameter).commandoutputformat;
            if (AdvancedLogging) logService.WriteToLogLevel("ExtraLogs: AnalysisComplete() function called:", LogLevelEnum.Info);
            UIController.AnalysisComplete(data);
        }

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

        public void ExecuteSplit(string command = null, FrameworkElement fe = null)
        {
            CommandRequest cr = new CommandRequest();
            cr.CommandSyntax = command;
            string stmt = command; //cmd.CommandSyntax;
            if (stmt.Contains("BSkySetDataFrameSplit("))///executes when SPLIT is fired from menu
            {
                bool setsplit = false;
                int startind = 0; int endind = 0;
                if (stmt.Contains("col.names"))
                {
                    startind = stmt.IndexOf("c(", stmt.IndexOf("col.names"));// index of c(
                }
                else
                {
                    startind = stmt.IndexOf("c(");// index of c(
                }
                if (startind > 0)
                    endind = stmt.IndexOf(")", startind);
                if (startind > 0 && endind > startind)
                {
                    int len = endind - startind + 1; // finding the length of  c(......)
                    string str = stmt.Substring(startind, len); // this will contain c('tg0','tg1') or just c()
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
                //Executing the command in R///
                analytics.ExecuteR(cr,false,false);

                ////Creating a command output////11Jul2014

                CommandOutput splitCo = new CommandOutput();
                splitCo.NameOfAnalysis = "Split:#" + setsplit;
                splitCo.IsFromSyntaxEditor = false;
                splitCo.Insert(0, new BSkyOutputOptionsToolbar());

                AUParagraph aup = new AUParagraph();
                aup.FontSize = BSkyStyler.BSkyConstants.TEXT_FONTSIZE;//App.HEADER_FONTSIZE;
                aup.Text = stmt;
                aup.ControlType = "Header";// treenodename.Length < treenodecharlen ? treenodename : treenodename.Substring(0, treenodecharlen);
                splitCo.Add(aup);

                //////////  Setting/Unsetting Macro  for SPLIT //////////
                if (setsplit)
                {
                    OutputHelper.AddGlobalObject(string.Format("GLOBAL.{0}.SPLIT", UIController.GetActiveDocument().Name), fe);
                    //window.Template = null; //11Mar2013 release the XAML object. ie obj is no more child of window.
                    AddToSyntaxSession(splitCo);//11Jul2014 SendToOutputWindow(dialogTitle, stmt);
                    Refresh_Statusbar();// RefreshGrids();
                    return;// no need to do any thing further
                }
                else // unset split
                {
                    OutputHelper.DeleteGlobalObject(string.Format("GLOBAL.{0}.SPLIT", UIController.GetActiveDocument().Name));
                    //if (window != null && window.Template != null)
                    //    window.Template = null; //11Mar2013 release the XAML object. ie obj is no more child of window.
                    AddToSyntaxSession(splitCo);//11Jul2014 SendToOutputWindow(dialogTitle, stmt);
                    Refresh_Statusbar();// RefreshGrids();
                    return;// no need to do any thing further
                }
            }

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
                cmd.CommandSyntax = "print('No command to execute')";
            }

            if (cmd.CommandSyntax.Contains("BSkyReloadDataset("))// if its relaod dataset commmand then prepare some parameter before executing the command
            {
                DataSource tempds = UIController.GetActiveDocument();
                string filename = tempds.FileName.Replace("\\", "/");
                string filetype = tempds.FileType;
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
            if (bdt != null)
                if (!bdt.IsBatchCommand && bdt.IsCommandOnly && !bdt.IsXMLDefined && !cmd.CommandSyntax.Contains("BSkySetDataFrameSplit("))
                {
                    SendToOutputWindow(dialogTitle, cmd.CommandSyntax);
                    ExecuteInSyntaxEditor(true, dialogTitle);//GenerateOutputTablesForNonXAML(null);// ExecuteXMLDefinedDialog();
                }

            if (cmd.CommandSyntax.Contains("BSkySortDataframe(") || cmd.CommandSyntax.Contains("BSkyComputeExpression(") ||
                         cmd.CommandSyntax.Contains("BSkyRecode("))
            {
                retval = analytics.Execute(cmd);
                OutputHelper.AnalyticsData.Result = retval;//putting latest DOM
                string[,] ew = OutputHelper.GetBSkyErrorsWarning(1, "normal");//08Nov2014

                if (cmd.CommandSyntax.Contains("BSkySortDataframe")) //11Apr2014 putting sort icon
                {
                    //single column logic
                    //int startidx = cmd.CommandSyntax.IndexOf("'");
                    //int endidx= cmd.CommandSyntax.IndexOf("'", startidx+1);
                    //int leng = endidx - startidx-1;
                    //string colname = cmd.CommandSyntax.Substring(startidx+1, leng);

                    //sort order 14Apr2014
                    string srtodr = string.Empty;
                    //THIS DOES NOT HOLD ANYMORE: descending=TRUE in command. There is just 1 boolean in sort so this 'if' will work
                    //
                    if (cmd.CommandSyntax.Contains("DES"))
                        srtodr = "desc";
                    else
                        srtodr = "asc";

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
                    else
                        RefreshGrids(null, collst);
                }
                else
                {
                    //testing something. If success, we dont need this. RefreshGrids();
                }

                //16Apr2014
                //must be excuted at the end after data is reloaded otherwise split is not refresh in statusbar. 
                if (cmd.CommandSyntax.Contains("BSkyReloadDataset("))
                {
                    Refresh_Statusbar();
                }

                ////Finally show messages in output
                //SendToOutputWindow(dialogTitle, cmd.CommandSyntax);
                //Show errors if any 08Nov2014
                if (ew != null && ew[0, 0] != null)
                {
                    //SendToOutputWindow("", ew[0, 2]);
                    CommandOutput ewmsg = new CommandOutput();
                    ewmsg.NameOfAnalysis = "Errors/Warnings";
                    ewmsg.IsFromSyntaxEditor = false;

                    AUParagraph ewtypeaup = new AUParagraph();
                    ewtypeaup.FontSize = BSkyStyler.BSkyConstants.TEXT_FONTSIZE;//App.TEXT_FONTSIZE;
                    ewtypeaup.Text = (ew[0, 0] != null)?ew[0, 0]:"no type info";
                    ewtypeaup.ControlType = "Type:";
                    ewmsg.Add(ewtypeaup);

                    AUParagraph usrmsgaup = new AUParagraph();
                    usrmsgaup.FontSize = BSkyStyler.BSkyConstants.TEXT_FONTSIZE;//App.TEXT_FONTSIZE;
                    usrmsgaup.Text = (ew[0, 1] != null) ? ew[0, 1] : "no user message info";
                    usrmsgaup.ControlType = "User Message:";
                    ewmsg.Add(usrmsgaup);

                    AUParagraph rmsgaup = new AUParagraph();
                    rmsgaup.FontSize = BSkyStyler.BSkyConstants.TEXT_FONTSIZE;//App.TEXT_FONTSIZE;
                    rmsgaup.Text = (ew[0, 2] != null) ? ew[0, 2] : "no R message info";
                    rmsgaup.ControlType = "R Message:";
                    ewmsg.Add(rmsgaup);

                    AddToSyntaxSession(ewmsg);
                }
            }
        }

        public void SetSortColForSortIcon(List<string> ascCols = null, List<string> descCols = null)
        {
            PreExecuteSub();

            //two different lists are maintaing each contains colname with its order of sorting
            UIController.sortasccolnames = ascCols;
            UIController.sortdesccolnames = descCols;


            //Above code did not work properly as it was holding the values even after closing datasets and when you reopen 
            //same dataset in same session you still see sort icons even though col are not sorted as they just got loaded
            //from file.
            //TabItem panel = UIController.GetTabItem(ds);
            //DataPanel datapanel = panel.Content as DataPanel;
            //datapanel.sortasccolnames = ascCols;
            //datapanel.sortdesccolnames = descCols;

            //if (ds != null)//19Mar2013
            //{

            //    IUnityContainer container = LifetimeService.Instance.Container;
            //    IDataService service = container.Resolve<IDataService>();
            //    ds = service.Refresh(ds);
            //    if (ds != null)
            //    {
            //        //two different lists are maintaing each contains colname with its order of sorting
            //        UIController.sortasccolnames = ascCols;
            //        UIController.sortdesccolnames = descCols;

            //        UIController.RefreshGrids(ds);
            //    }
            //}
        }

        protected void ExecuteInSyntaxEditor(bool ExecuteCommand, string sessionTitle = "", CommandOutput sliceco = null, bool islastslice = true)
        {
            //Launch Syntax Editor window with command pasted /// 29Jan2013
            MainWindow mwindow = LifetimeService.Instance.Container.Resolve<MainWindow>();
            ////// Start Syntax Editor  //////
            SyntaxEditorWindow sewindow = LifetimeService.Instance.Container.Resolve<SyntaxEditorWindow>();
            sewindow.Owner = mwindow;
            //21Nov2013. if there is slicename add it first to the syntax editor output session list
            if (sliceco != null)
                sewindow.AddToSession(sliceco);
            if (cmd.CommandSyntax != null && cmd.CommandSyntax.Length > 0)
                sewindow.RunCommands(cmd.CommandSyntax);//, sessionheader);

            //22Nov2013
            //if sessionTitle is empty that means there are more (split)slices to execute
            //when the last slice is ready for execution that time sessionTitle 
            //will have the main title for whole session
            if (islastslice)//sessionTitle != null && sessionTitle.Length > 0)
                sewindow.DisplayAllSessionOutput(sessionTitle);
            else
                return;//go get another slice. Do not process rest of the code till last slice comes in.
        }


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

        private void PasteSyntax()
        {
            //copy to clipboard and return for this function
            Clipboard.SetText(cmd.CommandSyntax);

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
            sewindow.PasteSyntax(syncomment + cmd.CommandSyntax);//paste command  :-> 
            sewindow.Show();
            sewindow.WindowState = WindowState.Normal;
            sewindow.Activate();
        }

        //Save executed command in History
        private void SaveInHistory()
        {
            ///Store executed command in "History" menu /// 04March2013
            UAMenuCommand command = (UAMenuCommand)parameter;
            Window1 appWindow = LifetimeService.Instance.Container.Resolve<Window1>();
            if (ds != null)
                appWindow.History.AddCommand(ds.Name, command);
        }

        private object GetSessionDialog()
        {
            SessionDialogContainer sdc = LifetimeService.Instance.Container.Resolve<SessionDialogContainer>();//13Feb2013
            string dialogkey = TemplateFileName + UIController.GetActiveDocument().FileName + UIController.GetActiveDocument().Name;
            object obj=null;
            if (sdc.SessionDialogList.ContainsKey(dialogkey))
            {
                obj = sdc.SessionDialogList[dialogkey];//gets the exisiting dialog
                //isOldDialog = true;
            }
            return obj;
        }
        //Send executed command to output window. So, user will know what he executed
        private void SendToOutputWindow(string title, string message)//26Mar2013
        {
            #region Get Active output Window
            //////// Active output window ///////
            OutputWindowContainer owc = (LifetimeService.Instance.Container.Resolve<IOutputWindowContainer>()) as OutputWindowContainer;
            OutputWindow ow = owc.ActiveOutputWindow as OutputWindow; //get currently active window
            #endregion
            ow.AddMessage(title, message);
        }

        private void AddToSyntaxSession(CommandOutput co)
        {
            SyntaxEditorWindow sewindow = LifetimeService.Instance.Container.Resolve<SyntaxEditorWindow>();
            sewindow.AddToSession(co);
        }
        #endregion


        #region Refreshing Grids etc..

        //16Jul2015 only refreshes datagrid//Refreshes Both Data and Variable Grids and echoes the command in output window.
        public void DatasetRefreshAndPrintTitle(string title) //This can be called to refresh Grids from Syn Editor
        {
            RefreshGrids();
            if (window != null)
                window.Template = null; //19Mar2013 release the XAML object. ie obj is no more child of window.

            if (cmd != null && title != null)//16May2013
                SendToOutputWindow(title, cmd.CommandSyntax);
            return;
        }

        //16Jul2015Refreshes Both Data and Variable Grids and echoes the command in output window.
        public void BothGridRefreshAndPrintTitle(string title) //This can be called to refresh Grids from Syn Editor
        {
            RefreshBothGrids();
            if (window != null)
                window.Template = null; //19Mar2013 release the XAML object. ie obj is no more child of window.

            if (cmd != null && title != null)//16May2013
                SendToOutputWindow(title, cmd.CommandSyntax);
            return;
        }

        //16Jul2015 only refreshes datagrid
        public void RefreshGridsOLD(List<string> sortcolnames = null, string sortorder = null)//16May2013 //This can/may be called to refresh Grids from App Main window
        {
            PreExecuteSub();
            if (ds != null)//19Mar2013
            {

                IUnityContainer container = LifetimeService.Instance.Container;
                IDataService service = container.Resolve<IDataService>();
                ds = service.Refresh(ds);
                if (ds != null)
                {
                    //UIController.sortcolnames = sortcolnames;//11Apr2014
                    //UIController.sortorder = sortorder; //14Apr2014

                    UIController.RefreshGrids(ds);
                }
            }
        }


        //16Jul2015 only refreshes datagrid
        public void RefreshGrids(List<string> sortasccolnames = null, List<string> sortdesccolnames = null)//16May2013 //This can/may be called to refresh Grids from App Main window
        {
            PreExecuteSub();
            if (ds != null)//19Mar2013
            {

                IUnityContainer container = LifetimeService.Instance.Container;
                IDataService service = container.Resolve<IDataService>();
                ds = service.Refresh(ds);
                if (ds != null)
                {
                    //two different lists are maintaing each contains colname with its order of sorting
                    UIController.sortasccolnames = sortasccolnames;
                    UIController.sortdesccolnames = sortdesccolnames;

                    UIController.RefreshGrids(ds);
                }
            }
        }
        //16Jul2015 refresh both grids when 'refresh' icon in outputwindow is clicked
        public void RefreshBothGrids(List<string> sortcolnames = null, string sortorder = null)//16May2013 //This can/may be called to refresh Grids from App Main window
        {
            PreExecuteSub();
            if (ds != null)//19Mar2013
            {

                IUnityContainer container = LifetimeService.Instance.Container;
                IDataService service = container.Resolve<IDataService>();

                //03Jul2016 taking a backup
                string DSname = ds.Name;
                //string Ftype = ds.FileType;
                string DSFname = ds.FileName;
 DataSource ds_backup = ds;//25Oct2016 just making a copy of refrence
                ds = service.Refresh(ds);
                if (ds != null)
                {
                    if(sortorder=="asc")
                        UIController.sortasccolnames = sortcolnames;//11Apr2014
                    if(sortorder=="desc")
                        UIController.sortdesccolnames = sortcolnames;//11Apr2014
                    //UIController.sortorder = sortorder; //14Apr2014

                    //ds.StartColindex = 0; ds.EndColindex=15;
                    UIController.RefreshBothGrids(ds);
                }
                else //03Jul2016 In case Dataset becomes NULL we need to empty the datagrid.
                {
                    ds = new DataSource();
                    ds.Variables = new List<DataSourceVariable>();
                    ds.FileName = DSFname;
                    ds.Name = DSname;
                    ds.SheetName = "";
                    //ds.FileType = Ftype;

                    ds.DecimalCharacter = ds_backup.DecimalCharacter;
                    ds.FieldSeparator = ds_backup.FieldSeparator;
                    ds.HasHeader = ds_backup.HasHeader;
                    ds.IsBasketData = ds_backup.IsBasketData;

                    UIController.RefreshBothGrids(ds);

                    //12Sep2016 Generating the message for null dataset
                    CommandRequest msg = new CommandRequest();
                    msg.CommandSyntax = DSname + " "+ BSky.GlobalResources.Properties.Resources.DatasetBecameNull;
                    string title = BSky.GlobalResources.Properties.Resources.DatasetNull;
                    if (msg != null && title != null)
                        SendToOutputWindow(title, msg.CommandSyntax);
                }
            }
        }

        //05Dec2013 refresh statusbar in main grid for split info
        public void Refresh_Statusbar()
        {
            UIController.RefreshStatusbar();
        }

        #endregion


    }
}
