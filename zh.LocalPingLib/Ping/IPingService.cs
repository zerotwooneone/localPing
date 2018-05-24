using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace zh.LocalPingLib.Ping
{
    public interface IPingService
    {
        IEnumerable<Task<IPingResponse>> Ping(IEnumerable<IPAddress> addresses);
    }
}