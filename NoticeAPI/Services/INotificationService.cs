namespace NoticeAPI.Services
{
    public interface INotificationService
    {
        Task SendNotificationAsync(string token, string title, string body);
        Task SendTopicNotificationAsync(string topic, string title, string body);
    }
}
