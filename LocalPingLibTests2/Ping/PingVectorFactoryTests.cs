using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using zh.LocalPingLib.Ping;
using zh.Vector;

namespace LocalPingLibTests2.Ping
{
    [TestClass]
    public class PingVectorFactoryTests
    {
        private MockRepository mockRepository;

        private static readonly IDimensionKeyFactory DimensionKeyFactory = new DimensionKeyFactory();
        private Mock<IPingResponseUtil> mockPingResponseUtil;
        private Mock<PingStatsUtil> mockPingStatsUtil;
        private static readonly IVectorComparer VectorComparer = new VectorComparer();
        private const double EighthPi = Math.PI / 8;
        private const double HundredthPi = Math.PI / 100;
        private const double HalfPi = Math.PI / 2;

        private static readonly IVector Boring = new Vector(new[]
        {
            new DimensionValue(DimensionKeyFactory.GetOrCreate(DimensionNames.Is100PercentSuccessOrFailure), PingVectorFactory.Hash(true)),
            new DimensionValue(DimensionKeyFactory.GetOrCreate(DimensionNames.Has1SuccessOrFailure), PingVectorFactory.Hash(false)),
            new DimensionValue(DimensionKeyFactory.GetOrCreate(DimensionNames.FiveMinuteDeltaSinceLastSuccessOrFailure), PingVectorFactory.Hash(1000)),
            new DimensionValue(DimensionKeyFactory.GetOrCreate(DimensionNames.OneMinuteDeltaSinceLastSuccessOrFailure), PingVectorFactory.Hash(1000)),
        });

        private static readonly IVector Interesting = new Vector(new[]
        {
            new DimensionValue(DimensionKeyFactory.GetOrCreate(DimensionNames.Is100PercentSuccessOrFailure), PingVectorFactory.Hash(false)),
            new DimensionValue(DimensionKeyFactory.GetOrCreate(DimensionNames.Has1SuccessOrFailure), PingVectorFactory.Hash(true)),
            new DimensionValue(DimensionKeyFactory.GetOrCreate(DimensionNames.FiveMinuteDeltaSinceLastSuccessOrFailure), PingVectorFactory.Hash(0.0)),
            new DimensionValue(DimensionKeyFactory.GetOrCreate(DimensionNames.OneMinuteDeltaSinceLastSuccessOrFailure), PingVectorFactory.Hash(0.0)),
        });

        [TestInitialize]
        public void TestInitialize()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);

            this.mockPingResponseUtil = this.mockRepository.Create<IPingResponseUtil>();
            this.mockPingStatsUtil = this.mockRepository.Create<PingStatsUtil>();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            this.mockRepository.VerifyAll();
        }

        private PingVectorFactory CreateFactory()
        {
            return new PingVectorFactory(
                DimensionKeyFactory,
                this.mockPingResponseUtil.Object,
                this.mockPingStatsUtil.Object);
        }

        [TestMethod]
        public void GetVector_100PercentFailure_WillMatchBoring()
        {
            // Arrange
            PingVectorFactory unitUnderTest = CreateFactory();
            IPingResponse pingResponse = new PingResponse(IPAddress.Loopback, TimeSpan.Zero, IPStatus.Success, IPAddress.Loopback);
            IPingStats stats = new PingStats(null, DateTime.Now) { Average25 = 0, Average25Count = 0, StatusHistory = Enumerable.Range(1, PingStatsUtil.MaxHistoryCount).Select(i => false).ToList() };

            // Act
            IVector actual = unitUnderTest.GetVector(
                pingResponse,
                stats);

            // Assert
            IVector expected = Boring;
            VectorComparer.AssertAreEqual(expected, actual, HundredthPi);
        }

        [TestMethod]
        public void GetVector_100PercentSuccess_WillMatchBoring()
        {
            // Arrange
            PingVectorFactory unitUnderTest = CreateFactory();
            IPingResponse pingResponse = new PingResponse(IPAddress.Loopback, TimeSpan.Zero, IPStatus.Success, IPAddress.Loopback);
            IPingStats stats = new PingStats(DateTime.Now, null) { Average25 = 0, Average25Count = 0, StatusHistory = Enumerable.Range(1, PingStatsUtil.MaxHistoryCount).Select(i => true).ToList() };

            // Act
            IVector actual = unitUnderTest.GetVector(
                pingResponse,
                stats);

            // Assert
            IVector expected = Boring;
            VectorComparer.AssertAreEqual(expected, actual, HundredthPi);
        }

        [TestMethod]
        public void GetVector_1Failure_WillNotMatchBoring()
        {
            // Arrange
            PingVectorFactory unitUnderTest = CreateFactory();
            IPingResponse pingResponse = new PingResponse(IPAddress.Loopback, TimeSpan.Zero, IPStatus.Success, IPAddress.Loopback);
            IPingStats stats = new PingStats(null, DateTime.Now) { Average25 = 0, Average25Count = 0, StatusHistory = Enumerable.Range(1, PingStatsUtil.MaxHistoryCount).Select(i => true).Append(false).ToList() };

            // Act
            IVector actual = unitUnderTest.GetVector(
                pingResponse,
                stats);

            // Assert
            IVector expected = Boring;
            VectorComparer.AssertAreNotEqual(expected, actual, HalfPi);
        }

        [TestMethod]
        public void GetVector_1Success_WillNotMatchBoring()
        {
            // Arrange
            PingVectorFactory unitUnderTest = CreateFactory();
            IPingResponse pingResponse = new PingResponse(IPAddress.Loopback, TimeSpan.Zero, IPStatus.Success, IPAddress.Loopback);
            IPingStats stats = new PingStats(DateTime.Now, null) { Average25 = 0, Average25Count = 0, StatusHistory = Enumerable.Range(1, PingStatsUtil.MaxHistoryCount).Select(i => false).Append(true).ToList() };

            // Act
            IVector actual = unitUnderTest.GetVector(
                pingResponse,
                stats);

            // Assert
            IVector expected = Boring;
            VectorComparer.AssertAreNotEqual(expected, actual, HalfPi);
        }



        [TestMethod]
        public void GetVector_1Failure_CloseToInteresting()
        {
            // Arrange
            PingVectorFactory unitUnderTest = CreateFactory();
            IPingResponse pingResponse = new PingResponse(IPAddress.Loopback, TimeSpan.Zero, IPStatus.Success, IPAddress.Loopback);
            IPingStats stats = new PingStats(null, DateTime.Now) { Average25 = 0, Average25Count = 0, StatusHistory = Enumerable.Range(1, PingStatsUtil.MaxHistoryCount).Select(i => true).Append(false).ToList() };

            // Act
            IVector actual = unitUnderTest.GetVector(
                pingResponse,
                stats);

            // Assert
            IVector expected = Interesting;
            VectorComparer.AssertAreEqual(expected, actual, EighthPi);
        }

        [TestMethod]
        public void GetVector_1Success_CloseToInteresting()
        {
            // Arrange
            PingVectorFactory unitUnderTest = CreateFactory();
            IPingResponse pingResponse = new PingResponse(IPAddress.Loopback, TimeSpan.Zero, IPStatus.Success, IPAddress.Loopback);
            IPingStats stats = new PingStats(DateTime.Now, null) { Average25 = 0, Average25Count = 0, StatusHistory = Enumerable.Range(1, PingStatsUtil.MaxHistoryCount).Select(i => false).Append(true).ToList() };

            // Act
            IVector actual = unitUnderTest.GetVector(
                pingResponse,
                stats);

            // Assert
            IVector expected = Interesting;
            VectorComparer.AssertAreEqual(expected, actual, EighthPi);
        }
    }
}
