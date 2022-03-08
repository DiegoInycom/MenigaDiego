using Newtonsoft.Json;
using System.Collections.Generic;

namespace Ibercaja.Aggregation.Eurobits
{
    public class AggregationResponse : IResponse
    {
        [JsonProperty("aggregationInfo")]
        public AggregationInfo AggregationInfo { get; set; }
        [JsonProperty("accounts")]
        public Account[] Accounts { get; set; }
        [JsonProperty("deposits")]
        public Deposit[] Deposits { get; set; }
        [JsonProperty("shares")]
        public Share[] Shares { get; set; }
        [JsonProperty("funds")]
        public Fund[] Funds { get; set; }
        [JsonProperty("loans")]
        public Loan[] Loans { get; set; }
        [JsonProperty("creditCards")]
        public CreditCard[] CreditCards { get; set; }
        [JsonProperty("debitCards")]
        public DebitCard[] DebitCards { get; set; }
        [JsonProperty("credits")]
        public Credit[] Credits { get; set; }
        [JsonProperty("accountHolders")]
        public AccountHolder[] AccountHolders { get; set; }
        [JsonProperty("pensionPlans")]
        public PensionPlan[] PensionPlans { get; set; }
        [JsonProperty("portfolios")]
        public Portfolio[] Portfolios { get; set; }
        [JsonProperty("fundsextendedinfo")]
        public FundsExtendedInfo[] FundsExtendedInfo { get; set; }
        [JsonProperty("directdebits")]
        public DirectDebit[] DirectDebits { get; set; }
        [JsonProperty("personalInfo")]
        public PersonalInfo PersonalInfo { get; set; }

        public bool HaveResults()
        {
            return Accounts != null || Deposits != null || Shares != null ||
                   Funds != null || Loans != null || CreditCards != null ||
                   DebitCards != null || Credits != null || AccountHolders != null ||
                   PensionPlans != null || Portfolios != null ||
                   FundsExtendedInfo != null || DirectDebits != null || PersonalInfo != null;
        }
    }

    public class AggregationInfo
    {
        [JsonProperty("code")]
        public string Code { get; set; }
        [JsonProperty("message")]
        public string Message { get; set; }
        [JsonProperty("warnings")]
        public Warning[] Warnings { get; set; }
    }

    public class Warning
    {
        [JsonProperty("warningCause")]
        public string WarningCause { get; set; }
        [JsonProperty("warningDetails")]
        public string WarningDetails { get; set; }
    }

    #region Common
    public class Amount
    {
        [JsonProperty("amount")]
        public string Value { get; set; }
        [JsonProperty("currency")]
        public string Currency { get; set; }
    }
    public class AmountWithValueDate
    {
        [JsonProperty("amount")]
        public string Value { get; set; }
        [JsonProperty("currency")]
        public string Currency { get; set; }
        [JsonProperty("valueDate")]
        public string ValueDate { get; set; }
    }
    public class Period
    {
        [JsonProperty("endDate")]
        public string EndDate { get; set; }
        [JsonProperty("startDate")]
        public string StartDate { get; set; }
    }
    #endregion

    #region Account
    public class Account
    {
        [JsonProperty("account")]
        public string AccountNumber { get; set; }
        [JsonProperty("balance")]
        public Amount Balance { get; set; }
        [JsonProperty("bank")]
        public string Bank { get; set; }
        [JsonProperty("branch")]
        public string Branch { get; set; }
        [JsonProperty("controlDigits")]
        public string ControlDigits { get; set; }
        [JsonProperty("webAlias")]
        public string WebAlias { get; set; }
        [JsonProperty("transactions")]
        public AccountTransaction[] Transactions { get; set; }
    }
    public class AccountTransaction
    {
        [JsonProperty("transactionType")]
        public string Type { get; set; }
        [JsonProperty("amount")]
        public Amount Amount { get; set; }
        [JsonProperty("balance")]
        public Amount Balance { get; set; }
        [JsonProperty("description")]
        public string Description { get; set; }
        [JsonProperty("operationDate")]
        public string OperationDate { get; set; }
        [JsonProperty("payee")]
        public string Payee { get; set; }
        [JsonProperty("payer")]
        public string Payer { get; set; }
        [JsonProperty("reference")]
        public string Reference { get; set; }
        [JsonProperty("valueDate")]
        public string ValueDate { get; set; }
    }
    #endregion

