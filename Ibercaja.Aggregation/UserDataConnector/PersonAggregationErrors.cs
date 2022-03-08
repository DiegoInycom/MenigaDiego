using System.Collections.Generic;
using System.Linq;
using log4net;
using Meniga.Runtime.Language;

namespace Ibercaja.Aggregation.UserDataConnector
{
    /// <summary>
    /// The PersonAggregationErrors class keeps track of all aggregation errors for a particular person.
    /// </summary>
    public class PersonAggregationErrors : IPersonAggregationErrors
    {
        /// <summary>
        /// The logger for this class.
        /// </summary>
        private static readonly ILog Logger = LogManager.GetLogger(typeof(PersonAggregationErrors));

        /// <summary>
        /// All aggregation errors that exist in the system.
        /// </summary>
        private readonly Dictionary<long, List<AggregationError>> personIdToErrors;

        /// <summary>
        /// Provides access to resources
        /// </summary>
        private ILanguageResourceManager _resourceManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="PersonAggregationErrors"/> class.
        /// </summary>
        /// <param name="resourceHelper">The ICoreResourceHelper implementation to use</param>
        public PersonAggregationErrors(ILanguageResourceManager resourceHelper)
        {
            _resourceManager = resourceHelper;
            personIdToErrors = new Dictionary<long, List<AggregationError>>();
        }

        /// <summary>
        /// Adds an aggregation error to the internal person aggregation error storage.
        /// </summary>
        /// <param name="personId">Person identifier</param>
        /// <param name="realm">Realm the error happened in</param>
        /// <param name="errorCode">Error code that occurred</param>
        public void AddError(long personId, string realm, string errorCode)
        {
            List<AggregationError> errors;
            if (!personIdToErrors.TryGetValue(personId, out errors))
            {
                errors = new List<AggregationError>();
            }

            // Don't add the error again for the this person, bank and errorCode triplet.
            if (errors.Any(x => x.PersonId == personId && x.Realm == realm && x.EurobitsErrorCode == errorCode))
            {
                return;
            }

            var syncError = GetSyncTypeAndMessage(errorCode, realm);
            var aggregationError = GetAggregationTypeAndMessage(errorCode);
            errors.Add(
                new AggregationError
                    {
                        Realm = realm,
                        SyncErrorMessage = syncError.Value,
                        SyncErrorType = syncError.Key,
                        AggregationErrorMessage = aggregationError.Value,
                        AggregationErrorType = aggregationError.Key,
                        PersonId = personId,
                        EurobitsErrorCode = errorCode
                    });
            personIdToErrors[personId] = errors;   
        }

        /// <summary>
        /// Gets all aggregation errors for a particular person.
        /// This function also removes the errors from the internal storage.
        /// </summary>
        /// <param name="personId">Person identifier</param>
        /// <param name="removeFromStorage">
        /// A flag that states whether the errors should 
        /// be removed from the storage or not.
        /// </param>
        /// <returns>A list of errors that occurred for this person</returns>
        public IList<AggregationError> GetErrors(long personId, bool removeFromStorage = true)
        {
            List<AggregationError> errors;
            if (!personIdToErrors.TryGetValue(personId, out errors))
            {
                return new List<AggregationError>();
            }

            if (removeFromStorage)
            {
                personIdToErrors.Remove(personId);
            }

            return errors;
        }

        /// <summary>
        /// Maps the Eurobits error code into an aggregation error type and a message to display to the user as well.
        /// </summary>
        /// <param name="errorCode">Error code</param>
        /// <returns>A key value pair with the key as the error type and value as the error message.</returns>
        private KeyValuePair<string, string> GetAggregationTypeAndMessage(string errorCode)
        {
            switch (errorCode)
            {
                case "R001":
                    return new KeyValuePair<string, string>("InvalidCredentials", _resourceManager.GetResourceKey("Aggregation.ErrorInvalidCredentials", "AngularAppResources", "Mobile"));
                case "R010":
                case "R012":
                case "R014":
                    return new KeyValuePair<string, string>("PendingActions", _resourceManager.GetResourceKey("Aggregation.ErrorPendingActions", "AngularAppResources", "Mobile"));
                case "R020":
                    return new KeyValuePair<string, string>("CorporateAccount", _resourceManager.GetResourceKey("Aggregation.ErrorCorporateAccount", "AngularAppResources", "Mobile"));
                case "R017":
                    return new KeyValuePair<string, string>("TwoPhase", _resourceManager.GetResourceKey("Aggregation.ErrorTwoPhase", "AngularAppResources", "Mobile"));
                case "R002":
                case "R015":
                case "R065":
                case "R080":
                case "R999":
                    return new KeyValuePair<string, string>("TryLater", _resourceManager.GetResourceKey("Aggregation.ErrorTryLater", "AngularAppResources", "Mobile"));
                default:
                    Logger.Error(string.Format("An unknown error code({0}) was returned from Eurobits", errorCode));
                    return new KeyValuePair<string, string>("TryLater", _resourceManager.GetResourceKey("Aggregation.ErrorTryLater", "AngularAppResources", "Mobile"));
            }
        }

        /// <summary>
        /// Maps the Eurobits error code into an synchronization error type and a message to display to the user as well.
        /// </summary>
        /// <param name="errorCode">Error code</param>
        /// <param name="realm">Realm the error happened in. Used to format the error messages</param>
        /// <returns>A key value pair with the key as the error type and value as the error message.</returns>
        private KeyValuePair<string, string> GetSyncTypeAndMessage(string errorCode, string realm)
        {
            switch (errorCode)
            {
                case "R001":
                    return new KeyValuePair<string, string>("InvalidCredentials", string.Format(_resourceManager.GetResourceKey("Synchronization.ErrorInvalidCredentials", "AngularAppResources", "Mobile"), realm));
                case "R002":
                case "R015":
                case "R065":
                case "R080":
                case "R999":
                    return new KeyValuePair<string, string>("NonUpdatedDataNoUserActionRequired", string.Format(_resourceManager.GetResourceKey("Synchronization.ErrorNonUpdatedDataNoUserActionRequired", "AngularAppResources", "Mobile"), realm));
                case "R010":
                case "R012":
                case "R014":
                    return new KeyValuePair<string, string>("NonUpdatedDataUserActionRequired", string.Format(_resourceManager.GetResourceKey("Synchronization.ErrorNonUpdatedDataUserActionRequired", "AngularAppResources", "Mobile"), realm));
                case "R020":
                    return new KeyValuePair<string, string>("CorporateAccount", string.Format(_resourceManager.GetResourceKey("Synchronization.ErrorCorporateAccount", "AngularAppResources", "Mobile"), realm));
                case "R900":
                    return new KeyValuePair<string, string>("PartiallyUpdated", string.Format(_resourceManager.GetResourceKey("Synchronization.ErrorPartiallyUpdated", "AngularAppResources", "Mobile"), realm));
                case "R017":
                    return new KeyValuePair<string, string>("TwoPhase", _resourceManager.GetResourceKey("Synchronization.ErrorTwoPhase", "AngularAppResources", "Mobile"));
                default:
                    Logger.Error(string.Format("An unknown error code({0}) was returned from Eurobits", errorCode));
                    return new KeyValuePair<string, string>("TryLater", _resourceManager.GetResourceKey("Aggregation.ErrorTryLater", "AngularAppResources", "Mobile"));
            }
        }
    }
}
