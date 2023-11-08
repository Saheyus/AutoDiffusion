using MediatR;

namespace Domain.Events
{
    public abstract class DomainEvent<T> : ApplicationEvent
    {
        public T Data { get; }

        protected DomainEvent(T data)
        {
            Data = data;
        }
    }

    public abstract class ApplicationEvent : INotification
    {
        public DateTime EventTime { get; } = DateTime.UtcNow;
    }
}
