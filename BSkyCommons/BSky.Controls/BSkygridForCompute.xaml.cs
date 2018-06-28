using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using BSky.Interfaces.Controls;
using System.ComponentModel;
using BSky.Interfaces.Controls;
using System.Collections;
using System.Collections.ObjectModel;
using BSky.Interfaces.Interfaces;
using BSky.Lifetime;
using BSky.Statistics.Common;


namespace BSky.Controls
{
    /// <summary> 
    /// Interaction logic for BSkygridForCompute.xaml 
    /// </summary> 
    /// 
    [TypeConverter(typeof(PropertySorter))]
    [DefaultPropertyAttribute("Type")]
    public partial class BSkygridForCompute : Grid, IBSkyControl
    {

        IUIController UIController;

        public BSkygridForCompute()
        {
            InitializeComponent();
          //  if (BSkyCanvas.dialogMode!)
            
            tab1.SelectedIndex = 0;

            //HelpText2.ItemsSource= observable 
            // HelpText2.ItemsSource = new ListCollectionView(new ObservableCollection<string>()); 
        }

        [Description("The compute control gives you the ability to access popular mathematical, logical and statistical functions. The function you click on automatically gets inserted in the textbox that captures the formula. This is a read only property. Click on each property in the grid to see the configuration options for this mathematical operator control. ")]

        [Category("Control Settings"), PropertyOrder(1)]
        public string Type
        {
            get
            {
                return "Compute Control";
            }
        }



        [Category("Control Settings"), PropertyOrder(2)]
        [Description("Required property. You must specify a unique name for every control added to the dialog. You will not be able to save a dialog definition unless every control on the dialog and containing sub-dialogs has a unique name. ")]
        public string name
        {
            get
            { return base.Name; }

            set
            { base.Name = value; }
        }


        private string _TextBoxNameForSyntaxSubstitution = null;
        [Category("Control Settings"), PropertyOrder(3)]
        [Description("Required property. This is the name of the textbox where the operators in this control will be substituted into. ")]
        public string TextBoxNameForSyntaxSubstitution
        {
            get
            { return _TextBoxNameForSyntaxSubstitution; }

            set
            { _TextBoxNameForSyntaxSubstitution = value; }
        }


        public bool checkValidTextBox(string name)
        {

            BSkyCanvas canvas = UIHelper.FindVisualParent<BSkyCanvas>(this);
            foreach (FrameworkElement fe in canvas.Children)
            {
                if (fe.Name == name && fe is BSkyTextBox) return true;
            }
            return false;
        }


        public BSkyTextBox getTextBoxForSubstitution(string Text)
        {
            BSkyCanvas canvas = UIHelper.FindVisualParent<BSkyCanvas>(this);
            foreach (FrameworkElement fe in canvas.Children)
            {
                if (fe.Name == Text && fe is BSkyTextBox) return fe as BSkyTextBox;
            }
            return null;
        }

