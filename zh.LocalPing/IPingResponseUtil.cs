using System.Net.NetworkInformation;

namespace zh.LocalPing
{
    public interface IPingResponseUtil
    {
        IPingResponse Convert(PingCompletedEventArgs pingCompletedEventArgs);
    }
}