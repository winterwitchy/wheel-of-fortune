using System;
using NUnit.Framework;
using WheelGame.Core;

namespace WheelGame.Tests.EditMode
{
    public sealed class ZoneRulesTests
    {
        [TestCase(1, ZoneType.Normal)]
        [TestCase(5, ZoneType.Safe)]
        [TestCase(30, ZoneType.Super)]
        [TestCase(35, ZoneType.Safe)]
        [TestCase(60, ZoneType.Super)]
        public void GetZoneType_ReturnsExpectedType(int zone, ZoneType expected)
        {
            var rules = new ZoneRules(5, 30);
            Assert.AreEqual(expected, rules.GetZoneType(zone));
        }

        [Test]
        public void Constructor_NonPositiveInterval_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new ZoneRules(0, 30));
            Assert.Throws<ArgumentOutOfRangeException>(() => new ZoneRules(5, 0));
        }

        [TestCase(1, 5)]
        [TestCase(5, 10)]
        [TestCase(17, 20)]
        [TestCase(29, 30)]
        [TestCase(30, 35)]
        public void GetNextRiskFreeZone_ReturnsFollowingSafeOrSuperZone(int zone, int expected)
        {
            var rules = new ZoneRules(5, 30);
            Assert.AreEqual(expected, rules.GetNextRiskFreeZone(zone));
        }
    }
}