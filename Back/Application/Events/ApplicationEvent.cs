using MediatR;

namespace Application.Events
{
    public abstract class ApplicationEvent<T> : ApplicationEvent
    {
        public T Data { get; }

        protected ApplicationEvent(T data)
        {
            Data = data;
        }
    }

    public abstract class ApplicationEvent : INotification
    {
        public DateTime EventTime { get; } = DateTime.UtcNow;
    }
}
