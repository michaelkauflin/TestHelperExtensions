﻿using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelperExtensions;

namespace TestHelperExtensions.Test
{
    [TestClass]
    public class LongExtensions_GetRandom_Should
    {
        public TestContext TestContext { get; set; }
        private Random _random = new Random();

        [TestCleanup]
        public void TestCleanup()
        {
            TestHelperExtensions.LongExtensions._rnd = new Random();
        }

        #region Interaction Tests

        // These tests require some knowlege of the underlying
        // implementation.  As a result, they can be more brittle
        // then the rules test (needing to be changed if the
        // implementation details change).

        [TestMethod]
        public void CallTheRandomNumberGenerator()
        {
            var radius = _random.Next(3, 33000);
            long upperBound = Convert.ToInt32(Int16.MaxValue + radius);
            long lowerBound = Convert.ToInt32(Int16.MaxValue - radius);
            var randomGeneratorCalled = false;

            var r = new System.Fakes.StubRandom();
            r.NextDouble01 = () =>
            {
                randomGeneratorCalled = true;
                return 0.0;
            };

            TestHelperExtensions.LongExtensions._rnd = r;

            var actual = upperBound.GetRandom(lowerBound);
            Assert.IsTrue(randomGeneratorCalled);
        }

        #endregion

        #region Rules Tests

        // Rules tests are the preferred types of unit tests since they
        // test those things that the customers care about. However, they
        // can sometimes be incomplete, or extremely difficult to make 
        // comprehensive.  In this example, we can easily check the
        // boundary rules, but would have a very difficult time proving
        // that we actually called the random number generator properly.
        // For example, if we were off by one in our calls to the generator
        // such that we never reached our bounds, but were always
        // at least 1 away, these tests might not identify that situation.

        [TestMethod]
        public void AlwaysBeAboveOrEqualToTheLowerBound()
        {
            const int executionCount = 10000;

            long upperBound = Convert.ToInt64(Int32.MaxValue) + Convert.ToInt64(_random.Next(Int32.MaxValue));
            long lowerBound = upperBound - Convert.ToInt64(_random.Next(Int32.MaxValue));
            TestContext.WriteLine("LowerBound={0} UpperBound={1}", lowerBound, upperBound);

            for (int i = 0; i < executionCount; i++)
            {
                var actual = upperBound.GetRandom(lowerBound);
                TestContext.WriteLine("Actual={0}", actual);
                Assert.IsTrue(actual >= lowerBound);
            }
        }

        [TestMethod]
        public void NotReachTheUpperBound()
        {
            const int executionCount = 10000;

            long upperBound = Convert.ToInt64(Int32.MaxValue) + Convert.ToInt64(_random.Next(Int32.MaxValue));
            long lowerBound = upperBound - Convert.ToInt64(_random.Next(Int32.MaxValue));
            TestContext.WriteLine("LowerBound={0} UpperBound={1}", lowerBound, upperBound);

            for (int i = 0; i < executionCount; i++)
            {
                var actual = upperBound.GetRandom(lowerBound);
                TestContext.WriteLine("Actual={0}", actual);
                Assert.IsTrue(actual < upperBound);
            }
        }

        [TestMethod]
        public void AlwaysBeAboveOrEqualToZeroIfNoLowerBoundSpecified()
        {
            const int executionCount = 10000;

            long upperBound = Convert.ToInt64(Int32.MaxValue) + Convert.ToInt64(_random.Next(Int32.MaxValue));
            TestContext.WriteLine("UpperBound={0}", upperBound);

            for (int i = 0; i < executionCount; i++)
            {
                var actual = upperBound.GetRandom();
                TestContext.WriteLine("Actual={0}", actual);
                Assert.IsTrue(actual >= 0);
            }
        }

        [TestMethod, ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void ThrowExceptionIfLowerBoundIsNotBelowTheUpperBound()
        {
            long upperBound = Int32.MaxValue + _random.Next(100, 1000);
            long lowerBound = upperBound + _random.Next(100);
            var result = upperBound.GetRandom(lowerBound);
        }

        #endregion

        #region Sanity Tests

        // Not for testing randomness, just to make sure things look reasonable. 
        // I feel no need to test the implementation of Microsoft's random number generator.

        [TestMethod]
        public void HaveAnAverageResultNearTheMiddleOfTheRange()
        {
            const int valueRadius = 2000;
            const double tolerance = .02;

            long upperBound = Convert.ToInt64(Int32.MaxValue) + Convert.ToInt64(_random.Next(valueRadius));
            long lowerBound = Convert.ToInt64(Int32.MaxValue) - Convert.ToInt64(_random.Next(valueRadius));

            var expectedMean = Convert.ToInt64(((upperBound - 1) - lowerBound) / 2) + lowerBound;
            var slop = Convert.ToInt64(expectedMean * tolerance);
            var minMean = expectedMean - slop;
            var maxMean = expectedMean + slop;

            var result = 100000.GetRandomLongValues(upperBound, lowerBound); 
            var actualMean = result.Average();

            TestContext.WriteLine("mean:{0} min allowed:{1} max allowed:{2} lower bound:{3} upper bound:{4}", actualMean, minMean, maxMean, lowerBound, upperBound);
            Assert.IsTrue(actualMean > minMean);
            Assert.IsTrue(actualMean < maxMean);
        }

        [TestMethod]
        public void HaveAMedianResultNearTheMiddleOfTheRange()
        {
            const int valueRadius = 2000;
            const double tolerance = .02;

            long upperBound = Convert.ToInt64(Int32.MaxValue) + Convert.ToInt64(_random.Next(valueRadius));
            long lowerBound = Convert.ToInt64(Int32.MaxValue) - Convert.ToInt64(_random.Next(valueRadius));

            var expectedMedian = Convert.ToInt64(((upperBound - 1) - lowerBound) / 2) + lowerBound;
            var slop = Convert.ToInt64(expectedMedian * tolerance);
            var minMedian = expectedMedian - slop;
            var maxMedian = expectedMedian + slop;

            var result = 100000.GetRandomLongValues(upperBound, lowerBound);
            var actualMedian = result.Median();

            TestContext.WriteLine("median:{0} min allowed:{1} max allowed:{2}", actualMedian, minMedian, maxMedian);
            Assert.IsTrue(actualMedian > minMedian);
            Assert.IsTrue(actualMedian < maxMedian);
        }

        [TestMethod]
        public void GetResultsAcrossTheEntireRangeOfTheRequest()
        {
            const int valueRadius = 2000;
            const double tolerance = .02;

            long upperBound = Convert.ToInt64(Int32.MaxValue) + Convert.ToInt64(_random.Next(valueRadius));
            long lowerBound = Convert.ToInt64(Int32.MaxValue) - Convert.ToInt64(_random.Next(valueRadius));

            double expectedRange = upperBound - lowerBound;
            var slop = Convert.ToByte(expectedRange * tolerance);
            var minRange = expectedRange - slop;
            var maxRange = expectedRange + slop;


            var result = 100000.GetRandomLongValues(upperBound, lowerBound);
            var actualRange = result.Range();

            TestContext.WriteLine("range:{0} min allowed:{1} max allowed:{2}", actualRange, minRange, maxRange);
            Assert.IsTrue(actualRange > minRange);
            Assert.IsTrue(actualRange < maxRange);

        }

        #endregion

    }
}
