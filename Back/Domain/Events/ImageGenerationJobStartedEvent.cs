namespace Domain.Events
{
    public sealed class ImageGenerationJobStartedEvent : ApplicationEvent
    {
        public Guid ImageGenerationJobId { get; }

        public ImageGenerationJobStartedEvent(Guid imageGenerationJobId)
        {
            ImageGenerationJobId = imageGenerationJobId;
        }
    }
}
