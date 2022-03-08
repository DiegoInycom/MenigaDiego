using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ibercaja.Aggregation.Eurobits.Service
{
    public class DummyEurobitsApiService : EurobitsApiService
    {
        public DummyEurobitsApiService(
            string BaseAddress,
            string certificateAlias,
            string eurobitsApiServiceId,
            string eurobitsApiPassword) :
            base(BaseAddress, certificateAlias, eurobitsApiServiceId, eurobitsApiPassword)
        {
        }

        public override async Task<AggregationResponse> GetAggregation(string executionId)
        {
            var result = await base.GetAggregation(executionId).ConfigureAwait(false);
            Decorate(result.Accounts, a => a.AccountNumber = $"A{a.AccountNumber}");
            Decorate(result.AccountHolders, a => a.AccountNumber = $"AH{a.AccountNumber}");
            Decorate(result.DebitCards, a => a.CardNumber = $"DC{a.CardNumber}");
            Decorate(result.CreditCards, a => a.CardNumber = $"CC{a.CardNumber}");
            Decorate(result.Credits, a => a.AccountNumber = $"C{a.AccountNumber}");
            Decorate(result.Deposits, a => a.AccountNumber = $"D{a.AccountNumber}");
            Decorate(result.Funds, a => a.AccountNumber = $"F{a.AccountNumber}");
            Decorate(result.FundsExtendedInfo, a => a.AccountNumber = $"FEI{a.AccountNumber}");
            Decorate(result.Loans, a => a.AccountNumber = $"L{a.AccountNumber}");
            Decorate(result.PensionPlans, a => a.PlanNumber = $"PP{a.PlanNumber}");
            Decorate(result.Shares, a => a.AccountNumber = $"S{a.AccountNumber}");
            return result;
        }

        void Decorate<T>(IEnumerable<T> col, Action<T> action)
        {
            foreach (var e in col ?? Enumerable.Empty<T>())
            {
                action(e);
            }
        }
    }
}