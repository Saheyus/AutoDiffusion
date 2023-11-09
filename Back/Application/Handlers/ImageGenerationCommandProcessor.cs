using Application.Commands;
using Application.Dtos;
using Application.Ports;
using Domain.Events;
using MediatR;

namespace Application.Handlers
{
    public sealed class ImageGenerationCommandProcessor : IImageGenerationCommandProcessor
    {
        private readonly IPythonScriptInvoker _pythonScriptInvoker;
        private readonly IPublisher _publisher;

        public ImageGenerationCommandProcessor(IPythonScriptInvoker pythonScriptInvoker, IPublisher publisher)
        {
            _pythonScriptInvoker = pythonScriptInvoker;
            _publisher = publisher;
        }

        public async Task ProcessAsync(CreateImageGenerationJobCommand createImageGenerationJobCommand, CancellationToken cancellationToken = default)
        {
            var startedEvent = new ImageGenerationJobStartedEvent(createImageGenerationJobCommand.Id);
            await _publisher.Publish(startedEvent, cancellationToken);

            PythonScriptResponse response;
            try
            {
                response = await _pythonScriptInvoker.InvokeAsync(new[] { createImageGenerationJobCommand.InputText }, cancellationToken);
            }
            catch (Exception ex)
            {
                var failedEvent = new ImageGenerationJobFailedEvent(createImageGenerationJobCommand.Id, ex);
                await _publisher.Publish(failedEvent, cancellationToken);
                return;
            }
            var finishedEvent = new ImageGenerationJobFinishedEvent(createImageGenerationJobCommand.Id, response.ExitCode, response.IsSuccessful, response.HasErrors, response.Output, response.ErrorOutput);
            await _publisher.Publish(finishedEvent, cancellationToken);
        }
    }
}
