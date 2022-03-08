namespace Ibercaja.Aggregation.UserDataConnector.Configuration
{
    public interface IUserDataConnectorConfiguration
    {
        bool TryDeserializeConfigurationFromJson(string connectionData);
        UserDataConnectorConfigurationRealm GetValidatedConfiguration();
    }
}