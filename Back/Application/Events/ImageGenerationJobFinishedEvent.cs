namespace Application.Events
{
    public sealed class ImageGenerationJobFinishedEvent : ApplicationEvent
    {
        public int ExitCode { get; }
        public string Output { get; }
        public string? ErrorOutput { get; }
        public bool IsSuccessful { get; }
        public bool HasErrors { get; }
        public Guid ImageGenerationJobId { get; }

        public ImageGenerationJobFinishedEvent(Guid imageGenerationJobId, int exitCode, bool isSuccessful, bool hasErrors, string output, string? errorOutput = null)
        {
            ExitCode = exitCode;
            Output = output;
            ImageGenerationJobId = imageGenerationJobId;
            ErrorOutput = errorOutput;
            IsSuccessful = isSuccessful;
            HasErrors = hasErrors;
        }
    }
}
