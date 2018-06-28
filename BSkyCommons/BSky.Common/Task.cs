
namespace BSky.Statistics.Common
{
    public enum TaskPriority : uint { High, Normal, Low, Background }
    public class Task
    {
        public TaskPriority Priority { get; set; }
        public IServerCommand Command { get; private set; }

    }
}
