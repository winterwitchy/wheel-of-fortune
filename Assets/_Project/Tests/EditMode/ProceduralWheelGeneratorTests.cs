using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using WheelGame.Core;
using WheelGame.Data;

namespace WheelGame.Tests.EditMode
{
    public sealed class ProceduralWheelGeneratorTests
    {
        private RewardItemData _cash;
        private RewardItemData _gold;
        private List<RewardItemData> _earlyPoints;
        private List<RewardItemData> _latePoints;
        private List<RewardItemData> _earlyConsumables;
        private List<RewardItemData> _lateConsumables;
        private List<RewardItemData> _skins;
        private List<RewardItemData> _superSkins;
        private RewardItemData _superChest;
        private List<RewardItemData> _chests;
        private List<ZoneBand> _bands;
        private WheelGenerationSettings _settings;

        [SetUp]
        public void SetUp()
        {
            _cash = CreateReward();
            _gold = CreateReward();
            _earlyPoints = CreateRewards(2);
            _latePoints = CreateRewards(1);
            _earlyConsumables = CreateRewards(1);
            _lateConsumables = CreateRewards(2);
            _skins = CreateRewards(3);
            _superSkins = CreateRewards(2);
            _superChest = CreateReward();
            _chests = CreateRewards(7);

            _bands = new List<ZoneBand>
            {
                new ZoneBand(1, new SliceComposition(2, 2, 1, 1, 1, 0), _earlyPoints, _earlyConsumables, new List<RewardItemData>()),
                new ZoneBand(11, new SliceComposition(1, 1, 1, 2, 2, 0), _latePoints, _lateConsumables, new List<RewardItemData>()),
                new ZoneBand(31, new SliceComposition(1, 1, 0, 1, 1, 3), _latePoints, _lateConsumables, _skins)
            };

            _settings = new WheelGenerationSettings(
                _cash, new AmountFormula(GrowthCurve.Quadratic, 10, 1, 25),
                _gold, new AmountFormula(GrowthCurve.Linear, 1, 0, 1),
                new AmountFormula(GrowthCurve.Linear, 2, 1, 5),
                new AmountFormula(GrowthCurve.SquareRoot, 1, 0, 2),
                _bands,
                _skins, 2,
                new SliceComposition(1, 1, 1, 1, 2, 2), _superSkins, _superChest, 5,
                new ChestProgression(_chests, 3, 10));
        }

        [Test]
        public void Generate_FirstBand_FavorsCashAndGoldAndDoublesTheSecondCopy()
        {
            var slices = CreateGenerator().Generate(1, ZoneType.Normal);

            Assert.AreEqual(WheelLayout.SliceCount, slices.Count);
            Assert.AreEqual(1, slices.Count(slice => slice.IsBomb));
            CollectionAssertEquivalent(new[] { 25, 50 }, AmountsOf(slices, _cash));
            CollectionAssertEquivalent(new[] { 1, 2 }, AmountsOf(slices, _gold));
            Assert.AreEqual(1, slices.Count(slice => _earlyPoints.Contains(slice.Reward)));
            Assert.AreEqual(1, slices.Count(slice => _chests.Contains(slice.Reward)));
        }

        [Test]
        public void Generate_SecondBand_UsesItsOwnPoolsWithDistinctConsumables()
        {
            var slices = CreateGenerator().Generate(12, ZoneType.Normal);

            Assert.AreEqual(1, slices.Count(slice => slice.IsBomb));
            Assert.AreEqual(1, slices.Count(slice => slice.Reward == _cash));
            Assert.AreEqual(1, slices.Count(slice => _latePoints.Contains(slice.Reward)));
            Assert.AreEqual(0, slices.Count(slice => _earlyPoints.Contains(slice.Reward)));

            var consumables = slices.Where(slice => _lateConsumables.Contains(slice.Reward)).ToList();
            Assert.AreEqual(2, consumables.Count);
            Assert.AreNotSame(consumables[0].Reward, consumables[1].Reward);
            Assert.IsTrue(consumables.All(slice => slice.Amount == 6));
        }

        [Test]
        public void Generate_ChestLevelRisesEveryTenZones()
        {
            var early = CreateGenerator().Generate(1, ZoneType.Normal);
            var later = CreateGenerator().Generate(12, ZoneType.Normal);

            Assert.AreSame(_chests[0], early.Single(slice => _chests.Contains(slice.Reward)).Reward);
            Assert.IsTrue(later.Where(slice => _chests.Contains(slice.Reward)).All(slice => slice.Reward == _chests[1]));
        }

