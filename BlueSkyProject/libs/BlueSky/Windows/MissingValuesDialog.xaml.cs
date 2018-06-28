using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Documents;

namespace BlueSky.Windows
{
    /// <summary>
    /// Interaction logic for MissingValuesDialog.xaml
    /// </summary>
    public partial class MissingValuesDialog : Window
    {
        public MissingValuesDialog()
        {
            InitializeComponent();
            OKClicked = false;
            isModified = false;
        }

        public bool OKClicked; //trace flag for changes
        public bool isModified;//trace flag for changes
        public string oldMisType;//store original val
        public List<string> oldmisvals;//store original val

        private string _misvaltype;
        public string mistype
        {
            get { return _misvaltype; }
            set
            {
                _misvaltype = value;
                switch (_misvaltype)
                {
                    case "none":
                        noMisvalRadio.IsChecked = true;
                        noneEnabled();
                        break;
                    case "three":
                        discreteMisvalRadio.IsChecked = true;
                        if (_misvals.Count >= 1)
                            misval1.Text = _misvals.ElementAt(0);
                        if (_misvals.Count >= 2)
                            misval2.Text = _misvals.ElementAt(1);
                        if (_misvals.Count == 3)
                            misval3.Text = _misvals.ElementAt(2);

                        threeEnabled();
                        break;
                    case "range+1":
                        rangeMisvalRadio.IsChecked = true;
                        if (_misvals.Count >= 2)
                        {
                            rangeLow.Text = _misvals.ElementAt(0);
                            rangeHigh.Text = _misvals.ElementAt(1);
                        }
                        if (_misvals.Count == 3)
                            rangeDiscrete.Text = _misvals.ElementAt(2);

                        rangeEnabled();
                        break;
                    default:
                        break;
                }
            }
        }

        //private DataColumnTypeEnum _vartype;
        //public DataColumnTypeEnum vartype
        //{
        //    get { return _vartype; }
        //    set { _vartype = value; }
        //}

        private List<string> _misvals = new List<string>();
        public List<string> misvals
        {
            get { return _misvals; }
            set
            {
                int i = 0;
                if (value != null)
                {
                    _misvals.Clear();
                    foreach (var v in value)
                    {
                        _misvals.Add(v);
                    }
                }
            }
        }

        private void noMisvalRadio_Click(object sender, RoutedEventArgs e)
        {
            noneEnabled();
        }

        private void discreteMisvalRadio_Click(object sender, RoutedEventArgs e)
        {
            threeEnabled();
        }

        private void rangeMisvalRadio_Click(object sender, RoutedEventArgs e)
        {
            rangeEnabled();
        }

        private void noneEnabled()
        {
            misval1.IsEnabled = false;
            misval2.IsEnabled = false;
            misval3.IsEnabled = false;
            rangeLow.IsEnabled = false;
            rangeHigh.IsEnabled = false;
            rangeDiscrete.IsEnabled = false;
        }

        private void threeEnabled()
        {
            misval1.IsEnabled = true;
            misval2.IsEnabled = true;
            misval3.IsEnabled = true;
            rangeLow.IsEnabled = false;
            rangeHigh.IsEnabled = false;
            rangeDiscrete.IsEnabled = false;
        }

        private void rangeEnabled()
        {
            misval1.IsEnabled = false;
            misval2.IsEnabled = false;
            misval3.IsEnabled = false;
            rangeLow.IsEnabled = true;
            rangeHigh.IsEnabled = true;
            rangeDiscrete.IsEnabled = true;
        }

        private void misvalOkbut_Click(object sender, RoutedEventArgs e)
        {
            List<string> mvlist = new List<string>();
            string mtyp = "";
            bool validentry = true;
            if (noMisvalRadio.IsChecked == true)
            {
                mtyp = "none";
                //string[] mvs = { "", "", "" };
                //misvals = mvs;
            }
            if (discreteMisvalRadio.IsChecked == true)
            {
                mtyp = "three";//+mval1+"-"+mval2+"-"+mval3;
                string mval1 = misval1.Text;
                string mval2 = misval2.Text;
                string mval3 = misval3.Text;
                string[] mvs = { mval1, mval2, mval3 };

                //misvals = mvs;

                if ((misval1.Text.Length < 1 && misval2.Text.Length < 1 && misval3.Text.Length < 1))
                {
                    MessageBox.Show("At Least one Discrete value must be provided.");
                    validentry = false;
                }
                else
                {
                    foreach (string s in mvs)//Add non blank values to list
                    {
                        if (s.Length > 0)
                            mvlist.Add(s);
                    }
                }
            }
            if (rangeMisvalRadio.IsChecked == true)
            {
                mtyp = "range+1";//+low+"-"+high+"-"+discrete;
                string low = rangeLow.Text;
                string high = rangeHigh.Text;
                string discrete = rangeDiscrete.Text;
                string[] mvs = { low, high, discrete };
                //misvals = mvs;

                if ((rangeLow.Text.Length < 1 || rangeHigh.Text.Length < 1))
                {
                    MessageBox.Show("Range must be provided.");
                    validentry = false;
                }
                else
                {
                    foreach (string s in mvs)//Add non blank values to list
                    {
                        if (s.Length > 0)
                            mvlist.Add(s);
                    }
                }

            }
            _misvaltype = mtyp;

            /// Validations ///
            foreach (string t in mvlist)//Fields must numeric only
            {
                Int32 temp;
                if (!Int32.TryParse(t, out temp))//can check other data types here in future enhancement
                {
                    validentry = false;
                    break;
                }

            }

            /// Popup window hides ///
            if (validentry)
            {
                misvals = mvlist;//assigning new missing values. Or say current missing values that user selected

                if (isMissingModified())// if missing values or types  has been modified.
                {
                    OKClicked = true;
                    isModified = true;
                    //MessageBox.Show("Modified!");
                }
                else
                {
                    OKClicked = false;
                    isModified = false;
                    //MessageBox.Show("Not Modified!");
                }
                
                MissingValuesDialog.GetWindow(this).Close();
            }
            else
            {
                if (mvlist.Count > 0)
                    MessageBox.Show("Not a valid data type.");
            }
        }


        private void misvalCancelbut_Click(object sender, RoutedEventArgs e)
        {
            OKClicked = false;
            isModified = false;
            MissingValuesDialog.GetWindow(this).Close();
        }


        private bool isMissingModified()
        {
            if (oldmisvals != null && misvals != null)
            {
                if ((misvals.Count != oldmisvals.Count)) //count is different then something has been modified
                    return true;
                for (int i = 0; i < misvals.Count; i++)// if any of the value has changed
                {
                    if (misvals.ElementAt(i) != oldmisvals.ElementAt(i))
                        return true;
                }
            }
            if (oldMisType != mistype) // if missing type has been changed
                return true;

            return false;
        }

    }
}
