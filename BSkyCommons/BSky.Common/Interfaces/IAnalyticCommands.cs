using System.Collections.Generic;

namespace BSky.Statistics.Common.Interfaces
{
    public interface IAnalyticCommands
    {
        //bilkul bekar interface hai ye
        UAReturn UAOneSample(ServerDataSource dataSource, List<string> vars, double mu, double confidenceLevel, int missing);
        UAReturn UABinomial(ServerDataSource dataSource, List<string> vars, double p, string alternative, double confidenceLevel, bool descriptives, bool quartiles, int missing);

    }
}
