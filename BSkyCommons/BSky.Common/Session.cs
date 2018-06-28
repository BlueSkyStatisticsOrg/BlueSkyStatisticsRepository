
namespace BSky.Statistics.Common
{
    public class Session
    {
        public CommandDispatcher DefaultDispatcher { get; private set;}
                
        public Session(CommandDispatcher dispatcher)
        {
            DefaultDispatcher = dispatcher;
        }

        public ServerCommand CreateAdhocCommand(string name, string sourceSyntax)
        {
            return new ServerCommand(this.DefaultDispatcher, name, sourceSyntax);
        }
    }
}
