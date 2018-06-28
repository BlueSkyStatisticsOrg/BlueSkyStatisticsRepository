using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using BSky.Statistics.Common;
using BSky.Statistics.Service.Engine.Interfaces;
using BSky.Lifetime;

namespace BlueSky.Windows
{
    /// <summary>
    /// Interaction logic for ValueLablesDialog.xaml
    /// </summary>
    public partial class ValueLablesDialog : Window
    {
        public ValueLablesDialog()
        {
            InitializeComponent();
        }

        Dictionary<int, string> lstBox;
        int originalSelectedIndex=-1;//No Selection. The selected index in measure dropdown when this window pop-open
        //get and set values of listbox
        public string[] ValueLableListBoxValues
        {
            get
            {
                int totitem = ValLstBox.Items.Count;
                string[] valuelablelist = new string[totitem];
                int i = 0;
                foreach (string item in ValLstBox.Items)
                {
                    valuelablelist[i++] = item;
                }
                return valuelablelist;
            }
            set {
                int i = 1;
                lstBox = new Dictionary<int, string>();////for 1=male map
                    foreach (string v in value)
                    {
                        ValLstBox.Items.Add(v);
                        lstBox.Add(i, v);
                        i++;
                    }
                }
        }
        public bool OKclicked;
        private DataColumnMeasureEnum _ColMeasure;
        public DataColumnMeasureEnum colMeasure
        {
            get { return _ColMeasure; }
            set {
                    _ColMeasure = value;
                    if (value == DataColumnMeasureEnum.Nominal)
                    {
                        changeMeasureCombo.SelectedIndex = 0;

                    }
                    else if (value == DataColumnMeasureEnum.Ordinal)
                    {
                        changeMeasureCombo.SelectedIndex = 1;

                    }
                    else
                    {
                        changeMeasureCombo.SelectedIndex = 2;

                        valLblAddBut.IsEnabled = false;
                        valLblChangeBut.IsEnabled = false;
                        valLblRemoveBut.IsEnabled = false;
                        valLblUpBut.IsEnabled = false;
                        valLblDwnBut.IsEnabled = false;

                    }
                }
        }

        private string _colName;
        public string colName
        {
            get { return _colName; }
            set { _colName = value; }
        }

        private string _datasetName;
        public string datasetName
        {
            get { return _datasetName; }
            set { _datasetName = value; }
        }

        private List<FactorMap> _factormapList = new List<FactorMap>();
        public List<FactorMap> factormapList
        {
            get 
            {
                return _factormapList;
            }
            set
            {
                _factormapList = factormapList;
            }
        }

        private string _changeFrom;
        public string changeFrom
        {
            get { return _changeFrom; }
            set { _changeFrom = value; }
        }

        private string _changeTo;
        public string changeTo
        {
            get { return _changeTo; }
            set { _changeTo = value; }
        }

        private int _maxfactors;
        public int maxfactors
        {
            get
            {
                return _maxfactors;
            }

            set 
            {
                _maxfactors = value;
            }
        }

        public bool modified;
        public int oldfactcount;// = dsvals.Length;//original number of factors
        public int newfactcount;//= fm.ValueLableListBoxValues.Length; //

        private ValueLabelDialogMatrix _vlmatrix;
        public ValueLabelDialogMatrix vlmatrix
        {
            get { return _vlmatrix; }
            set { _vlmatrix = value; }
        }
        // for finding duplicates in list
        private bool isDuplicate(string newl)
        {
            bool found = false;
            int totitem = ValLstBox.Items.Count;
            foreach (string item in ValLstBox.Items)
            {
                string oldl = item;

                if (oldl.Trim().Equals(newl.Trim()))
                {
                    found = true;
                    break;
                }

            }
            return found;
        }

        private string getItemAtIndex(int i)
        {
            int j = 0;
            foreach (string item in ValLstBox.Items)
            {
                if (j == i) return item;
                j++;
            }
            return "";
        }

        private void valLblAddBut_Click(object sender, RoutedEventArgs e)
        {
            string newl = labelTextbox.Text;
            if (newl.Length > 0)
            {
                //validations here. 1.No duplicates, 2.v could be numeric only
                //Checking duplicate value and lable
                if (isDuplicate(newl))
                {
                    MessageBox.Show("Duplicate entry!");
                }
                else
                {
                    //Adding to list if value is unique
                    ValLstBox.Items.Add(newl);
                    //ValuetextBox.Text = "";
                    vlmatrix.addLevel(newl, ValLstBox.Items.IndexOf(newl), false);//adding to matrix too
                    labelTextbox.Text = "";
                    modified = true; 
                }
            }
        }

