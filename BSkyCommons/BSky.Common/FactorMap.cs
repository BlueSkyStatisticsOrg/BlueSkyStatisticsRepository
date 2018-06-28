
namespace BSky.Statistics.Common
{
    public class FactorMap
    {
        private string _labels;
        public string labels
        {
            get
            {
                return _labels;
            }
            set
            {
                _labels = value;
            }
        }

        private string _textbox;
        public string textbox
        {
            get
            {
                return _textbox;
            }
            set
            {
                _textbox = value;
            }
        }

        //15Jan2018 Mod for Joao
        private string _numlevel;
        public string numlevel
        {
            get { return _numlevel; }
            set { _numlevel = value; }
        }

        //For disabling the textbox that contains '<NA>'
        public bool IsEnabled
        {
            get
            {
                if (textbox.Equals("<NA>"))
                {
                    return false;
                }
                else
                    return true;
            }
        }
    }

}
