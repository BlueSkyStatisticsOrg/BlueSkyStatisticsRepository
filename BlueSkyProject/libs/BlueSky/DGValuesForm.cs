using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Collections;

namespace AnalyticsUnlimited.Client_WPF
{
    public partial class DGValuesForm : Form
    {
        public DGValuesForm()
        {
            InitializeComponent();
        }

        //get and set values of listbox
        public string[] ValueLableListBoxValues
        {
            get {
                    int totitem = ValLstBox.Items.Count;
                    string[] valuelablelist = new string[totitem];
                    int i = 0;
                    foreach (string item in ValLstBox.Items)
                    {
                        valuelablelist[i++] = item;
                    }                
                return valuelablelist; 
            }
            set { ValLstBox.Items.AddRange(value); }
        }
        private void ValAddBut_Click(object sender, EventArgs e)
        {
            string newl = LabeltextBox.Text;

            //validations here. 1.No duplicates, 2.v could be numeric only
            //Checking duplicate value and lable
            if(isDuplicate(newl))
            {
                MessageBox.Show("Duplicate entry!");
            }
            else
            {
                //Adding to list if value is unique
                ValLstBox.Items.Add( newl );
                //ValuetextBox.Text = "";
                LabeltextBox.Text = "";
            }

        }

        private void ValChngBut_Click(object sender, EventArgs e)
        {
            if (ValLstBox.SelectedIndex >= 0)
            {
                int index = ValLstBox.SelectedIndex;
                ///////add////////
                //string newv = ValuetextBox.Text;
                string newl = LabeltextBox.Text;

                //validations here. 1.No duplicates, 2.v could be numeric only
                //Checking duplicate value and lable
                if (isDuplicate(newl))
                {
                    MessageBox.Show("Duplicate entry!");
                }
                else
                {
                    /////remove////////
                    ValLstBox.Items.RemoveAt(index);
                    //Adding to list if value is unique
                    ValLstBox.Items.Insert(index,  newl );
                    //ValuetextBox.Text = "";
                    LabeltextBox.Text = "";
                }
            }
        }

        private void ValRemvBut_Click(object sender, EventArgs e)
        {
            MessageBox.Show(ValLstBox.SelectedIndex.ToString());
            if (ValLstBox.SelectedIndex >= 0)
            {
                ValLstBox.Items.Remove(ValLstBox.SelectedItem);
            }
        }

        private void ValLstBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ValLstBox.SelectedIndex >= 0)
            {
                string item = ValLstBox.SelectedItem.ToString();
                string l = item;
                LabeltextBox.Text = l;
            }
        }

        // for finding duplicates in list
        private bool isDuplicate(string newl)
        {
            bool found = false;
            int totitem = ValLstBox.Items.Count;
            foreach (string item in ValLstBox.Items)
            {
                string oldl = item;
               
                if (oldl.Equals(newl))
                {
                    found = true;
                    break;
                }

            }
            return found;
        }

        private void ValOKBut_Click(object sender, EventArgs e)
        {
            int totitem = ValLstBox.Items.Count;

            ///Then close the dialogue////
            DGValuesForm.ActiveForm.Close();
            //release resources
           // DGValuesForm.ActiveForm.Dispose();//if disposed here we can get listbox values in DataPanel.xaml.cs
        }

        private void ValCancelBut_Click(object sender, EventArgs e)
        {
            DGValuesForm.ActiveForm.Close();
        }

        private string getItemAtIndex(int i)
        {
            int j = 0;
            foreach(string item in ValLstBox.Items)
            {
                if (j == i) return item;
                j++;
            }
            return "";
        }

        private void moveUp_Click(object sender, EventArgs e)
        {
            int curindex = ValLstBox.SelectedIndex;//ValLstBox.Items.IndexOf(LabeltextBox.Text);//
            string selecteditem = getItemAtIndex(curindex);
            if (curindex > 0)
            {
                ValLstBox.Items.RemoveAt(curindex);//selected removed

                ValLstBox.Items.Insert(curindex, getItemAtIndex(curindex-1));//shifting above item one step down
                ValLstBox.Items.RemoveAt(curindex - 1);

                ValLstBox.Items.Insert(curindex - 1, selecteditem);
                ValLstBox.SelectedIndex = curindex - 1;
            }
        }

        private void moveDown_Click(object sender, EventArgs e)
        {
            int curindex = ValLstBox.SelectedIndex;//ValLstBox.Items.IndexOf(LabeltextBox.Text);//
            string selecteditem = getItemAtIndex(curindex);
            if (curindex < ValLstBox.Items.Count -1)
            {
                ValLstBox.Items.RemoveAt(curindex);//selected removed
                string s = getItemAtIndex(curindex + 1);
                ValLstBox.Items.Insert(curindex + 1, selecteditem);
                ValLstBox.SelectedIndex = curindex + 1;
            }
        }
    }
}
