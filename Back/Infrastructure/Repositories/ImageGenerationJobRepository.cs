using System.Collections.Concurrent;
using Domain.Entities;
using Domain.Exceptions;
using Domain.Ports;

namespace Infrastructure.Repositories
{
    public sealed class ImageGenerationJobRepository : IImageGenerationJobRepository
    {
        private static readonly ConcurrentDictionary<Guid, ImageGenerationJob> Generations = new();

        public Task<ImageGenerationJob> GetAsync(Guid id, CancellationToken cancellationToken = default)
        {
            if (Generations.TryGetValue(id, out var generation))
                return Task.FromResult(generation);

            throw new ImageGenerationJobNotFoundException(id);
        }

        public Task<IEnumerable<ImageGenerationJob>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IEnumerable<ImageGenerationJob>>(Generations.Values);
        }

        public Task SaveAsync(ImageGenerationJob imageGeneration, CancellationToken cancellationToken = default)
        {
            Generations[imageGeneration.Id] = imageGeneration;
            return Task.CompletedTask;
        }
    }
}
