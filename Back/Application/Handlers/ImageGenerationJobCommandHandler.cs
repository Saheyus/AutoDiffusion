using Application.Commands;
using Application.Ports;
using Domain.Entities;
using Infrastructure.Ports;
using MediatR;
using Microsoft.Extensions.Caching.Memory;

namespace Application.Handlers
{
    public class ImageGenerationJobCommandHandler : IRequestHandler<CreateImageGenerationJobCommand, ImageGenerationJob>
    {
        private readonly IMemoryCache _cache;
        private readonly TimeSpan _cacheSlidingExpiration = TimeSpan.FromMinutes(10);
        private readonly IImageGenerationCommandQueue _commandQueue;
        private readonly IImageGenerationJobRepository _imageGenerationRepository;

        public ImageGenerationJobCommandHandler(IMemoryCache cache,
            IImageGenerationCommandQueue commandQueue, 
            IImageGenerationJobRepository imageGenerationRepository)
        {
            _cache = cache;
            _commandQueue = commandQueue;
            _imageGenerationRepository = imageGenerationRepository;
        }

        public async Task<ImageGenerationJob> Handle(CreateImageGenerationJobCommand command, CancellationToken cancellationToken = default)
        {
            var imageGenerationJob = new ImageGenerationJob(command.Id, command.InputText);
         
            await _imageGenerationRepository.SaveAsync(imageGenerationJob, cancellationToken);
          
            _cache.Set(imageGenerationJob.Id, imageGenerationJob, new MemoryCacheEntryOptions
            {
                SlidingExpiration = _cacheSlidingExpiration,
                Priority = CacheItemPriority.High
            });

            await _commandQueue.EnqueueAsync(command, cancellationToken);

            return imageGenerationJob;
        }
    }
}
