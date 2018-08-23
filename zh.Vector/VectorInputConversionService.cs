using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace zh.Vector
{
    public class VectorInputConversionService<T> : IVectorInputConversionService<T>
    {
        public async Task<IVector> GetVector(T input, IReadOnlyDictionary<IDimensionKey, Func<T, Task<double>>> conversionFuncs)
        {
            var dimensionValueTasks = conversionFuncs.Select(async kvp =>
            {
                var value = await kvp.Value(input).ConfigureAwait(false);
                var dimensionValue = new DimensionValue(kvp.Key, value);
                return dimensionValue;
            });
            var dimensionValues = await Task.WhenAll(dimensionValueTasks).ConfigureAwait(false);
            var vector = new Vector(dimensionValues);
            return vector;
        }
    }
}