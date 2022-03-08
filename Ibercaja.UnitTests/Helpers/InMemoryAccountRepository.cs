using Ibercaja.Aggregation;
using System;
using System.Collections.Generic;
using System.Linq;
using Meniga.Core.Data.User;

namespace Ibercaja.UnitTests.Helpers
{
    public class InMemoryAccountRepository : IAccountRepository
    {
        private readonly List<Account> _accounts;

        public InMemoryAccountRepository()
        {
            _accounts = new List<Account>
                {
                    new Account
                    {
                        Identifier = "Account",
                        PersonId = 111
                    }
                };
        }

        internal InMemoryAccountRepository(List<Account> accountsSource)
        {
            _accounts = accountsSource;
        }

        public bool AccountExists(long personId, string accountIdentifier)
        {
            return _accounts.Any(x => x.PersonId == personId && x.Identifier == accountIdentifier);
        }

        public void VisitAccount(long personId, string accountIdentifier, Action<Account> accountVisitor)
        {
            foreach (var a in _accounts.Where(x => x.PersonId == personId && x.Identifier == accountIdentifier))
            {
                accountVisitor(a);
            }
        }
    }
}
