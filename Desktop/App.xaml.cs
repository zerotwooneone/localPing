using System.Windows;
using Desktop.Dispatcher;
using Desktop.Ping;
using Ninject;

namespace Desktop
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private IKernel _iocKernel;
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            _iocKernel = new StandardKernel();
            _iocKernel.Load(new PingModule());
            _iocKernel.Load(new PingLibModule());
            _iocKernel.Load(new VectorModule());
            _iocKernel.Load(new DispatcherModule());

            Current.MainWindow = _iocKernel.Get<MainWindow>();
            Current.MainWindow.Show();
        }
    }
}
