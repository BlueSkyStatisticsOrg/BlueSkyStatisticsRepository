using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows;
using System.ComponentModel;



namespace BSky.Controls
{
    public class MasterSlaveEntry
    {
        public string masterentry { get; set; }
        public string slaveentry { get; set; }
    }



    public class MasterSlaveValueCollection : List<MasterSlaveEntry>
    {
    }



    
    [TypeConverter(typeof(PropertySorter))]
    [DefaultPropertyAttribute("Type")]
   
    public class BSkyMasterListBox : CtrlListBox
    {

        //[Description("A Master listbox allows you to setup a listbox with items which when selected populate another listbox with predefined values. This is a read only property. Click on each property in the grid to see the configuration options for this listbox control.")]
        [BSkyLocalizedDescription("BSkyMasterListBox_TypeDescription", typeof(BSky.GlobalResources.Properties.Resources))]
        [Category("Control Settings"), PropertyOrder(1)]
        public string Type
        {
            get
            {
                return " Master ListBox Control";
            }
        }

        [Category("Control Settings"), PropertyOrder(3)]
        //[Description("This is the name of the slave listbox which will get populated with predefined items based on the items selected in the master listbox. Any listbox control can be a slave listbox.")]
        [BSkyLocalizedDescription("BSkyListBox_SlaveListBoxNameDescription", typeof(BSky.GlobalResources.Properties.Resources))]
        public string SlaveListBoxName
        {
            get;
            set;
        }

        private MasterSlaveValueCollection masterslaveentries;
        

        [Category("Control Settings"), PropertyOrder(5)]
        //[Description("Clisk this property and then click the lookup button to create a mapping between items in the master listbox and the items that will be displayed in the slave listbox.")]
        [BSkyLocalizedDescription("BSkyListBox_MappingMasterSlaveEntriesDescription", typeof(BSky.GlobalResources.Properties.Resources))]

        [Editor(@"BSky.Controls.DesignerSupport.MasterSlaveEditor, BSky.Controls,  Culture=neutral",
                typeof(System.Drawing.Design.UITypeEditor))]
        public MasterSlaveValueCollection MappingMasterSlaveEntries
        {
            get
            {

                return masterslaveentries;
                // return _radioButtons;
            }
            set
            {
                //01/03/2013
                //Added line below
                //_radioButtons = value;
                masterslaveentries = value;
            }
        }

        //Added by Aaron 01/01/2014
        //This function was added to ensure that when a slave name was added, a valid slave name was added
        //This is called from  void OptionsPropertyGrid_PropertyValueChanged(object s, System.Windows.Forms.PropertyValueChangedEventArgs e)
        //in window1.xaml.cs
        public bool checkIfValidChild(string slavename)
        {
            BSkyCanvas canvas = UIHelper.FindVisualParent<BSkyCanvas>(this);
            foreach (FrameworkElement fe in canvas.Children)
            {
                if (fe.Name == slavename && fe is BSkyListBox) return true;
            }
            return false;
        }


        public bool checkIfValidChild()
        {
            string slavename = this.SlaveListBoxName;
            BSkyCanvas canvas = UIHelper.FindVisualParent<BSkyCanvas>(this);
            foreach (FrameworkElement fe in canvas.Children)
            {
                if (fe.Name == slavename && fe is BSkyListBox) return true;
            }
            return false;
        }

         public FrameworkElement GetResource(string name)
        {
            BSkyCanvas canvas = UIHelper.FindVisualParent<BSkyCanvas>(this);
            foreach (FrameworkElement fe in canvas.Children)
            {
                if (fe.Name == name)
                    return fe;
            }
            return null;
        }


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

            //if (!renderVars)
            //{

            //Since we support master slave listboxes, we need to handle the following scenarios
            //1. The case that a master listbox points to a slave that does not exist or was deleted
            //2. NOT AN ERROR CONDITION Master listbox points has an empty slave, this is fine and supported.
            //3. One or more master listboxes point to the same slave
            //4. NOT AN ERROR CONDITION Master listbox points to a valid slave but no master slave mappings created. In this situation as there are no mappings as soon as an entry in the master is selected, the slave blanks out as there are no entries in the slave that map to the master. The initial settings for the slave is to display all entries
            //5. NOT AN ERROR CONDITION In the case that the entry selected does not have any mapping to the slave, the slave blanks out
                base.OnSelectionChanged(e);
            //Added by Aaron 07/05/2014
            //Prevents the slave listbox being populated with items based on selection in master listbox in dialog editor mode
                if (BSkyCanvas.applyBehaviors == true)
                {
                    string newselection = this.SelectedItem as string;
                    FrameworkElement feslave = GetResource(this.SlaveListBoxName);
                    //feslave is null if there is no valid slave listbox, in this case we don't try and display anyting in the slave listbox
                    //The slave listbox does not exist and we just treat the master as a normal listbox
                    if (feslave != null)
                    {
                        BSkyListBox slavelistbox = feslave as BSkyListBox;
                        //4. NOT AN ERROR CONDITION Master listbox points to a valid slave but no master slave mappings created. In this situation as there are no mappings as soon as an entry in the master is selected, the slave blanks out as there are no entries in the slave that map to the master. The initial settings for the slave is to display all entries
                        if (MappingMasterSlaveEntries == null) { slavelistbox.Items.Clear(); return; }
                        if (MappingMasterSlaveEntries.Count == 0) { slavelistbox.Items.Clear(); return; }
                        slavelistbox.Items.Clear();
                        int i = 0;
                        foreach (MasterSlaveEntry mse in MappingMasterSlaveEntries)
                        {
                            if (mse.masterentry == newselection)
                            {
                                slavelistbox.Items.Add(mse.slaveentry as string);
                            }
                            if (i == 0) 
                            { 
                                slavelistbox.SelectedIndex = 0;
                                i = 1; 
                            }
                        }

                    }
                }
                   
        }

  
    }
}
