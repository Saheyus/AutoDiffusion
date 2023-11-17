using System.Collections.Concurrent;
using Domain.Entities;
using Domain.Exceptions;
using Domain.Ports;

namespace Infrastructure.Repositories
{
    internal sealed class ImageGenerationJobRepository : IImageGenerationJobRepository
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

        public Task SaveAsync(ImageGenerationJob imageGenerationJob, CancellationToken cancellationToken = default)
        {
            Generations[imageGenerationJob.Id] = imageGenerationJob;
            return Task.CompletedTask;
        }
    }
}
