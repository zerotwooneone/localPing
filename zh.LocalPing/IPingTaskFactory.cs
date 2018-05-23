using System.Net;
using System.Threading.Tasks;

namespace zh.LocalPing
{
    public interface IPingTaskFactory
    {
        Task<IPingResponse> Ping(IPAddress ipadrress);
    }
}