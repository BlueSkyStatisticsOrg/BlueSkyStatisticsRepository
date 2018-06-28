using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.ComponentModel;

namespace BSky.Controls
{
    public class TargetAggregate : DragDropListForSummarize, IBSkyAffectsExecute
    {

         public TargetAggregate()
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
       
        
    
    }


    



}
