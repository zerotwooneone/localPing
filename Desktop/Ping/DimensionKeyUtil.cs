using System.Net;

namespace Desktop.Ping
{
    public class DimensionKeyUtil : IDimensionKeyUtil
    {
        public string GetStatusFlag(IPAddress ipAddress)
        {
            return $"{ipAddress} status flag";
        }
    }
}