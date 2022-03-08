namespace Ibercaja.UserEvents.Notifications
{
    /// <summary>
    ///     Encapsulates required dependencies for all User Events construction
    /// </summary>
    public interface INotificationProviderFactory
    {
        INotificationProvider CreateNotificationProvider(string userEventTypeId);
    }
}