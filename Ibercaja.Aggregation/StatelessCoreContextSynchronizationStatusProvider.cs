using Meniga.Core.BusinessModels;
using Meniga.Core.Data;
using Meniga.Runtime.IOC;
using System;
using System.Linq;

namespace Ibercaja.Aggregation
{
    class StatelessCoreContextSynchronizationStatusProvider : ISynchronizationStatusProvider
    {
        public DateTime? GetLastSyncDate(RealmUser realmUser)
        {
            using (var personContext = IoC.Resolve<ICoreContextProvider>().UserContext(realmUser.PersonId))
            {
                var realmUserId = realmUser.Id ?? personContext.RealmUsers?
                    .FirstOrDefault(f => f.RealmId == realmUser.RealmId && f.PersonId == realmUser.PersonId)?
                    .Id;
                var realmProcessingEntry = personContext.RealmProcessingEntries
                    .OrderByDescending(f => f.Id)
                    .Where(f => f.RealmUserId == realmUserId && f.SyncEndDate.HasValue)
                    .FirstOrDefault();
                return realmProcessingEntry?.SyncEndDate;
            }
        }

        public int GetPastSyncDays(RealmUser realmUser)
        {
            using (var appContext = IoC.Resolve<ICoreContextProvider>().AppContext())
            {
                int? pastSyncDate = appContext.AccountTypes?
                    .Where(f => f.RealmId == realmUser.RealmId)
                    .Select(f => f.PastSyncDays)
                    .DefaultIfEmpty()
                    .Max();
                return pastSyncDate.HasValue ? Negative(pastSyncDate.Value) : 0;
            }
        }

        public static int Negative(int i) => -Math.Abs(i);
    }
}
