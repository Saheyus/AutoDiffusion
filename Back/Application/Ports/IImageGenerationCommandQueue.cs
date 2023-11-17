using Application.Commands;

namespace Application.Ports
{
    public interface IImageGenerationCommandQueue
    {
        Task EnqueueAsync(CreateImageGenerationJobCommand createImageGenerationJobCommand, CancellationToken cancellationToken = default);
    }
}
