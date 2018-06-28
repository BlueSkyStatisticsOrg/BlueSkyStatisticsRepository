using System.Xml.Serialization;
using System.IO;
using System.Xml;
using System.Diagnostics;

namespace BSky.Statistics.Common
{
    public interface IServerCommand
    {       
        string Name {get; set;}
        UAReturn Result { get; }
        string CommandSyntax { get; set; }
        CommandDispatcher Dispatcher { get; set; }
        void Execute();
    }

    public class ServerCommand : IServerCommand
    {
        private UAReturn _result;

        private CommandDispatcher _Dispatcher;
        private string _DispatcherClass;
        private string _DispatcherAssembly;
        private string _Name;
        private string _Description;
        private string _CommandSyntax;


        public ServerCommand(CommandDispatcher dispatcher) : this(dispatcher, "", "") { }
        public ServerCommand(CommandDispatcher dispatcher, string name, string sourceCodeScript)
        {
            _Dispatcher = dispatcher;
            _Name = name;
            _CommandSyntax = sourceCodeScript;
            _result = new UAReturn();
        }

        public ILogDevice LogDevice { get; set; }

        public ServerDataSource DataSource { get; set; }

        public CommandDispatcher Dispatcher
        {
            get
            {
                if (_Dispatcher == null && !string.IsNullOrEmpty(_DispatcherClass) )
                {
                    //Lets try to load the command dispatcher
                    //_DispatcherClass = Type.GetType(_DispatcherClass);
                }
                return _Dispatcher;
            }
            set { _Dispatcher = value; _DispatcherClass = _Dispatcher.GetType().FullName; }
        }

        public UAReturn Result { get { return _result; } }
        public string Name
        {
            get { return _Name; }
            set { _Name = value; }
        }

        public string Description
        {
            get { return _Description; }
            set { _Description = value; }
        }
        
        public string CommandSyntax
        {
            get { return _CommandSyntax; }
            set { _CommandSyntax = value; }
        }

        public virtual bool Validate() { return !string.IsNullOrEmpty(this.CommandSyntax) && this.Dispatcher != null; }
        
        public void Execute()
        {
            Debug.Assert(this.Validate());
            _result = this.Dispatcher.Execute(this);
        }


        public object ExecuteR(bool hasReturn, bool hasUAReturn)//ad
        {
            Debug.Assert(this.Validate());
            return this.Dispatcher.ExecuteR(this, hasReturn, hasUAReturn);
        }

        public void JustLogCommandDoNotExecute()//16Aug2016
        {
            Debug.Assert(this.Validate());
            _result = this.Dispatcher.DontExecuteJustLogCommand(this.CommandSyntax);
        }


        public static ServerCommand Load(string fileName)
        {
            ServerCommand command;
            XmlDocument doc = new XmlDocument();

            doc.Load(fileName);
            string typeName = doc.DocumentElement.Name;
            doc = null;

            FileStream stream = new FileStream(fileName, FileMode.Open);
            XmlSerializer serializer = new XmlSerializer(typeof(ServerCommand));

            command = (ServerCommand)serializer.Deserialize(stream);
            stream.Close();
            return command;
        }
        public static void Save(string fileName, ServerCommand command)
        {

            XmlSerializer serializer = new XmlSerializer(typeof(ServerCommand));
            FileStream stream = new FileStream(fileName, FileMode.Create);
            serializer.Serialize(stream, command);
            stream.Close();

        }
    }
}
