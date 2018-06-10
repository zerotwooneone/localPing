using System.Net.NetworkInformation;

namespace zh.LocalPingLib.Ping
{
    public interface IPingResponseUtil
    {
        IPingResponse Convert(PingCompletedEventArgs pingCompletedEventArgs);
        bool IsSuccess(IPStatus ipStatus);
    }
}