namespace Domain.Exceptions
{
    public class ImageGenerationJobNotFoundException : Exception
    {
        public ImageGenerationJobNotFoundException(Guid id) : base ($"ImageGenerationJob {id} not found!")
        {
            ImageGenerationJobId = id;
        }

        public Guid ImageGenerationJobId { get; }
    }
}
