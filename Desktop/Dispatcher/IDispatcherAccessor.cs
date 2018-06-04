namespace Desktop.Dispatcher
{
    public interface IDispatcherAccessor
    {
        System.Windows.Threading.Dispatcher GetDispatcher();
    }
}