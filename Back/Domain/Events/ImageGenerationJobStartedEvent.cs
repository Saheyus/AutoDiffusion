namespace Domain.Events
{
    public sealed class ImageGenerationJobStartedEvent : ImageGenerationJobEvent
    {
        public Guid ImageGenerationJobId { get; }

        public ImageGenerationJobStartedEvent(Guid imageGenerationJobId)
        {
            ImageGenerationJobId = imageGenerationJobId;
        }
    }
}
