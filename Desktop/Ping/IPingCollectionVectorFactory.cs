using System.Collections.Generic;
using System.Threading.Tasks;
using Desktop.Vector;
using zh.LocalPingLib.Ping;

namespace Desktop.Ping
{
    public interface IPingCollectionVectorFactory
    {
        Task<IVector> GeVector(IEnumerable<IPingResponse> pingResponses);
    }
}