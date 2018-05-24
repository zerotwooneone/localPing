using System.Collections.Generic;

namespace Desktop.Vector
{
    public class Vector : IVector
    {
        public Vector(IEnumerable<IDimensionValue> dimensionValues)
        {
            DimensionValues = dimensionValues;
        }

        public IEnumerable<IDimensionValue> DimensionValues { get; }
    }
}