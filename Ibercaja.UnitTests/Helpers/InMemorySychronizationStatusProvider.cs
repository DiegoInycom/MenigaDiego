using Meniga.Core.BusinessModels;
using Ibercaja.Aggregation;
using System;

namespace Ibercaja.UnitTests.Helpers
{
    class InMemorySychronizationStatusProvider : ISynchronizationStatusProvider
    {
        public DateTime? GetLastSyncDate(RealmUser realmUser)
        {
            return null;
        }

        public int GetPastSyncDays(RealmUser realmUser)
        {
            return 0;
        }
    }
}
