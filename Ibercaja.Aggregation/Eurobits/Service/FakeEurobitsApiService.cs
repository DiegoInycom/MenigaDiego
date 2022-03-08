using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Ibercaja.Aggregation.Eurobits.Service
{
    public class FakeSession
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(FakeSession));
        private readonly TimeSpan _sessionLength;
        private readonly Thread _thread;

        public FakeSession(TimeSpan sessionLength, string robotName, string errorCode)
        {
            Filename = GetJsonName(robotName.ToLower().Trim());
            _sessionLength = sessionLength;
            _thread = new Thread(() => PrepareResult(errorCode));
            ExecutionResponse = new ExecutionResponse
            {
                ExecutionId = Guid.NewGuid().ToString()
            };
            Status = HttpStatusCode.Accepted;
        }

        public void Start()
        {
            _thread.Start();
        }

        void PrepareResult(string errorCode)
        {
            Started = DateTime.Now;
            Thread.Sleep(_sessionLength);
            if (string.IsNullOrEmpty(errorCode))
            {
                AggregationResponse = GetAggregationFromFile();
            }
            else
            {
                AggregationResponse = new AggregationResponse
                {
                    AggregationInfo = new AggregationInfo
                    {
                        Code = errorCode,
                        Message = "check Eurobits error codes for more information"
                    }
                };
            }
            Ended = DateTime.Now;
            Status = HttpStatusCode.OK;
        }

        public AggregationResponse AggregationResponse { get; private set; }
        public ExecutionResponse ExecutionResponse { get; }
        public DateTime Started { get; private set; }
        public DateTime? Ended { get; private set; }
        public string Filename { get; }
        public HttpStatusCode Status { get; private set; }

        private string GetJsonName(string value)
        {
            switch (value)
            {
                case "ibercaja":
                    return "2.ibercaja.json";
                case "bbva":
                    return "8.bbva.json";
                case "caixabank":
                    return "9.caixabank.json";
                case "kutxabank":
                    return "10.kutxabank.json";
                case "abanca":
                    return "11.abanca.json";
                case "liberbank":
                    return "12.liberbank.json";
                case "caja laboral":
                    return "15.cajalaboral.json";
                case "bankia":
                    return "17.bankia.json";
                case "bankinter":
                    return "21.bankinter.json";
                case "ing direct gnoma":
                    return "23.ingdirect.json";
                case "banc sabadell":
                    return "32.bancsabadell.json";
                case "santander":
                    return "33.santander.json";
                case "unicaja":
                    return "35.unicaja.json";
                case "ruralvia":
                    return "42.ruralvia.json";
                case "imaginbank":
                    return "62.imaginBank.json";
                case "demo":
                    return "17.bankia.json";

                default:
                    throw new Exception($"Json file for {value} is missing");
            }
        }

        private AggregationResponse GetAggregationFromFile()
        {
            var fileLocation = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"bin\\Eurobits\\Files\\{Filename}");
            Logger.Debug($"Loading file {fileLocation}");
            using (StreamReader r = new StreamReader(fileLocation))
            {
                string json = r.ReadToEnd();
                return JsonConvert.DeserializeObject<AggregationResponse>(json);
            }
        }

    }

    public class FakeEurobitsApiService : IEurobitsApiService
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(FakeEurobitsApiService));
        private static readonly ConcurrentDictionary<string, FakeSession> Aggregations = new ConcurrentDictionary<string, FakeSession>();

        public bool ConfigurationIsCorrect => true;

        public async Task<JwtAuthResponse> Login()
        {
            return new JwtAuthResponse
            {
                Token = Guid.NewGuid().ToString(),
                Message = string.Empty
            };
        }

        public async Task<RobotDetailsResponse> GetRobotInfo(string robotName)
        {
            if (robotName != "ING Direct Gnoma")
            {
                return new RobotDetailsResponse
                {
                    GlobalParameters = new GlobalParameters
                    {
                        Params = new Param[]
                        {
                            new Param
                            {
                                Name = "user",
                                Description = "User",
                                Required = true,
                                Encoded = true
                            },
                            new Param
                            {
                                Name = "password",
                                Description = "Password",
                                Required = true,
                                Encoded = true
                            }
                        }
                    }
                };
            }
            else
            {
                return new RobotDetailsResponse
                {
                    GlobalParameters = new GlobalParameters
                    {
                        Params = new Param[]
                        {
                            new Param
                            {
                                Name = "id",
                                Description = "ID",
                                Required = true,
                                Encoded = true
                            },
                            new Param
                            {
                                Name = "birthDay",
                                Description = "BirthDay",
                                Required = true,
                                Encoded = true
                            },
                            new Param
                            {
                                Name = "birthMonth",
                                Description = "BirthMonth",
                                Required = true,
                                Encoded = true
                            },
                            new Param
                            {
                                Name = "birthYear",
                                Description = "BirthYear",
                                Required = true,
                                Encoded = true
                            },
                            new Param
                            {
                                Name = "password",
                                Description = "Password",
                                Required = true,
                                Encoded = true
                            }
                        }
                    }
                };
            }
        }

        public async Task<ExecutionResponse> NewAggregation(string robotName,
                                                            string userId,
                                                            Dictionary<string, string> loginParameters,
                                                            DateTime? fromDateNullable = null,
                                                            string[] productsToFetch = null)
        {
            Logger.Debug($"New aggregation invoked with parameters: {nameof(robotName)}:{robotName}, {nameof(userId)}:{userId}");

            var parametersEncrypted = !loginParameters.Any(p => p.Value.Length != 344);
            var errorCode = parametersEncrypted ? string.Empty : "R021";

            var newSession = new FakeSession(TimeSpan.FromSeconds(2), robotName, errorCode);

            if (!Aggregations.TryAdd(newSession.ExecutionResponse.ExecutionId, newSession))
            {
                Logger.Warn("Multiple aggregation has been started exactly in same time !");
            }
            newSession.Start();
            return newSession.ExecutionResponse;
        }

        public async Task<AggregationResponse> GetAggregation(string executionId)
        {
            Logger.Debug($"GetAggregation invoked with {nameof(executionId)}:{executionId}");
            if (string.IsNullOrEmpty(executionId))
            {
                return new AggregationResponse
                {
                    AggregationInfo = new AggregationInfo
                    {
                        Code = "500",
                        Message = "executionId is empty"
                    }
                };
            }

            FakeSession session;
            if (Aggregations.TryGetValue(executionId, out session))
            {
                return session.AggregationResponse;
            }

            Logger.Debug($"ExecutionId {executionId} not started");
            return new AggregationResponse
            {
                AggregationInfo = new AggregationInfo
                {
                    Code = "500",
                    Message = $"executionId {executionId} not found"
                }
            };
        }

        public async Task<HttpStatusCode> GetAggregationStatus(string executionId)
        {
            Logger.Debug($"GetAggregationStatus invoked with {nameof(executionId)}:{executionId}");
            FakeSession session;
            if (executionId != null && Aggregations.TryGetValue(executionId, out session))
            {
                if (!session.Ended.HasValue)
                    Logger.Debug($"Simulating delay for aggregation retrieval: {executionId}: time passed: {DateTime.Now - session.Started}");

                return session.Status;
            }

            return HttpStatusCode.BadRequest;
        }

        public async Task<DynamicParam> GetAggregationWaitingParam(string executionId)
        {
            return null;
        }

        public async Task UpdateAggregation(string executionId, string pwdVble)
        {
        }

        public async Task RemoveAggregation(string executionId)
        {
        }

        public async Task<AggregationStatusResponse> GetAggregationPagingStatus(string executionId)
        {
            return new AggregationStatusResponse
            {
                Accounts = new Status
                {
                    ItemsFound = 1,
                    ItemsCompleted = 1
                }
            };
        }
    }
}

