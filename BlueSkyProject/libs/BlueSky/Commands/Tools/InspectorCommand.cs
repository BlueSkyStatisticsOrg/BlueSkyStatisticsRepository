using System;
using System.Windows;
using BSky.Lifetime;
using BSky.Lifetime.Interfaces;
using BSky.Controls;
using BlueSky.CommandBase;

using System.Windows.Documents;
using System.Windows.Markup;
//using System.Windows.Forms;
using System.IO;
using BSky.Statistics.Common;
using System.Collections.Generic;

namespace BlueSky.Commands.Tools
{
    class InspectorCommand : BSkyCommandBase
    {
        ILoggerService logService = LifetimeService.Instance.Container.Resolve<ILoggerService>();//17Dec2012
        protected override void OnPreExecute(object param)
        {
            
        }

        protected override void OnExecute(object param)
        {
            InspectBinding_Executed();
        }

        protected override void OnPostExecute(object param)
        {
            
        }



        void InspectBinding_Executed()
        {
            CommonFunctions cf = new CommonFunctions();
           InspectDialog window = new InspectDialog();
            System.Windows.Forms.DialogResult result;

            //Added by Aaron 05/11/2014
            //Clearing up chainopen canvas as we are starting from a Blank Slate when ever we are inspecting a dialog
            //We alsodo it when we finish inspecting the dialog
            BSkyCanvas.chainOpenCanvas.Clear();
            System.Windows.Forms.OpenFileDialog dialog = new System.Windows.Forms.OpenFileDialog();
            dialog.Filter = "Xaml Document (*.bsky)|*.bsky";
            result = dialog.ShowDialog();
            object obj;
            string nameOfFile;

            if (result == System.Windows.Forms.DialogResult.OK)
            {
                string xamlfile = string.Empty, outputfile = string.Empty;
                cf.SaveToLocation(@dialog.FileName, Path.GetTempPath(), out xamlfile, out outputfile);
                if (!string.IsNullOrEmpty(xamlfile))
                {
                    FileStream stream = System.IO.File.Open(Path.Combine(Path.GetTempPath(), Path.GetFileName(xamlfile)), FileMode.Open);

                    //FileStream stream = File.Open(Path.Combine(Path.GetTempPath(), xamlfile,FileMode.Open);
                    try
                    {
                        //Added by Aaron 11/24/2013
                        //The line below is important as when the dialog editor is launched for some reason and I DONT know why
                        //BSKYCanvas is invoked from the controlfactor and hence dialogMode is true, we need to reset
                        BSkyCanvas.dialogMode = false;
                        obj = XamlReader.Load(stream);
                        //Added by Aaron 04/13/2014
                        //There are 3 conditions here, when I launch dialog editor
                        //1. The first thing I do is open an existing dialog. Chainopencanvas.count=0
                        //2. I have an existing dialog open and I click open again.  Chainopencanvas.count=2. The first dialog is empty as I clean the existing canvas and open a new one
                        //3. I hit new the first time I open dialogeditor and then I hit open. Again,Chainopencanvas.count=2. The first dialog is empty as I clean the existing canvas and open a new one 

                        //The chainopencanvas at this point has 2 entries, one with an empty canvas as we have removed all the children
                        //the other with the dialog we just opened
                        //we remove the empty one
                        //if (BSkyCanvas.chainOpenCanvas.Count > 1)
                        //    BSkyCanvas.chainOpenCanvas.RemoveAt(0);
                        // 11/18/2012, code inserted by Aaron to display the predetermined variable list
                        BSkyCanvas canvasobj;
                        canvasobj = obj as BSkyCanvas;
                        BSkyCanvas.chainOpenCanvas.Add(canvasobj);
                        BSkyCanvas.previewinspectMode = true;

                        //Added by Aaron 12/08/2013
                        //if (canvasobj.Command == true) c1ToolbarStrip1.IsEnabled = false;
                        //else c1ToolbarStrip1.IsEnabled = true;

                        //Added by Aaron 11/21/2013
                        //Added line below to enable checking of properties in behaviour.cs i.e. propertyname and control name in dialog 
                        //editor mode to make sure that the property names are generated correctly
                        //  BSkyCanvas.dialogMode = true;


                        //Added 04/07/2013
                        //The properties like outputdefinition are not set for firstCanvas 
                        //The line below sets it
                       // firstCanvas = canvasobj;
                        foreach (UIElement child in canvasobj.Children)
                        {
                            // Code below has to be written as we have saved BSKYvariable list with rendervars=False.
                            //BSKyvariablelist has already been created with the default constructore and we need to 
                            //do some work to point the itemsource properties to the dummy variables we create
                            if (child.GetType().Name == "BSkyVariableList")
                            {
                                List<DataSourceVariable> preview = new List<DataSourceVariable>();
                                DataSourceVariable var1 = new DataSourceVariable();
                                var1.Name = "var1";
                                var1.DataType = DataColumnTypeEnum.Numeric;
                                var1.Width = 4;
                                var1.Decimals = 0;
                                var1.Label = "var1";
                                var1.Alignment = DataColumnAlignmentEnum.Left;
                                var1.Measure = DataColumnMeasureEnum.Scale;
                                var1.ImgURL = "../Resources/scale.png";
                                //  var1.ImgURL = "C:/Users/Aiden/Downloads/Client/libs/BSky.Controls/Resources/scale.png";

                                DataSourceVariable var2 = new DataSourceVariable();
                                var2.Name = "var2";
                                var2.DataType = DataColumnTypeEnum.Character;
                                var2.Width = 4;
                                var2.Decimals = 0;
                                var2.Label = "var2";
                                var2.Alignment = DataColumnAlignmentEnum.Left;
                                var2.Measure = DataColumnMeasureEnum.Nominal; ;
                                var2.ImgURL = "../Resources/nominal.png";

                                DataSourceVariable var3 = new DataSourceVariable();
                                var3.Name = "var3";
                                var3.DataType = DataColumnTypeEnum.Character;
                                var3.Width = 4;
                                var3.Decimals = 0;
                                var3.Label = "var3";
                                var3.Alignment = DataColumnAlignmentEnum.Left;
                                var3.Measure = DataColumnMeasureEnum.Ordinal;
                                var3.ImgURL = "../Resources/ordinal.png";


                                preview.Add(var1);
                                preview.Add(var2);
                                preview.Add(var3);
                                BSkyVariableList temp;
                                temp = child as BSkyVariableList;
                                // 12/25/2012 
                                //renderVars =TRUE meanswe will render var1, var2 and var3 as listed above. This means that we are in the Dialog designer
                                temp.renderVars = false;
                                temp.ItemsSource = preview;


                            }
                            //Added by Aaron 04/06/2014
                            //Code below ensures that when the dialog is opened in dialog editor mode, the IsEnabled from
                            //the base class is set to true. This ensures that the control can never be disabled in dialog editor mode
                            //Once the dialog is saved, the Enabled property is saved to the base ISEnabled property to make sure that
                            //the proper state of the dialog is saved
                            //if (child is IBSkyEnabledControl)
                            //{
                            //    IBSkyEnabledControl bskyCtrl = child as IBSkyEnabledControl;
                            //    bskyCtrl.Enabled = bskyCtrl.IsEnabled;
                            //    bskyCtrl.IsEnabled = true;
                            //}
                        }
                       // subCanvasCount = GetCanvasObjectCount(obj as BSkyCanvas);
                       // BSkyControlFactory.SetCanvasCount(subCanvasCount);
                        // OpenCanvas(obj);
                        //Aaron 01/12/2013
                        //This stores the file name so that we don't have to display the file save dialog on file save and we can save to the file directly;
                        nameOfFile = @dialog.FileName;

                        //01/26/2013 Aaron
                        //Setting the title of the window
                        //Title = nameOfFile;
                        //Aaron: Commented this on 12/15
                        // myCanvas.OutputDefinition = Path.Combine(Path.GetTempPath(), outputfile);
                        //Aaron 01/26/2013
                        //Making sure saved is set to true as soon as you open the dialog.
                        // saved = true;
                        //Aaron
                        //02/10/2013
                        //Added code below to ensure that the RoutedUICommand gets fired correctly and the menu items under file and Dialog (top level menu items)
                        //are properly opened and closed
                        //   c1ToolbarStrip1.IsEnabled = true;
                        // CanClose = true;
                        // CanSave = true;
                        //this.Focus();
                        // window.Template = obj as FrameworkElement;
                        BSkyCanvas cs =obj as BSkyCanvas;
                        System.Windows.Forms.PropertyGrid CanvasPropertyGrid;
                        // window.CanvasPropHost = obj as BSkyCanvas;
                        CanvasPropertyGrid = window.CanvasPropHost.Child as System.Windows.Forms.PropertyGrid;
                       // window.CanvasHost.Child = obj as BSkyCanvas;

                        window.Template = cs;
                        CanvasPropertyGrid.SelectedObject = obj as BSkyCanvas;
                        window.setSizeOfPropertyGrid();
                        
                        window.Width = cs.Width + 20;
                     //   window.CanvasHost.Width = cs.Width;
                      //  window.CanvasHost.Height = cs.Height;
                       // window.Height = cs.Height + 200+80;
                        //CanvasPropertyGrid.Refresh();
                        // Aaron
                        //03/05/2012
                        //Making the preview window a fixed size window
                      //  if (obj.GetType().Name == "BSkyCanvas") window.ResizeMode = ResizeMode.NoResize;
                        stream.Close();
                        window.ShowDialog();
                        BSkyCanvas.previewinspectMode = false;
                        //Added by Aaron 05/11/2014
                        //Clearing up chainopen canvas as we are starting from a Blank Slate when ever we are inspecting a dialog
                        //We alsodo it when we finish inspecting the dialog
                        BSkyCanvas.chainOpenCanvas.Clear();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                }
                else
                {
                    MessageBox.Show(BSky.GlobalResources.Properties.Resources.CantOpenFile);
                }


                // *******************************************************************************************************
                //gencopy();
                //Aaron 11/25/2012, the line below needs to be uncommented once the serialization issue is addressed with Anil
                // string xaml = GetXamlforpreview();
                //string xaml = GetXaml();


                //*************************************************************************************************
            }
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
