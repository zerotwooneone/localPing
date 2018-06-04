namespace Desktop.Dispatcher
{
    public class WpfDispatcherAccessor : IDispatcherAccessor
    {
        public System.Windows.Threading.Dispatcher GetDispatcher()
        {
            return System.Windows.Threading.Dispatcher.CurrentDispatcher;
        }
    }
}