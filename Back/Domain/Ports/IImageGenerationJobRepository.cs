using Domain.Entities;

namespace Domain.Ports
{
    public interface IImageGenerationJobRepository
    {
        public Task<ImageGenerationJob?> GetAsync(Guid id, CancellationToken cancellationToken = default);
        public Task<IEnumerable<ImageGenerationJob>> GetAllAsync(CancellationToken cancellationToken = default);
        public Task SaveAsync(ImageGenerationJob imageGeneration, CancellationToken cancellationToken = default);
    }
}
