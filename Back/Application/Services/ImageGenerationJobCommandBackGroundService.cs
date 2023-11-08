using System.Threading.Channels;
using Application.Commands;
using Application.Ports;
using Domain.Entities;
using Domain.Ports;
using Microsoft.Extensions.Hosting;

namespace Application.Services
{
    //TODO
    //make loop handle 'complete' signal
    //make loop handle channel exceptions
    //handle dispose 
    //handle EnqueueAsync exceptions

    public sealed class ImageGenerationJobCommandBackGroundService : IHostedService , IImageGenerationCommandQueue, IDisposable
    {
        private readonly Channel<CreateImageGenerationJobCommand> _imageGenerationCommandChannel;
        private readonly IImageGenerationCommandProcessor _generationCommandProcessor;
        private readonly IImageGenerationJobRepository _generationJobRepository;
        public ImageGenerationJobCommandBackGroundService(IImageGenerationCommandProcessor generationCommandProcessor, IImageGenerationJobRepository generationJobRepository)
        {
            _generationCommandProcessor = generationCommandProcessor;
            _generationJobRepository = generationJobRepository;
            _imageGenerationCommandChannel = Channel.CreateUnbounded<CreateImageGenerationJobCommand> (new UnboundedChannelOptions
            {
                SingleReader = true,
                SingleWriter = false,
                AllowSynchronousContinuations = false 
            });
        }

        private async Task LoadPendingCommandsAsync(CancellationToken cancellationToken = default)
        {
            var imageJobs = await _generationJobRepository.GetAllAsync(cancellationToken);
            var imageJobsPending = imageJobs.Where(x => x.State == ImageGenerationJobStates.Pending);

            foreach (var imageGenerationJob in imageJobsPending)
            {
                var cmd = new CreateImageGenerationJobCommand(imageGenerationJob.Id, imageGenerationJob.InputText);

                await _imageGenerationCommandChannel.Writer.WriteAsync(cmd, cancellationToken);
            }
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            Task.Run(async () =>
            {
                //Get back pending jobs from database, if any
                await LoadPendingCommandsAsync(cancellationToken);

                while (!cancellationToken.IsCancellationRequested)
                {
                    if (await _imageGenerationCommandChannel.Reader.WaitToReadAsync(cancellationToken))
                    {
                        if (!_imageGenerationCommandChannel.Reader.TryRead(out var imageGenerationJob))
                        {
                            //handle unread task
                            //break loop or retry?...
                            continue;
                        }

                        try
                        {
                            await _generationCommandProcessor.ProcessAsync(imageGenerationJob, cancellationToken);
                        }
                        catch (Exception e)
                        {
                            //Handle if throws
                            continue;
                        }
                    }
                    else
                    {
                        //todo 
                        //WaitToReadAsync() == false means complete() was called on channel
                        //handle completion signal
                    }

                }
            }, cancellationToken);

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            //handle stop service
            throw new NotImplementedException();
        }

        //use async void if you do not care whether your item has been written or not to the channel
        public async Task EnqueueAsync(CreateImageGenerationJobCommand createImageGenerationJobCommand, CancellationToken cancellationToken = default)
        {
            await _imageGenerationCommandChannel.Writer.WriteAsync(createImageGenerationJobCommand, cancellationToken);
        }

        public void Dispose()
        {
            // TODO release managed resources here
        }
    }
}
