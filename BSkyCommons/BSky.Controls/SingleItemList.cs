using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.ComponentModel;

namespace BSky.Controls
{
    public class SingleItemList : DragDropList, IBSkyAffectsExecute
    {

         public SingleItemList()
        {
            SelectionChangeBehaviour = new BehaviourCollection();
        }

        [Category("Variable Settings"), PropertyOrder(4)]
        [DisplayName("Define Rules")]
        [Description("test")]

        //  [Category("Variable Settings")]
        public BehaviourCollection SelectionChangeBehaviour
        {
            get;
            set;
        }

        private bool canExecute = true;

        // [Category("Control Settings"), PropertyOrder(2)]
        [Category("Control Settings"), PropertyOrder(3)]
        [Description("bdzs")]
        public bool CanExecute
        {
            get
            {
                return canExecute;
            }
            set
            {
                canExecute = value;
                if (CanExecuteChanged != null)
                {
                    BSkyBoolEventArgs b = new BSkyBoolEventArgs();
                    b.Value = value;
                    CanExecuteChanged(this, b);
                }
            }
        }

        public event EventHandler<BSkyBoolEventArgs> CanExecuteChanged;

        protected override void OnSelectionChanged(SelectionChangedEventArgs e)
        {


            //A control is associated with a behavior collection
            //A behavior collection is a collection of behaviors
            //A behavior can contains one condition e.g. itemscount = 0 and one or more setters e.g. set the property canexecute =true on control destination. The setter also stores the
            //value of the property its bound to (the control and the propert name)
            //Behaviors are invoked on the control when a particular activity takes place for e.g. when a selection changes on a listbox control, when text is entered 
            //on a text control.
            //Typical use case for behaviors is as follows. Every control has a CanExecute property. This can be enabled or disabled by default.
            //The OK button will only be enabled when the can execute property of all the controls on the canvas are set to true.
            //
            //For example, when the
            //one sample t.test dialog is rendered for the first time, the destination listbox will always be empty. We would then set the default value of canexecute to false.
            //We would define a condition for when items count is > than 0 and set canexecute for the destination property to true only when items count is > than 0. 
            //We would also add another condition to set Canexecute to false when items count is 0 to account for the case when users move variables from the destination
            //back to the source.
            //Also note that you can have canexecute to true on the destination control of one sample t.test but have the canexecute property on the textbox set to false 
            //as no valid entry has be entered tocompare the dialog against. In this situation the OK button on the dialog is disabled


            // Aaron 12/25 code below ensures that the events don't fire when I am in dialog editor mode.
            //This is to address the defect when in dialog mode, the itemscount =3 and the event on the destination list fires to set CanExecute to true
            //even though the intention is to save the dialog with CanExecute to False. This will disable the OK button when running the application unless 
            //one item is in the destination list

            if (!renderVars)
            {
                base.OnSelectionChanged(e);
                BSkyCanvas parent = UIHelper.FindVisualParent<BSkyCanvas>(this);
                if (parent != null && SelectionChangeBehaviour != null)
                    parent.ApplyBehaviour(this, SelectionChangeBehaviour);
            }
        }


        //private void ListBox_MouseMove(object sender, MouseButtonEventArgs e)
        //{
        //    if (e.LeftButton == MouseButtonState.Pressed)
        //    {
        //        object data = ((ListBox)(FrameworkElement)sender).SelectedItem;
        //        if (data != null)
        //            DragDrop.DoDragDrop(this, data, DragDropEffects.Copy);
        //    }
        //}
       
