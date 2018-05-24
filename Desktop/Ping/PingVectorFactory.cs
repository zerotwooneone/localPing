using System;
using Desktop.Vector;
using zh.LocalPingLib.Ping;

namespace Desktop.Ping
{
    public class PingVectorFactory: IPingVectorFactory
    {
        public IVector GetVector(IPingResponse pingResponse)
        {
            return  new Vector.Vector(new []{new DimensionValue(new DimensionKey("status"), pingResponse.Status.GetHashCode() ), });
        }
    }
}
