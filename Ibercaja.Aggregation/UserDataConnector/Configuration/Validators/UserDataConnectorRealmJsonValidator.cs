using System;
using System.Collections.Generic;
using FluentValidation;
using Meniga.Runtime.Extensions;

namespace Ibercaja.Aggregation.UserDataConnector.Configuration.Validators
{
    public class UserDataConnectorRealmJsonValidator : AbstractValidator<UserDataConnectorConfigurationRealmJson>
    {
        private readonly List<string> _availableProducts = new List<string>()
        {
            "Accounts",
            "CreditCards",
            "Credits",
            "DebitCards",
            "Deposits",
            "Funds",
            "Loans",
            "PensionPlans",
            "Portfolios",
            "Shares",
            "AccountHolders",
            "PersonalInfo",
            "FundsExtendedInfo",
            "DirectDebits"
        };

        private readonly List<string> _availableInvertAmounts = new List<string>()
        {
            "0", "1", "false", "true"
        };

        private readonly List<string> _availableUserIdentifier = new List<string>()
        {
            "user", "username", "id", "docNumber"
        };

        public UserDataConnectorRealmJsonValidator()
        {
            RuleForEach(x => x.ProductsToFetch).NotEmpty().WithMessage("ProductToFetch cannot be empty")
                .When(x => x.IsNotNull()).Must(x => _availableProducts.Contains(x)).WithMessage($"ProductsToFetch is not available product. List of available products: {String.Join(", ", _availableProducts)}");

            RuleFor(x => x.InvertAmount).NotEmpty().WithMessage("Connection data json doesn't contain InvertAmount")
                .When(x => x.IsNotNull()).Must(x => _availableInvertAmounts.Contains(x.ToLower()))
                .WithMessage($"InvertAmount value must be one of those: {string.Join(", ", _availableInvertAmounts)}");

            RuleFor(x => x.Bank).NotEmpty().WithMessage("Connection data json doesn't contain Bank");

            RuleFor(x => x.UserIdentifier).NotEmpty().WithMessage("Connection data json doesn't contain UserIdentifier")
                .When(x => x.IsNotNull()).Must(x => _availableUserIdentifier.Contains(x))
                .WithMessage($"UserIdentifier value must be one of those: {string.Join(", ", _availableUserIdentifier)}");

            RuleForEach(x => x.TextReplacePatterns).Must(x => !string.IsNullOrEmpty(x.Pattern)).WithMessage("TextReplacePattern.Pattern cannot be empty");
        }
    }
}
