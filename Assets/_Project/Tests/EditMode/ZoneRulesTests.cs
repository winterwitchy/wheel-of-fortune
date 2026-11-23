using System;
using System.Collections.Generic;
using NUnit.Framework;
using WheelGame.Core;

namespace WheelGame.Tests.EditMode
{
    public sealed class ZoneRulesTests
    {
        [TestCase(1, "normal")]
        [TestCase(5, "safe")]
        [TestCase(30, "super")]
        [TestCase(35, "safe")]
        [TestCase(60, "super")]
        public void GetZoneKind_PicksTheHighestPriorityMatch(int zone, string expectedId)
        {
            Assert.AreEqual(expectedId, TestZones.CreateRules().GetZoneKind(zone).Id);
        }

        [TestCase(1, 5)]
        [TestCase(5, 10)]
        [TestCase(25, 35)]
        [TestCase(30, 35)]
        public void GetNextZoneOfKind_Safe_SkipsSuperZones(int zone, int expected)
        {
            Assert.AreEqual(expected, TestZones.CreateRules().GetNextZoneOfKind(zone, "safe"));
        }

        [TestCase(1, 30)]
        [TestCase(30, 60)]
        public void GetNextZoneOfKind_Super_ReturnsTheFollowingSuperZone(int zone, int expected)
        {
            Assert.AreEqual(expected, TestZones.CreateRules().GetNextZoneOfKind(zone, "super"));
        }

        [Test]
        public void NewZoneKind_IsPickedUpWithoutCodeChanges()
        {
            var bonus = new ZoneKind("bonus", 7, 15, false, 3, null, null);
            var rules = new ZoneRules(new List<ZoneKind> { TestZones.NORMAL, TestZones.SAFE, TestZones.SUPER, bonus });

            Assert.AreEqual("bonus", rules.GetZoneKind(7).Id);
            Assert.AreEqual("bonus", rules.GetZoneKind(35).Id);
            Assert.AreEqual("super", rules.GetZoneKind(210).Id);
            Assert.AreEqual(14, rules.GetNextZoneOfKind(7, "bonus"));
        }

        [Test]
        public void Constructor_WithoutADefaultKind_Throws()
        {
            Assert.Throws<ArgumentException>(() => new ZoneRules(new List<ZoneKind> { TestZones.SAFE, TestZones.SUPER }));
        }

        [Test]
        public void ZoneKind_NonPositiveInterval_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new ZoneKind("broken", 0, 0, true, 1, null, null));
        }
    }
}
