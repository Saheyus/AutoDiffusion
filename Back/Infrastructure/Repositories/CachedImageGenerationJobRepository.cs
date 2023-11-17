using Domain.Ports;
using Domain.Entities;
using Microsoft.Extensions.Caching.Memory;

namespace Infrastructure.Repositories
{
    //Proxy for our ImageGenerationJobRepository
    public sealed class CachedImageGenerationJobRepository : IImageGenerationJobRepository
    {
        private readonly IMemoryCache _memoryCache;
        private readonly IImageGenerationJobRepository _imageGenerationJobRepository;
        private readonly TimeSpan _cacheSlidingExpiration = TimeSpan.FromMinutes(10);

        public CachedImageGenerationJobRepository(IMemoryCache memoryCache )
        {
            _memoryCache = memoryCache;
            _imageGenerationJobRepository = new ImageGenerationJobRepository();
        }


        public async Task<ImageGenerationJob> GetAsync(Guid id, CancellationToken cancellationToken = default)
        {
            if (!_memoryCache.TryGetValue(id, out ImageGenerationJob? imageGenerationJob) || imageGenerationJob == null)
            {
                imageGenerationJob = await _imageGenerationJobRepository.GetAsync(id, cancellationToken);

                //save into cache if found inside repository
                _memoryCache.Set(imageGenerationJob.Id, imageGenerationJob, new MemoryCacheEntryOptions
                {
                    SlidingExpiration = _cacheSlidingExpiration,
                    Priority = CacheItemPriority.High
                });
            }

            if (imageGenerationJob == null)
                throw new NullReferenceException($"Cannot find generation {id} from repository! Inconsistent state between repository and application!");

            return imageGenerationJob;
        }

        public Task<IEnumerable<ImageGenerationJob>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return _imageGenerationJobRepository.GetAllAsync(cancellationToken);
        }

        public Task SaveAsync(ImageGenerationJob imageGenerationJob, CancellationToken cancellationToken = default)
        {
            _memoryCache.Set(imageGenerationJob.Id, imageGenerationJob, new MemoryCacheEntryOptions
            {
                SlidingExpiration = _cacheSlidingExpiration,
                Priority = CacheItemPriority.High
            });

            return _imageGenerationJobRepository.SaveAsync(imageGenerationJob, cancellationToken);
        }
    }
}
