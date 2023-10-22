using System.Threading.Channels;
using Application.Commands;
using Application.Events;
using Application.Ports;
using Domain.Entities;
using Endpoints.Ports;
using MediatR;
using Microsoft.Extensions.Hosting;

namespace Application.Services
{
    //TODO
    //make loop handle 'complete' signal
    //make loop handle channel exceptions
    //handle dispose 
    //handle EnqueueAsync exceptions

    public sealed class ImageGenerationBackGroundService : IHostedService , IImageGenerationQueue, IDisposable
    {
        private readonly Channel<CreateImageGenerationCommand> _imageGenerationChannel;
        private readonly IPythonScriptInvoker _pythonScriptInvoker;
        private readonly IPublisher _publisher;
        
        public ImageGenerationBackGroundService(IPythonScriptInvoker pythonScriptInvoker, IPublisher publisher)
        {
            _pythonScriptInvoker = pythonScriptInvoker;
            _publisher = publisher;
            _imageGenerationChannel = Channel.CreateUnbounded<CreateImageGenerationCommand> (new UnboundedChannelOptions
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
                while (!cancellationToken.IsCancellationRequested)
                {
                    if (await _imageGenerationChannel.Reader.WaitToReadAsync(cancellationToken))
                    {
                        if (!_imageGenerationChannel.Reader.TryRead(out var imageGeneration))
                        {
                            //handle unread task
                            //break loop or retry?...
                            continue;
                        }

                        ApplicationEvent @event;
                        try
                        {
                            var response = await _pythonScriptInvoker.InvokeAsync(imageGeneration.ScriptName, new []{imageGeneration.Prompt}, cancellationToken);
                            @event = new PythonScriptFinishedEvent(imageGeneration.Id, response.ExitCode, response.IsSuccessful, response.HasErrors, response.Output, response.ErrorOutput);
                        }
                        catch (Exception ex)
                        {
                            @event = new PythonScriptFailedEvent(imageGeneration.Id, ex);
                        }

                        await _publisher.Publish(@event, cancellationToken);
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
        public async Task EnqueueAsync(CreateImageGenerationCommand imageGeneration, CancellationToken cancellationToken = default)
        {
            await _imageGenerationChannel.Writer.WriteAsync(imageGeneration, cancellationToken);
        }

        public void Dispose()
        {
            // TODO release managed resources here
        }
    }
}
