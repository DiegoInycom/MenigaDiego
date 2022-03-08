using Meniga.Core.BusinessModels;
using System;
using System.Collections.Generic;

namespace Ibercaja.Aggregation.Eurobits
{
    public interface IAggregationService
    {
        void SetSessionToken(string token);
        string GetSessionToken();
        void Clear();

        AggregationStatus ExecuteAggregation(Parameter[] parameters, DateTime from);
        AggregationStatus GetAggregationStatus();
        IEnumerable<ParameterDescription> GetRequiredParameters();
        SecondPhaseParameter GetSecondPhaseParameter();
        bool PutSecondPhaseParameter(Parameter parameter);

        IEnumerable<Account> GetAccounts();
        IEnumerable<AccountHolder> GetAccountHolders();
        IEnumerable<DebitCard> GetDebitCards();
        IEnumerable<CreditCard> GetCreditCards();
        IEnumerable<Deposit> GetDeposits();
        IEnumerable<Credit> GetCredits();
        IEnumerable<Loan> GetLoans();
        IEnumerable<Fund> GetFunds();
        IEnumerable<FundsExtendedInfo> GetFundsExtendedInfo();
        IEnumerable<PensionPlan> GetPensionPlans();
        IEnumerable<Share> GetShares();
        IEnumerable<DirectDebit> GetDirectDebits();
        PersonalInfo GetPersonalInfo();

        AccountAggregationErrorEnum GetAggregationError();
        string GetAggregationErrorMessage();
    }

    public class SecondPhaseParameter
    {
        public ParameterDescription Parameter { get; set; }
        public ChallengeContentType ContentType { get; set; }
        public string TextChallenge { get; set; }
        public byte[] BinaryChallenge { get; set; }
    }

    public enum AggregationStatus
    {
        Login,
        SecondPhase,
        InProgress,
        Finished,
        Error
    }
}
