using System.Windows.Controls;
using System.Windows;
using BSky.Interfaces.Controls;

namespace BSky.Controls.DesignerSupport
{
    public class DialogDesigner : PropertyEditorBase
    {
        private static int count = 0;
        public DialogDesigner()
        {
        }


        //04/13/2013
        //Aaron
        //This code is invoked when clicking the dialog designer lookup button to create a sub dialog
        protected override Control GetEditControl(string PropName, object CurrentValue,object CurrentObj)
        {
            int length = 0;
            //Aaron added 11/10/2013
            //Object wrapper is a wrapper object used to show categories in the grid
            //We need to extract the button object from the wrapper
            ObjectWrapper placeHolder=CurrentObj as ObjectWrapper;
            FrameworkElement selectedElement = placeHolder.SelectedObject as FrameworkElement;
            Window1 w = null;
            BSkyCanvas temp;
            if (selectedElement.Resources.Count > 0)
            {
                //Added 11/24/2013 by Aaron
                //I need to add this back to the chain of control so that this canvas is evaluated for duplicate detection
                //even though this canvas is represented in resources of the button, the canvas can be modified in resources
                //To ensure that its not searched 2 times (for duplicate detection or for setting of behaviours), I also remove it from the resources in line 3 below
               // BSkyCanvas.chainOpenCanvas.Add(selectedElement.Resources["dlg"] as BSkyCanvas);
              //selectedElement.Resources.Remove("dlg");
               // w = new Window1(selectedElement.Resources["dlg"] as BSkyCanvas, false);
                //Code below saves the original state of the canvas
                temp = selectedElement.Resources["dlg"] as BSkyCanvas;

                foreach (object obj in temp.Children)
                {
                    if (obj is IBSkyEnabledControl)
                    {
                        
                        IBSkyEnabledControl bskyCtrl = obj as IBSkyEnabledControl;
                        bskyCtrl.Enabled = bskyCtrl.IsEnabled;
                        bskyCtrl.IsEnabled = true;
                    }
                }

                //Adds it to the chainOpenCanvas for duplicate detection and property searches
                BSkyCanvas.chainOpenCanvas.Add(selectedElement.Resources["dlg"] as BSkyCanvas);
                //Removes it from button resources so canvas does not get searches 2 times
                selectedElement.Resources.Remove("dlg");
                w = new Window1(temp, false);
            }
            else
            {
                w = new Window1(false);
                //Added by Aaron 05/13/2014
                BSkyCanvas.chainOpenCanvas.Add(w.CanvasHost.Child as BSkyCanvas);
            }
            //I use this to prevent opening and saving on subdialogs
            w.CanOpen = false;
            w.CanSave = false;
            w.CanSaveSubDialog = true;
         
            return w;
        }


