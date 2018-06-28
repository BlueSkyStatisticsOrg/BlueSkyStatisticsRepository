using System;
using Microsoft.Practices.Unity;
using Microsoft.Win32;
using System.Windows;
using ICSharpCode.SharpZipLib.Zip;
using System.IO;
using System.Windows.Markup;
using BSky.Interfaces;
using BSky.Lifetime;
using BSky.Lifetime.Interfaces;
using BlueSky.CommandBase;
using BSky.Interfaces.Interfaces;
using System.Collections.Generic;

namespace BlueSky.Commands.Tools
{
    class ToolsInstallDialog : BSkyCommandBase
    {
        ILoggerService logService = LifetimeService.Instance.Container.Resolve<ILoggerService>();//17Dec2012
        protected override void OnPreExecute(object param)
        {
        }

        public const String FileNameFilter = "BSky Commands (*.bsky)|*.bsky";

        List<string> extractedfiles = null;
        protected override void OnExecute(object param)
        {
            System.Windows.Forms.DialogResult dresult = System.Windows.Forms.MessageBox.Show(
                               BSky.GlobalResources.Properties.Resources.RestartToApplyChanges +
                               BSky.GlobalResources.Properties.Resources.ConfirmDialogInstall,
                               BSky.GlobalResources.Properties.Resources.InstallNewDialog,
                               System.Windows.Forms.MessageBoxButtons.YesNoCancel,
                               System.Windows.Forms.MessageBoxIcon.Question);
            if (dresult == System.Windows.Forms.DialogResult.Yes)//save
            {

                IUnityContainer container = LifetimeService.Instance.Container;
                IDataService service = container.Resolve<IDataService>();
                IUIController controller = container.Resolve<IUIController>();
                IDashBoardService dashboardService = container.Resolve<IDashBoardService>();
                Window1 mainWin = container.Resolve<Window1>();//15Jan2013

                extractedfiles = new List<string>();
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = FileNameFilter;
                bool? output = openFileDialog.ShowDialog(Application.Current.MainWindow);
                if (output.HasValue && output.Value)
                {
                    FileStream fileStreamIn = new FileStream(openFileDialog.FileName, FileMode.Open, FileAccess.Read);
                    ZipInputStream zipInStream = new ZipInputStream(fileStreamIn);
                    string xamlFile = string.Empty, outputFile = string.Empty;
                    ZipEntry entry = zipInStream.GetNextEntry();
                    string tempDir = Path.GetTempPath();

                    //Extract the files
                    while (entry != null)
                    {
                        //if(System.IO.File.Exists((@".\config\" + entry.Name)))
                        //{
                        //    if(MessageBox.Show("Do you want to replace existing file?","Warning",MessageBoxButton.YesNo,MessageBoxImage.Hand) == MessageBoxResult.No)
                        //        continue;
                        //}
                        FileStream fileStreamOut = new FileStream(Path.Combine(tempDir, entry.Name), FileMode.Create, FileAccess.Write);
                        if (entry.Name.EndsWith(".xaml")) xamlFile = entry.Name;
                        if (entry.Name.EndsWith(".xml")) outputFile = entry.Name;
                        int size;
                        byte[] buffer = new byte[1024];
                        do
                        {
                            size = zipInStream.Read(buffer, 0, buffer.Length);
                            fileStreamOut.Write(buffer, 0, size);
                        } while (size > 0);
                        fileStreamOut.Close();
                        entry = zipInStream.GetNextEntry();
                        MessageBox.Show("Extracted:" + entry.Name+" in " +tempDir);
                        //adding fullpath filename to list //08May2015
                        extractedfiles.Add(Path.Combine(tempDir, entry.Name));
                    }
                    zipInStream.Close();
                    fileStreamIn.Close();

                    if (string.IsNullOrEmpty(outputFile))
                    {
                        MessageBox.Show(BSky.GlobalResources.Properties.Resources.CantInstallDialogNoOutFile);
                        return;
                    }
                    //Load the dialog object, check the location and modify menu file.
                    if (!string.IsNullOrEmpty(xamlFile) && !string.IsNullOrEmpty(outputFile))
                    {
                        FileStream stream = System.IO.File.Open(Path.Combine(tempDir, xamlFile), FileMode.Open);
                        try
                        {
                            object obj = XamlReader.Load(stream);
                            BSky.Controls.BSkyCanvas canvas = obj as BSky.Controls.BSkyCanvas;

                            string menulocation = canvas.MenuLocation;
                            string title = canvas.Title;
                            if (string.IsNullOrEmpty(canvas.Title))
                            {
                                title = Microsoft.VisualBasic.Interaction.InputBox("Get Item Name", "Item Name", "New Node");
                                if (!string.IsNullOrEmpty(title))
                                {
                                    MessageBox.Show(BSky.GlobalResources.Properties.Resources.CantInstallDialogNoTitle, BSky.GlobalResources.Properties.Resources.DialogTitleEmpty);
                                    return;
                                }
                            }

                            //23Apr2015 bool? result = dashboardService.SetElementLocaton(menulocation, title, Path.Combine(@".\config\", xamlFile), false, "");
                            bool? result = dashboardService.SetElementLocaton(menulocation, title, Path.Combine(string.Format(@"{0}", BSkyAppData.RoamingUserBSkyConfigL18nPath), xamlFile), false, "");//23Apr2015 
                            MessageBoxResult msgResult = MessageBoxResult.No;
                            if (result.HasValue && !result.Value)
                            {
                                msgResult = MessageBox.Show(BSky.GlobalResources.Properties.Resources.ConfirmDialogOverwrite, BSky.GlobalResources.Properties.Resources.WarnOverwrite, MessageBoxButton.YesNo);
                            }
                            if (result.HasValue && (result.Value || msgResult == MessageBoxResult.Yes))
                            {
                                //23Apr2015 canvas.OutputDefinition = Path.GetFullPath(@".\Config\" + outputFile);
                                canvas.OutputDefinition = Path.GetFullPath(string.Format(@"{0}", BSkyAppData.RoamingUserBSkyConfigL18nPath) + outputFile);//23Apr2015 

                                System.IO.File.Copy(Path.Combine(tempDir, outputFile), canvas.OutputDefinition, true);

                                string xaml = XamlWriter.Save(canvas);
                                //23Apr2015 FileStream outputstream = System.IO.File.Create(@".\Config\" + xamlFile);
                                FileStream outputstream = System.IO.File.Create(string.Format(@"{0}", BSkyAppData.RoamingUserBSkyConfigL18nPath) + xamlFile);//23Apr2015 
                                TextWriter writer = new StreamWriter(outputstream);
                                writer.Write(xaml);
                                writer.Close();

                                MessageBox.Show(BSky.GlobalResources.Properties.Resources.RestartToApplyChanges2, BSky.GlobalResources.Properties.Resources.DialogInstalled);
                            }
                            else
                            {
                                string sInput = "New Command Node";
                                string aboveBelowSibling = "Below";//position of new command based on sibling location. Enum can be used for Above/Below
                                string location = dashboardService.SelectLocation(ref sInput, ref aboveBelowSibling);
                                //sInput is prefixed to XAML and XML filenames. You can think of dropping it for prefixing
                                //string sInput = Microsoft.VisualBasic.Interaction.InputBox("Get Item Name", "Item Name", "Command Name");
                                if (!string.IsNullOrEmpty(location))
                                {
                                    //string sInputNoBlanks = sInput.Replace(' ','_');//26sep2012
                                    //23Apr2015 dashboardService.SetElementLocaton(location, sInput, Path.Combine(@".\Config\", sInput + xamlFile), true, aboveBelowSibling);
                                    dashboardService.SetElementLocaton(location, sInput, Path.Combine(string.Format(@"{0}", BSkyAppData.RoamingUserBSkyConfigL18nPath), sInput + xamlFile), true, aboveBelowSibling);//23Apr2015 

                                    ///12Oct2012 canvas.OutputDefinition = Path.GetFullPath(@".\config\" + sInput + outputFile);///output template filename will be same as in zip
                                    //23Apr2015 canvas.OutputDefinition = Path.GetFullPath(@".\Config\" + sInput + xamlFile.Replace("xaml", "xml"));///output template filename will be same as dialog filename. No matter whats in zip file.
                                    canvas.OutputDefinition = Path.GetFullPath(string.Format(@"{0}", BSkyAppData.RoamingUserBSkyConfigL18nPath) + sInput + xamlFile.Replace("xaml", "xml"));//23Apr2015 //output template filename will be same as dialog filename. No matter whats in zip file.
                                                                                                                                        
                                    System.IO.File.Copy(Path.Combine(tempDir, outputFile), canvas.OutputDefinition, true);

                                    ///Following commented lines should not execute 17Jan2013
                                    ///These are writing a xaml and putting datatemplate tag in XAML while installing.
                                    //string xaml = XamlWriter.Save(canvas);
                                    //FileStream outputstream = System.IO.File.Create(@".\Config\" + sInput + xamlFile);
                                    //TextWriter writer = new StreamWriter(outputstream);
                                    //writer.Write(xaml);
                                    //writer.Close();
                                    //23Apr2015 string installpath = Path.GetFullPath(@".\Config\" + sInput + xamlFile);
                                    string installpath = Path.GetFullPath(string.Format(@"{0}", BSkyAppData.RoamingUserBSkyConfigL18nPath) + sInput + xamlFile);//23Apr2015 

                                    System.IO.File.Copy(Path.Combine(tempDir, xamlFile), installpath, true);

                                    MessageBox.Show(BSky.GlobalResources.Properties.Resources.RestartToApplyChanges2, BSky.GlobalResources.Properties.Resources.DialogInstalled);
                                    mainWin.Window_Refresh_Menus();//15Jan2013
                                    
                                }
                            }
                            CleanTempFiles();
                        }
                        catch (Exception e)
                        {
                            MessageBox.Show(BSky.GlobalResources.Properties.Resources.CantInstallDialog, BSky.GlobalResources.Properties.Resources.InstallFailed);
                            logService.WriteToLogLevel("Couldn't install Dialog", LogLevelEnum.Error);
                        }

                    }
                }
            }
        }

        private void CleanTempFiles()
        {
            MessageBox.Show("Cleanup Started...");
            if (extractedfiles != null && extractedfiles.Count > 0)
            {
                foreach (string fulpathfilename in extractedfiles)
                {
                    MessageBox.Show("Cleaning : "+fulpathfilename);
                    if (System.IO.File.Exists(fulpathfilename))
                    {
                        System.IO.File.Delete(fulpathfilename);
                        MessageBox.Show("Cleaned : " + fulpathfilename);
                    }
                }
            }
        }
        protected override void OnPostExecute(object param)
        {
        }

        ////Send executed command to output window. So, user will know what he executed
        //protected override void SendToOutputWindow(string command, string title)//13Dec2013
        //{
        //    #region Get Active output Window
        //    //////// Active output window ///////
        //    OutputWindowContainer owc = (LifetimeService.Instance.Container.Resolve<IOutputWindowContainer>()) as OutputWindowContainer;
        //    OutputWindow ow = owc.ActiveOutputWindow as OutputWindow; //get currently active window
        //    #endregion
        //    ow.AddMessage(command, title);
        //}
    }
}
