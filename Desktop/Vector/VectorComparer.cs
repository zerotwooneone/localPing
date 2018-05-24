using System;
using System.Linq;

namespace Desktop.Vector
{
    public class VectorComparer : IVectorComparer
    {
        public double Compare(IVector v1, IVector v2)
        {
            var v1Values = v1.DimensionValues.ToDictionary(dv => dv.DimensionKey);
            double dotProduct = 0;
            double squaredV1Sum=0;
            double squaredV2Sum=0;
            foreach (var v2Value in v2.DimensionValues)
            {
                var v1Value = v1Values[v2Value.DimensionKey];
                var product = v1Value.Value * v2Value.Value;

                dotProduct += product;
                
                squaredV1Sum += Math.Pow(v1Value.Value,2);
                squaredV2Sum += Math.Pow(v2Value.Value, 2);
            }

            var v1Length = Math.Sqrt(squaredV1Sum);
            var v2Length = Math.Sqrt(squaredV2Sum);
            var lengthProduct = v1Length * v2Length;

            var frac = dotProduct / lengthProduct;

            const double fracTollerance = 0.00001;
            if (Math.Abs(frac - 1) < fracTollerance)
            {
                frac = 1;
            }

            var arcCos = Math.Acos(frac);

            return arcCos;
        }
    }
}