    #region Deposits
    public class Deposit
    {
        [JsonProperty("accountNumber")]
        public string AccountNumber { get; set; }
        [JsonProperty("balance")]
        public Amount Balance { get; set; }
        [JsonProperty("bank")]
        public string Bank { get; set; }
        [JsonProperty("branch")]
        public string Branch { get; set; }
        [JsonProperty("controlDigits")]
        public string ControlDigits { get; set; }
        [JsonProperty("duration")]
        public Period Duration { get; set; }
        [JsonProperty("interest")]
        public Interest Interest { get; set; }
        [JsonProperty("number")]
        public string Number { get; set; }
        [JsonProperty("webAlias")]
        public string WebAlias { get; set; }
    }
    public class Interest
    {
        [JsonProperty("rate")]
        public string Rate { get; set; }
        [JsonProperty("type")]
        public string Type { get; set; }
    }
    #endregion

    #region Shares
    public class Share
    {
        [JsonProperty("accountNumber")]
        public string AccountNumber { get; set; }
        [JsonProperty("webAlias")]
        public string WebAlias { get; set; }
        [JsonProperty("balance")]
        public Amount Balance { get; set; }
        [JsonProperty("stocks")]
        public Stock[] Stocks { get; set; }
        [JsonProperty("transactions")]
        public ShareTransaction[] Transactions { get; set; }
    }
    public class Stock
    {
        [JsonProperty("market")]
        public string Market { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("quantity")]
        public string Quantity { get; set; }
        [JsonProperty("unitPrice")]
        public Amount UnitPrice { get; set; }
        [JsonProperty("valuationDate")]
        public string ValuationDate { get; set; }
    }
    public class ShareTransaction
    {
        [JsonProperty("amount")]
        public Amount Amount { get; set; }
        [JsonProperty("operationType")]
        public string OperationType { get; set; }
        [JsonProperty("operationDescription")]
        public string OperationDescription { get; set; }
        [JsonProperty("operationDate")]
        public string OperationDate { get; set; }
        [JsonProperty("market")]
        public string Market { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("quantity")]
        public string Quantity { get; set; }
        [JsonProperty("unitPrice")]
        public Amount UnitPrice { get; set; }
    }
    #endregion

    #region Funds
    public class Fund
    {
        [JsonProperty("accountNumber")]
        public string AccountNumber { get; set; }
        [JsonProperty("number")]
        public string Number { get; set; }
        [JsonProperty("balance")]
        public Amount Balance { get; set; }
        [JsonProperty("branch")]
        public string Branch { get; set; }
        [JsonProperty("controlDigits")]
        public string ControlDigits { get; set; }
        [JsonProperty("fundName")]
        public string FundName { get; set; }
        [JsonProperty("performance")]
        public string Performance { get; set; }
        [JsonProperty("performanceDescription")]
        public string PerformanceDescription { get; set; }
        [JsonProperty("quantity")]
        public string Quantity { get; set; }
        [JsonProperty("valueDate")]
        public string ValueDate { get; set; }
        [JsonProperty("webAlias")]
        public string WebAlias { get; set; }
        [JsonProperty("yield")]
        public Amount Yield { get; set; }
        [JsonProperty("transactions")]
        public FundTransaction[] Transactions { get; set; }
    }
    public class FundTransaction
    {
        [JsonProperty("amount")]
        public Amount Amount { get; set; }
        [JsonProperty("operationType")]
        public string OperationType { get; set; }
        [JsonProperty("operationDescription")]
        public string OperationDescription { get; set; }
        [JsonProperty("operationDate")]
        public string OperationDate { get; set; }
        [JsonProperty("fundName")]
        public string FundName { get; set; }
        [JsonProperty("quantity")]
        public string Quantity { get; set; }
        [JsonProperty("unitPrice")]
        public string UnitPrice { get; set; }
        [JsonProperty("operationAmount")]
        public Amount OperationAmount { get; set; }
    }

