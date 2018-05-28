namespace Desktop.Vector
{
    public class DimensionKey : IDimensionKey
    {
        public DimensionKey(string dimensionName)
        {
            DimensionName = dimensionName;
        }

        public string DimensionName { get; }
        public override string ToString()
        {
            return DimensionName;
        }
    }
}