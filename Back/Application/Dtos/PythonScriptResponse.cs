namespace Application.Dtos
{
    public sealed class PythonScriptResponse
    {
        public PythonScriptResponse(int exitCode, string output, string? errorOutput = null)
        {
            ExitCode = exitCode;
            Output = output;
            ErrorOutput = errorOutput;
            HasErrors = !string.IsNullOrEmpty(ErrorOutput);
            IsSuccessful = exitCode != 0;
        }

        public int ExitCode { get; }
        public string Output { get; }
        public string? ErrorOutput { get; }
        public bool IsSuccessful { get; }
        public bool HasErrors { get; }
    }
}
