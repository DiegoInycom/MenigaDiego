using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using log4net;
using Meniga.Core.Accounts;
using Meniga.Core.BusinessModels;
using Meniga.Core.TransactionsEngine;
using Meniga.Core.Users;
using Meniga.Runtime.Auth;
using Meniga.Runtime.Services;

namespace Ibercaja.Authentication
{
    public class IbercajaAuthorizationProvider : IMenigaAuthorizationProvider
    {
        private static readonly ILog _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly ICoreUserManager _userManager;
        private readonly IAccountSetupCache _accountSetupCache;

        private static CreateRealmUserInfo RealmUserInfo = new CreateRealmUserInfo
        {
            Culture = "es-ES",
            IsInitialSetupDone = false
        };

        public IbercajaAuthorizationProvider(ICoreUserManager userManager, IAccountSetupCache accountSetupCache)
        {
            _userManager = userManager;
            _accountSetupCache = accountSetupCache;
        }

        public MenigaServiceContext Authorize(HttpRequestMessage request)
        {
            const string customerNumberHeaderName = "NICI";
            if (!request.Headers.Contains(customerNumberHeaderName))
            {
                return null;
            }

            var customerNumber = request.Headers.GetValues(customerNumberHeaderName).FirstOrDefault();

            var personInfo = _userManager.GetUserInfoByUserIdentifierAndRealm(customerNumber, "Ibercaja", false, null);
            if (personInfo == null)
            {
                //the NICI (Identifier customer at Ibercaja doesn't exist. Return null
                return null;
                //// Create the user, relying on Microsoft to have already authenticated the user before he enters Meniga
                //var realm = _accountSetupCache.GetRealm("Ibercaja");
                //var realmUser = _userManager.GetOrCreateUserForRealmUser(realm.Id, customerNumber, 1, RealmUserInfo);
                //personInfo = _userManager.GetPersonInfo(realmUser.PersonId);
            }

            var context = new MenigaServiceContext();
            context.UserId = personInfo.UserId;
            context.PersonId = personInfo.PersonId;
            context.Culture = personInfo.Culture ?? "es-ES";

            MenigaServiceContext.Current = context;
            if (context.Culture.Length == 5)
            {
                var cultureInfo = CultureInfo.GetCultureInfo(context.Culture);

                Thread.CurrentThread.CurrentUICulture = cultureInfo;
                Thread.CurrentThread.CurrentCulture = cultureInfo;
            }

#if DEBUG
            _logger.Debug("Found customer with number:" + customerNumber + " , authorizing");
#endif

            return new MenigaServiceContext { PersonId = personInfo.PersonId, UserId = personInfo.UserId };
        }

        public void Initialize(IDictionary<string, string> parameters)
        {

        }
    }
}