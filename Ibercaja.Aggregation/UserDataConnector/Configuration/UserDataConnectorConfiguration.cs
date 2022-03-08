using System;
using FluentValidation;
using FluentValidation.Results;
using log4net;
using Newtonsoft.Json;
using Ibercaja.Aggregation.UserDataConnector.Configuration.Validators;

namespace Ibercaja.Aggregation.UserDataConnector.Configuration
{
    public class UserDataConnectorConfiguration : IUserDataConnectorConfiguration
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(UserDataConnectorConfiguration));

        private UserDataConnectorConfigurationRealmJson _userDataConnectorConfigurationRealmJson;
        public bool DeserializedSuccessfully { get; private set; }

        public UserDataConnectorConfiguration()
        {
            DeserializedSuccessfully = false;
        }

        public bool TryDeserializeConfigurationFromJson(string connectionData)
        {
            if (string.IsNullOrEmpty(connectionData))
            {
                Logger.Error("UserDataConnector connection data cannot be empty.");
                throw new ArgumentException("UserDataConnector connection data cannot be empty.");
            }

            try
            {
                connectionData = connectionData.Trim();
                _userDataConnectorConfigurationRealmJson = JsonConvert.DeserializeObject<UserDataConnectorConfigurationRealmJson>(connectionData);
                DeserializedSuccessfully = true;
                return DeserializedSuccessfully;
            }

            catch (JsonSerializationException ex)
            {
                Logger.Error($"UserDataConnector connection data is not valid json. Check Inputted data: {connectionData}", ex);
                throw;
            }
        }

        public UserDataConnectorConfigurationRealm GetValidatedConfiguration()
        {
            if (!DeserializedSuccessfully)
            {
                throw new Exception("Configuration deserialization wasn't invoked or wasn't completed.");
            }

            UserDataConnectorRealmJsonValidator validator = new UserDataConnectorRealmJsonValidator();
            ValidationResult validationResult = validator.Validate(_userDataConnectorConfigurationRealmJson);

            if (validationResult.IsValid)
            {
                UserDataConnectorConfigurationRealm userDataConnectorConfigurationRealm = new UserDataConnectorConfigurationRealm(_userDataConnectorConfigurationRealmJson);
                return userDataConnectorConfigurationRealm;
            }

            throw new ValidationException(validationResult.Errors);
        }
    }
}
