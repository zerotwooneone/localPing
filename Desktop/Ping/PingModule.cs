using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ninject.Modules;

namespace Desktop.Ping
{
    public class PingModule: NinjectModule
    {
        public override void Load()
        {
            Bind<IPingTimer>().To<PingTimer>();
            Bind<IPingTimerConfig>().To<Config>();
        }
    }
}
