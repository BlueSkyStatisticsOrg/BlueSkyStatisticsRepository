using System.Windows;

namespace BlueSky.Windows
{
    /// <summary>
    /// Interaction logic for DatasetLoadingBusyWindow.xaml
    /// </summary>
    public partial class DatasetLoadingBusyWindow : Window
    {
        //string busyMessage;
        //public string BusyMessage
        //{
        //    get;
        //    set { busyMessage = value; }
        //}
        public DatasetLoadingBusyWindow()
        {
            InitializeComponent();
            //Please wait while Dataset is  Loading...
            
            
        }
        public DatasetLoadingBusyWindow(string busymessage) // Sending custom message
        {
            InitializeComponent();
            //Binding b = new Binding(busymessage);
            //label1.SetBinding(Label.ContentProperty, b);
            label1.Content = busymessage;
            //System.Windows.Threading.DispatcherTimer dis = new System.Windows.Threading.DispatcherTimer();
            //dis.Interval = new TimeSpan(1000);
        }

    }
}
