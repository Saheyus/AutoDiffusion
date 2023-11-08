using MediatR;
using Application.Events;
using Domain.Entities;
using Infrastructure.Ports;
using Microsoft.Extensions.Caching.Memory;

namespace Application.Handlers
{
    public sealed class ImageGenerationJobEventHandler : 
        INotificationHandler<ImageGenerationJobFailedEvent>,
        INotificationHandler<ImageGenerationJobFinishedEvent>,
        INotificationHandler<ImageGenerationJobStartedEvent>
    {
        private readonly IMemoryCache _cache;
        private readonly IImageGenerationJobRepository _imageGenerationJobRepository;
        private readonly TimeSpan _cacheSlidingExpiration = TimeSpan.FromMinutes(10);

        public ImageGenerationJobEventHandler(IMemoryCache cache, IImageGenerationJobRepository imageGenerationJobRepository)
        {
            _cache = cache;
            _imageGenerationJobRepository = imageGenerationJobRepository;
        }

        public async Task Handle(ImageGenerationJobFailedEvent notification, CancellationToken cancellationToken = default)
        {
            var imageGenerationJob = await GetFromCacheOrRepositoryAsync(notification.ImageGenerationJobId, cancellationToken);

            imageGenerationJob.ChangeState(ImageGenerationJobStates.Failed);

            await _imageGenerationJobRepository.SaveAsync(imageGenerationJob, cancellationToken);

            _cache.Set(imageGenerationJob.Id, imageGenerationJob, new MemoryCacheEntryOptions
            {
                SlidingExpiration = _cacheSlidingExpiration,
                Priority = CacheItemPriority.Low
            });
        }

        public async Task Handle(ImageGenerationJobFinishedEvent notification, CancellationToken cancellationToken = default)
        {
            //handle script finished event here
            //parse output, save state & create notification event for notification handler

            Console.WriteLine(notification.Output);
            Console.WriteLine(notification.ErrorOutput);
            Console.WriteLine(notification.ExitCode);

            await Task.Delay(10000, cancellationToken);
          
            var imageGenerationJob = await GetFromCacheOrRepositoryAsync(notification.ImageGenerationJobId, cancellationToken);

            imageGenerationJob.ChangeState(ImageGenerationJobStates.Finished);

            await _imageGenerationJobRepository.SaveAsync(imageGenerationJob, cancellationToken);

            _cache.Set(imageGenerationJob.Id, imageGenerationJob, new MemoryCacheEntryOptions
            {
                SlidingExpiration = _cacheSlidingExpiration,
                Priority = CacheItemPriority.Normal
            });
        }

        public async Task Handle(ImageGenerationJobStartedEvent notification, CancellationToken cancellationToken = default)
        {
            var imageGeneration = await GetFromCacheOrRepositoryAsync(notification.ImageGenerationJobId, cancellationToken);

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
