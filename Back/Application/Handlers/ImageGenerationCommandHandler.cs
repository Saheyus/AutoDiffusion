using Application.Commands;
using Application.Ports;
using Domain.Entities;
using Infrastructure.Ports;
using MediatR;
using Microsoft.Extensions.Caching.Memory;

namespace Application.Handlers
{
    public class ImageGenerationCommandHandler : IRequestHandler<CreateImageGenerationCommand, ImageGeneration>
    {
        private readonly IMemoryCache _cache;
        private readonly TimeSpan _cacheSlidingExpiration = TimeSpan.FromMinutes(10);
        private readonly IImageGenerationQueue _queue;
        private readonly IImageGenerationRepository _imageGenerationRepository;

        public ImageGenerationCommandHandler(IMemoryCache cache,
            IImageGenerationQueue queue, 
            IImageGenerationRepository imageGenerationRepository)
        {
            _cache = cache;
            _queue = queue;
            _imageGenerationRepository = imageGenerationRepository;
        }

        public async Task<ImageGeneration> Handle(CreateImageGenerationCommand command, CancellationToken cancellationToken = default)
        {
            var imageGeneration = new ImageGeneration(command.Id, command.Prompt);
         
            await _imageGenerationRepository.SaveAsync(imageGeneration, cancellationToken);
          
            _cache.Set(imageGeneration.Id, imageGeneration, new MemoryCacheEntryOptions
            {
                SlidingExpiration = _cacheSlidingExpiration,
                Priority = CacheItemPriority.High
            });

            await _queue.EnqueueAsync(imageGeneration, cancellationToken);

            return imageGeneration;
        }
    }
}
