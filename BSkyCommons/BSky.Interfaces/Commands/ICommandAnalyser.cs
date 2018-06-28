using BSky.Interfaces.Model;

namespace BSky.Interfaces.Commands
{
    public interface ICommandAnalyser
    {
        CommandOutput Decode(AnalyticsData data);
    }
}
