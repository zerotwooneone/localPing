using Ninject.Modules;
using zh.LocalPingLib.Ping;

namespace Desktop.Ping
{
    public class PingLibModule : NinjectModule
    {
        public override void Load()
        {
            Bind<IPingService>().To<PingService>();
            Bind<IPingTaskFactory>().To<PingTaskFactory>();
            Bind<IPingResponseUtil>().To<PingResponseUtil>();
        }
    }
}