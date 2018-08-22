using System;
using System.Collections.Generic;
using System.Linq;

namespace Desktop.Vector
{
    public class VectorComparer : IVectorComparer
    {
        public double Compare(IVector v1, IVector v2)
        {
            //v1 = NormalizeOrDefault(v1);
            //v2 = NormalizeOrDefault(v2);
            Dictionary<IDimensionKey, IDimensionValue> v1Values = v1.DimensionValues.ToDictionary(dv => dv.DimensionKey);
            double dotProduct = 0;
            double squaredV1Sum = 0;
            double squaredV2Sum = 0;
            Dictionary<IDimensionKey, IDimensionValue> v1DimensionKeys = v1.DimensionValues.ToDictionary(dv => dv.DimensionKey);
            foreach (IDimensionValue v2DimensionValue in v2.DimensionValues)
            {
                IDimensionValue vDim1Value = v1Values.TryGetValue(v2DimensionValue.DimensionKey, out IDimensionValue v1v) ? v1v : null;
                v1DimensionKeys.Remove(v2DimensionValue.DimensionKey);
                v1Values.Remove(v2DimensionValue.DimensionKey);
                double v1Value = vDim1Value?.Value ?? 0.0;
                double v2Value = v2DimensionValue.Value;
                AddDimension(v1Value, v2Value, ref dotProduct, ref squaredV1Sum, ref squaredV2Sum);
            }

            foreach (double v1Value in v1DimensionKeys.Values.Select(dv => dv.Value))
            {
                const double v2Value = 0.0;
                AddDimension(v1Value, v2Value, ref dotProduct, ref squaredV1Sum, ref squaredV2Sum);
            }

            if (dotProduct == 0)
            {
                return 0;
            }

            double v1Length = Math.Sqrt(squaredV1Sum);
            double v2Length = Math.Sqrt(squaredV2Sum);

            double lengthProduct;

            lengthProduct = v1Length * v2Length;
            if (double.IsInfinity(lengthProduct))
            {
                throw new OverflowException();
            }

            if (lengthProduct == 0)
            {
                return 0;
            }

            double frac = dotProduct / lengthProduct;

            const double fracTollerance = 0.000000000000001;
            if (Math.Abs(frac - 1) < fracTollerance)
            {
                return 0;
            }

            double arcCos = Math.Acos(frac);

            return arcCos;
        }

        public void AddDimension(double v1, double v2, ref double dotProduct, ref double squaredV1Sum, ref double squaredV2Sum)
        {
            double product = v1 * v2;

            dotProduct += product;

            squaredV1Sum += Math.Pow(v1, 2);
            if (double.IsInfinity(squaredV1Sum))
            {
                throw new OverflowException();
            }
            squaredV2Sum += Math.Pow(v2, 2);
            if (double.IsInfinity(squaredV2Sum))
            {
                throw new OverflowException();
            }
        }

        public static IVector NormalizeOrDefault(IVector v1)
        {
            double magnitude = GetMagnitude(v1);
            /* Check that we are not attempting to normalize a vector of magnitude 1;
               if we are then return v(0,0,0) */
            if (magnitude == 0)
            {
                return GetOrigin(v1);
            }

            /* Check that we are not attempting to normalize a vector with NaN components;
               if we are then return v(NaN,NaN,NaN) */
            //if (v1.IsNaN())
            //{
            //    return NaN;
            //}

            // Special Cases
            if (double.IsInfinity(magnitude))
            {
                //double dimVal;

                //var x =
                //    v1.X == 0 ? 0 :
                //        v1.X == -0 ? -0 :
                //            double.IsPositiveInfinity(v1.X) ? 1 :
                //                double.IsNegativeInfinity(v1.X) ? -1 :
                //                    double.NaN;
                //var y =
                //    v1.Y == 0 ? 0 :
                //        v1.Y == -0 ? -0 :
                //            double.IsPositiveInfinity(v1.Y) ? 1 :
                //                double.IsNegativeInfinity(v1.Y) ? -1 :
                //                    double.NaN;

                //var z =
                //    v1.Z == 0 ? 0 :
                //        v1.Z == -0 ? -0 :
                //            double.IsPositiveInfinity(v1.Z) ? 1 :
                //                double.IsNegativeInfinity(v1.Z) ? -1 :
                //                    double.NaN;

                //var result = new Vector3(x, y, z);

                //// If this was a special case return the special case result otherwise return NaN
                //return result.IsNaN() ? NaN : result;
                throw new OverflowException();
            }

            // Run the normalization as usual
            return NormalizeOrNaN(v1, magnitude);
        }

        private static IVector NormalizeOrNaN(IVector v1, double magnitude)
        {
            // find the inverse of the vectors magnitude
            double inverse = 1 / magnitude;
            IEnumerable<IDimensionValue> dimVals = GetDimensionValues(v1, d => d * inverse); // multiply each component by the inverse of the magnitude
            return new Vector(dimVals);
        }

        private static IVector GetOrigin(IVector v1)
        {
            IEnumerable<IDimensionValue> dimVals = GetDimensionValues(v1, d => 0);
            return new Vector(dimVals);
        }

        public static IEnumerable<IDimensionValue> GetDimensionValues(IVector v1, Func<double, double> convert = null)
        {
            return v1.DimensionValues.Select(dv =>
            {
                double value = convert == null ? dv.Value : convert(dv.Value);
                DimensionValue result = new DimensionValue(dv.DimensionKey, value);
                return result;
            });
        }

        public static double GetMagnitude(IVector v1)
        {
            IEnumerable<double> valuesSquared = v1.DimensionValues.Select(dv => dv.Value * dv.Value);
            double sumOfSquares = valuesSquared.Sum();
            return Math.Sqrt(sumOfSquares);
        }
    }
}