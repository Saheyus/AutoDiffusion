namespace Domain.Ports
{
    public interface IAggregate<out T> 
    {
        public T Root { get; }
    }
}
