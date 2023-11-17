using MediatR;

namespace Domain.Events
{
    public abstract class ImageGenerationJobEvent<T> : ImageGenerationJobEvent
    {
        public T Data { get; }

        protected ImageGenerationJobEvent(T data)
        {
            Data = data;
        }
    }

    public abstract class ImageGenerationJobEvent : INotification
    {
        public string Name => GetType().Name;
        public DateTime EventTime { get; } = DateTime.UtcNow;
    }
}
