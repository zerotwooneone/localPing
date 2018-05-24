using Desktop.Vector;

namespace Desktop.Ping
{
    public interface IDimensionKeyFactory
    {
        IDimensionKey GetOrCreate(string name);
    }
}