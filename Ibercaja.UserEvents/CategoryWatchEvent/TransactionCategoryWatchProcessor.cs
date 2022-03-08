using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using log4net;
using Meniga.Core.Categories;
using Meniga.Core.Data;
using Meniga.Core.UserEvents.BusinessModels;
using Meniga.Core.UserEvents.Constants;
using Meniga.Core.UserEvents.Extensions;
using Meniga.Core.UserEvents.Processors.DataAccess;
using Microsoft.Practices.Unity;

namespace Ibercaja.UserEvents.CategoryWatchEvent
{
    public class TransactionCategoryWatchProcessor : IUserEventProcessor<TransactionCategoryWatchProcessor.Settings, TransactionCategoryWatchProcessor.Data>
    {
        private static readonly ILog _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private const string Identifier = "transactions_category_watch";

        [Dependency]
        public IUserEventProcessorDataAccess UserEventProcessorDataAccess { get; set; }

        [Dependency]
        public ICategoryCache CategoryCache { get; set; }

        [Dependency]
        private ICoreContextProvider _dataContextProvider { get; set; }

        public class Settings : DefaultProcessorSettings
        {
            public Settings()
            {
                
            }

            // default settings - does not  override existing settings. Should only be called by UserEventTypeRegistry.cs
            public Settings(bool registerDefaults)
            {
                Categories = new List<int>() { };  // TODO: Add categoryIds to watch
            }
                        
            /// <summary>
            /// The list of categories this event should trigger on 
            /// </summary>
            public List<int> Categories { get; set; }
        }

        public class Data : DefaultData
        {
            public int CategoryId { get; set; }
            public string DisplayIconIdentifier { get; set; }
        }

        public TransactionCategoryWatchProcessor()
        {
            
        }

        public ICollection<long> InitBatchProcess(IProcessingContext<Settings, Data> context)
        {
            throw new NotSupportedException(string.Format("The {0} user event processor does not support batch processing where the affected user(s) is unknown.", Identifier));
        }

        public ICollection<Data> ProcessUserEvents(long userId, IProcessingContext<Settings, Data> context)
        {
            var entryIds = GetProcessingEntryIds(context);
            if (entryIds == null) return null;            

            var categoryIds = context.HasSettings ? context.Settings.Categories : null;

            if (categoryIds == null || categoryIds.Count == 0)
            {
                _logger.Error("No category defined for the category watch processor");
                return null;
            }

            var data = new List<Data>();
            var triggeringTransactions = FindTransactionsInCategories(userId, entryIds, categoryIds).ToList();

            if (triggeringTransactions == null || !triggeringTransactions.Any())
                return null;

            foreach (var trans in triggeringTransactions)
            {
                var date = trans.SubDate;

                data.Add(new Data()
                {
                    CategoryId = trans.CategoryId.Value,
                    TopicId = trans.Id,                                 // TopicId is the Transaction.Id
                    Date = date,                                        // Date is the Transaction.Date
                    DisplayIconIdentifier = trans.CategoryId.ToString() // DisplayIconIdentifier is the categoryId
                });
            }
            return data;
        }

        private long[] GetProcessingEntryIds(IProcessingContext context)
        {
            if (context.Parameters.ContainsKey(UserEventTypeProcessingParametersConstants.ProcessingEntryIds))
            {
                if (context.Parameters[UserEventTypeProcessingParametersConstants.ProcessingEntryIds].Length > 0)
                {
                    long tmp;
                    return context.Parameters[UserEventTypeProcessingParametersConstants.ProcessingEntryIds].Split(',')
                        .Where(x => long.TryParse(x, out tmp)).Select(long.Parse).ToArray();
                }
            }
            return new long[0];
        }

        private IEnumerable<Meniga.Core.Data.User.Transaction> FindTransactionsInCategories(long userId, long[] entryIds, List<int> categoryIds)
        {
            using (var userContext = _dataContextProvider.UserContext(userId))
            {
                var query = from t in userContext.Transactions
                            where t.Account.Users.Any(u => u.Id == userId) && !t.IsDeleted && t.SubAmount.HasValue && t.SubAmount.Value < 0
                            where t.UserDataProcessingEntryId.HasValue && entryIds.Contains(t.UserDataProcessingEntryId.Value)
                            where t.CategoryId.HasValue && categoryIds.Contains(t.CategoryId.Value)
                            select t;

                return query.ToList();
            }
        }
    }
}
