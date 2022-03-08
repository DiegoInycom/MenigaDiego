using System.Collections.Generic;
using System.Globalization;
using log4net;
using Meniga.Core.BusinessModels;
using Ibercaja.Aggregation.Eurobits;

namespace Ibercaja.Aggregation.Products.PensionPlans
{
    public class PensionPlanAccountProvider : IAccountsProvider
    {
        private const string PensionPlanAccountFlag = "PensionPlanAccount";
        private const string PensionPlanTotalPerformance = "PensionPlanTotalPerformance";
        private const string PensionPlanLast12Performance = "PensionPlanLast12Performance";
        private const string PensionPlanYearToDatePerformance = "PensionPlanYearToDatePerformance";
        private const string PensionPlanQuantity = "PensionPlanQuantity";
        private const string PensionPlanStartDate = "PensionPlanStartDate";
        private const string PensionPlanName = "PensionPlanName";
        private const string PensionPlanTotalContributionDate = "PensionPlanTotalContributionDate";
        private const string PensionPlanYield = "PensionPlanYield";
        private static readonly ILog Logger = LogManager.GetLogger(typeof(PensionPlanAccountProvider));
        private readonly IAggregationService _aggregationService;
        private const string Relationship = "Relationship0";
        private readonly string _userDocument;


        public PensionPlanAccountProvider(IAggregationService aggregationService, string userDocument)
        {
            _aggregationService = aggregationService;
            _userDocument = userDocument;
        }

        public IEnumerable<BankAccountInfo> GetBankAccountInfos()
        {
            var pensionPlans = _aggregationService.GetPensionPlans();

            foreach (var pensionPlanAccount in pensionPlans)
            {
                decimal amount;
                if (decimal.TryParse(pensionPlanAccount.Balance.Value, NumberStyles.Currency, CultureInfo.InvariantCulture, out amount))
                {
                    var pensionPlan = new BankAccountInfo
                    {
                        AccountCategory = AccountCategoryEnum.Asset,
                        AccountCategoryDetails = IbercajaProducts.PensionPlan,
                        AccountIdentifier = pensionPlanAccount.PlanNumber,
                        Balance = amount,
                        CurrencyCode = pensionPlanAccount.Balance.Currency,
                        Limit = 0,
                        Name = GetAccountName(pensionPlanAccount),
                        AccountParameters = new List<KeyValuePair<string, string>>
                        {
                            new KeyValuePair<string, string>(
                                PensionPlanAccountFlag,
                                "true"),
                            new KeyValuePair<string, string>(
                                PensionPlanName,
                                pensionPlanAccount.PlanName),
                            new KeyValuePair<string, string>(
                                PensionPlanStartDate,
                                pensionPlanAccount.StartDate),
                            new KeyValuePair<string, string>(
                                PensionPlanTotalContributionDate,
                                $"{pensionPlanAccount.TotalContribution.Value}{pensionPlanAccount.TotalContribution.Currency}"),
                            new KeyValuePair<string, string>(
                                PensionPlanYield,
                                $"{pensionPlanAccount.Yield.Value}{pensionPlanAccount.Yield.Currency}"),
                            new KeyValuePair<string, string>(
                                PensionPlanTotalPerformance,
                                pensionPlanAccount.PlanPerformance.Total),
                            new KeyValuePair<string, string>(
                                PensionPlanLast12Performance,
                                pensionPlanAccount.PlanPerformance.LastTwelveMonths),
                            new KeyValuePair<string, string>(
                                PensionPlanYearToDatePerformance,
                                pensionPlanAccount.PlanPerformance.YearToDate),
                            new KeyValuePair<string, string>(
                                PensionPlanQuantity,
                                pensionPlanAccount.Quantity),
                            new KeyValuePair<string, string>(
                                Relationship, 
                                ExtractRelation(_userDocument))
                        }
                    };
                    yield return pensionPlan;
                }
            }
        }

        private static string GetAccountName(PensionPlan pensionPlanAccount)
        {
            return string.IsNullOrEmpty(pensionPlanAccount.WebAlias) ? pensionPlanAccount.PlanName : pensionPlanAccount.WebAlias;
        }

        private string ExtractRelation(string userDocument)
        {
            var document = _aggregationService.GetPersonalInfo()?.Document;

            if (string.IsNullOrWhiteSpace(document) || string.IsNullOrWhiteSpace(userDocument))
            {
                return "Unknown";
            }
            else
            {
                return userDocument.Contains(document) ? "Titular" : "Unknown";
            }
        }
    }
}