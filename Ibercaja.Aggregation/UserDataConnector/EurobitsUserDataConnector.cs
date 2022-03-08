using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Threading;
using log4net;
using Meniga.Core.BusinessModels;
using Meniga.Core.DataConsolidation;
using Newtonsoft.Json;
using Ibercaja.Aggregation.Products;
using Ibercaja.Aggregation.Eurobits;
using Ibercaja.Aggregation.Eurobits.Service;
using Ibercaja.Aggregation.UserDataConnector.Configuration;
using Ibercaja.Aggregation.Security;
using Meniga.Core.Users;
using Meniga.Runtime.Services;

namespace Ibercaja.Aggregation.UserDataConnector
{
    public abstract class EurobitsUserDataConnector : IAggregationAgent
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(EurobitsUserDataConnector));
        private readonly int _sleepTimer;
        private readonly DateTime? _lastSyncEndDate;
        private readonly int _pastDaysSync;
        protected Parameter[] StoredParameters;
        private readonly UserDataConnectorConfigurationRealm _userDataConnectorConfigurationRealm;
        private readonly IProductProviderFactory _productProviderFactory;
        private readonly IAggregationService _eurobits;
        private readonly ISecurityService _securityService;
        private readonly RealmUser _realmUser;
        private readonly bool _realmUserDoesNotExist;

        protected EurobitsUserDataConnector(
            IEurobitsApiService eurobitsApi,
            UserDataConnectorConfigurationRealm configuration,
            IDictionary<string, string> invertConfigurationConfig,
            RealmUser realmUser,
            ISynchronizationStatusProvider synchronizationStatusProvider,
            ICoreUserManager userManager,
            ISecurityService securityService)
        {
            _realmUser = realmUser;
            _userDataConnectorConfigurationRealm = configuration;
            _realmUserDoesNotExist = string.IsNullOrWhiteSpace(realmUser.UserIdentifier);
            var personId = realmUser.PersonId.ToString(CultureInfo.InvariantCulture);
            _eurobits = new EurobitsAggregationService(eurobitsApi, personId, _userDataConnectorConfigurationRealm);
            _securityService = securityService;
            _productProviderFactory = new IbercajaProductProviderFactory(
                configuration,
                invertConfigurationConfig,
                _eurobits,
                userManager.GetPersonInfo(realmUser.PersonId).Email.Split('@')[0]
            );

            // Last sync date for this realmUser
            _lastSyncEndDate = synchronizationStatusProvider.GetLastSyncDate(realmUser);
            _pastDaysSync = synchronizationStatusProvider.GetPastSyncDays(realmUser);

            if (!int.TryParse(ConfigurationManager.AppSettings["EurobitsApiIsFinishSleepTimer"], out _sleepTimer))
            {
                _sleepTimer = 1000;
            }
        }

        #region IUsedDataConnector implementation
        public BankAccountInfo[] GetAccountInfo(string userIdentifier)
        {
            Logger.Debug("In GetAccountInfo");

            var accountsProviders = _productProviderFactory.GetAllAccountsProviders();
            var accounts = accountsProviders.SelectMany(x => x.GetBankAccountInfos()).ToArray();
            return accounts;
        }

        public AccountStatement GetAccountStatement(
            AccountCategoryEnum accountCategory,
            string accountCategoryDetail,
            string accountIdentifier,
            DateTime from,
            DateTime to)
        {
            Logger.Debug("In GetAccountStatement");

            var transactionsProvider = _productProviderFactory.GetTransactionsProvider(accountCategory, accountCategoryDetail);
            var accountStatement = transactionsProvider.GetAccountStatement(accountIdentifier);

            return accountStatement;
        }

        public AccountStatement GetAccountStatementIncremental(
            AccountCategoryEnum accountCategory,
            string accountCategoryDetail,
            string accountId,
            string syncToken)
        {
            Logger.Debug("In GetAccountStatementIncremental");

            throw new NotImplementedException();
        }

        /// <summary>
        ///     This method has not yet been implemented
        /// </summary>
        /// <param name="accountCategory">Account category</param>
        /// <param name="accountCategoryDetail">Account category detail</param>
        /// <param name="accountId">Account Id</param>
        /// <returns>Nothing since this has not been implemented</returns>
        public AccountStatement GetAccountStatementIntraday(
            AccountCategoryEnum accountCategory,
            string accountCategoryDetail,
            string accountId)
        {
            Logger.Debug("In GetAccountStatementIntraday");

            throw new NotImplementedException();
        }
        #endregion

        #region IAggregationAgent implementation
        /// <summary>
        /// Sets the session token
        /// </summary>
        /// <param name="sessionToken">Session token to set</param>
        public void SetSessionToken(string sessionToken)
        {
            Logger.Debug($"Invoking SetSessionToken with parameter: '{sessionToken}'");

            _eurobits.SetSessionToken(sessionToken);
        }

        /// <summary>
        /// Starts a session for the user. 
        /// </summary>
        /// <param name="parameters">Set of parameters that have been stored in Meniga database</param>
        /// <returns>ExecutionId from Eurobits if a session could be started, else string.empty will be returned</returns>
        public string StartSession(Parameter[] parameters)
        {
            Logger.Debug("In StartSession");

            // As this parameters are stored in Meniga database they are encrypted and encoded, no need to check
            StoredParameters = parameters;

            return _eurobits.GetSessionToken();
        }

        /// <summary>
        /// Ends a session for the user
        /// </summary>
        public void EndSession()
        {
            Logger.Debug("In EndSession");

            _eurobits.Clear();
        }

        /// <summary>
        /// Authenticate a user according to the provided parameters,
        /// if successful then parameters will be stored in Meniga database.
        /// </summary>
        /// <param name="parameters">Parameters to authenticate with</param>
        /// <returns>Success if user could authenticated. Otherwise,
        /// an error response with the required parameters</returns>
        public OrganizationAuthenticationResponse Authenticate(Parameter[] parameters)
        {
            Logger.Debug("In Authenticate");

            var authResponse = new OrganizationAuthenticationResponse();

            // if there is no realmUserIdentifier, then core didn't find 
            // realmUser while creating instance of EurobitsUserDataConnector.
            // To prevent duplicates in realmUserIdentifier, we need to 
            // prefix it with personId.
            if (string.IsNullOrEmpty(_realmUser.UserIdentifier))
            {
                var userIdentifierParameter =
                    (parameters ?? Enumerable.Empty<Parameter>()).FirstOrDefault(x =>
                        x.Name == _userDataConnectorConfigurationRealm.UserIdentifier);
                if (userIdentifierParameter == null || string.IsNullOrWhiteSpace(userIdentifierParameter.Value))
                {
                    // when there is no user identifier in parameters then we can't create new
                    // realm user. This means the client asks for parameters required to be logged in
                    authResponse.HasError = false;
                    authResponse.CanSave = false;
                    authResponse.RequiredParameters = _eurobits.GetRequiredParameters().ToArray();
                    return authResponse;
                }
                _realmUser.UserIdentifier = $"{MenigaServiceContext.Current.PersonId}-{userIdentifierParameter.Value}";
            }

            if (string.IsNullOrEmpty(_eurobits.GetSessionToken()) && parameters == null && (StoredParameters == null || !StoredParameters.Any()))
            {
                authResponse.RequiredParameters = _eurobits.GetRequiredParameters().ToArray();
                if (authResponse.RequiredParameters.Length == 0)
                {
                    // If no required parameters returned by service, the robotInfo service is unavailable
                    authResponse.HasError = true;
                    authResponse.ErrorMessage = AccountAggregationErrorEnum.TemporarilyUnavailable;
                }
            }
            else
            {
                if (parameters != null)
                {
                    StoredParameters = EncryptParameters(parameters);
                }
                if (string.IsNullOrEmpty(_eurobits.GetSessionToken()))
                {
                    // New aggregation
                    var fromDate = _lastSyncEndDate?.AddDays(_pastDaysSync) ?? DateTime.Today.AddMonths(-6);
                    _eurobits.ExecuteAggregation(StoredParameters, fromDate);
                }
                else
                {
                    if (_eurobits.GetAggregationStatus() == AggregationStatus.SecondPhase)
                    {
                        var secondPhaseParameter = _eurobits.GetSecondPhaseParameter();
                        var parameter = parameters.FirstOrDefault(p => p.Name == secondPhaseParameter.Parameter.Name);
                        if (secondPhaseParameter.Parameter.IsEncrypted)
                        {
                            parameter = _securityService.EncryptParameter(parameter);
                        }
                        if (parameter != null)
                        {
                            _eurobits.PutSecondPhaseParameter(parameter);
                        }
                    }
                }

                while (_eurobits.GetAggregationStatus() == AggregationStatus.Login)
                {
                    Thread.Sleep(_sleepTimer);
                }

                authResponse.SessionToken = _eurobits.GetSessionToken();
                switch (_eurobits.GetAggregationStatus())
                {
                    case AggregationStatus.SecondPhase:
                        var secondPhaseParameter = _eurobits.GetSecondPhaseParameter();
                        var requiredParameters = new List<ParameterDescription>();

                        // when calling sync/realm/auth for the first time,
                        // realmUser doesn't exist and no parameters will be saved by core.
                        // To reuse credentials in future, we must ask user to provide them
                        // in second phase request as well.
                        if (_realmUserDoesNotExist)
                        {
                            requiredParameters.AddRange(_eurobits.GetRequiredParameters());
                        }
                        requiredParameters.Add(secondPhaseParameter.Parameter);

                        authResponse.HasError = true;
                        authResponse.ErrorMessage = AccountAggregationErrorEnum.TwoPhaseAuthentication;
                        authResponse.RequiredParameters = requiredParameters.ToArray();
                        authResponse.ContentType = secondPhaseParameter.ContentType;
                        authResponse.TextChallenge = secondPhaseParameter.TextChallenge;
                        authResponse.BinaryChallenge = secondPhaseParameter.BinaryChallenge;
                        authResponse.CanSave = true;
                        break;
                    case AggregationStatus.Error:
                        authResponse.HasError = true;
                        authResponse.RequiredParameters = _eurobits.GetRequiredParameters().ToArray();
                        authResponse.ErrorMessage = _eurobits.GetAggregationError();
                        authResponse.LoginHelp = _eurobits.GetAggregationErrorMessage();
                        break;
                    case AggregationStatus.Finished:
                        authResponse.HasError = false;
                        break;
                    case AggregationStatus.InProgress:
                        authResponse.HasError = false;
                        break;
                }
            }

            Logger.Info($"authResponse: {JsonConvert.SerializeObject(authResponse)}");
            return authResponse;
        }

        private Parameter[] EncryptParameters(Parameter[] parameters)
        {
            if (parameters != null)
            {
                var requiredParameters = _eurobits.GetRequiredParameters();
                var nonEncryptedParameters = parameters.Where(p => requiredParameters.Any(rp => rp.Name == p.Name && !rp.IsEncrypted));
                var encryptedParameters = _securityService.EncryptCredentials(parameters.Where(p => requiredParameters.Any(rp => rp.Name == p.Name && rp.IsEncrypted)));
                return encryptedParameters.Union(nonEncryptedParameters).ToArray();
            }
            return new Parameter[0];
        }

        /// <summary>
        /// The required parameters for authentication
        /// </summary>
        /// <returns>The set of required parameters</returns>
        public ParameterDescription[] GetRequiredParameters()
        {
            Logger.Debug("In GetRequiredParameters");

            var requiredParameters = _eurobits.GetRequiredParameters();

            if (StoredParameters != null && StoredParameters.Length != 0)
            {
                // Select missing Required Parameters
                requiredParameters = requiredParameters.Where(a =>
                    !StoredParameters.Any(b => b.Name.Equals(a.Name))
                ).ToArray();
            }

            return requiredParameters.Where(item => item != null).ToArray();
        }

        public Parameter[] GetParameters()
        {
            return StoredParameters ?? Array.Empty<Parameter>();
        }

        public OrganizationBasicInfo GetBasicInfo()
        {
            Logger.Debug("In GetBasicInfo");

            return new OrganizationBasicInfo { IdentityParamName = _userDataConnectorConfigurationRealm.UserIdentifier };
        }
        #endregion
    }
}
