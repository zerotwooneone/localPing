using System.Net;
using System.Threading.Tasks;

namespace zh.LocalPingLib.Ping
{
    public interface IPingTaskFactory
    {
        Task<IPingResponse> Ping(IPAddress ipadrress);
    }
}