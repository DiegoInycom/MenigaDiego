using System.Runtime.Serialization;

namespace Ibercaja.Aggregation.UserDataConnector
{
    /// <summary>
    /// The AggregationError class holds information about an aggregation error that occurred when
    /// contacting Eurobits.
    /// We have two sets of type and message because EURO wants to display different messages based
    /// on whether the user is aggregating or syncing
    /// </summary>
    [DataContract]
    public class AggregationError
    {
        /// <summary>
        /// Gets or sets the person id
        /// </summary>
        [DataMember]
        public long PersonId { get; set; }

        /// <summary>
        /// Gets or sets in what realm did the error occur
        /// </summary>
        [DataMember]
        public string Realm { get; set; }

        /// <summary>
        /// Gets or sets the type of synchronization error
        /// InvalidCredentials, CorporateAccount, TryLater, PendingActions, TwoPhase
        /// </summary>
        [DataMember]
        public string SyncErrorType { get; set; }

        /// <summary>
        /// Gets or sets the type of aggregation error
        /// </summary>
        [DataMember]
        public string AggregationErrorType { get; set; }

        /// <summary>
        /// Gets or sets a localized error message for synchronization
        /// </summary>
        [DataMember]
        public string SyncErrorMessage { get; set; }

        /// <summary>
        /// Gets or sets a localized error message for aggregation
        /// </summary>
        [DataMember]
        public string AggregationErrorMessage { get; set; }

        /// <summary>
        /// Gets or sets the eurobits error code
        /// </summary>
        [DataMember]
        public string EurobitsErrorCode { get; set; }
    }
}
