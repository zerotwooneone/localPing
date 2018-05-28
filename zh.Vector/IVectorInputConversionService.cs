using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Desktop.Vector
{
    public interface IVectorInputConversionService<T>
    {
        Task<IVector> GetVector(T input,
            IReadOnlyDictionary<IDimensionKey, Func<T, Task<double>>> conversionFuncs);
    }
}