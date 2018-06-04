using Ninject.Modules;

namespace Desktop.Dispatcher
{
    public class DispatcherModule: NinjectModule
    {
        public override void Load()
        {
            Bind<IDispatcherAccessor>().To<WpfDispatcherAccessor>();
        }
    }
}
