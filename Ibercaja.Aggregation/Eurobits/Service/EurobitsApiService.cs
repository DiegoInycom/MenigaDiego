using log4net;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Ibercaja.Aggregation.Eurobits.Service
{
    public class EurobitsApiService : IEurobitsApiService
    {
        private const string customer = "ibercaja";

        private readonly HttpClient _client;
        private readonly JwtAuthRequest _authRequest;
        private readonly string _certificateAlias;

        public bool ConfigurationIsCorrect { get; private set; } = true;

        private static readonly ILog Logger = LogManager.GetLogger(typeof(EurobitsApiService));

        public EurobitsApiService(string baseAddress, string certificateAlias, string eurobitsApiServiceId, string eurobitsApiPassword)
        {
            _client = HttpClientFactory.Create(new DelegatingHandler[] { new AuthenticationHandler(), new LoggingHandler() });
            _client.BaseAddress = new Uri(baseAddress);
            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _certificateAlias = certificateAlias;
            _authRequest = new JwtAuthRequest { Password = eurobitsApiPassword, Service = eurobitsApiServiceId };
            
        }

        public async Task<JwtAuthResponse> Login()
        {
            var uri = new Uri(_client.BaseAddress, $"{customer}/privateWS/api/login");
            using (var response = await Request(HttpMethod.Post, uri, _authRequest).ConfigureAwait(false))
            {
                if (response.IsSuccessStatusCode)
                {
                    var authResponse = await response.Content.ReadAsAsync<JwtAuthResponse>();
                    _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authResponse.Token);
                    return authResponse;
                }
                ConfigurationIsCorrect = false;
                var error = await response.Content.ReadAsAsync<ErrorResponse>().ConfigureAwait(false);
                Logger.Error($"Eurobits service is not available: {error.Message}");
                Logger.Debug($"Eurobits service is not available: {error.DeveloperMessage}");
                return new JwtAuthResponse
                {
                    Token = string.Empty,
                    Message = error.Message
                };
            }
        }

        public async Task<RobotDetailsResponse> GetRobotInfo(string robotName)
        {
            var uri = new Uri(_client.BaseAddress, Uri.EscapeUriString($"{customer}/privateWS/api/robot/robotName/{robotName}"));
            using (var response = await Request(HttpMethod.Get, uri).ConfigureAwait(false))
            {
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsAsync<RobotDetailsResponse>().ConfigureAwait(false);
                }

                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    // Eurobits issue make sometimes robotInfo endpoint fails
                    return null;
                }

                var error = await response.Content.ReadAsAsync<ErrorResponse>().ConfigureAwait(false);
                throw HttpListenerException(error, response.StatusCode.ToString());
            }
        }

        public async Task<ExecutionResponse> NewAggregation(string robotName,
                                                            string userId,
                                                            Dictionary<string, string> loginParameters,
                                                            DateTime? fromDateNullable = null,
                                                            string[] productsToFetch = null)
        {
            var fromDate = DateTime.Today.AddMonths(-6);
            var toDate = DateTime.Today;

            if (fromDateNullable.HasValue)
            {
                fromDate = fromDateNullable.Value;
            }

            if (toDate < fromDate)
            {
                toDate = fromDate;
            }

            if (productsToFetch == null)
            {
                productsToFetch = new string[] { };
            }

            Logger.Debug($"Eurobits New Aggregation request from {fromDate.ToString("dd/MM/yyyy")} to {toDate.ToString("dd/MM/yyyy")}");
            string[] productNames = null;

            var robotInfo = await GetRobotInfo(robotName).ConfigureAwait(false);
            var products = robotInfo.Products;
            productNames = products.AsEnumerable()
                    .Select(p => p.Name)
                    .Intersect(productsToFetch)
                    .ToArray();

            var aggregationRequest = new ExecutionRequest
            {
                RobotName = robotName,
                UserId = userId,
                Products = productNames,
                LoginParameters = loginParameters,
                FromDate = fromDate.ToString("dd/MM/yyyy"),
                ToDate = toDate.ToString("dd/MM/yyyy"),
                ExtendedTrxData = true,
                EncryptedCredentials = true,
                CertificateId = _certificateAlias
            };

            var uri = new Uri(_client.BaseAddress, $"{customer}/privateWS/api/aggregation");
            using (var response = await Request(HttpMethod.Post, uri, aggregationRequest).ConfigureAwait(false))
            {
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsAsync<ExecutionResponse>().ConfigureAwait(false);
                }

                var error = await response.Content.ReadAsAsync<ErrorResponse>().ConfigureAwait(false);
                if (!error.HaveData())
                {
                    return null;
                }

                string parametersSended = null;
                if (aggregationRequest.LoginParameters != null && aggregationRequest.LoginParameters.Any())
                {
                    parametersSended = string.Empty;
                    foreach (var p in aggregationRequest.LoginParameters) parametersSended = $"{parametersSended}{p.Key} ";
                }
                Logger.Error(
                    $"Eurobits Bad Request to robot {aggregationRequest.RobotName} for person {aggregationRequest.UserId} with certificate {aggregationRequest.CertificateId} and loginParameters {parametersSended ?? "empty"}",
                    HttpListenerException(error, response.StatusCode.ToString()));
                Logger.Error($"Eurobits Bad Request: {error.DeveloperMessage}");
                return null;
            }
        }

        public virtual async Task<AggregationResponse> GetAggregation(string executionId)
        {
            var uri = new Uri(_client.BaseAddress, $"{customer}/privateWS/api/aggregation/{executionId}");
            using (var response = await Request(HttpMethod.Get, uri).ConfigureAwait(false))
            {
                if (response.IsSuccessStatusCode || response.StatusCode == (HttpStatusCode)423)
                {
                    return await response.Content.ReadAsAsync<AggregationResponse>().ConfigureAwait(false);
                }

                var errorResponse = new AggregationResponse
                {
                    AggregationInfo = new AggregationInfo
                    {
                        Code = string.Empty
                    }
                };

                var error = await response.Content.ReadAsAsync<ErrorResponse>().ConfigureAwait(false);
                if (error.HaveData())
                {
                    Logger.Error($"Eurobits error {error.Code} ({error.Message}) on executionId {executionId}: {error.DeveloperMessage}");
                    errorResponse.AggregationInfo.Code = error.Code;
                    errorResponse.AggregationInfo.Message = error.Message;
                }
                return errorResponse;
            }
        }
        public async Task<HttpStatusCode> GetAggregationStatus(string executionId)
        {
            var uri = new Uri(_client.BaseAddress, $"{customer}/privateWS/api/aggregation/{executionId}");
            using (var response = await Request(HttpMethod.Head, uri).ConfigureAwait(false))
            {
                return response.StatusCode;
            }
        }

        public async Task<DynamicParam> GetAggregationWaitingParam(string executionId)
        {
            var uri = new Uri(_client.BaseAddress, $"{customer}/privateWS/api/aggregation/{executionId}");
            using (var response = await Request(HttpMethod.Get, uri).ConfigureAwait(false))
            {
                if (response.StatusCode.Equals(HttpStatusCode.Conflict))
                {
                    return await response.Content.ReadAsAsync<DynamicParam>().ConfigureAwait(false);
                }

                var error = await response.Content.ReadAsAsync<ErrorResponse>().ConfigureAwait(false);
                if (error.HaveData())
                {
                    throw HttpListenerException(error, response.StatusCode.ToString());
                }
                return null;
            }
        }

        public async Task UpdateAggregation(string executionId, string parameter)
        {
            var uri = new Uri(_client.BaseAddress, $"{customer}/privateWS/api/aggregation/{executionId}");
            var updateAggregationRequest = new UpdateAggregationRequest { Parameter = parameter };
            using (var response = await Request(HttpMethod.Put, uri, updateAggregationRequest).ConfigureAwait(false))
            {
                if (response.IsSuccessStatusCode)
                {
                    return;
                }

                var error = await response.Content.ReadAsAsync<ErrorResponse>().ConfigureAwait(false);
                if (error.HaveData())
                {
                    throw HttpListenerException(error, response.StatusCode.ToString());
                }
            }
        }

        public async Task RemoveAggregation(string executionId)
        {
            var uri = new Uri(_client.BaseAddress, $"{customer}/privateWS/api/aggregation/{executionId}");
            using (HttpResponseMessage response = await Request(HttpMethod.Delete, uri).ConfigureAwait(false))
            {
                if (response.IsSuccessStatusCode)
                {
                    return;
                }

                var error = await response.Content.ReadAsAsync<ErrorResponse>();
                if (error.HaveData())
                {
                    throw HttpListenerException(error, response.StatusCode.ToString());
                }
            }
        }

        public async Task<AggregationStatusResponse> GetAggregationPagingStatus(string executionId)
        {
            var uri = new Uri(_client.BaseAddress, $"{customer}/privateWS/api/aggregation/{executionId}/pagingStatus");
            using (var response = await Request(HttpMethod.Get, uri).ConfigureAwait(false))
            {
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsAsync<AggregationStatusResponse>().ConfigureAwait(false);
                }

                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    // NOT FOUND: The robot is in an invalid state to check paging status
                    return new AggregationStatusResponse();
                }

                var error = await response.Content.ReadAsAsync<ErrorResponse>().ConfigureAwait(false);
                if (!error.HaveData())
                {
                    throw HttpListenerException(error, response.StatusCode.ToString());
                }
                return null;
            }
        }


        private async Task<HttpResponseMessage> Request(HttpMethod method, Uri uri, IRequest parameter = null)
        {

            HttpResponseMessage response;
            try
            {
                switch (method.Method)
                {
                    case "POST":
                        response = await _client.PostAsJsonAsync(uri, parameter).ConfigureAwait(false);
                        break;
                    case "PUT":
                        response = await _client.PutAsJsonAsync(uri, parameter).ConfigureAwait(false);
                        break;
                    case "HEAD":
                        var request = new HttpRequestMessage(HttpMethod.Head, uri);
                        response = await _client.SendAsync(request).ConfigureAwait(false);
                        break;
                    case "DELETE":
                        response = await _client.DeleteAsync(uri).ConfigureAwait(false);
                        break;
                    case "GET":
                    default:
                        response = await _client.GetAsync(uri).ConfigureAwait(false);
                        break;
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Failed to contact Eurobits", ex);
                throw;
            }

            return response;
        }


        private static HttpListenerException HttpListenerException(ErrorResponse error, string statusCode)
        {
            if (string.IsNullOrEmpty(error.Code))
            {
                error.Code = statusCode;
            }
            return new HttpListenerException(int.Parse(error.Code), error.Message);
        }
    }
}

