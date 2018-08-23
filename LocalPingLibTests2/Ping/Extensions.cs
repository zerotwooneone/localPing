using Microsoft.VisualStudio.TestTools.UnitTesting;
using zh.Vector;

namespace LocalPingLibTests2.Ping
{
    public static class Extensions
    {
        public static void AssertAreEqual(this IVectorComparer vectorComparer, IVector expected, IVector actual, double delta = 0.0)
        {
            double result = vectorComparer.Compare(expected, actual);
            Assert.AreEqual(result, 0, delta);
        }

        public static void AssertAreNotEqual(this IVectorComparer vectorComparer, IVector expected, IVector actual, double delta = 0.0)
        {
            double result = vectorComparer.Compare(expected, actual);
            Assert.AreNotEqual(result, 0, delta);
        }
    }
}