using System.Threading.Tasks;

namespace Ibercaja.UserEvents.Iberfabric
{
    public interface INotificationService 
    {
        Task<bool> SendNotification(Notification notification);
    }
}