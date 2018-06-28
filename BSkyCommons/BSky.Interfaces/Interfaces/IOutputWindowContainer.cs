
namespace BSky.Interfaces.Interfaces
{
    public interface IOutputWindowContainer
    {
        void AddOutputWindow(IOutputWindow iow);

        void RemoveOutputWindow(string Windowname);

        void SetActiveOuputWindow(string Windowname);

        IOutputWindow GetOuputWindow(string Windowname);

        IOutputWindow ActiveOutputWindow
        {
            get;
            set;
        }
    }
}
