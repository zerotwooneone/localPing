using System.Collections.Generic;
using zh.Vector;

namespace zh.LocalPingLib.Ping
{
    public interface IPingCollectionVectorFactory
    {
        IVector GetVector(IEnumerable<IVector> pingVectors);
    }
}