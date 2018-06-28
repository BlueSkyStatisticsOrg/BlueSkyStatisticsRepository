using System;
using System.Windows.Input;

namespace BSky.Controls.Commands
{
    public class DialogDesignerCommand: ICommand
    {
        #region ICommand Members

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            
        }

        #endregion
    }
}