        public void populateFormulaTextbox(string Text)
        {
            
            BSkyTextBox tb = getTextBoxForSubstitution(TextBoxNameForSyntaxSubstitution);
            if (tb == null)
            {
                MessageBox.Show("The TextBoxNameForSyntaxSubstitution must hold the name of a valid textbox. Check this property on the BSkygridForCompute control");
                return;
            }

            if (Text == "+")
            {
                tb.Text = tb.Text + "+";
                // HelpText1.Text = "Addition, for e.g. varx+vary"; 
                // HelpText1.Items.Add("Help:"); 
                // HelpText1.Items.Add("Addition"); 
            }
            if (Text == "-")
            {
                tb.Text = tb.Text + "-";
                //HelpText1.Text = "Subtraction, for e.g. varx-vary"; 
                // HelpText1.Items.Add("Help:"); 
                //HelpText1.Items.Add("Subtraction"); 
            }
            if (Text == "*")
            {
                tb.Text = tb.Text + "*";
                // HelpText1.Text = "Multiplication, for example varx*vary"; 
                //HelpText1.Items.Add("Help:"); 
                //HelpText1.Items.Add("Multiplication"); 
            }
            if (Text == "/")
            {
                tb.Text = tb.Text + "/";
                //HelpText1.Text = "Division, for example varx/vary"; 
                //HelpText1.Items.Add("Division"); 
            }
            if (Text == "%in%")
            {
                tb.Text = tb.Text + "%in%";
                // HelpText1.Text = "TBA"; 
                //HelpText1.Items.Add("Help:"); 
                //HelpText1.Items.Add("TBA"); 
            }
            if (Text == "^")
            {
                tb.Text = tb.Text + "^";
                //HelpText1.Text = "Exponent, for e.g. varx^2"; 
                //HelpText1.Items.Add("Help:"); 
                //HelpText1.Items.Add("Exponent e.g. varx^2"); 
            }
            if (Text == "sqrt")
            {
                tb.Text = tb.Text + "sqrt( )";
                //HelpText1.Text = "Square root e.g. sqrt(varx)"; 
                //HelpText1.Items.Add("Help:"); 
                //HelpText1.Items.Add("Square root e.g. sqrt(varx)"); 
            }
            if (Text == "log")
            {
                tb.Text = tb.Text + "log( )";
                // HelpText1.Text = "Natural logarithms, e.g. log(varx)"; 
                // HelpText1.Items.Add("Help:"); 
                // HelpText1.Items.Add("Natural logarithms, e.g. log(varx)"); 
            }
            if (Text == "log10")
            {
                tb.Text = tb.Text + "log10( )";
                //HelpText1.Text = "base 10 logarithms, e.g. log10(varx)"; 
                // HelpText1.Items.Add("Help:"); 
                //HelpText1.Items.Add("base 10 logarithms, e.g. log10(varx)"); 
            }
            if (Text == "log2")
            {
                tb.Text = tb.Text + "log2( )";
                //HelpText1.Text = "binary (base 2) logarithms e.g. log2(varx)"; 
                // HelpText1.Items.Add("Help:"); 
                //HelpText1.Items.Add("binary ( base 2) logarithms e.g. log2(varx)"); 
            }
            if (Text == "Mod")
            {
                tb.Text = tb.Text + "%%";
                //HelpText1.Text = "The R syntax for mod is %% e.g. varx%%vary"; 
                //HelpText1.Items.Add("Help:"); 
                //HelpText1.Items.Add("The R syntax for mod is %% e.g. varx%%vary"); 
            }
            if (Text == "abs")
            {
                tb.Text = tb.Text + "abs( )";
                // HelpText1.Text = "The absolute value, e.g. if varx =-3, abs(varx) results in 3"; 
                // HelpText1.Items.Add("Help:"); 
                // HelpText1.Items.Add("The absolute value, e.g. abs(varx)"); 
            }
            if (Text == ">")
            {

                tb.Text = tb.Text + ">";
                //HelpText1.Text = "Greater than, for e.g. if varx = c(1,2,3), varx > 2 results in False False True"; 
                // HelpText1.Items.Add("Help:"); 
                // HelpText2.Items.Add("Greater than, for e.g. if varx = c(1,2,3), varx > 2 results in False False True"); 
            }
            if (Text == ">=")
            {
                tb.Text = tb.Text + ">=";
                //HelpText1.Items.Add("Help:"); 
                //HelpText2.Items.Add("Greater than equal to, for e.g. if varx = c(1,2,3), varx >= 2 results in False True True"); 
            }
            if (Text == "<")
            {

                tb.Text = tb.Text + "<";
                // HelpText1.Items.Add("Help:"); 
                // HelpText2.Items.Add("less than, for e.g. if varx = c(1,2,3), varx < 2 results in True False False"); 
                //HelpText1.Text = "less than, for e.g. if varx = c(1,2,3), varx < 2 results in True False False"; 
            }
            if (Text == "<=")
            {
                tb.Text = tb.Text + "<=";
                // HelpText1.Items.Add("Help:"); 
                //HelpText2.Items.Add("less than equal to, for e.g. if varx = c(1,2,3), varx <= 2 results in True True False"); 
            }
            if (Text == "==")
            {
                tb.Text = tb.Text + "==";
                //HelpText1.Text = "Equal to, for e.g. if varx = c(1,2,3), varx == 2 results in False True False"; 
                // HelpText1.Items.Add("Help:"); 
                // HelpText2.Items.Add("Equal to, for e.g. if varx = c(1,2,3), varx == 2 results in False True False"); 
            }
            if (Text == "!=")
            {
                tb.Text = tb.Text + "!=";
                //HelpText1.Text = "Not equal to, for e.g. if varx = c(1,2,3), varx != 2 results in True False True"; 
                // HelpText1.Items.Add("Help:"); 
                // HelpText2.Items.Add("Not equal to, for e.g. if varx = c(1,2,3), varx != 2 results in True False True"); 
            }
            if (Text == "isTRUE")
            {
                tb.Text = tb.Text + "isTRUE( )";
                //HelpText1.Text = "Is true, e.g. isTRUE(varx)"; 
                //HelpText1.Items.Add("Help:"); 
                //HelpText2.Items.Add("Is true, e.g. isTRUE(varx)"); 
            }
            if (Text == "|")
            {
                tb.Text = tb.Text + "|";
                //HelpText1.Text = "or, for e.g. if varx = c(1,2), varx >3 | varx < 2 results in True False"; 
                //HelpText1.Items.Add("Help:"); 
                //HelpText2.Items.Add("or, for e.g. if varx = c(1,2), varx >3 | varx < 2 results in True False"); 
            }
            if (Text == "&")
            {
                tb.Text = tb.Text + "&";
                //HelpText1.Items.Add("Help:"); 
                //HelpText2.Items.Add("and, for e.g. if varx = c(1,2), varx > 3 & varx < 2 results in False False"); 
            }
            if (Text == "%/%")
            {
                tb.Text = tb.Text + "%/%";
                // HelpText1.Text = "Integer division, for e.g. if varx = c(5,6), varx %/% 2 results in 2 , 3"; 
                //HelpText1.Items.Add("Help:"); 
                //HelpText2.Items.Add("and, for e.g. if varx = c(1,2), varx > 3 & varx < 2 results in False False"); 
            }


            //Math operators 
            if (Text == "round")
            {
                tb.Text = tb.Text + "round (x= , digits = ) ";
                // HelpText1.Text = "rounds values ito the specified number of decimal places (default 0), for e.g. round(x, digits = 2) rounds x to 2 decimal places."; 
                //HelpText1.Items.Add("Help:"); 
                //HelpText2.Items.Add("and, for e.g. if varx = c(1,2), varx > 3 & varx < 2 results in False False"); 
            }

            if (Text == "ceiling")
            {
                tb.Text = tb.Text + "ceiling (var1) ";
                // HelpText1.Text = "returns a numeric vector containing the smallest integers not less than the corresponding elements of x, for example varx =c(5.5, 6.6), ceiling(varx) results in 6,7"; 
                //HelpText1.Items.Add("Help:"); 
                //HelpText2.Items.Add("and, for e.g. if varx = c(1,2), varx > 3 & varx < 2 results in False False"); 
            }

            if (Text == "floor")
            {
                tb.Text = tb.Text + "floor(var1) ";
                //HelpText1.Text = "numeric vector containing the largest integers not greater than the corresponding elements of x, for example varx =c(5.5, 6.6), floor(varx) results in 5,6"; 
                //HelpText1.Items.Add("Help:"); 
                //HelpText2.Items.Add("and, for e.g. if varx = c(1,2), varx > 3 & varx < 2 results in False False"); 
            }


            if (Text == "trunc")
            {
                tb.Text = tb.Text + "trunc(var1) ";
                // HelpText1.Text = "returns a numeric vector containing the integers formed by truncating the values in x toward 0, for example varx =c(5.5, -6.6), trunc(varx) results in 5,-6"; 
                //HelpText1.Items.Add("Help:"); 
                //HelpText2.Items.Add("and, for e.g. if varx = c(1,2), varx > 3 & varx < 2 results in False False"); 
            }
            if (Text == "signif")
            {
                tb.Text = tb.Text + "signif(x=var1 , digits = )";
                // HelpText1.Text = "rounds the values in its first argument to the specified number of significant digits, for example varx =c(5.556, -6.61), varx =c(5.5566, -6.61) results in 5.56,-6.61"; 
                //HelpText1.Items.Add("Help:"); 
                //HelpText2.Items.Add("and, for e.g. if varx = c(1,2), varx > 3 & varx < 2 results in False False"); 
            }

            if (Text == "gamma")
            {
                tb.Text = tb.Text + "gamma(x=)";
                //HelpText1.Text = "see R help, type help(gamma) in syntax editor and the help will display in a browser window."; 
                //HelpText1.Items.Add("Help:"); 
                //HelpText2.Items.Add("and, for e.g. if varx = c(1,2), varx > 3 & varx < 2 results in False False"); 
            }
            if (Text == "lgamma")
            {
                tb.Text = tb.Text + "gamma(x=)";
                //HelpText1.Items.Add("Help:"); 
                //HelpText2.Items.Add("and, for e.g. if varx = c(1,2), varx > 3 & varx < 2 results in False False"); 
            }
            if (Text == "beta")
            {
                tb.Text = tb.Text + "beta(a= ,b= )";
                //HelpText1.Text = "see R help, type help(beta) in syntax editor and the help will display in a browser window.";
                //HelpText1.Items.Add("Help:"); 
                //HelpText2.Items.Add("and, for e.g. if varx = c(1,2), varx > 3 & varx < 2 results in False False"); 
            }
            if (Text == "lbeta")
            {
                tb.Text = tb.Text + "lbeta(a= ,b= )";
                //HelpText1.Text = "see R help, type help(lbeta) in syntax editor and the help will display in a browser window."; 
                //HelpText1.Items.Add("Help:"); 
                //HelpText2.Items.Add("and, for e.g. if varx = c(1,2), varx > 3 & varx < 2 results in False False"); 
            }
            if (Text == "factorial")
            {
                tb.Text = tb.Text + "factorial(x= )";
                //HelpText1.Text = "factorial(x) calculates the factorial of each element in vector x"; 
                //HelpText1.Items.Add("Help:"); 
                //HelpText2.Items.Add("and, for e.g. if varx = c(1,2), varx > 3 & varx < 2 results in False False"); 
            }
            if (Text == "psigamma")
            {
                tb.Text = tb.Text + "psigamma(x =, deriv = 0)";
                //HelpText1.Text = "see R help, type help(psigamma) in syntax editor and the help will display in a browser window."; 
                //HelpText1.Items.Add("Help:"); 
                //HelpText2.Items.Add("and, for e.g. if varx = c(1,2), varx > 3 & varx < 2 results in False False"); 
            }



            //Statistical 

            if (Text == "max")
            {
                tb.Text = tb.Text + "max( )";
                //HelpText1.Text = "return the maximum of all the values"; 
                //HelpText1.Items.Add("Help:"); 
                //HelpText2.Items.Add("and, for e.g. if varx = c(1,2), varx > 3 & varx < 2 results in False False"); 
            }

            if (Text == "min")
            {
                tb.Text = tb.Text + "min( )";
                //HelpText1.Text = "return the minimum of all the values"; 
                //HelpText1.Items.Add("Help:"); 
                //HelpText2.Items.Add("and, for e.g. if varx = c(1,2), varx > 3 & varx < 2 results in False False"); 
            }

            if (Text == "mean")
            {
                tb.Text = tb.Text + "mean( )";
                //HelpText1.Text = "computes the arithmetic mean"; 
                //HelpText1.Items.Add("Help:"); 
                //HelpText2.Items.Add("and, for e.g. if varx = c(1,2), varx > 3 & varx < 2 results in False False"); 
            }


            if (Text == "median")
            {
                tb.Text = tb.Text + "median( )";
                // HelpText1.Text = "computes the median"; 
                //HelpText1.Items.Add("Help:"); 
                //HelpText2.Items.Add("and, for e.g. if varx = c(1,2), varx > 3 & varx < 2 results in False False"); 
            }
            if (Text == "sd")
            {
                tb.Text = tb.Text + "sd( )";
                //HelpText1.Text = "computes the standard deviation"; 
                //HelpText1.Items.Add("Help:"); 
                //HelpText2.Items.Add("and, for e.g. if varx = c(1,2), varx > 3 & varx < 2 results in False False"); 
            }

            if (Text == "sum")
            {
                tb.Text = tb.Text + "sum( )";
                // HelpText1.Text = "calculates the sum"; 
                //HelpText1.Items.Add("Help:"); 
                //HelpText2.Items.Add("and, for e.g. if varx = c(1,2), varx > 3 & varx < 2 results in False False"); 
            }

            if (Text == "variance")
            {
                tb.Text = tb.Text + "var( )";
                //HelpText1.Text = "Calculates the variance"; 
                //HelpText1.Items.Add("Help:"); 
                //HelpText2.Items.Add("and, for e.g. if varx = c(1,2), varx > 3 & varx < 2 results in False False"); 
            }

            //String 

            if (Text == "Extract Substring")
            {
                tb.Text = tb.Text + "substr(x=var1,start=, stop=)";
                //HelpText1.Text = "Extract or replace substrings in a character vector. x <- \"abcdef\" substr(x, 2, 4) is \"bcd\"substr(x, 2, 4) <- \"22222\" is \"a222ef\""; 
                //HelpText1.Items.Add("Help:"); 
                //HelpText2.Items.Add("and, for e.g. if varx = c(1,2), varx > 3 & varx < 2 results in False False"); 
            }

            if (Text == "Replace Substring")
            {
                tb.Text = tb.Text + "substr(x=var1,start=, stop=) =\"Enter string\"";
                // HelpText1.Text = "Find pattern in x and replace with replacement text. If fixed=FALSE then pattern is a regular expression. If fixed = T then pattern is a text string. if x =\"test123\" then sub(\"test\",\".\",x) returns .123"; 
                //HelpText1.Items.Add("Help:"); 
                //HelpText2.Items.Add("and, for e.g. if varx = c(1,2), varx > 3 & varx < 2 results in False False"); 
            }

            if (Text == "Concatenate")
            {
                tb.Text = tb.Text + "paste(var1 , var2, sep=\"\" )";
                //HelpText1.Text = "Concatenate strings after using sep string to seperate them. if varx =c(\"a\",\"b\",\"c\") , vary =c(\"l\",\"m\",\"n\") paste(varx,vary,sep=\"\") returns c(\"al\",\"bm\" \"cn\") paste(varx,vary,sep=\"Z\") returns c(\"aZl\",\"bZm\" \"cZn\")"; 
                //HelpText1.Items.Add("Help:"); 
                //HelpText2.Items.Add("and, for e.g. if varx = c(1,2), varx > 3 & varx < 2 results in False False"); 
            }


            if (Text == "toupper")
            {
                tb.Text = tb.Text + "toupper( x=)";
                // HelpText1.Text = "Converts to uppercase"; 
                //HelpText1.Items.Add("Help:"); 
                //HelpText2.Items.Add("and, for e.g. if varx = c(1,2), varx > 3 & varx < 2 results in False False"); 
            }
            if (Text == "tolower")
            {
                tb.Text = tb.Text + "tolower(x=var1 )";
                //HelpText1.Text = "converts to lowercase"; 
                //HelpText1.Items.Add("Help:"); 
                //HelpText2.Items.Add("and, for e.g. if varx = c(1,2), varx > 3 & varx < 2 results in False False"); 
            }

            //Conversion 
            if (Text == "ToNumeric")
            {
                tb.Text = tb.Text + "as.numeric( )";
                // HelpText1.Text = "Converts a string to numeric. For e.g. if varx = c(\"12.2\",\"3.3\"), as.numeric(varx) will return 12.1 3.3"; 
                //HelpText1.Items.Add("Help:"); 
                //HelpText2.Items.Add("and, for e.g. if varx = c(1,2), varx > 3 & varx < 2 results in False False"); 
            }
            if (Text == "ToString")
            {
                tb.Text = tb.Text + "as.character( )";
                // HelpText1.Text = "Converts a numeric to a string. For e.g. if varx = c(12.2,3.3), as.string(varx) will return c(\"12.2\",\"3.3\")"; 
                //HelpText1.Items.Add("Help:"); 
                //HelpText2.Items.Add("and, for e.g. if varx = c(1,2), varx > 3 & varx < 2 results in False False"); 
            }

            if (Text == "ToFactor")
            {
                tb.Text = tb.Text + "factor(x = var1)";
                // HelpText1.Text = "Converts a numeric to a string. For e.g. if varx = c(12.2,3.3), as.string(varx) will return c(\"12.2\",\"3.3\")"; 
                //HelpText1.Items.Add("Help:"); 
                //HelpText2.Items.Add("and, for e.g. if varx = c(1,2), varx > 3 & varx < 2 results in False False"); 
            }




            //Random numbers 
            if (Text == "runif")
            {
                if (BSkyCanvas.previewinspectMode == false)
                {

                    UIController = LifetimeService.Instance.Container.Resolve<IUIController>();
                    DataSource ds = null;
                    ds = UIController.GetActiveDocument();

                    string noOfRowsActiveDataset = "nrow(" + ds.Name + ")";
                    tb.Text = tb.Text + "runif(n=" + noOfRowsActiveDataset + ", min = 0, max = 1)";
                }
                else
                {
                    string noOfRowsActiveDataset = "nrow(" + "Dataset1" + ")";
                    tb.Text = tb.Text + "runif(n=" + noOfRowsActiveDataset + ", min = 0, max = 1)";
                }
                //HelpText1.Text = "Generates random numbers from a uniform distribution (includes fractions), min and max specify the range. For e.g. runif(n=1, min=5.0, max=7.5), generates 1 random number between 5 and 7.5. To generate a new dataset column varx that contains random numbers type runif(nrow=(Datasetx), min=5.0, max=7.5)"; 
                //HelpText1.Items.Add("Help:"); 
                //HelpText2.Items.Add("and, for e.g. if varx = c(1,2), varx > 3 & varx < 2 results in False False"); 
            }
            if (Text == "sample")
            {
                if (BSkyCanvas.previewinspectMode == false)
                {
                    UIController = LifetimeService.Instance.Container.Resolve<IUIController>();
                    DataSource ds = null;
                    ds = UIController.GetActiveDocument();
                    string noOfRowsActiveDataset = "nrow(" + ds.Name + ")";
                    tb.Text = tb.Text + "sample(x=, size=" + noOfRowsActiveDataset + ", replace = TRUE)";
                }
                else
                {
                    string noOfRowsActiveDataset = "nrow(" + "Dataset1" + ")";
                    tb.Text = tb.Text + "sample(x=, size=" + noOfRowsActiveDataset + ", replace = TRUE)";
                }

                // HelpText1.Text = "Generates random numbers from a uniform distribution (w/o fractions), For e.g. sample(1:100, size=5, replace = FALSE), generates 5 random numbers from 1-100 without replacement. To generate a new dataset column varx that contains random numbers type varx =sample(1:100, nrow(Datasetx), replace =TRUE). Replace controls whether sampling should be with replacement or not."; 
                //HelpText1.Items.Add("Help:"); 
                //HelpText2.Items.Add("and, for e.g. if varx = c(1,2), varx > 3 & varx < 2 results in False False"); 
            }
            if (Text == "rnorm")
            {

                if (BSkyCanvas.previewinspectMode == false)
                {
                    UIController = LifetimeService.Instance.Container.Resolve<IUIController>();
                    DataSource ds = null;
                    ds = UIController.GetActiveDocument();
                    string noOfRowsActiveDataset = "nrow(" + ds.Name + ")";
                    tb.Text = tb.Text + "rnorm(n=" + noOfRowsActiveDataset + ", mean = , sd =)";
                }
                else
                {
                    string noOfRowsActiveDataset = "nrow(" + "Dataset1" + ")";
                    tb.Text = tb.Text + "rnorm(n=" + noOfRowsActiveDataset + ", mean = , sd =)";
                }
                // HelpText1.Text = "Generates random numbers from a normal distribution (includes fractions) with a specified mean and standard deviation. For e.g. rnorm(100, mean = 25, sd = 40), generates 100 random numbers from a normal distribution with mean 50 and standard deviation 40. To generate a new column dataset varx that contains random numbers type rnorm(nrow(Datasetx) , mean = 0, sd = 1)"; 
                //HelpText1.Items.Add("Help:"); 
                //HelpText2.Items.Add("and, for e.g. if varx = c(1,2), varx > 3 & varx < 2 results in False False"); 
            }

            if (Text == "Day of Week")
            {

                tb.Text = tb.Text + "weekdays(x=var1, abbreviate = FALSE)";

            }

            if (Text == "Day of Month")
            {

                tb.Text = tb.Text + "strftime(x=var1, format =\"%e\", tz=\"\")";

            }

            if (Text == "Day of Year")
            {

                tb.Text = tb.Text + "strftime(x=var1, format =\"%j\", tz=\"\")";

            }

            if (Text == "Week of Year")
            {

                tb.Text = tb.Text + "strftime(x=var1, format =\"%U\", tz=\"\")";

            }

            if (Text == "Month")
            {

                tb.Text = tb.Text + "months(var1, abbreviate =FALSE)";

            }

            if (Text == "Month(decimal)")
            {

                tb.Text = tb.Text + "strftime(x=var1, format =\"%m\", tz=\"\") ";

            }

            if (Text == "Quarters")
            {

                tb.Text = tb.Text + "quarters(var1, abbreviate =FALSE)";

            }

            if (Text == "Year(XXXX)")
            {

                tb.Text = tb.Text + "strftime(x=var1, format =\"%Y\", tz=\"\")";

            }

            if (Text == "Year(XX)")
            {

                tb.Text = tb.Text + "strftime(x=var1, format =\"%y\", tz=\"\")";

            }

            if (Text == "Hour(00-23)")
            {

                tb.Text = tb.Text + "strftime(x = var1, format = \"%H\", tz = \"\")";

            }



            if (Text == "Hour(00-12)")
            {

                tb.Text = tb.Text + "strftime(x=var1, format =\"%I\", tz=\"\")";

            }

            if (Text == "Minute")
            {

                tb.Text = tb.Text + "strftime(x=var1, format =\"%M\", tz=\"\")";

            }
            if (Text == "Secs")
            {

                tb.Text = tb.Text + "strftime(x=var1, format =\"%S\", tz=\"\")";

            }

            if (Text == "Create a date (from string)")
            {
                tb.Text = tb.Text + "as.POSIXct(strptime(x=dateVar, format=\"%d/%m/%Y %H:%M:%S\", tz = \"\"))";
            }

            if (Text == "Date Difference")
            {
                tb.Text = tb.Text + "as.double(difftime(time1=var1, time2=var2, units=c(\"days\")))";
            }

            //05Oct2016 added by Anil
            if (Text == "Numeric to date")
            {
                tb.Text = tb.Text + "as.Date(x=var1, origin=\"1970-01-01\")";
            }
            //05Oct2016 added by Anil
            if (Text == "String to date")
            {
                tb.Text = tb.Text + "as.Date(x=var1)";
            }

            if (Text == "Replace Pattern")
            {
                tb.Text = tb.Text + "str_replace(string=var1, pattern=\"Enter pattern to replace\", replacement=\"Enter string\")";
            }

            if (Text == "Replace Pattern(ALL)")
            {
                tb.Text = tb.Text + "str_replace_all(string=var1, pattern=\"Enter pattern to replace\", replacement=\"Enter string\")";
            }

            if (Text == "Extract a Number")
           {

               tb.Text = tb.Text + "str_extract(string=var1, pattern=\"\\\\d+\\\\.*\\\\d*\")";
            }

            if (Text == "Pad")
           {

               tb.Text = tb.Text + "str_pad(string=var1, width=, side=, pad=)";
            }

            if (Text == "Trim")
            {

                tb.Text = tb.Text + "str_trim(string=, side = )";
            }

            if (Text == "Length")
            {

                tb.Text = tb.Text + "str_length(string=var1)";
            }

            if (Text == "Count(matches)")
            {

                tb.Text = tb.Text + "str_count(string=var1, pattern=)";
            }



            if (Text == "Count(matches)")
            {
                HelpText1.Text = "Count the number of matches to a pattern in a string.";
                HelpText1.Text += "\nstr_count(string=, pattern = \"\")";
                HelpText1.Text += "\nstring: Either a character vector, or something coercible to one.";
                HelpText1.Text += "\npattern: Pattern to look for.";
                HelpText1.Text += "\nUsage: var2=str_count(string=var1, pattern=\"abc\")";

            }
            



        }







