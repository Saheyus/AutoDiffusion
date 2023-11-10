using Domain.Events;
using MediatR;

namespace Domain.Notifications
{
    public class ImageGenerationJobNotification : INotification
    {
        public DateTime DateTime { get; }
        public string NotificationType { get; }
        public object Data { get; }

        public ImageGenerationJobNotification(ImageGenerationJobEvent @event)
        {
            NotificationType = @event.Name;
            DateTime = DateTime.UtcNow;
            Data = @event;
        }
    }
}