        public override void ListBox_Drop(object sender, DragEventArgs e)
        {

            DragDropList tempDragDropList = null;
            DragDropListForSummarize tempDragDropListForSummarize = null;
            Boolean autoVarTemp = false;

            if (BSkyCanvas.sourceDrag is DragDropList)
            {
                tempDragDropList = BSkyCanvas.sourceDrag as DragDropList;
                autoVarTemp = tempDragDropList.AutoVar;
            }
            else if (BSkyCanvas.sourceDrag is DragDropListForSummarize)
            {
                tempDragDropListForSummarize = BSkyCanvas.sourceDrag as DragDropListForSummarize;
                autoVarTemp = tempDragDropListForSummarize.AutoVar;
            }
            
            string[] formats = e.Data.GetFormats();

            //BSkyCanvas.destDrag = (ListBox)sender;
            //The code below disables drag and drop on the source list, added June 16th
            int destindex, i, j, sourceindex, noofitems;
            System.Windows.Forms.DialogResult diagResult;
            //object[] newlist =null;
            //Aaron Modified 11/04
            //Code below prevents me from doing a move where the source and destination are the source list
            if (AutoVar == false && this == (ListBox) BSkyCanvas.sourceDrag)
            {
                e.Effects = DragDropEffects.None;
                return;
            }
            if (formats.Length > 0)
            {
                object sourcedata = e.Data.GetData(formats[0]) as object;
                ListCollectionView list = this.ItemsSource as ListCollectionView;

                if (sourcedata != null)
                {
                    //Soure and destination are different. I copy the selected item to the target
                    if ((this !=(ListBox) BSkyCanvas.sourceDrag) && (AutoVar == false))
                    {

                        if (list.IndexOf(sourcedata) < 0)
                        {
                            list.AddNewItem(sourcedata);
                            list.CommitNew();

                            //this.SelectedItem = d;
                            //e.Effects =  DragDropEffects.All;
                            this.ScrollIntoView(sourcedata);//AutoScroll
                        }

                        else
                            e.Effects = DragDropEffects.None;
                        //this.UnselectAll();
                        //02/24 Aaron
                        //This is to signify that since the source and destination are different, we have finished the copy. 
                        //We will go back to the initiation of the drag and drop to see if the source needs to be removed or kept
                        // in the source listbox. This will be determined by the value of movevariables property
                        //e.Effects is set to DragDropEffects.Copy to signify that the selected item has been copies to the destination control
                        e.Effects = DragDropEffects.Copy;
                        this.SelectedItem = sourcedata;
                        Focus();
                    }
                    
                    //Aaron 09/11/2013
                    //Here I am moving to a target that has 0 or 1 item
                    //If the target has 1 item (it cannot have more than one as I prevent this)
                    //I remove the itemin the target and add it back to the source
                    //ONE OF THE LISTBOXES MUST BE THE SOURCE LISTBOXES
                    else if ((this != (ListBox)BSkyCanvas.sourceDrag) && (AutoVar == true) && (autoVarTemp == false))
                    {
                        ListCollectionView srcList = BSkyCanvas.sourceDrag.ItemsSource as ListCollectionView;
                        noofitems = list.Count;
                        object[] arr = new object[noofitems];
                        
                        //Adding the items in the target to the source before adding new item to the target
                        for (i = 0; i < noofitems; i++)
                        {
                            arr[i] = list.GetItemAt(i);
                            srcList.AddNewItem(arr[i]);
                            srcList.CommitNew();
                        }
                        //Removing all items from the target
                        for (i = 0; i < noofitems; i++) list.Remove(arr[i]);
                        //Adding new item to the target
                        list.AddNewItem(sourcedata);
                        list.CommitNew();
                       // list.Refresh();
                        this.SelectedItem = sourcedata;
                        e.Effects = DragDropEffects.Copy;
                        Focus();
                    }

                    //Aaron 09/11/2013
                    //If the grouping variable target list box has an item, we want to disallow dragging and dropping from another target to the grouping variable target.
                    //This could crate problems with filter handling
                    //We want to prompt the user to move the variable in teh grouping variable to the source
                    else if ((this != (ListBox)BSkyCanvas.sourceDrag) && (autoVarTemp==true) && (AutoVar == true))
                    {
                        ListCollectionView srcList = BSkyCanvas.sourceDrag.ItemsSource as ListCollectionView;
                        noofitems = list.Count;
                        object[] arr = new object[noofitems];

                        if (noofitems == 1)
                        {
                            diagResult = System.Windows.Forms.MessageBox.Show("Please remove the variable from the grouping variable list before initiating the drag and drop", "Message", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Question);
                            e.Effects = DragDropEffects.None;
                            BSkyCanvas.sourceDrag.SelectedItem = null;
                            this.Focus();
                            return;
                        }     

                        //Adding the items in the target to the source before adding new item to the target
                        for (i = 0; i < noofitems; i++)
                        {
                            arr[i] = list.GetItemAt(i);
                            srcList.AddNewItem(arr[i]);
                            srcList.CommitNew();
                        }
                        //Removing all items from the target
                        for (i = 0; i < noofitems; i++) list.Remove(arr[i]);
                        //Adding new item to the target
                        list.AddNewItem(sourcedata);
                        list.CommitNew();
                       // list.Refresh();
                       // this.Focus();
                        this.SelectedItem = sourcedata;
                        e.Effects = DragDropEffects.Copy;
                        this.Focus();
                    }
                    //The source and the destination are the same i.e. the target variable
                    else
                    {
                        e.Effects = DragDropEffects.None;
                    }

                } //sourcedata |=null
            }
           
        }
    
    }


    



}
