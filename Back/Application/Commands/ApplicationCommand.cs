using MediatR;

namespace Application.Commands
{
    public abstract class ApplicationCommand : IRequest
    {
        public Guid Id { get; } = Guid.NewGuid();
    }
}
