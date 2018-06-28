using System;
using System.Windows.Input;

namespace BSky.Interfaces.Commands
{
    public abstract class AUCommandBase : ICommand
    {
        #region ICommand Members

        public bool CanExecute(object parameter)
        {
                return true;
        }

        protected string WrapInputVariable(string key, string val)
        {
            return string.Format(@"<Variable key=""{0}"">{1}</Variable>", key, val);
        }
        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            if (System.Windows.Input.Mouse.OverrideCursor == System.Windows.Input.Cursors.Wait)//29Sep2015 disable if mouse is busy
                return;
            else
            {
                OnPreExecute(parameter);
                OnExecute(parameter);
                OnPostExecute(parameter);
            }
        }

        protected abstract void OnPreExecute(object param);

        protected abstract void OnExecute(object param);

        protected abstract void OnPostExecute(object param);

        protected abstract void SendToOutputWindow(string title, string command, bool isCommand);//13Dec2013\

        #endregion
    }
}