        // Aaron 04/13/2013
        //Code below runs when I save a sub dialog (click save and return)
        protected override object GetEditedValue(Control EditControl, string PropertyName, object oldValue,object currentObj)
        {
            int length = 0;
            string xamltemp = string.Empty;
            string path = @"c:\aaron\Test1.xaml";
            if (EditControl is Window1)
            {
                Window1 w = EditControl as Window1;
                //Aaron added 11/10/2013
                //Object wrapper is a wrapper object used to show categories in the grid
                //We need to extract the button object from the wrapper
                ObjectWrapper placeHolder = currentObj as ObjectWrapper;
                FrameworkElement selectedElement = placeHolder.SelectedObject as FrameworkElement;
                
                //Aaron 12/02 ON close as though the result is false, I want the canvas saved
                //if (w.DialogResult.HasValue && w.DialogResult.Value)
               
                // Added by Aaron 05/11/2015
                // Commented line below and added the line if (w.DialogResult.HasValue), this ensures that the sub dialog is always saved even if I double click and close the window
               // if (w.DialogResult.HasValue && w.DialogResult.Value)

              if (w.DialogResult.HasValue)
                {
                    //04/13/2013
                    //Added by Aaron
                    //Code is executed for an existing sub dialog
                    //All we are doing is removing the canvas that was JUST closed from the chain as we can serach this canvas
                    //by accessing the button resources
                    length = BSkyCanvas.chainOpenCanvas.Count;
                    BSkyCanvas.chainOpenCanvas.RemoveAt(length - 1); 
                    
                    //if (selectedElement.Resources.Count > 0)
                    //{
                    //    //removing the old dialog 
                    //    selectedElement.Resources.Remove("dlg");
                    //    // 12/09 Commented line below by Aaron and added line below
                    //   // selectedElement.Resources.Add("dlg", w.DialogElement);
                    //    selectedElement.Resources.Add("dlg", w.Canvas);
                    //    xamltemp = System.Windows.Markup.XamlWriter.Save(w.Canvas);
                    //    //fileName = @dialog.FileName;
                    //    //nameOfFile = fileName;
                    //    //Title = nameOfFile;
                    //    ////fileName = Path.GetFileNameWithoutExtension(fileName);



                    //    if (File.Exists(path))
                    //    {
                    //        // Note that no lock is put on the 
                    //        // file and the possibility exists 
                    //        // that another process could do 
                    //        // something with it between 
                    //        // the calls to Exists and Delete.
                    //        File.Delete(path);
                    //    }

                    //    FileStream stream = File.Create(path);
                    //    TextWriter writer = new StreamWriter(stream);
                    //    writer.Write(xamltemp);
                    //    writer.Close();
                    //}

                    // //04/13/2013
                    ////Added by Aaron
                    ////Code is executed for a new sub dialog
                    //else
                    //{
                    //    w.Canvas.Name = BSkyControlFactory.Instance.GetName("Canvas");
                    //    // 12/02 Added by Aaron, commented lline below
                    //    //  selectedElement.Resources.Add("dlg", w.DialogElement);
                    //    selectedElement.Resources.Add("dlg", w.Canvas);

                    //    // Aaron 06/15/2013

                    //    xamltemp = System.Windows.Markup.XamlWriter.Save(w.Canvas);
                    //    //fileName = @dialog.FileName;
                    //    //nameOfFile = fileName;
                    //    //Title = nameOfFile;
                    //    ////fileName = Path.GetFileNameWithoutExtension(fileName);
                       

                        
                    //    if (File.Exists(path))
                    //    {
                    //        // Note that no lock is put on the 
                    //        // file and the possibility exists 
                    //        // that another process could do 
                    //        // something with it between 
                    //        // the calls to Exists and Delete.
                    //        File.Delete(path);
                    //    }

                    //    FileStream stream = File.Create(path);
                    //    TextWriter writer = new StreamWriter(stream);
                    //    writer.Write(xamltemp);
                    //    writer.Close();
                    //    //result = true;
                    //}

                    //Added by Aaron 05/11/2015
                    //I commented the 2 lines below the reason is as long as there name property is not set, its possible to not have to worry about duplicate names
                 //   if (w.Canvas.Name =="")
                //    w.Canvas.Name = BSkyControlFactory.Instance.GetName("Canvas");
                    selectedElement.Resources.Add("dlg", w.Canvas);

                    // Aaron 06/15/2013

                    //xamltemp = System.Windows.Markup.XamlWriter.Save(w.Canvas);
                    //fileName = @dialog.FileName;
                    //nameOfFile = fileName;
                    //Title = nameOfFile;
                    ////fileName = Path.GetFileNameWithoutExtension(fileName);



                    //if (File.Exists(path))
                    //{
                        // Note that no lock is put on the 
                        // file and the possibility exists 
                        // that another process could do 
                        // something with it between 
                        // the calls to Exists and Delete.
                      //  File.Delete(path);
                    //}

                    //FileStream stream = File.Create(path);
                    //TextWriter writer = new StreamWriter(stream);
                    //writer.Write(xamltemp);
                    //writer.Close();
                    //result = true;

                    
                }
                //BSkyButton setButtonName =currentObj as BSkyButton;
               // setButtonName.Designer = oldValue as string;

                
                w.DetachCanvas();
                //Added by Aaron 05/05/2015
                
                //w.Template = null;
                //Added by Aaron 12/15
                //This is so that we echo the string in the designer control that was originally there
                return oldValue as string;
            }
            return false;
        }
    }
}
