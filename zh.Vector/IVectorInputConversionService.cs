using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace zh.Vector
{
    public interface IVectorInputConversionService<T>
    {
        Task<IVector> GetVector(T input,
            IReadOnlyDictionary<IDimensionKey, Func<T, Task<double>>> conversionFuncs);
    }
}