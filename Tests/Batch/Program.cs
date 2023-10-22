// See https://aka.ms/new-console-template for more information

using Application.Commands;
using Application.Handlers;
using Application.Ports;
using Application.Services;
using Endpoints.Ports;
using Endpoints.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Linq;
using System.Reflection;

Console.WriteLine("Hello, World!");
try
{
    /*
    var processInfo = new ProcessStartInfo("cmd.exe", "/c " + "startpython.bat")
    {
        CreateNoWindow = false,
        UseShellExecute = false,
        RedirectStandardError = true,
        RedirectStandardOutput = true
    };

    using var process = Process.Start(processInfo) ??
                        throw new Exception($"Could not start process");

    var outputB = new StringBuilder();
    var errorOutputB = new StringBuilder();

    //Async reading in order to avoid deadlocks
    //https://stackoverflow.com/questions/5519328/executing-batch-file-in-c-sharp
    //https://learn.microsoft.com/en-us/dotnet/api/system.diagnostics.processstartinfo.redirectstandardoutput?view=net-7.0&redirectedfrom=MSDN#System_Diagnostics_ProcessStartInfo_RedirectStandardOutput

    process.OutputDataReceived += WriteOutput;
    process.BeginOutputReadLine();

    process.ErrorDataReceived += WriteErrorOutput;
    process.BeginErrorReadLine();

    await process.WaitForExitAsync();

    //local output handlers funcs
    void WriteErrorOutput(object sender, DataReceivedEventArgs e)
    {
        errorOutputB.Append(e.Data);
    }

    void WriteOutput(object sender, DataReceivedEventArgs e)
    {
        outputB.Append(e.Data);
    }

    Console.WriteLine(outputB.ToString());
    Console.WriteLine(errorOutputB.ToString());
        */

    var builder = Host.CreateApplicationBuilder(args);

    void MediatRConfiguration(MediatRServiceConfiguration cfg)
    {
        cfg.RegisterServicesFromAssemblies(typeof(ImageGenerationEventHandler).Assembly);
        cfg.NotificationPublisher = new ParallelNoWaitPublisher();
    }

    PythonScriptInvoker PythonScriptInvokerConfiguration(IServiceProvider p)
    {
        const string solutionName = "AutoDiffusion";
        var currentDir = Directory.GetCurrentDirectory();
        var idx = currentDir.IndexOf(solutionName, StringComparison.OrdinalIgnoreCase);
        var solutionRoot = currentDir.Substring(0, idx + solutionName.Length);
        var scriptsDirectory = solutionRoot + "\\Tests\\Batch\\Scripts";

        return new PythonScriptInvoker("cmd.exe", scriptsDirectory);
    }

    builder.Services.AddMediatR(MediatRConfiguration);
    builder.Services.AddSingleton<IPythonScriptInvoker, PythonScriptInvoker>(PythonScriptInvokerConfiguration);
    builder.Services.AddSingleton<IImageGenerationQueue, ImageGenerationBackGroundService>();
    builder.Services.AddHostedService(p => (ImageGenerationBackGroundService) p.GetRequiredService<IImageGenerationQueue>());

    var app = builder.Build();
    app.StartAsync();
 ;
    


    var imageGeneration = new CreateImageGenerationCommand("My, . test - \" prompt", "python.py");
    var imageGeneration2 = new CreateImageGenerationCommand("my other prompt", "python.py");

    var queue = app.Services.GetRequiredService<IImageGenerationQueue>();
    await queue.EnqueueAsync(imageGeneration);
    await queue.EnqueueAsync(imageGeneration2);

    await app.WaitForShutdownAsync();
}
catch (Exception e)
{
    await Console.Error.WriteLineAsync(e.ToString());
}



Console.WriteLine("Press any to finish");
Console.ReadKey(true);