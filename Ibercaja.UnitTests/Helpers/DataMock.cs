using System;
using System.Globalization;
using Meniga.Core.BusinessModels;
using Ibercaja.Aggregation.Eurobits;
using Account = Ibercaja.Aggregation.Eurobits.Account;

namespace Ibercaja.UnitTests.Helpers
{
    public static class DataMock
    {
        public static string BankName = "DummyRobot";
        public static string Data = "{\"productsToFetch\": [\"Accounts\", \"AccountHolders\", \"DebitCards\", \"CreditCards\",\"Deposits\"],\"invertAmount\":\"false\",\"bank\":\"DummyRobot\",\"userIdentifier\":\"username\",\"textReplacePatterns\":[]}";

        public static string SessionToken = "SessionTokenAccountsDepositsCreditCards";
        public static string SessionTokenAccounts = "SessionTokenAccounts";
        public static string SessionTokenDeposits = "SessionTokenDeposits";
        public static string SessionTokenCreditCards = "SessionTokenCreditCards";

        public static AggregationResponse AggregationResponseEmpty = new AggregationResponse
        {
            AggregationInfo = new AggregationInfo
            {
                Code = "R000"
            }
        };

        private static readonly Deposit[] Deposits =
            {
                new Deposit
                {
                    AccountNumber = "Deposit",
                    Balance = new Amount
                    {
                        Value = "1.0",
                        Currency = "EUR"
                    },
                    Bank = "Bank",
                    Branch = "Branch",
                    ControlDigits = "ControlDigits",
                    Duration = new Period
                    {
                        StartDate = "StartDate",
                        EndDate = "EndDate"
                    },
                    WebAlias = "WebAlias",
                    Interest = new Interest
                    {
                        Rate = "1.00",
                        Type = "TAE"
                    }
                }
        };

        private static readonly CreditCard[] CreditCards =
            {
                new CreditCard
                {
                    Available = new Amount
                    {
                        Value = "1.0",
                        Currency = "EUR"
                    },
                    Disposed = new Amount
                    {
                        Value = "1.0",
                        Currency = "EUR"
                    },
                    CardNumber = "0000-0000-0000-0000",
                    Limit = new Amount
                    {
                        Value = "1.0",
                        Currency = "EUR"
                    },
                    WebAlias = "WebAlias",
                    Transactions = new CreditCardTransaction[]
                    {
                        new CreditCardTransaction
                        {
                            Amount = new Amount
                            {
                                Value = "1.0",
                                Currency = "EUR"
                            },
                            Comments = "00000000000000000 - 1234",
                            Description = "Description",
                            TransactionType = "1",
                            ValueDate = DateTime.Today.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture)
                        }
                    }
                }
        };

        private static readonly Account[] Accounts =
            {
                new Account
                {
                    AccountNumber = "Account",
                    Balance = new Amount
                    {
                        Value = "1.0",
                        Currency = "EUR"
                    },
                    Bank = "Bank",
                    Branch = "Branch",
                    ControlDigits = "ControlDigits",
                    Transactions = new AccountTransaction[]
                    {
                        new AccountTransaction
                        {
                            Amount = new Amount
                            {
                                Value = "10.0",
                                Currency = "EUR"
                            },
                            Balance = new Amount
                            {
                                Value = "100.0",
                                Currency = "EUR"
                            },
                            OperationDate = "15/01/2019",
                            ValueDate = "15/01/2019",
                            Reference = "Duplicated Test Reference",
                            Description = "Transaction Description 1"
                        },
                        new AccountTransaction
                        {
                            Amount = new Amount
                            {
                                Value = "20.0",
                                Currency = "EUR"
                            },
                            Balance = new Amount
                            {
                                Value = "90.0",
                                Currency = "EUR"
                            },
                            OperationDate = "15/01/2019",
                            ValueDate = "15/01/2019",
                            Reference = "Duplicated Test Reference",
                            Description = "Transaction Description 2"
                        }
                    },
                    WebAlias = "WebAlias"
                }
        };

        public static AggregationResponse AggregationResponse = new AggregationResponse
        {
            AggregationInfo = new AggregationInfo
            {
                Code = "R000"
            },
            Accounts = Accounts,
            Deposits = Deposits,
            CreditCards = CreditCards
        };

        public static AggregationResponse AggregationResponseOnlyAccounts = new AggregationResponse
        {
            AggregationInfo = new AggregationInfo
            {
                Code = "R000"
            },
            Accounts = Accounts
        };

        public static AggregationResponse AggregationResponseOnlyDeposits = new AggregationResponse
        {
            AggregationInfo = new AggregationInfo
            {
                Code = "R000"
            },
            Deposits = Deposits
        };

        public static AggregationResponse AggregationResponseOnlyCreditCards = new AggregationResponse
        {
            AggregationInfo = new AggregationInfo
            {
                Code = "R000"
            },
            CreditCards = CreditCards
        };

        public static RealmUser RealmUser = new RealmUser
        {
            PersonId = 111,
            RealmId = 1,
            Id = 1,
            UserId = 111,
            UserIdentifier = "1234567890"
        };

        public static Parameter[] LoginParameters =
        {
            new Parameter
            {
                Name = "User",
                Value = "1234567890"
            }
        };
    }
}
