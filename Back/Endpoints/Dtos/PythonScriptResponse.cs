namespace Endpoints.Dtos
{
    public sealed class PythonScriptResponse
    {
        public PythonScriptResponse(int exitCode, string output, string? errorOutput = null)
        {
            ExitCode = exitCode;
            Output = output;
            ErrorOutput = errorOutput;
            HasErrors = !string.IsNullOrEmpty(ErrorOutput);
        }

        public int ExitCode { get; }
        public string Output { get; }
        public string? ErrorOutput { get; }
        public bool IsSuccessful => ExitCode != 0;
        public bool HasErrors { get; }
    }
}