        private void valLblRemoveBut_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to delete a factor? Empty factors will be converted to NAs", "Warning! Delete Factor?", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
            {
                ///No. Dont convert empty factors to NA. Allow me to modify
                OKclicked = false; //for allowing NAs, uncomment these 2 lines and comment 2 lines after if block
                return;
            }
            if (ValLstBox.SelectedIndex >= 0)
            {
                vlmatrix.delLevel(ValLstBox.SelectedItem.ToString(), 0);//shud come first. Before removing from the ValLstBox
                ValLstBox.Items.Remove(ValLstBox.SelectedItem);
                
                modified = true; 
            }
        }

        private void valLblChangeBut_Click(object sender, RoutedEventArgs e)
        {
            if (ValLstBox.SelectedIndex >= 0)
            {
                int index = ValLstBox.SelectedIndex;
                ///////add////////
                //string newv = ValuetextBox.Text;
                string newl = labelTextbox.Text;

                //validations here. 1.No duplicates, 2.v could be numeric only
                //Checking duplicate value and lable
                if (isDuplicate(newl))
                {
                    MessageBox.Show("Duplicate entry!");
                }
                else
                {
                    vlmatrix.changeLevel(ValLstBox.Items.GetItemAt(index).ToString(), newl);//shud com first, before changing valLstBox itms.
                    /////remove////////
                    ValLstBox.Items.RemoveAt(index);
                    //Adding to list if value is unique
                    ValLstBox.Items.Insert(index, newl);
                    
                    //ValuetextBox.Text = "";
                    labelTextbox.Text = "";
                    modified = true; 
                }
            }
        }

        private void valLblUpBut_Click(object sender, RoutedEventArgs e)
        {
            int curindex = ValLstBox.SelectedIndex;//ValLstBox.Items.IndexOf(LabeltextBox.Text);//
            string selecteditem = getItemAtIndex(curindex);
            if (curindex > 0)
            {
                ValLstBox.Items.RemoveAt(curindex);//selected removed

                ValLstBox.Items.Insert(curindex, getItemAtIndex(curindex - 1));//shifting above item one step down
                ValLstBox.Items.RemoveAt(curindex - 1);

                ValLstBox.Items.Insert(curindex - 1, selecteditem);
                ValLstBox.SelectedIndex = curindex - 1;
                modified = true; 
            }
        }

        private void valLblDwnBut_Click(object sender, RoutedEventArgs e)
        {
            int curindex = ValLstBox.SelectedIndex;//ValLstBox.Items.IndexOf(LabeltextBox.Text);//
            string selecteditem = getItemAtIndex(curindex);
            if (curindex >=0  && curindex < ValLstBox.Items.Count - 1)
            {
                ValLstBox.Items.RemoveAt(curindex);//selected removed
                string s = getItemAtIndex(curindex + 1);
                ValLstBox.Items.Insert(curindex + 1, selecteditem);
                ValLstBox.SelectedIndex = curindex + 1;
                modified = true; 
            }
        }

        private void valLblOkBut_Click(object sender, RoutedEventArgs e)
        {
            if (modified)
            {
                //vlmatrix.getFinalList();//get final list from matrix. // no need to do it here. do in DAtaPanel
                if (!checkMaxFactorsAndLoadFMap())//if factors are greater that maximum allowed
                    return;
                newfactcount = ValLstBox.Items.Count;
                switch (changeMeasureCombo.SelectedIndex)
                {
                    case 0:
                        colMeasure = DataColumnMeasureEnum.Nominal;
                        break;
                    case 1:
                        colMeasure = DataColumnMeasureEnum.Ordinal;
                        break;
                    case 2:
                        colMeasure = DataColumnMeasureEnum.Scale;
                        break;
                }

                //MessageBox.Show(colMeasure.ToString());
                OKclicked = true;
                if (oldfactcount > newfactcount)//if numerbr of factors are less than minimum required
                {
                    if (MessageBox.Show("Empty factors will be converted to NAs", "Warning!", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
                    {
                        ///No. Dont convert empty factors to NA. Allow me to modify
                        OKclicked = false; //for allowing NAs, uncomment these 2 lines and comment 2 lines after if block
                        return;
                    }
                    //OKclicked = false;// for not allowing NAs
                    //return;
                }

            }
            else
            {
                OKclicked = false;
            }
            ///Then close the dialogue////
            ValueLablesDialog.GetWindow(this).Close();
        }

        private void valLblCancelBut_Click(object sender, RoutedEventArgs e)
        {
            OKclicked = false;
            modified = false;//dont want to modify
            ///Then close the dialogue////
            ValueLablesDialog.GetWindow(this).Close();
            //DGValuesForm.ActiveForm.Close();
        }

        private void ValLstBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ValLstBox.SelectedIndex >= 0)
            {
                string item = ValLstBox.SelectedItem.ToString();
                string l = item;
                labelTextbox.Text = l;
            }
        }

