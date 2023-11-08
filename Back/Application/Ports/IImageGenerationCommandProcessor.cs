using Application.Commands;

namespace Application.Ports
{
    public interface IImageGenerationCommandProcessor
    {
        Task ProcessAsync(CreateImageGenerationJobCommand createImageGenerationJobCommand, CancellationToken cancellationToken = default);
    }
}