    public class FundsExtendedInfo
    {
        [JsonProperty("bank")]
        public string Bank { get; set; }
        [JsonProperty("branch")]
        public string Branch { get; set; }
        [JsonProperty("accountNumber")]
        public string AccountNumber { get; set; }
        [JsonProperty("controlDigits")]
        public string controlDigits { get; set; }
        [JsonProperty("number")]
        public string Number { get; set; }
        [JsonProperty("fundName")]
        public string FundName { get; set; }
        [JsonProperty("webAlias")]
        public string WebAlias { get; set; }
        [JsonProperty("ISIN")]
        public string ISIN { get; set; }
        [JsonProperty("category")]
        public string Category { get; set; }
        [JsonProperty("unitPrice")]
        public AmountWithValueDate UnitPrice { get; set; }
    }
    #endregion

    #region Loans
    public class Loan
    {
        [JsonProperty("accountNumber")]
        public string AccountNumber { get; set; }
        [JsonProperty("accountType")]
        public string AccountType { get; set; }
        [JsonProperty("branch")]
        public string Branch { get; set; }
        [JsonProperty("controlDigits")]
        public string ControlDigits { get; set; }
        [JsonProperty("debt")]
        public Amount Debt { get; set; }
        [JsonProperty("initialBalance")]
        public Amount InitialBalance { get; set; }
        [JsonProperty("loanRates")]
        public LoanRates LoanRates { get; set; }
        [JsonProperty("period")]
        public Period Period { get; set; }
        [JsonProperty("repayment")]
        public Amount Repayment { get; set; }
        [JsonProperty("repaymentDate")]
        public string RepaymentDate { get; set; }
        [JsonProperty("revisionDate")]
        public string RevisionDate { get; set; }
        [JsonProperty("webAlias")]
        public string WebAlias { get; set; }
    }
    public class LoanRates
    {
        [JsonProperty("interest")]
        public string Interest { get; set; }
        [JsonProperty("margin")]
        public string Margin { get; set; }
        [JsonProperty("referenceIndex")]
        public string ReferenceIndex { get; set; }
        [JsonProperty("partialCancellation")]
        public string PartialCancellation { get; set; }
        [JsonProperty("totalCancellation")]
        public string TotalCancellation { get; set; }
    }
    #endregion

    #region CreditCards
    public class CreditCard
    {
        [JsonProperty("cardType")]
        public string CardType { get; set; }
        [JsonProperty("cardNumber")]
        public string CardNumber { get; set; }
        [JsonProperty("limit")]
        public Amount Limit { get; set; }
        [JsonProperty("disposed")]
        public Amount Disposed { get; set; }
        [JsonProperty("available")]
        public Amount Available { get; set; }
        [JsonProperty("expirationDate")]
        public string ExpirationDate { get; set; }
        [JsonProperty("paymentDueDate")]
        public string PaymentDueDate { get; set; }
        [JsonProperty("paymentAmount")]
        public Amount PaymentAmount { get; set; }
        [JsonProperty("billingPeriodStart")]
        public string BillingPeriodStart { get; set; }
        [JsonProperty("billingPeriodEnd")]
        public string BillingPeriodEnd { get; set; }
        [JsonProperty("webAlias")]
        public string WebAlias { get; set; }
        [JsonProperty("transactions")]
        public CreditCardTransaction[] Transactions { get; set; }
    }
    public class CreditCardTransaction
    {
        [JsonProperty("amount")]
        public Amount Amount { get; set; }
        [JsonProperty("transactionType")]
        public string TransactionType { get; set; }
        [JsonProperty("valueDate")]
        public string ValueDate { get; set; }
        [JsonProperty("description")]
        public string Description { get; set; }
        [JsonProperty("comments")]
        public string Comments { get; set; }
    }
    #endregion

