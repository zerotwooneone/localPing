using Ninject.Modules;

namespace Desktop.Ping
{
    public class PingModule: NinjectModule
    {
        public override void Load()
        {
            Bind<IPingTimer>().To<PingTimer>();
            Bind<IPingTimerConfig>().To<Config>();
            Bind<IPingCollectionVectorFactory>().To<PingCollectionVectorFactory>();
            Bind<IDimensionKeyFactory>().To<DimensionKeyFactory>();
        }
    }
}
