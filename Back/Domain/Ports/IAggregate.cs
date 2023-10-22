namespace Domain.Ports
{
    public interface IAggregate<T> 
    {
        public T Root { get; }
    }
}
