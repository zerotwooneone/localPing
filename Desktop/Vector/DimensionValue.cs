namespace Desktop.Vector
{
    public class DimensionValue : IDimensionValue
    {
        public DimensionValue(IDimensionKey dimensionKey, double value)
        {
            DimensionKey = dimensionKey;
            Value = value;
        }

        public IDimensionKey DimensionKey { get; }
        public double Value { get; }
    }
}