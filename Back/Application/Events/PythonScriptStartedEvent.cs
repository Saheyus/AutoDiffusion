namespace Application.Events
{
    public sealed class PythonScriptStartedEvent : ApplicationEvent
    {
        public Guid ImageGenerationId { get; }

        public PythonScriptStartedEvent(Guid imageGenerationId)
        {
            ImageGenerationId = imageGenerationId;
        }
    }
}
