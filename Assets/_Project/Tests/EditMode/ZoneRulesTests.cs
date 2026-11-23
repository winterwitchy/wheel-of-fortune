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
        [TestCase(25, 35)]
        [TestCase(29, 35)]
        [TestCase(30, 35)]
        public void GetNextZoneOfType_Safe_SkipsSuperZones(int zone, int expected)
        {
            var rules = new ZoneRules(5, 30);
            Assert.AreEqual(expected, rules.GetNextZoneOfType(zone, ZoneType.Safe));
        }

        [TestCase(1, 30)]
        [TestCase(29, 30)]
        [TestCase(30, 60)]
        public void GetNextZoneOfType_Super_ReturnsTheFollowingSuperZone(int zone, int expected)
        {
            var rules = new ZoneRules(5, 30);
            Assert.AreEqual(expected, rules.GetNextZoneOfType(zone, ZoneType.Super));
        }
    }
}