using RDotNet.Devices;
using BSky.Statistics.Common;

namespace BSky.Statistics.R
{

    public class RDotNetConsoleLogDevice : ICharacterDevice
    {
        public RDotNetConsoleLogDevice() { }
        public string LastError { get; private set; }
        public ILogDevice LogDevice { get; set; }

        #region RDonNet interface members
        public RDotNet.SymbolicExpression AddHistory(RDotNet.Language call, RDotNet.SymbolicExpression operation, RDotNet.Pairlist args, RDotNet.REnvironment environment)
        {
            return null;// (new RDotNet.SymbolicExpression());
        }

        public RDotNet.Internals.YesNoCancel Ask(string question)
        {
            return RDotNet.Internals.YesNoCancel.Yes;
        }

        public void Busy(RDotNet.Internals.BusyType which)
        {

        }

        public void Callback()
        {

        }

        public string ChooseFile(bool create)
        {
            return "./rdotnet.txt";
        }

        public void CleanUp(RDotNet.Internals.StartupSaveAction saveAction, int status, bool runLast)
        {
            LastError = string.Empty;
        }

        public void ClearErrorConsole()
        {
            LastError = string.Empty;
        }

        public void EditFile(string file)
        {
            return;
        }

        public void FlushConsole()
        {
            return;
        }

        public RDotNet.SymbolicExpression LoadHistory(RDotNet.Language call, RDotNet.SymbolicExpression operation, RDotNet.Pairlist args, RDotNet.REnvironment environment)
        {
            return null; // (new RDotNet.SymbolicExpression());
        }

        public string ReadConsole(string prompt, int capacity, bool history)
        {
            return "Console Text is:";
        }

        public void ResetConsole()
        {
            return;
        }

        public RDotNet.SymbolicExpression SaveHistory(RDotNet.Language call, RDotNet.SymbolicExpression operation, RDotNet.Pairlist args, RDotNet.REnvironment environment)
        {
            return null;// (new RDotNet.SymbolicExpression());
        }

        public bool ShowFiles(string[] files, string[] headers, string title, bool delete, string pager)
        {
            return false;
        }

        public void ShowMessage(string message)
        {
            LogDevice.WriteLine(message);
        }

        public void Suicide(string message)
        {
            return;
        }

        public void WriteConsole(string output, int length, RDotNet.Internals.ConsoleOutputType outputType)
        {

            if (LogDevice != null) LogDevice.WriteLine(output);
            LastError = output;

        }
        #endregion
    }


}
