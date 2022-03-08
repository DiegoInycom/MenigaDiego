using System;
using System.Collections.Generic;
using Meniga.Core.BusinessModels;
using Ibercaja.Aggregation.Eurobits;
using Ibercaja.Aggregation.Products.CreditCards;
using Ibercaja.Aggregation.Products.Current;
using Ibercaja.Aggregation.Products.Deposits;
using Ibercaja.Aggregation.Products.Funds;
using Ibercaja.Aggregation.Products.Loans;
using Ibercaja.Aggregation.Products.Mortgages;
using Ibercaja.Aggregation.Products.PensionPlans;
using Ibercaja.Aggregation.Products.Shares;
using Ibercaja.Aggregation.Products.Unknown;
using Ibercaja.Aggregation.UserDataConnector.Configuration;

namespace Ibercaja.Aggregation.Products
{
    /// <summary>
    /// Contains mapping between Meniga account types and Ibercaja Product portfolio
    /// Creates all available product providers
    /// </summary>
    public class IbercajaProductProviderFactory : IProductProviderFactory
    {
        private readonly UserDataConnectorConfigurationRealm _configurationRealm;
        private readonly IAggregationService _aggregationService;
        private readonly IDictionary<string, string> _invertAmountConfiguration;
        private readonly string _userDocument;

        public IbercajaProductProviderFactory(
            UserDataConnectorConfigurationRealm configurationRealm,
            IDictionary<string, string> invertAmountConfiguration,
            IAggregationService aggregationService,
            string userDocument
            )
        {
            _invertAmountConfiguration = invertAmountConfiguration;
            _configurationRealm = configurationRealm;
            _aggregationService = aggregationService;
            _userDocument = userDocument;
        }

        public virtual ITransactionsProvider GetTransactionsProvider(AccountCategoryEnum accountCategory,
            string accountCategoryDetail)
        {
            ITransactionsProvider provider = new UnknownTransactionsProvider();

            switch (accountCategory)
            {
                case AccountCategoryEnum.Unknown:
                    break;
                case AccountCategoryEnum.Current:
                    provider = new CurrentTransactionsProvider(_aggregationService, _configurationRealm);
                    break;
                case AccountCategoryEnum.Savings:
                    provider = new DepositTransactionsProvider(_aggregationService);
                    break;
                case AccountCategoryEnum.Credit:
                    provider = new CreditCardTransactionsProvider(_aggregationService, _configurationRealm, _invertAmountConfiguration);
                    break;
                case AccountCategoryEnum.Loan:
                    switch (accountCategoryDetail)
                    {
                        case IbercajaProducts.Loan:
                            provider = new LoanTransactionsProvider(_aggregationService);
                            break;
                        case IbercajaProducts.Mortgage:
                            provider = new MortgageTransactionsProvider(_aggregationService);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(accountCategoryDetail), accountCategoryDetail, null);
                    }
                    break;
                case AccountCategoryEnum.Wallet:
                    break;
                case AccountCategoryEnum.Manual:
                    break;
                case AccountCategoryEnum.Asset:
                    switch (accountCategoryDetail)
                    {
                        case IbercajaProducts.PensionPlan:
                            provider = new PensionPlanTransactionsProvider(_aggregationService, _configurationRealm);
                            break;
                        case IbercajaProducts.Fund:
                            provider = new FundTransactionsProvider(_aggregationService, _configurationRealm);
                            break;
                        case IbercajaProducts.Share:
                            provider = new ShareTransactionsProvider(_aggregationService, _configurationRealm);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(accountCategoryDetail), accountCategoryDetail, null);
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(accountCategory), accountCategory, null);
            }

            return provider;
        }

        public IAccountsProvider GetAccountsProvider(AccountCategoryEnum accountCategory, string accountCategoryDetail)
        {
            IAccountsProvider provider = new UnknownAccountProvider();

            switch (accountCategory)
            {
                case AccountCategoryEnum.Unknown:
                    break;
                case AccountCategoryEnum.Current:
                    provider = new CurrentAccountProvider(_aggregationService, _userDocument);
                    break;
                case AccountCategoryEnum.Credit:
                    provider = new CreditCardAccountProvider(_aggregationService, _userDocument);
                    break;
                case AccountCategoryEnum.Savings:
                    provider = new DepositAccountProvider(_aggregationService, _userDocument);
                    break;
                case AccountCategoryEnum.Loan:
                    switch (accountCategoryDetail)
                    {
                        case IbercajaProducts.Loan:
                            provider = new LoanAccountProvider(_aggregationService, _userDocument);
                            break;
                        case IbercajaProducts.Mortgage:
                            provider = new MortgageAccountProvider(_aggregationService, _userDocument);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(accountCategoryDetail), accountCategoryDetail, null);
                    }
                    break;
                case AccountCategoryEnum.Wallet:
                    break;
                case AccountCategoryEnum.Manual:
                    break;
                case AccountCategoryEnum.Asset:
                    switch (accountCategoryDetail)
                    {
                        case IbercajaProducts.PensionPlan:
                            provider = new PensionPlanAccountProvider(_aggregationService, _userDocument);
                            break;
                        case IbercajaProducts.Fund:
                            provider = new FundAccountProvider(_aggregationService, _userDocument);
                            break;
                        case IbercajaProducts.Share:
                            provider = new ShareAccountProvider(_aggregationService, _userDocument);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(accountCategoryDetail), accountCategoryDetail, null);
                    }

                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(accountCategory), accountCategory, null);
            }

            return provider;
        }

        public IEnumerable<IAccountsProvider> GetAllAccountsProviders()
        {
            yield return new CurrentAccountProvider(_aggregationService, _userDocument);
            yield return new DepositAccountProvider(_aggregationService, _userDocument);
            yield return new CreditCardAccountProvider(_aggregationService, _userDocument);
            yield return new FundAccountProvider(_aggregationService, _userDocument);
            yield return new MortgageAccountProvider(_aggregationService, _userDocument);
            yield return new LoanAccountProvider(_aggregationService, _userDocument);
            yield return new PensionPlanAccountProvider(_aggregationService, _userDocument);
            yield return new ShareAccountProvider(_aggregationService, _userDocument);
        }
    }
}