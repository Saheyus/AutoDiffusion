using System.Threading.Channels;
using Application.Commands;
using Application.Ports;
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
        public ImageGenerationJobCommandBackGroundService(IImageGenerationCommandProcessor generationCommandProcessor)
        {
            _generationCommandProcessor = generationCommandProcessor;
            _imageGenerationCommandChannel = Channel.CreateUnbounded<CreateImageGenerationJobCommand> (new UnboundedChannelOptions
            {
                SingleReader = true,
                SingleWriter = false,
                AllowSynchronousContinuations = false 
            });
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            Task.Run(async () =>
            {
                //TO DO Get back pending jobs from database, if any
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
