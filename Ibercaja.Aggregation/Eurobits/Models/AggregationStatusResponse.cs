using Newtonsoft.Json;

namespace Ibercaja.Aggregation.Eurobits
{
    public class AggregationStatusResponse : IResponse
    {
        [JsonProperty("accounts")]
        public Status Accounts { get; set; }
        [JsonProperty("deposits")]
        public Status Deposits { get; set; }
        [JsonProperty("shares")]
        public Status Shares { get; set; }
        [JsonProperty("funds")]
        public Status Funds { get; set; }
        [JsonProperty("loans")]
        public Status Loans { get; set; }
        [JsonProperty("creditCards")]
        public Status CreditCards { get; set; }
        [JsonProperty("debitCards")]
        public Status DebitCards { get; set; }
        [JsonProperty("credits")]
        public Status Credits { get; set; }
        [JsonProperty("accountHolders")]
        public Status AccountHolders { get; set; }
        [JsonProperty("pensionPlans")]
        public Status PensionPlans { get; set; }
        [JsonProperty("portfolios")]
        public Status Portfolios { get; set; }
        [JsonProperty("fundsExtendedInfo")]
        public Status FundsExtendedInfo { get; set; }
        [JsonProperty("directDebits")]
        public Status DirectDebits { get; set; }

        public bool Started()
        {
            return
                (AccountHolders?.Code ?? 0) > 0 ||
                (Accounts?.Code ?? 0) > 0 ||
                (CreditCards?.Code ?? 0) > 0 ||
                (Credits?.Code ?? 0) > 0 ||
                (DebitCards?.Code ?? 0) > 0 ||
                (Deposits?.Code ?? 0) > 0 ||
                (Funds?.Code ?? 0) > 0 ||
                (Loans?.Code ?? 0) > 0 ||
                (PensionPlans?.Code ?? 0) > 0 ||
                (Portfolios?.Code ?? 0) > 0 ||
                (Shares?.Code ?? 0) > 0 ||
                (FundsExtendedInfo?.Code ?? 0) > 0 ||
                (DirectDebits?.Code ?? 0) > 0;
        }
    }

    public class Status
    {
        [JsonProperty("finishedMessage")]
        public string FinishedMessage { get; set; }
        [JsonProperty("itemsCompleted")]
        public int ItemsCompleted { get; set; }
        [JsonProperty("itemsFound")]
        public int ItemsFound { get; set; }
        [JsonProperty("status")]
        public int Code { get; set; }
    }
}
