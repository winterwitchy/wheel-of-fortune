using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using WheelGame.Core;

namespace WheelGame.Tests.EditMode
{
    public sealed class ProceduralWheelGeneratorTests
    {
        private RewardItem _cash;
        private List<RewardItem> _earlyPoints;
        private List<RewardItem> _skins;
        private List<RewardItem> _superSkins;
        private List<RewardItem> _chests;
        private ZoneKind _normal;
        private ZoneKind _safe;
        private ZoneKind _super;
        private ProceduralWheelGenerator _generator;

        [SetUp]
        public void SetUp()
        {
            _cash = TestRewards.Create("cash", true);
            _earlyPoints = TestRewards.CreateMany("points", 2);
            _skins = TestRewards.CreateMany("skin", 3);
            _superSkins = TestRewards.CreateMany("super_skin", 2);
            _chests = TestRewards.CreateMany("chest", 7);

            var cash = new ItemPoolSource(new List<RewardItem> { _cash }, new AmountFormula(GrowthCurve.Quadratic, 10, 1, 25), false);
            var points = new ItemPoolSource(_earlyPoints, new AmountFormula(GrowthCurve.Linear, 2, 1, 5), false);
            var chests = new ChestProgressionSource(_chests, 3, 10, 0);
            var upgrade = new ChestProgressionSource(_chests, 1, 10, 3);
            var skins = new ItemPoolSource(_skins, null, true);
            skins.SetFallback(upgrade);
            var superSkins = new ItemPoolSource(_superSkins, null, true);

            var bands = new List<ZoneBand>
            {
                new ZoneBand(1, new List<SliceRule> { new SliceRule(cash, 2), new SliceRule(points, 4), new SliceRule(chests, 1) }),
                new ZoneBand(31, new List<SliceRule> { new SliceRule(cash, 1), new SliceRule(points, 2), new SliceRule(chests, 1), new SliceRule(skins, 3) })
            };

            _normal = new ZoneKind("normal", 1, 0, true, 1, null, null);
            _safe = new ZoneKind("safe", 5, 10, false, 2, skins, null);
            _super = new ZoneKind("super", 30, 20, false, 5, null, new List<SliceRule> { new SliceRule(cash, 4), new SliceRule(superSkins, 2), new SliceRule(chests, 2) });
            _generator = new ProceduralWheelGenerator(bands, new FakeRandomProvider());
        }

        [Test]
        public void Generate_NormalZone_HasOneBombAndTheBandComposition()
        {
            var slices = _generator.Generate(1, _normal);

            Assert.AreEqual(WheelLayout.SLICE_COUNT, slices.Count);
            Assert.AreEqual(1, slices.Count(slice => slice.IsBomb));
            Assert.AreEqual("25,50", AmountsOf(slices, _cash));
            Assert.AreEqual(1, slices.Count(slice => _chests.Contains(slice.Reward)));
        }

        [Test]
        public void Generate_SafeZone_FillsTheBombSlotAndMultipliesAmounts()
        {
            var slices = _generator.Generate(5, _safe);

            Assert.AreEqual(0, slices.Count(slice => slice.IsBomb));
            Assert.AreEqual(1, slices.Count(slice => _skins.Contains(slice.Reward)));
            Assert.AreEqual("150,300", AmountsOf(slices, _cash));
        }

        [Test]
        public void Generate_LateSafeZone_FallsBackToAnUpgradedChestWhenSkinsRunOut()
        {
            var slices = _generator.Generate(35, _safe);

            Assert.AreEqual(WheelLayout.SLICE_COUNT, slices.Count);
            Assert.AreEqual(3, slices.Where(slice => _skins.Contains(slice.Reward)).Select(slice => slice.Reward).Distinct().Count());
            Assert.AreEqual(3, slices.Count(slice => _skins.Contains(slice.Reward)));
            Assert.AreEqual(1, slices.Count(slice => slice.Reward == _chests[6]));
        }

        [Test]
        public void Generate_ZoneWithOwnComposition_IgnoresTheBand()
        {
            var slices = _generator.Generate(30, _super);

            Assert.AreEqual(WheelLayout.SLICE_COUNT, slices.Count);
            Assert.AreEqual(0, slices.Count(slice => slice.IsBomb));
            Assert.AreEqual(2, slices.Where(slice => _superSkins.Contains(slice.Reward)).Select(slice => slice.Reward).Distinct().Count());
            Assert.AreEqual(4, slices.Count(slice => slice.Reward == _cash));
            Assert.AreEqual(2275 * 5, slices.Where(slice => slice.Reward == _cash).Min(slice => slice.Amount));
        }

        [Test]
        public void GetBand_PicksTheLastBandThatHasStarted()
        {
            Assert.AreEqual(1, _generator.GetBand(30).FirstZone);
            Assert.AreEqual(31, _generator.GetBand(31).FirstZone);
        }

        private static string AmountsOf(IReadOnlyList<WheelSliceEntry> slices, RewardItem reward)
        {
            return string.Join(",", slices.Where(slice => slice.Reward == reward).Select(slice => slice.Amount).OrderBy(amount => amount));
        }
    }
}
