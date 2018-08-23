using System.Collections.Generic;
using Ninject.Modules;
using zh.LocalPingLib.Ping;
using zh.Vector;

namespace Desktop.Ping
{
    public class PingModule: NinjectModule
    {
        public override void Load()
        {
            Bind<IPingTimer>().To<PingTimer>();
            Bind<IPingTimerConfig>().To<Config>();
            Bind<IPingCollectionVectorFactory>().To<PingCollectionVectorFactory>();
            Bind<IDimensionKeyFactory>().To<DimensionKeyFactory>().InSingletonScope();
            Bind<IVectorInputConversionService<IPingResponse>>().To<VectorInputConversionService<IPingResponse>>();
            Bind<IVectorInputConversionService<IEnumerable<IPingResponse>>>()
                .To<VectorInputConversionService<IEnumerable<IPingResponse>>>();
            Bind<IIpAddressService>().To<IpAddressService>();
            Bind<IPingVectorFactory>().To<PingVectorFactory>();
        }
    }
}
