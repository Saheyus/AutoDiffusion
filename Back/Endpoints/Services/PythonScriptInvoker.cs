﻿using System.Diagnostics;
using System.Text;
using Endpoints.Dtos;
using Endpoints.Ports;

namespace Endpoints.Services
{
    public class PythonScriptInvoker : IPythonScriptInvoker 
    {
        private readonly string _pathToProcess;
        private readonly string _scriptsDirectory;

        public PythonScriptInvoker(string pathToProcess, string scriptsDirectory)
        {
            if (string.IsNullOrWhiteSpace(pathToProcess))
                throw new ArgumentNullException(nameof(pathToProcess));
            
            if (!pathToProcess.Contains(".exe"))
                pathToProcess = string.Concat(pathToProcess, ".exe");

            if (!Directory.Exists(scriptsDirectory))
                throw new DirectoryNotFoundException($"{scriptsDirectory} Does not exists or is not a directory!");
        
            _pathToProcess = pathToProcess;
            _scriptsDirectory = scriptsDirectory;
        }
        public Task<PythonScriptResponse> InvokeAsync(string script, string[] arguments, CancellationToken cancellationToken = default)
        {
            return Task.Run(async () =>
            {
                var processInfo = new ProcessStartInfo(_pathToProcess, "/c " + script + " \"" + string.Join("\", \"", arguments) + "\"")
                {
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    StandardErrorEncoding = Encoding.UTF8,
                    StandardOutputEncoding = Encoding.UTF8,
                    WorkingDirectory = _scriptsDirectory
                };

                using var process = Process.Start(processInfo) ??
                              throw new Exception($"Could not start process at {_pathToProcess} with arguments {arguments}");

                var outputB = new StringBuilder();
                var errorOutputB = new StringBuilder();

                //Async reading in order to avoid deadlocks
                //https://stackoverflow.com/questions/5519328/executing-batch-file-in-c-sharp
                //https://learn.microsoft.com/en-us/dotnet/api/system.diagnostics.processstartinfo.redirectstandardoutput?view=net-7.0&redirectedfrom=MSDN#System_Diagnostics_ProcessStartInfo_RedirectStandardOutput

                process.OutputDataReceived += WriteOutput;
                process.BeginOutputReadLine();

                process.ErrorDataReceived += WriteErrorOutput;
                process.BeginErrorReadLine();

                await process.WaitForExitAsync(cancellationToken);

                //unassign handlers to prevent memory leaks 
                process.OutputDataReceived -= WriteOutput;
                process.ErrorDataReceived -= WriteErrorOutput;

                return new PythonScriptResponse(process.ExitCode, outputB.ToString(), errorOutputB.ToString());

                //local output handlers funcs
                void WriteErrorOutput(object sender, DataReceivedEventArgs e)
                {
                    errorOutputB.AppendLine(e.Data);
                }

                void WriteOutput(object sender,DataReceivedEventArgs e)
                {
                    outputB.AppendLine(e.Data);
                }

            }, cancellationToken);
        }
    }
}