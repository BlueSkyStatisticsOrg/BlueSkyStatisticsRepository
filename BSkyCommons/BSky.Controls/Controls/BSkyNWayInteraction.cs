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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BSky.Controls
{
    public class BSkyNWayInteraction:BSkyBaseButtonCtrl
    {

        [Description("Creates N way interactions based on the number of variables selected")]

        [Category("Control Settings"), PropertyOrder(1)]
        public string Type
        {
            get
            {
                return "N way interactions";
            }
        }


        private string nWAYInteraction;

        [Category("Control Settings"), PropertyOrder(4)]
        //The move button is associated with a source and destination target list.
        //There can be 2 or more move buttons on a single dialog
        //Basically sets the vInputList property of the move button to the source list
        [Description("This is the variable list that variables will be copied/moved from.")]
        public string ComboBoxForNWayInteraction
        {
            get
            {
                return nWAYInteraction;
            }
            set
            {

                object obj = GetResource(value);
                if (obj == null || (!(obj is BSkyNonEditableComboBox)))
                {
                    MessageBox.Show("Unable to associate this NWAYInteraction control with a Combobox control on the same canvas, you must specify the name of a combobox control");
                    return;
                }

                //Added by Aaron 09/01/2014
                //the function below makes sure that the move button is setup with the correct source and destination for a valid move
                //
                //  validInputtargets=validateInputTarget(value, "", obj.GetType().Name, "");
                //if (validInputtargets == false) return;

                //09/14/2013
                //Added by Aaron to support a Grouping variable
                if (obj is BSkyNonEditableComboBox)
                {
                    nWAYInteraction = value;

                }

            }
        }



        public BSkyNWayInteraction()
        {


            imgDest.BeginInit();
            imgDest.UriSource = new Uri(@"pack://application:,,,/BSky.Controls;component/Resources/left.png");
            imgDest.EndInit();

            this.Width = 40;
            this.Height = 40;

            imageBtn.Source = imgDest;

            this.Tag = TO_DEST;

            //Sets the content of the move button to the grid. We add image to the grid
            this.Content = g;
            this.g.Children.Add(imageBtn);
            this.Resources.MergedDictionaries.Clear();
            ToolTip tt = new ToolTip();
            tt.Content = "Creates N way interaction terms from the variables selected in the source variable list. The value of N is obtained from the related combobox control";
            this.ToolTip = tt;
        }


        private static bool NextCombination(IList<int> num, int n, int k)
        {
            bool finished;

            var changed = finished = false;

            if (k <= 0) return false;

            for (var i = k - 1; !finished && !changed; i--)
            {
                if (num[i] < n - 1 - (k - 1) + i)
                {
                    num[i]++;

                    if (i < k - 1)
                        for (var j = i + 1; j < k; j++)
                            num[j] = num[j - 1] + 1;
                    changed = true;
                }
                finished = i == 0;
            }

            return changed;
        }

        private static IEnumerable<IEnumerable<T>> Combinations<T>(IEnumerable<T> elements, int k)
        {
            var elem = elements.ToArray();
            var size = elem.Length;

            if (k > size) yield break;

            var numbers = new int[k];

            for (var i = 0; i < k; i++)
                numbers[i] = i;

            do
            {
                yield return numbers.Select(n => elem[n]);
            } while (NextCombination(numbers, size, k));
        }

        static IEnumerable<IEnumerable<T>> GetPermutations<T>(IEnumerable<T> list, int length)
        {
            if (length == 1) return list.Select(t => new T[] { t });
            return GetPermutations(list, length - 1)
                .SelectMany(t => list.Where(o => !t.Contains(o)),
                    (t1, t2) => t1.Concat(new T[] { t2 }));
        }






        public override void BSkyBaseButtonCtrl_Click(object sender, RoutedEventArgs e)
        {
            double maxnoofvars = 0;
            IEnumerable<IEnumerable<string>> permResult = null;
            int noSelectedItems = 0;
            int i = 0;
            System.Windows.Forms.DialogResult diagResult;
            List<string> listOfPermutations = new List<string>();
            string message = "";
            DataSourceVariable o;

            //Aaron 09/07/2013
            //I had to use a list object as I could not create a variable size array object
            List<object> validVars = new List<object>();
            List<string> invalidVars = new List<string>();

            //Added by Aaron 12/24/2013
            //You have the ability to move items to a textbox. When moving items to a textbox you don't have to check for filters
            //All we do is append the items selected separated by + into the textbox
            //We always copy the selected items to the textbox, items are never moved
            //We don't have to worry about tag

            //Destination is a BSkytargetlist 
            noSelectedItems = vInputList.SelectedItems.Count;
           
            //Checking whether variables moved are allowed by the destination filter
            //validVars meet filter requirements
            //invalidVars don't meet filter requirements
            List<string> variableListForCombo = new List<string>();

            if (vTargetList != null)
            {

                if (noSelectedItems == 0)
                {
                    diagResult = System.Windows.Forms.MessageBox.Show("You need to select a variable from the source variable list before clicking the N Way interaction creator button", "Error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Question);
                    return;
                }


                if (vTargetList.GetType().Name == "SingleItemList" && noSelectedItems > 1)
                {
                    diagResult = System.Windows.Forms.MessageBox.Show("You cannot move more than 1 variable into a grouping variable list", "Error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Question);
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
               //System.Collections.IList <DataSourceVariable> variables = vTargetList.SelectedItems;
                   
                object objspin = GetResource(ComboBoxForNWayInteraction);
                if (objspin == null || (!(objspin is BSkyNonEditableComboBox)))
                {
                    MessageBox.Show("Unable to associate this N Way Interaction control with a non editable comboBox control on the same canvas, you must specify a non-editable combobox control");
                    return;
                }
                //If there are valid variables then move them
                BSkyNonEditableComboBox objCombo =objspin as BSkyNonEditableComboBox;

                string interaction = objCombo.SelectedItem as string;
                int interactionLevel=0;
                //interactionLevel = Convert.ToInt32(interaction[0].ToString());
                switch (interaction)
                {
                    case "All 2 way":
                        interactionLevel = 2;
                        break;
                    case "All 3 way":
                        interactionLevel = 3;
                        break;
                    case "All 4 way":
                        interactionLevel = 4;
                        break;
                    case "All 5 way":
                        interactionLevel = 5;
                        break;
                }
                string[] permutationsAsString = null;

                if (noSelectedItems < interactionLevel)
                {
                    string printString = string.Format("You need to select {0} or more variables for a {0} way interaction",interactionLevel );
                    MessageBox.Show(printString);
                    return;

                }

                for (i = 0; i < noSelectedItems; i++)
                {
                   DataSourceVariable var = vInputList.SelectedItems[i] as DataSourceVariable;
                   variableListForCombo.Add(var.Name);
                }

                permutationsAsString = variableListForCombo.ToArray();
                // permResult = GetPermutations(permutationsAsString, interactionLevel);
                permResult = Combinations(permutationsAsString, interactionLevel);
                string permutation = "";
                foreach (IEnumerable<string> itm in permResult)
                {
                   List<string> set = itm.ToList<string>();
                   permutation = set.Aggregate((m, n) => m + ":" + n);
                   listOfPermutations.Add(permutation);
                }

                int numberOfItems = listOfPermutations.Count;
                int y = 0;

                DataSourceVariable inputVar = vInputList.SelectedItems[0] as DataSourceVariable;

                for (y = 0; y < numberOfItems; y++)
                {
                    
                    // newvar = inputVar.Name  + "^" + spin.text.Text;
                    //Preferred way

                    DataSourceVariable ds = new DataSourceVariable();
                    ds.XName = listOfPermutations[y] as string;
                    ds.Name = listOfPermutations[y] as string;
                    ds.RName = listOfPermutations[y] as string;
                    ds.Measure = inputVar.Measure;
                    ds.DataType = inputVar.DataType;
                    ds.Width = inputVar.Width;
                    ds.Decimals = inputVar.Decimals;
                    ds.Label = inputVar.Label;
                    ds.Alignment = inputVar.Alignment;

                    ds.ImgURL = inputVar.ImgURL;
                    validVars.Add(ds as object);
                   
                }

             
                vTargetList.AddItems(validVars);
               // The code below unselects everything
                vTargetList.UnselectAll();
                //The code below selects all the items that are moved
                //    vTargetList.SetSelectedItems(validVars);
                //vTargetList.SetSelectedItems(arr1);
                //Added by Aaron on 12/24/2012 to get the items moved scrolled into view
                //Added by Aaron on 12/24/2012. Value is 0 as you want to scroll to the top of the selected items
                vTargetList.ScrollIntoView(validVars[0]);
                    //if (vInputList.MoveVariables)
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
            //if (vInputList.MoveVariables)
            ////The compiler is not allowing me to use vInputList.Items.Remove() so I have to use ItemsSource
            //{
            //    ListCollectionView lcw = vInputList.ItemsSource as ListCollectionView;
            //    foreach (object obj in validVars) lcw.Remove(obj);
            //}
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
