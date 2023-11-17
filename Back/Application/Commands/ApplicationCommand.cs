using MediatR;

namespace Application.Commands
{
    public abstract class ApplicationCommand<T> : IRequest<T>
    {
        public Guid Id { protected init; get; } = Guid.NewGuid();
    }
}
