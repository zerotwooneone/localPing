using Desktop.Vector;

namespace zh.LocalPingLib.Ping
{
    public interface IDimensionKeyFactory
    {
        IDimensionKey GetOrCreate(string name);
    }
}