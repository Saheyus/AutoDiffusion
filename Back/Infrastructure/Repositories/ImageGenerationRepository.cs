using System.Collections.Concurrent;
using Domain.Entities;
using Infrastructure.Ports;

namespace Infrastructure.Repositories
{
    public class ImageGenerationRepository : IImageGenerationRepository
    {
        private static readonly ConcurrentDictionary<Guid, ImageGeneration> Generations = new();

        public Task<ImageGeneration?> GetAsync(Guid id, CancellationToken cancellationToken = default)
        {
            if (Generations.TryGetValue(id, out var generation))
                return Task.FromResult(generation)!;

            return Task.FromResult<ImageGeneration?>(null);
        }

        public Task<IEnumerable<ImageGeneration>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IEnumerable<ImageGeneration>>(Generations.Values);
        }

        public Task SaveAsync(ImageGeneration imageGeneration, CancellationToken cancellationToken = default)
        {
            Generations[imageGeneration.Id] = imageGeneration;
            return Task.CompletedTask;
        }
    }
}
