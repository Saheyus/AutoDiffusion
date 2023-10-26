using Domain.Ports;

namespace Domain.Entities
{
    //Aggregate with single entity
    public sealed class ImageGenerationJob : IAggregate<ImageGenerationJob>
    {
        public ImageGenerationJob(Guid id, string inputText)
        {
            Id = id;
            InputText = !string.IsNullOrWhiteSpace(inputText) ? inputText : throw new ArgumentException("Input text cannot be null or empty!", nameof(inputText));
            State = ImageGenerationJobStates.Pending;
            LastModified = DateTime.UtcNow;
            Created = DateTime.UtcNow;
        }

        public string InputText { get; }
        public Guid Id { get; }

        public DateTime LastModified { get; private set; }
        public DateTime Created { get; private set; }

        public ImageGenerationJob Root => this;

        public ImageGenerationJobStates State { get; private set; }

        public void ChangeState(ImageGenerationJobStates state)
        {
            State = state;
            LastModified = DateTime.UtcNow;
        }

    }
}
