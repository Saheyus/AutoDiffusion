 using System.Diagnostics;
using System.Text;
using Application.Dtos;
using Application.Ports;


namespace Endpoints.Services
{
    public class PythonScriptInvoker : IPythonScriptInvoker 
    {
        private readonly string _pathToProcess;
        private readonly string _scriptToInvoke;
        private readonly string _workingDirectory;

        public PythonScriptInvoker(string pathToProcess, string workingDirectory, string scriptToInvoke)
        {
            if (string.IsNullOrWhiteSpace(pathToProcess))
                throw new ArgumentNullException(nameof(pathToProcess));
            
            if (!pathToProcess.Contains(".exe"))
                pathToProcess = string.Concat(pathToProcess, ".exe");

            if (!Directory.Exists(workingDirectory))
                throw new ArgumentException($"{workingDirectory} does not exist or is not a directory!");

            if (!scriptToInvoke.Contains(".py"))
                throw new ArgumentException("Script must be a python script of .py extension!", nameof(scriptToInvoke));

            if (!File.Exists(Path.Combine(workingDirectory, scriptToInvoke)))
                throw new DirectoryNotFoundException($"{scriptToInvoke} does not exists!");

            _pathToProcess = pathToProcess;
            _scriptToInvoke = scriptToInvoke;
            _workingDirectory = workingDirectory;
        }
        public Task<PythonScriptResponse> InvokeAsync(string[] arguments, CancellationToken cancellationToken = default)
        {
            return Task.Run(async () =>
            {
                var processInfo = new ProcessStartInfo(_pathToProcess, "/c " + _scriptToInvoke + " \"" + string.Join("\", \"", arguments) + "\"")
                {
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    StandardErrorEncoding = Encoding.UTF8,
                    StandardOutputEncoding = Encoding.UTF8,
                    WorkingDirectory = _workingDirectory
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
