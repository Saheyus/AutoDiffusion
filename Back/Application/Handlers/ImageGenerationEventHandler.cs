using MediatR;
using Application.Events;
using Domain.Entities;
using Infrastructure.Ports;
using Microsoft.Extensions.Caching.Memory;

namespace Application.Handlers
{
    public sealed class ImageGenerationEventHandler : 
        INotificationHandler<PythonScriptFailedEvent>,
        INotificationHandler<PythonScriptFinishedEvent>,
        INotificationHandler<PythonScriptStartedEvent>
    {
        private readonly IMemoryCache _cache;
        private readonly IImageGenerationRepository _imageGenerationRepository;
        private readonly TimeSpan _cacheSlidingExpiration = TimeSpan.FromMinutes(10);

        public ImageGenerationEventHandler(IMemoryCache cache, IImageGenerationRepository imageGenerationRepository)
        {
            _cache = cache;
            _imageGenerationRepository = imageGenerationRepository;
        }

        public async Task Handle(PythonScriptFailedEvent notification, CancellationToken cancellationToken = default)
        {
            var imageGeneration = await GetFromCacheOrRepositoryAsync(notification.ImageGenerationId, cancellationToken);

            imageGeneration.ChangeState(ImageGenerationStates.Failed);

            await _imageGenerationRepository.SaveAsync(imageGeneration, cancellationToken);

            _cache.Set(imageGeneration.Id, imageGeneration, new MemoryCacheEntryOptions
            {
                SlidingExpiration = _cacheSlidingExpiration,
                Priority = CacheItemPriority.Low
            });
        }

        public async Task Handle(PythonScriptFinishedEvent notification, CancellationToken cancellationToken = default)
        {
            //handle script finished event here
            //parse output, save state & create notification event for notification handler

            Console.WriteLine(notification.Output);
            Console.WriteLine(notification.ErrorOutput);
            Console.WriteLine(notification.ExitCode);

            await Task.Delay(10000, cancellationToken);
          
            var imageGeneration = await GetFromCacheOrRepositoryAsync(notification.ImageGenerationId, cancellationToken);

            imageGeneration.ChangeState(ImageGenerationStates.Finished);

            await _imageGenerationRepository.SaveAsync(imageGeneration, cancellationToken);

            _cache.Set(imageGeneration.Id, imageGeneration, new MemoryCacheEntryOptions
            {
                SlidingExpiration = _cacheSlidingExpiration,
                Priority = CacheItemPriority.Normal
            });
        }

        public async Task Handle(PythonScriptStartedEvent notification, CancellationToken cancellationToken = default)
        {
            var imageGeneration = await GetFromCacheOrRepositoryAsync(notification.ImageGenerationId, cancellationToken);

            imageGeneration.ChangeState(ImageGenerationStates.Running);

            await _imageGenerationRepository.SaveAsync(imageGeneration, cancellationToken);

            _cache.Set(imageGeneration.Id, imageGeneration, new MemoryCacheEntryOptions
            {
                SlidingExpiration = _cacheSlidingExpiration,
                Priority = CacheItemPriority.High
            });
        }

        private async Task<ImageGeneration> GetFromCacheOrRepositoryAsync(Guid imageGenerationId, CancellationToken cancellationToken = default)
        {
            if (!_cache.TryGetValue(imageGenerationId, out ImageGeneration? imageGeneration) || imageGeneration == null)
            {
                imageGeneration = await _imageGenerationRepository.GetAsync(imageGenerationId, cancellationToken);
            }
            if (imageGeneration == null)
                throw new NullReferenceException($"Cannot find generation {imageGenerationId} from repository! Inconsistent state between repository and application!");

            return imageGeneration;
        }
    }
}
