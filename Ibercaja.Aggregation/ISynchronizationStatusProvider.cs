using Meniga.Core.BusinessModels;
using System;

namespace Ibercaja.Aggregation
{
    public interface ISynchronizationStatusProvider
    {
        DateTime? GetLastSyncDate(RealmUser realmUser);
        int GetPastSyncDays(RealmUser realmUser);
    }
}
