using Domain.Entities;

namespace Application.Ports
{
    public interface IImageGenerationQueue
    {
        Task EnqueueAsync(ImageGenerationJob imageGeneration, CancellationToken cancellationToken = default);
    }
}
