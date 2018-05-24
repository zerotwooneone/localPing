using Desktop.Vector;
using Ninject.Modules;

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
