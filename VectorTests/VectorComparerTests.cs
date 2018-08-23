using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using zh.Vector;

namespace VectorTests
{
    [TestClass]
    public class VectorComparerTests
    {
        private MockRepository mockRepository;



        [TestInitialize]
        public void TestInitialize()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);


        }

        [TestCleanup]
        public void TestCleanup()
        {
            this.mockRepository.VerifyAll();
        }

        private VectorComparer CreateVectorComparer()
        {
            return new VectorComparer();
        }

        [TestMethod]
        public void Compare_Perpendicular_ShouldBeHalfPi()
        {
            // Arrange
            var unitUnderTest = CreateVectorComparer();
            IDimensionKey xDimension = new DimensionKey("xDimension");
            IDimensionKey yDimension = new DimensionKey("yDimension");
            IVector v1 = new Vector(new []
            {
                new DimensionValue(xDimension, 0),
                new DimensionValue(yDimension, 1),
            });
            IVector v2 = new Vector(new []
            {
                new DimensionValue(xDimension, 1),
                new DimensionValue(yDimension, 0),
            });

            // Act
            var actual = unitUnderTest.Compare(
                v1,
                v2);

            // Assert
            const double expected = Math.PI/2;
            Assert.AreEqual(expected, actual,0.001);
        }


        [TestMethod]
        public void Compare_Same_ShouldBeZero()
        {
            // Arrange
            var unitUnderTest = CreateVectorComparer();
            IDimensionKey xDimension = new DimensionKey("xDimension");
            IDimensionKey yDimension = new DimensionKey("yDimension");
            IVector v1 = new Vector(new []
            {
                new DimensionValue(xDimension, 0),
                new DimensionValue(yDimension, 1),
            });
            IVector v2 = new Vector(new []
            {
                new DimensionValue(xDimension, 0),
                new DimensionValue(yDimension, 1),
            });

            // Act
            var actual = unitUnderTest.Compare(
                v1,
                v2);

            // Assert
            const double expected = 0.0;
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Compare_Inverted_ShouldBePi()
        {
            // Arrange
            var unitUnderTest = CreateVectorComparer();
            IDimensionKey xDimension = new DimensionKey("xDimension");
            IDimensionKey yDimension = new DimensionKey("yDimension");
            IVector v1 = new Vector(new []
            {
                new DimensionValue(xDimension, 0),
                new DimensionValue(yDimension, 1),
            });
            IVector v2 = new Vector(new []
            {
                new DimensionValue(xDimension, 0),
                new DimensionValue(yDimension, -1),
            });

            // Act
            var actual = unitUnderTest.Compare(
                v1,
                v2);

            // Assert
            const double expected = Math.PI;
            Assert.AreEqual(expected, actual);
        }

        //[TestMethod]
        //public void AddDimension_StateUnderTest_ExpectedBehavior()
        //{
        //    // Arrange
        //    var unitUnderTest = CreateVectorComparer();
        //    double v1 = TODO;
        //    double v2 = TODO;
        //    double dotProduct = TODO;
        //    double squaredV1Sum = TODO;
        //    double squaredV2Sum = TODO;

        //    // Act
        //    unitUnderTest.AddDimension(
        //        v1,
        //        v2,
        //        ref dotProduct,
        //        ref squaredV1Sum,
        //        ref squaredV2Sum);

        //    // Assert
        //    Assert.Fail();
        //}

        //[TestMethod]
        //public void NormalizeOrDefault_StateUnderTest_ExpectedBehavior()
        //{
        //    // Arrange
        //    var unitUnderTest = CreateVectorComparer();
        //    IVector v1 = TODO;

        //    // Act
        //    var result = unitUnderTest.NormalizeOrDefault(
        //        v1);

        //    // Assert
        //    Assert.Fail();
        //}

        //[TestMethod]
        //public void GetDimensionValues_StateUnderTest_ExpectedBehavior()
        //{
        //    // Arrange
        //    var unitUnderTest = CreateVectorComparer();
        //    IVector v1 = TODO;
        //    Func<double, double> convert = TODO;

        //    // Act
        //    var result = unitUnderTest.GetDimensionValues(
        //        v1,
        //        convert);

        //    // Assert
        //    Assert.Fail();
        //}

        //[TestMethod]
        //public void GetMagnitude_StateUnderTest_ExpectedBehavior()
        //{
        //    // Arrange
        //    var unitUnderTest = CreateVectorComparer();
        //    IVector v1 = TODO;

        //    // Act
        //    var result = unitUnderTest.GetMagnitude(
        //        v1);

        //    // Assert
        //    Assert.Fail();
        //}
    }
}
