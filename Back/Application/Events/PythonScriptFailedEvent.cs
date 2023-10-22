namespace Application.Events
{
    public sealed class PythonScriptFailedEvent : ApplicationEvent<Exception>
    {
        public Guid ImageGenerationId { get; }

        public PythonScriptFailedEvent(Guid imageGenerationId, Exception exception) : base (exception)
        {
            ImageGenerationId = imageGenerationId;
        }
    }
}
