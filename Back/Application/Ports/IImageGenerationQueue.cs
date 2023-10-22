using Application.Commands;

namespace Application.Ports
{
    public interface IImageGenerationQueue
    {
        Task EnqueueAsync(CreateImageGenerationCommand imageGeneration, CancellationToken cancellationToken = default);
    }
}
