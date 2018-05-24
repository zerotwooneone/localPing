using System.Collections.Generic;
using Desktop.Vector;
using zh.LocalPingLib.Ping;

namespace Desktop.Ping
{
    public interface IPingCollectionVectorFactory
    {
        IVector GeVector(IEnumerable<IPingResponse> pingResponses);
    }
}