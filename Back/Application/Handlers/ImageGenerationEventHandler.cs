using MediatR;
using Application.Events;

namespace Application.Handlers
{
    public sealed class ImageGenerationEventHandler : 
        INotificationHandler<PythonScriptFailedEvent>,
        INotificationHandler<PythonScriptFinishedEvent>
    {
        public async Task Handle(PythonScriptFailedEvent notification, CancellationToken cancellationToken)
        {
            await Console.Error.WriteLineAsync(notification.Data.ToString());
        }

        public async Task Handle(PythonScriptFinishedEvent notification, CancellationToken cancellationToken)
        {
            //handle script finished event here
            Console.WriteLine(notification.Output);
            Console.WriteLine(notification.ErrorOutput);
            Console.WriteLine(notification.ExitCode);

            await Task.Delay(10000, cancellationToken);

            Console.WriteLine("event consumed");
        }
    }
}
