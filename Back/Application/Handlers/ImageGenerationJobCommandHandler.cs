using Application.Commands;
using Application.Ports;
using Domain.Entities;
using Domain.Ports;
using MediatR;

namespace Application.Handlers
{
    public sealed class ImageGenerationJobCommandHandler : IRequestHandler<CreateImageGenerationJobCommand, ImageGenerationJob>
    {
        private readonly IImageGenerationCommandQueue _commandQueue;
        private readonly IImageGenerationJobRepository _imageGenerationRepository;

        public ImageGenerationJobCommandHandler(IImageGenerationCommandQueue commandQueue, IImageGenerationJobRepository imageGenerationRepository)
        {
            _commandQueue = commandQueue;
            _imageGenerationRepository = imageGenerationRepository;
        }

        public async Task<ImageGenerationJob> Handle(CreateImageGenerationJobCommand command, CancellationToken cancellationToken = default)
        {
            var imageGenerationJob = new ImageGenerationJob(command.Id, command.InputText);
         
            await _imageGenerationRepository.SaveAsync(imageGenerationJob, cancellationToken);
            
            await _commandQueue.EnqueueAsync(command, cancellationToken);

            return imageGenerationJob;
        }
    }
}
