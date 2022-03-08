namespace Ibercaja.Utils.Config
{
    public interface IIbercajaConfiguration
    {
        string NotificationsHubUrl { get; set; }
        int JobsMaxProcessingThreads { get; set; }
    }
}