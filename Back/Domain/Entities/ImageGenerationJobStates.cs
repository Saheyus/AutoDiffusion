namespace Domain.Entities
{
    public enum ImageGenerationJobStates
    {
        Pending = 0,
        Running = 1,
        Finished = 2,
        Failed = 3,
        Cancelled = 4
    }
}
