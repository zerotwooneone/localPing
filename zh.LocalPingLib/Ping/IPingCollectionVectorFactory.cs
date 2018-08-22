using System.Collections.Generic;
using Desktop.Vector;

namespace zh.LocalPingLib.Ping
{
    public interface IPingCollectionVectorFactory
    {
        IVector GetVector(IEnumerable<IVector> pingVectors);
    }
}