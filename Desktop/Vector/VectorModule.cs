using Ninject.Modules;
using zh.Vector;

namespace Desktop.Ping
{
    public class VectorModule: NinjectModule
    {
        public override void Load()
        {
            Bind<IVectorComparer>().To<VectorComparer>();
        }
    }
}
