namespace zh.Vector
{
    public interface IDimensionValue
    {
        IDimensionKey DimensionKey { get; }
        double Value { get; }
    }
}