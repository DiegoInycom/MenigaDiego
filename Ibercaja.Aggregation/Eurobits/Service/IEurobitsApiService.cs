using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace Ibercaja.Aggregation.Eurobits.Service
{
    public interface IEurobitsApiService
    {
        bool ConfigurationIsCorrect { get; }
        Task<JwtAuthResponse> Login();
        Task<RobotDetailsResponse> GetRobotInfo(string robotName);
        Task<ExecutionResponse> NewAggregation(string robotName, string userId, Dictionary<string, string> loginParameters, DateTime? fromDate = null, string[] productsToFetch = null);
        Task<AggregationResponse> GetAggregation(string executionId);
        Task<HttpStatusCode> GetAggregationStatus(string executionId);
        Task<DynamicParam> GetAggregationWaitingParam(string executionId);
        Task UpdateAggregation(string executionId, string parameter);
        Task RemoveAggregation(string executionId);
        Task<AggregationStatusResponse> GetAggregationPagingStatus(string executionId);
    }
}
