using Desktop.Vector;
using zh.LocalPingLib.Ping;

namespace Desktop.Ping
{
    public interface IPingVectorFactory
    {
        IVector GetVector(IPingResponse pingResponse);
    }
}