using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace zh.LocalPing
{
    public class PingService : IPingService
    {
        private readonly IPingTaskFactory _pingTaskFactory;

        public PingService(IPingTaskFactory pingTaskFactory)
        {
            _pingTaskFactory = pingTaskFactory;
        }

        public IEnumerable<Task<IPingResponse>> Ping(IEnumerable<IPAddress> addresses)
        {
            var result = addresses.Select(_pingTaskFactory.Ping);
            return result;
        }
    }
}
