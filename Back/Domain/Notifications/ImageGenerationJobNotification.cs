using Domain.Entities;
using MediatR;

namespace Domain.Notifications
{
    public sealed class ImageGenerationJobNotification : INotification
    {
        public Guid ImageJobGenerationId { get; }
        public ImageGenerationJobStates ImageGenerationJobState { get; }
        public IEnumerable<Uri> ImageGenerationImageUris { get; }

        public ImageGenerationJobNotification(ImageGenerationJob imageGenerationJob)
        {
            ImageJobGenerationId = imageGenerationJob.Id;
            ImageGenerationJobState = imageGenerationJob.State;
            ImageGenerationImageUris = imageGenerationJob.ImageUris;
        }
    }
}
