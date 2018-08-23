using zh.Vector;

namespace zh.LocalPingLib.Ping
{
    public interface IPingVectorFactory
    {
        IVector GetVector(IPingResponse pingResponse, IPingStats stats);
    }
}