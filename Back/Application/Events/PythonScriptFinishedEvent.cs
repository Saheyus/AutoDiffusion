namespace Application.Events
{
    public sealed class PythonScriptFinishedEvent : ApplicationEvent
    {
        public int ExitCode { get; }
        public string Output { get; }
        public string? ErrorOutput { get; }
        public bool IsSuccessful { get; }
        public bool HasErrors { get; }
        public Guid ImageGenerationId { get; }

        public PythonScriptFinishedEvent(Guid imageGenerationId, int exitCode, bool isSuccessful, bool hasErrors, string output, string? errorOutput = null)
        {
            ExitCode = exitCode;
            Output = output;
            ImageGenerationId = imageGenerationId;
            ErrorOutput = errorOutput;
            IsSuccessful = isSuccessful;
            HasErrors = hasErrors;
        }
    }
}
