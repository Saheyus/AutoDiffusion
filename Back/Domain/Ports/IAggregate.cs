using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Ports
{
    public interface IAggregate<T> 
    {
        public T Root { get; }
    }
}
