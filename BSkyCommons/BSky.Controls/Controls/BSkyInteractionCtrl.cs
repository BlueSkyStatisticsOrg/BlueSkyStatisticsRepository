using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows;
using System.ComponentModel;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows.Media.Effects;
using BSky.Interfaces.Controls;
using BSky.Statistics.Common;
using System.Windows.Data;

namespace BSky.Controls
{
    public class BSkyInteractionCtrl:BSkyBaseButtonCtrl
    {

        [Description("Specifies all main effects and all interactions")]

        [Category("Control Settings"), PropertyOrder(1)]
        public string Type
        {
            get
            {
                return "Specify a full factorial model";
            }
        }

        public BSkyInteractionCtrl()
        {


            imgDest.BeginInit();
            imgDest.UriSource = new Uri(@"pack://application:,,,/BSky.Controls;component/Resources/asterisk.png");
            imgDest.EndInit();

            this.Width = 40;
            this.Height = 40;

            imageBtn.Source = imgDest;

            this.Tag = TO_DEST;

            //Sets the content of the move button to the grid. We add image to the grid
            this.Content = g;
            this.g.Children.Add(imageBtn);
            this.Resources.MergedDictionaries.Clear();
        }

        public override void BSkyBaseButtonCtrl_Click(object sender, RoutedEventArgs e)
        {

            double maxnoofvars = 0;
            int noSelectedItems = 0;
            int i = 0;
            int selectionIndex = 0;
            bool filterResults = false;
            System.Windows.Forms.DialogResult diagResult;
            string varList = "";
            string message = "";
            DataSourceVariable o;

            //Aaron 09/07/2013
            //I had to use a list object as I could not create a variable size array object
            List<object> validVars = new List<object>();
            List<object> invalidVars = new List<object>();

            //Added by Aaron 12/24/2013
            //You have the ability to move items to a textbox. When moving items to a textbox you don't have to check for filters
            //All we do is append the items selected separated by + into the textbox
            //We always copy the selected items to the textbox, items are never moved
            //We don't have to worry about tag

            //Destination is a BSkytargetlist 
            noSelectedItems = vInputList.SelectedItems.Count;
            string interaction = "";
            //Checking whether variables moved are allowed by the destination filter
            //validVars meet filter requirements
            //invalidVars don't meet filter requirements


            if (vTargetList != null)
            {

                if (noSelectedItems == 0)
                {
                    diagResult = System.Windows.Forms.MessageBox.Show("You need to select a variable from the  list before clicking the move button", "Error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Question);
                    return;
                }


                if (vTargetList.GetType().Name == "SingleItemList" && noSelectedItems > 1)
                {
                    diagResult = System.Windows.Forms.MessageBox.Show("You cannot move more than 1 variable into a grouping variable list", "Error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Question);
                    return;
                }

                if (noSelectedItems <= 1)
                {
                    diagResult = System.Windows.Forms.MessageBox.Show("You need to select more than 1 variable to create a full factorial model", "Error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Question);
                    return;
                }

                //Added 10/19/2013
                //Added the code below to support listboxes that only allow a pre-specified number of items or less
                //I add the number of preexisting items to the number of selected items and if it is greater than limit, I show an error

                if (vTargetList.maxNoOfVariables != string.Empty && vTargetList.maxNoOfVariables != null)
                {
                    try
                    {
                        maxnoofvars = Convert.ToDouble(vTargetList.maxNoOfVariables);
                        //Console.WriteLine("Converted '{0}' to {1}.", vTargetList.maxNoOfVariables, maxnoofvars);

                        // diagResult = System.Windows.Forms.MessageBox.Show("An invalid value has been entered for the maximum number of variables in the destination variable list" , "Message", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Question);

                    }
                    catch (FormatException)
                    {
                        diagResult = System.Windows.Forms.MessageBox.Show("An invalid value has been entered for the maximum number of variables in the target variable list", "Error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Question);
                    }
                    catch (OverflowException)
                    {
                        diagResult = System.Windows.Forms.MessageBox.Show("An invalid value has been entered for the maximum number of variables in the target variable list", "Error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Question);
                    }
                    if (maxnoofvars < (noSelectedItems + vTargetList.ItemsCount))
                    {
                        //e.Effects = DragDropEffects.None;
                        //e.Handled = true;
                        message = "The target variable list cannot have more than " + vTargetList.maxNoOfVariables + " variable(s). Please reduce your selection or remove variables from the target list";
                        diagResult = System.Windows.Forms.MessageBox.Show(message, "Error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Question);
                        return;

                    }

                }


                for (i = 0; i < noSelectedItems; i++)
                {
                    //Added by Aaron 08/12/2014
                    //Line below ensures that I move items from the source to the target only when the items are not in the target
                    //If Item already exists in target I ignore
                    //if (!vTargetList.Items.Contains(vInputList.SelectedItems[i]))
                    //{
                    //    filterResults = vTargetList.CheckForFilter(vInputList.SelectedItems[i]);
                    //    if (filterResults)
                    //    {

                    //        validVars.Add(vInputList.SelectedItems[i]);
                    //    }
                    //    else
                    //    {
                    //        invalidVars.Add(vInputList.SelectedItems[i]);
                    //    }
                    //}
                    if (i == 0)
                    {
                        interaction = interaction + vInputList.SelectedItems[i] as string;
                    }
                    else
                    {
                        interaction = interaction + "*" + vInputList.SelectedItems[i] as string;
                    }

                }
                //If there are valid variables then move them
                if (interaction != "")
                {

                    //Option 1
                    //validVars.Add(vInputList.SelectedItems[0]);
                    //List<object> tempVars = new List<object>();
                    //tempVars = validVars;
                    //foreach (object obj1 in tempVars)
                    //{
                    //    DataSourceVariable ds2 = obj1 as DataSourceVariable;
                    //    ds2.Name = interaction;
                    //    ds2.RName = interaction;
                    //    ds2.XName = interaction;
                    //}
                    //vTargetList.AddItems(tempVars);

                    //Preferred way

                    DataSourceVariable inputVar = vInputList.SelectedItems[0] as DataSourceVariable;

                    DataSourceVariable ds = new DataSourceVariable();
                    ds.XName = interaction;
                    ds.Name = interaction;
                    ds.RName = interaction;

                    ds.Measure = inputVar.Measure;
                    ds.DataType = inputVar.DataType;
                    ds.Width = inputVar.Width;
                    ds.Decimals = inputVar.Decimals;
                    ds.Label = inputVar.Label;
                    ds.Alignment = inputVar.Alignment;
                    ds.ImgURL = inputVar.ImgURL;

                    validVars.Add(ds as object);
                    vTargetList.AddItems(validVars);

                    // vInputList.SelectedItems[i]
                    //    validVars.Add(vInputList.SelectedItems[1]);


                    // vTargetList.AddItems(validVars);
                    //The code below unselects everything
                    vTargetList.UnselectAll();
                    //The code below selects all the items that are moved
                    vTargetList.SetSelectedItems(validVars);
                    //vTargetList.SetSelectedItems(arr1);
                    //Added by Aaron on 12/24/2012 to get the items moved scrolled into view
                    //Added by Aaron on 12/24/2012. Value is 0 as you want to scroll to the top of the selected items
                    vTargetList.ScrollIntoView(validVars[0]);
                    if (vInputList.MoveVariables)
                    //The compiler is not allowing me to use vInputList.Items.Remove() so I have to use ItemsSource
                    {
                        ListCollectionView lcw = vInputList.ItemsSource as ListCollectionView;
                        foreach (object obj in validVars) lcw.Remove(obj);
                    }
                    vTargetList.Focus();

                }
            }
            //Added by Aaron 07/22/2015
            //This is a valid point
            //else
            //{


            //    vTargetList.AddItems(validVars);

            //    //The code below unselects everything
            //    vTargetList.UnselectAll();
            //    //The code below selects all the items that are moved
            //    vTargetList.SetSelectedItems(validVars);
            //    //Added by Aaron on 12/24/2012 to get the items moved scrolled into view
            //    //Added by Aaron on 12/24/2012. Value is 0 as you want to scroll to the top of the selected items
            //    vTargetList.ScrollIntoView(validVars[0]);
            //}
            if (vInputList.MoveVariables)
            //The compiler is not allowing me to use vInputList.Items.Remove() so I have to use ItemsSource
            {
                ListCollectionView lcw = vInputList.ItemsSource as ListCollectionView;
                foreach (object obj in validVars) lcw.Remove(obj);
            }
            if (vTargetList != null)
                vTargetList.Focus();

            //Added by Aaron 08/13/2014
            //This is for the case that I am moving a variable year to a target list that already contains year
            //validvars.count is 0 as I have already detercted its in the target variable. I now want to high light it in the targetvariable
            if (validVars.Count == 0)
            {
                List<object> firstitem = new List<object>();
                firstitem.Add(vInputList.SelectedItems[0]);

                if (vTargetList != null)
                {

                    if (vTargetList.Items.Contains(vInputList.SelectedItems[0]))
                    {
                        vTargetList.SetSelectedItems(firstitem);
                        vTargetList.Focus();
                    }
                }

            }
            //If there are variables that don't meet filter criteria, inform the user
            if (invalidVars.Count > 0)
            {
                string cantMove = string.Join(",", invalidVars.ToArray());
                System.Windows.Forms.DialogResult result = System.Windows.Forms.MessageBox.Show("The variable(s) \"" + cantMove + "\" cannot be moved, the destination variable list does not allow variables of that type", "Save Changes", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Question);
            }
        }

    }
}