    #region DebitCards
    public class DebitCard
    {
        [JsonProperty("cardType")]
        public string CardType { get; set; }
        [JsonProperty("cardNumber")]
        public string CardNumber { get; set; }
        [JsonProperty("disposed")]
        public Amount Disposed { get; set; }
        [JsonProperty("expirationDate")]
        public string ExpirationDate { get; set; }
        [JsonProperty("associatedAccount")]
        public string AssociatedAccount { get; set; }
        [JsonProperty("webAlias")]
        public string WebAlias { get; set; }
        [JsonProperty("transactions")]
        public DebitCardTransaction[] Transactions { get; set; }
    }
    public class DebitCardTransaction
    {
        [JsonProperty("amount")]
        public Amount Amount { get; set; }
        [JsonProperty("transactionType")]
        public string TransactionType { get; set; }
        [JsonProperty("valueDate")]
        public string ValueDate { get; set; }
        [JsonProperty("description")]
        public string Description { get; set; }
        [JsonProperty("comments")]
        public string Comments { get; set; }
    }
    #endregion

    #region Credits
    public class Credit
    {
        [JsonProperty("bank")]
        public string Bank { get; set; }
        [JsonProperty("branch")]
        public string Branch { get; set; }
        [JsonProperty("controlDigits")]
        public string ControlDigits { get; set; }
        [JsonProperty("account")]
        public string AccountNumber { get; set; }
        [JsonProperty("balance")]
        public Amount Balance { get; set; }
        [JsonProperty("availableBalance")]
        public Amount AvailableBalance { get; set; }
        [JsonProperty("webAlias")]
        public string WebAlias { get; set; }
        [JsonProperty("transactions")]
        public CreditTransaction[] Transactions { get; set; }
    }
    public class CreditTransaction
    {
        [JsonProperty("amount")]
        public Amount Amount { get; set; }
        [JsonProperty("transactionType")]
        public string TransactionType { get; set; }
        [JsonProperty("valueDate")]
        public string ValueDate { get; set; }
        [JsonProperty("description")]
        public string Description { get; set; }
        [JsonProperty("operationDate")]
        public string OperationDate { get; set; }
        [JsonProperty("payee")]
        public string Payee { get; set; }
        [JsonProperty("payer")]
        public string Payer { get; set; }
        [JsonProperty("reference")]
        public string Reference { get; set; }
        [JsonProperty("Comments")]
        public string Comments { get; set; }
    }
    #endregion

