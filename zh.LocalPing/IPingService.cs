using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace zh.LocalPing
{
    public interface IPingService
    {
        IEnumerable<Task<IPingResponse>> Ping(IEnumerable<IPAddress> addresses);
    }
}