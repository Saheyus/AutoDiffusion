using Domain.Entities;

namespace Application.Ports
{
    public interface IImageGenerationQueue
    {
        Task EnqueueAsync(ImageGeneration imageGeneration, CancellationToken cancellationToken = default);
    }
}
