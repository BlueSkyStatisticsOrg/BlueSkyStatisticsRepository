using BSky.ConfService.Intf.Interfaces;
using BSky.Interfaces.Commands;
using BSky.Interfaces.Controls;
using BSky.Interfaces.Model;
using BSky.Lifetime;
using BSky.Lifetime.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BSky.Controls.Controls
{
    /// <summary>
    /// Interaction logic for BSkyOutputOptionsToolbar.xaml
    /// </summary>
    public partial class BSkyOutputOptionsToolbar : UserControl, IAUControl
    {
        ILoggerService logService = LifetimeService.Instance.Container.Resolve<ILoggerService>();
        IConfigService confService = LifetimeService.Instance.Container.Resolve<IConfigService>();

        public BSkyOutputOptionsToolbar()
        {
            InitializeComponent();
            //commentBoxvisibility = Visibility.Collapsed;
            
            this.DataContext = toolbarData;
            delimage.Tag = this;//reference of this class in image tag for quick access to this class.
        }

        #region IAUControl members
        public string ControlType
        {
            get { return "Toolbar";  }
            set { }
        }
        public string NodeText
        {
            get;
            set;
        }

        public Thickness outerborderthickness
        {
            get{ return outerborder.BorderThickness; }
            set{ outerborder.BorderThickness = value; }
        }

        public SolidColorBrush controlsselectedcolor
        {
            get;
            set;
        }

        public SolidColorBrush controlsmouseovercolor
        {
            get;
            set;
        }

        public SolidColorBrush bordercolor
        {
            get{ return (SolidColorBrush)outerborder.BorderBrush; }
            set{ outerborder.BorderBrush = value; }
        }

        //To set visiblity in output window
        public System.Windows.Visibility BSkyControlVisibility
        {
            get { return this.Visibility; }
            set { this.Visibility = value; }
        }

        public bool DeleteControl { get; set; }
        #endregion

        OutputToolbarData toolbarData = new OutputToolbarData();
        public OutputToolbarData ToolbarData
        {
            get { return toolbarData; }

        }
        //OutputWindow ow;//holds the reference of the output windows in which this controls is.

        private CommandOutput analysisOutput;//holds the reference of the part of the analysis output
        public CommandOutput AnalysisOutput
        {
            get { return analysisOutput; }
            set { analysisOutput = value; }
        }

        private AnalyticsData _analysis;//holds the reference of the analysis output
        public AnalyticsData Analysis
        {
            get { return _analysis; }
            set { _analysis = value; }

        }

        #region click event
        private void AddEditCommentBtn_Click(object sender, RoutedEventArgs e)
        {
            if (commentTxt.Visibility == Visibility.Collapsed || commentTxt.Visibility == Visibility.Hidden)
            {
                commentTxt.Visibility = Visibility.Visible;
                cmntlbl.Visibility = Visibility.Visible;
            }
            else
            {
                commentTxt.Visibility = Visibility.Collapsed;
                cmntlbl.Visibility = Visibility.Collapsed;
            }
        }

        private void ReRunBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void CutBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void DelBtn_Click(object sender, RoutedEventArgs e)
        {

        }


        private void toolbar_MouseEnter(object sender, MouseEventArgs e)
        {
            string mousehovercol = confService.GetConfigValueForKey("outputmousehovercol");
            byte red = byte.Parse(mousehovercol.Substring(3, 2), NumberStyles.HexNumber);
            byte green = byte.Parse(mousehovercol.Substring(5, 2), NumberStyles.HexNumber);
            byte blue = byte.Parse(mousehovercol.Substring(7, 2), NumberStyles.HexNumber);
            Color c = Color.FromArgb(255, red, green, blue);
            //outerborder.BorderThickness = new Thickness(1);
            controlsselectedcolor = (SolidColorBrush)outerborder.BorderBrush;//storing current
            outerborder.BorderBrush = new SolidColorBrush(c);//Colors.DarkOrange);
        }

        private void toolbar_MouseLeave(object sender, MouseEventArgs e)
        {
            // outerborder.BorderThickness = new Thickness(0);
            outerborder.BorderBrush = controlsselectedcolor;// new SolidColorBrush(Colors.Transparent);
        }

        #endregion
    }

    public class OutputToolbarData : INotifyPropertyChanged
    {
        private string _comment=string.Empty;

        public string Comment
        {
            get { return _comment; }
            set
            {
                _comment = value;
                // Call OnPropertyChanged whenever the property is updated
                OnPropertyChanged("Comment");
            }
        }

        private string _dialogXAML;

        public string DialogXAML
        {
            get { return _dialogXAML; }
            set
            {
                _dialogXAML = value;
                // Call OnPropertyChanged whenever the property is updated
                OnPropertyChanged("DialogXAML");
            }
        }

        private string _syntaxExecuted;

        public string SyntaxExecuted
        {
            get { return _syntaxExecuted; }
            set
            {
                _syntaxExecuted = value;
                // Call OnPropertyChanged whenever the property is updated
                OnPropertyChanged("SyntaxExecuted");
            }
        }

        //private AnalyticsData _analysis;

        //public AnalyticsData Analysis
        //{
        //    get { return _analysis; }
        //    set { _analysis = value; }

        //}



        #region INotifyPropertyChanged members
        public event PropertyChangedEventHandler PropertyChanged;

        // Create the OnPropertyChanged method to raise the event
        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }
        #endregion

    }
}
