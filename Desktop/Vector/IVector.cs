using System.Collections.Generic;

namespace Desktop.Vector
{
    public interface IVector
    {
        IEnumerable<IDimensionValue> DimensionValues { get; }
    }
}