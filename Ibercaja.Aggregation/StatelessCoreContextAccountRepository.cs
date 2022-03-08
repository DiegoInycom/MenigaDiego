using Meniga.Core.Data;
using Meniga.Runtime.IOC;
using System;
using System.Linq;

namespace Ibercaja.Aggregation
{
    class StatelessCoreContextAccountRepository : IAccountRepository
    {
        public bool AccountExists(long personId, string accountIdentifier)
        {
            using (var personContext = IoC.Resolve<ICoreContextProvider>().UserContext(personId))
            {
                return personContext.Accounts.Any(a => a.Identifier == accountIdentifier && a.PersonId == personId);
            }
        }

        public void VisitAccount(long personId, string accountIdentifier, Action<Meniga.Core.Data.User.Account> accountVisitor)
        {
            using (var personContext = IoC.Resolve<ICoreContextProvider>().UserContext(personId))
            {
                var account = personContext.Accounts.FirstOrDefault(a => a.Identifier == accountIdentifier && a.PersonId == personId);
                accountVisitor(account);
                personContext.SaveChanges();
            }
        }
    }
}
