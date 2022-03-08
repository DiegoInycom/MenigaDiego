using Meniga.Core.Data.User;
using System;

namespace Ibercaja.Aggregation
{
    public interface IAccountRepository
    {
        bool AccountExists(long personId, string accountIdentifier);
        void VisitAccount(long personId, string accountIdentifier, Action<Account> accountVisitor);
    }
}
