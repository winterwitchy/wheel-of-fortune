using System;
using NUnit.Framework;
using WheelGame.Core;

namespace WheelGame.Tests.EditMode
{
    public sealed class RevivePricingTests
    {
        [TestCase(0, 25)]
        [TestCase(1, 50)]
        [TestCase(2, 100)]
        [TestCase(3, 200)]
        public void GetCost_DoublesWithEveryRevive(int revivesUsed, int expectedCost)
        {
            var pricing = new RevivePricing(25, 2f);

            Assert.AreEqual(expectedCost, pricing.GetCost(revivesUsed));
        }

        [Test]
        public void GetCost_RoundsFractionalCosts()
        {
            var pricing = new RevivePricing(25, 1.5f);

            Assert.AreEqual(38, pricing.GetCost(1));
        }

        [Test]
        public void GetCost_ClampsInsteadOfOverflowing()
        {
            var pricing = new RevivePricing(25, 2f);

            Assert.AreEqual(int.MaxValue, pricing.GetCost(64));
        }

        [Test]
        public void Constructor_RejectsMultiplierBelowOne()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new RevivePricing(25, 0.5f));
        }
    }
}
