using System.ComponentModel;
using BSky.Interfaces.Controls;

namespace BSky.Controls
{
    public class BSkyVariableList : DragDropList, IBSkyInputControl, IBSkyControl, IBSkyEnabledControl
    {
        public BSkyVariableList()
        {
           
        }
        public BSkyVariableList(bool SourceList, bool dialogDesigner)
        {
            base.AutoVar = !SourceList;
            
            base.renderVars = dialogDesigner;
           
            Syntax = "%%VALUE%%";
            // if (SourceList==1)
            if (SourceList && !dialogDesigner)
                {
                  if (this.ItemsCount > 0) this.SelectedIndex = 0;
                this.Focus();
                }
        }


      
      


        //Aaron 09/12/2013
        //commented the lines above to change the virtual bool function to a non virtual function 
        //commented the override in BSKYVariablelist
        //public override bool CheckForFilter(object o)
        //{
        //    //01/07/2013 Aaron

        //    //changed the if condition below from !base.Autovar to base.Autovar
        //    //Autovar = true means I am auromatrcally populating variables, also means its the source list
        //    //Aaron 03/31/2013 Autovar =true means that its the target and not the source

        //    //Aaron 09/03/2013
        //    //Commented the 2 lines below. This is becuase we are calling this functionwhen the source variable list is loaded 
        //    //and also when you are dragging a source variable to a target or a source variable from target back to source
        //    //   if (base.AutoVar)
        //    //     return true;

        //    DataSourceVariable var = o as DataSourceVariable;

        //    bool dataresult = false;
        //    bool measureresult = false;
        //    switch (var.DataType)
        //    {
        //        case DataColumnTypeEnum.String:
        //            if (Filter.Contains("String"))
        //            {
        //                dataresult = true;
        //            }
        //            break;
        //        case DataColumnTypeEnum.Numeric:
        //        case DataColumnTypeEnum.Double:
        //        case DataColumnTypeEnum.Float:
        //        case DataColumnTypeEnum.Int:
        //            if (Filter.Contains("Numeric"))
        //            {
        //                dataresult = true;
        //            }
        //            break;
        //    }

        //    if (!dataresult)
        //        return false;

        //    switch (var.Measure)
        //    {
        //        case DataColumnMeasureEnum.Nominal:
        //            if (Filter.Contains("Nominal"))
        //            {
        //                measureresult = true;
        //            }
        //            break;
        //        case DataColumnMeasureEnum.Ordinal:
        //            if (Filter.Contains("Ordinal"))
        //            {
        //                measureresult = true;
        //            }
        //            break;
        //        case DataColumnMeasureEnum.Scale:
        //            if (Filter.Contains("Scale"))
        //            {
        //                measureresult = true;
        //            }
        //            break;
        //    }

        //    return measureresult;
        //}

        
      //  [ReadOnly(true)]
        [Category("Control Settings"), PropertyOrder(1)]
       // [DisplayName("This is the source")]
        public string Type
        {
            get
            {
                if (!base.AutoVar) 
                {
                   // DisplayNameGridProperty(this, Type, "rain in spain");
                    return "Source List"; 
                }
                else 
                {
                    //DisplayNameGridProperty(this, Type, "rain in the drain spain");
                    return "Destination List"; 
                }
            }
        }


       

        #region IBSkyInputControl Members

        [CategoryAttribute("Syntax Settings")]
        //[Description("Default value of %%VALUE%% indicates that all the variables in the listbox will be replaced by the control name in the syntax. These values will be used to parameterize the syntax string created when the dialog is executed. If you want a different value, for example 'test' to replace the control name, replace %%VALUE%% with 'test' (you don't need to enter the single quotes) ")]
        [BSkyLocalizedDescription("BSkyVariableList_SyntaxDescription", typeof(BSky.GlobalResources.Properties.Resources))]
        public string Syntax
        {
            get;
            set;
        }

        #endregion

        //    public int Add(object value)
        //    {
        //        throw new NotImplementedException();
        //    }

        //    public void Clear()
        //    {
        //        throw new NotImplementedException();
        //    }

        //    public bool Contains(object value)
        //    {
        //        throw new NotImplementedException();
        //    }

        //    public int IndexOf(object value)
        //    {
        //        throw new NotImplementedException();
        //    }

        //    public void Insert(int index, object value)
        //    {
        //        throw new NotImplementedException();
        //    }

        //    public bool IsFixedSize
        //    {
        //        get { throw new NotImplementedException(); }
        //    }

        //    public bool IsReadOnly
        //    {
        //        get { throw new NotImplementedException(); }
        //    }

        //    public void Remove(object value)
        //    {
        //        throw new NotImplementedException();
        //    }

        //    public void RemoveAt(int index)
        //    {
        //        throw new NotImplementedException();
        //    }

        //    public object this[int index]
        //    {
        //        get
        //        {
        //            throw new NotImplementedException();
        //        }
        //        set
        //        {
        //            throw new NotImplementedException();
        //        }
        //    }

        //    public void CopyTo(Array array, int index)
        //    {
        //        throw new NotImplementedException();
        //    }

        //    public int Count
        //    {
        //        //Commented 04/18 
        //        get { throw new NotImplementedException(); }
        //        //Code below inserted 04/18
        //        //get
        //        //{
        //        //    return base.count;
        //        //}
        //        //set
        //        //{
        //        //    base.count = value;
        //        //}
        //    }

        //    public bool IsSynchronized
        //    {
        //        get { throw new NotImplementedException(); }
        //    }

        //    public object SyncRoot
        //    {
        //        get { throw new NotImplementedException(); }
        //    }

        //    public System.Collections.IEnumerator GetEnumerator()
        //    {
        //        throw new NotImplementedException();
        //    }


        //Added by Aaron 09/07/2013
        //SetSelectedItems is a method of Listbox that cannot be accessed directly due to its protection level.
        //I believe that this cannot be invoked directly, I had an issue calling vTargetList.SetSelectedItems directly
        //The function below was created by Vishal, I modified it to accept a list of objects

        //09/14/2013
        //Commented the code below as I have moved this to DragDropList
        //This allows me to invoke this function from SingleItemList and BSkyVariablelist
        //internal void SetSelectedItems(List<object> arr)
        //{
        //    //   // throw new NotImplementedException();
        //    //   int i =0;
        //    //   int j=0;
        //    //   int p=0;
        //    //   j =this.ItemsCount;
        //    ////ListBoxItem temp=this.Items[i] as ListBoxItem;
        //    //    ListBoxItem temp=null;
        //    //   for (i = 0; i < j; i++)
        //    //   {
        //    //       if (this.Items[i] == arr[p])
        //    //       {
        //    //           temp = this.Items[i] as ListBoxItem;
        //    //           temp.IsSelected = true;
        //    //           p = p + 1;

        //    //       }
        //    //   }
        //    //}
        //    base.SetSelectedItems(arr);
        //}

    }
}