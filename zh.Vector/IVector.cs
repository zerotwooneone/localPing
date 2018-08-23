using System.Collections.Generic;

namespace zh.Vector
{
    public interface IVector
    {
        IEnumerable<IDimensionValue> DimensionValues { get; }
    }
}