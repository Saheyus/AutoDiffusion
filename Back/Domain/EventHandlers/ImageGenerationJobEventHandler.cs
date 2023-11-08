using Domain.Entities;
using Domain.Events;
using Domain.Notifications;
using Domain.Ports;
using MediatR;
using Microsoft.Extensions.Caching.Memory;

namespace Domain.EventHandlers
{
    public sealed class ImageGenerationJobEventHandler : 
        INotificationHandler<ImageGenerationJobFailedEvent>,
        INotificationHandler<ImageGenerationJobFinishedEvent>,
        INotificationHandler<ImageGenerationJobStartedEvent>
    {
        private readonly IMemoryCache _cache;
        private readonly IImageGenerationJobRepository _imageGenerationJobRepository;
        private readonly IPublisher _publisher;
        private readonly TimeSpan _cacheSlidingExpiration = TimeSpan.FromMinutes(10);

        public ImageGenerationJobEventHandler(IMemoryCache cache, IImageGenerationJobRepository imageGenerationJobRepository, IPublisher publisher)
        {
            _cache = cache;
            _imageGenerationJobRepository = imageGenerationJobRepository;
            _publisher = publisher;
        }

        public async Task Handle(ImageGenerationJobFailedEvent @event, CancellationToken cancellationToken = default)
        {
            var imageGenerationJob = await GetFromCacheOrRepositoryAsync(@event.ImageGenerationJobId, cancellationToken);

            imageGenerationJob.ChangeState(ImageGenerationJobStates.Failed);

            await _imageGenerationJobRepository.SaveAsync(imageGenerationJob, cancellationToken);

            _cache.Set(imageGenerationJob.Id, imageGenerationJob, new MemoryCacheEntryOptions
            {
                SlidingExpiration = _cacheSlidingExpiration,
                Priority = CacheItemPriority.Low
            });
        }

        public async Task Handle(ImageGenerationJobFinishedEvent @event, CancellationToken cancellationToken = default)
        {
            //handle script finished event here
            //parse output, save state & create notification event for notification handler

            Console.WriteLine(@event.Output);
            Console.WriteLine(@event.ErrorOutput);
            Console.WriteLine(@event.ExitCode);

            var imageGenerationJob = await GetFromCacheOrRepositoryAsync(@event.ImageGenerationJobId, cancellationToken);

            imageGenerationJob.ChangeState(ImageGenerationJobStates.Finished);
            imageGenerationJob.AddImageUri(new Uri("file:///c:/whatever.png"));

            await _imageGenerationJobRepository.SaveAsync(imageGenerationJob, cancellationToken);

            _cache.Set(imageGenerationJob.Id, imageGenerationJob, new MemoryCacheEntryOptions
            {
                SlidingExpiration = _cacheSlidingExpiration,
                Priority = CacheItemPriority.Normal
            });

            //Publish notification
            await _publisher.Publish(new ImageGenerationJobNotification(imageGenerationJob), cancellationToken);
        }

        public async Task Handle(ImageGenerationJobStartedEvent @event, CancellationToken cancellationToken = default)
        {
            var imageGeneration = await GetFromCacheOrRepositoryAsync(@event.ImageGenerationJobId, cancellationToken);

            imageGeneration.ChangeState(ImageGenerationJobStates.Running);

            await _imageGenerationJobRepository.SaveAsync(imageGeneration, cancellationToken);

            _cache.Set(imageGeneration.Id, imageGeneration, new MemoryCacheEntryOptions
            {
                SlidingExpiration = _cacheSlidingExpiration,
                Priority = CacheItemPriority.High
            });
        }

        private async Task<ImageGenerationJob> GetFromCacheOrRepositoryAsync(Guid imageGenerationJobId, CancellationToken cancellationToken = default)
        {
            if (!_cache.TryGetValue(imageGenerationJobId, out ImageGenerationJob? imageGenerationJob) || imageGenerationJob == null)
            {
                imageGenerationJob = await _imageGenerationJobRepository.GetAsync(imageGenerationJobId, cancellationToken);
            }

            if (imageGenerationJob == null)
                throw new NullReferenceException($"Cannot find generation {imageGenerationJobId} from repository! Inconsistent state between repository and application!");

            return imageGenerationJob;
        }
    }
}
