using System.Collections.Generic;

namespace Ibercaja.Aggregation.UserDataConnector
{
    /// <summary>
    /// The IPersonAggregationErrors describes the interface for person aggregation errors
    /// </summary>
    public interface IPersonAggregationErrors
    {
        /// <summary>
        /// Adds an aggregation error to the internal person aggregation error storage.
        /// </summary>
        /// <param name="personId">Person identifier</param>
        /// <param name="realm">Realm the error happened in</param>
        /// <param name="errorCode">Error code that occured</param>
        void AddError(long personId, string realm, string errorCode);

        /// <summary>
        /// Gets all aggregation errors for a particular person.
        /// This function also removes the errors from the internal storage.
        /// </summary>
        /// <param name="personId">Person identifier</param>
        /// <param name="removeFromStorage">
        /// A flag that states whether the errors should 
        /// be removed from the storage or not.
        /// </param>
        /// <returns>A list of errors that occured for this user</returns>
        IList<AggregationError> GetErrors(long personId, bool removeFromStorage = true);
    }
}
