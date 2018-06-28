using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;


namespace BSky.Controls
{
    public class BehaviourCollection : List<Behaviour>
    {

        //public BehaviourCollection()
        //{
        //}

        //public BehaviourCollection(DragDropList list)
        //{

        //}


    }

    






    public class PropertySettersCollection : List<PropertySetter>
    {
    }
    //Anil here I have a class behavior and withing that class I have the constructor, is this correct
    public class Behaviour
    {
        public Behaviour()
        {
            Setters = new PropertySettersCollection();
            Condition = new Condition();
        }
		//Anil why is Condition followed by condition
        public Condition Condition { get; set; }
        public PropertySettersCollection Setters { get; set; }
    }

    [TypeConverter(typeof(PropertySorter))]

    //Added by Aaron 11/11/2013
        //Commented line below and added PropertySorter type
   // [TypeConverter(typeof(ConditionConverter))]
    public class Condition : BSky.Controls.propCtrlMethods
    {
        public Condition()
        {
        }
        public Condition(string propName, ConditionalOperator op, string value)
        {
            PropertyName = propName;
            Operator = op;
            Value = value;
        }


        private string propertyname = null;
        private string propertyvalue = null;


        [DefaultValue(true)]
        [ PropertyOrder(1)]
        public string PropertyName
        {
            get
            {
                return propertyname;
            }
            set
            {
                //Added by Aaron 11/18/2013
                //Code below is invoked when we are not in dialog editor
                //Here we call the default setter
                if (BSkyCanvas.dialogMode == false)
                {
                    propertyname = value;
                    return;
                }
                
                
               
                //firstCanvas
                if (valPropofCtrl(BSky.Controls.Window1.selectedElementRef, value))
                {
                    propertyname = value;
                    Window1.saved = false;
                }
                else
                {
                    System.Windows.Forms.MessageBox.Show(BSky.GlobalResources.Properties.Resources.EnteredInvalidProperty);
                    propertyname = null;
                    Window1.saved = false;
                }
                       
            }
        }
       
      // public string Value { get; set; }
        private string _value;
        [DefaultValue(false)]
        [PropertyOrder(3)]
        public string Value
        {

            get
            {
                return _value;
            }
            set
            {
                _value = value;
                //Added by Aaron 06/15/2015
                //Code below prevents the user from being prompted to save the dialog after its previewed even though its been saved befote 
                if (BSkyCanvas.dialogMode == true)
                {
                    Window1.saved = false;
                }
            }

        }

        
        //Added by Aaron 11/14/2013
        //The function below checks a control looking to see if it contains a valid property name

        private ConditionalOperator _operator;

        [DefaultValue(false)]
        [PropertyOrder(2)]
        public ConditionalOperator Operator 
        {
            get
            {
                return _operator;
            }
            set
            {
                _operator = value;
                //Added by Aaron 06/15/2015
                //Code below prevents the use from being prompted to save the dialog when ever its previewed even though its been saved befote 
                if (BSkyCanvas.dialogMode == true)
                {
                    Window1.saved = false;
                }
            }
        }
    }

    public class PropertySetter : BSky.Controls.propCtrlMethods
    {
        public PropertySetter()
        {
        }
        public PropertySetter(string Control, string Property, string value)
        {
            ControlName = Control;
            PropertyName = Property;
            Value = value;
        }

        private string propertyname;
        private string controlname;

