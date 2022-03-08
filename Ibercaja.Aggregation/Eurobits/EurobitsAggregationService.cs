using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net;
using System.Threading;
using log4net;
using Meniga.Core.BusinessModels;
using Ibercaja.Aggregation.Eurobits.Service;
using Ibercaja.Aggregation.UserDataConnector.Configuration;

namespace Ibercaja.Aggregation.Eurobits
{
    public class EurobitsAggregationService : IAggregationService
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(EurobitsAggregationService));

        private readonly int _sleepTimer = 1000;
        private readonly IEurobitsApiService _api;
        private readonly string _robotName;
        private readonly string _userId;
        private readonly string _userIdentifier;
        private readonly string[] _products;
        private string _executionId;
        private AggregationResponse _aggregation;

        public EurobitsAggregationService(IEurobitsApiService eurobitsApi, string userId, UserDataConnectorConfigurationRealm configuration)
        {
            _api = eurobitsApi;
            _robotName = configuration.Bank;
            _userId = userId;
            _userIdentifier = configuration.UserIdentifier;
            _products = configuration.ProductsToFetch;
        }

        public void SetSessionToken(string token)
        {
            _executionId = token;

            if (!string.IsNullOrEmpty(_executionId))
            {
                var status = _api.GetAggregationStatus(_executionId).Result;
                if (!status.Equals(HttpStatusCode.Accepted))
                {
                    _aggregation = _api.GetAggregation(_executionId).Result;
                }
            }
        }

        public string GetSessionToken()
        {
            return _executionId;
        }

        public void Clear()
        {
            _executionId = string.Empty;
            _aggregation = null;
        }

        public AggregationStatus ExecuteAggregation(Parameter[] parameters, DateTime from)
        {
            if (!_api.ConfigurationIsCorrect)
            {
                return AggregationStatus.Error;
            }

            dynamic loginParameters = new ExpandoObject();
            var dictionary = new Dictionary<string, string>();
            Array.ForEach(parameters, p => dictionary.Add(p.Name, p.Value));

            ExecutionResponse execution = _api.NewAggregation(_robotName, _userId, dictionary, from, _products).Result;
            _executionId = execution?.ExecutionId;
            if (!string.IsNullOrEmpty(_executionId))
            {
                Logger.Info($"Eurobits service started for person: {_userId} with execId: {_executionId}, robotName: {_robotName}, fromDate: {from.ToString("dd/MM/yyyy")}");
            }
            return GetAggregationStatus();
        }

        public AggregationStatus GetAggregationStatus()
        {
            HttpStatusCode status = HttpStatusCode.BadRequest;
            if (!string.IsNullOrEmpty(_executionId))
            {
                status = _api.GetAggregationStatus(_executionId).Result;
            }
            switch (status)
            {
                case HttpStatusCode.OK:
                    _aggregation = _api.GetAggregation(_executionId).Result;
                    if (_aggregation.AggregationInfo.Code == "R000")
                    {
                        Logger.Info($"Eurobits service finished for person: {_userId} and robotName: {_robotName} with execId: {_executionId}, code: {_aggregation.AggregationInfo.Code}");
                        return AggregationStatus.Finished;
                    }
                    Logger.Error($"Eurobits service finished for person: {_userId} and robotName: {_robotName} with execId: {_executionId}, code: {_aggregation.AggregationInfo.Code} - {_aggregation.AggregationInfo.Message}");
                    return AggregationStatus.Error;
                case HttpStatusCode.Accepted:
                    var pagingStatus = _api.GetAggregationPagingStatus(_executionId).Result;
                    if (pagingStatus != null)
                    {
                        return pagingStatus.Started() ? AggregationStatus.InProgress : AggregationStatus.Login;
                    }
                    return AggregationStatus.Login;
                case HttpStatusCode.BadRequest:
                    return AggregationStatus.Error;
                case (HttpStatusCode)423:
                    _aggregation = _api.GetAggregation(_executionId).Result;
                    Logger.Error($"Eurobits service finished for person: {_userId} and robotName: {_robotName} with execId: {_executionId}, code: {_aggregation.AggregationInfo.Code} - {_aggregation.AggregationInfo.Message}");
                    return AggregationStatus.Error;
                case HttpStatusCode.NotFound:
                    Logger.Error($"Eurobits execId {_executionId} not found");
                    return AggregationStatus.Error;
                case HttpStatusCode.Conflict:
                    return AggregationStatus.SecondPhase;
                default:
                    Logger.Error($"Eurobits service failed with unknown status code: {status}");
                    return AggregationStatus.Error;
            }
        }

        public IEnumerable<ParameterDescription> GetRequiredParameters()
        {
            var robotInfo = _api.GetRobotInfo(_robotName).Result;
            if (robotInfo != null)
            {
                var robotRequiredParameters = robotInfo.GlobalParameters.Params;
                return robotRequiredParameters
                    .Where(a => a.Required)
                    .Select(a => new ParameterDescription
                    {
                        CanSave = true,
                        Name = a.Name,
                        DisplayName = a.Description,
                        IsIdentity = a.Name.Equals(_userIdentifier),
                        IsPassword = a.Name.Equals("password"),
                        IsEncrypted = a.Encoded
                    });
            }
            
            return Enumerable.Empty<ParameterDescription>();
        }

        public SecondPhaseParameter GetSecondPhaseParameter()
        {
            var waitingParam = _api.GetAggregationWaitingParam(_executionId).Result;

            ChallengeContentType type;
            switch (waitingParam.Type)
            {
                case "TEXT":
                    type = ChallengeContentType.TEXT;
                    break;
                case "IMAGE":
                    type = ChallengeContentType.JPG;
                    break;
                default:
                    type = ChallengeContentType.NONE;
                    break;
            }
            return new SecondPhaseParameter
            {
                Parameter = new ParameterDescription
                {
                    CanSave = false,
                    Name = waitingParam.Name,
                    DisplayName = waitingParam.Description,
                    IsEncrypted = GetRequiredParameters().Any(p => p.IsEncrypted)
                },
                ContentType = type,
                TextChallenge = type == ChallengeContentType.TEXT ? waitingParam.Value : null,
                BinaryChallenge = type == ChallengeContentType.JPG ? Base64UrlEncodedString2Bytes(waitingParam.Value) : null
            };
        }

        private static byte[] Base64UrlEncodedString2Bytes(string base64UrlEncodedString)
        {
            var base64String = WebUtility.UrlDecode(base64UrlEncodedString);
            return Convert.FromBase64String(base64String);
        }

        public bool PutSecondPhaseParameter(Parameter parameter)
        {
            var waitingParam = _api.GetAggregationWaitingParam(_executionId).Result;

            if (waitingParam.Name == parameter.Name)
            {
                _api.UpdateAggregation(_executionId, parameter.Value);
                return true;
            }

            return false;
        }

        private AggregationResponse GetAggregation()
        {
            if (_aggregation == null)
            {
                var status = _api.GetAggregationStatus(_executionId).Result;
                while (status.Equals(HttpStatusCode.Accepted))
                {
                    Thread.Sleep(_sleepTimer);
                    status = _api.GetAggregationStatus(_executionId).Result;
                }

                _aggregation = _api.GetAggregation(_executionId).Result;
            }

            return _aggregation;
        }

        public IEnumerable<Account> GetAccounts()
        {
            return GetAggregation()?.Accounts ?? Enumerable.Empty<Account>();
        }

        public IEnumerable<AccountHolder> GetAccountHolders()
        {
            return GetAggregation()?.AccountHolders ?? Enumerable.Empty<AccountHolder>();
        }

        public IEnumerable<DebitCard> GetDebitCards()
        {
            return GetAggregation()?.DebitCards ?? Enumerable.Empty<DebitCard>();
        }

        public IEnumerable<CreditCard> GetCreditCards()
        {
            return GetAggregation()?.CreditCards ?? Enumerable.Empty<CreditCard>();
        }

        public IEnumerable<Deposit> GetDeposits()
        {
            return GetAggregation()?.Deposits ?? Enumerable.Empty<Deposit>();
        }

        public IEnumerable<Credit> GetCredits()
        {
            return GetAggregation()?.Credits ?? Enumerable.Empty<Credit>();
        }

        public IEnumerable<Loan> GetLoans()
        {
            return GetAggregation()?.Loans ?? Enumerable.Empty<Loan>();
        }

        public IEnumerable<Fund> GetFunds()
        {
            return GetAggregation()?.Funds ?? Enumerable.Empty<Fund>();
        }

        public IEnumerable<FundsExtendedInfo> GetFundsExtendedInfo()
        {
            return GetAggregation()?.FundsExtendedInfo ?? Enumerable.Empty<FundsExtendedInfo>();
        }

        public IEnumerable<PensionPlan> GetPensionPlans()
        {
            return GetAggregation()?.PensionPlans ?? Enumerable.Empty<PensionPlan>();
        }

        public IEnumerable<Share> GetShares()
        {
            return GetAggregation()?.Shares ?? Enumerable.Empty<Share>();
        }

        public IEnumerable<DirectDebit> GetDirectDebits()
        {
            return GetAggregation()?.DirectDebits ?? Enumerable.Empty<DirectDebit>();
        }

        public PersonalInfo GetPersonalInfo()
        {
            return GetAggregation().PersonalInfo;
        }

        public AccountAggregationErrorEnum GetAggregationError()
        {
            string code = "400";
            if (!string.IsNullOrEmpty(_executionId))
            {
                GetAggregation();
                code = _aggregation.AggregationInfo.Code;
            }
            switch (code)
            {
                case "R000": // Login success (no error)
                    return AccountAggregationErrorEnum.None;
                case "R001": // Login Error (wrong credentials)
                    return AccountAggregationErrorEnum.AuthenticationFailed;
                case "R002": // Entity Website Out of Service
                    Logger.Warn($"{_robotName} website Out of Service: {_aggregation.AggregationInfo.Message}");
                    return AccountAggregationErrorEnum.TemporarilyUnavailable;
                case "R010": // User Activation Pending (the credentials are ok but the user is not active)
                    return AccountAggregationErrorEnum.AuthenticationFailed;
                case "R011": // Error in digital signature or digital signature incorrect
                    Logger.Warn($"Digital signature incorrect or missing, need to review encryption configuration");
                    return AccountAggregationErrorEnum.ElectronicSignatureMissing;
                case "R012": // Change of Password (the user is ok but the website demands the password to be changed)
                    return AccountAggregationErrorEnum.UserInteractionNeeded;
                case "R014":
                    // User Confirmation Pending (the user is ok but the website demands an action before showing the data)
                    return AccountAggregationErrorEnum.UserInteractionNeeded;
                case "R015": // Automatic Captcha Process Error (the captcha could not be interpreted by the robot)
                    return AccountAggregationErrorEnum.UsernameOrPasswordMissing;
                case "R016":
                    // Access temporarily restricted (credentials are correct, but the website does not allow the user to log in for a set period of time)
                    return AccountAggregationErrorEnum.NotAvailableToUser;
                case "R017":
                    // Second Phase Password Rejected (the target website rejected the captcha value, coordinate or temporary password).
                    return AccountAggregationErrorEnum.AuthenticationFailed;
                case "R020":
                    // Incorrect Access (the website knows the user but it belongs to a different system, i.e. Enterprise user instead of a Private one)
                    return AccountAggregationErrorEnum.NotAvailableToUser;
                case "R021": // Decryption Error
                    Logger.Warn($"Decryption error, need to review encryption configuration");
                    return AccountAggregationErrorEnum.ElectronicSignatureMissing;
                case "R050": // Invalid credential format
                    return AccountAggregationErrorEnum.UsernameOrPasswordMissing;
                case "R060": // Robot blocked
                    Logger.Warn($"{_robotName} robot is blocked: {_aggregation.AggregationInfo.Message}");
                    return AccountAggregationErrorEnum.TemporarilyUnavailable;
                case "R065": // Robot in Maintenance Mode
                    Logger.Warn($"{_robotName} robot in Maintenance Mode: {_aggregation.AggregationInfo.Message}");
                    return AccountAggregationErrorEnum.TemporarilyUnavailable;
                case "R080": // Execution Time Out, please reduce the consultation period
                    Logger.Warn($"{_executionId} Time Out, please reduce the consultation period: {_aggregation.AggregationInfo.Message}");
                    return AccountAggregationErrorEnum.TemporarilyUnavailable;
                case "R990": // Capacity exceeded
                    Logger.Warn($"{_robotName} website Out of Service: {_aggregation.AggregationInfo.Message}");
                    return AccountAggregationErrorEnum.TemporarilyUnavailable;
                case "R999": // Robot Internal Exception
                    Logger.Warn($"{_robotName} robot Internal Exception with executionId {_executionId} for person {_userId}: {_aggregation.AggregationInfo.Message}");
                    return AccountAggregationErrorEnum.TemporarilyUnavailable;
                case "R987654321": // First phase of 2-phase-authentication successful
                    return AccountAggregationErrorEnum.TwoPhaseAuthentication;
                case "R900": // Incomplete Update - try again later
                    Logger.Warn($"{_robotName} robot Incomplete Update with executionId {_executionId} for person {_userId}: {_aggregation.AggregationInfo.Message}");
                    return AccountAggregationErrorEnum.TemporarilyUnavailable;
                case "400": // BadRequest: login parameters required
                    return AccountAggregationErrorEnum.UsernameOrPasswordMissing;
                case "404":
                default:
                    Logger.Warn($"Eurobits service fails for {_robotName} with unknown error {code}: {_aggregation?.AggregationInfo.Message}");
                    return AccountAggregationErrorEnum.UnknownError;
            }
        }

        public string GetAggregationErrorMessage()
        {
            if (!string.IsNullOrEmpty(_executionId))
            {
                GetAggregation();
                return _aggregation.AggregationInfo.Message;
            }

            return string.Empty;
        }
    }
}
