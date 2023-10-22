using Domain.Entities;

namespace Infrastructure.Ports
{
    public interface IImageGenerationRepository
    {
        public Task<ImageGeneration?> GetAsync(Guid id, CancellationToken cancellationToken = default);
        public Task<IEnumerable<ImageGeneration>> GetAllAsync(CancellationToken cancellationToken = default);
        public Task SaveAsync(ImageGeneration imageGeneration, CancellationToken cancellationToken = default);
    }
}
