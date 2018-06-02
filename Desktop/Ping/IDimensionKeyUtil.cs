using System.Net;

namespace Desktop.Ping
{
    public interface IDimensionKeyUtil
    {
        string GetStatusFlag(IPAddress ipAddress);
    }
}