        private void changeMeasureCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //for very first time, record the selection in measure dropdown as originalselection.
            if (originalSelectedIndex == -1)
            {
                originalSelectedIndex = changeMeasureCombo.SelectedIndex;
            }

            //changeFrom = changeMeasureCombo.Text; //commented for Scale > Nominal > Scale. Giving bad reasult. change from must be fixed
            // once the value lable pop_up appears
            /////finding 'changeTo'
            changeTo = "";
            switch (changeMeasureCombo.SelectedIndex)
            {
                case 0:
                    changeTo = "Nominal";
                        valLblAddBut.IsEnabled = true;
                        valLblChangeBut.IsEnabled = true;
                        valLblRemoveBut.IsEnabled = true;
                        valLblUpBut.IsEnabled = true;
                        valLblDwnBut.IsEnabled = true;
                    break;
                case 1:
                    changeTo = "Ordinal";
                        valLblAddBut.IsEnabled = true;
                        valLblChangeBut.IsEnabled = true;
                        valLblRemoveBut.IsEnabled = true;
                        valLblUpBut.IsEnabled = true;
                        valLblDwnBut.IsEnabled = true;
                    break;
                case 2:
                    changeTo = "Scale";
                        valLblAddBut.IsEnabled = false;
                        valLblChangeBut.IsEnabled = false;
                        valLblRemoveBut.IsEnabled = false;
                        valLblUpBut.IsEnabled = false;
                        valLblDwnBut.IsEnabled = false;
                    
                    break;
                default:
                    changeTo = "Scale";
                    break;
            }
            if (changeMeasureCombo.SelectedIndex != originalSelectedIndex)
            {
                modified = true;
            }
            else
            {
                modified = false;
            }
            //checkMaxFactorsAndLoadFMap();
        }

        private bool checkMaxFactorsAndLoadFMap()
        {
            bool success = true;
            IAnalyticsService analyticServ = LifetimeService.Instance.Container.Resolve<IAnalyticsService>();

            string fromTo = "from " + changeFrom + " to " + changeTo;
            object numfactors = null;
            string[] factors;
            if (changeFrom.Equals("Scale") && ( changeTo.Equals("Nominal") || changeTo.Equals("Ordinal") )) // S to N/O
            {
                //get the list of factors(numeric)
                numfactors = analyticServ.GetColNumFactors(colName, datasetName);

                if (numfactors != null && (numfactors as UAReturn).SimpleTypeData.GetType().Name == "String[]")
                {
                    factors = (string[])(numfactors as UAReturn).SimpleTypeData;
                    if (factors.Length > maxfactors)
                    {
                        string msg = "Conversion " + fromTo + " cannot be done, as number of factors are greater than " + maxfactors + ".";
                        if (changeMeasureCombo.SelectedIndex != 2)
                            MessageBox.Show(msg, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        changeMeasureCombo.SelectedIndex = 2;
                        success = false;
                    }
                    else
                    {
                        _factormapList = new List<FactorMap>();
                        FactorMap fm = null;
                        foreach (string str in factors)
                        {
                            if (!str.Trim().Equals("."))//exclude '.'
                            {
                                fm = new FactorMap();
                                fm.labels = str;
                                fm.textbox = str;
                                _factormapList.Add(fm);
                            }
                        }
                    }
                }
                else//if empty levels/factors
                {
                    string msg = "Conversion " + fromTo + " cannot be done, as number of values are less than 1. Please enter some values in data grid." ;
                    if (changeMeasureCombo.SelectedIndex != 2)
                        MessageBox.Show(msg, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    changeMeasureCombo.SelectedIndex = 2;
                    success = false;
                }
            }
            else if (changeTo.Equals("Scale") && (changeFrom.Equals("Nominal") || changeFrom.Equals("Ordinal")))// N/O to S
            {
                //get list of level names. 
                _factormapList = analyticServ.GetColumnFactormap(colName, datasetName);

            }
            //else
            //{
            //    success = false;
            //}
            //MessageBox.Show("Selection changed"+s);
            return (success);
        }

    }
}
