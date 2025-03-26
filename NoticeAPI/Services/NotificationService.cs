
using FirebaseAdmin.Messaging;

namespace NoticeAPI.Services
{
    public class FirebaseNotificationService : INotificationService
    {
        public async Task SendNotificationAsync(string token, string title, string body)
        {
            var message = new Message
            {
                Token = token,
                Notification = new Notification
                {
                    Title = title,
                    Body = body
                }
            };

            string result = await FirebaseMessaging.DefaultInstance.SendAsync(message);
            Console.WriteLine($"Notification sent: {result}");
        }

        public async Task SendTopicNotificationAsync(string topic, string title, string body)
        {
            var message = new Message
            {
                Topic = topic,
                Notification = new Notification
                {
                    Title = title,
                    Body = body
                }
            };

            string result = await FirebaseMessaging.DefaultInstance.SendAsync(message);
            Console.WriteLine($"Topic notification sent: {result}");
        }
    }
}

