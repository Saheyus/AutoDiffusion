using Domain.Notifications;
using MediatR;

namespace Endpoints.Services
{
    public sealed class NotificationsHandler : INotificationHandler<ImageGenerationJobNotification>
    {
        public Task Handle(ImageGenerationJobNotification notification, CancellationToken cancellationToken)
        {
            //TODO call front proj controller
            //make http call
            
            throw new NotImplementedException();
        }
    }
}
