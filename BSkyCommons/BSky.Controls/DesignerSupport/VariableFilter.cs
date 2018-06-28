using System;
using System.Windows.Controls;

namespace BSky.Controls.DesignerSupport
{
    public class VariableFilter : PropertyEditorBase
    {
        private static int count = 0;
        public VariableFilter()
        {
        }
        protected override Control GetEditControl(string PropName, object CurrentValue,object CurrentObj)
        {
            VariableFilterSelection w;
            ObjectWrapper placeHolder = CurrentObj as ObjectWrapper;
           // DragDropList selectedElement = placeHolder.SelectedObject as DragDropList;
            if (placeHolder.SelectedObject is DragDropList )
            {
                DragDropList variablelist = placeHolder.SelectedObject as DragDropList;
               //Added by Aaron 10/10/2013
                //This ensures that the variable filer dialog is opened with the correct filter settings for the number of 
                //ordinal and nominal levels
                w = new VariableFilterSelection(variablelist.nomlevels, variablelist.ordlevels);
            }
            else if (placeHolder.SelectedObject is BSkyGroupingVariable)
            {
                BSkyGroupingVariable grouplist = placeHolder.SelectedObject as BSkyGroupingVariable;
                w = new VariableFilterSelection(grouplist.nomlevels, grouplist.ordlevels);
            }
            else if(placeHolder.SelectedObject is BSkyAggregateCtrl)
            {
                BSkyAggregateCtrl agglist = placeHolder.SelectedObject as BSkyAggregateCtrl;
                w = new VariableFilterSelection(agglist.nomlevels, agglist.ordlevels);
            }

            else 
            {
                BSkySortCtrl sortlist = placeHolder.SelectedObject as BSkySortCtrl;
                w = new VariableFilterSelection(sortlist.nomlevels, sortlist.ordlevels);
            }
            
            w.Filter = CurrentValue.ToString();
            return w;
        }

        protected override object GetEditedValue(Control EditControl, string PropertyName, object oldValue,object currentObj)
        {
            ObjectWrapper placeHolder = currentObj as ObjectWrapper;
            double result;
            DragDropList variablelist=null;
            BSkyGroupingVariable grouplist=null;
            BSkyAggregateCtrl aggList =null;
            BSkySortCtrl sortList = null; 

            // This function is called by both the DragdropList and BSkyGroupingVariable controls
            //to get the filter value. The vale of number of ordinal and nominal levels are stored in
            //properties of the dragdroplist class to ensure that when the dialog is saved and opened again
            //the variable filter is populated with the saved values


            if (placeHolder.SelectedObject is DragDropList)
            {
                variablelist = placeHolder.SelectedObject as DragDropList;
            }
            else if (placeHolder.SelectedObject is BSkyGroupingVariable)
            {
                grouplist = placeHolder.SelectedObject as BSkyGroupingVariable;
               
            }
            else if (placeHolder.SelectedObject is BSkyAggregateCtrl)
            {
                aggList =placeHolder.SelectedObject as BSkyAggregateCtrl;
            }
            else
                sortList = placeHolder.SelectedObject as BSkySortCtrl;

            if (EditControl is VariableFilterSelection)
            {
            //Aaron 10/08/2013
                // I store the value ofthe number or levels I want an ordinal variable to have and the number of levels I want a nominal
                //variable to have as a property in dragdrop list
                //This is done so when I initialize the variable list, I can set the values correctly in the constructor
                VariableFilterSelection w = EditControl as VariableFilterSelection;
                if (w.chkordlevels.Text != "" && Double.TryParse(w.chkordlevels.Text, out result))
                {
                    if (placeHolder.SelectedObject is DragDropList)
                        variablelist.ordlevels = w.chkordlevels.Text;
                    else if (placeHolder.SelectedObject is BSkyAggregateCtrl)
                        aggList.ordlevels = w.chkordlevels.Text;
                    else if (placeHolder.SelectedObject is BSkySortCtrl)
                        sortList.ordlevels = w.chkordlevels.Text;
                    else if (placeHolder.SelectedObject is BSkyGroupingVariable) 
                        grouplist.ordlevels=w.chkordlevels.Text;
                }
                //Aaron 10/08/2013
                // The else part handles the case where I have entered number of levels =5 and then when I bring up the variable filterdialog, I 
                // reset the number of levels to blank.
                else
                {
                    if (placeHolder.SelectedObject is DragDropList)
                        variablelist.ordlevels = "";
                    else if (placeHolder.SelectedObject is BSkyGroupingVariable)
                        grouplist.ordlevels = "";
                    else if (placeHolder.SelectedObject is BSkyAggregateCtrl)
                        aggList.ordlevels = "";
                    else if (placeHolder.SelectedObject is BSkySortCtrl)
                        sortList.ordlevels = "";

                  //  else aggList.ordlevels = "";
                }
                if (w.chkNomlevels.Text != "" && Double.TryParse(w.chkNomlevels.Text, out result))
                {
                    if (placeHolder.SelectedObject is DragDropList)
                        variablelist.nomlevels = w.chkNomlevels.Text;
                    else if (placeHolder.SelectedObject is BSkyGroupingVariable) 
                        grouplist.nomlevels = w.chkNomlevels.Text;

                     else if (placeHolder.SelectedObject is BSkyAggregateCtrl)
                    aggList.nomlevels = w.chkNomlevels.Text;
                    else if (placeHolder.SelectedObject is BSkySortCtrl)
                        sortList.nomlevels = w.chkNomlevels.Text;
                }
                //Aaron 10/08/2013
                // The else part handles the case where I have entered number of levels =5 and then when I bring up the variable filterdialog, I 
                // reset the number of levels to blank.
                else
                {
                    if (placeHolder.SelectedObject is DragDropList)
                        variablelist.nomlevels = "";
                    else if (placeHolder.SelectedObject is BSkyGroupingVariable) grouplist.nomlevels = "";
                    else if (placeHolder.SelectedObject is BSkyAggregateCtrl)
                        aggList.nomlevels = "";
                    else if (placeHolder.SelectedObject is BSkySortCtrl)
                        sortList.nomlevels = "";
                }
              //Added by Aaron:commented line below on 12/02/2013
                //  FrameworkElement selectedElement = placeHolder.SelectedObject as FrameworkElement;
                if (w.DialogResult.HasValue && w.DialogResult.Value)
                {
                    return w.Filter;
                }
                return oldValue;
            }
            return false;
        }
    }
}
