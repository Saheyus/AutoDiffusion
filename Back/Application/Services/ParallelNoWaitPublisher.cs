using MediatR;

namespace Application.Services
{
    // https://github.com/jbogard/MediatR/blob/master/samples/MediatR.Examples.PublishStrategies/Publisher.cs#L77
    public class ParallelNoWaitPublisher : INotificationPublisher
    {
        public Task Publish(IEnumerable<NotificationHandlerExecutor> handlerExecutors, INotification notification, CancellationToken cancellationToken)
        {
            foreach (var handler in handlerExecutors)
            {
                Task.Run(() => handler.HandlerCallback(notification, cancellationToken), cancellationToken);
            }

            return Task.CompletedTask;
        }
    }
}
