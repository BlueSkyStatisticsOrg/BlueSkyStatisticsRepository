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
    public class BSkyPolynomialCtrl:BSkyBaseButtonCtrl
    {

        [Description("Raises the variable select by a polynomial order, the order of the polynomial is specified in textbox related with this control")]

        [Category("Control Settings"), PropertyOrder(1)]
        public string Type
        {
            get
            {
                return "Polynomial Control";
            }
        }


        private string polyOrder;

        [Category("Control Settings"), PropertyOrder(4)]
        //The move button is associated with a source and destination target list.
        //There can be 2 or more move buttons on a single dialog
        //Basically sets the vInputList property of the move button to the source list
        [Description("This is the variable list that variables will be copied/moved from.")]
        public string TextBoxForpolynomialOrder
        {
            get
            {
                return polyOrder;
            }
            set
            {

                object obj = GetResource(value);
                if (obj == null || (!(obj is BSkySpinnerCtrl)))
                {
                    MessageBox.Show("Unable to associate this polynomial control with a Spinner control on the same canvas, you must specify a spinner control");
                    return;
                }

                //Added by Aaron 09/01/2014
                //the function below makes sure that the move button is setup with the correct source and destination for a valid move
                //
                //  validInputtargets=validateInputTarget(value, "", obj.GetType().Name, "");
                //if (validInputtargets == false) return;

                //09/14/2013
                //Added by Aaron to support a Grouping variable
                if (obj is BSkySpinnerCtrl)
                {
                    polyOrder = value;

                }

            }
        }



        public BSkyPolynomialCtrl()
        {


            imgDest.BeginInit();
            imgDest.UriSource = new Uri(@"pack://application:,,,/BSky.Controls;component/Resources/noun_up_143208.png");
            imgDest.EndInit();

            this.Width = 40;
            this.Height = 40;

            imageBtn.Source = imgDest;

            this.Tag = TO_DEST;
            ToolTip tt = new ToolTip();
            tt.Content = "Creates a polynomial by raising the variable selected in the source variable list by the power specified in the related spinner control";
            this.ToolTip = tt;

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
            string newvar = "";
            //Checking whether variables moved are allowed by the destination filter
            //validVars meet filter requirements
            //invalidVars don't meet filter requirements


            if (vTargetList != null)
            {

                if (noSelectedItems == 0)
                {
                    diagResult = System.Windows.Forms.MessageBox.Show("You need to select a variable from the source variable list before clicking the move button", "Error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Question);
                    return;
                }


                if (vTargetList.GetType().Name == "SingleItemList" && noSelectedItems > 1)
                {
                    diagResult = System.Windows.Forms.MessageBox.Show("You cannot move more than 1 variable into a grouping variable list", "Error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Question);
                    return;
                }

                if (noSelectedItems > 1)
                {
                    diagResult = System.Windows.Forms.MessageBox.Show("You need to select 1 variable at a time to specify polynomial critera", "Error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Question);
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
                    
                   
                object objspin = GetResource(TextBoxForpolynomialOrder);
                if (objspin == null || (!(objspin is BSkySpinnerCtrl)))
                {
                    MessageBox.Show("Unable to associate this polynomial control with a Spinner control on the same canvas, you must specify a spinner control");
                    return;
                }
                //If there are valid variables then move them
                BSkySpinnerCtrl spin = objspin as BSkySpinnerCtrl;
                // IList<DataSourceVariable> Items = new List<DataSourceVariable>();
                List<DataSourceVariable> Items = new List<DataSourceVariable>();
                DataSourceVariable inputVar = vInputList.SelectedItems[0] as DataSourceVariable;
                int polynomialdegree = Int32.Parse(spin.text.Text);
                int startingpolynomialdegree = 1;
                bool itemIsInTarget = false;
                //Lets say the degree of the polynomial is 4, so engine^4, then we need to add
                //I(engine^4) and I(engine^3) and I(engine^2) and I(engine^1) 
                //Now if you add I(engine^5), only I(engine^5) should be added as I(engine^4) are already there
                while (polynomialdegree != 0)
                {
                    newvar = "I(" + inputVar.Name + "^" + startingpolynomialdegree + ")";
                    //Preferred way
                    DataSourceVariable ds = new DataSourceVariable();
                    ds.XName = newvar;
                    ds.Name = newvar;
                    ds.RName = newvar;
                    ds.Measure = inputVar.Measure;
                    ds.DataType = inputVar.DataType;
                    ds.Width = inputVar.Width;
                    ds.Decimals = inputVar.Decimals;
                    ds.Label = inputVar.Label;
                    ds.Alignment = inputVar.Alignment;
                    ds.ImgURL = inputVar.ImgURL;

                    if (vTargetList.ItemsCount == 0)
                    {
                        Items.Add(ds);
                    }
                    else
                    {
                        foreach (DataSourceVariable obj in vTargetList.Items)
                        {
                            if (obj.RName == ds.RName)
                                itemIsInTarget = true;

                               
                        }
                        if (!itemIsInTarget) Items.Add(ds);
                    }

                    polynomialdegree = polynomialdegree - 1;
                    startingpolynomialdegree = startingpolynomialdegree + 1;
                    itemIsInTarget = false;

                }
            
               



                //vTargetList.SetSelectedItems(arr1);
                //Added by Aaron on 12/24/2012 to get the items moved scrolled into view
                //Added by Aaron on 12/24/2012. Value is 0 as you want to scroll to the top of the selected items
                if (Items.Count != 0)
                {
                    vTargetList.AddItems(Items);
                    vTargetList.ScrollIntoView(Items[0]);
                }
                // Added by Aaron 06/09/2020
                // We will never remove the variable being moved from the source variable list
                // if (vInputList.MoveVariables)
                    ////The compiler is not allowing me to use vInputList.Items.Remove() so I have to use ItemsSource
                    //{
                    //    ListCollectionView lcw = vInputList.ItemsSource as ListCollectionView;
                    //    foreach (object obj in validVars) lcw.Remove(obj);
                    // }
                    vTargetList.Focus();

                
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
