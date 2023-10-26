using System.Threading.Channels;
using Application.Events;
using Application.Ports;
using Domain.Entities;
using Endpoints.Dtos;
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

    public sealed class ImageGenerationJobBackGroundService : IHostedService , IImageGenerationQueue, IDisposable
    {
        private readonly Channel<ImageGenerationJob> _imageGenerationJobChannel;
        private readonly IPythonScriptInvoker _pythonScriptInvoker;
        private readonly IPublisher _publisher;
        
        public ImageGenerationJobBackGroundService(IPythonScriptInvoker pythonScriptInvoker, IPublisher publisher)
        {
            _pythonScriptInvoker = pythonScriptInvoker;
            _publisher = publisher;
            _imageGenerationJobChannel = Channel.CreateUnbounded<ImageGenerationJob> (new UnboundedChannelOptions
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
                    if (await _imageGenerationJobChannel.Reader.WaitToReadAsync(cancellationToken))
                    {
                        if (!_imageGenerationJobChannel.Reader.TryRead(out var imageGenerationJob))
                        {
                            //handle unread task
                            //break loop or retry?...
                            continue;
                        }
                      
                        var startedEvent = new PythonScriptStartedEvent(imageGenerationJob.Id);
                        await _publisher.Publish(startedEvent, cancellationToken);

                        PythonScriptResponse response;
                        try
                        {
                            response = await _pythonScriptInvoker.InvokeAsync(new[] { imageGenerationJob.InputText }, cancellationToken);
                        }
                        catch (Exception ex)
                        {
                            var failedEvent = new PythonScriptFailedEvent(imageGenerationJob.Id, ex);
                            await _publisher.Publish(failedEvent, cancellationToken);
                            continue;
                        }
                        var finishedEvent = new PythonScriptFinishedEvent(imageGenerationJob.Id, response.ExitCode, response.IsSuccessful, response.HasErrors, response.Output, response.ErrorOutput);
                        await _publisher.Publish(finishedEvent, cancellationToken);
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
        public async Task EnqueueAsync(ImageGenerationJob imageGenerationJob, CancellationToken cancellationToken = default)
        {
            await _imageGenerationJobChannel.Writer.WriteAsync(imageGenerationJob, cancellationToken);
        }

        public void Dispose()
        {
            // TODO release managed resources here
        }
    }
}
