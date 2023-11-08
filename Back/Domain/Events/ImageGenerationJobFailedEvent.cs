namespace Domain.Events
{
    public sealed class ImageGenerationJobFailedEvent : DomainEvent<Exception>
    {
        public Guid ImageGenerationJobId { get; }

        public ImageGenerationJobFailedEvent(Guid imageGenerationJobId, Exception exception) : base (exception)
        {
            ImageGenerationJobId = imageGenerationJobId;
        }
    }
}
