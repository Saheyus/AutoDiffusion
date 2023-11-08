namespace Application.Events
{
    public sealed class ImageGenerationJobFailedEvent : ApplicationEvent<Exception>
    {
        public Guid ImageGenerationJobId { get; }

        public ImageGenerationJobFailedEvent(Guid imageGenerationJobId, Exception exception) : base (exception)
        {
            ImageGenerationJobId = imageGenerationJobId;
        }
    }
}