        public void setHelpText(string Text)
        {

            // ListCollectionView lcw = HelpText2.ItemsSource as ListCollectionView; 

            // lcw.AddNewItem("Test Test Test"); 
            // lcw.CommitNew(); 
            // lcw.Refresh(); 
            // HelpText2.Items.Add(Text); 
            // HelpText2.Focus(); 
            // HelpText2.Refresh(); 
            HelpText1.Text = "";
            //HelpText2.Text =""; 
            if (Text == "+")
            {
                HelpText1.Text = "Addition, for e.g. varx+vary";
                // HelpText1.Items.Add("Help:"); 
                // HelpText1.Items.Add("Addition"); 
            }
            if (Text == "-")
            {
                HelpText1.Text = "Subtraction, for e.g. varx-vary";
                // HelpText1.Items.Add("Help:"); 
                //HelpText1.Items.Add("Subtraction"); 
            }
            if (Text == "*")
            {
                HelpText1.Text = "Multiplication, for example varx*vary";
                //HelpText1.Items.Add("Help:"); 
                //HelpText1.Items.Add("Multiplication"); 
            }
            if (Text == "/")
            {
                HelpText1.Text = "Division, for example varx/vary";
                //HelpText1.Items.Add("Division"); 
            }
            if (Text == "%in%")
            {
                HelpText1.Text = "Checks to see whether every element in the first vector is contained in the second";
                HelpText1.Text += "\nx =c(1,2,3), y=(3,5,6), then x %in% y results in FALSE, FALSE, TRUE";
                //HelpText1.Items.Add("Help:"); 
                //HelpText1.Items.Add("TBA"); 
            }
            if (Text == "^")
            {
                HelpText1.Text = "Exponent, for e.g. varx^2";
                //HelpText1.Items.Add("Help:"); 
                //HelpText1.Items.Add("Exponent e.g. varx^2"); 
            }
            if (Text == "sqrt")
            {
                HelpText1.Text = "Square root e.g. sqrt(varx)";
                //HelpText1.Items.Add("Help:"); 
                //HelpText1.Items.Add("Square root e.g. sqrt(varx)"); 
            }
            if (Text == "log")
            {
                HelpText1.Text = "Natural logarithms, e.g. log(varx)";
                // HelpText1.Items.Add("Help:"); 
                // HelpText1.Items.Add("Natural logarithms, e.g. log(varx)"); 
            }
            if (Text == "log10")
            {
                HelpText1.Text = "base 10 logarithms, e.g. log10(varx)";
                // HelpText1.Items.Add("Help:"); 
                //HelpText1.Items.Add("base 10 logarithms, e.g. log10(varx)"); 
            }
            if (Text == "log2")
            {
                HelpText1.Text = "binary (base 2) logarithms e.g. log2(varx)";
                // HelpText1.Items.Add("Help:"); 
                //HelpText1.Items.Add("binary ( base 2) logarithms e.g. log2(varx)"); 
            }
            if (Text == "Mod")
            {
                HelpText1.Text = "Calculates the reminder after dividing one number by another";
                HelpText1.Text +="\nThe R syntax for mod is %% e.g. varx%%vary";
                //HelpText1.Items.Add("Help:"); 
                //HelpText1.Items.Add("The R syntax for mod is %% e.g. varx%%vary"); 
            }
            if (Text == "abs")
            {
                HelpText1.Text = "The absolute value, e.g. if varx =-3, abs(varx) results in 3";
                // HelpText1.Items.Add("Help:"); 
                // HelpText1.Items.Add("The absolute value, e.g. abs(varx)"); 
            }
            if (Text == ">")
            {
                HelpText1.Text = "Greater than, for e.g. if varx = c(1,2,3), varx > 2 results in False False True";
                // HelpText1.Items.Add("Help:"); 
                // HelpText2.Items.Add("Greater than, for e.g. if varx = c(1,2,3), varx > 2 results in False False True"); 
            }
            if (Text == ">=")
            {
                HelpText1.Text = "Greater than equal to, for e.g. if varx = c(1,2,3), varx >= 2 results in False True True";
                //HelpText1.Items.Add("Help:"); 
                //HelpText2.Items.Add("Greater than equal to, for e.g. if varx = c(1,2,3), varx >= 2 results in False True True"); 
            }
            if (Text == "<")
            {
                // HelpText1.Items.Add("Help:"); 
                // HelpText2.Items.Add("less than, for e.g. if varx = c(1,2,3), varx < 2 results in True False False"); 
                HelpText1.Text = "less than, for e.g. if varx = c(1,2,3), varx < 2 results in True False False";
            }
            if (Text == "<=")
            {
                HelpText1.Text = "less than equal to, for e.g. if varx = c(1,2,3), varx <= 2 results in True True False";
                // HelpText1.Items.Add("Help:"); 
                //HelpText2.Items.Add("less than equal to, for e.g. if varx = c(1,2,3), varx <= 2 results in True True False"); 
            }
            if (Text == "==")
            {
                HelpText1.Text = "Equal to, for e.g. if varx = c(1,2,3), varx == 2 results in False True False";
                // HelpText1.Items.Add("Help:"); 
                // HelpText2.Items.Add("Equal to, for e.g. if varx = c(1,2,3), varx == 2 results in False True False"); 
            }
            if (Text == "!=")
            {
                HelpText1.Text = "Not equal to, for e.g. if varx = c(1,2,3), varx != 2 results in True False True";
                // HelpText1.Items.Add("Help:"); 
                // HelpText2.Items.Add("Not equal to, for e.g. if varx = c(1,2,3), varx != 2 results in True False True"); 
            }
            if (Text == "isTRUE")
            {
                HelpText1.Text = "Is true, e.g. isTRUE(varx)";
                //HelpText1.Items.Add("Help:"); 
                //HelpText2.Items.Add("Is true, e.g. isTRUE(varx)"); 
            }
            if (Text == "|")
            {
                HelpText1.Text = "or, for e.g. if varx = c(1,2), varx >3 | varx < 2 results in c(True, False)";
                //HelpText1.Items.Add("Help:"); 
                //HelpText2.Items.Add("or, for e.g. if varx = c(1,2), varx >3 | varx < 2 results in True False"); 
            }
            if (Text == "&")
            {
                HelpText1.Text = "and, for e.g. if varx = c(1,2), varx > 3 & varx < 2 results in c(False, False)";
                //HelpText1.Items.Add("Help:"); 
                //HelpText2.Items.Add("and, for e.g. if varx = c(1,2), varx > 3 & varx < 2 results in False False"); 
            }
            if (Text == "%/%")
            {
                HelpText1.Text = "Integer division, for e.g. if varx = c(5,6), varx %/% 2 results in c(2,3)";
                //HelpText1.Items.Add("Help:"); 
                //HelpText2.Items.Add("and, for e.g. if varx = c(1,2), varx > 3 & varx < 2 results in False False"); 
            }


            //Math operators 
            if (Text == "round")
            {
                HelpText1.Text = "Rounds values ito the specified number of decimal places (default 0)";
                HelpText1.Text += "\nfor e.g. round(x, digits = 2) rounds x to 2 decimal places.";


                HelpText1.Text += "\nround(x, digits = 0)";
                HelpText1.Text += "\nx: a numeric vector";
                HelpText1.Text += "\ndigits: integer indicating the number of decimal places to round";
                
                HelpText1.Text += "\nfor example var1 =c(5.556, -6.61), round(x=var1, digits=2) results in 5.56,-6.61";
                //HelpText1.Items.Add("Help:"); 
                //HelpText2.Items.Add("and, for e.g. if varx = c(1,2), varx > 3 & varx < 2 results in False False"); 
            }

            if (Text == "ceiling")
            {
                HelpText1.Text = "Returns a numeric vector containing the smallest integers not less than the corresponding elements of x";
                HelpText1.Text +="\nfor example varx =c(5.5, 6.6), ceiling(varx) results in c(6,7)";
                //HelpText1.Items.Add("Help:"); 
                //HelpText2.Items.Add("and, for e.g. if varx = c(1,2), varx > 3 & varx < 2 results in False False"); 
            }

            if (Text == "floor")
            {
                HelpText1.Text = "Numeric vector containing the largest integers not greater than the corresponding elements of x";
                HelpText1.Text +="\nfor example varx =c(5.5, 6.6), floor(varx) results in c(5,6)";
                //HelpText1.Items.Add("Help:"); 
                //HelpText2.Items.Add("and, for e.g. if varx = c(1,2), varx > 3 & varx < 2 results in False False"); 
            }


            if (Text == "trunc")
            {
                HelpText1.Text = "returns a numeric vector containing the integers formed by truncating the values in x toward 0";
                HelpText1.Text +="\nfor example varx =c(5.5, -6.6), trunc(varx) results in c(5,-6)";
                //HelpText1.Items.Add("Help:"); 
                //HelpText2.Items.Add("and, for e.g. if varx = c(1,2), varx > 3 & varx < 2 results in False False"); 
            }
            if (Text == "signif")
            {
                HelpText1.Text = "rounds the values in its first argument to the specified number of significant digits";
                HelpText1.Text +="\nsignif(x=, digits = 6)";
                HelpText1.Text +="\nx: a numeric vector";
                HelpText1.Text +="\ndigits: integer indicating the number of significant digits to be used";
                HelpText1.Text +="\nx: integer indicating the number of significant digits to be used";
                HelpText1.Text += "\nfor example var1=c(243.44, 265.33,22.520). signif(var1,digits=3) results in c(243,265,22.5)";
                //HelpText1.Items.Add("Help:"); 
                //HelpText2.Items.Add("and, for e.g. if varx = c(1,2), varx > 3 & varx < 2 results in False False"); 
            }

            if (Text == "gamma")
            {
                HelpText1.Text = "see R help, type help(gamma) in syntax editor and the help will display in a browser window.";
                //HelpText1.Items.Add("Help:"); 
                //HelpText2.Items.Add("and, for e.g. if varx = c(1,2), varx > 3 & varx < 2 results in False False"); 
            }
            if (Text == "lgamma")
            {
                HelpText1.Text = "see R help, type help(lgamma) in syntax editor and the help will display in a browser window.";
                //HelpText1.Items.Add("Help:"); 
                //HelpText2.Items.Add("and, for e.g. if varx = c(1,2), varx > 3 & varx < 2 results in False False"); 
            }
            if (Text == "beta")
            {
                HelpText1.Text = "see R help, type help(beta) in syntax editor and the help will display in a browser window.";
                //HelpText1.Items.Add("Help:"); 
                //HelpText2.Items.Add("and, for e.g. if varx = c(1,2), varx > 3 & varx < 2 results in False False"); 
            }
            if (Text == "lbeta")
            {
                HelpText1.Text = "see R help, type help(lbeta) in syntax editor and the help will display in a browser window.";
                //HelpText1.Items.Add("Help:"); 
                //HelpText2.Items.Add("and, for e.g. if varx = c(1,2), varx > 3 & varx < 2 results in False False"); 
            }
            if (Text == "factorial")
            {
                HelpText1.Text = "factorial(x) calculates the factorial of each element in vector x";
                //HelpText1.Items.Add("Help:"); 
                //HelpText2.Items.Add("and, for e.g. if varx = c(1,2), varx > 3 & varx < 2 results in False False"); 
            }
            if (Text == "psigamma")
            {
                HelpText1.Text = "see R help, type help(psigamma) in syntax editor and the help will display in a browser window.";
                //HelpText1.Items.Add("Help:"); 
                //HelpText2.Items.Add("and, for e.g. if varx = c(1,2), varx > 3 & varx < 2 results in False False"); 
            }

         //   if (Text == "Extract a Number")
          //  {

            //    HelpText1.Text = "str_extract(string=var1, pattern=\"\\d+\\.*\\d*\")";
           // }
            //Statistical 

            if (Text == "max")
            {
                HelpText1.Text = "return the maximum of all the values";
                //HelpText1.Items.Add("Help:"); 
                //HelpText2.Items.Add("and, for e.g. if varx = c(1,2), varx > 3 & varx < 2 results in False False"); 
            }

            if (Text == "min")
            {
                HelpText1.Text = "return the minimum of all the values";
                //HelpText1.Items.Add("Help:"); 
                //HelpText2.Items.Add("and, for e.g. if varx = c(1,2), varx > 3 & varx < 2 results in False False"); 
            }

            if (Text == "mean")
            {
                HelpText1.Text = "computes the arithmetic mean";
                //HelpText1.Items.Add("Help:"); 
                //HelpText2.Items.Add("and, for e.g. if varx = c(1,2), varx > 3 & varx < 2 results in False False"); 
            }


            if (Text == "median")
            {
                HelpText1.Text = "computes the median";
                //HelpText1.Items.Add("Help:"); 
                //HelpText2.Items.Add("and, for e.g. if varx = c(1,2), varx > 3 & varx < 2 results in False False"); 
            }
            if (Text == "sd")
            {
                HelpText1.Text = "computes the standard deviation";
                //HelpText1.Items.Add("Help:"); 
                //HelpText2.Items.Add("and, for e.g. if varx = c(1,2), varx > 3 & varx < 2 results in False False"); 
            }

            if (Text == "sum")
            {
                HelpText1.Text = "calculates the sum";
                //HelpText1.Items.Add("Help:"); 
                //HelpText2.Items.Add("and, for e.g. if varx = c(1,2), varx > 3 & varx < 2 results in False False"); 
            }

            if (Text == "variance")
            {
                HelpText1.Text = "Calculates the variance";
                //HelpText1.Items.Add("Help:"); 
                //HelpText2.Items.Add("and, for e.g. if varx = c(1,2), varx > 3 & varx < 2 results in False False"); 
            }

            //String 

            if (Text == "Extract Substring")
            {
                // HelpText1.Text = "Extract substrings in a character vector. x <- \"abcdef\" substr(x, 2, 4) is \"bcd\"substr(x, 2, 4) <- \"22222\" is \"a222ef\""; 
                //HelpText1.Items.Add("Help:"); 
                //HelpText2.Items.Add("and, for e.g. if varx = c(1,2), varx > 3 & varx < 2 results in False False"); 
                HelpText1.Text = "Extract substrings in a character vector by specifying start and end position.";
                HelpText1.Text += "\nsubstr(x, start, stop)";
                HelpText1.Text += "\nx: a character vector";
                HelpText1.Text += "\nstart: The first character in the vector to extract";
                HelpText1.Text += "\nstop: The last character in the vector to extract";
                HelpText1.Text += "\nUsage: var2 =substr(x=var1,start=2, stop=4)";
                HelpText1.Text += "\nExample: if m <- \"abcdef\" y=substr(x=m, start=2, stop=4) y gets set to \"bcd\"";
            }

            if (Text == "Replace Substring")
            {
                //HelpText1.Text = "Find pattern in x and replace with replacement text. If fixed=FALSE then pattern is a regular expression. If fixed = T then pattern is a text string. if x =\"test123\" then sub(\"test\",\".\",x) returns .123"; 
                //HelpText1.Items.Add("Help:"); 
                //HelpText2.Items.Add("and, for e.g. if varx = c(1,2), varx > 3 & varx < 2 results in False False"); 
                HelpText1.Text = "Replace substring in a string by specifying start and end position.";
                HelpText1.Text += "\nsubstr(x, start, stop)";
                HelpText1.Text += "\nx: a character vector";
                HelpText1.Text += "\nstart: The first character in the vector to replace";
                HelpText1.Text += "\nstop: The last character in the vector to replace";
                HelpText1.Text += "\nUsage: substr(x=var1,start=2, stop=4) =\"pqr\"";
                HelpText1.Text += "\nExample if m <- \"abcdef\" substr(x=m, start=2, stop=4) <-\"222\" x is now \"a222ef\"";

            }

            if (Text == "Replace Pattern")
            {
                // HelpText1.Text = "Extract substrings in a character vector. x <- \"abcdef\" substr(x, 2, 4) is \"bcd\"substr(x, 2, 4) <- \"22222\" is \"a222ef\""; 
                //HelpText1.Items.Add("Help:"); 
                //HelpText2.Items.Add("and, for e.g. if varx = c(1,2), varx > 3 & varx < 2 results in False False"); 


                HelpText1.Text = "Replace a substring within a string (replaces the first match)";
                HelpText1.Text += "\nstr_replace(string, pattern, replacement)";
                HelpText1.Text += "\nstring :Either a character vector, or something coercible to one.";
                HelpText1.Text += "\npattern: a string or regular expression";
                HelpText1.Text += "\nreplacement: string";
                HelpText1.Text += "\nReturns a character vector";
                HelpText1.Text += "\nUsage1: str_replace(string=var1, pattern=\"bcd\", replacement=\"XXX\")";
                HelpText1.Text += "\nUsage2: str_replace(string=var1, pattern=\"[aeiou]\", replacement=\"-\")";
            }

            if (Text == "Replace Pattern(ALL)")
            {
                // HelpText1.Text = "Extract substrings in a character vector. x <- \"abcdef\" substr(x, 2, 4) is \"bcd\"substr(x, 2, 4) <- \"22222\" is \"a222ef\""; 
                //HelpText1.Items.Add("Help:"); 
                //HelpText2.Items.Add("and, for e.g. if varx = c(1,2), varx > 3 & varx < 2 results in False False"); 
                HelpText1.Text = "Replace a substring within a string (replaces ALL matches)";
                HelpText1.Text += "\nstr_replace_all(string, pattern, replacement)";
                HelpText1.Text += "\nstring :Either a character vector, or something coercible to one.";
                HelpText1.Text += "\npattern: a string or regular expression";
                HelpText1.Text += "\nreplacement: string";
                HelpText1.Text += "\nReturns a character vector";
                HelpText1.Text += "\nUsage1: str_replace_all(string=var1, pattern=\"bcd\", replacement=\"XXX\")";
                HelpText1.Text += "\nUsage2: str_replace_all(string=var1, pattern=\"[aeiou]\", replacement=\"-\")";
            }






            if (Text == "Concatenate")
            {
                //HelpText1.Text = "Concatenate strings after using sep string to seperate them. if varx =c(\"a\",\"b\",\"c\") , vary =c(\"l\",\"m\",\"n\") paste(varx,vary,sep=\"\") returns c(\"al\",\"bm\" \"cn\") paste(varx,vary,sep=\"Z\") returns c(\"aZl\",\"bZm\" \"cZn\")"; 
                //HelpText1.Items.Add("Help:"); 
                //HelpText2.Items.Add("and, for e.g. if varx = c(1,2), varx > 3 & varx < 2 results in False False"); 
                HelpText1.Text = "Concatenate strings with an optional separator";
                HelpText1.Text += "\npaste (..., sep = \"\")";
                HelpText1.Text += "\n... one or more strings";
                HelpText1.Text += "\nsep: a character to separate the strings or \"\" for no separator";
                HelpText1.Text += "\nUsage 1: var3 =paste(var1,var2,sep=\"\")";
                HelpText1.Text += "\nUsage 2: var3 =paste(var1,\"test\",sep=\"\")";
                //HelpText1.Text += "\nUsage1: str_replace_all(string=var1, pattern=\"bcd\", replacement=\"XXX\")"; 
                //HelpText1.Text += "\nUsage2: str_replace_all(string=var1, pattern=\"[aeiou]\", replacement=\"-\")"; 
                HelpText1.Text += "Example:if varx =c(\"a\",\"b\",\"c\") , vary =c(\"l\",\"m\",\"n\") paste(varx,vary,sep=\"\") returns c(\"al\",\"bm\" \"cn\")";

            }


            if (Text == "toupper")
            {
                HelpText1.Text = "Converts to uppercase";
                HelpText1.Text += "\ntoupper(x=)";
                HelpText1.Text += "\nx=a character vector, or an object that can be coerced to character by as.character.";
                HelpText1.Text += "\nUsage var2=toupper(x=var1)";
                //HelpText1.Items.Add("Help:"); 
                //HelpText2.Items.Add("and, for e.g. if varx = c(1,2), varx > 3 & varx < 2 results in False False"); 
            }
            if (Text == "tolower")
            {
                HelpText1.Text = "Converts to lowercase";
                HelpText1.Text += "\ntolower(x=)";
                HelpText1.Text += "\nx=a character vector, or an object that can be coerced to character by as.character.";
                HelpText1.Text += "\nUsage var2=tolower(x=var1)";
                //HelpText1.Items.Add("Help:"); 
                //HelpText2.Items.Add("and, for e.g. if varx = c(1,2), varx > 3 & varx < 2 results in False False"); 
            }

            //Conversion 
            if (Text == "ToNumeric")
            {
                HelpText1.Text = "Converts a string to numeric. For e.g. if varx = c(\"12.2\",\"3.3\"), as.numeric(varx) will return 12.1 3.3";
                //HelpText1.Items.Add("Help:"); 
                //HelpText2.Items.Add("and, for e.g. if varx = c(1,2), varx > 3 & varx < 2 results in False False"); 
            }
            if (Text == "ToString")
            {
                HelpText1.Text = "Converts a numeric to a string. For e.g. if varx = c(12.2,3.3), as.string(varx) will return c(\"12.2\",\"3.3\")";
                //HelpText1.Items.Add("Help:"); 
                //HelpText2.Items.Add("and, for e.g. if varx = c(1,2), varx > 3 & varx < 2 results in False False"); 
            }


            if (Text == "ToFactor")
            {
                HelpText1.Text = "Create a factor.";
                HelpText1.Text += "\n\nfactor(x = character(), levels, labels = levels,exclude = NA, ordered = is.ordered(x))";
                HelpText1.Text += "\n\nx: a vector of data, usually a small number of distinct values";
                HelpText1.Text += "\n\nlevels: The unique values that x contains";
                HelpText1.Text += "\n\nlabels: either an optional character vector of labels for the levels, or a character string of length 1.";
                HelpText1.Text += "\n\nexclude: a vector of values to be excluded when forming the set of levels.";
                HelpText1.Text += "\n\nordered: logical flag to determine if the levels should be regarded as ordered ";
                HelpText1.Text += "\n\nvar1=c(1,1,0,0,1,1,0,1,0)";
                HelpText1.Text += "\nUsage1:factor(x = var1)";
                HelpText1.Text += "\nUsage2:factor(x = var1, levels=c(0,1), labels = c(\"veggies\",\"fruits\"))";
                HelpText1.Text += "\nUsage3:factor(x = var1, levels=c(0,1), labels = c(\"veggies\",\"fruits\"),ordered=TRUE)";

                //HelpText1.Items.Add("Help:"); 
                //HelpText2.Items.Add("and, for e.g. if varx = c(1,2), varx > 3 & varx < 2 results in False False"); 
            }





            //Random numbers 
            if (Text == "runif")
            {
                HelpText1.Text = "Generates random numbers from a uniform distribution (includes fractions)";
                HelpText1.Text += "\nrunif(n, min = 0, max = 1)";
                HelpText1.Text += "\nn: The number of random numbers to generate, to create a random variable in a dataset, n=nrow(Dataset) sets n to the number of rows in the dataset";
                HelpText1.Text += "\n\nmin and max specify the range.";
                HelpText1.Text +="\nFor e.g. runif(n=1, min=5.0, max=7.5) generates 1 random number between 5 and 7.5.";
                HelpText1.Text += "\n\n var1=runif(n=nrow(Dataset), min=5.0, max=7.5) generates a new dataset column var1 that contains random numbers between 5 and 7.5";
                //HelpText1.Items.Add("Help:"); row
                //HelpText2.Items.Add("and, for e.g. if varx = c(1,2), varx > 3 & varx < 2 results in False False"); 
            }
            if (Text == "sample")
            {
                HelpText1.Text = "Takes a sample of the specified size from the elements of x using either with or without replacement.";
                HelpText1.Text += "\n\nsample(x, size, replace = FALSE, prob = NULL)";
                HelpText1.Text += "\n\nx:A vector of one or more elements from which to choose randomly from e.g. x=1:100.";
                HelpText1.Text += "\n\nsize:The number of items to choose randomly from x. When creating a random variable in a dataset, size=nrow(Dataset) sets size to the number of rows in the dataset";
                HelpText1.Text += "\n\nreplace:Shoud sampling be performed with replacement";
                HelpText1.Text += "\n\nprob:A vector of probability weights for obtaining the elements of the vector being sampled";

                HelpText1.Text += "\n\nFor e.g. sample(x=1:100, size=5, replace = FALSE), generates 5 random numbers from 1-100 without replacement.";
                HelpText1.Text += "\n\nTo generate a new dataset column varx that contains random numbers type varx =sample(x=1:100, size=nrow(Datasetx), replace =TRUE).";
                //HelpText1.Items.Add("Help:"); 
                //HelpText2.Items.Add("and, for e.g. if varx = c(1,2), varx > 3 & varx < 2 results in False False"); 
            }
            if (Text == "rnorm")
            {
                HelpText1.Text = "Generates random numbers from a normal distribution (includes fractions) with a specified mean and standard deviation.";
                HelpText1.Text += "\nrnorm(n, mean = 0, sd = 1)";
                HelpText1.Text += "\n\nn: the number of random numbers to generate. To create a random variable in a dataset, n=nrow(Dataset) sets n to the number of rows in the dataset";
                HelpText1.Text += "\n\nmean:the mean of the normal distribution.";
                HelpText1.Text += "\n\nsd:the standard deviation of the normal distribution.";
                HelpText1.Text += "\n\nFor e.g. rnorm(n=100, mean = 25, sd = 40), generates 100 random numbers from a normal distribution with mean 25 and standard deviation 40."; 
                HelpText1.Text += "\n\nTo generate a new column dataset varx that contains random numbers type rnorm(n=nrow(Datasetx) , mean = 0, sd = 1)";
                //HelpText1.Items.Add("Help:"); 
                //HelpText2.Items.Add("and, for e.g. if varx = c(1,2), varx > 3 & varx < 2 results in False False"); 
            }
            if (Text == "Day of Week")
            {
                HelpText1.Text = "Extracts the day of the week e.g. Wednesday from a date class.";
                HelpText1.Text += "\nweekdays(x, abbreviate = FALSE).";
                HelpText1.Text += "\nx: An object of type POSIXt (includes POSIXct and POSIXlt) or Date.";
                HelpText1.Text += "\nabbreviate: controls whether the name of the day should be abbreviated or not.";
                HelpText1.Text += "\nUsage: var2 =weekdays(var1, abbreviate =FALSE)";
            }

            if (Text == "Day of Month")
            {
                HelpText1.Text = "Extracts day of the month e.g. 1-31 from a date class";
                HelpText1.Text += "\nstrftime(x=, format=, tz = \"\") ";
                HelpText1.Text += "\nx: An object of type POSIXt (includes POSIXct and POSIXlt) or Date.";
                HelpText1.Text += "\nformat: defines what needs to get extracted, %e specifies day of month";
                HelpText1.Text += "\ntz: represents the timezone, \"\" is current time zone. ";
                HelpText1.Text += "\nUsage: var2 =strftime(x=var1, format =\"%e\", tz=\"\")";
            }

            if (Text == "Day of Year")
            {
                HelpText1.Text = "Extracts day of year as decimal number (001–366) from a date class.";
                HelpText1.Text += "\nstrftime(x=, format=, tz = \"\") ";
                HelpText1.Text += "\nx: An object of type POSIXt (includes POSIXct and POSIXlt) or Date.";
                HelpText1.Text += "\nformat: defines what needs to get extracted. %j represents day of year";
                HelpText1.Text += "\ntz: represents the timezone, \"\" is current time zone. ";
                HelpText1.Text += "\nUsage: var2 =strftime(x=var1, format =\"%j\", tz=\"\")";
            }


            if (Text == "Week of Year")
            {
                HelpText1.Text = "Extracts week of the year e.g. (00-53) from a date class.";
                HelpText1.Text += "\nstrftime(x=, format=, tz = \"\") ";
                HelpText1.Text += "\nx: An object of type POSIXt (includes POSIXct and POSIXlt) or Date.";
                HelpText1.Text += "\nformat: format: defines what needs to get extracted, %U extracts week of the year";
                HelpText1.Text += "\ntz: represents the timezone, \"\" is current time zone. ";
                HelpText1.Text += "\nUsage: var2 =strftime(x=var1, format =\"%U\", tz=\"\")";
            }

            if (Text == "Month")
            {
                HelpText1.Text = "Extract the month e.g November from a date class.";
                HelpText1.Text += "\nmonths(x, abbreviate = FALSE).";
                HelpText1.Text += "\nx: An object of type POSIXt (includes POSIXct and POSIXlt) or Date.";
                HelpText1.Text += "\nabbreviate: controls whether the name of the month should be abbreviated or not.";
                HelpText1.Text += "\nUsage: var2 =months(var1, abbreviate =FALSE)";
            }

            if (Text == "Month(decimal)")
            {
                HelpText1.Text = "Extracts the month e.g 1-12 from a date class.";
                HelpText1.Text += "\nstrftime(x=, format=, tz = \"\")";
                HelpText1.Text += "\nx: An object of type POSIXt (includes POSIXct and POSIXlt) or Date.";
                HelpText1.Text += "\nformat: format: defines what needs to get extracted, %m extracts month(1-12)";
                HelpText1.Text += "\ntz: represents the timezone, \"\" is current time zone. ";
                HelpText1.Text += "\nUsage: var2 =strftime(x=var1, format =\"%m\", tz=\"\")";
            }

            if (Text == "Day of Week")
            {
                HelpText1.Text = "Extracts the day of the week e.g. Wednesday from a date class.";
                HelpText1.Text += "\nweekdays(x, abbreviate = FALSE).";
                HelpText1.Text += "\nx: An object of type POSIXt (includes POSIXct and POSIXlt) or Date.";
                HelpText1.Text += "\nabbreviate: controls whether the name of the day should be abbreviated or not.";
                HelpText1.Text += "\nUsage: var2 =weekdays(var1, abbreviate =FALSE)";
            }

            if (Text == "Quarters")
            {
                HelpText1.Text = "Extracts the quarter e.g. Q4 from a date class.";
                HelpText1.Text += "\nquarters(x, abbreviate = FALSE). ";
                HelpText1.Text += "\nx: An object of type POSIXt (includes POSIXct and POSIXlt) or Date.";
                HelpText1.Text += "\nabbreviate: controls whether the name of the day should be abbreviated or not.";
                HelpText1.Text += "\nUsage var2=quarters(var1, abbreviate =FALSE)";
            }

            if (Text == "Year(XX)")
            {
                HelpText1.Text = "Extracts Year without century (00–99). On input, values 00 to 68 are prefixed by 20 and 69 to 99 by 19.";
                HelpText1.Text += "\nstrftime(x=, format=, tz = \"\")";
                HelpText1.Text += "\nx: An object of type POSIXt (includes POSIXct and POSIXlt) or Date.";
                HelpText1.Text += "\nformat: defines what needs to get extracted, %y extracts year without century";
                HelpText1.Text += "\ntz: represents the timezone, \"\" is current time zone. ";
                HelpText1.Text += "\nUsage: var2 =strftime(x=var1, format =\"%y\", tz=\"\")";
            }
            if (Text == "Year(XXXX)")
            {
                HelpText1.Text = "Extracts Year with century from a date class.";
                HelpText1.Text += "\nstrftime(x=, format=, tz = \"\")";
                HelpText1.Text += "\nx: An object of type POSIXt (includes POSIXct and POSIXlt) or Date.";
                HelpText1.Text += "\nformat: defines what needs to get extracted, %Y extracts year with century";
                HelpText1.Text += "\ntz: represents the timezone, \"\" is current time zone. ";
                HelpText1.Text += "\nUsage: var2 =strftime(x=var1, format =\"%Y\", tz=\"\")";
            }

            if (Text == "Hour(00-23)")
            {
                HelpText1.Text = "Hours as decimal number (00–23).";
                HelpText1.Text += "\nstrftime(x=, format=, tz = \"\")";
                HelpText1.Text += "\nx: An object of type POSIXt (includes POSIXct and POSIXlt) or Date.";
                HelpText1.Text += "\nformat defines what needs to get extracted, %H extracts hours (00-23)";
                HelpText1.Text += "\ntz: represents the timezone, \"\" is current time zone. ";
                HelpText1.Text += "\nUsage: var2 =strftime(x=var1, format =\"%H\", tz=\"\")";
            }

            if (Text == "Hour(00-12)")
            {
                HelpText1.Text = "Hours as decimal number (00–12).";
                HelpText1.Text += "\nstrftime(x=, format=, tz = \"\")";
                HelpText1.Text += "\nx: An object of type POSIXt (includes POSIXct and POSIXlt) or Date.";
                HelpText1.Text += "\nformat defines what needs to get extracted, %I extracts hours (00-12)";
                HelpText1.Text += "\ntz: represents the timezone, \"\" is current time zone. ";
                HelpText1.Text += "\nUsage: var2 =strftime(x=var1, format =\"%I\", tz=\"\")";
            }

            if (Text == "Minute")
            {
                HelpText1.Text = "Minute as decimal number (00–59) from a date class.";
                HelpText1.Text += "\nstrftime(x=, format=, tz = \"\")";
                HelpText1.Text += "\nx: An object of type POSIXt (includes POSIXct and POSIXlt) or Date.";
                HelpText1.Text += "\nformat: defines what needs to get extracted, %M extracts minutes";
                HelpText1.Text += "\ntz: represents the timezone, \"\" is current time zone. ";
                HelpText1.Text += "\nUsage: var2 =strftime(x=var1, format =\"%M\", tz=\"\")";
            }

            if (Text == "Secs")
            {
                HelpText1.Text = "Extract seconds as decimal number (00–59) from a date class.";
                HelpText1.Text += "\nstrftime(x=, format=, tz = \"\")";
                HelpText1.Text += "\nx: An object of type POSIXt (includes POSIXct and POSIXlt) or Date.";
                HelpText1.Text += "\nformat defines what needs to get extracted, %S extracts seconds.";
                HelpText1.Text += "\ntz: represents the timezone, \"\" is current time zone. ";
                HelpText1.Text += "\nUsage: var2 =strftime(x=var1, format =\"%S\", tz=\"\")";
            }

            if (Text == "Create a date (from string)")
            {
                HelpText1.Text = "Converts character vectors to the date class \"POSIXlt\".";
                HelpText1.Text += "\nYou must specify the format in which the year, month, hour, minute and seconds are specified in the character vector including any character seperators. Any trailing characters are ignored.";
                HelpText1.Text += "\n\nstrptime(x, format, tz = \"\")";
                HelpText1.Text += "\n\nx: A character vector.";
                HelpText1.Text += "\n\ntz: represents the timezone, \"\" is current time zone. ";
                HelpText1.Text += "\n\nTo display the new column in the grid, we must convert it to as.POSIXct";
                HelpText1.Text += "\n\nUsage 1: if dateVar =c(\"11/9/2011 12:00:00\") then strptime(x=dateVar, format=\"%d/%m/%Y %H:%M:%S\", tz = \"\")";
                HelpText1.Text += "\n%p for am/pm is used in conjunction with %I i.e. 12hr format. If %H i.e. 24 hr format is used.%p is ignored";
                HelpText1.Text += "\n\nUsage 2: if dateVar =c(\"11/9/2011 08:00:00PM\") then strptime(x=dateVar, format=\"%d/%m/%Y %I:%M:%S%p\", tz = \"\")";
                HelpText1.Text += "\n\nUsage 3: if dateVar =c(\"11-9-2011\") then strptime(x=dateVar, format=\"%d-%m-%Y\", tz = \"\")";
                HelpText1.Text += "\nCommonly used formats %H -24 hour format, %I 12 hr format, %Y year with century, %y year without century, %d day as a number(1-31), %m month as a number (1-12), %M minute as a number (0-59)";

            }

            if (Text == "Date Difference")
            {
                HelpText1.Text = "Calculate the difference between 2 times.";
                HelpText1.Text += "\ndifftime(time1=, time2=, tz=\"\", units = c(\"auto\", \"secs\", \"mins\", \"hours\", \"days\", \"weeks\"))";
                HelpText1.Text += "\ntime1 and time2 are objects of classes POSIXt (POSIXct, POSIXlt) or Date";
                HelpText1.Text += "\nformat defines what needs to get extracted, %S extracts seconds.";
                HelpText1.Text += "\ntz :an optional time zone specification to be used for the conversion, mainly for \"POSIXlt\" objects.";
                HelpText1.Text += "\nunits: units in which results are desired";
                HelpText1.Text += "\nUsage: var3 =difftime(time1=var1, time2=var2, tz=\"\", units = \"secs\")";
            }

            //05Oct2016 Anil: Added 'numeric to Date' conversion buttons 
            if (Text == "Numeric to date")
            {
                HelpText1.Text = "Convers a numeric column data to Date.";
                HelpText1.Text += "\nas.Date(x, origin=\"1970-01-01\", ...)";
                HelpText1.Text += "\nx is an object to be converted and is numeric type.";
                HelpText1.Text += "\norigin is a Date object, or something which can be coerced by as.Date(origin, ...) to such an object.";
                //HelpText1.Text += "\ntz :an optional time zone specification to be used for the conversion, mainly for \"POSIXlt\" objects.";
                //HelpText1.Text += "\nunits: units in which results are desired";
                HelpText1.Text += "\nUsage: var2 =as.Date(x=var1, origin=\"1970-01-01\")";
            }
            //05Oct2016 Anil: Added string to Date conversion buttons 
            if (Text == "String to date")
            {
                HelpText1.Text = "Convers a string column data to Date.";
                HelpText1.Text += "\nas.Date(x, ...)";
                HelpText1.Text += "\nx is an object to be converted and is a date string type.";
                //HelpText1.Text += "\nformat defines what needs to get extracted, %S extracts seconds.";
                //HelpText1.Text += "\ntz :an optional time zone specification to be used for the conversion, mainly for \"POSIXlt\" objects.";
                //HelpText1.Text += "\nunits: units in which results are desired";
                HelpText1.Text += "\nUsage: var2 =as.Date(x=var1)";
            }

            if (Text == "Extract a Number")
            {
                HelpText1.Text = "Extract the first numeric from a string";
                HelpText1.Text += "\nstr_extract(string,pattern)";
                HelpText1.Text += "\nstring :Either a character vector, or something coercible to one.";
                HelpText1.Text += "\npattern: a string or regular expression";
                HelpText1.Text += "\nUsage: str_extract(string=var1, pattern=\"\\d+\\.*\\d*\")";
            }


            if (Text == "Pad")
            {
                HelpText1.Text = "Pads a string with a character";
                HelpText1.Text += "\nstr_pad(string=, width=, side = c(\"left\", \"right\", \"both\"), pad = \" \")";
                HelpText1.Text += "\nstring :Either a character vector, or something coercible to one.";
                HelpText1.Text += "\nwidth: Minimum width of padded strings.";
                HelpText1.Text += "\nside: Side on which padding character is added (left, right or both).";
                HelpText1.Text += "\npad: Single padding character (default is a space).";

HelpText1.Text += "\nReturns a character vector";
HelpText1.Text +=  "\nUsage:var2 =str_pad(string =var1, width=10, side =\"left\", pad=\"X\")";

            }

            if (Text == "Trim")
            {
                HelpText1.Text = "Trim whitespace from start and end of string.";
                HelpText1.Text += "\nstr_trim(string=, side = c(\"both\", \"left\", \"right\"))";
                HelpText1.Text += "\nstring :Either a character vector, or something coercible to one.";
                HelpText1.Text += "\nside: Side on which padding character is added (left, right or both).";
                HelpText1.Text += "\nUsage:var2 =str_trim(string=var1, side= \"left\")";

            }

            if (Text == "Length")
            {
                HelpText1.Text = "Returns the number of characters in a string.";
            HelpText1.Text +="\nstring: Either a character vector, or something coercible to one.";
               
                HelpText1.Text += "\nUsage: var2=str_length(string=var1)";

            }

            if (Text == "Count(matches)")
            {
                HelpText1.Text = "Count the number of matches to a pattern in a string.";
                HelpText1.Text += "\nstr_count(string=, pattern = \"\")";
                 HelpText1.Text +="\nstring: Either a character vector, or something coercible to one.";
                 HelpText1.Text +="\npattern: Pattern to look for.";
                HelpText1.Text += "\nUsage: var2=str_count(string=var1, pattern=\"abc\")";

            }











        }


    }
}