        [Test]
        public void Generate_SafeZone_ReplacesBombWithSkinAndDoublesAmounts()
        {
            var slices = CreateGenerator().Generate(5, ZoneType.Safe);

            Assert.AreEqual(0, slices.Count(slice => slice.IsBomb));
            Assert.AreEqual(1, slices.Count(slice => _skins.Contains(slice.Reward)));
            CollectionAssertEquivalent(new[] { 150, 300 }, AmountsOf(slices, _cash));
            CollectionAssertEquivalent(new[] { 10, 20 }, AmountsOf(slices, _gold));
        }

        [Test]
        public void Generate_LateBand_DrawsEverySkinOnce()
        {
            var slices = CreateGenerator().Generate(31, ZoneType.Normal);

            Assert.AreEqual(1, slices.Count(slice => slice.IsBomb));
            Assert.AreEqual(3, slices.Count(slice => _skins.Contains(slice.Reward)));
            Assert.AreEqual(3, slices.Where(slice => _skins.Contains(slice.Reward)).Select(slice => slice.Reward).Distinct().Count());
        }

        [Test]
        public void Generate_LateSafeZoneWithEmptySkinBag_FallsBackToUpgradedChest()
        {
            var slices = CreateGenerator().Generate(35, ZoneType.Safe);

            Assert.AreEqual(0, slices.Count(slice => slice.IsBomb));
            Assert.AreEqual(3, slices.Where(slice => _skins.Contains(slice.Reward)).Select(slice => slice.Reward).Distinct().Count());
            Assert.AreEqual(3, slices.Count(slice => _skins.Contains(slice.Reward)));
            Assert.AreEqual(1, slices.Count(slice => slice.Reward == _chests[6]));
        }

        [Test]
        public void Generate_SuperZone_HasSuperSkinsAndChestsAndMultipliesCountables()
        {
            var slices = CreateGenerator().Generate(30, ZoneType.Super);

            Assert.AreEqual(WheelLayout.SliceCount, slices.Count);
            Assert.AreEqual(0, slices.Count(slice => slice.IsBomb));
            Assert.AreEqual(2, slices.Where(slice => _superSkins.Contains(slice.Reward)).Select(slice => slice.Reward).Distinct().Count());
            Assert.AreEqual(2, slices.Count(slice => slice.Reward == _superChest));
            Assert.AreEqual(2275 * 5, slices.Single(slice => slice.Reward == _cash).Amount);
            Assert.AreEqual(30 * 5, slices.Single(slice => slice.Reward == _gold).Amount);
        }

        [Test]
        public void GetValidationErrors_BandWithWrongSliceCount_IsReported()
        {
            _bands.Add(new ZoneBand(61, new SliceComposition(1, 1, 1, 1, 1, 0), _latePoints, _lateConsumables, new List<RewardItemData>()));

            var errors = _settings.GetValidationErrors();

            Assert.AreEqual(1, errors.Count);
            StringAssertContains("zone 61", errors[0]);
        }

        [Test]
        public void GetValidationErrors_ValidSettings_HasNoErrors()
        {
            Assert.AreEqual(0, _settings.GetValidationErrors().Count);
        }

        private ProceduralWheelGenerator CreateGenerator()
        {
            return new ProceduralWheelGenerator(_settings, new FakeRandomProvider());
        }

        private static List<int> AmountsOf(IReadOnlyList<WheelSliceEntry> slices, RewardItemData reward)
        {
            return slices.Where(slice => slice.Reward == reward).Select(slice => slice.Amount).ToList();
        }

        private static void CollectionAssertEquivalent(IEnumerable<int> expected, IEnumerable<int> actual)
        {
            Assert.AreEqual(string.Join(",", expected.OrderBy(value => value)), string.Join(",", actual.OrderBy(value => value)));
        }

        private static void StringAssertContains(string expected, string actual)
        {
            Assert.IsTrue(actual.Contains(expected), $"'{actual}' does not contain '{expected}'.");
        }

        private static RewardItemData CreateReward()
        {
            return ScriptableObject.CreateInstance<RewardItemData>();
        }

        private static List<RewardItemData> CreateRewards(int count)
        {
            var result = new List<RewardItemData>();

            for (var i = 0; i < count; i++)
            {
                result.Add(CreateReward());
            }

            return result;
        }
    }
}
