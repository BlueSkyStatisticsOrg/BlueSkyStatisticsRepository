using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using AnalyticsUnlimited.Statistics.Common;

namespace AnalyticsUnlimited.Service.Engine
{
    [ServiceContract]
    public interface IAnalyticsService
    {
        [OperationContract]
        UAReturn Execute(CommandRequest cmd);

        [OperationContract]
        UAReturn DataSourceLoad(string datasetName, string fileName);

        [OperationContract]
        UAReturn DataSourceReadRows(string datasetName, int startRow, int endRow);

        [OperationContract]
        UAReturn DataSourceReadCell(string datasetName, int rowIndex, int colIndex);

        [OperationContract]
        UAReturn OneSample(string datasetName, List<string> vars, double mu, double confidenceLevel, int missing);

        [OperationContract]
        UAReturn Binomial(string datasetName, List<string> vars, double p, string alternative, double confidenceLevel, bool descriptives, bool quartiles, int missing);

    }
}
