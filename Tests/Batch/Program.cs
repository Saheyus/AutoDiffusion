// See https://aka.ms/new-console-template for more information

using Application.Commands;
using Application.Handlers;
using Application.Ports;
using Application.Services;
using Domain.EventHandlers;
using Domain.Ports;
using Endpoints.Services;
using Infrastructure.Repositories;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

Console.WriteLine("Hello, World!");
try
{
    //waiting for autodiffusion font app loading
    await Task.Delay(10000);

    const string autoDiffusion = "AutoDiffusion";

    var builder = Host.CreateApplicationBuilder(args);

    void MediatRConfiguration(MediatRServiceConfiguration cfg)
    {
        cfg.RegisterServicesFromAssemblies(
            typeof(ImageGenerationJobEventHandler).Assembly,
            typeof(ImageGenerationJobCommandHandler).Assembly,
            typeof(NotificationsHandler).Assembly
            ); 
        cfg.NotificationPublisher = new ParallelNoWaitPublisher();
    }

    PythonScriptInvoker PythonScriptInvokerConfiguration(IServiceProvider p)
    {
        var currentDir = Directory.GetCurrentDirectory();
        var idx = currentDir.IndexOf(autoDiffusion, StringComparison.OrdinalIgnoreCase);
        var solutionRoot = currentDir[..(idx + autoDiffusion.Length)];
        var scriptsDirectory = solutionRoot + @"\Tests\Batch\Scripts";
        const string scriptToInvoke = "python.py";

        return new PythonScriptInvoker("cmd.exe", scriptsDirectory, scriptToInvoke);
    }

    builder.Services.AddHttpClient(autoDiffusion, cl =>
    {
        cl.BaseAddress = new Uri("http://localhost:7071");
        cl.Timeout = TimeSpan.FromSeconds(120);
    }).SetHandlerLifetime(Timeout.InfiniteTimeSpan);

    builder.Services.AddMemoryCache(options =>
    {
        options.ExpirationScanFrequency = TimeSpan.FromMinutes(1);
    });

    builder.Services.AddMediatR(MediatRConfiguration);
    builder.Services.AddSingleton<IImageGenerationJobRepository, CachedImageGenerationJobRepository>();
    builder.Services.AddSingleton<IPythonScriptInvoker, PythonScriptInvoker>(PythonScriptInvokerConfiguration);
    builder.Services.AddSingleton<IImageGenerationCommandQueue, ImageGenerationJobCommandBackGroundService>();
    builder.Services.AddSingleton<IImageGenerationCommandProcessor, ImageGenerationCommandProcessor>();
    builder.Services.AddHostedService(p => (ImageGenerationJobCommandBackGroundService) p.GetRequiredService<IImageGenerationCommandQueue>());

    var app = builder.Build();
    app.StartAsync();

    var request = new CreateImageGenerationJobCommand("draw me something interesting");
    var mediatR = app.Services.GetRequiredService<IMediator>();
    var result = await mediatR.Send(request);

    Console.WriteLine(result.Id + " " + result.State);

    await app.WaitForShutdownAsync();
}
catch (Exception e)
{
    await Console.Error.WriteLineAsync(e.ToString());
}



Console.WriteLine("Press any to finish");
Console.ReadKey(true);