        public string ControlName
        {
            get
            {
               return controlname ;

            }
            set
            {
                //Added by Aaron 11/18/2013
                //Code below is invoked when we are not in dialog editor
                //Here we call the default setter
              //  bool rendervars = false;

               if (BSkyCanvas.dialogMode == false)
               {
                    controlname = value;
                    return;
               }
                //From this point and below, code is invoked from dialog editor
               FrameworkElement obj = getCtrl(value);
               if (obj == null)
               {
                   System.Windows.Forms.MessageBox.Show(BSky.GlobalResources.Properties.Resources.EnteredInvalidControlName);
                   controlname = null;
                   Window1.saved = false;
                    return;
               }

               if (value == "")
               {
                   System.Windows.Forms.MessageBox.Show(BSky.GlobalResources.Properties.Resources.EnteredEmptyControlName);
                   controlname = null;
                   Window1.saved = false;
                   return;
               }


                //if (obj is BSkyVariableList)
                //{
                //    // BSkyGroupingVariable objGrpVar =obj as BSkyGroupingVariable;
                //    BSkyVariableList objVarList = obj as BSkyVariableList;
                //    rendervars = objVarList.renderVars;
                //}

                // IBSkyInputControl objcast = obj as IBSkyInputControl;
                //if (obj is BSkyVariableList)
                //{
                //    BSkyVariableList objcast = obj as BSkyVariableList;
                //    rendervars = objcast.dialogMode;
                //}

                //if (obj is BSkyCheckBox)
                //{
                //    BSkyCheckBox objcast = obj as BSkyCheckBox;
                //    rendervars = objcast.dialogMode;
                //}

                //if (!rendervars)
                //{
                //    controlname = value;
                //}
                //else
                //{
                //    if (value == "")
                //    {
                //        System.Windows.Forms.MessageBox.Show("You have entered an invalid control name, please review the control names in                        the grid and enter a valid value");
                //        return;
                //    }




               //if (BSky.Controls.Window1.firstCanvas == null)
               //{
               //    controlname = value;
               //    return;
               //}


               if (value != null && PropertyName != null)
               {
                   if (valPropofCtrl(obj, PropertyName))
                   {
                       controlname = value;
                       Window1.saved = false;
                   }
                   else
                   {

                       string message = BSky.GlobalResources.Properties.Resources.ControlNameEntered + value 
                            +BSky.GlobalResources.Properties.Resources.UnmatchingPropertyName  + PropertyName; System.Windows.Forms.MessageBox.Show(message);

                       controlname = null;
                       propertyname = null;
                       Window1.saved = false;
                   }
               }
                    // else if (valPropName(value, BSky.Controls.Window1.firstCanvas)) propertyname = value;
               else
               {
                 //if obj!= null then there is a valid control
                   if (obj != null)
                   {
                       controlname = value;
                       Window1.saved = false;
                   }
                   else
                   {
                       System.Windows.Forms.MessageBox.Show(BSky.GlobalResources.Properties.Resources.EnteredInvalidControlName);
                       controlname = null;
                       Window1.saved = false;
                   }
               }
            }
        }
        
        public string PropertyName 
        { 
            get
            {
                return propertyname;
            } 
            set
            {

                //Added by Aaron 11/18/2013
                //Code below is invoked when we are not in dialog editor
                //Here we call the default setter
                if (BSkyCanvas.dialogMode == false)
                {
                    propertyname = value;
                    return;
                }
                
                if (value == "")
                {
                    System.Windows.Forms.MessageBox.Show(BSky.GlobalResources.Properties.Resources.EnteredEmptyPropertyName);
                    //Setting this to null ensures that I can cancel the dialog and the prior valid value (original value of property name does not get set
                    propertyname = null;
                    Window1.saved = false;
                    return;
                }
                
                if (ControlName != null && value != null)
                {
                    if (valPropofCtrl(getCtrl(ControlName), value))
                    {
                        propertyname = value;
                        Window1.saved = false;
                    }
                    //Where the property entered does not match control
                    else
                    {
                        string message = BSky.GlobalResources.Properties.Resources.PropertyNameEntered+" " + value 
                            + " "+BSky.GlobalResources.Properties.Resources.ControlPropertyNotValid+" " + ControlName; System.Windows.Forms.MessageBox.Show(message);
                        controlname = null;
                        propertyname = null;
                        Window1.saved = false;
                    }

                }
                else
                {
                    if (valPropName(value, BSky.Controls.Window1.firstCanvas))
                    {
                        propertyname = value;
                        Window1.saved = false;
                    }
                    else
                    {
                        System.Windows.Forms.MessageBox.Show(BSky.GlobalResources.Properties.Resources.EnteredInvalidProperty);
                        propertyname = null;
                        Window1.saved = false;
                    }
                }
            } 
        }

        
        //Added by Aaron 11/03/2013
        //Commented the code below to simplify the 
      //  public bool IsBinding { get; set; }
       // public string SourceControlName { get; set; }
       // public string SourcePropertyName { get; set; }

      //  public string Value { get; set; }

        private string _value;

        public string Value
        {
           
            get
            {
                return _value;
            }
            set
            {
                _value = value;
               // Window1.saved = false;
                //Added by Aaron 06/15/2015
                //Code below prevents the user from being prompted to save the dialog after its previewed even though its been saved befote 
                if (BSkyCanvas.dialogMode == true)
                {
                    Window1.saved = false;
                }
            }

        }



    }


    public enum ConditionalOperator
    {
        Equals,
        GreaterThan,
        GreaterThanEqualsTo,
        LessThan,
        LessThanEqualsTo,
        Like,
        Contains,
        IsNumeric,
        NotNumeric,
        isNullOrEmpty,
        //Added by Aaron 12/12/2013
        //Added this to support checking for a valid string entered in a text box
        ValidString
    }

    public class StringCollection : List<string>
    {
    }
}
