using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.Windows.Forms;


//namespace BSky.Controls.DesignerSupport
//{
   
//    class OutputSelector : PropertyEditorBase
//    {
//        String OutputDefinition = string.Empty;
//        public OutputSelector()
//        {
//        }  
     
//         protected override Control GetEditControl(string PropName, object CurrentValue,object CurrentObj)
//        {
//            //RCommandDialog w = new RCommandDialog();
//            System.Windows.Forms.DialogResult result;
//            System.Windows.Forms.OpenFileDialog dialog = new System.Windows.Forms.OpenFileDialog();
//            dialog.Filter = "Xaml Document (*.bsky)|*.bsky";
//            result = dialog.ShowDialog();
//            if(CurrentValue != null)
//                OutputDefinition = CurrentValue.ToString();
//           // if (CurrentObj != null)
//             //   w.Canvas = CurrentObj as BSkyCanvas;
//            return result;
//        }

//        protected override object GetEditedValue(Control EditControl, string PropertyName, object oldValue,object currentObj)
//        {
//            if (EditControl is RCommandDialog)
//            {
//                RCommandDialog w = EditControl as RCommandDialog;
                
//                if (w.DialogResult.HasValue && w.DialogResult.Value)
//                {
//                    return w.CommandString;
//                }
//                return oldValue;
//            }
//            return oldValue;
//        }

     

//    }

  
      
        
//}

namespace BSky.Controls.DesignerSupport
{


    internal class FilteredFileNameEditor  : UITypeEditor
    {

       // string filter;
        //internal FilteredFileNameEditor(string s)
        //{
        //    ofd.Filter = s;
        //}
       
        private OpenFileDialog ofd = new OpenFileDialog();


        public override UITypeEditorEditStyle GetEditStyle(
         ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.Modal;
        }

        public override object EditValue(
         ITypeDescriptorContext context,
         IServiceProvider provider,
         object value)
        {
            //if (value == null) return null;
            //ofd.FileName = value.ToString();
            ofd.Filter = "XML Files|*.xml";
           
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                return ofd.FileName;
            }
            return base.EditValue(context, provider, value);
        }
    }

    //Added by Aaron 03/02/2014
    //This code below sets up a filter for the file open dialog used to select a package in the dialog installed
    //The Rpackages is a property of the canvas

    internal class FilteredZipFileNameEditor : UITypeEditor
    {

        //string filter;
        //internal FilteredFileNameEditor(string s)
        //{
        //    ofd.Filter = s;
        //}
        private OpenFileDialog ofd = new OpenFileDialog();


        public override UITypeEditorEditStyle GetEditStyle(
         ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.Modal;
        }

        public override object EditValue(
         ITypeDescriptorContext context,
         IServiceProvider provider,
         object value)
        {
            //if (value == null) return null;
            //ofd.FileName = value.ToString();
             ofd.Filter = "Zip Files|*.zip";

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                return ofd.FileName;
            }
            return base.EditValue(context, provider, value);
        }
    }
}