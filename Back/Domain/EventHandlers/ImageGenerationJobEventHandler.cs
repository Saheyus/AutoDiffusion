using Domain.Entities;
using Domain.Events;
using Domain.Notifications;
using Domain.Ports;
using MediatR;

namespace Domain.EventHandlers
{
    public sealed class ImageGenerationJobEventHandler : 
        INotificationHandler<ImageGenerationJobFailedEvent>,
        INotificationHandler<ImageGenerationJobFinishedEvent>,
        INotificationHandler<ImageGenerationJobStartedEvent>
    {
        private readonly IImageGenerationJobRepository _imageGenerationJobRepository;
        private readonly IPublisher _publisher;

        public ImageGenerationJobEventHandler(IImageGenerationJobRepository imageGenerationJobRepository, IPublisher publisher)
        {
            _imageGenerationJobRepository = imageGenerationJobRepository;
            _publisher = publisher;
        }

        public async Task Handle(ImageGenerationJobFailedEvent @event, CancellationToken cancellationToken = default)
        {
            var imageGenerationJob = await _imageGenerationJobRepository.GetAsync(@event.ImageGenerationJobId, cancellationToken);

            imageGenerationJob.ChangeState(ImageGenerationJobStates.Failed);

            await _imageGenerationJobRepository.SaveAsync(imageGenerationJob, cancellationToken);
        }

        public async Task Handle(ImageGenerationJobFinishedEvent @event, CancellationToken cancellationToken = default)
        {
            //handle script finished event here
            //parse output, save state & create notification event for notification handler

            Console.WriteLine(@event.Output);
            Console.WriteLine(@event.ErrorOutput);
            Console.WriteLine(@event.ExitCode);

            var imageGenerationJob = await _imageGenerationJobRepository.GetAsync(@event.ImageGenerationJobId, cancellationToken);
           
            imageGenerationJob.ChangeState(ImageGenerationJobStates.Finished);
            imageGenerationJob.AddImageUri(new Uri("file:///c:/whatever.png"));

            await _imageGenerationJobRepository.SaveAsync(imageGenerationJob, cancellationToken);

            //Publish notification
            await _publisher.Publish(new ImageGenerationJobNotification(imageGenerationJob), cancellationToken);
        }

        public async Task Handle(ImageGenerationJobStartedEvent @event, CancellationToken cancellationToken = default)
        {
            var imageGenerationJob = await _imageGenerationJobRepository.GetAsync(@event.ImageGenerationJobId, cancellationToken);

            imageGenerationJob.ChangeState(ImageGenerationJobStates.Running);

            await _imageGenerationJobRepository.SaveAsync(imageGenerationJob, cancellationToken);
        }
    }
}