    #region AccountHolders
    public class AccountHolder
    {
        [JsonProperty("bank")]
        public string Bank { get; set; }
        [JsonProperty("branch")]
        public string Branch { get; set; }
        [JsonProperty("accountNumber")]
        public string AccountNumber { get; set; }
        [JsonProperty("controlDigits")]
        public string ControlDigits { get; set; }
        [JsonProperty("holders")]
        public Holder[] Holders { get; set; }
    }
    public class Holder
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("document")]
        public string Document { get; set; }
        [JsonProperty("relation")]
        public string Relation { get; set; }
        [JsonProperty("category")]
        public string Category { get; set; }
    }
    #endregion

    #region PensionPlans
    public class PensionPlan
    {
        [JsonProperty("balance")]
        public Amount Balance { get; set; }
        [JsonProperty("planName")]
        public string PlanName { get; set; }
        [JsonProperty("planNumber")]
        public string PlanNumber { get; set; }
        [JsonProperty("planPerformance")]
        public PlanPerformance PlanPerformance { get; set; }
        [JsonProperty("quantity")]
        public string Quantity { get; set; }
        [JsonProperty("startDate")]
        public string StartDate { get; set; }
        [JsonProperty("totalContribution")]
        public Amount TotalContribution { get; set; }
        [JsonProperty("transactions")]
        public PensionPlanTransaction[] Transactions { get; set; }
        [JsonProperty("unitPrice")]
        public string UnitPrice { get; set; }
        [JsonProperty("valueDate")]
        public string ValueDate { get; set; }
        [JsonProperty("webAlias")]
        public string WebAlias { get; set; }
        [JsonProperty("yearToDateContribution")]
        public Amount YearToDateContribution { get; set; }
        [JsonProperty("yield")]
        public Amount Yield { get; set; }
    }
    public class PlanPerformance
    {
        [JsonProperty("lastTwelveMonths")]
        public string LastTwelveMonths { get; set; }
        [JsonProperty("total")]
        public string Total { get; set; }
        [JsonProperty("yearToDate")]
        public string YearToDate { get; set; }
    }
    public class PensionPlanTransaction
    {
        [JsonProperty("transactionType")]
        public string TransactionType { get; set; }
        [JsonProperty("valueDate")]
        public string ValueDate { get; set; }
        [JsonProperty("description")]
        public string Description { get; set; }
        [JsonProperty("amount")]
        public Amount Amount { get; set; }
        [JsonProperty("comments")]
        public string Comments { get; set; }
        [JsonProperty("location")]
        public string Location { get; set; }
    }
    #endregion

    #region Portfolios
    public class Portfolio
    {
        [JsonProperty("portfolioNumber")]
        public string PortfolioNumber { get; set; }
        [JsonProperty("webAlias")]
        public string WebAlias { get; set; }
        [JsonProperty("currency")]
        public string Currency { get; set; }
        [JsonProperty("balance")]
        public Amount Balance { get; set; }
        [JsonProperty("portfolioType")]
        public string PortfolioType { get; set; }
        [JsonProperty("valueDate")]
        public string ValueDate { get; set; }
        [JsonProperty("performance")]
        public string Performance { get; set; }
        [JsonProperty("performanceDescription")]
        public string PerformanceDescription { get; set; }
        [JsonProperty("yield")]
        public Amount Yield { get; set; }
    }
    #endregion

    #region DirectDebits
    public class DirectDebit
    {
        [JsonProperty("IBAN")]
        public string IBAN { get; set; }
        [JsonProperty("bank")]
        public string Bank { get; set; }
        [JsonProperty("branch")]
        public string Branch { get; set; }
        [JsonProperty("account")]
        public string AccountNumber { get; set; }
        [JsonProperty("controlDigits")]
        public string ControlDigits { get; set; }
        [JsonProperty("debits")]
        public Debit[] Debits { get; set; }
    }
    public class Debit
    {
        [JsonProperty("payeename")]
        public string PayeeName { get; set; }
        [JsonProperty("payeeid")]
        public string PayeeId { get; set; }
        [JsonProperty("directdebitholder")]
        public string DirectDebitHolder { get; set; }
        [JsonProperty("status")]
        public string Status { get; set; }
        [JsonProperty("startdate")]
        public string StartDate { get; set; }
        [JsonProperty("lastpaymentdate")]
        public string LastPaymentDate { get; set; }
        [JsonProperty("lastpaymentamount")]
        public Amount LastPaymentAmount { get; set; }
        [JsonProperty("mandatereference")]
        public string MandateReference { get; set; }
        [JsonProperty("payeereference")]
        public string PayeeReference { get; set; }
        [JsonProperty("contractnumber")]
        public string ContractNumber { get; set; }
    }
    #endregion

    #region PersonalInfo
    public class PersonalInfo
    {
        [JsonProperty("userName")]
        public string UserName { get; set; }
        [JsonProperty("document")]
        public string Document { get; set; }
        [JsonProperty("birthdate")]
        public string Birthdate { get; set; }
        [JsonProperty("phoneNumbers")]
        public List<string> PhoneNumbers { get; set; }
        [JsonProperty("emails")]
        public List<string> Emails { get; set; }
        [JsonProperty("addresses")]
        public Address[] Addresses { get; set; }
    }

    public class Address
    {
        [JsonProperty("addressDescription")]
        public string AddressDescription { get; set; }
        [JsonProperty("streetAddress")]
        public string StreetAddress { get; set; }
        [JsonProperty("city")]
        public string City { get; set; }
        [JsonProperty("stateProvinceRegion")]
        public string StateProvinceRegion { get; set; }
        [JsonProperty("zipCode")]
        public string ZipCode { get; set; }
        [JsonProperty("country")]
        public string Country { get; set; }
    }
    #endregion
}